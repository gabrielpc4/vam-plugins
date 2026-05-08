using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VR input: <see cref="OVRInput"/> when Oculus path is active or when
    /// <see cref="SuperController"/> reports OVR/OpenVR (VaM exposes those flags
    /// for typical VR setups). SteamVR/OpenVR uses hold-grab / select APIs.
    /// Does not reference <c>UnityEngine.XR</c> — VaM Mono dynamic-compile can
    /// fault on XR typerefs (<c>TypeBuilder.CreateType</c> / attributes).
    /// </summary>
    internal static class VrInput
    {
        /// <summary>
        /// True only when VaM marks OVR or OpenVR. No Unity XR fallback.
        /// </summary>
        internal static bool IsLikelyVrRuntimeSafe(SuperController sc)
        {
            try
            {
                return sc != null && (sc.isOVR || sc.isOpenVR);
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
                    return OVRInput.GetDown(
                        OVRInput.Button.SecondaryThumbstick,
                        OVRInput.Controller.RTouch);
                }
                catch
                {
                }
            }

            if (sc != null && sc.isOpenVR)
                return false;

            return false;
        }

        /// <summary>
        /// Right face <b>A</b> / right Select confirmation input.
        /// OVR <c>RTouch</c> <see cref="OVRInput.Button.One"/>; OpenVR
        /// <see cref="SuperController.GetRightSelect"/>.
        /// </summary>
        public static bool PollRightFaceADown(SuperController sc)
        {
            if (sc == null)
            {
                return false;
            }

            if (sc.isOVR)
            {
                return TryOvrRightTouchButtonDown(OVRInput.Button.One);
            }

            if (sc.isOpenVR)
            {
                return sc.GetRightSelect();
            }

            return false;
        }

        /// <summary>
        /// Palm HUD <b>Despossuir</b> row: same as
        /// <see cref="PollRightFaceADown"/>.
        /// </summary>
        public static bool PollPalmHudPossessRowFaceADown(SuperController sc)
        {
            return PollRightFaceADown(sc);
        }

        /// <summary>
        /// Palm HUD face <b>B</b> / SteamVR menu: OVR <c>RTouch</c>
        /// <see cref="OVRInput.Button.Two"/>; OpenVR
        /// <see cref="SuperController.GetMenuShow"/>.
        /// Shared by <see cref="PollPalmHudProximaCenaFaceBDown"/>.
        /// </summary>
        private static bool PollPalmHudFaceBOrVrMenuDown(SuperController sc)
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

            return false;
        }

        /// <summary>
        /// Palm HUD <b>Próxima cena</b> (upper): face <b>B</b> / Menu (same as
        /// <see cref="PollPalmHudFaceBOrVrMenuDown"/>).
        /// </summary>
        public static bool PollPalmHudProximaCenaFaceBDown(SuperController sc)
        {
            return PollPalmHudFaceBOrVrMenuDown(sc);
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

            return false;
        }

        /// <summary>
        /// Female passenger VR hands start: any controller <b>trigger or grip</b>
        /// press this frame. Uses <see cref="SuperController.GetLeftGrab"/> /
        /// <see cref="SuperController.GetLeftHoldGrab"/> (and right), which
        /// match VaM&apos;s Oculus trigger vs grip mapping including
        /// <c>oculusSwapGrabAndTrigger</c>.
        /// </summary>
        public static bool PollVrAnyTriggerOrGripPressDown(SuperController sc)
        {
            if (sc == null)
            {
                return false;
            }

            if (!sc.isOVR && !sc.isOpenVR)
            {
                return false;
            }

            try
            {
                return sc.GetLeftGrab() ||
                    sc.GetRightGrab() ||
                    sc.GetLeftHoldGrab() ||
                    sc.GetRightHoldGrab();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// OVR path matches <see cref="SuperController.GetLeftHoldGrab"/>
        /// (<c>Controller.Touch</c>).
        /// </summary>
        private static bool OvrLeftGripPhysicalDown()
        {
            bool swap = UserPreferences.singleton != null &&
                UserPreferences.singleton.oculusSwapGrabAndTrigger;
            if (swap)
                return OVRInput.GetDown(
                    OVRInput.Button.PrimaryIndexTrigger,
                    OVRInput.Controller.Touch);
            return OVRInput.GetDown(
                OVRInput.Button.PrimaryHandTrigger,
                OVRInput.Controller.Touch);
        }

        /// <summary>
        /// OVR path matches <see cref="SuperController.GetRightHoldGrab"/>
        /// (<c>Controller.Touch</c>).
        /// </summary>
        private static bool OvrRightGripPhysicalDown()
        {
            bool swap = UserPreferences.singleton != null &&
                UserPreferences.singleton.oculusSwapGrabAndTrigger;
            if (swap)
                return OVRInput.GetDown(
                    OVRInput.Button.SecondaryIndexTrigger,
                    OVRInput.Controller.Touch);
            return OVRInput.GetDown(
                OVRInput.Button.SecondaryHandTrigger,
                OVRInput.Controller.Touch);
        }
    }
}
