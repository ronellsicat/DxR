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
                case "radius":
                    SetRadius(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        /// <summary>
        /// This is an example of a method for setting channel values
        /// that are specific to this mark (versus generic ones in Mark base class).
        /// </summary>
        private void SetRadius(string value)
        {
            float r = float.Parse(value);
            gameObject.transform.localScale = new Vector3(r, r, r);
        }
    }

}
