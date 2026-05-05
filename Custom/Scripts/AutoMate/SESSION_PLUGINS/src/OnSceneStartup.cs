using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Forces CoreControl GlobalLighting &quot;camExposure&quot; to 0 while VaM is still settling after a load:
    /// SuperController scene load, full-screen loading UI/geometry (which stays up briefly after isLoading clears),
    /// and the loading icon used for queued textures (ImageLoader) and URL audio loads.
    /// Restores the snapshot value once all of those are inactive.
    /// Bright reads (&gt;= ~1) always set the backup even while isLoading (Default.json). Dim reads use Mathf.Min so transient 1.0 then 0.03 still restores 0.03 (pass.json). Bright after dim overrides stale low carryover from the previous scene (MainMenu after pass.json).
    /// Tick runs from LateUpdate so CoreControl JSON usually reflects the scene before we read.
    /// </summary>
    public class OnSceneStartup
    {
        private bool wasSceneStillSettling = false;

        private bool camExposureBackupCaptured = false;
        private float savedCamExposure = 0f;

        private const string coreControlAtomUid = "CoreControl";
        private const string globalLightingStorableId = "GlobalLighting";
        private const string camExposureParamName = "camExposure";

        private const float forcedExposureEpsilon = 0.001f;

        private const float nearDefaultFullExposure = 0.99f;

        public void TickDuringSuperControllerLoad()
        {
            bool settlingNow = ShouldTreatSceneAsStillSettling();

            if (settlingNow)
            {
                if (!wasSceneStillSettling)
                {
                    camExposureBackupCaptured = false;
                }

                ApplyCamExposureWhileSceneSettling();
            }
            else
            {
                if (wasSceneStillSettling)
                {
                    if (camExposureBackupCaptured)
                    {
                        try
                        {
                            RestoreCamExposure();
                        }
                        catch (Exception restoreException)
                        {
                            SuperController.LogError("[OnSceneStartup] Restore camExposure after settle phase failed: " + restoreException);
                        }
                    }
                    else
                    {
                        RestoreCamExposureUsingGlobalLightingDefaultBecauseBackupWasNeverCaptured();
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

            float rawCamExposure = globalLightingStorable.GetFloatParamValue(camExposureParamName);

            MaybeUpdateCamExposureBackupFromScene(rawCamExposure);

            globalLightingStorable.SetFloatParamValue(camExposureParamName, 0f);
        }

        void MaybeUpdateCamExposureBackupFromScene(float rawCamExposure)
        {
            if (rawCamExposure <= forcedExposureEpsilon)
            {
                return;
            }

            if (rawCamExposure >= nearDefaultFullExposure)
            {
                savedCamExposure = rawCamExposure;
                camExposureBackupCaptured = true;

                return;
            }

            if (!camExposureBackupCaptured)
            {
                savedCamExposure = rawCamExposure;
                camExposureBackupCaptured = true;
            }
            else
            {
                savedCamExposure = Mathf.Min(savedCamExposure, rawCamExposure);
            }
        }

        void RestoreCamExposureUsingGlobalLightingDefaultBecauseBackupWasNeverCaptured()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                SuperController.LogError("[OnSceneStartup] Settle ended without camExposure backup and CoreControl GlobalLighting was missing; exposure may stay at 0.");
                return;
            }

            JSONStorableFloat camExposureJsonFloat = globalLightingStorable.GetFloatJSONParam(camExposureParamName);
            float fallbackCamExposure = 1f;

            if (camExposureJsonFloat != null)
            {
                fallbackCamExposure = camExposureJsonFloat.defaultVal;
            }

            try
            {
                savedCamExposure = fallbackCamExposure;
                camExposureBackupCaptured = true;
                RestoreCamExposure();
            }
            catch (Exception fallbackRestoreException)
            {
                SuperController.LogError("[OnSceneStartup] Fallback restore camExposure failed: " + fallbackRestoreException);
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
    }
}
