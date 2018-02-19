//#define USE_INTERACTION_GUI

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace DxR
{
    /// <summary>
    /// Whenever a GUI action is performed, button clicked, dropdown clicked, etc., the guiVisSpecs is automatically updated so it 
    /// should be in sync all the time. The visSpecs of the targetVis is only updated when calling UpdateVisSpecsFromGUISpecs, and 
    /// for the other way around, the guiVisSpecs is updated from the targetVis specs when calling UpdateGUISpecsFromVisSpecs.
    /// </summary>
    public class GUI : MonoBehaviour
    {
        Vis targetVis = null;
        JSONNode guiVisSpecs = null;
        Dropdown dataDropdown = null;
        Dropdown markDropdown = null;
        
        Transform addChannelButtonTransform = null;
        GameObject channelGUIPrefab = null;

        Transform addInteractionButtonTransform = null;
        GameObject interactionGUIPrefab = null;

        List<string> dataFieldTypeDropdownOptions;
        
        // Use this for initialization
        void Start()
        {

        }

        public Vis GetTargetVis()
        {
            return targetVis;
        }

        public void Init(Vis targetVisInstance)
        {
            targetVis = targetVisInstance;
            
            dataFieldTypeDropdownOptions = new List<string> { "quantitative", "nominal", "ordinal", "temporal" };

            Transform dataDropdownTransform = gameObject.transform.Find("DataDropdown");
            dataDropdown = dataDropdownTransform.gameObject.GetComponent<Dropdown>();
            dataDropdown.onValueChanged.AddListener(delegate {
                OnDataDropdownValueChanged(dataDropdown);
            });

            Transform marksDropdownTransform = gameObject.transform.Find("MarkDropdown");
            markDropdown = marksDropdownTransform.gameObject.GetComponent<Dropdown>();
            markDropdown.onValueChanged.AddListener(delegate {
                OnMarkDropdownValueChanged(markDropdown);
            });

            Button btn = gameObject.transform.Find("UpdateButton").GetComponent<Button>();
            btn.onClick.AddListener(CallUpdateVisSpecsFromGUISpecs);

            channelGUIPrefab = Resources.Load("GUI/ChannelGUI") as GameObject;

            addChannelButtonTransform = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent/AddChannelButton");
            Button addChannelBtn = addChannelButtonTransform.GetComponent<Button>();
            addChannelBtn.onClick.AddListener(AddEmptyChannelGUICallback);

#if USE_INTERACTION_GUI
            
            interactionGUIPrefab = Resources.Load("GUI/InteractionGUI") as GameObject;

            addInteractionButtonTransform = gameObject.transform.Find("InteractionList/Viewport/InteractionListContent/AddInteractionButton");
            Button addInteractionBtn = addInteractionButtonTransform.GetComponent<Button>();
            addInteractionBtn.onClick.AddListener(AddEmptyInteractionGUICallback);
#endif
            InitInteractiveButtons();
            UpdateGUISpecsFromVisSpecs();
        }

        private void InitInteractiveButtons()
        {
            Button resetBtn = gameObject.transform.Find("ResetButton").GetComponent<Button>();
            resetBtn.onClick.AddListener(ResetCallback);

            Button zoomInBtn = gameObject.transform.Find("ZoomInButton").GetComponent<Button>();
            zoomInBtn.onClick.AddListener(ZoomInCallback);

            Button zoomOutBtn = gameObject.transform.Find("ZoomOutButton").GetComponent<Button>();
            zoomOutBtn.onClick.AddListener(ZoomOutCallback);

            Button rotateXBtn = gameObject.transform.Find("RotateXButton").GetComponent<Button>();
            rotateXBtn.onClick.AddListener(RotateXCallback);

            Button rotateYBtn = gameObject.transform.Find("RotateYButton").GetComponent<Button>();
            rotateYBtn.onClick.AddListener(RotateYCallback);

            Button rotateZBtn = gameObject.transform.Find("RotateZButton").GetComponent<Button>();
            rotateZBtn.onClick.AddListener(RotateZCallback);
        }

        public void RotateXCallback()
        {
            if (targetVis != null)
            {
                targetVis.RotateAroundCenter(Vector3.right, -15);
            }
        }

        public void RotateYCallback()
        {
            if (targetVis != null)
            {
                targetVis.RotateAroundCenter(Vector3.up, -15);
            }
        }

        public void RotateZCallback()
        {
            if (targetVis != null)
            {
                targetVis.RotateAroundCenter(Vector3.forward, -15);
            }
        }

        public void ResetCallback()
        {
            if (targetVis != null)
            {
                targetVis.ResetView();
            }
        }

        public void ZoomInCallback()
        {
            if(targetVis != null)
            {
                targetVis.Rescale(1.10f);
            }
        }

        public void ZoomOutCallback()
        {
            if (targetVis != null)
            {
                targetVis.Rescale(0.9f);
            }
        }

        // Call this to update the GUI and its specs when the vis specs of 
        // the target vis is updated.
        public void UpdateGUISpecsFromVisSpecs()
        {
            // Update the JSONNOde specs:
            guiVisSpecs = JSON.Parse(targetVis.GetVisSpecs().ToString());


            List<string> marksList = targetVis.GetMarksList();

            // Update the dropdown options:
            UpdateGUIDataDropdownList(targetVis.GetDataList());
            UpdateGUIMarksDropdownList(marksList);

            if(!marksList.Contains(guiVisSpecs["mark"].Value.ToString()))
            {
                throw new Exception("Cannot find mark name in DxR/Resources/Marks/marks.json");
            }

            // Update the dropdown values:
            UpdateDataDropdownValue(guiVisSpecs["data"]["url"].Value);
UpdateMarkDropdownValue(guiVisSpecs["mark"].Value);

            // Update GUI for channels:
            UpdateGUIChannelsList(guiVisSpecs);

#if USE_INTERACTION_GUI
            // Update GUI for interactions:
            //UpdateGUIInteractionsList(guiVisSpecs);
#endif 
        }

        // Adds or removes channel GUIs according to specs and updates the dropdowns.
        private void UpdateGUIChannelsList(JSONNode guiVisSpecs)
        {
            // Remove all channels;
            RemoveAllChannelGUIs();

            // Go through each channel encoding in the specs and add GUI for each:
            JSONObject channelEncodings = guiVisSpecs["encoding"].AsObject;
            if(channelEncodings != null)
            {
                foreach (KeyValuePair<string, JSONNode> kvp in channelEncodings.AsObject)
                {
                    string channelName = kvp.Key;
                    if(guiVisSpecs["encoding"][channelName]["value"] == null && 
                        IsChannelInMarksChannelList(guiVisSpecs["mark"].Value, channelName))
                    {
                        AddChannelGUI(channelName, kvp.Value.AsObject);
                    }
                }
            }
        }

        // Adds or removes interaction GUIs according to specs and updates the dropdowns.
        private void UpdateGUIInteractionsList(JSONNode guiVisSpecs)
        {
            // Remove all interactions;
            RemoveAllInteractionGUIs();

            // Go through each interaction in the specs and add GUI for each:
            JSONArray interactionSpecsArray = guiVisSpecs["interaction"].AsArray;
            if (interactionSpecsArray != null)
            {
                foreach (JSONObject interactionSpecs in interactionSpecsArray)
                {
                    AddInteractionGUI(interactionSpecs);
                }
            }
        }

        private void AddInteractionGUI(JSONObject interactionSpecs)
        {
            GameObject interactionGUI = AddEmptyInteractionGUI();

            UpdateInteractionGUIInteractionTypeDropdownValue(interactionSpecs["type"].Value, ref interactionGUI);
            UpdateInteractionGUIDataFieldDropdownValue(interactionSpecs["field"].Value, ref interactionGUI);
        }

        private void UpdateInteractionGUIInteractionTypeDropdownValue(string value, ref GameObject interactionGUI)
        {
            Dropdown dropdown = interactionGUI.transform.Find("InteractionTypeDropdown").GetComponent<Dropdown>();
            int valueIndex = GetOptionIndex(dropdown, value);
            if (valueIndex > 0)
            {
                dropdown.value = valueIndex;
            }
        }

        private void UpdateInteractionGUIDataFieldDropdownValue(string value, ref GameObject interactionGUI)
        {
            Dropdown dropdown = interactionGUI.transform.Find("DataFieldDropdown").GetComponent<Dropdown>();
            //string prevValue = dropdown.options[dropdown.value].text;
            int valueIndex = GetOptionIndex(dropdown, value);
            if (valueIndex > 0)
            {
                dropdown.value = valueIndex;
            }
        }

        private bool IsChannelInMarksChannelList(string markName, string channelName)
        {
            return targetVis.GetChannelsList(markName).Contains(channelName);
        }

        private void AddChannelGUI(string channelName, JSONObject channelEncodingSpecs)
        {
            GameObject channelGUI = AddEmptyChannelGUI();

            Debug.Log("Encoding:" + channelName + "," + channelEncodingSpecs.ToString(2));

            UpdateChannelGUIChannelDropdownValue(channelName, ref channelGUI);
            UpdateChannelGUIDataFieldDropdownValue(channelEncodingSpecs["field"].Value, ref channelGUI);
            UpdateChannelGUIDataFieldTypeDropdownValue(channelEncodingSpecs["type"].Value, ref channelGUI);

        }

        private void UpdateChannelGUIDataFieldTypeDropdownValue(string value, ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("DataFieldTypeDropdown").GetComponent<Dropdown>();
            int valueIndex = GetOptionIndex(dropdown, value);
            if (valueIndex > 0)
            {
                dropdown.value = valueIndex;
            }
        }

        private void UpdateChannelGUIDataFieldDropdownValue(string value, ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("DataFieldDropdown").GetComponent<Dropdown>();
            int valueIndex = GetOptionIndex(dropdown, value);
            if (valueIndex > 0)
            {
                dropdown.value = valueIndex;
            }
        }

        private void UpdateChannelGUIChannelDropdownValue(string value, ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("ChannelDropdown").GetComponent<Dropdown>();
            int valueIndex = GetOptionIndex(dropdown, value);
            if (valueIndex > 0)
            {
                dropdown.value = valueIndex;
            }
        }

        private void RemoveAllChannelGUIs()
        {
            Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            for(int i = 0; i < channelListContent.childCount - 1; i++)
            {
                GameObject.Destroy(channelListContent.GetChild(i).gameObject);
            }
        }

        private void RemoveAllInteractionGUIs()
        {
            Transform interactionListContent = gameObject.transform.Find("InteractionList/Viewport/InteractionListContent");
            for (int i = 0; i < interactionListContent.childCount - 1; i++)
            {
                GameObject.Destroy(interactionListContent.GetChild(i).gameObject);
            }
        }

        // Call this to update the vis specs with the current GUI specs.
        public void UpdateVisSpecsFromGUISpecs()
        {
            UpdateGUISpecsFromGUIValues();

            targetVis.UpdateVisSpecsFromGUISpecs();
        }

        private void UpdateGUISpecsFromGUIValues()
        {
            guiVisSpecs["data"]["url"] = dataDropdown.options[dataDropdown.value].text;
            guiVisSpecs["mark"] = markDropdown.options[markDropdown.value].text;

            guiVisSpecs["encoding"] = null;

            JSONObject encodingObject = new JSONObject();
            Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            for (int i = 0; i < channelListContent.childCount - 1; i++)
            {
                GameObject channelGUI = channelListContent.GetChild(i).gameObject;
                JSONObject channelSpecs = new JSONObject();
                
                Dropdown dropdown = channelGUI.transform.Find("DataFieldDropdown").GetComponent<Dropdown>();
                string dataField = dropdown.options[dropdown.value].text;
                channelSpecs.Add("field", new JSONString(dataField));

                dropdown = channelGUI.transform.Find("DataFieldTypeDropdown").GetComponent<Dropdown>();
                string dataFieldType = dropdown.options[dropdown.value].text;
                channelSpecs.Add("type", new JSONString(dataFieldType));

                dropdown = channelGUI.transform.Find("ChannelDropdown").GetComponent<Dropdown>();
                string channel = dropdown.options[dropdown.value].text;
                encodingObject.Add(channel, channelSpecs);
            }

            guiVisSpecs["encoding"] = encodingObject;
            Debug.Log("GUI CHANNEL SPECS: " + guiVisSpecs["encoding"].ToString());

#if USE_INTERACTION_GUI
            // Update interaction specs:
            guiVisSpecs["interaction"] = null;
            JSONArray interactionArrayObject = new JSONArray();
            Transform interactionListContent = gameObject.transform.Find("InteractionList/Viewport/InteractionListContent");
            for (int i = 0; i < interactionListContent.childCount - 1; i++)
            {
                GameObject interactionGUI = interactionListContent.GetChild(i).gameObject;
                JSONObject interactionSpecs = new JSONObject();

                Dropdown dropdown = interactionGUI.transform.Find("DataFieldDropdown").GetComponent<Dropdown>();
                string dataField = dropdown.options[dropdown.value].text;
                interactionSpecs.Add("field", new JSONString(dataField));

                dropdown = interactionGUI.transform.Find("InteractionTypeDropdown").GetComponent<Dropdown>();
                string interactionType = dropdown.options[dropdown.value].text;
                interactionSpecs.Add("type", new JSONString(interactionType));

                interactionArrayObject.Add(interactionSpecs);
            }

            guiVisSpecs["interaction"] = interactionArrayObject;
            Debug.Log("GUI INTERACTION SPECS: " + guiVisSpecs["interaction"].ToString());
#endif
        }

        public JSONNode GetGUIVisSpecs()
        {
            return guiVisSpecs;
        }

        public void CallUpdateVisSpecsFromGUISpecs()
        {
            UpdateVisSpecsFromGUISpecs();
        }

        // TODO:
        public void OnChannelGUIChannelDropdownValueChanged(Dropdown changed)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

//                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        // TODO:
        public void OnChannelGUIDataFieldDropdownValueChanged(Dropdown changed)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

                //                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        public void OnInteractionGUIDataFieldDropdownValueChanged(Dropdown changed, GameObject interactionGUI)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

                //                UpdateGUIChannelsList(guiVisSpecs);
            }

            Debug.Log("Object name " + interactionGUI.name);

            UpdateInteraction(interactionGUI);
        }

        private void UpdateInteraction(GameObject interactionGUI)
        {
            
        }

        public void OnInteractionGUIInteractionTypeDropdownValueChanged(Dropdown changed, GameObject interactionGUI)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

                //                UpdateGUIChannelsList(guiVisSpecs);
            }

            Debug.Log("Object name " + interactionGUI.name);
        }

        // TODO:
        public void OnChannelGUIDataFieldTypeDropdownValueChanged(Dropdown changed)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

                //                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        public void OnDataDropdownValueChanged(Dropdown changed)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                guiVisSpecs["data"]["url"] = curValue;

                // Keep channel field names if they exist in the data
                // and set to undefined if not, so user can use specs as template for new data.
                
                List<string> newDataFields = GetDataFieldsList();
                Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
                for (int i = 0; i < channelListContent.childCount - 1; i++)
                {
                    GameObject channelGUI = channelListContent.GetChild(i).gameObject;
                    
                    Dropdown dropdown = channelGUI.transform.Find("ChannelDropdown").GetComponent<Dropdown>();
                    string channel = dropdown.options[dropdown.value].text;

                    if(!newDataFields.Contains(channel))
                    {
                        dropdown = channelGUI.transform.Find("DataFieldDropdown").GetComponent<Dropdown>();
                        UpdateChannelGUIDataFieldDropdownValue("undefined", ref channelGUI);
                    }
                }

                Debug.Log("Updated specs " + guiVisSpecs["encoding"].ToString());

                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        public void OnMarkDropdownValueChanged(Dropdown changed)
        {
            Debug.Log("New mark " + changed.options[changed.value].text);
            string prevValue = guiVisSpecs["mark"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                guiVisSpecs["mark"] = curValue;
/*
                // Reset channels!
                // TODO: Only reset parts of the spec.
                guiVisSpecs["encoding"] = null;

                Debug.Log("Updated specs " + guiVisSpecs["encoding"].ToString());
                */
                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        public void AddEmptyChannelGUICallback()
        {
            AddEmptyChannelGUI();
        }

        public void AddEmptyInteractionGUICallback()
        {
            AddEmptyInteractionGUI();
        }

        private GameObject AddEmptyChannelGUI()
        {
            Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            GameObject channelGUI = Instantiate(channelGUIPrefab, channelListContent);

            UpdateChannelsListOptions(ref channelGUI);
            UpdateDataFieldListOptions(ref channelGUI);
            UpdateDataFieldTypeOptions(ref channelGUI);

            AddChannelGUIChannelCallback(ref channelGUI);
            AddChannelGUIDataFieldCallback(ref channelGUI);
            AddChannelGUIDataFieldTypeCallback(ref channelGUI);
            AddChannelGUIDeleteCallback(ref channelGUI);

            addChannelButtonTransform.SetAsLastSibling();

            return channelGUI;
        }

        private GameObject AddEmptyInteractionGUI()
        {
            Transform interactionListContent = gameObject.transform.Find("InteractionList/Viewport/InteractionListContent");
            GameObject interactionGUI = Instantiate(interactionGUIPrefab, interactionListContent);

            UpdateDataFieldListOptions(ref interactionGUI);

            AddInteractionGUIDataFieldCallback(interactionGUI);
            AddInteractionGUIInteractionTypeCallback(interactionGUI);
            AddInteractionGUIDeleteCallback(ref interactionGUI);

            addInteractionButtonTransform.SetAsLastSibling();

            return interactionGUI;
        }

        private void AddChannelGUIDeleteCallback(ref GameObject channelGUI)
        {
            Transform deleteChannelObject = channelGUI.transform.Find("DeleteChannelButton");
            Button btn = deleteChannelObject.gameObject.GetComponent<Button>();
            btn.onClick.AddListener(DeleteParentOfClickedObjectCallback);
        }

        private void AddInteractionGUIDeleteCallback(ref GameObject interactionGUI)
        {
            Transform deleteInteractionObject = interactionGUI.transform.Find("DeleteInteractionButton");
            Button btn = deleteInteractionObject.gameObject.GetComponent<Button>();
            btn.onClick.AddListener(DeleteParentOfClickedObjectCallback);
        }

        private void AddChannelGUIChannelCallback(ref GameObject channelGUI)
        {
            Transform dropdownObject = channelGUI.transform.Find("ChannelDropdown");
            Dropdown dropdown = dropdownObject.gameObject.GetComponent<Dropdown>();
            dropdown.onValueChanged.AddListener(delegate {
                OnChannelGUIChannelDropdownValueChanged(dropdown);
            });
        }

        private void AddChannelGUIDataFieldCallback(ref GameObject channelGUI)
        {
            Transform dropdownObject = channelGUI.transform.Find("DataFieldDropdown");
            Dropdown dropdown = dropdownObject.gameObject.GetComponent<Dropdown>();
            dropdown.onValueChanged.AddListener(delegate {
                OnChannelGUIDataFieldDropdownValueChanged(dropdown);
            });
        }

        private void AddInteractionGUIDataFieldCallback(GameObject interactionGUI)
        {
            Transform dropdownObject = interactionGUI.transform.Find("DataFieldDropdown");
            Dropdown dropdown = dropdownObject.gameObject.GetComponent<Dropdown>();
            dropdown.onValueChanged.AddListener(delegate {
                OnInteractionGUIDataFieldDropdownValueChanged(dropdown, interactionGUI);
            });
        }

        private void AddInteractionGUIInteractionTypeCallback(GameObject interactionGUI)
        {
            Transform dropdownObject = interactionGUI.transform.Find("InteractionTypeDropdown");
            Dropdown dropdown = dropdownObject.gameObject.GetComponent<Dropdown>();
            dropdown.onValueChanged.AddListener(delegate {
                OnInteractionGUIInteractionTypeDropdownValueChanged(dropdown, interactionGUI);
            });
        }

        private void AddChannelGUIDataFieldTypeCallback(ref GameObject channelGUI)
        {
            Transform dropdownObject = channelGUI.transform.Find("DataFieldTypeDropdown");
            Dropdown dropdown = dropdownObject.gameObject.GetComponent<Dropdown>();
            dropdown.onValueChanged.AddListener(delegate {
                OnChannelGUIDataFieldTypeDropdownValueChanged(dropdown);
            });
        }

        private void UpdateChannelsListOptions(ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("ChannelDropdown").GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(GetChannelDropdownOptions());
        }

        private void DeleteParentOfClickedObjectCallback()
        {
            Debug.Log("Clicked " + EventSystem.current.currentSelectedGameObject.transform.parent.name);
            //
            GameObject.Destroy(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject);
        }
        
        public List<string> GetChannelDropdownOptions()
        {
            return targetVis.GetChannelsList(markDropdown.options[markDropdown.value].text);
        }

        private void UpdateDataFieldListOptions(ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("DataFieldDropdown").GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(GetDataFieldDropdownOptions());
        }

        public List<string> GetDataFieldDropdownOptions()
        {
            List<string> fieldsListOptions = new List<string> { DxR.Vis.UNDEFINED };
            if(guiVisSpecs["data"]["url"].Value == "inline")
            {
                fieldsListOptions.AddRange(targetVis.GetDataFieldsListFromValues(guiVisSpecs["data"]["values"]));
            } else
            {
                fieldsListOptions.AddRange(targetVis.GetDataFieldsListFromURL(guiVisSpecs["data"]["url"].Value));
            }

            return fieldsListOptions;
        }

        public List<string> GetDataFieldsList()
        {
            if (guiVisSpecs["data"]["url"].Value == "inline")
            {
                return targetVis.GetDataFieldsListFromValues(guiVisSpecs["data"]["values"]);
            }
            else
            {
                return targetVis.GetDataFieldsListFromURL(guiVisSpecs["data"]["url"].Value);
            }
        }

        private void UpdateDataFieldTypeOptions(ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("DataFieldTypeDropdown").GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(dataFieldTypeDropdownOptions);
        }

        public void UpdateGUIMarksDropdownList(List<string> marksList)
        {
            markDropdown.ClearOptions();
            markDropdown.AddOptions(marksList);
        }

        public void UpdateGUIDataDropdownList(List<string> dataList)
        {
            dataDropdown.ClearOptions();
            dataDropdown.AddOptions(dataList);
        }

        private int GetOptionIndex(Dropdown dropdown, string value)
        {
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (dropdown.options[i].text == value)
                {
                    return i;
                }
            }

            return -1;
        }

        public void UpdateDataDropdownValue(string value)
        {
            string prevValue = dataDropdown.options[dataDropdown.value].text;
            int valueIndex = GetOptionIndex(dataDropdown, value);
            if (valueIndex > 0)
            {
                dataDropdown.value = valueIndex;
            }

            Debug.Log("Updated GUI data value to " + value);
        }

        public void UpdateMarkDropdownValue(string value)
        {
            int valueIndex = GetOptionIndex(markDropdown, value);
            if (valueIndex > 0)
            {
                markDropdown.value = valueIndex;
            }
        }
    }
}
