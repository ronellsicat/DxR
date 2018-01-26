using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;
using System;

namespace DxR
{
    /// <summary>
    /// This component can be attached to any GameObject (_parentObject) to create a 
    /// data visualization. This component takes a JSON specification as input, and
    /// generates an interactive visualization as output. The JSON specification 
    /// describes the visualization using ONE type of mark and one or more channels.
    /// </summary>
    public class Vis : MonoBehaviour
    {
        string visSpecsURL = "example.json";                     // URL of vis specs; relative to specsRootPath directory.
        bool enableGUI = true;                                   // Switch for in-situ GUI editor.
        bool enableSpecsExpansion = true;                       // Switch for automatically replacing the vis specs text file on disk with inferrence result.
        bool enableTooltip = true;                               // Switch for tooltip that shows datum attributes on-hover of mark instance.
        bool verbose = true;                                     // Switch for verbose log.

        public static string UNDEFINED = "undefined";                   // Value used for undefined objects in the JSON vis specs.
        public static float SIZE_UNIT_SCALE_FACTOR = 1.0f / 1000.0f;    // Conversion factor to convert each Unity unit to 1 meter.
        public static float DEFAULT_VIS_DIMS = 500.0f;                  // Default dimensions of a visualization, if not specified.
                
        JSONNode visSpecs;                                              // Vis specs that is synced w/ the inferred vis specs and vis.
        JSONNode visSpecsInferred;                                      // This is the inferred vis specs and is ultimately used for construction.

        Parser parser = null;                                           // Parser of JSON specs and data in text format to JSONNode object specs.
        GUI gui = null;                                                 // GUI object (class) attached to GUI game object.
        GameObject tooltip = null;                                      // Tooltip game object for displaying datum info, e.g., on-hover.

        string guiDataRootPath = "Assets/StreamingAssets/DxRData/";     // Root directory for data files used by GUI.
        string guiMarksRootPath = "Assets/DxR/Resources/Marks/";        // Root directory for marks folders used by GUI.

        // Vis Properties:
        string title;                                                   // Title of vis displayed.
        float width;                                                    // Width of scene in millimeters.
        float height;                                                   // Heigh of scene in millimeters.
        float depth;                                                    // Depth of scene in millimeters.
        string markType;                                                // Type or name of mark used in vis.
        public Data data;                                               // Object containing data.

        public List<GameObject> markInstances;                                 // List of mark instances; each mark instance corresponds to a datum.

        private GameObject parentObject = null;                         // Parent game object for all generated objects associated to vis.

        private GameObject viewParentObject = null;                     // Parent game object for all view related objects - axis, legend, marks.
        private GameObject marksParentObject = null;                    // Parent game object for all mark instances.

        private GameObject guidesParentObject = null;                   // Parent game object for all guies (axes/legends) instances.
        private GameObject interactionsParentObject = null;             // Parent game object for all interactions, e.g., filters.
        private GameObject markPrefab = null;                           // Prefab game object for instantiating marks.
        private List<ChannelEncoding> channelEncodings = null;          // List of channel encodings.
        
        // TODO: Move these to the anchor object.
        private bool distanceVisibility = true;                         // Switch for controlling visibility by user-vis distance.
        private bool gazeVisibility = true;                             // Switch for toggling visibility on-hover on the Anchor object.
        private bool currentVisibility = true;                          // Status of vis visibility.

        void Start()
        {
            // Initialize objects:
            parentObject = gameObject;
            viewParentObject = gameObject.transform.Find("DxRView").gameObject;
            marksParentObject = viewParentObject.transform.Find("DxRMarks").gameObject;
            guidesParentObject = viewParentObject.transform.Find("DxRGuides").gameObject;
            interactionsParentObject = gameObject.transform.Find("DxRInteractions").gameObject;

            if (viewParentObject == null || marksParentObject == null)
            {
                throw new Exception("Unable to load DxRView and/or DxRMarks objects.");
            }

            parser = new Parser();

            // Parse the vis specs URL into the vis specs object.
            parser.Parse(visSpecsURL, out visSpecs);

            // Initialize the GUI based on the initial vis specs.
            InitGUI();
            InitTooltip();
            InitAnchor();

            // Update vis based on the vis specs.
            UpdateVis();
        }

        private void InitAnchor()
        {
            Anchor anchor = parentObject.transform.Find("DxRAnchor").transform.GetComponent<Anchor>();
            if(anchor != null)
            {
                anchor.EnableTapToPlace();
            }
        }

        public JSONNode GetVisSpecs()
        {
            return visSpecs;
        }

        public int GetNumMarkInstances()
        {
            return markInstances.Count;
        }

        private void InitTooltip()
        {
            GameObject tooltipPrefab = Resources.Load("Tooltip/Tooltip") as GameObject;
            if(tooltipPrefab != null)
            {
                tooltip = Instantiate(tooltipPrefab, parentObject.transform);
                tooltip.SetActive(false);
            }
        }

        /// <summary>
        /// Update the visualization based on the current visSpecs object (whether updated from GUI or text editor).
        /// Currently, deletes everything and reconstructs everything from scratch.
        /// TODO: Only reconstruct newly updated properties.
        /// </summary>
        private void UpdateVis()
        {
            DeleteAll();

            UpdateVisConfig();

            UpdateVisData();

            UpdateMarkPrefab();

            InferVisSpecs();

            ConstructVis(visSpecsInferred);
        }

        private void ConstructVis(JSONNode specs)
        {
            CreateChannelEncodingObjects(specs);

            ConstructMarkInstances();

            ApplyChannelEncodings();

            ConstructAxes(specs);

            ConstructLegends(specs);

            ConstructInteractions(specs);
        }

        private void ConstructInteractions(JSONNode specs)
        {
            if (specs["interaction"] == null) return;

            interactionsParentObject.GetComponent<Interactions>().Init(this);

            foreach (JSONObject interactionSpecs in specs["interaction"].AsArray)
            {
                if(interactionSpecs["type"] != null && interactionSpecs["field"] != null && interactionSpecs["domain"] != null)
                {
                    switch(interactionSpecs["type"].Value)
                    {
                        case "rangeFilter":
                            break;

                        case "toggleFilter":
                            AddToggleFilterInteraction(interactionSpecs);
                            break;

                        default:
                            return;
                    }

                    Debug.Log("Constructed interaction: " + interactionSpecs["type"].Value +
                        " for data field " + interactionSpecs["field"].Value);
                } else
                {
                    throw new System.Exception("Make sure interaction object has type, field, and domain specs.");
                }

            }
        }

        private void AddToggleFilterInteraction(JSONObject interactionSpecs)
        {
            if(interactionsParentObject != null)
            {
                interactionsParentObject.GetComponent<Interactions>().AddToggleFilter(interactionSpecs);
            }            
        }

        private void ConstructLegends(JSONNode specs)
        {
            // Go through each channel and create legend for color, shape, or size channels:
            for (int channelIndex = 0; channelIndex < channelEncodings.Count; channelIndex++)
            {
                ChannelEncoding channelEncoding = channelEncodings[channelIndex];
                JSONNode legendSpecs = specs["encoding"][channelEncoding.channel]["legend"];
                if (legendSpecs != null && legendSpecs.Value.ToString() != "none" && channelEncoding.channel == "color")
                {
                    if (verbose)
                    {
                        Debug.Log("Constructing legend for channel " + channelEncoding.channel);
                    }

                    ConstructLegendObject(legendSpecs, ref channelEncoding);
                }
            }
        }

        private void ConstructLegendObject(JSONNode legendSpecs, ref ChannelEncoding channelEncoding)
        {
            GameObject legendPrefab = Resources.Load("Legend/Legend", typeof(GameObject)) as GameObject;
            if (legendPrefab != null && markPrefab != null)
            {
                channelEncoding.legend = Instantiate(legendPrefab, guidesParentObject.transform);
                channelEncoding.legend.GetComponent<Legend>().UpdateSpecs(legendSpecs, ref channelEncoding, markPrefab);
            }
            else
            {
                throw new Exception("Cannot find legend prefab.");
            }
        }

        private void ConstructAxes(JSONNode specs)
        {
            // Go through each channel and create axis for each spatial / position channel:
            for (int channelIndex = 0; channelIndex < channelEncodings.Count; channelIndex++)
            {
                ChannelEncoding channelEncoding = channelEncodings[channelIndex];
                JSONNode axisSpecs = specs["encoding"][channelEncoding.channel]["axis"];
                if (axisSpecs != null && axisSpecs.Value.ToString() != "none" &&
                    (channelEncoding.channel == "x" || channelEncoding.channel == "y" ||
                    channelEncoding.channel == "z"))
                {
                    if (verbose)
                    {
                        Debug.Log("Constructing axis for channel " + channelEncoding.channel);
                    }

                    ConstructAxisObject(axisSpecs, ref channelEncoding);
                }
            }
        }

        private void ConstructAxisObject(JSONNode axisSpecs, ref ChannelEncoding channelEncoding)
        {
            GameObject axisPrefab = Resources.Load("Axis/Axis", typeof(GameObject)) as GameObject;
            if (axisPrefab != null)
            {
                channelEncoding.axis = Instantiate(axisPrefab, guidesParentObject.transform);
                channelEncoding.axis.GetComponent<Axis>().UpdateSpecs(axisSpecs, channelEncoding.scale);                
            }
            else
            {
                throw new Exception("Cannot find axis prefab.");
            }
        }

        private void ApplyChannelEncodings()
        {
            foreach(ChannelEncoding ch in channelEncodings)
            {
                ApplyChannelEncoding(ch, ref markInstances);
            }
        }

        private void ApplyChannelEncoding(ChannelEncoding channelEncoding,
            ref List<GameObject> markInstances)
        {
            for(int i = 0; i < markInstances.Count; i++)
            {
                Mark markComponent = markInstances[i].GetComponent<Mark>();
                if (markComponent == null)
                {
                    throw new Exception("Mark component not present in mark prefab.");
                }

                if (channelEncoding.value != DxR.Vis.UNDEFINED)
                {
                    markComponent.SetChannelValue(channelEncoding.channel, channelEncoding.value);
                }
                else
                {
                    string channelValue = channelEncoding.scale.ApplyScale(markComponent.datum[channelEncoding.field]);
                    markComponent.SetChannelValue(channelEncoding.channel, channelValue);
                }
            }
        }

        private void ConstructMarkInstances()
        {
            markInstances = new List<GameObject>();

            // Create one mark prefab instance for each data point:
            foreach (Dictionary<string, string> dataValue in data.values)
            {
                // Instantiate mark prefab, copying parentObject's transform:
                GameObject markInstance = InstantiateMark(markPrefab, marksParentObject.transform);

                // Copy datum in mark:
                markInstance.GetComponent<Mark>().datum = dataValue;

                // Assign tooltip:
                if(enableTooltip)
                {
                    markInstance.GetComponent<Mark>().InitTooltip(ref tooltip);
                }

                markInstances.Add(markInstance);
            }
        }

        internal List<string> GetDataFieldsListFromURL(string dataURL)
        {
            return parser.GetDataFieldsList(dataURL);
        }

        private GameObject InstantiateMark(GameObject markPrefab, Transform parentTransform)
        {
            return Instantiate(markPrefab, parentTransform.position,
                        parentTransform.rotation, parentTransform);
        }

        private void CreateChannelEncodingObjects(JSONNode specs)
        {
            channelEncodings = new List<ChannelEncoding>();

            // Go through each channel and create ChannelEncoding for each:
            foreach (KeyValuePair<string, JSONNode> kvp in specs["encoding"].AsObject)
            {
                ChannelEncoding channelEncoding = new ChannelEncoding();

                channelEncoding.channel = kvp.Key;
                JSONNode channelSpecs = kvp.Value;
                if (channelSpecs["value"] != null)
                {
                    channelEncoding.value = channelSpecs["value"].Value.ToString();

                    if (channelSpecs["type"] != null)
                    {
                        channelEncoding.valueDataType = channelSpecs["type"].Value.ToString();
                    }
                }
                else
                {
                    channelEncoding.field = channelSpecs["field"];

                    if (channelSpecs["type"] != null)
                    {
                        channelEncoding.fieldDataType = channelSpecs["type"];
                    }
                    else
                    {
                        throw new Exception("Missing type for field in channel " + channelEncoding.channel);
                    }
                }

                JSONNode scaleSpecs = channelSpecs["scale"];
                if (scaleSpecs != null)
                {
                    CreateScaleObject(scaleSpecs, ref channelEncoding.scale);
                }

                channelEncodings.Add(channelEncoding);
            }
        }

        private void CreateScaleObject(JSONNode scaleSpecs, ref Scale scale)
        {
            switch (scaleSpecs["type"].Value.ToString())
            {
                case "none":
                case "custom":
                    scale = new ScaleCustom(scaleSpecs);
                    break;

                case "linear":
                    scale = new ScaleLinear(scaleSpecs);
                    break;

                case "band":
                case "point":
                    scale = new ScaleBand(scaleSpecs);
                    break;

                case "ordinal":
                    scale = new ScaleOrdinal(scaleSpecs);
                    break;

                case "sequential":
                    scale = new ScaleSequential(scaleSpecs);
                    break;

                default:
                    scale = null;
                    break;
            }
        }

        private void InferVisSpecs()
        {
            if (markPrefab != null)
            {
                markPrefab.GetComponent<Mark>().Infer(data, visSpecs, out visSpecsInferred, visSpecsURL);

                if(enableSpecsExpansion)
                {
                    JSONNode visSpecsToWrite = JSON.Parse(visSpecsInferred.ToString());
                    if (visSpecs["data"]["url"] != null && visSpecs["data"]["url"] != "inline")
                    {
                        visSpecsToWrite["data"].Remove("values");
                    }
                    System.IO.File.WriteAllText(Parser.GetFullSpecsPath(visSpecsURL), visSpecsToWrite.ToString(2));
                }
            }
            else
            {
                throw new Exception("Cannot perform inferrence without mark prefab loaded.");
            }
        }

        private void UpdateMarkPrefab()
        {
            string markType = visSpecs["mark"].Value;
            markPrefab = LoadMarkPrefab(markType);
        }

        private GameObject LoadMarkPrefab(string markName)
        {
            string markNameLowerCase = markName.ToLower();
            GameObject markPrefabResult = Resources.Load("Marks/" + markNameLowerCase + "/" + markNameLowerCase) as GameObject;

            if (markPrefabResult == null)
            {
                throw new Exception("Cannot load mark " + markNameLowerCase);
            }
            else if (verbose)
            {
                Debug.Log("Loaded mark " + markNameLowerCase);
            }

            // If the prefab does not have a Mark script attached to it, attach the default base Mark script object, i.e., core mark.
            if (markPrefabResult.GetComponent<Mark>() == null)
            {
                DxR.Mark markComponent = markPrefabResult.AddComponent(typeof(DxR.Mark)) as DxR.Mark;
            }
            markPrefabResult.GetComponent<Mark>().markName = markNameLowerCase;

            return markPrefabResult;
        }

        internal List<string> GetDataFieldsListFromValues(JSONNode valuesSpecs)
        {
            return parser.GetDataFieldsListFromValues(valuesSpecs);
        }

        private void UpdateVisData()
        {
            if(visSpecs["data"]["url"] != "inline")
            {
                visSpecs["data"].Add("values", parser.CreateValuesSpecs(visSpecs["data"]["url"]));
            }
            
            JSONNode valuesSpecs = visSpecs["data"]["values"];

            Debug.Log("Data update " + visSpecs["data"]["values"].ToString());

            data = new Data();

            CreateDataFields(valuesSpecs, ref data);

            data.values = new List<Dictionary<string, string>>();

            int numDataFields = data.fieldNames.Count;
            if (verbose)
            {
                Debug.Log("Counted " + numDataFields.ToString() + " fields in data.");
            }

            // Loop through the values in the specification
            // and insert one Dictionary entry in the values list for each.
            foreach (JSONNode value in valuesSpecs.Children)
            {
                Dictionary<string, string> d = new Dictionary<string, string>();

                bool valueHasNullField = false;
                for (int fieldIndex = 0; fieldIndex < numDataFields; fieldIndex++)
                {
                    string curFieldName = data.fieldNames[fieldIndex];

                    // TODO: Handle null / missing values properly.
                    if (value[curFieldName].IsNull)
                    {
                        valueHasNullField = true;
                        Debug.Log("value null found: ");
                        break;
                    }
                   
                    d.Add(curFieldName, value[curFieldName]);
                }

                if (!valueHasNullField)
                {
                    data.values.Add(d);
                }
            }

            //            SubsampleData(valuesSpecs, 8, "Assets/DxR/Resources/cars_subsampled.json");
        }

        private void SubsampleData(JSONNode data, int samplingRate, string outputName)
        {
            JSONArray output = new JSONArray();
            int counter = 0;
            foreach (JSONNode value in data.Children)
            {
                if (counter % 8 == 0)
                {
                    output.Add(value);
                }
                counter++;
            }

            System.IO.File.WriteAllText(outputName, output.ToString());
        }

        private void CreateDataFields(JSONNode valuesSpecs, ref Data data)
        {
            data.fieldNames = new List<string>();
            foreach (KeyValuePair<string, JSONNode> kvp in valuesSpecs[0].AsObject)
            {
                data.fieldNames.Add(kvp.Key);

                if (verbose)
                {
                    Debug.Log("Reading data field: " + kvp.Key);
                }
            }
        }

        private void UpdateVisConfig()
        {
            if (visSpecs["title"] != null)
            {
                title = visSpecs["title"].Value;
            }

            if (visSpecs["width"] == null)
            {
                visSpecs.Add("width", new JSONNumber(DEFAULT_VIS_DIMS));
                width = visSpecs["width"].AsFloat;
            } else
            {
                width = visSpecs["width"].AsFloat;
            }

            if (visSpecs["height"] == null)
            {
                visSpecs.Add("height", new JSONNumber(DEFAULT_VIS_DIMS));
                height = visSpecs["height"].AsFloat;
            }
            else
            {
                height = visSpecs["height"].AsFloat;
            }

            if (visSpecs["depth"] == null)
            {
                visSpecs.Add("depth", new JSONNumber(DEFAULT_VIS_DIMS));
                depth = visSpecs["depth"].AsFloat;
            }
            else
            {
                depth = visSpecs["depth"].AsFloat;
            }
        }

        private void DeleteAll()
        {
            foreach (Transform child in guidesParentObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (Transform child in marksParentObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        private void InitGUI()
        {
            Transform guiTransform = parentObject.transform.Find("DxRGUI");
            GameObject guiObject = guiTransform.gameObject;
            gui = guiObject.GetComponent<GUI>();
            gui.Init(this);

            if (!enableGUI && guiObject != null)
            {
                guiObject.SetActive(false);
            }
        }

        private void UpdateGUISpecsFromVisSpecs()
        {
            gui.UpdateGUISpecsFromVisSpecs();
        }

        public void UpdateVisSpecsFromGUISpecs()
        {
            // For now, just reset the vis specs to empty and
            // copy the contents of the text to vis specs; starting
            // everything from scratch. Later on, the new specs will have
            // to be compared with the current specs to get a list of what 
            // needs to be updated and only this list will be acted on.

            JSONNode guiSpecs = JSON.Parse(gui.GetGUIVisSpecs().ToString());


            // Remove data values so that parsing can put them again. 
            // TODO: Optimize this.
            if (guiSpecs["data"]["url"] != null)
            {
                if(guiSpecs["data"]["url"] != "inline")
                {
                    guiSpecs["data"].Remove("values");
                    visSpecs["data"].Remove("values");

                    visSpecs["data"]["url"] = guiSpecs["data"]["url"];
                }
            }

            visSpecs["mark"] = guiSpecs["mark"];

            Debug.Log("GUI SPECS: " + guiSpecs.ToString());

            // Go through vis specs and UPDATE fields and types of non-value channels
            // that are in the gui specs.
            List<string> channelsToUpdate = new List<string>();
            foreach (KeyValuePair<string, JSONNode> kvp in visSpecs["encoding"].AsObject)
            {
                string channelName = kvp.Key;
                if(visSpecs["encoding"][channelName]["value"] == null && guiSpecs["encoding"][channelName] != null)
                {
                    channelsToUpdate.Add(channelName);
                }
            }

            foreach(string channelName in channelsToUpdate)
            {
                visSpecs["encoding"][channelName]["field"] = guiSpecs["encoding"][channelName]["field"];
                visSpecs["encoding"][channelName]["type"] = guiSpecs["encoding"][channelName]["type"];
            }

            // Go through vis specs and DELETE non-field channels that are not in gui specs.
            List<string> channelsToDelete = new List<string>();
            foreach (KeyValuePair<string, JSONNode> kvp in visSpecs["encoding"].AsObject)
            {
                string channelName = kvp.Key;
                if (visSpecs["encoding"][channelName]["value"] == null && guiSpecs["encoding"][channelName] == null)
                {
                    channelsToDelete.Add(channelName);
                }
            }

            foreach (string channelName in channelsToDelete)
            {
                visSpecs["encoding"].Remove(channelName);
            }

            // Go through gui specs and ADD non-field channels in gui specs that are not in vis specs.
            foreach (KeyValuePair<string, JSONNode> kvp in guiSpecs["encoding"].AsObject)
            {
                string channelName = kvp.Key;
                Debug.Log("Testing channel " + channelName);
                
                if (guiSpecs["encoding"][channelName]["value"] == null && visSpecs["encoding"][channelName] == null)
                {
                    Debug.Log("Adding channel " + channelName);
                    visSpecs["encoding"].Add(channelName, guiSpecs["encoding"][channelName]);
                }
            }

            UpdateTextSpecsFromVisSpecs();
            UpdateVis();
        }
        
        public List<string> GetDataList()
        {
            string[] dirs = Directory.GetFiles(guiDataRootPath);
            List<string> dataList = new List<string>();
            dataList.Add(DxR.Vis.UNDEFINED);
            dataList.Add("inline");
            for (int i = 0; i < dirs.Length; i++)
            {
                if (Path.GetExtension(dirs[i]) != ".meta")
                {
                    dataList.Add(Path.GetFileName(dirs[i]));
                }
            }
            return dataList;
        }

        public List<string> GetMarksList()
        {
            string[] dirs = Directory.GetDirectories(guiMarksRootPath);
            List<string> marksList = new List<string>();
            marksList.Add(DxR.Vis.UNDEFINED);
            for (int i = 0; i < dirs.Length; i++)
            {
                marksList.Add(Path.GetFileName(dirs[i]));
            }
            return marksList;
        }

        public void UpdateVisSpecsFromTextSpecs()
        {
            // For now, just reset the vis specs to empty and
            // copy the contents of the text to vis specs; starting
            // everything from scratch. Later on, the new specs will have
            // to be compared with the current specs to get a list of what 
            // needs to be updated and only this list will be acted on.

            JSONNode textSpecs;
            parser.Parse(visSpecsURL, out textSpecs);

            visSpecs = textSpecs;

            gui.UpdateGUISpecsFromVisSpecs();
            UpdateVis();
        }

        public void UpdateTextSpecsFromVisSpecs()
        {
            JSONNode visSpecsToWrite = JSON.Parse(visSpecs.ToString());
            if(visSpecs["data"]["url"] != null && visSpecs["data"]["url"] != "inline")
            {
                visSpecsToWrite["data"].Remove("values");
            }
            System.IO.File.WriteAllText(Parser.GetFullSpecsPath(visSpecsURL), visSpecsToWrite.ToString(2));
        }

        public List<string> GetChannelsList(string markName)
        {
            GameObject markObject = LoadMarkPrefab(markName);
            return markObject.GetComponent<Mark>().GetChannelsList();
        }

        public void Rescale(float scaleFactor)
        {
            viewParentObject.transform.localScale = Vector3.Scale(viewParentObject.transform.localScale, 
                new Vector3(scaleFactor, scaleFactor, scaleFactor));
        }

        public void ResetView()
        {
            viewParentObject.transform.localScale = new Vector3(1, 1, 1);
            viewParentObject.transform.localEulerAngles = new Vector3(0, 0, 0);
            viewParentObject.transform.localPosition = new Vector3(0, 0, 0);
        }

        public void RotateAroundCenter(Vector3 rotationAxis, float angleDegrees)
        {
            Vector3 center = viewParentObject.transform.parent.transform.position + 
                new Vector3(width * SIZE_UNIT_SCALE_FACTOR / 2.0f, height * SIZE_UNIT_SCALE_FACTOR / 2.0f, 
                depth * SIZE_UNIT_SCALE_FACTOR / 2.0f);
            viewParentObject.transform.RotateAround(center, rotationAxis, angleDegrees);
        }

        // Update the visibility of each mark according to the filters results:
        internal void FiltersUpdated()
        {
            if(interactionsParentObject != null)
            {
                ShowAllMarks();

                foreach (KeyValuePair<string,List<bool>> filterResult in interactionsParentObject.GetComponent<Interactions>().filterResults)
                {
                    List<bool> visib = filterResult.Value;
                    for (int m = 0; m < markInstances.Count; m++)
                    {
                        markInstances[m].SetActive(visib[m] && markInstances[m].activeSelf);
                    }
                }
            }
        }

        void ShowAllMarks()
        {
            for (int m = 0; m < markInstances.Count; m++)
            {
                markInstances[m].SetActive(true);
            }
        }
    }

}
