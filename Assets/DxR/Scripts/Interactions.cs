using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DxR
{
    public class Interactions : MonoBehaviour
    {
        // Y offset for placement of filter objects.
        float curYOffset = 0;

        Vis targetVis = null;
        // Each data field's filter result. Each list is the same as the number of 
        // mark instances.
        public Dictionary<string, List<bool>> filterResults = null;

        Dictionary<string, List<string>> domains = null;

        // Use this for initialization
        void Start()
        {
           
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Init(Vis vis)
        {
            targetVis = vis;
            curYOffset = 0;
            
            if (targetVis != null)
            {
                filterResults = new Dictionary<string, List<bool>>();
                domains = new Dictionary<string, List<string>>();
            }
        }

        public void EnableLegendToggleFilter(GameObject legendGameObject)
        {
            HoloToolkit.Examples.InteractiveElements.InteractiveSet checkBoxSet =
               legendGameObject.GetComponent<HoloToolkit.Examples.InteractiveElements.InteractiveSet>();
            if (checkBoxSet == null) return;
            checkBoxSet.SelectedIndices.Clear();
            
            string fieldName = "";
            List<string> domain = new List<string>();

            // Go through each checkbox and set them to active:
            int checkBoxIndex = 0;
            for (int i = 0; i < legendGameObject.transform.childCount; i++)
            {
                Transform child = legendGameObject.transform.GetChild(i);
                if(child.GetComponent<LegendValue>() != null)
                {
                    fieldName = child.GetComponent<LegendValue>().dataFieldName;
                    domain.Add(child.GetComponent<LegendValue>().categoryName);

                    Transform box = child.Find("Title/CheckBox");
                    if(box != null)
                    {
                        box.gameObject.SetActive(true);
                        HoloToolkit.Examples.InteractiveElements.InteractiveToggle toggle = 
                            box.gameObject.GetComponent<HoloToolkit.Examples.InteractiveElements.InteractiveToggle>();

                        if(toggle != null)
                        {
                            checkBoxSet.Interactives.Add(toggle);
                            checkBoxSet.SelectedIndices.Add(checkBoxIndex);
                            checkBoxIndex++;
                        }
                    }
                }
            }

            // Add the call back function to update marks visibility when any checkbox is updated.
            checkBoxSet.OnSelectionEvents.AddListener(LegendToggleFilterUpdated);

            domains.Add(fieldName, domain);

            // Update the results vector
            int numMarks = targetVis.markInstances.Count;
            List<bool> results = new List<bool>(new bool[numMarks]);
            for (int j = 0; j < results.Count; j++)
            {
                results[j] = true;
            }
            filterResults.Add(fieldName, results);
        }
        
        void LegendToggleFilterUpdated()
        {
            if (EventSystem.current.currentSelectedGameObject == null) return;

            // If the selected object is not a check box, ignore.
            if (EventSystem.current.currentSelectedGameObject.transform.Find("CheckBoxOutline") == null) return;

            GameObject selectedCheckBox = EventSystem.current.currentSelectedGameObject;
            if (selectedCheckBox != null && targetVis != null)
            {
                string fieldName = selectedCheckBox.transform.parent.transform.parent.GetComponent<LegendValue>().dataFieldName;
                string categoryName = selectedCheckBox.transform.parent.transform.parent.GetComponent<LegendValue>().categoryName;

                GameObject legendGameObject = selectedCheckBox.transform.parent.transform.parent.transform.parent.gameObject;

                // Update filter results for toggled data field category.
                UpdateFilterResultsForCategoryFromLegend(legendGameObject, fieldName, categoryName);

                targetVis.FiltersUpdated();
                Debug.Log("Filter updated for " + fieldName);
            }
        }

        private void UpdateFilterResultsForCategoryFromLegend(GameObject legendObject, string field, string category)
        {
            if (legendObject == null) return;
            
            List<string> visibleCategories = new List<string>();
            for(int i = 0; i < legendObject.transform.childCount; i++)
            {
                LegendValue legendValue = legendObject.transform.GetChild(i).gameObject.GetComponent<LegendValue>();
                if (legendValue != null)
                {
                    Transform box = legendObject.transform.GetChild(i).Find("Title/CheckBox");
                    HoloToolkit.Examples.InteractiveElements.InteractiveToggle toggle =
                            box.gameObject.GetComponent<HoloToolkit.Examples.InteractiveElements.InteractiveToggle>();

                    // BUG: HasSelection is not up-to-date upon calling this function!!!
                    if(toggle.HasSelection)
                    {
                        visibleCategories.Add(legendValue.categoryName);
                    }
                }
            }

            Debug.Log("Updating filter results for field, category " + field + ", " + category);
            List<bool> res = filterResults[field];
            for (int b = 0; b < res.Count; b++)
            {
                if (visibleCategories.Contains(targetVis.markInstances[b].GetComponent<Mark>().datum[field]))
                {
                    res[b] = true;
                }
                else
                {
                    res[b] = false;
                }
            }

            filterResults[field] = res;
        }

        internal void AddToggleFilter(JSONObject interactionSpecs)
        {
            /*
            if (gameObject.transform.Find(interactionSpecs["field"].Value) != null)
            {
                Debug.Log("Will not duplicate existing filter for field " + interactionSpecs["field"].Value);
                return;
            }
            */

            GameObject toggleFilterPrefab = Resources.Load("GUI/ToggleFilter", typeof(GameObject)) as GameObject;
            if (toggleFilterPrefab == null) return;

            GameObject toggleFilterInstance = Instantiate(toggleFilterPrefab, gameObject.transform);

            toggleFilterInstance.transform.Find("ToggleFilterLabel").gameObject.GetComponent<TextMesh>().text =
                interactionSpecs["field"].Value + ":";

            toggleFilterInstance.name = interactionSpecs["field"];

            HoloToolkit.Unity.Collections.ObjectCollection collection = toggleFilterInstance.GetComponent<HoloToolkit.Unity.Collections.ObjectCollection>();
            if (collection == null) return;

            // Use the provided domain of the data field to create check boxes.
            // For each checkbox, add it to the interactiveset object, and add it to the object
            // collection object and update the layout.
            GameObject checkBoxPrefab = Resources.Load("GUI/CheckBox", typeof(GameObject)) as GameObject;
            if (checkBoxPrefab == null) return;

            HoloToolkit.Examples.InteractiveElements.InteractiveSet checkBoxSet =
                toggleFilterInstance.GetComponent<HoloToolkit.Examples.InteractiveElements.InteractiveSet>();
            if (checkBoxSet == null) return;

            List<string> domain = new List<string>();

            checkBoxSet.SelectedIndices.Clear();
            int i = 0;
            foreach (JSONNode category in interactionSpecs["domain"].AsArray)
            {
                GameObject checkBoxInstance = Instantiate(checkBoxPrefab, toggleFilterInstance.transform);

                Debug.Log("Creating toggle button for " + category.Value);
                checkBoxInstance.transform.Find("CheckBoxOutline/Label").gameObject.GetComponent<TextMesh>().text = category.Value;

                domain.Add(category.Value);

                checkBoxSet.Interactives.Add(checkBoxInstance.GetComponent<HoloToolkit.Examples.InteractiveElements.InteractiveToggle>());
                checkBoxSet.SelectedIndices.Add(i);
                i++;
            }

            domains.Add(interactionSpecs["field"].Value, domain);

            int numRows = interactionSpecs["domain"].AsArray.Count + 1;
            collection.Rows = numRows;
            collection.UpdateCollection();

            // Add the call back function to update marks visibility when any checkbox is updated.
            checkBoxSet.OnSelectionEvents.AddListener(ToggleFilterUpdated);

            // Update the results vector
            int numMarks = targetVis.markInstances.Count;
            List<bool> results = new List<bool>(new bool[numMarks]);
            for(int j = 0; j < results.Count; j++)
            {
                results[j] = true;
            }
            filterResults.Add(interactionSpecs["field"], results);

            toggleFilterInstance.transform.Translate(0, -curYOffset / 2.0f, 0);
            curYOffset = curYOffset + (0.085f * numRows) + 0.1f; 
        }

        void ToggleFilterUpdated()
        {
            if (EventSystem.current.currentSelectedGameObject == null) return;

            // If the selected object is not a check box, ignore.
            if (EventSystem.current.currentSelectedGameObject.transform.Find("CheckBoxOutline") == null) return;

            GameObject selectedCheckBox = EventSystem.current.currentSelectedGameObject;
            if (selectedCheckBox != null && targetVis != null)
            {
                // Update filter results for toggled data field category.
                UpdateFilterResultsForCategory(selectedCheckBox.transform.parent.name, selectedCheckBox.transform.Find("CheckBoxOutline/Label").gameObject.GetComponent<TextMesh>().text);

                targetVis.FiltersUpdated();
                Debug.Log("Filter updated! " +
                EventSystem.current.currentSelectedGameObject.transform.parent.name);
            }
        }

        private void UpdateFilterResultsForCategory(string field, string category)
        {
            GameObject toggleFilter = gameObject.transform.Find(field).gameObject;
            if (toggleFilter == null) return;

            HoloToolkit.Examples.InteractiveElements.InteractiveSet checkBoxSet =
                toggleFilter.GetComponent<HoloToolkit.Examples.InteractiveElements.InteractiveSet>();
            if (checkBoxSet == null) return;

            List<string> visibleCategories = new List<string>();
            foreach (int checkedCategoryIndex in checkBoxSet.SelectedIndices)
            {
                visibleCategories.Add(domains[field][checkedCategoryIndex]);

                Debug.Log("showing index: " + checkedCategoryIndex.ToString() + (domains[field][checkedCategoryIndex]));
            }

            Debug.Log("Updating filter results for field, category " + field + ", " + category);
            List<bool> res = filterResults[field];
            for(int b = 0; b < res.Count; b++)
            {
                if(visibleCategories.Contains(targetVis.markInstances[b].GetComponent<Mark>().datum[field]))
                {
                    res[b] = true;
                } else
                {
                    res[b] = false;
                }
            }

            filterResults[field] = res;
        }

        internal void EnableAxisThresholdFilter(string field)
        {
            int numMarks = targetVis.markInstances.Count;
            List<bool> results = new List<bool>(new bool[numMarks]);
            for (int j = 0; j < results.Count; j++)
            {
                results[j] = true;
            }
            filterResults.Add(field, results);
        }

        internal void AddThresholdFilter(JSONObject interactionSpecs)
        {
            GameObject thresholdFilterPrefab = Resources.Load("GUI/ThresholdFilter", typeof(GameObject)) as GameObject;
            if (thresholdFilterPrefab == null) return;

            GameObject thresholdFilterInstance = Instantiate(thresholdFilterPrefab, gameObject.transform);
            thresholdFilterInstance.transform.Find("ThresholdFilterLabel").gameObject.GetComponent<TextMesh>().text =
                interactionSpecs["field"].Value + ":";
            thresholdFilterInstance.name = interactionSpecs["field"];

            thresholdFilterInstance.transform.Find("ThresholdFilterMinLabel").gameObject.GetComponent<TextMesh>().text =
                interactionSpecs["domain"][0].Value;
            thresholdFilterInstance.transform.Find("ThresholdFilterMaxLabel").gameObject.GetComponent<TextMesh>().text =
                interactionSpecs["domain"][1].Value;

            DxR.SliderGestureControlBothSide sliderControl =
                thresholdFilterInstance.GetComponent<DxR.SliderGestureControlBothSide>();
            if (sliderControl == null) return;

            float domainMin = float.Parse(interactionSpecs["domain"][0].Value);
            float domainMax = float.Parse(interactionSpecs["domain"][1].Value);

            // TODO: Check validity of specs.

            sliderControl.SetSpan(domainMin, domainMax);
            sliderControl.SetSliderValue1(domainMin);
            sliderControl.SetSliderValue2(domainMax);

            sliderControl.OnUpdateEvent.AddListener(ThresholdFilterUpdated);
            
            // Update the results vector
            int numMarks = targetVis.markInstances.Count;
            List<bool> results = new List<bool>(new bool[numMarks]);
            for (int j = 0; j < results.Count; j++)
            {
                results[j] = true;
            }
            filterResults.Add(interactionSpecs["field"], results);
          
            thresholdFilterInstance.transform.Translate(0, -curYOffset / 2.0f, 0);
            curYOffset = curYOffset + (0.25f);
        }

        public void ThresholdFilterUpdated()
        {
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
            if (selectedObject == null) return;

            Debug.Log("Threshold updated");

            string fieldName = "";
            // If the selected object is not a slider, ignore.
            if (selectedObject.transform.Find("SliderBar") != null)
            {
                fieldName = selectedObject.name;
            } else if(selectedObject.name == "SliderBar")
            {
                fieldName = selectedObject.transform.parent.name;
                selectedObject = selectedObject.transform.parent.transform.gameObject;
            } else
            {
                return;
            }

            DxR.SliderGestureControlBothSide sliderControl =
               selectedObject.GetComponent<DxR.SliderGestureControlBothSide>();
            
            if (sliderControl != null && targetVis != null)
            {
                // Update filter results for thresholded data field category.
                if(sliderControl.SliderValue1< sliderControl.SliderValue2)
                    UpdateFilterResultsForThreshold(fieldName, sliderControl.SliderValue1, sliderControl.SliderValue2);
                else
                    UpdateFilterResultsForThreshold(fieldName, sliderControl.SliderValue2, sliderControl.SliderValue1);

                targetVis.FiltersUpdated();
                Debug.Log("Filter updated! " + fieldName);
            }
        }

        private void UpdateFilterResultsForThreshold(string field, float thresholdMinValue, float thresholdMaxValue)
        {
            Debug.Log("Updating filter results for field, threshold " + field + ", " + thresholdMinValue.ToString() +"<"+ thresholdMaxValue.ToString());
            List<bool> res = filterResults[field];
            for (int b = 0; b < res.Count; b++)
            {
                if (float.Parse(targetVis.markInstances[b].GetComponent<Mark>().datum[field]) >= thresholdMinValue && float.Parse(targetVis.markInstances[b].GetComponent<Mark>().datum[field]) <= thresholdMaxValue)
                {
                    res[b] = true;
                }
                else
                {
                    res[b] = false;
                }
            }

            filterResults[field] = res;
        }
    }
}