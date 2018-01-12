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
        public void Parse(string specsFilename, out JSONNode visSpecs)
        {
           visSpecs = JSON.Parse(GetStringFromFile(specsFilename));

           ExpandDataSpecs(ref visSpecs);
        }

        private void ExpandDataSpecs(ref JSONNode visSpecs)
        {
            if (visSpecs["data"]["url"] != null)
            {
                string dataFilename = visSpecs["data"]["url"];

                string ext = Path.GetExtension(dataFilename);
                if (ext == ".json")
                {
                    JSONNode valuesJSONNode = JSON.Parse(GetStringFromFile(dataFilename));
                    visSpecs["data"].AsObject.Add("values", valuesJSONNode);
                } else if(ext == ".csv")
                {
                    JSONNode valuesJSONNode = JSON.ParseCSV(GetStringFromFile(dataFilename));
                    visSpecs["data"].AsObject.Add("values", valuesJSONNode);
;               } else
                {
                    throw new Exception("Cannot load file type" + ext);
                }
                
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

