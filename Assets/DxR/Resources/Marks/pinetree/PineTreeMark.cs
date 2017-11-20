using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DxR
{
    /// <summary>
    /// This is the class for point mark which enables setting of channel
    /// values which may involve calling custom scripts. The idea is that 
    /// in order to add a custom channel, the developer simply has to implement
    /// a function that takes in the "channel" name and value in string format
    /// and performs the necessary changes under the SetChannelValue function.
    /// </summary>
    public class PineTreeMark : Mark
    {
        public PineTreeMark() : base("pinetree")
        {
            
        }

        public void Start()
        {
            
        }

        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "x":
                    SetX(value);
                    break;
                case "x2":
                    // TODO:
                    throw new Exception("Channel x2 not implemented.");
                    break;
                case "y":
                    SetY(value);
                    break;
                case "y2":
                    // TODO:
                    throw new Exception("Channel y2 not implemented.");
                case "yoffset":
                    SetOffsetY(value);
                    break;
                case "bandwidth":
                case "width":
                    SetSize(value, 0);
                    break;
                case "depth":
                    SetSize(value, 2);
                    break;
                case "color":
                    SetColor(value);
                    break;
                case "yrotation":
                    SetRotation(value, 1);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        private void SetSize(string value, int dim)
        {
            float size = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

            Vector3 initPos = transform.localPosition;

            Vector3 curScale = transform.localScale;
            GetComponent<MeshFilter>().mesh.RecalculateBounds();
            Vector3 origMeshSize = GetComponent<MeshFilter>().mesh.bounds.size;
            curScale[dim] = size / (origMeshSize[dim]);
            transform.localScale = curScale;

            transform.localPosition = initPos;  // This handles models that get translated with scaling.
        }

        private void SetRotation(string value, int dim)
        {
            Vector3 rot = transform.localEulerAngles;
            rot[dim] = float.Parse(value);
            transform.localEulerAngles = rot;
        }

        private void SetWidth(string value)
        {
            float width = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

            Vector3 curScale = transform.localScale;
            GetComponent<MeshFilter>().mesh.RecalculateBounds();
            Vector3 origMeshSize = GetComponent<MeshFilter>().mesh.bounds.size;
            curScale.x = width / (origMeshSize.x);
            transform.localScale = curScale;
        }

        private void SetX(string value)
        {
            // TODO: Do this more robustly.
            float xpos = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            Debug.Log("Translating cube by " + xpos.ToString());

            float x = gameObject.transform.localPosition.x;
            transform.Translate(xpos - x, 0, 0);
        }

        // This function assumes the position handle (pivot) of the object is in its center.
        private void SetY(string value)
        {
            Vector3 initPos = transform.localPosition;

            float height = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            Vector3 curScale = transform.localScale;
            GetComponent<MeshFilter>().mesh.RecalculateBounds();
            Vector3 origMeshSize = GetComponent<Renderer>().bounds.size;
            curScale.y = height / (origMeshSize.y);
            transform.localScale = curScale;

            //Vector3 localPos = transform.localPosition;
            // This assumes the position handle of the object is in its center.
            //transform.localPosition = new Vector3(localPos.x, height / 4.0f, localPos.z);
            //transform.Translate(0, height / 4.0f, 0);
            transform.localPosition = initPos;
            transform.Translate(0, height / 2.0f, 0);
        }

        private void SetColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            transform.GetComponent<Renderer>().material.color = color;
        }

        private void SetOffsetY(string value)
        {
            GetComponent<MeshFilter>().mesh.RecalculateBounds();
            float offset = float.Parse(value) * GetComponent<MeshRenderer>().bounds.size.y;
            transform.Translate(0, offset, 0);
            Debug.Log("Applying y offset " + offset.ToString());
        }
    }

}
