using System.Collections.Generic;
using MeshVR;
using SimpleJSON;

namespace geesp0t
{
    /// <summary>
    /// Read/write <see cref="MVRPluginManager"/> plugin path lists with VaM save-dir
    /// and <c>Custom/Scripts</c> fallbacks when serialized paths are stale.
    /// </summary>
    public static class PluginManager
    {
        /// <summary>
        /// When <paramref name="path"/> does not exist on disk under its stored
        /// folder, rewrite it to <c>Custom/Scripts/&lt;basename&gt;</c> if that
        /// file exists. Returns false if the entry should be dropped (missing
        /// everywhere), matching <see cref="CollectNormalizedPluginPaths"/>.
        /// </summary>
        public static bool TryNormalizeMissingPluginPath(ref string path)
        {
            int folderSeparatorIndex = path.LastIndexOf("/");
            if (folderSeparatorIndex <= 0 || folderSeparatorIndex >= path.Length - 1)
                return true;

            if (!FileManager.FileExists(path))
            {
                string scriptInStandardFolder =
                    "Custom/Scripts/" + FileManager.GetFileName(path);
                if (FileManager.FileExists(scriptInStandardFolder))
                {
                    path = scriptInStandardFolder;
                    return true;
                }

                return false;
            }

            return true;
        }

        public static List<string> CollectNormalizedPluginPaths(
            MVRPluginManager pluginManager)
        {
            List<string> paths = new List<string>();
            JSONClass current = pluginManager.GetJSON(true, true, true);

            if (current["plugins"] == null ||
                current["plugins"]["plugin#0"] == null ||
                current["plugins"]["plugin#0"].Value == "")
            {
                return paths;
            }

            foreach (JSONNode pluginNode in current["plugins"].Childs)
            {
                string path = pluginNode.Value;
                int folderSeparatorIndex;

                if (path.StartsWith("./"))
                    path = SuperController.singleton.currentSaveDir + "/" +
                        path.Substring(2);

                folderSeparatorIndex = path.LastIndexOf("/");
                if (folderSeparatorIndex < 0)
                {
                    path = SuperController.singleton.currentSaveDir + "/" + path;
                    folderSeparatorIndex = path.LastIndexOf("/");
                }

                if (!TryNormalizeMissingPluginPath(ref path))
                    continue;

                paths.Add(path);
            }

            return paths;
        }

        public static void ApplyPluginPathsToManager(
            MVRPluginManager pluginManager,
            List<string> paths)
        {
            if (paths == null || paths.Count == 0)
            {
                const string emptyPluginManager =
                    "{ \"id\" : \"PluginManager\", \"plugins\" : { } }";
                JSONClass emptyState =
                    JSONNode.Parse(emptyPluginManager).AsObject;
                pluginManager.LateRestoreFromJSON(emptyState);
                return;
            }

            pluginManager.LateRestoreFromJSON(CreatePluginJSON(paths.ToArray()));
        }

        public static JSONClass CreatePluginJSON(string[] pluginList)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.Append("{");
            builder.Append(" \"id\": \"PluginManager\",");
            builder.Append(" \"plugins\": {");
            for (int i = 0; i < pluginList.Length; i++)
            {
                builder.Append(
                    string.Format(
                        "    \"plugin#{0}\": \"{1}\"{2}",
                        i,
                        pluginList[i],
                        (i + 1) < pluginList.Length ? "," : ""));
            }
            builder.Append("  }");
            builder.Append("}");
            return JSONNode.Parse(builder.ToString()).AsObject;
        }

        public static bool PersonHasPluginByFileName(
            Atom atom,
            string desiredFileName)
        {
            MVRPluginManager pluginManager =
                atom.GetStorableByID("PluginManager") as MVRPluginManager;
            if (pluginManager == null)
                return false;

            foreach (string path in CollectNormalizedPluginPaths(pluginManager))
            {
                if (FileManager.GetFileName(path) == desiredFileName)
                    return true;
            }

            return false;
        }

        public static void TryMergePluginOntoPerson(
            Atom atom,
            string desiredPluginPath)
        {
            MVRPluginManager pluginManager =
                atom.GetStorableByID("PluginManager") as MVRPluginManager;
            if (pluginManager == null)
                return;

            string desiredFileName =
                FileManager.GetFileName(desiredPluginPath);
            List<string> paths = CollectNormalizedPluginPaths(pluginManager);
            bool has = false;

            foreach (string path in paths)
            {
                if (FileManager.GetFileName(path) == desiredFileName)
                {
                    has = true;
                    break;
                }
            }

            if (!has)
                paths.Add(desiredPluginPath);

            ApplyPluginPathsToManager(pluginManager, paths);
        }

        public static void TryRemovePluginFromPerson(
            Atom atom,
            string desiredFileName)
        {
            MVRPluginManager pluginManager =
                atom.GetStorableByID("PluginManager") as MVRPluginManager;
            if (pluginManager == null)
                return;

            List<string> paths = CollectNormalizedPluginPaths(pluginManager);
            paths.RemoveAll(
                path => FileManager.GetFileName(path) == desiredFileName);
            ApplyPluginPathsToManager(pluginManager, paths);
        }
    }
}
