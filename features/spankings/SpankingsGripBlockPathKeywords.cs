using System;
using System.Collections.Generic;

namespace geesp0t
{
    /// <summary>
    /// Parses <c>spankings_grip_merge_block_path_keywords.txt</c> and checks whether
    /// the current scene’s <see cref="SuperController.currentLoadDir"/> and
    /// <see cref="SuperController.currentSaveDir"/> (folder paths only) contain any
    /// configured keyword substring (case-insensitive). When true,
    /// <see cref="Hud"/> skips Spankings merge on first VR grip for this load.
    /// </summary>
    public static class SpankingsGripBlockPathKeywords
    {
        /// <summary>
        /// Relative to VaM install; haystack is lowercase
        /// <c>loadDir + " " + saveDir</c> with backslashes normalized.
        /// </summary>
        public const string KeywordsFileRelative =
            "Custom/Scripts/features/spankings/spankings_grip_merge_block_path_keywords.txt";

        private const string LogReadErrorPrefix = "Hud: read ";

        /// <summary>
        /// Substring match on <paramref name="haystack"/> for keywords parsed from
        /// <see cref="KeywordsFileRelative"/>.
        /// </summary>
        public static bool EvaluateKeywordsAgainstHaystack(List<string> keys, string haystack,
            out string matchDetail)
        {
            matchDetail = "";
            if (keys == null || keys.Count == 0)
            {
                matchDetail =
                    "no keywords in " + KeywordsFileRelative;
                return false;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                string k = keys[i];
                if (k == null || k.Length == 0)
                    continue;
                if (haystack.IndexOf(k, StringComparison.Ordinal) >= 0)
                {
                    matchDetail =
                        "substring matched block keyword \"" + k + "\"";
                    return true;
                }
            }

            matchDetail = "no block keyword substring in haystack";
            return false;
        }

        /// <summary>
        /// Loads keywords via <see cref="SuperController.ReadFileIntoString"/>, trims,
        /// skips <c>#</c> lines, splits on newlines / comma / semicolon.
        /// </summary>
        public static List<string> GetParsedKeywords()
        {
            List<string> result = new List<string>();
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return result;

            string raw = null;
            try
            {
                raw = sc.ReadFileIntoString(KeywordsFileRelative);
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    LogReadErrorPrefix + KeywordsFileRelative + ": " + e.Message);
                return result;
            }

            if (string.IsNullOrEmpty(raw))
                return result;

            char[] seps = new char[] { '\r', '\n', ',', ';' };
            string[] parts = raw.Split(seps);
            for (int p = 0; p < parts.Length; p++)
            {
                string t = parts[p].Trim();
                if (t.Length == 0)
                    continue;
                if (t[0] == '#')
                    continue;
                result.Add(t.ToLowerInvariant());
            }

            return result;
        }

        /// <summary>
        /// Same path haystack construction as emotion path keywords.
        /// </summary>
        public static bool EvaluatePathRule(out string loadDir, out string saveDir,
            out string haystack, out int keywordCount, out string matchDetail)
        {
            loadDir = "";
            saveDir = "";
            haystack = "";
            keywordCount = 0;
            matchDetail = "";

            List<string> keys = GetParsedKeywords();
            keywordCount = keys != null ? keys.Count : 0;

            SuperController sc = SuperController.singleton;
            if (sc == null)
            {
                matchDetail = "SuperController.singleton is null";
                return false;
            }

            loadDir = sc.currentLoadDir != null ? sc.currentLoadDir : "";
            saveDir = sc.currentSaveDir != null ? sc.currentSaveDir : "";
            haystack = (loadDir + " " + saveDir).Replace('\\', '/')
                .ToLowerInvariant();

            return EvaluateKeywordsAgainstHaystack(keys, haystack, out matchDetail);
        }

        /// <summary>
        /// True when the current scene folders match a Spankings grip-merge block keyword.
        /// </summary>
        public static bool CurrentSceneBlocksGripSpankingsMerge()
        {
            string ld;
            string sd;
            string hs;
            int kc;
            string detail;
            return EvaluatePathRule(out ld, out sd, out hs, out kc, out detail);
        }
    }
}
