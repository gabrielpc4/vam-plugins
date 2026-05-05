using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace geesp0t
{
    /// <summary>
    /// Session plugin: when the loaded scene path matches configurable filters (defaults: folder contains &quot;jessika bdroom&quot;; scene substring optional because VaM often exposes only the folder path),
    /// merges <see cref="LocalMp4Viewer"/> onto the DreamStreetBedroom TV FreeController or the laptop ImagePanel atom.
    /// Loaded via <c>Load_Session_Plugins.cs</c> alongside other AutoMate session plugins.
    /// </summary>
    public class IntroSceneLocalMp4Bootstrap : MVRScript
    {
        private const string LocalMp4ViewerCslistPath = "Custom/Scripts/LocalMp4Viewer/LocalMp4Viewer.cslist";

        private const string TargetViewerFileName = "LocalMp4Viewer.cslist";

        private bool wasSuperControllerLoading;

        private string lastMergedSceneSignature;

        public JSONStorableBool enableAutoAttach;

        public JSONStorableString loadDirMustContain;

        public JSONStorableString scenePathMustContain;

        public JSONStorableStringChooser attachScreenChooser;

        public JSONStorableString televisionHostAtomUid;

        public JSONStorableString televisionFreeControllerId;

        public JSONStorableString laptopHostAtomUid;

        public JSONStorableString laptopFreeControllerId;

        public JSONStorableString optionalVideoRelativePath;

        public JSONStorableBool applyBuiltInQuadPreset;

        public JSONStorableBool autoPlayWhenAttached;

        public override void Init()
        {
            enableAutoAttach = new JSONStorableBool("Enable intro LocalMp4 attach", true);
            RegisterBool(enableAutoAttach);

            loadDirMustContain = new JSONStorableString(
                "Load dir must contain",
                "jessika bdroom");
            RegisterString(loadDirMustContain);

            scenePathMustContain = new JSONStorableString(
                "Scene path must contain (optional)",
                "");
            RegisterString(scenePathMustContain);

            List<string> screenChoices = new List<string>();

            screenChoices.Add("Television");
            screenChoices.Add("Laptop");

            attachScreenChooser = new JSONStorableStringChooser(
                "Attach LocalMp4Viewer to",
                screenChoices,
                "Television",
                "Attach LocalMp4Viewer to");
            RegisterStringChooser(attachScreenChooser);

            televisionHostAtomUid = new JSONStorableString("TV host atom uid", "DreamStreetBedroom");
            RegisterString(televisionHostAtomUid);

            televisionFreeControllerId = new JSONStorableString("TV FreeController id", "DSBR_TelevisionControl");
            RegisterString(televisionFreeControllerId);

            laptopHostAtomUid = new JSONStorableString("Laptop host atom uid", "ImagePanel#2");
            RegisterString(laptopHostAtomUid);

            laptopFreeControllerId = new JSONStorableString("Laptop FreeController id", "control");
            RegisterString(laptopFreeControllerId);

            optionalVideoRelativePath = new JSONStorableString(
                "Optional video path (relative to VaM)",
                "");
            RegisterString(optionalVideoRelativePath);

            applyBuiltInQuadPreset = new JSONStorableBool("Apply built-in quad preset for target", true);
            RegisterBool(applyBuiltInQuadPreset);

            autoPlayWhenAttached = new JSONStorableBool("Auto-play when plugin first attaches", false);
            RegisterBool(autoPlayWhenAttached);
        }

        private void Update()
        {
            SuperController superController = SuperController.singleton;

            if (superController == null)
            {
                return;
            }

            bool loadingNow = superController.isLoading;

            if (loadingNow)
            {
                wasSuperControllerLoading = true;
                return;
            }

            if (wasSuperControllerLoading)
            {
                wasSuperControllerLoading = false;
                lastMergedSceneSignature = "";
            }

            if (!enableAutoAttach.val)
            {
                return;
            }

            if (!CurrentSceneMatchesFilters(superController))
            {
                return;
            }

            string sceneSignature = BuildSceneSignature(superController);

            if (sceneSignature == lastMergedSceneSignature)
            {
                return;
            }

            lastMergedSceneSignature = sceneSignature;

            TryMergeLocalMp4ViewerOntoIntroScreenHost();
        }

        private static string BuildSceneSignature(SuperController superController)
        {
            string saveDir = superController.currentSaveDir ?? "";
            string loadDir = superController.currentLoadDir ?? "";

            return saveDir.Replace('\\', '/') + "|" + loadDir.Replace('\\', '/');
        }

        private bool CurrentSceneMatchesFilters(SuperController superController)
        {
            string haystack = (superController.currentSaveDir + "/" + superController.currentLoadDir).Replace('\\', '/');

            string dirNeedle = loadDirMustContain.val != null ? loadDirMustContain.val.Trim() : "";

            if (dirNeedle.Length > 0)
            {
                if (haystack.IndexOf(dirNeedle, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            string sceneNeedle = scenePathMustContain.val != null ? scenePathMustContain.val.Trim() : "";

            if (sceneNeedle.Length > 0)
            {
                if (haystack.IndexOf(sceneNeedle, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void TryMergeLocalMp4ViewerOntoIntroScreenHost()
        {
            LocalMp4ViewerBootstrapHints.ClearAll();

            string hostAtomUid;
            string parentFreeControllerIdValue;

            ResolveHostAtomAndParentFreeController(out hostAtomUid, out parentFreeControllerIdValue);

            if (string.IsNullOrEmpty(hostAtomUid))
            {
                SuperController.LogError("[IntroSceneLocalMp4Bootstrap] Host atom uid is empty.");
                LocalMp4ViewerBootstrapHints.ClearAll();

                return;
            }

            Atom hostAtom = SuperController.singleton.GetAtomByUid(hostAtomUid);

            if (hostAtom == null)
            {
                SuperController.LogError(
                    "[IntroSceneLocalMp4Bootstrap] Atom not found: \"" + hostAtomUid +
                    "\". Adjust TV/Laptop host uid storables or load the expected scene.");

                LocalMp4ViewerBootstrapHints.ClearAll();

                return;
            }

            MVRPluginManager pluginManager = hostAtom.GetStorableByID("PluginManager") as MVRPluginManager;

            if (pluginManager == null)
            {
                SuperController.LogError(
                    "[IntroSceneLocalMp4Bootstrap] Atom \"" + hostAtomUid +
                    "\" has no PluginManager; VaM cannot host plugins on this atom.");

                LocalMp4ViewerBootstrapHints.ClearAll();

                return;
            }

            List<string> existingPluginPaths;

            TryCollectNormalizedPluginPaths(pluginManager, out existingPluginPaths);

            if (PluginListAlreadyContainsLocalMp4Viewer(existingPluginPaths))
            {
                LocalMp4ViewerBootstrapHints.ClearAll();

                return;
            }

            ApplyHintsBeforeMerge(parentFreeControllerIdValue);

            existingPluginPaths.Add(LocalMp4ViewerCslistPath);

            JSONClass pluginManagerJson = CreatePluginJson(existingPluginPaths.ToArray());

            try
            {
                pluginManager.LateRestoreFromJSON(pluginManagerJson);

                SuperController.LogMessage(
                    "[IntroSceneLocalMp4Bootstrap] Added LocalMp4Viewer to \"" + hostAtomUid + "\" (" + attachScreenChooser.val + ").");
            }
            catch (Exception mergeException)
            {
                SuperController.LogError("[IntroSceneLocalMp4Bootstrap] LateRestoreFromJSON failed: " + mergeException);
                LocalMp4ViewerBootstrapHints.ClearAll();
            }
        }

        private void ResolveHostAtomAndParentFreeController(out string hostUid, out string parentFcId)
        {
            string choice = attachScreenChooser.val != null ? attachScreenChooser.val : "Television";

            if (string.Equals(choice, "Laptop", StringComparison.OrdinalIgnoreCase))
            {
                hostUid = laptopHostAtomUid.val != null ? laptopHostAtomUid.val.Trim() : "";
                parentFcId = laptopFreeControllerId.val != null ? laptopFreeControllerId.val.Trim() : "";
            }
            else
            {
                hostUid = televisionHostAtomUid.val != null ? televisionHostAtomUid.val.Trim() : "";
                parentFcId = televisionFreeControllerId.val != null ? televisionFreeControllerId.val.Trim() : "";
            }
        }

        private void ApplyHintsBeforeMerge(string parentFreeControllerIdValue)
        {
            if (!string.IsNullOrEmpty(parentFreeControllerIdValue))
            {
                LocalMp4ViewerBootstrapHints.ParentFreeControllerId = parentFreeControllerIdValue;
            }

            string videoHint = optionalVideoRelativePath.val != null ? optionalVideoRelativePath.val.Trim() : "";

            if (videoHint.Length > 0)
            {
                LocalMp4ViewerBootstrapHints.VideoRelativePath = videoHint;
            }

            if (applyBuiltInQuadPreset.val)
            {
                string choice = attachScreenChooser.val != null ? attachScreenChooser.val : "Television";

                if (string.Equals(choice, "Laptop", StringComparison.OrdinalIgnoreCase))
                {
                    LocalMp4ViewerBootstrapHints.ApplyLaptopQuadPreset = true;
                }
                else
                {
                    LocalMp4ViewerBootstrapHints.ApplyTelevisionQuadPreset = true;
                }
            }

            if (autoPlayWhenAttached.val)
            {
                LocalMp4ViewerBootstrapHints.ShouldAutoPlayAfterLoad = true;
            }
        }

        private static bool PluginListAlreadyContainsLocalMp4Viewer(List<string> pluginPaths)
        {
            int pathCount = pluginPaths.Count;

            for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
            {
                string pathEntry = pluginPaths[pathIndex];

                if (pathEntry == null)
                {
                    continue;
                }

                string fileName = GetFileName(pathEntry);

                if (fileName.Equals(TargetViewerFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void TryCollectNormalizedPluginPaths(MVRPluginManager pluginManager, out List<string> pathsOut)
        {
            pathsOut = new List<string>();

            JSONClass currentJson = pluginManager.GetJSON(true, true, true);

            if (currentJson == null || currentJson["plugins"] == null)
            {
                return;
            }

            foreach (JSONNode pluginNode in currentJson["plugins"].Childs)
            {
                string rawPath = pluginNode.Value;

                if (string.IsNullOrEmpty(rawPath))
                {
                    continue;
                }

                string normalizedPath = NormalizePluginPath(rawPath);

                if (string.IsNullOrEmpty(normalizedPath))
                {
                    continue;
                }

                pathsOut.Add(normalizedPath);
            }
        }

        private string NormalizePluginPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            SuperController superController = SuperController.singleton;

            if (superController == null)
            {
                return path;
            }

            if (path.StartsWith("./"))
            {
                path = superController.currentSaveDir + "/" + path.Substring(2);
            }

            path = path.Replace('\\', '/');

            int folderSeparatorIndex = path.LastIndexOf("/");

            if (folderSeparatorIndex < 0)
            {
                path = superController.currentSaveDir + "/" + path;
                folderSeparatorIndex = path.LastIndexOf("/");
            }

            if (folderSeparatorIndex > 0 && folderSeparatorIndex < path.Length - 1)
            {
                if (!FileExistsQuiet(path))
                {
                    string scriptInStandardFolder = "Custom/Scripts/" + GetFileName(path);

                    if (FileExistsQuiet(scriptInStandardFolder))
                    {
                        path = scriptInStandardFolder;
                    }
                }
            }

            return path;
        }

        private static bool FileExistsQuiet(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || SuperController.singleton == null)
            {
                return false;
            }

            int folderSeparatorIndex = relativePath.LastIndexOfAny(new char[] { '/', '\\' });

            if (folderSeparatorIndex <= 0)
            {
                return false;
            }

            string pathFolder = relativePath.Substring(0, folderSeparatorIndex);
            string pathFile = relativePath.Substring(folderSeparatorIndex + 1);

            if (string.IsNullOrEmpty(pathFolder) || string.IsNullOrEmpty(pathFile))
            {
                return false;
            }

            string[] pathFileList = null;

            try
            {
                pathFileList = SuperController.singleton.GetFilesAtPath(pathFolder);
            }
            catch (Exception)
            {
                return false;
            }

            if (pathFileList == null || pathFileList.Length == 0)
            {
                return false;
            }

            foreach (string foundPathFile in pathFileList)
            {
                string correctedPathFile = foundPathFile.Replace("\\", "/");

                if (correctedPathFile.EndsWith("/" + pathFile))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetFileName(string relativePath)
        {
            int slashIndex = relativePath.LastIndexOfAny(new char[] { '/', '\\' });

            if (slashIndex < 0 || slashIndex >= relativePath.Length - 1)
            {
                return relativePath;
            }

            return relativePath.Substring(slashIndex + 1);
        }

        private static JSONClass CreatePluginJson(string[] pluginPathList)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            stringBuilder.Append("{");
            stringBuilder.Append(" \"id\": \"" + "PluginManager" + "\",");
            stringBuilder.Append(" \"plugins\": " + "{");

            int pluginCount = pluginPathList.Length;

            for (int pluginIndex = 0; pluginIndex < pluginCount; pluginIndex++)
            {
                string comma = (pluginIndex + 1) < pluginCount ? "," : "";

                stringBuilder.Append(string.Format(
                    "    \"plugin#{0}\": \"{1}\"{2}",
                    pluginIndex.ToString(),
                    pluginPathList[pluginIndex],
                    comma));
            }

            stringBuilder.Append("  }");
            stringBuilder.Append("}");

            return JSONNode.Parse(stringBuilder.ToString()).AsObject;
        }
    }
}
