using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace DxR
{
    public class Anchor : MonoBehaviour
    {
        bool isDistanceToggleEnabled = false;
        float cameraToAnchorDistance = 0.0f;
        float distanceToggleMinInMeters = 0.0f;
        float distanceToggleMaxInMeters = 0.0f;

        Vis visObject = null;

        // Use this for initialization
        void Start()
        {
            visObject = gameObject.transform.parent.GetComponent<Vis>();
            if(visObject == null)
            {
                throw new System.Exception("Cannot load Vis instance of anchor.");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(isDistanceToggleEnabled)
            {
                cameraToAnchorDistance = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);

                Debug.Log("Camera to anchor distance is " + cameraToAnchorDistance.ToString());
                Debug.Log("Camera to anchor min distance is " + distanceToggleMinInMeters.ToString());
                Debug.Log("Camera to anchor max distance is " + distanceToggleMaxInMeters.ToString());

                if (cameraToAnchorDistance >= distanceToggleMinInMeters && 
                    cameraToAnchorDistance <= distanceToggleMaxInMeters)
                {
                    SetSceneObjectVisibility(true);
                } else
                {
                    SetSceneObjectVisibility(false);
                }
            }
        }

        private void SetSceneObjectVisibility(bool val)
        {
            if(visObject)
            {
                //TODO:
                //visObject.SetDistanceVisibility(val);
            }
        }

        public void UpdateSpecs(JSONNode anchorSpecs)
        {
            if((anchorSpecs == null) || (anchorSpecs.Value.ToString() == "none"))
            {
                gameObject.SetActive(false);
                return;
            }

            Debug.Log("Updating anchor specs." + anchorSpecs.ToString());

            if(anchorSpecs["placement"] != null)
            {
                if(anchorSpecs["placement"].Value.ToString() == "tapToPlace")
                {
                    EnableTapToPlace();
                }
            }

            if(anchorSpecs["visibility"] != null)
            {
                JSONNode visSpecs = anchorSpecs["visibility"];

                if(visSpecs.Value.ToString() != "always")
                {
                    if (visSpecs["gazeToggle"] != null)
                    {
                        if (visSpecs["gazeToggle"].AsBool)
                        {
                            bool initVisibility = true;
                            if (visSpecs["gazeToggleInit"] != null)
                            {
                                initVisibility = visSpecs["gazeToggleInit"].AsBool;
                            }
                            EnableGazeToggle(initVisibility);
                        }
                    }

                    bool distToggleMinProvided = false;
                    if(visSpecs["distanceToggleMin"] != null)
                    {
                        distToggleMinProvided = true;
                        distanceToggleMinInMeters = visSpecs["distanceToggleMin"].AsFloat * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
                        isDistanceToggleEnabled = true;
                    }

                    bool distToggleMaxProvided = false;
                    if (visSpecs["distanceToggleMax"] != null)
                    {
                        distToggleMaxProvided = true;
                        distanceToggleMaxInMeters = visSpecs["distanceToggleMax"].AsFloat * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
                        isDistanceToggleEnabled = true;
                    }

                    if(isDistanceToggleEnabled && !distToggleMinProvided)
                    {
                        distanceToggleMinInMeters = 0.0f;
                    }

                    if (isDistanceToggleEnabled && !distToggleMaxProvided)
                    {
                        distanceToggleMaxInMeters = 100.0f;
                    }
                }
            }
        }

        private void EnableTapToPlace()
        {
            HoloToolkit.Unity.InputModule.TapToPlace tapToPlaceComponent = 
                gameObject.AddComponent(typeof(HoloToolkit.Unity.InputModule.TapToPlace)) as HoloToolkit.Unity.InputModule.TapToPlace;
            if (tapToPlaceComponent == null)
            {
                throw new System.Exception("Cannot load TapToPlace component for anchor.");
            } else
            {
                tapToPlaceComponent.PlaceParentOnTap = true;
            }
        }

        private void EnableGazeToggle(bool initVisibility)
        {
            DxR.GazeResponderAnchor gazeResp =
                gameObject.AddComponent(typeof(DxR.GazeResponderAnchor)) as DxR.GazeResponderAnchor;
            if (gazeResp == null)
            {
                throw new System.Exception("Cannot load GazeResponderAnchor component for anchor.");
            } else
            {
                gazeResp.SetGazeToggle(true);
                gazeResp.SetInitVisibility(initVisibility);
            }
        }
    }
}
