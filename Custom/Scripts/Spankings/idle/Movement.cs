using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using SimpleJSON;

namespace IdleHelper
{

    public class MovementHelper : MVRScript
    {
        // Receiver 
        protected Rigidbody RB;
        protected JSONStorableStringChooser receiverChoiceJSON;
        protected List<string> receiverChoices;
        protected Dictionary<string, ForceReceiver> receiverNameToForceReceiver;
        public Vector3 currentTorque;
        public Vector3 currentForce;

        // Save / Load
        public static string pluginName = "Chest Movement";
//        public static string pluginVersion = "1.0";
        public static string saveExt = "movement";
        protected string _lastBrowseDir;

        // UI
        public static List<Movement> movements;
        public static List<Movement> enabledMovements;
        public static List<JSONStorableFloat> sliders;
        public static List<JSONStorableBool> bools;
        public static List<UIDynamicButton> buttons;
        public static List<JSONStorableStringChooser> popups;
        public static List<JSONStorableString> menuHeadings;
        public static List<UIDynamic> spacers;
        protected UIDynamicButton btn;
        protected JSONStorableBool loadReceiverToggle;
        protected JSONStorableStringChooser copyFromJSON;
        public static List<string> movementNames;
        public static Dictionary<string, Movement> movementNameToMovement;

        // Movements
        protected Movement tx; // Torque
        protected Movement ty;
        protected Movement tz;
        protected Movement fx; // Force
        protected Movement fy;
        protected Movement fz;

        public override void Init()
        {
            try
            {
                // Create easings list
                Easing.SetEasingChoices();
                // Create Lists
                enabledMovements = new List<Movement>();

                // UI Lists
                sliders = new List<JSONStorableFloat>();
                bools = new List<JSONStorableBool>();
                buttons = new List<UIDynamicButton>();
                popups = new List<JSONStorableStringChooser>();
                menuHeadings = new List<JSONStorableString>();
                spacers = new List<UIDynamic>();
                movementNames = new List<string>();
                movementNameToMovement = new Dictionary<string, Movement>();

                // Create movements
                tx = new Movement("Torque X");
                ty = new Movement("Torque Y");
                tz = new Movement("Torque Z");
                fx = new Movement("Force X");
                fy = new Movement("Force Y");
                fz = new Movement("Force Z");

                // Populate movements list and register all movement elements
                movements = new List<Movement>()
                {
                    tx,ty,tz,fx,fy,fz
                };
                foreach (var m in movements)
                {
                    RegisterMovement(m);
                    movementNames.Add(m.name.val);
                    movementNameToMovement.Add(m.name.val, m);
                }

                foreach (Movement m in movements)
                {
                    var mBools = m.GetStorableBools();
                    foreach(var mBool in mBools)
                    {
                        bools.Add(mBool);
                    }

                    var mFloats = m.GetStorableFloats();
                    foreach (var mFloat in mFloats)
                    {
                        sliders.Add(mFloat);
                    }
                    // Add the display sliders 
                    sliders.Add(m.displayCurrent);
                    sliders.Add(m.displayDelay);
                    sliders.Add(m.displayDuration);
                    sliders.Add(m.displayTarget);

                    var mStrings = m.GetStorableStringsChoosers();
                    foreach (var mString in mStrings)
                    {
                        popups.Add(mString);
                    }

                    menuHeadings.Add(m.displayName);
                }


                // Create preset directory
                _lastBrowseDir = CreateDirectory(GetPluginPath() + @"movement_presets\" );
                pluginLabelJSON.val = pluginName;

                // Static UI Elements 

                receiverChoices = new List<string>();
                receiverNameToForceReceiver = new Dictionary<string, ForceReceiver>();
                foreach (ForceReceiver fr in containingAtom.forceReceivers)
                {
                    receiverChoices.Add(fr.name);
                    receiverNameToForceReceiver.Add(fr.name, fr);
                }

                receiverChoiceJSON = new JSONStorableStringChooser("receiver", receiverChoices, null, "Receiver", SyncReceiver);
                 receiverChoiceJSON.storeType = JSONStorableParam.StoreType.Full;

                receiverChoiceJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterStringChooser(receiverChoiceJSON);
                UIDynamicPopup dp = CreateScrollablePopup(receiverChoiceJSON);
                dp.popupPanelHeight = 1100f;
                dp.popup.alwaysOpen = false;

                btn = CreateButton("Main Menu");
                btn.button.onClick.AddListener(() => { BuildMainMenu(); });
                btn.buttonColor = Color.green;

                btn = CreateButton("Torque X");
                btn.buttonColor = Color.cyan;
                btn.button.onClick.AddListener(() => { BuildMovementMenu(tx); });
                btn = CreateButton("Torque Y");
                btn.buttonColor = Color.cyan;
                btn.button.onClick.AddListener(() => { BuildMovementMenu(ty); });
                btn = CreateButton("Torque Z");
                btn.buttonColor = Color.cyan;
                btn.button.onClick.AddListener(() => { BuildMovementMenu(tz); });
                btn = CreateButton("Force X");
                btn.buttonColor = Color.yellow;
                btn.button.onClick.AddListener(() => { BuildMovementMenu(fx); });
                btn = CreateButton("Force Y");
                btn.buttonColor = Color.yellow;
                btn.button.onClick.AddListener(() => { BuildMovementMenu(fy); });
                btn = CreateButton("Force Z");
                btn.buttonColor = Color.yellow;
                btn.button.onClick.AddListener(() => { BuildMovementMenu(fz); });

                // Load Receiver option toggle 
                loadReceiverToggle = new JSONStorableBool("Load Receiver", true);
                bools.Add(loadReceiverToggle);


                // Initial Setup
                BuildMainMenu();

                // Initial setup for PhysisLife
                receiverChoiceJSON.val = ("chest");
                SyncReceiver("chest");
                tx.enabledJSON.val = true;
                ty.enabledJSON.val = true;
                tz.enabledJSON.val = true;
                foreach (var m in enabledMovements)
                {
                    m.durationJSON.val = 2;
                    m.durationRangeJSON.val = 0.5f;
                    m.targetRangeJSON.val = 30f;
                    m.easingJSON.val = "Quadratic InOut";
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        protected void Start()
        {
        }


        protected void Update()
        {
        }


        // FixedUpdate is called with each physics simulation frame by Unity
        void FixedUpdate()
        {
            try
            {
                if (RB && (!SuperController.singleton || !SuperController.singleton.freezeAnimation))
                {
                    foreach (Movement m in enabledMovements)
                    {
                        m.Update(Time.fixedDeltaTime);
                    }

                    currentTorque.x = tx.current;
                    currentTorque.y = ty.current;
                    currentTorque.z = tz.current;
                    RB.AddRelativeTorque(currentTorque, ForceMode.Force);

                    currentForce.x = fx.current;
                    currentForce.y = fy.current;
                    currentForce.z = fz.current;
                    RB.AddRelativeForce(currentForce, ForceMode.Force);
                }

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // Receiver choice
        protected void SyncReceiver(string receiver)
        {
            if (receiver != null)
            {
                ForceReceiver fr;
                if (receiverNameToForceReceiver.TryGetValue(receiver, out fr))
                {
                    RB = fr.GetComponent<Rigidbody>();
                }
                else
                {
                    RB = null;
                }
            }
            else
            {
                RB = null;
            }
        }

        public void BuildMainMenu()
        {
            ResetMenu();

            var spacer = CreateSpacer(true);
            spacers.Add(spacer);

            btn = CreateButton("Load Preset",true);
            btn.button.onClick.AddListener(() =>
            {
                SuperController.singleton.NormalizeMediaPath(_lastBrowseDir); 
                SuperController.singleton.GetMediaPathDialog(LoadPreset, saveExt);
            });
            btn.buttonColor = Color.blue;
            btn.textColor = Color.white;
            buttons.Add(btn);

            CreateToggle(loadReceiverToggle, true);

            btn = CreateButton("Save Preset", true);
            btn.button.onClick.AddListener(() =>
            {
                SuperController.singleton.NormalizeMediaPath(_lastBrowseDir); 
                SuperController.singleton.GetMediaPathDialog(SavePreset, saveExt);

                // Update the browser to be a Save browser
                uFileBrowser.FileBrowser browser = SuperController.singleton.mediaFileBrowserUI;
                browser.SetTextEntry(true);
                browser.fileEntryField.text = String.Format("{0}.{1}", ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString(), saveExt);
                browser.ActivateFileNameField();
            });
            btn.buttonColor = Color.magenta;
            btn.textColor = Color.white;
            buttons.Add(btn);

            btn = CreateButton("Disable all movements", true);
            btn.buttonColor = Color.white;
            btn.button.onClick.AddListener(() => { DisableAllMovements(); });
            buttons.Add(btn);

            btn = CreateButton("Zero all movements", true);
            btn.buttonColor = Color.red;
            btn.button.onClick.AddListener(() => { ZeroAllMovements(); });
            btn.textColor = Color.white;
            buttons.Add(btn);

            btn = CreateButton("Reset all timers", true);
            btn.buttonColor = Color.green;
            btn.button.onClick.AddListener(() => { ResetAllTimers(); });
            btn.textColor = Color.white;
            buttons.Add(btn);


        }

        public void BuildMovementMenu(Movement m)
        {
            ResetMenu();

            var heading = CreateTextField(m.displayName);
            heading.height = 20f;
            heading.backgroundColor = Color.blue;
            heading.textColor = Color.white;

            // Have to create the copyFrom string chooser here because the callback references the currently active movement
           copyFromJSON = new JSONStorableStringChooser("copyFrom", movementNames, null, "Copy Values From", x =>
           {
                var src = movementNameToMovement.First(k => k.Key == x.val).Value;
               m.CopyFrom(src);
           });
            popups.Add(copyFromJSON);
            CreateScrollablePopup(copyFromJSON);

            CreateToggle(m.syncEnabledJSON).label = "Enable Timing Sync";
            CreateScrollablePopup(m.syncTargetJSON);

            btn = CreateButton("Zero all values");
            btn.buttonColor = Color.red;
            btn.button.onClick.AddListener(() => { m.ZeroAllValues(); });
            btn.textColor = Color.white;
            buttons.Add(btn);

            var slider = CreateSlider(m.displayCurrent);
            slider.defaultButtonEnabled = false;
            slider.quickButtonsEnabled = false;

            slider = CreateSlider(m.displayTarget);
            slider.defaultButtonEnabled = false;
            slider.quickButtonsEnabled = false;

            slider = CreateSlider(m.displayDuration);
            slider.defaultButtonEnabled = false;
            slider.quickButtonsEnabled = false;

            slider = CreateSlider(m.displayDelay);
            slider.defaultButtonEnabled = false;
            slider.quickButtonsEnabled = false;

            CreateToggle(m.enabledJSON, true);
            CreateScrollablePopup(m.easingJSON, true).popupPanelHeight = 1100f; // We have to put the easing menu at the top otherwise most of the dropdown is hidden

            CreateSlider(m.durationJSON, true).label = "Duration";
            CreateSlider(m.targetJSON, true).label = "Target";
            CreateSlider(m.targetRangeJSON, true).label = "Range";
            CreateSlider(m.durationRangeJSON, true).label = "Duration Range";
            CreateSlider(m.durationUpdateIntervalJSON, true).label = "Duration Update Interval";
            CreateSlider(m.delayJSON, true).label = "Delay";
            CreateSlider(m.delayRangeJSON, true).label = "Delay Range";

            CreateToggle(m.targetTwoEnabledJSON, true).label = "Target Two Enabled";
            CreateSlider(m.targetTwoJSON, true).label = "Target Two";
            CreateSlider(m.targetTwoRangeJSON, true).label = "Target Two Range";
        }


        public void ResetMenu() // Removes all UI elements (except the static ones)
        {
            foreach (JSONStorableFloat slider in sliders)
            {
                RemoveSlider(slider);
            }
            foreach (JSONStorableBool myBool in bools)
            {
                RemoveToggle(myBool);
            }
            foreach (UIDynamicButton btn in buttons)
            {
                RemoveButton(btn);
            }
            foreach (JSONStorableStringChooser popup in popups)
            {
                RemovePopup(popup);
            }
            foreach (JSONStorableString heading in menuHeadings)
            {
                RemoveTextField(heading);
            }
            foreach (UIDynamic spacer in spacers)
            {
                RemoveSpacer(spacer);
            }
        }

        public static void UpdateEnabledList()
        {
            enabledMovements.Clear();
            foreach (Movement m in movements)
            {
                if (m.enabledJSON.val)
                {
                    enabledMovements.Add(m);
                }
                else
                {
                    m.current = 0f; // Make sure a disabled movement doesn't get stuck in the middle somewhere
                }
            }

        }

        public void DisableAllMovements()
        {
            foreach (var movement in movements)
            {
                movement.Disable();
            }
        }

        public void ZeroAllMovements()
        {
            foreach (var movement in movements)
            {
                movement.ZeroAllValues();
            }
        }

        public void ResetAllTimers()
        {
            foreach (var movement in movements)
            {
                    movement.ResetTimers();
            }
        }

        void SavePreset(string aPath)
        {
            if (String.IsNullOrEmpty(aPath))
            {
                return;
            }
            _lastBrowseDir = aPath.Substring(0, aPath.LastIndexOfAny(new char[] { '/', '\\' })) + @"\";

            if ( !aPath.ToLower().EndsWith(saveExt.ToLower()))
            {
                aPath += "." + saveExt;
            }
            JSONClass saveJson = new JSONClass();
            //saveJson["savedBy"] = pluginName + pluginVersion;
            saveJson["receiver"] = receiverChoiceJSON.val;
            saveJson["movements"] = new JSONArray();
            saveJson["positions"] = new JSONArray();
            foreach (var m in movements)
            {
                JSONClass movementNode = new JSONClass();
                foreach (var storable in m.GetStorableBools())
                {
                    storable.StoreJSON(movementNode);
                }
                foreach (var storable in m.GetStorableFloats())
                {
                    storable.StoreJSON(movementNode);
                }

                foreach (var storable in m.GetStorableStringsChoosers())
                {
                    storable.StoreJSON(movementNode);
                }
                if (movementNode.Count > 0)
                {
                    movementNode["id"] = m.name.val;
                    saveJson["movements"].Add(movementNode);
                }
            }
            this.SaveJSON(saveJson, aPath);
        }

        void LoadPreset(string aPath)
        {
            // Reset current movement
            DisableAllMovements();
            ZeroAllMovements();

            if (String.IsNullOrEmpty(aPath))
            {
                return;
            }
            _lastBrowseDir = aPath.Substring(0, aPath.LastIndexOfAny(new char[] { '/', '\\' })) + @"\";
            var aJson = this.LoadJSON(aPath);

            if (loadReceiverToggle.val)
            {
                SyncReceiver(aJson["receiver"].Value);
                receiverChoiceJSON.val = aJson["receiver"].Value;
            }
            foreach (JSONNode movementJSON in aJson["movements"].AsArray)
            {
                string movementName = movementJSON["id"].Value;
                Movement movement = GetMovementByName(movementName);
                if (movement != null)
                {
                    movement.RestoreFromJson(movementJSON);
                }
            }

        }
        Movement GetMovementByName(string aName )
        {
            foreach( var movement in movements )
            {
                if( movement.name.val == aName )
                {
                    return movement;
                }
            }
            return null;
        }

        string GetPluginPath()
        {
            SuperController.singleton.currentSaveDir = SuperController.singleton.currentLoadDir;
            string pluginId = this.storeId.Split('_')[0];
            string pathToScriptFile = this.manager.GetJSON(true, true)["plugins"][pluginId].Value;
            string pathToScriptFolder = pathToScriptFile.Substring(0, pathToScriptFile.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            pathToScriptFolder = pathToScriptFolder.Replace('/', '\\');
            return pathToScriptFolder;
        }

        string CreateDirectory(string aPath)
        {
            JSONNode node = new JSONNode();
            if (!(aPath.EndsWith("/") || aPath.EndsWith(@"\")))
            {
                aPath += @"\";
            }

            try
            {
                node.SaveToFile(aPath);
            }
            catch (Exception e)
            {
            }
            return aPath;
        }

        void RegisterMovement( Movement m )
        {
            var boollist = m.GetStorableBools();
            foreach( var storable in boollist )
            {
                RegisterBool(storable);
            }
            var floatList = m.GetStorableFloats();
            foreach( var storable in floatList )
            {
                RegisterFloat(storable);
            }
            var stringList = m.GetStorableStringsChoosers();
            foreach( var storable in stringList )
            {
                RegisterStringChooser(storable);
            }
        }
    }

    public class Movement
    {
        // Storables
        public JSONStorableString name;
        public JSONStorableBool enabledJSON;
        public JSONStorableBool targetTwoEnabledJSON;
        public JSONStorableBool syncEnabledJSON;
        public JSONStorableFloat durationJSON;
        public JSONStorableFloat durationRangeJSON;
        public JSONStorableFloat durationUpdateIntervalJSON;
        public JSONStorableFloat delayJSON;
        public JSONStorableFloat delayRangeJSON;
        public JSONStorableFloat targetJSON;
        public JSONStorableFloat targetRangeJSON;
        public JSONStorableFloat targetTwoJSON;
        public JSONStorableFloat targetTwoRangeJSON;
        public JSONStorableStringChooser easingJSON;
        public JSONStorableStringChooser syncTargetJSON;


        // UI Only - not actually saved
        public JSONStorableString displayName; // Just name with a linebreak added
        public JSONStorableFloat displayCurrent;
        public JSONStorableFloat displayTarget;
        public JSONStorableFloat displayDuration;
        public JSONStorableFloat displayDelay;

        // Dynamic
        private float start;
        private float target;
        private float perc; // percent of lerp complete
        public float lerpTime; // duration
        public float delay;
        public float current; // value eventually used by receiver
        public Func<float, float> easing;
        private bool isTarget = true; // used to switch to target two if enabled
        private Movement syncTarget; // movement to sync timings with
        // Timers
        private float lerpTimer;
        private float delayTimer;
        private float durationUpdateTimer;


        public Movement(string aName)
        {
            name = new JSONStorableString("name",aName);

            // UI
            displayName = new JSONStorableString("menuHeading", "\n" + name.val);
            displayCurrent = new JSONStorableFloat("Current Value", 0f, -1000f, 1000f, false, false);
            displayTarget = new JSONStorableFloat("Current Target", 0f, -1000f, 1000f, false, false);
            displayDuration = new JSONStorableFloat("Current Duration", 0f, -1000f, 1000f, false, false);
            displayDelay = new JSONStorableFloat("Current Delay", 0f, -1000f, 1000f, false, false);

            // Bools
            enabledJSON = new JSONStorableBool(name.val + " Enabled", false, OnEnabledChanged);
            enabledJSON.storeType = JSONStorableParam.StoreType.Full;
            targetTwoEnabledJSON = new JSONStorableBool(name.val + " Target Two Enabled", false);
            targetTwoEnabledJSON.storeType = JSONStorableParam.StoreType.Full;
            syncEnabledJSON = new JSONStorableBool(name.val + " Enable Timing Sync", false);
            syncEnabledJSON.storeType = JSONStorableParam.StoreType.Full;
            // Floats
            durationJSON = new JSONStorableFloat(name.val + " Duration", 0f, 0f, 100f, false);
            durationJSON.storeType = JSONStorableParam.StoreType.Full;
            durationRangeJSON = new JSONStorableFloat(name.val + " Duration Range", 0f, 0f, 100f, false);
            durationRangeJSON.storeType = JSONStorableParam.StoreType.Full;
            durationUpdateIntervalJSON = new JSONStorableFloat(name.val + " Duration Update Interval", 0f, 0f, 100f, false);
            durationUpdateIntervalJSON.storeType = JSONStorableParam.StoreType.Full;
            delayJSON = new JSONStorableFloat(name.val + " Delay", 0f, 0f, 100f, false);
            delayJSON.storeType = JSONStorableParam.StoreType.Full;
            delayRangeJSON = new JSONStorableFloat(name.val + " Delay Range", 0f, 0f, 100f, false);
            delayRangeJSON.storeType = JSONStorableParam.StoreType.Full;
            targetJSON = new JSONStorableFloat(name.val + " Target", 0f, -500f, 500f, false);
            targetJSON.storeType = JSONStorableParam.StoreType.Full;
            targetRangeJSON = new JSONStorableFloat(name.val + " Range", 0f, 0f, 1000f, false);
            targetRangeJSON.storeType = JSONStorableParam.StoreType.Full;
            targetTwoJSON = new JSONStorableFloat(name.val + " Target Two", 0f, -500f, 500f, false);
            targetTwoJSON.storeType = JSONStorableParam.StoreType.Full;
            targetTwoRangeJSON = new JSONStorableFloat(name.val + " Target Two Range", 0f, 0f, 1000f, false);
            targetTwoRangeJSON.storeType = JSONStorableParam.StoreType.Full;

            // Strings
            easingJSON = new JSONStorableStringChooser(name.val + " Easing Choice", Easing.easingChoicesList, "Linear", "Easing", SetEasing);
            easingJSON.storeType = JSONStorableParam.StoreType.Full;
            syncTargetJSON = new JSONStorableStringChooser(name.val + " Sync Target", MovementHelper.movementNames, "None", "Sync With", SetSyncTarget);
            syncTargetJSON.storeType = JSONStorableParam.StoreType.Full;

            // Set default easing
            SetEasing("Linear");
        }


        public void Update(float deltaTime)
        {
            // If sync is enabled, update values to match sync target - we only need duration and delay
            if (syncEnabledJSON.val && syncTarget != null)
            {
                lerpTime = syncTarget.lerpTime;
                delay = syncTarget.delay;
            }
            // Update timers
            lerpTimer += deltaTime;
            delayTimer += deltaTime;
            durationUpdateTimer += deltaTime;
            if (lerpTimer > lerpTime)
            {
                lerpTimer = lerpTime;
            }
            // If duration is set to 0 we just complete the movement (useful for positioning)
            if (lerpTime != 0)
            {
                perc = lerpTimer / lerpTime;
                // add easing
                perc = easing(perc);
            }
            else
            {
                perc = 1;
            }

            // Update current position
            current = Mathf.Lerp(start, target, perc);

            // Set new active position after each movement is complete and delay timer is reached
            if (current == target && delayTimer > delay)
            {
                start = target;
                lerpTimer = 0f;
                delayTimer = 0f;
                target = SetNewTarget();
                lerpTime = SetNewDurationTimer();
                delay = SetNewDelay();
            }
            // Update UI
            displayTarget.val = target;
            displayCurrent.val = current;
            displayDuration.val = lerpTime;
            displayDelay.val = delay;
        }

        private float SetNewTarget()
        {
            // if duration is 0, just return position value, otherwise we get stuttering as it generates a new random target every frame
            if (lerpTime == 0)
            {
                return targetJSON.val;
            }

            float min;
            float max;
            // if we're currently at first target and second target is active, switch to second target
            if (isTarget && targetTwoEnabledJSON.val)
            {
                min = targetTwoJSON.val - (targetTwoRangeJSON.val);
                max = targetTwoJSON.val + (targetTwoRangeJSON.val);
                isTarget = false;
            }
            else
            {
                min = targetJSON.val - (targetRangeJSON.val);
                max = targetJSON.val + (targetRangeJSON.val);
                isTarget = true;
            }
            return UnityEngine.Random.Range(min, max);
        }

        private float SetNewDurationTimer()
        {
            if (durationUpdateTimer > durationUpdateIntervalJSON.val)
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

        private float SetNewDelay()
        {
            if (delayRangeJSON.val == 0)
            {
                return delayJSON.val;
            }
            else
            {
                float min = delayJSON.val - (delayRangeJSON.val);
                if (min < 0)
                {
                    min = 0.1f;
                }
                float max = delayJSON.val + (delayRangeJSON.val);
                return UnityEngine.Random.Range(min, max);
            }
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

        public void SetSyncTarget(string aMovementName)
        {
            syncTarget = MovementHelper.movementNameToMovement.First(k => k.Key == aMovementName).Value;
        }

        private void OnEnabledChanged(bool enabled)
        {
            if (!enabled)
            {
                ResetTimers();
            }
            MovementHelper.UpdateEnabledList();
        }

        public void ZeroAllValues()
        {
            foreach (var f in GetStorableFloats())
            {
                f.val = 0;
            }

            syncEnabledJSON.val = false;
            targetTwoEnabledJSON.val = false;
            easingJSON.val = "Linear";
            target = 0f;
            current = 0f;
            ResetTimers();
        }

        public void ResetTimers()
        {
            lerpTimer = 0f;
            durationUpdateTimer = 0f;
            delayTimer = 0f;
        }

        public void Disable()
        {
            enabledJSON.val = false;
        }

        public void CopyFrom(Movement src)
        {
            // Would be nice to use GetStorables for this...
            enabledJSON.val = src.enabledJSON.val;
            durationJSON.val = src.durationJSON.val;
            durationRangeJSON.val = src.durationRangeJSON.val;
            durationUpdateIntervalJSON.val = src.durationUpdateIntervalJSON.val;
            targetJSON.val = src.targetJSON.val;
            targetRangeJSON.val = src.targetRangeJSON.val;
            delayJSON.val = src.delayJSON.val;
            delayRangeJSON.val = src.delayRangeJSON.val;
            easingJSON.val = src.easingJSON.val;
            targetTwoEnabledJSON.val = src.targetTwoEnabledJSON.val;
            targetTwoJSON.val = src.targetTwoJSON.val;
            targetTwoRangeJSON.val = src.targetTwoRangeJSON.val;
            syncEnabledJSON.val = src.syncEnabledJSON.val;
            syncTarget = src.syncTarget;
        }

        public List<JSONStorableBool> GetStorableBools()
        {
            var storables = new List<JSONStorableBool>();
            storables.Add(enabledJSON);
            storables.Add(targetTwoEnabledJSON);
            storables.Add(syncEnabledJSON);
            return storables;
        }
        public List<JSONStorableFloat> GetStorableFloats()
        {
            var storables = new List<JSONStorableFloat>();
            storables.Add(durationJSON);
            storables.Add(durationRangeJSON);
            storables.Add(durationUpdateIntervalJSON);
            storables.Add(delayJSON);
            storables.Add(delayRangeJSON);
            storables.Add(targetJSON);
            storables.Add(targetRangeJSON);
            storables.Add(targetTwoJSON);
            storables.Add(targetTwoRangeJSON);
            return storables;
        }
        public List<JSONStorableStringChooser> GetStorableStringsChoosers()
        {
            var storables = new List<JSONStorableStringChooser>();
            storables.Add(easingJSON);
            storables.Add(syncTargetJSON);
            return storables;
        }

        public void RestoreFromJson(JSONNode aJson)
        {
            foreach (var storable in GetStorableBools())
            {
                if (aJson[storable.name] != null)
                {
                    storable.val = aJson[storable.name].AsBool;
                }
            }
            foreach (var storable in GetStorableFloats())
            {
                if (aJson[storable.name] != null)
                {
                    storable.val = aJson[storable.name].AsFloat;
                }
            }
            foreach (var storable in GetStorableStringsChoosers())
            {
                if (aJson[storable.name] != null)
                {
                    storable.val = aJson[storable.name];
                }
            }
        }
    }
}

