using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DxR
{
    public class Portal : MonoBehaviour
    {
        public GameObject cameraTeleport;
        public GameObject cameraChildTeleport;
        public float teleportGazeTriggerTime;
        public float transitionSpeed;
        public GameObject targetGameObject;
        public float scaleTo = 1;
        public float speed = 0.1F;

        void Start()
        {
            cameraTeleport = GameObject.Find("MixedRealityCameraParent");
            cameraChildTeleport = GameObject.Find("MixedRealityCamera");
            if (targetGameObject == null)
                targetGameObject = gameObject;
        }

        void Update()
        {
            if (GazeManager.Instance.HitObject == this.gameObject && GazeManager.Instance.HitObject.tag == "Teleport")
            {
                transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            }
            if (GazeManager.Instance.HitObject == this.gameObject && GazeManager.Instance.HitObject.tag == "Teleport" && DxRCursor.mTimer >= teleportGazeTriggerTime)
            {
                //teleport(this.transform.position);
                translate(this.transform.position);
            }
        }


        public void teleport(Vector3 markPosition)
        {
            if(targetGameObject != null)
            {
                cameraTeleport.transform.position = markPosition - (CameraCache.Main.transform.position - cameraTeleport.transform.position);

                targetGameObject.transform.localScale = new Vector3(1, 1, 1) * scaleTo;
                cameraTeleport.transform.rotation = this.transform.rotation;

            }
        }

        public void translate(Vector3 markPosition)
        {
            if(targetGameObject != null)
            {
                float step = speed * Time.deltaTime;
                cameraTeleport.transform.position = Vector3.MoveTowards(cameraTeleport.transform.position, markPosition - (CameraCache.Main.transform.position - cameraTeleport.transform.position), step);
                targetGameObject.transform.localScale = Vector3.Lerp(targetGameObject.transform.localScale, new Vector3(1, 1, 1) * scaleTo, step);
                //Vector3 newDir = Vector3.RotateTowards(cameraTeleport.transform.forward, (this.transform.position - cameraTeleport.transform.position), step, 0.0F);
                //cameraTeleport.transform.rotation = Quaternion.LookRotation(newDir);
            }
        }
    }
}
