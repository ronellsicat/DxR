using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace DxR
{
    public class Axis : MonoBehaviour
    {

        private float meshLength = 2.0f;    // This is the initial length of the cylinder used for the axis.
        private float titleOffset = 0.075f;
        private float tickLabelOffset = 0.03f;
        private bool enableFilter = false;
        private Interactions interactionsObject = null;
        private string dataField = "";

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        internal void Init(Interactions interactions, string field)
        {
            interactionsObject = interactions;
            dataField = field;
        }

        public void UpdateSpecs(JSONNode axisSpecs, DxR.Scale scale)
        {
            if (axisSpecs["title"] != null)
            {
                SetTitle(axisSpecs["title"].Value);
            }

            if (axisSpecs["titlePadding"] != null)
            {
                SetTitlePadding(axisSpecs["titlePadding"].Value);
            }

            float axisLength = 0.0f;
            if (axisSpecs["length"] != null)
            {
                axisLength = axisSpecs["length"].AsFloat;
                SetLength(axisLength);
            }

            if (axisSpecs["orient"] != null && axisSpecs["face"] != null)
            {
                SetOrientation(axisSpecs["orient"].Value, axisSpecs["face"].Value);
            }

            if (axisSpecs["ticks"].AsBool && axisSpecs["values"] != null)
            {
                ConstructTicks(axisSpecs, scale);
            }

            if (axisSpecs["color"] != null)
            {
                SetColor(axisSpecs["color"].Value);
            }

            if (axisSpecs["filter"] != null)
            {
                if (axisSpecs["filter"].AsBool)
                {
                    EnableThresholdFilter(axisSpecs, scale);
                }
            }
        }

        private void EnableThresholdFilter(JSONNode axisSpecs, DxR.Scale scale)
        {
            Transform slider = gameObject.transform.Find("AxisLine/Slider");
            slider.gameObject.SetActive(true);

            SetFilterLength(axisSpecs["length"].AsFloat);

            DxR.SliderGestureControlBothSide sliderControl =
                    slider.GetComponent<DxR.SliderGestureControlBothSide>();
            if (sliderControl == null) return;

            float domainMin = float.Parse(scale.domain[0]);
            float domainMax = float.Parse(scale.domain[1]);

            // TODO: Check validity of specs.

            sliderControl.SetSpan(domainMin, domainMax);
            sliderControl.SetSliderValue1(domainMin);
            sliderControl.SetSliderValue2(domainMax);

            slider.gameObject.name = dataField;

            interactionsObject.EnableAxisThresholdFilter(dataField);

            if (interactionsObject != null)
            {
                sliderControl.OnUpdateEvent.AddListener(interactionsObject.ThresholdFilterUpdated);
            }
        }

        public void SetTitle(string title)
        {
            gameObject.GetComponentInChildren<TextMesh>().text = title;
        }

        public void SetTitlePadding(string titlePadding)
        {
            titleOffset = titleOffset + (float.Parse(titlePadding) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR);
        }

        // TODO: Create ticks marks and tick labels using mark and channel metaphor, 
        // i.e., create them using the tick values as data and set orientation channels
        // according to orient and face params.
        internal void SetOrientation(string orient, string face)
        {
            if (orient == "bottom" && face == "front")
            {
                OrientAlongPositiveX();
            }
            else if (orient == "left" && face == "front")
            {
                OrientAlongPositiveY();
            }
            else if (orient == "bottom" && face == "left")
            {
                OrientAlongPositiveZ();
            }
        }

        private void OrientAlongPositiveX()
        {
            gameObject.transform.localPosition = new Vector3(GetLength() / 2.0f, 0.0f, 0.0f);
            gameObject.transform.Find("Title").Translate(0, -titleOffset, 0);
        }

        private void OrientAlongPositiveY()
        {
            gameObject.transform.Rotate(0, 0, 90.0f);
            gameObject.transform.Find("Title").Translate(0, titleOffset, 0);
            gameObject.transform.localPosition = new Vector3(0.0f, GetLength() / 2.0f, 0.0f);
        }

        private void OrientAlongPositiveZ()
        {
            gameObject.transform.Rotate(0, -90.0f, 0);
            gameObject.transform.Find("Title").Translate(0, -titleOffset, 0);
            gameObject.transform.Find("Title").Rotate(0, 180, 0);
            gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, GetLength() / 2.0f);
        }

        internal void SetLength(float length)
        {
            length = length * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

            Transform lineTransform = gameObject.transform.Find("AxisLine");

            if (lineTransform != null)
            {
                float newLocalScale = length / GetMeshLength();
                lineTransform.localScale = new Vector3(lineTransform.localScale.x,
                    newLocalScale, lineTransform.localScale.z);
            }
        }

        private void SetFilterLength(float length)
        {
            length = length * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

            Debug.Log("Setting filter length");
            Transform sliderBar = gameObject.transform.Find("AxisLine/Slider/SliderBar");
            if (sliderBar != null)
            {
                Transform knob1 = sliderBar.Find("SliderKnob1");
                Transform knob2 = sliderBar.Find("SliderKnob2");
                Vector3 knobOrigScale1 = knob1.localScale;
                Vector3 knobOrigScale2 = knob2.localScale;

                float newLocalScale = length / 0.2127f; // sliderBar.GetComponent<MeshFilter>().mesh.bounds.size.x;
                sliderBar.transform.localScale = new Vector3(newLocalScale, sliderBar.transform.localScale.y,
                    sliderBar.transform.localScale.z);

                if (knob1 != null)
                {
                    knob1.transform.localScale = new Vector3(0.4f, 2.0f, 1.5f);
                }
                if (knob2 != null)
                {
                    knob2.transform.localScale = new Vector3(0.4f, 2.0f, 1.5f);
                }
            }
        }

        private float GetMeshLength()
        {
            return meshLength;
        }

        private float GetLength()
        {
            Transform lineTransform = gameObject.transform.Find("AxisLine");
            Vector3 scale = lineTransform.localScale;
            return GetMeshLength() * Math.Max(scale.x, Math.Max(scale.y, scale.z));
        }

        internal void SetColor(string colorString)
        {
            Transform lineTransform = gameObject.transform.Find("AxisLine");
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(colorString, out color);
            if (colorParsed)
            {
                lineTransform.GetComponent<Renderer>().material.color = color;
            }
        }

        public void ConstructTicks(JSONNode axisSpecs, DxR.Scale scale)
        {
            bool showTickLabels = false;

            if (axisSpecs["labels"] != null)
            {
                showTickLabels = axisSpecs["labels"].AsBool;
            }

            Transform parent = gameObject.transform.Find("Ticks");
            GameObject tickPrefab = Resources.Load("Axis/Tick") as GameObject;

            if (tickPrefab == null)
            {
                throw new Exception("Cannot find tick prefab.");
            }

            for (int i = 0; i < axisSpecs["values"].Count; i++)
            {
                string domainValue = axisSpecs["values"][i].Value;

                float pos = float.Parse(scale.ApplyScale(domainValue)) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

                string label = showTickLabels ? domainValue : "";
                AddTick(axisSpecs["face"], axisSpecs["orient"], pos, label, tickPrefab, parent);
            }
        }

        private void AddTickLabel(float pos, string label, GameObject prefab, Transform parent)
        {
            GameObject instance = Instantiate(prefab, parent.position, Quaternion.identity, parent);
            instance.transform.Translate(0, pos - GetLength() / 2.0f, 0);
            instance.GetComponent<TextMesh>().text = label;
        }

        private void AddTick(string face, string orient, float pos, string label, GameObject prefab, Transform parent)
        {
            GameObject instance = Instantiate(prefab, parent.position, parent.rotation, parent);

            //instance.transform.Translate(pos - GetLength() / 2.0f, 0, 0);
            instance.transform.localPosition = new Vector3(pos - (GetLength() / 2.0f), 0, 0);

            // Adjust label
            // TODO: Adjust label angle.
            Transform tickLabelTransform = instance.transform.Find("TickLabel");
            float yoffset = 0.0f;
            float xoffset = 0.0f;
            float zrot = tickLabelTransform.localEulerAngles.z;
            float yrot = tickLabelTransform.localEulerAngles.y;
            float xrot = tickLabelTransform.localEulerAngles.x;
            if (face == "front" && orient == "bottom")
            {
                float labelAngle = 0.0f;
                zrot = zrot + labelAngle + 90;
                yoffset = -tickLabelOffset;
            }
            else if (face == "front" && orient == "left")
            {
                instance.transform.localRotation = Quaternion.Euler(0, 0, 180.0f);
                float labelAngle = 0.0f;
                yoffset = -tickLabelOffset;
                zrot = zrot + labelAngle + 90.0f;
            }
            else if (face == "left" && orient == "bottom")
            {
                float labelAngle = 0.0f;
                yoffset = -tickLabelOffset;
                zrot = zrot + labelAngle - 90.0f;
                xrot = xrot + 180.0f;
            }

            tickLabelTransform.localPosition = new Vector3(xoffset, yoffset, 0);
            tickLabelTransform.localEulerAngles = new Vector3(xrot, yrot, zrot);
            tickLabelTransform.GetComponent<TextMesh>().text = label;
        }
    }
}