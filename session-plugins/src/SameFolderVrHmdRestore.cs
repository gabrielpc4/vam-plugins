using System.Collections;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// OpenVR/Oculus same-folder scene loads: capture
    /// <see cref="SuperController.navigationRig"/> world pose,
    /// <see cref="SuperController.playerHeightAdjust"/>, and
    /// <see cref="SuperController.MonitorCenterCamera"/> local euler (scene JSON
    /// reapplies it on load) on the idle→loading edge before
    /// <see cref="PassengerRuntime.NotifySceneChanged"/> (passenger: VaMScripts
    /// reads the pose stored at passenger start). After
    /// <see cref="SuperController.isLoading"/> clears, apply as soon as VaM
    /// finishes the frame (<see cref="WaitForEndOfFrame"/> plus a short
    /// reassert streak) instead of sticking to the scene file&apos;s preset
    /// rig/monitor tilt, then <see cref="SuperController.SetSceneLoadPosition"/>.
    /// </summary>
    public sealed class SameFolderVrHmdRestore
    {
        /// <summary>
        /// Apply the snapshot this many Unity frames after
        /// <see cref="WaitForEndOfFrame"/> so transient load code cannot win.
        /// </summary>
        private const int PostLoadReassertFrameCount = 3;

        private bool armRestoreWhenNextIdle;

        private bool hasCapturedSnapshot;

        private Vector3 capturedRigWorldPosition;

        private Quaternion capturedRigWorldRotation;

        private float capturedPlayerHeightAdjust;

        /// <summary>Scene JSON restores monitor cam euler; VaMScripts overrides.</summary>
        private bool hasCapturedMonitorLocalEuler;

        private Vector3 capturedMonitorLocalEulerAngles;

        private Coroutine runningDeferredRestore;

        private readonly WaitForEndOfFrame waitEndOfFrame = new WaitForEndOfFrame();

        /// <summary>Cancels deferred restore; clears arm on cross-folder hops.</summary>
        public void NotifyLoadingStarted(MVRScript host, SuperController sc, bool sameFolderLoad)
        {
            if (host == null || sc == null)
                return;

            CancelDeferred(host);
            ClearSnapshot();

            if (!sameFolderLoad || !(sc.isOVR || sc.isOpenVR))
            {
                armRestoreWhenNextIdle = false;
                return;
            }

            if (!PassengerRuntime.TryGetRigPoseForSameFolderRestore(
                    sc,
                    out capturedRigWorldPosition,
                    out capturedRigWorldRotation,
                    out capturedPlayerHeightAdjust))
                return;

            if (sc.MonitorCenterCamera != null)
            {
                capturedMonitorLocalEulerAngles =
                    sc.MonitorCenterCamera.transform.localEulerAngles;
                hasCapturedMonitorLocalEuler = true;
            }

            hasCapturedSnapshot = true;
            armRestoreWhenNextIdle = true;
        }

        /// <summary>Queues restore tick after VaM settles the load frame.</summary>
        public Coroutine NotifyLoadingEnded(MVRScript host, SuperController sc)
        {
            if (host == null || sc == null)
                return null;

            if (!armRestoreWhenNextIdle || !hasCapturedSnapshot)
                return null;

            armRestoreWhenNextIdle = false;

            CancelDeferred(host);
            runningDeferredRestore =
                host.StartCoroutine(CoApplySnapshotAfterSceneLoadAllowsRigWrites());

            return runningDeferredRestore;
        }

        public void OnPluginDestroy(MVRScript host)
        {
            CancelDeferred(host);
            armRestoreWhenNextIdle = false;
            ClearSnapshot();
        }

        private void CancelDeferred(MVRScript host)
        {
            if (runningDeferredRestore != null && host != null)
                host.StopCoroutine(runningDeferredRestore);

            runningDeferredRestore = null;
        }

        private void ClearSnapshot()
        {
            hasCapturedSnapshot = false;
            hasCapturedMonitorLocalEuler = false;
        }

        private IEnumerator CoApplySnapshotAfterSceneLoadAllowsRigWrites()
        {
            bool monitorCaptured;
            float heightCaptured;
            Quaternion rotCaptured;
            SuperController scAlive;
            Vector3 monitorEulerCaptured;
            Vector3 posCaptured;
            int frameIndex;

            heightCaptured = capturedPlayerHeightAdjust;
            monitorCaptured = hasCapturedMonitorLocalEuler;
            monitorEulerCaptured = capturedMonitorLocalEulerAngles;
            posCaptured = capturedRigWorldPosition;
            rotCaptured = capturedRigWorldRotation;

            try
            {
                yield return waitEndOfFrame;

                for (frameIndex = 0;
                    frameIndex < PostLoadReassertFrameCount;
                    frameIndex++)
                {
                    scAlive = SuperController.singleton;
                    if (scAlive == null || scAlive.isLoading)
                        yield break;

                    if (!(scAlive.isOVR || scAlive.isOpenVR))
                        yield break;

                    if (PassengerRuntime.IsPassengerModeActiveOrPending())
                        yield break;

                    ApplyCapturedSnapshot(
                        scAlive,
                        posCaptured,
                        rotCaptured,
                        heightCaptured,
                        monitorCaptured,
                        monitorEulerCaptured);

                    if (frameIndex + 1 < PostLoadReassertFrameCount)
                        yield return null;
                }

                scAlive = SuperController.singleton;
                if (scAlive == null || scAlive.isLoading)
                    yield break;

                if (!(scAlive.isOVR || scAlive.isOpenVR))
                    yield break;

                if (PassengerRuntime.IsPassengerModeActiveOrPending())
                    yield break;

                scAlive.SetSceneLoadPosition();
            }
            finally
            {
                runningDeferredRestore = null;
            }
        }

        private static void ApplyCapturedSnapshot(
            SuperController sc,
            Vector3 rigPosition,
            Quaternion rigRotation,
            float playerHeightAdjustValue,
            bool applyMonitorLocalEuler,
            Vector3 monitorLocalEulerAnglesValue)
        {
            Transform rig;

            rig = sc.navigationRig;
            if (rig == null)
                return;

            rig.rotation = rigRotation;
            rig.position = rigPosition;
            sc.playerHeightAdjust = playerHeightAdjustValue;

            if (!applyMonitorLocalEuler || sc.MonitorCenterCamera == null)
                return;

            sc.MonitorCenterCamera.transform.localEulerAngles =
                monitorLocalEulerAnglesValue;
        }
    }
}
