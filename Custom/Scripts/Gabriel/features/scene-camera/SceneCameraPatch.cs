using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// <b>K</b> hotkey: captures navigation / monitor / height / WindowCamera /
    /// center-eye <c>[CameraRig]</c> rotation (euler Z stripped to 0), writes
    /// <c>tools/last_scene_camera_patch_request.json</c>.
    /// If <see cref="SuperController.currentLoadDir"/> contains exactly one non-meta
    /// <c>.json</c>, runs <c>tools/patch_scene_initial_camera.py</c> automatically
    /// (drops root <c>playerNavCollider</c> so tilt survives VR reload).
    /// If there are none or multiple, logs an error and the full camera snapshot so you can run
    /// <c>tools/patch_scene_camera_manual_cli.py</c> with the scene JSON you choose and the same request file.
    /// </summary>
    public static class SceneCameraPatch
    {
        public const string PatchScriptRelative = "Custom/Scripts/Gabriel/tools/scene-camera/patch_scene_initial_camera.py";

        /// <summary>Written on every successful K diagnostics pass (automatic or manual follow-up).</summary>
        public const string RequestJsonRelative = "Custom/Scripts/Gabriel/tools/scene-camera/last_scene_camera_patch_request.json";

        /// <summary>
        /// Optional manual wrapper with friendlier CLI help (same argv as <see cref="PatchScriptRelative"/>).
        /// </summary>
        public const string ManualCliScriptRelative = "Custom/Scripts/Gabriel/tools/scene-camera/patch_scene_camera_manual_cli.py";

        /// <summary>
        /// Append-only log next to <see cref="PatchScriptRelative"/>.
        /// </summary>
        public const string PatchToolLogRelative = "Custom/Scripts/Gabriel/tools/scene-camera/last_scene_camera_patch_log.txt";

        public static void TryRunFromHotkey()
        {
            SuperController sceneControllerEarly;
            string loadDirectoryFwdTrimmedNormalized;
            List<string> jsonBasenamesSortedDistinct;
            string singleSceneRelativePathChosenFwdNormalized;
            string classifySummaryLine;
            JSONClass requestPayloadRoot;
            string requestPayloadText;
            string installRootFwd;
            string sceneJsonAbsolutePathFwd;
            string scriptAbsolutePathFwd;
            StringBuilder processArgumentsBuilderWide;
            string pythonJoinedArgumentsWide;
            Process launchedPythonProcessWide;
            string pythonLaunchAttemptsSummaryWide;
            bool finishedWaitingUpToDeadlineCaptureWideLate;
            int exitStatusFromPythonInterpreterWideLate;

            sceneControllerEarly = SuperController.singleton;
            if (sceneControllerEarly == null)
            {
                SuperController.LogError("GabrielHud [key K]: SuperController.singleton is null.");
                return;
            }

            loadDirectoryFwdTrimmedNormalized = NormalizeFwd(
                sceneControllerEarly.currentLoadDir != null ? sceneControllerEarly.currentLoadDir : "").TrimEnd('/');
            if (loadDirectoryFwdTrimmedNormalized.Length == 0)
            {
                SuperController.LogError("GabrielHud [key K]: SuperController.currentLoadDir is empty; cannot infer scene folder.");
                return;
            }

            jsonBasenamesSortedDistinct =
                CollectSortedDistinctImmediateSceneJsonBasenamesUnderFolder(
                    sceneControllerEarly,
                    loadDirectoryFwdTrimmedNormalized);
            if (jsonBasenamesSortedDistinct == null)
            {
                SuperController.LogError(string.Format(
                    "GabrielHud [key K]: could not enumerate .json scene files under {0}.",
                    loadDirectoryFwdTrimmedNormalized));
                return;
            }

            requestPayloadRoot = BuildPatchRequest(sceneControllerEarly);

            classifySummaryLine = ClassifyLoadFolderExclusiveJsonSelection(
                loadDirectoryFwdTrimmedNormalized,
                jsonBasenamesSortedDistinct,
                out singleSceneRelativePathChosenFwdNormalized);

            try
            {
                requestPayloadText = requestPayloadRoot.ToString("");
                sceneControllerEarly.SaveStringIntoFile(RequestJsonRelative, requestPayloadText);
            }
            catch (Exception requestWriteFailureExceptionCaptured)
            {
                SuperController.LogError(
                    "GabrielHud [key K]: failed to write request JSON: " + requestWriteFailureExceptionCaptured.Message);
                return;
            }

            installRootFwd = GetVaMInstallRoot();
            EmitDiagnosticsForKeyK(sceneControllerEarly, singleSceneRelativePathChosenFwdNormalized, classifySummaryLine);

            SuperController.LogMessage("GabrielHud [key K]: patch request JSON saved to " + RequestJsonRelative);
            SuperController.LogMessage(
                string.Format(
                    "GabrielHud [key K]: open load folder snapshot — {0} (distinct non-meta scene .json immediate children={1})",
                    loadDirectoryFwdTrimmedNormalized,
                    jsonBasenamesSortedDistinct.Count));
            EmitCandidateSceneJsonListingToLog(loadDirectoryFwdTrimmedNormalized, jsonBasenamesSortedDistinct, installRootFwd);

            if (singleSceneRelativePathChosenFwdNormalized.Length == 0)
            {
                EmitManualCliEscalationToLog(sceneControllerEarly, classifySummaryLine, installRootFwd, requestPayloadRoot);
                return;
            }

            SuperController.LogMessage("GabrielHud [key K]: auto patch target scene json=" + singleSceneRelativePathChosenFwdNormalized);

            sceneJsonAbsolutePathFwd = CombineFwd(installRootFwd, singleSceneRelativePathChosenFwdNormalized);
            scriptAbsolutePathFwd = CombineFwd(installRootFwd, PatchScriptRelative);

            processArgumentsBuilderWide = new StringBuilder();
            processArgumentsBuilderWide.Append("-u \"");
            processArgumentsBuilderWide.Append(scriptAbsolutePathFwd);
            processArgumentsBuilderWide.Append("\" \"");
            processArgumentsBuilderWide.Append(sceneJsonAbsolutePathFwd);
            processArgumentsBuilderWide.Append("\" \"");
            processArgumentsBuilderWide.Append(CombineFwd(installRootFwd, RequestJsonRelative));
            processArgumentsBuilderWide.Append("\"");

            pythonJoinedArgumentsWide = processArgumentsBuilderWide.ToString();
            launchedPythonProcessWide = null;
            pythonLaunchAttemptsSummaryWide = "";
            TryLaunchPythonInterpreterWithArgumentsSnippet(
                pythonJoinedArgumentsWide,
                out launchedPythonProcessWide,
                out pythonLaunchAttemptsSummaryWide);

            if (launchedPythonProcessWide == null)
            {
                SuperController.LogError(
                    "GabrielHud [key K]: could not start Python (tried: " + pythonLaunchAttemptsSummaryWide + "). Install Python 3 or add it to PATH.");
                EmitManualCliEscalationToLog(sceneControllerEarly, "Python launch failed.", installRootFwd, requestPayloadRoot);
                return;
            }

            finishedWaitingUpToDeadlineCaptureWideLate = launchedPythonProcessWide.WaitForExit(180000);
            if (!finishedWaitingUpToDeadlineCaptureWideLate)
            {
                try
                {
                    launchedPythonProcessWide.Kill();
                }
                catch
                {
                }

                SuperController.LogError("GabrielHud [key K]: python patch timed out (180s). See " + PatchToolLogRelative);
                return;
            }

            exitStatusFromPythonInterpreterWideLate = launchedPythonProcessWide.ExitCode;
            if (exitStatusFromPythonInterpreterWideLate != 0)
            {
                SuperController.LogError(string.Format(
                    "GabrielHud [key K]: python exit {0}. Details: {1}",
                    exitStatusFromPythonInterpreterWideLate,
                    PatchToolLogRelative));
            }
            else
            {
                SuperController.LogMessage(string.Format(
                    "GabrielHud [key K]: python exit 0. Details: {0}",
                    PatchToolLogRelative));
                SuperController.LogMessage(
                    "GabrielHud [key K]: patch removed root playerNavCollider if present — " +
                    "VaM otherwise forces rig orientation to physical floor each frame; " +
                    "re-bind Floor on Player Navigation panel if you want that again.");
            }
        }

        private static void EmitManualCliEscalationToLog(
            SuperController sceneControllerCaptured,
            string situationSummaryLineCaptured,
            string installRootCapturedFwdNormalized,
            JSONClass serializedRequestCaptured)
        {
            string manualScriptAbsFwdCaptured;
            string requestAbsCapturedFwdCaptured;
            string requestCompactOneLineCaptured;

            SuperController.LogError(
                string.Format(
                    "GabrielHud [key K]: automatic patch aborted — {0}. Use the CLI below with YOUR chosen scene .json.",
                    situationSummaryLineCaptured));

            EmitPoseSnapshotLinesFromControllers(sceneControllerCaptured);

            manualScriptAbsFwdCaptured = CombineFwd(installRootCapturedFwdNormalized, ManualCliScriptRelative);
            requestAbsCapturedFwdCaptured = CombineFwd(installRootCapturedFwdNormalized, RequestJsonRelative);
            SuperController.LogError(
                string.Format(
                    "GabrielHud [key K]: manual CLI (pick scene path yourself): python \"{0}\" \"ABS_PATH_SCENE.json\" \"{1}\"",
                    manualScriptAbsFwdCaptured,
                    requestAbsCapturedFwdCaptured));

            try
            {
                requestCompactOneLineCaptured = serializedRequestCaptured.ToString("");
            }
            catch (Exception serializeRequestFailureForLogCaptured)
            {
                requestCompactOneLineCaptured =
                    "(could not stringify request payload: " + serializeRequestFailureForLogCaptured.Message + ")";
            }

            SuperController.LogError(
                "GabrielHud [key K]: request payload JSON (paste into file or compare): " +
                requestCompactOneLineCaptured);
        }

        private static void EmitDiagnosticsForKeyK(
            SuperController sceneControllerCaptured,
            string chosenSceneRelativeFwdOrEmptyCaptured,
            string classificationLineCapturedCaptured)
        {
            SuperController.LogMessage("GabrielHud [key K]: folder classification — " + classificationLineCapturedCaptured);
            if (chosenSceneRelativeFwdOrEmptyCaptured.Length > 0)
            {
                SuperController.LogMessage(
                    string.Format(
                        "GabrielHud [key K]: auto-selected exclusive scene JSON under currentLoadDir → {0}",
                        chosenSceneRelativeFwdOrEmptyCaptured));
            }

            EmitPoseSnapshotLinesFromControllers(sceneControllerCaptured);
            SuperController.LogMessage("GabrielHud [key K]: playerHeightAdjust=" + sceneControllerCaptured.playerHeightAdjust.ToString("G9"));
        }

        private static string ClassifyLoadFolderExclusiveJsonSelection(
            string loadFolderFwdTrimmedNoTrailingCaptured,
            List<string> basenamesAscendingSortedCaptured,
            out string exclusiveRelativeCombinedPathChosenFwdCaptured)
        {
            int distinctCountCaptured;

            distinctCountCaptured = basenamesAscendingSortedCaptured != null ? basenamesAscendingSortedCaptured.Count : 0;
            exclusiveRelativeCombinedPathChosenFwdCaptured = "";

            if (distinctCountCaptured == 0)
            {
                return "no qualifying .json in this folder (excluding meta.json; immediate children only)";
            }

            if (distinctCountCaptured == 1)
            {
                exclusiveRelativeCombinedPathChosenFwdCaptured =
                    NormalizeFwd(
                        CombineFwd(loadFolderFwdTrimmedNoTrailingCaptured, basenamesAscendingSortedCaptured[0]));
                return "exactly one scene .json candidate — auto patch permitted";
            }

            return string.Format(
                "{0} scene .json candidates in this folder — choose one manually via CLI",
                distinctCountCaptured);
        }

        private static void EmitCandidateSceneJsonListingToLog(
            string loadFolderRelativeFwdCaptured,
            List<string> basenamesAscendingCaptured,
            string installRootCapturedFwdCaptured)
        {
            int walkIndexEmitted;
            string combinedRelativeEmitted;
            string absoluteEmittedCaptured;

            if (basenamesAscendingCaptured == null || basenamesAscendingCaptured.Count == 0)
            {
                return;
            }

            walkIndexEmitted = 0;
            while (walkIndexEmitted < basenamesAscendingCaptured.Count)
            {
                combinedRelativeEmitted =
                    NormalizeFwd(CombineFwd(loadFolderRelativeFwdCaptured, basenamesAscendingCaptured[walkIndexEmitted]));
                absoluteEmittedCaptured =
                    NormalizeFwd(
                        CombineFwd(installRootCapturedFwdCaptured, combinedRelativeEmitted));
                SuperController.LogMessage(string.Format(
                    "GabrielHud [key K]:   candidate [{0}/{1}] {2}",
                    walkIndexEmitted + 1,
                    basenamesAscendingCaptured.Count,
                    absoluteEmittedCaptured));
                walkIndexEmitted++;
            }
        }

        private static List<string> CollectSortedDistinctImmediateSceneJsonBasenamesUnderFolder(
            SuperController sceneControllerUsedCaptured,
            string loadFolderFwdTrimmedNoTrailingSlashCaptured)
        {
            string[] rawListedPathsFromVaMCapturedWide;
            int rawPathIndexCapturedWideScan;
            string normalizedListedPathCapturedWideFwd;
            List<string> workingDistinctBasenamesCaptured;
            HashSetUppercaseKeyTracker distinctInsensitiveTrackerWide;

            if (sceneControllerUsedCaptured == null)
            {
                return null;
            }

            rawListedPathsFromVaMCapturedWide = null;
            try
            {
                rawListedPathsFromVaMCapturedWide =
                    sceneControllerUsedCaptured.GetFilesAtPath(loadFolderFwdTrimmedNoTrailingSlashCaptured);
            }
            catch (Exception)
            {
                return null;
            }

            if (rawListedPathsFromVaMCapturedWide == null)
            {
                return null;
            }

            workingDistinctBasenamesCaptured = new List<string>();
            distinctInsensitiveTrackerWide = new HashSetUppercaseKeyTracker();
            rawPathIndexCapturedWideScan = 0;
            while (rawPathIndexCapturedWideScan < rawListedPathsFromVaMCapturedWide.Length)
            {
                normalizedListedPathCapturedWideFwd =
                    NormalizeFwd(rawListedPathsFromVaMCapturedWide[rawPathIndexCapturedWideScan]);
                if (!IsListedFilePathImmediateChildOfFolder(
                        normalizedListedPathCapturedWideFwd,
                        loadFolderFwdTrimmedNoTrailingSlashCaptured))
                {
                    rawPathIndexCapturedWideScan++;
                    continue;
                }

                TryAppendSceneJsonLeafFilenameDistinct(
                    workingDistinctBasenamesCaptured,
                    distinctInsensitiveTrackerWide,
                    normalizedListedPathCapturedWideFwd);
                rawPathIndexCapturedWideScan++;
            }

            workingDistinctBasenamesCaptured.Sort(StringComparer.OrdinalIgnoreCase);
            return workingDistinctBasenamesCaptured;
        }

        private sealed class HashSetUppercaseKeyTracker
        {
            private readonly HashSet<string> _uppercaseFingerprintsCollected = new HashSet<string>();

            public bool TryRegisterNewInsensitive(string basenameOriginalCaseCapturedWide)
            {
                string fingerprintUpperCapturedWide;

                if (basenameOriginalCaseCapturedWide == null || basenameOriginalCaseCapturedWide.Length == 0)
                {
                    return false;
                }

                fingerprintUpperCapturedWide =
                    basenameOriginalCaseCapturedWide.ToUpperInvariant();

                if (_uppercaseFingerprintsCollected.Contains(fingerprintUpperCapturedWide))
                {
                    return false;
                }

                _uppercaseFingerprintsCollected.Add(fingerprintUpperCapturedWide);
                return true;
            }
        }

        private static void TryAppendSceneJsonLeafFilenameDistinct(
            List<string> destinationBasenamesCollectedWide,
            HashSetUppercaseKeyTracker insensitiveRegistryCapturedWide,
            string listedListedPathFwdCapturedWideNormalized)
        {
            int lastSlashCapturedWideEarly;
            string leafFileNameCapturedWide;

            lastSlashCapturedWideEarly = listedListedPathFwdCapturedWideNormalized.LastIndexOf('/');
            if (lastSlashCapturedWideEarly < 0 || lastSlashCapturedWideEarly >= listedListedPathFwdCapturedWideNormalized.Length - 1)
            {
                return;
            }

            leafFileNameCapturedWide =
                listedListedPathFwdCapturedWideNormalized.Substring(lastSlashCapturedWideEarly + 1);

            if (leafFileNameCapturedWide.Length == 0 || !leafFileNameCapturedWide.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(leafFileNameCapturedWide, "meta.json", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!insensitiveRegistryCapturedWide.TryRegisterNewInsensitive(leafFileNameCapturedWide))
            {
                return;
            }

            destinationBasenamesCollectedWide.Add(leafFileNameCapturedWide);
        }

        private static bool IsListedFilePathImmediateChildOfFolder(
            string listedFileEntryPathFwdCapturedWide,
            string expectedParentFolderFwdTrimmedNoTrailCapturedWide)
        {
            string parentFolderFwdComputedCapturedWide;
            string expectedNormalizedWideCapturedWide;

            expectedNormalizedWideCapturedWide =
                NormalizeFwd(expectedParentFolderFwdTrimmedNoTrailCapturedWide).TrimEnd('/');
            if (expectedNormalizedWideCapturedWide.Length == 0)
            {
                return false;
            }

            parentFolderFwdComputedCapturedWide =
                NormalizeFwd(GetDirectoryPathOfRelativeFwd(listedFileEntryPathFwdCapturedWide));

            return string.Equals(
                parentFolderFwdComputedCapturedWide,
                expectedNormalizedWideCapturedWide,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDirectoryPathOfRelativeFwd(string fwdPathCapturedWideNormalized)
        {
            int lastSlashCapturedWideLocate;

            if (fwdPathCapturedWideNormalized == null || fwdPathCapturedWideNormalized.Length == 0)
            {
                return "";
            }

            lastSlashCapturedWideLocate = fwdPathCapturedWideNormalized.LastIndexOf('/');
            if (lastSlashCapturedWideLocate <= 0)
            {
                return "";
            }

            return fwdPathCapturedWideNormalized.Substring(0, lastSlashCapturedWideLocate);
        }

        private static void EmitPoseSnapshotLinesFromControllers(SuperController sc)
        {
            if (sc.MonitorCenterCamera != null)
            {
                Vector3 eulerMonitorCapturedWide;
                eulerMonitorCapturedWide =
                    sc.MonitorCenterCamera.transform.localEulerAngles;
                SuperController.LogMessage(string.Format(
                    "GabrielHud [key K]: monitorCameraRotation (local euler)=({0:F4},{1:F4},{2:F4})",
                    eulerMonitorCapturedWide.x,
                    eulerMonitorCapturedWide.y,
                    eulerMonitorCapturedWide.z));
            }
            else
            {
                SuperController.LogMessage("GabrielHud [key K]: MonitorCenterCamera is null");
            }

            if (sc.navigationRig != null)
            {
                Transform nrCapturedWideEarly;
                nrCapturedWideEarly = sc.navigationRig;
                SuperController.LogMessage(string.Format(
                    "GabrielHud [key K]: [CameraRig]/navigationRig world pos=({0:F4},{1:F4},{2:F4}) euler=({3:F4},{4:F4},{5:F4})",
                    nrCapturedWideEarly.position.x,
                    nrCapturedWideEarly.position.y,
                    nrCapturedWideEarly.position.z,
                    nrCapturedWideEarly.rotation.eulerAngles.x,
                    nrCapturedWideEarly.rotation.eulerAngles.y,
                    nrCapturedWideEarly.rotation.eulerAngles.z));
            }
            else
            {
                SuperController.LogMessage("GabrielHud [key K]: navigationRig is null");
            }

            if (sc.navigationRig != null && sc.centerCameraTarget != null)
            {
                Transform ceCapturedWide;
                Quaternion patchRotCapturedWide;

                ceCapturedWide = sc.centerCameraTarget.transform;
                patchRotCapturedWide = ComputeCameraRigRotationForPatch(sc);
                SuperController.LogMessage(string.Format(
                    "GabrielHud [key K]: centerCameraTarget world euler=({0:F4},{1:F4},{2:F4})",
                    ceCapturedWide.rotation.eulerAngles.x,
                    ceCapturedWide.rotation.eulerAngles.y,
                    ceCapturedWide.rotation.eulerAngles.z));
                SuperController.LogMessage(string.Format(
                    "GabrielHud [key K]: cameraRig PATCH euler (center-eye, Z=0)=({0:F4},{1:F4},{2:F4})",
                    patchRotCapturedWide.eulerAngles.x,
                    patchRotCapturedWide.eulerAngles.y,
                    patchRotCapturedWide.eulerAngles.z));
            }

            Atom windowCameraAtomCapturedWide;
            windowCameraAtomCapturedWide = sc.GetAtomByUid("WindowCamera");
            if (windowCameraAtomCapturedWide != null &&
                windowCameraAtomCapturedWide.mainController != null &&
                windowCameraAtomCapturedWide.mainController.control != null)
            {
                Transform ctlCapturedWide;
                ctlCapturedWide = windowCameraAtomCapturedWide.mainController.control;
                SuperController.LogMessage(string.Format(
                    "GabrielHud [key K]: WindowCamera control world pos=({0:F4},{1:F4},{2:F4}) euler=({3:F4},{4:F4},{5:F4})",
                    ctlCapturedWide.position.x,
                    ctlCapturedWide.position.y,
                    ctlCapturedWide.position.z,
                    ctlCapturedWide.rotation.eulerAngles.x,
                    ctlCapturedWide.rotation.eulerAngles.y,
                    ctlCapturedWide.rotation.eulerAngles.z));
            }
            else
            {
                SuperController.LogMessage(
                    "GabrielHud [key K]: WindowCamera / mainController / control missing — JSON patch skips WindowCamera when absent.");
            }
        }

        /// <summary>
        /// Euler baked into atom [CameraRig]. Uses world rotation of
        /// <see cref="SuperController.centerCameraTarget"/> (full tilt/yaw/pitch).
        /// Roll stripped to euler <c>Z == 0</c> before JSON write.
        /// VaM VR ties the rig to <c>playerNavCollider</c> unless the offline patch
        /// removes that binding so this rotation can survive reload.
        /// </summary>
        private static Quaternion ComputeCameraRigRotationForPatch(SuperController sc)
        {
            Transform rigCapturedWide;
            Transform camCapturedWide;
            Quaternion rawCapturedWide;

            if (sc == null)
            {
                return Quaternion.identity;
            }

            rigCapturedWide = sc.navigationRig;
            if (rigCapturedWide == null)
            {
                return Quaternion.identity;
            }

            if (sc.centerCameraTarget == null)
            {
                rawCapturedWide = rigCapturedWide.rotation;
            }
            else
            {
                camCapturedWide = sc.centerCameraTarget.transform;
                rawCapturedWide = camCapturedWide.rotation;
            }

            return StripCameraRigEulerRollZ(rawCapturedWide);
        }

        /// <summary>
        /// Forces euler Z to zero on patch payload (VaM rotation UI roll axis).
        /// </summary>
        private static Quaternion StripCameraRigEulerRollZ(Quaternion rotationCapturedWide)
        {
            Vector3 eulerCapturedWide;
            eulerCapturedWide = rotationCapturedWide.eulerAngles;
            return Quaternion.Euler(eulerCapturedWide.x, eulerCapturedWide.y, 0f);
        }

        private static JSONClass BuildPatchRequest(SuperController sc)
        {
            JSONClass root = new JSONClass();
            JSONClass navigationRigNodeCapturedWide;

            root["playerHeightAdjust"].AsFloat = sc.playerHeightAdjust;

            Vector3 monEulerCapturedWideEarly;
            monEulerCapturedWideEarly = Vector3.zero;
            if (sc.MonitorCenterCamera != null)
            {
                monEulerCapturedWideEarly = sc.MonitorCenterCamera.transform.localEulerAngles;
            }

            root["monitorCameraRotation"] = Vec3Json(monEulerCapturedWideEarly);

            navigationRigNodeCapturedWide = new JSONClass();
            if (sc.navigationRig != null)
            {
                navigationRigNodeCapturedWide["position"] = Vec3Json(sc.navigationRig.position);
                navigationRigNodeCapturedWide["rotation"] =
                    Vec3Json(ComputeCameraRigRotationForPatch(sc).eulerAngles);
            }
            else
            {
                navigationRigNodeCapturedWide["position"] = Vec3Json(Vector3.zero);
                navigationRigNodeCapturedWide["rotation"] = Vec3Json(Vector3.zero);
            }

            root["cameraRig"] = navigationRigNodeCapturedWide;

            Atom wcAtomCapturedWideEarly;
            wcAtomCapturedWideEarly = sc.GetAtomByUid("WindowCamera");
            if (wcAtomCapturedWideEarly != null &&
                wcAtomCapturedWideEarly.mainController != null &&
                wcAtomCapturedWideEarly.mainController.control != null)
            {
                Transform rootTrCapturedWideEarly;
                Transform containerTrCapturedWideEarly;
                Transform ctlInnerCapturedWide;

                rootTrCapturedWideEarly = wcAtomCapturedWideEarly.transform;
                containerTrCapturedWideEarly =
                    wcAtomCapturedWideEarly.childAtomContainer != null
                        ? wcAtomCapturedWideEarly.childAtomContainer
                        : rootTrCapturedWideEarly;
                ctlInnerCapturedWide =
                    wcAtomCapturedWideEarly.mainController.control;

                JSONClass wjCapturedWideEarly = new JSONClass();
                wjCapturedWideEarly["position"] = Vec3Json(rootTrCapturedWideEarly.position);
                wjCapturedWideEarly["rotation"] = Vec3Json(rootTrCapturedWideEarly.rotation.eulerAngles);
                wjCapturedWideEarly["containerPosition"] = Vec3Json(containerTrCapturedWideEarly.position);
                wjCapturedWideEarly["containerRotation"] = Vec3Json(containerTrCapturedWideEarly.rotation.eulerAngles);
                wjCapturedWideEarly["controlPosition"] = Vec3Json(ctlInnerCapturedWide.position);
                wjCapturedWideEarly["controlRotation"] = Vec3Json(ctlInnerCapturedWide.rotation.eulerAngles);
                root["windowCamera"] = wjCapturedWideEarly;
            }

            return root;
        }

        private static JSONClass Vec3Json(Vector3 vCapturedWide)
        {
            JSONClass oCapturedWideEarly;
            oCapturedWideEarly = new JSONClass();
            oCapturedWideEarly["x"].AsFloat = vCapturedWide.x;
            oCapturedWideEarly["y"].AsFloat = vCapturedWide.y;
            oCapturedWideEarly["z"].AsFloat = vCapturedWide.z;
            return oCapturedWideEarly;
        }

        private static void TryLaunchPythonInterpreterWithArgumentsSnippet(
            string argumentsSnippetJoinedQuotedCapturedWide,
            out Process procOutCapturedWideEarly,
            out string launchAttemptsSummaryCapturedWideLate)
        {
            string[] launchersCapturedWideWide;
            int launcherProbeIndexCapturedWideWide;
            string launcherExecutableNameCapturedWide;
            ProcessStartInfo processStartCapturedWideLate;
            string summaryJoinedCapturedWideAccumulator;

            procOutCapturedWideEarly = null;
            launchAttemptsSummaryCapturedWideLate = "";
            launchersCapturedWideWide = new string[] { "python", "py" };
            summaryJoinedCapturedWideAccumulator = "";
            launcherProbeIndexCapturedWideWide = 0;
            while (
                launcherProbeIndexCapturedWideWide < launchersCapturedWideWide.Length &&
                procOutCapturedWideEarly == null)
            {
                launcherExecutableNameCapturedWide =
                    launchersCapturedWideWide[launcherProbeIndexCapturedWideWide];
                summaryJoinedCapturedWideAccumulator =
                    summaryJoinedCapturedWideAccumulator +
                        (summaryJoinedCapturedWideAccumulator.Length > 0 ? ", " : "") +
                        launcherExecutableNameCapturedWide;
                processStartCapturedWideLate = new ProcessStartInfo();
                processStartCapturedWideLate.FileName = launcherExecutableNameCapturedWide;
                processStartCapturedWideLate.Arguments =
                    launcherExecutableNameCapturedWide == "py"
                        ? ("-3 " + argumentsSnippetJoinedQuotedCapturedWide)
                        : argumentsSnippetJoinedQuotedCapturedWide;
                processStartCapturedWideLate.UseShellExecute = false;
                processStartCapturedWideLate.CreateNoWindow = true;
                try
                {
                    procOutCapturedWideEarly = Process.Start(processStartCapturedWideLate);
                }
                catch (Exception)
                {
                    procOutCapturedWideEarly = null;
                }

                launcherProbeIndexCapturedWideWide++;
            }

            launchAttemptsSummaryCapturedWideLate = summaryJoinedCapturedWideAccumulator;
        }

        private static string NormalizeFwd(string pCapturedWide)
        {
            if (pCapturedWide == null)
            {
                return "";
            }

            return pCapturedWide.Replace('\\', '/');
        }

        private static string CombineFwd(string rootCapturedWide, string relCapturedWide)
        {
            string normalizedRootCapturedWide;
            string normalizedRelativeCapturedWide;

            normalizedRootCapturedWide = NormalizeFwd(rootCapturedWide).TrimEnd('/');
            normalizedRelativeCapturedWide = NormalizeFwd(relCapturedWide).TrimStart('/');
            if (normalizedRootCapturedWide.Length == 0)
            {
                return normalizedRelativeCapturedWide;
            }

            if (normalizedRelativeCapturedWide.Length == 0)
            {
                return normalizedRootCapturedWide;
            }

            return normalizedRootCapturedWide + "/" + normalizedRelativeCapturedWide;
        }

        private static string GetVaMInstallRoot()
        {
            string dataPathCapturedWide;
            const string suffixCapturedWide = "/VaM_Data";

            dataPathCapturedWide = NormalizeFwd(Application.dataPath);
            if (dataPathCapturedWide.Length >= suffixCapturedWide.Length &&
                dataPathCapturedWide.EndsWith(suffixCapturedWide))
            {
                return dataPathCapturedWide.Substring(0, dataPathCapturedWide.Length - suffixCapturedWide.Length);
            }

            int lastSlashCapturedWideFind;
            lastSlashCapturedWideFind = dataPathCapturedWide.LastIndexOf('/');
            if (lastSlashCapturedWideFind > 0)
            {
                return dataPathCapturedWide.Substring(0, lastSlashCapturedWideFind);
            }

            return dataPathCapturedWide;
        }
    }
}
