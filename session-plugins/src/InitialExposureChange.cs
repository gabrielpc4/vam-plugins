using System;
using UnityEngine;

namespace geesp0t
{
    public partial class SceneSettleRuntime
    {
        private bool camExposureBackupCaptured = false;

        private float savedCamExposure = 0f;

        private Atom cachedCoreControlAtom;

        private JSONStorable cachedGlobalLightingStorable;

        private const string coreControlAtomUid = "CoreControl";

        private const string globalLightingStorableId = "GlobalLighting";

        private const string camExposureParamName = "camExposure";

        private const float forcedExposureEpsilon = 0.001f;

        private const float nearDefaultFullExposure = 0.99f;

        void ApplyCamExposureWhileSceneSettling()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                return;
            }

            float rawCamExposure =
                globalLightingStorable.GetFloatParamValue(camExposureParamName);

            MaybeUpdateCamExposureBackupFromScene(rawCamExposure);

            if (rawCamExposure > forcedExposureEpsilon)
            {
                globalLightingStorable.SetFloatParamValue(
                    camExposureParamName,
                    0f);
            }
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
                SuperController.LogError(
                    "[SceneSettle] Settle ended without camExposure " +
                    "backup and CoreControl GlobalLighting was missing; " +
                    "exposure may stay at 0.");
                return;
            }

            JSONStorableFloat camExposureJsonFloat =
                globalLightingStorable.GetFloatJSONParam(camExposureParamName);
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
                SuperController.LogError(
                    "[SceneSettle] Fallback restore camExposure failed: " +
                    fallbackRestoreException);
            }
        }

        void RestoreCamExposure()
        {
            JSONStorable globalLightingStorable = TryGetGlobalLightingStorable();
            if (globalLightingStorable == null)
            {
                SuperController.LogError(
                    "[SceneSettle] Restore: CoreControl GlobalLighting " +
                    "not available.");
                camExposureBackupCaptured = false;
                return;
            }

            globalLightingStorable.SetFloatParamValue(
                camExposureParamName,
                savedCamExposure);
            camExposureBackupCaptured = false;
        }

        JSONStorable TryGetGlobalLightingStorable()
        {
            if (cachedCoreControlAtom != null &&
                !cachedCoreControlAtom.destroyed &&
                cachedGlobalLightingStorable != null)
            {
                return cachedGlobalLightingStorable;
            }

            SuperController superController = SuperController.singleton;
            if (superController == null)
            {
                ClearGlobalLightingCache();
                return null;
            }

            Atom coreAtom = superController.GetAtomByUid(coreControlAtomUid);
            if (coreAtom == null || coreAtom.destroyed)
            {
                ClearGlobalLightingCache();
                return null;
            }

            JSONStorable globalLightingStorable =
                coreAtom.GetStorableByID(globalLightingStorableId);
            if (globalLightingStorable == null)
            {
                ClearGlobalLightingCache();
                return null;
            }

            cachedCoreControlAtom = coreAtom;
            cachedGlobalLightingStorable = globalLightingStorable;

            return globalLightingStorable;
        }

        void ClearGlobalLightingCache()
        {
            cachedCoreControlAtom = null;
            cachedGlobalLightingStorable = null;
        }
    }
}
