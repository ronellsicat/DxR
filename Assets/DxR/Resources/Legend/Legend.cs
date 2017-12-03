using System;
using System.Collections;
using System.Collections.Generic;
using DxR;
using SimpleJSON;
using UnityEngine;

public class Legend : MonoBehaviour {

    LineRenderer colorLine = null;

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
            // Create symbols:
            ConstructSymbols(legendSpecs, ref channelEncoding, markPrefab);
        } else if(legendSpecs["type"] == "gradient")
        {
            ConstructGradient(legendSpecs, ref channelEncoding);
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

    // TODO: Get rid of hard coded default values.
    private void ConstructGradient(JSONNode legendSpecs, ref ChannelEncoding channelEncoding)
    {
        colorLine = gameObject.GetComponentInChildren<LineRenderer>(true);

        bool addTicks = false;
        Transform ticks = gameObject.transform.Find("Ticks");
        GameObject tickPrefab = null; 
        if(ticks != null)
        {
            addTicks = true;
            ticks.gameObject.SetActive(true);
            tickPrefab = Resources.Load("Legend/LegendTick") as GameObject;
        }

        if (colorLine == null)
        {
            throw new Exception("Cannot find ColorLine LineRenderer object in legend.");
        }

        colorLine.gameObject.SetActive(true);
        colorLine.material = new Material(Shader.Find("Sprites/Default"));

        float width = 0.2f;
        float height = 0.05f;
        if (legendSpecs["gradientWidth"] == null || legendSpecs["gradientHeight"] == null)
        {
            width = legendSpecs["gradientWidth"].AsFloat * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
            height = legendSpecs["gradientHeight"].AsFloat * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;
        }

        List<Vector3> positionsList = new List<Vector3>();
        List<GradientColorKey> colorKeyList = new List<GradientColorKey>();
        List<GradientAlphaKey> alphaKeyList = new List<GradientAlphaKey>();

        float alpha = 1.0f;
        int domainCount = channelEncoding.scale.domain.Count;
        for (int i = 0; i < domainCount; i++)
        {
            float pct = channelEncoding.scale.GetDomainPct(channelEncoding.scale.domain[i]);
            positionsList.Add(new Vector3(width * pct, 0.0f, 0.0f));
            Color col;
            ColorUtility.TryParseHtmlString(channelEncoding.scale.range[i], out col);
            colorKeyList.Add(new GradientColorKey(col, pct));
            alphaKeyList.Add(new GradientAlphaKey(alpha, pct));

            if(addTicks && tickPrefab != null)
            {
                GameObject tick = Instantiate(tickPrefab, ticks.transform.position, ticks.transform.rotation, ticks.transform);

                Vector3 pos = Vector3.zero;
                pos.x = width * pct;
                pos.y = 0.04f;             // TODO: Get this from text size.
                tick.transform.Translate(pos);

                tick.GetComponent<TextMesh>().text = channelEncoding.scale.domain[i];
            }
        }

        colorLine.positionCount = positionsList.Count;
        colorLine.SetPositions(positionsList.ToArray());
        colorLine.startWidth = height;
        colorLine.endWidth = height;

        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeyList.ToArray(), alphaKeyList.ToArray());    
        colorLine.colorGradient = gradient;

        colorLine.transform.parent = gameObject.transform;

        gameObject.GetComponent<HoloToolkit.Unity.Collections.ObjectCollection>().Rows = 3;             // TODO: Update this if no ticks are shown.
        gameObject.GetComponent<HoloToolkit.Unity.Collections.ObjectCollection>().CellHeight = 0.08f;
        gameObject.GetComponent<HoloToolkit.Unity.Collections.ObjectCollection>().UpdateCollection();
    }

    private void ConstructSymbols(JSONNode legendSpecs, ref ChannelEncoding channelEncoding, GameObject markPrefab)
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
            gameObject.GetComponent<HoloToolkit.Unity.Collections.ObjectCollection>().CellHeight = 0.05f;   // TODO: Set to height of each legendValue.
            gameObject.GetComponent<HoloToolkit.Unity.Collections.ObjectCollection>().UpdateCollection();
        } else if(channelEncoding.channel == "opacity")
        {
            // TODO:
        } else if(channelEncoding.channel == "size")
        {
            // TODO:
        }
        else if(channelEncoding.channel == "shape")
        {
            // TODO:
        }
        else
        {
            throw new Exception("Legend constructor does not know how to construct legend for " + channelEncoding.channel);
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
