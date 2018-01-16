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
        Dropdown dataDropdown = null;
        Dropdown markDropdown = null;
        Vis targetVis = null;

        Transform addChannelButtonTransform = null;
        GameObject channelParamsPrefab = null;

        // List of options for dropdown menus in ChannelGUI object.
        // These are always synced between the vis object and GUI object.
        List<string> channelDropdownOptions;
        List<string> dataFieldDropdownOptions;
        List<string> dataFieldTypeDropdownOptions;

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

            channelDropdownOptions = new List<string>();
            dataFieldDropdownOptions = new List<string>();
            dataFieldTypeDropdownOptions = new List<string> { "quantitative", "nominal", "ordinal", "temporal" };

            Transform dataDropdownTransform = gameObject.transform.Find("DataDropdown");
            dataDropdown = dataDropdownTransform.gameObject.GetComponent<Dropdown>();

            Transform marksDropdownTransform = gameObject.transform.Find("MarkDropdown");
            markDropdown = marksDropdownTransform.gameObject.GetComponent<Dropdown>();

            Button btn = gameObject.transform.Find("UpdateButton").GetComponent<Button>();
            btn.onClick.AddListener(CallUpdateVisSpecsFromGUISpecs);

            channelParamsPrefab = Resources.Load("GUI/ChannelGUI") as GameObject;

            addChannelButtonTransform = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent/AddChannelButton");
            Button addChannelBtn = addChannelButtonTransform.GetComponent<Button>();
            addChannelBtn.onClick.AddListener(CallAddChannelGUI);
        }

        public void CallUpdateVisSpecsFromGUISpecs()
        {
            targetVis.UpdateVisSpecsFromGUISpecs();
        }

        public void CallAddChannelGUI()
        {
            AddChannelGUI();
            addChannelButtonTransform.SetAsLastSibling();
        }

        public void AddChannelGUI(GUIChannelParams guiParams)
        {
            GameObject channelGUI = AddChannelGUI();
            //channelGUI.
        }

        private GameObject AddChannelGUI()
        {
            Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            GameObject channelGUI = Instantiate(channelParamsPrefab, channelListContent);

            UpdateChannelsListOptions(ref channelGUI);
            UpdateDataFieldListOptions(ref channelGUI);
            UpdateDataFieldTypeOptions(ref channelGUI);

            return channelGUI;
        }

        private void UpdateChannelsListOptions(ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("ChannelDropdown").GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(channelDropdownOptions);
        }

        private void UpdateDataFieldListOptions(ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("DataFieldDropdown").GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(dataFieldDropdownOptions);
        }


        private void UpdateDataFieldTypeOptions(ref GameObject channelGUI)
        {
            Dropdown dropdown = channelGUI.transform.Find("DataFieldTypeDropdown").GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(dataFieldTypeDropdownOptions);
        }

        public void UpdateMarksList(List<string> marksList)
        {
            markDropdown.ClearOptions();
            markDropdown.AddOptions(marksList);
        }

        public void UpdateDataList(List<string> dataList)
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

        public void UpdateDataValue(string value)
        {
            string prevValue = dataDropdown.options[dataDropdown.value].text;
            int valueIndex = GetOptionIndex(dataDropdown, value);
            if (valueIndex > 0)
            {
                dataDropdown.value = valueIndex;
            }

            /*
            if(prevValue != value)
            {
                UpdateAllDataFieldListOptions();
            }
            */
        }

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

        public void UpdateMarkValue(string value)
        {
            int valueIndex = GetOptionIndex(markDropdown, value);
            if (valueIndex > 0)
            {
                markDropdown.value = valueIndex;
            }

            UpdateAllChannelsListOptions();
        }

        // Update all channel GUI's channel list options
        private void UpdateAllChannelsListOptions()
        {
            throw new NotImplementedException();
        }

        public string GetCurrentDataValue()
        {
            return dataDropdown.options[dataDropdown.value].text;
        }

        public string GetCurrentMarkValue()
        {
            return markDropdown.options[markDropdown.value].text;
        }
        
        internal void UpdateChannels(JSONNode encodingSpecs)
        {
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
        }
    }

}
