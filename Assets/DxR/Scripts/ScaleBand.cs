using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

namespace DxR
{
    public class ScaleBand : Scale
    {
        private bool verbose = true;

        public static float PADDING_OUTER_DEFAULT = 0.05f;
        public static float PADDING_INNER_DEFAULT = 0.05f;

        public float paddingOuter = 0.0f;
        public float paddingInner = 0.0f;

        public float rangeStep = 100.0f;
        public float rangeMin = 0.0f;
        public float rangeMax = 100.0f;

        public bool isBandwidthSpecified = false;   // Set to true is manually specified in JSON specs.
        public float bandwidth = 1.0f;

        private float paddingOuterSize = 0.05f;
        private float paddingInnerSize = 0.05f;
        

        public ScaleBand(JSONNode scaleSpecs) : base(scaleSpecs) {

            // TODO: Check validity of parameters.

            if (scaleSpecs["paddingInner"] != null)
            {
                paddingInner = scaleSpecs["paddingInner"].AsFloat;
            } else
            {
                paddingInner = PADDING_INNER_DEFAULT;
            }

            if (scaleSpecs["paddingOuter"] != null)
            {
                paddingOuter = scaleSpecs["paddingOuter"].AsFloat;
            } else
            {
                paddingOuter = PADDING_OUTER_DEFAULT;
            }
            
            rangeMin = float.Parse(base.range[0]);
            rangeMax = float.Parse(base.range[1]);

            int numSteps = base.domain.Count;
            float tempStepSize = (rangeMax - rangeMin) / (float)(numSteps);

            paddingInnerSize = tempStepSize * paddingInner;
            paddingOuterSize = tempStepSize * paddingOuter;

            rangeStep = ((rangeMax - rangeMin) - (paddingOuterSize * 2.0f)) / (float)(numSteps);

            bandwidth = rangeStep - paddingInnerSize;

            if(verbose)
            {
                Debug.Log("ScaleBand created with " + numSteps.ToString() + " steps. " + 
                    rangeStep.ToString() + " rangeStep, and " + bandwidth.ToString() +
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
                rangeValue = rangeValue + ((float)(domainValueIndex) * rangeStep) + (bandwidth / 2.0f);
            }

            Debug.Log("Scaling " + domainValue + " to " + rangeValue.ToString());

            return rangeValue.ToString();
        }
    }
}
