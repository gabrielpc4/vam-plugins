using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Forces CoreControl GlobalLighting &quot;camExposure&quot; to 0 while VaM is still settling after a load:
    /// SuperController scene load, full-screen loading UI/geometry (which stays up briefly after isLoading clears),
    /// and the loading icon used for queued textures (ImageLoader) and URL audio loads.
    /// Restores the snapshot value once all of those are inactive.
    /// Backup prefers scene-applied exposure (&lt; ~1 default) and can refine down via Mathf.Min if we briefly saw 1.0 first.
    /// Tick runs from LateUpdate so CoreControl JSON usually reflects the scene before we read.
    /// </summary>
    public class OnSceneStartup
    {
        private bool wasSceneStillSettling = false;

        private bool camExposureBackupCaptured = false;
        private float savedCamExposure = 0f;

        private float lastSettleLoggedRawCamExposure = float.NaN;
        private float lastNear003ProbeLoggedRaw = float.NaN;

        private const string coreControlAtomUid = "CoreControl";
        private const string globalLightingStorableId = "GlobalLighting";
        private const string camExposureParamName = "camExposure";

        private const float forcedExposureEpsilon = 0.001f;

        private const float nearDefaultFullExposure = 0.99f;

        private const float passJsonExposureHint = 0.03f;
        private const float passJsonExposureProbeHalfWidth = 0.008f;

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
                    lastSettleLoggedRawCamExposure = float.NaN;
                    lastNear003ProbeLoggedRaw = float.NaN;
                    DebugLog("Settle phase started (isLoading and/or loading UI/icon); refining camExposure backup each LateUpdate before forcing 0.");
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

            SuperController superController = SuperController.singleton;
            float rawCamExposure = globalLightingStorable.GetFloatParamValue(camExposureParamName);

            LogSettleExposureSampleIfChanged(superController, rawCamExposure);
            LogNearPassJsonExposureHintIfNeeded(superController, rawCamExposure);

            MaybeUpdateCamExposureBackupFromScene(superController, rawCamExposure);

            globalLightingStorable.SetFloatParamValue(camExposureParamName, 0f);
        }

        void LogSettleExposureSampleIfChanged(SuperController superController, float rawCamExposure)
        {
            if (float.IsNaN(lastSettleLoggedRawCamExposure) || Mathf.Abs(rawCamExposure - lastSettleLoggedRawCamExposure) > 0.0005f)
            {
                lastSettleLoggedRawCamExposure = rawCamExposure;
                DebugLog(string.Format("Settle LateUpdate read camExposure={0} (isLoading={1}) — next line forces 0.", rawCamExposure, superController.isLoading));
            }
        }

        void LogNearPassJsonExposureHintIfNeeded(SuperController superController, float rawCamExposure)
        {
            float deltaFromHint = Mathf.Abs(rawCamExposure - passJsonExposureHint);
            if (deltaFromHint > passJsonExposureProbeHalfWidth)
            {
                return;
            }

            if (!float.IsNaN(lastNear003ProbeLoggedRaw) && Mathf.Abs(rawCamExposure - lastNear003ProbeLoggedRaw) < 0.0001f)
            {
                return;
            }

            lastNear003ProbeLoggedRaw = rawCamExposure;
            DebugLog(string.Format("PROBE ~pass.json camExposure band: raw={0} isLoading={1} (hint target ~{2}).", rawCamExposure, superController.isLoading, passJsonExposureHint));
        }

        void MaybeUpdateCamExposureBackupFromScene(SuperController superController, float rawCamExposure)
        {
            if (rawCamExposure <= forcedExposureEpsilon)
            {
                return;
            }

            if (rawCamExposure < nearDefaultFullExposure)
            {
                float savedCamExposureBefore = savedCamExposure;
                bool backupHeldBefore = camExposureBackupCaptured;

                if (!camExposureBackupCaptured)
                {
                    savedCamExposure = rawCamExposure;
                    camExposureBackupCaptured = true;
                }
                else
                {
                    savedCamExposure = Mathf.Min(savedCamExposure, rawCamExposure);
                }

                if (!backupHeldBefore || Mathf.Abs(savedCamExposureBefore - savedCamExposure) > 0.0001f)
                {
                    DebugLog(string.Format("Backup camExposure scene-like raw={0} saved->{1} isLoading={2}.", rawCamExposure, savedCamExposure, superController.isLoading));
                }

                return;
            }

            if (!superController.isLoading)
            {
                if (!camExposureBackupCaptured)
                {
                    savedCamExposure = rawCamExposure;
                    camExposureBackupCaptured = true;
                    DebugLog(string.Format("Backup camExposure post-load default-range raw={0} saved->{1}.", rawCamExposure, savedCamExposure));
                }
            }
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
