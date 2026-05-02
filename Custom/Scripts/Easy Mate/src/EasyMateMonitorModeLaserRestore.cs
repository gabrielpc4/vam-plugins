using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VaM clears controller laser lines whenever the monitor rig is active. Selection dots still render.
    /// Redraw uses VaM’s materials after <see cref="SuperController.Update"/> — scheduled from
    /// <see cref="EasyMateMonitorLaserCameraHook.OnPreRender"/> so it cannot be undone by later
    /// <see cref="LateUpdate"/> callbacks on other scripts.
    /// </summary>
    internal static class EasyMateMonitorModeLaserRestore
    {
        private static LineDrawer _drawerLeft;

        private static LineDrawer _drawerRight;

        private static bool _featureEnabled;

        private static Camera _hookCamera;

        /// <summary>Attach to <see cref="SuperController.MonitorCenterCamera"/> when it exists; cheap after first success.</summary>
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

        /// <summary>Call from <see cref="EasyMate.LateUpdate"/>; tears down meshes when leaving monitor mode.</summary>
        public static void NotifyEnabledAndCleanup(bool enabled)
        {
            _featureEnabled = enabled;
            EnsureMonitorCameraHook();

            SuperController sc = SuperController.singleton;
            if (!enabled || sc == null || sc.MonitorRig == null || !sc.MonitorRig.gameObject.activeSelf)
                CleanupDrawers();
        }

        /// <summary>Invoked from monitor camera immediately before it renders.</summary>
        public static void BeforeMonitorCameraRender()
        {
            if (!_featureEnabled)
                return;

            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading || sc.IsMonitorOnly)
                return;

            if (!sc.isOVR && !sc.isOpenVR)
                return;

            if (sc.MonitorRig == null || !sc.MonitorRig.gameObject.activeSelf)
                return;

            // Do not gate on main HUD: ToggleMainMonitor() + HideMainHUD can leave mainHUD inactive while targeting runs.

            int layer = sc.gameObject.layer;
            float rayW = sc.rayLineWidth * sc.worldScale;

            Transform motionL = MotionLeft(sc);
            Transform motionR = MotionRight(sc);

            ApplySide(sc.rayLineMaterialLeft, sc.rayLineLeft, motionL, layer, rayW, ref _drawerLeft);
            ApplySide(sc.rayLineMaterialRight, sc.rayLineRight, motionR, layer, rayW, ref _drawerRight);
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

        private static void ApplySide(Material mat, LineRenderer lr, Transform motion, int layer, float width, ref LineDrawer drawer)
        {
            if (motion == null)
            {
                if (lr != null)
                    lr.gameObject.SetActive(false);
                return;
            }

            Vector3 origin = motion.position;
            Vector3 tip = origin + 50f * motion.forward;

            if (mat != null)
            {
                if (drawer == null)
                    drawer = new LineDrawer(mat);
                drawer.SetLinePoints(origin, tip);
                drawer.Draw(layer);
            }

            if (lr != null)
            {
                lr.transform.position = origin;
                lr.transform.rotation = motion.rotation;
                lr.startWidth = width;
                lr.endWidth = width;
                lr.gameObject.SetActive(true);
            }
        }

        public static void OnPluginDestroy()
        {
            CleanupDrawers();
            if (_hookCamera != null)
            {
                EasyMateMonitorLaserCameraHook hook = _hookCamera.GetComponent<EasyMateMonitorLaserCameraHook>();
                if (hook != null)
                    Object.Destroy(hook);
                _hookCamera = null;
            }
        }

        private static void CleanupDrawers()
        {
            if (_drawerLeft != null)
            {
                _drawerLeft.Destroy();
                _drawerLeft = null;
            }

            if (_drawerRight != null)
            {
                _drawerRight.Destroy();
                _drawerRight = null;
            }
        }
    }

    /// <summary>Bridge: <see cref="Camera.OnPreRender"/> for <see cref="SuperController.MonitorCenterCamera"/>.</summary>
    public class EasyMateMonitorLaserCameraHook : MonoBehaviour
    {
        private void OnPreRender()
        {
            EasyMateMonitorModeLaserRestore.BeforeMonitorCameraRender();
        }
    }
}
