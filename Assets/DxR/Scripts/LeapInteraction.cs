using UnityEngine;
using System;

namespace Leap.Unity
{
    public class LeapInteraction : MonoBehaviour
    {

        private float stepSize = 0.01f;
        private KeyCode moveUpKey = KeyCode.UpArrow;
        private KeyCode moveDownKey = KeyCode.DownArrow;


        [SerializeField]
        private PinchDetector _pinchDetectorA;                      //Pinch Detector of Left hand
        public PinchDetector PinchDetectorA
        {
            get
            {
                return _pinchDetectorA;
            }
            set
            {
                _pinchDetectorA = value;
            }
        }

        [SerializeField]
        private PinchDetector _pinchDetectorB;                      //Pinch Detector of Right hand
        public PinchDetector PinchDetectorB
        {
            get
            {
                return _pinchDetectorB;
            }
            set
            {
                _pinchDetectorB = value;
            }
        }


        public bool enableLeapMotion = false;                           // Switch for enabling leap motion based interactions.


        private GameObject MainCameraObject = null;

        private GameObject[] DxRVisObject;

        private GameObject[] DxRViewObject = null;
        private GameObject[] DxRInteractionsObject = null;
        private GameObject[] DxRAnchorObject = null;

        private Vector3 previous_L_position;                        //Record previous frame's pinch position of left hand 
        private Vector3 previous_R_position;                        //Record previous frame's pinch position of right hand


        private Vector3[] VisSize;
        private Vector3[] Vis_center;

        private GameObject L_index_finger_end;
        private GameObject L_index_finger_c;

        private GameObject GazeObject;

        private int num_vis;


        private void Awake()
        {
            if (!enableLeapMotion)
            {
                enabled = false;
            }
            else
            {
                //Set the coordinate of Leapmotion to main camera. 
                MainCameraObject = GameObject.Find("MixedRealityCamera").gameObject;
                DxRVisObject = GameObject.FindGameObjectsWithTag("DxRVis");


                //The objects that will be controlled by Leapmotion
                DxRViewObject = GameObject.FindGameObjectsWithTag("DxRView");
                DxRInteractionsObject = GameObject.FindGameObjectsWithTag("DxRInteractions");
                DxRAnchorObject = GameObject.FindGameObjectsWithTag("DxRAnchor");
                GazeObject = GameObject.Find("DefaultCursor");

            }
        }


        void Start()
        {
            MainCameraObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.transform.parent = MainCameraObject.transform;

            num_vis = DxRVisObject.Length;

            VisSize = new Vector3[num_vis];
            Vis_center = new Vector3[num_vis];

            for (int i = 0; i < num_vis; i++)
            {
                VisSize[i] = DxRVisObject[i].GetComponent<DxR.Vis>().GetVisSize();
                Vis_center[i] = DxRViewObject[i].transform.position + VisSize[i] * DxR.Vis.SIZE_UNIT_SCALE_FACTOR / 2.0f;

            }

            HoloToolkit.Unity.CameraCache.Main.nearClipPlane = 0.01f;

        }

        void Update()
        {

            HoloToolkit.Unity.CameraCache.Main.nearClipPlane = 0.01f;


            if (Input.GetKeyDown(moveUpKey))
            {
                transform.Translate(Vector3.up * stepSize);
            }

            if (Input.GetKeyDown(moveDownKey))
            {
                transform.Translate(Vector3.down * stepSize);
            }



            //Interact with buttons and slider using left hand's index finger.
            L_index_finger_end = GameObject.Find("L_index_end");
            L_index_finger_c = GameObject.Find("L_index_c");
            if (L_index_finger_end != null && L_index_finger_c != null)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(
                        L_index_finger_end.transform.position,
                        L_index_finger_end.transform.position - L_index_finger_c.transform.position,
                        out hitInfo,
                        20.0f,
                        Physics.DefaultRaycastLayers))
                {
                    if (hitInfo.collider != null)
                    {

                    }
                }

            }

            //Reset previous position when pinch sitiuation is changed
            if (_pinchDetectorA != null && _pinchDetectorA.DidChangeFromLastFrame)
            {
                previous_L_position = _pinchDetectorA.Position;
            }

            if (_pinchDetectorB != null && _pinchDetectorB.DidChangeFromLastFrame)
            {
                previous_R_position = _pinchDetectorB.Position;
            }




            bool finish_control = false;

            //Select object based on real world coordinate
            //Two hand pinch control
            if (_pinchDetectorA != null && _pinchDetectorA.IsActive &&
                _pinchDetectorB != null && _pinchDetectorB.IsActive)
            {
                for (int i = 0; i < num_vis; i++)
                {
                    if ((DxRViewObject[i].transform.position - (_pinchDetectorA.Position + _pinchDetectorB.Position) / 2).magnitude
                        < (VisSize[i].CompMul(DxRViewObject[i].transform.localScale)).magnitude * DxR.Vis.SIZE_UNIT_SCALE_FACTOR / 2)
                    {
                        Two_hand_interaction(i);
                        finish_control = true;

                        previous_L_position = _pinchDetectorA.Position;
                        previous_R_position = _pinchDetectorB.Position;
                        break;
                    }
                }
            }


            //Left hand pinch control
            else if (_pinchDetectorA != null && _pinchDetectorA.IsActive)
            {
                for (int i = 0; i < num_vis; i++)
                {
                    if ((Vis_center[i] - _pinchDetectorA.Position).magnitude
                    < (VisSize[i].CompMul(DxRViewObject[i].transform.localScale)).magnitude * DxR.Vis.SIZE_UNIT_SCALE_FACTOR / 2)
                    {
                        Left_hand_interaction(i);
                        finish_control = true;
                        previous_L_position = _pinchDetectorA.Position;
                        break;
                    }
                }
                
            }
            //Right hand pinch control
            else if (_pinchDetectorB != null && _pinchDetectorB.IsActive)
            {
                for (int i = 0; i < num_vis; i++)
                {
                    if ((Vis_center[i] - _pinchDetectorB.Position).magnitude
                    < (VisSize[i].CompMul(DxRViewObject[i].transform.localScale)).magnitude * DxR.Vis.SIZE_UNIT_SCALE_FACTOR / 2)
                    {
                        Right_hand_interaction(i);
                        finish_control = true;
                        previous_R_position = _pinchDetectorB.Position;
                        break;
                    }
                }
                
            }


            if (!finish_control)
            {
                //Select object based on gaze
                //Two hand pinch control
                if (_pinchDetectorA != null && _pinchDetectorA.IsActive &&
                    _pinchDetectorB != null && _pinchDetectorB.IsActive)
                {
                    Interaction_based_on_raycating(0);
                }
                //Left hand pinch control
                else if (_pinchDetectorA != null && _pinchDetectorA.IsActive)
                {

                    Interaction_based_on_raycating(1);
                }
                //Right hand pinch control
                else if (_pinchDetectorB != null && _pinchDetectorB.IsActive)
                {
                    Interaction_based_on_raycating(2);
                }
            }
        }

        void Two_hand_interaction(int which_vis)
        {
            Vector3 rotation_axis = Vector3.Cross(_pinchDetectorA.Position - _pinchDetectorB.Position, _pinchDetectorB.Position - previous_L_position).normalized;
            float rotation_angle = ((_pinchDetectorA.Position - previous_L_position).magnitude + (_pinchDetectorB.Position - previous_R_position).magnitude) / 2 * 180;

            DxRViewObject[which_vis].transform.RotateAround(Vis_center[which_vis], rotation_axis, rotation_angle);

            Vector3 orientation_to_center = (Vis_center[which_vis] - DxRViewObject[which_vis].transform.position).CompDiv(DxRViewObject[which_vis].transform.localScale);

            DxRViewObject[which_vis].transform.localScale += Vector3.one * (Vector3.Distance(_pinchDetectorA.Position, _pinchDetectorB.Position) - Vector3.Distance(previous_L_position, previous_R_position));
            if (DxRViewObject[which_vis].transform.localScale.x < 0) DxRViewObject[which_vis].transform.localScale = Vector3.one * 0.01f;

            DxRViewObject[which_vis].transform.position = Vis_center[which_vis] - orientation_to_center.CompMul(DxRViewObject[which_vis].transform.localScale);
        }

        void Left_hand_interaction(int which_vis)
        {
            DxRViewObject[which_vis].transform.position += (_pinchDetectorA.Position - previous_L_position) * 2;
            DxRInteractionsObject[which_vis].transform.position += (_pinchDetectorA.Position - previous_L_position) * 2;
            DxRAnchorObject[which_vis].transform.position += (_pinchDetectorA.Position - previous_L_position) * 2;
            Vis_center[which_vis] += (_pinchDetectorA.Position - previous_L_position) * 2;
        }

        void Right_hand_interaction(int which_vis)
        {
            DxRViewObject[which_vis].transform.position += (_pinchDetectorB.Position - previous_R_position);
            DxRInteractionsObject[which_vis].transform.position += (_pinchDetectorB.Position - previous_R_position);
            DxRAnchorObject[which_vis].transform.position += (_pinchDetectorB.Position - previous_R_position);
            Vis_center[which_vis] += (_pinchDetectorB.Position - previous_R_position);

        }

        void Interaction_based_on_raycating(int interaction_type)
        {
            //Select object based on gaze
            //Find gazed object

            Vector3 Ray_moving = (GazeObject.transform.position - Camera.main.transform.position).normalized * 0.01f;
            Vector3 Current_ray_position = Camera.main.transform.position;
            Vector3 Start_position = Camera.main.transform.position;
            while (true)
            {
                if ((Current_ray_position - Start_position).magnitude > 20)
                {
                    return;
                }
                Current_ray_position += Ray_moving;
                for (int i = 0; i < num_vis; i++)
                {
                    if ((DxRViewObject[i].transform.position - Current_ray_position).magnitude
                        < (VisSize[i].CompMul(DxRViewObject[i].transform.localScale)).magnitude * DxR.Vis.SIZE_UNIT_SCALE_FACTOR / 2)
                    {

                        if (interaction_type == 0)
                        {
                            //Two hand pinch control
                            Two_hand_interaction(i);
                            previous_L_position = _pinchDetectorA.Position;
                            previous_R_position = _pinchDetectorB.Position;
                        }
                        else if (interaction_type == 1)
                        {
                            //Left hand pinch control
                            Left_hand_interaction(i);
                            previous_L_position = _pinchDetectorA.Position;
                        }
                        else if (interaction_type == 2)
                        {
                            //Right hand pinch control
                            Right_hand_interaction(i);
                            previous_R_position = _pinchDetectorB.Position;
                        }

                        return;
                    }
                }
            }
        }

    }
}
