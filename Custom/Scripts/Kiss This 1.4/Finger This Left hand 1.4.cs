//By Spacedsog, based on the Kiss plugin by Extraltodeusand modified by TimelordToby
//Attach this script to a person, select an atom in the menu for the person to bend their fingers when the atom is near

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using System.Threading;
using System.Text.RegularExpressions;

namespace FingerThisLeftHand {
	public class B : MVRScript {
		protected JSONStorableStringChooser uiFocusTarget;
		private static Atom currentAtom;
		private static string currentAtomName = "None";
		private Rigidbody handRigidbody;
		protected JSONStorableFloat intensitySlider;
		protected JSONStorableFloat defMinDistSlider;
		protected JSONStorableFloat defDistSlider;
		protected JSONStorableFloat leftPinkyFingerBendSlider;
		protected JSONStorableFloat leftRingFingerBendSlider;
		protected JSONStorableFloat leftMidFingerBendSlider;
		protected JSONStorableFloat leftIndexFingerBendSlider;
		protected JSONStorableFloat leftFingersInOutSlider;
		protected JSONStorableFloat leftThumbBendSlider;
		protected JSONStorableFloat leftThumbFistSlider;
		protected JSONStorableFloat leftThumbInOutSlider;
		protected JSONStorableFloat leftHandChopSlider;
		protected JSONStorableBool triggerByDistance;
		protected float intensityPower;
		protected float previousState;
		protected float newState;

				protected void distanceVariation()
				{
					currentAtom = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
					if (currentAtom == null) return;
					Vector3 SelectedAtomPos = currentAtom.mainController.transform.position;
					handRigidbody = containingAtom.rigidbodies.First(rb => rb.name == "lHand");
					Vector3 handPos = handRigidbody.transform.position;
					float dist = Vector3.Distance(SelectedAtomPos, handPos) - defMinDistSlider.val;
					if (dist < defDistSlider.val)
					{
						intensitySlider.val = 1-(1/defDistSlider.val*dist);
						
					}
				}
				
				protected void handControl()
				{
					newState = intensitySlider.val + leftPinkyFingerBendSlider.val + leftRingFingerBendSlider.val + leftMidFingerBendSlider.val + leftIndexFingerBendSlider.val + leftFingersInOutSlider.val + leftThumbBendSlider.val + leftThumbFistSlider.val + leftThumbInOutSlider.val + leftHandChopSlider.val;
					if (newState != previousState)
					{
							previousState = newState;
							JSONStorable geometry = containingAtom.GetStorableByID("geometry");
							DAZCharacterSelector character = geometry as DAZCharacterSelector;
							GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

							DAZMorph leftPinkyFingerBend = morphControl.GetMorphByDisplayName("Left Pinky Finger Bend");
							DAZMorph leftRingFingerBend  = morphControl.GetMorphByDisplayName("Left Ring Finger Bend");
							DAZMorph leftMidFingerBend  = morphControl.GetMorphByDisplayName("Left Mid Finger Bend");
							DAZMorph leftIndexFingerBend   = morphControl.GetMorphByDisplayName("Left Index Finger Bend");
							DAZMorph leftFingersInOut   = morphControl.GetMorphByDisplayName("Left Fingers In-Out");
							DAZMorph leftThumbBend   = morphControl.GetMorphByDisplayName("Left Thumb Bend");
							DAZMorph leftThumbFist   = morphControl.GetMorphByDisplayName("Left Thumb Fist");
							DAZMorph leftThumbInOut   = morphControl.GetMorphByDisplayName("Left Thumb In-Out");
							DAZMorph leftHandChop   = morphControl.GetMorphByDisplayName("Left Hand Chop");

							if (leftPinkyFingerBendSlider.val != 0)
								leftPinkyFingerBend.morphValue = leftPinkyFingerBendSlider.val * intensitySlider.val;
							if (leftRingFingerBendSlider.val != 0)
								leftRingFingerBend.morphValue  = leftRingFingerBendSlider.val  * intensitySlider.val;
							if (leftMidFingerBendSlider.val != 0)
								leftMidFingerBend.morphValue  = leftMidFingerBendSlider.val  * intensitySlider.val;
							if (leftIndexFingerBendSlider.val != 0)
								leftIndexFingerBend.morphValue   = leftIndexFingerBendSlider.val   * intensitySlider.val;
							if (leftFingersInOutSlider.val != 0)
								leftFingersInOut.morphValue   = leftFingersInOutSlider.val   * intensitySlider.val;
							if (leftThumbBendSlider.val != 0)
								leftThumbBend.morphValue   = leftThumbBendSlider.val   * intensitySlider.val;
							if (leftThumbFistSlider.val != 0)
								leftThumbFist.morphValue   = leftThumbFistSlider.val   * intensitySlider.val;
							if (leftThumbInOutSlider.val != 0)
								leftThumbInOut.morphValue   = leftThumbInOutSlider.val   * intensitySlider.val;
							if (leftHandChopSlider.val > 0)
								leftHandChop.morphValue   = leftHandChopSlider.val   * intensitySlider.val;
					}
				}

        protected void FixedUpdate()
        {

						if (triggerByDistance.val && uiFocusTarget.val != "none")

							distanceVariation();
						handControl();

 }

        public override void Init() {
            try
            {
			List<string> targetChoices = new List<string>();
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
				currentAtom = SuperController.singleton.GetAtomByUid(atomUID);
                if (currentAtom != containingAtom && atomUID != null)
                {
					targetChoices.Add(atomUID);
				}
			}
			uiFocusTarget = new JSONStorableStringChooser("Sex Giver", targetChoices, "None", "Choose target atom");
			RegisterStringChooser(uiFocusTarget);
			UIDynamicPopup udp = CreateScrollablePopup(uiFocusTarget, true);
			UIDynamic spacer = CreateSpacer(true);
            spacer.height = 360f;

			#region Sliders
                JSONStorable geometry = containingAtom.GetStorableByID("geometry");
                DAZCharacterSelector character = geometry as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

                intensitySlider = new JSONStorableFloat("Master Intensity", 0, 0f, 1.0f, true);
                intensitySlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(intensitySlider);
                CreateSlider(intensitySlider, false);

				defMinDistSlider = new JSONStorableFloat("Minimum trigger distance", 0.15f, 0, 3f, true);
				defMinDistSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(defMinDistSlider);
				CreateSlider(defMinDistSlider, false);

				defDistSlider = new JSONStorableFloat("Trigger distance", 0.25f, 0, 3f, true);
				defDistSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(defDistSlider);
				CreateSlider(defDistSlider, false);

				triggerByDistance = new JSONStorableBool("Trigger by target distance", true);
				RegisterBool(triggerByDistance);
				CreateToggle((triggerByDistance), false);

				
				UIDynamic spacer2 = CreateSpacer(false);
				spacer2.height = 5f;

				leftIndexFingerBendSlider = new JSONStorableFloat("Left Index Finger Bend variation", 0f, -1f, 1f, true);
				leftIndexFingerBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(leftIndexFingerBendSlider);
				CreateSlider(leftIndexFingerBendSlider, false);

				leftMidFingerBendSlider = new JSONStorableFloat("Left Mid Finger Bend variation", 0f, -1f, 1f, true);
				leftMidFingerBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(leftMidFingerBendSlider);
				CreateSlider(leftMidFingerBendSlider, false);

				leftRingFingerBendSlider = new JSONStorableFloat("Left Ring Finger Bend variation", 0f, -1f, 1f, true);
				leftRingFingerBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(leftRingFingerBendSlider);
				CreateSlider(leftRingFingerBendSlider, false);
				
				leftPinkyFingerBendSlider = new JSONStorableFloat("Left Pinky Finger Bend variation", 0f, -1f, 1f, true);
				leftPinkyFingerBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(leftPinkyFingerBendSlider);
				CreateSlider(leftPinkyFingerBendSlider, false);
				
				leftFingersInOutSlider = new JSONStorableFloat("Left Fingers In-Out variation", 0f, -1f, 1f, true);
				leftFingersInOutSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(leftFingersInOutSlider);
				CreateSlider(leftFingersInOutSlider, true);
				
				leftThumbBendSlider = new JSONStorableFloat("Left Thumb Bend variation", 0f, -1f, 1f, true);
				leftThumbBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(leftThumbBendSlider);
				CreateSlider(leftThumbBendSlider, true);
				
				leftThumbFistSlider = new JSONStorableFloat("Left Thumb Fist variation", 0f, -1f, 1f, true);
				leftThumbFistSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(leftThumbFistSlider);
				CreateSlider(leftThumbFistSlider, true);
				
				leftThumbInOutSlider = new JSONStorableFloat("Left Thumb In-Out variation", 0f, -1f, 1f, true);
				leftThumbInOutSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(leftThumbInOutSlider);
				CreateSlider(leftThumbInOutSlider, true);
				
				leftHandChopSlider = new JSONStorableFloat("Left Hand Chop variation", 0f, 0f, 1f, true);
				leftHandChopSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(leftHandChopSlider);
				CreateSlider(leftHandChopSlider, true);


                #endregion
            }

			catch (Exception e)
				{
					SuperController.LogError("Exception caught: " + e);
				}
			}

        void Start()
        {
			previousState = intensitySlider.val + leftPinkyFingerBendSlider.val + leftRingFingerBendSlider.val + leftMidFingerBendSlider.val + leftIndexFingerBendSlider.val + leftFingersInOutSlider.val + leftThumbBendSlider.val +leftThumbFistSlider.val + leftThumbInOutSlider.val + leftHandChopSlider.val;
			newState      = intensitySlider.val + leftPinkyFingerBendSlider.val + leftRingFingerBendSlider.val + leftMidFingerBendSlider.val + leftIndexFingerBendSlider.val + leftFingersInOutSlider.val + leftThumbBendSlider.val + leftThumbFistSlider.val + leftThumbInOutSlider.val + leftHandChopSlider.val;

		}
    }
}
