using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace DxR
{
    class ScaleSequential : Scale
    {
        string rangeType = DxR.SceneObject.UNDEFINED;

        public ScaleSequential(JSONNode scaleSpecs) : base(scaleSpecs)
        {
            // Load color scheme if specified
            if (scaleSpecs["scheme"] != null)
            {
                LoadColorScheme(scaleSpecs["scheme"].Value.ToString(), ref base.range);
            }

            if(base.domain.Count > base.range.Count)
            {
                throw new Exception("Cannot have sequential scale with more domain entries than range entries.");
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

        public override float GetDomainPct(string domainValue)
        {
            float value = float.Parse(domainValue);
            
            float startValue = float.Parse(base.domain[0]);
            float endValue = float.Parse(base.domain[base.domain.Count - 1]);

            float pct = (value - startValue) / (endValue - startValue);

            return pct;
        }

        public override string ApplyScale(string domainValue)
        {
            int endIndex = 0;

            float value = float.Parse(domainValue);

            // Clamping of value to min and max of domain is applied.
            if(value <= float.Parse(base.domain[0]))
            {
                return base.range[0];
            }

            if(value >= float.Parse(base.domain[base.domain.Count - 1]))
            {
                return base.range[base.domain.Count - 1];
            }

            for(int i = 0; i < base.domain.Count; i++)
            {
                if(value <= float.Parse(base.domain[i]))
                {
                    endIndex = i;
                    break;
                }
            }

            if(endIndex == 0 || endIndex >= base.domain.Count)
            {
                throw new Exception("Invalid end index");
            }

            float startValue = float.Parse(base.domain[endIndex - 1]);

            float pct = (value - startValue) / (float.Parse(base.domain[endIndex]) - startValue);

            Color startColor;
            Color endColor;

            ColorUtility.TryParseHtmlString(base.range[endIndex - 1], out startColor);
            ColorUtility.TryParseHtmlString(base.range[endIndex], out endColor);

            Color lerpedColor = Color.Lerp(startColor, endColor, pct);

            string col = "#" + ColorUtility.ToHtmlStringRGB(lerpedColor);

            return col;       
        }
    }
}