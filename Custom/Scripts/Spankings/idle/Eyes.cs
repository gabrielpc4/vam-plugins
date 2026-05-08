using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;

namespace IdleHelper {
    public class RealEyes : MVRScript {
        //Script by VariousScientists42, modified with code from ShortRecognition
        public static string pluginName = "Eyes";

        //From the LookAtMe plugin with saccades(VariousScientists42)
        private Transform LookTargetL;
        private Atom person;
        private JSONStorable eyes;
        private FreeControllerV3 eyestarget;
        private Vector3 CamVector;
        private Vector3 ActualSOffset;
        private Vector3 ActualLOffset;
        protected JSONStorableFloat TimeIntervalMax;
        protected JSONStorableFloat TimeIntervalMin;
        protected JSONStorableFloat SaccadeSpeed;
        protected JSONStorableFloat MaxSRange;
        protected JSONStorableFloat MinSRange;
        protected JSONStorableFloat MaxLRange;
        protected JSONStorableFloat MinLRange;
        protected JSONStorableFloat SelectTarget;
        protected JSONStorableFloat LookAway;
        private float ActualSRangeX;
        private float ActualSRangeY;
        private float ActualSRangeZ;
        private float ActualLRangeX;
        private float ActualLRangeY;
        private float ActualLRangeZ;
        private float targettime = 1;
        private float saccadetime = 1;
        private bool lookatme;

        //From the original LookAtMe script for reflections(ShortRecognition)
        protected Rigidbody EyeTarget; //eyeTargetController
        protected Rigidbody GazeTarget; //what the character actually looks as
        protected Rigidbody GazeTarget1; //first thing to look at
        protected Rigidbody GazeTarget2; //second thing to look at
        protected Rigidbody ContainingHead; //person's head

        protected Atom AtomTarget1;
        protected JSONStorableStringChooser atomJSON1;
        protected JSONStorableStringChooser receiverJSON1;
        protected Atom AtomTarget2;
        protected JSONStorableStringChooser atomJSON2;
        protected JSONStorableStringChooser receiverJSON2;
        //From MeshedVR's FloatParamRandomizer script
        protected void SyncAtomChocies1()
        {
            List<string> atomChoices1 = new List<string>();
            atomChoices1.Add("Player");
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
                if (atomUID != "[CameraRig]")
                {
                    atomChoices1.Add(atomUID);
                }
            }
            atomJSON1.choices = atomChoices1;
        }
        protected void SyncAtomChocies2()
        {
            List<string> atomChoices2 = new List<string>();
            atomChoices2.Add("Player");
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
                if (atomUID != "[CameraRig]")
                {
                    atomChoices2.Add(atomUID);
                }
            }
            atomJSON2.choices = atomChoices2;
        }

        //Also from MeshedVR's script
        protected void SyncAtom1(string atomUID)
        {
            List<string> receiverChoices1 = new List<string>();
            receiverChoices1.Add("None");
            if (atomUID != null && atomUID != "Player")
            {
                AtomTarget1 = SuperController.singleton.GetAtomByUid(atomUID);
                //SuperController.LogMessage("Atom: " + atomUID);
                if (AtomTarget1 != null)
                {
                    foreach (string receiverChoice in AtomTarget1.GetStorableIDs())
                    {
                        receiverChoices1.Add(receiverChoice);
                        //SuperController.LogMessage("Found receiver " + receiverChoice);
                    }
                }
            }
            else
            {
                AtomTarget1 = null;
                GazeTarget1 = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "CenterEye");
            }
            receiverJSON1.choices = receiverChoices1;
            receiverJSON1.val = "None";
        }
        protected void SyncAtom2(string atomUID)
        {
            List<string> receiverChoices2 = new List<string>();
            receiverChoices2.Add("None");
            if (atomUID != null && atomUID != "Player")
            {
                AtomTarget2 = SuperController.singleton.GetAtomByUid(atomUID);
                if (AtomTarget2 != null)
                {
                    foreach (string receiverChoice in AtomTarget2.GetStorableIDs())
                    {
                        receiverChoices2.Add(receiverChoice);
                    }
                }
            }
            else
            {
                AtomTarget2 = null;
                GazeTarget2 = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "CenterEye");
            }
            receiverJSON2.choices = receiverChoices2;
            receiverJSON2.val = "None";
        }

        protected void SyncReceiver1(string receiverID)
        {
            if (AtomTarget1 != null || receiverID != null || receiverID != "None")
            {
                GazeTarget1 = AtomTarget1.rigidbodies.First(rb => rb.name == receiverID);
                //SuperController.LogMessage("Receiver: " + receiverID);
            }
            else
            {
                GazeTarget1 = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "CenterEye");
            }
        }
        protected void SyncReceiver2(string receiverID)
        {
            if (AtomTarget2 != null || receiverID != null || receiverID != "None")
            {
                GazeTarget2 = AtomTarget2.rigidbodies.First(rb => rb.name == receiverID);
                //SuperController.LogMessage("Receiver: " + receiverID);
            }
            else
            {
                GazeTarget2 = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "CenterEye");
            }
        }

        protected float RandNeg(float num)
        {
            if (UnityEngine.Random.value < 0.5f)
            {
                num = num * -1;
            }
            return num;
        }

        public override void Init() {
			try {

                pluginLabelJSON.val = "Eyes";

                if (containingAtom.type != "Person")
                {
                    SuperController.LogError($"Please add this plugin to your target Person Atom, not '{containingAtom.type}'");
                    return;
                }
                person = containingAtom;
                eyes = person.GetStorableByID("Eyes");
                //eyestarget = (FreeControllerV3)person.GetStorableByID("eyeTargetControl");

                atomJSON1 = new JSONStorableStringChooser("atom1", SuperController.singleton.GetAtomUIDs(), null, "Target Atom 1", SyncAtom1);
                RegisterStringChooser(atomJSON1);
                SyncAtomChocies1();
                UIDynamicPopup dp1 = CreateScrollablePopup(atomJSON1);
                dp1.popupPanelHeight = 1100f;
                // want to always resync the atom choices on opening popup since atoms can be added/removed
                dp1.popup.onOpenPopupHandlers += SyncAtomChocies1;

                receiverJSON1 = new JSONStorableStringChooser("receiver1", null, null, "Atom 1 Point", SyncReceiver1);
                RegisterStringChooser(receiverJSON1);
                dp1 = CreateScrollablePopup(receiverJSON1);
                dp1.popupPanelHeight = 960f;

                atomJSON2 = new JSONStorableStringChooser("atom2", SuperController.singleton.GetAtomUIDs(), null, "Target Atom 2", SyncAtom2);
                RegisterStringChooser(atomJSON2);
                SyncAtomChocies2();
                UIDynamicPopup dp2 = CreateScrollablePopup(atomJSON2);
                dp2.popupPanelHeight = 1100f;
                // want to always resync the atom choices on opening popup since atoms can be added/removed
                dp2.popup.onOpenPopupHandlers += SyncAtomChocies2;

                receiverJSON2 = new JSONStorableStringChooser("receiver2", null, null, "Atom 2 Point", SyncReceiver2);
                RegisterStringChooser(receiverJSON2);
                dp2 = CreateScrollablePopup(receiverJSON2);
                dp2.popupPanelHeight = 960f;

                MaxSRange = new JSONStorableFloat("Max Saccade Range", 0.04f, 0.0f, 0.1f, true, true);
                RegisterFloat(MaxSRange);
                CreateSlider(MaxSRange, true);

                MinSRange = new JSONStorableFloat("Min Saccade Range", 0.01f, 0.0f, 0.1f, true, true);
                RegisterFloat(MinSRange);
                CreateSlider(MinSRange, true);

                MaxLRange = new JSONStorableFloat("Max Look Away Range", 0.5f, 0.4f, 2f, true, true);
                RegisterFloat(MaxLRange);
                CreateSlider(MaxLRange, true);

                MinLRange = new JSONStorableFloat("Min Look Away Range", 0.4f, 0.4f, 2f, true, true);
                RegisterFloat(MinLRange);
                CreateSlider(MinLRange, true);

                SelectTarget = new JSONStorableFloat("Gaze Target 1 Preference", 1f, 0f, 1f, true, true);
                RegisterFloat(SelectTarget);
                CreateSlider(SelectTarget, true);

                LookAway = new JSONStorableFloat("Look Away Preference", 0.10f, 0f, 1f, true, true);
                RegisterFloat(LookAway);
                CreateSlider(LookAway, true);

                TimeIntervalMax = new JSONStorableFloat("Max Time Between Targets", 5f, 0.8f, 10f, true, true);
                RegisterFloat(TimeIntervalMax);
                CreateSlider(TimeIntervalMax, false);

                TimeIntervalMin = new JSONStorableFloat("Min Time Between Targets", 2f, 0.8f, 10f, true, true);
                RegisterFloat(TimeIntervalMin);
                CreateSlider(TimeIntervalMin, false);

                SaccadeSpeed = new JSONStorableFloat("Saccade Slowness", 0.6f, 0.5f, 0.8f, true, true);
                RegisterFloat(SaccadeSpeed);
                CreateSlider(SaccadeSpeed, false);


                EyeTarget = containingAtom.rigidbodies.First(rb => rb.name == "eyeTargetControl");
                ContainingHead = containingAtom.rigidbodies.First(rb => rb.name == "headControl");

                // default to looking at player
                //SyncAtom1("Player");             - why don't these work??
			    //atomJSON1.val = "Player";
                GazeTarget1 = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "CenterEye");
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
        protected void UpdateSaccade()
        {
            ActualSRangeX = RandNeg(UnityEngine.Random.Range(MinSRange.val, MaxSRange.val));
            ActualSRangeY = RandNeg(UnityEngine.Random.Range(MinSRange.val, MaxSRange.val));
            ActualSRangeZ = RandNeg(UnityEngine.Random.Range(MinSRange.val, MaxSRange.val));
            ActualSOffset = new Vector3(ActualSRangeX, ActualSRangeY, ActualSRangeZ);
        }
        protected void UpdateTarget()
        {
            ActualLRangeX = RandNeg(UnityEngine.Random.Range(MinLRange.val, MaxLRange.val));
            ActualLRangeY = RandNeg(UnityEngine.Random.Range(MinLRange.val, MaxLRange.val));
            ActualLRangeZ = UnityEngine.Random.Range(RandNeg(MinLRange.val), MaxLRange.val) + 1;
            ActualLOffset = new Vector3(ActualLRangeX, ActualLRangeY, ActualLRangeZ);
            lookatme = UnityEngine.Random.value > LookAway.val;
            if (lookatme)
            {
                if (UnityEngine.Random.value < SelectTarget.val)
                {
                    GazeTarget = GazeTarget1;
                }
                else
                {
                    GazeTarget = GazeTarget2;
                }
            }
        }
        // Update is called with each rendered frame by Unity
        void Update() {
			try {
                saccadetime -= Time.deltaTime;
                if (saccadetime <= 0f)
                {
                    saccadetime = SaccadeSpeed.val;
                    UpdateSaccade();
                }
                targettime -= Time.deltaTime;
                if (targettime <= 0f)
                {
                    targettime = UnityEngine.Random.Range(TimeIntervalMin.val, TimeIntervalMax.val);
                    UpdateTarget();
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
                    EyeTarget.position = GazeTarget.position + ActualSOffset;
                }
                else if (minAngle <= 90f)
                {
                    EyeTarget.position = 2 * minFacingVector + GazeTarget.position + ActualSOffset;
                }
                else
                {
                    EyeTarget.position = ActualSOffset + ContainingHead.position + headForward * 10;
                }
                //Finally, if NOT supposed to be looking at the player, applies offset.
                if (lookatme != true)
                {
                    EyeTarget.position = ContainingHead.position + ActualLOffset + ActualSOffset;
                }

                }
			catch (Exception e) {
				//SuperController.LogError("Exception caught: " + e);
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
