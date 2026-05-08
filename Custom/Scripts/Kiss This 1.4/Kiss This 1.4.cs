//By Spacedsog, based on the Kiss plugin by Extraltodeusand modified by TimelordToby
//Attach this script to a person, select an atom in the menu for the person to kiss when the atom is near

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using System.Threading;
using System.Text.RegularExpressions;

namespace KissThis {
	public class B : MVRScript {
		protected JSONStorableStringChooser uiFocusTarget;
		private static Atom currentAtom;
		private static string currentAtomName = "None";
		private FreeControllerV3 head;
		private Rigidbody headRigidbody;
		protected JSONStorableFloat intensitySlider;
		protected JSONStorableFloat defMinDistSlider;
		protected JSONStorableFloat defDistSlider;
		protected JSONStorableFloat mouthNarrowSlider;
		protected JSONStorableFloat eyesClosedSlider;
		protected JSONStorableFloat lipsPuckerSlider;
		protected JSONStorableFloat mouthOpenSlider;
		protected JSONStorableFloat mouthOpenMinimumSlider;
		protected JSONStorableFloat tongueLengthSlider;
		protected JSONStorableFloat tongueBendTipSlider;
		protected JSONStorableFloat tongueNarrowWideSlider;
		protected JSONStorableFloat tongueThicknessSlider;
		protected JSONStorableFloat emotion1Slider;
		protected JSONStorableFloat emotion2Slider;
		protected JSONStorableFloat headDrive;
		protected JSONStorableBool triggerByDistance;
		protected JSONStorableBool blinkControl;
		protected float intensityPower;
		protected float previousState;
		protected float newState;

				protected void distanceVariation()
				{
					currentAtom = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
					if (currentAtom == null) return;
					Vector3 SelectedAtomPos = currentAtom.mainController.transform.position;
					headRigidbody = containingAtom.rigidbodies.First(rb => rb.name == "head");
					Vector3 headPos = headRigidbody.transform.position;
					float dist = Vector3.Distance(SelectedAtomPos, headPos) - defMinDistSlider.val;
					if (dist < defDistSlider.val)
					{
						if (blinkControl.val && containingAtom.GetStorableByID("EyelidControl").GetBoolJSONParam("blinkEnabled").val)
								containingAtom.GetStorableByID("EyelidControl").GetBoolJSONParam("blinkEnabled").val = false;

						intensitySlider.val = 1-(1/defDistSlider.val*dist);

						if (headDrive.val > 0)
						{
							float distHead = Vector3.Distance(SelectedAtomPos, headPos) - defMinDistSlider.val/2;
							head.jointRotationDriveXTarget = (1 - (1/defDistSlider.val*distHead))*headDrive.val;
						}
					}
					else
					{
						if (blinkControl.val && !containingAtom.GetStorableByID("EyelidControl").GetBoolJSONParam("blinkEnabled").val)
							containingAtom.GetStorableByID("EyelidControl").GetBoolJSONParam("blinkEnabled").val = true;

						if (intensitySlider.val != 0)
							intensitySlider.val = 0;
					}
				}
				
				protected void kissControl()
				{
					newState = intensitySlider.val + mouthNarrowSlider.val + eyesClosedSlider.val + lipsPuckerSlider.val + mouthOpenSlider.val + mouthOpenMinimumSlider.val + tongueLengthSlider.val + tongueBendTipSlider.val + tongueNarrowWideSlider.val + tongueThicknessSlider.val + emotion1Slider.val+ emotion2Slider.val;
					if (newState != previousState)
					{
							previousState = newState;
							JSONStorable geometry = containingAtom.GetStorableByID("geometry");
							DAZCharacterSelector character = geometry as DAZCharacterSelector;
							GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

							DAZMorph mouthNarrow = morphControl.GetMorphByDisplayName("Mouth Narrow");
							DAZMorph eyesClosed  = morphControl.GetMorphByDisplayName("Eyes Closed");
							DAZMorph lipsPucker  = morphControl.GetMorphByDisplayName("Lips Pucker");
							DAZMorph mouthOpen   = morphControl.GetMorphByDisplayName("Mouth Open");
							DAZMorph mouthOpenMinimum   = morphControl.GetMorphByDisplayName("Mouth Open");
							DAZMorph tongueLength   = morphControl.GetMorphByDisplayName("Tongue Length");
							DAZMorph tongueBendTip   = morphControl.GetMorphByDisplayName("Tongue Bend Tip");
							DAZMorph tongueNarrowWide   = morphControl.GetMorphByDisplayName("Tongue Narrow-Wide");
							DAZMorph tongueThickness   = morphControl.GetMorphByDisplayName("Tongue Thickness");
							DAZMorph emotion1   = morphControl.GetMorphByDisplayName("Afraid");
							DAZMorph emotion2   = morphControl.GetMorphByDisplayName("Smile Full Face");

							if (mouthNarrowSlider.val > 0)
								mouthNarrow.morphValue = mouthNarrowSlider.val * intensitySlider.val;
							if (eyesClosedSlider.val > 0)
								eyesClosed.morphValue  = eyesClosedSlider.val  * intensitySlider.val;
							if (lipsPuckerSlider.val > 0)
								lipsPucker.morphValue  = lipsPuckerSlider.val  * intensitySlider.val;
							if (mouthOpenSlider.val > 0)
								mouthOpen.morphValue   = mouthOpenSlider.val   * intensitySlider.val;
							if (mouthOpenMinimumSlider.val > 0 && mouthOpen.morphValue < mouthOpenMinimumSlider.val)
								mouthOpen.morphValue   = mouthOpenMinimumSlider.val;
							if (tongueLengthSlider.val > 0)
								tongueLength.morphValue   = tongueLengthSlider.val   * intensitySlider.val;
							if (tongueBendTipSlider.val != 0)
								tongueBendTip.morphValue   = tongueBendTipSlider.val   * intensitySlider.val;
							if (tongueNarrowWideSlider.val != 0)
								tongueNarrowWide.morphValue   = tongueNarrowWideSlider.val   * intensitySlider.val;
							if (tongueThicknessSlider.val != 0)
								tongueThickness.morphValue   = tongueThicknessSlider.val   * intensitySlider.val;
							if (emotion1Slider.val > 0)
								emotion1.morphValue   = emotion2Slider.val   * intensitySlider.val;
							if (emotion2Slider.val > 0)
								emotion2.morphValue   = emotion2Slider.val   * intensitySlider.val;
					}
				}

        protected void FixedUpdate()
        {

						head = containingAtom.GetStorableByID("headControl") as FreeControllerV3;
						if (triggerByDistance.val && uiFocusTarget.val != "none")

							distanceVariation();
						kissControl();

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
            spacer.height = 155f;

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

				triggerByDistance = new JSONStorableBool("Trigger by camera distance", true);
				RegisterBool(triggerByDistance);
				CreateToggle((triggerByDistance), false);

				blinkControl = new JSONStorableBool("Pause blink when kiss", true);
				RegisterBool(blinkControl);
				CreateToggle((blinkControl), false);
				
				UIDynamic spacer2 = CreateSpacer(false);
				spacer2.height = 5f;

				headDrive = new JSONStorableFloat("Head joint drive X", 10f, -35f, 150f, true);
				headDrive.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(headDrive);
				CreateSlider(headDrive, false);

				lipsPuckerSlider = new JSONStorableFloat("Lips pucker variation", 0.4f, 0f, 1f, true);
				lipsPuckerSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(lipsPuckerSlider);
				CreateSlider(lipsPuckerSlider, false);

				mouthNarrowSlider = new JSONStorableFloat("mouth narrow variation", 0.1f, 0f, 1f, true);
				mouthNarrowSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(mouthNarrowSlider);
				CreateSlider(mouthNarrowSlider, false);

				mouthOpenSlider = new JSONStorableFloat("mouth open variation", 0.5f, 0f, 2f, true);
				mouthOpenSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(mouthOpenSlider);
				CreateSlider(mouthOpenSlider, false);
				
				mouthOpenMinimumSlider = new JSONStorableFloat("mouth open minimum", 0f, 0f, 2f, true);
				mouthOpenMinimumSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(mouthOpenMinimumSlider);
				CreateSlider(mouthOpenMinimumSlider, true);


				eyesClosedSlider = new JSONStorableFloat("Eyes closed variation", 1f, 0f, 1f, true);
				eyesClosedSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(eyesClosedSlider);
				CreateSlider(eyesClosedSlider, false);
				
				tongueLengthSlider = new JSONStorableFloat("Tongue length variation", 0.1f, 0f, 1f, true);
				tongueLengthSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(tongueLengthSlider);
				CreateSlider(tongueLengthSlider, true);
				
				tongueBendTipSlider = new JSONStorableFloat("Tongue bend variation", 0.0f, -1f, 1f, true);
				tongueBendTipSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(tongueBendTipSlider);
				CreateSlider(tongueBendTipSlider, true);
				
				tongueNarrowWideSlider = new JSONStorableFloat("Tongue narrow-wide variation", 0f, -0.5f, 1f, true);
				tongueNarrowWideSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(tongueNarrowWideSlider);
				CreateSlider(tongueNarrowWideSlider, true);
				
				tongueThicknessSlider = new JSONStorableFloat("Tongue thickness variation", 0.0f, -1f, 1f, true);
				tongueThicknessSlider.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(tongueThicknessSlider);
				CreateSlider(tongueThicknessSlider, true);
				
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
			previousState = intensitySlider.val + mouthNarrowSlider.val + eyesClosedSlider.val + lipsPuckerSlider.val + mouthOpenSlider.val + mouthOpenMinimumSlider.val + tongueLengthSlider.val + tongueBendTipSlider.val + tongueNarrowWideSlider.val + tongueThicknessSlider.val + emotion1Slider.val + emotion2Slider.val;
			newState      = intensitySlider.val + mouthNarrowSlider.val + eyesClosedSlider.val + lipsPuckerSlider.val + mouthOpenSlider.val + mouthOpenMinimumSlider.val + tongueLengthSlider.val + tongueBendTipSlider.val + tongueNarrowWideSlider.val + tongueThicknessSlider.val + emotion1Slider.val + emotion2Slider.val;

		}
    }
}
