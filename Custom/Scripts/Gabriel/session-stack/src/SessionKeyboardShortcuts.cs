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
        private OnSceneStartup onSceneStartup;

        public void Init(MVRScript host, OnSceneStartup onSceneStartup)
        {
            pluginHost = host;
            this.onSceneStartup = onSceneStartup;
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
                onSceneStartup != null)
            {
                if (onSceneStartup.ForceReleaseSceneSettleHoldUserKey())
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
            onSceneStartup = null;
        }
    }
}
