using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace DxR
{
    class ScaleOrdinal : Scale
    {
        string rangeType = DxR.Vis.UNDEFINED;

        public ScaleOrdinal(JSONNode scaleSpecs) : base(scaleSpecs)
        {
            if(scaleSpecs["range"] != null)
            {
                rangeType = scaleSpecs["range"];

                switch(rangeType)
                {
                    case "category":
                        SetupScaleCategory(scaleSpecs);
                        break;

                    case "ordinal":
                        SetupScaleOrdinal(scaleSpecs);
                        break;

                    default:
                        throw new System.Exception("Invalid range " + rangeType
                            +" for ordinal scale type.");
                }

            } else
            {
                throw new System.Exception("Missing range in ScaleOrdinal spec.");
            }

        }

        private void SetupScaleCategory(JSONNode scaleSpecs)
        {
            if(scaleSpecs["scheme"] != null)
            {
                if (scaleSpecs["scheme"].IsArray)
                {
                    CopyNodeToList(scaleSpecs["scheme"], ref range);
                }
                else
                {
                    LoadColorScheme(scaleSpecs["scheme"].Value.ToString(), ref base.range);
                }
            } else
            {
                throw new System.Exception("Missing scheme spec in ordinal scale spec.");
            }
        }

        private void SetupScaleOrdinal(JSONNode scaleSpecs)
        {
            if (scaleSpecs["scheme"] != null)
            {
                if(scaleSpecs["scheme"].IsArray)
                {
                    CopyNodeToList(scaleSpecs["scheme"], ref range);
                } else
                {
                    LoadColorScheme(scaleSpecs["scheme"].Value.ToString(), ref base.range);
                }
            }
            else
            {
                throw new System.Exception("Missing scheme spec in ordinal scale spec.");
            }
        }

        private void LoadColorScheme(string schemeName, ref List<string> range)
        {
            string schemeFilename = "ColorSchemes/" + schemeName;
            
            TextAsset targetFile = Resources.Load<TextAsset>(schemeFilename);
            if(targetFile == null)
            {
                throw new Exception("Cannot load color scheme " + schemeFilename);
            }
            
            JSONNode colorSchemeSpec = JSON.Parse(targetFile.text);

            CopyNodeToList(colorSchemeSpec["colors"], ref range);
        }

        public override string ApplyScale(string domainValue)
        {
            switch(rangeType)
            {
                case "category":
                    return ApplyScaleCategory(domainValue);

                case "ordinal":
                    return ApplyScaleOrdinal(domainValue);

                default:
                    throw new Exception("Invalid range type for ordinal scale.");
            }           
        }

        private string ApplyScaleCategory(string domainValue)
        {
            string rangeValue = base.range[0];

            int domainValueIndex = base.domain.IndexOf(domainValue);

            if (domainValueIndex == -1)
            {
                throw new System.Exception("Invalid domain value " + domainValue);
            }
            else
            {
                rangeValue = base.range[domainValueIndex % base.range.Count];
            }

            Debug.Log("Scaling " + domainValue + " to " + rangeValue);

            return rangeValue;
        }

        // This currently only interpolates the output color using the first two
        // colors in the range.
        // TODO: Handle more complex schemes, e.g., blues-3, blues-4, ... 
        // see: https://vega.github.io/vega-lite/docs/scale.html#scheme
        private string ApplyScaleOrdinal(string domainValue)
        {
            string rangeValue = base.range[0];

            int domainValueIndex = base.domain.IndexOf(domainValue);

            if (domainValueIndex == -1)
            {
                throw new System.Exception("Invalid domain value " + domainValue);
            }
            else
            {
                if(domainValueIndex == 0)
                {
                    return base.range[0];
                } else if(domainValueIndex == base.domain.Count - 1)
                {
                    return base.range[1];
                } else
                {
                    float pct = (float)(domainValueIndex) / (float)(base.domain.Count - 1);

                    Color startColor;
                    Color endColor;

                    ColorUtility.TryParseHtmlString(base.range[0], out startColor);
                    ColorUtility.TryParseHtmlString(base.range[1], out endColor);

                    Color lerpedColor = Color.Lerp(startColor, endColor, pct);

                    string col = "#" + ColorUtility.ToHtmlStringRGB(lerpedColor);

                    return col;
                }
            }
        }
    }
}