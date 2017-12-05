using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace DxR
{
    public class ScaleLinear : Scale
    {
        private bool verbose = true;

        public float rangeMin = 0.0f;
        public float rangeMax = 100.0f;

        public float domainMin = 0.0f;
        public float domainMax = 100.0f;

        public ScaleLinear(JSONNode scaleSpecs) : base(scaleSpecs) {

            domainMin = float.Parse(base.domain[0]); 
            domainMax = float.Parse(base.domain[1]);

            rangeMin = float.Parse(base.range[0]);
            rangeMax = float.Parse(base.range[1]);

            if (verbose)
            {
                Debug.Log("Created ScaleLinear object with domain [" +
                    domainMin.ToString() + ", " + domainMax.ToString() +
                    "], range [" + rangeMin.ToString() + ", " + rangeMax.ToString() + "]");
            }
        }
        
        public override string ApplyScale(string domainValue)
        {
            float rangeValue = rangeMin;
            float pct = (float.Parse(domainValue) - domainMin) / (domainMax - domainMin);
            return (rangeValue + (pct * (rangeMax - rangeMin))).ToString();
        }
    }
}
