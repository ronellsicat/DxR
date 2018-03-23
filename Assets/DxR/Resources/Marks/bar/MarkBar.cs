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
    public class MarkBar : Mark
    {
        public MarkBar() : base()
        {

        }

        //public override List<string> GetChannelsList()
        //{
        //    List<string> myChannels = new List<string>() { "size" };
        //    myChannels.AddRange(base.GetChannelsList());

        //    return myChannels;
        //}


        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "width":
                    base.SetChannelValue(channel, value);
                    base.SetChannelValue("xoffsetpct", "-0.5");
                    break;
                case "height":
                    base.SetChannelValue(channel, value);
                    base.SetChannelValue("yoffsetpct", "-0.5");
                    break;
                case "depth":
                    base.SetChannelValue(channel, value);
                    base.SetChannelValue("zoffsetpct", "-0.5");
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        private void SetSize()
        {
            gameObject.transform.localScale = gameObject.transform.localScale * 0.5f;

        }

        //InferMarkSpecificSpecs
    }
}
