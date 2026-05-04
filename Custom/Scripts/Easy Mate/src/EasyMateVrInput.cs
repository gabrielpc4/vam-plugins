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
        /// Quest / Link RTouch face <b>A</b> (primary). Unity UI on the
        /// palm HUD often gets no pointer rays; this drives <b>Homem</b>.
        /// </summary>
        public static bool PollRightTouchPrimaryFaceButtonDown(SuperController sc)
        {
            if (sc == null || !ShouldPollOculusStyleRightFaceButtons(sc))
                return false;
            try
            {
                return OVRInput.GetDown(
                    OVRInput.Button.One,
                    OVRInput.Controller.RTouch);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Quest / Link RTouch face <b>B</b> (secondary). Drives
        /// <b>Mulher</b> on the palm gender row (VaM menu uses system
        /// handling; avoid relying on this when the main menu is open).
        /// </summary>
        public static bool PollRightTouchSecondaryFaceButtonDown(SuperController sc)
        {
            if (sc == null || !ShouldPollOculusStyleRightFaceButtons(sc))
                return false;
            try
            {
                return OVRInput.GetDown(
                    OVRInput.Button.Two,
                    OVRInput.Controller.RTouch);
            }
            catch
            {
                return false;
            }
        }

        private static bool ShouldPollOculusStyleRightFaceButtons(SuperController sc)
        {
            if (sc.isOVR)
                return true;
            if (!XrHeadsetLikelyOn(sc))
                return false;
            try
            {
                return OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);
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
