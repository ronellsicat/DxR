using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DxR
{
    /// <summary>
    /// Base class for Mark classes (e.g., MarkPoint for point mark).
    /// Contains methods for setting common mark channels such as position and size.
    /// </summary>
    public class Mark : MonoBehaviour
    {
        public string markName = "base";
        public Dictionary<string, string> datum = null;
        public GameObject tooltip = null;
        private string tooltipString = DxR.SceneObject.UNDEFINED;

        public Mark(string markName)
        {
            this.markName = markName;           
        }

        public void Start()
        {
            DxR.GazeResponder sc = gameObject.AddComponent(typeof(DxR.GazeResponder)) as DxR.GazeResponder;
        }

        public virtual void SetChannelValue(string channel, string value)
        {
            switch(channel)
            {
                case "x":
                    TranslateBy(value, 0);
                    break;
                case "y":
                    TranslateBy(value, 1);
                    break;
                case "z":
                    TranslateBy(value, 2);
                    break;
                 case "width":
                    SetSize(value, 0);
                    break;
                case "height":
                    SetSize(value, 1);
                    break;
                case "depth":
                    SetSize(value, 2);
                    break;
                case "xoffset":
                    SetOffset(value, 0);
                    break;
                case "yoffset":
                    SetOffset(value, 1);
                    break;
                case "zoffset":
                    SetOffset(value, 2);
                    break;
                case "xoffsetpct":
                    SetOffsetPct(value, 0);
                    break;
                case "yoffsetpct":
                    SetOffsetPct(value, 1);
                    break;
                case "zoffsetpct":
                    SetOffsetPct(value, 2);
                    break;
                case "color":
                    SetColor(value);
                    break;
                case "opacity":
                    SetOpacity(value);
                    break;
                case "size":
                    SetMaxSize(value);
                    break;
                case "xrotation":
                    SetRotation(value, 0);
                    break;
                case "yrotation":
                    SetRotation(value, 1);
                    break;
                case "zrotation":
                    SetRotation(value, 2);
                    break;
                case "tooltip":
                    SetTooltip(value);
                    break;
                case "x2":
                    throw new System.Exception("x2 is not a valid channel - use x, and width instead.");
                case "y2":
                    throw new System.Exception("y2 is not a valid channel - use y, and height instead.");
                case "z2":
                    throw new System.Exception("z2 is not a valid channel - use z, and depth instead.");
                default:
                    throw new System.Exception("Cannot find channel: " + channel);
            }
        }

        public void SetTooltipObject(ref GameObject tooltipObject)
        {
            tooltip = tooltipObject;
        }

        private void SetTooltip(string value)
        {
            tooltipString = value;
        }

        private void TranslateBy(string value, int dim)
        {
            // TODO: Do this more robustly.
            float pos = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

            Vector3 localPos = gameObject.transform.localPosition;
            Vector3 translateBy = Vector3.zero;
            translateBy[dim] = pos - localPos[dim];
            gameObject.transform.Translate(translateBy);
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

        private void SetOffset(string value, int dim)
        {
            float offset = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            Vector3 translateBy = transform.localPosition;
            translateBy[dim] = offset - translateBy[dim];
            transform.localPosition = translateBy;
        }

        private void SetOffsetPct(string value, int dim)
        {
            GetComponent<MeshFilter>().mesh.RecalculateBounds();
            float offset = float.Parse(value) * GetComponent<MeshFilter>().mesh.bounds.size[dim] *
                gameObject.transform.localScale[dim];
            Vector3 translateBy = transform.localPosition;
            translateBy[dim] = offset - translateBy[dim];
            transform.localPosition = translateBy;
        }

        private void SetRotation(string value, int dim)
        {
            Vector3 rot = transform.localEulerAngles;
            rot[dim] = float.Parse(value);
            transform.localEulerAngles = rot;
        }

        private void SetMaxSize(string value)
        {
            float size = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

            Vector3 renderSize = gameObject.transform.GetComponent<Renderer>().bounds.size;
            Vector3 localScale = gameObject.transform.localScale;

            int maxIndex = 0;
            float maxSize = renderSize[maxIndex];
            for(int i = 1; i < 3; i++)
            {
                if(maxSize < renderSize[i])
                {
                    maxSize = renderSize[i];
                    maxIndex = i;
                }
            }

            float origMaxSize = renderSize[maxIndex] / localScale[maxIndex];
            float newLocalScale = (size / origMaxSize);

            gameObject.transform.localScale = new Vector3(newLocalScale,
                newLocalScale, newLocalScale);
        }

        private void SetColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            transform.GetComponent<Renderer>().material.color = color;
        }

        private void SetOpacity(string value)
        {
            SetRenderModeToTransparent(transform.GetComponent<Renderer>().material);
            Color color = transform.GetComponent<Renderer>().material.color;
            color.a = float.Parse(value);
            transform.GetComponent<Renderer>().material.color = color;
        }

        private void SetRenderModeToTransparent(Material m)
        {
            m.SetFloat("_Mode", 2);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
        }

        public void OnFocusEnter()
        {
            Debug.Log("Mark focus entered.");
            // TODO: Get tooltip pointer somehow. 

            tooltip = gameObject.transform.parent.Find("tooltip").gameObject;

            if(tooltip != null)
            {
                tooltip.SetActive(true);

                Vector3 markPos = gameObject.transform.localPosition;

                tooltip.GetComponent<Tooltip>().SetChannelValue("text", tooltipString);
                tooltip.GetComponent<Tooltip>().SetChannelValue("x", markPos.x.ToString());
                tooltip.GetComponent<Tooltip>().SetChannelValue("y", markPos.y.ToString());
                tooltip.GetComponent<Tooltip>().SetChannelValue("z", markPos.z.ToString());

                Debug.Log("Displaying tooltip " + tooltipString);
            }
        }

        public void OnFocusExit()
        {
            Debug.Log("Mark focus exited.");

            if (tooltip != null)
            {
 //               tooltip.SetActive(false);
            }
        }
    }
}
