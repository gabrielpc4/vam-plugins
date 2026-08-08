using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// If monitor FOV matches VaM defaults, bump slightly for SceneControlSuite desktops.
    /// </summary>
    internal static class DefaultMonitorCameraFov
    {
        private const float VamDefaultMonitorCameraFov = 40f;

        private const float SceneControlSuiteDefaultMonitorCameraFov = 50f;

        internal static void ApplySceneControlSuitePreferenceIfStillStock(SuperController sc)
        {
            if (sc == null)
                return;

            if (Mathf.Abs(sc.monitorCameraFOV - VamDefaultMonitorCameraFov) > 0.001f)
                return;

            sc.monitorCameraFOV = SceneControlSuiteDefaultMonitorCameraFov;
        }
    }
}
