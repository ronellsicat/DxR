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

        public string sceneName;    // Name of scene used to name parent GameObject.
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

        private void CreateMarkObject(string markType, out GameObject markPrefab)
        {
            string markNameLowerCase = markType.ToLower();
            markPrefab = Resources.Load("Marks/" + markNameLowerCase + "/" + markNameLowerCase) as GameObject;

            if(markPrefab == null)
            {
                throw new Exception("Cannot load mark " + markNameLowerCase);
            } else if(verbose)
            {
                Debug.Log("Loaded mark " + markNameLowerCase);
            }
        }

        // Construct (full JSON specs -> working SceneObject): 
        private void Construct(JSONNode sceneSpecs, ref GameObject sceneRoot)
        {
            CreateDataObjectFromValues(sceneSpecs["data"]["values"], out data);

            CreateMarkObject(sceneSpecs["mark"].Value.ToString(), out markPrefab);

            CreateChannelEncodingObjects(sceneSpecs, out channelEncodings);

            ConstructMarks(sceneRoot);
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
                        Quaternion.identity, parentTransform);
        }

        private void ApplyChannelEncoding(List<ChannelEncoding> channelEncodings, 
            Dictionary<string, string> dataValue, ref GameObject markInstance)
        {
            Mark markComponent = markInstance.GetComponent<Mark>();

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

                    if(channelSpecs["type"] != null)
                    {
                        channelEncoding.valueDataType = channelSpecs["type"].Value.ToString();
                    } else
                    {
                        throw new Exception("Missing type for value in channel " + channelEncoding.channel);
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

                JSONNode axisSpecs = channelSpecs["axis"];
                if (axisSpecs != null)
                {
                    //CreateAxisObject(axisSpecs, ref channelEncoding.axis);
                }

                // TODO: Add legend object.
                JSONNode legendSpecs = channelSpecs["legend"];
                if (legendSpecs != null)
                {
                    //CreateLegendObject(axisSpecs, ref channelEncoding.legend);
                }

                channelEncodings.Add(channelEncoding);
            }
        }

        private void CreateScaleObject(JSONNode scaleSpecs, ref Scale scale)
        {
            switch (scaleSpecs["type"].Value.ToString())
            {
                case "linear":
                    scale = new ScaleLinear(scaleSpecs);
                    break;

                case "band":
                    //scale = new ScaleBand(scaleSpecs);
                    break;
                default:
                    scale = null;
                    break;
            }
        }

        private void CreateDataObjectFromValues(JSONNode valuesSpecs, out Data data)
        {
            data = new Data();

            CreateDataFields(valuesSpecs, ref data);

            data.values = new List<Dictionary<string, string>>();

            int numDataFields = data.fieldNames.Count;
            if(verbose)
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

                if(verbose)
                {
                    Debug.Log("Reading data field: " + kvp.Key);
                }
            }
        }

    }
}

