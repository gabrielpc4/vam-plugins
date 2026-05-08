// 07/09/2018 MacGruber Original: VaM ScriptEngine plugin created.
// 25/10/2018 VeeRifter Mod V1.0: McGruber code ported to new style standalone VaM plugin with eight user adjustable settings.
// 29/11/2018 VeeRifter Mod V2.0: Controls for horizontal and vertical gaze offsets, plus selectabe gaze target option added.
// Namespace change & public function calls added for compatability with Dollmaster (all credit to VAMDeluxe)
// 30/11/2018 VeeRifter Mod V2.1: Minor bugfix release.
// 21/12/2018 VeeRifter Mod V3.0: Fixed a bug whereby the gaze target selection was not being restored when a saved .json was reloaded.
// 05/02/2019 Physis Mod V1.0 Added activation range so the gaze can ignore eye saccades caused by other plugins
// Incorporated the two range tweaks to focus horizontal and vertical sliders, as per MaxRupert Modelers variant of my Mod V1.0 script.
// Added motion to the chest and pelvis joints to swivel naturally about the waist when the head turns.
// This action is enabled by default but can be turned off in the plugin settings.
// When enabled, the head horizontal rotation limit is increased from 90 to 140 degrees.
// 28/07/19 Spacedog Mod V1.0: Adjusted defaults, increased vertical look range, and set referance node to abdomen2Control to avoid unexpected head movement with hip rotated.
// Disabled head roll to fix incorrect head movement when root node is rotated.
// 19/02/20 Spacedog Mod V2.0: Adapted VRAdultFun's EmotionEngine code to eliminate gimball lock and any infulence of person root rotation. Reintroduced head roll and added offset adjustment.
// Added ability to disable randomisation via button and set look range via sliders. Dropdown for keeping head aligned up, down or with the body. New labels to help make functions more clear. Many bug fixes.

using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace EasyGaze
{
    public class EasyGaze : MVRScript
    {
	private static FreeControllerV3 head;
	private static FreeControllerV3 chestControl;
	private static FreeControllerV3 pelvisControl;
	private List<string> headAlignmentChoices = new List<string>() { "Body Aligned", "Up", "Down" };
	private const int BodyAligned       = 0;
	private const int Up       			= 1;
	private const int Down       		= 2;
	public int index;
	public JSONStorableStringChooser headAlignmentchooser;
	public int headAlignmentChoice = BodyAligned;	

        public override void Init() {
        try {
			if (containingAtom.type != "Person")
				{
					SuperController.LogError($"This plugin is for use with 'Person' atom only, not '{containingAtom.type}'");
					return;
				}
				pluginLabelJSON.val = "Easy Gaze";
				person = containingAtom;

				eyes = person.GetStorableByID("Eyes");
				if (eyes != null) {
					lookMode = eyes.GetStringChooserJSONParam("lookMode");
					if (lookMode == null) {
						SuperController.LogError("Could not find lookMode param on eyeTargetControl");
					}
				}
                personEyeTargetControl = person.GetStorableByID("eyeTargetControl") as FreeControllerV3;
                personEyeTarget = personEyeTargetControl.transform;
                playerTarget = CameraTarget.centerTarget.transform;
				
				//Initialise gazeTarget from current lookMode value
				if(lookMode.val == "Player") {
				gazeTarget = playerTarget;					
				} else {
					gazeTarget = personEyeTarget;
				}

				lookAtPosition = gazeTarget.TransformPoint(gazeOffset);
				
				chestControl = person.GetStorableByID( "chestControl") as FreeControllerV3;	
				pelvisControl = person.GetStorableByID( "pelvisControl") as FreeControllerV3;
				head = person.GetStorableByID( "headControl") as FreeControllerV3;	

                activationDistance = new JSONStorableFloat("Angle offset before turning head", 0.1f, 0f, 10f, true, true);
				RegisterFloat(activationDistance);
				CreateSlider(activationDistance, false);

                headTurnrangeH = new JSONStorableFloat("Horizontal head turn range", 180f, 0f, 180f, true, true);
				RegisterFloat(headTurnrangeH);
				CreateSlider(headTurnrangeH);

                headTurnrangeV = new JSONStorableFloat("Vertical head turn range", 180f, 0f, 180f, true, true);
				RegisterFloat(headTurnrangeV);
				CreateSlider(headTurnrangeV);				
				
                gazeDuration = new JSONStorableFloat("Duration of head turn", 0.7f, 0f, 5f, true, true);
				RegisterFloat(gazeDuration);
				CreateSlider(gazeDuration, false);			
				
				focusChangeDurationMin = new JSONStorableFloat("Random head turn timer Minimum", 1f, dmin => focusChangeDurationMax.SetVal(Mathf.Max(focusChangeDurationMax.val, dmin)), 1f, 10f, true,true);
				RegisterFloat(focusChangeDurationMin);
				CreateSlider(focusChangeDurationMin, true);

				focusChangeDurationMax = new JSONStorableFloat("Random head turn timer Maximum", 4f, dmax => focusChangeDurationMin.SetVal(Mathf.Min(focusChangeDurationMin.val, dmax)), 1f, 10f, true, true);
				RegisterFloat(focusChangeDurationMax);
				CreateSlider(focusChangeDurationMax, true);	

				focusAngleH = new JSONStorableFloat("Random head turn range Horizontal", 4f, 0f, 40f, true, true);
				RegisterFloat(focusAngleH);
				CreateSlider(focusAngleH, false);	
				
				focusAngleV = new JSONStorableFloat("Random head turn range Vertical", 4f, 0f, 40f, true, true);
				RegisterFloat(focusAngleV);
				CreateSlider(focusAngleV, false);				
				
				rollChangeDurationMin = new JSONStorableFloat("Random head roll timer Minimum", 2f, rmin => rollChangeDurationMax.SetVal(Mathf.Max(rollChangeDurationMax.val, rmin)), 1f, 10f, true, true);
				RegisterFloat(rollChangeDurationMin);
				CreateSlider(rollChangeDurationMin, true);

				rollChangeDurationMax = new JSONStorableFloat("Random head roll timer Maximum", 6f, rmax => rollChangeDurationMin.SetVal(Mathf.Min(rollChangeDurationMin.val, rmax)), 1f, 10f, true, true);
				RegisterFloat(rollChangeDurationMax);
				CreateSlider(rollChangeDurationMax, true);		

				rollAngleMax = new JSONStorableFloat("Random roll Angle Maximum", 2f, 0f, 40f, true, true);
				RegisterFloat(rollAngleMax);
				CreateSlider(rollAngleMax, true);
				
				offsetV = new JSONStorableFloat("Vertical Offset Angle", -5.0f, -90.0f, 90.0f, true, true);
				RegisterFloat(offsetV);
				CreateSlider(offsetV, false);				

				offsetH = new JSONStorableFloat("Horizontal Offset Angle", 0.0f, -90.0f, 90.0f, true, true);
				RegisterFloat(offsetH);
				CreateSlider(offsetH, false);
				
				offsetR = new JSONStorableFloat("Roll Offset Angle", 0.0f, -rollOffsetMax, rollOffsetMax, true, true);
				RegisterFloat(offsetR);
				CreateSlider(offsetR, true);
				
				headAlignmentchooser = new JSONStorableStringChooser("align head", headAlignmentChoices, headAlignmentChoices[headAlignmentChoice], "Align head");
				headAlignmentchooser.setCallbackFunction += (string modeName) => {
				headAlignmentChoice = headAlignmentChoices.FindIndex((string entry) => { return entry == modeName; });
				};
				CreatePopup(headAlignmentchooser, true);
				RegisterStringChooser(headAlignmentchooser);
				
				List<string> choices = new List<string>();
				choices.Add("Player");
				choices.Add("Target");
				JSONStorableStringChooser chooser = new JSONStorableStringChooser("Gaze Target Choice", choices, lookMode.val, "Gaze Follows", setGazeTarget);
				UIDynamicPopup udp = CreatePopup(chooser, true);

                swivelEnabled = new JSONStorableBool("Enable Body Swivel Action", false);
                swivelEnabled.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(swivelEnabled);
                CreateToggle(swivelEnabled, true);	
				
                disableRandom = new JSONStorableBool("Disable random movement", false);
                disableRandom.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(disableRandom);
                CreateToggle(disableRandom, true);
			}
			catch (System.Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}
        protected Vector3 lookAtPosition;

        protected void FixedUpdate()
        {
            if (gazeTarget == null || head == null)
                return;
			if (disableRandom.val)
			{
				disableRnd = 0;				
			}
			else
			{
				disableRnd = 1;				
			}
					
            // compute horizontal and vertical angles
			gazeOffset = offsetV.val * Mathf.Deg2Rad * Vector3.up + offsetH.val * Mathf.Deg2Rad * Vector3.left;

            // physis mod: only update lookAtPosition if it has changed significantly (so gaze doesn't follow eye sacades)
            if (Vector3.Distance(lookAtPosition, gazeTarget.TransformPoint(gazeOffset)) > activationDistance.val)
            {
                lookAtPosition = gazeTarget.TransformPoint(gazeOffset);
            }
			
			Transform reference = chestControl.followWhenOff;
			Transform headTransform = head.transform;
			Vector3 actualDir = reference.InverseTransformDirection(headTransform.forward);			
            Vector3 targetDir = lookAtPosition - head.transform.position;
            targetDir.Normalize();
            targetDir = reference.InverseTransformDirection(targetDir);
            Vector2 actualDirH = new Vector2(actualDir.x, actualDir.z);
            Vector2 targetDirH = new Vector2(targetDir.x, targetDir.z);
            Vector2 actualDirV = new Vector2(actualDirH.magnitude, actualDir.y);
            Vector2 targetDirV = new Vector2(targetDirH.magnitude, targetDir.y);
            actualDirH.Normalize();
            targetDirH.Normalize();
            actualDirV.Normalize();
            targetDirV.Normalize();
            float actualH = Mathf.Atan2(actualDirH.x, actualDirH.y);
            float targetH = Mathf.Atan2(targetDirH.x, targetDirH.y);
            float actualV = Mathf.Atan2(actualDirV.y, actualDirV.x);
            float targetV = Mathf.Atan2(targetDirV.y, targetDirV.x);

            // apply focus
            focusChangeClock += focusChangeSpeed * Time.fixedDeltaTime;
            if (focusChangeClock >= 1.0f)
            {
                focusChangeSpeed = 1.0f / Random.Range(focusChangeDurationMin.val, focusChangeDurationMax.val);
                focusChangeClock = 0.0f;
                focusPrev = focusNext;
                focusNext = Random.insideUnitCircle;
            }
            float t = Mathf.SmoothStep(0.0f, 1.0f, focusChangeClock);
            targetH += Mathf.Lerp(focusPrev.x, focusNext.x, t) * focusAngleH.val * Mathf.Deg2Rad * disableRnd;
            targetV += Mathf.Lerp(focusPrev.y, focusNext.y, t) * focusAngleV.val * Mathf.Deg2Rad * disableRnd;

            // adjust angles
            targetH = Mathf.Clamp(targetH, -headTurnrangeH.val * Mathf.Deg2Rad, headTurnrangeH.val * Mathf.Deg2Rad);
            targetV = Mathf.Clamp(targetV, -headTurnrangeV.val * Mathf.Deg2Rad, headTurnrangeV.val * Mathf.Deg2Rad);
            actualH = Mathf.SmoothDamp(actualH, targetH, ref velocityH, gazeDuration.val, Mathf.Infinity, Time.fixedDeltaTime);
            actualV = Mathf.SmoothDamp(actualV, targetV, ref velocityV, gazeDuration.val, Mathf.Infinity, Time.fixedDeltaTime);

            // recombine
            actualDir = RecombineDirection(actualH, actualV);
            targetDir = RecombineDirection(targetH, targetV);
            actualDir = reference.TransformDirection(actualDir);
			
			if (headAlignmentChoices[headAlignmentChoice] == "Body Aligned")
			{
				head.transform.LookAt(head.transform.position + actualDir, head.followWhenOff.position - chestControl.followWhenOff.position);
			}
			if (headAlignmentChoices[headAlignmentChoice] == "Up")
			{	
				head.transform.LookAt(head.transform.position + actualDir, Vector3.up);
			}
			if (headAlignmentChoices[headAlignmentChoice] == "Down")
			{
				head.transform.LookAt(head.transform.position + actualDir, Vector3.down);
			}

            // apply roll

			if (offsetR.val != offsetPrev)
			{
				rollOffsetHasChanged = true;
			}
			else
			{
				rollOffsetHasChanged = false;
			}
			offsetPrev = offsetR.val;
			rollChangeClock += rollChangeSpeed * Time.fixedDeltaTime;				
			if (rollChangeClock >= 1.0f || rollOffsetHasChanged)
			{
				rollPrev = rollNext;
				rollChangeClock = 0.0f;					
				rollChangeSpeed = 1.0f / Random.Range(rollChangeDurationMin.val, rollChangeDurationMax.val);
				float rollOffsetSlider = offsetR.val * Mathf.Deg2Rad;
				if (!rollOffsetHasChanged)
				{
					float rollAngleRangeSlider = rollAngleMax.val * Mathf.Deg2Rad * disableRnd;
					float rollAngleLimit = rollOffsetMax * Mathf.Deg2Rad;
					float rollNextMinus =  Mathf.Clamp(-rollAngleRangeSlider + rollOffsetSlider, -rollAngleLimit, rollAngleLimit);
					float rollNextPlus = Mathf.Clamp(rollAngleRangeSlider + rollOffsetSlider, -rollAngleLimit, rollAngleLimit);
					rollNext = Random.Range(rollNextMinus, rollNextPlus);
				}
				else
				{
					rollNext = rollOffsetSlider;
				}					
			}
			if (disableRandom.val)
			{
				rollChangeClock = 1.0f;
			}
			t = Mathf.SmoothStep(0.0f, 1.0f, rollChangeClock);
			roll = Mathf.Lerp(rollPrev, rollNext, t) * Mathf.Rad2Deg;
            Vector3 eulerAngles = head.transform.localEulerAngles;
            eulerAngles.z += roll;
            head.transform.localEulerAngles = eulerAngles;

            // compute angle
            currentAngle = Vector3.Angle(actualDir, targetDir);

			// Set chest and pelvis joint swivel.
            if (swivelEnabled.val && chestControl != null && pelvisControl != null)
			{
				float driveYTarget = Mathf.Clamp(actualH * -10.0f, -20.0f, 20.0f);
				chestControl.jointRotationDriveYTarget = driveYTarget;
				driveYTarget = Mathf.Clamp(actualH * -7.5f, -15.0f, 15.0f);		
				pelvisControl.jointRotationDriveYTarget = driveYTarget;
	        }
        }
	
        // Set a reference object to determine where "forward" is.
        public void SetReference(Transform transform)
        {
            reference = transform;
        }

        // Set a reference object to determine where "forward" is.
        public void SetReference(string atomID, string controlID)
        {
            reference = null;
            Atom atom = GetAtomById(atomID);
            if (atom == null)
            {
                SuperController.LogError("[GazeController] Atom '{0}' not found. " + atomID);
                return;
            }

            JSONStorable storable = atom.GetStorableByID(controlID);
            if (storable == null)
            {
                SuperController.LogError("[GazeController] Control '{0}/{1}' not found. " + atomID + " " + controlID);
                return;
            }

            reference = storable.transform;
        }
	
        // Set player as target to look at.
        public void SetLookAtPlayer()
        {
            gazeTarget = CameraTarget.centerTarget.transform;
            gazeOffset = Vector3.zero;
        }

        // Set player as target to look at.
        public void SetLookAtPlayer(Vector3 offset)
        {
            gazeTarget = CameraTarget.centerTarget.transform;
            gazeOffset = offset;
        }

        // Set transform as target to look at.
        public void SetLookAt(Transform transform)
        {
            gazeTarget = transform;
            gazeOffset = Vector3.zero;
        }

        // Set transform as target to look at.
        public void SetLookAt(Transform transform, Vector3 offset)
        {
            gazeTarget = transform;
            gazeOffset = offset;
        }

        // Set maximum offset angle (in degrees) from directly looking at the target during idle animations.
		//Values closer to 0 mean the character will be more focused on the target.
        public void SetFocusAngles(JSONStorableFloat angleH, JSONStorableFloat angleV)
        {
            focusAngleH.val = angleH.val * Mathf.Deg2Rad;
            focusAngleV.val = angleV.val * Mathf.Deg2Rad;
        }

        // Set min/max duration between random focus point changes.
        public void SetFocusChangeDuration(float min, float max)
        {
            focusChangeDurationMin.val = Mathf.Clamp(min, 0.01f, max);
            focusChangeDurationMax.val = Mathf.Max(max, focusChangeDurationMin.val);
        }

        // Set how fast the gaze adapts to a new target position.
        public void SetGazeDuration(float duration)
        {
            gazeDuration.val = Mathf.Max(duration, 0.001f);
        }

        // Current angle (in degrees) between look direction and target direction.
        public float GetCurrentAngle()
        {
            return currentAngle;
        }

        public void ClearLookAt()
        {
            gazeTarget = null;
        }

        private Vector3 RecombineDirection(float angleH, float angleV)
        {
            float cosV = Mathf.Cos(angleV);
            return new Vector3(
                Mathf.Sin(angleH) * cosV,
                Mathf.Sin(angleV),
                Mathf.Cos(angleH) * cosV
            );
        }

	    // Set gaze vertical offset.
        public void SetGazeOffsetV(float offset)
        {
			offsetV.val = offset;
		}	
		
	    // Set gaze horizontalal offset.
        public void SetGazeOffsetH(float offset)
        {
			offsetH.val = offset;
		}		
		
		// Chooser callback to set gaze target
		 public void setGazeTarget(string choice) {
			if (lookMode != null && gazeTarget != null) {
				lookMode.val = choice;
				if (choice == "Player") {
					gazeTarget = playerTarget;
				} else {
					gazeTarget = personEyeTarget;
				}
			}
		}

		
		// Can be called from FixedUpdate() of another plugin using this script.
        public void OnFixedUpdate()	{
			FixedUpdate();
		}	

        private Atom person;
        private FreeControllerV3 personEyeTargetControl;
        private Transform playerTarget;
        private Transform personEyeTarget;
        private Transform gazeTarget;
        private Vector3 gazeOffset;
        private Transform reference;

        // tweak parameters
        protected JSONStorableFloat activationDistance;
		protected JSONStorableFloat headTurnrangeH;
		protected JSONStorableFloat headTurnrangeV;
		protected JSONStorableFloat gazeDuration;
		protected JSONStorableFloat offsetV;
		protected JSONStorableFloat offsetH;
		protected JSONStorableFloat offsetR;		
		protected JSONStorableFloat focusChangeDurationMin;
		protected JSONStorableFloat focusChangeDurationMax;
		protected JSONStorableFloat focusAngleV;
		protected JSONStorableFloat focusAngleH;
		protected JSONStorableFloat rollChangeDurationMin;
		protected JSONStorableFloat rollChangeDurationMax;
		protected JSONStorableFloat rollAngleMax;
		protected JSONStorableStringChooser lookMode;
		protected JSONStorableBool swivelEnabled;
		protected JSONStorableBool disableRandom;
		protected JSONStorable eyes;		
		
        // runtime data
        private float velocityH = 0.0f;
        private float velocityV = 0.0f;
        private float focusChangeClock = 1.0f;
        private float focusChangeSpeed = 1.0f;
        private Vector2 focusNext = Vector2.zero;
        private Vector2 focusPrev = Vector2.zero;
        private float rollNext = 0.0f;
        private float rollPrev = 0.0f;
		private float offsetPrev = 0.0f;
		private bool rollOffsetHasChanged = false;
        private float rollChangeClock = 1.0f;
        private float rollChangeSpeed = 1.0f;
        private float currentAngle = 0.0f;
		private float rollOffsetMax = 45.0f;
		private float roll = 0.0f;
		private int disableRnd = 0;
    }
}
