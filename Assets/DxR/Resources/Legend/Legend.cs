using System;
using System.Collections;
using System.Collections.Generic;
using DxR;
using SimpleJSON;
using UnityEngine;

public class Legend : MonoBehaviour {
    
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateSpecs(JSONNode legendSpecs, ref DxR.ChannelEncoding channelEncoding, GameObject markPrefab)
    {
        // Create title:
        if (legendSpecs["title"] != null)
        {
            gameObject.GetComponent<Legend>().SetTitle(legendSpecs["title"].Value);
        }

        if(legendSpecs["type"] == "symbol")
        {
            // Create values:
            ConstructValues(legendSpecs, ref channelEncoding, markPrefab);
        } else if(legendSpecs["type"] == "gradient")
        {
            // TODO:
        }

        // Orient legend:
        if (legendSpecs["orient"] != null && legendSpecs["face"] != null)
        {
            if(legendSpecs["x"] != null && legendSpecs["x"] != null && legendSpecs["x"] != null)
            {
                gameObject.GetComponent<Legend>().SetOrientation(legendSpecs["orient"].Value, legendSpecs["face"].Value, 
                    legendSpecs["x"].AsFloat, legendSpecs["y"].AsFloat, legendSpecs["z"].AsFloat);
            }
            else
            {
                gameObject.GetComponent<Legend>().SetOrientation(legendSpecs["orient"].Value, legendSpecs["face"].Value,
                    0,0,0);
            }
        }
        else
        {
            throw new Exception("Legend requires both orient and face specs.");
        }
    }

    private void ConstructValues(JSONNode legendSpecs, ref ChannelEncoding channelEncoding, GameObject markPrefab)
    {
        GameObject legendValuePrefab = Resources.Load("Legend/LegendValue", typeof(GameObject)) as GameObject;
        if (channelEncoding.channel == "color")
        {
            foreach(string domainValue in channelEncoding.scale.domain)
            {
                // Create container for mark + label:
                GameObject legendValueInstance = Instantiate(legendValuePrefab, gameObject.transform.position,
                        gameObject.transform.rotation, gameObject.transform);

                // Create mark instance:
                GameObject markInstance = Instantiate(markPrefab, legendValueInstance.transform.position,
                        legendValueInstance.transform.rotation, legendValueInstance.transform);

                // Apply channel value for this domain:
                string channelValue = channelEncoding.scale.ApplyScale(domainValue);
                Mark markComponent = markInstance.GetComponent<Mark>();
                markComponent.SetChannelValue(channelEncoding.channel, channelValue);

                // Assign mark and label:
                legendValueInstance.GetComponent<LegendValue>().SetTitle(domainValue);
                legendValueInstance.GetComponent<LegendValue>().SetMark(markInstance);

                // Update the collection.
                legendValueInstance.GetComponent<HoloToolkit.Unity.Collections.ObjectCollection>().UpdateCollection();
            }

            gameObject.GetComponent<HoloToolkit.Unity.Collections.ObjectCollection>().Rows = channelEncoding.scale.domain.Count + 1;
            gameObject.GetComponent<HoloToolkit.Unity.Collections.ObjectCollection>().UpdateCollection();
        }
    }

    public void SetTitle(string title)
    {
        gameObject.GetComponentInChildren<TextMesh>().text = title;
    }

    // TODO: Support all possible orientations.
    internal void SetOrientation(string orient, string face, float x, float y, float z)
    {
        gameObject.GetComponentInChildren<TextMesh>().anchor = TextAnchor.UpperLeft;
        gameObject.transform.localPosition = new Vector3(x, y, z) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
    }  
}
