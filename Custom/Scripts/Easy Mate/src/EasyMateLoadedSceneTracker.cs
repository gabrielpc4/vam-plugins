using System;
using System.Collections.Generic;
using SimpleJSON;

namespace geesp0t
{
    /// <summary>
    /// Tracks the exact scene JSON path VaM most recently loaded so follow-up
    /// tools can target that same file instead of guessing from folder state.
    /// When several <c>.json</c> files share <see cref="SuperController.currentLoadDir"/>,
    /// resolution uses public <see cref="SuperController.loadJson"/> in tiers: one file in the folder
    /// skips serialization; otherwise compare normalized raw disk text vs one dump, then parse only if needed.
    /// </summary>
    public static class EasyMateLoadedSceneTracker
    {
        public const string TrackerStateRelative = "Custom/Scripts/Easy Mate/tools/last_loaded_scene_tracker.json";

        private static string _lastLoadedSceneJsonPath = "";

        private static string _lastLoadedSceneLoadDir = "";

        private static string _pendingExplicitSceneJsonPath = "";

        private static string _lastTrackerNotifyLogKey = "";

        public static void LogLoadingStartedSummary(SuperController sc)
        {
            string normalizedCurrentLoadDir;
            string normalizedCurrentSaveDir;
            SceneState diskState;

            if (sc == null)
            {
                return;
            }

            normalizedCurrentLoadDir = NormalizeFwd(sc.currentLoadDir != null ? sc.currentLoadDir : "");
            normalizedCurrentSaveDir = NormalizeFwd(sc.currentSaveDir != null ? sc.currentSaveDir : "");

            SuperController.LogMessage(
                string.Format(
                    "EasyMate [scene tracker]: VaM started loading - currentLoadDir={0} currentSaveDir={1}",
                    normalizedCurrentLoadDir,
                    normalizedCurrentSaveDir));

            diskState = TryPeekTrackerStateFromDisk(sc);
            if (diskState != null && diskState.IsValid)
            {
                SuperController.LogMessage(string.Format(
                    "EasyMate [scene tracker]: before this load, state file had scene JSON={0} tracked folder={1}",
                    NormalizeFwd(diskState.SceneJsonPath),
                    NormalizeFwd(diskState.LoadDir)));
            }
            else
            {
                SuperController.LogMessage(
                    string.Format(
                        "EasyMate [scene tracker]: before this load, no usable state yet (file {0} missing or empty).",
                        TrackerStateRelative));
            }
        }

        public static void RememberUpcomingSceneLoad(
            SuperController sc,
            string sceneJsonPath)
        {
            SuperController.LogMessage(string.Format(
                "EasyMate [scene tracker]: registering upcoming load target path={0}",
                NormalizeFwd(sceneJsonPath)));
            if (!IsPatchableLocalScenePath(sceneJsonPath))
            {
                SuperController.LogMessage(
                    "EasyMate [scene tracker]: skipping register (not a patchable local Saves/.json scene path).");
                return;
            }

            RememberLoadedScene(sc, sceneJsonPath, true);
            _pendingExplicitSceneJsonPath = NormalizeFwd(sceneJsonPath);
        }

        public static void NotifySceneLoaded(SuperController sc)
        {
            string normalizedLoadDir;
            JSONClass diskTrackerJson;
            SceneState diskTrackedState;
            string logKeyTrustExact;
            string normalizedPendingExplicit;
            string reconciledSceneJsonPath;
            string logKeyExplicit;
            string loadJsonMatchScenePathFwd;
            string logKeyLoadJsonFingerprint;
            string logKeyReconcile;

            if (sc == null)
            {
                return;
            }

            normalizedLoadDir = NormalizeFwd(sc.currentLoadDir != null ? sc.currentLoadDir : "");
            if (normalizedLoadDir.Length == 0)
            {
                return;
            }

            diskTrackerJson = TryLoadTrackerDiskJson(sc);
            if (diskTrackerJson != null)
            {
                diskTrackedState = TryParseTrackerDiskSceneState(diskTrackerJson);
                if (diskTrackedState != null &&
                    diskTrackedState.TrustExactPath &&
                    diskTrackedState.IsValid &&
                    LoadDirsMatch(sc, diskTrackedState.LoadDir) &&
                    FileExists(sc, diskTrackedState.SceneJsonPath))
                {
                    logKeyTrustExact = "trustexact|" + normalizedLoadDir + "|" + NormalizeFwd(diskTrackedState.SceneJsonPath);
                    LogTrackerPhaseOnce(
                        logKeyTrustExact,
                        string.Format(
                            "EasyMate [scene tracker]: after load, honoring trustExactPath for {0} (currentLoadDir={1})",
                            NormalizeFwd(diskTrackedState.SceneJsonPath),
                            normalizedLoadDir));
                    RememberLoadedScene(sc, diskTrackedState.SceneJsonPath, false);
                    _pendingExplicitSceneJsonPath = "";
                    return;
                }
            }

            if (TryResolveExactSceneJsonMatchingPublicSuperControllerLoadJsonFingerprint(
                    sc,
                    normalizedLoadDir,
                    out loadJsonMatchScenePathFwd))
            {
                logKeyLoadJsonFingerprint = "loadjson|" + normalizedLoadDir + "|" + NormalizeFwd(loadJsonMatchScenePathFwd);
                LogTrackerPhaseOnce(
                    logKeyLoadJsonFingerprint,
                    string.Format(
                        "EasyMate [scene tracker]: after load, matched public SuperController.loadJson fingerprint to disk file={0} (currentLoadDir={1})",
                        NormalizeFwd(loadJsonMatchScenePathFwd),
                        normalizedLoadDir));
                RememberLoadedScene(sc, loadJsonMatchScenePathFwd, false);
                _pendingExplicitSceneJsonPath = "";
                return;
            }

            normalizedPendingExplicit = NormalizeFwd(_pendingExplicitSceneJsonPath);
            if (normalizedPendingExplicit.Length > 0)
            {
                if (LoadDirsMatch(sc, GetDirectoryPath(normalizedPendingExplicit)))
                {
                    logKeyExplicit = "explicit|" + normalizedLoadDir + "|" + normalizedPendingExplicit;
                    LogTrackerPhaseOnce(
                        logKeyExplicit,
                        string.Format(
                            "EasyMate [scene tracker]: after load, using explicit preload path {0} (currentLoadDir={1})",
                            normalizedPendingExplicit,
                            normalizedLoadDir));
                    RememberLoadedScene(sc, normalizedPendingExplicit, false);
                    _pendingExplicitSceneJsonPath = "";
                    return;
                }

                _pendingExplicitSceneJsonPath = "";
            }

            reconciledSceneJsonPath = PickMainSceneJsonRelativeFromLoadFolder(sc, normalizedLoadDir);
            if (reconciledSceneJsonPath.Length == 0)
            {
                SuperController.LogError(string.Format(
                    "EasyMate [scene tracker]: after load, could not pick a scene .json in currentLoadDir={0}",
                    normalizedLoadDir));

                return;
            }

            logKeyReconcile = "reconcile|" + normalizedLoadDir + "|" + NormalizeFwd(reconciledSceneJsonPath);
            LogTrackerPhaseOnce(
                logKeyReconcile,
                string.Format(
                    "EasyMate [scene tracker]: after load, reconciled patch target to {0} (currentLoadDir={1}, same rules as patch_scene_initial_camera.py)",
                    NormalizeFwd(reconciledSceneJsonPath),
                    normalizedLoadDir));

            RememberLoadedScene(sc, reconciledSceneJsonPath, false);
        }

        public static bool TryGetPatchTargetSceneJsonPath(
            SuperController sc,
            out string sceneJsonPath,
            out string scenePathSource,
            out string errorMessage)
        {
            SceneState rememberedState;

            sceneJsonPath = "";
            scenePathSource = "";
            errorMessage = "";

            rememberedState = GetRememberedState(sc);
            if (rememberedState == null || !rememberedState.IsValid)
            {
                errorMessage =
                    "could not determine the exact last loaded scene JSON path from public EasyMate state. Load the scene through EasyMate menu buttons first, then press K again.";
                return false;
            }

            if (!LoadDirsMatch(sc, rememberedState.LoadDir))
            {
                LogLoadFolderMismatch(sc, rememberedState);
                errorMessage =
                    "remembered scene JSON does not match the current load folder, so EasyMate will not patch a stale scene path.";
                return false;
            }

            sceneJsonPath = rememberedState.SceneJsonPath;
            scenePathSource = "EasyMate scene tracker";
            return true;
        }

        public static string ToAbsoluteScenePath(string installRoot, string sceneJsonPath)
        {
            string normalizedSceneJsonPath;

            normalizedSceneJsonPath = NormalizeFwd(sceneJsonPath);
            if (IsAbsolutePath(normalizedSceneJsonPath))
            {
                return normalizedSceneJsonPath;
            }

            return CombineFwd(installRoot, normalizedSceneJsonPath);
        }

        /// <summary>
        /// Uses public <see cref="SuperController.loadJson"/> versus each <c>.json</c>
        /// under <see cref="SuperController.currentLoadDir"/> (<see cref="SuperController.ReadFileIntoString"/>).
        /// This disambiguates folders with multiple scene files without reading non-public VaM fields.
        /// Path selection is tiered for speed:
        /// a single listing uses that file; multiple listings try normalized raw-text equality (one serialize of
        /// <see cref="SuperController.loadJson"/>); only if that misses does it parse and reserialize candidates
        /// (semantic match when VaM changes formatting vs disk).
        /// </summary>
        private static bool TryResolveExactSceneJsonMatchingPublicSuperControllerLoadJsonFingerprint(
            SuperController superControllerReference,
            string normalizedCurrentLoadDirFwdNormalized,
            out string savesRelativeChosenSceneJsonPathFwd)
        {
            string trimmedLoadFolderFwd;
            string liveFingerprintNorm;
            List<string> jsonBasenamesDistinct;
            int basenameIndexWalk;
            string basenameEntry;
            string relativeCandidatePathFwd;
            string fileTextFromDiskWhole;
            string diskRawNormFingerprint;
            JSONNode parsedDiskRoot;
            string diskFingerprintNorm;
            List<string> matchingRelativePathsGathered;
            JSONNode loadedGraphRootEarly;
            int skippedProblematicDiskCandidatesFingerprintPass;
            string onlyBasenameSingleton;
            string onlyRelativeCombinedSingletonFwdNormalized;

            savesRelativeChosenSceneJsonPathFwd = "";

            loadedGraphRootEarly =
                superControllerReference != null ? superControllerReference.loadJson : null;
            if (loadedGraphRootEarly == null)
            {
                return false;
            }

            trimmedLoadFolderFwd = NormalizeFwd(normalizedCurrentLoadDirFwdNormalized).TrimEnd('/');
            if (trimmedLoadFolderFwd.Length == 0)
            {
                return false;
            }

            jsonBasenamesDistinct = new List<string>();
            if (!TryFillDistinctSceneJsonBasenamesListedInFolder(
                    superControllerReference,
                    trimmedLoadFolderFwd,
                    jsonBasenamesDistinct))
            {
                SuperController.LogError(string.Format(
                    "EasyMate [scene tracker]: could not list .json candidates in folder {0}",
                    trimmedLoadFolderFwd));
                return false;
            }

            if (jsonBasenamesDistinct.Count == 0)
            {
                SuperController.LogError(string.Format(
                    "EasyMate [scene tracker]: no .json files listed in folder {0} for fingerprint match.",
                    trimmedLoadFolderFwd));
                return false;
            }

            if (jsonBasenamesDistinct.Count == 1)
            {
                onlyBasenameSingleton = jsonBasenamesDistinct[0];
                onlyRelativeCombinedSingletonFwdNormalized = NormalizeFwd(CombineFwd(trimmedLoadFolderFwd, onlyBasenameSingleton));
                if (FileExists(superControllerReference, onlyRelativeCombinedSingletonFwdNormalized))
                {
                    savesRelativeChosenSceneJsonPathFwd = onlyRelativeCombinedSingletonFwdNormalized;
                    return true;
                }

                SuperController.LogError(string.Format(
                    "EasyMate [scene tracker]: only one .json listed ({0}) but file not found via GetFilesAtPath.",
                    onlyRelativeCombinedSingletonFwdNormalized));
                return false;
            }

            if (!TryBuildNormalizedFingerprintFromJsonTreeRoot(loadedGraphRootEarly, out liveFingerprintNorm))
            {
                return false;
            }

            SuperController.LogMessage(string.Format(
                "EasyMate [scene tracker]: matching loadJson to {0} scene JSON candidate(s) under {1} (fast path: normalized raw text).",
                jsonBasenamesDistinct.Count,
                trimmedLoadFolderFwd));

            matchingRelativePathsGathered = new List<string>();
            basenameIndexWalk = 0;
            while (basenameIndexWalk < jsonBasenamesDistinct.Count)
            {
                basenameEntry = jsonBasenamesDistinct[basenameIndexWalk];
                relativeCandidatePathFwd = CombineFwd(trimmedLoadFolderFwd, basenameEntry);

                fileTextFromDiskWhole = superControllerReference.ReadFileIntoString(relativeCandidatePathFwd);
                if (fileTextFromDiskWhole == null || fileTextFromDiskWhole.Length == 0)
                {
                    basenameIndexWalk++;
                    continue;
                }

                diskRawNormFingerprint = NormalizeSimpleJsonFingerprintDumpText(fileTextFromDiskWhole);
                if (string.Equals(
                        liveFingerprintNorm,
                        diskRawNormFingerprint,
                        StringComparison.Ordinal))
                {
                    matchingRelativePathsGathered.Add(NormalizeFwd(relativeCandidatePathFwd));
                }

                basenameIndexWalk++;
            }

            if (matchingRelativePathsGathered.Count > 1)
            {
                SuperController.LogError(string.Format(
                    "EasyMate [scene tracker]: ambiguous raw-text match ({0}) in {1}; refusing to guess.",
                    matchingRelativePathsGathered.Count,
                    trimmedLoadFolderFwd));
                return false;
            }

            if (matchingRelativePathsGathered.Count == 1)
            {
                savesRelativeChosenSceneJsonPathFwd = matchingRelativePathsGathered[0];
                return true;
            }

            SuperController.LogMessage(string.Format(
                "EasyMate [scene tracker]: raw-text match missed for {0} candidate(s); running slower parse-and-reserialize compare.",
                jsonBasenamesDistinct.Count));

            matchingRelativePathsGathered.Clear();
            skippedProblematicDiskCandidatesFingerprintPass = 0;
            basenameIndexWalk = 0;
            while (basenameIndexWalk < jsonBasenamesDistinct.Count)
            {
                basenameEntry = jsonBasenamesDistinct[basenameIndexWalk];
                relativeCandidatePathFwd = CombineFwd(trimmedLoadFolderFwd, basenameEntry);

                fileTextFromDiskWhole = superControllerReference.ReadFileIntoString(relativeCandidatePathFwd);
                if (fileTextFromDiskWhole == null || fileTextFromDiskWhole.Length == 0)
                {
                    basenameIndexWalk++;
                    continue;
                }

                try
                {
                    parsedDiskRoot = JSON.Parse(fileTextFromDiskWhole);
                }
                catch (Exception)
                {
                    skippedProblematicDiskCandidatesFingerprintPass++;
                    basenameIndexWalk++;
                    continue;
                }

                if (!TryBuildNormalizedFingerprintFromJsonTreeRoot(parsedDiskRoot, out diskFingerprintNorm))
                {
                    skippedProblematicDiskCandidatesFingerprintPass++;
                    basenameIndexWalk++;
                    continue;
                }

                if (string.Equals(
                        liveFingerprintNorm,
                        diskFingerprintNorm,
                        StringComparison.Ordinal))
                {
                    matchingRelativePathsGathered.Add(NormalizeFwd(relativeCandidatePathFwd));
                }

                basenameIndexWalk++;
            }

            if (matchingRelativePathsGathered.Count > 1)
            {
                SuperController.LogError(string.Format(
                    "EasyMate [scene tracker]: ambiguous fingerprint match ({0}) in {1}; refusing to guess.",
                    matchingRelativePathsGathered.Count,
                    trimmedLoadFolderFwd));
                return false;
            }

            if (matchingRelativePathsGathered.Count == 1)
            {
                savesRelativeChosenSceneJsonPathFwd = matchingRelativePathsGathered[0];
                return true;
            }

            SuperController.LogMessage(string.Format(
                "EasyMate [scene tracker]: no .json fingerprint match for public loadJson inside {0} " +
                    "(candidate files={1}, unreadable-or-empty fingerprints skipped={2}).",
                trimmedLoadFolderFwd,
                jsonBasenamesDistinct.Count,
                skippedProblematicDiskCandidatesFingerprintPass));
            return false;
        }

        private static bool TryFillDistinctSceneJsonBasenamesListedInFolder(
            SuperController superControllerReference,
            string trimmedLoadFolderFwdNoTrailingSlash,
            List<string> destinationOrderedDistinctJsonFilenamesOnly)
        {
            string[] listedPathsRawListed;
            int listedPathIndexListed;
            string listedPathFwd;

            destinationOrderedDistinctJsonFilenamesOnly.Clear();

            if (superControllerReference == null)
            {
                return false;
            }

            listedPathsRawListed = null;
            try
            {
                listedPathsRawListed = superControllerReference.GetFilesAtPath(trimmedLoadFolderFwdNoTrailingSlash);
            }
            catch (Exception enumerationExceptionListing)
            {
                SuperController.LogError(string.Format(
                    "EasyMate [scene tracker]: GetFilesAtPath threw for {0}: {1}",
                    trimmedLoadFolderFwdNoTrailingSlash,
                    enumerationExceptionListing.Message));
                return false;
            }

            if (listedPathsRawListed == null)
            {
                return false;
            }

            listedPathIndexListed = 0;
            while (listedPathIndexListed < listedPathsRawListed.Length)
            {
                listedPathFwd = NormalizeFwd(listedPathsRawListed[listedPathIndexListed]);
                AppendJsonSceneFileNamesUnique(destinationOrderedDistinctJsonFilenamesOnly, listedPathFwd);
                listedPathIndexListed++;
            }

            return true;
        }

        private static bool TryBuildNormalizedFingerprintFromJsonTreeRoot(
            JSONNode loadedSceneGraphRootInMemory,
            out string fingerprintNormalizedTextOutWide)
        {
            string rawDumpFingerprintSource;

            fingerprintNormalizedTextOutWide = "";

            if (loadedSceneGraphRootInMemory == null)
            {
                return false;
            }

            try
            {
                rawDumpFingerprintSource = loadedSceneGraphRootInMemory.ToString("");
            }
            catch (Exception dumpExceptionCaptured)
            {
                SuperController.LogError(string.Format(
                    "EasyMate [scene tracker]: JSONNode.ToString for fingerprint threw: {0}",
                    dumpExceptionCaptured.Message));
                return false;
            }

            fingerprintNormalizedTextOutWide = NormalizeSimpleJsonFingerprintDumpText(rawDumpFingerprintSource);
            if (fingerprintNormalizedTextOutWide.Length == 0)
            {
                return false;
            }

            return true;
        }

        private static string NormalizeSimpleJsonFingerprintDumpText(string rawTextInputFingerprintSource)
        {
            string workingTextSweep;

            if (rawTextInputFingerprintSource == null)
            {
                return "";
            }

            workingTextSweep = rawTextInputFingerprintSource.Replace("\r\n", "\n");
            workingTextSweep = workingTextSweep.Replace('\r', '\n');
            return workingTextSweep;
        }

        private static bool LoadDirsMatch(SuperController sc, string rememberedLoadDir)
        {
            string currentLoadDir;

            if (sc == null)
            {
                return false;
            }

            currentLoadDir = NormalizeFwd(sc.currentLoadDir != null ? sc.currentLoadDir : "");
            return string.Equals(
                currentLoadDir,
                NormalizeFwd(rememberedLoadDir),
                StringComparison.OrdinalIgnoreCase);
        }

        private static void LogLoadFolderMismatch(SuperController sc, SceneState rememberedState)
        {
            string normalizedSceneJsonPath;
            string normalizedRememberedLoadDir;
            string normalizedCurrentLoadDir;
            string normalizedCurrentSaveDir;
            bool trackerFileExists;

            if (sc == null || rememberedState == null)
            {
                return;
            }

            normalizedSceneJsonPath = NormalizeFwd(rememberedState.SceneJsonPath);
            normalizedRememberedLoadDir = NormalizeFwd(rememberedState.LoadDir);
            normalizedCurrentLoadDir = NormalizeFwd(sc.currentLoadDir != null ? sc.currentLoadDir : "");
            normalizedCurrentSaveDir = NormalizeFwd(sc.currentSaveDir != null ? sc.currentSaveDir : "");
            trackerFileExists = FileExists(sc, TrackerStateRelative);

            SuperController.LogMessage(
                string.Format(
                    "EasyMate [patch K]: mismatch - tracked scene JSON was {0}",
                    normalizedSceneJsonPath));
            SuperController.LogMessage(
                string.Format(
                    "EasyMate [patch K]: tracked folder (derived from scene path)={0}",
                    normalizedRememberedLoadDir));
            SuperController.LogMessage(
                string.Format(
                    "EasyMate [patch K]: SuperController.currentLoadDir={0}",
                    normalizedCurrentLoadDir));

            SuperController.LogMessage(
                string.Format(
                    "EasyMate [patch K]: SuperController.currentSaveDir={0}",
                    normalizedCurrentSaveDir));

            SuperController.LogMessage(
                string.Format(
                    "EasyMate [patch K]: state file ({0}) present={1}",
                    TrackerStateRelative,
                    trackerFileExists ? "true" : "false"));
        }

        private static void LogTrackerPhaseOnce(string logKey, string message)
        {
            if (string.Equals(logKey, _lastTrackerNotifyLogKey, StringComparison.Ordinal))
            {
                return;
            }

            _lastTrackerNotifyLogKey = logKey;
            SuperController.LogMessage(message);
        }

        /// <summary>
        /// Mirrors <c>pick_main_scene_json</c> in
        /// <c>Easy Mate/tools/patch_scene_initial_camera.py</c> using
        /// <see cref="SuperController.GetFilesAtPath"/>.
        /// </summary>
        private static string PickMainSceneJsonRelativeFromLoadFolder(SuperController sc, string loadDirFolderFwd)
        {
            string trimmedFolderFwd;
            string[] listedPathsRaw;
            List<string> jsonFileNamesDistinct;
            int listedIndexRaw;
            string listedPathFwd;
            string baseFileNameSegment;
            string preferredFileName;
            int preferredIndex;
            List<string> nonBadCandidates;
            int nameIndexCandidate;
            string candidateNameOriginal;
            string candidateUpper;
            string chosenNameOriginal;
            int scanIndexPrefer;
            int scanPickLongest;

            trimmedFolderFwd = NormalizeFwd(loadDirFolderFwd).TrimEnd('/');
            if (trimmedFolderFwd.Length == 0)
            {
                return "";
            }

            try
            {
                listedPathsRaw = sc.GetFilesAtPath(trimmedFolderFwd);
            }
            catch (Exception)
            {
                listedPathsRaw = null;
            }

            if (listedPathsRaw == null || listedPathsRaw.Length == 0)
            {
                return "";
            }

            jsonFileNamesDistinct = new List<string>();
            listedIndexRaw = 0;
            while (listedIndexRaw < listedPathsRaw.Length)
            {
                listedPathFwd = NormalizeFwd(listedPathsRaw[listedIndexRaw]);
                AppendJsonSceneFileNamesUnique(jsonFileNamesDistinct, listedPathFwd);
                listedIndexRaw++;
            }

            if (jsonFileNamesDistinct.Count == 0)
            {
                return "";
            }

            baseFileNameSegment = GetLastPathSegment(trimmedFolderFwd);
            preferredFileName = baseFileNameSegment + ".json";

            for (preferredIndex = 0; preferredIndex < jsonFileNamesDistinct.Count; preferredIndex++)
            {
                if (string.Equals(jsonFileNamesDistinct[preferredIndex], preferredFileName, StringComparison.Ordinal))
                {
                    return CombineFwd(trimmedFolderFwd, jsonFileNamesDistinct[preferredIndex]);
                }
            }

            for (scanIndexPrefer = 0; scanIndexPrefer < jsonFileNamesDistinct.Count; scanIndexPrefer++)
            {
                if (string.Equals(
                    jsonFileNamesDistinct[scanIndexPrefer],
                    preferredFileName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return CombineFwd(trimmedFolderFwd, jsonFileNamesDistinct[scanIndexPrefer]);
                }
            }

            nonBadCandidates = new List<string>();
            nameIndexCandidate = 0;
            while (nameIndexCandidate < jsonFileNamesDistinct.Count)
            {
                candidateNameOriginal = jsonFileNamesDistinct[nameIndexCandidate];
                candidateUpper = candidateNameOriginal.ToUpperInvariant();
                if (candidateUpper.IndexOf("COPY", StringComparison.Ordinal) >= 0)
                {
                    nameIndexCandidate++;
                    continue;
                }

                if (candidateUpper.IndexOf("COPIA", StringComparison.Ordinal) >= 0)
                {
                    nameIndexCandidate++;
                    continue;
                }

                if (candidateUpper.IndexOf("ORIGINAL", StringComparison.Ordinal) >= 0)
                {
                    nameIndexCandidate++;
                    continue;
                }

                nonBadCandidates.Add(candidateNameOriginal);
                nameIndexCandidate++;
            }

            if (nonBadCandidates.Count == 0)
            {
                nonBadCandidates.AddRange(jsonFileNamesDistinct);
            }

            chosenNameOriginal = nonBadCandidates[0];
            scanPickLongest = 1;
            while (scanPickLongest < nonBadCandidates.Count)
            {
                if (nonBadCandidates[scanPickLongest].Length > chosenNameOriginal.Length)
                {
                    chosenNameOriginal = nonBadCandidates[scanPickLongest];
                }

                scanPickLongest++;
            }

            return CombineFwd(trimmedFolderFwd, chosenNameOriginal);
        }

        private static void AppendJsonSceneFileNamesUnique(List<string> storeDistinctNames, string listedPathFwd)
        {
            int lastSlashListed;
            string fileNameListed;
            int existingIndexListed;
            string existingListedFileName;

            if (listedPathFwd == null || listedPathFwd.Length == 0)
            {
                return;
            }

            lastSlashListed = listedPathFwd.LastIndexOf('/');
            if (lastSlashListed >= 0 && lastSlashListed < listedPathFwd.Length - 1)
            {
                fileNameListed = listedPathFwd.Substring(lastSlashListed + 1);
            }
            else
            {
                fileNameListed = listedPathFwd;
            }

            if (!fileNameListed.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(fileNameListed, "meta.json", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            existingIndexListed = 0;
            while (existingIndexListed < storeDistinctNames.Count)
            {
                existingListedFileName = storeDistinctNames[existingIndexListed];
                if (string.Equals(existingListedFileName, fileNameListed, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                existingIndexListed++;
            }

            storeDistinctNames.Add(fileNameListed);
        }

        private static string GetLastPathSegment(string normalizedFwdPathNoTrail)
        {
            int slashIndexSegment;

            if (normalizedFwdPathNoTrail == null || normalizedFwdPathNoTrail.Length == 0)
            {
                return "";
            }

            slashIndexSegment = normalizedFwdPathNoTrail.LastIndexOf('/');
            if (slashIndexSegment >= 0 && slashIndexSegment < normalizedFwdPathNoTrail.Length - 1)
            {
                return normalizedFwdPathNoTrail.Substring(slashIndexSegment + 1);
            }

            return normalizedFwdPathNoTrail;
        }

        private static void RememberLoadedScene(
            SuperController sc,
            string sceneJsonPath,
            bool trustExactPathToWrite)
        {
            JSONClass trackerState;
            string normalizedSceneJsonPath;
            string normalizedLoadDir;
            string payload;

            if (sc == null)
            {
                return;
            }

            normalizedSceneJsonPath = NormalizeFwd(sceneJsonPath);
            normalizedLoadDir = GetDirectoryPath(normalizedSceneJsonPath);
            if (!IsPatchableLocalScenePath(normalizedSceneJsonPath))
            {
                return;
            }

            _lastLoadedSceneJsonPath = normalizedSceneJsonPath;
            _lastLoadedSceneLoadDir = normalizedLoadDir;

            trackerState = new JSONClass();
            trackerState["lastLoadedSceneJsonPath"] = normalizedSceneJsonPath;
            trackerState["lastLoadedSceneLoadDir"] = normalizedLoadDir;

            if (trustExactPathToWrite)
            {
                trackerState["trustExactPath"] = "true";
            }
            else
            {
                trackerState["trustExactPath"] = "false";
            }

            payload = trackerState.ToString("");

            try
            {
                sc.SaveStringIntoFile(TrackerStateRelative, payload);
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "EasyMate loaded-scene tracker: failed to persist state: " +
                    e.Message);

                return;
            }

            SuperController.LogMessage(string.Format(
                "EasyMate [scene tracker]: wrote state file ({0}); scene JSON={1} derived folder={2} trustExactPath={3}",
                TrackerStateRelative,
                normalizedSceneJsonPath,
                normalizedLoadDir,
                trustExactPathToWrite ? "true" : "false"));
        }

        private static JSONClass TryLoadTrackerDiskJson(SuperController sc)
        {
            JSONNode trackerStateNode;
            JSONClass trackerStateObject;

            if (sc == null)
            {
                return null;
            }

            if (!FileExists(sc, TrackerStateRelative))
            {
                return null;
            }

            try
            {
                trackerStateNode = sc.LoadJSON(TrackerStateRelative);
            }
            catch (Exception)
            {
                return null;
            }

            trackerStateObject = trackerStateNode != null ? trackerStateNode.AsObject : null;

            return trackerStateObject;
        }

        private static SceneState TryParseTrackerDiskSceneState(JSONClass trackerStateObject)
        {
            string sceneJsonPath;
            string loadDir;
            JSONNode trustNode;
            bool trustExactPath;

            if (trackerStateObject == null)
            {
                return null;
            }

            sceneJsonPath = trackerStateObject["lastLoadedSceneJsonPath"] != null
                ? trackerStateObject["lastLoadedSceneJsonPath"].Value
                : "";
            loadDir = trackerStateObject["lastLoadedSceneLoadDir"] != null
                ? trackerStateObject["lastLoadedSceneLoadDir"].Value
                : "";

            if (!IsPatchableLocalScenePath(sceneJsonPath))
            {
                return null;
            }

            trustNode = trackerStateObject["trustExactPath"];
            trustExactPath = trustNode != null &&
                string.Equals(trustNode.Value, "true", StringComparison.OrdinalIgnoreCase);

            return new SceneState(
                NormalizeFwd(sceneJsonPath),
                NormalizeFwd(loadDir),
                trustExactPath);
        }

        private static SceneState TryPeekTrackerStateFromDisk(SuperController sc)
        {
            JSONClass trackerStateObject;

            trackerStateObject = TryLoadTrackerDiskJson(sc);
            if (trackerStateObject == null)
            {
                return null;
            }

            return TryParseTrackerDiskSceneState(trackerStateObject);
        }

        private static SceneState GetRememberedState(SuperController sc)
        {
            JSONClass trackerStateObject;
            SceneState parsedDiskState;

            if (IsPatchableLocalScenePath(_lastLoadedSceneJsonPath))
            {
                return new SceneState(
                    NormalizeFwd(_lastLoadedSceneJsonPath),
                    NormalizeFwd(_lastLoadedSceneLoadDir),
                    false);
            }

            if (sc == null)
            {
                return null;
            }

            trackerStateObject = TryLoadTrackerDiskJson(sc);
            if (trackerStateObject == null)
            {
                return null;
            }

            parsedDiskState = TryParseTrackerDiskSceneState(trackerStateObject);
            if (parsedDiskState == null || !parsedDiskState.IsValid)
            {
                return null;
            }

            _lastLoadedSceneJsonPath = NormalizeFwd(parsedDiskState.SceneJsonPath);
            _lastLoadedSceneLoadDir = NormalizeFwd(parsedDiskState.LoadDir);

            return new SceneState(_lastLoadedSceneJsonPath, _lastLoadedSceneLoadDir, false);
        }

        private static bool IsPatchableLocalScenePath(string path)
        {
            string normalizedPath;
            int packageSeparatorIndex;

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            normalizedPath = NormalizeFwd(path);
            if (!normalizedPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            packageSeparatorIndex = normalizedPath.IndexOf(":/", StringComparison.Ordinal);
            if (packageSeparatorIndex > 1)
            {
                return false;
            }

            return true;
        }

        private static bool FileExists(SuperController sc, string relativePath)
        {
            string normalizedPath;
            int lastSlashIndex;
            string parentFolderPath;
            string fileName;
            string[] foundPaths;
            int pathIndex;
            string normalizedFoundPath;

            if (sc == null || string.IsNullOrEmpty(relativePath))
            {
                return false;
            }

            normalizedPath = NormalizeFwd(relativePath);
            lastSlashIndex = normalizedPath.LastIndexOf('/');
            if (lastSlashIndex <= 0 || lastSlashIndex >= normalizedPath.Length - 1)
            {
                return false;
            }

            parentFolderPath = normalizedPath.Substring(0, lastSlashIndex);
            fileName = normalizedPath.Substring(lastSlashIndex + 1);

            try
            {
                foundPaths = sc.GetFilesAtPath(parentFolderPath);
            }
            catch (Exception)
            {
                return false;
            }

            if (foundPaths == null)
            {
                return false;
            }

            for (pathIndex = 0; pathIndex < foundPaths.Length; pathIndex++)
            {
                normalizedFoundPath = NormalizeFwd(foundPaths[pathIndex]);
                if (normalizedFoundPath.EndsWith(
                    "/" + fileName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAbsolutePath(string path)
        {
            string normalizedPath;

            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            normalizedPath = NormalizeFwd(path);
            if (normalizedPath.StartsWith("//", StringComparison.Ordinal))
            {
                return true;
            }

            return normalizedPath.Length > 2 &&
                char.IsLetter(normalizedPath[0]) &&
                normalizedPath[1] == ':' &&
                normalizedPath[2] == '/';
        }

        private static string GetDirectoryPath(string path)
        {
            string normalizedPath;
            int lastSlashIndex;

            normalizedPath = NormalizeFwd(path);
            lastSlashIndex = normalizedPath.LastIndexOf('/');
            if (lastSlashIndex <= 0)
            {
                return "";
            }

            return normalizedPath.Substring(0, lastSlashIndex);
        }

        private static string NormalizeFwd(string path)
        {
            if (path == null)
            {
                return "";
            }

            return path.Replace('\\', '/');
        }

        private static string CombineFwd(string root, string relativePath)
        {
            string normalizedRoot;
            string normalizedRelativePath;

            normalizedRoot = NormalizeFwd(root).TrimEnd('/');
            normalizedRelativePath = NormalizeFwd(relativePath).TrimStart('/');
            if (normalizedRoot.Length == 0)
            {
                return normalizedRelativePath;
            }

            if (normalizedRelativePath.Length == 0)
            {
                return normalizedRoot;
            }

            return normalizedRoot + "/" + normalizedRelativePath;
        }

        private class SceneState
        {
            public readonly string SceneJsonPath;

            public readonly string LoadDir;

            public readonly bool TrustExactPath;

            public bool IsValid
            {
                get
                {
                    return SceneJsonPath != null && SceneJsonPath.Length > 0;
                }
            }

            public SceneState(string sceneJsonPath, string loadDir)
                : this(sceneJsonPath, loadDir, false)
            {
            }

            public SceneState(string sceneJsonPath, string loadDir, bool trustExactPath)
            {
                SceneJsonPath = sceneJsonPath;
                LoadDir = loadDir;
                TrustExactPath = trustExactPath;
            }
        }
    }
}
