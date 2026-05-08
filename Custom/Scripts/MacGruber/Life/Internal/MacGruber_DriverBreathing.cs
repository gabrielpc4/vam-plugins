using UnityEngine;
using System;
using static MacGruber.Breathing;
using static MacGruber.Utils;

namespace MacGruber
{
	public class DriverBreathing : MVRScript
	{
		private Breathing myBreathing;
		
		private JSONStorableFloat myStomachMin;
		private JSONStorableFloat myStomachMax;
		private JSONStorableFloat myChestJointMin;
		private JSONStorableFloat myChestJointMax;
		private JSONStorableFloat myChestMorphMin;
		private JSONStorableFloat myChestMorphMax;
		private JSONStorableFloat myChestSpring;
		private JSONStorableFloat myMouthOpenMin;
		private JSONStorableFloat myMouthOpenMax;
		private JSONStorableFloat myMouthOpenTime;
		private JSONStorableFloat myMouthCloseTime;
		private JSONStorableFloat myLipsMax;
		private JSONStorableFloat myNoseInMax;
		private JSONStorableFloat myNoseOutMax;
		private JSONStorableFloat myDebug1;
		private JSONStorableFloat myDebug2;
		private JSONStorableFloat myDebug3;	
				
		private DAZMorph myChestMorph;
		private DAZMorph myStomachMorph;
		private DAZMorph myMouthMorph;
		private DAZMorph myLipsMorph;
		private DAZMorph myNoseInMorph;
		private DAZMorph myNoseOutMorph;
		private FreeControllerV3 myChestControl;
		private bool myAnimationValid = false;
		private float myMouthOpen = 0.0f;
		private float myMouthValue = 0.0f;
		private float myMouthVelocity = 0.0f;
		private float myLungDepthPower = 0.0f;
		
		private BlendValue myLungValue;
		private BlendValue myBreathInValue;
		private BlendValue myBreathOutValue;
		private BlendValue myBreathMouthIOValue;
		private BlendValue myBreathMouthOIValue;		
		private BlendValue myDebugValue1;
		private BlendValue myDebugValue2;
		private BlendValue myDebugValue3;
		
		public override void Init()
		{
			myBreathing = FindWithinSamePlugin<Breathing>(this);
			InitUI();
			InitAnimation();
			InitEvents();
		}
		
		private void InitUI()
		{
			Utils.SetupInfoText(this, 
				"<color=#606060><size=40><b>DriverBreathing</b></size>\nThis animation driver controls chest, stomach, nose and mouth animation based on breathing.</color>\n\n" + 	
				"<b>ChestMorph Min/Max:</b>\nMinimum/Maximum values for chest morph.\n\n" +
				"<b>ChestJointDrive Min/Max:</b>\nMinimum/Maximum values for 'Joint Drive X Angle' of chest control.\n\n" + 
				"<b>ChestJointDrive Spring:</b>\nValue for 'Joint Drive Spring' of chest control.\n\n" +
				"<b>Stomach Min/Max:</b>\nMinimum/Maximum values for stomach morph.\n\n" +
				"<b>MouthMorph Min/Max:</b>\nMinimum/Maximum values for MouthOpen morph.\n\n" +
				"<b>Mouth Open/Close Time:</b>\nTime it takes for the mouth to open/close.\n\n" +
				"<b>LipsMorph Max:</b>\nMaximum value for lips morph when the mouth is fully open.\n\n" +
				"<b>NoseInMorph Max:</b>\nMaximum value for nose morph when breathing in.\n\n" +
				"<b>NoseOutMorph Max:</b>\nMaximum value for nose morph when breathing out.\n\n",
				1200.0f, true
			);
			
			myChestMorphMin = SetupSliderFloat(this, "ChestMorph Min", -0.1f, -1.0f, 1.0f, false);
			myChestMorphMax = SetupSliderFloat(this, "ChestMorph Max",  0.5f, -1.0f, 1.0f, false);
			myChestJointMin = SetupSliderFloat(this, "ChestJointDrive Min", -2.0f, -20.0f, 20.0f, false);
			myChestJointMax = SetupSliderFloat(this, "ChestJointDrive Max", 3.0f, -20.0f, 20.0f, false);
			myChestSpring = SetupSliderFloat(this, "ChestJointDrive Spring", 160.0f, 0.0f, 250.0f, false);
			myStomachMin = SetupSliderFloat(this, "StomachMin", -0.1f, -1.0f, 1.0f, false);
			myStomachMax = SetupSliderFloat(this, "StomachMax",  0.4f, -1.0f, 1.0f, false);
			myMouthOpenMin = SetupSliderFloat(this, "MouthMorph Min", 0.0f, -0.5f, 2.0f, false);
			myMouthOpenMax = SetupSliderFloat(this, "MouthMorph Max", 1.0f, -0.5f, 2.0f, false);
			myMouthOpenTime = SetupSliderFloat(this, "Mouth Open Time", 0.05f, 0.0f, 1.0f, false);
			myMouthCloseTime = SetupSliderFloat(this, "Mouth Close Time", 0.2f, 0.0f, 1.0f, false);
			myLipsMax = SetupSliderFloat(this, "LipsMorph Max", 1.0f, -1.0f, 1.0f, false);
			myNoseInMax = SetupSliderFloat(this, "NoseInMorph Max", 0.6f, 0.0f, 1.0f, false);
			myNoseOutMax = SetupSliderFloat(this, "NoseOutMorph Max", 0.25f, 0.0f, 1.0f, false);
			
			/*myDebug1 = SetupSliderFloat(this, "Debug1", 0.0f, 0.0f, 1.0f, true);
			myDebug2 = SetupSliderFloat(this, "Debug2", 0.0f, 0.0f, 1.0f, true);
			myDebug3 = SetupSliderFloat(this, "Debug3", 0.0f, 0.0f, 1.0f, true);*/
        }
		
		private void InitAnimation()
		{			
			DAZCharacterSelector geometry = containingAtom.GetStorableByID("geometry") as DAZCharacterSelector;
			GenerateDAZMorphsControlUI morphControl = geometry?.morphsControlUI;
			if (morphControl != null)
			{				
				myChestMorph = morphControl.GetMorphByDisplayName("Breathing Chest");
				myStomachMorph = morphControl.GetMorphByDisplayName("Breathing Stomach");				
				myMouthMorph = morphControl.GetMorphByDisplayName("Mouth Open");
				myLipsMorph = morphControl.GetMorphByDisplayName("Breathing Lips");
				myNoseInMorph  = morphControl.GetMorphByDisplayName("Breathing NoseIn");
				myNoseOutMorph = morphControl.GetMorphByDisplayName("Breathing NoseOut");
			}					
			myChestControl = containingAtom.GetStorableByID("chestControl") as FreeControllerV3;			
			
			myAnimationValid =
				myChestMorph     != null &&
				myStomachMorph   != null &&
				myMouthMorph     != null &&
				myLipsMorph      != null &&
				myNoseInMorph    != null &&
				myNoseOutMorph   != null &&
				myChestControl   != null;
		}
		
		private void InitEvents()
		{		
			{
				BlendEvent value1 = myBreathing.RegisterBlendEvent(
					SM(MRK_BreathIn),
					SM(MRK_HoldIn),
					SM(MRK_BreathEnd)
				);
				BlendEvent value2 = myBreathing.RegisterBlendEvent(
					SM(MRK_BreathOut),
					SM(MRK_HoldOut),
					SM(MRK_BreathEnd)
				);
				
				myLungValue = new BlendCombineLinearInOut(value1, value2);
				
				myDebugValue1 = value1;
				myDebugValue2 = value2;
				myDebugValue3 = myLungValue;
			}
			
			{
				BlendEvent value1 = myBreathing.RegisterBlendEvent(
					SM(MRK_BreathIn, -0.12f),
					RM(MRK_BreathIn, MRK_HoldIn, 0.70f, 0.00f, 1.00f),
					SM(MRK_BreathEnd)
				);
				BlendEvent value2 = myBreathing.RegisterBlendEvent(					
					RM(MRK_HoldIn, MRK_BreathIn, 0.30f, -0.40f, -0.05f),
					SM(MRK_HoldIn,  0.1f),
					SM(MRK_BreathEnd)
				);
				myBreathInValue = new BlendCombineLinearInOut(value1, value2);
			}
			
			{
				BlendEvent value1 = myBreathing.RegisterBlendEvent(
					SM(MRK_BreathOut, -0.12f),
					RM(MRK_BreathOut, MRK_HoldOut, 0.30f, 0.0f, 0.2f),
					SM(MRK_BreathEnd)
				);
				BlendEvent value2 = myBreathing.RegisterBlendEvent(					
					RM(MRK_HoldOut, MRK_BreathOut, 0.70f, -2.0f, 0.0f),
					SM(MRK_HoldOut,  0.05f),
					SM(MRK_BreathEnd)
				);
				myBreathOutValue = new BlendCombineLinearInOut(value1, value2);
			}
			
			{
				BlendEvent value1 = myBreathing.RegisterBlendEvent(
					SM(MRK_BreathIn, 0.0f),
					RM(MRK_BreathIn, MRK_HoldIn, 0.50f, 0.0f, 0.3f),
					SM(MRK_BreathEnd)
				);
				BlendEvent value2 = myBreathing.RegisterBlendEvent(					
					RM(MRK_HoldOut, MRK_BreathOut, 0.70f, -1.0f, 0.0f),
					SM(MRK_HoldOut,  0.0f),
					SM(MRK_BreathEnd)
				);
				myBreathMouthIOValue = new BlendCombineLinearInOut(value1, value2);
			}
			
			{
				BlendEvent value1 = myBreathing.RegisterBlendEvent(
					SM(MRK_BreathOut, 0.0f),
					RM(MRK_BreathOut, MRK_HoldOut, 0.30f, 0.0f, 0.1f),
					SM(MRK_BreathEnd)
				);
				BlendEvent value2 = myBreathing.RegisterBlendEvent(
					RM(MRK_HoldIn, MRK_BreathIn, 0.30f, -0.1f, 0.0f),
					SM(MRK_HoldIn, 0.0f),
					SM(MRK_BreathEnd)
				);
				myBreathMouthOIValue = new BlendCombineLinearInOut(value1, value2);
			}
		}
		
		private void Update()
		{
			if (myDebug1 != null && myDebugValue1 != null)
				myDebug1.val = myDebugValue1.Value;
			if (myDebug2 != null && myDebugValue2 != null)
				myDebug2.val = myDebugValue2.Value;
			if (myDebug3 != null && myDebugValue3 != null)
				myDebug3.val = myDebugValue3.Value;
						
			Breath breath = myBreathing.Get(CURR);
			if (!breath.IsValid() || !myAnimationValid)
				return;			
		
			{
				float lungValue = myLungValue.Value;
				if (lungValue <= 0.0001f)
					myLungDepthPower = breath.Depth*breath.Power;				
				lungValue = Mathf.SmoothStep(0.0f, myLungDepthPower, myLungValue.Value);
				
				// correct a bit for the chest and stomach morphs being essentially linear scale, moving faster near max range
				float lungValueCorrected = Mathf.Pow(lungValue, 0.9f); 
						
				// Chest morphs	
				myChestMorph.morphValue = Mathf.Lerp(myChestMorphMin.val, myChestMorphMax.val, lungValueCorrected);
				
				// Chest joint rotation
				float chestJoint = Mathf.Lerp(myChestJointMin.val, myChestJointMax.val, lungValue);
                myChestControl.jointRotationDriveXTarget = chestJoint;
				myChestControl.jointRotationDriveDamper = 1.0f;
				myChestControl.jointRotationDriveSpring = myChestSpring.val;

				// Stomach morph
				myStomachMorph.morphValue = Mathf.Lerp(myStomachMin.val, myStomachMax.val, lungValueCorrected);			
            }
			
			{
				float mouth = 0.0f;
				float nose = 0.0f;
				bool hasNoseIn = breath.Entry.HasTag(TAG_NOSEIN);
				bool hasNoseOut = breath.Entry.HasTag(TAG_NOSEOUT);
				if (hasNoseIn && hasNoseOut)
				{
					nose = myBreathInValue.Value - myBreathOutValue.Value;
				}
				else if (hasNoseIn)
				{
					nose = myBreathInValue.Value;
					mouth = myBreathOutValue.Value;
				}
				else if (hasNoseOut)
				{
					nose = -myBreathOutValue.Value;
					mouth = myBreathInValue.Value;
				}
				else
				{
					mouth = (breath.Entry.Format == FORMAT_INOUT) ? myBreathMouthIOValue.Value : myBreathMouthOIValue.Value;
				}
				
				// Nose morphs				
				float noseScale = breath.Depth * breath.Power;
				if (nose >= 0.0f)
				{
					myNoseInMorph.morphValue = noseScale*myNoseInMax.val*nose;
					myNoseOutMorph.morphValue = 0.0f;
				}
				else
				{
					myNoseInMorph.morphValue = 0.0f;
					myNoseOutMorph.morphValue = -noseScale*myNoseOutMax.val*nose;
				}
				
				// Mouth & Lips morphs
				float direction = (hasNoseIn || hasNoseOut) ? -1.0f : 1.0f;
				myMouthOpen = Mathf.Clamp01(myMouthOpen + direction * Time.deltaTime);
				mouth = Mathf.Lerp(mouth, mouth*0.8f+0.2f, myMouthOpen);
				float mouthSmoothTime = mouth < myMouthValue ? myMouthCloseTime.val : myMouthOpenTime.val;
				mouth = Mathf.SmoothDamp(myMouthValue, mouth, ref myMouthVelocity, mouthSmoothTime);
				myMouthValue = mouth;				
				float mouthScale = (breath.Intensity*0.5f+0.5f) * breath.Power;
				myMouthMorph.morphValue = Mathf.SmoothStep(myMouthOpenMin.val, mouthScale * myMouthOpenMax.val, mouth);
				myLipsMorph.morphValue = Mathf.SmoothStep(0.0f, myLipsMax.val, mouth);
			}
		}	
	}	
}