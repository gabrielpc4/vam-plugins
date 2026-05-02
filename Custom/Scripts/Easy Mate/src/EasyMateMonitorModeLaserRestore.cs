using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VaM clears controller laser lines whenever <see cref="SuperController"/>'s monitor rig is active
    /// (<c>drawRayLine* = !MonitorRigActive</c>, and the VR-only branch that forces rays on skips monitor mode).
    /// Selection dots still render; this restores the beam in <see cref="UnityEngine.LineRenderer"/> +
    /// <see cref="LineDrawer"/> form after <see cref="SuperController.Update"/>.
    /// </summary>
    internal static class EasyMateMonitorModeLaserRestore
    {
        private static LineDrawer _drawerLeft;

        private static LineDrawer _drawerRight;

        public static void LateTick(bool enabled)
        {
            if (!enabled)
            {
                CleanupDrawers();
                return;
            }

            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading || sc.IsMonitorOnly)
            {
                CleanupDrawers();
                return;
            }

            if (!sc.isOVR && !sc.isOpenVR)
            {
                CleanupDrawers();
                return;
            }

            if (sc.MonitorRig == null || !sc.MonitorRig.gameObject.activeSelf)
            {
                CleanupDrawers();
                return;
            }

            if (sc.mainHUD == null || !sc.mainHUD.gameObject.activeInHierarchy)
                return;

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
}
