using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System.IO;

namespace DxR
{
    public class Analysis_timing : MonoBehaviour
    {
        public int TryNum=10;

        public string TestDataSize = "10,100,1000,10000";
//        public int StartDataSize = 100;
//        public int StopDataSize = 1000;
//        public int Term = 100;
        public string OutputFile = "Analysis_timing.csv";
        public GameObject DxRVis = null;

        [Tooltip("Analysis timing using original data and specs. if checked, TestDataSize and UseRandomDataForScatterplot are ignored")]
        public bool AnalysisOriginalSpecs = false;


        [Tooltip("Use randomly generated data (0<x,y,z<1) for 3D scatterplot")]
        public bool UseRandomDataForScatterplot = false;

        private int cur_time;
        private int start_time;
        private void Awake()
        {
        }
        Vis vis;
        string data_name;
        JSONNode original;
        Data data;
        JSONNode valuesSpecs;

        public void Start()
        {

            if (DxRVis == null) return;


            StreamWriter output = new StreamWriter(OutputFile);
            output.WriteLine("Try,Data_Size,Init,DeleteAll,UpdateVisConfig,UpdateVisData,UpdateMarkPrefab,InferVisSpecs,ConstructVis,Total");


            vis = DxRVis.GetComponent<Vis>();

            if (UseRandomDataForScatterplot)
            {
                vis.visSpecsURL = "scatterplot_timing_test.json";
            }

            Read_data();


            string word="";
            List<int> DataSizes=new List<int>();
            for(int i=0;i< TestDataSize.Length; i++)
            {
                if (TestDataSize[i] == ',')
                {
                    DataSizes.Add(int.Parse(word));
                    word = "";
                }
                else
                {
                    word += TestDataSize[i];
                }
            }
            DataSizes.Add(int.Parse(word));

            for (int i = 0; i < TryNum; i++)
            {
                if (AnalysisOriginalSpecs)
                {
                    output.Write("{0},", i.ToString());
                    output.Write("{0},", data.values.Count.ToString());

                    cur_time = System.Environment.TickCount;
                    start_time = cur_time;
                    vis.I_Awake();
                    output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                    cur_time = System.Environment.TickCount;
                    vis.DeleteAll();
                    output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                    cur_time = System.Environment.TickCount;
                    vis.UpdateVisConfig();
                    output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                    cur_time = System.Environment.TickCount;
                    vis.UpdateVisData();
                    output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                    cur_time = System.Environment.TickCount;
                    vis.UpdateMarkPrefab();
                    output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                    cur_time = System.Environment.TickCount;
                    vis.InferVisSpecs();
                    output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                    cur_time = System.Environment.TickCount;
                    vis.ConstructVis(vis.GetvisSpecsInferred());
                    output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                    output.WriteLine("{0}", (System.Environment.TickCount - start_time).ToString());
                }
                else
                {
                    foreach (int j in DataSizes)
                    {
                        JSONNode cur_Data;

                        cur_Data = ReGenerateData(j);

                        JSONNode temp_file;


                        temp_file = original;
                        temp_file["data"].Remove("values");
                        temp_file["data"].Remove("url");
                        temp_file["data"].Add("values", cur_Data);
                        temp_file["data"].Add("url", new JSONString("inline"));


                        File.WriteAllText(Application.streamingAssetsPath + "/DxRSpecs/For_test.json", temp_file.ToString());

                        vis.visSpecsURL = "For_test.json";
                        output.Write("{0},", i.ToString());
                        output.Write("{0},", j.ToString());

                        cur_time = System.Environment.TickCount;
                        start_time = cur_time;
                        vis.I_Awake();
                        output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                        cur_time = System.Environment.TickCount;
                        vis.DeleteAll();
                        output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                        cur_time = System.Environment.TickCount;
                        vis.UpdateVisConfig();
                        output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                        cur_time = System.Environment.TickCount;
                        vis.UpdateVisData();
                        output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                        cur_time = System.Environment.TickCount;
                        vis.UpdateMarkPrefab();
                        output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                        cur_time = System.Environment.TickCount;
                        vis.InferVisSpecs();
                        output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                        cur_time = System.Environment.TickCount;
                        vis.ConstructVis(vis.GetvisSpecsInferred());
                        output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                        output.WriteLine("{0}", (System.Environment.TickCount - start_time).ToString());

                    }

                }
            }
            output.Close();

        }

        void Read_data()
        {
            Parser t_parser;
            t_parser = new Parser();

            t_parser.Parse(vis.visSpecsURL, out original);

            if (original["data"]["url"] != "inline")
            {
                original["data"].Add("values", t_parser.CreateValuesSpecs(original["data"]["url"]));
                data_name = original["data"]["url"];
            }

            valuesSpecs = original["data"]["values"];


            data = new Data();

            vis.CreateDataFields(valuesSpecs, ref data);

            data.values = new List<Dictionary<string, string>>();

            int numDataFields = data.fieldNames.Count;

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
        JSONNode ReGenerateData(int num) 
        {
            JSONNode cur_data = new JSONArray();
            
            int numDataFields = data.fieldNames.Count;
            int numData = data.values.Count;
            System.Random rnd = new System.Random();
            for(int i = 0; i < num; i++)
            {
                JSONNode cur_line = new JSONObject();

                for (int fieldIndex = 0; fieldIndex < numDataFields; fieldIndex++)
                {
                    if (UseRandomDataForScatterplot)
                    {
                        float xValue = ((float)rnd.Next(100000)) / 100000;
                        float yValue = ((float)rnd.Next(100000)) / 100000;
                        float zValue = ((float)rnd.Next(100000)) / 100000;
                        cur_line.Add("x", new JSONNumber(xValue));
                        cur_line.Add("y", new JSONNumber(yValue));
                        cur_line.Add("z", new JSONNumber(zValue));
                    }
                    else
                    {
                        string curFieldName = data.fieldNames[fieldIndex];
                        float floatValue = 0.0f;
                        int intValue = 0;
                        string curValue = valuesSpecs[rnd.Next(numData)][curFieldName].Value;
                        while (curValue == "null") curValue = valuesSpecs[rnd.Next(numData)][curFieldName].Value;
                        if (float.TryParse(curValue, out floatValue))
                        {
                            cur_line.Add(curFieldName, new JSONNumber(floatValue));
                        }
                        else if (int.TryParse(curValue, out intValue))
                        {
                            cur_line.Add(curFieldName, new JSONNumber((float)intValue));
                        }
                        else
                        {
                            cur_line.Add(curFieldName, new JSONString(curValue));
                        }

                    }
                }
                
                cur_data.Add(cur_line);
                
            }
            return cur_data;
        }
    }
}