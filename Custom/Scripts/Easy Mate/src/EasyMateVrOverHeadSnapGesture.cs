using System;
using UnityEngine;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// VR-only: right controller in a cylinder segment above the HMD invokes
    /// the same handler as the <b>I</b> snap hotkey. Built into
    /// <c>EasyMate.cslist</c> — no separate plugin load. Called from
    /// <see cref="MainUIButtons.ProcessHotkeysUpdate"/>.
    /// </summary>
    public static class EasyMateVrOverHeadSnapGesture
    {
        private const float CylinderRadiusM = 0.14f;
        private const float MinHeightAlongHmdUpM = 0.06f;
        private const float MaxHeightAlongHmdUpM = 0.34f;
        private const float CooldownSeconds = 4f;

        private static bool _insideLatch;
        private static float _lastTriggerUnscaledTime = -1000f;

        /// <summary>
        /// After keyboard hotkeys, calls <paramref name="triggerSameAsKeyI"/>
        /// when the gesture arms (once per zone visit, with cooldown).
        /// </summary>
        public static void ProcessUpdate(Action triggerSameAsKeyI)
        {
            if (triggerSameAsKeyI == null)
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
                        triggerSameAsKeyI();
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
