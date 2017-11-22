// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// This example is built from HoloToolkit examples package.

using UnityEngine;

namespace DxR
{
    /// <summary>
    /// This class implements IFocusable to respond to gaze changes.
    /// It highlights the object being gazed at.
    /// </summary>
    public class GazeResponder : MonoBehaviour, HoloToolkit.Unity.InputModule.IFocusable
    {
        private Material[] defaultMaterials;

        private void Start()
        {
            defaultMaterials = GetComponent<Renderer>().materials;
        }

        public void OnFocusEnter()
        {
            Mark mark = gameObject.GetComponent<Mark>();
            if(mark != null)
            {
                mark.OnFocusEnter();
            } 
        }

        public void OnFocusExit()
        {
            Mark mark = gameObject.GetComponent<Mark>();
            if (mark != null)
            {
                mark.OnFocusExit();
            }
        }

        private void OnDestroy()
        {
            
        }
    }
}