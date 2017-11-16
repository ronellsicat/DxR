using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace DxR
{
    class ScaleOrdinal : Scale
    {
        string rangeType = DxR.SceneObject.UNDEFINED;

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
                LoadColorScheme(scaleSpecs["scheme"].Value.ToString(), ref base.range);
            } else
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
    }
}