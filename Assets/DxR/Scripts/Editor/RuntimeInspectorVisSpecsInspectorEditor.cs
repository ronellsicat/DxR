using UnityEngine;
using System.Collections;
using UnityEditor;

namespace DxR
{
    [CustomEditor(typeof(RuntimeInspectorVisSpecs))]
    public class RuntimeInspectorVisSpecsInspectorEditor : Editor
    {
        public SerializedProperty jsonSpecificationProperty;
        public SerializedProperty inputSpecificationProperty;
        RuntimeInspectorVisSpecs runtimeVisScript;

        private void OnEnable()
        {
            runtimeVisScript = (RuntimeInspectorVisSpecs)target;

            jsonSpecificationProperty = serializedObject.FindProperty("JSONSpecification");
            inputSpecificationProperty = serializedObject.FindProperty("InputSpecification");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Create top level label and update vis button
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Input Specification");
            if (Application.isPlaying && GUILayout.Button("Update Vis"))
            {
                runtimeVisScript.UpdateVis();
            }
            GUILayout.EndHorizontal();

            // Create field for JSON text file and clear button if file is entered
            GUILayout.BeginHorizontal();
            jsonSpecificationProperty.objectReferenceValue = (TextAsset)EditorGUILayout.ObjectField("", jsonSpecificationProperty.objectReferenceValue, typeof(TextAsset), true);
            if (jsonSpecificationProperty.objectReferenceValue != null && GUILayout.Button("Remove JSON File"))
            {
                // Transfer the text from the text file to the editor
                inputSpecificationProperty.stringValue = ((TextAsset)jsonSpecificationProperty.objectReferenceValue).text;
                jsonSpecificationProperty.objectReferenceValue = null;
            }
            GUILayout.EndHorizontal();

            // Create text area for the input specification. If a text file is provided, display its contents in greyed out form
            if (jsonSpecificationProperty.objectReferenceValue != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextArea(((TextAsset)jsonSpecificationProperty.objectReferenceValue).text, GUILayout.MinHeight(40), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                inputSpecificationProperty.stringValue = null;
                EditorGUI.EndDisabledGroup();
            }
            // If none is provided, show the in-editor text instead
            else
            {
                inputSpecificationProperty.stringValue = EditorGUILayout.TextArea(inputSpecificationProperty.stringValue, GUILayout.MinHeight(40), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            }

            // Create label for the internal specification
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Internal Specification");
            if (Application.isPlaying && runtimeVisScript.JSONSpecification == null && GUILayout.Button("Copy Internal Specification to Input"))
            {
                inputSpecificationProperty.stringValue = runtimeVisScript.InternalSpecification;
            }
            GUILayout.EndHorizontal();

            // Create text area for the internal specification
            EditorGUI.BeginDisabledGroup(true);
            if (runtimeVisScript.InternalSpecification != "")
            {
                EditorGUILayout.TextArea(runtimeVisScript.InternalSpecification, GUILayout.MinHeight(40), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            }
            else
            {
                EditorGUILayout.TextArea("The specification of this Vis that is currently being used internally will be shown here during runtime.", GUILayout.MinHeight(40), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}