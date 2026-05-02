/*
Doc.0c's Spank Machine
    Sounds, movement, expression & feedback
    when the lady's butt gets interacted with by a VR hand.

    1.1 had haptic feedback for the SteamVRController
    1.2 - The hand of a person can also collide
        - big butt will require some adjustement of the spherescolliders
        - haptic feedback is off and should be back in a next release
    1.3 - trigger actions
    1.4 - vam 1.18 compat
        - fixed the damp.cs keeping update on exit
        - pitchshift slider to adjust the voice height
        - haptic feedback still's off

Credits to MeshedVR, MacGruber, Physis, VamDeluxe, WadeVRX
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DeluxePlugin.Emotion;
using UnityEngine;

namespace octopussy
{
    public class Spankings : MVRScript
    {
        public const string pluginAuthor = "octopussy (mod by geesp0t)";
        public const string pluginName = "Spankings";
        public const string pluginVersion = "1.6";
        public const string pluginDate = "[2020-03-03]";
        public const string pluginDescription = @"
        Gently manages sound effects, movement and feedback
        when the lady's butt gets interacted with
        by a VR or Player's hand.
        ";

        SuperController SC;

        static string AssetPath = "Custom/Scripts/Spankings";
        static string LOUD_VOICE_AUDIO_DIR;
        static string SOFT_VOICE_AUDIO_DIR;
        static string SPANK_AUDIO_DIR;
        static string EXPRESSION_DIR;
        string lastLoadPath = SPANK_AUDIO_DIR;

        public void SetupLoadPath(string basedir)
        // can be local to the vac, or using the asset folder
        {
            LOUD_VOICE_AUDIO_DIR = basedir + "/audio/moan/loud";
            SOFT_VOICE_AUDIO_DIR = basedir + "/audio/moan/soft";
            SPANK_AUDIO_DIR = basedir + "/audio/spanks";
            EXPRESSION_DIR = basedir + "/expressions/spanx";
            lastLoadPath = SPANK_AUDIO_DIR;
        }

        List<NamedAudioClip> hitAudioclips;
        List<NamedAudioClip> softVoiceAudioclips;
        List<NamedAudioClip> loudVoiceAudioclips;

        AudioSourceControl hitAudioSourceControl;
        AudioSourceControl headAudio;
        RandomAudio randAudio;
        RandomAudio randSoftVoiceAudio;
        RandomAudio randLoudVoiceAudio;

        JSONStorableFloat pitchshift;
        JSONStorableFloat collisionThreshold;
        JSONStorableFloat collisionSoundVolume;
        JSONStorableBool playMoansHighPriority;
        JSONStorableBool useExpressions;
        JSONStorableFloat expressionIntensity;
        JSONStorableBool collideOnlyWithTriggers;
        JSONStorableBool collideWithCustomUnityAssets;

        protected JSONStorableString arousalString;
        protected JSONStorableString explanationString;
        private UIDynamicButton createCycleForceButton;

        Atom her;
        Atom ALCheek;
        Atom ARCheek;
        Atom AHitAudio;

        CycleForceOnce reaction; // shake it baby
        ForceProducerV2.AxisName[] axis = {
            ForceProducerV2.AxisName.X,  ForceProducerV2.AxisName.NegX,
            ForceProducerV2.AxisName.Y,  ForceProducerV2.AxisName.NegY,
            ForceProducerV2.AxisName.Z,  ForceProducerV2.AxisName.NegZ
        };
        int crntAxis = 0; // randomized axis

        ExpressionBank expressionBank;

        public string action;
        AtomActionSelector actionSelector;

        float arousal = 0;
        geesp0t.EasyMoanCycleForce easyMoanCycleForce = null;

        GameObject playerHands;

        /// <summary>When true, moan/voice folders are not loaded, expressions are not loaded, and only spank + cheek + hit audio run.</summary>
        private bool _isMalePerson;

        private bool CanPlaySoftMoan()
        {
            return !_isMalePerson && softVoiceAudioclips != null && softVoiceAudioclips.Count > 0;
        }

        private bool CanPlayLoudMoan()
        {
            return !_isMalePerson && loudVoiceAudioclips != null && loudVoiceAudioclips.Count > 0;
        }

        private bool CanPlayExpressionNow()
        {
            return !_isMalePerson && useExpressions.val && expressionBank != null;
        }

        public static T FindInPlugin<T>(MVRScript self) where T : MVRScript // thanks mcgruber
        {
            int i = self.name.IndexOf('_');
            if (i < 0)
                return null;
            string prefix = self.name.Substring(0, i + 1);
            string scriptName = prefix + typeof(T).FullName;
            return self.containingAtom.GetStorableByID(scriptName) as T;
        }

        protected virtual IEnumerator CreateAtom(string atomType, string atomId, Action<Atom> onAtomCreated)
        {
            Atom atom = SuperController.singleton.GetAtomByUid(atomId);
            if (atom == null)
            {
                yield return SuperController.singleton.AddAtomByType(atomType, atomId);
                atom = SuperController.singleton.GetAtomByUid(atomId);
            }
            if (atom != null)
            {
                onAtomCreated(atom);
            }
        }

        public void parentLinkAtPosition(Atom a, Atom parent, JSONStorable toPosition, string toPositionName)
        {
            a.parentAtom = parent;
            Vector3 p = toPosition.transform.position;
            a.mainController.currentPositionState = FreeControllerV3.PositionState.On;
            a.mainController.transform.position = new Vector3(p.x, p.y, p.z);
            a.mainController.SetLinkToAtom(parent.uid);
            a.mainController.SetLinkToRigidbodyObject(toPositionName);
            a.mainController.currentPositionState = FreeControllerV3.PositionState.ParentLink;
            a.mainController.canGrabPosition = false;
            a.mainController.canGrabRotation = false;
        }

        public override void Init()
        {
            try
            {
                pluginLabelJSON.val = "Spankings";
                SC = SuperController.singleton;

                SetupLoadPath(AssetPath);

                createCycleForceButton = CreateButton("Create / Refresh Cycle Force Movement", true);

                CreateButton("Simulate Soft Spank", true).button.onClick.AddListener(
                    () => SoftSpank());

                CreateButton("Simulate Hard Spank", true).button.onClick.AddListener(
                    () => HardSpank());

                arousalString = new JSONStorableString("", "");
                CreateTextField(arousalString, true);

                explanationString = new JSONStorableString("", "If you press Create / Refresh Cycle Force Movement or configure the Easy Moan Cycle Force -> Custom UI..., then spanking will increase Cycle Force movement / cause hip thrusting.");
                CreateTextField(explanationString, true);


                collisionThreshold = new JSONStorableFloat("Impact Force Required for Sound", 0.25f, 0, 1.0f, true);
                collisionThreshold.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(collisionThreshold);
                CreateSlider(collisionThreshold, false);

                collisionSoundVolume = new JSONStorableFloat("Impact Sound Volume Multiplier", 1.0f, 0, 100.0f, true);
                collisionSoundVolume.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(collisionSoundVolume);
                CreateSlider(collisionSoundVolume, false);

                playMoansHighPriority = new JSONStorableBool("Moan Sounds Stop Current Sound to Play", true);
                playMoansHighPriority.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(playMoansHighPriority);
                CreateToggle(playMoansHighPriority, false);

                collideOnlyWithTriggers = new JSONStorableBool("Spanking Only (Ass Triggers Collide)", false);
                collideOnlyWithTriggers.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(collideOnlyWithTriggers);
                CreateToggle(collideOnlyWithTriggers, false);

                CreateButton("Refresh Colliders Now (New Atoms)").button.onClick.AddListener(() => RefreshColliders());

                collideWithCustomUnityAssets = new JSONStorableBool("CustomUnityAssets Trigger Collision", true);
                collideWithCustomUnityAssets.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(collideWithCustomUnityAssets);
                CreateToggle(collideWithCustomUnityAssets, false).toggle.onValueChanged.AddListener(delegate { RefreshColliders(); });
                
                useExpressions = new JSONStorableBool("Use Expressions", true);
                useExpressions.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(useExpressions);
                CreateToggle(useExpressions, false);

                expressionIntensity = new JSONStorableFloat("Expression Intensity",
                    0.75f, f => { if (expressionBank != null) expressionBank.morphIntensity = f;  },
                    0, 1.0f, true);
                expressionIntensity.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(expressionIntensity);
                CreateSlider(expressionIntensity, false);

                CreateButton("Load spank Audio (Folder)").button.onClick.AddListener(
                    () => AudioBulk.OpenLoadFolder(hitAudioclips, lastLoadPath));
                CreateButton("Load soft voice Audio Folder").button.onClick.AddListener(
                    () => AudioBulk.OpenLoadFolder(softVoiceAudioclips, lastLoadPath));
                CreateButton("Load loud voice Audio Folder").button.onClick.AddListener(
                    () => AudioBulk.OpenLoadFolder(loudVoiceAudioclips, lastLoadPath));

                CreateButton("Load expression Folder").button.onClick.AddListener(
                   () => {
                       if (_isMalePerson)
                           return;
                       if (expressionBank == null)
                           return;
                       expressionBank.ClearUIList();
                       expressionBank.openLoadFolder();
                   });

                actionSelector = new AtomActionSelector();
                actionSelector.SetupActionCallback(this);

                her = this.containingAtom;
                DAZCharacter dazCharacter = her != null ? her.GetComponentInChildren<DAZCharacter>() : null;
                _isMalePerson = dazCharacter != null && dazCharacter.isMale;

                headAudio = her.GetStorableByID("HeadAudioSource") as AudioSourceControl;
                headAudio.spatialize = false;
                headAudio.volume = 1.0f;
                headAudio.spatialBlend = 0.5f;

                try
                {
                    hitAudioclips = AudioBulk.LoadFolder(SPANK_AUDIO_DIR);
                    if (_isMalePerson)
                    {
                        softVoiceAudioclips = new List<NamedAudioClip>();
                        loudVoiceAudioclips = new List<NamedAudioClip>();
                    }
                    else
                    {
                        softVoiceAudioclips = AudioBulk.LoadFolderSkippingLeafNameSubstring(SOFT_VOICE_AUDIO_DIR, "", "breath");
                        loudVoiceAudioclips = AudioBulk.LoadFolderSkippingLeafNameSubstring(LOUD_VOICE_AUDIO_DIR, "", "breath");
                    }
                }
                catch (Exception e)// fallback to the local folder
                {
                    SetupLoadPath(SuperController.singleton.currentLoadDir);
                    hitAudioclips = AudioBulk.LoadFolder(SPANK_AUDIO_DIR);

                    /*softVoiceAudioclips = loudVoiceAudioclips =
                    EmbeddedAudioClipManager.singleton.GetCategoryClips("FemaleMoan")
                    .Where(n => n.sourceClip.name.StartsWith("Pixie")).ToList();*/

                    if (_isMalePerson)
                    {
                        softVoiceAudioclips = new List<NamedAudioClip>();
                        loudVoiceAudioclips = new List<NamedAudioClip>();
                    }
                    else
                    {
                        softVoiceAudioclips = AudioBulk.LoadFolderSkippingLeafNameSubstring(SOFT_VOICE_AUDIO_DIR, "", "breath");
                        loudVoiceAudioclips = AudioBulk.LoadFolderSkippingLeafNameSubstring(LOUD_VOICE_AUDIO_DIR, "", "breath");
                    }
                }

                randAudio = new RandomAudio(hitAudioclips);
                randSoftVoiceAudio = new RandomAudio(softVoiceAudioclips);
                randLoudVoiceAudio = new RandomAudio(loudVoiceAudioclips);

                pitchshift = new JSONStorableFloat("voice pitch shift", 
                    0f,f =>randLoudVoiceAudio.pitchshift = randSoftVoiceAudio.pitchshift = f,
                    -0.3f, 0.3f, true);

                pitchshift.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(pitchshift);
                CreateSlider(pitchshift, false);


                ALCheek = SC.GetAtomByUid("CheekLeft");
                ARCheek = SC.GetAtomByUid("CheekRight");
                
                JSONStorable rThigh = her.containingAtom.GetStorableByID("rThigh");
                JSONStorable lThigh = her.containingAtom.GetStorableByID("lThigh");
                JSONStorable hip = her.containingAtom.GetStorableByID("hip");

                

                if (ARCheek == null)
                    base.StartCoroutine(CreateAtom("CollisionTrigger", "CheekRight", newAtom =>
                    {
                        
                        parentLinkAtPosition(newAtom, her.containingAtom, rThigh, "rThigh");
                        newAtom.GetStorableByID("scale").SetFloatParamValue("scale", 1.25f);
                        ARCheek = newAtom;
                    }));
                else
                {
                    /*CollisionTrigger t = (ARCheek as JSONStorable) as CollisionTrigger;
                    SuperController.LogError("X" + ARCheek.uid);
                    SuperController.LogError("X"+t);*/
                }

                if (ALCheek == null)
                    base.StartCoroutine(CreateAtom("CollisionTrigger", "CheekLeft", newAtom =>
                    {
                        parentLinkAtPosition(newAtom, her.containingAtom, lThigh, "lThigh");
                        newAtom.GetStorableByID("scale").SetFloatParamValue("scale", 1.25f);
                        ALCheek = newAtom;
                    }));
                else
                {
                    //collisionFilter(ALCheek, her);
                }

                if (AHitAudio == null)
                    base.StartCoroutine(CreateAtom("AudioSource", "HitAudioSource", newAtom =>
                    {
                        parentLinkAtPosition(newAtom, her.containingAtom, hip, "hip");
                        AHitAudio = newAtom;
                        hitAudioSourceControl = AHitAudio.GetStorableByID("AudioSource") as AudioSourceControl;
                        hitAudioSourceControl.spatialize = false;
                        hitAudioSourceControl.volume = 1.0f;
                        hitAudioSourceControl.pitch = 1.0f;
                    }));
                else
                {
                    hitAudioSourceControl = AHitAudio.GetStorableByID("AudioSource") as AudioSourceControl;
                    hitAudioSourceControl.volume = 1.0f;
                    hitAudioSourceControl.pitch = 1.0f;
                }


            }
            catch (Exception e)
            {
                SuperController.LogError("Error in Spank Init " + e);
            }
        }

        public void Update()
        {
            arousalString.val = string.Format("Arousal from Spanking: {0:P}", arousal);
            if (easyMoanCycleForce != null)
            {
                if (arousal > 1.0f) arousal = 1.0f;
                else arousal -= Time.deltaTime * 0.05f;

                if (arousal < 0) arousal = 0;

                easyMoanCycleForce.arousal = arousal;
            }
        }

        void collisionFilter(CollisionTrigger col, Atom atom)
        {
            CollisionTrigger c = col;
            SuperController.LogError("?" + c);
            //col.enabled = false;
            c.invertAtomFilter = true;
            c.atomFilterPopup.currentValue = her.uid;
        }

        public void RefreshColliders()
        {
            playerHands = GameObject.Find("Hands");
            if (playerHands)
            {// only the middle finger is the actual collider
                playerHands
                    .GetComponentsInChildren<Rigidbody>()
                    .Where(f => f.name == "R_Finger_Middle_C"
                             || f.name == "L_Finger_Middle_C").ToList().ForEach(f => {
                                 f.gameObject.AddComponent<TriggerCollide>().OnCollide += ObserveSpankTrigger;
                             });
            }

            GetSceneAtoms()
                .Where(a => a.type == "Person")
                .ToList().ForEach(p => p.rigidbodies
                   .Where(h =>
                      h.name == "rHand"
                   || h.name == "lHand").ToList().ForEach(h => {
                       h.gameObject.AddComponent<TriggerCollide>().OnCollide += ObserveSpankTrigger;
                   }));

            GetSceneAtoms()
                .Where(a => a.category == "Toys")
                .ToList().ForEach(h => {
                    if (h.rigidbodies.Length > 0) { 
                        foreach (Rigidbody rigidbody in h.rigidbodies) { 
                            rigidbody.gameObject.AddComponent<TriggerCollide>().OnCollide += ObserveSpankTrigger;
                        }
                    }
                });

            GetSceneAtoms()
                .Where(a => a.category == "Shapes")
                .ToList().ForEach(h => {
                    if (h.rigidbodies.Length > 0)
                    {
                        foreach (Rigidbody rigidbody in h.rigidbodies)
                        {
                            rigidbody.gameObject.AddComponent<TriggerCollide>().OnCollide += ObserveSpankTrigger;
                        }
                    }
                });

            //some shapes don't have category but begin with IS, this may of course add other objects as triggers, but not sure if that's a big deal, in fact, it can be useful if documented
            GetSceneAtoms()
                .Where(a => a.name.StartsWith("IS"))
                .ToList().ForEach(h => {
                    if (h.rigidbodies.Length > 0)
                    {
                        foreach (Rigidbody rigidbody in h.rigidbodies)
                        {
                            rigidbody.gameObject.AddComponent<TriggerCollide>().OnCollide += ObserveSpankTrigger;
                        }
                    }
                });

            if (collideWithCustomUnityAssets.val)
            {
                GetSceneAtoms()
                    .Where(a => a.type == "CustomUnityAsset")
                    .ToList().ForEach(h => {
                        //SuperController.LogMessage("Found customunityasset: " + h.gameObject.name);
                        foreach (Transform transform in h.GetComponentsInChildren<Transform>())
                        {
                            if (transform.GetComponent<Collider>() || transform.GetComponent<MeshCollider>() || transform.GetComponent<CapsuleCollider>() || transform.GetComponent<BoxCollider>() || transform.GetComponent<NonConvexMeshCollider>() || transform.GetComponent<SphereCollider>())
                            {
                                //SuperController.LogMessage("Found customunityasset collider: " + transform.gameObject.name);
                                transform.gameObject.AddComponent<TriggerCollide>().OnCollide += ObserveSpankTrigger;
                            }
                        }
                        if (h.rigidbodies.Length > 0)
                        {
                            foreach (Rigidbody rigidbody in h.rigidbodies)
                            {
                                //SuperController.LogMessage("Found customunityasset rigidbody: " + rigidbody.name);
                                rigidbody.gameObject.AddComponent<TriggerCollide>().OnCollide += ObserveSpankTrigger;
                            }
                        }
                    });
            }
        }

        public void Start()
        {
            try
            {
                SC.commonHandModelControl.useCollision = true;

                expressionBank = FindInPlugin<ExpressionBank>(this);
                if (!_isMalePerson && expressionBank != null)
                {
                    expressionBank.ClearUIList();
                    expressionBank.loadExpressionFolder(EXPRESSION_DIR);
                }

                reaction = FindInPlugin<CycleForceOnce>(this);
                reaction.SyncReceiver("hip");
                
                if (easyMoanCycleForce == null)
                {
                    easyMoanCycleForce = FindInPlugin<geesp0t.EasyMoanCycleForce>(this);
                    createCycleForceButton.button.onClick.AddListener(
                        () => easyMoanCycleForce.CreateCycleForceIfNeeded());
                }

                RefreshColliders();
            }
            catch (Exception e)
            {
                SuperController.LogError("" + e);
            }
        }

        void spankTrigger()
        {
            //base.StartCoroutine();
        }

        void OnDestroy()
        {
            playerHands = GameObject.Find("Hands");
            if (playerHands)
            {  // only the middle finger is the actual collider
                playerHands
                    .GetComponentsInChildren<Rigidbody>()
                    .Where(f => f.name == "R_Finger_Middle_C"
                             || f.name == "L_Finger_Middle_C").ToList().ForEach(f =>
                             {
                                 f.gameObject.AddComponent<TriggerCollide>().OnCollide -= ObserveSpankTrigger;
                             });
            }

            GetSceneAtoms()
                .Where(a => a.type == "Person")
                .ToList().ForEach(p => p.rigidbodies
                   .Where(h =>
                      h.name == "rHand"
                   || h.name == "lHand").ToList().ForEach(h => {
                       h.gameObject.AddComponent<TriggerCollide>().OnCollide -= ObserveSpankTrigger;
                   }));

            GetSceneAtoms()
                .Where(a => a.category == "Toys")
                .ToList().ForEach(h => {
                    if (h.rigidbodies.Length > 0)
                    {
                        foreach (Rigidbody rigidbody in h.rigidbodies)
                        {
                            rigidbody.gameObject.AddComponent<TriggerCollide>().OnCollide -= ObserveSpankTrigger;
                        }
                    }
                });

            GetSceneAtoms()
                .Where(a => a.category == "Shapes")
                .ToList().ForEach(h => {
                    if (h.rigidbodies.Length > 0)
                    {
                        foreach (Rigidbody rigidbody in h.rigidbodies)
                        {
                            rigidbody.gameObject.AddComponent<TriggerCollide>().OnCollide -= ObserveSpankTrigger;
                        }
                    }
                });

            //some shapes don't have category but begin with IS, this may of course add other objects as triggers, but not sure if that's a big deal, in fact, it can be useful if documented
            GetSceneAtoms()
                .Where(a => a.name.StartsWith("IS"))
                .ToList().ForEach(h => {
                    if (h.rigidbodies.Length > 0)
                    {
                        foreach (Rigidbody rigidbody in h.rigidbodies)
                        {
                            rigidbody.gameObject.AddComponent<TriggerCollide>().OnCollide -= ObserveSpankTrigger;
                        }
                    }
                });

            if (collideWithCustomUnityAssets.val)
            {
                GetSceneAtoms()
                    .Where(a => a.type == "CustomUnityAsset")
                    .ToList().ForEach(h => {
                        //SuperController.LogMessage("Found customunityasset: " + h.gameObject.name);
                        foreach (Transform transform in h.GetComponentsInChildren<Transform>())
                        {
                            if (transform.GetComponent<Collider>() || transform.GetComponent<MeshCollider>() || transform.GetComponent<CapsuleCollider>() || transform.GetComponent<BoxCollider>() || transform.GetComponent<NonConvexMeshCollider>() || transform.GetComponent<SphereCollider>())
                            {
                                //SuperController.LogMessage("Found customunityasset collider: " + transform.gameObject.name);
                                transform.gameObject.AddComponent<TriggerCollide>().OnCollide -= ObserveSpankTrigger;
                            }
                        }
                        if (h.rigidbodies.Length > 0)
                        {
                            foreach (Rigidbody rigidbody in h.rigidbodies)
                            {
                                //SuperController.LogMessage("Found customunityasset rigidbody: " + rigidbody.name);
                                rigidbody.gameObject.AddComponent<TriggerCollide>().OnCollide -= ObserveSpankTrigger;
                            }
                        }
                    });
            }
        }

        public void HardSpank()
        {
            float level = 1.5f;

            randAudio.playRandom(hitAudioSourceControl.audioSource, level * collisionSoundVolume.val);

            if (CanPlayExpressionNow()) expressionBank.PlayRandomAction(true); // << needs a quick lerp here

            if (CanPlayLoudMoan())
            {
                randLoudVoiceAudio.playNow = playMoansHighPriority.val;
                randLoudVoiceAudio.playRandomDelayedIfClear(headAudio.audioSource, level * collisionSoundVolume.val, 0.1f, 0.8f);
            }
            reaction.SetForceAxis(axis[(++crntAxis + 1) % axis.Length]);
            reaction.restart();
            arousal += 0.5f;

            reaction.restart();
            actionSelector.CallAction();
        }

        public void SoftSpank()
        {
            float level = 0.5f;

            randAudio.playRandom(hitAudioSourceControl.audioSource, level * collisionSoundVolume.val);

            if (CanPlayExpressionNow()) expressionBank.PlayRandomAction(true); // << needs a quick lerp here

            if (CanPlaySoftMoan())
            {
                randSoftVoiceAudio.playNow = playMoansHighPriority.val;
                randSoftVoiceAudio.playRandomDelayedIfClear(headAudio.audioSource, level * collisionSoundVolume.val, 0.1f, 0.8f);
            }
            arousal += 0.25f;

            reaction.restart();
            actionSelector.CallAction();
        }

        void ObserveSpankTrigger(object sender, TriggerEventArgs e)
        {
            //SuperController.LogError("Receiving event " + e.collider.name);
            if (e.evtType == EventType.HIT)
            {
                float level = e.collision.relativeVelocity.magnitude / 3.0f;
                 if (level > collisionThreshold.val)
                {
                    //SuperController.LogMessage("collide at high level: " + level + ", threshold: " + collisionThreshold.val);
                    if (e.insideTrigger)
                    {
                        //SuperController.LogMessage("Collide with trigger");
                        randAudio.playRandom(hitAudioSourceControl.audioSource, level * collisionSoundVolume.val);

                        if (CanPlayExpressionNow()) expressionBank.PlayRandomAction(true); // << needs a quick lerp here

                        if (level > 1.0f)
                        {
                            if (CanPlayLoudMoan())
                            {
                                randLoudVoiceAudio.playNow = playMoansHighPriority.val;
                                randLoudVoiceAudio.playRandomDelayedIfClear(headAudio.audioSource, level * collisionSoundVolume.val, 0.1f, 0.8f);
                            }
                            reaction.SetForceAxis(axis[(++crntAxis + 1) % axis.Length]);
                            reaction.restart();
                            arousal += 0.5f;
                        }
                        else
                        {
                            if (CanPlaySoftMoan())
                            {
                                randSoftVoiceAudio.playNow = playMoansHighPriority.val;
                                randSoftVoiceAudio.playRandomDelayedIfClear(headAudio.audioSource, level * collisionSoundVolume.val, 0.1f, 0.8f);
                            }
                            arousal += 0.25f;
                        }

                        reaction.restart();
                        actionSelector.CallAction();
                    }
                    else
                    {
                        //SuperController.LogMessage("Collide with other object");
                        if (!collideOnlyWithTriggers.val) { 
                            randAudio.playRandom(hitAudioSourceControl.audioSource, level * collisionSoundVolume.val);
                        }
                    }
                } else
                {
                    //SuperController.LogMessage("collide at low level: " + level + ", threshold: " + collisionThreshold.val);
                }
            }
            else
            {

                //SuperController.LogMessage("Non hit collide");
            }
        }
    }
}



