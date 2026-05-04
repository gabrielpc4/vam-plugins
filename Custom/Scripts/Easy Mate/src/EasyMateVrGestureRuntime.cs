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
        /// Possess + Align + Select closest female by head when both palms face
        /// the HMD (~3s dwell; not keyboard <b>P</b>, which uses any gender).
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
            PalmGazePossessClosestFemale.ProcessUpdate(
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
        /// Both hands toward HMD (~3s dwell); after a fire, leave the pose once
        /// before another dwell (avoids repeat while holding through cooldown).
        /// </summary>
        private static class PalmGazePossessClosestFemale
        {
            private const string LogPrefix = "Easy Mate VR palm:";

            private const float DwellSeconds = 3f;
            private const float CooldownSeconds = 4f;
            private const float MinHandCamDistM = 0.12f;
            private const float MaxHandCamDistM = 1.05f;
            /// <summary>
            /// Palm plane ~toward face (max dot of ±local axes vs hand→HMD).
            /// Calibrated OpenVR <c>leftHand</c>: palms ~0.93–0.95, backs
            /// ~0.79/0.89, sideways ~0.65–0.76; both hands must exceed this.
            /// </summary>
            private const float MinPalmFacingDotLoose = 0.90f;
            /// <summary>
            /// Hand must sit in front of HMD (not behind); dot(camFwd, toHand).
            /// </summary>
            private const float MinCamForwardDotToHand = -0.08f;

            private static float _dwellAccumUnscaled;
            private static float _lastTriggerUnscaledTime = -1000f;
            /// <summary>
            /// After a fire, false until both-hands pose has been left once
            /// (stops re-trigger while still holding palms through cooldown).
            /// </summary>
            private static bool _dwellArmed = true;
            private static bool _prevBothPalmsZone;
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

                Transform camTf = sc.lookCamera != null
                    ? sc.lookCamera.transform
                    : null;
                if (camTf == null && sc.centerCameraTarget != null)
                    camTf = sc.centerCameraTarget.transform;
                if (camTf == null)
                    return;

                Transform lh = sc.leftHand;
                Transform rh = sc.rightHand;

                float leftDot = -1f;
                float rightDot = -1f;
                bool leftOk = lh != null &&
                    TryGetPalmFacingDotLoose(lh, camTf, out leftDot);
                bool rightOk = rh != null &&
                    TryGetPalmFacingDotLoose(rh, camTf, out rightDot);
                bool palmGaze = leftOk && rightOk;

                if (palmGaze != _prevBothPalmsZone)
                {
                    if (palmGaze)
                    {
                        SuperController.LogMessage(
                            LogPrefix + " both-hands pose ON (dwell only if armed).");
                    }
                    else
                    {
                        SuperController.LogMessage(
                            LogPrefix + " both-hands pose OFF; dwell cleared, re-armed.");
                    }

                    _prevBothPalmsZone = palmGaze;
                }

                if (!palmGaze)
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
                    LogPrefix + " TRIGGER possess closest female (L palm axis dot " +
                    leftDot.ToString("F2") + ", R " + rightDot.ToString("F2") +
                    "). Exit pose once before another dwell.");

                boundAction();
                _lastTriggerUnscaledTime = now;
                _dwellAccumUnscaled = 0f;
                _dwellArmed = false;
            }

            private static float BestLocalAxisDotToward(
                Transform hand,
                Vector3 unitTowardCam)
            {
                if (hand == null || unitTowardCam.sqrMagnitude < 1e-6f)
                    return -1f;

                float best = -1f;
                float d;

                d = Vector3.Dot(hand.forward, unitTowardCam);
                if (d > best)
                    best = d;
                d = Vector3.Dot(-hand.forward, unitTowardCam);
                if (d > best)
                    best = d;
                d = Vector3.Dot(hand.up, unitTowardCam);
                if (d > best)
                    best = d;
                d = Vector3.Dot(-hand.up, unitTowardCam);
                if (d > best)
                    best = d;
                d = Vector3.Dot(hand.right, unitTowardCam);
                if (d > best)
                    best = d;
                d = Vector3.Dot(-hand.right, unitTowardCam);
                if (d > best)
                    best = d;

                return best;
            }

            /// <summary>
            /// True when distance + forward hemisphere + palm dot pass; outputs
            /// best axis dot toward cam for logging.
            /// </summary>
            private static bool TryGetPalmFacingDotLoose(
                Transform hand,
                Transform camTf,
                out float bestAxisDotTowardCam)
            {
                bestAxisDotTowardCam = -1f;
                if (hand == null || camTf == null)
                    return false;

                Vector3 camPos = camTf.position;
                Vector3 camFwd = camTf.forward;
                if (camFwd.sqrMagnitude < 1e-10f)
                    return false;
                camFwd.Normalize();

                Vector3 toHand = hand.position - camPos;
                float dist = toHand.magnitude;
                if (dist < MinHandCamDistM || dist > MaxHandCamDistM)
                    return false;

                Vector3 dirToHand = toHand * (1f / dist);
                if (Vector3.Dot(camFwd, dirToHand) < MinCamForwardDotToHand)
                    return false;

                Vector3 towardCam = camPos - hand.position;
                float tcMag = towardCam.magnitude;
                if (tcMag < 1e-5f)
                    return false;
                towardCam = towardCam * (1f / tcMag);

                float best = BestLocalAxisDotToward(hand, towardCam);
                bestAxisDotTowardCam = best;
                return best >= MinPalmFacingDotLoose;
            }
        }
    }
}
