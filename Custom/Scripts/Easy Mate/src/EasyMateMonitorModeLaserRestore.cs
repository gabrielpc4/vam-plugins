using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// With main monitor mode on (<see cref="SuperController.MonitorRig"/> active), shows a
    /// thin cylinder per hand aligned to each motion controller’s <c>forward</c> (length scales
    /// with <see cref="SuperController.worldScale"/>). Independent of stock UI lasers; hides when
    /// monitor mode or the plugin toggle is off.
    /// </summary>
    internal static class EasyMateMonitorModeLaserRestore
    {
        /// <summary>World-space beam radius before applying worldScale.</summary>
        private const float BaseRadiusM = 0.0015f;

        /// <summary>Beam length along aim (m), multiplied by worldScale.</summary>
        private const float BeamLengthM = 5f;

        private static GameObject _root;

        private static Transform _beamLeft;

        private static Transform _beamRight;

        /// <summary>Call from <see cref="EasyMate.LateUpdate"/> with plugin toggle.</summary>
        public static void Tick(bool featureEnabled)
        {
            SuperController sc = SuperController.singleton;
            if (!featureEnabled || sc == null)
            {
                HideBeams();
                return;
            }

            if (sc.isLoading || sc.IsMonitorOnly || (!sc.isOVR && !sc.isOpenVR))
            {
                HideBeams();
                return;
            }

            if (sc.MonitorRig == null || !sc.MonitorRig.gameObject.activeSelf)
            {
                HideBeams();
                return;
            }

            EnsureBeams(sc);

            UpdateBeam(sc, MotionLeft(sc), _beamLeft);
            UpdateBeam(sc, MotionRight(sc), _beamRight);
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

        private static void EnsureBeams(SuperController sc)
        {
            if (_beamLeft != null && _beamRight != null && _root != null)
                return;

            if (_root == null)
            {
                _root = new GameObject("EasyMateMonitorForwardBeams");
                _root.transform.SetParent(sc.transform, false);
            }

            if (_beamLeft == null)
                _beamLeft = CreateCylinder(_root.transform, "MonitorBeamLeft", Color.blue);
            if (_beamRight == null)
                _beamRight = CreateCylinder(_root.transform, "MonitorBeamRight", Color.red);
        }

        /// <summary>
        /// Unity cylinder: height 2 on local Y, radius 0.5 on X/Z at scale 1.
        /// </summary>
        private static Transform CreateCylinder(
            Transform parent,
            string objectName,
            Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = objectName;
            go.transform.SetParent(parent, false);
            Collider col = go.GetComponent<Collider>();
            if (col != null)
                UnityEngine.Object.Destroy(col);
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Shader sh = Shader.Find("Unlit/Color");
                if (sh != null)
                {
                    Material mat = new Material(sh);
                    mat.SetColor("_Color", color);
                    mr.material = mat;
                }

                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }

            go.SetActive(false);
            return go.transform;
        }

        private static void UpdateBeam(
            SuperController sc,
            Transform motion,
            Transform beam)
        {
            if (motion == null || beam == null)
            {
                if (beam != null)
                    beam.gameObject.SetActive(false);
                return;
            }

            float ws = sc.worldScale;
            if (ws < 0.01f)
                ws = 0.01f;

            float len = BeamLengthM * ws;
            float radiusWorld = BaseRadiusM * ws;
            if (radiusWorld < 0.0003f)
                radiusWorld = 0.0003f;

            Quaternion align = Quaternion.FromToRotation(Vector3.up, motion.forward);
            beam.rotation = align;
            beam.position = motion.position + motion.forward * (len * 0.5f);

            float halfHeightScale = len * 0.5f;
            float radialScale = radiusWorld / 0.5f;
            beam.localScale = new Vector3(
                radialScale,
                halfHeightScale,
                radialScale);

            beam.gameObject.SetActive(true);
        }

        private static void HideBeams()
        {
            if (_beamLeft != null)
                _beamLeft.gameObject.SetActive(false);
            if (_beamRight != null)
                _beamRight.gameObject.SetActive(false);
        }

        public static void OnPluginDestroy()
        {
            HideBeams();
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }

            _beamLeft = null;
            _beamRight = null;
        }
    }
}
