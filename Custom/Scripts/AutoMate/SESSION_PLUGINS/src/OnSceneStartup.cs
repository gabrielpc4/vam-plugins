using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// While the scene is still settling (same signals as VaM&apos;s load UI / icon / isLoading):
    /// drives CoreControl GlobalLighting camExposure to 0, freezes simulation via SuperController.PauseSimulation,
    /// and forces AudioListener.pause so motion/sound do not run ahead of loaded assets.
    /// After settle ends, starts a linear camExposure ramp from the current value to the backed-up target over 2 seconds, then raises the pause flag and restores the prior audio pause state.
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

        private bool camExposureGradientRestoreActive;
        private float camExposureGradientRestoreStartTime;
        private float camExposureGradientRestoreFrom;
        private float camExposureGradientRestoreTo;

        private const string coreControlAtomUid = "CoreControl";
        private const string globalLightingStorableId = "GlobalLighting";
        private const string camExposureParamName = "camExposure";

        private const float forcedExposureEpsilon = 0.001f;

        private const float nearDefaultFullExposure = 0.99f;

        private const string sceneSettlePauseFlagDisplayName = "AutoMate OnSceneStartup scene settle";

        private const float camExposureRestoreRampDurationSeconds = 2f;

        /// <summary>Returns true the first tick after VaM&apos;s loading/settle UI has cleared — playback hold was released.</summary>
        public bool TickDuringSuperControllerLoad()
        {
            bool settlingNow = ShouldTreatSceneAsStillSettling();
            bool settleEndedThisTick = false;

            if (!settlingNow && camExposureGradientRestoreActive)
            {
                TickCamExposureGradientRestore();
            }

            if (settlingNow)
            {
                if (!wasSceneStillSettling)
                {
                    camExposureBackupCaptured = false;
                    camExposureGradientRestoreActive = false;
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
                }
            }

            wasSceneStillSettling = settlingNow;
            return settleEndedThisTick;
        }

        public void OnOwningPluginDestroy()
        {
            try
            {
                if (camExposureGradientRestoreActive)
                {
                    ApplyCamExposureImmediate(camExposureGradientRestoreTo);
                    camExposureGradientRestoreActive = false;
                }
                else if (camExposureBackupCaptured)
                {
                    ApplyCamExposureImmediate(savedCamExposure);
                    camExposureBackupCaptured = false;
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
                    StartCamExposureGradientRestoreFromCurrentToSavedTarget();
                }
                catch (Exception restoreException)
                {
                    SuperController.LogError("[OnSceneStartup] Start camExposure ramp after settle phase failed: " + restoreException);
                }
            }
            else
            {
                RestoreCamExposureUsingGlobalLightingDefaultBecauseBackupWasNeverCaptured();
            }

            ReleaseSceneSettlePlaybackHold();
        }

        void StartCamExposureGradientRestoreFromCurrentToSavedTarget()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                SuperController.LogError("[OnSceneStartup] CamExposure ramp: CoreControl GlobalLighting not available.");
                camExposureBackupCaptured = false;
                return;
            }

            camExposureGradientRestoreFrom = globalLightingStorable.GetFloatParamValue(camExposureParamName);
            camExposureGradientRestoreTo = savedCamExposure;
            camExposureGradientRestoreStartTime = Time.time;
            camExposureGradientRestoreActive = true;
            camExposureBackupCaptured = false;

            TickCamExposureGradientRestore();
        }

        void TickCamExposureGradientRestore()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                SuperController.LogError("[OnSceneStartup] CamExposure ramp tick: GlobalLighting missing; ramp aborted.");
                camExposureGradientRestoreActive = false;
                return;
            }

            float elapsedSeconds = Time.time - camExposureGradientRestoreStartTime;
            float rampBlend = Mathf.Clamp01(elapsedSeconds / camExposureRestoreRampDurationSeconds);
            float blendedCamExposure = Mathf.Lerp(camExposureGradientRestoreFrom, camExposureGradientRestoreTo, rampBlend);

            globalLightingStorable.SetFloatParamValue(camExposureParamName, blendedCamExposure);

            if (rampBlend >= 1f)
            {
                camExposureGradientRestoreActive = false;
            }
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
                StartCamExposureGradientRestoreFromCurrentToSavedTarget();
            }
            catch (Exception fallbackRestoreException)
            {
                SuperController.LogError("[OnSceneStartup] Fallback restore camExposure failed: " + fallbackRestoreException);
            }
        }

        void ApplyCamExposureImmediate(float targetCamExposure)
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                SuperController.LogError("[OnSceneStartup] ApplyCamExposureImmediate: CoreControl GlobalLighting not available.");
                return;
            }

            globalLightingStorable.SetFloatParamValue(camExposureParamName, targetCamExposure);
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
