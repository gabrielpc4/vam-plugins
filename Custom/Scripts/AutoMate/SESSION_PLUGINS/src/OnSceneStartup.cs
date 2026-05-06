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

        private const string coreControlAtomUid = "CoreControl";
        private const string globalLightingStorableId = "GlobalLighting";
        private const string camExposureParamName = "camExposure";

        private const float forcedExposureEpsilon = 0.001f;

        private const float nearDefaultFullExposure = 0.99f;

        private const string sceneSettlePauseFlagDisplayName = "AutoMate OnSceneStartup scene settle";

        private const float sceneSettlePauseHoldTimeoutSeconds = 30f;

        static AsyncFlag GetSharedSceneSettlePauseAsyncFlag()
        {
            if (sharedSceneSettlePauseAsyncFlag == null)
            {
                sharedSceneSettlePauseAsyncFlag = new AsyncFlag(sceneSettlePauseFlagDisplayName);
            }

            return sharedSceneSettlePauseAsyncFlag;
        }

        /// <summary>Returns true the first tick after VaM&apos;s loading/settle UI has cleared — playback hold was released.</summary>
        public bool TickDuringSuperControllerLoad()
        {
            SuperController superController = SuperController.singleton;
            bool superControllerIsLoadingNow = superController != null && superController.isLoading;
            bool superControllerIsLoadingFellThisTick = lastSuperControllerIsLoading && !superControllerIsLoadingNow;

            if (superControllerIsLoadingNow && !lastSuperControllerIsLoading)
            {
                GetSharedSceneSettlePauseAsyncFlag().Raise();
                ReleaseSceneSettlePlaybackHold();
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

            bool settleEndedThisTick = false;

            if (!rawSceneSettlingIndicatorsActive && !superControllerIsLoadingNow && sceneSettleSimulationPauseAppliedToSuperController)
            {
                FinishSceneSettleExposureThenReleasePlaybackHold();
                wasSceneStillSettling = false;
                initialSceneLoadSettleWorkflowFinished = true;
                settleEndedThisTick = true;
            }

            if (sceneSettleSimulationPauseAppliedToSuperController && sceneSettlePauseHoldDeadlineActive)
            {
                if (Time.unscaledTime >= sceneSettlePauseHoldDeadlineUnscaledTime)
                {
                    SuperController.LogMessage("[OnSceneStartup] Scene settle pause exceeded " + sceneSettlePauseHoldTimeoutSeconds + "s after isLoading cleared; forcing finish.");
                    FinishSceneSettleExposureThenReleasePlaybackHold();
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
                    FinishSceneSettleExposureThenReleasePlaybackHold();
                    settleEndedThisTick = true;
                    initialSceneLoadSettleWorkflowFinished = true;
                }
            }

            wasSceneStillSettling = sceneSettlingForExposureWorkflow;

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

        void FinishSceneSettleExposureThenReleasePlaybackHold()
        {
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
                return;
            }

            float rawCamExposure = globalLightingStorable.GetFloatParamValue(camExposureParamName);

            MaybeUpdateCamExposureBackupFromScene(rawCamExposure);

            globalLightingStorable.SetFloatParamValue(camExposureParamName, 0f);
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
            Atom coreAtom = SuperController.singleton.GetAtomByUid(coreControlAtomUid);
            if (coreAtom == null || coreAtom.destroyed)
            {
                return null;
            }

            return coreAtom.GetStorableByID(globalLightingStorableId);
        }
    }
}
