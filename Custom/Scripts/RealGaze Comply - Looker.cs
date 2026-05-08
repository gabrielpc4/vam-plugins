using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
using System.Timers;

namespace Compliance {
    public class RealGazeLooker : MVRScript {
        //Script by pornaway4148415, modified by Almadiel
        //Based on/using code from VariousScientists42, ShortRecognition, MeshedVR, MacGruber and VeeRifter
        /*TODO:
         * Nothing!
        */

        //From VariousScientists42's LookAtMe script for saccades
        private Transform LookTargetL;
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
        protected JSONStorableFloat offsetX;
        protected JSONStorableFloat offsetZ;
        protected JSONStorableFloat LookAway;
        protected JSONStorableFloat lookerKey;
        protected JSONStorableFloat LookPlayer;
        protected JSONStorableFloat PlayerMirr;
        protected JSONStorableFloat maxAngle;
        private float ActualSRangeX;
        private float ActualSRangeY;
        private float ActualSRangeZ;
        private float ActualLRangeX;
        private float ActualLRangeY;
        private float ActualLRangeZ;
        private float targettime = 0.1f;
        private float saccadetime = 0.1f;
        private float loadtime = 3f;
        private float totalChance = 0;
        private bool lookatme;
        private bool mirrorPref = false;
        private bool suppressWarnings = true;

        //From ShortRecognition's LookAtMe script for reflections
        protected Rigidbody EyeTarget; //eyeTargetController
        protected Rigidbody LookTarget; //The actual target receiver
        protected Rigidbody ContainingHead; //Containing person atom's head
        protected Rigidbody PlayerCamera = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "CenterEye"); //Player camera

        //To add pupil dilation based on selection
        protected JSONStorable geometry;
        DAZCharacterSelector character;
        GenerateDAZMorphsControlUI morphControl;
        DAZMorph morphPupil;
        protected JSONStorableFloat morphVal;
        private float startingVal;
        private float targetVal;

        //By setting up this struct, I'm then able to make a list of these structs which I can use to have more than two valid targets for the character.
        public struct Target
        {
            public float relFreq;
            public float mirrPref;
            public float pupilScale;
            public Rigidbody receiver;
        }

        //This is the targets list, only contains receivers with a matching key float value.
        List<Target> validTargets = new List<Target>();

        protected void AddToTargets(float relFreqIn, float mirrPrefIn, float pupilScaleIn, Rigidbody receiverIn)
        {
            Target item = new Target();
            item.relFreq = relFreqIn;
            totalChance += relFreqIn;
            item.mirrPref = mirrPrefIn;
            item.pupilScale = pupilScaleIn;
            item.receiver = receiverIn;
            validTargets.Add(item);
        }

        public void CheckForTargets()
        {
            totalChance = 0;
            validTargets.Clear();
            AddToTargets(LookPlayer.val, PlayerMirr.val, 0, PlayerCamera);
            Atom current = null;
            JSONStorable receiver;
            Rigidbody input;
            string lookFor = "PA4148415.RealGazeTarget";
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
                if (atomUID != "[CameraRig]" && atomUID != null)
                {
                    current = SuperController.singleton.GetAtomByUid(atomUID);

                    if (current.on && current.gameObject.activeInHierarchy)
                    {
                        //find all plugin instances, check each one for matching key, add to targets if valid
                        foreach (string receiverID in current.GetStorableIDs())
                        {
                            if (receiverID.Substring(Math.Max(0, receiverID.Length - lookFor.Length)) == lookFor)
                            {
                                //SuperController.LogMessage("Found valid target " + receiverID);
                                receiver = current.GetStorableByID(receiverID);
                                if (receiver.GetFloatParamValue("Key value") == lookerKey.val || receiver.GetFloatParamValue("Key value") == 0f)
                                {
                                    if (receiver.GetStringChooserParamValue("receiver") != null)
                                    {
                                        input = current.rigidbodies.First(rb => rb != null && rb.name == receiver.GetStringChooserParamValue("receiver"));
                                        if (input.position != null)
                                        {
                                            AddToTargets(receiver.GetFloatParamValue("Relative frequency"), receiver.GetFloatParamValue("Mirror preference"), receiver.GetFloatParamValue("Pupil size effect"), input);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
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
        
        public override void Init()
        {
            try
            {
                if (containingAtom.type != "Person")
                {
                    SuperController.LogError($"Please add this plugin to the Person Atom whose eyes you want to animate, not '{containingAtom.type}'");
                    return;
                }
                pluginLabelJSON.val = "RealGaze Looker";
                person = containingAtom;
                eyes = person.GetStorableByID("Eyes");
                //This all allows me to directly manipulate the pupil dilation morph
                geometry = person.GetStorableByID("geometry");
                character = geometry as DAZCharacterSelector;
                morphControl = character.morphsControlUI;
                morphPupil = morphControl.GetMorphByDisplayName("Pupils Dialate");
                morphVal = new JSONStorableFloat("pupilMorphVal", morphPupil.morphValue, -1f, 1f);
                startingVal = morphVal.val;
                targetVal = startingVal;

                personEyeTargetControl = person.GetStorableByID("eyeTargetControl") as FreeControllerV3;
                personEyeTarget = personEyeTargetControl.transform;
                playerTarget = CameraTarget.centerTarget.transform;
                lookatme = true;
                suppressWarnings = true;
                totalChance = 0;

                chestControl = person.GetStorableByID("chestControl") as FreeControllerV3;
                pelvisControl = person.GetStorableByID("abdomenControl") as FreeControllerV3;
                head = person.GetStorableByID("headControl") as FreeControllerV3;
                reference = person.GetStorableByID("hipControl").transform;

                EyeTarget = containingAtom.rigidbodies.First(rb => rb.name == "eyeTargetControl");
                ContainingHead = containingAtom.rigidbodies.First(rb => rb.name == "headControl");

                lookerKey = new JSONStorableFloat("Key value", 0.0f, 0.0f, 10.0f, true, true);
                RegisterFloat(lookerKey);
                CreateSlider(lookerKey, false);

                LookAway = new JSONStorableFloat("Look Away Preference", 0.10f, 0f, 1f, true, true);
                RegisterFloat(LookAway);
                CreateSlider(LookAway, true);

                LookPlayer = new JSONStorableFloat("Look at player frequency", 1f, 0.0f, 1f, true, true);
                RegisterFloat(LookPlayer);
                CreateSlider(LookPlayer, false);

                PlayerMirr = new JSONStorableFloat("Player through mirror pref", 0.25f, 0.0f, 1f, true, true);
                RegisterFloat(PlayerMirr);
                CreateSlider(PlayerMirr, true);

                MinSRange = new JSONStorableFloat("Min Saccade Range", 0.01f, 0.0f, 0.3f, true, true);
                RegisterFloat(MinSRange);
                CreateSlider(MinSRange, false);

                MaxSRange = new JSONStorableFloat("Max Saccade Range", 0.03f, 0.0f, 0.3f, true, true);
                RegisterFloat(MaxSRange);
                CreateSlider(MaxSRange, true);

                MinLRange = new JSONStorableFloat("Min Look Away Range", 0.4f, 0.4f, 2f, true, true);
                RegisterFloat(MinLRange);
                CreateSlider(MinLRange, false);

                MaxLRange = new JSONStorableFloat("Max Look Away Range", 1f, 0.4f, 2f, true, true);
                RegisterFloat(MaxLRange);
                CreateSlider(MaxLRange, true);

                offsetX = new JSONStorableFloat("Look Away X Offset", 0f, -2f, 2f, true, true);
                RegisterFloat(offsetX);
                CreateSlider(offsetX, false);

                offsetZ = new JSONStorableFloat("Look Away Z Offset", 1.5f, -2f, 2f, true, true);
                RegisterFloat(offsetZ);
                CreateSlider(offsetZ, true);

                SaccadeSpeed = new JSONStorableFloat("Saccade Slowness", 0.6f, 0.5f, 0.8f, true, true);
                RegisterFloat(SaccadeSpeed);
                CreateSlider(SaccadeSpeed, false);

                gazeDuration = new JSONStorableFloat("Gaze Slowness", 0.7f, 0f, 5f, true, true);
                RegisterFloat(gazeDuration);
                CreateSlider(gazeDuration, true);

                TimeIntervalMin = new JSONStorableFloat("Min Time Between Targets", 1f, 0.8f, 10f, true, true);
                RegisterFloat(TimeIntervalMin);
                CreateSlider(TimeIntervalMin, false);

                TimeIntervalMax = new JSONStorableFloat("Max Time Between Targets", 4f, 0.8f, 10f, true, true);
                RegisterFloat(TimeIntervalMax);
                CreateSlider(TimeIntervalMax, true);

                focusChangeDurationMin = new JSONStorableFloat("Focus Change Duration Minimum", 1f, dmin => focusChangeDurationMax.SetVal(Mathf.Max(focusChangeDurationMax.val, dmin)), 1f, 10f, true, true);
                RegisterFloat(focusChangeDurationMin);
                CreateSlider(focusChangeDurationMin, false);

                focusChangeDurationMax = new JSONStorableFloat("Focus Change Duration Maximum", 4f, dmax => focusChangeDurationMin.SetVal(Mathf.Min(focusChangeDurationMin.val, dmax)), 1f, 10f, true, true);
                RegisterFloat(focusChangeDurationMax);
                CreateSlider(focusChangeDurationMax, true);

                rollChangeDurationMin = new JSONStorableFloat("Roll Change Duration Minimum", 2f, rmin => rollChangeDurationMax.SetVal(Mathf.Max(rollChangeDurationMax.val, rmin)), 1f, 10f, true, true);
                RegisterFloat(rollChangeDurationMin);
                CreateSlider(rollChangeDurationMin, false);

                rollChangeDurationMax = new JSONStorableFloat("Roll Change Duration Maximum", 6f, rmax => rollChangeDurationMin.SetVal(Mathf.Min(rollChangeDurationMin.val, rmax)), 1f, 10f, true, true);
                RegisterFloat(rollChangeDurationMax);
                CreateSlider(rollChangeDurationMax, true);

                maxAngle = new JSONStorableFloat("Max Look Angle", 155f, 45f, 180f, true, true);
                RegisterFloat(maxAngle);
                CreateSlider(maxAngle, false);

                rollAngleMax = new JSONStorableFloat("Roll Angle Maximum", 6f, 0f, 40f, true, true);
                RegisterFloat(rollAngleMax);
                CreateSlider(rollAngleMax, true);

                focusAngleH = new JSONStorableFloat("Focus Angle Horizontal", 4f, 0f, 40f, true, true);
                RegisterFloat(focusAngleH);
                CreateSlider(focusAngleH, false);

                focusAngleV = new JSONStorableFloat("Focus Angle Vertical", 6f, 0f, 40f, true, true);
                RegisterFloat(focusAngleV);
                CreateSlider(focusAngleV, true);

                offsetH = new JSONStorableFloat("Horizontal Offset Angle", 0.0f, -30.0f, 30.0f, true, true);
                RegisterFloat(offsetH);
                CreateSlider(offsetH, false);

                offsetV = new JSONStorableFloat("Vertical Offset Angle", -5.0f, -30.0f, 30.0f, true, true);
                RegisterFloat(offsetV);
                CreateSlider(offsetV, true);

                swivelEnabled = new JSONStorableBool("Enable Body Swivel Action", true);
                swivelEnabled.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(swivelEnabled);
                CreateToggle(swivelEnabled, false);

                saccadeMove = new JSONStorableBool("Saccades Move Head", false);
                saccadeMove.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(saccadeMove);
                CreateToggle(saccadeMove, true);
            }
			catch (Exception e)
            {
				SuperController.LogError("Exception caught on load: " + e);
			}
		}

		void Start()
        {
			
		}

        protected void UpdateSaccade()
        {
            ActualSRangeX = RandNeg(UnityEngine.Random.Range(MinSRange.val, MaxSRange.val));
            ActualSRangeY = RandNeg(UnityEngine.Random.Range(MinSRange.val, MaxSRange.val));
            ActualSRangeZ = RandNeg(UnityEngine.Random.Range(MinSRange.val, MaxSRange.val));
            ActualSOffset = new Vector3(ActualSRangeX, ActualSRangeY, ActualSRangeZ);
        }

        protected Rigidbody AddSaccade(Rigidbody rbTarget)
        {
            rbTarget.position = rbTarget.position + ActualSOffset;
            return rbTarget;
        }

        protected void UpdateTarget()
        {
            ActualLRangeX = offsetX.val + RandNeg(UnityEngine.Random.Range(MinLRange.val, MaxLRange.val)) * 2f;
            ActualLRangeY = RandNeg(UnityEngine.Random.Range(MinLRange.val, MaxLRange.val)) * 0.8f;
            ActualLRangeZ = offsetZ.val + RandNeg(UnityEngine.Random.Range(MinLRange.val, MaxLRange.val)) * 2f;
            ActualLOffset = new Vector3(ActualLRangeX, ActualLRangeY, ActualLRangeZ);
            lookatme = UnityEngine.Random.value > LookAway.val;
            if (totalChance < 0.01)
            {
                lookatme = false;
            }
            float selection = UnityEngine.Random.Range(0, totalChance);
            float running = 0;
            foreach (Target check in validTargets)
            {
                if ((check.relFreq + running) >= selection)
                {
                    LookTarget = check.receiver;
                    if (check.mirrPref > 0)
                    {
                        mirrorPref = UnityEngine.Random.value < check.mirrPref;
                    }
                    else
                    {
                        mirrorPref = false;
                    }
                    startingVal = morphPupil.morphValue;
                    targetVal = morphVal.val + (0.6f * check.pupilScale);
                    break;
                }
                else
                {
                    running += check.relFreq;
                }
            }
            if (saccadeMove.val)
            {
                gazeTarget = personEyeTarget;
            }
            else if (lookatme)
            {
                gazeTarget = LookTarget.transform;
            }
            else
            {
                gazeTarget = personEyeTarget;
                startingVal = morphPupil.morphValue;
                targetVal = morphVal.val;
            }
        }

        // Update is called with each rendered frame by Unity
        void Update()
        {
            try {
                //If saccade time has passed, update saccade values
                saccadetime -= Time.deltaTime;
                if (saccadetime <= 0f)
                {
                    saccadetime = SaccadeSpeed.val;
                    UpdateSaccade();
                }
                //If target switch time has passed, update target
                targettime -= Time.deltaTime;
                if (targettime <= 0f)
                {
                    targettime = UnityEngine.Random.Range(TimeIntervalMin.val, TimeIntervalMax.val);
                    UpdateTarget();
                }
                //If loading time has passed, check for new targets
                loadtime -= Time.deltaTime;
                if (loadtime <= 0f)
                {
                    loadtime = 3f;
                    suppressWarnings = false;
                    CheckForTargets();
                }
                if (LookTarget == null)
                {
                    //SuperController.LogError("No valid eye target selected.");
                    return;
                }
                //First, use ShortRecognition's "find best gaze" method to figure out whether or not to look through the mirror.
                float minAngle = 361f; //overly big number
                Vector3 headForward = ContainingHead.rotation * Vector3.forward; //constant
                Vector3 gazeForward = LookTarget.rotation * Vector3.forward; //constant
                Vector3 minFacingVector = Vector3.zero; //meaningless but cannot be null
                foreach (Rigidbody reflector in SuperController.singleton.GetAtoms().Where(atom => atom.category == "Reflective").Select(atom => atom.rigidbodies.First()))
                {
                    Vector3 reflectorUp = reflector.rotation * Vector3.up;
                    //throw out any mirror that containing cannot look at
                    if (Vector3.Angle(headForward, Vector3.Dot(reflectorUp, reflector.position - ContainingHead.position) * reflectorUp) > maxAngle.val)
                    {
                        //SuperController.LogMessage("Threw out mirror " + reflector);
                        continue;
                    }
                    Vector3 facingVector = Vector3.Dot(reflectorUp, reflector.position - LookTarget.position) * reflectorUp;
                    float diffAngle = Vector3.Angle(gazeForward, facingVector);
                    if (diffAngle < minAngle)
                    {
                        minAngle = diffAngle;
                        minFacingVector = facingVector;
                    }
                }
                //Then, set the eye target position to the appropriate location.
                if (!lookatme)
                {
                    //For no intended LoS
                    EyeTarget.position = ContainingHead.position + ActualLOffset;
                    if (!saccadeMove.val)
                    {
                        gazeTarget.position = ContainingHead.position + ActualLOffset;
                    }
                }
                else
                {
                    bool directGood = Vector3.Angle(gazeForward, ContainingHead.position - LookTarget.position) < minAngle && Vector3.Angle(headForward, LookTarget.position - ContainingHead.position) <= maxAngle.val;
                    bool mirrorGood = minAngle <= maxAngle.val;

                    if ((mirrorPref && mirrorGood) || (!mirrorPref && !directGood && mirrorGood))
                    {
                        //For mirror LoS
                        EyeTarget.position = 2 * minFacingVector + LookTarget.position;
                        if (!saccadeMove.val)
                        {
                            gazeTarget = EyeTarget.transform;
                        }
                    }
                    else if ((mirrorPref && !mirrorGood && directGood) || (!mirrorPref && directGood))
                    {
                        //For direct LoS
                        EyeTarget.position = LookTarget.position;
                    }
                    else
                    {
                        //For no valid LoS
                        EyeTarget.position = ContainingHead.position + headForward * 10;
                        //This rarely happens as the max angle can be adjusted up to 180 which is decently far outside the technical 140 degree RoM of Gaze.
                    }
                }
                EyeTarget.position = AddSaccade(EyeTarget).position;
                }
            catch (Exception e)
            {
                //Warnings are suppressed until CheckForTargets() runs at least once
                if (!suppressWarnings)
                {
                    SuperController.LogError("Exception caught in RealEyes: " + e);
                }
            }
            
            if (morphPupil.morphValue < targetVal - 0.02 || morphPupil.morphValue > targetVal + 0.02)
            {
                morphPupil.morphValue += (targetVal - startingVal) / 180;
            }

            if (gazeTarget == null || head == null || reference == null)
                return;

            // compute horizontal and vertical angles
            gazeOffset = offsetV.val * Mathf.Deg2Rad * Vector3.up + offsetH.val * Mathf.Deg2Rad * Vector3.left;
            Vector3 lookAtPosition = gazeTarget.TransformPoint(gazeOffset);
            Vector3 actualDir = reference.InverseTransformDirection(head.transform.forward);
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
                focusChangeSpeed = 1.0f / UnityEngine.Random.Range(focusChangeDurationMin.val, focusChangeDurationMax.val);
                focusChangeClock = 0.0f;
                focusPrev = focusNext;
                focusNext = UnityEngine.Random.insideUnitCircle;
            }
            float t = Mathf.SmoothStep(0.0f, 1.0f, focusChangeClock);
            targetH += Mathf.Lerp(focusPrev.x, focusNext.x, t) * focusAngleH.val * Mathf.Deg2Rad;
            targetV += Mathf.Lerp(focusPrev.y, focusNext.y, t) * focusAngleV.val * Mathf.Deg2Rad;

            // adjust angles
            targetH = Mathf.Clamp(targetH, -maxAngleH, maxAngleH);
            targetV = Mathf.Clamp(targetV, -maxAngleV, maxAngleV);
            actualH = Mathf.SmoothDamp(actualH, targetH, ref velocityH, gazeDuration.val, Mathf.Infinity, Time.fixedDeltaTime);
            actualV = Mathf.SmoothDamp(actualV, targetV, ref velocityV, gazeDuration.val, Mathf.Infinity, Time.fixedDeltaTime);

            // recombine
            actualDir = RecombineDirection(actualH, actualV);
            targetDir = RecombineDirection(targetH, targetV);
            actualDir = reference.TransformDirection(actualDir);

            // compute angle
            currentAngle = Vector3.Angle(actualDir, targetDir);

            if (head.currentRotationState == FreeControllerV3.RotationState.Comply && currentAngle > 1.0f)
            {
                // compliance. prevent roll, preserve head up direction
                head.transform.LookAt(head.transform.position + actualDir, head.transform.up);
                
                // apply roll
                /*rollChangeClock += rollChangeSpeed * Time.fixedDeltaTime;
                if (rollChangeClock >= 1.0f)
                {
                    rollChangeSpeed = 1.0f / UnityEngine.Random.Range(rollChangeDurationMin.val, rollChangeDurationMax.val);
                    rollChangeClock = 0.0f;
                    rollPrev = rollNext;
                    rollNext = UnityEngine.Random.Range(-(rollAngleMax.val * Mathf.Deg2Rad), rollAngleMax.val * Mathf.Deg2Rad);
                }
                t = Mathf.SmoothStep(0.0f, 1.0f, rollChangeClock);
                float roll = Mathf.Lerp(rollPrev, rollNext, t);
                Vector3 eulerAngles = head.transform.localEulerAngles;
                eulerAngles.z = roll * Mathf.Rad2Deg;
                head.transform.localEulerAngles = eulerAngles;*/

                // Set chest and pelvis joint swivel.
                if (swivelEnabled.val && chestControl != null && pelvisControl != null)
                {
                    maxAngleH = 140.0f * Mathf.Deg2Rad;
                    float driveYTarget = Mathf.Clamp(actualH * -10.0f, -20.0f, 20.0f);
                    chestControl.jointRotationDriveYTarget = driveYTarget;
                    driveYTarget = Mathf.Clamp(actualH * -7.5f, -15.0f, 15.0f);
                    
                    pelvisControl.jointRotationDriveYTarget = driveYTarget;
                }
                else
                {
                    maxAngleH = 90.0f * Mathf.Deg2Rad;
                }
            }
        }

        protected void FixedUpdate()
        {
            
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

        private Atom person;
        private FreeControllerV3 personEyeTargetControl;
        private FreeControllerV3 chestControl;
        private FreeControllerV3 pelvisControl;
        private Transform playerTarget;
        private Transform personEyeTarget;
        private Transform gazeTarget;
        private Vector3 gazeOffset;
        private FreeControllerV3 head;
        private Transform reference;

        // tweak parameters
        protected JSONStorableFloat gazeDuration;
        protected JSONStorableFloat offsetV;
        protected JSONStorableFloat offsetH;
        protected JSONStorableFloat focusChangeDurationMin;
        protected JSONStorableFloat focusChangeDurationMax;
        protected JSONStorableFloat focusAngleV;
        protected JSONStorableFloat focusAngleH;
        protected JSONStorableFloat rollChangeDurationMin;
        protected JSONStorableFloat rollChangeDurationMax;
        protected JSONStorableFloat rollAngleMax;
        protected JSONStorableStringChooser lookMode;
        protected JSONStorableBool swivelEnabled;
        protected JSONStorableBool saccadeMove;
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
        private float rollChangeClock = 1.0f;
        private float rollChangeSpeed = 1.0f;
        private float currentAngle = 0.0f;
        private float maxAngleH = 90.0f * Mathf.Deg2Rad;
        private float maxAngleV = 45.0f * Mathf.Deg2Rad;

    }
}