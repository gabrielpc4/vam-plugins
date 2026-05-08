using System;
using System.Collections;
using System.Collections.Generic;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Late-order Gabriel session runtime: scene-settle release, scene-load edge
    /// tracking, per-frame feature ticks, and session-wide toggles that used to
    /// live on <see cref="GabrielHud"/>.
    /// </summary>
    [DefaultExecutionOrder(32000)]
    public class GabrielSessionOrchestrator : MVRScript
    {
        private const string CoreControlAtomUid = "CoreControl";
        private const string GabrielHudClassSuffix = ".GabrielHud";
        private const string ForceReleaseSceneSettleHoldActionName =
            "ForceReleaseSceneSettleHold";
        private const int HudBindRetryFrames = 120;

        private static bool logMessages;

        private GabrielHud _gabrielHud;

        private bool isLoading = true;

        private bool sceneChanged = true;

        private float loadingTimeCounter;

        private string lastLoadDir = "";

        private Coroutine _applyEmotionAfterSceneCo;

        private Coroutine _pathRuleEmotionMergeCo;

        private Coroutine _mergeSpankingsAfterGripCo;

        private Coroutine _mergeClothingTouchFallOffAfterGripCo;

        private Coroutine _animationNoLoopDetectionDeferredDefaultCo;

        private JSONStorableString explanationString;

        private JSONStorableAction forceReleaseSceneSettleHoldAction;

        /// <summary>
        /// When true (default), calls
        /// <see cref="SuperController.DisableRemoteHoldGrab"/> so the VR
        /// <b>grip</b> cannot start the remote hand-link while aiming with the
        /// controller laser.
        /// </summary>
        public JSONStorableBool disableRemoteGripHandLink;

        /// <summary>
        /// When true (default), a short VR grip press toggles both sides between
        /// articulated hands and VaM sphere/kinematic hands; the first such grip
        /// can also queue deferred Spankings merge.
        /// </summary>
        public JSONStorableBool gripTogglesHandVisibility;

        /// <summary>
        /// When true (default), restores pre-link state on overlap full-grab
        /// targets during <see cref="LateUpdate"/>.
        /// </summary>
        public JSONStorableBool blockOverlapFullGrab;

        /// <summary>
        /// When true (default), HMD inside a Person head cylinder hides
        /// face/hair/glasses on VR eye cameras.
        /// </summary>
        public JSONStorableBool headProximityHide;

        /// <summary>
        /// After a long non-looping main scene animation ends, load Default.json.
        /// </summary>
        public JSONStorableBool loadDefaultWhenLongNonLoopAnimationEnds;

        /// <summary>Min dominant clip length for that path (seconds).</summary>
        public JSONStorableFloat minSecondsNonLoopAnimationClipForDefaultSceneLoad;

        public JSONStorableBool restoreMonitorModeControllerLaser;

        public JSONStorableBool retainCameraPoseSameFolderLoads;

        public JSONStorableBool hideFluidCumMeshUntilAfterLoadDelay;

        public JSONStorableFloat fluidCumRevealDelayRealtimeSeconds;

        private bool prevSuperControllerIsLoading;

        // Same-folder pulses come from loads like atom/toy changes and should not
        // re-run the full settle workflow.
        private bool skipSceneSettleWorkflowForPendingLoad;

        private SameFolderSceneLoadCheck sameFolderSceneLoadCheck =
            new SameFolderSceneLoadCheck();

        private SceneSettleRuntime sceneSettle = new SceneSettleRuntime();

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

        internal void RefreshHudPluginToggleLabels()
        {
            GabrielHud hud = ResolveGabrielHud();
            if (hud != null)
            {
                hud.RefreshPluginToggleLabels();
            }
        }

        /// <summary>
        /// Merges
        /// <see cref="ClothingTouchFallOffPluginPath.PersonPlugin"/> onto every
        /// Person atom.
        /// </summary>
        public void MergeClothingTouchFallOffOnAllPersonsOnly()
        {
            try
            {
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    PluginManager.TryMergePluginOntoPerson(
                        at,
                        ClothingTouchFallOffPluginPath.PersonPlugin);
                }
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "Gabriel clothing touch fall-off merge on all Persons: " + e);
            }
        }

        internal void BindGabrielHud(GabrielHud hud)
        {
            _gabrielHud = hud;
            if (_gabrielHud != null)
            {
                GripHandVisibility.SetMergeSpankingsOnFirstGrip(
                    QueueMergeSpankingsAfterGripDeferred);
                GripHandVisibility.SetMergeClothingTouchFallOffOnFirstMale2Grip(
                    QueueMergeClothingTouchFallOffAfterGripDeferred);
            }
            else
            {
                GripHandVisibility.SetMergeSpankingsOnFirstGrip(null);
                GripHandVisibility.SetMergeClothingTouchFallOffOnFirstMale2Grip(
                    null);
            }
        }

        public override void Init()
        {
            explanationString = new JSONStorableString(
                "",
                "Gabriel session orchestrator handles scene-settle playback " +
                "hold, same-folder load suppression, late runtime ticks, " +
                "and shared session toggles.");
            UIDynamicTextField dtext = CreateTextField(explanationString);
            dtext.height = 520;

            forceReleaseSceneSettleHoldAction = new JSONStorableAction(
                ForceReleaseSceneSettleHoldActionName,
                ForceReleaseSceneSettleHoldFromAction);
            RegisterAction(forceReleaseSceneSettleHoldAction);

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

            minSecondsNonLoopAnimationClipForDefaultSceneLoad =
                new JSONStorableFloat(
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

            SuperController sc = SuperController.singleton;
            if (sc != null)
            {
                sc.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedHandlers;
                sc.onAtomUIDsChangedHandlers += OnAtomUIDsChangedHandlers;
            }

            PassengerRuntime.SetSessionPluginHost(this);
            ResolveGabrielHud();
        }

        private IEnumerator CoAnimationNoLoopDetectionDeferredDefault()
        {
            try
            {
                yield return new WaitForSecondsRealtime(
                    AnimationNoLoopDetection
                        .DeferredDefaultSceneRealtimeDelaySeconds);
                AnimationNoLoopDetection.ExecuteDeferredDefaultSceneLoad();
            }
            finally
            {
                _animationNoLoopDetectionDeferredDefaultCo = null;
            }
        }

        private void QueueMergeSpankingsAfterGripDeferred()
        {
            if (SpankingsGripDeferredMerge.ShouldSkipQueue(this))
            {
                return;
            }

            if (_mergeSpankingsAfterGripCo != null)
            {
                StopCoroutine(_mergeSpankingsAfterGripCo);
            }

            _mergeSpankingsAfterGripCo =
                StartCoroutine(WrapTrackSpankingsGripDeferred());
        }

        private IEnumerator WrapTrackSpankingsGripDeferred()
        {
            try
            {
                GabrielHud hud = null;
                yield return StartCoroutine(CoWaitForHudBinding());
                hud = ResolveGabrielHud();
                if (hud == null)
                {
                    yield break;
                }

                yield return StartCoroutine(
                    SpankingsGripDeferredMerge.CoMergeAfterGripDeferred(
                        this,
                        hud));
            }
            finally
            {
                _mergeSpankingsAfterGripCo = null;
            }
        }

        private void QueueMergeClothingTouchFallOffAfterGripDeferred()
        {
            if (AnimationNoLoopDetection.CurrentSceneUsesLongNonLoopAnimation(
                GetMinNonLoopAnimationSecondsForDefaultScene()))
            {
                return;
            }

            if (_mergeClothingTouchFallOffAfterGripCo != null)
            {
                StopCoroutine(_mergeClothingTouchFallOffAfterGripCo);
            }

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
                        this));
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

        void Start()
        {
            Log("GabrielSessionOrchestrator Start");
            ResolveGabrielHud();

            ApplyRemoteHoldGrabPreference();
            if (headProximityHide != null)
            {
                HeadProximityHide.SetHeadProximityHideEnabled(
                    headProximityHide.val,
                    this);
            }

            StartCoroutine(CoRefreshHeadProximityHooksAfterStartFrames());
            GripHandVisibility.DisableVrHandModelsForSceneStart();
            AnimationNoLoopDetection.ResetForNewScene();

            SuperController camSc = SuperController.singleton;
            if (camSc != null)
            {
                DefaultMonitorCameraFov.ApplyGabrielPreferenceIfStillStock(
                    camSc);
            }
        }

        private IEnumerator CoRefreshHeadProximityHooksAfterStartFrames()
        {
            yield return null;
            yield return null;

            if (headProximityHide == null)
            {
                yield break;
            }

            HeadProximityHide.SetHeadProximityHideEnabled(
                headProximityHide.val,
                this);
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
                GabrielHud hud = null;
                yield return StartCoroutine(CoWaitForHudBinding());
                hud = ResolveGabrielHud();
                if (hud == null)
                {
                    yield break;
                }

                yield return StartCoroutine(
                    EmotionPathRuleMerge.CoPathRuleMergeDeferred(hud));
            }
            finally
            {
                _pathRuleEmotionMergeCo = null;
            }
        }

        private IEnumerator CoWaitForHudBinding()
        {
            int frame;
            for (frame = 0; frame < HudBindRetryFrames; frame++)
            {
                if (ResolveGabrielHud() != null)
                {
                    yield break;
                }

                yield return null;
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
                int i;
                for (i = 0; i < atomUids.Count; i++)
                {
                    Atom atom = sc.GetAtomByUid(atomUids[i]);
                    if (atom != null && atom.type == "Person")
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
                    "GabrielSessionOrchestrator path-rule E-Motion " +
                    "(atom UID change): " + e.Message);
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
            bool loadingNow;
            bool fluidCumHide;
            float fluidCumDelay;

            if (scFsm == null)
            {
                return;
            }

            ResolveGabrielHud();
            PassengerRuntime.SetSessionPluginHost(this);

            loadingNow = scFsm.isLoading;
            if (!prevSuperControllerIsLoading && loadingNow)
            {
                skipSceneSettleWorkflowForPendingLoad =
                    sameFolderSceneLoadCheck.IsSameFolderLoad(scFsm);
                PassengerRuntime.NotifySceneChanged(this);
            }
            else if (!loadingNow)
            {
                sameFolderSceneLoadCheck.CaptureIdleLoadDir(scFsm);
            }

            fluidCumHide =
                hideFluidCumMeshUntilAfterLoadDelay != null &&
                hideFluidCumMeshUntilAfterLoadDelay.val;
            fluidCumDelay =
                fluidCumRevealDelayRealtimeSeconds != null
                    ? fluidCumRevealDelayRealtimeSeconds.val
                    : 10f;
            FluidCumHideDuringSceneLoad.Tick(
                fluidCumHide,
                loadingNow,
                fluidCumDelay);

            if (retainCameraPoseSameFolderLoads != null &&
                retainCameraPoseSameFolderLoads.val)
            {
                if (prevSuperControllerIsLoading && !loadingNow)
                    SameFolderCameraRetain.QueueRestoreCoroutineIfNeeded(this);
                if (!prevSuperControllerIsLoading && loadingNow)
                    SameFolderCameraRetain.NotifyLoadBeginning(scFsm);
            }

            prevSuperControllerIsLoading = loadingNow;

            if (loadingNow)
            {
                isLoading = true;
                loadingTimeCounter = Time.timeSinceLevelLoad;
            }

            if (isLoading && !loadingNow)
            {
                if (Time.timeSinceLevelLoad > loadingTimeCounter + 1f)
                {
                    isLoading = false;
                    sceneChanged = true;
                }
            }

            if (sceneChanged)
            {
                GabrielHud hud;
                string currentLoadDirNorm;
                bool sameFolderLoad;

                AnimationNoLoopDetection.ResetForNewScene();
                sceneChanged = false;
                Log(
                    "GabrielSessionOrchestrator Scene Changed, Load Dir: " +
                    scFsm.currentLoadDir +
                    ", Time Since Level Load: " +
                    Time.timeSinceLevelLoad);

                currentLoadDirNorm = SceneLoadDirNormalize.Normalize(
                    scFsm.currentLoadDir);
                sameFolderLoad = SceneLoadDirNormalize.SameFolderLoads(
                    lastLoadDir,
                    currentLoadDirNorm);
                hud = ResolveGabrielHud();

                PersonAtomCache.InvalidatePersonGenderCaches();

                PassengerRuntime.NotifySceneChanged(this);
                SceneLoadPossessionCleanup.ClearPossessionAfterSceneApplyIfHadAny();

                if (_applyEmotionAfterSceneCo != null)
                {
                    StopCoroutine(_applyEmotionAfterSceneCo);
                    _applyEmotionAfterSceneCo = null;
                }

                _applyEmotionAfterSceneCo =
                    StartCoroutine(WrapTrackApplyEmotionAfterScene());

                if (!sameFolderLoad)
                {
                    Log(
                        "Load Dir Changed from " + lastLoadDir + " to " +
                        scFsm.currentLoadDir);
                    lastLoadDir = scFsm.currentLoadDir;
                    if (hud != null)
                    {
                        hud.ClothingResetCycle();
                    }
                }

                ApplyRemoteHoldGrabPreference();
                DefaultMonitorCameraFov.ApplyGabrielPreferenceIfStillStock(scFsm);
                VrInput.ResetEdgeState();
                GripHandVisibility.DisableVrHandModelsForSceneStart(
                    sameFolderLoad);

                if (headProximityHide != null)
                {
                    HeadProximityHide.SetHeadProximityHideEnabled(
                        headProximityHide.val,
                        this);
                }
            }
        }

        private IEnumerator WrapTrackApplyEmotionAfterScene()
        {
            try
            {
                GabrielHud hud = null;
                yield return StartCoroutine(CoWaitForHudBinding());
                hud = ResolveGabrielHud();
                if (hud == null)
                {
                    yield break;
                }

                yield return StartCoroutine(
                    EmotionPathRuleMerge.CoApplyAfterSceneSettles(hud));
            }
            finally
            {
                _applyEmotionAfterSceneCo = null;
            }
        }

        void LateUpdate()
        {
            bool sceneSettleJustEnded;
            SuperController sc;
            bool blockOverlap;
            bool gripHands;
            bool animationNoLoopDetectionLoadDefault;
            float animationMinSec;
            bool monitorLaser;

            sceneSettleJustEnded =
                sceneSettle.TickDuringLoad(
                    skipSceneSettleWorkflowForPendingLoad);
            if (sceneSettleJustEnded)
            {
                HeadProximityHide.AfterSuperControllerFinishedSceneSettle(this);
            }

            sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            PersonAtomCache.PrimeFramePersonPossessionSnapshot(sc);

            blockOverlap =
                blockOverlapFullGrab != null && blockOverlapFullGrab.val;
            OverlapFullGrabRelease.LateTick(blockOverlap);

            gripHands =
                gripTogglesHandVisibility != null &&
                gripTogglesHandVisibility.val;
            GripHandVisibility.LateTick(gripHands);

            if (retainCameraPoseSameFolderLoads != null &&
                retainCameraPoseSameFolderLoads.val)
            {
                SameFolderCameraRetain.LateTickIdleCapture(sc);
            }

            animationNoLoopDetectionLoadDefault =
                loadDefaultWhenLongNonLoopAnimationEnds != null &&
                loadDefaultWhenLongNonLoopAnimationEnds.val;
            animationMinSec = GetMinNonLoopAnimationSecondsForDefaultScene();
            AnimationNoLoopDetection.LateTick(
                animationNoLoopDetectionLoadDefault,
                animationMinSec,
                this);

            monitorLaser =
                restoreMonitorModeControllerLaser != null &&
                restoreMonitorModeControllerLaser.val;
            MonitorModeLaserRestore.Tick(monitorLaser);
            VrEulerPossessHandHud.Tick();
            PassengerRuntime.Tick(this);
        }

        void OnDestroy()
        {
            SuperController atomEventsSc;

            // CoreControl plugin reload runs multiple session MVRScripts; destroy order
            // is undefined — unhook globals and stop all coroutines before static
            // teardown to avoid dangling delegates or LateUpdate overlap.
            try
            {
                FluidCumHideDuringSceneLoad.OnPluginDestroy();

                atomEventsSc = SuperController.singleton;
                if (atomEventsSc != null)
                {
                    atomEventsSc.onAtomUIDsChangedHandlers -=
                        OnAtomUIDsChangedHandlers;
                }

                StopAllCoroutines();
                _applyEmotionAfterSceneCo = null;
                _pathRuleEmotionMergeCo = null;
                _mergeSpankingsAfterGripCo = null;
                _mergeClothingTouchFallOffAfterGripCo = null;
                _animationNoLoopDetectionDeferredDefaultCo = null;

                BindGabrielHud(null);
                sceneSettle.OnPluginDestroy();
                MonitorModeLaserRestore.OnPluginDestroy();
                VrEulerPossessHandHud.OnPluginDestroy();
                PassengerRuntime.OnPluginDestroy();
                HeadProximityHide.End();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "GabrielSessionOrchestrator.OnDestroy (reload teardown): " +
                    e.Message);
            }
        }

        private void ForceReleaseSceneSettleHoldFromAction()
        {
            if (!sceneSettle.ForceReleaseHoldFromShortcut())
            {
                return;
            }

            HeadProximityHide.AfterSuperControllerFinishedSceneSettle(this);
            SuperController.LogMessage(
                "Gabriel session orchestrator: Space released scene settle " +
                "hold.");
        }

        private GabrielHud ResolveGabrielHud()
        {
            Atom coreControl;
            List<string> storableIds;
            int i;
            string storableId;
            JSONStorable storable;
            GabrielHud hud;

            if (_gabrielHud != null)
            {
                return _gabrielHud;
            }

            coreControl = containingAtom;
            if (coreControl == null && SuperController.singleton != null)
            {
                coreControl =
                    SuperController.singleton.GetAtomByUid(CoreControlAtomUid);
            }

            if (coreControl == null)
            {
                return null;
            }

            storableIds = coreControl.GetStorableIDs();
            if (storableIds == null)
            {
                return null;
            }

            for (i = 0; i < storableIds.Count; i++)
            {
                storableId = storableIds[i];
                if (string.IsNullOrEmpty(storableId) ||
                    !storableId.EndsWith(
                        GabrielHudClassSuffix,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                storable = coreControl.GetStorableByID(storableId);
                hud = storable as GabrielHud;
                if (hud == null)
                {
                    continue;
                }

                BindGabrielHud(hud);
                return _gabrielHud;
            }

            return null;
        }
    }
}
