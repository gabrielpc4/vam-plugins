using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// HMD-relative euler checks for the right-hand back-of-hand HUD pose.
    /// </summary>
    public static class VrEulerPossessPoseCheck
    {
        /// <summary>Right-hand HUD: euler X must exceed this (degrees, 0-360).</summary>
        public const float RightPalmHudMinEulerX = 300f;

        /// <summary>
        /// Right palm HUD only: Z window for back-of-hand (“look at watch”);
        /// this is the only surviving hand-pose window.
        /// </summary>
        public const float RightPalmHudMinEulerZ = 110f;
        public const float RightPalmHudMaxEulerZ = 150f;

        public static Transform ResolveHmdTransform(SuperController sc)
        {
            if (sc == null)
                return null;
            if (sc.lookCamera != null)
                return sc.lookCamera.transform;
            if (sc.centerCameraTarget != null)
                return sc.centerCameraTarget.transform;
            return null;
        }

        /// <summary>
        /// Right hand, palm HUD only (back of hand toward HMD).
        /// </summary>
        public static bool RightPalmHudMatches(Vector3 e)
        {
            float x = e.x;
            float z = e.z;
            if (x <= RightPalmHudMinEulerX)
                return false;
            if (z <= RightPalmHudMinEulerZ)
                return false;
            if (z >= RightPalmHudMaxEulerZ)
                return false;
            return true;
        }

        /// <summary>
        /// HMD-relative rotation of <paramref name="hand"/>; euler per axis
        /// in [0, 360).
        /// </summary>
        public static bool TryHmdRelativeEuler360(
            Transform hand,
            Transform hmdTf,
            out Vector3 euler360)
        {
            euler360 = Vector3.zero;
            if (hand == null || hmdTf == null)
                return false;
            Quaternion rel =
                Quaternion.Inverse(hmdTf.rotation) * hand.rotation;
            Vector3 e = rel.eulerAngles;
            euler360.x = NormalizeEuler360(e.x);
            euler360.y = NormalizeEuler360(e.y);
            euler360.z = NormalizeEuler360(e.z);
            return true;
        }

        public static float NormalizeEuler360(float degrees)
        {
            float d = degrees % 360f;
            if (d < 0f)
                d += 360f;
            return d;
        }

        public static bool RightHandOnlyMatchTriggerWindow(SuperController sc)
        {
            Vector3 ignore;
            return RightHandOnlyMatchTriggerWindow(sc, out ignore);
        }

        /// <summary>
        /// Only <c>rightHand</c> euler window (VR palm HUD). Ignores left hand.
        /// </summary>
        public static bool RightHandOnlyMatchTriggerWindow(
            SuperController sc,
            out Vector3 rightEuler)
        {
            rightEuler = Vector3.zero;
            Transform hmd = ResolveHmdTransform(sc);
            if (sc == null || hmd == null)
                return false;
            Transform rh = sc.rightHand;
            if (rh == null)
                return false;
            if (!TryHmdRelativeEuler360(rh, hmd, out rightEuler))
                return false;
            return RightPalmHudMatches(rightEuler);
        }
    }
}
