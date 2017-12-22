using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeekRenderTexture : MonoBehaviour {

    public RenderTexture rt;
    public Camera renderCamera;
    public Shader shader;


    void Start()
    {
        rt = new RenderTexture(256, 256, 16);
        rt.Create();
        renderCamera = transform.parent.GetComponent<Camera>();
        renderCamera.targetTexture = rt;
        Renderer rend = GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Standard"));
        rend.material.mainTexture = rt;
    }

    void Update () {
		
	}
}
