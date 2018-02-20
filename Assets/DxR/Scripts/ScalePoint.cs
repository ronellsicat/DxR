using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

namespace DxR
{
    public class ScalePoint : Scale
    {
        private bool verbose = true;

        public static float PADDING_DEFAULT = 0.05f;

        public float paddingOuter = 0.0f;

        public float rangeStep = 100.0f;
        public float rangeMin = 0.0f;
        public float rangeMax = 100.0f;
        
        private float paddingOuterSize = 0.05f;

        public ScalePoint(JSONNode scaleSpecs) : base(scaleSpecs) {

            // TODO: Check validity of parameters.

            if (scaleSpecs["padding"] != null)
            {
                paddingOuter = scaleSpecs["padding"].AsFloat;
            }
             else if (scaleSpecs["paddingOuter"] != null)
            {
                paddingOuter = scaleSpecs["paddingOuter"].AsFloat;
            } else
            {
                paddingOuter = PADDING_DEFAULT;
            }
            
            rangeMin = float.Parse(base.range[0]);
            rangeMax = float.Parse(base.range[1]);

            int numSteps = base.domain.Count;
            float tempStepSize = (rangeMax - rangeMin) / (float)(numSteps);

            paddingOuterSize = tempStepSize * paddingOuter;

            rangeStep = ((rangeMax - rangeMin) - (paddingOuterSize * 2.0f)) / (float)(numSteps);
            
            if (verbose)
            {
                Debug.Log("ScalPoint created with " + numSteps.ToString() + " steps. " + 
                    rangeStep.ToString() + " rangeStep");
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
                rangeValue = rangeValue + ((float)(domainValueIndex) * rangeStep) + (rangeStep / 2.0f); ;
            }

            Debug.Log("Scaling " + domainValue + " to " + rangeValue.ToString());

            return rangeValue.ToString();
        }
    }
}
