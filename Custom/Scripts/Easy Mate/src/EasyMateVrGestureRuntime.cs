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
            OverHeadRightHandCylinderGesture.ProcessUpdate(
                bindings.TriggerISnapSameAsKeyI);
        }

        /// <summary>
        /// Right hand in a vertical cylinder above the HMD (headset
        /// <c>up</c>), once per visit, with cooldown.
        /// </summary>
        private static class OverHeadRightHandCylinderGesture
        {
            private const float CylinderRadiusM = 0.14f;
            private const float MinHeightAlongHmdUpM = 0.06f;
            private const float MaxHeightAlongHmdUpM = 0.34f;
            private const float CooldownSeconds = 4f;

            private static bool _insideLatch;
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
                Vector3 radial = deltaW - headUp * hAlongUp;
                float rMax = CylinderRadiusM;
                bool inZone = hAlongUp >= MinHeightAlongHmdUpM &&
                    hAlongUp <= MaxHeightAlongHmdUpM &&
                    radial.sqrMagnitude <= rMax * rMax;

                if (inZone)
                {
                    if (!_insideLatch)
                    {
                        float now = Time.unscaledTime;
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
    }
}
