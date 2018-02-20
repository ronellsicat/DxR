using System;
using System.Collections;
using System.Collections.Generic;
using DxR;
using SimpleJSON;
using UnityEngine;

public class LegendValue : MonoBehaviour {

    public string dataFieldName = "";
    public string categoryName = "";

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetTitle(string title)
    {
        gameObject.GetComponentInChildren<TextMesh>().text = title;
        categoryName = title;
    } 

    public void SetMark(GameObject mark)
    {
        mark.name = "Mark";
        mark.transform.parent = gameObject.transform;
    }

    public void SetDataFieldName(string fieldName)
    {
        dataFieldName = fieldName;
    }
}
