using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// While the scene is still settling (same signals as VaM's load UI, icon,
    /// or `isLoading`), drive `camExposure` to 0, pause simulation, and mute
    /// audio so motion and sound do not run ahead of loaded assets.
    ///
    /// After the first full settle for a load, later loading UI or icon-only
    /// activity is ignored so streaming assets do not force exposure to 0
    /// again. A new scene folder re-arms the workflow, while same-folder
    /// `isLoading` pulses from atom or toy changes do not.
    ///
    /// When exposure is skipped by the same-folder load guard, the one-shot
    /// settle still finishes once VaM is idle so fast loads do not wait on an
    /// exposure workflow that never ran. Worst case, 30 seconds after
    /// `isLoading` clears, the hold is forced to finish. Tick runs from
    /// `LateUpdate()` so CoreControl JSON usually reflects the scene before the
    /// exposure backup is read.
    /// </summary>
    public partial class SceneSettleRuntime
    {
        private bool wasSceneStillSettling = false;

        private bool lastSuperControllerIsLoading;

        private bool initialSceneLoadSettleWorkflowFinished;

        private bool hasObservedLoadStart;

        private string exposureWorkflowLastIdleLoadDirNorm = "";

        /// <summary>
        /// Emergency: release pause, audio hold, and exposure clamp, and mark
        /// first settle done (Space in session plugin).
        /// </summary>
        /// <returns>
        /// True if something was held or settle was incomplete; session hooks
        /// should run when true.
        /// </returns>
        public bool ForceReleaseHoldFromShortcut()
        {
            bool needFinish =
                sceneSettleSimulationPauseAppliedToSuperController ||
                camExposureBackupCaptured ||
                wasSceneStillSettling ||
                (!initialSceneLoadSettleWorkflowFinished &&
                    hasObservedLoadStart);

            if (!needFinish)
            {
                return false;
            }

            FinishSceneSettleExposureThenReleasePlaybackHold();
            initialSceneLoadSettleWorkflowFinished = true;
            wasSceneStillSettling = false;
            sceneSettlePauseHoldDeadlineActive = false;
            return true;
        }

        /// <summary>
        /// Returns true the first tick after VaM's loading or settle UI has
        /// cleared and playback hold was released.
        /// </summary>
        public bool TickDuringLoad(
            bool skipExposureWorkflowForCurrentLoad)
        {
            SuperController superController = SuperController.singleton;
            bool superControllerIsLoadingNow =
                superController != null && superController.isLoading;
            bool superControllerIsLoadingFellThisTick =
                lastSuperControllerIsLoading && !superControllerIsLoadingNow;

            if (!superControllerIsLoadingNow &&
                superController != null &&
                !string.IsNullOrEmpty(superController.currentLoadDir))
            {
                exposureWorkflowLastIdleLoadDirNorm =
                    SameFolderLoadCheck.NormalizeLoadDir(
                        superController.currentLoadDir);
            }

            if (superControllerIsLoadingNow && !lastSuperControllerIsLoading)
            {
                bool transientSameDirLoadPulse =
                    initialSceneLoadSettleWorkflowFinished &&
                    IsExposureTransientSameDirIsLoadingPulse(superController);

                if (!transientSameDirLoadPulse)
                {
                    hasObservedLoadStart = true;
                    ClearGlobalLightingCache();
                    GetSharedSceneSettlePauseAsyncFlag().Raise();
                    FinishSceneSettleExposureThenReleasePlaybackHold();
                    initialSceneLoadSettleWorkflowFinished = false;
                    wasSceneStillSettling = false;
                }
            }

            if (superControllerIsLoadingFellThisTick &&
                sceneSettleSimulationPauseAppliedToSuperController)
            {
                sceneSettlePauseHoldDeadlineUnscaledTime =
                    Time.unscaledTime + sceneSettlePauseHoldTimeoutSeconds;
                sceneSettlePauseHoldDeadlineActive = true;
            }

            lastSuperControllerIsLoading = superControllerIsLoadingNow;

            bool rawSceneSettlingIndicatorsActive =
                ShouldTreatSceneAsStillSettling();

            if (skipExposureWorkflowForCurrentLoad)
            {
                bool settleJustEndedFromSkipIdle = false;

                if (sceneSettleSimulationPauseAppliedToSuperController ||
                    camExposureBackupCaptured)
                {
                    FinishSceneSettleExposureThenReleasePlaybackHold();
                }

                if (!initialSceneLoadSettleWorkflowFinished &&
                    !rawSceneSettlingIndicatorsActive &&
                    !superControllerIsLoadingNow)
                {
                    initialSceneLoadSettleWorkflowFinished = true;
                    settleJustEndedFromSkipIdle = true;
                }

                wasSceneStillSettling = false;
                return settleJustEndedFromSkipIdle;
            }

            bool settleEndedThisTick = false;

            if (!rawSceneSettlingIndicatorsActive &&
                !superControllerIsLoadingNow &&
                sceneSettleSimulationPauseAppliedToSuperController)
            {
                FinishSceneSettleExposureThenReleasePlaybackHold();
                wasSceneStillSettling = false;
                initialSceneLoadSettleWorkflowFinished = true;
                settleEndedThisTick = true;
            }

            if (sceneSettleSimulationPauseAppliedToSuperController &&
                sceneSettlePauseHoldDeadlineActive)
            {
                if (Time.unscaledTime >=
                    sceneSettlePauseHoldDeadlineUnscaledTime)
                {
                    SuperController.LogMessage(
                        "[SceneSettle] Scene settle pause exceeded " +
                        sceneSettlePauseHoldTimeoutSeconds +
                        "s after isLoading cleared; forcing finish.");
                    FinishSceneSettleExposureThenReleasePlaybackHold();
                    settleEndedThisTick = true;
                    initialSceneLoadSettleWorkflowFinished = true;
                    wasSceneStillSettling = false;
                }
            }

            bool sceneSettlingForExposureWorkflow =
                rawSceneSettlingIndicatorsActive;

            if (initialSceneLoadSettleWorkflowFinished &&
                !superControllerIsLoadingNow)
            {
                sceneSettlingForExposureWorkflow = false;
            }
            else if (initialSceneLoadSettleWorkflowFinished &&
                superControllerIsLoadingNow &&
                IsExposureTransientSameDirIsLoadingPulse(superController))
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

            if (!initialSceneLoadSettleWorkflowFinished &&
                hasObservedLoadStart &&
                !rawSceneSettlingIndicatorsActive &&
                !superControllerIsLoadingNow &&
                !sceneSettleSimulationPauseAppliedToSuperController)
            {
                initialSceneLoadSettleWorkflowFinished = true;
                settleEndedThisTick = true;
            }

            return settleEndedThisTick;
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

        /// <summary>
        /// Hand spawn and other atom loads raise `isLoading` without changing
        /// `SuperController.currentLoadDir` compared to the last idle sample.
        /// </summary>
        bool IsExposureTransientSameDirIsLoadingPulse(
            SuperController superController)
        {
            if (superController == null ||
                string.IsNullOrEmpty(exposureWorkflowLastIdleLoadDirNorm))
            {
                return false;
            }

            string nowNorm = SameFolderLoadCheck.NormalizeLoadDir(
                superController.currentLoadDir);
            return nowNorm.Length > 0 &&
                string.Equals(
                    nowNorm,
                    exposureWorkflowLastIdleLoadDirNorm,
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}
