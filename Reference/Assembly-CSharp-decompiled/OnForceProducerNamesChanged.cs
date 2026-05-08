using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AssetBundles;
using Battlehub.RTCommon;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Leap.Unity;
using MeshVR;
using MeshVR.Hands;
using MHLab.PATCH.Settings;
using MHLab.PATCH.Utilities;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using SimpleJSON;
using uFileBrowser;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;

public class SuperController : MonoBehaviour
{
	public delegate void ScreenShotCallback(string imgPath);

	public enum GameMode
	{
		Edit,
		Play
	}

	public enum AlignRotationOffset
	{
		None,
		Right,
		Left
	}

	public enum ActiveUI
	{
		None,
		MainMenu,
		MainMenuOnly,
		SelectedOptions,
		MultiButtonPanel,
		EmbeddedScenePanel,
		OnlineBrowser,
		PackageBuilder,
		PackageManager,
		Custom
	}

	private enum SelectMode
	{
		Off,
		FilteredTargets,
		Targets,
		Controller,
		ForceReceiver,
		ForceProducer,
		Rigidbody,
		Atom,
		Possess,
		TwoStagePossess,
		PossessAndAlign,
		Unpossess,
		AnimationRecord,
		ArmedForRecord,
		Teleport,
		FreeMove,
		FreeMoveMouse,
		SaveScreenshot,
		Screenshot,
		Custom,
		CustomWithTargetControl,
		CustomWithVRTargetControl
	}

	public delegate void SelectControllerCallback(FreeControllerV3 fc);

	public delegate void SelectForceProducerCallback(ForceProducerV2 fp);

	public delegate void SelectForceReceiverCallback(ForceReceiver fr);

	public delegate void SelectRigidbodyCallback(Rigidbody rb);

	public delegate void SelectAtomCallback(Atom a);

	public delegate void OnForceReceiverNamesChanged(string[] receiverNames);

	public delegate void OnForceProducerNamesChanged(string[] producerNames);

	public delegate void OnRhythmControllerNamesChanged(string[] controllerNames);

	public delegate void OnFreeControllerNamesChanged(string[] controllerNames);

	public delegate void OnRigidbodyNamesChanged(string[] rigidbodyNames);

	[Serializable]
	public class AtomAsset
	{
		public string assetBundleName;

		public string assetName;

		public string category;
	}

	public delegate void OnAtomUIDsChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDsWithForceReceiversChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDsWithForceProducersChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDsWithFreeControllersChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDsWithRigidbodiesChanged(List<string> atomUIDs);

	public delegate void OnAtomUIDRename(string oldName, string newName);

	public enum ThumbstickFunction
	{
		GrabWorld,
		SwapAxis,
		Both
	}

	private static SuperController _singleton;

	[Tooltip("Add Atom Tab, Animation Tab, Audio Tab")]
	public bool disableAdvancedSceneEdit;

	public bool disableSaveSceneButton;

	public bool disableLoadSceneButton;

	public bool disableCustomUI;

	public bool disableBrowse;

	public string savesDir = "Saves\\";

	public string savesDirEditor = "Saves\\";

	protected string lastLoadDir = string.Empty;

	protected string loadedName;

	protected bool _isLoading;

	public PackageBuilder packageBuilder;

	public Transform packageBuilderUI;

	public PackageBuilder packageManager;

	public Transform packageManagerUI;

	protected List<Atom> _saveQueue;

	public bool packageMode;

	protected ZipOutputStream zos;

	protected Dictionary<string, bool> alreadyPackaged;

	protected HashSet<string> referencedVarPackages;

	public JSONNode loadJson;

	private AsyncFlag loadFlag;

	public bool enableStartScene = true;

	public JSONEmbed startJSONEmbedScene;

	public JSONEmbed newJSONEmbedScene;

	public bool disableUI;

	public bool alwaysEnablePointers;

	public bool disableNavigation;

	public bool disableVR;

	private bool onStartScene;

	public string startSceneName = "scene/default.json";

	public string startSceneAltName = "scene/MeshedVR/default.json";

	public string newSceneName = "scene/default.json";

	public string newSceneAltName = "scene/MeshedVR/default.json";

	public string[] editorSceneList;

	public Transform atomContainer;

	public FileBrowser fileBrowserUI;

	public FileBrowser mediaFileBrowserUI;

	public FileBrowser directoryBrowserUI;

	public MultiButtonPanel multiButtonPanel;

	protected string lastMediaDir = string.Empty;

	protected string lastScenePathDir = string.Empty;

	protected string lastBrowseDir = string.Empty;

	protected Dictionary<string, string> pathMigrationMappings;

	protected List<string> legacyDirectories;

	public Transform migratePathsPanel;

	public Text oldPathsText;

	public Text newPathsText;

	protected Dictionary<string, string> filesToMigrateMap;

	public Camera screenshotCamera;

	public Transform screenshotPreview;

	public Camera hiResScreenshotCamera;

	public Transform hiResScreenshotPreview;

	public Slider loResScreenShotCameraFOVSlider;

	[SerializeField]
	private float _loResScreenShotCameraFOV = 40f;

	public Slider hiResScreenShotCameraFOVSlider;

	[SerializeField]
	private float _hiResScreenShotCameraFOV = 40f;

	protected string savingName;

	protected ScreenShotCallback screenShotCallback;

	[SerializeField]
	private GameMode _gameMode;

	public Toggle editModeToggle;

	public Toggle playModeToggle;

	public Transform[] editModeOnlyTransforms;

	public Transform errorLogPanel;

	protected string errorLog;

	public Text allErrorsText;

	public Text allErrorsText2;

	public Text allErrorsCountText;

	public Text allErrorsCountText2;

	public Transform errorSplashTransform;

	protected int errorSplashCount;

	public int errorSplashTime = 500;

	public Transform msgLogPanel;

	protected string msgLog;

	public Text allMessagesText;

	public Text allMessagesText2;

	public Text allMessagesCountText;

	public Text allMessagesCountText2;

	public Transform msgSplashTransform;

	protected int msgSplashCount;

	public int msgSplashTime = 500;

	protected int _errorCount;

	protected int maxLength = 5000;

	protected int _msgCount;

	public float maxAngularVelocity = 20f;

	public float maxDepenetrationVelocity = 1f;

	protected List<AsyncFlag> waitResumeSimulationFlags;

	protected bool _pauseSimulation;

	public Transform waitTransform;

	public Text[] waitReasonTexts;

	protected int pauseFrames;

	protected bool hideWaitTransform;

	protected bool hiddenPause;

	protected AsyncFlag pauseSimulationTimerFlag;

	protected List<AsyncFlag> holdLoadCompleteFlags;

	public Transform loadingIcon;

	protected List<AsyncFlag> loadingIconFlags;

	public Toggle freezeAnimationToggle;

	public Toggle freezeAnimationToggleAlt;

	private bool _freezeAnimation;

	public string buttonToggleMainHUD = "ButtonStart";

	public Transform mainHUD;

	public Transform mainMenuUI;

	public UITabSelector mainMenuTabSelector;

	public UITabSelector userPrefsTabSelector;

	public Transform loadingUI;

	public Transform loadingUIAlt;

	public Transform loadingGeometry;

	public Slider loadingProgressSlider;

	public Slider loadingProgressSliderAlt;

	public Text loadingTextStatus;

	public Text loadingTextStatusAlt;

	public Transform sceneControlUI;

	public Transform sceneControlUIAlt;

	public VRWebBrowser onlineBrowser;

	public Transform onlineBrowserUI;

	public Transform embeddedSceneUI;

	public Transform[] loadSceneButtons;

	public Transform[] onlineBrowseSceneButtons;

	public Transform[] saveSceneButtons;

	public Transform addAtomUI;

	public Transform addAtomUIAlt;

	public Transform animationUI;

	public Transform audioUI;

	public float targetAlpha;

	public Material rayLineMaterialRight;

	public Material rayLineMaterialLeft;

	private bool drawRayLineLeft;

	private bool drawRayLineRight;

	private LineDrawer rayLineDrawerRight;

	private LineDrawer rayLineDrawerLeft;

	public LineRenderer rayLineRight;

	public LineRenderer rayLineLeft;

	public float rayLineWidth = 0.004f;

	public Toggle quickSelectMoveAndAlignToggle;

	public UIPopup selectAtomPopup;

	public UIPopup selectControllerPopup;

	public Material twoStageLineMaterial;

	private LineDrawer rightTwoStageLineDrawer;

	private LineDrawer leftTwoStageLineDrawer;

	private LineDrawer headTwoStageLineDrawer;

	private LineDrawer leapRightTwoStageLineDrawer;

	private LineDrawer leapLeftTwoStageLineDrawer;

	private LineDrawer tracker1TwoStageLineDrawer;

	private LineDrawer tracker2TwoStageLineDrawer;

	private LineDrawer tracker3TwoStageLineDrawer;

	private LineDrawer tracker4TwoStageLineDrawer;

	private LineDrawer tracker5TwoStageLineDrawer;

	private LineDrawer tracker6TwoStageLineDrawer;

	private LineDrawer tracker7TwoStageLineDrawer;

	private LineDrawer tracker8TwoStageLineDrawer;

	public string version = "UNOFFICIAL";

	public Text versionText;

	protected string resolvedVersion;

	protected string lastCycleSelectAtomType;

	protected string lastCycleSelectAtomUid;

	public UIPopup alignRotationOffsetPopup;

	protected AlignRotationOffset _alignRotationOffset = AlignRotationOffset.Left;

	private ActiveUI _lastActiveUI;

	private Transform customUI;

	public Transform alternateCustomUI;

	private ActiveUI _activeUI = ActiveUI.SelectedOptions;

	public Toggle helpToggle;

	public Toggle helpToggleAlt;

	public Transform helpOverlayOVR;

	public Transform helpOverlayVive;

	private bool _helpOverlayOnAux = true;

	[SerializeField]
	private bool _helpOverlayOn = true;

	protected string tempHelpText;

	public Text helpHUDText;

	protected string _helpText;

	protected Color _helpColor;

	public Transform leftHand;

	public Transform leftHandAlternate;

	public Transform rightHand;

	public Transform rightHandAlternate;

	public HandModelControl commonHandModelControl;

	public HandModelControl alternateControllerHandModelControl;

	private HandControl leftHandControl;

	private HandControl rightHandControl;

	public Transform handsContainer;

	public OVRHandInput ovrHandInputLeft;

	public OVRHandInput ovrHandInputRight;

	public SteamVRHandInput steamVRHandInputLeft;

	public SteamVRHandInput steamVRHandInputRight;

	public Toggle alwaysUseAlternateHandsToggle;

	[SerializeField]
	protected bool _alwaysUseAlternateHands;

	public Transform mouseGrab;

	public Camera leftControllerCamera;

	public Camera rightControllerCamera;

	private FreeControllerV3 rightGrabbedController;

	private bool rightGrabbedControllerIsRemote;

	private FreeControllerV3 leftGrabbedController;

	private bool leftGrabbedControllerIsRemote;

	private FreeControllerV3 rightFullGrabbedController;

	private bool rightFullGrabbedControllerIsRemote;

	private FreeControllerV3 leftFullGrabbedController;

	private bool leftFullGrabbedControllerIsRemote;

	public Transform worldScaleTransform;

	public Slider worldScaleSlider;

	public Slider worldScaleSliderAlt;

	private float _worldScale = 1f;

	public Slider controllerScaleSlider;

	private float _controllerScale = 1f;

	public LayerMask targetColliderMask;

	public Transform selectPrefab;

	public LayerMask selectColliderMask;

	private FreeControllerV3 selectedController;

	public FCPositionHandle selectedControllerPositionHandle;

	public FCRotationHandle selectedControllerRotationHandle;

	public Text selectedControllerNameDisplay;

	private SelectMode selectMode;

	public Transform selectionHUDTransform;

	private SelectionHUD selectionHUD;

	public SelectionHUD mouseSelectionHUD;

	private List<FreeControllerV3> highlightedControllersLook;

	private List<SelectTarget> highlightedSelectTargetsLook;

	private List<FreeControllerV3> highlightedControllersMouse;

	private List<SelectTarget> highlightedSelectTargetsMouse;

	public Transform rightSelectionHUDTransform;

	public Transform leftSelectionHUDTransform;

	private SelectionHUD rightSelectionHUD;

	private SelectionHUD leftSelectionHUD;

	private List<FreeControllerV3> highlightedControllersLeft;

	private List<FreeControllerV3> highlightedControllersRight;

	private List<SelectTarget> highlightedSelectTargetsLeft;

	private List<SelectTarget> highlightedSelectTargetsRight;

	private HashSet<FreeControllerV3> onlyShowControllers;

	private List<Transform> selectionInstances;

	private SelectControllerCallback selectControllerCallback;

	private SelectForceProducerCallback selectForceProducerCallback;

	private SelectForceReceiverCallback selectForceReceiverCallback;

	private SelectRigidbodyCallback selectRigidbodyCallback;

	private SelectAtomCallback selectAtomCallback;

	public SteamVR_Action_Boolean menuAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Menu");

	public SteamVR_Action_Boolean UIInteractAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("UIInteract");

	public SteamVR_Action_Boolean targetShowAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TargetShow");

	public SteamVR_Action_Boolean cycleEngageAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("CycleEngage");

	public SteamVR_Action_Vector2 cycleUsingXAxisAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("CycleUsingXAxis");

	public SteamVR_Action_Vector2 cycleUsingYAxisAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("CycleUsingYAxis");

	public SteamVR_Action_Boolean selectAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Select");

	public SteamVR_Action_Vector2 pushPullAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("PushPullNode");

	public SteamVR_Action_Boolean teleportShowAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TeleportShow");

	public SteamVR_Action_Boolean teleportAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");

	public SteamVR_Action_Boolean grabNavigateAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabNavigate");

	public SteamVR_Action_Vector2 freeMoveAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("FreeMove");

	public SteamVR_Action_Vector2 freeModeMoveAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("FreeModeMove");

	public SteamVR_Action_Boolean swapFreeMoveAxis = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("SwapFreeMoveAxis");

	public SteamVR_Action_Boolean grabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Grab");

	public SteamVR_Action_Boolean holdGrabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("HoldGrab");

	public SteamVR_Action_Single grabValAction = SteamVR_Input.GetAction<SteamVR_Action_Single>("GrabVal");

	public SteamVR_Action_Boolean remoteGrabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("RemoteGrab");

	public SteamVR_Action_Boolean remoteHoldGrabAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("RemoteHoldGrab");

	public SteamVR_Action_Boolean toggleHandAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("ToggleHand");

	public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

	public bool useLookSelect;

	public Camera lookCamera;

	public string buttonToggleTargets = "ButtonBack";

	private bool targetsOnWithButton;

	public string buttonSelect = "ButtonA";

	public string buttonUnselect = "ButtonB";

	public string buttonToggleRotateMode = "ButtonY";

	public string buttonCycleSelection = "ButtonX";

	public JoystickControl.Axis navigationForwardAxis = JoystickControl.Axis.LeftStickY;

	public bool invertNavigationForwardAxis;

	public JoystickControl.Axis navigationSideAxis = JoystickControl.Axis.LeftStickX;

	public bool invertNavigationSideAxis;

	public JoystickControl.Axis navigationTurnAxis = JoystickControl.Axis.RightStickX;

	public bool invertNavigationTurnAxis;

	public JoystickControl.Axis navigationUpAxis = JoystickControl.Axis.RightStickY;

	public bool invertNavigationUpAxis;

	public bool invertAxis1 = true;

	public bool invertAxis2 = true;

	public bool invertAxis3 = true;

	private bool _swapAxis;

	private bool leftGUIInteract;

	private bool rightGUIInteract;

	private bool _setLeftSelect;

	private bool _setRightSelect;

	protected bool remoteHoldGrabDisabled;

	public LayerMask lookAtTriggerMask;

	private LookAtTrigger currentLookAtTrigger;

	private bool _pointerModeLeft;

	private bool _pointerModeRight;

	protected Dictionary<FreeControllerV3, bool> wasHitFC;

	protected List<FreeControllerV3> overlappingFcs;

	protected List<FreeControllerV3> alreadyDisplayed;

	protected Collider[] overlappingControls;

	protected bool _allowGrabPlusTriggerHandToggle = true;

	public Toggle allowGrabPlusTriggerHandToggleToggle;

	private float cycleClick = 0.25f;

	private bool _leftCycleOn;

	private float _leftCycleXPosition;

	private float _leftCycleYPosition;

	private int leftCycleX;

	private int leftCycleY;

	private bool _rightCycleOn;

	private float _rightCycleXPosition;

	private float _rightCycleYPosition;

	private int rightCycleX;

	private int rightCycleY;

	private bool isLeftOverlap;

	private bool isRightOverlap;

	private FreeControllerV3 potentialGrabbedControllerMouse;

	private FreeControllerV3 grabbedControllerMouse;

	private float grabbedControllerMouseDistance;

	private Vector3 mouseDownPosition;

	private Vector3 lastMousePosition;

	private Vector3 mouseDownLastWorldPosition;

	private bool dragActivated;

	private bool mouseClickUsed;

	private bool eligibleForMouseSelect;

	public UIPopup UISidePopup;

	[SerializeField]
	protected UISideAlign.Side _UISide = UISideAlign.Side.Right;

	public Transform mainHUDAttachPoint;

	public Transform mainHUDPivot;

	public float mainHUDPivotXRotationVR = -30f;

	public float mainHUDPivotXRotationMonitor;

	private Vector3 mainHUDAttachPointStartingPosition;

	private Quaternion mainHUDAttachPointStartingRotation;

	public bool showMainHUDOnStart;

	private bool _mainHUDVisible;

	private bool _mainHUDAnchoredOnMonitor;

	private bool GUIhit;

	private bool GUIhitLeft;

	private bool GUIhitRight;

	private bool GUIhitMouse;

	public bool overrideCanvasSortingLayer;

	public string overrideCanvasSortingLayerName;

	protected Dictionary<SelectTarget, bool> wasHitST;

	protected RaycastHit[] raycastHits;

	private FreeControllerV3 rightPossessedController;

	private FreeControllerV3 rightStartPossessedController;

	private FreeControllerV3 leftPossessedController;

	private FreeControllerV3 leftStartPossessedController;

	private FreeControllerV3 headPossessedController;

	private FreeControllerV3 headStartPossessedController;

	private FreeControllerV3 tracker1PossessedController;

	private FreeControllerV3 tracker1StartPossessedController;

	private FreeControllerV3 tracker2PossessedController;

	private FreeControllerV3 tracker2StartPossessedController;

	private FreeControllerV3 tracker3PossessedController;

	private FreeControllerV3 tracker3StartPossessedController;

	private FreeControllerV3 tracker4PossessedController;

	private FreeControllerV3 tracker4StartPossessedController;

	private FreeControllerV3 tracker5PossessedController;

	private FreeControllerV3 tracker5StartPossessedController;

	private FreeControllerV3 tracker6PossessedController;

	private FreeControllerV3 tracker6StartPossessedController;

	private FreeControllerV3 tracker7PossessedController;

	private FreeControllerV3 tracker7StartPossessedController;

	private FreeControllerV3 tracker8PossessedController;

	private FreeControllerV3 tracker8StartPossessedController;

	private HandControl leftPossessHandControl;

	private HandControl leapLeftPossessHandControl;

	private HandControl rightPossessHandControl;

	private HandControl leapRightPossessHandControl;

	public Toggle allowPossessSpringAdjustmentToggle;

	[SerializeField]
	private bool _allowPossessSpringAdjustment = true;

	public Slider possessPositionSpringSlider;

	[SerializeField]
	private float _possessPositionSpring = 10000f;

	public Slider possessRotationSpringSlider;

	[SerializeField]
	private float _possessRotationSpring = 1000f;

	public MotionAnimationMaster motionAnimationMaster;

	protected bool isRecording;

	public bool assetManagerSimulateInEditor = true;

	private bool _assetManagerReady;

	protected Dictionary<string, GameObject> assetBundleAssetNameToPrefab;

	protected Dictionary<string, int> assetBundleAssetNameRefCounts;

	private Dictionary<string, bool> uids;

	private Dictionary<string, Atom> atoms;

	private Dictionary<string, Atom> startingAtoms;

	private bool _pauseSyncAtomLists;

	private List<string> atomUIDs;

	private List<string> atomUIDsWithForceReceivers;

	private List<string> atomUIDsWithForceProducers;

	private List<string> atomUIDsWithRhythmControllers;

	private List<string> atomUIDsWithFreeControllers;

	private List<string> atomUIDsWithRigidbodies;

	private List<string> sortedAtomUIDs;

	private List<string> sortedAtomUIDsWithForceReceivers;

	private List<string> sortedAtomUIDsWithForceProducers;

	private List<string> sortedAtomUIDsWithRhythmControllers;

	private List<string> sortedAtomUIDsWithFreeControllers;

	private List<string> sortedAtomUIDsWithRigidbodies;

	private List<string> hiddenAtomUIDs;

	private List<string> hiddenAtomUIDsWithFreeControllers;

	private List<string> visibleAtomUIDs;

	private List<string> visibleAtomUIDsWithFreeControllers;

	public bool sortAtomUIDs = true;

	public Toggle showHiddenAtomsToggle;

	public Toggle showHiddenAtomsToggleAlt;

	protected bool _showHiddenAtoms;

	private List<FreeControllerV3> allControllers;

	private List<AnimationPattern> allAnimationPatterns;

	private List<AnimationStep> allAnimationSteps;

	private List<Animator> allAnimators;

	private List<Canvas> allCanvases;

	private Dictionary<string, ForceReceiver> frMap;

	private Dictionary<string, ForceProducerV2> fpMap;

	private Dictionary<string, FreeControllerV3> fcMap;

	private Dictionary<string, RhythmController> rcMap;

	private Dictionary<string, Rigidbody> rbMap;

	private Dictionary<string, GrabPoint> gpMap;

	private Dictionary<string, MotionAnimationControl> macMap;

	private Dictionary<string, PlayerNavCollider> pncMap;

	private int maxUID = 1000;

	public OnForceReceiverNamesChanged onForceReceiverNamesChangedHandlers;

	private string[] _forceReceiverNames;

	public OnForceProducerNamesChanged onForceProducerNamesChangedHandlers;

	private string[] _forceProducerNames;

	public OnRhythmControllerNamesChanged onRhythmControllerNamesChangedHandlers;

	private string[] _rhythmControllerNames;

	public OnFreeControllerNamesChanged onFreeControllerNamesChangedHandlers;

	private string[] _freeControllerNames;

	public OnRigidbodyNamesChanged onRigidbodyNamesChangedHandlers;

	private string[] _rigidbodyNames;

	public string atomAssetsFile;

	public AtomAsset[] atomAssets;

	public AtomAsset[] indirectAtomAssets;

	public Atom[] atomPrefabs;

	public Atom[] indirectAtomPrefabs;

	protected Dictionary<string, Atom> atomPrefabByType;

	protected Dictionary<string, AtomAsset> atomAssetByType;

	protected List<string> atomTypes;

	protected List<string> atomCategories;

	protected Dictionary<string, List<string>> atomCategoryToAtomTypes;

	public string atomCategory;

	public UIPopup atomCategoryPopup;

	public UIPopup atomPrefabPopup;

	public OnAtomUIDsChanged onAtomUIDsChangedHandlers;

	public OnAtomUIDsWithForceReceiversChanged onAtomUIDsWithForceReceiversChangedHandlers;

	public OnAtomUIDsWithForceProducersChanged onAtomUIDsWithForceProducersChangedHandlers;

	public OnAtomUIDsWithFreeControllersChanged onAtomUIDsWithFreeControllersChangedHandlers;

	public OnAtomUIDsWithRigidbodiesChanged onAtomUIDsWithRigidbodiesChangedHandlers;

	public OnAtomUIDRename onAtomUIDRenameHandlers;

	public Transform navigationPlayArea;

	public Transform regularPlayArea;

	public Transform navigationRig;

	public Transform navigationRigParent;

	public Transform navigationPlayer;

	public Transform navigationCamera;

	public Transform navigationHologrid;

	protected bool navigationHologridVisible;

	protected float navigationHologridShowTime;

	protected float navigationHologridTransparencyMultiplier = 1f;

	public Toggle showNavigationHologridToggle;

	[SerializeField]
	private bool _showNavigationHologrid = true;

	public Slider hologridTransparencySlider;

	[SerializeField]
	private float _hologridTransparency = 0.01f;

	protected Vector3 sceneLoadPosition;

	protected Quaternion sceneLoadRotation;

	protected float sceneLoadPlayerHeightAdjust;

	public Toggle useSceneLoadPositionToggle;

	protected bool _useSceneLoadPosition;

	public CubicBezierCurve navigationCurve;

	public float navigationDistance = 100f;

	public bool useLookForNavigation = true;

	public LayerMask navigationColliderMask;

	public Toggle lockHeightDuringNavigateToggle;

	public Toggle lockHeightDuringNavigateToggleAlt;

	[SerializeField]
	private bool _lockHeightDuringNavigate = true;

	public Toggle disableAllNavigationToggle;

	[SerializeField]
	private bool _disableAllNavigation;

	public Toggle freeMoveFollowFloorToggle;

	public Toggle freeMoveFollowFloorToggleAlt;

	[SerializeField]
	private bool _freeMoveFollowFloor = true;

	public Toggle teleportAllowRotationToggle;

	[SerializeField]
	private bool _teleportAllowRotation;

	public Toggle disableTeleportToggle;

	[SerializeField]
	private bool _disableTeleport;

	public Toggle disableTeleportDuringPossessToggle;

	[SerializeField]
	private bool _disableTeleportDuringPossess = true;

	public Slider freeMoveMultiplierSlider;

	[SerializeField]
	private float _freeMoveMultiplier = 1f;

	public Toggle disableGrabNavigationToggle;

	[SerializeField]
	private bool _disableGrabNavigation;

	public Slider grabNavigationPositionMultiplierSlider;

	[SerializeField]
	private float _grabNavigationPositionMultiplier = 1f;

	public Slider grabNavigationRotationMultiplierSlider;

	[SerializeField]
	private float _grabNavigationRotationMultiplier = 0.5f;

	private float _grabNavigationRotationResistance = 0.1f;

	private Vector3 startNavigatePosition;

	private Vector3 startGrabNavigatePositionRight;

	private Vector3 startGrabNavigatePositionLeft;

	private bool isGrabNavigatingRight;

	private bool isGrabNavigatingLeft;

	private Quaternion startNavigateRotation;

	private Quaternion startGrabNavigateRotationRight;

	private Quaternion startGrabNavigateRotationLeft;

	private bool startedTeleport;

	private PlayerNavCollider teleportPlayerNavCollider;

	private PlayerNavCollider playerNavCollider;

	private GameObject playerNavTrackerGO;

	public Transform heightAdjustTransform;

	public Slider playerHeightAdjustSlider;

	public Slider playerHeightAdjustSliderAlt;

	private float _playerHeightAdjust;

	private Ray castRay;

	private MeshRenderer regularPlayAreaMR;

	private MeshRenderer navigationPlayAreaMR;

	private MeshRenderer navigationPlayerMR;

	private MeshRenderer navigationCameraMR;

	private bool isTeleporting;

	private bool didStartLeftNavigate;

	private bool didStartRightNavigate;

	public float focusDistance = 1.5f;

	private int _solverIterations = 15;

	private bool _useInterpolation = true;

	public bool disableLeap;

	public Transform LeapRig;

	public LeapXRServiceProvider[] LeapServiceProviders;

	public Transform leapHandLeft;

	public Transform leapHandRight;

	public Transform leapHandMountLeft;

	public Transform leapHandMountRight;

	public LeapHandModelControl leapHandModelControl;

	private FreeControllerV3 leapLeftPossessedController;

	private FreeControllerV3 leapLeftStartPossessedController;

	private FreeControllerV3 leapRightPossessedController;

	private FreeControllerV3 leapRightStartPossessedController;

	protected bool _leapHandLeftConnected;

	protected bool _leapHandRightConnected;

	public Transform viveTracker1;

	public SteamVR_RenderModel viveTracker1Model;

	public Transform viveTracker2;

	public SteamVR_RenderModel viveTracker2Model;

	public Transform viveTracker3;

	public SteamVR_RenderModel viveTracker3Model;

	public Transform viveTracker4;

	public SteamVR_RenderModel viveTracker4Model;

	public Transform viveTracker5;

	public SteamVR_RenderModel viveTracker5Model;

	public Transform viveTracker6;

	public SteamVR_RenderModel viveTracker6Model;

	public Transform viveTracker7;

	public SteamVR_RenderModel viveTracker7Model;

	public Transform viveTracker8;

	public SteamVR_RenderModel viveTracker8Model;

	protected bool _hideTrackers;

	protected bool _tracker1Visible = true;

	protected bool _tracker2Visible = true;

	protected bool _tracker3Visible = true;

	protected bool _tracker4Visible = true;

	protected bool _tracker5Visible = true;

	protected bool _tracker6Visible = true;

	protected bool _tracker7Visible = true;

	protected bool _tracker8Visible = true;

	public bool isOVR;

	public bool isOpenVR;

	public CameraTarget centerCameraTarget;

	private bool MonitorRigActive;

	private bool isMonitorOnly;

	public Transform MonitorRig;

	public Camera MonitorCenterCamera;

	public Vector3 MonitorCenterCameraOffset = Vector3.zero;

	public Transform MonitorUI;

	public Transform MonitorUIAnchor;

	public Transform MonitorUIAttachPoint;

	public Transform MonitorModeAuxUI;

	public Button MonitorModeButton;

	protected bool _toggleMonitorSaveMainHUDVisible;

	public Slider monitorUIScaleSlider;

	public float fixedMonitorUIScale = 1f;

	private float _monitorUIScale = 1f;

	public Slider monitorUIYOffsetSlider;

	private float _monitorUIYOffset;

	public Slider monitorCameraFOVSlider;

	[SerializeField]
	private float _monitorCameraFOV = 40f;

	private Transform saveCenterEyeAttachPoint;

	public Transform OVRRig;

	public Camera OVRCenterCamera;

	public Transform touchObjectLeft;

	public Transform touchHandMountLeft;

	public Transform touchCenterHandLeft;

	public Transform touchObjectRight;

	public Transform touchHandMountRight;

	public Transform touchCenterHandRight;

	public UIPopup oculusThumbstickFunctionPopup;

	[SerializeField]
	protected ThumbstickFunction _oculusThumbstickFunction;

	public Transform ViveRig;

	public Camera ViveCenterCamera;

	public Transform viveObjectLeft;

	public Transform viveHandMountLeft;

	public Transform viveCenterHandLeft;

	public Transform viveObjectRight;

	public Transform viveHandMountRight;

	public Transform viveCenterHandRight;

	public static SuperController singleton => _singleton;

	protected bool advancedSceneEditDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableAdvancedSceneEdit;
			}
			return disableAdvancedSceneEdit;
		}
	}

	protected bool saveSceneButtonDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableSaveSceneButton;
			}
			return disableSaveSceneButton;
		}
	}

	protected bool loadSceneButtonDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableLoadSceneButton;
			}
			return disableLoadSceneButton;
		}
	}

	protected bool customUIDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableCustomUI;
			}
			return disableCustomUI;
		}
	}

	protected bool browseDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableBrowse;
			}
			return disableBrowse;
		}
	}

	public string currentSaveDir
	{
		get
		{
			return FileManager.CurrentSaveDir;
		}
		set
		{
			FileManager.SetSaveDir(value);
		}
	}

	public string currentLoadDir
	{
		get
		{
			return FileManager.CurrentLoadDir;
		}
		set
		{
			FileManager.SetLoadDir(value, restrictPath: true);
		}
	}

	public bool isLoading => _isLoading;

	public string savesDirResolved
	{
		get
		{
			if (Application.isEditor)
			{
				return savesDirEditor;
			}
			return savesDir;
		}
	}

	protected bool startSceneEnabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.enableStartScene;
			}
			return enableStartScene;
		}
	}

	protected JSONEmbed embeddedJSONScene
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.startJSONEmbedScene;
			}
			return startJSONEmbedScene;
		}
	}

	protected bool UIDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableUI;
			}
			return disableUI;
		}
	}

	protected bool pointersAlwaysEnabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.alwaysEnablePointers;
			}
			return alwaysEnablePointers;
		}
	}

	protected bool navigationDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableNavigation;
			}
			return disableNavigation;
		}
	}

	protected bool VRDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableVR;
			}
			return disableVR;
		}
	}

	protected Transform atomContainerTransform
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.atomContainer;
			}
			return atomContainer;
		}
	}

	public float loResScreenShotCameraFOV
	{
		get
		{
			return _loResScreenShotCameraFOV;
		}
		set
		{
			if (_loResScreenShotCameraFOV != value)
			{
				_loResScreenShotCameraFOV = value;
				if (screenshotCamera != null)
				{
					screenshotCamera.fieldOfView = _loResScreenShotCameraFOV;
				}
				if (loResScreenShotCameraFOVSlider != null)
				{
					loResScreenShotCameraFOVSlider.value = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float hiResScreenShotCameraFOV
	{
		get
		{
			return _hiResScreenShotCameraFOV;
		}
		set
		{
			if (_hiResScreenShotCameraFOV != value)
			{
				_hiResScreenShotCameraFOV = value;
				if (hiResScreenshotCamera != null)
				{
					hiResScreenshotCamera.fieldOfView = _hiResScreenShotCameraFOV;
				}
				if (hiResScreenShotCameraFOVSlider != null)
				{
					hiResScreenShotCameraFOVSlider.value = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public GameMode gameMode
	{
		get
		{
			return _gameMode;
		}
		set
		{
			if (_gameMode != value)
			{
				_gameMode = value;
				SyncGameMode();
			}
		}
	}

	public int errorCount
	{
		get
		{
			return _errorCount;
		}
		set
		{
			if (_errorCount != value)
			{
				_errorCount = value;
				if (allErrorsCountText != null)
				{
					allErrorsCountText.text = _errorCount.ToString();
				}
				if (allErrorsCountText2 != null)
				{
					allErrorsCountText2.text = _errorCount.ToString();
				}
			}
		}
	}

	public int msgCount
	{
		get
		{
			return _msgCount;
		}
		set
		{
			if (_msgCount != value)
			{
				_msgCount = value;
				if (allMessagesCountText != null)
				{
					allMessagesCountText.text = _msgCount.ToString();
				}
				if (allMessagesCountText2 != null)
				{
					allMessagesCountText2.text = _msgCount.ToString();
				}
			}
		}
	}

	protected bool pauseSimulation
	{
		get
		{
			return _pauseSimulation;
		}
		set
		{
			if (_pauseSimulation != value)
			{
				_pauseSimulation = value;
			}
		}
	}

	public bool freezeAnimation => _freezeAnimation || _isLoading || _pauseSimulation;

	public AlignRotationOffset alignRotationOffset
	{
		get
		{
			return _alignRotationOffset;
		}
		set
		{
			if (_alignRotationOffset != value)
			{
				_alignRotationOffset = value;
				if (alignRotationOffsetPopup != null)
				{
					alignRotationOffsetPopup.currentValue = _alignRotationOffset.ToString();
				}
			}
		}
	}

	public ActiveUI lastActiveUI => _lastActiveUI;

	public ActiveUI activeUI
	{
		get
		{
			return _activeUI;
		}
		set
		{
			if (_activeUI != value)
			{
				_lastActiveUI = _activeUI;
				_activeUI = value;
			}
			switch (_activeUI)
			{
				case ActiveUI.None:
					ClearAllUI();
					break;
				case ActiveUI.MainMenu:
					ClearAllUI();
					if (mainMenuUI != null)
					{
						mainMenuUI.gameObject.SetActive(value: true);
					}
					break;
				case ActiveUI.MainMenuOnly:
					ClearAllUI();
					if (mainMenuUI != null)
					{
						mainMenuUI.gameObject.SetActive(value: true);
					}
					if (sceneControlUI != null)
					{
						sceneControlUI.gameObject.SetActive(value: false);
					}
					if (sceneControlUIAlt != null)
					{
						sceneControlUIAlt.gameObject.SetActive(value: false);
					}
					break;
				case ActiveUI.SelectedOptions:
					ClearAllUI();
					if (selectedController != null && _mainHUDVisible)
					{
						selectedController.guihidden = false;
					}
					break;
				case ActiveUI.MultiButtonPanel:
					ClearAllUI();
					if (multiButtonPanel != null)
					{
						multiButtonPanel.gameObject.SetActive(value: true);
					}
					break;
				case ActiveUI.EmbeddedScenePanel:
					ClearAllUI();
					if (embeddedSceneUI != null)
					{
						embeddedSceneUI.gameObject.SetActive(value: true);
					}
					break;
				case ActiveUI.OnlineBrowser:
					ClearAllUI();
					if (UserPreferences.singleton == null || UserPreferences.singleton.enableWebBrowser)
					{
						if (onlineBrowserUI != null)
						{
							onlineBrowserUI.gameObject.SetActive(value: true);
						}
					}
					else
					{
						Error("Web Browsing is disabled. To use this feature you must enable browser in User Preferences -> Security tab");
						SetActiveUI("MainMenu");
						SetMainMenuTab("TabUserPrefs");
						SetUserPrefsTab("TabSecurity");
					}
					break;
				case ActiveUI.PackageBuilder:
					ClearAllUI();
					if (packageBuilderUI != null)
					{
						packageBuilderUI.gameObject.SetActive(value: true);
					}
					break;
				case ActiveUI.PackageManager:
					ClearAllUI();
					if (packageManagerUI != null)
					{
						packageManagerUI.gameObject.SetActive(value: true);
					}
					break;
				case ActiveUI.Custom:
					ClearAllUI();
					if (customUI != null)
					{
						customUI.gameObject.SetActive(value: true);
					}
					break;
			}
			SyncVisibility();
		}
	}

	public bool helpOverlayOn
	{
		get
		{
			return _helpOverlayOn;
		}
		set
		{
			if (_helpOverlayOn != value)
			{
				_helpOverlayOn = value;
				SyncHelpOverlay();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public string helpText
	{
		get
		{
			return _helpText;
		}
		set
		{
			if (_helpText != value)
			{
				_helpText = value;
				SyncHelpText();
			}
		}
	}

	public Color helpColor
	{
		get
		{
			return _helpColor;
		}
		set
		{
			if (_helpColor != value)
			{
				_helpColor = value;
				SyncHelpText();
			}
		}
	}

	public bool alwaysUseAlternateHands
	{
		get
		{
			return _alwaysUseAlternateHands;
		}
		set
		{
			if (_alwaysUseAlternateHands != value)
			{
				_alwaysUseAlternateHands = value;
				if (alwaysUseAlternateHandsToggle != null)
				{
					alwaysUseAlternateHandsToggle.isOn = _alwaysUseAlternateHands;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
				SyncActiveHands();
			}
		}
	}

	public FreeControllerV3 RightGrabbedController => rightGrabbedController;

	public FreeControllerV3 LeftGrabbedController => leftGrabbedController;

	public FreeControllerV3 RightFullGrabbedController => rightFullGrabbedController;

	public FreeControllerV3 LeftFullGrabbedController => leftFullGrabbedController;

	public float worldScale
	{
		get
		{
			return _worldScale;
		}
		set
		{
			if (_worldScale != value)
			{
				_worldScale = value;
				if (worldScaleSlider != null)
				{
					worldScaleSlider.value = _worldScale;
				}
				if (worldScaleSliderAlt != null)
				{
					worldScaleSliderAlt.value = _worldScale;
				}
				SplashNavigationHologrid(1f);
				if (rayLineLeft != null)
				{
					rayLineLeft.startWidth = rayLineWidth * _worldScale;
					rayLineLeft.endWidth = rayLineLeft.startWidth;
				}
				if (rayLineRight != null)
				{
					rayLineRight.startWidth = rayLineWidth * _worldScale;
					rayLineRight.endWidth = rayLineRight.startWidth;
				}
				Vector3 vector = Vector3.zero;
				if (centerCameraTarget != null)
				{
					vector = centerCameraTarget.transform.position;
				}
				Vector3 localScale = new Vector3(_worldScale, _worldScale, _worldScale);
				if ((bool)worldScaleTransform)
				{
					worldScaleTransform.localScale = localScale;
				}
				else
				{
					base.transform.localScale = localScale;
				}
				if (centerCameraTarget != null && navigationRig != null)
				{
					Vector3 position = centerCameraTarget.transform.position;
					Vector3 vector2 = vector - position;
					Vector3 vector3 = navigationRig.position + vector2;
					Vector3 up = navigationRig.up;
					float num = Vector3.Dot(vector3 - navigationRig.position, up);
					vector3 += up * (0f - num);
					navigationRig.position = vector3;
				}
				if (LookInputModule.singleton != null)
				{
					LookInputModule.singleton.worldScale = _worldScale;
				}
				ScaleChangeReceiver[] componentsInChildren = GetComponentsInChildren<ScaleChangeReceiver>(includeInactive: true);
				ScaleChangeReceiver[] array = componentsInChildren;
				foreach (ScaleChangeReceiver scaleChangeReceiver in array)
				{
					scaleChangeReceiver.ScaleChanged(_worldScale);
				}
				SyncPlayerHeightAdjust();
			}
		}
	}

	public float controllerScale
	{
		get
		{
			return _controllerScale;
		}
		set
		{
			if (_controllerScale != value)
			{
				_controllerScale = value;
				if (controllerScaleSlider != null)
				{
					controllerScaleSlider.value = _controllerScale;
				}
			}
		}
	}

	private Transform motionControllerLeft
	{
		get
		{
			if (isOVR)
			{
				return touchObjectLeft;
			}
			if (isOpenVR)
			{
				return viveObjectLeft;
			}
			return null;
		}
	}

	private Transform handMountLeft
	{
		get
		{
			if (isOVR)
			{
				return touchHandMountLeft;
			}
			if (isOpenVR)
			{
				return viveHandMountLeft;
			}
			return null;
		}
	}

	private Transform centerHandLeft
	{
		get
		{
			if (isOVR)
			{
				return touchCenterHandLeft;
			}
			if (isOpenVR)
			{
				return viveCenterHandLeft;
			}
			return null;
		}
	}

	private Transform motionControllerRight
	{
		get
		{
			if (isOVR)
			{
				return touchObjectRight;
			}
			if (isOpenVR)
			{
				return viveObjectRight;
			}
			return null;
		}
	}

	private Transform handMountRight
	{
		get
		{
			if (isOVR)
			{
				return touchHandMountRight;
			}
			if (isOpenVR)
			{
				return viveHandMountRight;
			}
			return null;
		}
	}

	private Transform centerHandRight
	{
		get
		{
			if (isOVR)
			{
				return touchCenterHandRight;
			}
			if (isOpenVR)
			{
				return viveCenterHandRight;
			}
			return null;
		}
	}

	private Transform motionControllerHead
	{
		get
		{
			if (centerCameraTarget != null)
			{
				return centerCameraTarget.transform;
			}
			return null;
		}
	}

	public bool allowGrabPlusTriggerHandToggle
	{
		get
		{
			return _allowGrabPlusTriggerHandToggle;
		}
		set
		{
			if (_allowGrabPlusTriggerHandToggle != value)
			{
				_allowGrabPlusTriggerHandToggle = value;
				if (allowGrabPlusTriggerHandToggleToggle != null)
				{
					allowGrabPlusTriggerHandToggleToggle.isOn = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public UISideAlign.Side UISide
	{
		get
		{
			return _UISide;
		}
		set
		{
			if (_UISide != value)
			{
				_UISide = value;
				if (UISidePopup != null)
				{
					UISidePopup.currentValueNoCallback = _UISide.ToString();
				}
				SyncUISide();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool allowPossessSpringAdjustment
	{
		get
		{
			return _allowPossessSpringAdjustment;
		}
		set
		{
			if (_allowPossessSpringAdjustment != value)
			{
				_allowPossessSpringAdjustment = value;
				if (allowPossessSpringAdjustmentToggle != null)
				{
					allowPossessSpringAdjustmentToggle.isOn = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float possessPositionSpring
	{
		get
		{
			return _possessPositionSpring;
		}
		set
		{
			if (_possessPositionSpring != value)
			{
				_possessPositionSpring = value;
				if (possessPositionSpringSlider != null)
				{
					possessPositionSpringSlider.value = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float possessRotationSpring
	{
		get
		{
			return _possessRotationSpring;
		}
		set
		{
			if (_possessRotationSpring != value)
			{
				_possessRotationSpring = value;
				if (possessRotationSpringSlider != null)
				{
					possessRotationSpringSlider.value = value;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool assetManagerReady => _assetManagerReady;

	public bool showHiddenAtoms
	{
		get
		{
			return _showHiddenAtoms;
		}
		set
		{
			if (_showHiddenAtoms != value)
			{
				_showHiddenAtoms = value;
				if (showHiddenAtomsToggle != null)
				{
					showHiddenAtomsToggle.isOn = value;
				}
				if (showHiddenAtomsToggleAlt != null)
				{
					showHiddenAtomsToggleAlt.isOn = value;
				}
				SyncSelectAtomPopup();
				SyncVisibility();
			}
		}
	}

	public string[] forceReceiverNames => _forceReceiverNames;

	public string[] forceProducerNames => _forceProducerNames;

	public string[] rhythmControllerNames => _rhythmControllerNames;

	public string[] freeControllerNames => _freeControllerNames;

	public string[] rigidbodyNames => _rigidbodyNames;

	public bool showNavigationHologrid
	{
		get
		{
			return _showNavigationHologrid;
		}
		set
		{
			if (_showNavigationHologrid != value)
			{
				_showNavigationHologrid = value;
				if (showNavigationHologridToggle != null)
				{
					showNavigationHologridToggle.isOn = _showNavigationHologrid;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float hologridTransparency
	{
		get
		{
			return _hologridTransparency;
		}
		set
		{
			if (_hologridTransparency != value)
			{
				_hologridTransparency = value;
				SyncHologridTransparency();
				if (hologridTransparencySlider != null)
				{
					hologridTransparencySlider.value = _hologridTransparency;
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool useSceneLoadPosition
	{
		get
		{
			return _useSceneLoadPosition;
		}
		set
		{
			if (_useSceneLoadPosition != value)
			{
				_useSceneLoadPosition = value;
				if (useSceneLoadPositionToggle != null)
				{
					useSceneLoadPositionToggle.isOn = _useSceneLoadPosition;
				}
			}
		}
	}

	public bool lockHeightDuringNavigate
	{
		get
		{
			return _lockHeightDuringNavigate;
		}
		set
		{
			if (_lockHeightDuringNavigate != value)
			{
				_lockHeightDuringNavigate = value;
				SyncLockHeightDuringNavigate();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool disableAllNavigation
	{
		get
		{
			return _disableAllNavigation;
		}
		set
		{
			if (_disableAllNavigation != value)
			{
				_disableAllNavigation = value;
				SyncDisableAllNavigation();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool freeMoveFollowFloor
	{
		get
		{
			return _freeMoveFollowFloor;
		}
		set
		{
			if (_freeMoveFollowFloor != value)
			{
				_freeMoveFollowFloor = value;
				SyncFreeMoveFollowFloor();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool teleportAllowRotation
	{
		get
		{
			return _teleportAllowRotation;
		}
		set
		{
			if (_teleportAllowRotation != value)
			{
				_teleportAllowRotation = value;
				SyncTeleportAllowRotation();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool disableTeleport
	{
		get
		{
			return _disableTeleport;
		}
		set
		{
			if (_disableTeleport != value)
			{
				_disableTeleport = value;
				SyncDisableTeleport();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool disableTeleportDuringPossess
	{
		get
		{
			return _disableTeleportDuringPossess;
		}
		set
		{
			if (_disableTeleportDuringPossess != value)
			{
				_disableTeleportDuringPossess = value;
				SyncDisableTeleportDuringPossess();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float freeMoveMultiplier
	{
		get
		{
			return _freeMoveMultiplier;
		}
		set
		{
			if (_freeMoveMultiplier != value)
			{
				_freeMoveMultiplier = value;
				SyncFreeMoveMultiplier();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public bool disableGrabNavigation
	{
		get
		{
			return _disableGrabNavigation;
		}
		set
		{
			if (_disableGrabNavigation != value)
			{
				_disableGrabNavigation = value;
				SyncDisableGrabNavigation();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float grabNavigationPositionMultiplier
	{
		get
		{
			return _grabNavigationPositionMultiplier;
		}
		set
		{
			if (_grabNavigationPositionMultiplier != value)
			{
				_grabNavigationPositionMultiplier = value;
				SyncGrabNavigationPositionMultiplier();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float grabNavigationRotationMultiplier
	{
		get
		{
			return _grabNavigationRotationMultiplier;
		}
		set
		{
			if (_grabNavigationRotationMultiplier != value)
			{
				_grabNavigationRotationMultiplier = value;
				SyncGrabNavigationRotationMultiplier();
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public float playerHeightAdjust
	{
		get
		{
			return _playerHeightAdjust;
		}
		set
		{
			if (_playerHeightAdjust != value)
			{
				float adj = value - _playerHeightAdjust;
				HUDAnchor.AdjustAnchorHeights(adj);
				_playerHeightAdjust = value;
				SyncPlayerHeightAdjust();
				if (playerHeightAdjustSlider != null)
				{
					playerHeightAdjustSlider.value = _playerHeightAdjust;
				}
				if (playerHeightAdjustSliderAlt != null)
				{
					playerHeightAdjustSliderAlt.value = _playerHeightAdjust;
				}
			}
		}
	}

	public int solverIterations
	{
		get
		{
			return _solverIterations;
		}
		set
		{
			if (_solverIterations == value)
			{
				return;
			}
			_solverIterations = value;
			foreach (Atom value2 in atoms.Values)
			{
				Rigidbody[] rigidbodies = value2.rigidbodies;
				foreach (Rigidbody rigidbody in rigidbodies)
				{
					rigidbody.solverIterations = _solverIterations;
				}
				PhysicsSimulator[] physicsSimulators = value2.physicsSimulators;
				foreach (PhysicsSimulator physicsSimulator in physicsSimulators)
				{
					physicsSimulator.solverIterations = _solverIterations;
				}
				PhysicsSimulatorJSONStorable[] physicsSimulatorsStorable = value2.physicsSimulatorsStorable;
				foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in physicsSimulatorsStorable)
				{
					physicsSimulatorJSONStorable.solverIterations = _solverIterations;
				}
			}
		}
	}

	private bool useInterpolation
	{
		get
		{
			return _useInterpolation;
		}
		set
		{
			if (_useInterpolation == value)
			{
				return;
			}
			_useInterpolation = value;
			foreach (Atom value2 in atoms.Values)
			{
				value2.useRigidbodyInterpolation = _useInterpolation;
			}
		}
	}

	protected bool leapDisabled
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.disableLeap;
			}
			return disableLeap;
		}
	}

	public bool hideTrackers
	{
		get
		{
			return _hideTrackers;
		}
		set
		{
			if (_hideTrackers != value)
			{
				_hideTrackers = value;
				SyncTrackerVisibility();
			}
		}
	}

	protected bool tracker1Visible
	{
		get
		{
			return _tracker1Visible;
		}
		set
		{
			if (_tracker1Visible != value)
			{
				_tracker1Visible = value;
				SyncTracker1Visibility();
			}
		}
	}

	protected bool tracker2Visible
	{
		get
		{
			return _tracker2Visible;
		}
		set
		{
			if (_tracker2Visible != value)
			{
				_tracker2Visible = value;
				SyncTracker2Visibility();
			}
		}
	}

	protected bool tracker3Visible
	{
		get
		{
			return _tracker3Visible;
		}
		set
		{
			if (_tracker3Visible != value)
			{
				_tracker3Visible = value;
				SyncTracker3Visibility();
			}
		}
	}

	protected bool tracker4Visible
	{
		get
		{
			return _tracker4Visible;
		}
		set
		{
			if (_tracker4Visible != value)
			{
				_tracker4Visible = value;
				SyncTracker4Visibility();
			}
		}
	}

	protected bool tracker5Visible
	{
		get
		{
			return _tracker5Visible;
		}
		set
		{
			if (_tracker5Visible != value)
			{
				_tracker5Visible = value;
				SyncTracker5Visibility();
			}
		}
	}

	protected bool tracker6Visible
	{
		get
		{
			return _tracker6Visible;
		}
		set
		{
			if (_tracker6Visible != value)
			{
				_tracker6Visible = value;
				SyncTracker6Visibility();
			}
		}
	}

	protected bool tracker7Visible
	{
		get
		{
			return _tracker7Visible;
		}
		set
		{
			if (_tracker7Visible != value)
			{
				_tracker7Visible = value;
				SyncTracker7Visibility();
			}
		}
	}

	protected bool tracker8Visible
	{
		get
		{
			return _tracker8Visible;
		}
		set
		{
			if (_tracker8Visible != value)
			{
				_tracker8Visible = value;
				SyncTracker8Visibility();
			}
		}
	}

	public bool IsMonitorOnly => isMonitorOnly;

	public float monitorUIScale
	{
		get
		{
			return _monitorUIScale;
		}
		set
		{
			if (_monitorUIScale != value)
			{
				_monitorUIScale = value;
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
				if (monitorUIScaleSlider != null)
				{
					monitorUIScaleSlider.value = _monitorUIScale;
				}
			}
		}
	}

	public float monitorUIYOffset
	{
		get
		{
			return _monitorUIYOffset;
		}
		set
		{
			if (_monitorUIYOffset != value)
			{
				_monitorUIYOffset = value;
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
				if (monitorUIYOffsetSlider != null)
				{
					monitorUIYOffsetSlider.value = _monitorUIYOffset;
				}
			}
		}
	}

	public float startingMonitorCameraFOV
	{
		get
		{
			if (GlobalSceneOptions.singleton != null)
			{
				return GlobalSceneOptions.singleton.startingMonitorCameraFOV;
			}
			return _monitorCameraFOV;
		}
	}

	public float monitorCameraFOV
	{
		get
		{
			return _monitorCameraFOV;
		}
		set
		{
			if (_monitorCameraFOV != value)
			{
				_monitorCameraFOV = value;
				if (monitorCameraFOVSlider != null)
				{
					monitorCameraFOVSlider.value = _monitorCameraFOV;
				}
				SyncMonitorCameraFOV();
			}
		}
	}

	public ThumbstickFunction oculusThumbstickFunction
	{
		get
		{
			return _oculusThumbstickFunction;
		}
		set
		{
			if (_oculusThumbstickFunction != value)
			{
				_oculusThumbstickFunction = value;
				if (oculusThumbstickFunctionPopup != null)
				{
					oculusThumbstickFunctionPopup.currentValue = _oculusThumbstickFunction.ToString();
				}
				if (UserPreferences.singleton != null)
				{
					UserPreferences.singleton.SavePreferences();
				}
			}
		}
	}

	public void GetScenePathDialog(FileBrowserCallback callback)
	{
		LoadDialog(lastScenePathDir);
		fileBrowserUI.SetTitle("Select File");
		fileBrowserUI.Show(callback);
	}

	public void GetMediaPathDialog(FileBrowserCallback callback, string filter = "", string suggestedFolder = null, bool fullComputerBrowse = true, bool showDirs = true, bool showKeepOpt = false, string fileRemovePrefix = null, bool hideExtenstion = false, List<ShortCut> shortCuts = null, bool browseVarFilesAsDirectories = true, bool showInstallFolderInDirectoryList = false)
	{
		if (!browseDisabled)
		{
			string text = savesDirResolved + "scene";
			if (suggestedFolder != null && suggestedFolder != string.Empty)
			{
				if (FileManager.DirectoryExists(suggestedFolder))
				{
					text = suggestedFolder;
				}
				else
				{
					text = ".";
					fullComputerBrowse = true;
				}
			}
			else if (lastMediaDir != string.Empty && FileManager.DirectoryExists(lastMediaDir))
			{
				text = lastMediaDir;
			}
			List<ShortCut> list = shortCuts;
			if (fullComputerBrowse)
			{
				VarDirectoryEntry varDirectoryEntry = FileManager.GetVarDirectoryEntry(text);
				text = ((varDirectoryEntry == null) ? Path.GetFullPath(text) : varDirectoryEntry.Path);
				if (list == null)
				{
					list = new List<ShortCut>();
					ShortCut shortCut = new ShortCut();
					shortCut.package = string.Empty;
					shortCut.displayName = "Default";
					shortCut.path = text;
					list.Add(shortCut);
					ShortCut shortCut2 = new ShortCut();
					shortCut2.package = string.Empty;
					shortCut2.displayName = "Addon Packages";
					shortCut2.path = "AddonPackages";
					list.Add(shortCut2);
				}
			}
			mediaFileBrowserUI.fileRemovePrefix = fileRemovePrefix;
			mediaFileBrowserUI.hideExtension = hideExtenstion;
			mediaFileBrowserUI.keepOpen = false;
			mediaFileBrowserUI.fileFormat = filter;
			mediaFileBrowserUI.defaultPath = text;
			mediaFileBrowserUI.showDirs = showDirs;
			mediaFileBrowserUI.shortCuts = list;
			mediaFileBrowserUI.browseVarFilesAsDirectories = browseVarFilesAsDirectories;
			mediaFileBrowserUI.showInstallFolderInDirectoryList = showInstallFolderInDirectoryList;
			mediaFileBrowserUI.SetTextEntry(b: false);
			mediaFileBrowserUI.Show(callback);
		}
		else
		{
			LogMessage("Please back this project on Patreon at https://www.patreon.com/meshedvr to unlock this feature!");
		}
	}

	protected void TestDirectoryCallback(string dir)
	{
		UnityEngine.Debug.Log("Selected dir " + dir);
	}

	public void TestDirectoryPathBrowse()
	{
		GetDirectoryPathDialog(TestDirectoryCallback);
	}

	public void GetDirectoryPathDialog(FileBrowserCallback callback, string suggestedFolder = null, List<ShortCut> shortCuts = null, bool fullComputerBrowse = true)
	{
		if (!browseDisabled)
		{
			string text = savesDirResolved + "scene";
			if (suggestedFolder != null && suggestedFolder != string.Empty && FileManager.DirectoryExists(suggestedFolder))
			{
				text = suggestedFolder;
			}
			else if (lastBrowseDir != string.Empty && FileManager.DirectoryExists(lastBrowseDir))
			{
				text = lastBrowseDir;
			}
			if (fullComputerBrowse)
			{
				text = Path.GetFullPath(text);
			}
			directoryBrowserUI.fileFormat = string.Empty;
			directoryBrowserUI.defaultPath = text;
			directoryBrowserUI.shortCuts = shortCuts;
			directoryBrowserUI.SetTextEntry(b: true);
			directoryBrowserUI.Show(callback);
		}
		else
		{
			LogMessage("Please back this project on Patreon at https://www.patreon.com/meshedvr to unlock this feature!");
		}
	}

	protected void SetSavesDirFromCommandline()
	{
		if (Application.isEditor)
		{
			return;
		}
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-savesdir" && i + 1 < commandLineArgs.Length)
			{
				savesDir = commandLineArgs[i + 1];
				if (savesDir[savesDir.Length - 1] != '\\')
				{
					savesDir += "\\";
				}
			}
		}
	}

	public void StartScene()
	{
		if (embeddedJSONScene != null)
		{
			LoadFromJSONEmbed(embeddedJSONScene);
			return;
		}
		if (File.Exists(savesDirResolved + startSceneName))
		{
			Load(savesDirResolved + startSceneName);
		}
		else
		{
			Load(savesDirResolved + startSceneAltName);
		}
		loadedName = string.Empty;
	}

	public void NewScene()
	{
		if (newJSONEmbedScene != null)
		{
			LoadFromJSONEmbed(newJSONEmbedScene, loadMerge: false, editMode: true);
			return;
		}
		if (File.Exists(savesDirResolved + newSceneName))
		{
			LoadForEdit(savesDirResolved + newSceneName);
		}
		else
		{
			LoadForEdit(savesDirResolved + newSceneAltName);
		}
		loadedName = string.Empty;
	}

	public void ClearScene()
	{
		if (Application.isPlaying)
		{
			if (!_isLoading)
			{
				onStartScene = false;
				gameMode = GameMode.Edit;
				loadedName = string.Empty;
				_isLoading = true;
				StartCoroutine(LoadCo(clearOnly: true));
			}
			else
			{
				UnityEngine.Debug.LogWarning("Already loading file " + loadedName + ". Can't clear until complete");
			}
		}
	}

	public void SaveSceneDialog()
	{
		SaveSceneDialog(Save);
	}

	public void SaveSceneLegacyPackageDialog()
	{
		SaveSceneDialog(SavePackage);
	}

	public void SaveSceneNewAddonPackageDialog()
	{
		SaveSceneDialog(SaveAndAddToNewPackage);
	}

	public void SaveSceneCurrentAddonPackageDialog()
	{
		SaveSceneDialog(SaveAndAddToCurrentPackage);
	}

	public void OpenPackageBuilder()
	{
		activeUI = ActiveUI.PackageBuilder;
	}

	public void OpenPackageManager()
	{
		activeUI = ActiveUI.PackageManager;
	}

	public void OpenPackageInManager(string packageUid)
	{
		packageUid = Regex.Replace(packageUid, ":.*", string.Empty);
		OpenPackageManager();
		if (packageManager != null)
		{
			packageManager.LoadMetaFromPackageUid(packageUid);
		}
	}

	public void RescanPackages()
	{
		FileManager.Refresh();
	}

	public void OpenLinkInBrowser(string url)
	{
		if (onlineBrowser != null)
		{
			activeUI = ActiveUI.OnlineBrowser;
			onlineBrowser.url = url;
		}
	}

	public void SaveSceneDialog(FileBrowserCallback callback)
	{
		try
		{
			string text = savesDirResolved + "scene";
			fileBrowserUI.shortCuts = null;
			string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastLoadDir, allowPackagePath: false);
			if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
			{
				text = suggestedBrowserDirectoryFromDirectoryPath;
			}
			if (!FileManager.DirectoryExists(text, onlySystemDirectories: true))
			{
				FileManager.CreateDirectory(text);
			}
			fileBrowserUI.defaultPath = text;
			activeUI = ActiveUI.None;
			fileBrowserUI.SetTitle("Select Save File");
			fileBrowserUI.SetTextEntry(b: true);
			fileBrowserUI.Show(callback);
			if (fileBrowserUI.fileEntryField != null)
			{
				string text2 = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds/*cast due to .constrained prefix*/).ToString();
				fileBrowserUI.fileEntryField.text = text2;
				fileBrowserUI.ActivateFileNameField();
			}
		}
		catch (Exception ex)
		{
			LogError("Exception during open of save scene dialog: " + ex.Message);
		}
	}

	public void SaveConfirm(string option)
	{
		if (_lastActiveUI == ActiveUI.MultiButtonPanel)
		{
			activeUI = ActiveUI.None;
		}
		else
		{
			activeUI = _lastActiveUI;
		}
		multiButtonPanel.gameObject.SetActive(value: false);
		multiButtonPanel.buttonCallback = null;
		if (option == "Save New")
		{
			int num = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			loadedName = savesDirResolved + "scene\\" + num + ".json";
			Save(loadedName);
		}
		else if (loadedName != null && loadedName != string.Empty && option == "Overwrite Current")
		{
			Save(loadedName);
		}
	}

	public void SaveAddDependency(Atom saveAtom)
	{
		if (_saveQueue != null)
		{
			_saveQueue.Add(saveAtom);
		}
	}

	public string NormalizeSavePath(string path)
	{
		string result = path;
		if (path != null && path != string.Empty && path != "/" && path != "NULL")
		{
			result = FileManager.NormalizeSavePath(path);
			if (packageMode)
			{
				string fileName = Path.GetFileName(path);
				result = AddFileToPackage(path, fileName);
			}
		}
		return result;
	}

	public void Save(string saveName)
	{
		if (saveName != string.Empty)
		{
			Save(saveName, null, includePhysical: true, includeAppearance: true, null);
		}
	}

	public void SaveAndAddToCurrentPackage(string saveName)
	{
		if (saveName != string.Empty)
		{
			Save(saveName, null, includePhysical: true, includeAppearance: true, delegate
			{
				packageBuilder.AddContentItem(saveName + ".json");
				OpenPackageBuilder();
			});
		}
	}

	public void SaveAndAddToNewPackage(string saveName)
	{
		if (saveName != string.Empty)
		{
			Save(saveName, null, includePhysical: true, includeAppearance: true, delegate
			{
				packageBuilder.ClearAll();
				packageBuilder.PackageName = Path.GetFileName(saveName);
				packageBuilder.AddContentItem(saveName + ".json");
				OpenPackageBuilder();
			});
		}
	}

	public JSONClass GetSaveJSON(Atom specificAtom = null, bool includePhysical = true, bool includeAppearance = true)
	{
		JSONClass jSONClass = new JSONClass();
		if (specificAtom == null)
		{
			if (headPossessedController != null)
			{
				jSONClass["headPossessedController"] = headPossessedController.containingAtom.uid + ":" + headPossessedController.name;
			}
			if (playerNavCollider != null)
			{
				jSONClass["playerNavCollider"] = playerNavCollider.containingAtom.uid + ":" + playerNavCollider.name;
			}
			if (worldScaleSlider != null)
			{
				SliderControl component = worldScaleSlider.GetComponent<SliderControl>();
				if (component == null || component.defaultValue != worldScale)
				{
					jSONClass["worldScale"].AsFloat = worldScale;
				}
			}
			if (playerHeightAdjustSlider != null)
			{
				SliderControl component2 = playerHeightAdjustSlider.GetComponent<SliderControl>();
				if (component2 == null || component2.defaultValue != _playerHeightAdjust)
				{
					jSONClass["playerHeightAdjust"].AsFloat = _playerHeightAdjust;
				}
			}
			if (MonitorCenterCamera != null)
			{
				Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
				jSONClass["monitorCameraRotation"]["x"].AsFloat = localEulerAngles.x;
				jSONClass["monitorCameraRotation"]["y"].AsFloat = localEulerAngles.y;
				jSONClass["monitorCameraRotation"]["z"].AsFloat = localEulerAngles.z;
			}
			if (useSceneLoadPositionToggle != null)
			{
				jSONClass["useSceneLoadPosition"].AsBool = _useSceneLoadPosition;
			}
			if (useSceneLoadPosition)
			{
				MoveToSceneLoadPosition();
			}
			JSONArray jSONArray = (JSONArray)(jSONClass["atoms"] = new JSONArray());
			foreach (Atom value in atoms.Values)
			{
				value.Store(jSONArray);
			}
		}
		else
		{
			_saveQueue = new List<Atom>();
			JSONArray jSONArray2 = (JSONArray)(jSONClass["atoms"] = new JSONArray());
			specificAtom.Store(jSONArray2, includePhysical, includeAppearance);
			if (includePhysical)
			{
				foreach (Atom item in _saveQueue)
				{
					item.Store(jSONArray2, includePhysical, includeAppearance);
				}
			}
		}
		return jSONClass;
	}

	public void Save(string saveName = "Saves\\scene\\savefile.json", Atom specificAtom = null, bool includePhysical = true, bool includeAppearance = true, ScreenShotCallback callback = null)
	{
		try
		{
			if (!saveName.EndsWith(".json"))
			{
				saveName += ".json";
			}
			UnityEngine.Debug.Log("Save " + saveName);
			packageMode = false;
			loadedName = saveName;
			int num = saveName.LastIndexOf('\\');
			if (num >= 0)
			{
				string path = saveName.Substring(0, num);
				FileManager.CreateDirectory(path);
			}
			lastLoadDir = FileManager.GetDirectoryName(saveName, returnSlashPath: true);
			FileManager.SetSaveDirFromFilePath(saveName);
			FileManager.SetLoadDirFromFilePath(saveName);
			JSONClass saveJSON = GetSaveJSON(specificAtom, includePhysical, includeAppearance);
			SaveJSON(saveJSON, saveName);
			DoSaveScreenshot(saveName, callback);
		}
		catch (Exception ex)
		{
			LogError("Exception during Save: " + ex.Message);
		}
	}

	public void SaveJSON(JSONClass jc, string saveName)
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder(100000);
			jc.ToString(string.Empty, stringBuilder);
			string value = stringBuilder.ToString();
			using StreamWriter streamWriter = FileManager.OpenStreamWriter(saveName);
			streamWriter.Write(value);
		}
		catch (Exception ex)
		{
			LogError("Exception during SaveJSON: " + ex.Message);
		}
	}

	public JSONNode LoadJSON(string saveName)
	{
		JSONNode result = null;
		try
		{
			FileEntry fileEntry = FileManager.GetFileEntry(saveName, restrictPath: true);
			if (fileEntry != null)
			{
				using FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(fileEntry);
				string aJSON = fileEntryStreamReader.ReadToEnd();
				result = JSON.Parse(aJSON);
			}
			else
			{
				LogError("LoadJSON: File " + saveName + " not found");
			}
		}
		catch (Exception ex)
		{
			LogError("Exception during LoadJSON: " + ex.Message);
		}
		return result;
	}

	public void AddVarPackageRefToVacPackage(string packageUid)
	{
		referencedVarPackages.Add(packageUid);
	}

	public string AddFileToPackage(string path, string packagepath)
	{
		if (zos != null)
		{
			VarFileEntry varFileEntry = FileManager.GetVarFileEntry(path);
			if (varFileEntry == null)
			{
				if (!alreadyPackaged.ContainsKey(packagepath))
				{
					alreadyPackaged.Add(packagepath, value: true);
					byte[] buffer = new byte[4096];
					ZipEntry entry = new ZipEntry(packagepath);
					zos.PutNextEntry(entry);
					using (FileEntryStream fileEntryStream = FileManager.OpenStream(path))
					{
						StreamUtils.Copy(fileEntryStream.Stream, zos, buffer);
					}
					zos.CloseEntry();
				}
				return packagepath;
			}
			referencedVarPackages.Add(varFileEntry.Package.Uid);
		}
		return path;
	}

	public void SavePackage(string saveName = "Saves\\scene\\savefile.vac")
	{
		try
		{
			if (!(saveName != string.Empty))
			{
				return;
			}
			alreadyPackaged = new Dictionary<string, bool>();
			referencedVarPackages = new HashSet<string>();
			string fileName = Path.GetFileName(saveName);
			fileName = fileName.Replace(".vac", string.Empty);
			if (!saveName.EndsWith(".vac"))
			{
				saveName += ".vac";
			}
			packageMode = true;
			UnityEngine.Debug.Log("Save Package " + saveName);
			byte[] buffer = new byte[4096];
			using (zos = new ZipOutputStream(File.Create(saveName)))
			{
				zos.SetLevel(5);
				JSONClass saveJSON = GetSaveJSON();
				ZipEntry entry = new ZipEntry(fileName + ".json");
				zos.PutNextEntry(entry);
				StringBuilder stringBuilder = new StringBuilder(100000);
				saveJSON.ToString(string.Empty, stringBuilder);
				string s = stringBuilder.ToString();
				using (MemoryStream source = new MemoryStream(Encoding.Default.GetBytes(s)))
				{
					StreamUtils.Copy(source, zos, buffer);
				}
				zos.CloseEntry();
				ZipEntry entry2 = new ZipEntry("meta.json");
				zos.PutNextEntry(entry2);
				JSONClass jSONClass = new JSONClass();
				JSONClass jSONClass2 = new JSONClass();
				HashSet<string> visited = new HashSet<string>();
				HashSet<VarPackage> allReferencedPackages = new HashSet<VarPackage>();
				HashSet<string> allReferencedPackageUids = new HashSet<string>();
				foreach (string referencedVarPackage in referencedVarPackages)
				{
					VarPackage package = FileManager.GetPackage(referencedVarPackage);
					PackageBuilder.GetPackageDependenciesRecursive(package, referencedVarPackage, visited, allReferencedPackages, allReferencedPackageUids, jSONClass2);
					LogMessage("INFO: VAC references VAR package " + referencedVarPackage);
				}
				jSONClass["programVersion"] = GetVersion();
				jSONClass["dependencies"] = jSONClass2;
				s = jSONClass.ToString(string.Empty);
				using (MemoryStream source2 = new MemoryStream(Encoding.Default.GetBytes(s)))
				{
					StreamUtils.Copy(source2, zos, buffer);
				}
				zos.CloseEntry();
			}
			packageMode = false;
			DoSaveScreenshot(saveName);
		}
		catch (Exception ex)
		{
			LogError("Exception during SavePackage: " + ex.Message);
		}
	}

	protected IEnumerator LoadCo(bool clearOnly = false, bool loadMerge = false)
	{
		hideWaitTransform = loadMerge;
		if (loadFlag != null)
		{
			loadFlag.Raise();
		}
		loadFlag = new AsyncFlag("Scene Load");
		PauseSimulation(loadFlag);
		if (UserPreferences.singleton != null)
		{
			UserPreferences.singleton.pauseGlow = true;
		}
		if (loadingUI != null)
		{
			if (!loadMerge)
			{
				if (fileBrowserUI == null || fileBrowserUI.IsHidden() || !fileBrowserUI.keepOpen)
				{
					HideMainHUD();
				}
				HUDAnchor.SetAnchorsToReference();
				loadingUI.gameObject.SetActive(value: true);
				if (loadingUIAlt != null && !_mainHUDAnchoredOnMonitor)
				{
					loadingUIAlt.gameObject.SetActive(value: true);
				}
			}
			if (loadingGeometry != null)
			{
				loadingGeometry.gameObject.SetActive(value: true);
			}
		}
		yield return null;
		ResetMonitorCenterCamera();
		if (!loadMerge)
		{
			JSONClass jc = new JSONClass();
			ClearSelection();
			ClearPossess();
			DisconnectNavRigFromPlayerNavCollider();
			if (worldScaleSlider != null)
			{
				SliderControl component = worldScaleSlider.GetComponent<SliderControl>();
				if (component != null)
				{
					worldScale = component.defaultValue;
				}
			}
			if (playerHeightAdjustSlider != null)
			{
				SliderControl component2 = playerHeightAdjustSlider.GetComponent<SliderControl>();
				if (component2 != null)
				{
					playerHeightAdjust = component2.defaultValue;
				}
			}
			Atom[] atms = new Atom[atoms.Count];
			atoms.Values.CopyTo(atms, 0);
			for (int i = 0; i < atms.Length; i++)
			{
				Atom atom = atms[0];
				if (atom != null)
				{
					atom.PreRestore();
				}
			}
			if (startingAtoms != null)
			{
				List<Atom> list = new List<Atom>();
				foreach (Atom value2 in atoms.Values)
				{
					if (!startingAtoms.ContainsKey(value2.uid))
					{
						list.Add(value2);
					}
					else
					{
						value2.SetOn(b: true);
					}
				}
				foreach (Atom item in list)
				{
					if (item != null)
					{
						RemoveAtom(item);
					}
				}
			}
			yield return null;
			foreach (Atom value3 in atoms.Values)
			{
				value3.ClearParentAtom();
			}
			foreach (Atom value4 in atoms.Values)
			{
				value4.RestoreTransform(jc);
			}
			foreach (Atom value5 in atoms.Values)
			{
				value5.RestoreParentAtom(jc);
			}
			FileManager.PushLoadDir(string.Empty);
			foreach (Atom value6 in atoms.Values)
			{
				value6.Restore(jc, restorePhysical: true, restoreAppearance: true, restoreCore: true, null, isClear: true);
			}
			FileManager.PopLoadDir();
			foreach (Atom value7 in atoms.Values)
			{
				value7.LateRestore(jc);
			}
			foreach (Atom value8 in atoms.Values)
			{
				value8.PostRestore();
			}
			yield return Resources.UnloadUnusedAssets();
			GC.Collect();
		}
		if (!clearOnly)
		{
			JSONArray jatoms = loadJson["atoms"].AsArray;
			if (loadJson["worldScale"] != null)
			{
				worldScale = loadJson["worldScale"].AsFloat;
			}
			else if (worldScaleSlider != null)
			{
				SliderControl component3 = worldScaleSlider.GetComponent<SliderControl>();
				if (component3 != null)
				{
					worldScale = component3.defaultValue;
				}
			}
			if (loadJson["environmentHeight"] != null)
			{
				playerHeightAdjust = loadJson["environmentHeight"].AsFloat;
			}
			else if (loadJson["playerHeightAdjust"] != null)
			{
				playerHeightAdjust = loadJson["playerHeightAdjust"].AsFloat;
			}
			else if (playerHeightAdjustSlider != null)
			{
				SliderControl component4 = playerHeightAdjustSlider.GetComponent<SliderControl>();
				if (component4 != null)
				{
					playerHeightAdjust = component4.defaultValue;
				}
			}
			if (loadJson["monitorCameraRotation"] != null)
			{
				Vector3 localEulerAngles = default(Vector3);
				localEulerAngles.x = 0f;
				localEulerAngles.y = 0f;
				localEulerAngles.z = 0f;
				if (loadJson["monitorCameraRotation"]["x"] != null)
				{
					localEulerAngles.x = loadJson["monitorCameraRotation"]["x"].AsFloat;
				}
				if (loadJson["monitorCameraRotation"]["y"] != null)
				{
					localEulerAngles.y = loadJson["monitorCameraRotation"]["y"].AsFloat;
				}
				if (loadJson["monitorCameraRotation"]["z"] != null)
				{
					localEulerAngles.z = loadJson["monitorCameraRotation"]["z"].AsFloat;
				}
				if (MonitorCenterCamera != null)
				{
					MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
				}
			}
			if (loadJson["useSceneLoadPosition"] != null)
			{
				useSceneLoadPosition = loadJson["useSceneLoadPosition"].AsBool;
			}
			if (loadingProgressSlider != null)
			{
				loadingProgressSlider.minValue = 0f;
				loadingProgressSlider.maxValue = (float)jatoms.Count * 2f + 2f;
				loadingProgressSlider.value = 0f;
			}
			if (loadingProgressSliderAlt != null)
			{
				loadingProgressSliderAlt.minValue = 0f;
				loadingProgressSliderAlt.maxValue = (float)jatoms.Count * 2f + 2f;
				loadingProgressSliderAlt.value = 0f;
			}
			UpdateLoadingStatus("Pre-Restore");
			IEnumerator enumerator9 = jatoms.GetEnumerator();
			try
			{
				while (enumerator9.MoveNext())
				{
					JSONClass jSONClass = (JSONClass)enumerator9.Current;
					string uid = jSONClass["id"];
					Atom atomByUid = GetAtomByUid(uid);
					if (atomByUid != null)
					{
						atomByUid.PreRestore();
					}
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable2 = (disposable = enumerator9 as IDisposable);
				if (disposable != null)
				{
					disposable2.Dispose();
				}
			}
			IncrementLoadingSlider();
			Physics.autoSimulation = false;
			IEnumerator enumerator10 = jatoms.GetEnumerator();
			try
			{
				while (enumerator10.MoveNext())
				{
					JSONClass jatom = (JSONClass)enumerator10.Current;
					string auid = jatom["id"];
					string type = jatom["type"];
					UpdateLoadingStatus("Loading Atom " + auid);
					Atom a = GetAtomByUid(auid);
					if (a == null)
					{
						yield return StartCoroutine(AddAtomByType(type, auid));
						a = GetAtomByUid(auid);
						if (a != null)
						{
							a.PauseSimulation(loadFlag);
						}
					}
					else if (a.type != type)
					{
						Error("Atom " + a.name + " already exists, but uses different type " + a.type + " compared to requested " + type);
					}
					if (a != null)
					{
						a.SetOn(b: true);
					}
					IncrementLoadingSlider();
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable3 = (disposable = enumerator10 as IDisposable);
				if (disposable != null)
				{
					disposable3.Dispose();
				}
			}
			UpdateLoadingStatus("Restoring atom contents. Note large save files could take a while...");
			yield return null;
			Physics.Simulate(0.01f);
			yield return null;
			IEnumerator enumerator11 = jatoms.GetEnumerator();
			try
			{
				while (enumerator11.MoveNext())
				{
					JSONClass jSONClass2 = (JSONClass)enumerator11.Current;
					string text = jSONClass2["id"];
					string text2 = jSONClass2["type"];
					Atom atomByUid2 = GetAtomByUid(text);
					if (atomByUid2 != null)
					{
						atomByUid2.RestoreTransform(jSONClass2);
					}
					else
					{
						Error("Failed to find atom " + text + " of type " + text2);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable4 = (disposable = enumerator11 as IDisposable);
				if (disposable != null)
				{
					disposable4.Dispose();
				}
			}
			IEnumerator enumerator12 = jatoms.GetEnumerator();
			try
			{
				while (enumerator12.MoveNext())
				{
					JSONClass jSONClass3 = (JSONClass)enumerator12.Current;
					string uid2 = jSONClass3["id"];
					Atom atomByUid3 = GetAtomByUid(uid2);
					if (atomByUid3 != null)
					{
						atomByUid3.RestoreParentAtom(jSONClass3);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable5 = (disposable = enumerator12 as IDisposable);
				if (disposable != null)
				{
					disposable5.Dispose();
				}
			}
			IEnumerator enumerator13 = jatoms.GetEnumerator();
			try
			{
				while (enumerator13.MoveNext())
				{
					JSONClass jSONClass4 = (JSONClass)enumerator13.Current;
					string text3 = jSONClass4["id"];
					Atom atomByUid4 = GetAtomByUid(text3);
					if (atomByUid4 != null)
					{
						UpdateLoadingStatus("Restoring atom " + text3);
						atomByUid4.Restore(jSONClass4);
					}
					else
					{
						Error("Could not find atom by uid " + text3);
					}
					IncrementLoadingSlider();
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable6 = (disposable = enumerator13 as IDisposable);
				if (disposable != null)
				{
					disposable6.Dispose();
				}
			}
			UpdateLoadingStatus("Post-Restore");
			IEnumerator enumerator14 = jatoms.GetEnumerator();
			try
			{
				while (enumerator14.MoveNext())
				{
					JSONClass jSONClass5 = (JSONClass)enumerator14.Current;
					string text4 = jSONClass5["id"];
					Atom atomByUid5 = GetAtomByUid(text4);
					if (atomByUid5 != null)
					{
						atomByUid5.LateRestore(jSONClass5);
					}
					else
					{
						Error("Could not find atom by uid " + text4);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable7 = (disposable = enumerator14 as IDisposable);
				if (disposable != null)
				{
					disposable7.Dispose();
				}
			}
			foreach (Atom value9 in atoms.Values)
			{
				value9.PostRestore();
			}
			while (CheckHoldLoad())
			{
				UpdateLoadingStatus("Waiting for async load from " + holdLoadCompleteFlags[0].Name);
				yield return null;
			}
			Physics.autoSimulation = true;
			SetSceneLoadPosition();
			IncrementLoadingSlider();
			yield return null;
			if (loadJson["headPossessedController"] != null)
			{
				FreeControllerV3 freeControllerV = FreeControllerNameToFreeController(loadJson["headPossessedController"]);
				if (freeControllerV != null)
				{
					HeadPossess(freeControllerV, alignRig: true);
				}
			}
			if (loadJson["playerNavCollider"] != null)
			{
				string text5 = loadJson["playerNavCollider"];
				if (pncMap.TryGetValue(text5, out var value))
				{
					playerNavCollider = value;
					ConnectNavRigToPlayerNavCollider();
				}
				else
				{
					Error("Could not find playerNavCollider " + text5);
				}
			}
		}
		if (loadMerge)
		{
			foreach (Atom value10 in atoms.Values)
			{
				value10.Validate();
			}
		}
		for (int j = 0; j < 20; j++)
		{
			yield return null;
		}
		loadFlag.Raise();
		for (int k = 0; k < 5; k++)
		{
			yield return null;
		}
		_isLoading = false;
		SyncSortedAtomUIDs();
		SyncSortedAtomUIDsWithForceProducers();
		SyncSortedAtomUIDsWithForceReceivers();
		SyncSortedAtomUIDsWithFreeControllers();
		SyncSortedAtomUIDsWithRhythmControllers();
		SyncSortedAtomUIDsWithRigidbodies();
		SyncHiddenAtoms();
		SyncSelectAtomPopup();
		if (loadingUI != null)
		{
			for (int l = 0; l < 10; l++)
			{
				yield return null;
			}
			loadingUI.gameObject.SetActive(value: false);
			if (loadingUIAlt != null)
			{
				loadingUIAlt.gameObject.SetActive(value: false);
			}
			if (loadingGeometry != null)
			{
				loadingGeometry.gameObject.SetActive(value: false);
			}
		}
		if (UserPreferences.singleton != null)
		{
			UserPreferences.singleton.pauseGlow = false;
		}
		if (UIDisabled && !loadMerge)
		{
			HideMainHUD();
		}
		if (!loadMerge && mainHUDAttachPoint != null)
		{
			mainHUDAttachPoint.localPosition = mainHUDAttachPointStartingPosition;
			mainHUDAttachPoint.localRotation = mainHUDAttachPointStartingRotation;
		}
		SyncVisibility();
		hideWaitTransform = false;
	}

	public string ExtractZipFile(string archiveFilenameIn)
	{
		ZipFile zipFile = null;
		string result = null;
		bool flag = false;
		try
		{
			using (FileEntryStream fileEntryStream = FileManager.OpenStream(archiveFilenameIn, restrictPath: true))
			{
				zipFile = new ZipFile(fileEntryStream.Stream);
				string directoryName = FileManager.GetDirectoryName(archiveFilenameIn);
				string fileName = Path.GetFileName(archiveFilenameIn);
				fileName = fileName.Replace(".zip", string.Empty);
				fileName = fileName.Replace(".vac", string.Empty);
				directoryName = directoryName + "/" + fileName;
				foreach (ZipEntry item in zipFile)
				{
					if (!item.IsFile)
					{
						continue;
					}
					string text = item.Name;
					byte[] buffer = new byte[4096];
					Stream inputStream = zipFile.GetInputStream(item);
					string text2 = Path.Combine(directoryName, text);
					string fileName2 = Path.GetFileName(text);
					if (text.EndsWith(".var"))
					{
						text2 = "AddonPackages/" + fileName2;
						string packageUidOrPath = fileName2.Replace(".var", string.Empty);
						if (File.Exists(text2) || FileManager.GetPackage(packageUidOrPath) != null)
						{
							continue;
						}
						flag = true;
					}
					else
					{
						string directoryName2 = Path.GetDirectoryName(text2);
						if (directoryName2.Length > 0)
						{
							FileManager.CreateDirectory(directoryName2);
						}
						if (fileName2 != "meta.json" && (text2.EndsWith(".vac") || text2.EndsWith(".json")))
						{
							result = text2;
						}
					}
					using FileStream destination = File.Create(text2);
					StreamUtils.Copy(inputStream, destination, buffer);
				}
			}
			if (flag)
			{
				FileManager.Refresh();
			}
		}
		catch (Exception ex)
		{
			Error("Exception during zip file extract of " + archiveFilenameIn + ": " + ex.Message);
		}
		finally
		{
			if (zipFile != null)
			{
				zipFile.IsStreamOwner = true;
				zipFile.Close();
			}
		}
		return result;
	}

	protected void LoadInternal(string saveName = "savefile", bool loadMerge = false, bool editMode = false)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (!_isLoading)
		{
			try
			{
				if (saveName != null && saveName != string.Empty && FileManager.FileExists(saveName))
				{
					if (onStartScene)
					{
						lastLoadDir = FileManager.GetDirectoryName(savesDirResolved + startSceneName, returnSlashPath: true);
					}
					else
					{
						lastLoadDir = FileManager.GetDirectoryName(saveName, returnSlashPath: true);
					}
					onStartScene = false;
					if (editMode)
					{
						gameMode = GameMode.Edit;
					}
					else
					{
						gameMode = GameMode.Play;
					}
					UnityEngine.Debug.Log("Load " + saveName);
					if (!loadMerge)
					{
						RemoveNonStartingAtoms();
					}
					if (saveName.EndsWith(".zip"))
					{
						string text = ExtractZipFile(saveName);
						if (text != null)
						{
							saveName = text;
						}
					}
					if (saveName.EndsWith(".vac"))
					{
						string text2 = ExtractZipFile(saveName);
						if (text2 != null && text2.EndsWith(".json"))
						{
							saveName = text2;
						}
					}
					if (saveName.EndsWith(".json") && FileManager.FileExists(saveName))
					{
						using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(saveName, restrictPath: true))
						{
							string aJSON = fileEntryStreamReader.ReadToEnd();
							loadJson = JSON.Parse(aJSON);
						}
						loadedName = saveName;
						FileManager.PushLoadDirFromFilePath(saveName);
						_isLoading = true;
						StartCoroutine(LoadCo(clearOnly: false, loadMerge));
					}
					else
					{
						Error("json file " + saveName + " is missing");
					}
				}
				else
				{
					onStartScene = false;
				}
				return;
			}
			catch (Exception ex)
			{
				_isLoading = false;
				Error("Exception during load " + ex.Message);
				return;
			}
		}
		UnityEngine.Debug.LogWarning("Already loading file " + loadedName + ". Can't load another until complete");
	}

	public void Load(string saveName = "savefile")
	{
		LoadInternal(saveName);
	}

	public void LoadForEdit(string saveName = "savefile")
	{
		LoadInternal(saveName, loadMerge: false, editMode: true);
	}

	public void LoadMerge(string saveName = "savefile")
	{
		LoadInternal(saveName, loadMerge: true);
	}

	public void LoadFromJSONEmbed(JSONEmbed je, bool loadMerge = false, bool editMode = false)
	{
		if (Application.isPlaying)
		{
			onStartScene = false;
			if (editMode)
			{
				gameMode = GameMode.Edit;
			}
			else
			{
				gameMode = GameMode.Play;
			}
			loadJson = JSON.Parse(je.jsonStore);
			loadedName = je.name;
			FileManager.SetLoadDir(string.Empty);
			_isLoading = true;
			StartCoroutine(LoadCo(clearOnly: false, loadMerge));
		}
	}

	protected void IncrementLoadingSlider()
	{
		if (loadingProgressSlider != null)
		{
			loadingProgressSlider.value += 1f;
		}
		if (loadingProgressSliderAlt != null)
		{
			loadingProgressSliderAlt.value += 1f;
		}
	}

	protected void UpdateLoadingStatus(string txt)
	{
		if (loadingTextStatus != null)
		{
			loadingTextStatus.text = txt;
		}
		if (loadingTextStatusAlt != null)
		{
			loadingTextStatusAlt.text = txt;
		}
	}

	protected void RemoveNonStartingAtoms()
	{
		if (startingAtoms == null)
		{
			return;
		}
		List<Atom> list = new List<Atom>();
		foreach (Atom value in atoms.Values)
		{
			if (!startingAtoms.ContainsKey(value.uid))
			{
				list.Add(value);
			}
		}
		foreach (Atom item in list)
		{
			if (item != null)
			{
				RemoveAtom(item);
			}
		}
	}

	protected void LoadDialog(string lastPath)
	{
		string text = savesDirResolved + "scene";
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(text, allowNavigationAboveRegularDirectories: true, useFullPaths: false, generateAllFlattenedShortcut: true);
		if (FileManager.DirectoryExists("Saves/Downloads"))
		{
			ShortCut shortCut = new ShortCut();
			shortCut.displayName = "Saves\\Downloads";
			shortCut.path = "Saves\\Downloads";
			shortCutsForDirectory.Insert(1, shortCut);
		}
		fileBrowserUI.shortCuts = shortCutsForDirectory;
		string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastPath);
		if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
		{
			text = suggestedBrowserDirectoryFromDirectoryPath;
		}
		fileBrowserUI.defaultPath = text;
		activeUI = ActiveUI.None;
		fileBrowserUI.SetTextEntry(b: false);
		fileBrowserUI.keepOpen = false;
	}

	public void LoadMergeSceneDialog()
	{
		LoadDialog(lastLoadDir);
		fileBrowserUI.SetTitle("Select Load File");
		fileBrowserUI.Show(LoadMerge);
	}

	public void LoadSceneForEditDialog()
	{
		LoadDialog(lastLoadDir);
		fileBrowserUI.SetTitle("Select Load File");
		fileBrowserUI.Show(LoadForEdit);
	}

	public void LoadSceneDialog()
	{
		LoadDialog(lastLoadDir);
		fileBrowserUI.SetTitle("Select Load File");
		fileBrowserUI.Show(Load);
	}

	public void HardReset()
	{
		UnregisterAllPrefabsFromAtoms();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void OpenFolderInExplorer(string path)
	{
		if (MonitorRigActive && path != null && FileManager.DirectoryExists(path, onlySystemDirectories: true))
		{
			string fullPath = Path.GetFullPath(path);
			Process.Start("explorer", fullPath);
		}
	}

	public string NormalizePath(string path)
	{
		return FileManager.NormalizePath(path);
	}

	public string NormalizeMediaPath(string path)
	{
		string result = path;
		if (path != null && path != string.Empty)
		{
			lastMediaDir = FileManager.GetDirectoryName(path);
			result = NormalizePath(path);
		}
		return result;
	}

	public string NormalizeScenePath(string path)
	{
		string result = path;
		if (path != null && path != string.Empty)
		{
			lastScenePathDir = FileManager.GetDirectoryName(path);
			result = NormalizePath(path);
		}
		return result;
	}

	public string NormalizeDirectoryPath(string path)
	{
		string result = path;
		if (path != null && path != string.Empty)
		{
			lastBrowseDir = FileManager.GetDirectoryName(path);
			result = NormalizePath(path);
		}
		return result;
	}

	public string NormalizeLoadPath(string path)
	{
		string text = path;
		if (path != string.Empty && path != "NULL")
		{
			text = FileManager.NormalizeLoadPath(path);
			if (!text.StartsWith("http") && !File.Exists(text))
			{
				foreach (KeyValuePair<string, string> pathMigrationMapping in pathMigrationMappings)
				{
					string key = pathMigrationMapping.Key;
					string pattern = "^" + key;
					if (Regex.IsMatch(text, pattern))
					{
						string value = pathMigrationMapping.Value;
						string text2 = Regex.Replace(text, pattern, value);
						text = text2;
					}
				}
			}
		}
		return text;
	}

	public void PushOverrideLoadDirFromFilePath(string path)
	{
		FileManager.PushLoadDirFromFilePath(path, restrictPath: true);
	}

	public void PopOverrideLoadDir()
	{
		FileManager.PopLoadDir();
	}

	public void SetLoadDirFromFilePath(string path)
	{
		FileManager.SetLoadDirFromFilePath(path, restrictPath: true);
	}

	public void SetSaveDirFromFilePath(string path)
	{
		FileManager.SetSaveDirFromFilePath(path);
	}

	public void SetNullSaveDir()
	{
		FileManager.SetNullSaveDir();
	}

	protected void BuildMigrationMappings()
	{
		legacyDirectories = new List<string>();
		legacyDirectories.Add("Textures/");
		legacyDirectories.Add("Saves/Scripts/");
		legacyDirectories.Add("Saves/Assets/");
		legacyDirectories.Add("Import/morphs/");
		legacyDirectories.Add("Import/");
		pathMigrationMappings = new Dictionary<string, string>();
		pathMigrationMappings.Add("Textures/", "Custom/Atom/Person/Textures/");
		pathMigrationMappings.Add("Saves/Scripts/", "Custom/Scripts/");
		pathMigrationMappings.Add("Import/morphs/", "Custom/Atom/Person/Morphs/");
		pathMigrationMappings.Add("Saves/Assets/", "Custom/Assets/");
	}

	protected bool BuildFilesToMigrateMap()
	{
		filesToMigrateMap = new Dictionary<string, string>();
		bool flag = false;
		try
		{
			bool flag2 = false;
			StringBuilder stringBuilder = new StringBuilder(25000);
			StringBuilder stringBuilder2 = new StringBuilder(25000);
			StreamWriter streamWriter = new StreamWriter("migrate.log");
			streamWriter.WriteLine("Report:");
			foreach (KeyValuePair<string, string> pathMigrationMapping in pathMigrationMappings)
			{
				string key = pathMigrationMapping.Key;
				string value = pathMigrationMapping.Value;
				if (!FileManager.DirectoryExists(key))
				{
					continue;
				}
				string[] files = Directory.GetFiles(key, "*", SearchOption.AllDirectories);
				string pattern = "^" + key;
				string[] array = files;
				foreach (string text in array)
				{
					flag = true;
					string text2 = text;
					string text3 = Regex.Replace(text2, pattern, value);
					string value2 = text2;
					string value3 = text3;
					if (text2.Length > text3.Length)
					{
						value3 = text3.PadRight(text2.Length);
					}
					else if (text3.Length > text2.Length)
					{
						value2 = text2.PadRight(text3.Length);
					}
					streamWriter.WriteLine(text2 + " -> " + text3);
					if (stringBuilder.Length < 16000 && stringBuilder2.Length < 16000)
					{
						stringBuilder.AppendLine(value2);
						stringBuilder2.AppendLine(value3);
					}
					else
					{
						flag2 = true;
					}
					filesToMigrateMap.Add(text, text3);
				}
			}
			if (flag2)
			{
				stringBuilder.AppendLine("Truncated...too long to display. See migrate.log");
			}
			if (!flag)
			{
				streamWriter.WriteLine("No files found that need migrating");
			}
			streamWriter.Close();
			if (oldPathsText != null)
			{
				oldPathsText.text = stringBuilder.ToString();
			}
			if (newPathsText != null)
			{
				newPathsText.text = stringBuilder2.ToString();
			}
			if (flag)
			{
				HideMainHUD();
			}
			if (migratePathsPanel != null)
			{
				migratePathsPanel.gameObject.SetActive(flag);
			}
		}
		catch (Exception ex)
		{
			Error("Exception during search for migration file " + ex);
		}
		return flag;
	}

	public void CancelMigrateFiles()
	{
		if (migratePathsPanel != null)
		{
			migratePathsPanel.gameObject.SetActive(value: false);
		}
		if (startSceneEnabled)
		{
			StartCoroutine(DelayStart());
		}
	}

	protected void RemoveLegacyDirectories(StreamWriter log)
	{
		try
		{
			foreach (string legacyDirectory in legacyDirectories)
			{
				if (FileManager.DirectoryExists(legacyDirectory))
				{
					string[] files = Directory.GetFiles(legacyDirectory, "*", SearchOption.AllDirectories);
					if (files.Length == 0)
					{
						log?.WriteLine("Deleting directory " + legacyDirectory);
						UnityEngine.Debug.Log("Delete directory " + legacyDirectory);
						Directory.Delete(legacyDirectory, recursive: true);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Error("Exception during migrate directory removal " + ex);
		}
	}

	public void MigrateFilesInMigrateMap()
	{
		if (filesToMigrateMap != null)
		{
			try
			{
				StreamWriter streamWriter = new StreamWriter("migrate.log", append: true);
				foreach (KeyValuePair<string, string> item in filesToMigrateMap)
				{
					string key = item.Key;
					string value = item.Value;
					streamWriter.WriteLine("Migrate file " + key + " to " + value + " ...");
					UnityEngine.Debug.Log("Migrate file " + key + " to " + value);
					string directoryName = Path.GetDirectoryName(value);
					try
					{
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						File.SetAttributes(key, FileAttributes.Normal);
						if (!File.Exists(value))
						{
							File.Move(key, value);
							streamWriter.WriteLine("  ...File moved");
						}
						else
						{
							streamWriter.WriteLine("  ...File already exists in new location. Just removing old file");
							File.Delete(key);
						}
					}
					catch (Exception ex)
					{
						Error("Exception during migrate of " + key + " :" + ex);
					}
				}
				RemoveLegacyDirectories(streamWriter);
				streamWriter.Close();
			}
			catch (Exception ex2)
			{
				Error("Exception during migrate copy " + ex2);
			}
		}
		CancelMigrateFiles();
	}

	protected void ProcessSaveScreenshot(bool force = false)
	{
		if (GetRightSelect() || GetLeftSelect() || GetMouseSelect() || force)
		{
			if (screenshotCamera != null)
			{
				RenderTexture targetTexture = screenshotCamera.targetTexture;
				if (targetTexture != null)
				{
					try
					{
						Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.ARGB32, mipmap: false);
						RenderTexture.active = targetTexture;
						texture2D.ReadPixels(new Rect(0f, 0f, targetTexture.width, targetTexture.height), 0, 0);
						texture2D.Apply();
						byte[] bytes = texture2D.EncodeToJPG(100);
						string text = savingName.Replace(".json", ".jpg");
						text = text.Replace(".vac", ".jpg");
						FileManager.WriteAllBytes(text, bytes);
						if (fileBrowserUI != null)
						{
							fileBrowserUI.ClearCacheImage(text);
						}
						if (screenShotCallback != null)
						{
							screenShotCallback(text);
							screenShotCallback = null;
						}
						UnityEngine.Object.Destroy(texture2D);
					}
					catch (Exception ex)
					{
						LogError("Exception during screenshot processing: " + ex.Message);
					}
				}
				screenshotCamera.enabled = false;
			}
			SelectModeOff();
			ShowMainHUDAuto();
		}
		if (GetCancel())
		{
			SelectModeOff();
			ShowMainHUDAuto();
		}
	}

	protected void ProcessHiResScreenshot()
	{
		try
		{
			if ((GetRightSelect() || GetLeftSelect() || GetMouseSelect()) && hiResScreenshotCamera != null)
			{
				RenderTexture targetTexture = hiResScreenshotCamera.targetTexture;
				if (targetTexture != null)
				{
					Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.ARGB32, mipmap: false);
					RenderTexture.active = targetTexture;
					texture2D.ReadPixels(new Rect(0f, 0f, targetTexture.width, targetTexture.height), 0, 0);
					texture2D.Apply();
					byte[] bytes = texture2D.EncodeToJPG(100);
					int num = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
					string text = savesDirResolved + "screenshots\\" + num + ".jpg";
					int num2 = text.LastIndexOf('\\');
					if (num2 >= 0)
					{
						string path = text.Substring(0, num2);
						FileManager.CreateDirectory(path);
					}
					FileManager.WriteAllBytes(text, bytes);
					if (SkyshopLightController.singleton != null)
					{
						SkyshopLightController.singleton.Flash();
					}
					UnityEngine.Object.Destroy(texture2D);
				}
			}
			if (GetCancel())
			{
				SelectModeOff();
			}
		}
		catch (Exception ex)
		{
			LogError("Exception during process of screenshot: " + ex.Message);
		}
	}

	public void DoSaveScreenshot(string saveName, ScreenShotCallback callback = null)
	{
		if (screenshotCamera != null)
		{
			savingName = saveName;
			screenShotCallback = callback;
			if (screenshotPreview != null)
			{
				screenshotPreview.gameObject.SetActive(value: true);
			}
			ResetSelectionInstances();
			ClearSelectionHUDs();
			HideMainHUD();
			selectMode = SelectMode.SaveScreenshot;
			helpText = "Aim head and press select to take screenshot for save.";
			SyncVisibility();
			screenshotCamera.enabled = true;
			if (loResScreenShotCameraFOVSlider != null)
			{
				screenshotCamera.fieldOfView = loResScreenShotCameraFOVSlider.value;
			}
			else
			{
				screenshotCamera.fieldOfView = 40f;
			}
		}
	}

	public string[] GetFilesAtPath(string path, string pattern = null)
	{
		string[] result = null;
		if (FileManager.IsSecureReadPath(path))
		{
			result = FileManager.GetFiles(path, pattern, restrictPath: true);
		}
		else
		{
			Error("Attempted to use GetFilesAtPath on a path that is not inside game directory. That is not allowed");
		}
		return result;
	}

	public string[] GetDirectoriesAtPath(string path, string pattern = null)
	{
		string[] result = null;
		if (FileManager.IsSecureReadPath(path))
		{
			result = FileManager.GetDirectories(path, pattern, restrictPath: true);
		}
		else
		{
			Error("Attempted to use GetDirectoriesAtPath on a path that is not inside game directory. That is not allowed");
		}
		return result;
	}

	public string ReadFileIntoString(string path)
	{
		string result = null;
		if (FileManager.IsSecureReadPath(path))
		{
			result = FileManager.ReadAllText(path, restrictPath: true);
		}
		else
		{
			Error("Attempted to use ReadFileIntoString on a path that is not inside game directory. That is not allowed");
		}
		return result;
	}

	public void SaveStringIntoFile(string path, string contents)
	{
		if (FileManager.IsSecureWritePath(path))
		{
			FileManager.WriteAllText(path, contents);
		}
		else
		{
			Error("Attempted to use ReadFileIntoString on a path that is not inside game directory Saves or Custom area. That is not allowed");
		}
	}

	private void SyncGameMode()
	{
		if (_gameMode == GameMode.Edit)
		{
			if (editModeToggle != null)
			{
				editModeToggle.isOn = true;
			}
			if (playModeToggle != null)
			{
				playModeToggle.isOn = false;
			}
			if (_activeUI == ActiveUI.SelectedOptions)
			{
				activeUI = ActiveUI.SelectedOptions;
			}
			Transform[] array = editModeOnlyTransforms;
			foreach (Transform transform in array)
			{
				if (!advancedSceneEditDisabled || (transform != addAtomUI && transform != addAtomUIAlt && transform != animationUI && transform != audioUI))
				{
					transform.gameObject.SetActive(value: true);
				}
			}
		}
		else
		{
			if (editModeToggle != null)
			{
				editModeToggle.isOn = false;
			}
			if (playModeToggle != null)
			{
				playModeToggle.isOn = true;
			}
			if (_activeUI == ActiveUI.SelectedOptions)
			{
				activeUI = ActiveUI.SelectedOptions;
			}
			Transform[] array2 = editModeOnlyTransforms;
			foreach (Transform transform2 in array2)
			{
				transform2.gameObject.SetActive(value: false);
			}
		}
		SyncVisibility();
	}

	public void PurgeImageCache()
	{
		if (ImageLoaderThreaded.singleton != null)
		{
			ImageLoaderThreaded.singleton.PurgeAllTextures();
		}
	}

	public void UnloadUnusedResources()
	{
		Resources.UnloadUnusedAssets();
	}

	public void GarbageCollect()
	{
		LogMessage("Memory usage before garbage collect " + GC.GetTotalMemory(forceFullCollection: false));
		GC.Collect();
		LogMessage("Memory usage  after garbage collect " + GC.GetTotalMemory(forceFullCollection: true));
	}

	public void ReportLoadedAssetBundles()
	{
		AssetBundleManager.ReportLoadedAssetBundles();
	}

	public void Quit()
	{
		Application.Quit();
	}

	public static void LogError(string err, bool logToFile = true)
	{
		if (_singleton != null)
		{
			_singleton.Error(err, logToFile);
		}
		else
		{
			UnityEngine.Debug.LogError(err);
		}
	}

	public static void LogMessage(string msg, bool logToFile = true)
	{
		if (_singleton != null)
		{
			_singleton.Message(msg, logToFile);
		}
		else
		{
			UnityEngine.Debug.Log(msg);
		}
	}

	public void OpenErrorLogPanel()
	{
		if (!_mainHUDVisible)
		{
			ShowMainHUDAuto();
		}
		if (errorLogPanel != null)
		{
			errorLogPanel.gameObject.SetActive(value: true);
		}
	}

	public void CloseErrorLogPanel()
	{
		if (errorLogPanel != null)
		{
			errorLogPanel.gameObject.SetActive(value: false);
		}
	}

	public void ClearErrors()
	{
		errorCount = 0;
		errorLog = string.Empty;
		if (allErrorsText != null)
		{
			allErrorsText.text = string.Empty;
		}
		if (allErrorsText2 != null)
		{
			allErrorsText2.text = string.Empty;
		}
	}

	public void TruncateErrors()
	{
		if (errorLog.Length > maxLength)
		{
			errorLog = errorLog.Substring(0, maxLength / 2);
			errorLog += "\n<Truncated>\n";
		}
		if (allErrorsText != null && allErrorsText.text.Length > maxLength)
		{
			allErrorsText.text = allErrorsText.text.Substring(0, maxLength / 2);
			allErrorsText.text += "\n<Truncated>\n";
		}
		if (allErrorsText2 != null && allErrorsText2.text.Length > maxLength)
		{
			allErrorsText2.text = allErrorsText2.text.Substring(0, maxLength / 2);
			allErrorsText2.text += "\n<Truncated>\n";
		}
	}

	public void Error(string err, bool logToFile = true)
	{
		if (!_mainHUDVisible)
		{
			ShowMainHUDAuto();
		}
		errorCount++;
		errorSplashCount = errorSplashTime;
		errorLog = errorLog + err + "\n";
		if (allErrorsText != null)
		{
			allErrorsText.text = errorLog;
		}
		if (allErrorsText2 != null)
		{
			allErrorsText2.text = errorLog;
		}
		TruncateErrors();
		if (logToFile)
		{
			UnityEngine.Debug.LogError(err);
		}
	}

	public void OpenMessageLogPanel()
	{
		if (!_mainHUDVisible)
		{
			ShowMainHUDAuto();
		}
		if (msgLogPanel != null)
		{
			msgLogPanel.gameObject.SetActive(value: true);
		}
	}

	public void CloseMessageLogPanel()
	{
		if (msgLogPanel != null)
		{
			msgLogPanel.gameObject.SetActive(value: false);
		}
	}

	public void ClearMessages()
	{
		msgCount = 0;
		msgLog = string.Empty;
		if (allMessagesText != null)
		{
			allMessagesText.text = string.Empty;
		}
		if (allMessagesText2 != null)
		{
			allMessagesText2.text = string.Empty;
		}
	}

	public void TruncateMessages()
	{
		if (msgLog.Length > maxLength)
		{
			msgLog = msgLog.Substring(0, maxLength / 2);
			msgLog += "\n<Truncated>\n";
		}
		if (allMessagesText != null && allMessagesText.text.Length > maxLength)
		{
			allMessagesText.text = allMessagesText.text.Substring(0, maxLength / 2);
			allMessagesText.text += "\n<Truncated>\n";
		}
		if (allMessagesText2 != null && allMessagesText2.text.Length > maxLength)
		{
			allMessagesText2.text = allMessagesText2.text.Substring(0, maxLength / 2);
			allMessagesText2.text += "\n<Truncated>\n";
		}
	}

	public void Message(string msg, bool logToFile = true)
	{
		msgCount++;
		msgSplashCount = msgSplashTime;
		msgLog = msgLog + msg + "\n";
		if (allMessagesText != null)
		{
			allMessagesText.text = msgLog;
		}
		if (allMessagesText2 != null)
		{
			allMessagesText2.text = msgLog;
		}
		TruncateMessages();
		if (logToFile)
		{
			UnityEngine.Debug.Log(msg);
		}
	}

	protected void CheckMessageAndErrorQueue()
	{
		if (errorSplashCount > 0)
		{
			errorSplashCount--;
			if (errorSplashCount == 0)
			{
				if (errorSplashTransform != null)
				{
					errorSplashTransform.gameObject.SetActive(value: false);
				}
			}
			else if (errorSplashTransform != null)
			{
				errorSplashTransform.gameObject.SetActive(value: true);
			}
		}
		else
		{
			if (msgSplashCount <= 0)
			{
				return;
			}
			msgSplashCount--;
			if (msgSplashCount == 0)
			{
				if (msgSplashTransform != null)
				{
					msgSplashTransform.gameObject.SetActive(value: false);
				}
			}
			else if (msgSplashTransform != null)
			{
				msgSplashTransform.gameObject.SetActive(value: true);
			}
		}
	}

	public bool IsSimulationPaused()
	{
		return _pauseSimulation;
	}

	protected void CheckResumeSimulation()
	{
		if (pauseFrames >= 0)
		{
			pauseFrames--;
			if (pauseFrames < 0 && pauseSimulationTimerFlag != null)
			{
				pauseSimulationTimerFlag.Raise();
			}
		}
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		bool flag = false;
		if (waitResumeSimulationFlags.Count > 0)
		{
			List<AsyncFlag> list = new List<AsyncFlag>();
			Text[] array = waitReasonTexts;
			foreach (Text text in array)
			{
				text.text = string.Empty;
			}
			int num = 0;
			foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
			{
				if (num < waitReasonTexts.Length)
				{
					waitReasonTexts[num].text = waitResumeSimulationFlag.Name;
					num++;
				}
				if (waitResumeSimulationFlag.Raised)
				{
					list.Add(waitResumeSimulationFlag);
					flag = true;
				}
			}
			foreach (AsyncFlag item in list)
			{
				waitResumeSimulationFlags.Remove(item);
			}
		}
		if (waitResumeSimulationFlags.Count > 0)
		{
			if (waitTransform != null && !hideWaitTransform && !hiddenPause)
			{
				waitTransform.gameObject.SetActive(value: true);
			}
			pauseSimulation = true;
		}
		else if (flag)
		{
			if (waitTransform != null)
			{
				waitTransform.gameObject.SetActive(value: false);
			}
			pauseSimulation = false;
		}
	}

	public void PauseSimulation(AsyncFlag af, bool hidden = false)
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		waitResumeSimulationFlags.Add(af);
		hiddenPause = hidden;
		pauseSimulation = true;
		foreach (Atom value in atoms.Values)
		{
			value.PauseSimulation(af);
		}
	}

	public void PauseSimulation(int numFrames, string pauseName, bool hidden = false)
	{
		if (pauseFrames < numFrames)
		{
			pauseFrames = numFrames;
			if (waitResumeSimulationFlags == null)
			{
				waitResumeSimulationFlags = new List<AsyncFlag>();
			}
			if (pauseSimulationTimerFlag == null)
			{
				pauseSimulationTimerFlag = new AsyncFlag(pauseName);
			}
			if (!waitResumeSimulationFlags.Contains(pauseSimulationTimerFlag))
			{
				pauseSimulationTimerFlag.Lower();
				PauseSimulation(pauseSimulationTimerFlag, hidden);
			}
		}
	}

	protected bool CheckHoldLoad()
	{
		if (holdLoadCompleteFlags == null)
		{
			holdLoadCompleteFlags = new List<AsyncFlag>();
		}
		if (holdLoadCompleteFlags.Count > 0)
		{
			List<AsyncFlag> list = new List<AsyncFlag>();
			foreach (AsyncFlag holdLoadCompleteFlag in holdLoadCompleteFlags)
			{
				if (holdLoadCompleteFlag.Raised)
				{
					list.Add(holdLoadCompleteFlag);
				}
			}
			foreach (AsyncFlag item in list)
			{
				holdLoadCompleteFlags.Remove(item);
			}
		}
		if (holdLoadCompleteFlags.Count > 0)
		{
			return true;
		}
		return false;
	}

	public void HoldLoadComplete(AsyncFlag af)
	{
		if (holdLoadCompleteFlags == null)
		{
			holdLoadCompleteFlags = new List<AsyncFlag>();
		}
		holdLoadCompleteFlags.Add(af);
	}

	public void SetLoadFlag()
	{
		if (loadFlag != null)
		{
			loadFlag.Raise();
		}
	}

	protected bool CheckLoadingIcon()
	{
		if (loadingIconFlags == null)
		{
			loadingIconFlags = new List<AsyncFlag>();
		}
		if (loadingIconFlags.Count > 0)
		{
			List<AsyncFlag> list = new List<AsyncFlag>();
			foreach (AsyncFlag loadingIconFlag in loadingIconFlags)
			{
				if (loadingIconFlag.Raised)
				{
					list.Add(loadingIconFlag);
				}
			}
			foreach (AsyncFlag item in list)
			{
				loadingIconFlags.Remove(item);
			}
		}
		if (loadingIconFlags.Count > 0)
		{
			if (loadingIcon != null)
			{
				loadingIcon.gameObject.SetActive(value: true);
			}
			return true;
		}
		if (loadingIcon != null)
		{
			loadingIcon.gameObject.SetActive(value: false);
		}
		return false;
	}

	public void SetLoadingIconFlag(AsyncFlag af)
	{
		if (loadingIconFlags == null)
		{
			loadingIconFlags = new List<AsyncFlag>();
		}
		loadingIconFlags.Add(af);
	}

	protected void SyncFreezeAnimation()
	{
		if (allAnimators == null)
		{
			return;
		}
		foreach (Animator allAnimator in allAnimators)
		{
			allAnimator.enabled = !_freezeAnimation;
		}
	}

	public void SetFreezeAnimation(bool freeze)
	{
		if (_freezeAnimation != freeze)
		{
			_freezeAnimation = freeze;
			SyncFreezeAnimation();
			if (freezeAnimationToggle != null)
			{
				freezeAnimationToggle.isOn = _freezeAnimation;
			}
			if (freezeAnimationToggleAlt != null)
			{
				freezeAnimationToggleAlt.isOn = _freezeAnimation;
			}
		}
	}

	public void SyncVersionText()
	{
		SettingsManager.APP_PATH = Path.GetFullPath(".");
		SettingsManager.RegeneratePaths();
		string vERSION_FILE_LOCAL_PATH = SettingsManager.VERSION_FILE_LOCAL_PATH;
		if (File.Exists(vERSION_FILE_LOCAL_PATH))
		{
			string cipherText = File.ReadAllText(SettingsManager.PATCH_VERSION_PATH).Replace("\n", string.Empty).Replace("\r", string.Empty);
			resolvedVersion = Rijndael.Decrypt(cipherText, SettingsManager.PATCH_VERSION_ENCRYPTION_PASSWORD);
		}
		else
		{
			resolvedVersion = version;
		}
		if (versionText != null)
		{
			versionText.text = "Version: " + resolvedVersion;
		}
		if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.versionText != null)
		{
			GlobalSceneOptions.singleton.versionText.text = "Version: " + resolvedVersion;
		}
	}

	public string GetVersion()
	{
		if (resolvedVersion != null)
		{
			return resolvedVersion;
		}
		return version;
	}

	private void SyncSelectAtomPopup()
	{
		if (!(selectAtomPopup != null) || _isLoading || _pauseSyncAtomLists)
		{
			return;
		}
		string text = string.Empty;
		if (selectedController != null && selectedController.containingAtom != null)
		{
			text = selectedController.containingAtom.uid;
		}
		bool flag = false;
		List<string> list = GetAtomUIDsWithFreeControllers();
		selectAtomPopup.numPopupValues = list.Count + 1;
		selectAtomPopup.setPopupValue(0, "None");
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == text)
			{
				flag = true;
			}
			selectAtomPopup.setPopupValue(i + 1, list[i]);
		}
		if (!flag)
		{
			selectAtomPopup.currentValue = "None";
			SyncControllerPopup("None");
		}
	}

	public void SelectLastAddedAtom()
	{
		if (visibleAtomUIDsWithFreeControllers != null && visibleAtomUIDsWithFreeControllers.Count > 0 && selectAtomPopup != null)
		{
			selectAtomPopup.currentValue = visibleAtomUIDsWithFreeControllers[visibleAtomUIDsWithFreeControllers.Count - 1];
		}
	}

	public void CycleSelectAtomOfType(string type)
	{
		if (visibleAtomUIDsWithFreeControllers == null)
		{
			return;
		}
		List<string> list = visibleAtomUIDsWithFreeControllers;
		string text = null;
		if (lastCycleSelectAtomType != null && lastCycleSelectAtomType == type)
		{
			List<string> list2 = new List<string>();
			int num = 0;
			int num2 = 0;
			foreach (string item in list)
			{
				Atom atomByUid = GetAtomByUid(item);
				if (atomByUid.type == type)
				{
					list2.Add(item);
					if (lastCycleSelectAtomUid == item)
					{
						num2 = num;
					}
					num++;
				}
			}
			text = ((num2 != list2.Count - 1) ? list2[num2 + 1] : list2[0]);
		}
		else
		{
			foreach (string item2 in list)
			{
				Atom atomByUid2 = GetAtomByUid(item2);
				if (atomByUid2.type == type)
				{
					text = item2;
					break;
				}
			}
		}
		if (text != null)
		{
			lastCycleSelectAtomUid = text;
			lastCycleSelectAtomType = type;
			if (selectAtomPopup != null)
			{
				selectAtomPopup.currentValue = text;
			}
			List<string> freeControllerNamesInAtom = GetFreeControllerNamesInAtom(text);
			if (freeControllerNamesInAtom != null && freeControllerNamesInAtom.Count > 1)
			{
				selectControllerPopup.currentValue = freeControllerNamesInAtom[0];
			}
			activeUI = ActiveUI.SelectedOptions;
		}
	}

	private void SyncControllerPopup(string nv)
	{
		string currentValue = selectAtomPopup.currentValue;
		selectControllerPopup.currentValueNoCallback = "None";
		if (currentValue == "None")
		{
			selectControllerPopup.numPopupValues = 1;
			selectControllerPopup.setPopupValue(0, "None");
			return;
		}
		List<string> freeControllerNamesInAtom = GetFreeControllerNamesInAtom(currentValue);
		if (freeControllerNamesInAtom == null || freeControllerNamesInAtom.Count == 0)
		{
			selectControllerPopup.numPopupValues = 1;
			selectControllerPopup.setPopupValue(0, "None");
			return;
		}
		selectControllerPopup.numPopupValues = freeControllerNamesInAtom.Count + 1;
		selectControllerPopup.setPopupValue(0, "None");
		for (int i = 0; i < freeControllerNamesInAtom.Count; i++)
		{
			selectControllerPopup.setPopupValue(i + 1, freeControllerNamesInAtom[i]);
		}
		if (freeControllerNamesInAtom.Count == 1)
		{
			selectControllerPopup.currentValue = freeControllerNamesInAtom[0];
		}
	}

	private void SyncUIToSelectedController()
	{
		if (selectedController == null)
		{
			if (selectedControllerNameDisplay != null)
			{
				selectedControllerNameDisplay.text = string.Empty;
			}
			if (selectAtomPopup != null)
			{
				selectAtomPopup.currentValueNoCallback = "None";
			}
			if (selectControllerPopup != null)
			{
				selectControllerPopup.currentValueNoCallback = "None";
			}
			return;
		}
		Atom containingAtom = selectedController.containingAtom;
		if (containingAtom != null)
		{
			if (selectedControllerNameDisplay != null)
			{
				selectedControllerNameDisplay.text = selectedController.containingAtom.uid + ":" + selectedController.name;
			}
			if (selectAtomPopup != null)
			{
				selectAtomPopup.currentValue = containingAtom.uid;
			}
			if (selectControllerPopup != null)
			{
				selectControllerPopup.currentValueNoCallback = selectedController.name;
			}
		}
		else if (selectedControllerNameDisplay != null)
		{
			selectedControllerNameDisplay.text = selectedController.name;
		}
	}

	public void SetAlignRotationOffset(string type)
	{
		try
		{
			alignRotationOffset = (AlignRotationOffset)Enum.Parse(typeof(AlignRotationOffset), type);
		}
		catch (ArgumentException)
		{
			Error("Attempted to align rotation offset type to " + type + " which is not a valid type");
		}
	}

	public void AlignRigFacingSelectedController(bool rotationOnly = false)
	{
		if (!(selectedController != null))
		{
			return;
		}
		Transform focusPoint = selectedController.focusPoint;
		if (focusPoint == null)
		{
			focusPoint = selectedController.transform;
		}
		Vector3 up = navigationRig.up;
		Vector3 toDirection = Vector3.ProjectOnPlane(focusPoint.position - motionControllerHead.position, up);
		Vector3 fromDirection = Vector3.ProjectOnPlane(motionControllerHead.forward, up);
		Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
		navigationRig.rotation = quaternion * navigationRig.rotation;
		if (!rotationOnly)
		{
			Possessor component = motionControllerHead.GetComponent<Possessor>();
			Vector3 position = focusPoint.position;
			Vector3 vector = position - component.autoSnapPoint.position;
			Vector3 vector2 = navigationRig.position + vector - motionControllerHead.forward * 1.5f;
			float num = Vector3.Dot(vector2 - navigationRig.position, up);
			vector2 += up * (0f - num);
			navigationRig.position = vector2;
			playerHeightAdjust += num;
		}
		if (!_mainHUDAnchoredOnMonitor)
		{
			if (_alignRotationOffset == AlignRotationOffset.Left)
			{
				navigationRig.Rotate(0f, 30f, 0f);
			}
			else if (_alignRotationOffset == AlignRotationOffset.Right)
			{
				navigationRig.Rotate(0f, -30f, 0f);
			}
		}
	}

	public void SetCustomUI(Transform cui)
	{
		if (customUI != null)
		{
			customUI.gameObject.SetActive(value: false);
		}
		if (!customUIDisabled)
		{
			customUI = cui;
		}
		else
		{
			customUI = alternateCustomUI;
		}
		activeUI = ActiveUI.Custom;
	}

	public void SetToLastActiveUI()
	{
		activeUI = _lastActiveUI;
	}

	private void ClearAllUI()
	{
		if (mainMenuUI != null)
		{
			mainMenuUI.gameObject.SetActive(value: false);
		}
		if (selectedController != null)
		{
			selectedController.guihidden = true;
		}
		if (multiButtonPanel != null)
		{
			multiButtonPanel.gameObject.SetActive(value: false);
		}
		if (sceneControlUI != null)
		{
			sceneControlUI.gameObject.SetActive(value: true);
		}
		if (embeddedSceneUI != null)
		{
			embeddedSceneUI.gameObject.SetActive(value: false);
		}
		if (customUI != null)
		{
			customUI.gameObject.SetActive(value: false);
		}
		if (onlineBrowserUI != null)
		{
			onlineBrowserUI.gameObject.SetActive(value: false);
		}
		if (packageBuilderUI != null)
		{
			packageBuilderUI.gameObject.SetActive(value: false);
		}
		if (packageManagerUI != null)
		{
			packageManagerUI.gameObject.SetActive(value: false);
		}
		if (fileBrowserUI != null)
		{
			fileBrowserUI.Hide();
		}
		if (mediaFileBrowserUI != null)
		{
			mediaFileBrowserUI.Hide();
		}
		if (directoryBrowserUI != null)
		{
			directoryBrowserUI.Hide();
		}
	}

	public void SetActiveUI(string uiName)
	{
		try
		{
			activeUI = (ActiveUI)Enum.Parse(typeof(ActiveUI), uiName);
		}
		catch (ArgumentException)
		{
			Error("Attempted to set UI to " + uiName + " which is not a valid UI name");
		}
	}

	public void SetMainMenuTab(string tabName)
	{
		if (mainMenuTabSelector != null)
		{
			mainMenuTabSelector.SetActiveTab(tabName);
		}
	}

	public void SetUserPrefsTab(string tabName)
	{
		if (userPrefsTabSelector != null)
		{
			userPrefsTabSelector.SetActiveTab(tabName);
		}
	}

	private void InitUI()
	{
		if (MonitorModeAuxUI != null)
		{
			if (isMonitorOnly && !UIDisabled)
			{
				MonitorModeAuxUI.gameObject.SetActive(value: true);
			}
			else
			{
				MonitorModeAuxUI.gameObject.SetActive(value: false);
			}
		}
		if (loadingUI != null)
		{
			loadingUI.gameObject.SetActive(value: false);
		}
		if (loadingUIAlt != null)
		{
			loadingUIAlt.gameObject.SetActive(value: false);
		}
		if (loadingGeometry != null)
		{
			loadingGeometry.gameObject.SetActive(value: false);
		}
		if (mainHUDAttachPoint != null)
		{
			mainHUDAttachPointStartingPosition = mainHUDAttachPoint.localPosition;
			mainHUDAttachPointStartingRotation = mainHUDAttachPoint.localRotation;
		}
		activeUI = ActiveUI.SelectedOptions;
		if (selectionHUDTransform != null)
		{
			selectionHUD = selectionHUDTransform.GetComponent<SelectionHUD>();
			SetSelectionHUDHeader(selectionHUD, "Highlighted Controllers");
		}
		if (rightSelectionHUDTransform != null)
		{
			rightSelectionHUD = rightSelectionHUDTransform.GetComponent<SelectionHUD>();
			SetSelectionHUDHeader(rightSelectionHUD, string.Empty);
		}
		if (leftSelectionHUDTransform != null)
		{
			leftSelectionHUD = leftSelectionHUDTransform.GetComponent<SelectionHUD>();
			SetSelectionHUDHeader(leftSelectionHUD, string.Empty);
		}
		if (worldScaleSlider != null)
		{
			worldScaleSlider.value = _worldScale;
			worldScaleSlider.onValueChanged.AddListener(delegate
			{
				worldScale = worldScaleSlider.value;
			});
			SliderControl component = worldScaleSlider.GetComponent<SliderControl>();
			if (component != null)
			{
				component.defaultValue = 1f;
			}
		}
		if (worldScaleSliderAlt != null)
		{
			worldScaleSliderAlt.value = _worldScale;
			worldScaleSliderAlt.onValueChanged.AddListener(delegate
			{
				worldScale = worldScaleSliderAlt.value;
			});
			SliderControl component2 = worldScaleSliderAlt.GetComponent<SliderControl>();
			if (component2 != null)
			{
				component2.defaultValue = 1f;
			}
		}
		if (controllerScaleSlider != null)
		{
			controllerScaleSlider.value = _controllerScale;
			controllerScaleSlider.onValueChanged.AddListener(delegate
			{
				controllerScale = controllerScaleSlider.value;
			});
			SliderControl component3 = controllerScaleSlider.GetComponent<SliderControl>();
			if (component3 != null)
			{
				component3.defaultValue = 1f;
			}
		}
		if (playerHeightAdjustSlider != null)
		{
			playerHeightAdjustSlider.value = _playerHeightAdjust;
			playerHeightAdjustSlider.onValueChanged.AddListener(delegate
			{
				playerHeightAdjust = playerHeightAdjustSlider.value;
			});
			SliderControl component4 = playerHeightAdjustSlider.GetComponent<SliderControl>();
			if (component4 != null)
			{
				component4.defaultValue = 0f;
			}
		}
		if (playerHeightAdjustSliderAlt != null)
		{
			playerHeightAdjustSliderAlt.value = _playerHeightAdjust;
			playerHeightAdjustSliderAlt.onValueChanged.AddListener(delegate
			{
				playerHeightAdjust = playerHeightAdjustSliderAlt.value;
			});
			SliderControl component5 = playerHeightAdjustSliderAlt.GetComponent<SliderControl>();
			if (component5 != null)
			{
				component5.defaultValue = 0f;
			}
		}
		if (monitorUIScaleSlider != null)
		{
			monitorUIScaleSlider.value = _monitorUIScale;
			monitorUIScaleSlider.onValueChanged.AddListener(delegate
			{
				monitorUIScale = monitorUIScaleSlider.value;
			});
			SliderControl component6 = monitorUIScaleSlider.GetComponent<SliderControl>();
			if (component6 != null)
			{
				component6.defaultValue = 1f;
			}
		}
		if (monitorUIYOffsetSlider != null)
		{
			monitorUIYOffsetSlider.value = _monitorUIYOffset;
			monitorUIYOffsetSlider.onValueChanged.AddListener(delegate
			{
				monitorUIYOffset = monitorUIYOffsetSlider.value;
			});
			SliderControl component7 = monitorUIYOffsetSlider.GetComponent<SliderControl>();
			if (component7 != null)
			{
				component7.defaultValue = 0f;
			}
		}
		if (monitorCameraFOVSlider != null)
		{
			monitorCameraFOVSlider.value = _monitorCameraFOV;
			monitorCameraFOVSlider.onValueChanged.AddListener(delegate
			{
				monitorCameraFOV = monitorCameraFOVSlider.value;
			});
			SliderControl component8 = monitorCameraFOVSlider.GetComponent<SliderControl>();
			if (component8 != null)
			{
				component8.defaultValue = 40f;
			}
		}
		if (editModeToggle != null)
		{
			editModeToggle.isOn = _gameMode == GameMode.Edit;
			editModeToggle.onValueChanged.AddListener(delegate
			{
				if (editModeToggle.isOn)
				{
					gameMode = GameMode.Edit;
				}
			});
		}
		if (playModeToggle != null)
		{
			playModeToggle.isOn = _gameMode == GameMode.Play;
			playModeToggle.onValueChanged.AddListener(delegate
			{
				if (playModeToggle.isOn)
				{
					gameMode = GameMode.Play;
				}
			});
		}
		if (selectedControllerNameDisplay != null)
		{
			selectedControllerNameDisplay.text = string.Empty;
		}
		if (rayLineMaterialLeft != null)
		{
			rayLineDrawerLeft = new LineDrawer(rayLineMaterialLeft);
		}
		if (rayLineMaterialRight != null)
		{
			rayLineDrawerRight = new LineDrawer(rayLineMaterialRight);
		}
		if (twoStageLineMaterial != null)
		{
			rightTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			leftTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			headTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			leapRightTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			leapLeftTwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker1TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker2TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker3TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker4TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker5TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker6TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker7TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
			tracker8TwoStageLineDrawer = new LineDrawer(twoStageLineMaterial);
		}
		if (UISidePopup != null)
		{
			UISidePopup.currentValueNoCallback = _UISide.ToString();
			UIPopup uISidePopup = UISidePopup;
			uISidePopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uISidePopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetUISide));
		}
		if (helpToggle != null)
		{
			helpToggle.onValueChanged.AddListener(delegate
			{
				helpOverlayOn = helpToggle.isOn;
			});
		}
		if (helpToggleAlt != null)
		{
			helpToggleAlt.onValueChanged.AddListener(delegate
			{
				helpOverlayOn = helpToggleAlt.isOn;
			});
		}
		SyncHelpOverlay();
		if (lockHeightDuringNavigateToggle != null)
		{
			lockHeightDuringNavigateToggle.onValueChanged.AddListener(delegate
			{
				lockHeightDuringNavigate = lockHeightDuringNavigateToggle.isOn;
			});
		}
		if (lockHeightDuringNavigateToggleAlt != null)
		{
			lockHeightDuringNavigateToggleAlt.onValueChanged.AddListener(delegate
			{
				lockHeightDuringNavigate = lockHeightDuringNavigateToggleAlt.isOn;
			});
		}
		SyncLockHeightDuringNavigate();
		if (freeMoveFollowFloorToggle != null)
		{
			freeMoveFollowFloorToggle.onValueChanged.AddListener(delegate
			{
				freeMoveFollowFloor = freeMoveFollowFloorToggle.isOn;
			});
		}
		if (freeMoveFollowFloorToggleAlt != null)
		{
			freeMoveFollowFloorToggleAlt.onValueChanged.AddListener(delegate
			{
				freeMoveFollowFloor = freeMoveFollowFloorToggleAlt.isOn;
			});
		}
		SyncFreeMoveFollowFloor();
		if (disableAllNavigationToggle != null)
		{
			disableAllNavigationToggle.onValueChanged.AddListener(delegate
			{
				disableAllNavigation = disableAllNavigationToggle.isOn;
			});
		}
		SyncDisableAllNavigation();
		if (disableGrabNavigationToggle != null)
		{
			disableGrabNavigationToggle.onValueChanged.AddListener(delegate
			{
				disableGrabNavigation = disableGrabNavigationToggle.isOn;
			});
		}
		SyncDisableGrabNavigation();
		if (disableTeleportToggle != null)
		{
			disableTeleportToggle.onValueChanged.AddListener(delegate
			{
				disableTeleport = disableTeleportToggle.isOn;
			});
		}
		SyncDisableTeleport();
		if (disableTeleportDuringPossessToggle != null)
		{
			disableTeleportDuringPossessToggle.onValueChanged.AddListener(delegate
			{
				disableTeleportDuringPossess = disableTeleportDuringPossessToggle.isOn;
			});
		}
		SyncDisableTeleportDuringPossess();
		if (teleportAllowRotationToggle != null)
		{
			teleportAllowRotationToggle.onValueChanged.AddListener(delegate
			{
				teleportAllowRotation = teleportAllowRotationToggle.isOn;
			});
		}
		SyncTeleportAllowRotation();
		if (freeMoveMultiplierSlider != null)
		{
			freeMoveMultiplierSlider.onValueChanged.AddListener(delegate
			{
				freeMoveMultiplier = freeMoveMultiplierSlider.value;
			});
		}
		if (grabNavigationPositionMultiplierSlider != null)
		{
			grabNavigationPositionMultiplierSlider.onValueChanged.AddListener(delegate
			{
				grabNavigationPositionMultiplier = grabNavigationPositionMultiplierSlider.value;
			});
		}
		SyncGrabNavigationPositionMultiplier();
		if (grabNavigationRotationMultiplierSlider != null)
		{
			grabNavigationRotationMultiplierSlider.onValueChanged.AddListener(delegate
			{
				grabNavigationRotationMultiplier = grabNavigationRotationMultiplierSlider.value;
			});
		}
		SyncGrabNavigationRotationMultiplier();
		if (loadSceneButtons != null)
		{
			Transform[] array = loadSceneButtons;
			foreach (Transform transform in array)
			{
				if (transform != null)
				{
					transform.gameObject.SetActive(!loadSceneButtonDisabled);
				}
			}
		}
		if (onlineBrowseSceneButtons != null)
		{
			Transform[] array2 = onlineBrowseSceneButtons;
			foreach (Transform transform2 in array2)
			{
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(!browseDisabled);
				}
			}
		}
		if (saveSceneButtons != null)
		{
			Transform[] array3 = saveSceneButtons;
			foreach (Transform transform3 in array3)
			{
				if (transform3 != null)
				{
					transform3.gameObject.SetActive(!saveSceneButtonDisabled);
				}
			}
		}
		if (addAtomUI != null)
		{
			addAtomUI.gameObject.SetActive(!advancedSceneEditDisabled);
		}
		if (addAtomUIAlt != null)
		{
			addAtomUIAlt.gameObject.SetActive(!advancedSceneEditDisabled);
		}
		if (animationUI != null)
		{
			animationUI.gameObject.SetActive(!advancedSceneEditDisabled);
		}
		if (audioUI != null)
		{
			audioUI.gameObject.SetActive(!advancedSceneEditDisabled);
		}
		if (showNavigationHologridToggle != null)
		{
			showNavigationHologridToggle.isOn = _showNavigationHologrid;
			showNavigationHologridToggle.onValueChanged.AddListener(delegate
			{
				showNavigationHologrid = showNavigationHologridToggle.isOn;
			});
		}
		SyncHologridTransparency();
		if (hologridTransparencySlider != null)
		{
			hologridTransparencySlider.value = _hologridTransparency;
			hologridTransparencySlider.onValueChanged.AddListener(delegate
			{
				hologridTransparency = hologridTransparencySlider.value;
			});
			SliderControl component9 = hologridTransparencySlider.GetComponent<SliderControl>();
			if (component9 != null)
			{
				component9.defaultValue = 0.01f;
			}
		}
		if (oculusThumbstickFunctionPopup != null)
		{
			oculusThumbstickFunctionPopup.currentValue = _oculusThumbstickFunction.ToString();
			UIPopup uIPopup = oculusThumbstickFunctionPopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetOculusThumbstickFunctionFromString));
		}
		if (allowPossessSpringAdjustmentToggle != null)
		{
			allowPossessSpringAdjustmentToggle.isOn = _allowPossessSpringAdjustment;
			allowPossessSpringAdjustmentToggle.onValueChanged.AddListener(delegate
			{
				allowPossessSpringAdjustment = allowPossessSpringAdjustmentToggle.isOn;
			});
		}
		if (possessPositionSpringSlider != null)
		{
			possessPositionSpringSlider.value = _possessPositionSpring;
			possessPositionSpringSlider.onValueChanged.AddListener(delegate
			{
				possessPositionSpring = possessPositionSpringSlider.value;
			});
			SliderControl component10 = possessPositionSpringSlider.GetComponent<SliderControl>();
			if (component10 != null)
			{
				component10.defaultValue = 10000f;
			}
		}
		if (possessRotationSpringSlider != null)
		{
			possessRotationSpringSlider.value = _possessRotationSpring;
			possessRotationSpringSlider.onValueChanged.AddListener(delegate
			{
				possessRotationSpring = possessRotationSpringSlider.value;
			});
			SliderControl component11 = possessRotationSpringSlider.GetComponent<SliderControl>();
			if (component11 != null)
			{
				component11.defaultValue = 1000f;
			}
		}
		if (navigationHologrid != null)
		{
			navigationHologrid.gameObject.SetActive(value: false);
		}
		if (showMainHUDOnStart && !UIDisabled)
		{
			ShowMainHUDAuto();
		}
		else
		{
			HideMainHUD();
		}
		helpText = string.Empty;
		helpColor = Color.white;
		if (loResScreenShotCameraFOVSlider != null)
		{
			loResScreenShotCameraFOVSlider.value = _loResScreenShotCameraFOV;
			loResScreenShotCameraFOVSlider.onValueChanged.AddListener(delegate
			{
				loResScreenShotCameraFOV = loResScreenShotCameraFOVSlider.value;
			});
			SliderControl component12 = loResScreenShotCameraFOVSlider.GetComponent<SliderControl>();
			if (component12 != null)
			{
				component12.defaultValue = 40f;
			}
		}
		if (hiResScreenShotCameraFOVSlider != null)
		{
			hiResScreenShotCameraFOVSlider.value = _hiResScreenShotCameraFOV;
			hiResScreenShotCameraFOVSlider.onValueChanged.AddListener(delegate
			{
				hiResScreenShotCameraFOV = hiResScreenShotCameraFOVSlider.value;
			});
			SliderControl component13 = hiResScreenShotCameraFOVSlider.GetComponent<SliderControl>();
			if (component13 != null)
			{
				component13.defaultValue = 40f;
			}
		}
		if (selectAtomPopup != null)
		{
			selectAtomPopup.currentValue = "None";
			UIPopup uIPopup2 = selectAtomPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SyncControllerPopup));
		}
		if (selectControllerPopup != null)
		{
			selectControllerPopup.currentValue = "None";
			UIPopup uIPopup3 = selectControllerPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SelectFreeController));
		}
		if (alignRotationOffsetPopup != null)
		{
			alignRotationOffsetPopup.currentValue = _alignRotationOffset.ToString();
			UIPopup uIPopup4 = alignRotationOffsetPopup;
			uIPopup4.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup4.onValueChangeHandlers, new UIPopup.OnValueChange(SetAlignRotationOffset));
		}
		if (freezeAnimationToggle != null)
		{
			freezeAnimationToggle.onValueChanged.AddListener(SetFreezeAnimation);
		}
		if (useSceneLoadPositionToggle != null)
		{
			useSceneLoadPositionToggle.isOn = _useSceneLoadPosition;
			useSceneLoadPositionToggle.onValueChanged.AddListener(SetUseSceneLoadPosition);
		}
		if (showHiddenAtomsToggle != null)
		{
			showHiddenAtomsToggle.isOn = _showHiddenAtoms;
			showHiddenAtomsToggle.onValueChanged.AddListener(delegate(bool b)
			{
				showHiddenAtoms = b;
			});
		}
		if (showHiddenAtomsToggleAlt != null)
		{
			showHiddenAtomsToggleAlt.isOn = _showHiddenAtoms;
			showHiddenAtomsToggleAlt.onValueChanged.AddListener(delegate(bool b)
			{
				showHiddenAtoms = b;
			});
		}
		if (allowGrabPlusTriggerHandToggleToggle != null)
		{
			allowGrabPlusTriggerHandToggleToggle.isOn = _allowGrabPlusTriggerHandToggle;
			allowGrabPlusTriggerHandToggleToggle.onValueChanged.AddListener(delegate(bool b)
			{
				allowGrabPlusTriggerHandToggle = b;
			});
		}
		if (alwaysUseAlternateHandsToggle != null)
		{
			alwaysUseAlternateHandsToggle.isOn = _alwaysUseAlternateHands;
			alwaysUseAlternateHandsToggle.onValueChanged.AddListener(delegate(bool b)
			{
				alwaysUseAlternateHands = b;
			});
		}
	}

	private void SyncHelpOverlay()
	{
		if (helpToggle != null)
		{
			helpToggle.isOn = _helpOverlayOn;
		}
		if (helpToggleAlt != null)
		{
			helpToggleAlt.isOn = _helpOverlayOn;
		}
		if (_helpOverlayOn && _helpOverlayOnAux)
		{
			if (isOVR)
			{
				if (helpOverlayOVR != null)
				{
					helpOverlayOVR.gameObject.SetActive(value: true);
				}
				if (helpOverlayVive != null)
				{
					helpOverlayVive.gameObject.SetActive(value: false);
				}
			}
			else if (isOpenVR)
			{
				if (helpOverlayOVR != null)
				{
					helpOverlayOVR.gameObject.SetActive(value: false);
				}
				if (helpOverlayVive != null)
				{
					helpOverlayVive.gameObject.SetActive(value: true);
				}
			}
			else
			{
				if (helpOverlayOVR != null)
				{
					helpOverlayOVR.gameObject.SetActive(value: false);
				}
				if (helpOverlayVive != null)
				{
					helpOverlayVive.gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			if (helpOverlayOVR != null)
			{
				helpOverlayOVR.gameObject.SetActive(value: false);
			}
			if (helpOverlayVive != null)
			{
				helpOverlayVive.gameObject.SetActive(value: false);
			}
		}
	}

	protected void SyncHelpText()
	{
		if (helpHUDText != null)
		{
			if (tempHelpText != null)
			{
				helpHUDText.text = tempHelpText;
				helpHUDText.color = Color.white;
			}
			else
			{
				helpHUDText.text = _helpText;
				helpHUDText.color = _helpColor;
			}
		}
	}

	public void ShowTempHelp(string text)
	{
		tempHelpText = text;
		SyncHelpText();
	}

	public void HideTempHelp()
	{
		tempHelpText = null;
		SyncHelpText();
	}

	protected void SyncActiveHands()
	{
		if (_alwaysUseAlternateHands)
		{
			if (leftHandAlternate != null)
			{
				leftHandAlternate.gameObject.SetActive(!IsMonitorOnly);
			}
			if (leftHand != null)
			{
				leftHand.gameObject.SetActive(_leapHandLeftConnected);
			}
			if (rightHandAlternate != null)
			{
				rightHandAlternate.gameObject.SetActive(!IsMonitorOnly);
			}
			if (rightHand != null)
			{
				rightHand.gameObject.SetActive(_leapHandRightConnected);
			}
		}
		else
		{
			if (leftHandAlternate != null)
			{
				leftHandAlternate.gameObject.SetActive(_leapHandLeftConnected);
			}
			if (leftHand != null)
			{
				leftHand.gameObject.SetActive(!IsMonitorOnly);
			}
			if (rightHandAlternate != null)
			{
				rightHandAlternate.gameObject.SetActive(_leapHandRightConnected);
			}
			if (rightHand != null)
			{
				rightHand.gameObject.SetActive(!IsMonitorOnly);
			}
		}
	}

	public void SelectController(FreeControllerV3 controller, bool alignView = false)
	{
		if (selectedController != controller)
		{
			ClearSelection(syncSelectedUI: false);
			selectedController = controller;
			selectedController.selected = true;
			AddPositionRotationHandlesToSelectedController();
			if (selectedControllerNameDisplay != null)
			{
				selectedControllerNameDisplay.text = selectedController.containingAtom.uid + ":" + selectedController.name;
			}
			activeUI = ActiveUI.SelectedOptions;
			if (alignView)
			{
				FocusOnSelectedController();
			}
		}
	}

	public void SelectController(string atomName, string controllerName, bool alignView = false)
	{
		FreeControllerV3 freeControllerV = FreeControllerNameToFreeController(atomName + ":" + controllerName);
		if (freeControllerV != null)
		{
			SelectController(freeControllerV, alignView);
		}
	}

	private void SelectFreeController(string cv)
	{
		string currentValue = selectAtomPopup.currentValue;
		if (currentValue != "None")
		{
			string currentValue2 = selectControllerPopup.currentValue;
			if (currentValue2 != "None")
			{
				bool alignView = false;
				if (quickSelectMoveAndAlignToggle != null && quickSelectMoveAndAlignToggle.isOn)
				{
					alignView = true;
				}
				SelectController(currentValue, currentValue2, alignView);
			}
			else
			{
				ClearSelection();
			}
		}
		else
		{
			ClearSelection();
		}
	}

	public Atom GetSelectedAtom()
	{
		if (selectedController != null)
		{
			return selectedController.containingAtom;
		}
		return null;
	}

	public FreeControllerV3 GetSelectedController()
	{
		return selectedController;
	}

	public void ClearSelection(bool syncSelectedUI = true)
	{
		if (selectedController != null)
		{
			selectedController.selected = false;
			selectedController.hidden = true;
			selectedController.guihidden = true;
			if (selectedControllerPositionHandle != null)
			{
				selectedControllerPositionHandle.controller = null;
				selectedControllerPositionHandle.enabled = false;
			}
			if (selectedControllerRotationHandle != null)
			{
				selectedControllerRotationHandle.controller = null;
				selectedControllerRotationHandle.enabled = false;
			}
			selectedController = null;
			if (syncSelectedUI)
			{
				SyncUIToSelectedController();
			}
		}
		if (LookInputModule.singleton != null)
		{
			LookInputModule.singleton.ClearSelection();
		}
		SyncVisibility();
	}

	public void ToggleRotationMode()
	{
		if (selectedController != null)
		{
			selectedController.NextControlMode();
		}
	}

	public void SetOnlyShowControllers(HashSet<FreeControllerV3> onlyControllers)
	{
		onlyShowControllers = onlyControllers;
		SyncVisibility();
	}

	private void ClearSelectionHUDs()
	{
		if (selectionHUD != null)
		{
			selectionHUD.gameObject.SetActive(value: false);
			selectionHUD.ClearSelections();
		}
		if (rightSelectionHUD != null)
		{
			rightSelectionHUD.ClearSelections();
		}
		if (leftSelectionHUD != null)
		{
			leftSelectionHUD.ClearSelections();
		}
	}

	private void SyncVisibility()
	{
		switch (selectMode)
		{
			case SelectMode.Targets:
				if (onlyShowControllers != null)
				{
					foreach (FreeControllerV3 allController in allControllers)
					{
						if (onlyShowControllers.Contains(allController))
						{
							allController.hidden = false;
						}
						else
						{
							allController.hidden = true;
						}
					}
					break;
				}
				foreach (FreeControllerV3 allController2 in allControllers)
				{
					if (gameMode == GameMode.Edit && (_mainHUDVisible || !UserPreferences.singleton.showTargetsMenuOnly))
					{
						if (_showHiddenAtoms || allController2.containingAtom == null || !allController2.containingAtom.hidden)
						{
							allController2.hidden = false;
						}
						else
						{
							allController2.hidden = true;
						}
					}
					else
					{
						allController2.hidden = true;
					}
				}
				if (selectedControllerPositionHandle != null && selectedControllerPositionHandle.controller != null)
				{
					selectedControllerPositionHandle.enabled = _mainHUDVisible;
				}
				if (selectedControllerRotationHandle != null && selectedControllerRotationHandle.controller != null)
				{
					selectedControllerRotationHandle.enabled = _mainHUDVisible;
				}
				break;
			case SelectMode.FilteredTargets:
				foreach (FreeControllerV3 allController3 in allControllers)
				{
					if (selectedController == null || selectedController != allController3)
					{
						allController3.hidden = true;
					}
					else if (gameMode == GameMode.Edit || allController3.interactableInPlayMode)
					{
						allController3.hidden = false;
					}
					else
					{
						allController3.hidden = true;
					}
				}
				if (selectedControllerPositionHandle != null && selectedControllerPositionHandle.controller != null)
				{
					selectedControllerPositionHandle.enabled = _mainHUDVisible;
				}
				if (selectedControllerRotationHandle != null && selectedControllerRotationHandle.controller != null)
				{
					selectedControllerRotationHandle.enabled = _mainHUDVisible;
				}
				break;
			default:
				if (allControllers != null)
				{
					foreach (FreeControllerV3 allController4 in allControllers)
					{
						allController4.hidden = true;
					}
				}
				if (selectedControllerPositionHandle != null && selectedControllerPositionHandle.controller != null)
				{
					selectedControllerPositionHandle.enabled = _mainHUDVisible;
				}
				if (selectedControllerRotationHandle != null && selectedControllerRotationHandle.controller != null)
				{
					selectedControllerRotationHandle.enabled = _mainHUDVisible;
				}
				break;
		}
		Atom atom = null;
		if (selectedController != null)
		{
			atom = selectedController.containingAtom;
		}
		if (selectMode == SelectMode.FilteredTargets && atom != null)
		{
			FreeControllerV3[] freeControllers = atom.freeControllers;
			foreach (FreeControllerV3 freeControllerV in freeControllers)
			{
				if (gameMode == GameMode.Edit || freeControllerV.interactableInPlayMode)
				{
					freeControllerV.hidden = false;
				}
				else
				{
					freeControllerV.hidden = true;
				}
			}
		}
		if (fpMap != null)
		{
			foreach (ForceProducerV2 value in fpMap.Values)
			{
				value.drawLines = false;
				if (atom != null && atom == value.containingAtom && _mainHUDVisible && (selectMode == SelectMode.Targets || selectMode == SelectMode.FilteredTargets))
				{
					value.drawLines = true;
				}
			}
		}
		if (gpMap != null)
		{
			foreach (GrabPoint value2 in gpMap.Values)
			{
				value2.drawLines = false;
				if (atom != null && atom == value2.containingAtom && _mainHUDVisible && (selectMode == SelectMode.Targets || selectMode == SelectMode.FilteredTargets))
				{
					value2.drawLines = true;
				}
			}
		}
		if (allAnimationPatterns != null)
		{
			foreach (AnimationPattern allAnimationPattern in allAnimationPatterns)
			{
				allAnimationPattern.draw = selectMode == SelectMode.Targets && _mainHUDVisible && !allAnimationPattern.hideCurveUnlessSelected;
				allAnimationPattern.SetDrawColor(Color.red);
				if (!(atom != null) || !(atom == allAnimationPattern.containingAtom) || !_mainHUDVisible || (selectMode != SelectMode.Targets && selectMode != SelectMode.FilteredTargets))
				{
					continue;
				}
				allAnimationPattern.draw = true;
				allAnimationPattern.SetDrawColor(Color.blue);
				AnimationStep[] steps = allAnimationPattern.steps;
				foreach (AnimationStep animationStep in steps)
				{
					if (!(animationStep.containingAtom != null) || animationStep.containingAtom.freeControllers == null)
					{
						continue;
					}
					FreeControllerV3[] freeControllers2 = animationStep.containingAtom.freeControllers;
					foreach (FreeControllerV3 freeControllerV2 in freeControllers2)
					{
						if (gameMode == GameMode.Edit || freeControllerV2.interactableInPlayMode)
						{
							freeControllerV2.hidden = false;
						}
						else
						{
							freeControllerV2.hidden = true;
						}
					}
				}
			}
		}
		if (allAnimationSteps == null)
		{
			return;
		}
		foreach (AnimationStep allAnimationStep in allAnimationSteps)
		{
			if (!(allAnimationStep.animationParent != null) || !(atom != null) || !(atom == allAnimationStep.containingAtom) || !_mainHUDVisible || (selectMode != SelectMode.Targets && selectMode != SelectMode.FilteredTargets))
			{
				continue;
			}
			allAnimationStep.animationParent.draw = true;
			allAnimationStep.animationParent.SetDrawColor(Color.blue);
			if (allAnimationStep.animationParent.containingAtom != null && allAnimationStep.animationParent.containingAtom.freeControllers != null)
			{
				FreeControllerV3[] freeControllers3 = allAnimationStep.animationParent.containingAtom.freeControllers;
				foreach (FreeControllerV3 freeControllerV3 in freeControllers3)
				{
					if (gameMode == GameMode.Edit || freeControllerV3.interactableInPlayMode)
					{
						freeControllerV3.hidden = false;
					}
					else
					{
						freeControllerV3.hidden = true;
					}
				}
			}
			AnimationStep[] steps2 = allAnimationStep.animationParent.steps;
			foreach (AnimationStep animationStep2 in steps2)
			{
				if (!(animationStep2.containingAtom != null) || animationStep2.containingAtom.freeControllers == null)
				{
					continue;
				}
				FreeControllerV3[] freeControllers4 = animationStep2.containingAtom.freeControllers;
				foreach (FreeControllerV3 freeControllerV4 in freeControllers4)
				{
					if (gameMode == GameMode.Edit || freeControllerV4.interactableInPlayMode)
					{
						freeControllerV4.hidden = false;
					}
					else
					{
						freeControllerV4.hidden = true;
					}
				}
			}
		}
	}

	protected void SetSelectionHUDHeader(SelectionHUD sh, string txt)
	{
		if (sh != null && sh.headerText != null)
		{
			sh.headerText.text = txt;
		}
	}

	private void ResetSelectionInstances()
	{
		if (selectionInstances != null)
		{
			foreach (Transform selectionInstance in selectionInstances)
			{
				if (selectionInstance != null)
				{
					UnityEngine.Object.Destroy(selectionInstance.gameObject);
				}
			}
		}
		if (selectionInstances == null)
		{
			selectionInstances = new List<Transform>();
		}
		else
		{
			selectionInstances.Clear();
		}
		highlightedSelectTargetsLook = null;
		highlightedSelectTargetsLeft = null;
		highlightedSelectTargetsRight = null;
		highlightedSelectTargetsMouse = null;
	}

	private void SelectModeCommon(string helpT, string selectionText = "", bool setSelectHUDActive = true, bool resetSelectionInstances = true, bool clearSelection = true, bool clearSelectionHUDs = true)
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		helpText = helpT;
		if (resetSelectionInstances)
		{
			ResetSelectionInstances();
		}
		if (clearSelection)
		{
			ClearSelection();
		}
		if (clearSelectionHUDs)
		{
			ClearSelectionHUDs();
		}
		if (setSelectHUDActive)
		{
			SetSelectionHUDHeader(selectionHUD, selectionText);
			if (selectionHUD != null)
			{
				selectionHUD.gameObject.SetActive(value: true);
			}
		}
		if (hiResScreenshotPreview != null)
		{
			hiResScreenshotPreview.gameObject.SetActive(value: false);
		}
		if (hiResScreenshotCamera != null)
		{
			hiResScreenshotCamera.enabled = false;
		}
		if (selectMode == SelectMode.SaveScreenshot)
		{
			ProcessSaveScreenshot(force: true);
		}
	}

	public void SelectModeControllers(SelectControllerCallback scc)
	{
		if (selectMode != SelectMode.Controller)
		{
			SelectModeCommon("Press Select to select Controller. Press Remote Grab to cancel.", "Select Controller");
			selectMode = SelectMode.Controller;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
					SelectTarget component = transform.GetComponent<SelectTarget>();
					if (component != null)
					{
						component.selectionName = allController.containingAtom.uid + ":" + allController.name;
					}
					transform.parent = allController.transform;
					transform.position = allController.transform.position;
					selectionInstances.Add(transform);
				}
			}
			selectControllerCallback = scc;
		}
		SyncVisibility();
	}

	public void SelectModeForceProducers(SelectForceProducerCallback sfpc)
	{
		if (selectMode != SelectMode.ForceProducer)
		{
			SelectModeCommon("Press Select to select Force Producer. Press Remote Grab to cancel.", "Select Force Producer");
			selectMode = SelectMode.ForceProducer;
			if (selectPrefab != null)
			{
				foreach (string key in fpMap.Keys)
				{
					ForceProducerV2 forceProducerV = ProducerNameToForceProducer(key);
					if (forceProducerV != null)
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component = transform.GetComponent<SelectTarget>();
						if (component != null)
						{
							component.selectionName = key;
						}
						transform.parent = forceProducerV.transform;
						transform.position = forceProducerV.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
			selectForceProducerCallback = sfpc;
		}
		SyncVisibility();
	}

	public void SelectModeForceReceivers(SelectForceReceiverCallback sfrc)
	{
		if (selectMode != SelectMode.ForceReceiver)
		{
			SelectModeCommon("Press Select to select Force Receiver. Press Remote Grab to cancel.", "Select Force Receiver");
			selectMode = SelectMode.ForceReceiver;
			if (selectPrefab != null)
			{
				foreach (string key in frMap.Keys)
				{
					ForceReceiver forceReceiver = ReceiverNameToForceReceiver(key);
					if (forceReceiver != null && !forceReceiver.skipUIDrawing)
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component = transform.GetComponent<SelectTarget>();
						if (component != null)
						{
							component.selectionName = key;
						}
						transform.parent = forceReceiver.transform;
						transform.position = forceReceiver.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
			selectForceReceiverCallback = sfrc;
		}
		SyncVisibility();
	}

	public void SelectModeRigidbody(SelectRigidbodyCallback srbc)
	{
		if (selectMode != SelectMode.Rigidbody)
		{
			SelectModeCommon("Press Select to select Physics Object. Press Remote Grab to cancel.", "Select Physics Object");
			selectMode = SelectMode.Rigidbody;
			if (selectPrefab != null)
			{
				foreach (string key in rbMap.Keys)
				{
					Rigidbody rigidbody = RigidbodyNameToRigidbody(key);
					if (!(rigidbody != null))
					{
						continue;
					}
					ForceReceiver component = rigidbody.GetComponent<ForceReceiver>();
					if (component == null || !component.skipUIDrawing)
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component2 = transform.GetComponent<SelectTarget>();
						if (component2 != null)
						{
							component2.selectionName = key;
						}
						transform.parent = rigidbody.transform;
						transform.position = rigidbody.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
			selectRigidbodyCallback = srbc;
		}
		SyncVisibility();
	}

	public void SelectModeAtom(SelectAtomCallback sac)
	{
		if (selectMode != SelectMode.Atom)
		{
			SelectModeCommon("Press Select to select Atom. Press either Remote Grab to cancel.", "Select Atom");
			selectMode = SelectMode.Atom;
			if (selectPrefab != null)
			{
				foreach (string atomUID in atomUIDs)
				{
					Atom atomByUid = GetAtomByUid(atomUID);
					if (atomByUid != null)
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component = transform.GetComponent<SelectTarget>();
						if (component != null)
						{
							component.selectionName = atomUID;
						}
						if (atomByUid.childAtomContainer != null)
						{
							transform.parent = atomByUid.childAtomContainer;
							transform.position = atomByUid.childAtomContainer.position;
						}
						else
						{
							transform.parent = atomByUid.transform;
							transform.position = atomByUid.transform.position;
						}
						selectionInstances.Add(transform);
					}
				}
			}
			selectAtomCallback = sac;
		}
		SyncVisibility();
	}

	public void SelectModePossess(bool excludeHeadClear = false)
	{
		if (selectMode != SelectMode.Possess)
		{
			SelectModeCommon("Move Controllers or Head into spheres to possess. Press Select when complete, or press Remote Grab to cancel possess mode.", string.Empty, setSelectHUDActive: false);
			ClearPossess(excludeHeadClear);
			selectMode = SelectMode.Possess;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (allController.possessable && (allController.canGrabPosition || allController.canGrabRotation))
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component = transform.GetComponent<SelectTarget>();
						if (component != null)
						{
							component.selectionName = allController.containingAtom.uid + ":" + allController.name;
						}
						transform.parent = allController.transform;
						transform.position = allController.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
		}
		SyncVisibility();
	}

	public void SelectModeTwoStagePossess()
	{
		if (selectMode != SelectMode.TwoStagePossess)
		{
			SelectModeCommon("Move Controllers or Head into spheres to select nodes to be possessed. Once all controls are selected, align motion controllers to desired possess from point. Press Select to lock in possess, or press Remote Grab to cancel.", string.Empty, setSelectHUDActive: false);
			ClearPossess();
			selectMode = SelectMode.TwoStagePossess;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (allController.possessable && (allController.canGrabPosition || allController.canGrabRotation))
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component = transform.GetComponent<SelectTarget>();
						if (component != null)
						{
							component.selectionName = allController.containingAtom.uid + ":" + allController.name;
						}
						transform.parent = allController.transform;
						transform.position = allController.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
		}
		SyncVisibility();
	}

	public void SelectModeTwoStagePossessNoClear()
	{
		if (selectMode != SelectMode.TwoStagePossess)
		{
			SelectModeCommon("Move Controllers or Head into spheres to select nodes to be possessed. Once all controls are selected, align motion controllers to desired possess from point. Press Select to lock in possess, or press Remote Grab to cancel.", string.Empty, setSelectHUDActive: false);
			selectMode = SelectMode.TwoStagePossess;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (allController.possessable && (allController.canGrabPosition || allController.canGrabRotation))
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component = transform.GetComponent<SelectTarget>();
						if (component != null)
						{
							component.selectionName = allController.containingAtom.uid + ":" + allController.name;
						}
						transform.parent = allController.transform;
						transform.position = allController.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
		}
		SyncVisibility();
	}

	public void SelectModeUnpossess()
	{
		if (selectMode == SelectMode.Unpossess)
		{
			return;
		}
		SelectModeCommon("Point at controller you would like to unpossess and press Select to unpossess. Press Remote Grab to when finished.", string.Empty);
		selectMode = SelectMode.Unpossess;
		if (!(selectPrefab != null))
		{
			return;
		}
		foreach (FreeControllerV3 allController in allControllers)
		{
			if (allController.possessed)
			{
				Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
				SelectTarget component = transform.GetComponent<SelectTarget>();
				if (component != null)
				{
					component.selectionName = allController.containingAtom.uid + ":" + allController.name;
				}
				transform.parent = allController.transform;
				transform.position = allController.transform.position;
				selectionInstances.Add(transform);
			}
		}
	}

	public void SelectModePossessAndAlign()
	{
		if (selectMode != SelectMode.PossessAndAlign)
		{
			SelectModeCommon("Press Select to select which controller to move head into, align to, and possess.", string.Empty, setSelectHUDActive: false);
			ClearPossess();
			selectMode = SelectMode.PossessAndAlign;
			if (selectPrefab != null)
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (allController.possessable && (allController.canGrabPosition || allController.canGrabRotation))
					{
						Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
						SelectTarget component = transform.GetComponent<SelectTarget>();
						if (component != null)
						{
							component.selectionName = allController.containingAtom.uid + ":" + allController.name;
						}
						transform.parent = allController.transform;
						transform.position = allController.transform.position;
						selectionInstances.Add(transform);
					}
				}
			}
		}
		SyncVisibility();
	}

	public void SelectModeAnimationRecord()
	{
		if (selectMode != SelectMode.AnimationRecord)
		{
			SelectModeCommon("Press Select or Spacebar to start recording", string.Empty, setSelectHUDActive: false);
			selectMode = SelectMode.AnimationRecord;
		}
		SyncVisibility();
	}

	protected bool CheckIfControllerLinkedToMotionControl(Transform t, FreeControllerV3 fc)
	{
		Rigidbody component = t.GetComponent<Rigidbody>();
		if (component != null && fc.linkToRB == component)
		{
			return true;
		}
		return false;
	}

	public void SelectModeArmedForRecord()
	{
		if (selectMode == SelectMode.ArmedForRecord)
		{
			return;
		}
		SelectModeCommon("Press Select to toggle which controllers are armed for record. Green=Armed Red=Not Armed. Press Remote Grab when done.", string.Empty, setSelectHUDActive: false);
		selectMode = SelectMode.ArmedForRecord;
		if (!(selectPrefab != null))
		{
			return;
		}
		foreach (MotionAnimationControl value in macMap.Values)
		{
			Transform transform = UnityEngine.Object.Instantiate(selectPrefab);
			SelectTarget component = transform.GetComponent<SelectTarget>();
			if (component != null)
			{
				component.selectionName = value.containingAtom.uid + ":" + value.name;
				if (value.armedForRecord)
				{
					component.SetColor(Color.green);
				}
				else
				{
					component.SetColor(Color.red);
				}
			}
			transform.parent = value.transform;
			transform.position = value.transform.position;
			selectionInstances.Add(transform);
		}
	}

	public void SelectModeTeleport()
	{
		if (selectMode != SelectMode.Teleport)
		{
			SelectModeCommon("Aim controller and touch Select to choose where to teleport to. Press Select to teleport or press Remote Grab to cancel teleport mode.", string.Empty, setSelectHUDActive: false, resetSelectionInstances: true, clearSelection: false);
			ClearPossess();
			selectMode = SelectMode.Teleport;
		}
		SyncVisibility();
	}

	public void SelectModeFreeMove()
	{
		if (selectMode != SelectMode.FreeMove)
		{
			string helpT;
			if (isOVR)
			{
				helpT = "Use left and right thumbsticks to move freely. Press Remote Grab to cancel free-move mode.";
			}
			else if (isOpenVR)
			{
				string localizedOrigin = freeModeMoveAction.GetLocalizedOrigin(SteamVR_Input_Sources.LeftHand);
				string localizedOrigin2 = freeModeMoveAction.GetLocalizedOrigin(SteamVR_Input_Sources.RightHand);
				helpT = "Use " + localizedOrigin + " and " + localizedOrigin2 + " to move freely. Press Grab to cancel free-move mode";
			}
			else
			{
				helpT = string.Empty;
			}
			SelectModeCommon(helpT, string.Empty, setSelectHUDActive: false, resetSelectionInstances: true, clearSelection: false);
			selectMode = SelectMode.FreeMove;
		}
		SyncVisibility();
	}

	public void SelectModeFreeMoveMouse()
	{
		if (selectMode == SelectMode.FreeMoveMouse)
		{
			SelectModeOff();
		}
		else if (selectMode != SelectMode.FreeMoveMouse)
		{
			SelectModeCommon(string.Empty, string.Empty, setSelectHUDActive: false, resetSelectionInstances: true, clearSelection: false);
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			selectMode = SelectMode.FreeMoveMouse;
		}
		SyncVisibility();
	}

	public void SelectModeScreenshot()
	{
		if (selectMode != SelectMode.Screenshot)
		{
			SelectModeCommon("Look where you want to take a screenshot and press Select. Press Remote Grab to cancel screenshot mode.", string.Empty, setSelectHUDActive: false, resetSelectionInstances: true, clearSelection: false);
			ClearPossess();
			selectMode = SelectMode.Screenshot;
			HideMainHUD();
			if (hiResScreenshotCamera != null)
			{
				hiResScreenshotCamera.enabled = true;
				if (hiResScreenShotCameraFOVSlider != null)
				{
					hiResScreenshotCamera.fieldOfView = hiResScreenShotCameraFOVSlider.value;
				}
				else
				{
					hiResScreenshotCamera.fieldOfView = 40f;
				}
			}
			if (hiResScreenshotPreview != null)
			{
				hiResScreenshotPreview.gameObject.SetActive(value: true);
			}
		}
		SyncVisibility();
	}

	public void SelectModeCustom(string helpText)
	{
		if (selectMode != SelectMode.Custom)
		{
			SelectModeCommon(helpText, string.Empty, setSelectHUDActive: false, resetSelectionInstances: true, clearSelection: false);
			selectMode = SelectMode.Custom;
		}
		SyncVisibility();
	}

	public void SelectModeCustomWithTargetControl(string helpText)
	{
		if (selectMode != SelectMode.CustomWithTargetControl)
		{
			SelectModeCommon(helpText, string.Empty, setSelectHUDActive: false, resetSelectionInstances: true, clearSelection: false);
			selectMode = SelectMode.CustomWithTargetControl;
		}
		SyncVisibility();
	}

	public void SelectModeCustomWithVRTargetControl(string helpText)
	{
		if (selectMode != SelectMode.CustomWithVRTargetControl)
		{
			SelectModeCommon(helpText, string.Empty, setSelectHUDActive: false, resetSelectionInstances: true, clearSelection: false);
			selectMode = SelectMode.CustomWithVRTargetControl;
		}
		SyncVisibility();
	}

	public void SelectModeOff()
	{
		if (selectMode != 0)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			ResetSelectionInstances();
			selectMode = SelectMode.Off;
			SetSelectionHUDHeader(selectionHUD, "Highlighted Controllers");
			selectControllerCallback = null;
			selectForceProducerCallback = null;
			selectForceReceiverCallback = null;
			selectRigidbodyCallback = null;
			selectAtomCallback = null;
			if (selectionHUD != null)
			{
				selectionHUD.gameObject.SetActive(value: false);
			}
			helpText = string.Empty;
			helpColor = Color.white;
			_pointerModeLeft = false;
			_pointerModeRight = false;
			if (screenshotPreview != null)
			{
				screenshotPreview.gameObject.SetActive(value: false);
			}
			if (screenshotCamera != null)
			{
				screenshotCamera.enabled = false;
			}
			if (hiResScreenshotPreview != null)
			{
				hiResScreenshotPreview.gameObject.SetActive(value: false);
			}
			if (hiResScreenshotCamera != null)
			{
				hiResScreenshotCamera.enabled = false;
			}
		}
		SyncVisibility();
	}

	private void SelectModeTargets()
	{
		if (selectMode != SelectMode.Targets)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			ResetSelectionInstances();
			selectMode = SelectMode.Targets;
			SetSelectionHUDHeader(selectionHUD, "Highlighted Controllers");
			selectControllerCallback = null;
			selectForceProducerCallback = null;
			selectForceReceiverCallback = null;
			selectRigidbodyCallback = null;
			selectAtomCallback = null;
			if (selectionHUD != null)
			{
				selectionHUD.gameObject.SetActive(value: false);
			}
			helpText = string.Empty;
			helpColor = Color.white;
			_pointerModeLeft = true;
			_pointerModeRight = true;
			if (hiResScreenshotPreview != null)
			{
				hiResScreenshotPreview.gameObject.SetActive(value: false);
			}
			if (hiResScreenshotCamera != null)
			{
				hiResScreenshotCamera.enabled = false;
			}
		}
		SyncVisibility();
	}

	private void SelectModeFiltered()
	{
		if (selectMode != SelectMode.FilteredTargets)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			ResetSelectionInstances();
			selectMode = SelectMode.FilteredTargets;
			SetSelectionHUDHeader(selectionHUD, "Highlighted Controllers");
			selectControllerCallback = null;
			selectForceProducerCallback = null;
			selectForceReceiverCallback = null;
			selectRigidbodyCallback = null;
			selectAtomCallback = null;
			if (selectionHUD != null)
			{
				selectionHUD.gameObject.SetActive(value: false);
			}
			if (hiResScreenshotPreview != null)
			{
				hiResScreenshotPreview.gameObject.SetActive(value: false);
			}
			if (hiResScreenshotCamera != null)
			{
				hiResScreenshotCamera.enabled = false;
			}
		}
		SyncVisibility();
	}

	public bool GetMenuShow()
	{
		if (isOVR)
		{
			return OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.Touch) || OVRInput.GetDown(OVRInput.Button.Four, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return menuAction.GetStateDown(SteamVR_Input_Sources.Any);
		}
		return false;
	}

	public bool GetMenuMoveLeft()
	{
		if (isOVR)
		{
			return OVRInput.Get(OVRInput.Button.Four, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return menuAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetMenuMoveRight()
	{
		if (isOVR)
		{
			return OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return menuAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetTeleportStart(bool inTeleportMode = false)
	{
		return GetTeleportStartLeft(inTeleportMode) || GetTeleportStartRight(inTeleportMode);
	}

	public bool GetTeleportStartLeft(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitLeft && OVRInput.GetDown(OVRInput.Touch.Three, OVRInput.Controller.Touch);
			}
			return !GUIhitLeft && OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitLeft)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportShowAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetTeleportStartRight(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitRight && OVRInput.GetDown(OVRInput.Touch.One, OVRInput.Controller.Touch);
			}
			return false;
		}
		if (isOpenVR)
		{
			if (GUIhitRight)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportShowAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetTeleportShow(bool inTeleportMode = false)
	{
		return GetTeleportShowLeft(inTeleportMode) || GetTeleportShowRight(inTeleportMode);
	}

	public bool GetTeleportShowLeft(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitLeft && OVRInput.Get(OVRInput.Touch.Three, OVRInput.Controller.Touch);
			}
			return !GUIhitLeft && OVRInput.Get(OVRInput.Button.Start, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitLeft)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportShowAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetTeleportShowRight(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitRight && OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.Touch);
			}
			return false;
		}
		if (isOpenVR)
		{
			if (GUIhitRight)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportShowAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetTeleportFinish(bool inTeleportMode = false)
	{
		return GetTeleportFinishLeft(inTeleportMode) || GetTeleportFinishRight(inTeleportMode);
	}

	public bool GetTeleportFinishLeft(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitLeft && OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.Touch);
			}
			return !GUIhitLeft && OVRInput.GetUp(OVRInput.Button.Start, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitLeft)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetTeleportFinishRight(bool inTeleportMode = false)
	{
		if (isOVR)
		{
			if (inTeleportMode)
			{
				return !GUIhitRight && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch);
			}
			return false;
		}
		if (isOpenVR)
		{
			if (GUIhitRight)
			{
				return false;
			}
			if (inTeleportMode)
			{
				return true;
			}
			return teleportAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetGrabNavigateStartLeft()
	{
		if (isOVR)
		{
			return oculusThumbstickFunction != ThumbstickFunction.SwapAxis && OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitLeft)
			{
				return false;
			}
			return grabNavigateAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetGrabNavigateStartRight()
	{
		if (isOVR)
		{
			return oculusThumbstickFunction != ThumbstickFunction.SwapAxis && OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			if (GUIhitRight)
			{
				return false;
			}
			return grabNavigateAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetGrabNavigateLeft()
	{
		if (isOVR)
		{
			return oculusThumbstickFunction != ThumbstickFunction.SwapAxis && OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabNavigateAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetGrabNavigateRight()
	{
		if (isOVR)
		{
			return oculusThumbstickFunction != ThumbstickFunction.SwapAxis && OVRInput.Get(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabNavigateAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	private void CheckSwapAxis()
	{
		if (isOVR)
		{
			if (oculusThumbstickFunction != 0 && (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch) || OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch)))
			{
				_swapAxis = !_swapAxis;
			}
		}
		else if (isOpenVR && swapFreeMoveAxis.GetStateDown(SteamVR_Input_Sources.Any))
		{
			_swapAxis = !_swapAxis;
		}
	}

	public Vector4 GetFreeNavigateVector(SteamVR_Action_Vector2 moveAction, bool ignoreDisable = false)
	{
		Vector4 result = default(Vector4);
		result.x = 0f;
		result.y = 0f;
		result.z = 0f;
		result.w = 0f;
		if (isOVR)
		{
			if (ignoreDisable || !UserPreferences.singleton.oculusDisableFreeMove)
			{
				JoystickControl.Axis axis = navigationForwardAxis;
				JoystickControl.Axis axis2 = navigationSideAxis;
				JoystickControl.Axis axis3 = navigationUpAxis;
				JoystickControl.Axis axis4 = navigationTurnAxis;
				if (_swapAxis)
				{
					axis = navigationUpAxis;
					axis2 = navigationTurnAxis;
					axis3 = navigationForwardAxis;
					axis4 = navigationSideAxis;
				}
				if (axis != 0)
				{
					if (invertNavigationForwardAxis)
					{
						result.y = 0f - JoystickControl.GetAxis(axis);
					}
					else
					{
						result.y = JoystickControl.GetAxis(axis);
					}
				}
				if (axis2 != 0)
				{
					if (invertNavigationSideAxis)
					{
						result.x = 0f - JoystickControl.GetAxis(axis2);
					}
					else
					{
						result.x = JoystickControl.GetAxis(axis2);
					}
				}
				if (axis3 != 0)
				{
					if (invertNavigationUpAxis)
					{
						result.w = 0f - JoystickControl.GetAxis(axis3);
					}
					else
					{
						result.w = JoystickControl.GetAxis(axis3);
					}
				}
				if (axis4 != 0)
				{
					if (invertNavigationTurnAxis)
					{
						result.z = 0f - JoystickControl.GetAxis(axis4);
					}
					else
					{
						result.z = JoystickControl.GetAxis(axis4);
					}
				}
			}
		}
		else if (isOpenVR)
		{
			Vector2 axis5 = moveAction.GetAxis(SteamVR_Input_Sources.LeftHand);
			Vector2 axis6 = moveAction.GetAxis(SteamVR_Input_Sources.RightHand);
			if (_swapAxis)
			{
				result.x = axis6.x;
				result.y = axis6.y;
				result.z = axis5.x;
				result.w = axis5.y;
			}
			else
			{
				result.x = axis5.x;
				result.y = axis5.y;
				result.z = axis6.x;
				result.w = axis6.y;
			}
		}
		return result;
	}

	private void HideLeftController()
	{
		if (isOVR && touchObjectLeft != null)
		{
			MeshRenderer[] componentsInChildren = touchObjectLeft.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			MeshRenderer[] array = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.enabled = false;
			}
		}
	}

	private void HideRightController()
	{
		if (isOVR && touchObjectRight != null)
		{
			MeshRenderer[] componentsInChildren = touchObjectRight.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			MeshRenderer[] array = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.enabled = false;
			}
		}
	}

	private void ShowLeftController()
	{
		if (isOVR && touchObjectLeft != null)
		{
			MeshRenderer[] componentsInChildren = touchObjectLeft.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			MeshRenderer[] array = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.enabled = true;
			}
		}
	}

	private void ShowRightController()
	{
		if (isOVR && touchObjectRight != null)
		{
			MeshRenderer[] componentsInChildren = touchObjectRight.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			MeshRenderer[] array = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.enabled = true;
			}
		}
	}

	private void ProcessGUIInteract()
	{
		if (isOVR)
		{
			bool down = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch);
			if (GUIhitRight && down)
			{
				rightGUIInteract = true;
			}
			bool down2 = OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.Touch);
			if (GUIhitLeft && down2)
			{
				leftGUIInteract = true;
			}
			if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.Touch) && rightGUIInteract)
			{
				rightGUIInteract = false;
			}
			if (OVRInput.GetUp(OVRInput.Button.Three, OVRInput.Controller.Touch) && leftGUIInteract)
			{
				leftGUIInteract = false;
			}
		}
		else if (isOpenVR)
		{
			bool stateDown = UIInteractAction.GetStateDown(SteamVR_Input_Sources.RightHand);
			if (GUIhitRight && stateDown)
			{
				rightGUIInteract = true;
			}
			bool stateDown2 = UIInteractAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
			if (GUIhitLeft && stateDown2)
			{
				leftGUIInteract = true;
			}
			if (UIInteractAction.GetStateUp(SteamVR_Input_Sources.RightHand) && rightGUIInteract)
			{
				rightGUIInteract = false;
			}
			if (UIInteractAction.GetStateUp(SteamVR_Input_Sources.LeftHand) && leftGUIInteract)
			{
				leftGUIInteract = false;
			}
		}
	}

	public bool GetTargetShow()
	{
		if (isOVR)
		{
			bool flag = !rightGUIInteract && OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.Touch);
			bool flag2 = !leftGUIInteract && OVRInput.Get(OVRInput.Touch.Three, OVRInput.Controller.Touch);
			return flag || flag2 || targetsOnWithButton;
		}
		if (isOpenVR)
		{
			if (!rightGUIInteract && targetShowAction.GetState(SteamVR_Input_Sources.RightHand))
			{
				return true;
			}
			if (!leftGUIInteract && targetShowAction.GetState(SteamVR_Input_Sources.LeftHand))
			{
				return true;
			}
		}
		return targetsOnWithButton;
	}

	public bool GetLeftUIPointerShow()
	{
		if (isOVR)
		{
			return OVRInput.Get(OVRInput.Touch.Three, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return targetShowAction.GetState(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightUIPointerShow()
	{
		if (isOVR)
		{
			return OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return targetShowAction.GetState(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public void SetLeftSelect()
	{
		_setLeftSelect = true;
	}

	public bool GetLeftSelect()
	{
		if (_setLeftSelect)
		{
			_setLeftSelect = false;
			return true;
		}
		if (leftGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			return OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return selectAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public void SetRightSelect()
	{
		_setRightSelect = true;
	}

	public bool GetRightSelect()
	{
		if (_setRightSelect)
		{
			_setRightSelect = false;
			return true;
		}
		if (rightGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return selectAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetMouseSelect()
	{
		if (GUIhitMouse || mouseClickUsed)
		{
			return false;
		}
		return Input.GetMouseButtonDown(0);
	}

	public bool GetMouseRelease()
	{
		return Input.GetMouseButtonUp(0);
	}

	public bool GetCancel()
	{
		return GetLeftCancel() || GetRightCancel() || Input.GetKeyDown(KeyCode.Escape);
	}

	public bool GetLeftCancel()
	{
		if (isOVR)
		{
			return GetLeftRemoteGrab();
		}
		if (isOpenVR)
		{
			return GetLeftRemoteGrab();
		}
		return false;
	}

	public bool GetRightCancel()
	{
		if (isOVR)
		{
			return GetRightRemoteGrab();
		}
		if (isOpenVR)
		{
			return GetRightRemoteGrab();
		}
		return false;
	}

	public float GetLeftGrabVal()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabValAction.GetAxis(SteamVR_Input_Sources.LeftHand);
		}
		return 0f;
	}

	public float GetRightGrabVal()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabValAction.GetAxis(SteamVR_Input_Sources.RightHand);
		}
		return 0f;
	}

	public bool GetLeftGrab()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightGrab()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public void DisableRemoteHoldGrab()
	{
		remoteHoldGrabDisabled = true;
	}

	public void EnableRemoteHoldGrab()
	{
		remoteHoldGrabDisabled = false;
	}

	public bool GetLeftRemoteGrab()
	{
		if (leftGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteGrabAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightRemoteGrab()
	{
		if (rightGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteGrabAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftGrabRelease()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightGrabRelease()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return grabAction.GetStateUp(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftRemoteGrabRelease()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteGrabAction.GetStateUp(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightRemoteGrabRelease()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteGrabAction.GetStateUp(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftHoldGrab()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return holdGrabAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightHoldGrab()
	{
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return holdGrabAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftRemoteHoldGrab()
	{
		if (leftGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteHoldGrabAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightRemoteHoldGrab()
	{
		if (rightGUIInteract)
		{
			return false;
		}
		if (isOVR)
		{
			if (UserPreferences.singleton.oculusSwapGrabAndTrigger)
			{
				return OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch);
			}
			return OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch);
		}
		if (isOpenVR)
		{
			return remoteHoldGrabAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	public bool GetLeftToggleHand()
	{
		if (isOVR)
		{
			return (_allowGrabPlusTriggerHandToggle && OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch) && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch)) || (_allowGrabPlusTriggerHandToggle && OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.Touch) && OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.Touch));
		}
		if (isOpenVR)
		{
			return toggleHandAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
		}
		return false;
	}

	public bool GetRightToggleHand()
	{
		if (isOVR)
		{
			return (_allowGrabPlusTriggerHandToggle && OVRInput.Get(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch) && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch)) || (_allowGrabPlusTriggerHandToggle && OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.Touch) && OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.Touch));
		}
		if (isOpenVR)
		{
			return toggleHandAction.GetStateDown(SteamVR_Input_Sources.RightHand);
		}
		return false;
	}

	private void ProcessLookAtTrigger()
	{
		if (_isLoading)
		{
			return;
		}
		castRay.origin = lookCamera.transform.position;
		castRay.direction = lookCamera.transform.forward;
		if (Physics.Raycast(castRay, out var hitInfo, 50f, lookAtTriggerMask))
		{
			LookAtTrigger component = hitInfo.collider.GetComponent<LookAtTrigger>();
			if (component != null)
			{
				if (currentLookAtTrigger != null)
				{
					if (currentLookAtTrigger != component)
					{
						currentLookAtTrigger.EndLookAt();
						currentLookAtTrigger = component;
						currentLookAtTrigger.StartLookAt();
					}
				}
				else
				{
					currentLookAtTrigger = component;
					currentLookAtTrigger.StartLookAt();
				}
			}
			else if (currentLookAtTrigger != null)
			{
				currentLookAtTrigger.EndLookAt();
				currentLookAtTrigger = null;
			}
		}
		else if (currentLookAtTrigger != null)
		{
			currentLookAtTrigger.EndLookAt();
			currentLookAtTrigger = null;
		}
	}

	private void UnhighlightControllers(List<FreeControllerV3> highlightList)
	{
		foreach (FreeControllerV3 highlight in highlightList)
		{
			highlight.highlighted = false;
		}
		highlightList.Clear();
	}

	private void InitTargets()
	{
		if (selectionHUD != null)
		{
			selectionHUD.gameObject.SetActive(value: false);
		}
		SyncVisibility();
	}

	private void PrepControllers()
	{
		foreach (FreeControllerV3 allController in allControllers)
		{
			allController.ResetAppliedForces();
		}
	}

	private void ProcessTargetSelectionDoRaycast(SelectionHUD sh, Ray ray, List<FreeControllerV3> hitsList, bool doHighlight = true, bool includeHidden = false, bool setSelectionHUDTransform = true)
	{
		AllocateRaycastHits();
		int num = Physics.RaycastNonAlloc(ray, raycastHits, 50f, targetColliderMask);
		if (num > 0)
		{
			if (wasHitFC == null)
			{
				wasHitFC = new Dictionary<FreeControllerV3, bool>();
			}
			else
			{
				wasHitFC.Clear();
			}
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = raycastHits[i];
				FreeControllerV3 freeControllerV = raycastHit.transform.GetComponent<FreeControllerV3>();
				if (freeControllerV == null)
				{
					FreeControllerV3Link component = raycastHit.transform.GetComponent<FreeControllerV3Link>();
					if (component != null)
					{
						freeControllerV = component.linkedController;
					}
				}
				if (freeControllerV != null && !wasHitFC.ContainsKey(freeControllerV) && (gameMode == GameMode.Edit || freeControllerV.interactableInPlayMode) && !freeControllerV.possessed && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV)) && (!freeControllerV.hidden || !(freeControllerV.containingAtom != null) || !freeControllerV.containingAtom.hidden))
				{
					wasHitFC.Add(freeControllerV, value: true);
					if (!hitsList.Contains(freeControllerV))
					{
						hitsList.Add(freeControllerV);
					}
				}
			}
			FreeControllerV3[] array = hitsList.ToArray();
			FreeControllerV3[] array2 = array;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				if (!wasHitFC.ContainsKey(freeControllerV2))
				{
					freeControllerV2.highlighted = false;
					hitsList.Remove(freeControllerV2);
				}
			}
			if (doHighlight)
			{
				for (int k = 0; k < hitsList.Count; k++)
				{
					FreeControllerV3 freeControllerV3 = hitsList[k];
					if (k == 0)
					{
						freeControllerV3.highlighted = true;
					}
					else
					{
						freeControllerV3.highlighted = false;
					}
				}
			}
			if (sh != null)
			{
				sh.ClearSelections();
				if (hitsList.Count > 0)
				{
					int num2 = 0;
					foreach (FreeControllerV3 hits in hitsList)
					{
						sh.SetSelection(name: (gameMode != GameMode.Play || !setSelectionHUDTransform) ? (hits.containingAtom.uid + ":" + hits.name) : string.Empty, index: num2, selection: hits.transform);
						num2++;
					}
				}
			}
		}
		else
		{
			if (doHighlight)
			{
				foreach (FreeControllerV3 hits2 in hitsList)
				{
					hits2.highlighted = false;
				}
			}
			hitsList.Clear();
		}
		if (!(sh != null))
		{
			return;
		}
		if (hitsList.Count > 0)
		{
			sh.gameObject.SetActive(value: true);
			if (setSelectionHUDTransform)
			{
				sh.transform.position = hitsList[0].transform.position;
				Vector3 localScale = default(Vector3);
				localScale.z = (localScale.y = (localScale.x = (sh.transform.position - lookCamera.transform.position).magnitude));
				sh.transform.localScale = localScale;
				sh.transform.LookAt(lookCamera.transform.position, lookCamera.transform.up);
			}
		}
		else
		{
			sh.gameObject.SetActive(value: false);
		}
	}

	private void AddPositionRotationHandlesToSelectedController()
	{
		if (MonitorCenterCamera != null)
		{
			if (selectedControllerPositionHandle != null)
			{
				selectedControllerPositionHandle.enabled = _mainHUDVisible;
				selectedControllerPositionHandle.controller = selectedController;
			}
			if (selectedControllerRotationHandle != null)
			{
				selectedControllerRotationHandle.enabled = _mainHUDVisible;
				selectedControllerRotationHandle.controller = selectedController;
			}
		}
	}

	private bool ProcessTargetSelectionDoSelect(List<FreeControllerV3> highlightedControllers)
	{
		bool result = false;
		if (highlightedControllers.Count > 0)
		{
			FreeControllerV3 freeControllerV = highlightedControllers[0];
			highlightedControllers.RemoveAt(0);
			highlightedControllers.Add(freeControllerV);
			if (!(selectedController != null) || !(selectedController == freeControllerV))
			{
				if (selectedController != null)
				{
					ClearSelection();
				}
				freeControllerV.selected = true;
				selectedController = freeControllerV;
				AddPositionRotationHandlesToSelectedController();
				SyncUIToSelectedController();
				activeUI = ActiveUI.SelectedOptions;
				result = true;
			}
		}
		else
		{
			ClearSelection();
		}
		return result;
	}

	private void ProcessTargetSelectionCycleSelect(List<FreeControllerV3> highlightedControllers)
	{
		if (highlightedControllers != null && highlightedControllers.Count > 1)
		{
			FreeControllerV3 item = highlightedControllers[0];
			highlightedControllers.RemoveAt(0);
			highlightedControllers.Add(item);
		}
	}

	private void ProcessTargetSelectionCycleBackwardsSelect(List<FreeControllerV3> highlightedControllers)
	{
		if (highlightedControllers != null && highlightedControllers.Count > 1)
		{
			int index = highlightedControllers.Count - 1;
			FreeControllerV3 item = highlightedControllers[index];
			highlightedControllers.RemoveAt(index);
			highlightedControllers.Insert(0, item);
		}
	}

	public List<FreeControllerV3> GetOverlappingTargets(Transform processFrom, float overlapRadius = 0.01f)
	{
		if (overlappingFcs == null)
		{
			overlappingFcs = new List<FreeControllerV3>();
		}
		else
		{
			overlappingFcs.Clear();
		}
		AllocateOverlappingControls();
		int num = Physics.OverlapSphereNonAlloc(processFrom.position, overlapRadius * _worldScale, overlappingControls, targetColliderMask);
		if (num > 0)
		{
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				Collider collider = overlappingControls[i];
				FreeControllerV3 component = collider.GetComponent<FreeControllerV3>();
				if (component != null && (gameMode == GameMode.Edit || component.interactableInPlayMode) && !component.possessed && (component.currentPositionState == FreeControllerV3.PositionState.On || component.currentRotationState == FreeControllerV3.RotationState.On) && (onlyShowControllers == null || onlyShowControllers.Contains(component)) && (!(component.containingAtom != null) || !component.containingAtom.hidden))
				{
					flag = true;
					if (!overlappingFcs.Contains(component))
					{
						overlappingFcs.Add(component);
					}
					float distanceHolder = Vector3.SqrMagnitude(processFrom.position - collider.transform.position);
					component.distanceHolder = distanceHolder;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < num; j++)
				{
					Collider collider2 = overlappingControls[j];
					FreeControllerV3 freeControllerV = null;
					FreeControllerV3Link component2 = collider2.GetComponent<FreeControllerV3Link>();
					if (component2 != null)
					{
						freeControllerV = component2.linkedController;
					}
					if (freeControllerV != null && (gameMode == GameMode.Edit || freeControllerV.interactableInPlayMode) && !freeControllerV.possessed && (freeControllerV.currentPositionState == FreeControllerV3.PositionState.On || freeControllerV.currentRotationState == FreeControllerV3.RotationState.On) && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV)) && (!(freeControllerV.containingAtom != null) || !freeControllerV.containingAtom.hidden))
					{
						if (!overlappingFcs.Contains(freeControllerV))
						{
							overlappingFcs.Add(freeControllerV);
						}
						float distanceHolder2 = Vector3.SqrMagnitude(processFrom.position - collider2.transform.position);
						freeControllerV.distanceHolder = distanceHolder2;
					}
				}
			}
			if (!flag)
			{
				for (int k = 0; k < num; k++)
				{
					Collider collider3 = overlappingControls[k];
					FreeControllerV3 component3 = collider3.GetComponent<FreeControllerV3>();
					if (component3 != null && (gameMode == GameMode.Edit || component3.interactableInPlayMode) && !component3.possessed && (onlyShowControllers == null || onlyShowControllers.Contains(component3)) && (!component3.hidden || !(component3.containingAtom != null) || !component3.containingAtom.hidden))
					{
						flag = true;
						if (!overlappingFcs.Contains(component3))
						{
							overlappingFcs.Add(component3);
						}
						float distanceHolder3 = Vector3.SqrMagnitude(processFrom.position - collider3.transform.position);
						component3.distanceHolder = distanceHolder3;
					}
				}
			}
			if (!flag)
			{
				for (int l = 0; l < num; l++)
				{
					Collider collider4 = overlappingControls[l];
					FreeControllerV3 freeControllerV2 = null;
					FreeControllerV3Link component4 = collider4.GetComponent<FreeControllerV3Link>();
					if (component4 != null)
					{
						freeControllerV2 = component4.linkedController;
					}
					if (freeControllerV2 != null && (gameMode == GameMode.Edit || freeControllerV2.interactableInPlayMode) && !freeControllerV2.possessed && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV2)) && (!freeControllerV2.hidden || !(freeControllerV2.containingAtom != null) || !freeControllerV2.containingAtom.hidden))
					{
						if (!overlappingFcs.Contains(freeControllerV2))
						{
							overlappingFcs.Add(freeControllerV2);
						}
						float distanceHolder4 = Vector3.SqrMagnitude(processFrom.position - collider4.transform.position);
						freeControllerV2.distanceHolder = distanceHolder4;
					}
				}
			}
		}
		return overlappingFcs;
	}

	protected void AllocateOverlappingControls()
	{
		if (overlappingControls == null)
		{
			overlappingControls = new Collider[256];
		}
	}

	public bool ProcessControllerTargetHighlight(SelectionHUD sh, Transform processFromPointer, Transform processFromOverlap, bool ptrMode, List<FreeControllerV3> highlightedControllers, bool uihit, FreeControllerV3 excludeController, out bool isOverlap, float overlapRadius = 0.03f)
	{
		if (overlappingFcs == null)
		{
			overlappingFcs = new List<FreeControllerV3>();
		}
		else
		{
			overlappingFcs.Clear();
		}
		bool result = false;
		AllocateOverlappingControls();
		int num = Physics.OverlapSphereNonAlloc(processFromOverlap.position, overlapRadius * _worldScale, overlappingControls, targetColliderMask);
		if (num > 0)
		{
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				Collider collider = overlappingControls[i];
				FreeControllerV3 component = collider.GetComponent<FreeControllerV3>();
				if (component != null && (gameMode == GameMode.Edit || component.interactableInPlayMode) && !component.possessed && component.hidden && (component.currentPositionState == FreeControllerV3.PositionState.On || component.currentRotationState == FreeControllerV3.RotationState.On) && (!(excludeController != null) || !(component == excludeController)) && (onlyShowControllers == null || onlyShowControllers.Contains(component)) && (!(component.containingAtom != null) || !component.containingAtom.hidden))
				{
					flag = true;
					if (!overlappingFcs.Contains(component))
					{
						overlappingFcs.Add(component);
					}
					float distanceHolder = Vector3.SqrMagnitude(processFromOverlap.position - collider.transform.position);
					component.distanceHolder = distanceHolder;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < num; j++)
				{
					Collider collider2 = overlappingControls[j];
					FreeControllerV3 freeControllerV = null;
					FreeControllerV3Link component2 = collider2.GetComponent<FreeControllerV3Link>();
					if (component2 != null)
					{
						freeControllerV = component2.linkedController;
					}
					if (freeControllerV != null && (gameMode == GameMode.Edit || freeControllerV.interactableInPlayMode) && !freeControllerV.possessed && freeControllerV.hidden && (freeControllerV.currentPositionState == FreeControllerV3.PositionState.On || freeControllerV.currentRotationState == FreeControllerV3.RotationState.On) && (!(excludeController != null) || !(freeControllerV == excludeController)) && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV)) && (!(freeControllerV.containingAtom != null) || !freeControllerV.containingAtom.hidden))
					{
						if (!overlappingFcs.Contains(freeControllerV))
						{
							overlappingFcs.Add(freeControllerV);
						}
						float distanceHolder2 = Vector3.SqrMagnitude(processFromOverlap.position - collider2.transform.position);
						freeControllerV.distanceHolder = distanceHolder2;
					}
				}
			}
			if (!flag)
			{
				for (int k = 0; k < num; k++)
				{
					Collider collider3 = overlappingControls[k];
					FreeControllerV3 component3 = collider3.GetComponent<FreeControllerV3>();
					if (component3 != null && (gameMode == GameMode.Edit || component3.interactableInPlayMode) && !component3.possessed && (!(excludeController != null) || !(component3 == excludeController)) && (onlyShowControllers == null || onlyShowControllers.Contains(component3)) && (!component3.hidden || !(component3.containingAtom != null) || !component3.containingAtom.hidden))
					{
						flag = true;
						if (!overlappingFcs.Contains(component3))
						{
							overlappingFcs.Add(component3);
						}
						float distanceHolder3 = Vector3.SqrMagnitude(processFromOverlap.position - collider3.transform.position);
						component3.distanceHolder = distanceHolder3;
					}
				}
			}
			if (!flag)
			{
				for (int l = 0; l < num; l++)
				{
					Collider collider4 = overlappingControls[l];
					FreeControllerV3 freeControllerV2 = null;
					FreeControllerV3Link component4 = collider4.GetComponent<FreeControllerV3Link>();
					if (component4 != null)
					{
						freeControllerV2 = component4.linkedController;
					}
					if (freeControllerV2 != null && (gameMode == GameMode.Edit || freeControllerV2.interactableInPlayMode) && !freeControllerV2.possessed && (!(excludeController != null) || !(freeControllerV2 == excludeController)) && (onlyShowControllers == null || onlyShowControllers.Contains(freeControllerV2)) && (!freeControllerV2.hidden || !(freeControllerV2.containingAtom != null) || !freeControllerV2.containingAtom.hidden))
					{
						if (!overlappingFcs.Contains(freeControllerV2))
						{
							overlappingFcs.Add(freeControllerV2);
						}
						float distanceHolder4 = Vector3.SqrMagnitude(processFromOverlap.position - collider4.transform.position);
						freeControllerV2.distanceHolder = distanceHolder4;
					}
				}
			}
		}
		if (sh != null)
		{
			sh.ClearSelections();
			if (overlappingFcs.Count > 0)
			{
				highlightedControllers.Clear();
				if (alreadyDisplayed == null)
				{
					alreadyDisplayed = new List<FreeControllerV3>();
				}
				else
				{
					alreadyDisplayed.Clear();
				}
				overlappingFcs.Sort((FreeControllerV3 c1, FreeControllerV3 c2) => c1.distanceHolder.CompareTo(c2.distanceHolder));
				if (gameMode == GameMode.Edit)
				{
					sh.gameObject.SetActive(value: true);
				}
				else
				{
					sh.gameObject.SetActive(value: false);
				}
				sh.useDrawFromPosition = true;
				sh.drawFrom = processFromOverlap.position;
				int num2 = 0;
				foreach (FreeControllerV3 overlappingFc in overlappingFcs)
				{
					if (!alreadyDisplayed.Contains(overlappingFc))
					{
						if (num2 == 0)
						{
							highlightedControllers.Add(overlappingFc);
						}
						sh.SetSelection(name: (gameMode != GameMode.Play) ? (overlappingFc.containingAtom.uid + ":" + overlappingFc.name) : string.Empty, index: num2, selection: overlappingFc.transform);
						num2++;
						alreadyDisplayed.Add(overlappingFc);
					}
				}
				sh.transform.position = processFromOverlap.position;
				Vector3 localScale = default(Vector3);
				localScale.z = (localScale.y = (localScale.x = (sh.transform.position - lookCamera.transform.position).magnitude));
				sh.transform.localScale = localScale;
				sh.transform.LookAt(lookCamera.transform.position, lookCamera.transform.up);
			}
			else if (!ptrMode)
			{
				highlightedControllers.Clear();
				sh.gameObject.SetActive(value: false);
			}
		}
		if ((overlappingFcs.Count == 0 && ptrMode) || pointersAlwaysEnabled)
		{
			result = !MonitorRigActive;
		}
		isOverlap = overlappingFcs.Count != 0;
		if (overlappingFcs.Count == 0 && ptrMode)
		{
			castRay.origin = processFromPointer.position;
			castRay.direction = processFromPointer.forward;
			sh.useDrawFromPosition = true;
			sh.drawFrom = processFromPointer.position;
			if (!uihit)
			{
				ProcessTargetSelectionDoRaycast(sh, castRay, highlightedControllers, doHighlight: false);
			}
		}
		return result;
	}

	private bool ProcessTargetSelectionDoGrabRight(Transform rightControl, bool isRemote)
	{
		bool result = false;
		if ((bool)rightGrabbedController)
		{
			rightGrabbedController.RestorePreLinkState();
			rightGrabbedController = null;
		}
		FreeControllerV3 freeControllerV = null;
		for (int i = 0; i < highlightedControllersRight.Count; i++)
		{
			FreeControllerV3 freeControllerV2 = highlightedControllersRight[i];
			if ((freeControllerV2.canGrabPosition || freeControllerV2.canGrabRotation) && (!(rightFullGrabbedController != null) || !(rightFullGrabbedController == freeControllerV2)))
			{
				freeControllerV = freeControllerV2;
				highlightedControllersRight.RemoveAt(i);
				highlightedControllersRight.Add(freeControllerV);
				break;
			}
		}
		if (freeControllerV != null)
		{
			Rigidbody component = rightControl.GetComponent<Rigidbody>();
			rightGrabbedController = freeControllerV;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == freeControllerV)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			if (leftFullGrabbedController == rightGrabbedController)
			{
				leftFullGrabbedController.RestorePreLinkState();
				leftFullGrabbedController = null;
				leftHandControl = null;
			}
			if (leftGrabbedController == rightGrabbedController)
			{
				leftGrabbedController.RestorePreLinkState();
				leftGrabbedController = null;
			}
			if (component != null)
			{
				bool flag = true;
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (rightGrabbedController.canGrabPosition)
				{
					if (rightGrabbedController.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (rightGrabbedController.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					result = true;
					rightGrabbedControllerIsRemote = isRemote;
					if (rightFullGrabbedController != null)
					{
						Rigidbody followWhenOffRB = rightFullGrabbedController.followWhenOffRB;
						if (followWhenOffRB != null)
						{
							rightGrabbedController.SelectLinkToRigidbody(followWhenOffRB, linkState, usePhysicalLink: true);
						}
						else
						{
							rightGrabbedController.SelectLinkToRigidbody(component, linkState);
						}
					}
					else
					{
						rightGrabbedController.SelectLinkToRigidbody(component, linkState);
					}
				}
			}
		}
		return result;
	}

	private bool ProcessTargetSelectionDoFullGrabRight(Transform rightControl, bool isRemote, bool isRemoteGrab)
	{
		bool result = false;
		FreeControllerV3 freeControllerV = null;
		for (int i = 0; i < highlightedControllersRight.Count; i++)
		{
			FreeControllerV3 freeControllerV2 = highlightedControllersRight[i];
			if (freeControllerV2.canGrabPosition || freeControllerV2.canGrabRotation)
			{
				freeControllerV = freeControllerV2;
				highlightedControllersRight.RemoveAt(i);
				highlightedControllersRight.Add(freeControllerV);
				break;
			}
		}
		if (freeControllerV != null && ((isRemote && isRemoteGrab) || (!isRemote && !isRemoteGrab)))
		{
			Rigidbody component = rightControl.GetComponent<Rigidbody>();
			rightFullGrabbedController = freeControllerV;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == freeControllerV)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			if (leftFullGrabbedController == rightFullGrabbedController)
			{
				leftFullGrabbedController.RestorePreLinkState();
				leftFullGrabbedController = null;
				if (leftHandControl != null)
				{
					leftHandControl.possessed = false;
				}
				leftHandControl = null;
			}
			if (leftGrabbedController == rightFullGrabbedController)
			{
				leftGrabbedController.RestorePreLinkState();
				leftGrabbedController = null;
			}
			if (rightGrabbedController == rightFullGrabbedController)
			{
				rightGrabbedController.RestorePreLinkState();
				rightGrabbedController = null;
			}
			if (component != null)
			{
				bool flag = true;
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (rightFullGrabbedController.canGrabPosition)
				{
					if (rightFullGrabbedController.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (rightFullGrabbedController.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					result = true;
					rightFullGrabbedControllerIsRemote = isRemote;
					rightFullGrabbedController.SelectLinkToRigidbody(component, linkState);
					rightHandControl = rightFullGrabbedController.GetComponent<HandControl>();
					if (rightHandControl == null)
					{
						HandControlLink component2 = rightFullGrabbedController.GetComponent<HandControlLink>();
						if (component2 != null)
						{
							rightHandControl = component2.handControl;
						}
					}
					if (rightHandControl != null)
					{
						rightHandControl.possessed = true;
					}
				}
			}
		}
		else if (rightGrabbedController != null && ((rightGrabbedControllerIsRemote && isRemoteGrab) || (!rightGrabbedControllerIsRemote && !isRemoteGrab)))
		{
			result = true;
			rightFullGrabbedControllerIsRemote = rightGrabbedControllerIsRemote;
			rightFullGrabbedController = rightGrabbedController;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == rightFullGrabbedController)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			rightHandControl = rightFullGrabbedController.GetComponent<HandControl>();
			if (rightHandControl == null)
			{
				HandControlLink component3 = rightFullGrabbedController.GetComponent<HandControlLink>();
				if (component3 != null)
				{
					rightHandControl = component3.handControl;
				}
			}
			if (rightHandControl != null)
			{
				rightHandControl.possessed = true;
			}
			rightGrabbedController = null;
		}
		return result;
	}

	private bool ProcessTargetSelectionDoGrabLeft(Transform leftControl, bool isRemote)
	{
		bool result = false;
		if ((bool)leftGrabbedController)
		{
			leftGrabbedController.RestorePreLinkState();
			leftGrabbedController = null;
		}
		FreeControllerV3 freeControllerV = null;
		for (int i = 0; i < highlightedControllersLeft.Count; i++)
		{
			FreeControllerV3 freeControllerV2 = highlightedControllersLeft[i];
			if ((freeControllerV2.canGrabPosition || freeControllerV2.canGrabRotation) && (!(leftFullGrabbedController != null) || !(leftFullGrabbedController == freeControllerV2)))
			{
				freeControllerV = freeControllerV2;
				highlightedControllersLeft.RemoveAt(i);
				highlightedControllersLeft.Add(freeControllerV);
				break;
			}
		}
		if (freeControllerV != null)
		{
			Rigidbody component = leftControl.GetComponent<Rigidbody>();
			leftGrabbedController = freeControllerV;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == freeControllerV)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			if (rightFullGrabbedController == leftGrabbedController)
			{
				rightFullGrabbedController.RestorePreLinkState();
				rightFullGrabbedController = null;
				if (rightHandControl != null)
				{
					rightHandControl.possessed = false;
				}
				rightHandControl = null;
			}
			if (rightGrabbedController == leftGrabbedController)
			{
				rightGrabbedController.RestorePreLinkState();
				rightGrabbedController = null;
			}
			if (component != null)
			{
				bool flag = true;
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (leftGrabbedController.canGrabPosition)
				{
					if (leftGrabbedController.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (leftGrabbedController.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					result = true;
					leftGrabbedControllerIsRemote = isRemote;
					if (leftFullGrabbedController != null)
					{
						Rigidbody followWhenOffRB = leftFullGrabbedController.followWhenOffRB;
						if (followWhenOffRB != null)
						{
							leftGrabbedController.SelectLinkToRigidbody(followWhenOffRB, linkState, usePhysicalLink: true);
						}
						else
						{
							leftGrabbedController.SelectLinkToRigidbody(component, linkState);
						}
					}
					else
					{
						leftGrabbedController.SelectLinkToRigidbody(component, linkState);
					}
				}
			}
		}
		return result;
	}

	private bool ProcessTargetSelectionDoFullGrabLeft(Transform leftControl, bool isRemote, bool isRemoteGrab)
	{
		bool result = false;
		FreeControllerV3 freeControllerV = null;
		for (int i = 0; i < highlightedControllersLeft.Count; i++)
		{
			FreeControllerV3 freeControllerV2 = highlightedControllersLeft[i];
			if (freeControllerV2.canGrabPosition || freeControllerV2.canGrabRotation)
			{
				freeControllerV = freeControllerV2;
				highlightedControllersLeft.RemoveAt(i);
				highlightedControllersLeft.Add(freeControllerV);
				break;
			}
		}
		if (freeControllerV != null && ((isRemote && isRemoteGrab) || (!isRemote && !isRemoteGrab)))
		{
			Rigidbody component = leftControl.GetComponent<Rigidbody>();
			leftFullGrabbedController = freeControllerV;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == freeControllerV)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			if (rightFullGrabbedController == leftFullGrabbedController)
			{
				rightFullGrabbedController.RestorePreLinkState();
				rightFullGrabbedController = null;
				if (rightHandControl != null)
				{
					rightHandControl.possessed = false;
				}
				rightHandControl = null;
			}
			if (rightGrabbedController == leftFullGrabbedController)
			{
				rightGrabbedController.RestorePreLinkState();
				rightGrabbedController = null;
			}
			if (leftGrabbedController == leftFullGrabbedController)
			{
				leftGrabbedController.RestorePreLinkState();
				leftGrabbedController = null;
			}
			if (component != null)
			{
				bool flag = true;
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (leftFullGrabbedController.canGrabPosition)
				{
					if (leftFullGrabbedController.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (leftFullGrabbedController.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					result = true;
					leftFullGrabbedControllerIsRemote = isRemote;
					leftFullGrabbedController.SelectLinkToRigidbody(component, linkState);
					leftHandControl = leftFullGrabbedController.GetComponent<HandControl>();
					if (leftHandControl == null)
					{
						HandControlLink component2 = leftFullGrabbedController.GetComponent<HandControlLink>();
						if (component2 != null)
						{
							leftHandControl = component2.handControl;
						}
					}
					if (leftHandControl != null)
					{
						leftHandControl.possessed = true;
					}
				}
			}
		}
		else if (leftGrabbedController != null && ((leftGrabbedControllerIsRemote && isRemoteGrab) || (!leftGrabbedControllerIsRemote && !isRemoteGrab)))
		{
			result = true;
			leftFullGrabbedControllerIsRemote = leftGrabbedControllerIsRemote;
			leftFullGrabbedController = leftGrabbedController;
			if (playerNavCollider != null && playerNavCollider.underlyingControl == leftFullGrabbedController)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			leftGrabbedController = null;
			leftHandControl = leftFullGrabbedController.GetComponent<HandControl>();
			if (leftHandControl == null)
			{
				HandControlLink component3 = leftFullGrabbedController.GetComponent<HandControlLink>();
				if (component3 != null)
				{
					leftHandControl = component3.handControl;
				}
			}
			if (leftHandControl != null)
			{
				leftHandControl.possessed = true;
			}
		}
		return result;
	}

	private void ProcessCycle()
	{
		leftCycleX = 0;
		leftCycleY = 0;
		rightCycleX = 0;
		rightCycleY = 0;
		if (!isOpenVR)
		{
			return;
		}
		Vector2 axis = cycleUsingXAxisAction.GetAxis(SteamVR_Input_Sources.LeftHand);
		Vector2 axis2 = cycleUsingYAxisAction.GetAxis(SteamVR_Input_Sources.LeftHand);
		if (cycleEngageAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
		{
			_leftCycleOn = true;
			_leftCycleXPosition = axis.x;
			_leftCycleYPosition = axis2.y;
		}
		if (cycleEngageAction.GetStateUp(SteamVR_Input_Sources.LeftHand))
		{
			_leftCycleOn = false;
		}
		if (_leftCycleOn)
		{
			float num = axis.x - _leftCycleXPosition;
			if ((num > 0f && num > cycleClick) || (num < 0f && 0f - num > cycleClick))
			{
				leftCycleX = (int)(num / cycleClick);
				_leftCycleXPosition = axis.x;
			}
			float num2 = axis2.y - _leftCycleYPosition;
			if ((num2 > 0f && num2 > cycleClick) || (num2 < 0f && 0f - num2 > cycleClick))
			{
				leftCycleY = (int)(num2 / cycleClick);
				_leftCycleYPosition = axis2.y;
			}
		}
		Vector2 axis3 = cycleUsingXAxisAction.GetAxis(SteamVR_Input_Sources.RightHand);
		Vector2 axis4 = cycleUsingYAxisAction.GetAxis(SteamVR_Input_Sources.RightHand);
		if (cycleEngageAction.GetStateDown(SteamVR_Input_Sources.RightHand))
		{
			_rightCycleOn = true;
			_rightCycleXPosition = axis3.x;
			_rightCycleYPosition = axis4.y;
		}
		if (cycleEngageAction.GetStateUp(SteamVR_Input_Sources.RightHand))
		{
			_rightCycleOn = false;
		}
		if (_rightCycleOn)
		{
			float num3 = axis3.x - _rightCycleXPosition;
			if ((num3 > 0f && num3 > cycleClick) || (num3 < 0f && 0f - num3 > cycleClick))
			{
				rightCycleX = (int)(num3 / cycleClick);
				_rightCycleXPosition = axis3.x;
			}
			float num4 = axis4.y - _rightCycleYPosition;
			if ((num4 > 0f && num4 > cycleClick) || (num4 < 0f && 0f - num4 > cycleClick))
			{
				rightCycleY = (int)(num4 / cycleClick);
				_rightCycleYPosition = axis4.y;
			}
		}
	}

	private void ProcessMotionControllerTargetHighlight()
	{
		if (!isMonitorOnly)
		{
			if (highlightedControllersLeft == null)
			{
				highlightedControllersLeft = new List<FreeControllerV3>();
			}
			if (highlightedControllersRight == null)
			{
				highlightedControllersRight = new List<FreeControllerV3>();
			}
			centerHandLeft.rotation = motionControllerLeft.rotation;
			if (ProcessControllerTargetHighlight(leftSelectionHUD, motionControllerLeft, centerHandLeft, _pointerModeLeft, highlightedControllersLeft, GUIhitLeft, leftFullGrabbedController, out isLeftOverlap))
			{
				drawRayLineLeft = !MonitorRigActive;
			}
			if (leftGrabbedController != null || leftFullGrabbedController != null || leftPossessedController != null)
			{
				HideLeftController();
				leftSelectionHUD.gameObject.SetActive(value: false);
			}
			else if (_mainHUDVisible || !UserPreferences.singleton.showControllersMenuOnly)
			{
				ShowLeftController();
			}
			else
			{
				HideLeftController();
			}
			centerHandRight.rotation = motionControllerRight.rotation;
			if (ProcessControllerTargetHighlight(rightSelectionHUD, motionControllerRight, centerHandRight, _pointerModeRight, highlightedControllersRight, GUIhitRight, rightFullGrabbedController, out isRightOverlap))
			{
				drawRayLineRight = !MonitorRigActive;
			}
			if (rightGrabbedController != null || rightFullGrabbedController != null || rightPossessedController != null)
			{
				HideRightController();
				rightSelectionHUD.gameObject.SetActive(value: false);
			}
			else if (_mainHUDVisible || !UserPreferences.singleton.showControllersMenuOnly)
			{
				ShowRightController();
			}
			else
			{
				HideRightController();
			}
		}
	}

	private void ProcessMotionControllerTargetControl(bool canSelect = true)
	{
		if (isMonitorOnly)
		{
			return;
		}
		if (canSelect && !didStartRightNavigate && GetRightSelect())
		{
			ProcessTargetSelectionDoSelect(highlightedControllersRight);
		}
		if (commonHandModelControl != null && !_leapHandRightConnected && GetRightToggleHand())
		{
			commonHandModelControl.ToggleRightHandEnabled();
		}
		else
		{
			if (!isRightOverlap && GetRightRemoteGrab())
			{
				ProcessTargetSelectionDoGrabRight(motionControllerRight, isRemote: true);
			}
			else if (isRightOverlap && GetRightGrab())
			{
				ProcessTargetSelectionDoGrabRight(motionControllerRight, isRemote: false);
			}
			bool flag = false;
			if (GetRightHoldGrab())
			{
				if ((bool)rightFullGrabbedController)
				{
					flag = true;
					rightFullGrabbedController.RestorePreLinkState();
					rightFullGrabbedController = null;
					if (rightHandControl != null)
					{
						rightHandControl.possessed = false;
					}
					rightHandControl = null;
				}
				else
				{
					flag = ProcessTargetSelectionDoFullGrabRight(motionControllerRight, !isRightOverlap, isRemoteGrab: false);
				}
			}
			if (!flag && !remoteHoldGrabDisabled && GetRightRemoteHoldGrab())
			{
				if ((bool)rightFullGrabbedController)
				{
					rightFullGrabbedController.RestorePreLinkState();
					rightFullGrabbedController = null;
					if (rightHandControl != null)
					{
						rightHandControl.possessed = false;
					}
					rightHandControl = null;
				}
				else
				{
					ProcessTargetSelectionDoFullGrabRight(motionControllerRight, !isRightOverlap, isRemoteGrab: true);
				}
			}
		}
		if (isOpenVR)
		{
			Vector2 axis = pushPullAction.GetAxis(SteamVR_Input_Sources.RightHand);
			if (rightGrabbedController != null && rightGrabbedControllerIsRemote)
			{
				if (!Mathf.Approximately(axis.sqrMagnitude, 0f))
				{
					if (float.IsNaN(axis.y))
					{
						axis.y = 0f;
					}
					axis.y = Mathf.Clamp(axis.y, -100f, 100f);
					rightGrabbedController.MoveLinkConnectorTowards(motionControllerRight, axis.y * 0.05f);
				}
			}
			else if (rightFullGrabbedController != null && rightFullGrabbedControllerIsRemote && !Mathf.Approximately(axis.sqrMagnitude, 0f))
			{
				if (float.IsNaN(axis.y))
				{
					axis.y = 0f;
				}
				axis.y = Mathf.Clamp(axis.y, -100f, 100f);
				rightFullGrabbedController.MoveLinkConnectorTowards(motionControllerRight, axis.y * 0.05f);
			}
		}
		if (!GUIhitRight && highlightedControllersRight.Count > 1)
		{
			if (rightCycleX < 0 || rightCycleY < 0)
			{
				ProcessTargetSelectionCycleBackwardsSelect(highlightedControllersRight);
				float num = (float)(Mathf.Abs(rightCycleX) + Mathf.Abs(rightCycleY)) * 0.1f;
				hapticAction.Execute(0f, num, 1f / num, 1f, SteamVR_Input_Sources.RightHand);
			}
			else if (rightCycleX > 0 || rightCycleY > 0)
			{
				ProcessTargetSelectionCycleSelect(highlightedControllersRight);
				float num2 = (float)(Mathf.Abs(rightCycleX) + Mathf.Abs(rightCycleY)) * 0.1f;
				hapticAction.Execute(0f, num2, 1f / num2, 1f, SteamVR_Input_Sources.RightHand);
			}
		}
		if (canSelect && !didStartLeftNavigate && GetLeftSelect())
		{
			ProcessTargetSelectionDoSelect(highlightedControllersLeft);
		}
		if (commonHandModelControl != null && !_leapHandLeftConnected && GetLeftToggleHand())
		{
			commonHandModelControl.ToggleLeftHandEnabled();
		}
		else
		{
			if (!isLeftOverlap && GetLeftRemoteGrab())
			{
				ProcessTargetSelectionDoGrabLeft(motionControllerLeft, isRemote: true);
			}
			else if (isLeftOverlap && GetLeftGrab())
			{
				ProcessTargetSelectionDoGrabLeft(motionControllerLeft, isRemote: false);
			}
			bool flag2 = false;
			if (GetLeftHoldGrab())
			{
				if ((bool)leftFullGrabbedController)
				{
					flag2 = true;
					leftFullGrabbedController.RestorePreLinkState();
					leftFullGrabbedController = null;
					if (leftHandControl != null)
					{
						leftHandControl.possessed = false;
					}
					leftHandControl = null;
				}
				else
				{
					flag2 = ProcessTargetSelectionDoFullGrabLeft(motionControllerLeft, !isLeftOverlap, isRemoteGrab: false);
				}
			}
			if (!flag2 && !remoteHoldGrabDisabled && GetLeftRemoteHoldGrab())
			{
				if ((bool)leftFullGrabbedController)
				{
					leftFullGrabbedController.RestorePreLinkState();
					leftFullGrabbedController = null;
					if (leftHandControl != null)
					{
						leftHandControl.possessed = false;
					}
					leftHandControl = null;
				}
				else
				{
					flag2 = ProcessTargetSelectionDoFullGrabLeft(motionControllerLeft, !isLeftOverlap, isRemoteGrab: true);
				}
			}
		}
		if (isOpenVR)
		{
			Vector2 axis2 = pushPullAction.GetAxis(SteamVR_Input_Sources.LeftHand);
			if (leftGrabbedController != null && leftGrabbedControllerIsRemote)
			{
				if (!Mathf.Approximately(axis2.sqrMagnitude, 0f))
				{
					if (float.IsNaN(axis2.y))
					{
						axis2.y = 0f;
					}
					axis2.y = Mathf.Clamp(axis2.y, -100f, 100f);
					leftGrabbedController.MoveLinkConnectorTowards(motionControllerLeft, axis2.y * 0.05f);
				}
			}
			else if (leftFullGrabbedController != null && leftFullGrabbedControllerIsRemote && !Mathf.Approximately(axis2.sqrMagnitude, 0f))
			{
				if (float.IsNaN(axis2.y))
				{
					axis2.y = 0f;
				}
				axis2.y = Mathf.Clamp(axis2.y, -100f, 100f);
				leftFullGrabbedController.MoveLinkConnectorTowards(motionControllerLeft, axis2.y * 0.05f);
			}
		}
		if (!GUIhitLeft && highlightedControllersLeft.Count > 1)
		{
			if (leftCycleX < 0 || leftCycleY < 0)
			{
				ProcessTargetSelectionCycleBackwardsSelect(highlightedControllersLeft);
				float num3 = (float)(Mathf.Abs(leftCycleX) + Mathf.Abs(leftCycleY)) * 0.1f;
				hapticAction.Execute(0f, num3, 1f / num3, 1f, SteamVR_Input_Sources.LeftHand);
			}
			else if (leftCycleX > 0 || leftCycleY > 0)
			{
				ProcessTargetSelectionCycleSelect(highlightedControllersLeft);
				float num4 = (float)(Mathf.Abs(leftCycleX) + Mathf.Abs(leftCycleY)) * 0.1f;
				hapticAction.Execute(0f, num4, 1f / num4, 1f, SteamVR_Input_Sources.LeftHand);
			}
		}
		if (((rightGrabbedControllerIsRemote && GetRightRemoteGrabRelease()) || (!rightGrabbedControllerIsRemote && GetRightGrabRelease())) && (bool)rightGrabbedController)
		{
			rightGrabbedController.RestorePreLinkState();
			rightGrabbedController = null;
		}
		if (((leftGrabbedControllerIsRemote && GetLeftRemoteGrabRelease()) || (!leftGrabbedControllerIsRemote && GetLeftGrabRelease())) && (bool)leftGrabbedController)
		{
			leftGrabbedController.RestorePreLinkState();
			leftGrabbedController = null;
		}
	}

	private void ProcessCommonTargetSelection()
	{
		if (highlightedControllersLook == null)
		{
			highlightedControllersLook = new List<FreeControllerV3>();
		}
		if (!(lookCamera != null) || !useLookSelect)
		{
			return;
		}
		if (GUIhit)
		{
			UnhighlightControllers(highlightedControllersLook);
			if (selectionHUD != null)
			{
				selectionHUD.ClearSelections();
				selectionHUD.gameObject.SetActive(value: false);
			}
		}
		else if (selectMode != 0)
		{
			Transform transform = lookCamera.transform;
			castRay.origin = transform.position;
			castRay.direction = transform.forward;
			ProcessTargetSelectionDoRaycast(selectionHUD, castRay, highlightedControllersLook);
		}
	}

	private void ProcessTargetShow(bool canSelect = true)
	{
		bool flag = gameMode == GameMode.Edit && GetTargetShow();
		if (canSelect)
		{
			if (selectMode != SelectMode.Targets && flag)
			{
				SelectModeTargets();
			}
			if (selectMode != 0 && !flag)
			{
				SelectModeOff();
			}
			return;
		}
		if (flag)
		{
			_pointerModeLeft = true;
			_pointerModeRight = true;
			{
				foreach (FreeControllerV3 allController in allControllers)
				{
					if (onlyShowControllers != null)
					{
						if (onlyShowControllers.Contains(allController))
						{
							allController.hidden = false;
						}
						else
						{
							allController.hidden = true;
						}
					}
					else if (gameMode == GameMode.Edit || allController.interactableInPlayMode)
					{
						if (_showHiddenAtoms || allController.containingAtom == null || !allController.containingAtom.hidden)
						{
							allController.hidden = false;
						}
						else
						{
							allController.hidden = true;
						}
					}
					else
					{
						allController.hidden = true;
					}
				}
				return;
			}
		}
		_pointerModeLeft = false;
		_pointerModeRight = false;
		foreach (FreeControllerV3 allController2 in allControllers)
		{
			allController2.hidden = true;
		}
	}

	public void ToggleTargetsOnWithButton()
	{
		targetsOnWithButton = !targetsOnWithButton;
	}

	private void ProcessControllerTargetSelection()
	{
		if (!useLookSelect)
		{
			return;
		}
		if (highlightedControllersLook == null)
		{
			highlightedControllersLook = new List<FreeControllerV3>();
		}
		if (buttonToggleTargets != null && JoystickControl.GetButtonDown(buttonToggleTargets))
		{
			ToggleTargetsOnWithButton();
		}
		if (!GUIhit)
		{
			if (buttonSelect != null && buttonSelect != string.Empty && JoystickControl.GetButtonDown(buttonSelect))
			{
				ProcessTargetSelectionDoSelect(highlightedControllersLook);
			}
			if (buttonCycleSelection != null && buttonCycleSelection != string.Empty && JoystickControl.GetButtonDown(buttonCycleSelection) && highlightedControllersLook.Count > 0)
			{
				ProcessTargetSelectionCycleSelect(highlightedControllersLook);
			}
		}
		if (buttonUnselect != null && buttonUnselect != string.Empty && JoystickControl.GetButtonDown(buttonUnselect))
		{
			ClearSelection();
		}
	}

	private void ProcessMouseTargetControl(bool canSelect = true)
	{
		if (!(MonitorCenterCamera != null) || !MonitorRigActive)
		{
			return;
		}
		if (highlightedControllersMouse == null)
		{
			highlightedControllersMouse = new List<FreeControllerV3>();
		}
		bool flag = RuntimeTools.ActiveTool != null;
		Vector3 mousePosition = Input.mousePosition;
		Ray ray = MonitorCenterCamera.ScreenPointToRay(mousePosition);
		if (!GUIhitMouse)
		{
			ProcessTargetSelectionDoRaycast(mouseSelectionHUD, ray, highlightedControllersMouse, doHighlight: true, includeHidden: true, setSelectionHUDTransform: false);
		}
		if (Input.GetMouseButtonDown(0))
		{
			eligibleForMouseSelect = false;
			potentialGrabbedControllerMouse = null;
			if (grabbedControllerMouse != null)
			{
				grabbedControllerMouse.RestorePreLinkState();
				grabbedControllerMouse = null;
			}
			if (!GUIhitMouse && !flag)
			{
				eligibleForMouseSelect = true;
				lastMousePosition = Input.mousePosition;
				mouseDownPosition = lastMousePosition;
				dragActivated = false;
				if (highlightedControllersMouse.Count > 0)
				{
					potentialGrabbedControllerMouse = highlightedControllersMouse[0];
					Cursor.visible = false;
				}
				if (potentialGrabbedControllerMouse != null)
				{
					mouseClickUsed = true;
				}
				if (potentialGrabbedControllerMouse != null)
				{
					grabbedControllerMouseDistance = (potentialGrabbedControllerMouse.transform.position - ray.origin).magnitude;
					mouseDownLastWorldPosition = ray.origin + ray.direction * grabbedControllerMouseDistance;
				}
			}
		}
		if (!flag && Input.GetMouseButton(0) && potentialGrabbedControllerMouse != null)
		{
			if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftControl))
			{
				lastMousePosition = mousePosition;
			}
			if (!dragActivated && (Mathf.Abs(mousePosition.x - mouseDownPosition.x) > 2f || Mathf.Abs(mousePosition.y - mouseDownPosition.y) > 2f))
			{
				dragActivated = true;
				if (mouseGrab != null)
				{
					Rigidbody component = mouseGrab.GetComponent<Rigidbody>();
					if (component != null)
					{
						bool flag2 = true;
						FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
						if (potentialGrabbedControllerMouse.canGrabPosition)
						{
							if (potentialGrabbedControllerMouse.canGrabRotation)
							{
								linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
							}
						}
						else if (potentialGrabbedControllerMouse.canGrabRotation)
						{
							linkState = FreeControllerV3.SelectLinkState.Rotation;
						}
						else
						{
							flag2 = false;
						}
						if (flag2)
						{
							mouseGrab.position = potentialGrabbedControllerMouse.transform.position;
							mouseGrab.rotation = potentialGrabbedControllerMouse.transform.rotation;
							grabbedControllerMouse = potentialGrabbedControllerMouse;
							grabbedControllerMouse.SelectLinkToRigidbody(component, linkState);
						}
					}
				}
			}
			if (dragActivated && grabbedControllerMouse != null)
			{
				Vector3 vector = ray.origin + ray.direction * grabbedControllerMouseDistance;
				bool key = Input.GetKey(KeyCode.LeftControl);
				bool key2 = Input.GetKey(KeyCode.LeftShift);
				if (grabbedControllerMouse.canGrabRotation && (key || key2))
				{
					Vector3 vector2 = mousePosition - lastMousePosition;
					lastMousePosition = mousePosition;
					if (key2)
					{
						mouseGrab.Rotate(MonitorCenterCamera.transform.up, 0f - vector2.x, Space.World);
						mouseGrab.Rotate(MonitorCenterCamera.transform.right, vector2.y, Space.World);
					}
					else
					{
						mouseGrab.Rotate(MonitorCenterCamera.transform.forward, 0f - vector2.x, Space.World);
						mouseGrab.Rotate(MonitorCenterCamera.transform.right, vector2.y, Space.World);
					}
				}
				if (grabbedControllerMouse.canGrabPosition && !key && !key2)
				{
					Vector3 vector3 = vector - mouseDownLastWorldPosition;
					mouseGrab.position += vector3;
				}
				mouseDownLastWorldPosition = vector;
			}
		}
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}
		if (!dragActivated && !flag && !GUIhitMouse && eligibleForMouseSelect)
		{
			ProcessTargetSelectionDoRaycast(null, ray, highlightedControllersMouse, doHighlight: true, includeHidden: true);
			if (canSelect)
			{
				ProcessTargetSelectionDoSelect(highlightedControllersMouse);
			}
		}
		if (grabbedControllerMouse != null)
		{
			grabbedControllerMouse.RestorePreLinkState();
			grabbedControllerMouse = null;
		}
		Cursor.visible = true;
	}

	protected void SyncUISide()
	{
		if (MonitorRigActive)
		{
			UISideAlign.useNeutralRotation = true;
			UISideAlign.globalSide = UISideAlign.Side.Left;
		}
		else
		{
			UISideAlign.useNeutralRotation = false;
			UISideAlign.globalSide = _UISide;
		}
	}

	public void SetUISide(string side)
	{
		try
		{
			UISide = (UISideAlign.Side)Enum.Parse(typeof(UISideAlign.Side), side);
		}
		catch (FormatException)
		{
			LogError("Tried to set UI side to " + side + " which is not a valid side type");
		}
	}

	public void SelectModeOffAndShowMainHUDAuto()
	{
		SelectModeOff();
		ShowMainHUDAuto();
	}

	public void ShowMainHUDAuto()
	{
		if (MonitorRigActive)
		{
			ShowMainHUD(setAnchors: true, forceMonitor: true);
		}
		else
		{
			ShowMainHUD();
		}
	}

	public void ShowMainHUD(bool setAnchors = true, bool forceMonitor = false)
	{
		SyncUISide();
		_mainHUDVisible = true;
		if (isMonitorOnly || forceMonitor)
		{
			_helpOverlayOnAux = false;
			if (helpToggle != null)
			{
				helpToggle.gameObject.SetActive(value: false);
			}
			if (helpToggleAlt != null)
			{
				helpToggleAlt.gameObject.SetActive(value: false);
			}
			_mainHUDAnchoredOnMonitor = true;
			MoveMainHUD(MonitorUIAttachPoint);
		}
		else
		{
			_helpOverlayOnAux = true;
			if (helpToggle != null)
			{
				helpToggle.gameObject.SetActive(value: true);
			}
			if (helpToggleAlt != null)
			{
				helpToggleAlt.gameObject.SetActive(value: true);
			}
			_mainHUDAnchoredOnMonitor = false;
		}
		SyncHelpOverlay();
		if (mainHUDPivot != null)
		{
			Vector3 localEulerAngles = default(Vector3);
			if (isMonitorOnly || forceMonitor)
			{
				localEulerAngles.x = mainHUDPivotXRotationMonitor;
			}
			else
			{
				localEulerAngles.x = mainHUDPivotXRotationVR;
			}
			localEulerAngles.y = 0f;
			localEulerAngles.z = 0f;
			mainHUDPivot.localEulerAngles = localEulerAngles;
		}
		if (mainHUD != null)
		{
			mainHUD.gameObject.SetActive(value: true);
		}
		activeUI = _activeUI;
		if (setAnchors)
		{
			HUDAnchor.SetAnchorsToReference();
		}
		if (selectedControllerPositionHandle != null && selectedControllerPositionHandle.controller != null)
		{
			selectedControllerPositionHandle.enabled = true;
		}
		if (selectedControllerRotationHandle != null && selectedControllerRotationHandle.controller != null)
		{
			selectedControllerRotationHandle.enabled = true;
		}
		SyncVisibility();
	}

	public void HideMainHUD()
	{
		_mainHUDVisible = false;
		if (mainHUD != null)
		{
			mainHUD.gameObject.SetActive(value: false);
		}
		if (selectedController != null)
		{
			selectedController.guihidden = true;
		}
		if (selectedControllerPositionHandle != null)
		{
			selectedControllerPositionHandle.enabled = false;
		}
		if (selectedControllerRotationHandle != null)
		{
			selectedControllerRotationHandle.enabled = false;
		}
		if (customUI != null)
		{
			customUI.gameObject.SetActive(value: false);
		}
		HideTempHelp();
		SyncVisibility();
	}

	public void MoveMainHUD(Vector3 v)
	{
		if (mainHUDAttachPoint != null)
		{
			mainHUDAttachPoint.position = v;
		}
	}

	public void MoveMainHUD(Transform t)
	{
		if (mainHUDAttachPoint != null && t != null)
		{
			mainHUDAttachPoint.position = t.position;
			mainHUDAttachPoint.rotation = t.rotation;
		}
	}

	public void SetHelpHUDText(string txt)
	{
		helpText = txt;
	}

	public void ToggleMainHUD()
	{
		if (mainHUD != null)
		{
			if (_mainHUDVisible)
			{
				HideMainHUD();
			}
			else
			{
				ShowMainHUD();
			}
		}
	}

	public void ToggleMainHUDMonitor()
	{
		if (mainHUD != null)
		{
			if (_mainHUDVisible)
			{
				HideMainHUD();
			}
			else
			{
				ShowMainHUDMonitor();
			}
		}
	}

	public void ShowMainHUDMonitor()
	{
		ShowMainHUD(setAnchors: true, forceMonitor: true);
	}

	private void AssignUICamera(Camera c)
	{
		if (c != null)
		{
			LookInputModule.singleton.referenceCamera = c;
			{
				foreach (Canvas allCanvase in allCanvases)
				{
					if (allCanvase != null && allCanvase.renderMode == RenderMode.WorldSpace)
					{
						allCanvase.worldCamera = c;
					}
				}
				return;
			}
		}
		Error("Tried to call AssignUICamera with a null camera");
	}

	private void ProcessUI()
	{
		if (!UIDisabled)
		{
			if (GetMenuShow())
			{
				if (_mainHUDVisible)
				{
					HideMainHUD();
				}
				else
				{
					ShowMainHUD();
				}
			}
			if (_mainHUDVisible)
			{
				if (GetMenuMoveLeft())
				{
					MoveMainHUD(motionControllerLeft);
				}
				if (GetMenuMoveRight())
				{
					MoveMainHUD(motionControllerRight);
				}
			}
		}
		if (!(LookInputModule.singleton != null))
		{
			return;
		}
		if (useLookSelect)
		{
			AssignUICamera(lookCamera);
			LookInputModule.singleton.ProcessMain();
			GUIhit = LookInputModule.singleton.guiRaycastHit;
		}
		else if (leftControllerCamera != null)
		{
			AssignUICamera(leftControllerCamera);
			LookInputModule.singleton.ProcessMain();
			GUIhitLeft = LookInputModule.singleton.guiRaycastHit;
			if (GUIhitLeft)
			{
				drawRayLineLeft = !MonitorRigActive;
			}
			if (rightControllerCamera != null)
			{
				AssignUICamera(rightControllerCamera);
				LookInputModule.singleton.ProcessRight();
				GUIhitRight = LookInputModule.singleton.guiRaycastHit;
				if (GUIhitRight)
				{
					drawRayLineRight = !MonitorRigActive;
				}
			}
			else
			{
				Error("Right controller camera is null while processing UI");
			}
		}
		else if (rightControllerCamera != null)
		{
			AssignUICamera(rightControllerCamera);
			LookInputModule.singleton.ProcessRight();
			GUIhitRight = LookInputModule.singleton.guiRaycastHit;
			if (GUIhitRight)
			{
				drawRayLineRight = !MonitorRigActive;
			}
		}
		AssignUICamera(MonitorCenterCamera);
		LookInputModule.singleton.ProcessMouseAlt();
		GUIhitMouse = LookInputModule.singleton.mouseRaycastHit;
	}

	private void ProcessUIMove()
	{
		if (!UIDisabled && _mainHUDAnchoredOnMonitor && MonitorCenterCamera != null && MonitorUIAnchor != null && MonitorUIAttachPoint != null)
		{
			Vector3 position = default(Vector3);
			position.x = 5f;
			position.y = _monitorUIYOffset + 60f;
			position.z = (float)MonitorCenterCamera.pixelHeight / 700f / _monitorUIScale / fixedMonitorUIScale * (60f / _monitorCameraFOV) * worldScale;
			Vector3 position2 = MonitorCenterCamera.ScreenToWorldPoint(position);
			MonitorUIAnchor.position = position2;
			MoveMainHUD(MonitorUIAttachPoint);
			HUDAnchor.SetAnchorsToReference();
		}
	}

	public void RemoveCanvas(Canvas c)
	{
		allCanvases.Remove(c);
	}

	public void AddCanvas(Canvas c)
	{
		if (overrideCanvasSortingLayer)
		{
			IgnoreCanvas component = c.GetComponent<IgnoreCanvas>();
			if (component == null)
			{
				c.sortingLayerName = overrideCanvasSortingLayerName;
			}
		}
		allCanvases.Add(c);
	}

	protected void AllocateRaycastHits()
	{
		if (raycastHits == null)
		{
			raycastHits = new RaycastHit[256];
		}
	}

	private void ProcessSelectDoRaycast(SelectionHUD sh, Ray ray, List<SelectTarget> hitsList, bool doHighlight = true, bool setSelectionHUDTransform = true)
	{
		AllocateRaycastHits();
		int num = Physics.RaycastNonAlloc(ray, raycastHits, 50f, selectColliderMask);
		if (num > 0)
		{
			if (wasHitST == null)
			{
				wasHitST = new Dictionary<SelectTarget, bool>();
			}
			else
			{
				wasHitST.Clear();
			}
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = raycastHits[i];
				SelectTarget component = raycastHit.transform.GetComponent<SelectTarget>();
				if (component != null && !wasHitST.ContainsKey(component))
				{
					wasHitST.Add(component, value: true);
					if (!hitsList.Contains(component))
					{
						hitsList.Add(component);
					}
				}
			}
			SelectTarget[] array = hitsList.ToArray();
			SelectTarget[] array2 = array;
			foreach (SelectTarget selectTarget in array2)
			{
				if (!wasHitST.ContainsKey(selectTarget))
				{
					selectTarget.highlighted = false;
					hitsList.Remove(selectTarget);
				}
			}
			if (doHighlight)
			{
				for (int k = 0; k < hitsList.Count; k++)
				{
					SelectTarget selectTarget2 = hitsList[k];
					if (k == 0)
					{
						selectTarget2.highlighted = true;
					}
					else
					{
						selectTarget2.highlighted = false;
					}
				}
			}
			if (sh != null)
			{
				sh.ClearSelections();
				if (hitsList.Count > 0)
				{
					int num2 = 0;
					foreach (SelectTarget hits in hitsList)
					{
						sh.SetSelection(num2, hits.transform, hits.selectionName);
						num2++;
					}
				}
			}
		}
		else
		{
			if (doHighlight)
			{
				foreach (SelectTarget hits2 in hitsList)
				{
					hits2.highlighted = false;
				}
			}
			hitsList.Clear();
		}
		if (!(sh != null))
		{
			return;
		}
		if (hitsList.Count > 0)
		{
			sh.gameObject.SetActive(value: true);
			if (setSelectionHUDTransform)
			{
				sh.transform.position = hitsList[0].transform.position;
				Vector3 localScale = default(Vector3);
				localScale.z = (localScale.y = (localScale.x = (sh.transform.position - lookCamera.transform.position).magnitude));
				sh.transform.localScale = localScale;
				sh.transform.LookAt(lookCamera.transform.position);
			}
		}
		else
		{
			sh.gameObject.SetActive(value: false);
		}
	}

	private void ProcessSelectDoSelect(List<SelectTarget> highlightedSelectTargets)
	{
		SelectTarget selectTarget = highlightedSelectTargets[0];
		FreeControllerV3 value;
		switch (selectMode)
		{
			case SelectMode.Controller:
				if (fcMap.TryGetValue(selectTarget.selectionName, out value))
				{
					selectControllerCallback(value);
					SelectModeOff();
				}
				break;
			case SelectMode.ForceProducer:
			{
				if (fpMap.TryGetValue(selectTarget.selectionName, out var value6))
				{
					selectForceProducerCallback(value6);
					SelectModeOff();
				}
				break;
			}
			case SelectMode.ForceReceiver:
			{
				if (frMap.TryGetValue(selectTarget.selectionName, out var value2))
				{
					selectForceReceiverCallback(value2);
					SelectModeOff();
				}
				break;
			}
			case SelectMode.Rigidbody:
			{
				if (rbMap.TryGetValue(selectTarget.selectionName, out var value5))
				{
					selectRigidbodyCallback(value5);
					SelectModeOff();
				}
				break;
			}
			case SelectMode.Atom:
			{
				if (atoms.TryGetValue(selectTarget.selectionName, out var value3))
				{
					selectAtomCallback(value3);
					SelectModeOff();
				}
				break;
			}
			case SelectMode.ArmedForRecord:
			{
				if (macMap.TryGetValue(selectTarget.selectionName, out var value4))
				{
					value4.armedForRecord = !value4.armedForRecord;
					if (value4.armedForRecord)
					{
						selectTarget.SetColor(Color.green);
					}
					else
					{
						selectTarget.SetColor(Color.red);
					}
				}
				break;
			}
			case SelectMode.PossessAndAlign:
				if (fcMap.TryGetValue(selectTarget.selectionName, out value))
				{
					HeadPossess(value, alignRig: true);
					SelectModePossess(excludeHeadClear: true);
				}
				break;
			case SelectMode.Unpossess:
				if (fcMap.TryGetValue(selectTarget.selectionName, out value))
				{
					ClearPossess(excludeHeadClear: false, value);
					if (GetCancel())
					{
						SelectModeOff();
					}
				}
				break;
			case SelectMode.Possess:
			case SelectMode.TwoStagePossess:
			case SelectMode.AnimationRecord:
				break;
		}
	}

	private void ProcessSelectCycleSelect(List<SelectTarget> highlightedSelectTargets)
	{
		if (highlightedSelectTargets != null && highlightedSelectTargets.Count > 1)
		{
			SelectTarget item = highlightedSelectTargets[0];
			highlightedSelectTargets.RemoveAt(0);
			highlightedSelectTargets.Add(item);
		}
	}

	private void ProcessSelectCycleBackwardsSelect(List<SelectTarget> highlightedSelectTargets)
	{
		if (highlightedSelectTargets != null && highlightedSelectTargets.Count > 1)
		{
			int index = highlightedSelectTargets.Count - 1;
			SelectTarget item = highlightedSelectTargets[index];
			highlightedSelectTargets.RemoveAt(index);
			highlightedSelectTargets.Insert(0, item);
		}
	}

	private void ProcessSelectTargetHighlight(SelectionHUD sh, Transform processFrom, bool isLeft)
	{
		castRay.origin = processFrom.position;
		castRay.direction = processFrom.forward;
		if (isLeft)
		{
			drawRayLineLeft = !MonitorRigActive;
			ProcessSelectDoRaycast(sh, castRay, highlightedSelectTargetsLeft, doHighlight: false);
			sh.useDrawFromPosition = true;
			sh.drawFrom = processFrom.position;
		}
		else
		{
			drawRayLineRight = !MonitorRigActive;
			ProcessSelectDoRaycast(sh, castRay, highlightedSelectTargetsRight, doHighlight: false);
			sh.useDrawFromPosition = true;
			sh.drawFrom = processFrom.position;
		}
	}

	private void ProcessMotionControllerSelect()
	{
		if (isMonitorOnly)
		{
			return;
		}
		if (highlightedSelectTargetsLeft == null)
		{
			highlightedSelectTargetsLeft = new List<SelectTarget>();
		}
		if (highlightedSelectTargetsRight == null)
		{
			highlightedSelectTargetsRight = new List<SelectTarget>();
		}
		if ((bool)motionControllerLeft && !GUIhitLeft)
		{
			ProcessSelectTargetHighlight(leftSelectionHUD, motionControllerLeft, isLeft: true);
		}
		if ((bool)motionControllerRight && !GUIhitRight)
		{
			ProcessSelectTargetHighlight(rightSelectionHUD, motionControllerRight, isLeft: false);
		}
		if (GetLeftSelect() && highlightedSelectTargetsLeft.Count > 0)
		{
			ProcessSelectDoSelect(highlightedSelectTargetsLeft);
		}
		if (GetRightSelect() && highlightedSelectTargetsRight.Count > 0)
		{
			ProcessSelectDoSelect(highlightedSelectTargetsRight);
		}
		if (isOpenVR)
		{
			if (highlightedSelectTargetsLeft != null && highlightedSelectTargetsLeft.Count > 1)
			{
				if (leftCycleX < 0 || leftCycleY < 0)
				{
					ProcessSelectCycleBackwardsSelect(highlightedSelectTargetsLeft);
					float num = (float)(Mathf.Abs(leftCycleX) + Mathf.Abs(leftCycleY)) * 0.1f;
					hapticAction.Execute(0f, num, 1f / num, 1f, SteamVR_Input_Sources.LeftHand);
				}
				else if (leftCycleX > 0 || leftCycleY > 0)
				{
					ProcessSelectCycleSelect(highlightedSelectTargetsLeft);
					float num2 = (float)(Mathf.Abs(leftCycleX) + Mathf.Abs(leftCycleY)) * 0.1f;
					hapticAction.Execute(0f, num2, 1f / num2, 1f, SteamVR_Input_Sources.LeftHand);
				}
			}
			if (highlightedSelectTargetsRight != null && highlightedSelectTargetsRight.Count > 0)
			{
				if (rightCycleX < 0 || rightCycleY < 0)
				{
					ProcessSelectCycleBackwardsSelect(highlightedSelectTargetsRight);
					float num3 = (float)(Mathf.Abs(rightCycleX) + Mathf.Abs(rightCycleY)) * 0.1f;
					hapticAction.Execute(0f, num3, 1f / num3, 1f, SteamVR_Input_Sources.RightHand);
				}
				else if (rightCycleX > 0 || rightCycleY > 0)
				{
					ProcessSelectCycleSelect(highlightedSelectTargetsRight);
					float num4 = (float)(Mathf.Abs(rightCycleX) + Mathf.Abs(rightCycleY)) * 0.1f;
					hapticAction.Execute(0f, num4, 1f / num4, 1f, SteamVR_Input_Sources.RightHand);
				}
			}
		}
		if (GetCancel())
		{
			SelectModeOff();
		}
	}

	private void ProcessSelect()
	{
		if (highlightedSelectTargetsLook == null)
		{
			highlightedSelectTargetsLook = new List<SelectTarget>();
		}
		if (useLookSelect && lookCamera != null && !GUIhit)
		{
			Transform transform = lookCamera.transform;
			castRay.origin = transform.position;
			castRay.direction = transform.forward;
			ProcessSelectDoRaycast(selectionHUD, castRay, highlightedSelectTargetsLook);
			if (buttonSelect != null && buttonSelect != string.Empty && JoystickControl.GetButtonDown(buttonSelect) && highlightedSelectTargetsLook.Count > 0)
			{
				ProcessSelectDoSelect(highlightedSelectTargetsLook);
			}
			if (buttonCycleSelection != null && buttonCycleSelection != string.Empty && JoystickControl.GetButtonDown(buttonCycleSelection) && highlightedSelectTargetsLook.Count > 0)
			{
				ProcessSelectCycleSelect(highlightedSelectTargetsLook);
			}
			if (buttonUnselect != null && buttonUnselect != string.Empty && JoystickControl.GetButtonDown(buttonUnselect))
			{
				SelectModeOff();
			}
		}
	}

	private void ProcessMouseSelect()
	{
		if (highlightedSelectTargetsMouse == null)
		{
			highlightedSelectTargetsMouse = new List<SelectTarget>();
		}
		if (MonitorRigActive && !GUIhitMouse)
		{
			Vector3 mousePosition = Input.mousePosition;
			Ray ray = MonitorCenterCamera.ScreenPointToRay(mousePosition);
			ProcessSelectDoRaycast(mouseSelectionHUD, ray, highlightedSelectTargetsMouse, doHighlight: true, setSelectionHUDTransform: false);
			if (Input.GetMouseButtonDown(0) && highlightedSelectTargetsMouse.Count > 0)
			{
				ProcessSelectDoSelect(highlightedSelectTargetsMouse);
			}
			if (Input.GetKeyDown(KeyCode.C))
			{
				ProcessSelectCycleSelect(highlightedSelectTargetsMouse);
			}
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				SelectModeOff();
			}
		}
	}

	private FreeControllerV3 ProcessControllerPossess(Transform processFrom)
	{
		AllocateOverlappingControls();
		int num = Physics.OverlapSphereNonAlloc(processFrom.position, 0.01f * _worldScale, overlappingControls, targetColliderMask);
		if (num > 0)
		{
			if (overlappingFcs == null)
			{
				overlappingFcs = new List<FreeControllerV3>();
			}
			else
			{
				overlappingFcs.Clear();
			}
			for (int i = 0; i < num; i++)
			{
				Collider collider = overlappingControls[i];
				FreeControllerV3 component = collider.GetComponent<FreeControllerV3>();
				if (component != null && component.possessable && (component.canGrabPosition || component.canGrabRotation) && !component.possessed && !component.startedPossess)
				{
					if (!overlappingFcs.Contains(component))
					{
						overlappingFcs.Add(component);
					}
					float distanceHolder = Vector3.SqrMagnitude(processFrom.position - collider.transform.position);
					component.distanceHolder = distanceHolder;
				}
			}
			if (overlappingFcs.Count > 0)
			{
				overlappingFcs.Sort((FreeControllerV3 c1, FreeControllerV3 c2) => c1.distanceHolder.CompareTo(c2.distanceHolder));
				return overlappingFcs[0];
			}
		}
		return null;
	}

	private void AlignRigAndController(FreeControllerV3 controller)
	{
		Possessor component = motionControllerHead.GetComponent<Possessor>();
		Vector3 forwardPossessAxis = controller.GetForwardPossessAxis();
		Vector3 upPossessAxis = controller.GetUpPossessAxis();
		Vector3 up = navigationRig.up;
		Vector3 fromDirection = Vector3.ProjectOnPlane(motionControllerHead.forward, up);
		Vector3 vector = Vector3.ProjectOnPlane(forwardPossessAxis, navigationRig.up);
		if (Vector3.Dot(upPossessAxis, up) < 0f && Vector3.Dot(motionControllerHead.up, up) > 0f)
		{
			vector = -vector;
		}
		Quaternion quaternion = Quaternion.FromToRotation(fromDirection, vector);
		navigationRig.rotation = quaternion * navigationRig.rotation;
		if (controller.canGrabRotation)
		{
			controller.AlignTo(component.autoSnapPoint, alsoRotateRB: true);
		}
		Vector3 vector2 = ((!(controller.possessPoint != null)) ? controller.control.position : controller.possessPoint.position);
		Vector3 vector3 = vector2 - component.autoSnapPoint.position;
		Vector3 vector4 = navigationRig.position + vector3;
		float num = Vector3.Dot(vector4 - navigationRig.position, up);
		vector4 += up * (0f - num);
		navigationRig.position = vector4;
		playerHeightAdjust += num;
		if (MonitorCenterCamera != null)
		{
			MonitorCenterCamera.transform.LookAt(controller.transform.position + forwardPossessAxis);
			Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
			localEulerAngles.y = 0f;
			localEulerAngles.z = 0f;
			MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
		}
		headPossessedController.PossessMoveAndAlignTo(component.autoSnapPoint);
	}

	private void HeadPossess(FreeControllerV3 headPossess, bool alignRig = false, bool usePossessorSnapPoint = true, bool adjustSpring = true)
	{
		if (!headPossess.canGrabPosition && !headPossess.canGrabRotation)
		{
			return;
		}
		Possessor component = motionControllerHead.GetComponent<Possessor>();
		Rigidbody component2 = motionControllerHead.GetComponent<Rigidbody>();
		headPossessedController = headPossess;
		headPossessedController.possessed = true;
		if (rightFullGrabbedController == headPossessedController)
		{
			rightFullGrabbedController = null;
			if (rightHandControl != null)
			{
				rightHandControl.possessed = false;
			}
			rightHandControl = null;
		}
		if (leftFullGrabbedController == headPossessedController)
		{
			leftFullGrabbedController = null;
			if (leftHandControl != null)
			{
				leftHandControl.possessed = false;
			}
			leftHandControl = null;
		}
		if (leftGrabbedController == headPossessedController)
		{
			leftGrabbedController = null;
		}
		if (rightGrabbedController == headPossessedController)
		{
			rightGrabbedController = null;
		}
		if (headPossessedController.canGrabPosition)
		{
			MotionAnimationControl component3 = headPossessedController.GetComponent<MotionAnimationControl>();
			if (component3 != null)
			{
				component3.suspendPositionPlayback = true;
			}
			if (_allowPossessSpringAdjustment && adjustSpring)
			{
				headPossessedController.RBHoldPositionSpring = _possessPositionSpring;
			}
		}
		if (headPossessedController.canGrabRotation)
		{
			MotionAnimationControl component4 = headPossessedController.GetComponent<MotionAnimationControl>();
			if (component4 != null)
			{
				component4.suspendRotationPlayback = true;
			}
			if (_allowPossessSpringAdjustment && adjustSpring)
			{
				headPossessedController.RBHoldRotationSpring = _possessRotationSpring;
			}
		}
		if (alignRig)
		{
			AlignRigAndController(headPossessedController);
		}
		else if (component != null && component.autoSnapPoint != null && usePossessorSnapPoint)
		{
			headPossessedController.PossessMoveAndAlignTo(component.autoSnapPoint);
		}
		if (!(component2 != null))
		{
			return;
		}
		FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
		if (headPossessedController.canGrabPosition)
		{
			if (headPossessedController.canGrabRotation)
			{
				linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
			}
		}
		else if (headPossessedController.canGrabRotation)
		{
			linkState = FreeControllerV3.SelectLinkState.Rotation;
		}
		headPossessedController.SelectLinkToRigidbody(component2, linkState);
	}

	protected bool MotionControlPossess(Transform motionController, FreeControllerV3 controllerToPossess, bool usePossessorSnapPoint = true, bool adjustSpring = true)
	{
		if (controllerToPossess.canGrabPosition || controllerToPossess.canGrabRotation)
		{
			Possessor component = motionController.GetComponent<Possessor>();
			Rigidbody component2 = motionController.GetComponent<Rigidbody>();
			if (playerNavCollider != null && playerNavCollider.underlyingControl == controllerToPossess)
			{
				DisconnectNavRigFromPlayerNavCollider();
			}
			controllerToPossess.possessed = true;
			if (rightFullGrabbedController == controllerToPossess)
			{
				rightFullGrabbedController = null;
				if (rightHandControl != null)
				{
					rightHandControl.possessed = false;
				}
				rightHandControl = null;
			}
			if (leftFullGrabbedController == controllerToPossess)
			{
				leftFullGrabbedController = null;
				if (leftHandControl != null)
				{
					leftHandControl.possessed = false;
				}
				leftHandControl = null;
			}
			if (leftGrabbedController == controllerToPossess)
			{
				leftGrabbedController = null;
			}
			if (rightGrabbedController == controllerToPossess)
			{
				rightGrabbedController = null;
			}
			if (controllerToPossess.canGrabPosition)
			{
				MotionAnimationControl component3 = controllerToPossess.GetComponent<MotionAnimationControl>();
				if (component3 != null)
				{
					component3.suspendPositionPlayback = true;
				}
				if (_allowPossessSpringAdjustment && adjustSpring)
				{
					controllerToPossess.RBHoldPositionSpring = _possessPositionSpring;
				}
			}
			if (controllerToPossess.canGrabRotation)
			{
				MotionAnimationControl component4 = controllerToPossess.GetComponent<MotionAnimationControl>();
				if (component4 != null)
				{
					component4.suspendRotationPlayback = true;
				}
				if (_allowPossessSpringAdjustment && adjustSpring)
				{
					controllerToPossess.RBHoldRotationSpring = _possessRotationSpring;
				}
			}
			if (component != null && component.autoSnapPoint != null && usePossessorSnapPoint)
			{
				controllerToPossess.PossessMoveAndAlignTo(component.autoSnapPoint);
			}
			if (component2 != null)
			{
				FreeControllerV3.SelectLinkState linkState = FreeControllerV3.SelectLinkState.Position;
				if (controllerToPossess.canGrabPosition)
				{
					if (controllerToPossess.canGrabRotation)
					{
						linkState = FreeControllerV3.SelectLinkState.PositionAndRotation;
					}
				}
				else if (controllerToPossess.canGrabRotation)
				{
					linkState = FreeControllerV3.SelectLinkState.Rotation;
				}
				controllerToPossess.SelectLinkToRigidbody(component2, linkState);
			}
			return true;
		}
		return false;
	}

	private void ProcessPossess()
	{
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		if (rightPossessedController == null)
		{
			FreeControllerV3 freeControllerV = ProcessControllerPossess(motionControllerRight);
			if (freeControllerV != null && MotionControlPossess(motionControllerRight, freeControllerV))
			{
				flag2 = true;
				if (commonHandModelControl != null && !_leapHandRightConnected)
				{
					commonHandModelControl.rightHandEnabled = false;
				}
				if (alternateControllerHandModelControl != null)
				{
					alternateControllerHandModelControl.rightHandEnabled = false;
				}
				rightPossessedController = freeControllerV;
				HandControl handControl = freeControllerV.GetComponent<HandControl>();
				if (handControl == null)
				{
					HandControlLink component = freeControllerV.GetComponent<HandControlLink>();
					if (component != null)
					{
						handControl = component.handControl;
					}
				}
				if (handControl != null)
				{
					rightPossessHandControl = handControl;
					rightPossessHandControl.possessed = true;
				}
			}
		}
		else
		{
			flag2 = true;
		}
		if (leftPossessedController == null)
		{
			FreeControllerV3 freeControllerV2 = ProcessControllerPossess(motionControllerLeft);
			if (freeControllerV2 != null && MotionControlPossess(motionControllerLeft, freeControllerV2))
			{
				flag3 = true;
				if (commonHandModelControl != null && !_leapHandLeftConnected)
				{
					commonHandModelControl.leftHandEnabled = false;
				}
				if (alternateControllerHandModelControl != null)
				{
					alternateControllerHandModelControl.leftHandEnabled = false;
				}
				leftPossessedController = freeControllerV2;
				HandControl handControl2 = freeControllerV2.GetComponent<HandControl>();
				if (handControl2 == null)
				{
					HandControlLink component2 = freeControllerV2.GetComponent<HandControlLink>();
					if (component2 != null)
					{
						handControl2 = component2.handControl;
					}
				}
				if (handControl2 != null)
				{
					leftPossessHandControl = handControl2;
					leftPossessHandControl.possessed = true;
				}
			}
		}
		else
		{
			flag3 = true;
		}
		if (leapRightPossessedController == null)
		{
			FreeControllerV3 freeControllerV3 = ProcessControllerPossess(leapHandRight);
			if (freeControllerV3 != null && MotionControlPossess(leapHandRight, freeControllerV3))
			{
				flag2 = true;
				if (commonHandModelControl != null)
				{
					commonHandModelControl.rightHandEnabled = false;
				}
				leapRightPossessedController = freeControllerV3;
				HandControl handControl3 = freeControllerV3.GetComponent<HandControl>();
				if (handControl3 == null)
				{
					HandControlLink component3 = freeControllerV3.GetComponent<HandControlLink>();
					if (component3 != null)
					{
						handControl3 = component3.handControl;
					}
				}
				if (handControl3 != null)
				{
					leapRightPossessHandControl = handControl3;
					leapRightPossessHandControl.possessed = true;
				}
			}
		}
		else
		{
			flag2 = true;
		}
		if (leapLeftPossessedController == null)
		{
			FreeControllerV3 freeControllerV4 = ProcessControllerPossess(leapHandLeft);
			if (freeControllerV4 != null && MotionControlPossess(leapHandLeft, freeControllerV4))
			{
				flag3 = true;
				if (commonHandModelControl != null)
				{
					commonHandModelControl.leftHandEnabled = false;
				}
				leapLeftPossessedController = freeControllerV4;
				HandControl handControl4 = freeControllerV4.GetComponent<HandControl>();
				if (handControl4 == null)
				{
					HandControlLink component4 = freeControllerV4.GetComponent<HandControlLink>();
					if (component4 != null)
					{
						handControl4 = component4.handControl;
					}
				}
				if (handControl4 != null)
				{
					leapLeftPossessHandControl = handControl4;
					leapLeftPossessHandControl.possessed = true;
				}
			}
		}
		else
		{
			flag3 = true;
		}
		if (headPossessedController == null)
		{
			FreeControllerV3 freeControllerV5 = ProcessControllerPossess(motionControllerHead);
			if (freeControllerV5 != null)
			{
				HeadPossess(freeControllerV5);
				flag4 = headPossessedController != null;
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			flag4 = true;
		}
		if (GetCancel())
		{
			ClearPossess();
			SelectModeOff();
		}
		if ((flag2 && flag3 && flag4) || GetLeftSelect() || GetRightSelect() || GetMouseSelect())
		{
			SelectModeOff();
		}
	}

	private void ProcessTwoStagePossess()
	{
		if (rightPossessedController == null && rightStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV = ProcessControllerPossess(motionControllerRight);
			if (freeControllerV != null)
			{
				rightStartPossessedController = freeControllerV;
				rightStartPossessedController.startedPossess = true;
			}
		}
		if (leftPossessedController == null && leftStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV2 = ProcessControllerPossess(motionControllerLeft);
			if (freeControllerV2 != null)
			{
				leftStartPossessedController = freeControllerV2;
				leftStartPossessedController.startedPossess = true;
			}
		}
		if (headPossessedController == null && headStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV3 = ProcessControllerPossess(motionControllerHead);
			if (freeControllerV3 != null)
			{
				headStartPossessedController = freeControllerV3;
				headStartPossessedController.startedPossess = true;
			}
		}
		if (leapRightPossessedController == null && leapRightStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV4 = ProcessControllerPossess(leapHandRight);
			if (freeControllerV4 != null)
			{
				leapRightStartPossessedController = freeControllerV4;
				leapRightStartPossessedController.startedPossess = true;
			}
		}
		if (leapLeftPossessedController == null && leapLeftStartPossessedController == null)
		{
			FreeControllerV3 freeControllerV5 = ProcessControllerPossess(leapHandLeft);
			if (freeControllerV5 != null)
			{
				leapLeftStartPossessedController = freeControllerV5;
				leapLeftStartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker1 != null && viveTracker1.gameObject.activeSelf && tracker1PossessedController == null && tracker1StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV6 = ProcessControllerPossess(viveTracker1.transform);
			if (freeControllerV6 != null)
			{
				tracker1StartPossessedController = freeControllerV6;
				tracker1StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker2 != null && viveTracker2.gameObject.activeSelf && tracker2PossessedController == null && tracker2StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV7 = ProcessControllerPossess(viveTracker2.transform);
			if (freeControllerV7 != null)
			{
				tracker2StartPossessedController = freeControllerV7;
				tracker2StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker3 != null && viveTracker3.gameObject.activeSelf && tracker3PossessedController == null && tracker3StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV8 = ProcessControllerPossess(viveTracker3.transform);
			if (freeControllerV8 != null)
			{
				tracker3StartPossessedController = freeControllerV8;
				tracker3StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker4 != null && viveTracker4.gameObject.activeSelf && tracker4PossessedController == null && tracker4StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV9 = ProcessControllerPossess(viveTracker4.transform);
			if (freeControllerV9 != null)
			{
				tracker4StartPossessedController = freeControllerV9;
				tracker4StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker5 != null && viveTracker5.gameObject.activeSelf && tracker5PossessedController == null && tracker5StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV10 = ProcessControllerPossess(viveTracker5.transform);
			if (freeControllerV10 != null)
			{
				tracker5StartPossessedController = freeControllerV10;
				tracker5StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker6 != null && viveTracker6.gameObject.activeSelf && tracker6PossessedController == null && tracker6StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV11 = ProcessControllerPossess(viveTracker6.transform);
			if (freeControllerV11 != null)
			{
				tracker6StartPossessedController = freeControllerV11;
				tracker6StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker7 != null && viveTracker7.gameObject.activeSelf && tracker7PossessedController == null && tracker7StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV12 = ProcessControllerPossess(viveTracker7.transform);
			if (freeControllerV12 != null)
			{
				tracker7StartPossessedController = freeControllerV12;
				tracker7StartPossessedController.startedPossess = true;
			}
		}
		if (viveTracker8 != null && viveTracker8.gameObject.activeSelf && tracker8PossessedController == null && tracker8StartPossessedController == null)
		{
			FreeControllerV3 freeControllerV13 = ProcessControllerPossess(viveTracker8.transform);
			if (freeControllerV13 != null)
			{
				tracker8StartPossessedController = freeControllerV13;
				tracker8StartPossessedController.startedPossess = true;
			}
		}
		if (rightStartPossessedController != null && rightTwoStageLineDrawer != null)
		{
			rightTwoStageLineDrawer.SetLinePoints(motionControllerRight.position, rightStartPossessedController.transform.position);
			rightTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (leftStartPossessedController != null && leftTwoStageLineDrawer != null)
		{
			leftTwoStageLineDrawer.SetLinePoints(motionControllerLeft.position, leftStartPossessedController.transform.position);
			leftTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (headStartPossessedController != null && headTwoStageLineDrawer != null)
		{
			headTwoStageLineDrawer.SetLinePoints(motionControllerHead.position, headStartPossessedController.transform.position);
			headTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (leapRightStartPossessedController != null && leapRightTwoStageLineDrawer != null)
		{
			leapRightTwoStageLineDrawer.SetLinePoints(leapHandRight.transform.position, leapRightStartPossessedController.transform.position);
			leapRightTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (leapLeftStartPossessedController != null && leapLeftTwoStageLineDrawer != null)
		{
			leapLeftTwoStageLineDrawer.SetLinePoints(leapHandLeft.transform.position, leapLeftStartPossessedController.transform.position);
			leapLeftTwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker1StartPossessedController != null && tracker1TwoStageLineDrawer != null)
		{
			tracker1TwoStageLineDrawer.SetLinePoints(viveTracker1.transform.position, tracker1StartPossessedController.transform.position);
			tracker1TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker2StartPossessedController != null && tracker2TwoStageLineDrawer != null)
		{
			tracker2TwoStageLineDrawer.SetLinePoints(viveTracker2.transform.position, tracker2StartPossessedController.transform.position);
			tracker2TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker3StartPossessedController != null && tracker3TwoStageLineDrawer != null)
		{
			tracker3TwoStageLineDrawer.SetLinePoints(viveTracker3.transform.position, tracker3StartPossessedController.transform.position);
			tracker3TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker4StartPossessedController != null && tracker4TwoStageLineDrawer != null)
		{
			tracker4TwoStageLineDrawer.SetLinePoints(viveTracker4.transform.position, tracker4StartPossessedController.transform.position);
			tracker4TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker5StartPossessedController != null && tracker5TwoStageLineDrawer != null)
		{
			tracker5TwoStageLineDrawer.SetLinePoints(viveTracker5.transform.position, tracker5StartPossessedController.transform.position);
			tracker5TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker6StartPossessedController != null && tracker6TwoStageLineDrawer != null)
		{
			tracker6TwoStageLineDrawer.SetLinePoints(viveTracker6.transform.position, tracker6StartPossessedController.transform.position);
			tracker6TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker7StartPossessedController != null && tracker7TwoStageLineDrawer != null)
		{
			tracker7TwoStageLineDrawer.SetLinePoints(viveTracker7.transform.position, tracker7StartPossessedController.transform.position);
			tracker7TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (tracker8StartPossessedController != null && tracker8TwoStageLineDrawer != null)
		{
			tracker8TwoStageLineDrawer.SetLinePoints(viveTracker8.transform.position, tracker8StartPossessedController.transform.position);
			tracker8TwoStageLineDrawer.Draw(base.gameObject.layer);
		}
		if (GetCancel())
		{
			ClearPossess();
			SelectModeOff();
		}
		if (GetLeftSelect() || GetRightSelect() || GetMouseSelect())
		{
			CompleteTwoStagePossess();
		}
	}

	protected void CompleteTwoStagePossess()
	{
		if (rightStartPossessedController != null)
		{
			if (MotionControlPossess(motionControllerRight, rightStartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				rightPossessedController = rightStartPossessedController;
				HandControl handControl = rightPossessedController.GetComponent<HandControl>();
				if (handControl == null)
				{
					HandControlLink component = rightPossessedController.GetComponent<HandControlLink>();
					if (component != null)
					{
						handControl = component.handControl;
					}
				}
				if (handControl != null)
				{
					rightPossessHandControl = handControl;
					rightPossessHandControl.possessed = true;
				}
			}
			rightStartPossessedController.startedPossess = false;
			rightStartPossessedController = null;
		}
		if (leftStartPossessedController != null)
		{
			if (MotionControlPossess(motionControllerLeft, leftStartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				leftPossessedController = leftStartPossessedController;
				HandControl handControl2 = leftPossessedController.GetComponent<HandControl>();
				if (handControl2 == null)
				{
					HandControlLink component2 = leftPossessedController.GetComponent<HandControlLink>();
					if (component2 != null)
					{
						handControl2 = component2.handControl;
					}
				}
				if (handControl2 != null)
				{
					leftPossessHandControl = handControl2;
					leftPossessHandControl.possessed = true;
				}
			}
			leftStartPossessedController.startedPossess = false;
			leftStartPossessedController = null;
		}
		if (headStartPossessedController != null)
		{
			HeadPossess(headStartPossessedController, alignRig: false, usePossessorSnapPoint: false, adjustSpring: false);
			headStartPossessedController.startedPossess = false;
			headStartPossessedController = null;
		}
		if (leapRightStartPossessedController != null)
		{
			if (MotionControlPossess(leapHandRight, leapRightStartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				leapRightPossessedController = leapRightStartPossessedController;
				HandControl handControl3 = leapRightPossessedController.GetComponent<HandControl>();
				if (handControl3 == null)
				{
					HandControlLink component3 = leapRightPossessedController.GetComponent<HandControlLink>();
					if (component3 != null)
					{
						handControl3 = component3.handControl;
					}
				}
				if (handControl3 != null)
				{
					leapRightPossessHandControl = handControl3;
					leapRightPossessHandControl.possessed = true;
				}
			}
			leapRightStartPossessedController.startedPossess = false;
			leapRightStartPossessedController = null;
		}
		if (leapLeftStartPossessedController != null)
		{
			if (MotionControlPossess(leapHandLeft, leapLeftStartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				leapLeftPossessedController = leapLeftStartPossessedController;
				HandControl handControl4 = leapLeftPossessedController.GetComponent<HandControl>();
				if (handControl4 == null)
				{
					HandControlLink component4 = leapLeftPossessedController.GetComponent<HandControlLink>();
					if (component4 != null)
					{
						handControl4 = component4.handControl;
					}
				}
				if (handControl4 != null)
				{
					leapLeftPossessHandControl = handControl4;
					leapLeftPossessHandControl.possessed = true;
				}
			}
			leapLeftStartPossessedController.startedPossess = false;
			leapLeftStartPossessedController = null;
		}
		if (tracker1StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker1.transform, tracker1StartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				tracker1PossessedController = tracker1StartPossessedController;
				tracker1Visible = false;
			}
			tracker1StartPossessedController.startedPossess = false;
			tracker1StartPossessedController = null;
		}
		if (tracker2StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker2.transform, tracker2StartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				tracker2PossessedController = tracker2StartPossessedController;
				tracker2Visible = false;
			}
			tracker2StartPossessedController.startedPossess = false;
			tracker2StartPossessedController = null;
		}
		if (tracker3StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker3.transform, tracker3StartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				tracker3PossessedController = tracker3StartPossessedController;
				tracker3Visible = false;
			}
			tracker3StartPossessedController.startedPossess = false;
			tracker3StartPossessedController = null;
		}
		if (tracker4StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker4.transform, tracker4StartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				tracker4PossessedController = tracker4StartPossessedController;
				tracker4Visible = false;
			}
			tracker4StartPossessedController.startedPossess = false;
			tracker4StartPossessedController = null;
		}
		if (tracker5StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker5.transform, tracker5StartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				tracker5PossessedController = tracker5StartPossessedController;
				tracker5Visible = false;
			}
			tracker5StartPossessedController.startedPossess = false;
			tracker5StartPossessedController = null;
		}
		if (tracker6StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker6.transform, tracker6StartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				tracker6PossessedController = tracker6StartPossessedController;
				tracker6Visible = false;
			}
			tracker6StartPossessedController.startedPossess = false;
			tracker6StartPossessedController = null;
		}
		if (tracker7StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker7.transform, tracker7StartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				tracker7PossessedController = tracker7StartPossessedController;
				tracker7Visible = false;
			}
			tracker7StartPossessedController.startedPossess = false;
			tracker7StartPossessedController = null;
		}
		if (tracker8StartPossessedController != null)
		{
			if (MotionControlPossess(viveTracker8.transform, tracker8StartPossessedController, usePossessorSnapPoint: false, adjustSpring: false))
			{
				tracker8PossessedController = tracker8StartPossessedController;
				tracker8Visible = false;
			}
			tracker8StartPossessedController.startedPossess = false;
			tracker8StartPossessedController = null;
		}
		SelectModeOff();
	}

	public void ClearHeadPossess()
	{
		if (headPossessedController != null)
		{
			ClearPossess(excludeHeadClear: false, headPossessedController);
		}
	}

	public void ClearPossess()
	{
		ClearPossess(excludeHeadClear: false, null);
	}

	public void ClearPossess(bool excludeHeadClear)
	{
		ClearPossess(excludeHeadClear, null);
	}

	public void ClearPossess(bool excludeHeadClear, FreeControllerV3 specificController)
	{
		if (selectMode == SelectMode.Possess || selectMode == SelectMode.TwoStagePossess || selectMode == SelectMode.PossessAndAlign)
		{
			SelectModeOff();
		}
		if (leftPossessedController != null && (specificController == null || leftPossessedController == specificController))
		{
			leftPossessedController.RestorePreLinkState();
			leftPossessedController.possessed = false;
			leftPossessedController.startedPossess = false;
			MotionAnimationControl component = leftPossessedController.GetComponent<MotionAnimationControl>();
			if (component != null)
			{
				component.suspendPositionPlayback = false;
				component.suspendRotationPlayback = false;
			}
			leftPossessedController = null;
			if (leftPossessHandControl != null)
			{
				leftPossessHandControl.possessed = false;
			}
			leftPossessHandControl = null;
			if (alternateControllerHandModelControl != null)
			{
				alternateControllerHandModelControl.leftHandEnabled = true;
			}
		}
		if (leftStartPossessedController != null)
		{
			leftStartPossessedController.startedPossess = false;
			leftStartPossessedController = null;
		}
		if (rightPossessedController != null && (specificController == null || rightPossessedController == specificController))
		{
			rightPossessedController.RestorePreLinkState();
			rightPossessedController.possessed = false;
			rightPossessedController.startedPossess = false;
			MotionAnimationControl component2 = rightPossessedController.GetComponent<MotionAnimationControl>();
			if (component2 != null)
			{
				component2.suspendPositionPlayback = false;
				component2.suspendRotationPlayback = false;
			}
			rightPossessedController = null;
			if (rightPossessHandControl != null)
			{
				rightPossessHandControl.possessed = false;
			}
			rightPossessHandControl = null;
			if (alternateControllerHandModelControl != null)
			{
				alternateControllerHandModelControl.rightHandEnabled = true;
			}
		}
		if (rightStartPossessedController != null)
		{
			rightStartPossessedController.startedPossess = false;
			rightStartPossessedController = null;
		}
		if (leapRightPossessedController != null && (specificController == null || leapRightPossessedController == specificController))
		{
			leapRightPossessedController.RestorePreLinkState();
			leapRightPossessedController.possessed = false;
			leapRightPossessedController.startedPossess = false;
			MotionAnimationControl component3 = leapRightPossessedController.GetComponent<MotionAnimationControl>();
			if (component3 != null)
			{
				component3.suspendPositionPlayback = false;
				component3.suspendRotationPlayback = false;
			}
			leapRightPossessedController = null;
			if (leapRightPossessHandControl != null)
			{
				leapRightPossessHandControl.possessed = false;
			}
			rightPossessHandControl = null;
		}
		if (leapRightStartPossessedController != null)
		{
			leapRightStartPossessedController.startedPossess = false;
			leapRightStartPossessedController = null;
		}
		if (leapLeftPossessedController != null && (specificController == null || leapLeftPossessedController == specificController))
		{
			leapLeftPossessedController.RestorePreLinkState();
			leapLeftPossessedController.possessed = false;
			leapLeftPossessedController.startedPossess = false;
			MotionAnimationControl component4 = leapLeftPossessedController.GetComponent<MotionAnimationControl>();
			if (component4 != null)
			{
				component4.suspendPositionPlayback = false;
				component4.suspendRotationPlayback = false;
			}
			leapLeftPossessedController = null;
			if (leapLeftPossessHandControl != null)
			{
				leapLeftPossessHandControl.possessed = false;
			}
			rightPossessHandControl = null;
		}
		if (leapLeftStartPossessedController != null)
		{
			leapLeftStartPossessedController.startedPossess = false;
			leapLeftStartPossessedController = null;
		}
		if (tracker1PossessedController != null && (specificController == null || tracker1PossessedController == specificController))
		{
			tracker1PossessedController.RestorePreLinkState();
			tracker1PossessedController.possessed = false;
			tracker1PossessedController.startedPossess = false;
			MotionAnimationControl component5 = tracker1PossessedController.GetComponent<MotionAnimationControl>();
			if (component5 != null)
			{
				component5.suspendPositionPlayback = false;
				component5.suspendRotationPlayback = false;
			}
			tracker1PossessedController = null;
			tracker1Visible = true;
		}
		if (tracker1StartPossessedController != null)
		{
			tracker1StartPossessedController.startedPossess = false;
			tracker1StartPossessedController = null;
		}
		if (tracker2PossessedController != null && (specificController == null || tracker2PossessedController == specificController))
		{
			tracker2PossessedController.RestorePreLinkState();
			tracker2PossessedController.possessed = false;
			tracker2PossessedController.startedPossess = false;
			MotionAnimationControl component6 = tracker2PossessedController.GetComponent<MotionAnimationControl>();
			if (component6 != null)
			{
				component6.suspendPositionPlayback = false;
				component6.suspendRotationPlayback = false;
			}
			tracker2PossessedController = null;
			tracker2Visible = true;
		}
		if (tracker2StartPossessedController != null)
		{
			tracker2StartPossessedController.startedPossess = false;
			tracker2StartPossessedController = null;
		}
		if (tracker3PossessedController != null && (specificController == null || tracker3PossessedController == specificController))
		{
			tracker3PossessedController.RestorePreLinkState();
			tracker3PossessedController.possessed = false;
			tracker3PossessedController.startedPossess = false;
			MotionAnimationControl component7 = tracker3PossessedController.GetComponent<MotionAnimationControl>();
			if (component7 != null)
			{
				component7.suspendPositionPlayback = false;
				component7.suspendRotationPlayback = false;
			}
			tracker3PossessedController = null;
			tracker3Visible = true;
		}
		if (tracker3StartPossessedController != null)
		{
			tracker3StartPossessedController.startedPossess = false;
			tracker3StartPossessedController = null;
		}
		if (tracker4PossessedController != null && (specificController == null || tracker4PossessedController == specificController))
		{
			tracker4PossessedController.RestorePreLinkState();
			tracker4PossessedController.possessed = false;
			tracker4PossessedController.startedPossess = false;
			MotionAnimationControl component8 = tracker4PossessedController.GetComponent<MotionAnimationControl>();
			if (component8 != null)
			{
				component8.suspendPositionPlayback = false;
				component8.suspendRotationPlayback = false;
			}
			tracker4PossessedController = null;
			tracker4Visible = true;
		}
		if (tracker5StartPossessedController != null)
		{
			tracker5StartPossessedController.startedPossess = false;
			tracker5StartPossessedController = null;
		}
		if (tracker5PossessedController != null && (specificController == null || tracker5PossessedController == specificController))
		{
			tracker5PossessedController.RestorePreLinkState();
			tracker5PossessedController.possessed = false;
			tracker5PossessedController.startedPossess = false;
			MotionAnimationControl component9 = tracker5PossessedController.GetComponent<MotionAnimationControl>();
			if (component9 != null)
			{
				component9.suspendPositionPlayback = false;
				component9.suspendRotationPlayback = false;
			}
			tracker5PossessedController = null;
			tracker5Visible = true;
		}
		if (tracker6StartPossessedController != null)
		{
			tracker6StartPossessedController.startedPossess = false;
			tracker6StartPossessedController = null;
		}
		if (tracker6PossessedController != null && (specificController == null || tracker6PossessedController == specificController))
		{
			tracker6PossessedController.RestorePreLinkState();
			tracker6PossessedController.possessed = false;
			tracker6PossessedController.startedPossess = false;
			MotionAnimationControl component10 = tracker6PossessedController.GetComponent<MotionAnimationControl>();
			if (component10 != null)
			{
				component10.suspendPositionPlayback = false;
				component10.suspendRotationPlayback = false;
			}
			tracker6PossessedController = null;
			tracker6Visible = true;
		}
		if (tracker7StartPossessedController != null)
		{
			tracker7StartPossessedController.startedPossess = false;
			tracker7StartPossessedController = null;
		}
		if (tracker7PossessedController != null && (specificController == null || tracker7PossessedController == specificController))
		{
			tracker7PossessedController.RestorePreLinkState();
			tracker7PossessedController.possessed = false;
			tracker7PossessedController.startedPossess = false;
			MotionAnimationControl component11 = tracker7PossessedController.GetComponent<MotionAnimationControl>();
			if (component11 != null)
			{
				component11.suspendPositionPlayback = false;
				component11.suspendRotationPlayback = false;
			}
			tracker7PossessedController = null;
			tracker7Visible = true;
		}
		if (tracker8StartPossessedController != null)
		{
			tracker8StartPossessedController.startedPossess = false;
			tracker8StartPossessedController = null;
		}
		if (tracker8PossessedController != null && (specificController == null || tracker8PossessedController == specificController))
		{
			tracker8PossessedController.RestorePreLinkState();
			tracker8PossessedController.possessed = false;
			tracker8PossessedController.startedPossess = false;
			MotionAnimationControl component12 = tracker8PossessedController.GetComponent<MotionAnimationControl>();
			if (component12 != null)
			{
				component12.suspendPositionPlayback = false;
				component12.suspendRotationPlayback = false;
			}
			tracker8PossessedController = null;
			tracker8Visible = true;
		}
		if (headPossessedController != null && !excludeHeadClear && (specificController == null || headPossessedController == specificController))
		{
			headPossessedController.RestorePreLinkState();
			headPossessedController.possessed = false;
			MotionAnimationControl component13 = headPossessedController.GetComponent<MotionAnimationControl>();
			if (component13 != null)
			{
				component13.suspendPositionPlayback = false;
				component13.suspendRotationPlayback = false;
			}
			headPossessedController = null;
		}
		if (headStartPossessedController != null)
		{
			headStartPossessedController.startedPossess = false;
			headStartPossessedController = null;
		}
	}

	protected void VerifyPossess()
	{
		if (rightPossessedController != null)
		{
			Rigidbody component = motionControllerRight.GetComponent<Rigidbody>();
			if (component != null && rightPossessedController.linkToRB != component)
			{
				ClearPossess(excludeHeadClear: true, rightPossessedController);
			}
			else if (rightPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && rightPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && rightPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && rightPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, rightPossessedController);
			}
		}
		if (leftPossessedController != null)
		{
			Rigidbody component2 = motionControllerLeft.GetComponent<Rigidbody>();
			if (component2 != null && leftPossessedController.linkToRB != component2)
			{
				ClearPossess(excludeHeadClear: true, leftPossessedController);
			}
			else if (leftPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && leftPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && leftPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && leftPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, leftPossessedController);
			}
		}
		if (headPossessedController != null)
		{
			Rigidbody component3 = motionControllerHead.GetComponent<Rigidbody>();
			if (component3 != null && headPossessedController.linkToRB != component3)
			{
				ClearPossess(excludeHeadClear: true, headPossessedController);
			}
			else if (headPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && headPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && headPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && headPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, headPossessedController);
			}
		}
		if (leapRightPossessedController != null)
		{
			Rigidbody component4 = leapHandRight.GetComponent<Rigidbody>();
			if (component4 != null && leapRightPossessedController.linkToRB != component4)
			{
				ClearPossess(excludeHeadClear: true, leapRightPossessedController);
			}
			else if (leapRightPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && leapRightPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && leapRightPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && leapRightPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, leapRightPossessedController);
			}
		}
		if (leapLeftPossessedController != null)
		{
			Rigidbody component5 = leapHandLeft.GetComponent<Rigidbody>();
			if (component5 != null && leapLeftPossessedController.linkToRB != component5)
			{
				ClearPossess(excludeHeadClear: true, leapLeftPossessedController);
			}
			else if (leapLeftPossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && leapLeftPossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && leapLeftPossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && leapLeftPossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, leapLeftPossessedController);
			}
		}
		if (tracker1PossessedController != null)
		{
			Rigidbody component6 = viveTracker1.GetComponent<Rigidbody>();
			if (component6 != null && tracker1PossessedController.linkToRB != component6)
			{
				ClearPossess(excludeHeadClear: true, tracker1PossessedController);
			}
			else if (tracker1PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker1PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker1PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker1PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, tracker1PossessedController);
			}
		}
		if (tracker2PossessedController != null)
		{
			Rigidbody component7 = viveTracker2.GetComponent<Rigidbody>();
			if (component7 != null && tracker2PossessedController.linkToRB != component7)
			{
				ClearPossess(excludeHeadClear: true, tracker2PossessedController);
			}
			else if (tracker2PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker2PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker2PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker2PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, tracker2PossessedController);
			}
		}
		if (tracker3PossessedController != null)
		{
			Rigidbody component8 = viveTracker3.GetComponent<Rigidbody>();
			if (component8 != null && tracker3PossessedController.linkToRB != component8)
			{
				ClearPossess(excludeHeadClear: true, tracker3PossessedController);
			}
			else if (tracker3PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker3PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker3PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker3PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, tracker3PossessedController);
			}
		}
		if (tracker4PossessedController != null)
		{
			Rigidbody component9 = viveTracker4.GetComponent<Rigidbody>();
			if (component9 != null && tracker4PossessedController.linkToRB != component9)
			{
				ClearPossess(excludeHeadClear: true, tracker4PossessedController);
			}
			else if (tracker4PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker4PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker4PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker4PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, tracker4PossessedController);
			}
		}
		if (tracker5PossessedController != null)
		{
			Rigidbody component10 = viveTracker5.GetComponent<Rigidbody>();
			if (component10 != null && tracker5PossessedController.linkToRB != component10)
			{
				ClearPossess(excludeHeadClear: true, tracker5PossessedController);
			}
			else if (tracker5PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker5PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker5PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker5PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, tracker5PossessedController);
			}
		}
		if (tracker6PossessedController != null)
		{
			Rigidbody component11 = viveTracker6.GetComponent<Rigidbody>();
			if (component11 != null && tracker6PossessedController.linkToRB != component11)
			{
				ClearPossess(excludeHeadClear: true, tracker6PossessedController);
			}
			else if (tracker6PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker6PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker6PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker6PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, tracker6PossessedController);
			}
		}
		if (tracker7PossessedController != null)
		{
			Rigidbody component12 = viveTracker7.GetComponent<Rigidbody>();
			if (component12 != null && tracker7PossessedController.linkToRB != component12)
			{
				ClearPossess(excludeHeadClear: true, tracker7PossessedController);
			}
			else if (tracker7PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker7PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker7PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker7PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, tracker7PossessedController);
			}
		}
		if (tracker8PossessedController != null)
		{
			Rigidbody component13 = viveTracker8.GetComponent<Rigidbody>();
			if (component13 != null && tracker8PossessedController.linkToRB != component13)
			{
				ClearPossess(excludeHeadClear: true, tracker8PossessedController);
			}
			else if (tracker8PossessedController.currentPositionState != FreeControllerV3.PositionState.ParentLink && tracker8PossessedController.currentPositionState != FreeControllerV3.PositionState.PhysicsLink && tracker8PossessedController.currentRotationState != FreeControllerV3.RotationState.ParentLink && tracker8PossessedController.currentRotationState != FreeControllerV3.RotationState.PhysicsLink)
			{
				ClearPossess(excludeHeadClear: true, tracker8PossessedController);
			}
		}
	}

	public void StopPlayback()
	{
		if (isRecording)
		{
			if (motionAnimationMaster != null)
			{
				motionAnimationMaster.StopRecord();
			}
			isRecording = false;
			SelectModeOff();
		}
		else if (motionAnimationMaster != null)
		{
			motionAnimationMaster.StopPlayback();
		}
	}

	public void StartPlayback()
	{
		if (motionAnimationMaster != null)
		{
			motionAnimationMaster.StartPlayback();
		}
	}

	public void ProcessAnimationRecord()
	{
		if (isRecording)
		{
			helpText = "Recording...press Select or Spacebar to stop recording\n" + motionAnimationMaster.playbackCounter.ToString("F0");
		}
		if (!GetLeftSelect() && !GetRightSelect() && !GetMouseSelect() && !Input.GetKeyDown(KeyCode.Space))
		{
			return;
		}
		if (isRecording)
		{
			StopPlayback();
			return;
		}
		helpText = "Recording...press Select or Spacebar key to stop recording\n" + motionAnimationMaster.playbackCounter.ToString("F0");
		helpColor = Color.red;
		if (motionAnimationMaster != null)
		{
			motionAnimationMaster.StartRecord();
		}
		isRecording = true;
	}

	public void ArmAllControlledControllersForRecord()
	{
		foreach (FreeControllerV3 allController in allControllers)
		{
			if (allController.currentPositionState == FreeControllerV3.PositionState.ParentLink || allController.currentPositionState == FreeControllerV3.PositionState.PhysicsLink || allController.currentRotationState == FreeControllerV3.RotationState.ParentLink || allController.currentRotationState == FreeControllerV3.RotationState.PhysicsLink)
			{
				MotionAnimationControl component = allController.GetComponent<MotionAnimationControl>();
				if (component != null && (CheckIfControllerLinkedToMotionControl(motionControllerLeft, allController) || CheckIfControllerLinkedToMotionControl(motionControllerRight, allController) || CheckIfControllerLinkedToMotionControl(motionControllerHead, allController) || (viveTracker1 != null && CheckIfControllerLinkedToMotionControl(viveTracker1.transform, allController)) || (viveTracker2 != null && CheckIfControllerLinkedToMotionControl(viveTracker2.transform, allController)) || (viveTracker3 != null && CheckIfControllerLinkedToMotionControl(viveTracker3.transform, allController)) || (viveTracker4 != null && CheckIfControllerLinkedToMotionControl(viveTracker4.transform, allController)) || (viveTracker5 != null && CheckIfControllerLinkedToMotionControl(viveTracker5.transform, allController)) || (viveTracker6 != null && CheckIfControllerLinkedToMotionControl(viveTracker6.transform, allController)) || (viveTracker7 != null && CheckIfControllerLinkedToMotionControl(viveTracker7.transform, allController)) || (viveTracker8 != null && CheckIfControllerLinkedToMotionControl(viveTracker8.transform, allController))))
				{
					component.armedForRecord = true;
				}
			}
		}
	}

	public static IEnumerator AssetManagerReady()
	{
		yield return new WaitUntil(() => _singleton != null && _singleton.assetManagerReady);
	}

	private IEnumerator InitAssetManager()
	{
		AssetBundleManager.SetSourceAssetBundleDirectory(Application.streamingAssetsPath + "/");
		AssetBundleLoadManifestOperation request = AssetBundleManager.Initialize();
		if (request != null)
		{
			yield return StartCoroutine(request);
		}
		UnityEngine.Debug.Log("Asset Manager Ready");
		_assetManagerReady = true;
	}

	protected void InitAssetBundleDictionaries()
	{
		if (assetBundleAssetNameToPrefab == null)
		{
			assetBundleAssetNameToPrefab = new Dictionary<string, GameObject>();
		}
		if (assetBundleAssetNameRefCounts == null)
		{
			assetBundleAssetNameRefCounts = new Dictionary<string, int>();
		}
	}

	public GameObject GetCachedPrefab(string assetBundleName, string assetName)
	{
		string key = assetBundleName + ":" + assetName;
		InitAssetBundleDictionaries();
		GameObject value = null;
		if (assetBundleAssetNameToPrefab.TryGetValue(assetBundleName + ":" + assetName, out value))
		{
			if (assetBundleAssetNameRefCounts.TryGetValue(key, out var value2))
			{
				value2++;
				assetBundleAssetNameRefCounts.Remove(key);
				assetBundleAssetNameRefCounts.Add(key, value2);
				AssetBundleManager.RegisterAssetBundleAdditionalUse(assetBundleName);
			}
			else
			{
				UnityEngine.Debug.LogError("Asset bundle ref count dictionary corruption");
			}
		}
		return value;
	}

	public void RegisterPrefab(string assetBundleName, string assetName, GameObject prefab)
	{
		string key = assetBundleName + ":" + assetName;
		InitAssetBundleDictionaries();
		if (assetBundleAssetNameRefCounts.TryGetValue(key, out var value))
		{
			value++;
			assetBundleAssetNameRefCounts.Remove(key);
		}
		else
		{
			value = 1;
			assetBundleAssetNameToPrefab.Add(key, prefab);
		}
		assetBundleAssetNameRefCounts.Add(key, value);
	}

	public void UnregisterPrefab(string assetBundleName, string assetName)
	{
		string text = assetBundleName + ":" + assetName;
		InitAssetBundleDictionaries();
		if (assetBundleAssetNameRefCounts.TryGetValue(text, out var value))
		{
			value--;
			assetBundleAssetNameRefCounts.Remove(text);
			if (value == 0)
			{
				assetBundleAssetNameToPrefab.Remove(text);
			}
			else
			{
				assetBundleAssetNameRefCounts.Add(text, value);
			}
			AssetBundleManager.UnloadAssetBundle(assetBundleName);
		}
		else
		{
			LogError("Tried to UnregisterPrefab " + text + " that was not registered");
		}
	}

	protected void UnregisterAllPrefabsFromAtoms()
	{
		foreach (AtomAsset value2 in atomAssetByType.Values)
		{
			string key = value2.assetBundleName + ":" + value2.assetName;
			if (assetBundleAssetNameRefCounts.TryGetValue(key, out var value))
			{
				assetBundleAssetNameRefCounts.Remove(key);
				assetBundleAssetNameToPrefab.Remove(key);
				for (int i = 0; i < value; i++)
				{
					AssetBundleManager.UnloadAssetBundle(value2.assetBundleName);
				}
			}
		}
	}

	protected IEnumerator LoadAtomFromBundleAsync(AtomAsset aa, string useuid = null, bool userInvoked = false)
	{
		yield return AssetManagerReady();
		float startTime = Time.realtimeSinceStartup;
		GameObject go = GetCachedPrefab(aa.assetBundleName, aa.assetName);
		if (go == null)
		{
			AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(aa.assetBundleName, aa.assetName, typeof(GameObject));
			if (request == null)
			{
				Error("Failed to load Atom " + aa.assetName);
				yield break;
			}
			yield return StartCoroutine(request);
			go = request.GetAsset<GameObject>();
			if (go != null)
			{
				RegisterPrefab(aa.assetBundleName, aa.assetName, go);
			}
			else
			{
				Error("Asset " + aa.assetName + " is missing game object");
			}
		}
		if (!(go != null))
		{
			yield break;
		}
		Atom component = go.GetComponent<Atom>();
		if (component != null)
		{
			startTime = Time.realtimeSinceStartup;
			Transform transform = AddAtom(component, useuid, userInvoked);
			if (transform != null)
			{
				Atom component2 = transform.GetComponent<Atom>();
				if (component2 != null)
				{
					component2.loadedFromBundle = true;
				}
			}
		}
		else
		{
			Error("Asset " + aa.assetName + " is missing Atom component");
		}
	}

	public void PauseSyncAtomLists()
	{
		_pauseSyncAtomLists = true;
	}

	public void ResumeSyncAtomLists()
	{
		_pauseSyncAtomLists = false;
		SyncSortedAtomUIDs();
		SyncSortedAtomUIDsWithForceProducers();
		SyncSortedAtomUIDsWithForceReceivers();
		SyncSortedAtomUIDsWithFreeControllers();
		SyncSortedAtomUIDsWithRhythmControllers();
		SyncSortedAtomUIDsWithRigidbodies();
		SyncHiddenAtoms();
		SyncSelectAtomPopup();
	}

	private void SyncSortedAtomUIDs()
	{
		if (!_isLoading)
		{
			sortedAtomUIDs.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithForceReceivers()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithForceReceivers.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithForceProducers()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithForceProducers.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithRhythmControllers()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithRhythmControllers.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithFreeControllers()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithFreeControllers.Sort();
		}
	}

	private void SyncSortedAtomUIDsWithRigidbodies()
	{
		if (!_isLoading)
		{
			sortedAtomUIDsWithRigidbodies.Sort();
		}
	}

	public void SyncHiddenAtoms()
	{
		if (_isLoading)
		{
			return;
		}
		hiddenAtomUIDs = new List<string>();
		visibleAtomUIDs = new List<string>();
		foreach (string sortedAtomUID in sortedAtomUIDs)
		{
			Atom atomByUid = GetAtomByUid(sortedAtomUID);
			if (atomByUid != null)
			{
				if (atomByUid.hidden)
				{
					hiddenAtomUIDs.Add(sortedAtomUID);
				}
				else
				{
					visibleAtomUIDs.Add(sortedAtomUID);
				}
			}
		}
		hiddenAtomUIDsWithFreeControllers = new List<string>();
		visibleAtomUIDsWithFreeControllers = new List<string>();
		foreach (string sortedAtomUIDsWithFreeController in sortedAtomUIDsWithFreeControllers)
		{
			Atom atomByUid2 = GetAtomByUid(sortedAtomUIDsWithFreeController);
			if (atomByUid2 != null)
			{
				if (atomByUid2.hidden)
				{
					hiddenAtomUIDsWithFreeControllers.Add(sortedAtomUIDsWithFreeController);
				}
				else
				{
					visibleAtomUIDsWithFreeControllers.Add(sortedAtomUIDsWithFreeController);
				}
			}
		}
		SyncSelectAtomPopup();
		SyncVisibility();
	}

	public void ToggleShowHiddenAtoms()
	{
		showHiddenAtoms = !_showHiddenAtoms;
	}

	public List<FreeControllerV3> GetAllFreeControllers()
	{
		return allControllers;
	}

	public List<AnimationPattern> GetAllAnimationPatterns()
	{
		return allAnimationPatterns;
	}

	public List<AnimationStep> GetAllAnimationSteps()
	{
		return allAnimationSteps;
	}

	private string CreateUID(string name)
	{
		if (!uids.ContainsKey(name))
		{
			uids.Add(name, value: true);
			return name;
		}
		for (int i = 2; i < maxUID; i++)
		{
			string text = name + "#" + i;
			if (!uids.ContainsKey(text))
			{
				uids.Add(text, value: true);
				return text;
			}
		}
		Error("Exceeded UID limit of " + maxUID + " for " + name);
		return null;
	}

	public string GetTempUID()
	{
		string text = Guid.NewGuid().ToString();
		while (uids.ContainsKey(text))
		{
			text = Guid.NewGuid().ToString();
		}
		uids.Add(text, value: true);
		return text;
	}

	public void ReleaseTempUID(string uid)
	{
		uids.Remove(uid);
	}

	private void SyncForceReceiverNames()
	{
		_forceReceiverNames = new string[frMap.Keys.Count];
		frMap.Keys.CopyTo(_forceReceiverNames, 0);
		if (onForceReceiverNamesChangedHandlers != null)
		{
			onForceReceiverNamesChangedHandlers(_forceReceiverNames);
		}
	}

	public List<string> GetForceReceiverNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		if (atoms.TryGetValue(atomUID, out var value))
		{
			ForceReceiver[] forceReceivers = value.forceReceivers;
			foreach (ForceReceiver forceReceiver in forceReceivers)
			{
				list.Add(forceReceiver.name);
			}
		}
		return list;
	}

	public ForceReceiver ReceiverNameToForceReceiver(string receiverName)
	{
		if (frMap != null && frMap.TryGetValue(receiverName, out var value))
		{
			return value;
		}
		return null;
	}

	private void SyncForceProducerNames()
	{
		_forceProducerNames = new string[fpMap.Keys.Count];
		fpMap.Keys.CopyTo(_forceProducerNames, 0);
		if (onForceProducerNamesChangedHandlers != null)
		{
			onForceProducerNamesChangedHandlers(_forceProducerNames);
		}
	}

	public List<string> GetForceProducerNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		if (atoms.TryGetValue(atomUID, out var value))
		{
			ForceProducerV2[] forceProducers = value.forceProducers;
			foreach (ForceProducerV2 forceProducerV in forceProducers)
			{
				list.Add(forceProducerV.name);
			}
		}
		return list;
	}

	public ForceProducerV2 ProducerNameToForceProducer(string producerName)
	{
		if (fpMap != null && fpMap.TryGetValue(producerName, out var value))
		{
			return value;
		}
		return null;
	}

	private void SyncRhythmControllerNames()
	{
		_rhythmControllerNames = new string[rcMap.Keys.Count];
		rcMap.Keys.CopyTo(_rhythmControllerNames, 0);
		if (onRhythmControllerNamesChangedHandlers != null)
		{
			onRhythmControllerNamesChangedHandlers(_rhythmControllerNames);
		}
	}

	public List<string> GetRhythmControllerNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		if (atoms.TryGetValue(atomUID, out var value))
		{
			RhythmController[] rhythmControllers = value.rhythmControllers;
			foreach (RhythmController rhythmController in rhythmControllers)
			{
				list.Add(rhythmController.name);
			}
		}
		return list;
	}

	public RhythmController RhythmControllerrNameToRhythmController(string controllerName)
	{
		if (rcMap != null && rcMap.TryGetValue(controllerName, out var value))
		{
			return value;
		}
		return null;
	}

	private void SyncFreeControllerNames()
	{
		_freeControllerNames = new string[fcMap.Keys.Count];
		fcMap.Keys.CopyTo(_freeControllerNames, 0);
		if (onFreeControllerNamesChangedHandlers != null)
		{
			onFreeControllerNamesChangedHandlers(_freeControllerNames);
		}
	}

	public List<string> GetFreeControllerNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		if (atoms.TryGetValue(atomUID, out var value))
		{
			FreeControllerV3[] freeControllers = value.freeControllers;
			foreach (FreeControllerV3 freeControllerV in freeControllers)
			{
				list.Add(freeControllerV.name);
			}
		}
		return list;
	}

	public FreeControllerV3 FreeControllerNameToFreeController(string controllerName)
	{
		if (fcMap != null && fcMap.TryGetValue(controllerName, out var value))
		{
			return value;
		}
		return null;
	}

	private void SyncRigidbodyNames()
	{
		_rigidbodyNames = new string[rbMap.Keys.Count];
		rbMap.Keys.CopyTo(_rigidbodyNames, 0);
		if (onRigidbodyNamesChangedHandlers != null)
		{
			onRigidbodyNamesChangedHandlers(_rigidbodyNames);
		}
	}

	public List<string> GetRigidbodyNamesInAtom(string atomUID)
	{
		List<string> list = new List<string>();
		if (atoms.TryGetValue(atomUID, out var value))
		{
			Rigidbody[] linkableRigidbodies = value.linkableRigidbodies;
			foreach (Rigidbody rigidbody in linkableRigidbodies)
			{
				list.Add(rigidbody.name);
			}
		}
		return list;
	}

	public Rigidbody RigidbodyNameToRigidbody(string rigidbodyName)
	{
		if (rbMap != null && rbMap.TryGetValue(rigidbodyName, out var value))
		{
			return value;
		}
		return null;
	}

	public List<string> GetAtomCategories()
	{
		return atomCategories;
	}

	protected virtual void SetAddAtomAtomPopupValues(string category)
	{
		if (!(atomPrefabPopup != null))
		{
			return;
		}
		if (atomCategoryToAtomTypes != null && atomCategoryToAtomTypes.TryGetValue(category, out var value))
		{
			int num = 0;
			atomPrefabPopup.numPopupValues = value.Count;
			foreach (string item in value)
			{
				atomPrefabPopup.setPopupValue(num, item);
				num++;
			}
			atomPrefabPopup.currentValue = "None";
		}
		else
		{
			atomPrefabPopup.numPopupValues = 0;
		}
	}

	public List<Atom> GetAtoms()
	{
		return new List<Atom>(atoms.Values);
	}

	public List<string> GetAtomUIDs()
	{
		if (showHiddenAtoms)
		{
			return sortedAtomUIDs;
		}
		if (sortAtomUIDs)
		{
			return visibleAtomUIDs;
		}
		return atomUIDs;
	}

	public List<string> GetAtomUIDsWithForceReceivers()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDsWithForceReceivers;
		}
		return atomUIDsWithForceReceivers;
	}

	public List<string> GetAtomUIDsWithForceProducers()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDsWithForceProducers;
		}
		return atomUIDsWithForceProducers;
	}

	public List<string> GetAtomUIDsWithRhythmControllers()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDsWithRhythmControllers;
		}
		return atomUIDsWithRhythmControllers;
	}

	public List<string> GetAtomUIDsWithFreeControllers()
	{
		if (showHiddenAtoms)
		{
			return sortedAtomUIDsWithFreeControllers;
		}
		if (sortAtomUIDs)
		{
			return visibleAtomUIDsWithFreeControllers;
		}
		return atomUIDsWithFreeControllers;
	}

	public List<string> GetAtomUIDsWithRigidbodies()
	{
		if (sortAtomUIDs)
		{
			return sortedAtomUIDsWithRigidbodies;
		}
		return atomUIDsWithRigidbodies;
	}

	public Atom GetAtomByUid(string uid)
	{
		Atom value = null;
		atoms.TryGetValue(uid, out value);
		return value;
	}

	public void AddAtomByPopupValue()
	{
		if (atomPrefabPopup != null && atomPrefabPopup.currentValue != "None")
		{
			StartCoroutine(AddAtomByType(atomPrefabPopup.currentValue, null, userInvoked: true));
		}
	}

	public IEnumerator AddAtomByType(string type, string useuid = null, bool userInvoked = false)
	{
		AsyncFlag loadIconFlag = new AsyncFlag("Load Atom " + type);
		SetLoadingIconFlag(loadIconFlag);
		if (type != null && type != string.Empty)
		{
			Atom atom;
			if (atomAssetByType.TryGetValue(type, out var aa))
			{
				yield return StartCoroutine(LoadAtomFromBundleAsync(aa, useuid, userInvoked));
			}
			else if (atomPrefabByType.TryGetValue(type, out atom))
			{
				AddAtom(atom, useuid, userInvoked);
			}
			else
			{
				Error("Atom type " + type + " does not exist. Cannot add");
			}
		}
		loadIconFlag.Raise();
	}

	public Transform AddAtom(Atom atom, string useuid = null, bool userInvoked = false)
	{
		string text = ((useuid == null) ? CreateUID(atom.name) : CreateUID(useuid));
		if (text != null)
		{
			Transform transform = UnityEngine.Object.Instantiate(atom.transform);
			transform.SetParent(atomContainerTransform, worldPositionStays: true);
			Atom component = transform.GetComponent<Atom>();
			component.uid = text;
			component.name = text;
			InitAtom(component);
			if (userInvoked)
			{
				SubAtom[] componentsInChildren = atom.GetComponentsInChildren<SubAtom>();
				SubAtom[] array = componentsInChildren;
				foreach (SubAtom subAtom in array)
				{
					Atom atomPrefab = subAtom.atomPrefab;
					if (atomPrefab != null)
					{
						Transform transform2 = AddAtom(atomPrefab);
						if (transform2 != null)
						{
							transform2.position = subAtom.transform.position;
							transform2.rotation = subAtom.transform.rotation;
						}
					}
				}
			}
			if (onAtomUIDsChangedHandlers != null)
			{
				onAtomUIDsChangedHandlers(GetAtomUIDs());
			}
			if (onAtomUIDsWithForceReceiversChangedHandlers != null && component.forceReceivers.Length > 0)
			{
				onAtomUIDsWithForceReceiversChangedHandlers(GetAtomUIDsWithForceReceivers());
			}
			if (onAtomUIDsWithForceProducersChangedHandlers != null && component.forceProducers.Length > 0)
			{
				onAtomUIDsWithForceProducersChangedHandlers(GetAtomUIDsWithForceProducers());
			}
			if (onAtomUIDsWithFreeControllersChangedHandlers != null && component.freeControllers.Length > 0)
			{
				onAtomUIDsWithFreeControllersChangedHandlers(GetAtomUIDsWithFreeControllers());
			}
			if (onAtomUIDsWithRigidbodiesChangedHandlers != null && component.linkableRigidbodies.Length > 0)
			{
				onAtomUIDsWithRigidbodiesChangedHandlers(GetAtomUIDsWithRigidbodies());
			}
			SyncVisibility();
			SyncForceReceiverNames();
			SyncForceProducerNames();
			SyncFreeControllerNames();
			SyncRigidbodyNames();
			SyncHiddenAtoms();
			SyncSelectAtomPopup();
			return transform;
		}
		return null;
	}

	public void RenameAtom(Atom atom, string requestedID)
	{
		if (!(atom != null) || atom.uid == requestedID)
		{
			return;
		}
		string uid = atom.uid;
		string text = CreateUID(requestedID);
		atoms.Remove(atom.uid);
		uids.Remove(atom.uid);
		int num = -1;
		if (atomUIDs.Contains(atom.uid))
		{
			num = atomUIDs.IndexOf(atom.uid);
			atomUIDs.RemoveAt(num);
			sortedAtomUIDs.Remove(atom.uid);
		}
		int num2 = -1;
		if (atomUIDsWithForceReceivers.Contains(atom.uid))
		{
			num2 = atomUIDsWithForceReceivers.IndexOf(atom.uid);
			atomUIDsWithForceReceivers.RemoveAt(num2);
			sortedAtomUIDsWithForceReceivers.Remove(atom.uid);
		}
		int num3 = -1;
		if (atomUIDsWithForceProducers.Contains(atom.uid))
		{
			num3 = atomUIDsWithForceProducers.IndexOf(atom.uid);
			atomUIDsWithForceProducers.RemoveAt(num3);
			sortedAtomUIDsWithForceProducers.Remove(atom.uid);
		}
		int num4 = -1;
		if (atomUIDsWithRhythmControllers.Contains(atom.uid))
		{
			num4 = atomUIDsWithRhythmControllers.IndexOf(atom.uid);
			atomUIDsWithRhythmControllers.RemoveAt(num4);
			sortedAtomUIDsWithRhythmControllers.Remove(atom.uid);
		}
		int num5 = -1;
		if (atomUIDsWithFreeControllers.Contains(atom.uid))
		{
			num5 = atomUIDsWithFreeControllers.IndexOf(atom.uid);
			atomUIDsWithFreeControllers.RemoveAt(num5);
			sortedAtomUIDsWithFreeControllers.Remove(atom.uid);
		}
		int num6 = -1;
		if (atomUIDsWithRigidbodies.Contains(atom.uid))
		{
			num6 = atomUIDsWithRigidbodies.IndexOf(atom.uid);
			atomUIDsWithRigidbodies.RemoveAt(num6);
			sortedAtomUIDsWithRigidbodies.Remove(atom.uid);
		}
		FreeControllerV3[] freeControllers = atom.freeControllers;
		foreach (FreeControllerV3 freeControllerV in freeControllers)
		{
			string key = atom.uid + ":" + freeControllerV.name;
			fcMap.Remove(key);
		}
		ForceProducerV2[] forceProducers = atom.forceProducers;
		foreach (ForceProducerV2 forceProducerV in forceProducers)
		{
			string key2 = atom.uid + ":" + forceProducerV.name;
			fpMap.Remove(key2);
		}
		RhythmController[] rhythmControllers = atom.rhythmControllers;
		foreach (RhythmController rhythmController in rhythmControllers)
		{
			string key3 = atom.uid + ":" + rhythmController.name;
			rcMap.Remove(key3);
		}
		GrabPoint[] grabPoints = atom.grabPoints;
		foreach (GrabPoint grabPoint in grabPoints)
		{
			string key4 = atom.uid + ":" + grabPoint.name;
			gpMap.Remove(key4);
		}
		ForceReceiver[] forceReceivers = atom.forceReceivers;
		foreach (ForceReceiver forceReceiver in forceReceivers)
		{
			string key5 = atom.uid + ":" + forceReceiver.name;
			frMap.Remove(key5);
		}
		Rigidbody[] linkableRigidbodies = atom.linkableRigidbodies;
		foreach (Rigidbody rigidbody in linkableRigidbodies)
		{
			string key6 = atom.uid + ":" + rigidbody.name;
			rbMap.Remove(key6);
		}
		MotionAnimationControl[] motionAnimationControls = atom.motionAnimationControls;
		foreach (MotionAnimationControl motionAnimationControl in motionAnimationControls)
		{
			string key7 = atom.uid + ":" + motionAnimationControl.name;
			macMap.Remove(key7);
		}
		PlayerNavCollider[] playerNavColliders = atom.playerNavColliders;
		foreach (PlayerNavCollider playerNavCollider in playerNavColliders)
		{
			string key8 = atom.uid + ":" + playerNavCollider.name;
			pncMap.Remove(key8);
		}
		atom.uid = text;
		atom.name = text;
		atoms.Add(atom.uid, atom);
		if (num != -1)
		{
			atomUIDs.Insert(num, atom.uid);
			sortedAtomUIDs.Add(atom.uid);
			SyncSortedAtomUIDs();
			SyncHiddenAtoms();
			if (onAtomUIDsChangedHandlers != null)
			{
				onAtomUIDsChangedHandlers(GetAtomUIDs());
			}
		}
		if (num2 != -1)
		{
			atomUIDsWithForceReceivers.Insert(num2, atom.uid);
			sortedAtomUIDsWithForceReceivers.Add(atom.uid);
			SyncSortedAtomUIDsWithForceReceivers();
			if (onAtomUIDsWithForceReceiversChangedHandlers != null)
			{
				onAtomUIDsWithForceReceiversChangedHandlers(GetAtomUIDsWithForceReceivers());
			}
		}
		if (num3 != -1)
		{
			atomUIDsWithForceProducers.Insert(num3, atom.uid);
			sortedAtomUIDsWithForceProducers.Add(atom.uid);
			SyncSortedAtomUIDsWithForceProducers();
			if (onAtomUIDsWithForceProducersChangedHandlers != null)
			{
				onAtomUIDsWithForceProducersChangedHandlers(GetAtomUIDsWithForceProducers());
			}
		}
		if (num4 != -1)
		{
			atomUIDsWithRhythmControllers.Insert(num4, atom.uid);
			sortedAtomUIDsWithRhythmControllers.Add(atom.uid);
			SyncSortedAtomUIDsWithRhythmControllers();
		}
		if (num5 != -1)
		{
			atomUIDsWithFreeControllers.Insert(num5, atom.uid);
			sortedAtomUIDsWithFreeControllers.Add(atom.uid);
			SyncSortedAtomUIDsWithFreeControllers();
			SyncHiddenAtoms();
			if (onAtomUIDsWithFreeControllersChangedHandlers != null)
			{
				onAtomUIDsWithFreeControllersChangedHandlers(GetAtomUIDsWithFreeControllers());
			}
		}
		if (num6 != -1)
		{
			atomUIDsWithRigidbodies.Insert(num6, atom.uid);
			sortedAtomUIDsWithRigidbodies.Add(atom.uid);
			SyncSortedAtomUIDsWithRigidbodies();
			if (onAtomUIDsWithRigidbodiesChangedHandlers != null)
			{
				onAtomUIDsWithRigidbodiesChangedHandlers(GetAtomUIDsWithRigidbodies());
			}
		}
		FreeControllerV3[] freeControllers2 = atom.freeControllers;
		foreach (FreeControllerV3 freeControllerV2 in freeControllers2)
		{
			string key9 = atom.uid + ":" + freeControllerV2.name;
			fcMap.Add(key9, freeControllerV2);
		}
		ForceProducerV2[] forceProducers2 = atom.forceProducers;
		foreach (ForceProducerV2 forceProducerV2 in forceProducers2)
		{
			string key10 = atom.uid + ":" + forceProducerV2.name;
			fpMap.Add(key10, forceProducerV2);
		}
		ForceReceiver[] forceReceivers2 = atom.forceReceivers;
		foreach (ForceReceiver forceReceiver2 in forceReceivers2)
		{
			string key11 = atom.uid + ":" + forceReceiver2.name;
			frMap.Add(key11, forceReceiver2);
		}
		RhythmController[] rhythmControllers2 = atom.rhythmControllers;
		foreach (RhythmController rhythmController2 in rhythmControllers2)
		{
			string key12 = atom.uid + ":" + rhythmController2.name;
			rcMap.Add(key12, rhythmController2);
		}
		GrabPoint[] grabPoints2 = atom.grabPoints;
		foreach (GrabPoint grabPoint2 in grabPoints2)
		{
			string key13 = atom.uid + ":" + grabPoint2.name;
			gpMap.Add(key13, grabPoint2);
		}
		Rigidbody[] linkableRigidbodies2 = atom.linkableRigidbodies;
		foreach (Rigidbody rigidbody2 in linkableRigidbodies2)
		{
			string key14 = atom.uid + ":" + rigidbody2.name;
			rbMap.Add(key14, rigidbody2);
		}
		MotionAnimationControl[] motionAnimationControls2 = atom.motionAnimationControls;
		foreach (MotionAnimationControl motionAnimationControl2 in motionAnimationControls2)
		{
			string key15 = atom.uid + ":" + motionAnimationControl2.name;
			macMap.Add(key15, motionAnimationControl2);
		}
		PlayerNavCollider[] playerNavColliders2 = atom.playerNavColliders;
		foreach (PlayerNavCollider playerNavCollider2 in playerNavColliders2)
		{
			string key16 = atom.uid + ":" + playerNavCollider2.name;
			pncMap.Add(key16, playerNavCollider2);
		}
		if (onAtomUIDRenameHandlers != null)
		{
			onAtomUIDRenameHandlers(uid, text);
		}
		SyncSelectAtomPopup();
	}

	public void RemoveAtom(Atom atom)
	{
		foreach (string atomUID in atomUIDs)
		{
			Atom atomByUid = GetAtomByUid(atomUID);
			if (atomByUid != null && atomByUid.parentAtom == atom)
			{
				atomByUid.parentAtom = null;
			}
		}
		if (selectedController != null && selectedController.containingAtom != null && selectedController.containingAtom == atom)
		{
			ClearSelection();
		}
		if (atom.parentAtom != null)
		{
			atom.parentAtom = null;
		}
		atoms.Remove(atom.uid);
		atomUIDs.Remove(atom.uid);
		sortedAtomUIDs.Remove(atom.uid);
		uids.Remove(atom.uid);
		if (this.playerNavCollider != null && this.playerNavCollider.containingAtom == atom)
		{
			DisconnectNavRigFromPlayerNavCollider();
		}
		if (onAtomUIDsChangedHandlers != null)
		{
			onAtomUIDsChangedHandlers(GetAtomUIDs());
		}
		if (atomUIDsWithForceReceivers.Remove(atom.uid))
		{
			sortedAtomUIDsWithForceReceivers.Remove(atom.uid);
			if (onAtomUIDsWithForceReceiversChangedHandlers != null)
			{
				onAtomUIDsWithForceReceiversChangedHandlers(GetAtomUIDsWithForceReceivers());
			}
		}
		if (atomUIDsWithForceProducers.Remove(atom.uid))
		{
			sortedAtomUIDsWithForceProducers.Remove(atom.uid);
			if (onAtomUIDsWithForceProducersChangedHandlers != null)
			{
				onAtomUIDsWithForceProducersChangedHandlers(GetAtomUIDsWithForceProducers());
			}
		}
		if (atomUIDsWithRhythmControllers.Remove(atom.uid))
		{
			sortedAtomUIDsWithRhythmControllers.Remove(atom.uid);
		}
		if (atomUIDsWithFreeControllers.Remove(atom.uid))
		{
			sortedAtomUIDsWithFreeControllers.Remove(atom.uid);
			if (onAtomUIDsWithFreeControllersChangedHandlers != null)
			{
				onAtomUIDsWithFreeControllersChangedHandlers(GetAtomUIDsWithFreeControllers());
			}
		}
		if (atomUIDsWithRigidbodies.Remove(atom.uid))
		{
			sortedAtomUIDsWithRigidbodies.Remove(atom.uid);
			if (onAtomUIDsWithRigidbodiesChangedHandlers != null)
			{
				onAtomUIDsWithRigidbodiesChangedHandlers(GetAtomUIDsWithRigidbodies());
			}
		}
		SyncHiddenAtoms();
		FreeControllerV3[] freeControllers = atom.freeControllers;
		foreach (FreeControllerV3 freeControllerV in freeControllers)
		{
			allControllers.Remove(freeControllerV);
			string key = atom.uid + ":" + freeControllerV.name;
			fcMap.Remove(key);
		}
		ForceProducerV2[] forceProducers = atom.forceProducers;
		foreach (ForceProducerV2 forceProducerV in forceProducers)
		{
			string key2 = atom.uid + ":" + forceProducerV.name;
			fpMap.Remove(key2);
		}
		RhythmController[] rhythmControllers = atom.rhythmControllers;
		foreach (RhythmController rhythmController in rhythmControllers)
		{
			string key3 = atom.uid + ":" + rhythmController.name;
			rcMap.Remove(key3);
		}
		GrabPoint[] grabPoints = atom.grabPoints;
		foreach (GrabPoint grabPoint in grabPoints)
		{
			string key4 = atom.uid + ":" + grabPoint.name;
			gpMap.Remove(key4);
		}
		ForceReceiver[] forceReceivers = atom.forceReceivers;
		foreach (ForceReceiver forceReceiver in forceReceivers)
		{
			string key5 = atom.uid + ":" + forceReceiver.name;
			frMap.Remove(key5);
		}
		Rigidbody[] linkableRigidbodies = atom.linkableRigidbodies;
		foreach (Rigidbody rigidbody in linkableRigidbodies)
		{
			string key6 = atom.uid + ":" + rigidbody.name;
			rbMap.Remove(key6);
		}
		AnimationPattern[] animationPatterns = atom.animationPatterns;
		foreach (AnimationPattern item in animationPatterns)
		{
			allAnimationPatterns.Remove(item);
		}
		AnimationStep[] animationSteps = atom.animationSteps;
		foreach (AnimationStep item2 in animationSteps)
		{
			allAnimationSteps.Remove(item2);
		}
		Animator[] animators = atom.animators;
		foreach (Animator item3 in animators)
		{
			allAnimators.Remove(item3);
		}
		MotionAnimationControl[] motionAnimationControls = atom.motionAnimationControls;
		foreach (MotionAnimationControl motionAnimationControl in motionAnimationControls)
		{
			string key7 = atom.uid + ":" + motionAnimationControl.name;
			macMap.Remove(key7);
		}
		PlayerNavCollider[] playerNavColliders = atom.playerNavColliders;
		foreach (PlayerNavCollider playerNavCollider in playerNavColliders)
		{
			string key8 = atom.uid + ":" + playerNavCollider.name;
			pncMap.Remove(key8);
		}
		foreach (Canvas canvase in atom.canvases)
		{
			allCanvases.Remove(canvase);
		}
		if (motionAnimationMaster != null)
		{
			MotionAnimationControl[] motionAnimationControls2 = atom.motionAnimationControls;
			foreach (MotionAnimationControl mac in motionAnimationControls2)
			{
				motionAnimationMaster.DeregisterAnimationControl(mac);
			}
		}
		SyncForceReceiverNames();
		SyncForceProducerNames();
		SyncFreeControllerNames();
		SyncRigidbodyNames();
		SyncSelectAtomPopup();
		atom.OnRemove();
		atom.destroyed = true;
		ValidateAllAtoms();
		UnityEngine.Object.DestroyImmediate(atom.gameObject);
	}

	public void ValidateAllAtoms()
	{
		foreach (Atom value in atoms.Values)
		{
			value.Validate();
		}
	}

	private void InitAtom(Atom atom)
	{
		atoms.Add(atom.uid, atom);
		atomUIDs.Add(atom.uid);
		sortedAtomUIDs.Add(atom.uid);
		SyncSortedAtomUIDs();
		bool flag = false;
		FreeControllerV3[] freeControllers = atom.freeControllers;
		foreach (FreeControllerV3 freeControllerV in freeControllers)
		{
			flag = true;
			allControllers.Add(freeControllerV);
			string key = atom.uid + ":" + freeControllerV.name;
			fcMap.Add(key, freeControllerV);
		}
		if (flag)
		{
			atomUIDsWithFreeControllers.Add(atom.uid);
			sortedAtomUIDsWithFreeControllers.Add(atom.uid);
			SyncSortedAtomUIDsWithFreeControllers();
		}
		bool flag2 = false;
		ForceProducerV2[] forceProducers = atom.forceProducers;
		foreach (ForceProducerV2 forceProducerV in forceProducers)
		{
			flag2 = true;
			string key2 = atom.uid + ":" + forceProducerV.name;
			fpMap.Add(key2, forceProducerV);
		}
		if (flag2)
		{
			atomUIDsWithForceProducers.Add(atom.uid);
			sortedAtomUIDsWithForceProducers.Add(atom.uid);
			SyncSortedAtomUIDsWithForceProducers();
		}
		bool flag3 = false;
		ForceReceiver[] forceReceivers = atom.forceReceivers;
		foreach (ForceReceiver forceReceiver in forceReceivers)
		{
			flag3 = true;
			string key3 = atom.uid + ":" + forceReceiver.name;
			frMap.Add(key3, forceReceiver);
		}
		if (flag3)
		{
			atomUIDsWithForceReceivers.Add(atom.uid);
			sortedAtomUIDsWithForceReceivers.Add(atom.uid);
			SyncSortedAtomUIDsWithForceReceivers();
		}
		bool flag4 = false;
		RhythmController[] rhythmControllers = atom.rhythmControllers;
		foreach (RhythmController rhythmController in rhythmControllers)
		{
			flag4 = true;
			string key4 = atom.uid + ":" + rhythmController.name;
			rcMap.Add(key4, rhythmController);
		}
		if (flag4)
		{
			atomUIDsWithRhythmControllers.Add(atom.uid);
			sortedAtomUIDsWithRhythmControllers.Add(atom.uid);
			SyncSortedAtomUIDsWithRhythmControllers();
		}
		GrabPoint[] grabPoints = atom.grabPoints;
		foreach (GrabPoint grabPoint in grabPoints)
		{
			string key5 = atom.uid + ":" + grabPoint.name;
			gpMap.Add(key5, grabPoint);
		}
		bool flag5 = false;
		Rigidbody[] rigidbodies = atom.rigidbodies;
		foreach (Rigidbody rigidbody in rigidbodies)
		{
			rigidbody.maxAngularVelocity = maxAngularVelocity;
			rigidbody.maxDepenetrationVelocity = maxDepenetrationVelocity;
			rigidbody.solverIterations = _solverIterations;
		}
		PhysicsSimulator[] physicsSimulators = atom.physicsSimulators;
		foreach (PhysicsSimulator physicsSimulator in physicsSimulators)
		{
			physicsSimulator.solverIterations = _solverIterations;
		}
		PhysicsSimulatorJSONStorable[] physicsSimulatorsStorable = atom.physicsSimulatorsStorable;
		foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in physicsSimulatorsStorable)
		{
			physicsSimulatorJSONStorable.solverIterations = _solverIterations;
		}
		Rigidbody[] linkableRigidbodies = atom.linkableRigidbodies;
		foreach (Rigidbody rigidbody2 in linkableRigidbodies)
		{
			flag5 = true;
			string key6 = atom.uid + ":" + rigidbody2.name;
			rbMap.Add(key6, rigidbody2);
		}
		if (flag5)
		{
			atomUIDsWithRigidbodies.Add(atom.uid);
			sortedAtomUIDsWithRigidbodies.Add(atom.uid);
			SyncSortedAtomUIDsWithRigidbodies();
		}
		AnimationPattern[] animationPatterns = atom.animationPatterns;
		foreach (AnimationPattern item in animationPatterns)
		{
			allAnimationPatterns.Add(item);
		}
		AnimationStep[] animationSteps = atom.animationSteps;
		foreach (AnimationStep item2 in animationSteps)
		{
			allAnimationSteps.Add(item2);
		}
		Animator[] animators = atom.animators;
		foreach (Animator item3 in animators)
		{
			allAnimators.Add(item3);
		}
		foreach (Canvas canvase in atom.canvases)
		{
			if (overrideCanvasSortingLayer)
			{
				IgnoreCanvas component = canvase.GetComponent<IgnoreCanvas>();
				if (component == null)
				{
					canvase.sortingLayerName = overrideCanvasSortingLayerName;
				}
			}
			allCanvases.Add(canvase);
		}
		if (motionAnimationMaster != null)
		{
			MotionAnimationControl[] motionAnimationControls = atom.motionAnimationControls;
			foreach (MotionAnimationControl motionAnimationControl in motionAnimationControls)
			{
				string key7 = atom.uid + ":" + motionAnimationControl.name;
				macMap.Add(key7, motionAnimationControl);
				motionAnimationMaster.RegisterAnimationControl(motionAnimationControl);
			}
		}
		PlayerNavCollider[] playerNavColliders = atom.playerNavColliders;
		foreach (PlayerNavCollider playerNavCollider in playerNavColliders)
		{
			string key8 = atom.uid + ":" + playerNavCollider.name;
			pncMap.Add(key8, playerNavCollider);
		}
		SyncHiddenAtoms();
		atom.useRigidbodyInterpolation = _useInterpolation;
	}

	public void SetAtomAssetsFromFile()
	{
		if (atomAssetsFile == null || !(atomAssetsFile != string.Empty))
		{
			return;
		}
		string text = File.ReadAllText(atomAssetsFile);
		string[] array = text.Split('\n');
		List<AtomAsset> list = new List<AtomAsset>();
		string[] array2 = array;
		foreach (string input in array2)
		{
			string[] array3 = Regex.Split(input, "\\s+");
			if (array3.Length >= 3)
			{
				AtomAsset atomAsset = new AtomAsset();
				atomAsset.assetBundleName = array3[0];
				atomAsset.assetName = array3[1];
				atomAsset.category = array3[2];
				list.Add(atomAsset);
			}
		}
		atomAssets = list.ToArray();
	}

	private void InitAtoms()
	{
		atomPrefabByType = new Dictionary<string, Atom>();
		atomAssetByType = new Dictionary<string, AtomAsset>();
		AtomAsset[] array = atomAssets;
		foreach (AtomAsset atomAsset in array)
		{
			if (atomAsset != null)
			{
				string assetName = atomAsset.assetName;
				if (!atomAssetByType.ContainsKey(assetName))
				{
					atomAssetByType.Add(assetName, atomAsset);
				}
				else
				{
					Error("Atom asset " + assetName + " is a duplicate");
				}
			}
		}
		AtomAsset[] array2 = indirectAtomAssets;
		foreach (AtomAsset atomAsset2 in array2)
		{
			if (atomAsset2 != null)
			{
				string assetName2 = atomAsset2.assetName;
				if (!atomAssetByType.ContainsKey(assetName2))
				{
					atomAssetByType.Add(assetName2, atomAsset2);
				}
				else
				{
					Error("Atom asset " + assetName2 + " is a duplicate");
				}
			}
		}
		Atom[] array3 = atomPrefabs;
		foreach (Atom atom in array3)
		{
			if (atom != null)
			{
				string type = atom.type;
				if (!atomPrefabByType.ContainsKey(type))
				{
					atomPrefabByType.Add(type, atom);
					continue;
				}
				Error("Atom " + atom.name + " uses type " + type + " that is already used");
			}
		}
		Atom[] array4 = indirectAtomPrefabs;
		foreach (Atom atom2 in array4)
		{
			if (atom2 != null)
			{
				string type2 = atom2.type;
				if (!atomPrefabByType.ContainsKey(type2))
				{
					atomPrefabByType.Add(type2, atom2);
					continue;
				}
				Error("Atom " + atom2.name + " uses type " + type2 + " that is already used");
			}
		}
		atomTypes = new List<string>();
		atomCategories = new List<string>();
		atomCategoryToAtomTypes = new Dictionary<string, List<string>>();
		Atom[] array5 = atomPrefabs;
		foreach (Atom atom3 in array5)
		{
			atomTypes.Add(atom3.type);
			if (atomCategoryToAtomTypes.TryGetValue(atom3.category, out var value))
			{
				value.Add(atom3.type);
				continue;
			}
			atomCategories.Add(atom3.category);
			value = new List<string>();
			value.Add(atom3.type);
			atomCategoryToAtomTypes.Add(atom3.category, value);
		}
		AtomAsset[] array6 = atomAssets;
		foreach (AtomAsset atomAsset3 in array6)
		{
			atomTypes.Add(atomAsset3.assetName);
			if (atomCategoryToAtomTypes.TryGetValue(atomAsset3.category, out var value2))
			{
				value2.Add(atomAsset3.assetName);
				continue;
			}
			atomCategories.Add(atomAsset3.category);
			value2 = new List<string>();
			value2.Add(atomAsset3.assetName);
			atomCategoryToAtomTypes.Add(atomAsset3.category, value2);
		}
		atomTypes.Sort();
		atomCategories.Sort();
		foreach (List<string> value3 in atomCategoryToAtomTypes.Values)
		{
			value3.Sort();
		}
		if (atomCategoryPopup != null)
		{
			atomCategoryPopup.currentValue = "None";
			atomCategoryPopup.numPopupValues = atomCategories.Count;
			for (int num = 0; num < atomCategories.Count; num++)
			{
				atomCategoryPopup.setPopupValue(num, atomCategories[num]);
			}
			UIPopup uIPopup = atomCategoryPopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetAddAtomAtomPopupValues));
		}
		if (atomPrefabPopup != null)
		{
			atomPrefabPopup.currentValue = "None";
		}
		atoms = new Dictionary<string, Atom>();
		atomUIDs = new List<string>();
		atomUIDsWithForceReceivers = new List<string>();
		atomUIDsWithForceProducers = new List<string>();
		atomUIDsWithRhythmControllers = new List<string>();
		atomUIDsWithFreeControllers = new List<string>();
		atomUIDsWithRigidbodies = new List<string>();
		sortedAtomUIDs = new List<string>();
		sortedAtomUIDsWithForceReceivers = new List<string>();
		sortedAtomUIDsWithForceProducers = new List<string>();
		sortedAtomUIDsWithRhythmControllers = new List<string>();
		sortedAtomUIDsWithFreeControllers = new List<string>();
		sortedAtomUIDsWithRigidbodies = new List<string>();
		hiddenAtomUIDs = new List<string>();
		hiddenAtomUIDsWithFreeControllers = new List<string>();
		visibleAtomUIDs = new List<string>();
		visibleAtomUIDsWithFreeControllers = new List<string>();
		uids = new Dictionary<string, bool>();
		frMap = new Dictionary<string, ForceReceiver>();
		fpMap = new Dictionary<string, ForceProducerV2>();
		rcMap = new Dictionary<string, RhythmController>();
		gpMap = new Dictionary<string, GrabPoint>();
		fcMap = new Dictionary<string, FreeControllerV3>();
		rbMap = new Dictionary<string, Rigidbody>();
		macMap = new Dictionary<string, MotionAnimationControl>();
		pncMap = new Dictionary<string, PlayerNavCollider>();
		allControllers = new List<FreeControllerV3>();
		allAnimationPatterns = new List<AnimationPattern>();
		allAnimationSteps = new List<AnimationStep>();
		allAnimators = new List<Animator>();
		allCanvases = new List<Canvas>();
		Atom[] componentsInChildren = atomContainerTransform.GetComponentsInChildren<Atom>();
		startingAtoms = new Dictionary<string, Atom>();
		Atom[] array7 = componentsInChildren;
		foreach (Atom atom4 in array7)
		{
			string text = CreateUID(atom4.name);
			if (text != null)
			{
				atom4.uid = text;
				atom4.name = text;
				InitAtom(atom4);
				startingAtoms.Add(text, atom4);
			}
		}
		if (onAtomUIDsChangedHandlers != null)
		{
			onAtomUIDsChangedHandlers(GetAtomUIDs());
		}
		if (onAtomUIDsWithForceReceiversChangedHandlers != null)
		{
			onAtomUIDsWithForceReceiversChangedHandlers(GetAtomUIDsWithForceReceivers());
		}
		if (onAtomUIDsWithForceProducersChangedHandlers != null)
		{
			onAtomUIDsWithForceProducersChangedHandlers(GetAtomUIDsWithForceProducers());
		}
		if (onAtomUIDsWithFreeControllersChangedHandlers != null)
		{
			onAtomUIDsWithFreeControllersChangedHandlers(GetAtomUIDsWithFreeControllers());
		}
		if (onAtomUIDsWithRigidbodiesChangedHandlers != null)
		{
			onAtomUIDsWithRigidbodiesChangedHandlers(GetAtomUIDsWithRigidbodies());
		}
		SyncForceReceiverNames();
		SyncForceProducerNames();
		SyncFreeControllerNames();
		SyncRigidbodyNames();
		SyncSelectAtomPopup();
	}

	protected void SyncNavigationHologridVisibility()
	{
		if (navigationHologridShowTime > 0f)
		{
			navigationHologridTransparencyMultiplier = 5f;
		}
		else
		{
			navigationHologridTransparencyMultiplier = 1f;
		}
		if (navigationHologridVisible || navigationHologridShowTime > 0f)
		{
			if (!navigationHologrid.gameObject.activeSelf)
			{
				navigationHologrid.gameObject.SetActive(value: true);
				SyncHologridTransparency();
			}
		}
		else if (navigationHologrid.gameObject.activeSelf)
		{
			navigationHologrid.gameObject.SetActive(value: false);
		}
		if (navigationHologridShowTime > 0f)
		{
			navigationHologridShowTime -= Time.unscaledDeltaTime;
		}
	}

	protected void SplashNavigationHologrid(float seconds)
	{
		navigationHologridShowTime = seconds;
		SyncNavigationHologridVisibility();
	}

	private void SyncHologridTransparency()
	{
		if (!(navigationHologrid != null))
		{
			return;
		}
		MeshRenderer component = navigationHologrid.GetComponent<MeshRenderer>();
		if (component != null)
		{
			Material material = component.material;
			if (material != null)
			{
				material.SetFloat("_Alpha", _hologridTransparency * navigationHologridTransparencyMultiplier);
			}
		}
	}

	public void SetSceneLoadPosition()
	{
		if (navigationRig != null)
		{
			sceneLoadPlayerHeightAdjust = _playerHeightAdjust;
			sceneLoadPosition = navigationRig.position;
			sceneLoadRotation = navigationRig.rotation;
		}
	}

	public void MoveToSceneLoadPosition()
	{
		if (navigationRig != null)
		{
			navigationRig.position = sceneLoadPosition;
			navigationRig.rotation = sceneLoadRotation;
			playerHeightAdjust = sceneLoadPlayerHeightAdjust;
		}
	}

	public void SetUseSceneLoadPosition(bool b)
	{
		useSceneLoadPosition = b;
	}

	private void SyncLockHeightDuringNavigate()
	{
		if (lockHeightDuringNavigateToggle != null)
		{
			lockHeightDuringNavigateToggle.isOn = _lockHeightDuringNavigate;
		}
		if (lockHeightDuringNavigateToggleAlt != null)
		{
			lockHeightDuringNavigateToggleAlt.isOn = _lockHeightDuringNavigate;
		}
	}

	private void SyncDisableAllNavigation()
	{
		if (disableAllNavigationToggle != null)
		{
			disableAllNavigationToggle.isOn = _disableAllNavigation;
		}
	}

	public void ToggleDisableAllNavigation()
	{
		disableAllNavigation = !_disableAllNavigation;
	}

	private void SyncFreeMoveFollowFloor()
	{
		if (freeMoveFollowFloorToggle != null)
		{
			freeMoveFollowFloorToggle.isOn = _freeMoveFollowFloor;
		}
		if (freeMoveFollowFloorToggleAlt != null)
		{
			freeMoveFollowFloorToggleAlt.isOn = _freeMoveFollowFloor;
		}
	}

	private void SyncTeleportAllowRotation()
	{
		if (teleportAllowRotationToggle != null)
		{
			teleportAllowRotationToggle.isOn = _teleportAllowRotation;
		}
	}

	private void SyncDisableTeleport()
	{
		if (disableTeleportToggle != null)
		{
			disableTeleportToggle.isOn = _disableTeleport;
		}
	}

	private void SyncDisableTeleportDuringPossess()
	{
		if (disableTeleportDuringPossessToggle != null)
		{
			disableTeleportDuringPossessToggle.isOn = _disableTeleportDuringPossess;
		}
	}

	private void SyncFreeMoveMultiplier()
	{
		if (freeMoveMultiplierSlider != null)
		{
			freeMoveMultiplierSlider.value = _freeMoveMultiplier;
		}
	}

	private void SyncDisableGrabNavigation()
	{
		if (disableGrabNavigationToggle != null)
		{
			disableGrabNavigationToggle.isOn = _disableGrabNavigation;
		}
	}

	private void SyncGrabNavigationPositionMultiplier()
	{
		if (grabNavigationPositionMultiplierSlider != null)
		{
			grabNavigationPositionMultiplierSlider.value = _grabNavigationPositionMultiplier;
		}
	}

	private void SyncGrabNavigationRotationMultiplier()
	{
		if (grabNavigationRotationMultiplierSlider != null)
		{
			grabNavigationRotationMultiplierSlider.value = _grabNavigationRotationMultiplier;
		}
	}

	private void SyncPlayerHeightAdjust()
	{
		if (heightAdjustTransform != null && navigationRig != null)
		{
			Vector3 localPosition = heightAdjustTransform.localPosition;
			localPosition.y = _playerHeightAdjust / _worldScale;
			heightAdjustTransform.localPosition = localPosition;
		}
	}

	public void playerHeightAdjustAdjust(float val)
	{
		playerHeightAdjust += val;
	}

	private void InitMotionControllerNaviation()
	{
		if (!(navigationPlayArea != null))
		{
			return;
		}
		if (regularPlayArea != null)
		{
			regularPlayAreaMR = regularPlayArea.GetComponent<MeshRenderer>();
		}
		if (regularPlayAreaMR != null)
		{
			regularPlayAreaMR.enabled = false;
		}
		navigationPlayAreaMR = navigationPlayArea.GetComponent<MeshRenderer>();
		if (navigationPlayAreaMR != null)
		{
			navigationPlayAreaMR.enabled = false;
		}
		if (navigationCurve != null)
		{
			navigationCurve.draw = false;
		}
		navigationPlayerMR = null;
		navigationCameraMR = null;
		if (!(lookCamera != null))
		{
			return;
		}
		if (navigationPlayer != null)
		{
			navigationPlayerMR = navigationPlayer.GetComponent<MeshRenderer>();
			if (navigationPlayerMR != null)
			{
				navigationPlayerMR.enabled = false;
			}
		}
		if (navigationCamera != null)
		{
			navigationCameraMR = navigationCamera.GetComponentInChildren<MeshRenderer>();
			if (navigationCameraMR != null)
			{
				navigationCameraMR.enabled = false;
			}
		}
	}

	private void ProcessTeleportMode()
	{
		if (navigationPlayArea != null)
		{
			if (regularPlayAreaMR != null)
			{
				regularPlayAreaMR.enabled = false;
			}
			if (navigationPlayAreaMR != null)
			{
				navigationPlayAreaMR.enabled = false;
			}
			if (navigationPlayerMR != null)
			{
				navigationPlayerMR.enabled = false;
			}
			if (navigationCameraMR != null)
			{
				navigationCameraMR.enabled = false;
			}
			if (navigationCurve != null)
			{
				navigationCurve.draw = false;
			}
		}
		if (GetTeleportStart(inTeleportMode: true))
		{
			ProcessTeleportStart();
		}
		if (GetTeleportShowLeft(inTeleportMode: true))
		{
			ProcessTeleportShow(isLeft: true);
		}
		else if (GetTeleportShowRight(inTeleportMode: true))
		{
			ProcessTeleportShow(isLeft: false);
		}
		if (GetTeleportFinish(inTeleportMode: true))
		{
			ProcessTeleportFinish();
		}
		if (GetCancel())
		{
			SelectModeOff();
		}
	}

	private void ProcessTeleportStart()
	{
		if (navigationRig != null)
		{
			isTeleporting = true;
			startNavigateRotation = navigationRig.rotation;
		}
	}

	private void ProcessTeleportShow(bool isLeft)
	{
		if (navigationPlayArea != null && navigationRig != null)
		{
			navigationPlayArea.rotation = navigationRig.rotation;
		}
		if (regularPlayAreaMR != null)
		{
			regularPlayAreaMR.enabled = true;
		}
		if (navigationPlayAreaMR != null)
		{
			navigationPlayAreaMR.enabled = true;
		}
		if (navigationPlayerMR != null)
		{
			navigationPlayerMR.enabled = true;
		}
		if (navigationCameraMR != null)
		{
			navigationCameraMR.enabled = true;
		}
		bool flag = false;
		if (useLookForNavigation && lookCamera != null)
		{
			castRay.origin = lookCamera.transform.position;
			castRay.direction = lookCamera.transform.forward;
		}
		else if (isLeft)
		{
			castRay.origin = motionControllerLeft.position;
			castRay.direction = motionControllerLeft.forward;
		}
		else
		{
			castRay.origin = motionControllerRight.position;
			castRay.direction = motionControllerRight.forward;
			flag = true;
		}
		AllocateRaycastHits();
		int num = Physics.RaycastNonAlloc(castRay, raycastHits, navigationDistance, navigationColliderMask);
		if (num <= 0)
		{
			return;
		}
		int num2 = -1;
		float num3 = navigationDistance;
		for (int i = 0; i < num; i++)
		{
			float magnitude = (raycastHits[i].point - castRay.origin).magnitude;
			if (magnitude < num3)
			{
				num2 = i;
				num3 = magnitude;
			}
		}
		if (lookCamera != null && navigationPlayArea != null)
		{
			if (navigationPlayer != null)
			{
				Vector3 localPosition = lookCamera.transform.localPosition;
				localPosition.y = 0f;
				navigationPlayer.localPosition = localPosition;
			}
			if (navigationCamera != null)
			{
				navigationCamera.localRotation = lookCamera.transform.localRotation;
			}
			Vector3 vector = navigationPlayer.position - navigationPlayArea.position;
			navigationPlayArea.position = raycastHits[num2].point - vector;
			Collider collider = raycastHits[num2].collider;
			PlayerNavCollider playerNavCollider = (teleportPlayerNavCollider = collider.GetComponent<PlayerNavCollider>());
			if (_teleportAllowRotation)
			{
				navigationPlayArea.rotation = startNavigateRotation;
				if (playerNavCollider != null)
				{
					Quaternion quaternion = Quaternion.FromToRotation(navigationPlayArea.up, playerNavCollider.transform.up);
					navigationPlayArea.rotation = quaternion * navigationPlayArea.rotation;
				}
				Vector3 vector2 = ((!isLeft) ? (Quaternion2Angles.GetAngles(Quaternion.Inverse(lookCamera.transform.rotation) * motionControllerRight.rotation, Quaternion2Angles.RotationOrder.ZXY) * 57.29578f) : (Quaternion2Angles.GetAngles(Quaternion.Inverse(lookCamera.transform.rotation) * motionControllerLeft.rotation, Quaternion2Angles.RotationOrder.ZXY) * 57.29578f));
				navigationPlayArea.Rotate(Vector3.up, (0f - vector2.z) * 2f);
			}
			else if (playerNavCollider != null)
			{
				Quaternion quaternion2 = Quaternion.FromToRotation(navigationPlayArea.up, playerNavCollider.transform.up);
				navigationPlayArea.rotation = quaternion2 * navigationPlayArea.rotation;
			}
		}
		if (navigationCurve != null && navigationCurve.points != null && navigationCurve.points.Length == 3)
		{
			if (useLookForNavigation)
			{
				navigationCurve.points[0].transform.position = lookCamera.transform.position;
			}
			else if (flag)
			{
				navigationCurve.points[0].transform.position = motionControllerRight.position;
			}
			else
			{
				navigationCurve.points[0].transform.position = motionControllerLeft.position;
			}
			if (navigationPlayer != null)
			{
				navigationCurve.points[2].transform.position = navigationPlayer.position;
			}
			else
			{
				navigationCurve.points[2].transform.position = raycastHits[num2].point;
			}
			Vector3 position = (navigationCurve.points[0].transform.position + navigationCurve.points[2].transform.position) * 0.5f;
			position.y += 1f * navigationCurve.transform.lossyScale.y;
			navigationCurve.points[1].transform.position = position;
			navigationCurve.draw = true;
		}
	}

	private void DisconnectNavRigFromPlayerNavCollider()
	{
		if (playerNavCollider != null)
		{
			playerNavTrackerGO.transform.SetParent(null);
			playerNavCollider = null;
		}
	}

	private void ProcessPlayerNavMove()
	{
		if (playerNavCollider != null && navigationRigParent != null)
		{
			navigationRigParent.transform.position = playerNavTrackerGO.transform.position;
			navigationRigParent.transform.rotation = playerNavTrackerGO.transform.rotation;
		}
	}

	private void ConnectNavRigToPlayerNavCollider()
	{
		if (navigationRigParent != null)
		{
			navigationRigParent.position = navigationRig.position;
			navigationRigParent.rotation = navigationRig.rotation;
			navigationRig.position = navigationRigParent.position;
			navigationRig.rotation = navigationRigParent.rotation;
			if ((bool)playerNavCollider)
			{
				playerNavTrackerGO.transform.SetParent(playerNavCollider.transform);
				playerNavTrackerGO.transform.position = navigationRigParent.position;
				playerNavTrackerGO.transform.rotation = navigationRigParent.rotation;
			}
			else
			{
				playerNavTrackerGO.transform.SetParent(null);
			}
		}
	}

	private void ProcessTeleportFinish()
	{
		if (isTeleporting)
		{
			navigationRig.position = navigationPlayArea.position;
			navigationRig.rotation = navigationPlayArea.rotation;
			playerNavCollider = teleportPlayerNavCollider;
			ConnectNavRigToPlayerNavCollider();
		}
		isTeleporting = false;
	}

	private void ProcessMotionControllerNavigation()
	{
		if (isMonitorOnly)
		{
			return;
		}
		didStartLeftNavigate = false;
		didStartRightNavigate = false;
		if (navigationPlayArea != null)
		{
			if (regularPlayAreaMR != null)
			{
				regularPlayAreaMR.enabled = false;
			}
			if (navigationPlayAreaMR != null)
			{
				navigationPlayAreaMR.enabled = false;
			}
			if (navigationPlayerMR != null)
			{
				navigationPlayerMR.enabled = false;
			}
			if (navigationCameraMR != null)
			{
				navigationCameraMR.enabled = false;
			}
			if (navigationCurve != null)
			{
				navigationCurve.draw = false;
			}
		}
		if (navigationDisabled || _disableAllNavigation || !(navigationRig != null))
		{
			return;
		}
		bool flag = GetLeftSelect() && highlightedControllersLeft.Count > 0;
		bool flag2 = GetRightSelect() && highlightedControllersRight.Count > 0;
		if (!_disableGrabNavigation && GetGrabNavigateStartLeft() && !flag)
		{
			startGrabNavigatePositionLeft = motionControllerLeft.position;
			startGrabNavigateRotationLeft = motionControllerLeft.rotation;
			isGrabNavigatingLeft = true;
			didStartLeftNavigate = true;
		}
		if (isGrabNavigatingLeft && GetGrabNavigateLeft())
		{
			Vector3 position = navigationRig.position;
			position += (startGrabNavigatePositionLeft - motionControllerLeft.position) * _grabNavigationPositionMultiplier;
			Vector3 up = navigationRig.up;
			float num = Vector3.Dot(position - navigationRig.position, up);
			position += up * (0f - num);
			navigationRig.position = position;
			if (!_lockHeightDuringNavigate)
			{
				playerHeightAdjust += num;
			}
			startGrabNavigatePositionLeft = motionControllerLeft.position;
			float num2 = (Quaternion2Angles.GetAngles(motionControllerLeft.rotation * Quaternion.Inverse(startGrabNavigateRotationLeft), Quaternion2Angles.RotationOrder.ZXY) * 57.29578f).y * _grabNavigationRotationMultiplier;
			if (num2 > 0f)
			{
				num2 -= _grabNavigationRotationResistance;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
			}
			if (num2 < 0f)
			{
				num2 += _grabNavigationRotationResistance;
				if (num2 > 0f)
				{
					num2 = 0f;
				}
			}
			navigationRig.RotateAround(lookCamera.transform.position, navigationRig.up, 0f - num2);
			startGrabNavigateRotationLeft = motionControllerLeft.rotation;
		}
		else
		{
			isGrabNavigatingLeft = false;
		}
		if (!_disableGrabNavigation && GetGrabNavigateStartRight() && !flag2)
		{
			startGrabNavigatePositionRight = motionControllerRight.position;
			startGrabNavigateRotationRight = motionControllerRight.rotation;
			isGrabNavigatingRight = true;
			didStartRightNavigate = true;
		}
		if (isGrabNavigatingRight && GetGrabNavigateRight())
		{
			Vector3 position2 = navigationRig.position;
			position2 += (startGrabNavigatePositionRight - motionControllerRight.position) * _grabNavigationPositionMultiplier;
			Vector3 up2 = navigationRig.up;
			float num3 = Vector3.Dot(position2 - navigationRig.position, up2);
			position2 += up2 * (0f - num3);
			navigationRig.position = position2;
			if (!_lockHeightDuringNavigate)
			{
				playerHeightAdjust += num3;
			}
			startGrabNavigatePositionRight = motionControllerRight.position;
			float num4 = (Quaternion2Angles.GetAngles(motionControllerRight.rotation * Quaternion.Inverse(startGrabNavigateRotationRight), Quaternion2Angles.RotationOrder.ZXY) * 57.29578f).y * _grabNavigationRotationMultiplier;
			if (num4 > 0f)
			{
				num4 -= _grabNavigationRotationResistance;
				if (num4 < 0f)
				{
					num4 = 0f;
				}
			}
			if (num4 < 0f)
			{
				num4 += _grabNavigationRotationResistance;
				if (num4 > 0f)
				{
					num4 = 0f;
				}
			}
			navigationRig.RotateAround(lookCamera.transform.position, navigationRig.up, 0f - num4);
			startGrabNavigateRotationRight = motionControllerRight.rotation;
		}
		else
		{
			isGrabNavigatingRight = false;
		}
		if (navigationHologrid != null)
		{
			if (_showNavigationHologrid && (isGrabNavigatingLeft || isGrabNavigatingRight))
			{
				navigationHologridVisible = true;
			}
			else
			{
				navigationHologridVisible = false;
			}
		}
		if (!_disableTeleport && (!_disableTeleportDuringPossess || (leftPossessedController == null && rightPossessedController == null && headPossessedController == null)))
		{
			if (GetTeleportStartLeft() && !flag)
			{
				ProcessTeleportStart();
				didStartLeftNavigate = true;
			}
			if (GetTeleportStartRight() && !flag2)
			{
				ProcessTeleportStart();
				didStartRightNavigate = true;
			}
			if (GetTeleportShowLeft() && highlightedControllersLeft.Count == 0)
			{
				ProcessTeleportShow(isLeft: true);
			}
			else if (GetTeleportShowRight() && highlightedControllersRight.Count == 0)
			{
				ProcessTeleportShow(isLeft: false);
			}
			if (GetTeleportFinish())
			{
				ProcessTeleportFinish();
			}
		}
	}

	private void AdjustNavigationRigHeight()
	{
		if (!(navigationRig != null) || !(lookCamera != null) || !_freeMoveFollowFloor)
		{
			return;
		}
		Vector3 position = navigationRig.position;
		Plane plane = new Plane(navigationRig.up, navigationRig.transform.position);
		castRay.origin = lookCamera.transform.position;
		castRay.direction = -navigationRig.transform.up;
		if (!plane.Raycast(castRay, out var enter))
		{
			castRay.direction = navigationRig.transform.up;
			plane.Raycast(castRay, out enter);
		}
		castRay.origin = (castRay.GetPoint(enter) + lookCamera.transform.position) * 0.5f;
		castRay.direction = -navigationRig.up;
		float num = navigationDistance;
		Vector3 direction = castRay.direction;
		Vector3 vector = navigationRig.position;
		bool flag = false;
		AllocateRaycastHits();
		int num2 = Physics.RaycastNonAlloc(castRay, raycastHits, navigationDistance, navigationColliderMask);
		if (num2 > 0)
		{
			flag = true;
			for (int i = 0; i < num2; i++)
			{
				float magnitude = (raycastHits[i].point - castRay.origin).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					vector = raycastHits[i].point;
				}
			}
		}
		castRay.direction = navigationRig.up;
		num2 = Physics.RaycastNonAlloc(castRay, raycastHits, navigationDistance, navigationColliderMask);
		if (num2 > 0)
		{
			for (int j = 0; j < num2; j++)
			{
				float magnitude2 = (raycastHits[j].point - castRay.origin).magnitude;
				if (magnitude2 < num)
				{
					direction = castRay.direction;
					num = magnitude2;
					vector = raycastHits[j].point;
				}
			}
		}
		if (flag)
		{
			Vector3 vector2 = direction * Vector3.Dot(vector - navigationRig.position, direction);
			navigationRig.position = Vector3.Lerp(navigationRig.position, navigationRig.position + vector2, Time.deltaTime * 2f);
		}
	}

	private void ProcessControllerNavigation(SteamVR_Action_Vector2 moveAction, bool ignoreDisable = false)
	{
		CheckSwapAxis();
		if (navigationRig != null && lookCamera != null && !navigationDisabled && !_disableAllNavigation)
		{
			Vector4 freeNavigateVector = GetFreeNavigateVector(moveAction, ignoreDisable);
			bool flag = false;
			if (freeNavigateVector.x > 0.01f || freeNavigateVector.x < -0.01f)
			{
				Vector3 vector = Vector3.ProjectOnPlane(lookCamera.transform.right, navigationRig.up);
				vector.Normalize();
				Vector3 position = navigationRig.position;
				position += vector * (freeNavigateVector.x * 0.5f * Time.unscaledDeltaTime) * _freeMoveMultiplier;
				navigationRig.position = position;
				flag = true;
			}
			if (freeNavigateVector.y > 0.01f || freeNavigateVector.y < -0.01f)
			{
				Vector3 vector2 = Vector3.ProjectOnPlane(lookCamera.transform.forward, navigationRig.up);
				vector2.Normalize();
				Vector3 position2 = navigationRig.position;
				position2 += vector2 * (freeNavigateVector.y * 0.5f * Time.unscaledDeltaTime) * _freeMoveMultiplier;
				navigationRig.position = position2;
				flag = true;
			}
			if (freeNavigateVector.z > 0.01f || freeNavigateVector.z < -0.01f)
			{
				navigationRig.RotateAround(lookCamera.transform.position, navigationRig.up, freeNavigateVector.z * 50f * Time.unscaledDeltaTime);
				flag = true;
			}
			if ((freeNavigateVector.w > 0.01f || freeNavigateVector.w < -0.01f) && !_lockHeightDuringNavigate)
			{
				playerHeightAdjust += freeNavigateVector.w * 0.5f * Time.unscaledDeltaTime * _freeMoveMultiplier;
				flag = true;
			}
			if (flag)
			{
				AdjustNavigationRigHeight();
			}
		}
	}

	public void FocusOnSelectedController()
	{
		if (MonitorCenterCamera != null && selectedController != null)
		{
			AlignRigFacingSelectedController(rotationOnly: true);
			Vector3 position;
			if (selectedController.focusPoint != null)
			{
				position = selectedController.focusPoint.position;
				focusDistance = (selectedController.focusPoint.position - MonitorCenterCamera.transform.position).magnitude;
			}
			else
			{
				position = selectedController.transform.position;
				focusDistance = (selectedController.transform.position - MonitorCenterCamera.transform.position).magnitude;
			}
			if (MonitorCenterCamera != null)
			{
				MonitorCenterCamera.transform.LookAt(position);
				Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
				localEulerAngles.y = 0f;
				localEulerAngles.z = 0f;
				MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
			}
		}
	}

	public void ResetFocusPoint()
	{
		focusDistance = 1.5f;
	}

	public void ResetMonitorCenterCamera()
	{
		ResetFocusPoint();
		if (MonitorCenterCamera != null)
		{
			MonitorCenterCamera.transform.localEulerAngles = Vector3.zero;
		}
	}

	private void ProcessFreeMoveNavigation()
	{
		ProcessControllerNavigation(freeModeMoveAction, ignoreDisable: true);
		if (GetCancel())
		{
			SelectModeOff();
		}
	}

	private void ProcessKeyBindings()
	{
		if (!(LookInputModule.singleton == null) && LookInputModule.singleton.inputFieldActive)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			ToggleMainMonitor();
		}
		if (Input.GetKeyDown(KeyCode.F1) && !UIDisabled)
		{
			ToggleMonitorUI();
		}
		if (!MonitorRigActive)
		{
			return;
		}
		if (!navigationDisabled)
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				SelectModeFreeMoveMouse();
			}
			if (Input.GetKeyDown(KeyCode.F))
			{
				FocusOnSelectedController();
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				ResetFocusPoint();
			}
		}
		if (!UIDisabled)
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				gameMode = GameMode.Edit;
			}
			if (Input.GetKeyDown(KeyCode.P))
			{
				gameMode = GameMode.Play;
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				CycleSelectAtomOfType("Person");
			}
			if (Input.GetKeyDown(KeyCode.U))
			{
				ToggleMainHUDMonitor();
			}
			if (Input.GetKeyDown(KeyCode.T))
			{
				ToggleTargetsOnWithButton();
			}
			if (Input.GetKeyDown(KeyCode.H))
			{
				ToggleShowHiddenAtoms();
			}
			if (Input.GetKeyDown(KeyCode.C))
			{
				ProcessTargetSelectionCycleSelect(highlightedControllersMouse);
			}
		}
	}

	private void ProcessMouseControl()
	{
		mouseClickUsed = false;
		if (!(navigationRig != null) || !MonitorRigActive || navigationDisabled)
		{
			return;
		}
		if (Input.GetMouseButtonDown(1))
		{
		}
		if (Input.GetMouseButtonUp(1))
		{
		}
		if (Input.GetMouseButton(1))
		{
			Vector3 vector = MonitorCenterCamera.transform.position + MonitorCenterCamera.transform.forward * focusDistance;
			float axisRaw = Input.GetAxisRaw("Mouse X");
			axisRaw = Mathf.Clamp(axisRaw, -10f, 10f);
			if (axisRaw > 0.01f || axisRaw < -0.01f)
			{
				navigationRig.RotateAround(vector, navigationRig.up, axisRaw * 2f);
			}
			float axisRaw2 = Input.GetAxisRaw("Mouse Y");
			axisRaw2 = Mathf.Clamp(axisRaw2, -10f, 10f);
			if ((axisRaw2 > 0.01f || axisRaw2 < -0.01f) && MonitorCenterCamera != null)
			{
				Vector3 position = MonitorCenterCamera.transform.position;
				Vector3 up = navigationRig.up;
				Vector3 vector2 = position - up * axisRaw2 * 0.1f * focusDistance;
				Vector3 vector3 = vector2 - vector;
				vector3.Normalize();
				vector2 = vector + vector3 * focusDistance;
				Vector3 vector4 = vector2 - position;
				Vector3 position2 = navigationRig.position + vector4;
				float num = Vector3.Dot(vector4, up);
				position2 += up * (0f - num);
				navigationRig.position = position2;
				playerHeightAdjust += num;
				if (MonitorCenterCamera != null)
				{
					MonitorCenterCamera.transform.LookAt(vector);
					Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
					localEulerAngles.y = 0f;
					localEulerAngles.z = 0f;
					MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
				}
			}
		}
		else if (Input.GetMouseButton(2))
		{
			bool flag = false;
			Vector3 position3 = navigationRig.position;
			float axisRaw3 = Input.GetAxisRaw("Mouse X");
			axisRaw3 = Mathf.Clamp(axisRaw3, -10f, 10f);
			if (axisRaw3 > 0.01f || axisRaw3 < -0.01f)
			{
				position3 += MonitorCenterCamera.transform.right * (0f - axisRaw3) * 0.03f;
				flag = true;
			}
			float axisRaw4 = Input.GetAxisRaw("Mouse Y");
			axisRaw4 = Mathf.Clamp(axisRaw4, -10f, 10f);
			if (axisRaw4 > 0.01f || axisRaw4 < -0.01f)
			{
				position3 += MonitorCenterCamera.transform.up * (0f - axisRaw4) * 0.03f;
				flag = true;
			}
			if (flag)
			{
				Vector3 up2 = navigationRig.up;
				Vector3 lhs = position3 - navigationRig.position;
				float num2 = Vector3.Dot(lhs, up2);
				position3 += up2 * (0f - num2);
				navigationRig.position = position3;
				playerHeightAdjust += num2;
			}
		}
		float y = Input.mouseScrollDelta.y;
		if (!GUIhitMouse && (y > 0.5f || y < -0.5f))
		{
			float num3 = 0.1f;
			if (y < -0.5f)
			{
				num3 = 0f - num3;
			}
			Vector3 forward = MonitorCenterCamera.transform.forward;
			Vector3 vector5 = num3 * forward * focusDistance;
			Vector3 position4 = navigationRig.position + vector5;
			focusDistance *= 1f - num3;
			Vector3 up3 = navigationRig.up;
			float num4 = Vector3.Dot(vector5, up3);
			position4 += up3 * (0f - num4);
			navigationRig.position = position4;
			playerHeightAdjust += num4;
		}
	}

	private void ProcessKeyboardFreeNavigation()
	{
		if (navigationRig != null && lookCamera != null && (LookInputModule.singleton == null || !LookInputModule.singleton.inputFieldActive) && !navigationDisabled)
		{
			bool flag = false;
			Vector2 vector = default(Vector2);
			vector.x = 0f;
			vector.y = 0f;
			float num = _freeMoveMultiplier;
			if (Input.GetKey(KeyCode.LeftShift))
			{
				num *= 3f;
			}
			if (Input.GetKey(KeyCode.W))
			{
				vector.y = num;
			}
			if (Input.GetKey(KeyCode.A))
			{
				vector.x = 0f - num;
			}
			if (Input.GetKey(KeyCode.S))
			{
				vector.y = 0f - num;
			}
			if (Input.GetKey(KeyCode.D))
			{
				vector.x = num;
			}
			if (vector.y != 0f)
			{
				flag = true;
				Vector3 vector2 = Vector3.ProjectOnPlane(lookCamera.transform.forward, navigationRig.up);
				vector2.Normalize();
				Vector3 position = navigationRig.position;
				position += vector2 * (vector.y * Time.unscaledDeltaTime);
				navigationRig.position = position;
			}
			if (vector.x != 0f)
			{
				flag = true;
				Vector3 vector3 = Vector3.ProjectOnPlane(lookCamera.transform.right, navigationRig.up);
				vector3.Normalize();
				Vector3 position2 = navigationRig.position;
				position2 += vector3 * (vector.x * Time.unscaledDeltaTime);
				navigationRig.position = position2;
			}
			float num2 = 0f;
			if (Input.GetKey(KeyCode.Z))
			{
				num2 = num;
			}
			if (Input.GetKey(KeyCode.X))
			{
				num2 = 0f - num;
			}
			if (num2 != 0f)
			{
				flag = true;
				playerHeightAdjust += num2 * 0.5f * Time.unscaledDeltaTime;
			}
			if (flag)
			{
				AdjustNavigationRigHeight();
			}
		}
	}

	private void ProcessMouseFreeNavigation()
	{
		if (!(navigationRig != null) || !(lookCamera != null) || navigationDisabled || Input.GetMouseButton(1))
		{
			return;
		}
		float axisRaw = Input.GetAxisRaw("Mouse X");
		axisRaw = Mathf.Clamp(axisRaw, -10f, 10f);
		if (axisRaw > 0.01f || axisRaw < -0.01f)
		{
			if (_mainHUDAnchoredOnMonitor && MonitorCenterCamera != null)
			{
				navigationRig.RotateAround(MonitorCenterCamera.transform.position, navigationRig.up, axisRaw);
			}
			else if (lookCamera != null)
			{
				navigationRig.RotateAround(lookCamera.transform.position, navigationRig.up, axisRaw);
			}
		}
		float axisRaw2 = Input.GetAxisRaw("Mouse Y");
		axisRaw2 = Mathf.Clamp(axisRaw2, -10f, 10f);
		if ((axisRaw2 > 0.01f || axisRaw2 < -0.01f) && MonitorCenterCamera != null)
		{
			Vector3 localEulerAngles = MonitorCenterCamera.transform.localEulerAngles;
			if (localEulerAngles.x > 180f)
			{
				localEulerAngles.x -= 360f;
			}
			if (localEulerAngles.x < -180f)
			{
				localEulerAngles.x += 360f;
			}
			localEulerAngles.x -= axisRaw2;
			localEulerAngles.x = Mathf.Clamp(localEulerAngles.x, -89f, 89f);
			localEulerAngles.y = 0f;
			localEulerAngles.z = 0f;
			MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
		}
	}

	private void ProcessTimeScale()
	{
		if (_isLoading)
		{
			useInterpolation = false;
			return;
		}
		if (Time.timeScale < 1f)
		{
			useInterpolation = true;
			return;
		}
		bool isPresent = XRDevice.isPresent;
		if (isMonitorOnly || !isPresent)
		{
			if (Time.fixedDeltaTime > 0.014f)
			{
				useInterpolation = true;
			}
			else
			{
				useInterpolation = false;
			}
			return;
		}
		if (Time.fixedDeltaTime <= 0.0069f)
		{
			useInterpolation = false;
			return;
		}
		float refreshRate = XRDevice.refreshRate;
		if (refreshRate != 0f)
		{
			bool flag = false;
			if (refreshRate <= 59f)
			{
				flag = true;
			}
			else if (refreshRate > 59f && refreshRate < 61f && Time.fixedDeltaTime > 0.0166f && Time.fixedDeltaTime < 0.0167f)
			{
				flag = true;
			}
			else if (refreshRate > 71f && refreshRate < 73f && Time.fixedDeltaTime > 0.0138f && Time.fixedDeltaTime < 0.0139f)
			{
				flag = true;
			}
			else if (refreshRate > 79f && refreshRate < 81f && Time.fixedDeltaTime > 0.0124f && Time.fixedDeltaTime < 0.0126f)
			{
				flag = true;
			}
			else if (refreshRate > 89f && refreshRate < 91f && Time.fixedDeltaTime > 0.0111f && Time.fixedDeltaTime < 0.0112f)
			{
				flag = true;
			}
			else if (refreshRate > 119f && refreshRate < 121f && Time.fixedDeltaTime > 0.0083f && Time.fixedDeltaTime < 0.0084f)
			{
				flag = true;
			}
			else if (refreshRate > 143f && refreshRate < 145f && Time.fixedDeltaTime > 0.0069f && Time.fixedDeltaTime < 0.007f)
			{
				flag = true;
			}
			if (flag)
			{
				useInterpolation = false;
			}
			else
			{
				useInterpolation = true;
			}
		}
		else if (Time.fixedDeltaTime > 0.012f)
		{
			useInterpolation = true;
		}
		else
		{
			useInterpolation = false;
		}
	}

	protected void CheckAutoConnectLeapHands()
	{
		if (leapHandModelControl != null && leapHandMountLeft != null)
		{
			if (_leapHandLeftConnected)
			{
				if (!leapHandModelControl.leftHandEnabled)
				{
					DisconnectLeapHandLeft();
				}
			}
			else if (leapHandModelControl.leftHandEnabled && leapHandMountLeft.gameObject.activeInHierarchy)
			{
				ConnectLeapHandLeft();
			}
		}
		if (!(leapHandModelControl != null) || !(leapHandMountRight != null))
		{
			return;
		}
		if (_leapHandRightConnected)
		{
			if (!leapHandModelControl.rightHandEnabled)
			{
				DisconnectLeapHandRight();
			}
		}
		else if (leapHandModelControl.rightHandEnabled && leapHandMountRight.gameObject.activeInHierarchy)
		{
			ConnectLeapHandRight();
		}
	}

	protected void ConnectLeapHandLeft()
	{
		if (leftHand != null && leapHandMountLeft != null)
		{
			_leapHandLeftConnected = true;
			leftHand.transform.SetParent(leapHandMountLeft);
			leftHand.transform.localPosition = Vector3.zero;
			leftHand.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (commonHandModelControl != null)
		{
			commonHandModelControl.ignorePositionRotationLeft = true;
		}
		if (ovrHandInputLeft != null)
		{
			ovrHandInputLeft.enabled = false;
		}
		if (steamVRHandInputLeft != null)
		{
			steamVRHandInputLeft.enabled = false;
		}
		if (handsContainer != null)
		{
			ConfigurableJointReconnector[] componentsInChildren = handsContainer.GetComponentsInChildren<ConfigurableJointReconnector>();
			ConfigurableJointReconnector[] array = componentsInChildren;
			foreach (ConfigurableJointReconnector configurableJointReconnector in array)
			{
				configurableJointReconnector.Reconnect();
			}
		}
	}

	protected void DisconnectLeapHandLeft()
	{
		_leapHandLeftConnected = false;
		if (leftHand != null)
		{
			leftHand.transform.SetParent(handMountLeft);
			leftHand.transform.localPosition = Vector3.zero;
			leftHand.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (commonHandModelControl != null)
		{
			commonHandModelControl.ignorePositionRotationLeft = false;
		}
		if (ovrHandInputLeft != null)
		{
			ovrHandInputLeft.enabled = true;
		}
		if (steamVRHandInputLeft != null)
		{
			steamVRHandInputLeft.enabled = true;
		}
		if (handsContainer != null)
		{
			ConfigurableJointReconnector[] componentsInChildren = handsContainer.GetComponentsInChildren<ConfigurableJointReconnector>();
			ConfigurableJointReconnector[] array = componentsInChildren;
			foreach (ConfigurableJointReconnector configurableJointReconnector in array)
			{
				configurableJointReconnector.Reconnect();
			}
		}
	}

	protected void ConnectLeapHandRight()
	{
		if (rightHand != null && leapHandMountRight != null)
		{
			_leapHandRightConnected = true;
			rightHand.transform.SetParent(leapHandMountRight);
			rightHand.transform.localPosition = Vector3.zero;
			rightHand.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (commonHandModelControl != null)
		{
			commonHandModelControl.ignorePositionRotationRight = true;
		}
		if (ovrHandInputRight != null)
		{
			ovrHandInputRight.enabled = false;
		}
		if (steamVRHandInputRight != null)
		{
			steamVRHandInputRight.enabled = false;
		}
		if (handsContainer != null)
		{
			ConfigurableJointReconnector[] componentsInChildren = handsContainer.GetComponentsInChildren<ConfigurableJointReconnector>();
			ConfigurableJointReconnector[] array = componentsInChildren;
			foreach (ConfigurableJointReconnector configurableJointReconnector in array)
			{
				configurableJointReconnector.Reconnect();
			}
		}
	}

	protected void DisconnectLeapHandRight()
	{
		_leapHandRightConnected = false;
		if (rightHand != null)
		{
			rightHand.transform.SetParent(handMountRight);
			rightHand.transform.localPosition = Vector3.zero;
			rightHand.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (commonHandModelControl != null)
		{
			commonHandModelControl.ignorePositionRotationRight = false;
		}
		if (ovrHandInputRight != null)
		{
			ovrHandInputRight.enabled = true;
		}
		if (steamVRHandInputRight != null)
		{
			steamVRHandInputRight.enabled = true;
		}
		if (handsContainer != null)
		{
			ConfigurableJointReconnector[] componentsInChildren = handsContainer.GetComponentsInChildren<ConfigurableJointReconnector>();
			ConfigurableJointReconnector[] array = componentsInChildren;
			foreach (ConfigurableJointReconnector configurableJointReconnector in array)
			{
				configurableJointReconnector.Reconnect();
			}
		}
	}

	protected void SyncTrackerVisibility()
	{
		SyncTracker1Visibility();
		SyncTracker2Visibility();
		SyncTracker3Visibility();
		SyncTracker4Visibility();
		SyncTracker5Visibility();
		SyncTracker6Visibility();
		SyncTracker7Visibility();
		SyncTracker8Visibility();
	}

	public void SyncTracker1Visibility()
	{
		if (!(viveTracker1 != null) || !(viveTracker1Model != null))
		{
			return;
		}
		bool active = _tracker1Visible && !_hideTrackers;
		viveTracker1Model.enabled = active;
		foreach (Transform item in viveTracker1Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker2Visibility()
	{
		if (!(viveTracker2 != null) || !(viveTracker2Model != null))
		{
			return;
		}
		bool active = _tracker2Visible && !_hideTrackers;
		viveTracker2Model.enabled = active;
		foreach (Transform item in viveTracker2Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker3Visibility()
	{
		if (!(viveTracker3 != null) || !(viveTracker3Model != null))
		{
			return;
		}
		bool active = _tracker3Visible && !_hideTrackers;
		viveTracker3Model.enabled = active;
		foreach (Transform item in viveTracker3Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker4Visibility()
	{
		if (!(viveTracker4 != null) || !(viveTracker4Model != null))
		{
			return;
		}
		bool active = _tracker4Visible && !_hideTrackers;
		viveTracker4Model.enabled = active;
		foreach (Transform item in viveTracker4Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker5Visibility()
	{
		if (!(viveTracker5 != null) || !(viveTracker5Model != null))
		{
			return;
		}
		bool active = _tracker5Visible && !_hideTrackers;
		viveTracker5Model.enabled = active;
		foreach (Transform item in viveTracker5Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker6Visibility()
	{
		if (!(viveTracker6 != null) || !(viveTracker6Model != null))
		{
			return;
		}
		bool active = _tracker6Visible && !_hideTrackers;
		viveTracker6Model.enabled = active;
		foreach (Transform item in viveTracker6Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker7Visibility()
	{
		if (!(viveTracker7 != null) || !(viveTracker7Model != null))
		{
			return;
		}
		bool active = _tracker7Visible && !_hideTrackers;
		viveTracker7Model.enabled = active;
		foreach (Transform item in viveTracker7Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public void SyncTracker8Visibility()
	{
		if (!(viveTracker8 != null) || !(viveTracker8Model != null))
		{
			return;
		}
		bool active = _tracker8Visible && !_hideTrackers;
		viveTracker8Model.enabled = active;
		foreach (Transform item in viveTracker8Model.transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	private void ConnectCenterCameraTarget(Transform parent, Vector3 offset, bool isMonitor)
	{
		if (centerCameraTarget != null)
		{
			centerCameraTarget.transform.SetParent(parent);
			centerCameraTarget.transform.localPosition = offset;
			centerCameraTarget.transform.localRotation = Quaternion.identity;
			centerCameraTarget.FindCamera();
			centerCameraTarget.isMonitorCamera = isMonitor;
		}
	}

	private void ToggleMonitorUI()
	{
		if (!(MonitorUI != null) || UIDisabled)
		{
			return;
		}
		if (MonitorUI.gameObject.activeSelf)
		{
			_toggleMonitorSaveMainHUDVisible = _mainHUDVisible;
			MonitorUI.gameObject.SetActive(value: false);
			HideMainHUD();
			return;
		}
		MonitorUI.gameObject.SetActive(value: true);
		if (_toggleMonitorSaveMainHUDVisible)
		{
			ShowMainHUDMonitor();
		}
	}

	private void SyncMonitorCameraFOV()
	{
		if (MonitorCenterCamera != null)
		{
			MonitorCenterCamera.fieldOfView = _monitorCameraFOV;
		}
	}

	public void ToggleMainMonitor()
	{
		if (!(MonitorRig != null))
		{
			return;
		}
		if (MonitorRigActive)
		{
			MonitorRigActive = false;
			MonitorRig.gameObject.SetActive(value: false);
			if (MonitorModeAuxUI != null)
			{
				MonitorModeAuxUI.gameObject.SetActive(value: false);
			}
			if (centerCameraTarget != null && !isMonitorOnly && saveCenterEyeAttachPoint != null)
			{
				ConnectCenterCameraTarget(saveCenterEyeAttachPoint, Vector3.zero, isMonitor: false);
			}
		}
		else
		{
			if (UserPreferences.singleton != null)
			{
				UserPreferences.singleton.overlayUI = true;
			}
			if (commonHandModelControl != null)
			{
				commonHandModelControl.useCollision = false;
			}
			if (alternateControllerHandModelControl != null)
			{
				alternateControllerHandModelControl.useCollision = false;
			}
			MonitorRigActive = true;
			MonitorRig.gameObject.SetActive(value: true);
			if (MonitorModeAuxUI != null && !UIDisabled)
			{
				MonitorModeAuxUI.gameObject.SetActive(value: true);
			}
			if (MonitorUIAttachPoint != null)
			{
				MoveMainHUD(MonitorUIAttachPoint);
			}
			if (centerCameraTarget != null && !isMonitorOnly)
			{
				saveCenterEyeAttachPoint = centerCameraTarget.transform.parent;
				ConnectCenterCameraTarget(MonitorCenterCamera.transform, MonitorCenterCameraOffset, isMonitor: true);
			}
			if (_mainHUDVisible)
			{
				HideMainHUD();
				ShowMainHUDAuto();
			}
			else
			{
				ShowMainHUDAuto();
				HideMainHUD();
			}
		}
		SyncUISide();
	}

	protected void SetMonitorRig()
	{
		isOVR = false;
		isOpenVR = false;
		isMonitorOnly = true;
		if (OVRRig != null)
		{
			OVRRig.gameObject.SetActive(value: false);
		}
		if (ViveRig != null)
		{
			ViveRig.gameObject.SetActive(value: false);
		}
		if (MonitorRig != null)
		{
			MonitorRigActive = true;
			MonitorRig.gameObject.SetActive(value: true);
		}
		if (MonitorModeButton != null)
		{
			MonitorModeButton.gameObject.SetActive(value: false);
		}
		if (MonitorCenterCamera != null && centerCameraTarget != null)
		{
			AudioListener component = MonitorCenterCamera.GetComponent<AudioListener>();
			if (component != null)
			{
				component.enabled = true;
			}
			ConnectCenterCameraTarget(MonitorCenterCamera.transform, MonitorCenterCameraOffset, isMonitor: true);
		}
		if (MonitorUIAttachPoint != null)
		{
			MoveMainHUD(MonitorUIAttachPoint);
			mainHUDAttachPointStartingPosition = mainHUDAttachPoint.localPosition;
			mainHUDAttachPointStartingRotation = mainHUDAttachPoint.localRotation;
		}
		if (MonitorUI != null)
		{
			if (UIDisabled)
			{
				MonitorUI.gameObject.SetActive(value: false);
			}
			else
			{
				MonitorUI.gameObject.SetActive(value: true);
			}
		}
		SyncActiveHands();
	}

	public void SetOculusThumbstickFunctionFromString(string str)
	{
		switch (str)
		{
			case "GrabWorld":
				oculusThumbstickFunction = ThumbstickFunction.GrabWorld;
				break;
			case "SwapAxis":
				oculusThumbstickFunction = ThumbstickFunction.SwapAxis;
				break;
			case "Both":
				oculusThumbstickFunction = ThumbstickFunction.Both;
				break;
			default:
				UnityEngine.Debug.LogWarning("Tried to set oculusThumbstickFunction to " + str + " which is not a valid type");
				break;
		}
	}

	protected void SetOculusRig()
	{
		isOVR = true;
		isOpenVR = false;
		isMonitorOnly = false;
		if (OVRRig != null)
		{
			OVRRig.gameObject.SetActive(value: true);
		}
		if (ViveRig != null)
		{
			ViveRig.gameObject.SetActive(value: false);
		}
		if (MonitorRig != null)
		{
			MonitorRigActive = false;
			MonitorRig.gameObject.SetActive(value: false);
		}
		if (MonitorModeButton != null)
		{
			MonitorModeButton.gameObject.SetActive(value: true);
		}
		if (MonitorUI != null)
		{
			if (UIDisabled)
			{
				MonitorUI.gameObject.SetActive(value: false);
			}
			else
			{
				MonitorUI.gameObject.SetActive(value: true);
			}
		}
		if (OVRCenterCamera != null && centerCameraTarget != null)
		{
			ConnectCenterCameraTarget(OVRCenterCamera.transform, Vector3.zero, isMonitor: false);
		}
		if (touchObjectLeft != null)
		{
			Camera component = touchObjectLeft.GetComponent<Camera>();
			if (component != null)
			{
				leftControllerCamera = component;
			}
		}
		if (touchObjectRight != null)
		{
			Camera component2 = touchObjectRight.GetComponent<Camera>();
			if (component2 != null)
			{
				rightControllerCamera = component2;
			}
		}
		if (leftHand != null)
		{
			leftHand.transform.SetParent(handMountLeft);
			leftHand.transform.localPosition = Vector3.zero;
			leftHand.transform.localRotation = Quaternion.identity;
		}
		if (leftHandAlternate != null)
		{
			leftHandAlternate.transform.SetParent(handMountLeft);
			leftHandAlternate.transform.localPosition = Vector3.zero;
			leftHandAlternate.transform.localRotation = Quaternion.identity;
		}
		if (rightHand != null)
		{
			rightHand.transform.SetParent(handMountRight);
			rightHand.transform.localPosition = Vector3.zero;
			rightHand.transform.localRotation = Quaternion.identity;
		}
		if (rightHandAlternate != null)
		{
			rightHandAlternate.transform.SetParent(handMountRight);
			rightHandAlternate.transform.localPosition = Vector3.zero;
			rightHandAlternate.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
	}

	protected void SetOpenVRRig()
	{
		isOVR = false;
		isOpenVR = true;
		isMonitorOnly = false;
		if (OVRRig != null)
		{
			OVRRig.gameObject.SetActive(value: false);
		}
		if (ViveRig != null)
		{
			ViveRig.gameObject.SetActive(value: true);
		}
		if (MonitorRig != null)
		{
			MonitorRigActive = false;
			MonitorRig.gameObject.SetActive(value: false);
		}
		if (MonitorModeButton != null)
		{
			MonitorModeButton.gameObject.SetActive(value: true);
		}
		if (MonitorUI != null)
		{
			if (UIDisabled)
			{
				MonitorUI.gameObject.SetActive(value: false);
			}
			else
			{
				MonitorUI.gameObject.SetActive(value: true);
			}
		}
		if (ViveCenterCamera != null && centerCameraTarget != null)
		{
			ConnectCenterCameraTarget(ViveCenterCamera.transform, Vector3.zero, isMonitor: false);
		}
		if (viveObjectLeft != null)
		{
			Camera component = viveObjectLeft.GetComponent<Camera>();
			if (component != null)
			{
				leftControllerCamera = component;
			}
			else
			{
				Error("Could not find camera on left controller");
			}
		}
		if (viveObjectRight != null)
		{
			Camera component2 = viveObjectRight.GetComponent<Camera>();
			if (component2 != null)
			{
				rightControllerCamera = component2;
			}
			else
			{
				Error("Could not find camera on right controller");
			}
		}
		if (leftHand != null)
		{
			leftHand.transform.SetParent(handMountLeft);
			leftHand.transform.localPosition = Vector3.zero;
			leftHand.transform.localRotation = Quaternion.identity;
		}
		if (leftHandAlternate != null)
		{
			leftHandAlternate.transform.SetParent(handMountLeft);
			leftHandAlternate.transform.localPosition = Vector3.zero;
			leftHandAlternate.transform.localRotation = Quaternion.identity;
		}
		if (rightHand != null)
		{
			rightHand.transform.SetParent(handMountRight);
			rightHand.transform.localPosition = Vector3.zero;
			rightHand.transform.localRotation = Quaternion.identity;
		}
		if (rightHandAlternate != null)
		{
			rightHandAlternate.transform.SetParent(handMountRight);
			rightHandAlternate.transform.localPosition = Vector3.zero;
			rightHandAlternate.transform.localRotation = Quaternion.identity;
		}
		SyncActiveHands();
		if (!Application.isEditor)
		{
			string dataPath = Application.dataPath;
			int num = dataPath.LastIndexOf('/');
			dataPath = dataPath.Remove(num, dataPath.Length - num);
			string pchApplicationManifestFullPath = Path.Combine(dataPath, "vrmanifest");
			EVRApplicationError eVRApplicationError = OpenVR.Applications.AddApplicationManifest(pchApplicationManifestFullPath, bTemporary: true);
			if (eVRApplicationError != 0)
			{
				UnityEngine.Debug.LogError("<b>[SteamVR]</b> Error adding vr manifest file: " + eVRApplicationError);
			}
			else
			{
				UnityEngine.Debug.Log("<b>[SteamVR]</b> Successfully added VR manifest to SteamVR");
			}
			int id = Process.GetCurrentProcess().Id;
			EVRApplicationError eVRApplicationError2 = OpenVR.Applications.IdentifyApplication((uint)id, SteamVR_Settings.instance.editorAppKey);
			if (eVRApplicationError2 != 0)
			{
				UnityEngine.Debug.LogError("<b>[SteamVR]</b> Error identifying application: " + eVRApplicationError2);
			}
			else
			{
				UnityEngine.Debug.Log($"<b>[SteamVR]</b> Successfully identified process as project to SteamVR ({SteamVR_Settings.instance.editorAppKey})");
			}
		}
	}

	public void OpenSteamVRBindingsInBrowser()
	{
		Process.Start("http://localhost:8998/dashboard/controllerbinding.html?app=" + SteamVR_Settings.instance.editorAppKey);
	}

	protected void DetermineVRRig()
	{
		Application.targetFrameRate = 300;
		if (VRDisabled)
		{
			SetMonitorRig();
			return;
		}
		string loadedDeviceName = XRSettings.loadedDeviceName;
		UnityEngine.Debug.Log("XR device active is " + XRSettings.isDeviceActive);
		UnityEngine.Debug.Log("XR device present is " + XRDevice.isPresent);
		UnityEngine.Debug.Log("Loaded XR device is " + loadedDeviceName);
		UnityEngine.Debug.Log("XR device model is " + XRDevice.model);
		UnityEngine.Debug.Log("XR device refresh rate is " + XRDevice.refreshRate);
		if (!XRSettings.isDeviceActive || loadedDeviceName == null || loadedDeviceName == string.Empty)
		{
			SetMonitorRig();
		}
		else if (loadedDeviceName == "Oculus")
		{
			SetOculusRig();
		}
		else
		{
			SetOpenVRRig();
		}
	}

	private void Awake()
	{
		_singleton = this;
		ZipConstants.DefaultCodePage = 0;
		DetermineVRRig();
		SyncVersionText();
		FileManager.ClearSecureReadPaths();
		FileManager.RegisterSecureReadPath(".");
		FileManager.ClearSecureWritePaths();
		FileManager.RegisterSecureWritePath("Saves");
		FileManager.RegisterSecureWritePath("Custom");
		FileManager.RegisterSecureWritePath("AddonPackages");
		FileManager.RegisterSecureWritePath("AddonPackagesBuilder");
	}

	private IEnumerator DelayStart()
	{
		onStartScene = true;
		yield return null;
		yield return null;
		yield return null;
		StartScene();
	}

	private void Start()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		Caching.ClearCache();
		if (LeapRig != null)
		{
			if (LeapServiceProviders != null)
			{
				LeapXRServiceProvider[] leapServiceProviders = LeapServiceProviders;
				foreach (LeapXRServiceProvider leapXRServiceProvider in leapServiceProviders)
				{
					leapXRServiceProvider.enabled = !leapDisabled;
				}
			}
			LeapRig.gameObject.SetActive(!leapDisabled);
		}
		SetSavesDirFromCommandline();
		castRay = default(Ray);
		playerNavTrackerGO = new GameObject();
		StartCoroutine(InitAssetManager());
		monitorCameraFOV = startingMonitorCameraFOV;
		SyncMonitorCameraFOV();
		InitUI();
		InitAtoms();
		InitTargets();
		InitMotionControllerNaviation();
		ResetFocusPoint();
		SyncGameMode();
		BuildMigrationMappings();
		bool flag = false;
		if (!UIDisabled)
		{
			flag = BuildFilesToMigrateMap();
		}
		if (!flag && startSceneEnabled)
		{
			StartCoroutine(DelayStart());
		}
	}

	private void Update()
	{
		CheckResumeSimulation();
		CheckLoadingIcon();
		CheckMessageAndErrorQueue();
		if (Time.unscaledDeltaTime > 0.015f)
		{
			DebugHUD.Alert1();
		}
		if (CameraTarget.centerTarget != null && CameraTarget.centerTarget.targetCamera != null)
		{
			lookCamera = CameraTarget.centerTarget.targetCamera;
		}
		drawRayLineRight = false;
		drawRayLineLeft = false;
		SyncNavigationHologridVisibility();
		PrepControllers();
		ProcessTimeScale();
		if (!onStartScene)
		{
			CheckAutoConnectLeapHands();
			ProcessUI();
			ProcessGUIInteract();
			ProcessPlayerNavMove();
			ProcessMouseControl();
			ProcessKeyBindings();
			ProcessKeyboardFreeNavigation();
			ProcessCycle();
			VerifyPossess();
			if (selectMode == SelectMode.FreeMoveMouse)
			{
				ProcessMouseFreeNavigation();
				ProcessLookAtTrigger();
			}
			else if (selectMode == SelectMode.FreeMove)
			{
				ProcessFreeMoveNavigation();
				ProcessLookAtTrigger();
			}
			else if (selectMode == SelectMode.Teleport)
			{
				ProcessTeleportMode();
			}
			else
			{
				ProcessControllerNavigation(freeMoveAction);
				switch (selectMode)
				{
					case SelectMode.Screenshot:
						drawRayLineLeft = !MonitorRigActive;
						drawRayLineRight = !MonitorRigActive;
						ProcessMotionControllerNavigation();
						ProcessHiResScreenshot();
						break;
					case SelectMode.SaveScreenshot:
						drawRayLineLeft = !MonitorRigActive;
						drawRayLineRight = !MonitorRigActive;
						ProcessMotionControllerNavigation();
						ProcessSaveScreenshot();
						break;
					case SelectMode.Off:
					case SelectMode.FilteredTargets:
					case SelectMode.Targets:
						ProcessLookAtTrigger();
						ProcessCommonTargetSelection();
						ProcessTargetShow();
						ProcessMouseTargetControl();
						ProcessMotionControllerTargetHighlight();
						ProcessMotionControllerNavigation();
						ProcessMotionControllerTargetControl();
						break;
					case SelectMode.Possess:
						ProcessMotionControllerNavigation();
						ProcessPossess();
						break;
					case SelectMode.TwoStagePossess:
						ProcessMotionControllerNavigation();
						ProcessTwoStagePossess();
						break;
					case SelectMode.AnimationRecord:
						ProcessTargetShow(canSelect: false);
						ProcessMouseTargetControl(canSelect: false);
						ProcessMotionControllerTargetHighlight();
						ProcessMotionControllerNavigation();
						ProcessMotionControllerTargetControl(canSelect: false);
						ProcessAnimationRecord();
						break;
					case SelectMode.Custom:
						drawRayLineLeft = false;
						drawRayLineRight = false;
						ProcessMotionControllerNavigation();
						break;
					case SelectMode.CustomWithTargetControl:
						ProcessTargetShow(canSelect: false);
						ProcessMouseTargetControl(canSelect: false);
						ProcessMotionControllerTargetHighlight();
						ProcessMotionControllerNavigation();
						ProcessMotionControllerTargetControl(canSelect: false);
						break;
					case SelectMode.CustomWithVRTargetControl:
						ProcessTargetShow(canSelect: false);
						ProcessMotionControllerTargetHighlight();
						ProcessMotionControllerNavigation();
						ProcessMotionControllerTargetControl(canSelect: false);
						break;
					default:
						ProcessMotionControllerSelect();
						ProcessMotionControllerNavigation();
						ProcessMouseSelect();
						break;
				}
			}
			ProcessUIMove();
		}
		if (!MonitorRigActive && _mainHUDVisible)
		{
			drawRayLineLeft = true;
			drawRayLineRight = true;
		}
		if (drawRayLineLeft)
		{
			if (rayLineDrawerLeft != null)
			{
				rayLineDrawerLeft.SetLinePoints(motionControllerLeft.position, motionControllerLeft.position + 50f * motionControllerLeft.forward);
				rayLineDrawerLeft.Draw(base.gameObject.layer);
			}
			if (rayLineLeft != null)
			{
				rayLineLeft.transform.position = motionControllerLeft.position;
				rayLineLeft.transform.rotation = motionControllerLeft.rotation;
				rayLineLeft.gameObject.SetActive(value: true);
			}
		}
		else if (rayLineLeft != null)
		{
			rayLineLeft.gameObject.SetActive(value: false);
		}
		if (drawRayLineRight)
		{
			if (rayLineDrawerRight != null)
			{
				rayLineDrawerRight.SetLinePoints(motionControllerRight.position, motionControllerRight.position + 50f * motionControllerRight.forward);
				rayLineDrawerRight.Draw(base.gameObject.layer);
			}
			if (rayLineRight != null)
			{
				rayLineRight.transform.position = motionControllerRight.position;
				rayLineRight.transform.rotation = motionControllerRight.rotation;
				rayLineRight.gameObject.SetActive(value: true);
			}
		}
		else if (rayLineRight != null)
		{
			rayLineRight.gameObject.SetActive(value: false);
		}
	}
}
