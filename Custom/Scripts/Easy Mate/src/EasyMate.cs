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

        private Coroutine _maleEmotionOnNewPersonCo;

        private Coroutine _mergeSpankingsAfterGripCo;

        private Coroutine _postSceneLoadUnfreezeCo;

        private bool _prevSuperLoading;

        private bool _sceneLoadFreezeCheckboxSnapshot;

        private bool _sceneLoadFreezeAppliedForCurrentLoad;

        private bool _sceneLoadFreezeWatchPrimed;

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
        /// (see <see cref="EasyMateGripHandVisibility"/>); a possessed hand side stays sphere. Collisions stay off while both
        /// sides sphere. The <b>first</b> such grip press this scene queues a merge of <b>Spankings</b> onto <b>female</b> <c>Person</c> atoms only
        /// that do not already have the plugin (deferred; merge-only), then after <b>4</b> seconds re-checks and merges again if any female still
        /// lacks the plugin — unless <c>Custom/Scripts/Easy Mate/spankings_grip_merge_block_path_keywords.txt</c> matches current load/save dirs (same
        /// substring rules as <c>emotion_path_keywords.txt</c>), in which case no grip Spankings merge runs. Hands still toggle regardless.
        /// </summary>
        public JSONStorableBool gripTogglesHandVisibility;

        /// <summary>
        /// When true (default), calls <see cref="FreeControllerV3.RestorePreLinkState"/> on active full-grab targets
        /// each <see cref="LateUpdate"/> when not using remote hold-grab (public <see cref="SuperController"/> API only).
        /// </summary>
        public JSONStorableBool blockOverlapFullGrab;

        /// <summary>
        /// When true (default), HMD inside any Person’s head cylinder hides face/hair/glasses without using Snap F/M.
        /// </summary>
        public JSONStorableBool headProximityHideWithoutSnap;

        /// <summary>
        /// When true (default), clears all possession if the look camera moves farther than
        /// <see cref="possessAutoUnpossessFeetMaxHorizontalM"/> (meters) from the possessed character’s feet in the horizontal plane (rig up).
        /// </summary>
        public JSONStorableBool possessAutoUnpossessWhenFarFromFeet;

        /// <summary>When true (default), after a non-looping scene mocap at least <see cref="longMocapMinSecondsForEmotionMerge"/> long finishes, merge E-Motion Final onto female Persons once (uses <see cref="SuperController.motionAnimationMaster"/>).</summary>
        public JSONStorableBool mergeEmotionWhenLongMocapEndsNoLoop;

        /// <summary>Minimum longest <see cref="MotionAnimationClip.clipLength"/> in the scene (seconds) for end-of-mocap female E-Motion Final merge; avoids short clips.</summary>
        public JSONStorableFloat longMocapMinSecondsForEmotionMerge;

        /// <summary>
        /// After scene load (and when males are added mid-scene), merges <see cref="MainUIButtons.PluginEMotion"/> onto male <c>Person</c> atoms only when they have no scene motion-animation clip targeting head,
        /// neck, or eye target (<see cref="EasyMateHeadFaceMotionProbe"/>). Toggle off to leave males unchanged.
        /// </summary>
        public JSONStorableBool mergeEmotionOriginalOnMaleWithoutHeadFaceMocap;

        /// <summary>
        /// When true (default), draws blue/red <see cref="LineRenderer"/> beams from each VR
        /// controller to that side’s <c>LaserBeamDot</c> while main monitor mode is on (stock UI
        /// mesh lasers often disappear there). Off when monitor mode is off.
        /// </summary>
        public JSONStorableBool restoreMonitorModeControllerLaser;

        /// <summary>Horizontal distance threshold from look camera to feet midpoint for <see cref="possessAutoUnpossessWhenFarFromFeet"/>.</summary>
        public JSONStorableFloat possessAutoUnpossessFeetMaxHorizontalM;

        /// <summary>
        /// When true (default), snapshots the Animation “Freeze Animations / Sound”
        /// checkbox when a load starts, turns freeze on for the load, then restores
        /// the snapshot after <see cref="sceneLoadUnfreezeDelaySeconds"/> realtime
        /// once loading finishes.
        /// </summary>
        public JSONStorableBool autoFreezeAnimAndSoundBrieflyAfterSceneLoad;

        /// <summary>
        /// Realtime delay after loading ends before restoring the Animation freeze
        /// checkbox.
        /// </summary>
        public JSONStorableFloat sceneLoadUnfreezeDelaySeconds;

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

            headProximityHideWithoutSnap = new JSONStorableBool("VR head proximity hide (no Snap required)", true, OnHeadProximityHideWithoutSnapChanged);
            RegisterBool(headProximityHideWithoutSnap);

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

            autoFreezeAnimAndSoundBrieflyAfterSceneLoad =
                new JSONStorableBool(
                    "Freeze animations/audio during scene loads",
                    true);
            RegisterBool(autoFreezeAnimAndSoundBrieflyAfterSceneLoad);

            sceneLoadUnfreezeDelaySeconds = new JSONStorableFloat(
                "Seconds after load before unfreeze (realtime)",
                3f,
                0f,
                60f,
                true,
                true);
            RegisterFloat(sceneLoadUnfreezeDelaySeconds);

            mergeEmotionWhenLongMocapEndsNoLoop = new JSONStorableBool("Merge E-Motion Final on females when long mocap ends (no loop)", true);
            RegisterBool(mergeEmotionWhenLongMocapEndsNoLoop);

            longMocapMinSecondsForEmotionMerge = new JSONStorableFloat("Min mocap length (s) for end-of-clip E-Motion Final", 45f, 5f, 600f);
            RegisterFloat(longMocapMinSecondsForEmotionMerge);

            mergeEmotionOriginalOnMaleWithoutHeadFaceMocap = new JSONStorableBool(
                "Auto-merge E-Motion Original on males (no head/neck/gaze mocap)",
                true);
            RegisterBool(mergeEmotionOriginalOnMaleWithoutHeadFaceMocap);

            restoreMonitorModeControllerLaser = new JSONStorableBool(
                "Monitor mode: LineRenderer to LaserBeamDot",
                true);
            RegisterBool(restoreMonitorModeControllerLaser);

            SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedPathRuleEmotion;
            SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChangedPathRuleEmotion;
            SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedMaleEmotionMerge;
            SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChangedMaleEmotionMerge;

            EasyMateGripHandVisibility.SetMergeSpankingsOnFirstGrip(QueueMergeSpankingsAfterGripDeferred);
        }

        private void QueueMergeSpankingsAfterGripDeferred()
        {
            if (mainUIButtons == null)
                return;
            if (EasyMateSpankingsGripBlockPathKeywords.CurrentSceneBlocksGripSpankingsMerge())
                return;
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
                if (EasyMateSpankingsGripBlockPathKeywords
                    .CurrentSceneBlocksGripSpankingsMerge())
                    yield break;
                mainUIButtons.MergeSpankingsOnFemalePersonsOnly();
                yield return new WaitForSeconds(4f);
                if (mainUIButtons == null)
                    yield break;
                if (EasyMateSpankingsGripBlockPathKeywords
                    .CurrentSceneBlocksGripSpankingsMerge())
                    yield break;
                if (mainUIButtons.AnyFemalePersonMissingSpankings())
                    mainUIButtons.MergeSpankingsOnFemalePersonsOnly();
            }
            finally
            {
                _mergeSpankingsAfterGripCo = null;
            }
        }

        private void OnHeadProximityHideWithoutSnapChanged(bool v)
        {
            EasyMateHeadSnapPovRuntime.SetHeadProximityHideWithoutSnapEnabled(v, this);
        }

        private void OnDisableRemoteGripHandLinkChanged(bool v)
        {
            ApplyRemoteHoldGrabPreference();
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
            if (headProximityHideWithoutSnap != null)
                EasyMateHeadSnapPovRuntime.SetHeadProximityHideWithoutSnapEnabled(headProximityHideWithoutSnap.val, this);
            EasyMateGripHandVisibility.DisableVrHandModelsForSceneStart();
            EasyMateMotionAnimationEmotionEnd.ResetForNewScene();
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
                if (mergeEmotionOriginalOnMaleWithoutHeadFaceMocap != null &&
                    mergeEmotionOriginalOnMaleWithoutHeadFaceMocap.val)
                    mainUIButtons.MergeEmotionOriginalOnMalePersonsWithoutHeadFaceMotionClip();
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
                if (mergeEmotionOriginalOnMaleWithoutHeadFaceMocap != null &&
                    mergeEmotionOriginalOnMaleWithoutHeadFaceMocap.val)
                    mainUIButtons.MergeEmotionOriginalOnMalePersonsWithoutHeadFaceMotionClip();
                mainUIButtons.RefreshPluginToggleLabels();
            }
            finally
            {
                _pathRuleEmotionMergeCo = null;
            }
        }

        private void OnAtomUIDsChangedMaleEmotionMerge(List<string> atomUids)
        {
            try
            {
                if (mergeEmotionOriginalOnMaleWithoutHeadFaceMocap == null ||
                    !mergeEmotionOriginalOnMaleWithoutHeadFaceMocap.val)
                    return;
                if (atomUids == null || atomUids.Count == 0)
                    return;

                SuperController sc = SuperController.singleton;
                if (sc == null || sc.isLoading)
                    return;

                bool sawMalePerson = false;
                for (int i = 0; i < atomUids.Count; i++)
                {
                    Atom a = sc.GetAtomByUid(atomUids[i]);
                    if (a != null && a.type == "Person" && MainUIButtons.IsMalePerson(a))
                    {
                        sawMalePerson = true;
                        break;
                    }
                }

                if (!sawMalePerson)
                    return;

                StartMaleEmotionMergeDeferred();
            }
            catch (Exception e)
            {
                LogError("EasyMate male E-Motion (atom UID change): " + e.Message);
            }
        }

        private void StartMaleEmotionMergeDeferred()
        {
            if (mergeEmotionOriginalOnMaleWithoutHeadFaceMocap == null ||
                !mergeEmotionOriginalOnMaleWithoutHeadFaceMocap.val)
                return;
            if (_maleEmotionOnNewPersonCo != null)
            {
                StopCoroutine(_maleEmotionOnNewPersonCo);
                _maleEmotionOnNewPersonCo = null;
            }
            _maleEmotionOnNewPersonCo = StartCoroutine(CoMaleEmotionMergeDeferred());
        }

        private IEnumerator CoMaleEmotionMergeDeferred()
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
                if (mergeEmotionOriginalOnMaleWithoutHeadFaceMocap == null ||
                    !mergeEmotionOriginalOnMaleWithoutHeadFaceMocap.val)
                    yield break;

                mainUIButtons.MergeEmotionOriginalOnMalePersonsWithoutHeadFaceMotionClip();
                mainUIButtons.RefreshPluginToggleLabels();
            }
            finally
            {
                _maleEmotionOnNewPersonCo = null;
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

        private static bool SnapshotUserFreezeAnimationCheckbox(SuperController sc)
        {
            if (sc == null)
                return false;
            if (sc.freezeAnimationToggle != null)
                return sc.freezeAnimationToggle.isOn;
            if (sc.freezeAnimationToggleAlt != null)
                return sc.freezeAnimationToggleAlt.isOn;
            return false;
        }

        /// <summary>
        /// Stops delayed unfreeze. If restoring, reapplies checkbox snapshot so a
        /// new load’s Snapshot reflects user intent rather than leftover forced freeze.
        /// </summary>
        private void CancelPendingPostSceneLoadUnfreeze(bool restoreSnapshotIfApplicable)
        {
            if (_postSceneLoadUnfreezeCo != null)
            {
                StopCoroutine(_postSceneLoadUnfreezeCo);
                _postSceneLoadUnfreezeCo = null;
            }

            if (restoreSnapshotIfApplicable &&
                _sceneLoadFreezeAppliedForCurrentLoad &&
                SuperController.singleton != null)
            {
                SuperController.singleton.SetFreezeAnimation(
                    _sceneLoadFreezeCheckboxSnapshot);
            }

            _sceneLoadFreezeAppliedForCurrentLoad = false;
        }

        private IEnumerator CoUnfreezeAnimationAfterDelay(float delayRealtimeSeconds)
        {
            if (delayRealtimeSeconds > 0f)
                yield return new WaitForSecondsRealtime(delayRealtimeSeconds);

            SuperController sc2 = SuperController.singleton;
            if (sc2 != null)
                sc2.SetFreezeAnimation(_sceneLoadFreezeCheckboxSnapshot);
            _sceneLoadFreezeAppliedForCurrentLoad = false;
            _postSceneLoadUnfreezeCo = null;
        }

        /// <summary>
        /// Syncs <see cref="SuperController.SetFreezeAnimation"/> with load start/end.
        /// </summary>
        private void TickPostSceneLoadFreezeAnimAndSound(SuperController sc)
        {
            if (sc == null)
                return;
            bool nowLoading = sc.isLoading;

            if (!_sceneLoadFreezeWatchPrimed)
            {
                _prevSuperLoading = nowLoading;
                _sceneLoadFreezeWatchPrimed = true;
                return;
            }

            bool useSync = autoFreezeAnimAndSoundBrieflyAfterSceneLoad != null &&
                autoFreezeAnimAndSoundBrieflyAfterSceneLoad.val;

            if (nowLoading && !_prevSuperLoading)
            {
                CancelPendingPostSceneLoadUnfreeze(true);
                if (useSync)
                {
                    _sceneLoadFreezeCheckboxSnapshot =
                        SnapshotUserFreezeAnimationCheckbox(sc);
                    _sceneLoadFreezeAppliedForCurrentLoad = true;
                    sc.SetFreezeAnimation(true);
                }
            }
            else if (!nowLoading && _prevSuperLoading)
            {
                if (_sceneLoadFreezeAppliedForCurrentLoad)
                {
                    float delaySec =
                        sceneLoadUnfreezeDelaySeconds != null
                            ? sceneLoadUnfreezeDelaySeconds.val
                            : 3f;

                    if (_postSceneLoadUnfreezeCo != null)
                    {
                        StopCoroutine(_postSceneLoadUnfreezeCo);
                        _postSceneLoadUnfreezeCo = null;
                    }

                    if (delaySec <= 0f)
                    {
                        if (SuperController.singleton != null)
                            SuperController.singleton.SetFreezeAnimation(
                                _sceneLoadFreezeCheckboxSnapshot);
                        _sceneLoadFreezeAppliedForCurrentLoad = false;
                    }
                    else
                    {
                        _postSceneLoadUnfreezeCo = StartCoroutine(
                            CoUnfreezeAnimationAfterDelay(delaySec));
                    }
                }
            }

            _prevSuperLoading = nowLoading;
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
                // SuperController.ClearPossess() — see Reference/Assembly-CSharp-decompiled/SuperController.cs
                SuperController.singleton.ClearPossess();
                SuperController.LogMessage("EasyMate: ClearPossess after scene load (possession was active).");
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
            if (scFsm != null)
                TickPostSceneLoadFreezeAnimAndSound(scFsm);

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
                EasyMateVrInput.ResetEdgeState();
                EasyMateGripHandVisibility.DisableVrHandModelsForSceneStart();
            }

            if (!SuperController.singleton.isLoading)
            {
                EasyMateNxtUiQuestThumbstick.Tick(this);
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

            bool footDist = possessAutoUnpossessWhenFarFromFeet != null && possessAutoUnpossessWhenFarFromFeet.val;
            float footMax = possessAutoUnpossessFeetMaxHorizontalM != null ? possessAutoUnpossessFeetMaxHorizontalM.val : 1.35f;
            EasyMatePossessFootDistanceAutoRelease.LateTick(footDist, footMax);

            bool mocapEmotionEnd = mergeEmotionWhenLongMocapEndsNoLoop != null && mergeEmotionWhenLongMocapEndsNoLoop.val;
            float mocapMinSec = longMocapMinSecondsForEmotionMerge != null ? longMocapMinSecondsForEmotionMerge.val : 45f;
            EasyMateMotionAnimationEmotionEnd.LateTick(mocapEmotionEnd, mocapMinSec, mainUIButtons);

            bool monitorLaser = restoreMonitorModeControllerLaser != null && restoreMonitorModeControllerLaser.val;
            EasyMateMonitorModeLaserRestore.Tick(monitorLaser);
        }

        void OnDestroy()
        {
            CancelPendingPostSceneLoadUnfreeze(true);

            if (SuperController.singleton != null)
            {
                SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedPathRuleEmotion;
                SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedMaleEmotionMerge;
            }

            if (_pathRuleEmotionMergeCo != null)
            {
                StopCoroutine(_pathRuleEmotionMergeCo);
                _pathRuleEmotionMergeCo = null;
            }

            if (_maleEmotionOnNewPersonCo != null)
            {
                StopCoroutine(_maleEmotionOnNewPersonCo);
                _maleEmotionOnNewPersonCo = null;
            }

            if (_mergeSpankingsAfterGripCo != null)
            {
                StopCoroutine(_mergeSpankingsAfterGripCo);
                _mergeSpankingsAfterGripCo = null;
            }

            EasyMateGripHandVisibility.SetMergeSpankingsOnFirstGrip(null);
            EasyMateMonitorModeLaserRestore.OnPluginDestroy();
            EasyMateHeadSnapPovRuntime.End();
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