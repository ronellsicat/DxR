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
    public class MarkMilk : Mark
    {
        public MarkMilk() : base()
        {
            
        }

        //public override List<string> GetChannelsList()
        //{
        //    //List<string> myChannels = new List<string>() { "size" };
        //    //myChannels.AddRange(base.GetChannelsList());

        //    return myChannels;
        //}


        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "size":
                    SetSize(value);
                    break;
                case "color":
                    SetColor(value);
                    break;
                case "sugar":
                    SetSugar(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }
        private void SetSugar(string value)
        {
            int number = int.Parse(value);
            GameObject sugar = gameObject.transform.Find("sugar").gameObject;
            GameObject milk = gameObject.transform.Find("contents").gameObject;

            float milk_height=milk.transform.lossyScale.z*0.4f;

            for (int i = 0; i < number; i++)
            {
                GameObject t=Instantiate(sugar, new Vector3(sugar.transform.position.x, sugar.transform.position.y + 0.025f*i, sugar.transform.position.z), sugar.transform.rotation, transform);
                t.transform.position = new Vector3(t.transform.position.x, t.transform.position.y+ milk_height, t.transform.position.z);
                t.SetActive(true);
            }
        }
        private void SetColor(string value)
        {
            MeshRenderer contents = gameObject.transform.Find("contents").gameObject.GetComponent<MeshRenderer>();
            if (contents != null)
            {
//                contents.material.SetColor("_EmissionColor", Color.red);

                Color color;
                bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
                if (!colorParsed) return;
                Debug.Log(String.Format("{0},{1},{2}", color.r, color.g, color.b));
                contents.material.SetColor("_Color", color);
                
            }
            
        }

        private void SetSize(string value)
        {

            GameObject contents = gameObject.transform.Find("contents").gameObject;
            float size = float.Parse(value);

            Vector3 initPos = contents.transform.localPosition;

            Vector3 curScale = contents.transform.localScale;

            curScale[2] = curScale[2] * size;
            contents.transform.localScale = curScale;

            contents.transform.localPosition = initPos;  // This handles models that get translated with scaling.
        }
    }
}
