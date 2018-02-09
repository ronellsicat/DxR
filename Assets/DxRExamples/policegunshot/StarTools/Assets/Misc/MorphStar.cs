using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This is a simple example of how to use the Star class to modify values in real time.
 */ 
[ExecuteInEditMode]
public class MorphStar : MonoBehaviour {

    public bool size = false;
    public bool color = false;
    public bool timescale = false;
    public bool resolution = false;
    public bool contrast = false;
    public bool rotation = false;

    private Star myStar;

    public void Update () {
        myStar = GetComponent<Star>();
        if(color)
        {
            UpdateColors();
        }
        if(size)
        {
            UpdateScale();
        }
        if(rotation)
        {
            UpdateRotationRate();
        }
        if(timescale)
        {
            UpdateTimeScale();
        }
        if(resolution)
        {
            UpdateResolution();
        }
        if(contrast)
        {
            UpdateContrast();
        }
	}

    public void UpdateContrast()
    {
        float scale = (1 + Mathf.Sin(Time.time / 3f)) * 2f + 0.1f;
        myStar.contrast = scale;
    }

    public void UpdateScale()
    {
        float scale = (1 + Mathf.Sin(Time.time / 3f)) * 60 + 30;
        myStar.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void UpdateRotationRate()
    {
        //TODO fix
        float rate_x = Mathf.Pow(Mathf.Sin(Time.time / 5f), 2) * 5;
        float rate_y = 1;
        float rate_z = 1;

        myStar.rotationRates = new Vector3(rate_x, rate_y, rate_z);
    }

    public void UpdateTimeScale()
    {
        float scale = Mathf.Pow(1 + Mathf.Sin(Time.time / 3), 4);
        myStar.timeScale = scale;
    }

    public void UpdateResolution()
    {
        float scale = Mathf.Pow(1 + Mathf.Sin(Time.time / 5), 3) * 10 + 0.1f;
        myStar.resolutionScale = scale;
    }


    public void UpdateColors()
    {
        myStar.baseStarColor = Color.HSVToRGB((Time.time / 10f) % 1, 0.5f + Mathf.PingPong(Time.time / 3, 0.5f), 1);
    }
}
