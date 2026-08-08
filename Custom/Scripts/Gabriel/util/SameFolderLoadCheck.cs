using System;
using MeshVR;

namespace geesp0t
{
    /// <summary>
    /// Same VaM save/load folder detection: normalized path compares,
    /// idle-folder capture versus load-start folder
    /// (<see cref="SuperController.currentLoadDir"/>), and helpers for
    /// scene-settle and camera retain.
    /// </summary>
    public sealed class SameFolderLoadCheck
    {
        private string lastIdleLoadDirNorm = "";

        /// <summary>
        /// Capture the current load folder only while VaM is idle.
        /// </summary>
        public void CaptureIdleLoadDir(SuperController superController)
        {
            if (superController == null || superController.isLoading)
            {
                return;
            }

            lastIdleLoadDirNorm = NormalizeLoadDir(superController.currentLoadDir);
        }

        /// <summary>
        /// Returns true when the previous idle folder matches the folder of the
        /// load that is starting now.
        /// </summary>
        public bool IsSameFolderLoad(SuperController superController)
        {
            if (superController == null ||
                string.IsNullOrEmpty(lastIdleLoadDirNorm))
            {
                return false;
            }

            string newLoadDirNorm = NormalizeLoadDir(superController.currentLoadDir);
            return newLoadDirNorm.Length > 0 &&
                string.Equals(
                    newLoadDirNorm,
                    lastIdleLoadDirNorm,
                    StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// For debug logs: last idle folder from
        /// <see cref="CaptureIdleLoadDir"/>.
        /// </summary>
        public string DebugLastIdleLoadDirNormalized
        {
            get
            {
                return lastIdleLoadDirNorm;
            }
        }

        /// <summary>
        /// Legacy alias (<c>NormalizeLoadDir</c>) for scene-settle call sites.
        /// </summary>
        public static string NormalizeLoadDir(string dir)
        {
            return Normalize(dir);
        }

        /// <summary>
        /// Trims, normalizes slashes, and strips a trailing path separator.
        /// </summary>
        public static string Normalize(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return "";
            }

            string normalized = dir.Replace('\\', '/').Trim();
            while (normalized.Length > 1 && normalized.EndsWith("/"))
            {
                normalized = normalized.Substring(0, normalized.Length - 1);
            }

            return normalized;
        }

        /// <summary>
        /// True when normalized <paramref name="currentDir"/> is non-empty
        /// and equals normalized <paramref name="previousDir"/>
        /// (ordinal ignore-case).
        /// </summary>
        public static bool SameFolderLoads(string previousDir, string currentDir)
        {
            string currentNorm = Normalize(currentDir);
            bool sameFolderLoad =
                currentNorm.Length > 0 &&
                string.Equals(
                    Normalize(previousDir),
                    currentNorm,
                    StringComparison.OrdinalIgnoreCase);
            return sameFolderLoad;
        }
    }
}
