namespace geesp0t
{
    /// <summary>
    /// Normalizes VaM <see cref="SuperController.currentLoadDir"/> strings so
    /// same-folder detection matches across session code paths.
    /// </summary>
    public static class SameFolderLoadDirNormalize
    {
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
    }
}
