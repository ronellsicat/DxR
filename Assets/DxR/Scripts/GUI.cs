using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DxR
{
    public class GUI : MonoBehaviour
    {
        Vis targetVis = null;

        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetTargetVis(Vis vis)
        {
            targetVis = vis;

            // Update specs
        }

        public void UpdateVis()
        {
            Debug.Log("Update vis from GUI");
            targetVis.UpdateVisFromTextSpecs();
        }
    }

}
