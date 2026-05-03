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
        public const string pluginVersion = "1.5.2";
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
        JSONStorableFloat minSecondsBetweenMoans;
        float lastQueuedHeadMoanTime = -500f;
        geesp0t.EasyMoanCycleForce easyMoanCycleForce = null;

        GameObject playerHands;

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


                collisionThreshold = new JSONStorableFloat("Impact Force Required for Sound", 0.48f, 0, 2.0f, true);
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

                minSecondsBetweenMoans = new JSONStorableFloat(
                    "Min Seconds Between Moan Sounds",
                    1.15f,
                    0f,
                    30f,
                    false);
                minSecondsBetweenMoans.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(minSecondsBetweenMoans);
                CreateSlider(minSecondsBetweenMoans, false);

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
                       expressionBank.ClearUIList();
                       expressionBank.openLoadFolder();
                   });

                actionSelector = new AtomActionSelector();
                actionSelector.SetupActionCallback(this);

                her = this.containingAtom;
                headAudio = her.GetStorableByID("HeadAudioSource") as AudioSourceControl;
                headAudio.spatialize = false;
                headAudio.volume = 1.0f;
                headAudio.spatialBlend = 0.5f;

                try
                {
                    hitAudioclips = AudioBulk.LoadFolder(SPANK_AUDIO_DIR);
                    softVoiceAudioclips = AudioBulk.LoadFolder(SOFT_VOICE_AUDIO_DIR);
                    loudVoiceAudioclips = AudioBulk.LoadFolder(LOUD_VOICE_AUDIO_DIR);
                }
                catch(Exception e)// fallback to the local folder
                {
                    SetupLoadPath(SuperController.singleton.currentLoadDir);
                    hitAudioclips = AudioBulk.LoadFolder(SPANK_AUDIO_DIR);

                    /*softVoiceAudioclips = loudVoiceAudioclips =
                    EmbeddedAudioClipManager.singleton.GetCategoryClips("FemaleMoan")
                    .Where(n => n.sourceClip.name.StartsWith("Pixie")).ToList();*/

                    softVoiceAudioclips = AudioBulk.LoadFolder(SOFT_VOICE_AUDIO_DIR);
                    loudVoiceAudioclips = AudioBulk.LoadFolder(LOUD_VOICE_AUDIO_DIR);
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

        /// <summary>One <see cref="TriggerCollide"/> per GameObject; all Spankings handlers share it and filter by struck Person.</summary>
        private void HookCollideOnGameObject(GameObject go)
        {
            if (go == null)
                return;
            TriggerCollide tc = go.GetComponent<TriggerCollide>();
            if (tc == null)
                tc = go.AddComponent<TriggerCollide>();
            tc.OnCollide -= ObserveSpankTrigger;
            tc.OnCollide += ObserveSpankTrigger;
        }

        private void HookCollideOnRigidbody(Rigidbody rb)
        {
            if (rb == null)
                return;
            HookCollideOnGameObject(rb.gameObject);
        }

        private static Atom ResolveOwningPersonFromCollider(Collider col)
        {
            if (col == null)
                return null;
            Transform t = col.transform;
            while (t != null)
            {
                Atom a = t.GetComponent<Atom>();
                if (a != null)
                {
                    if (a.type == "Person")
                        return a;
                    if (a.parentAtom != null && a.parentAtom.type == "Person")
                        return a.parentAtom;
                }

                FreeControllerV3 fc = t.GetComponent<FreeControllerV3>();
                if (fc != null && fc.containingAtom != null)
                {
                    Atom ca = fc.containingAtom;
                    if (ca.type == "Person")
                        return ca;
                    if (ca.parentAtom != null && ca.parentAtom.type == "Person")
                        return ca.parentAtom;
                }

                t = t.parent;
            }

            return null;
        }

        private Atom ResolveStruckPersonFromEvent(TriggerEventArgs e)
        {
            if (e == null)
                return null;
            Atom struck = ResolveOwningPersonFromCollider(e.collider);
            if (struck != null)
                return struck;
            if (e.collision == null)
                return null;
            ContactPoint[] pts = e.collision.contacts;
            if (pts == null || pts.Length == 0)
                return null;
            for (int i = 0; i < pts.Length; i++)
            {
                struck = ResolveOwningPersonFromCollider(pts[i].otherCollider);
                if (struck != null)
                    return struck;
                struck = ResolveOwningPersonFromCollider(pts[i].thisCollider);
                if (struck != null)
                    return struck;
            }
            return null;
        }

        private void UnhookObserveFromGameObject(GameObject go)
        {
            if (go == null)
                return;
            TriggerCollide[] tcs = go.GetComponents<TriggerCollide>();
            if (tcs == null)
                return;
            for (int i = 0; i < tcs.Length; i++)
            {
                if (tcs[i] != null)
                    tcs[i].OnCollide -= ObserveSpankTrigger;
            }
        }

        private void UnhookObserveFromAllKnownColliders()
        {
            playerHands = GameObject.Find("Hands");
            if (playerHands)
            {
                playerHands
                    .GetComponentsInChildren<Rigidbody>()
                    .Where(f => f.name == "R_Finger_Middle_C"
                             || f.name == "L_Finger_Middle_C").ToList().ForEach(f =>
                             {
                                 UnhookObserveFromGameObject(f.gameObject);
                             });
            }

            GetSceneAtoms()
                .Where(a => a.type == "Person")
                .ToList().ForEach(p => p.rigidbodies
                   .Where(h =>
                      h.name == "rHand"
                   || h.name == "lHand").ToList().ForEach(h => {
                       UnhookObserveFromGameObject(h.gameObject);
                   }));

            GetSceneAtoms()
                .Where(a => a.category == "Toys")
                .ToList().ForEach(h => {
                    if (h.rigidbodies.Length > 0)
                    {
                        foreach (Rigidbody rigidbody in h.rigidbodies)
                        {
                            UnhookObserveFromGameObject(rigidbody.gameObject);
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
                            UnhookObserveFromGameObject(rigidbody.gameObject);
                        }
                    }
                });

            GetSceneAtoms()
                .Where(a => a.name.StartsWith("IS"))
                .ToList().ForEach(h => {
                    if (h.rigidbodies.Length > 0)
                    {
                        foreach (Rigidbody rigidbody in h.rigidbodies)
                        {
                            UnhookObserveFromGameObject(rigidbody.gameObject);
                        }
                    }
                });

            if (collideWithCustomUnityAssets.val)
            {
                GetSceneAtoms()
                    .Where(a => a.type == "CustomUnityAsset")
                    .ToList().ForEach(h => {
                        foreach (Transform transform in h.GetComponentsInChildren<Transform>())
                        {
                            if (transform.GetComponent<Collider>() || transform.GetComponent<MeshCollider>() || transform.GetComponent<CapsuleCollider>() || transform.GetComponent<BoxCollider>() || transform.GetComponent<NonConvexMeshCollider>() || transform.GetComponent<SphereCollider>())
                            {
                                UnhookObserveFromGameObject(transform.gameObject);
                            }
                        }
                        if (h.rigidbodies.Length > 0)
                        {
                            foreach (Rigidbody rigidbody in h.rigidbodies)
                            {
                                UnhookObserveFromGameObject(rigidbody.gameObject);
                            }
                        }
                    });
            }
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
                                 HookCollideOnRigidbody(f);
                             });
            }

            GetSceneAtoms()
                .Where(a => a.type == "Person")
                .ToList().ForEach(p => p.rigidbodies
                   .Where(h =>
                      h.name == "rHand"
                   || h.name == "lHand").ToList().ForEach(h => {
                       HookCollideOnRigidbody(h);
                   }));

            GetSceneAtoms()
                .Where(a => a.category == "Toys")
                .ToList().ForEach(h => {
                    if (h.rigidbodies.Length > 0) { 
                        foreach (Rigidbody rigidbody in h.rigidbodies) { 
                            HookCollideOnRigidbody(rigidbody);
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
                            HookCollideOnRigidbody(rigidbody);
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
                            HookCollideOnRigidbody(rigidbody);
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
                                HookCollideOnGameObject(transform.gameObject);
                            }
                        }
                        if (h.rigidbodies.Length > 0)
                        {
                            foreach (Rigidbody rigidbody in h.rigidbodies)
                            {
                                //SuperController.LogMessage("Found customunityasset rigidbody: " + rigidbody.name);
                                HookCollideOnRigidbody(rigidbody);
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
                expressionBank.ClearUIList();
                expressionBank.loadExpressionFolder(EXPRESSION_DIR);

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

        /// <summary>
        /// Plays moan only when head audio can take it or high-priority clears it,
        /// and not faster than minimum interval — stops cycle-force butt bounce loops.
        /// </summary>
        void TryPlayHeadMoan(RandomAudio voiceRand, float volumeMultiplier)
        {
            if (headAudio == null || headAudio.audioSource == null || voiceRand == null)
                return;
            float now = Time.timeSinceLevelLoad;
            if (now - lastQueuedHeadMoanTime < minSecondsBetweenMoans.val)
                return;

            voiceRand.playNow = playMoansHighPriority.val;
            bool queued = voiceRand.playRandomDelayedIfClear(headAudio.audioSource,
                    volumeMultiplier,
                    0.1f,
                    0.8f);
            if (queued)
            {
                lastQueuedHeadMoanTime = now;
                return;
            }

            AudioSource hs = headAudio.audioSource;
            if (hs != null && hs.isPlaying && !playMoansHighPriority.val)
                lastQueuedHeadMoanTime = now;
        }

        void spankTrigger()
        {
            //base.StartCoroutine();
        }

        void OnDestroy()
        {
            try
            {
                UnhookObserveFromAllKnownColliders();
            }
            catch (Exception e)
            {
                SuperController.LogError("Spankings OnDestroy unhook: " + e.Message);
            }
        }

        public void HardSpank()
        {
            float level = 1.5f;

            randAudio.playRandom(hitAudioSourceControl.audioSource, level * collisionSoundVolume.val);

            if (useExpressions.val) expressionBank.PlayRandomAction(true); // << needs a quick lerp here

            TryPlayHeadMoan(randLoudVoiceAudio, level * collisionSoundVolume.val);
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

            if (useExpressions.val) expressionBank.PlayRandomAction(true); // << needs a quick lerp here

            TryPlayHeadMoan(randSoftVoiceAudio, level * collisionSoundVolume.val);
            arousal += 0.25f;

            reaction.restart();
            actionSelector.CallAction();
        }

        void ObserveSpankTrigger(object sender, TriggerEventArgs e)
        {
            //SuperController.LogError("Receiving event " + e.collider.name);
            if (e.evtType == EventType.HIT)
            {
                Atom struck = ResolveStruckPersonFromEvent(e);
                if (struck != her)
                    return;
                if (e.collision == null)
                    return;

                float level = e.collision.relativeVelocity.magnitude / 3.0f;
                 if (level > collisionThreshold.val)
                {
                    //SuperController.LogMessage("collide at high level: " + level + ", threshold: " + collisionThreshold.val);
                    if (e.insideTrigger)
                    {
                        //SuperController.LogMessage("Collide with trigger");
                        randAudio.playRandom(hitAudioSourceControl.audioSource, level * collisionSoundVolume.val);

                        if (useExpressions.val) expressionBank.PlayRandomAction(true); // << needs a quick lerp here

                        if (level > 1.0f)
                        {
                            TryPlayHeadMoan(randLoudVoiceAudio, level * collisionSoundVolume.val);
                            reaction.SetForceAxis(axis[(++crntAxis + 1) % axis.Length]);
                            reaction.restart();
                            arousal += 0.5f;
                        }
                        else
                        {
                            TryPlayHeadMoan(randSoftVoiceAudio, level * collisionSoundVolume.val);
                            arousal += 0.25f;
                        }

                        reaction.restart();
                        actionSelector.CallAction();
                    }
                    else
                    {
                        //SuperController.LogMessage("Collide with other object");
                        if (!collideOnlyWithTriggers.val)
                        {
                            randAudio.playRandom(hitAudioSourceControl.audioSource, level * collisionSoundVolume.val);
                            if (useExpressions.val) expressionBank.PlayRandomAction(true);
                            if (level > 1.0f)
                            {
                                TryPlayHeadMoan(randLoudVoiceAudio, level * collisionSoundVolume.val);
                                arousal += 0.5f;
                            }
                            else
                            {
                                TryPlayHeadMoan(randSoftVoiceAudio, level * collisionSoundVolume.val);
                                arousal += 0.25f;
                            }
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



