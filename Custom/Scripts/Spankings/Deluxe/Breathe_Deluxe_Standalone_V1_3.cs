using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

// Original "Breathe.cs" Script from Dollmaster by u/VAM Deluxe
// Adaptations for breate standalone use, added idle motions and expressions by u/JustLookingForNothin

namespace DeluxePlugin
{
    public class BreatheStandalone : MVRScript
    {
        public float breath = 0.0f;
        private float breatheCylce = -0.1f;
        private float breathLerp = 0.16f;
        private float MouthOpenVal = 0.0f;
        

        private DAZMorph breatheMorph;
        private DAZMorph moMorph;
        private DAZMorph NoseMorphPinch;
        private DAZMorph NoseMorphTilt;
        private DAZMorph MouthMorphSH;
        private DAZMorph MouthMorphAA;
        private DAZMorph MouthMorphLUD;
        private DAZMorph MouthMorphLBI;
        private DAZMorph MouthMorphMCD;
        private DAZMorph FaceMorphBrowsArch;
        private DAZMorph EyesMorphEyesClosedL;
        private DAZMorph EyesMorphEyesClosedR;
        private DAZMorph EyesMorphEyesRound;
        private DAZMorph FaceMorphBrowsIU;
        private DAZMorph FaceMorphBrowDR;
        private DAZMorph TongueMorphIO;
        private DAZMorph TongueMorphCu;

        private FreeControllerV3 chest;
        private FreeControllerV3 neck;
        private FreeControllerV3 head;
        private FreeControllerV3 lArm;
        private FreeControllerV3 rArm;
        private FreeControllerV3 lHand;
        private FreeControllerV3 rHand;
        private FreeControllerV3 lThigh;
        private FreeControllerV3 rThigh;

        public JSONStorableFloat intensity;
        public JSONStorableFloat chestmove;
        public JSONStorableBool invertChestBool;
        public JSONStorableBool UseHeadControlBool;
        public JSONStorableBool EnableIdleMotionsBool;
        public JSONStorableBool EnableExpressionsBool;

        // Idle motion variables
        protected JSONStorableFloat periodJSON;
        protected JSONStorableFloat quicknessJSON;
        protected JSONStorableFloat motTntensityJSON;
        protected JSONStorableFloat targetValueJSON;
        protected JSONStorableFloat targetValNxJSON;
        protected JSONStorableFloat targetValNyJSON;
        protected JSONStorableFloat targetValNzJSON;
        protected JSONStorableFloat targetValSyJSON;
        protected JSONStorableFloat targetValHxJSON;
        protected JSONStorableFloat targetValHandsJSON;
        protected JSONStorableFloat targetValHandsXJSON;
        protected JSONStorableFloat targetValThighsJSON;
        protected JSONStorableFloat targetValExpJSON;
        protected JSONStorableFloat currentValueJSON;
        protected JSONStorableFloat randomizeTimerJSON;
        protected JSONStorableFloat lowerValueJSON;
        protected JSONStorableFloat upperValueJSON;
        protected JSONStorableString targetNameJSON;
        protected JSONStorableFloat currentValNxJSON;
        protected JSONStorableFloat currentValNyJSON;
        protected JSONStorableFloat currentValNzJSON;
        protected JSONStorableFloat currentValSyJSON;
        protected JSONStorableFloat currentValHxJSON;
        protected JSONStorableFloat currentValHandsJSON;
        protected JSONStorableFloat currentValHandsXJSON;
        protected JSONStorableFloat currentValThighsJSON;
        protected JSONStorableFloat currentValExpJSON;

        //protected string _receiverTargetName;
        private float motionTimer = 0f;
        protected float motionTimerNX = 0f;
        protected float motionTimerNY = 0f;
        protected float motionTimerNZ = 0f;
        protected float motionTimerSY = 0f;
        protected float motionTimerHeX = 0f;
        protected float motionTimerHaS = 0f;
        protected float motionTimerHaX = 0f;
        protected float motionTimerTX = 0f;
        protected float mirrorRNDS = 0f;
        protected float mirrorRNDH = 0f;
        protected float expressionTimer1 = 5f;
        protected float expressionTimer2 = 0f;
        protected float Nod_initialVal = 0f;
        protected int expressionID = 0;
        protected int expressionStart = 0;
        protected int expressionDone = 1;
        protected int RevExpression = 0;
        protected int HeadNod = 0;
        protected int CurrExprID = 0;
        protected JSONStorable autoBl;
        protected JSONStorableBool autoBlEnabled;


        
        //Idle Motion settings
        //Head Drive X {min value, max value, period, smoothness}
        float[] HeadDriveValuesX = { -15.0f, 15.0f, 10.0f, 1.0f };

        //Neck Drive X {min value, max value, period, smoothness}
        float[] NeckDriveValuesX = { -30.0f, 15.0f, 3.4f, 0.8f };

        // Neck Drive Y {min value, max value, period, smoothness}
        float[] NeckDriveValuesY = { -35.0f, 35.0f, 4.3f, 0.6f };

        // Neck Drive Z {min value, max value, period, smoothness}
        float[] NeckDriveValuesZ = { -20.0f, 20.0f, 5.4f, 1.5f };

        // Shoulder Drive Y {min value, max value, period, smoothness}
        float[] ShoulderDriveValuesY = { -20.0f, 10.0f, 7.4f, 0.9f };

        // Thighs Drive X {min value, max value, period, smoothness}
        float[] ThighsDriveValuesX = { -30.0f, 30.0f, 14.0f, 1,1f };

        // Hands Position Spring {min value, max value, period, smoothness}
        float[] HandSpringValues = { 900f, 2500f, 6.4f, 1.5f };

        // Hands Drive X {min value, max value, period, smoothness}
        float[] HandDriveValuesx = { -40.0f, 40.0f, 20.0f, 0.9f };

        Dictionary<string, float> CurrentMorphsValues = new Dictionary<string, float>();



        public override void Init()
        {
            try
            {
                JSONStorable js = containingAtom.GetStorableByID("geometry");
                if (js != null)
                {
                    DAZCharacterSelector dcs = js as DAZCharacterSelector;
                    GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;
                    if (morphUI != null)
                    {
                        breatheMorph = morphUI.GetMorphByDisplayName("Breath1");
                        moMorph = morphUI.GetMorphByDisplayName("Mouth Open");
                        NoseMorphPinch = morphUI.GetMorphByDisplayName("Nose Pinch");
                        NoseMorphTilt = morphUI.GetMorphByDisplayName("Nose Tilt");
                        MouthMorphSH = morphUI.GetMorphByDisplayName("SH");
                        MouthMorphAA = morphUI.GetMorphByDisplayName("AA");
                        MouthMorphLUD = morphUI.GetMorphByDisplayName("Lip Upper Depth");
                        MouthMorphLBI = morphUI.GetMorphByDisplayName("Lip Bottom In");
                        MouthMorphMCD = morphUI.GetMorphByDisplayName("Mouth Corner Depth");
                        FaceMorphBrowsArch = morphUI.GetMorphByDisplayName("Brows Arch");
                        FaceMorphBrowsIU = morphUI.GetMorphByDisplayName("Brow Inner Up");
                        FaceMorphBrowDR = morphUI.GetMorphByDisplayName("Brow Down Right");
                        EyesMorphEyesClosedL = morphUI.GetMorphByDisplayName("Eyes Closed Left");
                        EyesMorphEyesClosedR = morphUI.GetMorphByDisplayName("Eyes Closed Right");
                        EyesMorphEyesRound = morphUI.GetMorphByDisplayName("Eyes Round");
                        TongueMorphIO = morphUI.GetMorphByDisplayName("Tongue In-Out");
                        TongueMorphCu = morphUI.GetMorphByDisplayName("Tongue Curl");

                    }
                }

                // Auto-Blink 
                autoBl = containingAtom.GetStorableByID("EyelidControl");
                if (autoBl != null)
                {
                    autoBlEnabled = autoBl.GetBoolJSONParam("blinkEnabled");
                    if (autoBlEnabled == null)
                    {
                        SuperController.LogError("Could not find 'enabled' param in 'Auto Blink'");
                    }
                }
                else
                {
                    SuperController.LogError("Could not find 'AutoBlink' storable. Load plugin onto Person atom");
                }
                



                chest = containingAtom.GetStorableByID("chestControl") as FreeControllerV3;
                if (chest != null)
                {
                    chest.jointRotationDriveSpring = 60.0f;
                    chest.jointRotationDriveDamper = 1.0f;
                }

                neck = containingAtom.GetStorableByID("neckControl") as FreeControllerV3;
                if (neck != null)
                {
                    //neck.jointRotationDriveSpring = 60.0f;
                    //neck.jointRotationDriveDamper = 1.0f;
                }

                lArm = containingAtom.GetStorableByID("lArmControl") as FreeControllerV3;
                if (lArm != null)
                {
                    //lArm.jointRotationDriveSpring = 60.0f;
                    //lArm.jointRotationDriveDamper = 1.0f;
                }

                rArm = containingAtom.GetStorableByID("rArmControl") as FreeControllerV3;
                if (rArm != null)
                {
                    //rArm.jointRotationDriveSpring = 60.0f;
                    //rArm.jointRotationDriveDamper = 1.0f;
                }

                lHand = containingAtom.GetStorableByID("lHandControl") as FreeControllerV3;
                if (lHand != null)
                {
                    //lHand.jointRotationDriveSpring = 60.0f;
                    //lHand.jointRotationDriveDamper = 1.0f;
                }

                rHand = containingAtom.GetStorableByID("rHandControl") as FreeControllerV3;
                if (rHand != null)
                {
                    //rHand.jointRotationDriveSpring = 60.0f;
                    //rHand.jointRotationDriveDamper = 1.0f;
                }
      
                head = containingAtom.GetStorableByID("headControl") as FreeControllerV3;
                if (head != null)
                {
                    head.RBHoldPositionSpring = 3000.0f;
                    head.RBHoldRotationSpring = 100;
                }

                lThigh = containingAtom.GetStorableByID("lThighControl") as FreeControllerV3;
                if (lThigh != null)
                {
                    //lThigh.jointRotationDriveSpring = 60.0f;
                    //lThigh.jointRotationDriveDamper = 1.0f;
                }

                rThigh = containingAtom.GetStorableByID("rThighControl") as FreeControllerV3;
                if (rThigh != null)
                {
                    //rThigh.jointRotationDriveSpring = 60.0f;
                    //rThigh.jointRotationDriveDamper = 1.0f;
                }


                intensity = new JSONStorableFloat("Breath Intensity", 0.1f, 0, 1, true, true);
                intensity.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(intensity);
                RegisterFloat(intensity);

                chestmove = new JSONStorableFloat("Chest Movement", 15, 0, 30, true, true);
                chestmove.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(chestmove);
                RegisterFloat(chestmove);

                // create random value generation period
                periodJSON = new JSONStorableFloat("Idle Motion Period", 1.0f, 0.1f, 5.0f, false);
                RegisterFloat(periodJSON);
                CreateSlider(periodJSON, true);

                // quickness (smoothness)
                quicknessJSON = new JSONStorableFloat("Idle Motion Move Speed", 1.0f, 0.1f, 5.0f, true);
                RegisterFloat(quicknessJSON);
                CreateSlider(quicknessJSON, true);

                lowerValueJSON = new JSONStorableFloat("lowerValue", 0.2f, -20f, 20f, false);
                upperValueJSON = new JSONStorableFloat("upperValue", 0.6f, -20f, 20f, false);
               
                currentValueJSON = new JSONStorableFloat("currentValue", 0f, 0f, 1f, false, false);
                currentValNxJSON = new JSONStorableFloat("currentValNx", 0f, 0f, 1f, false, false);
                currentValNyJSON = new JSONStorableFloat("currentValNy", 0f, 0f, 1f, false, false);
                currentValNzJSON = new JSONStorableFloat("currentValNz", 0f, 0f, 1f, false, false);
                currentValHxJSON = new JSONStorableFloat("currentValHx", 0f, 0f, 1f, false, false);
                currentValHandsJSON = new JSONStorableFloat("currentValHands", 0f, 0f, 1f, false, false);
                currentValHandsXJSON = new JSONStorableFloat("currentValHands", 0f, 0f, 1f, false, false);
                currentValThighsJSON = new JSONStorableFloat("currentValThighs", 0f, 0f, 1f, false, false);
                currentValSyJSON = new JSONStorableFloat("currentValSy", 0f, 0f, 1f, false, false);
                currentValExpJSON = new JSONStorableFloat("currentValExp", 0f, 0f, 1f, false, false);

                targetValueJSON = new JSONStorableFloat("targetValue", 0f, 0f, 1f, false, false);
                targetValNxJSON = new JSONStorableFloat("targetValNx", 0f, 0f, 1f, false, false);
                targetValNyJSON = new JSONStorableFloat("targetValNy", 0f, 0f, 1f, false, false);
                targetValNzJSON = new JSONStorableFloat("targetValNz", 0f, 0f, 1f, false, false);
                targetValSyJSON = new JSONStorableFloat("targetValNy", 0f, 0f, 1f, false, false);
                targetValHxJSON = new JSONStorableFloat("targetValHx", 0f, 0f, 1f, false, false);
                targetValHandsJSON = new JSONStorableFloat("targetValHands", 0f, 0f, 1f, false, false);
                targetValHandsXJSON = new JSONStorableFloat("targetValHands", 0f, 0f, 1f, false, false);
                targetValThighsJSON = new JSONStorableFloat("targetValThighs", 0f, 0f, 1f, false, false);
                targetValExpJSON = new JSONStorableFloat("targetValExp", 0f, 0f, 1f, false, false);

                randomizeTimerJSON = new JSONStorableFloat("randomizeValue", 0f, 0f, 1f, false, false);

                // targetNameJSON = new JSONStorableString ();
                targetNameJSON = new JSONStorableString("targetName", "");
                targetNameJSON.val = "None";

                //Dictionary<string, string> TargetNameJSON = new Dictionary<string, string>();
                //TargetNameJSON.Add("NeckX", "neck.jointRotationDriveXTarget");
                //TargetNameJSON.Add("NeckY", "neck.jointRotationDriveYTarget");
                //TargetNameJSON.Add("NeckZ", "neck.jointRotationDriveZTarget");


                invertChestBool = new JSONStorableBool("Invert Chest Synchronization", false);
                invertChestBool.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(invertChestBool);
                CreateToggle(invertChestBool, true);

                UseHeadControlBool = new JSONStorableBool("Use Head Control for Idle", false);
                UseHeadControlBool.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(UseHeadControlBool);
                CreateToggle(UseHeadControlBool, true);

                EnableIdleMotionsBool = new JSONStorableBool("Enable Idle Motions", true);
                EnableIdleMotionsBool.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(EnableIdleMotionsBool);
                CreateToggle(EnableIdleMotionsBool, true);

                EnableExpressionsBool = new JSONStorableBool("Enable Expressions", true);
                EnableExpressionsBool.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(EnableExpressionsBool);
                CreateToggle(EnableExpressionsBool, true);



            }
                catch (Exception e)
                {
                SuperController.LogError("Exception caught: " + e);
                }
        }

        public void Update()
        {
            // #### Breathing ####
            float iv = intensity.val;
            
           chest.jointRotationDriveSpring = (chestmove.val *2);

            breath += (iv - breath) * Time.deltaTime * breathLerp;
            breath = Mathf.Clamp01(breath);

            breatheCylce += Time.deltaTime * Mathf.Clamp((0.8f + breath * 7.0f), 0.0f, 20.0f);
            if (breatheMorph != null)
            {
                float power = Mathf.Clamp(breath, 0.4f, 0.6f);
                float cycle = Mathf.Sin(breatheCylce) * power;
                if (invertChestBool.val)
                {
                    cycle = cycle * -1;
                }

                breatheMorph.morphValue = cycle;
                //SuperController.LogMessage("BreatheMorph");
            }


            if (chest != null)
            {
                float power = Remap(breath, 0.0f, 1.0f, 4, 14);
                float cycle = Mathf.Sin(breatheCylce + 0.4f) * power;
                chest.jointRotationDriveXTarget = cycle;
                //SuperController.LogMessage("ChestRotation");
            }


            if (moMorph != null && NoseMorphPinch != null)
            {
                float cycle = Mathf.Sin(breatheCylce);
                float MouthOpenVal = (breath * 1.2f) - (cycle / 15);
                moMorph.morphValue = MouthOpenVal;
                NoseMorphPinch.morphValue = (breath * 0.25f) + (cycle / 14); ;
                //SuperController.LogMessage("MouthMorph");
            }



            // #### IdleMotions ####
            if (EnableIdleMotionsBool.val)
            {
                IdleMotions();
            }

            

            // #### Expressions ####
            if (EnableExpressionsBool.val)
            {
                if (expressionStart == 0 && expressionDone == 1)
                {
                    ExpressionRandomize();
                }

                // Test a defined Expression
                // expressionID = 9;

                if (expressionStart == 1 && expressionDone == 0)
                {
                    Expressions();
                }
            }

        }



        
        public void IdleMotions()
        {
            // IDLE HeadX
            if (UseHeadControlBool.val)
            {
                motionTimerHeX -= Time.deltaTime;
                if (motionTimerHeX < 0.0f)
                {
                    motionTimerHeX = HeadDriveValuesX[2] * UnityEngine.Random.Range(0.7f, 1.3f) * periodJSON.val;
                    targetValHxJSON.val = UnityEngine.Random.Range(HeadDriveValuesX[0], HeadDriveValuesX[1]);
                    //SuperController.LogMessage("motionTimerHX RESET " + motionTimerHX);
                }
                currentValHxJSON.val = Mathf.Lerp(currentValHxJSON.val, targetValHxJSON.val, Time.deltaTime * (HeadDriveValuesX[3] * UnityEngine.Random.Range(0.7f, 1.3f) * quicknessJSON.val));
                //head.transform.Rotate(0, currentValHxJSON.val, 0);
                head.transform.eulerAngles = new Vector3(0, 0, currentValHxJSON.val);
            }
                        
            // IDLE NeckX
            motionTimerNX -= Time.deltaTime;
            if (motionTimerNX < 0.0f)
            {
                motionTimerNX = NeckDriveValuesX[2] * UnityEngine.Random.Range(0.7f, 1.3f) * periodJSON.val;
                targetValNxJSON.val = UnityEngine.Random.Range(NeckDriveValuesX[0], NeckDriveValuesX[1]);
                //SuperController.LogMessage("motionTimerNX RESET " + motionTimerNX);
            }
            currentValNxJSON.val = Mathf.Lerp(currentValNxJSON.val, targetValNxJSON.val, Time.deltaTime * (NeckDriveValuesX[3] * quicknessJSON.val));
            neck.jointRotationDriveXTarget = currentValNxJSON.val;
            neck.jointRotationDriveSpring = ((currentValNxJSON.val * -1) + 20);
                        
            // IDLE NeckY
            motionTimerNY -= Time.deltaTime;
            if (motionTimerNY < 0.0f)
            {
                motionTimerNY = NeckDriveValuesY[2] * UnityEngine.Random.Range(0.7f, 1.3f) * periodJSON.val;
                targetValNyJSON.val = UnityEngine.Random.Range(NeckDriveValuesY[0], NeckDriveValuesY[1]);
                //SuperController.LogMessage("motionTimerNY RESET " + motionTimerNY);
            }
            currentValNyJSON.val = Mathf.Lerp(currentValNyJSON.val, targetValNyJSON.val, Time.deltaTime * (NeckDriveValuesY[3] * quicknessJSON.val));
            neck.jointRotationDriveYTarget = currentValNyJSON.val;
            
            // IDLE NeckZ
            motionTimerNZ -= Time.deltaTime;
            if (motionTimerNZ < 0.0f)
            {
                motionTimerNZ = NeckDriveValuesZ[2] * UnityEngine.Random.Range(0.7f, 1.3f) * periodJSON.val;
                targetValNzJSON.val = UnityEngine.Random.Range(NeckDriveValuesZ[0], NeckDriveValuesZ[1]);
                //SuperController.LogMessage("motionTimerNZ RESET " + motionTimerNZ);
            }
            currentValNzJSON.val = Mathf.Lerp(currentValNzJSON.val, targetValNzJSON.val, Time.deltaTime * (NeckDriveValuesZ[3] * quicknessJSON.val));
            neck.jointRotationDriveZTarget = currentValNzJSON.val;

            // IDLE ShouldersY 
            motionTimerSY -= Time.deltaTime;
            if (motionTimerSY < 0.0f)
            {
                motionTimerSY = ShoulderDriveValuesY[2] * UnityEngine.Random.Range(0.7f, 1.3f) * periodJSON.val;
                targetValSyJSON.val = UnityEngine.Random.Range(ShoulderDriveValuesY[0], ShoulderDriveValuesY[1]);
                mirrorRNDS = UnityEngine.Random.Range(0.8f, 1.4f);
                //SuperController.LogMessage("motionTimerNZ RESET " + motionTimerNZ);
            }
            currentValSyJSON.val = Mathf.Lerp(currentValSyJSON.val, targetValSyJSON.val, Time.deltaTime * (ShoulderDriveValuesY[3] * mirrorRNDS * quicknessJSON.val));
            rArm.jointRotationDriveYTarget = (currentValSyJSON.val * mirrorRNDS);
            lArm.jointRotationDriveYTarget = (currentValSyJSON.val * -1);
            //SuperController.LogMessage("currentValSyJSON.val " + currentValSyJSON.val);

            // IDLE Hands Spring 
            motionTimerHaS -= Time.deltaTime;
            if (motionTimerHaS < 0.0f)
            {
                motionTimerHaS = HandSpringValues[2] * UnityEngine.Random.Range(0.7f, 2.0f) * periodJSON.val;
                targetValHandsJSON.val = UnityEngine.Random.Range(HandSpringValues[0], HandSpringValues[1]);
                mirrorRNDH = UnityEngine.Random.Range(0.8f, 1.4f);
                //SuperController.LogMessage("motionTimerNZ RESET " + motionTimerNZ);
            }
            currentValHandsJSON.val = Mathf.Lerp(currentValHandsJSON.val, targetValHandsJSON.val, Time.deltaTime * (HandSpringValues[3] * mirrorRNDH * quicknessJSON.val));

            rHand.RBHoldPositionSpring = currentValHandsJSON.val;
            lHand.RBHoldPositionSpring = currentValHandsJSON.val;
            //SuperController.LogMessage("currentValSyJSON.val " + currentValSyJSON.val);


            // IDLE Hands Rotate X 
            motionTimerHaX -= Time.deltaTime;
            if (motionTimerHaX < 0.0f)
            {
                motionTimerHaX = HandDriveValuesx[2] * UnityEngine.Random.Range(0.7f, 1.3f) * periodJSON.val;
                if (UnityEngine.Random.Range(1f, 4f) < 1.5f) { motionTimerHaX = 2.5f; }
                targetValHandsXJSON.val = UnityEngine.Random.Range(HandDriveValuesx[0], HandDriveValuesx[1]);
                //SuperController.LogMessage("motionTimerNZ RESET " + motionTimerNZ);
            }
            currentValHandsXJSON.val = Mathf.Lerp(currentValHandsXJSON.val, targetValHandsXJSON.val, Time.deltaTime * (HandDriveValuesx[3] * quicknessJSON.val));

            rHand.jointRotationDriveXTarget = currentValHandsXJSON.val;
            lHand.jointRotationDriveXTarget = currentValHandsXJSON.val;
            //SuperController.LogMessage("currentValSyJSON.val " + currentValSyJSON.val);


            // IDLE ThighsX
            motionTimerTX -= Time.deltaTime;
            if (motionTimerTX < 0.0f)
            {
                motionTimerTX = ThighsDriveValuesX[2] * UnityEngine.Random.Range(0.7f, 1.3f) * periodJSON.val;
                targetValThighsJSON.val = UnityEngine.Random.Range(ThighsDriveValuesX[0], ThighsDriveValuesX[1]);
                //SuperController.LogMessage("motionTimerTX RESET " + motionTimerTX);
            }
            currentValThighsJSON.val = Mathf.Lerp(currentValThighsJSON.val, targetValThighsJSON.val, Time.deltaTime * (ThighsDriveValuesX[3] * UnityEngine.Random.Range(0.7f, 1.3f) * quicknessJSON.val));
            rThigh.jointRotationDriveXTarget = currentValThighsJSON.val;
            lThigh.jointRotationDriveXTarget = (currentValThighsJSON.val * -1);
            rThigh.jointRotationDriveZTarget = Remap(currentValThighsJSON.val, -30.0f, 30.0f, -50.0f, 10.0f) * -1;
            lThigh.jointRotationDriveZTarget = Remap(currentValThighsJSON.val, -30.0f, 30.0f, -5.0f, 50.0f) * -1;
            //SuperController.LogMessage("currentValSyJSON.val " + currentValSyJSON.val);
        }



        public void ExpressionRandomize()
        {
            // SuperController.LogMessage("ExpressionRandomize");
            expressionTimer1 -= Time.deltaTime;
            if (expressionTimer1 < 0.0f)
            {
                // delay to next expression start.
                expressionTimer1 = 8 + UnityEngine.Random.Range(0.0f, 10.0f);
                expressionID = UnityEngine.Random.Range(1, 9);
                
                // check if same ID again
                if (CurrExprID == expressionID)
                {
                    expressionID = UnityEngine.Random.Range(1, 9);
                }
                CurrExprID = expressionID;
                expressionStart = 1;
                expressionDone = 0;
                //SuperController.LogMessage("expressionID = " + expressionID);
            }


        }


        //Expressions
        public void Expressions()
        {
            if (expressionID == 1)
            {
                // Expression 1 (Lips)
                // ExpressionDriveValues {StartValue, MaxValue, smoothness}
                float[] ExpressionDriveValues = { 0.0f, 0.7f, 4.0f };

                if (RevExpression == 0)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[1], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphLUD.morphValue = currentValExpJSON.val;
                    MouthMorphLBI.morphValue = currentValExpJSON.val;
                    //SuperController.LogMessage("currentValExpJSON.val " + currentValExpJSON.val + " - " + ExpressionDriveValues[1]);
                    if (currentValExpJSON.val > ExpressionDriveValues[1] - 0.0001f)
                    {
                        RevExpression = 1;
                    }
                }
                if (RevExpression == 1)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[0], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphLUD.morphValue = currentValExpJSON.val;
                    MouthMorphLBI.morphValue = currentValExpJSON.val;

                    if (currentValExpJSON.val < ExpressionDriveValues[0] + 0.001f)
                    {
                        expressionStart = 0;
                        RevExpression = 0;
                        expressionDone = 1;
                    }
                }
            }

            if (expressionID == 2)
            {
                // Expression 2 (BrowsEyes)
                // ExpressionDriveValues {StartValue, MaxValue, smoothness}
                float[] ExpressionDriveValues = { 0.0f, 0.8f, 6.0f };

                if (RevExpression == 0)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[1], Time.deltaTime * ExpressionDriveValues[2]);
                    FaceMorphBrowsArch.morphValue = currentValExpJSON.val;
                    EyesMorphEyesClosedL.morphValue = currentValExpJSON.val * -0.3f;
                    EyesMorphEyesClosedR.morphValue = currentValExpJSON.val * -0.3f;
                    EyesMorphEyesRound.morphValue = currentValExpJSON.val * 1.4f;


                    //SuperController.LogMessage("currentValExpJSON.val " + currentValExpJSON.val + " - " + ExpressionDriveValues[1]);
                    if (currentValExpJSON.val > ExpressionDriveValues[1] - 0.01f)
                    {
                        RevExpression = 1;
                    }
                }
                if (RevExpression == 1)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[0], Time.deltaTime * ExpressionDriveValues[2]);
                    FaceMorphBrowsArch.morphValue = currentValExpJSON.val;
                    EyesMorphEyesClosedL.morphValue = currentValExpJSON.val * -0.3f;
                    EyesMorphEyesClosedR.morphValue = currentValExpJSON.val * -0.3f;
                    EyesMorphEyesRound.morphValue = currentValExpJSON.val * 1.4f;

                    if (currentValExpJSON.val < ExpressionDriveValues[0] + 0.001f)
                    {
                        expressionStart = 0;
                        RevExpression = 0;
                        expressionDone = 1;
                    }
                }
            }

            if (expressionID == 3)
            {
                // Expression 2 (EyesClose)
                // ExpressionDriveValues {StartValue, MaxValue, smoothness}
                float[] ExpressionDriveValues = { 0.0f, 1.0f, 4.0f };

                if (RevExpression == 0)
                {
                    autoBlEnabled.val = false;
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[1], Time.deltaTime * ExpressionDriveValues[2]);
                    EyesMorphEyesClosedL.morphValue = currentValExpJSON.val;
                    EyesMorphEyesClosedR.morphValue = currentValExpJSON.val;

                    if (currentValExpJSON.val > ExpressionDriveValues[1] - 0.01f)
                    {
                        RevExpression = 1;
                    }
                }
                if (RevExpression == 1)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[0], Time.deltaTime * (ExpressionDriveValues[2] * 1.3f));
                    EyesMorphEyesClosedL.morphValue = currentValExpJSON.val;
                    EyesMorphEyesClosedR.morphValue = currentValExpJSON.val;

                    if (currentValExpJSON.val < ExpressionDriveValues[0] + 0.001f)
                    {
                        expressionStart = 0;
                        RevExpression = 0;
                        expressionDone = 1;
                        autoBlEnabled.val = true;
                    }
                }
            }

            if (expressionID == 4)
            {
                // Expression 2 (Eyes Wink)
                // ExpressionDriveValues {StartValue, MaxValue, smoothness}
                float[] ExpressionDriveValues = { 0.0f, 1.0f, 15.0f };

                if (RevExpression == 0)
                {
                    autoBlEnabled.val = false;
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[1], Time.deltaTime * ExpressionDriveValues[2]);
                    EyesMorphEyesClosedR.morphValue = currentValExpJSON.val;
                    FaceMorphBrowDR.morphValue = currentValExpJSON.val / 2f;

                    if (currentValExpJSON.val > ExpressionDriveValues[1] - 0.01f)
                    {
                        RevExpression = 1;
                    }
                }
                if (RevExpression == 1)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[0], Time.deltaTime * ExpressionDriveValues[2]);
                    EyesMorphEyesClosedR.morphValue = currentValExpJSON.val;
                    FaceMorphBrowDR.morphValue = currentValExpJSON.val / 4f;

                    if (currentValExpJSON.val < ExpressionDriveValues[0] + 0.001f)
                    {
                        expressionStart = 0;
                        RevExpression = 0;
                        expressionDone = 1;
                        autoBlEnabled.val = true;
                    }
                }
            }

            if (expressionID == 5)
            {
                // Expression 2 (Mouth Shh!)
                // ExpressionDriveValues {StartValue, MaxValue, smoothness}
                float[] ExpressionDriveValues = { 0.0f, 0.7f, 1.5f };

                if (RevExpression == 0)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[1], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphSH.morphValue = currentValExpJSON.val;
                    
                    if (currentValExpJSON.val > ExpressionDriveValues[1] - 0.01f)
                    {
                        RevExpression = 1;
                    }
                }
                if (RevExpression == 1)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[0], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphSH.morphValue = currentValExpJSON.val;
                    

                    if (currentValExpJSON.val < ExpressionDriveValues[0] + 0.001f)
                    {
                        expressionStart = 0;
                        RevExpression = 0;
                        expressionDone = 1;
                    }
                }
            }

            if (expressionID == 6)
            {
                // Expression 2 (Mouth Corner)
                // ExpressionDriveValues {StartValue, MaxValue, smoothness}
                float[] ExpressionDriveValues = { 0.0f, 2.5f, 1.6f };

                if (RevExpression == 0)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[1], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphMCD.morphValue = currentValExpJSON.val;

                    if (currentValExpJSON.val > ExpressionDriveValues[1] - 0.0001f)
                    {
                        RevExpression = 1;
                    }
                }
                if (RevExpression == 1)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[0], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphMCD.morphValue = currentValExpJSON.val;


                    if (currentValExpJSON.val < ExpressionDriveValues[0] + 0.001f)
                    {
                        expressionStart = 0;
                        RevExpression = 0;
                        expressionDone = 1;
                    }
                }
            }

            if (expressionID == 7)
            {
                // Expression 2 (Tongue)
                // ExpressionDriveValues {StartValue, MaxValue, smoothness}
                float[] ExpressionDriveValues = { 0.0f, 1.0f, 1.3f };

                if (RevExpression == 0)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[1], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphAA.morphValue = currentValExpJSON.val * 1.3f;
                    TongueMorphIO.morphValue = (currentValExpJSON.val * -0.9f);
                    TongueMorphCu.morphValue = (currentValExpJSON.val / 3f);

                    if (currentValExpJSON.val > ExpressionDriveValues[1] - 0.01f)
                    {
                       RevExpression = 1;
                    }
                }
                if (RevExpression == 1)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[0], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphAA.morphValue = currentValExpJSON.val * 1.3f;
                    TongueMorphIO.morphValue = (currentValExpJSON.val * -0.9f);
                    TongueMorphCu.morphValue = (currentValExpJSON.val / 3f);

                    if (currentValExpJSON.val < ExpressionDriveValues[0] + 0.001f)
                    {
                        expressionStart = 0;
                        RevExpression = 0;
                        expressionDone = 1;
                    }
                }

            }

            if (expressionID == 8)
            {
                // Expression 2 (Nose tilt)
                // ExpressionDriveValues {StartValue, MaxValue, smoothness}
                float[] ExpressionDriveValues = { 0.0f, 0.6f, 4.5f };

                if (RevExpression == 0)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[1], Time.deltaTime * ExpressionDriveValues[2]);
                    NoseMorphTilt.morphValue = currentValExpJSON.val * -1;
                    MouthMorphAA.morphValue = currentValExpJSON.val * 1.3f;
                    FaceMorphBrowsIU.morphValue = currentValExpJSON.val * -1;


                    // Head nod

                    if (HeadNod == 0)
                    {
                        Nod_initialVal = targetValNxJSON.val;
                        targetValNxJSON.val = currentValNxJSON.val + 20.0f;
                        HeadNod = 1;
                    } 
                    currentValNxJSON.val = Mathf.Lerp(currentValNxJSON.val, targetValNxJSON.val, Time.deltaTime * 3.5f);
                    neck.jointRotationDriveXTarget = currentValNxJSON.val;
                    



                    //SuperController.LogMessage("currentValExpJSON.val " + currentValExpJSON.val + " - " + ExpressionDriveValues[1]);
                    if (currentValExpJSON.val > ExpressionDriveValues[1] - 0.1f)
                    {
                       RevExpression = 1;
                    }
                }
                if (RevExpression == 1)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[0], Time.deltaTime * ExpressionDriveValues[2]);
                    NoseMorphTilt.morphValue = currentValExpJSON.val * -1;
                    MouthMorphAA.morphValue = currentValExpJSON.val * 1.3f;
                    FaceMorphBrowsIU.morphValue = currentValExpJSON.val * -1;
                    
                    // Head Nod
                    currentValNxJSON.val = Mathf.Lerp(currentValNxJSON.val, Nod_initialVal, Time.deltaTime * 1.5f);
                    neck.jointRotationDriveXTarget = currentValNxJSON.val;

                    if (currentValExpJSON.val < ExpressionDriveValues[0] + 0.0001f)
                    {
                        expressionStart = 0;
                        RevExpression = 0;
                        expressionDone = 1;
                        HeadNod = 0;
                    }
                }

            }

            if (expressionID == 9)
            {
                // Expression 2 (Mouth Open)
                // ExpressionDriveValues {StartValue, MaxValue, smoothness}
                float[] ExpressionDriveValues = { 0.0f, 0.8f, 1.2f };

                if (RevExpression == 0)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[1], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphAA.morphValue = currentValExpJSON.val * 0.9f;
                    FaceMorphBrowsIU.morphValue = currentValExpJSON.val * 1.3f;



                    if (currentValExpJSON.val > ExpressionDriveValues[1] - 0.001f)
                    {
                        RevExpression = 1;
                    }
                }
                if (RevExpression == 1)
                {
                    currentValExpJSON.val = Mathf.Lerp(currentValExpJSON.val, ExpressionDriveValues[0], Time.deltaTime * ExpressionDriveValues[2]);
                    MouthMorphAA.morphValue = currentValExpJSON.val * 0.9f;
                    FaceMorphBrowsIU.morphValue = currentValExpJSON.val * 1.3f;



                    if (currentValExpJSON.val < ExpressionDriveValues[0] + 0.001f)
                    {
                        expressionStart = 0;
                        RevExpression = 0;
                        expressionDone = 1;
                    }
                }

            }


        }







        private float Remap(float x, float x1, float x2, float y1, float y2)
        {
            var m = (y2 - y1) / (x2 - x1);
            var c = y1 - m * x1;

            return m * x + c;
        }
    }
}
