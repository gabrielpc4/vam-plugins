namespace geesp0t
{
    /// <summary>
    /// VaM-install relative paths via <see cref="SuperController"/> file listings.
    /// </summary>
    public static class VaMFilePathUtil
    {
        public static string GetFileName(string relativePath)
        {
            return relativePath.Substring(
                relativePath.LastIndexOfAny(new[] { '/', '\\' }) + 1);
        }

        public static bool FileExists(string relativePath)
        {
            int folderSeparatorIndex;
            string pathFolder;
            string pathFile;
            string[] pathFileList;

            folderSeparatorIndex = relativePath.LastIndexOfAny(
                new[] { '/', '\\' });
            if (folderSeparatorIndex < 0)
                return false;

            pathFolder = relativePath.Substring(0, folderSeparatorIndex);
            pathFile = relativePath.Substring(folderSeparatorIndex + 1);
            pathFileList = SuperController.singleton.GetFilesAtPath(pathFolder);
            if (pathFileList == null || pathFileList.Length == 0)
                return false;

            foreach (string foundPathFile in pathFileList)
            {
                string correctedPathFile = foundPathFile.Replace("\\", "/");
                if (correctedPathFile.EndsWith("/" + pathFile))
                    return true;
            }

            return false;
        }
    }
}
