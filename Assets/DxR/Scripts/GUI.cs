using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using System;

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

            Transform marksDropdownTransform = gameObject.transform.Find("MarkDropdown");
            markDropdown = marksDropdownTransform.gameObject.GetComponent<Dropdown>();

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

            // Go through each channel encoding in the specs; for each encoding:
            //      If the encoding is new, add it.
            //      If the encoding is not not new, but data types are the same, only change the field and domain;
            //          otherwise delete previous content and add new specs.
            JSONObject channelEncodings = guiVisSpecs["encoding"].AsObject;
            if(channelEncodings != null)
            {
                foreach (KeyValuePair<string, JSONNode> kvp in channelEncodings.AsObject)
                {
                    AddChannelGUI(kvp.Key, kvp.Value.AsObject);
                }
                /*
                for (int i = 0; i < channelEncodings.Count; i++)
                {
                    AddChannelGUI(channelEncodings[i].Value, channelEncodings[i].AsObject);
                }
                */
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
                GameObject.Destroy(channelListContent.GetChild(i));
            }
        }

        // Call this to update the vis specs with the current GUI specs.
        public void UpdateVisSpecsFromGUISpecs()
        {
            targetVis.UpdateVisSpecsFromGUISpecs();
        }

        public JSONNode GetGUIVisSpecs()
        {
            return guiVisSpecs;
        }

        public void CallUpdateVisSpecsFromGUISpecs()
        {
            targetVis.UpdateVisSpecsFromGUISpecs();
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

            addChannelButtonTransform.SetAsLastSibling();

            return channelGUI;
        }

        private void UpdateChannelsListOptions(ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("ChannelDropdown").GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(GetChannelDropdownOptions());
        }

        public List<string> GetChannelDropdownOptions()
        {
            List<string> list = new List<string>();

            // TODO:
            list.Add("x");
            list.Add("y");
            list.Add("z");
            list.Add("color");
            list.Add("opacity");
            list.Add("size");
            list.Add("width");
            list.Add("height");
            list.Add("depth");

            return list;
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

        /*
        // Update all channel GUI's data field list options
        // TODO:
        private void UpdateAllDataFieldListOptions()
        {
            // Update the global list of options.
            string curDataValue = dataDropdown.options[dataDropdown.value].text;
            //dataFieldDropdownOptions = targetVis.GetDataFieldsList(curDataValue);

            // Go through each channel GUI to update the dropdowns, keeping the values
            // if available; if not available, set to UNDEFINED.
            Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            for (int i = 0; i < channelListContent.childCount - 1; i++)
            {
                GameObject channelGUI = channelListContent.GetChild(i).gameObject;
                Dropdown dropdown = channelGUI.transform.Find("DataFieldDropdown").GetComponent<Dropdown>();

                string curValue = dropdown.options[dropdown.value].text;
                UpdateDataFieldListOptions(ref channelGUI);

                // TODO:
            }
        }
        */

        public void UpdateMarkDropdownValue(string value)
        {
            int valueIndex = GetOptionIndex(markDropdown, value);
            if (valueIndex > 0)
            {
                markDropdown.value = valueIndex;
            }
        }
        
        internal void UpdateChannels(JSONNode encodingSpecs)
        {
            /*
            JSONArray channelEncodings = encodingSpecs.AsArray;

            // Go through each channel encoding in the specs; for each encoding:
            //      If the encoding is new, add it.
            //      If the encoding is not not new, but data types are the same, only change the field and domain;
            //          otherwise delete previous content and add new specs.
            for(int i = 0; i < channelEncodings.Count; i++)
            {

            }
            
            // Go through each channel in the GUI
            //      If the channel is not in the specs, remove it from the GUI.
            */
        }
    }
}
