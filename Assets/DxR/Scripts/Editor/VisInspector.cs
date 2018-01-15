using UnityEngine;
using System.Collections;
using UnityEditor;

namespace DxR
{
    [CustomEditor(typeof(Vis))]
    public class VisInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Vis visObject = (Vis)target;
            if (GUILayout.Button("Update Vis"))
            {
                visObject.UpdateVisSpecsFromTextSpecs();
                Debug.Log("Update Vis");
            }
        }
    }
}