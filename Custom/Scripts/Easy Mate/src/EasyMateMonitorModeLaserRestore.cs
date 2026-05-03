using MeshVR;
using UnityEngine;
using UnityEngine.UI;

namespace geesp0t
{
    /// <summary>
    /// With main monitor mode on (<see cref="SuperController.MonitorRig"/> active), shows a
    /// thin blue/red cylinder per hand along the controller <c>forward</c> only when a physics ray
    /// hits an active, interactable <see cref="Button"/> in the collider’s parents. Max length scales
    /// with <see cref="SuperController.worldScale"/>; visible length ends at the hit.
    /// </summary>
    internal static class EasyMateMonitorModeLaserRestore
    {
        private static readonly Color BeamBlue = new Color(0.1f, 0.42f, 1f, 1f);

        private static readonly Color BeamRed = new Color(1f, 0.2f, 0.12f, 1f);

        /// <summary>World-space beam radius before applying worldScale.</summary>
        private const float BaseRadiusM = 0.0015f;

        /// <summary>Max aim ray length (m), multiplied by worldScale.</summary>
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
                _beamLeft = CreateCylinder(_root.transform, "MonitorBeamLeft", BeamBlue);
            if (_beamRight == null)
                _beamRight = CreateCylinder(_root.transform, "MonitorBeamRight", BeamRed);
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
                    mat.color = color;
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

            float maxLen = BeamLengthM * ws;
            RaycastHit hit;
            if (!TryNearestUIButtonHit(motion, maxLen, out hit))
            {
                beam.gameObject.SetActive(false);
                return;
            }

            float len = hit.distance;
            if (len < 0.02f * ws)
                len = 0.02f * ws;

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

        /// <summary>
        /// Pick the closest raycast hit whose hierarchy has an interactable UI
        /// <see cref="Button"/>.
        /// </summary>
        private static bool TryNearestUIButtonHit(
            Transform motion,
            float maxDistance,
            out RaycastHit outHit)
        {
            outHit = default(RaycastHit);
            if (motion == null || maxDistance <= 0f)
                return false;

            RaycastHit[] hits = Physics.RaycastAll(
                motion.position,
                motion.forward,
                maxDistance,
                -1,
                QueryTriggerInteraction.Collide);

            if (hits == null || hits.Length == 0)
                return false;

            float bestDist = float.MaxValue;
            RaycastHit best = default(RaycastHit);
            Button bestBtn = null;
            int i;
            for (i = 0; i < hits.Length; i++)
            {
                RaycastHit h = hits[i];
                if (h.collider == null)
                    continue;

                Button b = h.collider.GetComponentInParent<Button>();
                if (b == null)
                    continue;
                if (!b.isActiveAndEnabled || !b.interactable)
                    continue;

                if (h.distance < bestDist)
                {
                    bestDist = h.distance;
                    best = h;
                    bestBtn = b;
                }
            }

            if (bestBtn == null)
                return false;

            outHit = best;
            return true;
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
