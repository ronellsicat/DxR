// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace DxR
{
    /// <summary>
    /// updates slider UI based on gesture input
    /// </summary>
    public class SliderGestureControlBothSide : HoloToolkit.Examples.InteractiveElements.GestureInteractiveControl
    {
        [Tooltip("The main bar of the slider, used to get the actual width of the slider")]
        public GameObject SliderBar;
        [Tooltip("The visual marker of the slider first value")]
        public GameObject first_Knob;


        [Tooltip("The visual marker of the slider second value")]
        public GameObject second_Knob;


        [Tooltip("The fill that represents the volume of the shader value")]
        public GameObject SliderFill;
        [Tooltip("The text representation of the slider value")]
        public TextMesh Label1;
        public TextMesh Label2;

        [Tooltip("Used for centered format only, will be turned off if LeftJustified")]
        public GameObject CenteredDot;

        [Tooltip("Sends slider event information on Update")]
        public UnityEvent OnUpdateEvent;

        /// <summary>
        /// The value of the slider
        /// </summary>
        public float SliderValue1
        {
            private set
            {
                if (mSliderValue1 != value)
                {
                    mSliderValue1 = value;
                    OnUpdateEvent.Invoke();
                }
            }
            get
            {
                return mSliderValue1;
            }
        }

        public float SliderValue2
        {
            private set
            {
                if (mSliderValue2 != value)
                {
                    mSliderValue2 = value;
                    OnUpdateEvent.Invoke();
                }
            }
            get
            {
                return mSliderValue2;
            }
        }


        [Tooltip("Min numeric value to display in the slider label")]
        public float MinSliderValue = 0;

        [Tooltip("Max numeric value to display in the slider label")]
        public float MaxSliderValue = 1;

        [Tooltip("Switches between a left justified or centered slider")]
        public bool Centered = false;

        [Tooltip("Format the slider value and control decimal places if needed")]
        public string LabelFormat = "#.##";

        private float mSliderValue1;
        private float mSliderValue2;

        // calculation variables
        private float mValueSpan;
        private float mCachedValue1;
        private float mCachedValue2;
        private float mDeltaValue1;
        private float mDeltaValue2;
        private Vector3 mStartCenter1 = new Vector3();
        private Vector3 mStartCenter2 = new Vector3();
        private float mSliderMagnitude;
        private Vector3 mStartSliderPosition1;
        private Vector3 mStartSliderPosition2;

        // cached UI values
        private Vector3 mKnobVector1;
        private Vector3 mKnobVector2;
        private Vector3 mSliderFillScale;
        private float mSliderWidth;

        private float AutoSliderTime = 0.25f;
        private float AutoSliderTimerCounter = 0.5f;
        private float AutoSliderValue = 0;

        private Vector3 mSliderVector;
        private Quaternion mCachedRotation;

        GameObject GazePoint;
        int current_selection=1;

        protected override void Awake()
        {
            base.Awake();

            GazePoint = GameObject.Find("DefaultCursor");

            if (first_Knob != null)
            {
                mStartCenter1.z = first_Knob.transform.localPosition.z;
            }
            if (second_Knob != null)
            {
                mStartCenter2.z = second_Knob.transform.localPosition.z;
            }

            mCachedRotation = SliderBar.transform.rotation;

            // with some better math below, I may be able to avoid rotating to get the proper size of the component

            SliderBar.transform.rotation = Quaternion.identity;

            // set the width of the slider 
            mSliderMagnitude = SliderBar.transform.InverseTransformVector(SliderBar.GetComponent<Renderer>().bounds.size).x;

            // set the center position
            mStartSliderPosition1 = mStartCenter1 + Vector3.left * mSliderMagnitude / 2;
            mStartSliderPosition2 = mStartCenter2 + Vector3.left * mSliderMagnitude / 2;

            mValueSpan = MaxSliderValue - MinSliderValue;

            mSliderValue1 = Mathf.Clamp(SliderValue1, MinSliderValue, MaxSliderValue);
            mSliderValue2 = Mathf.Clamp(SliderValue2, MinSliderValue, MaxSliderValue);

            if (!Centered)
            {
                mDeltaValue1 = SliderValue1 / mValueSpan;
                mDeltaValue2 = SliderValue2 / mValueSpan;
            }
            else
            {
                mValueSpan = (MaxSliderValue - MinSliderValue) / 2;
                mDeltaValue1 = (SliderValue1 + mValueSpan) / 2 / mValueSpan;
                mDeltaValue2 = (SliderValue2 + mValueSpan) / 2 / mValueSpan;
            }

            mSliderFillScale = new Vector3(1, 1, 1);
            mSliderWidth = mSliderMagnitude;
            if (SliderFill != null)
            {
                mSliderFillScale = SliderFill.transform.localScale;
                mSliderWidth = SliderFill.transform.InverseTransformVector(SliderFill.GetComponent<Renderer>().bounds.size).x;
            }

            if (CenteredDot != null && !Centered)
            {
                CenteredDot.SetActive(false);
            }

            SliderBar.transform.rotation = mCachedRotation;

            UpdateVisuals();
            mCachedValue1 = mDeltaValue1;
            mCachedValue2 = mDeltaValue2;

            mSliderVector = SliderBar.transform.InverseTransformPoint(mStartCenter2 + SliderBar.transform.right * mSliderMagnitude / 2) - SliderBar.transform.InverseTransformPoint(mStartCenter2 - SliderBar.transform.right * mSliderMagnitude / 2);
            AlignmentVector = SliderBar.transform.right;
            AlignmentVector = mSliderVector;
        }

        public override void ManipulationUpdate(Vector3 startGesturePosition, Vector3 currentGesturePosition, Vector3 startHeadOrigin, Vector3 startHeadRay, HoloToolkit.Examples.InteractiveElements.GestureInteractive.GestureManipulationState gestureState)
        {
            if (AlignmentVector != SliderBar.transform.right)
            {
                if (current_selection == 1)
                {
                    mSliderVector = SliderBar.transform.InverseTransformPoint(mStartCenter1 + SliderBar.transform.right * mSliderMagnitude / 2) - SliderBar.transform.InverseTransformPoint(mStartCenter1 - SliderBar.transform.right * mSliderMagnitude / 2);
                }
                else
                {
                    mSliderVector = SliderBar.transform.InverseTransformPoint(mStartCenter2 + SliderBar.transform.right * mSliderMagnitude / 2) - SliderBar.transform.InverseTransformPoint(mStartCenter2 - SliderBar.transform.right * mSliderMagnitude / 2);
                }
                AlignmentVector = SliderBar.transform.right;

                mCachedRotation = SliderBar.transform.rotation;
            }

            base.ManipulationUpdate(startGesturePosition, currentGesturePosition, startHeadOrigin, startHeadRay, gestureState);
            
            // get the current delta
            float delta =  (CurrentDistance > 0) ? CurrentPercentage : -CurrentPercentage;
            
            // combine the delta with the current slider position so the slider does not start over every time
            if(current_selection==1)mDeltaValue1 = Mathf.Clamp01(delta + mCachedValue1);
            else mDeltaValue2 = Mathf.Clamp01(delta + mCachedValue2);

            if (!Centered)
            {
                if(current_selection==1)SliderValue1 = mDeltaValue1 * mValueSpan;
                else SliderValue2 = mDeltaValue2 * mValueSpan;
            }
            else
            {
                if(current_selection==1)SliderValue1 = mDeltaValue1 * mValueSpan * 2 - mValueSpan;
                else SliderValue2 = mDeltaValue2 * mValueSpan * 2 - mValueSpan;
            }

            UpdateVisuals();

            if (gestureState == HoloToolkit.Examples.InteractiveElements.GestureInteractive.GestureManipulationState.None)
            {
                // gesture ended - cache the current delta
                mCachedValue1 = mDeltaValue1;
                mCachedValue2 = mDeltaValue2;
            }
        }

        /// <summary>
        /// allows the slider to be automated or triggered by a key word
        /// </summary>
        /// <param name="gestureValue"></param>
        public override void setGestureValue(int gestureValue)
        {
            //base.setGestureValue(gestureValue);

            if (GestureStarted)
            {
                return;
            }

            switch (gestureValue)
            {
                case 0:
                    AutoSliderValue = 0;
                    break;
                case 1:
                    AutoSliderValue = 0.5f;
                    break;
                case 2:
                    AutoSliderValue = 1;
                    break;
            }
            AutoSliderTimerCounter = 0;
        }
		
        /// <summary>
        /// set the distance of the slider
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
		public void SetSpan(float min, float max)
		{
			mValueSpan = max - min;
			MaxSliderValue = max;
			MinSliderValue = min;
		}

        /// <summary>
        /// override the slider value
        /// </summary>
        /// <param name="value"></param>

        public void SetSliderValue1(float value)
        {
            if (GestureStarted)
            {
                return;
            }

            mSliderValue1 = Mathf.Clamp(value, MinSliderValue, MaxSliderValue);
            mDeltaValue1 = (SliderValue1-MinSliderValue) / (MaxSliderValue-MinSliderValue);
            UpdateVisuals();
            mCachedValue1 = mDeltaValue1;

        }

        public void SetSliderValue2(float value)
		{
			if(GestureStarted)
			{
				return;
			}
			
			mSliderValue2 = Mathf.Clamp(value, MinSliderValue, MaxSliderValue);
			mDeltaValue2 = (SliderValue2-MinSliderValue) / (MaxSliderValue-MinSliderValue);
			UpdateVisuals();
            mCachedValue2 = mDeltaValue2;

        }

        // update visuals
        private void UpdateVisuals()
        {
            // set the knob position
            mKnobVector1 = mStartSliderPosition1 + Vector3.right * mSliderMagnitude * mDeltaValue1;
            mKnobVector1.z = mStartCenter1.z;

            mKnobVector2 = mStartSliderPosition2 + Vector3.right * mSliderMagnitude * mDeltaValue2;
            mKnobVector2.z = mStartCenter2.z;

            // TODO: Add snapping!

            if (first_Knob != null)
            {
                first_Knob.transform.localPosition = mKnobVector1;
            }

            if (second_Knob != null)
            {
                second_Knob.transform.localPosition = mKnobVector2;
            }


            // set the fill scale and position
            if (SliderFill != null)
            {
                
                Vector3 scale = mSliderFillScale;
                if(mDeltaValue2> mDeltaValue1)scale.x = mSliderFillScale.x * (mDeltaValue2-mDeltaValue1);
                else scale.x = mSliderFillScale.x * (mDeltaValue1 - mDeltaValue2);

                Vector3 position;
                if(SliderValue2 > SliderValue1) position=Vector3.left * (mSliderWidth * 0.5f - mSliderWidth * (mDeltaValue2 + mDeltaValue1) * 0.5f); // left justified;
                else position = Vector3.left * (mSliderWidth * 0.5f - mSliderWidth * (mDeltaValue1 + mDeltaValue2) * 0.5f); // left justified;

                if (Centered)
                {
                    if (SliderValue2 > SliderValue1)
                    {
                        position = Vector3.right * ((mSliderWidth * 0.5f * (mDeltaValue2 + mDeltaValue1 - 0.5f))); // pinned to center, going right
                        scale.x = mSliderFillScale.x * ((mDeltaValue2 + mDeltaValue1 - 0.5f) / 0.5f) * 0.5f;
                    }
                    else
                    {
                        position = Vector3.right * ((mSliderWidth * 0.5f * (mDeltaValue1 + mDeltaValue2 - 0.5f))); // pinned to center, going right
                        scale.x = mSliderFillScale.x * ((mDeltaValue1 + mDeltaValue2 - 0.5f) / 0.5f) * 0.5f;

                    }
                }

                SliderFill.transform.localScale = scale;
                SliderFill.transform.localPosition = position;
            }

            // set the label
            if (Label1 != null)
            {
                float displayValue = SliderValue1;
                if (Centered)
                {
                    displayValue = SliderValue1 * 2 - SliderValue1;
                }

                if (LabelFormat.IndexOf('.') > -1)
                {
                    Label1.text = displayValue.ToString(LabelFormat);

                }
                else
                {
                    Label1.text = Mathf.Round(displayValue).ToString(LabelFormat);
                }
            }
            if (Label2 != null)
            {
                float displayValue = SliderValue2;
                if (Centered)
                {
                    displayValue = SliderValue2 * 2 - SliderValue2;
                }

                if (LabelFormat.IndexOf('.') > -1)
                {
                    Label2.text = displayValue.ToString(LabelFormat);

                }
                else
                {
                    Label2.text = Mathf.Round(displayValue).ToString(LabelFormat);
                }
            }
            OnUpdateEvent.Invoke();
        }

        /// <summary>
        /// Handle automation
        /// </summary>
        protected override void Update()
        {
            if (!GestureStarted)
            {
                if ((GazePoint.transform.position - first_Knob.transform.position).magnitude < (GazePoint.transform.position - second_Knob.transform.position).magnitude)
                {
                    current_selection = 1;
                }
                else
                {
                    current_selection = 2;
                }
            }

            base.Update();

            if (AutoSliderTimerCounter < AutoSliderTime)
            {
                if (GestureStarted)
                {
                    AutoSliderTimerCounter = AutoSliderTime;
                    return;
                }

                AutoSliderTimerCounter += Time.deltaTime;


                if (AutoSliderTimerCounter >= AutoSliderTime)
                {
                    AutoSliderTimerCounter = AutoSliderTime;
                    mCachedValue1 = AutoSliderValue;
                }

                mDeltaValue1 = (AutoSliderValue - mCachedValue1) * AutoSliderTimerCounter / AutoSliderTime + mCachedValue1;

                if (!Centered)
                {
                    SliderValue1 = mDeltaValue1 * mValueSpan+MinSliderValue;
                }
                else
                {
                    SliderValue1 = mDeltaValue1 * mValueSpan * 2 - mValueSpan + MinSliderValue;
                }



                if (AutoSliderTimerCounter >= AutoSliderTime)
                {
                    AutoSliderTimerCounter = AutoSliderTime;
                    mCachedValue2 = AutoSliderValue;
                }

                mDeltaValue2 = (AutoSliderValue - mCachedValue2) * AutoSliderTimerCounter / AutoSliderTime + mCachedValue2;

                if (!Centered)
                {
                    SliderValue2 = mDeltaValue2 * mValueSpan + MinSliderValue;
                }
                else
                {
                    SliderValue2 = mDeltaValue2 * mValueSpan * 2 - mValueSpan + MinSliderValue;
                }


                UpdateVisuals();
            }
        }
    }
}
