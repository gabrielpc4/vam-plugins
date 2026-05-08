using System;

namespace geesp0t
{
    /// <summary>
    /// Centralizes the "same-folder scene load" rule.
    ///
    /// True means the folder sampled while VaM was idle before the load
    /// matches the folder VaM points <see cref="SuperController.currentLoadDir"/>
    /// at when the next load begins.
    /// </summary>
    public class SameFolderSceneLoadCheck
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

        /// <summary>For debug logs: last idle folder captured by <see cref="CaptureIdleLoadDir"/>.
        /// </summary>
        public string DebugLastIdleLoadDirNormalized
        {
            get
            {
                return lastIdleLoadDirNorm;
            }
        }

        public static string NormalizeLoadDir(string dir)
        {
            return SameFolderLoadDirNormalize.Normalize(dir);
        }
    }
}
