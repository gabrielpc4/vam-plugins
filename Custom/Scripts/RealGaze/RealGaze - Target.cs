using UnityEngine;
using System.Collections.Generic;
using System;

namespace PA4148415 {
    public class RealGazeTarget : MVRScript {
        //Script by pornaway4148415
        //This plugin is as lightweight as possible to avoid increasing load times and impacting performance.
        /*Changelog 1.9:
         * Added attributes stuff
        */
        /*TODO:
         * Nothing I think
        */

        #region Storables
        protected JSONStorableStringChooser selection;
        protected JSONStorableFloat key;
        protected JSONStorableFloat relFreq;
        protected JSONStorableFloat mirrPref;
        protected JSONStorableFloat pupilScale;
        protected JSONStorableFloat minDis;
        protected JSONStorableFloat maxDis;
        protected JSONStorableFloat givenCharKey;
        protected JSONStorableBool haveCharKey;
        protected JSONStorableFloat arouseDisgust;
        protected JSONStorableFloat moodSlider;
        protected JSONStorableFloat angrySad;
        protected JSONStorableFloat surpriseFactor;
        protected JSONStorableColor attributesA;
        protected JSONStorableColor attributesB;
        #endregion

        protected void givenKey() {
            key.val = givenCharKey.val;
        }

        public override void Init() {
            try {
                pluginLabelJSON.val = "RealGaze Target 1.9";
                #region Populate receiver choices and choose default
                List<string> receiverChoices = new List<string>();
                foreach (Rigidbody rb in containingAtom.rigidbodies) {
                    //I thought that using realRigidbodies removed the weird nonsense Rigidbodies like the many AutoColliders and the _ prefix Rigidbodies, but apparently not
                    //So I'm manually excluding them from the list. xxxControl and xxxLink are redundant but I don't care enough to remove them, maybe there's even a slight difference that somebody can make use of
                    string excluder = "AutoCollider";
                    string subStr = rb.name.Substring(0, Math.Min(rb.name.Length, 1));
                    if (subStr != "_") {
                        subStr = rb.name.Substring(0, Math.Min(rb.name.Length, excluder.Length));
                        if (subStr != excluder) {
                            receiverChoices.Add(rb.name);
                        }
                    }
                }
                string defaultRec = "control";
                if (containingAtom.type == "Person") {
                    defaultRec = "headControl";
                }
                #endregion

                #region Custom UI
                selection = new JSONStorableStringChooser("receiver", receiverChoices, defaultRec, "Receiver choice");
                RegisterStringChooser(selection);
                UIDynamicPopup udp = CreateScrollablePopup(selection, true);

                key = new JSONStorableFloat("Target key", 0f, -10.0f, 10.0f, true, true);
                RegisterFloat(key);
                CreateSlider(key, false);

                if (containingAtom.type == "Person") {
                    UIDynamicButton keyButton;
                    keyButton = CreateButton("Use given key", false);
                    keyButton.button.onClick.AddListener(givenKey);
                    CreateSpacer(true).height = 50;
                }

                CreateSpacer(true).height = 40;
                CreateSpacer(false).height = 20;

                relFreq = new JSONStorableFloat("Relative frequency", 1f, 0.0f, 5f, true, true);
                RegisterFloat(relFreq);
                CreateSlider(relFreq, false);

                mirrPref = new JSONStorableFloat("Mirror preference", 0.25f, 0.0f, 1f, true, true);
                RegisterFloat(mirrPref);
                CreateSlider(mirrPref, true);

                minDis = new JSONStorableFloat("\"Greater than\" distance", 0.3f, 0.0f, 100f, true, true);
                RegisterFloat(minDis);
                CreateSlider(minDis, false);

                maxDis = new JSONStorableFloat("\"Less than\" distance", 10f, 0.0f, 100f, true, true);
                RegisterFloat(maxDis);
                CreateSlider(maxDis, true);

                pupilScale = new JSONStorableFloat("Pupil size effect", 0.0f, -1f, 1f, true, true);
                RegisterFloat(pupilScale);
                CreateSlider(pupilScale, false);

                CreateSpacer(false).height = 10;
                CreateSpacer(true).height = 145;

                arouseDisgust = new JSONStorableFloat("Disgust/Arousal factor\n(Positive = Arousing)", 0.0f, -4f, 4f, true, true);
                RegisterFloat(arouseDisgust);
                CreateSlider(arouseDisgust, true);

                moodSlider = new JSONStorableFloat("Worsens/Improves mood\n(Positive = Improves)", 0.0f, -4f, 4f, true, true);
                RegisterFloat(moodSlider);
                CreateSlider(moodSlider, false);

                angrySad = new JSONStorableFloat("Makes Angry/Sad\n(Positive = Sad)", 0.0f, -1f, 1f, true, true);
                RegisterFloat(angrySad);
                CreateSlider(angrySad, true);

                surpriseFactor = new JSONStorableFloat("Typical/Surprising\n(Positive = Surprising)", 0.0f, -1f, 1f, true, true);
                RegisterFloat(surpriseFactor);
                CreateSlider(surpriseFactor, false);
                #endregion

                #region Non-UI JSONStorable
                attributesA = new JSONStorableColor("attributesA", new HSVColor());
                RegisterColor(attributesA);

                attributesB = new JSONStorableColor("attributesB", new HSVColor());
                RegisterColor(attributesB);

                givenCharKey = new JSONStorableFloat("givenCharKey", 0.0f, 0.0f, 10f, true, true);
                RegisterFloat(givenCharKey);

                haveCharKey = new JSONStorableBool("haveCharKey", false);
                haveCharKey.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(haveCharKey);
                #endregion

                SuperController.LogMessage("Target loaded on " + containingAtom.uid + "(Receiver: " + selection.val + ") with key " + key.val + ".");
            } catch (Exception e) {
                SuperController.LogError("Exception caught on load: " + e);
            }
        }

        #region Compile attributes
        protected void FixedUpdate() {
            //compile the various attributes into HSV color to be put into JSONStorableColors that can be accessed by the Looker script
            HSVColor a;
            a.H = moodSlider.val + 4f;
            a.S = posOnly(angrySad.val);
            a.V = posOnly(angrySad.val * -1f);
            HSVColor b;
            b.H = posOnly(arouseDisgust.val);
            b.S = surpriseFactor.val + 1f;
            b.V = posOnly(arouseDisgust.val * -1f);
            attributesA.val = a;
            attributesB.val = b;
        }

        //returns a positive float only
        public float posOnly(float input) {
            if (input > 0) {
                return input;
            } else {
                return 0;
            }
        }
        #endregion
    }
}