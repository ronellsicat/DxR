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
            CreateDataObjectFromValues(sceneSpecs["data"]["values"], out data);

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

