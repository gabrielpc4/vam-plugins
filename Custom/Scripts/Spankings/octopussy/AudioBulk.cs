using System;
using System.Collections.Generic;

// uncomment the two lines to use as a  plugin

namespace octopussy {

public class AudioBulk {//: MVRScript {

        public List<NamedAudioClip> audioclips;

        public static string lastLoadPath;
        const string WILDCARD = "*";
        public static string[] supportedFileTypes = {WILDCARD, ".wav", ".ogg", ".mp3"}; // regex next time

        public static bool LeafNameContainsSubstring(string filePath, string substring)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(substring))
                return false;
            int slash = filePath.LastIndexOf('/');
            int bslash = filePath.LastIndexOf('\\');
            if (bslash > slash)
                slash = bslash;
            string leaf = slash >= 0 ? filePath.Substring(slash + 1) : filePath;
            string sub = substring.ToLowerInvariant();
            return leaf.ToLowerInvariant().IndexOf(sub, StringComparison.Ordinal) >= 0;
        }

        public  static JSONStorableStringChooser filetype = new JSONStorableStringChooser(
            "File type", new List<string>(supportedFileTypes), "", "Restrict File type");

        //public override void Init() { Init(this); }

        public void Init(MVRScript script)
        {
            script.CreateScrollablePopup(AudioBulk.filetype, true);
            script.CreateButton("Load Audio (Folder)").button.onClick.AddListener(
                () => OpenLoadFolder(audioclips, lastLoadPath));
            script.CreateButton("Remove all clips from scene audio").button.onClick.AddListener(
                () => URLAudioClipManager.singleton.RemoveAllClips());
        }

        public static NamedAudioClip
        LoadAudio(string path)
        {
            string localPath = SuperController.singleton.NormalizeLoadPath(path);
            NamedAudioClip nac;
            nac = URLAudioClipManager.singleton.GetClip(localPath);
            if (nac != null)
            {
                return nac;
            }

            URLAudioClip clip = URLAudioClipManager.singleton.QueueClip(
                SuperController.singleton.NormalizeMediaPath(path));
            if (clip == null)
            {
                return null;
            }

            nac = URLAudioClipManager.singleton.GetClip(clip.uid);
            if (nac == null)
            {
                return null;
            }
            return nac;
        }

        public static void OpenLoadFolder(List<NamedAudioClip> clips,  string lastLoadPath)
        {
            clips = null;
            SuperController.singleton.GetDirectoryPathDialog(
                (path) => {
                    if (!string.IsNullOrEmpty(path))
                    {
                        clips = AudioBulk.LoadFolder(path, filetype.val);
                    }
                }, lastLoadPath);
        }


        public static List<NamedAudioClip>
        LoadFolder(string path, string filter="")
        {
            return LoadFolderImpl(path, filter, null);
        }

        /// <summary>Same as <see cref="LoadFolder"/> but never loads files whose leaf name contains <paramref name="skipLeafNameContains"/> (case-insensitive), e.g. <c>breath</c> to avoid decoding breathing moans.</summary>
        public static List<NamedAudioClip> LoadFolderSkippingLeafNameSubstring(string path, string filter, string skipLeafNameContains)
        {
            return LoadFolderImpl(path, filter, skipLeafNameContains);
        }

        private static List<NamedAudioClip> LoadFolderImpl(string path, string filter, string skipLeafNameContains)
        {
            lastLoadPath = path;
            List<NamedAudioClip> clips = new List<NamedAudioClip>();
            string[] filesAtPath = SuperController.singleton.GetFilesAtPath(path);
            if (filesAtPath == null)
                return clips;

            bool useWildcard = filter == WILDCARD;
            string filterLower = filter != null ? filter.ToLowerInvariant() : "";

            for (int i = 0; i < filesAtPath.Length; i++)
            {
                string filePath = filesAtPath[i];
                if (string.IsNullOrEmpty(filePath))
                    continue;
                if (!string.IsNullOrEmpty(skipLeafNameContains) &&
                    LeafNameContainsSubstring(filePath, skipLeafNameContains))
                    continue;

                if (useWildcard)
                {
                    NamedAudioClip nacW = AudioBulk.LoadAudio(filePath);
                    if (nacW != null)
                        clips.Add(nacW);
                }
                else if (string.IsNullOrEmpty(filterLower) || filePath.ToLowerInvariant().EndsWith(filterLower))
                {
                    NamedAudioClip nac = AudioBulk.LoadAudio(filePath);
                    if (nac != null)
                        clips.Add(nac);
                }
            }

            return clips;
        }
    }
}
