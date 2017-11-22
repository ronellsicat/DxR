using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class Axis : MonoBehaviour {

    private float meshLength = 2.0f;    // This is the initial length of the cylinder used for the axis.
    private float titleOffset = 0.05f;
    
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetTitle(string title)
    {
        gameObject.GetComponentInChildren<TextMesh>().text = title;
    }

    internal void SetOrientation(string orient, string face)
    {
        if(orient == "bottom" && face == "front")
        {
            OrientAlongPositiveX();
        } else if(orient == "left" && face == "front")
        {
            OrientAlongPositiveY();
        } else if(orient == "bottom" && face == "left")
        {
            OrientAlongPositiveZ();
        }
    }  

    private void OrientAlongPositiveX()
    {
        gameObject.transform.localPosition = new Vector3(GetLength() / 2.0f, 0.0f, 0.0f);
        gameObject.transform.Find("Title").Translate(0, -titleOffset, 0);
    }
    
    private void OrientAlongPositiveY()
    {
        gameObject.transform.Rotate(0, 0, 90.0f);
        gameObject.transform.Find("Title").Translate(0, titleOffset, 0);
        gameObject.transform.localPosition = new Vector3(0.0f, GetLength() / 2.0f, 0.0f);
    }

    private void OrientAlongPositiveZ()
    {
        gameObject.transform.Rotate(0, -90.0f, 0);
        gameObject.transform.Find("Title").Translate(0, -titleOffset, 0);
        gameObject.transform.Find("Title").Rotate(0, 180, 0);
        gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, GetLength() / 2.0f);
    }
    
    internal void SetLength(float length)
    {
        length = length * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

        Transform lineTransform = gameObject.transform.Find("AxisLine");

        if(lineTransform != null)
        {
            float newLocalScale = length / GetMeshLength();
            lineTransform.localScale = new Vector3(lineTransform.localScale.x,
                newLocalScale, lineTransform.localScale.z);
        }
    }

    private float GetMeshLength()
    {
        return meshLength;
    }

    private float GetLength()
    {
        Transform lineTransform = gameObject.transform.Find("AxisLine");
        Vector3 scale = lineTransform.localScale;
        return GetMeshLength() * Math.Max(scale.x, Math.Max(scale.y, scale.z));
    }

    internal void EnableAxisColorCoding(string channelType)
    {
        Transform lineTransform = gameObject.transform.Find("AxisLine");

        if (channelType == "x" || channelType == "width")
        {
            lineTransform.GetComponent<Renderer>().material.color = Color.red;
        } else if(channelType == "y" || channelType == "height")
        {
            lineTransform.GetComponent<Renderer>().material.color = Color.green;
        } else if(channelType == "z" || channelType == "depth")
        {
            lineTransform.GetComponent<Renderer>().material.color = Color.blue;
        }
        
    }

    public void ConstructTicks(JSONNode valuesSpecs, DxR.Scale scale)
    {
        Transform parent = gameObject.transform.Find("Ticks");
        GameObject tickPrefab = Resources.Load("Axis/Tick") as GameObject;
        if(tickPrefab == null)
        {
            throw new Exception("Cannot find tick prefab.");
        }

        for(int i = 0; i < valuesSpecs.Count; i++)
        {
            string domainValue = valuesSpecs[i].Value;

            float pos = float.Parse(scale.ApplyScale(domainValue)) * DxR.SceneObject.SIZE_UNIT_SCALE_FACTOR;

            AddTick(pos, tickPrefab, parent);
        }
    }

    private void AddTick(float pos, GameObject prefab, Transform parent)
    {
        GameObject instance = Instantiate(prefab, parent.position, parent.rotation, parent);
       instance.transform.Translate(0, pos - GetLength() / 2.0f, 0);
    }
}
