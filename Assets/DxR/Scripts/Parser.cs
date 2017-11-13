using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using System.IO;

namespace DxR
{
    public class Parser
    {
        private bool verbose = false;           // Set to true to display debugging info.

        /// <summary>
        ///  Read specifications in JSON file specified by specsFilename as 
        ///  well as data file (if needed) and expand to a JSONNode scene specs with the 
        ///  data represented as a JSON object.
        /// </summary>
        public void Parse(string specsFilename, out JSONNode sceneSpecs)
        {
           sceneSpecs = JSON.Parse(GetStringFromFile(specsFilename));

           ExpandDataSpecs(ref sceneSpecs);
        }

        private void ExpandDataSpecs(ref JSONNode sceneSpecs)
        {
            if (sceneSpecs["data"]["url"] != null)
            {
                string dataFilename = sceneSpecs["data"]["url"];
                JSONNode valuesJSONNode = JSON.Parse(GetStringFromFile(dataFilename));
                sceneSpecs["data"].AsObject.Add("values", valuesJSONNode);
            }

            // TODO: Do some checks.
        }

        private string GetStringFromFile(string filename)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, filename);
            return File.ReadAllText(filePath);
        }
    }
}

