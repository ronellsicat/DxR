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
    public class MarkText : Mark
    {
        public MarkText() : base("text")
        {
            
        }
        
        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "text":
                    SetText(value);
                    break;
                case "size":
                    SetFontSize(value);
                    break;
                case "color":
                    SetFontColor(value);
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
        
        private void SetFontSize(string value)
        {       
            gameObject.GetComponent<TextMesh>().fontSize = int.Parse(value);
        }

        private void SetFontColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            gameObject.GetComponent<TextMesh>().color = color;
        }
    }
}
