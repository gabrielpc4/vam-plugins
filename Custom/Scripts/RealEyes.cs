using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;

namespace MVRPlugin {
    public class RealEyes : MVRScript {
        //Script by VariousScientists42, modified with code from ShortRecognition

        //From the LookAtMe plugin with saccades(VariousScientists42)
        private Transform LookTargetL;
        private Atom person;
        private JSONStorable eyes;
        private FreeControllerV3 eyestarget;
        private Vector3 CamVector;
        private Vector3 ActualOffset;
        protected JSONStorableFloat TimeIntervalMax;
        protected JSONStorableFloat TimeIntervalMin;
        protected JSONStorableFloat MaxRange;
        protected JSONStorableFloat MinRange;
        protected JSONStorableFloat LookAtMeOffset;
        private float ActualRangeX;
        private float ActualRangeY;
        private float ActualRangeZ;
        private float time = 1;
        private bool lookatme;

        //From the original LookAtMe script for reflections(ShortRecognition)
        protected Rigidbody EyeTarget; //eyeTargetController
        protected Rigidbody GazeTarget; //camera to look at
        protected Rigidbody ContainingHead; //person's head


        public override void Init() {
			try {

                if (containingAtom.type != "Person")
                {
                    SuperController.LogError($"Please add this plugin to your target Person Atom, not '{containingAtom.type}'");
                    return;
                }
                pluginLabelJSON.val = "Real Eyes plugin ver 1.0";
                person = containingAtom;
                eyes = person.GetStorableByID("Eyes");
                //eyestarget = (FreeControllerV3)person.GetStorableByID("eyeTargetControl");

                TimeIntervalMax = new JSONStorableFloat("Time Length Max", 3f, 0f, 6f, true, true);
                RegisterFloat(TimeIntervalMax);
                CreateSlider(TimeIntervalMax, false);

                TimeIntervalMin = new JSONStorableFloat("Time Length Min", 1f, 0f, 6f, true, true);
                RegisterFloat(TimeIntervalMin);
                CreateSlider(TimeIntervalMin, false);

                MaxRange = new JSONStorableFloat("Max Look Range", 0.1f, -0.4f, 0.4f, true, true);
                RegisterFloat(MaxRange);
                CreateSlider(MaxRange, true);

                MinRange = new JSONStorableFloat("Min Look Range", -0.1f, -0.4f, 0.4f, true, true);
                RegisterFloat(MinRange);
                CreateSlider(MinRange, true);

                LookAtMeOffset = new JSONStorableFloat("Look At Me Time Length", 0f, 0f, 5f, true, true);
                RegisterFloat(LookAtMeOffset);
                CreateSlider(LookAtMeOffset, true);

                EyeTarget = containingAtom.rigidbodies.First(rb => rb.name == "eyeTargetControl");
                GazeTarget = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "CenterEye");
                ContainingHead = containingAtom.rigidbodies.First(rb => rb.name == "headControl");

                SuperController.LogMessage("Template Loaded");

                }
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}


		void Start() {
			try {
                               
            }
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}
        protected void UpdateParams()
        {
            ActualRangeX = UnityEngine.Random.Range(MinRange.val, MaxRange.val);
            ActualRangeY = UnityEngine.Random.Range(MinRange.val, MaxRange.val);
            ActualRangeZ = UnityEngine.Random.Range(MinRange.val, MaxRange.val);
            ActualOffset = new Vector3(ActualRangeX, ActualRangeY, ActualRangeZ);
            lookatme = UnityEngine.Random.value > 0.5f;
        }
        // Update is called with each rendered frame by Unity
        void Update() {
			try {
                
 
                time -= Time.deltaTime;
                if (time <= 0f)
                {
                    if (lookatme == true)
                    {
                        time = UnityEngine.Random.Range(TimeIntervalMin.val, TimeIntervalMax.val) + LookAtMeOffset.val;
                        UpdateParams();
                    }
                    else
                    {
                        time = UnityEngine.Random.Range(TimeIntervalMin.val, TimeIntervalMax.val);
                        UpdateParams();
                    }
                }
                //First, use ShortRecognition's "find best gaze" method to figure out whether or not to look through the mirror.
                float minAngle = 361f; //overly big number
                Vector3 headForward = ContainingHead.rotation * Vector3.forward; //constant
                Vector3 gazeForward = GazeTarget.rotation * Vector3.forward; //constant
                Vector3 minFacingVector = Vector3.zero; //meaningless but cannot be null
                foreach (Rigidbody reflector in SuperController.singleton.GetAtoms().Where(atom => atom.category == "Reflective").Select(atom => atom.rigidbodies.First()))
                {
                    Vector3 reflectorUp = reflector.rotation * Vector3.up;
                    //throw out any mirror that containing cannot look at
                    if (Vector3.Angle(headForward, Vector3.Dot(reflectorUp, reflector.position - ContainingHead.position) * reflectorUp) > 90f) continue;
                    Vector3 facingVector = Vector3.Dot(reflectorUp, reflector.position - GazeTarget.position) * reflectorUp;
                    float diffAngle = Vector3.Angle(gazeForward, facingVector);
                    if (diffAngle < minAngle)
                    {
                        minAngle = diffAngle;
                        minFacingVector = facingVector;
                    }
                }
                //Then, set the eye target position to the appropriate location.
                if (Vector3.Angle(gazeForward, ContainingHead.position - GazeTarget.position) < minAngle && Vector3.Angle(headForward, GazeTarget.position - ContainingHead.position) <= 90f)
                {
                    EyeTarget.position = GazeTarget.position;
                }
                else if (minAngle <= 90f)
                {
                    EyeTarget.position = 2 * minFacingVector + GazeTarget.position;
                }
                else
                {
                    EyeTarget.position = ContainingHead.position + headForward * 10;
                }
                //Finally, if NOT supposed to be looking at the player, applies offset.
                if (lookatme != true)
                {
                    EyeTarget.position = EyeTarget.position + ActualOffset;
                }
                
                }
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		void FixedUpdate() {
			try {

                }
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		void OnDestroy() {
		}

	}
}