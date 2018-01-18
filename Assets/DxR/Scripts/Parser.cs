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

            // If the specs file is empty, provide the boiler plate data and marks specs.
            if(visSpecs == null)
            {
                CreateEmptyTemplateSpecs(specsFilename, ref visSpecs);
            }

            ExpandDataSpecs(ref visSpecs);
        }

        private void CreateEmptyTemplateSpecs(string specsFilename, ref JSONNode visSpecs)
        {
            JSONNode emptySpecs = new JSONObject();
            JSONNode dataSpecs = new JSONObject();
            dataSpecs.Add("url", new JSONString(DxR.Vis.UNDEFINED));
            emptySpecs.Add("data", dataSpecs);
            emptySpecs.Add("mark", new JSONString(DxR.Vis.UNDEFINED));

            visSpecs = emptySpecs;

            System.IO.File.WriteAllText(GetFullSpecsPath(specsFilename), emptySpecs.ToString(2));
        }

        private void ExpandDataSpecs(ref JSONNode visSpecs)
        {
            if (visSpecs["data"].Value == DxR.Vis.UNDEFINED || visSpecs["data"]["url"].Value == DxR.Vis.UNDEFINED) return;

            if (visSpecs["data"]["url"] != null)
            {
                if(visSpecs["data"]["url"].Value == "inline")
                {
                    return;
                }

                visSpecs["data"].Add("values", CreateValuesSpecs(visSpecs["data"]["url"]));
            } else if(visSpecs["data"]["values"] != null)
            {
                visSpecs["data"].Add("url", new JSONString("inline"));
            }

            // TODO: Do some checks.
        }

        public JSONNode CreateValuesSpecs(string dataURL)
        {
            string dataFilename = GetFullDataPath(dataURL);
            string ext = Path.GetExtension(dataFilename);
            if (ext == ".json")
            {
                JSONNode valuesJSONNode = JSON.Parse(GetStringFromFile(dataFilename));
                return valuesJSONNode;
            }
            else if (ext == ".csv")
            {
                JSONNode valuesJSONNode = JSON.ParseCSV(GetStringFromFile(dataFilename));
                return valuesJSONNode;
            }
            else
            {
                throw new Exception("Cannot load file type" + ext);
            }
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

        internal List<string> GetDataFieldsList(string dataURL)
        {
            List<string> fieldNames = new List<string>() { DxR.Vis.UNDEFINED };
            JSONNode dataSpecs = new JSONObject();
            string filename = GetFullDataPath(dataURL);

            string ext = Path.GetExtension(filename);
            if (ext == ".json")
            {
                JSONNode valuesJSONNode = JSON.Parse(GetStringFromFile(filename));
                dataSpecs.Add("values", valuesJSONNode);
            }
            else if (ext == ".csv")
            {
                JSONNode valuesJSONNode = JSON.ParseCSV(GetStringFromFile(filename));
                dataSpecs.Add("values", valuesJSONNode);
            }
            
            foreach (KeyValuePair<string, JSONNode> kvp in dataSpecs["values"][0].AsObject)
            {
                fieldNames.Add(kvp.Key);
            }

            return fieldNames;
        }

        internal List<string> GetDataFieldsListFromValues(JSONNode valuesSpecs)
        {
            List<string> fieldNames = new List<string>() { DxR.Vis.UNDEFINED };
            foreach (KeyValuePair<string, JSONNode> kvp in valuesSpecs[0].AsObject)
            {
                fieldNames.Add(kvp.Key);
            }

            return fieldNames;
        }
    }
}

