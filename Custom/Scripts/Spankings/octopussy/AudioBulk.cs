using System.Collections.Generic;
using System.Linq;

// uncomment the two lines to use as  plugin

namespace octopussy {

public class AudioBulk {//: MVRScript {

        public List<NamedAudioClip> audioclips;

        public static string lastLoadPath;
        const string WILDCARD = "*";
        public static string[] supportedFileTypes = {WILDCARD, ".wav", ".ogg", ".mp3"}; // regex next time
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
        LoadFolder(string path, string filter="") {
            lastLoadPath = path;
            List<NamedAudioClip> clips = new List<NamedAudioClip>();
            if (filter == WILDCARD) {
                SuperController.singleton.GetFilesAtPath(path)
                    .ToList().ForEach(filePath => clips.Add(AudioBulk.LoadAudio(filePath)));
            }
            else SuperController.singleton.GetFilesAtPath(path).ToList()
                    .ForEach((filePath) =>
            {
                if (filePath.ToLower().EndsWith(filter))
                {
                    clips.Add(AudioBulk.LoadAudio(filePath));
                }
            });
            return clips;
        }
    }
}
