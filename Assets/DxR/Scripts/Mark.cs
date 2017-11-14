using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DxR
{
    /// <summary>
    /// Base class for Mark classes (e.g., PointMark for point mark).
    /// Contains methods for setting common mark channels such as position and size.
    /// </summary>
    public class Mark : MonoBehaviour
    {
        public string markName; 
 
        public Mark(string markName)
        {
            this.markName = markName;
        }

        public virtual void SetChannelValue(string channel, string value)
        {
            switch(channel)
            {
                case "x":
                    SetX(value);
                    break;
                case "y":
                    SetY(value);
                    break;
                case "z":
                    SetZ(value);
                    break;
                default:
                    throw new System.Exception("Cannot find channel: " + channel);
            }
        }

        private void SetX(string value)
        {
            //Debug.Log("setting x value");
            float targetX = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            float x = gameObject.transform.localPosition.x;
            transform.Translate(targetX - x, 0, 0);
        }

        private void SetY(string value)
        {
            //Debug.Log("setting y value");
            float targetY = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            float y = gameObject.transform.localPosition.y;
            gameObject.transform.Translate(0, targetY - y, 0);
        }

        private void SetZ(string value)
        {
            //Debug.Log("setting z value");
            float targetZ = float.Parse(value) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            float z = gameObject.transform.localPosition.z;
            transform.Translate(0, 0, targetZ - z);
        }

    }

}
