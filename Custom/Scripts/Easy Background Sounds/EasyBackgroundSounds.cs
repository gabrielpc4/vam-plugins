using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
namespace geesp0t
{
    public class EasyBackgroundSounds : MVRScript
    {

        protected bool logMessages = false;

        protected NamedAudioClip audioClip;

        protected List<NamedAudioClip> backgroundMusicClips = new List<NamedAudioClip>();
        protected List<string> backgroundMusicClipNames = new List<string>();
        protected List<NamedAudioClip> otherClips = new List<NamedAudioClip>();

        protected Atom audioSourceAtom = null;
        protected AudioSource audioSource = null;
        protected String audioSourceName = "";
        protected bool creatingAudioSource = false;
        protected bool audioSourceCreated = false;

        protected Atom webPanelAtom = null;
        protected VRWebBrowser webPanel = null;
        protected String webPanelName = "";
        protected bool creatingWebPanel = false;
        protected bool webPanelCreated = false;
        protected bool wantsToOpenURL = false;
        protected bool isPlayingURLMusic = false;

        AudioSource headAudioSource = null;

        protected string randomName = "Random Music";
        protected bool musicClipChanged = false;
        protected int clipIndex = 0;
        protected float lastVolume = -1.0f;
        protected bool clipIndexIsMusicClip = true;

        protected UnityEngine.UI.InputField inputField = null;
        protected UIDynamicTextField webPanelURLField = null;

        protected JSONStorableStringChooser backgroundMusicChooser;
        protected JSONStorableFloat volume;
        protected JSONStorableBool spatializeAudio;
        protected JSONStorableBool headAudio;
        protected JSONStorableBool spawnDedicatedAudioAtom;
        protected JSONStorableBool mouthMoves;
        protected JSONStorableBool webPageAudio;
        protected JSONStorableBool hideWebPage;
        protected JSONStorableString webPanelURL;
        protected JSONStorableFloat pitch;


        //THIS WHOLE CLASS ASSUMES THAT AUDIO CATEGORIES ARE NOT NEEDED, AND THAT EACH DISPLAY NAME IS UNIQUE. IF WE HAVE TWO CATEGORIES WITH IDENTICAL DISPLAY NAMES, THIS PLUGIN WILL NEED IMPROVING
        public override void Init()
        {
            try
            {
                if (logMessages) SuperController.LogMessage("easy background sounds init");

               backgroundMusicClips.Clear();
                otherClips.Clear();

                backgroundMusicClipNames.Clear();
                backgroundMusicClipNames.Add(randomName);

                foreach (NamedAudioClip namedAudioClip in EmbeddedAudioClipManager.singleton.embeddedClips)
                {
                    if (namedAudioClip.category == "FemaleSexLong")
                    {
                        otherClips.Add(namedAudioClip);
                        backgroundMusicClipNames.Add(namedAudioClip.displayName);
                    }
                    else if (namedAudioClip.category == "Music")
                    {
                        backgroundMusicClips.Add(namedAudioClip);
                        backgroundMusicClipNames.Add(namedAudioClip.displayName);
                    }
                    else if (namedAudioClip.category == "PersonOral")
                    {
                        otherClips.Add(namedAudioClip);
                        backgroundMusicClipNames.Add(namedAudioClip.displayName);
                    }
                    else if (namedAudioClip.category == "SoundFXSex")
                    {
                        otherClips.Add(namedAudioClip);
                        backgroundMusicClipNames.Add(namedAudioClip.displayName);
                    }
                }


                foreach (string category in URLAudioClipManager.singleton.GetCategories())
                {
                    foreach (NamedAudioClip namedAudioClip in URLAudioClipManager.singleton.GetCategoryClips(category))
                    {
                        otherClips.Add(namedAudioClip);
                        backgroundMusicClipNames.Add(namedAudioClip.displayName);
                    }
                }

                backgroundMusicChooser = new JSONStorableStringChooser("backgroundMusicClip", backgroundMusicClipNames, randomName, "Sound", SetBackgroundMusicFromChooser);
                backgroundMusicChooser.storeType = JSONStorableParam.StoreType.Full;
                backgroundMusicChooser.val = randomName;
                RegisterStringChooser(backgroundMusicChooser);
                CreateScrollablePopup(backgroundMusicChooser);

                volume = new JSONStorableFloat("Local Sound Volume", 0.18f, 0.0f, 1.0f, false);
                RegisterFloat(volume);
                volume.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(volume);

                pitch = new JSONStorableFloat("Local Sound Pitch", 1.0f, 0.8f, 1.2f, true);
                RegisterFloat(pitch);
                volume.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(pitch);
                pitch.setJSONCallbackFunction = (x) => UpdatePitch(x.val);

                spatializeAudio = new JSONStorableBool("Local Sound 3D (spatialize)", false);
                RegisterBool(spatializeAudio);
                spatializeAudio.storeType = JSONStorableParam.StoreType.Full;
                CreateToggle(spatializeAudio);
                spatializeAudio.toggle.onValueChanged.AddListener(delegate { UpdateSpatialize(); });

                headAudio = new JSONStorableBool("Use Parent Person's Head Audio Source", true);
                RegisterBool(headAudio);
                headAudio.storeType = JSONStorableParam.StoreType.Full;
                CreateToggle(headAudio);
                headAudio.toggle.onValueChanged.AddListener(delegate
                {
                    if (headAudio.val)
                    {
                        audioSource = null;
                        audioSourceAtom = null;
                    }
                    else
                    {
                        creatingAudioSource = false;
                        audioSourceCreated = false;
                    }
                    UpdateHeadAudio();
                });

                spawnDedicatedAudioAtom = new JSONStorableBool("Spawn separate EasyBgSound AudioSource atom", false);
                RegisterBool(spawnDedicatedAudioAtom);
                spawnDedicatedAudioAtom.storeType = JSONStorableParam.StoreType.Full;
                CreateToggle(spawnDedicatedAudioAtom);
                spawnDedicatedAudioAtom.toggle.onValueChanged.AddListener(delegate
                {
                    creatingAudioSource = false;
                    audioSourceCreated = false;
                    audioSource = null;
                    audioSourceAtom = null;
                    UpdateHeadAudio();
                });

                mouthMoves = new JSONStorableBool("Parent Person's Mouth Moves", false);
                RegisterBool(mouthMoves);
                mouthMoves.storeType = JSONStorableParam.StoreType.Full;
                CreateToggle(mouthMoves);
                mouthMoves.toggle.onValueChanged.AddListener(delegate { UpdateMouthMoves(); });

                CreateSpacer();

                webPageAudio = new JSONStorableBool("Use Web Page Audio Instead", false);
                RegisterBool(webPageAudio);
                webPageAudio.storeType = JSONStorableParam.StoreType.Full;
                CreateToggle(webPageAudio);
                webPageAudio.toggle.onValueChanged.AddListener(delegate { UpdateWebPanelVisibility(); });

                hideWebPage = new JSONStorableBool("Hide Web Page Display", false);
                RegisterBool(hideWebPage);
                hideWebPage.storeType = JSONStorableParam.StoreType.Full;
                CreateToggle(hideWebPage);
                hideWebPage.toggle.onValueChanged.AddListener(delegate { UpdateWebPanelVisibility(); });

                webPanelURL = new JSONStorableString("Web Page URL", "https://soundcloud.com/geesp00t/sets/easybackgroundsounds");
                RegisterString(webPanelURL);
                webPanelURL.storeType = JSONStorableParam.StoreType.Full;
                webPanelURLField = CreateTextField(webPanelURL);
                inputField = webPanelURLField.gameObject.AddComponent<UnityEngine.UI.InputField>();
                inputField.textComponent = webPanelURLField.UItext;
                webPanelURLField.backgroundColor = Color.white;
                webPanelURL.inputField = inputField;

                var btn = CreateButton("Open Page");
                btn.button.onClick.AddListener(() => { wantsToOpenURL = true; });
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        private void UpdateAll()
        {
            UpdateHeadAudio();
            UpdateSpatialize();
            UpdateMouthMoves();
            UpdatePitch(pitch.val);
        }

        private void UpdateSpatialize()
        {
            if (audioSource) {
                if (spatializeAudio.val)
                {
                    audioSource.spatialize = true;
                    audioSource.spatialBlend = 1.0f;
                } else
                {
                    audioSource.spatialize = false;
                    audioSource.spatialBlend = 0;
                }
            }
        }
        private void UpdateHeadAudio()
        {
            spatializeAudio.toggle.interactable = !headAudio.val && spawnDedicatedAudioAtom.val;

            if (audioSource)
            {
                audioSource.Stop();
                audioSource.volume = volume.val;
            }

            if (headAudioSource)
            {
                headAudioSource.Stop();
                headAudioSource.volume = volume.val;
            }
        }

        private bool PlaybackUsesHeadAudioSource()
        {
            return headAudio.val || !spawnDedicatedAudioAtom.val;
        }

        private void UpdatePitch(float newPitch)
        {
            if (PlaybackUsesHeadAudioSource())
            {
                if (headAudioSource != null)
                {
                    var pitchJSON = containingAtom.GetStorableByID("HeadAudioSource").GetFloatJSONParam("pitch");
                    pitchJSON.val = newPitch;
                }
            } else
            {
                if (audioSource != null)
                {
                    audioSource.pitch = newPitch;
                }
            }
        }

        private void UpdateMouthMoves()
        {
            if (mouthMoves.val)
            {
                SetHeadAudioDefaults(mouthMoves.val);
            }
        }

        private float GetAdjustedAudioSourceMultiplier()
        {
            if (headAudioSource != null) {
                return (15.75f / Math.Max(0.01f, headAudioSource.volume));
            }
            return 70.0f;
        }

        private void SetHeadAudioDefaults(bool enable)
        {
            //some good settings
            JSONStorable jawControl = containingAtom.GetStorableByID("JawControl");
            if (jawControl != null)
            {
                if (enable) { 
                    jawControl.SetBoolParamValue("driveXRotationFromAudioSource", true);
                    jawControl.SetFloatParamValue("spring", 40.83f);
                    jawControl.SetFloatParamValue("damper", 3.9f);
                    jawControl.SetFloatParamValue("driveXRotationFromAudioSourceAdditionalAngle", 0);
                    jawControl.SetFloatParamValue("driveXRotationFromAudioSourceMaxAngle", -35f);
                    jawControl.SetFloatParamValue("targetRotationX", 0);

                    jawControl.SetFloatParamValue("driveXRotationFromAudioSourceMultiplier", GetAdjustedAudioSourceMultiplier());
                } else
                {
                    jawControl.SetBoolParamValue("driveXRotationFromAudioSource", false);
                    jawControl.SetFloatParamValue("targetRotationX", 0);
                }
            }
        }

        private void UpdateWebPanelVisibility(bool mustHide = false)
        {
            if (webPanel != null)
            {
                if (logMessages) SuperController.LogMessage("Hide Web Page: " + hideWebPage.val);
                if (hideWebPage.val || mustHide)
                {
                    webPanel.transform.localScale = new Vector3(0, 0, 0);
                } else
                {
                    webPanel.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
        
        private IEnumerator CreateAudioSource()
        {
             yield return SuperController.singleton.AddAtomByType("AudioSource", audioSourceName);
            audioSourceCreated = true;
            if (logMessages) SuperController.LogMessage("created " + audioSourceName);

            SetBackgroundMusicFromChooser(backgroundMusicChooser.val);
        }
        private IEnumerator CreateWebPanel()
        {
            yield return SuperController.singleton.AddAtomByType("WebPanelEmissive", webPanelName);
            webPanelCreated = true;
            if (logMessages) SuperController.LogMessage("created " + webPanelName);

            Atom webPanelAtom = SuperController.singleton.GetAtomByUid(webPanelName);
            if (webPanelAtom != null)
            {
                if (logMessages) SuperController.LogMessage("found web panel atom");
                webPanel = webPanelAtom.GetComponentInChildren<VRWebBrowser>();
                if (webPanel != null)
                {
                    if (logMessages) SuperController.LogMessage("found web panel");
                    PlayURLMusic();
                }
            }

        }

        public void SetBackgroundMusicFromChooser(string musicSelected)
        {
            musicClipChanged = true;

            clipIndex = 0;

            bool clipFound = false;

            if (name == randomName)
            {
                clipIndex = UnityEngine.Random.Range(0, backgroundMusicClips.Count);
            } else
            {
                for (int i = 0; i < backgroundMusicClips.Count(); i++)
                {
                    if (backgroundMusicClips[i].displayName == musicSelected)
                    {
                        clipIndex = i;
                        clipFound = true;
                        clipIndexIsMusicClip = true;
                        break;
                    }
                }
                if (!clipFound)
                {
                    for (int i = 0; i < otherClips.Count(); i++)
                    {
                        if (otherClips[i].displayName == musicSelected)
                        {
                            clipIndex = i;
                            clipFound = true;
                            clipIndexIsMusicClip = false;
                            break;
                        }
                    }
                }
            }

            if (audioSource != null) audioSource.Stop();
            if (headAudioSource != null) headAudioSource.Stop();
        }


        public void Start()
        {
            try
            {
                //if we had a web panel running, stop it
                webPanelName = "EasyBgSoundWebPanelFromAtom" + containingAtom.name;
                webPanelAtom = SuperController.singleton.GetAtomByUid(webPanelName);
                if (webPanelAtom != null)
                {
                    webPanel = webPanelAtom.GetComponentInChildren<VRWebBrowser>();
                    if (webPanel != null)
                    {
                        webPanel.SetUrl("https://www.google.com");
                    }
                }
                webPanelName = "";
                webPanelAtom = null;
                webPanel = null;

                UpdateAll();
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        public void PlayURLMusic()
        {
            webPanel.SetUrl(webPanelURL.val);
            UpdateWebPanelVisibility();
            wantsToOpenURL = false;
        }

        void OnDestroy()
        {
            try
            {
                if (headAudioSource) headAudioSource.Stop();
                if (audioSource) audioSource.Stop();
                if (webPanel != null)
                {
                    isPlayingURLMusic = false;
                    webPanel.SetUrl("https://www.google.com");
                    UpdateWebPanelVisibility(true);
                }
            }

            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }

        }

        public void Update()
        {
            if (webPageAudio.val)
            {
                isPlayingURLMusic = true;

                if ((audioSource != null && audioSource.isPlaying) || (headAudioSource != null && headAudioSource.isPlaying))
                {
                    if (audioSource != null) audioSource.Stop();
                    if (headAudioSource != null) headAudioSource.Stop();
                    if (webPanelAtom != null && webPanel != null)
                    {
                        //switching back, start playing again
                        PlayURLMusic();
                    }
                }

                if (!creatingWebPanel)
                {
                    creatingWebPanel = true;

                    webPanelName = "EasyBgSoundWebPanelFromAtom" + containingAtom.name;

                    if (SuperController.singleton.GetAtomByUid(webPanelName) == null)
                    {
                        StartCoroutine(CreateWebPanel());
                    }
                    else
                    {
                        webPanelCreated = true;
                    }
                } else
                {
                    if (webPanelCreated)
                    {
                        if (webPanel == null)
                        {
                            webPanelAtom = SuperController.singleton.GetAtomByUid(webPanelName);
                            if (webPanelAtom != null)
                            {
                                webPanel = webPanelAtom.GetComponentInChildren<VRWebBrowser>();
                                if (webPanel != null)
                                {
                                    PlayURLMusic();
                                }
                            }
                        } else
                        {
                            if (wantsToOpenURL)
                            {
                                PlayURLMusic();
                            }
                        }
                    }
                }
            } else
            {
                if (isPlayingURLMusic && webPanel != null)
                {
                    isPlayingURLMusic = false;
                    webPanel.SetUrl("https://www.google.com");
                    UpdateWebPanelVisibility(true);

                    //and update the current volume
                    lastVolume = volume.val;
                    if (audioSource != null)
                    {
                        audioSource.volume = lastVolume;
                    }
                    if (headAudioSource) headAudioSource.volume = lastVolume;
                    UpdateAll();
                }
                if (!creatingAudioSource)
                {
                    creatingAudioSource = true;

                    audioSourceName = "EasyBgSoundAudioFromAtom" + containingAtom.name;

                    if (PlaybackUsesHeadAudioSource())
                    {
                        audioSourceCreated = true;
                    }
                    else if (SuperController.singleton.GetAtomByUid(audioSourceName) == null)
                    {
                        StartCoroutine(CreateAudioSource());
                    }
                    else
                    {
                        audioSourceCreated = true;
                    }
                } else
                {

                    if (audioSourceCreated)
                    {
                        if (!headAudio.val && spawnDedicatedAudioAtom.val && audioSource == null)
                        {
                            audioSourceAtom = SuperController.singleton.GetAtomByUid(audioSourceName);

                            if (audioSourceAtom != null)
                            {
                                if (logMessages) SuperController.LogMessage("found " + audioSourceName);
                                audioSource = audioSourceAtom.GetComponentInChildren<AudioSource>();
                                audioSource.Stop();
                                UpdateAll();
                                if (logMessages) SuperController.LogMessage(audioSourceAtom.name + " " + audioSource);
                            }
                        }

                        if (PlaybackUsesHeadAudioSource())
                        {
                            if (headAudioSource == null)
                            {
                                JSONStorable headAudioJSON = containingAtom.GetStorableByID("HeadAudioSource");
                                if (headAudioJSON != null)
                                {
                                    headAudioSource = headAudioJSON.gameObject.GetComponentInChildren<AudioSource>();
                                    if (headAudioSource == null)
                                    {
                                        SuperController.LogError("Attach this plugin to a person or turn off 'Use Parent Person's Head Audio Source'.");
                                    } else
                                    {
                                        headAudioSource.volume = volume.val;
                                        headAudioSource.Stop();
                                        UpdateAll();
                                    }
                                } else
                                {
                                    SuperController.LogError("Attach this plugin to a person or turn off 'Use Parent Person's Head Audio Source'.");
                                }
                            } else
                            {
                                //PLAY FROM HEAD AUDIO SOURCE
                                if (lastVolume != volume.val)
                                {
                                    lastVolume = volume.val;
                                    headAudioSource.volume = lastVolume;
                                }

                                if (!headAudioSource.isPlaying && !SuperController.singleton.freezeAnimation)
                                {
                                    headAudioSource.volume = volume.val;
                                    headAudioSource.loop = false;

                                    if (backgroundMusicChooser.val == randomName)
                                    {
                                        //next clip
                                        audioClip = backgroundMusicClips[UnityEngine.Random.Range(0, backgroundMusicClips.Count)];
                                    }
                                    else if (clipIndexIsMusicClip)
                                    {
                                        audioClip = backgroundMusicClips[clipIndex];
                                    }
                                    else
                                    {
                                        audioClip = otherClips[clipIndex];
                                    }

                                    headAudioSource.clip = audioClip.clipToPlay;
                                    headAudioSource.Play();
                                }
                            }
                        } else if (spawnDedicatedAudioAtom.val && audioSource != null)
                        {
                            //PLAY FROM CREATED AUDIO SOURCE
                            if (lastVolume != volume.val)
                            {
                                lastVolume = volume.val;
                                audioSource.volume = lastVolume;
                            }

                            if (!audioSource.isPlaying && !SuperController.singleton.freezeAnimation)
                            {
                                audioSource.volume = volume.val;
                                audioSource.loop = false;
                                
                                UpdateAll();

                                if (backgroundMusicChooser.val == randomName)
                                {
                                    //next clip
                                    audioClip = backgroundMusicClips[UnityEngine.Random.Range(0, backgroundMusicClips.Count)];
                                }
                                else if (clipIndexIsMusicClip)
                                {
                                    audioClip = backgroundMusicClips[clipIndex];
                                }
                                else
                                {
                                    audioClip = otherClips[clipIndex];
                                }

                                audioSource.clip = audioClip.clipToPlay;
                                audioSource.Play();
                            }
                        }
                    }
                }
            }
        }
    }

}