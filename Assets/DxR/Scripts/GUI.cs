using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace DxR
{
    public class GUI : MonoBehaviour
    {
        Vis targetVis = null;
        JSONNode guiVisSpecs = null;
        Dropdown dataDropdown = null;
        Dropdown markDropdown = null;
        
        Transform addChannelButtonTransform = null;
        GameObject channelGUIPrefab = null;
        
        List<string> dataFieldTypeDropdownOptions;

        /// <summary>
        /// Whenever a GUI action is performed, button clicked, dropdown clicked, etc., the guiVisSpecs is automatically updated so it 
        /// should be in sync all the time. The visSpecs of the targetVis is only updated when calling UpdateVisSpecsFromGUISpecs, and 
        /// for the other way around, the guiVisSpecs is updated from the targetVis specs when calling UpdateGUISpecsFromVisSpecs.
        /// </summary>
        public struct GUIChannelParams
        {
            string channel;
            string dataField;
            string dataType;
        }

        // Use this for initialization
        void Start()
        {

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
            addChannelBtn.onClick.AddListener(CallAddEmptyChannelGUI);
            
            UpdateGUISpecsFromVisSpecs();
        }

        // Call this to update the GUI and its specs when the vis specs of 
        // the target vis is updated.
        public void UpdateGUISpecsFromVisSpecs()
        {
            // Update the JSONNOde specs:
            guiVisSpecs = JSON.Parse(targetVis.GetVisSpecs().ToString());

            // Update the dropdown options:
            UpdateGUIDataDropdownList(targetVis.GetDataList());
            UpdateGUIMarksDropdownList(targetVis.GetMarksList());
            
            // Update the dropdown values:
            UpdateDataDropdownValue(guiVisSpecs["data"]["url"].Value);
            UpdateMarkDropdownValue(guiVisSpecs["mark"].Value);

            // Update GUI for channels:
            UpdateGUIChannelsList(guiVisSpecs);
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
                    if(guiVisSpecs["encoding"][kvp.Key]["value"] == null)
                    {
                        AddChannelGUI(kvp.Key, kvp.Value.AsObject);
                    }
                }
            }
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
            //string prevValue = dropdown.options[dropdown.value].text;
            int valueIndex = GetOptionIndex(dropdown, value);
            if (valueIndex > 0)
            {
                dropdown.value = valueIndex;
            }
        }

        private void UpdateChannelGUIDataFieldDropdownValue(string value, ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("DataFieldDropdown").GetComponent<Dropdown>();
            //string prevValue = dropdown.options[dropdown.value].text;
            int valueIndex = GetOptionIndex(dropdown, value);
            if (valueIndex > 0)
            {
                dropdown.value = valueIndex;
            }
        }

        private void UpdateChannelGUIChannelDropdownValue(string value, ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("ChannelDropdown").GetComponent<Dropdown>();
            //string prevValue = dropdown.options[dropdown.value].text;
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

            Debug.Log("GUI SPECS: " + guiVisSpecs["encoding"].ToString());
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

                // Reset channels!
                // TODO: Only reset parts of the spec.
                guiVisSpecs["encoding"] = null;

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

        public void CallAddEmptyChannelGUI()
        {
            AddEmptyChannelGUI();
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

        private void AddChannelGUIDeleteCallback(ref GameObject channelGUI)
        {
            Transform deleteChannelObject = channelGUI.transform.Find("DeleteChannelButton");
            Button btn = deleteChannelObject.gameObject.GetComponent<Button>();
            btn.onClick.AddListener(DeleteChannelGUICallback);
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

        private void DeleteChannelGUICallback()
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
             return targetVis.GetDataFieldsList(guiVisSpecs["data"]["url"].Value);
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
