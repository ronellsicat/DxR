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
    public class MarkRadialBar : Mark
    {
        private float lng = 0.0f;
        private float lat = 0.0f;
        private float radius = 0.5f;

        public MarkRadialBar() : base()
        {
            
        }

        public override List<string> GetChannelsList()
        {
            List<string> myChannels = new List<string>() { "radius", "latitude", "longitude", "length" };
            myChannels.AddRange(base.GetChannelsList());

            return myChannels;
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
                case "radius":
                    SetRadius(value);
                    break;
                case "length":
                    SetLength(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        private void SetRadius(string value)
        {
            radius = float.Parse(value);
            UpdatePos();
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
            pos.x = radius * Mathf.Cos((lng) * Mathf.Deg2Rad) * Mathf.Cos(lat * Mathf.Deg2Rad);
            pos.y = radius * Mathf.Sin(lat * Mathf.Deg2Rad);
            pos.z = radius * Mathf.Sin((lng) * Mathf.Deg2Rad) * Mathf.Cos(lat * Mathf.Deg2Rad);

            gameObject.transform.localPosition = pos;
            gameObject.transform.LookAt(gameObject.transform.parent.position + (pos * 2));
        }

        private void SetLength(string value)
        {
            gameObject.transform.localScale = new Vector3(1, 1, Mathf.Max(0.001f, float.Parse(value)));
        }
    }
}
