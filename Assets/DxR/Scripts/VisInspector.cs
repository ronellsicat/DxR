using UnityEngine;
using System.Collections;
using UnityEditor;

namespace DxR
{
    [CustomEditor(typeof(DxR.Vis))]
    public class VisInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DxR.Vis visObject = (DxR.Vis)target;
            if (GUILayout.Button("Update Vis"))
            {
                //visObject.BuildObject();
                Debug.Log("Update Vis");
            }
        }
    }
}