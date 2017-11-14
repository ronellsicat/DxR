using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace DxR
{
    public class Scale
    {
        public string scaleType;       // Type or name of scale (e.g., "linear", "time").
        public List<string> domain;
        public List<string> range;
        
        public Scale(JSONNode scaleSpecs)
        {
            domain = new List<string>();
            range = new List<string>();
        }

        public virtual string ApplyScale(string domainValue)
        {
            string rangeValue = "";
            return rangeValue;
        }
    }
}
