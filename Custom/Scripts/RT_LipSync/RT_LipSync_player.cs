using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Unity;
using UnityEngine;

namespace RT_LipSync_Player
{

    class RT_LipSync_Player : MVRScript
    {

        private AudioSource voice_source;
        AudioSourceControl headAudioSource;
        private JSONStorable headAudio;

        private DAZMorph[] viseme_morphs;
        private DAZMorph mouthOpen_morph;

        SampleClip[] sampleClips;

        private const float fs = 48e3f;
        private const int no_visemes = 16;
        private const int no_phonemes = 16;
        private const int N_freq_bins = 4096;

        string sampleAudioDirectory = "./Custom/Assets/Audio/RT_LipSync/";

        public bool randomPlaybackEnabled = false;

        private const float frequency_resolution = fs / N_freq_bins / 2;

        // General Parameters 
        float bs_change_speed = 0.6f;
        float volume_scaling = 15f;
        float jaw_volume_scaling = 20f;

        // MFCC parameters 
        const float fft_scaling = 4.4350e+06f;
        const float alpha = 0.97f;
        const int M_FB = 20;
        const int C = 12;
        const int L = 22;
        const int LF = 300;            // lower frequency limit(Hz)
        const int HF = 4500;           // upper frequency limit(Hz)
        int f_low;
        int f_high;
        float fres;

        float[,] filterBanks;
        float[,] DCT_mat;
        float[] cep_lifter;
        public float[] bs_setpoint;
        float[,] cc_mem;
        float bs_mouthOpen_setpoint;
        float current_volume;

        float avgPeakVol = 0f;
        float avgVolUpdateSpeed = .1f;
        float targetVol = .2f;

        float last_volume = 0;
        float last_random_playback = 0;

        public override void Init()
        {
            SuperController.singleton.PauseSimulation(3, "Loading " + this.containingAtom.name);
            pluginLabelJSON.val = "RT LipSync Player";
            viseme_morphs = new DAZMorph[16];
            JSONStorable js = containingAtom.GetStorableByID("geometry");
            headAudio = containingAtom.GetStorableByID("HeadAudioSource");
            if (js != null)
            {
                DAZCharacterSelector Dchar = js as DAZCharacterSelector;
                headAudioSource = Dchar.containingAtom.GetStorableByID("HeadAudioSource") as AudioSourceControl;
                voice_source = headAudioSource.audioSource;
                voice_source.rolloffMode = AudioRolloffMode.Linear;
            }

            string[] audioFiles = SuperController.singleton.GetFilesAtPath(sampleAudioDirectory);
            if (audioFiles.Length > 0)
            {

                sampleClips = new SampleClip[audioFiles.Length];

                for (int i = 0; i < audioFiles.Length; i++)
                {
                    sampleClips[i] = new SampleClip();
                    sampleClips[i].path = audioFiles[i];
                    URLAudioClipManager.singleton.QueueClip(audioFiles[i]);

                    string[] stringSeparators = new string[] { "RT_LipSync/" };
                    string[] splitStrings = sampleClips[i].path.Split(stringSeparators, StringSplitOptions.None);
                    sampleClips[i].name = splitStrings[1];
                    sampleClips[i].nameClip = URLAudioClipManager.singleton.GetClip(sampleClips[i].name);
                }

            }


            List<string> sampleClipList = new List<string>();
            if (audioFiles.Length > 0)
            {
                for (int i = 0; i < sampleClips.Length; i++)
                {
                    sampleClipList.Add(sampleClips[i].name);
                }
            }
            
            uiSelectSampleClip = new JSONStorableStringChooser("Audio Clips", sampleClipList, "None", "Select Clip");
            RegisterStringChooser(uiSelectSampleClip);
            UIDynamicPopup dp = CreateScrollablePopup(uiSelectSampleClip, false);
            dp.popupPanelHeight = 1100f;

            playAudioButton = CreateButton("Play Single", false);
            playAudioButton.height = 100;
            if (playAudioButton != null)
            {
                playAudioButton.button.onClick.AddListener(PlaySampleAudio);
            }
            stopAudioButton = CreateButton("Stop Playback", false);
            stopAudioButton.height = 100;
            if (stopAudioButton != null)
            {
                stopAudioButton.button.onClick.AddListener(StopPlayback);
            }
            JSONfloat_randomPlaybackDelay = new JSONStorableFloat("Random Playback Delay", 3f, 0.2f, 10.0f, false);
            RegisterFloat(JSONfloat_randomPlaybackDelay);
            CreateSlider(JSONfloat_randomPlaybackDelay, false); // true -> right side

            JSONfloat_PlaybackDelayRandomness = new JSONStorableFloat("Delay Randomness", 0f, 0.0f, 3.0f, false);
            RegisterFloat(JSONfloat_PlaybackDelayRandomness);
            CreateSlider(JSONfloat_PlaybackDelayRandomness, false); // true -> right side


            toggleRandomPlaybackButton = CreateButton("Random Playback", false);
            toggleRandomPlaybackButton.height = 100;
            if (toggleRandomPlaybackButton != null)
            {
                toggleRandomPlaybackButton.button.onClick.AddListener(StartRandomPlayback);
            }
            
        }

        void Start()
        {
            //SuperController.LogMessage("RT Lip Sync Player: " + sampleClips.Length + " sample clips detected");
        }

        void PlaySampleAudio()
        {
            if (headAudioSource)
            {
                string clipSelection = uiSelectSampleClip.val;
                for (int i = 0; i < sampleClips.Length; i++)
                {
                    if (clipSelection == sampleClips[i].name)
                    {
                        headAudioSource.CallAction("PlayNow", sampleClips[i].nameClip);
                        break;
                    }
                }
            }
            else
                SuperController.LogMessage("RT_LipSync: No audiosource detected");
        }

        void StartRandomPlayback()
        {
            randomPlaybackEnabled = true;
        }

        void PlayRandomClip()
        {
            if (headAudioSource)
            {
                System.Random rnd = new System.Random();
                int Rclip = rnd.Next(sampleClips.Length);
                headAudioSource.PlayIfClear(sampleClips[Rclip].nameClip);
            }
        }

        void StopPlayback()
        {
            if (headAudioSource)
            {
                headAudioSource.Stop();
            }
            randomPlaybackEnabled = false;
        }

        public void FixedUpdate()
        {
            if (randomPlaybackEnabled)
            {
                System.Random rnd = new System.Random();
                float randomDelay = JSONfloat_PlaybackDelayRandomness.val * ((float)rnd.Next(100) / 100f - 0.5f);

                if ((Time.time - last_random_playback) > (JSONfloat_randomPlaybackDelay.val + randomDelay))
                {
                    PlayRandomClip();
                    last_random_playback = Time.time;
                }
            }
        }

        
        protected JSONStorableStringChooser uiSelectSampleClip;
        private UIDynamicButton playAudioButton;
        private UIDynamicButton stopAudioButton;
        private UIDynamicButton toggleRandomPlaybackButton;
        private URLAudioClipUI sampleClipUI;
        protected JSONStorableFloat JSONfloat_randomPlaybackDelay;
        protected JSONStorableFloat JSONfloat_PlaybackDelayRandomness;

        public class SampleClip
        {
            public string path;
            public string name;
            public NamedAudioClip nameClip;
        }
    }

}
