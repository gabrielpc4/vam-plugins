using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geesp0t
{
    /// <summary>Late lifecycle; session toggles; ticks feature glue for palm/lasers,
    /// passenger, etc.</summary>
    [DefaultExecutionOrder(32000)]
    public class GabrielHud : MVRScript
    {
        private static bool logMessages;

        private GabrielHudButtons mainUIButtons;

        private bool isLoading = true;

        private bool sceneChanged = true;

        private float loadingTimeCounter;

        private string lastLoadDir = "";

        private Coroutine _applyEmotionAfterSceneCo;

        private Coroutine _pathRuleEmotionMergeCo;

        private Coroutine _mergeSpankingsAfterGripCo;

        private Coroutine _mergeClothingTouchFallOffAfterGripCo;

        private Coroutine _animationNoLoopDetectionDeferredDefaultCo;

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

        /// <summary>
        /// After non-looping main scene animation runs long enough, load Default.json
        /// (see <see cref="AnimationNoLoopDetection"/>).
        /// </summary>
        public JSONStorableBool loadDefaultWhenLongNonLoopAnimationEnds;

        /// <summary>Min dominant clip length for that path (seconds).</summary>
        public JSONStorableFloat minSecondsNonLoopAnimationClipForDefaultSceneLoad;

        public JSONStorableBool restoreMonitorModeControllerLaser;

        public JSONStorableBool retainCameraPoseSameFolderLoads;

        public JSONStorableBool hideFluidCumMeshUntilAfterLoadDelay;

        public JSONStorableFloat fluidCumRevealDelayRealtimeSeconds;

        private bool prevSuperLoading;

        internal bool IsLoadDefaultOnLongNonLoopAnimationEndEnabled()
        {
            return loadDefaultWhenLongNonLoopAnimationEnds != null &&
                loadDefaultWhenLongNonLoopAnimationEnds.val;
        }

        internal float GetMinNonLoopAnimationSecondsForDefaultScene()
        {
            return minSecondsNonLoopAnimationClipForDefaultSceneLoad != null
                ? minSecondsNonLoopAnimationClipForDefaultSceneLoad.val
                : 45f;
        }

        /// <summary>
        /// Called when <see cref="AnimationNoLoopDetection"/> detects main timeline end.
        /// </summary>
        internal void StartAnimationNoLoopDetectionDeferredDefaultCoroutine()
        {
            if (_animationNoLoopDetectionDeferredDefaultCo != null)
            {
                StopCoroutine(_animationNoLoopDetectionDeferredDefaultCo);
                _animationNoLoopDetectionDeferredDefaultCo = null;
            }

            _animationNoLoopDetectionDeferredDefaultCo =
                StartCoroutine(CoAnimationNoLoopDetectionDeferredDefault());
        }

        private IEnumerator CoAnimationNoLoopDetectionDeferredDefault()
        {
            try
            {
                yield return new WaitForSecondsRealtime(
                    AnimationNoLoopDetection.DeferredDefaultSceneRealtimeDelaySeconds);
                AnimationNoLoopDetection.ExecuteDeferredDefaultSceneLoad();
            }
            finally
            {
                _animationNoLoopDetectionDeferredDefaultCo = null;
            }
        }

        public override void Init()
        {
            Log("GabrielHud Init");
            mainUIButtons = new GabrielHudButtons();
            mainUIButtons.Init(this);

            hideUI = new JSONStorableAction("Hide UI", HideUI);
            RegisterAction(hideUI);
            showUI = new JSONStorableAction("Show UI", ShowUI);
            RegisterAction(showUI);

            disableRemoteGripHandLink = new JSONStorableBool(
                "Disable remote grip hand-link (Quest / OpenVR)",
                true,
                OnDisableRemoteGripHandLinkChanged);
            RegisterBool(disableRemoteGripHandLink);

            gripTogglesHandVisibility = new JSONStorableBool(
                "Grip toggles VR hand visibility",
                true);
            RegisterBool(gripTogglesHandVisibility);

            blockOverlapFullGrab = new JSONStorableBool(
                "Block overlap full-grab (auto-release each frame)",
                true);
            RegisterBool(blockOverlapFullGrab);

            headProximityHide = new JSONStorableBool(
                "VR head proximity hide",
                true,
                OnHeadProximityHideChanged);
            RegisterBool(headProximityHide);

            retainCameraPoseSameFolderLoads = new JSONStorableBool(
                "Retain camera pose (loads from same folder)",
                true,
                OnRetainSameFolderPoseChanged);
            RegisterBool(retainCameraPoseSameFolderLoads);
            SameFolderCameraRetain.SetRetainEnabled(
                retainCameraPoseSameFolderLoads.val);

            loadDefaultWhenLongNonLoopAnimationEnds = new JSONStorableBool(
                "Load Saves/scene/Default.json when long animation ends (no loop)",
                true);
            RegisterBool(loadDefaultWhenLongNonLoopAnimationEnds);

            minSecondsNonLoopAnimationClipForDefaultSceneLoad = new JSONStorableFloat(
                "Min animation length (s) for end-of-clip default scene load",
                45f,
                5f,
                600f);
            RegisterFloat(minSecondsNonLoopAnimationClipForDefaultSceneLoad);

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

            SuperController.singleton.onAtomUIDsChangedHandlers -=
                OnAtomUIDsChangedHandlers;
            SuperController.singleton.onAtomUIDsChangedHandlers +=
                OnAtomUIDsChangedHandlers;

            GripHandVisibility.SetMergeSpankingsOnFirstGrip(
                QueueMergeSpankingsAfterGripDeferred);
            GripHandVisibility.SetMergeClothingTouchFallOffOnFirstMale2Grip(
                QueueMergeClothingTouchFallOffAfterGripDeferred);
        }

        private void QueueMergeSpankingsAfterGripDeferred()
        {
            if (mainUIButtons == null)
                return;
            if (SpankingsGripDeferredMerge.ShouldSkipQueue(this, mainUIButtons))
                return;

            if (_mergeSpankingsAfterGripCo != null)
                StopCoroutine(_mergeSpankingsAfterGripCo);

            _mergeSpankingsAfterGripCo =
                StartCoroutine(WrapTrackSpankingsGripDeferred());
        }

        private IEnumerator WrapTrackSpankingsGripDeferred()
        {
            try
            {
                yield return StartCoroutine(
                    SpankingsGripDeferredMerge.CoMergeAfterGripDeferred(
                        this,
                        mainUIButtons));
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
            float animationMinSec = GetMinNonLoopAnimationSecondsForDefaultScene();
            if (AnimationNoLoopDetection
                .CurrentSceneUsesLongNonLoopAnimation(animationMinSec))
                return;
            if (_mergeClothingTouchFallOffAfterGripCo != null)
                StopCoroutine(_mergeClothingTouchFallOffAfterGripCo);
            _mergeClothingTouchFallOffAfterGripCo =
                StartCoroutine(WrapTrackClothingGripDeferred());
        }

        private IEnumerator WrapTrackClothingGripDeferred()
        {
            try
            {
                yield return StartCoroutine(
                    ClothingTouchFallOffGripMerge.CoMergeAfterGripDeferred(
                        GetMinNonLoopAnimationSecondsForDefaultScene(),
                        mainUIButtons));
            }
            finally
            {
                _mergeClothingTouchFallOffAfterGripCo = null;
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

        public void ShowUI()
        {
            if (mainUIButtons != null)
                mainUIButtons.ShowUI(true);
        }

        public void HideUI()
        {
            if (mainUIButtons != null)
                mainUIButtons.ShowUI(false);
        }

        void Start()
        {
            Log("GabrielHud Start");
            if (mainUIButtons != null)
                mainUIButtons.Start();

            ApplyRemoteHoldGrabPreference();
            if (headProximityHide != null)
                HeadProximityHide.SetHeadProximityHideEnabled(headProximityHide.val, this);

            StartCoroutine(CoRefreshHeadProximityHooksAfterStartFrames());
            GripHandVisibility.DisableVrHandModelsForSceneStart();
            AnimationNoLoopDetection.ResetForNewScene();

            SuperController camSc = SuperController.singleton;
            if (camSc != null)
                DefaultMonitorCameraFov.ApplyGabrielPreferenceIfStillStock(camSc);
        }

        private IEnumerator CoRefreshHeadProximityHooksAfterStartFrames()
        {
            yield return null;
            yield return null;

            if (headProximityHide == null)
                yield break;

            HeadProximityHide.SetHeadProximityHideEnabled(headProximityHide.val, this);
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
            _pathRuleEmotionMergeCo =
                StartCoroutine(WrapTrackPathRuleMergeDeferred());
        }

        private IEnumerator WrapTrackPathRuleMergeDeferred()
        {
            try
            {
                yield return StartCoroutine(
                    EmotionPathRuleMerge.CoPathRuleMergeDeferred(mainUIButtons));
            }
            finally
            {
                _pathRuleEmotionMergeCo = null;
            }
        }

        private void OnAtomUIDsChangedHandlers(List<string> atomUids)
        {
            try
            {
                PassengerRuntime.NotifyAtomUidsChanged(atomUids, this);

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
                LogError(
                    "GabrielHud path-rule E-Motion (atom UID change): " + e.Message);
            }
        }

        private static void LogError(string error)
        {
            SuperController.LogError(error);
        }

        private void Log(string message)
        {
            if (logMessages)
                SuperController.LogMessage(message);
        }

        void Update()
        {
            SuperController scFsm = SuperController.singleton;
            bool loadingNow =
                scFsm != null && scFsm.isLoading;

            if (mainUIButtons != null)
                mainUIButtons.ProcessHotkeysUpdate();

            if (!prevSuperLoading && loadingNow)
                PassengerRuntime.NotifySceneChanged(this);

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

            if (SuperController.singleton.isLoading)
            {
                isLoading = true;
                loadingTimeCounter = Time.timeSinceLevelLoad;
            }

            if (isLoading && !SuperController.singleton.isLoading)
            {
                if (Time.timeSinceLevelLoad > loadingTimeCounter + 1.0f)
                {
                    isLoading = false;
                    sceneChanged = true;
                }
            }

            if (sceneChanged)
            {
                AnimationNoLoopDetection.ResetForNewScene();
                sceneChanged = false;
                Log(
                    "GabrielHud Scene Changed, Load Dir: "
                    + SuperController.singleton.currentLoadDir
                    + ", Time Since Level Load: "
                    + Time.timeSinceLevelLoad);

                string currentLoadDirNorm = SceneLoadDirNormalize.Normalize(
                    SuperController.singleton.currentLoadDir);

                bool sameFolderLoad =
                    SceneLoadDirNormalize.SameFolderLoads(lastLoadDir, currentLoadDirNorm);

                if (mainUIButtons != null)
                    mainUIButtons.InvalidateCachedPersonLists();

                PassengerRuntime.NotifySceneChanged(this);

                SceneLoadPossessionCleanup.ClearPossessionAfterSceneApplyIfHadAny();

                if (mainUIButtons != null)
                {
                    if (_applyEmotionAfterSceneCo != null)
                    {
                        StopCoroutine(_applyEmotionAfterSceneCo);
                        _applyEmotionAfterSceneCo = null;
                    }

                    _applyEmotionAfterSceneCo =
                        StartCoroutine(WrapTrackApplyEmotionAfterScene());
                }

                if (!sameFolderLoad)
                {
                    Log(
                        "Load Dir Changed from " + lastLoadDir + " to "
                        + SuperController.singleton.currentLoadDir);
                    lastLoadDir =
                        SuperController.singleton.currentLoadDir;
                    if (mainUIButtons != null)
                        mainUIButtons.ClothingResetCycle();
                }

                ApplyRemoteHoldGrabPreference();
                if (scFsm != null)
                    DefaultMonitorCameraFov.ApplyGabrielPreferenceIfStillStock(scFsm);
                VrInput.ResetEdgeState();
                GripHandVisibility.DisableVrHandModelsForSceneStart(
                    sameFolderLoad);

                if (headProximityHide != null)
                    HeadProximityHide.SetHeadProximityHideEnabled(headProximityHide.val, this);
            }
        }

        private IEnumerator WrapTrackApplyEmotionAfterScene()
        {
            try
            {
                yield return StartCoroutine(
                    EmotionPathRuleMerge.CoApplyAfterSceneSettles(mainUIButtons));
            }
            finally
            {
                _applyEmotionAfterSceneCo = null;
            }
        }

        void LateUpdate()
        {
            if (SuperController.singleton == null || SuperController.singleton.isLoading)
                return;

            bool blockOverlap =
                blockOverlapFullGrab != null && blockOverlapFullGrab.val;

            OverlapFullGrabRelease.LateTick(blockOverlap);

            bool gripHands =
                gripTogglesHandVisibility != null &&
                gripTogglesHandVisibility.val;

            GripHandVisibility.LateTick(gripHands);

            if (retainCameraPoseSameFolderLoads != null &&
                retainCameraPoseSameFolderLoads.val)
                SameFolderCameraRetain.LateTickIdleCapture(SuperController.singleton);

            bool animationNoLoopDetectionLoadDefault =
                loadDefaultWhenLongNonLoopAnimationEnds != null &&
                loadDefaultWhenLongNonLoopAnimationEnds.val;

            float animationMinSec =
                minSecondsNonLoopAnimationClipForDefaultSceneLoad != null
                    ? minSecondsNonLoopAnimationClipForDefaultSceneLoad.val
                    : 45f;

            AnimationNoLoopDetection.LateTick(
                animationNoLoopDetectionLoadDefault,
                animationMinSec,
                this);

            bool monitorLaser =
                restoreMonitorModeControllerLaser != null &&
                restoreMonitorModeControllerLaser.val;

            MonitorModeLaserRestore.Tick(monitorLaser);
            VrEulerPossessHandHud.Tick();
            PassengerRuntime.Tick(this);
        }

        void OnDestroy()
        {
            FluidCumHideDuringSceneLoad.OnPluginDestroy();

            if (SuperController.singleton != null)
                SuperController.singleton.onAtomUIDsChangedHandlers -=
                    OnAtomUIDsChangedHandlers;

            if (_applyEmotionAfterSceneCo != null)
            {
                StopCoroutine(_applyEmotionAfterSceneCo);
                _applyEmotionAfterSceneCo = null;
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

            if (_animationNoLoopDetectionDeferredDefaultCo != null)
            {
                StopCoroutine(_animationNoLoopDetectionDeferredDefaultCo);
                _animationNoLoopDetectionDeferredDefaultCo = null;
            }

            GripHandVisibility.SetMergeSpankingsOnFirstGrip(null);
            GripHandVisibility.SetMergeClothingTouchFallOffOnFirstMale2Grip(null);
            MonitorModeLaserRestore.OnPluginDestroy();
            VrEulerPossessHandHud.OnPluginDestroy();
            PassengerRuntime.OnPluginDestroy();
            HeadProximityHide.End();

            if (mainUIButtons != null)
                mainUIButtons.OnDestroy();
        }
    }
}