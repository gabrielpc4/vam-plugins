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
        /// Both hands: each palm faces the HMD loosely (controller axis ~toward
        /// the look camera), in front of the user, for three seconds
        /// (Oculus / OpenVR).
        /// </summary>
        private static class PalmGazePossessClosestFemale
        {
            private const float DwellSeconds = 3f;
            private const float CooldownSeconds = 4f;
            private const float MinHandCamDistM = 0.12f;
            private const float MaxHandCamDistM = 1.05f;
            /// <summary>
            /// Palm plane ~toward face; lower = looser (both palms must pass).
            /// </summary>
            private const float MinPalmFacingDotLoose = 0.52f;
            /// <summary>
            /// Hand must sit in front of HMD (not behind); dot(camFwd, toHand).
            /// </summary>
            private const float MinCamForwardDotToHand = -0.08f;

            private static float _dwellAccumUnscaled;
            private static float _lastTriggerUnscaledTime = -1000f;

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
                bool palmGaze = lh != null && rh != null &&
                    HandPalmFacesHmdLoosely(lh, camTf) &&
                    HandPalmFacesHmdLoosely(rh, camTf);

                float dt = Time.unscaledDeltaTime;
                if (dt < 0f || dt > 0.5f)
                    dt = 0.016f;

                if (palmGaze)
                {
                    _dwellAccumUnscaled += dt;
                    float now = Time.unscaledTime;
                    if (_dwellAccumUnscaled >= DwellSeconds &&
                        now - _lastTriggerUnscaledTime >= CooldownSeconds)
                    {
                        boundAction();
                        _lastTriggerUnscaledTime = now;
                        _dwellAccumUnscaled = 0f;
                    }
                }
                else
                    _dwellAccumUnscaled = 0f;
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
            /// Palm-oriented toward HMD, in front hemisphere, within distance
            /// (no strict “look at palm” cone).
            /// </summary>
            private static bool HandPalmFacesHmdLoosely(Transform hand, Transform camTf)
            {
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

                return BestLocalAxisDotToward(hand, towardCam) >=
                    MinPalmFacingDotLoose;
            }
        }
    }
}
