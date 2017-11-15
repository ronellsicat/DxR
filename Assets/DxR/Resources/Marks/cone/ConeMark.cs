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
        public Vector3 origOrientation;
        public Vector3 curOrientation;

        public ConeMark() : base("cone")
        {
            origOrientation = curOrientation = Vector3.up;
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
            // Set target direction dim to normalized size.
            Vector3 targetOrient = Vector3.zero;
            targetOrient[vectorIndex] = float.Parse(value);
            targetOrient.Normalize();

            // Copy coordinate to current orientation and normalize.
            curOrientation[vectorIndex] = targetOrient[vectorIndex];
            curOrientation.Normalize();

            Quaternion rotation = Quaternion.FromToRotation(origOrientation, curOrientation);
            transform.rotation = rotation;
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
