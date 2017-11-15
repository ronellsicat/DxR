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
    public class PointMark : Mark
    {
        public PointMark() : base("point")
        {
            
        }
        
        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "size":
                    SetSize(value);
                    break;
                case "color":
                    SetColor(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        // Sets the diameter of the point to the value.
        private void SetSize(string value)
        {
            float d = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

            Vector3 renderSize = gameObject.transform.GetComponent<Renderer>().bounds.size;
            Vector3 localScale = gameObject.transform.localScale;

            float origSize = renderSize.x / localScale.x;
            float newLocalScale = (d / origSize);

            gameObject.transform.localScale = new Vector3(newLocalScale,
                newLocalScale, newLocalScale);
        }

        private void SetColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            transform.GetComponent<Renderer>().material.color = color;
        }
    }

}
