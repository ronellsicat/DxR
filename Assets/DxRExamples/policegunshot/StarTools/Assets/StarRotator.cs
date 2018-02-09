using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The CoronaRotator is a simple billboarding script- The corona effect is actually a thin disc, and should always be facing the main camera.
 */
[ExecuteInEditMode]
public class StarRotator : MonoBehaviour {
    public Camera starLockon;
    void Update() {
        if (starLockon == null) {
            transform.LookAt(Camera.main.transform);
        } else {
            transform.LookAt(starLockon.transform);
        }
    }
}