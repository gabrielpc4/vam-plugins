using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace PA4148415 {
    public class RealGazeLooker : MVRScript {
        //Script by pornaway4148415
        //Using a few lines of code from ShortRecognition, some code based on code by VeeRifter and MacGruber
        //Also including modifications by Almadiel for compatability with Compliance
        /*Changelog 1.9
         * Rewrite to improve performance and clean up code
         * Users can define the maximum turn range
         * Changed the way target selection works
         * Added bias to target selection, currently only factors in closeness to the character head
         * Set up some of the groundwork for EmotionBrain and BodyAutonomy communication
        */
        /*TODO:
         * Use LookFor from EmotionBrain to influence bias
         * Use touch input from BodyAutonomy to influence bias
        */

        #region Structs
        //a RealGaze target, containing all necessary information
        public struct Target {
            //The purpose of this struct is to make it easy for me to pass targets around in this script without losing important information about them
            //It also allows me to store all targets without having to grab their information every time the target changes
            public float relFreq;
            public float adjFreq;
            public float mirrPref;
            public float pupilScale;
            public float minDis;
            public float maxDis;
            public HSVColor visualA;
            public HSVColor visualB;
            public string atomName;
            public Rigidbody receiver;
            public float playerZOffset;
            public bool direct;
            public bool mirror;
            public List<List<Vector3>> mirrPos;
            public Target(string playerSuffix, JSONStorable JSONinput, Atom atom, float off) {
                relFreq = (float)Math.Round(JSONinput.GetFloatParamValue(String.Concat("Relative frequency", playerSuffix)), 2);
                adjFreq = relFreq;
                mirrPref = JSONinput.GetFloatParamValue(String.Concat("Mirror preference", playerSuffix));
                pupilScale = JSONinput.GetFloatParamValue(String.Concat("Pupil size effect", playerSuffix));
                minDis = JSONinput.GetFloatParamValue(String.Concat("\"Greater than\" distance", playerSuffix));
                maxDis = JSONinput.GetFloatParamValue(String.Concat("\"Less than\" distance", playerSuffix));
                visualA = JSONinput.GetColorParamValue(String.Concat("attributesA", playerSuffix));
                visualB = JSONinput.GetColorParamValue(String.Concat("attributesB", playerSuffix));
                atomName = atom.uid;
                if (atom.uid == "[CameraRig]") {
                    receiver = atom.rigidbodies.First(rb => rb.name == "CenterEye");
                } else {
                    receiver = atom.rigidbodies.First(rb => rb.name == JSONinput.GetStringChooserParamValue("receiver"));
                }
                playerZOffset = off;
                direct = false;
                mirror = false;
                mirrPos = new List<List<Vector3>>();
            }
        }

        //a script search result. could be used to search for any receiver, but I'm using it exclusively for script searching
        public struct ScriptSearchResult {
            public bool found;
            public int count;
            public List<JSONStorable> scripts;
            public ScriptSearchResult(bool f, int c, List<JSONStorable> s) {
                found = f;
                count = c;
                scripts = new List<JSONStorable>();
                scripts.AddRange(s);
            }
        }
        #endregion


        #region Randomized eye positioning stuff
        //Makes a number negative with input value chance
        protected float RandNeg(float num, float chance) {
            if (UnityEngine.Random.value < chance) {
                num = num * -1;
            }
            return num;
        }

        //returns only positive integers
        public float posOnly(float input) {
            if (input > 0) {
                return input;
            } else {
                return 0;
            }
        }

        //Updates the saccade offset
        protected void UpdateSaccade(float off) {
            //normalizing the rotation of the containingHead times each of these unit vectors gives me the directions I need to make sure the offset is applied correctly
            Vector3 headSideways = Vector3.Normalize(ContainingHead.rotation * Vector3.right);
            Vector3 headVertical = Vector3.Normalize(ContainingHead.rotation * Vector3.up);
            Vector3 headForward = Vector3.Normalize(ContainingHead.rotation * Vector3.forward);
            //this dictionary is the way of picking a saccade direction based on real life statistics for saccade directions
            Dictionary<float, Vector3> saccades = new Dictionary<float, Vector3>(); //these are all vector3s for simplicity; each has an x, y, and z value which is all I need
            #region Saccade directions
            saccades.Add(7.50f, Vector3.zero); //actual direct. Vector3.zero is shorthand for new Vector3(0, 0, 0)
            saccades.Add(17.50f, new Vector3(RandNeg(UnityEngine.Random.Range(0, MinSRange.val), 0.5f), RandNeg(UnityEngine.Random.Range(0, MinSRange.val), 0.5f), 0)); //slightly non-direct
            //everything except for actual direct and slightly non-direct are pulled from actual data. I just needed to make sure the saccade could sometimes end up looking directly, or almost directly, at the player
            saccades.Add(20.38f, new Vector3(RandNeg(UnityEngine.Random.Range(0, MinSRange.val), 0.5f), -1 * UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), 0)); //down
            saccades.Add(17.69f, new Vector3(RandNeg(UnityEngine.Random.Range(0, MinSRange.val), 0.5f), UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), 0)); //up
            saccades.Add(16.80f, new Vector3(-1 * UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), RandNeg(UnityEngine.Random.Range(0, MinSRange.val), 0.5f), 0)); //left
            saccades.Add(15.54f, new Vector3(UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), RandNeg(UnityEngine.Random.Range(0, MinSRange.val), 0.5f), 0)); //right
            saccades.Add(7.89f, new Vector3(-1 * UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), -1 * UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), 0)); //down left
            saccades.Add(7.79f, new Vector3(UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), -1 * UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), 0)); //down right
            saccades.Add(7.45f, new Vector3(-1 * UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), 0)); //up left
            saccades.Add(6.46f, new Vector3(UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), UnityEngine.Random.Range(MinSRange.val, MaxSRange.val), 0)); //up right
            #endregion
            #region Random selector
            float selected = UnityEngine.Random.Range(0.0f, 125.0f);
            float running = 0;
            Vector3 active = Vector3.zero;
            if (doSaccades.val) {
                foreach (KeyValuePair<float, Vector3> pair in saccades) {
                    if (running + pair.Key >= selected) {
                        active = pair.Value;
                        break;
                    } else {
                        running += pair.Key;
                    }
                }
            }
            #endregion
            SaccadeOffset = (headSideways * active.x) + (headVertical * active.y) + (headForward * (active.z + off));
        }

        //Updates the look away position
        protected void UpdateLookAway() {
            //normalizing the rotation of the containingHead times each of these unit vectors gives me the directions I need to make sure the offset is applied correctly
            Vector3 headSideways = Vector3.Normalize(ContainingHead.rotation * Vector3.right);
            Vector3 headVertical = Vector3.Normalize(ContainingHead.rotation * Vector3.up);
            Vector3 headForward = Vector3.Normalize(ContainingHead.rotation * Vector3.forward);
            float LookAwayS = offsetS.val + RandNeg(UnityEngine.Random.Range(MinLRange.val, MaxLRange.val), 0.5f) * 2f;
            float LookAwayV = RandNeg(UnityEngine.Random.Range(MinLRange.val, MaxLRange.val), 0.5f) * 0.8f;
            float LookAwayF = offsetF.val + RandNeg(UnityEngine.Random.Range(MinLRange.val, MaxLRange.val), 0.5f) * 2f;
            LookAwayPos = (headSideways * LookAwayS) + (headVertical * LookAwayV) + (headForward * LookAwayF); //because the vectors were normalized(set to 1) they can simply be multiplied by the values to get the correct magnitude for each part of the vector. added, they create a 3D vector
        }
        #endregion

        #region Keystring stuff
        //adds the currently selected key to the list
        protected void AddKey() {
            lookerKey.val = (float)Math.Round(lookerKey.val, 2);
            if (!currentKeys.Contains(lookerKey.val)) {
                currentKeys.Add(lookerKey.val);
            }
            allKeys.val = KeyStringMaker(currentKeys, "");
            keysLabel.val = "Current keys: " + KeyStringMaker(currentKeys, " ");
            CheckForTargets();
            UpdateTarget(true);
        }

        //removes the currently selected key from the list
        protected void RemoveKey() {
            //I could've/likely should've used list.Remove() but I was suspicious and wrote this instead
            lookerKey.val = (float)Math.Round(lookerKey.val, 2);
            List<float> tempKeys = new List<float>();
            foreach (float key in currentKeys) {
                if (key != lookerKey.val) {
                    tempKeys.Add(key);
                }
            }
            currentKeys.Clear();
            currentKeys.AddRange(tempKeys);
            allKeys.val = KeyStringMaker(currentKeys, "");
            keysLabel.val = "Current keys: " + KeyStringMaker(currentKeys, " ");
            CheckForTargets();
            UpdateTarget(true);
        }

        //makes the key string so it can be saved in JSON
        protected string KeyStringMaker(List<float> keys, string extraSpace) {
            string build = "";
            foreach (float key in keys) {
                build += key.ToString() + "," + extraSpace;
            }
            return build;
        }

        //parses and loads the key string
        protected void KeyStringLoader(string str) {
            if (str.Length < 1) {
                return;
            }
            currentKeys.Clear();
            string[] keys = str.Split(',');
            foreach (string key in keys) {
                if (key.Length < 1) {
                    continue;
                }
                float toFloat = (float)Convert.ToDouble(key);
                currentKeys.Add(toFloat);
            }
            keysLabel.val = "Current keys: " + KeyStringMaker(currentKeys, " ");
        }
        #endregion

        #region Unity functions
        //Plugin initialization stuff
        public override void Init() {
            try {
                if (containingAtom.type != "Person") {
                    SuperController.LogError($"Please add this plugin to the Person Atom whose eyes you want to animate, not '{containingAtom.type}'");
                    return;
                }
                pluginLabelJSON.val = "RealGaze Looker 1.9";
                #region Game object setting
                EyeTarget = containingAtom.rigidbodies.First(rb => rb.name == "eyeTargetControl");
                ContainingHead = containingAtom.rigidbodies.First(rb => rb.name == "headControl");
                head = containingAtom.GetStorableByID("headControl").transform;
                eyes = containingAtom.GetStorableByID("Eyes");
                eyelids = containingAtom.GetStorableByID("EyelidControl");
                CharHead = containingAtom.GetStorableByID("headControl") as FreeControllerV3;
                CharChest = containingAtom.GetStorableByID("chestControl") as FreeControllerV3;
                CharPelv = containingAtom.GetStorableByID("pelvisControl") as FreeControllerV3;
                #endregion
                #region DAZMorph stuff
                character = containingAtom.GetStorableByID("geometry") as DAZCharacterSelector;
                eyelidControl = eyelids as DAZMeshEyelidControl;
                morphControl = character.morphsControlUI;
                morphPupil = morphControl.GetMorphByDisplayName("Pupils Dialate");
                //there seems to be a spelling error. female characters have "Pupils Dialate" while male characters have "Pupils Dilate"
                if (morphPupil == null) {  //therefore, if it tries to get Dialate and it fails, resulting null, it falls back on Dilate 
                    morphPupil = morphControl.GetMorphByDisplayName("Pupils Dilate");
                }
                morphVal = new JSONStorableFloat("pupilMorphVal", 0f, -1f, 1f);
                morphVal.val = morphPupil.morphValue; //by having the if null fallback, male characters work with the plugin
                RegisterFloat(morphVal);
                startingVal = morphVal.val;
                targetVal = startingVal;
                #endregion
                lookattarg = true;
                suppressWarnings = true;
                totalChance = 0;
                currentKey = -1f;

                #region Custom UI
                lookerKey = new JSONStorableFloat("Looker key", 0.0f, -10.0f, 10.0f, true, true);
                RegisterFloat(lookerKey);
                CreateSlider(lookerKey, false);

                allKeys = new JSONStorableString("keys", "");
                keysLabel = new JSONStorableString("keylabel", "Current keys: ");
                RegisterString(allKeys);
                CreateTextField(keysLabel, true).height = 115;

                UIDynamicButton addButton, remButton;
                addButton = CreateButton("Add key", false);
                addButton.button.onClick.AddListener(AddKey);
                remButton = CreateButton("Remove key", false);
                remButton.button.onClick.AddListener(RemoveKey);

                LookAway = new JSONStorableFloat("Look Away Preference", 0.10f, 0f, 1f, true, true);
                RegisterFloat(LookAway);
                CreateSlider(LookAway, true);

                CreateSpacer(false).height = 35;
                CreateSpacer(true).height = 35;

                LookPlayer = new JSONStorableFloat("Relative frequency\n(Player)", 1f, 0.0f, 5f, true, true);
                RegisterFloat(LookPlayer);
                CreateSlider(LookPlayer, false);

                PlayerMirr = new JSONStorableFloat("Mirror preference\n(Player)", 0.25f, 0.0f, 1f, true, true);
                RegisterFloat(PlayerMirr);
                CreateSlider(PlayerMirr, true);

                PlayerMinDis = new JSONStorableFloat("\"Greater than\" distance\n(Player)", 0.3f, 0.0f, 100f, true, true);
                RegisterFloat(PlayerMinDis);
                CreateSlider(PlayerMinDis, false);

                PlayerMaxDis = new JSONStorableFloat("\"Less than\" distance\n(Player)", 10f, 0.0f, 100f, true, true);
                RegisterFloat(PlayerMaxDis);
                CreateSlider(PlayerMaxDis, true);

                pupilScale = new JSONStorableFloat("Pupil size effect\n(Player)", 0.0f, -1f, 1f, true, true);
                RegisterFloat(pupilScale);
                CreateSlider(pupilScale, false);

                CreateSpacer(false).height = 10;
                CreateSpacer(true).height = 145;

                angrySad = new JSONStorableFloat("Makes Angry/Sad\n(Player)", 0.0f, -1f, 1f, true, true);
                RegisterFloat(angrySad);
                CreateSlider(angrySad, true);

                moodSlider = new JSONStorableFloat("Worsens/Improves mood\n(Player)", 0.0f, -4f, 4f, true, true);
                RegisterFloat(moodSlider);
                CreateSlider(moodSlider, false);

                arouseDisgust = new JSONStorableFloat("Disgust/Arousal factor\n(Player)", 0.0f, -4f, 4f, true, true);
                RegisterFloat(arouseDisgust);
                CreateSlider(arouseDisgust, true);

                surpriseFactor = new JSONStorableFloat("Is Surprising\n(Player)", 0.0f, -1f, 1f, true, true);
                RegisterFloat(surpriseFactor);
                CreateSlider(surpriseFactor, false);

                CreateSpacer(false).height = 35;
                CreateSpacer(true).height = 35;

                UIadd.Clear();
                UIrem.Clear();

                advShow = new JSONStorableBool("Show advanced", false);
                advShow.storeType = JSONStorableParam.StoreType.Full;
                CreateToggle(advShow, false);
                CreateSpacer(true).height = 50;

                distanceBiasCo = new JSONStorableFloat("Distance Bias Coefficient", 2f, 0f, 10f, true, true);
                RegisterFloat(distanceBiasCo);
                AddAdv(distanceBiasCo, false, 'f');

                distanceBiasEx = new JSONStorableFloat("Distance Bias Exponent", -1f, -10f, 10f, true, true);
                RegisterFloat(distanceBiasEx);
                AddAdv(distanceBiasEx, true, 'f');

                MinSRange = new JSONStorableFloat("Min Saccade Range", 0.01f, 0.0f, 0.1f, true, true);
                RegisterFloat(MinSRange);
                AddAdv(MinSRange, false, 'f');

                MaxSRange = new JSONStorableFloat("Max Saccade Range", 0.03f, 0.0f, 0.1f, true, true);
                RegisterFloat(MaxSRange);
                AddAdv(MaxSRange, true, 'f');

                MinLRange = new JSONStorableFloat("Min Look Away Range", 0.4f, 0.4f, 2f, true, true);
                RegisterFloat(MinLRange);
                AddAdv(MinLRange, false, 'f');

                MaxLRange = new JSONStorableFloat("Max Look Away Range", 1f, 0.4f, 2f, true, true);
                RegisterFloat(MaxLRange);
                AddAdv(MaxLRange, true, 'f');

                ClampH = new JSONStorableFloat("Horizontal twist limit", 140f, 0f, 140f, true, true);
                RegisterFloat(ClampH);
                AddAdv(ClampH, false, 'f');

                ClampV = new JSONStorableFloat("Vertical twist limit", 70f, 0f, 70f, true, true);
                RegisterFloat(ClampV);
                AddAdv(ClampV, true, 'f');

                offsetS = new JSONStorableFloat("Look Away Sideways Offset", 0f, -2f, 2f, true, true);
                RegisterFloat(offsetS);
                AddAdv(offsetS, false, 'f');

                offsetF = new JSONStorableFloat("Look Away Forward Offset", 1.5f, -2f, 2f, true, true);
                RegisterFloat(offsetF);
                AddAdv(offsetF, true, 'f');

                SaccadeSpeed = new JSONStorableFloat("Saccade Time Length", 0.6f, 0.5f, 0.8f, true, true);
                RegisterFloat(SaccadeSpeed);
                AddAdv(SaccadeSpeed, false, 'f');

                moveDuration = new JSONStorableFloat("Body Move Time Length", 0.6f, 0.5f, 2f, true, true);
                RegisterFloat(moveDuration);
                AddAdv(moveDuration, true, 'f');

                TimeIntervalMin = new JSONStorableFloat("Min Time Between Targets", 2.5f, 0.8f, 10f, true, true);
                RegisterFloat(TimeIntervalMin);
                AddAdv(TimeIntervalMin, false, 'f');

                TimeIntervalMax = new JSONStorableFloat("Max Time Between Targets", 5f, 0.8f, 10f, true, true);
                RegisterFloat(TimeIntervalMax);
                AddAdv(TimeIntervalMax, true, 'f');

                maxAngle = new JSONStorableFloat("Look range", 200f, 50f, 360f, true, true);
                RegisterFloat(maxAngle);
                AddAdv(maxAngle, false, 'f');

                minAngle = new JSONStorableFloat("Min. req. angle for head movement", 1f, 0.01f, 5f, true, true);
                RegisterFloat(minAngle);
                AddAdv(minAngle, true, 'f');

                blinkTimeMin = new JSONStorableFloat("Min time between blinks", 2.4f, 1f, 15f, true, true);
                RegisterFloat(blinkTimeMin);
                AddAdv(blinkTimeMin, false, 'f');

                blinkTimeMax = new JSONStorableFloat("Max time between blinks", 8f, 1f, 15f, true, true);
                RegisterFloat(blinkTimeMax);
                AddAdv(blinkTimeMax, true, 'f');

                blinkAngle = new JSONStorableFloat("Min. req. angle for forced blink", 30f, 0.0f, 181f, true, true);
                RegisterFloat(blinkAngle);
                AddAdv(blinkAngle, false, 'f');

                List<string> motionChoices = new List<string>();
                motionChoices.Add("Disable none");
                motionChoices.Add("Disable body");
                motionChoices.Add("Disable all");
                useMotion = new JSONStorableStringChooser("bodymotion", motionChoices, "Disable none", "Body motion");
                RegisterStringChooser(useMotion);
                AddAdv(useMotion, true, 's');

                pupilDilate = new JSONStorableBool("Do pupil dilation", true);
                pupilDilate.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(pupilDilate);
                AddAdv(pupilDilate, false, 'b');

                doSaccades = new JSONStorableBool("Do saccades", true);
                doSaccades.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(doSaccades);
                AddAdv(doSaccades, false, 'b');

                outsideRange = new JSONStorableBool("Look away when target invalid", true);
                outsideRange.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(outsideRange);
                AddAdv(outsideRange, true, 'b');

                doBlink = new JSONStorableBool("Do blinking", true);
                doBlink.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(doBlink);
                AddAdv(doBlink, true, 'b');
                #endregion

                #region Non-UI JSONStorable
                amMaster = new JSONStorableBool("amMaster", false);
                amMaster.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(amMaster);

                checkFrequency = new JSONStorableFloat("masterslavetime", 3f, 3f, 15f, true, true);
                RegisterFloat(checkFrequency);

                attributesA = new JSONStorableColor("attributesA", new HSVColor());
                RegisterColor(attributesA);

                attributesB = new JSONStorableColor("attributesB", new HSVColor());
                RegisterColor(attributesB);

                visualOutA = new JSONStorableColor("visualOutA", new HSVColor());
                RegisterColor(visualOutA);

                visualOutB = new JSONStorableColor("visualOutB", new HSVColor());
                RegisterColor(visualOutB);
                #endregion

                SuperController.LogMessage("Looker loaded on " + containingAtom.uid + " with key(s) " + lookerKey.val + " & " + allKeys.val + ".");
            }
            #region Exception
            catch (Exception e) {
                SuperController.LogError("Exception caught on load: " + e);
            }
            #endregion
        }

        //The Start function is used only to load the key string
        protected void Start() {
            KeyStringLoader(allKeys.val);
            ScriptSearchResult find = ScriptSearch(containingAtom, "PA4148415.RealGazeLooker", false);
            if (find.count > 1) {
                stopThis = true;
                SuperController.LogError("Remove any duplicate RealGaze - Looker scripts from " + containingAtom.uid + " or none of the Looker scripts will work on load.");
            } else {
                thisScript = find.scripts[0];
            }
        }

        //Using FixedUpdate because it's associated with physics frames rather than render frames, perhaps meaning motion will be a little smoother
        protected void FixedUpdate() {
            try {
                CheckShow();
                #region Compile attributes
                HSVColor a;
                a.H = moodSlider.val + 4f;
                a.S = posOnly(angrySad.val);
                a.V = posOnly(angrySad.val * -1f);
                HSVColor b;
                b.H = posOnly(arouseDisgust.val);
                b.S = surpriseFactor.val + 1f;
                b.V = posOnly(arouseDisgust.val * -1f);
                attributesA.val = a;
                attributesB.val = b;
                #endregion
                if (stopThis || CharHead.possessed) {
                    return; //if the character head is possessed, there's no point in doing anything from this script since the player can't see the eyes, and controls the head movement
                }
                if (SuperController.singleton.freezeAnimation == false) {
                    #region Other scripts
                    //get information like LookFor, and the various touch inputs from the other PA4148415 namespace scripts
                    if (EmotionScript != null) {
                        lookFor = EmotionScript.GetStringParamValue("LookFor");
                    } else {
                        lookFor = "Anything";
                    }
                    if (BodyScript != null) {
                        a = BodyScript.GetColorParamValue("touchA");
                        b = BodyScript.GetColorParamValue("touchB");
                        c = BodyScript.GetColorParamValue("touchC");
                        d = BodyScript.GetColorParamValue("touchD");
                    } else {
                        a = new HSVColor();
                        b = new HSVColor();
                        c = new HSVColor();
                        d = new HSVColor();
                    }
                    #endregion
                    CheckTimers();
                    if (useMotion.val != "Disable all") {
                        SetBodyMotion();
                    }
                    if (pupilDilate.val && (morphPupil.morphValue < targetVal - 0.001 || morphPupil.morphValue > targetVal + 0.001)) {
                        morphPupil.morphValue += (targetVal - startingVal) / 180;
                    }
                }
            }
            #region Exception
            catch (Exception e) {
                if (!suppressWarnings) {
                    SuperController.LogError("Exception caught in FixedUpdate: " + e);
                }
            }
            #endregion
        }

        #region Custom UI ADVANCED
        //checks if the advanced UI is showing, toggles if it's not supposed to be or if it isn't and is supposed to
        protected void CheckShow() {
            if (advShow.val != advShowing) {
                if (advShowing) {
                    //if the advanced menu is showing and it shouldn't be, remove all items
                    foreach (KeyValuePair<JSONStorableParam, char> UIpair in UIrem) {
                        if (UIpair.Value == 'f') {
                            RemoveSlider(UIpair.Key as JSONStorableFloat);
                        } else if (UIpair.Value == 'b') {
                            RemoveToggle(UIpair.Key as JSONStorableBool);
                        } else if (UIpair.Value == 's') {
                            RemovePopup(UIpair.Key as JSONStorableStringChooser);
                        }
                    }
                    advShowing = false;
                } else {
                    //otherwise, add all items
                    foreach (KeyValuePair<JSONStorableParam, bool> UIpair in UIadd) {
                        if (UIrem[UIpair.Key] == 'f') {
                            CreateSlider(UIpair.Key as JSONStorableFloat, UIpair.Value);
                        } else if (UIrem[UIpair.Key] == 'b') {
                            CreateToggle(UIpair.Key as JSONStorableBool, UIpair.Value);
                        } else if (UIrem[UIpair.Key] == 's') {
                            udp = CreateScrollablePopup(UIpair.Key as JSONStorableStringChooser, UIpair.Value);
                        }
                    }
                    advShowing = true;
                }
            }
        }

        //adds to both lists
        protected void AddAdv(JSONStorableParam param, bool LR, char type) {
            UIadd.Add(param, LR);
            UIrem.Add(param, type);
        }
        #endregion
        #endregion

        #region Master/slave logic
        #region Atom/Key dictionary pair dual functions
        //easier than doing these things on a case-by-case basis
        protected void ClearBoth() {
            atomToKey.Clear();
            keyToAtom.Clear();
        }
        protected void AddBoth(Atom a, float f) {
            atomToKey.Add(a, f);
            keyToAtom.Add(f, a);
        }
        #endregion

        //Checks master/non-master status
        protected void CheckStatus() {
            amMaster.val = false;
            int index = 0;
            ClearBoth();
            foreach (Atom atom in SuperController.singleton.GetAtoms().Where(atom => atom.type == "Person")) {
                ScriptSearchResult looker = ScriptSearch(atom, "PA4148415.RealGazeLooker", true); //break-on-find is true here because there should only be one RealGazeLooker per person atom
                if (looker.found) {
                    if (index > 0 && !amMaster.val) {
                        return; //if the index is greater than 0 and this script isn't the master yet, it won't be the master, so stop this script
                    }
                    if (atom == containingAtom && index == 0) {
                        amMaster.val = true;
                        checkFrequency.val = 3f; //if this script is the master, it should check for other looker scripts at a decent frequency so it can change their check frequency
                    }
                    if (amMaster.val && atom != containingAtom) {
                        looker.scripts[0].SetFloatParamValue("masterslavetime", 15f); //any looker script that isn't the master should check less frequently
                    }
                    index++;
                }
            }
        }

        //If this looker script is the master, pass down keys based on matching atoms, or find a new key for that character
        protected void PassDownKeys() {
            foreach (Atom atom in SuperController.singleton.GetAtoms().Where(atom => atom.type == "Person")) {
                foreach (JSONStorable receiver in ScriptSearch(atom, "PA4148415.RealGazeTarget", false).scripts) {
                    if (!receiver.GetBoolParamValue("haveCharKey")) { //if a target script reports that it doesn't have a key,
                        float output;
                        if (atomToKey.TryGetValue(atom, out output)) { //determine if a key exists already for the atom it's on. if it does,
                            receiver.SetBoolParamValue("haveCharKey", true); //tell it that it's received the key
                            receiver.SetFloatParamValue("givenCharKey", output); //and set its given character key to that value
                        } else { //if no key exists yet,
                            for (float f = 0.1f; f < 10.0f; f += 0.1f) { //comb all valid keys
                                Atom receive;
                                if (!keyToAtom.TryGetValue(f, out receive)) { //if this key does not already exist in the atomKey database,
                                    receiver.SetBoolParamValue("haveCharKey", true); //tell it that it's received the key
                                    receiver.SetFloatParamValue("givenCharKey", f); //set its given character key
                                    AddBoth(atom, f); //and update the database
                                    break; //break so you don't assign it multiple character keys
                                }
                            }
                        }
                        receiver.SetFloatParamValue("Looker key", receiver.GetFloatParamValue("givenCharKey")); //then, set its actual current key to the given key
                    }
                }
            }
        }
        #endregion

        #region Check things
        //Checks the timers for various time-related functions
        protected void CheckTimers() {
            //The reason that the target doesn't update in real time is due to the fact that eyes do not naturally float unless fixated on an object while the head itself moves
            //When an object is moving and the eye is tracking it, it'll flick from position A to position B in very very small and very fast increments
            #region Eye follow
            eyemovetime -= Time.deltaTime;
            if (eyemovetime <= 0f) {
                eyes.SetStringChooserParamValue("lookMode", "Target");
                eyelids.SetBoolParamValue("blinkEnabled", false);
                UpdateTarget(false);
                eyemovetime = 0.15f;
            }
            #endregion
            #region Saccades
            saccadetime -= Time.deltaTime;
            if (saccadetime <= 0f) {
                //check for targets each saccade-- this way, the check isn't running too often but is running often enough that it'll update quickly
                CheckForTargets();
                saccadetime = SaccadeSpeed.val;
                UpdateSaccade(offset);
                blinkCooldown--;
            }
            #endregion
            #region Blinking
            blinktime -= Time.deltaTime;
            if (blinktime <= 0f) {
                CharBlink();
            }
            #endregion
            #region Target switch
            //If target switch time has passed, update target and set suppresswarnings to false
            targettime -= Time.deltaTime;
            if (targettime <= 0f) {
                targettime = UnityEngine.Random.Range(TimeIntervalMin.val, TimeIntervalMax.val);
                UpdateTarget(true);
                suppressWarnings = false;
            }
            #endregion
            #region Master/Slave
            masterslavetime -= Time.deltaTime;
            if (masterslavetime <= 0f) {
                masterslavetime = checkFrequency.val;
                CheckStatus();
                if (amMaster.val) {
                    PassDownKeys();
                }
            }
            #endregion
            #region Other script
            checkSelfTimer -= Time.deltaTime;
            if (checkSelfTimer <= 0f) {
                ScriptSearchResult search = ScriptSearch(containingAtom, "PA4148415.BodyAutonomy", true);
                if (search.found) {
                    BodyScript = search.scripts[0];
                }
                search = ScriptSearch(containingAtom, "PA4148415.EmotionBrain", true);
                if (search.found) {
                    EmotionScript = search.scripts[0];
                }
                checkSelfTimer = 3f;
            }
            #endregion
        }

        //checks if the head is animated. if so, cancels out all body movement
        protected bool CheckHeadAnimated() {
            foreach (AnimationPattern AP in SuperController.singleton.GetAllAnimationPatterns()) {
                //if the transform the AnimationPattern it's looking at has the same values as the head transform, they must be the same transform and thus the head is animated
                bool playing = SuperController.singleton.GetAtomByUid(AP.uid).GetStorableByID("AnimationPattern").GetBoolParamValue("on"); //this lets me check to see whether or not the animation is playing, meaning animations can be turned on and off at will
                if (AP.animatedTransform.position == head.position && AP.animatedTransform.rotation == head.rotation && playing) {
                    return true;
                }
            }
            return false;
        }

        //Checks for valid eye targets with the correct key
        public void CheckForTargets() {
            List<float> checkKeys = new List<float>();
            if (currentKeys.Count > 0) {
                checkKeys.AddRange(currentKeys);
            }
            if (!checkKeys.Contains(lookerKey.val)) {
                checkKeys.Add(lookerKey.val);
            }
            allTargets.Clear();
            //add the player to the targets list
            allTargets.Add(new Target("\n(Player)", thisScript, SuperController.singleton.GetAtomByUid("[CameraRig]"), 0.01f));
            foreach (Atom atom in SuperController.singleton.GetAtoms()) {
                //check all atoms in the scene,
                if (atom.uid != "[CameraRig]") {
                    //if the atom isn't the camera rig,
                    ScriptSearchResult ssc = ScriptSearch(atom, "PA4148415.RealGazeTarget", false);
                    if (ssc.count > 0) {
                        foreach (JSONStorable receiver in ssc.scripts) {
                            if (receiver.GetBoolParamValue("haveCharKey")) {
                                Atom receive;
                                if (!keyToAtom.TryGetValue(receiver.GetFloatParamValue("givenCharKey"), out receive)) {
                                    AddBoth(atom, receiver.GetFloatParamValue("givenCharKey"));
                                }
                                if (receiver.GetStringChooserParamValue("receiver") != null) {
                                    foreach (float key in checkKeys) {
                                        if (receiver.GetStringChooserParamValue("receiver") != null && (receiver.GetFloatParamValue("Looker key") == key || (receiver.GetFloatParamValue("Looker key") == 0f && key > 0f) || (key == 0 && atom != containingAtom && receiver.GetFloatParamValue("Looker key") > 0f))) {
                                            Target newTarget = new Target("", receiver, atom, 0f);
                                            allTargets.Add(newTarget);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //determines the bias multiplier for the frequency of a target
        public float testBias(Target input) {
            float avgDis = 0;
            int divide = 0;
            if (input.direct) { //if direct is good, get the direct distance
                avgDis += Vector3.Distance(CharHead.transform.position, input.receiver.position) * (1 - input.mirrPref); //then, multiply it by the reverse of the mirror preference to get the "adjusted" direct distance
                divide++;
            }
            if (input.mirror) { //if mirror is good, get the average of all mirror distances
                List<Vector3> mirrorBlacklist = new List<Vector3>();
                foreach (List<Vector3> pair in input.mirrPos) {
                    if (!mirrorBlacklist.Contains(pair[1])) {
                        avgDis += (Vector3.Distance(CharHead.transform.position, pair[1]) + Vector3.Distance(pair[1], pair[0])) * input.mirrPref; //add the adjusted version of the distance
                        divide++;
                    }
                }
            }
            if (divide > 0) {
                avgDis = avgDis / divide; //actually get the average of all distances
            }
            float disBias = distanceBiasCo.val * (float)Math.Pow(avgDis, distanceBiasEx.val); //feed average distance into the distance bias equation
            //updates will add more things that change the bias here
            return disBias; //once those biases are added, this script will return all of them multiplied instead of just disBias
        }

        //search for a script with a given name on the given atom. if found, break depending on selection
        public ScriptSearchResult ScriptSearch(Atom atom, string scriptName, bool breakOnFind) {
            ScriptSearchResult result = new ScriptSearchResult(false, 0, new List<JSONStorable>());
            List<JSONStorable> allFound = new List<JSONStorable>();
            foreach (string receiverID in atom.GetStorableIDs()) {
                string subStr = receiverID.Substring(Math.Max(0, receiverID.Length - scriptName.Length)); //gets the last scriptName.Length characters of the receiver ID
                if (subStr == scriptName) {
                    result.found = true;
                    result.count++;
                    allFound.Add(atom.GetStorableByID(receiverID)); //add the receiver in case I plan on using the list of receivers in the scriptsearchresult
                    if (breakOnFind) {
                        break;
                    }
                }
            }
            if (!result.found) {
                allFound.Add(null);
            }
            result.scripts.AddRange(allFound);
            return result; //return the scriptsearchresult
        }
        #endregion

        #region Set things
        //Updates the current target
        protected void UpdateTarget(bool randTarget) {
            //Because the code for updating the target and the code for updating the eyetarget position both require checking the distance and angle validity of the target, I've combined them into the same function
            //using the randTarget boolean allows me to select, in a way, which of the "two" functions I want this function to perform
            bool selectNew = randTarget || (lookerKey.val != currentKey);
            Vector3 headForward = ContainingHead.rotation * Vector3.forward;
            bool distanceGood = false;
            if (needsBlink > 0.0f) {
                needsBlink += 0.05f;
            }
            #region Find valid targets
            totalChance = 0f; //checking the validity of targets before randomly selecting one means that this script doesn't need to do any of the re-runs of the random selection
            List<Target> validTargets = new List<Target>(); //since all of the targets are valid, if it randomly selects ANY target, it will be a target that works for the current frame
            foreach (Target check in allTargets) {
                #region Check if direct LoS is good
                if (check.minDis < check.maxDis) {
                    distanceGood = Vector3.Distance(ContainingHead.position, check.receiver.position) > check.minDis && Vector3.Distance(ContainingHead.position, check.receiver.position) < check.maxDis;
                } else {
                    distanceGood = Vector3.Distance(ContainingHead.position, check.receiver.position) > check.minDis || Vector3.Distance(ContainingHead.position, check.receiver.position) < check.maxDis;
                }
                //directGood determines if the object is within the given FoV and is within the specified min and max distance for the 
                bool directGood = Vector3.Angle(headForward, check.receiver.position - ContainingHead.position) <= (maxAngle.val / 2f) && distanceGood;
                #endregion
                #region Check mirrors for valid LoS
                bool mirrorGood = false;
                List<List<Vector3>> mirroredPositions = new List<List<Vector3>>();
                foreach (Atom reflectiveAtom in SuperController.singleton.GetAtoms().Where(atom => atom.category == "Reflective")) {
                    Rigidbody reflector = reflectiveAtom.rigidbodies.First(rb => rb.name == "control");
                    //This determines the current "up" of the reflector/looker/target setup
                    Vector3 reflectorUp = reflector.rotation * Vector3.up; //Line by ShortRecognition
                                                                           //The given mirror is good provided that the angle between the head looking forward and the mirror is less than the maximum angle allowed
                    if (Vector3.Angle(headForward, Vector3.Dot(reflectorUp, reflector.position - ContainingHead.position) * reflectorUp) <= (maxAngle.val / 2f)) { //Line by ShortRecognition
                                                                                                                                                                   //Then, if the mirror is determined to be good, it calculated the direction to the receiver
                        Vector3 facingVector = Vector3.Dot(reflectorUp, reflector.position - check.receiver.position) * reflectorUp; //Line by ShortRecognition
                        if (check.minDis < check.maxDis) {
                            distanceGood = Vector3.Distance(ContainingHead.position, 2 * facingVector + check.receiver.position) > check.minDis && Vector3.Distance(ContainingHead.position, 2 * facingVector + check.receiver.position) < check.maxDis;
                        } else {
                            distanceGood = Vector3.Distance(ContainingHead.position, 2 * facingVector + check.receiver.position) > check.minDis || Vector3.Distance(ContainingHead.position, 2 * facingVector + check.receiver.position) < check.maxDis;
                        }
                        if (ScriptSearch(reflectiveAtom, "PA4148415.RealGazeMirror", true).found) {
                            distanceGood = false;
                        }
                        if (distanceGood) {
                            //And adds it + the mirror's position to the list if it's within distance
                            List<Vector3> mirror = new List<Vector3>();
                            mirror.Add(2 * facingVector + check.receiver.position /*Line by ShortRecognition*/); //index 0 is the actual mirrored position
                            mirror.Add(reflector.position);
                            mirroredPositions.Add(mirror);
                        }
                    }
                }
                if (mirroredPositions.Count > 0) {
                    //as long as there were mirrors added to the list, mirrorGood 
                    mirrorGood = true;
                }
                #endregion
                if ((directGood && check.mirrPref != 1) || (mirrorGood && check.mirrPref != 0)) {
                    Target target = check;
                    target.direct = directGood;
                    target.mirror = mirrorGood;
                    target.mirrPos.AddRange(mirroredPositions);
                    target.adjFreq = target.relFreq * testBias(target);
                    totalChance += target.adjFreq;
                    validTargets.Add(target);
                }
            }
            #endregion
            #region Stuff to do when selectNew true
            if (selectNew) {
                UpdateLookAway();
                CheckForTargets();
                currentKey = lookerKey.val;
                LookTarget = ContainingHead.position + LookAwayPos;
                startingVal = morphPupil.morphValue;
                targetVal = morphVal.val;
                currentMinD = 0f;
                currentMaxD = 10000f;
                lookattarg = (UnityEngine.Random.value >= LookAway.val) && (totalChance >= 0.01);
                selection = UnityEngine.Random.Range(0, totalChance);
            }
            #endregion
            if (validTargets.Count > 0) { //only try this if the valid targets is greater than 0. saves some performance
                if (lookattarg) {
                    currentSum = 0;
                    foreach (Target select in validTargets) {
                        if ((selectNew && currentSum + select.adjFreq >= selection) || (!selectNew && currentTarg == select.receiver.name)) {
                            if (selectNew) {
                                mirrorPref = UnityEngine.Random.value < select.mirrPref;
                            }
                            bool directGood = select.direct;
                            bool mirrorGood = select.mirror;
                            #region Decide mirror or direct
                            if ((mirrorPref && mirrorGood) || (!mirrorPref && mirrorGood && !directGood && (select.mirrPref != 0))) {
                                //If the mirror is preferred and good, OR the mirror is not preferred, but the direct LoS is not good, and the mirror preference of the target is not 0, then set LookTarget to any of the mirrored positions in the list
                                if (selectNew) {
                                    LookTarget = (select.mirrPos.ElementAt(UnityEngine.Random.Range(0, (select.mirrPos.Count - 1))))[0];
                                    OldMirror = LookTarget;
                                } else {
                                    LookTarget = OldMirror;
                                }
                            }
                            if ((!mirrorPref && directGood) || (mirrorPref && !mirrorGood && directGood && (select.mirrPref != 1))) {
                                //If the mirror is not preferred and a direct LoS is good, OR the mirror is preferred, but the mirror LoS is not good, and the mirror preference of the target is not 1, then set LookTarget to the position of the receiver
                                LookTarget = select.receiver.position;
                            }
                            #endregion
                            #region Handle results
                            if (LookTarget != ContainingHead.position + LookAwayPos) {
                                startingVal = morphPupil.morphValue;
                                targetVal = morphVal.val + (0.6f * select.pupilScale);
                                currentMinD = select.minDis;
                                currentMaxD = select.maxDis;
                                offset = select.playerZOffset;
                                visualOutA.val = select.visualA;
                                visualOutB.val = select.visualB;
                                currentTarg = select.receiver.name;
                            }
                            //once a valid target has been selected, break
                            if (LookTarget != ContainingHead.position + LookAwayPos) {
                                break;
                            }
                            #endregion
                        } else {
                            currentSum += select.adjFreq;
                        }
                    }
                }
            }
            //Once all of the above is done, move the EyeTarget and add saccade
            #region Actually move the eye target and determine if blink necessary
            EyeTarget.position = LookTarget;
            EyeTarget.position += SaccadeOffset;
            //then, determine if it needs to try to force a blink
            if (Vector3.Angle(headForward, LookTarget - ContainingHead.position) > blinkAngle.val && blinkCooldown <= 0) {
                needsBlink = 0.45f;
            } else if (Vector3.Angle(headForward, LookTarget - ContainingHead.position) < blinkAngle.val) {
                needsBlink = 0.0f;
            }
            if (needsBlink > 0.01f && UnityEngine.Random.value >= needsBlink) {
                needsBlink = 0.0f;
                CharBlink();
            }
            #endregion
        }

        //Sets the rotation of the head, chest, and pelvis based on position of the look target
        protected void SetBodyMotion() {
            try {
                Vector3 headSideways = ContainingHead.rotation * Vector3.right;
                Vector3 headForward = ContainingHead.rotation * Vector3.forward;
                Vector3 toTarget = LookTarget - ContainingHead.position;
                #region Clamp definition stuff
                //I kinda hate the way that this actually works, but it seems successful in actually preventing the head from moving in weird ways
                float targAng = Vector3.SignedAngle(ContainingHead.position + Vector3.down, toTarget, headSideways);
                if (targAng >= -15 && targAng <= 90) {
                    clampH = Mathf.Clamp(ClampH.val * (targAng / 90), 0f, ClampH.val) * Mathf.Deg2Rad;
                } else if (targAng < -15 && targAng >= -90) { //Really it's just the use of if/elseif/else that bugs me. Would be nice to clean this up in the future
                    clampH = Mathf.Clamp(ClampH.val * ((15 + targAng) / -75), 0f, ClampH.val) * Mathf.Deg2Rad;
                } else {
                    clampH = 140f * Mathf.Deg2Rad;
                }
                #endregion
                //InverseTransformDirection turns global values into local values
                Transform neck = containingAtom.GetStorableByID("neckControl").transform;
                headForward = neck.InverseTransformDirection(Vector3.Normalize(headForward));
                toTarget = neck.InverseTransformDirection(Vector3.Normalize(toTarget));

                #region VeeRifter & MacGruber maths
                //most of the following code is based on VeeRifter and MacGruber's code, but I've made some changes that should eliminate some major issues
                float currH = Mathf.Atan2(headForward.x, headForward.z);
                float currV = Mathf.Atan2(headForward.y, (float)Math.Sqrt((headForward.x * headForward.x) + (headForward.z * headForward.z)));
                float targH = Mathf.Clamp(Mathf.Atan2(toTarget.x, toTarget.z), -clampH, clampH);
                float targV = Mathf.Clamp(Mathf.Atan2(toTarget.y, (float)Math.Sqrt((toTarget.x * toTarget.x) + (toTarget.z * toTarget.z))), -ClampV.val, ClampV.val) - (5f * Mathf.Deg2Rad);
                float startZ = head.localEulerAngles.z;
                #endregion

                #region Head control-ability checks
                if (CheckHeadAnimated()) {
                    return;
                }
                if (CharHead.currentRotationState == FreeControllerV3.RotationState.Comply) {
                    CharHead.currentRotationState = FreeControllerV3.RotationState.On;
                }
                #endregion

                if (Vector3.Angle(headForward, toTarget) > minAngle.val && CharHead.currentRotationState == FreeControllerV3.RotationState.On) {
                    #region Calculate next step
                    currH = Mathf.SmoothDamp(currH, targH, ref velH, moveDuration.val);
                    currV = Mathf.SmoothDamp(currV, targV, ref velV, moveDuration.val);
                    //this recombines the angles into a vector and switch them from local to global which then can be fed into Quaternion.LookRotation()
                    Vector3 step = neck.TransformDirection(new Vector3(Mathf.Sin(currH) * Mathf.Cos(currV), Mathf.Sin(currV), Mathf.Cos(currH) * Mathf.Cos(currV)));
                    #endregion
                    head.rotation = Quaternion.LookRotation(step, neck.position - CharChest.transform.position);
                    Vector3 eulerAngles = head.localEulerAngles;
                    eulerAngles.z = startZ;
                    head.localEulerAngles = eulerAngles;

                    if (useMotion.val != "Disable body") {
                        //then, use the rotation of the head to determine how the body should pivot
                        float driveTarget = Mathf.Clamp(currH * -10.0f, -20.0f, 20.0f);
                        CharChest.jointRotationDriveYTarget = driveTarget;
                        driveTarget = Mathf.Clamp(currH * -7.5f, -15.0f, 15.0f);
                        CharPelv.jointRotationDriveYTarget = driveTarget;
                    }
                }
            }
            #region Exception
            catch (Exception e) {
                if (!suppressWarnings) {
                    SuperController.LogError("Exception caught in SetBodyMotion: " + e);
                }
            }
            #endregion
        }

        //tries to blink the eyes. if it can, resets the blink cooldown and the blink timer
        protected void CharBlink() {
            if (blinkCooldown <= 0 && doBlink.val) {
                blinktime = UnityEngine.Random.Range(blinkTimeMin.val, blinkTimeMax.val);
                eyelidControl.Blink();
                blinkCooldown = 2;
            }
        }
        #endregion


        #region Game objects
        protected Rigidbody EyeTarget; //eyeTargetController
        protected Rigidbody ContainingHead; //Containing person atom's head
        protected Transform head; //the containing person's head, as a transform for rotation purposes
        protected FreeControllerV3 CharHead; //the containing person's head, as a freecontroller for joint drive purposes
        protected FreeControllerV3 CharChest; //the containing person's chest
        protected FreeControllerV3 CharPelv; //the containing person's pelvis
        protected JSONStorable eyes; //gets the eyes receiver of the character so the lookMode can be set to Target
        protected JSONStorable eyelids; //gets the eyelids receiver of the character so automatic blinking can be turned off
        protected JSONStorable EmotionScript; //the emotion script, if found
        protected JSONStorable BodyScript; //the body script, if found
        private JSONStorable thisScript; //this script
        DAZCharacterSelector character; //selects the character for DAZ morph control
        DAZMeshEyelidControl eyelidControl; //the DAZ eyelid control object
        GenerateDAZMorphsControlUI morphControl; //the actual morph control for DAZ
        DAZMorph morphPupil; //the character's pupil morph
        #endregion

        #region JSONStorables
        protected JSONStorableString allKeys; //a string which stores all of the keys so they can be saved and loaded
        protected JSONStorableFloat TimeIntervalMax; //maximum time between targets
        protected JSONStorableFloat TimeIntervalMin; //minimum time between targets
        protected JSONStorableFloat SaccadeSpeed; //speed of saccades
        protected JSONStorableFloat MinSRange; //minimum range of motion for saccades
        protected JSONStorableFloat MaxSRange; //maximum range of motion for saccades
        protected JSONStorableFloat MinLRange; //minimum range of motion for look away
        protected JSONStorableFloat MaxLRange; //maximum range of motion for look away
        protected JSONStorableFloat offsetS; //look away offset sideways
        protected JSONStorableFloat offsetF; //look away offset forward
        protected JSONStorableFloat LookAway; //look away frequency
        protected JSONStorableFloat lookerKey; //current key selection, whether or not it's been added. if the length of the key list is 0, this key is added as the only item
        protected JSONStorableFloat LookPlayer; //look at player frequency
        protected JSONStorableFloat PlayerMirr; //look at player through mirror frequency
        protected JSONStorableFloat PlayerMinDis; //minimum distance for the player to be valid
        protected JSONStorableFloat PlayerMaxDis; //maximum distance for the player to be valid
        protected JSONStorableFloat pupilScale; //the pupil scaling value for the player camera
        protected JSONStorableFloat arouseDisgust; //(player) whether or not this target is arousing
        protected JSONStorableFloat moodSlider; //(player) whether or not this characters gets happy or angry/sad looking at this target
        protected JSONStorableFloat angrySad; //(player) whether or not the character gets sad or angry looking at this target
        protected JSONStorableFloat surpriseFactor; //(player) surprise factor
        protected JSONStorableFloat maxAngle; //maximum angle for targets to be considered valid. this is artificially doubled, as the angle function used to determine look angle only maxes out at 180. actually going up to 360 isn't necessary for 360 degree viewing
        protected JSONStorableFloat minAngle; //minimum angle to bother with head movement
        protected JSONStorableFloat morphVal; //the morph value of the pupil given by the look
        protected JSONStorableFloat moveDuration; //speed of body motion changing
        protected JSONStorableFloat arousalLevel; //the level of arousal the looker is currently experiencing, from 0 to 100. can be read by other plugins using .GetFloatParamValue("Character arousal level")
        protected JSONStorableFloat checkFrequency; //the frequency with which this looker script should check to see if it's the master or slave
        protected JSONStorableFloat blinkAngle; //how many degrees the eye target must move from current forward to require a blink
        protected JSONStorableFloat blinkTimeMin; //minimum time between blinking
        protected JSONStorableFloat blinkTimeMax; //maximum time between blinking
        protected JSONStorableFloat ClampH; //user-defined horizontal clamp value
        protected JSONStorableFloat ClampV; //user-defined vertical clamp value
        protected JSONStorableFloat distanceBiasCo; //how much the closeness of an object determines its bias
        protected JSONStorableFloat distanceBiasEx; //how quickly the closeness of an object determines its bias
        protected JSONStorableBool outsideRange; //whether or not to look away when the chosen target is outside its specified range
        protected JSONStorableBool pupilDilate; //whether or not to dilate the pupils ever
        protected JSONStorableBool doSaccades; //whether or not to use saccades
        protected JSONStorableBool doBlink; //whether or not to use saccades
        protected JSONStorableBool amMaster; //whether or not this given Looker script is the master script
        protected JSONStorableStringChooser useMotion; //whether or not to actually move the head/body
        protected JSONStorableColor attributesA; //the attributes for the player. happy/sad/angry
        protected JSONStorableColor attributesB; //the attributes for the player arousal/surprise/disgust
        protected JSONStorableColor visualOutA; //happy, sad, angry
        protected JSONStorableColor visualOutB; //arousal, surprise, disgust
        #endregion

        #region Private runtime vars
        private UIDynamicPopup udp; //dynamic popup for dropdown lists
        private JSONStorableString keysLabel; //not actually stored, used for making the "keys" display reasonable
        private JSONStorableBool advShow; //again not actually stored, used for showing/hiding the advanced UI
        private Vector3 SaccadeOffset; //the saccade offset
        private Vector3 LookAwayPos; //where the character looks away to
        private Vector3 LookTarget; //the position of the current target
        private Vector3 OldMirror; //the previously selected mirror, for when not selecting a new target
        private HSVColor a; //touch input group a, used for determining bias
        private HSVColor b; //touch input group b, used for determining bias
        private HSVColor c; //touch input group c, used for determining bias
        private HSVColor d; //touch input group d, used for determining bias
        private string lookFor; //what this character currently has a bias for, based on emotions
        private string currentTarg; //the current selected target. for use when the adjusted frequency ends up moving the selected target out of range
        private float velH; //a float which determines the current speed of the horizontal motion of the head
        private float velV; //like velH, but for vertical motion
        private float currentMinD = 0f; //the current minimum distance the target must be at
        private float currentMaxD = 10000f; //the current maximum distance the target must be at
        private float currentKey; //the current key
        private float currentSum; //used for finding the randomly selected target
        private float eyemovetime = 0.1f; //timer for target tracking
        private float targettime = 0.1f; //timer for changing targets
        private float saccadetime = 0.1f; //timer for updating saccade
        private float masterslavetime = 3f; //timer for checking master/slave
        private float blinktime = 2f; //timer for blinking
        private float checkSelfTimer; //how often to check for other PA4148415 namespace scripts
        private float totalChance = 0; //used for finding the randomly selected target
        private float startingVal; //the starting size of the pupil
        private float targetVal; //the target pupil size
        private float selection; //the randomly generated number used for finding the randomly selected target
        private float offset; //the z offset. used only internally
        private float clampH = 140f * Mathf.Deg2Rad; //the maximum angle for horizontal head movement
        private float clampV = 70f * Mathf.Deg2Rad; //the maximum angle for vertical head movement
        private float needsBlink = 0f; //whether or not the eye movement is large enough to justify a blink, and how likely the character should be to blink during any given frame
        private bool lookattarg; //whether or not to look at a target or away
        private bool mirrorPref = false; //whether or not to prefer a mirror at the moment
        private bool suppressWarnings = true; //whether or not to suppress warnings
        private bool stopThis = false; //whether or not this plugin should not run
        private bool advShowing = false; //whether or not the advanced UI is actually showing
        private int blinkCooldown = 2; //the cooldown for blinking, set to 2 when char blinks on eye movement, decremented on saccade, cooldown gone at 0
        List<Target> allTargets = new List<Target>(); //This is the targets list, only contains receivers with a matching key float value
        List<float> currentKeys = new List<float>(); //the list of current keys
        Dictionary<float, Atom> keyToAtom = new Dictionary<float, Atom>(); //key to atom for checking character keys
        Dictionary<Atom, float> atomToKey = new Dictionary<Atom, float>(); //atom to key for checking character keys
        Dictionary<JSONStorableParam, bool> UIadd = new Dictionary<JSONStorableParam, bool>(); //dictionary for adding sliders/bool/dropdowns from advanced UI
        Dictionary<JSONStorableParam, char> UIrem = new Dictionary<JSONStorableParam, char>(); //dictionary for removing sliders/bool/dropdowns from advanced UI
        #endregion
    }
}