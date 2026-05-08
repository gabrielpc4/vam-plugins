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
    public class GabrielHud : MVRScript
    {
        //Manage the GabrielHud menu system, add Main Menu and other buttons to scenes which are loaded from a menu but don't have any return buttons
        //This is also required for scenes packaged in var files, etc., which we won't modify

        private static bool logMessages = false;

        private GabrielHudButtons mainUIButtons = null; //LOAD PERSON, POSE, ETC.

        public const string resetVROrientationID = "_ResetVROrientation";
        Atom resetVROrientation;

        private string menuDataJSONNodeName = "_Gabriel";
        private string menuDataJSONName = "plugin#0_geesp0t.GabrielBootstrap";
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

        private Coroutine _mergeSpankingsAfterGripCo;

        private Coroutine _mergeClothingTouchFallOffAfterGripCo;

        private Coroutine _mocapEndDefaultSceneCo;

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
        /// (see <see cref="GripHandVisibility"/>); while any <c>Person</c> head or hand is possessed, VR proxies use
        /// <b>None</b> (not sphere / not Male2). Collisions stay off while both
        /// sides sphere. The <b>first</b> such grip press this scene queues a merge of <b>Spankings</b> onto <b>female</b> <c>Person</c> atoms only
        /// that do not already have the plugin (deferred; merge-only), then after <b>4</b> seconds re-checks and merges again if any female still
        /// lacks the plugin — unless <c>Custom/Scripts/Gabriel/features/spankings/spankings_grip_merge_block_path_keywords.txt</c> matches current load/save dirs (same
        /// substring rules as <c>emotion_path_keywords.txt</c>), in which case no grip Spankings merge runs. Hands still toggle regardless.
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

        /// <summary>
        /// Restore navigation rig, monitor orientation, and player height after a
        /// load when VaM stays in the same <see cref="SuperController.currentLoadDir"/>
        /// (e.g. switching between JSON files inside one chapter folder).
        /// </summary>
        public JSONStorableBool retainCameraPoseSameFolderLoads;

        /// <summary>
        /// Hides DillDoe cum Fluid CustomUnityAsset until load completes plus
        /// <see cref="fluidCumRevealDelayRealtimeSeconds"/> (see
        /// <see cref="FluidCumHideDuringSceneLoad"/>).
        /// </summary>
        public JSONStorableBool hideFluidCumMeshUntilAfterLoadDelay;

        /// <summary>
        /// Realtime seconds after <see cref="SuperController.isLoading"/> becomes
        /// false before DillDoe cum mesh / embedded canvases under that atom are
        /// shown again.
        /// </summary>
        public JSONStorableFloat fluidCumRevealDelayRealtimeSeconds;

        private bool prevSuperLoading;

        private const float VamDefaultMonitorCameraFov = 40f;

        private const float GabrielDefaultMonitorCameraFov = 50f;

        private static string NormalizeLoadDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return "";

            string normalized = dir.Replace('\\', '/').Trim();
            while (normalized.Length > 1 && normalized.EndsWith("/"))
                normalized = normalized.Substring(0, normalized.Length - 1);

            return normalized;
        }

        public override void Init()
        {
            Log("GabrielHud Init");
            mainUIButtons = new GabrielHudButtons();
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

            retainCameraPoseSameFolderLoads = new JSONStorableBool(
                "Retain camera pose (loads from same folder)",
                true,
                OnRetainSameFolderPoseChanged);
            RegisterBool(retainCameraPoseSameFolderLoads);
            SameFolderCameraRetain.SetRetainEnabled(
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

            hideFluidCumMeshUntilAfterLoadDelay = new JSONStorableBool(
                "Hide DillDoe cum (Fluid) until after load delay",
                true);
            RegisterBool(hideFluidCumMeshUntilAfterLoadDelay);

            fluidCumRevealDelayRealtimeSeconds = new JSONStorableFloat(
                "Fluid cum: seconds after load before show (realtime)",
                10f,
                0f,
                120f);
            RegisterFloat(fluidCumRevealDelayRealtimeSeconds);

            SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedPathRuleEmotion;
            SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChangedPathRuleEmotion;

            GripHandVisibility.SetMergeSpankingsOnFirstGrip(QueueMergeSpankingsAfterGripDeferred);
            GripHandVisibility.SetMergeClothingTouchFallOffOnFirstMale2Grip(
                QueueMergeClothingTouchFallOffAfterGripDeferred);
        }

        private void QueueMergeSpankingsAfterGripDeferred()
        {
            if (mainUIButtons == null)
                return;
            if (SpankingsGripBlockPathKeywords.CurrentSceneBlocksGripSpankingsMerge())
                return;
            if (mergeEmotionWhenLongMocapEndsNoLoop != null &&
                mergeEmotionWhenLongMocapEndsNoLoop.val)
            {
                float mocapMinSec =
                    longMocapMinSecondsForEmotionMerge != null
                    ? longMocapMinSecondsForEmotionMerge.val
                    : 45f;
                if (MotionAnimationEmotionEnd
                    .CurrentSceneBlocksGripSpankingsMerge(mocapMinSec))
                    return;
            }
            if (_mergeSpankingsAfterGripCo != null)
                StopCoroutine(_mergeSpankingsAfterGripCo);
            _mergeSpankingsAfterGripCo = StartCoroutine(CoMergeSpankingsAfterGripDeferred());
        }

        private IEnumerator CoMergeSpankingsAfterGripDeferred()
        {
            try
            {
                yield return null;
                yield return null;
                if (mainUIButtons == null)
                    yield break;
                if (SpankingsGripBlockPathKeywords
                    .CurrentSceneBlocksGripSpankingsMerge())
                    yield break;
                if (mergeEmotionWhenLongMocapEndsNoLoop != null &&
                    mergeEmotionWhenLongMocapEndsNoLoop.val)
                {
                    float mocapMinSec =
                        longMocapMinSecondsForEmotionMerge != null
                        ? longMocapMinSecondsForEmotionMerge.val
                        : 45f;
                    if (MotionAnimationEmotionEnd
                        .CurrentSceneBlocksGripSpankingsMerge(mocapMinSec))
                        yield break;
                }
                mainUIButtons.MergeSpankingsOnFemalePersonsOnly();
                yield return new WaitForSeconds(4f);
                if (mainUIButtons == null)
                    yield break;
                if (SpankingsGripBlockPathKeywords
                    .CurrentSceneBlocksGripSpankingsMerge())
                    yield break;
                if (mergeEmotionWhenLongMocapEndsNoLoop != null &&
                    mergeEmotionWhenLongMocapEndsNoLoop.val)
                {
                    float mocapMinSec =
                        longMocapMinSecondsForEmotionMerge != null
                        ? longMocapMinSecondsForEmotionMerge.val
                        : 45f;
                    if (MotionAnimationEmotionEnd
                        .CurrentSceneBlocksGripSpankingsMerge(mocapMinSec))
                        yield break;
                }
                if (mainUIButtons.AnyFemalePersonMissingSpankings())
                    mainUIButtons.MergeSpankingsOnFemalePersonsOnly();
            }
            finally
            {
                _mergeSpankingsAfterGripCo = null;
            }
        }

        private void QueueMergeClothingTouchFallOffAfterGripDeferred()
        {
            if (mainUIButtons == null)
                return;
            if (CurrentSceneHasLongNonLoopMocap())
                return;
            if (_mergeClothingTouchFallOffAfterGripCo != null)
                StopCoroutine(_mergeClothingTouchFallOffAfterGripCo);
            _mergeClothingTouchFallOffAfterGripCo =
                StartCoroutine(CoMergeClothingTouchFallOffAfterGripDeferred());
        }

        private IEnumerator CoMergeClothingTouchFallOffAfterGripDeferred()
        {
            try
            {
                yield return null;
                yield return null;
                if (mainUIButtons == null)
                    yield break;
                if (CurrentSceneHasLongNonLoopMocap())
                    yield break;
                mainUIButtons.MergeClothingTouchFallOffOnAllPersonsOnly();
                mainUIButtons.RefreshPluginToggleLabels();
            }
            finally
            {
                _mergeClothingTouchFallOffAfterGripCo = null;
            }
        }

        private bool CurrentSceneHasLongNonLoopMocap()
        {
            float mocapMinSec =
                longMocapMinSecondsForEmotionMerge != null
                ? longMocapMinSecondsForEmotionMerge.val
                : 45f;

            return MotionAnimationEmotionEnd
                .CurrentSceneUsesLongNonLoopMocap(mocapMinSec);
        }

        /// <summary>
        /// Called by <see cref="MotionAnimationEmotionEnd"/> after mocap-end
        /// is detected; waits realtime then loads Default.json.
        /// </summary>
        public void StartDelayedMocapEndDefaultScene()
        {
            if (_mocapEndDefaultSceneCo != null)
            {
                StopCoroutine(_mocapEndDefaultSceneCo);
                _mocapEndDefaultSceneCo = null;
            }

            _mocapEndDefaultSceneCo =
                StartCoroutine(CoDelayedMocapEndDefaultScene());
        }

        private IEnumerator CoDelayedMocapEndDefaultScene()
        {
            try
            {
                yield return new WaitForSecondsRealtime(
                    MotionAnimationEmotionEnd
                        .MocapEndToDefaultSceneRealtimeDelaySeconds);
                MotionAnimationEmotionEnd.ExecuteDeferredDefaultSceneLoad();
            }
            finally
            {
                _mocapEndDefaultSceneCo = null;
            }
        }

        private void OnHeadProximityHideChanged(bool v)
        {
            HeadProximityHide.SetHeadProximityHideEnabled(v, this);
        }

        private void OnDisableRemoteGripHandLinkChanged(bool v)
        {
            ApplyRemoteHoldGrabPreference();
        }

        private void OnRetainSameFolderPoseChanged(bool v)
        {
            SameFolderCameraRetain.SetRetainEnabled(v);
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

            sc.monitorCameraFOV = GabrielDefaultMonitorCameraFov;
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
            Log("GabrielHud Start");
            if (mainUIButtons != null) mainUIButtons.Start();
            ApplyRemoteHoldGrabPreference();
            if (headProximityHide != null)
                HeadProximityHide.SetHeadProximityHideEnabled(headProximityHide.val, this);
            StartCoroutine(CoRefreshHeadProximityHooksAfterStartFrames());
            GripHandVisibility.DisableVrHandModelsForSceneStart();
            MotionAnimationEmotionEnd.ResetForNewScene();
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

            HeadProximityHide.SetHeadProximityHideEnabled(headProximityHide.val, this);
        }

        /// <summary>
        /// Person plugin lists can restore over several frames; merge E-MotionLite when load/save paths match keywords in
        /// <see cref="EmotionPathKeywords.KeywordsFileRelative"/> (see <see cref="EmotionPathKeywords"/>); refresh HUD.
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

                bool pathRuleMerge = EmotionPathKeywords.MatchesCurrentScenePath();

                if (pathRuleMerge)
                    mainUIButtons.MergeEmotionLiteForPathRuleOnAllPersonsOnly();
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
                PassengerRuntime.NotifyAtomUidsChanged(
                    atomUids,
                    this);

                if (atomUids == null || atomUids.Count == 0)
                    return;
                if (!EmotionPathKeywords.MatchesCurrentScenePath())
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
                LogError("GabrielHud path-rule E-Motion (atom UID change): " + e.Message);
            }
        }

        private void StartPathRuleEmotionMergeDeferred()
        {
            if (!EmotionPathKeywords.MatchesCurrentScenePath())
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
                if (!EmotionPathKeywords.MatchesCurrentScenePath())
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
                GabrielHudButtons.RequestClearAllPossession(
                    "GabrielHud: ClearPossess after scene load (possession was active).",
                    advanceVrPalmHudGenderCycle: false);
            }
            catch (Exception e)
            {
                LogError("GabrielHud ClearPossess on scene load failed: " + e.Message);
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

            if (mainUIButtons != null)
                mainUIButtons.ProcessHotkeysUpdate();

            if (!prevSuperLoading && loadingNow)
            {
                PassengerRuntime.NotifySceneChanged(this);
            }

            bool fluidCumHide =
                hideFluidCumMeshUntilAfterLoadDelay != null &&
                hideFluidCumMeshUntilAfterLoadDelay.val;
            float fluidCumDelay =
                fluidCumRevealDelayRealtimeSeconds != null
                    ? fluidCumRevealDelayRealtimeSeconds.val
                    : 10f;
            FluidCumHideDuringSceneLoad.Tick(
                fluidCumHide,
                loadingNow,
                fluidCumDelay);

            if (retainCameraPoseSameFolderLoads != null &&
                retainCameraPoseSameFolderLoads.val &&
                scFsm != null)
            {
                if (prevSuperLoading && !loadingNow)
                    SameFolderCameraRetain.QueueRestoreCoroutineIfNeeded(this);
                if (!prevSuperLoading && loadingNow)
                    SameFolderCameraRetain.NotifyLoadBeginning(scFsm);
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
                MotionAnimationEmotionEnd.ResetForNewScene();
                sceneChanged = false;
                Log("GabrielHud Scene Changed, Load Dir: " + SuperController.singleton.currentLoadDir + ", Time Since Level Load: " + Time.timeSinceLevelLoad);
                string currentLoadDirNorm =
                    NormalizeLoadDir(SuperController.singleton.currentLoadDir);
                bool sameFolderLoad =
                    currentLoadDirNorm.Length > 0 &&
                    string.Equals(
                        NormalizeLoadDir(lastLoadDir),
                        currentLoadDirNorm,
                        StringComparison.OrdinalIgnoreCase);
                //get menu data, if this is a menu
                GetMenuData();

                if (mainUIButtons != null)
                    mainUIButtons.InvalidateCachedPersonLists();

                PassengerRuntime.NotifySceneChanged(this);

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

                if (!sameFolderLoad)
                {
                    Log("Load Dir Changed from " + lastLoadDir + " to " + SuperController.singleton.currentLoadDir);
                    lastLoadDir = SuperController.singleton.currentLoadDir;
                    //reset clothes cycling, etc.
                    if (mainUIButtons != null) mainUIButtons.ClothingResetCycle();
                }

                resetVROrientation = SuperController.singleton.GetAtomByUid(resetVROrientationID);
                if (resetVROrientation == null)
                {
                    Log("GabrielHud adding ResetVROrientation Atom.");
                    StartCoroutine(CreateResetVROrientationAtom());
                }
                else
                {
                    ConfigureResetVROrientation();
                }

                ApplyRemoteHoldGrabPreference();
                ApplyDefaultMonitorCameraFovIfNeeded();
                VrInput.ResetEdgeState();
                GripHandVisibility.DisableVrHandModelsForSceneStart(
                    sameFolderLoad);

                if (headProximityHide != null)
                {
                    HeadProximityHide.SetHeadProximityHideEnabled(headProximityHide.val, this);
                }
            }

        }

        void LateUpdate()
        {
            if (SuperController.singleton == null || SuperController.singleton.isLoading)
                return;
            bool blockOverlap = blockOverlapFullGrab != null && blockOverlapFullGrab.val;
            OverlapFullGrabRelease.LateTick(blockOverlap);

            bool gripHands = gripTogglesHandVisibility != null && gripTogglesHandVisibility.val;
            GripHandVisibility.LateTick(gripHands);

            if (retainCameraPoseSameFolderLoads != null && retainCameraPoseSameFolderLoads.val)
                SameFolderCameraRetain.LateTickIdleCapture(SuperController.singleton);

            bool mocapEmotionEnd = mergeEmotionWhenLongMocapEndsNoLoop != null && mergeEmotionWhenLongMocapEndsNoLoop.val;
            float mocapMinSec = longMocapMinSecondsForEmotionMerge != null ? longMocapMinSecondsForEmotionMerge.val : 45f;
            MotionAnimationEmotionEnd.LateTick(
                mocapEmotionEnd,
                mocapMinSec,
                this);

            bool monitorLaser = restoreMonitorModeControllerLaser != null && restoreMonitorModeControllerLaser.val;
            MonitorModeLaserRestore.Tick(monitorLaser);
            VrEulerPossessHandHud.Tick();
            PassengerRuntime.Tick(this);
        }

        void OnDestroy()
        {
            FluidCumHideDuringSceneLoad.OnPluginDestroy();

            if (SuperController.singleton != null)
            {
                SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedPathRuleEmotion;
            }

            if (_pathRuleEmotionMergeCo != null)
            {
                StopCoroutine(_pathRuleEmotionMergeCo);
                _pathRuleEmotionMergeCo = null;
            }

            if (_mergeSpankingsAfterGripCo != null)
            {
                StopCoroutine(_mergeSpankingsAfterGripCo);
                _mergeSpankingsAfterGripCo = null;
            }

            if (_mergeClothingTouchFallOffAfterGripCo != null)
            {
                StopCoroutine(_mergeClothingTouchFallOffAfterGripCo);
                _mergeClothingTouchFallOffAfterGripCo = null;
            }

            if (_mocapEndDefaultSceneCo != null)
            {
                StopCoroutine(_mocapEndDefaultSceneCo);
                _mocapEndDefaultSceneCo = null;
            }

            GripHandVisibility.SetMergeSpankingsOnFirstGrip(null);
            GripHandVisibility.SetMergeClothingTouchFallOffOnFirstMale2Grip(null);
            MonitorModeLaserRestore.OnPluginDestroy();
            VrEulerPossessHandHud.OnPluginDestroy();
            PassengerRuntime.OnPluginDestroy();
            HeadProximityHide.End();
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