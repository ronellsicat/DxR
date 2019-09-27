using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DxR
{
    /// <summary>
    /// </summary>
    public class Tooltip : MonoBehaviour
    {
        public Tooltip()
        {
            
        }
        
        public void SetText(string value)
        {
            gameObject.GetComponent<TextMesh>().text = value;
        }

        public void SetLocalPos(float pos, int dim)
        {
            Vector3 localPos = gameObject.transform.localPosition;
            localPos[dim] = pos;
            gameObject.transform.localPosition = localPos;
        }

        public void SetFontSize(string value)
        {       
            gameObject.GetComponent<TextMesh>().fontSize = int.Parse(value);
        }

        public void SetFontColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            gameObject.GetComponent<TextMesh>().color = color;
        }

        public void SetAnchor(string value)
        {
            TextAnchor anchor = TextAnchor.MiddleCenter;
            switch(value)
            {
                case "upperleft":
                    anchor = TextAnchor.UpperLeft;
                    break;
                case "uppercenter":
                    anchor = TextAnchor.UpperCenter;
                    break;
                case "upperright":
                    anchor = TextAnchor.UpperRight;
                    break;
                case "middleleft":
                    anchor = TextAnchor.MiddleLeft;
                    break;
                case "middlecenter":
                    anchor = TextAnchor.MiddleCenter;
                    break;
                case "middleright":
                    anchor = TextAnchor.MiddleRight;
                    break;
                case "lowerleft":
                    anchor = TextAnchor.LowerLeft;
                    break;
                case "lowercenter":
                    anchor = TextAnchor.LowerCenter;
                    break;
                case "lowerright":
                    anchor = TextAnchor.LowerRight;
                    break;
                default:
                    anchor = TextAnchor.MiddleCenter;
                    break;
            }

            gameObject.GetComponent<TextMesh>().anchor = anchor;
        }
    }
}
