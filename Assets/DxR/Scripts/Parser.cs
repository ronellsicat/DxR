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
        static string specsBaseDir = "/DxRSpecs/";
        static string dataBaseDir = "/DxRData/";
        private bool verbose = false;           // Set to true to display debugging info.

        /// <summary>
        ///  Read specifications in JSON file specified by specsFilename as 
        ///  well as data file (if needed) and expand to a JSONNode scene specs with the 
        ///  data represented as a JSON object.
        /// </summary>
        public void Parse(string specsFilename, out JSONNode visSpecs)
        {
            visSpecs = JSON.Parse(GetStringFromFile(GetFullSpecsPath(specsFilename)));

            ExpandDataSpecs(ref visSpecs);
        }

        private void ExpandDataSpecs(ref JSONNode visSpecs)
        {
            if (visSpecs["data"]["url"] != null)
            {
                if(visSpecs["data"]["url"].Value == "inline")
                {
                    return;
                }

                string dataFilename = GetFullDataPath(visSpecs["data"]["url"]);

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

        public static string GetStringFromFile(string filename)
        {
            return File.ReadAllText(filename);
        }

        public static string GetFullSpecsPath(string filename)
        {
            return Application.streamingAssetsPath + specsBaseDir + filename;
        }

        public static string GetFullDataPath(string filename)
        {
            return Application.streamingAssetsPath + dataBaseDir + filename;
        }
    }
}

