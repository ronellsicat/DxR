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
    public class MarkGlobebar : Mark
    {
        private float lng = 0.0f;
        private float lat = 0.0f;

        public MarkGlobebar() : base()
        {
            
        }
        
        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "latitute":
                case "lat":
                    SetLatituteValue(value);
                    break;
                case "longitude":
                case "long":
                    SetLongitudeValue(value);
                    break;
                case "length":
                    SetLength(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        private void SetLatituteValue(string value)
        {
            lat = float.Parse(value);
            UpdatePos();
        }

        private void SetLongitudeValue(string value)
        {
            lng = float.Parse(value);
            UpdatePos();
        }

        private void UpdatePos()
        {
            Vector3 pos;
            pos.x = 0.5f * Mathf.Cos((lng) * Mathf.Deg2Rad) * Mathf.Cos(lat * Mathf.Deg2Rad);
            pos.y = 0.5f * Mathf.Sin(lat * Mathf.Deg2Rad);
            pos.z = 0.5f * Mathf.Sin((lng) * Mathf.Deg2Rad) * Mathf.Cos(lat * Mathf.Deg2Rad);

            gameObject.transform.localPosition = pos;
            gameObject.transform.LookAt(gameObject.transform.parent.position + (pos * 2));
        }

        private void SetLength(string value)
        {
            gameObject.transform.localScale = new Vector3(1, 1, Mathf.Max(0.001f, float.Parse(value)));
        }
    }
}
