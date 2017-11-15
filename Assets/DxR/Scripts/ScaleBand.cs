using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace DxR
{
    public class ScaleBand : Scale
    {
        private bool verbose = true;

        public float paddingOuter = 0.05f;
        public float paddingInner = 0.05f;

        public float stepSize = 100.0f;
        public float rangeMin = 0.0f;
        public float rangeMax = 100.0f;

        public bool isBandwidthSpecified = false;   // Set to true is manually specified in JSON specs.
        public float bandwidth = 1.0f;

        private float paddingOuterSize = 0.05f;
        private float paddingInnerSize = 0.05f;
        

        public ScaleBand(JSONNode scaleSpecs) : base(scaleSpecs) {

            // TODO: Check validity of parameters.
            rangeMin = float.Parse(base.range[0]);
            rangeMax = float.Parse(base.range[1]);

            int numSteps = base.domain.Count;
            float tempStepSize = (rangeMax - rangeMin) / (float)(numSteps);

            paddingInnerSize = tempStepSize * paddingInner;
            paddingOuterSize = tempStepSize * paddingOuter;

            stepSize = ((rangeMax - rangeMin) - (paddingOuterSize * 2.0f)) / (float)(numSteps);

            bandwidth = stepSize - paddingInnerSize;

            if(verbose)
            {
                Debug.Log("ScaleBand created with " + numSteps.ToString() + " steps. " + 
                    stepSize.ToString() + " stepSize, and " + bandwidth.ToString() +
                    " bandwidth.");
            }
        }
        
        public override string ApplyScale(string domainValue)
        {
            float rangeValue = paddingOuterSize;

            int domainValueIndex = base.domain.IndexOf(domainValue);

            if(domainValueIndex == -1)
            {
                throw new System.Exception("Invalid domain value " + domainValue);
            } else
            {
                rangeValue = rangeValue + ((float)(domainValueIndex) * stepSize) + (bandwidth / 2.0f);
            }

            Debug.Log("Scaling " + domainValue + " to " + rangeValue.ToString());

            return rangeValue.ToString();
        }
    }
}
