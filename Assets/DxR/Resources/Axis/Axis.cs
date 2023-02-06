using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace DxR
{
    public class Axis : MonoBehaviour
    {
        private readonly float meshLength = 2.0f;    // This is the initial length of the cylinder used for the axis.
        private float titleOffset = 0.075f;
        private float tickLabelOffset = 0.03f;

        private Interactions interactionsObject = null;
        private string dataField = "";
        private char facingDirection;

        private GameObject title;
        private GameObject axisLine;
        private GameObject sliderBar;
        private GameObject ticksHolder;
        private GameObject tickPrefab;
        private TextMesh titleTextMesh;
        private List<GameObject> ticks = new List<GameObject>();

        public void Init(Interactions interactions, string field)
        {
            interactionsObject = interactions;
            dataField = field;

            title = gameObject.transform.Find("Title").gameObject;
            axisLine = gameObject.transform.Find("AxisLine").gameObject;
            sliderBar = gameObject.transform.Find("AxisLine/Slider/SliderBar").gameObject;
            ticksHolder = gameObject.transform.Find("Ticks").gameObject;
            tickPrefab = Resources.Load("Axis/Tick") as GameObject;
            titleTextMesh = gameObject.transform.Find("Title/Text").GetComponent<TextMesh>();
        }

        public void UpdateSpecs(JSONNode axisSpecs, DxR.Scale scale)
        {
            if (axisSpecs["title"] != null)
            {
                SetTitle(axisSpecs["title"].Value);
            }

            if (axisSpecs["titlePadding"] != null)
            {
                SetTitlePadding(axisSpecs["titlePadding"].AsFloat);
            }
            else
            {
                titleOffset = 0.075f;
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
                ConstructOrUpdateTicks(axisSpecs, scale);
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

        private void SetTitle(string title)
        {
            titleTextMesh.text = title;
        }

        private void SetTitlePadding(float titlePadding)
        {
            titleOffset = 0.075f + titlePadding * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
        }

        // TODO: Create ticks marks and tick labels using mark and channel metaphor,
        // i.e., create them using the tick values as data and set orientation channels
        // according to orient and face params.
        private void SetOrientation(string orient, string face)
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
            facingDirection = 'x';
            gameObject.transform.localPosition = GetAxisPosition(facingDirection, GetLength());
            title.transform.localPosition = new Vector3(0, -titleOffset, 0);
        }

        private void OrientAlongPositiveY()
        {
            facingDirection = 'y';
            gameObject.transform.localPosition = GetAxisPosition(facingDirection, GetLength());
            gameObject.transform.localRotation = GetAxisRotation(facingDirection);
            title.transform.localPosition = new Vector3(0, titleOffset, 0);
        }

        private void OrientAlongPositiveZ()
        {
            facingDirection = 'z';
            gameObject.transform.localPosition = GetAxisPosition(facingDirection, GetLength());
            gameObject.transform.localRotation = GetAxisRotation(facingDirection);
            title.transform.localPosition = new Vector3(0, -titleOffset, 0);
            title.transform.localEulerAngles = new Vector3(0, 180, 0);
        }

        private Vector3 GetAxisPosition(char dim, float length)
        {
            switch (dim)
            {
                default:
                case 'x':
                    return new Vector3(length / 2.0f, 0.0f, 0.0f);
                case 'y':
                    return new Vector3(0.0f, length / 2.0f, 0.0f);
                case 'z':
                    return new Vector3(0.0f, 0.0f, length / 2.0f);
            }
        }

        private Quaternion GetAxisRotation(char dim)
        {
            switch (dim)
            {
                default:
                case 'x':
                    return Quaternion.identity;
                case 'y':
                    return Quaternion.Euler(0, 0, 90);
                case 'z':
                    return Quaternion.Euler(0, -90, 0);
            }
        }

        private void CentreTitle()
        {
            switch (facingDirection)
            {
                case 'x':
                    title.transform.localPosition = new Vector3(0, -titleOffset, 0);
                    return;

                case 'y':
                    title.transform.localPosition = new Vector3(0, titleOffset, 0);
                    return;

                case 'z':
                    title.transform.localPosition = new Vector3(0, -titleOffset, 0);
                    title.transform.localEulerAngles = new Vector3(0, 180, 0);
                    return;
            }
        }

        /// <summary>
        /// Translates the axes along a spatial direction. To be used AFTER the orient functions
        /// </summary>
        public void SetTranslation(float value, int dim)
        {
            float offset = value * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
            Vector3 translateBy = transform.localPosition;
            translateBy[dim] = offset + translateBy[dim];
            transform.localPosition = translateBy;
        }

        public void SetTranslation(Vector3 translation)
        {
            translation *= DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
            Vector3 translateBy = transform.localPosition;
            translateBy += translation;
            transform.localPosition = translateBy;
        }

        public void SetRotate(Quaternion rotate)
        {
            Vector3 targetPosition = rotate * GetAxisPosition(facingDirection, GetLength());
            Quaternion targetRotation = rotate * GetAxisRotation(facingDirection);
            transform.localPosition = targetPosition;
            transform.localRotation = targetRotation;
        }

        private void SetLength(float length)
        {
            length = length * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
            float newLocalScale = length / meshLength;
            axisLine.transform.localScale = new Vector3(axisLine.transform.localScale.x, newLocalScale, axisLine.transform.localScale.z);
        }

        private float GetLength()
        {
            Vector3 scale = axisLine.transform.localScale;

            // If any of the dimensions in the scale Vector3 are negative, we assume that the size of the View is negative as well,
            // meaning this axis should also move towards the negative direction
            if (scale.x < 0 || scale.y < 0 || scale.z < 0)
            {
                return meshLength * Mathf.Min(scale.x, scale.y, scale.z);
            }
            else
            {
                return meshLength * Mathf.Max(scale.x, scale.y, scale.z);
            }
        }

        private void SetColor(string colorString)
        {
            if (ColorUtility.TryParseHtmlString(colorString, out Color color))
            {
                axisLine.GetComponent<Renderer>().material.color = color;
            }
        }

        private void SetColor(Color color)
        {
            axisLine.GetComponent<Renderer>().material.color = color;
        }

        /// <summary>
        /// Updates the ticks along these axes with new values, creating new ticks if necessary. Will automatically hide unneeded ticks
        /// </summary>
        private void ConstructOrUpdateTicks(JSONNode axisSpecs, DxR.Scale scale)
        {
            bool showTickLabels = axisSpecs["labels"] != null ? showTickLabels = axisSpecs["labels"].AsBool : false;
            int tickCount = axisSpecs["values"].Count;

            for (int i = 0; i < tickCount; i++)
            {
                string domainValue = axisSpecs["values"][i].Value;
                float position = float.Parse(scale.ApplyScale(domainValue)) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
                string label = showTickLabels ? domainValue : "";
                string face = axisSpecs["face"];
                string orient = axisSpecs["orient"];

                // If there is a tick already for us to use, take it, otherwise instantiate a new one
                GameObject tick;
                if (i < ticks.Count)
                {
                    tick = ticks[i];
                    if (!tick.activeSelf)
                        tick.SetActive(true);
                }
                else
                {
                    tick = Instantiate(tickPrefab, ticksHolder.transform.position, ticksHolder.transform.rotation, ticksHolder.transform);
                    ticks.Add(tick);
                }

                UpdateTick(tick, position, label, face, orient, GetLength());
            }

            // Hide all leftover ticks
            for (int i = tickCount; i < ticks.Count; i++)
            {
                GameObject tick = ticks[i];
                if (tick.activeSelf)
                {
                    tick.SetActive(false);
                }
            }
        }

        private void SetTickVisibility(bool visible)
        {
            foreach (GameObject tick in ticks)
            {
                tick.SetActive(visible);
            }
        }

        private void UpdateTick(GameObject tick, float position, string label, string face, string orient, float axisLength)
        {
            tick.transform.localPosition = new Vector3(position - (axisLength / 2f), 0, 0);

            // Adjust label
            Transform tickLabelTransform = tick.transform.Find("TickLabel");

            float yoffset = 0.0f;
            float xoffset = 0.0f;
            float zrot = 0;
            float yrot = 0;
            float xrot = 0;

            // Adjust label
            // TODO: Adjust label angle.
            if (face == "front" && orient == "bottom")
            {
                float labelAngle = 0.0f;
                zrot = zrot + labelAngle + 90;
                yoffset = -tickLabelOffset;
            }
            else if (face == "front" && orient == "left")
            {
                tick.transform.localRotation = Quaternion.Euler(0, 0, 180.0f);
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

        private void EnableThresholdFilter(JSONNode axisSpecs, DxR.Scale scale)
        {
            Transform slider = gameObject.transform.Find("AxisLine/Slider");
            slider.gameObject.SetActive(true);

            SetFilterLength(axisSpecs["length"].AsFloat);

            // DxR.SliderGestureControlBothSide sliderControl =
            //         slider.GetComponent<DxR.SliderGestureControlBothSide>();
            // if (sliderControl == null) return;

            float domainMin = float.Parse(scale.domain[0]);
            float domainMax = float.Parse(scale.domain[1]);

            // // TODO: Check validity of specs.
            // sliderControl.SetSpan(domainMin, domainMax);
            // sliderControl.SetSliderValue1(domainMin);
            // sliderControl.SetSliderValue2(domainMax);

            // slider.gameObject.name = dataField;

            // interactionsObject.EnableAxisThresholdFilter(dataField);

            // if (interactionsObject != null)
            // {
            //     sliderControl.OnUpdateEvent.AddListener(interactionsObject.ThresholdFilterUpdated);
            // }
        }

        private void SetFilterLength(float length)
        {
            length = length * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

            Debug.Log("Setting filter length");

            Transform knob1 = sliderBar.transform.Find("SliderKnob1");
            Transform knob2 = sliderBar.transform.Find("SliderKnob2");
            // Vector3 knobOrigScale1 = knob1.localScale;
            // Vector3 knobOrigScale2 = knob2.localScale;

            float newLocalScale = 0.5f / 0.2127f;
            // float newLocalScale = length / 0.2127f; // sliderBar.GetComponent<MeshFilter>().mesh.bounds.size.x;
            sliderBar.transform.localScale = new Vector3(newLocalScale, sliderBar.transform.localScale.y, sliderBar.transform.localScale.z);

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
}