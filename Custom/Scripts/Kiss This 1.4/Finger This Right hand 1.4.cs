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

namespace FingerThisRightHand {
	public class B : MVRScript {
		protected JSONStorableStringChooser uiFocusTarget;
		private static Atom currentAtom;
		private static string currentAtomName = "None";
		private Rigidbody handRigidbody;
		protected JSONStorableFloat intensitySlider;
		protected JSONStorableFloat defMinDistSlider;
		protected JSONStorableFloat defDistSlider;
		protected JSONStorableFloat rightPinkyFingerBendSlider;
		protected JSONStorableFloat rightRingFingerBendSlider;
		protected JSONStorableFloat rightMidFingerBendSlider;
		protected JSONStorableFloat rightIndexFingerBendSlider;
		protected JSONStorableFloat rightFingersInOutSlider;
		protected JSONStorableFloat rightThumbBendSlider;
		protected JSONStorableFloat rightThumbFistSlider;
		protected JSONStorableFloat rightThumbInOutSlider;
		protected JSONStorableFloat rightHandChopSlider;
		protected JSONStorableBool triggerByDistance;
		protected float intensityPower;
		protected float previousState;
		protected float newState;

				protected void distanceVariation()
				{
					currentAtom = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
					if (currentAtom == null) return;
					Vector3 SelectedAtomPos = currentAtom.mainController.transform.position;
					handRigidbody = containingAtom.rigidbodies.First(rb => rb.name == "rHand");
					Vector3 handPos = handRigidbody.transform.position;
					float dist = Vector3.Distance(SelectedAtomPos, handPos) - defMinDistSlider.val;
					if (dist < defDistSlider.val)
					{
						intensitySlider.val = 1-(1/defDistSlider.val*dist);
						
					}
				}
				
				protected void handControl()
				{
					newState = intensitySlider.val + rightPinkyFingerBendSlider.val + rightRingFingerBendSlider.val + rightMidFingerBendSlider.val + rightIndexFingerBendSlider.val + rightFingersInOutSlider.val + rightThumbBendSlider.val + rightThumbFistSlider.val + rightThumbInOutSlider.val + rightHandChopSlider.val;
					if (newState != previousState)
					{
							previousState = newState;
							JSONStorable geometry = containingAtom.GetStorableByID("geometry");
							DAZCharacterSelector character = geometry as DAZCharacterSelector;
							GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

							DAZMorph rightPinkyFingerBend = morphControl.GetMorphByDisplayName("Right Pinky Finger Bend");
							DAZMorph rightRingFingerBend  = morphControl.GetMorphByDisplayName("Right Ring Finger Bend");
							DAZMorph rightMidFingerBend  = morphControl.GetMorphByDisplayName("Right Mid Finger Bend");
							DAZMorph rightIndexFingerBend   = morphControl.GetMorphByDisplayName("Right Index Finger Bend");
							DAZMorph rightFingersInOut   = morphControl.GetMorphByDisplayName("Right Fingers In-Out");
							DAZMorph rightThumbBend   = morphControl.GetMorphByDisplayName("Right Thumb Bend");
							DAZMorph rightThumbFist   = morphControl.GetMorphByDisplayName("Right Thumb Fist");
							DAZMorph rightThumbInOut   = morphControl.GetMorphByDisplayName("Right Thumb In-Out");
							DAZMorph rightHandChop   = morphControl.GetMorphByDisplayName("Right Hand Chop");

							if (rightPinkyFingerBendSlider.val != 0)
								rightPinkyFingerBend.morphValue = rightPinkyFingerBendSlider.val * intensitySlider.val;
							if (rightRingFingerBendSlider.val != 0)
								rightRingFingerBend.morphValue  = rightRingFingerBendSlider.val  * intensitySlider.val;
							if (rightMidFingerBendSlider.val != 0)
								rightMidFingerBend.morphValue  = rightMidFingerBendSlider.val  * intensitySlider.val;
							if (rightIndexFingerBendSlider.val != 0)
								rightIndexFingerBend.morphValue   = rightIndexFingerBendSlider.val   * intensitySlider.val;
							if (rightFingersInOutSlider.val != 0)
								rightFingersInOut.morphValue   = rightFingersInOutSlider.val   * intensitySlider.val;
							if (rightThumbBendSlider.val != 0)
								rightThumbBend.morphValue   = rightThumbBendSlider.val   * intensitySlider.val;
							if (rightThumbFistSlider.val != 0)
								rightThumbFist.morphValue   = rightThumbFistSlider.val   * intensitySlider.val;
							if (rightThumbInOutSlider.val != 0)
								rightThumbInOut.morphValue   = rightThumbInOutSlider.val   * intensitySlider.val;
							if (rightHandChopSlider.val > 0)
								rightHandChop.morphValue   = rightHandChopSlider.val   * intensitySlider.val;
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

				rightIndexFingerBendSlider = new JSONStorableFloat("Right Index Finger Bend variation", 0f, -1f, 1f, true);
				rightIndexFingerBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(rightIndexFingerBendSlider);
				CreateSlider(rightIndexFingerBendSlider, false);

				rightMidFingerBendSlider = new JSONStorableFloat("Right Mid Finger Bend variation", 0f, -1f, 1f, true);
				rightMidFingerBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(rightMidFingerBendSlider);
				CreateSlider(rightMidFingerBendSlider, false);

				rightRingFingerBendSlider = new JSONStorableFloat("Right Ring Finger Bend variation", 0f, -1f, 1f, true);
				rightRingFingerBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(rightRingFingerBendSlider);
				CreateSlider(rightRingFingerBendSlider, false);
				
				rightPinkyFingerBendSlider = new JSONStorableFloat("Right Pinky Finger Bend variation", 0f, -1f, 1f, true);
				rightPinkyFingerBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(rightPinkyFingerBendSlider);
				CreateSlider(rightPinkyFingerBendSlider, false);
				
				rightFingersInOutSlider = new JSONStorableFloat("Right Fingers In-Out variation", 0f, -1f, 1f, true);
				rightFingersInOutSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(rightFingersInOutSlider);
				CreateSlider(rightFingersInOutSlider, true);
				
				rightThumbBendSlider = new JSONStorableFloat("Right Thumb Bend variation", 0f, -1f, 1f, true);
				rightThumbBendSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(rightThumbBendSlider);
				CreateSlider(rightThumbBendSlider, true);
				
				rightThumbFistSlider = new JSONStorableFloat("Right Thumb Fist variation", 0f, -1f, 1f, true);
				rightThumbFistSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(rightThumbFistSlider);
				CreateSlider(rightThumbFistSlider, true);
				
				rightThumbInOutSlider = new JSONStorableFloat("Right Thumb In-Out variation", 0f, -1f, 1f, true);
				rightThumbInOutSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(rightThumbInOutSlider);
				CreateSlider(rightThumbInOutSlider, true);
				
				rightHandChopSlider = new JSONStorableFloat("Right Hand Chop variation", 0f, 0f, 1f, true);
				rightHandChopSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(rightHandChopSlider);
				CreateSlider(rightHandChopSlider, true);


                #endregion
            }

			catch (Exception e)
				{
					SuperController.LogError("Exception caught: " + e);
				}
			}

        void Start()
        {
			previousState = intensitySlider.val + rightPinkyFingerBendSlider.val + rightRingFingerBendSlider.val + rightMidFingerBendSlider.val + rightIndexFingerBendSlider.val + rightFingersInOutSlider.val + rightThumbBendSlider.val + rightThumbFistSlider.val + rightThumbInOutSlider.val + rightHandChopSlider.val;
			newState      = intensitySlider.val + rightPinkyFingerBendSlider.val + rightRingFingerBendSlider.val + rightMidFingerBendSlider.val + rightIndexFingerBendSlider.val + rightFingersInOutSlider.val + rightThumbBendSlider.val + rightThumbFistSlider.val + rightThumbInOutSlider.val + rightHandChopSlider.val;
            
		}
    }
}
