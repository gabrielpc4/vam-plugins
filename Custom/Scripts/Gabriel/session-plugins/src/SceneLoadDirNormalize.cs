namespace geesp0t
{
    /// <summary>
    /// Normalizes VaM save/load folder paths for same-folder compares.
    /// </summary>
    internal static class SceneLoadDirNormalize
    {
        internal static string Normalize(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return "";

            string normalized = dir.Replace('\\', '/').Trim();
            while (normalized.Length > 1 && normalized.EndsWith("/"))
                normalized = normalized.Substring(0, normalized.Length - 1);

            return normalized;
        }

        internal static bool SameFolderLoads(
            string previousDir,
            string currentDir)
        {
            string currentNorm = Normalize(currentDir);
            bool sameFolderLoad =
                currentNorm.Length > 0 &&
                string.Equals(
                    Normalize(previousDir),
                    currentNorm,
                    System.StringComparison.OrdinalIgnoreCase);
            return sameFolderLoad;
        }
    }
}
