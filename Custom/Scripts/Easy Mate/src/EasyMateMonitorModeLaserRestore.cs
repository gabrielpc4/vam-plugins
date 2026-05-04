using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// With main monitor mode on (<see cref="SuperController.MonitorRig"/> active), shows a
    /// thin blue (left) / red (right) cylinder along each motion controller’s forward
    /// while the user shows the UI aim gesture: on Oculus runtime, X left and A right
    /// capacitive (<c>OVRInput.Touch</c> on LTouch/RTouch); on OpenVR (SteamVR, including
    /// Virtual Desktop), <see cref="SuperController.GetLeftUIPointerShow"/> /
    /// <see cref="SuperController.GetRightUIPointerShow"/> (SteamVR TargetShow per hand).
    /// Hides when that input is inactive. Oculus and OpenVR paths are combined (OR), so
    /// either runtime is supported without preferring one over the other.
    /// </summary>
    internal static class EasyMateMonitorModeLaserRestore
    {
        private static readonly Color BeamBlue = new Color(0.1f, 0.42f, 1f, 1f);

        private static readonly Color BeamRed = new Color(1f, 0.2f, 0.12f, 1f);

        private static float _nextMonitorCameraSyncIssueLogTime = -1f;

        /// <summary>World-space beam radius before applying worldScale.</summary>
        private const float BaseRadiusM = 0.0015f;

        /// <summary>Beam length along aim (m), multiplied by worldScale.</summary>
        private const float BeamLengthM = 5f;

        private static GameObject _root;

        private static Transform _beamLeft;

        private static Transform _beamRight;

        /// <summary>
        /// Call from <see cref="EasyMate.LateUpdate"/> with plugin toggle.
        /// </summary>
        public static void Tick(bool featureEnabled)
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
            {
                HideBeams();
                return;
            }

            bool monitorModeActive = IsMonitorModeActive(sc);
            if (!monitorModeActive)
            {
                HideBeams();
                return;
            }

            SyncMonitorCameraToVrHeadset(sc);

            if (!featureEnabled)
            {
                HideBeams();
                return;
            }

            EnsureBeams(sc);

            bool capLeft = CapTouchLeft(sc);
            bool capRight = CapTouchRight(sc);

            if (capLeft)
                UpdateBeamForward(sc, MotionLeft(sc), _beamLeft);
            else
                HideOne(_beamLeft);

            if (capRight)
                UpdateBeamForward(sc, MotionRight(sc), _beamRight);
            else
                HideOne(_beamRight);
        }

        /// <summary>
        /// True when left UI-aim should show the beam: Oculus X capacitive (LTouch), or
        /// OpenVR SteamVR TargetShow via <see cref="SuperController.GetLeftUIPointerShow"/>.
        /// Both are evaluated so mixed or overlapping flags still work.
        /// </summary>
        private static bool CapTouchLeft(SuperController sc)
        {
            bool ovr =
                sc.isOVR
                && OVRInput.Get(OVRInput.Touch.Three, OVRInput.Controller.LTouch);
            bool openVr = sc.isOpenVR && sc.GetLeftUIPointerShow();
            return ovr || openVr;
        }

        /// <summary>
        /// True when right UI-aim should show the beam: Oculus A capacitive (RTouch), or
        /// OpenVR TargetShow via <see cref="SuperController.GetRightUIPointerShow"/>.
        /// </summary>
        private static bool CapTouchRight(SuperController sc)
        {
            bool ovr =
                sc.isOVR && OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.RTouch);
            bool openVr = sc.isOpenVR && sc.GetRightUIPointerShow();
            return ovr || openVr;
        }

        /// <summary>
        /// Motion-controller aim origin: OVR touch rig first, else OpenVR vive rig, then
        /// whichever reference SuperController still has (covers odd monitor/transition
        /// states).
        /// </summary>
        private static Transform MotionLeft(SuperController sc)
        {
            if (sc.isOVR && sc.touchObjectLeft != null)
                return sc.touchObjectLeft;
            if (sc.isOpenVR && sc.viveObjectLeft != null)
                return sc.viveObjectLeft;
            if (sc.touchObjectLeft != null)
                return sc.touchObjectLeft;
            if (sc.viveObjectLeft != null)
                return sc.viveObjectLeft;
            return null;
        }

        /// <summary>Same as <see cref="MotionLeft"/> for the right controller.</summary>
        private static Transform MotionRight(SuperController sc)
        {
            if (sc.isOVR && sc.touchObjectRight != null)
                return sc.touchObjectRight;
            if (sc.isOpenVR && sc.viveObjectRight != null)
                return sc.viveObjectRight;
            if (sc.touchObjectRight != null)
                return sc.touchObjectRight;
            if (sc.viveObjectRight != null)
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
                _beamLeft = CreateCylinder(_root.transform, "MonitorBeamLeft", BeamBlue);
            if (_beamRight == null)
                _beamRight = CreateCylinder(_root.transform, "MonitorBeamRight", BeamRed);
        }

        private static bool IsMonitorModeActive(SuperController sc)
        {
            if (sc == null)
                return false;

            if (sc.isLoading || sc.IsMonitorOnly || (!sc.isOVR && !sc.isOpenVR))
                return false;

            return sc.MonitorRig != null && sc.MonitorRig.gameObject.activeSelf;
        }

        private static void SyncMonitorCameraToVrHeadset(SuperController sc)
        {
            if (!IsMonitorModeActive(sc))
                return;

            Camera monitorCamera = sc.MonitorCenterCamera;
            if (monitorCamera == null)
            {
                LogMonitorCameraSyncIssue(
                    "Easy Mate monitor camera sync: MonitorCenterCamera is missing while monitor mode is active.");
                return;
            }

            Camera sourceCamera = sc.lookCamera;
            Transform sourceTransform = sourceCamera != null
                ? sourceCamera.transform
                : (sc.centerCameraTarget != null ? sc.centerCameraTarget.transform : null);
            if (sourceTransform == null)
            {
                LogMonitorCameraSyncIssue(
                    "Easy Mate monitor camera sync: no VR headset camera/target found while monitor mode is active.");
                return;
            }

            Transform monitorTransform = monitorCamera.transform;
            if ((monitorTransform.position - sourceTransform.position).sqrMagnitude > 1e-10f)
                monitorTransform.position = sourceTransform.position;
            if (Quaternion.Angle(monitorTransform.rotation, sourceTransform.rotation) > 0.001f)
                monitorTransform.rotation = sourceTransform.rotation;

            if (sourceCamera != null)
            {
                if (Mathf.Abs(monitorCamera.fieldOfView - sourceCamera.fieldOfView) > 0.0001f)
                    monitorCamera.fieldOfView = sourceCamera.fieldOfView;
                if (Mathf.Abs(monitorCamera.nearClipPlane - sourceCamera.nearClipPlane) > 0.000001f)
                    monitorCamera.nearClipPlane = sourceCamera.nearClipPlane;
                if (Mathf.Abs(monitorCamera.farClipPlane - sourceCamera.farClipPlane) > 0.001f)
                    monitorCamera.farClipPlane = sourceCamera.farClipPlane;
            }
        }

        private static void LogMonitorCameraSyncIssue(string message)
        {
            if (Time.unscaledTime < _nextMonitorCameraSyncIssueLogTime)
                return;

            _nextMonitorCameraSyncIssueLogTime = Time.unscaledTime + 5f;
            SuperController.LogError(message);
        }

        private static Material CreateBeamMaterial(Color color)
        {
            Shader sh = Shader.Find("Standard");
            if (sh == null)
                sh = Shader.Find("Legacy Shaders/Diffuse");
            if (sh == null)
                sh = Shader.Find("Unlit/Color");
            Material mat = new Material(sh);
            mat.SetColor("_Color", color);
            if (sh != null && sh.name != null && sh.name.IndexOf("Unlit") < 0)
            {
                mat.SetFloat("_Metallic", 0f);
                mat.SetFloat("_Glossiness", 0.35f);
            }

            return mat;
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
                mr.material = CreateBeamMaterial(color);

                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }

            go.SetActive(false);
            return go.transform;
        }

        private static void UpdateBeamForward(
            SuperController sc,
            Transform motion,
            Transform beam)
        {
            if (motion == null || beam == null)
            {
                HideOne(beam);
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

        private static void HideOne(Transform beam)
        {
            if (beam != null)
                beam.gameObject.SetActive(false);
        }

        private static void HideBeams()
        {
            HideOne(_beamLeft);
            HideOne(_beamRight);
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
