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
    public class BarMark : Mark
    {
        public BarMark() : base("bar")
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
                    break;
                case "width":
                    SetWidth(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        private void SetWidth(string value)
        {
            // TODO: Fix this.
            float width = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            Vector3 curScale = transform.localScale;
            transform.localScale = new Vector3(width, curScale.y, curScale.z);
        }

        private void SetX(string value)
        {
            // TODO: Do this more robustly.
            float xpos = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            Debug.Log("Translating cube by " + xpos.ToString());

            float x = gameObject.transform.localPosition.x;
            transform.Translate(xpos - x, 0, 0);
        }

        private void SetY(string value)
        {
            // TODO: Do this more robustly.
            float height = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            Vector3 curScale = transform.localScale;
            transform.localScale = new Vector3(curScale.x, height, curScale.z);

            float y = gameObject.transform.localPosition.y;
            transform.Translate(0, height / 2.0f - y, 0);
        }
    }

}
