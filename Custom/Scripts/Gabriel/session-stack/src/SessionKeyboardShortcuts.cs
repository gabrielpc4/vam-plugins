using UnityEngine;
using UnityEngine.EventSystems;

namespace geesp0t
{
    /// <summary>
    /// Keyboard shortcuts for the Gabriel session stack.
    /// <b>Space</b> forces release of the current scene-settle hold.
    /// </summary>
    public class SessionKeyboardShortcuts
    {
        private MVRScript pluginHost;
        private SceneSettleRuntime sceneSettle;

        public void Init(MVRScript host, SceneSettleRuntime sceneSettle)
        {
            pluginHost = host;
            this.sceneSettle = sceneSettle;
        }

        public void ProcessHotkeysUpdate()
        {
            if (pluginHost == null || SuperController.singleton == null)
                return;

            bool noTextFocus =
                EventSystem.current == null ||
                EventSystem.current.currentSelectedGameObject == null;
            bool noCtrlAlt =
                !Input.GetKey(KeyCode.LeftControl) &&
                !Input.GetKey(KeyCode.RightControl) &&
                !Input.GetKey(KeyCode.LeftAlt) &&
                !Input.GetKey(KeyCode.RightAlt);

            if (noTextFocus && noCtrlAlt && Input.GetKeyDown(KeyCode.Space) &&
                sceneSettle != null)
            {
                if (sceneSettle.ForceReleaseHoldFromShortcut())
                {
                    HeadProximityHide.AfterSuperControllerFinishedSceneSettle(
                        pluginHost);
                    SuperController.LogMessage(
                        "Gabriel session stack: Space released scene settle " +
                        "hold.");
                }
            }
        }

        public void OnDestroy()
        {
            pluginHost = null;
            sceneSettle = null;
        }
    }
}
