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
    /// Tick runs from LateUpdate so CoreControl JSON usually reflects the scene before we read exposure backup.
    /// </summary>
    public class OnSceneStartup
    {
        private bool wasSceneStillSettling = false;

        private bool camExposureBackupCaptured = false;
        private float savedCamExposure = 0f;

        private AsyncFlag sceneSettleSimulationPauseFlag;
        private bool sceneSettleSimulationPauseAppliedToSuperController;

        private bool audioPauseSnapshotCapturedForSceneSettleHold;
        private bool savedAudioListenerPauseBeforeSceneSettleHold;

        private bool lastSuperControllerIsLoading;

        private bool initialSceneLoadSettleWorkflowFinished;

        private const string coreControlAtomUid = "CoreControl";
        private const string globalLightingStorableId = "GlobalLighting";
        private const string camExposureParamName = "camExposure";

        private const float forcedExposureEpsilon = 0.001f;

        private const float nearDefaultFullExposure = 0.99f;

        private const string sceneSettlePauseFlagDisplayName = "AutoMate OnSceneStartup scene settle";

        /// <summary>Returns true the first tick after VaM&apos;s loading/settle UI has cleared — playback hold was released.</summary>
        public bool TickDuringSuperControllerLoad()
        {
            SuperController superController = SuperController.singleton;
            bool superControllerIsLoadingNow = superController != null && superController.isLoading;

            if (superControllerIsLoadingNow && !lastSuperControllerIsLoading)
            {
                initialSceneLoadSettleWorkflowFinished = false;
            }

            lastSuperControllerIsLoading = superControllerIsLoadingNow;

            bool rawSceneSettlingIndicatorsActive = ShouldTreatSceneAsStillSettling();
            bool sceneSettlingForExposureWorkflow = rawSceneSettlingIndicatorsActive;

            if (initialSceneLoadSettleWorkflowFinished && !superControllerIsLoadingNow)
            {
                sceneSettlingForExposureWorkflow = false;
            }

            bool settleEndedThisTick = false;

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

            ReleaseSceneSettlePlaybackHold();
        }

        void BeginSceneSettleSimulationPauseHold()
        {
            SuperController superController = SuperController.singleton;
            if (superController == null)
            {
                return;
            }

            if (sceneSettleSimulationPauseFlag == null)
            {
                sceneSettleSimulationPauseFlag = new AsyncFlag(sceneSettlePauseFlagDisplayName);
            }

            sceneSettleSimulationPauseFlag.Lower();
            superController.PauseSimulation(sceneSettleSimulationPauseFlag, true);
            sceneSettleSimulationPauseAppliedToSuperController = true;
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
            if (sceneSettleSimulationPauseAppliedToSuperController && sceneSettleSimulationPauseFlag != null)
            {
                sceneSettleSimulationPauseFlag.Raise();
                sceneSettleSimulationPauseAppliedToSuperController = false;
            }

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
