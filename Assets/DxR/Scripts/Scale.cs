using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

namespace DxR
{
    public class Scale
    {
        public List<string> domain;
        public List<string> range;
        
        public Scale(JSONNode scaleSpecs)
        {
            domain = new List<string>();
            range = new List<string>();

            if(scaleSpecs["domain"] != null)
            {
                CopyNodeToList(scaleSpecs["domain"], ref domain);
            } else if(scaleSpecs["type"] != "custom")
            {
                throw new Exception("Scale is missing domain.");
            }

            if (scaleSpecs["range"] != null)
            {
                CopyNodeToList(scaleSpecs["range"], ref range);
            }
            else if(scaleSpecs["type"] != "custom")
            {
                throw new Exception("Scale is missing range.");
            }
        }

        protected void CopyNodeToList(JSONNode node, ref List<string> list)
        {
            if (node == null) return;

            for(int i = 0; i < node.Count; i++)
            {
                list.Add(node[i]);
            }
        }

        public virtual string ApplyScale(string domainValue)
        {
            string rangeValue = "";
            return rangeValue;
        }

        public virtual float GetDomainPct(string domainValue)
        {
            return 0.0f;
        }
    }
}
