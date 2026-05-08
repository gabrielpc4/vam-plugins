//By Spacedsog, based on the Kiss plugin by Extraltodeusand modified by TimelordToby
//Attach this script to a person, select an atom in the menu for the person to bend their fingers when the atom is near

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace FingerMotionRightHand {
	public class FingerMotionRightHand : MVRScript {
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
		protected float previousState;
		protected float newState;
		
			protected void SyncAtomChocies() {
			List<string> targetChoices = new List<string>();
			targetChoices.Add("None");
			foreach (string atomUID in SuperController.singleton.GetAtomUIDs()) {
				if (atomUID != null)
				{
				targetChoices.Add(atomUID);
				}
			}
			uiFocusTarget.choices = targetChoices;
			}

				protected void distanceVariation()
				{
					Vector3 SelectedAtomPos = currentAtom.mainController.transform.position;
					handRigidbody = containingAtom.rigidbodies.First(rb => rb.name == "rHand");
					Vector3 handPos = handRigidbody.transform.position;
					float dist = Vector3.Distance(SelectedAtomPos, handPos) - defMinDistSlider.val;
					if (dist < defDistSlider.val ||  intensitySlider.val > 0.01f)
					{
						intensitySlider.val = 1-(1/defDistSlider.val*dist);
						
					}
				}
				
				protected void handControl()
				{
					newState = intensitySlider.val + rightPinkyFingerBendSlider.val + rightRingFingerBendSlider.val + rightMidFingerBendSlider.val + rightIndexFingerBendSlider.val + rightFingersInOutSlider.val + rightThumbBendSlider.val + rightThumbFistSlider.val + rightThumbInOutSlider.val;
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
							
							
					}
				}
					//Create sliders that allow range +/- buttons
				    private Slider CreateSlider(string label, float val, float max, bool constrained, string format)
					{
						var uiElement = CreateUIElement(manager.configurableSliderPrefab.transform, true);
						var dynamicSlider = uiElement.GetComponent<UIDynamicSlider>();
						dynamicSlider.Configure(label, -max, max, val, constrained, format, true, !constrained);
						return dynamicSlider.slider;
					}

        protected void FixedUpdate()
        {
		if (uiFocusTarget.val != "none")
			pluginLabelJSON.val = uiFocusTarget.val;
			currentAtom = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
			if (currentAtom == null) return;
			distanceVariation();
			handControl();
		}


        public override void Init() {
            try
            {
			uiFocusTarget = new JSONStorableStringChooser("Target", SuperController.singleton.GetAtomUIDs(), "None", "Right hand target atom");
			RegisterStringChooser(uiFocusTarget);
			SyncAtomChocies();
			UIDynamicPopup udp = CreateScrollablePopup(uiFocusTarget, true);
			udp.popup.onOpenPopupHandlers += SyncAtomChocies;
			udp.popupPanelHeight = 1100f;
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
				
				defMinDistSlider = new JSONStorableFloat("Trigger distance", 0.15f, 0f, 0.5f, false, true);
				defMinDistSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(defMinDistSlider);
				CreateSlider(defMinDistSlider, false);				
				
				defDistSlider = new JSONStorableFloat("Trigger falloff", 0.25f, 0f, 0.5f, false, true);
				defDistSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(defDistSlider);
				CreateSlider(defDistSlider, false);		
				
				RegisterFloat(defDistSlider);
				CreateSlider(defDistSlider, false);
				
				UIDynamic spacer2 = CreateSpacer(false);
				spacer2.height = 70f;

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


                #endregion
            }

			catch (Exception e)
				{
					SuperController.LogError("Exception caught: " + e);
				}
			}

        void Start()
        {
			previousState = intensitySlider.val + rightPinkyFingerBendSlider.val + rightRingFingerBendSlider.val + rightMidFingerBendSlider.val + rightIndexFingerBendSlider.val + rightFingersInOutSlider.val + rightThumbBendSlider.val + rightThumbFistSlider.val + rightThumbInOutSlider.val;
			newState      = intensitySlider.val + rightPinkyFingerBendSlider.val + rightRingFingerBendSlider.val + rightMidFingerBendSlider.val + rightIndexFingerBendSlider.val + rightFingersInOutSlider.val + rightThumbBendSlider.val + rightThumbFistSlider.val + rightThumbInOutSlider.val;
            
		}
    }
}
