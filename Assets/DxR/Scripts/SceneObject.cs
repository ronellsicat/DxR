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

        public string specsFilename = "DxRData/example.json";
        public JSONNode sceneSpecs;

        public string sceneName;    // Name of scene.
        public string title;        // Title of scene displayed.
        public float width;         // Width of scene in millimeters.
        public float height;        // Heigh of scene in millimeters.
        public float depth;         // Depth of scene in millimeters.

        public Data data;           // Data object.
        public string markType;     // Type or name of mark used in scene.
        
        private GameObject sceneRoot = null;
        private GameObject markPrefab = null;
        private List<ChannelEncoding> channelEncodings = null;

        void Start()
        {
            sceneRoot = gameObject;

            Parse(specsFilename, out sceneSpecs);

            Infer(ref sceneSpecs);
            
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

        // Infer (raw JSON specs -> full JSON specs): 
        // automatically fill in missing specs by inferrence (informed by marks and data type).
        private void Infer(ref JSONNode sceneSpecs)
        {

        }

        // Construct (full JSON specs -> working SceneObject): 
        private void Construct(JSONNode sceneSpecs, ref GameObject sceneRoot)
        {
            InitSceneObjectProperties(sceneSpecs, ref sceneRoot);

            CreateDataObjectFromValues(sceneSpecs["data"]["values"], out data);

            CreateMarkObject(sceneSpecs["mark"].Value.ToString(), out markPrefab);

            CreateChannelEncodingObjects(sceneSpecs, out channelEncodings);

            ConstructMarks(sceneRoot);

            ConstructAxes(sceneSpecs, ref channelEncodings, ref sceneRoot);
        }

        private void InitSceneObjectProperties(JSONNode sceneSpecs, ref GameObject sceneRoot)
        {
            if (sceneSpecs["name"] != null)
            {
                sceneName = sceneSpecs["name"].Value;
            }

            if (sceneSpecs["title"] != null)
            {
                title = sceneSpecs["title"].Value;
            }

            if(sceneSpecs["width"] != null)
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

                    d.Add(curFieldName, value[curFieldName]);
                }

                if (!valueHasNullField)
                {
                    data.values.Add(d);
                }
            }
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
                case "custom":
                    scale = new ScaleCustom(scaleSpecs);
                    break;

                case "linear":
                    scale = new ScaleLinear(scaleSpecs);
                    break;

                case "band":
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
                    //Debug.Log("Mapping channel: " + channelParam.channelType +
                    //    ", value: " + dataValue[channelParam.fieldName]); 

                    string channelValue = channelEncoding.scale.ApplyScale(dataValue[channelEncoding.field]);
                    markComponent.SetChannelValue(channelEncoding.channel, channelValue);
                }
            }
        }

        private void ConstructAxes(JSONNode sceneSpecs, ref List<ChannelEncoding> channelEncodings, ref GameObject sceneRoot)
        {
            // Go through each channel and create axis for each:
            for (int channelIndex = 0; channelIndex < channelEncodings.Count; channelIndex++)
            {
                ChannelEncoding channelEncoding = channelEncodings[channelIndex];
                JSONNode axisSpecs = sceneSpecs["encoding"][channelEncoding.channel]["axis"];
                if (axisSpecs != null && (channelEncoding.channel == "x" ||
                    channelEncoding.channel == "y" || channelEncoding.channel == "z"))
                {
                    if (verbose)
                    {
                        Debug.Log("Constructing axis for channel " + channelEncoding.channel);
                    }

                    ConstructAxisObject(axisSpecs, ref channelEncoding, ref sceneRoot);
                }
            }
        }

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

                float axisLength = 0.0f;
                if (axisSpecs["length"] != null)
                {
                    axisLength = axisSpecs["length"].AsFloat;
                }
                else
                {
                    switch (channelEncoding.channel)
                    {
                        case "x":
                            axisLength = width;
                            break;
                        case "y":
                            axisLength = height;
                            break;
                        case "z":
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

                // TODO: Do the axis color coding more elegantly.  
                // Experimental: Set color of axis based on channel type.
                channelEncoding.axis.GetComponent<Axis>().EnableAxisColorCoding(channelEncoding.channel);
            }
            else
            {
                throw new Exception("Cannot find axis prefab.");
            }
        }
    }
}

