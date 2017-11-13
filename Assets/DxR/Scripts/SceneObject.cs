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
        public string specsFilename = "example.json";
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

            Parse(specsFilename, ref sceneSpecs);

            Infer(ref sceneSpecs);
            
            Construct(sceneSpecs, ref sceneRoot);
        }

        // Parse (JSON spec file (data file info in specs) -> expanded raw JSON specs): 
        // Read in the specs and data files to create expanded raw JSON specs.
        public void Parse(string specsFilename, ref JSONNode sceneSpecs)
        {
            Parser parser = new Parser();
            parser.Parse(specsFilename, ref sceneSpecs);
        }

        // Infer (raw JSON specs -> full JSON specs): 
        // automatically fill in missing specs by inferrence (informed by marks and data type).
        public void Infer(ref JSONNode sceneSpecs)
        {

        }
        
        // Construct (full JSON specs -> working SceneObject): 
        public void Construct(JSONNode sceneSpec, ref GameObject sceneRoot)
        {

        }
    }
}

