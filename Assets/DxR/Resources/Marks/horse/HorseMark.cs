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
    public class HorseMark : Mark
    {
        public HorseMark() : base()
        {
            origOrientation = curOrientation = Vector3.left;
        }
        
        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "size":
                    SetSize(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }
        
        // vectorIndex = 0 for x, 1 for y, 2 for z
        private void SetOrientation(string value, int vectorIndex)
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
        private void SetSize(string value)
        {
            float size = float.Parse(value) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
            gameObject.transform.localScale = new Vector3(size, size, size);
        }
    }
}
