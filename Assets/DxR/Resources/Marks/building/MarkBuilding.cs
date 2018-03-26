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
    public class MarkBuilding : Mark
    {
        public MarkBuilding() : base()
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
                case "color":
                    SetColor(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        private void SetColor(string value)
        {
            Material[] mats = gameObject.GetComponent<MeshRenderer>().materials;
            
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            if (!colorParsed) return;
            mats[2].SetColor("_Color", color);
        }

        //InferMarkSpecificSpecs
    }
}
