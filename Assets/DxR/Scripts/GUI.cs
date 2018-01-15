using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;

namespace DxR
{
    public class GUI : MonoBehaviour
    {
        Dropdown dataDropdown = null;
        Dropdown markDropdown = null;
        Vis targetVis = null;

        // Use this for initialization
        void Start()
        {

        }

        public void Init(Vis targetVisInstance)
        {
            targetVis = targetVisInstance;

            Transform dataDropdownTransform = gameObject.transform.Find("DataDropdown");
            dataDropdown = dataDropdownTransform.gameObject.GetComponent<Dropdown>();

            Transform marksDropdownTransform = gameObject.transform.Find("MarkDropdown");
            markDropdown = marksDropdownTransform.gameObject.GetComponent<Dropdown>();

            Button btn = gameObject.transform.Find("UpdateButton").GetComponent<Button>();
            btn.onClick.AddListener(CallVisUpdate);
        }

        public void CallVisUpdate()
        {
            targetVis.UpdateVisSpecsFromGUISpecs();
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
            int valueIndex = GetOptionIndex(dataDropdown, value);
            if (valueIndex > 0)
            {
                dataDropdown.value = valueIndex;
            }
        }

        public void UpdateMarkValue(string value)
        {
            int valueIndex = GetOptionIndex(markDropdown, value);
            if (valueIndex > 0)
            {
                markDropdown.value = valueIndex;
            }
        }

        public string GetCurrentDataValue()
        {
            return dataDropdown.options[dataDropdown.value].text;
        }

        public string GetCurrentMarkValue()
        {
            return markDropdown.options[markDropdown.value].text;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
