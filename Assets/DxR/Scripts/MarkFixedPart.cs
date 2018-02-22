using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using System.IO;

namespace DxR
{
    public class MarkFixedPart : MonoBehaviour
    {
        Renderer cur_renderer=null;

        private void Awake()
        {
        }

        public void Start()
        {
            cur_renderer = transform.GetComponent<Renderer>();
            if (cur_renderer != null)
            {
                Renderer m_parent = transform.parent.GetComponent<Renderer>();
                cur_renderer.material = m_parent.material;
            }

            // Todo: set size and location to original.//
            Vector3 initPos = transform.localPosition;
            transform.localScale.Scale(new Vector3((1.0f)/transform.parent.transform.localScale.x, (1.0f) / transform.parent.transform.localScale.y, (1.0f)/transform.parent.transform.localScale.z));

            transform.localPosition = initPos;
            /////////////////////////////////////////////
        }

        private void SetSize(string value, int dim)
        {
//            float size = float.Parse(value) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

//            Vector3 initPos = transform.localPosition;

//            Vector3 curScale = transform.localScale;

//            GetComponent<MeshFilter>().mesh.RecalculateBounds();
//            Vector3 origMeshSize = GetComponent<MeshFilter>().mesh.bounds.size;
//            curScale[dim] = size / (origMeshSize[dim]);
//            transform.localScale = curScale;

////            transform.localPosition = initPos;  // This handles models that get translated with scaling.
        }        
    }
}
