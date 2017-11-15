using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace DxR
{
    public class ScaleCustom : Scale
    {
        public ScaleCustom(JSONNode scaleSpecs) : base(scaleSpecs) {

        }
        
        public override string ApplyScale(string domainValue)
        {
            return domainValue;
        }
    }
}
