using System;
using UnityEngine;

namespace geesp0t
{
    public partial class SceneSettleRuntime
    {
        private bool sceneSettleSimulationPauseAppliedToSuperController;

        private static AsyncFlag sharedSceneSettlePauseAsyncFlag;

        private bool audioPauseSnapshotCapturedForSceneSettleHold;

        private bool savedAudioListenerPauseBeforeSceneSettleHold;

        private bool sceneSettlePauseHoldDeadlineActive;

        private float sceneSettlePauseHoldDeadlineUnscaledTime;

        private const string sceneSettlePauseFlagDisplayName =
            "SceneControlSuite scene settle";

        private const float sceneSettlePauseHoldTimeoutSeconds = 30f;

        static AsyncFlag GetSharedSceneSettlePauseAsyncFlag()
        {
            if (sharedSceneSettlePauseAsyncFlag == null)
            {
                sharedSceneSettlePauseAsyncFlag =
                    new AsyncFlag(sceneSettlePauseFlagDisplayName);
            }

            return sharedSceneSettlePauseAsyncFlag;
        }

        public void OnPluginDestroy()
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
                SuperController.LogError(
                    "[SceneSettle] Restore camExposure in OnDestroy " +
                    "failed: " + destroyRestoreException);
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

            AsyncFlag sceneSettlePauseAsyncFlag =
                GetSharedSceneSettlePauseAsyncFlag();
            sceneSettlePauseAsyncFlag.Raise();
            sceneSettlePauseAsyncFlag.Lower();
            superController.PauseSimulation(sceneSettlePauseAsyncFlag, true);
            sceneSettleSimulationPauseAppliedToSuperController = true;

            if (!superController.isLoading)
            {
                sceneSettlePauseHoldDeadlineUnscaledTime =
                    Time.unscaledTime + sceneSettlePauseHoldTimeoutSeconds;
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
                savedAudioListenerPauseBeforeSceneSettleHold =
                    AudioListener.pause;
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
                    SuperController.LogError(
                        "[SceneSettle] Restore camExposure after settle " +
                        "phase failed: " + restoreException);
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
                AudioListener.pause =
                    savedAudioListenerPauseBeforeSceneSettleHold;
                audioPauseSnapshotCapturedForSceneSettleHold = false;
            }
        }
    }
}
