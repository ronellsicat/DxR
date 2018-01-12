using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace DxR
{
    /// <summary>
    /// This is the component that needs to be attached to a GameObject (root) in order 
    /// to create a data-driven scene. This component takes in a json file, parses its
    /// encoded specification and generates scene parameters for one or more scenes.
    /// A scene is defined as a visualization with ONE type of mark.  
    /// Each scene gets its own scene root (sceneRoot) GameObject that gets created 
    /// under the root GameObject on which this component is attached to.
     /// </summary>
    public class SceneObject : MonoBehaviour
    {
        private bool verbose = true;
        public static string UNDEFINED = "undefined";
        public static float SIZE_UNIT_SCALE_FACTOR = 1.0f / 1000.0f;    // Each unit in the specs is 1 mm.
        public static float DEFAULT_VIS_DIMS = 500.0f;

        public string specsFilename = "DxRData/example.json";
        public JSONNode sceneSpecs;
        public bool enableGUI = true;

        string sceneName;    // Name of scene.
        string title;        // Title of scene displayed.
        float width;         // Width of scene in millimeters.
        float height;        // Heigh of scene in millimeters.
        float depth;         // Depth of scene in millimeters.

        public Data data;           // Data object.
        string markType;     // Type or name of mark used in scene.
        
        private GameObject sceneRoot = null;
        private GameObject markPrefab = null;
        private List<ChannelEncoding> channelEncodings = null;

        private GameObject tooltipInstance = null;

        private bool distanceVisibility = true;
        private bool gazeVisibility = true;
        private bool currentVisibility = true;

        void Start()
        {
            sceneRoot = gameObject;

            Parse(specsFilename, out sceneSpecs);
            
            Initialize(ref sceneSpecs);

            Infer(data, ref sceneSpecs);
            
            Construct(sceneSpecs, ref sceneRoot);
        }
        
        // Parse (JSON spec file (data file info in specs) -> expanded raw JSON specs): 
        // Read in the specs and data files to create expanded raw JSON specs.
        // Filenames should be relative to Assets/StreamingAssets/ directory.
        private void Parse(string specsFilename, out JSONNode sceneSpecs)
        {
            Parser parser = new Parser();
            parser.Parse(specsFilename, out sceneSpecs);
        }

        // Create initial objects that are required for inferrence.
        // The sceneSpecs should provide minimum required specs.
        private void Initialize(ref JSONNode sceneSpecs)
        {
            InferSceneObjectProperties(ref sceneSpecs);

            UpdateSceneObjectProperties(sceneSpecs);

            CreateDataObjectFromValues(sceneSpecs["data"]["values"], out data);

            CreateTooltipObject(out tooltipInstance, ref sceneRoot);

            CreateMarkObject(sceneSpecs["mark"].Value.ToString(), out markPrefab);
        }

        private void InferSceneObjectProperties(ref JSONNode sceneSpecs)
        {
            if (sceneSpecs["width"] == null)
            {
                sceneSpecs.Add("width", new JSONNumber(DEFAULT_VIS_DIMS));
            }

            if (sceneSpecs["height"] == null)
            {
                sceneSpecs.Add("height", new JSONNumber(DEFAULT_VIS_DIMS));
            }

            if (sceneSpecs["depth"] == null)
            {
                sceneSpecs.Add("depth", new JSONNumber(DEFAULT_VIS_DIMS));
            }
        }

        public void TestUI()
        {
            Debug.Log("Test ui");
        }

        // Infer (raw JSON specs -> full JSON specs): 
        // automatically fill in missing specs by inferrence (informed by marks and data type).
        private void Infer(Data data, ref JSONNode sceneSpecs)
        {
            InferAnchorProperties(ref sceneSpecs);

            if (markPrefab != null)
            {
                markPrefab.GetComponent<Mark>().Infer(data, ref sceneSpecs, specsFilename);
            } else
            {
                throw new Exception("Cannot perform inferrence without mark prefab loaded.");
            }

            // Update properties if needed - some properties, e.g., width, height, depth
            // may get changed based on inferrence.
            UpdateSceneObjectProperties(sceneSpecs);
        }

        private void InferAnchorProperties(ref JSONNode sceneSpecs)
        {
            JSONNode anchorSpecs = sceneSpecs["anchor"];
            if (anchorSpecs != null && anchorSpecs.Value.ToString() == "none") return;
            JSONObject anchorSpecsObj = (anchorSpecs == null) ? new JSONObject() : anchorSpecs.AsObject;
            
            if (anchorSpecsObj["placement"] == null)
            {
                anchorSpecsObj.Add("placement", new JSONString("tapToPlace"));
            }

            if(anchorSpecsObj["visibility"] == null)
            {
                anchorSpecsObj.Add("visibility", new JSONString("always"));
            }

            sceneSpecs.Add("anchor", anchorSpecsObj);
        }

        // Construct (full JSON specs -> working SceneObject): 
        private void Construct(JSONNode sceneSpecs, ref GameObject sceneRoot)
        {
            CreateChannelEncodingObjects(sceneSpecs, out channelEncodings);

            ConstructMarks(sceneRoot);

            ConstructAxes(sceneSpecs, ref channelEncodings, ref sceneRoot);

            ConstructLegends(sceneSpecs, ref channelEncodings, ref sceneRoot);

            ConstructAnchor(sceneSpecs, ref sceneRoot);

            ConstructPortals(sceneSpecs, ref sceneRoot);
        }

        private void ConstructPortals(JSONNode sceneSpecs, ref GameObject sceneRoot)
        {
            if (sceneSpecs["portals"] == null) return;

            JSONArray values = (sceneSpecs["portals"]["values"] == null) ? new JSONArray() :
                sceneSpecs["portals"]["values"].AsArray;

            if(sceneSpecs["portals"]["scheme"] != null)
            {
                // TODO: Load scheme contents (in local file Assets/DxR/Resources/PortalSchemes/ into values array.
            }

            GameObject portalPrefab = Resources.Load("Portal/Portal", typeof(GameObject)) as GameObject;
            if (portalPrefab == null)
            {
                throw new Exception("Cannot load Portal prefab from Assets/DxR/Resources/Portal/Portal.prefab");
            }
            else if (verbose)
            {
                Debug.Log("Loaded portal prefab");
            }

            foreach (JSONNode portalSpec in values)
            {
                Debug.Log("Portal spec: " + portalSpec.ToString());

                ConstructPortal(portalSpec, portalPrefab, ref sceneRoot);
            }
        }

        private void ConstructPortal(JSONNode portalSpec, GameObject portalPrefab, ref GameObject parent)
        {
            GameObject portalInstance = Instantiate(portalPrefab, parent.transform.position,
                        parent.transform.rotation, parent.transform);

            Vector3 localPos = Vector3.zero;
            Vector3 localRot = Vector3.zero;

            if(portalSpec["x"] != null)
            {
                localPos.x = portalSpec["x"].AsFloat * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            }

            if (portalSpec["y"] != null)
            {
                localPos.y = portalSpec["y"].AsFloat * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            }

            if (portalSpec["z"] != null)
            {
                localPos.z = portalSpec["z"].AsFloat * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            }

            if (portalSpec["xrot"] != null)
            {
                localRot.x = portalSpec["xrot"].AsFloat;
            }

            if (portalSpec["yrot"] != null)
            {
                localRot.y = portalSpec["yrot"].AsFloat;
            }
            if (portalSpec["zrot"] != null)
            {
                localRot.z = portalSpec["zrot"].AsFloat;
            }

            portalInstance.transform.localPosition = localPos;
            portalInstance.transform.localEulerAngles = localRot;
        }

        private void ConstructAnchor(JSONNode sceneSpecs, ref GameObject sceneRoot)
        {
            if (sceneSpecs["anchor"] == null) return;

            Anchor anchor = sceneRoot.transform.GetComponentInChildren<Anchor>();
            if(anchor != null)
            {
                anchor.UpdateSpecs(sceneSpecs["anchor"]);
            }
        }

        private void CreateTooltipObject(out GameObject tooltipInstance, ref GameObject parent)
        {
            GameObject tooltipPrefab = Resources.Load("Tooltip/tooltip") as GameObject;
            tooltipInstance = Instantiate(tooltipPrefab, parent.transform.position,
                        parent.transform.rotation, parent.transform);

            if (tooltipInstance == null)
            {
                throw new Exception("Cannot load tooltip");
            }
            else if (verbose)
            {
                Debug.Log("Loaded tooltip");
            }

            tooltipInstance.name = "tooltip";
            tooltipInstance.GetComponent<Tooltip>().SetAnchor("upperleft");
            tooltipInstance.SetActive(false);            
        }

        private void UpdateSceneObjectProperties(JSONNode sceneSpecs)
        {
            if (sceneSpecs["name"] != null)
            {
                sceneName = sceneSpecs["name"].Value;
            }

            if (sceneSpecs["title"] != null)
            {
                title = sceneSpecs["title"].Value;
            }

            if (sceneSpecs["width"] != null)
            {
                width = sceneSpecs["width"].AsFloat;
            } 

            if (sceneSpecs["height"] != null)
            {
                height = sceneSpecs["height"].AsFloat;
            }

            if (sceneSpecs["depth"] != null)
            {
                depth = sceneSpecs["depth"].AsFloat;
            }
        }

        private void CreateDataObjectFromValues(JSONNode valuesSpecs, out Data data)
        {
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
                    /*
                    if(curFieldName == "vmeg" && value["vmeg"] == 0)
                    {
                        valueHasNullField = true;
                        Debug.Log("value null found: ");
                        break;
                    }
                    */

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

        private void CreateMarkObject(string markType, out GameObject markPrefab)
        {
            string markNameLowerCase = markType.ToLower();
            markPrefab = Resources.Load("Marks/" + markNameLowerCase + "/" + markNameLowerCase) as GameObject;

            if (markPrefab == null)
            {
                throw new Exception("Cannot load mark " + markNameLowerCase);
            }
            else if (verbose)
            {
                Debug.Log("Loaded mark " + markNameLowerCase);
            }

            markPrefab.GetComponent<Mark>().markName = markNameLowerCase;
        }

        private void CreateChannelEncodingObjects(JSONNode sceneSpecs, out List<ChannelEncoding> channelEncodings)
        {
            channelEncodings = new List<ChannelEncoding>();

            // Go through each channel and create ChannelEncoding for each:
            foreach (KeyValuePair<string, JSONNode> kvp in sceneSpecs["encoding"].AsObject)
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

        private void ConstructMarks(GameObject sceneRoot)
        {
            if(markPrefab != null)
            {
                // Create one mark prefab instance for each data point:
                foreach (Dictionary<string, string> dataValue in data.values)
                {
                    // Instantiate mark prefab
                    GameObject markInstance = InstantiateMark(markPrefab, sceneRoot.transform);

                    // Copy data in mark:
                    markInstance.GetComponent<Mark>().datum = dataValue;

                    // Apply channel encodings:
                    ApplyChannelEncoding(channelEncodings, dataValue, ref markInstance);
                }
            } else
            {
                throw new Exception("Error constructing marks with mark prefab not loaded.");
            }
        }

        private GameObject InstantiateMark(GameObject markPrefab, Transform parentTransform)
        {
            return Instantiate(markPrefab, parentTransform.position,
                        parentTransform.rotation, parentTransform);
        }

        private void ApplyChannelEncoding(List<ChannelEncoding> channelEncodings, 
            Dictionary<string, string> dataValue, ref GameObject markInstance)
        {
            Mark markComponent = markInstance.GetComponent<Mark>();
            if(markComponent == null)
            {
                throw new Exception("Mark component not present in mark prefab.");
            }

            foreach (ChannelEncoding channelEncoding in channelEncodings)
            {
                if (channelEncoding.value != DxR.SceneObject.UNDEFINED)
                {
                    markComponent.SetChannelValue(channelEncoding.channel, channelEncoding.value);
                }
                else
                {
                    if(channelEncoding.channel == "tooltip")
                    {
                        SetupTooltip(channelEncoding, markComponent);
                    } else
                    {
                        string channelValue = channelEncoding.scale.ApplyScale(dataValue[channelEncoding.field]);
                        markComponent.SetChannelValue(channelEncoding.channel, channelValue);
                    }
                }
            }
        }

        private void SetupTooltip(ChannelEncoding channelEncoding, Mark markComponent)
        {
            if (tooltipInstance != null)
            {
                markComponent.SetTooltipObject(ref tooltipInstance);
                markComponent.SetTooltipField(channelEncoding.field);
            }
        }

        private void ConstructAxes(JSONNode sceneSpecs, ref List<ChannelEncoding> channelEncodings, ref GameObject sceneRoot)
        {
            // Go through each channel and create axis for each spatial / position channel:
            for (int channelIndex = 0; channelIndex < channelEncodings.Count; channelIndex++)
            {
                ChannelEncoding channelEncoding = channelEncodings[channelIndex];
                JSONNode axisSpecs = sceneSpecs["encoding"][channelEncoding.channel]["axis"];
                if (axisSpecs != null && axisSpecs.Value.ToString() != "none" && 
                    (channelEncoding.channel == "x" || channelEncoding.channel == "y" || 
                    channelEncoding.channel == "z" || 
                    channelEncoding.channel == "width" || channelEncoding.channel == "height" ||
                    channelEncoding.channel == "depth"))
                {
                    if (verbose)
                    {
                        Debug.Log("Constructing axis for channel " + channelEncoding.channel);
                    }

                    ConstructAxisObject(axisSpecs, ref channelEncoding, ref sceneRoot);
                }
            }
        }

        // TODO: Move all this in axis object.
        private void ConstructAxisObject(JSONNode axisSpecs, ref ChannelEncoding channelEncoding, ref GameObject sceneRoot)
        {
            GameObject axisPrefab = Resources.Load("Axis/Axis", typeof(GameObject)) as GameObject;
            if (axisPrefab != null)
            {
                channelEncoding.axis = Instantiate(axisPrefab, sceneRoot.transform);

                // TODO: Move all the following update code to the Axis object class.

                if (axisSpecs["title"] != null)
                {
                    channelEncoding.axis.GetComponent<Axis>().SetTitle(axisSpecs["title"].Value);
                }

                if(axisSpecs["titlePadding"] != null)
                {
                    channelEncoding.axis.GetComponent<Axis>().SetTitlePadding(axisSpecs["titlePadding"].Value);
                }

                float axisLength = 0.0f;
                if (axisSpecs["length"] != null)
                {
                    axisLength = axisSpecs["length"].AsFloat;
                }
                else
                {
                    // TODO: Move this to infer stage.
                    switch (channelEncoding.channel)
                    {
                        case "x":
                        case "width":
                            axisLength = width;
                            break;
                        case "y":
                        case "height":
                            axisLength = height;
                            break;
                        case "z":
                        case "depth":
                            axisLength = depth;
                            break;
                        default:
                            axisLength = 0.0f;
                            break;
                    }
                    channelEncoding.axis.GetComponent<Axis>().SetLength(axisLength);
                }

                if(axisSpecs["orient"] != null && axisSpecs["face"] != null)
                {
                    channelEncoding.axis.GetComponent<Axis>().SetOrientation(axisSpecs["orient"].Value, axisSpecs["face"].Value);
                } else
                {
                    throw new Exception("Axis of channel " + channelEncoding.channel + " requires both orient and face specs.");
                }

                if(axisSpecs["ticks"].AsBool && axisSpecs["values"] != null)
                {
                    channelEncoding.axis.GetComponent<Axis>().ConstructTicks(axisSpecs, channelEncoding.scale);
                }

                // TODO: Do the axis color coding more elegantly.  
                // Experimental: Set color of axis based on channel type.
                channelEncoding.axis.GetComponent<Axis>().EnableAxisColorCoding(channelEncoding.channel);
            }
            else
            {
                throw new Exception("Cannot find axis prefab.");
            }
        }
        
        private void ConstructLegends(JSONNode sceneSpecs, ref List<ChannelEncoding> channelEncodings, ref GameObject sceneRoot)
        {
            // Go through each channel and create legend for color, shape, or size channels:
            for (int channelIndex = 0; channelIndex < channelEncodings.Count; channelIndex++)
            {
                ChannelEncoding channelEncoding = channelEncodings[channelIndex];
                JSONNode legendSpecs = sceneSpecs["encoding"][channelEncoding.channel]["legend"];
                if (legendSpecs != null && legendSpecs.Value.ToString() != "none")
                {
                    if (verbose)
                    {
                        Debug.Log("Constructing legend for channel " + channelEncoding.channel);
                    }

                    ConstructLegendObject(legendSpecs, ref channelEncoding, ref sceneRoot);
                }
            }
        }

        private void ConstructLegendObject(JSONNode legendSpecs, ref ChannelEncoding channelEncoding, ref GameObject sceneRoot)
        {
            GameObject legendPrefab = Resources.Load("Legend/Legend", typeof(GameObject)) as GameObject;
            if (legendPrefab != null && markPrefab != null)
            {
                channelEncoding.legend = Instantiate(legendPrefab, sceneRoot.transform);
                channelEncoding.legend.GetComponent<Legend>().UpdateSpecs(legendSpecs, ref channelEncoding, markPrefab);
            }
            else
            {
                throw new Exception("Cannot find legend prefab.");
            }
        }

        public void SetVisibility(bool val)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i).gameObject;
                if (child != null && child.name != "Anchor")
                {
                    child.SetActive(val);
                }
            }
        }

        public void SetDistanceVisibility(bool val)
        {
            distanceVisibility = val;
            if((distanceVisibility && gazeVisibility) != currentVisibility)
            {
                currentVisibility = distanceVisibility && gazeVisibility;
                SetVisibility(currentVisibility);
            }
        }

        public void SetGazeVisibility(bool val)
        {
            gazeVisibility = val;
            if ((distanceVisibility && gazeVisibility) != currentVisibility)
            {
                currentVisibility = distanceVisibility && gazeVisibility;
                SetVisibility(currentVisibility);
            }
        }
    }
}

