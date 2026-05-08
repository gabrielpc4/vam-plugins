using UnityEngine;

namespace geesp0t
{
    public class GabrielSessionStack : MVRScript
    {
        private JSONStorableString explanationString;

        private SessionKeyboardShortcuts keyboardShortcuts = null;

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
                "Gabriel session stack handles scene-settle playback hold, " +
                "same-folder load suppression, and session keyboard " +
                "shortcuts.\n\n" +
                "Space forces release of the current scene-settle hold.");
            UIDynamicTextField dtext = CreateTextField(explanationString);
            dtext.height = 420;

            keyboardShortcuts = new SessionKeyboardShortcuts();
            keyboardShortcuts.Init(this, sceneSettle);
        }


        public void Update()
        {
            SuperController superController = SuperController.singleton;
            bool superLoadingNow;

            if (keyboardShortcuts != null)
                keyboardShortcuts.ProcessHotkeysUpdate();

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

            if (keyboardShortcuts != null)
                keyboardShortcuts.OnDestroy();
        }

    }
}
 