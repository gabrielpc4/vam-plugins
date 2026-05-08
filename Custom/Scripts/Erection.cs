using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using System.Threading;
using System.Text.RegularExpressions;

namespace extraltodeusErectionPlugin
{
    public class B : MVRScript
    {

        protected JSONStorableFloat initialLengthSlider;
        protected JSONStorableFloat intensitySlider;
        protected JSONStorableFloat flacidSlider;
        protected JSONStorableFloat angleSlider;
        protected JSONStorableFloat smallSlider;
        protected JSONStorableFloat bigPSlider;
        protected JSONStorableFloat glossMinSlider;
        protected JSONStorableFloat glossMaxSlider;
        protected JSONStorableFloat gloss;

        private FreeControllerV3 penB;
        private FreeControllerV3 penM;
        private FreeControllerV3 penT;
        private JSONStorable skin;


        protected float previousState;
        protected float initialLength;
        protected float initialBigP;
        protected float newState;
        protected float intensityPower;

        // Tweakable variables, default values at start :
        private float intensityDefaultValue = 1.0f;
        private float softDefaultValue = 1.33f;
        private float angleDefaultValue = 15.0f;
        private bool glossEnabled = true;  // Write "false" instead of "true" here if you don't want the gloss change effect !!!

        private bool ready = false;

        protected void Update()
        {

            if (!ready)
            {
                JSONStorable geometry = containingAtom.GetStorableByID("geometry");
                skin = containingAtom.GetStorableByID("skin");

                if (geometry != null && skin != null)
                {
                    ready = true;

                    DAZCharacterSelector character = geometry as DAZCharacterSelector;
                    GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

                    DAZMorph PLength = morphControl.GetMorphByDisplayName("Penis Length");
                    initialLength = PLength.morphValue;

                    DAZMorph bigP = morphControl.GetMorphByDisplayName("bigpenis");
                    initialBigP = bigP.morphValue;

                    gloss = skin.GetFloatJSONParam("Gloss");



                    #region Sliders

                    intensitySlider = new JSONStorableFloat("Master Intensity", intensityDefaultValue, 0.01f, 1.0f, true);
                    intensitySlider.storeType = JSONStorableParam.StoreType.Full;
                    RegisterFloat(intensitySlider);
                    CreateSlider(intensitySlider, false);

                    initialLengthSlider = new JSONStorableFloat("Erect penis length", initialLength, 0.01f, 6f, true);
                    initialLengthSlider.storeType = JSONStorableParam.StoreType.Full;
                    RegisterFloat(initialLengthSlider);
                    CreateSlider(initialLengthSlider, true);

                    bigPSlider = new JSONStorableFloat("Erect girth", initialBigP, 0f, 2f, true);
                    bigPSlider.storeType = JSONStorableParam.StoreType.Full;
                    RegisterFloat(bigPSlider);
                    CreateSlider(bigPSlider, true);

                    smallSlider = new JSONStorableFloat("Flacid penis lentgh", initialLength / 2.5f, 0.01f, 5f, true);
                    smallSlider.storeType = JSONStorableParam.StoreType.Full;
                    RegisterFloat(smallSlider);
                    CreateSlider(smallSlider, true);

                    flacidSlider = new JSONStorableFloat("General flacidity", softDefaultValue, 0.01f, 20.0f, true);
                    flacidSlider.storeType = JSONStorableParam.StoreType.Full;
                    RegisterFloat(flacidSlider);
                    CreateSlider(flacidSlider, true);

                    angleSlider = new JSONStorableFloat("Erection angle (reversed at 50%)", angleDefaultValue, -60f, 60f, true);
                    angleSlider.storeType = JSONStorableParam.StoreType.Full;
                    RegisterFloat(angleSlider);
                    CreateSlider(angleSlider, true);

                    glossMinSlider = new JSONStorableFloat("Skin gloss minimum value", gloss.val - 1, 2f, 8f, true);
                    glossMinSlider.storeType = JSONStorableParam.StoreType.Full;

                    glossMaxSlider = new JSONStorableFloat("Skin gloss maximum value", gloss.val, 2f, 8f, true);
                    glossMaxSlider.storeType = JSONStorableParam.StoreType.Full;

                    if (glossEnabled == true)
                    {
                        RegisterFloat(glossMinSlider);
                        CreateSlider(glossMinSlider, true);
                        RegisterFloat(glossMaxSlider);
                        CreateSlider(glossMaxSlider, true);
                    }

                    #endregion
                }
            }

            if (!ready) return;



            newState = intensitySlider.val * flacidSlider.val * initialLengthSlider.val * angleSlider.val * smallSlider.val * (bigPSlider.val + 0.1f) * glossMinSlider.val * glossMaxSlider.val;

            if (newState != previousState)
            {

                previousState = newState;
                JSONStorable geometry = containingAtom.GetStorableByID("geometry");
                DAZCharacterSelector character = geometry as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

                DAZMorph PLength = morphControl.GetMorphByDisplayName("Penis Length");
                DAZMorph bigP = morphControl.GetMorphByDisplayName("bigpenis");

                PLength.morphValue = initialLengthSlider.val - (initialLengthSlider.val - smallSlider.val) * (1 - intensitySlider.val);
                bigP.morphValue = bigPSlider.val * intensitySlider.val;

                penB = containingAtom.GetStorableByID("penisBaseControl") as FreeControllerV3;
                penM = containingAtom.GetStorableByID("penisMidControl") as FreeControllerV3;
                penT = containingAtom.GetStorableByID("penisTipControl") as FreeControllerV3;


                if (glossEnabled == true)
                {
                    gloss.val = glossMinSlider.val + (glossMaxSlider.val - glossMinSlider.val) * intensitySlider.val;
                }

                intensityPower = intensitySlider.val * intensitySlider.val * intensitySlider.val * intensitySlider.val;

                penB.jointRotationDriveSpring = intensityPower * 24f / flacidSlider.val + 1f;
                penM.jointRotationDriveSpring = intensityPower * 23f / flacidSlider.val + 1f;
                penT.jointRotationDriveSpring = intensityPower * 11f / flacidSlider.val + 1f;

                penB.jointRotationDriveXTarget = -angleSlider.val + intensitySlider.val * angleSlider.val * 2;
                penM.jointRotationDriveXTarget = -angleSlider.val + intensitySlider.val * angleSlider.val * 2;
                penT.jointRotationDriveXTarget = -angleSlider.val + intensitySlider.val * angleSlider.val * 2;

                penB.jointRotationDriveDamper = intensitySlider.val * 0.04f + 0.04f;
                penM.jointRotationDriveDamper = intensitySlider.val * 0.04f + 0.04f;
                penT.jointRotationDriveDamper = intensitySlider.val * 0.04f + 0.04f;


            }
        }

        public override void Init()
        {
            try
            {
            }

            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }

        }


        void Start()
        {
            previousState = intensitySlider.val * flacidSlider.val * initialLengthSlider.val * (angleSlider.val + 0.01f) * smallSlider.val * (bigPSlider.val + 0.01f) * glossMinSlider.val * glossMaxSlider.val;
            newState = intensitySlider.val * flacidSlider.val * initialLengthSlider.val * (angleSlider.val + 0.01f) * smallSlider.val * (bigPSlider.val + 0.01f) * glossMinSlider.val * glossMaxSlider.val;
        }
    }
}
