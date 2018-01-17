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
    public class PaperplaneMark : Mark
    {
        public PaperplaneMark() : base()
        {
            origOrientation = curOrientation = Vector3.left;
        }
        
        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "length":
                    base.SetMaxSize(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }
    }
}
