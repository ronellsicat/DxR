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
        public string TestName;

        //[Tooltip("Analysis timing using original data and specs. if checked, TestDataSize and UseRandomDataForScatterplot are ignored")]
        //public bool AnalysisOriginalSpecs = false;


        //[Tooltip("Use randomly generated data (0<x,y,z<1) for 3D scatterplot")]
        //public bool UseRandomDataForScatterplot = false;

        private int cur_time;
        private int start_time;
        private void Awake()
        {
        }
        string data_name;
        JSONNode original;
        Data data;
        JSONNode valuesSpecs;

        int frame = 0;
        StreamWriter output;
        int i = 0;
        GameObject vis;

        bool measureFPS = false;

        bool measureFPSinit = false;

        public void Start()
        {
            output = new StreamWriter(OutputFile);
            output.WriteLine("Try,Construct time,Data_Size,FPS");
            string word = "";
            List<int> DataSizes = new List<int>();
            for (int i = 0; i < TestDataSize.Length; i++)
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



        }


        private void Update()
        {
            frame++;

            if (measureFPS)
            {
                if (!measureFPSinit)
                {
                    if (frame == 60)
                    {
                        frame = 0;
                        cur_time = System.Environment.TickCount;
                        measureFPSinit = true;
                    }
                }
                else
                {
                    if (frame == 180)
                    {
                        float fps = frame * 1000.0f / (System.Environment.TickCount - cur_time);
                        output.WriteLine("{0}", fps.ToString());

                        Destroy(vis);

                        i++;
                        frame = 0;
                        if (i == TryNum)
                        {
                            frame = 9999;
                            output.Close();
                        }
                        measureFPS = false;
                        measureFPSinit = false;
                    }
                }
            }
            else{
                if (frame == 60)
                {
                    output.Write("{0},", i.ToString());

                    string filename = "Timing/" + TestName;
                    cur_time = System.Environment.TickCount;
                    vis = Instantiate(Resources.Load(filename) as GameObject);
                    while (!vis.GetComponent<Vis>().IsReady)
                    {
                        //wait
                    }
                    output.Write("{0},", (System.Environment.TickCount - cur_time).ToString());

                    Read_data(vis.GetComponent<Vis>());
                    output.Write("{0},", data.values.Count.ToString());
                    measureFPS = true;
                    frame = 0;

                }
            }
        }
        
        void Read_data(Vis vis)
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

            data.fieldNames = new List<string>();
            foreach (KeyValuePair<string, JSONNode> kvp in valuesSpecs[0].AsObject)
            {
                data.fieldNames.Add(kvp.Key);
                
            }

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
        //JSONNode ReGenerateData(int num) 
        //{
        //    JSONNode cur_data = new JSONArray();

        //    int numDataFields = data.fieldNames.Count;
        //    int numData = data.values.Count;
        //    System.Random rnd = new System.Random();
        //    for(int i = 0; i < num; i++)
        //    {
        //        JSONNode cur_line = new JSONObject();

        //        for (int fieldIndex = 0; fieldIndex < numDataFields; fieldIndex++)
        //        {
        //            if (UseRandomDataForScatterplot)
        //            {
        //                float xValue = ((float)rnd.Next(100000)) / 100000;
        //                float yValue = ((float)rnd.Next(100000)) / 100000;
        //                float zValue = ((float)rnd.Next(100000)) / 100000;
        //                cur_line.Add("x", new JSONNumber(xValue));
        //                cur_line.Add("y", new JSONNumber(yValue));
        //                cur_line.Add("z", new JSONNumber(zValue));
        //            }
        //            else
        //            {
        //                string curFieldName = data.fieldNames[fieldIndex];
        //                float floatValue = 0.0f;
        //                int intValue = 0;
        //                string curValue = valuesSpecs[rnd.Next(numData)][curFieldName].Value;
        //                while (curValue == "null") curValue = valuesSpecs[rnd.Next(numData)][curFieldName].Value;
        //                if (float.TryParse(curValue, out floatValue))
        //                {
        //                    cur_line.Add(curFieldName, new JSONNumber(floatValue));
        //                }
        //                else if (int.TryParse(curValue, out intValue))
        //                {
        //                    cur_line.Add(curFieldName, new JSONNumber((float)intValue));
        //                }
        //                else
        //                {
        //                    cur_line.Add(curFieldName, new JSONString(curValue));
        //                }

        //            }
        //        }

        //        cur_data.Add(cur_line);

        //    }
        //    return cur_data;
        //}
    }
}