using System;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// When main monitor mode is on, draws simple <see cref="LineRenderer"/> beams from
    /// each motion controller to that side’s <c>LaserBeamDot</c> (VaM / Weelco layout).
    /// Stock mesh lasers often hide in that mode; these lines are independent and toggle
    /// with <see cref="SuperController.MonitorRig"/> activation.
    /// </summary>
    internal static class EasyMateMonitorModeLaserRestore
    {
        private const float BaseLineWidth = 0.003f;

        private static GameObject _root;

        private static LineRenderer _lineLeft;

        private static LineRenderer _lineRight;

        /// <summary>
        /// Call each <see cref="EasyMate.LateUpdate"/> with the plugin toggle value.
        /// </summary>
        public static void Tick(bool featureEnabled)
        {
            SuperController sc = SuperController.singleton;
            if (!featureEnabled || sc == null)
            {
                HideLasers();
                return;
            }

            if (sc.isLoading || sc.IsMonitorOnly || (!sc.isOVR && !sc.isOpenVR))
            {
                HideLasers();
                return;
            }

            if (sc.MonitorRig == null || !sc.MonitorRig.gameObject.activeSelf)
            {
                HideLasers();
                return;
            }

            EnsureLasers(sc);

            UpdateSide(sc, MotionLeft(sc), _lineLeft);
            UpdateSide(sc, MotionRight(sc), _lineRight);
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

        private static void EnsureLasers(SuperController sc)
        {
            if (_lineLeft != null && _lineRight != null && _root != null)
                return;

            if (_root == null)
            {
                _root = new GameObject("EasyMateMonitorDotLasers");
                _root.transform.SetParent(sc.transform, false);
            }

            if (_lineLeft == null)
                _lineLeft = CreateLineChild(_root.transform, "MonitorDotLaserLeft", Color.blue);
            if (_lineRight == null)
                _lineRight = CreateLineChild(_root.transform, "MonitorDotLaserRight", Color.red);
        }

        private static LineRenderer CreateLineChild(
            Transform parent,
            string name,
            Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            Shader sh = Shader.Find("Unlit/Color");
            if (sh != null)
            {
                Material mat = new Material(sh);
                mat.SetColor("_Color", color);
                lr.material = mat;
            }

            lr.enabled = false;
            return lr;
        }

        private static void UpdateSide(
            SuperController sc,
            Transform motion,
            LineRenderer line)
        {
            if (motion == null || line == null)
            {
                if (line != null)
                    line.enabled = false;
                return;
            }

            Transform dot = FindLaserBeamDotUnder(motion);
            if (dot == null)
            {
                line.enabled = false;
                return;
            }

            float w = BaseLineWidth * sc.worldScale;
            if (w < 0.0005f)
                w = 0.0005f;

            line.startWidth = w;
            line.endWidth = w;
            line.SetPosition(0, motion.position);
            line.SetPosition(1, dot.position);
            line.enabled = true;
        }

        /// <summary>
        /// Prefab path per Weelco <c>IUIHitPointer</c>, then name fallback.
        /// </summary>
        private static Transform FindLaserBeamDotUnder(Transform motionRoot)
        {
            if (motionRoot == null)
                return null;

            Transform byPath = FindChildPath(motionRoot, "LaserPointer/LaserBeamDot");
            if (byPath != null)
                return byPath;

            return FindTransformRecursive(motionRoot, IsLaserBeamDotName);
        }

        private static bool IsLaserBeamDotName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            string lower = name.ToLowerInvariant();
            return lower.Contains("laserbeamd")
                || lower.Contains("beamdot");
        }

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

        private static Transform FindTransformRecursive(
            Transform node,
            Func<string, bool> nameMatch)
        {
            if (node == null || nameMatch == null)
                return null;

            if (nameMatch(node.name))
                return node;

            int i;
            int cc = node.childCount;
            for (i = 0; i < cc; i++)
            {
                Transform found = FindTransformRecursive(node.GetChild(i), nameMatch);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static void HideLasers()
        {
            if (_lineLeft != null)
                _lineLeft.enabled = false;
            if (_lineRight != null)
                _lineRight.enabled = false;
        }

        public static void OnPluginDestroy()
        {
            HideLasers();
            if (_root != null)
            {
                Object.Destroy(_root);
                _root = null;
            }

            _lineLeft = null;
            _lineRight = null;
        }
    }
}
