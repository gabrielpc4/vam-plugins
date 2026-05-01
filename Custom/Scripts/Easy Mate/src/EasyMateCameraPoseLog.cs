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

            LogAtomControlPose(reason, sc, "WindowCamera", logNavigationRig: false);
            LogAtomControlPose(reason, sc, "[CameraRig]", logNavigationRig: true);
        }

        private static void LogAtomControlPose(string reason, SuperController sc, string uid, bool logNavigationRig)
        {
            Atom a = sc.GetAtomByUid(uid);
            if (a == null)
            {
                SuperController.LogMessage("EasyMate camera pose [" + reason + "] " + uid + ": atom missing");
                if (logNavigationRig)
                    LogNavigationRigLine(reason, sc);
                return;
            }

            string src;
            Transform t = ResolveAtomWorldPoseTransform(a, out src);
            if (t != null)
            {
                SuperController.LogMessage("EasyMate camera pose [" + reason + "] " + uid + " (" + src + ") position=" + t.position.ToString("G9") + " rotation(euler)=" + t.rotation.eulerAngles.ToString("G9"));
            }
            else
            {
                SuperController.LogMessage("EasyMate camera pose [" + reason + "] " + uid + ": no usable transform (main/control/root)");
            }

            if (logNavigationRig)
                LogNavigationRigLine(reason, sc);
        }

        /// <summary><c>[CameraRig]</c> is often a <c>VRController</c> without <see cref="Atom.mainController"/> set during early load; VaM still drives <see cref="SuperController.navigationRig"/>.</summary>
        private static void LogNavigationRigLine(string reason, SuperController sc)
        {
            Transform nr = sc.navigationRig;
            if (nr == null)
            {
                SuperController.LogMessage("EasyMate camera pose [" + reason + "] [CameraRig] SuperController.navigationRig: null");
                return;
            }

            SuperController.LogMessage("EasyMate camera pose [" + reason + "] [CameraRig] SuperController.navigationRig position=" + nr.position.ToString("G9") + " rotation(euler)=" + nr.rotation.eulerAngles.ToString("G9"));
        }

        private static Transform ResolveAtomWorldPoseTransform(Atom a, out string sourceLabel)
        {
            sourceLabel = "";
            if (a == null)
                return null;

            FreeControllerV3 mc = a.mainController;
            if (mc != null && mc.control != null)
            {
                sourceLabel = "mainController.control";
                return mc.control;
            }

            FreeControllerV3 ctrl = a.GetStorableByID("control") as FreeControllerV3;
            if (ctrl != null && ctrl.control != null)
            {
                sourceLabel = "control";
                return ctrl.control;
            }

            sourceLabel = "atom.transform";
            return a.transform;
        }
    }
}
