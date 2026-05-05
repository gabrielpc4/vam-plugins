using System;

namespace geesp0t
{
    /// <summary>
    /// While SuperController loads a scene, forces CoreControl GlobalLighting &quot;camExposure&quot; to 0,
    /// then restores the saved value on the same sceneChanged gate as Auto_Load_Person_Plugins.
    /// </summary>
    public class OnSceneStartup
    {
        private bool wasSuperControllerLoading = false;

        private bool exposureDimPendingDuringLoad = false;
        private bool camExposureBackupCaptured = false;
        private float savedCamExposure = 0f;

        private bool camExposureRestorePendingAfterLoad = false;

        private const string coreControlAtomUid = "CoreControl";
        private const string globalLightingStorableId = "GlobalLighting";
        private const string camExposureParamName = "camExposure";

        public void TickDuringSuperControllerLoad()
        {
            bool superControllerLoading = SuperController.singleton.isLoading;

            if (superControllerLoading != wasSuperControllerLoading)
            {
                DebugLog(string.Format("SuperController.isLoading {0} -> {1}", wasSuperControllerLoading, superControllerLoading));
            }

            if (superControllerLoading && !wasSuperControllerLoading)
            {
                camExposureRestorePendingAfterLoad = false;
                camExposureBackupCaptured = false;
                exposureDimPendingDuringLoad = true;
                DebugLog("Load started: will force GlobalLighting camExposure to 0 until load completes.");
            }

            ApplyCamExposureDuringLoad();

            if (!superControllerLoading && wasSuperControllerLoading)
            {
                exposureDimPendingDuringLoad = false;

                DebugLog("Scene load finished (SuperController.isLoading became false).");

                if (camExposureBackupCaptured)
                {
                    camExposureRestorePendingAfterLoad = true;
                    DebugLog("camExposure restore will run on sceneChanged gate (~1s).");
                }
                else
                {
                    DebugLog("GlobalLighting was never available during load; camExposure not changed.");
                }
            }

            wasSuperControllerLoading = superControllerLoading;
        }

        public void OnSceneChangedGateAfterLoadSettled()
        {
            if (!camExposureRestorePendingAfterLoad)
            {
                return;
            }

            camExposureRestorePendingAfterLoad = false;

            try
            {
                DebugLog(string.Format("sceneChanged gate: restoring camExposure to {0}.", savedCamExposure));
                RestoreCamExposure();
            }
            catch (Exception restoreException)
            {
                SuperController.LogError("[OnSceneStartup] Restore camExposure after scene settled failed: " + restoreException);
            }
        }

        public void OnOwningPluginDestroy()
        {
            if (!camExposureBackupCaptured && !camExposureRestorePendingAfterLoad)
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

        void ApplyCamExposureDuringLoad()
        {
            if (!SuperController.singleton.isLoading || !exposureDimPendingDuringLoad)
            {
                return;
            }

            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                return;
            }

            if (!camExposureBackupCaptured)
            {
                savedCamExposure = globalLightingStorable.GetFloatParamValue(camExposureParamName);
                camExposureBackupCaptured = true;
                DebugLog(string.Format("Snapshot GlobalLighting camExposure={0}; forcing 0 during load.", savedCamExposure));
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
                camExposureRestorePendingAfterLoad = false;
                return;
            }

            globalLightingStorable.SetFloatParamValue(camExposureParamName, savedCamExposure);
            camExposureBackupCaptured = false;
            camExposureRestorePendingAfterLoad = false;
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
