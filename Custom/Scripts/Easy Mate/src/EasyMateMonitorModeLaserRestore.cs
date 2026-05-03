using MeshVR;
using UnityEngine;
using System.Collections.Generic;

namespace geesp0t
{
    /// <summary>
    /// VaM’s blue/red controller UI lasers are not the green SelectionHUD
    /// lines. The laser beam usually lives under a controller subtree such as
    /// LaserPointer/LaserBeam, but some rigs differ, so this searches by
    /// likely names and keeps re-applying in monitor mode.
    /// </summary>
    internal static class EasyMateMonitorModeLaserRestore
    {
        /// <summary>
        /// Avoid a collapsed beam if nothing updates scale this frame.
        /// </summary>
        private const float MinBeamLocalScaleZ = 0.08f;

        private const float HeartbeatSeconds = 1f;

        private static bool _featureEnabled;

        private static Camera _hookCamera;

        private static float _nextHeartbeatTime;

        private static readonly List<Transform> _laserNodes = new List<Transform>();

        /// <summary>
        /// Attach to <see cref="SuperController.MonitorCenterCamera"/>
        /// when it exists; cheap after first success.
        /// </summary>
        public static void EnsureMonitorCameraHook()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.MonitorCenterCamera == null)
                return;

            Camera mc = sc.MonitorCenterCamera;
            if (_hookCamera == mc && mc.GetComponent<EasyMateMonitorLaserCameraHook>() != null)
                return;

            if (_hookCamera != null && _hookCamera != mc)
            {
                EasyMateMonitorLaserCameraHook oldHook = _hookCamera.GetComponent<EasyMateMonitorLaserCameraHook>();
                if (oldHook != null)
                    Object.Destroy(oldHook);
            }

            _hookCamera = mc;
            if (mc.GetComponent<EasyMateMonitorLaserCameraHook>() == null)
                mc.gameObject.AddComponent<EasyMateMonitorLaserCameraHook>();
        }

        /// <summary>
        /// Call from <see cref="EasyMate.LateUpdate"/>.
        /// </summary>
        public static void NotifyEnabledAndCleanup(bool enabled)
        {
            _featureEnabled = enabled;
            EnsureMonitorCameraHook();
            if (!enabled)
                return;

            if (Time.unscaledTime >= _nextHeartbeatTime)
            {
                _nextHeartbeatTime = Time.unscaledTime + HeartbeatSeconds;
                AttemptRestore("heartbeat", true);
            }
        }

        /// <summary>
        /// Invoked from monitor camera immediately before it renders.
        /// </summary>
        public static void BeforeMonitorCameraRender()
        {
            AttemptRestore("pre-render", false);
        }

        private static Transform MotionLeft(SuperController sc)
        {
            if (sc.isOVR && sc.touchObjectLeft != null)
                return sc.touchObjectLeft;
            if (sc.isOpenVR && sc.viveObjectLeft != null)
                return sc.viveObjectLeft;
            return null;
        }

        private static Transform MotionRight(SuperController sc)
        {
            if (sc.isOVR && sc.touchObjectRight != null)
                return sc.touchObjectRight;
            if (sc.isOpenVR && sc.viveObjectRight != null)
                return sc.viveObjectRight;
            return null;
        }

        private static void AttemptRestore(string reason, bool logAttempt)
        {
            if (!_featureEnabled)
                return;

            SuperController sc = SuperController.singleton;
            if (sc == null)
            {
                if (logAttempt)
                    SuperController.LogMessage(
                        "EasyMate monitor laser restore [" + reason
                        + "]: skipped (no SuperController)");
                return;
            }

            bool active =
                !sc.isLoading
                && !sc.IsMonitorOnly
                && (sc.isOVR || sc.isOpenVR)
                && sc.MonitorRig != null
                && sc.MonitorRig.gameObject.activeSelf;

            if (!active)
            {
                if (logAttempt)
                {
                    SuperController.LogMessage(
                        "EasyMate monitor laser restore [" + reason + "]: "
                        + "skipped"
                        + " isLoading=" + sc.isLoading
                        + " isMonitorOnly=" + sc.IsMonitorOnly
                        + " isOVR=" + sc.isOVR
                        + " isOpenVR=" + sc.isOpenVR
                        + " monitorRig="
                        + (sc.MonitorRig != null
                            ? sc.MonitorRig.gameObject.activeSelf.ToString()
                            : "null"));
                }
                return;
            }

            int leftFound = RestoreUiLaserUnderMotion(MotionLeft(sc));
            int rightFound = RestoreUiLaserUnderMotion(MotionRight(sc));
            if (logAttempt)
            {
                SuperController.LogMessage(
                    "EasyMate monitor laser restore [" + reason + "]: "
                    + "left=" + leftFound + " right=" + rightFound);
            }
        }

        private static int RestoreUiLaserUnderMotion(Transform motionRoot)
        {
            if (motionRoot == null)
                return 0;

            _laserNodes.Clear();
            CollectLikelyLaserNodes(motionRoot, _laserNodes);

            int i;
            for (i = 0; i < _laserNodes.Count; i++)
            {
                Transform t = _laserNodes[i];
                if (t == null)
                    continue;

                t.gameObject.SetActive(true);

                Renderer[] rends = t.GetComponentsInChildren<Renderer>(true);
                int ri;
                for (ri = 0; ri < rends.Length; ri++)
                    rends[ri].enabled = true;

                if (LooksLikeBeam(t.name))
                {
                    Vector3 ls = t.localScale;
                    if (ls.z < MinBeamLocalScaleZ)
                    {
                        t.localScale = new Vector3(
                            ls.x,
                            ls.y,
                            MinBeamLocalScaleZ);
                    }
                }
            }

            return _laserNodes.Count;
        }

        private static void CollectLikelyLaserNodes(
            Transform root,
            List<Transform> results)
        {
            if (root == null)
                return;

            string nameLower = root.name != null
                ? root.name.ToLowerInvariant()
                : string.Empty;
            if (IsLikelyLaserNode(nameLower))
                results.Add(root);

            int i;
            int cc = root.childCount;
            for (i = 0; i < cc; i++)
                CollectLikelyLaserNodes(root.GetChild(i), results);
        }

        private static bool IsLikelyLaserNode(string nameLower)
        {
            if (string.IsNullOrEmpty(nameLower))
                return false;

            if (nameLower.Contains("laserpointer"))
                return true;
            if (nameLower.Contains("laserbeam"))
                return true;
            if (nameLower.Contains("beamdot"))
                return true;
            if (nameLower.Contains("laser") && nameLower.Contains("beam"))
                return true;
            if (nameLower.Contains("pointer") && nameLower.Contains("beam"))
                return true;

            return false;
        }

        private static bool LooksLikeBeam(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            string nameLower = name.ToLowerInvariant();
            return nameLower.Contains("beam")
                && !nameLower.Contains("dot");
        }

        public static void OnPluginDestroy()
        {
            _nextHeartbeatTime = 0f;
            if (_hookCamera != null)
            {
                EasyMateMonitorLaserCameraHook hook = _hookCamera.GetComponent<EasyMateMonitorLaserCameraHook>();
                if (hook != null)
                    Object.Destroy(hook);
                _hookCamera = null;
            }
        }
    }

    /// <summary>
    /// Bridge: <see cref="Camera.OnPreRender"/> for
    /// <see cref="SuperController.MonitorCenterCamera"/>.
    /// </summary>
    public class EasyMateMonitorLaserCameraHook : MonoBehaviour
    {
        private void OnPreRender()
        {
            EasyMateMonitorModeLaserRestore.BeforeMonitorCameraRender();
        }
    }
}
