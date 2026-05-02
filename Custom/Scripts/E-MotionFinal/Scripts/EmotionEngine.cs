using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
using System.IO;
using MVR.FileManagementSecure;
using System;

namespace VRAdultFun
{
    partial class EmotionEngine : MVRScript
    {
        #region Initilation Variables
        //private static Atom debugUI;
        //private static UITextControl debugUIControl;
		private static float playerLookDirectAngle = 25.0f;
		private static float hFuzz = 0.0f;
		private static float vFuzz = 0.0f;
		private static float lastGloss = 0.0f;
		private static float currentGloss = 0.0f;
		private static float normalizedGloss = 0.0f;
		private static float rollTimer = 0.0f;
		private static float eyeMoveDist = 0.0f;
		private static float blinkSpeed = 0.0f;
		private static bool allSetup = false;
		private static float tempFloat = 0.0f;
		private static float tempFloat2 = 0.0f;
		private static string currentLook = "";
		private static string currentBrow = "";
		private static string currentEye = "";
		private static string currentMouth = "";
        private static float debugClock = 0.0f;
		private static string debugString = "";
		private static string testString = "";
		private static bool loadedFocusTarget = false;
        private static string logLine;
        private static Atom aCube;
		private static FreeControllerV3 aCubeController;
        private static Atom aCube2;
		private static FreeControllerV3 aCubeController2;
        private static Atom aCube3;
		private static FreeControllerV3 aCubeController3;
        private static Atom aCube4;
		private static FreeControllerV3 aCubeController4;
        private static float saccadeClock = 0.0f;
		private static float saccadeRepeat = 0.0f;
        private static float eyeClock = 0.0f;
		private static float eyeCloseMaxMorph = 1.1f;
		private static float eyeOpenMaxMorph = -0.1f;
        private static float randomX = 0.0f;
        private static float randomY = 0.0f;
        private static string currentInterest = "RandomF";
        private static string prevInterest = "RandomF";
		private static string lastFrameInterest = "RandomF";
        private static float currentInterestLevel = 0.0f;
        private static float maxInterestLevel = 100.0f;
        private static float interestClock = 0.0f;
        private static float interestArousal = 0.0f;
        private static float interestValence = 0.0f;
        private static float interestMaxSmile = 0.65f;
		private static float interestArousalLast = 0.0f;
        private static bool interestKissing = false;
        private static float pExtraversion = 80.0f; //Random.Range(20.0f,100.0f);//80.0f;
        private static float pAgreeableness = 60.0f; //Random.Range(40.0f,100.0f);//60.0f;
        private static float pStableness = 40.0f; //Random.Range(10.0f,100.0f);//40.0f;
        private static float peronalityAdjustH = 0.0f;
		private static float lastAdjustH = 0.0f;
		private static float lastAdjustV = 0.0f;
        private static float peronalityAdjustV = 0.0f;
        private static float endAdjustV = 0.0f;
        private static float endAdjustH = 0.0f;
        private static bool amGlancing = false;
        private static float glanceClock = 0.0f;
        private static float glanceTimeout = 0.0f;
        private static float velocityH = 0.0f;
        private static float velocityV = 0.0f;
        private static float velocityLastH = 0.0f;
        private static float velocityLastV = 0.0f;
        private static float eyesNonDirectClock = 0.0f;
        private static float eyesNonDirectAngle = 5.0f;
		private static float eyesDirectClock = 0.0f;
        private static float fuzzyLock = 1.0f;
		private static float fuzzyLockActual = 1.0f;
		private static float headToEyeController = 0.0f;
        private static Vector3 refAngle;
		private static Vector3 oldEyePos;
		private JSONStorable headAudio;
		private AudioSource audiosource;
		private static float lastRandomVoice = 0.0f;
		public NamedAudioClip audioClip;
		private static string voiceLast = "";
		private static bool voiceMoan = false;
		private static float voiceOpenAdjust = 0.0f;
		private static float voicePuckerAdjust = 0.0f;
		private static float slowAdjustment = 0.0f;
		private static float fastAdjustment = 0.0f;
		private static float adjustPart = 0.0f;
		private static float gazeAdjust = 1.0f;
		private static bool randomResetDir = true;
		private static string[] tempvalues;
		private static bool interestRepeating = false;
		private static float interestRepeat = 0.0f;
		private static float headActivityBoost = 0.0f;
		private static float lHandActivityBoost = 0.0f;
		private static float rHandActivityBoost = 0.0f;
		private static float blinkRepeat = 0.0f;
		private static float blinkRepTimer = 0.0f;
		private static float mouthOpenTimer = 0.0f;
		private static bool mouthCanOpen = true;
		private static float headDelayTimer = 0.0f;
		private static float armAdjustment = 1.0f;
		private static float leanAdjustment = 0.0f;
		private static float twistAdjustment = 0.0f;
		private static float randomBaseDistance = 0.0f;
		private static float randomBaseHeight = 0.0f;
		private static float randomBaseOffset = 0.0f;
		private static float baseGloss = 0.0f;
		private static float baseSpec = 0.0f;
		private static float baseSpecBump = 0.0f;
		private static bool materialCaptured = false;
		private static float heatupValue = 0.0f;
		private static bool soundsLoaded = false;
		private static bool resetMorphs = false;
		private static float headUsedLeftRight = 0.0f;
		private static float headUsedUpDown = 0.0f;
		private static float headLeftRight = 0.0f;
		private static float headUpDown = 0.0f;
		private static float headLastLeftRight = 0.0f;
		private static float headLastLeftRightActual = 0.0f;
		private static float headLastUpDown = 0.0f;
		private static float tongueExpressionOffset = 0.0f;
		private static float tongueExpressionOffsetActual = 0.0f;
		private static bool lookOverride = false;
		private static float lookOverrideTimer = 0.0f;
		private static float idleFlip = 1.0f;
		private static float idleLArmTimeout = 0.0f;
		private static float idleRArmTimeout = 0.0f;
		private static float idleLLegTimeout = 0.0f;
		private static float idleRLegTimeout = 0.0f;
		private static float idleBodyTimeout = 0.0f;
		private static float abAngleUp = 0.0f;
		private static float minInterest = 0.0f;

		private static float pantingTimeout = 0.0f;
		private static float pantCount = 0.0f;
		private static float pantSmooth = 0.0f;
		
		private static float mouthBreath = 0.0f;
		private static bool breathWMouth = false;
		
		private static float expandSmooth = 0.0f;
		
		private static float decisionArousal = 0.0f;
		private static float decisionValence = 0.0f;
		private static float decisionRepeat = 0.0f;

		private static string chosenRandom = "";
		private static bool interestInterrupt = false;

		private static string uiString = "";


		private static bool breastContact = false;
		private static bool lBunnyHands = false;
		private static bool rBunnyHands = false;
		private static bool lBellyHands = false;
		private static bool rBellyHands = false;
		private static bool lBreastHands = false;
		private static bool rBreastHands = false;
		private static bool lFancyHands = false;
		private static bool rFancyHands = false;
		private static bool lSideHands = true;
		private static bool rSideHands = true;
		private static bool lHipHands = false;
		private static bool rHipHands = false;
		private static bool lBackArchHands = false;
		private static bool rBackArchHands = false;

		
		private static float interestPeakArousal = 0.0f;
		private static float interestPeakArousalTimer = 0.0f;
		private static float interestPeakValence = 0.0f;
		private static float interestPeakValenceTimer = 0.0f;
		private static float exertion = 0.0f;
		
		private static float neckSpringActual = 50.0f;
		private static float neckSpringTarget = 50.0f;
		private static float neckXActual = 0.0f;
		private static float neckXTarget = 0.0f;
		
		
		private static float twistActual = 0.0f;
		private static float twistTarget = 0.0f;
		private static float twistSpeed = 0.03f;

		private static float twist2Actual = 0.0f;
		private static float twist2Target = 0.0f;

		private static float lElbowHoldActual = 0.0f;
		private static float lElbowHoldTarget = 0.0f;
		private static float rElbowHoldActual = 0.0f;
		private static float rElbowHoldTarget = 0.0f;

		private static float lElbowActual = 0.0f;
		private static float lElbowTarget = 0.0f;
		private static float rElbowActual = 0.0f;
		private static float rElbowTarget = 0.0f;

		private static float lThighHoldActual = 0.0f;
		private static float lThighHoldTarget = 0.0f;
		private static float rThighHoldActual = 0.0f;
		private static float rThighHoldTarget = 0.0f;

		private static float lThighActual = 0.0f;
		private static float lThighTarget = 0.0f;
		private static float rThighActual = 0.0f;
		private static float rThighTarget = 0.0f;

		private static float lKneeHoldActual = 0.0f;
		private static float lKneeHoldTarget = 0.0f;
		private static float rKneeHoldActual = 0.0f;
		private static float rKneeHoldTarget = 0.0f;

		private static float lKneeActual = 0.0f;
		private static float lKneeTarget = 0.0f;
		private static float rKneeActual = 0.0f;
		private static float rKneeTarget = 0.0f;
		
		
		private static float dynAdjustActual = 0.0f;
		private static float dynAdjustTarget = 0.0f;
		
		
		
		private static float lShoulderX = 0.0f;
		private static float lShoulderY = 0.0f;
		private static float rShoulderX = 0.0f;
		private static float rShoulderY = 0.0f;
		private static float lElbowX = 0.0f;
		private static float lElbowY = 0.0f;
		private static float rElbowX = 0.0f;
		private static float rElbowY = 0.0f;
		private static float lKneeX = 0.0f;
		private static float lKneeY = 0.0f;
		private static float rKneeX = 0.0f;
		private static float rKneeY = 0.0f;
		
		private static float adjustTimeout = 0.0f;
		private static float adjustWaitTime = 30.0f;
		
		
		private static UIDynamicButton personalityButton;
		private static UIDynamicButton targetButton;
		private static UIDynamicButton gazeButton;
		private static UIDynamicButton distanceButton;
		private static UIDynamicButton featureButton;
		private static UIDynamicButton idleButton;
		private static UIDynamicButton eyeButton;
		private static UIDynamicButton characterButton;
		protected static bool uiShowingPersonality = false;
		protected static bool uiShowingFeatures = false;
		protected static bool uiShowingTarget = false;
		protected static bool uiShowingGaze = false;
		protected static bool uiShowingDistAngle = false;
		protected static bool uiShowingIdleBreathing = false;
		protected static bool uiShowingEye = false;
		protected static bool uiShowingCharacter = false;

		protected JSONStorableFloat uiExtraversion;
		protected JSONStorableFloat uiAgreeableness;
		protected JSONStorableFloat uiStableness;
		protected JSONStorableFloat uiBreatheSpeed;
		protected JSONStorableFloat uiBreatheRaiseMultiplier;
		protected JSONStorableFloat uiBreatheExpandMultiplier;
		protected JSONStorableFloat uiGazeSpeed;
		protected JSONStorableFloat uiGazeVariation;
		private static float gazeVariation = 0.0f;
		protected JSONStorableBool uiGazeAvoid;
		protected JSONStorableFloat uiGazeLookTime;
		protected JSONStorableFloat uiGazeAvoidTime;
		protected JSONStorableBool uiGazeGlance;
		protected JSONStorableFloat uiRollSpeed;
		protected JSONStorableFloat uiSaccadeSpeed;
		protected JSONStorableFloat uiSaccadeAmount;
		protected JSONStorableFloat uiSaccadeWanderMult;
		protected JSONStorableFloat uiBlinkSpeed;
		protected JSONStorableFloat uiEyeOpenMaxMorph;
		protected JSONStorableFloat uiEyeCloseMaxMorph;
		protected JSONStorableFloat uiArousalSpeed;
		protected JSONStorableFloat uiValenceSpeed;
		protected JSONStorableFloat uiMoodSpeed;
		protected JSONStorableFloat uiInterestSpeed;
		protected JSONStorableFloat uiInterestRate;
		protected JSONStorableBool uiDoHead;
		protected JSONStorableBool uiDoShoulders;
		protected JSONStorableFloat uiShoulderBack;
		protected JSONStorableFloat uiShoulderAmount;
		protected JSONStorableFloat uiShoulderHeight;
		protected JSONStorableBool uiDoChest;
		protected JSONStorableFloat uiChestAmount;
		protected JSONStorableBool uiDoHands;
		protected JSONStorableBool uiConfigHead;
		protected JSONStorableBool uiUsePerson2;
		protected JSONStorableBool uiShowStats;
		protected JSONStorableBool uiDoKiss;
		protected JSONStorableFloat uiKissAmount;
		protected JSONStorableBool uiDoBlowjob;
		protected JSONStorableFloat uiBlowjobAmount;
		protected JSONStorableBool uiDoSex;
		protected JSONStorableFloat uiSexAmount;
		protected JSONStorableStringChooser uiFocusTarget;
		protected JSONStorableStringChooser uiObjectTarget;
		protected JSONStorableBool uiTargetLook;
		protected JSONStorableFloat uiPersonalSpace;
		protected JSONStorableFloat uiDirectGaze;
		protected JSONStorableFloat uiPeripheralGaze;
		protected JSONStorableFloat uiOutOfGaze;
		protected JSONStorableFloat uiCloseToFaceDist;
		protected JSONStorableFloat uiKissingDist;
		protected JSONStorableFloat uiInteractDist;
		protected JSONStorableFloat uiMaxMorphSmile;
		protected JSONStorableFloat uiEyeUpdate;
		protected JSONStorableFloat uiHeadInterest;
		protected JSONStorableFloat uiLHandInterest;
		protected JSONStorableFloat uiRHandInterest;
		protected JSONStorableFloat uiSelfLHandInterest;
		protected JSONStorableFloat uiSelfRHandInterest;
		protected JSONStorableFloat uiPenisInterest;
		protected JSONStorableFloat uiObjectInterest;
		protected JSONStorableBool uiSavePreset;
		protected JSONStorableBool uiLoadPreset;
		protected JSONStorableBool uiLoadDefaults;
		protected JSONStorableBool uiDoMorphs;
		protected JSONStorableBool uiDoSounds;
		protected JSONStorableFloat uiSoundVolume;
		protected JSONStorableFloat triggerArousal;
		protected JSONStorableFloat triggerValence;
		protected JSONStorableFloat uiIdleAmount;
		protected JSONStorableFloat uiIdleLegAmount;
		protected JSONStorableFloat uiIdleArmHold;
		protected JSONStorableFloat uiIdleLegHold;
		protected JSONStorableFloat uiIdleHoldPow;
		protected JSONStorableFloat uiIdleArmAmount;
		protected JSONStorableFloat uiIdleArmOffset;
		protected JSONStorableFloat uiIdleArmRotOffset;
		protected JSONStorableFloat uiIdleChance;
		protected JSONStorableFloat uiIdleArmChance;
		protected JSONStorableFloat uiIdleSpeed;
		protected JSONStorableFloat uiIdleArmSpeed;
		protected JSONStorableFloat uiIdleBodyDelay;
		protected JSONStorableFloat uiIdleArmDelay;
		protected JSONStorableFloat uiIdleLegDelay;
		protected JSONStorableBool uiSetupComplete;
		protected JSONStorableFloat uiChestHeightOffset;
		protected JSONStorableFloat uiGlanceTimeout;
		protected JSONStorableFloat uiPupilDialation;
		protected JSONStorableFloat uiPupilRate;
		protected JSONStorableFloat uiMaxHeadRoll;
		protected JSONStorableFloat uiRollChance;
		protected JSONStorableFloat uiAnimationSpeed;
		protected JSONStorableFloat uiExpressionChance;
		protected JSONStorableFloat uiGazeMaxUp;
		protected JSONStorableFloat uiGazeMaxDown;
		protected JSONStorableFloat uiGazeMaxSideways;
		protected JSONStorableFloat uiRandomBaseHeight;
		protected JSONStorableFloat uiRandomBaseDistance;
		protected JSONStorableFloat uiRandomBaseOffset;
		protected JSONStorableFloat uiGazeDirectLookDelay;
		protected JSONStorableFloat uiIndirectDecay;
		protected JSONStorableBool uiEffectMaterial;
		protected JSONStorableFloat uiMaterialMult;
		protected JSONStorableFloat uiMouthOpenOffset;
		protected JSONStorableFloat uiLipsCloseOffset;
		protected JSONStorableFloat uiHeadAngleOffset;
		protected JSONStorableFloat uiTongueLength;
		protected JSONStorableFloat uiTongueRaise;
		protected JSONStorableBool uiObjectAsPrimary;
		protected JSONStorableBool uiEyeControl;
		protected JSONStorableFloat uiBreastLift;
		protected JSONStorableFloat uiExpressionLength;
		protected JSONStorableFloat uicustomarmsweight;
		protected JSONStorableFloat uicustombodyweight;
		protected JSONStorableFloat uicustomfeetweight;
		protected JSONStorableFloat uicustomhandsweight;
		protected JSONStorableFloat uicustomheadweight;
		protected JSONStorableFloat uicustomlegsweight;
		protected JSONStorableBool uicustomweights;
		protected JSONStorableFloat uiSmileOffset;
		protected JSONStorableString uiArousalStatus;
		protected JSONStorableString uiValenceStatus;
		protected JSONStorableString backendCurrentFocusTarget;
		protected JSONStorableString backendCurrentObjectTarget;
		protected JSONStorableFloat uiHeadRotStrength;
		protected JSONStorableFloat uiHeadRotDamper;
		protected JSONStorableBool uiDynamicDirectAngle;
		protected JSONStorableFloat uiSmileDamper;
		protected JSONStorableFloat uiMinInterest;
		protected JSONStorableBool uiUseBodyMotion;
    protected JSONStorableFloat uiEyeMoveBlinkDist;
    protected JSONStorableBool uiControlTongue;
    protected JSONStorableBool uiControlJaw;
		protected JSONStorableBool uiOnlyBuiltIn;
		protected JSONStorableFloat uiSmileSuppression;
		protected JSONStorableFloat uiVariationChance;
		protected JSONStorableFloat uiMoanChance;
		
		

		protected JSONStorableBool uiShowHelp;
		protected JSONStorableString uiFeatureControlHelp;
		protected JSONStorableString uiLookAdjustmentHelp;
		protected JSONStorableString uiEyeControlHelp;
		protected JSONStorableString uiPersonalitySettingsHelp;
		protected JSONStorableString uiDistancesAndAnglesHelp;
		protected JSONStorableString uiGazeControlsHelp;
		protected JSONStorableString uiBodyMovementHelp;
		protected JSONStorableString uiTargetControlHelp;
		protected JSONStorableString uiOverviewLeftHelp;
		protected JSONStorableString uiOverviewRightHelp;
		
		protected JSONStorableFloat uiMovementInterest;
		protected JSONStorableFloat uiMovementInterestArousal;
		
		private static bool onlyUseBuiltIn = false;

		private static float movementInterest = 1.0f;

		protected string lastPath = "Custom/E-Motion/Presets";

		private static float uiExpressionLengthVal;
		

		private static Atom emTarget;
		private static string emTargetName;
		private static FreeControllerV3 emTargetController;
		private static float emTargetDistance;
		private static float emTargetPelvisDistance;
		private static float emTargetDir;
		private static float emTargetHeadDir;
		private static Transform emTargetTransform;
		//private static Vector3 emTargetPosPrev;
		private static float interestEMTarget;
		private static Vector3 lookAtPosition;
		private static float morphSpeed;
		private static bool useObjectUp = false;
		private static Atom objectUp;
		private static bool useObjectForward = false;
		private static Atom objectForward;
		private static bool useObjectLeft = false;
		private static Atom objectLeft;
		private static bool useObjectRight = false;
		private static Atom objectRight;
		private static FreeControllerV3 objectController;
		
        private static float gAvoidance = Mathf.Clamp((((100.0f - pExtraversion) + (100.0f - pStableness)) / 2.0f), 30.0f, 100.0f);
        private static float gDuration = 10.0f * (Mathf.Clamp((pStableness / 5.0f) + ((100.0f - pAgreeableness) / 2.0f) + (pExtraversion / 5.0f), 0.0f, 100.0f) / 100.0f);
        private static float gFrequency = Mathf.Clamp((pStableness / 2.0f) + (pExtraversion / 2.0f), 0.0f, 100.0f);
        private static float gHeadSpeed = 1.75f;
		private static float adjustedSpeed = 0.0f;
        private static bool gDirectionCenter;
        private static bool gDirectionUp;
        private static bool gDirectionDown;
        private static bool gDirectionSide;
        private static float gHeadRoll = 0.0f;
        private static float gHeadRollIdle = 0.0f;
        private static float gHeadRollTarget = 0.0f;
		private static float gHeadRollTimer = 0.0f;
        private static float gAvoid = 0.0f;
        private static float gAvoidHeight = 0.0f;
        private static float gAvoidanceClock = 0.0f;
		private static float gAvoidingClock = 0.0f;
		private static string gAvoidInterest = "";
        private static float sexActionNeckX = 0.0f;
        private static float sexActionNeckActual = 0.0f;
		private static float maxHeadRoll = 0.0f;
		private static float actualH = 0.0f;
		private static float actualV = 0.0f;
		private static float targetH = 0.0f;
		private static float targetV = 0.0f;
		private static float variationChance;
		private static float moanChance;

        private static string mainInterest = "RandomF";
        private static float mainValue = 0.0f;
        private static string secondInterest = "RandomF";
        private static float secondValue = 0.0f;
        private static string mainOld = "RandomF";
        private static string secondOld = "RandomF";
        private static bool mainSwitch = false;
        private static bool secondSwitch = false;
        private static float mainClock = 0.0f;
        private static float secondClock = 0.0f;

        private static StateMachine emotionSM;
        private static StateMachine lookSM;
        private static State lastLookState;
        private static StateMachine systemSM;
        private static StateMachine browSM;
        private static State lastBrowState;
        private static StateMachine mouthSM;
        private static State lastMouthState;
        private static StateMachine eyesSM;
        private static State lastEyesState;

        private static Atom person;
		private static bool personIsMale = false;
        private static Vector3 curEyePosition;
        private static Vector3 curEyeAngles;
        private static float eyeUpdateTime = 0.05f;
        private static float eyeUpdateClock = 0.0f;
        private static JSONStorable personEyes;
        private static JSONStorable personEyelids;
        private static FreeControllerV3 eyeController;
        private static FreeControllerV3 headActual;
        private static FreeControllerV3 headController;
		private static Vector3 prevPosHead;
        private static FreeControllerV3 neckController;
        private static FreeControllerV3 chestController;
		private static Vector3 prevPosChest;
        private static FreeControllerV3 lBreastController;
        private static FreeControllerV3 rBreastController;
        private static FreeControllerV3 pelvisController;
        private static FreeControllerV3 pelvis2Controller;
        private static FreeControllerV3 lowerabsController;
        private static FreeControllerV3 abdomenController;
        private static FreeControllerV3 hipController;
		private static Vector3 prevPosHip;
        private static FreeControllerV3 lHipController;
        private static FreeControllerV3 rHipController;
        private static FreeControllerV3 lHandController;
		private static Vector3 prevPosLHand;
        private static FreeControllerV3 rHandController;
		private static Vector3 prevPosRHand;
        private static FreeControllerV3 lShoulderController;
        private static FreeControllerV3 rShoulderController;
        private static FreeControllerV3 lArmController;
        private static FreeControllerV3 rArmController;
        private static FreeControllerV3 lElbowController;
        private static FreeControllerV3 rElbowController;
        private static FreeControllerV3 lThighController;
        private static FreeControllerV3 rThighController;
        private static FreeControllerV3 lKneeController;
        private static FreeControllerV3 rKneeController;
        private static FreeControllerV3 lFootController;
		private static Vector3 prevPoslFoot;
        private static FreeControllerV3 rFootController;
		private static Vector3 prevPosrFoot;
		private static float chestControllerYAngle = 0.0f;

        private static bool lookAction = false;
        private static float lookVariation = 1.0f;
        private static float browVariation = 1.0f;
        private static float eyeVariation = 1.0f;
        private static float mouthVariation = 1.0f;
        private static float saccadeAmount = 0.0f;
		private static float shoulderUp = 0.0f;
		private static float saccadeOffsetCounter = 0.0f;
		private static string lastSaccade = "";
		private static float rollChance = 0.0f;
		private static bool smiledlast = false;
		private static float smileDamper = 0.0f;
		private static float smileDamperTimeout = 0.0f;
		
		
		private static Vector3 focusPos;
		private static Quaternion focusRot;

		
		protected Rigidbody lipTrigger;
		private static float lipsTouchCount = 0.0f;
		protected Rigidbody vagTrigger;
		private static float vagTouchCount = 0.0f;
		private static bool lipsOnly = false;
		
		private static float idleArousal = 0.0f;
		private static float idleValence = 0.0f;
		
        private static DAZMorph morphLBicepFlex;
        private static DAZMorph morphRBicepFlex;

        //Hand Morphs
        private static bool doHands = true;
        private static DAZMorph morphLHandFist;
        private static float mLHandFistValue = 0.0f;
        private static float mLHandFistTarget = 0.0f;
        private static DAZMorph morphRHandFist;
        private static float mRHandFistValue = 0.0f;
        private static float mRHandFistTarget = 0.0f;
        private static DAZMorph morphLHandStraighten;
        private static float mLHandStraightenValue = 0.0f;
        private static float mLHandStraightenTarget = 0.0f;
        private static DAZMorph morphRHandStraighten;
        private static float mRHandStraightenValue = 0.0f;
        private static float mRHandStraightenTarget = 0.0f;


        //Eye Brow Morphs
        private static bool morphBrowAction = false;
        private static DAZMorph morphExpExcitement;
        private static float mExcitementValue = 0.0f;
        private static float mExcitementTarget = 0.0f;
        private static DAZMorph morphBrowDown;
        private static float mBrowDownValue = 0.0f;
        private static float mBrowDownTarget = 0.0f;
        private static DAZMorph morphBrowUp;
        private static float mBrowUpValue = 0.0f;
        private static float mBrowUpTarget = 0.0f;
        private static DAZMorph morphBrowCenterUp;
        private static float mBrowCenterUpValue = 0.0f;
        private static float mBrowCenterUpTarget = 0.0f;
        private static DAZMorph morphBrowOuterUpLeft;
        private static float mBrowOuterUpLeftValue = 0.0f;
        private static float mBrowOuterUpLeftTarget = 0.0f;
        private static DAZMorph morphBrowOuterUpRight;
        private static float mBrowOuterUpRightValue = 0.0f;
        private static float mBrowOuterUpRightTarget = 0.0f;

        private static Vector3 saccadeOffset;
        //Eye Morphs
        private static bool morphEyeAction = false;
		private static string lookAwaySide = "left";
        private static DAZMorph morphEyesClosedLeft;
        private static float mEyesClosedLeftValue = 0.0f;
        private static float mEyesClosedLeftTarget = 0.0f;
        private static float mEyesClosedLeftBlinkTarget = 0.0f;
        private static DAZMorph morphEyesClosedRight;
        private static float mEyesClosedRightValue = 0.0f;
        private static float mEyesClosedRightTarget = 0.0f;
        private static float mEyesClosedRightBlinkTarget = 0.0f;
        private static DAZMorph morphEyesSquint;
        private static float mEyesSquintValue = 0.0f;
        private static float mEyesSquintTarget = 0.0f;
        private static DAZMorph morphEyesPupils;
        private static float mEyesPupilsValue = 0.0f;
        private static float mEyesPupilsOrig = 0.0f;
        private static float mEyesPupilsTarget = 0.0f;

        //Mouth Morphs
        private static bool morphMouthAction = false;
        private static bool morphBlinking = false;
		private static float blinkTimer = 0.0f;
        private static DAZMorph morphNoseFlare;
        private static float mNoseFlareValue = 0.0f;
        private static float mNoseFlareTarget = 0.0f;
        private static DAZMorph morphExpSmileFullFace;
        private static float mSmileFullFaceValue = 0.0f;
        private static float mSmileFullFaceTarget = 0.0f;
        private static DAZMorph morphExpSmileOpenFullFace;
        private static float mSmileOpenFullFaceValue = 0.0f;
        private static float mSmileOpenFullFaceTarget = 0.0f;
        private static DAZMorph morphExpGlare;
        private static float mGlareValue = 0.0f;
        private static float mGlareTarget = 0.0f;
        private static DAZMorph morphExpHappy;
        private static float mHappyValue = 0.0f;
        private static float mHappyTarget = 0.0f;
        private static DAZMorph morphExpFlirting;
        private static float mFlirtingValue = 0.0f;
        private static float mFlirtingTarget = 0.0f;
        private static DAZMorph morphExpDeserveIt;
        private static float mDeserveItValue = 0.0f;
        private static float mDeserveItTarget = 0.0f;
        private static DAZMorph morphExpTakingIt;
        private static float mTakingItValue = 0.0f;
        private static float mTakingItTarget = 0.0f;
        private static DAZMorph morphMouthMouthOpen;
        private static float mMouthOpenValue = 0.0f;
        private static float mMouthOpenTarget = 0.0f;
        private static DAZMorph morphMouthMouthOpenWide;
        private static float mMouthOpenWideValue = 0.0f;
        private static float mMouthOpenWideTarget = 0.0f;
        private static DAZMorph morphMouthOpenWider;
        private static float mMouthOpenWiderValue = 0.0f;
        private static float mMouthOpenWiderTarget = 0.0f;
        private static DAZMorph morphMouthNarrow;
        private static float mMouthNarrowValue = 0.0f;
        private static float mMouthNarrowTarget = 0.0f;
        private static DAZMorph morphMouthSideLeft;
        private static float mMouthSideLeftValue = 0.0f;
        private static float mMouthSideLeftTarget = 0.0f;
        private static DAZMorph morphMouthSideRight;
        private static float mMouthSideRightValue = 0.0f;
        private static float mMouthSideRightTarget = 0.0f;
        private static DAZMorph morphMouthSmileSimpleLeft;
        private static float mSmileSimpleLeftValue = 0.0f;
        private static float mSmileSimpleLeftTarget = 0.0f;
        private static DAZMorph morphMouthSmileSimpleRight;
        private static float mSmileSimpleRightValue = 0.0f;
        private static float mSmileSimpleRightTarget = 0.0f;
        private static DAZMorph morphMouthSmileMuscle;
        private static float mSmileMuscleValue = 0.0f;
        private static float mSmileMuscleTarget = 0.0f;
        private static DAZMorph morphMouthCornerUpDown;
        private static float mMouthCornerUpDownValue = 0.0f;
        private static float mMouthCornerUpDownTarget = 0.0f;
        private static DAZMorph morphLipsLipsPucker;
        private static float mLipsPuckerValue = 0.0f;
        private static float mLipsPuckerTarget = 0.0f;
        private static DAZMorph morphLipsLipsPuckerWide;
        private static float mLipsPuckerWideValue = 0.0f;
        private static float mLipsPuckerWideTarget = 0.0f;
        private static DAZMorph morphLipsLipBite;
        private static float mLipBiteValue = 0.0f;
        private static float mLipBiteTarget = 0.0f;
        private static DAZMorph morphLipBottomIn;
        private static float mLipBottomInValue = 0.0f;
        private static float mLipBottomInTarget = 0.0f;
        private static DAZMorph morphLipsLipsClose;
        private static float mLipsCloseValue = 0.0f;
        private static float mLipsCloseTarget = 0.0f;
        private static DAZMorph morphLipsLipsPart;
        private static float mLipsPartValue = 0.0f;
        private static float mLipsPartTarget = 0.0f;
        private static DAZMorph morphLipsLipsPartCenter;
        private static float mLipsCenterPartValue = 0.0f;
        private static float mLipsCenterPartTarget = 0.0f;
        private static DAZMorph morphLipsPouty;
        private static float mLipsPoutyValue = 0.0f;
        private static float mLipsPoutyTarget = 0.0f;
        private static DAZMorph morphLipsBottomDown;
        private static float mLipsBottomDownValue = 0.0f;
        private static float mLipsBottomDownTarget = 0.0f;
        private static DAZMorph morphVisF;
        private static float mVisFValue = 0.0f;
        private static float mVisFTarget = 0.0f;
        private static DAZMorph morphVisAA;
        private static float mVisAAValue = 0.0f;
        private static float mVisAATarget = 0.0f;
        private static DAZMorph morphVisOW;
        private static float mVisOWValue = 0.0f;
        private static float mVisOWTarget = 0.0f;
        private static DAZMorph morphVisM;
        private static float mVisMValue = 0.0f;
        private static float mVisMTarget = 0.0f;
		private static bool timboSmile = true;
		private static bool malSmile = true;
        private static DAZMorph morphNoseSmile;
        private static DAZMorph morphMouthStretchL;
        private static DAZMorph morphMouthStretchR;

		private static float smileTimer = 0.0f;
		private static float suppressTimer = 0.0f;
		private static bool suppressSmile = false;

        private static DAZMorph morphTongueInOut;
        private static float mTongueInOutValue = 1.0f;
        private static float mTongueInOutTarget = 1.0f;
        private static DAZMorph morphTongueSideSide;
        private static float mTongueSideSideValue = 0.0f;
        private static float mTongueSideSideTarget = 0.0f;
        private static DAZMorph morphTongueBendTip;
        private static float mTongueBendTipValue = 0.0f;
        private static float mTongueBendTipTarget = 0.0f;
        private static DAZMorph morphTongueLength;
        private static float mTongueTongueLengthValue = 0.0f;
        private static float mTongueTongueLengthTarget = 0.0f;
        private static DAZMorph morphTongueRaise;
        private static float mTongueTongueRaiseValue = 0.0f;
        private static float mTongueTongueRaiseTarget = 0.0f;
        private static DAZMorph morphTongueTwist;
        private static float mTongueTongueTwistValue = 0.0f;
        private static float mTongueTongueTwistTarget = 0.0f;

        private static DAZMorph morphBreastDroopLeft;
        private static DAZMorph morphBreastDroopRight;
        private static DAZMorph morphBreastHangLeft;
        private static DAZMorph morphBreastHangRight;

        private static DAZMorph morphRibCageSize;
        private static float mRibCageSizeOrig = 0.0f;
        private static float mRibCageSizeValue = 0.0f;
        private static float mRibCageSizeTarget = 0.0f;
        private static DAZMorph morphRibCageWidth;
        private static float mRibCageWidthOrig = 0.0f;
        private static float mRibCageWidthValue = 0.0f;
        private static float mRibCageWidthTarget = 0.0f;
        private static DAZMorph morphChestHeight;
        private static float mChestHeightOrig = 0.0f;
        private static float mChestHeightValue = 0.0f;
        private static float mChestHeightTarget = 0.0f;
        private static DAZMorph morphBreastHeight;
        private static float mBreastHeightOrig = 0.0f;
        private static float mBreastHeightValue = 0.0f;
        private static float mBreastHeightTarget = 0.0f;
        private static DAZMorph morphRibsDef;
        private static float mRibsDefOrig = 0.0f;
        private static float mRibsDefValue = 0.0f;
        private static float mRibsDefTarget = 0.0f;
        private static DAZMorph morphBreath;
        private static float mBreathOrig = 0.0f;
        private static float mBreathValue = 0.0f;
        private static float mBreathTarget = 0.0f;
        private static DAZMorph morphSternumDepth;
        private static float mSternumDepthOrig = 0.0f;
        private static float mSternumDepthValue = 0.0f;
        private static float mSternumDepthTarget = 0.0f;
        private static float breathClock = 0.0f;
        private static float breathHold = 0.0f;
        private static float breathCount = 0.0f;
        private static float breatheInSpeed = 1.1f;
        private static float breatheOutSpeed = 1.1f;
        private static float breathRate = Random.Range(0.2f, 0.3f);
        private static string breathState = "in";
        private static DAZMorph morphNipplesApply;
        private static float mNipplesApplyOrig = 0.0f;
        private static float mNipplesApplyValue = 0.0f;
        private static float mNipplesApplyTarget = 0.0f;
        private static DAZMorph morphDeepBulgeBellyBottom;
        private static float mDeepBulgeBellyBottomOrig = 0.0f;
        private static float mDeepBulgeBellyBottomValue = 0.0f;
        private static float mDeepBulgeBellyBottomTarget = 0.0f;
        private static DAZMorph morphDeepBulgeBellyMid;
        private static float mDeepBulgeBellyMidOrig = 0.0f;
        private static float mDeepBulgeBellyMidValue = 0.0f;
        private static float mDeepBulgeBellyMidTarget = 0.0f;
        private static DAZMorph morphDeepThroat;
        private static float mDeepThroatOrig = 0.0f;
        private static float mDeepThroatValue = 0.0f;
        private static float mDeepThroatTarget = 0.0f;
        private static DAZMorph morphBlowjobLips;
        private static float mBlowjobLipsOrig = 0.0f;
        private static float mBlowjobLipsValue = 0.0f;
        private static float mBlowjobLipsTarget = 0.0f;
        private static DAZMorph morphCheekSink;
        private static float mCheekSinkOrig = 0.0f;
        private static float mCheekSinkValue = 0.0f;
        private static float mCheekSinkTarget = 0.0f;
		
        private static DAZMorph morphShoulderFixLeftF;
        private static DAZMorph morphShoulderFixLeftR;
        private static float mShoulderFixLeftValue = 0.0f;
        private static float mShoulderFixLeftTarget = 0.0f;
        private static DAZMorph morphShoulderFixRightF;
        private static DAZMorph morphShoulderFixRightR;
        private static float mShoulderFixRightValue = 0.0f;
        private static float mShoulderFixRightTarget = 0.0f;
		
		private static bool enableIntense = false;
		private static bool enableInquisitive = false;
		private static bool enableCasual = false;
		private static bool enableBored = false;
		private static bool enableDayDream = false;
		private static bool enablePlayful = false;
		private static bool enableFeel = false;
		private static bool enableKissing = false;
		private static bool enableSucking = false;
		private static bool enableSex = false;
		private static bool enableRaised = false;
		private static bool enableLowered = false;
		private static bool enableConcentrate = false;
		private static bool enableOneRaise = false;
		private static bool enableApprehensive = false;
		private static bool enableBlink = false;
		private static bool enableEyeOpen = false;
		private static bool enableEyeClosed = false;
		private static bool enableFocus = false;
		private static bool enableSquint = false;
		private static bool enableWide = false;
		private static bool enableWink = false;
		private static bool enableMouthOpen = false;
		private static bool enableMouthClosed = false;
		private static bool enableBiteLip = false;
		private static bool enableSmile = false;
		private static bool enableBigSmile = false;
		private static bool enableSmirk = false;
		private static bool enableSideways = false;
		private static bool enableKiss = false;
		private static bool enableSuck = false;
		private static bool enableJoy = false;
		private static bool enableOh = false;
		


        private static bool usePerson2;
		private static Atom currentAtom;
		private static string currentAtomName;
        private static bool person2Usable;
        private static Atom person2;
		private static bool person2IsMale = false;

        private static Transform player;
        public static Transform playerVRLHand = SuperController.singleton.leftHand;
        public static Transform playerVRRHand = SuperController.singleton.rightHand;
        private static bool playerHandsUsable;
        private static FreeControllerV3 playerHeadController;
        private static FreeControllerV3 playerChestController;
        private static FreeControllerV3 playerLHandController;
        private static FreeControllerV3 playerRHandController;
        private static FreeControllerV3 playerPelvisController;
        private static FreeControllerV3 playerTipController;
        private static FreeControllerV3 playerTipBaseController;


        private static Vector3 playerFace;
        private static Vector3 playerFacePrev;
        private static Vector3 playerFaceRot;
        private static Vector3 playerFaceRotPrev;
        private static Vector3 playerChest;
        private static Vector3 playerLHand;
        private static Vector3 playerLHandPrev;
        private static Vector3 playerRHand;
        private static Vector3 playerRHandPrev;
        private static Vector3 playerPelvis;
        private static Vector3 playerTip;
        private static Vector3 playerTipPrev;
        private static Vector3 playerTipBase;
        private static Vector3 playerGround;
        private static Vector3 randomPointForward;
        private static Vector3 randomPointLeft;
        private static Vector3 randomPointRight;
        private static Vector3 randomPointUp;
        private static Vector3 randomPointForwardBase;
        private static Vector3 randomPointLeftBase;
        private static Vector3 randomPointRightBase;
        private static Vector3 randomPointUpBase;

        private static Transform personHeadTransform;
        private static float headToFaceRot;
        private static float playerHeadToFaceRot;
        private static float headToChestRot;
        private static float headToLHandRot;
        private static float headToRHandRot;
        private static float headToPelvisRot;
        private static float headToTipRot;

        private static Transform playerHeadTransform;
        private static float personChestToHead;
        private static float playerToHead;
        private static float playerToLBreast;
        private static float playerToRBreast;
        private static float playerToPelvis;
        private static float playerToLHand;
        private static float playerToRHand;
        private static float playerToPLHand;
        private static float playerToPRHand;
        private static float playerToLFoot;
        private static float playerToRFoot;
		private static bool playerLHandInteract;
		private static bool playerRHandInteract;
		private static bool playerHeadInteract;
		private static bool playerPenisInteract;
		private static bool playerLHandFirstInteract;
		private static bool playerRHandFirstInteract;
		private static bool playerHeadFirstInteract;
		private static bool playerPenisFirstInteract;
		private static float playerLHandFirstTimeout = 0.0f;
		private static float playerRHandFirstTimeout = 0.0f;
		private static float playerHeadFirstTimeout = 0.0f;
		private static float playerPenisFirstTimeout = 0.0f;
		private static float interactTimeout = 10.0f;

        private static float playerHeadToHead;
        private static float playerHeadToLHand;
        private static float playerHeadToRHand;
        private static float playerHeadToLBreast;
        private static float playerHeadToRBreast;
        private static float playerHeadToPelvis;

        private static Transform playerLHandTransform;
        private static float personChestToLHand;
        private static float playerLHandToHead;
        private static float playerLHandToLHand;
        private static float playerLHandToRHand;
        private static float playerLHandToLBreast;
        private static float playerLHandToRBreast;
        private static float playerLHandToPelvis;

        private static Transform playerRHandTransform;
        private static float personChestToRHand;
        private static float playerRHandToHead;
        private static float playerRHandToLHand;
        private static float playerRHandToRHand;
        private static float playerRHandToLBreast;
        private static float playerRHandToRBreast;
        private static float playerRHandToPelvis;

        private static float playerTipToHead;
        private static float playerTipToLHand;
        private static float playerTipToRHand;
        private static float playerTipToLBreast;
        private static float playerTipToRBreast;
        private static float playerTipToPelvis;

        private static float playerPelvisToHead;

        private static bool playerHeadMovement;
        private static bool playerLHandMovement;
        private static bool playerRHandMovement;
        private static bool playerTipMovement;
        private static bool emTargetMovement;
        private static bool pLHandTouch;
        private static bool pRHandTouch;

        private static float playerHeadTimeout;
        private static float playerLHandTimeout;
        private static float playerRHandTimeout;
        private static float playerTipTimeout;
        private static float emTargetTimeout;

        private static float minHeadMotion;
        private static float minHandMotion;
        private static float minTipMotion;

        private static float minFaceDistance;
        private static float closeFaceDistance;
        private static float personalSpaceDistance;
        private static float backgroundDistance;
        private static float interactionDistance;
        private static float kissingDistance = 0.285f;
        private static float kissingAngle = 0.0f;
        private static float kissingActual = 0.0f;
        private static float kissingAmount = 1.0f;
        
        private static bool smiling = false;
        private static bool funnyFace = false;


        private static float lookDirectAngle;
        private static float lookPeripheralAngle;
        private static float lookNoAwarenessAngle;

        private static float movementMaxTimeout;
        private static float movementModifier;
        private static float movementFalloff;

        private static float playerInterest;
        private static float interestFace;
        private static float interestLHand;
        private static float interestRHand;
        private static float interestPLHand;
        private static float interestPRHand;
        private static float interestPelvis;
        private static float interestTip;
        private static float interestFaceBase = 60.0f;
        private static float interestLHandBase = 45.0f;
        private static float interestRHandBase = 45.0f;
        private static float interestPelvisBase = 20.0f;
        private static float interestTipBase = 30.0f;
		private static float interestEMTargetBase = 20.0f;
        private static float interestPLHandBase = 65.0f;
        private static float interestPRHandBase = 65.0f;

        private static float randomInterest;
				
				private static float throatBulgeAmount = 0.0f;

        #endregion
        static void ObserveLipTrigger(object sender, TriggerEventArgs e)
        {
            //Do whatever you want here
			if (e.evtType == "Entered")
			{
				lipsTouchCount += 1.0f;
			}
			else
			{
				lipsTouchCount = Mathf.Clamp(lipsTouchCount - 1.0f,0.0f,100.0f);
			}
            //SuperController.LogMessage(e.evtType + "(" + lipsTouchCount + ") =" + e.collider.transform.parent.name);
        }
		
        static void ObserveVagTrigger(object sender, TriggerEventArgs e)
        {
            //Do whatever you want here
			if (e.evtType == "Entered")
			{
				vagTouchCount += 1.0f;
			}
			else
			{
				vagTouchCount = Mathf.Clamp(vagTouchCount - 1.0f,0.0f,100.0f);
			}
            //SuperController.LogMessage(e.evtType + "(" + lipsTouchCount + ") =" + e.collider.transform.parent.name);
        }
		
        public override void Init()
        {

            //SuperController.LogError("public void OnPreLoad()");
            //emotionSM = new StateMachine();
            lookSM = new StateMachine();
            systemSM = new StateMachine();
            browSM = new StateMachine();
            mouthSM = new StateMachine();
            eyesSM = new StateMachine();

			lipTrigger = containingAtom.rigidbodies.First(rb => rb.name == "LipTrigger");
			lipTrigger.gameObject.AddComponent<TriggerCollide>().OnCollide += ObserveLipTrigger;

			vagTrigger = containingAtom.rigidbodies.First(rb => rb.name == "VaginaTrigger");
			vagTrigger.gameObject.AddComponent<TriggerCollide>().OnCollide += ObserveVagTrigger;
			
            minHeadMotion = 0.005f;
            minHandMotion = 0.015f;
            minTipMotion = 0.21f;

            minFaceDistance = 0.065f;
            closeFaceDistance = 0.075f;
            personalSpaceDistance = 0.85f;
            backgroundDistance = 1.2f;
            interactionDistance = 0.13f;

            movementMaxTimeout = 4.0f;
            movementModifier = 0.085f;
            movementFalloff = 0.11f;
			
			RegisterUIElements();
			CreateMainMenuUI();

            lookDirectAngle = uiDirectGaze.val;// * (playerHeadToHead / personalSpaceDistance);
            lookPeripheralAngle = uiPeripheralGaze.val;
            lookNoAwarenessAngle = uiOutOfGaze.val;

            usePerson2 = uiUsePerson2.val;

            //pre init
            player = CameraTarget.centerTarget.transform;
            playerInterest = 0.0f;
            randomInterest = 0.0f;
            interestFace = 0.0f;
            interestLHand = 0.0f;
            interestRHand = 0.0f;
            interestPLHand = 0.0f;
            interestPRHand = 0.0f;
            interestPelvis = 0.0f;
            interestTip = 0.0f;
			interestEMTarget = 0.0f;

            playerHeadTimeout = 0.0f;
            playerLHandTimeout = 0.0f;
            playerRHandTimeout = 0.0f;
            playerTipTimeout = 0.0f;
			emTargetTimeout = 0.0f;
			//emTargetPosPrev = new Vector3(0.0f,0.0f,0.0f);

            playerLHandMovement = false;
            playerRHandMovement = false;
            playerTipMovement = false;
            emTargetMovement = false;

            //fallbacks incase hands/person2 missing
            headToLHandRot = 180.0f;
            headToRHandRot = 180.0f;
            headToPelvisRot = 180.0f;
            headToTipRot = 180.0f;
            playerLHandToHead = backgroundDistance;
            playerRHandToHead = backgroundDistance;
            playerLHandToLHand = backgroundDistance;
            playerRHandToLHand = backgroundDistance;
            playerRHandToRHand = backgroundDistance;
            playerRHandToRHand = backgroundDistance;
            playerLHandToLBreast = backgroundDistance;
            playerRHandToLBreast = backgroundDistance;
            playerLHandToRBreast = backgroundDistance;
            playerRHandToRBreast = backgroundDistance;
            playerLHandToPelvis = backgroundDistance;
            playerRHandToPelvis = backgroundDistance;
            playerPelvisToHead = backgroundDistance;
            playerTipToHead = backgroundDistance;
            playerTipToLHand = backgroundDistance;
            playerTipToRHand = backgroundDistance;
            playerTipToLBreast = backgroundDistance;
            playerTipToRBreast = backgroundDistance;
            playerTipToPelvis = backgroundDistance;
			prevPosHead = new Vector3(0.0f,0.0f,0.0f);
			prevPosChest = new Vector3(0.0f,0.0f,0.0f);
			prevPosHip = new Vector3(0.0f,0.0f,0.0f);
			prevPosLHand = new Vector3(0.0f,0.0f,0.0f);
			prevPosRHand = new Vector3(0.0f,0.0f,0.0f);
			prevPoslFoot = new Vector3(0.0f,0.0f,0.0f);
			prevPosrFoot = new Vector3(0.0f,0.0f,0.0f);

        }
		
		void Awake()
		{
			audiosource = gameObject.AddComponent<AudioSource>();
		}
			
        public void Start()
		{
			headAudio = containingAtom.GetStorableByID("HeadAudioSource");

            //SuperController.LogError("public void OnPostLoad()");
            //debugUI = Utils.GetAtom("debugText");
            //debugUIControl = debugUI.GetStorableByID("control") as UITextControl;
			//emTarget = SuperController.singleton.GetAtomByUid("EMTarget");
			
			
			/*
			Object[] allObj = GameObject.FindObjectsOfType(typeof(MonoBehaviour));
			string name = "";
			foreach(Object go in allObj)
			{
				name = go.name;
				if (name.Contains("Collider") && name.Contains("sh"))
				{
				//SuperController.LogError(go.name);
				}
			}
			
			
			GameObject collider = GameObject.Find("AutoColliderFemaleAutoColliderschest6");
			if (collider != null)
			{
				//SuperController.LogError("Collider Found");
				AutoCollider ac = collider.GetComponent<AutoCollider>();
				ac.autoRadiusBuffer = 0.02f;
				ac.autoLengthBuffer = 0.06f;
			}
			collider = GameObject.Find("AutoColliderFemaleAutoColliderschest5");
			if (collider != null)
			{
				//SuperController.LogError("Collider Found");
				AutoCollider ac = collider.GetComponent<AutoCollider>();
				ac.autoRadiusBuffer = 0.005f;
				ac.autoLengthBuffer = 0.047f;
			}
			collider = GameObject.Find("AutoColliderFemaleAutoColliderschest4");
			if (collider != null)
			{
				//SuperController.LogError("Collider Found");
				AutoCollider ac = collider.GetComponent<AutoCollider>();
				ac.autoRadiusBuffer = 0.02f;
				ac.autoLengthBuffer = 0.06f;
			}

			collider = GameObject.Find("AutoColliderFemaleAutoColliderschest3");
			if (collider != null)
			{
				//SuperController.LogError("Collider Found");
				AutoCollider ac = collider.GetComponent<AutoCollider>();
				ac.autoRadiusBuffer = 0.00f;
				ac.autoLengthBuffer = 0.02f;
			}
			
			collider = GameObject.Find("AutoColliderFemaleAutoCollidersarm1");
			if (collider != null)
			{
				//SuperController.LogError("Collider Found");
				AutoCollider ac = collider.GetComponent<AutoCollider>();
				ac.autoRadiusBuffer = 0.10f;
				ac.autoLengthBuffer = 0.02f;
			}*/

            person = containingAtom;//SuperController.singleton.GetAtomByUid("Person");
            if (person != null)
            {
				//SuperController.LogError("Person found");
                JSONStorable js = person.GetStorableByID("geometry");
                DAZCharacterSelector dcs = js as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;

                headController = person.GetStorableByID("headControl") as FreeControllerV3;
                headActual = person.GetStorableByID("head") as FreeControllerV3;
                eyeController = person.GetStorableByID("eyeTargetControl") as FreeControllerV3;
				eyeController.hidden = true;
                neckController = person.GetStorableByID("neckControl") as FreeControllerV3;
                chestController = person.GetStorableByID("chestControl") as FreeControllerV3;
				chestControllerYAngle = chestController.transform.eulerAngles.y;
                lBreastController = person.GetStorableByID("lNippleControl") as FreeControllerV3;
                rBreastController = person.GetStorableByID("rNippleControl") as FreeControllerV3;
                pelvisController = person.GetStorableByID("hipControl") as FreeControllerV3;
                pelvis2Controller = person.GetStorableByID("pelvisControl") as FreeControllerV3;
                abdomenController = person.GetStorableByID("abdomen2Control") as FreeControllerV3;
                lowerabsController = person.GetStorableByID("abdomenControl") as FreeControllerV3;
				//pelvisController.RBMass = 600.0f;
                lHandController = person.GetStorableByID("lHandControl") as FreeControllerV3;
                rHandController = person.GetStorableByID("rHandControl") as FreeControllerV3;
                lShoulderController = person.GetStorableByID("lShoulderControl") as FreeControllerV3;
                rShoulderController = person.GetStorableByID("rShoulderControl") as FreeControllerV3;
                lArmController = person.GetStorableByID("lArmControl") as FreeControllerV3;
                rArmController = person.GetStorableByID("rArmControl") as FreeControllerV3;
                lElbowController = person.GetStorableByID("lElbowControl") as FreeControllerV3;
                rElbowController = person.GetStorableByID("rElbowControl") as FreeControllerV3;
                lFootController = person.GetStorableByID("lFootControl") as FreeControllerV3;
                rFootController = person.GetStorableByID("rFootControl") as FreeControllerV3;
                lThighController = person.GetStorableByID("lThighControl") as FreeControllerV3;
                rThighController = person.GetStorableByID("rThighControl") as FreeControllerV3;
                lKneeController = person.GetStorableByID("lKneeControl") as FreeControllerV3;
                rKneeController = person.GetStorableByID("rKneeControl") as FreeControllerV3;

                refAngle = new Vector3(0.0f,-10.0f,-10.0f);
                if (morphUI != null)
                {
					//SuperController.LogError("Morph Controller Found");
                    morphLHandFist = morphUI.GetMorphByDisplayName("Left Hand Fist");
                    morphRHandFist = morphUI.GetMorphByDisplayName("Right Hand Fist");
                    morphLHandStraighten = morphUI.GetMorphByDisplayName("Left Hand Straighten");
                    morphRHandStraighten = morphUI.GetMorphByDisplayName("Right Hand Straighten");

                    morphBrowDown = morphUI.GetMorphByDisplayName("Brow Down");
                    morphBrowUp = morphUI.GetMorphByDisplayName("Brow Up");
                    //morphBrowCenterUp = morphUI.GetMorphByDisplayName("AAsex_sqntwrry2sm1");
					//if (morphBrowCenterUp == null)
					//{
						morphBrowCenterUp = morphUI.GetMorphByDisplayName("Brow Inner Up");
					//}
                    morphBrowOuterUpLeft = morphUI.GetMorphByDisplayName("Brow Outer Up Left");
                    morphBrowOuterUpRight = morphUI.GetMorphByDisplayName("Brow Outer Up Right");

                    morphEyesClosedLeft = morphUI.GetMorphByDisplayName("Eyes Closed Left");
                    morphEyesClosedRight = morphUI.GetMorphByDisplayName("Eyes Closed Right");
                    morphEyesSquint = morphUI.GetMorphByDisplayName("Eyes Squint");
                    morphEyesPupils = morphUI.GetMorphByDisplayName("Pupils Dialate");
					if (morphEyesPupils != null)
					{
						mEyesPupilsOrig = morphEyesPupils.morphValue;
						mEyesPupilsTarget = mEyesPupilsOrig;
						mEyesPupilsValue = mEyesPupilsOrig;
					}

                    morphNoseFlare = morphUI.GetMorphByDisplayName("Nose ala relax");
                    morphExpSmileFullFace = morphUI.GetMorphByDisplayName("CMC-Partial-Smile-1");
					if (morphExpSmileFullFace == null)
					{
						morphExpSmileFullFace = morphUI.GetMorphByDisplayName("Smile Full Face");
						timboSmile = false;
					}
                    morphExpSmileOpenFullFace = morphUI.GetMorphByDisplayName("asco - Parted Smile");
					if (morphExpSmileOpenFullFace == null)
					{
						morphExpSmileOpenFullFace = morphUI.GetMorphByDisplayName("Mouth Smile Simple");
					}

                    morphExpGlare = morphUI.GetMorphByDisplayName("Glare");
                    morphExpExcitement = morphUI.GetMorphByDisplayName("AA Mouth Cute 4");
					if (morphExpExcitement == null)
					{
						morphExpExcitement = morphUI.GetMorphByDisplayName("Mouth Smile Open");
						malSmile = false;
					}
					
					morphMouthCornerUpDown = morphUI.GetMorphByDisplayName("Mouth Corner Up-Down");
                    morphExpHappy = morphUI.GetMorphByDisplayName("Stifled2");
                    morphExpFlirting = morphUI.GetMorphByDisplayName("AA Mouth Kiss 4");
					if (morphExpFlirting == null)
					{
						morphExpFlirting = morphUI.GetMorphByDisplayName("Flirting");
					}
                    morphExpDeserveIt = morphUI.GetMorphByDisplayName("AAsex_sqntwrry1sm6b");
					if (morphExpDeserveIt == null)
					{
						morphExpDeserveIt = morphUI.GetMorphByDisplayName("Deserving It");
					}
					morphNoseSmile = morphUI.GetMorphByDisplayName("CMC-Nose-Sneer-1");
					if (morphNoseSmile == null)
					{
						morphNoseSmile = morphUI.GetMorphByDisplayName("Nose Wrinkle");
					}
					morphMouthStretchL = morphUI.GetMorphByDisplayName("CMC-Mouth-Stretch-L-1");
					morphMouthStretchR = morphUI.GetMorphByDisplayName("CMC-Mouth-Stretch-R-1");
					
                    morphExpTakingIt = morphUI.GetMorphByDisplayName("asco - Lip Bite Wide");
					if (morphExpTakingIt == null)
					{
						morphExpTakingIt = morphUI.GetMorphByDisplayName("Taking It");
					}
                    morphMouthMouthOpen = morphUI.GetMorphByDisplayName("Mouth Open Wide 2");
                    morphMouthMouthOpenWide = morphUI.GetMorphByDisplayName("Mouth Open Wide");
					if (morphMouthMouthOpenWide == null)
					{
						morphMouthMouthOpenWide = morphUI.GetMorphByDisplayName("Mouth Open Wide");
					}
                    morphMouthOpenWider = morphUI.GetMorphByDisplayName("Scream");
                    morphMouthNarrow = morphUI.GetMorphByDisplayName("Mouth Narrow");
                    morphMouthSideLeft = morphUI.GetMorphByDisplayName("Mouth Side-Side Left");
                    morphMouthSideRight = morphUI.GetMorphByDisplayName("Mouth Side-Side Right");
                    morphMouthSmileSimpleLeft = morphUI.GetMorphByDisplayName("Mouth Smile Simple Left");
                    morphMouthSmileSimpleRight = morphUI.GetMorphByDisplayName("Mouth Smile Simple Right");
                    morphLipsLipsPucker = morphUI.GetMorphByDisplayName("Lips Pucker");
					if (morphLipsLipsPucker == null)
					{
						morphLipsLipsPucker = morphUI.GetMorphByDisplayName("W");
					}
                    morphLipsLipsPuckerWide = morphUI.GetMorphByDisplayName("Lips Pucker Wide");
					morphLipsLipBite = morphUI.GetMorphByDisplayName("AA Mouth Lip Bite 1");//morphUI.GetMorphByDisplayName("AAsex_wideFF8");
					if (morphLipsLipBite == null)
					{
						morphLipsLipBite = morphUI.GetMorphByDisplayName("Lip Bite");
					}
                    morphLipsLipsClose = morphUI.GetMorphByDisplayName("Lips Close");
                    morphLipsLipsPart = morphUI.GetMorphByDisplayName("Lips Part");
                    morphLipsLipsPartCenter = morphUI.GetMorphByDisplayName("Lips Part Center");
                    morphLipsBottomDown = morphUI.GetMorphByDisplayName("Lip Bottom Down");
                    morphLipBottomIn = morphUI.GetMorphByDisplayName("Lip Bottom In");
                    morphLipsPouty = morphUI.GetMorphByDisplayName("MouthPouty");
                    morphMouthSmileMuscle = morphUI.GetMorphByDisplayName("Laugh Lines");
                    morphVisF = morphUI.GetMorphByDisplayName("F");
                    morphVisM = morphUI.GetMorphByDisplayName("M");
                    morphVisOW = morphUI.GetMorphByDisplayName("OW");
                    morphVisAA = morphUI.GetMorphByDisplayName("EH");

                    morphTongueInOut = morphUI.GetMorphByDisplayName("Tongue In-Out");
                    morphTongueSideSide = morphUI.GetMorphByDisplayName("Tongue Side-Side");
                    morphTongueBendTip = morphUI.GetMorphByDisplayName("Tongue Curl");
                    morphTongueLength = morphUI.GetMorphByDisplayName("Tongue Length");
                    morphTongueRaise = morphUI.GetMorphByDisplayName("Tongue Raise-Lower");
                    morphTongueTwist = morphUI.GetMorphByDisplayName("Tongue Twist");

                    morphRibCageSize = morphUI.GetMorphByDisplayName("RibcageWidth");
 					if (morphRibCageSize == null)
					{
						morphRibCageSize = morphUI.GetMorphByDisplayName("Ribcage Size");
					}
                    morphRibCageWidth = morphUI.GetMorphByDisplayName("CMC-Breathe-1");
					if (morphRibCageWidth == null)
					{
						morphRibCageWidth = morphUI.GetMorphByDisplayName("RibCageWidth");
					}
                    morphChestHeight = morphUI.GetMorphByDisplayName("Chest Height");
					if (morphChestHeight == null)
					{
						morphChestHeight = morphUI.GetMorphByDisplayName("Costal Angle Arched");
						mChestHeightOrig = -0.2f;//morphChestHeight.morphValue;
						//personIsMale = true;
					}
					else
					{
						mChestHeightOrig = 0.0f;//morphChestHeight.morphValue;
					}
                    morphBreastHeight = morphUI.GetMorphByDisplayName("Chest Up");
					if (morphBreastHeight == null)
					{
						morphBreastHeight = morphUI.GetMorphByDisplayName("Breathing Chest");
						//personIsMale = true;
					}
					morphBreastDroopLeft = morphUI.GetMorphByDisplayName("Breast droop left");
					morphBreastDroopRight = morphUI.GetMorphByDisplayName("Breast droop right");
					morphBreastHangLeft = morphUI.GetMorphByDisplayName("Breasts Hang Forward Left");
					morphBreastHangRight = morphUI.GetMorphByDisplayName("Breasts Hang Forward Right");
					morphBreath = morphUI.GetMorphByDisplayName("Breath1");
                    morphRibsDef = morphUI.GetMorphByDisplayName("Ribs Definition");
                    morphSternumDepth = morphUI.GetMorphByDisplayName("Breathing Chest");
                    morphNipplesApply = morphUI.GetMorphByDisplayName("Nipples Apply");
                    morphDeepBulgeBellyBottom = morphUI.GetMorphByDisplayName("deepbulge_bot");
                    morphDeepBulgeBellyMid = morphUI.GetMorphByDisplayName("deepbulge_mid");
                    morphDeepThroat = morphUI.GetMorphByDisplayName("deepthroat");
                    morphBlowjobLips = morphUI.GetMorphByDisplayName("Blowjob Lips");
                    morphCheekSink = morphUI.GetMorphByDisplayName("Cheeks Sink Lower");
					
                    morphShoulderFixLeftF = morphUI.GetMorphByDisplayName("ArmpitFixLeft");
                    morphShoulderFixLeftR = morphUI.GetMorphByDisplayName("ArmpitBackFixLeft");
                    morphShoulderFixRightF = morphUI.GetMorphByDisplayName("ArmpitFixRight");
                    morphShoulderFixRightR = morphUI.GetMorphByDisplayName("ArmpitBackFixRight");
					
					//morphLBicepFlex = morphUI.GetMorphByDisplayName("fm_bicepsL");
					//morphRBicepFlex = morphUI.GetMorphByDisplayName("fm_bicepsR");
					//SuperController.LogError("Morphs Loaded");
					
                }
				headLeftRight = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
				headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
				headLastLeftRight = headLeftRight;
				headLastUpDown = headUpDown;
            }
			if (person.GetComponentInChildren<DAZCharacterSelector>().gender != DAZCharacterSelector.Gender.Female)//morphTemp == null)
			{
				personIsMale = true;
			}
            playerHandsUsable = false;
            person2Usable = false;
		    //loadDefaults();

            if (playerVRLHand != null && playerVRRHand != null)
            {
				//SuperController.LogError("VR Hands Found");
                playerHandsUsable = true;
            }

            aCube = SuperController.singleton.GetAtomByUid("EMCube");
			if (aCube != null)
			{
			aCubeController = aCube.GetStorableByID("control") as FreeControllerV3;
			}
			
            aCube2 = SuperController.singleton.GetAtomByUid("EMCube2");
			if (aCube2 != null)
			{
			aCubeController2 = aCube2.GetStorableByID("control") as FreeControllerV3;
			}
            aCube3 = SuperController.singleton.GetAtomByUid("EMCube3");
			if (aCube3 != null)
			{
			aCubeController3 = aCube3.GetStorableByID("control") as FreeControllerV3;
			}
            aCube4 = SuperController.singleton.GetAtomByUid("EMCube4");
			if (aCube4 != null)
			{
			aCubeController4 = aCube4.GetStorableByID("control") as FreeControllerV3;
			}
            person2 = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
            if (person2 != null && usePerson2)
            {
				person2IsMale = false;
				//SuperController.LogError(person2.gameObject.name);
				if (person2.GetComponentInChildren<DAZCharacterSelector>().gender != DAZCharacterSelector.Gender.Female)//morphTemp == null)
				{
					person2IsMale = true;
				}
				//SuperController.LogError("Person2 Found");
                person2Usable = true;
                playerHeadController = person2.GetStorableByID("headControl") as FreeControllerV3;
                playerChestController = person2.GetStorableByID("chestControl") as FreeControllerV3;
                playerLHandController = person2.GetStorableByID("lHandControl") as FreeControllerV3;
                playerRHandController = person2.GetStorableByID("rHandControl") as FreeControllerV3;
                playerPelvisController = person2.GetStorableByID("pelvisControl") as FreeControllerV3;
                playerTipController = person2.GetStorableByID("penisTipControl") as FreeControllerV3;
                playerTipBaseController = person2.GetStorableByID("penisBaseControl") as FreeControllerV3;
            }
			else
			{
				//SuperController.LogError("No Person2 or not used");
				person2Usable = false;
				usePerson2 = false;
			}

            if (person == null || headController == null)
            {
                //SuperController.LogError("[EmotionEngine] Person not found");
                return;
            }
            if (player == null)
            {
                //SuperController.LogError("[EmotionEngine] Player not found");
                return;
            }

            if (usePerson2 && person2Usable)
            {
				//SuperController.LogError("Using Person2");
                playerFace = playerHeadController.followWhenOff.position;
                playerLHand = playerLHandController.followWhenOff.position;
                playerRHand = playerRHandController.followWhenOff.position;
                playerPelvis = playerPelvisController.followWhenOff.position;
                playerTip = playerTipController.followWhenOff.position;
            }
            else
            {
				//SuperController.LogError("Using Camera");
                playerFace = player.position;
                if (person2Usable && usePerson2)
                {
                    playerLHand = playerLHandController.followWhenOff.position;
                    playerRHand = playerRHandController.followWhenOff.position;
                    playerLHandTransform = playerLHandController.followWhenOff;
                    playerRHandTransform = playerRHandController.followWhenOff;
                }
                if (playerHandsUsable && usePerson2 == false)
                {
                    playerLHand = playerVRLHand.position;
                    playerRHand = playerVRRHand.position;
					playerLHandTransform = playerVRLHand;
					playerRHandTransform = playerVRRHand;
                }
                if (person2Usable == false && playerHandsUsable == false)
                {
                    playerLHand = playerFace;
                    playerRHand = playerFace;
                    playerLHandTransform = playerHeadTransform;
                    playerRHandTransform = playerHeadTransform;
                }
				playerPelvis = playerFace;
				playerTip = playerFace;
                if (person2Usable)
                {
                    playerPelvis = playerPelvisController.followWhenOff.position;
                    playerTip = playerTipController.followWhenOff.position;
                }
				else
				{
					uiUsePerson2.val = false;
				}
            }

            if (usePerson2 && person2 != null)
            {
                playerHeadTransform = playerHeadController.followWhenOff;
                closeFaceDistance = closeFaceDistance * 1.85f;
                person2Usable = true;
            }
            else
            {
                playerHeadTransform = player;
            }
			personEyes = containingAtom.GetStorableByID("Eyes");
			personEyelids = containingAtom.GetStorableByID("EyelidControl");
            lookAtPosition = eyeController.transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
			audiosource.Play();
			//volume if needed, 0-1, 1 might be loud
			audiosource.volume = 1.0f;
			//might find useful if its a oneshot or looping
			audiosource.loop = false;
			//headAudio.minDistance = 0.03f;
			//SuperController.LogError("Setup Complete");

		//savePreset();
			if (uiSetupComplete.val == false)
			{
				loadDefaults();
				//testString = "not Setup";
			}
            systemSM.Switch(sUpdate);
			//SuperController.LogError("Startup Complete");			
			
			loadStateConfig();
			//CreatePersonalityUI();
			//CreateTargetUI();
        }

                     
        public void FixedUpdate()
        {
		bool mainToggleFrozen =
                SuperController.singleton.freezeAnimationToggle != null &&
                SuperController.singleton.freezeAnimationToggle.isOn;
		if (uiSetupComplete.val == false || mainToggleFrozen)
		{
		//	uiFocusTarget.val = backendCurrentFocusTarget.val;
		}
		else
		{
            lookSM.OnUpdate();
            browSM.OnUpdate();
            mouthSM.OnUpdate();
            eyesSM.OnUpdate();
            systemSM.OnUpdate();
			//SuperController.LogError("State Machines Updated");

			//SuperController.LogError("check for state idle");
			if (morphMouthAction == false)
			{
				currentMouth = "Idle";
				smiledlast = false;
				//mSmileFullFaceTarget = 0.0f;
				//SuperController.LogMessage("Full Face set to 0 mouth action false", false);
				//mSmileOpenFullFaceTarget = 0.0f;
				//mSmileSimpleLeftTarget = 0.0f;
				//mSmileSimpleRightTarget = 0.0f;
				mHappyTarget = 0.0f;
			}
			if (morphEyeAction == false)
			{
				currentEye = "Idle";
			}
			if (morphBrowAction == false)
			{
				currentBrow = "Idle";
			}
			if (morphMouthAction == false && morphEyeAction == false && morphBrowAction == false && lookAction)
			{
				lookAction = false;
			}
			if (lookAction == false && morphMouthAction == false)
			{
				currentLook = "Idle";
			}

			if (uiFocusTarget.val != backendCurrentFocusTarget.val && backendCurrentFocusTarget.val != "None" && loadedFocusTarget == false)
			{
				//SuperController.LogError("Backend Focus Update");
				loadedFocusTarget = true;
				uiUsePerson2.val = true;
				uiFocusTarget.val = backendCurrentFocusTarget.val;
				if (uiFocusTarget.val != "None" && uiUsePerson2.val)
				{
					person2 = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
					if (person2 != null)
					{
						//SuperController.LogError("Setting Focus Target from backend");
						systemSM.Switch(sReselectPerson2);
					}
					else
					{
						uiFocusTarget.val = "None";
						backendCurrentFocusTarget.SetVal(uiFocusTarget.val);
						uiUsePerson2.val = false;
						person2Usable = false;
						usePerson2 = false;
					}
				}
			}
			else
			{
				loadedFocusTarget = true;
				backendCurrentFocusTarget.SetVal(uiFocusTarget.val);
			}
//			if (person2Usable && usePerson2)
	//		{
		//		uiUsePerson2.val = true;
			//}
			//SuperController.LogError(backendCurrentFocusTarget.val);
			
			if (aCubeController != null)
			{
				//aCubeController.transform.position = personHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
				aCubeController.transform.position = lHandController.followWhenOff.TransformPoint(new Vector3(-0.08f, 0.00f, 0.00f));
			}
			if (aCubeController2 != null)
			{
				aCubeController2.transform.position = rHandController.followWhenOff.TransformPoint(new Vector3(0.08f, 0.00f, 0.00f));
			}
			if (aCubeController3 != null)
			{
				aCubeController3.transform.position = randomPointUp;
			}
			if (aCubeController4 != null)
			{
				aCubeController4.transform.position = randomPointForward;
			}
			//SuperController.singleton.ClearErrors();
			playerHeadToHead = Vector3.Distance(headController.followWhenOff.position, playerHeadTransform.position);
			string superdebug = "";
			if (amGlancing)
			{
				superdebug = "Glancing ";
			}
			else
			{
				superdebug = "Not Glancing ";
			}
			if (gAvoid == 1.0f)
			{
				superdebug += "Avoiding ";
			}
			else
			{
				superdebug += "Not Avoiding ";
			}
			if (interestKissing)
			{
				superdebug += "Kissing ";
			}
			else
			{
				superdebug += "Not Kissing ";
			}
			if (usePerson2)
			{
				superdebug += "Using Person2 ";
			}
			else
			{
				superdebug += "Not Using Person2 ";
			}
			if (person2 == null && uiFocusTarget.val != "None")
			{
				superdebug += "Person2 error ";
			}
			else
			{
				superdebug += "Person2 found ";
			}
			//SuperController.LogError(superdebug);

			
			if (uiDoSounds.val)
			{
				if (soundsLoaded == false)
				{
					audioClip = URLAudioClipManager.singleton.GetClip("Breath_Nose_Out_Long1.wav");
					if (audioClip == null)
					{
						LoadSounds();
					}
					else
					{
						soundsLoaded = true;
					}
				}
				headAudio.SetBoolParamValue("spatialize", true);
				headAudio.SetFloatParamValue("volume", 5.0f);
			}
			//audiosource.Play();
			//player = CameraTarget.centerTarget.transform;
			//SuperController.LogError("Set Variables to values");
			float dynamicDirectAngle = Mathf.Abs(Vector3.Angle(playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)) - personHeadTransform.TransformPoint(new Vector3(0.0f, 0.1f, 0.07f)), playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)) - personHeadTransform.TransformPoint(new Vector3(0.0f, -0.1f, 0.07f))));
			dynamicDirectAngle = Mathf.Clamp(dynamicDirectAngle, 10.0f, lookPeripheralAngle);
			gHeadSpeed = 1.75f;
			usePerson2 = uiUsePerson2.val;
			//oldEyePos = eyeController.transform.position;
			personalSpaceDistance = uiPersonalSpace.val;//Mathf.Min(uiPersonalSpace.val,Mathf.Max(playerHeadToHead + 0.5f, closeFaceDistance*2.0f));
			backgroundDistance = personalSpaceDistance * 1.5f;
			if (uiDynamicDirectAngle.val == true)
			{
				lookDirectAngle = dynamicDirectAngle;
				uiDirectGaze.val = dynamicDirectAngle;
			}
			else
			{
				lookDirectAngle = Mathf.Lerp(uiDirectGaze.val / 2.0f, uiDirectGaze.val * 2.0f,Mathf.Clamp(playerHeadToHead-kissingDistance,0.0f,personalSpaceDistance) / personalSpaceDistance);// * (playerHeadToHead / personalSpaceDistance);
			}
            
            lookPeripheralAngle = uiPeripheralGaze.val;
            lookNoAwarenessAngle = uiOutOfGaze.val;
			eyesNonDirectAngle = lookDirectAngle;
			closeFaceDistance = uiCloseToFaceDist.val;
			interactionDistance = uiInteractDist.val;
			interestMaxSmile = uiMaxMorphSmile.val;
			doHands = uiDoHands.val;
			kissingDistance = uiKissingDist.val;
			eyeUpdateTime = uiEyeUpdate.val;
			currentAtomName = uiFocusTarget.val;
			kissingAmount = uiKissAmount.val;
			playerLHandInteract = false;
			playerRHandInteract = false;
			playerHeadInteract = false;
			playerPenisInteract = false;
			blinkTimer += Time.fixedDeltaTime;
			blinkRepTimer += Time.fixedDeltaTime;
			blinkSpeed = uiBlinkSpeed.val;
			rollTimer += Time.fixedDeltaTime;
			mChestHeightOrig = uiChestHeightOffset.val;
			rollChance = uiRollChance.val;
			morphSpeed = uiAnimationSpeed.val;
			randomBaseDistance = uiRandomBaseDistance.val;
			randomBaseHeight = uiRandomBaseHeight.val;
			randomBaseOffset = uiRandomBaseOffset.val;
			interestFaceBase = 60.0f * uiHeadInterest.val;
			interestLHandBase = 45.0f * uiLHandInterest.val;
			interestRHandBase = 45.0f * uiRHandInterest.val;
			interestPelvisBase = 20.0f * uiPenisInterest.val;
			interestTipBase = 30.0f * uiPenisInterest.val;
			interestEMTargetBase = 20.0f * uiObjectInterest.val;
			maxHeadRoll = uiMaxHeadRoll.val;
			minInterest = uiMinInterest.val;
			gazeVariation = uiGazeVariation.val;
			adjustWaitTime = Mathf.Lerp(10.0f, 5.0f, interestArousal/10.0f) * uiGazeDirectLookDelay.val;
			variationChance = uiVariationChance.val;
			moanChance = uiMoanChance.val;
			pLHandTouch = false;
			pRHandTouch = false;
			if (adjustTimeout > 0.0f)
			{
				adjustTimeout = adjustTimeout - Time.fixedDeltaTime;
			}
			else
			{
				adjustTimeout = 0.0f;
			}
			
			if (suppressTimer <= 0.0f)
			{
				smileTimer = smileTimer + Time.fixedDeltaTime;
				suppressSmile = false;
			}
			else
			{
				suppressTimer = suppressTimer - Time.fixedDeltaTime;
			}
			
			if (playerLHandFirstTimeout > 0.0f)
			{
				playerLHandFirstTimeout -= Time.fixedDeltaTime;
			}
			else
			{
				if (playerLHandFirstInteract == false)
				{
					playerLHandFirstInteract = true;
					playerLHandFirstTimeout = 0.0f;
				}
			}
			if (playerRHandFirstTimeout > 0.0f)
			{
				playerRHandFirstTimeout -= Time.fixedDeltaTime;
			}
			else
			{
				if (playerRHandFirstInteract == false)
				{
					playerRHandFirstInteract = true;
					playerRHandFirstTimeout = 0.0f;
				}
			}
			if (playerPenisFirstTimeout > 0.0f)
			{
				playerPenisFirstTimeout -= Time.fixedDeltaTime;
			}
			else
			{
				if (playerPenisFirstInteract == false)
				{
					playerPenisFirstInteract = true;
					playerPenisFirstTimeout = 0.0f;
				}
			}

			
			
			
			
			bool testRun = false;
			if (testRun)
			{
				if (lookAction == false)
				{
					//playerLHandInteract = true;
					lookSM.Switch(lSex);
					//mouthSM.Switch(mBiteLip);
					
				}
			}			

            uiArousalStatus.SetVal(uiString);
						
				if (uiOnlyBuiltIn.val != onlyUseBuiltIn)
				{
					JSONStorable js = person.GetStorableByID("geometry");
					DAZCharacterSelector dcs = js as DAZCharacterSelector;
					GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;
					morphExpSmileFullFace.morphValue = 0.0f;
					morphExpExcitement.morphValue = 0.0f;
					morphExpFlirting.morphValue = 0.0f;
					morphExpDeserveIt.morphValue = 0.0f;
					morphExpTakingIt.morphValue = 0.0f;
					if (uiOnlyBuiltIn.val == false)
					{
						if (morphExpSmileFullFace != null) {morphExpSmileFullFace.morphValue = 0.0f;}
						if (morphExpSmileOpenFullFace != null) {morphExpSmileOpenFullFace.morphValue = 0.0f;}
						if (morphExpExcitement != null) {morphExpExcitement.morphValue = 0.0f;}
						if (morphExpFlirting != null) {morphExpFlirting.morphValue = 0.0f;}
						if (morphExpDeserveIt != null) {morphExpDeserveIt.morphValue = 0.0f;}
						if (morphExpTakingIt != null) {morphExpTakingIt.morphValue = 0.0f;}
						if (morphNoseSmile != null) {morphNoseSmile.morphValue = 0.0f;}
						if (morphLipsLipBite != null) {morphLipsLipBite.morphValue = 0.0f;}
						if (morphRibCageWidth != null) {morphRibCageWidth.morphValue = 0.0f;}

						morphExpSmileFullFace = morphUI.GetMorphByDisplayName("CMC-Partial-Smile-1");
						if (morphExpSmileFullFace == null)
						{
							morphExpSmileFullFace = morphUI.GetMorphByDisplayName("Smile Full Face");
							timboSmile = false;
						}
						morphExpSmileOpenFullFace = morphUI.GetMorphByDisplayName("asco - Parted Smile");
						if (morphExpSmileOpenFullFace == null)
						{
							morphExpSmileOpenFullFace = morphUI.GetMorphByDisplayName("Mouth Smile Simple");
						}
						morphExpExcitement = morphUI.GetMorphByDisplayName("AA Mouth Cute 4");
						if (morphExpExcitement == null)
						{
							morphExpExcitement = morphUI.GetMorphByDisplayName("Mouth Smile Open");
							malSmile = false;
						}
						morphExpFlirting = morphUI.GetMorphByDisplayName("AA Mouth Kiss 4");
						if (morphExpFlirting == null)
						{
							morphExpFlirting = morphUI.GetMorphByDisplayName("Flirting");
						}
						morphExpDeserveIt = morphUI.GetMorphByDisplayName("AAsex_sqntwrry1sm6b");
						if (morphExpDeserveIt == null)
						{
							morphExpDeserveIt = morphUI.GetMorphByDisplayName("Deserving It");
						}
						morphExpTakingIt = morphUI.GetMorphByDisplayName("asco - Lip Bite Wide");
						if (morphExpTakingIt == null)
						{
							morphExpTakingIt = morphUI.GetMorphByDisplayName("Taking It");
						}
						morphNoseSmile = morphUI.GetMorphByDisplayName("CMC-Nose-Sneer-1");
						if (morphNoseSmile == null)
						{
							morphNoseSmile = morphUI.GetMorphByDisplayName("Nose Wrinkle");
						}
						morphLipsLipBite = morphUI.GetMorphByDisplayName("AA Mouth Lip Bite 1");//morphUI.GetMorphByDisplayName("AAsex_wideFF8");
						if (morphLipsLipBite == null)
						{
							morphLipsLipBite = morphUI.GetMorphByDisplayName("Lip Bite");
						}
						morphRibCageWidth = morphUI.GetMorphByDisplayName("CMC-Breathe-1");
						if (morphRibCageWidth == null)
						{
							morphRibCageWidth = morphUI.GetMorphByDisplayName("RibCageWidth");
						}
						morphChestHeight = morphUI.GetMorphByDisplayName("Chest Height");
						if (morphChestHeight == null)
						{
							morphChestHeight = morphUI.GetMorphByDisplayName("Costal Angle Arched");
							mChestHeightOrig = -0.2f;//morphChestHeight.morphValue;
							//personIsMale = true;
						}
						else
						{
							mChestHeightOrig = 0.0f;//morphChestHeight.morphValue;
						}
						onlyUseBuiltIn = uiOnlyBuiltIn.val;
						//SuperController.LogMessage("Custom Morphs Used", false);
					}
					else
					{
						if (morphExpSmileFullFace != null) {morphExpSmileFullFace.morphValue = 0.0f;}
						if (morphExpSmileOpenFullFace != null) {morphExpSmileOpenFullFace.morphValue = 0.0f;}
						if (morphExpExcitement != null) {morphExpExcitement.morphValue = 0.0f;}
						if (morphExpFlirting != null) {morphExpFlirting.morphValue = 0.0f;}
						if (morphExpDeserveIt != null) {morphExpDeserveIt.morphValue = 0.0f;}
						if (morphExpTakingIt != null) {morphExpTakingIt.morphValue = 0.0f;}
						if (morphNoseSmile != null) {morphNoseSmile.morphValue = 0.0f;}
						if (morphLipsLipBite != null) {morphLipsLipBite.morphValue = 0.0f;}
						if (morphRibCageWidth != null) {morphRibCageWidth.morphValue = 0.0f;}
						
						morphExpSmileFullFace = morphUI.GetMorphByDisplayName("Smile Full Face");
						morphExpSmileOpenFullFace = morphUI.GetMorphByDisplayName("Mouth Smile Simple");
						timboSmile = false;
						morphExpExcitement = morphUI.GetMorphByDisplayName("Mouth Smile Open");
						malSmile = false;
						morphExpFlirting = morphUI.GetMorphByDisplayName("Flirting");
						morphExpDeserveIt = morphUI.GetMorphByDisplayName("Deserving It");
						morphExpTakingIt = morphUI.GetMorphByDisplayName("Taking It");
						morphNoseSmile = morphUI.GetMorphByDisplayName("Nose Wrinkle");
						morphLipsLipBite = morphUI.GetMorphByDisplayName("Lip Bite");
						morphRibCageWidth = morphUI.GetMorphByDisplayName("RibCageWidth");
						onlyUseBuiltIn = uiOnlyBuiltIn.val;
						//SuperController.LogMessage("Built-In Morphs", false);
					}
				}
			
			//energyAmount = 0.0f;
			uiExpressionLengthVal = uiExpressionLength.val;
			
			if (idleLArmTimeout > 0.0f && Mathf.Abs(lElbowActual - lElbowTarget) < 1.0f)
			{
				idleLArmTimeout -= Time.fixedDeltaTime;
			}
			if (idleRArmTimeout > 0.0f && Mathf.Abs(rElbowActual - rElbowTarget) < 1.0f)
			{
				idleRArmTimeout -= Time.fixedDeltaTime;
			}
			if (idleLLegTimeout > 0.0f && Mathf.Abs(lKneeActual - lKneeTarget) < 1.0f)
			{
				idleLLegTimeout -= Time.fixedDeltaTime;
			}
			if (idleRLegTimeout > 0.0f && Mathf.Abs(rKneeActual - rKneeTarget) < 1.0f)
			{
				idleRLegTimeout -= Time.fixedDeltaTime;
			}
			if (idleBodyTimeout > 0.0f)
			{
				idleBodyTimeout -= Time.fixedDeltaTime;
			}
			
			

			Vector3 headForwardOnPelvis = Vector3.ProjectOnPlane(headController.followWhenOff.forward, pelvisController.followWhenOff.up);
			headLeftRight = Vector3.SignedAngle(pelvisController.followWhenOff.forward, headForwardOnPelvis, pelvisController.followWhenOff.up);
			Vector3 headUpOnChest = Vector3.ProjectOnPlane(headController.followWhenOff.forward, -chestController.followWhenOff.right);
			headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headUpOnChest, -chestController.followWhenOff.right);
						
			//SuperController.LogError("Do Material Effect");
			if (uiEffectMaterial.val && materialCaptured == false)
			{
				MaterialOptions[] mo = containingAtom.gameObject.GetComponentsInChildren<MaterialOptions>();
				tempFloat = 0.0f;
				if(mo!=null)
				{
					//SuperController.LogMessage("Materials found = " + mo.Length.ToString());
					foreach (MaterialOptions m in mo)
					{
						//SuperController.LogMessage(m.name); //Show all the names
						if (m.name == "RenKayla" && tempFloat == 0.0f)
						{
							tempFloat = 1.0f;
							baseGloss = m.GetFloatParamValue("Gloss");
							baseSpec = m.GetFloatParamValue("Specular Intensity");
							baseSpecBump = m.GetFloatParamValue("Specular Bumpiness");
							materialCaptured = true;
							//SuperController.LogMessage("Gloss : " + baseGloss + " Spec : " + baseSpec);
						}
					}
				}
			}
			
			if (uiEffectMaterial.val == false && materialCaptured)
			{
				MaterialOptions[] mo = containingAtom.gameObject.GetComponentsInChildren<MaterialOptions>();
				tempFloat = 0.0f;
				if(mo!=null)
				{
					//SuperController.LogMessage("Materials found = " + mo.Length.ToString());
					foreach (MaterialOptions m in mo)
					{
						//SuperController.LogMessage(m.name); //Show all the names
						if (m.name == "RenJanie" && tempFloat == 0.0f)
						{
							tempFloat = 1.0f;
							m.SetFloatParamValue("Gloss", baseGloss);
							m.SetFloatParamValue("Specular Intensity", baseSpec);
							m.SetFloatParamValue("Specular Bumpiness", baseSpecBump);
							materialCaptured = false;
							//SuperController.LogMessage("Gloss : " + (baseGloss + (interestArousal/2.0f)) + " Spec : " + (baseSpec + (interestArousal/30.0f)));
						}
					}
				}
			}
			
			tempFloat = 0.0f;
			if (vagTouchCount > 0.0f)
			{
				if (playerTipToPelvis < interactionDistance)
				{
					tempFloat = 1.0f;
				}
				if (playerHeadToPelvis < interactionDistance*2.0f)
				{
					tempFloat = 1.0f;
				}
				if (playerLHandToPelvis < interactionDistance*2.0f)
				{
					tempFloat = 1.0f;
				}
				if (playerRHandToPelvis < interactionDistance*2.0f)
				{
					tempFloat = 1.0f;
				}
			}
			
			if (interestArousal > 7.0f || tempFloat == 1.0f)
			{
				heatupValue += Time.fixedDeltaTime / 15.0f;
				interestPeakArousalTimer += Time.fixedDeltaTime;
				if (interestPeakArousalTimer > 1.0f)
				{
					interestPeakArousal = Mathf.Min(interestPeakArousal + 0.01f,10.0f);
					interestPeakArousalTimer = 0.0f;
				}
			}
			else
			{
				heatupValue -= Time.fixedDeltaTime / 20.0f;
				interestPeakArousalTimer += Time.fixedDeltaTime;
				if (interestPeakArousalTimer > 2.0f)
				{
					interestPeakArousal = Mathf.Max(interestPeakArousal - 0.03f,1.0f);
					interestPeakArousalTimer = 0.0f;
				}
			}
			heatupValue = Mathf.Clamp(heatupValue, 0.0f, 10.0f);
			if (interestValence > 8.0f)
			{
				interestPeakValenceTimer += Time.fixedDeltaTime;
				if (interestPeakValenceTimer > 1.0f)
				{
					interestPeakValence = Mathf.Min(interestPeakValence + 0.01f,10.0f);
					interestPeakValenceTimer = 0.0f;
				}
			}
			else
			{
				interestPeakValenceTimer += Time.fixedDeltaTime;
				if (interestPeakValenceTimer > 2.0f)
				{
					interestPeakValence = Mathf.Max(interestPeakValence - 0.03f,1.0f);
					interestPeakValenceTimer = 0.0f;
				}
			}
			
			if (currentLook == "Sex" || currentLook == "Kissing")
			{
				interestPeakArousal = 0.0f;
				interestPeakValence = 0.0f;
			}

			if (uiEffectMaterial.val && materialCaptured)
			{
				MaterialOptions[] mo = containingAtom.gameObject.GetComponentsInChildren<MaterialOptions>();
				tempFloat = 0.0f;
				if(mo!=null)
				{
					//SuperController.LogMessage("Materials found = " + mo.Length.ToString());
					foreach (MaterialOptions m in mo)
					{
						//SuperController.LogMessage(m.name); //Show all the names
						if (m.name == "RenJanie" && tempFloat == 0.0f)
						{
							tempFloat = 1.0f;
							m.SetFloatParamValue("Gloss", baseGloss + (Mathf.Lerp(0.0f,3.0f,heatupValue) * uiMaterialMult.val));
							m.SetFloatParamValue("Specular Intensity", baseSpec + (Mathf.Lerp(0.0f,0.3f,heatupValue) * uiMaterialMult.val));
							m.SetFloatParamValue("Specular Bumpiness", baseSpecBump + (Mathf.Lerp(0.0f,0.1f,heatupValue)));
							//SuperController.LogMessage("Gloss : " + (baseGloss + (interestArousal/2.0f)) + " Spec : " + (baseSpec + (interestArousal/30.0f)));
						}
					}
				}
			}

			//SuperController.LogError("Material complete");

			
			if (blinkRepTimer > 1.75f)
			{
				blinkRepeat = Mathf.Clamp(blinkRepeat - 1.0f,1.0f,100.0f);
				blinkRepTimer = 0.0f;
			}

			tempFloat = uiIdleSpeed.val / 10.0f;
			tempFloat2 = uiIdleArmSpeed.val / 10.0f;
			twistSpeed = 0.005f * uiIdleSpeed.val;

			if (neckSpringActual < neckSpringTarget)
			{
				neckSpringActual = Mathf.Clamp(neckSpringActual + tempFloat, neckSpringActual, neckSpringTarget);
			}
			if (neckSpringActual > neckSpringTarget)
			{
				neckSpringActual = Mathf.Clamp(neckSpringActual - tempFloat, neckSpringTarget, neckSpringActual);
			}
			if (neckXActual < neckXTarget)
			{
				neckXActual = Mathf.Clamp(neckXActual + (tempFloat * 3.0f), neckXActual, neckXTarget);
			}
			if (neckXActual > neckXTarget)
			{
				neckXActual = Mathf.Clamp(neckXActual - (tempFloat * 3.0f), neckXTarget, neckXActual);
			}

			if (kissingActual < kissingAngle)
			{
				kissingActual = Mathf.Clamp(kissingActual + twistSpeed, kissingActual, kissingAngle);
			}
			if (kissingActual > kissingAngle)
			{
				kissingActual = Mathf.Clamp(kissingActual - twistSpeed, kissingAngle, kissingActual);
			}

			if (sexActionNeckActual < sexActionNeckX)
			{
				sexActionNeckActual = Mathf.Clamp(sexActionNeckActual + twistSpeed, sexActionNeckActual, sexActionNeckX);
			}
			if (sexActionNeckActual > sexActionNeckX)
			{
				sexActionNeckActual = Mathf.Clamp(sexActionNeckActual - twistSpeed, sexActionNeckX, sexActionNeckActual);
			}

			if (headLastLeftRightActual < headLastLeftRight)
			{
				headLastLeftRightActual = Mathf.Clamp(headLastLeftRightActual + twistSpeed, headLastLeftRightActual, headLastLeftRight);
			}
			if (headLastLeftRightActual > headLastLeftRight)
			{
				headLastLeftRightActual = Mathf.Clamp(headLastLeftRightActual - twistSpeed, headLastLeftRight, headLastLeftRightActual);
			}

			if (twistActual < twistTarget)
			{
				twistActual = Mathf.Clamp(twistActual + twistSpeed, twistActual, twistTarget);
			}
			if (twistActual > twistTarget)
			{
				twistActual = Mathf.Clamp(twistActual - twistSpeed, twistTarget, twistActual);
			}

			if (twist2Actual < twist2Target)
			{
				twist2Actual = Mathf.Clamp(twist2Actual + twistSpeed, twist2Actual, twist2Target);
			}
			if (twist2Actual > twist2Target)
			{
				twist2Actual = Mathf.Clamp(twist2Actual - twistSpeed, twist2Target, twist2Actual);
			}

			if (idleArousal < interestArousal)
			{
				idleArousal = Mathf.Clamp(idleArousal + (tempFloat / 225.0f), idleArousal, interestArousal);
			}
			if (idleArousal > interestArousal)
			{
				idleArousal = Mathf.Clamp(idleArousal - (tempFloat / 225.0f), interestArousal, idleArousal);
			}

			if (idleValence < interestValence)
			{
				idleValence = Mathf.Clamp(idleValence + (tempFloat / 225.0f), idleValence, interestValence);
			}
			if (idleValence > interestValence)
			{
				idleValence = Mathf.Clamp(idleValence - (tempFloat / 225.0f), interestValence, idleValence);
			}

			if (gHeadRollIdle < gHeadRoll)
			{
				gHeadRollIdle = Mathf.Clamp(gHeadRollIdle + (tempFloat / 25.0f), gHeadRollIdle, gHeadRoll);
			}
			if (gHeadRollIdle > gHeadRoll)
			{
				gHeadRollIdle = Mathf.Clamp(gHeadRollIdle - (tempFloat / 25.0f), gHeadRoll, gHeadRollIdle);
			}


			if (lElbowHoldActual < lElbowHoldTarget)
			{
				lElbowHoldActual = Mathf.Clamp(lElbowHoldActual + ((tempFloat2 * 0.5f) / Mathf.Lerp(300.0f,150.0f,interestValence/10.0f)), lElbowHoldActual, lElbowHoldTarget);
			}
			if (lElbowHoldActual > lElbowHoldTarget)
			{
				lElbowHoldActual = Mathf.Clamp(lElbowHoldActual - ((tempFloat2 * 0.5f) / Mathf.Lerp(300.0f,150.0f,interestValence/10.0f)), lElbowHoldTarget, lElbowHoldActual);
			}
			if (rElbowHoldActual < rElbowHoldTarget)
			{
				rElbowHoldActual = Mathf.Clamp(rElbowHoldActual + ((tempFloat2 * 0.5f) / Mathf.Lerp(300.0f,150.0f,interestValence/10.0f)), rElbowHoldActual, rElbowHoldTarget);
			}
			if (rElbowHoldActual > rElbowHoldTarget)
			{
				rElbowHoldActual = Mathf.Clamp(rElbowHoldActual - ((tempFloat2 * 0.5f) / Mathf.Lerp(300.0f,150.0f,interestValence/10.0f)), rElbowHoldTarget, rElbowHoldActual);
			}

			if (lThighHoldActual < lThighHoldTarget)
			{
				lThighHoldActual = Mathf.Clamp(lThighHoldActual + ((tempFloat * 1.0f) / Mathf.Lerp(200.0f,150.0f,interestValence/10.0f)), lThighHoldActual, lThighHoldTarget);
			}
			if (lThighHoldActual > lThighHoldTarget)
			{
				lThighHoldActual = Mathf.Clamp(lThighHoldActual - ((tempFloat * 1.0f) / Mathf.Lerp(200.0f,150.0f,interestArousal/10.0f)), lThighHoldTarget, lThighHoldActual);
			}
			if (rThighHoldActual < rThighHoldTarget)
			{
				rThighHoldActual = Mathf.Clamp(rThighHoldActual + ((tempFloat * 1.0f) / Mathf.Lerp(200.0f,150.0f,interestValence/10.0f)), rThighHoldActual, rThighHoldTarget);
			}
			if (rThighHoldActual > rThighHoldTarget)
			{
				rThighHoldActual = Mathf.Clamp(rThighHoldActual - ((tempFloat * 1.0f) / Mathf.Lerp(200.0f,150.0f,interestArousal/10.0f)), rThighHoldTarget, rThighHoldActual);
			}

			if (lKneeHoldActual < lKneeHoldTarget)
			{
				lKneeHoldActual = Mathf.Clamp(lKneeHoldActual + ((tempFloat / 1.0f) / Mathf.Lerp(200.0f,150.0f,interestArousal/10.0f)), lKneeHoldActual, lKneeHoldTarget);
			}
			if (lKneeHoldActual > lKneeHoldTarget)
			{
				lKneeHoldActual = Mathf.Clamp(lKneeHoldActual - ((tempFloat / 1.0f) / Mathf.Lerp(200.0f,150.0f,interestArousal/10.0f)), lKneeHoldTarget, lKneeHoldActual);
			}
			if (rKneeHoldActual < rKneeHoldTarget)
			{
				rKneeHoldActual = Mathf.Clamp(rKneeHoldActual + ((tempFloat / 1.0f) / Mathf.Lerp(200.0f,150.0f,interestArousal/10.0f)), rKneeHoldActual, rKneeHoldTarget);
			}
			if (rKneeHoldActual > rKneeHoldTarget)
			{
				rKneeHoldActual = Mathf.Clamp(rKneeHoldActual - ((tempFloat / 1.0f) / Mathf.Lerp(200.0f,150.0f,interestArousal/10.0f)), rKneeHoldTarget, rKneeHoldActual);
			}

			if (lElbowActual < lElbowTarget)
			{
				lElbowActual = Mathf.Clamp(lElbowActual + Mathf.Min((Mathf.Abs(lElbowActual - lElbowTarget) / 50.0f), tempFloat2), lElbowActual, lElbowTarget);
			}
			if (lElbowActual > lElbowTarget)
			{
				lElbowActual = Mathf.Clamp(lElbowActual -  Mathf.Min((Mathf.Abs(lElbowActual - lElbowTarget) / 50.0f), tempFloat2), lElbowTarget, lElbowActual);
			}
			
			if (rElbowActual < rElbowTarget)
			{
				rElbowActual = Mathf.Clamp(rElbowActual +  Mathf.Min((Mathf.Abs(rElbowActual - rElbowTarget) / 50.0f), tempFloat2), rElbowActual, rElbowTarget);
			}
			if (rElbowActual > rElbowTarget)
			{
				rElbowActual = Mathf.Clamp(rElbowActual -  Mathf.Min((Mathf.Abs(rElbowActual - rElbowTarget) / 50.0f), tempFloat2), rElbowTarget, rElbowActual);
			}
			
			/*if (lThighHoldActual < lThighHoldTarget)
			{
				lThighHoldActual = Mathf.Clamp(lThighHoldActual + tempFloat, lThighHoldActual, lThighHoldTarget);
			}
			if (lThighHoldActual > lThighHoldTarget)
			{
				lThighHoldActual = Mathf.Clamp(lThighHoldActual - tempFloat, lThighHoldTarget, lThighHoldActual);
			}
			if (rThighHoldActual < lThighHoldTarget)
			{
				rThighHoldActual = Mathf.Clamp(rThighHoldActual + tempFloat, rThighHoldActual, rThighHoldTarget);
			}
			if (rThighHoldActual > rThighHoldTarget)
			{
				rThighHoldActual = Mathf.Clamp(rThighHoldActual - tempFloat, rThighHoldTarget, rThighHoldActual);
			}
			if (lKneeHoldActual < lKneeHoldTarget)
			{
				lKneeHoldActual = Mathf.Clamp(lKneeHoldActual + tempFloat, lKneeHoldActual, lKneeHoldTarget);
			}
			if (lKneeHoldActual > lKneeHoldTarget)
			{
				lKneeHoldActual = Mathf.Clamp(lKneeHoldActual - tempFloat, lKneeHoldTarget, lKneeHoldActual);
			}
			if (rKneeHoldActual < lKneeHoldTarget)
			{
				rKneeHoldActual = Mathf.Clamp(rKneeHoldActual + tempFloat, rKneeHoldActual, rKneeHoldTarget);
			}
			if (rKneeHoldActual > rKneeHoldTarget)
			{
				rKneeHoldActual = Mathf.Clamp(rKneeHoldActual - tempFloat, rKneeHoldTarget, rKneeHoldActual);
			}
			
			*/

			if (lThighActual < lThighTarget)
			{
				lThighActual = Mathf.Clamp(lThighActual + Mathf.Min((Mathf.Abs(lThighActual - lThighTarget) / 50.0f), tempFloat), lThighActual, lThighTarget);
			}
			if (lThighActual > lThighTarget)
			{
				lThighActual = Mathf.Clamp(lThighActual - Mathf.Min((Mathf.Abs(lThighActual - lThighTarget) / 50.0f), tempFloat), lThighTarget, lThighActual);
			}
			//SuperController.LogError(lThighActual.ToString());
			if (rThighActual < rThighTarget)
			{
				rThighActual = Mathf.Clamp(rThighActual + Mathf.Min((Mathf.Abs(rThighActual - rThighTarget) / 50.0f), tempFloat), rThighActual, rThighTarget);
			}
			if (rThighActual > rThighTarget)
			{
				rThighActual = Mathf.Clamp(rThighActual - Mathf.Min((Mathf.Abs(rThighActual - rThighTarget) / 50.0f), tempFloat), rThighTarget, rThighActual);
			}
			//SuperController.LogError(rThighActual.ToString());

			if (lKneeActual < lKneeTarget)
			{
				lKneeActual = Mathf.Clamp(lKneeActual + Mathf.Min((Mathf.Abs(lKneeActual - lKneeTarget) / 20.0f), tempFloat), lKneeActual, lKneeTarget);
			}
			if (lKneeActual > lKneeTarget)
			{
				lKneeActual = Mathf.Clamp(lKneeActual - Mathf.Min((Mathf.Abs(lKneeActual - lKneeTarget) / 20.0f), tempFloat), lKneeTarget, lKneeActual);
			}
			
			if (rKneeActual < rKneeTarget)
			{
				rKneeActual = Mathf.Clamp(rKneeActual + Mathf.Min((Mathf.Abs(rKneeActual - rKneeTarget) / 20.0f), tempFloat), rKneeActual, rKneeTarget);
			}
			if (rKneeActual > rKneeTarget)
			{
				rKneeActual = Mathf.Clamp(rKneeActual - Mathf.Min((Mathf.Abs(rKneeActual - rKneeTarget) / 20.0f), tempFloat), rKneeTarget, rKneeActual);
			}


			if (dynAdjustActual < dynAdjustTarget)
			{
				dynAdjustActual = Mathf.Clamp(dynAdjustActual + (tempFloat / 7.0f), dynAdjustActual, dynAdjustTarget);
			}
			if (dynAdjustActual > dynAdjustTarget)
			{
				dynAdjustActual = Mathf.Clamp(dynAdjustActual - (tempFloat / 7.0f), dynAdjustTarget, dynAdjustActual);
			}


			eyeCloseMaxMorph = uiEyeCloseMaxMorph.val;
			eyeOpenMaxMorph = uiEyeOpenMaxMorph.val;
			
			//if (uiSavePreset.val)
			//{
				//savePreset();
			//}
			//if (uiLoadPreset.val)
			//{
			//	loadPreset();
			//}
			//if (uiLoadDefaults.val)
			//{
			//	loadDefaults();
			//}
			
			//SuperController.LogError("Init");

			if (uiFocusTarget.val == "None")
			{
				uiUsePerson2.val = false;
			}
			if (uiObjectTarget.val == "None")
			{
				uiTargetLook.val = false;
			}
			
/*			if (uiObjectTarget.val != "None" && emTarget != null)
			{
				if (emTarget.type == "Person")
				{
					emTargetController = emTarget.GetStorableByID("headControl") as FreeControllerV3;
				}
				else
				{
					emTargetController = emTarget.GetStorableByID("control") as FreeControllerV3;
				}
				if (uiObjectTarget.val == "[CameraRig]")
				{
					emTargetTransform = CameraTarget.centerTarget.transform;
				}
				else
				{
					emTargetTransform = emTargetController.transform;
				}
			}
			else
			{
				emTargetName = "None";
				emTarget = null;
				emTargetController = null;
			}*/
						if (uiObjectTarget.val != "None" && emTargetName != uiObjectTarget.val)
						{
							emTargetName = uiObjectTarget.val;
							emTarget = SuperController.singleton.GetAtomByUid(uiObjectTarget.val);
							if (emTarget != null)
							{
								if (emTarget.type == "Person")
								{
									emTargetController = emTarget.GetStorableByID("headControl") as FreeControllerV3;
								}
								else
								{
									emTargetController = emTarget.GetStorableByID("control") as FreeControllerV3;
								}
								if (uiObjectTarget.val == "[CameraRig]")
								{
									emTargetTransform = CameraTarget.centerTarget.transform;
								}
								else
								{
									emTargetTransform = emTargetController.transform;
								}
							}
							else
							{
								emTargetName = "None";
								emTarget = null;
								emTargetController = null;
							}
						}
						else
						{
							emTargetName = "None";
							emTarget = null;
							emTargetController = null;
						}
			
			//SuperController.LogError("Target Done");
			currentAtom = SuperController.singleton.GetAtomByUid(currentAtomName);
			if (currentAtom == person)
			{
				currentAtom = null;
				currentAtomName = "None";
				uiFocusTarget.val = "None";
			}
			if (currentAtom != person2 && currentAtom != null)
			{
				//SuperController.LogError("reselecting");
				person2 = currentAtom;
				systemSM.Switch(sReselectPerson2);
			}
			//SuperController.LogError("set stuff");
			if (pAgreeableness != uiAgreeableness.val || pExtraversion != uiExtraversion.val || pStableness != uiStableness.val)
				{
				pAgreeableness = uiAgreeableness.val;
				pExtraversion = uiExtraversion.val;
				pStableness = uiStableness.val;
				if (pAgreeableness > 50 || pStableness > 50) { gDirectionCenter = true; } else { gDirectionCenter = false; }
				if (pExtraversion > 75 && pAgreeableness < 25) { gDirectionUp = true; } else { gDirectionUp = false; }
				if (pExtraversion < 25 || pAgreeableness < 50 || pStableness < 50) { gDirectionDown = true; } else { gDirectionDown = false; }
				if (pExtraversion > 50 || pAgreeableness > 50) { gDirectionSide = true; } else { gDirectionSide = false; }
				
				//gAvoidance = Mathf.Clamp((((100.0f - pExtraversion) + (100.0f - pStableness)) / 2.0f), 0.0f, 100.0f);
				gAvoidance = Mathf.Clamp((((pExtraversion) + (pStableness)) / 2.0f), 20.0f, 100.0f);
				gDuration = 5.0f * (Mathf.Clamp((pStableness / 5.0f) + ((100.0f - pAgreeableness) / 2.0f) + (pExtraversion / 5.0f), 0.0f, 100.0f) / 100.0f);
				gFrequency = Mathf.Clamp((pStableness / 2.0f) + (pExtraversion / 2.0f), 0.0f, 100.0f);
				}
			
			if (person != null)
			{
				if (uiConfigHead.val && uiDoHead.val)
				{
					//SuperController.LogError("Config Head");
					//headController.currentPositionState = FreeControllerV3.PositionState.Off;
					if (personEyes != null)
					{
						personEyes.SetStringChooserParamValue("lookMode", "Target");
						//SuperController.LogError("Config Head 2");
					}
					else
					{
						personEyes = containingAtom.GetStorableByID("Eyes");
						personEyes.SetStringChooserParamValue("lookMode", "Target");
						//SuperController.LogError("Config Head 1");
					}

					headController.currentRotationState = FreeControllerV3.RotationState.On;
					headController.jointRotationDriveSpring = uiHeadRotStrength.val;
					headController.jointRotationDriveDamper = uiHeadRotDamper.val;
					headController.jointRotationDriveXTarget = 0.0f;
					headController.jointRotationDriveYTarget = 0.0f;
					headController.jointRotationDriveZTarget = 0.0f;
					headController.RBHoldRotationSpring = Mathf.Lerp(uiHeadRotStrength.val * 0.75f,uiHeadRotStrength.val,interestValence/10.0f);
					if (currentLook == "Kissing" || currentLook == "Sucking")
					{
						headController.RBHoldRotationSpring = uiHeadRotStrength.val * 1.25f;
					}
					headController.RBHoldRotationDamper = Mathf.Lerp(uiHeadRotDamper.val,uiHeadRotDamper.val,interestArousal/10.0f);
					if (morphTongueLength != null)
					{
						//morphTongueLength.morphValue = 0.08f;
					}

					//SuperController.LogError("Config Neck");
					//neckController.currentPositionState = FreeControllerV3.PositionState.Off;
					neckController.currentRotationState = FreeControllerV3.RotationState.On;
					neckSpringTarget = uiHeadRotStrength.val * 2.0f;
					neckController.jointRotationDriveSpring = neckSpringActual;
					neckController.jointRotationDriveDamper = uiHeadRotDamper.val * 2.5f;
					//SuperController.singleton.ClearMessages();
					tempFloat = Mathf.Clamp(Mathf.Abs(actualH) * Mathf.Rad2Deg / uiGazeMaxSideways.val, 0.0f, 1.0f);
					//SuperController.LogMessage("H " + tempFloat, false);
					//SuperController.LogMessage("Actual H " + actualH, false);
					tempFloat2 = Mathf.Clamp(Mathf.Abs(actualV + 0.2f) * Mathf.Rad2Deg / uiGazeMaxUp.val, 0.0f, 1.0f);
					//SuperController.LogMessage("V " + tempFloat2, false);
					//SuperController.LogMessage("Actual V " + actualV, false);
					neckXTarget = Mathf.Lerp(0.0f,20.0f,tempFloat);
					if (actualV < -0.2f)
					{
						neckXTarget = Mathf.Lerp(0.0f,Mathf.Lerp(-30.0f, 0.0f, tempFloat),tempFloat2);
					}
					if (actualV > 0.05f)
					{
						neckXTarget = Mathf.Lerp(0.0f,Mathf.Lerp(10.0f, 20.0f, tempFloat),tempFloat2);
					}
					neckController.jointRotationDriveYTarget = 0.0f;
					neckController.jointRotationDriveZTarget = 0.0f;
					neckController.RBHoldRotationSpring = uiHeadRotStrength.val * 2.0f;
					neckController.RBHoldRotationDamper = uiHeadRotDamper.val * 2.0f;
					if (currentLook == "Sucking")
					{
						neckController.RBHoldRotationSpring = Mathf.Lerp(65,120,interestValence/10.0f);
						neckSpringTarget = uiHeadRotStrength.val * 4.0f;
						neckController.jointRotationDriveSpring = neckSpringActual;
						//neckController.jointRotationDriveDamper = neckSpringActual / 2.0f;
						neckXTarget = Mathf.SmoothStep(-40.0f,00.0f, breathClock);
					}
					else
					{
						neckController.RBHoldRotationSpring = uiHeadRotStrength.val;
						if (mainInterest == "Face" && playerHeadToHead < kissingDistance+0.15f && currentLook == "Kissing")// && lipsTouchCount > 0.0f)
						{
							neckSpringTarget = uiHeadRotStrength.val * 2.5f;
							neckController.jointRotationDriveSpring = neckSpringActual;
							if (playerHeadToHead > kissingDistance - 0.03f && playerHeadToHead < kissingDistance + 0.05f)
							{
								kissingAngle = Mathf.Clamp(kissingAngle - 0.05f,-40.0f,00.0f);
							}
							else
							{
								kissingAngle = Mathf.Clamp(kissingAngle + 0.1f,-40.0f,00.0f);
							}
							neckXTarget = kissingActual;
						}
						else
						{

							if (currentLook == "Sex" && vagTouchCount > 0.0f && mainInterest == "Pelvis" && interestValence > 5.5f)
							{
								kissingAngle = Mathf.Clamp(kissingAngle + 0.15f,0.0f,20.0f);
							}
							else
							{
								kissingAngle = headUpDown / 5.0f;
							}

							if (kissingAngle < 0.0f && headUpDown < 0.0f)
							{
								kissingAngle = Mathf.Clamp(kissingAngle + 0.3f,-40.0f,00.0f);
							}
							//neckController.jointRotationDriveXTarget = kissingActual;
						}
					}
					//SuperController.LogMessage("X Target " + neckXTarget, false);
					neckController.jointRotationDriveXTarget = neckXActual;
					neckController.RBHoldRotationDamper = uiHeadRotDamper.val;
				}
				if (uiDoChest.val && uiUseBodyMotion.val == true)
				{
					//SuperController.LogError("Do Chest");
					chestController.jointRotationDriveSpring = 1000;
					chestController.jointRotationDriveDamper = 235;
					chestController.jointRotationDriveXTarget = (Mathf.SmoothStep(-10.0f,Mathf.Lerp(10.0f,20.0f,interestArousal/10.0f), breathClock) * uiBreatheExpandMultiplier.val) * uiChestAmount.val;
					if (currentLook == "Sucking")
					{
						chestController.jointRotationDriveXTarget = 20.0f - (40.0f * breathClock);
					}
					if (uiIdleAmount.val > 0.0f)
					{
						//chestController.transform.eulerAngles = new Vector3(chestController.transform.eulerAngles.x, pelvisController.transform.eulerAngles.y + ((headLeftRight / 4.0f) * uiIdleAmount.val), chestController.transform.eulerAngles.z);
						chestController.jointRotationDriveXTarget = ((Mathf.Lerp(-10.0f,Mathf.Lerp(10.0f,20.0f,interestArousal/10.0f), breathClock)) * uiChestAmount.val);// + (Mathf.Lerp(5.0f, -20.0f, Mathf.Max(interestArousal, interestValence)/10.0f) * uiIdleAmount.val);
					}
					chestController.jointRotationDriveYTarget = (twistActual / 7.7f) * uiIdleAmount.val;
					chestController.jointRotationDriveZTarget = (gHeadRollIdle * 2.0f) * (1.0f-Mathf.Clamp(Mathf.Abs((twistActual / 7.7f) * uiIdleAmount.val)/25.0f,0.0f,1.0f)) * uiIdleAmount.val;
					abdomenController.jointRotationDriveYTarget = (twistActual / 1.0f) * uiIdleAmount.val;
					abdomenController.jointRotationDriveZTarget = (gHeadRollIdle * 1.0f) * uiIdleAmount.val;
				}
				if (uiDoShoulders.val && uiUseBodyMotion.val == true)
				{
					//SuperController.LogError("Do Shoulders");
					//lShoulderController.RBHoldRotationSpring = 10;
					//lShoulderController.RBHoldRotationDamper = 1;
					tempFloat = 25.0f - (45.0f * uiShoulderHeight.val) + (((5.0f * shoulderUp) + Mathf.Lerp(0.0f,10.0f,interestArousal/10.0f) + (breathClock * Mathf.Lerp(0.05f * uiBreatheExpandMultiplier.val, 0.5f * uiBreatheExpandMultiplier.val,interestArousal/10.0f))) * uiShoulderAmount.val);
					tempFloat2 = (-5.0f + (5.0f * (breathClock * Mathf.Lerp(0.5f, 1.0f,interestArousal/10.0f))) + (pExtraversion/20.0f)) * uiShoulderAmount.val;
					tempFloat2 = Mathf.Clamp((tempFloat2 / 2.0f) + (twistActual / 2.0f),-20.0f, 10.0f);
					if (personIsMale)
					{
						tempFloat = tempFloat / 10.0f;
						tempFloat2 = 0.0f;
					}
					tempFloat2 += uiShoulderBack.val * 20.0f;
					lShoulderController.jointRotationDriveSpring = Mathf.Lerp(70.0f,120.0f,((interestArousal + interestValence)/2.0f) / 10.0f);
					lShoulderController.jointRotationDriveDamper = Mathf.Lerp(45.0f,20.0f,((interestArousal + interestValence)/2.0f) / 10.0f);
					lShoulderController.jointRotationDriveXTarget = tempFloat;
					lShoulderController.jointRotationDriveYTarget = tempFloat2;//(headController.followWhenOff.eulerAngles.y + Mathf.Lerp(10.0f,20.0f,interestValence/10.0f) + (breathClock * Mathf.Lerp(1.0f * uiBreatheExpandMultiplier.val, 2.5f * uiBreatheExpandMultiplier.val,interestArousal/10.0f))) * uiShoulderAmount.val;
					//lShoulderController.jointRotationDriveYTarget += -sexActionNeckX * 2.0f;
					lShoulderController.jointRotationDriveZTarget = Mathf.Lerp(-5.0f, Mathf.Lerp(5.0f, 0.0f, pantSmooth/10.0f),interestArousal/10.0f) * uiShoulderAmount.val;//((-6.0f + (breathClock * 16.0f)) * uiBreatheExpandMultiplier.val) * uiShoulderAmount.val;//headController.followWhenOff.eulerAngles.y;
					//lShoulderController.jointRotationDriveZTarget += -sexActionNeckX * 2.0f;
					//rShoulderController.RBHoldRotationSpring = 10;
					//rShoulderController.RBHoldRotationDamper = 1;
					rShoulderController.jointRotationDriveSpring = Mathf.Lerp(70.0f,120.0f,((interestArousal + interestValence)/2.0f) / 10.0f);
					rShoulderController.jointRotationDriveDamper = Mathf.Lerp(45.0f,20.0f,((interestArousal + interestValence)/2.0f) / 10.0f);
					rShoulderController.jointRotationDriveXTarget = -tempFloat;
					rShoulderController.jointRotationDriveYTarget = -tempFloat2; //(neckController.followWhenOff.eulerAngles.y + Mathf.Lerp(10.0f,20.0f,interestValence/10.0f) + (breathClock * Mathf.Lerp(1.0f * uiBreatheExpandMultiplier.val, 2.5f * uiBreatheExpandMultiplier.val,interestArousal/10.0f))) * uiShoulderAmount.val;
					//rShoulderController.jointRotationDriveYTarget += -sexActionNeckX * 2.0f;
					rShoulderController.jointRotationDriveZTarget = Mathf.Lerp(-5.0f, Mathf.Lerp(5.0f, 0.0f, pantSmooth/10.0f),interestArousal/10.0f) * uiShoulderAmount.val;//((-6.0f + (breathClock * 16.0f)) * uiBreatheExpandMultiplier.val) * uiShoulderAmount.val;
					//rShoulderController.jointRotationDriveZTarget += -sexActionNeckX * 2.0f;
				}
				//SuperController.LogError("Do Idle Arm");
				if (uiIdleArmAmount.val > 0.0f && uiUseBodyMotion.val == true)
				{
					
					//float abAngleUp = Vector3.Angle(abdomenController.followWhenOff.forward, Vector3.up);
					abAngleUp = Vector3.Angle(abdomenController.followWhenOff.forward, Vector3.up);
					dynAdjustTarget = 0.0f;
					string facingPose = "Sitting";
					if (abAngleUp < 20.0f)
					{
						dynAdjustTarget = -30.0f;
						facingPose = "Laying";
					}
					else
					{
						if (abAngleUp < 55.0f)
						{
							dynAdjustTarget = 10.0f;
							facingPose = "LeanBack";
						}
						else
						{
							if (abAngleUp < 85.0f)
							{
								if (Vector3.Angle(abdomenController.followWhenOff.up, Vector3.up) < 7.0f)
								{
									dynAdjustTarget = 0.0f;
									facingPose = "Sitting";
								}
								else
								{
									dynAdjustTarget = -10.0f;
									facingPose = "LeanForward";
								}
							}
							else
							{
								dynAdjustTarget = -10.0f;
								facingPose = "Hanging";
							}
						}
					}
					

					//SuperController.LogError(abAngleUp.ToString() + " " + dynAdjustActual.ToString() + " (" + dynAdjustTarget.ToString() + ")");
					lElbowController.jointRotationDriveSpring = Mathf.Lerp(0.0f, 150.0f, lElbowHoldActual) * uiIdleArmHold.val ;
					lElbowController.jointRotationDriveXTarget = lElbowActual * uiIdleArmAmount.val * (1.0f + (idleValence / 10.0f));
					lElbowController.jointRotationDriveYTarget = lElbowActual * 0.5f * uiIdleArmAmount.val;
					lArmController.jointRotationDriveSpring = Mathf.Lerp(20.0f, 120.0f, lElbowHoldActual) * uiIdleArmHold.val;
					lArmController.jointRotationDriveYTarget = 10.0f + (Mathf.Lerp(50.0f, 30.0f, interestValence/10.0f) * Mathf.Clamp(lElbowActual/100.0f, -1.0f, 1.3f)) + ((Mathf.Lerp(lElbowActual + 20.0f, -lElbowActual - 20.0f, interestArousal/10.0f) + dynAdjustActual) * uiIdleArmAmount.val * (idleArousal / 10.0f)) + (headLastLeftRightActual/1.0f) + uiIdleArmRotOffset.val; //twistActual;
					lArmController.jointRotationDriveZTarget = ((idleArousal - 5.0f) * 1.0f * uiIdleArmAmount.val) + dynAdjustActual + Mathf.Lerp(0.0f,40.0f,Mathf.Abs(lElbowActual / 100.0f)) + uiIdleArmOffset.val;
					//lArmController.jointRotationDriveXTarget = -90.0f + (twist2Actual/3.0f) - Mathf.Lerp(0.0f, 45.0f, Mathf.Abs(dynAdjustActual)/60.0f) + uiIdleArmRotOffset.val;//Mathf.Lerp(-55.0f, -70.0f, lElbowActual / 120.0f);//Mathf.Clamp(-30.0f + Mathf.Abs(lElbowActual) * uiIdleAmount.val,-70.0f, -35.0f);

					lArmController.jointRotationDriveXTarget = -90.0f + Mathf.Lerp(lElbowActual, -lElbowActual, interestArousal/10.0f) - Mathf.Lerp(0.0f, 45.0f, Mathf.Abs(dynAdjustActual)/60.0f);//Mathf.Lerp(-55.0f, -70.0f, lElbowActual / 120.0f);//Mathf.Clamp(-30.0f + Mathf.Abs(lElbowActual) * uiIdleAmount.val,-70.0f, -35.0f);
					
					tempFloat = Vector3.Distance(lHandController.followWhenOff.position, lBreastController.followWhenOff.position);
					//SuperController.LogError("LH2LB " + tempFloat.ToString());
					tempFloat2 = Vector3.Distance(lHandController.followWhenOff.position, rBreastController.followWhenOff.position);
					//SuperController.LogError("LH2RB " + tempFloat2.ToString());//Mathf.Lerp(60.0f, 0.0f, Mathf.Min((tempFloat - interactionDistance)*5.0f,1.0f)).ToString());
					tempFloat = Mathf.Min(tempFloat, tempFloat2);
					//SuperController.LogError(Mathf.Lerp(60.0f, 0.0f, Mathf.Min((tempFloat)*7.0f,1.0f)).ToString());
					if (uiDoHands.val)
					{
						//lHandController.jointRotationDriveSpring = 25.0f * uiIdleArmHold.val;
						//lHandController.jointRotationDriveDamper = 0.5f * uiIdleArmHold.val;
						//lHandController.jointRotationDriveXTarget = Mathf.Clamp(((lElbowActual + 30.0f - Mathf.Lerp(-60.0f, -25.0f, Mathf.Min((tempFloat)*10.0f,1.0f))) * 0.7f) * Mathf.Max(uiIdleArmAmount.val, 1.0f),-30.0f,20.0f);
						//lHandController.jointRotationDriveZTarget = Mathf.Lerp(0.0f, Mathf.Lerp(-40.0f,40.0f, Mathf.Max(Mathf.Abs(-twistActual), 100.0f)/100.0f), Mathf.Min((tempFloat)*5.0f,1.0f));//(lElbowActual + 90.0f) * Mathf.Max(uiIdleArmAmount.val, 1.0f);
						//lHandController.jointRotationDriveYTarget = Mathf.Lerp(0.0f, ((Mathf.Abs(lElbowActual)/3.0f) + (twist2Actual/5.0f)) * Mathf.Max(uiIdleArmAmount.val, 1.0f), Mathf.Min((tempFloat)*5.0f,1.0f));
					}
					
					rElbowController.jointRotationDriveSpring = Mathf.Lerp(0.0f, 150.0f, rElbowHoldActual) * uiIdleArmHold.val;
					rElbowController.jointRotationDriveXTarget = rElbowActual * uiIdleArmAmount.val * (1.0f + (idleValence / 10.0f));
					rElbowController.jointRotationDriveYTarget = rElbowActual * 0.5f * uiIdleArmAmount.val;
					rArmController.jointRotationDriveSpring = Mathf.Lerp(20.0f, 120.0f, rElbowHoldActual) * uiIdleArmHold.val;
					rArmController.jointRotationDriveYTarget = -10.0f + (Mathf.Lerp(50.0f, 30.0f, interestValence/10.0f) * Mathf.Clamp(rElbowActual/100.0f, 1.0f, -500.3f)) - ((Mathf.Lerp(rElbowActual - 20.0f, -rElbowActual + 20.0f, interestArousal/10.0f) - dynAdjustActual) * uiIdleArmAmount.val * (idleArousal / 10.0f)) + (headLastLeftRightActual/1.0f) - uiIdleArmRotOffset.val; //twistActual;
					rArmController.jointRotationDriveZTarget = ((idleArousal - 5.0f) * 1.0f * uiIdleArmAmount.val) + dynAdjustActual + Mathf.Lerp(0.0f,40.0f,Mathf.Abs(rElbowActual / 100.0f)) + uiIdleArmOffset.val;
					//rArmController.jointRotationDriveXTarget = 90.0f - (twist2Actual/3.0f) + Mathf.Lerp(0.0f, 45.0f, Mathf.Abs(dynAdjustActual)/60.0f) - uiIdleArmRotOffset.val;//Mathf.Lerp(55.0f, 70.0f, rElbowActual / 120.0f);//Mathf.Clamp(30.0f - Mathf.Abs(rElbowActual) * uiIdleAmount.val,70.0f, 35.0f);
					
					rArmController.jointRotationDriveXTarget = 90.0f + Mathf.Lerp(rElbowActual, -rElbowActual, interestArousal/10.0f) + Mathf.Lerp(0.0f, 45.0f, Mathf.Abs(dynAdjustActual)/60.0f);//Mathf.Lerp(55.0f, 70.0f, rElbowActual / 120.0f);//Mathf.Clamp(30.0f - Mathf.Abs(rElbowActual) * uiIdleAmount.val,70.0f, 35.0f);
					tempFloat = Vector3.Distance(rHandController.followWhenOff.position, lBreastController.followWhenOff.position);
					//SuperController.LogError("RH2LB " + tempFloat.ToString());
					tempFloat2 = Vector3.Distance(rHandController.followWhenOff.position, rBreastController.followWhenOff.position);
					//SuperController.LogError("RH2RB " + tempFloat2.ToString());//Mathf.Lerp(60.0f, 0.0f, Mathf.Min((tempFloat - interactionDistance)*5.0f,1.0f)).ToString());
					tempFloat = Mathf.Min(tempFloat, tempFloat2);
					//SuperController.LogError(Mathf.Lerp(60.0f, 0.0f, Mathf.Min((tempFloat)*7.0f,1.0f)).ToString());
					//tempFloat = Mathf.Min(tempFloat, Vector3.Distance(rHandController.followWhenOff.TransformPoint(new Vector3(-0.07f, 0.05f, -0.2f)), rBreastController.followWhenOff.position));
					//SuperController.LogError(tempFloat.ToString());//Mathf.Lerp(60.0f, 0.0f, Mathf.Min((tempFloat - interactionDistance)*5.0f,1.0f)).ToString());
					if (uiDoHands.val)
					{
						//rHandController.jointRotationDriveSpring = 25.0f * uiIdleArmHold.val;
						//rHandController.jointRotationDriveDamper = 0.5f * uiIdleArmHold.val;
						//rHandController.jointRotationDriveXTarget = Mathf.Clamp(((rElbowActual - 30.0f + Mathf.Lerp(60.0f, 25.0f, Mathf.Min((tempFloat)*10.0f,1.0f))) * 0.7f) * Mathf.Max(uiIdleArmAmount.val, 1.0f),-20.0f,30.0f);
						//rHandController.jointRotationDriveZTarget = Mathf.Lerp(0.0f, Mathf.Lerp(40.0f,-40.0f, Mathf.Max(Mathf.Abs(twistActual), 100.0f)/100.0f), Mathf.Min((tempFloat)*5.0f,1.0f));//(rElbowActual - 90.0f) * Mathf.Max(uiIdleArmAmount.val, 1.0f);
						//rHandController.jointRotationDriveYTarget = Mathf.Lerp(0.0f, -((Mathf.Abs(rElbowActual)/3.0f) - (twist2Actual/5.0f)) * Mathf.Max(uiIdleArmAmount.val, 1.0f), Mathf.Min((tempFloat)*5.0f,1.0f));
					}
				}
				else
				{
					/*lElbowController.jointRotationDriveSpring = 5.0f;
					lElbowController.jointRotationDriveDamper = 3.5f;
					lArmController.jointRotationDriveSpring = 5.0f;
					lArmController.jointRotationDriveDamper = 3.5f;
					rElbowController.jointRotationDriveSpring = 5.0f;
					rElbowController.jointRotationDriveDamper = 3.5f;
					rArmController.jointRotationDriveSpring = 5.0f;
					rArmController.jointRotationDriveDamper = 3.5f;*/
				}
				//SuperController.LogError("Do Idle Body");
				if (uiIdleAmount.val > 0.0f && uiUseBodyMotion.val == true)
				{
				
					pelvis2Controller.jointRotationDriveSpring = 1325.0f * uiIdleHoldPow.val;
					pelvis2Controller.jointRotationDriveDamper = 55.0f * uiIdleHoldPow.val;
					
					pelvis2Controller.jointRotationDriveXTarget = (headLastUpDown / 30.0f);
					pelvis2Controller.jointRotationDriveZTarget = (gHeadRollIdle * uiIdleAmount.val) / 2.0f;
					pelvis2Controller.jointRotationDriveYTarget = Mathf.Clamp(headLastLeftRightActual / 2.0f, -15.0f, 15.0f);//(twistActual * uiIdleAmount.val) / 14.6f;

					lowerabsController.jointRotationDriveSpring = 675.0f * uiIdleHoldPow.val;
					lowerabsController.jointRotationDriveDamper = 135.0f * uiIdleHoldPow.val;

					lowerabsController.jointRotationDriveXTarget = (headLastUpDown / 30.0f);
					lowerabsController.jointRotationDriveYTarget = (headLastLeftRightActual * uiIdleAmount.val) * 5.0f;
					lowerabsController.jointRotationDriveZTarget = (twistActual * uiIdleAmount.val) / 1.6f;
					
					if (currentInterest == "Tip" && playerTipToHead < interactionDistance * 1.15f)
					{
//						headController.currentPositionState = FreeControllerV3.PositionState.on;
						
					}
					
					//chestController.jointRotationDriveXTarget = twistActual * 2.0f * uiIdleAmount.val;
					chestController.jointRotationDriveZTarget = -gHeadRollIdle * 0.5f * uiIdleAmount.val;
				
					abdomenController.jointRotationDriveSpring = 475.0f * uiIdleHoldPow.val;
					abdomenController.jointRotationDriveDamper = 135.0f * uiIdleHoldPow.val;
					abdomenController.jointRotationDriveXTarget = idleArousal * uiIdleAmount.val;
					abdomenController.jointRotationDriveYTarget = (headLastLeftRightActual  * 5.0f) * uiIdleAmount.val;
					abdomenController.jointRotationDriveZTarget = gHeadRollIdle * 0.1f * uiIdleAmount.val;
					if (vagTouchCount > 0.0f && uiDoSex.val && (playerLHandToPelvis < interactionDistance * 1.5f || playerRHandToPelvis < interactionDistance * 1.5f || playerHeadToPelvis < interactionDistance * 1.5f || emTargetPelvisDistance < interactionDistance * 1.5f))
					{
						//chestController.jointRotationDriveSpring = 400.0f * uiIdleHoldPow.val;
						//abdomenController.jointRotationDriveSpring = 475.0f * uiIdleHoldPow.val;
						chestController.jointRotationDriveYTarget = (sexActionNeckActual * 5.0f) + (Mathf.Max(idleArousal - 8.0f) * 5.0f);
						abdomenController.jointRotationDriveYTarget = -sexActionNeckActual * 5.0f;
					}
					
					lKneeController.jointRotationDriveSpring = Mathf.Lerp(30.0f, 75.0f, lKneeHoldActual) * uiIdleLegHold.val * Mathf.Lerp(1.0f, 0.3f, Mathf.Clamp(Mathf.Abs(lThighTarget), 0.0f, 100.0f) / 100.0f);
					lKneeController.jointRotationDriveXTarget = -30.0f + (lKneeActual * uiIdleLegAmount.val * (0.5f + ((idleValence / 10.0f)/2.0f)));//Mathf.Clamp(rElbowActual * -1.5f, -20.0f, -150.0f);
					rKneeController.jointRotationDriveSpring = Mathf.Lerp(30.0f, 75.0f, rKneeHoldActual) * uiIdleLegHold.val * Mathf.Lerp(1.0f, 0.3f, Mathf.Clamp(Mathf.Abs(rThighTarget), 0.0f, 100.0f) / 100.0f);
					rKneeController.jointRotationDriveXTarget = -30.0f + (rKneeActual * uiIdleLegAmount.val * (0.5f + ((idleValence / 10.0f)/2.0f)));//Mathf.Clamp(lElbowActual * 1.5f, -20.0f, -150.0f);

					lThighController.jointRotationDriveSpring = Mathf.Lerp(60.0f, 335.0f, lThighHoldActual) * uiIdleLegHold.val;
					rThighController.jointRotationDriveSpring = Mathf.Lerp(60.0f, 335.0f, rThighHoldActual) * uiIdleLegHold.val;
					
					lThighController.jointRotationDriveYTarget = (-60.0f + (-gHeadRollIdle * 1.0f * uiIdleLegAmount.val * (0.5f + ((idleArousal / 10.0f)/2.0f))) - lKneeActual) / 2.0f;
					rThighController.jointRotationDriveYTarget = (-60.0f + (gHeadRollIdle * 1.0f * uiIdleLegAmount.val * (0.5f + ((idleArousal / 10.0f)/2.0f))) - rKneeActual) / 2.0f;
					tempFloat = Mathf.Lerp(Mathf.Lerp(10.0f, -10.0f, interestValence/10.0f), -35.0f, interestArousal/10.0f);
					lThighController.jointRotationDriveXTarget = lThighActual;//Mathf.Clamp(-headLeftRight / 3.0f, -15.0f, 15.0f);
					rThighController.jointRotationDriveXTarget = rThighActual;//Mathf.Clamp(headLeftRight / 3.0f, -15.0f, 15.0f);
					lThighController.jointRotationDriveZTarget = Mathf.Clamp(-headLastLeftRightActual / 2.0f, -15.0f, 15.0f);
					rThighController.jointRotationDriveZTarget = Mathf.Clamp(headLastLeftRightActual / 2.0f, -15.0f, 15.0f);
				}
				//SuperController.LogError("Do breast lift");
				if (personIsMale == false)
				{
					if (morphBreastDroopLeft != null && morphBreastDroopRight != null && morphBreastHangLeft != null && morphBreastHangRight != null)
					{
						float diff = (chestController.followWhenOff.position.y - lElbowController.followWhenOff.position.y) * 1.5f;
						//float tempval = Mathf.Lerp(0.0f,-0.25f,Mathf.Clamp(diff, 0.0f, 1.0f));
						//testString = Mathf.Clamp(diff,-0.25f, 0.0f).ToString();
						tempFloat = -0.5f * uiBreastLift.val;
						morphBreastDroopLeft.morphValue = Round(Mathf.Clamp(diff,tempFloat, 0.0f));
						morphBreastHangLeft.morphValue = Round(Mathf.Clamp(diff,tempFloat, 0.0f));
						diff = (chestController.followWhenOff.position.y - rElbowController.followWhenOff.position.y) * 1.5f;
						//testString = diff.ToString();
						//tempval = Mathf.Lerp(0.0f,-0.25f,Mathf.Clamp(diff, 0.0f, 1.0f));
						morphBreastDroopRight.morphValue = Round(Mathf.Clamp(diff,tempFloat, 0.0f));
						morphBreastHangRight.morphValue = Round(Mathf.Clamp(diff,tempFloat, 0.0f));
					}
				}
			}
			
			//playerFace = player.position;
			if (usePerson2 && person2Usable)
			{
				playerHeadTransform = playerHeadController.followWhenOff;
				playerLHand = playerLHandController.followWhenOff.position;
				playerRHand = playerRHandController.followWhenOff.position;
				playerLHandTransform = playerLHandController.followWhenOff;
				playerRHandTransform = playerRHandController.followWhenOff;
			}
			else
			{
				playerHeadTransform = CameraTarget.centerTarget.transform;
				if (playerHandsUsable && usePerson2 == false)
				{
					playerLHand = playerVRLHand.position;
					playerRHand = playerVRRHand.position;
					playerLHandTransform = playerVRLHand;
					playerRHandTransform = playerVRRHand;
				}
				if (person2Usable == false && playerHandsUsable == false)
				{
					playerLHand = playerHeadTransform.position;
					playerRHand = playerHeadTransform.position;
					playerLHandTransform = playerHeadTransform;
					playerRHandTransform = playerHeadTransform;
				}
			}
			triggerArousal.val = interestArousal;
			triggerValence.val = interestValence;
			
			//emotionSM.OnUpdate();
            string dbgHead = "";
            string dbgLHand = "";
            string dbgRHand = "";
            string dbgPenis = "";
            string dbgObject = "";
            string dbgPLHand = "";
            string dbgPRHand = "";
			
            if (lastBrowState != browSM.CurrentState && browSM.CurrentState != null)
            {
                lastBrowState = browSM.CurrentState;
            }
            if (lastMouthState != mouthSM.CurrentState && mouthSM.CurrentState != null)
            {
                lastMouthState = mouthSM.CurrentState;
            }
            if (lastEyesState != eyesSM.CurrentState && eyesSM.CurrentState != null)
            {
                lastEyesState = eyesSM.CurrentState;
            }
            if (lastLookState != lookSM.CurrentState && lookSM.CurrentState != null)
            {
                lastLookState = lookSM.CurrentState;
            }
			//SuperController.LogError("Previous States Captured");
			
			/*tempFloat = lElbowController.followWhenOff.localEulerAngles.x;
			tempFloat2 = lElbowController.followWhenOff.localEulerAngles.y;
			if (tempFloat > 270.0f && tempFloat2 < 250.0f)
			{
			morphLBicepFlex.morphValue = Round(Mathf.Lerp(0.8f,0.0f,(tempFloat - 270.0f) / 90.0f));
			}
			else
			{
			morphLBicepFlex.morphValue = 0;
			}
			
			tempFloat = rElbowController.followWhenOff.localEulerAngles.x;
			tempFloat2 = rElbowController.followWhenOff.localEulerAngles.y;
			if (tempFloat > 270.0f && tempFloat2 < 250.0f)
			{
			morphRBicepFlex.morphValue = Round(Mathf.Lerp(0.8f,0.0f,(tempFloat - 270.0f) / 90.0f));
			}
			else
			{
			morphRBicepFlex.morphValue = 0;
			}*/

			
            interestArousal = Mathf.Clamp(interestArousal, Mathf.Lerp(2.0f, 5.0f, interestValence/10.0f), 10.0f);
            interestValence = Mathf.Clamp(interestValence, Mathf.Lerp(1.0f, 5.0f, interestArousal/10.0f), 10.0f);
			if (currentLook == "Kissing" || currentLook == "Sucking" || currentLook == "Sex")
			{
				interestArousal += 0.1f;
				interestValence += 0.05f;
			}
			tempFloat = Vector3.Distance(headController.followWhenOff.position, prevPosHead) * 150.0f;
			tempFloat2 = 1.5f;
			if (tempFloat > 0.15f)
			{
				interestValence += Mathf.Lerp(Mathf.Lerp(0.03f * (pExtraversion/100.0f), 0.065f * (pExtraversion/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), 0.0f, Mathf.Clamp(interestValence-2.0f, 0.0f, 10.0f)/8.0f) * (uiMovementInterest.val / tempFloat2);
				interestArousal += Mathf.Lerp(Mathf.Lerp(0.0f * (pAgreeableness/100.0f), 0.003f * (pAgreeableness/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), 0.0f, Mathf.Clamp(interestArousal-4.0f, 0.0f, 10.0f)/6.0f) * (uiMovementInterestArousal.val / tempFloat2);
				if (currentEye == "Idle" || currentEye == "Closed")
				{
					eyesSM.Switch(eOpen);
				}
			}
			tempFloat = Vector3.Distance(chestController.followWhenOff.position, prevPosChest) * 100.0f;
			if (tempFloat > 0.1f)
			{
				interestValence += Mathf.Lerp(Mathf.Lerp(0.04f * (pExtraversion/100.0f), 0.015f * (pExtraversion/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), 0.0f, Mathf.Clamp(interestValence-2.0f, 0.0f, 10.0f)/8.0f) * (uiMovementInterest.val / tempFloat2);
				interestArousal += Mathf.Lerp(Mathf.Lerp(0.002f * (pAgreeableness/100.0f), 0.006f * (pAgreeableness/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), 0.0f, Mathf.Clamp(interestArousal-4.0f, 0.0f, 10.0f)/6.0f) * (uiMovementInterestArousal.val / tempFloat2);
			}
			tempFloat = Vector3.Distance(abdomenController.followWhenOff.position, prevPosHip) * 100.0f;
      //SuperController.LogError("Movement : " + Round(tempFloat));
			if (tempFloat > 0.1f)
			{
				interestValence += Mathf.Lerp(Mathf.Lerp(0.0f, 0.065f * (pExtraversion/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), 0.0f, Mathf.Clamp(interestValence-2.0f, 0.0f, 10.0f)/8.0f) * (uiMovementInterest.val / tempFloat2);
				interestArousal += Mathf.Lerp(Mathf.Lerp(0.005f * (pAgreeableness/100.0f), 0.007f * (pAgreeableness/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), Mathf.Lerp(0.0f, 0.025f * (pAgreeableness/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), Mathf.Clamp(interestArousal-4.0f, 0.0f, 10.0f)/6.0f) * (uiMovementInterestArousal.val / tempFloat2);
			}
			tempFloat = Vector3.Distance(lHandController.followWhenOff.TransformPoint(new Vector3(-0.08f, 0.00f, 0.00f)), prevPosLHand) * 25.0f;
			if (tempFloat > 0.05f)
			{
				interestValence += Mathf.Lerp(Mathf.Lerp(0.002f, 0.035f * (((pExtraversion+pAgreeableness)/2)/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), 0.0f, Mathf.Clamp(interestValence-2.0f, 0.0f, 10.0f)/8.0f) * (uiMovementInterest.val / tempFloat2);
			}
			tempFloat = Vector3.Distance(rHandController.followWhenOff.TransformPoint(new Vector3(0.08f, 0.00f, 0.00f)), prevPosRHand) * 0.25f;
			if (tempFloat > 0.05f)
			{
				interestValence += Mathf.Lerp(Mathf.Lerp(0.002f, 0.035f * (((pExtraversion+pAgreeableness)/2)/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), 0.0f, Mathf.Clamp(interestValence-2.0f, 0.0f, 10.0f)/8.0f) * (uiMovementInterest.val / tempFloat2);
			}
			tempFloat = Vector3.Distance(lFootController.followWhenOff.position, prevPoslFoot) * 30.0f;
			if (tempFloat > 0.05f)
			{
				interestValence += Mathf.Lerp(Mathf.Lerp(0.001f, 0.055f * (((pExtraversion+pAgreeableness)/2)/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), 0.0f, Mathf.Clamp(interestValence-2.0f, 0.0f, 10.0f)/8.0f) * (uiMovementInterest.val / tempFloat2);
			}
			tempFloat = Vector3.Distance(rFootController.followWhenOff.position, prevPosrFoot) * 30.0f;
			if (tempFloat > 0.05f)
			{
				interestValence += Mathf.Lerp(Mathf.Lerp(0.001f, 0.055f * (((pExtraversion+pAgreeableness)/2)/100.0f), Mathf.Clamp(tempFloat, 0.0f, 1.0f)), 0.0f, Mathf.Clamp(interestValence-2.0f, 0.0f, 10.0f)/8.0f) * (uiMovementInterest.val / tempFloat2);
			}

			if (playerHeadToHead < personalSpaceDistance || playerLHandToHead < personalSpaceDistance || playerRHandToHead < personalSpaceDistance)
			{
				interestValence = Mathf.Clamp(interestValence + (Mathf.Lerp(0.03f * ((100.0f-pExtraversion)/100.0f), 0.055f * (pExtraversion/100.0f), Mathf.Clamp(Vector3.Distance(headController.followWhenOff.position, prevPosHead), 0.0f, 1.0f)) * (uiMovementInterest.val / tempFloat2)), 3.5f, 10.0f);
			}
			//SuperController.LogError("Eye Dialation Start");
			if (uiPupilDialation.val > 0.0f)
			{
				tempFloat = 0.0f;
				if (playerHeadToFaceRot < playerLookDirectAngle && mainInterest == "Face" && Vector3.Distance(new Vector3(0.0f,0.0f,0.0f),saccadeOffset) < 25.0f && mEyesClosedLeftValue <= 0.7f && amGlancing == false && gAvoid == 0.0f && playerHeadToHead < personalSpaceDistance && saccadeOffset.y > -10.0f)
				{
					mEyesPupilsTarget =  tempFloat + (Mathf.Lerp(0.2f,0.5f,interestArousal/10.0f) * uiPupilDialation.val); //mEyesPupilsOrig +
				}
				else
				{
					mEyesPupilsTarget = tempFloat;
				}
				if (currentEye == "Closed" && mEyesClosedLeftValue > 0.5f)
				{
					mEyesPupilsTarget = 0.8f * uiPupilDialation.val;
				}
			}
						//SuperController.singleton.ClearMessages();


				MaterialOptions[] mo2 = containingAtom.gameObject.GetComponentsInChildren<MaterialOptions>();
				if(mo2!=null)
				{
					//SuperController.LogMessage("Materials found = " + mo2.Length.ToString());
					foreach (MaterialOptions m in mo2)
					{
						//SuperController.LogMessage(m.name); //Show all the names
						if (m.name == "RenKayla")
						{
							tempFloat = m.GetFloatParamValue("Gloss");
							currentGloss = tempFloat;
							break;
						}
					}
				}
			tempFloat2 = ((Mathf.Clamp(currentGloss, 4.47f, 11.0f)-4.47f)/(11.0f-4.47f));
			normalizedGloss = tempFloat2;
			//SuperController.singleton.ClearMessages();	
				
			if (pantingTimeout <= 0.0f)
			{
				pantingTimeout = 2.75f * uiBreatheSpeed.val;
				pantCount = Mathf.Max(pantCount - (pantCount / Mathf.Lerp(5.0f, 2.0f, interestArousal/10.0f)),0.0f);
				
				//SuperController.LogMessage("Setting Pant " + Round(pantCount), false);
			}
			else
			{
				pantingTimeout -= Time.fixedDeltaTime;
			}
			
			if (pantSmooth < pantCount)
			{
				pantSmooth = Mathf.Min(pantSmooth + Mathf.Lerp(0.01f, 0.2f, (interestArousal - interestArousalLast) * Mathf.Lerp(1.0f, 3.0f, interestArousal/10.0f)), pantCount);
			}
			if (pantSmooth > pantCount)
			{
				pantSmooth = Mathf.Max(pantCount, pantSmooth - Mathf.Lerp(0.01f, 0.2f, (interestArousal - interestArousalLast) * Mathf.Lerp(1.0f, 3.0f, interestArousal/10.0f)));
			}
			//SuperController.LogMessage("Smoothed Pant " + Round(pantSmooth), false);			
			//SuperController.LogMessage("Diff " + Round(interestArousal - interestArousalLast), false);
				
			if (interestArousalLast > 5.0f && interestArousalLast < interestArousal)
			{
				//pantCount = Mathf.Max(pantCount - ((interestArousal - interestArousalLast) * Mathf.Lerp(0.5f, 2.0f, interestArousal/10.0f)),0.0f);
			}
			interestArousalLast = interestArousal;
			
			//SuperController.LogMessage("Gloss : " + tempFloat + " | Value " + normalizedGloss);
			//breatheInSpeed = Mathf.Lerp(Mathf.Lerp(1.0f, 0.5f, interestValence/10.0f), Mathf.Lerp(1.9f,1.0f,pantCount / 10.0f), interestArousal/10.0f) * uiBreatheSpeed.val;
            //breatheOutSpeed = Mathf.Lerp(Mathf.Lerp(0.5f, 1.5f, interestValence/10.0f),Mathf.Lerp(1.0f,0.5f,pantCount / 10.0f),interestArousal/10.0f) * breatheInSpeed;
			breatheInSpeed = Mathf.Lerp(1.0f, Mathf.Lerp(3.9f,2.0f,pantSmooth / 20.0f), interestArousal/10.0f) * uiBreatheSpeed.val;
            breatheOutSpeed = Mathf.Lerp(0.55f,Mathf.Lerp(2.3f,0.5f,pantSmooth / 20.0f), interestArousal/10.0f) * breatheInSpeed;
			if (currentGloss - lastGloss >= 0.005f)
			{
				//breatheOutSpeed = 0.0f;
			}
			lastGloss = currentGloss;
			//SuperController.LogMessage("Breathing In/Out Sp " + Round(breatheInSpeed) + "/" + Round(breatheOutSpeed) + "|Pants " + Round(pantCount) + "|Smooth " + Round(pantSmooth) + "| Last Gloss " + Round(lastGloss));
			if (pantCount > 5.0f)
			{
				//breatheInSpeed = Mathf.Lerp(1.0f, Mathf.Lerp(0.5f,0.1f,pantCount / 10.0f), interestArousal/10.0f) * uiBreatheSpeed.val;
				//breatheOutSpeed = Mathf.Lerp(0.5f,Mathf.Lerp(0.03f,0.02f,pantCount / 10.0f),interestArousal/10.0f) * breatheInSpeed;
			}
            //breatheInSpeed = Mathf.Lerp(1.0f, 1.3f, interestArousal/10.0f) * uiBreatheSpeed.val;
            //breatheOutSpeed = breatheInSpeed * Mathf.Lerp(0.7f,1.5f,interestArousal/10.0f);
            if (interestKissing || playerTipToHead < interactionDistance || lipsTouchCount > 1.0f)
            {
                breatheInSpeed = Mathf.Lerp(1.5f, 2.0f,interestArousal / 10.0f) * uiBreatheSpeed.val;
                breatheOutSpeed = Mathf.Lerp(0.4f,1.5f,interestValence / 10.0f) * uiBreatheSpeed.val;
            }
            //breathing
			if (voiceMoan)
			{
				breatheOutSpeed = breatheOutSpeed * 0.75f;
				if (mVisAAValue > mVisAATarget - 0.01f)
				{
					mVisAATarget = 0.0f;
					voiceOpenAdjust = 0.0f;
				}
				if (mVisMValue > mVisMTarget - 0.01f)
				{
					mVisMTarget = 0.0f;
					mLipsCloseTarget = 0.0f;
				}
				if (mVisOWValue > mVisOWTarget - 0.01f)
				{
					mVisOWTarget = 0.0f;
					voiceOpenAdjust = 0.0f;
				}
			}
			else
			{
				voiceOpenAdjust = 0.0f;
				voicePuckerAdjust = 0.0f;
				mVisAATarget = 0.0f;
				mVisMTarget = 0.0f;
				mVisOWTarget = 0.0f;
				mLipsCloseTarget = 0.0f;
			}
			float tempRandom = -1.0f;
			//SuperController.LogError("Kissing Sounds");
			if (interestKissing && uiDoSounds.val && ((AudioSourceControl)headAudio).playingClip == null)// && lipsTouchCount > 0.0f)
			{
				tempRandom = Mathf.Round(Random.Range(1.0f,10.0f));
				while (tempRandom == lastRandomVoice)
				{
					tempRandom = Mathf.Round(Random.Range(1.0f,10.0f));
				}
				audioClip = URLAudioClipManager.singleton.GetClip(@"Kiss" + tempRandom + ".wav");
				if (Random.Range(0.0f,100.0f) > 80.0f)
				{
					//audioClip = URLAudioClipManager.singleton.GetClip(@"Kiss_Mmm" + Mathf.Round(Random.Range(1.0f,2.0f)) + ".wav");
					tempRandom = Mathf.Round(Random.Range(1.0f,16.0f));
					while (tempRandom == lastRandomVoice)
					{
						tempRandom = Mathf.Round(Random.Range(1.0f,16.0f));
					}
					audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_Mmm" + tempRandom + ".wav");
					//energyAmount += 1.0f;
				}
				headAudio.SetFloatParamValue("minDistance", 0.09f * uiSoundVolume.val);
				headAudio.CallAction("PlayNow", audioClip);
				lastRandomVoice = tempRandom;
			}
			//SuperController.LogError("Breathing in Start");
			
			
            float breathingRate = (Random.Range(0.1f, 0.35f) + (interestArousal / Mathf.Lerp(15.0f,25.0f, pantSmooth/5.0f))) * uiBreatheSpeed.val;
			
            if (breathState == "in" && breathHold <= 0.0f && (lipsTouchCount < 1.0f || (lipsTouchCount > 0.0f && playerTipToHead > 0.077f)))
            {
                breathClock = Mathf.Min(breathClock + (Time.fixedDeltaTime * (breathingRate * breatheInSpeed)), 1.0f);
                if (breathClock == 1.0f && (((AudioSourceControl)headAudio).playingClip == null || voiceMoan == false))
                {
                    breathState = "out";
					if (Random.Range(0.0f, Mathf.Lerp(80.0f, 0.0f, interestArousalLast / 10.0f)) < pantCount && pantCount < 8.0f)
						{
						pantCount += Mathf.Lerp(Mathf.Lerp(0.0f, 10.0f, interestArousalLast / 10.0f), 20.0f, (interestArousal - interestArousalLast) * Mathf.Lerp(0.5f, 3.0f, interestArousalLast / 10.0f));
						//SuperController.LogMessage("Sigh");
						}
					else
						{
						pantCount += 2.0f;
						}	
					breathHold = Mathf.Lerp(0.1f,0.03f,interestArousal/10.0f) * uiBreatheSpeed.val;
					if (uiDoSounds.val && interestKissing == false)
					{
						if ((mMouthOpenValue < 0.03f && mMouthOpenWideValue < 0.05f && mSmileFullFaceValue/2.0f < 0.07f && mSmileOpenFullFaceValue < 0.05f && mSmileSimpleLeftValue < 0.5f && mSmileSimpleRightValue < 0.5f && mExcitementValue < 0.05f && mTakingItValue < 0.1f) && (currentMouth == "Idle" || currentMouth == "Closed" || currentMouth == "Open" || currentMouth == "LipBite") )// && interestArousal < 9.5f && currentLook != "Feel" && currentMouth != "Smile" && currentMouth != "Big Smile" && (currentLook != "Playful" || interestValence < 5.0f))
						{
							breathWMouth = false;
							voicePuckerAdjust = 0.0f;
							tempFloat = Mathf.Lerp(140.0f,60.0f,interestArousal/10.0f) * (3.0f - moanChance);
							if (lipsTouchCount > 0.0f || vagTouchCount > 0.0f || playerLHandInteract || playerRHandInteract)
							{
								tempFloat = tempFloat / 2.0f;
							}
							if (((interestArousal > 4.0f && Random.Range(0.0f,100.0f) > tempFloat) || (currentLook == "Feel" && interestArousal > 7.0f && Random.Range(0.0f,140.0f) > tempFloat)) && mLipBiteValue < 0.1f && mLipsPuckerValue < 0.1f && mDeserveItValue < 0.1f && mLipBiteTarget < 0.1f && mLipsPuckerTarget < 0.1f && mDeserveItTarget < 0.1f)
							{
								tempRandom = Mathf.Round(Random.Range(1.0f,16.0f));
								while (tempRandom == lastRandomVoice)
								{
									tempRandom = Mathf.Round(Random.Range(1.0f,16.0f));
								}
								audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_Mmm" + tempRandom + ".wav");
								voiceOpenAdjust = -0.25f;
								mVisMTarget = 0.45f;
								mLipsCloseTarget = 0.16f;
								headAudio.SetFloatParamValue("minDistance", 0.07f * uiSoundVolume.val);
								voiceMoan = true;
								if (Random.Range(0.0f,100.0f) > Mathf.Lerp(200, 50.0f, interestArousal/10.0f) && currentLook != "Feel" && currentLook != "Sex" && playerHeadToHead < personalSpaceDistance/2.0f && testRun == false && lookAction == false)
								{
									lookSM.Switch(lFeel);
								}
								//energyAmount += 1.0f;
								//SuperController.LogMessage("Moaning", false);
							}
							else
							{
								tempRandom = Mathf.Round(Random.Range(1.0f,4.0f));
								while (tempRandom == lastRandomVoice)
								{
									tempRandom = Mathf.Round(Random.Range(1.0f,4.0f));
								}
								audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Nose_Out_Long" + tempRandom + ".wav");
								//breatheOutSpeed = breatheOutSpeed / 2.0f;
								headAudio.SetFloatParamValue("minDistance", 0.01f * uiSoundVolume.val);
								voiceMoan = false;
							}
						}
						else
						{
							breathWMouth = true;
							tempFloat = Mathf.Lerp(150.0f,60.0f,interestArousal/10.0f) * (3.0f - moanChance);
							if (vagTouchCount > 0.0f || playerLHandInteract || playerRHandInteract || interestArousal > 9.0f || (interestArousal > 5.0f && interestValence > 7.0f))
							{
								tempFloat = tempFloat / 2.0f;
							}
							if (((interestArousal > 4.0f && Random.Range(0.0f,100.0f) > tempFloat) || (currentLook == "Feel" && interestArousal > 7.0f && Random.Range(0.0f,140.0f) > tempFloat) || currentLook == "Sex") && (mLipBiteValue < 0.1f && mLipsPuckerValue < 0.1f && mDeserveItValue < 0.1f && mLipBiteTarget < 0.1f && mLipsPuckerTarget < 0.1f && mDeserveItTarget < 0.1f))
							{
								if (Random.Range(0.0f,100.0f) > 60.0f)
								{
									tempRandom = Mathf.Round(Random.Range(1.0f,15.0f));
									while (tempRandom == lastRandomVoice)
									{
										tempRandom = Mathf.Round(Random.Range(1.0f,15.0f));
									}
									audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_Ooh" + tempRandom + ".wav");
									//breatheOutSpeed = breatheOutSpeed / 2.0f;
									//voicePuckerAdjust = 0.2f;
									//energyAmount += 1.0f;
									mVisOWTarget = 0.5f;
								}
								else
								{
									tempRandom = Mathf.Round(Random.Range(1.0f,16.0f));
									while (tempRandom == lastRandomVoice)
									{
										tempRandom = Mathf.Round(Random.Range(1.0f,16.0f));
									}
									audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_Aah" + tempRandom + ".wav");
									//energyAmount += 1.0f;
									mVisAATarget = 0.4f;
									if ((vagTouchCount > 0.0f && Random.Range(0.0f,100.0f) > 30.0f) || Random.Range(0.0f,100.0f) > Mathf.Lerp(90.0f, 50.0f, interestArousal/10.0f))
									{
										tempRandom = Mathf.Round(Random.Range(1.0f,8.0f));
										while (tempRandom == lastRandomVoice)
										{
											tempRandom = Mathf.Round(Random.Range(1.0f,8.0f));
										}
										audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_Yeah" + tempRandom + ".wav");
										voicePuckerAdjust = 0.3f;
										voiceOpenAdjust = 0.2f;
										mVisOWTarget = 0.2f;
										mVisAATarget = 0.3f;
									}
									//mVisFTarget = 0.5f;
								}
								headAudio.SetFloatParamValue("minDistance", 0.07f * uiSoundVolume.val);
								if (mMouthOpenTarget < 0.2f)
								{
									voiceOpenAdjust = 0.2f;
								}
								if (mMouthOpenTarget < 0.0f)
								{
									voiceOpenAdjust = 0.3f;
								}
								voiceMoan = true;
								if (Random.Range(0.0f,100.0f) > Mathf.Lerp(200, 50.0f, interestArousal/10.0f) && currentLook != "Feel" && currentLook != "Sex" && playerHeadToHead < personalSpaceDistance/2.0f && testRun == false && lookAction == false)
								{
									lookSM.Switch(lFeel);
								}
							}
							else
							{
								tempRandom = Mathf.Round(Random.Range(1.0f,4.0f));
								while (tempRandom == lastRandomVoice)
								{
									tempRandom = Mathf.Round(Random.Range(1.0f,4.0f));
								}
								audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_Out" + tempRandom + ".wav");
								headAudio.SetFloatParamValue("minDistance", 0.02f * uiSoundVolume.val);
								voicePuckerAdjust = 0.0f;
								voiceOpenAdjust = 0.0f;
								voiceMoan = false;
							}
						}
						headAudio.CallAction("PlayNow", audioClip);
						lastRandomVoice = tempRandom;
					}
                }
            }
			//SuperController.LogError("Breathing out start");
            if (breathState == "out" && breathHold <= 0.0f)
            {
                breathClock = Mathf.Max(breathClock - (Time.fixedDeltaTime * (breathingRate * breatheOutSpeed)), 0.0f);
                if (breathClock == 0.0f && (((AudioSourceControl)headAudio).playingClip == null || voiceMoan == false))
                {
                    breathState = "in";
                    breathCount += 1.0f;
					breathHold = Mathf.Lerp(0.3f,0.00f,Mathf.Clamp(interestArousal+3.0f,0.0f,10.0f)/10.0f) * uiBreatheSpeed.val;
					voiceOpenAdjust = 0.0f;
					//voicePuckerAdjust = 0.0f;
					mVisAATarget = 0.0f;
					mVisMTarget = 0.0f;
					mVisOWTarget = 0.0f;
					if (uiDoSounds.val && interestKissing == false)
					{
						headAudio.SetFloatParamValue("minDistance", 0.02f * uiSoundVolume.val);
						if ((mMouthOpenValue < 0.03f && mMouthOpenWideValue < 0.05f && mSmileFullFaceValue/2.0f < 0.07f && mSmileOpenFullFaceValue < 0.05f && mSmileSimpleLeftValue < 0.5f && mSmileSimpleRightValue < 0.5f && mExcitementValue < 0.05f && mTakingItValue < 0.1f) && interestArousal < 9.5f && currentLook != "Feel" && currentMouth != "Smile" && currentMouth != "Big Smile")
						{
							breathWMouth = false;
							if (interestValence < 6.0f)
							{
								audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Nose_In_Long" + Mathf.Round(Random.Range(1.0f,2.0f)) + ".wav");
							}
							else
							{
								audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Nose_In_Med" + Mathf.Round(Random.Range(1.0f,3.0f)) + ".wav");
							}
							voiceMoan = false;
						}
						else
						{
							breathWMouth = true;
							if (interestArousal > 7.0f)
							{
								audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_In_Fast" + Mathf.Round(Random.Range(1.0f,4.0f)) + ".wav");
							}
							else
							{
								audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_In" + Mathf.Round(Random.Range(1.0f,4.0f)) + ".wav");
							}
							voiceMoan = false;
						}
						headAudio.CallAction("PlayNow", audioClip);
					}
                }
            }
			if (breathHold > 0.0f)
			{
				breathHold -= Time.fixedDeltaTime;
			}

			tempFloat = Mathf.Lerp(0.003f, 0.01f, interestArousal/10.0f);
			if (breathState == "out" && voiceMoan == false && breathWMouth == true && currentMouth != "Smile" && currentMouth != "Big Smile" && currentMouth != "Kissing" && currentMouth != "Pout" && currentMouth != "LipBite")
			{
				if (breathClock > 0.2f)
				{
					if (breathClock < 0.8f)
					{
						mouthBreath = Mathf.Min(mouthBreath + tempFloat, Mathf.Lerp(0.02f, 0.05f, interestArousal/10.0f));
					}
				}
				else
				{
					mouthBreath = Mathf.Max(mouthBreath - tempFloat, 0.0f);
				}
			}
			else
			{
				mouthBreath = Mathf.Max(mouthBreath - tempFloat, 0.0f);
			}

			//SuperController.LogError("Morph Start");
			if (uiDoMorphs.val)
			{
				resetMorphs = false;
				if (personEyelids != null)
				{
					personEyelids.SetBoolParamValue("blinkEnabled", false);
				}
				else
				{
					personEyelids = containingAtom.GetStorableByID("EyelidControl");
					personEyelids.SetBoolParamValue("blinkEnabled", false);
				}
				containingAtom.GetStorableByID("AutoExpressions").SetBoolParamValue("enabled", false);
				
				if (tongueExpressionOffsetActual < tongueExpressionOffset)
				{
					tongueExpressionOffsetActual = Mathf.Max(tongueExpressionOffsetActual + 0.005f, tongueExpressionOffset);
				}
				if (tongueExpressionOffsetActual > tongueExpressionOffset)
				{
					tongueExpressionOffsetActual = Mathf.Min(tongueExpressionOffsetActual - 0.005f, tongueExpressionOffset);
				}
				
				if (uiTongueLength.val != 0.0f && morphTongueLength != null && uiControlTongue.val == true)
				{
					morphTongueLength.morphValue = uiTongueLength.val;
				}
				if (uiTongueRaise.val != 0.0f && morphTongueRaise != null && uiControlTongue.val == true)
				{
					morphTongueRaise.morphValue = uiTongueRaise.val + tongueExpressionOffsetActual;
				}
				tempFloat = Mathf.Lerp(0.5f,Mathf.Lerp(1.0f, 0.3f, pantSmooth / 10.0f),interestArousal/10.0f);
				if (expandSmooth < tempFloat)
				{
					expandSmooth = Mathf.Min(expandSmooth + 0.0025f, tempFloat);
				}
				if (expandSmooth > tempFloat)
				{
					expandSmooth = Mathf.Max(tempFloat, expandSmooth - 0.0025f);
				}
				
				if (morphRibCageSize != null)
				{
					morphRibCageSize.morphValue = Mathf.SmoothStep(0.0f, Mathf.Lerp(0.23f, 0.63f, breatheInSpeed / 10.0f) * uiBreatheExpandMultiplier.val, breathClock);
				}
				if (morphRibCageWidth != null)
				{
					morphRibCageWidth.morphValue = Mathf.SmoothStep(0.0f, Mathf.Lerp(0.03f, 0.83f*0.35f, breatheInSpeed / 10.0f) * uiBreatheExpandMultiplier.val * expandSmooth, breathClock);
				}
				if (morphChestHeight != null)
				{
					morphChestHeight.morphValue = Mathf.SmoothStep(mChestHeightOrig, mChestHeightOrig-( Mathf.Lerp(0.12f, 0.52f, breatheInSpeed / 10.0f) * uiBreatheRaiseMultiplier.val * expandSmooth),breathClock);//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f);
				}
				if (morphBreastHeight != null)
				{
					if (personIsMale)
					{
						morphBreastHeight.morphValue = Mathf.SmoothStep(-0.1f, 0.1f * uiBreatheRaiseMultiplier.val * expandSmooth,breathClock);//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f);
					}
					else
					{
						morphBreastHeight.morphValue = Mathf.SmoothStep(-0.05f, Mathf.Lerp(0.05f, 0.2f, breatheInSpeed / 10.0f) * uiBreatheRaiseMultiplier.val * expandSmooth,breathClock);//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f);
					}
				}
				if (morphBreath != null)
				{
					morphBreath.morphValue = Mathf.SmoothStep(0.0f, Mathf.Clamp(1.0f - (1.0f * uiBreatheExpandMultiplier.val * expandSmooth) * Mathf.Lerp(0.5f, 1.0f, breatheInSpeed / 10.0f),0.0f,1.0f),breathClock);//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f);
				}
				else
				{
					//morphRibsDef.morphValue = 0.3f + (breathClock*0.7f) * 1.22f * uiBreatheExpandMultiplier.val * tempFloat);//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f);
				}
				morphRibsDef.morphValue = Mathf.Max(0.2f + Round(Mathf.SmoothStep(-0.2f * uiBreatheExpandMultiplier.val * expandSmooth, Mathf.Lerp(0.22f, 0.42f, breatheInSpeed / 10.0f) * uiBreatheExpandMultiplier.val * expandSmooth,breathClock)), 0.0f);
				if (morphSternumDepth != null)
				{
					morphSternumDepth.morphValue = Round(Mathf.SmoothStep(-0.2f * uiBreatheExpandMultiplier.val * expandSmooth, Mathf.Lerp(0.22f, 0.42f, breatheInSpeed / 10.0f) * uiBreatheExpandMultiplier.val * expandSmooth,breathClock));
				}
				if (morphNipplesApply != null && uiDoSex.val)
				{
					morphNipplesApply.morphValue = Round(interestArousal/13.0f);
				}
				
				
				//SuperController.LogError("Breathing Morphs done");
				/*if (morphDeepBulgeBellyBottom != null && morphDeepBulgeBellyMid != null && usePerson2 && uiDoSex.val)
				{
					tempFloat = Vector3.Distance(pelvisController.followWhenOff.position, playerTip);
					if ( tempFloat < 0.15f)
					{
						morphDeepBulgeBellyBottom.morphValue = Mathf.Clamp((1.0f - (tempFloat*6.666f)) * uiSexAmount.val,0.0f,1.0f);
					}
					if ( tempFloat < 0.065f)
					{
						morphDeepBulgeBellyMid.morphValue = Mathf.Clamp((1.0f - (tempFloat*15.384f)) * uiSexAmount.val,0.0f,1.0f) * 0.75f;
					}
				}
				//SuperController.LogError("Bulge Morphs done");
				
				
				if (morphDeepThroat != null && usePerson2 && uiDoBlowjob.val)
				{
					tempFloat = Vector3.Distance(headController.followWhenOff.TransformPoint(new Vector3(0.0f, -0.03f, -0.05f)), playerTip);
					tempFloat2 = Mathf.Lerp(0.0f, 1.0f, 1.0f - Mathf.Clamp((tempFloat * 10.1f) - 0.1f, 0.0f, 1.0f));
					if (tempFloat > 0.15f)
					{
						tempFloat2 = 0.0f;
					}
						//SuperController.singleton.ClearErrors();
						//SuperController.LogError("Dist - " + Round(tempFloat) + "/ Morph " + Round(tempFloat2));
					if (tempFloat2 > throatBulgeAmount + 0.01f) { throatBulgeAmount = Mathf.Min(throatBulgeAmount + 0.02f, tempFloat2); }
					if (tempFloat2 < throatBulgeAmount - 0.01f) { throatBulgeAmount = Mathf.Max(throatBulgeAmount - 0.05f, tempFloat2); }
					morphDeepThroat.morphValue = Mathf.Clamp(throatBulgeAmount * uiBlowjobAmount.val,0.0f,0.7f);
				}*/
				//SuperController.LogError("Throat Morphs done");
				
				if (morphBlowjobLips != null && morphCheekSink != null && usePerson2 && playerTipToHead < playerHeadToHead && playerTipToHead < personalSpaceDistance/2.0f)
				{
					if ((lipsTouchCount > 2.0f || (playerTipToHead < 0.08f && lipsTouchCount > 0.0f)) && uiDoBlowjob.val)
					{
						if (Mathf.Abs(Vector3.Angle(playerTip, headController.followWhenOff.position)) < lookDirectAngle)
						{
							mMouthOpenWiderTarget = 0.34f;
						}
						else
						{
							mMouthOpenWiderTarget = 0.0f;
						}
						if (mainInterest == "Tip")
						{
							mainInterest = "Pelvis";
							//SuperController.LogError("Switching Main from Tip to Pelvis : Deepthroat");
						}
						tempFloat = Mathf.Clamp(Vector3.Distance(playerTip,playerTipPrev) * 9.0f,0.0f,1.0f);
						tempFloat2 = 0.0f - Mathf.Clamp(Vector3.Distance(headController.followWhenOff.position,prevPosHead) * 9.0f,0.0f,1.0f);
						if (tempFloat2 > tempFloat)
						{
							tempFloat = tempFloat2;
						}
						if (Vector3.Distance(personHeadTransform.position, playerTip) >= Vector3.Distance(personHeadTransform.position, playerTipPrev))// && Mathf.Abs(tempFloat) > 0.002f)
						{
							mBlowjobLipsTarget = Mathf.Clamp(mBlowjobLipsTarget + (tempFloat * uiBlowjobAmount.val),-0.2f,1.0f);
							mCheekSinkTarget = Mathf.Clamp(mBlowjobLipsTarget + 0.3f,0.0f,1.0f);
							if (mEyesClosedLeftValue < 0.0f && morphBlinking == false)
							{
								//mEyesClosedLeftTarget = Mathf.Clamp(mEyesClosedLeftTarget + 0.1f,-0.2f,0.0f);
								//mEyesClosedRightTarget = Mathf.Clamp(mEyesClosedRightTarget + 0.1f,-0.2f,0.0f);
							}
							mBrowUpTarget = Mathf.Clamp(mBrowUpTarget - 0.1f,0.0f,1.0f);
						}
						else
						{
							mBlowjobLipsTarget = Mathf.Clamp(mBlowjobLipsTarget - (tempFloat * uiBlowjobAmount.val * 2.0f),-0.2f,1.0f);
							mCheekSinkTarget = mBlowjobLipsTarget;
							mBrowUpTarget = Mathf.Clamp(mBrowUpTarget + 0.1f,0.0f,1.0f);
							if (mEyesClosedLeftValue < 0.3f && morphBlinking == false)
							{
								//mEyesClosedLeftTarget = Mathf.Clamp(mEyesClosedLeftTarget - 0.2f,-0.5f,1.0f);
								//mEyesClosedRightTarget = Mathf.Clamp(mEyesClosedRightTarget - 0.2f,-0.5f,1.0f);
							}
						}
					}
					else
					{
						mMouthOpenWiderTarget = 0.0f;
						mBlowjobLipsTarget = Mathf.Clamp(mBlowjobLipsTarget - 0.02f,0.0f,1.0f);
						mCheekSinkTarget = Mathf.Clamp(mCheekSinkTarget - 0.02f,0.0f,1.0f);
					}
					if (mBlowjobLipsTarget > mBlowjobLipsValue + 0.01f) { mBlowjobLipsValue = Mathf.Min(mBlowjobLipsValue + 0.07f, mBlowjobLipsTarget); }
					if (mBlowjobLipsTarget < mBlowjobLipsValue - 0.01f) { mBlowjobLipsValue = Mathf.Max(mBlowjobLipsValue - 0.07f, mBlowjobLipsTarget); }
					morphBlowjobLips.morphValue = Mathf.Clamp(mBlowjobLipsValue,0.0f,0.8f);
					if (mCheekSinkTarget > mCheekSinkValue + 0.01f) { mCheekSinkValue = Mathf.Min(mCheekSinkValue + 0.02f, mCheekSinkTarget); }
					if (mCheekSinkTarget < mCheekSinkValue - 0.01f) { mCheekSinkValue = Mathf.Max(mCheekSinkValue - 0.01f, mCheekSinkTarget); }
					morphCheekSink.morphValue = Mathf.Clamp(mCheekSinkValue*2.0f,0.0f,1.0f);
					playerTipPrev = playerTipController.followWhenOff.position;
					prevPosHead = headController.followWhenOff.position;
					//SuperController.LogError("Lips - " + Round(mBlowjobLipsValue) + "/ Cheeks" + Round(mCheekSinkValue));
				}
				//SuperController.LogError("Sex Morphs Done");
				
				//Morph Controller
				if (doHands)
				{
					if (mLHandStraightenTarget > mLHandStraightenValue) { mLHandStraightenValue = Mathf.Min(mLHandStraightenValue + (mLHandStraightenTarget - mLHandStraightenValue) / 15.0f, mLHandStraightenTarget); }
					if (mLHandStraightenTarget < mLHandStraightenValue) { mLHandStraightenValue = Mathf.Max(mLHandStraightenValue - (mLHandStraightenValue - mLHandStraightenTarget) / 15.0f, mLHandStraightenTarget); }
					morphLHandStraighten.morphValue = Mathf.Clamp(mLHandStraightenValue + (interestValence / 100.0f), 0.0f, 1.0f);
					if (mRHandStraightenTarget > mRHandStraightenValue) { mRHandStraightenValue = Mathf.Min(mRHandStraightenValue + (mRHandStraightenTarget - mRHandStraightenValue) / 15.0f, mRHandStraightenTarget); }
					if (mRHandStraightenTarget < mRHandStraightenValue) { mRHandStraightenValue = Mathf.Max(mRHandStraightenValue - (mRHandStraightenValue - mRHandStraightenTarget) / 15.0f, mRHandStraightenTarget); }
					morphRHandStraighten.morphValue = Mathf.Clamp(mRHandStraightenValue + (interestValence / 100.0f), 0.0f, 1.0f);
					if (mLHandFistTarget > mLHandFistValue) { mLHandFistValue = Mathf.Min(mLHandFistValue + (mLHandFistTarget - mLHandFistValue) / 15.0f, mLHandFistTarget); }
					if (mLHandFistTarget < mLHandFistValue) { mLHandFistValue = Mathf.Max(mLHandFistValue - (mLHandFistValue - mLHandFistTarget) / 15.0f, mLHandFistTarget); }
					morphLHandFist.morphValue = Mathf.Clamp(mLHandFistValue + (interestArousal / 100.0f), 0.0f, 1.2f);
					if (mRHandFistTarget > mRHandFistValue) { mRHandFistValue = Mathf.Min(mRHandFistValue + (mRHandFistTarget - mRHandFistValue) / 15.0f, mRHandFistTarget); }
					if (mRHandFistTarget < mRHandFistValue) { mRHandFistValue = Mathf.Max(mRHandFistValue - (mRHandFistValue - mRHandFistTarget) / 15.0f, mRHandFistTarget); }
					morphRHandFist.morphValue = Mathf.Clamp(mRHandFistValue + (interestArousal / 100.0f), 0.0f, 1.2f);
					//SuperController.LogError("Hand Morphs Done");
				}

				//tempFloat = -0.2f + (Mathf.Max(mSmileFullFaceValue,mSmileOpenFullFaceValue) + mTakingItValue) * 1.0f;
				tempFloat = 0.0f;
				if (mBrowUpTarget + (interestValence / 30.0f) + tempFloat > mBrowUpValue + 0.05f) { mBrowUpValue = Mathf.Min(mBrowUpValue + ((Mathf.Abs(mBrowUpTarget - mBrowUpValue - tempFloat) / 20.0f) * browVariation * morphSpeed), mBrowUpTarget + (interestValence / 30.0f) + tempFloat); }
				if (mBrowUpTarget + (interestValence / 30.0f) + tempFloat < mBrowUpValue - 0.05f) { mBrowUpValue = Mathf.Max(mBrowUpValue - ((Mathf.Abs(mBrowUpTarget - mBrowUpValue - tempFloat) / 85.0f) * browVariation * morphSpeed), mBrowUpTarget + (interestValence / 30.0f) + tempFloat); }
				if (morphBrowUp != null)
				{
					morphBrowUp.morphValue = Round(Mathf.Clamp(mBrowUpValue, 0.0f, 1.0f));
				}
				if (mBrowDownTarget > mBrowDownValue + 0.02f) { mBrowDownValue = Mathf.Min(mBrowDownValue + ((Mathf.Abs(mBrowDownTarget - mBrowDownValue) / 65.0f) * browVariation * morphSpeed), mBrowDownTarget); }
				if (mBrowDownTarget < mBrowDownValue - 0.02f) { mBrowDownValue = Mathf.Max(mBrowDownValue - ((Mathf.Abs(mBrowDownTarget - mBrowDownValue) / 10.0f) * browVariation * morphSpeed), mBrowDownTarget); }
				if (morphBrowDown != null)
				{
					morphBrowDown.morphValue = Mathf.SmoothStep(0.0f,1.0f,Round(mBrowDownValue));
				}

				//tempFloat = -0.2f + (Mathf.Max(mSmileFullFaceValue,mSmileOpenFullFaceValue) + mSmileSimpleLeftValue) * 1.5f;
				if (mBrowOuterUpLeftTarget + tempFloat > mBrowOuterUpLeftValue + 0.01f) { mBrowOuterUpLeftValue = Mathf.Min(mBrowOuterUpLeftValue + ((Mathf.Abs(mBrowOuterUpLeftTarget - mBrowOuterUpLeftValue - tempFloat) / 15.0f) * browVariation * morphSpeed), mBrowOuterUpLeftTarget + tempFloat); }
				if (mBrowOuterUpLeftTarget + tempFloat < mBrowOuterUpLeftValue - 0.01f) { mBrowOuterUpLeftValue = Mathf.Max(mBrowOuterUpLeftValue - ((Mathf.Abs(mBrowOuterUpLeftTarget - mBrowOuterUpLeftValue - tempFloat) / 65.0f) * browVariation * morphSpeed), mBrowOuterUpLeftTarget + tempFloat); }
				if (morphBrowOuterUpLeft != null)
				{
					morphBrowOuterUpLeft.morphValue = Round(mBrowOuterUpLeftValue);
				}

				//tempFloat = -0.2f + (Mathf.Max(mSmileFullFaceValue,mSmileOpenFullFaceValue) + mSmileSimpleRightValue) * 1.5f;
				if (mBrowOuterUpRightTarget + tempFloat > mBrowOuterUpRightValue + 0.01f) { mBrowOuterUpRightValue = Mathf.Min(mBrowOuterUpRightValue + ((Mathf.Abs(mBrowOuterUpRightTarget - mBrowOuterUpRightValue - tempFloat) / 15.0f) * browVariation * morphSpeed), mBrowOuterUpRightTarget + tempFloat); }
				if (mBrowOuterUpRightTarget + tempFloat < mBrowOuterUpRightValue - 0.01f) { mBrowOuterUpRightValue = Mathf.Max(mBrowOuterUpRightValue - ((Mathf.Abs(mBrowOuterUpRightTarget - mBrowOuterUpRightValue - tempFloat) / 65.0f) * browVariation * morphSpeed), mBrowOuterUpRightTarget + tempFloat); }
				if (morphBrowOuterUpRight != null)
				{
					morphBrowOuterUpRight.morphValue = Round(mBrowOuterUpRightValue);
				}
				
				if (mBrowCenterUpTarget + (interestArousal / 20.0f) > mBrowCenterUpValue + 0.01f) { mBrowCenterUpValue = Mathf.Min(mBrowCenterUpValue + ((Mathf.Abs(mBrowCenterUpTarget - mBrowCenterUpValue) / 125.0f) * browVariation * morphSpeed), mBrowCenterUpTarget + (interestArousal / 20.0f)); }
				if (mBrowCenterUpTarget + (interestArousal / 20.0f) < mBrowCenterUpValue - 0.01f) { mBrowCenterUpValue = Mathf.Max(mBrowCenterUpValue - ((Mathf.Abs(mBrowCenterUpTarget - mBrowCenterUpValue) / 35.0f) * browVariation * morphSpeed), mBrowCenterUpTarget + (interestArousal / 20.0f)); }
				if (morphBrowCenterUp != null)
				{
					morphBrowCenterUp.morphValue = Round(Mathf.Clamp(mBrowCenterUpValue, -0.3f, 1.3f));
				}
				//SuperController.LogError("Brow Morphs Done");

				tempFloat = Mathf.Max(0.0f, mSmileSimpleLeftValue / 2.0f, mSmileSimpleRightValue / 2.0f) + mSmileFullFaceValue + (mSmileOpenFullFaceValue / 1.5f) + mHappyValue;
				tempFloat = -0.25f + ((Mathf.Clamp((tempFloat / 1.5f) - mEyesSquintTarget, 0.0f, 1.0f)) / 2.0f);
				if (mEyesSquintTarget + tempFloat > mEyesSquintValue + 0.005) { mEyesSquintValue = Mathf.Min(mEyesSquintValue + (0.04f * eyeVariation * (morphSpeed / 3.0f)), mEyesSquintTarget + tempFloat); }
				if (mEyesSquintTarget + tempFloat < mEyesSquintValue - 0.005) { mEyesSquintValue = Mathf.Max(mEyesSquintValue - (0.01f * eyeVariation * (morphSpeed / 3.0f)), mEyesSquintTarget + tempFloat); }
				if (morphEyesSquint != null)
				{
					morphEyesSquint.morphValue = Round(Mathf.Clamp(mEyesSquintValue - mSmileFullFaceValue, -0.1f, 1.0f) * 10.0f)/10.0f;
				}

				float lidLower = 0.25f;
				float lidRaise = 0.4f;
				//tempFloat = Random.Range(-0.05f, 0.05f);
				//tempFloat = eyeCloseMaxMorph - Mathf.Lerp(0.0f,0.1f,mEyesSquintValue) - Mathf.Lerp(0.0f,0.05f,mTakingItValue) - Mathf.Lerp(0.0f,0.1f,mDeserveItValue) - Mathf.Lerp(0.0f,0.1f,mSmileOpenFullFaceValue);//Mathf.Clamp(1.25f - Mathf.Lerp(0.0f,0.3f,mEyesSquintValue),0.0f, eyeCloseMaxMorph);
				tempFloat = eyeCloseMaxMorph;
				tempFloat2 = 0.0f;
				if (amGlancing)
				{
					tempFloat2 = -0.1f;
				}
				if ((currentEye != "Closed" && currentEye != "Blink" && mEyesClosedLeftTarget > 0.23f) || amGlancing)
				{
					//mEyesClosedLeftTarget = 0.0f;
					//mEyesClosedRightTarget = 0.0f;
				}
				if (morphEyesClosedLeft != null && morphEyesClosedRight != null)
				{
					if (morphBlinking)
					{
						lidLower = 0.53f + Random.Range(-0.05f, 0.05f);
						lidRaise = 0.1f + Random.Range(-0.05f, 0.05f);
					}
					else
					{
						if (interestKissing)
						{
							lidLower = lidLower / 20.0f;
							lidRaise = lidRaise / 40.0f;
						}
						if (currentEye == "Squint" || currentEye == "Focus")
						{
							lidLower = lidLower / 80.0f;
							lidRaise = lidRaise / 120.0f;
						}
						//if (mEyesClosedLeftTarget + tempFloat2 > mEyesClosedLeftValue) { mEyesClosedLeftValue = Mathf.Min(mEyesClosedLeftValue + (lidLower * eyeVariation * (morphSpeed / 5.0f)), mEyesClosedLeftTarget); }
						//if (mEyesClosedLeftTarget + tempFloat2 < mEyesClosedLeftValue) { mEyesClosedLeftValue = Mathf.Max(mEyesClosedLeftValue - (lidRaise * eyeVariation * (morphSpeed / 5.0f)), mEyesClosedLeftTarget); }
						//morphEyesClosedLeft.morphValue = Round(Mathf.Clamp(mEyesClosedLeftValue, -0.5f, tempFloat));
						//if (mEyesClosedRightTarget + tempFloat2 > mEyesClosedRightValue) { mEyesClosedRightValue = Mathf.Min(mEyesClosedRightValue + (lidLower * eyeVariation * (morphSpeed / 5.0f)), mEyesClosedRightTarget); }
						//if (mEyesClosedRightTarget + tempFloat2 < mEyesClosedRightValue) { mEyesClosedRightValue = Mathf.Max(mEyesClosedRightValue - (lidRaise * eyeVariation * (morphSpeed / 5.0f)), mEyesClosedRightTarget); }
						//morphEyesClosedRight.morphValue = Round(Mathf.Clamp(mEyesClosedRightValue, -0.5f, tempFloat));
					}
					//SuperController.singleton.ClearErrors();
						tempFloat = 0.0f; //Mathf.Clamp(Round(Mathf.Clamp(mEyesSquintValue - mSmileFullFaceValue, -0.3f, 1.0f)), -1.0f, 0.0f);
						if (mEyesClosedLeftTarget > mEyesClosedLeftValue) { mEyesClosedLeftValue = Mathf.Min(mEyesClosedLeftValue + lidLower, mEyesClosedLeftTarget); }
						if (mEyesClosedLeftTarget < mEyesClosedLeftValue) { mEyesClosedLeftValue = Mathf.Max(mEyesClosedLeftValue - lidRaise, mEyesClosedLeftTarget); }
						morphEyesClosedLeft.morphValue = Mathf.Clamp(Round(mEyesClosedLeftValue*10.0f)/10.0f, uiEyeOpenMaxMorph.val, uiEyeCloseMaxMorph.val);
						//SuperController.LogError(Round(mEyesClosedLeftValue).ToString(), false);
						if (mEyesClosedRightTarget > mEyesClosedRightValue) { mEyesClosedRightValue = Mathf.Min(mEyesClosedRightValue + lidLower, mEyesClosedRightTarget); }
						if (mEyesClosedRightTarget < mEyesClosedRightValue) { mEyesClosedRightValue = Mathf.Max(mEyesClosedRightValue - lidRaise, mEyesClosedRightTarget); }
						morphEyesClosedRight.morphValue = Mathf.Clamp(Round(mEyesClosedRightValue*10.0f)/10.0f, uiEyeOpenMaxMorph.val, uiEyeCloseMaxMorph.val);

            
				}

				if (morphBlinking && mEyesClosedRightValue <= 0.1f && mEyesClosedLeftValue <= 0.1f)
				{
					morphBlinking = false;
				}

				tempFloat = Mathf.Lerp(0.0f,0.15f, interestArousal / 10.0f) * (breathClock) * uiBreatheExpandMultiplier.val;
				mNoseFlareTarget = 0.2f;
				if (breathState == "in")
				{
					tempFloat = (((interestArousal / 20.0f) * Mathf.Lerp(1.0f,0.0f,breathClock)) + Mathf.Lerp(0.0f,0.2f,((interestArousal+interestValence)/2.0f)/10.0f)) * uiBreatheExpandMultiplier.val;
					mNoseFlareTarget = -0.5f;
				}
				//tempFloat = Mathf.Clamp(tempFloat + (0.2f * (interestArousal/10.0f)),0.0f,1.0f) * uiBreatheExpandMultiplier.val;
				//SuperController.LogError("Eye Morphs Done");
				tempFloat2 = mouthVariation;
				if (mainInterest != "Face")
				{
					//mMouthOpenWideTarget = (Mathf.Clamp(interestArousal-6.0f,0.0f,10.0f)/10.0f)/2.0f;
				}
				else
				{
					//mMouthOpenWideTarget = 0.0f;
				}
				if (currentMouth == "Smile" || currentMouth == "Big Smile" || currentMouth == "Kissing" || currentMouth == "Pout" || currentMouth == "LipBite")
				{
					tempFloat = Mathf.Lerp(0.0f,Mathf.Lerp(0.01f, 0.05f,interestArousal/10.0f),(breathClock)) * uiBreatheExpandMultiplier.val;
					tempFloat2 = 0.25f;
					mNoseFlareTarget = Mathf.Clamp(mNoseFlareTarget * 5.0f,0.0f,0.5f);
					mMouthOpenWideTarget = 0.0f;
				}
				
				if (usePerson2 && person2Usable && currentLook == "Sex" && playerTipToPelvis < interactionDistance)
				{
					tempFloat = Mathf.Clamp(Vector3.Distance(playerTip,playerTipPrev) * 7.0f,0.0f,1.0f);
					if (tempFloat > 0.05f)
					{
						mMouthOpenWiderTarget = Mathf.Clamp(mMouthOpenWiderTarget + (tempFloat / 1.0f),0.0f,0.8f);
					}
				}
				if (mouthCanOpen == false || currentMouth == "Closed")
				{
					tempFloat = 0.0f;
				}
				
				if (morphEyesPupils != null && uiPupilDialation.val > 0.0f)
				{
					if (mEyesPupilsTarget > mEyesPupilsValue + 0.01f) { mEyesPupilsValue = Mathf.Min(mEyesPupilsValue + (0.01f * uiPupilRate.val), mEyesPupilsTarget); }
					if (mEyesPupilsTarget < mEyesPupilsValue - 0.01f) { mEyesPupilsValue = Mathf.Max(mEyesPupilsValue - (0.03f * uiPupilRate.val), mEyesPupilsTarget); }
					morphEyesPupils.morphValue = Round(mEyesPupilsValue);
				}

				
				if (morphMouthMouthOpen != null)
				{
					//SuperController.singleton.ClearMessages();
					//SuperController.LogMessage("Valence " + Round(interestValence) + "Arousal " + Round(interestArousal) + "Look " + currentLook, false);
					tempFloat = Mathf.Lerp(0.00f, 0.04f + Mathf.Lerp(0.0f, 0.02f, Mathf.Clamp(pantSmooth/10.0f, 0.0f, 1.0f)), interestArousal/10.0f);
					tempFloat = tempFloat - (mSmileFullFaceValue/2.0f) - Mathf.Max(mSmileOpenFullFaceValue,0.0f) - (Mathf.Max(0.0f, mSmileSimpleLeftValue, mSmileSimpleRightValue) / 3.0f) - mLipsPartValue - (mLipBiteTarget*2.0f) - mHappyValue;
					//SuperController.LogMessage(" |mSmileFullFaceValue " + Round((mSmileFullFaceValue/2.0f)) + " |mSmileOpenFullFaceValue " + Round(Mathf.Max(mSmileOpenFullFaceValue,0.0f)) + " |mSmileSimpleLeftRightValue " + Round((Mathf.Max(0.0f, mSmileSimpleLeftValue, mSmileSimpleRightValue) / 3.0f) ), false);
					//SuperController.LogMessage(" |mLipsPartValue " + Round(mLipsPartValue) + " |mLipBiteValue " + Round(mLipBiteValue*2.0f) + " |mHappyValue " + Round(mHappyValue) + " |mExcitementValue " + Round(mExcitementValue) + " |mLipBiteValue " + Round(mLipBiteValue) + " |mTakingItValue " + Round(mTakingItValue), false);
					if (currentMouth != "Big Smile" && currentMouth != "Pout" && currentMouth != "Smile")// && currentMouth != "Closed")
					{
						//mMouthOpenTarget = mMouthOpenTarget / 2.0f;
						if (mouthCanOpen == false)
						{
						//tempFloat = 0.0f - Mathf.Lerp(0.2f,0.0f,mouthOpenTimer);
						}
					}
					else
					{
						if (currentMouth == "Closed" || currentMouth == "Demure" || currentMouth == "LipBite" || currentMouth == "Pout")
						{
							//mMouthOpenTarget = 0.0f;
							tempFloat = 0.0f;
							//SuperController.LogMessage("Mouth closed demure bite or pout");
							//voiceOpenAdjust = 0.0f;
						}
						else
						{
							//mMouthOpenTarget = Mathf.Lerp(-0.2f,0.4f,Mathf.Max((interestArousal/10.0f)-0.5f,0.0f));
						}
					}
					if (currentMouth == "Open")
					{
						tempFloat = 0.0f; //
						//SuperController.LogMessage("Mouth open");
					}
					if (interestKissing)
					{
						tempFloat2 = 0.01f + Mathf.Abs(Mathf.Clamp(mMouthOpenTarget + tempFloat + voiceOpenAdjust, 0.0f, 0.4f) - mMouthOpenValue) * 5.0f;
					}
					else
					{
						//mMouthOpenTarget = Mathf.Clamp(mMouthOpenTarget, -0.15f, 0.6f);
						tempFloat2 = 0.001f + Mathf.Abs(Mathf.Clamp(mMouthOpenTarget + tempFloat + voiceOpenAdjust, 0.0f, 0.4f) - mMouthOpenValue) * 0.1f;
					}
					if ((mSmileFullFaceValue/2.0f) > 0.15f || mSmileOpenFullFaceValue > 0.05f || mSmileSimpleLeftValue > 0.4f || mSmileSimpleRightValue > 0.4f || mExcitementValue > 0.05f || mLipBiteValue > 0.1f || mTakingItValue > 0.1f)
					{
						tempFloat = -0.05f;
						//SuperController.LogMessage("Mouth smiling");
					}
					if (mHappyTarget > 0.15f)
					{
						tempFloat = -0.2f;
						//SuperController.LogMessage("Mouth happy");
					}
					
					if (mSmileOpenFullFaceTarget > 0.1f)
					{
						tempFloat = Mathf.Lerp(0.0f, -0.1f, mSmileOpenFullFaceValue);
						//SuperController.LogMessage("Mouth full smile");
					}

					if (mouthCanOpen)
					{
						//mMouthOpenTarget = Mathf.Lerp(mMouthOpenTarget, mMouthOpenValue+(0.3f * (interestArousal/10.0f)), 1.0f-breathClock);//Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mMouthOpenValue / 2.0f, 0.0f, 0.5f)));
					}
					else
					{
						//mMouthOpenTarget = mMouthOpenTarget;//Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mMouthOpenValue / 2.0f, 0.0f, 0.5f)));
					}

					if (mouthCanOpen == false || currentMouth == "Closed" || mMouthOpenTarget > 0.05)
					{
						tempFloat = -0.01f;
						//SuperController.LogMessage("Mouth can open false or closed set or already open");
					}
					if (tempFloat == 0.0f)
					{
						tempFloat = Mathf.Lerp(0.0f, 0.02f, interestArousal/10.0f);
					}

					//tempFloat = 0.0f;
					if (Mathf.Clamp(mMouthOpenTarget + tempFloat + voiceOpenAdjust, 0.0f, 0.4f) > mMouthOpenValue + 0.005f) { mMouthOpenValue = Mathf.Min(mMouthOpenValue + Mathf.Clamp((tempFloat2 / 30.0f) * mouthVariation * morphSpeed, 0.0005f, 0.1f), Mathf.Clamp(mMouthOpenTarget + tempFloat + voiceOpenAdjust, 0.0f, 0.4f));}
					if (Mathf.Clamp(mMouthOpenTarget + tempFloat + voiceOpenAdjust, 0.0f, 0.4f) < mMouthOpenValue - 0.005f) { mMouthOpenValue = Mathf.Max(mMouthOpenValue - Mathf.Clamp((tempFloat2 / 1.0f) * mouthVariation * morphSpeed, 0.0005f, 0.1f), Mathf.Clamp(mMouthOpenTarget + tempFloat + voiceOpenAdjust, 0.0f, 0.4f));}
					mMouthOpenValue = Mathf.Clamp(mMouthOpenValue, 0.0f, 0.4f);
					//SuperController.LogMessage("Target : " + Mathf.Clamp(mMouthOpenTarget + tempFloat + voiceOpenAdjust, 0.0f, 0.4f) + "| Value " + mMouthOpenValue + "| Adjust " + Round(tempFloat) + "| Change " + Round(tempFloat2) + "| Voice" + Round(voiceOpenAdjust) + "|Anim " + Round(tempFloat2), false);
					if (uiControlJaw.val == true)
					{
						if (personIsMale)
						{
							morphMouthMouthOpen.morphValue = (Round((mMouthOpenValue/ 5.0f) * 10.0f) / 10.0f) + uiMouthOpenOffset.val;//Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mMouthOpenValue / 2.0f, 0.0f, 0.5f)));
						}
						else
						{
							//tempFloat = mMouthOpenValue * 1.0f;
							if (mSmileFullFaceTarget > 0.05f || mSmileOpenFullFaceTarget > 0.05f || mSmileSimpleLeftTarget > 0.1f || mSmileSimpleRightTarget > 0.1f || mExcitementTarget > 0.05f || mLipBiteTarget > 0.3f || mTakingItTarget > 0.2f)
							{
								//tempFloat = 0.0f;
							}
							morphMouthMouthOpen.morphValue = (Round(mMouthOpenValue * 10.0f) / 10.0f) + uiMouthOpenOffset.val;
						}
					}
				}
        
				if (morphNoseFlare != null)
				{
					if (mNoseFlareTarget > mNoseFlareValue + 0.01f) { mNoseFlareValue = Mathf.Min(mNoseFlareValue + (0.05f * mouthVariation * morphSpeed), mNoseFlareTarget); }
					if (mNoseFlareTarget < mNoseFlareValue - 0.01f) { mNoseFlareValue = Mathf.Max(mNoseFlareValue - (0.005f * mouthVariation * morphSpeed), mNoseFlareTarget); }
					//morphNoseFlare.morphValue = Round(Mathf.Clamp(mNoseFlareValue, -1.0f, 1.0f));
				}
				

				tempFloat2 = 1.0f;
				if (currentLook == "Sex")
				{
					tempFloat2 = 10.0f;
				}				
				if (morphMouthMouthOpenWide != null && morphMouthNarrow != null && uiControlJaw.val == true)
				{
					if (mouthCanOpen == false)
					{
						mMouthOpenWideTarget = 0.0f;
					}
					if (mMouthOpenWideTarget + voiceOpenAdjust > mMouthOpenWideValue + 0.01f) { mMouthOpenWideValue = Mathf.Min(mMouthOpenWideValue + (0.002f * tempFloat2 * morphSpeed), mMouthOpenWideTarget + voiceOpenAdjust); }
					if (mMouthOpenWideTarget + voiceOpenAdjust < mMouthOpenWideValue - 0.01f) { mMouthOpenWideValue = Mathf.Max(mMouthOpenWideValue - (0.0005f * tempFloat2 * morphSpeed), mMouthOpenWideTarget + voiceOpenAdjust); }
					morphMouthMouthOpenWide.morphValue = Mathf.SmoothStep(0.0f,0.7f,Round(mMouthOpenWideValue));
					if (currentMouth == "Open")
					{
						mMouthNarrowTarget = Mathf.Clamp((mMouthOpenWideValue* 0.55f) + (mMouthOpenValue * 3.0f),0.0f,1.0f);
					}
					else
					{
						mMouthNarrowTarget = Mathf.Clamp((mMouthOpenWideValue* 0.55f) + (mMouthOpenValue * 2.0f),0.0f,1.0f);
					}
					if (mouthCanOpen == false)
					{
						mMouthNarrowTarget = 0.0f;
					}
					if (mMouthNarrowTarget > mMouthNarrowValue + 0.01f) { mMouthNarrowValue = Mathf.Min(mMouthNarrowValue + (0.005f * morphSpeed), mMouthNarrowTarget); }
					if (mMouthNarrowTarget < mMouthNarrowValue - 0.01f) { mMouthNarrowValue = Mathf.Max(mMouthNarrowValue - (0.047f * morphSpeed), mMouthNarrowTarget); }
					morphMouthNarrow.morphValue = mMouthOpenValue * 2.0f; //Round(mMouthNarrowValue * 10.0f) / 10.0f; //Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mMouthNarrowValue,0.0f,1.0f)));
				}
				
				if (morphMouthOpenWider != null && usePerson2 && person2Usable && uiControlJaw.val == true)
				{
					if ((mainInterest == "Tip" || mainInterest == "Pelvis") && Vector3.Distance(playerTip,headController.followWhenOff.position) < closeFaceDistance * 1.3f && uiDoBlowjob.val)
					{
						mMouthOpenWiderTarget = 0.44f;
						mLipsCloseTarget = 1.1f;
					}
					else
					{
						mMouthOpenWiderTarget = 0.0f;
						if (currentLook != "Kissing" && voiceMoan == false)
						{
							mLipsCloseTarget = 0.0f ;
						}
						if (lipsTouchCount <= 1.0f && voiceMoan == false)
						{
							mLipsCloseTarget = 0.0f;
						}					
						if (mouthCanOpen == false)
						{
							mMouthOpenWiderTarget = 0.0f;
						}
					}
					if (mMouthOpenWiderTarget > mMouthOpenWiderValue + 0.01f) { mMouthOpenWiderValue = Mathf.Min(mMouthOpenWiderValue + (0.02f * tempFloat2 * morphSpeed), mMouthOpenWiderTarget); }
					if (mMouthOpenWiderTarget < mMouthOpenWiderValue - 0.01f) { mMouthOpenWiderValue = Mathf.Max(mMouthOpenWiderValue - (0.005f * tempFloat2 * morphSpeed), mMouthOpenWiderTarget); }
					morphMouthOpenWider.morphValue = Mathf.SmoothStep(0.0f,1.0f,Round(mMouthOpenWiderValue));
				}

				if (morphLipsLipsPart != null && morphLipsPouty != null)
				{
					if (currentMouth != "Open" && currentMouth != "Demure")
					{
					mLipsPartTarget = Mathf.Clamp(0.0f + ((interestArousal- 3.0f)/30.0f) - (Mathf.Max(mSmileSimpleLeftValue / 1.5f,mSmileSimpleRightValue / 1.5f, mSmileOpenFullFaceValue * 2.0f, mSmileFullFaceValue)) - mHappyValue - mLipsPuckerValue - mMouthOpenValue - (mTakingItValue * 2.0f) - mLipsBottomDownValue,0.0f,1.0f);
					mLipsPoutyTarget = Mathf.Round(Mathf.Clamp(mLipsPartValue,0.0f,1.0f) * 50.0f) / 50.0f;
					}
					else
					{
						mLipsPoutyTarget = 0.0f;
					}
					if (mLipsPartTarget > mLipsPartValue + 0.01f) { mLipsPartValue = Mathf.Min(mLipsPartValue + (0.002f * morphSpeed), mLipsPartTarget); }
					if (mLipsPartTarget < mLipsPartValue - 0.01f) { mLipsPartValue = Mathf.Max(mLipsPartValue - (0.01f * morphSpeed), mLipsPartTarget); }
					morphLipsLipsPart.morphValue = Mathf.Round(Mathf.Clamp(mLipsPartValue,0.0f,1.0f) * 50.0f) / 50.0f;
					if (mLipsPoutyTarget > mLipsPoutyValue + 0.01f) { mLipsPoutyValue = Mathf.Min(mLipsPoutyValue + (0.002f * morphSpeed), mLipsPoutyTarget); }
					if (mLipsPoutyTarget < mLipsPoutyValue - 0.01f) { mLipsPoutyValue = Mathf.Max(mLipsPoutyValue - (0.01f * morphSpeed), mLipsPoutyTarget); }
					morphLipsPouty.morphValue = mLipsPoutyValue;
				}
				if (morphLipsLipsPartCenter != null)
				{
					if (mLipsCenterPartTarget > mLipsCenterPartValue + 0.01f) { mLipsCenterPartValue = Mathf.Min(mLipsCenterPartValue + (0.002f * morphSpeed), mLipsCenterPartTarget); }
					if (mLipsCenterPartTarget < mLipsCenterPartValue - 0.01f) { mLipsCenterPartValue = Mathf.Max(mLipsCenterPartValue - (0.05f * morphSpeed), mLipsCenterPartTarget); }
					morphLipsLipsPartCenter.morphValue = Mathf.Round(Mathf.Clamp(mLipsCenterPartValue,0.0f,1.0f) * 50.0f) / 50.0f;
				}
				//
				if (morphMouthSmileMuscle != null)
				{
					mSmileMuscleTarget = Mathf.Clamp(Mathf.Max(mSmileSimpleLeftValue * 2.0f,mSmileSimpleRightValue * 2.0f, mSmileOpenFullFaceValue * 1.0f, mSmileFullFaceValue * 1.5f, 0.0f), 0.0f, 0.3f) * uiMaxMorphSmile.val;
					if (mSmileMuscleTarget > mSmileMuscleValue + 0.01f) { mSmileMuscleValue = Mathf.Min(mSmileMuscleValue + (0.002f * morphSpeed), mSmileMuscleTarget); }
					if (mSmileMuscleTarget < mSmileMuscleValue - 0.01f) { mSmileMuscleValue = Mathf.Max(mSmileMuscleValue - (0.05f * morphSpeed), mSmileMuscleTarget); }
					morphMouthSmileMuscle.morphValue = Mathf.Round(Mathf.Clamp(mSmileMuscleValue,0.0f,1.0f) * 10.0f) / 10.0f;
				}

				if (mSmileFullFaceValue > 0.5f || mSmileOpenFullFaceValue > 0.5f || mSmileSimpleLeftValue > 0.7f || mSmileSimpleRightValue > 0.7f || mExcitementValue > 0.5f)
				{
					smileDamperTimeout += Time.fixedDeltaTime;
					if (smileDamperTimeout > 1.0f)
					{
						smileDamper = Mathf.Clamp(smileDamper + 0.001f, 0.0f, 1.0f);
					}
					//SuperController.LogError("Damping Smile " + smileDamperTimeout.ToString());
				}
				else
				{
					smileDamperTimeout -= Time.fixedDeltaTime;
					if (smileDamperTimeout < 0.0f)
					{
						smileDamperTimeout = 0.0f;
						smileDamper = Mathf.Clamp(smileDamper - 0.003f, 0.0f, 1.0f);
					}
					//SuperController.LogError("Allowing Smile " + smileDamperTimeout.ToString());
				}
				if (morphMouthCornerUpDown != null)
				{
					mMouthCornerUpDownTarget = 0.0f - Mathf.Max(mSmileSimpleLeftValue * 2.0f,mSmileSimpleRightValue * 2.0f, mSmileOpenFullFaceValue * 0.5f, mSmileFullFaceValue * 1.5f, mExcitementValue * 2.0f) + uiSmileOffset.val;
					mMouthCornerUpDownTarget = mMouthCornerUpDownTarget * uiSmileDamper.val;
					if (mMouthCornerUpDownTarget > mMouthCornerUpDownValue + 0.01f) { mMouthCornerUpDownValue = Mathf.Min(mMouthCornerUpDownValue + (0.05f * morphSpeed), mMouthCornerUpDownTarget); }
					if (mMouthCornerUpDownTarget < mMouthCornerUpDownValue - 0.01f) { mMouthCornerUpDownValue = Mathf.Max(mMouthCornerUpDownValue - (0.05f * morphSpeed), mMouthCornerUpDownTarget); }
					morphMouthCornerUpDown.morphValue = Round(Mathf.Lerp(Mathf.Clamp(mMouthCornerUpDownValue,-1.0f,1.0f), 0.0f, smileDamper));
				}

				if (morphLipsBottomDown != null)
				{
					mLipsBottomDownValue = (Round(Mathf.Clamp(Mathf.Max(mSmileSimpleLeftValue / 2.0f,mSmileSimpleRightValue / 2.0f, mSmileFullFaceValue / 2.0f) - Mathf.Max(mMouthOpenValue,0.0f),0.0f,0.2f) * 50.0f) / 50.0f)  * uiMaxMorphSmile.val;
					//morphLipsBottomDown.morphValue = mLipsBottomDownValue;
				}
				
				if (morphLipsLipsClose != null)
				{
					if (currentMouth == "Demure")
					{
						//mLipsCloseTarget = -0.4f;
					}
					if (currentMouth == "Pout") //currentMouth == "LipBite" || 
					{
						mLipsCloseTarget = 0.2f;
					}
					tempFloat = 0.0f - (mSmileFullFaceValue/10.0f);
					if (currentMouth == "Idle" || currentMouth == "Idle(S)" || currentMouth == "Open" || currentMouth == "Closed")
					{
						tempFloat = (mLipsPartValue / 2.0f) - (mSmileFullFaceValue/10.0f);
					}
					if (mLipsCloseTarget - tempFloat > mLipsCloseValue + 0.005f) { mLipsCloseValue = Mathf.Min(mLipsCloseValue + ((Mathf.Abs(mLipsCloseTarget - mLipsCloseValue) / 55.0f) * morphSpeed), mLipsCloseTarget - tempFloat); }
					if (mLipsCloseTarget - tempFloat < mLipsCloseValue - 0.005f) { mLipsCloseValue = Mathf.Max(mLipsCloseValue - ((Mathf.Abs(mLipsCloseTarget - mLipsCloseValue) / 25.0f) * morphSpeed), mLipsCloseTarget - tempFloat); }
					morphLipsLipsClose.morphValue = Round(Mathf.Clamp(mLipsCloseValue,-0.2f,1.0f) + uiLipsCloseOffset.val);
				}
				if (morphLipsLipsPucker != null && morphLipsLipsPuckerWide != null)
				{
					tempFloat = Mathf.Abs(mLipsPuckerTarget - mLipsPuckerValue);
					if (interestKissing || lipsOnly)
					{
						tempFloat = Mathf.Abs(mLipsPuckerTarget - mLipsPuckerValue) * 1.0f;
					}
//					if (mLipsPuckerTarget + mLipsPoutyValue + voicePuckerAdjust > mLipsPuckerValue + 0.01f) { mLipsPuckerValue = Mathf.Min(mLipsPuckerValue + ((tempFloat / 65.0f) * morphSpeed), mLipsPuckerTarget + mLipsPoutyValue + voicePuckerAdjust ); }
//					if (mLipsPuckerTarget + mLipsPoutyValue + voicePuckerAdjust  < mLipsPuckerValue - 0.01f) { mLipsPuckerValue = Mathf.Max(mLipsPuckerValue - ((tempFloat / 120.0f) * morphSpeed), mLipsPuckerTarget + mLipsPoutyValue + voicePuckerAdjust ); }
					if (mLipsPuckerTarget > mLipsPuckerValue + 0.01f) { mLipsPuckerValue = Mathf.Min(mLipsPuckerValue + (0.015f), mLipsPuckerTarget); }
					if (mLipsPuckerTarget < mLipsPuckerValue - 0.01f) { mLipsPuckerValue = Mathf.Max(mLipsPuckerValue - (0.015f), mLipsPuckerTarget); }
					morphLipsLipsPucker.morphValue = Round(Mathf.Clamp(mLipsPuckerValue - mSmileFullFaceValue - Mathf.Max(mSmileSimpleLeftValue / 2.0f, mSmileSimpleRightValue / 2.0f),0.0f,1.0f));
					
					if (mLipsPuckerWideTarget > mLipsPuckerWideValue + 0.1f) { mLipsPuckerWideValue = Mathf.Min(mLipsPuckerWideValue + (0.01f * mouthVariation * morphSpeed), mLipsPuckerWideTarget); }
					if (mLipsPuckerWideTarget < mLipsPuckerWideValue - 0.1f) { mLipsPuckerWideValue = Mathf.Max(mLipsPuckerWideValue - (0.007f * mouthVariation * morphSpeed), mLipsPuckerWideTarget); }
					morphLipsLipsPuckerWide.morphValue = Round(Mathf.Clamp(mLipsPuckerWideValue,0.0f,1.0f));
					//SuperController.LogError("Target " + mLipsPuckerWideTarget + " Value " + mLipsPuckerWideValue);
				}
				if (morphLipsLipBite != null)
				{
					if (mLipBiteTarget > mLipBiteValue + 0.05f) { mLipBiteValue = Mathf.Min(mLipBiteValue + ((Mathf.Abs(mLipBiteTarget - mLipBiteValue) / 30.0f) * mouthVariation * morphSpeed), mLipBiteTarget ); }
					if (mLipBiteTarget < mLipBiteValue - 0.05f) { mLipBiteValue = Mathf.Max(mLipBiteValue - ((Mathf.Abs(mLipBiteTarget - mLipBiteValue) / 20.0f) * mouthVariation * morphSpeed), mLipBiteTarget ); }
					morphLipsLipBite.morphValue = Round(mLipBiteValue);
					mLipBottomInTarget = Mathf.Clamp(mLipBiteValue,0.0f,0.3f);
					//SuperController.LogError("V " + mLipBiteValue + "." + mLipBiteTarget, false);
				}

				if (morphLipBottomIn != null)
				{
					mLipBottomInValue = morphLipBottomIn.morphValue;
					if (mLipBottomInTarget > mLipBottomInValue + 0.05f) { mLipBottomInValue = Mathf.Min(mLipBottomInValue + ((Mathf.Abs(mLipBottomInTarget - mLipBottomInValue) / 25.0f) * mouthVariation * morphSpeed), mLipBottomInTarget ); }
					if (mLipBottomInTarget < mLipBottomInValue - 0.05f) { mLipBottomInValue = Mathf.Max(mLipBottomInValue - ((Mathf.Abs(mLipBottomInTarget - mLipBottomInValue) / 35.0f) * mouthVariation * morphSpeed), mLipBottomInTarget ); }
					//morphLipBottomIn.morphValue = Round(mLipBottomInValue);
				}
				if (morphExpFlirting != null)
				{
					tempFloat = Limit(mFlirtingTarget - Limit(mSmileOpenFullFaceTarget) - Limit(mLipBiteTarget) - Limit(mMouthOpenTarget) - Limit(mMouthOpenWideTarget));
					if (tempFloat > mFlirtingValue + 0.01f) { mFlirtingValue = Mathf.Min(mFlirtingValue + ((Mathf.Abs(tempFloat - mFlirtingValue) / 15.0f) * mouthVariation * morphSpeed), tempFloat); }
					if (tempFloat < mFlirtingValue - 0.01f) { mFlirtingValue = Mathf.Max(mFlirtingValue - ((Mathf.Abs(tempFloat - mFlirtingValue) / 45.0f) * mouthVariation * morphSpeed), tempFloat); }
					morphExpFlirting.morphValue = Round(mFlirtingValue);
				}
				if (morphExpDeserveIt != null)
				{
					if (Round(mTakingItValue) <= 0.1f)
					{
						if (mDeserveItTarget > mDeserveItValue + 0.01f) { mDeserveItValue = Mathf.Min(mDeserveItValue + ((Mathf.Abs(mDeserveItTarget - mDeserveItValue) / 85.0f) * mouthVariation * tempFloat2 * morphSpeed), mDeserveItTarget); }
						morphExpDeserveIt.morphValue = Round(mDeserveItValue);
					}
					else
					{
						if (Round(mDeserveItValue) > 0.1f)
						{
							mTakingItTarget = 0.0f;
						}
					}
					if (mDeserveItTarget < mDeserveItValue - 0.01f) { mDeserveItValue = Mathf.Max(mDeserveItValue - ((Mathf.Abs(mDeserveItTarget - mDeserveItValue) / 125.0f) * mouthVariation * tempFloat2 * morphSpeed), mDeserveItTarget); }
				}
				if (morphExpTakingIt != null)
				{
					if (Round(mDeserveItValue) <= 0.1f)
					{
						if (mTakingItTarget > mTakingItValue + 0.01f) { mTakingItValue = Mathf.Min(mTakingItValue + ((Mathf.Abs(mTakingItTarget - mTakingItValue) / 75.0f) * mouthVariation * tempFloat2 * morphSpeed), mTakingItTarget); }
						morphExpTakingIt.morphValue = Round(mTakingItValue);
					}
					else
					{
						if (Round(mTakingItValue) > 0.1f)
						{
							mDeserveItTarget = 0.0f;
						}
					}
					if (mTakingItTarget < mTakingItValue - 0.01f) { mTakingItValue = Mathf.Max(mTakingItValue - ((Mathf.Abs(mTakingItTarget - mTakingItValue) / 105.0f) * mouthVariation * tempFloat2 * morphSpeed), mTakingItTarget); }
				}

				if (morphExpHappy != null)
				{
					if (mHappyTarget > mHappyValue + 0.02f) { mHappyValue = Mathf.Min(mHappyValue + ((Mathf.Abs(mHappyTarget - mHappyValue) / 75.0f) * mouthVariation * morphSpeed), mHappyTarget); }
					if (mHappyTarget < mHappyValue - 0.02f) { mHappyValue = Mathf.Max(mHappyValue - ((Mathf.Abs(mHappyTarget - mHappyValue) / 185.0f) * mouthVariation * morphSpeed), mHappyTarget); }
					morphExpHappy.morphValue = Round(mHappyValue * uiMaxMorphSmile.val);
				}
				if (mExcitementTarget > mExcitementValue + 0.1f) { mExcitementValue = Mathf.Min(mExcitementValue + ((Mathf.Abs(mExcitementTarget - mExcitementValue) / 65.0f) * browVariation * morphSpeed), mExcitementTarget); }
				if (mExcitementTarget < mExcitementValue - 0.1f) { mExcitementValue = Mathf.Max(mExcitementValue - ((Mathf.Abs(mExcitementTarget - mExcitementValue) / 55.0f) * browVariation * morphSpeed), mExcitementTarget); }
				if (morphExpExcitement != null)
				{
					if (malSmile)
					{
						morphExpExcitement.morphValue = Round(Mathf.Min(mExcitementValue * 1.5f * uiMaxMorphSmile.val, 1.0f));
					}
					else
					{
						morphExpExcitement.morphValue = Round(mExcitementValue * uiMaxMorphSmile.val);
					}
				}

				//SuperController.LogError(uiSmileOffset.val.ToString());
				if (morphExpSmileFullFace != null && morphExpSmileOpenFullFace != null)
				{
					if (mouthCanOpen == false && currentMouth != "Big Smile")
					{
						//mSmileFullFaceTarget = 0.0f;
						//SuperController.LogMessage("Full Face set to 0 mouth cant open", false);
						//mSmileOpenFullFaceTarget = 0.0f;
					}
					if (mSmileOpenFullFaceTarget + Mathf.Min(voiceOpenAdjust,0.0f) - mSmileFullFaceValue - mHappyValue > mSmileOpenFullFaceValue + 0.01f) { mSmileOpenFullFaceValue = Mathf.Min(mSmileOpenFullFaceValue + ((Mathf.Abs(mSmileOpenFullFaceTarget - mSmileOpenFullFaceValue) / 45.0f) * mouthVariation * morphSpeed), mSmileOpenFullFaceTarget + Mathf.Min(voiceOpenAdjust,0.0f) - mSmileFullFaceValue - mHappyValue, 1.0f); }
					if (mSmileOpenFullFaceTarget + Mathf.Min(voiceOpenAdjust,0.0f) - mSmileFullFaceValue - mHappyValue < mSmileOpenFullFaceValue - 0.01f) { mSmileOpenFullFaceValue = Mathf.Max(mSmileOpenFullFaceValue - ((Mathf.Abs(mSmileOpenFullFaceTarget - mSmileOpenFullFaceValue) / Mathf.Lerp(25.0f, 85.0f, interestValence/10.0f)) * mouthVariation * morphSpeed), mSmileOpenFullFaceTarget + Mathf.Min(voiceOpenAdjust,0.0f) - mSmileFullFaceValue - mHappyValue, 0.0f); }
					morphExpSmileOpenFullFace.morphValue = Mathf.Lerp(Mathf.SmoothStep(0.0f,1.0f,Round(mSmileOpenFullFaceValue) * uiMaxMorphSmile.val) + uiSmileOffset.val, 0.0f, smileDamper);
					if (Mathf.Clamp(mSmileFullFaceTarget, 0.0f, interestMaxSmile) > mSmileFullFaceValue + 0.01f) { mSmileFullFaceValue = Mathf.Min(mSmileFullFaceValue + ((Mathf.Abs(mSmileFullFaceTarget - mSmileFullFaceValue) / 35.0f) * mouthVariation * morphSpeed), 1.0f); }
					if (Mathf.Clamp(mSmileFullFaceTarget, 0.0f, interestMaxSmile) < mSmileFullFaceValue - 0.01f) { mSmileFullFaceValue = Mathf.Max(mSmileFullFaceValue - ((Mathf.Abs(mSmileFullFaceTarget - mSmileFullFaceValue) / Mathf.Lerp(30.0f, 85.0f, interestValence/10.0f)) * mouthVariation * morphSpeed), 0.0f); }
					//mSmileFullFaceValue = Mathf.Min(0.5f * uiMaxMorphSmile.val,mSmileFullFaceValue);
					if (timboSmile)
					{
						morphExpSmileFullFace.morphValue = (Round(Mathf.Lerp(Mathf.Max(mSmileFullFaceValue - (mSmileOpenFullFaceValue / 1.0f), 0.0f) / 2.0f, 0.0f, smileDamper)) * uiMaxMorphSmile.val) + uiSmileOffset.val;
					}
					else
					{
						morphExpSmileFullFace.morphValue = (Round(Mathf.Lerp(Mathf.Max(mSmileFullFaceValue - (mSmileOpenFullFaceValue / 1.0f), 0.0f), 0.0f, smileDamper)) * uiMaxMorphSmile.val) + uiSmileOffset.val;
					}
					//SuperController.LogMessage("Full Smile target = " + Round(mSmileFullFaceTarget) + "/" + Round(mSmileFullFaceValue));
				}
				if (morphVisF != null)
				{
					if (mVisFTarget > mVisFValue + 0.03f) { mVisFValue = Mathf.Min(mVisFValue + (0.0003f * mouthVariation * morphSpeed), mVisFTarget); }
					if (mVisFTarget < mVisFValue - 0.03f) { mVisFValue = Mathf.Max(mVisFValue - (0.0001f * mouthVariation * morphSpeed), mVisFTarget); }
					morphVisF.morphValue = Round(mVisFValue);
				}
				if (morphVisM != null)
				{
					if (mVisMTarget > mVisMValue + 0.01f) { mVisMValue = Mathf.Min(mVisMValue + (0.0125f * mouthVariation * morphSpeed), mVisMTarget); }
					if (mVisMTarget < mVisMValue - 0.01f) { mVisMValue = Mathf.Max(mVisMValue - (0.007f * mouthVariation * morphSpeed), mVisMTarget); }
					morphVisM.morphValue = Round(mVisMValue);
				}
				if (morphVisOW != null)
				{
					if (mVisOWTarget > mVisOWValue + 0.005f) { mVisOWValue = Mathf.Min(mVisOWValue + (0.015f * mouthVariation * morphSpeed), mVisOWTarget); }
					if (mVisOWTarget < mVisOWValue - 0.005f) { mVisOWValue = Mathf.Max(mVisOWValue - (0.025f * mouthVariation * morphSpeed), mVisOWTarget); }
					morphVisOW.morphValue = Round(mVisOWValue + mouthBreath);
				}
				if (morphVisAA != null)
				{
					if (mVisAATarget > mVisAAValue + 0.01f) { mVisAAValue = Mathf.Min(mVisAAValue + (0.025f * mouthVariation * morphSpeed), mVisAATarget); }
					if (mVisAATarget < mVisAAValue - 0.01f) { mVisAAValue = Mathf.Max(mVisAAValue - (0.015f * mouthVariation * morphSpeed), mVisAATarget); }
					morphVisAA.morphValue = Round(mVisAAValue);
				}

				if (morphMouthSmileSimpleLeft != null && morphMouthSmileSimpleRight != null)
				{
					if (Mathf.Clamp((interestValence / 40.0f) + mSmileSimpleLeftTarget - mMouthNarrowValue - mSmileFullFaceValue, 0.0f, interestMaxSmile) > mSmileSimpleLeftValue + 0.02f) { mSmileSimpleLeftValue = Mathf.Min(mSmileSimpleLeftValue + ((Mathf.Abs(mSmileSimpleLeftTarget - mSmileSimpleLeftValue) / 135.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / 80.0f) + mSmileSimpleLeftTarget - mMouthNarrowValue - mSmileFullFaceValue, 0.0f, interestMaxSmile)); }
					if (Mathf.Clamp((interestValence / 40.0f) + mSmileSimpleLeftTarget - mMouthNarrowValue - mSmileFullFaceValue, 0.0f, interestMaxSmile) < mSmileSimpleLeftValue - 0.02f) { mSmileSimpleLeftValue = Mathf.Max(mSmileSimpleLeftValue - ((Mathf.Abs(mSmileSimpleLeftTarget - mSmileSimpleLeftValue) / 150.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / 80.0f) + mSmileSimpleLeftTarget - mMouthNarrowValue - mSmileFullFaceValue, 0.0f, interestMaxSmile)); }
					morphMouthSmileSimpleLeft.morphValue = (Round(Mathf.Lerp(Mathf.Clamp(mSmileSimpleLeftValue - mSmileOpenFullFaceValue, 0.0f, 0.4f), 0.0f, smileDamper)) * uiMaxMorphSmile.val) + uiSmileOffset.val;
					if (Mathf.Clamp((interestValence / 40.0f) + mSmileSimpleRightTarget - mMouthNarrowValue, 0.0f, interestMaxSmile) > mSmileSimpleRightValue + 0.02f) { mSmileSimpleRightValue = Mathf.Min(mSmileSimpleRightValue + ((Mathf.Abs(mSmileSimpleRightTarget - mSmileSimpleRightValue) / 135.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / 80.0f) + mSmileSimpleRightTarget - mMouthNarrowValue, 0.0f, interestMaxSmile)); }
					if (Mathf.Clamp((interestValence / 40.0f) + mSmileSimpleRightTarget - mMouthNarrowValue, 0.0f, interestMaxSmile) < mSmileSimpleRightValue - 0.02f) { mSmileSimpleRightValue = Mathf.Max(mSmileSimpleRightValue - ((Mathf.Abs(mSmileSimpleRightTarget - mSmileSimpleRightValue) / 150.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / 80.0f) + mSmileSimpleRightTarget - mMouthNarrowValue, 0.0f, interestMaxSmile)); }
					morphMouthSmileSimpleRight.morphValue = (Round(Mathf.Lerp(Mathf.Clamp(mSmileSimpleRightValue - mSmileOpenFullFaceValue, 0.0f, 0.4f), 0.0f, smileDamper)) * uiMaxMorphSmile.val) + uiSmileOffset.val;
				}

				if (morphMouthSideLeft != null && morphMouthSideRight != null)
				{
					if (mMouthSideLeftTarget > mMouthSideLeftValue + 0.05f) { mMouthSideLeftValue = Mathf.Min(mMouthSideLeftValue + ((Mathf.Abs(mMouthSideLeftTarget - mMouthSideLeftValue) / 15.0f) * mouthVariation * morphSpeed), mMouthSideLeftTarget); }
					if (mMouthSideLeftTarget < mMouthSideLeftValue - 0.05f) { mMouthSideLeftValue = Mathf.Max(mMouthSideLeftValue - ((Mathf.Abs(mMouthSideLeftTarget - mMouthSideLeftValue) / 30.0f) * mouthVariation * morphSpeed), mMouthSideLeftTarget); }
					morphMouthSideLeft.morphValue = Round(mMouthSideLeftValue);
					if (mMouthSideRightTarget > mMouthSideRightValue + 0.05f) { mMouthSideRightValue = Mathf.Min(mMouthSideRightValue + ((Mathf.Abs(mMouthSideRightTarget - mMouthSideRightValue) / 15.0f) * mouthVariation * morphSpeed), mMouthSideRightTarget); }
					if (mMouthSideRightTarget < mMouthSideRightValue - 0.05f) { mMouthSideRightValue = Mathf.Max(mMouthSideRightValue - ((Mathf.Abs(mMouthSideRightTarget - mMouthSideRightValue) / 30.0f) * mouthVariation * morphSpeed), mMouthSideRightTarget); }
					morphMouthSideRight.morphValue = Round(mMouthSideRightValue);
				}
				//SuperController.LogError("Mouth Morphs Done");
				
				if (mTongueInOutTarget > mTongueInOutValue + 0.01f) { mTongueInOutValue = Mathf.Min(mTongueInOutValue + (0.035f * mouthVariation * morphSpeed), mTongueInOutTarget); }
				if (mTongueInOutTarget < mTongueInOutValue - 0.01f) { mTongueInOutValue = Mathf.Max(mTongueInOutValue - (0.035f * mouthVariation * morphSpeed), mTongueInOutTarget); }
				if (morphTongueInOut != null && uiControlTongue.val == true)
				{
					morphTongueInOut.morphValue = Round(mTongueInOutValue);
				}
				if (mTongueSideSideTarget > mTongueSideSideValue + 0.01f) { mTongueSideSideValue = Mathf.Min(mTongueSideSideValue + (0.034f * mouthVariation * morphSpeed), mTongueSideSideTarget); }
				if (mTongueSideSideTarget < mTongueSideSideValue - 0.01f) { mTongueSideSideValue = Mathf.Max(mTongueSideSideValue - (0.037f * mouthVariation * morphSpeed), mTongueSideSideTarget); }
				if (morphTongueSideSide != null && uiControlTongue.val == true)
				{
					morphTongueSideSide.morphValue = Round(mTongueSideSideValue);
				}
				if (mTongueBendTipTarget > mTongueBendTipValue + 0.01f) { mTongueBendTipValue = Mathf.Min(mTongueBendTipValue + (0.06f * mouthVariation * morphSpeed), mTongueBendTipTarget); }
				if (mTongueBendTipTarget < mTongueBendTipValue - 0.01f) { mTongueBendTipValue = Mathf.Max(mTongueBendTipValue - (0.06f * mouthVariation * morphSpeed), mTongueBendTipTarget); }
				if (morphTongueBendTip != null && uiControlTongue.val == true)
				{
					morphTongueBendTip.morphValue = Round(mTongueBendTipValue);
				}
				if (currentInterest != "Kissing")
				{
					mTongueTongueTwistTarget = 0.0f;
				}
				if (mTongueTongueTwistTarget > mTongueTongueTwistValue + 0.01f) { mTongueTongueTwistValue = Mathf.Min(mTongueTongueTwistValue + (0.015f * mouthVariation * morphSpeed), mTongueTongueTwistTarget); }
				if (mTongueTongueTwistTarget < mTongueTongueTwistValue - 0.01f) { mTongueTongueTwistValue = Mathf.Max(mTongueTongueTwistValue - (0.015f * mouthVariation * morphSpeed), mTongueTongueTwistTarget); }
				if (morphTongueTwist != null && uiControlTongue.val == true)
				{
					morphTongueTwist.morphValue = Round(mTongueTongueTwistValue);
				}
				//SuperController.LogError("Tongue Morphs Done");
				
				morphNoseSmile.morphValue = Mathf.Clamp(Round(Mathf.Max(mSmileFullFaceValue * 2.0f, mSmileOpenFullFaceValue, mSmileSimpleLeftValue * 2.0f, mSmileSimpleRightValue * 2.0f) * 0.75f), 0.0f, 0.7f);
				if (morphMouthStretchL != null && morphMouthStretchR != null && onlyUseBuiltIn == false)
				{
					morphMouthStretchL.morphValue = Mathf.Clamp(Round(Mathf.Max(mSmileFullFaceValue / 2.0f, mSmileOpenFullFaceValue / 2.5f, mSmileSimpleLeftValue / 2.0f)), 0.0f, 0.4f);
					morphMouthStretchR.morphValue = Mathf.Clamp(Round(Mathf.Max(mSmileFullFaceValue / 2.0f, mSmileOpenFullFaceValue / 2.5f, mSmileSimpleRightValue / 2.0f)), 0.0f, 0.4f);
				}

				if (morphShoulderFixLeftF != null && morphShoulderFixLeftR != null && morphShoulderFixRightF != null && morphShoulderFixRightR != null)
				{
					tempFloat = Mathf.Clamp((-0.2f + Vector3.Distance(lElbowController.followWhenOff.position, abdomenController.followWhenOff.position)) * 2.5f, 0.0f, 1.0f);
					//testString = "|" + ((Mathf.Clamp(tempFloat, 0.18f, 0.3f) - 0.12f) * 8.33f);
					
					//Vector3 tempplane = Vector3.ProjectOnPlane(lArmController.followWhenOff.right, pelvisController.followWhenOff.forward);
					//float tempangle = Vector3.SignedAngle(chestController.followWhenOff.forward, lArmController.followWhenOff.right, chestController.followWhenOff.forward);
					//testString = tempangle.ToString();
          //SuperController.LogError("value " + Round(tempFloat), false);
					
					mShoulderFixLeftTarget = Mathf.Lerp(0.2f, -0.45f, tempFloat);//1.0f - (tempFloat * 18.33f) * 100.0f;
					
					tempFloat = Mathf.Clamp((-0.2f + Vector3.Distance(rElbowController.followWhenOff.position, abdomenController.followWhenOff.position)) * 2.5f, 0.0f, 1.0f);
					mShoulderFixRightTarget = Mathf.Lerp(0.2f, -0.45f, tempFloat);//1.0f - (tempFloat * 18.33f) * 100.0f;
					
					if (mShoulderFixLeftTarget > mShoulderFixLeftValue + 0.01f) { mShoulderFixLeftValue = Mathf.Min(mShoulderFixLeftValue + (0.1f), mShoulderFixLeftTarget); }
					if (mShoulderFixLeftTarget < mShoulderFixLeftValue - 0.01f) { mShoulderFixLeftValue = Mathf.Max(mShoulderFixLeftValue - (0.1f), mShoulderFixLeftTarget); }
					if (mShoulderFixLeftTarget > 0.0f)
					  {
						//morphShoulderFixLeftF.morphValue = Round(-mShoulderFixLeftValue * 2);
					  }
					  else
					  {
						//morphShoulderFixLeftF.morphValue = Round(mShoulderFixLeftValue);
					  }
					  //morphShoulderFixLeftR.morphValue = Round(Mathf.Lerp(0.0f, 1.0f, mShoulderFixLeftValue));
					if (mShoulderFixRightTarget > mShoulderFixRightValue + 0.01f) { mShoulderFixRightValue = Mathf.Min(mShoulderFixRightValue + (0.1f), mShoulderFixRightTarget); }
					if (mShoulderFixRightTarget < mShoulderFixRightValue - 0.01f) { mShoulderFixRightValue = Mathf.Max(mShoulderFixRightValue - (0.1f), mShoulderFixRightTarget); }
					if (mShoulderFixLeftTarget > 0.0f)
					  {
					   // morphShoulderFixLeftF.morphValue = Round(-mShoulderFixRightValue * 2);
					  }
					  else
					  {
						//morphShoulderFixRightF.morphValue = Round(mShoulderFixRightValue);
					  }
								//morphShoulderFixRightR.morphValue = Round(Mathf.Lerp(0.0f, 1.0f, mShoulderFixRightValue));
				}
				
			}
			else
			{
				if (resetMorphs == false)
				{
					resetMorphs = true;
					//morphLHandFist.morphValue = 0.0f;
					//morphRHandFist.morphValue = 0.0f;
					//morphLHandStraighten.morphValue = 0.0f;
					//morphRHandStraighten.morphValue = 0.0f;

					morphBrowDown.morphValue = 0.0f;
					morphBrowUp.morphValue = 0.0f;
					morphBrowCenterUp.morphValue = 0.0f;
					morphBrowOuterUpLeft.morphValue = 0.0f;
					morphBrowOuterUpRight.morphValue = 0.0f;

					morphEyesClosedLeft.morphValue = 0.0f;
					morphEyesClosedRight.morphValue = 0.0f;
					morphEyesSquint.morphValue = 0.0f;
					morphEyesPupils.morphValue = 0.0f;
					morphNoseFlare.morphValue = 0.0f;

					morphExpSmileFullFace.morphValue = 0.0f;
					morphExpSmileOpenFullFace.morphValue = 0.0f;
					morphExpGlare.morphValue = 0.0f;
					morphExpExcitement.morphValue = 0.0f;
					morphExpHappy.morphValue = 0.0f;
					morphExpFlirting.morphValue = 0.0f;
					morphExpDeserveIt.morphValue = 0.0f;
					morphExpTakingIt.morphValue = 0.0f;

					morphMouthMouthOpen.morphValue = 0.0f;
					morphMouthMouthOpenWide.morphValue = 0.0f;
					morphMouthOpenWider.morphValue = 0.0f;
					morphMouthNarrow.morphValue = 0.0f;
					morphMouthSideLeft.morphValue = 0.0f;
					morphMouthSideRight.morphValue = 0.0f;
					morphMouthSmileSimpleLeft.morphValue = 0.0f;
					morphMouthSmileSimpleRight.morphValue = 0.0f;

					morphLipsLipsPucker.morphValue = 0.0f;
					morphLipsLipsPuckerWide.morphValue = 0.0f;
					morphLipsLipBite.morphValue = 0.0f;
					morphLipsLipsClose.morphValue = 0.0f;
					morphLipsLipsPart.morphValue = 0.0f;
					morphLipsLipsPartCenter.morphValue = 0.0f;
					morphLipsBottomDown.morphValue = 0.0f;
					morphLipBottomIn.morphValue = 0.0f;
					morphLipsPouty.morphValue = 0.0f;
					morphMouthSmileMuscle.morphValue = 0.0f;
					morphVisF.morphValue = 0.0f;
					morphVisM.morphValue = 0.0f;
					morphVisOW.morphValue = 0.0f;
					morphVisAA.morphValue = 0.0f;

					morphTongueInOut.morphValue = 1.0f;
					morphTongueSideSide.morphValue = 0.0f;
					morphTongueBendTip.morphValue = 0.0f;
					morphTongueLength.morphValue = 0.0f;
					morphRibCageSize.morphValue = 0.0f;
					morphChestHeight.morphValue = 0.0f;
					morphBreastHeight.morphValue = 0.0f;
					morphBreastDroopLeft.morphValue = 0.0f;
					morphBreastDroopRight.morphValue = 0.0f;
					morphBreastHangLeft.morphValue = 0.0f;
					morphBreastHangRight.morphValue = 0.0f;
					morphBreath.morphValue = 0.0f;
					//morphRibsDef.morphValue = 0.0f;
					morphSternumDepth.morphValue = 0.0f;
					//morphNipplesApply.morphValue = 0.0f;
					if (morphDeepBulgeBellyBottom != null)
					{
						morphDeepBulgeBellyBottom.morphValue = 0.0f;
						morphDeepBulgeBellyMid.morphValue = 0.0f;
						morphDeepThroat.morphValue = 0.0f;
						morphBlowjobLips.morphValue = 0.0f;
					}
					morphCheekSink.morphValue = 0.0f;

					if (morphShoulderFixRightR != null)
					{
						morphShoulderFixLeftF.morphValue = 0.0f;
						morphShoulderFixLeftR.morphValue = 0.0f;
						morphShoulderFixRightF.morphValue = 0.0f;
						morphShoulderFixRightR.morphValue = 0.0f;
					}
				}
			}
			
			if (mMouthOpenValue > 0.05f || mMouthOpenWideValue > 0.15f || mMouthOpenWiderValue > 0.15f || mSmileOpenFullFaceValue > 0.34f || mLipsBottomDownValue > 0.3f || mMouthOpenValue + mMouthOpenWideValue + mMouthOpenWiderValue + mSmileOpenFullFaceValue > 0.7f)
			{
				if (mouthCanOpen)
				{
					mouthOpenTimer += Time.fixedDeltaTime;
				}
				else
				{
					mouthOpenTimer = Mathf.Max(mouthOpenTimer - Time.fixedDeltaTime, 0.0f);
					if (mouthOpenTimer <= 0.0f)
					{
						mouthCanOpen = true;
					}
				}
				if (mouthOpenTimer > 5.0f)
				{
					mouthCanOpen = false;
				}
			}
			else
			{
				mouthOpenTimer = Mathf.Max(mouthOpenTimer - Time.fixedDeltaTime, 0.0f);
				if (mouthCanOpen == false && mouthOpenTimer <= 0.0f)
				{
					mouthCanOpen = true;
				}
				
			}

			
            if (saccadeClock <= 0.0f && mEyesClosedLeftValue < 0.5f && interestKissing == false)
            {
				//SuperController.LogError("Saccade Start");
				tempFloat2 = Mathf.Clamp(Vector3.Distance(headController.followWhenOff.position, eyeController.transform.position) - (closeFaceDistance * 1.0f),0.0f,1.0f);
				tempFloat = (10.0f + (uiSaccadeAmount.val * Mathf.Lerp(0.2f,1.0f,tempFloat2)));//(saccadeAmount / Random.Range(0.8f,1.2f)) * uiSaccadeAmount.val * Mathf.Lerp(0.3f,1.0f,tempFloat2);
                float saccade = Random.Range(0.0f,100.0f); //Mathf.Lerp(30.0f,60.0f,tempFloat/30.0f)) / uiSaccadeSpeed.val;//Random.Range(0.0f, Mathf.Clamp(150.0f * (0.5f + (100.0f - pExtraversion)), 0.0f, 100.0f));
				debugString = " RND " + Round(saccade) + " Base " + Round(tempFloat) + " ";
                //saccadeOffset = new Vector3(0.0f,0.0f,0.0f);
                float saccadeLength = (((10.0f + interestArousal) / (2.0f * (10.0f - interestValence))));// * ((2.0f * tempFloat + 20.0f) / 100.0f));// * Random.Range(0.5f, 1.5f);
                float saccadeRandom = Random.Range(-1.0f, 1.0f);
                saccadeClock = Mathf.Clamp(saccadeLength, 1.1f, 3.5f) * Mathf.Lerp(0.85f,0.15f,tempFloat/30.0f) / uiSaccadeSpeed.val;

				if (gAvoid == 1.0f)
				{
					//tempFloat = tempFloat * 2.0f;
				}
				tempFloat2 = Mathf.Clamp(0.5f,1.0f,playerHeadToHead - closeFaceDistance);
				if (Vector3.Distance(new Vector3(0.0f,0.0f,0.0f), saccadeOffset) > 10.0f * uiSaccadeAmount.val)
				{
					saccadeOffsetCounter += 1.0f;
				}
                bool sChange = false;
                if (saccade <= Mathf.Lerp(6.46f, 0.0f, Mathf.Clamp(-saccadeOffset.x*0.1f, 0.0f, 1.0f)) && sChange == false && lastSaccade != "U")
                {
                    //up right
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(tempFloat, tempFloat/2.0f), saccadeOffset.y + Random.Range(0.0f, tempFloat / 200.0f), 0.0f);
                    sChange = true;
					debugString += " |UR ";
					saccadeClock = saccadeClock / 2.0f;
					lastSaccade = "R";
                }
                if (saccade <= Mathf.Lerp(7.45f, 0.0f, Mathf.Clamp(saccadeOffset.x*0.1f, 0.0f, 1.0f)) && sChange == false && lastSaccade != "U")
                {
                    //up left
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(-tempFloat, -tempFloat/2.0f), saccadeOffset.y - Random.Range(0.0f, tempFloat / 200.0f), 0.0f);
                    sChange = true;
					debugString += " |UL ";
					saccadeClock = saccadeClock / 2.0f;
					lastSaccade = "L";
                }
                if (saccade <= Mathf.Lerp(7.79f, 0.0f, Mathf.Clamp(-saccadeOffset.x*0.1f, 0.0f, 1.0f)) && sChange == false && lastSaccade != "D")
                {
                    //down right
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(tempFloat*Random.Range(0.8f,1.2f), tempFloat/2.0f), saccadeOffset.y + Random.Range(0.0f, -tempFloat*Random.Range(0.8f,1.2f)), 0.0f);
                    sChange = true;
					debugString += " |DR ";
					lastSaccade = "R";
                }
                if (saccade <= Mathf.Lerp(7.89f, 0.0f, Mathf.Clamp(saccadeOffset.x*0.1f, 0.0f, 1.0f)) && sChange == false && lastSaccade != "D")
                {
                    //down left
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(-tempFloat*Random.Range(0.8f,1.2f), -tempFloat/2.0f), saccadeOffset.y + Random.Range(0.0f, -tempFloat*Random.Range(0.8f,1.2f)), 0.0f);
                    sChange = true;
					debugString += " |DL ";
					lastSaccade = "L";
                }
                if ((saccade <= Mathf.Lerp(Mathf.Lerp(12.8f, 16.8f, interestArousal/10.0f), 0.0f, Mathf.Clamp(-saccadeOffset.x*0.1f, 0.0f, 1.0f)) || (saccade <= 31.08f && saccadeOffset.x < 0.0f)) && sChange == false && lastSaccade != "R")
                {
                    //right
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(tempFloat*Random.Range(0.5f,1.0f), tempFloat/2.0f), saccadeOffset.y, 0.0f);
                    sChange = true;
					debugString += " |R ";
					//SuperController.LogError("Right Saccade", false);
					if (interestArousal > 6.0f && saccadeOffset.x < 0.0f)
					{
						saccadeClock = saccadeClock / 2.0f;
					}
					lastSaccade = "R";
                }
                if ((saccade <= Mathf.Lerp(Mathf.Lerp(16.8f, 20.8f, interestArousal/10.0f), 0.0f, Mathf.Clamp(saccadeOffset.x*0.1f, 0.0f, 1.0f)) || (saccade <= 33.6f && saccadeOffset.x > 0.0f))  && sChange == false && lastSaccade != "L")
                {
                    //left
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(-tempFloat*Random.Range(0.5f,1.0f), -tempFloat/2.0f), saccadeOffset.y, 0.0f);
                    sChange = true;
					debugString += " |L ";
					//SuperController.LogError("Left Saccade", false);
					if (interestArousal > 6.0f && saccadeOffset.x > 0.0f)
					{
						saccadeClock = saccadeClock / 2.0f;
					}
					lastSaccade = "L";
                }
                if ((saccade <= 17.69f || (saccadeOffset.y < 0.0f && saccade <= Mathf.Lerp(17.69f, 62.0f, Mathf.Max(-saccadeOffset.y, 1.0f)))) && sChange == false && lastSaccade != "U")
                {
                    //up
                    saccadeOffset = new Vector3(saccadeRandom, Random.Range(saccadeOffset.x, saccadeOffset.x + (tempFloat / 100.0f)), 0.0f);
                    sChange = true;
					debugString += " |U ";
					saccadeClock = saccadeClock / 2.0f;
					lastSaccade = "U";
                }
                if ((saccade <= 20.38f || (saccade <= Mathf.Lerp(20.38f,50.0f,interestValence/10.0f) && mainInterest == "Face")) && sChange == false && lastSaccade != "D")
                {
                    //down
                    saccadeOffset = new Vector3(saccadeOffset.x, Random.Range(0.0f, saccadeOffset.y-tempFloat*Random.Range(0.8f,1.6f)), 0.0f);
                    sChange = true;
					debugString += " |D ";
					lastSaccade = "D";
					if ((currentMouth == "Idle" || currentMouth == "Idle(S)" ) && saccadeOffset.x == 0.0f && saccadeOffset.y == 0.0f && Random.Range(0.0f, 100.0f) > Mathf.Lerp(100.0f, 80.0f, interestArousal/10.0f))
					{
						mouthSM.Switch(mBiteLip);
					}
					
                }
				if ((Mathf.Abs(saccadeOffset.x) > 12.5f * uiSaccadeWanderMult.val * tempFloat2 || saccadeOffset.y > 2.5f * uiSaccadeWanderMult.val * tempFloat2 || saccadeOffset.y < Mathf.Lerp(-7.5f, -14.5f, interestArousal/10.0f) * uiSaccadeWanderMult.val * tempFloat2) || Random.Range(0.0f,100.0f) > Mathf.Lerp(99855.0f,99989.0f,pExtraversion/100.0f) / 1000.0f || saccadeOffsetCounter > Mathf.Lerp(15.0f, 3.0f, interestValence/10.0f) || Random.Range(0.0f,100.0f) > Mathf.Lerp(Mathf.Lerp(95.0f, 80.0f, Mathf.Max(-saccadeOffset.y, 1.0f)), Mathf.Lerp(85.0f, 65.0f, Mathf.Max(-saccadeOffset.y, 1.0f)), interestArousal/10.0f) || Vector3.Distance(headController.followWhenOff.position, prevPosHead) > 0.05f || (playerHeadMovement && currentInterest == "Face"))
				{
					if (gAvoid == 0.0f)
					{
						saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
						debugString += " Reset ";
						saccadeOffsetCounter = 0.0f;
						lastSaccade = "";
					//SuperController.LogError("Saccade Start");
					}
				}
				if (Random.Range(0.0f,100.0f * uiBlinkSpeed.val) < 15.0f && sChange)
				{
					//SuperController.LogError("should blink");
					if (tempFloat >= 3.0f * uiSaccadeAmount.val && eyeClock > (2.05f * uiBlinkSpeed.val) * blinkRepeat)
					{
						if (currentEye != "Closed")
						{
							//SuperController.LogError("small move blink");
							//eyesSM.Switch(eBlink);
							//eyeClock = 0.0f;
							//debugString += " Sml Blnk ";
						}
					}
					else
					{
						if (tempFloat >= 5.0f * uiSaccadeAmount.val && eyeClock > (1.05f * uiBlinkSpeed.val) * blinkRepeat)
						{
							if (currentEye != "Closed")
							{
								//SuperController.LogError("large move blink");
								//eyesSM.Switch(eBlink);
								//eyeClock = 0.0f;
								//debugString += " Big Blnk";
							}
						}
					}
				}
				saccadeOffset.x = Mathf.Lerp(saccadeOffset.x / 10.0f, saccadeOffset.x, Mathf.Min(playerHeadToHead, 1.0f));
				if ((mainInterest == "Face" && Mathf.Abs(saccadeOffset.x) > 10.0f * uiSaccadeWanderMult.val && Random.Range(0.0f,100.0f) > 86.0f) || (currentMouth != "Idle" && currentMouth != "Idle(S)" && currentMouth != "Open" && currentMouth != "Closed" && currentMouth != "LipBite"))
				{
					saccadeOffset.x = 0.0f;
					debugString += " SReset";
				}
				debugString = "Offset " + Round(Vector3.Distance(new Vector3(0.0f,0.0f,0.0f), saccadeOffset)) + debugString;
				//SuperController.LogMessage(debugString + saccadeOffsetCounter, false);
				//SuperController.LogError(saccadeOffset.x + "/" + saccadeOffset.y + "/" + saccadeOffset.z);
				
				
            }
            else
            {
                saccadeClock -= Time.fixedDeltaTime;
            }
            //saccadeOffset = new Vector3(eyeController.transform.rotation.x + saccadeOffset.x,eyeController.transform.rotation.y + saccadeOffset.y,eyeController.transform.rotation.z + saccadeOffset.z);

			//SuperController.LogError("Saccade Done");

			tempFloat = 0.1f;
			tempFloat2 = Mathf.Clamp(50.0f - headActivityBoost,0.0f,50.0f);
            if (playerHandsUsable || person2Usable)
            {
                if (Vector3.Distance(playerLHand, playerLHandPrev) > minHandMotion)
                {
                    playerLHandMovement = true;
                    playerLHandTimeout = Mathf.Min(playerLHandTimeout + movementModifier, movementMaxTimeout);
					if (mainInterest == "LHand" || mainOld == "LHand" || secondInterest == "LHand" || secondOld == "LHand")
					{
						lHandActivityBoost = Mathf.Clamp(lHandActivityBoost + tempFloat,0.0f,tempFloat2);
					}
					else
					{
						lHandActivityBoost = Mathf.Clamp(lHandActivityBoost + tempFloat / 3.0f,0.0f,tempFloat2);
					}
                }
                else
                {
                    playerLHandTimeout = Mathf.Max(playerLHandTimeout - movementFalloff, 0.0f);
                    if (playerLHandTimeout == 0.0f)
                    {
                        playerLHandMovement = false;
                    }
					if (mainInterest != "LHand")
					{
						lHandActivityBoost = Mathf.Clamp(lHandActivityBoost - tempFloat / 2.0f,0.0f,tempFloat2);
					}
					else
					{
						lHandActivityBoost = Mathf.Clamp(lHandActivityBoost - tempFloat / 5.0f,0.0f,tempFloat2);
					}
                }
                if (Vector3.Distance(playerRHand, playerRHandPrev) > minHandMotion)
                {
                    playerRHandMovement = true;
                    playerRHandTimeout = Mathf.Min(playerRHandTimeout + movementModifier, movementMaxTimeout);
					if (mainInterest == "RHand" || mainOld == "RHand" || secondInterest == "RHand" || secondOld == "RHand")
					{
						rHandActivityBoost = Mathf.Clamp(rHandActivityBoost + tempFloat,0.0f,tempFloat2);
					}
					else
					{
						rHandActivityBoost = Mathf.Clamp(rHandActivityBoost + tempFloat / 3.0f,0.0f,tempFloat2);
					}
                }
                else
                {
                    playerRHandTimeout = Mathf.Max(playerRHandTimeout - movementFalloff, 0.0f);
                    if (playerRHandTimeout == 0.0f)
                    {
                        playerRHandMovement = false;
                    }
					if (mainInterest != "RHand")
					{
						rHandActivityBoost = Mathf.Clamp(rHandActivityBoost - tempFloat / 2.0f,0.0f,tempFloat2);
					}
					else
					{
						rHandActivityBoost = Mathf.Clamp(rHandActivityBoost - tempFloat / 5.0f,0.0f,tempFloat2);
					}
                }
            }
			
			if (emTargetName != "None")
			{
				//float temp = emTargetDistance
                /*if (Vector3.Distance(emTargetController.transform.position, emTargetPosPrev) > minHandMotion)
                {
					emTargetMovement = true;
					emTargetTimeout = Mathf.Min(emTargetTimeout + movementModifier, movementMaxTimeout);
				}
				else
				{
                    emTargetTimeout = Mathf.Max(emTargetTimeout - movementFalloff, 0.0f);
                    if (emTargetTimeout == 0.0f)
                    {
                        emTargetMovement = false;
                    }
				}*/
			}
			
			//SuperController.LogError("Movement Check done");
			
            if (amGlancing)
            {
                glanceClock += Time.fixedDeltaTime;
				//SuperController.singleton.ClearMessages();
				//SuperController.LogMessage("Glancing : " + Round(glanceClock) + " > " + Mathf.Clamp(5.0f + ((100.0f - pExtraversion) / 10), 10.0f * uiGlanceTimeout.val, 15.0f * uiGlanceTimeout.val), false);
                if (glanceClock > Mathf.Clamp(5.0f + ((100.0f - pExtraversion) / 10), 10.0f * uiGlanceTimeout.val, 15.0f * uiGlanceTimeout.val))
                {
                    glanceClock = 0.0f;
                    amGlancing = false;
					eyeUpdateClock = eyeUpdateTime + 1.0f;
					if (eyeClock >  1.5f * uiBlinkSpeed.val * blinkRepeat && currentEye != "Closed")
					{
						eyeClock = 0.0f;
						eyesSM.Switch(eBlink);
						//SuperController.LogError("glance done blink");
					}
                }
				//SuperController.LogError("amGlancing clock done");
				//energyAmount += 1.0f;
            }

			if (interestKissing)
			{
				if (playerHeadToHead < kissingDistance &&  uiDoKiss.val) //interestKissing == false &&
				{
					interestClock -= Time.fixedDeltaTime * 5.0f;
				}
				if (playerHeadToHead > kissingDistance && morphMouthAction == false && testRun == false && lipsTouchCount <= 0.0f)
				{
					//lookSM.Switch(lPlayful);
					mouthSM.Switch(mClosed);
					//SuperController.LogError("Closing Mouth");
					interestKissing = false;
					mTongueInOutTarget = 1.0f;
					interestClock = 0.0f;
				}
			}

            if (playerHandsUsable || person2Usable)
            {
                //interestLHand = interestLHandBase;
                //interestRHand = interestRHandBase;
			}
            //currentInterestLevel = Mathf.Max(currentInterestLevel - 0.12f, 0.0f);
            //interestFace = interestFaceBase;
			//SuperController.LogError("Interest Calc Start");
			
			//interestPLHand += (interestPLHandBase / 5000.0f) * uiInterestRate.val;
			//interestPRHand += (interestPRHandBase / 5000.0f) * uiInterestRate.val;
			interestLHand += (interestLHandBase / 5000.0f) * uiInterestRate.val;
			interestRHand += (interestRHandBase / 5000.0f) * uiInterestRate.val;
			interestFace += (interestFaceBase / 5000.0f) * uiInterestRate.val;
			interestEMTarget += (interestEMTargetBase / 5000.0f) * uiInterestRate.val;
			
			interestFace -= Mathf.Lerp(0.001f,0.004f,interestArousal/10.0f) * uiInterestRate.val;
			interestLHand -= Mathf.Lerp(0.001f,0.005f,interestValence/10.0f) * uiInterestRate.val;
			interestRHand -= Mathf.Lerp(0.001f,0.005f,interestValence/10.0f) * uiInterestRate.val;
			interestEMTarget -= Mathf.Lerp(0.003f,0.001f,interestValence/10.0f) * uiInterestRate.val;
			interestPLHand -= Mathf.Lerp(0.004f,0.018f,interestArousal/10.0f) * uiInterestRate.val;
			interestPRHand -= Mathf.Lerp(0.004f,0.018f,interestArousal/10.0f) * uiInterestRate.val;

            //Face
            if ((mainInterest == "Face" && mainOld == "Face") && mainClock > gDuration)
            {
                interestFace -= 0.1f * uiInterestRate.val;
				dbgHead += "-Timeout ";
            }
            if (playerHeadToFaceRot < playerLookDirectAngle)
            {
				if (interestFace + headActivityBoost < 50.0f)
				{
					if (playerHeadToHead < personalSpaceDistance)
					{
						interestFace += 0.04f * uiInterestRate.val;
					}
					else
					{
						interestFace += 0.005f * uiInterestRate.val;
					}
					interestLHand -= 0.001f * uiInterestRate.val;
					interestRHand -= 0.001f * uiInterestRate.val;
					interestPLHand -= 0.003f * uiInterestRate.val;
					interestPRHand -= 0.003f * uiInterestRate.val;
					dbgPLHand += "-LootAtPlayer ";
					dbgPRHand += "-LootAtPlayer ";
				}
				else
				{
					if (playerHeadToHead < personalSpaceDistance)
					{
						interestFace += 0.002f * uiInterestRate.val;
						lBackArchHands = true;
						rBackArchHands = true;
					}
				}
				interestArousal += 0.002f * uiArousalSpeed.val;
				lHandActivityBoost = lHandActivityBoost * 0.8f;
				rHandActivityBoost = rHandActivityBoost * 0.8f;
				if (interestValence > 5.0f)
				{
					interestArousal += 0.0004f * uiArousalSpeed.val;
				}
				interestValence += 0.003f * uiValenceSpeed.val;
				if (mainInterest == "Face"){fuzzyLock = 1.2f;}
				dbgHead += "+PLooking ";
            }
			else
			{
				interestFace -= 0.06f * uiInterestRate.val;
			}
			float tempDir = Vector3.Angle(headController.followWhenOff.forward, playerHeadTransform.forward);
			//SuperController.LogError(tempDir.ToString());
			if (playerHeadToHead < kissingDistance && interestKissing == false && uiDoKiss.val && testRun == false && tempDir > 160.0f && tempDir < 200.0f)// && lipsTouchCount > 0.0f)
			{
				lipsOnly = false;
				if (currentLook != "Kissing")
				{
					lookSM.Switch(lKissing);
				}
				//SuperController.LogError("Interest Kiss");
				interestFace += 0.08f * uiInterestRate.val;
				interestArousal += 0.005f * uiArousalSpeed.val;
				interestValence += 0.007f * uiValenceSpeed.val;
				//energyAmount += 1.0f;
				if (Random.Range(0.0f, 100.0f) < 50.0f && interestArousal > 7.0f)
				{
						lBreastHands = true;
						rBreastHands = true;
				}
				else
				{
						lBellyHands = true;
						rBellyHands = true;
				}
				dbgHead += "+WantKiss ";
			}
			if (interestKissing)
			{
				interestFace += 0.3f * uiInterestRate.val;
				interestArousal += 0.015f * uiArousalSpeed.val;
				interestValence += 0.011f * uiValenceSpeed.val;
				//energyAmount += 1.0f;
				dbgHead += "+Kissing ";
			}
            //if (headToEyeController < 10.0f && headToFaceRot < lookDirectAngle && playerHeadToFaceRot < lookDirectAngle && gAvoid == 0.0f && amGlancing == false && Vector3.Distance(new Vector3(0.0f,0.0f,0.0f),saccadeOffset) < 10.0f)
            if (playerHeadToFaceRot < playerLookDirectAngle && headToFaceRot < lookDirectAngle)
			{
				interestFace += 0.16f * uiInterestRate.val;
				interestArousal += 0.005f * uiArousalSpeed.val;
				interestValence += 0.007f * uiValenceSpeed.val;
				lHandActivityBoost = lHandActivityBoost * 0.8f;
				rHandActivityBoost = rHandActivityBoost * 0.8f;
				//energyAmount += 1.0f;
				dbgHead += "+EyeToEye ";
			}
            if (headToFaceRot < lookDirectAngle && mainInterest == "Face")
            {
                interestFace += 0.04f * uiInterestRate.val;
				if (Vector3.Distance(new Vector3(0.0f,0.0f,0.0f),saccadeOffset) < 1.0f)
				{
					interestFace += 0.01f * uiInterestRate.val;
					interestArousal += 0.0005f * uiArousalSpeed.val;
				}
				else
				{
					interestArousal -= 0.001f * uiArousalSpeed.val;
				}
				interestValence += 0.0005f * uiValenceSpeed.val;
                if (mainInterest == "Face"){fuzzyLock = 1.15f;}
				//energyAmount += 1.0f;
				dbgHead += "+LookAt ";
            }
			else
			{
				interestArousal -= 0.001f * uiArousalSpeed.val;
			}
            if (playerHeadToHead < closeFaceDistance * 3.0f)
            {
				if (mainInterest != "Face" && interestFace > minInterest && headToFaceRot < lookPeripheralAngle && gAvoid == 0 && amGlancing == false)
				{
					mainInterest = "Face";
				}
                interestFace += 0.03f * uiInterestRate.val;
                interestPelvis -= 0.1f * uiInterestRate.val;
                interestTip -= 0.2f * uiInterestRate.val;
				interestValence += 0.0001f * uiValenceSpeed.val;
				if (mainInterest == "Face"){fuzzyLock = 0.5f;}
				dbgHead += "+Close ";
            }
            if (playerHeadToHead < interactionDistance || playerHeadToLBreast < interactionDistance || playerHeadToRBreast < interactionDistance || playerHeadToPelvis < interactionDistance)
            {
                interestFace += 0.15f * uiInterestRate.val;
                interestArousal += 0.0012f * uiArousalSpeed.val;
                interestValence += 0.0022f * uiValenceSpeed.val;
                if (mainInterest == "Face"){fuzzyLock = 2.0f;}
				playerHeadInteract = true;
				//energyAmount += 1.0f;
				lBunnyHands = true;
				rBunnyHands = true;
				dbgHead += "+Interact ";
            }
            if (playerHeadMovement && headToFaceRot < lookPeripheralAngle)
            {
                interestFace += 0.025f * uiInterestRate.val;
                //interestValence += 0.25f;
                if (mainInterest == "Face"){fuzzyLock = 1.5f;}
				dbgHead += "+Move ";
            }
            if (playerHeadMovement == false)
            {
                interestFace -= 0.007f * uiInterestRate.val;
				dbgHead += "-NoMove ";
            }
            if (headToFaceRot > lookPeripheralAngle)// && playerHeadToHead > personalSpaceDistance)
            {
                interestFace -= 0.05f * uiInterestRate.val;
                if (mainInterest == "Face"){fuzzyLock = 2.0f;}
				dbgHead += "-HighAngle ";
            }
            if (playerHeadToHead < personalSpaceDistance)
            {
                interestFace += 0.005f * uiInterestRate.val;
				dbgHead += "+PSpace ";
            }
            if (playerHeadToHead > personalSpaceDistance && playerHeadToHead < backgroundDistance)
            {
				if (interestFace + headActivityBoost  < 41.0f)
				{
					interestFace += 0.01f * uiInterestRate.val;
					dbgHead += "+AwareDist ";
				}
				else
				{
					interestFace -= 0.02f * uiInterestRate.val;
					dbgHead += "-AwareDist ";
				}
            }
			if (Vector3.Distance(lHandController.followWhenOff.TransformPoint(new Vector3(-0.08f, 0.00f, 0.00f)), lBreastController.followWhenOff.position) < uiInteractDist.val || Vector3.Distance(lHandController.followWhenOff.TransformPoint(new Vector3(-0.08f, 0.00f, 0.00f)), rBreastController.followWhenOff.position) < uiInteractDist.val)
			{
				pLHandTouch = true;
				if (Vector3.Distance(lHandController.followWhenOff.TransformPoint(new Vector3(-0.08f, 0.00f, 0.00f)), prevPosLHand) > 0.0005f)
				{
					//interestFace += 0.015f * uiInterestRate.val;
					interestArousal += 0.0045f * uiArousalSpeed.val;
					interestValence += 0.0010f * uiValenceSpeed.val;
					interestPLHand += 0.5f;
					dbgPLHand += "+Touch Move ";
					if (currentLook == "Idle" && Random.Range(0.0f, 100.0f) > Mathf.Lerp(100.0f, 80.0f, Mathf.Clamp(interestArousal-5.0f, 0.0f, 1.0f)/5.0f))
					{
						lookSM.Switch(lFeel);
					}
				}
				else
				{
					interestFace += 0.015f * uiInterestRate.val;
					interestArousal += 0.0010f * uiArousalSpeed.val;
					interestValence += 0.0004f * uiValenceSpeed.val;
					interestPLHand += 0.015f;
					dbgPLHand += "+Touch ";
				}
			}
			if (currentInterest == "PLHand" && prevInterest == "PLHand")
			{
				interestPLHand -= 0.03f;
				dbgPLHand += "-Repeat ";
			}
			if (Vector3.Distance(rHandController.followWhenOff.TransformPoint(new Vector3(0.08f, 0.00f, 0.00f)), lBreastController.followWhenOff.position) < uiInteractDist.val || Vector3.Distance(rHandController.followWhenOff.TransformPoint(new Vector3(0.08f, 0.0f, 0.00f)), rBreastController.followWhenOff.position) < uiInteractDist.val)
			{
				pRHandTouch = true;
				if (Vector3.Distance(rHandController.followWhenOff.TransformPoint(new Vector3(0.08f, 0.00f, 0.00f)), prevPosRHand) > 0.0005f)
				{
					//interestFace += 0.015f * uiInterestRate.val;
					interestArousal += 0.0045f * uiArousalSpeed.val;
					interestValence += 0.0010f * uiValenceSpeed.val;
					interestPRHand += 0.5f;
					dbgPRHand += "+Touch Move ";
					if (currentLook == "Idle" && Random.Range(0.0f, 100.0f) > Mathf.Lerp(200, 80.0f, interestArousal/10.0f))
					{
						lookSM.Switch(lFeel);
					}
				}
				else
				{
					interestFace += 0.015f * uiInterestRate.val;
					interestArousal += 0.0010f * uiArousalSpeed.val;
					interestValence += 0.0004f * uiValenceSpeed.val;
					interestPRHand += 0.015f;
					dbgPRHand += "+Touch ";
				}
			}
			if (currentInterest == "PRHand" && prevInterest == "PRHand")
			{
				interestPRHand -= 0.03f;
				dbgPRHand += "-Repeat ";
			}
			if (pLHandTouch && pRHandTouch == false)
			{
				interestPRHand -= 0.06f;
				dbgPRHand += "-Other Hand Touch ";
			}
			if (pRHandTouch && pLHandTouch == false)
			{
				interestPLHand -= 0.06f;
				dbgPLHand += "-Other Hand Touch ";
			}
			if (pRHandTouch == false && mainInterest != "PRHand")
			{
				interestPRHand -= 0.1f;
				dbgPRHand += "-No Interest ";
			}
			if (pLHandTouch == false && mainInterest != "PLHand")
			{
				interestPLHand -= 0.10f;
				dbgPLHand += "-No Interest ";
			}
			
            if (playerHeadToHead > backgroundDistance)
            {
                interestFace -= 0.15f * uiInterestRate.val;
				dbgHead += "-FarOff ";
            }
            //interestFace -= (100.0f - pExtraversion) / 10.0f;
            //interestFace += (100.0f - pStableness) / 10.0f;

			//SuperController.LogError("Face Calc Done");
            if (playerHandsUsable || person2Usable)
            {

                bool interact = false;
                //Left Hand
                if ((mainInterest == "LHand" && mainOld == "LHand") && mainClock > gDuration)
                {
                    interestLHand -= 0.07f * uiInterestRate.val;
                    dbgLHand += "-Timeout ";
                }
                if (playerLHandToHead < closeFaceDistance && playerHandsUsable)// && playerLHandToHead > interactionDistance)
                {
                    interestLHand += 0.02f * uiInterestRate.val;
					interestFace -= 0.01f * uiInterestRate.val;
                    interestArousal += 0.00005f * uiArousalSpeed.val;
                    interestValence += 0.00015f * uiValenceSpeed.val;
					//energyAmount += 1.0f;
					rBunnyHands = true;
					lBunnyHands = false;
                    dbgLHand += "+FaceContact ";
                }
				tempFloat = 0.0f;
                if (playerLHandToHead < interactionDistance * 1.5f || playerLHandToLBreast < interactionDistance || playerLHandToRBreast < interactionDistance || playerLHandToPelvis < interactionDistance * 2.0f)
                {
					interestArousal += 0.0033f * uiArousalSpeed.val;
					interestValence += 0.00155f * uiValenceSpeed.val;
					if (playerLHandMovement && playerHandsUsable)
					{
						interestLHand += 0.35f * uiInterestRate.val;
						interestArousal += 0.0063f * uiArousalSpeed.val;
						interestValence += 0.00255f * uiValenceSpeed.val;
						if (playerLHandToLBreast < interactionDistance)
						{
							lBunnyHands = true;
						}
						if (playerRHandToLBreast < interactionDistance)
						{
							rBunnyHands = true;
						}
						tempFloat = 1.0f;
					}
					else
					{
						interestLHand -= 0.05f;
					}
					if (playerLHandToPelvis < interactionDistance * 1.3f && testRun == false && playerHandsUsable)
					{
						dbgLHand += "+Sex ";
						interestArousal += 0.06f * uiArousalSpeed.val;
						if (playerLHandMovement && interestArousal > 5.0f && currentLook != "Feel" && currentLook != "Sex")
						{
							lookSM.Switch(lFeel);
						}
							
					}
					else
					{
						interestArousal += 0.0001f * uiArousalSpeed.val;
					}
					interestFace -= 0.1f * uiInterestRate.val;
					interestRHand -= 0.02f * uiInterestRate.val;
					gHeadSpeed = 0.5f;
					playerLHandInteract = true;
					if (playerLHandFirstInteract && playerLHandMovement)
					{
						playerLHandFirstInteract = false;
						playerLHandFirstTimeout = interactTimeout;
						if (interestLHand < 90.0f)
						{
							interestLHand = 99.0f;
						}
						interestFace -= 30.0f;
						mainInterest = "LHand";
						interestClock = -1.0f;
						//SuperController.LogError("L Hand Triggered", false);
						
					}
                    if (mainInterest == "LHand"){fuzzyLock = 1.5f;}
					//energyAmount += 1.0f;
                    dbgLHand += "+Interact ";
                }
				
				if (playerHeadToFaceRot < playerLookDirectAngle && headToFaceRot < lookDirectAngle && playerHeadToHead < personalSpaceDistance && interestLHand + lHandActivityBoost > 40.0f)
				{
					interestLHand -= 0.53f * uiInterestRate.val;
					dbgLHand += "-EyeToEye ";
				}
				if (playerHeadToFaceRot < playerLookDirectAngle)
				{
					interestLHand -= 0.025f * uiInterestRate.val;
					dbgLHand += "-PLookFace ";
				}

                if (playerLHandToLHand < interactionDistance)
                {
                    mLHandFistTarget = Random.Range(0.5f, 0.8f);
                }
                if (playerLHandToRHand < interactionDistance)
                {
                    mRHandFistTarget = Random.Range(0.5f, 0.8f);
                }
                if (playerLHandMovement && headToLHandRot < lookNoAwarenessAngle && playerHandsUsable)
                {
                    interestLHand += 0.04f * (playerLHandTimeout / movementMaxTimeout) * uiInterestRate.val;
                    dbgLHand += "+Move ";
                    if (headToLHandRot > lookPeripheralAngle)
                    {
                        interestLHand += 0.01f * (playerLHandTimeout / movementMaxTimeout) * uiInterestRate.val;
                        dbgLHand += "+MoveNoVis ";
                    }
                }
                if (playerLHandMovement == false)
                {
                    interestLHand -= 0.125f * (1.0f - (playerLHandTimeout / movementMaxTimeout)) * uiInterestRate.val;
                    dbgLHand += "-NoMove ";
                }
                if (playerToPLHand < lookDirectAngle && headToLHandRot < lookNoAwarenessAngle && playerHandsUsable)
                {
                    interestLHand += 0.033f * uiInterestRate.val;
                    if (mainInterest == "LHand"){fuzzyLock = 0.5f;}
                    dbgLHand += "+PLookAt ";
                }
                if (headToLHandRot > lookNoAwarenessAngle)
                {
                    interestLHand -= 0.1f * uiInterestRate.val;
                    if (mainInterest == "LHand"){fuzzyLock = 2.0f;}
                    dbgLHand += "-HighAngle ";
                }
                if (playerLHandToHead < personalSpaceDistance && playerHandsUsable)
                {
                    interestLHand += 0.005f * uiInterestRate.val;
                    dbgLHand += "+PSpace ";
                }
                if (mainInterest == "RHand")
                {
                    interestLHand -= 0.01f * uiInterestRate.val;
                    dbgLHand += "-OtherHand ";
                }
                if (mainOld == "LHand")
                {
                    interestLHand -= 0.04f * uiInterestRate.val;
                    dbgLHand += "-Repeat ";
                }
				
                if (playerLHandToHead > playerHeadToHead)
                {
                    interestLHand -= 0.03f * uiInterestRate.val;
                    dbgLHand += "-HeadCloser ";
                }
                if (playerLHandToHead > personalSpaceDistance && playerLHandToHead < backgroundDistance && playerHandsUsable)
                {
					if (interestLHand + lHandActivityBoost < 40.0f)
					{
						interestLHand += 0.01f * uiInterestRate.val;
						dbgLHand += "+AwareDist ";
					}
					else
					{
						interestLHand -= 0.01f * uiInterestRate.val;
						dbgLHand += "-AwareDist ";
					}
                }
                if (playerLHandToHead > backgroundDistance)
                {
                    interestLHand -= 0.25f * uiInterestRate.val;
                    dbgLHand += "-FarOff ";
                }
                if (usePerson2 == false && playerHandsUsable == false)
                {
                    interestLHand -= 1000.0f;
                    dbgLHand += "-NoHands ";
                }
                //interestLHand -= ((100.0f - pStableness) / 1000.0f) * uiInterestRate.val;
                //interestLHand += ((100.0f - pExtraversion) / 1000.0f) * uiInterestRate.val;

				//SuperController.LogError("Left Hand Done");
                //Right Hand
                interact = false;
                if ((mainInterest == "RHand" && mainOld == "RHand") && mainClock > gDuration)
                {
                    interestRHand -= 0.07f * uiInterestRate.val;
                    dbgRHand += "-Timeout ";
                }
                if (playerRHandToHead < closeFaceDistance && playerHandsUsable)// && playerRHandToHead > interactionDistance)
                {
                    interestRHand += 0.02f * uiInterestRate.val;
					interestFace -= 0.01f * uiInterestRate.val;
                    interestArousal += 0.00005f * uiArousalSpeed.val;
                    interestValence += 0.00015f * uiValenceSpeed.val;
					//energyAmount += 1.0f;
					rBunnyHands = false;
					lBunnyHands = true;
                    dbgRHand += "+FaceContact ";
                }
				tempFloat = 0.0f;
                if (playerRHandToHead < interactionDistance || playerRHandToLBreast < interactionDistance || playerRHandToRBreast < interactionDistance || playerRHandToPelvis < interactionDistance * 1.3f)
                {
						interestArousal += 0.0033f * uiArousalSpeed.val;
						interestValence += 0.00155f * uiValenceSpeed.val;
					if (playerRHandMovement && playerHandsUsable)
					{
						interestRHand += 0.35f * uiInterestRate.val;
						interestArousal += 0.0063f * uiArousalSpeed.val;
						interestValence += 0.00255f * uiValenceSpeed.val;
						if (playerRHandToLBreast < interactionDistance)
						{
							lBunnyHands = true;
						}
						if (playerLHandToLBreast < interactionDistance)
						{
							rBunnyHands = true;
						}
						tempFloat = 1.0f;
					}
					else
					{
						interestRHand -= 0.05f * uiInterestRate.val;
					}
					if (playerRHandToPelvis < interactionDistance * 1.3f && playerHandsUsable)
					{
						dbgRHand += "+Sex ";
						interestArousal += 0.01f * uiArousalSpeed.val;
						if (playerRHandMovement && interestArousal > 5.0f && currentLook != "Feel" && currentLook != "Sex" && testRun == false && currentInterest != "Face")
						{
							lookSM.Switch(lFeel);
						}
					}
					else
					{
						interestArousal += 0.0001f * uiArousalSpeed.val;
					}
					interestFace -= 0.1f * uiInterestRate.val;
					interestLHand -= 0.02f * uiInterestRate.val;
                    if (mainInterest == "RHand"){fuzzyLock = 1.5f;}
					gHeadSpeed = 0.5f;
					playerRHandInteract = true;
					if (playerRHandFirstInteract && playerRHandMovement)
					{
						playerRHandFirstInteract = false;
						playerRHandFirstTimeout = interactTimeout;
						if (interestRHand < 90.0f)
						{
							interestRHand = 99.0f;
						}
						interestFace -= 30.0f;
						mainInterest = "RHand";
						interestClock = -1.0f;
						//SuperController.LogError("R Hand Triggered", false);
						
					}
					//energyAmount += 1.0f;
                    dbgRHand += "+Interact ";
                }

				if (playerHeadToFaceRot < playerLookDirectAngle && headToFaceRot < lookDirectAngle && playerHeadToHead < personalSpaceDistance && interestRHand + rHandActivityBoost > 40.0f)
				{
					interestRHand -= 0.53f * uiInterestRate.val;
					dbgRHand += "-EyeToEye ";
				}
				if (playerHeadToFaceRot < playerLookDirectAngle)
				{
					interestRHand -= 0.025f * uiInterestRate.val;
					dbgRHand += "-PLookFace ";
				}
                if (playerRHandToRHand < interactionDistance)
                {
                    mRHandFistTarget = Random.Range(0.5f, 0.8f);
                }
                if (playerRHandToRHand < interactionDistance)
                {
                    mRHandFistTarget = Random.Range(0.5f, 0.8f);
                }
                if (playerRHandMovement && headToRHandRot < lookNoAwarenessAngle && playerHandsUsable)
                {
                    interestRHand += 0.04f * (playerRHandTimeout / movementMaxTimeout) * uiInterestRate.val;
                    dbgRHand += "+Move ";
                    if (headToRHandRot > lookPeripheralAngle)
                    {
                        interestRHand += 0.01f * (playerRHandTimeout / movementMaxTimeout) * uiInterestRate.val;
                        dbgRHand += "+MoveNoVis ";
                    }
                }
                if (playerRHandMovement == false)
                {
                    interestRHand -= 0.125f * (1.0f - (playerRHandTimeout / movementMaxTimeout)) * uiInterestRate.val;
                    dbgRHand += "-NoMove ";
                }
                if (playerToPRHand < lookDirectAngle && headToRHandRot < lookNoAwarenessAngle && playerHandsUsable)
                {
                    interestRHand += 0.033f * uiInterestRate.val;
                    if (mainInterest == "RHand"){fuzzyLock = 0.5f;}
                    dbgRHand += "+PLookAt ";
                }
                if (headToRHandRot > lookNoAwarenessAngle)
                {
                    interestRHand -= 0.1f * uiInterestRate.val;
                    if (mainInterest == "RHand"){fuzzyLock = 2.0f;}
                    dbgRHand += "-HighAngle ";
                }
                if (playerRHandToHead < personalSpaceDistance && playerHandsUsable)
                {
                    interestRHand += 0.005f * uiInterestRate.val;
                    dbgRHand += "+PSpace ";
                }
                if (mainInterest == "RHand")
                {
                    interestRHand -= 0.01f * uiInterestRate.val;
                    dbgRHand += "-OtherHand ";
                }
                if (mainOld == "RHand")
                {
                    interestRHand -= 0.02f * uiInterestRate.val;
                    dbgRHand += "-Repeat ";
                }
				
                if (playerRHandToHead > playerHeadToHead)
                {
                    interestRHand -= 0.03f * uiInterestRate.val;
                    dbgRHand += "-HeadCloser ";
                }
                if (playerRHandToHead > personalSpaceDistance && playerRHandToHead < backgroundDistance && playerHandsUsable)
                {
					if (interestRHand + rHandActivityBoost < 40.0f)
					{
						interestRHand += 0.01f * uiInterestRate.val;
						dbgRHand += "+AwareDist ";
					}
					else
					{
						interestRHand -= 0.01f * uiInterestRate.val;
						dbgRHand += "-AwareDist ";
					}
                }
                if (playerRHandToHead > backgroundDistance)
                {
                    interestRHand -= 0.25f * uiInterestRate.val;
                    dbgRHand += "-FarOff ";
                }
                if (usePerson2 == false && playerHandsUsable == false)
                {
                    interestRHand -= 1000.0f;
                    dbgRHand += "-NoHands ";
                }
                //interestRHand -= ((100.0f - pStableness) / 1000.0f) * uiInterestRate.val;
                //interestRHand += ((100.0f - pExtraversion) / 1000.0f) * uiInterestRate.val;

                if (interestLHand == interestRHand)
                {
                    if (playerLHandToHead < playerRHandToHead)
                    {
                        interestRHand -= 1.0f;
                    }
                    else
                    {
                        interestLHand -= 1.0f;
                    }
                }
				//SuperController.LogError("Right Hand Done");
            }
            if (person2Usable)
            {
                //pelvis
                //interestPelvis = interestPelvisBase;
                if ((mainInterest == "Pelvis" && mainOld == "Pelvis") && mainClock > gDuration)
                {
                    interestPelvis -= 0.02f * uiInterestRate.val;
					dbgPenis += "-Timeout ";
                }
                if (mainInterest == "Tip")
                {
                    interestPelvis -= 0.01f * uiInterestRate.val;
					dbgPenis += "-TipFirst ";
                }
                if (playerPelvisToHead < closeFaceDistance * 2.0f)
                {
                    interestPelvis += 0.02f * uiInterestRate.val;
                    //interestArousal += 2.0f;
					playerPenisInteract = true;
					//energyAmount += 1.0f;
                    if (mainInterest == "Pelvis"){fuzzyLock = 0.5f;}
					dbgPenis += "+FaceInteract ";
                }
                if (playerHeadToHead < personalSpaceDistance / 2.0f && playerHeadToHead - 0.25f < playerPelvisToHead)
                {
                    interestPelvis -= 0.02f * uiInterestRate.val;
                    if (mainInterest == "Pelvis"){fuzzyLock = 1.5f;}
					dbgPenis += "-HeadCloser ";
                }
                if ((playerTipToPelvis < interactionDistance * 1.0f && mainInterest != "Tip") && uiDoSex.val == true)
                {
                    //interestArousal += 2.0f;
                    interestPelvis += 0.1f * uiInterestRate.val;
                    interestTip += 0.1f * uiInterestRate.val;
                    if (mainInterest == "Pelvis"){fuzzyLock = 1.5f;}
					//energyAmount += 1.0f;
					if (lSideHands || rSideHands || lBellyHands || rBellyHands || lBackArchHands || rBackArchHands)
					{
						lBunnyHands = false;
						rBunnyHands = false;
						lBellyHands = false;
						rBellyHands = false;
						lBreastHands = false;
						rBreastHands = false;
						lFancyHands = false;
						rFancyHands = false;
						lHipHands = false;
						rHipHands = false;
						lBackArchHands = false;
						rBackArchHands = false;

						lSideHands = true;
						rSideHands = true;
					}
					dbgPenis += "+SexActPelvis ";
                }
                if (playerPelvisToHead < personalSpaceDistance)
                {
                    interestPelvis += 0.01f * uiInterestRate.val;
					dbgPenis += "+PSpace ";
                }
                if (playerPelvisToHead > personalSpaceDistance && playerPelvisToHead < backgroundDistance)
                {
                    interestPelvis -= 0.01f * uiInterestRate.val;
					dbgPenis += "-Aware ";
                }
                if (playerPelvisToHead > backgroundDistance)
                {
                    interestPelvis -= 0.25f * uiInterestRate.val;
					dbgPenis += "-FarOff ";
                }
				if (playerHeadToFaceRot < playerLookDirectAngle && playerTipToHead > closeFaceDistance)
				{
					interestPelvis -= 0.02f * uiInterestRate.val;
					dbgPenis += "-LookAtFace ";
				}
				if (interestArousal < 6.0f)
				{
					interestPelvis -= 0.04f * uiInterestRate.val;
					dbgPenis += "-NotAroused ";
				}
                //interestPelvis -= ((100.0f - pAgreeableness) / 1000.0f) * uiInterestRate.val;
                //interestPelvis += ((pExtraversion - 50.0f) / 1000.0f) * uiInterestRate.val;

				//SuperController.LogError("Pelvis Done");
                //Penis
                //interestTip = interestTipBase;
                if ((mainInterest == "Penis" || mainOld == "Penis") && mainClock > gDuration && playerTipToHead > closeFaceDistance && playerTipToLBreast > closeFaceDistance && playerTipToRBreast > closeFaceDistance && playerTipToPelvis > closeFaceDistance * 3 && playerTipToLHand > interactionDistance && playerTipToRHand > interactionDistance)
                {
                    interestTip -= 0.002f * uiInterestRate.val;
					dbgPenis += "-Timeout ";
                }
                if (playerTipToHead < closeFaceDistance * 3.0f)
                {
                    //interestArousal += 2.0f;
                    interestTip += 1.0f * uiInterestRate.val;
                    if (mainInterest == "Penis"){fuzzyLock = 0.5f;}
					dbgPenis += "+CloseToFace ";
                }
                if (playerTipToHead < interactionDistance || playerTipToLBreast < interactionDistance || playerTipToRBreast < interactionDistance || playerTipToPelvis < interactionDistance)
                {
                    //interestArousal += 2.0f;
                    interestTip += 0.5f * uiInterestRate.val;
					interestArousal += 0.01f;
                    if (mainInterest == "Penis"){fuzzyLock = 0.25f;}
					playerPenisInteract = true;
					if (playerPenisFirstInteract)
					{
						playerPenisFirstInteract = false;
						playerPenisFirstTimeout = interactTimeout;
						if (interestTip < 90.0f)
						{
							interestTip = 99.0f;
						}
						interestFace -= 30.0f;
						mainInterest = "Tip";
						interestClock = -1.0f;
						
					}
					//energyAmount += 1.0f;
					dbgPenis += "+Interact ";
                }
                if (playerTipMovement && headToFaceRot > lookPeripheralAngle)
                {
                    interestTip += 0.03f * uiInterestRate.val;
					dbgPenis += "+Move ";
                }
                if (playerTipMovement == false && playerTipToHead > closeFaceDistance)
                {
                    interestTip -= 0.01f * uiInterestRate.val;
                    if (mainInterest == "Penis"){fuzzyLock = 1.5f;}
					dbgPenis += "-NoMove ";
                }
                if (playerTipToHead < personalSpaceDistance)
                {
                    interestTip += 0.03f * uiInterestRate.val;
					dbgPenis += "+PSpace ";
                }
                if ((playerTipToHead < playerLHandToHead && playerTipToHead < playerRHandToHead && (playerTipToPelvis < interactionDistance * 2 || playerTipToLBreast < interactionDistance || playerTipToRBreast < interactionDistance) && person2IsMale) && uiDoSex.val == true)
                {
                    interestTip += 2.0f * uiInterestRate.val;
                    interestPelvis += 0.3f * uiInterestRate.val;
                    interestLHand -= 0.03f * uiInterestRate.val;
                    interestRHand -= 0.03f * uiInterestRate.val;
					interestArousal += 0.03f;
                    if (mainInterest == "Penis"){fuzzyLock = 0.5f;}
					//energyAmount += 1.0f;
					dbgPenis += "+SexAction ";
                }
                if (playerTipToHead > personalSpaceDistance && playerTipToHead < backgroundDistance)
                {
					if (interestTip < 40.5f)
					{
						interestTip += 0.01f * uiInterestRate.val;
						dbgPenis += "+Aware ";
					}
					else
					{
						interestTip -= 0.01f * uiInterestRate.val;
						dbgPenis += "-Aware ";
					}
                }
                if (playerTipToHead > backgroundDistance)
                {
                    interestTip -= 1.0f * uiInterestRate.val;
					dbgPenis += "-FarOff ";
                }
				if (playerHeadToFaceRot < playerLookDirectAngle && playerTipToHead > closeFaceDistance)
				{
					interestTip -= 0.05f * uiInterestRate.val;
					dbgPenis += "-LookAtFace ";
				}
                if ((playerTipToHead < interactionDistance * 1.5f || playerTipToLHand < interactionDistance * 1.5f || playerTipToRHand < interactionDistance * 1.5f) && person2IsMale == true && playerHeadToHead > personalSpaceDistance/2.0f)
                {
                    interestTip += 1.0f * uiInterestRate.val;
                    interestPelvis += 0.3f * uiInterestRate.val;
					interestArousal += 0.03f;
                    if (mainInterest == "Penis"){fuzzyLock = 0.5f;}
					//energyAmount += 1.0f;
					dbgPenis += "+SexInteract ";
                }
                else
                {
                    interestTip -= (pStableness) / 100.0f;
                }
				if (interestArousal < 7.0f && playerTipToHead > personalSpaceDistance)
				{
					interestTip -= 0.7f * uiInterestRate.val;
					dbgPenis += "-NotAroused ";
				}

				//SuperController.LogError("Penis Done");
                //interestTip += (pExtraversion - 50.0f) / 500.0f;
            }
			
			//interestEMTarget = interestEMTargetBase;
			if (emTargetName != "None")
			{
				
				if (emTargetMovement)
				{
					interestEMTarget += 0.07f * Mathf.Clamp(emTargetTimeout / movementMaxTimeout,0.0f,2.0f) * uiInterestRate.val;
					dbgObject += "+Move ";
				}
				if (emTargetDistance < personalSpaceDistance)
				{
					interestEMTarget += 0.04f * uiInterestRate.val;
					if (uiTargetLook.val == false)
					{
						interestEMTarget += 0.01f * uiInterestRate.val;
					}
					dbgObject += "+PSpace ";
				}
				else
				{
					interestEMTarget -= 0.1f * uiInterestRate.val;
				}
				if (emTargetDistance < closeFaceDistance * 2.0f)
				{
					interestEMTarget += 0.07f * uiInterestRate.val;
					if (uiTargetLook.val == false)
					{
						interestEMTarget += 0.03f * uiInterestRate.val;
					}
					dbgObject += "+Close ";
				}
				else
				{
					interestEMTarget -= 0.1f * uiInterestRate.val;
				}
				if (emTargetDir < lookPeripheralAngle)
				{
					interestEMTarget += 0.06f * uiInterestRate.val;
					dbgObject += "+Visible ";
				}
				else
				{
					interestEMTarget -= 0.01f * uiInterestRate.val;
				}
				if (emTargetPelvisDistance < interactionDistance && vagTouchCount > 0.0f)
				{
					interestEMTarget += 0.1f * uiInterestRate.val;
					//energyAmount += 1.0f;
					dbgObject += "+Sex ";
				}
				if (emTargetHeadDir < eyesNonDirectAngle && uiTargetLook.val)
				{
					interestEMTarget += 0.3f * uiInterestRate.val;
					if (emTargetDistance < personalSpaceDistance)
					{
						interestEMTarget += 0.2f * uiInterestRate.val;
					}
					dbgObject += "+Looking ";
				}
				else
				{
					interestEMTarget -= 0.01f * uiInterestRate.val;
				}
				if (mainOld == "Target")
				{
					interestEMTarget -= 0.03f * uiInterestRate.val;
					dbgObject += "+Previous ";
				}
				if (headToFaceRot < lookDirectAngle)
				{
					interestEMTarget -= 0.05f * uiInterestRate.val;
					dbgObject += "-LookingAtPlayer ";
					if (playerHeadToFaceRot < playerLookDirectAngle && amGlancing == false && gAvoid == 0.0f)
					{
						interestEMTarget -= 0.3f * uiInterestRate.val;
						dbgObject += "-EyeToEye ";
					}
				}
			}
			else
			{
				interestEMTarget = 0.0f;
			}
				float lhandangle = Vector3.Angle(lHandController.followWhenOff.position - headController.followWhenOff.position, headController.followWhenOff.forward);
				float rhandangle = Vector3.Angle(rHandController.followWhenOff.position - headController.followWhenOff.position, headController.followWhenOff.forward);
				//SuperController.LogError(lhandangle.ToString() + "/" + rhandangle.ToString());
			
			//if (Mathf.Max(interestFace, interestLHand, interestRHand, interestPelvis) < 50.0f)
			//{
				if (lhandangle < lookPeripheralAngle)
				{
					interestPLHand += 0.005f;
					dbgPLHand += "+Visible ";
				}
				else
				{
					interestPLHand -= 0.25f;
					dbgPLHand += "-Not Visible ";
				}
				if (rhandangle < lookPeripheralAngle)
				{
					interestPRHand += 0.005f;
					dbgPRHand += "+Visible ";
				}
				else
				{
					interestPRHand -= 0.25f;
					dbgPRHand += "-Not Visible ";
				}
			//}
			/*else
			{
				interestPLHand -= 0.5f;
				interestPRHand -= 0.5f;
			}*/
			//SuperController.LogError("Target Object Done");
			
			if (interestArousal > 6.0f && interestValence < 5.0f)
			{
				interestArousal -= 0.00025f / uiArousalSpeed.val;
			}
			if (interestValence > 8.0f && interestArousal < 6.0f)
			{
				interestValence -= 0.00065f / uiValenceSpeed.val;
			}
      if (interestArousal > 9.0f)
      {
        interestArousal -= 0.00075f / uiArousalSpeed.val;
      }
			
				interestFace = Mathf.Clamp(interestFace, (interestFaceBase/2.0f) * uiHeadInterest.val, (maxInterestLevel) * uiHeadInterest.val);
				interestLHand = Mathf.Clamp(interestLHand, (interestLHandBase/2.0f) * uiLHandInterest.val, (maxInterestLevel - 1.0f) * uiLHandInterest.val);
				interestRHand = Mathf.Clamp(interestRHand, (interestRHandBase/2.0f) * uiRHandInterest.val, (maxInterestLevel - 1.0f) * uiRHandInterest.val);
				interestPelvis = Mathf.Clamp(interestPelvis, (interestPelvisBase/2.0f) * uiPenisInterest.val, (maxInterestLevel + 1.0f) * uiPenisInterest.val);
				interestTip = Mathf.Clamp(interestTip, (interestTipBase/2.0f) * uiPenisInterest.val, (maxInterestLevel + 4.0f) * uiPenisInterest.val);
				interestEMTarget = Mathf.Clamp(interestEMTarget, (interestEMTargetBase/2.0f) * uiObjectInterest.val, (maxInterestLevel + 5.0f) * uiObjectInterest.val);
				
				interestPLHand = Mathf.Clamp(interestPLHand, (interestPLHandBase/2.0f) * uiSelfLHandInterest.val, (maxInterestLevel - 1.0f));
				interestPRHand = Mathf.Clamp(interestPRHand, (interestPRHandBase/2.0f) * uiSelfRHandInterest.val, (maxInterestLevel - 1.0f));
				
			
			//SuperController.LogError("Interest Calc Done (" + interestClock.ToString() + ")");

			
				//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
				//float leftright = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
				
				tempFloat = uiIdleChance.val / 50.0f;

				if (Mathf.Abs(twistActual - twistTarget) < 1.0f && Random.Range(0.0f,100.0f) < tempFloat  && idleBodyTimeout <= 0.0f)
				{
					twistTarget = Random.Range(-30.0f,30.0f);
					idleBodyTimeout = uiIdleBodyDelay.val;
				}
				if (Mathf.Abs(twist2Actual - twist2Target) < 1.0f && Random.Range(0.0f,100.0f) < tempFloat  && idleBodyTimeout <= 0.0f)
				{
					twist2Target = Random.Range(-60.0f,60.0f);
					idleBodyTimeout = uiIdleBodyDelay.val;
				}

				//SuperController.LogError("Twist Target");
				if (headLeftRight > lookDirectAngle && Random.Range(0.0f,100.0f) < tempFloat && Mathf.Abs(twistActual - twistTarget) < 1.0f && idleBodyTimeout <= 0.0f)
				{
					//twistTarget = Random.Range(10.0f,-70.0f);
					twistTarget = Random.Range(-30.0f,110.0f);
					//SuperController.LogError("Twist Idle " + twistTarget.ToString());
					idleBodyTimeout = uiIdleBodyDelay.val;
				}
				if (headLeftRight < -lookDirectAngle && Random.Range(0.0f,100.0f) < tempFloat && Mathf.Abs(twistActual - twistTarget) < 1.0f && idleBodyTimeout <= 0.0f)
				{
					//twistTarget = Random.Range(-10.0f,70.0f);
					twistTarget = Random.Range(30.0f,-110.0f);
					//SuperController.LogError("Twist Idle " + twistTarget.ToString());
					idleBodyTimeout = uiIdleBodyDelay.val;
				}
				
				if (Mathf.Abs(rElbowActual - rElbowTarget) < 10.0f && idleRArmTimeout <= 0.0f)
				{
					//SuperController.LogError("Elbow Targets");
					if ((Random.Range(0.0f,100.0f) < uiIdleArmChance.val / 50.0f || (Mathf.Abs(lElbowActual) > -90.0f && Mathf.Abs(lElbowActual) < -20.0f && Random.Range(0.0f,100.0f) < (uiIdleArmChance.val / 50.0f) * 2.0f)) && Mathf.Abs(lElbowActual - lElbowTarget) < 20.0f)
					{
						idleRArmTimeout = uiIdleArmDelay.val;
						tempFloat = lElbowTarget;
						lElbowTarget = Random.Range(10.0f,-100.0f);
						if (Mathf.Abs(tempFloat - lElbowTarget) < 20.0f)
						{
							lElbowTarget = Random.Range(10.0f,-100.0f);
						}
						if (Mathf.Abs(tempFloat - lElbowTarget) < 10.0f)
						{
							lElbowTarget = Random.Range(10.0f,-100.0f);
						}
						if (lElbowActual > -80.0f && lElbowActual < -20.0f)
						{
							if (Mathf.Abs(lElbowActual - lElbowTarget) < 50.0f)
							{
								lElbowTarget = Random.Range(10.0f,-10.0f);
							}
						}
						if (interestKissing)
						{
							lElbowTarget = Random.Range(10.0f,-10.0f);
							lElbowHoldTarget = 0.2f;
						}
						if ((interestArousal > 4.0f && (playerLHandToLBreast < interactionDistance || playerRHandToLBreast < interactionDistance || playerHeadToLBreast < interactionDistance*2.0f)) || lBunnyHands)
						{
							lElbowTarget = Random.Range(-100.0f,-130.0f);
							lElbowHoldTarget = 0.8f;
						}
						//SuperController.LogError("Left Elbow Idle " + lElbowTarget.ToString() + " / " + lElbowActual.ToString());
					}
				}
				if (Mathf.Abs(lElbowActual - lElbowTarget) < 10.0f && idleRArmTimeout <= 0.0f)
				{
					if ((Random.Range(0.0f,100.0f) < uiIdleArmChance.val / 50.0f || (Mathf.Abs(rElbowActual) < 90.0f && Mathf.Abs(rElbowActual) > 20.0f && Random.Range(0.0f,100.0f) < (uiIdleArmChance.val / 50.0f) * 2.0f)) && Mathf.Abs(rElbowActual - rElbowTarget) < 20.0f)
					{
						idleRArmTimeout = uiIdleArmDelay.val;
						tempFloat = rElbowTarget;
						rElbowTarget = Random.Range(-10.0f,100.0f);
						if (Mathf.Abs(tempFloat - rElbowTarget) < 20.0f)
						{
							rElbowTarget = Random.Range(-10.0f,100.0f);
						}
						if (Mathf.Abs(tempFloat - rElbowTarget) < 10.0f)
						{
							rElbowTarget = Random.Range(-10.0f,100.0f);
						}
						if (rElbowActual < 80.0f && rElbowActual > 20.0f)
						{
							if (Mathf.Abs(rElbowActual - rElbowTarget) < 50.0f)
							{
								rElbowTarget = Random.Range(-10.0f,10.0f);
							}
						}
						if (interestKissing)
						{
							rElbowTarget = Random.Range(-10.0f,10.0f);
							rElbowHoldTarget = 0.2f;
						}
						if ((interestArousal > 4.0f && (playerLHandToRBreast < interactionDistance || playerRHandToRBreast < interactionDistance || playerHeadToRBreast < interactionDistance*2.0f)) || rBunnyHands)
						{
							rElbowTarget = Random.Range(100.0f,130.0f);
							rElbowHoldTarget = 0.8f;
						}
						//SuperController.LogError("Right Elbow Idle " + rElbowTarget.ToString());
					}
				}
				//SuperController.LogError("Thigh Targets");
				if ((Random.Range(0.0f,100.0f) < tempFloat || (Mathf.Abs(lThighActual) > 60.0f && Random.Range(0.0f,100.0f) < tempFloat * 10.0f)) && Mathf.Abs(lThighActual - lThighTarget) < 5.0f && idleLLegTimeout <= 0.0f)
				{
					lThighTarget = Random.Range(-20.0f,100.0f);
					idleLLegTimeout = uiIdleLegDelay.val;
					//SuperController.LogError("Left Thigh Idle " + lThighTarget.ToString());
				}
				if ((Random.Range(0.0f,100.0f) < tempFloat || (Mathf.Abs(rThighActual) > 60.0f && Random.Range(0.0f,100.0f) < tempFloat * 10.0f)) && Mathf.Abs(rThighActual - rThighTarget) < 5.0f && idleLLegTimeout <= 0.0f)
				{
					rThighTarget = Random.Range(-20.0f,100.0f);
					idleLLegTimeout = uiIdleLegDelay.val;
					//SuperController.LogError("Right Thigh Idle " + rThighTarget.ToString());
				}

				//SuperController.LogError("Knee Targets");
				if ((Random.Range(0.0f,100.0f) < tempFloat || (Mathf.Abs(lKneeTarget) > 80.0f && Random.Range(0.0f,100.0f) < tempFloat * 10.0f)) && Mathf.Abs(lKneeActual - lKneeTarget) < 5.0f && idleLLegTimeout <= 0.0f)
				{
					idleLLegTimeout = uiIdleLegDelay.val;
					if (lKneeTarget < -25.0f)
					{
						lKneeTarget = Random.Range(0.0f,-25.0f);
					}
					else
					{
						lKneeTarget = Random.Range(0.0f,-130.0f);
					}
					//SuperController.LogError("Left Knee Idle " + lKneeTarget.ToString());
				}
				
				if ((Random.Range(0.0f,100.0f) < tempFloat || (Mathf.Abs(rKneeTarget) > 80.0f && Random.Range(0.0f,100.0f) < tempFloat * 10.0f)) && Mathf.Abs(rKneeActual - rKneeTarget) < 5.0f && idleRLegTimeout <= 0.0f)
				{
					idleRLegTimeout = uiIdleLegDelay.val;
					if (rKneeTarget < -25.0f)
					{
						rKneeTarget = Random.Range(0.0f,-25.0f);
					}
					else
					{
						rKneeTarget = Random.Range(0.0f,-130.0f);
					}
					//SuperController.LogError("Left Knee Idle " + rKneeTarget.ToString());
				}
				
				
				//SuperController.LogError("Thigh Spread Check " + interactionDistance);
				if (Mathf.Min(	Vector3.Distance(playerHeadTransform.position, pelvisController.followWhenOff.position),
								Vector3.Distance(playerLHandTransform.position, pelvisController.followWhenOff.position),
								Vector3.Distance(playerRHandTransform.position, pelvisController.followWhenOff.position),
								Vector3.Distance(lHandController.followWhenOff.position, pelvisController.followWhenOff.position),
								Vector3.Distance(rHandController.followWhenOff.position, pelvisController.followWhenOff.position) ) < interactionDistance
								&& ((Mathf.Abs(lThighActual - lThighTarget) < 5.0f || lThighTarget < 50.0f) || (Mathf.Abs(rThighActual - lThighTarget) < 5.0f || rThighTarget > -50.0f)))
				{
					//SuperController.LogError("Opening Legs for touch");
					lThighTarget = Mathf.Lerp(lThighTarget, Random.Range(50.0f, 70.0f), interestArousal/10.0f);
					lThighTarget = Mathf.Lerp(rThighTarget, Random.Range(-50.0f, -70.0f), interestArousal/10.0f);
					//idleLegTimeout = uiIdleLegDelay.val;
				}
				else
				{
					if (emTargetName != "None")
					{
						if (Vector3.Distance(emTargetTransform.position, pelvisController.followWhenOff.position) < interactionDistance && ((Mathf.Abs(lThighActual - lThighTarget) < 5.0f || lThighTarget < 50.0f) || (Mathf.Abs(rThighActual - lThighTarget) < 5.0f || rThighTarget > -50.0f)))
						{
							lThighTarget = Mathf.Lerp(lThighTarget, Random.Range(50.0f, 70.0f), interestArousal/10.0f);
							rThighTarget = Mathf.Lerp(rThighTarget, Random.Range(-50.0f, -70.0f), interestArousal/10.0f);
							//SuperController.LogError("Opening Legs for target");
						}
					}
					if (usePerson2)
					{
						if (Vector3.Distance(playerPelvis, pelvisController.followWhenOff.position) < interactionDistance * 5.0f && Vector3.Angle(abdomenController.followWhenOff.forward, Vector3.up) < 30.0f && ((Mathf.Abs(lThighActual - lThighTarget) < 5.0f || lThighTarget < 50.0f) || (Mathf.Abs(rThighActual - lThighTarget) < 5.0f || rThighTarget > -50.0f)))
						{
							lThighTarget = Mathf.Lerp(lThighTarget, Random.Range(50.0f, 70.0f), interestArousal/10.0f);
							rThighTarget = Mathf.Lerp(rThighTarget, Random.Range(-50.0f, -70.0f), interestArousal/10.0f);
							//SuperController.LogError("Opening Legs for pelvis");
						}
					}
				}

				//SuperController.LogError("lElbow Hold");
				if (Mathf.Abs(lElbowHoldActual - lElbowHoldTarget) < 0.01f && Random.Range(0.0f,100.0f) < uiIdleArmChance.val / 50.0f && idleLArmTimeout <= 0.0f)
				{
					idleLArmTimeout = uiIdleArmDelay.val;
					tempFloat2 = lElbowHoldTarget;
					lElbowHoldTarget = Random.Range(0.0f, 1.0f);
					if (lElbowHoldTarget <= 0.25f)
					{
						lElbowHoldTarget = Random.Range(0.8f, 1.0f);
					}
					if (lElbowHoldTarget >= 0.85f)
					{
						lElbowHoldTarget = Random.Range(0.3f, 0.7f);
					}
					if (Mathf.Abs(lElbowHoldTarget - tempFloat2) < 0.15f && Random.Range(0.0f,100.0f) > 50.0f)
					{
						lElbowHoldTarget = Random.Range(0.0f, 1.0f);
					}
					if (Mathf.Abs(lElbowActual - lElbowTarget) > 5.0f)
					{
						lElbowHoldTarget = Mathf.Max(lElbowHoldTarget, 0.75f);
					}
					//SuperController.LogError("lElbow Hold " + lElbowHoldActual + "/" + lElbowHoldTarget);
				}
				
				//SuperController.LogError("rElbow Hold");
				if (Mathf.Abs(rElbowHoldActual - rElbowHoldTarget) < 0.01f && Random.Range(0.0f,100.0f) < uiIdleArmChance.val / 50.0f && idleRArmTimeout <= 0.0f)
				{
					idleRArmTimeout = uiIdleArmDelay.val;
					tempFloat2 = rElbowHoldTarget;
					rElbowHoldTarget = Random.Range(0.0f, 1.0f);
					if (rElbowHoldTarget <= 0.25f)
					{
						rElbowHoldTarget = Random.Range(0.8f, 1.0f);
					}
					if (rElbowHoldTarget >= 0.85f)
					{
						rElbowHoldTarget = Random.Range(0.3f, 0.7f);
					}
					if (Mathf.Abs(rElbowHoldTarget - tempFloat2) < 0.15f && Random.Range(0.0f,100.0f) > 50.0f)
					{
						rElbowHoldTarget = Random.Range(0.0f, 1.0f);
					}
					if (Mathf.Abs(rElbowActual - rElbowTarget) > 5.0f)
					{
						rElbowHoldTarget = Mathf.Max(rElbowHoldTarget, 0.75f);
					}
					//SuperController.LogError("rElbow Hold " + rElbowHoldActual + "/" + rElbowHoldTarget);
				}

				//SuperController.LogError("lThigh Hold");
				if (Mathf.Abs(lThighHoldActual - lThighHoldTarget) < 0.01f && Random.Range(0.0f,100.0f) < tempFloat && idleBodyTimeout <= 0.0f)
				{
					idleBodyTimeout = uiIdleBodyDelay.val;
					tempFloat2 = lThighHoldTarget;
					if (lThighHoldTarget <= 0.25f)
					{
						lThighHoldTarget = Random.Range(0.5f, 1.0f);
					}
					if (lThighHoldTarget >= 0.75f)
					{
						lThighHoldTarget = Random.Range(0.0f, 0.5f);
					}
					if (Mathf.Abs(lThighHoldTarget - tempFloat2) < 0.15f && Random.Range(0.0f,100.0f) > 50.0f)
					{
						lThighHoldTarget = Random.Range(0.0f, 1.0f);
					}
					if (Mathf.Abs(lThighActual - lThighTarget) < 5.0f)
					{
						lThighHoldTarget = Mathf.Max(lThighHoldTarget, 0.5f);
					}
				}
				
				//SuperController.LogError("rThigh Hold");
				if (Mathf.Abs(rThighHoldActual - rThighHoldTarget) < 0.01f && Random.Range(0.0f,100.0f) < tempFloat && idleBodyTimeout <= 0.0f)
				{
					idleBodyTimeout = uiIdleBodyDelay.val;
					tempFloat2 = rThighHoldTarget;
					if (rThighHoldTarget <= 0.25f)
					{
						rThighHoldTarget = Random.Range(0.5f, 1.0f);
					}
					if (rThighHoldTarget >= 0.75f)
					{
						rThighHoldTarget = Random.Range(0.0f, 0.5f);
					}
					if (Mathf.Abs(rThighHoldTarget - tempFloat2) < 0.15f && Random.Range(0.0f,100.0f) > 50.0f)
					{
						rThighHoldTarget = Random.Range(0.0f, 1.0f);
					}
					if (Mathf.Abs(rThighActual - rThighTarget) < 5.0f)
					{
						rThighHoldTarget = Mathf.Max(rThighHoldTarget, 0.5f);
					}
				}

				//SuperController.LogError("lKnee Hold");
				if (Mathf.Abs(lKneeHoldActual - lKneeHoldTarget) < 0.01f && Random.Range(0.0f,100.0f) < tempFloat && idleLLegTimeout <= 0.0f)
				{
					idleLLegTimeout = uiIdleLegDelay.val;
					tempFloat2 = lKneeHoldTarget;
					if (lKneeHoldTarget <= 0.25f)
					{
						lKneeHoldTarget = Random.Range(0.5f, 1.0f);
					}
					if (lKneeHoldTarget >= 0.75f)
					{
						lKneeHoldTarget = Random.Range(0.2f, 0.5f);
					}
					if (Mathf.Abs(lKneeHoldTarget - tempFloat2) < 0.15f && Random.Range(0.0f,100.0f) > 50.0f)
					{
						lKneeHoldTarget = Random.Range(0.2f, 1.0f);
					}
					if (Mathf.Abs(lKneeActual - lKneeTarget) < 5.0f)
					{
						lKneeHoldTarget = Mathf.Max(lKneeHoldTarget, 0.5f);
					}
				}
				
				//SuperController.LogError("rKnee Hold");
				if (Mathf.Abs(rKneeHoldActual - rKneeHoldTarget) < 0.01f && Random.Range(0.0f,100.0f) < tempFloat && idleRLegTimeout <= 0.0f)
				{
					idleRLegTimeout = uiIdleLegDelay.val;
					tempFloat2 = rKneeHoldTarget;
					if (rKneeHoldTarget <= 0.25f)
					{
						rKneeHoldTarget = Random.Range(0.5f, 1.0f);
					}
					if (rKneeHoldTarget >= 0.75f)
					{
						rKneeHoldTarget = Random.Range(0.2f, 0.5f);
					}
					if (Mathf.Abs(rKneeHoldTarget - tempFloat2) < 0.15f && Random.Range(0.0f,100.0f) > 50.0f)
					{
						rKneeHoldTarget = Random.Range(0.2f, 1.0f);
					}
					if (Mathf.Abs(lKneeActual - lKneeTarget) < 5.0f)
					{
						rKneeHoldTarget = Mathf.Max(rKneeHoldTarget, 0.5f);
					}
				}
				
				//SuperController.LogError("Idle Move look overrides");
				headLastUpDown = headUpDown;
				headLastLeftRight = headLeftRight;
				
			if (currentLook == "BlowJob" || currentLook == "Kissing")
				{
					twistTarget = 0.0f;
					lElbowTarget = 0.0f;
					rElbowTarget = 0.0f;
				}
			if (currentLook == "Intense" && playerHeadToHead < personalSpaceDistance/2.0f && idleLArmTimeout <= 0.0f && idleRArmTimeout <= 0.0f)
				{
					lElbowTarget = Random.Range(10.0f,-20.0f);
					rElbowTarget = Random.Range(-10.0f,20.0f);
				}

			if (amGlancing == false && interestClock <= 0.0f)
            {

				//eyesNonDirectClock = 0.0f;
				idleFlip = -idleFlip;
				//SuperController.LogError("Interest Reset");
				if (interestInterrupt == false && Random.Range(0.0f, 100.0f) > 0.0f)
				{
					chosenRandom = "";
				}

				interestInterrupt = false;

				
				float rand = Random.Range(0.0f, 100.0f);
				if (rand > 33.0f)
                {
					if (rand < 50.0f)
					{
						mLHandFistTarget = Random.Range(0.0f, 1.0f);
					}
					else
					{
						mLHandStraightenTarget = Random.Range(0.0f, Mathf.Lerp(0.5f, 1.0f, mLHandFistTarget));
					}
                }
                if (rand < 66.0f)
                {
					if (rand > 50.0f)
					{
						mRHandFistTarget = Random.Range(0.0f, 1.0f);
					}
					else
					{
						mRHandStraightenTarget = Random.Range(0.0f, Mathf.Lerp(0.5f, 1.0f, mRHandFistTarget));
					}
                }

				//SuperController.LogError("Idle Movement Done");
				
				
				lBunnyHands = false;
				rBunnyHands = false;
				lBellyHands = false;
				rBellyHands = false;
				lBreastHands = false;
				rBreastHands = false;
				lFancyHands = false;
				rFancyHands = false;
				lHipHands = false;
				rHipHands = false;
				lBackArchHands = false;
				rBackArchHands = false;
				lSideHands = false;
				rSideHands = false;

				/*if (interestArousal < 4.0f && interestValence < 4.0f || playerHeadToHead > personalSpaceDistance*2.0f)
				{
					lHipHands = true;
					rHipHands = true;
				}
				else
				{
					if (Random.Range(0.0f, 100.0f) > 15.0f)
					{
						lSideHands = true;
						rSideHands = true;
					}
					else
					{
						if (Random.Range(0.0f, 100.0f) > 15.0f)
						{
							lFancyHands = true;
							rFancyHands = true;
						}
						else
						{
							if (Random.Range(0.0f, 100.0f) > 15.0f)
							{
								if (Random.Range(0.0f, 100.0f) > 30.0f)
								{
									lHipHands = true;
								}
								if (Random.Range(0.0f, 100.0f) > 30.0f)
								{
									rHipHands = true;
								}
							}
							else
							{
								if (Random.Range(0.0f, 100.0f) > 15.0f)
								{
									if (Random.Range(0.0f, 100.0f) > 30.0f && (lBellyHands || lSideHands || lFancyHands || lHipHands))
									{
										lBreastHands = true;
									}
									if (Random.Range(0.0f, 100.0f) > 30.0f && (rBellyHands || rSideHands || rFancyHands || rHipHands))
									{
										rBreastHands = true;
									}
								}
								else
								{
									if (Random.Range(0.0f, 100.0f) > 15.0f)
									{
										lBackArchHands = true;
										rBackArchHands = true;
									}
								}
							}
						}
					}
				}*/
				



                float interestUpdate = currentInterestLevel;
                if (currentInterest == "Pelvis")
                {
                    interestUpdate = interestPelvis;
                    //interestPelvis = interestPelvis - (25.0f * (mainClock / 5.0f));
                }
                if (currentInterest == "Tip")
                {
                    interestUpdate = interestTip;
                    //interestTip = interestTip - (25.0f * (mainClock / 5.0f));
                }
                if (currentInterest == "RHand")
                {
                    interestUpdate = interestRHand;
                    //interestRHand = interestRHand - (25.0f * (mainClock / 5.0f));
                }
                if (currentInterest == "LHand")
                {
                    interestUpdate = interestLHand;
                    //interestLHand = interestLHand - (25.0f * (mainClock / 5.0f));
                }
                if (currentInterest == "Face" && interestKissing == false)
                {
                    interestUpdate = interestFace;
                    //interestFace = interestFace - (10.0f * (mainClock / 5.0f));
                }
                if (currentInterest == "Target")
                {
                    interestUpdate = interestEMTarget;
                    //interestEMTarget = interestEMTarget - (10.0f * (mainClock / 5.0f));
                }
				//SuperController.LogError("Interest Update Set");

				interestRepeating = false;
				if (Random.Range(0.0f,100.0f) > 85.0f + interestRepeat)
				{
					//interestRepeating = true;
					interestRepeat += 1;
				}
				else
				{
					//interestRepeat = 0.0f;
				}
				//SuperController.LogError("Main Interest Start");
				string oldInterest = currentInterest;
				if (interestRepeating == false && Random.Range(0.0f,100.0f) < uiExpressionChance.val)
				{
					mainValue = 0.0f;
					secondValue = 0.0f;
					tempFloat = 0.0f;
					tempFloat2 = Vector3.Dot(headController.followWhenOff.position - playerHeadTransform.position, headController.followWhenOff.forward);
						//SuperController.singleton.ClearMessages();
						//SuperController.LogMessage("Arousal < 1 " + Round(Mathf.Abs(interestArousal - decisionArousal)) + "|Valence < 0.5 " + Round(Mathf.Abs(interestValence - decisionValence)), false);
					
					if (Mathf.Abs(interestArousal - decisionArousal) < 1.0f * uiArousalSpeed.val * uiMoodSpeed.val && Mathf.Abs(interestValence - decisionValence) < 0.5f * uiValenceSpeed.val * uiMoodSpeed.val && lookAction == false)
					{
						decisionRepeat = decisionRepeat + 1.0f;
						if (lastLookState == lBored)
						{
						  lookSM.Switch(lBored);
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(20.0f, 0.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lDayDream);
						  }
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 100.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lCasual);
						  }
						}
						if (lastLookState == lCasual)
						{
						  lookSM.Switch(lCasual);
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(50.0f, 0.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lBored);
						  }
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, Mathf.Lerp(50.0f, 20.0f, interestValence/10.0f), interestArousal/10.0f))
						  {
							lookSM.Switch(lInquisitive);
						  }
						}
						if (lastLookState == lInquisitive)
						{
						  lookSM.Switch(lInquisitive);
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(0.0f, 80.0f, interestArousal/10.0f), 80.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lPlayful);
						  }
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(30.0f, 10.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lCasual);
						  }
						}
						if (lastLookState == lPlayful)
						{
						  lookSM.Switch(lPlayful);
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(60.0f, 10.0f, interestArousal/10.0f), 10.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lInquisitive);
						  }
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(30.0f, 0.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lCasual);
						  }
						}
						if (lastLookState == lFeel)
						{
						  lookSM.Switch(lFeel);
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(30.0f, 0.0f, interestArousal/10.0f), 0.0f, interestValence/10.0f) && interestArousal < 8.0f)
						  {
							lookSM.Switch(lPlayful);
						  }
						}
						if (lastLookState == lDayDream)
						{
						  lookSM.Switch(lDayDream);
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lCasual);
						  }
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(40.0f, 0.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lBored);
						  }
						}
						if (lastLookState == lIntense)
						{
						  lookSM.Switch(lIntense);
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(80.0f, 5.0f, interestArousal/10.0f), 10.0f, interestValence/10.0f))
						  {
							lookSM.Switch(lPlayful);
						  }
						  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 60.0f, interestArousal/10.0f))
						  {
							lookSM.Switch(lFeel);
						  }
						}
						
					}
					else
					{
						decisionRepeat = 0.0f;
					}
					//testString = Mathf.Abs(Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward)).ToString();
					if (Mathf.Abs(Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookNoAwarenessAngle)
					{
						if (interestFace + headActivityBoost > secondValue && playerHeadToHead < backgroundDistance)// || playerHeadToHead < closeFaceDistance * 1.75f)
						{
							if (interestFace + headActivityBoost > mainValue)// || playerHeadToHead < closeFaceDistance * 1.75f)
							{
								//if ((mainInterest != "RandomR" && mainInterest != "RandomL" && mainInterest != "RandomF") || interestValence > 5.0f)
								//{
								if ((Random.Range(0.0f, 100.0f) <= 100.0f - interestRepeat && oldInterest == "Face") || oldInterest != "Face")
								{
									if (mainInterest != "Face")
									{
										mainOld = mainInterest;
										mainSwitch = true;
										interestRepeat = 0.0f;
										//eyesSM.Switch(eFocus);
										//mouthSM.Switch(mBiteLip);
										if ((currentLook == "Casual" || currentLook == "Bored" || currentLook == "Daydream") && morphMouthAction == false && smiledlast == false && interestValence > 4.0f)
										{
											if (interestValence < 6.0f)
											{
												mouthSM.Switch(mSmile);
											}
											else
											{
												mouthSM.Switch(mBigSmile);
											}
										}
									}
									mainInterest = "Face";
									mainValue = interestFace + headActivityBoost;
									//SuperController.LogError("Face is most interesting");
									//SuperController.LogError("|| Set Main Interest to " + mainInterest);

									if (playerHeadToHead < closeFaceDistance * 1.75f)
									{
										//mainValue = interestFace + headActivityBoost + 30.0f;
									}
								}
								//}
							}
							else
							{
								if (headToFaceRot < lookPeripheralAngle)
								{
									if (secondInterest != "Face" && secondOld != "Face")
									{
										secondOld = secondInterest;
										secondSwitch = true;
										//mouthSM.Switch(mSmile);
									}
									secondInterest = "Face";
									secondValue = interestFace + headActivityBoost;
									//SuperController.LogError("Face is second most interesting");
								}
							}
						}
					}
					if (Mathf.Abs(Vector3.Angle(playerLHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle || interestLHand + lHandActivityBoost > interestFace + headActivityBoost)
					{
						if (interestLHand + lHandActivityBoost > secondValue && interestLHand + lHandActivityBoost > minInterest && (playerHandsUsable || (person2Usable && usePerson2)))
						{
							if (interestLHand + lHandActivityBoost > mainValue)
							{
								if (interestLHand + lHandActivityBoost > 27.0f && interestLHand + lHandActivityBoost > interestFace - 10.0f)
								{
									if ((Random.Range(0.0f, 100.0f) <= 100.0f - interestRepeat && oldInterest == "LHand") || oldInterest != "LHand")
									{
										if (mainInterest != "LHand")
										{
											mainOld = mainInterest;
											mainSwitch = true;
											interestRepeat = 0.0f;
											if (morphBrowAction == false && testRun == false)
											{
												browSM.Switch(bRaised);
											}
											if (Vector3.Angle(playerLHand - headController.followWhenOff.position, headController.followWhenOff.forward) > lookNoAwarenessAngle && playerLHandToHead > interactionDistance && currentEye != "Closed")
											{
												//eyesSM.Switch(eClosed);
											}
										}
										mainInterest = "LHand";
										//SuperController.LogError("|| Set Main Interest to " + mainInterest);
										mainValue = interestLHand + lHandActivityBoost;
										//SuperController.LogError("Left Hand is most interesting");
									}
								}
							}
							else
							{
								if (secondInterest != "LHand" && secondOld != "LHand")
								{
									secondOld = secondInterest;
									secondSwitch = true;
									if (interestArousal < 5.0f && morphBrowAction == false && Random.Range(0.0f,100.0f) > 97.0f && testRun == false)
									{
										browSM.Switch(bOneRaise);
									}
								}
								secondInterest = "LHand";
								secondValue = interestLHand + lHandActivityBoost;
								//SuperController.LogError("Left Hand is second most interesting");
							}
						}
					}
					if (Mathf.Abs(Vector3.Angle(playerRHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle || interestRHand + rHandActivityBoost > interestFace + headActivityBoost)
					{
						if (interestRHand + rHandActivityBoost > secondValue && interestRHand + rHandActivityBoost > minInterest && (playerHandsUsable || (person2Usable && usePerson2)))
						{
							if (interestRHand + rHandActivityBoost > mainValue)
							{
								if (interestRHand + rHandActivityBoost > 27.0f && interestRHand + rHandActivityBoost > interestFace - 10.0f)
								{
									if ((Random.Range(0.0f, 100.0f) <= 100.0f - interestRepeat && oldInterest == "RHand") || oldInterest != "RHand")
									{
										if (mainInterest != "RHand")
										{
											mainOld = mainInterest;
											mainSwitch = true;
											interestRepeat = 0.0f;
											if (morphBrowAction == false && testRun == false)
											{
												browSM.Switch(bRaised);
											}
											if (Vector3.Angle(playerRHand - headController.followWhenOff.position, headController.followWhenOff.forward) > lookNoAwarenessAngle && playerRHandToHead > interactionDistance && currentEye != "Closed")
											{
												//eyesSM.Switch(eClosed);
											}
										}
										mainInterest = "RHand";
										//SuperController.LogError("|| Set Main Interest to " + mainInterest);
										mainValue = interestRHand + rHandActivityBoost;
										//SuperController.LogError("Right Hand is most interesting");
									}
								}
							}
							else
							{
								if (secondInterest != "RHand" && secondOld != "RHand")
								{
									secondOld = secondInterest;
									secondSwitch = true;
									if (interestArousal < 5.0f && morphBrowAction == false && Random.Range(0.0f,100.0f) > 97.0f && testRun == false)
									{
										browSM.Switch(bOneRaise);
									}
								}
								secondInterest = "RHand";
								secondValue = interestRHand + rHandActivityBoost;
								//SuperController.LogError("Right Hand is second most interesting");
							}
						}
					}
					if (interestPelvis > secondValue && person2Usable && usePerson2 && interestPelvis > uiMinInterest.val)
					{
						if (interestPelvis > mainValue && interestPelvis > minInterest && playerHeadToHead > uiCloseToFaceDist.val * 2.0f)
						{
							if (mainInterest != "Pelvis")
							{
								mainOld = mainInterest;
								mainSwitch = true;
								interestRepeat = 0.0f;
								if ((interestArousal > 5.0f && lookAction == false && testRun == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < 20.0f)))
								{
									decisionArousal = interestArousal;
									decisionValence = interestValence;
									decisionRepeat = 0.0f;
									lookSM.Switch(lPlayful);
								}
							}
							mainInterest = "Pelvis";
							//SuperController.LogError("|| Set Main Interest to " + mainInterest);
							mainValue = interestPelvis;
							//SuperController.LogError("Pelvis is most interesting");
						}
						else
						{
							if (secondInterest != "Pelvis" && secondOld != "Pelvis")
							{
								secondOld = secondInterest;
								secondSwitch = true;
								if ((lookAction == false && testRun == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < 20.0f)))
								{
									decisionArousal = interestArousal;
									decisionValence = interestValence;
									decisionRepeat = 0.0f;
									lookSM.Switch(lInquisitive);
								}
							}
							secondInterest = "Pelvis";
							secondValue = interestPelvis;
							//SuperController.LogError("Pelvis is second most interesting");
						}
					}
					if ((interestTip > secondValue) && interestTip > minInterest)// || ((playerTipToHead < closeFaceDistance || playerTipToPelvis < closeFaceDistance) && playerHeadToHead > closeFaceDistance * 2.0f)) && usePerson2)
					{
						if (interestTip > mainValue && playerHeadToHead > uiCloseToFaceDist.val * 2.0f)// || (playerTipToHead < closeFaceDistance || playerTipToPelvis < closeFaceDistance && playerHeadToHead > closeFaceDistance))
						{
							if (mainInterest != "Tip")
							{
								mainOld = mainInterest;
								mainSwitch = true;
								interestRepeat = 0.0f;
								//lookSM.Switch(lIntense);
							}
							mainInterest = "Tip";
							//SuperController.LogError("|| Set Main Interest to " + mainInterest);
							mainValue = interestTip;
							//SuperController.LogError("Tip is most interesting");
						}
						else
						{
							if (secondInterest != "Tip")
							{
								secondOld = secondInterest;
								secondSwitch = true;
								//lookSM.Switch(lPlayful);
							}
							secondInterest = "Tip";
							secondValue = interestTip;
							//SuperController.LogError("Tip is second most interesting");
						}
					}
					
					if (interestEMTarget > secondValue && uiObjectTarget.val != "None")
					{
						if (interestEMTarget > mainValue)
						{
							if (mainInterest != "Target")
							{
								mainOld = mainInterest;
								mainSwitch = true;
								interestRepeat = 0.0f;
								if ((interestArousal > 5.0f && lookAction == false && testRun == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < 20.0f)))
								{
									decisionArousal = interestArousal;
									decisionValence = interestValence;
									decisionRepeat = 0.0f;
									lookSM.Switch(lPlayful);
								}
							}
							mainInterest = "Target";
							//SuperController.LogError("|| Set Main Interest to " + mainInterest);
							mainValue = interestEMTarget;
							//SuperController.LogError("Target is most interesting");
						}
						else
						{
							if (secondInterest != "Target" && secondOld != "Target")
							{
								secondOld = secondInterest;
								secondSwitch = true;
								if ((lookAction == false && testRun == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < 20.0f)))
								{
									decisionArousal = interestArousal;
									decisionValence = interestValence;
									decisionRepeat = 0.0f;
									lookSM.Switch(lInquisitive);
								}
							}
							secondInterest = "Target";
							secondValue = interestEMTarget;
							//SuperController.LogError("Target is second most interesting");
						}
					}
					
					if (interestPLHand > secondValue && interestPLHand > minInterest && Vector3.Distance(lHandController.followWhenOff.position, headController.followWhenOff.position) > closeFaceDistance)
					{
						if (Mathf.Abs(Vector3.Angle(lHandController.followWhenOff.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle) 
						{
							if (interestPLHand > mainValue)// && (headToFaceRot > lookNoAwarenessAngle || interestFace < 50.0f))
							{
								if (mainInterest != "PLHand")
								{
									mainOld = mainInterest;
									mainSwitch = true;
									interestRepeat = 0.0f;
								}
								mainInterest = "PLHand";
								//SuperController.LogError("|| Set Main Interest to " + mainInterest);
								mainValue = interestPLHand;
								//SuperController.LogError("Own Left Hand is most interesting");
							}
							else
							{
								if (secondInterest != "PLHand" && secondOld != "PLHand")
								{
									secondOld = secondInterest;
									secondSwitch = true;
								}
								secondInterest = "PLHand";
								secondValue = interestPLHand;
								//SuperController.LogError("Own Left Hand is second most interesting");
							}
						}
					}
					
					if (interestPRHand > secondValue && interestPRHand > minInterest && Vector3.Distance(rHandController.followWhenOff.position, headController.followWhenOff.position) > closeFaceDistance)
					{
						if (Mathf.Abs(Vector3.Angle(rHandController.followWhenOff.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle) 
						{
							if (interestPRHand > mainValue)// && (headToFaceRot > lookNoAwarenessAngle || interestFace < 50.0f))
							{
								if (mainInterest != "PRHand")
								{
									mainOld = mainInterest;
									mainSwitch = true;
									interestRepeat = 0.0f;
								}
								mainInterest = "PRHand";
								//SuperController.LogError("|| Set Main Interest to " + mainInterest);
								mainValue = interestPRHand;
								//SuperController.LogError("Own Right Hand is most interesting");
							}
							else
							{
								if (secondInterest != "PRHand" && secondOld != "PRHand")
								{
									secondOld = secondInterest;
									secondSwitch = true;
								}
								secondInterest = "PRHand";
								secondValue = interestPRHand;
								//SuperController.LogError("Own Right Hand is second most interesting");
							}
						}
					}

					if (mainSwitch)
					{
						mainClock = 0.0f;
						//saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
						tempFloat = Random.Range(0.0f,100.0f);
						if (mainInterest == "Face" && mainOld != "Face" && tempFloat > 75.0f)
						{
							gazeAdjust = 3.0f;
						}
						if ((mainInterest == "RHand" && mainInterest == "LHand") && mainOld == "Face" && tempFloat > 75.0f)
						{
							gazeAdjust = 0.5f;
						}
						if ((mainInterest == "RandomF" || mainInterest == "RandomL" || mainInterest == "RandomR" || mainInterest == "RandomU") && (mainOld == "RandomF" || mainOld == "RandomL" || mainOld == "RandomR" || mainOld == "RandomU") && tempFloat > 75.0f)
						{
							gazeAdjust = 2.0f;
						}
					}
					else
					{
						gazeAdjust = 1.0f;
					}
					
					if (playerLHandToHead < closeFaceDistance)
						{
							if (mainInterest != "LHand")
							{
								mainOld = mainInterest;
							}
							mainInterest = "LHand";
							//SuperController.LogError("|| closeface Set Main Interest to " + mainInterest);
							mainValue = 100;
							secondOld = secondInterest;
							secondInterest = "Face";
							secondValue = 60;
							//SuperController.LogError("Left Hand near face, switching to main");
						}
					if (playerRHandToHead < closeFaceDistance)
						{
							if (mainInterest != "RHand")
							{
								mainOld = mainInterest;
							}
							mainInterest = "RHand";
							//SuperController.LogError("|| closeface Set Main Interest to " + mainInterest);
							mainValue = 100;
							secondOld = secondInterest;
							secondInterest = "Face";
							secondValue = 60;
							//SuperController.LogError("Right Hand near face, switching to main");
						}
					
					if (chosenRandom == "")
					{
						if (secondInterest == "RandomR" || secondInterest == "RandomL" || secondInterest == "RandomF" || secondInterest == "RandomU")
						{
							if (Random.Range(0.0f,100.0f) > 85.0f)
							{
								if (Random.Range(0.0f,100.0f) > 90.0f)
								{
									if (Vector3.Angle(randomPointForward - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
									{
										secondInterest = "RandomF";
										chosenRandom = secondInterest;
									}
								}
								else
								{
									if (Random.Range(0.0f,100.0f) > 50.0f)
									{
										if (Vector3.Angle(randomPointLeft - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
										{
											secondInterest = "RandomL";
											chosenRandom = secondInterest;
											//SuperController.LogError("1");
										}
									}
									else
									{
										if (Vector3.Angle(randomPointRight - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
										{
											secondInterest = "RandomR";
											chosenRandom = secondInterest;
										}
									}
								}
							}
						}
					}
						
					if (secondInterest == "RHand" && interestRHand + rHandActivityBoost < 10.0f && chosenRandom == "")
					{
						if (Random.Range(0.0f,100.0f) > 5.0f)
						{
							if (Vector3.Angle(randomPointRight - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
							{
								secondInterest = "RandomR";
								chosenRandom = secondInterest;
							}
						}
						else
						{
							if (Vector3.Angle(randomPointForward - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
							{
								secondInterest = "RandomF";
								chosenRandom = secondInterest;
							}
						}
					}
					
					if (secondInterest == "LHand" && interestLHand + lHandActivityBoost < 10.0f && chosenRandom == "")
					{
						if (Random.Range(0.0f,100.0f) > 5.0f)
						{
							if (Vector3.Angle(randomPointLeft - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
							{
								secondInterest = "RandomL";
								chosenRandom = secondInterest;
								//SuperController.LogError("2");
							}
						}
						else
						{
							if (Vector3.Angle(randomPointForward - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
							{
								secondInterest = "RandomF";
								chosenRandom = secondInterest;
							}
						}
					}
					
					if ((mainOld == "RandomR" || mainOld == "RandomL" || mainOld == "RandomF") && mainInterest == "Face" && chosenRandom == "")
					{
						if (playerHeadToHead > backgroundDistance && interestValence < 5.0f && Random.Range(0.0f,100.0f) > 95.0f)
						{
							mainInterest = mainOld;
							chosenRandom = mainInterest;
							//mainOld = "Face";
							//SuperController.LogError("Looking at face and bored, switching back to random");
						}
					}
					

					if (mainInterest != "Tip" && playerTipToHead < interactionDistance * 3.0f && interestArousal > 5.0f)
					{
						mainInterest = "Tip";
						interestClock = 0.0f;
						//SuperController.LogError("Tip close to face, switching");
					}
					
					if (vagTouchCount > 0.0f && currentLook == "Sex" && currentEye != "Closed")
					{
						if ((mainOld == "Face" || mainOld == "Pelvis") && Random.Range(0.0f,100.0f) > 80.0f)
						{
							mainOld = mainInterest;
							mainInterest = "RandomU";
							//SuperController.LogError("|| sex Set Main Interest to " + mainInterest);
							//SuperController.LogError("Sex action, looking at random up");
						}
						else
						{
							if (interestArousal > 9.5f && interestValence > 7.5f && mainInterest != "Pelvis"  && Random.Range(0.0f,100.0f) > 80.0f)
							{
								if (Mathf.Abs(Vector3.Angle(pelvisController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
								{
									mainOld = mainInterest;
									mainInterest = "Pelvis";
									//SuperController.LogError("|| sex Set Main Interest to " + mainInterest);
									//SuperController.LogError("Sex Action very aroused switch to Pelvis");
								}
							}
							if (interestArousal < 9.5f && interestValence < 9.5f && mainInterest != "Face" )
							{
								mainOld = mainInterest;
								mainInterest = "Face";
								//SuperController.LogError("|| sex Set Main Interest to " + mainInterest);
								//SuperController.LogError("Sex Action switch to Face");
							}
						}
						if (mainInterest == "Pelvis" && mainOld == "Face")
						{
								mainOld = mainInterest;
								mainInterest = "Face";
						}
						currentInterest = mainInterest;
						//SuperController.LogError("Set Current to Main");						
					}
					
					//currentInterestLevel = 0.0f;
					//SuperController.LogError("Main interest Pelvis");
					if (mainInterest == "Pelvis")
					{
						interestArousal += (0.05f * (0.1f + (pExtraversion / 100.0f))) * uiArousalSpeed.val;
						interestValence += (0.012f * (0.1f + (pAgreeableness / 100.0f))) * uiValenceSpeed.val;
						currentInterest = "Pelvis";
						currentInterestLevel = interestPelvis;
						playerInterest += 2.0f;
						if ((lookAction == false && (Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < uiExpressionChance.val / 2.0f)))
						{
							decisionArousal = interestArousal;
							decisionValence = interestValence;
							decisionRepeat = 0.0f;
							if (interestArousal + interestValence < 5)
							{
								lookSM.Switch(lCasual);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 10.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestValence/10.0f))
								{
								  lookSM.Switch(lInquisitive);
								}
							}
							else
							{
								if (interestArousal + interestValence < 15)
								{
								  lookSM.Switch(lCasual);
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestArousal/10.0f))
								  {
									lookSM.Switch(lPlayful);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, interestValence/10.0f))
								  {
									lookSM.Switch(lInquisitive);
								  }
								}
								else
								{
								  lookSM.Switch(lIntense);
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 60.0f, interestValence/10.0f))
								  {
									lookSM.Switch(lInquisitive);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(60.0f, 0.0f, interestArousal/10.0f))
								  {
									lookSM.Switch(lPlayful);
								  }
								}
							}
						}
						//SuperController.LogError("Main is Pelvis set to current");
					}
					//SuperController.LogError("Main interest Tip");
					if (mainInterest == "Tip")
					{
						interestArousal += (0.2f * (0.1f + (pExtraversion / 100.0f))) * uiArousalSpeed.val;
						interestValence += (0.035f * (0.1f + (pAgreeableness / 100.0f))) * uiValenceSpeed.val;
						currentInterest = "Tip";
						currentInterestLevel = interestTip;
						playerInterest += 5.0f;
						if (lookAction == false && Random.Range(0.0f,100.0f) < uiExpressionChance.val && testRun == false)
						{
							if (playerTipToPelvis < interactionDistance * 1.5f && playerHeadToHead > uiCloseToFaceDist.val*2.0f && vagTouchCount > 0.0f && uiDoSex.val && testRun == false)
							{
								lookSM.Switch(lSex);
							}
							else
							{
								if (playerTipToHead < interactionDistance && uiDoBlowjob.val && lipsTouchCount > 0.0f && testRun == false)
								{
									lookSM.Switch(lSucking);
								}
								else
								{
									if ((lookAction == false && testRun == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < uiExpressionChance.val * 2.0f)))
									{
										decisionArousal = interestArousal;
										decisionValence = interestValence;
										decisionRepeat = 0.0f;
										if (interestArousal + interestValence < 5)
										{
										  lookSM.Switch(lCasual);
										  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 10.0f, interestArousal/10.0f))
										  {
											lookSM.Switch(lPlayful);
										  }
										  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestValence/10.0f))
										  {
											lookSM.Switch(lInquisitive);
										  }
										}
										else
										{
										  if (interestArousal + interestValence < 15)
										  {
											lookSM.Switch(lCasual);
											if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestArousal/10.0f))
											{
											  lookSM.Switch(lPlayful);
											}
											if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, interestValence/10.0f))
											{
											  lookSM.Switch(lInquisitive);
											}
										  }
										  else
										  {
											lookSM.Switch(lIntense);
											if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 60.0f, interestValence/10.0f))
											{
											  lookSM.Switch(lInquisitive);
											}
											if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(60.0f, 0.0f, interestArousal/10.0f))
											{
											  lookSM.Switch(lPlayful);
											}
										  }
										}
									}
								}
							}
						}
						//SuperController.LogError("Main is Tip set to current");
					}
					//SuperController.LogError("Main interest RHand");
					if (mainInterest == "RHand")
					{
						interestArousal += (0.005f * (0.1f + ((100.0f - pExtraversion) / 100.0f))) * uiArousalSpeed.val;
						interestValence += (0.012f * (0.1f + (pAgreeableness / 100.0f))) * uiValenceSpeed.val;
						currentInterest = "RHand";
						currentInterestLevel = interestRHand;
						playerInterest += 1.0f;
						if ((lookAction == false && (Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false && lipsTouchCount <= 0) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < uiExpressionChance.val / 2.0f)))
						{
							decisionArousal = interestArousal;
							decisionValence = interestValence;
							decisionRepeat = 0.0f;
							if (playerRHandToHead < interactionDistance || playerRHandToLBreast < interactionDistance || playerRHandToRBreast < interactionDistance || playerRHandToPelvis < interactionDistance * 1.3f)
							{
								lookSM.Switch(lInquisitive);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestValence/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lFeel);
								}
							}
							else
							{
								lookSM.Switch(lInquisitive);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, interestValence/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(50.0f, 0.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lCasual);
								}
							}
						}
						//SuperController.LogError("Main is Right Hand set to current");
					}
					//SuperController.LogError("Main interest LHand");
					if (mainInterest == "LHand")
					{
						interestArousal += (0.005f * (0.1f + ((100.0f - pExtraversion) / 100.0f))) * uiArousalSpeed.val;
						interestValence += (0.012f * (0.1f + (pAgreeableness / 100.0f))) * uiValenceSpeed.val;
						currentInterest = "LHand";
						currentInterestLevel = interestLHand;
						playerInterest += 1.0f;
						if ((lookAction == false && (Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false && lipsTouchCount <= 0) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < uiExpressionChance.val / 2.0f)))
						{
							decisionArousal = interestArousal;
							decisionValence = interestValence;
							decisionRepeat = 0.0f;
							if (playerLHandToHead < interactionDistance || playerLHandToLBreast < interactionDistance || playerLHandToRBreast < interactionDistance || playerLHandToPelvis < interactionDistance * 2.0f)
							{
								lookSM.Switch(lInquisitive);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestValence/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lFeel);
								}
							}
							else
							{
								lookSM.Switch(lInquisitive);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, interestValence/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(50.0f, 0.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lCasual);
								}
							}
						}
						//SuperController.LogError("Main is Left Hand set to current");
					}
					tempFloat = 0.0f;
					if ((playerLHandToLBreast < interactionDistance || playerLHandToRBreast < interactionDistance || playerLHandToPelvis < interactionDistance * 1.3f) || (playerRHandToLBreast < interactionDistance || playerRHandToRBreast < interactionDistance || playerRHandToPelvis < interactionDistance * 1.3f))
						{
							tempFloat = 1.0f;
						}
					if (playerLHandToPelvis < interactionDistance * 1.3f || playerRHandToPelvis < interactionDistance * 1.3f && vagTouchCount > 0.0f && uiDoSex.val)
						{
							tempFloat = 2.0f;
						}
					if (interestArousal > 9.5f && interestValence > 9.0f)
					{
						//tempFloat = 1.0f;
					}
					//SuperController.LogError("Main interest Face");
					if (mainInterest == "Face" || mainInterest == "Target")
					{
						if (playerHeadToHead < closeFaceDistance * 2.5f && mainInterest == "Face")
						{
							interestValence += (0.12f * (0.1f + (Mathf.Clamp(pAgreeableness + (interestArousal * 2.0f),0.0f,100.0f) / 100.0f))) * uiValenceSpeed.val;
						}
						else
						{
							interestValence += (0.1f * (0.1f + (Mathf.Clamp(pAgreeableness + (interestArousal * 2.0f),0.0f,100.0f) / 100.0f))) * uiValenceSpeed.val;
						}
						if (mainInterest == "Face")
						{
							interestArousal += (0.007f * (0.1f + ((100.0f - pExtraversion) / 100.0f))) * uiArousalSpeed.val;
							if (currentInterest != "Face")
							{
								currentInterest = "Face";
								//SuperController.LogError("Main is Face set to current");
							}
							else
							{
								currentInterest = "Face";
								if (interestRepeat > 2.0f && interestRepeat < 5.0f)
								{
									if (Random.Range(0.0f, 100.0f) > 90.0f && playerPelvisToHead > personalSpaceDistance && interestArousal > 8.0f)
									{
										currentInterest = "Pelvis";
										//SuperController.LogError("Main is face, set current to pelvis");
									}
								}
								if (interestRepeat > 1.0f && interestRepeat < 3.0f)
								{
									if (Random.Range(0.0f, 100.0f) > 80.0f && playerHeadToHead > uiCloseToFaceDist.val*2.0f && interestValence > 5.0f)
									{
										currentInterest = "Chest";
										//SuperController.LogError("Main is Face, set current to chest");
									}
								}
							}
							currentInterestLevel = interestFace;
						}
						else
						{
							currentInterest = "Target";
							currentInterestLevel = interestEMTarget;
							//SuperController.LogError("Main is Target switch to current");
							
						}
						if ((lookAction == false && (Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < 20.0f)))
						{
							decisionArousal = interestArousal;
							decisionValence = interestValence;
							decisionRepeat = 0.0f;
							if (playerHeadToPelvis < interactionDistance && playerHeadMovement && mainInterest == "Face" && playerHeadToHead > uiCloseToFaceDist.val*2.0f && uiDoSex.val)
							{
								lookSM.Switch(lSex);
							}
							else
							{
								if (interestArousal + interestValence < 7.0f)
								{
									if (playerHeadToFaceRot < playerLookDirectAngle)
									{
										lookSM.Switch(lCasual);
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestValence/10.0f))
										{
										  lookSM.Switch(lInquisitive);
										}
									}
									else
									{
										lookSM.Switch(lCasual);
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, interestValence/10.0f))
										{
										  lookSM.Switch(lInquisitive);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(60.0f, 0.0f, interestValence/10.0f))
										{
										  lookSM.Switch(lBored);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(80.0f, 0.0f, interestValence/10.0f))
										{
										  lookSM.Switch(lDayDream);
										}
									}
								}
								else
								{
									if (interestArousal + interestValence < 15.0f)
									{
										lookSM.Switch(lInquisitive);
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(50.0f, 0.0f, interestValence/10.0f))
										{
										  lookSM.Switch(lCasual);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestArousal/10.0f))
										{
										  lookSM.Switch(lIntense);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestArousal/10.0f))
										{
										  lookSM.Switch(lPlayful);
										}
									}
									else
									{
										if (tempFloat == 2.0f && interestArousal > 8.0f)
										{
											lookSM.Switch(lSex);
										}
										else
										{
											if (tempFloat > 0.0f && interestArousal > 5.0f && currentInterest != "Face")
											{
												lookSM.Switch(lFeel);
											}
											else
											{
												lookSM.Switch(lPlayful);
												if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(50.0f, 0.0f, interestValence/10.0f))
												{
												  lookSM.Switch(lCasual);
												}
												if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestArousal/10.0f))
												{
												  lookSM.Switch(lIntense);
												}
												if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestArousal/10.0f))
												{
												  lookSM.Switch(lFeel);
												}
											}
										}
									}
								}
							}
						}
						playerInterest += 0.5f;
					}

					//SuperController.LogError("Main interest Bored");
					if (((interestFace + headActivityBoost <= uiMinInterest.val || playerHeadToHead > backgroundDistance) && interestLHand + lHandActivityBoost <= uiMinInterest.val*1.0f && interestRHand + rHandActivityBoost <= uiMinInterest.val*1.0f && interestPelvis <= uiMinInterest.val*2.0f && interestTip <= uiMinInterest.val*2.0f && interestEMTarget <= uiMinInterest.val*1.0f && interestPLHand < uiMinInterest.val*2.0f && interestPRHand < uiMinInterest.val*2.0f) || ((currentLook == "Bored" || currentLook == "Daydream") && Random.Range(0.0f,100.0f) > 40.0f && interestArousal < 5.0f))
					{
						interestArousal -= (0.2f * (0.1f + ((100.0f - pStableness) / 100.0f))) / uiArousalSpeed.val;
						interestValence -= (0.065f * (0.1f + ((100.0f - pAgreeableness) / 100.0f))) / uiValenceSpeed.val;
						if (((Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < 20.0f)))
						{
							decisionArousal = interestArousal;
							decisionValence = interestValence;
							decisionRepeat = 0.0f;
							if ((currentLook == "Bored" || currentLook == "Daydream") && interestArousal > 5.5f && testRun == false)
							{
								lookSM.Switch(lCasual);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, interestValence/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lInquisitive);
								}
							  }
							else
							{
								if ((lookAction == false && playerHeadToHead > personalSpaceDistance && interestValence < 5.0f && playerInterest < 50.0f && testRun == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < uiExpressionChance.val / 2.0f)))
								{
								  lookSM.Switch(lBored);
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, interestValence/10.0f))
								  {
									lookSM.Switch(lCasual);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(30.0f, 00.0f, interestArousal/10.0f))
								  {
									lookSM.Switch(lDayDream);
								  }
								}
							}
						}
						if (Random.Range(0.0f, 100.0f) < 1.0f + (interestFace + headActivityBoost / 10.0f) && currentLook != "Sex" && headToFaceRot < lookPeripheralAngle && playerHeadToHead < backgroundDistance)
						{
							currentInterest = "Face";
							mainInterest = currentInterest;
							//SuperController.LogError("|| bored Set Main Interest to " + mainInterest);
							//SuperController.LogError("Bored switch current to Face");
						}
						else
						{
							if (interestClock < 0.1f && chosenRandom == "")
							{
								playerInterest -= 10.5f;
								if (Random.Range(0.0f, 100.0f) < 25.0f && Vector3.Angle(randomPointForward - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
								{
									currentInterest = "RandomF";
									chosenRandom = currentInterest;
									interestClock = Mathf.Clamp(Random.Range(1.0f, 2.0f) * (pStableness / 50.0f), 2.0f, 12.0f) * uiInterestSpeed.val;
									//SuperController.LogError("Bored switch current to Random Front " + interestClock);
								}
								else
								{
									if (Random.Range(0.0f, 100.0f) < 10.0f && Vector3.Angle(randomPointUp - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
									{
										currentInterest = "RandomU";
										chosenRandom = currentInterest;
										interestClock = Mathf.Clamp(Random.Range(1.0f, 2.0f) * (pStableness / 50.0f), 2.0f, 12.0f) * uiInterestSpeed.val;
										//SuperController.LogError("Bored switch current to Random Up " + interestClock);
									}
									else
									{
										if (Random.Range(0.0f, 100.0f) > 50.0f && Vector3.Angle(randomPointRight - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
										{
											currentInterest = "RandomR";
											chosenRandom = currentInterest;
											interestClock = Mathf.Clamp(Random.Range(1.0f, 2.0f) * (pStableness / 50.0f), 2.0f, 12.0f) * uiInterestSpeed.val;
											//SuperController.LogError("Bored switch current to Random Right " + interestClock);
										}
										else
										{
											if (Vector3.Angle(randomPointLeft - headController.followWhenOff.position, headController.followWhenOff.forward) < uiPeripheralGaze.val)
											{
												currentInterest = "RandomL";
												chosenRandom = currentInterest;
												interestClock = Mathf.Clamp(Random.Range(1.0f, 2.0f) * (pStableness / 50.0f), 2.0f, 12.0f) * uiInterestSpeed.val;
												//SuperController.LogError("Bored switch current to Random Left " + interestClock);
											}
										}
									}
								}
								mainInterest = currentInterest;
								//SuperController.LogError("|| bored Set Main Interest to " + mainInterest);
							}
						}
						if (mainInterest != currentInterest)
						{
							mainOld = mainInterest;
						}
						mainInterest = currentInterest;
						//SuperController.LogError("|| bored Set Main Interest to " + mainInterest);
					}
					//SuperController.LogError("Main interest Own LHand");
					if (mainInterest == "PLHand" && interestValence > 5.0f)
					{
						currentInterest = "PLHand";
						currentInterestLevel = interestPLHand;
						playerInterest -= 1.0f;
						if (lookAction == false && (Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false)
						{
							if ((lookAction == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < 20.0f)))
							{
								decisionArousal = interestArousal;
								decisionValence = interestValence;
								decisionRepeat = 0.0f;
								lookSM.Switch(lInquisitive);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(50.0f, 0.0f, interestValence/10.0f))
								{
								  lookSM.Switch(lCasual);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
							}
						}
						//SuperController.LogError("Main is Own Left Hand, Set as Current");
					}
					//SuperController.LogError("Main interest Own Rhand");
					if (mainInterest == "PRHand" && interestValence > 5.0f)
					{
						currentInterest = "PRHand";
						currentInterestLevel = interestPRHand;
						playerInterest -= 1.0f;
						if (lookAction == false && (Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false)
						{
							if ((lookAction == false) && (decisionRepeat == 0.0f || (decisionRepeat > 4.0f && Random.Range(0.0f,100.0f) < 20.0f)))
							{
								decisionArousal = interestArousal;
								decisionValence = interestValence;
								decisionRepeat = 0.0f;
								lookSM.Switch(lInquisitive);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(50.0f, 0.0f, interestValence/10.0f))
								{
								  lookSM.Switch(lCasual);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
							}
						}
						//SuperController.LogError("Main is Own Right Hand, Set as Current");
					}

				}
				if (oldInterest == currentInterest)
				{
					//interestRepeat += 1.0f;
				}
				//SuperController.LogError("Main interest finish");
                if (currentInterest != oldInterest && eyeClock > (1.45f * uiBlinkSpeed.val) * blinkRepeat && amGlancing == false && currentEye != "Closed")
                {
					if (morphEyeAction == false)
					{
					eyesSM.Switch(eBlink);
						eyeClock = 0.0f;
						//SuperController.LogError("Interest change, blink");
					}
                }
				
				if (headToEyeController > lookNoAwarenessAngle && chosenRandom == "")
				{
					//gAvoid = 1.0f;
					mainOld = mainInterest;
					mainInterest = "RandomF";
					currentInterest = "RandomF";
					chosenRandom = currentInterest;
					//SuperController.LogError("|| main out of view Set Main Interest to " + mainInterest);
					//SuperController.LogError("Main out of view, switching to random front");
				}
				//SuperController.LogError("Kissing check");
				
				tempDir = Vector3.Angle(headController.followWhenOff.forward, playerHeadTransform.forward);
				tempFloat = 0.0f;
				if (person2Usable && usePerson2)
				{
					if (playerHeadController.possessed)
					{
						tempFloat = 1.0f;
					}
				}
                if (playerHeadToHead < kissingDistance && interestKissing == false && uiDoKiss.val && (lipsTouchCount > 0.0f || tempFloat == 1.0f) && testRun == false && tempDir > 110.0f && tempDir < 250.0f)
                {
					
					if (tempDir > 90.0f && tempDir < 270.0f)
					{
						lipsOnly = false;
						lookSM.Switch(lKissing);
						//SuperController.LogError("Override Kiss");
						
					}
                }
				//SuperController.LogError("Interest Clock");
                float clockBase = Random.Range(2.0f, 5.0f);
                if (mainSwitch || secondSwitch)
                {
                    clockBase = Random.Range(3.0f, 7.0f);
                }
				if ((mainInterest == "Tip" || mainInterest == "Pelvis") && (playerTipToHead < closeFaceDistance || playerTipToPelvis < interactionDistance))
				{
					clockBase = Random.Range(1.0f, 2.0f);
				}
				if (mainInterest != "Face" && interestFace > uiMinInterest.val && gAvoid == 0.0f && amGlancing == false && headToFaceRot < lookPeripheralAngle && playerHeadToHead < personalSpaceDistance)
				{
					clockBase = Random.Range(0.5f, 1.0f);
				}
                interestClock = Mathf.Clamp(clockBase * (pStableness / 50.0f), 3.0f, 12.0f) * uiInterestSpeed.val;
				//SuperController.LogError("Main Interest Done");
            }
            else
            {
				//SuperController.LogError("Interest Clock Running");
				if (interestClock > -1.0f)
				{
					if (playerHeadToFaceRot < playerLookDirectAngle && mainInterest != "Face" && playerHeadToHead < personalSpaceDistance)
					{
						interestClock -= Time.fixedDeltaTime * 2.0f;
					}
					else
					{
						interestClock -= Time.fixedDeltaTime;
					}
					if ((currentInterest == "Face" && interestFace < minInterest)
						|| (currentInterest == "LHand" && interestLHand < minInterest)
						|| (currentInterest == "RHand" && interestRHand < minInterest)
						|| (currentInterest == "PLHand" && interestPLHand < minInterest)
						|| (currentInterest == "PRHand" && interestPRHand < minInterest)
						|| (currentInterest == "Pelvis" && interestPelvis < minInterest)
						|| (currentInterest == "Tip" && interestTip < minInterest))
					{
						//currentInterest = prevInterest;
						//SuperController.LogError("Current out of view, setting prev");
						interestClock -= Time.fixedDeltaTime * 5.0f;
					}
					if (headToEyeController > lookDirectAngle)
					{
						interestClock -= Time.fixedDeltaTime * 2.0f;
					}
					if (headToEyeController > lookPeripheralAngle)
					{
						interestClock -= Time.fixedDeltaTime * 5.0f;
					}
					if ((currentInterest == "Random" || 
						currentInterest == "RandomL" || 
						currentInterest == "RandomR" || 
						currentInterest == "RandomU" || 
						currentInterest == "RandomF") && interestFace > 80.0f * Mathf.Clamp(uiHeadInterest.val, 0.0f, 1.0f))
					{
						interestClock -= Time.fixedDeltaTime * 2.0f;
					}
					if (((pLHandTouch && mainInterest != "PLHand") || (pRHandTouch && mainInterest != "PRHand")) && playerHeadMovement == false)
					{
						interestClock -= Time.fixedDeltaTime * 1.5f;
					}
				}
				//SuperController.LogError("point A");
				if (amGlancing == false && gAvoid == 0.0f)
				{
					//SuperController.LogError("point B");
					tempFloat = Mathf.Abs(Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					if (mainInterest == "Face" && tempFloat > lookNoAwarenessAngle)
					{
						if (interestClock == -1.0f && interestInterrupt == false)
						{
							mainInterest = "RandomF";
							currentInterest = "RandomF";
							//SuperController.LogError("|| hard angle Set Main Interest to " + mainInterest);
							//SuperController.LogError("Face far away and to the side, switch to random front");
						}
						interestClock = -1.0f;
					}
					//SuperController.LogError("point C");
					tempFloat = Mathf.Abs(Vector3.Angle(playerLHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					if (mainInterest == "LHand" && tempFloat > lookNoAwarenessAngle && interestInterrupt == false)
					{
						if (interestClock == -1.0f)
						{
							mainInterest = "RandomF";
							currentInterest = "RandomF";
							//SuperController.LogError("|| hard angle Set Main Interest to " + mainInterest);
							//SuperController.LogError("Left Hand far away and to the side, switch to random front");
						}
						interestClock = -1.0f;
					}
			//SuperController.LogError("point D");

					tempFloat = Mathf.Abs(Vector3.Angle(playerRHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					if (mainInterest == "RHand" && tempFloat > lookNoAwarenessAngle && interestInterrupt == false)
					{
						if (interestClock == -1.0f)
						{
							mainInterest = "RandomF";
							currentInterest = "RandomF";
							//SuperController.LogError("|| hard angle Set Main Interest to " + mainInterest);
							//SuperController.LogError("Right Hand far away and to the side, switch to random front");
						}
						interestClock = -1.0f;
					}
			//SuperController.LogError("point E");

					if (uiObjectTarget.val != "None" && emTarget != null)
					{
						tempFloat = Mathf.Abs(Vector3.Angle(emTargetTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
						if (mainInterest == "Target" && tempFloat > lookNoAwarenessAngle && interestInterrupt == false)
						{
							if (interestClock == -1.0f)
							{
								mainInterest = "RandomF";
								currentInterest = "RandomF";
								//SuperController.LogError("|| hard angle Set Main Interest to " + mainInterest);
								//SuperController.LogError("Target far away and to the side, switch to random front");
							}
							interestClock = -1.0f;
						}
					}
								//SuperController.LogError("point F");

				}
				//SuperController.LogError("Interest Clock Counting");
            }
			//SuperController.LogError("Interest Clock Complete");
					


			if (uiObjectAsPrimary.val == true && uiObjectTarget.val != "None")
			{
				mainInterest = "Target";
				secondInterest = "Target";
				currentInterestLevel = 100.0f;
				currentInterest = "Target";
				interestFace = 0.0f;
				interestLHand = 0.0f;
				interestRHand = 0.0f;
				interestPelvis = 0.0f;
				//SuperController.LogError("|| object as primary Set Main Interest to " + mainInterest);
				//SuperController.LogError("Object is Primary active, setting Target to main");
			}
			if (playerHeadToFaceRot < playerLookDirectAngle && (playerHeadToHead < personalSpaceDistance || pExtraversion < 35.0f) && uiGazeAvoid.val && interestArousal < 5.0f)
			{
				//SuperController.LogError("Gaze Avoid start");
				if (mainInterest != "Face" && gAvoid == 0.0f && amGlancing == false && playerHeadToHead < personalSpaceDistance / 2.0f && interestClock > 0.0f)
				{
					interestClock -= Time.fixedDeltaTime;
				}
				//SuperController.singleton.ClearMessages();
				//SuperController.LogMessage("Eye Contact (" + (gAvoidance / 4.0) + "/" + ((100.0f-gAvoidance) / 10.0f) + ")" + gAvoidanceClock + "/" + gAvoidingClock + "/" + gAvoid, false);
				if (gAvoidingClock > 0.0f && gAvoid == 1.0f)
				{
					tempFloat = 1.0f;
					if (currentLook == "bored")
					{
						tempFloat = 0.3f;
					}
					if (currentLook == "casual")
					{
						tempFloat = 0.65f;
					}
					if (currentLook == "intense")
					{
						tempFloat = 1.35f;
					}
					if ( mEyesClosedLeftValue > 0.5f)
					{
						tempFloat = 0.1f;
					}
					if ( playerHeadToFaceRot < playerLookDirectAngle)
					{
						tempFloat = Mathf.Max(tempFloat - 0.2f,0.1f);
					}
					if (interestArousal < 4.0f && interestValence < 4.0f && playerHeadToHead >= personalSpaceDistance)
					{
						gAvoidingClock -= (Time.fixedDeltaTime * (tempFloat * Mathf.Lerp(0.3f, 1.3f,interestArousal / 10.0f)) / 5.0f);
						if (Random.Range(0.0f,100.0f) > 99.5f)
						{
							//if (lookAwaySide == "left")
							//{
							//	lookAwaySide = "right";
							//}
							//else
							//{
							//	lookAwaySide = "left";
							//}
							tempFloat = Random.Range(0.0f,100.0f);
							gAvoidHeight = Random.Range(-5.0f,2.0f);
							randomResetDir = true;
							if (tempFloat < 25.0f)
							{
								lookAwaySide = "left";
							}
							else
							{
								if (tempFloat < 50.0f)
								{
									lookAwaySide = "right";
								}
								else
								{
									if (tempFloat < 75.0f)
									{
										lookAwaySide = "up";
									}
									else
									{
										lookAwaySide = "down";
									}
								}
							}
						}
						if (Random.Range(0.0f,100.0f) > 99.8f)
						{
							gAvoidingClock = 0.0f;
							mainInterest = "Pelvis";
							//SuperController.LogError("|| avoiding face Set Main Interest to " + mainInterest);
							//SuperController.LogError("Avoid look down using pelvis");
						}
					}
					else
					{
						gAvoidingClock -= Time.fixedDeltaTime * (tempFloat * Mathf.Lerp(1.7f, 3.3f,interestArousal / 10.0f));
					}
					if ((mainInterest == "Face" && playerHeadMovement) || (mainInterest == "LHand" && playerLHandMovement) || (mainInterest == "RHand" && playerRHandMovement))
					{
						gAvoid = 0.0f;
						gAvoidingClock = 0.0f;
						glanceTimeout = Mathf.Lerp(4.0f,10.0f,pExtraversion/100.0f) * uiGlanceTimeout.val;
					}
				}
				else
				{
					if (gAvoidingClock <= 0.0f && gAvoid == 1.0f)
					{
						gAvoid = 0.0f;
						gAvoidanceClock = 0.0f;
						gAvoidingClock = 0.0f;
						eyeUpdateClock = eyeUpdateTime + 1.0f;
						glanceTimeout = Mathf.Lerp(4.0f,10.0f,pExtraversion/100.0f) * uiGlanceTimeout.val;
						if (eyeClock > 0.7f * uiBlinkSpeed.val * blinkRepeat && currentEye != "Closed")
						{
							eyesSM.Switch(eBlink);
							eyeClock = 0.0f;
							//SuperController.LogError("avoid done blink");
						}
						if (mainInterest != gAvoidInterest)
						{
							//mainOld = mainInterest;
						}
						mainInterest = gAvoidInterest;
						currentInterest = mainInterest;
						//SuperController.LogError("|| gavoid end Set Main Interest to " + mainInterest);
						interestClock = 0.0f;
						//SuperController.LogError("Avoid ended switch main back to " + gAvoidInterest);
					}
				}
				if (gAvoidanceClock < Mathf.Lerp(1.5f * uiGazeLookTime.val,13.0f * uiGazeLookTime.val,pExtraversion/100.0f) && gAvoid == 0.0f)
				{
					if (playerHeadToHead <= personalSpaceDistance && amGlancing == false && mEyesClosedLeftTarget < 0.5f && currentLook != "Intense" && interestValence < 9.5f)
					{
						//SuperController.LogError("within personal, not glancing, not closed, not intense and not super happy");
						if (mainInterest == "Face" && Vector3.Distance(new Vector3(0.0f,0.0f,0.0f),saccadeOffset) < 25.0f * uiSaccadeAmount.val && playerHeadMovement == false)
						{
							//SuperController.LogError("face target, not saccaded and player head not moving");
							gAvoidanceClock += Time.fixedDeltaTime * (2.0f * (1.0f-(interestArousal / 10.0f)));
						}
					}
					if ((amGlancing == true || Vector3.Distance(new Vector3(0.0f,0.0f,0.0f),saccadeOffset) > 25.0f * uiSaccadeAmount.val || mEyesClosedLeftTarget > 0.5f) && gAvoidanceClock > 0.0f)
					{
						//SuperController.LogError("glancing or saccaded or closed and clock > 0");
						gAvoidanceClock -= Time.fixedDeltaTime / 3.0f;
					}
				}
				else
				{
					//SuperController.LogError("gavoid = " + gAvoid + " clock " + gAvoidanceClock + " / " + Mathf.Lerp(1.5f * uiGazeLookTime.val,13.0f * uiGazeLookTime.val,pExtraversion/100.0f));
					if (gAvoidanceClock >= Mathf.Lerp(1.5f * uiGazeLookTime.val,13.0f * uiGazeLookTime.val,pExtraversion/100.0f) && gAvoid == 0.0f)// && Random.Range(0.0f,100.0f) > Mathf.Lerp(99.0f,99.99f,interestArousal/10.0f))
					{
						if (currentInterest != "Face" || playerHeadToHead > uiCloseToFaceDist.val*2.0f || (interestArousal < 5.0f && playerHeadToHead > personalSpaceDistance/4.0f))
						{
							gAvoid = 1.0f;
							gAvoidInterest = mainInterest;
							gAvoidingClock = Mathf.Lerp(4.0f * uiGazeAvoidTime.val, 0.5f * uiGazeAvoidTime.val, interestValence/10.0f) * Mathf.Lerp(2.0f,0.75f,pExtraversion/100.0f);
							//eyesSM.Switch(eClosed);
							if (morphMouthAction == false && testRun == false)
							{
								if (interestValence > 5.0f)
								{
									mouthSM.SwitchRandom(new State[] {
												mBiteLip,
												mBiteLip,
												mSmirk,
												mSideways
											});
                      //SuperController.LogError("Mouth a");
								}
								else
								{
									mouthSM.SwitchRandom(new State[] {
												mSmile,
												mSmile,
												mSmile,
												mSmirk,
												mSideways
											});
                      //SuperController.LogError("Mouth b");
								}
							}
							//mouthSM.Switch(mBigSmile);
							if (secondInterest == "RandomL" || secondInterest == "RandomR")
							{
								if (secondInterest == "RandomL")
								{
									if (lookAwaySide == "right" && eyeClock > 1.5f * uiBlinkSpeed.val * blinkRepeat)
									{
										//eyesSM.Switch(eBlink);
										//eyeClock = 0.0f;
										//SuperController.LogError("Look away, blink");
									}
									lookAwaySide = "left";
								}
								else
								{
									if (lookAwaySide == "left" && eyeClock > 1.5f * uiBlinkSpeed.val * blinkRepeat)
									{
										//eyesSM.Switch(eBlink);
										//eyeClock = 0.0f;
										//SuperController.LogError("Look away, blink");
									}
									lookAwaySide = "right";
								}
							}
							else
							{
								if (Random.Range(0.0f, Mathf.Lerp(35.0f, 100.0f, interestValence/10.0f)) < 32.0f)
								{
									if (lookAwaySide == "right" && eyeClock > 1.5f * uiBlinkSpeed.val * blinkRepeat)
									{
										//eyesSM.Switch(eBlink);
										//eyeClock = 0.0f;
										//SuperController.LogError("Look away, blink");
									}
									lookAwaySide = "left";
								}
								else
								{
									if (lookAwaySide == "left" && eyeClock > 1.5f * uiBlinkSpeed.val * blinkRepeat)
									{
										//eyesSM.Switch(eBlink);
										//eyeClock = 0.0f;
										//SuperController.LogError("Look away, blink");
									}
									lookAwaySide = "right";
								}
							}
						}
					}
				}
				//SuperController.LogError("Gaze Avoid Done");
			}
			else
			{
				gAvoid = 0.0f;
				gAvoidanceClock = 0.0f;
				gAvoidingClock = 0.0f;
			}
			
			if (((playerLHandToPelvis < interactionDistance * 1.5f) || (playerRHandToPelvis < interactionDistance * 1.5f) || playerHeadToPelvis < interactionDistance * 1.5f || emTargetPelvisDistance < interactionDistance * 1.5f) && vagTouchCount > 0 && uiDoSex.val)
			{
				interestArousal += 0.15f;
				if (interestArousal > 6.0f)
				{
					if (currentLook != "Sex" && lookAction == false && testRun == false)
					{
						lookSM.Switch(lSex);
					}
				}
				else
				{
					if (currentLook != "Sex" && lookAction == false && interestArousal > 8.0f && testRun == false)
					{
						lookSM.Switch(lSex);
					}
					else
					{
						if (currentLook != "Feel" && lookAction == false && currentLook != "Sex" && testRun == false)
						{
							lookSM.Switch(lFeel);
						}
					}
				}

			}
      
      if ((currentLook == "Idle" || currentLook == "Playful" || currentLook == "Intense") && vagTouchCount > 0 && uiDoSex.val && interestArousal > 5.0f)
      {
			lookSM.Switch(lSex);
			}
			
			//SuperController.LogError("Checking for face Idle");
			if (lookAction == false && Random.Range(0.0f,100.0f) > Mathf.Lerp(99.99f,98.0f,interestArousal/10.0f) && currentLook != "Sex" && currentLook != "Sucking" && currentLook != "Kissing" && testRun == false && Random.Range(0.0f,100.0f) < uiExpressionChance.val)
			{
				if (amGlancing == false && gAvoid == 0.0f && playerPenisInteract && playerHeadToHead > personalSpaceDistance/2.0f)
				{
					if (playerTipToHead < interactionDistance && lipsTouchCount > 0.0f)
					{
						if (uiDoBlowjob.val && testRun == false)
						{
							lookSM.Switch(lSucking);
						}
					}
					else
					{
						if (uiDoSex.val && testRun == false)
						{
							lookSM.Switch(lSex);
						}
					}
				}
				else
				{
					if (interestArousal < 5.0f)
					{
						if (interestValence < 5.0f)
						{
							  lookSM.Switch(lCasual);
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestValence/10.0f))
							  {
								lookSM.Switch(lInquisitive);
							  }
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(40.0f, 00.0f, interestArousal/10.0f))
							  {
								lookSM.Switch(lBored);
							  }
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(60.0f, 0.0f, interestValence/10.0f))
							  {
								lookSM.Switch(lDayDream);
							  }
						}
						else
						{
							  lookSM.Switch(lCasual);
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestValence/10.0f))
							  {
								lookSM.Switch(lInquisitive);
							  }
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 40.0f, interestArousal/10.0f))
							  {
								lookSM.Switch(lPlayful);
							  }
						}
					}
					else
					{
						if (interestValence < 5.0f)
						{
							  lookSM.Switch(lCasual);
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestValence/10.0f))
							  {
								lookSM.Switch(lInquisitive);
							  }
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 60.0f, interestArousal/10.0f))
							  {
								lookSM.Switch(lPlayful);
							  }
						}
						else
						{
							if (interestArousal > 8.0f && interestValence > 8.0f && playerHeadToFaceRot > playerLookDirectAngle)
							{
								lookSM.Switch(lFeel);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 10.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
							}
							else
							{
								lookSM.Switch(lFeel);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lInquisitive);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, interestArousal/10.0f))
								{
								  lookSM.Switch(lPlayful);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 50.0f, (interestArousal + interestValence) / 20.0f))
								{
								  lookSM.Switch(lIntense);
								}
							}
						}
					}
				}
				
			}
			
			//SuperController.LogError("Checking for motion interact");
			if (lookAction == false && interestArousal >= Mathf.Lerp(6.5f, 8.5f, pExtraversion/100.0f) && interestValence > Mathf.Lerp(8.5f, 6.0f, pAgreeableness/100.0f) && currentLook != "Feel" && currentLook != "Sex" && currentLook != "Sucking" && currentLook != "Kissing" && testRun == false)
			{
				if ((playerLHandInteract && playerLHandMovement) || (playerRHandInteract && playerRHandMovement) || (playerHeadInteract && playerHeadMovement) || (playerPenisInteract && playerTipMovement))
				{
					lookSM.Switch(lFeel);
				}
			}
			
			if (playerLHandToPelvis < interactionDistance * 1.3f && vagTouchCount > 0.0f && uiDoSex.val && testRun == false)
			{
				if (playerLHandMovement)
				{
					if (interestArousal > 4.0f)
					{
						if (currentLook != "Feel" && currentLook != "Sex")
						{
							lookSM.Switch(lFeel);
						}
					}
					if (interestArousal > 9.0f)
					{
						if (currentLook != "Sex")
						{
							lookSM.Switch(lSex);
						}
					}
				}
				else
				{
					if (interestArousal > 7.0f)
					{
						if (currentLook != "Feel" && currentLook != "Sex")
						{
							lookSM.Switch(lFeel);
						}
					}
					if (interestArousal > 9.9f)
					{
						if (currentLook != "Sex")
						{
							lookSM.Switch(lSex);
						}
					}
				}
			}
			if (playerRHandToPelvis < interactionDistance * 1.3f && vagTouchCount > 0.0f && uiDoSex.val && testRun == false)
			{
				if (playerRHandMovement)
				{
					if (interestArousal > 4.0f)
					{
						if (currentLook != "Feel" && currentLook != "Sex")
						{
							lookSM.Switch(lFeel);
						}
					}
					if (interestArousal > 9.0f)
					{
						if (currentLook != "Sex")
						{
							lookSM.Switch(lSex);
						}
					}
				}
				else
				{
					if (interestArousal > 7.0f)
					{
						if (currentLook != "Feel" && currentLook != "Sex")
						{
							lookSM.Switch(lFeel);
						}
					}
					if (interestArousal > 9.9f)
					{
						if (currentLook != "Sex")
						{
							lookSM.Switch(lSex);
						}
					}
				}
			}
			//SuperController.LogError("Checking for Blowjob look timeout");
            if (lookAction == false)
            {
                if (playerTipToHead < interactionDistance * 1.05f && uiDoBlowjob.val && lipsTouchCount > 0.0f)
                {
                    lookSM.Switch(lSucking);
					if (mainInterest != "Tip" && mainInterest != "Pelvis")
					{
						mainInterest = "Tip";
						interestTip = 100.0f;
						//SuperController.LogError("|| blowjob Set Main Interest to " + mainInterest);
						//SuperController.LogError("Start Blowjob, switch to Tip");
					}
                }
            }

			//SuperController.LogError("Calculating mood decay");
            if (currentInterestLevel > 70.0f && playerHeadToHead < personalSpaceDistance)
            {
                playerInterest += (currentInterestLevel - 70.0f) / 200.0f;
            }
            else
			{
				if (currentInterestLevel > 40.0f && playerHeadToHead < personalSpaceDistance)
				{
					playerInterest += (currentInterestLevel - 40.0f) / 500.0f;
				}
				else
				{
					if (playerHeadToHead > backgroundDistance)
					{
						playerInterest -= 0.1f;
					}
					else
					{
						playerInterest -= 0.05f;
					}
				}
            }
            playerInterest = Mathf.Clamp(playerInterest, 0.0f, 100.0f);


            interestArousal = Mathf.Clamp(interestArousal - (0.002f * uiMoodSpeed.val * (1.0f+(interestPeakArousal/10.0f))), 3.0f, 10.0f);
			if (playerHeadToHead > personalSpaceDistance)
			{
				interestValence = Mathf.Clamp(interestValence - (Mathf.Lerp(0.0043f, 0.0081f, Mathf.Clamp(playerHeadToHead - personalSpaceDistance, 0.0f, 1.0f)) * uiMoodSpeed.val * (1.0f+(interestPeakValence/5.0f))), 2.0f, 10.0f);
			}
			else
			{
				interestValence = Mathf.Clamp(interestValence - (0.0043f * uiMoodSpeed.val * (1.0f+(interestPeakValence/5.0f))), 2.0f, 10.0f);
			}
			
			if (Vector3.Distance(headController.followWhenOff.position, eyeController.transform.position) < closeFaceDistance)
			{
				if ((mainInterest == "LHand" || mainInterest == "RHand" || mainInterest == "Face") && headToFaceRot < lookNoAwarenessAngle)
				{
					currentInterest = "Face";
					//SuperController.LogError("Faces very close, set current to face");
				}
				else
				{
					if (currentEye != "Closed")
					{
						eyesSM.Switch(eClosed);
						//SuperController.LogError("Closing due to eye target being close to face");
					}
				}
			}
			
			if ((playerLHandInteract && playerRHandInteract) || playerHeadInteract)
			{
				if (playerHeadToFaceRot > playerLookDirectAngle && playerHeadToHead < closeFaceDistance * 1.7f)// && (playerLHandMovement || playerRHandMovement))
				{
					if (currentLook != "Intense")
					{
						//gAvoid = 1.0f;
						
						if ((playerLHandToLBreast < interactionDistance || playerLHandToRBreast < interactionDistance) && (playerRHandToLBreast < interactionDistance || playerRHandToRBreast < interactionDistance))
						{
							if (currentInterest != "Face")
							{
								currentInterest = "RandomU";
								//SuperController.LogError("Multiple interact not looking at face, look up");
							}
						}
						else
						{
							if (headToFaceRot < lookPeripheralAngle)
							{
								currentInterest = "Face";
								//SuperController.LogError("Multiple interact set current to face");
							}
							else
							{
								gAvoid = 1.0f;
							}
						}
					}
				}
			}

			if ((playerLHandToHead < closeFaceDistance || playerRHandToHead < closeFaceDistance) && lipsTouchCount > 0.0f && interestArousal > 3.0f)
			{
				if (interestKissing == false && testRun == false && currentLook != "Kissing")// && (currentMouth == "Idle" || currentLook == "Playful"))
				{
					lipsOnly = true;
					interestKissing = true;
					currentLook = "Kiss";
					lookSM.Switch(lKissing);
					lookVariation = 0.1f;
					//SuperController.LogError("Finger Kiss");
				}
			}


			//SuperController.LogError("Look Position start");
            if (currentInterest == "RandomF")
            {
                //headController.transform.LookAt(randomPointForward);
                lookAtPosition = randomPointForward;
				if (uiEyeControl.val)
				{
					eyeController.transform.position = randomPointForward;
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
				}
            }
            if (currentInterest == "Target")
            {
                //headController.transform.LookAt(randomPointForward);
				if (emTargetName == "[CameraRig]")
				{
					lookAtPosition = CameraTarget.centerTarget.transform.position;
					if (uiEyeControl.val)
					{
						eyeController.transform.position = CameraTarget.centerTarget.transform.position;
						focusPos = eyeController.transform.position;
						focusRot = eyeController.transform.rotation;
					}
				}
				else
				{
					lookAtPosition = emTargetTransform.position;
					if (uiEyeControl.val)
					{
						eyeController.transform.position = emTargetTransform.position;
						focusPos = eyeController.transform.position;
						focusRot = eyeController.transform.rotation;
					}
				}
            }
            if (currentInterest == "RandomL")
            {
                //headController.transform.LookAt(randomPointLeft);
                lookAtPosition = randomPointLeft;
				if (uiEyeControl.val)
				{
					eyeController.transform.position = randomPointLeft;
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
				}
            }
            if (currentInterest == "RandomR")
            {
                //headController.transform.LookAt(randomPointRight);
                lookAtPosition = randomPointRight;
				if (uiEyeControl.val)
				{
					eyeController.transform.position = randomPointRight;
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
				}
            }
            if (currentInterest == "RandomU")
            {
                //headController.transform.LookAt(randomPointUp);
                lookAtPosition = randomPointUp;
				if (uiEyeControl.val)
				{
					eyeController.transform.position = randomPointUp;
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
				}
            }
            if (currentInterest == "Face")
            {
					if (usePerson2 && person2Usable)
					{
						lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.06f, 0.07f));
					}
					else
					{
						lookAtPosition = playerHeadTransform.position;
					}
				fuzzyLock = 1.0f;
            }
            if (currentInterest == "Chest" && usePerson2 && person2Usable)
            {
                lookAtPosition = playerChestController.followWhenOff.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
				if (uiEyeControl.val)
				{
					eyeController.transform.position = lookAtPosition;
				}
                fuzzyLock = 2.0f;
            }
            if (currentInterest == "Pelvis" && usePerson2 && person2Usable)
            {
                lookAtPosition = playerPelvisController.followWhenOff.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
                fuzzyLock = 2.0f;
            }
            if (currentInterest == "LHand" && (usePerson2 || playerHandsUsable))
            {
				if (playerLHandToHead < closeFaceDistance)
				{
//					lookAtPosition = playerLHandTransform.TransformPoint(new Vector3(0.2f, 0.0f, 0.2f));
					if (usePerson2 == false)
					{
						lookAtPosition = playerLHandTransform.TransformPoint(new Vector3(0.07f, 0.05f, -0.2f));
					}
					else
					{
						lookAtPosition = playerLHandTransform.TransformPoint(new Vector3(0.2f, -0.07f, 0.05f));
						
					}
					lookAtPosition.y = playerHeadTransform.position.y;
					if (uiDoHead.val)
					{
						headController.RBHoldRotationSpring = Mathf.Lerp(05,15,interestValence/10.0f);
						headController.RBHoldRotationDamper = Mathf.Lerp(4,13,interestArousal/10.0f);
					}
				}
				else
				{
					lookAtPosition = playerLHandTransform.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
				}
				peronalityAdjustV = 10.0f;
                fuzzyLock = 2.0f;
            }
			
            if (currentInterest == "RHand" && (usePerson2 || playerHandsUsable))
            {
				if (playerRHandToHead < closeFaceDistance)
				{
					if (usePerson2 == false)
					{
						lookAtPosition = playerRHandTransform.TransformPoint(new Vector3(-0.07f, 0.05f, -0.2f));
					}
					else
					{
						lookAtPosition = playerRHandTransform.TransformPoint(new Vector3(-0.2f, -0.07f, 0.05f));
					}
					lookAtPosition.y = playerHeadTransform.position.y;
					if (uiDoHead.val)
					{
						headController.RBHoldRotationSpring = Mathf.Lerp(05,15,interestValence/10.0f);
						headController.RBHoldRotationDamper = Mathf.Lerp(4,13,interestArousal/10.0f);
					}
				}
				else
				{
					lookAtPosition = playerRHandTransform.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
				}
				peronalityAdjustV = 10.0f;
                fuzzyLock = 2.0f;
            }
            if (currentInterest == "Tip" && usePerson2 && person2Usable)
            {
                if (playerTipToHead < personalSpaceDistance)// && Random.Range(0.0f,100.0f) > pExtraversion + 25.0f)
                {
                    if (Mathf.Abs(headToFaceRot) <= lookNoAwarenessAngle && interestValence > 8.0f && lipsTouchCount > 1.0f)
                    {
                        lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
                        fuzzyLock = 3.0f;
						interestValence -= 0.5f;
                    }
                    else
                    {
						if (playerTipToHead > 0.125f || lipsTouchCount <= 0.0f)
						{
							lookAtPosition = playerTipController.followWhenOff.TransformPoint(new Vector3(0.0f, Mathf.Lerp(0.22f,0.0f,Mathf.Min(playerTipToHead, 1.0f)), 0.0f));
						}
						else
						{
							gHeadSpeed = 2.5f;
							lookAtPosition = playerTipBaseController.followWhenOff.TransformPoint(new Vector3(0.0f, 0.1f, -0.3f));
						}
						fuzzyLock = 0.0f;
                    }
                }
                else
                {
					lookAtPosition = playerTipController.followWhenOff.position;
                    fuzzyLock = 2.0f;
                }
            }
            if (secondClock <= 0.0f && amGlancing)
            {
                amGlancing = false;
				eyeUpdateClock = eyeUpdateTime + 1.0f;
            }
			
			if (interestKissing && uiDoKiss.val)
			{
				tempFloat = 0.0f;
				if (usePerson2 == false || person2Usable == false)
				{
					tempFloat = 0.0f;
				}
				if (person2IsMale)
				{
					lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.0f,0.055f,0.02f));
				}
				else
				{
					if (person2Usable && usePerson2 && playerHeadController.possessed)
					{
						lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.0f,0.038f,0.02f));
					}
					else
					{
						lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.0f,-0.027f,0.02f));
					}
				}
				if (gHeadRoll > 10.0f)
				{
					//lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.01f, 0.00f, tempFloat));
				}
				if (gHeadRoll < -10.0f)
				{
					//lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(-0.01f, 0.00f, tempFloat));
				}
				fuzzyLock = 0.25f;
				if (eyesSM.CurrentState != eClosed)
				{
					//eyesSM.Switch(eClosed);
				}
			}
			//SuperController.LogError("Look Position Done");
		
            if (currentInterest == "PLHand")
            {
				lookAtPosition = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
				peronalityAdjustV = 10.0f;
                fuzzyLock = 2.0f;
            }
            if (currentInterest == "PRHand")
            {
				lookAtPosition = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
				peronalityAdjustV = 10.0f;
                fuzzyLock = 2.0f;
            }
			
			if (playerHeadToHead > personalSpaceDistance && playerInterest <= 10.0f && amGlancing == false && interestKissing == false)
			{
				//gAvoid = 1.0f;
			}
			
			//SuperController.LogError("BlowJob / Sex Check");
			if (amGlancing == false && gAvoid == 0.0f && playerPenisInteract && lookAction == false && playerHeadToHead > personalSpaceDistance/2.0f && testRun == false)
			{
				if (playerTipToHead < interactionDistance)
				{
					if (uiDoBlowjob.val && currentLook != "Sucking" && lipsTouchCount > 0.0f)
					{
						lookSM.Switch(lSucking);
					}
				}
				else
				{
					if (uiDoSex.val && currentLook != "Sex" && vagTouchCount > 0.0f)
					{
						lookSM.Switch(lSex);
					}
				}
			}

				//SuperController.singleton.ClearMessages();
				//SuperController.LogMessage("Full Face (0.15)" + Round(mSmileFullFaceValue) + "|Open Full Face (0.15)" + Round(mSmileOpenFullFaceValue) + "|Simple Left (0.2)" + Round(mSmileSimpleLeftValue) + "|Simple Right (0.2)" + Round(mSmileSimpleRightValue) + "|Excitment (0.2)" + Round(mExcitementValue) + "|Happy (0.2)" + Round(mHappyValue), false);
				if (smiledlast)
				{
					//SuperController.LogMessage("Smiled Last", false);
				}
				
				if (smileTimer > Mathf.Lerp(10.0f * uiSmileSuppression.val, 15.0f * uiSmileSuppression.val, interestValence/10.0f) && suppressTimer <= 0.0f)
				{
					suppressTimer = Mathf.Lerp(10.0f * uiSmileSuppression.val, 5.0f * uiSmileSuppression.val, interestValence/10.0f);
					
				}

				if (suppressTimer > 0.0f)
				{
					suppressSmile = true;
					mSmileFullFaceTarget = 0.0f;
					mSmileOpenFullFaceTarget = 0.0f;
					mSmileSimpleLeftTarget = 0.0f;
					mSmileSimpleRightTarget = 0.0f;
					mExcitementTarget = 0.0f;
					mHappyTarget = 0.0f;
					mMouthOpenTarget = 0.0f;
					mMouthOpenWideTarget = 0.0f;
					mMouthOpenWiderTarget = 0.0f;
					smileTimer = 0.0f;
					//SuperController.LogMessage("suppressing smile", false);
					
				}
				if ((mSmileFullFaceValue < 0.05f + Mathf.Clamp(uiSmileOffset.val, 0.0f, 0.1f) && mSmileOpenFullFaceValue < 0.05f + Mathf.Clamp(uiSmileOffset.val, 0.0f, 0.1f) && mSmileSimpleLeftValue < 0.5f && mSmileSimpleRightValue < 0.5f && mExcitementValue < 0.2f && mHappyValue < 0.2f) && smileTimer > 0.0f)
				{
					//SuperController.LogMessage("Reset Smile Timer " + Round(smileTimer) + "|" + Round(suppressTimer));
					smileTimer = Mathf.Max(smileTimer - (Time.fixedDeltaTime * 1.5f), 0.0f);
				}
				
				if (mSmileFullFaceTarget > 0.05f + Mathf.Clamp(uiSmileOffset.val, 0.0f, 0f) || mSmileOpenFullFaceTarget > 0.05f || mSmileSimpleLeftTarget > 0.1f || mSmileSimpleRightTarget > 0.1f || mExcitementTarget > 0.15f || mLipBiteTarget > 0.2f || mTakingItTarget > 0.1f || mHappyTarget > 0.25f)
				{
					//mMouthOpenTarget = 0.0f;
					mMouthOpenWideTarget = 0.0f;
					mMouthOpenWiderTarget = 0.0f;
					mExcitementTarget = 0.0f;
				}
				
				if (currentMouth == "Idle" || currentMouth == "Idle(S)")
				{
					mHappyTarget = 0.0f;
					mExcitementTarget = 0.0f;
					mTakingItTarget = 0.0f;
					mMouthSideLeftTarget = 0.0f;
					mMouthSideRightTarget = 0.0f;
				}
				
				if (currentBrow == "Idle")
				{
					mBrowCenterUpTarget = mBrowCenterUpTarget * 0.8f;
					mBrowUpTarget = mBrowUpTarget * 0.8f;
					mBrowOuterUpLeftTarget = mBrowOuterUpLeftTarget * 0.8f;
					mBrowOuterUpRightTarget = mBrowOuterUpRightTarget * 0.8f;
				}
				

			tempFloat = 1.0f;
			if (amGlancing == true)
			{
				tempFloat = 0.0f;
				saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
				lastSaccade = "";
			}
			if (glanceTimeout >= 0.0f)
			{
				glanceTimeout -= Time.fixedDeltaTime;
			}
			
			if (uiEyeControl.val && mEyesClosedLeftValue < 0.7f)
			{
				//SuperController.LogError("Eye Position start");
				//			if (((Random.Range(0.0f,100000.0f) / 1000.0f > Mathf.Clamp(100.00f - ((100.0f-pStableness)/100.0f),99.15f,99.899f) || secondClock > 0.0f) && mainClock > Mathf.Clamp(6.0f * ((100.0f-pExtraversion)/100),1.5f,5.0f) && playerHeadToHead > closeFaceDistance) || amGlancing == true)
				if (((Random.Range(0.0f, 100.0f) > Mathf.Lerp(40.2f,76.95f,pStableness/100.0f) || (gAvoid == 1.0f && Random.Range(0.0f, 100.0f) > Mathf.Lerp(20.2f,56.95f,pStableness/100.0f))) || secondClock > 0.0f || amGlancing == true) && (glanceTimeout <= 0.0f || amGlancing == true) && uiGazeGlance.val && (gAvoidanceClock < Mathf.Lerp(1.5f * uiGazeLookTime.val,13.0f * uiGazeLookTime.val,pExtraversion/100.0f) * 0.5f || gAvoid == 1.0f))
				//if (1.0f == 1.0f)
				{
					amGlancing = false;
					if (currentInterest != "Face" || playerHeadToHead > uiCloseToFaceDist.val*2.0f || (interestArousal < 5.0f && playerHeadToHead > personalSpaceDistance/3.0f))
					{
						//SuperController.LogError("Glancing at Second");
						if (mainInterest == secondInterest)
						{
							//eyeController.transform.position = chestController.followWhenOff.position;
							//eyeController.transform.rotation = chestController.followWhenOff.rotation;
							//eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
							if (lookAwaySide == "left" && Mathf.Abs(Vector3.Angle(randomPointLeft - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
							{
								eyeController.transform.position = randomPointLeft;
								eyeController.transform.rotation = chestController.followWhenOff.rotation;
								//eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
								amGlancing = true;
							}
							if (lookAwaySide == "right" && Mathf.Abs(Vector3.Angle(randomPointRight - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
							{
								eyeController.transform.position = randomPointRight;
								eyeController.transform.rotation = chestController.followWhenOff.rotation;
								//eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
								amGlancing = true;
							}
							if (lookAwaySide == "up" && Mathf.Abs(Vector3.Angle(randomPointUp - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
							{
								eyeController.transform.position = randomPointUp;
								eyeController.transform.rotation = chestController.followWhenOff.rotation;
								//eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
								amGlancing = true;
							}
							if (lookAwaySide == "down" && Mathf.Abs(Vector3.Angle(randomPointForward - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
							{
								eyeController.transform.position = randomPointForward;
								eyeController.transform.rotation = chestController.followWhenOff.rotation;
								//eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
								amGlancing = true;
							}
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
						}
						
						if (secondInterest == "RandomL" && Mathf.Abs(Vector3.Angle(randomPointLeft - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							//lookAtPosition = playerPelvisController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							eyeController.transform.position = randomPointLeft;
							eyeController.transform.rotation = chestController.followWhenOff.rotation;
							eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 3.5f;
						}
						if (secondInterest == "RandomR" && Mathf.Abs(Vector3.Angle(randomPointRight - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							//lookAtPosition = playerPelvisController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							eyeController.transform.position = randomPointRight;
							eyeController.transform.rotation = chestController.followWhenOff.rotation;
							eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 3.5f;
						}
						if (secondInterest == "RandomU" && Mathf.Abs(Vector3.Angle(randomPointUp - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							//lookAtPosition = playerPelvisController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							eyeController.transform.position = randomPointUp;
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 3.5f;
						}
						if (secondInterest == "RandomF" && Mathf.Abs(Vector3.Angle(randomPointForward - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							//lookAtPosition = playerPelvisController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							eyeController.transform.position = randomPointForward;
							eyeController.transform.rotation = chestController.followWhenOff.rotation;
							eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 3.5f;
						}
						if (secondInterest == secondOld && secondInterest != "RandomF" && Mathf.Abs(Vector3.Angle(randomPointForward - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							eyeController.transform.position = randomPointForward;
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
						}
						if (secondInterest == "Face" && interestFace+headActivityBoost > interestFaceBase*0.75f && headToFaceRot < lookPeripheralAngle)
						{
							//if (gAvoid == 0.0f)
							//{
							//lookAtPosition = playerHeadController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							//}

							if (person2Usable && usePerson2)
							{
								eyeController.transform.position = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
								eyeController.transform.rotation = playerHeadTransform.rotation;
								eyeController.transform.Translate(0.0f, 0.03f, 0.07f);
								focusPos = eyeController.transform.position;
								focusRot = eyeController.transform.rotation;
							}
							else
							{
								eyeController.transform.position = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.00f, 0.00f));
								focusPos = eyeController.transform.position;
								focusRot = eyeController.transform.rotation;
							}
							amGlancing = true;
							//fuzzyLock = 1.5f;
						}
						if (((secondInterest == "LHand" && interestLHand + lHandActivityBoost > interestLHandBase*0.75f) || (secondInterest == "RandomL" && interestLHand + lHandActivityBoost > interestLHandBase*0.6f)) && Mathf.Abs(Vector3.Angle(playerLHandTransform.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f)) - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							//lookAtPosition = playerLHandController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							eyeController.transform.position = playerLHandTransform.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 2.5f;
						}
						if (((secondInterest == "RHand" && interestRHand + rHandActivityBoost > interestRHandBase*0.75f) || (secondInterest == "RandomR" && interestRHand + rHandActivityBoost > interestRHandBase*0.6f)) && Mathf.Abs(Vector3.Angle(playerRHandTransform.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f)) - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							//lookAtPosition = playerRHandController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							eyeController.transform.position = playerRHandTransform.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;

							amGlancing = true;
							//fuzzyLock = 2.5f;
						}
						if (secondInterest == "Pelvis" && usePerson2 && interestPelvis > interestPelvisBase*0.75f && Mathf.Abs(Vector3.Angle(playerPelvis - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							//lookAtPosition = playerPelvisController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							eyeController.transform.position = playerPelvis;
							eyeController.transform.rotation = chestController.followWhenOff.rotation;
							eyeController.transform.Translate(0.0f, 0.0f, 1.0f);
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 3.5f;
						}
						if (secondInterest == "Tip" && headToTipRot < lookNoAwarenessAngle && usePerson2 && interestTip > interestTipBase*0.75f && Mathf.Abs(Vector3.Angle(playerTip - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							//lookAtPosition = playerTipController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							eyeController.transform.position = playerTip;
							eyeController.transform.rotation = chestController.followWhenOff.rotation;
							eyeController.transform.Translate(0.0f, 0.0f, 1.0f);
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 3.5f;
						}
						if (secondInterest == "Target" && interestEMTarget > interestEMTargetBase * 0.75f && Mathf.Abs(Vector3.Angle(emTargetTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							//lookAtPosition = playerTipController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
							eyeController.transform.position = emTargetTransform.position;
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 3.5f;
						}
						if (secondInterest == "PLHand" && Mathf.Abs(Vector3.Angle(lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f)) - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							eyeController.transform.position = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 2.5f;
						}
						if (secondInterest == "PRHand" && Mathf.Abs(Vector3.Angle(rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f)) - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle)
						{
							eyeController.transform.position = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							amGlancing = true;
							//fuzzyLock = 2.5f;
						}
						if (gAvoid == 1.0f)
						{
							//secondClock = 0.0f;//Time.fixedDeltaTime;
							glanceTimeout = 0.0f;
							if (gAvoidInterest == "Face")
							{
								if (person2Usable)
								{
									eyeController.transform.position = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
									eyeController.transform.rotation = playerHeadTransform.rotation;
									eyeController.transform.Translate(0.0f, 0.03f, 0.07f);
									focusPos = eyeController.transform.position;
									focusRot = eyeController.transform.rotation;
								}
								else
								{
									eyeController.transform.position = playerHeadTransform.position;
									focusPos = eyeController.transform.position;
									focusRot = eyeController.transform.rotation;
								}
								amGlancing = true;
							}
							if (gAvoidInterest == "LHand")
							{
								eyeController.transform.position = playerLHandTransform.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
								focusPos = eyeController.transform.position;
								focusRot = eyeController.transform.rotation;
								amGlancing = true;
							}
							if (gAvoidInterest == "RHand")
							{
								eyeController.transform.position = playerRHandTransform.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
								focusPos = eyeController.transform.position;
								focusRot = eyeController.transform.rotation;
								amGlancing = true;
							}
							if ((gAvoidInterest == "Pelvis" || gAvoidInterest == "Tip") && usePerson2)
							{
								eyeController.transform.position = playerPelvis;
								focusPos = eyeController.transform.position;
								focusRot = eyeController.transform.rotation;
								amGlancing = true;
							}
							if (gAvoidInterest == "PLHand")
							{
								eyeController.transform.position = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
								focusPos = eyeController.transform.position;
								focusRot = eyeController.transform.rotation;
								amGlancing = true;
							}
							if (gAvoidInterest == "PRHand")
							{
								eyeController.transform.position = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
								focusPos = eyeController.transform.position;
								focusRot = eyeController.transform.rotation;
								amGlancing = true;
							}
						}
						if (secondClock <= 0.0f && amGlancing)
						{
							secondClock = Random.Range(Mathf.Lerp(0.85f, 0.45f, interestValence/10.0f), Mathf.Lerp(1.0f, 1.65f, interestArousal/10.0f));
							//SuperController.LogError("Glancing");
						}
						if (amGlancing == false)
						{
							secondClock = 0.0f;
						}
						else
						{
							if (tempFloat == 1.0f && eyeClock > (1.25f * uiBlinkSpeed.val) * blinkRepeat && currentEye != "Closed")
							{
								//eyesSM.Switch(eBlink);
								//eyeClock = 0.0f;
								//SuperController.LogError("glance in progress blink");
							}
							gHeadSpeed = 2.0f;
							glanceTimeout = Mathf.Lerp(4.0f,10.0f,pExtraversion/100.0f) * uiGlanceTimeout.val;
						}
					}
				}
				else
				{
					//SuperController.LogError("Looking at main");
					gHeadSpeed = 1.0f;
					if (mainInterest == "Face" && (gAvoid != 1.0f || Random.Range(0.0f, 1.00f) < interestArousal / 10.0f))
					{
						if (person2Usable && usePerson2)
						{
							eyeController.transform.position = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
							if (currentInterest == "Chest")
							{
								eyeController.transform.position = playerChestController.followWhenOff.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
							}
						}
						else
						{
							eyeController.transform.position = playerHeadTransform.position;
						}
						eyeController.transform.rotation = playerHeadTransform.rotation;
						focusPos = eyeController.transform.position;
						focusRot = eyeController.transform.rotation;
					}
					if (mainInterest == "Pelvis" && usePerson2 && person2Usable)
					{
						if (playerTipToHead < closeFaceDistance || headToTipRot > 45.0f)
						{
							eyeController.transform.position = playerPelvis;
							if (playerTipToHead < interactionDistance || headToTipRot > 45.0f)
							{
								if (mainInterest == "Face" || mainOld == "Face" || secondInterest == "Face" || secondOld == "Face")
								{
									eyeController.transform.rotation = playerHeadTransform.rotation;
									eyeController.transform.position = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
								}
								if (mainInterest == "LHand" || mainOld == "LHand" || secondInterest == "LHand" || secondOld == "LHand")
								{
									eyeController.transform.rotation = playerLHandTransform.rotation;
									eyeController.transform.position = playerLHandTransform.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
								}
								if (mainInterest == "RHand" || mainOld == "RHand" || secondInterest == "RHand" || secondOld == "RHand")
								{
									eyeController.transform.rotation = playerLHandTransform.rotation;
									eyeController.transform.position = playerRHandTransform.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
								}
							}
							else
							{
								eyeController.transform.position = playerPelvis;
							}
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
						}
						else
						{
							eyeController.transform.position = playerPelvis;
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
						}
						
					}
					if (mainInterest == "LHand" && (usePerson2 || playerHandsUsable))
					{
						if (playerLHandToHead < closeFaceDistance * 1.5f)
						{
							eyeController.transform.position = playerHeadTransform.position;
							eyeController.transform.rotation = playerHeadTransform.rotation;
							eyeController.transform.Translate(0.0f, 0.03f, 0.07f);
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
						}
						else
						{
							eyeController.transform.position = playerLHandTransform.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
							eyeController.transform.rotation = playerLHandTransform.rotation;
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							//eyeController.transform.Translate(-0.1f, 0.0f, 0.0f);
						}
					}
					if (mainInterest == "RHand" && (usePerson2 || playerHandsUsable))
					{
						if (playerRHandToHead < closeFaceDistance * 1.5f)
						{
							eyeController.transform.position = playerHeadTransform.position;
							eyeController.transform.rotation = playerHeadTransform.rotation;
							eyeController.transform.Translate(0.0f, 0.03f, 0.07f);
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
						}
						else
						{
							eyeController.transform.position = playerRHandTransform.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
							eyeController.transform.rotation = playerRHandTransform.rotation;
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
							//eyeController.transform.Translate(0.1f, 0.0f, 0.0f);
						}
					}
					if (mainInterest == "Tip" && usePerson2 && person2Usable)
					{
						if (playerTipToHead < closeFaceDistance || headToTipRot > 45.0f)
						{
							if (playerTipToHead < interactionDistance || headToTipRot > 45.0f)
							{
								eyeController.transform.rotation = playerHeadTransform.rotation;
								eyeController.transform.position = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
								focusPos = eyeController.transform.position;
								focusRot = eyeController.transform.rotation;
							}
							else
							{
								eyeController.transform.position = playerTip;
								focusPos = eyeController.transform.position;
								focusRot = eyeController.transform.rotation;
								//eyeController.transform.rotation = playerTipController.followWhenOff.rotation;
								//eyeController.transform.Translate(0.0f, 0.05f, -0.17f);
							}
						}
						else
						{
							eyeController.transform.position = playerTip;
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
						}
					}
					if (mainInterest == "PLHand")
					{
							eyeController.transform.position = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
							eyeController.transform.rotation = lHandController.followWhenOff.rotation;
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
					}
					if (mainInterest == "PRHand")
					{
							eyeController.transform.position = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
							eyeController.transform.rotation = rHandController.followWhenOff.rotation;
							focusPos = eyeController.transform.position;
							focusRot = eyeController.transform.rotation;
					}
				}
			}
				

            secondClock = Mathf.Clamp(secondClock - Time.fixedDeltaTime, 0.0f, 10.0f);
            mainClock += Time.fixedDeltaTime;
            if (mainClock > 3.0f && mainSwitch)
            {
                mainSwitch = false;
				gazeAdjust = 1.0f;
            }
            if (secondClock <= 0.0f && secondSwitch)
            {
                secondSwitch = false;
            }


			
			//SuperController.LogError("Eye Position Done");
			
			if (gAvoid == 1.0f && interestKissing == false)
			{
				//SuperController.LogError("Doing avoidance");
				//interestArousal += 0.0006f;
				//SuperController.LogMessage("Avoiding" + gAvoidance, false);
				//saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
				if (lookAwaySide == "right")
				{
					//mainOld = mainInterest;
					mainInterest = "RandomR";
					currentInterest = mainInterest;
					//SuperController.LogError("|| avoiding Set Main Interest to " + mainInterest);
					lookAtPosition = randomPointRight;//playerRHandTransform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
					fuzzyLock = 2.0f;
					float lookAngle2 = Vector3.Angle(randomPointRight - headController.followWhenOff.position, headController.followWhenOff.forward);
					float lookdist2 = Vector3.Distance(randomPointRight, headController.followWhenOff.position);

					if (amGlancing == false && lookdist2 > uiCloseToFaceDist.val * 1.5f && lookAngle2 < lookNoAwarenessAngle)
					{
						eyeController.transform.position = randomPointRight;//playerRHandTransform.position;
						focusPos = eyeController.transform.position;
						focusRot = eyeController.transform.rotation;
					}
					//SuperController.LogError("Avoid set side to right");
					//gHeadRollTarget = Random.Range(-25.0f,-5.0f);
				}
				else
				{
					//mainOld = mainInterest;
					mainInterest = "RandomL";
					currentInterest = mainInterest;
					//SuperController.LogError("|| avoiding Set Main Interest to " + mainInterest);
					//SuperController.LogError("4");
					lookAtPosition = randomPointLeft;//playerLHandTransform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
					fuzzyLock = 2.0f;
					float lookAngle2 = Vector3.Angle(randomPointLeft - headController.followWhenOff.position, headController.followWhenOff.forward);
					float lookdist2 = Vector3.Distance(randomPointLeft, headController.followWhenOff.position);
					if (amGlancing == false && lookdist2 > uiCloseToFaceDist.val * 1.5f && lookAngle2 < lookNoAwarenessAngle)
					{
						eyeController.transform.position = randomPointLeft;//playerLHandTransform.position;
						focusPos = eyeController.transform.position;
						focusRot = eyeController.transform.rotation;
					}
					//SuperController.LogError("Avoid set side to left");
					//gHeadRollTarget = Random.Range(25.0f,5.0f);
				}
			}
			
			if (uiEyeControl.val && mEyesClosedLeftValue < 0.7f)
			{
				if ((playerLHandToHead < closeFaceDistance || playerRHandToHead < closeFaceDistance) && mainInterest != "Face")
				{
					eyeController.transform.position = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
					eyeController.transform.rotation = playerHeadTransform.rotation;
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
				}
			}

			if (mEyesClosedLeftValue > 0.9f && morphBlinking == false)
			{
				//eyeController.transform.position = headController.transform.TransformPoint(new Vector3(0.0f, 0.00f, 1.0f));
			}
		
			if (interestKissing && uiDoKiss.val)
			{
				fuzzyLock = 0.25f;
				gHeadSpeed = 0.2f;
			}
			
			if ((playerHeadToHead <= personalSpaceDistance / 2.0f && mainInterest == "Face") || currentLook == "Intense")
			{
				//fuzzyLock = 0.25f;
				//gHeadSpeed = 0.7f;
				//peronalityAdjustH = peronalityAdjustH * 0.995f;
				//peronalityAdjustV = peronalityAdjustV * 0.995f;
			}
			//SuperController.LogError("Interaction checks Done");

			
			//if (playerHeadToHead <= Mathf.Max(personalSpaceDistance / 3.0f,closeFaceDistance*2.0f) && currentLook != "Sucking" && currentLook != "Kissing" && mainInterest == "Face")
			//{
				//tempFloat = playerHeadTransform.eulerAngles.z - headController.followWhenOff.eulerAngles.z;
				//if (tempFloat > 180.0f){tempFloat -= 360.0f;}
				//if (tempFloat < -180.0f){tempFloat += 360.0f;}

				//gHeadRollTarget = Mathf.Clamp(tempFloat + Random.Range(-25.0f,25.0f),-40.0f,40.0f);
			//}
			//SuperController.LogError("Blink calculation");
			
			//saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);

			bool redoEyePos = false;


			
			//if (currentEye == "Idle" && mEyesClosedLeftValue > 0.5f)
			//{
			//	mEyesClosedLeftTarget = eyeOpenMaxMorph;
			//	mEyesClosedRightTarget = eyeOpenMaxMorph;
			//}
			
			if (1==1)//(lookOverride == false)
			{
				float headToLook = Mathf.Abs(Vector3.Angle(lookAtPosition - headController.followWhenOff.position, headController.followWhenOff.forward));
				float headToMain = 999.0f;
				Vector3 mainPos = new Vector3(0.0f,0.0f,0.0f);
				if (mainInterest == "Face")
				{
					if (person2Usable && usePerson2)
					{
						headToMain = Mathf.Abs(Vector3.Angle(playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)) - headController.followWhenOff.position, headController.followWhenOff.forward));
						mainPos = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
					}
					else
					{
						headToMain = Mathf.Abs(Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
						mainPos = playerHeadTransform.position;
					}
					if (headToMain > lookNoAwarenessAngle)
					{
						interestFace -= 0.04f;
					}
				}
				if (mainInterest == "LHand")
				{
					headToMain = Mathf.Abs(Vector3.Angle(playerLHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					mainPos = playerLHandTransform.position;
					if (headToMain > lookNoAwarenessAngle)
					{
						interestLHand -= 0.04f;
					}
				}
				if (mainInterest == "RHand")
				{
					headToMain = Mathf.Abs(Vector3.Angle(playerRHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					mainPos = playerRHandTransform.position;
					if (headToMain > lookNoAwarenessAngle)
					{
						interestRHand -= 0.04f;
					}
				}
				if ((mainInterest == "Pelvis" || mainInterest == "Penis") && usePerson2 && person2Usable)
				{
					headToMain = Mathf.Abs(Vector3.Angle(playerPelvis - headController.followWhenOff.position, headController.followWhenOff.forward));
					mainPos = playerPelvis;
					if (headToMain > lookNoAwarenessAngle)
					{
						interestPelvis -= 0.04f;
						interestTip -= 0.04f;
					}
				}
				if (mainInterest == "Target" && emTargetName != "None")
				{
					headToMain = Mathf.Abs(Vector3.Angle(emTargetTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					mainPos = emTargetTransform.position;
					if (headToMain > lookNoAwarenessAngle)
					{
						interestEMTarget -= 0.04f;
					}
				}
				if (mainInterest == "PLHand")
				{
					headToMain = Mathf.Abs(Vector3.Angle(lHandController.followWhenOff.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					mainPos = lHandController.followWhenOff.position;
					if (headToMain > lookNoAwarenessAngle)
					{
						interestPLHand -= 0.04f;
					}
				}
				if (mainInterest == "PRHand")
				{
					headToMain = Mathf.Abs(Vector3.Angle(rHandController.followWhenOff.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					mainPos = rHandController.followWhenOff.position;
					if (headToMain > lookNoAwarenessAngle)
					{
						interestPRHand -= 0.04f;
					}
				}
				
				float headToSecond = 999.0f;
				Vector3 secondPos = new Vector3(0.0f,0.0f,0.0f);
				if (secondInterest == "Face")
				{
					if (person2Usable && usePerson2)
					{
						headToSecond = Mathf.Abs(Vector3.Angle(playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)) - headController.followWhenOff.position, headController.followWhenOff.forward));
						secondPos = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
					}
					else
					{
						headToSecond = Mathf.Abs(Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
						secondPos = playerHeadTransform.position;
					}
					if (headToSecond > lookNoAwarenessAngle)
					{
						interestFace -= 0.01f;
					}
				}
				if (secondInterest == "LHand" && interestLHand > 30.0f)
				{
					headToSecond = Mathf.Abs(Vector3.Angle(playerLHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					secondPos = playerLHandTransform.position;
					if (headToSecond > lookNoAwarenessAngle)
					{
						interestLHand -= 0.01f;
					}
				}
				if (secondInterest == "RHand" && interestRHand > 30.0f)
				{
					headToSecond = Mathf.Abs(Vector3.Angle(playerRHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					secondPos = playerRHandTransform.position;
					if (headToSecond > lookNoAwarenessAngle)
					{
						interestRHand -= 0.01f;
					}
				}
				if (secondInterest == "Pelvis" || secondInterest == "Penis")
				{
					headToSecond = Mathf.Abs(Vector3.Angle(playerPelvis - headController.followWhenOff.position, headController.followWhenOff.forward));
					secondPos = playerPelvis;
					if (headToSecond > lookNoAwarenessAngle)
					{
						interestPelvis -= 0.01f;
						interestTip -= 0.01f;
					}
				}
				if (secondInterest == "Target" && emTargetName != "None")
				{
					headToSecond = Mathf.Abs(Vector3.Angle(emTargetTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					secondPos = emTargetTransform.position;
					if (headToSecond > lookNoAwarenessAngle)
					{
						interestEMTarget -= 0.01f;
					}
				}
				if (secondInterest == "PLHand")
				{
					headToSecond = Mathf.Abs(Vector3.Angle(lHandController.followWhenOff.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					secondPos = lHandController.followWhenOff.position;
					if (headToSecond > lookNoAwarenessAngle)
					{
						interestPLHand -= 0.01f;
					}
				}
				if (secondInterest == "PRHand")
				{
					headToSecond = Mathf.Abs(Vector3.Angle(rHandController.followWhenOff.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					secondPos = rHandController.followWhenOff.position;
					if (headToSecond > lookNoAwarenessAngle)
					{
						interestPRHand -= 0.01f;
					}
				}

				float headToLeft = Mathf.Abs(Vector3.Angle(randomPointLeft - headController.followWhenOff.position, headController.followWhenOff.forward));
				float headToRight = Mathf.Abs(Vector3.Angle(randomPointRight - headController.followWhenOff.position, headController.followWhenOff.forward));
				float headToUp = Mathf.Abs(Vector3.Angle(randomPointUp - headController.followWhenOff.position, headController.followWhenOff.forward));
				float headToForward = Mathf.Abs(Vector3.Angle(randomPointForward - headController.followWhenOff.position, headController.followWhenOff.forward));
				float headToTarget = 999.0f;
				//testString = lookAngle.ToString() + "|" + headToLook.ToString() + "|" + headToMain.ToString();
				if (emTargetName != "None")
				{
					headToTarget = Mathf.Abs(Vector3.Angle(emTargetTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
				}
				
				if (amGlancing == false)
				{
					if (headToLook > lookNoAwarenessAngle)
					{
						if (headToMain < lookPeripheralAngle)
						{
							//lookAtPosition = mainPos;
							//currentInterest = mainInterest;
						}
						else
						{
							if (headToSecond < lookDirectAngle)
							{
								lookAtPosition = secondPos;
								currentInterest = secondInterest;
								lookOverride = true;
								//SuperController.LogError("Main @ High Angle, Looking at Second");
							}
							else
							{
								if (headToTarget < lookDirectAngle + 20.0f)
								{
									lookAtPosition = emTargetTransform.position;
									currentInterest = "Target";
									lookOverride = true;
									//SuperController.LogError("Main+Second @ High Angle, Looking at Target");
								}
								else
								{
									if (headToLeft < lookDirectAngle + 20.0f && headToLeft < headToRight)
									{
										lookAtPosition = randomPointLeft;
										currentInterest = "RandomL";
										//SuperController.LogError("5");
										lookOverride = true;
										//SuperController.LogError("Main+Second+Target @ High Angle, Looking at Left Random");
									}
									else
									{
										if (headToRight < lookDirectAngle + 20.0f && headToRight < headToUp)
										{
											lookAtPosition = randomPointRight;
											currentInterest = "RandomR";
											lookOverride = true;
											//SuperController.LogError("Main+Second+Target @ High Angle, Looking at Right Random");
										}
										else
										{
											if (headToUp < lookDirectAngle && headToUp < headToForward)
											{
												lookAtPosition = randomPointUp;
												currentInterest = "RandomU";
												lookOverride = true;
												//SuperController.LogError("Main+Second+Target @ High Angle, Looking at Up Random");
											}
											else
											{
												if (headToForward < lookPeripheralAngle)
												{
													lookAtPosition = randomPointForward;
													currentInterest = "RandomF";
													lookOverride = true;
													//SuperController.LogError("Main+Second+Target @ High Angle, Looking at Forward Random");
												}
											}
										}
									}
								}
							}
						}
					}
					if (lookOverride)
					{
						lookOverrideTimer = 2.0f;
						//SuperController.LogError("Look Override");
					}
				}
				else
				{
					lookOverrideTimer -= Time.fixedDeltaTime;
					if (lookOverrideTimer <= 0.0f)
					{
						lookOverrideTimer = 0.0f;
						lookOverride = false;
					}
				}
			}

			float lookAngle = Vector3.Angle(eyeController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
			float lookdist = Vector3.Distance(eyeController.transform.position, headController.followWhenOff.position);
			if (uiEyeControl.val == false)
			{
				lookAngle = 0.0f;
				lookdist = uiPersonalSpace.val;
			}
			
			if (lookdist > uiCloseToFaceDist.val)
			{
				tempFloat = uiPeripheralGaze.val;
				if (amGlancing)
				{
					tempFloat = lookNoAwarenessAngle;
				}
				if (lookAngle > tempFloat)
				{
					if (currentEye != "Closed" || mEyesClosedLeftValue < 0.5f)
					{
						if (amGlancing)
						{
							amGlancing = false;
							glanceTimeout = 0.0f;
						}
						else
						{
							if (gAvoid == 1.0f)
							{
								//gAvoid = 0.0f;
								//gAvoidanceClock = 0.0f;
								//gAvoidingClock = 0.0f;
							}
							else
							{
								//currentInterest = "RandomF";
								//mainInterest = currentInterest;
								//lookAtPosition = randomPointForward;
								eyeController.transform.position = lookAtPosition;
								focusPos = eyeController.transform.position;
								lookAngle = Vector3.Angle(randomPointLeft - headController.followWhenOff.position, headController.followWhenOff.forward);
								if (lookAngle < tempFloat)
								{
									currentInterest = "RandomL";
									mainInterest = currentInterest;
									lookAtPosition = randomPointLeft;
									eyeController.transform.position = randomPointLeft;
									focusPos = eyeController.transform.position;
								}
								lookAngle = Vector3.Angle(randomPointRight - headController.followWhenOff.position, headController.followWhenOff.forward);
								if (lookAngle < tempFloat)
								{
									currentInterest = "RandomR";
									mainInterest = currentInterest;
									lookAtPosition = randomPointRight;
									eyeController.transform.position = randomPointRight;
									focusPos = eyeController.transform.position;
								}
								if (person2Usable && usePerson2 && interestArousal > 5.0f)
								{
									lookAngle = headToPelvisRot;
									if (lookAngle < tempFloat)
									{
										currentInterest = "Pelvis";
										mainInterest = currentInterest;
										lookAtPosition = playerPelvisController.transform.position;
										eyeController.transform.position = playerPelvisController.transform.position;
										focusPos = eyeController.transform.position;
									}
								}
								lookAngle = headToLHandRot;
								if (lookAngle < tempFloat)
								{
									currentInterest = "LHand";
									mainInterest = currentInterest;
									lookAtPosition = playerLHandTransform.position;
									eyeController.transform.position = playerLHandTransform.position;
									focusPos = eyeController.transform.position;
								}
								lookAngle = headToRHandRot;
								if (lookAngle < tempFloat)
								{
									currentInterest = "RHand";
									mainInterest = currentInterest;
									lookAtPosition = playerRHandTransform.position;
									eyeController.transform.position = playerRHandTransform.position;
									focusPos = eyeController.transform.position;
								}
								lookAngle = headToFaceRot;
								if (lookAngle < tempFloat)
								{
									currentInterest = "Face";
									mainInterest = currentInterest;
									lookAtPosition = playerFace;
									eyeController.transform.position = playerFace;
									focusPos = eyeController.transform.position;
								}
								lookAngle = Vector3.Angle(eyeController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
								if (uiEyeControl.val)
								{
									if (lookAngle > tempFloat)
									{
										//eyesSM.Switch(eClosed);
										//SuperController.LogError("Closing eyes due to angle " + lookAngle.ToString() + " " + lookdist.ToString());
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if (uiEyeControl.val)
				{
					if (currentInterest != "Face")
					{
						eyesSM.Switch(eClosed);
						//SuperController.LogError("Too Close, closing", false);
					}
				}
			}
			
			eyeMoveDist = Vector3.Distance(oldEyePos, eyeController.transform.position);
      //if (eyeMoveDist > 0.0f)
      //{
        //SuperController.LogError("Eye Distance " + Round(eyeMoveDist), false);
      //}
      oldEyePos = eyeController.transform.position;


            Vector3 centrePoint = chestController.followWhenOff.position;
            Vector3 eyePoint = eyeController.transform.position;
			if (currentEye == "Closed")
			{
				if (mainInterest == "Face")
				{
					eyePoint = playerFace;
				}
				if (mainInterest == "LHand")
				{
					eyePoint = playerLHand;
				}
				if (mainInterest == "RHand")
				{
					eyePoint = playerRHand;
				}
			}
            float c2eActual = Vector3.Distance(centrePoint, eyePoint);
            centrePoint.y = 0.0f;
            eyePoint.y = 0.0f;
            float c2eDist = Vector3.Distance(centrePoint, eyePoint);
			//if (uiEyeControl.val == false)
			//{
				//c2eDist = uiPersonalSpace.val;
			//}
			//SuperController.singleton.ClearErrors();
			//SuperController.LogError("chest to eye dist : " + Round(c2eDist) + "|" + Round(closeFaceDistance * 3.0f), false);
            if (c2eDist < closeFaceDistance * 3.0f && (currentInterest != "Face" || amGlancing) && mEyesClosedLeftValue < 0.5f)
            {
				//SuperController.LogError("Pushing eye target forward", false);
                eyeController.transform.position = eyeController.transform.position + (chestController.followWhenOff.forward * 0.1f);
				focusPos = eyeController.transform.position;
				//SuperController.LogError("Pushed", false);
            }

			if (currentEye == "Blink" || currentEye == "Closed")
			{
				eyeMoveDist = 0.0f;
			}
      
			  if (currentEye != "Closed" && currentEye != "Blink" && interestKissing == false)
			  {
				if (eyeMoveDist > uiEyeMoveBlinkDist.val && eyeClock > (0.25f * uiBlinkSpeed.val) * blinkRepeat)
				{
				  //SuperController.LogError("large eye movement, blink " + Round(eyeMoveDist) + "|" + eyeClock + "(" + ((0.5f * uiBlinkSpeed.val) * blinkRepeat) +  ")");
				  eyesSM.Switch(eBlink);
				  eyeClock = 0.0f;
				}
				if (eyeMoveDist > uiEyeMoveBlinkDist.val * 100.0f)
				{
				  //SuperController.LogError("larger eye movement, blink " + Round(eyeMoveDist) + "|" + eyeClock + "(" + ((0.85f * uiBlinkSpeed.val) * blinkRepeat) + ")");
				  eyesSM.Switch(eBlink);
				  eyeClock = 0.0f;
				}
			}
			  if (interestKissing == false)
			  {
				//if (((Random.Range(0.0f, (((15.0f - (10.0f - interestArousal)) / (2.0f * (interestArousal))) * (1.0f / Random.Range(1.333f, 2.5f))) * Time.fixedDeltaTime) / 300.0f) - (Mathf.Max(eyeClock - 1.0f, 0.0f) / 5000000.0f) <= 0.0f) && eyeClock > 1.0f)
				//if (((Random.Range(0.0f,(1.0f / Random.Range(1.333f,3.5f)) * Time.fixedDeltaTime) / 10.0f) - (Mathf.Max(eyeClock-1.0f,0.0f) / 5000000.0f) <= 0.0f) && eyeClock > 0.5f)
				if (eyeClock > 2.0f * uiBlinkSpeed.val * blinkRepeat && Random.Range(0.0f,100.0f) > Mathf.Lerp(99.4f - (interestValence / 100.0f),99.7f - (interestArousal / 100.0f),pExtraversion / 100.0f)) 
				  {
					eyeClock = 0.0f;
					eyesSM.Switch(eBlink);
					//SuperController.LogError("Regular, blink");
				  }
			  }
			if  ((eyeClock > 0.3f && morphBlinking && currentEye != "Closed") || (currentEye != "Closed" && currentEye != "Blink" && mEyesClosedLeftValue > 0.5f && morphBlinking == false))
			{
				morphBlinking = false;
				eyesSM.Switch(eOpen);
				eyeClock = 0.0f;
				//SuperController.LogError("Force open");
			}
			
			lookAngle = Vector3.Angle(eyeController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
			if ((lookAngle > uiPeripheralGaze.val || lookdist < uiCloseToFaceDist.val * 1.5f))// && currentEye != "Closed")
			{
				if (uiEyeControl.val)
				{
					//eyesSM.Switch(eClosed);
					//mEyesClosedLeftTarget = 1.0f;
					//mEyesClosedRightTarget = 1.0f;
					//SuperController.LogError("Closing eyes due to angle " + lookAngle.ToString() + " " + lookdist.ToString());
				}
			}
			eyeClock += Time.fixedDeltaTime;

            //eyeController.transform.Translate(randomX,randomY,0.0f);

			
			//SuperController.LogError("Calculating Head Rotation");
            Transform head = headController.transform;
			Transform eye = eyeController.transform;
            Transform reference = chestController.followWhenOff;

            Vector3 actualDir = reference.InverseTransformDirection(head.forward);
			tempFloat = 0.0f;
			if ((currentLook == "Intense" || currentLook == "Playful") && mainInterest == "Face" && gAvoid == 0.0f && amGlancing == false)
			{
				//tempFloat = Mathf.Lerp(0.05f, 0.15f, Mathf.Clamp(playerHeadToHead - (closeFaceDistance*3.0f), 0.0f, 1.0f));
			}
            Vector3 targetDir = lookAtPosition - head.position;
			//SuperController.singleton.ClearMessages();
			if (playerHeadToFaceRot > playerLookDirectAngle && playerHeadToHead < closeFaceDistance * 1.5f)// && currentInterest == "Face")
			{
				targetDir = head.forward;
				//SuperController.LogMessage("Too close! " + playerHeadToFaceRot.ToString(), false);
			}
            targetDir.Normalize();
            targetDir = reference.InverseTransformDirection(targetDir);
            Vector2 actualDirH = new Vector2(actualDir.x, actualDir.z);
            Vector2 targetDirH = new Vector2(targetDir.x, targetDir.z);
            Vector2 actualDirV = new Vector2(actualDirH.magnitude, actualDir.y);
            Vector2 targetDirV = new Vector2(targetDirH.magnitude, targetDir.y);
            actualDirH.Normalize();
            targetDirH.Normalize();
            actualDirV.Normalize();
            targetDirV.Normalize();
			if (amGlancing)
			{
				targetDirV = actualDirV;
			}
            actualH = Mathf.Atan2(actualDirH.x, actualDirH.y);
            targetH = Mathf.Atan2(targetDirH.x, targetDirH.y);
            actualV = Mathf.Atan2(actualDirV.y, actualDirV.x);
            targetV = Mathf.Atan2(targetDirV.y, targetDirV.x);

            headToEyeController = Mathf.Abs(Vector3.Angle(eyeController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
			if (uiEyeControl.val == false)
			{
				headToEyeController = 0.0f;
			}
			

            if (headToEyeController > lookDirectAngle * 0.85f || (playerHeadToHead < uiCloseToFaceDist.val*2.0f && headToEyeController > eyesNonDirectAngle) || Vector3.Distance(new Vector3(0.0f,0.0f,0.0f), saccadeOffset) > 15.0f)
            {
				if (gAvoid == 0.0f && amGlancing == false)
				{
					eyesNonDirectClock += Time.fixedDeltaTime * Mathf.Lerp(0.55f, 2.8f, interestValence / 10.0f);
				}
            }
            else
            {
				if ((amGlancing == false && gAvoid == 0.0f) || headToEyeController <= lookDirectAngle * 0.85f)
				{
					eyesNonDirectClock = 0.0f;
					//SuperController.LogError("reset eye");
				}
            }
			
			//10.0f - ((100.0f - pExtraversion) / 10.0f) - eyesNonDirectClock < 0.0f
            if (( Mathf.Lerp(7.0f * uiGazeDirectLookDelay.val, 3.0f * uiGazeDirectLookDelay.val, interestArousal/10.0f) - eyesNonDirectClock < 0.0f || headDelayTimer > 10.0f * uiGazeDirectLookDelay.val) && interestKissing == false && gAvoid == 0.0f)// || playerHeadToHead < closeFaceDistance * 2.0f)
            {
				if (adjustTimeout <= adjustWaitTime / 2.0f)
				{
					peronalityAdjustH = peronalityAdjustH * (1.0f - (uiIndirectDecay.val / 10.0f));
					peronalityAdjustV = peronalityAdjustV * (1.0f - (uiIndirectDecay.val / 10.0f));
	//				//SuperController.LogError("Reducing Adjustment");
					//fuzzyLock = fuzzyLock - Mathf.Lerp(0.0f, fuzzyLock, Mathf.Clamp(eyesNonDirectClock - (5.0f * uiGazeDirectLookDelay.val), 0.0f, 1.0f));
					//eyesNonDirectClock = 0.0f;
				}
				
            }
			
			if (Mathf.Abs(gHeadRoll - gHeadRollTarget) > 20.0f && adjustTimeout <= 0.0f)
			{
				peronalityAdjustH = 0.0f;
				if (currentInterest != "LHand" && currentInterest != "RHand")
				{
					peronalityAdjustV = 0.0f;				
				}
			}
			
			if (currentLook == "Sucking")
			{
				peronalityAdjustH = 0.0f;
				peronalityAdjustV = 0.0f;
                fuzzyLock = 0.0f;
			}

			if (gAvoid == 1.0f)
			{
				peronalityAdjustH = 0.0f;
				peronalityAdjustV = 0.0f;
			}
			
			if (mainInterest == "Tip")
			{
				fuzzyLock = 0.0f;
				gHeadSpeed = gHeadSpeed / 5.0f;
			}
			
            // adjust angles
			

				if ((currentInterest == "Face" && playerHeadMovement) || (currentInterest == "LHand" && playerLHandMovement) || (currentInterest == "RHand" && playerRHandMovement))
				{
					tempFloat = 0.005f;
					if (headToFaceRot > lookPeripheralAngle)
					{
						tempFloat = 0.03f;
						//SuperController.LogMessage("Head Rotation > Peripheral", false);
					}
					else
					{
						if (headToFaceRot > lookDirectAngle)
						{
							tempFloat = 0.01f;
							//SuperController.LogMessage("Head Rotation > Look", false);
							if (currentLook == "Inquisitive" || currentLook == "Playful")
							{
								tempFloat = 0.0225f;
							}
						}
					}
					if (peronalityAdjustH < 0.0f)
					{
						peronalityAdjustH = Mathf.Min(peronalityAdjustH + tempFloat, 0.0f);
					}
					else
					{
						peronalityAdjustH = Mathf.Max(peronalityAdjustH - tempFloat, 0.0f);
					}

					if (peronalityAdjustV < 0.0f)
					{
						peronalityAdjustV = Mathf.Min(peronalityAdjustV + (tempFloat * 5.0f), 0.0f);
					}
					else
					{
						peronalityAdjustV = Mathf.Max(peronalityAdjustV - (tempFloat * 5.0f), 0.0f);
					}
				}

			tempFloat = 0.003f * uiGazeSpeed.val;
			if (playerHeadToHead < uiCloseToFaceDist.val*2.0f)
			{
				tempFloat = 0.006f * uiGazeSpeed.val;
			}
			tempFloat2 = Mathf.Clamp(peronalityAdjustH, -(lookPeripheralAngle * 0.75f) * uiGazeVariation.val, (lookPeripheralAngle * 0.75f) * uiGazeVariation.val) * Mathf.Deg2Rad;

			if (gAvoid == 0.0f)
			{
				
				if (endAdjustH < tempFloat2)
				{
					endAdjustH = Mathf.Clamp(endAdjustH + tempFloat, endAdjustH, tempFloat2);
				}
				else
				{
					endAdjustH = Mathf.Clamp(endAdjustH - tempFloat, tempFloat2, endAdjustH);
				}
				
				//tempFloat2 = Mathf.Clamp(peronalityAdjustV, -15.0f * uiGazeVariation.val, 0.0f * uiGazeVariation.val);
				tempFloat2 = Mathf.Clamp(peronalityAdjustV * fuzzyLockActual, -(lookPeripheralAngle * 0.5f) * uiGazeVariation.val, (lookPeripheralAngle * 0.15f) * uiGazeVariation.val) * Mathf.Deg2Rad;
				if (endAdjustV < tempFloat2)
				{
					endAdjustV = Mathf.Clamp(endAdjustV + tempFloat, endAdjustV, tempFloat2);
				}
				else
				{
					endAdjustV = Mathf.Clamp(endAdjustV - tempFloat, tempFloat2, endAdjustV);
				}			
				
			}
			
			if (endAdjustV > 0.0f)
			{
				endAdjustV = Mathf.Max(0.0f, endAdjustV - Mathf.Lerp(0.0f, 0.01f, interestArousal/10.0f));
			}
			headToEyeController = Mathf.Abs(Vector3.Angle(eyeController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
			//if (uiEyeControl.val == false)
			//{
			//	headToEyeController = 0.0f;
			//}
			if (uiDoHead.val)
			{
				//velocityH = Mathf.Clamp(velocityH, -0.5f, 0.5f);
				//velocityV = Mathf.Clamp(velocityV, -2.0f, 2.0f);
				/*tempFloat = 0.02f * uiGazeSpeed.val;
				if (Mathf.Abs(velocityLastH - velocityH) > tempFloat)
				{
					if (velocityLastH - velocityH < 0)
					{
						velocityH = velocityLastH + tempFloat;
					}
					else
					{
						velocityH = velocityLastH - tempFloat;
					}
				}
				if (Mathf.Abs(velocityLastH - velocityH) > tempFloat)
				{
					if (velocityLastH - velocityH < 0)
					{
						velocityH = velocityLastH + tempFloat;
					}
					else
					{
						velocityH = velocityLastH - tempFloat;
					}
				}
				tempFloat = 0.01f * uiGazeSpeed.val;
				if (Mathf.Abs(velocityLastV - velocityV) > tempFloat)
				{
					if (velocityLastV - velocityV < 0)
					{
						velocityV = velocityLastV + tempFloat;
					}
					else
					{
						velocityV = velocityLastV - tempFloat;
					}
				}
				if (Mathf.Abs(velocityLastV - velocityV) > tempFloat)
				{
					if (velocityLastV - velocityV < 0)
					{
						velocityV = velocityLastV + tempFloat;
					}
					else
					{
						velocityV = velocityLastV - tempFloat;
					}
				}*/
				velocityLastH = velocityH;
				velocityLastV = velocityV;				
				//SuperController.LogError("Adjusting head rotation");
				if (targetH < 0.0f)
				{
					endAdjustH = Mathf.Clamp(endAdjustH,(-uiGazeMaxSideways.val * Mathf.Deg2Rad) - targetH,uiGazeMaxSideways.val * Mathf.Deg2Rad);
				}
				else
				{
					endAdjustH = Mathf.Clamp(endAdjustH,-uiGazeMaxSideways.val * Mathf.Deg2Rad,(uiGazeMaxSideways.val * Mathf.Deg2Rad) - targetH);
				}
				endAdjustV = Mathf.Clamp(endAdjustV,-uiGazeMaxDown.val * Mathf.Deg2Rad, uiGazeMaxUp.val * Mathf.Deg2Rad);
				
				tempFloat = endAdjustH;
				if (Mathf.Abs(endAdjustH - lastAdjustH) > 10.0f * Mathf.Deg2Rad)
				{
					if (lastAdjustH < endAdjustH)
					{
						//peronalityAdjustH = lastAdjustH + 10.0f * Mathf.Deg2Rad;
					}
					else
					{
						//peronalityAdjustH = lastAdjustH - 10.0f * Mathf.Deg2Rad;
					}
				}
				lastAdjustH = tempFloat;
				lastAdjustV = endAdjustV;
				
				//SuperController.singleton.ClearErrors();
				adjustedSpeed = Mathf.Clamp((Mathf.Lerp(gHeadSpeed * 0.55f * gazeAdjust, gHeadSpeed * 1.0f * gazeAdjust,((interestArousal+interestValence)/2.0f)/10.0f)) / uiGazeSpeed.val,0.1f, 90.0f); // - 0.5f + (closeFaceDistance / playerHeadToHead)
				//adjustedSpeed = Mathf.Lerp(adjustedSpeed, adjustedSpeed/10.0f, Mathf.Clamp(Mathf.Abs(targetH - actualH) / 2.0f, 0.0f, 3.0f));
				if (currentLook == "Sex" && playerHeadToHead >= uiCloseToFaceDist.val*2.0f)
				{
					adjustedSpeed = adjustedSpeed * Mathf.Lerp(5.0f,10.0f,pExtraversion/100.0f);
				}
				if (playerTipToHead < interactionDistance)
				{
					adjustedSpeed = adjustedSpeed / 10.0f;
				}
				if (mainInterest == "RandomR" || mainInterest == "RandomL" || mainInterest == "RandomU" || mainInterest == "RandomF")
				{
					if (currentLook == "Bored" || currentLook == "Daydream")
					{
						adjustedSpeed = adjustedSpeed * 2.0f;
					}
					else
					{
						adjustedSpeed = adjustedSpeed * 6.0f;
					}
				}
				if ((mainOld == "RandomR" || mainOld == "RandomL" || mainOld == "RandomU" || mainOld == "RandomF") && playerHeadToHead > backgroundDistance)
				{
					adjustedSpeed = adjustedSpeed;
				}
				if (lipsTouchCount > 0.0f && playerHeadToHead > kissingDistance * 1.1f && (playerLHandToHead < closeFaceDistance || playerRHandToHead < closeFaceDistance))
				{
					adjustedSpeed = adjustedSpeed * 50.0f;
				}
				if (gAvoid == 1.0f)
				{
					adjustedSpeed = adjustedSpeed * 2.0f;
					//velocityH = velocityH * 0.9f;
				}
				if (amGlancing)
				{
					adjustedSpeed = adjustedSpeed * 5.0f;
					//velocityH = velocityH * 0.9f;
				}
				

				/*
				if (Mathf.Abs(actualH - targetH) < (5.0f * (pAgreeableness / 100.0f) * fuzzyLock) * Mathf.Deg2Rad)// || (Mathf.Abs(actualH - targetH) > 65.0f * Mathf.Deg2Rad && Mathf.Abs(actualH) < Mathf.Abs(targetH)))// || eyesSM.CurrentState == eClosed)
				{
					targetH = actualH + (velocityH * 0.5f);
				}
				if (Mathf.Abs(actualV - targetV) < (2.0f * (pExtraversion / 100.0f) * fuzzyLock) * Mathf.Deg2Rad)// || (Mathf.Abs(actualV - targetV) > 55.0f * Mathf.Deg2Rad && Mathf.Abs(actualV) < Mathf.Abs(targetV)))// || eyesSM.CurrentState == eClosed)
				{
					targetV = actualV + (velocityV * 0.5f);
				}*/

				
				hFuzz = (uiGazeMaxSideways.val * 0.15f) * lookVariation * uiGazeVariation.val;
				vFuzz = (uiGazeMaxUp.val * 0.1f) * lookVariation * uiGazeVariation.val;
				if (targetV < 0.0f)
				{
					vFuzz = vFuzz / 2.0f;
				}
				if (playerHeadMovement)
				{
					//hFuzz = Mathf.Lerp(hFuzz, 0.0f, Mathf.Clamp((headToFaceRot - lookDirectAngle) / 5.0f, 0.0f, 1.0f));
					//vFuzz = Mathf.Lerp(vFuzz, 0.0f, Mathf.Clamp((headToFaceRot - lookDirectAngle) / 5.0f, 0.0f, 1.0f));
				}
				else
				{
					//hFuzz = Mathf.Lerp(hFuzz, 0.0f, Mathf.Clamp(Mathf.Abs(Round(actualH * Mathf.Rad2Deg)) - uiGazeMaxSideways.val, 0.0f, 1.0f));
					//vFuzz = Mathf.Lerp(vFuzz, 0.0f, Mathf.Clamp(Mathf.Abs(Round(actualV * Mathf.Rad2Deg)) - uiGazeMaxUp.val, 0.0f, 1.0f));
				}
				if (playerTipToHead < interactionDistance && currentLook == "Tip")
				{
					hFuzz = 0.0f;
					vFuzz = 0.0f;
				}
				if (gAvoid == 1.0f)
				{
					//headDelayTimer = 0.0f;
				}
				else
				{
					if (headToEyeController > lookPeripheralAngle)
					{
						headDelayTimer += Time.fixedDeltaTime * 5.0f;
						
					}
					else
					{
						headDelayTimer += Time.fixedDeltaTime * Mathf.Lerp(0.5f,5.0f, Mathf.Clamp(headToEyeController - lookDirectAngle, 0.0f, lookPeripheralAngle) / lookPeripheralAngle);
					}
				}
				
					if (Mathf.Abs(actualH - Mathf.Clamp(targetH + (endAdjustH), -uiGazeMaxSideways.val * Mathf.Deg2Rad, uiGazeMaxSideways.val * Mathf.Deg2Rad)) < (lookDirectAngle / 1.0f) * Mathf.Deg2Rad && Mathf.Abs(actualV - (targetV + endAdjustV)) < (lookDirectAngle / 1.0f) * Mathf.Deg2Rad && headDelayTimer > Mathf.Lerp(10.0f * uiGazeDirectLookDelay.val, 5.0f * uiGazeDirectLookDelay.val, interestValence/10.0f) && Mathf.Lerp(fuzzyLock, 0.0f, Mathf.Clamp(headDelayTimer/10.0f, 0.0f, 1.0f)) <= 0.0f)
					{
						headDelayTimer = 0.0f;
						hFuzz = 0.0f;
						vFuzz = 0.0f;
					}
					
				if (uiIndirectDecay.val <= 0.0f)
				{
					headDelayTimer = 0.0f;
				}
				
					
				//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
				//float updown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
				//SuperController.singleton.ClearMessages();
				//SuperController.LogMessage("Fuzz " + Round(Mathf.Lerp(fuzzyLock, 0.0f, Mathf.Clamp(headDelayTimer/5.0f, 0.0f, 1.0f))).ToString() + "| " + Round(Mathf.Lerp(adjustedSpeed, adjustedSpeed / 3.0f, headDelayTimer / 3.0f)).ToString(), false);

				//SuperController.LogError("if ((" + currentLook + " == 'Sex' || 'Intense' || 'Inquisitive' || 'Kissing' || " + headToEyeController + " > " + Mathf.Lerp(lookDirectAngle, lookPeripheralAngle, interestArousal/10.0f) + " || " + headDelayTimer + " > " + Mathf.Lerp(5.0f * uiGazeDirectLookDelay.val, 3.0f * uiGazeDirectLookDelay.val, interestValence/10.0f) + " || " + Mathf.Abs(targetV - actualV) + " > " + (lookDirectAngle/2.0f) + " || " + playerTipToHead + " < " + interactionDistance + ") && (" + headUpDown + " > -27.0f || " + targetV + " > " + actualV + "))");
				if (amGlancing == false) // ((currentLook == "Sex" || currentLook == "Intense" || headToEyeController > Mathf.Lerp(lookPeripheralAngle, lookDirectAngle, interestArousal/10.0f) * fuzzyLock || headDelayTimer > Mathf.Lerp(5.0f * uiGazeDirectLookDelay.val, 3.0f * uiGazeDirectLookDelay.val, interestValence/10.0f) || Mathf.Abs(actualV - (targetV + endAdjustV)) > lookDirectAngle/3.0f || Mathf.Abs(actualH - (targetH + endAdjustH)) > lookDirectAngle/5.0f || playerTipToHead < interactionDistance))// && (headUpDown > -27.0f || targetV > actualV))
				{
					//fuzzyLock = 0.0f;
					//energyAmount += 1.0f;
					//SuperController.LogError("doing head");
					tempFloat2 = Mathf.Clamp(playerHeadToHead - 0.1f, 0.0f, 1.0f);
					tempFloat = Mathf.Lerp(uiGazeMaxSideways.val * 0.9f, uiGazeMaxSideways.val, tempFloat2);
					targetH = Mathf.Clamp(targetH + (endAdjustH), -tempFloat * Mathf.Deg2Rad, tempFloat * Mathf.Deg2Rad);
					tempFloat = Mathf.Lerp(85.00f,69.0f, tempFloat2);
					targetV = Mathf.Clamp(targetV + (endAdjustV), -uiGazeMaxDown.val * Mathf.Deg2Rad, uiGazeMaxUp.val * Mathf.Deg2Rad);
					if (Mathf.Abs(actualH - targetH) * Mathf.Rad2Deg < lookDirectAngle/2.0f)
					{
						headDelayTimer = 0.0f;
					}
					
					if (Mathf.Abs(actualH - targetH) > lookDirectAngle * Mathf.Deg2Rad || Mathf.Abs(actualV - targetV) > (lookDirectAngle / 2.0f) * Mathf.Deg2Rad)
					{
						if (Mathf.Abs(gHeadRoll) > 1.0f)
						{
							gHeadRollTarget = gHeadRollTarget * Mathf.Max(1.0f - (0.15f * uiRollSpeed.val), 0.0f);
						}
					}
					//adjustedSpeed = Mathf.Lerp(adjustedSpeed*10.0f, adjustedSpeed, Mathf.Clamp(Mathf.Abs(velocityH) * 10.0f, 0.0f, 1.0f));
					//adjustedSpeed = Mathf.Lerp(adjustedSpeed, adjustedSpeed / 3.0f, headDelayTimer / 3.0f);
					
					adjustedSpeed = adjustedSpeed / Mathf.Lerp(1.0f, 5.0f, Mathf.Clamp(Mathf.Abs(actualH - targetH), 0.0f, 1.0f));
					fuzzyLock = Mathf.Lerp(fuzzyLock, fuzzyLock / 10.0f, Mathf.Clamp(Mathf.Abs(actualH - targetH), 0.0f, 1.0f));
					if (mainInterest == "PLHand" || mainInterest == "PRHand")
					{
						fuzzyLock = 15.0f;
					}
					tempFloat = 0.001f;
					if (fuzzyLockActual < fuzzyLock)
					{
						fuzzyLockActual = Mathf.Clamp(fuzzyLockActual + tempFloat, fuzzyLockActual, fuzzyLock);
					}
					if (fuzzyLockActual > fuzzyLock)
					{
						fuzzyLockActual = Mathf.Clamp(fuzzyLockActual - tempFloat, fuzzyLock, fuzzyLockActual);
					}


					//SuperController.singleton.ClearMessages();
					//SuperController.LogMessage("adjusted Speed " + Round(adjustedSpeed) + "| Adjust " + Round(endAdjustH * Mathf.Rad2Deg) + "| Diff " + Round(Mathf.Abs(actualH - targetH) * Mathf.Rad2Deg), false);
					//SuperController.LogMessage("Fuzz " + Round((hFuzz * Mathf.Lerp(fuzzyLock, 0.0f, Mathf.Clamp(headDelayTimer/Mathf.Lerp(10.0f * uiGazeDirectLookDelay.val, 5.0f * uiGazeDirectLookDelay.val, interestValence/10.0f), 0.0f, 1.0f)))), false);
					//SuperController.LogMessage("Delay Timer " + Round(headDelayTimer) + "| " + Round(Mathf.Clamp(headDelayTimer/Mathf.Lerp(10.0f * uiGazeDirectLookDelay.val, 5.0f * uiGazeDirectLookDelay.val, interestValence/10.0f), 0.0f, 1.0f)));
					
			//SuperController.LogMessage(endAdjustH.ToString(), false);
					//if (Mathf.Abs(actualH - targetH) > ((hFuzz * uiGazeVariation.val) * (pAgreeableness / 100.0f) * Mathf.Lerp(fuzzyLock, 0.0f, Mathf.Clamp(headDelayTimer, 0.0f, Mathf.Lerp(5.0f, 3.0f, interestValence/10.0f)) / Mathf.Lerp(5.0f, 3.0f, interestValence/10.0f))) * Mathf.Deg2Rad)// || (Mathf.Abs(actualH - targetH) > 65.0f * Mathf.Deg2Rad && Mathf.Abs(actualH) < Mathf.Abs(targetH)))// || eyesSM.CurrentState == eClosed)
					if (Mathf.Abs(actualH - targetH) > (hFuzz * Mathf.Lerp(fuzzyLockActual, 0.0f, Mathf.Clamp(headDelayTimer/Mathf.Lerp(10.0f * uiGazeDirectLookDelay.val, 5.0f * uiGazeDirectLookDelay.val, interestValence/10.0f), 0.0f, 1.0f))) * Mathf.Deg2Rad)// || (Mathf.Abs(actualH - targetH) > 65.0f * Mathf.Deg2Rad && Mathf.Abs(actualH) < Mathf.Abs(targetH)))// || eyesSM.CurrentState == eClosed)
					{
						if (Mathf.Abs(actualH - targetH) > 0.0f)
						{
							actualH = Mathf.SmoothDamp(actualH, targetH, ref velocityH, adjustedSpeed, Mathf.Infinity, Time.fixedDeltaTime);
							//SuperController.LogMessage("Adjusted H");
						}
					}
//					if (Mathf.Abs(actualV - targetV) > ((vFuzz * uiGazeVariation.val) * (pAgreeableness / 100.0f) * Mathf.Lerp(fuzzyLock, 0.0f, Mathf.Clamp(headDelayTimer, 0.0f, Mathf.Lerp(5.0f, 3.0f, interestValence/10.0f)) / Mathf.Lerp(5.0f, 3.0f, interestValence/10.0f))) * Mathf.Deg2Rad)// || (Mathf.Abs(actualV - targetV) > 55.0f * Mathf.Deg2Rad && Mathf.Abs(actualV) < Mathf.Abs(targetV)))// || eyesSM.CurrentState == eClosed)
					if (Mathf.Abs(actualV - targetV) > (vFuzz * Mathf.Lerp(fuzzyLockActual, 0.0f, Mathf.Clamp(headDelayTimer/Mathf.Lerp(10.0f * uiGazeDirectLookDelay.val, 5.0f * uiGazeDirectLookDelay.val, interestValence/10.0f), 0.0f, 1.0f))) * Mathf.Deg2Rad)// || (Mathf.Abs(actualV - targetV) > 55.0f * Mathf.Deg2Rad && Mathf.Abs(actualV) < Mathf.Abs(targetV)))// || eyesSM.CurrentState == eClosed)
					{
						if (Mathf.Abs(actualV - targetV) > 0.0f)
						{
							actualV = Mathf.SmoothDamp(actualV, targetV + (uiHeadAngleOffset.val * Mathf.Deg2Rad), ref velocityV, adjustedSpeed, Mathf.Infinity, Time.fixedDeltaTime);
							//SuperController.LogMessage("Adjusted V");
						}
					}
					//SuperController.singleton.ClearErrors();
					//SuperController.LogError("Fuzz " + (vFuzz * uiGazeVariation.val) * (pExtraversion / 100.0f) * Mathf.Lerp(fuzzyLock, 0.0f, Mathf.Clamp(headDelayTimer, 0.0f, Mathf.Lerp(5.0f * uiGazeDirectLookDelay.val, 3.0f * uiGazeDirectLookDelay.val, interestValence/10.0f)) / Mathf.Lerp(5.0f * uiGazeDirectLookDelay.val, 3.0f * uiGazeDirectLookDelay.val, interestValence/10.0f)));
					//SuperController.LogError("Ho " + actualH + "/" + targetH + "/" + velocityH);
					actualH = Mathf.Clamp(actualH,-uiGazeMaxSideways.val * Mathf.Deg2Rad,uiGazeMaxSideways.val * Mathf.Deg2Rad);
					actualV = Mathf.Clamp(actualV,-uiGazeMaxDown.val * Mathf.Deg2Rad,uiGazeMaxUp.val * Mathf.Deg2Rad);
				}
				
				//SuperController.LogError("Adjusted " + endAdjustH.ToString());
				
				if (Mathf.Abs(actualH * Mathf.Rad2Deg) > uiGazeMaxSideways.val * 0.85f || Mathf.Abs(actualV * Mathf.Rad2Deg) > uiGazeMaxDown.val * 0.85f)
				{
					if (mainInterest == "PLHand")
					{
						interestPLHand -= 0.5f;
					}
					if (mainInterest == "PRHand")
					{
						interestPRHand -= 0.5f;
					}
					if (mainInterest == "LHand")
					{
						interestLHand -= 0.5f;
					}
					if (mainInterest == "RHand")
					{
						interestRHand -= 0.5f;
					}
					if (mainInterest == "Tip")
					{
						interestTip -= 0.5f;
					}
				}

				// recombine
				actualDir = RecombineDirection(actualH, actualV);
				targetDir = RecombineDirection(targetH, targetV);
				actualDir = reference.TransformDirection(actualDir);

				//head.eulerAngles.z = curRotation.z;
				head.transform.LookAt(head.transform.position + actualDir, headController.followWhenOff.position - chestController.followWhenOff.position);
				
				tempFloat = 50.0f / uiRollSpeed.val;//(150.0f / uiRollSpeed.val) / (Mathf.Max(Mathf.Abs(gHeadRollTarget - gHeadRoll) / 30.0f, 1.0f) / 10.0f);
				//tempFloat2 = Mathf.Clamp(gHeadRoll, -uiMaxHeadRoll.val, uiMaxHeadRoll.val);
				if (currentMouth == "Sucking")
				{
					tempFloat = Random.Range(25.0f, 40.0f) / uiRollSpeed.val;
				}
				
				if (interestKissing)
				{
					tempFloat = tempFloat * 3.0f;
				}

				if (gAvoid == 1.0f)
				{
					tempFloat = tempFloat * 2.0f;
					if (gHeadRoll < 0)
					{
						gHeadRoll = Mathf.Min(gHeadRoll + (((0.0f) - gHeadRoll) / tempFloat), uiMaxHeadRoll.val);
					}
					
					if (gHeadRoll > 0)
					{
						gHeadRoll = Mathf.Max(gHeadRoll - ((gHeadRoll - (0.0f)) / tempFloat), -uiMaxHeadRoll.val); // * (interestValence/10.0f)
					}
				}
				else
				{
					tempFloat2 = Round(actualH * Mathf.Rad2Deg) * 0.25f;
					//SuperController.LogMessage(Round(tempFloat2).ToString(), false);
					if (gHeadRoll < gHeadRollTarget - tempFloat2)
					{
						gHeadRoll = Mathf.Min(gHeadRoll + (((gHeadRollTarget - tempFloat2) - gHeadRoll) / tempFloat), uiMaxHeadRoll.val);
					}
					
					if (gHeadRoll > gHeadRollTarget - tempFloat2)
					{
						gHeadRoll = Mathf.Max(gHeadRoll - ((gHeadRoll - (gHeadRollTarget - tempFloat2)) / tempFloat), -uiMaxHeadRoll.val); // * (interestValence/10.0f)
					}
				}
				gHeadRoll = Mathf.Clamp(gHeadRoll, -uiMaxHeadRoll.val, uiMaxHeadRoll.val);
				
				if (Mathf.Abs(gHeadRoll) < 3.0f)
				{
					rollTimer = 0.0f;
				}

				// apply roll
				Vector3 eulerAngles = head.transform.localEulerAngles;
				//testString = (cross.x * Mathf.Rad2Deg) + "/" + (cross.y * Mathf.Rad2Deg) + "/" + (cross.z * Mathf.Rad2Deg);
				eulerAngles.z += (gHeadRoll - ((actualH * Mathf.Rad2Deg) / Mathf.Lerp(30.0f, 5.0f, Mathf.Max(interestArousal - (interestPeakArousal/2.0f), interestValence - (interestPeakValence/2.0f))/10.0f)));
				/*
				if ((cross.y * Mathf.Rad2Deg < -lookPeripheralAngle && gHeadRoll < -10.0f) || (cross.y * Mathf.Rad2Deg > lookPeripheralAngle && gHeadRoll > 10.0f))
				{
					eulerAngles.z += gHeadRoll * ((5.0f - Mathf.Min(rollTimer,5.0f)) / 5.0f);
				}
				else
				{
					eulerAngles.z += gHeadRoll * ((15.0f - Mathf.Min(rollTimer,15.0f)) / 15.0f);
				}*/
				head.transform.localEulerAngles = eulerAngles;


				/*Vector3 playerLipPos = playerHeadTransform.TransformPoint(new Vector3(0.0f,-0.005f,0.105f));
				Vector3 personLipPos = headController.followWhenOff.TransformPoint(new Vector3(0.0f,-0.005f,0.105f));
				float distance = Mathf.Lerp(0.9f, 0.0f, Vector3.Distance(playerLipPos, personLipPos) / kissingDistance);
				if (distance <= 1.0f)
				{
					Vector3 currentPosition = headController.followWhenOff.position;
					Vector3 currentRotation = headController.followWhenOff.eulerAngles;
					headController.transform.position = headController.followWhenOff.TransformPoint(new Vector3(0.0f,-0.005f,0.105f));
					headController.transform.LookAt(personLipPos);
					Vector3 desiredRotation = headController.followWhenOff.eulerAngles;
					headController.transform.position = currentPosition;
					headController.transform.eulerAngles = currentRotation;
					
					headController.transform.eulerAngles = new Vector3(
																	Mathf.Lerp(currentRotation.x, desiredRotation.x, distance),
																	Mathf.Lerp(currentRotation.y, desiredRotation.y, distance),
																	Mathf.Lerp(currentRotation.z, desiredRotation.z, distance)
																   );
				}*/


				//SuperController.LogError("Adjusting neck rotation");

				eulerAngles = head.transform.eulerAngles;
				Vector3 forwardEuler = chestController.transform.eulerAngles;
				Vector3 difference = forwardEuler - eulerAngles;

				Vector3 newDir = Vector3.RotateTowards(neckController.transform.forward, headController.followWhenOff.forward, Mathf.Lerp(1.1f,70.5f,interestValence/10.0f) / 100.0f, 0.0f);
				neckController.transform.rotation = Quaternion.LookRotation(newDir);
				
				neckController.transform.eulerAngles = eulerAngles;
				eulerAngles = neckController.transform.localEulerAngles;
				//eulerAngles.x += sexActionNeckX / 5.0f;
				if (interestKissing)
				{
					if (usePerson2 && person2Usable)
					{
						eulerAngles.z = playerHeadController.followWhenOff.eulerAngles.z;
					}
					else
					{
						eulerAngles.z = player.eulerAngles.z;
					}
				}
				else
				{
				eulerAngles.z -= (gHeadRoll + (actualH * Mathf.Rad2Deg)) * Mathf.Lerp(0.6f,0.2f,interestValence/10.0f) * ((15.0f - Mathf.Min(rollTimer,15.0f)) / 15.0f);
				}
				neckController.transform.localEulerAngles = eulerAngles;
				//SuperController.LogError("neck rotation done");
			}
			else
			{
				peronalityAdjustH = 0.0f;
				peronalityAdjustV = 0.0f;
				gHeadRoll = 0.0f;
			}

//				if ((playerLHandToHead < closeFaceDistance && playerLHandMovement) || (playerRHandToHead < closeFaceDistance && playerRHandMovement))
//					{
//						if (eyeClock > (2.75f * uiBlinkSpeed.val) && morphBlinking == false && currentEye != "Closed" && Random.Range(0.0f,100.0f) > 90.0f)
//						{
						//eyesSM.Switch(eClosed);
//						}
//					}

			//SuperController.LogError("Apply saccade to eye controller");			
			if (uiEyeControl.val)
			{
				eyeController.transform.position = focusPos;
				eyeController.transform.rotation = focusRot;
			}
			
			//SuperController.LogError("Do blink");
			if (eyeUpdateClock >= eyeUpdateTime)
			{
				if (Vector3.Distance(curEyePosition, eyeController.transform.position) > 4.5f)
				{
					if (currentEye != "Blink" && eyeClock > 2.5f * uiBlinkSpeed.val * blinkRepeat)
					{
						//eyesSM.Switch(eBlink);
            //eyeClock = 0.0f;
						//blinkRepeat = blinkRepeat / 2.0f;
						//SuperController.LogError("Blinking  " + Vector3.Distance(curEyePosition, eyeController.transform.position));
					}
				}
			}
			//SuperController.LogError("Limit eye controller movement");
			if (currentInterest == "LHand" || currentInterest == "RHand")
			{
				eyeUpdateTime = 0.5f;
			}
			

			if (uiEyeControl.val)
			{

				if (mEyesClosedLeftValue < 0.5f)
				{
					if (eyeUpdateClock >= eyeUpdateTime || (mainInterest == "Face" && playerHeadMovement && playerHeadToHead > uiCloseToFaceDist.val*2.0f))
					{
						curEyePosition = eyeController.transform.position;
						curEyeAngles = eyeController.transform.eulerAngles;
						eyeUpdateClock = 0.0f;
					}
					else
					{
						eyeUpdateClock += Time.fixedDeltaTime;
						eyeController.transform.position = curEyePosition;
						eyeController.transform.eulerAngles = curEyeAngles;
					}
				eyeController.transform.Translate(Vector3.Scale(saccadeOffset, new Vector3(0.01f,0.01f,0.01f)));// * (Vector3.Distance(headController.followWhenOff.position, eyeController.transform.position) / 100.0f));
				}
			}
			if ((currentMouth == "Idle"  || currentMouth == "Idle(S)") && interestValence > 8.0f && interestArousal < 4.0f && testRun == false && currentLook == "Idle")
			{
				if (Random.Range(0.0f,100.0f) < Mathf.Lerp(0.4f, 3.0f, pExtraversion))
				{
					mouthSM.Switch(mSmile);
				}
			}
			
			
			
			//SuperController.LogError("Applying Custom Weight");

			if (uicustomweights.val)
			{
				uicustomheadweight.val = Mathf.Clamp(uicustomheadweight.val, 0.025f, 100.0f);
				uicustomarmsweight.val = Mathf.Clamp(uicustomarmsweight.val, 0.025f, 100.0f);
				uicustomhandsweight.val = Mathf.Clamp(uicustomhandsweight.val, 0.025f, 100.0f);
				uicustombodyweight.val = Mathf.Clamp(uicustombodyweight.val, 0.025f, 100.0f);
				uicustomlegsweight.val = Mathf.Clamp(uicustomlegsweight.val, 0.025f, 100.0f);
				uicustomfeetweight.val = Mathf.Clamp(uicustomfeetweight.val, 0.025f, 100.0f);
				
				headController.RBMass = 1.25f * uicustomheadweight.val;
				neckController.RBMass = 0.325f * uicustomheadweight.val;

				chestController.RBMass = 3.75f * uicustombodyweight.val;
				pelvisController.RBMass = 1.75f * uicustombodyweight.val;
				pelvis2Controller.RBMass = 1.75f * uicustombodyweight.val;
				abdomenController.RBMass = 3.35f * uicustombodyweight.val;
				lShoulderController.RBMass = 1.25f * uicustombodyweight.val;
				rShoulderController.RBMass = 1.25f * uicustombodyweight.val;
				
				//lArmController.RBMass = 1.05f * uicustomarmsweight.val;
				//rArmController.RBMass = 1.05f * uicustomarmsweight.val;
				//lElbowController.RBMass = 0.325f * uicustomarmsweight.val;
				//rElbowController.RBMass = 0.325f * uicustomarmsweight.val;

				//lHandController.RBMass = 0.015f * uicustomhandsweight.val;
				//rHandController.RBMass = 0.015f * uicustomhandsweight.val;

				lThighController.RBMass = 2.5f * uicustombodyweight.val;
				rThighController.RBMass = 2.5f * uicustombodyweight.val;
				lKneeController.RBMass = 1.25f * uicustomlegsweight.val;
				rKneeController.RBMass = 1.25f * uicustomlegsweight.val;

				lFootController.RBMass = 1.0f * uicustomfeetweight.val;
				rFootController.RBMass = 1.0f * uicustomfeetweight.val;
			}
			
			if (emTarget == null)
			{
				if (uiObjectTarget.val != "None")
				{
					emTarget = SuperController.singleton.GetAtomByUid(uiObjectTarget.val);
					//SuperController.LogError("Reset em Target as null");
				}
				if (emTarget == null)
				{
					uiObjectTarget.val = "None";
				}
			}
			
			/*
			objectUp = null;
			objectUp = SuperController.singleton.GetAtomByUid("EMObjectUp");
			if (objectUp != null)
			{
				objectController = objectUp.GetStorableByID("control") as FreeControllerV3;
				//randomPointUp = objectController.transform.position;
			}

			objectForward = null;
			objectForward = SuperController.singleton.GetAtomByUid("EMObjectForward");
			if (objectForward != null)
			{
				objectController = objectForward.GetStorableByID("control") as FreeControllerV3;
				//randomPointForward = objectController.transform.position;
			}

			objectLeft = null;
			objectLeft = SuperController.singleton.GetAtomByUid("EMObjectLeft");
			if (objectLeft != null)
			{
				objectController = objectLeft.GetStorableByID("control") as FreeControllerV3;
				//randomPointLeft = objectController.transform.position;
			}

			objectRight = null;
			objectRight = SuperController.singleton.GetAtomByUid("EMObjectRight");
			if (objectRight != null)
			{
				objectController = objectRight.GetStorableByID("control") as FreeControllerV3;
				//randomPointRight = objectController.transform.position;
			}*/

			objectUp = null;
			objectUp = SuperController.singleton.GetAtomByUid("EMObjectUp");
			if (objectUp != null)
			{
				useObjectUp = true;
			}
			else
			{
				useObjectUp = false;
			}

			objectForward = null;
			objectForward = SuperController.singleton.GetAtomByUid("EMObjectForward");
			if (objectForward != null)
			{
				useObjectForward = true;
			}
			else
			{
				useObjectForward = false;
			}

			objectLeft = null;
			objectLeft = SuperController.singleton.GetAtomByUid("EMObjectLeft");
			if (objectLeft != null)
			{
				useObjectLeft = true;
			}
			else
			{
				useObjectLeft = false;
			}

			objectRight = null;
			objectRight = SuperController.singleton.GetAtomByUid("EMObjectRight");
			if (objectRight != null)
			{
				useObjectRight = true;
			}
			else
			{
				useObjectRight = false;
			}

			float randdir = 999.0f;
			float objdir = 999.0f;
			float lhanddir = Vector3.Angle(lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f)) - headController.followWhenOff.position, headController.followWhenOff.forward);
			float rhanddir = Vector3.Angle(rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f)) - headController.followWhenOff.position, headController.followWhenOff.forward);
			randdir = Vector3.Angle(randomPointForwardBase - headController.followWhenOff.position, headController.followWhenOff.forward);
			if (useObjectForward)
			{
				objectController = objectForward.GetStorableByID("control") as FreeControllerV3;
				objdir = Vector3.Angle(objectController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
				if (objdir <= randdir + 2.0f && objdir < lookPeripheralAngle)
				{
					randomPointForward = objectController.transform.position;
				}
				else
				{
					randomPointForward = randomPointForwardBase;
				}
			}
			randdir = Vector3.Angle(randomPointLeftBase - headController.followWhenOff.position, headController.followWhenOff.forward);
			if (useObjectLeft)
			{
				objectController = objectLeft.GetStorableByID("control") as FreeControllerV3;
				objdir = Vector3.Angle(objectController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
				if (objdir <= randdir + 2.0f)
				{
					if (objdir < lhanddir && objdir < lookPeripheralAngle)
					{
						randomPointLeft = objectController.transform.position;
					}
					else
					{
						if (lhanddir < randdir + 2.0f && lhanddir < lookPeripheralAngle)
						{
							randomPointLeft = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
						}
						else
						{
							randomPointLeft = randomPointLeftBase;
						}

					}
				}
				else
				{
					if (lhanddir < randdir + 2.0f && lhanddir < lookPeripheralAngle)
					{
						randomPointLeft = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
					}
					else
					{
						randomPointLeft = randomPointLeftBase;
					}
				}
			}
			else
			{
				if (lhanddir < randdir + 2.0f)
				{
					//randomPointLeft = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
				}
				else
				{
					randomPointLeft = randomPointLeftBase;
				}
			}
			randdir = Vector3.Angle(randomPointRightBase - headController.followWhenOff.position, headController.followWhenOff.forward);
			if (useObjectRight)
			{
				objectController = objectRight.GetStorableByID("control") as FreeControllerV3;
				objdir = Vector3.Angle(objectController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
				if (objdir <= randdir + 2.0f && objdir < lookPeripheralAngle)
				{
					if (objdir < rhanddir)
					{
						randomPointRight = objectController.transform.position;
					}
					else
					{
						if (rhanddir < randdir + 2.0f && rhanddir < lookPeripheralAngle)
						{
							randomPointRight = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
						}
						else
						{
							randomPointRight = randomPointRightBase;
						}
					}
				}
				else
				{
					if (rhanddir < randdir + 2.0f && rhanddir < lookPeripheralAngle)
					{
						randomPointRight = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
					}
					else
					{
						randomPointRight = randomPointRightBase;
					}
				}
			}
			else
			{
				if (rhanddir < randdir)
				{
					//randomPointRight = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
				}
				else
				{
					randomPointRight = randomPointRightBase;
				}
			}						
			randdir = Vector3.Angle(randomPointUpBase - headController.followWhenOff.position, headController.followWhenOff.forward);
			if (useObjectUp)
			{
				objectController = objectUp.GetStorableByID("control") as FreeControllerV3;
				objdir = Vector3.Angle(objectController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
				if (objdir < randdir + 2.0f && objdir < lookPeripheralAngle)
				{
					randomPointUp = objectController.transform.position;
				}
				else
				{
					randomPointUp = randomPointUpBase;
				}
			}

			prevPosHead = headController.followWhenOff.position;
			prevPosChest = chestController.followWhenOff.position;
			prevPosHip = abdomenController.followWhenOff.position;
			prevPosLHand = lHandController.followWhenOff.TransformPoint(new Vector3(-0.08f, 0.00f, 0.00f));
			prevPosRHand = rHandController.followWhenOff.TransformPoint(new Vector3(0.08f, 0.00f, 0.00f));
			prevPoslFoot = lFootController.followWhenOff.position;
			prevPosrFoot = rFootController.followWhenOff.position;
			
			interestArousalLast = interestArousal;
			if (currentInterest != lastFrameInterest)
			{
				prevInterest = lastFrameInterest;
			}
			lastFrameInterest = currentInterest;
			//SuperController.LogError("Fixed Update Complete");
			
						
			//Vector3 tempvector = chestController.followWhenOff.forward;
			//Vector3 temp2vector = headController.followWhenOff.forward;
			//float vecfloat = Vector3.Angle(new Vector3(temp2vector.x, tempvector.y, tempvector.z), tempvector);
			
			//float leftright2 = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
			//float updown2 = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
			//testString = "Left/Right : " + headLeftRight + "  Up/Down : " + headUpDown;
			//testString = vecfloat.ToString();
			if (uiShowStats.val)
			{
			//SuperController.LogError("Doing Message Stats");
			SuperController.singleton.ClearMessages();
			//Vector3 tempAngles = playerHeadTransform.eulerAngles;
			if (lookAction)
			{
				//SuperController.LogMessage("Look in action");
			}
			else
			{
				//SuperController.LogMessage("Look idle");
			}
			//SuperController.LogMessage(decisionRepeat.ToString());
			//SuperController.LogMessage(mEyesClosedRightTarget + " / " + mEyesClosedRightValue, false);
			SuperController.LogMessage("Arousal / Happiness : " + Round(interestArousal) + "(" + interestPeakArousal + ")" + "/" + Round(interestValence) + "(" + interestPeakValence+ ")", false);
			testString = "";
			if (amGlancing)
			{
				testString = " (Glancing)";
			}
			if (gAvoid == 1.0f)
			{
				testString = " (Avoiding)";
			}
			SuperController.LogMessage("Current Emotion :" + currentLook + testString, false);
			SuperController.LogMessage("Brow :" + currentBrow + "| Eye :" + currentEye + "| Mouth :" + currentMouth, false);
			SuperController.LogMessage("", false);
			SuperController.LogMessage("Interest Clock : " + Round(interestClock), false);
			SuperController.LogMessage("Interest Target Repeat / Attention Repeat : " + Round(interestRepeat) + "/" + Round(decisionRepeat), false);
			SuperController.LogMessage("", false);
			if (interestKissing)
			{
				SuperController.LogMessage("Current Interest : " + currentInterest + " (Kissing)");
			}
			else
			{
				SuperController.LogMessage("Current Interest : " + currentInterest);
			}
			SuperController.LogMessage("Main Interest : " + mainInterest + "(" + mainOld + ")", false);
			SuperController.LogMessage("Second Interest : " + secondInterest + "(" + secondOld + ")", false);
			SuperController.LogMessage("", false);
			SuperController.LogMessage("Face (" + Round(playerHeadToHead) + "): " + Round(interestFace) + "(" + Round(headActivityBoost) + ")", false);
			SuperController.LogMessage("Inf : " + dbgHead, false);
			SuperController.LogMessage("LHand (" + Round(playerLHandToHead) + "): " + Round(interestLHand) + "(" + Round(lHandActivityBoost) + ")", false);
			SuperController.LogMessage("Inf : " + dbgLHand, false);
			SuperController.LogMessage("RHand (" + Round(playerRHandToHead) + "): " + Round(interestRHand) + "(" + Round(rHandActivityBoost) + ")", false);
			SuperController.LogMessage("Inf : " + dbgRHand, false);
			SuperController.LogMessage("Pelvis (" + Round(playerPelvisToHead) + "): " + Round(interestPelvis), false);
			SuperController.LogMessage("  Tip (" + Round(playerTipToHead) + "): " + Round(interestTip), false);
			SuperController.LogMessage("Inf : " + dbgPenis, false);
			SuperController.LogMessage("Target (" + Round(emTargetDistance) + "): " + Round(interestEMTarget), false);
			SuperController.LogMessage("Inf : " + dbgObject, false);
			SuperController.LogMessage("Self Hand Interest L/R : " + Round(interestPLHand) + "/" + Round(interestPRHand), false);
			SuperController.LogMessage("Inf : " + dbgPLHand + "/ " + dbgPRHand, false);
			SuperController.LogMessage("", false);
			SuperController.LogMessage("Eye Contact  -Buildup:" + Round(gAvoidanceClock) + " -TimeOut:" + Round(gAvoidingClock) + " Dir: " + lookAwaySide + " Height: " + Round(gAvoidHeight), false);
			SuperController.LogMessage("Eye NON Direct Timer: " + Round(eyesNonDirectClock), false);
			SuperController.LogMessage("Eye/Saccade Clock : " + Round(eyeClock) + "/" + Round(saccadeClock), false);
			SuperController.LogMessage("Blink Timer / Repeat :" + Round(blinkTimer) + "/" + Round(blinkRepeat), false);
			SuperController.LogMessage("Saccade : " + debugString, false);
			if (morphBlinking)
			{
				SuperController.LogMessage("Eye State : Blinking", false);
			}
			else
			{
				if (mEyesClosedLeftValue > 0.6f)
				{
					SuperController.LogMessage("Eye State : Closed", false);
				}
				else
				{
					SuperController.LogMessage("Eye State : Open", false);
				}
			}
			if (amGlancing)
			{
				SuperController.LogMessage("Gaze : Glancing", false);
			}
			else
			{
				if (gAvoid == 1.0f)
				{
					SuperController.LogMessage("Gaze : Avoiding " + gAvoidInterest, false);
				}
				else
				{
					if (headToEyeController > eyesNonDirectAngle + (hFuzz * Mathf.Rad2Deg))
					{
						SuperController.LogMessage("Gaze : Indirect", false);
					}
					else
					{
						SuperController.LogMessage("Gaze : Direct", false);
					}
				}
			}
			SuperController.LogMessage("Gaze Variation H/V/Timeout : " + Round(endAdjustH * Mathf.Rad2Deg) + "/" + Round(endAdjustV * Mathf.Rad2Deg) + "/" + Round(adjustTimeout), false);
			SuperController.LogMessage("Gaze Head Roll / Target / Timer : " + Round(gHeadRoll) + " /" + Round(gHeadRollTarget - (-Round(actualH * Mathf.Rad2Deg) * 0.25f)) + " /" + Round(rollTimer), false);
			SuperController.LogMessage("Gaze Avoid : " + Round(gAvoid), false);
			SuperController.LogMessage("Gaze Neck Adjust : " + Round(sexActionNeckX), false);
			SuperController.LogMessage("Gaze Fuzzy Lock / Delay Timer : " + Round(fuzzyLockActual) + "/" + Round(headDelayTimer) + "(" + Round(Mathf.Abs(actualH - targetH) * Mathf.Rad2Deg) + "/" + Round(Mathf.Abs(actualV - targetV) * Mathf.Rad2Deg) + ")", false);
			SuperController.LogMessage("Gaze Direct Look Delay Timer : " + Round(Mathf.Lerp(7.0f * uiGazeDirectLookDelay.val, 3.0f * uiGazeDirectLookDelay.val, interestArousal/10.0f) - eyesNonDirectClock), false);
			SuperController.LogMessage("Gaze Non Direct Timer : " + Round(eyesNonDirectClock), false);
			
			SuperController.LogMessage("Hor Fuzz : " + Round((hFuzz * Mathf.Lerp(fuzzyLockActual, 0.0f, Mathf.Clamp(headDelayTimer/Mathf.Lerp(10.0f * uiGazeDirectLookDelay.val, 5.0f * uiGazeDirectLookDelay.val, interestValence/10.0f), 0.0f, 1.0f)))) + " Vert Fuzz : " + Round((vFuzz * Mathf.Lerp(fuzzyLockActual, 0.0f, Mathf.Clamp(headDelayTimer/Mathf.Lerp(10.0f * uiGazeDirectLookDelay.val, 5.0f * uiGazeDirectLookDelay.val, interestValence/10.0f), 0.0f, 1.0f)))));
			SuperController.LogMessage("Target Vert : " + Round((targetV * Mathf.Rad2Deg) + uiHeadAngleOffset.val) + "| Hor : " + Round(targetH * Mathf.Rad2Deg) + "| Speed : " + Round(adjustedSpeed), false);
			SuperController.LogMessage("Actual Vert : " + Round(actualV * Mathf.Rad2Deg) + "| Hor : " + Round(actualH * Mathf.Rad2Deg) + "| H Speed : " + Round(velocityH) + "| V Speed : " + Round(velocityV), false);
			if (mouthCanOpen)
			{
				SuperController.LogMessage("Mouth Can Open / Timer : True /" + Round(mouthOpenTimer) , false);
			}
			else
			{
				SuperController.LogMessage("Mouth Can Open / Timer : False /" + Round(mouthOpenTimer) , false);
			}
			SuperController.LogMessage("Penis to Pelvis Distance : " + Round(playerTipToPelvis), false);
			SuperController.LogMessage("Person Face->Eye Angle : " + Round(headToEyeController), false);
			SuperController.LogMessage("Person->Player Face Angle : " + Round(headToFaceRot), false);
			SuperController.LogMessage("Player->Person Face Angle : " + Round(playerHeadToFaceRot), false);
			SuperController.LogMessage("", false);
			SuperController.LogMessage("Lips Touch Count : " + lipsTouchCount, false);
			SuperController.LogMessage("V Touch Count : " + vagTouchCount, false);
			SuperController.LogMessage("", false);
			if (personIsMale)
			{
				SuperController.LogMessage("Character is Male", false);
			}
			else
			{
				SuperController.LogMessage("Character is Female", false);
			}
			if (person2IsMale)
			{
				SuperController.LogMessage("Target is Male", false);
			}
			else
			{
				SuperController.LogMessage("Target is Female", false);
			}
			if (playerHandsUsable)
			{
				SuperController.LogMessage("Player Hands Available", false);
			}
			else
			{
				SuperController.LogMessage("Player Hands Unavailable", false);
			}			
			if (uiUseBodyMotion.val)
			{
			SuperController.LogMessage("Idle Body Timeout : " + Round(idleBodyTimeout), false);
			SuperController.LogMessage("Idle Left Arm Timeout : " + Round(idleLArmTimeout), false);
			SuperController.LogMessage("Idle Right Arm Timeout : " + Round(idleRArmTimeout), false);
			SuperController.LogMessage("Idle Left Leg Timeout : " + Round(idleLLegTimeout), false);
			SuperController.LogMessage("Idle Right Leg Timeout : " + Round(idleRLegTimeout), false);
			SuperController.LogMessage("Idle Twist Angle Actual/Target : " + Round(twistActual) + "/" + Round(twistTarget), false);
			SuperController.LogMessage("Idle Twist2 Angle Actual/Target : " + Round(twist2Actual) + "/" + Round(twist2Target), false);
			SuperController.LogMessage("Idle Dynamic Abs Angle : " + Round(abAngleUp), false);
			SuperController.LogMessage("Idle Dynamic Angle Actual/Target : " + Round(dynAdjustActual) + "/" + Round(dynAdjustTarget), false);
			SuperController.LogMessage("Idle LElbow Angle (Hold%) Actual/Target : " + Round(lElbowActual) + "/" + Round(lElbowTarget) + "(" + Round(lElbowHoldActual) + "/" + Round(lElbowHoldTarget) + ")", false);
			SuperController.LogMessage("Idle RElbow Angle (Hold%) Actual/Target : " + Round(rElbowActual) + "/" + Round(rElbowTarget) + "(" + Round(rElbowHoldActual) + "/" + Round(rElbowHoldTarget) + ")", false);
			SuperController.LogMessage("Idle LThigh Angle (Hold%) Actual/Target : " + Round(lThighActual) + "/" + Round(lThighTarget) + "(" + Round(lThighHoldActual) + "/" + Round(lThighHoldTarget) + ")", false);
			SuperController.LogMessage("Idle RThigh Angle (Hold%) Actual/Target : " + Round(rThighActual) + "/" + Round(rThighTarget) + "(" + Round(rThighHoldActual) + "/" + Round(rThighHoldTarget) + ")", false);
			SuperController.LogMessage("Idle LKnee Angle (Hold%) Actual/Target : " + Round(lKneeActual) + "/" + Round(lKneeTarget) + "(" + Round(lKneeHoldActual) + "/" + Round(lKneeHoldTarget) + ")", false);
			SuperController.LogMessage("Idle RKnee Angle (Hold%) Actual/Target : " + Round(rKneeActual) + "/" + Round(rKneeTarget) + "(" + Round(rKneeHoldActual) + "/" + Round(rKneeHoldTarget) + ")", false);
			}
			/*SuperController.LogMessage("Flirt Morph : " + Round(mFlirtingValue), false);
			SuperController.LogMessage("Happy Morph : " + Round(mHappyValue), false);
			SuperController.LogMessage("Excitement Morph : " + Round(mExcitementValue), false);
			SuperController.LogMessage("Glare Morph : " + Round(mGlareValue), false);
			SuperController.LogMessage("Brow Center Up Morph : " + Round(mBrowCenterUpValue), false);
			SuperController.LogMessage("Brow Down Morph : " + Round(mBrowDownValue), false);
			SuperController.LogMessage("Brow Outer Up Left Morph : " + Round(mBrowOuterUpLeftValue), false);
			SuperController.LogMessage("Brow Outer Up Right Morph : " + Round(mBrowOuterUpRightValue), false);
			SuperController.LogMessage("Brow All Up Morph : " + Round(mBrowUpValue), false);
			SuperController.LogMessage("Left Eye Closed Morph : " + Round(mEyesClosedLeftValue), false);
			SuperController.LogMessage("Right Eye Closed Morph : " + Round(mEyesClosedRightValue), false);
			SuperController.LogMessage("Eyes Squint Morph : " + Round(mEyesSquintValue), false);
			SuperController.LogMessage("Nose Flare Morph : " + Round(mNoseFlareValue), false);
			SuperController.LogMessage("Smile Full Face Morph : " + Round(mSmileFullFaceValue), false);
			SuperController.LogMessage("Smile Open Full Face Morph : " + Round(mSmileOpenFullFaceValue), false);
			SuperController.LogMessage("Smile Simple Left Morph : " + Round(mSmileSimpleLeftValue), false);
			SuperController.LogMessage("Smile Simple Right Morph : " + Round(mSmileSimpleRightValue), false);
			SuperController.LogMessage("Smile Muscles Morph : " + Round(mSmileMuscleValue), false);
			SuperController.LogMessage("Mouth Narrow Morph : " + Round(mMouthNarrowValue), false);
			SuperController.LogMessage("Mouth Open Morph : " + Round(mMouthOpenValue), false);
			SuperController.LogMessage("Mouth Open Wide Morph : " + Round(mMouthOpenWideValue), false);
			SuperController.LogMessage("Mouth Open Wider Morph : " + Round(mMouthOpenWiderValue), false);
			SuperController.LogMessage("Mouth Side Left Morph : " + Round(mMouthSideLeftValue), false);
			SuperController.LogMessage("Mouth Side Right Morph : " + Round(mMouthSideRightValue), false);
			SuperController.LogMessage("Visime F : " + Round(mVisFValue), false);
			SuperController.LogMessage("Visime M : " + Round(mVisMValue), false);
			SuperController.LogMessage("Visime AA : " + Round(mVisAAValue), false);
			SuperController.LogMessage("Visime OW : " + Round(mVisOWValue), false);
			SuperController.LogMessage("Lips Pucker Morph : " + Round(mLipsPuckerValue), false);
			SuperController.LogMessage("Lips Pucker Wide Morph : " + Round(mLipsPuckerWideValue), false);
			SuperController.LogMessage("Lips Close Morph : " + Round(mLipsCloseValue), false);
			SuperController.LogMessage("Lips Part Morph : " + Round(mLipsPartValue), false);
			SuperController.LogMessage("Lips Pouty Morph : " + Round(mLipsPoutyValue), false);
			SuperController.LogMessage("Lip Bite Morph : " + Round(mLipBiteValue), false);
			SuperController.LogMessage("Tongue In/Out Morph : " + Round(mTongueInOutValue), false);
			SuperController.LogMessage("Tongue Bend Tip Morph : " + Round(mTongueBendTipValue), false);
			SuperController.LogMessage("Tongue SideSide Morph : " + Round(mTongueSideSideValue), false);
			SuperController.LogMessage("Bottom Lip Down Morph : " + Round(mLipsBottomDownValue), false);
			SuperController.LogMessage("Cheeks Sink Morph : " + Round(mCheekSinkValue), false);
			SuperController.LogMessage("Nipples Apply Morph : " + Round(mNipplesApplyValue), false);
			SuperController.LogMessage("Ten Strip Blowjob Lips Morph : " + Round(mBlowjobLipsValue), false);
			SuperController.LogMessage("Ten Strip Deserving It Morph : " + Round(mDeserveItValue), false);
			SuperController.LogMessage("Ten Strip Taking It Morph : " + Round(mTakingItValue), false);*/
			
			}
		}
        }

        //System States
        private static State sUpdate = new SUpdate();
        private static State sUpdatePerson = new SUpdatePerson();
        private static State sUpdatePlayer = new SUpdatePlayer();
        private static State sUpdatePlayerHands = new SUpdatePlayerHands();
        private static State sUpdatePerson2 = new SUpdatePerson2();
		private static State sReselectPerson2 = new SReselectPerson2();

        //Emotion States
        //private static State emHappy = new EMHappy();
        //private static State emPleased = new EMPleased();
        //private static State emDistracted = new EMDistracted();
        //private static State emWaiting = new EMWaiting();
        //private static State emBored = new EMBored();

        //Reaction States
        //private static State rExcited = new RExcited();
        //private static State rSuprised = new RSuprised();
        //private static State rConcentrate = new RConcentrate();
        //private static State rFrustrated = new RFrustrated();
        //private static State rAnticipate = new RAnticipate();
        //private static State rShy = new RShy();

        //Attention States
        private static State lIntense = new LIntense();
        private static State lInquisitive = new LInquisitive();
        private static State lCasual = new LCasual();
        private static State lBored = new LBored();
        private static State lDayDream = new LDayDream();
        private static State lPlayful = new LPlayful();
        private static State lFeel = new LFeel();
        private static State lKissing = new LKissing();
        private static State lSucking = new LSucking();
        private static State lSex = new LSex();


        //Brow States
        private static State bRaised = new BRaised();
        private static State bLowered = new BLowered();
        private static State bConcentrate = new BConcentrate();
        private static State bOneRaise = new BOneRaise();
        private static State bApprehensive = new BApprehensive();

        //Eye States
        private static State eBlink = new EBlink();
        private static State eOpen = new EOpen();
        private static State eClosed = new EClosed();
        private static State eFocus = new EFocus();
        private static State eSquint = new ESquint();
        private static State eWide = new EWide();
        private static State eWink = new EWink();

        //Mouth States
        private static State mOpen = new MOpen();
        private static State mClosed = new MClosed();
        private static State mBiteLip = new MBiteLip();
        private static State mSmile = new MSmile();
        private static State mBigSmile = new MBigSmile();
        private static State mSmirk = new MSmirk();
        private static State mSideways = new MSideways();
        private static State mKiss = new MKiss();
        private static State mSuck = new MSuck();
        private static State mJoy = new MJoy();
        private static State mOh = new MOh();

        private class SUpdate : State
        {
            public override void OnEnter()
            {
                Duration = 0.05f;
			string tempString = "";
			string tempString2 = "";
			string tempString3 = "";
			if (suppressSmile)
			{
				tempString = "(S)";
			}
			if (mainInterest == currentInterest)
			{
				tempString2 = "(*)";
			}
			if (secondInterest == currentInterest)
			{
				tempString3 = "(*)";
			}
			
			uiString = "Arousal : " + Round(interestArousal) + " " + "Happy : " + Round(interestValence) + "\n" + 
					   "1st : " + mainInterest + tempString2 + " " + "|2nd : " + secondInterest + tempString3 + "\n" + 
					   "Head/LHand/RHand : " + Round(playerHeadToHead) + "/" + Round(playerLHandToHead) + "/" + Round(playerRHandToHead) + "\n" +
					   "Face2Face Angle : " + Round(headToFaceRot) + "\n" +
					   "B:" + currentBrow + " |E:" + currentEye + " |M:" + currentMouth + tempString + "\n";
			
			/*if (morphBlinking && currentEye == "Blink")
			{
				uiString = uiString + "Eye State : Blink   ";
			}
			else
			{
				if (mEyesClosedLeftValue > 0.8f)
				{
					uiString = uiString + "Eye State : Closed ";
				}
				else
				{
					uiString = uiString + "Eye State : Open   ";
				}
			}*/
      
      uiString = uiString + "Look : " + currentLook;
      
			if (amGlancing)
			{
				uiString = uiString + " |Gaze : Glancing";
			}
			else
			{
				if (gAvoid == 1.0f)
				{
					uiString = uiString + " |Gaze : Avoiding " + gAvoidInterest;
				}
				else
				{
					if (headToEyeController > eyesNonDirectAngle)
					{
						uiString = uiString + " |Gaze : Indirect";
					}
					else
					{
						uiString = uiString + "|Gaze : Direct";
					}
				}
			}
			
				
                //Vector3 perp = Vector3.Cross(chestController.followWhenOff.eulerAngles, refAngle);
                //float dir = Vector3.Dot(perp, chestController.followWhenOff.up);
				tempFloat = Mathf.Abs(Vector3.Angle(refAngle, abdomenController.followWhenOff.forward));
                if (Mathf.Abs(tempFloat) > 10.0f || allSetup == false)
                {
                    randomResetDir = true;
                    refAngle = abdomenController.followWhenOff.forward;
                }
				tempFloat = Random.Range(-4.0f * randomBaseHeight,1.0f);
				if (playerHeadToHead < personalSpaceDistance)
				{
					tempFloat = -1.5f * randomBaseHeight;
				}
				else
				{
					if (gAvoid == 1.0f)
					{
						tempFloat = gAvoidHeight * randomBaseHeight;
					}
				}
				
				objectUp = null;
				objectUp = SuperController.singleton.GetAtomByUid("EMObjectUp");
				if (objectUp != null)
				{
					useObjectUp = true;
				}
				else
				{
					useObjectUp = false;
				}

				objectForward = null;
				objectForward = SuperController.singleton.GetAtomByUid("EMObjectForward");
				if (objectForward != null)
				{
					useObjectForward = true;
				}
				else
				{
					useObjectForward = false;
				}

				objectLeft = null;
				objectLeft = SuperController.singleton.GetAtomByUid("EMObjectLeft");
				if (objectLeft != null)
				{
					useObjectLeft = true;
				}
				else
				{
					useObjectLeft = false;
				}

				objectRight = null;
				objectRight = SuperController.singleton.GetAtomByUid("EMObjectRight");
				if (objectRight != null)
				{
					useObjectRight = true;
				}
				else
				{
					useObjectRight = false;
				}
				float randdir = 999.0f;
				float objdir = 999.0f;
				float lhanddir = Vector3.Angle(lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f)) - headController.followWhenOff.position, headController.followWhenOff.forward);
				float rhanddir = Vector3.Angle(rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f)) - headController.followWhenOff.position, headController.followWhenOff.forward);
				tempFloat2 = 0.0f;
				//SuperController.LogError("Random Left Hand " + Round(lhanddir));
				//SuperController.LogError("Random Right Hand " + Round(rhanddir));
				
				if ((Random.Range(0.0f, 100.0f) > tempFloat2 || randomResetDir) && ((mainInterest != "RandomF" && currentInterest != "RandomF") || Mathf.Abs(actualH) * Mathf.Rad2Deg > 45.0f))
				{
					randomPointForward = headController.followWhenOff.position; //Base pos
					randomPointForward += headController.followWhenOff.right * (0.0f * randomBaseOffset); //Left Adjust
					randomPointForward += headController.followWhenOff.up * randomBaseHeight; //Up Adjust
					randomPointForward += headController.followWhenOff.forward * (randomBaseDistance); //Up Adjust
					randomPointForwardBase = randomPointForward;
				}
				randdir = Vector3.Angle(randomPointForwardBase - headController.followWhenOff.position, headController.followWhenOff.forward);
				//SuperController.LogError("Random Point Forward " + Round(randdir));
				if (useObjectForward)
				{
					objectController = objectForward.GetStorableByID("control") as FreeControllerV3;
					objdir = Vector3.Angle(objectController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
					//SuperController.LogError("Random Object Forward " + Round(objdir));
					if (objdir <= randdir + 10.0f)
					{
						randomPointForward = objectController.transform.position;
					}
					//SuperController.LogMessage("Using Forward Object", false);
				}

				if ((Random.Range(0.0f, 100.0f) > tempFloat2 || randomResetDir) && ((mainInterest != "RandomL" && currentInterest != "RandomL") || Mathf.Abs(actualH) * Mathf.Rad2Deg > 45.0f))
				{
					randomPointLeft = headController.followWhenOff.position; //Base pos
					randomPointLeft += headController.followWhenOff.right * -randomBaseOffset; //Left Adjust
					randomPointLeft += headController.followWhenOff.up * randomBaseHeight; //Up Adjust
					randomPointLeft += headController.followWhenOff.forward * randomBaseDistance; //Up Adjust
					randomPointLeftBase = randomPointLeft;
				}
				randdir = Vector3.Angle(randomPointLeftBase - headController.followWhenOff.position, headController.followWhenOff.forward);
				//SuperController.LogError("Random Point Left " + Round(randdir));
				if (useObjectLeft)
				{
					objectController = objectLeft.GetStorableByID("control") as FreeControllerV3;
					objdir = Vector3.Angle(objectController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
					//SuperController.LogError("Random Object Left " + Round(objdir));
					if (objdir <= randdir + 10.0f)
					{
						if (objdir < lhanddir)
						{
							randomPointLeft = objectController.transform.position;
							//SuperController.LogError("Using Left Object", false);
						}
						else
						{
							if (lhanddir < randdir + 10.0f)
							{
								randomPointLeft = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
								//SuperController.LogError("Using Left Hand 1", false);
							}
						}
					}
					else
					{
						if (lhanddir < randdir + 10.0f)
						{
							randomPointLeft = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
							//SuperController.LogError("Using Left Hand 2", false);
						}
					}
					//SuperController.LogMessage("Using Left Object", false);
				}
				else
				{
					if (lhanddir < randdir + 10.0f)
					{
						//randomPointLeft = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
						//SuperController.LogError("Using Left Hand 3", false);
					}
				}
				
				if ((Random.Range(0.0f, 100.0f) > tempFloat2 || randomResetDir) && ((mainInterest != "RandomR" && currentInterest != "RandomR") || Mathf.Abs(actualH) * Mathf.Rad2Deg > 45.0f))
				{
					randomPointRight = headController.followWhenOff.position; //Base pos
					randomPointRight += headController.followWhenOff.right * randomBaseOffset; //Left Adjust
					randomPointRight += headController.followWhenOff.up * randomBaseHeight; //Up Adjust
					randomPointRight += headController.followWhenOff.forward * randomBaseDistance; //Up Adjust
					randomPointRightBase = randomPointRight;
				}
				randdir = Vector3.Angle(randomPointRightBase - headController.followWhenOff.position, headController.followWhenOff.forward);
				//SuperController.LogError("Random Point Right " + Round(randdir));
				if (useObjectRight)
				{
					objectController = objectRight.GetStorableByID("control") as FreeControllerV3;
					objdir = Vector3.Angle(objectController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
					//SuperController.LogError("Random Object Right " + Round(objdir));
					if (objdir <= randdir + 10.0f)
					{
						if (objdir < rhanddir)
						{
							randomPointRight = objectController.transform.position;
							//SuperController.LogError("Using Right Object", false);
						}
						else
						{
							if (rhanddir < randdir + 10.0f)
							{
								randomPointRight = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
								//SuperController.LogError("Using Right Hand 1", false);
							}
						}
					}
					else
					{
						if (rhanddir < randdir + 10.0f)
						{
							//randomPointRight = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
							//SuperController.LogError("Using Right Hand 2", false);
						}
					}
					//SuperController.LogMessage("Using Right Object", false);
				}
				else
				{
					if (rhanddir < randdir)
					{
						randomPointRight = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
						//SuperController.LogError("Using Right Hand 3", false);
					}
				}						

				if ((Random.Range(0.0f, 100.0f) > tempFloat2 || randomResetDir) && ((mainInterest != "RandomU" && currentInterest != "RandomU") || Mathf.Abs(actualH) * Mathf.Rad2Deg > 45.0f))
				{
					randomPointUp = headController.followWhenOff.position; //Base pos
					randomPointUp += headController.followWhenOff.right * (0.0f * randomBaseOffset); //Left Adjust
					randomPointUp += headController.followWhenOff.up * (randomBaseHeight + 0.45f); //Up Adjust
					randomPointUp += headController.followWhenOff.forward * randomBaseDistance; //Up Adjust
					randomPointUpBase = randomPointUp;
				}
				randdir = Vector3.Angle(randomPointUpBase - headController.followWhenOff.position, headController.followWhenOff.forward);
				//SuperController.LogError("Random Point Up " + Round(randdir));
				if (useObjectUp)
				{
					objectController = objectUp.GetStorableByID("control") as FreeControllerV3;
					objdir = Vector3.Angle(objectController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
					//SuperController.LogError("Random Object Up " + Round(objdir));
					if (objdir < randdir + 10.0f)
					{
						randomPointUp = objectController.transform.position;
					}
					//SuperController.LogMessage("Using Up Object", false);
				}
				randomResetDir = false;
				//SuperController.LogError("----Random Dir Done");

            }

            public override void OnTimeout()
            {
                systemSM.Switch(sUpdatePerson);
            }
        }

        private class SUpdatePerson : State
        {
            public override void OnEnter()
            {
                Duration = 0.05f;
                //Person to Player/Person2 update
				
                personHeadTransform = headController.followWhenOff; //headController.transform;


				if (usePerson2 && person2Usable)
				{
					headToFaceRot = Mathf.Abs(Vector3.Angle(playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)) - personHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)), personHeadTransform.forward));
					playerHeadToFaceRot = Mathf.Abs(Vector3.Angle(personHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)) - playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)), playerHeadTransform.forward));
				}
				else
				{
					headToFaceRot = Mathf.Abs(Vector3.Angle(playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.00f, 0.00f)) - personHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)), personHeadTransform.forward));
					playerHeadToFaceRot = Mathf.Abs(Vector3.Angle(personHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)) - playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.00f, 0.00f)), playerHeadTransform.forward));
				}
                headToChestRot = Mathf.Abs(Vector3.Angle(playerChest - personHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)), personHeadTransform.forward));
				if (emTargetName == "[CameraRig]")
				{
					emTargetDir = Mathf.Abs(Vector3.Angle(CameraTarget.centerTarget.transform.position - personHeadTransform.position, personHeadTransform.forward));
					emTargetHeadDir = Mathf.Abs(Vector3.Angle(personHeadTransform.position - CameraTarget.centerTarget.transform.position, CameraTarget.centerTarget.transform.forward));
					emTargetDistance = Vector3.Distance(personHeadTransform.position, CameraTarget.centerTarget.transform.position);
					emTargetPelvisDistance = Vector3.Distance(pelvisController.transform.position, CameraTarget.centerTarget.transform.position);
				}
				else
				{
					if (emTarget != null)
					{
						emTargetDir = Mathf.Abs(Vector3.Angle(emTargetTransform.position - personHeadTransform.position, personHeadTransform.forward));
						emTargetHeadDir = Mathf.Abs(Vector3.Angle(personHeadTransform.position - emTargetTransform.position, emTargetTransform.forward));
						emTargetDistance = Vector3.Distance(personHeadTransform.position, emTargetTransform.position);
						emTargetPelvisDistance = Vector3.Distance(pelvisController.transform.position, emTargetTransform.position);
					}
				}
				
				tempFloat = 5.0f;
                if (Vector3.Distance(playerFace, playerFacePrev) > minHeadMotion || Vector3.Angle(playerFaceRot, playerFaceRotPrev) > 5.0f)
                {
                    playerHeadMovement = true;
                    playerHeadTimeout = Mathf.Min(playerHeadTimeout + movementModifier, movementMaxTimeout);
					if (mainInterest == "Face" || mainOld == "Face" || secondInterest == "Face" || secondOld == "Face")
					{
						headActivityBoost = Mathf.Clamp(headActivityBoost - tempFloat / 5.0f,-10.0f,100.0f - lHandActivityBoost - rHandActivityBoost);
					}
					else
					{
						headActivityBoost = Mathf.Clamp(headActivityBoost + tempFloat * 2.0f,-10.0f,100.0f);
					}
                }
                else
                {
                    playerHeadTimeout = Mathf.Max(playerHeadTimeout - movementFalloff, 0.0f);
                    if (playerHeadTimeout == 0.0f)
                    {
                        playerHeadMovement = false;
                    }
					if ((mainInterest != "Face" && secondInterest != "Face") || playerHeadToFaceRot > playerLookDirectAngle || headToEyeController > lookDirectAngle)
					{
						if (headActivityBoost > 0.0f)
						{
							headActivityBoost = Mathf.Clamp(headActivityBoost - (tempFloat),-10.0f,100.0f);
						}
					}
                }
				if (pLHandTouch || pRHandTouch)
				{
					if ((mainInterest == "PLHand" || mainInterest == "PRHand"))
					{
						headActivityBoost = Mathf.Clamp(headActivityBoost + 0.5f,-10.0f,100.0f);
					}
					else
					{
						headActivityBoost = Mathf.Clamp(headActivityBoost - Mathf.Lerp(0.05f, 0.6f, interestArousal/10.0f),-10.0f,100.0f);
					}
					
				}
				else
				{
					if (headActivityBoost < 0.0f)
					{
						headActivityBoost = Mathf.Clamp(headActivityBoost + 0.7f,-10.0f,0.0f);
					}
				}
				
                if (playerHandsUsable || person2Usable)
                {
                    headToLHandRot = Mathf.Abs(Vector3.Angle(playerLHand - personHeadTransform.position, personHeadTransform.forward));
                    headToRHandRot = Mathf.Abs(Vector3.Angle(playerRHand - personHeadTransform.position, personHeadTransform.forward));
                }
                if (person2Usable)
                {
                    headToPelvisRot = Mathf.Abs(Vector3.Angle(playerPelvis - personHeadTransform.position, personHeadTransform.forward));
                    headToTipRot = Mathf.Abs(Vector3.Angle(playerTip - personHeadTransform.position, personHeadTransform.forward));
                }
			
				//SuperController.LogError("----Update Person Done");
            }
            public override void OnTimeout()
            {
                systemSM.Switch(sUpdatePerson2);
            }
        }
		
        private class SUpdatePerson2 : State
        {
            public override void OnEnter()
            {
                Duration = 0.05f;
                if (usePerson2 && person2Usable)
                {
                    playerFacePrev = playerFace;
                    playerFace = playerHeadTransform.position;
                    playerFaceRotPrev = playerFaceRot;
                    playerFaceRot = playerHeadTransform.eulerAngles;
                    playerChest = playerChestController.followWhenOff.position;
                    playerLHandPrev = playerLHand;
                    playerLHand = playerLHandController.followWhenOff.position;
                    playerRHandPrev = playerRHand;
                    playerRHand = playerRHandController.followWhenOff.position;
                    playerPelvis = playerPelvisController.followWhenOff.position;
                    //playerTipPrev = playerTip;
                    playerTip = playerTipController.followWhenOff.position;
                    playerTipBase = playerTipBaseController.followWhenOff.position;
                }
                else
                {
					player = CameraTarget.centerTarget.transform;
                    playerFacePrev = playerFace;
                    playerFace = player.TransformPoint(new Vector3(-0.3f, 0.0f, 0.0f));
                    playerFaceRotPrev = playerFaceRot;
                    playerFaceRot = player.eulerAngles;
                    playerChest = new Vector3(playerFace.x, playerFace.y - 1.0f, playerFace.z); //REDO THIS
                    playerLHandPrev = playerLHand;
                    playerRHandPrev = playerRHand;
                    if (person2Usable && playerHandsUsable == false)
                    {
                        playerLHand = playerLHandController.followWhenOff.position;
                        playerRHand = playerRHandController.followWhenOff.position;
                    }
                    if (playerHandsUsable && (usePerson2 == false || person2Usable == false))
                    {
                        playerLHand = playerVRLHand.position;
                        playerRHand = playerVRRHand.position;
                    }
                    if (person2Usable == false && playerHandsUsable == false)
                    {
                        playerLHand = playerFace;
                        playerRHand = playerFace;
                    }
                    if (person2Usable && usePerson2)
                    {
                        playerPelvis = playerPelvisController.followWhenOff.position;
                        //playerTipPrev = playerTip;
                        playerTip = playerTipController.followWhenOff.position;
                        playerTipBase = playerTipBaseController.followWhenOff.position;
                    }
					else
					{
                        playerPelvis = playerFace;
                        //playerTipPrev = playerTip;
                        playerTip = playerFace;
                        playerTipBase = playerFace;
					}
                }
                playerGround = new Vector3(playerFace.x, 0.0f, playerFace.z);
				//SuperController.LogError("----Update Person 2 Done");
            }
            public override void OnTimeout()
            {
                systemSM.Switch(sUpdatePlayer);
				allSetup = true;
            }
        }
		
        private class SUpdatePlayer : State
        {
            public override void OnEnter()
            {
                Duration = 0.05f;
                //Player/Person2 to Person update
                if (usePerson2 && person2 != null)
                {
                    playerHeadTransform = playerHeadController.followWhenOff;
					person2Usable = true;
                }
                else
                {
                    playerHeadTransform = CameraTarget.centerTarget.transform;
					person2Usable = false;
                }

                personChestToHead = Mathf.Abs(Vector3.Angle(playerFace - chestController.followWhenOff.position, chestController.followWhenOff.forward));

                playerToHead = Mathf.Abs(Vector3.Angle(headController.followWhenOff.position - playerFace, playerHeadTransform.forward));
                playerToPelvis = Mathf.Abs(Vector3.Angle(pelvisController.followWhenOff.position - playerFace, playerHeadTransform.forward));
                playerToLHand = Mathf.Abs(Vector3.Angle(lHandController.followWhenOff.position - playerFace, playerHeadTransform.forward));
                playerToRHand = Mathf.Abs(Vector3.Angle(rHandController.followWhenOff.position - playerFace, playerHeadTransform.forward));
                playerToLFoot = Mathf.Abs(Vector3.Angle(lFootController.followWhenOff.position - playerFace, playerHeadTransform.forward));
                playerToRFoot = Mathf.Abs(Vector3.Angle(rFootController.followWhenOff.position - playerFace, playerHeadTransform.forward));
				
				playerHeadToHead = Vector3.Distance(headController.followWhenOff.position, playerHeadTransform.position);
                playerHeadToLHand = Vector3.Distance(lHandController.followWhenOff.position, playerFace);
                playerHeadToRHand = Vector3.Distance(rHandController.followWhenOff.position, playerFace);
				if (lBreastController != null)
				{
					playerHeadToLBreast = Vector3.Distance(lBreastController.followWhenOff.position, playerFace);
					playerHeadToRBreast = Vector3.Distance(rBreastController.followWhenOff.position, playerFace);
					playerToLBreast = Mathf.Abs(Vector3.Angle(lBreastController.followWhenOff.position - playerFace, playerHeadTransform.forward));
					playerToRBreast = Mathf.Abs(Vector3.Angle(rBreastController.followWhenOff.position - playerFace, playerHeadTransform.forward));
				}
				else
				{
					playerHeadToLBreast = 999.0f;
					playerHeadToRBreast = 999.0f;
					playerToLBreast = 180.0f;
					playerToRBreast = 180.0f;
				}
                playerHeadToPelvis = Vector3.Distance(pelvisController.followWhenOff.position, playerFace);
                if (person2Usable && person2 != null)
                {
                    playerPelvisToHead = Vector3.Distance(headController.followWhenOff.position, playerPelvis);
					//playerTipToHeadLast = playerTipToHead;
					if (lBreastController != null)
					{
						playerTipToHead = Vector3.Distance(headController.followWhenOff.position, playerTip);
						playerTipToLHand = Vector3.Distance(lHandController.followWhenOff.position, playerTip);
						playerTipToRHand = Vector3.Distance(rHandController.followWhenOff.position, playerTip);
						if (lBreastController != null)
						{
							playerTipToLBreast = Vector3.Distance(lBreastController.followWhenOff.position, playerTip);
							playerTipToRBreast = Vector3.Distance(rBreastController.followWhenOff.position, playerTip);
						}
						else
						{
							playerTipToLBreast = 999.0f;
							playerTipToRBreast = 999.0f;
						}
						playerTipToPelvis = Vector3.Distance(pelvisController.followWhenOff.position, playerTip);
						if (Vector3.Distance(playerTip, playerTipPrev) > minTipMotion / 2.0f)
						{
							playerTipMovement = true;
							playerTipTimeout = Mathf.Min(playerTipTimeout + movementModifier, movementMaxTimeout);
						}
						else
						{
							playerTipTimeout = Mathf.Max(playerTipTimeout - movementFalloff, 0.0f);
							if (playerTipTimeout == 0.0f)
							{
								playerTipMovement = false;
							}
						}
					}
					else
					{
						playerTipToHead = 999.0f;
						playerTipToLHand = 999.0f;
						playerTipToRHand = 999.0f;
						playerTipToLBreast = 999.0f;
						playerTipToRBreast = 999.0f;
						playerTipToPelvis = 999.0f;
						playerTipMovement = false;
						playerTipTimeout = 0.0f;
					}
                }
				else
				{
                    playerPelvisToHead = 999.0f;
                    playerTipToHead = 999.0f;
                    playerTipToLHand = 999.0f;
                    playerTipToRHand = 999.0f;
                    playerTipToLBreast = 999.0f;
                    playerTipToRBreast = 999.0f;
                    playerTipToPelvis = 999.0f;
					playerTipMovement = false;
					playerTipTimeout = 0.0f;
				}
				//SuperController.LogError("----Update Player");
            }
            public override void OnTimeout()
            {
                systemSM.Switch(sUpdatePlayerHands);
            }
        }

        private class SUpdatePlayerHands : State
        {
            public override void OnEnter()
            {
                Duration = 0.05f;
                //Player/Person2 hands to Person update

                if (playerHandsUsable || (person2Usable && usePerson2))
                {
                    if (playerHandsUsable)
                    {
                        playerLHandTransform = playerVRLHand;
                        playerRHandTransform = playerVRRHand;
                    }
                    if (usePerson2 || (playerHandsUsable == false && person2Usable))
                    {
                        playerLHandTransform = playerLHandController.followWhenOff;
                        playerRHandTransform = playerRHandController.followWhenOff;
                    }
                    if (person2Usable == false && playerHandsUsable == false)
                    {
                        playerLHandTransform = playerHeadTransform;
                        playerRHandTransform = playerHeadTransform;
                    }
                    personChestToLHand = Mathf.Abs(Vector3.Angle(playerLHandTransform.position - chestController.followWhenOff.position, chestController.followWhenOff.forward));
                    personChestToRHand = Mathf.Abs(Vector3.Angle(playerRHandTransform.position - chestController.followWhenOff.position, chestController.followWhenOff.forward));

                    playerToPLHand = Mathf.Abs(Vector3.Angle(playerLHandTransform.position - playerHeadTransform.position, playerHeadTransform.forward));
                    playerToPRHand = Mathf.Abs(Vector3.Angle(playerRHandTransform.position - playerHeadTransform.position, playerHeadTransform.forward));
                    playerLHandToHead = Vector3.Distance(headController.followWhenOff.position, playerLHandTransform.TransformPoint(new Vector3(-0.1f,0.0f,0.0f)));
                    playerRHandToHead = Vector3.Distance(headController.followWhenOff.position, playerRHandTransform.TransformPoint(new Vector3(0.1f,0.0f,0.0f)));
                    playerLHandToLHand = Vector3.Distance(lHandController.followWhenOff.position, playerLHandTransform.TransformPoint(new Vector3(-0.1f,0.0f,0.0f)));
                    playerRHandToLHand = Vector3.Distance(lHandController.followWhenOff.position, playerRHandTransform.TransformPoint(new Vector3(0.1f,0.0f,0.0f)));
                    playerLHandToRHand = Vector3.Distance(rHandController.followWhenOff.position, playerLHandTransform.TransformPoint(new Vector3(-0.1f,0.0f,0.0f)));
                    playerRHandToRHand = Vector3.Distance(rHandController.followWhenOff.position, playerRHandTransform.TransformPoint(new Vector3(0.1f,0.0f,0.0f)));
					if (lBreastController != null)
					{
						playerLHandToLBreast = Vector3.Distance(lBreastController.followWhenOff.position, playerLHandTransform.TransformPoint(new Vector3(-0.1f,0.0f,0.0f)));
						playerRHandToLBreast = Vector3.Distance(lBreastController.followWhenOff.position, playerRHandTransform.TransformPoint(new Vector3(0.1f,0.0f,0.0f)));
						playerLHandToRBreast = Vector3.Distance(rBreastController.followWhenOff.position, playerLHandTransform.TransformPoint(new Vector3(-0.1f,0.0f,0.0f)));
						playerRHandToRBreast = Vector3.Distance(rBreastController.followWhenOff.position, playerRHandTransform.TransformPoint(new Vector3(0.1f,0.0f,0.0f)));
					}
					else
					{
						playerLHandToLBreast = 999.0f;
						playerRHandToLBreast = 999.0f;
						playerLHandToRBreast = 999.0f;
						playerRHandToRBreast = 999.0f;
					}
                    playerLHandToPelvis = Vector3.Distance(pelvisController.followWhenOff.position, playerLHandTransform.TransformPoint(new Vector3(-0.1f,0.0f,0.0f)));
                    playerRHandToPelvis = Vector3.Distance(pelvisController.followWhenOff.position, playerRHandTransform.TransformPoint(new Vector3(0.1f,0.0f,0.0f)));
                    if (playerHandsUsable == false)
                    {
						playerToPLHand = 180.0f;
						playerToPRHand = 180.0f;
						playerLHandToHead = 999.0f;
						playerRHandToHead = 999.0f;
						playerLHandToLHand = 999.0f;
						playerRHandToLHand = 999.0f;
						playerLHandToRHand = 999.0f;
						playerRHandToRHand = 999.0f;
						playerLHandToLBreast = 999.0f;
						playerRHandToLBreast = 999.0f;
						playerLHandToRBreast = 999.0f;
						playerRHandToRBreast = 999.0f;
						playerLHandToPelvis = 999.0f;
						playerRHandToPelvis = 999.0f;
                    }
                }
				//SuperController.LogError("----Update Player Hands");
            }
            public override void OnTimeout()
            {
                systemSM.Switch(sUpdate);
            }
        }
		
		
        private class SReselectPerson2 : State
        {
            public override void OnEnter()
            {
				//SuperController.LogError("doing reselect");
				Duration = 0.1f;
				person2Usable = false;
				
				if (person2 != null)
				{
					//SuperController.LogError("person is not null");
					//JSONStorable js = person2.GetStorableByID("geometry");
					//DAZCharacterSelector dcs = js as DAZCharacterSelector;
					//GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;
					//if (morphUI != null)
					//{
						//DAZMorph morphTemp = morphUI.GetMorphByDisplayName("Breast Height");
						person2IsMale = false;
						//SuperController.LogError(person2.gameObject.name);
						if (person2.GetComponentInChildren<DAZCharacterSelector>().gender != DAZCharacterSelector.Gender.Female)//morphTemp == null)
						{
							person2IsMale = true;
						}
					//}
					if (person2IsMale)
					{
					//SuperController.LogError("male");
					}
					else
					{
					//SuperController.LogError("female");
					}
					person2Usable = true;
					playerHeadController = person2.GetStorableByID("headControl") as FreeControllerV3;
					playerChestController = person2.GetStorableByID("chestControl") as FreeControllerV3;
					playerLHandController = person2.GetStorableByID("lHandControl") as FreeControllerV3;
					playerRHandController = person2.GetStorableByID("rHandControl") as FreeControllerV3;
					playerPelvisController = person2.GetStorableByID("pelvisControl") as FreeControllerV3;
					playerTipController = person2.GetStorableByID("penisTipControl") as FreeControllerV3;
					playerTipBaseController = person2.GetStorableByID("penisBaseControl") as FreeControllerV3;
					usePerson2 = true;
					//uiUsePerson2.val = true;
					//SuperController.LogError("New Person " + currentAtomName + " Found");
				}
				else
				{
					//SuperController.LogError("New Person " + currentAtomName + " Not Found");
					person2Usable = false;
					usePerson2 = false;
				}
				
				if (person2.GetStorableByID("geometry") == null)
				{
					person2Usable = false;
					usePerson2 = false;
				}
				
				if (usePerson2 && person2Usable)
				{
					playerFace = playerHeadController.followWhenOff.position;
					playerLHand = playerLHandController.followWhenOff.position;
					playerRHand = playerRHandController.followWhenOff.position;
					playerPelvis = playerPelvisController.followWhenOff.position;
					playerTip = playerTipController.followWhenOff.position;
				}
				else
				{
					playerFace = player.position;
					if (person2Usable && usePerson2)
					{
						playerLHand = playerLHandController.followWhenOff.position;
						playerRHand = playerRHandController.followWhenOff.position;
						playerLHandTransform = playerLHandController.followWhenOff;
						playerRHandTransform = playerRHandController.followWhenOff;
					}
					if (playerHandsUsable && usePerson2 == false)
					{
						playerLHand = playerVRLHand.position;
						playerRHand = playerVRRHand.position;
						playerLHandTransform = playerVRLHand;
						playerRHandTransform = playerVRRHand;
					}
					if (person2Usable == false && playerHandsUsable == false)
					{
						playerLHand = playerFace;
						playerRHand = playerFace;
						playerLHandTransform = playerHeadTransform;
						playerRHandTransform = playerHeadTransform;
					}
					playerPelvis = playerFace;
					playerTip = playerFace;
					if (person2Usable)
					{
						playerPelvis = playerPelvisController.followWhenOff.position;
						playerTip = playerTipController.followWhenOff.position;
					}
					else
					{
						//uiUsePerson2.val = false;
					}
				}
				
				if (usePerson2 && person2 != null)
				{
					playerHeadTransform = playerHeadController.followWhenOff;
					closeFaceDistance = closeFaceDistance * 1.85f;
				}
				else
				{
					playerHeadTransform = player;
				}
				
				//SuperController.LogError("----Reselect Player 2");
			}
            public override void OnTimeout()
            {
                systemSM.Switch(sUpdate);
            }		
		}
		
        private Vector3 RecombineDirection(float angleH, float angleV)
        {
            float cosV = Mathf.Cos(angleV);
            return new Vector3(
                Mathf.Sin(angleH) * cosV,
                Mathf.Sin(angleV),
                Mathf.Cos(angleH) * cosV
            );
        }
		
		private void loadStateConfig()
		{
			SimpleJSON.JSONNode loadedSettings = new SimpleJSON.JSONClass();
			string tempPath = GetPluginPath();
			loadedSettings=SuperController.singleton.LoadJSON(tempPath + "\\Config\\FaceStateControl.json");
			enableIntense = loadedSettings["Look : Intense"].AsBool;
			enableInquisitive = loadedSettings["Look : Inquisitive"].AsBool;
			enableCasual = loadedSettings["Look : Casual"].AsBool;
			enableBored = loadedSettings["Look : Bored"].AsBool;
			enableDayDream = loadedSettings["Look : DayDream"].AsBool;
			enablePlayful = loadedSettings["Look : Playful"].AsBool;
			enableFeel = loadedSettings["Look : Feel"].AsBool;
			enableKissing = loadedSettings["Look : Kissing"].AsBool;
			enableSucking = loadedSettings["Look : Sucking"].AsBool;
			enableSex = loadedSettings["Look : Sex"].AsBool;
			enableRaised = loadedSettings["EyeBrow : Raised"].AsBool;
			enableLowered = loadedSettings["EyeBrow : Lowered"].AsBool;
			enableConcentrate = loadedSettings["EyeBrow : Concentrate"].AsBool;
			enableOneRaise = loadedSettings["EyeBrow : One Raise"].AsBool;
			enableApprehensive = loadedSettings["EyeBrow : Apprehensive"].AsBool;
			enableBlink = loadedSettings["Eye : Blink"].AsBool;
			enableEyeOpen = loadedSettings["Eye : Open"].AsBool;
			enableEyeClosed = loadedSettings["Eye : Closed"].AsBool;
			enableFocus = loadedSettings["Eye : Focus"].AsBool;
			enableSquint = loadedSettings["Eye : Squint"].AsBool;
			enableWide = loadedSettings["Eye : Wide"].AsBool;
			enableWink = loadedSettings["Eye : Wink"].AsBool;
			enableMouthOpen = loadedSettings["Mouth : Open"].AsBool;
			enableMouthClosed = loadedSettings["Mouth : Closed"].AsBool;
			enableBiteLip = loadedSettings["Mouth : Bite Lip / Demure"].AsBool;
			enableSmile = loadedSettings["Mouth : Smile"].AsBool;
			enableBigSmile = loadedSettings["Mouth : Big Smile"].AsBool;
			enableSmirk = loadedSettings["Mouth : Smirk / Pout"].AsBool;
			enableSideways = loadedSettings["Mouth : Side Ways"].AsBool;
			enableKiss = loadedSettings["Mouth : Kiss"].AsBool;
			enableSuck = loadedSettings["Mouth : Suck"].AsBool;
			enableJoy = loadedSettings["Mouth : Joy"].AsBool;
			enableOh = loadedSettings["Mouth : Oh"].AsBool;
		}

	
		private void loadDefaults()
		{
			//SuperController.LogError("Load Defaults Start");
			SimpleJSON.JSONNode loadedSettings = new SimpleJSON.JSONClass();
			string tempPath = this.GetPackagePath();
			//SuperController.LogMessage("Path is " + tempPath, false);
			loadedSettings=SuperController.singleton.LoadJSON(tempPath + lastPath + "/E-Motion_Defaults.json");
			if (loadedSettings != null)
			{
				uiExtraversion.val = loadedSettings["Extraversion"].AsFloat;
				uiAgreeableness.val = loadedSettings["Agreeableness"].AsFloat;
				uiStableness.val = loadedSettings["Stableness"].AsFloat;
				uiBreatheSpeed.val = loadedSettings["Breathing Speed"].AsFloat;
				uiBreatheExpandMultiplier.val = loadedSettings["Breathe Morph Multiplier"].AsFloat;
				uiBreatheRaiseMultiplier.val = loadedSettings["Breath Raise Mult"].AsFloat;
				uiChestHeightOffset.val = loadedSettings["Breath Upper Chest Offset"].AsFloat;
				uiGazeVariation.val = loadedSettings["Gaze Angle Variation"].AsFloat;
				uiGazeSpeed.val = loadedSettings["Gaze Speed"].AsFloat;
				uiGazeAvoid.val = loadedSettings["Gaze Avoidance Enable"].AsBool;
				uiGazeLookTime.val = loadedSettings["Gaze Look At Time"].AsFloat;
				uiGazeAvoidTime.val = loadedSettings["Gaze Avoid Look Time"].AsFloat;
				uiGazeGlance.val = loadedSettings["Gaze Glance Enable"].AsBool;
				uiRollSpeed.val = loadedSettings["Gaze Head Roll Speed"].AsFloat;
				uiMaxHeadRoll.val = loadedSettings["Gaze Max Tilt"].AsFloat;
				uiRollChance.val = loadedSettings["Gaze Tilt Chance"].AsFloat;
				uiGlanceTimeout.val = loadedSettings["Glance Timeout Mult"].AsFloat;
				uiSaccadeSpeed.val = loadedSettings["Eye Saccade Frequency"].AsFloat;
				uiSaccadeAmount.val = loadedSettings["Eye Saccade Movement Scale"].AsFloat;
				uiSaccadeWanderMult.val = loadedSettings["Eye Saccade Max Dist from Target Scale"].AsFloat;
				uiBlinkSpeed.val = loadedSettings["Eye Blink Delay Scale"].AsFloat;
				uiPupilDialation.val = loadedSettings["Eye Contact Pupil Dialation"].AsFloat;
				uiPupilRate.val = loadedSettings["Pupil Dialation Speed Mult"].AsFloat;
				uiArousalSpeed.val = loadedSettings["Mood Arousal Scale"].AsFloat;
				uiValenceSpeed.val = loadedSettings["Mood Valence Scale"].AsFloat;
				uiMoodSpeed.val = loadedSettings["Mood Change Scale"].AsFloat;
				uiInterestSpeed.val = loadedSettings["Main Interest Switch Delay Scale"].AsFloat;
				uiInterestRate.val = loadedSettings["Global Interest Rate Scale"].AsFloat;
				uiDoHead.val = loadedSettings["Control Head and Neck Movements"].AsBool;
				uiDoMorphs.val = loadedSettings["Control Breath and Expression Morphs"].AsBool;
				uiAnimationSpeed.val = loadedSettings["Animation Speed Mult"].AsFloat;
				uiDoShoulders.val = loadedSettings["Control Shoulder Movements"].AsBool;
				uiDoChest.val = loadedSettings["Control Chest Movement"].AsBool;
				uiDoSounds.val = loadedSettings["Play Emotion Sounds"].AsBool;
				uiSoundVolume.val = loadedSettings["Sound Volume Scale"].AsFloat;
				uiChestAmount.val = loadedSettings["Chest Movement Scale"].AsFloat;
				uiDoHands.val = loadedSettings["Control Hand Morphs"].AsBool;
				uiConfigHead.val = loadedSettings["Automatically Configure Head and Neck physics"].AsBool;
				uiUsePerson2.val = loadedSettings["Look At Target Person"].AsBool;
				uiShowStats.val = loadedSettings["Show Debug Info on Message Log"].AsBool;
				uiDoKiss.val = loadedSettings["Kissing Enable"].AsBool;
				uiKissAmount.val = loadedSettings["Kissing Morph Scale"].AsFloat;
				uiDoBlowjob.val = loadedSettings["Blowjob Enable"].AsBool;
				uiBlowjobAmount.val = loadedSettings["Blowjob Morph Scale"].AsFloat;
				uiDoSex.val = loadedSettings["Sex Enable"].AsBool;
				uiSexAmount.val = loadedSettings["Sex Morph Scale"].AsFloat;
				uiFocusTarget.val = loadedSettings["Current Focus Target"];
				uiObjectTarget.val = loadedSettings["Current Object Target"];
				uiTargetLook.val = loadedSettings["Object View Direction Enable"].AsBool;
				uiPersonalSpace.val = loadedSettings["Personal Space Distance"].AsFloat;
				uiDirectGaze.val = loadedSettings["Direct Viewing Angle"].AsFloat;
				uiPeripheralGaze.val = loadedSettings["Peripheral Viewing Angle"].AsFloat;
				uiOutOfGaze.val = loadedSettings["Maximum Viewing Angle"].AsFloat;
				uiCloseToFaceDist.val = loadedSettings["Face Interaction Distance"].AsFloat;
				uiKissingDist.val = loadedSettings["Kissing Activation Distance"].AsFloat;
				uiInteractDist.val = loadedSettings["General Interaction Distance"].AsFloat;
				uiMaxMorphSmile.val = loadedSettings["Maximum Allowed Value For Smile Morphs"].AsFloat;
				uiEyeCloseMaxMorph.val = loadedSettings["Maximum Amount To Close Eyes"].AsFloat;
				uiEyeOpenMaxMorph.val = loadedSettings["Maximum Amount To Open Eyes"].AsFloat;
				uiEyeUpdate.val = loadedSettings["Minimum Time Between Eye Target Movements Excl Saccades"].AsFloat;
				uiHeadInterest.val = loadedSettings["Target Head Interest Rate Scale"].AsFloat;
				uiLHandInterest.val = loadedSettings["Target Left Hand Interest Rate Scale"].AsFloat;
				uiRHandInterest.val = loadedSettings["Target Right Hand Interest Rate Scale"].AsFloat;
				uiSelfLHandInterest.val = loadedSettings["Self Left Hand Interest Rate Scale"].AsFloat;
				uiSelfRHandInterest.val = loadedSettings["Self Right Hand Interest Rate Scale"].AsFloat;
				uiPenisInterest.val = loadedSettings["Target Pelvis Interest Rate Scale"].AsFloat;
				uiObjectInterest.val = loadedSettings["Target Object Interest Rate Scale"].AsFloat;
				uiIdleAmount.val = loadedSettings["Idle Movement Mult"].AsFloat;
				uiIdleLegAmount.val = loadedSettings["Idle Leg Movement Mult"].AsFloat;
				uiIdleArmAmount.val = loadedSettings["Idle Arm Movement Mult"].AsFloat;
				uiIdleChance.val = loadedSettings["Idle Movement Chance"].AsFloat;
				uiIdleArmChance.val = loadedSettings["Idle Arm Movement Chance"].AsFloat;
				uiIdleSpeed.val = loadedSettings["Idle Movement Speed"].AsFloat;
				uiShoulderAmount.val = loadedSettings["Shoulder Adjust Mult"].AsFloat;
				uiShoulderHeight.val = loadedSettings["Shoulder Height Mult"].AsFloat;
				uiShoulderBack.val = loadedSettings["Shoulders Back"].AsFloat;
				uiExpressionChance.val = loadedSettings["Expression Chance"].AsFloat;
				uiGazeMaxUp.val = loadedSettings["Tracking Max Up Angle"].AsFloat;
				uiGazeMaxDown.val = loadedSettings["Tracking Max Down Angle"].AsFloat;
				uiGazeMaxSideways.val = loadedSettings["Tracking Max Side/Side Angle"].AsFloat;
				uiRandomBaseDistance.val = loadedSettings["Random Base Distance"].AsFloat;
				uiRandomBaseHeight.val = loadedSettings["Random Base Height"].AsFloat;
				uiRandomBaseOffset.val = loadedSettings["Random Base Center Offset"].AsFloat;
				uiGazeDirectLookDelay.val = loadedSettings["Gaze Direct Delay Mult"].AsFloat;
				uiIndirectDecay.val = loadedSettings["Gaze Indirect Decay"].AsFloat;
				uiMaterialMult.val = loadedSettings["Arousal Gloss Mult"].AsFloat;
				uiEffectMaterial.val = loadedSettings["Arousal Effects Gloss"].AsBool;
				uiObjectAsPrimary.val = loadedSettings["Treat Object As Primary"].AsBool;
				uiEyeControl.val = loadedSettings["Control Eye Target"].AsBool; 
				uiBreastLift.val = loadedSettings["Breast Lift"].AsFloat;
				uiExpressionLength.val = loadedSettings["Expression Length"].AsFloat;
				uiIdleHoldPow.val = loadedSettings["Idle Hold Power"].AsFloat;
				uiIdleLegHold.val = loadedSettings["Idle Leg Hold"].AsFloat;
				uiIdleArmOffset.val  = loadedSettings["Idle Arm Pos Offset"].AsFloat;
				uiIdleArmRotOffset.val  = loadedSettings["Idle Arm Rot Offset"].AsFloat;
				uicustomweights.val = loadedSettings["Use Custom Weight"].AsBool; 
				uicustomheadweight.val = loadedSettings["Custom Head Weight"].AsFloat;
				uicustombodyweight.val = loadedSettings["Custom Body Weight"].AsFloat;
				uicustomarmsweight.val = loadedSettings["Custom Arms Weight"].AsFloat;
				uicustomhandsweight.val = loadedSettings["Custom Hands Weight"].AsFloat;
				uicustomlegsweight.val = loadedSettings["Custom Legs Weight"].AsFloat;
				uicustomfeetweight.val = loadedSettings["Custom Feet Weight"].AsFloat;
				uiSmileOffset.val  = loadedSettings["Smile Morphs Offset"].AsFloat;
				uiMovementInterest.val = loadedSettings["Movement Interest Rate"].AsFloat;
				uiMovementInterest.val = loadedSettings["Movement Interest Rate Arousal"].AsFloat;
				uiShowHelp.val = loadedSettings["Show Help Text"].AsBool; 
				uiHeadRotStrength.val = loadedSettings["Head Rotation Strength"].AsFloat;
				uiHeadRotDamper.val = loadedSettings["Head Rotation Damper"].AsFloat;
				uiDynamicDirectAngle.val = loadedSettings["Dynamic Direct Angle"].AsBool;
				uiSmileDamper.val = loadedSettings["Smile Damper"].AsFloat;
        uiEyeMoveBlinkDist.val = loadedSettings["Eye Move Blink"].AsFloat;
        uiControlTongue.val = loadedSettings["Control Tongue"].AsBool;
        uiControlJaw.val = loadedSettings["Control Jaw"].AsBool;
				uiOnlyBuiltIn.val = loadedSettings["Built In Morphs"].AsBool;
				uiSmileSuppression.val = loadedSettings["Smile Suppression Delay"].AsFloat;
				uiVariationChance.val = loadedSettings["Gaze Variation Chance"].AsFloat;
				uiMoanChance.val = loadedSettings["Moan Chance"].AsFloat;
				uiUseBodyMotion.val = loadedSettings["Use Body Motion Features"].AsBool;

				uiSetupComplete.val = true;
				if (uiFocusTarget.val != "None" && uiUsePerson2.val)
				{
					person2 = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
					if (person2 != null)
					{
					systemSM.Switch(sReselectPerson2);
					}
					else
					{
						uiFocusTarget.val = "None";
						uiUsePerson2.val = false;
						person2Usable = false;
						usePerson2 = false;
					}
				}
				//uiLoadDefaults.val = false;
				//SuperController.LogError("E-Motion Defaults Loaded successfully!");
			}
		}
	
		public static float Round(float value)
		{
			return Mathf.Round(value * 100.0f) / 100.0f;
		}

		public static float Limit(float value)
		{
			return Mathf.Clamp(value, 0.0f, 1.0f);
		}
		
        string GetPluginPath()
        {
			//SuperController.LogError("Getting Plugin Path");
			//SuperController.singleton.currentSaveDir = SuperController.singleton.currentLoadDir;
			//SuperController.LogError("Plugin Path A");
            string pluginId = this.storeId.Split('_')[0];
			//SuperController.LogError("Plugin Path B");
            MVRPluginManager manager = containingAtom.GetStorableByID("PluginManager") as MVRPluginManager;
			//SuperController.LogError("Plugin Path C");
            string pathToScriptFile = manager.GetJSON(true, true)["plugins"][pluginId].Value;
			//SuperController.LogError("Plugin Path D");
            string pathToScriptFolder = pathToScriptFile.Substring(0, pathToScriptFile.LastIndexOfAny(new char[] { '/', '\\' }));
			//SuperController.LogError("Getting Plugin Path End");
            return pathToScriptFolder;
        }
		
		public void LoadSounds()
		{
			string pluginPath = GetPluginPath();
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In4.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In5.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In6.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In_Fast1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In_Fast2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In_Fast3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_In_Fast4.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Out1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Out2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Out3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Out4.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Out5.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Nose_In_Long1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Nose_In_Long2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Nose_In_Med1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Nose_In_Med2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Nose_In_Med3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Nose_Out_Long1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Nose_Out_Long2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Nose_Out_Long3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Nose_Out_Long4.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah4.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah5.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah6.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah7.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah8.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah9.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah10.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah11.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah12.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah13.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah14.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah15.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Aah16.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh4.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh5.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh6.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh7.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh8.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh9.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh10.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh11.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh12.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh13.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh14.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Ooh15.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm4.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm5.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm6.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm7.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm8.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm9.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm10.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm11.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm12.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm13.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm14.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm15.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Mmm16.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Yeah1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Yeah2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Yeah3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Yeah4.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Yeah5.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Yeah6.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Yeah7.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Breath_Mouth_Yeah8.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss2.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss3.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss4.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss5.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss6.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss7.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss8.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss9.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss10.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss_Mmm1.wav");
			URLAudioClipManager.singleton.QueueClip(pluginPath + "/Sounds/Kiss_Mmm2.wav");
			soundsLoaded = true;
		}

		public void RegisterUIElements()
		{
			uiAgreeableness = new JSONStorableFloat("Personality Agreeableness", 50.0f, 1.0f, 99.0f, true, true);
			RegisterFloat(uiAgreeableness);

			uiExtraversion = new JSONStorableFloat("Personality Extraversion", 50.0f, 1.0f, 99.0f, true, true);
			RegisterFloat(uiExtraversion);

			uiStableness = new JSONStorableFloat("Personality Stableness", 50.0f, 1.0f, 99.0f, true, true);
			RegisterFloat(uiStableness);

			uiShowStats = new JSONStorableBool("Show Stats on Message Log", false);
			RegisterBool(uiShowStats);
				
			uiConfigHead = new JSONStorableBool("Auto Config Head", false);
			RegisterBool(uiConfigHead);

			uiDoHead = new JSONStorableBool("Control Head", false);
			RegisterBool(uiDoHead);

			uiDoMorphs = new JSONStorableBool("Control Morphs", false);
			RegisterBool(uiDoMorphs);

			uiUsePerson2 = new JSONStorableBool("Look at selected Person", true);
			RegisterBool(uiUsePerson2);
			
						
			List<string> targetChoices = new List<string>();
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
				currentAtom = SuperController.singleton.GetAtomByUid(atomUID);
                if (currentAtom != containingAtom && atomUID != null && currentAtom.type == "Person")
                {
					targetChoices.Add(atomUID);
				}
			}
			uiFocusTarget = new JSONStorableStringChooser("Target Selector", targetChoices, "None", "Choose Person");
			RegisterStringChooser(uiFocusTarget);

			List<string> objectChoices = new List<string>();
			objectChoices.Add("None");
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
				currentAtom = SuperController.singleton.GetAtomByUid(atomUID);
                if (atomUID != null)
                {
						objectChoices.Add(atomUID);
				}
			}
			uiObjectTarget = new JSONStorableStringChooser("Object Selector", objectChoices, "None", "Choose Object");
			RegisterStringChooser(uiObjectTarget);

			uiTargetLook = new JSONStorableBool("Object Can Look", false);
			RegisterBool(uiTargetLook);

			uiDoSounds = new JSONStorableBool("Play Sounds", false);
			RegisterBool(uiDoSounds);

			uiSoundVolume = new JSONStorableFloat("Sound Volume Mult", 1.50f, 0.0f, 100.0f, true, true);
			RegisterFloat(uiSoundVolume);

			uiEffectMaterial = new JSONStorableBool("Arousal Effects Gloss", false);
			RegisterBool(uiEffectMaterial);

			uiMaterialMult = new JSONStorableFloat("Arousal Gloss Mult", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiMaterialMult);

			uiDoHands = new JSONStorableBool("Adjust Hands", false);
			RegisterBool(uiDoHands);

			uiDoKiss = new JSONStorableBool("Auto Kissing", false);
			RegisterBool(uiDoKiss);
			
			uiKissAmount = new JSONStorableFloat("Kissing Effect Mult", 1.0f, 0.0f, 1.5f, true, true);
			RegisterFloat(uiKissAmount);

			uiDoBlowjob = new JSONStorableBool("Auto Blowjob", false);
			RegisterBool(uiDoBlowjob);
			
			uiBlowjobAmount = new JSONStorableFloat("Blowjob Effect Mult", 0.7f, 0.0f, 1.5f, true, true);
			RegisterFloat(uiBlowjobAmount);

			uiDoSex = new JSONStorableBool("Auto Sex", false);
			RegisterBool(uiDoSex);

			uiSexAmount = new JSONStorableFloat("Sex Effect Mult", 0.75f, 0.0f, 1.5f, true, true);
			RegisterFloat(uiSexAmount);
			
			uiGazeAvoid = new JSONStorableBool("Gaze Avoidance", false);
			RegisterBool(uiGazeAvoid);

			uiGazeLookTime = new JSONStorableFloat("Avoidance Eye Contact Time", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiGazeLookTime);

			uiGazeAvoidTime = new JSONStorableFloat("Avoidance Look Away Time", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiGazeAvoidTime);

			uiGazeGlance = new JSONStorableBool("Gaze Glancing", false);
			RegisterBool(uiGazeGlance);

			uiGlanceTimeout = new JSONStorableFloat("Glance Timeout Mult", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiGlanceTimeout);

			uiGazeDirectLookDelay = new JSONStorableFloat("Gaze Direct Delay Mult", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiGazeDirectLookDelay);

			uiIndirectDecay = new JSONStorableFloat("Gaze Indirect Decay", 0.7f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIndirectDecay);

			uiGazeSpeed = new JSONStorableFloat("Gaze Speed Mult", 2.0f, 0.0f, 20.0f, true, true);
			RegisterFloat(uiGazeSpeed);

			uiGazeVariation = new JSONStorableFloat("Gaze Variation Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiGazeVariation);

			uiRollChance = new JSONStorableFloat("Gaze Tilt Chance", 1.0f, 0.0f, 2.0f, true, true);
			RegisterFloat(uiRollChance);

			uiMaxHeadRoll = new JSONStorableFloat("Gaze Max Tilt", 60.0f, 0.0f, 90.0f, true, true);
			RegisterFloat(uiMaxHeadRoll);

			uiRollSpeed = new JSONStorableFloat("Gaze Tilt Speed Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiRollSpeed);

			uiBreatheSpeed = new JSONStorableFloat("Breath Speed Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiBreatheSpeed);

			uiChestHeightOffset = new JSONStorableFloat("Breath Upper Chest Offset", 0.0f, -1.0f, 1.0f, true, true);
			RegisterFloat(uiChestHeightOffset);

			uiBreatheRaiseMultiplier = new JSONStorableFloat("Breath Raise Mult", 0.7f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiBreatheRaiseMultiplier);

			uiBreatheExpandMultiplier = new JSONStorableFloat("Breath Expansion Mult", 0.7f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiBreatheExpandMultiplier);

			uiDoShoulders = new JSONStorableBool("Adjust Shoulders", false);
			RegisterBool(uiDoShoulders);

			uiShoulderBack = new JSONStorableFloat("Shoulders Back", 0.00f, 0.0f, 1.0f, true, true);
			RegisterFloat(uiShoulderBack);

			uiShoulderAmount = new JSONStorableFloat("Shoulder Adjust Mult", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiShoulderAmount);

			uiShoulderHeight = new JSONStorableFloat("Shoulder Height Mult", 1.00f, 0.0f, 2.0f, true, true);
			RegisterFloat(uiShoulderHeight);


			uiDoChest = new JSONStorableBool("Adjust Chest", false);
			RegisterBool(uiDoChest);

			uiChestAmount = new JSONStorableFloat("Chest Adjust Mult", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiChestAmount);

			uiDoHands = new JSONStorableBool("Adjust Hands", false);
			RegisterBool(uiDoHands);

			uiIdleAmount = new JSONStorableFloat("Idle Body Movement Mult", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIdleAmount);

			uiIdleLegAmount = new JSONStorableFloat("Idle Leg Movement Mult", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIdleLegAmount);

			uiIdleArmAmount = new JSONStorableFloat("Idle Arm Movement Mult", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIdleArmAmount);
			
			uiIdleArmOffset = new JSONStorableFloat("Idle Arm Pos Offset", 0.00f, -180.0f, 180.0f, true, true);
			RegisterFloat(uiIdleArmOffset);
			
			uiIdleArmRotOffset = new JSONStorableFloat("Idle Arm Rot Offset", 0.00f, -180.0f, 180.0f, true, true);
			RegisterFloat(uiIdleArmRotOffset);

			uiIdleHoldPow = new JSONStorableFloat("Idle Move Hold Power", 0.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIdleHoldPow);
			
			uiIdleLegHold = new JSONStorableFloat("Idle Leg Move Hold Power", 0.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIdleLegHold);
			
			
			uiIdleArmHold = new JSONStorableFloat("Idle Arm Move Hold Power", 0.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIdleArmHold);

			uiIdleChance = new JSONStorableFloat("Idle Movement Chance", 60.00f, 0.0f, 100.0f, true, true);
			RegisterFloat(uiIdleChance);

			uiIdleArmChance = new JSONStorableFloat("Idle Arm Movement Chance", 60.00f, 0.0f, 100.0f, true, true);
			RegisterFloat(uiIdleArmChance);

			uiIdleSpeed = new JSONStorableFloat("Idle Movement Speed", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIdleSpeed);

			uiIdleArmSpeed = new JSONStorableFloat("Idle Arm Movement Speed", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIdleArmSpeed);

			uiIdleBodyDelay = new JSONStorableFloat("Idle Body Move Delay", 1.00f, 0.0f, 15.0f, true, true);
			RegisterFloat(uiIdleBodyDelay);

			uiIdleArmDelay = new JSONStorableFloat("Idle Arm Move Delay", 1.00f, 0.0f, 15.0f, true, true);
			RegisterFloat(uiIdleArmDelay);

			uiIdleLegDelay = new JSONStorableFloat("Idle Leg Move Delay", 1.00f, 0.0f, 15.0f, true, true);
			RegisterFloat(uiIdleLegDelay);

			uiPersonalSpace = new JSONStorableFloat("Personal Space", 1.0f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiPersonalSpace);

			uiInteractDist = new JSONStorableFloat("Interaction Distance", 0.15f, 0.0f, 0.5f, true, true);
			RegisterFloat(uiInteractDist);

			uiCloseToFaceDist = new JSONStorableFloat("Face Interact Dist", 0.15f, 0.0f, 0.5f, true, true);
			RegisterFloat(uiCloseToFaceDist);

			uiKissingDist = new JSONStorableFloat("Kissing Dist", 0.29f, 0.0f, 0.5f, true, true);
			RegisterFloat(uiKissingDist);
			
			uiDirectGaze = new JSONStorableFloat("Direct View Angle", 12.0f, 0.0f, 180.0f, true, true);
			RegisterFloat(uiDirectGaze);

			uiPeripheralGaze = new JSONStorableFloat("Peripheral View Angle", 45.0f, 0.0f, 180.0f, true, true);
			RegisterFloat(uiPeripheralGaze);

			uiOutOfGaze = new JSONStorableFloat("Out of View Angle", 90.0f, 0.0f, 180.0f, true, true);
			RegisterFloat(uiOutOfGaze);

			uiInterestSpeed = new JSONStorableFloat("Main Change Delay Mult", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiInterestSpeed);

			uiInterestRate = new JSONStorableFloat("Interest Rate Mult", 1.0f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiInterestRate);

			uiArousalSpeed = new JSONStorableFloat("Arousal Speed Mult", 1.0f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiArousalSpeed);

			uiValenceSpeed = new JSONStorableFloat("Valence Speed Mult", 1.0f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiValenceSpeed);

			uiMoodSpeed = new JSONStorableFloat("Mood Degrade Speed Mult", 1.0f, 0.0f, 50.0f, true, true);
			RegisterFloat(uiMoodSpeed);
			
			uiExpressionChance = new JSONStorableFloat("Expression Change Chance", 20.0f, 0.0f, 100.0f, true, true);
			RegisterFloat(uiExpressionChance);

			uiGazeMaxUp = new JSONStorableFloat("Tracking Max Up Angle", 49.0f, 0.0f, 90.0f, true, true);
			RegisterFloat(uiGazeMaxUp);

			uiGazeMaxDown = new JSONStorableFloat("Tracking Max Down Angle", 85.0f, 0.0f, 90.0f, true, true);
			RegisterFloat(uiGazeMaxDown);

			uiGazeMaxSideways = new JSONStorableFloat("Tracking Max Side/Side Angle", 114.0f, 0.0f, 180.0f, true, true);
			RegisterFloat(uiGazeMaxSideways);

			uiBlinkSpeed = new JSONStorableFloat("Blink Delay Mult", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiBlinkSpeed);

			uiSaccadeSpeed = new JSONStorableFloat("Saccade Rate Mult", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiSaccadeSpeed);
			
			uiSaccadeAmount = new JSONStorableFloat("Saccade Amount", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiSaccadeAmount);

			uiSaccadeWanderMult = new JSONStorableFloat("Saccade Max Dist Mult", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiSaccadeWanderMult);

			uiPupilDialation = new JSONStorableFloat("Eye Contact Pupil Dialation", 1.0f, 0.0f, 2.0f, true, true);
			RegisterFloat(uiPupilDialation);

			uiPupilRate = new JSONStorableFloat("Pupil Dialation Speed Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiPupilRate);

			uiEyeUpdate = new JSONStorableFloat("Eye Update Speed Mult", 0.3f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiEyeUpdate);

			uiAnimationSpeed = new JSONStorableFloat("Animation Speed Mult", 1.00f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiAnimationSpeed);

			uiMouthOpenOffset = new JSONStorableFloat("Morph Mouth Open Offset", 0.0f, -1.0f, 1.0f, true, true);
			RegisterFloat(uiMouthOpenOffset);

			uiLipsCloseOffset = new JSONStorableFloat("Morph Lips Closed Offset", 0.0f, -1.0f, 1.0f, true, true);
			RegisterFloat(uiLipsCloseOffset);

			uiMaxMorphSmile = new JSONStorableFloat("Max Smile (Morphs)", 0.5f, 0.0f, 1.0f, true, true);
			RegisterFloat(uiMaxMorphSmile);
			
			uiEyeOpenMaxMorph = new JSONStorableFloat("Max Eye Open (Morphs)", -0.1f, -0.5f, 0.5f, true, true);
			RegisterFloat(uiEyeOpenMaxMorph);

			uiEyeCloseMaxMorph = new JSONStorableFloat("Max Eye Close (Morphs)", 1.1f, 0.0f, 1.5f, true, true);
			RegisterFloat(uiEyeCloseMaxMorph);

			uiHeadAngleOffset = new JSONStorableFloat("Head Base Angle Offset", 0.0f, -45.0f, 45.0f, true, true);
			RegisterFloat(uiHeadAngleOffset);

			uiObjectInterest = new JSONStorableFloat("Object Interest Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiObjectInterest);

			uiHeadInterest = new JSONStorableFloat("Head Interest Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiHeadInterest);

			uiLHandInterest = new JSONStorableFloat("LHand Interest Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiLHandInterest);

			uiRHandInterest = new JSONStorableFloat("RHand Interest Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiRHandInterest);

			uiSelfLHandInterest = new JSONStorableFloat("LHand Self Interest Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiSelfLHandInterest);

			uiSelfRHandInterest = new JSONStorableFloat("RHand Self Interest Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiSelfRHandInterest);

			uiPenisInterest = new JSONStorableFloat("Penis Interest Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiPenisInterest);

			uiRandomBaseDistance = new JSONStorableFloat("Random Base Distance", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiRandomBaseDistance);

			uiRandomBaseHeight = new JSONStorableFloat("Random Base Height", -0.5f, -2.0f, 2.0f, true, true);
			RegisterFloat(uiRandomBaseHeight);

			uiRandomBaseOffset = new JSONStorableFloat("Random Base Center Offset", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiRandomBaseOffset);

			uiTongueLength = new JSONStorableFloat("Tongue Length", 0.04f, -1.0f, 1.0f, true, true);
			RegisterFloat(uiTongueLength);

			uiTongueRaise = new JSONStorableFloat("Tongue Raise", 0.24f, -1.0f, 1.0f, true, true);
			RegisterFloat(uiTongueRaise);

			triggerArousal = new JSONStorableFloat("Arousal", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(triggerArousal);
			triggerValence = new JSONStorableFloat("Valence", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(triggerValence);

			uiSetupComplete = new JSONStorableBool("Plugin has been Setup", false);
			RegisterBool(uiSetupComplete);
			
			uiObjectAsPrimary = new JSONStorableBool("Treat Object as Primary", false);
			RegisterBool(uiObjectAsPrimary);

			uiEyeControl = new JSONStorableBool("Control Eye Target", true);
			RegisterBool(uiEyeControl);
						
			uiBreastLift = new JSONStorableFloat("Arm Breast Lift", 1.00f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiBreastLift);

			uicustomweights = new JSONStorableBool("Use Custom Physics Weight", false);
			RegisterBool(uicustomweights);

			uicustombodyweight = new JSONStorableFloat("Custom Body Weight Mult", 1.0f, 0.01f, 50.0f, true, true);
			RegisterFloat(uicustombodyweight);
			
			uicustomhandsweight = new JSONStorableFloat("Custom Hands Weight Mult", 1.0f, 0.01f, 50.0f, true, true);
			RegisterFloat(uicustombodyweight);

			uicustomheadweight = new JSONStorableFloat("Custom Head Weight Mult", 1.0f, 0.01f, 50.0f, true, true);
			RegisterFloat(uicustombodyweight);

			uicustomfeetweight = new JSONStorableFloat("Custom Feet Weight Mult", 1.0f, 0.01f, 50.0f, true, true);
			RegisterFloat(uicustombodyweight);

			uicustomarmsweight = new JSONStorableFloat("Custom Arms Weight Mult", 1.0f, 0.01f, 50.0f, true, true);
			RegisterFloat(uicustomarmsweight);

			uicustomlegsweight = new JSONStorableFloat("Custom Legs Weight Mult", 1.0f, 0.01f, 50.0f, true, true);
			RegisterFloat(uicustomlegsweight);

			uiSmileOffset = new JSONStorableFloat("Smile Morphs Offset", 0.0f, -1.0f, 1.0f, true, true);
			RegisterFloat(uiSmileOffset);
			
			uiExpressionLength = new JSONStorableFloat("Expression Length Mult", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiExpressionLength);
			
			uiMovementInterest = new JSONStorableFloat("Movement Happiness Rate", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiMovementInterest);

			uiMovementInterestArousal = new JSONStorableFloat("Movement Arousal Rate", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiMovementInterestArousal);
			
			uiHeadRotStrength = new JSONStorableFloat("Head Rotation Strength", 15.00f, 5.0f, 200.0f, true, true);
			RegisterFloat(uiHeadRotStrength);

			uiHeadRotDamper = new JSONStorableFloat("Head Rotation Damper", 5.00f, 0.0f, 100.0f, true, true);
			RegisterFloat(uiHeadRotDamper);
			
			uiDynamicDirectAngle = new JSONStorableBool("Dynamically Calculate Direct Angle", false);
			RegisterBool(uiDynamicDirectAngle);

			uiSmileDamper = new JSONStorableFloat("Smile Damper", 1.00f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiSmileDamper);

			uiMinInterest = new JSONStorableFloat("Minimum Interest", 30.00f, 0.0f, 100.0f, true, true);
			RegisterFloat(uiMinInterest);

			uiUseBodyMotion = new JSONStorableBool("Use Body Motion Features", false);
			RegisterBool(uiUseBodyMotion);

			uiEyeMoveBlinkDist = new JSONStorableFloat("Eye Movement Blink threshold", 0.6f, 0.0f, 2.0f, true, true);
			RegisterFloat(uiEyeMoveBlinkDist);

            uiArousalStatus = new JSONStorableString("Arousal", "");
            uiValenceStatus = new JSONStorableString("Happiness", "");

   		uiShowHelp = new JSONStorableBool("Show Help Text", true);
			RegisterBool(uiShowHelp);

   		uiControlTongue = new JSONStorableBool("Control Tongue Morphs", true);
			RegisterBool(uiControlTongue);

   		uiControlJaw = new JSONStorableBool("Control Jaw Morphs", true);
			RegisterBool(uiControlJaw);

			uiOnlyBuiltIn = new JSONStorableBool("Only Use Built-in Morphs", false);
			RegisterBool(uiOnlyBuiltIn);

			uiSmileSuppression = new JSONStorableFloat("Smile Suppression Delay", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiSmileSuppression);

			uiVariationChance = new JSONStorableFloat("Gaze Variation Chance", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiVariationChance);

			uiMoanChance = new JSONStorableFloat("Moan Chance", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiMoanChance);


			uiOverviewLeftHelp = new JSONStorableString("Help", "Welcome to E-Motion!\n\nE-Motion is a control system\nthat takes care of all the\nidle and reactive motion you\ncould want in a person to bring\nthem to life!\n\nEach of the tabs above allow\nyou to configure emotion to\nyour liking and adjust how\nevery aspect of the plugin works\nHowever it is recommonded to start\nwith one of the included presets,\njust click Load Preset!\n\nAt the top you will find an\noverview of the current state\nof emotion, showing the current\narousal and happiness, distances\nand angles as well as what the\nsystem finds interesting and \nthe expression state of the face.\nUse this to help tweak the \npersonality\n\nFor significantly more in depth\nanalysis you can tick the show\nstats on message log to see all\nthe values and calculations that\nE-Motion is using to determine\nwhat to look at and how. Note\nthat enabling this will cause\nquite a performance hit\n\nLoad Defaults will reset E-Motion\nto the default values. You can set\nwhat the defaults values are for\nthe plugin by saving a preset with\nthe name 'Emotion Defaults'. Just\nOverride the existing preset.\nNext time you add E-Motion to a\nperson these settings will be\nloaded for you automatically\n\nLoad Preset allows you to load\na previously saved preset to quickly\nsetup E-Motion\n\nSave Preset allows you to save\nthe current setup into a preset\nto allow you to quickly setup\nE-Motion in a new scene with your\nfavorite settings");
			uiOverviewRightHelp = new JSONStorableString("Help", "If you open any tab by itself\nadditional help information will\nbe displayed to help you understand\nwhat all the options are for. Help\ncan be disabled under features\n\n\nE-Motion uses randomly\ngenerated points to give\nthe person something to\nlook at when they are bored,\nor when the target is not visible\n\nThese points can be overriden with\nactual objects in the scene by\nrenaming objects one of the\nfollowing names :\nEMObjectLeft\nEMObjectRight\nEMObjectForward\nEMObjectUp\n\nRandom points are still generated\nand used by the system if the object\nyou have overriden the point with\nis not currently in view.\n\nThis can be used to give the person\nthe ability to look at something in\nthe scene instead of just staring\ninto space to ground them better\nin your scene!");
			uiFeatureControlHelp = new JSONStorableString("Help", "Feature Control allows you to\nenable and disable features of\nE-Motion\n\nHelp shows these Texts\n\nAuto Config Head configures Head\nand Neck nodes automatically to\nwork with E-Motion. Sliders allow\nAdjustment of strength\n\nEye Target lets E-Motion control\nwhere the character looks. Disable\nif you are using Glance to control\neyes\n\nControl Head allows E-Motion to\ncontrol where the character turns\ntheir head\n\nControl Morphs allows E-Motion to\nshow expressions and Breathing\n\nControl Tongue/Jaw Morphs can be\ndisabled to support VAMMOAN. Only\nwhen E-Motion sound is not active\n\nUse Only Built In Morphs will force\nEMotion to use VAMs morphs and no\ncustom morphs. You should not need\nto enable this\nBody Motion enable to allow use\nof options on Body Movement tab\n\nEnabling Sounds will generate\nBreathing, Kissing and moaning\nsound effects\n\nMoan Chance adjusts how likely the\nvoice will moan when aroused\nor touched\n\n\nAuto Kissing enables detection\nand animation of kissing when\nyou lean in to kiss the person\n\nAuto Blowjob animates the lips\nWhen target person's penis enters\nthe mouth\n\nAuto Sex animates the head and face\nexpression when penetration occurs");
			uiLookAdjustmentHelp = new JSONStorableString("Help", "These options allow you to\nadjust how the expressions are\napplied to your model and to\nadjust settings to fit your \nlook.\n\nThe first 4 options effect\nmouth expression and should\nbe tweaked to get the best look\n\nSmile Morph Offset is important\nfor getting smiles to look right\nadjust this before reducing max\nsmile\n\nSmile Damper pulls the mouth \ncorners down when smiling\n\nMax Eye Open/Closed control the\namount eyes can open and close\nwhen animated\n\nHead Base Angle adjusts the tilt\nof the head when looking at a \ntarget and can help give your \nperson different overall emotion\n\nTongue settings mostly effect\nkissing so that the tongue is in\nthe right position for your look\n\nCustom Physics Weight adjusts the\nactual weight of each body part\nin the physics simulation of VAM\nand can be used to make your person\nlighter or heavier for more realistic\nragdoll simulation");
			uiEyeControlHelp = new JSONStorableString("Help", "These settings control the eye\nmovement of E-Motion and allow\nyou to customize eye behavior\n\nBlinking is a dynamic system\nthat works by increasing the\nchance of blinking over time\noffset by the number of recent\nblinks.  Rapid blinking will\ncause an increased delay before\nthe next blink.\nThreshold is the minimum amount of\nmovement to force a blink to occur\nSmall adjustments are recomm.\n\nSaccade is a type of micro eye\nmovement that adjusts the focus\npoint of the eyes over the surface\nof the current target. Default\nvalues are based off papers on this\nsubject.  Increasing the rate can\nmake a person seem more nervous\nand increasing the amount will make\nthe movement more pronounced.\nMaximum Distance prevents the eyes\nfrom wandering too far from the object\nof interest\n\nWhen the person looks into the eyes\nof the target, their pupils will dialate\nwhich happens in real life when you\nlook at someone you are interested in\n\nEye Update Speed controls how often \nthe eye target is updated for the person.\nIn real life your eyes don't move\nsmoothly but jump from point to point. \nThis helps to make following a target look more realistic");
			uiPersonalitySettingsHelp = new JSONStorableString("Help", "These options control the\npersonality and how a \ntarget is selected and the\nexpression displayed on\nthe face.  The three main\nvalues are based on a white\npaper on personality.\n\nArousal Speed controls how fast\narousal increases when\ninteracting with the person\n\nValence Speed controls how fast\nthe person becomes happy when\nthe target is interacting or\nlooking at them\n\nMood Degrade controls the speed\nat which both arousal and valence\nfall off when there is not alot\nof interaction happening\n\nMovement Interest control how\nmuch arousal and happiness is\ngenerated by movement of body parts\n\nAgreeableness adjusts how\nmuch the person will adjust\ntheir mood according to \nyour interaction with them.\nSuch as how quickly they will\nlook at your hand when fondling\nor look back to you if you look\nat their face\n\nExtraversion adjusts how often\nthe person will smile when happy\nand how long they can hold eye\ncontact. Also has an effect on\ntarget selection when interacting\nwith the person through touches\n\nStableness adjusts the variation\nin expression and the the speed\nat which targets are changed as\nwell as the amount of change in\nmood according to interaction\n\nMain Change Delay controls how\nquickly target calculations are\nupdated for choosing a target\n\nInterest Rate controls how fast\ntargets gain the interest of the\nperson when in view or touching\n\nSmile Suppression Delay adjusts\nhow long smiling can occur \nbefore a break in smiling is forced\n\nExpression Change chance\ncontrols how likely one\nexpression can be overwridden\nby another\n\nExpression Length adjusts\nhow long an expression lasts\n\nAnimation Speed adjusts the\nspeed at which morphs are\nadjusted to create expressions");
			uiDistancesAndAnglesHelp = new JSONStorableString("Help", "These options allow you\nto adjust the distances\nand angles that limit how\nE-Motion determines the \ntarget to look at and\nhow random points are\ngenerated\n\nPersonal space is important\nas it controls how interested\nthe person is in the target\ndependant on distance. the\nfurther into the personal space\nthe target is, the more\ninteresting it becomes\n\nInteraction Distance controls\nhow close a target must be to\nthe person for it to be\nconsidered interacting with\nthe person (such as touching)\n\nFace Interact Distance is a\nseperate value for touching \nthe face\n\nKissing Distance controls how\nclose the target must be to\ntrigger kissing. default value\nis for possession, you may need\nto reduce this if you are not\npossessing another person\n\nDirect View Angle determines\nwhen a target is considered\ndirectly in view to the person\nDynamic Calculation adjusts this\naccording to the distance to \ntarget selected in target settings\n\nPeripheral angle determines\nwhen an object is no longer in\nindirect view of the person\n\nOut of View angle controls\nwhen a target is no longer\nconsidered viewable\n\nTracking angles control the\nmaximum angle the head can\nturn from the body to track\na target\n\nRandom points are generated\ninfront of the person to give\nthem something to look at\nwhen no target is visible or\ninteresting enough to look at\n\nRandom Base distance sets\nhow far from the body the\nrandom points are generated\n\nRandom Base Height sets how\nmuch above or below the head\nrandom points are generated\n\nRandom Base Center Offset\nsets how far apart the left\nand right random points are\ngenerated");
			uiGazeControlsHelp = new JSONStorableString("Help", "These options allow\ncustomisation of how E-Motion\nlooks at targets\n\nGaze Avoidance is a system\nthat makes the person break\neye contact after a period of\ntime to look away for a short\nperiod before looking back again\nAvoidance will not occur if the\ntarget is extremely close\n\nGaze Glancing allows the person\nto glance at the secondary\ninterest while still facing the\nprimary interest\n\nGaze Direct Delay controls\nhow long the person can look\nindirectly at the target before\nturning their head to look\ndirectly at the target\n\nGaze Indirect Decay controls\nhow fast the head turns to direct\nafter the delay above runs out\n\nGaze Speed sets how fast the\nhead turns to follow a target\n\nGaze Variation controls the\namount of angle variation from\nlooking directly at a target\nworks with Gaze Direct Delay\n\nVariation Chance adjusts the\nlikelyhood gaze variation will\nchange when expression changes\n\nGaze Tilt Chance controls the\nlikelyhood of changing the tilt\nof the head. Tracking a moving\ntarget will zero out the tilt\n\nGaze Max Tilt control the max\nangle the head can tilt left\nor right\n\nGaze Tilt Speed controls how\nfast the head tilts");
			uiBodyMovementHelp = new JSONStorableString("Help", "These options control\nhow E-Motion effects the\nbody of the person such\nas breathing and motion\n\nBreathing Speed adjusts\nthe base speed of breathing\nshould be adjusted in small\nincrements.\nBreathing increases as arousal\nincreases.  Rapid breathing\nwill cause the target to hold\ntheir breath for a moment.\nWhen sound is enabled the\nbreathing speed is synced to\nthe sound to ensure they are\nnot cut off\n\nBreath Upper Chest Offset\nis used to adjust the height\nof the upper chest when\nmorphs are applied to create\nthe breathing effects\n\nBreath Raise Multiplier sets\nhow much lift each breath\ncreates on the chest via morphs\n\nBreath Expansion sets how much\nthe entire chest expands with\neach breath\n\nAdjust Shoulders enables\nbreathing and interaction to\nchange the angle and height of\nthe shoulders\n\nShoulders Back adjusts the base\nposition of the shoulders when\nthis feature is enabled\n\nShoulder Adjust Mult controls\nthe amount of change in the\nshoulder position\n\nShoulder Height Mult adjusts\nthe base heigh the shoulders\nare held at\n\nAdjust Chest allows breathing\nto change the angle of the\nchest to accentuate breathing\n\nArm Breast lift applies morphs\nto the breasts when arms are\nraised above the chest lifting\nthem up slightly\n\nAdjust Hands enables random\nfinger pose changes.\nI recommend you use my\nHand Animator plugin instead\nas it is much more powerful\n\nIdle Motion is a system that\nmoves the entire body randomly\nto give some life to static\nposes.  This is done via joint\ndrives so as to not permanently\neffect the static pose\n\nIdle Body Movement effects\nthe body and the legs, shifting\nthe body left and right as well\nas twisting the body along with\ngaze\n\nIdle Arm movement has the arms\nbend up to the chest or down\nto the sides and reacts to\ntouching the breasts\n\nIdle Arm Offset controls the\nbase angle of the arms,\nforward or behind the body\n\nIdle Delays control how many\nseconds the system waits\nbefore trying to set a new\nposition for each body part\n\nIdle Chance controls how\nlikely a new angle is chosen\nonce the delay timer has been\nreached\n\nIdle Speed controls how fast\nthe new angle is set on the\nbody parts");
			uiTargetControlHelp = new JSONStorableString("Help", "These options set what\nE-Motion should be interested\nin.\n\nWhen Choose Person is set\nto NONE E-Motion will look\nat the window camera in \ndesktop mode or the VR headset\nand hands in VR\n\nChoose Person lists all other\ncharacters in the scene to\nallow you to select them\nas the main target of interest\nIf a character is not listed,\nclose and re-open the Target\nSettings tab to refresh the list\n\nChoose Object allows you to set\na non character object as a\nsecondary interest, or even\nanother character.  If a char\nis selected only the head is\nof interest\n\nObject Can Look tells emotion\nthat the objects facing angle\nshould be taken into\nconsideration when determining\nhow interesting it is.\nSuch as when a person is selected\nas the object\n\nTreat Object as Primary overrides\nthe system to keep the person\nlooking at the object\n\nThe Interest multipliers adjust\nthe amount of interest generated\nby the object or body part and\ncan be used to force interest\nin something, or prevent interest\nfrom developing in that part\nSelf Interest are the characters\nown hands");

			backendCurrentFocusTarget = new JSONStorableString("BackendChosenFocus", "None");
			RegisterString(backendCurrentFocusTarget);
			backendCurrentObjectTarget = new JSONStorableString("BackendChosenObject", "");
			RegisterString(backendCurrentObjectTarget);
			

		}

		public void CreateMainMenuUI()
		{
            CreateTextField(uiArousalStatus, true);
			UIDynamic spacer = CreateSpacer(true);
			spacer.height = 30f;
			FileManagerSecure.CreateDirectory(lastPath);
			CreateToggle(uiShowStats, false);
			CreateButton("Load Defaults", false).button.onClick.AddListener(() =>
			{
				loadDefaults();
			});
			CreateButton("Load Preset", false).button.onClick.AddListener(() =>
			{
				//SuperController.LogError("Starting Load Preset");
				SuperController.singleton.fileBrowserUI.defaultPath = lastPath;
				SuperController.singleton.fileBrowserUI.shortCuts = FileManagerSecure.GetShortCutsForDirectory(lastPath, false, false, true, true);
				//SuperController.LogError("Starting Load Preset A");
				SuperController.singleton.fileBrowserUI.SetTextEntry(false);
				//SuperController.LogError("Starting Load Preset B");
				SuperController.singleton.fileBrowserUI.Show((path) =>
				{
					if (string.IsNullOrEmpty(path))
					{
						//SuperController.LogError("Null Path");
						return;
					}
					SimpleJSON.JSONNode loadedSettings = new SimpleJSON.JSONClass();
					loadedSettings=SuperController.singleton.LoadJSON(path);
					//SuperController.LogError("Starting Load Preset C");
					uiExtraversion.val = loadedSettings["Extraversion"].AsFloat;
					uiAgreeableness.val = loadedSettings["Agreeableness"].AsFloat;
					uiStableness.val = loadedSettings["Stableness"].AsFloat;
					uiBreatheSpeed.val = loadedSettings["Breathing Speed"].AsFloat;
					uiBreatheExpandMultiplier.val = loadedSettings["Breathe Morph Multiplier"].AsFloat;
					uiBreatheRaiseMultiplier.val = loadedSettings["Breath Raise Mult"].AsFloat;
					uiChestHeightOffset.val = loadedSettings["Breath Upper Chest Offset"].AsFloat;
					uiGazeVariation.val = loadedSettings["Gaze Angle Variation"].AsFloat;
					uiGazeSpeed.val = loadedSettings["Gaze Speed"].AsFloat;
					uiGazeAvoid.val = loadedSettings["Gaze Avoidance Enable"].AsBool;
					uiGazeLookTime.val = loadedSettings["Gaze Look At Time"].AsFloat;
					uiGazeAvoidTime.val = loadedSettings["Gaze Avoid Look Time"].AsFloat;
					uiGazeGlance.val = loadedSettings["Gaze Glance Enable"].AsBool;
					uiRollSpeed.val = loadedSettings["Gaze Head Roll Speed"].AsFloat;
					uiMaxHeadRoll.val = loadedSettings["Gaze Max Tilt"].AsFloat;
					uiRollChance.val = loadedSettings["Gaze Tilt Chance"].AsFloat;
					uiGlanceTimeout.val = loadedSettings["Glance Timeout Mult"].AsFloat;
					uiSaccadeSpeed.val = loadedSettings["Eye Saccade Frequency"].AsFloat;
					uiSaccadeAmount.val = loadedSettings["Eye Saccade Movement Scale"].AsFloat;
					uiSaccadeWanderMult.val = loadedSettings["Eye Saccade Max Dist from Target Scale"].AsFloat;
					uiBlinkSpeed.val = loadedSettings["Eye Blink Delay Scale"].AsFloat;
					uiPupilDialation.val = loadedSettings["Eye Contact Pupil Dialation"].AsFloat;
					uiPupilRate.val = loadedSettings["Pupil Dialation Speed Mult"].AsFloat;
					uiArousalSpeed.val = loadedSettings["Mood Arousal Scale"].AsFloat;
					uiValenceSpeed.val = loadedSettings["Mood Valence Scale"].AsFloat;
					uiMoodSpeed.val = loadedSettings["Mood Change Scale"].AsFloat;
					uiInterestSpeed.val = loadedSettings["Main Interest Switch Delay Scale"].AsFloat;
					uiInterestRate.val = loadedSettings["Global Interest Rate Scale"].AsFloat;
					uiDoHead.val = loadedSettings["Control Head and Neck Movements"].AsBool;
					uiDoMorphs.val = loadedSettings["Control Breath and Expression Morphs"].AsBool;
					uiAnimationSpeed.val = loadedSettings["Animation Speed Mult"].AsFloat;
					uiDoShoulders.val = loadedSettings["Control Shoulder Movements"].AsBool;
					uiDoChest.val = loadedSettings["Control Chest Movement"].AsBool;
					uiDoSounds.val = loadedSettings["Play Emotion Sounds"].AsBool;
					uiSoundVolume.val = loadedSettings["Sound Volume Scale"].AsFloat;
					uiChestAmount.val = loadedSettings["Chest Movement Scale"].AsFloat;
					uiDoHands.val = loadedSettings["Control Hand Morphs"].AsBool;
					uiConfigHead.val = loadedSettings["Automatically Configure Head and Neck physics"].AsBool;
					uiUsePerson2.val = loadedSettings["Look At Target Person"].AsBool;
					uiShowStats.val = loadedSettings["Show Debug Info on Message Log"].AsBool;
					uiDoKiss.val = loadedSettings["Kissing Enable"].AsBool;
					uiKissAmount.val = loadedSettings["Kissing Morph Scale"].AsFloat;
					uiDoBlowjob.val = loadedSettings["Blowjob Enable"].AsBool;
					uiBlowjobAmount.val = loadedSettings["Blowjob Morph Scale"].AsFloat;
					uiDoSex.val = loadedSettings["Sex Enable"].AsBool;
					uiSexAmount.val = loadedSettings["Sex Morph Scale"].AsFloat;
					uiFocusTarget.val = loadedSettings["Current Focus Target"];
					uiObjectTarget.val = loadedSettings["Current Object Target"];
					uiTargetLook.val = loadedSettings["Object View Direction Enable"].AsBool;
					uiPersonalSpace.val = loadedSettings["Personal Space Distance"].AsFloat;
					uiDirectGaze.val = loadedSettings["Direct Viewing Angle"].AsFloat;
					uiPeripheralGaze.val = loadedSettings["Peripheral Viewing Angle"].AsFloat;
					uiOutOfGaze.val = loadedSettings["Maximum Viewing Angle"].AsFloat;
					uiCloseToFaceDist.val = loadedSettings["Face Interaction Distance"].AsFloat;
					uiKissingDist.val = loadedSettings["Kissing Activation Distance"].AsFloat;
					uiInteractDist.val = loadedSettings["General Interaction Distance"].AsFloat;
					uiMaxMorphSmile.val = loadedSettings["Maximum Allowed Value For Smile Morphs"].AsFloat;
					uiEyeOpenMaxMorph.val = loadedSettings["Maximum Amount To Open Eyes"].AsFloat;
					uiEyeCloseMaxMorph.val = loadedSettings["Maximum Amount To Close Eyes"].AsFloat;
					uiEyeUpdate.val = loadedSettings["Minimum Time Between Eye Target Movements Excl Saccades"].AsFloat;
					uiHeadInterest.val = loadedSettings["Target Head Interest Rate Scale"].AsFloat;
					uiLHandInterest.val = loadedSettings["Target Left Hand Interest Rate Scale"].AsFloat;
					uiRHandInterest.val = loadedSettings["Target Right Hand Interest Rate Scale"].AsFloat;
					uiSelfLHandInterest.val = loadedSettings["Self Left Hand Interest Rate Scale"].AsFloat;
					uiSelfRHandInterest.val = loadedSettings["Self Right Hand Interest Rate Scale"].AsFloat;
					uiPenisInterest.val = loadedSettings["Target Pelvis Interest Rate Scale"].AsFloat;
					uiObjectInterest.val = loadedSettings["Target Object Interest Rate Scale"].AsFloat;
					uiIdleAmount.val = loadedSettings["Idle Movement Mult"].AsFloat;
					uiIdleLegAmount.val = loadedSettings["Idle Leg Movement Mult"].AsFloat;
					uiIdleArmAmount.val = loadedSettings["Idle Arm Movement Mult"].AsFloat;
					uiIdleChance.val = loadedSettings["Idle Movement Chance"].AsFloat;
					uiIdleArmChance.val = loadedSettings["Idle Arm Movement Chance"].AsFloat;
					uiIdleSpeed.val = loadedSettings["Idle Movement Speed"].AsFloat;
					uiIdleHoldPow.val = loadedSettings["Idle Hold Power"].AsFloat;
					uiIdleLegHold.val = loadedSettings["Idle Leg Hold"].AsFloat;
					uiIdleArmSpeed.val = loadedSettings["Idle Arm Movement Speed"].AsFloat;
					uiIdleArmHold.val = loadedSettings["Idle Arm Hold Power"].AsFloat;
					uiIdleBodyDelay.val = loadedSettings["Idle Body Move Delay"].AsFloat;
					uiIdleArmDelay.val = loadedSettings["Idle Arm Move Delay"].AsFloat;
					uiIdleLegDelay.val = loadedSettings["Idle Leg Move Delay"].AsFloat;
					uiShoulderAmount.val = loadedSettings["Shoulder Adjust Mult"].AsFloat;
					uiShoulderHeight.val = loadedSettings["Shoulder Height Mult"].AsFloat;
					uiShoulderBack.val = loadedSettings["Shoulders Back"].AsFloat;
					uiExpressionChance.val = loadedSettings["Expression Chance"].AsFloat;
					uiGazeMaxUp.val = loadedSettings["Tracking Max Up Angle"].AsFloat;
					uiGazeMaxDown.val = loadedSettings["Tracking Max Down Angle"].AsFloat;
					uiGazeMaxSideways.val = loadedSettings["Tracking Max Side/Side Angle"].AsFloat;
					uiRandomBaseDistance.val = loadedSettings["Random Base Distance"].AsFloat;
					uiRandomBaseHeight.val = loadedSettings["Random Base Height"].AsFloat;
					uiRandomBaseOffset.val = loadedSettings["Random Base Center Offset"].AsFloat;
					uiGazeDirectLookDelay.val = loadedSettings["Gaze Direct Delay Mult"].AsFloat;
					uiIndirectDecay.val = loadedSettings["Gaze Indirect Decay"].AsFloat;
					uiMaterialMult.val = loadedSettings["Arousal Gloss Mult"].AsFloat;
					uiEffectMaterial.val = loadedSettings["Arousal Effects Gloss"].AsBool;
					uiHeadAngleOffset.val = loadedSettings["Head Base Angle Offset"].AsFloat;
					uiMouthOpenOffset.val = loadedSettings["Morph Mouth Open Offset"].AsFloat;
					uiLipsCloseOffset.val = loadedSettings["Morph Lips Closed Offset"].AsFloat;
					uiTongueLength.val = loadedSettings["Tongue Length"].AsFloat;
					uiTongueRaise.val = loadedSettings["Tongue Raise"].AsFloat;
					uiObjectAsPrimary.val = loadedSettings["Treat Object As Primary"].AsBool;
					uiEyeControl.val = loadedSettings["Control Eye Target"].AsBool; 
					uiBreastLift.val = loadedSettings["Breast Lift"].AsFloat;
					uiExpressionLength.val = loadedSettings["Expression Length"].AsFloat;
					uiIdleArmOffset.val  = loadedSettings["Idle Arm Pos Offset"].AsFloat;
					uiIdleArmRotOffset.val  = loadedSettings["Idle Arm Rot Offset"].AsFloat;

					uicustomweights.val = loadedSettings["Use Custom Weight"].AsBool; 
					uicustomheadweight.val = loadedSettings["Custom Head Weight"].AsFloat;
					uicustombodyweight.val = loadedSettings["Custom Body Weight"].AsFloat;
					uicustomarmsweight.val = loadedSettings["Custom Arms Weight"].AsFloat;
					uicustomhandsweight.val = loadedSettings["Custom Hands Weight"].AsFloat;
					uicustomlegsweight.val = loadedSettings["Custom Legs Weight"].AsFloat;
					uicustomfeetweight.val = loadedSettings["Custom Feet Weight"].AsFloat;
					uiSmileOffset.val  = loadedSettings["Smile Morphs Offset"].AsFloat;
					uiMovementInterest.val = loadedSettings["Movement Interest Rate"].AsFloat;
					uiMovementInterestArousal.val = loadedSettings["Movement Interest Rate Arousal"].AsFloat;
					uiShowHelp.val = loadedSettings["Show Help Text"].AsBool; 
					uiHeadRotStrength.val = loadedSettings["Head Rotation Strength"].AsFloat;
					uiHeadRotDamper.val = loadedSettings["Head Rotation Damper"].AsFloat;
					uiDynamicDirectAngle.val = loadedSettings["Dynamic Direct Angle"].AsBool;
					uiSmileDamper.val = loadedSettings["Smile Damper"].AsFloat;
					uiMinInterest.val = loadedSettings["Minimum Interest"].AsFloat;
          uiEyeMoveBlinkDist.val = loadedSettings["Eye Move Blink"].AsFloat;
					uiControlTongue.val = loadedSettings["Control Tongue"].AsBool;
					uiControlJaw.val = loadedSettings["Control Jaw"].AsBool;
					uiOnlyBuiltIn.val = loadedSettings["Built In Morphs"].AsBool;
					uiSmileSuppression.val = loadedSettings["Smile Suppression Delay"].AsFloat;
					uiVariationChance.val = loadedSettings["Gaze Variation Chance"].AsFloat;
					uiMoanChance.val = loadedSettings["Moan Chance"].AsFloat;
					uiUseBodyMotion.val = loadedSettings["Use Body Motion Features"].AsBool;


					uiSetupComplete.val = loadedSettings["Setup Complete"].AsBool;

				//SuperController.LogError("Loaded Preset Info");

					//uiLoadPreset.val = false;
					if (uiObjectTarget.val != "None")
					{
						emTargetName = uiObjectTarget.val;
						emTarget = SuperController.singleton.GetAtomByUid(uiObjectTarget.val);
						if (emTarget != null)
						{
							if (emTarget.type == "Person")
							{
								emTargetController = emTarget.GetStorableByID("headControl") as FreeControllerV3;
							}
							else
							{
								emTargetController = emTarget.GetStorableByID("control") as FreeControllerV3;
							}
							if (uiObjectTarget.val == "[CameraRig]")
							{
								emTargetTransform = CameraTarget.centerTarget.transform;
							}
							else
							{
								emTargetTransform = emTargetController.transform;
							}
						}
					}
					else
					{
						emTargetName = "None";
						emTarget = null;
						emTargetController = null;
					}
					if (uiFocusTarget.val != "None" && uiUsePerson2.val)
					{
						person2 = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
						if (person2 != null)
						{
							systemSM.Switch(sReselectPerson2);
						}
						else
						{
							uiFocusTarget.val = "None";
							uiUsePerson2.val = false;
							person2Usable = false;
							usePerson2 = false;
						}
					}
				//SuperController.LogError("Targets updated");

				});
			});
			CreateButton("Save Preset", false).button.onClick.AddListener(() =>
			{
				SuperController.singleton.fileBrowserUI.defaultPath = lastPath; // or path to your plugin
				SuperController.singleton.fileBrowserUI.shortCuts = FileManagerSecure.GetShortCutsForDirectory(lastPath, false, false, true, true);
				SuperController.singleton.fileBrowserUI.SetTextEntry(true);

				SuperController.singleton.fileBrowserUI.Show((path) =>
				{
					//  cancel or invalid
					if (string.IsNullOrEmpty(path))
					{
						return;
					}

					//  ensure extension
					if (!path.EndsWith(".json"))
					{
						path += ".json";
					}
					SimpleJSON.JSONClass mySettings = new SimpleJSON.JSONClass();

					//Add some data                        
					if (uiShowStats.val){mySettings["Show Debug Info on Message Log"] = "True";}else{mySettings["Show Debug Info on Message Log"] = "False";}
					if (uiConfigHead.val){mySettings["Automatically Configure Head and Neck physics"] = "True";}else{mySettings["Automatically Configure Head and Neck physics"] = "False";}
					if (uiDoHead.val){mySettings["Control Head and Neck Movements"] = "True";}else{mySettings["Control Head and Neck Movements"] = "False";}
					if (uiDoMorphs.val){mySettings["Control Breath and Expression Morphs"] = "True";}else{mySettings["Control Breath and Expression Morphs"] = "False";}
					mySettings.Add("Animation Speed Mult", new SimpleJSON.JSONData(uiAnimationSpeed.val));
					if (uiDoShoulders.val){mySettings["Control Shoulder Movements"] = "True";}else{mySettings["Control Shoulder Movements"] = "False";}
					mySettings.Add("Shoulder Movement Scale", new SimpleJSON.JSONData(uiShoulderAmount.val));
					if (uiDoChest.val){mySettings["Control Chest Movement"] = "True";}else{mySettings["Control Chest Movement"] = "False";}
					if (uiDoSounds.val){mySettings["Play Emotion Sounds"] = "True";}else{mySettings["Play Emotion Sounds"] = "False";}
					mySettings.Add("Sound Volume Scale", new SimpleJSON.JSONData(uiSoundVolume.val));
					mySettings.Add("Chest Movement Scale", new SimpleJSON.JSONData(uiChestAmount.val));
					if (uiDoHands.val){mySettings["Control Hand Morphs"] = "True";}else{mySettings["Control Hand Morphs"] = "False";}
					if (uiUsePerson2.val){mySettings["Look At Target Person"] = "True";}else{mySettings["Look At Target Person"] = "False";}
					mySettings.Add("Current Focus Target", new SimpleJSON.JSONData(uiFocusTarget.val));
					mySettings.Add("Current Object Target", new SimpleJSON.JSONData(uiObjectTarget.val));
					if (uiTargetLook.val){mySettings["Object View Direction Enable"] = "True";}else{mySettings["Object View Direction Enable"] = "False";}
					if (uiDoKiss.val){mySettings["Kissing Enable"] = "True";}else{mySettings["Kissing Enable"] = "False";}
					mySettings.Add("Kissing Activation Distance", new SimpleJSON.JSONData(uiKissingDist.val));
					mySettings.Add("Kissing Morph Scale", new SimpleJSON.JSONData(uiKissAmount.val));
					if (uiDoBlowjob.val){mySettings["Blowjob Enable"] = "True";}else{mySettings["Blowjob Enable"] = "False";}
					mySettings.Add("Blowjob Morph Scale", new SimpleJSON.JSONData(uiBlowjobAmount.val));
					if (uiDoSex.val){mySettings["Sex Enable"] = "True";}else{mySettings["Sex Enable"] = "False";}
					mySettings.Add("Sex Morph Scale", new SimpleJSON.JSONData(uiSexAmount.val));
					mySettings.Add("Minimum Time Between Eye Target Movements Excl Saccades", new SimpleJSON.JSONData(uiEyeUpdate.val));
					mySettings.Add("Main Interest Switch Delay Scale", new SimpleJSON.JSONData(uiInterestSpeed.val));
					mySettings.Add("Global Interest Rate Scale", new SimpleJSON.JSONData(uiInterestRate.val));
					mySettings.Add("Extraversion", new SimpleJSON.JSONData(uiExtraversion.val));
					mySettings.Add("Agreeableness", new SimpleJSON.JSONData(uiAgreeableness.val));
					mySettings.Add("Stableness", new SimpleJSON.JSONData(uiStableness.val));
					mySettings.Add("Mood Arousal Scale", new SimpleJSON.JSONData(uiArousalSpeed.val));
					mySettings.Add("Mood Valence Scale", new SimpleJSON.JSONData(uiValenceSpeed.val));
					mySettings.Add("Mood Change Scale", new SimpleJSON.JSONData(uiMoodSpeed.val));
					mySettings.Add("Breathing Speed", new SimpleJSON.JSONData(uiBreatheSpeed.val));
					mySettings.Add("Breath Upper Chest Offset", new SimpleJSON.JSONData(uiChestHeightOffset.val));
					mySettings.Add("Breathe Morph Multiplier", new SimpleJSON.JSONData(uiBreatheExpandMultiplier.val));
					mySettings.Add("Breath Raise Mult", new SimpleJSON.JSONData(uiBreatheRaiseMultiplier.val));
					mySettings.Add("Gaze Speed", new SimpleJSON.JSONData(uiGazeSpeed.val));
					mySettings.Add("Gaze Angle Variation", new SimpleJSON.JSONData(uiGazeVariation.val));
					if (uiGazeAvoid.val){mySettings["Gaze Avoidance Enable"] = "True";}else{mySettings["Gaze Avoidance Enable"] = "False";}
					mySettings.Add("Gaze Look At Time", new SimpleJSON.JSONData(uiGazeLookTime.val));
					mySettings.Add("Gaze Avoid Look Time", new SimpleJSON.JSONData(uiGazeAvoidTime.val));
					if (uiGazeGlance.val){mySettings["Gaze Glance Enable"] = "True";}else{mySettings["Gaze Glance Enable"] = "False";}
					mySettings.Add("Glance Timeout Mult", new SimpleJSON.JSONData(uiGlanceTimeout.val));
					mySettings.Add("Gaze Head Roll Speed", new SimpleJSON.JSONData(uiRollSpeed.val));
					mySettings.Add("Gaze Max Tilt", new SimpleJSON.JSONData(uiMaxHeadRoll.val));
					mySettings.Add("Gaze Tilt Chance", new SimpleJSON.JSONData(uiRollChance.val));
					mySettings.Add("Eye Saccade Frequency", new SimpleJSON.JSONData(uiSaccadeSpeed.val));
					mySettings.Add("Eye Saccade Movement Scale", new SimpleJSON.JSONData(uiSaccadeAmount.val));
					mySettings.Add("Eye Saccade Max Dist from Target Scale", new SimpleJSON.JSONData(uiSaccadeWanderMult.val));
					mySettings.Add("Eye Contact Pupil Dialation", new SimpleJSON.JSONData(uiPupilDialation.val));
					mySettings.Add("Pupil Dialation Speed Mult", new SimpleJSON.JSONData(uiPupilRate.val));
					mySettings.Add("Eye Blink Delay Scale", new SimpleJSON.JSONData(uiBlinkSpeed.val));
					mySettings.Add("Direct Viewing Angle", new SimpleJSON.JSONData(uiDirectGaze.val));
					mySettings.Add("Peripheral Viewing Angle", new SimpleJSON.JSONData(uiPeripheralGaze.val));
					mySettings.Add("Maximum Viewing Angle", new SimpleJSON.JSONData(uiOutOfGaze.val));
					mySettings.Add("Personal Space Distance", new SimpleJSON.JSONData(uiPersonalSpace.val));
					mySettings.Add("Face Interaction Distance", new SimpleJSON.JSONData(uiCloseToFaceDist.val));
					mySettings.Add("General Interaction Distance", new SimpleJSON.JSONData(uiInteractDist.val));
					mySettings.Add("Target Head Interest Rate Scale", new SimpleJSON.JSONData(uiHeadInterest.val));
					mySettings.Add("Target Left Hand Interest Rate Scale", new SimpleJSON.JSONData(uiLHandInterest.val));
					mySettings.Add("Target Right Hand Interest Rate Scale", new SimpleJSON.JSONData(uiRHandInterest.val));
					mySettings.Add("Self Left Hand Interest Rate Scale", new SimpleJSON.JSONData(uiSelfLHandInterest.val));
					mySettings.Add("Self Right Hand Interest Rate Scale", new SimpleJSON.JSONData(uiSelfRHandInterest.val));
					mySettings.Add("Target Pelvis Interest Rate Scale", new SimpleJSON.JSONData(uiPenisInterest.val));
					mySettings.Add("Target Object Interest Rate Scale", new SimpleJSON.JSONData(uiObjectInterest.val));
					mySettings.Add("Idle Movement Mult", new SimpleJSON.JSONData(uiIdleAmount.val));
					mySettings.Add("Idle Leg Movement Mult", new SimpleJSON.JSONData(uiIdleLegAmount.val));
					mySettings.Add("Idle Hold Power", new SimpleJSON.JSONData(uiIdleHoldPow.val));
					mySettings.Add("Idle Leg Hold", new SimpleJSON.JSONData(uiIdleLegHold.val));
					mySettings.Add("Idle Arm Movement Mult", new SimpleJSON.JSONData(uiIdleArmAmount.val));
					mySettings.Add("Idle Arm Hold Power", new SimpleJSON.JSONData(uiIdleArmHold.val));
					mySettings.Add("Idle Arm Movement Speed", new SimpleJSON.JSONData(uiIdleArmSpeed.val));
					mySettings.Add("Idle Movement Chance", new SimpleJSON.JSONData(uiIdleChance.val));
					mySettings.Add("Idle Arm Movement Chance", new SimpleJSON.JSONData(uiIdleArmChance.val));
					mySettings.Add("Idle Movement Speed", new SimpleJSON.JSONData(uiIdleSpeed.val));
					mySettings.Add("Idle Body Move Delay", new SimpleJSON.JSONData(uiIdleBodyDelay.val));
					mySettings.Add("Idle Arm Move Delay", new SimpleJSON.JSONData(uiIdleArmDelay.val));
					mySettings.Add("Idle Leg Move Delay", new SimpleJSON.JSONData(uiIdleLegDelay.val));
					mySettings.Add("Shoulder Adjust Mult", new SimpleJSON.JSONData(uiShoulderAmount.val));
					mySettings.Add("Shoulder Height Mult", new SimpleJSON.JSONData(uiShoulderHeight.val));
					mySettings.Add("Shoulders Back", new SimpleJSON.JSONData(uiShoulderBack.val));
					mySettings.Add("Expression Chance", new SimpleJSON.JSONData(uiExpressionChance.val));
					mySettings.Add("Tracking Max Up Angle", new SimpleJSON.JSONData(uiGazeMaxUp.val));
					mySettings.Add("Tracking Max Down Angle", new SimpleJSON.JSONData(uiGazeMaxDown.val));
					mySettings.Add("Tracking Max Side/Side Angle", new SimpleJSON.JSONData(uiGazeMaxSideways.val));
					mySettings.Add("Random Base Distance", new SimpleJSON.JSONData(uiRandomBaseDistance.val));
					mySettings.Add("Random Base Height", new SimpleJSON.JSONData(uiRandomBaseHeight.val));
					mySettings.Add("Random Base Center Offset", new SimpleJSON.JSONData(uiRandomBaseOffset.val));
					mySettings.Add("Gaze Direct Delay Mult", new SimpleJSON.JSONData(uiGazeDirectLookDelay.val));
					mySettings.Add("Gaze Indirect Decay", new SimpleJSON.JSONData(uiIndirectDecay.val));
					mySettings.Add("Arousal Gloss Mult", new SimpleJSON.JSONData(uiMaterialMult.val));
					mySettings.Add("Head Base Angle Offset", new SimpleJSON.JSONData(uiHeadAngleOffset.val));
					mySettings.Add("Morph Mouth Open Offset", new SimpleJSON.JSONData(uiMouthOpenOffset.val));
					mySettings.Add("Morph Lips Closed Offset", new SimpleJSON.JSONData(uiLipsCloseOffset.val));
					mySettings.Add("Maximum Allowed Value For Smile Morphs", new SimpleJSON.JSONData(uiMaxMorphSmile.val));
					mySettings.Add("Maximum Amount To Close Eyes", new SimpleJSON.JSONData(uiEyeCloseMaxMorph.val));
					mySettings.Add("Maximum Amount To Open Eyes", new SimpleJSON.JSONData(uiEyeOpenMaxMorph.val));
					mySettings.Add("Tongue Length", new SimpleJSON.JSONData(uiTongueLength.val));
					mySettings.Add("Tongue Raise", new SimpleJSON.JSONData(uiTongueRaise.val));
					mySettings.Add("Breast Lift", new SimpleJSON.JSONData(uiBreastLift.val));
					mySettings.Add("Expression Length", new SimpleJSON.JSONData(uiExpressionLength.val));
					mySettings.Add("Use Custom Weight", new SimpleJSON.JSONData(uicustomweights.val));
					mySettings.Add("Custom Head Weight", new SimpleJSON.JSONData(uicustomheadweight.val));
					mySettings.Add("Custom Body Weight", new SimpleJSON.JSONData(uicustombodyweight.val));
					mySettings.Add("Custom Arms Weight", new SimpleJSON.JSONData(uicustomarmsweight.val));
					mySettings.Add("Custom Hands Weight", new SimpleJSON.JSONData(uicustomhandsweight.val));
					mySettings.Add("Custom Legs Weight", new SimpleJSON.JSONData(uicustomlegsweight.val));
					mySettings.Add("Custom Feet Weight", new SimpleJSON.JSONData(uicustomfeetweight.val));
					mySettings.Add("Idle Arm Pos Offset", new SimpleJSON.JSONData(uiIdleArmOffset.val));
					mySettings.Add("Idle Arm Rot Offset", new SimpleJSON.JSONData(uiIdleArmRotOffset.val));
					mySettings.Add("Smile Morphs Offset", new SimpleJSON.JSONData(uiSmileOffset.val));
					mySettings.Add("Movement Interest Rate", new SimpleJSON.JSONData(uiMovementInterest.val));
					mySettings.Add("Movement Interest Rate Arousal", new SimpleJSON.JSONData(uiMovementInterestArousal.val));
					mySettings.Add("Head Rotation Strength", new SimpleJSON.JSONData(uiHeadRotStrength.val));
					mySettings.Add("Head Rotation Damper", new SimpleJSON.JSONData(uiHeadRotDamper.val));
					mySettings.Add("Smile Damper", new SimpleJSON.JSONData(uiSmileDamper.val));
					mySettings.Add("Minimum Interest", new SimpleJSON.JSONData(uiMinInterest.val));
					mySettings.Add("Eye Move Blink", new SimpleJSON.JSONData(uiEyeMoveBlinkDist.val));
					mySettings.Add("Smile Suppression Delay", new SimpleJSON.JSONData(uiSmileSuppression.val));
					mySettings.Add("Gaze Variation Chance", new SimpleJSON.JSONData(uiVariationChance.val));
					mySettings.Add("Moan Chance", new SimpleJSON.JSONData(uiMoanChance.val));



					if (uiControlTongue.val){mySettings["Control Tongue"] = "True";}else{mySettings["Control Tongue"] = "False";}
					if (uiControlJaw.val){mySettings["Control Jaw"] = "True";}else{mySettings["Control Jaw"] = "False";}
					if (uiOnlyBuiltIn.val){mySettings["Built In Morphs"] = "True";}else{mySettings["Built In Morphs"] = "False";}
					if (uiUseBodyMotion.val){mySettings["Use Body Motion Features"] = "True";}else{mySettings["Use Body Motion Features"] = "False";}

					if (uiDynamicDirectAngle.val){mySettings["Dynamic Direct Angle"] = "True";}else{mySettings["Dynamic Direct Angle"] = "False";}

					if (uiShowHelp.val){mySettings["Show Help Text"] = "True";}else{mySettings["Show Help Text"] = "False";}
					
					if (uiObjectAsPrimary.val){mySettings["Treat Object As Primary"] = "True";}else{mySettings["Treat Object As Primary"] = "False";}
					if (uiEyeControl.val && mEyesClosedLeftValue < 0.7f){mySettings["Control Eye Target"] = "True";}else{mySettings["Control Eye Target"] = "False";}
					
					if (uiEffectMaterial.val){mySettings["Arousal Effects Gloss"] = "True";}else{mySettings["Arousal Effects Gloss"] = "False";}
					if (uiSetupComplete.val){mySettings["Setup Complete"] = "True";}else{mySettings["Setup Complete"] = "False";}
					SuperController.singleton.SaveJSON(mySettings,path);
					//SuperController.singleton.SaveStringIntoFile(path, json.ToString(""));
					//SuperController.LogMessage("Wrote settings file: " + path);
				});

				//  set default filename
				if (SuperController.singleton.fileBrowserUI.fileEntryField != null)
				{
					SuperController.singleton.fileBrowserUI.fileEntryField.text = "EMotion_Preset" + ".json";
					SuperController.singleton.fileBrowserUI.ActivateFileNameField();
				}


			});

            //CreateTextField(uiValenceStatus, true);

			CreateButtons();
			ColorButtons();
		}

		public void CreateButtons()
		{
			featureButton = CreateButton("Feature Controls", false);
			featureButton.button.onClick.AddListener(() => 
				{
					if (uiShowingFeatures)
					{
						RemoveFeatureControlUI();
					}
					else
					{
						CreateFeatureControlUI();
					}
				});

			distanceButton = CreateButton("Distances and Angles", true);
			distanceButton.button.onClick.AddListener(() => 
				{
					if (uiShowingDistAngle)
					{
						RemoveDistAngleUI();
					}
					else
					{
						CreateDistAngleUI();
					}
				});

			characterButton = CreateButton("Look Adjustments", false);
			characterButton.button.onClick.AddListener(() => 
				{
					if (uiShowingCharacter)
					{
						RemoveCharacterControl();
					}
					else
					{
						CreateCharacterControl();
					}
				});

			gazeButton = CreateButton("Gaze Controls", true);
			gazeButton.button.onClick.AddListener(() => 
				{
					if (uiShowingGaze)
					{
						RemoveGazeUI();
					}
					else
					{
						CreateGazeUI();
					}
				});

			eyeButton = CreateButton("Eye Controls", false);
			eyeButton.button.onClick.AddListener(() => 
				{
					if (uiShowingEye)
					{
						RemoveEyeControls();
					}
					else
					{
						CreateEyeControls();
					}
				});

			idleButton = CreateButton("Body Movement Settings", true);
			idleButton.button.onClick.AddListener(() => 
				{
					if (uiShowingIdleBreathing)
					{
						RemoveIdleBeathingUI();
					}
					else
					{
						CreateIdleBeathingUI();
					}
				});

			personalityButton = CreateButton("Personality Settings", false);
			personalityButton.button.onClick.AddListener(() => 
				{
					if (uiShowingPersonality)
					{
						RemovePersonalityUI();
					}
					else
					{
						CreatePersonalityUI();
					}
				});

			targetButton = CreateButton("Target Settings", true);
			targetButton.button.onClick.AddListener(() => 
				{
					if (uiShowingTarget)
					{
						RemoveTargetUI();
					}
					else
					{
						CreateTargetUI();
					}
				});
		}
		
		public void ColorButtons()
		{
			if (uiShowingFeatures)
			{featureButton.buttonColor = Color.green;}
			else
			{featureButton.buttonColor = Color.gray;}
			if (uiShowingDistAngle)
			{distanceButton.buttonColor = Color.green;}
			else
			{distanceButton.buttonColor = Color.gray;}
			if (uiShowingCharacter)
			{characterButton.buttonColor = Color.green;}
			else
			{characterButton.buttonColor = Color.gray;}
			if (uiShowingGaze)
			{gazeButton.buttonColor = Color.green;}
			else
			{gazeButton.buttonColor = Color.gray;}
			if (uiShowingEye)
			{eyeButton.buttonColor = Color.green;}
			else
			{eyeButton.buttonColor = Color.gray;}
			if (uiShowingTarget)
			{targetButton.buttonColor = Color.green;}
			else
			{targetButton.buttonColor = Color.gray;}
			if (uiShowingPersonality)
			{personalityButton.buttonColor = Color.green;}
			else
			{personalityButton.buttonColor = Color.gray;}
			if (uiShowingIdleBreathing)
			{idleButton.buttonColor = Color.green;}
			else
			{idleButton.buttonColor = Color.gray;}
			if (uiShowHelp.val == true)
			{
				if (uiShowingFeatures && uiShowingDistAngle == false && uiShowingCharacter == false && uiShowingEye == false && uiShowingGaze == false && uiShowingIdleBreathing == false && uiShowingPersonality == false && uiShowingTarget == false)
				{
					UIDynamicTextField dtf = CreateTextField(uiFeatureControlHelp, true);
					dtf.height = 1200;
				}
				else
				{
					RemoveTextField(uiFeatureControlHelp);
				}
				if (uiShowingCharacter && uiShowingDistAngle == false && uiShowingFeatures == false && uiShowingEye == false && uiShowingGaze == false && uiShowingIdleBreathing == false && uiShowingPersonality == false && uiShowingTarget == false)
				{
					UIDynamicTextField dtf = CreateTextField(uiLookAdjustmentHelp, true);
					dtf.height = 1200;
				}
				else
				{
					RemoveTextField(uiLookAdjustmentHelp);
				}
				if (uiShowingEye && uiShowingDistAngle == false && uiShowingFeatures == false && uiShowingCharacter == false && uiShowingGaze == false && uiShowingIdleBreathing == false && uiShowingPersonality == false && uiShowingTarget == false)
				{
					UIDynamicTextField dtf = CreateTextField(uiEyeControlHelp, true);
					dtf.height = 1400;
				}
				else
				{
					RemoveTextField(uiEyeControlHelp);
				}
				if (uiShowingPersonality && uiShowingDistAngle == false && uiShowingFeatures == false && uiShowingCharacter == false && uiShowingGaze == false && uiShowingIdleBreathing == false && uiShowingEye == false && uiShowingTarget == false)
				{
					UIDynamicTextField dtf = CreateTextField(uiPersonalitySettingsHelp, true);
					dtf.height = 2100;
				}
				else
				{
					RemoveTextField(uiPersonalitySettingsHelp);
				}
				if (uiShowingDistAngle && uiShowingPersonality == false && uiShowingFeatures == false && uiShowingCharacter == false && uiShowingGaze == false && uiShowingIdleBreathing == false && uiShowingEye == false && uiShowingTarget == false)
				{
					UIDynamicTextField dtf = CreateTextField(uiDistancesAndAnglesHelp, false);
					dtf.height = 2200;
				}
				else
				{
					RemoveTextField(uiDistancesAndAnglesHelp);
				}
				if (uiShowingGaze && uiShowingPersonality == false && uiShowingFeatures == false && uiShowingCharacter == false && uiShowingDistAngle == false && uiShowingIdleBreathing == false && uiShowingEye == false && uiShowingTarget == false)
				{
					UIDynamicTextField dtf = CreateTextField(uiGazeControlsHelp, false);
					dtf.height = 1400;
				}
				else
				{
					RemoveTextField(uiGazeControlsHelp);
				}
				if (uiShowingIdleBreathing && uiShowingPersonality == false && uiShowingFeatures == false && uiShowingCharacter == false && uiShowingDistAngle == false && uiShowingGaze == false && uiShowingEye == false && uiShowingTarget == false)
				{
					UIDynamicTextField dtf = CreateTextField(uiBodyMovementHelp, false);
					dtf.height = 3200;
				}
				else
				{
					RemoveTextField(uiBodyMovementHelp);
				}
				if (uiShowingTarget && uiShowingPersonality == false && uiShowingFeatures == false && uiShowingCharacter == false && uiShowingDistAngle == false && uiShowingGaze == false && uiShowingEye == false && uiShowingIdleBreathing == false)
				{
					UIDynamicTextField dtf = CreateTextField(uiTargetControlHelp, false);
					dtf.height = 1400;
				}
				else
				{
					RemoveTextField(uiTargetControlHelp);
				}
				if (uiShowingTarget == false && uiShowingPersonality == false && uiShowingFeatures == false && uiShowingCharacter == false && uiShowingDistAngle == false && uiShowingGaze == false && uiShowingEye == false && uiShowingIdleBreathing == false)
				{
					UIDynamicTextField dtf = CreateTextField(uiOverviewLeftHelp, false);
					dtf.height = 1600;
					UIDynamicTextField dtf2 = CreateTextField(uiOverviewRightHelp, true);
					dtf2.height = 1200;
				}
				else
				{
					RemoveTextField(uiOverviewLeftHelp);
					RemoveTextField(uiOverviewRightHelp);
				}
			}
			else
			{
				RemoveTextField(uiFeatureControlHelp);
				RemoveTextField(uiLookAdjustmentHelp);
				RemoveTextField(uiEyeControlHelp);
				RemoveTextField(uiPersonalitySettingsHelp);
				RemoveTextField(uiDistancesAndAnglesHelp);
				RemoveTextField(uiGazeControlsHelp);
				RemoveTextField(uiBodyMovementHelp);
				RemoveTextField(uiTargetControlHelp);
				RemoveTextField(uiOverviewLeftHelp);
				RemoveTextField(uiOverviewRightHelp);
			}
		}

		public void CreatePersonalityUI()
		{
			CreateSlider(uiArousalSpeed, false);
			CreateSlider(uiValenceSpeed, false);
			CreateSlider(uiMoodSpeed, false);
			CreateSlider(uiMovementInterest, false);
			CreateSlider(uiMovementInterestArousal, false);
			CreateSlider(uiAgreeableness, false);
			CreateSlider(uiExtraversion, false);
			CreateSlider(uiStableness, false);
			CreateSlider(uiSmileSuppression, false);
			CreateSlider(uiInterestSpeed, false);
			CreateSlider(uiInterestRate, false);
			CreateSlider(uiMinInterest, false);
			CreateSlider(uiExpressionChance, false);
			CreateSlider(uiExpressionLength, false);
			CreateSlider(uiAnimationSpeed, false);
			uiShowingPersonality = true;
			ColorButtons();
		}
		public void RemovePersonalityUI()
		{
			RemoveSlider(uiAgreeableness);
			RemoveSlider(uiExtraversion);
			RemoveSlider(uiStableness);
			RemoveSlider(uiInterestSpeed);
			RemoveSlider(uiInterestRate);
			RemoveSlider(uiSmileSuppression);
			RemoveSlider(uiMinInterest);
			RemoveSlider(uiArousalSpeed);
			RemoveSlider(uiValenceSpeed);
			RemoveSlider(uiMovementInterest);
			RemoveSlider(uiMovementInterestArousal);
			RemoveSlider(uiMoodSpeed);
			RemoveSlider(uiExpressionChance);
			RemoveSlider(uiExpressionLength);
			RemoveSlider(uiAnimationSpeed);
			uiShowingPersonality = false;
			ColorButtons();
		}
		
		public void CreateTargetUI()
		{
			CreateToggle(uiUsePerson2, true);


			List<string> targetChoices = new List<string>();
			targetChoices.Add("None");
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
				currentAtom = SuperController.singleton.GetAtomByUid(atomUID);
                if (currentAtom != containingAtom && atomUID != null && currentAtom.type == "Person")
                {
					targetChoices.Add(atomUID);
				}
			}
			uiFocusTarget = new JSONStorableStringChooser("Target Selector", targetChoices, uiFocusTarget.val, "Choose Person");
			UIDynamicPopup udp = CreatePopup(uiFocusTarget, true);

			List<string> objectChoices = new List<string>();
			objectChoices.Add("None");
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
				currentAtom = SuperController.singleton.GetAtomByUid(atomUID);
                if (atomUID != null)
                {
						objectChoices.Add(atomUID);
				}
			}
			uiObjectTarget = new JSONStorableStringChooser("Object Selector", objectChoices, uiObjectTarget.val, "Choose Object");
			UIDynamicPopup udp2 = CreatePopup(uiObjectTarget, true);
			
			
			CreateToggle(uiTargetLook, true);
			CreateToggle(uiObjectAsPrimary, true);
			CreateSlider(uiObjectInterest, true);
			CreateSlider(uiHeadInterest, true);
			CreateSlider(uiLHandInterest, true);
			CreateSlider(uiRHandInterest, true);
			CreateSlider(uiPenisInterest, true);
			CreateSlider(uiSelfLHandInterest, true);
			CreateSlider(uiSelfRHandInterest, true);
			uiShowingTarget = true;
			ColorButtons();
		}
		public void RemoveTargetUI()
		{
			RemoveToggle(uiUsePerson2);
			RemovePopup(uiFocusTarget);
			RemovePopup(uiObjectTarget);
			RemoveToggle(uiTargetLook);
			RemoveToggle(uiObjectAsPrimary);
			RemoveSlider(uiObjectInterest);
			RemoveSlider(uiHeadInterest);
			RemoveSlider(uiLHandInterest);
			RemoveSlider(uiRHandInterest);
			RemoveSlider(uiPenisInterest);
			RemoveSlider(uiSelfLHandInterest);
			RemoveSlider(uiSelfRHandInterest);
			uiShowingTarget = false;
			ColorButtons();
		}

		public void CreateFeatureControlUI()
		{
			CreateToggle(uiShowHelp, false);
			CreateToggle(uiConfigHead, false);
			CreateSlider(uiHeadRotStrength, false);
			CreateSlider(uiHeadRotDamper, false);
			CreateToggle(uiEyeControl, false);
			CreateToggle(uiControlTongue, false);
			CreateToggle(uiControlJaw, false);
			CreateToggle(uiDoHead, false);
			CreateToggle(uiDoMorphs, false);
			CreateToggle(uiOnlyBuiltIn, false);
			CreateToggle(uiUseBodyMotion, false);
			CreateToggle(uiDoSounds, false);
			CreateSlider(uiMoanChance, false);
			CreateSlider(uiSoundVolume, false);
			CreateToggle(uiDoKiss, false);
			CreateSlider(uiKissAmount, false);
			CreateToggle(uiDoBlowjob, false);
			CreateSlider(uiBlowjobAmount, false);
			CreateToggle(uiDoSex, false);
			//CreateSlider(uiSexAmount, false);
			uiShowingFeatures = true;
			ColorButtons();
		}
		public void RemoveFeatureControlUI()
		{
			RemoveToggle(uiShowHelp);
			RemoveToggle(uiConfigHead);
			RemoveSlider(uiHeadRotStrength);
			RemoveSlider(uiHeadRotDamper);
			RemoveToggle(uiEyeControl);
			RemoveToggle(uiControlTongue);
			RemoveToggle(uiControlJaw);
			RemoveToggle(uiDoHead);
			RemoveToggle(uiDoMorphs);
			RemoveToggle(uiOnlyBuiltIn);
			RemoveToggle(uiUseBodyMotion);
			RemoveToggle(uiDoSounds);
			RemoveSlider(uiMoanChance);
			RemoveSlider(uiSoundVolume);
			RemoveToggle(uiDoKiss);
			RemoveSlider(uiKissAmount);
			RemoveToggle(uiDoBlowjob);
			RemoveSlider(uiBlowjobAmount);
			RemoveToggle(uiDoSex);
			//RemoveSlider(uiSexAmount);
			uiShowingFeatures = false;
			ColorButtons();
		}

		public void CreateGazeUI()
		{
			CreateToggle(uiGazeAvoid, true);
			CreateSlider(uiGazeLookTime, true);
			CreateSlider(uiGazeAvoidTime, true);
			CreateToggle(uiGazeGlance, true);
			CreateSlider(uiGlanceTimeout, true);
			CreateSlider(uiGazeDirectLookDelay, true);
			CreateSlider(uiIndirectDecay, true);
			CreateSlider(uiGazeSpeed, true);
			CreateSlider(uiGazeVariation, true);
			CreateSlider(uiVariationChance, true);
			CreateSlider(uiRollChance, true);
			CreateSlider(uiMaxHeadRoll, true);
			CreateSlider(uiRollSpeed, true);
			uiShowingGaze = true;
			ColorButtons();
		}
		public void RemoveGazeUI()
		{
			RemoveToggle(uiGazeAvoid);
			RemoveSlider(uiGazeLookTime);
			RemoveSlider(uiGazeAvoidTime);
			RemoveToggle(uiGazeGlance);
			RemoveSlider(uiGlanceTimeout);
			RemoveSlider(uiGazeDirectLookDelay);
			RemoveSlider(uiIndirectDecay);
			RemoveSlider(uiVariationChance);
			RemoveSlider(uiGazeSpeed);
			RemoveSlider(uiGazeVariation);
			RemoveSlider(uiRollChance);
			RemoveSlider(uiMaxHeadRoll);
			RemoveSlider(uiRollSpeed);
			uiShowingGaze = false;
			ColorButtons();
		}

		public void CreateIdleBeathingUI()
		{
			CreateSlider(uiBreatheSpeed, true);
			CreateSlider(uiChestHeightOffset, true);
			CreateSlider(uiBreatheRaiseMultiplier, true);
			CreateSlider(uiBreatheExpandMultiplier, true);
			CreateToggle(uiDoShoulders, true);
			CreateSlider(uiShoulderBack, true);
			CreateSlider(uiShoulderAmount, true);
			CreateSlider(uiShoulderHeight, true);
			CreateToggle(uiDoChest, true);
			CreateSlider(uiChestAmount, true);
			CreateToggle(uiDoHands, true);
			CreateSlider(uiIdleBodyDelay, true);
			CreateSlider(uiIdleArmDelay, true);
			CreateSlider(uiIdleLegDelay, true);
			CreateSlider(uiIdleHoldPow, true);
			CreateSlider(uiIdleAmount, true);
			CreateSlider(uiIdleArmHold, true);
			CreateSlider(uiIdleArmAmount, true);
			CreateSlider(uiIdleArmOffset, true);
			CreateSlider(uiIdleArmRotOffset, true);
			CreateSlider(uiIdleLegHold, true);
			CreateSlider(uiIdleLegAmount, true);
			CreateSlider(uiIdleChance, true);
			CreateSlider(uiIdleArmChance, true);
			CreateSlider(uiIdleSpeed, true);
			CreateSlider(uiIdleArmSpeed, true);
			uiShowingIdleBreathing = true;
			ColorButtons();
		}
		public void RemoveIdleBeathingUI()
		{
			RemoveSlider(uiBreatheSpeed);
			RemoveSlider(uiChestHeightOffset);
			RemoveSlider(uiBreatheRaiseMultiplier);
			RemoveSlider(uiBreatheExpandMultiplier);
			RemoveToggle(uiDoShoulders);
			RemoveSlider(uiShoulderBack);
			RemoveSlider(uiShoulderAmount);
			RemoveSlider(uiShoulderHeight);
			RemoveToggle(uiDoChest);
			RemoveSlider(uiChestAmount);
			RemoveToggle(uiDoHands);
			RemoveSlider(uiIdleAmount);
			RemoveSlider(uiIdleLegAmount);
			RemoveSlider(uiIdleBodyDelay);
			RemoveSlider(uiIdleArmDelay);
			RemoveSlider(uiIdleLegDelay);
			RemoveSlider(uiIdleHoldPow);
			RemoveSlider(uiIdleArmHold);
			RemoveSlider(uiIdleLegHold);
			RemoveSlider(uiIdleArmAmount);
			RemoveSlider(uiIdleArmOffset);
			RemoveSlider(uiIdleArmRotOffset);
			RemoveSlider(uiIdleChance);
			RemoveSlider(uiIdleArmChance);
			RemoveSlider(uiIdleSpeed);
			RemoveSlider(uiIdleArmSpeed);
			uiShowingIdleBreathing = false;
			ColorButtons();
		}

		public void CreateDistAngleUI()
		{
			CreateSlider(uiPersonalSpace, true);
			CreateSlider(uiInteractDist, true);
			CreateSlider(uiCloseToFaceDist, true);
			CreateSlider(uiKissingDist, true);
			CreateToggle(uiDynamicDirectAngle, true);
			CreateSlider(uiDirectGaze, true);
			CreateSlider(uiPeripheralGaze, true);
			CreateSlider(uiOutOfGaze, true);
			CreateSlider(uiGazeMaxUp, true);
			CreateSlider(uiGazeMaxDown, true);
			CreateSlider(uiGazeMaxSideways, true);
			CreateSlider(uiRandomBaseDistance, true);
			CreateSlider(uiRandomBaseHeight, true);
			CreateSlider(uiRandomBaseOffset, true);
			uiShowingDistAngle = true;
			ColorButtons();
		}
		public void RemoveDistAngleUI()
		{
			RemoveSlider(uiPersonalSpace);
			RemoveSlider(uiInteractDist);
			RemoveSlider(uiCloseToFaceDist);
			RemoveSlider(uiKissingDist);
			RemoveToggle(uiDynamicDirectAngle);
			RemoveSlider(uiDirectGaze);
			RemoveSlider(uiPeripheralGaze);
			RemoveSlider(uiOutOfGaze);
			RemoveSlider(uiGazeMaxUp);
			RemoveSlider(uiGazeMaxDown);
			RemoveSlider(uiGazeMaxSideways);
			RemoveSlider(uiRandomBaseDistance);
			RemoveSlider(uiRandomBaseHeight);
			RemoveSlider(uiRandomBaseOffset);
			uiShowingDistAngle = false;
			ColorButtons();
		}
		public void CreateEyeControls()
		{
			CreateSlider(uiBlinkSpeed, false);
			CreateSlider(uiEyeMoveBlinkDist, false);
			CreateSlider(uiSaccadeSpeed, false);
			CreateSlider(uiSaccadeAmount, false);
			CreateSlider(uiSaccadeWanderMult, false);
			CreateSlider(uiPupilDialation, false);
			CreateSlider(uiPupilRate, false);
			CreateSlider(uiEyeUpdate, false);
			uiShowingEye = true;
			ColorButtons();
		}
		public void RemoveEyeControls()
		{
			RemoveSlider(uiBlinkSpeed);
			RemoveSlider(uiEyeMoveBlinkDist);
			RemoveSlider(uiSaccadeSpeed);
			RemoveSlider(uiSaccadeAmount);
			RemoveSlider(uiSaccadeWanderMult);
			RemoveSlider(uiPupilDialation);
			RemoveSlider(uiPupilRate);
			RemoveSlider(uiEyeUpdate);
			uiShowingEye = false;
			ColorButtons();
		}
		private void CreateCharacterControl()
		{
			uiShowingCharacter = true;
			ColorButtons();
			CreateSlider(uiMouthOpenOffset, false);
			CreateSlider(uiLipsCloseOffset, false);
			CreateSlider(uiMaxMorphSmile, false);
			CreateSlider(uiSmileOffset, false);
			CreateSlider(uiSmileDamper, false);
			CreateSlider(uiEyeOpenMaxMorph, false);
			CreateSlider(uiEyeCloseMaxMorph, false);
			CreateSlider(uiHeadAngleOffset, false);
			CreateSlider(uiTongueLength, false);
			CreateSlider(uiTongueRaise, false);
			CreateSlider(uiBreastLift, false);
			CreateToggle(uicustomweights, false);
			CreateSlider(uicustomheadweight, false);
			CreateSlider(uicustombodyweight, false);
			CreateSlider(uicustomarmsweight, false);
			CreateSlider(uicustomhandsweight, false);
			CreateSlider(uicustomlegsweight, false);
			CreateSlider(uicustomfeetweight, false);
		}
		private void RemoveCharacterControl()
		{
			RemoveSlider(uiMouthOpenOffset);
			RemoveSlider(uiLipsCloseOffset);
			RemoveSlider(uiMaxMorphSmile);
			RemoveSlider(uiSmileOffset);
			RemoveSlider(uiSmileDamper);
			RemoveSlider(uiEyeOpenMaxMorph);
			RemoveSlider(uiEyeCloseMaxMorph);
			RemoveSlider(uiHeadAngleOffset);
			RemoveSlider(uiTongueLength);
			RemoveSlider(uiTongueRaise);
			RemoveSlider(uiBreastLift);
			RemoveToggle(uicustomweights);
			RemoveSlider(uicustomheadweight);
			RemoveSlider(uicustombodyweight);
			RemoveSlider(uicustomarmsweight);
			RemoveSlider(uicustomhandsweight);
			RemoveSlider(uicustomlegsweight);
			RemoveSlider(uicustomfeetweight);
			uiShowingCharacter = false;
			ColorButtons();
		}
	}
	public static class MVRScriptExtension
	{
		public static string GetPackagePath(this MVRScript script)
		{
			string packageId = script.GetPackageId();
			return packageId == "" ? "" : $"{packageId}:/";
		}

		//MacGruber / Discord 20.10.2020
		//Get path prefix of the package that contains this plugin
		public static string GetPackageId(this MVRScript script)
		{
			string id = script.name.Substring(0, script.name.IndexOf('_'));
			string filename = script.manager.GetJSON()["plugins"][id].Value;
			int idx = filename.IndexOf(":/", StringComparison.Ordinal);
			return idx >= 0 ? filename.Substring(0, idx) : "";
		}
	}
}
