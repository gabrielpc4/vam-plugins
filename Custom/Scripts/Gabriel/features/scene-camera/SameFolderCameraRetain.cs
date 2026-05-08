using System.Collections;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// If you open another scene JSON from the same VaM load-directory as the
    /// current scene (e.g. multiple files under one story folder), restore your
    /// navigation rig pose, monitor camera aim, and player height to what they
    /// were immediately before loading—after VaM-applied JSON baselines settle.
    /// </summary>
    public static class SameFolderCameraRetain
    {
        private static bool retainEnabledStatic = true;

        private static string lastIdleLoadDirNorm = "";

        private static Vector3 capturedNavWorldPos;

        private static Quaternion capturedNavWorldRot;

        private static Vector3 capturedMonitorLocalEuler;

        private static float capturedPlayerHeightAdjust;

        private static bool hasCapturedPose;

        private static bool pendingRestoreAfterCurrentLoad;

        public static void SetRetainEnabled(bool value)
        {
            retainEnabledStatic = value;
            if (!value)
                pendingRestoreAfterCurrentLoad = false;
        }

        /// <summary>
        /// Call from <see cref="MVRScript.LateUpdate"/> only while not loading so
        /// the sampled folder matches VaM&apos;s idle <see cref="SuperController.currentLoadDir"/>
        /// before <c>PushLoadDirFromFilePath</c> rewrote it at load start.
        /// </summary>
        public static void LateTickIdleCapture(SuperController sc)
        {
            if (!retainEnabledStatic || sc == null || sc.isLoading)
                return;
            if (sc.navigationRig == null)
                return;

            if (string.IsNullOrEmpty(sc.currentLoadDir))
                return;

            lastIdleLoadDirNorm = SameFolderLoadCheck.Normalize(
                sc.currentLoadDir);

            capturedNavWorldPos = sc.navigationRig.position;
            capturedNavWorldRot = sc.navigationRig.rotation;

            if (sc.MonitorCenterCamera != null)
                capturedMonitorLocalEuler =
                    sc.MonitorCenterCamera.transform.localEulerAngles;
            capturedPlayerHeightAdjust = sc.playerHeightAdjust;
            hasCapturedPose = true;
        }

        /// <summary>
        /// VaM pushes the new folder before flipping <see cref="SuperController.isLoading"/>
        /// (<c>PushLoadDirFromFilePath</c>). Compare idle cache to the new folder.
        /// </summary>
        public static void NotifyLoadBeginning(SuperController sc)
        {
            pendingRestoreAfterCurrentLoad = false;
            if (!retainEnabledStatic || sc == null)
                return;
            if (!hasCapturedPose || string.IsNullOrEmpty(lastIdleLoadDirNorm))
                return;
            string newNorm = SameFolderLoadCheck.Normalize(sc.currentLoadDir);
            if (newNorm.Length == 0 || newNorm != lastIdleLoadDirNorm)
                return;
            pendingRestoreAfterCurrentLoad = true;
        }

        public static IEnumerator CoRestoreAfterSceneLoadEnds(MVRScript host)
        {
            if (!retainEnabledStatic || host == null)
                yield break;
            if (!pendingRestoreAfterCurrentLoad || !hasCapturedPose)
                yield break;

            pendingRestoreAfterCurrentLoad = false;

            yield return null;
            yield return null;

            ApplyCapturedPose(SuperController.singleton);
        }

        public static void QueueRestoreCoroutineIfNeeded(MVRScript host)
        {
            if (!retainEnabledStatic || host == null)
                return;
            if (!pendingRestoreAfterCurrentLoad || !hasCapturedPose)
                return;

            host.StartCoroutine(CoRestoreAfterSceneLoadEnds(host));
        }

        private static void ApplyCapturedPose(SuperController sc)
        {
            if (!retainEnabledStatic || sc == null)
                return;
            if (!hasCapturedPose)
                return;

            Transform nr = sc.navigationRig;
            if (nr != null)
            {
                nr.position = capturedNavWorldPos;
                nr.rotation = capturedNavWorldRot;
            }

            if (sc.MonitorCenterCamera != null)
                sc.MonitorCenterCamera.transform.localEulerAngles =
                    capturedMonitorLocalEuler;

            sc.playerHeightAdjust = capturedPlayerHeightAdjust;
            try
            {
                sc.SetSceneLoadPosition();
            }
            catch
            {
                // Non-fatal: retain rig pose even if snapshot API fails later.
            }
        }
    }
}
