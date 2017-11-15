using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

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

            CopyNodeToList(scaleSpecs["domain"], ref domain);
            CopyNodeToList(scaleSpecs["range"], ref range);
        }

        private void CopyNodeToList(JSONNode node, ref List<string> list)
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
    }
}
