using UnityEngine;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// VR input: <see cref="OVRInput"/> when Oculus path is active or when XR is enabled (no Unity <c>InputDevice</c> API —
    /// not available to VaM plugin compile in some builds), plus <see cref="SuperController"/> hold-grab / select for SteamVR/OpenVR.
    /// </summary>
    internal static class EasyMateVrInput
    {
        private static bool XrHeadsetLikelyOn(SuperController sc)
        {
            if (sc != null && (sc.isOVR || sc.isOpenVR))
                return true;
            try
            {
                return XRSettings.enabled;
            }
            catch
            {
                return false;
            }
        }

        public static void ResetEdgeState()
        {
        }

        public static bool PollRightThumbstickClickDown(SuperController sc)
        {
            if (sc != null && sc.isOVR)
            {
                try
                {
                    return OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.RTouch);
                }
                catch
                {
                }
            }

            if (sc != null && sc.isOpenVR)
                return false;

            if (XrHeadsetLikelyOn(sc))
            {
                try
                {
                    return OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.RTouch);
                }
                catch
                {
                }
            }

            return false;
        }

        /// <summary>
        /// Palm HUD: <b>Próxima cena</b> — <b>only</b> physical shortcut for
        /// next scene (no thumbstick). OVR <c>RTouch</c> face <b>B</b>;
        /// OpenVR <see cref="SuperController.GetMenuShow"/> (same binding as
        /// face B on many rigs). Call only while the two-row panel is visible
        /// and not on the Mulher-only step so it does not clash with
        /// <see cref="PollPalmHudMulherChoiceDown"/>.
        /// </summary>
        public static bool PollPalmHudProximaCenaFaceBDown(SuperController sc)
        {
            if (sc == null)
            {
                return false;
            }

            if (sc.isOVR)
            {
                return TryOvrRightTouchButtonDown(OVRInput.Button.Two);
            }

            if (sc.isOpenVR)
            {
                return sc.GetMenuShow();
            }

            if (XrHeadsetLikelyOn(sc))
            {
                return TryOvrRightTouchButtonDown(OVRInput.Button.Two);
            }

            return false;
        }

        /// <summary>
        /// Palm HUD <b>Mulher (B)</b>: OVR <c>RTouch</c> face B only
        /// (avoids left-hand Y); OpenVR uses
        /// <see cref="SuperController.GetMenuShow"/> (SteamVR Menu binding,
        /// often right B — may fire for either hand depending on bindings).
        /// </summary>
        public static bool PollPalmHudMulherChoiceDown(SuperController sc)
        {
            if (sc == null)
            {
                return false;
            }

            if (sc.isOVR)
            {
                return TryOvrRightTouchButtonDown(OVRInput.Button.Two);
            }

            if (sc.isOpenVR)
            {
                return sc.GetMenuShow();
            }

            if (XrHeadsetLikelyOn(sc))
            {
                return TryOvrRightTouchButtonDown(OVRInput.Button.Two);
            }

            return false;
        }

        /// <summary>
        /// Palm HUD <b>Homem (A)</b>: OVR <c>RTouch</c> face A only; OpenVR
        /// uses <see cref="SuperController.GetRightSelect"/> (SteamVR
        /// Select on the right hand).
        /// </summary>
        public static bool PollPalmHudHomemChoiceDown(SuperController sc)
        {
            if (sc == null)
                return false;
            if (sc.isOVR)
                return TryOvrRightTouchButtonDown(OVRInput.Button.One);
            if (sc.isOpenVR)
                return sc.GetRightSelect();
            if (XrHeadsetLikelyOn(sc))
                return TryOvrRightTouchButtonDown(OVRInput.Button.One);
            return false;
        }

        private static bool TryOvrRightTouchButtonDown(OVRInput.Button button)
        {
            try
            {
                return OVRInput.GetDown(button, OVRInput.Controller.RTouch);
            }
            catch
            {
                return false;
            }
        }

        public static bool PollLeftGripClickDown(SuperController sc)
        {
            if (sc != null && sc.isOVR)
            {
                try
                {
                    if (OvrLeftGripPhysicalDown())
                        return true;
                }
                catch
                {
                }
            }

            if (sc != null && sc.isOpenVR && sc.GetLeftHoldGrab())
                return true;

            if (XrHeadsetLikelyOn(sc))
            {
                try
                {
                    if (OvrLeftGripPhysicalDown())
                        return true;
                }
                catch
                {
                }
            }

            return false;
        }

        public static bool PollRightGripClickDown(SuperController sc)
        {
            if (sc != null && sc.isOVR)
            {
                try
                {
                    if (OvrRightGripPhysicalDown())
                        return true;
                }
                catch
                {
                }
            }

            if (sc != null && sc.isOpenVR && sc.GetRightHoldGrab())
                return true;

            if (XrHeadsetLikelyOn(sc))
            {
                try
                {
                    if (OvrRightGripPhysicalDown())
                        return true;
                }
                catch
                {
                }
            }

            return false;
        }

        /// <summary>OVR path matches <see cref="SuperController.GetLeftHoldGrab"/> (<c>Controller.Touch</c>).</summary>
        private static bool OvrLeftGripPhysicalDown()
        {
            bool swap = UserPreferences.singleton != null && UserPreferences.singleton.oculusSwapGrabAndTrigger;
            if (swap)
                return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
            return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
        }

        /// <summary>OVR path matches <see cref="SuperController.GetRightHoldGrab"/> (<c>Controller.Touch</c>).</summary>
        private static bool OvrRightGripPhysicalDown()
        {
            bool swap = UserPreferences.singleton != null && UserPreferences.singleton.oculusSwapGrabAndTrigger;
            if (swap)
                return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
            return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
        }
    }
}
