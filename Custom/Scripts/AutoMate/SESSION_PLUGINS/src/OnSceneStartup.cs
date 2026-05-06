using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// While the scene is still settling (same signals as VaM&apos;s load UI / icon / isLoading):
    /// drives CoreControl GlobalLighting camExposure to 0, freezes simulation via SuperController.PauseSimulation,
    /// and forces AudioListener.pause so motion/sound do not run ahead of loaded assets.
    /// After the first full settle for a load, ignores later loading UI / icon-only activity (so streaming assets do not force exposure to 0 again).
    /// Re-arms when SuperController.isLoading becomes true (new VaM scene load).
    /// Worst case: 30s (unscaled) after SuperController.isLoading becomes false — not from scene load start —
    /// forces the same finish path as a normal settle if the pause flag is still held.
    /// Tick runs from LateUpdate so CoreControl JSON usually reflects the scene before we read exposure backup.
    /// </summary>
    public class OnSceneStartup
    {
        private bool wasSceneStillSettling = false;

        private bool camExposureBackupCaptured = false;
        private float savedCamExposure = 0f;

        private bool sceneSettleSimulationPauseAppliedToSuperController;

        private static AsyncFlag sharedSceneSettlePauseAsyncFlag;

        private bool audioPauseSnapshotCapturedForSceneSettleHold;
        private bool savedAudioListenerPauseBeforeSceneSettleHold;

        private bool lastSuperControllerIsLoading;

        private bool initialSceneLoadSettleWorkflowFinished;

        private bool sceneSettlePauseHoldDeadlineActive;

        private float sceneSettlePauseHoldDeadlineUnscaledTime;

        private Atom cachedCoreControlAtom;
        private JSONStorable cachedGlobalLightingStorable;

        private const string coreControlAtomUid = "CoreControl";
        private const string globalLightingStorableId = "GlobalLighting";
        private const string camExposureParamName = "camExposure";

        private const float forcedExposureEpsilon = 0.001f;

        private const float nearDefaultFullExposure = 0.99f;

        private const string sceneSettlePauseFlagDisplayName = "AutoMate OnSceneStartup scene settle";

        private const float sceneSettlePauseHoldTimeoutSeconds = 30f;

        private const bool exposureDebugLog = true;

        private int dbgLoadSerial = 0;

        private int dbgSkipLogForLoadSerial = -1;

        private int dbgApplyNullGlForLoadSerial = -1;

        private int dbgForceZeroLogForLoadSerial = -1;

        private int dbgStuckZeroLogForLoadSerial = -1;

        static AsyncFlag GetSharedSceneSettlePauseAsyncFlag()
        {
            if (sharedSceneSettlePauseAsyncFlag == null)
            {
                sharedSceneSettlePauseAsyncFlag = new AsyncFlag(sceneSettlePauseFlagDisplayName);
            }

            return sharedSceneSettlePauseAsyncFlag;
        }

        /// <summary>Returns true the first tick after VaM&apos;s loading/settle UI has cleared — playback hold was released.</summary>
        public bool TickDuringSuperControllerLoad(bool skipExposureWorkflowForCurrentLoad)
        {
            SuperController superController = SuperController.singleton;
            bool superControllerIsLoadingNow = superController != null && superController.isLoading;
            bool superControllerIsLoadingFellThisTick = lastSuperControllerIsLoading && !superControllerIsLoadingNow;

            if (superControllerIsLoadingNow && !lastSuperControllerIsLoading)
            {
                dbgLoadSerial++;
                if (exposureDebugLog)
                {
                    bool rawForLog = ShouldTreatSceneAsStillSettling();
                    LogExposureDbg(
                        "isLoading rose serial=" + dbgLoadSerial +
                        " skipWorkflowFlag=" + skipExposureWorkflowForCurrentLoad +
                        " " + DiagFormatExposureState() +
                        " " + DiagSettleBreakdown(
                            superController,
                            rawForLog,
                            superControllerIsLoadingNow));
                }
                ClearGlobalLightingCache();
                GetSharedSceneSettlePauseAsyncFlag().Raise();
                FinishSceneSettleExposureThenReleasePlaybackHold("loadStart");
                initialSceneLoadSettleWorkflowFinished = false;
                wasSceneStillSettling = false;
            }

            if (superControllerIsLoadingFellThisTick && sceneSettleSimulationPauseAppliedToSuperController)
            {
                sceneSettlePauseHoldDeadlineUnscaledTime = Time.unscaledTime + sceneSettlePauseHoldTimeoutSeconds;
                sceneSettlePauseHoldDeadlineActive = true;
            }

            lastSuperControllerIsLoading = superControllerIsLoadingNow;

            bool rawSceneSettlingIndicatorsActive = ShouldTreatSceneAsStillSettling();

            if (skipExposureWorkflowForCurrentLoad)
            {
                if (exposureDebugLog && dbgSkipLogForLoadSerial != dbgLoadSerial)
                {
                    dbgSkipLogForLoadSerial = dbgLoadSerial;
                    LogExposureDbg(
                        "skipWorkflow branch serial=" + dbgLoadSerial +
                        " rawSettle=" + rawSceneSettlingIndicatorsActive +
                        " isLoading=" + superControllerIsLoadingNow +
                        " oneShotDone=" + initialSceneLoadSettleWorkflowFinished +
                        " pauseOn=" + sceneSettleSimulationPauseAppliedToSuperController +
                        " " + DiagFormatExposureState());
                }
                if (sceneSettleSimulationPauseAppliedToSuperController ||
                    camExposureBackupCaptured)
                {
                    FinishSceneSettleExposureThenReleasePlaybackHold("skipWorkflow");
                }

                wasSceneStillSettling = false;
                return false;
            }

            bool settleEndedThisTick = false;

            if (!rawSceneSettlingIndicatorsActive && !superControllerIsLoadingNow && sceneSettleSimulationPauseAppliedToSuperController)
            {
                FinishSceneSettleExposureThenReleasePlaybackHold("idleSafety");
                wasSceneStillSettling = false;
                initialSceneLoadSettleWorkflowFinished = true;
                settleEndedThisTick = true;
            }

            if (sceneSettleSimulationPauseAppliedToSuperController && sceneSettlePauseHoldDeadlineActive)
            {
                if (Time.unscaledTime >= sceneSettlePauseHoldDeadlineUnscaledTime)
                {
                    SuperController.LogMessage("[OnSceneStartup] Scene settle pause exceeded " + sceneSettlePauseHoldTimeoutSeconds + "s after isLoading cleared; forcing finish.");
                    FinishSceneSettleExposureThenReleasePlaybackHold("timeout");
                    settleEndedThisTick = true;
                    initialSceneLoadSettleWorkflowFinished = true;
                    wasSceneStillSettling = false;
                }
            }

            bool sceneSettlingForExposureWorkflow = rawSceneSettlingIndicatorsActive;

            if (initialSceneLoadSettleWorkflowFinished && !superControllerIsLoadingNow)
            {
                sceneSettlingForExposureWorkflow = false;
            }

            if (sceneSettlingForExposureWorkflow)
            {
                if (!wasSceneStillSettling)
                {
                    camExposureBackupCaptured = false;
                    BeginSceneSettleSimulationPauseHold();
                }

                MaintainSceneSettleAudioPauseDuringTick();

                ApplyCamExposureWhileSceneSettling();
            }
            else
            {
                if (wasSceneStillSettling)
                {
                    FinishSceneSettleExposureThenReleasePlaybackHold("workflowExit");
                    settleEndedThisTick = true;
                    initialSceneLoadSettleWorkflowFinished = true;
                }
            }

            wasSceneStillSettling = sceneSettlingForExposureWorkflow;

            if (exposureDebugLog && settleEndedThisTick)
            {
                LogExposureDbg(
                    "settleEndedThisTick serial=" + dbgLoadSerial + " " +
                    DiagFormatExposureState() + " " +
                    DiagSettleBreakdown(
                        superController,
                        rawSceneSettlingIndicatorsActive,
                        superControllerIsLoadingNow));
            }

            return settleEndedThisTick;
        }

        public void OnOwningPluginDestroy()
        {
            try
            {
                if (camExposureBackupCaptured)
                {
                    RestoreCamExposure();
                }
            }
            catch (Exception destroyRestoreException)
            {
                SuperController.LogError("[OnSceneStartup] Restore camExposure in OnDestroy failed: " + destroyRestoreException);
            }

            GetSharedSceneSettlePauseAsyncFlag().Raise();
            ReleaseSceneSettlePlaybackHold();
        }

        void BeginSceneSettleSimulationPauseHold()
        {
            SuperController superController = SuperController.singleton;
            if (superController == null)
            {
                return;
            }

            AsyncFlag sceneSettlePauseAsyncFlag = GetSharedSceneSettlePauseAsyncFlag();
            sceneSettlePauseAsyncFlag.Raise();
            sceneSettlePauseAsyncFlag.Lower();
            superController.PauseSimulation(sceneSettlePauseAsyncFlag, true);
            sceneSettleSimulationPauseAppliedToSuperController = true;

            if (exposureDebugLog)
            {
                LogExposureDbg(
                    "BeginPauseHold serial=" + dbgLoadSerial + " " +
                    DiagFormatExposureState() +
                    " scIsLoading=" + superController.isLoading);
            }

            if (!superController.isLoading)
            {
                sceneSettlePauseHoldDeadlineUnscaledTime = Time.unscaledTime + sceneSettlePauseHoldTimeoutSeconds;
                sceneSettlePauseHoldDeadlineActive = true;
            }
            else
            {
                sceneSettlePauseHoldDeadlineActive = false;
            }
        }

        void MaintainSceneSettleAudioPauseDuringTick()
        {
            if (!audioPauseSnapshotCapturedForSceneSettleHold)
            {
                savedAudioListenerPauseBeforeSceneSettleHold = AudioListener.pause;
                audioPauseSnapshotCapturedForSceneSettleHold = true;
            }

            AudioListener.pause = true;
        }

        void FinishSceneSettleExposureThenReleasePlaybackHold(string dbgReason)
        {
            if (exposureDebugLog)
            {
                LogExposureDbg(
                    "Finish(" + dbgReason + ") enter " +
                    DiagFormatExposureState() + " pauseOn=" +
                    sceneSettleSimulationPauseAppliedToSuperController);
            }

            if (camExposureBackupCaptured)
            {
                try
                {
                    RestoreCamExposure();
                }
                catch (Exception restoreException)
                {
                    SuperController.LogError("[OnSceneStartup] Restore camExposure after settle phase failed: " + restoreException);
                }
            }
            else
            {
                RestoreCamExposureUsingGlobalLightingDefaultBecauseBackupWasNeverCaptured();
            }

            if (exposureDebugLog)
            {
                LogExposureDbg(
                    "Finish(" + dbgReason + ") afterRestore " +
                    DiagFormatExposureState());
            }

            ReleaseSceneSettlePlaybackHold();
        }

        void ReleaseSceneSettlePlaybackHold()
        {
            if (sceneSettleSimulationPauseAppliedToSuperController)
            {
                GetSharedSceneSettlePauseAsyncFlag().Raise();
                sceneSettleSimulationPauseAppliedToSuperController = false;
            }

            sceneSettlePauseHoldDeadlineActive = false;

            if (audioPauseSnapshotCapturedForSceneSettleHold)
            {
                AudioListener.pause = savedAudioListenerPauseBeforeSceneSettleHold;
                audioPauseSnapshotCapturedForSceneSettleHold = false;
            }
        }

        bool ShouldTreatSceneAsStillSettling()
        {
            SuperController superController = SuperController.singleton;

            if (superController.isLoading)
            {
                return true;
            }

            if (IsTransformActive(superController.loadingUI))
            {
                return true;
            }

            if (IsTransformActive(superController.loadingUIAlt))
            {
                return true;
            }

            if (IsTransformActive(superController.loadingGeometry))
            {
                return true;
            }

            if (IsTransformActive(superController.loadingIcon))
            {
                return true;
            }

            return false;
        }

        static bool IsTransformActive(Transform sceneTransform)
        {
            if (sceneTransform == null)
            {
                return false;
            }

            return sceneTransform.gameObject.activeSelf;
        }

        void ApplyCamExposureWhileSceneSettling()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                if (exposureDebugLog && dbgApplyNullGlForLoadSerial != dbgLoadSerial)
                {
                    dbgApplyNullGlForLoadSerial = dbgLoadSerial;
                    LogExposureDbg(
                        "Apply: GlobalLighting missing serial=" + dbgLoadSerial);
                }

                return;
            }

            float rawCamExposure = globalLightingStorable.GetFloatParamValue(camExposureParamName);

            MaybeUpdateCamExposureBackupFromScene(rawCamExposure);

            if (rawCamExposure > forcedExposureEpsilon)
            {
                if (exposureDebugLog && dbgForceZeroLogForLoadSerial != dbgLoadSerial)
                {
                    dbgForceZeroLogForLoadSerial = dbgLoadSerial;
                    LogExposureDbg(
                        "Apply: force camExposure 0 rawWas=" +
                        rawCamExposure.ToString("F4") + " serial=" + dbgLoadSerial);
                }

                globalLightingStorable.SetFloatParamValue(camExposureParamName, 0f);
            }
            else if (exposureDebugLog &&
                dbgStuckZeroLogForLoadSerial != dbgLoadSerial)
            {
                dbgStuckZeroLogForLoadSerial = dbgLoadSerial;
                LogExposureDbg(
                    "Apply: skip force (raw ~0) raw=" +
                    rawCamExposure.ToString("F4") + " backupCap=" +
                    camExposureBackupCaptured + " serial=" + dbgLoadSerial);
            }
        }

        void MaybeUpdateCamExposureBackupFromScene(float rawCamExposure)
        {
            if (rawCamExposure <= forcedExposureEpsilon)
            {
                return;
            }

            if (rawCamExposure >= nearDefaultFullExposure)
            {
                savedCamExposure = rawCamExposure;
                camExposureBackupCaptured = true;

                return;
            }

            if (!camExposureBackupCaptured)
            {
                savedCamExposure = rawCamExposure;
                camExposureBackupCaptured = true;
            }
            else
            {
                savedCamExposure = Mathf.Min(savedCamExposure, rawCamExposure);
            }
        }

        void RestoreCamExposureUsingGlobalLightingDefaultBecauseBackupWasNeverCaptured()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                SuperController.LogError("[OnSceneStartup] Settle ended without camExposure backup and CoreControl GlobalLighting was missing; exposure may stay at 0.");
                return;
            }

            JSONStorableFloat camExposureJsonFloat = globalLightingStorable.GetFloatJSONParam(camExposureParamName);
            float fallbackCamExposure = 1f;

            if (camExposureJsonFloat != null)
            {
                fallbackCamExposure = camExposureJsonFloat.defaultVal;
            }

            try
            {
                savedCamExposure = fallbackCamExposure;
                camExposureBackupCaptured = true;
                RestoreCamExposure();
            }
            catch (Exception fallbackRestoreException)
            {
                SuperController.LogError("[OnSceneStartup] Fallback restore camExposure failed: " + fallbackRestoreException);
            }
        }

        void RestoreCamExposure()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                SuperController.LogError("[OnSceneStartup] Restore: CoreControl GlobalLighting not available.");
                camExposureBackupCaptured = false;
                return;
            }

            globalLightingStorable.SetFloatParamValue(camExposureParamName, savedCamExposure);
            camExposureBackupCaptured = false;
        }

        JSONStorable TryGetGlobalLightingStorable()
        {
            if (cachedCoreControlAtom != null &&
                !cachedCoreControlAtom.destroyed &&
                cachedGlobalLightingStorable != null)
            {
                return cachedGlobalLightingStorable;
            }

            SuperController superController = SuperController.singleton;
            if (superController == null)
            {
                ClearGlobalLightingCache();
                return null;
            }

            Atom coreAtom = superController.GetAtomByUid(coreControlAtomUid);
            if (coreAtom == null || coreAtom.destroyed)
            {
                ClearGlobalLightingCache();
                return null;
            }

            JSONStorable globalLightingStorable = coreAtom.GetStorableByID(globalLightingStorableId);
            if (globalLightingStorable == null)
            {
                ClearGlobalLightingCache();
                return null;
            }

            cachedCoreControlAtom = coreAtom;
            cachedGlobalLightingStorable = globalLightingStorable;

            return globalLightingStorable;
        }

        void ClearGlobalLightingCache()
        {
            cachedCoreControlAtom = null;
            cachedGlobalLightingStorable = null;
        }

        void LogExposureDbg(string message)
        {
            if (!exposureDebugLog)
            {
                return;
            }

            SuperController.LogMessage(
                "[OnSceneStartupDbg] " +
                DateTime.Now.ToString("HH:mm:ss.fff") +
                " " +
                message);
        }

        string DiagFormatExposureState()
        {
            JSONStorable gl = TryGetGlobalLightingStorable();
            if (gl == null)
            {
                return "gl=null backup=" + camExposureBackupCaptured +
                    " savedTarget=" + savedCamExposure.ToString("F4");
            }

            float ev = gl.GetFloatParamValue(camExposureParamName);
            return "camExp=" + ev.ToString("F4") + " backup=" +
                camExposureBackupCaptured + " savedTarget=" +
                savedCamExposure.ToString("F4");
        }

        static string DiagSettleBreakdown(
            SuperController sc,
            bool rawCombined,
            bool isLoadingNow)
        {
            if (sc == null)
            {
                return "sc=null";
            }

            return string.Format(
                "raw={0} load={1} ui={2} alt={3} geo={4} icon={5}",
                rawCombined,
                isLoadingNow,
                IsTransformActive(sc.loadingUI),
                IsTransformActive(sc.loadingUIAlt),
                IsTransformActive(sc.loadingGeometry),
                IsTransformActive(sc.loadingIcon));
        }
    }
}
