using System;
using UnityEngine;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// Maps VR gesture outcomes to Easy Mate behavior. When you add a gesture,
    /// add a public <c>Action</c> field here, assign it in
    /// <see cref="MainUIButtons.Init"/>, and call it from a private module in
    /// <see cref="EasyMateVrGestureRuntime"/>.
    /// </summary>
    public sealed class EasyMateVrGestureBindings
    {
        /// <summary>Same as keyboard <b>I</b> (hands + Person head snap cycle).</summary>
        public Action TriggerISnapSameAsKeyI;

        /// <summary>
        /// Possess + Align + Select closest female when both
        /// <c>leftHand</c>/<c>rightHand</c> match HMD-relative euler windows
        /// (~3s dwell; not keyboard <b>P</b>).
        /// </summary>
        public Action TriggerPossessAlignSelectClosestFemaleByHead;
   }

    /// <summary>
    /// Called from <see cref="MainUIButtons.ProcessHotkeysUpdate"/> after
    /// keyboard handling. Runs gesture modules in order; a module does nothing
    /// if its binding is null. Built via <c>EasyMate.cslist</c>.
    /// </summary>
    public static class EasyMateVrGestureRuntime
    {
        public static void ProcessUpdate(EasyMateVrGestureBindings bindings)
        {
            if (bindings == null)
                return;
            OverHeadRightHandGesture.ProcessUpdate(
                bindings.TriggerISnapSameAsKeyI);
            DualHandHmdRelativeEulerPossessClosestFemale.ProcessUpdate(
                bindings.TriggerPossessAlignSelectClosestFemaleByHead);
        }

        /// <summary>
        /// Right hand above the HMD along headset <c>up</c>, within a small
        /// lateral cap (not far forward/side), once per visit, with cooldown.
        /// Pose checks at most once per second; otherwise only mode + interval.
        /// </summary>
        private static class OverHeadRightHandGesture
        {
            /// <summary>
            /// Max offset perpendicular to headset <c>up</c> through the HMD
            /// (m); keeps “pat top of head” vs arm held high elsewhere.
            /// </summary>
            private const float MaxLateralOffsetM = 0.14f;
            private const float MinHeightAlongHmdUpM = 0.06f;
            private const float MaxHeightAlongHmdUpM = 0.34f;
            private const float CooldownSeconds = 4f;
            /// <summary>
            /// Min seconds between HMD/hand reads and zone test (~1 Hz).
            /// </summary>
            private const float EvalIntervalUnscaledSeconds = 1f;

            private static bool _insideLatch;
            private static float _lastTriggerUnscaledTime = -1000f;
            private static float _lastEvalUnscaledTime = -1000f;

            public static void ProcessUpdate(Action boundAction)
            {
                if (boundAction == null)
                    return;

                SuperController sc = SuperController.singleton;
                if (sc == null || sc.isLoading)
                    return;
                if (!(sc.isOVR || sc.isOpenVR || XRSettings.enabled))
                    return;

                float now = Time.unscaledTime;
                if (now - _lastEvalUnscaledTime < EvalIntervalUnscaledSeconds)
                    return;
                _lastEvalUnscaledTime = now;

                Transform hmdTf = sc.centerCameraTarget != null ?
                    sc.centerCameraTarget.transform :
                    null;
                if (hmdTf == null && sc.lookCamera != null)
                    hmdTf = sc.lookCamera.transform;
                if (hmdTf == null)
                    return;

                Transform rh = sc.rightHand;
                if (rh == null)
                    return;

                Vector3 headUp = hmdTf.up;
                if (headUp.sqrMagnitude < 1e-10f)
                    headUp = Vector3.up;
                else
                    headUp.Normalize();

                Vector3 deltaW = rh.position - hmdTf.position;
                float hAlongUp = Vector3.Dot(deltaW, headUp);
                float lateralSq = deltaW.sqrMagnitude - hAlongUp * hAlongUp;
                if (lateralSq < 0f)
                    lateralSq = 0f;
                bool inZone = hAlongUp >= MinHeightAlongHmdUpM &&
                    hAlongUp <= MaxHeightAlongHmdUpM &&
                    lateralSq <= MaxLateralOffsetM * MaxLateralOffsetM;

                if (inZone)
                {
                    if (!_insideLatch)
                    {
                        if (now - _lastTriggerUnscaledTime >= CooldownSeconds)
                        {
                            boundAction();
                            _lastTriggerUnscaledTime = now;
                        }
                        _insideLatch = true;
                    }
                }
                else
                {
                    _insideLatch = false;
                }
            }
        }

        /// <summary>
        /// Both hands: HMD-relative euler on <c>leftHand</c>/<c>rightHand</c>
        /// (~3s dwell); release pose once after trigger before the next dwell.
        /// </summary>
        private static class DualHandHmdRelativeEulerPossessClosestFemale
        {
            private const string LogPrefix = "Easy Mate VR hand euler:";

            private const float DwellSeconds = 3f;
            private const float CooldownSeconds = 10f;

            /// <summary>Left: euler X &gt; this (degrees, 0–360).</summary>
            private const float LeftMinEulerX = 300f;

            /// <summary>Left: Z strictly between these (degrees).</summary>
            private const float LeftMinEulerZ = 30f;
            private const float LeftMaxEulerZ = 90f;

            /// <summary>Right: euler X &gt; this (degrees, 0–360).</summary>
            private const float RightMinEulerX = 300f;

            /// <summary>Right: Z strictly between these (degrees).</summary>
            private const float RightMinEulerZ = 290f;
            private const float RightMaxEulerZ = 330f;

            private static float _dwellAccumUnscaled;
            private static float _lastTriggerUnscaledTime = -1000f;
            /// <summary>
            /// After a fire, false until both-hands pose has been left once
            /// (stops re-trigger while holding through cooldown).
            /// </summary>
            private static bool _dwellArmed = true;
            private static bool _prevBothHandsPose;
            /// <summary>
            /// One log per cooldown-wait episode while holding pose, not each
            /// frame.
            /// </summary>
            private static bool _loggedCooldownSkipThisCycle;

            public static void ProcessUpdate(Action boundAction)
            {
                if (boundAction == null)
                    return;

                SuperController sc = SuperController.singleton;
                if (sc == null || sc.isLoading)
                    return;
                if (!(sc.isOVR || sc.isOpenVR || XRSettings.enabled))
                    return;

                Transform camTf = sc.lookCamera != null ?
                    sc.lookCamera.transform :
                    null;
                if (camTf == null && sc.centerCameraTarget != null)
                    camTf = sc.centerCameraTarget.transform;
                if (camTf == null)
                    return;

                Transform lh = sc.leftHand;
                Transform rh = sc.rightHand;

                Vector3 leftEuler;
                Vector3 rightEuler;
                bool leftOk = lh != null &&
                    TryHmdRelativeEuler360(lh, camTf, out leftEuler) &&
                    LeftMatches(leftEuler);
                bool rightOk = rh != null &&
                    TryHmdRelativeEuler360(rh, camTf, out rightEuler) &&
                    RightMatches(rightEuler);
                bool poseOk = leftOk && rightOk;

                if (poseOk != _prevBothHandsPose)
                {
                    if (poseOk)
                    {
                        SuperController.LogMessage(
                            LogPrefix + " both-hands pose ON (dwell if armed).");
                    }
                    else
                    {
                        SuperController.LogMessage(
                            LogPrefix + " both-hands pose OFF; dwell cleared, re-armed.");
                    }

                    _prevBothHandsPose = poseOk;
                }

                if (!poseOk)
                {
                    _dwellAccumUnscaled = 0f;
                    _dwellArmed = true;
                    _loggedCooldownSkipThisCycle = false;
                    return;
                }

                if (!_dwellArmed)
                    return;

                float dt = Time.unscaledDeltaTime;
                if (dt < 0f || dt > 0.5f)
                    dt = 0.016f;

                _dwellAccumUnscaled += dt;
                float now = Time.unscaledTime;
                if (_dwellAccumUnscaled < DwellSeconds)
                    return;

                if (now - _lastTriggerUnscaledTime < CooldownSeconds)
                {
                    if (!_loggedCooldownSkipThisCycle)
                    {
                        SuperController.LogMessage(
                            LogPrefix + " dwell done but cooldown active (" +
                            CooldownSeconds.ToString("F0") + "s).");
                        _loggedCooldownSkipThisCycle = true;
                    }

                    return;
                }

                _loggedCooldownSkipThisCycle = false;
                SuperController.LogMessage(
                    LogPrefix + " TRIGGER possess closest female; eulerRelHMD L " +
                    FormatEuler(leftEuler) + " R " + FormatEuler(rightEuler) +
                    ". Exit pose once before another dwell.");

                boundAction();
                _lastTriggerUnscaledTime = now;
                _dwellAccumUnscaled = 0f;
                _dwellArmed = false;
            }

            private static string FormatEuler(Vector3 e)
            {
                return "(" +
                    e.x.ToString("F1") + "," +
                    e.y.ToString("F1") + "," +
                    e.z.ToString("F1") + ")";
            }

            /// <summary>
            /// HMD-relative rotation of <paramref name="hand"/>; euler per axis
            /// in [0, 360).
            /// </summary>
            private static bool TryHmdRelativeEuler360(
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

            private static float NormalizeEuler360(float degrees)
            {
                float d = degrees % 360f;
                if (d < 0f)
                    d += 360f;
                return d;
            }

            private static bool LeftMatches(Vector3 e)
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

            private static bool RightMatches(Vector3 e)
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
        }
    }
}
