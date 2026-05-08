namespace geesp0t
{
    /// <summary>
    /// Normalizes VaM save/load folder paths for same-folder compares.
    /// </summary>
    internal static class SceneLoadDirNormalize
    {
        internal static string Normalize(string dir)
        {
            return SameFolderLoadDirNormalize.Normalize(dir);
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
