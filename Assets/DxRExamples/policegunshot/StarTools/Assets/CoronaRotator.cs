using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The CoronaRotator is a simple billboarding script- The corona effect is actually a thin disc, and should always be facing the main camera.
 */
[ExecuteInEditMode]
public class CoronaRotator : MonoBehaviour {
    public Camera coronaLockon;
    void Update() {
        if (coronaLockon == null) {
            transform.parent.LookAt(Camera.main.transform);
        } else {
            transform.parent.LookAt(coronaLockon.transform);
        }
    }
}