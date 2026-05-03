using System;
using System.Diagnostics;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// <b>K</b> hotkey: logs navigation / monitor / height / WindowCamera /
    /// <c>[CameraRig]</c> values, writes a request JSON, and runs
    /// <c>tools/patch_scene_initial_camera.py</c> to update the loaded scene’s
    /// main <c>.json</c> (see
    /// <c>Reference/VaM-Camera-Initial-Scene-Pose.md</c>).
    /// Scene folder comes from <see cref="SuperController.currentLoadDir"/>.
    /// Absolute paths for Python are <see cref="Application.dataPath"/> minus
    /// <c>VaM_Data</c>, plus forward-slash joins (dynamic scripts cannot use
    /// <c>System.IO</c> or <c>MVR.FileManagement</c>). Process stdout/stderr
    /// are not read (that would pull <c>System.IO</c>); see
    /// <see cref="PatchToolLogRelative"/>.
    /// </summary>
    public static class EasyMateKSceneCameraPatch
    {
        public const string PatchScriptRelative = "Custom/Scripts/Easy Mate/tools/patch_scene_initial_camera.py";
        public const string RequestJsonRelative = "Custom/Scripts/Easy Mate/tools/last_scene_camera_patch_request.json";

        /// <summary>
        /// Append-only log next to the Python script (relative to VaM install).
        /// </summary>
        public const string PatchToolLogRelative = "Custom/Scripts/Easy Mate/tools/last_scene_camera_patch_log.txt";

        public static void TryRunFromHotkey()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
            {
                SuperController.LogError("EasyMate [key K]: SuperController.singleton is null.");
                return;
            }

            string loadDir = sc.currentLoadDir != null ? sc.currentLoadDir : "";
            loadDir = NormalizeFwd(loadDir);
            if (loadDir.Length == 0)
            {
                SuperController.LogError("EasyMate [key K]: currentLoadDir is empty; cannot resolve scene folder.");
                return;
            }

            JSONClass request = BuildPatchRequest(sc);
            string payload = request.ToString("");
            try
            {
                sc.SaveStringIntoFile(RequestJsonRelative, payload);
            }
            catch (Exception e)
            {
                SuperController.LogError("EasyMate [key K]: failed to write request JSON: " + e.Message);
                return;
            }

            LogCapturedPose(sc, loadDir, request);

            string installRoot = GetVaMInstallRoot();
            string sceneFolderAbs = CombineFwd(installRoot, loadDir);
            string scriptAbs = CombineFwd(installRoot, PatchScriptRelative);
            string requestAbs = CombineFwd(installRoot, RequestJsonRelative);

            StringBuilder args = new StringBuilder();
            args.Append("-u \"");
            args.Append(scriptAbs);
            args.Append("\" \"");
            args.Append(sceneFolderAbs);
            args.Append("\" \"");
            args.Append(requestAbs);
            args.Append("\"");

            string pyArgs = args.ToString();
            Process proc = null;
            string launcherTried = "";
            string[] launchers = new string[] { "python", "py" };
            for (int li = 0; li < launchers.Length && proc == null; li++)
            {
                string exe = launchers[li];
                launcherTried = launcherTried + (launcherTried.Length > 0 ? ", " : "") + exe;
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exe;
                psi.Arguments = exe == "py" ? ("-3 " + pyArgs) : pyArgs;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                try
                {
                    proc = Process.Start(psi);
                }
                catch (Exception)
                {
                    proc = null;
                }
            }

            if (proc == null)
            {
                SuperController.LogError(
                    "EasyMate [key K]: could not start Python (tried: " + launcherTried + "). Install Python 3 or add it to PATH.");
                return;
            }

            bool finished = proc.WaitForExit(180000);
            if (!finished)
            {
                try
                {
                    proc.Kill();
                }
                catch
                {
                }

                SuperController.LogError("EasyMate [key K]: python patch timed out (180s). See " + PatchToolLogRelative);
                return;
            }

            int exitCode = proc.ExitCode;
            if (exitCode != 0)
            {
                SuperController.LogError(string.Format(
                    "EasyMate [key K]: python exit {0}. Details: {1}",
                    exitCode,
                    PatchToolLogRelative));
            }
            else
                SuperController.LogMessage(string.Format(
                    "EasyMate [key K]: python exit 0. Details: {0}",
                    PatchToolLogRelative));
        }

        private static JSONClass BuildPatchRequest(SuperController sc)
        {
            JSONClass root = new JSONClass();
            root["playerHeightAdjust"].AsFloat = sc.playerHeightAdjust;

            Vector3 monEuler = Vector3.zero;
            if (sc.MonitorCenterCamera != null)
                monEuler = sc.MonitorCenterCamera.transform.localEulerAngles;
            root["monitorCameraRotation"] = Vec3Json(monEuler);

            JSONClass cr = new JSONClass();
            if (sc.navigationRig != null)
            {
                cr["position"] = Vec3Json(sc.navigationRig.position);
                cr["rotation"] = Vec3Json(sc.navigationRig.rotation.eulerAngles);
            }
            else
            {
                cr["position"] = Vec3Json(Vector3.zero);
                cr["rotation"] = Vec3Json(Vector3.zero);
            }

            root["cameraRig"] = cr;

            Atom wc = sc.GetAtomByUid("WindowCamera");
            if (wc != null && wc.mainController != null && wc.mainController.control != null)
            {
                Transform rootTr = wc.transform;
                Transform containerTr = wc.childAtomContainer != null ? wc.childAtomContainer : rootTr;
                Transform ctl = wc.mainController.control;

                JSONClass wj = new JSONClass();
                wj["position"] = Vec3Json(rootTr.position);
                wj["rotation"] = Vec3Json(rootTr.rotation.eulerAngles);
                wj["containerPosition"] = Vec3Json(containerTr.position);
                wj["containerRotation"] = Vec3Json(containerTr.rotation.eulerAngles);
                wj["controlPosition"] = Vec3Json(ctl.position);
                wj["controlRotation"] = Vec3Json(ctl.rotation.eulerAngles);
                root["windowCamera"] = wj;
            }

            return root;
        }

        private static JSONClass Vec3Json(Vector3 v)
        {
            JSONClass o = new JSONClass();
            o["x"].AsFloat = v.x;
            o["y"].AsFloat = v.y;
            o["z"].AsFloat = v.z;
            return o;
        }

        private static void LogCapturedPose(SuperController sc, string loadDir, JSONClass request)
        {
            SuperController.LogMessage("EasyMate [key K]: scene folder (currentLoadDir)=" + loadDir);
            SuperController.LogMessage("EasyMate [key K]: patch request JSON written to " + RequestJsonRelative);
            SuperController.LogMessage("EasyMate [key K]: playerHeightAdjust=" + sc.playerHeightAdjust.ToString("G9"));

            if (sc.MonitorCenterCamera != null)
            {
                Vector3 e = sc.MonitorCenterCamera.transform.localEulerAngles;
                SuperController.LogMessage(string.Format(
                    "EasyMate [key K]: monitorCameraRotation (local euler)=({0:F4},{1:F4},{2:F4})",
                    e.x,
                    e.y,
                    e.z));
            }
            else
                SuperController.LogMessage("EasyMate [key K]: MonitorCenterCamera is null");

            if (sc.navigationRig != null)
            {
                Transform nr = sc.navigationRig;
                SuperController.LogMessage(string.Format(
                    "EasyMate [key K]: [CameraRig]/navigationRig world pos=({0:F4},{1:F4},{2:F4}) euler=({3:F4},{4:F4},{5:F4})",
                    nr.position.x,
                    nr.position.y,
                    nr.position.z,
                    nr.rotation.eulerAngles.x,
                    nr.rotation.eulerAngles.y,
                    nr.rotation.eulerAngles.z));
            }
            else
                SuperController.LogMessage("EasyMate [key K]: navigationRig is null");

            Atom wc = sc.GetAtomByUid("WindowCamera");
            if (wc != null && wc.mainController != null && wc.mainController.control != null)
            {
                Transform ctl = wc.mainController.control;
                SuperController.LogMessage(string.Format(
                    "EasyMate [key K]: WindowCamera control world pos=({0:F4},{1:F4},{2:F4}) euler=({3:F4},{4:F4},{5:F4})",
                    ctl.position.x,
                    ctl.position.y,
                    ctl.position.z,
                    ctl.rotation.eulerAngles.x,
                    ctl.rotation.eulerAngles.y,
                    ctl.rotation.eulerAngles.z));
            }
            else
                SuperController.LogMessage("EasyMate [key K]: WindowCamera / mainController / control missing — JSON patch will skip WindowCamera.");

            if (request["windowCamera"] != null)
                SuperController.LogMessage("EasyMate [key K]: request includes windowCamera (atom + container + control).");
        }

        private static string NormalizeFwd(string p)
        {
            if (p == null)
                return "";
            return p.Replace('\\', '/');
        }

        /// <summary>
        /// Join install root and VaM-relative path using <c>/</c> only (no
        /// <c>System.IO</c> / <c>MVR.FileManagement</c>).
        /// </summary>
        private static string CombineFwd(string root, string rel)
        {
            string r = NormalizeFwd(root).TrimEnd('/');
            string x = NormalizeFwd(rel).TrimStart('/');
            if (r.Length == 0)
                return x;
            if (x.Length == 0)
                return r;
            return r + "/" + x;
        }

        /// <summary>
        /// Game install directory (folder containing <c>VaM_Data</c>), from Unity
        /// only; string operations only.
        /// </summary>
        private static string GetVaMInstallRoot()
        {
            string dataPath = NormalizeFwd(Application.dataPath);
            const string suffix = "/VaM_Data";
            if (dataPath.Length >= suffix.Length && dataPath.EndsWith(suffix))
                return dataPath.Substring(0, dataPath.Length - suffix.Length);
            int li = dataPath.LastIndexOf('/');
            if (li > 0)
                return dataPath.Substring(0, li);
            return dataPath;
        }
    }
}
