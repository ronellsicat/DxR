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
    public class ConeMark : Mark
    {
        public ConeMark() : base("cone")
        {
            
        }
        
        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "height":
                    SetHeight(value);
                    break;
                case "color":
                    SetColor(value);
                    break;
                case "xorient":
                    SetOrient(value, 0);
                    break;
                case "yorient":
                    SetOrient(value, 1);
                    break;
                case "zorient":
                    SetOrient(value, 2);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        // vectorIndex = 0 for x, 1 for y, 2 for z
        private void SetOrient(string value, int vectorIndex)
        {
            if (float.Parse(value) == 0) return;
            
            Vector3 initOrient = Vector3.up;

            Vector3 targetOrient = Vector3.zero;
            targetOrient[vectorIndex] = float.Parse(value);
            targetOrient.Normalize();

            Vector3 axis = Vector3.forward;
            if(vectorIndex == 2)
            {
                axis = Vector3.right;
            }

            float rot = -Vector3.SignedAngle(targetOrient, initOrient, axis);

            Vector3 localRot = gameObject.transform.localEulerAngles;

            if(vectorIndex == 2)
            {
                localRot.x = rot;
            } else
            {
                localRot.z = rot;
            }
            
            gameObject.transform.localEulerAngles = localRot;
        }

        // Sets the diameter of the point to the value.
        private void SetHeight(string value)
        {
            /*
            float d = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

            Vector3 renderSize = gameObject.transform.GetComponent<Renderer>().bounds.size;
            Vector3 localScale = gameObject.transform.localScale;

            float origSize = renderSize.x / localScale.x;
            float newLocalScale = (d / origSize);

            gameObject.transform.localScale = new Vector3(newLocalScale,
                newLocalScale, newLocalScale);
                */
        }

        private void SetColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            transform.GetComponent<Renderer>().material.color = color;
        }
    }

}
