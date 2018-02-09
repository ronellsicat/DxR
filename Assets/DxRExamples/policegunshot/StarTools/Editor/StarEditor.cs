using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Star))]
public class StarEditor : Editor {

    public override void OnInspectorGUI()
    {
        Star targetStar = (Star)target;

        targetStar.isPaused = EditorGUILayout.Toggle("Pause (freeze rotation and texture)", targetStar.isPaused);

        //Color selections, temp, and b-v
        targetStar.manualColors = EditorGUILayout.Toggle("Enable manual color selection", targetStar.manualColors);

        if (targetStar.manualColors)
        {
            targetStar.SetColor(EditorGUILayout.ColorField("Star base color", targetStar.GetColor()));
        }
        else
        {
            float temp = EditorGUILayout.FloatField("Temperature (K)", targetStar.GetTemperature());
            if (targetStar.GetTemperature() != temp)
            {
                targetStar.SetTemperature(temp);
            }

            float bmv = EditorGUILayout.FloatField("Blue - Violet", targetStar.GetB_V());
            if (targetStar.GetB_V() != bmv)
            {
                targetStar.SetB_V(bmv);
            }

        }

        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        //Other properties
        targetStar.timeScale = EditorGUILayout.FloatField("Time scale", targetStar.timeScale);
        targetStar.resolutionScale = EditorGUILayout.FloatField("Resolution", targetStar.resolutionScale);
        targetStar.contrast = EditorGUILayout.FloatField("Contrast", targetStar.contrast);
        targetStar.rotationRates = EditorGUILayout.Vector3Field("Rotation rates", targetStar.rotationRates);

    }

}