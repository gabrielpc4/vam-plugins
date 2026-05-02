using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
using System.IO;

namespace VRAdultFun
{
    partial class EasyMotionLite : MVRScript
    {
        public static readonly bool IsFaceOnlyExpressionMode = true;

        #region Initilation Variables
        //private static Atom debugUI;
        //private static UITextControl debugUIControl;
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
        private static string logLine;
        private static Atom aCube;
		private static FreeControllerV3 aCubeController;
        private static float saccadeClock = 0.0f;
		private static float saccadeRepeat = 0.0f;
        private static float eyeClock = 0.0f;
		private static float eyeCloseMaxMorph = 1.1f;
        private static float randomX = 0.0f;
        private static float randomY = 0.0f;
        private static string currentInterest = "Random";
        private static string prevInterest = "Random";
        private static float currentInterestLevel = 0.0f;
        private static float maxInterestLevel = 100.0f;
        private static float interestClock = 0.0f;
        private static float interestArousal = 0.0f;
        private static float interestValence = 0.0f;
        private static float interestMaxSmile = 0.65f;
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
        private static float eyesNonDirectClock = 0.0f;
        private static float eyesNonDirectAngle = 5.0f;
		private static float eyesDirectClock = 0.0f;
        private static float fuzzyLock = 1.0f;
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
		
		
		private static float interestPeakArousal = 0.0f;
		private static float interestPeakArousalTimer = 0.0f;
		private static float interestPeakValence = 0.0f;
		private static float interestPeakValenceTimer = 0.0f;
	
		
		
		private static float twistActual = 0.0f;
		private static float twistTarget = 0.0f;
		private static float twistSpeed = 0.03f;
		private static float lElbowActual = 0.0f;
		private static float lElbowTarget = 0.0f;
		private static float rElbowActual = 0.0f;
		private static float rElbowTarget = 0.0f;
		
		private static float lShoulderX = 0.0f;
		private static float lShoulderY = 0.0f;
		private static float rShoulderX = 0.0f;
		private static float rShoulderY = 0.0f;
		private static float lElbowX = 0.0f;
		private static float lElbowY = 0.0f;
		private static float rElbowX = 0.0f;
		private static float rElbowY = 0.0f;
		
		
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
		protected JSONStorableBool uiGazeAvoid;
		protected JSONStorableFloat uiGazeLookTime;
		protected JSONStorableFloat uiGazeAvoidTime;
		protected JSONStorableBool uiGazeGlance;
		protected JSONStorableFloat uiRollSpeed;
		protected JSONStorableFloat uiSaccadeSpeed;
		protected JSONStorableFloat uiSaccadeAmount;
		protected JSONStorableFloat uiSaccadeWanderMult;
		protected JSONStorableFloat uiBlinkSpeed;
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
		protected JSONStorableFloat uiIdleArmAmount;
		protected JSONStorableFloat uiIdleArmOffset;
		protected JSONStorableFloat uiIdleChance;
		protected JSONStorableFloat uiIdleSpeed;
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
		protected JSONStorableFloat uiDirectLookDelay;
		protected JSONStorableBool uiEffectMaterial;
		protected JSONStorableFloat uiMaterialMult;
		protected JSONStorableFloat uiMouthOpenOffset;
		protected JSONStorableFloat uiLipsCloseOffset;
		protected JSONStorableFloat uiHeadAngleOffset;
		
		

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
        private static float gHeadRollTarget = 0.0f;
		private static float gHeadRollTimer = 0.0f;
        private static float gAvoid = 0.0f;
        private static float gAvoidHeight = 0.0f;
        private static float gAvoidanceClock = 0.0f;
		private static float gAvoidingClock = 0.0f;
		private static string gAvoidInterest = "";
        private static float sexActionNeckX = 0.0f;

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
		private static Vector3 headPrevPos;
        private static FreeControllerV3 neckController;
        private static FreeControllerV3 chestController;
        private static FreeControllerV3 lBreastController;
        private static FreeControllerV3 rBreastController;
        private static FreeControllerV3 pelvisController;
        private static FreeControllerV3 pelvis2Controller;
        private static FreeControllerV3 abdomenController;
        private static FreeControllerV3 lHandController;
        private static FreeControllerV3 rHandController;
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
        private static FreeControllerV3 rFootController;
		private static float chestControllerYAngle = 0.0f;

        private static bool lookAction = false;
        private static float lookVariation = 1.0f;
        private static float browVariation = 1.0f;
        private static float eyeVariation = 1.0f;
        private static float mouthVariation = 1.0f;
        private static float saccadeAmount = 0.0f;
		private static float shoulderUp = 0.0f;
		private static float saccadeOffsetCounter = 0.0f;
		private static float rollChance = 0.0f;
		
		
		private static Vector3 focusPos;
		private static Quaternion focusRot;

		
		protected Rigidbody lipTrigger;
		private static float lipsTouchCount = 0.0f;
		protected Rigidbody vagTrigger;
		private static float vagTouchCount = 0.0f;
		private static bool lipsOnly = false;
		
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

        private static DAZMorph morphBreastDroopLeft;
        private static DAZMorph morphBreastDroopRight;
        private static DAZMorph morphBreastHangLeft;
        private static DAZMorph morphBreastHangRight;

        private static DAZMorph morphRibCageSize;
        private static float mRibCageSizeOrig = 0.0f;
        private static float mRibCageSizeValue = 0.0f;
        private static float mRibCageSizeTarget = 0.0f;
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
		private static float kissingAmount = 1.0f;


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
        private static float interestPelvis;
        private static float interestTip;
        private static float interestFaceBase = 60.0f;
        private static float interestLHandBase = 45.0f;
        private static float interestRHandBase = 45.0f;
        private static float interestPelvisBase = 20.0f;
        private static float interestTipBase = 30.0f;
		private static float interestEMTargetBase = 20.0f;

        private static float randomInterest;

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
            pluginLabelJSON.val = "EasyMotion Lite (E-Motion face)";

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

            movementMaxTimeout = 15.0f;
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
			headPrevPos = new Vector3(0.0f,0.0f,0.0f);

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
			
			/*Object[] allObj = GameObject.FindObjectsOfType(typeof(MonoBehaviour));
			string name = "";
			foreach(Object go in allObj)
			{
				name = go.name;
				if (name.Contains("Arm"))// && name.Contains("arm"))
				{
				//SuperController.LogError(go.name);
				}
			}*/
			
			/*
			GameObject collider = GameObject.Find("AutoColliderFemaleAutoColliderschest6");
			if (collider != null)
			{
				//SuperController.LogError("Collider Found");
				AutoCollider ac = collider.GetComponent<AutoCollider>();
				ac.autoRadiusBuffer = 0.06f;
				ac.autoLengthBuffer = 0.02f;
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
				ac.autoRadiusBuffer = 0.05f;
				ac.autoLengthBuffer = 0.06f;
			}

			collider = GameObject.Find("AutoColliderFemaleAutoColliderschest3");
			if (collider != null)
			{
				//SuperController.LogError("Collider Found");
				AutoCollider ac = collider.GetComponent<AutoCollider>();
				ac.autoRadiusBuffer = 0.00f;
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
				headController.RBMass = 10.0f;
                headActual = person.GetStorableByID("head") as FreeControllerV3;
                eyeController = person.GetStorableByID("eyeTargetControl") as FreeControllerV3;
				eyeController.hidden = true;
                neckController = person.GetStorableByID("neckControl") as FreeControllerV3;
                chestController = person.GetStorableByID("chestControl") as FreeControllerV3;
				chestControllerYAngle = chestController.transform.eulerAngles.y;
				chestController.RBMass = 50.0f;
                lBreastController = person.GetStorableByID("lNippleControl") as FreeControllerV3;
				lBreastController.RBMass = 0.5f;
                rBreastController = person.GetStorableByID("rNippleControl") as FreeControllerV3;
				rBreastController.RBMass = 0.5f;
                pelvisController = person.GetStorableByID("hipControl") as FreeControllerV3;
                pelvis2Controller = person.GetStorableByID("pelvisControl") as FreeControllerV3;
                abdomenController = person.GetStorableByID("abdomen2Control") as FreeControllerV3;
				//pelvisController.RBMass = 600.0f;
                lHandController = person.GetStorableByID("lHandControl") as FreeControllerV3;
				lHandController.RBMass = 2.0f;
                rHandController = person.GetStorableByID("rHandControl") as FreeControllerV3;
				lHandController.RBMass = 2.0f;
                lShoulderController = person.GetStorableByID("lShoulderControl") as FreeControllerV3;
                rShoulderController = person.GetStorableByID("rShoulderControl") as FreeControllerV3;
                lArmController = person.GetStorableByID("lArmControl") as FreeControllerV3;
                rArmController = person.GetStorableByID("rArmControl") as FreeControllerV3;
                lElbowController = person.GetStorableByID("lElbowControl") as FreeControllerV3;
				lElbowController.RBMass = 5.0f;
                rElbowController = person.GetStorableByID("rElbowControl") as FreeControllerV3;
				rElbowController.RBMass = 5.0f;
                lFootController = person.GetStorableByID("lFootControl") as FreeControllerV3;
                rFootController = person.GetStorableByID("rFootControl") as FreeControllerV3;
                lThighController = person.GetStorableByID("lThighControl") as FreeControllerV3;
                rThighController = person.GetStorableByID("rThighControl") as FreeControllerV3;
                lKneeController = person.GetStorableByID("lKneeControl") as FreeControllerV3;
				lKneeController.RBMass = 2.0f;
                rKneeController = person.GetStorableByID("rKneeControl") as FreeControllerV3;
				rKneeController.RBMass = 2.0f;
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
                    morphExpSmileFullFace = morphUI.GetMorphByDisplayName("Smile Full Face");
                    morphExpSmileOpenFullFace = morphUI.GetMorphByDisplayName("Smile Open Full Face");
                    morphExpGlare = morphUI.GetMorphByDisplayName("Glare");
                    morphExpExcitement = morphUI.GetMorphByDisplayName("Concentrate");
                    morphExpHappy = morphUI.GetMorphByDisplayName("Huge Smile");
					if (morphExpHappy == null)
					{
						morphExpHappy = morphUI.GetMorphByDisplayName("Happy");
					}
                    morphExpFlirting = morphUI.GetMorphByDisplayName("Flirting");
                    morphExpDeserveIt = morphUI.GetMorphByDisplayName("AAsex_sqntwrry1sm6b");
					if (morphExpDeserveIt == null)
					{
						morphExpDeserveIt = morphUI.GetMorphByDisplayName("Deserving It");
					}
                    morphExpTakingIt = morphUI.GetMorphByDisplayName("AAsex_wideFF5");
					if (morphExpTakingIt == null)
					{
						morphExpTakingIt = morphUI.GetMorphByDisplayName("Taking It");
					}
                    morphMouthMouthOpen = morphUI.GetMorphByDisplayName("Mouth Open");
                    morphMouthMouthOpenWide = morphUI.GetMorphByDisplayName("AAsex_closetightsm1e");
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
					morphLipsLipBite = morphUI.GetMorphByDisplayName("AAsex_wideFF8");
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
                    morphMouthSmileMuscle = morphUI.GetMorphByDisplayName("SmileMuscles");
                    morphVisF = morphUI.GetMorphByDisplayName("F");
                    morphVisM = morphUI.GetMorphByDisplayName("M");
                    morphVisOW = morphUI.GetMorphByDisplayName("OW");
                    morphVisAA = morphUI.GetMorphByDisplayName("EH");

                    morphTongueInOut = morphUI.GetMorphByDisplayName("Tongue In-Out");
                    morphTongueSideSide = morphUI.GetMorphByDisplayName("Tongue Side-Side");
                    morphTongueBendTip = morphUI.GetMorphByDisplayName("Tongue Curl");
                    morphTongueLength = morphUI.GetMorphByDisplayName("Tongue Length");

                    morphRibCageSize = morphUI.GetMorphByDisplayName("Ribcage Size");
                    morphChestHeight = morphUI.GetMorphByDisplayName("Chest Height");
					if (morphChestHeight == null)
					{
						morphChestHeight = morphUI.GetMorphByDisplayName("Costal Angle Arched");
						mChestHeightOrig = -0.2f;//morphChestHeight.morphValue;
						personIsMale = true;
					}
					else
					{
						mChestHeightOrig = 0.0f;//morphChestHeight.morphValue;
					}
                    morphBreastHeight = morphUI.GetMorphByDisplayName("Breast Height");
					if (morphBreastHeight == null)
					{
						morphBreastHeight = morphUI.GetMorphByDisplayName("Sternum Depth");
						personIsMale = true;
					}
					morphBreastDroopLeft = morphUI.GetMorphByDisplayName("Breast droop left");
					morphBreastDroopRight = morphUI.GetMorphByDisplayName("Breast droop right");
					morphBreastHangLeft = morphUI.GetMorphByDisplayName("Breasts Hang Forward Left");
					morphBreastHangRight = morphUI.GetMorphByDisplayName("Breasts Hang Forward Right");
					morphBreath = morphUI.GetMorphByDisplayName("Breath1");
                    //morphRibsDef = morphUI.GetMorphByDisplayName("Ribs Definition");
                    morphSternumDepth = morphUI.GetMorphByDisplayName("Sternum Depth");
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
            person2 = SuperController.singleton.GetAtomByUid(uiFocusTarget.val);
            if (person2 != null)
            {
				person2IsMale = false;
				
				if (person2.gameObject.name == "Genesis2Male")//morphTemp == null)
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
				//SuperController.LogError("No Person2");
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
			ApplyFaceOnlyExpressionDefaultsIfNeeded();
			CreatePersonalityUI();
			CreateTargetUI();
        }

		private void ApplyFaceOnlyExpressionDefaultsIfNeeded()
		{
			if (!IsFaceOnlyExpressionMode)
			{
				return;
			}
			uiDoHead.val = false;
			uiConfigHead.val = false;
			uiDoChest.val = false;
			uiDoShoulders.val = false;
			uiIdleAmount.val = 0.0f;
			uiIdleArmAmount.val = 0.0f;
			uiDoHands.val = false;
			uiTargetLook.val = false;
			uiDoMorphs.val = true;
		}

                     
        public void FixedUpdate()
        {
			bool testRun = false;
			if (testRun)
			{
				if (lookAction == false)
				{
					lookSM.Switch(lSex);
				}
			}
			
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
			//SuperController.singleton.ClearErrors();
			//player = CameraTarget.centerTarget.transform;
			gHeadSpeed = 1.75f;
			usePerson2 = uiUsePerson2.val;
			if (!IsFaceOnlyExpressionMode)
			{
				oldEyePos = eyeController.transform.position;
			}
			personalSpaceDistance = uiPersonalSpace.val;//Mathf.Min(uiPersonalSpace.val,Mathf.Max(playerHeadToHead + 0.5f, closeFaceDistance*3.0f));
			backgroundDistance = personalSpaceDistance * 1.5f;
            lookDirectAngle = Mathf.Lerp(uiDirectGaze.val / 2.0f, uiDirectGaze.val * 2.0f,Mathf.Clamp(playerHeadToHead-kissingDistance,0.0f,personalSpaceDistance) / personalSpaceDistance);// * (playerHeadToHead / personalSpaceDistance);
            lookPeripheralAngle = uiPeripheralGaze.val;
            lookNoAwarenessAngle = uiOutOfGaze.val;
			eyesNonDirectAngle = lookDirectAngle / 2.0f;
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
			//energyAmount = 0.0f;
			

			Vector3 headForwardOnPelvis = Vector3.ProjectOnPlane(headController.followWhenOff.forward, pelvisController.followWhenOff.up);
			headLeftRight = Vector3.SignedAngle(pelvisController.followWhenOff.forward, headForwardOnPelvis, pelvisController.followWhenOff.up);
			Vector3 headUpOnChest = Vector3.ProjectOnPlane(headController.followWhenOff.forward, -chestController.followWhenOff.right);
			headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headUpOnChest, -chestController.followWhenOff.right);
						
			
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
						if (m.name == "female7" && tempFloat == 0.0f)
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
						if (m.name == "female7" && tempFloat == 0.0f)
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
			
			if (interestArousal > 7.0f)
			{
				heatupValue += Time.fixedDeltaTime / 15.0f;
				interestPeakArousalTimer += Time.fixedDeltaTime;
				if (interestPeakArousalTimer > 1.0f)
				{
					interestPeakArousal = Mathf.Min(interestPeakArousal + 1.0f,10.0f);
					interestPeakArousalTimer = 0.0f;
				}
			}
			else
			{
				heatupValue -= Time.fixedDeltaTime / 20.0f;
				interestPeakArousalTimer += Time.fixedDeltaTime;
				if (interestPeakArousalTimer > 1.0f)
				{
					interestPeakArousal = Mathf.Max(interestPeakArousal - 1.0f,1.0f);
					interestPeakArousalTimer = 0.0f;
				}
			}
			heatupValue = Mathf.Clamp(heatupValue, 0.0f, 10.0f);
			if (interestValence > 8.0f)
			{
				interestPeakValenceTimer += Time.fixedDeltaTime;
				if (interestPeakValenceTimer > 1.0f)
				{
					interestPeakValence = Mathf.Min(interestPeakValence + 1.0f,10.0f);
					interestPeakValenceTimer = 0.0f;
				}
			}
			else
			{
				interestPeakValenceTimer += Time.fixedDeltaTime;
				if (interestPeakValenceTimer > 2.0f)
				{
					interestPeakValence = Mathf.Max(interestPeakValence - 1.0f,1.0f);
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
						if (m.name == "female7" && tempFloat == 0.0f)
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



			
			if (!IsFaceOnlyExpressionMode)
			{
			if (blinkRepTimer > 4.0f * uiBlinkSpeed.val)
			{
				blinkRepeat = Mathf.Clamp(blinkRepeat - 1.0f,0.0f,100.0f);
				blinkRepTimer = 0.0f;
			}
			
			if (twistActual < twistTarget)
			{
				twistActual = Mathf.Clamp(twistActual + twistSpeed, twistActual, twistTarget);
			}
			if (twistActual > twistTarget)
			{
				twistActual = Mathf.Clamp(twistActual - twistSpeed, twistTarget, twistActual);
			}

			if (headLastLeftRightActual < headLastLeftRight)
			{
				headLastLeftRightActual = Mathf.Clamp(headLastLeftRightActual + twistSpeed, headLastLeftRightActual, headLastLeftRight);
			}
			if (headLastLeftRightActual > headLastLeftRight)
			{
				headLastLeftRightActual = Mathf.Clamp(headLastLeftRightActual - twistSpeed, headLastLeftRight, headLastLeftRightActual);
			}

			tempFloat = uiIdleSpeed.val / 5.0f;
			if (lElbowActual < lElbowTarget)
			{
				lElbowActual = Mathf.Clamp(lElbowActual + tempFloat, lElbowActual, lElbowTarget);
			}
			if (lElbowActual > lElbowTarget)
			{
				lElbowActual = Mathf.Clamp(lElbowActual - tempFloat, lElbowTarget, lElbowActual);
			}
			
			if (rElbowActual < rElbowTarget)
			{
				rElbowActual = Mathf.Clamp(rElbowActual + tempFloat, rElbowActual, rElbowTarget);
			}
			if (rElbowActual > rElbowTarget)
			{
				rElbowActual = Mathf.Clamp(rElbowActual - tempFloat, rElbowTarget, rElbowActual);
			}
			
			}
			
			eyeCloseMaxMorph = uiEyeCloseMaxMorph.val;
			
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
			if (currentAtom != person2 && currentAtom != null)
			{
				person2 = currentAtom;
				systemSM.Switch(sReselectPerson2);
			}

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
			
			
			if (person != null && !IsFaceOnlyExpressionMode)
			{
				if (uiConfigHead.val && uiDoHead.val)
				{
					//SuperController.LogError("Config Head");
					//headController.currentPositionState = FreeControllerV3.PositionState.Off;
					personEyes.SetStringChooserParamValue("lookMode", "Target");

					headController.currentRotationState = FreeControllerV3.RotationState.On;
					headController.jointRotationDriveSpring = 3;
					headController.jointRotationDriveDamper = 2.9f;
					headController.jointRotationDriveXTarget = 0.0f;
					headController.jointRotationDriveYTarget = 0.0f;
					headController.jointRotationDriveZTarget = 0.0f;
					headController.RBHoldRotationSpring = Mathf.Lerp(25,35,interestValence/10.0f);
					if (currentLook == "Kissing" || currentLook == "Sucking")
					{
						headController.RBHoldRotationSpring = 80;
					}
					headController.RBHoldRotationDamper = Mathf.Lerp(8,3,interestArousal/10.0f);
					if (morphTongueLength != null)
					{
						morphTongueLength.SetValue(0.08f);
					}

					//SuperController.LogError("Config Neck");
					//neckController.currentPositionState = FreeControllerV3.PositionState.Off;
					neckController.currentRotationState = FreeControllerV3.RotationState.On;
					neckController.jointRotationDriveSpring = 70;
					neckController.jointRotationDriveDamper = 40;
					
					neckController.jointRotationDriveXTarget = Mathf.Lerp(10.0f,-20.0f,Mathf.Clamp(playerHeadToHead, 0.0f, personalSpaceDistance)/personalSpaceDistance);
					neckController.jointRotationDriveYTarget = 0.0f;
					neckController.jointRotationDriveZTarget = 0.0f;
					if (currentLook == "Sucking")
					{
						neckController.RBHoldRotationSpring = Mathf.Lerp(65,120,interestValence/10.0f);
						neckController.jointRotationDriveSpring = 60;
						neckController.jointRotationDriveDamper = 40;
						neckController.jointRotationDriveXTarget = Mathf.SmoothStep(-40.0f,00.0f, breathClock);
					}
					else
					{
						neckController.RBHoldRotationSpring = Mathf.Lerp(25,80,interestValence/10.0f);
						if (mainInterest == "Face" && playerHeadToHead < kissingDistance+0.15f)// && lipsTouchCount > 0.0f)
						{
							neckController.jointRotationDriveSpring = 60;
							if (playerHeadToHead > kissingDistance - 0.03f && playerHeadToHead < kissingDistance + 0.05f)
							{
								kissingAngle = Mathf.Clamp(kissingAngle - 0.05f,-40.0f,00.0f);
							}
							else
							{
								kissingAngle = Mathf.Clamp(kissingAngle + 0.1f,-40.0f,00.0f);
							}
							neckController.jointRotationDriveXTarget = kissingAngle;
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
							neckController.jointRotationDriveXTarget = kissingAngle;
						}
					}
					neckController.RBHoldRotationDamper = Mathf.Lerp(35,85,interestArousal/10.0f);
				}
				
				if (uiDoChest.val)
				{
					//SuperController.LogError("Do Chest");
					chestController.jointRotationDriveSpring = 100;
					chestController.jointRotationDriveDamper = 235;
					chestController.jointRotationDriveXTarget = (Mathf.SmoothStep(-10.0f,Mathf.Lerp(10.0f,20.0f,interestArousal/10.0f), breathClock) * uiBreatheExpandMultiplier.val) * uiChestAmount.val;
					if (currentLook == "Sucking")
					{
						chestController.jointRotationDriveXTarget = 20.0f - (40.0f * breathClock);
					}
					if (uiIdleAmount.val > 0.0f)
					{
						chestController.transform.eulerAngles = new Vector3(chestController.transform.eulerAngles.x, pelvisController.transform.eulerAngles.y + ((headLeftRight / 4.0f) * uiIdleAmount.val), chestController.transform.eulerAngles.z);
					}
					chestController.jointRotationDriveYTarget = (twistActual / 2.7f) * uiIdleAmount.val;
					chestController.jointRotationDriveZTarget = (gHeadRoll * 4.0f) * uiIdleAmount.val;
					abdomenController.jointRotationDriveYTarget = (twistActual / 2.0f) * uiIdleAmount.val;
					abdomenController.jointRotationDriveZTarget = (gHeadRoll * 1.0f) * uiIdleAmount.val;
				}
				
				if (uiDoShoulders.val)
				{
					//SuperController.LogError("Do Shoulders");
					//lShoulderController.RBHoldRotationSpring = 10;
					//lShoulderController.RBHoldRotationDamper = 1;
					tempFloat = 25.0f - (45.0f * uiShoulderHeight.val) + (((5.0f * shoulderUp) + Mathf.Lerp(0.0f,10.0f,interestArousal/10.0f) + (breathClock * Mathf.Lerp(0.05f * uiBreatheExpandMultiplier.val, 0.5f * uiBreatheExpandMultiplier.val,interestArousal/10.0f))) * uiShoulderAmount.val);
					tempFloat2 = (-1.0f + (5.0f * (breathClock * Mathf.Lerp(0.5f, 1.0f,interestArousal/10.0f))) + (pExtraversion/20.0f)) * uiShoulderAmount.val;
					tempFloat2 = (tempFloat2 / 2.0f) + (twistActual / 2.0f);
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
					lShoulderController.jointRotationDriveZTarget = Mathf.Lerp(-10.0f, 10.0f,interestArousal/10.0f) * uiShoulderAmount.val;//((-6.0f + (breathClock * 16.0f)) * uiBreatheExpandMultiplier.val) * uiShoulderAmount.val;//headController.followWhenOff.eulerAngles.y;
					//lShoulderController.jointRotationDriveZTarget += -sexActionNeckX * 2.0f;
					//rShoulderController.RBHoldRotationSpring = 10;
					//rShoulderController.RBHoldRotationDamper = 1;
					rShoulderController.jointRotationDriveSpring = Mathf.Lerp(70.0f,120.0f,((interestArousal + interestValence)/2.0f) / 10.0f);
					rShoulderController.jointRotationDriveDamper = Mathf.Lerp(45.0f,20.0f,((interestArousal + interestValence)/2.0f) / 10.0f);
					rShoulderController.jointRotationDriveXTarget = -tempFloat;
					rShoulderController.jointRotationDriveYTarget = -tempFloat2; //(neckController.followWhenOff.eulerAngles.y + Mathf.Lerp(10.0f,20.0f,interestValence/10.0f) + (breathClock * Mathf.Lerp(1.0f * uiBreatheExpandMultiplier.val, 2.5f * uiBreatheExpandMultiplier.val,interestArousal/10.0f))) * uiShoulderAmount.val;
					//rShoulderController.jointRotationDriveYTarget += -sexActionNeckX * 2.0f;
					rShoulderController.jointRotationDriveZTarget = Mathf.Lerp(-10.0f, 10.0f,interestArousal/10.0f) * uiShoulderAmount.val;//((-6.0f + (breathClock * 16.0f)) * uiBreatheExpandMultiplier.val) * uiShoulderAmount.val;
					//rShoulderController.jointRotationDriveZTarget += -sexActionNeckX * 2.0f;
				}

				if (uiIdleArmAmount.val > 0.0f)
				{
					lElbowController.jointRotationDriveSpring = 120.0f;
					lElbowController.jointRotationDriveXTarget = lElbowActual * uiIdleArmAmount.val * (interestValence / 10.0f);
					lArmController.jointRotationDriveSpring = 120.0f;
					lArmController.jointRotationDriveYTarget = ((lElbowActual + 50.0f) * uiIdleArmAmount.val * (interestArousal / 10.0f)) + (headLastLeftRightActual/1.0f); //-twistActual;
					lArmController.jointRotationDriveZTarget = ((interestArousal - 5.0f) * 5.0f * uiIdleArmAmount.val) + uiIdleArmOffset.val;
					lArmController.jointRotationDriveXTarget = -50.0f - (headLastLeftRightActual/3.0f);//Mathf.Lerp(-55.0f, -70.0f, lElbowActual / 120.0f);//Mathf.Clamp(-30.0f + Mathf.Abs(lElbowActual) * uiIdleAmount.val,-70.0f, -35.0f);
					
					lHandController.jointRotationDriveSpring = 15.0f;
					lHandController.jointRotationDriveDamper = 4.5f;
					lHandController.jointRotationDriveXTarget = (lElbowActual * 0.3f) * uiIdleArmAmount.val;
					lHandController.jointRotationDriveZTarget = (lElbowActual + 80.0f) * uiIdleArmAmount.val;
					lHandController.jointRotationDriveYTarget = -(lElbowActual + 50.0f) * uiIdleArmAmount.val;
					
					rElbowController.jointRotationDriveSpring = 120.0f;
					rElbowController.jointRotationDriveXTarget = rElbowActual * uiIdleArmAmount.val * (interestValence / 10.0f);
					rArmController.jointRotationDriveSpring = 120.0f;
					rArmController.jointRotationDriveYTarget = ((rElbowActual - 50.0f) * uiIdleArmAmount.val * (interestArousal / 10.0f)) + (headLastLeftRightActual/1.0f); //twistActual;
					rArmController.jointRotationDriveZTarget = ((interestArousal - 5.0f) * 5.0f * uiIdleArmAmount.val) + uiIdleArmOffset.val;
					rArmController.jointRotationDriveXTarget = 50.0f - (headLastLeftRightActual/3.0f);//Mathf.Lerp(55.0f, 70.0f, rElbowActual / 120.0f);//Mathf.Clamp(30.0f - Mathf.Abs(rElbowActual) * uiIdleAmount.val,70.0f, 35.0f);
					
					rHandController.jointRotationDriveSpring = 15.0f;
					rHandController.jointRotationDriveDamper = 4.5f;
					rHandController.jointRotationDriveXTarget = (rElbowActual * 0.3f) * uiIdleArmAmount.val;
					rHandController.jointRotationDriveZTarget = (rElbowActual - 80.0f) * uiIdleArmAmount.val;
					rHandController.jointRotationDriveYTarget = -(rElbowActual - 50.0f) * uiIdleArmAmount.val;
				}
				else
				{
					lElbowController.jointRotationDriveSpring = 5.0f;
					lElbowController.jointRotationDriveDamper = 3.5f;
					lArmController.jointRotationDriveSpring = 5.0f;
					lArmController.jointRotationDriveDamper = 3.5f;
					rElbowController.jointRotationDriveSpring = 5.0f;
					rElbowController.jointRotationDriveDamper = 3.5f;
					rArmController.jointRotationDriveSpring = 5.0f;
					rArmController.jointRotationDriveDamper = 3.5f;
				}
				
				if (uiIdleAmount.val > 0.0f)
				{
				
					pelvis2Controller.jointRotationDriveSpring = 175;
					pelvis2Controller.jointRotationDriveDamper = 55;
					pelvis2Controller.jointRotationDriveYTarget = (-twistActual * uiIdleAmount.val) / 4.6f;
					pelvis2Controller.jointRotationDriveZTarget = (twistActual * uiIdleAmount.val) / 7.0f;
					
					//chestController.jointRotationDriveXTarget = twistActual * 2.0f * uiIdleAmount.val;
					chestController.jointRotationDriveZTarget = -gHeadRoll * 0.5f * uiIdleAmount.val;

					abdomenController.jointRotationDriveSpring = 275;
					abdomenController.jointRotationDriveDamper = 135;
					abdomenController.jointRotationDriveYTarget = (twistActual / 10.0f) * uiIdleAmount.val * (interestValence / 10.0f);
					abdomenController.jointRotationDriveZTarget = gHeadRoll * 0.2f * uiIdleAmount.val * (interestValence / 10.0f);
					if (vagTouchCount > 0.0f)
					{
					chestController.jointRotationDriveSpring = 400;
					abdomenController.jointRotationDriveSpring = 475;
					chestController.jointRotationDriveYTarget = (sexActionNeckX * 5.0f) + (Mathf.Max(interestArousal - 8.0f) * 5.0f);
					abdomenController.jointRotationDriveYTarget = -sexActionNeckX * 5.0f;
					}
					
					lKneeController.jointRotationDriveXTarget = 0.0f - (rElbowActual * 1.50f) * uiIdleAmount.val * (interestArousal / 10.0f);//Mathf.Clamp(rElbowActual * -1.5f, -20.0f, -150.0f);
					rKneeController.jointRotationDriveXTarget = lElbowActual * 1.50f * uiIdleAmount.val * (interestArousal / 10.0f);//Mathf.Clamp(lElbowActual * 1.5f, -20.0f, -150.0f);

					lThighController.jointRotationDriveXTarget = -gHeadRoll * 3.0f * uiIdleAmount.val * (interestArousal / 10.0f);
					rThighController.jointRotationDriveXTarget = gHeadRoll * 3.0f * uiIdleAmount.val * (interestArousal / 10.0f);
				}
				
				if (morphBreastDroopLeft != null && morphBreastDroopRight != null)
				{
					float diff = (chestController.followWhenOff.position.y - lElbowController.followWhenOff.position.y) * 1.5f;
					//float tempval = Mathf.Lerp(0.0f,-0.25f,Mathf.Clamp(diff, 0.0f, 1.0f));
					//testString = Mathf.Clamp(diff,-0.25f, 0.0f).ToString();
					tempFloat = -0.7f;
					morphBreastDroopLeft.SetValue(Round(Mathf.Clamp(diff,tempFloat, 0.0f)));
					morphBreastHangLeft.SetValue(Round(Mathf.Clamp(diff,tempFloat, 0.0f)));
					diff = (chestController.followWhenOff.position.y - rElbowController.followWhenOff.position.y) * 1.5f;
					testString = diff.ToString();
					//tempval = Mathf.Lerp(0.0f,-0.25f,Mathf.Clamp(diff, 0.0f, 1.0f));
					morphBreastDroopRight.SetValue(Round(Mathf.Clamp(diff,tempFloat, 0.0f)));
					morphBreastHangRight.SetValue(Round(Mathf.Clamp(diff,tempFloat, 0.0f)));
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
            lookSM.OnUpdate();
            systemSM.OnUpdate();
            browSM.OnUpdate();
            mouthSM.OnUpdate();
			if (!IsFaceOnlyExpressionMode)
			{
				eyesSM.OnUpdate();
			}
			//SuperController.LogError("State Machines Updated");
            string dbgHead = "";
            string dbgLHand = "";
            string dbgRHand = "";
            string dbgPenis = "";
            string dbgObject = "";
			
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
			morphLBicepFlex.SetValue(Round(Mathf.Lerp(0.8f,0.0f,(tempFloat - 270.0f) / 90.0f)));
			}
			else
			{
			morphLBicepFlex.SetValue(0);
			}
			
			tempFloat = rElbowController.followWhenOff.localEulerAngles.x;
			tempFloat2 = rElbowController.followWhenOff.localEulerAngles.y;
			if (tempFloat > 270.0f && tempFloat2 < 250.0f)
			{
			morphRBicepFlex.SetValue(Round(Mathf.Lerp(0.8f,0.0f,(tempFloat - 270.0f) / 90.0f)));
			}
			else
			{
			morphRBicepFlex.SetValue(0);
			}*/

			
            interestArousal = Mathf.Clamp(interestArousal, 2.0f, 10.0f);
            interestValence = Mathf.Clamp(interestValence, 1.0f, 10.0f);
			if (currentLook == "Kissing" || currentLook == "Sucking" || currentLook == "Sex")
			{
				interestArousal += 0.1f;
				interestValence += 0.05f;
			}

			if (playerHeadToHead < personalSpaceDistance || playerLHandToHead < personalSpaceDistance || playerRHandToHead < personalSpaceDistance)
			{
				interestValence = Mathf.Clamp(interestValence, 3.5f, 10.0f);
			}
			
			tempFloat = -0.5f;
			if (!IsFaceOnlyExpressionMode)
			{
			if (playerHeadToFaceRot < eyesNonDirectAngle && mainInterest == "Face" && Vector3.Distance(new Vector3(0.0f,0.0f,0.0f),saccadeOffset) < 25.0f && mEyesClosedLeftValue <= 0.7f && amGlancing == false && gAvoid == 0.0f && playerHeadToHead < personalSpaceDistance && saccadeOffset.y > -10.0f)
			{
				mEyesPupilsTarget =  tempFloat + (Mathf.Lerp(0.2f,0.5f,interestArousal/10.0f) * uiPupilDialation.val); //mEyesPupilsOrig +
			}
			else
			{
				mEyesPupilsTarget = tempFloat;
			}
			if (currentEye == "Closed" && mEyesClosedLeftValue > 0.8f)
			{
				mEyesPupilsTarget = 0.8f * uiPupilDialation.val;
			}
			}
			
			
            breatheInSpeed = Mathf.Lerp(1.0f, 1.7f, interestArousal/10.0f) * uiBreatheSpeed.val;
            breatheOutSpeed = breatheInSpeed * Mathf.Lerp(0.5f,0.8f,interestArousal/10.0f);
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
				headAudio.SetFloatParamValue("minDistance", 0.01f * uiSoundVolume.val);
				headAudio.CallAction("PlayNow", audioClip);
				lastRandomVoice = tempRandom;
			}
            float breathingRate = (Random.Range(0.1f, 0.35f) + (interestArousal / 15.0f)) * uiBreatheSpeed.val;
            if (breathState == "in" && breathHold <= 0.0f && (lipsTouchCount < 1.0f || (lipsTouchCount > 0.0f && playerTipToHead > 0.077f)))
            {
                breathClock = Mathf.Min(breathClock + (Time.fixedDeltaTime * (breathingRate * breatheInSpeed)), 1.0f);
                if (breathClock == 1.0f && (((AudioSourceControl)headAudio).playingClip == null || voiceMoan == false))
                {
                    breathState = "out";
					breathHold = Mathf.Lerp(0.1f,0.03f,interestArousal/10.0f) * uiBreatheSpeed.val;
					if (uiDoSounds.val && interestKissing == false)
					{
						if ((mMouthOpenValue < 0.15f && mMouthOpenWideValue < 0.2f && mSmileOpenFullFaceValue < 0.3f) || currentMouth == "Demure" || currentMouth == "Pout" || currentMouth == "Sideways") 
						{
							voicePuckerAdjust = 0.0f;
							tempFloat = Mathf.Lerp(110.0f,40.0f,interestArousal/10.0f);
							if (lipsTouchCount > 0.0f || vagTouchCount > 0.0f || playerLHandInteract || playerRHandInteract)
							{
								tempFloat = tempFloat / 2.0f;
							}
							if ((interestArousal > 4.0f && Random.Range(0.0f,100.0f) > tempFloat) || currentLook == "Feel")
							{
								tempRandom = Mathf.Round(Random.Range(1.0f,16.0f));
								while (tempRandom == lastRandomVoice)
								{
									tempRandom = Mathf.Round(Random.Range(1.0f,16.0f));
								}
								audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_Mmm" + tempRandom + ".wav");
								voiceOpenAdjust = -0.25f;
								mVisMTarget = 0.35f;
								mLipsCloseTarget = 0.1f;
								headAudio.SetFloatParamValue("minDistance", 0.07f * uiSoundVolume.val);
								voiceMoan = true;
								if (Random.Range(0.0f,100.0f) > 50.0f && currentLook != "Feel" && currentLook != "Sex" && playerHeadToHead < personalSpaceDistance/2.0f && testRun == false)
								{
									lookSM.Switch(lFeel);
								}
								//energyAmount += 1.0f;
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
								headAudio.SetFloatParamValue("minDistance", 0.02f * uiSoundVolume.val);
								voiceMoan = false;
							}
						}
						else
						{
							tempFloat = Mathf.Lerp(110.0f,70.0f,interestArousal/10.0f);
							if (vagTouchCount > 0.0f || playerLHandInteract || playerRHandInteract)
							{
								tempFloat = tempFloat / 2.0f;
							}
							if ((interestArousal > 4.0f && Random.Range(0.0f,100.0f) > tempFloat) || currentLook == "Feel" || currentLook == "Sex")
							{
								if (Random.Range(0.0f,100.0f) > 50.0f)
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
									mVisOWTarget = 0.3f;
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
									mVisAATarget = 0.3f;
									if ((vagTouchCount > 0.0f && Random.Range(0.0f,100.0f) > 50.0f) || Random.Range(0.0f,100.0f) > 90.0f)
									{
										tempRandom = Mathf.Round(Random.Range(1.0f,8.0f));
										while (tempRandom == lastRandomVoice)
										{
											tempRandom = Mathf.Round(Random.Range(1.0f,8.0f));
										}
										audioClip = URLAudioClipManager.singleton.GetClip(@"Breath_Mouth_Yeah" + tempRandom + ".wav");
										voicePuckerAdjust = 0.2f;
										voiceOpenAdjust = 0.4f;
										mVisOWTarget = 0.3f;
										mVisAATarget = 0.5f;
									}
									//mVisFTarget = 0.5f;
								}
								headAudio.SetFloatParamValue("minDistance", 0.07f * uiSoundVolume.val);
								if (mMouthOpenTarget < 0.4f)
								{
									voiceOpenAdjust = 0.4f;
								}
								if (mMouthOpenTarget < 0.0f)
								{
									voiceOpenAdjust = 0.6f;
								}
								voiceMoan = true;
								if (Random.Range(0.0f,100.0f) > 50.0f && currentLook != "Feel" && currentLook != "Sex" && playerHeadToHead < personalSpaceDistance/2.0f && testRun == false)
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
						if ((mMouthOpenValue < 0.15f && mMouthOpenWideValue < 0.2f && mSmileOpenFullFaceValue < 0.15f) || currentMouth == "Demure" || currentMouth == "Pout" || currentMouth == "Sideways")
						{
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

			
			if (uiDoMorphs.val)
			{
				resetMorphs = false;
				if (IsFaceOnlyExpressionMode)
				{
					personEyelids.SetBoolParamValue("blinkEnabled", true);
				}
				else
				{
					personEyelids.SetBoolParamValue("blinkEnabled", false);
				}
				containingAtom.GetStorableByID("AutoExpressions").SetBoolParamValue("enabled", false);
				tempFloat = Mathf.Lerp(0.5f,1.0f,interestArousal/10.0f);
				if (!IsFaceOnlyExpressionMode)
				{
				if (morphRibCageSize != null)
				{
					morphRibCageSize.SetValue(Mathf.SmoothStep(0.0f, 0.23f * uiBreatheExpandMultiplier.val * tempFloat,breathClock));
				}
				if (morphChestHeight != null)
				{
					morphChestHeight.SetValue(Mathf.SmoothStep(mChestHeightOrig, mChestHeightOrig-(0.22f * uiBreatheRaiseMultiplier.val * tempFloat),breathClock));//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f));
				}
				if (morphBreastHeight != null)
				{
					if (personIsMale)
					{
						morphBreastHeight.SetValue(Mathf.SmoothStep(0.0f, 0.3f * uiBreatheRaiseMultiplier.val * tempFloat,breathClock));//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f));
					}
					else
					{
						morphBreastHeight.SetValue(Mathf.SmoothStep(0.0f, 0.5f * uiBreatheRaiseMultiplier.val * tempFloat,breathClock));//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f));
					}
				}
				if (morphBreath != null)
				{
					morphBreath.SetValue(Mathf.SmoothStep(0.0f, Mathf.Clamp(1.0f - (0.5f * uiBreatheExpandMultiplier.val * tempFloat),0.0f,1.0f),breathClock));//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f));
				}
				else
				{
					//morphRibsDef.SetValue(0.3f + (breathClock*0.7f) * 1.22f * uiBreatheExpandMultiplier.val * tempFloat);//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f));
				}
				//morphRibsDef.SetValue(0.3f + (breathClock*0.7f) * 1.22f * uiBreatheExpandMultiplier.val * tempFloat);//Mathf.Min((breathClock * -0.365f) + 0.08f, 0.0f));
				if (morphSternumDepth != null)
				{
					morphSternumDepth.SetValue(Mathf.SmoothStep(0.0f, 0.21f * uiBreatheExpandMultiplier.val * tempFloat,breathClock));
				}
				if (morphNipplesApply != null)
				{
					morphNipplesApply.SetValue(interestArousal/13.0f);
				}
				
				
				//SuperController.LogError("Breathing Morphs done");
				if (morphDeepBulgeBellyBottom != null && morphDeepBulgeBellyMid != null && usePerson2 && uiDoSex.val)
				{
					tempFloat = Vector3.Distance(pelvisController.followWhenOff.position, playerTipController.followWhenOff.position);
					if ( tempFloat < 0.15f)
					{
						morphDeepBulgeBellyBottom.SetValue(Mathf.Clamp((1.0f - (tempFloat*6.666f)) * uiSexAmount.val,0.0f,1.0f));
					}
					if ( tempFloat < 0.065f)
					{
						morphDeepBulgeBellyMid.SetValue(Mathf.Clamp((1.0f - (tempFloat*15.384f)) * uiSexAmount.val,0.0f,1.0f) * 0.75f);
					}
				}
				
				
				if (morphDeepThroat != null && usePerson2 && uiDoBlowjob.val)
				{
					tempFloat = Vector3.Distance(headController.followWhenOff.position, playerTipController.followWhenOff.position);
					if ( tempFloat < 0.077f)
					{
						morphDeepThroat.SetValue(Mathf.Clamp((1.0f - (tempFloat*12.987f)) * uiBlowjobAmount.val,0.0f,1.0f));
					}
				}
				
				if (morphBlowjobLips != null && morphCheekSink != null && usePerson2 && playerTipToHead < playerHeadToHead && playerTipToHead < personalSpaceDistance/2.0f)
				{
					if (lipsTouchCount > 0.0f && uiDoBlowjob.val)
					{
						if (Mathf.Abs(Vector3.Angle(playerTipController.followWhenOff.position, headController.followWhenOff.position)) < lookDirectAngle)
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
						}
						tempFloat = Mathf.Clamp(Vector3.Distance(playerTipController.followWhenOff.position,playerTipPrev) * 9.0f,0.0f,1.0f);
						tempFloat2 = 0.0f - Mathf.Clamp(Vector3.Distance(headController.followWhenOff.position,headPrevPos) * 9.0f,0.0f,1.0f);
						if (tempFloat2 > tempFloat)
						{
							tempFloat = tempFloat2;
						}
						if (Vector3.Distance(personHeadTransform.position, playerTipController.followWhenOff.position) >= Vector3.Distance(personHeadTransform.position, playerTipPrev))// && Mathf.Abs(tempFloat) > 0.002f)
						{
							mBlowjobLipsTarget = Mathf.Clamp(mBlowjobLipsTarget + (tempFloat * uiBlowjobAmount.val),-0.2f,1.0f);
							mCheekSinkTarget = Mathf.Clamp(mBlowjobLipsTarget + 0.3f,0.0f,1.0f);
							if (mEyesClosedLeftValue < 0.0f && morphBlinking == false)
							{
								mEyesClosedLeftTarget = Mathf.Clamp(mEyesClosedLeftTarget + 0.1f,-0.2f,0.0f);
								mEyesClosedRightTarget = Mathf.Clamp(mEyesClosedRightTarget + 0.1f,-0.2f,0.0f);
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
								mEyesClosedLeftTarget = Mathf.Clamp(mEyesClosedLeftTarget - 0.2f,-0.5f,1.0f);
								mEyesClosedRightTarget = Mathf.Clamp(mEyesClosedRightTarget - 0.2f,-0.5f,1.0f);
							}
						}
					}
					else
					{
						mMouthOpenWiderTarget = 0.0f;
						mBlowjobLipsTarget = Mathf.Clamp(mBlowjobLipsTarget - 0.1f,0.0f,1.0f);
						mCheekSinkTarget = Mathf.Clamp(mCheekSinkTarget - 0.1f,0.0f,1.0f);
					}
					if (mBlowjobLipsTarget > mBlowjobLipsValue + 0.01f) { mBlowjobLipsValue = Mathf.Min(mBlowjobLipsValue + 0.3f, mBlowjobLipsTarget); }
					if (mBlowjobLipsTarget < mBlowjobLipsValue - 0.01f) { mBlowjobLipsValue = Mathf.Max(mBlowjobLipsValue - 0.3f, mBlowjobLipsTarget); }
					morphBlowjobLips.SetValue(Mathf.Clamp(mBlowjobLipsValue,0.0f,0.8f));
					if (mCheekSinkTarget > mCheekSinkValue + 0.01f) { mCheekSinkValue = Mathf.Min(mCheekSinkValue + 0.2f, mCheekSinkTarget); }
					if (mCheekSinkTarget < mCheekSinkValue - 0.01f) { mCheekSinkValue = Mathf.Max(mCheekSinkValue - 0.1f, mCheekSinkTarget); }
					morphCheekSink.SetValue(Mathf.Clamp(mCheekSinkValue*2.0f,0.0f,1.0f));
					playerTipPrev = playerTipController.followWhenOff.position;
					headPrevPos = headController.followWhenOff.position;
				}
				//SuperController.LogError("Sex Morphs Done");
				
				//Morph Controller
				if (doHands)
				{
					if (mLHandStraightenTarget > mLHandStraightenValue) { mLHandStraightenValue = Mathf.Min(mLHandStraightenValue + (mLHandStraightenTarget - mLHandStraightenValue) / 15.0f, mLHandStraightenTarget); }
					if (mLHandStraightenTarget < mLHandStraightenValue) { mLHandStraightenValue = Mathf.Max(mLHandStraightenValue - (mLHandStraightenValue - mLHandStraightenTarget) / 15.0f, mLHandStraightenTarget); }
					morphLHandStraighten.SetValue(Mathf.Clamp(mLHandStraightenValue + (interestValence / 100.0f), 0.0f, 1.0f));
					if (mRHandStraightenTarget > mRHandStraightenValue) { mRHandStraightenValue = Mathf.Min(mRHandStraightenValue + (mRHandStraightenTarget - mRHandStraightenValue) / 15.0f, mRHandStraightenTarget); }
					if (mRHandStraightenTarget < mRHandStraightenValue) { mRHandStraightenValue = Mathf.Max(mRHandStraightenValue - (mRHandStraightenValue - mRHandStraightenTarget) / 15.0f, mRHandStraightenTarget); }
					morphRHandStraighten.SetValue(Mathf.Clamp(mRHandStraightenValue + (interestValence / 100.0f), 0.0f, 1.0f));
					if (mLHandFistTarget > mLHandFistValue) { mLHandFistValue = Mathf.Min(mLHandFistValue + (mLHandFistTarget - mLHandFistValue) / 15.0f, mLHandFistTarget); }
					if (mLHandFistTarget < mLHandFistValue) { mLHandFistValue = Mathf.Max(mLHandFistValue - (mLHandFistValue - mLHandFistTarget) / 15.0f, mLHandFistTarget); }
					morphLHandFist.SetValue(Mathf.Clamp(mLHandFistValue + (interestArousal / 100.0f), 0.0f, 1.2f));
					if (mRHandFistTarget > mRHandFistValue) { mRHandFistValue = Mathf.Min(mRHandFistValue + (mRHandFistTarget - mRHandFistValue) / 15.0f, mRHandFistTarget); }
					if (mRHandFistTarget < mRHandFistValue) { mRHandFistValue = Mathf.Max(mRHandFistValue - (mRHandFistValue - mRHandFistTarget) / 15.0f, mRHandFistTarget); }
					morphRHandFist.SetValue(Mathf.Clamp(mRHandFistValue + (interestArousal / 100.0f), 0.0f, 1.2f));
					//SuperController.LogError("Hand Morphs Done");
				}
				}

				tempFloat = -0.2f + (Mathf.Max(mSmileFullFaceValue,mSmileOpenFullFaceValue) + mTakingItValue) * 1.0f;
				//tempFloat = tempFloat / 2.0f;
				if (mBrowUpTarget + (interestValence / 30.0f) + tempFloat > mBrowUpValue + 0.2f) { mBrowUpValue = Mathf.Min(mBrowUpValue + ((Mathf.Abs(mBrowUpTarget - mBrowUpValue - tempFloat) / 25.0f) * browVariation * morphSpeed), mBrowUpTarget + (interestValence / 30.0f) + tempFloat); }
				if (mBrowUpTarget + (interestValence / 30.0f) + tempFloat < mBrowUpValue - 0.2f) { mBrowUpValue = Mathf.Max(mBrowUpValue - ((Mathf.Abs(mBrowUpTarget - mBrowUpValue - tempFloat) / 65.0f) * browVariation * morphSpeed), mBrowUpTarget + (interestValence / 30.0f) + tempFloat); }
				morphBrowUp.SetValue(Round(Mathf.Clamp(mBrowUpValue, 0.0f, 1.0f)));
				
				if (mBrowDownTarget > mBrowDownValue + 0.02f) { mBrowDownValue = Mathf.Min(mBrowDownValue + ((Mathf.Abs(mBrowDownTarget - mBrowDownValue) / 65.0f) * browVariation * morphSpeed), mBrowDownTarget); }
				if (mBrowDownTarget < mBrowDownValue - 0.02f) { mBrowDownValue = Mathf.Max(mBrowDownValue - ((Mathf.Abs(mBrowDownTarget - mBrowDownValue) / 25.0f) * browVariation * morphSpeed), mBrowDownTarget); }
				morphBrowDown.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(mBrowDownValue)));
				if (mExcitementTarget > mExcitementValue + 0.1f) { mExcitementValue = Mathf.Min(mExcitementValue + ((Mathf.Abs(mExcitementTarget - mExcitementValue) / 65.0f) * browVariation * morphSpeed), mExcitementTarget); }
				if (mExcitementTarget < mExcitementValue - 0.1f) { mExcitementValue = Mathf.Max(mExcitementValue - ((Mathf.Abs(mExcitementTarget - mExcitementValue) / 55.0f) * browVariation * morphSpeed), mExcitementTarget); }
				morphExpExcitement.SetValue(Round(mExcitementValue));

				tempFloat = -0.2f + (Mathf.Max(mSmileFullFaceValue,mSmileOpenFullFaceValue) + mSmileSimpleLeftValue) * 1.5f;
				if (mBrowOuterUpLeftTarget + tempFloat > mBrowOuterUpLeftValue + 0.2f) { mBrowOuterUpLeftValue = Mathf.Min(mBrowOuterUpLeftValue + ((Mathf.Abs(mBrowOuterUpLeftTarget - mBrowOuterUpLeftValue - tempFloat) / 35.0f) * browVariation * morphSpeed), mBrowOuterUpLeftTarget + tempFloat); }
				if (mBrowOuterUpLeftTarget + tempFloat < mBrowOuterUpLeftValue - 0.2f) { mBrowOuterUpLeftValue = Mathf.Max(mBrowOuterUpLeftValue - ((Mathf.Abs(mBrowOuterUpLeftTarget - mBrowOuterUpLeftValue - tempFloat) / 65.0f) * browVariation * morphSpeed), mBrowOuterUpLeftTarget + tempFloat); }
				morphBrowOuterUpLeft.SetValue(Round(mBrowOuterUpLeftValue));

				tempFloat = -0.2f + (Mathf.Max(mSmileFullFaceValue,mSmileOpenFullFaceValue) + mSmileSimpleRightValue) * 1.5f;
				if (mBrowOuterUpRightTarget + tempFloat > mBrowOuterUpRightValue + 0.2f) { mBrowOuterUpRightValue = Mathf.Min(mBrowOuterUpRightValue + ((Mathf.Abs(mBrowOuterUpRightTarget - mBrowOuterUpRightValue - tempFloat) / 35.0f) * browVariation * morphSpeed), mBrowOuterUpRightTarget + tempFloat); }
				if (mBrowOuterUpRightTarget + tempFloat < mBrowOuterUpRightValue - 0.2f) { mBrowOuterUpRightValue = Mathf.Max(mBrowOuterUpRightValue - ((Mathf.Abs(mBrowOuterUpRightTarget - mBrowOuterUpRightValue - tempFloat) / 65.0f) * browVariation * morphSpeed), mBrowOuterUpRightTarget + tempFloat); }
				morphBrowOuterUpRight.SetValue(Round(mBrowOuterUpRightValue));
				
				
				if (mBrowCenterUpTarget + (interestArousal / 20.0f) > mBrowCenterUpValue + 0.1) { mBrowCenterUpValue = Mathf.Min(mBrowCenterUpValue + ((Mathf.Abs(mBrowCenterUpTarget - mBrowCenterUpValue) / 75.0f) * browVariation * morphSpeed), mBrowCenterUpTarget + (interestArousal / 20.0f)); }
				if (mBrowCenterUpTarget + (interestArousal / 20.0f) < mBrowCenterUpValue - 0.1) { mBrowCenterUpValue = Mathf.Max(mBrowCenterUpValue - ((Mathf.Abs(mBrowCenterUpTarget - mBrowCenterUpValue) / 135.0f) * browVariation * morphSpeed), mBrowCenterUpTarget + (interestArousal / 20.0f)); }
				morphBrowCenterUp.SetValue(Round(Mathf.Clamp(mBrowCenterUpValue, -0.3f, 1.3f)));
				//SuperController.LogError("Brow Morphs Done");

				if (!IsFaceOnlyExpressionMode)
				{
				tempFloat = Mathf.Max(0.0f, mSmileSimpleLeftValue / 2.0f, mSmileSimpleRightValue / 2.0f) + mSmileFullFaceValue + (mSmileOpenFullFaceValue / 1.5f) + mHappyValue;
				tempFloat = Mathf.Clamp((tempFloat / 1.5f) - mEyesSquintTarget, 0.0f, 1.0f);
				if (mEyesSquintTarget + tempFloat > mEyesSquintValue + 0.01) { mEyesSquintValue = Mathf.Min(mEyesSquintValue + (0.04f * eyeVariation * morphSpeed), mEyesSquintTarget + tempFloat); }
				if (mEyesSquintTarget + tempFloat < mEyesSquintValue - 0.01) { mEyesSquintValue = Mathf.Max(mEyesSquintValue - (0.01f * eyeVariation * morphSpeed), mEyesSquintTarget + tempFloat); }
				morphEyesSquint.SetValue(Round(Mathf.Clamp(mEyesSquintValue - mEyesClosedLeftValue + mTakingItValue - (Mathf.Max(mSmileOpenFullFaceValue, mSmileFullFaceValue)/2.0f) - (mHappyValue/2.0f), -0.3f, 1.0f)));

				float lidLower = 0.25f;
				float lidRaise = 0.4f;
				tempFloat = Random.Range(-0.05f, 0.05f);
				tempFloat = eyeCloseMaxMorph - Mathf.Lerp(0.0f,0.2f,mEyesSquintValue) - Mathf.Lerp(0.0f,0.3f,mTakingItValue) - Mathf.Lerp(0.0f,0.2f,mDeserveItValue) - Mathf.Lerp(0.0f,0.2f,mSmileOpenFullFaceValue);//Mathf.Clamp(1.25f - Mathf.Lerp(0.0f,0.3f,mEyesSquintValue),0.0f, eyeCloseMaxMorph);
				tempFloat2 = 0.0f;
				if (amGlancing)
				{
					tempFloat2 = -0.1f;
				}
				
				if (morphBlinking)
				{
					lidLower = 0.53f + Random.Range(-0.05f, 0.05f);;
					lidRaise = 0.1f + Random.Range(-0.05f, 0.05f);;
					if (mEyesClosedLeftTarget > mEyesClosedLeftValue) { mEyesClosedLeftValue = Mathf.Min(mEyesClosedLeftValue + lidLower, mEyesClosedLeftTarget); }
					if (mEyesClosedLeftTarget < mEyesClosedLeftValue) { mEyesClosedLeftValue = Mathf.Max(mEyesClosedLeftValue - lidRaise, mEyesClosedLeftTarget); }
					morphEyesClosedLeft.SetValue(Round(Mathf.Clamp(mEyesClosedLeftValue, -0.2f, tempFloat)));
					if (mEyesClosedRightTarget > mEyesClosedRightValue) { mEyesClosedRightValue = Mathf.Min(mEyesClosedRightValue + lidLower, mEyesClosedRightTarget); }
					if (mEyesClosedRightTarget < mEyesClosedRightValue) { mEyesClosedRightValue = Mathf.Max(mEyesClosedRightValue - lidRaise, mEyesClosedRightTarget); }
					morphEyesClosedRight.SetValue(Round(Mathf.Clamp(mEyesClosedRightValue, -0.2f, tempFloat)));
				}
				else
				{
					if (mEyesClosedLeftTarget + tempFloat2 > mEyesClosedLeftValue + 0.005f) { mEyesClosedLeftValue = Mathf.Min(mEyesClosedLeftValue + (lidLower * eyeVariation * morphSpeed), mEyesClosedLeftTarget); }
					if (mEyesClosedLeftTarget + tempFloat2 < mEyesClosedLeftValue - 0.005f) { mEyesClosedLeftValue = Mathf.Max(mEyesClosedLeftValue - (lidRaise * eyeVariation * morphSpeed), mEyesClosedLeftTarget); }
					morphEyesClosedLeft.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mEyesClosedLeftValue, -0.5f, tempFloat))));
					if (mEyesClosedRightTarget + tempFloat2 > mEyesClosedRightValue + 0.005f) { mEyesClosedRightValue = Mathf.Min(mEyesClosedRightValue + (lidLower * eyeVariation * morphSpeed), mEyesClosedRightTarget); }
					if (mEyesClosedRightTarget + tempFloat2 < mEyesClosedRightValue - 0.005f) { mEyesClosedRightValue = Mathf.Max(mEyesClosedRightValue - (lidRaise * eyeVariation * morphSpeed), mEyesClosedRightTarget); }
					morphEyesClosedRight.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mEyesClosedRightValue, -0.5f, tempFloat))));
				}

				if (morphBlinking && mEyesClosedRightValue <= 0.1f && mEyesClosedLeftValue <= 0.1f)
				{
					morphBlinking = false;
				}
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
				if (currentMouth == "Smile" || currentMouth == "Big Smile" || currentMouth == "Kissing" || currentMouth == "Sideways" || currentMouth == "Pout")
				{
					tempFloat = Mathf.Lerp(0.0f,Mathf.Lerp(0.01f, 0.05f,interestArousal/10.0f),(breathClock)) * uiBreatheExpandMultiplier.val;
					tempFloat2 = 0.25f;
					mNoseFlareTarget = Mathf.Clamp(mNoseFlareTarget * 5.0f,0.0f,0.5f);
					mMouthOpenWideTarget = 0.0f;
				}
				
				if (usePerson2 && person2Usable && currentLook == "Sex" && playerTipToPelvis < interactionDistance)
				{
					tempFloat = Mathf.Clamp(Vector3.Distance(playerTipController.followWhenOff.position,playerTipPrev) * 7.0f,0.0f,1.0f);
					//mMouthOpenWideTarget = Mathf.Clamp(mMouthOpenWideTarget + (tempFloat / 1.0f),0.0f,1.0f);
				}
				if (mouthCanOpen == false || currentMouth == "Closed")
				{
					tempFloat = 0.0f;
				}
				
				if (!IsFaceOnlyExpressionMode && morphEyesPupils != null)
				{
					if (mEyesPupilsTarget > mEyesPupilsValue + 0.01f) { mEyesPupilsValue = Mathf.Min(mEyesPupilsValue + (0.01f * uiPupilRate.val), mEyesPupilsTarget); }
					if (mEyesPupilsTarget < mEyesPupilsValue - 0.01f) { mEyesPupilsValue = Mathf.Max(mEyesPupilsValue - (0.03f * uiPupilRate.val), mEyesPupilsTarget); }
					morphEyesPupils.SetValue(Round(mEyesPupilsValue));
				}

				
				if (morphMouthMouthOpen != null)
				{
					tempFloat = 0.35f;
					if (mouthCanOpen == false || currentMouth == "Closed")
					{
						//mMouthOpenTarget = -0.1f + (((interestArousal+interestValence)/2.0f) / 20.0f);
						tempFloat = 0.0f;
						//if (currentMouth == "Idle")
						//{
						//tempFloat = Mathf.Clamp(0.0f + (interestArousal/20.0f) - mSmileOpenFullFaceValue - mHappyValue - mLipBiteValue - mMouthSideLeftValue - mMouthSideRightValue - mLipsPuckerValue,0.0f,1.0f);
						//}
						//voiceOpenAdjust = 0.0f;
					}
					tempFloat = tempFloat - mSmileFullFaceValue - Mathf.Max(mSmileOpenFullFaceValue,0.0f) - Mathf.Max(0.0f, mSmileSimpleLeftValue, mSmileSimpleRightValue) - mLipsPartValue - (mLipBiteTarget*2.0f) - mHappyValue;
					if (currentMouth != "Big Smile" && currentMouth != "Sideways" && currentMouth != "Smile")// && currentMouth != "Closed")
					{
						//mMouthOpenTarget = mMouthOpenTarget / 2.0f;
						if (mouthCanOpen == false)
						{
						//tempFloat = 0.0f - Mathf.Lerp(0.2f,0.0f,mouthOpenTimer);
						}
					}
					else
					{
						if (currentMouth == "Closed" || currentMouth == "Demure" || currentMouth == "Pout" || currentMouth == "Sideways")
						{
							mMouthOpenTarget = 0.0f;
							tempFloat = 0.0f;
							//voiceOpenAdjust = 0.0f;
						}
						else
						{
							//mMouthOpenTarget = Mathf.Lerp(-0.2f,0.4f,Mathf.Max((interestArousal/10.0f)-0.5f,0.0f));
						}
					}
					if (currentMouth == "Open")
					{
						tempFloat += 0.1f;
					}
					mMouthOpenTarget = Mathf.Clamp(mMouthOpenTarget, -0.15f, 0.6f);
					if (mMouthOpenTarget + tempFloat + voiceOpenAdjust > mMouthOpenValue + 0.2f) { mMouthOpenValue = Mathf.Min(mMouthOpenValue + ((Mathf.Abs(mMouthOpenTarget - mMouthOpenValue) / 75.0f) * tempFloat2 * morphSpeed), mMouthOpenTarget + tempFloat + voiceOpenAdjust); }
					if (mMouthOpenTarget + tempFloat + voiceOpenAdjust < mMouthOpenValue - 0.2f) { mMouthOpenValue = Mathf.Max(mMouthOpenValue - ((Mathf.Abs(mMouthOpenTarget - mMouthOpenValue) / 35.0f) * mouthVariation * morphSpeed), mMouthOpenTarget + tempFloat + voiceOpenAdjust); }
					mMouthOpenValue = Mathf.Clamp(mMouthOpenValue, -0.2f,1.0f);
					if (person2IsMale)
					{
						morphMouthMouthOpen.SetValue(Round(mMouthOpenValue/ 2.0f) + uiMouthOpenOffset.val);//Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mMouthOpenValue / 2.0f, 0.0f, 0.5f))));
					}
					else
					{
						morphMouthMouthOpen.SetValue(Round(mMouthOpenValue) + uiMouthOpenOffset.val);//Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mMouthOpenValue / 2.0f, 0.0f, 0.5f))));
					}
				}
				if (morphNoseFlare != null)
				{
					if (mNoseFlareTarget > mNoseFlareValue + 0.01f) { mNoseFlareValue = Mathf.Min(mNoseFlareValue + (0.05f * tempFloat2 * morphSpeed), mNoseFlareTarget); }
					if (mNoseFlareTarget < mNoseFlareValue - 0.01f) { mNoseFlareValue = Mathf.Max(mNoseFlareValue - (0.005f * mouthVariation * morphSpeed), mNoseFlareTarget); }
					morphNoseFlare.SetValue(Round(Mathf.Clamp(mNoseFlareValue, -1.0f, 1.0f)));
				}
				tempFloat2 = 1.0f;
				if (currentLook == "Sex")
				{
					tempFloat2 = 3.0f;
				}				
				if (morphMouthMouthOpenWide != null && morphMouthNarrow != null)
				{
					if (mouthCanOpen == false)
					{
						mMouthOpenWideTarget = 0.0f;
					}
					if (mMouthOpenWideTarget + voiceOpenAdjust > mMouthOpenWideValue + 0.01f) { mMouthOpenWideValue = Mathf.Min(mMouthOpenWideValue + (0.002f * tempFloat2 * morphSpeed), mMouthOpenWideTarget + voiceOpenAdjust); }
					if (mMouthOpenWideTarget + voiceOpenAdjust < mMouthOpenWideValue - 0.01f) { mMouthOpenWideValue = Mathf.Max(mMouthOpenWideValue - (0.0005f * tempFloat2 * morphSpeed), mMouthOpenWideTarget + voiceOpenAdjust); }
					morphMouthMouthOpenWide.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(mMouthOpenWideValue)));
					if (currentMouth == "Open")
					{
						mMouthNarrowTarget = Mathf.Clamp((mMouthOpenWideValue* 0.55f),0.0f,1.0f);
					}
					else
					{
						mMouthNarrowTarget = Mathf.Clamp((mMouthOpenWideValue* 0.55f) + (mMouthOpenValue * 0.55f),0.0f,1.0f);
					}
					if (mouthCanOpen == false)
					{
						mMouthNarrowTarget = 0.0f;
					}
					if (mMouthNarrowTarget > mMouthNarrowValue + 0.01f) { mMouthNarrowValue = Mathf.Min(mMouthNarrowValue + (0.002f * morphSpeed), mMouthNarrowTarget); }
					if (mMouthNarrowTarget < mMouthNarrowValue - 0.01f) { mMouthNarrowValue = Mathf.Max(mMouthNarrowValue - (0.0005f * morphSpeed), mMouthNarrowTarget); }
					morphMouthNarrow.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mMouthNarrowValue,0.0f,1.0f))));
				}
				if (morphMouthOpenWider != null && usePerson2 && person2Usable)
				{
					if ((mainInterest == "Tip" || mainInterest == "Pelvis") && Vector3.Distance(playerTipController.followWhenOff.position,headController.followWhenOff.position) < closeFaceDistance * 1.3f && uiDoBlowjob.val)
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
					morphMouthOpenWider.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(mMouthOpenWiderValue)));
				}
				if (morphLipsLipsPart != null && morphLipsPouty != null)
				{
					if (currentMouth != "Open" && currentMouth != "Demure")
					{
					mLipsPartTarget = Mathf.Clamp(0.0f + ((interestArousal- 3.0f)/20.0f) - (Mathf.Max(mSmileSimpleLeftValue / 1.5f,mSmileSimpleRightValue / 1.5f, mSmileOpenFullFaceValue * 2.0f, mSmileFullFaceValue)) - mHappyValue - mLipsPuckerValue - mMouthOpenValue - (mTakingItValue * 2.0f) - mLipsBottomDownValue,0.0f,1.0f);
					mLipsPoutyTarget = Mathf.Round(Mathf.Clamp(mLipsPartValue,0.0f,1.0f) * 50.0f) / 50.0f;
					}
					else
					{
						mLipsPoutyTarget = 0.0f;
					}
					if (mLipsPartTarget > mLipsPartValue + 0.01f) { mLipsPartValue = Mathf.Min(mLipsPartValue + (0.002f * morphSpeed), mLipsPartTarget); }
					if (mLipsPartTarget < mLipsPartValue - 0.01f) { mLipsPartValue = Mathf.Max(mLipsPartValue - (0.01f * morphSpeed), mLipsPartTarget); }
					morphLipsLipsPart.SetValue(Mathf.Round(Mathf.Clamp(mLipsPartValue,0.0f,1.0f) * 50.0f) / 50.0f);
					if (mLipsPoutyTarget > mLipsPoutyValue + 0.01f) { mLipsPoutyValue = Mathf.Min(mLipsPoutyValue + (0.002f * morphSpeed), mLipsPoutyTarget); }
					if (mLipsPoutyTarget < mLipsPoutyValue - 0.01f) { mLipsPoutyValue = Mathf.Max(mLipsPoutyValue - (0.01f * morphSpeed), mLipsPoutyTarget); }
					morphLipsPouty.SetValue(mLipsPoutyValue);
				}
				if (morphLipsLipsPartCenter != null)
				{
					if (mLipsCenterPartTarget > mLipsCenterPartValue + 0.01f) { mLipsCenterPartValue = Mathf.Min(mLipsCenterPartValue + (0.002f * morphSpeed), mLipsCenterPartTarget); }
					if (mLipsCenterPartTarget < mLipsCenterPartValue - 0.01f) { mLipsCenterPartValue = Mathf.Max(mLipsCenterPartValue - (0.05f * morphSpeed), mLipsCenterPartTarget); }
					morphLipsLipsPartCenter.SetValue(Mathf.Round(Mathf.Clamp(mLipsCenterPartValue,0.0f,1.0f) * 50.0f) / 50.0f);
				}
				//
				if (morphMouthSmileMuscle != null)
				{
					mSmileMuscleTarget = Mathf.Max(mSmileSimpleLeftValue / 3.0f,mSmileSimpleRightValue / 3.0f, mSmileOpenFullFaceValue * 2.0f, mSmileFullFaceValue) * uiMaxMorphSmile.val;
					if (mSmileMuscleTarget > mSmileMuscleValue + 0.01f) { mSmileMuscleValue = Mathf.Min(mSmileMuscleValue + (0.002f * morphSpeed), mSmileMuscleTarget); }
					if (mSmileMuscleTarget < mSmileMuscleValue - 0.01f) { mSmileMuscleValue = Mathf.Max(mSmileMuscleValue - (0.05f * morphSpeed), mSmileMuscleTarget); }
					morphMouthSmileMuscle.SetValue(Mathf.Round(Mathf.Clamp(mSmileMuscleValue,0.0f,1.0f) * 50.0f) / 50.0f);
				}
				if (morphLipsBottomDown != null)
				{
					mLipsBottomDownValue = (Mathf.Round(Mathf.Clamp(Mathf.Max(mSmileSimpleLeftValue / 2.0f,mSmileSimpleRightValue / 2.0f, mSmileFullFaceValue / 2.0f) - Mathf.Max(mMouthOpenValue,0.0f),0.0f,0.2f) * 50.0f) / 50.0f)  * uiMaxMorphSmile.val;
					morphLipsBottomDown.SetValue(mLipsBottomDownValue);
				}
				
				if (morphLipsLipsClose != null)
				{
					if (currentMouth == "Demure")
					{
						//mLipsCloseTarget = -0.4f;
					}
					if (currentMouth == "Sideways") //currentMouth == "Pout" || 
					{
						mLipsCloseTarget = 0.2f;
					}
					tempFloat = 0.0f - (mSmileFullFaceValue/10.0f);
					if (currentMouth == "Idle" || currentMouth == "Open" || currentMouth == "Closed")
					{
						tempFloat = (mLipsPartValue / 2.0f) - (mSmileFullFaceValue/10.0f);
					}
					if (mLipsCloseTarget - tempFloat > mLipsCloseValue + 0.005f) { mLipsCloseValue = Mathf.Min(mLipsCloseValue + ((Mathf.Abs(mLipsCloseTarget - mLipsCloseValue) / 55.0f) * morphSpeed), mLipsCloseTarget - tempFloat); }
					if (mLipsCloseTarget - tempFloat < mLipsCloseValue - 0.005f) { mLipsCloseValue = Mathf.Max(mLipsCloseValue - ((Mathf.Abs(mLipsCloseTarget - mLipsCloseValue) / 25.0f) * morphSpeed), mLipsCloseTarget - tempFloat); }
					morphLipsLipsClose.SetValue(Round(Mathf.Clamp(mLipsCloseValue,-0.2f,1.0f) + uiLipsCloseOffset.val));
				}
				if (morphLipsLipsPucker != null && morphLipsLipsPuckerWide != null)
				{
					if (mLipsPuckerTarget + mLipsPoutyValue + voicePuckerAdjust > mLipsPuckerValue + 0.2f) { mLipsPuckerValue = Mathf.Min(mLipsPuckerValue + ((Mathf.Abs(mLipsPuckerTarget - mLipsPuckerValue) / 65.0f) * mouthVariation * morphSpeed), mLipsPuckerTarget + mLipsPoutyValue + voicePuckerAdjust ); }
					if (mLipsPuckerTarget + mLipsPoutyValue + voicePuckerAdjust  < mLipsPuckerValue - 0.2f) { mLipsPuckerValue = Mathf.Max(mLipsPuckerValue - ((Mathf.Abs(mLipsPuckerTarget - mLipsPuckerValue) / 120.0f) * mouthVariation * morphSpeed), mLipsPuckerTarget + mLipsPoutyValue + voicePuckerAdjust ); }
					morphLipsLipsPucker.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mLipsPuckerValue - mSmileFullFaceValue - Mathf.Max(mSmileSimpleLeftValue / 2.0f, mSmileSimpleRightValue / 2.0f),0.0f,1.0f))));
					if (mLipsPuckerWideTarget > mLipsPuckerWideValue + 0.1f) { mLipsPuckerWideValue = Mathf.Min(mLipsPuckerWideValue + (0.01f * mouthVariation * morphSpeed), mLipsPuckerWideTarget); }
					if (mLipsPuckerWideTarget < mLipsPuckerWideValue - 0.1f) { mLipsPuckerWideValue = Mathf.Max(mLipsPuckerWideValue - (0.007f * mouthVariation * morphSpeed), mLipsPuckerWideTarget); }
					morphLipsLipsPuckerWide.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(Mathf.Clamp(mLipsPuckerWideValue,0.0f,1.0f))));
				}
				if (morphLipsLipBite != null)
				{
					if (mLipBiteTarget > mLipBiteValue + 0.05f) { mLipBiteValue = Mathf.Min(mLipBiteValue + ((Mathf.Abs(mLipBiteTarget - mLipBiteValue) / 25.0f) * mouthVariation * morphSpeed), mLipBiteTarget ); }
					if (mLipBiteTarget < mLipBiteValue - 0.05f) { mLipBiteValue = Mathf.Max(mLipBiteValue - ((Mathf.Abs(mLipBiteTarget - mLipBiteValue) / 35.0f) * mouthVariation * morphSpeed), mLipBiteTarget ); }
					morphLipsLipBite.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(mLipBiteValue)));
					mLipBottomInTarget = Mathf.Clamp(mLipBiteValue,0.0f,0.3f);
				}
				if (morphLipBottomIn != null)
				{
					mLipBottomInValue = morphLipBottomIn.morphValue;
					if (mLipBottomInTarget > mLipBottomInValue + 0.05f) { mLipBottomInValue = Mathf.Min(mLipBottomInValue + ((Mathf.Abs(mLipBottomInTarget - mLipBottomInValue) / 25.0f) * mouthVariation * morphSpeed), mLipBottomInTarget ); }
					if (mLipBottomInTarget < mLipBottomInValue - 0.05f) { mLipBottomInValue = Mathf.Max(mLipBottomInValue - ((Mathf.Abs(mLipBottomInTarget - mLipBottomInValue) / 35.0f) * mouthVariation * morphSpeed), mLipBottomInTarget ); }
					//morphLipBottomIn.SetValue(Round(mLipBottomInValue));
				}
				if (morphExpFlirting != null)
				{
					if (mFlirtingTarget > mFlirtingValue + 0.01f) { mFlirtingValue = Mathf.Min(mFlirtingValue + ((Mathf.Abs(mFlirtingTarget - mFlirtingValue) / 15.0f) * mouthVariation * morphSpeed), mFlirtingTarget); }
					if (mFlirtingTarget < mFlirtingValue - 0.01f) { mFlirtingValue = Mathf.Max(mFlirtingValue - ((Mathf.Abs(mFlirtingTarget - mFlirtingValue) / 45.0f) * mouthVariation * morphSpeed), mFlirtingTarget); }
					morphExpFlirting.SetValue(Round(mFlirtingValue));
				}
				if (morphExpDeserveIt != null)
				{
					if (Round(mTakingItValue) <= 0.1f)
					{
						if (mDeserveItTarget > mDeserveItValue + 0.01f) { mDeserveItValue = Mathf.Min(mDeserveItValue + ((Mathf.Abs(mDeserveItTarget - mDeserveItValue) / 85.0f) * mouthVariation * tempFloat2 * morphSpeed), mDeserveItTarget); }
						morphExpDeserveIt.SetValue(Round(mDeserveItValue));
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
						morphExpTakingIt.SetValue(Round(mTakingItValue));
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
					morphExpHappy.SetValue(Round(mHappyValue));
				}
				if (morphExpSmileFullFace != null && morphExpSmileOpenFullFace != null)
				{
					if (mouthCanOpen == false && currentMouth != "Big Smile")
					{
						mSmileFullFaceTarget = -1.0f;
						mSmileOpenFullFaceTarget = -1.0f;
					}
					if (mSmileOpenFullFaceTarget + Mathf.Min(voiceOpenAdjust,0.0f) > mSmileOpenFullFaceValue + 0.1f) { mSmileOpenFullFaceValue = Mathf.Min(mSmileOpenFullFaceValue + ((Mathf.Abs(mSmileOpenFullFaceTarget - mSmileOpenFullFaceValue) / 45.0f) * mouthVariation * morphSpeed), mSmileOpenFullFaceTarget + Mathf.Min(voiceOpenAdjust,0.0f)); }
					if (mSmileOpenFullFaceTarget + Mathf.Min(voiceOpenAdjust,0.0f) < mSmileOpenFullFaceValue - 0.1f) { mSmileOpenFullFaceValue = Mathf.Max(mSmileOpenFullFaceValue - ((Mathf.Abs(mSmileOpenFullFaceTarget - mSmileOpenFullFaceValue) / 125.0f) * mouthVariation * morphSpeed), mSmileOpenFullFaceTarget + Mathf.Min(voiceOpenAdjust,0.0f)); }
					morphExpSmileOpenFullFace.SetValue(Mathf.SmoothStep(0.0f,1.0f,Round(mSmileOpenFullFaceValue) * uiMaxMorphSmile.val));
					if (Mathf.Clamp((interestValence / Mathf.Lerp(30.0f, 15.0f,pExtraversion/100.0f)) + mSmileFullFaceTarget, 0.0f, interestMaxSmile) > mSmileFullFaceValue + 0.1f) { mSmileFullFaceValue = Mathf.Min(mSmileFullFaceValue + ((Mathf.Abs(mSmileFullFaceTarget - mSmileFullFaceValue) / 35.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / Mathf.Lerp(30.0f, 15.0f,pExtraversion/100.0f)) + mSmileFullFaceTarget, 0.0f, interestMaxSmile)); }
					if (Mathf.Clamp((interestValence / Mathf.Lerp(30.0f, 15.0f,pExtraversion/100.0f)) + mSmileFullFaceTarget, 0.0f, interestMaxSmile) < mSmileFullFaceValue - 0.1f) { mSmileFullFaceValue = Mathf.Max(mSmileFullFaceValue - ((Mathf.Abs(mSmileFullFaceTarget - mSmileFullFaceValue) / 190.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / Mathf.Lerp(30.0f, 15.0f,pExtraversion/100.0f)) + mSmileFullFaceTarget, 0.0f, interestMaxSmile)); }
					morphExpSmileFullFace.SetValue((Round(Mathf.Max(mSmileFullFaceValue - (mSmileOpenFullFaceValue / 3.0f), 0.0f)) * uiMaxMorphSmile.val));
				}
				if (morphVisF != null)
				{
					if (mVisFTarget > mVisFValue + 0.03f) { mVisFValue = Mathf.Min(mVisFValue + (0.0003f * mouthVariation * morphSpeed), mVisFTarget); }
					if (mVisFTarget < mVisFValue - 0.03f) { mVisFValue = Mathf.Max(mVisFValue - (0.0001f * mouthVariation * morphSpeed), mVisFTarget); }
					morphVisF.SetValue(Round(mVisFValue));
				}
				if (morphVisM != null)
				{
					if (mVisMTarget > mVisMValue + 0.01f) { mVisMValue = Mathf.Min(mVisMValue + (0.05f * mouthVariation * morphSpeed), mVisMTarget); }
					if (mVisMTarget < mVisMValue - 0.01f) { mVisMValue = Mathf.Max(mVisMValue - (0.015f * mouthVariation * morphSpeed), mVisMTarget); }
					morphVisM.SetValue(Round(mVisMValue));
				}
				if (morphVisOW != null)
				{
					if (mVisOWTarget > mVisOWValue + 0.01f) { mVisOWValue = Mathf.Min(mVisOWValue + (0.03f * mouthVariation * morphSpeed), mVisOWTarget); }
					if (mVisOWTarget < mVisOWValue - 0.01f) { mVisOWValue = Mathf.Max(mVisOWValue - (0.05f * mouthVariation * morphSpeed), mVisOWTarget); }
					morphVisOW.SetValue(Round(mVisOWValue));
				}
				if (morphVisAA != null)
				{
					if (mVisAATarget > mVisAAValue + 0.01f) { mVisAAValue = Mathf.Min(mVisAAValue + (0.05f * mouthVariation * morphSpeed), mVisAATarget); }
					if (mVisAATarget < mVisAAValue - 0.01f) { mVisAAValue = Mathf.Max(mVisAAValue - (0.03f * mouthVariation * morphSpeed), mVisAATarget); }
					morphVisAA.SetValue(Round(mVisAAValue));
				}

				if (morphMouthSmileSimpleLeft != null && morphMouthSmileSimpleRight != null)
				{
					if (Mathf.Clamp((interestValence / 50.0f) + mSmileSimpleLeftTarget - mMouthNarrowValue - mSmileFullFaceValue, 0.0f, interestMaxSmile) > mSmileSimpleLeftValue + 0.02f) { mSmileSimpleLeftValue = Mathf.Min(mSmileSimpleLeftValue + ((Mathf.Abs(mSmileSimpleLeftTarget - mSmileSimpleLeftValue) / 135.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / 50.0f) + mSmileSimpleLeftTarget - mMouthNarrowValue - mSmileFullFaceValue, 0.0f, interestMaxSmile)); }
					if (Mathf.Clamp((interestValence / 50.0f) + mSmileSimpleLeftTarget - mMouthNarrowValue - mSmileFullFaceValue, 0.0f, interestMaxSmile) < mSmileSimpleLeftValue - 0.02f) { mSmileSimpleLeftValue = Mathf.Max(mSmileSimpleLeftValue - ((Mathf.Abs(mSmileSimpleLeftTarget - mSmileSimpleLeftValue) / 150.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / 50.0f) + mSmileSimpleLeftTarget - mMouthNarrowValue - mSmileFullFaceValue, 0.0f, interestMaxSmile)); }
					morphMouthSmileSimpleLeft.SetValue(Round(Mathf.Max(mSmileSimpleLeftValue - mSmileOpenFullFaceValue, 0.0f)) * uiMaxMorphSmile.val);
					if (Mathf.Clamp((interestValence / 50.0f) + mSmileSimpleRightTarget - mMouthNarrowValue, 0.0f, interestMaxSmile) > mSmileSimpleRightValue + 0.02f) { mSmileSimpleRightValue = Mathf.Min(mSmileSimpleRightValue + ((Mathf.Abs(mSmileSimpleRightTarget - mSmileSimpleRightValue) / 135.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / 50.0f) + mSmileSimpleRightTarget - mMouthNarrowValue, 0.0f, interestMaxSmile)); }
					if (Mathf.Clamp((interestValence / 50.0f) + mSmileSimpleRightTarget - mMouthNarrowValue, 0.0f, interestMaxSmile) < mSmileSimpleRightValue - 0.02f) { mSmileSimpleRightValue = Mathf.Max(mSmileSimpleRightValue - ((Mathf.Abs(mSmileSimpleRightTarget - mSmileSimpleRightValue) / 150.0f) * mouthVariation * morphSpeed), Mathf.Clamp((interestValence / 50.0f) + mSmileSimpleRightTarget - mMouthNarrowValue, 0.0f, interestMaxSmile)); }
					morphMouthSmileSimpleRight.SetValue(Round(Mathf.Max(mSmileSimpleRightValue - mSmileOpenFullFaceValue, 0.0f)) * uiMaxMorphSmile.val);
				}

				if (morphMouthSideLeft != null && morphMouthSideRight != null)
				{
					if (mMouthSideLeftTarget > mMouthSideLeftValue + 0.05f) { mMouthSideLeftValue = Mathf.Min(mMouthSideLeftValue + ((Mathf.Abs(mMouthSideLeftTarget - mMouthSideLeftValue) / 15.0f) * mouthVariation * morphSpeed), mMouthSideLeftTarget); }
					if (mMouthSideLeftTarget < mMouthSideLeftValue - 0.05f) { mMouthSideLeftValue = Mathf.Max(mMouthSideLeftValue - ((Mathf.Abs(mMouthSideLeftTarget - mMouthSideLeftValue) / 30.0f) * mouthVariation * morphSpeed), mMouthSideLeftTarget); }
					morphMouthSideLeft.SetValue(Round(mMouthSideLeftValue));
					if (mMouthSideRightTarget > mMouthSideRightValue + 0.05f) { mMouthSideRightValue = Mathf.Min(mMouthSideRightValue + ((Mathf.Abs(mMouthSideRightTarget - mMouthSideRightValue) / 15.0f) * mouthVariation * morphSpeed), mMouthSideRightTarget); }
					if (mMouthSideRightTarget < mMouthSideRightValue - 0.05f) { mMouthSideRightValue = Mathf.Max(mMouthSideRightValue - ((Mathf.Abs(mMouthSideRightTarget - mMouthSideRightValue) / 30.0f) * mouthVariation * morphSpeed), mMouthSideRightTarget); }
					morphMouthSideRight.SetValue(Round(mMouthSideRightValue));
				}
				//SuperController.LogError("Mouth Morphs Done");
				
				if (mTongueInOutTarget > mTongueInOutValue + 0.01f) { mTongueInOutValue = Mathf.Min(mTongueInOutValue + (0.08f * mouthVariation * morphSpeed), mTongueInOutTarget); }
				if (mTongueInOutTarget < mTongueInOutValue - 0.01f) { mTongueInOutValue = Mathf.Max(mTongueInOutValue - (0.05f * mouthVariation * morphSpeed), mTongueInOutTarget); }
				morphTongueInOut.SetValue(Round(mTongueInOutValue));
				if (mTongueSideSideTarget > mTongueSideSideValue + 0.01f) { mTongueSideSideValue = Mathf.Min(mTongueSideSideValue + (0.084f * mouthVariation * morphSpeed), mTongueSideSideTarget); }
				if (mTongueSideSideTarget < mTongueSideSideValue - 0.01f) { mTongueSideSideValue = Mathf.Max(mTongueSideSideValue - (0.017f * mouthVariation * morphSpeed), mTongueSideSideTarget); }
				morphTongueSideSide.SetValue(Round(mTongueSideSideValue));
				if (mTongueBendTipTarget > mTongueBendTipValue + 0.01f) { mTongueBendTipValue = Mathf.Min(mTongueBendTipValue + (0.08f * mouthVariation * morphSpeed), mTongueBendTipTarget); }
				if (mTongueBendTipTarget < mTongueBendTipValue - 0.01f) { mTongueBendTipValue = Mathf.Max(mTongueBendTipValue - (0.06f * mouthVariation * morphSpeed), mTongueBendTipTarget); }
				morphTongueBendTip.SetValue(Round(mTongueBendTipValue));
				//SuperController.LogError("Tongue Morphs Done");

				if (morphShoulderFixLeftF != null)
				{
					
					tempFloat = Vector3.Distance(lElbowController.followWhenOff.position, abdomenController.followWhenOff.position);
					//testString = "|" + ((Mathf.Clamp(tempFloat, 0.18f, 0.3f) - 0.12f) * 8.33f);
					
					//Vector3 tempplane = Vector3.ProjectOnPlane(lArmController.followWhenOff.right, pelvisController.followWhenOff.forward);
					//float tempangle = Vector3.SignedAngle(chestController.followWhenOff.forward, lArmController.followWhenOff.right, chestController.followWhenOff.forward);
					//testString = tempangle.ToString();
					
					mShoulderFixLeftTarget = 1.0f - ((Mathf.Clamp(tempFloat, 0.18f, 0.3f) - 0.12f) * 18.33f) * 100.0f;
					
					tempFloat = Vector3.Distance(rElbowController.followWhenOff.position, abdomenController.followWhenOff.position);
					mShoulderFixRightTarget = 1.0f - ((Mathf.Clamp(tempFloat, 0.18f, 0.3f) - 0.12f) * 18.33f) * 100.0f;
					
					if (mShoulderFixLeftTarget > mShoulderFixLeftValue + 0.01f) { mShoulderFixLeftValue = Mathf.Min(mShoulderFixLeftValue + (0.1f), mShoulderFixLeftTarget); }
					if (mShoulderFixLeftTarget < mShoulderFixLeftValue - 0.01f) { mShoulderFixLeftValue = Mathf.Max(mShoulderFixLeftValue - (0.1f), mShoulderFixLeftTarget); }
					morphShoulderFixLeftF.SetValue(Round(Mathf.Lerp(0.0f, 0.45f, mShoulderFixLeftValue)));
					morphShoulderFixLeftR.SetValue(Round(Mathf.Lerp(0.0f, 1.0f, mShoulderFixLeftValue)));
					if (mShoulderFixRightTarget > mShoulderFixRightValue + 0.01f) { mShoulderFixRightValue = Mathf.Min(mShoulderFixRightValue + (0.1f), mShoulderFixRightTarget); }
					if (mShoulderFixRightTarget < mShoulderFixRightValue - 0.01f) { mShoulderFixRightValue = Mathf.Max(mShoulderFixRightValue - (0.1f), mShoulderFixRightTarget); }
					morphShoulderFixRightF.SetValue(Round(Mathf.Lerp(0.0f, 0.45f, mShoulderFixRightValue)));
					morphShoulderFixRightR.SetValue(Round(Mathf.Lerp(0.0f, 1.0f, mShoulderFixRightValue)));
				}

			}
			else
			{
				if (resetMorphs == false)
				{
					resetMorphs = true;
					morphLHandFist.SetValue(0.0f);
					morphRHandFist.SetValue(0.0f);
					morphLHandStraighten.SetValue(0.0f);
					morphRHandStraighten.SetValue(0.0f);

					morphBrowDown.SetValue(0.0f);
					morphBrowUp.SetValue(0.0f);
					morphBrowCenterUp.SetValue(0.0f);
					morphBrowOuterUpLeft.SetValue(0.0f);
					morphBrowOuterUpRight.SetValue(0.0f);

					morphEyesClosedLeft.SetValue(0.0f);
					morphEyesClosedRight.SetValue(0.0f);
					morphEyesSquint.SetValue(0.0f);
					morphEyesPupils.SetValue(0.0f);
					morphNoseFlare.SetValue(0.0f);

					morphExpSmileFullFace.SetValue(0.0f);
					morphExpSmileOpenFullFace.SetValue(0.0f);
					morphExpGlare.SetValue(0.0f);
					morphExpExcitement.SetValue(0.0f);
					morphExpHappy.SetValue(0.0f);
					morphExpFlirting.SetValue(0.0f);
					morphExpDeserveIt.SetValue(0.0f);
					morphExpTakingIt.SetValue(0.0f);

					morphMouthMouthOpen.SetValue(0.0f);
					morphMouthMouthOpenWide.SetValue(0.0f);
					morphMouthOpenWider.SetValue(0.0f);
					morphMouthNarrow.SetValue(0.0f);
					morphMouthSideLeft.SetValue(0.0f);
					morphMouthSideRight.SetValue(0.0f);
					morphMouthSmileSimpleLeft.SetValue(0.0f);
					morphMouthSmileSimpleRight.SetValue(0.0f);

					morphLipsLipsPucker.SetValue(0.0f);
					morphLipsLipsPuckerWide.SetValue(0.0f);
					morphLipsLipBite.SetValue(0.0f);
					morphLipsLipsClose.SetValue(0.0f);
					morphLipsLipsPart.SetValue(0.0f);
					morphLipsLipsPartCenter.SetValue(0.0f);
					morphLipsBottomDown.SetValue(0.0f);
					morphLipBottomIn.SetValue(0.0f);
					morphLipsPouty.SetValue(0.0f);
					morphMouthSmileMuscle.SetValue(0.0f);
					morphVisF.SetValue(0.0f);
					morphVisM.SetValue(0.0f);
					morphVisOW.SetValue(0.0f);
					morphVisAA.SetValue(0.0f);

					morphTongueInOut.SetValue(1.0f);
					morphTongueSideSide.SetValue(0.0f);
					morphTongueBendTip.SetValue(0.0f);
					morphTongueLength.SetValue(0.0f);
					morphRibCageSize.SetValue(0.0f);
					morphChestHeight.SetValue(0.0f);
					morphBreastHeight.SetValue(0.0f);
					morphBreastDroopLeft.SetValue(0.0f);
					morphBreastDroopRight.SetValue(0.0f);
					morphBreastHangLeft.SetValue(0.0f);
					morphBreastHangRight.SetValue(0.0f);
					morphBreath.SetValue(0.0f);
					//morphRibsDef.SetValue(0.0f);
					morphSternumDepth.SetValue(0.0f);
					morphNipplesApply.SetValue(0.0f);
					morphDeepBulgeBellyBottom.SetValue(0.0f);
					morphDeepBulgeBellyMid.SetValue(0.0f);
					morphDeepThroat.SetValue(0.0f);
					morphBlowjobLips.SetValue(0.0f);
					morphCheekSink.SetValue(0.0f);

					if (morphShoulderFixRightR != null)
					{
						morphShoulderFixLeftF.SetValue(0.0f);
						morphShoulderFixLeftR.SetValue(0.0f);
						morphShoulderFixRightF.SetValue(0.0f);
						morphShoulderFixRightR.SetValue(0.0f);
					}
				}
			}
			
			if (mMouthOpenValue > 0.12f || mMouthOpenWideValue > 0.15f || mMouthOpenWiderValue > 0.15f || mSmileOpenFullFaceValue > 0.34f || mLipsBottomDownValue > 0.3f || mMouthOpenValue + mMouthOpenWideValue + mMouthOpenWiderValue + mSmileOpenFullFaceValue > 0.7f)
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

			
            if (saccadeClock <= 0.0f && mEyesClosedLeftValue < 0.7f && interestKissing == false)
            {
				//SuperController.LogError("Saccade Start");
				tempFloat2 = Mathf.Clamp(Vector3.Distance(headController.followWhenOff.position, eyeController.transform.position) - (closeFaceDistance * 1.0f),0.0f,1.0f);
				tempFloat = (10.0f + (saccadeAmount * Mathf.Lerp(0.2f,1.0f,tempFloat2))) * uiSaccadeAmount.val;//(saccadeAmount / Random.Range(0.8f,1.2f)) * uiSaccadeAmount.val * Mathf.Lerp(0.3f,1.0f,tempFloat2);
                float saccade = Random.Range(0.0f,100.0f); //Mathf.Lerp(30.0f,60.0f,tempFloat/30.0f)) / uiSaccadeSpeed.val;//Random.Range(0.0f, Mathf.Clamp(150.0f * (0.5f + (100.0f - pExtraversion)), 0.0f, 100.0f));
				debugString = " RND " + Round(saccade) + " Base " + Round(tempFloat) + " ";
                //saccadeOffset = new Vector3(0.0f,0.0f,0.0f);
                float saccadeLength = (((10.0f + interestArousal) / (2.0f * (10.0f - interestValence))));// * ((2.0f * tempFloat + 20.0f) / 100.0f));// * Random.Range(0.5f, 1.5f);
                float saccadeRandom = Random.Range(-1.0f, 1.0f);
                saccadeClock = Mathf.Clamp(saccadeLength, 1.1f, 3.5f) * Mathf.Lerp(0.85f,0.15f,tempFloat/30.0f) / uiSaccadeSpeed.val;

				if (gAvoid == 1.0f)
				{
					tempFloat = tempFloat * 2.0f;
				}
				tempFloat2 = Mathf.Clamp(0.0f,1.0f,playerHeadToHead - closeFaceDistance);
				if (Vector3.Distance(new Vector3(0.0f,0.0f,0.0f), saccadeOffset) > 10.0f)
				{
					saccadeOffsetCounter += 1.0f;
				}
				if ((Mathf.Abs(saccadeOffset.x) > 12.5f * uiSaccadeWanderMult.val * tempFloat2 || saccadeOffset.y > 2.5f * uiSaccadeWanderMult.val * tempFloat2 || saccadeOffset.y < Mathf.Lerp(-7.5f, -22.5f, interestArousal/10.0f) * uiSaccadeWanderMult.val * tempFloat2) || Random.Range(0.0f,100.0f) > Mathf.Lerp(99655.0f,99989.0f,pExtraversion/100.0f) / 1000.0f || saccadeOffsetCounter > 5.0f || Random.Range(0.0f,100.0f) > Mathf.Lerp(59.0f, 39.0f, interestArousal/10.0f))
				{
					if (gAvoid == 0.0f)
					{
						saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
						debugString += " Reset ";
						saccadeOffsetCounter = 0.0f;
					//SuperController.LogError("Saccade Start");
					}
				}
                bool sChange = false;
                if (saccade <= 6.46f && playerHeadToHead > closeFaceDistance * 2.0f && sChange == false)
                {
                    //up right
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(tempFloat, 0.0f), saccadeOffset.y + Random.Range(0.0f, tempFloat / 200.0f), 0.0f);
                    sChange = true;
					debugString += " |UR ";
					saccadeClock = saccadeClock / 2.0f;
                }
                if (saccade <= 7.45f && playerHeadToHead > closeFaceDistance * 2.0f && sChange == false)
                {
                    //up left
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(-tempFloat, 0.0f), saccadeOffset.y - Random.Range(0.0f, tempFloat / 200.0f), 0.0f);
                    sChange = true;
					debugString += " |UL ";
					saccadeClock = saccadeClock / 2.0f;
                }
                if (saccade <= 7.79f && sChange == false)
                {
                    //down right
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(tempFloat*Random.Range(0.8f,1.2f), 0.0f), saccadeOffset.y + Random.Range(0.0f, -tempFloat*Random.Range(0.8f,1.2f)), 0.0f);
                    sChange = true;
					debugString += " |DR ";
                }
                if (saccade <= 7.89f && sChange == false)
                {
                    //down left
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(-tempFloat*Random.Range(0.8f,1.2f), 0.0f), saccadeOffset.y + Random.Range(0.0f, -tempFloat*Random.Range(0.8f,1.2f)), 0.0f);
                    sChange = true;
					debugString += " |DL ";
                }
                if ((saccade <= 15.54f || ((playerHeadToHead < personalSpaceDistance / 2.0f || saccadeOffset.x < 0.0f) && saccade <= 31.08f)) && sChange == false)
                {
                    //right
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(tempFloat*Random.Range(0.5f,1.0f), 0.0f), saccadeOffset.y, 0.0f);
                    sChange = true;
					debugString += " |R ";
					if (interestArousal > 6.0f && saccadeOffset.x < 0.0f)
					{
						saccadeClock = saccadeClock / 2.0f;
					}
                }
                if ((saccade <= 16.8f || ((playerHeadToHead < personalSpaceDistance / 2.0f || saccadeOffset.x > 0.0f) && saccade <= 33.6f))  && sChange == false)
                {
                    //left
                    saccadeOffset = new Vector3(saccadeOffset.x + Random.Range(-tempFloat*Random.Range(0.5f,1.0f), 0.0f), saccadeOffset.y, 0.0f);
                    sChange = true;
					debugString += " |L ";
					if (interestArousal > 6.0f && saccadeOffset.x > 0.0f)
					{
						saccadeClock = saccadeClock / 2.0f;
					}
                }
                if (((saccade <= 17.69f && playerHeadToHead > personalSpaceDistance / 2.0f) || (saccadeOffset.y < 0.0f && saccade <= 22.0f)) && sChange == false)
                {
                    //up
                    saccadeOffset = new Vector3(saccadeRandom, Random.Range(saccadeOffset.x, saccadeOffset.x + (tempFloat / 100.0f)), 0.0f);
                    sChange = true;
					debugString += " |U ";
					saccadeClock = saccadeClock / 2.0f;
                }
                if ((saccade <= 20.38f || (saccade <= Mathf.Lerp(20.38f,50.0f,interestArousal/10.0f) && mainInterest == "Face")) && sChange == false)
                {
                    //down
                    saccadeOffset = new Vector3(saccadeOffset.x, Random.Range(0.0f, saccadeOffset.y-tempFloat*Random.Range(0.8f,1.2f)), 0.0f);
                    sChange = true;
					debugString += " |D ";
                }
				if (Random.Range(0.0f,1000.0f * uiBlinkSpeed.val) < 15.0f && sChange)
				{
					if (tempFloat >= 5.0f * uiSaccadeAmount.val && eyeClock > (2.45f * uiBlinkSpeed.val))
					{
						if (currentEye != "Closed")
						{
						eyesSM.Switch(eBlink);
						eyeClock = 0.0f;
						debugString += " Sml Blnk ";
						}
					}
					else
					{
						if (tempFloat >= 10.0f * uiSaccadeAmount.val && eyeClock > (1.35f * uiBlinkSpeed.val))
						{
							if (currentEye != "Closed")
							{
							eyesSM.Switch(eBlink);
							eyeClock = 0.0f;
							debugString += " Big Blnk";
							}
						}
					}
				}
				saccadeOffset.x = Mathf.Lerp(saccadeOffset.x / 10.0f, saccadeOffset.x, Mathf.Min(playerHeadToHead, 1.0f));
				if (mainInterest == "Face" && Mathf.Abs(saccadeOffset.x) > 5.0f && Random.Range(0.0f,100.0f) > 66.0f)
				{
					saccadeOffset.x = 0.0f;
					debugString += " SReset";
				}
				debugString = "Offset " + Round(Vector3.Distance(new Vector3(0.0f,0.0f,0.0f), saccadeOffset)) + debugString;
				//SuperController.LogError(debugString + saccadeOffsetCounter);
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
                if (glanceClock > Mathf.Clamp((100.0f - pExtraversion) / 10, 4.0f, 7.0f))
                {
                    glanceClock = 0.0f;
                    amGlancing = false;
					eyeUpdateClock = eyeUpdateTime + 1.0f;
					if (eyeClock >  1.5f * uiBlinkSpeed.val && currentEye != "Closed")
					{
						eyeClock = 0.0f;
						eyesSM.Switch(eBlink);
					}
                }
				//SuperController.LogError("amGlancing clock done");
				//energyAmount += 1.0f;
            }

            if (playerHeadToHead < kissingDistance &&  uiDoKiss.val) //interestKissing == false &&
            {
                interestClock -= Time.fixedDeltaTime * 5.0f;
            }
            if (playerHeadToHead > kissingDistance && interestKissing && testRun == false)
            {
                lookSM.Switch(lPlayful);
                mouthSM.Switch(mClosed);
                interestKissing = false;
				mTongueInOutTarget = 1.0f;
				interestClock = 0.0f;
            }

            if (playerHandsUsable || person2Usable)
            {
                //interestLHand = interestLHandBase;
                //interestRHand = interestRHandBase;
			}
            //currentInterestLevel = Mathf.Max(currentInterestLevel - 0.12f, 0.0f);
            //interestFace = interestFaceBase;
			//SuperController.LogError("Interest Calc Start");
			
			interestLHand += (interestLHandBase / 5000.0f) * uiInterestRate.val;
			interestRHand += (interestRHandBase / 5000.0f) * uiInterestRate.val;
			interestFace += (interestFaceBase / 5000.0f) * uiInterestRate.val;
			interestEMTarget += (interestEMTargetBase / 5000.0f) * uiInterestRate.val;
			
			interestFace -= Mathf.Lerp(0.001f,0.004f,interestArousal/10.0f) * uiInterestRate.val;
			interestLHand -= Mathf.Lerp(0.001f,0.005f,interestValence/10.0f) * uiInterestRate.val;
			interestRHand -= Mathf.Lerp(0.001f,0.005f,interestValence/10.0f) * uiInterestRate.val;
			interestEMTarget -= Mathf.Lerp(0.003f,0.001f,interestValence/10.0f) * uiInterestRate.val;

            //Face
            if ((mainInterest == "Face" && mainOld == "Face") && mainClock > gDuration)
            {
                interestFace -= 0.1f * uiInterestRate.val;
				dbgHead += "-Timeout ";
            }
            if (playerHeadToFaceRot < eyesNonDirectAngle)
            {
				if (interestFace + headActivityBoost < 50.0f)
				{
					if (playerHeadToHead < personalSpaceDistance)
					{
						interestFace += 0.08f * uiInterestRate.val;
					}
					else
					{
						interestFace += 0.005f * uiInterestRate.val;
					}
					interestLHand -= 0.001f * uiInterestRate.val;
					interestRHand -= 0.001f * uiInterestRate.val;
				}
				else
				{
					if (playerHeadToHead < personalSpaceDistance)
					{
						interestFace += 0.002f * uiInterestRate.val;
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
				if (mainInterest == "Face"){fuzzyLock = 2.0f;}
				dbgHead += "+PLooking ";
            }
			else
			{
				interestFace -= 0.06f * uiInterestRate.val;
			}
			if (playerHeadToHead < kissingDistance && interestKissing == false && headToFaceRot < lookDirectAngle && playerHeadToFaceRot < lookDirectAngle && uiDoKiss.val && testRun == false)// && lipsTouchCount > 0.0f)
			{
				lookSM.Switch(lKissing);
				interestFace += 0.08f * uiInterestRate.val;
				interestArousal += 0.005f * uiArousalSpeed.val;
				interestValence += 0.007f * uiValenceSpeed.val;
				//energyAmount += 1.0f;
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
            if (Vector3.Distance(playerFace, eyeController.transform.position) < 1.5f && playerHeadToFaceRot < eyesNonDirectAngle)
			{
				interestFace += 0.08f * uiInterestRate.val;
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
				interestValence += 0.0008f * uiValenceSpeed.val;
                if (mainInterest == "Face"){fuzzyLock = 1.5f;}
				//energyAmount += 1.0f;
				dbgHead += "+LookAt ";
            }
			else
			{
				interestArousal -= 0.001f * uiArousalSpeed.val;
			}
            if (playerHeadToHead < closeFaceDistance * 3.0f)
            {
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
                interestArousal += 0.0002f * uiArousalSpeed.val;
                interestValence += 0.0002f * uiValenceSpeed.val;
                if (mainInterest == "Face"){fuzzyLock = 07.0f;}
				playerHeadInteract = true;
				//energyAmount += 1.0f;
				dbgHead += "+Interact ";
            }
            if (playerHeadMovement && headToFaceRot < lookPeripheralAngle)
            {
                interestFace += 0.05f * uiInterestRate.val;
                //interestValence += 0.25f;
                if (mainInterest == "Face"){fuzzyLock = 5.5f;}
				dbgHead += "+Move ";
            }
            if (playerHeadMovement == false)
            {
                interestFace -= 0.014f * uiInterestRate.val;
				dbgHead += "-NoMove ";
            }
            if (Mathf.Abs(personChestToHead) > lookNoAwarenessAngle)// && playerHeadToHead > personalSpaceDistance)
            {
                interestFace -= 0.52f * uiInterestRate.val;
                if (mainInterest == "Face"){fuzzyLock = 5.0f;}
				dbgHead += "-HighAngle ";
            }
            if (playerHeadToHead < personalSpaceDistance)
            {
                interestFace += 0.015f * uiInterestRate.val;
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
					interestFace -= 0.05f * uiInterestRate.val;
					dbgHead += "-AwareDist ";
				}
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
                if (playerLHandToHead < closeFaceDistance)// && playerLHandToHead > interactionDistance)
                {
                    interestLHand += 0.02f * uiInterestRate.val;
					interestFace -= 0.01f * uiInterestRate.val;
                    interestArousal += 0.00005f * uiArousalSpeed.val;
                    interestValence += 0.00015f * uiValenceSpeed.val;
					//energyAmount += 1.0f;
                    dbgLHand += "+FaceContact ";
                }
				if (playerHeadToFaceRot < lookDirectAngle && headToFaceRot < lookDirectAngle && playerHeadToHead < personalSpaceDistance && interestLHand + lHandActivityBoost > 40.0f)
				{
					interestLHand -= 0.03f * uiInterestRate.val;
					dbgLHand += "-EyeToEye ";
				}
				if (playerHeadToFaceRot < eyesNonDirectAngle)
				{
					interestLHand -= 1.00f * uiInterestRate.val;
					dbgLHand += "-PLookFace ";
				}

                if (playerLHandToHead < interactionDistance || playerLHandToLBreast < interactionDistance || playerLHandToRBreast < interactionDistance || playerLHandToPelvis < interactionDistance * 1.3f)
                {
					if (playerLHandMovement)
					{
                    interestLHand += 0.15f * uiInterestRate.val;
                    interestArousal += 0.0003f * uiArousalSpeed.val;
                    interestValence += 0.00055f * uiValenceSpeed.val;
					}
					else
					{
                    interestLHand -= 0.05f;
					}
					if (playerLHandToPelvis < interactionDistance * 1.3f && vagTouchCount > 0.0f && testRun == false)
					{
						dbgLHand += "+Sex ";
						interestArousal += 0.01f * uiArousalSpeed.val;
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
                    if (mainInterest == "LHand"){fuzzyLock = 3.5f;}
					gHeadSpeed = 0.5f;
					playerLHandInteract = true;
					//energyAmount += 1.0f;
                    dbgLHand += "+Interact ";
                }
                if (playerLHandToLHand < interactionDistance)
                {
                    mLHandFistTarget = Random.Range(0.5f, 0.8f);
                }
                if (playerLHandToRHand < interactionDistance)
                {
                    mRHandFistTarget = Random.Range(0.5f, 0.8f);
                }
                if (playerLHandMovement && headToLHandRot < lookNoAwarenessAngle)
                {
                    interestLHand += 0.07f * (playerLHandTimeout / movementMaxTimeout) * uiInterestRate.val;
                    dbgLHand += "+Move ";
                    if (headToLHandRot > lookPeripheralAngle)
                    {
                        interestLHand += 0.01f * (playerLHandTimeout / movementMaxTimeout) * uiInterestRate.val;
                        dbgLHand += "+MoveNoVis ";
                    }
                }
                if (playerLHandMovement == false)
                {
                    interestLHand -= 0.035f * (1.0f - (playerLHandTimeout / movementMaxTimeout)) * uiInterestRate.val;
                    dbgLHand += "-NoMove ";
                }
                if (playerToPLHand < lookDirectAngle && headToLHandRot < lookNoAwarenessAngle)
                {
                    interestLHand += 0.33f * uiInterestRate.val;
                    if (mainInterest == "LHand"){fuzzyLock = 0.5f;}
                    dbgLHand += "+PLookAt ";
                }
                if (headToLHandRot > lookNoAwarenessAngle)
                {
                    interestLHand -= 0.1f * uiInterestRate.val;
                    if (mainInterest == "LHand"){fuzzyLock = 5.0f;}
                    dbgLHand += "-HighAngle ";
                }
                if (playerLHandToHead < personalSpaceDistance)
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
                    interestLHand -= 0.02f * uiInterestRate.val;
                    dbgLHand += "-Repeat ";
                }
				
                if (playerLHandToHead > playerHeadToHead * 1.5f)
                {
                    interestLHand -= 0.02f * uiInterestRate.val;
                    dbgLHand += "-HeadCloser ";
                }
                if (playerLHandToHead > personalSpaceDistance && playerLHandToHead < backgroundDistance)
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
                if (playerRHandToHead < closeFaceDistance)// && playerRHandToHead > interactionDistance)
                {
                    interestRHand += 0.02f * uiInterestRate.val;
					interestFace -= 0.01f * uiInterestRate.val;
                    interestArousal += 0.00005f * uiArousalSpeed.val;
                    interestValence += 0.00015f * uiValenceSpeed.val;
					//energyAmount += 1.0f;
                    dbgRHand += "+FaceContact ";
                }
				if (playerHeadToFaceRot < lookDirectAngle && headToFaceRot < lookDirectAngle && playerHeadToHead < personalSpaceDistance && interestRHand + rHandActivityBoost > 40.0f)
				{
					interestRHand -= 0.03f * uiInterestRate.val;
					dbgRHand += "-EyeToEye ";
				}
				if (playerHeadToFaceRot < eyesNonDirectAngle)
				{
					interestRHand -= 1.00f * uiInterestRate.val;
					dbgRHand += "-PLookFace ";
				}

                if (playerRHandToHead < interactionDistance || playerRHandToLBreast < interactionDistance || playerRHandToRBreast < interactionDistance || playerRHandToPelvis < interactionDistance * 1.3f)
                {
					if (playerRHandMovement)
					{
                    interestRHand += 0.15f * uiInterestRate.val;
                    interestArousal += 0.0003f * uiArousalSpeed.val;
                    interestValence += 0.00055f * uiValenceSpeed.val;
					}
					else
					{
                    interestRHand -= 0.05f * uiInterestRate.val;
					}
					if (playerRHandToPelvis < interactionDistance * 1.3f && vagTouchCount > 0.0f)
					{
						dbgRHand += "+Sex ";
						interestArousal += 0.01f * uiArousalSpeed.val;
						if (playerRHandMovement && interestArousal > 5.0f && currentLook != "Feel" && currentLook != "Sex" && testRun == false)
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
                    if (mainInterest == "RHand"){fuzzyLock = 3.5f;}
					gHeadSpeed = 0.5f;
					playerRHandInteract = true;
					//energyAmount += 1.0f;
                    dbgRHand += "+Interact ";
                }
                if (playerRHandToRHand < interactionDistance)
                {
                    mRHandFistTarget = Random.Range(0.5f, 0.8f);
                }
                if (playerRHandToRHand < interactionDistance)
                {
                    mRHandFistTarget = Random.Range(0.5f, 0.8f);
                }
                if (playerRHandMovement && headToRHandRot < lookNoAwarenessAngle)
                {
                    interestRHand += 0.07f * (playerRHandTimeout / movementMaxTimeout) * uiInterestRate.val;
                    dbgRHand += "+Move ";
                    if (headToRHandRot > lookPeripheralAngle)
                    {
                        interestRHand += 0.01f * (playerRHandTimeout / movementMaxTimeout) * uiInterestRate.val;
                        dbgRHand += "+MoveNoVis ";
                    }
                }
                if (playerRHandMovement == false)
                {
                    interestRHand -= 0.035f * (1.0f - (playerRHandTimeout / movementMaxTimeout)) * uiInterestRate.val;
                    dbgRHand += "-NoMove ";
                }
                if (playerToPRHand < lookDirectAngle && headToRHandRot < lookNoAwarenessAngle)
                {
                    interestRHand += 0.33f * uiInterestRate.val;
                    if (mainInterest == "RHand"){fuzzyLock = 0.5f;}
                    dbgRHand += "+PLookAt ";
                }
                if (headToRHandRot > lookNoAwarenessAngle)
                {
                    interestRHand -= 0.1f * uiInterestRate.val;
                    if (mainInterest == "RHand"){fuzzyLock = 5.0f;}
                    dbgRHand += "-HighAngle ";
                }
                if (playerRHandToHead < personalSpaceDistance)
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
				
                if (playerRHandToHead > playerHeadToHead * 1.5f)
                {
                    interestRHand -= 0.02f * uiInterestRate.val;
                    dbgRHand += "-HeadCloser ";
                }
                if (playerRHandToHead > personalSpaceDistance && playerRHandToHead < backgroundDistance)
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
                if (playerHeadToHead < personalSpaceDistance / 2.0f && playerHeadToHead - 1.0f < playerPelvisToHead)
                {
                    interestPelvis -= 0.2f * uiInterestRate.val;
                    if (mainInterest == "Pelvis"){fuzzyLock = 2.5f;}
					dbgPenis += "-HeadCloser ";
                }
                if (playerTipToPelvis < interactionDistance * 1.0f && mainInterest != "Tip" && playerHeadToHead > personalSpaceDistance/2.0f)
                {
                    //interestArousal += 2.0f;
                    interestPelvis += 0.1f * uiInterestRate.val;
                    interestTip += 0.1f * uiInterestRate.val;
                    if (mainInterest == "Pelvis"){fuzzyLock = 2.5f;}
					//energyAmount += 1.0f;
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
				if (playerHeadToFaceRot < lookDirectAngle && playerTipToHead > closeFaceDistance)
				{
					interestPelvis -= 0.1f * uiInterestRate.val;
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
                    interestTip -= 0.02f * uiInterestRate.val;
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
                    if (mainInterest == "Penis"){fuzzyLock = 2.5f;}
					dbgPenis += "-NoMove ";
                }
                if (playerTipToHead < personalSpaceDistance)
                {
                    interestTip += 0.03f * uiInterestRate.val;
					dbgPenis += "+PSpace ";
                }
                if (playerTipToHead < playerLHandToHead && playerTipToHead < playerRHandToHead && (playerTipToPelvis < interactionDistance * 2 || playerTipToLBreast < interactionDistance || playerTipToRBreast < interactionDistance) && person2IsMale)
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
				if (playerHeadToFaceRot < lookDirectAngle && playerTipToHead > closeFaceDistance)
				{
					interestTip -= 0.1f * uiInterestRate.val;
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
                    interestTip -= (pStableness) / 10.0f;
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
					if (playerHeadToFaceRot < lookDirectAngle && amGlancing == false && gAvoid == 0.0f)
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
			//SuperController.LogError("Target Object Done");
			
			if (interestArousal > 6.0f && interestValence < 5.0f)
			{
				interestArousal -= 0.00025f / uiArousalSpeed.val;
			}
			if (interestValence > 8.0f && interestArousal < 6.0f)
			{
				interestValence -= 0.00065f / uiValenceSpeed.val;
			}
			
				interestFace = Mathf.Clamp(interestFace, (interestFaceBase/2.0f) * uiHeadInterest.val, (maxInterestLevel) * uiHeadInterest.val);
				interestLHand = Mathf.Clamp(interestLHand, (interestLHandBase/2.0f) * uiLHandInterest.val, (maxInterestLevel - 1.0f) * uiLHandInterest.val);
				interestRHand = Mathf.Clamp(interestRHand, (interestRHandBase/2.0f) * uiRHandInterest.val, (maxInterestLevel - 1.0f) * uiRHandInterest.val);
				interestPelvis = Mathf.Clamp(interestPelvis, (interestPelvisBase/2.0f) * uiPenisInterest.val, (maxInterestLevel + 1.0f) * uiPenisInterest.val);
				interestTip = Mathf.Clamp(interestTip, (interestTipBase/2.0f) * uiPenisInterest.val, (maxInterestLevel + 4.0f) * uiPenisInterest.val);
				interestEMTarget = Mathf.Clamp(interestEMTarget, (interestEMTargetBase/2.0f) * uiObjectInterest.val, (maxInterestLevel + 5.0f) * uiObjectInterest.val);
				
				
			
			//SuperController.LogError("Interest Calc Done");
			
            if (interestClock <= 0.0f && amGlancing == false)
            {

				eyesNonDirectClock = 0.0f;
				twistTarget = Random.Range(-30.0f,30.0f);
				//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
				//float leftright = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
				
				if (headLeftRight > lookDirectAngle)
				{
					twistTarget = Random.Range(10.0f,-70.0f);
				}
				if (headLeftRight < -lookDirectAngle)
				{
					twistTarget = Random.Range(-10.0f,70.0f);
				}
				if (Random.Range(0.0f,100.0f) < uiIdleChance.val || (Mathf.Abs(lElbowActual) < -40.0f && Random.Range(0.0f,100.0f) < uiIdleChance.val * 2.0f))
				{
					lElbowTarget = Random.Range(10.0f,-120.0f);
				}
				if (Random.Range(0.0f,100.0f) < uiIdleChance.val || (Mathf.Abs(rElbowActual) > 40.0f && Random.Range(0.0f,100.0f) < uiIdleChance.val * 2.0f))
				{
					rElbowTarget = Random.Range(-10.0f,120.0f);
				}
				
				headLastUpDown = headUpDown;
				headLastLeftRight = headLeftRight;
				
				if (currentLook == "Bored" || currentLook == "BlowJob" || currentLook == "Kissing")
				{
					twistTarget = 0.0f;
					lElbowTarget = 0.0f;
					rElbowTarget = 0.0f;
				}
				if (currentLook == "Intense")
				{
					lElbowTarget = Random.Range(10.0f,-20.0f);
					rElbowTarget = Random.Range(-10.0f,20.0f);
				}
				
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
				/*if (Random.Range(0.0f,100.0f) > 50.0f)
				{
					lElbowX = -130.0f;
					lShoulderY = 70.0f;
					lShoulderX = -75.0f;
				}
				else
				{
					lElbowX = -10.0f;
					lShoulderY = 0.0f;
					lShoulderX = -50.0f;
				}
				if (Random.Range(0.0f,100.0f) > 50.0f)
				{
					rShoulderY = -70.0f;
					rShoulderX = 75.0f;
					rElbowX = 130.0f;
				}
				else
				{
					rShoulderY = 0.0f;
					rShoulderX = 50.0f;
					rElbowX = 10.0f;
				}
				

				lArmController.jointRotationDriveXTarget = lShoulderX;
				lArmController.jointRotationDriveYTarget = lShoulderY;
				rArmController.jointRotationDriveXTarget = rShoulderX;
				rArmController.jointRotationDriveYTarget = rShoulderY;
				
				
				lElbowController.jointRotationDriveXTarget = lElbowX;
				lElbowController.jointRotationDriveYTarget = lElbowY;
				rElbowController.jointRotationDriveXTarget = rElbowX;
				rElbowController.jointRotationDriveYTarget = rElbowY;
				*/
				
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
				if (interestRepeating == false)
				{
					mainValue = 0.0f;
					secondValue = 0.0f;
					tempFloat = 0.0f;
					tempFloat2 = Vector3.Dot(headController.followWhenOff.position - playerHeadTransform.position, headController.followWhenOff.forward);
					//testString = Mathf.Abs(Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward)).ToString();
					if (Mathf.Abs(Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookNoAwarenessAngle)
					{
						if (interestFace + headActivityBoost > secondValue)// || playerHeadToHead < closeFaceDistance * 1.75f)
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
										if ((currentLook == "Casual" || currentLook == "Bored" || currentLook == "Daydream") && morphMouthAction == false)
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

									if (playerHeadToHead < closeFaceDistance * 1.75f)
									{
										//mainValue = interestFace + headActivityBoost + 30.0f;
									}
								}
								//}
							}
							else
							{
								if (secondInterest != "Face" && secondOld != "Face")
								{
									secondOld = secondInterest;
									secondSwitch = true;
									//mouthSM.Switch(mSmile);
								}
								secondInterest = "Face";
								secondValue = interestFace + headActivityBoost;
							}
						}
					}
					if (Mathf.Abs(Vector3.Angle(playerLHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle || interestLHand + lHandActivityBoost > interestFace + headActivityBoost)
					{
						if (interestLHand + lHandActivityBoost > secondValue && (playerHandsUsable || (person2Usable && usePerson2)))
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
										mainValue = interestLHand + lHandActivityBoost;
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
							}
						}
					}
					if (Mathf.Abs(Vector3.Angle(playerRHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward)) < lookPeripheralAngle || interestRHand + rHandActivityBoost > interestFace + headActivityBoost)
					{
						if (interestRHand + rHandActivityBoost > secondValue && (playerHandsUsable || (person2Usable && usePerson2)))
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
										mainValue = interestRHand + rHandActivityBoost;
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
							}
						}
					}
					if (interestPelvis > secondValue && person2Usable && usePerson2 && interestPelvis > 15.0f)
					{
						if (interestPelvis > mainValue && playerHeadToHead > personalSpaceDistance/2.0f)
						{
							if (mainInterest != "Pelvis")
							{
								mainOld = mainInterest;
								mainSwitch = true;
								interestRepeat = 0.0f;
								if (interestArousal > 5.0f && lookAction == false && testRun == false)
								{
									lookSM.Switch(lPlayful);
								}
							}
							mainInterest = "Pelvis";
							mainValue = interestPelvis;
						}
						else
						{
							if (secondInterest != "Pelvis" && secondOld != "Pelvis")
							{
								secondOld = secondInterest;
								secondSwitch = true;
								if (lookAction == false && testRun == false)
								{
									lookSM.Switch(lInquisitive);
								}
							}
							secondInterest = "Pelvis";
							secondValue = interestPelvis;
						}
					}
					if ((interestTip > secondValue) && interestTip > 30.0f)// || ((playerTipToHead < closeFaceDistance || playerTipToPelvis < closeFaceDistance) && playerHeadToHead > closeFaceDistance * 2.0f)) && usePerson2)
					{
						if (interestTip > mainValue && playerHeadToHead > personalSpaceDistance/2.0f)// || (playerTipToHead < closeFaceDistance || playerTipToPelvis < closeFaceDistance && playerHeadToHead > closeFaceDistance))
						{
							if (mainInterest != "Tip")
							{
								mainOld = mainInterest;
								mainSwitch = true;
								interestRepeat = 0.0f;
								//lookSM.Switch(lIntense);
							}
							mainInterest = "Tip";
							mainValue = interestTip;
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
								if (interestArousal > 5.0f && lookAction == false && testRun == false)
								{
									lookSM.Switch(lPlayful);
								}
							}
							mainInterest = "Target";
							mainValue = interestEMTarget;
						}
						else
						{
							if (secondInterest != "Target" && secondOld != "Target")
							{
								secondOld = secondInterest;
								secondSwitch = true;
								if (lookAction == false && testRun == false)
								{
									lookSM.Switch(lInquisitive);
								}
							}
							secondInterest = "Target";
							secondValue = interestEMTarget;
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
							gazeAdjust = 5.0f;
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
							mainValue = 100;
							secondOld = secondInterest;
							secondInterest = "Face";
							secondValue = 60;
						}
					if (playerRHandToHead < closeFaceDistance)
						{
							if (mainInterest != "RHand")
							{
								mainOld = mainInterest;
							}
							mainInterest = "RHand";
							mainValue = 100;
							secondOld = secondInterest;
							secondInterest = "Face";
							secondValue = 60;
						}
						
					if (secondInterest == "RandomR" || secondInterest == "RandomL" || secondInterest == "RandomF" || secondInterest == "RandomU")
					{
						if (Random.Range(0.0f,100.0f) > 85.0f)
						{
							if (Random.Range(0.0f,100.0f) > 90.0f)
							{
								secondInterest = "RandomF";
							}
							else
							{
								if (Random.Range(0.0f,100.0f) > 50.0f)
								{
								secondInterest = "RandomL";
								}
								else
								{
								secondInterest = "RandomR";
								}
							}
						}
					}
					
					if (secondInterest == "RHand" && interestRHand + rHandActivityBoost < 10.0f)
					{
						if (Random.Range(0.0f,100.0f) > 5.0f)
						{
							secondInterest = "RandomR";
						}
						else
						{
							secondInterest = "RandomF";
						}
					}
					
					if (secondInterest == "LHand" && interestLHand + lHandActivityBoost < 10.0f)
					{
						if (Random.Range(0.0f,100.0f) > 5.0f)
						{
							secondInterest = "RandomL";
						}
						else
						{
							secondInterest = "RandomF";
						}
					}
					
					if ((mainOld == "RandomR" || mainOld == "RandomL" || mainOld == "RandomF") && mainInterest == "Face")
					{
						if (playerHeadToHead > personalSpaceDistance && interestValence < 5.0f && Random.Range(0.0f,100.0f) > 95.0f)
						{
							mainInterest = mainOld;
							//mainOld = "Face";
						}
					}
					

					if (mainInterest != "Tip" && playerTipToHead < closeFaceDistance * 3.0f && interestArousal > 7.0f)
					{
						mainInterest = "Tip";
						interestClock = 0.0f;
					}
					
					if (vagTouchCount > 0.0f && currentLook == "Sex" && currentEye != "Closed")
					{
						if ((mainOld == "Face" || mainOld == "Pelvis") && Random.Range(0.0f,100.0f) > 20.0f)
						{
							mainOld = mainInterest;
							mainInterest = "RandomU";
						}
						else
						{
							if (interestArousal > 9.5f && interestValence > 9.5f && mainInterest != "Pelvis" )
							{
								mainOld = mainInterest;
								mainInterest = "Pelvis";
							}
							if (interestArousal < 9.5f && interestValence < 9.5f && mainInterest != "Face" )
							{
								mainOld = mainInterest;
								mainInterest = "Face";
							}
						}
						currentInterest = mainInterest;
					}
					
					//SuperController.LogError("Main interest mood and look");
					//currentInterestLevel = 0.0f;
					if (mainInterest == "Pelvis")
					{
						interestArousal += (0.05f * (0.1f + (pExtraversion / 100.0f))) * uiArousalSpeed.val;
						interestValence += (0.012f * (0.1f + (pAgreeableness / 100.0f))) * uiValenceSpeed.val;
						currentInterest = "Pelvis";
						currentInterestLevel = interestPelvis;
						playerInterest += 2.0f;
						if (lookAction == false && testRun == false)
						{
							if (interestArousal + interestValence < 5)
							{
								lookSM.SwitchRandom(new State[] {
												lCasual,
												lInquisitive
											});
							}
							else
							{
								if (interestArousal + interestValence > 15)
								{
									lookSM.SwitchRandom(new State[] {
													lCasual,
													lCasual,
													lInquisitive,
													lPlayful
												});
								}
								else
								{
									lookSM.SwitchRandom(new State[] {
													lIntense,
													lIntense,
													lPlayful,
													lPlayful,
													lInquisitive,
													lInquisitive,
													lInquisitive
												});
								}
							}
						}
					}
					if (mainInterest == "Tip")
					{
						interestArousal += (0.2f * (0.1f + (pExtraversion / 100.0f))) * uiArousalSpeed.val;
						interestValence += (0.035f * (0.1f + (pAgreeableness / 100.0f))) * uiValenceSpeed.val;
						currentInterest = "Tip";
						currentInterestLevel = interestTip;
						playerInterest += 5.0f;
						if (lookAction == false)
						{
							if (playerTipToPelvis < interactionDistance * 1.5f && playerHeadToHead > personalSpaceDistance/2.0f && vagTouchCount > 0.0f && uiDoSex.val && testRun == false)
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
									if (lookAction == false && testRun == false)
									{
										if (interestArousal + interestValence < 5)
										{
											lookSM.SwitchRandom(new State[] {
															lCasual,
															lInquisitive
														});
										}
										else
										{
											if (interestArousal + interestValence > 15)
											{
												lookSM.SwitchRandom(new State[] {
																lCasual,
																lCasual,
																lInquisitive,
																lPlayful
															});
											}
											else
											{
												lookSM.SwitchRandom(new State[] {
																lIntense,
																lIntense,
																lPlayful,
																lPlayful,
																lInquisitive,
																lInquisitive,
																lInquisitive
															});
											}
										}
									}
								}
							}
						}
					}
					if (mainInterest == "RHand")
					{
						interestArousal += (0.005f * (0.1f + ((100.0f - pExtraversion) / 100.0f))) * uiArousalSpeed.val;
						interestValence += (0.012f * (0.1f + (pAgreeableness / 100.0f))) * uiValenceSpeed.val;
						currentInterest = "RHand";
						currentInterestLevel = interestRHand;
						playerInterest += 1.0f;
						if (lookAction == false && (mainInterest != mainOld || Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false)
						{
							if (playerRHandToHead < interactionDistance || playerRHandToLBreast < interactionDistance || playerRHandToRBreast < interactionDistance || playerRHandToPelvis < interactionDistance * 1.3f)
							{
								lookSM.SwitchRandom(new State[] {
												lInquisitive,
												lFeel,
												lFeel,
												lFeel,
												lPlayful
											});
							}
							else
							{
								lookSM.SwitchRandom(new State[] {
												lInquisitive,
												lInquisitive,
												lCasual,
												lPlayful
											});
							}
						}
					}
					if (mainInterest == "LHand")
					{
						interestArousal += (0.005f * (0.1f + ((100.0f - pExtraversion) / 100.0f))) * uiArousalSpeed.val;
						interestValence += (0.012f * (0.1f + (pAgreeableness / 100.0f))) * uiValenceSpeed.val;
						currentInterest = "LHand";
						currentInterestLevel = interestLHand;
						playerInterest += 1.0f;
						if (lookAction == false && (mainInterest != mainOld || Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false)
						{
							if (playerLHandToHead < interactionDistance || playerLHandToLBreast < interactionDistance || playerLHandToRBreast < interactionDistance || playerLHandToPelvis < interactionDistance * 2.0f)
							{
								lookSM.SwitchRandom(new State[] {
												lInquisitive,
												lFeel,
												lFeel,
												lFeel,
												lPlayful
											});
							}
							else
							{
								lookSM.SwitchRandom(new State[] {
												lInquisitive,
												lInquisitive,
												lCasual,
												lPlayful
											});
							}
						}
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
							}
							else
							{
								currentInterest = "Face";
								if (interestRepeat > 2.0f && interestRepeat < 5.0f)
								{
									if (Random.Range(0.0f, 100.0f) > 90.0f && playerPelvisToHead > personalSpaceDistance && interestArousal > 8.0f)
									{
										currentInterest = "Pelvis";
									}
								}
								if (interestRepeat > 1.0f && interestRepeat < 3.0f)
								{
									if (Random.Range(0.0f, 100.0f) > 80.0f && playerHeadToHead > personalSpaceDistance/2.0f && interestValence > 5.0f)
									{
										currentInterest = "Chest";
									}
								}
							}
							currentInterestLevel = interestFace;
						}
						else
						{
							currentInterest = "Target";
							currentInterestLevel = interestEMTarget;
						}
						if (lookAction == false && (mainInterest != mainOld || Random.Range(0.0f,100.0f) < uiExpressionChance.val) && testRun == false)
						{
							if (playerHeadToPelvis < interactionDistance && playerHeadMovement && mainInterest == "Face" && playerHeadToHead > personalSpaceDistance/2.0f && uiDoSex.val)
							{
								lookSM.Switch(lSex);
							}
							else
							{
								if (interestArousal + interestValence < 7.0f)
								{
									if (playerHeadToFaceRot < lookDirectAngle)
									{
										lookSM.SwitchRandom(new State[] {
														//lBored,
														lCasual,
														lCasual,
														lCasual,
														lInquisitive,
														lInquisitive
													});
									}
									else
									{
										lookSM.SwitchRandom(new State[] {
														lBored,
														lDayDream,
														lDayDream,
														lCasual
													});
									}
								}
								else
								{
									if (interestArousal + interestValence < 15.0f)
									{
										lookSM.SwitchRandom(new State[] {
														lInquisitive,
														lInquisitive,
														lInquisitive,
														lInquisitive,
														lIntense,
														lPlayful
													});
									}
									else
									{
										if (tempFloat == 2.0f && interestArousal > 8.0f)
										{
											lookSM.Switch(lSex);
										}
										else
										{
											if (tempFloat > 0.0f)
											{
												lookSM.Switch(lFeel);
											}
											else
											{
												lookSM.SwitchRandom(new State[] {
																lFeel,
																lPlayful,
																lPlayful,
																lPlayful,
																lPlayful,
																lPlayful,
																lPlayful,
																lIntense,
																lIntense,
																lIntense
															});
											}
										}
									}
								}
							}
						}
						playerInterest += 0.5f;
					}


					if (((interestFace + headActivityBoost <= 60.0f || playerHeadToHead > personalSpaceDistance) && interestLHand + lHandActivityBoost <= 40.0f && interestRHand + rHandActivityBoost <= 40.0f && interestPelvis <= 40.0f && interestTip <= 40.0f && interestEMTarget <= 40.0f) || mainInterest == "RandomF" || mainInterest == "RandomU" || mainInterest == "RandomL" || mainInterest == "RandomR" || ((currentLook == "Bored" || currentLook == "Daydream") && Random.Range(0.0f,100.0f) > 40.0f && interestArousal < 5.0f))
					{
						interestArousal -= (0.2f * (0.1f + ((100.0f - pStableness) / 100.0f))) / uiArousalSpeed.val;
						interestValence -= (0.065f * (0.1f + ((100.0f - pAgreeableness) / 100.0f))) / uiValenceSpeed.val;
						if ((currentLook == "Bored" || currentLook == "Daydream") && interestArousal > 5.5f && testRun == false)
						{
							lookSM.SwitchRandom(new State[] {
											lPlayful,
											lInquisitive,
											lCasual,
										});
						}
						else
						{
							if (lookAction == false && playerHeadToHead > personalSpaceDistance && interestValence < 5.0f && playerInterest < 50.0f && testRun == false)
							{
								lookSM.SwitchRandom(new State[] {
												lBored,
												lBored,
												lBored,
												lCasual,
												lDayDream,
											});
							}
						}
						if (Random.Range(0.0f, 100.0f) < 1.0f + (interestFace + headActivityBoost / 10.0f) && currentLook != "Sex")
						{
							currentInterest = "Face";
							mainInterest = currentInterest;
						}
						else
						{
							playerInterest -= 10.5f;
								if (Random.Range(0.0f, 100.0f) < 5.0f)
								{
									currentInterest = "RandomF";
								}
								else
								{
									if (Random.Range(0.0f, 100.0f) < 10.0f)
									{
										currentInterest = "RandomU";
									}
									else
									{
										if (Random.Range(0.0f, 100.0f) > 50.0f)
										{
											currentInterest = "RandomR";
										}
										else
										{
											currentInterest = "RandomL";
										}
									}
								}
								mainInterest = currentInterest;
						}
						if (mainInterest != currentInterest)
						{
							mainOld = mainInterest;
						}
						mainInterest = currentInterest;
					}
				}
				if (oldInterest == currentInterest)
				{
					//interestRepeat += 1.0f;
				}

                if (currentInterest != oldInterest && eyeClock > (1.45f * uiBlinkSpeed.val) && amGlancing == false && currentEye != "Closed")
                {
					if (morphEyeAction == false)
					{
						eyesSM.Switch(eBlink);
						eyeClock = 0.0f;
					}
                }
				
				if (headToEyeController > lookNoAwarenessAngle && currentInterest == "Face")
				{
					//gAvoid = 1.0f;
					mainOld = mainInterest;
					mainInterest = "RandomF";
					currentInterest = "RandomF";
					//SuperController.LogError("Out of view");
				}

				
                if (playerHeadToHead < kissingDistance && interestKissing == false && headToFaceRot < lookDirectAngle && playerHeadToFaceRot < eyesNonDirectAngle && uiDoKiss.val && (lipsTouchCount > 0.0f || playerHeadController.possessed) && testRun == false)
                {
                    lookSM.Switch(lKissing);
                }
                float clockBase = Random.Range(2.0f, 7.0f);
                if (mainSwitch || secondSwitch)
                {
                    clockBase = Random.Range(4.0f, 10.0f);
                }
				if ((mainInterest == "Tip" || mainInterest == "Pelvis") && (playerTipToHead < closeFaceDistance || playerTipToPelvis < interactionDistance))
				{
					clockBase = Random.Range(1.0f, 2.0f);
				}
                interestClock = Mathf.Clamp(clockBase * (pStableness / 50.0f), 2.0f, 12.0f) * uiInterestSpeed.val;
				//SuperController.LogError("Main Interest Done");
            }
            else
            {
				//SuperController.LogError("Interest Clock Running");
				if (interestClock > -1.0f)
				{
					interestClock -= Time.fixedDeltaTime;
				}
				if (amGlancing == false && gAvoid == 0.0f)
				{
					tempFloat = Mathf.Abs(Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					if (mainInterest == "Face" && tempFloat > lookNoAwarenessAngle)
					{
						if (interestClock == -1.0f)
						{
							mainInterest = "RandomF";
							currentInterest = "RandomF";
						}
						interestClock = -1.0f;
					}
					tempFloat = Mathf.Abs(Vector3.Angle(playerLHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					if (mainInterest == "LHand" && tempFloat > lookPeripheralAngle)
					{
						if (interestClock == -1.0f)
						{
							mainInterest = "RandomF";
							currentInterest = "RandomF";
						}
						interestClock = -1.0f;
					}

					tempFloat = Mathf.Abs(Vector3.Angle(playerRHandTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
					if (mainInterest == "RHand" && tempFloat > lookPeripheralAngle)
					{
						if (interestClock == -1.0f)
						{
							mainInterest = "RandomF";
							currentInterest = "RandomF";
						}
						interestClock = -1.0f;
					}

					if (uiObjectTarget.val != "None")
					{
						tempFloat = Mathf.Abs(Vector3.Angle(emTargetTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
						if (mainInterest == "Target" && tempFloat > lookPeripheralAngle)
						{
							if (interestClock == -1.0f)
							{
								mainInterest = "RandomF";
							currentInterest = "RandomF";
							}
							interestClock = -1.0f;
						}
					}
				}
				//SuperController.LogError("Interest Clock Counting");
            }
			//SuperController.LogError("Interest Clock Complete");
						
			if (playerHeadToFaceRot < lookDirectAngle && (playerHeadToHead < personalSpaceDistance || pExtraversion < 35.0f) && uiGazeAvoid.val)
			{
				//SuperController.LogError("Gaze Avoid start");
				if (mainInterest != "Face" && gAvoid == 0.0f && amGlancing == false && playerHeadToHead < personalSpaceDistance / 2.0f)
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
					if ( playerHeadToFaceRot < eyesNonDirectAngle)
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
						}
					}
					else
					{
						gAvoidingClock -= Time.fixedDeltaTime * (tempFloat * Mathf.Lerp(0.3f, 1.3f,interestArousal / 10.0f));
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
						if (eyeClock > 0.7f * uiBlinkSpeed.val && currentEye != "Closed")
						{
							eyesSM.Switch(eBlink);
							eyeClock = 0.0f;
						}
						if (mainInterest != gAvoidInterest)
						{
							//mainOld = mainInterest;
						}
						mainInterest = gAvoidInterest;
						interestClock = 0.0f;
					}
				}
				if (gAvoidanceClock < Mathf.Lerp(1.5f * uiGazeLookTime.val,13.0f * uiGazeLookTime.val,pExtraversion/100.0f) && gAvoid == 0.0f)
				{
					if (playerHeadToHead <= backgroundDistance && amGlancing == false && mEyesClosedLeftTarget < 0.5f && currentLook != "Intense" && interestValence < 9.5f)
					{
						if (mainInterest == "Face" && Vector3.Distance(new Vector3(0.0f,0.0f,0.0f),saccadeOffset) < 15.0f * uiSaccadeAmount.val)
						{
							gAvoidanceClock += Time.fixedDeltaTime * (2.0f * (1.0f-(interestArousal / 10.0f)));
						}
					}
					if ((amGlancing == true || Vector3.Distance(new Vector3(0.0f,0.0f,0.0f),saccadeOffset) > 15.0f * uiSaccadeAmount.val || mEyesClosedLeftTarget > 0.5f) && gAvoidanceClock > 0.0f)
					{
						gAvoidanceClock -= Time.fixedDeltaTime / 10.0f;
					}
				}
				else
				{
					if (gAvoidanceClock >= Mathf.Lerp(1.5f * uiGazeLookTime.val,13.0f * uiGazeLookTime.val,pExtraversion/100.0f) && gAvoid == 0.0f && Random.Range(0.0f,100.0f) > Mathf.Lerp(99.0f,99.99f,interestArousal/10.0f))
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
							}
						}
						//mouthSM.Switch(mBigSmile);
						if (secondInterest == "RandomL" || secondInterest == "RandomR")
						{
							if (secondInterest == "RandomL")
							{
								if (lookAwaySide == "right")
								{
									eyesSM.Switch(eBlink);
									eyeClock = 0.0f;
								}
								lookAwaySide = "left";
							}
							else
							{
								if (lookAwaySide == "left")
								{
									eyesSM.Switch(eBlink);
									eyeClock = 0.0f;
								}
								lookAwaySide = "right";
							}
						}
						else
						{
							if (Random.Range(0.0f, Mathf.Lerp(35.0f, 100.0f, interestValence/10.0f)) < 32.0f)
							{
								if (lookAwaySide == "right")
								{
									eyesSM.Switch(eBlink);
									eyeClock = 0.0f;
								}
								lookAwaySide = "left";
							}
							else
							{
								if (lookAwaySide == "left")
								{
									eyesSM.Switch(eBlink);
									eyeClock = 0.0f;
								}
								lookAwaySide = "right";
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
			
			if ((playerLHandToHead < closeFaceDistance || playerRHandToHead < closeFaceDistance) && lipsTouchCount > 0.0f)
			{
				lipsOnly = true;
				if (currentMouth != "Kissing" && testRun == false)
				{
					mouthSM.Switch(mKiss);
				}
			}
			else
			{
				lipsOnly = false;
			}

			//SuperController.LogError("Checking for face Idle");
			if (lookAction == false && Random.Range(0.0f,100.0f) > Mathf.Lerp(99.99f,98.0f,interestArousal/10.0f) && currentLook != "Sex" && currentLook != "Sucking" && currentLook != "Kissing" && testRun == false)
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
							lookSM.SwitchRandom(new State[] {
											lCasual,
											lCasual,
											lCasual,
											lCasual,
											lInquisitive,
											lInquisitive,
											lInquisitive,
											lBored,
											lBored,
											lDayDream,
											lDayDream,
											lDayDream,
										});
						}
						else
						{
							lookSM.SwitchRandom(new State[] {
											lCasual,
											lCasual,
											lCasual,
											lCasual,
											lInquisitive,
											lInquisitive,
											lInquisitive,
											lInquisitive,
											lInquisitive,
											lPlayful,
										});
						}
					}
					else
					{
						if (interestValence < 5.0f)
						{
							lookSM.SwitchRandom(new State[] {
											lCasual,
											lCasual,
											lInquisitive,
											lInquisitive,
											lInquisitive,
											lPlayful,
											lPlayful,
											lPlayful,
											lPlayful,
										});
						}
						else
						{
							if (interestArousal > 8.0f && interestValence > 8.0f && playerHeadToFaceRot > lookDirectAngle)
							{
								lookSM.SwitchRandom(new State[] {
											lFeel,
											lFeel,
											lPlayful
										});
							}
							else
							{
								lookSM.SwitchRandom(new State[] {
											lFeel,
											lInquisitive,
											lInquisitive,
											lInquisitive,
											lInquisitive,
											lPlayful,
											lPlayful,
											lPlayful,
											lPlayful,
											lPlayful,
											lIntense,
											lIntense
										});
							}
						}
					}
				}
				
			}
			
			//SuperController.LogError("Checking for motion interact");
			if (lookAction == false && interestArousal >= Mathf.Lerp(6.5f, 9.3f, pExtraversion/100.0f) && interestValence > Mathf.Lerp(8.5f, 6.0f, pAgreeableness/100.0f) && currentLook != "Feel" && currentLook != "Sex" && currentLook != "Sucking" && currentLook != "Kissing" && testRun == false)
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
                if (playerTipToHead < interactionDistance * 1.05f && uiDoBlowjob.val)
                {
                    lookSM.Switch(lSucking);
					if (mainInterest != "Tip" && mainInterest != "Pelvis")
					{
						mainInterest = "Tip";
						interestTip = 100.0f;
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
					if (playerHeadToHead > personalSpaceDistance * 2.0f)
					{
						playerInterest -= 1.0f;
					}
					else
					{
						playerInterest -= 0.05f;
					}
				}
            }
            playerInterest = Mathf.Clamp(playerInterest, 0.0f, 100.0f);


            interestArousal = Mathf.Clamp(interestArousal - (0.002f * uiMoodSpeed.val * (1.0f+(interestPeakArousal/5.0f))), 3.0f, 10.0f);
			if (playerHeadToHead > personalSpaceDistance)
			{
				interestValence = Mathf.Clamp(interestValence - (0.0081f * uiMoodSpeed.val * (1.0f+(interestPeakValence/5.0f))), 2.0f, 10.0f);
			}
			else
			{
				interestValence = Mathf.Clamp(interestValence - (0.0043f * uiMoodSpeed.val * (1.0f+(interestPeakValence/5.0f))), 2.0f, 10.0f);
			}
			
			if (!IsFaceOnlyExpressionMode)
			{
			if (Vector3.Distance(headController.followWhenOff.position, eyeController.transform.position) < closeFaceDistance * 1.5f)
			{
				if (mainInterest == "LHand" || mainInterest == "RHand" || mainInterest == "Face")
				{
					currentInterest = "Face";
				}
				else
				{
					if (currentEye != "Closed")
					{
						eyesSM.Switch(eClosed);
					}
				}
			}
			
			if ((playerLHandInteract && playerRHandInteract) || playerHeadInteract)
			{
				if (playerHeadToFaceRot > lookDirectAngle && playerHeadToHead > closeFaceDistance * 1.2f)// && (playerLHandMovement || playerRHandMovement))
				{
					if (currentLook != "Intense")
					{
						//gAvoid = 1.0f;
						
						if ((playerLHandToLBreast < interactionDistance || playerLHandToRBreast < interactionDistance) && (playerRHandToLBreast < interactionDistance || playerRHandToRBreast < interactionDistance))
						{
							currentInterest = "RandomU";
						}
						else
						{
							if (Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, headController.followWhenOff.forward) < lookPeripheralAngle)
							{
								currentInterest = "Face";
							}
							else
							{
								gAvoid = 1.0f;
							}
						}
					}
				}
			}



			//SuperController.LogError("Look Position start");
            if (currentInterest == "RandomF")
            {
                //headController.transform.LookAt(randomPointForward);
                lookAtPosition = randomPointForward;
                eyeController.transform.position = randomPointForward;
				focusPos = eyeController.transform.position;
				focusRot = eyeController.transform.rotation;
            }
            if (currentInterest == "Target")
            {
                //headController.transform.LookAt(randomPointForward);
				if (emTargetName == "[CameraRig]")
				{
					lookAtPosition = CameraTarget.centerTarget.transform.position;
					eyeController.transform.position = CameraTarget.centerTarget.transform.position;
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
				}
				else
				{
					lookAtPosition = emTargetTransform.position;
					eyeController.transform.position = emTargetTransform.position;
					focusPos = eyeController.transform.position;
				focusRot = eyeController.transform.rotation;
				}
            }
            if (currentInterest == "RandomL")
            {
                //headController.transform.LookAt(randomPointLeft);
                lookAtPosition = randomPointLeft;
                eyeController.transform.position = randomPointLeft;
				focusPos = eyeController.transform.position;
				focusRot = eyeController.transform.rotation;
            }
            if (currentInterest == "RandomR")
            {
                //headController.transform.LookAt(randomPointRight);
                lookAtPosition = randomPointRight;
                eyeController.transform.position = randomPointRight;
				focusPos = eyeController.transform.position;
				focusRot = eyeController.transform.rotation;
            }
            if (currentInterest == "RandomU")
            {
                //headController.transform.LookAt(randomPointUp);
                lookAtPosition = randomPointUp;
                eyeController.transform.position = randomPointUp;
				focusPos = eyeController.transform.position;
				focusRot = eyeController.transform.rotation;
            }
            if (currentInterest == "Face")
            {
				if (playerHeadToHead > personalSpaceDistance)
				{
					if (usePerson2 && person2Usable)
					{
						lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
					}
					else
					{
						lookAtPosition = playerHeadTransform.position;
					}
				}
				else
				{
					if (usePerson2 && person2Usable)
					{
						lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.0f, Mathf.Lerp(0.0f,0.04f,Mathf.Clamp((playerHeadToHead-kissingDistance)/personalSpaceDistance,0.0f,1.0f)), 0.07f));
					}
					else
					{
						lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.0f, -0.02f, 0.0f));
					}
				}
				fuzzyLock = 1.0f;
            }
            if (currentInterest == "Chest" && usePerson2 && person2Usable)
            {
                lookAtPosition = playerChestController.followWhenOff.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
                eyeController.transform.position = lookAtPosition;
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
					lookAtPosition.y = playerHeadController.followWhenOff.position.y;
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
                fuzzyLock = 5.0f;
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
					lookAtPosition.y = playerHeadController.followWhenOff.position.y;
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
                fuzzyLock = 5.0f;
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
						if (playerTipToHead > 0.08f || lipsTouchCount <= 1.0f)
						{
							lookAtPosition = playerTipController.followWhenOff.TransformPoint(new Vector3(0.0f, 0.02f, 0.0f));
						}
						else
						{
							lookAtPosition = playerTipBaseController.followWhenOff.TransformPoint(new Vector3(0.0f, 0.1f, -0.3f));
						}
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
				lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.0f,0.00f,0.07f));
				if (gHeadRoll > 10.0f)
				{
					//lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(0.01f, 0.00f, tempFloat));
				}
				if (gHeadRoll < -10.0f)
				{
					//lookAtPosition = playerHeadTransform.TransformPoint(new Vector3(-0.01f, 0.00f, tempFloat));
				}
				fuzzyLock = 0.0f;
				if (eyesSM.CurrentState != eClosed)
				{
					//eyesSM.Switch(eClosed);
				}
			}
			//SuperController.LogError("Look Position Done");
		
			
			if (playerHeadToHead > personalSpaceDistance && playerInterest <= 10.0f && amGlancing == false && interestKissing == false)
			{
				//gAvoid = 1.0f;
			}
			
			//SuperController.LogError("BlowJob / Sex Check");
			if (amGlancing == false && gAvoid == 0.0f && playerPenisInteract && lookAction == false && playerHeadToHead > personalSpaceDistance/2.0f && testRun == false)
			{
				if (playerTipToHead < interactionDistance)
				{
					if (uiDoBlowjob.val && currentLook != "Sucking")
					{
						lookSM.Switch(lSucking);
					}
				}
				else
				{
					if (uiDoSex.val && currentLook != "Sex")
					{
						lookSM.Switch(lSex);
					}
				}
			}

			tempFloat = 1.0f;
			if (amGlancing == true)
			{
				tempFloat = 0.0f;
				saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
			}
			if (glanceTimeout >= 0.0f)
			{
				glanceTimeout -= Time.fixedDeltaTime;
			}
			//SuperController.LogError("Eye Position start");
            //			if (((Random.Range(0.0f,100000.0f) / 1000.0f > Mathf.Clamp(100.00f - ((100.0f-pStableness)/100.0f),99.15f,99.899f) || secondClock > 0.0f) && mainClock > Mathf.Clamp(6.0f * ((100.0f-pExtraversion)/100),1.5f,5.0f) && playerHeadToHead > closeFaceDistance) || amGlancing == true)
            if (((Random.Range(0.0f, 100.0f) > Mathf.Lerp(40.2f,76.95f,pStableness/100.0f) || (gAvoid == 1.0f && Random.Range(0.0f, 100.0f) > Mathf.Lerp(20.2f,56.95f,pStableness/100.0f))) || secondClock > 0.0f || amGlancing == true) && (glanceTimeout <= 0.0f || amGlancing == true) && uiGazeGlance.val)// && mainInterest == mainOld && secondInterest != "RandomL" && secondInterest != "RandomR")
            //if (1.0f == 1.0f)
			{
				//SuperController.LogError("Glancing at Second");
				if (mainInterest == secondInterest)
				{
					if (lookAwaySide == "left")
					{
						eyeController.transform.position = randomPointLeft;
						eyeController.transform.rotation = chestController.followWhenOff.rotation;
						eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
						amGlancing = true;
					}
					if (lookAwaySide == "right")
					{
						eyeController.transform.position = randomPointRight;
						eyeController.transform.rotation = chestController.followWhenOff.rotation;
						eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
						amGlancing = true;
					}
					if (lookAwaySide == "up")
					{
						eyeController.transform.position = randomPointUp;
						eyeController.transform.rotation = chestController.followWhenOff.rotation;
						eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
						amGlancing = true;
					}
					if (lookAwaySide == "down")
					{
						eyeController.transform.position = randomPointForward;
						eyeController.transform.rotation = chestController.followWhenOff.rotation;
						eyeController.transform.Translate(0.0f, -1.0f, 0.0f);
						amGlancing = true;
					}
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
				}
				
                if (secondInterest == "RandomL")
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
                if (secondInterest == "RandomR")
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
                if (secondInterest == "RandomU")
                {
                    //lookAtPosition = playerPelvisController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
                    eyeController.transform.position = playerPelvisController.followWhenOff.position;
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
                    amGlancing = true;
                    //fuzzyLock = 3.5f;
                }
                if (secondInterest == "RandomF")
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
				if (secondInterest == secondOld && secondInterest != "RandomF")
				{
                    eyeController.transform.position = randomPointForward;
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
                    amGlancing = true;
				}
                if (secondInterest == "Face" && interestFace+headActivityBoost > interestFaceBase*0.75f)
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
                if ((secondInterest == "LHand" && interestLHand + lHandActivityBoost > interestLHandBase*0.75f) || (secondInterest == "RandomL" && interestLHand + lHandActivityBoost > interestLHandBase*0.6f))
                {
                    //lookAtPosition = playerLHandController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
					eyeController.transform.position = playerLHandTransform.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
                    amGlancing = true;
                    //fuzzyLock = 2.5f;
                }
                if ((secondInterest == "RHand" && interestRHand + rHandActivityBoost > interestRHandBase*0.75f) || (secondInterest == "RandomR" && interestRHand + rHandActivityBoost > interestRHandBase*0.6f))
                {
                    //lookAtPosition = playerRHandController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
                    eyeController.transform.position = playerRHandTransform.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;

                    amGlancing = true;
                    //fuzzyLock = 2.5f;
                }
                if (secondInterest == "Pelvis" && usePerson2 && interestPelvis > interestPelvisBase*0.75f)
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
                if (secondInterest == "Tip" && headToTipRot < lookNoAwarenessAngle && usePerson2 && interestTip > interestTipBase*0.75f)
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
                if (secondInterest == "Target" && interestEMTarget > interestEMTargetBase * 0.75f)
                {
                    //lookAtPosition = playerTipController.transform.TransformPoint(new Vector3(0.0f,0.0f,0.0f));
					eyeController.transform.position = emTargetTransform.position;
					focusPos = eyeController.transform.position;
					focusRot = eyeController.transform.rotation;
                    amGlancing = true;
                    //fuzzyLock = 3.5f;
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
				}
                if (secondClock <= 0.0f && amGlancing)
                {
                    secondClock = Random.Range(0.35f, 0.65f + (1.0f * ((100.0f - pExtraversion) / 100.0f)));
                    ////LogError("Glancing");
                }
                if (amGlancing == false)
                {
                    secondClock = 0.0f;
                }
                else
                {
					if (tempFloat == 1.0f && eyeClock > (1.25f * uiBlinkSpeed.val) && currentEye != "Closed")
					{
						eyesSM.Switch(eBlink);
						eyeClock = 0.0f;
					}
					gHeadSpeed = 2.0f;
					glanceTimeout = Mathf.Lerp(4.0f,10.0f,pExtraversion/100.0f) * uiGlanceTimeout.val;
                }
            }
            else
            {
				//SuperController.LogError("Looking at main");
                gHeadSpeed = 1.0f;
                if (mainInterest == "Face" && (gAvoid != 1.0f || Random.Range(0.0f, 1.00f) < interestArousal / 10.0f))
                {
					if (person2Usable)
					{
						eyeController.transform.position = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
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
						eyeController.transform.position = playerPelvisController.followWhenOff.position;
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
                    //if (playerLHandToHead < closeFaceDistance)
                    //{
                    //    eyeController.transform.position = playerHeadTransform.position;
                    //    eyeController.transform.rotation = playerHeadTransform.rotation;
                    //    eyeController.transform.Translate(0.0f, 0.03f, 0.07f);
                    //}
                    //else
                    //{
                        eyeController.transform.position = playerLHandTransform.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
                        eyeController.transform.rotation = playerLHandTransform.rotation;
						focusPos = eyeController.transform.position;
						focusRot = eyeController.transform.rotation;
                        //eyeController.transform.Translate(-0.1f, 0.0f, 0.0f);
                    //}
                }
                if (mainInterest == "RHand" && (usePerson2 || playerHandsUsable))
                {
                    //if (playerRHandToHead < minFaceDistance)
                    //{
                    //    eyeController.transform.position = playerHeadTransform.position;
                    //    eyeController.transform.rotation = playerHeadTransform.rotation;
                    //    eyeController.transform.Translate(0.0f, 0.03f, 0.07f);
                    //}
                    //else
                    //{
                        eyeController.transform.position = playerRHandTransform.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
                        eyeController.transform.rotation = playerRHandTransform.rotation;
						focusPos = eyeController.transform.position;
						focusRot = eyeController.transform.rotation;
                        //eyeController.transform.Translate(0.1f, 0.0f, 0.0f);
                    //}
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
            }
			
			bool redoEyePos = false;
			float lookAngle = Vector3.Angle(eyeController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward);
			float lookdist = Vector3.Distance(eyeController.transform.position, headController.followWhenOff.position);
			
			if (lookdist < closeFaceDistance * 1.5f)
			{
				if (lookAngle < lookPeripheralAngle)
				{
					redoEyePos = true;
					saccadeOffset = new Vector3(0.0f,0.0f,0.0f);
				}
			}

			float headToLook = Vector3.Angle(lookAtPosition - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
			
			float headToMain = 999.0f;
			Vector3 mainPos = new Vector3(0.0f,0.0f,0.0f);
			if (mainInterest == "Face")
			{
				if (person2Usable && usePerson2)
				{
					headToMain = Vector3.Angle(playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)) - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
					mainPos = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
				}
				else
				{
					headToMain = Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
					mainPos = playerHeadTransform.position;
				}
			}
			if (mainInterest == "LHand")
			{
				headToMain = Vector3.Angle(playerLHandTransform.position - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
				mainPos = playerLHandTransform.position;
			}
			if (mainInterest == "RHand")
			{
				headToMain = Vector3.Angle(playerRHandTransform.position - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
				mainPos = playerRHandTransform.position;
			}
			if ((mainInterest == "Pelvis" || mainInterest == "Penis") && usePerson2 && person2Usable)
			{
				headToMain = Vector3.Angle(playerPelvis - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
				mainPos = playerPelvis;
			}
			if (mainInterest == "Target" && emTargetName != "None")
			{
				headToMain = Vector3.Angle(emTargetTransform.position - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
				mainPos = emTargetTransform.position;
			}
			
			float headToSecond = 999.0f;
			Vector3 secondPos = new Vector3(0.0f,0.0f,0.0f);
			if (secondInterest == "Face")
			{
				if (person2Usable && usePerson2)
				{
					headToSecond = Vector3.Angle(playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f)) - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
					secondPos = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
				}
				else
				{
					headToSecond = Vector3.Angle(playerHeadTransform.position - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
					secondPos = playerHeadTransform.position;
				}
			}
			if (secondInterest == "LHand")
			{
				headToSecond = Vector3.Angle(playerLHandTransform.position - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
				secondPos = playerLHandTransform.position;
			}
			if (secondInterest == "RHand")
			{
				headToSecond = Vector3.Angle(playerRHandTransform.position - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
				secondPos = playerRHandTransform.position;
			}
			if (secondInterest == "Pelvis" || secondInterest == "Penis")
			{
				headToSecond = Vector3.Angle(playerPelvis - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
				secondPos = playerPelvis;
			}
			if (secondInterest == "Target" && emTargetName != "None")
			{
				headToSecond = Vector3.Angle(emTargetTransform.position - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
				secondPos = emTargetTransform.position;
			}

			float headToLeft = Vector3.Angle(randomPointLeft - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
			float headToRight = Vector3.Angle(randomPointLeft - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
			float headToUp = Vector3.Angle(randomPointLeft - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
			float headToForward = Vector3.Angle(randomPointForward - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
			float headToTarget = 999.0f;
			if (emTargetName != "None")
			{
				headToTarget = Vector3.Angle(emTargetTransform.position - headController.followWhenOff.position, abdomenController.followWhenOff.forward);
			}
			
			if (amGlancing == false)
			{
				if (headToLook > lookNoAwarenessAngle)
				{
					if (headToMain < lookPeripheralAngle)
					{
						lookAtPosition = mainPos;
						currentInterest = mainInterest;
						if (redoEyePos)
						{
							eyeController.transform.position = lookAtPosition;
							focusPos = eyeController.transform.position;
						}
					}
					else
					{
						if (headToSecond < lookPeripheralAngle)
						{
							lookAtPosition = secondPos;
							currentInterest = secondInterest;
							if (redoEyePos)
							{
								eyeController.transform.position = lookAtPosition;
								focusPos = eyeController.transform.position;
							}
						}
						else
						{
							if (headToTarget < lookPeripheralAngle)
							{
								lookAtPosition = emTargetTransform.position;
								currentInterest = "Target";
								if (redoEyePos)
								{
									eyeController.transform.position = lookAtPosition;
									focusPos = eyeController.transform.position;
								}
							}
							else
							{
								if (headToLeft < lookPeripheralAngle && headToLeft < headToRight)
								{
									lookAtPosition = randomPointLeft;
									currentInterest = "RandomL";
								}
								else
								{
									if (headToRight < lookPeripheralAngle && headToRight < headToUp)
									{
										lookAtPosition = randomPointRight;
										currentInterest = "RandomR";
										if (redoEyePos)
										{
											eyeController.transform.position = lookAtPosition;
											focusPos = eyeController.transform.position;
										}
									}
									else
									{
										if (headToUp < lookPeripheralAngle && headToUp < headToForward)
										{
											lookAtPosition = randomPointUp;
											currentInterest = "RandomU";
											if (redoEyePos)
											{
												eyeController.transform.position = lookAtPosition;
												focusPos = eyeController.transform.position;
											}
										}
										else
										{
											lookAtPosition = randomPointForward;
											currentInterest = "RandomF";
											if (redoEyePos)
											{
												eyeController.transform.position = lookAtPosition;
												focusPos = eyeController.transform.position;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			
				
            Vector3 centrePoint = chestController.followWhenOff.position;
            Vector3 eyePoint = eyeController.transform.position;
            float c2eActual = Vector3.Distance(centrePoint, eyePoint);
            centrePoint.y = 0.0f;
            eyePoint.y = 0.0f;
            float c2eDist = Vector3.Distance(centrePoint, eyePoint);
            if (c2eDist < closeFaceDistance * 3.0f && mainInterest != "Face")
            {
                eyeController.transform.position = eyeController.transform.position + (abdomenController.followWhenOff.forward * 0.1f);
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
					lookAtPosition = randomPointRight;//playerRHandTransform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
					fuzzyLock = 2.0f;
					if (amGlancing == false)
					{
						eyeController.transform.position = randomPointRight;//playerRHandTransform.position;
						focusPos = eyeController.transform.position;
						focusRot = eyeController.transform.rotation;
					}
					//gHeadRollTarget = Random.Range(-25.0f,-5.0f);
				}
				else
				{
					//mainOld = mainInterest;
					mainInterest = "RandomL";
					lookAtPosition = randomPointLeft;//playerLHandTransform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f));
					fuzzyLock = 2.0f;
					if (amGlancing == false)
					{
						eyeController.transform.position = randomPointLeft;//playerLHandTransform.position;
						focusPos = eyeController.transform.position;
						focusRot = eyeController.transform.rotation;
					}
					//gHeadRollTarget = Random.Range(25.0f,5.0f);
				}
			}
			
			if (playerLHandToHead < closeFaceDistance || playerRHandToHead < closeFaceDistance)
				{
				eyeController.transform.position = playerHeadTransform.TransformPoint(new Vector3(0.0f, 0.04f, 0.07f));
                eyeController.transform.rotation = playerHeadTransform.rotation;
						focusPos = eyeController.transform.position;
				focusRot = eyeController.transform.rotation;
				}

			if (mEyesClosedLeftValue > 0.9f && morphBlinking == false)
			{
				//eyeController.transform.position = headController.transform.TransformPoint(new Vector3(0.0f, 0.00f, 1.0f));
			}
		
			if (interestKissing && uiDoKiss.val)
			{
				fuzzyLock = 0.0f;
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

			
			if (playerHeadToHead <= Mathf.Max(personalSpaceDistance / 3.0f,closeFaceDistance*2.0f) && currentLook != "Sucking" && currentLook != "Kissing" && mainInterest == "Face")
			{
				tempFloat = playerHeadController.followWhenOff.eulerAngles.z - headController.followWhenOff.eulerAngles.z;
				if (tempFloat > 180.0f){tempFloat -= 360.0f;}
				if (tempFloat < -180.0f){tempFloat += 360.0f;}

				//gHeadRollTarget = Mathf.Clamp(tempFloat + Random.Range(-25.0f,25.0f),-40.0f,40.0f);
			}
			//SuperController.LogError("Blink calculation");
			
			//saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
			
			eyeMoveDist = Vector3.Distance(oldEyePos, eyeController.transform.position);
			if (currentEye == "Blink" || currentEye == "Closed")
			{
				eyeMoveDist = 0.0f;
			}
		    if ((eyeMoveDist > 0.62f && eyeClock > (0.65f * uiBlinkSpeed.val)) || (eyeMoveDist > 1.0f && eyeClock > (0.25f * uiBlinkSpeed.val)))
			{
				if (currentEye != "Closed" && currentEye != "Blink" && interestKissing == false)
				{
                    eyesSM.Switch(eBlink);
                    eyeClock = 0.0f;
				}
			}
			
            if (interestKissing == false)
            {
                //if (((Random.Range(0.0f, (((15.0f - (10.0f - interestArousal)) / (2.0f * (interestArousal))) * (1.0f / Random.Range(1.333f, 2.5f))) * Time.fixedDeltaTime) / 300.0f) - (Mathf.Max(eyeClock - 1.0f, 0.0f) / 5000000.0f) <= 0.0f) && eyeClock > 1.0f)
                //if (((Random.Range(0.0f,(1.0f / Random.Range(1.333f,3.5f)) * Time.fixedDeltaTime) / 10.0f) - (Mathf.Max(eyeClock-1.0f,0.0f) / 5000000.0f) <= 0.0f) && eyeClock > 0.5f)
                if (eyeClock > 2.0f * uiBlinkSpeed.val && Random.Range(0.0f,100.0f) > Mathf.Lerp(99.4f - (interestValence / 100.0f),99.7f - (interestArousal / 100.0f),pExtraversion / 100.0f)) 
				{
                    eyeClock = 0.0f;
                    eyesSM.Switch(eBlink);
                }
            }
			if (eyeClock > 0.3f && morphBlinking && currentEye != "Closed")
			{
				morphBlinking = false;
				eyesSM.Switch(eOpen);
				eyeClock = 0.0f;
			}
			eyeClock += Time.fixedDeltaTime;

            //eyeController.transform.Translate(randomX,randomY,0.0f);

			
			//SuperController.LogError("Calculating Head Rotation");
            Transform head = headController.transform;
            Transform reference = chestController.followWhenOff;

            Vector3 actualDir = reference.InverseTransformDirection(head.forward);
			tempFloat = 0.0f;
			if ((currentLook == "Intense" || currentLook == "Playful") && mainInterest == "Face" && gAvoid == 0.0f && amGlancing == false)
			{
				tempFloat = Mathf.Lerp(0.05f, 0.15f, Mathf.Clamp(playerHeadToHead - (closeFaceDistance*3.0f), 0.0f, 1.0f));
			}
            Vector3 targetDir = (lookAtPosition - new Vector3(0.0f,Mathf.Lerp(0.0f,tempFloat,interestArousal/10.0f),0.0f)) - head.position;
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
            float actualH = Mathf.Atan2(actualDirH.x, actualDirH.y);
            float targetH = Mathf.Atan2(targetDirH.x, targetDirH.y);
            float actualV = Mathf.Atan2(actualDirV.y, actualDirV.x);
            float targetV = Mathf.Atan2(targetDirV.y, targetDirV.x);


            headToEyeController = Mathf.Abs(Vector3.Angle(eyeController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
            if (headToEyeController > lookDirectAngle || (playerHeadToHead < personalSpaceDistance/2.0f && headToEyeController > eyesNonDirectAngle) || Vector3.Distance(new Vector3(0.0f,0.0f,0.0f), saccadeOffset) > 15.0f)
            {
				if (gAvoid == 0.0f && amGlancing == false)
				{
					eyesNonDirectClock += Time.fixedDeltaTime * Mathf.Lerp(0.55f, 2.8f, interestValence / 10.0f);
				}
            }
            else
            {
				if (amGlancing == false && gAvoid == 0.0f)
				{
					eyesNonDirectClock = 0.0f;
				}
            }
			if (headToEyeController > lookNoAwarenessAngle && amGlancing == false)
			{
				eyesSM.Switch(eClosed);
			}

			
			//10.0f - ((100.0f - pExtraversion) / 10.0f) - eyesNonDirectClock < 0.0f
            if (( Mathf.Lerp(7.0f * uiDirectLookDelay.val, 3.0f * uiDirectLookDelay.val, interestArousal/10.0f) - eyesNonDirectClock < 0.0f || headDelayTimer > 10.0f * uiDirectLookDelay.val) && interestKissing == false && gAvoid == 0.0f)// || playerHeadToHead < closeFaceDistance * 2.0f)
            {
				
				peronalityAdjustH = peronalityAdjustH * 0.605f;
				peronalityAdjustV = peronalityAdjustV * 0.795f;
				//SuperController.LogError("Reducing Adjustment");
                fuzzyLock = fuzzyLock * 0.75f;
				//eyesNonDirectClock = 0.0f;
				
            }
			
			if (Mathf.Abs(gHeadRoll - gHeadRollTarget) > 20.0f)
			{
				peronalityAdjustH = 0.0f;
				peronalityAdjustV = 0.0f;				
			}
			
			if (currentLook == "Sucking")
			{
				peronalityAdjustH = 0.0f;
				peronalityAdjustV = 0.0f;
                fuzzyLock = 0.0f;
			}
			
			if (mainInterest == "Tip")
			{
				fuzzyLock = 0.0f;
				gHeadSpeed = gHeadSpeed / 5.0f;
			}
			
            // adjust angles
			
			tempFloat = 0.0015f * uiGazeSpeed.val;
			if (playerHeadToHead < personalSpaceDistance/2.0f)
			{
				tempFloat = 0.003f * uiGazeSpeed.val;
			}
			
			
			
			tempFloat2 = Mathf.Clamp(peronalityAdjustH, -20.0f * uiGazeVariation.val, 20.0f * uiGazeVariation.val);
			if (endAdjustH < tempFloat2)
			{
				endAdjustH = Mathf.Clamp(endAdjustH + tempFloat, endAdjustH, tempFloat2);
			}
			else
			{
				endAdjustH = Mathf.Clamp(endAdjustH - tempFloat, tempFloat2, endAdjustH);
			}
			
			tempFloat2 = Mathf.Clamp(peronalityAdjustV, -5.0f * uiGazeVariation.val, 20.0f * uiGazeVariation.val);
			if (endAdjustV < tempFloat2)
			{
				endAdjustV = Mathf.Clamp(endAdjustV + tempFloat, endAdjustV, tempFloat2);
			}
			else
			{
				endAdjustV = Mathf.Clamp(endAdjustV - tempFloat, tempFloat2, endAdjustV);
			}
			
			if (targetH * Mathf.Rad2Deg < -15.0f && endAdjustH * Mathf.Rad2Deg < -15.0f)
			{
				endAdjustH = Mathf.Min(endAdjustH + tempFloat, 0.0f);
			}				
			if (targetH * Mathf.Rad2Deg > 15.0f && endAdjustH * Mathf.Rad2Deg > 15.0f)
			{
				endAdjustH = Mathf.Max(endAdjustH - tempFloat, 0.0f);
			}				
			
			if ((currentInterest == "Face" && playerHeadMovement) || gAvoid == 1.0f || amGlancing)
			{
				tempFloat = 0.01f;
				if (endAdjustH < 0.0f)
				{
					endAdjustH = Mathf.Min(endAdjustH + tempFloat, 0.0f);
				}
				else
				{
					endAdjustH = Mathf.Max(endAdjustH - tempFloat, 0.0f);
				}
				if (endAdjustV < 0.0f)
				{
					endAdjustV = Mathf.Min(endAdjustV + tempFloat/3.0f, 0.0f);
				}
				else
				{
					endAdjustV = Mathf.Max(endAdjustV - tempFloat/3.0f, 0.0f);
				}
			}
			
			if (endAdjustV > 0.0f)
			{
				endAdjustV = Mathf.Max(0.0f, endAdjustV - Mathf.Lerp(0.0f, 0.01f, interestArousal/10.0f));
			}
			headToEyeController = Mathf.Abs(Vector3.Angle(eyeController.transform.position - headController.followWhenOff.position, headController.followWhenOff.forward));
			if (uiDoHead.val)
			{
				
				//SuperController.LogError("Adjusting head rotation");
				if (targetH < 0.0f)
				{
					endAdjustH = Mathf.Clamp(endAdjustH,(-uiGazeMaxSideways.val * Mathf.Deg2Rad) - targetH,uiGazeMaxSideways.val * Mathf.Deg2Rad);
				}
				else
				{
					endAdjustH = Mathf.Clamp(endAdjustH,-uiGazeMaxSideways.val * Mathf.Deg2Rad,(uiGazeMaxSideways.val * Mathf.Deg2Rad) - targetH);
				}
				endAdjustV = Mathf.Clamp(endAdjustV,-uiGazeMaxDown.val * Mathf.Deg2Rad,uiGazeMaxUp.val * Mathf.Deg2Rad);
				
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
				
				adjustedSpeed = Mathf.Clamp((Mathf.Lerp(gHeadSpeed * 0.55f * gazeAdjust, gHeadSpeed * 1.0f * gazeAdjust,((interestArousal+interestValence)/2.0f)/10.0f)) / uiGazeSpeed.val,0.1f, 90.0f); // - 0.5f + (closeFaceDistance / playerHeadToHead)
				//adjustedSpeed = Mathf.Lerp(adjustedSpeed, adjustedSpeed/10.0f, Mathf.Clamp(Mathf.Abs(targetH - actualH) / 2.0f, 0.0f, 3.0f));
				if (currentLook == "Sex" && playerHeadToHead < personalSpaceDistance / 2.0f)
				{
					adjustedSpeed = adjustedSpeed * Mathf.Lerp(5.0f,10.0f,pExtraversion/100.0f);
				}
				if (gAvoid == 1.0f)
				{
					adjustedSpeed = adjustedSpeed * 1.2f;
				}
				if (mainInterest == "RandomR" || mainInterest == "RandomL" || mainInterest == "RandomU" || mainInterest == "RandomF")
				{
					if (currentLook == "Bored" || currentLook == "Daydream")
					{
						adjustedSpeed = adjustedSpeed / 2.0f;
					}
					else
					{
						adjustedSpeed = adjustedSpeed * 6.0f;
					}
				}
				if ((mainOld == "RandomR" || mainOld == "RandomL" || mainOld == "RandomU" || mainOld == "RandomF") && playerHeadToHead > personalSpaceDistance)
				{
					adjustedSpeed = adjustedSpeed / 2.0f;
				}
				if (lipsTouchCount > 0.0f && playerHeadToHead > kissingDistance * 1.1f && (playerLHandToHead < closeFaceDistance || playerRHandToHead < closeFaceDistance))
				{
					adjustedSpeed = adjustedSpeed * 50.0f;
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
				velocityH = Mathf.Clamp(velocityH, -1.0f, 1.0f);
				velocityV = Mathf.Clamp(velocityV, -2.0f, 2.0f);
				if (gAvoid == 1.0f)
				{
					headDelayTimer = 0.0f;
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
					if (Mathf.Abs(actualH - targetH) < (lookDirectAngle / 2.0f) * Mathf.Deg2Rad && Mathf.Abs(actualV - targetV) < (lookDirectAngle / 2.0f) * Mathf.Deg2Rad && headDelayTimer > Mathf.Lerp(5.0f, 3.0f, interestValence/10.0f))
					{
						headDelayTimer = 0.0f;

					}
				//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
				//float updown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);

				if ((currentLook == "Sex" || currentLook == "Intense" || currentLook == "Inquisitive" || currentLook == "Kissing" || headToEyeController > Mathf.Lerp(lookDirectAngle, lookPeripheralAngle, interestArousal/10.0f) || gAvoid == 1.0f || headDelayTimer > Mathf.Lerp(5.0f * uiDirectLookDelay.val, 3.0f * uiDirectLookDelay.val, interestValence/10.0f) || Mathf.Abs(targetV - actualV) > 10.0f) && (headUpDown > -27.0f || targetV > actualV)) //|| headDelayTimer > Mathf.Lerp(4.0f, 1.0f, interestValence/10.0f) * pExtraversion/100.0f
				{
					//fuzzyLock = 0.0f;
					//energyAmount += 1.0f;
					tempFloat2 = Mathf.Clamp(playerHeadToHead - 0.1f, 0.0f, 1.0f);
					tempFloat = Mathf.Lerp(uiGazeMaxSideways.val * 0.75f,uiGazeMaxSideways.val, tempFloat2);
					targetH = Mathf.Clamp(targetH + (endAdjustH * uiGazeVariation.val), -tempFloat * Mathf.Deg2Rad, tempFloat * Mathf.Deg2Rad);
					tempFloat = Mathf.Lerp(85.00f,69.0f, tempFloat2);
					targetV = Mathf.Clamp(targetV + (endAdjustV * uiGazeVariation.val), -Mathf.Lerp(uiGazeMaxDown.val,uiGazeMaxDown.val * 0.75f, tempFloat2) * Mathf.Deg2Rad, Mathf.Lerp(uiGazeMaxUp.val,uiGazeMaxUp.val * 0.75f, tempFloat2) * Mathf.Deg2Rad);
					
					if (Mathf.Abs(actualH - targetH) > 15.0f * Mathf.Deg2Rad || Mathf.Abs(actualV - targetV) > 10.0f * Mathf.Deg2Rad)
					{
						if (Mathf.Abs(gHeadRoll) > 1.0f)
						{
							gHeadRollTarget = gHeadRollTarget * 0.55f;
						}
					}
					//adjustedSpeed = Mathf.Lerp(adjustedSpeed*10.0f, adjustedSpeed, Mathf.Clamp(Mathf.Abs(velocityH) * 10.0f, 0.0f, 1.0f));
					adjustedSpeed = Mathf.Lerp(adjustedSpeed, adjustedSpeed / 3.0f, headDelayTimer / 50.0f);
					
					if (Mathf.Abs(actualH - targetH) > ((30.0f * uiGazeVariation.val) * (pAgreeableness / 100.0f) * Mathf.Lerp(fuzzyLock, 0.0f, Mathf.Clamp(headDelayTimer, 0.0f, Mathf.Lerp(5.0f, 3.0f, interestValence/10.0f)) / Mathf.Lerp(5.0f, 3.0f, interestValence/10.0f))) * Mathf.Deg2Rad)// || (Mathf.Abs(actualH - targetH) > 65.0f * Mathf.Deg2Rad && Mathf.Abs(actualH) < Mathf.Abs(targetH)))// || eyesSM.CurrentState == eClosed)
					{
						actualH = Mathf.SmoothDamp(actualH, targetH, ref velocityH, Mathf.Lerp(adjustedSpeed, adjustedSpeed/10.0f,Mathf.Abs(actualH - targetH)), Mathf.Infinity, Time.fixedDeltaTime);
					}
					if (Mathf.Abs(actualV - targetV) > ((15.0f * uiGazeVariation.val) * (pExtraversion / 100.0f) * Mathf.Lerp(fuzzyLock, 0.0f, Mathf.Clamp(headDelayTimer, 0.0f, Mathf.Lerp(5.0f * uiDirectLookDelay.val, 3.0f * uiDirectLookDelay.val, interestValence/10.0f)) / Mathf.Lerp(5.0f * uiDirectLookDelay.val, 3.0f * uiDirectLookDelay.val, interestValence/10.0f))) * Mathf.Deg2Rad)// || (Mathf.Abs(actualV - targetV) > 55.0f * Mathf.Deg2Rad && Mathf.Abs(actualV) < Mathf.Abs(targetV)))// || eyesSM.CurrentState == eClosed)
					{
						actualV = Mathf.SmoothDamp(actualV, targetV + (uiHeadAngleOffset.val * Mathf.Deg2Rad), ref velocityV, adjustedSpeed * 0.75f, Mathf.Infinity, Time.fixedDeltaTime);
					}
					else
					{
						velocityV = velocityV * 0.9f;
						actualV = Mathf.SmoothDamp(actualV, targetV + (uiHeadAngleOffset.val * Mathf.Deg2Rad), ref velocityV, adjustedSpeed * 0.75f, Mathf.Infinity, Time.fixedDeltaTime);
					}
					//testString = "Ver " + actualV + "/" + targetV + "/" + velocityV + "/" + ((3.0f * uiGazeVariation.val) * (pExtraversion / 100.0f) * fuzzyLock);
					actualH = Mathf.Clamp(actualH,-uiGazeMaxSideways.val * Mathf.Deg2Rad,uiGazeMaxSideways.val * Mathf.Deg2Rad);
					actualV = Mathf.Clamp(actualV,-uiGazeMaxDown.val * Mathf.Deg2Rad,uiGazeMaxUp.val * Mathf.Deg2Rad);
				}
				
				

				// recombine
				actualDir = RecombineDirection(actualH, actualV);
				targetDir = RecombineDirection(targetH, targetV);
				actualDir = reference.TransformDirection(actualDir);

				//head.eulerAngles.z = curRotation.z;
				head.transform.LookAt(head.transform.position + actualDir, headController.followWhenOff.position - chestController.followWhenOff.position);
				
				tempFloat = 50.0f;//(150.0f / uiRollSpeed.val) / (Mathf.Max(Mathf.Abs(gHeadRollTarget - gHeadRoll) / 30.0f, 1.0f) / 10.0f);
				//tempFloat2 = Mathf.Clamp(gHeadRoll, -uiMaxHeadRoll.val, uiMaxHeadRoll.val);
				if (interestKissing || currentMouth == "Sucking")
				{
					tempFloat = 10.0f / uiRollSpeed.val;
				}
				if (gAvoid == 1.0f)
				{
					tempFloat = tempFloat * 2.0f;
				}
				
				
				if (gHeadRoll < gHeadRollTarget)
				{
					gHeadRoll = Mathf.Min(gHeadRoll + (((gHeadRollTarget) - gHeadRoll) / tempFloat), uiMaxHeadRoll.val);
				}
				
				if (gHeadRoll > gHeadRollTarget)
				{
					gHeadRoll = Mathf.Max(gHeadRoll - ((gHeadRoll - (gHeadRollTarget)) / tempFloat), -uiMaxHeadRoll.val); // * (interestValence/10.0f)
				}
				gHeadRoll = Mathf.Clamp(gHeadRoll, -uiMaxHeadRoll.val, uiMaxHeadRoll.val);
				
				if (Mathf.Abs(gHeadRoll) < 3.0f)
				{
					rollTimer = 0.0f;
				}

				// apply roll
				Vector3 eulerAngles = head.transform.localEulerAngles;
				//testString = (cross.x * Mathf.Rad2Deg) + "/" + (cross.y * Mathf.Rad2Deg) + "/" + (cross.z * Mathf.Rad2Deg);
				eulerAngles.z += gHeadRoll;
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

				//SuperController.LogError("Adjusting neck rotation");

				eulerAngles = head.transform.eulerAngles;
				Vector3 forwardEuler = chestController.transform.eulerAngles;
				Vector3 difference = forwardEuler - eulerAngles;

				Vector3 newDir = Vector3.RotateTowards(neckController.transform.forward, headController.followWhenOff.forward, Mathf.Lerp(1.1f,70.5f,interestValence/10.0f) / 100.0f, 0.0f);
				neckController.transform.rotation = Quaternion.LookRotation(newDir);
				
				neckController.transform.eulerAngles = eulerAngles;
				eulerAngles = neckController.transform.localEulerAngles;
				eulerAngles.x += sexActionNeckX / 5.0f;
				if (interestKissing)
				{
				eulerAngles.z = playerHeadController.transform.eulerAngles.z;
				}
				else
				{
				eulerAngles.z -= gHeadRoll * Mathf.Lerp(0.6f,0.2f,interestValence/10.0f) * ((15.0f - Mathf.Min(rollTimer,15.0f)) / 15.0f);
				}
				neckController.transform.localEulerAngles = eulerAngles;
			}
			else
			{
				peronalityAdjustH = 0.0f;
				peronalityAdjustV = 0.0f;
				gHeadRoll = 0.0f;
			}

				if ((playerLHandToHead < closeFaceDistance && playerLHandMovement) || (playerRHandToHead < closeFaceDistance && playerRHandMovement))
					{
						if (eyeClock > (2.75f * uiBlinkSpeed.val) && morphBlinking == false && currentEye != "Closed")
						{
						eyesSM.Switch(eClosed);
						}
					}
			
			
			eyeController.transform.position = focusPos;
			eyeController.transform.rotation = focusRot;
			eyeController.transform.Translate(saccadeOffset * (Vector3.Distance(headController.followWhenOff.position, eyeController.transform.position) / 100.0f));
			
			if (eyeUpdateClock >= eyeUpdateTime - Time.fixedDeltaTime)
			{
				if (Vector3.Distance(curEyePosition, eyeController.transform.position) > 2.5f)
				{
					if (currentEye != "Blink")
					{
						eyesSM.Switch(eBlink);
						blinkRepeat = blinkRepeat / 2.0f;
						//SuperController.LogError("Blinking  " + Vector3.Distance(curEyePosition, eyeController.transform.position));
					}
				}
			}
			if (mEyesClosedLeftValue < 0.7f)
			{
				if (eyeUpdateClock >= eyeUpdateTime || (mainInterest == "Face" && playerHeadMovement && playerHeadToHead > personalSpaceDistance / 2.0f) || (mainInterest == "LHand" && playerLHandMovement) || (mainInterest == "RHand" && playerRHandMovement))
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
			}
			}

			if (morphMouthAction == false)
			{
				currentMouth = "Idle";
			}
			if (morphEyeAction == false)
			{
				currentEye = "Idle";
			}
			if (morphBrowAction == false)
			{
				currentBrow = "Idle";
			}
			if (lookAction == false)
			{
				currentLook = "Idle";
			}
			
			if (emTargetName != "None")
			{
				//emTargetPosPrev = emTargetController.transform.position;
			}
			//SuperController.LogError("Fixed Update Complete");
			
			if (currentLook == "Idle" && currentMouth == "Idle" && interestValence > 8.0f && testRun == false)
			{
				mouthSM.Switch(mBigSmile);
			}
						
			//Vector3 tempvector = chestController.followWhenOff.forward;
			//Vector3 temp2vector = headController.followWhenOff.forward;
			//float vecfloat = Vector3.Angle(new Vector3(temp2vector.x, tempvector.y, tempvector.z), tempvector);
			
			//float leftright2 = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
			//float updown2 = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
			//testString = "Left/Right : " + headLeftRight + "  Up/Down : " + headUpDown;
			//testString = vecfloat.ToString();
			if (uiShowStats.val && !IsFaceOnlyExpressionMode)
			{
			//SuperController.LogError("Doing Message Stats");
			SuperController.singleton.ClearMessages();
			Vector3 tempAngles = playerHeadTransform.eulerAngles;
			//SuperController.LogMessage(energyAmount.ToString());
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
			SuperController.LogMessage("Interest Clock : " + Round(interestClock) + "(" + Mathf.Round(interestRepeat) + ")", false);
			SuperController.LogMessage("Current Interest : " + currentInterest);
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
				if (mEyesClosedLeftValue > 0.8f)
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
					if (headToEyeController > eyesNonDirectAngle)
					{
						SuperController.LogMessage("Gaze : Indirect", false);
					}
					else
					{
						SuperController.LogMessage("Gaze : Direct", false);
					}
				}
			}
			SuperController.LogMessage("Gaze Variation H/V : " + Round(endAdjustH * Mathf.Rad2Deg) + "/" + Round(endAdjustV * Mathf.Rad2Deg), false);
			SuperController.LogMessage("Gaze Head Roll / Target / Timer : " + Round(gHeadRoll) + " /" + Round(gHeadRollTarget) + " /" + Round(rollTimer), false);
			SuperController.LogMessage("Gaze Neck Adjust : " + Round(sexActionNeckX), false);
			SuperController.LogMessage("Gaze Fuzzy Lock / Delay Timer : " + Round(fuzzyLock) + "/" + Round(headDelayTimer) + "(" + Round(Mathf.Abs(actualH - targetH) * Mathf.Rad2Deg) + "/" + Round(Mathf.Abs(actualV - targetV) * Mathf.Rad2Deg) + ")", false);
			SuperController.LogMessage("Hor Fuzz : " + Round(((15.0f * uiGazeVariation.val) * (pAgreeableness / 100.0f) * fuzzyLock) * Mathf.Deg2Rad) + " Vert Fuzz : " + Round(((3.0f * uiGazeVariation.val) * (pExtraversion / 100.0f) * fuzzyLock) * Mathf.Deg2Rad));
			SuperController.LogMessage("Target Vert : " + Round(targetV * Mathf.Rad2Deg) + "| Hor : " + Round(targetH * Mathf.Rad2Deg) + "| Speed : " + Round(adjustedSpeed), false);
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
			if (person2IsMale)
			{
				SuperController.LogMessage("Character is Male", false);
			}
			else
			{
				SuperController.LogMessage("Character is Female", false);
			}
			SuperController.LogMessage("Flirt Morph : " + Round(mFlirtingValue), false);
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
			SuperController.LogMessage("Ten Strip Taking It Morph : " + Round(mTakingItValue), false);
			
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
                //Vector3 perp = Vector3.Cross(chestController.followWhenOff.eulerAngles, refAngle);
                //float dir = Vector3.Dot(perp, chestController.followWhenOff.up);
				tempFloat = Mathf.Abs(Vector3.Angle(refAngle, abdomenController.followWhenOff.forward));
                if (Mathf.Abs(tempFloat) > 60.0f || allSetup == false)
                {
                    randomResetDir = true;
                    refAngle = abdomenController.followWhenOff.forward;
                }
				tempFloat = Random.Range(-4.0f,1.0f);
				if (playerHeadToHead < personalSpaceDistance)
				{
					tempFloat = -1.5f;
				}
				else
				{
					if (gAvoid == 1.0f)
					{
						tempFloat = gAvoidHeight;
					}
				}
				tempFloat2 = 93.0f;
                if (Random.Range(0.0f, 100.0f) > tempFloat2 || randomResetDir)
                {
                    //generate random points
                    float rad = Random.Range(-15.0f, 15.0f) * Mathf.Deg2Rad;
                    Vector3 position = abdomenController.followWhenOff.right * Mathf.Sin(rad) + abdomenController.followWhenOff.forward * Mathf.Cos(rad);
                    randomPointForward = abdomenController.followWhenOff.position + (abdomenController.followWhenOff.right * (Random.Range(-3.0f, 3.0f) * randomBaseOffset)) + (abdomenController.followWhenOff.up * (Random.Range(-0.5f, tempFloat) * randomBaseHeight)) + (abdomenController.followWhenOff.forward * (5.0f * randomBaseDistance));
					if (gAvoid == 1.0f)
					{
						gAvoid = 0.0f;
						gAvoidanceClock = 0.0f;
						gAvoidingClock = 0.0f;
					}
				}				
                if (Random.Range(0.0f, 100.0f) > tempFloat2 || randomResetDir)
                {
                    //generate random points
                    float rad = Random.Range(15.0f, 65.0f) * Mathf.Deg2Rad;
                    Vector3 position = abdomenController.followWhenOff.right * Mathf.Sin(rad) + abdomenController.followWhenOff.up * Mathf.Cos(rad);
                    //randomPointLeft = abdomenController.followWhenOff.position + position * Random.Range(1.0f, 4.0f);
					randomPointLeft = abdomenController.followWhenOff.position + (abdomenController.followWhenOff.right * (-1.0f * (Random.Range(1.75f, 7.0f) * randomBaseOffset))) + (abdomenController.followWhenOff.up * (Random.Range(0.0f, tempFloat) * randomBaseHeight)) + (abdomenController.followWhenOff.forward * (5.0f * randomBaseDistance));
                    //randomPointLeft = abdomenController.followWhenOff.position - (abdomenController.followWhenOff.right * 30.0f) - (abdomenController.followWhenOff.up * 7.0f) + (abdomenController.followWhenOff.forward * 30.0f);
					if (gAvoid == 1.0f)
					{
						gAvoid = 0.0f;
						gAvoidanceClock = 0.0f;
						gAvoidingClock = 0.0f;
					}
                }
                if (Random.Range(0.0f, 100.0f) > tempFloat2 || randomResetDir)
                {
                    //generate random points
                    float rad = Random.Range(15.0f, 65.0f) * Mathf.Deg2Rad;
                    Vector3 position = abdomenController.followWhenOff.right * Mathf.Sin(rad) + abdomenController.followWhenOff.forward * Mathf.Cos(rad);
                    //randomPointRight = abdomenController.followWhenOff.position + position * Random.Range(1.0f, 4.0f) - (abdomenController.followWhenOff.up * 2.0f) + (abdomenController.followWhenOff.forward * 3.0f);
                    randomPointRight = abdomenController.followWhenOff.position + (abdomenController.followWhenOff.right * (1.0f * (Random.Range(1.75f, 7.0f) * randomBaseOffset))) + (abdomenController.followWhenOff.up * (Random.Range(0.0f, tempFloat) * randomBaseHeight)) + (abdomenController.followWhenOff.forward * (5.0f * randomBaseDistance));
					if (gAvoid == 1.0f)
					{
						gAvoid = 0.0f;
						gAvoidanceClock = 0.0f;
						gAvoidingClock = 0.0f;
					}
                }
                if (Random.Range(0.0f, 100.0f) > tempFloat2 || randomResetDir)
                {
                    //generate random points
                    float rad = Random.Range(0.0f, -5.0f) * Mathf.Deg2Rad;
                    Vector3 position = abdomenController.followWhenOff.up * Mathf.Sin(rad) + abdomenController.followWhenOff.forward * Mathf.Cos(rad);
                    randomPointUp = abdomenController.followWhenOff.position + (abdomenController.followWhenOff.right * Random.Range(-2.0f, 2.0f)) + (abdomenController.followWhenOff.up * (Random.Range(-3.0f, 1.0f) * randomBaseHeight)) + (abdomenController.followWhenOff.forward * (7.0f * randomBaseDistance));
					if (gAvoid == 1.0f)
					{
						gAvoid = 0.0f;
						gAvoidanceClock = 0.0f;
						gAvoidingClock = 0.0f;
					}
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
						headActivityBoost = Mathf.Clamp(headActivityBoost - tempFloat / 5.0f,0.0f,100.0f - lHandActivityBoost - rHandActivityBoost);
					}
					else
					{
						headActivityBoost = Mathf.Clamp(headActivityBoost + tempFloat * 2.0f,0.0f,100.0f);
					}
                }
                else
                {
                    playerHeadTimeout = Mathf.Max(playerHeadTimeout - movementFalloff, 0.0f);
                    if (playerHeadTimeout == 0.0f)
                    {
                        playerHeadMovement = false;
                    }
					if ((mainInterest != "Face" && secondInterest != "Face") || playerHeadToFaceRot > lookDirectAngle || headToEyeController > lookDirectAngle)
					{
						headActivityBoost = Mathf.Clamp(headActivityBoost - (tempFloat),0.0f,100.0f);
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

                playerHeadToHead = Vector3.Distance(headController.followWhenOff.position, playerFace);
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
                    if (person2Usable == false && playerHandsUsable == false)
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
				Duration = 0.1f;
				person2Usable = false;
				
				if (person2 != null)
				{
					//JSONStorable js = person2.GetStorableByID("geometry");
					//DAZCharacterSelector dcs = js as DAZCharacterSelector;
					//GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;
					//if (morphUI != null)
					//{
						//DAZMorph morphTemp = morphUI.GetMorphByDisplayName("Breast Height");
						person2IsMale = false;
						if (person2.gameObject.name == "Genesis2Male")//morphTemp == null)
						{
							person2IsMale = true;
						}
					//}
					person2Usable = true;
					playerHeadController = person2.GetStorableByID("headControl") as FreeControllerV3;
					playerChestController = person2.GetStorableByID("chestControl") as FreeControllerV3;
					playerLHandController = person2.GetStorableByID("lHandControl") as FreeControllerV3;
					playerRHandController = person2.GetStorableByID("rHandControl") as FreeControllerV3;
					playerPelvisController = person2.GetStorableByID("pelvisControl") as FreeControllerV3;
					playerTipController = person2.GetStorableByID("penisTipControl") as FreeControllerV3;
					playerTipBaseController = person2.GetStorableByID("penisBaseControl") as FreeControllerV3;
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
			SimpleJSON.JSONNode loadedSettings = new SimpleJSON.JSONClass();
			string tempPath = GetPluginPath();
			loadedSettings=SuperController.singleton.LoadJSON(tempPath + "\\Presets\\E-Motion_Defaults.json");
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
				uiEyeUpdate.val = loadedSettings["Minimum Time Between Eye Target Movements Excl Saccades"].AsFloat;
				uiHeadInterest.val = loadedSettings["Target Head Interest Rate Scale"].AsFloat;
				uiLHandInterest.val = loadedSettings["Target Left Hand Interest Rate Scale"].AsFloat;
				uiRHandInterest.val = loadedSettings["Target Right Hand Interest Rate Scale"].AsFloat;
				uiPenisInterest.val = loadedSettings["Target Pelvis Interest Rate Scale"].AsFloat;
				uiObjectInterest.val = loadedSettings["Target Object Interest Rate Scale"].AsFloat;
				uiIdleAmount.val = loadedSettings["Idle Movement Mult"].AsFloat;
				uiIdleArmAmount.val = loadedSettings["Idle Arm Movement Mult"].AsFloat;
				uiIdleChance.val = loadedSettings["Idle Movement Chance"].AsFloat;
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
				uiDirectLookDelay.val = loadedSettings["Gaze Direct Delay Mult"].AsFloat;
				uiMaterialMult.val = loadedSettings["Arousal Gloss Mult"].AsFloat;
				uiEffectMaterial.val = loadedSettings["Arousal Effects Gloss"].AsBool;
				uiSetupComplete.val = loadedSettings["Setup Complete"].AsBool;
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
        string GetPluginPath()
        {
            SuperController.singleton.currentSaveDir = SuperController.singleton.currentLoadDir;
            string pluginId = this.storeId.Split('_')[0];
            MVRPluginManager manager = containingAtom.GetStorableByID("PluginManager") as MVRPluginManager;
            string pathToScriptFile = manager.GetJSON(true, true)["plugins"][pluginId].Value;
            string pathToScriptFolder = pathToScriptFile.Substring(0, pathToScriptFile.LastIndexOfAny(new char[] { '/', '\\' }));
            return pathToScriptFolder;
        }
		
		public void LoadSounds()
		{
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In4.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In5.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In6.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In_Fast1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In_Fast2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In_Fast3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_In_Fast4.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Out1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Out2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Out3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Out4.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Out5.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Nose_In_Long1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Nose_In_Long2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Nose_In_Med1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Nose_In_Med2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Nose_In_Med3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Nose_Out_Long1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Nose_Out_Long2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Nose_Out_Long3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Nose_Out_Long4.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah4.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah5.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah6.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah7.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah8.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah9.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah10.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah11.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah12.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah13.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah14.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah15.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Aah16.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh4.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh5.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh6.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh7.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh8.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh9.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh10.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh11.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh12.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh13.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh14.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Ooh15.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm4.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm5.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm6.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm7.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm8.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm9.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm10.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm11.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm12.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm13.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm14.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm15.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Mmm16.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Yeah1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Yeah2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Yeah3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Yeah4.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Yeah5.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Yeah6.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Yeah7.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Breath_Mouth_Yeah8.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss2.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss3.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss4.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss5.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss6.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss7.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss8.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss9.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss10.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss_Mmm1.wav");
			URLAudioClipManager.singleton.QueueClip(@"file:///Custom/scripts/E-Motion/Sounds/Kiss_Mmm2.wav");
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

			uiUsePerson2 = new JSONStorableBool("Look at selected Person", false);
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

			uiDirectLookDelay = new JSONStorableFloat("Gaze Direct Delay Mult", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiDirectLookDelay);

			uiGazeSpeed = new JSONStorableFloat("Gaze Speed Mult", 2.0f, 0.0f, 7.0f, true, true);
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

			uiIdleArmAmount = new JSONStorableFloat("Idle Arm Movement Mult", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(uiIdleArmAmount);
			
			uiIdleArmOffset = new JSONStorableFloat("Idle Arm Pos Offset", 0.00f, -100.0f, 100.0f, true, true);
			RegisterFloat(uiIdleArmOffset);
			
			
			uiIdleChance = new JSONStorableFloat("Idle Movement Chance", 60.00f, 0.0f, 100.0f, true, true);
			RegisterFloat(uiIdleChance);

			uiIdleSpeed = new JSONStorableFloat("Idle Movement Speed", 1.00f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiIdleSpeed);

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

			uiMoodSpeed = new JSONStorableFloat("Mood Degrade Speed Mult", 1.0f, 0.0f, 10.0f, true, true);
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

			uiPenisInterest = new JSONStorableFloat("Penis Interest Mult", 1.0f, 0.0f, 3.0f, true, true);
			RegisterFloat(uiPenisInterest);

			uiRandomBaseDistance = new JSONStorableFloat("Random Base Distance", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiRandomBaseDistance);

			uiRandomBaseHeight = new JSONStorableFloat("Random Base Height", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiRandomBaseHeight);

			uiRandomBaseOffset = new JSONStorableFloat("Random Base Center Offset", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uiRandomBaseOffset);

			triggerArousal = new JSONStorableFloat("Arousal", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(triggerArousal);
			triggerValence = new JSONStorableFloat("Valence", 1.00f, 0.0f, 10.0f, true, true);
			RegisterFloat(triggerValence);

			uiSetupComplete = new JSONStorableBool("Plugin has been Setup", false);
			RegisterBool(uiSetupComplete);
		}

		public void CreateMainMenuUI()
		{
			CreateToggle(uiShowStats, true);
			CreateButton("Load Defaults", false).button.onClick.AddListener(() =>
			{
				loadDefaults();
			});
			CreateButton("Load Preset", false).button.onClick.AddListener(() =>
			{
				SuperController.singleton.fileBrowserUI.defaultPath = GetPluginPath() + "\\Presets\\";
				SuperController.singleton.fileBrowserUI.SetTextEntry(false);
				SuperController.singleton.fileBrowserUI.Show((path) =>
				{
					if (string.IsNullOrEmpty(path))
					{
						return;
					}
					SimpleJSON.JSONNode loadedSettings = new SimpleJSON.JSONClass();
					loadedSettings=SuperController.singleton.LoadJSON(path);
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
					uiEyeUpdate.val = loadedSettings["Minimum Time Between Eye Target Movements Excl Saccades"].AsFloat;
					uiHeadInterest.val = loadedSettings["Target Head Interest Rate Scale"].AsFloat;
					uiLHandInterest.val = loadedSettings["Target Left Hand Interest Rate Scale"].AsFloat;
					uiRHandInterest.val = loadedSettings["Target Right Hand Interest Rate Scale"].AsFloat;
					uiPenisInterest.val = loadedSettings["Target Pelvis Interest Rate Scale"].AsFloat;
					uiObjectInterest.val = loadedSettings["Target Object Interest Rate Scale"].AsFloat;
					uiIdleAmount.val = loadedSettings["Idle Movement Mult"].AsFloat;
					uiIdleArmAmount.val = loadedSettings["Idle Arm Movement Mult"].AsFloat;
					uiIdleChance.val = loadedSettings["Idle Movement Chance"].AsFloat;
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
					uiDirectLookDelay.val = loadedSettings["Gaze Direct Delay Mult"].AsFloat;
					uiMaterialMult.val = loadedSettings["Arousal Gloss Mult"].AsFloat;
					uiEffectMaterial.val = loadedSettings["Arousal Effects Gloss"].AsBool;
					uiSetupComplete.val = loadedSettings["Setup Complete"].AsBool;

					uiHeadAngleOffset.val = loadedSettings["Head Base Angle Offset"].AsFloat;
					uiMouthOpenOffset.val = loadedSettings["Morph Mouth Open Offset"].AsFloat;
					uiLipsCloseOffset.val = loadedSettings["Morph Lips Closed Offset"].AsFloat;

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

				});
			});
			CreateButton("Save Preset", true).button.onClick.AddListener(() =>
			{
				SuperController.singleton.fileBrowserUI.defaultPath = GetPluginPath() + "\\Presets\\"; // or path to your plugin
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
					if (uiDoHead.val){mySettings["Control Breath and Expression Morphs"] = "True";}else{mySettings["Control Breath and Expression Morphs"] = "False";}
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
					mySettings.Add("Target Pelvis Interest Rate Scale", new SimpleJSON.JSONData(uiPenisInterest.val));
					mySettings.Add("Target Object Interest Rate Scale", new SimpleJSON.JSONData(uiObjectInterest.val));
					mySettings.Add("Idle Movement Mult", new SimpleJSON.JSONData(uiIdleAmount.val));
					mySettings.Add("Idle Arm Movement Mult", new SimpleJSON.JSONData(uiIdleArmAmount.val));
					mySettings.Add("Idle Movement Chance", new SimpleJSON.JSONData(uiIdleChance.val));
					mySettings.Add("Idle Movement Speed", new SimpleJSON.JSONData(uiIdleSpeed.val));
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
					mySettings.Add("Gaze Direct Delay Mult", new SimpleJSON.JSONData(uiDirectLookDelay.val));
					mySettings.Add("Arousal Gloss Mult", new SimpleJSON.JSONData(uiMaterialMult.val));
					mySettings.Add("Head Base Angle Offset", new SimpleJSON.JSONData(uiHeadAngleOffset.val));
					mySettings.Add("Morph Mouth Open Offset", new SimpleJSON.JSONData(uiMouthOpenOffset.val));
					mySettings.Add("Morph Lips Closed Offset", new SimpleJSON.JSONData(uiLipsCloseOffset.val));
					mySettings.Add("Maximum Allowed Value For Smile Morphs", new SimpleJSON.JSONData(uiMaxMorphSmile.val));
					mySettings.Add("Maximum Amount To Close Eyes", new SimpleJSON.JSONData(uiEyeCloseMaxMorph.val));
					
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
		}

		public void CreatePersonalityUI()
		{
			CreateSlider(uiAgreeableness, false);
			CreateSlider(uiExtraversion, false);
			CreateSlider(uiStableness, false);
			CreateSlider(uiInterestSpeed, false);
			CreateSlider(uiInterestRate, false);
			CreateSlider(uiArousalSpeed, false);
			CreateSlider(uiValenceSpeed, false);
			CreateSlider(uiMoodSpeed, false);
			CreateSlider(uiExpressionChance, false);
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
			RemoveSlider(uiArousalSpeed);
			RemoveSlider(uiValenceSpeed);
			RemoveSlider(uiMoodSpeed);
			RemoveSlider(uiExpressionChance);
			RemoveSlider(uiAnimationSpeed);
			uiShowingPersonality = false;
			ColorButtons();
		}
		
		public void CreateTargetUI()
		{
			CreateToggle(uiUsePerson2, true);
			UIDynamicPopup udp = CreatePopup(uiFocusTarget, true);

			UIDynamicPopup udp2 = CreatePopup(uiObjectTarget, true);
			CreateToggle(uiTargetLook, true);
			CreateSlider(uiObjectInterest, true);
			CreateSlider(uiHeadInterest, true);
			CreateSlider(uiLHandInterest, true);
			CreateSlider(uiRHandInterest, true);
			CreateSlider(uiPenisInterest, true);
			uiShowingTarget = true;
			ColorButtons();
		}
		public void RemoveTargetUI()
		{
			RemoveToggle(uiUsePerson2);
			RemovePopup(uiFocusTarget);
			RemovePopup(uiObjectTarget);
			RemoveToggle(uiTargetLook);
			RemoveSlider(uiObjectInterest);
			RemoveSlider(uiHeadInterest);
			RemoveSlider(uiLHandInterest);
			RemoveSlider(uiRHandInterest);
			RemoveSlider(uiPenisInterest);
			uiShowingTarget = false;
			ColorButtons();
		}

		public void CreateFeatureControlUI()
		{
			CreateToggle(uiConfigHead, false);
			CreateToggle(uiDoHead, false);
			CreateToggle(uiDoMorphs, false);
			CreateToggle(uiDoSounds, false);
			CreateSlider(uiSoundVolume, false);
			CreateToggle(uiEffectMaterial, false);
			CreateSlider(uiMaterialMult, false);
			CreateToggle(uiDoKiss, false);
			CreateSlider(uiKissAmount, false);
			CreateToggle(uiDoBlowjob, false);
			CreateSlider(uiBlowjobAmount, false);
			CreateToggle(uiDoSex, false);
			CreateSlider(uiSexAmount, false);
			uiShowingFeatures = true;
			ColorButtons();
		}
		public void RemoveFeatureControlUI()
		{
			RemoveToggle(uiConfigHead);
			RemoveToggle(uiDoHead);
			RemoveToggle(uiDoMorphs);
			RemoveToggle(uiDoSounds);
			RemoveSlider(uiSoundVolume);
			RemoveToggle(uiEffectMaterial);
			RemoveSlider(uiMaterialMult);
			RemoveToggle(uiDoKiss);
			RemoveSlider(uiKissAmount);
			RemoveToggle(uiDoBlowjob);
			RemoveSlider(uiBlowjobAmount);
			RemoveToggle(uiDoSex);
			RemoveSlider(uiSexAmount);
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
			CreateSlider(uiDirectLookDelay, true);
			CreateSlider(uiGazeSpeed, true);
			CreateSlider(uiGazeVariation, true);
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
			RemoveSlider(uiDirectLookDelay);
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
			CreateSlider(uiIdleAmount, true);
			CreateSlider(uiIdleArmAmount, true);
			CreateSlider(uiIdleArmOffset, true);
			CreateSlider(uiIdleChance, true);
			CreateSlider(uiIdleSpeed, true);
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
			RemoveSlider(uiIdleArmAmount);
			RemoveSlider(uiIdleArmOffset);
			RemoveSlider(uiIdleChance);
			RemoveSlider(uiIdleSpeed);
			uiShowingIdleBreathing = false;
			ColorButtons();
		}

		public void CreateDistAngleUI()
		{
			CreateSlider(uiPersonalSpace, true);
			CreateSlider(uiInteractDist, true);
			CreateSlider(uiCloseToFaceDist, true);
			CreateSlider(uiKissingDist, true);
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
			CreateSlider(uiEyeCloseMaxMorph, false);
			CreateSlider(uiHeadAngleOffset, false);
		}
		private void RemoveCharacterControl()
		{
			RemoveSlider(uiMouthOpenOffset);
			RemoveSlider(uiLipsCloseOffset);
			RemoveSlider(uiMaxMorphSmile);
			RemoveSlider(uiEyeCloseMaxMorph);
			RemoveSlider(uiHeadAngleOffset);
			uiShowingCharacter = false;
			ColorButtons();
		}
	}
}