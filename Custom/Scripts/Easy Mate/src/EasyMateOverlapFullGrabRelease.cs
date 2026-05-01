using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VaM overlap full-grab (grip while the controller overlap sphere hits a <see cref="FreeControllerV3"/>) can stick.
    /// Plugins cannot use <c>System.Reflection</c>, so this uses only public <see cref="SuperController"/> input and
    /// <see cref="FreeControllerV3.RestorePreLinkState"/>. Skips while the same hand’s <b>remote</b> hold-grab is active
    /// so intentional remote links are not torn down each frame.
    /// </summary>
    internal static class EasyMateOverlapFullGrabRelease
    {
        public static void LateTick(bool enabled)
        {
            if (!enabled)
                return;

            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;
            if (!sc.isOVR && !sc.isOpenVR)
                return;

            TryRestoreNonRemoteFullGrab(sc, isLeft: false);
            TryRestoreNonRemoteFullGrab(sc, isLeft: true);
        }

        private static void TryRestoreNonRemoteFullGrab(SuperController sc, bool isLeft)
        {
            FreeControllerV3 fc = isLeft ? sc.LeftFullGrabbedController : sc.RightFullGrabbedController;
            if (fc == null)
                return;

            if (isLeft)
            {
                if (sc.GetLeftRemoteHoldGrab())
                    return;
            }
            else
            {
                if (sc.GetRightRemoteHoldGrab())
                    return;
            }

            try
            {
                fc.RestorePreLinkState();
            }
            catch (Exception e)
            {
                SuperController.LogError("EasyMateOverlapFullGrabRelease: RestorePreLinkState failed: " + e.Message);
            }
        }
    }
}
