using System;
using System.Collections.Generic;

namespace geesp0t
{
    /// <summary>
    /// Hides InvisibleLight and UIButton atoms plus CoreControl GlobalLighting while SuperController loads a scene,
    /// then restores them on the same ~1s sceneChanged gate Auto_Load_Person_Plugins uses for person plugins.
    /// </summary>
    public class OnSceneStartup
    {
        private sealed class SceneToggleAtomBackupEntry
        {
            public string atomUid;
            public bool savedOn;
        }

        private readonly List<SceneToggleAtomBackupEntry> sceneToggleAtomBackupList = new List<SceneToggleAtomBackupEntry>();
        private bool sceneToggleAtomBackupActive = false;
        private bool wasSuperControllerLoading = false;

        private bool sceneLightingRestorePendingAfterLoad = false;

        private bool toggleAtomBackupPendingDuringLoad = false;
        private bool toggleAtomBackupWaitLogged = false;

        private bool globalLightingDimBackupCaptured = false;
        private bool globalLightingRestoreShowSkybox = false;
        private float globalLightingRestoreMasterIntensity = 0f;
        private float globalLightingRestoreDiffuseIntensity = 0f;
        private float globalLightingRestoreSpecularIntensity = 0f;
        private float globalLightingRestoreCamExposure = 0f;
        private float globalLightingRestoreSkyboxIntensity = 0f;

        private const string coreControlAtomUid = "CoreControl";
        private const string globalLightingStorableId = "GlobalLighting";

        public void TickDuringSuperControllerLoad()
        {
            bool superControllerLoading = SuperController.singleton.isLoading;

            if (superControllerLoading != wasSuperControllerLoading)
            {
                DebugLog(string.Format("SuperController.isLoading {0} -> {1}", wasSuperControllerLoading, superControllerLoading));
            }

            if (superControllerLoading && !wasSuperControllerLoading)
            {
                sceneToggleAtomBackupList.Clear();
                sceneToggleAtomBackupActive = false;
                sceneLightingRestorePendingAfterLoad = false;
                toggleAtomBackupPendingDuringLoad = true;
                toggleAtomBackupWaitLogged = false;
                globalLightingDimBackupCaptured = false;
                DebugLog("Load started: stale backup discarded; will hide InvisibleLight, UIButton, and GlobalLighting when atoms appear.");
            }

            MergeToggleAtomsAndGlobalLightingDuringLoad();

            if (!superControllerLoading && wasSuperControllerLoading)
            {
                if (toggleAtomBackupPendingDuringLoad && sceneToggleAtomBackupList.Count == 0 && !globalLightingDimBackupCaptured)
                {
                    DebugLog("Load finished before any InvisibleLight/UIButton atoms or CoreControl GlobalLighting appeared; no lighting backup for this load.");
                }

                toggleAtomBackupPendingDuringLoad = false;
                toggleAtomBackupWaitLogged = false;

                DebugLog("Scene load finished (SuperController.isLoading became false).");

                if (sceneToggleAtomBackupActive || globalLightingDimBackupCaptured)
                {
                    sceneLightingRestorePendingAfterLoad = true;
                    DebugLog("Restore will run on sceneChanged gate (~1s) after textures/plugins settle.");
                }
                else
                {
                    DebugLog("No lighting/UI backup active; restore not needed.");
                }
            }

            wasSuperControllerLoading = superControllerLoading;
        }

        public void OnSceneChangedGateAfterLoadSettled()
        {
            if (!sceneLightingRestorePendingAfterLoad)
            {
                return;
            }

            sceneLightingRestorePendingAfterLoad = false;

            try
            {
                DebugLog("sceneChanged gate (~1s after SuperController load): restoring GlobalLighting, InvisibleLight, UIButton.");
                RestoreSceneLightingFromBackup();
            }
            catch (Exception restoreException)
            {
                SuperController.LogError("[OnSceneStartup] RestoreSceneLightingFromBackup after scene settled failed: " + restoreException);
            }
        }

        public void OnOwningPluginDestroy()
        {
            if (sceneToggleAtomBackupActive || globalLightingDimBackupCaptured)
            {
                DebugLog("OnDestroy: restoring scene lighting before unload.");
                try
                {
                    RestoreSceneLightingFromBackup();
                }
                catch (Exception lightsRestoreException)
                {
                    SuperController.LogError("[OnSceneStartup] RestoreSceneLightingFromBackup in OnDestroy failed: " + lightsRestoreException);
                }
            }
        }

        static bool IsVaMLightAtom(Atom sceneAtom)
        {
            if (sceneAtom == null)
            {
                return false;
            }

            if (sceneAtom.destroyed)
            {
                return false;
            }

            if (sceneAtom.type == "InvisibleLight")
            {
                return true;
            }

            if (sceneAtom.category == "Light")
            {
                return true;
            }

            return false;
        }

        static bool IsVaMUIButtonAtom(Atom sceneAtom)
        {
            if (sceneAtom == null)
            {
                return false;
            }

            if (sceneAtom.destroyed)
            {
                return false;
            }

            return sceneAtom.type == "UIButton";
        }

        static bool IsVaMHiddenDuringLoadToggleAtom(Atom sceneAtom)
        {
            return IsVaMLightAtom(sceneAtom) || IsVaMUIButtonAtom(sceneAtom);
        }

        bool ToggleAtomUidAlreadyInBackup(string atomUid)
        {
            for (int i = 0; i < sceneToggleAtomBackupList.Count; i++)
            {
                if (sceneToggleAtomBackupList[i].atomUid == atomUid)
                {
                    return true;
                }
            }

            return false;
        }

        void MergeToggleAtomsAndGlobalLightingDuringLoad()
        {
            if (!SuperController.singleton.isLoading || !toggleAtomBackupPendingDuringLoad)
            {
                return;
            }

            int totalToggleAtomsInScene = 0;

            foreach (Atom sceneAtom in SuperController.singleton.GetAtoms())
            {
                if (IsVaMHiddenDuringLoadToggleAtom(sceneAtom))
                {
                    totalToggleAtomsInScene++;
                }
            }

            if (totalToggleAtomsInScene == 0 && sceneToggleAtomBackupList.Count == 0 && !globalLightingDimBackupCaptured && !toggleAtomBackupWaitLogged)
            {
                toggleAtomBackupWaitLogged = true;
                DebugLog("Load in progress: no InvisibleLight/UIButton atoms in GetAtoms yet; retrying each frame until they spawn.");
            }

            foreach (Atom sceneAtom in SuperController.singleton.GetAtoms())
            {
                if (!IsVaMHiddenDuringLoadToggleAtom(sceneAtom))
                {
                    continue;
                }

                string atomUid = sceneAtom.uid;

                if (ToggleAtomUidAlreadyInBackup(atomUid))
                {
                    continue;
                }

                bool wasOn = sceneAtom.on;

                DebugLog(string.Format(
                    "Load in progress: new atom \"{0}\" ({1}) uid=\"{2}\": on {3} -> false",
                    sceneAtom.name,
                    sceneAtom.type,
                    atomUid,
                    wasOn));

                SceneToggleAtomBackupEntry backupEntry = new SceneToggleAtomBackupEntry();
                backupEntry.atomUid = atomUid;
                backupEntry.savedOn = wasOn;
                sceneToggleAtomBackupList.Add(backupEntry);

                sceneAtom.SetOn(false);

                DebugLog(string.Format("After SetOn(false), uid=\"{0}\" read-back on: {1}", atomUid, sceneAtom.on));
            }

            if (sceneToggleAtomBackupList.Count > 0)
            {
                sceneToggleAtomBackupActive = true;
            }

            MergeGlobalLightingDuringLoad();
        }

        void MergeGlobalLightingDuringLoad()
        {
            if (!SuperController.singleton.isLoading || !toggleAtomBackupPendingDuringLoad)
            {
                return;
            }

            Atom coreAtom = SuperController.singleton.GetAtomByUid(coreControlAtomUid);
            if (coreAtom == null || coreAtom.destroyed)
            {
                return;
            }

            JSONStorable globalLightingStorable = coreAtom.GetStorableByID(globalLightingStorableId);
            if (globalLightingStorable == null)
            {
                return;
            }

            if (!globalLightingDimBackupCaptured)
            {
                globalLightingRestoreShowSkybox = globalLightingStorable.GetBoolParamValue("showSkybox");
                globalLightingRestoreMasterIntensity = globalLightingStorable.GetFloatParamValue("masterIntensity");
                globalLightingRestoreDiffuseIntensity = globalLightingStorable.GetFloatParamValue("diffuseIntensity");
                globalLightingRestoreSpecularIntensity = globalLightingStorable.GetFloatParamValue("specularIntensity");
                globalLightingRestoreCamExposure = globalLightingStorable.GetFloatParamValue("camExposure");
                globalLightingRestoreSkyboxIntensity = globalLightingStorable.GetFloatParamValue("skyboxIntensity");
                globalLightingDimBackupCaptured = true;

                DebugLog(string.Format(
                    "GlobalLighting snapshot (showSkybox={0}, skyboxIntensity={1}); forcing dark each frame until load completes.",
                    globalLightingRestoreShowSkybox,
                    globalLightingRestoreSkyboxIntensity));
            }

            globalLightingStorable.SetBoolParamValue("showSkybox", false);
            globalLightingStorable.SetFloatParamValue("masterIntensity", 0f);
            globalLightingStorable.SetFloatParamValue("diffuseIntensity", 0f);
            globalLightingStorable.SetFloatParamValue("specularIntensity", 0f);
            globalLightingStorable.SetFloatParamValue("camExposure", 0f);
            globalLightingStorable.SetFloatParamValue("skyboxIntensity", 0f);
        }

        void RestoreSceneLightingFromBackup()
        {
            DebugLog("RestoreSceneLightingFromBackup entered.");

            bool hadAtomBackup = sceneToggleAtomBackupList.Count > 0;
            bool hadGlobalBackup = globalLightingDimBackupCaptured;

            if (!hadAtomBackup && !hadGlobalBackup)
            {
                DebugLog("No lighting backup active; restore exits without changes.");
                return;
            }

            if (hadGlobalBackup)
            {
                Atom coreAtom = SuperController.singleton.GetAtomByUid(coreControlAtomUid);
                if (coreAtom != null && !coreAtom.destroyed)
                {
                    JSONStorable globalLightingStorable = coreAtom.GetStorableByID(globalLightingStorableId);
                    if (globalLightingStorable != null)
                    {
                        DebugLog(string.Format(
                            "Restoring GlobalLighting (showSkybox={0}, skyboxIntensity={1}).",
                            globalLightingRestoreShowSkybox,
                            globalLightingRestoreSkyboxIntensity));

                        globalLightingStorable.SetBoolParamValue("showSkybox", globalLightingRestoreShowSkybox);
                        globalLightingStorable.SetFloatParamValue("masterIntensity", globalLightingRestoreMasterIntensity);
                        globalLightingStorable.SetFloatParamValue("diffuseIntensity", globalLightingRestoreDiffuseIntensity);
                        globalLightingStorable.SetFloatParamValue("specularIntensity", globalLightingRestoreSpecularIntensity);
                        globalLightingStorable.SetFloatParamValue("camExposure", globalLightingRestoreCamExposure);
                        globalLightingStorable.SetFloatParamValue("skyboxIntensity", globalLightingRestoreSkyboxIntensity);
                    }
                    else
                    {
                        SuperController.LogError("[OnSceneStartup] Restore: CoreControl has no GlobalLighting storable.");
                    }
                }
                else
                {
                    SuperController.LogError("[OnSceneStartup] Restore: CoreControl atom missing; cannot restore GlobalLighting.");
                }

                globalLightingDimBackupCaptured = false;
            }

            if (hadAtomBackup)
            {
                DebugLog(string.Format("Restoring {0} atom backup entry/entries (InvisibleLight / UIButton).", sceneToggleAtomBackupList.Count));

                for (int entryIndex = 0; entryIndex < sceneToggleAtomBackupList.Count; entryIndex++)
                {
                    SceneToggleAtomBackupEntry backupEntry = sceneToggleAtomBackupList[entryIndex];
                    Atom sceneAtom = SuperController.singleton.GetAtomByUid(backupEntry.atomUid);

                    if (sceneAtom != null && IsVaMHiddenDuringLoadToggleAtom(sceneAtom))
                    {
                        bool onBefore = sceneAtom.on;
                        DebugLog(string.Format("Try restore uid=\"{0}\" ({1}): on now {2} -> saved {3}", backupEntry.atomUid, sceneAtom.type, onBefore, backupEntry.savedOn));
                        sceneAtom.SetOn(backupEntry.savedOn);
                        DebugLog(string.Format("After SetOn, uid=\"{0}\" read-back on: {1}", backupEntry.atomUid, sceneAtom.on));
                    }
                    else
                    {
                        DebugLog(string.Format("Entry {0} uid=\"{1}\": atom missing or not InvisibleLight/UIButton; skip.", entryIndex, backupEntry.atomUid));
                    }
                }
            }

            sceneToggleAtomBackupList.Clear();
            sceneToggleAtomBackupActive = false;
            DebugLog("Restore complete; backup cleared.");
        }

        static void DebugLog(string messageBody)
        {
            string timeText = DateTime.Now.ToString("HH:mm:ss.fff");
            SuperController.LogMessage("[OnSceneStartup] " + timeText + " " + messageBody);
        }
    }
}
