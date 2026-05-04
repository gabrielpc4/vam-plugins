using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// HMD-relative euler windows for VR euler possess and the angle test HUD.
    /// </summary>
    public static class EasyMateVrEulerPossessPoseCheck
    {
        /// <summary>Left: euler X &gt; this (degrees, 0–360).</summary>
        public const float LeftMinEulerX = 300f;

        /// <summary>Left: Z strictly between these (degrees).</summary>
        public const float LeftMinEulerZ = 30f;
        public const float LeftMaxEulerZ = 90f;

        /// <summary>Right: euler X &gt; this (degrees, 0–360).</summary>
        public const float RightMinEulerX = 300f;

        /// <summary>Right: Z strictly between these (degrees).</summary>
        public const float RightMinEulerZ = 290f;
        public const float RightMaxEulerZ = 330f;

        /// <summary>
        /// Same HMD reference as <see cref="EasyMateVrGestureRuntime"/>.
        /// </summary>
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

        public static string FormatEuler(Vector3 e)
        {
            return "(" +
                e.x.ToString("F1") + "," +
                e.y.ToString("F1") + "," +
                e.z.ToString("F1") + ")";
        }

        /// <summary>
        /// <paramref name="e"/> from <see cref="TryHmdRelativeEuler360"/>.
        /// </summary>
        public static bool LeftMatches(Vector3 e)
        {
            float x = e.x;
            float z = e.z;
            if (x <= LeftMinEulerX)
                return false;
            if (z <= LeftMinEulerZ)
                return false;
            if (z >= LeftMaxEulerZ)
                return false;
            return true;
        }

        /// <summary>
        /// <paramref name="e"/> from <see cref="TryHmdRelativeEuler360"/>.
        /// </summary>
        public static bool RightMatches(Vector3 e)
        {
            float x = e.x;
            float z = e.z;
            if (x <= RightMinEulerX)
                return false;
            if (z <= RightMinEulerZ)
                return false;
            if (z >= RightMaxEulerZ)
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

        /// <summary>
        /// Both gesture euler checks pass (ignores dwell/cooldown). Writes
        /// euler even when that side fails the window.
        /// </summary>
        public static bool BothHandsMatchTriggerWindow(
            SuperController sc,
            out Vector3 leftEuler,
            out Vector3 rightEuler,
            out bool leftOk,
            out bool rightOk)
        {
            leftEuler = Vector3.zero;
            rightEuler = Vector3.zero;
            leftOk = false;
            rightOk = false;

            Transform hmd = ResolveHmdTransform(sc);
            if (sc == null || hmd == null)
                return false;

            Transform lh = sc.leftHand;
            Transform rh = sc.rightHand;

            bool haveL = lh != null && TryHmdRelativeEuler360(lh, hmd, out leftEuler);
            bool haveR = rh != null && TryHmdRelativeEuler360(rh, hmd, out rightEuler);
            leftOk = haveL && LeftMatches(leftEuler);
            rightOk = haveR && RightMatches(rightEuler);
            return leftOk && rightOk;
        }
    }
}
