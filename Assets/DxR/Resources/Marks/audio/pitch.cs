using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitch : MonoBehaviour
{

    public float minY;
    public float maxY;

    void Start()
    {
        gameObject.GetComponent<AudioSource>().pitch = (transform.position.y - minY) / (-3 - minY) * (3 - maxY) + maxY;
    }
}
