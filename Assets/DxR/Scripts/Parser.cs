using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using System.IO;
using System.Linq;

namespace DxR
{
    public class Parser
    {
        static string specsBaseDir = "/DxRSpecs/";
        static string dataBaseDir = "/DxRData/";

        /// <summary>
        ///  Read specifications in JSON file specified by specsFilename without any further changes.
        /// </summary>
        public void Parse(string specsFilename, out JSONNode visSpecs)
        {
            visSpecs = JSON.Parse(GetStringFromFile(GetFullSpecsPath(specsFilename)));

            // If the specs file is empty, provide the boiler plate data and marks specs.
            if (visSpecs == null || visSpecs.ToString() == "\"\"")
            {
                CreateEmptyTemplateSpecs(specsFilename, ref visSpecs);
            }
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

        /// <summary>
        /// Reads specifications from a JSON string specified by specs instead of a file.
        /// Otherwise functions identically to the Parse function
        /// </summary>
        public void ParseString(string specs, out JSONNode visSpecs)
        {
            visSpecs = JSON.Parse(specs);

            // If the specs file is empty, provide the boiler plate data and marks specs.
            if(visSpecs == null || visSpecs.ToString() == "\"\"")
            {
                CreateEmptyTemplateSpecs(ref visSpecs);
            }
        }

        private void CreateEmptyTemplateSpecs(ref JSONNode visSpecs)
        {
            JSONNode emptySpecs = new JSONObject();
            JSONNode dataSpecs = new JSONObject();
            dataSpecs.Add("url", new JSONString(DxR.Vis.UNDEFINED));
            emptySpecs.Add("data", dataSpecs);
            emptySpecs.Add("mark", new JSONString(DxR.Vis.UNDEFINED));

            visSpecs = emptySpecs;
        }

        /// <summary>
        /// Expands a given vis specs to include its data values in-line.
        /// </summary>
        public void ExpandDataSpecs(ref JSONNode visSpecs)
        {
            if (visSpecs["data"].Value == DxR.Vis.UNDEFINED || visSpecs["data"]["url"].Value == DxR.Vis.UNDEFINED) return;

            if (visSpecs["data"]["url"] != null)
            {
                if (visSpecs["data"]["url"].Value == "inline")
                {
                    return;
                }

                visSpecs["data"].Add("values", CreateValuesSpecs(visSpecs["data"]["url"]));
            }
            else if (visSpecs["data"]["values"] != null)
            {
                if (visSpecs["data"]["url"] == null)
                {
                    visSpecs["data"].Add("url", new JSONString("inline"));
                }
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
            List<string> fieldNames = new List<string>();
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

            // Check special condition if the data is a geoJSON file
            if (dataSpecs["values"]["type"] != null && dataSpecs["values"]["type"] == "FeatureCollection")
            {
                fieldNames.Add("Longitude");
                fieldNames.Add("Latitude");
                var featureCollection = Newtonsoft.Json.JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(dataSpecs["values"].ToString());
                foreach (var kvp in featureCollection.Features.First().Properties)
                {
                    fieldNames.Add(kvp.Key);
                }
            }
            else
            {
                foreach (KeyValuePair<string, JSONNode> kvp in dataSpecs["values"][0].AsObject)
                {
                    fieldNames.Add(kvp.Key);
                }
            }

            return fieldNames;
        }

        internal List<string> GetDataFieldsListFromValues(JSONNode valuesSpecs)
        {
            List<string> fieldNames = new List<string>();
            foreach (KeyValuePair<string, JSONNode> kvp in valuesSpecs[0].AsObject)
            {
                fieldNames.Add(kvp.Key);
            }

            return fieldNames;
        }
    }
}

