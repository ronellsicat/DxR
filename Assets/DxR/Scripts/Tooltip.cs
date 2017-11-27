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
    public class Tooltip : MonoBehaviour
    {
        public Tooltip()
        {
            
        }
        
        public void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "text":
                    SetText(value);
                    break;
                case "x":
                    TranslateBy(value, 0);
                    break;
                case "y":
                    TranslateBy(value, 1);
                    break;
                case "z":
                    TranslateBy(value, 2);
                    break;
                case "size":
                    SetFontSize(value);
                    break;
                case "color":
                    SetFontColor(value);
                    break;
                case "anchor":
                    SetAnchor(value);
                    break;
                default:
                    throw new Exception("Invalid tooltip channel.");
            }
        }

        private void SetText(string value)
        {
            gameObject.GetComponent<TextMesh>().text = value;

            Debug.Log("setting tooltip to " + value);
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

        private void SetFontSize(string value)
        {       
            gameObject.GetComponent<TextMesh>().fontSize = int.Parse(value);
        }

        private void SetFontColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            gameObject.GetComponent<TextMesh>().color = color;
        }

        private void SetAnchor(string value)
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
