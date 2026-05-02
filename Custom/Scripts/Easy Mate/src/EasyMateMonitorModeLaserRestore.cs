using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VaM’s <b>blue / red</b> controller “UI lasers” live under each motion controller as
    /// <c>LaserPointer/LaserBeam</c> (see Weelco <c>IUILaserPointer</c>). Oculus path
    /// <see cref="SuperController"/> uses <c>HideLeftController</c>/<c>HideRightController</c>, which disable
    /// <b>every</b> <see cref="MeshRenderer"/> under the touch objects — that kills the beam while monitor mode
    /// and HUD/menu logic often keep those hides active. The green lines toward atoms come from
    /// <see cref="SelectionHUD"/> and are unrelated.
    /// Runs from <see cref="EasyMateMonitorLaserCameraHook.OnPreRender"/> so it wins over later lifecycle hooks.
    /// </summary>
    internal static class EasyMateMonitorModeLaserRestore
    {
        /// <summary>Avoid a collapsed beam if nothing updates scale this frame.</summary>
        private const float MinBeamLocalScaleZ = 0.08f;

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

        /// <summary>Call from <see cref="EasyMate.LateUpdate"/>.</summary>
        public static void NotifyEnabledAndCleanup(bool enabled)
        {
            _featureEnabled = enabled;
            EnsureMonitorCameraHook();
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

            RestoreUiLaserUnderMotion(MotionLeft(sc));
            RestoreUiLaserUnderMotion(MotionRight(sc));
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

        /// <summary>
        /// Walks only real children (includes inactive — unlike <see cref="Transform.Find"/>, which skips inactive branches).
        /// </summary>
        private static Transform FindChildPath(Transform root, string slashPath)
        {
            if (root == null || string.IsNullOrEmpty(slashPath))
                return null;

            string[] segments = slashPath.Split('/');
            Transform current = root;
            int si;
            for (si = 0; si < segments.Length; si++)
            {
                string seg = segments[si];
                if (seg.Length == 0)
                    continue;

                Transform found = null;
                int ci;
                int cc = current.childCount;
                for (ci = 0; ci < cc; ci++)
                {
                    Transform ch = current.GetChild(ci);
                    if (ch.name == seg)
                    {
                        found = ch;
                        break;
                    }
                }

                if (found == null)
                    return null;
                current = found;
            }

            return current;
        }

        private static void RestoreUiLaserUnderMotion(Transform motionRoot)
        {
            if (motionRoot == null)
                return;

            Transform laserPointer = FindChildPath(motionRoot, "LaserPointer");
            if (laserPointer == null)
                return;

            laserPointer.gameObject.SetActive(true);

            Transform beam = FindChildPath(motionRoot, "LaserPointer/LaserBeam");
            if (beam != null)
            {
                beam.gameObject.SetActive(true);
                Renderer[] beamRends = beam.GetComponentsInChildren<Renderer>(true);
                int bi;
                for (bi = 0; bi < beamRends.Length; bi++)
                    beamRends[bi].enabled = true;

                Vector3 ls = beam.localScale;
                if (ls.z < MinBeamLocalScaleZ)
                    beam.localScale = new Vector3(ls.x, ls.y, MinBeamLocalScaleZ);
            }

            Transform dot = FindChildPath(motionRoot, "LaserPointer/LaserBeamDot");
            if (dot != null)
            {
                dot.gameObject.SetActive(true);
                Renderer[] dotRends = dot.GetComponentsInChildren<Renderer>(true);
                int di;
                for (di = 0; di < dotRends.Length; di++)
                    dotRends[di].enabled = true;
            }
        }

        public static void OnPluginDestroy()
        {
            if (_hookCamera != null)
            {
                EasyMateMonitorLaserCameraHook hook = _hookCamera.GetComponent<EasyMateMonitorLaserCameraHook>();
                if (hook != null)
                    Object.Destroy(hook);
                _hookCamera = null;
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
