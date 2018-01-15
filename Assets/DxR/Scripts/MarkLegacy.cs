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
    public class MarkLegacy : MonoBehaviour
    {
        public string markName = DxR.Vis.UNDEFINED;
        public Dictionary<string, string> datum = null;
        public GameObject tooltip = null;
        private string tooltipDataField = DxR.Vis.UNDEFINED;

        public MarkLegacy()
        {

        }

        public void Start()
        {
            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer != null)
            {
                DxR.GazeResponder sc = gameObject.AddComponent(typeof(DxR.GazeResponder)) as DxR.GazeResponder;
            }
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
                case "tooltip":
                    throw new System.Exception("tooltip is not a valid channel.");
                case "x2":
                    throw new System.Exception("x2 is not a valid channel - use x, and width instead.");
                case "y2":
                    throw new System.Exception("y2 is not a valid channel - use y, and height instead.");
                case "z2":
                    throw new System.Exception("z2 is not a valid channel - use z, and depth instead.");
                default:
                    throw new System.Exception("Cannot find channel: " + channel);
            }
        }

        public void Infer(Data data, ref JSONNode visSpecs, string visSpecsFilename)
        {
            // Go through each channel and infer the missing specs.
            foreach (KeyValuePair<string, JSONNode> kvp in visSpecs["encoding"].AsObject)
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
                    
                    InferScaleSpecsForChannel(ref channelEncoding, ref visSpecs, data);
                    
                    if (channelEncoding.channel == "x" || channelEncoding.channel == "y" ||
                        channelEncoding.channel == "z" || channelEncoding.channel == "width" ||
                        channelEncoding.channel == "height" || channelEncoding.channel == "depth")
                    {
                        InferAxisSpecsForChannel(ref channelEncoding, ref visSpecs, data);
                    }

                    if(channelEncoding.channel == "color" || channelEncoding.channel == "size" ||
                        channelEncoding.channel == "shape" || channelEncoding.channel == "opacity")
                    {
                        InferLegendSpecsForChannel(ref channelEncoding, ref visSpecs);
                    }
                }
            }

            InferMarkSpecificSpecs(ref visSpecs);

            string inferResults = visSpecs.ToString();
            string filename = "Assets/StreamingAssets/" + visSpecsFilename.TrimEnd(".json".ToCharArray()) + "_inferred.json";
            WriteStringToFile(inferResults, filename);
        }
        
        private void InferLegendSpecsForChannel(ref ChannelEncoding channelEncoding, ref JSONNode visSpecs)
        {
            string channel = channelEncoding.channel;
            JSONNode channelSpecs = visSpecs["encoding"][channel];
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
                legendSpecsObj.Add("x", new JSONNumber(float.Parse(visSpecs["width"].Value.ToString())));
            }

            if (legendSpecsObj["y"] == null)
            {
                legendSpecsObj.Add("y", new JSONNumber(float.Parse(visSpecs["height"].Value.ToString())));
            }

            if (legendSpecsObj["z"] == null)
            {
                legendSpecsObj.Add("z", new JSONNumber(0));
            }

            if (legendSpecsObj["title"] == null)
            {
                legendSpecsObj.Add("title", new JSONString("Legend: " + channelSpecs["field"]));
            }
            
            visSpecs["encoding"][channelEncoding.channel].Add("legend", legendSpecsObj);
        }

        private void InferAxisSpecsForChannel(ref ChannelEncoding channelEncoding, ref JSONNode visSpecs, Data data)
        {
            string channel = channelEncoding.channel;
            JSONNode channelSpecs = visSpecs["encoding"][channel];
            JSONNode axisSpecs = channelSpecs["axis"];
            if (axisSpecs != null && axisSpecs.Value.ToString() == "none") return;

            JSONObject axisSpecsObj = (axisSpecs == null) ? new JSONObject() : axisSpecs.AsObject;
            
            if(axisSpecsObj["face"] == null)
            {
                if(channel == "x" || channel == "y" || channel == "width" || channel == "height")
                {
                    axisSpecsObj.Add("face", new JSONString("front"));
                } else if(channel == "z" || channel == "depth")
                {
                    axisSpecsObj.Add("face", new JSONString("left"));
                }
            }

            if (axisSpecsObj["orient"] == null)
            {
                if (channel == "x" || channel == "z" || channel == "width" || channel == "depth")
                {
                    axisSpecsObj.Add("orient", new JSONString("bottom"));
                }
                else if (channel == "y" || channel == "height")
                {
                    axisSpecsObj.Add("orient", new JSONString("left"));
                }
            }

            if(axisSpecsObj["title"] == null)
            {
                axisSpecsObj.Add("title", new JSONString(channelEncoding.field));
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
                JSONNode domain = visSpecs["encoding"][channelEncoding.channel]["scale"]["domain"];
                JSONNode values = channelEncoding.fieldDataType == "quantitative" ? new JSONArray() : domain;

                if (channelEncoding.fieldDataType == "quantitative" && 
                    (channel == "x" || channel == "y" || channel == "z" ||
                    channel == "width" || channel == "height" || channel == "depth"))
                {
                    // Round domain into a nice number.
                    // TODO: make robust rounding.
                    // HACK:
                    float maxDomain = RoundNice(domain.AsArray[1].AsFloat - domain.AsArray[0].AsFloat);

                    // Add number of ticks.
                    int defaultNumTicks = 6;
                    int numTicks = axisSpecsObj["tickCount"] == null ? defaultNumTicks : axisSpecsObj["tickCount"].AsInt;
                    float intervals = maxDomain / (numTicks - 1.0f);


                    for (int i = 0; i < numTicks; i++)
                    {
                        float tickVal = domain.AsArray[0].AsFloat + (intervals * (float)(i));
                        values.Add(new JSONNumber(tickVal));
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

            visSpecs["encoding"][channelEncoding.channel].Add("axis", axisSpecsObj);
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
        private void InferMarkSpecificSpecs(ref JSONNode visSpecs)
        {
            if(markName == "bar" || markName == "rect")
            {
                // Set size of bar or rect along dimension for type band or point.
                
                
                if (visSpecs["encoding"]["x"] != null && visSpecs["encoding"]["width"] == null &&
                    visSpecs["encoding"]["x"]["scale"]["type"] == "band")
                {
                    float bandwidth = ScaleBand.ComputeBandSize(visSpecs["encoding"]["x"]["scale"]);
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber(bandwidth.ToString()));
                    visSpecs["encoding"].Add("width", forceSizeValueObj);
                }

                if (visSpecs["encoding"]["y"] != null && visSpecs["encoding"]["height"] == null &&
                    visSpecs["encoding"]["y"]["scale"]["type"] == "band")
                {
                    float bandwidth = ScaleBand.ComputeBandSize(visSpecs["encoding"]["y"]["scale"]);
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber(bandwidth.ToString()));
                    visSpecs["encoding"].Add("height", forceSizeValueObj);
                }

                if (visSpecs["encoding"]["z"] != null && visSpecs["encoding"]["depth"] == null &&
                    visSpecs["encoding"]["z"]["scale"]["type"] == "band")
                {
                    float bandwidth = ScaleBand.ComputeBandSize(visSpecs["encoding"]["z"]["scale"]);
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber(bandwidth.ToString()));
                    visSpecs["encoding"].Add("depth", forceSizeValueObj);
                }

                if (visSpecs["encoding"]["width"] != null && visSpecs["encoding"]["width"]["value"] == null &&
                    visSpecs["encoding"]["width"]["type"] == "quantitative" && visSpecs["encoding"]["xoffsetpct"] == null)
                {
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber("0.5"));
                    visSpecs["encoding"].Add("xoffsetpct", forceSizeValueObj);
                }

                if (visSpecs["encoding"]["height"] != null && visSpecs["encoding"]["height"]["value"] == null &&
                    visSpecs["encoding"]["height"]["type"] == "quantitative" && visSpecs["encoding"]["yoffsetpct"] == null)
                {
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber("0.5"));
                    visSpecs["encoding"].Add("yoffsetpct", forceSizeValueObj);
                }

                if (visSpecs["encoding"]["depth"] != null && visSpecs["encoding"]["depth"]["value"] == null &&
                   visSpecs["encoding"]["depth"]["type"] == "quantitative" && visSpecs["encoding"]["zoffsetpct"] == null)
                {
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber("0.5"));
                    visSpecs["encoding"].Add("zoffsetpct", forceSizeValueObj);
                }
            }
        }

        private void InferScaleSpecsForChannel(ref ChannelEncoding channelEncoding, ref JSONNode visSpecs, Data data)
        {
            JSONNode channelSpecs = visSpecs["encoding"][channelEncoding.channel];
            JSONNode scaleSpecs = channelSpecs["scale"];
            JSONObject scaleSpecsObj = (scaleSpecs == null) ? new JSONObject() : scaleSpecs.AsObject;
            
            if(scaleSpecs["type"] == null)
            {
                InferScaleType(channelEncoding.channel, channelEncoding.fieldDataType, ref scaleSpecsObj);
            }

            if(!(scaleSpecsObj["type"].Value.ToString() == "none" || scaleSpecsObj["type"].Value.ToString() == "custom"))
            {
                if (scaleSpecs["domain"] == null)
                {
                    InferDomain(channelEncoding, visSpecs, ref scaleSpecsObj, data);
                }

                if (scaleSpecs["padding"] != null)
                {
                    scaleSpecsObj.Add("paddingInner", scaleSpecs["padding"]);
                    scaleSpecsObj.Add("paddingOuter", scaleSpecs["padding"]);
                }
                else
                {
                    if (scaleSpecs["paddingInner"] == null)
                    {
                        scaleSpecsObj.Add("paddingInner", new JSONString(ScaleBand.PADDING_INNER_DEFAULT.ToString()));
                    }

                    if (scaleSpecs["paddingOuter"] == null)
                    {
                        scaleSpecsObj.Add("paddingOuter", new JSONString(ScaleBand.PADDING_OUTER_DEFAULT.ToString()));
                    }
                }

                if (scaleSpecs["range"] == null)
                {
                    InferRange(channelEncoding, visSpecs, ref scaleSpecsObj);
                }

                if (channelEncoding.channel == "color" && !scaleSpecsObj["range"].IsArray && scaleSpecsObj["scheme"] == null)
                {
                    InferColorScheme(channelEncoding, ref scaleSpecsObj);
                }
            }

            visSpecs["encoding"][channelEncoding.channel].Add("scale", scaleSpecsObj);
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
                scheme = "blues";
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
        private void InferRange(ChannelEncoding channelEncoding, JSONNode visSpecs, ref JSONObject scaleSpecsObj)
        {
            JSONArray range = new JSONArray();

            string channel = channelEncoding.channel;
            if (channel == "x" || channel == "width")
            {
                range.Add(new JSONString("0"));

                if(scaleSpecsObj["rangeStep"] == null)
                {
                    range.Add(new JSONString(visSpecs["width"]));                    
                } else
                {
                    float rangeSize = float.Parse(scaleSpecsObj["rangeStep"]) * (float)scaleSpecsObj["domain"].Count;
                    range.Add(new JSONString(rangeSize.ToString()));
                    visSpecs["width"] = rangeSize;
                }
                
            } else if(channel == "y" || channel == "height")
            {
                range.Add(new JSONString("0"));
                if (scaleSpecsObj["rangeStep"] == null)
                {
                    range.Add(new JSONString(visSpecs["height"]));                   
                }
                else
                {
                    float rangeSize = float.Parse(scaleSpecsObj["rangeStep"]) * (float)scaleSpecsObj["domain"].Count;
                    range.Add(new JSONString(rangeSize.ToString()));
                    visSpecs["height"] = rangeSize;
                }
            } else if(channel == "z" || channel == "depth")
            {
                range.Add(new JSONString("0"));
                if (scaleSpecsObj["rangeStep"] == null)
                {
                    range.Add(new JSONString(visSpecs["depth"]));                   
                }
                else
                {
                    float rangeSize = float.Parse(scaleSpecsObj["rangeStep"]) * (float)scaleSpecsObj["domain"].Count;
                    range.Add(new JSONString(rangeSize.ToString()));
                    visSpecs["depth"] = rangeSize;
                }
            } else if(channel == "opacity")
            {
                range.Add(new JSONString("0"));
                range.Add(new JSONString("1"));
            } else if(channel == "size")
            {
                // TODO: Get min and max size of mark.
                
                // HACK: Hard code range
                range.Add(new JSONString("0"));
                range.Add(new JSONString("200"));

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
                    if(markName == "rect")
                    {
                        scaleSpecsObj.Add("range", new JSONString("heatmap"));
                    } else
                    {
                        scaleSpecsObj.Add("range", new JSONString("ramp"));
                    }
                }
                
            } else if(channel == "shape")
            {
                range.Add(new JSONString("symbol"));
                throw new Exception("Not implemented yet.");
            }

            if(range.Count > 0)
            {
                scaleSpecsObj.Add("range", range);
            }
        }

        private void InferDomain(ChannelEncoding channelEncoding, JSONNode visSpecs, ref JSONObject scaleSpecsObj, Data data)
        {
            string sortType = "ascending";
            if(visSpecs["encoding"][channelEncoding.channel]["sort"] != null)
            {
                sortType = visSpecs["encoding"][channelEncoding.channel]["sort"].Value.ToString();
            }

            string channel = channelEncoding.channel;
            JSONArray domain = new JSONArray();
            if (channelEncoding.fieldDataType == "quantitative" &&
                (channel == "x" || channel == "y" || channel == "z" ||
                channel == "width" || channel == "height" || channel == "depth" || 
                channel == "color" || channel == "xorient" || channel == "yorient" 
                || channel == "zorient") )
            {
                List<float> minMax = new List<float>();
                GetExtent(data, channelEncoding.field, ref minMax);
                // For positive minimum values, set the baseline to zero.
                // TODO: Handle logarithmic scale with undefined 0 value.
                if(minMax[0] >= 0)
                {
                    minMax[0] = 0;
                }

                float roundedMaxDomain = RoundNice(minMax[1] - minMax[0]);

                if (sortType == "none" || sortType == "ascending")
                {
                    domain.Add(new JSONString(minMax[0].ToString()));
                    domain.Add(new JSONString(roundedMaxDomain.ToString()));
                } else
                {
                    domain.Add(new JSONString(roundedMaxDomain.ToString()));
                    domain.Add(new JSONString(minMax[0].ToString()));
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
                    type = "band";
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
            } else if (channel == "width" || channel == "height" || channel == "depth")
            {
                if (fieldDataType == "nominal" || fieldDataType == "ordinal")
                {
                    type = "band";
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
            } else if (channel == "text" || channel == "tooltip") {

                type = "none";
            } else
            {
                Debug.Log("Cannot infer scale type of channel " + channel);
                return;
            }

            scaleSpecsObj.Add("type", new JSONString(type));
        }
        
        private void WriteStringToFile(string str, string outputName)
        {
            System.IO.File.WriteAllText(outputName, str);
        }
        
        public void SetTooltipObject(ref GameObject tooltipObject)
        {
            tooltip = tooltipObject;
        }

        public void SetTooltipField(string dataField)
        {
            tooltipDataField = dataField;
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
            translateBy[dim] = offset - translateBy[dim];
            transform.localPosition = translateBy;
        }

        private void SetOffsetPct(string value, int dim)
        {
            GetComponent<MeshFilter>().mesh.RecalculateBounds();
            float offset = float.Parse(value) * GetComponent<MeshFilter>().mesh.bounds.size[dim] *
                gameObject.transform.localScale[dim];
            Vector3 translateBy = transform.localPosition;
            translateBy[dim] = offset - translateBy[dim];
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

        private void SetColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
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

        public void OnFocusEnter()
        {            
            if(tooltip != null)
            {
                tooltip.SetActive(true);

                Vector3 markPos = gameObject.transform.localPosition;

                tooltip.GetComponent<Tooltip>().SetText(tooltipDataField + ": " + datum[tooltipDataField]);
                tooltip.GetComponent<Tooltip>().SetLocalPos(markPos.x, 0);
                tooltip.GetComponent<Tooltip>().SetLocalPos(markPos.y, 1);
                tooltip.GetComponent<Tooltip>().SetLocalPos(markPos.z, 2);
            }
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
