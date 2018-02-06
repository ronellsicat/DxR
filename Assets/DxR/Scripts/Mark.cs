using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System.IO;

namespace DxR
{
    /// <summary>
    /// Base class for Mark classes (e.g., MarkPoint for point mark).
    /// Contains methods for setting common mark channels such as position and size.
    /// </summary>
    public class Mark : MonoBehaviour
    {
        public string markName = DxR.Vis.UNDEFINED;
        public Dictionary<string, string> datum = null;
        GameObject tooltip = null;

        public Vector3 forwardDirection = Vector3.up;
        Vector3 curDirection;

        public Mark()
        {

        }

        public void Start()
        {
            curDirection = forwardDirection;
        }

        public virtual List<string> GetChannelsList()
        {
            return new List<string> { "x", "y", "z", "color", "size", "width", "height", "depth", "opacity", "xrotation", "yrotation", "zrotation", "length", "xdirection", "ydirection", "zdirection" };
        }

        public virtual void SetChannelValue(string channel, string value)
        {
            switch(channel)
            {
                case "x":
                    SetLocalPos(value, 0);
                    break;
                case "y":
                    SetLocalPos(value, 1);
                    break;
                case "z":
                    SetLocalPos(value, 2);
                    break;
                 case "width":
                    SetSize(value, 0);
                    break;
                case "height":
                    SetSize(value, 1);
                    break;
                case "length":
                    SetSize(value, GetMaxSizeDimension(forwardDirection));
                    break;
                case "depth":
                    SetSize(value, 2);
                    break;
                case "xoffset":
                    SetOffset(value, 0);
                    break;
                case "yoffset":
                    SetOffset(value, 1);
                    break;
                case "zoffset":
                    SetOffset(value, 2);
                    break;
                case "xoffsetpct":
                    SetOffsetPct(value, 0);
                    break;
                case "yoffsetpct":
                    SetOffsetPct(value, 1);
                    break;
                case "zoffsetpct":
                    SetOffsetPct(value, 2);
                    break;
                case "color":
                    SetColor(value);
                    break;
                case "opacity":
                    SetOpacity(value);
                    break;
                case "size":
                    SetMaxSize(value);
                    break;
                case "xrotation":
                    SetRotation(value, 0);
                    break;
                case "yrotation":
                    SetRotation(value, 1);
                    break;
                case "zrotation":
                    SetRotation(value, 2);
                    break;
                case "xdirection":
                    SetDirectionVector(value, 0);
                    break;
                case "ydirection":
                    SetDirectionVector(value, 1);
                    break;
                case "zdirection":
                    SetDirectionVector(value, 2);
                    break;
                default:
                    throw new System.Exception("Cannot find channel: " + channel);
            }
        }

        private int GetMaxSizeDimension(Vector3 direction)
        {
            if( Math.Abs(direction.x) > Math.Abs(direction.y) &&
                Math.Abs(direction.x) > Math.Abs(direction.z) )
            {
                return 0;

            } else if(  Math.Abs(direction.y) > Math.Abs(direction.x) &&
                        Math.Abs(direction.y) > Math.Abs(direction.z)) 
            {
                return 1;
            }

            return 2;
        }

        public void Infer(Data data, JSONNode specsOrig, out JSONNode specs, 
            string specsFilename)
        {
            specs = null;
            string origSpecsString = specsOrig.ToString();
            specs = JSON.Parse(origSpecsString);

            // Go through each channel and infer the missing specs.
            foreach (KeyValuePair<string, JSONNode> kvp in specs["encoding"].AsObject)
            {
                ChannelEncoding channelEncoding = new ChannelEncoding();

                // Get minimum required values:
                channelEncoding.channel = kvp.Key;
                JSONNode channelSpecs = kvp.Value;
                if (channelSpecs["value"] == null)
                {
                    if (channelSpecs["field"] == null)
                    {
                        throw new Exception("Missing field in channel " + channelEncoding.channel);
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
                            throw new Exception("Missing field data type in channel " + channelEncoding.channel);
                        }
                    }

                    InferScaleSpecsForChannel(ref channelEncoding, ref specs, data);

                    if (channelEncoding.channel == "x" || channelEncoding.channel == "y" ||
                        channelEncoding.channel == "z")
                    {
                        InferAxisSpecsForChannel(ref channelEncoding, ref specs, data);
                    }

                    if (channelEncoding.channel == "color" || channelEncoding.channel == "size")
                    {
                        InferLegendSpecsForChannel(ref channelEncoding, ref specs);
                    }
                }
            }

            for(int n = 0; n < specs["interaction"].AsArray.Count; n++)
            {
                JSONObject node = specs["interaction"].AsArray[n].AsObject;
                if (node["type"] == null || node["field"] == null)
                {
                    continue;
                    //throw new Exception("Missing type and/or field for interaction specs.");
                } else
                {
                    if(node["domain"] == null)
                    {
                        ChannelEncoding ch = new ChannelEncoding();
                        ch.field = node["field"].Value;
                        ch.channel = "color";


                        switch (node["type"].Value)
                        {
                            case "toggleFilter":
                                ch.fieldDataType = "nominal";
                                break;
                            case "thresholdFilter":
                            case "rangeFilter":
                                ch.fieldDataType = "quantitative";
                                break;
                            default:
                                break;
                        }

                        JSONNode temp = null;
                        InferDomain(ch, temp, ref node, data);
                    }
                }
            }

                /*
                string inferResults = specs.ToString(2);
                string filename = "Assets/StreamingAssets/" + specsFilename.TrimEnd(".json".ToCharArray()) + "_inferred.json";
                WriteStringToFile(inferResults, filename);

                Debug.Log("inferred mark:" + specs["mark"].Value);

                string origSpecsStringPrint = specsOrig.ToString(2);
                string filenameOrig = "Assets/StreamingAssets/" + specsFilename.TrimEnd(".json".ToCharArray()) + "_orig.json";
                WriteStringToFile(origSpecsStringPrint, filenameOrig);
                Debug.Log("orig mark:" + specsOrig["mark"].Value);
                */
        }
        
        //public void Infer(Data data, ref JSONNode specs, string specsFilename) { }
        /*
        public void Infer(Data data, ref JSONNode specs, string specsFilename)
        {
            // Go through each channel and infer the missing specs.
            foreach (KeyValuePair<string, JSONNode> kvp in specs["encoding"].AsObject)
            {
                ChannelEncoding channelEncoding = new ChannelEncoding();

                // Get minimum required values:
                channelEncoding.channel = kvp.Key;
                JSONNode channelSpecs = kvp.Value;
                if (channelSpecs["value"] == null)
                {
                    if (channelSpecs["field"] == null)
                    {
                        throw new Exception("Missing field in channel " + channelEncoding.channel);
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
                            throw new Exception("Missing field data type in channel " + channelEncoding.channel);
                        }
                    }
                    
                    InferScaleSpecsForChannel(ref channelEncoding, ref specs, data);
                    
                    if (channelEncoding.channel == "x" || channelEncoding.channel == "y" ||
                        channelEncoding.channel == "z" || channelEncoding.channel == "width" ||
                        channelEncoding.channel == "height" || channelEncoding.channel == "depth")
                    {
                        InferAxisSpecsForChannel(ref channelEncoding, ref specs, data);
                    }

                    if(channelEncoding.channel == "color" || channelEncoding.channel == "size" ||
                        channelEncoding.channel == "shape" || channelEncoding.channel == "opacity")
                    {
                        InferLegendSpecsForChannel(ref channelEncoding, ref specs);
                    }
                }
            }

            InferMarkSpecificSpecs(ref specs);

            string inferResults = specs.ToString();
            string filename = "Assets/StreamingAssets/" + specsFilename.TrimEnd(".json".ToCharArray()) + "_inferred.json";
            WriteStringToFile(inferResults, filename);
        }
        */

        private void InferLegendSpecsForChannel(ref ChannelEncoding channelEncoding, ref JSONNode specs)
        {
            string channel = channelEncoding.channel;
            JSONNode channelSpecs = specs["encoding"][channel];
            JSONNode legendSpecs = channelSpecs["legend"];
            if (legendSpecs != null && legendSpecs.Value.ToString() == "none") return;

            JSONObject legendSpecsObj = (legendSpecs == null) ? new JSONObject() : legendSpecs.AsObject;

            if(legendSpecsObj["type"] == null)
            {
                string fieldDataType = channelSpecs["type"].Value.ToString();
                if (fieldDataType == "quantitative" || fieldDataType == "temporal")
                {
                    legendSpecsObj.Add("type", new JSONString("gradient"));
                } else
                {
                    legendSpecsObj.Add("type", new JSONString("symbol"));
                }
            }

            // TODO: Add proper inference. 
            // HACK: For now, always use hard coded options.
            if(legendSpecsObj["gradientWidth"] == null)
            {
                legendSpecsObj.Add("gradientWidth", new JSONNumber(200));
            }

            if (legendSpecsObj["gradientHeight"] == null)
            {
                legendSpecsObj.Add("gradientHeight", new JSONNumber(50));
            }

            if (legendSpecsObj["face"] == null)
            {
                legendSpecsObj.Add("face", new JSONString("front"));
            }

            if (legendSpecsObj["orient"] == null)
            {
                legendSpecsObj.Add("orient", new JSONString("right"));
            }

            if (legendSpecsObj["face"] == null)
            {
                legendSpecsObj.Add("face", new JSONString("front"));
            }

            if (legendSpecsObj["x"] == null)
            {
                legendSpecsObj.Add("x", new JSONNumber(float.Parse(specs["width"].Value.ToString())));
            }

            if (legendSpecsObj["y"] == null)
            {
                legendSpecsObj.Add("y", new JSONNumber(float.Parse(specs["height"].Value.ToString())));
            }

            if (legendSpecsObj["z"] == null)
            {
                legendSpecsObj.Add("z", new JSONNumber(0));
            }

            if (legendSpecsObj["title"] == null)
            {
                legendSpecsObj.Add("title", new JSONString("Legend: " + channelSpecs["field"]));
            }
            
            specs["encoding"][channelEncoding.channel].Add("legend", legendSpecsObj);
        }

        int GetNumDecimalPlaces(float val)
        {
            return val.ToString().Length - val.ToString().IndexOf(".") - 1;
        }

        private void InferAxisSpecsForChannel(ref ChannelEncoding channelEncoding, ref JSONNode specs, Data data)
        {
            string channel = channelEncoding.channel;
            JSONNode channelSpecs = specs["encoding"][channel];
            JSONNode axisSpecs = channelSpecs["axis"];
            if (axisSpecs != null && axisSpecs.Value.ToString() == "none") return;

            JSONObject axisSpecsObj = (axisSpecs == null) ? new JSONObject() : axisSpecs.AsObject;
            
            if(axisSpecsObj["face"] == null)
            {
                if(channel == "x" || channel == "y")
                {
                    axisSpecsObj.Add("face", new JSONString("front"));
                } else if(channel == "z")
                {
                    axisSpecsObj.Add("face", new JSONString("left"));
                }
            }

            if (axisSpecsObj["orient"] == null)
            {
                if (channel == "x" || channel == "z")
                {
                    axisSpecsObj.Add("orient", new JSONString("bottom"));
                }
                else if (channel == "y")
                {
                    axisSpecsObj.Add("orient", new JSONString("left"));
                }
            }

            if(axisSpecsObj["title"] == null)
            {
                axisSpecsObj.Add("title", new JSONString(channelEncoding.field));
            }

            if(axisSpecsObj["length"] == null)
            {
                float axisLength = 0.0f;
                switch (channelEncoding.channel)
                {
                    case "x":
                    //case "width":
                        axisLength = specs["width"].AsFloat;
                        break;
                    case "y":
                    //case "height":
                        axisLength = specs["height"].AsFloat;
                        break;
                    case "z":
                    //case "depth":
                        axisLength = specs["depth"].AsFloat;
                        break;
                    default:
                        axisLength = 0.0f;
                        break;
                }

                axisSpecsObj.Add("length", new JSONNumber(axisLength));
            }

            if(axisSpecs["color"] == null)
            {
                string color = "";
                switch (channelEncoding.channel)
                {
                    case "x":
                        color = "#ff0000";
                        break;
                    case "y":
                        color = "#00ff00";
                        break;
                    case "z":
                        color = "#0000ff";
                        break;
                    default:
                        break;
                }
                
                axisSpecsObj.Add("color", new JSONString(color));
            }

            if(axisSpecsObj["grid"] == null)
            {
                axisSpecsObj.Add("grid", new JSONBool(false));
            }

            if(axisSpecs["ticks"] == null)
            {
                axisSpecsObj.Add("ticks", new JSONBool(true));
            }
            
            if(axisSpecsObj["values"] == null)
            {
                JSONArray tickValues = new JSONArray();
                JSONNode domain = specs["encoding"][channelEncoding.channel]["scale"]["domain"];
                JSONNode values = channelEncoding.fieldDataType == "quantitative" ? new JSONArray() : domain;

                if (channelEncoding.fieldDataType == "quantitative" && 
                    (channel == "x" || channel == "y" || channel == "z"))
                {
                    // Round domain into a nice number.
                    //float maxDomain = RoundNice(domain.AsArray[1].AsFloat - domain.AsArray[0].AsFloat);

                    int numDecimals = Math.Max(GetNumDecimalPlaces(domain.AsArray[0].AsFloat), GetNumDecimalPlaces(domain.AsArray[1].AsFloat));
                    Debug.Log("NUM DEC " + numDecimals);
                    // Add number of ticks.
                    int defaultNumTicks = 6;
                    int numTicks = axisSpecsObj["tickCount"] == null ? defaultNumTicks : axisSpecsObj["tickCount"].AsInt;
                    float intervals = Math.Abs(domain.AsArray[1].AsFloat - domain.AsArray[0].AsFloat) / (numTicks - 1.0f);


                    for (int i = 0; i < numTicks; i++)
                    {
                        float tickVal = (float)Math.Round(domain.AsArray[0].AsFloat + (intervals * (float)(i)), numDecimals);
                        //Debug.Log(tickVal);
                        values.Add(new JSONString(tickVal.ToString()));
                    }
                }
                
                axisSpecsObj.Add("values", values.AsArray);
            }

            if (axisSpecsObj["tickCount"] == null)
            {
                axisSpecsObj.Add("tickCount", new JSONNumber(axisSpecsObj["values"].Count));
            }

            if(axisSpecsObj["labels"] == null)
            {
                axisSpecsObj.Add("labels", new JSONBool(true));
            }

            specs["encoding"][channelEncoding.channel].Add("axis", axisSpecsObj);
        }

        private float RoundNice(float num)
        {
            float[] roundNumbersArray = { 0.5f, 5.0f, 50.0f };
            List<float> roundNumbers = new List<float>(roundNumbersArray);

            float multiplier = 1.0f;

            while(true)
            {
                for (int i = 0; i < roundNumbers.Count; i++)
                {
                    if (roundNumbers[i] * multiplier >= num)
                    {
                        return roundNumbers[i] * multiplier;
                    }
                }

                multiplier = multiplier + 1.0f;
            }
        }


        // TODO: Expose this so it is very easy to add mark-specific rules.
        private void InferMarkSpecificSpecs(ref JSONNode specs)
        {
            if(markName == "bar" || markName == "rect")
            {
                // Set size of bar or rect along dimension for type band or point.
                
                
                if (specs["encoding"]["x"] != null && specs["encoding"]["width"] == null &&
                    specs["encoding"]["x"]["scale"]["type"] == "band")
                {
                    float bandwidth = ScaleBand.ComputeBandSize(specs["encoding"]["x"]["scale"]);
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber(bandwidth.ToString()));
                    specs["encoding"].Add("width", forceSizeValueObj);
                }

                if (specs["encoding"]["y"] != null && specs["encoding"]["height"] == null &&
                    specs["encoding"]["y"]["scale"]["type"] == "band")
                {
                    float bandwidth = ScaleBand.ComputeBandSize(specs["encoding"]["y"]["scale"]);
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber(bandwidth.ToString()));
                    specs["encoding"].Add("height", forceSizeValueObj);
                }

                if (specs["encoding"]["z"] != null && specs["encoding"]["depth"] == null &&
                    specs["encoding"]["z"]["scale"]["type"] == "band")
                {
                    float bandwidth = ScaleBand.ComputeBandSize(specs["encoding"]["z"]["scale"]);
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber(bandwidth.ToString()));
                    specs["encoding"].Add("depth", forceSizeValueObj);
                }

                if (specs["encoding"]["width"] != null && specs["encoding"]["width"]["value"] == null &&
                    specs["encoding"]["width"]["type"] == "quantitative" && specs["encoding"]["xoffsetpct"] == null)
                {
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber("0.5"));
                    specs["encoding"].Add("xoffsetpct", forceSizeValueObj);
                }

                if (specs["encoding"]["height"] != null && specs["encoding"]["height"]["value"] == null &&
                    specs["encoding"]["height"]["type"] == "quantitative" && specs["encoding"]["yoffsetpct"] == null)
                {
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber("0.5"));
                    specs["encoding"].Add("yoffsetpct", forceSizeValueObj);
                }

                if (specs["encoding"]["depth"] != null && specs["encoding"]["depth"]["value"] == null &&
                   specs["encoding"]["depth"]["type"] == "quantitative" && specs["encoding"]["zoffsetpct"] == null)
                {
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber("0.5"));
                    specs["encoding"].Add("zoffsetpct", forceSizeValueObj);
                }
            }
        }

        private void InferScaleSpecsForChannel(ref ChannelEncoding channelEncoding, ref JSONNode specs, Data data)
        {
            JSONNode channelSpecs = specs["encoding"][channelEncoding.channel];
            JSONNode scaleSpecs = channelSpecs["scale"];
            JSONObject scaleSpecsObj = (scaleSpecs == null) ? new JSONObject() : scaleSpecs.AsObject;
            
            if(scaleSpecs["type"] == null)
            {
                InferScaleType(channelEncoding.channel, channelEncoding.fieldDataType, ref scaleSpecsObj);
            }

            if(!(scaleSpecsObj["type"].Value.ToString() == "none"))
            {
                if (scaleSpecs["domain"] == null)
                {
                    InferDomain(channelEncoding, specs, ref scaleSpecsObj, data);
                }

                if (scaleSpecs["padding"] != null)
                {
                    scaleSpecsObj.Add("paddingInner", scaleSpecs["padding"]);
                    scaleSpecsObj.Add("paddingOuter", scaleSpecs["padding"]);
                }
                else
                {
                    /*
                    if (scaleSpecs["paddingInner"] == null)
                    {
                        scaleSpecsObj.Add("paddingInner", new JSONString(ScaleBand.PADDING_INNER_DEFAULT.ToString()));
                    }

                    if (scaleSpecs["paddingOuter"] == null)
                    {
                        scaleSpecsObj.Add("paddingOuter", new JSONString(ScaleBand.PADDING_OUTER_DEFAULT.ToString()));
                    }*/
                    scaleSpecsObj.Add("padding", new JSONString(ScalePoint.PADDING_DEFAULT.ToString()));
                }

                if (scaleSpecs["range"] == null)
                {
                    InferRange(channelEncoding, specs, ref scaleSpecsObj);
                }

                if (channelEncoding.channel == "color" && !scaleSpecsObj["range"].IsArray && scaleSpecsObj["scheme"] == null)
                {
                    InferColorScheme(channelEncoding, ref scaleSpecsObj);
                }
            }

            specs["encoding"][channelEncoding.channel].Add("scale", scaleSpecsObj);
        }

        private void InferColorScheme(ChannelEncoding channelEncoding, ref JSONObject scaleSpecsObj)
        {
            string range = scaleSpecsObj["range"].Value.ToString();
            string scheme = "";
            if (range == "category")
            {
                if(scaleSpecsObj["domain"].AsArray.Count <= 10)
                {
                    scheme = "tableau10";
                } else
                {
                    scheme = "tableau20";
                }
            } else if(range == "ordinal" || range == "ramp")
            {
                //scheme = "blues";
                scheme = "ramp";
            } else if(range == "heatmap")
            {
                scheme = "viridis";
            } else
            {
                throw new Exception("Cannot infer color scheme for range " + range);
            }

            scaleSpecsObj.Add("scheme", new JSONString(scheme));
        }

        // TODO: Fix range computation to consider paddingOUter!!!
        // TODO: Fix range size.
        private void InferRange(ChannelEncoding channelEncoding, JSONNode specs, ref JSONObject scaleSpecsObj)
        {
            JSONArray range = new JSONArray();

            string channel = channelEncoding.channel;
            if (channel == "x" || channel == "width")
            {
                range.Add(new JSONString("0"));

                if (scaleSpecsObj["rangeStep"] == null)
                {
                    range.Add(new JSONString(specs["width"]));
                } else
                {
                    float rangeSize = float.Parse(scaleSpecsObj["rangeStep"]) * (float)scaleSpecsObj["domain"].Count;
                    range.Add(new JSONString(rangeSize.ToString()));
                    specs["width"] = rangeSize;
                }

            } else if (channel == "y" || channel == "height")
            {
                range.Add(new JSONString("0"));
                if (scaleSpecsObj["rangeStep"] == null)
                {
                    range.Add(new JSONString(specs["height"]));
                }
                else
                {
                    float rangeSize = float.Parse(scaleSpecsObj["rangeStep"]) * (float)scaleSpecsObj["domain"].Count;
                    range.Add(new JSONString(rangeSize.ToString()));
                    specs["height"] = rangeSize;
                }
            } else if (channel == "z" || channel == "depth")
            {
                range.Add(new JSONString("0"));
                if (scaleSpecsObj["rangeStep"] == null)
                {
                    range.Add(new JSONString(specs["depth"]));
                }
                else
                {
                    float rangeSize = float.Parse(scaleSpecsObj["rangeStep"]) * (float)scaleSpecsObj["domain"].Count;
                    range.Add(new JSONString(rangeSize.ToString()));
                    specs["depth"] = rangeSize;
                }
            } else if (channel == "opacity")
            {
                range.Add(new JSONString("0"));
                range.Add(new JSONString("1"));
            } else if (channel == "size" || channel == "length")
            {
                range.Add(new JSONString("0"));
                string maxDimSize = Math.Max(Math.Max(specs["width"].AsFloat, specs["height"].AsFloat),
                    specs["depth"].AsFloat).ToString();

                range.Add(new JSONString(maxDimSize));

            } else if(channel == "color")
            {
                if(channelEncoding.fieldDataType == "nominal")
                {
                    scaleSpecsObj.Add("range", new JSONString("category"));
                }
                else if (channelEncoding.fieldDataType == "ordinal")
                {
                    scaleSpecsObj.Add("range", new JSONString("ordinal"));
                }
                else if (channelEncoding.fieldDataType == "quantitative" ||
                    channelEncoding.fieldDataType == "temporal")
                {
                    scaleSpecsObj.Add("range", new JSONString("ramp"));
                }
                
            } else if(channel == "shape")
            {
                range.Add(new JSONString("symbol"));
                throw new Exception("Not implemented yet.");
            } else if(channel == "xrotation" || channel == "yrotation" || channel == "zrotation")
            {
                range.Add(new JSONString("0"));
                range.Add(new JSONString("360"));
            }
            else if (channel == "xdirection" || channel == "ydirection" || channel == "zdirection")
            {
                range.Add(new JSONString("0"));
                range.Add(new JSONString("1"));
            }

            if (range.Count > 0)
            {
                scaleSpecsObj.Add("range", range);
            }
        }

        private void InferDomain(ChannelEncoding channelEncoding, JSONNode specs, ref JSONObject scaleSpecsObj, Data data)
        {
            string sortType = "ascending";
            if(specs != null && specs["encoding"][channelEncoding.channel]["sort"] != null)
            {
                sortType = specs["encoding"][channelEncoding.channel]["sort"].Value.ToString();
            }
          
            string channel = channelEncoding.channel;
            JSONArray domain = new JSONArray();
            if (channelEncoding.fieldDataType == "quantitative" &&
                (channel == "x" || channel == "y" || channel == "z" ||
                channel == "width" || channel == "height" || channel == "depth" || channel == "length" ||
                channel == "color" || channel == "xrotation" || channel == "yrotation" 
                || channel == "zrotation" || channel == "size" || channel == "xdirection")
                || channel == "ydirection" || channel == "zdirection" || channel == "opacity")
            {
                List<float> minMax = new List<float>();
                GetExtent(data, channelEncoding.field, ref minMax);

                /*
                // For positive minimum values, set the baseline to zero.
                // TODO: Handle logarithmic scale with undefined 0 value.
                if(minMax[0] >= 0)
                {
                    minMax[0] = 0;
                }

                float roundedMaxDomain = RoundNice(minMax[1] - minMax[0]);
                */

                if (sortType == "none" || sortType == "ascending")
                {
                    //domain.Add(new JSONString(minMax[0].ToString()));
                    domain.Add(new JSONString("0"));
                    domain.Add(new JSONString(minMax[1].ToString()));
                } else
                {
                    domain.Add(new JSONString(minMax[1].ToString()));
                    domain.Add(new JSONString("0"));
                    //domain.Add(new JSONString(minMax[0].ToString()));
                }
            } else
            {
                List<string> uniqueValues = new List<string>(); 
                GetUniqueValues(data, channelEncoding.field, ref uniqueValues);

                if (sortType == "ascending")
                {
                    uniqueValues.Sort();
                }
                else if(sortType == "descending")
                {
                    uniqueValues.Sort();
                    uniqueValues.Reverse();
                }

                foreach (string val in uniqueValues)
                {
                    domain.Add(val);
                }
            }

            scaleSpecsObj.Add("domain", domain);
        }

        private void GetUniqueValues(Data data, string field, ref List<string> uniqueValues)
        {
            foreach (Dictionary<string, string> dataValue in data.values)
            {
                string val = dataValue[field];
                if (!uniqueValues.Contains(val))
                {
                    uniqueValues.Add(val);
                }
            }
        }

        private void GetExtent(Data data, string field, ref List<float> extent)
        {
            float min = float.Parse(data.values[0][field]);
            float max = min;
            foreach (Dictionary<string, string> dataValue in data.values)
            {
                float val = float.Parse(dataValue[field]);
                if(val < min)
                {
                    min = val;
                }

                if(val > max)
                {
                    max = val;
                }
            }

            extent.Add(min);
            extent.Add(max);
        }

        private void InferScaleType(string channel, string fieldDataType, ref JSONObject scaleSpecsObj)
        {
            string type = "";
            if (channel == "x" || channel == "y" || channel == "z" ||
                channel == "size" || channel == "opacity")
            {
                if (fieldDataType == "nominal" || fieldDataType == "ordinal")
                {
                    type = "point";
                } else if (fieldDataType == "quantitative")
                {
                    type = "linear";
                } else if (fieldDataType == "temporal")
                {
                    type = "time";
                } else
                {
                    throw new Exception("Invalid field data type: " + fieldDataType);
                }
            } else if (channel == "width" || channel == "height" || channel == "depth" || channel == "length"
                || channel == "xrotation" || channel == "yrotation" || channel == "zrotation" 
                || channel == "xdirection" || channel == "ydirection" || channel == "zdirection")
            {
                if (fieldDataType == "nominal" || fieldDataType == "ordinal")
                {
                    type = "point";
                }
                else if (fieldDataType == "quantitative")
                {
                    type = "linear";
                }
                else if (fieldDataType == "temporal")
                {
                    type = "time";
                }
                else
                {
                    throw new Exception("Invalid field data type: " + fieldDataType);
                }
            } else if (channel == "color")
            {
                if (fieldDataType == "nominal" || fieldDataType == "ordinal")
                {
                    type = "ordinal";
                }
                else if (fieldDataType == "quantitative" || fieldDataType == "temporal")
                {
                    type = "sequential";
                }
                else
                {
                    throw new Exception("Invalid field data type: " + fieldDataType);
                }
            } else if (channel == "shape")
            {
                if (fieldDataType == "nominal" || fieldDataType == "ordinal")
                {
                    type = "ordinal";
                }
                else
                {
                    throw new Exception("Invalid field data type: " + fieldDataType + " for shape channel.");
                }
            } else
            {
                type = "none";
            }

            scaleSpecsObj.Add("type", new JSONString(type));
        }
        
        private void WriteStringToFile(string str, string outputName)
        {
            System.IO.File.WriteAllText(outputName, str);
        }
        
        public void InitTooltip(ref GameObject tooltipObject)
        {
            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer != null)
            {
                DxR.GazeResponder sc = gameObject.AddComponent(typeof(DxR.GazeResponder)) as DxR.GazeResponder;
                tooltip = tooltipObject;
            }
        }

        public void SetTooltipField(string dataField)
        {
            //tooltipDataField = dataField;
        }

        private void SetLocalPos(string value, int dim)
        {
            // TODO: Do this more robustly.
            float pos = float.Parse(value) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

            Vector3 localPos = gameObject.transform.localPosition;
            localPos[dim] = pos;
            gameObject.transform.localPosition = localPos;
        }

        private void SetSize(string value, int dim)
        {
            float size = float.Parse(value) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

            Vector3 initPos = transform.localPosition;

            Vector3 curScale = transform.localScale;

            GetComponent<MeshFilter>().mesh.RecalculateBounds();
            Vector3 origMeshSize = GetComponent<MeshFilter>().mesh.bounds.size;
            curScale[dim] = size / (origMeshSize[dim]);
            transform.localScale = curScale;

            transform.localPosition = initPos;  // This handles models that get translated with scaling.
        }

        private void SetOffset(string value, int dim)
        {
            float offset = float.Parse(value) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
            Vector3 translateBy = transform.localPosition;
            translateBy[dim] = offset + translateBy[dim];
            transform.localPosition = translateBy;
        }

        private void SetOffsetPct(string value, int dim)
        {
            GetComponent<MeshFilter>().mesh.RecalculateBounds();
            float offset = float.Parse(value) * GetComponent<MeshFilter>().mesh.bounds.size[dim] *
                gameObject.transform.localScale[dim];
            Vector3 translateBy = transform.localPosition;
            translateBy[dim] = offset + translateBy[dim];
            transform.localPosition = translateBy;
        }

        private void SetRotation(string value, int dim)
        {
            Vector3 rot = transform.localEulerAngles;
            rot[dim] = float.Parse(value);
            transform.localEulerAngles = rot;
        }

        public void SetMaxSize(string value)
        {
            float size = float.Parse(value) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

            Vector3 renderSize = gameObject.transform.GetComponent<Renderer>().bounds.size;
            Vector3 localScale = gameObject.transform.localScale;

            int maxIndex = 0;
            float maxSize = renderSize[maxIndex];
            for(int i = 1; i < 3; i++)
            {
                if(maxSize < renderSize[i])
                {
                    maxSize = renderSize[i];
                    maxIndex = i;
                }
            }

            float origMaxSize = renderSize[maxIndex] / localScale[maxIndex];
            float newLocalScale = (size / origMaxSize);

            gameObject.transform.localScale = new Vector3(newLocalScale,
                newLocalScale, newLocalScale);
        }

        private void ScaleToMaxDim(string value, int maxDim)
        {
            float size = float.Parse(value) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

            Vector3 renderSize = gameObject.transform.GetComponent<Renderer>().bounds.size;
            Vector3 localScale = gameObject.transform.localScale;

            float origMaxSize = renderSize[maxDim] / localScale[maxDim];
            float newLocalScale = (size / origMaxSize);

            gameObject.transform.localScale = new Vector3(newLocalScale,
                newLocalScale, newLocalScale);
        }

        private void SetColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            if (!colorParsed) return;

            Renderer renderer = transform.GetComponent<Renderer>();
            if(renderer != null)
            {
                renderer.material.color = color;
            } else
            {
                Debug.Log("Cannot set color of mark without renderer object.");
            }
        }

        private void SetOpacity(string value)
        {
            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer != null)
            {
                SetRenderModeToTransparent(renderer.material);
                Color color = renderer.material.color;
                color.a = float.Parse(value);
                renderer.material.color = color;
            }
            else
            {
                Debug.Log("Cannot set opacity of mark without renderer object.");
            }
        }

        private void SetRenderModeToTransparent(Material m)
        {
            m.SetFloat("_Mode", 2);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
        }

        // vectorIndex = 0 for x, 1 for y, 2 for z
        private void SetDirectionVector(string value, int vectorIndex)
        {
            // Set target direction dim to normalized size.
            Vector3 targetOrient = Vector3.zero;
            targetOrient[vectorIndex] = float.Parse(value);
            targetOrient.Normalize();

            // Copy coordinate to current orientation and normalize.
            curDirection[vectorIndex] = targetOrient[vectorIndex];
            curDirection.Normalize();

            Quaternion rotation = Quaternion.FromToRotation(forwardDirection, curDirection);
            transform.rotation = rotation;
        }

        public void OnFocusEnter()
        {            
            if(tooltip != null)
            {
                tooltip.SetActive(true);

                Vector3 markPos = gameObject.transform.localPosition;
                
                string datumTooltipString = BuildTooltipString();
                float tooltipXOffset = 0.05f;
                float tooltipZOffset = -0.05f;
                tooltip.GetComponent<Tooltip>().SetText(datumTooltipString);
                tooltip.GetComponent<Tooltip>().SetLocalPos(markPos.x + tooltipXOffset, 0);
                tooltip.GetComponent<Tooltip>().SetLocalPos(markPos.y, 1);
                tooltip.GetComponent<Tooltip>().SetLocalPos(markPos.z + tooltipZOffset, 2);
            }
        }

        private string BuildTooltipString()
        {
            string output = "";

            foreach (KeyValuePair<string, string> entry in datum)
            {
                // do something with entry.Value or entry.Key
                output = output + entry.Key + ": " + entry.Value + "\n";
            }

            return output;
        }

        public void OnFocusExit()
        {
            if (tooltip != null)
            {
                tooltip.SetActive(false);
            }
        }
    }
}
