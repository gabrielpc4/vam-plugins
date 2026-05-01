using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Console snapshots for aligning scenes: <see cref="SuperController.playerHeightAdjust"/>, monitor tilt,
    /// and atom poses for <c>WindowCamera</c> / <c>[CameraRig]</c>.
    /// </summary>
    internal static class EasyMateCameraPoseLog
    {
        public static void LogSnapshot(string reason)
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
            {
                SuperController.LogMessage("EasyMate camera pose [" + reason + "] SuperController.singleton is null");
                return;
            }

            SuperController.LogMessage("EasyMate camera pose [" + reason + "] playerHeightAdjust=" + sc.playerHeightAdjust.ToString("G9"));

            if (sc.MonitorCenterCamera != null)
            {
                Vector3 e = sc.MonitorCenterCamera.transform.localEulerAngles;
                SuperController.LogMessage("EasyMate camera pose [" + reason + "] monitorCameraRotation (MonitorCenterCamera.localEulerAngles)=" + e.ToString("G9"));
            }
            else
            {
                SuperController.LogMessage("EasyMate camera pose [" + reason + "] monitorCameraRotation: MonitorCenterCamera is null");
            }

            LogAtomControlPose(reason, sc, "WindowCamera");
            LogAtomControlPose(reason, sc, "[CameraRig]");
        }

        private static void LogAtomControlPose(string reason, SuperController sc, string uid)
        {
            Atom a = sc.GetAtomByUid(uid);
            if (a == null)
            {
                SuperController.LogMessage("EasyMate camera pose [" + reason + "] " + uid + ": atom missing");
                return;
            }

            FreeControllerV3 mc = a.mainController;
            if (mc == null || mc.control == null)
            {
                SuperController.LogMessage("EasyMate camera pose [" + reason + "] " + uid + ": mainController/control missing");
                return;
            }

            Transform t = mc.control;
            SuperController.LogMessage("EasyMate camera pose [" + reason + "] " + uid + " position=" + t.position.ToString("G9") + " rotation(euler)=" + t.rotation.eulerAngles.ToString("G9"));
        }
    }
}
