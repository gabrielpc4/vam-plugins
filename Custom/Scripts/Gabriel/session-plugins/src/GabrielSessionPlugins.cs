using UnityEngine;

namespace geesp0t
{
    public class GabrielSessionPlugins : MVRScript
    {
        private const string ForceReleaseSceneSettleHoldActionName =
            "ForceReleaseSceneSettleHold";

        private JSONStorableString explanationString;

        private JSONStorableAction forceReleaseSceneSettleHoldAction;

        private bool prevSuperControllerIsLoading = false;

        // Same-folder pulses come from loads like atom/toy changes and should not
        // re-run the full settle workflow.
        private bool skipSceneSettleWorkflowForPendingLoad = false;

        private SameFolderSceneLoadCheck sameFolderSceneLoadCheck =
            new SameFolderSceneLoadCheck();

        private SceneSettleRuntime sceneSettle = new SceneSettleRuntime();

        public override void Init()
        {
            explanationString = new JSONStorableString(
                "",
                "Gabriel session plugins handle scene-settle playback hold, " +
                "same-folder load suppression, and the scene-settle release " +
                "action used by the shared hotkey dispatcher.");
            UIDynamicTextField dtext = CreateTextField(explanationString);
            dtext.height = 420;

            forceReleaseSceneSettleHoldAction = new JSONStorableAction(
                ForceReleaseSceneSettleHoldActionName,
                ForceReleaseSceneSettleHoldFromAction);
            RegisterAction(forceReleaseSceneSettleHoldAction);
        }

        public void Update()
        {
            SuperController superController = SuperController.singleton;
            bool superLoadingNow;

            if (superController == null)
                return;

            superLoadingNow = superController.isLoading;
            if (!prevSuperControllerIsLoading && superLoadingNow)
            {
                skipSceneSettleWorkflowForPendingLoad =
                    sameFolderSceneLoadCheck.IsSameFolderLoad(superController);
            }
            else if (!superLoadingNow)
            {
                sameFolderSceneLoadCheck.CaptureIdleLoadDir(superController);
            }
            prevSuperControllerIsLoading = superLoadingNow;
        }

        public void LateUpdate()
        {
            bool sceneSettleJustEnded;

            sceneSettleJustEnded =
                sceneSettle.TickDuringLoad(
                    skipSceneSettleWorkflowForPendingLoad);
            if (sceneSettleJustEnded)
            {
                HeadProximityHide.AfterSuperControllerFinishedSceneSettle(this);
            }
        }

        public void OnDestroy()
        {
            sceneSettle.OnPluginDestroy();
        }

        private void ForceReleaseSceneSettleHoldFromAction()
        {
            if (!sceneSettle.ForceReleaseHoldFromShortcut())
            {
                return;
            }

            HeadProximityHide.AfterSuperControllerFinishedSceneSettle(this);
            SuperController.LogMessage(
                "Gabriel session plugins: Space released scene settle hold.");
        }
    }
}
