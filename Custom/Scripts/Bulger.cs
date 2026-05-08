using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace VRAdultFun {
	public class Bulger : MVRScript {
		protected JSONStorableStringChooser uiFocusTarget;
		protected JSONStorableFloat uiThroatMult;
		protected JSONStorableFloat uiBulgeMult;
		protected JSONStorableFloat uiBulgeUMult;
		protected JSONStorableFloat uiMinThroatDistMult;
		protected JSONStorableFloat uiMinBulgeDistMult;
		
        private static FreeControllerV3 headController;
        private static FreeControllerV3 pelvisController;
        private static DAZMorph morphDeepBulgeBellyBottom;
        private static DAZMorph morphDeepBulgeBellyMid;
        private static DAZMorph morphDeepThroat;
        private static Atom person;
        private static Atom person2;
		private static Atom currentAtom;
		private static string currentAtomName = "None";
        private static FreeControllerV3 playerPelvisController;
        private static FreeControllerV3 playerTipController;
        private static FreeControllerV3 playerTipBaseController;
		private static float tempFloat = 0.0f;
		private static bool isObject = false;

		public override void Init() {
			try {
			List<string> targetChoices = new List<string>();
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
				currentAtom = SuperController.singleton.GetAtomByUid(atomUID);
                if (currentAtom != containingAtom && atomUID != null)
                {
					targetChoices.Add(atomUID);
				}
			}
			uiFocusTarget = new JSONStorableStringChooser("Sex Giver", targetChoices, "None", "Choose Person");
			RegisterStringChooser(uiFocusTarget);
			UIDynamicPopup udp = CreatePopup(uiFocusTarget, true);
			uiThroatMult = new JSONStorableFloat("Deepthroat Multiplier", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiThroatMult);
			CreateSlider(uiThroatMult, false);
			
			uiBulgeUMult = new JSONStorableFloat("Belly Bulge Upper Multiplier", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiBulgeUMult);
			CreateSlider(uiBulgeUMult, false);

			uiBulgeMult = new JSONStorableFloat("Belly Bulge Lower Multiplier", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiBulgeMult);
			CreateSlider(uiBulgeMult, false);
			
			uiMinThroatDistMult = new JSONStorableFloat("Min Throat Dist Multiplier", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiMinThroatDistMult);
			CreateSlider(uiMinThroatDistMult, true);
			
			uiMinBulgeDistMult = new JSONStorableFloat("Min Bulge Dist Multiplier", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiMinBulgeDistMult);
			CreateSlider(uiMinBulgeDistMult, true);

			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		void Start() {
			try {
				person = containingAtom;//SuperController.singleton.GetAtomByUid("Person");
				if (person != null)
				{
					//SuperController.LogError("Person found");
					JSONStorable js = person.GetStorableByID("geometry");
					DAZCharacterSelector dcs = js as DAZCharacterSelector;
					GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;

					headController = person.GetStorableByID("headControl") as FreeControllerV3;
					pelvisController = person.GetStorableByID("hipControl") as FreeControllerV3;
					if (morphUI != null)
					{
						morphDeepBulgeBellyBottom = morphUI.GetMorphByDisplayName("deepbulge_bot");
						morphDeepBulgeBellyMid = morphUI.GetMorphByDisplayName("deepbulge_mid");
						morphDeepThroat = morphUI.GetMorphByDisplayName("deepthroat");
					}
					if (uiFocusTarget.val != "none")
					{
						person2 = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
						if (person2 != null)
						{
							playerPelvisController = person2.GetStorableByID("pelvisControl") as FreeControllerV3;
						}
					}
				}
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		void FixedUpdate() {
			try {
				currentAtomName = uiFocusTarget.val;
				if ((person2 == null || playerTipController == null) && currentAtomName != "None")
				{
					person2 = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
					if (person2 == null)
					{
						uiFocusTarget.val = "None";
						currentAtomName = "None";
					}
					else
					{
						if (person2.type == "Person")
						{
							isObject = false;
							playerTipController = person2.GetStorableByID("penisTipControl") as FreeControllerV3;
						}
						else
						{
							isObject = true;
							playerTipController = person2.GetStorableByID("control") as FreeControllerV3;
						}
					}
				}
				if (morphDeepBulgeBellyBottom != null && morphDeepBulgeBellyMid != null && person2 != null && playerTipController != null)
				{
					tempFloat = Vector3.Distance(pelvisController.followWhenOff.position, playerTipController.followWhenOff.position);
					if ( tempFloat < 0.15f * uiMinBulgeDistMult.val)
					{
						morphDeepBulgeBellyBottom.SetValue(Mathf.Clamp((1.0f - (tempFloat*(6.666f / uiMinBulgeDistMult.val))) * uiBulgeMult.val,0.0f,1.0f));
					}
					else
					{
						morphDeepBulgeBellyBottom.SetValue(0.0f);
					}
					if ( tempFloat < 0.065f * uiMinBulgeDistMult.val)
					{
						morphDeepBulgeBellyMid.SetValue(Mathf.Clamp((1.0f - (tempFloat*(15.384f / uiMinBulgeDistMult.val))) * uiBulgeUMult.val,0.0f,1.0f) * 0.75f);
					}
					else
					{
						morphDeepBulgeBellyMid.SetValue(0.0f);
					}
				}
				
				
				if (morphDeepThroat != null && person2 != null && playerTipController != null)
				{
					tempFloat = Vector3.Distance(headController.followWhenOff.position, playerTipController.followWhenOff.position);
					if ( tempFloat < 0.077f * uiMinThroatDistMult.val)
					{
						morphDeepThroat.SetValue(Mathf.Clamp((1.0f - (tempFloat*(12.987f/uiMinThroatDistMult.val))) * uiThroatMult.val,0.0f,1.0f));
					}
					else
					{
						morphDeepThroat.SetValue(0.0f);
					}
				}
			
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		void OnDestroy() {
		}

	}
}