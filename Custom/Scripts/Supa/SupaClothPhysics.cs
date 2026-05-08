using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace SupaPlugin
{
    public class SupaAdditionalClothPhysics : MVRScript
    {

        // IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
        // some reason

        // IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
        // is called right after creation

        protected UIDynamicTextField dTitle;
        protected UIDynamicTextField dFoot;
        protected UIDynamicTextField dDescription;
        protected JSONStorableString jTitle;
        protected JSONStorableString jFoot;
        protected JSONStorableString jDescription;
        protected JSONStorableFloat jBreastsAdjust;
        protected JSONStorableFloat jGlutesAdjust;
        protected JSONStorableFloat jFloatBreastsRotationMultiX;
        protected JSONStorableFloat jFloatBreastsRotationMultiY;
        protected JSONStorableFloat jFloatBreastsRotationMultiZ;
        protected JSONStorableFloat jFloatGlutesRotationMultiX;
        protected JSONStorableFloat jFloatGlutesRotationMultiY;
        protected JSONStorableFloat jFloatGlutesRotationMultiZ;
        protected UIDynamicSlider dBreastsAdjust;
        protected UIDynamicSlider dGlutesAdjust;
        protected UIDynamicSlider dFloatBreastsRotationMultiX;
        protected UIDynamicSlider dFloatBreastsRotationMultiY;
        protected UIDynamicSlider dFloatBreastsRotationMultiZ;
        protected UIDynamicSlider dFloatGlutesRotationMultiX;
        protected UIDynamicSlider dFloatGlutesRotationMultiY;
        protected UIDynamicSlider dFloatGlutesRotationMultiZ;
        protected UIDynamicSlider[] BreastsSliderArray = new UIDynamicSlider[4];
        protected UIDynamicSlider[] GlutesSliderArray = new UIDynamicSlider[4];
        protected UIDynamicButton dBreastsTargetXButton;
        protected UIDynamicButton dBreastsTargetYButton;
        protected UIDynamicButton dBreastsTargetZButton;
        protected UIDynamicButton dGlutesTargetXButton;
        protected UIDynamicButton dGlutesTargetYButton;
        protected UIDynamicButton dGlutesTargetZButton;
        protected UIDynamicButton dBreastsAdjustButton;
        protected UIDynamicButton dGlutesAdjustButton;
        protected UIDynamicButton dBreastsPhysics;
        protected UIDynamicButton dGlutesPhysics;
        protected UIDynamicButton dRestoreAll;
        protected UIDynamicButton dResetAll;
        protected UIDynamicButton[] BreastsButtonArray = new UIDynamicButton[4];
        protected UIDynamicButton[] GlutesButtonArray = new UIDynamicButton[4];
        protected DAZClothingItemControl oTopBreastsDcc;
        protected DAZClothingItemControl oTopBreastsDccPrev;
        protected DAZClothingItemControl oTopGlutesDcc;
        protected DAZClothingItemControl oTopGlutesDccPrev;
        protected bool bBreastGlute = true;
        protected float[] fRestoreTargets = new float[8];
        protected string sBreastsDescription;
        protected string sGlutesDescription;
        protected float jBreastsAdjustPrev;
        protected float jGlutesAdjustPrev;
        public override void Init()
        {
            try
            {
                if (containingAtom.type != "Person")
                {
                    SuperController.LogError("SupaAdditionalClothPhysics Plugin must added to a valid person atom");
                    return;
                }
                SuperController.LogMessage("Supa Additional Cloth Physics Plugin Loaded");

                // Reset breasts target for Y and Z
                DAZCharacterSelector dcs = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
                dcs.femaleBreastAdjustJoints.invertJoint2RotationZ = true;


                // Reset glutes target for xyz
                dcs.femaleGluteAdjustJoints.invertJoint2RotationZ = true;


                // Title
                jTitle = new JSONStorableString("SupaClothPhysics", "Supa Additional Cloth Physics");
                dTitle = CreateTextField(jTitle);
                dTitle.height = 128;
                dTitle.UItext.fontSize = 56;
                dTitle.UItext.alignment = TextAnchor.LowerCenter;
                dTitle.UItext.fontStyle = FontStyle.Bold;
                dTitle.textColor = Color.Lerp(Color.red, Color.black, 0.5f);
                dTitle.UItext.fontStyle = FontStyle.BoldAndItalic;
                dTitle.backgroundColor = Color.Lerp(Color.grey, Color.black, 0.69f);

                // Foot string message
                jFoot = new JSONStorableString("FootString", "");
                dFoot = CreateTextField(jFoot, true);
                dFoot.UItext.fontSize = 28;
                dFoot.UItext.lineSpacing = 1.5f;
                dFoot.UItext.alignment = TextAnchor.MiddleCenter;
                dFoot.backgroundColor = Color.Lerp(Color.grey, Color.black, 0.69f);
                dFoot.textColor = Color.Lerp(Color.black, Color.white, 0.69f);
                dFoot.height = 128;

                // Breasts Physics Tab
                dBreastsPhysics = CreateButton("");
                dBreastsPhysics.buttonText.text = "Show Breasts Physics";
                dBreastsPhysics.buttonColor = Color.Lerp(Color.magenta, Color.white, 0.69f);
                dBreastsPhysics.button.onClick.AddListener(BreastsPhysicsTabCallback);
                // Glutes Physics Tab
                dGlutesPhysics = CreateButton("");
                dGlutesPhysics.buttonText.text = "Show Glutes Physics";
                dGlutesPhysics.buttonColor = Color.Lerp(Color.blue, Color.white, 0.69f);
                dGlutesPhysics.button.onClick.AddListener(GlutesPhysicsTabCallback);

                // Set all param to Previous value
                dRestoreAll = CreateButton("");
                dRestoreAll.buttonText.text = "Restore Previous Breasts Settings";
                dRestoreAll.buttonColor = Color.Lerp(Color.yellow, Color.white, 0.69f);
                dRestoreAll.button.onClick.AddListener(RestoreAllButtonCallback);
                // Set all param to reset value
                dResetAll = CreateButton("");
                dResetAll.buttonText.text = "Reset Breasts Rotation Targets";
                dResetAll.buttonColor = Color.Lerp(Color.red, Color.white, 0.69f);
                dResetAll.button.onClick.AddListener(ResetAllButtonCallback);

                // Breasts Physics Buttons/Sliders
                jBreastsAdjust = new JSONStorableFloat("Max Adjust Breasts Multi", 3f, RotationMultiFloatCallback, 1f, 10f, false);
                jFloatBreastsRotationMultiX = new JSONStorableFloat("Breasts Rotation Multi X", 1f, RotationMultiFloatCallback, 0f, 5f, false);
                jFloatBreastsRotationMultiY = new JSONStorableFloat("Breasts Rotation Multi Y", 3f, RotationMultiFloatCallback, 0f, 5f, false);
                jFloatBreastsRotationMultiZ = new JSONStorableFloat("Breasts Rotation Multi Z", 5f, RotationMultiFloatCallback, 0f, 5f, false);
                RegisterFloat(jBreastsAdjust);
                RegisterFloat(jFloatBreastsRotationMultiX);
                RegisterFloat(jFloatBreastsRotationMultiY);
                RegisterFloat(jFloatBreastsRotationMultiZ);
                fRestoreTargets[0] = jBreastsAdjust.defaultVal;
                fRestoreTargets[1] = jFloatBreastsRotationMultiX.defaultVal;
                fRestoreTargets[2] = jFloatBreastsRotationMultiY.defaultVal;
                fRestoreTargets[3] = jFloatBreastsRotationMultiZ.defaultVal;
                BreastsPhysicsButtonSlider();

                jGlutesAdjust = new JSONStorableFloat("Max Adjust Glutes Multi", 3f, RotationMultiFloatCallback, 1f, 10f, false);
                jFloatGlutesRotationMultiX = new JSONStorableFloat("Glutes Rotation Multi X", 1f, RotationMultiFloatCallback, 0f, 5f, false);
                jFloatGlutesRotationMultiY = new JSONStorableFloat("Glutes Rotation Multi Y", 3f, RotationMultiFloatCallback, 0f, 5f, false);
                jFloatGlutesRotationMultiZ = new JSONStorableFloat("Glutes Rotation Multi Z", 5f, RotationMultiFloatCallback, 0f, 5f, false);
                RegisterFloat(jGlutesAdjust);
                RegisterFloat(jFloatGlutesRotationMultiX);
                RegisterFloat(jFloatGlutesRotationMultiY);
                RegisterFloat(jFloatGlutesRotationMultiZ);
                fRestoreTargets[4] = jGlutesAdjust.defaultVal;
                fRestoreTargets[5] = jFloatGlutesRotationMultiX.defaultVal;
                fRestoreTargets[6] = jFloatGlutesRotationMultiY.defaultVal;
                fRestoreTargets[7] = jFloatGlutesRotationMultiZ.defaultVal;
                // GlutesPhysicsButtonSlider();

                // register tells engine you want value saved in json file during save and also make it available to
                // animation/trigger system
                // RegisterString(jFoot);
                jDescription = new JSONStorableString("Description String", "");
                // jDescription.val = "<i>Additional cloth physis sliders to achieve better cloth physics.</i>";
                sBreastsDescription = "\n<b>Click Buttons to switch tab</b>\n\n\n\n<b><color=red>0</color></b> out multipliers to remove effects" +
                "\n\n\nMax Adjust Breasts Multi: \nIndicate <b>LARGEST</b> Adjust Breasts Spring Multiplier among all clothes" +
                "\n\n\n\n<i>(MaxAdjustBreasts - 1) *\n MultiplierX = RotationTargetX</i>" +
                "\n\nTargetX <b>pushes breasts higher</b>" +
                "\n\n\n<i>(MaxAdjustBreasts - 1) *\n MultiplierY = RotationTargetY</i>" +
                "\n\nTargetY <b>moves breasts closer</b>" +
                "\n\n\n<i>(MaxAdjustBreasts - 1) *\n MultiplierZ = RotationTargetZ</i>" +
                "\n\nTargetZ <b>turns breasts upward</b>";
                jDescription.val = sBreastsDescription;
                sGlutesDescription = "\n<b>Click Buttons to switch tab</b>\n\n\n\n<b><color=red>0</color></b> out multipliers to remove effects" +
                "\n\n\nMax Adjust Glutes Multi: \nIndicate <b>LARGEST</b> Adjust Glutes Spring Multiplier among all clothes" +
                "\n\n\n\n<i>-(MaxAdjustGlutes - 1) *\n MultiplierX = RotationTargetX</i>" +
                "\n\nTargetX <b>pushes glutes lower</b>" +
                "\n\n\n<i>(MaxAdjustGlutes - 1) *\n MultiplierY = RotationTargetY</i>" +
                "\n\nTargetY <b>moves glutes closer</b>" +
                "\n\n\n<i>-(MaxAdjustGlutes - 1) *\n MultiplierZ = RotationTargetZ</i>" +
                "\n\nTargetZ <b>turns glutes downward</b>";
                dDescription = CreateTextField(jDescription, true);
                dDescription.height = 1042;
                dDescription.UItext.fontSize = 30;
                dDescription.UItext.supportRichText = true;

                // create custom JSON storable params here if you want them to be stored with scene JSON
                // types are JSONStorableFloat, JSONStorableBool, JSONStorableString, JSONStorableStringChooser
                // JSONStorableColor

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught Init(): " + e);
            }
        }
        // Start is called once before Update or FixedUpdate is called and after Init()
        void Start()
        {
            try
            {
                // put code in here
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught Start(): " + e);
            }
        }

        // Update is called with each rendered frame by Unity
        void Update()
        {
            try
            {
                // put code in here
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught Update(): " + e);
            }
        }

        // FixedUpdate is called with each physics simulation frame by Unity
        void FixedUpdate()
        {
            try
            {
                if (containingAtom.type != "Person")
                {
                    return;
                }
                DAZCharacterSelector dcs = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
                BreastsFixedUpdate(dcs);
                GlutesFixedUpdate(dcs);
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught FixedUpdate(): " + e);
            }
        }

        // OnDestroy is where you should put any cleanup
        // if you registered objects to supercontroller or atom, you should unregister them here
        void OnDestroy()
        {
        }

        protected void BreastsFixedUpdate(DAZCharacterSelector dcs, float fTopClothAdjustBreasts = 0.999f, bool hasAdjust = false)
        {
            // cycle all clothes for the Max Adjust Breasts Multi
            foreach (DAZClothingItemControl dcc in containingAtom.GetComponentsInChildren<DAZClothingItemControl>())
            {
                if (dcc.GetBoolParamValue("enableBreastJointAdjust"))
                {
                    hasAdjust = true;
                    if (fTopClothAdjustBreasts < dcc.GetFloatParamValue("breastJointSpringAndDamperMultiplier"))
                    {
                        fTopClothAdjustBreasts = dcc.GetFloatParamValue("breastJointSpringAndDamperMultiplier");
                        oTopBreastsDcc = dcc;
                    }
                }
            }
            // first update to reflect top adjust breasts to slider
            if (!oTopBreastsDccPrev)
            {
                jBreastsAdjust.val = fTopClothAdjustBreasts;
                fRestoreTargets[0] = jBreastsAdjust.val;
                oTopBreastsDccPrev = oTopBreastsDcc;
            }
            else
            {
                if (jBreastsAdjust.val != jBreastsAdjustPrev && hasAdjust)
                {
                    // set adjust value to slider value
                    jBreastsAdjustPrev = oTopBreastsDcc.GetFloatParamValue("breastJointSpringAndDamperMultiplier");
                    oTopBreastsDcc.SetFloatParamValue("breastJointSpringAndDamperMultiplier", jBreastsAdjust.val);
                    fTopClothAdjustBreasts = jBreastsAdjust.val;
                }
            }
            // find the correspondent simControl for breastJointAdjust toggle 
            // for undress to disable adjust glutes
            foreach (ClothSimControl csc in containingAtom.GetComponentsInChildren<ClothSimControl>())
            {
                if (oTopBreastsDcc && csc.name == oTopBreastsDcc.name)
                {
                    oTopBreastsDcc.SetBoolParamValue("enableBreastJointAdjust", (csc.GetBoolParamValue("allowDetach") ? false : true));
                    break;
                }
            }
            // update targets
            dcs.femaleBreastAdjustJoints.targetRotationX = (fTopClothAdjustBreasts - 1) * jFloatBreastsRotationMultiX.val;
            dcs.femaleBreastAdjustJoints.targetRotationY = (fTopClothAdjustBreasts - 1) * jFloatBreastsRotationMultiY.val;
            dcs.femaleBreastAdjustJoints.targetRotationZ = (fTopClothAdjustBreasts - 1) * jFloatBreastsRotationMultiZ.val;
            if (bBreastGlute) // update text on indicators 
            {
                dBreastsTargetXButton.buttonText.text = "Breasts Rotation Target X: " + dcs.femaleBreastAdjustJoints.targetRotationX.ToString("0.00");
                dBreastsTargetYButton.buttonText.text = "Breasts Rotation Target Y: " + dcs.femaleBreastAdjustJoints.targetRotationY.ToString("0.00");
                dBreastsTargetZButton.buttonText.text = "Breasts Rotation Target Z: " + dcs.femaleBreastAdjustJoints.targetRotationZ.ToString("0.00");
                dBreastsAdjustButton.buttonText.text = hasAdjust ? (oTopBreastsDcc.name + ": " + fTopClothAdjustBreasts.ToString("0.00")) : "No Cloth Adjust Breasts Enabled";
                jBreastsAdjust.val = hasAdjust ? fTopClothAdjustBreasts : jBreastsAdjust.val;
            }
        }

        protected void GlutesFixedUpdate(DAZCharacterSelector dcs, float fTopClothAdjustGlutes = 0.999f, bool hasAdjust = false)
        {
            // cycle all clothes for the Max Adjust Glutes Multi param
            foreach (DAZClothingItemControl dcc in containingAtom.GetComponentsInChildren<DAZClothingItemControl>())
            {
                if (dcc.GetBoolParamValue("enableGluteJointAdjust"))
                {
                    hasAdjust = true;
                    if (fTopClothAdjustGlutes < dcc.GetFloatParamValue("gluteJointSpringAndDamperMultiplier"))
                    {
                        fTopClothAdjustGlutes = dcc.GetFloatParamValue("gluteJointSpringAndDamperMultiplier");
                        oTopGlutesDcc = dcc;
                    }
                }
            }
            // first update to reflect top adjust glutes to slider
            if (!oTopGlutesDccPrev)
            {
                jGlutesAdjust.val = fTopClothAdjustGlutes;
                fRestoreTargets[4] = jGlutesAdjust.val;
                oTopGlutesDccPrev = oTopGlutesDcc;
            }
            else
            {
                if (jGlutesAdjust.val != jGlutesAdjustPrev && hasAdjust)
                {
                    // set adjust value to slider value
                    jGlutesAdjustPrev = oTopGlutesDcc.GetFloatParamValue("gluteJointSpringAndDamperMultiplier");
                    oTopGlutesDcc.SetFloatParamValue("gluteJointSpringAndDamperMultiplier", jGlutesAdjust.val);
                    fTopClothAdjustGlutes = jGlutesAdjust.val;
                }
            }
            // find the correspondent simControl for gluteJointAdjust toggle 
            // for undress to disable adjust glutes
            foreach (ClothSimControl csc in containingAtom.GetComponentsInChildren<ClothSimControl>())
            {
                if (oTopGlutesDcc && csc.name == oTopGlutesDcc.name)
                {
                    oTopGlutesDcc.SetBoolParamValue("enableGluteJointAdjust", (csc.GetBoolParamValue("allowDetach") ? false : true));
                    break;
                }
            }
            // update targets
            dcs.femaleGluteAdjustJoints.targetRotationX = -(fTopClothAdjustGlutes - 1) * jFloatGlutesRotationMultiX.val;
            dcs.femaleGluteAdjustJoints.targetRotationY = (fTopClothAdjustGlutes - 1) * jFloatGlutesRotationMultiY.val;
            dcs.femaleGluteAdjustJoints.targetRotationZ = -(fTopClothAdjustGlutes - 1) * jFloatGlutesRotationMultiZ.val;
            if (!bBreastGlute) // text on indicators 
            {
                dGlutesTargetXButton.buttonText.text = "Glutes Rotation Target X: " + dcs.femaleGluteAdjustJoints.targetRotationX.ToString("0.00");
                dGlutesTargetYButton.buttonText.text = "Glutes Rotation Target Y: " + dcs.femaleGluteAdjustJoints.targetRotationY.ToString("0.00");
                dGlutesTargetZButton.buttonText.text = "Glutes Rotation Target Z: " + dcs.femaleGluteAdjustJoints.targetRotationZ.ToString("0.00");
                dGlutesAdjustButton.buttonText.text = hasAdjust ? (oTopGlutesDcc.name + ": " + fTopClothAdjustGlutes.ToString("0.000")) : "No Cloth Adjust Glutes Enabled";
                jGlutesAdjust.val = hasAdjust ? fTopClothAdjustGlutes : jGlutesAdjust.val;
            }
        }

        protected void RotationMultiFloatCallback(JSONStorableFloat jf)
        {
            if (jFoot != null)
            {
                jFoot.val = "\n" + jf.name + " set to " + jf.val.ToString("0.00");
            }
        }

        protected void RestoreAllButtonCallback()
        {
            if (bBreastGlute)
            {
                jBreastsAdjust.SetVal(fRestoreTargets[0]);
                jFloatBreastsRotationMultiX.SetVal(fRestoreTargets[1]);
                jFloatBreastsRotationMultiY.SetVal(fRestoreTargets[2]);
                jFloatBreastsRotationMultiZ.SetVal(fRestoreTargets[3]);
            }
            else
            {
                jGlutesAdjust.SetVal(fRestoreTargets[4]);
                jFloatGlutesRotationMultiX.SetVal(fRestoreTargets[5]);
                jFloatGlutesRotationMultiY.SetVal(fRestoreTargets[6]);
                jFloatGlutesRotationMultiZ.SetVal(fRestoreTargets[7]);
            }
            jFoot.val = "\nRestore Previous " + (bBreastGlute ? "Breasts Settings" : "Glutes Settings");
        }

        protected void ResetAllButtonCallback()
        {
            DAZCharacterSelector dcs = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
            if (bBreastGlute)
            {
                fRestoreTargets[0] = jBreastsAdjust.val;
                fRestoreTargets[1] = jFloatBreastsRotationMultiX.val;
                fRestoreTargets[2] = jFloatBreastsRotationMultiY.val;
                fRestoreTargets[3] = jFloatBreastsRotationMultiZ.val;
                jBreastsAdjust.SetValToDefault();
                jFloatBreastsRotationMultiX.SetVal(5 / (jBreastsAdjust.val - 1));
                jFloatBreastsRotationMultiY.SetVal(0);
                jFloatBreastsRotationMultiZ.SetVal(0);
            }
            else
            {
                fRestoreTargets[4] = jGlutesAdjust.val;
                fRestoreTargets[5] = jFloatGlutesRotationMultiX.val;
                fRestoreTargets[6] = jFloatGlutesRotationMultiY.val;
                fRestoreTargets[7] = jFloatGlutesRotationMultiZ.val;
                jGlutesAdjust.SetValToDefault();
                jFloatGlutesRotationMultiX.SetVal(10 / (jGlutesAdjust.val - 1));
                jFloatGlutesRotationMultiY.SetVal(0);
                jFloatGlutesRotationMultiZ.SetVal(0);
            }
            jFoot.val = "\nReset " + (bBreastGlute ? "Breasts Rotation" : "Glutes Rotation") + " Targets";
        }

        protected void BreastsPhysicsButtonSlider()
        {
            // Adjust Breasts Slider 
            dBreastsAdjust = CreateSlider(jBreastsAdjust);
            dBreastsAdjust.rangeAdjustEnabled = false;
            BreastsSliderArray[0] = dBreastsAdjust;
            // Adjust Breasts Status
            dBreastsAdjustButton = CreateButton("");
            dBreastsAdjustButton.buttonColor = Color.Lerp(Color.magenta, Color.white, 0.69f);
            dBreastsAdjustButton.button.enabled = false;
            BreastsButtonArray[0] = dBreastsAdjustButton;
            // BreastsRotationMultiX
            dFloatBreastsRotationMultiX = CreateSlider(jFloatBreastsRotationMultiX);
            BreastsSliderArray[1] = dFloatBreastsRotationMultiX;
            // BreastsTargetXButton
            dBreastsTargetXButton = CreateButton("");
            dBreastsTargetXButton.buttonColor = Color.Lerp(Color.magenta, Color.white, 0.69f);
            dBreastsTargetXButton.button.enabled = false;
            BreastsButtonArray[1] = dBreastsTargetXButton;
            // BreastsRotationMultiY
            dFloatBreastsRotationMultiY = CreateSlider(jFloatBreastsRotationMultiY);
            BreastsSliderArray[2] = dFloatBreastsRotationMultiY;
            // BreastsTargetYButton
            dBreastsTargetYButton = CreateButton("");
            dBreastsTargetYButton.buttonColor = Color.Lerp(Color.magenta, Color.white, 0.69f);
            dBreastsTargetYButton.button.enabled = false;
            BreastsButtonArray[2] = dBreastsTargetYButton;
            // BreastsRotationMultiZ
            dFloatBreastsRotationMultiZ = CreateSlider(jFloatBreastsRotationMultiZ);
            BreastsSliderArray[3] = dFloatBreastsRotationMultiZ;
            // BreastsTargetZButton
            dBreastsTargetZButton = CreateButton("");
            dBreastsTargetZButton.buttonColor = Color.Lerp(Color.magenta, Color.white, 0.69f);
            dBreastsTargetZButton.button.enabled = false;
            BreastsButtonArray[3] = dBreastsTargetZButton;
        }

        protected void GlutesPhysicsButtonSlider()
        {
            // Adjust Glutes Slider 
            dGlutesAdjust = CreateSlider(jGlutesAdjust);
            dGlutesAdjust.rangeAdjustEnabled = false;
            GlutesSliderArray[0] = dGlutesAdjust;
            // Adjust Glutes Status 
            dGlutesAdjustButton = CreateButton("");
            dGlutesAdjustButton.buttonColor = Color.Lerp(Color.blue, Color.white, 0.69f);
            dGlutesAdjustButton.button.enabled = false;
            GlutesButtonArray[0] = dGlutesAdjustButton;
            // GlutesRotationMultiX
            dFloatGlutesRotationMultiX = CreateSlider(jFloatGlutesRotationMultiX);
            GlutesSliderArray[1] = dFloatGlutesRotationMultiX;
            // GlutesTargetXButton
            dGlutesTargetXButton = CreateButton("");
            dGlutesTargetXButton.buttonColor = Color.Lerp(Color.blue, Color.white, 0.69f);
            dGlutesTargetXButton.button.enabled = false;
            GlutesButtonArray[1] = dGlutesTargetXButton;
            // GlutesRotationMultiY
            dFloatGlutesRotationMultiY = CreateSlider(jFloatGlutesRotationMultiY);
            GlutesSliderArray[2] = dFloatGlutesRotationMultiY;
            // GlutesTargetYButton
            dGlutesTargetYButton = CreateButton("");
            dGlutesTargetYButton.buttonColor = Color.Lerp(Color.blue, Color.white, 0.69f);
            dGlutesTargetYButton.button.enabled = false;
            GlutesButtonArray[2] = dGlutesTargetYButton;
            // GlutesRotationMultiZ
            dFloatGlutesRotationMultiZ = CreateSlider(jFloatGlutesRotationMultiZ);
            GlutesSliderArray[3] = dFloatGlutesRotationMultiZ;
            // GlutesTargetZButton
            dGlutesTargetZButton = CreateButton("");
            dGlutesTargetZButton.buttonColor = Color.Lerp(Color.blue, Color.white, 0.69f);
            dGlutesTargetZButton.button.enabled = false;
            GlutesButtonArray[3] = dGlutesTargetZButton;
        }
        protected void RemoveBreastsButtonSlider()
        {
            foreach (UIDynamicButton button in BreastsButtonArray)
            {
                RemoveButton(button);
            }
            foreach (UIDynamicSlider slider in BreastsSliderArray)
            {
                RemoveSlider(slider);
            }
        }

        protected void RemoveGlutesButtonSlider()
        {
            foreach (UIDynamicButton button in GlutesButtonArray)
            {
                RemoveButton(button);
            }
            foreach (UIDynamicSlider slider in GlutesSliderArray)
            {
                RemoveSlider(slider);
            }
        }

        protected void BreastsPhysicsTabCallback()
        {
            if (!bBreastGlute)
            {
                RemoveGlutesButtonSlider();
                BreastsPhysicsButtonSlider();
                bBreastGlute = true;
                jFoot.val = "\nShowing Breasts Physics Tab";
                dResetAll.buttonText.text = "Reset Breasts Rotation Targets";
                dRestoreAll.buttonText.text = "Restore Previous Breasts Settings";
                jDescription.val = sBreastsDescription;
            }
            else
            {
                jFoot.val = "\nBreasts Physics Tab is already showing";
            }
        }

        protected void GlutesPhysicsTabCallback()
        {
            if (bBreastGlute)
            {
                RemoveBreastsButtonSlider();
                GlutesPhysicsButtonSlider();
                bBreastGlute = false;
                jFoot.val = "\nShowing Glutes Physics Tab";
                dResetAll.buttonText.text = "Reset Glutes Rotation Targets";
                dRestoreAll.buttonText.text = "Restore Previous Glutes Settings";
                jDescription.val = sGlutesDescription;
            }
            else
            {
                jFoot.val = "\nGlutes Physics Tab is already showing";
            }
        }
    }
}