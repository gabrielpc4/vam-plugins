using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using System.Threading;
using System.Text.RegularExpressions;

namespace TTobysKissPlugin
{
    public class B : MVRScript
    {


        protected JSONStorableFloat intensitySlider;

        protected JSONStorableFloat defMinDistSlider;
        protected JSONStorableFloat defDistSlider;

        protected JSONStorableFloat mouthNarrowSlider;
        protected JSONStorableFloat eyesClosedSlider;
        protected JSONStorableFloat lipsPuckerSlider;
        protected JSONStorableFloat mouthOpenSlider;
        protected JSONStorableFloat tongueLengthSlider;
        protected JSONStorableFloat tongueNarrowWideSlider;
        protected JSONStorableFloat tongueRaiseLowerSlider;
        protected JSONStorableFloat tongueBendTipSlider;
        protected JSONStorableFloat tongueBendTipSliderAnimate;
        protected JSONStorableFloat
tongueBendTipSliderAnimateMin;
        protected JSONStorableFloat
tongueBendTipSliderAnimateMax;
        protected JSONStorableFloat emotion1Slider;
        protected JSONStorableFloat emotion2Slider;
        protected JSONStorableFloat headDrive;

        protected JSONStorableBool triggerByDistance;
        protected JSONStorableBool blinkControl;

        private FreeControllerV3 head;


        protected float intensityPower;
        protected float previousState;
        protected float newState;
        protected int animationDirection;

        protected bool wasKissing = false;
        protected bool wasVAMAutoBlink = false;

        protected void distanceVariation()
        {
            Vector3 camPos = SuperController.singleton.lookCamera.transform.position;
            Vector3 headPos = head.transform.position;

            float dist = Vector3.Distance(camPos, headPos) - defMinDistSlider.val;

            float dist2 = (DistanceToParts() - defMinDistSlider.val);

            if (dist2 < dist) dist = dist2;



            if (dist < defDistSlider.val)
            {
                if (!wasKissing)
                {
                    wasKissing = true;
                    if (blinkControl.val)
                    {
                        containingAtom.GetStorableByID("EyelidControl").GetBoolJSONParam("blinkEnabled").val = false;
                    }
                }

                intensitySlider.val = 1 - (1 / defDistSlider.val * dist);

                if (headDrive.val > 0)
                {
                    float distHead = Vector3.Distance(camPos, headPos) - defMinDistSlider.val / 2;
                    head.jointRotationDriveXTarget = (1 - (1 / defDistSlider.val * distHead)) * headDrive.val;
                }
            }
            else
            {
                if (wasKissing)
                {
                    wasKissing = false;
                    if (blinkControl.val)
                    {
                        containingAtom.GetStorableByID("EyelidControl").GetBoolJSONParam("blinkEnabled").val = wasVAMAutoBlink;
                    }
                }

                if (intensitySlider.val != 0)
                    intensitySlider.val = 0;
            }
        }


        protected void kissControl()
        {
            if (intensitySlider.val >= 0.5f)
            {
                if (animationDirection == 1)
                {
                    tongueBendTipSlider.val += tongueBendTipSliderAnimate.val * 5f * Time.fixedDeltaTime * intensitySlider.val;
                    if (tongueBendTipSlider.val >= tongueBendTipSliderAnimateMax.val) animationDirection = -1;
                }
                else
                {
                    tongueBendTipSlider.val -= tongueBendTipSliderAnimate.val * 5f * Time.fixedDeltaTime * intensitySlider.val;
                    if (tongueBendTipSlider.val <= tongueBendTipSliderAnimateMin.val) animationDirection = 1;
                }
            }
            else
            {
                float amount = tongueBendTipSliderAnimate.val * 5f * Time.fixedDeltaTime;
                if (tongueBendTipSlider.val >= amount)
                {
                    tongueBendTipSlider.val -= amount;
                }
            }

            newState = intensitySlider.val + tongueNarrowWideSlider.val + tongueRaiseLowerSlider.val + mouthNarrowSlider.val + eyesClosedSlider.val + lipsPuckerSlider.val + mouthOpenSlider.val + tongueLengthSlider.val + tongueBendTipSlider.val + emotion1Slider.val + emotion2Slider.val;
            if (newState != previousState)
            {
                previousState = newState;
                JSONStorable geometry = containingAtom.GetStorableByID("geometry");
                DAZCharacterSelector character = geometry as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

                DAZMorph mouthNarrow = morphControl.GetMorphByDisplayName("Mouth Narrow"); //
                DAZMorph eyesClosed = morphControl.GetMorphByDisplayName("Eyes Closed"); //
                DAZMorph lipsPucker = morphControl.GetMorphByDisplayName("Lips Pucker"); //
                DAZMorph mouthOpen = morphControl.GetMorphByDisplayName("Mouth Open"); //
                DAZMorph tongueLength = morphControl.GetMorphByDisplayName("Tongue Length"); //
                DAZMorph tongueNarrowWide = morphControl.GetMorphByDisplayName("Tongue Narrow-Wide");
                DAZMorph tongueRaiseLower = morphControl.GetMorphByDisplayName("Tongue Raise-Lower");
                DAZMorph tongueBendTip = morphControl.GetMorphByDisplayName("Tongue Bend Tip");
                DAZMorph emotion1 = morphControl.GetMorphByDisplayName("Afraid");
                DAZMorph emotion2 = morphControl.GetMorphByDisplayName("Smile Full Face");

                if (mouthNarrowSlider.val > 0)
                    mouthNarrow.morphValue = mouthNarrowSlider.val * intensitySlider.val;
                if (eyesClosedSlider.val > 0)
                    eyesClosed.morphValue = eyesClosedSlider.val * intensitySlider.val;
                if (lipsPuckerSlider.val > 0)
                    lipsPucker.morphValue = lipsPuckerSlider.val * intensitySlider.val;
                if (mouthOpenSlider.val > 0)
                    mouthOpen.morphValue = mouthOpenSlider.val * intensitySlider.val;
                if (tongueLengthSlider.val > 0)
                    tongueLength.morphValue = tongueLengthSlider.val * intensitySlider.val;
                if (tongueNarrowWideSlider.val != 0)
                    tongueNarrowWide.morphValue = tongueNarrowWideSlider.val * intensitySlider.val;
                if (tongueRaiseLowerSlider.val != 0)
                    tongueRaiseLower.morphValue = tongueRaiseLowerSlider.val * intensitySlider.val;
                if (tongueBendTipSlider.val > 0)
                    tongueBendTip.morphValue = tongueBendTipSlider.val * intensitySlider.val;
                if (emotion1Slider.val > 0)
                    emotion1.morphValue = emotion2Slider.val * intensitySlider.val;
                if (emotion2Slider.val > 0)
                    emotion2.morphValue = emotion2Slider.val * intensitySlider.val;
            }
        }


        float DistanceToParts()
        {
            float minDistance = 1000000;
            FreeControllerV3 nearestPart = null;

            SuperController.singleton.GetAtoms()
            .Where((otherAtom) =>
            {
                return otherAtom.GetStorableByID("headControl") != null && otherAtom != containingAtom;
            }).ToList()
            .Select((containingAtom) =>
            {
                return (containingAtom.GetStorableByID("headControl") as FreeControllerV3);
            }).ToList()
            .ForEach((controller) =>
            {
                float distance = (controller.transform.position - head.transform.position).magnitude;

                bool partOK = true;
                if (distance > minDistance)
                {
                    partOK = false;
                }


                if (partOK)
                {
                    minDistance = distance;
                    nearestPart = controller;
                }
            });


            SuperController.singleton.GetAtoms()
            .Where((otherAtom) =>
            {
                return otherAtom.GetStorableByID("penisTipControl") != null && otherAtom != containingAtom;
            }).ToList()
            .Select((containingAtom) =>
            {
                return (containingAtom.GetStorableByID("penisTipControl") as FreeControllerV3);
            }).ToList()
            .ForEach((controller) =>
            {
                float distance = (controller.transform.position - head.transform.position).magnitude;

                bool partOK = true;
                if (distance > minDistance)
                {
                    partOK = false;
                }

                if (partOK)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestPart = controller;
                    }
                }
            });


            /*SuperController.singleton.GetAtoms()
            .Where((otherAtom) => 
            {
                return otherAtom.GetStorableByID("lHandControl") != null && otherAtom!=containingAtom;
            }).ToList()
            .Select((containingAtom) =>
            {
                return (containingAtom.GetStorableByID("lHandControl") as FreeControllerV3);
            }).ToList()
            .ForEach((controller) =>
            {
                float distance = (controller.transform.position - head.transform.position).magnitude;

                bool partOK = true;
                if (distance > minDistance)
                {
                    partOK = false;
                }

                if (partOK) {
                    if (distance < minDistance) {
                        minDistance = distance;
                        nearestPart = controller;
                    }
                }
            });

    
    
            SuperController.singleton.GetAtoms()
            .Where((otherAtom) => 
            {
                return otherAtom.GetStorableByID("rHandControl") != null && otherAtom!=containingAtom;
            }).ToList()
            .Select((containingAtom) =>
            {
                return (containingAtom.GetStorableByID("rHandControl") as FreeControllerV3);
            }).ToList()
            .ForEach((controller) =>
            {
                float distance = (controller.transform.position - head.transform.position).magnitude;

                bool partOK = true;
                if (distance > minDistance)
                {
                    partOK = false;
                }

                if (partOK) {
                    if (distance < minDistance) {
                        minDistance = distance;
                        nearestPart = controller;
                    }
                }
            });*/

            return minDistance;
        }



        protected void FixedUpdate()
        {
            head = containingAtom.GetStorableByID("headControl") as FreeControllerV3;

            if (triggerByDistance.val)
                distanceVariation();
            kissControl();
        }

        public override void Init()
        {
            try
            {
                #region Sliders

                JSONStorable geometry = containingAtom.GetStorableByID("geometry");
                DAZCharacterSelector character = geometry as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

                intensitySlider = new JSONStorableFloat("Master Intensity", 0, 0f, 1.0f, true);
                intensitySlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(intensitySlider);
                CreateSlider(intensitySlider, false);

                defMinDistSlider = new JSONStorableFloat("Minimum trigger distance", 0.082f, 0, 3f, true);
                defMinDistSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(defMinDistSlider);
                CreateSlider(defMinDistSlider, false);

                defDistSlider = new JSONStorableFloat("Trigger distance", 0.419f, 0, 3f, true);
                defDistSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(defDistSlider);
                CreateSlider(defDistSlider, false);

                triggerByDistance = new JSONStorableBool("Trigger by camera distance", true);
                RegisterBool(triggerByDistance);
                CreateToggle((triggerByDistance), false);

                blinkControl = new JSONStorableBool("Pause blink when kiss", true);
                RegisterBool(blinkControl);
                CreateToggle((blinkControl), false);

                headDrive = new JSONStorableFloat("Head joint drive X", -35f, -35f, 150f, true);
                headDrive.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(headDrive);
                CreateSlider(headDrive, true);

                lipsPuckerSlider = new JSONStorableFloat("Lips pucker variation", 0.517f, 0f, 1f, true);
                lipsPuckerSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(lipsPuckerSlider);
                CreateSlider(lipsPuckerSlider, true);

                mouthNarrowSlider = new JSONStorableFloat("mouth narrow variation", 0.0638f, 0f, 1f, true);
                mouthNarrowSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(mouthNarrowSlider);
                CreateSlider(mouthNarrowSlider, true);

                eyesClosedSlider = new JSONStorableFloat("Eyes closed variation", 1f, 0f, 1f, true);
                eyesClosedSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(eyesClosedSlider);
                CreateSlider(eyesClosedSlider, true);

                tongueLengthSlider = new JSONStorableFloat("Tongue length variation", 0.303f, 0f, 1f, true);
                tongueLengthSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(tongueLengthSlider);
                CreateSlider(tongueLengthSlider, true);

                tongueNarrowWideSlider = new JSONStorableFloat("Tongue narrow wide variation", -0.54f, -1f, 1f, true);
                tongueNarrowWideSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(tongueNarrowWideSlider);
                CreateSlider(tongueNarrowWideSlider, true);

                tongueRaiseLowerSlider = new JSONStorableFloat("Tongue raise lower variation", 0.208f, -1f, 2f, true);
                tongueRaiseLowerSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(tongueRaiseLowerSlider);
                CreateSlider(tongueRaiseLowerSlider, true);

                mouthOpenSlider = new JSONStorableFloat("mouth open variation", 0.973f, 0f, 4f, true);
                mouthOpenSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(mouthOpenSlider);
                CreateSlider(mouthOpenSlider, true);

                tongueBendTipSlider = new JSONStorableFloat("Tongue bend variation", 0.005f, 0f, 1f, true);
                tongueBendTipSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(tongueBendTipSlider);
                CreateSlider(tongueBendTipSlider, true);

                tongueBendTipSliderAnimate = new JSONStorableFloat("Tongue bend animation", 0.162f, 0f, 1f, true);
                tongueBendTipSliderAnimate.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(tongueBendTipSliderAnimate);
                CreateSlider(tongueBendTipSliderAnimate, true);

                tongueBendTipSliderAnimateMin = new JSONStorableFloat("Tongue bend animation min", 0f, 0f, 1f, true);
                tongueBendTipSliderAnimateMin.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(tongueBendTipSliderAnimateMin);
                CreateSlider(tongueBendTipSliderAnimateMin, true);


                tongueBendTipSliderAnimateMax = new JSONStorableFloat("Tongue bend animation max", 0.5f, 0f, 1f, true);
                tongueBendTipSliderAnimateMax.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(tongueBendTipSliderAnimateMax);
                CreateSlider(tongueBendTipSliderAnimateMax, true);



                emotion1Slider = new JSONStorableFloat("Emotion 1 variation", 0f, 0f, 1f, true);
                emotion1Slider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(emotion1Slider);
                CreateSlider(emotion1Slider, true);

                emotion2Slider = new JSONStorableFloat("Emotion 2 variation", 0f, 0f, 1f, true);
                emotion2Slider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(emotion2Slider);
                CreateSlider(emotion2Slider, true);

                #endregion
            }

            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        void Start()
        {
            previousState = intensitySlider.val + tongueNarrowWideSlider.val + tongueRaiseLowerSlider.val + mouthNarrowSlider.val + eyesClosedSlider.val + lipsPuckerSlider.val + mouthOpenSlider.val + tongueLengthSlider.val + tongueBendTipSlider.val + emotion1Slider.val + emotion2Slider.val;
            newState = intensitySlider.val + tongueNarrowWideSlider.val + tongueRaiseLowerSlider.val + mouthNarrowSlider.val + eyesClosedSlider.val + lipsPuckerSlider.val + mouthOpenSlider.val + tongueLengthSlider.val + tongueBendTipSlider.val + emotion1Slider.val + emotion2Slider.val;
        }
    }
}
