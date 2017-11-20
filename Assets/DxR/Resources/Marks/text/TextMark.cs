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
    public class TextMark : Mark
    {
        public TextMark() : base("text")
        {
            
        }
        
        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "text":
                    SetText(value);
                    break;
                case "x":
                    TranslateBy(value, 0);
                    break;
                case "x2":
                    throw new Exception("x2 is not a bar channel - use x, and width instead.");
                case "y":
                    TranslateBy(value, 1);
                    break;
                case "y2":
                    throw new Exception("y2 is not a bar channel - use y, and height instead.");
                case "z":
                    TranslateBy(value, 2);
                    break;
                case "z2":
                    throw new Exception("z2 is not a bar channel - use z, and depth instead.");
                case "size":
                    SetSize(value);
                    break;
                case "color":
                    SetColor(value);
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
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        private void SetText(string value)
        {
            gameObject.GetComponent<TextMesh>().text = value;
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

        private void SetSize(string value)
        {       
            gameObject.GetComponent<TextMesh>().fontSize = int.Parse(value);
        }

        private void SetColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            gameObject.GetComponent<TextMesh>().color = color;
        }
        
        private void SetOffset(string value, int dim)
        {
            Vector3 translateBy = transform.localPosition;
            translateBy[dim] = float.Parse(value) - translateBy[dim];
            transform.localPosition = translateBy;
        }
    }

}
