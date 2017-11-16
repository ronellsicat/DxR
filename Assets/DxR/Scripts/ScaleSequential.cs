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
            // TODO:
            return "";       
        }
    }
}