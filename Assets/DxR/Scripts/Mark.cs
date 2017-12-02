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
        public string markName = DxR.SceneObject.UNDEFINED;
        public Dictionary<string, string> datum = null;
        public GameObject tooltip = null;
        private string tooltipDataField = DxR.SceneObject.UNDEFINED;

        public Mark()
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

        public void Infer(Data data, ref JSONNode sceneSpecs)
        {
            // Go through each channel and infer the missing specs.
            foreach (KeyValuePair<string, JSONNode> kvp in sceneSpecs["encoding"].AsObject)
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
                }

                InferScaleSpecsForChannel(ref channelEncoding, ref sceneSpecs, data);  
                
                if(channelEncoding.channel == "x" || channelEncoding.channel == "y" ||
                    channelEncoding.channel == "z" || channelEncoding.channel == "width" ||
                    channelEncoding.channel == "height" || channelEncoding.channel == "depth")
                {
                    InferAxisSpecsForChannel(ref channelEncoding, ref sceneSpecs, data);
                }
                //InferLegendSpecsForChannel(ref channelEncoding, ref sceneSpecs);

            }

            InferMarkSpecificSpecs(ref sceneSpecs);

            string inferResults = sceneSpecs.ToString();
            WriteStringToFile(inferResults, "Assets/StreamingAssets/DxRSpecs/inferred.json");
        }

        private void InferAxisSpecsForChannel(ref ChannelEncoding channelEncoding, ref JSONNode sceneSpecs, Data data)
        {
            string channel = channelEncoding.channel;
            JSONNode channelSpecs = sceneSpecs["encoding"][channel];
            JSONNode axisSpecs = channelSpecs["axis"];
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
                JSONNode domain = sceneSpecs["encoding"][channelEncoding.channel]["scale"]["domain"];
                axisSpecsObj.Add("values", domain.AsArray);
            }

            if (axisSpecsObj["tickCount"] == null)
            {
                axisSpecsObj.Add("tickCount", new JSONNumber(axisSpecsObj["values"].Count));
            }

            if(axisSpecsObj["labels"] == null)
            {
                axisSpecsObj.Add("labels", new JSONBool(true));
            }

            sceneSpecs["encoding"][channelEncoding.channel].Add("axis", axisSpecsObj);
        }

        // TODO: Expose this so it is very easy to add mark-specific rules.
        private void InferMarkSpecificSpecs(ref JSONNode sceneSpecs)
        {
            if(markName == "bar" || markName == "rect")
            {
                // Set size of bar or rect along dimension for type band or point.
                
                
                if (sceneSpecs["encoding"]["x"] != null && sceneSpecs["encoding"]["width"] == null &&
                    sceneSpecs["encoding"]["x"]["scale"]["type"] == "band")
                {
                    float bandwidth = ScaleBand.ComputeBandSize(sceneSpecs["encoding"]["x"]["scale"]);
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber(bandwidth.ToString()));
                    sceneSpecs["encoding"].Add("width", forceSizeValueObj);
                }

                if (sceneSpecs["encoding"]["y"] != null && sceneSpecs["encoding"]["height"] == null &&
                    sceneSpecs["encoding"]["x"]["scale"]["type"] == "band")
                {
                    float bandwidth = ScaleBand.ComputeBandSize(sceneSpecs["encoding"]["y"]["scale"]);
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber(bandwidth.ToString()));
                    sceneSpecs["encoding"].Add("height", forceSizeValueObj);
                }

                if (sceneSpecs["encoding"]["z"] != null && sceneSpecs["encoding"]["depth"] == null &&
                    sceneSpecs["encoding"]["x"]["scale"]["type"] == "band")
                {
                    float bandwidth = ScaleBand.ComputeBandSize(sceneSpecs["encoding"]["z"]["scale"]);
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber(bandwidth.ToString()));
                    sceneSpecs["encoding"].Add("depth", forceSizeValueObj);
                }

                if (sceneSpecs["encoding"]["width"] != null && sceneSpecs["encoding"]["width"]["value"] == null &&
                    sceneSpecs["encoding"]["width"]["type"] == "quantitative" && sceneSpecs["encoding"]["xoffsetpct"] == null)
                {
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber("0.5"));
                    sceneSpecs["encoding"].Add("xoffsetpct", forceSizeValueObj);
                }

                if (sceneSpecs["encoding"]["height"] != null && sceneSpecs["encoding"]["height"]["value"] == null &&
                    sceneSpecs["encoding"]["height"]["type"] == "quantitative" && sceneSpecs["encoding"]["yoffsetpct"] == null)
                {
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber("0.5"));
                    sceneSpecs["encoding"].Add("yoffsetpct", forceSizeValueObj);
                }

                if (sceneSpecs["encoding"]["depth"] != null && sceneSpecs["encoding"]["depth"]["value"] == null &&
                   sceneSpecs["encoding"]["depth"]["type"] == "quantitative" && sceneSpecs["encoding"]["zoffsetpct"] == null)
                {
                    JSONObject forceSizeValueObj = new JSONObject();
                    forceSizeValueObj.Add("value", new JSONNumber("0.5"));
                    sceneSpecs["encoding"].Add("zoffsetpct", forceSizeValueObj);
                }
            }
        }

        private void InferScaleSpecsForChannel(ref ChannelEncoding channelEncoding, ref JSONNode sceneSpecs, Data data)
        {
            JSONNode channelSpecs = sceneSpecs["encoding"][channelEncoding.channel];
            JSONNode scaleSpecs = channelSpecs["scale"];
            JSONObject scaleSpecsObj = null;

            if (scaleSpecs == null)
            {
                scaleSpecsObj = new JSONObject();
                InferScaleType(channelEncoding.channel, channelEncoding.fieldDataType, ref scaleSpecsObj);
            } else
            {
                scaleSpecsObj = scaleSpecs.AsObject;
            }

            if(scaleSpecs["domain"] == null)
            {
                InferDomain(channelEncoding.field, channelEncoding.fieldDataType, sceneSpecs, ref scaleSpecsObj, data);
            }

            if(scaleSpecs["padding"] != null)
            {
                scaleSpecsObj.Add("paddingInner", scaleSpecs["padding"]);
                scaleSpecsObj.Add("paddingOuter", scaleSpecs["padding"]);
            } else
            {
                if(scaleSpecs["paddingInner"] == null)
                {
                    scaleSpecsObj.Add("paddingInner", new JSONString(ScaleBand.PADDING_INNER_DEFAULT.ToString()));
                }

                if (scaleSpecs["paddingOuter"] == null)
                {
                    scaleSpecsObj.Add("paddingOuter", new JSONString(ScaleBand.PADDING_OUTER_DEFAULT.ToString()));
                }
            }
            
            if(scaleSpecs["range"] == null)
            {
                InferRange(channelEncoding, sceneSpecs, ref scaleSpecsObj);
            }
            
            sceneSpecs["encoding"][channelEncoding.channel].Add("scale", scaleSpecsObj);
        }

        // TODO: Fix range computation to consider paddingOUter!!!
        private void InferRange(ChannelEncoding channelEncoding, JSONNode sceneSpecs, ref JSONObject scaleSpecsObj)
        {
            JSONArray range = new JSONArray();

            string channel = channelEncoding.channel;
            if (channel == "x" || channel == "width")
            {
                range.Add(new JSONString("0"));

                if(scaleSpecsObj["rangeStep"] == null)
                {
                    range.Add(new JSONString(sceneSpecs["width"]));
                    /*
                    if(channel == "x")
                    {
                        float rangeStep = ScaleBand.ComputeRangeStep(sceneSpecs["encoding"]["x"]["scale"]);
                        scaleSpecsObj.Add("rangeStep", new JSONString(rangeStep.ToString()));
                    }
                    */
                } else
                {
                    float rangeSize = float.Parse(scaleSpecsObj["rangeStep"]) * (float)scaleSpecsObj["domain"].Count;
                    range.Add(new JSONString(rangeSize.ToString()));
                    sceneSpecs["width"] = rangeSize.ToString();
                }
                
            } else if(channel == "y" || channel == "height")
            {
                range.Add(new JSONString("0"));
                if (scaleSpecsObj["rangeStep"] == null)
                {
                    range.Add(new JSONString(sceneSpecs["height"]));
                    /*
                    if (channel == "y")
                    {
                        float rangeStep = ScaleBand.ComputeRangeStep(sceneSpecs["encoding"]["y"]["scale"]);
                        scaleSpecsObj.Add("rangeStep", new JSONString(rangeStep.ToString()));
                    }
                    */
                }
                else
                {
                    float rangeSize = float.Parse(scaleSpecsObj["rangeStep"]) * (float)scaleSpecsObj["domain"].Count;
                    range.Add(new JSONString(rangeSize.ToString()));
                    sceneSpecs["height"] = rangeSize.ToString();
                }
            } else if(channel == "z" || channel == "depth")
            {
                range.Add(new JSONString("0"));
                if (scaleSpecsObj["rangeStep"] == null)
                {
                    range.Add(new JSONString(sceneSpecs["depth"]));
                    /*
                    if (channel == "z")
                    {
                        float rangeStep = ScaleBand.ComputeRangeStep(sceneSpecs["encoding"]["z"]["scale"]);
                        scaleSpecsObj.Add("rangeStep", new JSONString(rangeStep.ToString()));
                    }
                    */
                }
                else
                {
                    float rangeSize = float.Parse(scaleSpecsObj["rangeStep"]) * (float)scaleSpecsObj["domain"].Count;
                    range.Add(new JSONString(rangeSize.ToString()));
                    sceneSpecs["depth"] = rangeSize.ToString();
                }
            } else if(channel == "opacity")
            {
                range.Add(new JSONString("0"));
                range.Add(new JSONString("1"));
            } else if(channel == "size")
            {
                // TODO: Get min and max size of mark.
                throw new Exception("Not implemented yet.");
            } else if(channel == "color")
            {
                // TODO: Set default colors.
                throw new Exception("Not implemented yet.");
            }

            scaleSpecsObj.Add("range", range);
        }

        private void InferDomain(string field, string fieldDataType, JSONNode sceneSpecs, ref JSONObject scaleSpecsObj, Data data)
        {
            JSONArray domain = new JSONArray();
            if (fieldDataType == "quantitative")
            {
                List<float> minMax = new List<float>();
                GetExtent(data, field, ref minMax);
                // For positive minimum values, set the baseline to zero.
                // TODO: Handle logarithmic scale with undefined 0 value.
                if(minMax[0] >= 0)
                {
                    minMax[0] = 0;
                }
                domain.Add(new JSONString(minMax[0].ToString()));
                domain.Add(new JSONString(minMax[1].ToString()));
            } else
            {
                List<string> uniqueValues = new List<string>(); 
                GetUniqueValues(data, field, ref uniqueValues);

                foreach(string val in uniqueValues)
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
            if(channel == "x" || channel == "y" || channel == "z" ||
                channel == "size" || channel == "opacity")
            {
                if(fieldDataType == "nominal" || fieldDataType == "ordinal")
                {
                    type = "band";
                } else if(fieldDataType == "quantitative")
                {
                    type = "linear";
                } else if(fieldDataType == "temporal")
                {
                    type = "time";
                } else
                {
                    throw new Exception("Invalid field data type: " + fieldDataType);
                }
            } else if(channel == "width" || channel == "height" || channel == "depth")
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
            } else if(channel == "color")
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
            } else if(channel == "shape")
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
                throw new Exception("Invalid channel " + channel);
            }

            scaleSpecsObj.Add("type", new JSONString(type));
        }

        private void WriteStringToFile(string str, string outputName)
        {
            StreamWriter writer = new StreamWriter(outputName);
            writer.Write(str);
            writer.Close();
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
            float pos = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

            Vector3 localPos = gameObject.transform.localPosition;
            localPos[dim] = pos;
            gameObject.transform.localPosition = localPos;
        }

        private void SetSize(string value, int dim)
        {
            float size = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

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
            float offset = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
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

        private void SetMaxSize(string value)
        {
            float size = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

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
