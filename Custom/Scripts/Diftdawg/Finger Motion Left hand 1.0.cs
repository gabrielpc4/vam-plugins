//By Spacedsog, based on the Kiss plugin by Extraltodeusand modified by TimelordToby
//Attach this script to a person, select an atom in the menu for the person to bend their fingers when the atom is near

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace FingerMotionLeftHand {
	public class FingerMotionLeftHand : MVRScript {
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
					handRigidbody = containingAtom.rigidbodies.First(rb => rb.name == "lHand");
					Vector3 handPos = handRigidbody.transform.position;
					float dist = Vector3.Distance(SelectedAtomPos, handPos) - defMinDistSlider.val;
					if (dist < defDistSlider.val ||  intensitySlider.val > 0.01f)
					{
						intensitySlider.val = 1-(1/defDistSlider.val*dist);
						
					}
				}
				
				protected void handControl()
				{
					newState = intensitySlider.val + leftPinkyFingerBendSlider.val + leftRingFingerBendSlider.val + leftMidFingerBendSlider.val + leftIndexFingerBendSlider.val + leftFingersInOutSlider.val + leftThumbBendSlider.val + leftThumbFistSlider.val + leftThumbInOutSlider.val;
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
			uiFocusTarget = new JSONStorableStringChooser("Target", SuperController.singleton.GetAtomUIDs(), "None", "Left hand target atom");
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
				
				UIDynamic spacer2 = CreateSpacer(false);
				spacer2.height = 70f;

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


                #endregion
            }

			catch (Exception e)
				{
					SuperController.LogError("Exception caught: " + e);
				}
			}

        void Start()
        {
			previousState = intensitySlider.val + leftPinkyFingerBendSlider.val + leftRingFingerBendSlider.val + leftMidFingerBendSlider.val + leftIndexFingerBendSlider.val + leftFingersInOutSlider.val + leftThumbBendSlider.val +leftThumbFistSlider.val + leftThumbInOutSlider.val;
			newState      = intensitySlider.val + leftPinkyFingerBendSlider.val + leftRingFingerBendSlider.val + leftMidFingerBendSlider.val + leftIndexFingerBendSlider.val + leftFingersInOutSlider.val + leftThumbBendSlider.val + leftThumbFistSlider.val + leftThumbInOutSlider.val;

		}
    }
}
