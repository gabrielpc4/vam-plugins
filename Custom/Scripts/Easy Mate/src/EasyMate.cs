using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    /// <summary>Runs lifecycle callbacks late for overlap release and monitor dot lasers.</summary>
    [DefaultExecutionOrder(32000)]
    public class EasyMate : MVRScript
    {
        //Manage the EasyMate menu system, add Main Menu and other buttons to scenes which are loaded from a menu but don't have any return buttons
        //This is also required for scenes packaged in var files, etc., which we won't modify

        private static bool logMessages = false;

        private MainUIButtons mainUIButtons = null; //LOAD PERSON, POSE, ETC.

        public const string resetVROrientationID = "_ResetVROrientation";
        Atom resetVROrientation;

        private string menuDataJSONNodeName = "_EasyMate";
        private string menuDataJSONName = "plugin#0_geesp0t.AutoLoadEasyMate";
        private string pluginDataJSONName = "plugin#0_geesp0t.ResetVROrientation";
        private string uiNeedsUpdateJSONName = "UI Needs Update";
        private string buttonTextJSONName = "Additional Button Text";
        private string buttonSceneJSONName = "Additional Button Scene";
        private string menuButtonScene = "Saves/scene/MainMenu_Page_3.json"; //change this as we browse between menus
        private string menuButtonText = "Menu Page 3";

        private bool isLoading = true;
        private bool sceneChanged = true;
        private float loadingTimeCounter = 0;

        private string lastLoadDir = ""; //wish this was last full path of loaded file included directory and filename!

        private Coroutine _applyEmotionAfterSceneCo;

        private Coroutine _pathRuleEmotionMergeCo;

        private Coroutine _spankingsAutoMergeProximityCo;

        private bool _spankingsAutoMergeCompletedThisScene;

        private const float SpankingsAutoMergePollSeconds = 0.2f;

        private const float SpankingsAutoMergeDistanceMeters = 0.16f;

        public JSONStorableAction hideUI;
        public JSONStorableAction showUI;

        /// <summary>
        /// When true (default), calls <see cref="SuperController.DisableRemoteHoldGrab"/> so the VR <b>grip</b>
        /// (Quest side squeeze / OpenVR HoldGrab) cannot start the remote “hand sticks to FreeController” link
        /// while pointing with the controller laser. VaM has no built-in toggle for this; overlap (hand inside
        /// the control collider) + grip can still full-grab.
        /// </summary>
        public JSONStorableBool disableRemoteGripHandLink;

        /// <summary>
        /// When true (default), a short press on <b>either</b> controller’s physical <b>grip</b> (Oculus HandTrigger / swapped index trigger, OpenVR HoldGrab)
        /// toggles <b>both</b> sides together between articulated VR hands (<b>Male2</b>) and VaM’s sphere/kinematic hand mode
        /// (see <see cref="EasyMateGripHandVisibility"/>); while any <c>Person</c> head or hand is possessed, VR proxies use
        /// <b>None</b> (not sphere / not Male2). Collisions stay off while both
        /// sides sphere. When grip first enables articulated <b>Male2</b> hands,
        /// Easy Mate starts a lightweight timed proximity check (not every frame).
        /// Once any controller hand gets very close to any female body, it merges
        /// <b>Spankings</b> onto all female <c>Person</c> atoms missing it, then
        /// 4 seconds later retries once if any female still lacks the plugin.
        /// If <c>Custom/Scripts/Easy Mate/spankings_grip_merge_block_path_keywords.txt</c>
        /// matches current load/save dirs, no automatic grip-hand Spankings logic runs.
        /// </summary>
        public JSONStorableBool gripTogglesHandVisibility;

        /// <summary>
        /// When true (default), calls <see cref="FreeControllerV3.RestorePreLinkState"/> on active full-grab targets
        /// each <see cref="LateUpdate"/> when not using remote hold-grab (public <see cref="SuperController"/> API only).
        /// </summary>
        public JSONStorableBool blockOverlapFullGrab;

        /// <summary>
        /// When true (default), HMD inside any Person’s head cylinder hides face/hair/glasses on VR eye cameras.
        /// Scenes or presets may still override the saved value.
        /// </summary>
        public JSONStorableBool headProximityHide;

        /// <summary>
        /// When true (default), clears all possession if the look camera moves farther than
        /// <see cref="possessAutoUnpossessFeetMaxHorizontalM"/> (meters) from the possessed character’s feet in the horizontal plane (rig up).
        /// </summary>
        public JSONStorableBool possessAutoUnpossessWhenFarFromFeet;

        /// <summary>When true (default), after a non-looping scene mocap at least
        /// <see cref="longMocapMinSecondsForEmotionMerge"/> long finishes, loads
        /// <c>Saves/scene/Default.json</c> once (uses
        /// <see cref="SuperController.motionAnimationMaster"/>).</summary>
        public JSONStorableBool mergeEmotionWhenLongMocapEndsNoLoop;

        /// <summary>Minimum longest <see cref="MotionAnimationClip.clipLength"/> in
        /// the scene (seconds) for end-of-mocap default scene load; avoids short clips.
        /// </summary>
        public JSONStorableFloat longMocapMinSecondsForEmotionMerge;

        /// <summary>
        /// When true (default), in main monitor mode, shows blue/red aim cylinders while
        /// the UI-aim gesture is active: Oculus X/A capacitive touch, or OpenVR (SteamVR
        /// / e.g. Virtual Desktop) <c>TargetShow</c> via
        /// <see cref="SuperController.GetLeftUIPointerShow"/> /
        /// <see cref="SuperController.GetRightUIPointerShow"/>.
        /// </summary>
        public JSONStorableBool restoreMonitorModeControllerLaser;

        /// <summary>Horizontal distance threshold from look camera to feet midpoint for <see cref="possessAutoUnpossessWhenFarFromFeet"/>.</summary>
        public JSONStorableFloat possessAutoUnpossessFeetMaxHorizontalM;

        /// <summary>
        /// Restore navigation rig, monitor orientation, and player height after a
        /// load when VaM stays in the same <see cref="SuperController.currentLoadDir"/>
        /// (e.g. switching between JSON files inside one chapter folder).
        /// </summary>
        public JSONStorableBool retainCameraPoseSameFolderLoads;

        private bool prevSuperLoading;

        private const float VamDefaultMonitorCameraFov = 40f;

        private const float EasyMateDefaultMonitorCameraFov = 50f;

        public override void Init()
        {
            Log("EasyMate Init");
            mainUIButtons = new MainUIButtons();
            mainUIButtons.Init(this);

            hideUI = new JSONStorableAction("Hide UI", () => HideUI());
            RegisterAction(hideUI);
            showUI = new JSONStorableAction("Show UI", () => ShowUI());
            RegisterAction(showUI);

            disableRemoteGripHandLink = new JSONStorableBool(
                "Disable remote grip hand-link (Quest / OpenVR)",
                true,
                OnDisableRemoteGripHandLinkChanged);
            RegisterBool(disableRemoteGripHandLink);

            gripTogglesHandVisibility = new JSONStorableBool("Grip toggles VR hand visibility", true);
            RegisterBool(gripTogglesHandVisibility);

            blockOverlapFullGrab = new JSONStorableBool("Block overlap full-grab (auto-release each frame)", true);
            RegisterBool(blockOverlapFullGrab);

            headProximityHide = new JSONStorableBool("VR head proximity hide", true, OnHeadProximityHideChanged);
            RegisterBool(headProximityHide);

            possessAutoUnpossessWhenFarFromFeet = new JSONStorableBool(
                "Auto-unpossess when look camera drifts from feet (horizontal)",
                true);
            RegisterBool(possessAutoUnpossessWhenFarFromFeet);

            possessAutoUnpossessFeetMaxHorizontalM = new JSONStorableFloat(
                "Max horizontal camera–feet distance (m)",
                1.35f,
                0.35f,
                5f);
            RegisterFloat(possessAutoUnpossessFeetMaxHorizontalM);

            retainCameraPoseSameFolderLoads = new JSONStorableBool(
                "Retain camera pose (loads from same folder)",
                true,
                OnRetainSameFolderPoseChanged);
            RegisterBool(retainCameraPoseSameFolderLoads);
            EasyMateSameFolderCameraRetain.SetRetainEnabled(
                retainCameraPoseSameFolderLoads.val);

            mergeEmotionWhenLongMocapEndsNoLoop = new JSONStorableBool(
                "Load Saves/scene/Default.json when long mocap ends (no loop)",
                true);
            RegisterBool(mergeEmotionWhenLongMocapEndsNoLoop);

            longMocapMinSecondsForEmotionMerge = new JSONStorableFloat(
                "Min mocap length (s) for end-of-clip default scene load",
                45f,
                5f,
                600f);
            RegisterFloat(longMocapMinSecondsForEmotionMerge);

            restoreMonitorModeControllerLaser = new JSONStorableBool(
                "Monitor mode: beams (Quest X/A touch or SteamVR TargetShow)",
                true);
            RegisterBool(restoreMonitorModeControllerLaser);

            SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedPathRuleEmotion;
            SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChangedPathRuleEmotion;

            EasyMateGripHandVisibility.SetOnMale2HandsEnabled(
                StartSpankingsAutoMergeProximityCheck);
        }

        private void ResetSpankingsAutoMergeStateForScene()
        {
            _spankingsAutoMergeCompletedThisScene = false;
            if (_spankingsAutoMergeProximityCo != null)
            {
                StopCoroutine(_spankingsAutoMergeProximityCo);
                _spankingsAutoMergeProximityCo = null;
            }
        }

        private void StartSpankingsAutoMergeProximityCheck()
        {
            if (mainUIButtons == null || _spankingsAutoMergeCompletedThisScene)
                return;
            if (EasyMateSpankingsGripBlockPathKeywords
                .CurrentSceneBlocksGripSpankingsMerge())
            {
                _spankingsAutoMergeCompletedThisScene = true;
                return;
            }
            if (!EasyMateGripHandVisibility.IsAnyPreferredHandArticulated())
                return;
            if (!mainUIButtons.AnyFemalePersonMissingSpankings())
            {
                _spankingsAutoMergeCompletedThisScene = true;
                return;
            }
            if (_spankingsAutoMergeProximityCo != null)
                return;
            _spankingsAutoMergeProximityCo =
                StartCoroutine(CoWaitForHandNearFemaleThenMergeSpankings());
        }

        private IEnumerator CoWaitForHandNearFemaleThenMergeSpankings()
        {
            try
            {
                while (true)
                {
                    if (mainUIButtons == null ||
                        _spankingsAutoMergeCompletedThisScene)
                    {
                        yield break;
                    }

                    if (EasyMateSpankingsGripBlockPathKeywords
                        .CurrentSceneBlocksGripSpankingsMerge())
                    {
                        _spankingsAutoMergeCompletedThisScene = true;
                        yield break;
                    }

                    if (!EasyMateGripHandVisibility
                        .IsAnyPreferredHandArticulated())
                    {
                        yield break;
                    }

                    if (!mainUIButtons.AnyFemalePersonMissingSpankings())
                    {
                        _spankingsAutoMergeCompletedThisScene = true;
                        yield break;
                    }

                    if (!mainUIButtons
                        .AnyFemalePersonWithinControllerHandDistance(
                            SpankingsAutoMergeDistanceMeters))
                    {
                        yield return new WaitForSeconds(
                            SpankingsAutoMergePollSeconds);
                        continue;
                    }

                    mainUIButtons.MergeSpankingsOnFemalePersonsOnly();
                    yield return new WaitForSeconds(4f);

                    if (mainUIButtons == null)
                        yield break;
                    if (EasyMateSpankingsGripBlockPathKeywords
                        .CurrentSceneBlocksGripSpankingsMerge())
                    {
                        _spankingsAutoMergeCompletedThisScene = true;
                        yield break;
                    }
                    if (!EasyMateGripHandVisibility
                        .IsAnyPreferredHandArticulated())
                    {
                        yield break;
                    }

                    if (mainUIButtons.AnyFemalePersonMissingSpankings())
                    {
                        mainUIButtons.MergeSpankingsOnFemalePersonsOnly();
                        yield return new WaitForSeconds(
                            SpankingsAutoMergePollSeconds);
                        continue;
                    }

                    _spankingsAutoMergeCompletedThisScene = true;
                    yield break;
                }
            }
            finally
            {
                _spankingsAutoMergeProximityCo = null;
            }
        }

        private void OnHeadProximityHideChanged(bool v)
        {
            EasyMateVrHeadCylinderHide.SetHeadProximityHideEnabled(v, this);
        }

        private void OnDisableRemoteGripHandLinkChanged(bool v)
        {
            ApplyRemoteHoldGrabPreference();
        }

        private void OnRetainSameFolderPoseChanged(bool v)
        {
            EasyMateSameFolderCameraRetain.SetRetainEnabled(v);
        }

        private void ApplyRemoteHoldGrabPreference()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || disableRemoteGripHandLink == null)
                return;
            if (!sc.isOVR && !sc.isOpenVR)
                return;
            if (disableRemoteGripHandLink.val)
                sc.DisableRemoteHoldGrab();
            else
                sc.EnableRemoteHoldGrab();
        }

        private void ApplyDefaultMonitorCameraFovIfNeeded()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            if (Mathf.Abs(sc.monitorCameraFOV - VamDefaultMonitorCameraFov) >
                0.001f)
            {
                return;
            }

            sc.monitorCameraFOV = EasyMateDefaultMonitorCameraFov;
        }

        public void ShowUI()
        {
            if (mainUIButtons != null)
            {
                mainUIButtons.ShowUI(true);
            }
        }
        public void HideUI()
        {
            if (mainUIButtons != null)
            {
                mainUIButtons.ShowUI(false);
            }
        }

        void Start()
        {
            Log("EasyMate Start");
            if (mainUIButtons != null) mainUIButtons.Start();
            ApplyRemoteHoldGrabPreference();
            if (headProximityHide != null)
                EasyMateVrHeadCylinderHide.SetHeadProximityHideEnabled(headProximityHide.val, this);
            StartCoroutine(CoRefreshHeadProximityHooksAfterStartFrames());
            ResetSpankingsAutoMergeStateForScene();
            EasyMateGripHandVisibility.DisableVrHandModelsForSceneStart();
            EasyMateMotionAnimationEmotionEnd.ResetForNewScene();
            ApplyDefaultMonitorCameraFovIfNeeded();
        }

        private IEnumerator CoRefreshHeadProximityHooksAfterStartFrames()
        {
            yield return null;
            yield return null;

            if (headProximityHide == null)
            {
                yield break;
            }

            EasyMateVrHeadCylinderHide.SetHeadProximityHideEnabled(headProximityHide.val, this);
        }

        /// <summary>
        /// Person plugin lists can restore over several frames; merge E-MotionLite when load/save paths match keywords in
        /// <see cref="EasyMateEmotionPathKeywords.KeywordsFileRelative"/> (see <see cref="EasyMateEmotionPathKeywords"/>); clothing touch fall-off on everyone; refresh HUD.
        /// </summary>
        private IEnumerator CoApplyEmotionAfterSceneSettles()
        {
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.35f);

            try
            {
                if (SuperController.singleton == null || mainUIButtons == null)
                    yield break;

                bool pathRuleMerge = EasyMateEmotionPathKeywords.MatchesCurrentScenePath();

                if (pathRuleMerge)
                    mainUIButtons.MergeEmotionLiteForPathRuleOnAllPersonsOnly();
                mainUIButtons.MergeClothingTouchFallOffOnAllPersonsOnly();
                mainUIButtons.RefreshPluginToggleLabels();
            }
            finally
            {
                _applyEmotionAfterSceneCo = null;
            }
        }

        private void OnAtomUIDsChangedPathRuleEmotion(List<string> atomUids)
        {
            try
            {
                EasyMateFemalePassengerRuntime.NotifyAtomUidsChanged(
                    atomUids,
                    this);

                if (atomUids == null || atomUids.Count == 0)
                    return;
                if (!EasyMateEmotionPathKeywords.MatchesCurrentScenePath())
                    return;

                SuperController sc = SuperController.singleton;
                if (sc == null || sc.isLoading)
                    return;

                bool sawPerson = false;
                for (int i = 0; i < atomUids.Count; i++)
                {
                    Atom a = sc.GetAtomByUid(atomUids[i]);
                    if (a != null && a.type == "Person")
                    {
                        sawPerson = true;
                        break;
                    }
                }

                if (!sawPerson)
                    return;

                StartPathRuleEmotionMergeDeferred();
            }
            catch (Exception e)
            {
                LogError("EasyMate path-rule E-Motion (atom UID change): " + e.Message);
            }
        }

        private void StartPathRuleEmotionMergeDeferred()
        {
            if (!EasyMateEmotionPathKeywords.MatchesCurrentScenePath())
                return;
            if (_pathRuleEmotionMergeCo != null)
            {
                StopCoroutine(_pathRuleEmotionMergeCo);
                _pathRuleEmotionMergeCo = null;
            }
            _pathRuleEmotionMergeCo = StartCoroutine(CoPathRuleEmotionMergeDeferred());
        }

        private IEnumerator CoPathRuleEmotionMergeDeferred()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                while (sc != null && sc.isLoading)
                    yield return null;

                yield return null;
                yield return null;
                yield return new WaitForSecondsRealtime(0.35f);

                if (sc == null || mainUIButtons == null)
                    yield break;
                if (!EasyMateEmotionPathKeywords.MatchesCurrentScenePath())
                    yield break;

                mainUIButtons.MergeEmotionLiteForPathRuleOnAllPersonsOnly();
                mainUIButtons.RefreshPluginToggleLabels();
            }
            finally
            {
                _pathRuleEmotionMergeCo = null;
            }
        }

        private IEnumerator CreateResetVROrientationAtom()
        {
            yield return SuperController.singleton.AddAtomByType("Empty", resetVROrientationID);
            resetVROrientation = SuperController.singleton.GetAtomByUid(resetVROrientationID);
            ConfigureResetVROrientation();
        }

        private void GetMenuData()
        {
            //THIS DATA ISN'T WHAT TO SET FOR THIS PAGE, IT'S WHAT TO SET ON ANY NEXT PAGE THAT DOESN'T HAVE ANYTHING SET
            Atom menuData = SuperController.singleton.GetAtomByUid(menuDataJSONNodeName);
            if (menuData != null)
            {
                JSONStorable easyMateMenuData = menuData.GetStorableByID(menuDataJSONName);
                if (easyMateMenuData != null)
                {
                    string buttonText = easyMateMenuData.GetStringParamValue(buttonTextJSONName);
                    if (buttonText != null && buttonText != "")
                    {
                        Log("Last Menu Button Text: " + buttonText);
                        menuButtonText = buttonText;
                    }

                    string buttonScene = easyMateMenuData.GetStringParamValue(buttonSceneJSONName);
                    if (buttonScene != null && buttonScene != "")
                    {
                        Log("Last Menu Button Scene: " + buttonScene);
                        menuButtonScene = buttonScene;
                    }
                }
            }
        }

        private void LogError(string error)
        {
            SuperController.LogError(error);
        }

        private void Log(string message)
        {
            if (logMessages) SuperController.LogMessage(message);
        }

        private static bool SceneHasAnyActivePossession()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3[] fcs = a.transform.GetComponentsInChildren<FreeControllerV3>(true);
                    for (int i = 0; i < fcs.Length; i++)
                    {
                        if (fcs[i] != null && fcs[i].possessed)
                            return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private void ClearAllPossessionIfLoadedSceneHadAny()
        {
            try
            {
                if (!SceneHasAnyActivePossession())
                    return;
                MainUIButtons.RequestClearAllPossession(
                    "EasyMate: ClearPossess after scene load (possession was active).",
                    advanceVrPalmHudGenderCycle: false);
            }
            catch (Exception e)
            {
                LogError("EasyMate ClearPossess on scene load failed: " + e.Message);
            }
        }

        private void ConfigureResetVROrientation()
        {
            if (resetVROrientation == null)
            {
                LogError("Missing required " + resetVROrientationID + " Atom.");
            }

            JSONClass jc;
            MVRPluginManager pluginManager = resetVROrientation.GetStorableByID("PluginManager") as MVRPluginManager;
            JSONClass pluginManagerJSON = pluginManager.GetJSON(true, true, true);
            if (pluginManagerJSON["plugins"] != null && pluginManagerJSON["plugins"]["plugin#0"] != null &&
                    pluginManagerJSON["plugins"]["plugin#0"].Value != "")
            {
                //has a plugin
            } else
            {
                //make the plugin
                Log("Adding ResetVROrientation Plugin");
                jc = BuildResetVROrientationPlugin();
                pluginManager.LateRestoreFromJSON(jc);
            }

            JSONStorable pluginData = resetVROrientation.GetStorableByID(pluginDataJSONName);
            if (pluginData != null)
            {
                string buttonText = pluginData.GetStringParamValue(buttonTextJSONName);
                string buttonScene = pluginData.GetStringParamValue(buttonSceneJSONName);

                if (buttonText == null || buttonText == "" || buttonText == "Looks Menu"
                    || buttonScene == null || buttonScene == "" || buttonScene.EndsWith("PersonLooksMenu.json"))
                { 
                    pluginData.SetStringParamValue(buttonTextJSONName, menuButtonText);
                    pluginData.SetStringParamValue(buttonSceneJSONName, menuButtonScene);
                    pluginData.SetBoolParamValue(uiNeedsUpdateJSONName, true);
                    Log("Setting ResetVROrientation Plugin Data: " + menuButtonText + ", " + menuButtonScene);
                } else
                {
                    Log("Already had ResetVROrientation Plugin Data");
                }
            } else
            {
                LogError("Missing ResetVROrientation Data Storable: " + pluginDataJSONName);
            }
        }

        JSONClass BuildResetVROrientationPlugin()
        {
            //the plugin
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(" {");
            sb.Append(" \"id\" : \"" + "PluginManager" + "\",");
            sb.Append(" \"plugins\" : " + "{");
            sb.Append(" \"plugin#0\" : \"Custom/Scripts/Reset VR Orientation/ResetVROrientation.cs\" ");
            sb.Append(" }");
            sb.Append(" }");
            Log("Built ResetVROrientation Plugin String: " + sb.ToString());
            return JSONNode.Parse(sb.ToString()).AsObject;
        }

        void Update()
        {
            SuperController scFsm = SuperController.singleton;
            bool loadingNow =
                scFsm != null && scFsm.isLoading;
            if (!prevSuperLoading && loadingNow)
            {
                EasyMateFemalePassengerRuntime.NotifySceneChanged(this);
            }

            if (retainCameraPoseSameFolderLoads != null &&
                retainCameraPoseSameFolderLoads.val &&
                scFsm != null)
            {
                if (prevSuperLoading && !loadingNow)
                    EasyMateSameFolderCameraRetain.QueueRestoreCoroutineIfNeeded(this);
                if (!prevSuperLoading && loadingNow)
                    EasyMateSameFolderCameraRetain.NotifyLoadBeginning(scFsm);
            }

            prevSuperLoading = loadingNow;

            //once finished loading, apply
            if (SuperController.singleton.isLoading)
            {
                isLoading = true;
                loadingTimeCounter = Time.timeSinceLevelLoad;
            }

            if (isLoading && !SuperController.singleton.isLoading)
            {
                //wait a little, otherwise it reloads plugins multiple times during loading
                if (Time.timeSinceLevelLoad > loadingTimeCounter + 1.0f)
                {
                    isLoading = false;
                    sceneChanged = true;
                }
            }

            if (sceneChanged)
            {
                EasyMateMotionAnimationEmotionEnd.ResetForNewScene();
                sceneChanged = false;
                Log("EasyMate Scene Changed, Load Dir: " + SuperController.singleton.currentLoadDir + ", Time Since Level Load: " + Time.timeSinceLevelLoad);
                //get menu data, if this is a menu
                GetMenuData();

                if (mainUIButtons != null)
                    mainUIButtons.InvalidateCachedPersonLists();

                EasyMateFemalePassengerRuntime.NotifySceneChanged(this);

                ClearAllPossessionIfLoadedSceneHadAny();

                if (mainUIButtons != null)
                {
                    if (_applyEmotionAfterSceneCo != null)
                    {
                        StopCoroutine(_applyEmotionAfterSceneCo);
                        _applyEmotionAfterSceneCo = null;
                    }

                    _applyEmotionAfterSceneCo = StartCoroutine(CoApplyEmotionAfterSceneSettles());
                }

                if (lastLoadDir != SuperController.singleton.currentLoadDir)
                {
                    Log("Load Dir Changed from " + lastLoadDir + " to " + SuperController.singleton.currentLoadDir);
                    lastLoadDir = SuperController.singleton.currentLoadDir;
                    //reset clothes cycling, etc.
                    if (mainUIButtons != null) mainUIButtons.ClothingResetCycle();
                }

                resetVROrientation = SuperController.singleton.GetAtomByUid(resetVROrientationID);
                if (resetVROrientation == null)
                {
                    Log("EasyMate adding ResetVROrientation Atom.");
                    StartCoroutine(CreateResetVROrientationAtom());
                }
                else
                {
                    ConfigureResetVROrientation();
                }

                ApplyRemoteHoldGrabPreference();
                ApplyDefaultMonitorCameraFovIfNeeded();
                EasyMateVrInput.ResetEdgeState();
                ResetSpankingsAutoMergeStateForScene();
                EasyMateGripHandVisibility.DisableVrHandModelsForSceneStart();

                if (headProximityHide != null)
                {
                    EasyMateVrHeadCylinderHide.SetHeadProximityHideEnabled(headProximityHide.val, this);
                }
            }

            if (!SuperController.singleton.isLoading)
            {
                if (mainUIButtons != null)
                    mainUIButtons.ProcessHotkeysUpdate();
            }
        }

        void LateUpdate()
        {
            if (SuperController.singleton == null || SuperController.singleton.isLoading)
                return;
            bool blockOverlap = blockOverlapFullGrab != null && blockOverlapFullGrab.val;
            EasyMateOverlapFullGrabRelease.LateTick(blockOverlap);

            bool gripHands = gripTogglesHandVisibility != null && gripTogglesHandVisibility.val;
            EasyMateGripHandVisibility.LateTick(gripHands);

            if (retainCameraPoseSameFolderLoads != null && retainCameraPoseSameFolderLoads.val)
                EasyMateSameFolderCameraRetain.LateTickIdleCapture(SuperController.singleton);

            bool footDist = possessAutoUnpossessWhenFarFromFeet != null && possessAutoUnpossessWhenFarFromFeet.val;
            float footMax = possessAutoUnpossessFeetMaxHorizontalM != null ? possessAutoUnpossessFeetMaxHorizontalM.val : 1.35f;
            EasyMatePossessFootDistanceAutoRelease.LateTick(footDist, footMax);

            bool mocapEmotionEnd = mergeEmotionWhenLongMocapEndsNoLoop != null && mergeEmotionWhenLongMocapEndsNoLoop.val;
            float mocapMinSec = longMocapMinSecondsForEmotionMerge != null ? longMocapMinSecondsForEmotionMerge.val : 45f;
            EasyMateMotionAnimationEmotionEnd.LateTick(mocapEmotionEnd, mocapMinSec);

            bool monitorLaser = restoreMonitorModeControllerLaser != null && restoreMonitorModeControllerLaser.val;
            EasyMateMonitorModeLaserRestore.Tick(monitorLaser);
            EasyMateVrEulerPossessHandHud.Tick();
            EasyMateFemalePassengerRuntime.Tick(this);
        }

        void OnDestroy()
        {
            if (SuperController.singleton != null)
            {
                SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedPathRuleEmotion;
            }

            if (_pathRuleEmotionMergeCo != null)
            {
                StopCoroutine(_pathRuleEmotionMergeCo);
                _pathRuleEmotionMergeCo = null;
            }

            if (_spankingsAutoMergeProximityCo != null)
            {
                StopCoroutine(_spankingsAutoMergeProximityCo);
                _spankingsAutoMergeProximityCo = null;
            }

            EasyMateGripHandVisibility.SetOnMale2HandsEnabled(null);
            EasyMateMonitorModeLaserRestore.OnPluginDestroy();
            EasyMateVrEulerPossessHandHud.OnPluginDestroy();
            EasyMateFemalePassengerRuntime.OnPluginDestroy();
            EasyMateVrHeadCylinderHide.End();
            if (mainUIButtons != null) mainUIButtons.OnDestroy();
        }

        //NOT USED, ADDING THE OBJECT, THEN ONLY BUILDING THE PLUGIN STRING, NOT THE WHOLE OBJECT STRING
        string BuildFullResetVROrientationJSON()
        {
            string json = " { \"id\" : \"_ResetVROrientation\", \"on\" : \"true\", \"type\" : \"Empty\", " +
                "\"position\" : { \"x\" : \"0.5\", \"y\" : \"1\", \"z\" : \"0\" }, " +
                "\"rotation\" : { \"x\" : \"0\", \"y\" : \"0\", \"z\" : \"0\" }, " +
                "\"containerPosition\" : { \"x\" : \"0.5\", \"y\" : \"1\", \"z\" : \"0\" }, " +
                "\"containerRotation\" : { \"x\" : \"0\", \"y\" : \"0\", \"z\" : \"0\" }," +
                "\"storables\" : [" +
                "{ \"id\" : \"CollisionTrigger\", \"trigger\" : { \"startActions\" : [ ], \"transitionActions\" : [ ], \"endActions\" : [ ] } }," +
                "{ \"id\" : \"PluginManager\", \"plugins\" : { \"plugin#0\" : \"Custom/Scripts/Reset VR Orientation/ResetVROrientation.cs\" } }," +
                "{ \"id\" : \"control\", \"position\" : { \"x\" : \"0.5\", \"y\" : \"1\", \"z\" : \"0\" }, \"rotation\" : { \"x\" : \"0\", \"y\" : \"0\", \"z\" : \"0\" } }," +
                "{ \"id\" : \"plugin#0_geesp0t.ResetVROrientation\", " +
                "\"Show Additional Button (reload scene to see the change)\" : \"true\"," +
                "\"Additional Button Text\" : \"Looks Page 2\"," +
                "\"Additional Button Scene\" : \"Saves/scene/PersonLooksMenu_2.json\"" +
                " } ] }, } ";

            return json;
        }
    }
}