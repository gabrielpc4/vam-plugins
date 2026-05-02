
using System;
using System.Collections.Generic;
using UnityEngine;


namespace octopussy
{
    class RandaudioAction : MVRScript
    {
        AudioSourceControl audiosource;
        RandomAudio randAudio;

        List<NamedAudioClip> audioClips;
        JSONStorableAction playAction;
        JSONStorableString lastLoadPath;

        JSONStorableBool playIfClear;

        //List<NamedAudioClip> audioClips;
        List<NamedAudioClip> li;
        JSONStorableAction audioClipAction;
        JSONStorableFloat defaultDelay;
        JSONStorableString folderpath;

        AudioSourceControl audioSource;
        protected AudioBulk audioBulk;

        public static string[] supportedFileTypes = { "", ".wav", ".ogg", ".mp3" }; // regex next time
        public static JSONStorableStringChooser filetype = new JSONStorableStringChooser(
            "File type", new List<string>(supportedFileTypes), "", "Restrict File type");

        //SelectJsonStorable<AudioSource> selector;

        public override void Init()
        {
            pluginLabelJSON.val = "RandomDelayedAudioAction";
            // if containing atom type is person
            // headAudio = her.GetStorableByID("HeadAudioSource") as AudioSourceControl;

            folderpath = new JSONStorableString("loadpath", SuperController.singleton.currentLoadDir);

            if (insideRestore)
            {
                audioClips = AudioBulk.LoadFolder(folderpath.val);
            }

            audioBulk = new AudioBulk();
            audioBulk.Init(this);
            /*
            CreateScrollablePopup(AudioBulk.filetype, true);

            CreateButton("Load Audio Folder").button.onClick.AddListener(
                () => AudioBulk.OpenLoadFolder(li, folderpath.val));

            CreateButton("Remove all clips from scene audio").button.onClick.AddListener(
                () => URLAudioClipManager.singleton.RemoveAllClips());
                */
            playIfClear = new JSONStorableBool("Play if clear", true);
            playIfClear.storeType = JSONStorableParam.StoreType.Full;
            CreateToggle(playIfClear);

            defaultDelay = new JSONStorableFloat("DefaultDelay", 0f, 0f, 2f, false);
            CreateSlider(defaultDelay, true);

            audioSource = containingAtom.GetStorableByID("AudioSource") as AudioSourceControl;


            audioClipAction = new JSONStorableAction("Play Random Delayed", () => playRandomClipAction());

            //audioClipActionDelayed = new JSONStorableAction("Play Random Delayed", () => playRandomClipAction(defaultDelay));

            CreateButton("Play Random").button.onClick.AddListener(() => audioClipAction.actionCallback());

            RegisterString(folderpath);
            RegisterBool(playIfClear);
            RegisterAction(audioClipAction);
        }


        public void playRandomClipAction()
        {
            if (audioClips == null)
            {
                SuperController.LogError("no audioClips list");
            }
            else
            if (audioClips.Count > 0)
            {
                randAudio.playRandomDelayedIfClear(audioSource.audioSource, 1f, defaultDelay.val);
            }
        }
    }
}
