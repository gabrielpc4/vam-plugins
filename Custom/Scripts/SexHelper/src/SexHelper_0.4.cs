// ToDo:
// Easing
// Activation distance
// Target select
// Angle offset
// Movement offset

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
using GPUTools.Hair.Scripts.Settings;

namespace PhysisPlugins
{
    public class SexHelper04 : MVRScript
    {
        public static string pluginName = "Sex Helper";
        public static string pluginVersion = "0.4";

		private static readonly string SCENE_DIR = "file:///Custom/Assets/Audio/";
		private static readonly string AUDIO_DIR = "SexHelper/";

		private BreathEntry[] breathEntries = new BreathEntry[] {
			new BreathEntry("00.wav", 1f),
			new BreathEntry("01.wav", 1f),
			new BreathEntry("02.wav", 1f),
			new BreathEntry("03.wav", 1f),
			new BreathEntry("04.wav", 1f),
			new BreathEntry("05.wav", 1f),
			new BreathEntry("06.wav", 1f),
			new BreathEntry("07.wav", 1f),
			new BreathEntry("08.wav", 1f),
			new BreathEntry("09.wav", 1f),
			new BreathEntry("10.wav", 1f),
			new BreathEntry("11.wav", 1f),
			new BreathEntry("12.wav", 1f),
			new BreathEntry("13.wav", 1f),
			new BreathEntry("14.wav", 1f),
			new BreathEntry("15.wav", 1f),
			new BreathEntry("16.wav", 1f),
			new BreathEntry("17.wav", 1f),
			new BreathEntry("18.wav", 1f),
			new BreathEntry("19.wav", 1f),
			new BreathEntry("20.wav", 1f),
			new BreathEntry("21.wav", 1f),
			new BreathEntry("22.wav", 1f),
			new BreathEntry("23.wav", 1f),
            new BreathEntry("24.wav", 1f),
            new BreathEntry("25.wav", 1f),
            new BreathEntry("26.wav", 1f),
            new BreathEntry("27.wav", 1f),
            new BreathEntry("28.wav", 1f),
            new BreathEntry("29.wav", 1f),
            new BreathEntry("30.wav", 1f),
            new BreathEntry("31.wav", 1f),
            new BreathEntry("32.wav", 1f),
            new BreathEntry("33.wav", 1f),
            new BreathEntry("34.wav", 1f),
            new BreathEntry("35.wav", 1f),
            new BreathEntry("36.wav", 1f),
            new BreathEntry("37.wav", 1f),
			new BreathEntry("38.wav", 1f),
			new BreathEntry("39.wav", 1f),
		};

        protected JSONStorableBool autoStart;

        //DISABLED SWEAT/GLOSS AS NOT WORKING IN 1.18 AND DIDN'T CARE ENOUGH TO FIX
        //protected JSONStorableBool sweatEnabled;
        //protected JSONStorableFloat gloss; 
        //protected JSONStorableFloat sweatMultiplier;
        //protected JSONStorableFloat sweatSpeedMultiplier;

        protected JSONStorableBool alignOnlyDisableMotion;
        protected JSONStorableBool soundEnabled;
        protected JSONStorableBool soundDebug;
		private JSONStorable headAudio;
        private JSONStorableFloat pitchJSON;
        private float pitchBase;
        private float pitchCurrent;
        private JSONStorableFloat pitchMultiplier;
        private JSONStorableFloat soundIntensityMultiplier;
        private JSONStorableFloat soundVariance;
		private BreathEntry breatheEntry;


        private Atom him;
        private FreeControllerV3 penisBase;
        private FreeControllerV3 penisMid;
        private FreeControllerV3 penisTip;
        private FreeControllerV3 himPelvisControl;
        private Rigidbody himPelvis;
        private Rigidbody gen1;
        private Rigidbody gen2;
        private Rigidbody gen3;

        private FreeControllerV3 hipControl;
        private FreeControllerV3 pelvisControl;
        private Rigidbody labiaTrigger;
        private Rigidbody vaginaTrigger;
        private Rigidbody deepVaginaTrigger;
        private Rigidbody deeperVaginaTrigger;
        private Rigidbody pelvis;

        private JSONStorableFloat durationJSON;
        private JSONStorableFloat durationRangeJSON;
        private JSONStorableFloat durationUpdateIntervalJSON;

        private JSONStorableStringChooser maleAtomJSON;
        private JSONStorableFloat penisPositionOneJSON;
        private JSONStorableFloat penisPositionTwoJSON;
        private JSONStorableFloat penisRotationJSON;
        private JSONStorableFloat penisPositionJSON;
        private JSONStorableFloat penisRangeJSON;
        private JSONStorableBool penisEnabledJSON;

        private JSONStorableFloat hipPositionOneJSON;
        private JSONStorableFloat hipPositionTwoJSON;
        private JSONStorableFloat hipRotationJSON;
        private JSONStorableFloat hipRangeJSON;
        private JSONStorableBool hipEnabledJSON;
        private JSONStorableBool targetAnus;

        private JSONStorableFloat grindJSON;
        private JSONStorableFloat grindMultiplierJSON;

        public JSONStorableStringChooser easingJSON;
        public Func<float, float> easing;

        private bool penisWasEnabled = false;
        private JSONStorableBool useGen3RotationJSON;

        private bool ready = false;

        public override void Init()
        {
            try
            {
                // Easing Setup
                Easing.SetEasingChoices();
                easingJSON = new JSONStorableStringChooser("Easing Choice", Easing.easingChoicesList, "Linear", "Easing", SetEasing);
                easingJSON.storeType = JSONStorableParam.StoreType.Full;
                easingJSON.val = "Quadratic InOut";
                SetEasing("Quadratic InOut");
                
                // UI Setup
                var btn = CreateButton("Start");
                btn.button.onClick.AddListener(() => { StartSex(); });
                btn.buttonColor = Color.green;

                btn = CreateButton("Stop", true);
                btn.button.onClick.AddListener(() => { StopSex(); });
                btn.buttonColor = Color.red;

                soundEnabled = new JSONStorableBool("Sound Enabled", false);
                CreateToggle(soundEnabled);
                RegisterBool(soundEnabled);
                soundEnabled.storeType = JSONStorableParam.StoreType.Full;

                useGen3RotationJSON = new JSONStorableBool("Use Gen3 Rotations", false);
                CreateToggle(useGen3RotationJSON);
                RegisterBool(useGen3RotationJSON);
                useGen3RotationJSON.storeType = JSONStorableParam.StoreType.Full;

                alignOnlyDisableMotion = new JSONStorableBool("Align Only (Disable Motion)", false);
                CreateToggle(alignOnlyDisableMotion);
                RegisterBool(alignOnlyDisableMotion);
                alignOnlyDisableMotion.storeType = JSONStorableParam.StoreType.Full;

                
                autoStart = new JSONStorableBool("Start on Scene Load", true);
                CreateToggle(autoStart, true);
                RegisterBool(autoStart);
                autoStart.storeType = JSONStorableParam.StoreType.Full;

                // Duration sliders
                durationJSON = new JSONStorableFloat("Thrust Time (lower = faster)", 0.25f, 0f, 1f, false);
                RegisterFloat(durationJSON);
                durationJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(durationJSON);


                durationRangeJSON = new JSONStorableFloat("Thrust Time Range", 0.1f, 0f, 1f, false);
                RegisterFloat(durationRangeJSON);
                durationRangeJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(durationRangeJSON, true);

                durationUpdateIntervalJSON = new JSONStorableFloat("Thrust Time Update Interval", 5f, 0f, 1f, false);
                RegisterFloat(durationUpdateIntervalJSON);
                durationUpdateIntervalJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(durationUpdateIntervalJSON);

                CreateScrollablePopup(easingJSON, true).popupPanelHeight = 1100f; 

                // Left side (male)
                var heading = CreateTextField(new JSONStorableString("male", "\n Male"));
                heading.height = 20f;
                heading.backgroundColor = Color.blue;
                heading.textColor = Color.white;

                penisEnabledJSON = new JSONStorableBool("Penis Thrust Enabled", true);
			    RegisterBool(penisEnabledJSON);
                CreateToggle(penisEnabledJSON);


                penisPositionOneJSON = new JSONStorableFloat("Position One (higher = deeper)", -0.2f, -5f, 5f, false);
                RegisterFloat(penisPositionOneJSON);
                penisPositionOneJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(penisPositionOneJSON);

                penisPositionTwoJSON = new JSONStorableFloat("Position Two", -1.0f, -5f, 5f, false);
                RegisterFloat(penisPositionTwoJSON);
                penisPositionTwoJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(penisPositionTwoJSON);

                penisRangeJSON = new JSONStorableFloat("Random Range", 0.2f, 0f, 5f, false);
                RegisterFloat(penisRangeJSON);
                penisRangeJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(penisRangeJSON);

                penisPositionJSON = new JSONStorableFloat("Penis Position Up/Down", 0f, -1f, 1f, false);
                RegisterFloat(penisPositionJSON);
                penisPositionJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(penisPositionJSON);

                penisRotationJSON = new JSONStorableFloat("Penis Angle Up/Down", 0f, -5f, 5f, false);
                RegisterFloat(penisRotationJSON);
                penisRotationJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(penisRotationJSON);

                // Right side (female)
                heading = CreateTextField(new JSONStorableString("female", "\n Female"),true);
                heading.height = 20f;
                heading.backgroundColor = Color.cyan;
                heading.textColor = Color.white;
                

                hipEnabledJSON = new JSONStorableBool("Hip Thrust Enabled (Experimental)", false);
                CreateToggle(hipEnabledJSON, true);
                RegisterBool(hipEnabledJSON);
                hipEnabledJSON.storeType = JSONStorableParam.StoreType.Full;
                
                // Can't get this to work 
                //targetAnus = new JSONStorableBool("Target Butt", false);
                //CreateToggle(targetAnus, true);
                //RegisterBool(targetAnus);
                //targetAnus.storeType = JSONStorableParam.StoreType.Full;

                // Can't get this to work
                //JSONStorableFloat.SetFloatCallback updateHipRotationCallback = (float val) => { UpdateHipRotation(val); };
                //hipRotationJSON = new JSONStorableFloat("Hip Rotate Up/Down", 0f, updateHipRotationCallback, -50f, 50f, false);
                //RegisterFloat(hipRotationJSON);
                //hipRotationJSON.storeType = JSONStorableParam.StoreType.Full;
                //CreateSlider(hipRotationJSON, true);

                hipPositionOneJSON = new JSONStorableFloat("Hip Position One", 0f, -3f, 3f, false);
                RegisterFloat(hipPositionOneJSON);
                hipPositionOneJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(hipPositionOneJSON, true);

                hipPositionTwoJSON = new JSONStorableFloat("Hip Position Two", 1f, -3f, 3f, false);
                RegisterFloat(hipPositionTwoJSON);
                hipPositionTwoJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(hipPositionTwoJSON, true);

                hipRangeJSON = new JSONStorableFloat("Hip Random Range", 0f, 0f, 5f, false);
                RegisterFloat(hipRangeJSON);
                hipRangeJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(hipRangeJSON, true);

                grindJSON = new JSONStorableFloat("Hip Grind Amount", 5f, 0f, 50f, false);
                RegisterFloat(grindJSON);
                grindJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(grindJSON, true);

                grindMultiplierJSON = new JSONStorableFloat("Grind Speed Multiplier (lower = faster)", 2f, 0f, 10f, false);
                RegisterFloat(grindMultiplierJSON);
                grindMultiplierJSON.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(grindMultiplierJSON, true);

                // Sound
                heading = CreateTextField(new JSONStorableString("sound", "\nSound Options"));
                heading.height = 20f;
                heading.backgroundColor = Color.yellow;
                heading.textColor = Color.white;

                soundDebug = new JSONStorableBool("Show Sound Debug", false);
                CreateToggle(soundDebug);
                RegisterBool(soundDebug);
                soundDebug.storeType = JSONStorableParam.StoreType.Full;

                pitchMultiplier = new JSONStorableFloat("Pitch Multiplier", 0.25f, 0f, 1f, false);
                RegisterFloat(pitchMultiplier);
                pitchMultiplier.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(pitchMultiplier);

                soundIntensityMultiplier = new JSONStorableFloat("Sound Intensity Multiplier", 0.15f, 0f, 1f, false);
                RegisterFloat(soundIntensityMultiplier);
                soundIntensityMultiplier.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(soundIntensityMultiplier);

                soundVariance = new JSONStorableFloat("Sound Variance (1 = all sounds)", 0.3f, 0f, 1f, false);
                RegisterFloat(soundVariance);
                soundVariance.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(soundVariance);

                // Sweat
                /* heading = CreateTextField(new JSONStorableString("sound", "\nSweat Options"),true);
                heading.height = 20f;
                heading.backgroundColor = Color.yellow;
                heading.textColor = Color.white;

               sweatEnabled = new JSONStorableBool("Sweat Enabled", true);
                CreateToggle(sweatEnabled, true);
                RegisterBool(sweatEnabled);
                sweatEnabled.storeType = JSONStorableParam.StoreType.Full;

                sweatMultiplier = new JSONStorableFloat("Sweat Multiplier (higher = more sweat)", 0.65f, 0f, 1f, false);
                RegisterFloat(sweatMultiplier);
                sweatMultiplier.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(sweatMultiplier, true);


                sweatMultiplier = new JSONStorableFloat("Sweat Multiplier (higher = more sweat)", 0.65f, 0f, 1f, false);
                RegisterFloat(sweatMultiplier);
                sweatMultiplier.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(sweatMultiplier, true);

                sweatSpeedMultiplier = new JSONStorableFloat("Sweat Speed Multiplier (higher = faster change)", 0.05f, 0f, 1f, false);
                RegisterFloat(sweatSpeedMultiplier);
                sweatSpeedMultiplier.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(sweatSpeedMultiplier, true);*/

                // Utility buttons
                heading = CreateTextField(new JSONStorableString("utility", "\nHelper Functions"));
                heading.height = 20f;
                heading.backgroundColor = Color.yellow;
                heading.textColor = Color.white;

                CreateSpacer(true);

                btn = CreateButton("Penis Physics to Hard");
                btn.button.onClick.AddListener(() => { PenisHardPhysics(); });
                btn.buttonColor = Color.blue;
                btn.textColor = Color.white;
                
                btn = CreateButton("Penis Physics to Default");
                btn.button.onClick.AddListener(() => { PenisDefaultPhysics(); });
                btn.buttonColor = Color.blue;
                btn.textColor = Color.white;
                btn = CreateButton("Penis Shrinkifier");
                btn.button.onClick.AddListener(() => { PenisShrink(); });
                btn.buttonColor = Color.blue;
                btn.textColor = Color.white;
                btn = CreateButton("Penis Regrow");
                btn.button.onClick.AddListener(() => { PenisGrow(); });
                btn.buttonColor = Color.blue;
                btn.textColor = Color.white;

                btn = CreateButton("Hip/Pelvis Physics to Hard", true);
                btn.button.onClick.AddListener(() => { FemaleHardPhysics(); });
                btn.buttonColor = Color.cyan;
                btn.textColor = Color.white;
                btn = CreateButton("Hip/Pelvis Physics to Default", true);
                btn.button.onClick.AddListener(() => { FemaleDefaultPhysics(); });
                btn.buttonColor = Color.cyan;
                btn.textColor = Color.white;

                // Register actions
                JSONStorableAction StartSexAction = new JSONStorableAction("Start Sex", () =>
                {
                    StartSex();
                });
                RegisterAction(StartSexAction);

                JSONStorableAction StopSexAction = new JSONStorableAction("Stop Sex", () =>
                {
                    StopSex();
                });
                RegisterAction(StopSexAction);
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private bool isActive = false;

        private float lerpTime;
        private float lerpTimer;
        private float durationUpdateTimer;

        private float soundDurationTimer;
        private float soundDuration;
        private int breatheIndex = 0;

        private float glossTarget;
        private float glossSpeed = 2f;

        private Vector3 penisTarget;
        private Vector3 penisStart;
        private Vector3 penisCurrent;
        private Vector3 penisAngleOffset;
        private Vector3 penisPositionOffset;
        private float penisPosition;
        private bool penisIsPositionOne = false;
        private Vector3 penisResetPosition;

        private Vector3 hipTarget;
        private Vector3 hipStart;
        private Vector3 hipCurrent;
        private Vector3 pelvisPositionOffset;
        private Quaternion hipRotation;

        private Vector3 grindTarget;
        private Vector3 grindCurrent;
        private Vector3 grindStart;
        private float grindLerpTime;
        private float grindLerpTimer;


        private float perc; // percent of lerp comlete

        private float hipPosition;
        private bool hipIsPositionOne = false;

        private Vector3 hipResetPosition;
        private Quaternion hipResetRotation;

        private DAZMorph penisLength;
        private float penisLengthVal;

        // Start is called once before Update or FixedUpdate is called and after Init()
        void Start()
        {
            try
            {
                //                him = containingAtom;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // Update is called with each rendered frame by Unity
        void Update()
        {
            try
            {
                if (!ready && !SuperController.singleton.isLoading)
                {
                    
                    //sex helper now only works on scenes with one male atom
                    IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

                    foreach (Atom at in personAtoms)
                    {
                        if (at.GetComponentInChildren<DAZCharacter>().isMale) {
                            him = at;
                            break;
                        }
                    }

                    if (him == null) {
                        return; //just keep checking, as VAM now stops loading before the male is even ready!
                        //SuperController.LogError("You must add Sex Helper to a Female Atom after there is already a Male atom in the scene.");
                    } else {
                        penisBase = him.freeControllers.First(fc => fc.name == "penisBaseControl");
                        penisMid = him.freeControllers.First(fc => fc.name == "penisMidControl");
                        penisTip = him.freeControllers.First(fc => fc.name == "penisTipControl");
                        himPelvisControl = him.freeControllers.First(fc => fc.name == "pelvisControl");
                        himPelvis = him.rigidbodies.First(rb => rb.name == "pelvis");
                        gen1 = him.rigidbodies.First(rb => rb.name == "Gen1");
                        gen2 = him.rigidbodies.First(rb => rb.name == "Gen2");
                        gen3 = him.rigidbodies.First(rb => rb.name == "Gen3");
                    }

                    SyncFemaleAtom();

                    if (autoStart.val)
                    {
                        StartSex();
                    }
                    
                    ready = true;
                }

                // put code in here
                if (soundEnabled.val)
                {
                    soundDurationTimer += Time.fixedDeltaTime;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private float startTime;

        // FixedUpdate is called with each physics simulation frame by Unity
        void FixedUpdate()
        {
            try
            {
                if (!ready) return;

                if (isActive)
                {
                    if (penisWasEnabled && !penisEnabledJSON.val)
                    {
                        penisWasEnabled = false;
                        penisBase.transform.position = penisTarget;
                        penisPosition = 0f;
                    }

                    // Update Timers;
                    durationUpdateTimer += Time.fixedDeltaTime;
                    lerpTimer += Time.fixedDeltaTime;
                    grindLerpTimer += Time.fixedDeltaTime;
                    if (lerpTimer > lerpTime)
                    {
                        lerpTimer = lerpTime;
                    }

                    // Update Targets
                    penisTarget = (vaginaTrigger.transform.position + (deeperVaginaTrigger.transform.position - vaginaTrigger.transform.position) * penisPosition);
                    hipTarget = (penisBase.transform.position + (penisBase.transform.forward) * (hipPosition / 10)); // divide by 10 to make the hip sliders more reasonable
                    // Add penis target offset
                    penisPositionOffset.x = 0;
                    penisPositionOffset.y = penisPositionJSON.val;
                    penisPositionOffset.z = 0;
                    penisTarget += penisPositionOffset;

                    // update penis position 
                    if (penisEnabledJSON.val)
                    {
                        penisWasEnabled = true;
                        perc = lerpTimer / lerpTime;
                        // add easing
                        perc = easing(perc);
                        penisCurrent = Vector3.Lerp(penisStart, penisTarget, perc);
                        penisBase.transform.position = penisCurrent;
                    }
                    if (penisCurrent == penisTarget && penisEnabledJSON.val && !alignOnlyDisableMotion.val)
                    {
                        NewThrust("penis");
                    }

                    // update hip position 
                    if (hipEnabledJSON.val)
                    {
                        perc = lerpTimer / lerpTime;
                        perc = easing(perc);
                        hipCurrent = Vector3.Lerp(hipStart, hipTarget, perc);
                        hipControl.transform.position = hipCurrent;
                    }
                    if (hipCurrent == hipTarget && hipEnabledJSON.val && !alignOnlyDisableMotion.val)
                    {
                        NewThrust("hip");
                    }

                    // update grind

                    perc = grindLerpTimer / grindLerpTime;
                    grindCurrent = Vector3.Lerp(grindStart, grindTarget, perc);
                    pelvis.AddRelativeTorque(grindCurrent);
                    if (grindCurrent == grindTarget)
                    {
                        SetNewGrindTarget();
                    }

                    // Update sweat
                    /*if (sweatEnabled.val)
                    {
                        gloss.val = Mathf.Lerp(gloss.val, glossTarget, sweatSpeedMultiplier.val * Time.fixedDeltaTime);
                    }*/
                }


                // Update rotations
                UpdateRotations();

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void NewThrust(string type)
        {
            if (type == "penis")
            {
                SetNewPenisTarget();
            }
            if (type == "hip")
            {
                SetNewHipTarget();
            }
            // Play sound
            if (soundEnabled.val)
            {
                if (soundDurationTimer > soundDuration)
                {
                    PlayNewSound();
                }
            }

            // Update gloss target
            /*if (sweatEnabled.val)
            {
                glossTarget = 3f / lerpTime * sweatMultiplier.val;
            }*/
        }

        private void PlayNewSound()
        {
            // random pitch
            pitchCurrent = pitchBase + UnityEngine.Random.Range(-0.05f, 0.05f);
            pitchCurrent += 0.03f / lerpTime * pitchMultiplier.val;
            pitchJSON.val = pitchCurrent;
            

            // Copy MacGruber's random selection code
            float v = soundVariance.val * (breathEntries.Length-1);	
//            float bi = breatheIntensity * (breathEntries.Length-1);
            float bi = (breathEntries.Length-1) / lerpTime * soundIntensityMultiplier.val;
            float min = Mathf.Max(bi-v-1.0f, -0.5f);
            float max = Mathf.Min(bi+v+1.0f, breathEntries.Length-0.5f);
            if (min < breatheIndex && max > breatheIndex)
                max -= 1.0f;
            int index = Mathf.RoundToInt(UnityEngine.Random.Range(min, max));
            index = Mathf.Clamp(index, 0, breathEntries.Length - 2);
            breatheIndex = index < breatheIndex ? index : index + 1;
            breatheEntry = breathEntries[breatheIndex];

            headAudio.CallAction("PlayNow", breatheEntry.audioClip);

            if (soundDebug.val)
            {
                SuperController.LogMessage("Min: " + Mathf.RoundToInt(min) + " Max: " + Mathf.RoundToInt(max) + " Playing: " + breatheEntry.name);
            }

            // reset timers
            soundDuration = breatheEntry.duration;
            soundDurationTimer = 0;

        }

        private void SetNewPenisTarget()
        {
            penisStart = penisTarget;
            lerpTimer = 0f;
            lerpTime = SetNewDurationTimer();
            if (penisIsPositionOne)
            {
                penisPosition = penisPositionTwoJSON.val;
                penisIsPositionOne = false;


            }
            else
            {
                penisPosition = penisPositionOneJSON.val;
                penisIsPositionOne = true;
            }
            // Add random
            penisPosition += UnityEngine.Random.Range(penisPosition - penisRangeJSON.val, penisPosition + penisRangeJSON.val);

        }
        private void SetNewHipTarget()
        {
            hipStart = hipTarget;
            lerpTimer = 0f;
            lerpTime = SetNewDurationTimer();
            if (hipIsPositionOne)
            {
                hipPosition = hipPositionTwoJSON.val;
                hipIsPositionOne = false;

            }
            else
            {
                hipPosition = hipPositionOneJSON.val;
                hipIsPositionOne = true;
            }
            // Add random
            hipPosition += UnityEngine.Random.Range(hipPosition - hipRangeJSON.val, hipPosition + hipRangeJSON.val);
        }

        private void SetNewGrindTarget()
        {
            grindStart = grindTarget;
            grindLerpTimer = 0f;
            grindLerpTime = lerpTime * grindMultiplierJSON.val;
//            grindTarget.x = UnityEngine.Random.Range(-grindJSON.val, grindJSON.val);
            grindTarget.x = 0;
            grindTarget.y = UnityEngine.Random.Range(-grindJSON.val, grindJSON.val);
            grindTarget.z = UnityEngine.Random.Range(-grindJSON.val, grindJSON.val);
        }

        private float SetNewDurationTimer()
        {
            if (durationRangeJSON.val == 0 || durationUpdateTimer > durationUpdateIntervalJSON.val) 
            {
                durationUpdateTimer = 0;
                float min = durationJSON.val - (durationRangeJSON.val);
                if (min < 0)
                {
                    min = 0.1f;
                }
                float max = durationJSON.val + (durationRangeJSON.val);
                return UnityEngine.Random.Range(min, max);
            }
            else
            {
                return lerpTime; // If it's not time to set a new duration, return the current duration
            }
        }

        private void StartSex()
        {
            penisPosition = -2.1f;
            lerpTime = durationJSON.val;
            penisStart = penisBase.transform.position;
            hipStart = hipControl.transform.position;
            penisResetPosition = penisBase.transform.position;
            hipResetPosition = hipControl.transform.position;
            hipResetRotation = hipControl.transform.rotation;
            isActive = true;
//            SetNewHipTarget();
//            SetNewPenisTarget();

            //UpdateHipRotation(hipRotationJSON.val);

            penisBase.currentPositionState = FreeControllerV3.PositionState.On;
            penisBase.currentRotationState = FreeControllerV3.RotationState.Off;
            penisMid.currentPositionState = FreeControllerV3.PositionState.Off;
            penisMid.currentRotationState = FreeControllerV3.RotationState.Off;
            penisTip.currentPositionState = FreeControllerV3.PositionState.Off;
            penisTip.currentRotationState = FreeControllerV3.RotationState.Off;

            pelvisControl.currentPositionState = FreeControllerV3.PositionState.On;
            pelvisControl.currentRotationState = FreeControllerV3.RotationState.On;
            hipControl.currentRotationState = FreeControllerV3.RotationState.On;
            hipControl.currentPositionState = FreeControllerV3.PositionState.On;
        }

        private void StopSex()
        {
            isActive = false;
            penisBase.transform.position = penisResetPosition;
            hipControl.transform.position = hipResetPosition;
            hipControl.transform.rotation = hipResetRotation;
            penisBase.currentPositionState = FreeControllerV3.PositionState.Off;
            penisBase.currentRotationState = FreeControllerV3.RotationState.Off;
            penisMid.currentPositionState = FreeControllerV3.PositionState.Off;
            penisMid.currentRotationState = FreeControllerV3.RotationState.Off;
            penisTip.currentPositionState = FreeControllerV3.PositionState.Off;
            penisTip.currentRotationState = FreeControllerV3.RotationState.Off;

            pelvisControl.currentPositionState = FreeControllerV3.PositionState.Off;
            pelvisControl.currentRotationState = FreeControllerV3.RotationState.Off;

            //hipControl.transform.rotation = Quaternion.AngleAxis(0, hipControl.transform.up);
            //hipControl.transform.rotation = Quaternion.AngleAxis(0, hipControl.transform.forward);
        }

        private void UpdateRotations()
        {
            penisAngleOffset.x = 0;
            penisAngleOffset.z = 0;
            penisAngleOffset.y = penisRotationJSON.val;

            if (useGen3RotationJSON.val)
            {
                gen1.transform.LookAt(deepVaginaTrigger.transform.position + penisAngleOffset);
                gen2.transform.LookAt(deepVaginaTrigger.transform.position + penisAngleOffset);
                gen3.transform.LookAt(deepVaginaTrigger.transform.position + penisAngleOffset);
            } else
            {
                penisBase.transform.LookAt(deepVaginaTrigger.transform.position + penisAngleOffset);
                //penisMid.transform.LookAt(deepVaginaTrigger.transform.position + penisAngleOffset);
            }

        }

		private struct BreathEntry
		{
		    public float duration;
			public string name;
			public NamedAudioClip audioClip;			
			
			public BreathEntry(string name, float duration)
			{
				this.name = name;
			    this.duration = duration;
				
				string filename = "./"+AUDIO_DIR+name;
				audioClip = URLAudioClipManager.singleton.GetClip(filename);
				if (audioClip == null)
				{
					URLAudioClipManager.singleton.QueueClip(SCENE_DIR+AUDIO_DIR+name);
					audioClip = URLAudioClipManager.singleton.GetClip(filename);
				}
			}
		}
        private void PitchChanged(float val)
        {
            // If the new value matches the random pitch we've generated, then no big deal. Otherwise it has been changed manually / by another plugin, so we need to update the base value
            if (val != pitchCurrent)
            {
                pitchBase = val;
            }

        }

        private void UpdateHipRotation(float val)
        {
            hipControl.transform.rotation = Quaternion.AngleAxis(val, hipControl.transform.right);
        }

        public void SetEasing(string aEasing)
        {
            foreach (var pair in Easing.easingChoices)
            {
                if (pair.Key == aEasing)
                {
                    easing = pair.Value;
                    break;
                }
            }
        }

        protected void SyncFemaleAtom()
        {
            try
            {
                hipControl = containingAtom.freeControllers.First(fc => fc.name == "hipControl");
                pelvisControl = containingAtom.freeControllers.First(fc => fc.name == "pelvisControl");
                labiaTrigger = containingAtom.rigidbodies.First(rb => rb.name == "LabiaTrigger");
                vaginaTrigger = containingAtom.rigidbodies.First(rb => rb.name == "VaginaTrigger");
                deepVaginaTrigger = containingAtom.rigidbodies.First(rb => rb.name == "DeepVaginaTrigger");
                deeperVaginaTrigger = containingAtom.rigidbodies.First(rb => rb.name == "DeeperVaginaTrigger");
                pelvis = containingAtom.rigidbodies.First(rb => rb.name == "pelvis");
                headAudio = containingAtom.GetStorableByID("HeadAudioSource");
                pitchJSON = containingAtom.GetStorableByID("HeadAudioSource").GetFloatJSONParam("pitch");
                pitchJSON.setJSONCallbackFunction = (x) => PitchChanged(x.val);
                pitchBase = pitchJSON.val;
                // gloss 
                //gloss = containingAtom.GetStorableByID("skin").GetFloatJSONParam("Gloss");

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        protected void PenisHardPhysics()
        {
            penisBase.RBHoldPositionSpring = 10000;
            penisBase.RBHoldRotationSpring = 1000;
            penisBase.RBHoldPositionDamper = 100;
            penisBase.RBHoldRotationDamper = 100;
            penisBase.RBHoldPositionMaxForce = 10000;
            penisBase.RBHoldRotationMaxForce = 1000;
            penisBase.jointRotationDriveSpring = 200;
            penisBase.jointRotationDriveDamper = 10;
            penisBase.jointRotationDriveMaxForce = 100;
            penisMid.RBHoldPositionSpring = 10000;
            penisMid.RBHoldRotationSpring = 1000;
            penisMid.RBHoldPositionDamper = 100;
            penisMid.RBHoldRotationDamper = 100;
            penisMid.RBHoldPositionMaxForce = 10000;
            penisMid.RBHoldRotationMaxForce = 1000;
            penisMid.jointRotationDriveSpring = 200;
            penisMid.jointRotationDriveDamper = 10;
            penisMid.jointRotationDriveMaxForce = 100;
            penisTip.RBHoldPositionSpring = 10000;
            penisTip.RBHoldRotationSpring = 1000;
            penisTip.RBHoldPositionDamper = 100;
            penisTip.RBHoldRotationDamper = 100;
            penisTip.RBHoldPositionMaxForce = 10000;
            penisTip.RBHoldRotationMaxForce = 1000;
            penisTip.jointRotationDriveSpring = 200;
            penisTip.jointRotationDriveDamper = 10;
            penisTip.jointRotationDriveMaxForce = 100;
        }
        protected void PenisDefaultPhysics()
        {
            penisBase.RBHoldPositionSpring = 2000;
            penisBase.RBHoldRotationSpring = 100;
            penisBase.RBHoldPositionDamper = 35;
            penisBase.RBHoldRotationDamper = 50;
            penisBase.RBHoldPositionMaxForce = 1000;
            penisBase.RBHoldRotationMaxForce = 1000;
            penisBase.jointRotationDriveSpring = 24;
            penisBase.jointRotationDriveDamper = 0.1f;
            penisBase.jointRotationDriveMaxForce = 10;
            penisMid.RBHoldPositionSpring = 10000;
            penisMid.RBHoldRotationSpring = 1000;
            penisMid.RBHoldPositionDamper = 100;
            penisMid.RBHoldRotationDamper = 100;
            penisMid.RBHoldPositionMaxForce = 10000;
            penisMid.RBHoldRotationMaxForce = 1000;
            penisMid.jointRotationDriveSpring = 200;
            penisMid.jointRotationDriveDamper = 10;
            penisMid.jointRotationDriveMaxForce = 100;
            penisTip.RBHoldPositionSpring = 10000;
            penisTip.RBHoldRotationSpring = 1000;
            penisTip.RBHoldPositionDamper = 100;
            penisTip.RBHoldRotationDamper = 100;
            penisTip.RBHoldPositionMaxForce = 10000;
            penisTip.RBHoldRotationMaxForce = 1000;
            penisTip.jointRotationDriveSpring = 200;
            penisTip.jointRotationDriveDamper = 10;
            penisTip.jointRotationDriveMaxForce = 100;
        }

        protected void FemaleHardPhysics()
        {
            hipControl.RBHoldPositionSpring = 10000;
            hipControl.RBHoldRotationSpring = 1000;
            hipControl.RBHoldPositionDamper = 100;
            hipControl.RBHoldRotationDamper = 100;
            hipControl.RBHoldPositionMaxForce = 10000;
            hipControl.RBHoldRotationMaxForce = 1000;
            hipControl.jointRotationDriveSpring = 200;
            hipControl.jointRotationDriveDamper = 10;
            hipControl.jointRotationDriveMaxForce = 100;
            pelvisControl.RBHoldPositionSpring = 10000;
            pelvisControl.RBHoldRotationSpring = 1000;
            pelvisControl.RBHoldPositionDamper = 100;
            pelvisControl.RBHoldRotationDamper = 100;
            pelvisControl.RBHoldPositionMaxForce = 10000;
            pelvisControl.RBHoldRotationMaxForce = 1000;
            pelvisControl.jointRotationDriveSpring = 200;
            pelvisControl.jointRotationDriveDamper = 10;
            pelvisControl.jointRotationDriveMaxForce = 100;

        }

        protected void FemaleDefaultPhysics()
        {
            hipControl.RBHoldPositionSpring = 2000;
            hipControl.RBHoldRotationSpring = 100;
            hipControl.RBHoldPositionDamper = 35;
            hipControl.RBHoldRotationDamper = 50;
            hipControl.RBHoldPositionMaxForce = 1000;
            hipControl.RBHoldRotationMaxForce = 1000;
            hipControl.jointRotationDriveSpring = 24;
            hipControl.jointRotationDriveDamper = 0.1f;
            hipControl.jointRotationDriveMaxForce = 10;
            pelvisControl.RBHoldPositionSpring = 2000;
            pelvisControl.RBHoldRotationSpring = 100;
            pelvisControl.RBHoldPositionDamper = 35;
            pelvisControl.RBHoldRotationDamper = 50;
            pelvisControl.RBHoldPositionMaxForce = 1000;
            pelvisControl.RBHoldRotationMaxForce = 1000;
            pelvisControl.jointRotationDriveSpring = 24;
            pelvisControl.jointRotationDriveDamper = 0.1f;
            pelvisControl.jointRotationDriveMaxForce = 10;

        }

        private void PenisShrink()
        {
            JSONStorable geometry = him.GetStorableByID("geometry");
            DAZCharacterSelector character = geometry as DAZCharacterSelector;
            GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;
            penisLength = morphControl.GetMorphByDisplayName("Penis Length");

            penisLengthVal = penisLength.morphValue;
            penisLength.morphValue = -3f;
        }

        private void PenisGrow()
        {
            penisLength.morphValue = penisLengthVal;
        }

        //private List<string> GetPeopleNamesFromScene()
        //{
        //    return SuperController.singleton.GetAtoms().Where(atom => atom.GetStorableByID("geometry") != null).Select(atom => atom.name).ToList();
        //}


        // OnDestroy is where you should put any cleanup
        // if you registered objects to supercontroller or atom, you should unregister them here
        void OnDestroy()
        {
        }
    }

}