using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Forces CoreControl GlobalLighting &quot;camExposure&quot; to 0 while VaM is still settling after a load:
    /// SuperController scene load, full-screen loading UI/geometry (which stays up briefly after isLoading clears),
    /// and the loading icon used for queued textures (ImageLoader) and URL audio loads.
    /// Restores the snapshot value once all of those are inactive.
    /// </summary>
    public class OnSceneStartup
    {
        private bool wasSceneStillSettling = false;

        private bool camExposureBackupCaptured = false;
        private float savedCamExposure = 0f;

        private const string coreControlAtomUid = "CoreControl";
        private const string globalLightingStorableId = "GlobalLighting";
        private const string camExposureParamName = "camExposure";

        public void TickDuringSuperControllerLoad()
        {
            bool settlingNow = ShouldTreatSceneAsStillSettling();

            if (settlingNow != wasSceneStillSettling)
            {
                DebugLog(string.Format("Scene settle indicator {0} -> {1}", wasSceneStillSettling, settlingNow));
            }

            if (settlingNow)
            {
                if (!wasSceneStillSettling)
                {
                    camExposureBackupCaptured = false;
                    DebugLog("Settle phase started (isLoading and/or loading UI/icon); snapshot camExposure on first GlobalLighting access.");
                }

                ApplyCamExposureWhileSceneSettling();
            }
            else
            {
                if (wasSceneStillSettling && camExposureBackupCaptured)
                {
                    DebugLog(string.Format("Settle phase ended; restoring camExposure to {0}.", savedCamExposure));
                    try
                    {
                        RestoreCamExposure();
                    }
                    catch (Exception restoreException)
                    {
                        SuperController.LogError("[OnSceneStartup] Restore camExposure after settle phase failed: " + restoreException);
                    }
                }
            }

            wasSceneStillSettling = settlingNow;
        }

        public void OnOwningPluginDestroy()
        {
            if (!camExposureBackupCaptured)
            {
                return;
            }

            try
            {
                DebugLog("OnDestroy: restoring camExposure.");
                RestoreCamExposure();
            }
            catch (Exception destroyRestoreException)
            {
                SuperController.LogError("[OnSceneStartup] Restore camExposure in OnDestroy failed: " + destroyRestoreException);
            }
        }

        bool ShouldTreatSceneAsStillSettling()
        {
            SuperController superController = SuperController.singleton;

            if (superController.isLoading)
            {
                return true;
            }

            if (IsTransformActive(superController.loadingUI))
            {
                return true;
            }

            if (IsTransformActive(superController.loadingUIAlt))
            {
                return true;
            }

            if (IsTransformActive(superController.loadingGeometry))
            {
                return true;
            }

            if (IsTransformActive(superController.loadingIcon))
            {
                return true;
            }

            return false;
        }

        static bool IsTransformActive(Transform sceneTransform)
        {
            if (sceneTransform == null)
            {
                return false;
            }

            return sceneTransform.gameObject.activeSelf;
        }

        void ApplyCamExposureWhileSceneSettling()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                return;
            }

            if (!camExposureBackupCaptured)
            {
                savedCamExposure = globalLightingStorable.GetFloatParamValue(camExposureParamName);
                camExposureBackupCaptured = true;
                DebugLog(string.Format("Snapshot GlobalLighting camExposure={0}; forcing 0 until settle ends.", savedCamExposure));
            }

            globalLightingStorable.SetFloatParamValue(camExposureParamName, 0f);
        }

        void RestoreCamExposure()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                SuperController.LogError("[OnSceneStartup] Restore: CoreControl GlobalLighting not available.");
                camExposureBackupCaptured = false;
                return;
            }

            globalLightingStorable.SetFloatParamValue(camExposureParamName, savedCamExposure);
            camExposureBackupCaptured = false;
        }

        JSONStorable TryGetGlobalLightingStorable()
        {
            Atom coreAtom = SuperController.singleton.GetAtomByUid(coreControlAtomUid);
            if (coreAtom == null || coreAtom.destroyed)
            {
                return null;
            }

            return coreAtom.GetStorableByID(globalLightingStorableId);
        }

        static void DebugLog(string messageBody)
        {
            string timeText = DateTime.Now.ToString("HH:mm:ss.fff");
            SuperController.LogMessage("[OnSceneStartup] " + timeText + " " + messageBody);
        }
    }
}
