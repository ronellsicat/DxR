using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleJSON;
using System.IO;
using Newtonsoft.Json;
using System;

namespace DxR
{
    /// <summary>
    /// This component can be attached to any GameObject (_parentObject) that already
    /// has a Vis component on it. This component acts as an alternative method to modify
    /// a Vis' JSON specification by doing so within the Unity Inspector at runtime,
    /// rather than from a .json asset file. All similar rules for DxR Vis specifications
    /// still apply.
    /// </summary>
    [RequireComponent(typeof(Vis))]
    public class RuntimeInspectorVisSpecs : MonoBehaviour
    {
        public TextAsset JSONSpecification;
        [TextArea(4, 20)]
        public string InputSpecification;
        [TextArea(4, 20)]
        public string InternalSpecification;

        private Vis parentVis;

        private void Start()
        {
            parentVis = GetComponent<Vis>();
            parentVis.VisUpdated.AddListener(UpdateInternalSpecification);
            UpdateInternalSpecification(parentVis, parentVis.GetVisSpecs());
        }

        public void UpdateVis()
        {
            if (EditorApplication.isPlaying)
            {
                if (parentVis == null)
                {
                    parentVis = GetComponent<Vis>();
                }

                if (parentVis != null)
                {
                    if (JSONSpecification != null)
                    {
                        parentVis.UpdateVisSpecsFromStringSpecs(JSONSpecification.text);
                    }
                    else
                    {
                        parentVis.UpdateVisSpecsFromStringSpecs(InputSpecification);
                    }
                }
            }
        }

        private void UpdateInternalSpecification(Vis vis, JSONNode visSpec)
        {
            using (var stringReader = new StringReader(visSpec.ToString()))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                InternalSpecification = stringWriter.ToString();
            }
        }
    }
}