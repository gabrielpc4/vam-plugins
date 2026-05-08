using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using System.Threading;
using System.Text.RegularExpressions;

namespace extraltodeusBreathingPlugin {
	public class B : MVRScript {

        Dictionary<string, UIDynamicToggle> toggles     = new Dictionary<string, UIDynamicToggle>();

        protected JSONStorableBool  playingBool;
        protected JSONStorableFloat breathSlider;
        protected JSONStorableFloat chestSlider;
        protected JSONStorableFloat sternumSlider;
        protected JSONStorableFloat ribcageSlider;
        protected JSONStorableFloat mouthLinkSlider;
        protected JSONStorableFloat intensitySlider;
        protected JSONStorableFloat animLengthSlider;

        private Atom person;
        private FreeControllerV3 personChestControl;
        private Transform chest;

        float inOut = 1f;
        float f     = 0f;
        float varia = 0f;

        protected void Update()
        {
            person = containingAtom;
            personChestControl = (FreeControllerV3)person.GetStorableByID("chestControl");
            chest = personChestControl.transform;
            Vector3 eulerAngles = chest.transform.localEulerAngles;

            JSONStorable geometry = containingAtom.GetStorableByID("geometry");
            DAZCharacterSelector character = geometry as DAZCharacterSelector;
            GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

            DAZMorph morphBreath  = morphControl.GetMorphByDisplayName("Breath1");
            DAZMorph morphMouth   = morphControl.GetMorphByDisplayName("Mouth Open");
            DAZMorph morphRibcage = morphControl.GetMorphByDisplayName("Ribcage Size");
            DAZMorph morphSternum = morphControl.GetMorphByDisplayName("Sternum Depth");

            if (toggles["Play"].toggle.isOn)
            {
                f += animLengthSlider.val;
                varia = (float)Math.Sin(f / 100) * intensitySlider.val * (Time.deltaTime*100);

                if (breathSlider.val != 0)
                    morphBreath.morphValue += (varia * breathSlider.val) /  50*-1;

                if (ribcageSlider.val != 0)
                    morphRibcage.morphValue += (varia * ribcageSlider.val) / 500;

                if (sternumSlider.val != 0)
                    morphSternum.morphValue += (varia * sternumSlider.val) / 750;

                if (chestSlider.val > 0)
                {
                    eulerAngles = chest.transform.localEulerAngles;
                    eulerAngles.x -= varia / (float)(Math.PI / 0.2) * (animLengthSlider.val - 0.3f) * 1.5f * chestSlider.val;
                    chest.transform.localEulerAngles = eulerAngles;
                }

                if (mouthLinkSlider.val > 0)
                {
                    morphMouth.morphValue = (varia) * animLengthSlider.val * mouthLinkSlider.val/25 + ((mouthLinkSlider.val/3 * (animLengthSlider.val - 2)) + (intensitySlider.val-0.7f)/2f + (animLengthSlider.val-2) / 24)*intensitySlider.val;
                }

                if (f > (Math.PI * 200))
                    f = 0;
            }
        }

        public override void Init() {
            try
            {
                #region Sliders
                JSONStorable geometry = containingAtom.GetStorableByID("geometry");
                DAZCharacterSelector character = geometry as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

                intensitySlider = new JSONStorableFloat("Intensity", intensityDefaultValue, 0.0f, 1.0f, true);
                intensitySlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(intensitySlider);
                CreateSlider(intensitySlider, true);

                animLengthSlider = new JSONStorableFloat("Speed", speedDefaultValue, 0.1f, 10.0f, true);
                animLengthSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(animLengthSlider);
                CreateSlider(animLengthSlider, true);

                chestSlider = new JSONStorableFloat("Chest rotation intensity", chestRotationDefaultValue, 0.0f, 2.0f, true);
                chestSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(chestSlider);
                CreateSlider(chestSlider, true);

                breathSlider = new JSONStorableFloat("Breath link intensity", breathDefaultValue, 0.0f, 2.0f, true);
                breathSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(breathSlider);
                CreateSlider(breathSlider, true);

                mouthLinkSlider = new JSONStorableFloat("Mouth link intensity", mouthLinkDefaultValue, 0.0f, 2.0f, true);
                mouthLinkSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(mouthLinkSlider);
                CreateSlider(mouthLinkSlider, true);

                ribcageSlider = new JSONStorableFloat("Ribcage size link intensity", ribcageSizeLinkDefaultValue, 0.0f, 2.0f, true);
                ribcageSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(ribcageSlider);
                CreateSlider(ribcageSlider, true);

                sternumSlider = new JSONStorableFloat("Sternum link intensity", sternumLinkDefaultValue, 0.0f, 2.0f, true);
                sternumSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(sternumSlider);
                CreateSlider(sternumSlider, true);

                JSONStorableBool playingBool = new JSONStorableBool("Play", true);
                playingBool.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(playingBool);
                toggles["Play"] = CreateToggle((playingBool), true);
                #endregion
            }

            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
		}

        // Tweakable variables, default values at start :
        private float intensityDefaultValue       = 1.0f;
        private float speedDefaultValue           = 3.0f;
        private float chestRotationDefaultValue   = 0.8f;
        private float breathDefaultValue          = 1.0f;
        private float mouthLinkDefaultValue       = 0.0f;
        private float ribcageSizeLinkDefaultValue = 0.0f;
        private float sternumLinkDefaultValue     = 0.0f;
    }
}
