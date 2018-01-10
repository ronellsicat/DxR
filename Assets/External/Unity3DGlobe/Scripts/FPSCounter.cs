using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class FPSCounter : MonoBehaviour {
    public Text text;
	// Use this for initialization
	void Start () {
        if (!text)
            text = GetComponent<Text>();
	}
    int framesCount = 0;
    float elapsedTime = 0;
	// Update is called once per frame
	void Update () {
        elapsedTime += Time.deltaTime;
        framesCount++;
        if(elapsedTime>1)
        {
            elapsedTime -= 1;
            text.text = "FPS:"+framesCount;
            framesCount = 0;
        }
	}
}
