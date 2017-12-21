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
    public class GazeResponderAnchor : MonoBehaviour, HoloToolkit.Unity.InputModule.IFocusable
    {
        bool isGazeToggleEnabled = false;
        bool visibility = true;

        SceneObject sceneObject = null;

        private void Start()
        {
            sceneObject = gameObject.transform.parent.GetComponent<SceneObject>();
            if (sceneObject == null)
            {
                throw new System.Exception("Cannot load SceneObject instance of GazeResponderAnchor.");
            }
        }

        private void Update()
        {
            
        }

        public void SetGazeToggle(bool set)
        {
            isGazeToggleEnabled = set;
        }

        public void SetInitVisibility(bool initVis)
        {
            visibility = initVis;
            if(!initVis)
            {
                SetSceneObjectVisibility(initVis);
            }
        }

        public void OnFocusEnter()
        {
            visibility = !visibility;
            SetSceneObjectVisibility(visibility);
        }

        private void SetSceneObjectVisibility(bool visib)
        {
             if (sceneObject != null)
            {
                sceneObject.SetGazeVisibility(visib);
            }
        }

        public void OnFocusExit()
        {

        }

        private void OnDestroy()
        {
            
        }
    }
}