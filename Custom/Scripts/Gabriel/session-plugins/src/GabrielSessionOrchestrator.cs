using System;
using System.Collections;
using System.Collections.Generic;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Late-order Gabriel session runtime: scene-settle release, scene-load edge
    /// tracking, per-frame feature ticks, and built-in defaults that used to
    /// live on <see cref="GabrielHud"/>.
    /// </summary>
    public class GabrielSessionOrchestrator : MVRScript
    {
        private const string CoreControlAtomUid = "CoreControl";
        private const string GabrielHudClassSuffix = ".GabrielHud";
        private const string ForceReleaseSceneSettleHoldActionName =
            "ForceReleaseSceneSettleHold";
        private const string ClothingTouchFallOffPersonPluginPath =
            "Custom/Scripts/Gabriel/features/clothing-interactions/" +
            "ClothingTouchFallOff.cslist";
        private const int HudBindRetryFrames = 120;

        /// <summary>
        /// Long-animation threshold (seconds) for Default.json end path,
        /// deferred Spankings merge, and clothing touch-fall gating (formerly
        /// JSON-tunable).
        /// </summary>
        internal const float MinNonLoopAnimationSecondsForDefaultSceneLoad = 45f;

        /// <summary>
        /// Fluid cum mesh hide delay after load (realtime seconds).
        /// </summary>
        private const float FluidCumRevealDelayRealtimeSeconds = 10f;

        private static bool logMessages;

        private GabrielHud _gabrielHud;

        private bool isLoading = true;

        private bool sceneChanged = true;

        private float loadingTimeCounter;

        private string lastLoadDir = "";

        private Coroutine _applyEmotionAfterSceneCo;

        private Coroutine _pathRuleEmotionMergeCo;

        private Coroutine _mergeSpankingsAfterGripCo;

        private Coroutine _animationNoLoopDetectionDeferredDefaultCo;

        private JSONStorableString explanationString;

        private JSONStorableAction forceReleaseSceneSettleHoldAction;

        private bool prevSuperControllerIsLoading;

        // Same-folder pulses come from loads like atom/toy changes and should not
        // re-run the full settle workflow.
        private bool skipSceneSettleWorkflowForPendingLoad;

        private SameFolderLoadCheck sameFolderLoadCheck = new SameFolderLoadCheck();

        private SceneSettleRuntime sceneSettle = new SceneSettleRuntime();

        /// <summary>
        /// After a grip-triggered ClothingTouchFallOff merge for the current VaM
        /// load-folder chain, suppress repeating on same-folder scene loads until
        /// the user navigates to a different folder.
        /// </summary>
        private bool clothingTouchFallOffGripMergeCommittedForFolderBatch;

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
        /// Merges <c>ClothingTouchFallOff</c> onto each female Person (from
        /// <see cref="PersonAtomCache.FemalePersonsByUid"/>)
        /// that has active clothing geometry.
        /// </summary>
        public void MergeClothingTouchFallOffOnAllPersonsOnly()
        {
            try
            {
                foreach (Atom at in PersonAtomCache.FemalePersonsByUid())
                {
                    if (!PersonAtomCache.PersonHasAnyActiveClothingOnGeometry(at))
                        continue;

                    PluginManager.TryMergePluginOntoPerson(
                        at,
                        ClothingTouchFallOffPersonPluginPath);
                }
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "Gabriel clothing touch fall-off merge on Persons: " + e);
            }
        }

        internal void BindGabrielHud(GabrielHud hud)
        {
            _gabrielHud = hud;
        }

        /// <summary>
        /// Registers static <see cref="GripHandVisibility"/> delegates on this
        /// orchestrator; cleared in <see cref="OnDestroy"/>.
        /// </summary>
        private void WireGripHandVisibilityMergeCallbacks()
        {
            GripHandVisibility.SetMergeSpankingsOnFirstGrip(
                QueueMergeSpankingsAfterGripDeferred);
            GripHandVisibility.SetMergeClothingTouchFallOffOnFirstMale2Grip(
                TryMergeClothingTouchFallOffOnFirstMale2Grip);
        }

        /// <summary>
        /// Drops <see cref="GripHandVisibility"/> delegates so reload does not
        /// call a destroyed orchestrator.
        /// </summary>
        private void UnwireGripHandVisibilityMergeCallbacks()
        {
            GripHandVisibility.SetMergeSpankingsOnFirstGrip(null);
            GripHandVisibility.SetMergeClothingTouchFallOffOnFirstMale2Grip(null);
        }

        public override void Init()
        {
            explanationString = new JSONStorableString(
                "",
                "Gabriel session orchestrator handles scene-settle playback " +
                "hold, same-folder load suppression, late runtime ticks, " +
                "and built-in session default behaviors.");
            UIDynamicTextField dtext = CreateTextField(explanationString);
            dtext.height = 520;

            forceReleaseSceneSettleHoldAction = new JSONStorableAction(
                ForceReleaseSceneSettleHoldActionName,
                ForceReleaseSceneSettleHoldFromAction);
            RegisterAction(forceReleaseSceneSettleHoldAction);

            SameFolderCameraRetain.SetRetainEnabled(true);

            SuperController sc = SuperController.singleton;
            if (sc != null)
            {
                sc.onAtomUIDsChangedHandlers -= OnAtomUIDsChangedHandlers;
                sc.onAtomUIDsChangedHandlers += OnAtomUIDsChangedHandlers;
            }

            PassengerRuntime.SetSessionPluginHost(this);
            WireGripHandVisibilityMergeCallbacks();
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

        private bool TryMergeClothingTouchFallOffOnFirstMale2Grip()
        {
            if (clothingTouchFallOffGripMergeCommittedForFolderBatch)
            {
                return true;
            }

            if (AnimationNoLoopDetection.CurrentSceneUsesLongNonLoopAnimation(
                    MinNonLoopAnimationSecondsForDefaultSceneLoad))
            {
                return false;
            }

            if (!SceneHasFemaleWithActiveClothing())
            {
                clothingTouchFallOffGripMergeCommittedForFolderBatch = true;
                return true;
            }

            try
            {
                MergeClothingTouchFallOffOnAllPersonsOnly();
                RefreshHudPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "Gabriel clothing touch fall-off first Male2 grip: " + e);
                throw;
            }

            clothingTouchFallOffGripMergeCommittedForFolderBatch = true;
            return true;
        }

        private static bool SceneHasFemaleWithActiveClothing()
        {
            int i;
            List<Atom> females;

            females = PersonAtomCache.FemalePersonsByUid();
            for (i = 0; i < females.Count; i++)
            {
                if (PersonAtomCache.PersonHasAnyActiveClothingOnGeometry(females[i]))
                    return true;
            }

            return false;
        }

        private void ApplyRemoteHoldGrabPreference()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;
            if (!sc.isOVR && !sc.isOpenVR)
                return;
            // Same default as former JSON: disable remote laser aim hand-link.
            sc.DisableRemoteHoldGrab();
        }

        void Start()
        {
            Log("GabrielSessionOrchestrator Start");
            ResolveGabrielHud();

            ApplyRemoteHoldGrabPreference();
            HeadProximityHide.SetHeadProximityHideEnabled(true, this);

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

            HeadProximityHide.SetHeadProximityHideEnabled(true, this);
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
                TriggerClothingRemover.NotifyClothingStripEligibilityDirty();
                PassengerRuntime.NotifyAtomUidsChanged(atomUids, this);

                if (atomUids == null || atomUids.Count == 0)
                    return;

                SuperController sc = SuperController.singleton;
                if (sc == null || sc.isLoading)
                    return;

                int j;
                for (j = 0; j < atomUids.Count; j++)
                {
                    Atom a = sc.GetAtomByUid(atomUids[j]);
                    if (a != null && a.type == "Person")
                    {
                        PersonTongueCollisionDisable.TryDisableForPerson(a);
                    }
                }

                if (!EmotionPathKeywords.MatchesCurrentScenePath())
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
                    sameFolderLoadCheck.IsSameFolderLoad(scFsm);
                PassengerRuntime.NotifySceneChanged(this);
            }
            else if (!loadingNow)
            {
                sameFolderLoadCheck.CaptureIdleLoadDir(scFsm);
            }

            FluidCumHideDuringSceneLoad.Tick(
                true,
                loadingNow,
                FluidCumRevealDelayRealtimeSeconds);

            if (prevSuperControllerIsLoading && !loadingNow)
                SameFolderCameraRetain.QueueRestoreCoroutineIfNeeded(this);
            if (!prevSuperControllerIsLoading && loadingNow)
                SameFolderCameraRetain.NotifyLoadBeginning(scFsm);

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
                TriggerClothingRemover.NotifyClothingStripEligibilityDirty();
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

                currentLoadDirNorm = SameFolderLoadCheck.Normalize(
                    scFsm.currentLoadDir);
                sameFolderLoad = SameFolderLoadCheck.SameFolderLoads(
                    lastLoadDir,
                    currentLoadDirNorm);
                hud = ResolveGabrielHud();

                PersonAtomCache.InvalidatePersonGenderCaches();

                PersonTongueCollisionDisable.ResetForNewScene();
                PersonTongueCollisionDisable.ApplyToAllPersonAtoms(scFsm);

                MirrorReflectionHighResOnSceneLoad.ApplyIfSceneHasMirrorHosts(
                    scFsm);

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
                    clothingTouchFallOffGripMergeCommittedForFolderBatch = false;
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
                    sameFolderLoad,
                    sameFolderLoad &&
                        clothingTouchFallOffGripMergeCommittedForFolderBatch);

                HeadProximityHide.SetHeadProximityHideEnabled(true, this);
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

            OverlapFullGrabRelease.LateTick(true);

            GripHandVisibility.LateTick();

            TriggerClothingRemover.LateTickStrip(sc);

            SameFolderCameraRetain.LateTickIdleCapture(sc);

            AnimationNoLoopDetection.LateTick(
                true,
                MinNonLoopAnimationSecondsForDefaultSceneLoad,
                this);

            MonitorModeLaserRestore.Tick(true);
            VrEulerPossessHandHud.Tick();
            PassengerRuntime.Tick(this);
            PassengerRuntime.LateTick(this);
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
                _animationNoLoopDetectionDeferredDefaultCo = null;

                UnwireGripHandVisibilityMergeCallbacks();
                _gabrielHud = null;
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
