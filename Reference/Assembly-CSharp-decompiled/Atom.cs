using System;
using System.Collections.Generic;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class Atom : JSONStorable
{
	public string category;

	public string type;

	[NonSerialized]
	public bool destroyed;

	[NonSerialized]
	public bool loadedFromBundle;

	public Transform[] onToggleObjects;

	[SerializeField]
	protected bool _useRigidbodyInterpolation = true;

	protected List<AsyncFlag> waitResumeSimulationFlags;

	protected bool _pauseSimulation;

	protected JSONStorableAction toggleOnJSON;

	protected JSONStorableBool onJSON;

	[SerializeField]
	protected bool _on = true;

	protected JSONStorableBool hiddenJSON;

	protected bool _hidden;

	protected bool _tempHidden;

	protected float _currentScale = 1f;

	public JSONStorableBool collisionEnabledJSON;

	[SerializeField]
	protected bool _collisionEnabled = true;

	[SerializeField]
	private Atom _parentAtom;

	public UIPopup parentAtomSelectionPopup;

	public Transform reParentObject;

	public Transform childAtomContainer;

	public InputField idText;

	public InputFieldAction idTextAction;

	public InputField idTextAlt;

	public InputFieldAction idTextActionAlt;

	[SerializeField]
	private string _uid;

	public Text descriptionText;

	public Text descriptionTextAlt;

	[SerializeField]
	private string _description;

	private Transform[] masterControllerCorners;

	[SerializeField]
	private FreeControllerV3 _masterController;

	public FreeControllerV3 mainController;

	public float extentPadding = 0.3f;

	public bool alwaysShowExtents = true;

	private float extentLowX;

	private float extentHighX;

	private float extentLowY;

	private float extentHighY;

	private float extentLowZ;

	private float extentHighZ;

	private Vector3 extentlll;

	private Vector3 extentllh;

	private Vector3 extentlhl;

	private Vector3 extentlhh;

	private Vector3 extenthll;

	private Vector3 extenthlh;

	private Vector3 extenthhl;

	private Vector3 extenthhh;

	private bool _wasInit;

	private bool _callbackRegistered;

	private Vector3 reParentObjectStartingPosition;

	private Quaternion reParentObjectStartingRotation;

	private Vector3 childAtomContainerStartingPosition;

	private Quaternion childAtomContainerStartingRotation;

	private List<JSONStorable> _storables;

	private Dictionary<string, JSONStorable> _storableById;

	protected JSONClass lastRestoredData;

	protected bool lastRestorePhysical;

	protected bool lastRestoreAppearance;

	protected bool saveIncludePhysical;

	protected bool saveIncludeAppearance;

	protected string loadedName;

	protected string loadedPhysicalName;

	protected string loadedAppearanceName;

	protected string lastLoadPresetDir;

	protected string lastLoadAppearanceDir;

	protected string lastLoadPhysicalDir;

	private ForceReceiver[] _forceReceivers;

	private ForceProducerV2[] _forceProducers;

	private GrabPoint[] _grabPoints;

	private ScaleChangeReceiver[] _scaleChangeReceivers;

	private ScaleChangeReceiverJSONStorable[] _scaleChangeReceiverJSONStorables;

	private FreeControllerV3[] _freeControllers;

	private Rigidbody[] _rigidbodies;

	private Rigidbody[] _linkableRigidbodies;

	private Dictionary<Rigidbody, bool> _collisionExemptRigidbodies;

	private Rigidbody[] _realRigidbodies;

	private AnimationPattern[] _animationPatterns;

	private AnimationStep[] _animationSteps;

	private Animator[] _animators;

	private MotionAnimationControl[] _motionAnimationsControls;

	private PlayerNavCollider[] _playerNavColliders;

	private List<Canvas> _canvases;

	private PhysicsSimulator[] _physicsSimulators;

	private PhysicsSimulatorJSONStorable[] _physicsSimulatorsStorable;

	private List<PhysicsSimulator> _dynamicPhysicsSimulators;

	private List<ScaleChangeReceiver> _dynamicScaleChangeReceivers;

	private List<ScaleChangeReceiverJSONStorable> _dynamicScaleChangeReceiverJSONStorables;

	private AutoColliderBatchUpdater[] _autoColliderBatchUpdaters;

	private RhythmController[] _rhythmControllers;

	public Button selectAtomParentFromSceneButton;

	public bool useRigidbodyInterpolation
	{
		get
		{
			return _useRigidbodyInterpolation;
		}
		set
		{
			if (_useRigidbodyInterpolation != value)
			{
				_useRigidbodyInterpolation = value;
				if (Application.isPlaying)
				{
					SyncRigidbodyInterpolation();
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

	public bool on => _on;

	public bool hidden
	{
		get
		{
			if (hiddenJSON != null)
			{
				return hiddenJSON.val;
			}
			return _hidden;
		}
		set
		{
			if (hiddenJSON != null)
			{
				hiddenJSON.val = value;
			}
			else if (_hidden != value)
			{
				_hidden = value;
				SyncHidden(value);
			}
		}
	}

	public bool hiddenNoCallback
	{
		get
		{
			if (hiddenJSON != null)
			{
				return hiddenJSON.val;
			}
			return _hidden;
		}
		set
		{
			if (hiddenJSON != null)
			{
				hiddenJSON.valNoCallback = value;
			}
			else if (_hidden != value)
			{
				_hidden = value;
				SyncHidden(value);
			}
		}
	}

	public bool tempHidden
	{
		get
		{
			return _tempHidden;
		}
		set
		{
			if (_tempHidden != value)
			{
				_tempHidden = value;
			}
		}
	}

	public Atom parentAtom
	{
		get
		{
			return _parentAtom;
		}
		set
		{
			if (!(_parentAtom != value))
			{
				return;
			}
			_parentAtom = value;
			if (!(reParentObject != null))
			{
				return;
			}
			if (_parentAtom != null)
			{
				if (_parentAtom.childAtomContainer != null)
				{
					reParentObject.parent = _parentAtom.childAtomContainer;
				}
				else
				{
					reParentObject.parent = _parentAtom.transform;
				}
			}
			else
			{
				reParentObject.parent = base.transform;
			}
		}
	}

	public string uid
	{
		get
		{
			return _uid;
		}
		set
		{
			_uid = value;
			SyncIdText();
		}
	}

	public string description
	{
		get
		{
			return _description;
		}
		set
		{
			_description = value;
			SyncDescriptionText();
		}
	}

	public FreeControllerV3 masterController
	{
		get
		{
			return _masterController;
		}
		set
		{
			if (_masterController != value)
			{
				_masterController = value;
				SyncMasterControllerCorners();
			}
		}
	}

	public ForceReceiver[] forceReceivers => _forceReceivers;

	public ForceProducerV2[] forceProducers => _forceProducers;

	public GrabPoint[] grabPoints => _grabPoints;

	public ScaleChangeReceiver[] scaleChangeReceivers => _scaleChangeReceivers;

	public ScaleChangeReceiverJSONStorable[] scaleChangeReceiverJSONStorables => _scaleChangeReceiverJSONStorables;

	public FreeControllerV3[] freeControllers => _freeControllers;

	public Rigidbody[] rigidbodies => _rigidbodies;

	public Rigidbody[] linkableRigidbodies => _linkableRigidbodies;

	public Rigidbody[] realRigidbodies => _realRigidbodies;

	public AnimationPattern[] animationPatterns => _animationPatterns;

	public AnimationStep[] animationSteps => _animationSteps;

	public Animator[] animators => _animators;

	public MotionAnimationControl[] motionAnimationControls => _motionAnimationsControls;

	public PlayerNavCollider[] playerNavColliders => _playerNavColliders;

	public List<Canvas> canvases => _canvases;

	public PhysicsSimulator[] physicsSimulators => _physicsSimulators;

	public PhysicsSimulatorJSONStorable[] physicsSimulatorsStorable => _physicsSimulatorsStorable;

	public AutoColliderBatchUpdater[] autoColliderBatchUpdaters => _autoColliderBatchUpdaters;

	public RhythmController[] rhythmControllers => _rhythmControllers;

	protected void CheckResumeSimulation()
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		bool flag = false;
		if (waitResumeSimulationFlags.Count > 0)
		{
			List<AsyncFlag> list = new List<AsyncFlag>();
			foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
			{
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
			pauseSimulation = true;
		}
		else if (flag)
		{
			pauseSimulation = false;
		}
	}

	public void PauseSimulation(AsyncFlag af)
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		waitResumeSimulationFlags.Add(af);
		pauseSimulation = true;
		if (_autoColliderBatchUpdaters != null)
		{
			AutoColliderBatchUpdater[] array = _autoColliderBatchUpdaters;
			foreach (AutoColliderBatchUpdater autoColliderBatchUpdater in array)
			{
				autoColliderBatchUpdater.PauseSimulation(af);
			}
		}
		if (_physicsSimulators != null)
		{
			PhysicsSimulator[] array2 = _physicsSimulators;
			foreach (PhysicsSimulator physicsSimulator in array2)
			{
				if (physicsSimulator.enabled)
				{
					physicsSimulator.PauseSimulation(af);
				}
			}
		}
		if (_physicsSimulatorsStorable != null)
		{
			PhysicsSimulatorJSONStorable[] array3 = _physicsSimulatorsStorable;
			foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in array3)
			{
				if (physicsSimulatorJSONStorable.enabled)
				{
					physicsSimulatorJSONStorable.PauseSimulation(af);
				}
			}
		}
		if (_dynamicPhysicsSimulators == null)
		{
			return;
		}
		foreach (PhysicsSimulator dynamicPhysicsSimulator in _dynamicPhysicsSimulators)
		{
			dynamicPhysicsSimulator.PauseSimulation(af);
		}
	}

	protected void SyncRigidbodyInterpolation()
	{
		if (_realRigidbodies != null)
		{
			Rigidbody[] array = _realRigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				RigidbodyAttributes component = rigidbody.GetComponent<RigidbodyAttributes>();
				if (component != null)
				{
					component.useInterpolation = _on && _useRigidbodyInterpolation;
				}
				else if (_on && _useRigidbodyInterpolation)
				{
					if (!rigidbody.isKinematic)
					{
						rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
					}
				}
				else
				{
					rigidbody.interpolation = RigidbodyInterpolation.None;
				}
			}
		}
		if (_physicsSimulators != null)
		{
			PhysicsSimulator[] array2 = _physicsSimulators;
			foreach (PhysicsSimulator physicsSimulator in array2)
			{
				physicsSimulator.useInterpolation = _useRigidbodyInterpolation;
			}
		}
		if (_physicsSimulatorsStorable != null)
		{
			PhysicsSimulatorJSONStorable[] array3 = _physicsSimulatorsStorable;
			foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in array3)
			{
				physicsSimulatorJSONStorable.useInterpolation = _useRigidbodyInterpolation;
			}
		}
		if (_dynamicPhysicsSimulators == null)
		{
			return;
		}
		foreach (PhysicsSimulator dynamicPhysicsSimulator in _dynamicPhysicsSimulators)
		{
			dynamicPhysicsSimulator.useInterpolation = _useRigidbodyInterpolation;
		}
	}

	protected void SyncOnToggleObjects()
	{
		if (onToggleObjects == null)
		{
			return;
		}
		Transform[] array = onToggleObjects;
		foreach (Transform transform in array)
		{
			if (transform != null)
			{
				transform.gameObject.SetActive(_on);
			}
		}
	}

	protected void SyncOn(bool b)
	{
		_on = b;
		SyncOnToggleObjects();
		SyncRigidbodyInterpolation();
	}

	public void SetOn(bool b)
	{
		onJSON.val = b;
	}

	public void ToggleOn()
	{
		if (onJSON != null)
		{
			onJSON.val = !onJSON.val;
		}
	}

	protected void SyncHidden(bool b)
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SyncHiddenAtoms();
		}
	}

	public void ScaleChanged(float sc)
	{
		_currentScale = sc;
		if (_scaleChangeReceivers != null)
		{
			ScaleChangeReceiver[] array = _scaleChangeReceivers;
			foreach (ScaleChangeReceiver scaleChangeReceiver in array)
			{
				scaleChangeReceiver.ScaleChanged(sc);
			}
		}
		if (_scaleChangeReceiverJSONStorables != null)
		{
			ScaleChangeReceiverJSONStorable[] array2 = _scaleChangeReceiverJSONStorables;
			foreach (ScaleChangeReceiverJSONStorable scaleChangeReceiverJSONStorable in array2)
			{
				scaleChangeReceiverJSONStorable.ScaleChanged(sc);
			}
		}
		if (_dynamicScaleChangeReceivers != null)
		{
			foreach (ScaleChangeReceiver dynamicScaleChangeReceiver in _dynamicScaleChangeReceivers)
			{
				dynamicScaleChangeReceiver.ScaleChanged(sc);
			}
		}
		if (_dynamicScaleChangeReceiverJSONStorables == null)
		{
			return;
		}
		foreach (ScaleChangeReceiverJSONStorable dynamicScaleChangeReceiverJSONStorable in _dynamicScaleChangeReceiverJSONStorables)
		{
			dynamicScaleChangeReceiverJSONStorable.ScaleChanged(sc);
		}
	}

	protected void SyncCollisionEnabled(bool b)
	{
		_collisionEnabled = b;
		if (_freeControllers != null)
		{
			FreeControllerV3[] array = _freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				if (freeControllerV.controlsCollisionEnabled)
				{
					freeControllerV.globalCollisionEnabled = b;
				}
			}
		}
		if (_realRigidbodies != null)
		{
			Rigidbody[] array2 = _realRigidbodies;
			foreach (Rigidbody rigidbody in array2)
			{
				if (!_collisionExemptRigidbodies.ContainsKey(rigidbody))
				{
					rigidbody.detectCollisions = _collisionEnabled;
				}
			}
		}
		if (_physicsSimulators != null)
		{
			PhysicsSimulator[] array3 = _physicsSimulators;
			foreach (PhysicsSimulator physicsSimulator in array3)
			{
				physicsSimulator.collisionEnabled = _collisionEnabled;
			}
		}
		if (_physicsSimulatorsStorable != null)
		{
			PhysicsSimulatorJSONStorable[] array4 = _physicsSimulatorsStorable;
			foreach (PhysicsSimulatorJSONStorable physicsSimulatorJSONStorable in array4)
			{
				physicsSimulatorJSONStorable.collisionEnabled = _collisionEnabled;
			}
		}
		if (_dynamicPhysicsSimulators == null)
		{
			return;
		}
		foreach (PhysicsSimulator dynamicPhysicsSimulator in _dynamicPhysicsSimulators)
		{
			dynamicPhysicsSimulator.collisionEnabled = _collisionEnabled;
		}
	}

	protected void SyncIdText()
	{
		if (idText != null)
		{
			idText.text = _uid;
		}
		if (idTextAlt != null)
		{
			idTextAlt.text = _uid;
		}
	}

	public void SetUID(string val)
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.RenameAtom(this, val);
		}
	}

	protected void SetUIDToInputField()
	{
		if (idText != null)
		{
			SetUID(idText.text);
		}
	}

	protected void SetUIDToInputFieldAlt()
	{
		if (idTextAlt != null)
		{
			SetUID(idTextAlt.text);
		}
	}

	protected void SyncDescriptionText()
	{
		if (descriptionText != null)
		{
			descriptionText.text = _description;
		}
		if (descriptionTextAlt != null)
		{
			descriptionTextAlt.text = _description;
		}
	}

	private void SyncMasterControllerCorners()
	{
		List<Transform> list = new List<Transform>();
		if (_masterController != null)
		{
			foreach (Transform item in _masterController.transform)
			{
				list.Add(item);
			}
		}
		masterControllerCorners = list.ToArray();
	}

	private void walkAndGetComponents(Transform t, List<ForceReceiver> receivers, List<ForceProducerV2> producers, List<GrabPoint> gpoints, List<FreeControllerV3> controllers, List<Rigidbody> rbs, List<Rigidbody> linkablerbs, List<Rigidbody> realrbs, List<AnimationPattern> ans, List<AnimationStep> asts, List<Animator> anms, List<JSONStorable> jss, List<Canvas> cvs, List<AutoColliderBatchUpdater> acbus, List<PhysicsSimulator> psms, List<PhysicsSimulatorJSONStorable> psmjss, List<MotionAnimationControl> macs, List<PlayerNavCollider> pncs, List<JSONStorableDynamic> jsds, List<RhythmController> rcs, List<ScaleChangeReceiver> scrs, List<ScaleChangeReceiverJSONStorable> scrjss, bool insidePhysicsSimulator)
	{
		Rigidbody component = t.GetComponent<Rigidbody>();
		if (component != null)
		{
			rbs.Add(component);
		}
		ForceReceiver component2 = t.GetComponent<ForceReceiver>();
		if (component2 != null)
		{
			component2.containingAtom = this;
			receivers.Add(component2);
		}
		ForceProducerV2 component3 = t.GetComponent<ForceProducerV2>();
		if (component3 != null)
		{
			component3.containingAtom = this;
			producers.Add(component3);
		}
		GrabPoint component4 = t.GetComponent<GrabPoint>();
		if (component4 != null)
		{
			component4.containingAtom = this;
			gpoints.Add(component4);
		}
		FreeControllerV3 component5 = t.GetComponent<FreeControllerV3>();
		if (component5 != null)
		{
			component5.containingAtom = this;
			controllers.Add(component5);
			if (component5.controlsCollisionEnabled && component5.followWhenOffRB != null)
			{
				_collisionExemptRigidbodies.Add(component5.followWhenOffRB, value: true);
			}
		}
		MotionAnimationControl component6 = t.GetComponent<MotionAnimationControl>();
		if (component6 != null)
		{
			component6.containingAtom = this;
			macs.Add(component6);
		}
		PhysicsSimulator component7 = t.GetComponent<PhysicsSimulator>();
		if (component7 != null)
		{
			insidePhysicsSimulator = true;
			psms.Add(component7);
		}
		PhysicsSimulatorJSONStorable component8 = t.GetComponent<PhysicsSimulatorJSONStorable>();
		if (component8 != null)
		{
			insidePhysicsSimulator = true;
			psmjss.Add(component8);
		}
		if (component != null && component5 == null && !insidePhysicsSimulator)
		{
			realrbs.Add(component);
		}
		if (component != null && (component2 != null || component5 != null))
		{
			linkablerbs.Add(component);
		}
		AnimationPattern component9 = t.GetComponent<AnimationPattern>();
		if (component9 != null)
		{
			component9.containingAtom = this;
			ans.Add(component9);
		}
		AnimationStep component10 = t.GetComponent<AnimationStep>();
		if (component10 != null)
		{
			component10.containingAtom = this;
			asts.Add(component10);
		}
		Animator component11 = t.GetComponent<Animator>();
		if (component11 != null)
		{
			anms.Add(component11);
		}
		Canvas component12 = t.GetComponent<Canvas>();
		if (component12 != null)
		{
			cvs.Add(component12);
		}
		JSONStorable[] components = t.GetComponents<JSONStorable>();
		if (components != null)
		{
			JSONStorable[] array = components;
			foreach (JSONStorable jSONStorable in array)
			{
				jSONStorable.containingAtom = this;
				jss.Add(jSONStorable);
			}
		}
		JSONStorableDynamic[] components2 = t.GetComponents<JSONStorableDynamic>();
		if (components2 != null)
		{
			JSONStorableDynamic[] array2 = components2;
			foreach (JSONStorableDynamic jSONStorableDynamic in array2)
			{
				jSONStorableDynamic.containingAtom = this;
				jsds.Add(jSONStorableDynamic);
			}
		}
		PlayerNavCollider component13 = t.GetComponent<PlayerNavCollider>();
		if (component13 != null)
		{
			component13.containingAtom = this;
			pncs.Add(component13);
		}
		AutoColliderBatchUpdater component14 = t.GetComponent<AutoColliderBatchUpdater>();
		if (component14 != null)
		{
			acbus.Add(component14);
		}
		RhythmController component15 = t.GetComponent<RhythmController>();
		if (component15 != null)
		{
			component15.containingAtom = this;
			rcs.Add(component15);
		}
		UIConnectorMaster component16 = t.GetComponent<UIConnectorMaster>();
		if (component16 != null)
		{
			component16.containingAtom = this;
		}
		SetTransformScale component17 = t.GetComponent<SetTransformScale>();
		if (component17 != null)
		{
			component17.containingAtom = this;
		}
		ScaleChangeReceiver[] components3 = t.GetComponents<ScaleChangeReceiver>();
		ScaleChangeReceiver[] array3 = components3;
		foreach (ScaleChangeReceiver scaleChangeReceiver in array3)
		{
			if (scaleChangeReceiver != null)
			{
				scrs.Add(scaleChangeReceiver);
			}
		}
		ScaleChangeReceiverJSONStorable[] components4 = t.GetComponents<ScaleChangeReceiverJSONStorable>();
		ScaleChangeReceiverJSONStorable[] array4 = components4;
		foreach (ScaleChangeReceiverJSONStorable scaleChangeReceiverJSONStorable in array4)
		{
			if (scaleChangeReceiverJSONStorable != null)
			{
				scrjss.Add(scaleChangeReceiverJSONStorable);
			}
		}
		foreach (Transform item in t)
		{
			if (!item.GetComponent<Atom>())
			{
				walkAndGetComponents(item, receivers, producers, gpoints, controllers, rbs, linkablerbs, realrbs, ans, asts, anms, jss, cvs, acbus, psms, psmjss, macs, pncs, jsds, rcs, scrs, scrjss, insidePhysicsSimulator);
			}
		}
	}

	private void Init()
	{
		if (_wasInit)
		{
			Debug.LogError("Init was already called");
		}
		_wasInit = true;
		onJSON = new JSONStorableBool("on", _on, SyncOn);
		RegisterBool(onJSON);
		onJSON.isStorable = false;
		onJSON.isRestorable = false;
		toggleOnJSON = new JSONStorableAction("ToggleOn", ToggleOn);
		RegisterAction(toggleOnJSON);
		hiddenJSON = new JSONStorableBool("hidden", _hidden, SyncHidden);
		hiddenJSON.storeType = JSONStorableParam.StoreType.Full;
		RegisterBool(hiddenJSON);
		collisionEnabledJSON = new JSONStorableBool("collisionEnabled", _collisionEnabled, SyncCollisionEnabled);
		RegisterBool(collisionEnabledJSON);
		collisionEnabledJSON.isStorable = false;
		collisionEnabledJSON.isRestorable = false;
		_collisionExemptRigidbodies = new Dictionary<Rigidbody, bool>();
		List<ForceReceiver> list = new List<ForceReceiver>();
		List<ForceProducerV2> list2 = new List<ForceProducerV2>();
		List<GrabPoint> list3 = new List<GrabPoint>();
		List<FreeControllerV3> list4 = new List<FreeControllerV3>();
		List<Rigidbody> list5 = new List<Rigidbody>();
		List<Rigidbody> list6 = new List<Rigidbody>();
		List<Rigidbody> list7 = new List<Rigidbody>();
		List<AnimationPattern> list8 = new List<AnimationPattern>();
		List<AnimationStep> list9 = new List<AnimationStep>();
		List<Animator> list10 = new List<Animator>();
		List<Canvas> cvs = new List<Canvas>();
		List<JSONStorable> list11 = new List<JSONStorable>();
		List<AutoColliderBatchUpdater> list12 = new List<AutoColliderBatchUpdater>();
		List<PhysicsSimulator> list13 = new List<PhysicsSimulator>();
		List<PhysicsSimulatorJSONStorable> list14 = new List<PhysicsSimulatorJSONStorable>();
		List<MotionAnimationControl> list15 = new List<MotionAnimationControl>();
		List<PlayerNavCollider> list16 = new List<PlayerNavCollider>();
		List<JSONStorableDynamic> jsds = new List<JSONStorableDynamic>();
		List<RhythmController> list17 = new List<RhythmController>();
		List<ScaleChangeReceiver> list18 = new List<ScaleChangeReceiver>();
		List<ScaleChangeReceiverJSONStorable> list19 = new List<ScaleChangeReceiverJSONStorable>();
		walkAndGetComponents(base.transform, list, list2, list3, list4, list5, list6, list7, list8, list9, list10, list11, cvs, list12, list13, list14, list15, list16, jsds, list17, list18, list19, insidePhysicsSimulator: false);
		_forceReceivers = list.ToArray();
		_forceProducers = list2.ToArray();
		_rhythmControllers = list17.ToArray();
		_grabPoints = list3.ToArray();
		_freeControllers = list4.ToArray();
		_rigidbodies = list5.ToArray();
		_linkableRigidbodies = list6.ToArray();
		_realRigidbodies = list7.ToArray();
		_animationPatterns = list8.ToArray();
		_animationSteps = list9.ToArray();
		_animators = list10.ToArray();
		_motionAnimationsControls = list15.ToArray();
		_playerNavColliders = list16.ToArray();
		_canvases = cvs;
		_storables = list11;
		_physicsSimulators = list13.ToArray();
		_physicsSimulatorsStorable = list14.ToArray();
		_autoColliderBatchUpdaters = list12.ToArray();
		_scaleChangeReceivers = list18.ToArray();
		_scaleChangeReceiverJSONStorables = list19.ToArray();
		_storableById = new Dictionary<string, JSONStorable>();
		foreach (JSONStorable storable in _storables)
		{
			if (!storable.exclude)
			{
				if (_storableById.ContainsKey(storable.storeId))
				{
					Debug.LogError("Found duplicate storable uid " + storable.storeId + " in atom " + uid + " of type " + type);
				}
				else
				{
					_storableById.Add(storable.storeId, storable);
				}
			}
		}
		SyncOnToggleObjects();
		SyncRigidbodyInterpolation();
		SyncCollisionEnabled(_collisionEnabled);
		if (SuperController.singleton != null)
		{
			_callbackRegistered = true;
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	private void OnDestroy()
	{
		if (_callbackRegistered && SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	public List<string> GetStorableIDs()
	{
		List<string> list = new List<string>();
		foreach (string key in _storableById.Keys)
		{
			if (_storableById.TryGetValue(key, out var value) && !value.exclude && (!value.onlyStoreIfActive || value.gameObject.activeInHierarchy))
			{
				list.Add(key);
			}
		}
		list.Sort();
		return list;
	}

	public JSONStorable GetStorableByID(string storeid)
	{
		JSONStorable value = null;
		_storableById.TryGetValue(storeid, out value);
		return value;
	}

	public void Store(JSONArray atoms, bool includePhysical = true, bool includeAppearance = true)
	{
		JSONClass jSONClass = new JSONClass();
		jSONClass["id"] = uid;
		if (includePhysical)
		{
			jSONClass["on"].AsBool = _on;
		}
		if (includePhysical && collisionEnabledJSON.val != collisionEnabledJSON.defaultVal)
		{
			jSONClass["collisionEnabled"].AsBool = _collisionEnabled;
		}
		if (type != null)
		{
			jSONClass["type"] = type;
		}
		else
		{
			Debug.LogWarning("Atom " + uid + " does not have a type set");
		}
		if (parentAtom != null)
		{
			jSONClass["parentAtom"] = parentAtom.uid;
		}
		if (reParentObject != null && includePhysical)
		{
			Vector3 position = reParentObject.position;
			jSONClass["position"]["x"].AsFloat = position.x;
			jSONClass["position"]["y"].AsFloat = position.y;
			jSONClass["position"]["z"].AsFloat = position.z;
			Vector3 eulerAngles = reParentObject.eulerAngles;
			jSONClass["rotation"]["x"].AsFloat = eulerAngles.x;
			jSONClass["rotation"]["y"].AsFloat = eulerAngles.y;
			jSONClass["rotation"]["z"].AsFloat = eulerAngles.z;
		}
		if (childAtomContainer != null && includePhysical)
		{
			Vector3 position2 = childAtomContainer.position;
			jSONClass["containerPosition"]["x"].AsFloat = position2.x;
			jSONClass["containerPosition"]["y"].AsFloat = position2.y;
			jSONClass["containerPosition"]["z"].AsFloat = position2.z;
			Vector3 eulerAngles2 = childAtomContainer.eulerAngles;
			jSONClass["containerRotation"]["x"].AsFloat = eulerAngles2.x;
			jSONClass["containerRotation"]["y"].AsFloat = eulerAngles2.y;
			jSONClass["containerRotation"]["z"].AsFloat = eulerAngles2.z;
		}
		atoms.Add(jSONClass);
		jSONClass["storables"] = new JSONArray();
		foreach (JSONStorable storable in _storables)
		{
			if (!storable.exclude && (!storable.onlyStoreIfActive || storable.gameObject.activeInHierarchy))
			{
				JSONClass jSON = storable.GetJSON(includePhysical, includeAppearance);
				if (storable.needsStore)
				{
					jSONClass["storables"].Add(jSON);
				}
			}
		}
	}

	public void RestoreForceInitialize()
	{
		onJSON.val = true;
	}

	public void RestoreStartingOn()
	{
		onJSON.val = onJSON.defaultVal;
	}

	public void RestoreTransform(JSONClass jc)
	{
		if (reParentObject != null)
		{
			if (jc["position"] != null)
			{
				Vector3 position = reParentObject.position;
				if (jc["position"]["x"] != null)
				{
					position.x = jc["position"]["x"].AsFloat;
				}
				if (jc["position"]["y"] != null)
				{
					position.y = jc["position"]["y"].AsFloat;
				}
				if (jc["position"]["z"] != null)
				{
					position.z = jc["position"]["z"].AsFloat;
				}
				reParentObject.position = position;
			}
			else
			{
				reParentObject.position = reParentObjectStartingPosition;
			}
			if (jc["rotation"] != null)
			{
				Vector3 eulerAngles = reParentObject.eulerAngles;
				if (jc["rotation"]["x"] != null)
				{
					eulerAngles.x = jc["rotation"]["x"].AsFloat;
				}
				if (jc["rotation"]["y"] != null)
				{
					eulerAngles.y = jc["rotation"]["y"].AsFloat;
				}
				if (jc["rotation"]["z"] != null)
				{
					eulerAngles.z = jc["rotation"]["z"].AsFloat;
				}
				reParentObject.eulerAngles = eulerAngles;
			}
			else
			{
				reParentObject.rotation = reParentObjectStartingRotation;
			}
		}
		if (!(childAtomContainer != null))
		{
			return;
		}
		if (jc["containerPosition"] != null)
		{
			Vector3 position2 = childAtomContainer.position;
			if (jc["containerPosition"]["x"] != null)
			{
				position2.x = jc["containerPosition"]["x"].AsFloat;
			}
			if (jc["containerPosition"]["y"] != null)
			{
				position2.y = jc["containerPosition"]["y"].AsFloat;
			}
			if (jc["containerPosition"]["z"] != null)
			{
				position2.z = jc["containerPosition"]["z"].AsFloat;
			}
			childAtomContainer.position = position2;
		}
		else
		{
			childAtomContainer.position = childAtomContainerStartingPosition;
		}
		if (jc["containerRotation"] != null)
		{
			Vector3 eulerAngles2 = childAtomContainer.eulerAngles;
			if (jc["containerRotation"]["x"] != null)
			{
				eulerAngles2.x = jc["containerRotation"]["x"].AsFloat;
			}
			if (jc["containerRotation"]["y"] != null)
			{
				eulerAngles2.y = jc["containerRotation"]["y"].AsFloat;
			}
			if (jc["containerRotation"]["z"] != null)
			{
				eulerAngles2.z = jc["containerRotation"]["z"].AsFloat;
			}
			childAtomContainer.eulerAngles = eulerAngles2;
		}
		else
		{
			childAtomContainer.rotation = childAtomContainerStartingRotation;
		}
	}

	public void ClearParentAtom()
	{
		SelectAtomParent(null);
	}

	public void RestoreParentAtom(JSONClass jc)
	{
		if (jc["parentAtom"] != null)
		{
			Atom atomByUid = SuperController.singleton.GetAtomByUid(jc["parentAtom"]);
			SelectAtomParent(atomByUid);
		}
		else
		{
			SelectAtomParent(null);
		}
	}

	public void SetLastRestoredData(JSONClass jc, bool isAppearance = true, bool isPhysical = true)
	{
		lastRestoredData = jc;
		lastRestorePhysical = isPhysical;
		lastRestoreAppearance = isAppearance;
	}

	public void RestoreFromLast(JSONStorable js)
	{
		if (!(lastRestoredData != null))
		{
			return;
		}
		bool flag = false;
		foreach (JSONClass item in lastRestoredData["storables"].AsArray)
		{
			string text = item["id"];
			if (text == js.storeId)
			{
				flag = true;
				js.RestoreFromJSON(item, lastRestorePhysical, lastRestoreAppearance);
				js.LateRestoreFromJSON(item, lastRestorePhysical, lastRestoreAppearance);
				break;
			}
		}
		if (!flag)
		{
			JSONClass jc = new JSONClass();
			js.RestoreFromJSON(jc, lastRestorePhysical, lastRestoreAppearance);
		}
	}

	public void Restore(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool restoreCore = true, JSONArray presetAtoms = null, bool isClear = false)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		lastRestoredData = jc;
		lastRestoreAppearance = restoreAppearance;
		lastRestorePhysical = restorePhysical;
		if (restoreCore)
		{
			if (jc["on"] != null)
			{
				onJSON.val = jc["on"].AsBool;
			}
			else
			{
				onJSON.val = onJSON.defaultVal;
			}
		}
		foreach (JSONClass item in jc["storables"].AsArray)
		{
			if (_storableById.TryGetValue(item["id"], out var value))
			{
				value.RestoreFromJSON(item, restorePhysical, restoreAppearance, presetAtoms);
				if (!dictionary.ContainsKey(item["id"]))
				{
					dictionary.Add(item["id"], value: true);
				}
			}
		}
		JSONStorable[] array = _storables.ToArray();
		JSONStorable[] array2 = array;
		foreach (JSONStorable jSONStorable in array2)
		{
			if (!jSONStorable.exclude && !dictionary.ContainsKey(jSONStorable.storeId) && (isClear || !jSONStorable.onlyStoreIfActive || jSONStorable.gameObject.activeInHierarchy))
			{
				JSONClass jc2 = new JSONClass();
				jSONStorable.RestoreFromJSON(jc2, restorePhysical, restoreAppearance);
			}
		}
	}

	public void LateRestore(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool restoreCore = true)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		if (restoreCore)
		{
			if (jc["collisionEnabled"] != null)
			{
				collisionEnabledJSON.val = jc["collisionEnabled"].AsBool;
			}
			else
			{
				collisionEnabledJSON.val = collisionEnabledJSON.defaultVal;
			}
		}
		foreach (JSONClass item in jc["storables"].AsArray)
		{
			if (_storableById.TryGetValue(item["id"], out var value))
			{
				value.LateRestoreFromJSON(item, restorePhysical, restoreAppearance);
				if (!dictionary.ContainsKey(item["id"]))
				{
					dictionary.Add(item["id"], value: true);
				}
			}
		}
		JSONStorable[] array = _storables.ToArray();
		JSONStorable[] array2 = array;
		foreach (JSONStorable jSONStorable in array2)
		{
			if (!jSONStorable.exclude && !dictionary.ContainsKey(jSONStorable.storeId))
			{
				JSONClass jc2 = new JSONClass();
				jSONStorable.LateRestoreFromJSON(jc2, restorePhysical, restoreAppearance);
			}
		}
	}

	public new void Validate()
	{
		foreach (JSONStorable storable in _storables)
		{
			storable.Validate();
		}
	}

	public new void PreRestore()
	{
		foreach (JSONStorable storable in _storables)
		{
			storable.PreRestore();
		}
	}

	public new void PostRestore()
	{
		foreach (JSONStorable storable in _storables)
		{
			try
			{
				storable.PostRestore();
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during PostRestore of " + storable.storeId + ": " + ex);
			}
		}
	}

	public new void Remove()
	{
		SuperController.singleton.RemoveAtom(this);
	}

	public void OnRemove()
	{
		foreach (JSONStorable storable in _storables)
		{
			storable.Remove();
		}
	}

	public void Reset()
	{
		JSONClass jc = new JSONClass();
		loadedName = null;
		loadedPhysicalName = null;
		loadedAppearanceName = null;
		PreRestore();
		RestoreTransform(jc);
		RestoreParentAtom(jc);
		Restore(jc, restorePhysical: true, restoreAppearance: true, restoreCore: true, null, isClear: true);
		Restore(jc);
		LateRestore(jc);
		PostRestore();
		if ((bool)SuperController.singleton)
		{
			SuperController.singleton.PauseSimulation(5, "Reset " + uid);
		}
	}

	public void ResetPhysical()
	{
		JSONClass jc = new JSONClass();
		loadedName = null;
		loadedPhysicalName = null;
		loadedAppearanceName = null;
		PreRestore();
		RestoreTransform(jc);
		RestoreParentAtom(jc);
		Restore(jc, restorePhysical: true, restoreAppearance: false, restoreCore: false, null, isClear: true);
		Restore(jc, restorePhysical: true, restoreAppearance: false, restoreCore: false);
		LateRestore(jc, restorePhysical: true, restoreAppearance: false, restoreCore: false);
		PostRestore();
		if ((bool)SuperController.singleton)
		{
			SuperController.singleton.PauseSimulation(5, "ResetPhysical " + uid);
		}
	}

	public void ResetAppearance()
	{
		JSONClass jc = new JSONClass();
		loadedName = null;
		loadedPhysicalName = null;
		loadedAppearanceName = null;
		PreRestore();
		Restore(jc, restorePhysical: false, restoreAppearance: true, restoreCore: false, null, isClear: true);
		Restore(jc, restorePhysical: false, restoreAppearance: true, restoreCore: false);
		LateRestore(jc, restorePhysical: false, restoreAppearance: true, restoreCore: false);
		PostRestore();
	}

	public void SavePresetDialog(bool includePhysical = false, bool includeAppearance = false)
	{
		if (!(SuperController.singleton != null) || !(SuperController.singleton.fileBrowserUI != null))
		{
			return;
		}
		saveIncludePhysical = includePhysical;
		saveIncludeAppearance = includeAppearance;
		string text = SuperController.singleton.savesDir + type;
		if (saveIncludePhysical && saveIncludeAppearance)
		{
			text += "\\full";
			if (lastLoadPresetDir != string.Empty && FileManager.DirectoryExists(lastLoadPresetDir))
			{
				string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastLoadPresetDir, allowPackagePath: false);
				if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
				{
					text = suggestedBrowserDirectoryFromDirectoryPath;
				}
			}
			else if (!FileManager.DirectoryExists(text))
			{
				FileManager.CreateDirectory(text);
			}
		}
		else if (saveIncludePhysical)
		{
			text += "\\pose";
			if (lastLoadPhysicalDir != string.Empty && FileManager.DirectoryExists(lastLoadPhysicalDir))
			{
				string suggestedBrowserDirectoryFromDirectoryPath2 = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastLoadPhysicalDir, allowPackagePath: false);
				if (suggestedBrowserDirectoryFromDirectoryPath2 != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath2))
				{
					text = suggestedBrowserDirectoryFromDirectoryPath2;
				}
			}
			else if (!FileManager.DirectoryExists(text))
			{
				FileManager.CreateDirectory(text);
			}
		}
		else if (saveIncludeAppearance)
		{
			text += "\\appearance";
			if (lastLoadAppearanceDir != string.Empty && FileManager.DirectoryExists(lastLoadAppearanceDir))
			{
				string suggestedBrowserDirectoryFromDirectoryPath3 = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text, lastLoadAppearanceDir, allowPackagePath: false);
				if (suggestedBrowserDirectoryFromDirectoryPath3 != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath3))
				{
					text = suggestedBrowserDirectoryFromDirectoryPath3;
				}
			}
			else if (!FileManager.DirectoryExists(text))
			{
				FileManager.CreateDirectory(text);
			}
		}
		SuperController.singleton.fileBrowserUI.shortCuts = null;
		SuperController.singleton.fileBrowserUI.keepOpen = false;
		SuperController.singleton.fileBrowserUI.SetTitle("Select Save Preset File");
		SuperController.singleton.fileBrowserUI.defaultPath = text;
		SuperController.singleton.fileBrowserUI.SetTextEntry(b: true);
		SuperController.singleton.fileBrowserUI.Show(SavePreset);
		if (SuperController.singleton.fileBrowserUI.fileEntryField != null)
		{
			string text2 = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds/*cast due to .constrained prefix*/).ToString();
			SuperController.singleton.fileBrowserUI.fileEntryField.text = text2;
			SuperController.singleton.fileBrowserUI.ActivateFileNameField();
		}
	}

	public void SavePreset(string saveName)
	{
		if (saveName != string.Empty)
		{
			if (saveIncludePhysical && saveIncludeAppearance)
			{
				loadedName = saveName;
				lastLoadPresetDir = FileManager.GetDirectoryName(saveName, returnSlashPath: true);
			}
			else if (saveIncludePhysical)
			{
				loadedPhysicalName = saveName;
				lastLoadPhysicalDir = FileManager.GetDirectoryName(saveName, returnSlashPath: true);
			}
			else if (saveIncludeAppearance)
			{
				loadedAppearanceName = saveName;
				lastLoadAppearanceDir = FileManager.GetDirectoryName(saveName, returnSlashPath: true);
			}
			SuperController.singleton.Save(saveName, this, saveIncludePhysical, saveIncludeAppearance);
		}
	}

	public void LoadPresetDialog()
	{
		string text = SuperController.singleton.savesDir + type + "\\full";
		string text2 = text;
		if (lastLoadPresetDir != string.Empty && FileManager.DirectoryExists(lastLoadPresetDir))
		{
			string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text2, lastLoadPresetDir);
			if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
			{
				text2 = suggestedBrowserDirectoryFromDirectoryPath;
			}
		}
		else if (!FileManager.DirectoryExists(text2))
		{
			FileManager.CreateDirectory(text2);
		}
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(text, allowNavigationAboveRegularDirectories: true, useFullPaths: false, generateAllFlattenedShortcut: true, includeRegularDirsInFlattenedShortcut: true);
		SuperController.singleton.fileBrowserUI.shortCuts = shortCutsForDirectory;
		SuperController.singleton.fileBrowserUI.keepOpen = false;
		SuperController.singleton.fileBrowserUI.defaultPath = text2;
		SuperController.singleton.fileBrowserUI.SetTitle("Select Preset File");
		SuperController.singleton.fileBrowserUI.SetTextEntry(b: false);
		SuperController.singleton.fileBrowserUI.Show(LoadPreset);
	}

	public void LoadPreset(string saveName = "savefile")
	{
		if (!(saveName != string.Empty))
		{
			return;
		}
		try
		{
			using FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(saveName);
			lastLoadPresetDir = FileManager.GetDirectoryName(saveName);
			FileManager.SetLoadDirFromFilePath(saveName);
			string aJSON = fileEntryStreamReader.ReadToEnd();
			JSONNode jSONNode = JSON.Parse(aJSON);
			JSONArray asArray = jSONNode["atoms"].AsArray;
			loadedName = saveName;
			JSONClass asObject = asArray[0].AsObject;
			if (!(asObject != null))
			{
				return;
			}
			string text = asObject["type"];
			if (!(text == type))
			{
				return;
			}
			PreRestore();
			RestoreTransform(asObject);
			Restore(asObject, restorePhysical: true, restoreAppearance: true, restoreCore: false, asArray);
			LateRestore(asObject, restorePhysical: true, restoreAppearance: true, restoreCore: false);
			PostRestore();
			if (SuperController.singleton != null)
			{
				if (asObject["id"] != null)
				{
					SuperController.singleton.RenameAtom(this, asObject["id"]);
				}
				SuperController.singleton.PauseSimulation(5, "LoadPreset " + uid);
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during LoadPreset " + ex);
		}
	}

	public void LoadPhysicalPresetDialog()
	{
		string text = SuperController.singleton.savesDir + type + "\\pose";
		string text2 = text;
		if (lastLoadPhysicalDir != string.Empty && FileManager.DirectoryExists(lastLoadPhysicalDir))
		{
			string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text2, lastLoadPhysicalDir);
			if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
			{
				text2 = suggestedBrowserDirectoryFromDirectoryPath;
			}
		}
		else if (!FileManager.DirectoryExists(text2))
		{
			FileManager.CreateDirectory(text2);
		}
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(text, allowNavigationAboveRegularDirectories: true, useFullPaths: false, generateAllFlattenedShortcut: true, includeRegularDirsInFlattenedShortcut: true);
		SuperController.singleton.fileBrowserUI.shortCuts = shortCutsForDirectory;
		SuperController.singleton.fileBrowserUI.keepOpen = false;
		SuperController.singleton.fileBrowserUI.defaultPath = text2;
		SuperController.singleton.fileBrowserUI.SetTitle("Select Preset File");
		SuperController.singleton.fileBrowserUI.SetTextEntry(b: false);
		SuperController.singleton.fileBrowserUI.Show(LoadPhysicalPreset);
	}

	public void LoadPhysicalPreset(string saveName = "savefile")
	{
		if (!(saveName != string.Empty))
		{
			return;
		}
		try
		{
			using FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(saveName);
			lastLoadPhysicalDir = FileManager.GetDirectoryName(saveName, returnSlashPath: true);
			FileManager.SetLoadDirFromFilePath(saveName);
			string aJSON = fileEntryStreamReader.ReadToEnd();
			JSONNode jSONNode = JSON.Parse(aJSON);
			JSONArray asArray = jSONNode["atoms"].AsArray;
			loadedPhysicalName = saveName;
			JSONClass asObject = asArray[0].AsObject;
			if (!(asObject != null))
			{
				return;
			}
			string text = asObject["type"];
			if (text == type)
			{
				PreRestore();
				RestoreTransform(asObject);
				Restore(asObject, restorePhysical: true, restoreAppearance: false, restoreCore: false, asArray);
				LateRestore(asObject, restorePhysical: true, restoreAppearance: false, restoreCore: false);
				PostRestore();
				if (SuperController.singleton != null)
				{
					SuperController.singleton.PauseSimulation(5, "LoadPreset " + uid);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error during LoadPhysicalPreset " + ex);
		}
	}

	public void LoadAppearancePresetDialog()
	{
		string text = SuperController.singleton.savesDir + type + "\\appearance";
		string text2 = text;
		if (lastLoadAppearanceDir != string.Empty && FileManager.DirectoryExists(lastLoadAppearanceDir))
		{
			string suggestedBrowserDirectoryFromDirectoryPath = FileManager.GetSuggestedBrowserDirectoryFromDirectoryPath(text2, lastLoadAppearanceDir);
			if (suggestedBrowserDirectoryFromDirectoryPath != null && FileManager.DirectoryExists(suggestedBrowserDirectoryFromDirectoryPath))
			{
				text2 = suggestedBrowserDirectoryFromDirectoryPath;
			}
		}
		else if (!FileManager.DirectoryExists(text2))
		{
			FileManager.CreateDirectory(text2);
		}
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(text, allowNavigationAboveRegularDirectories: true, useFullPaths: false, generateAllFlattenedShortcut: true, includeRegularDirsInFlattenedShortcut: true);
		SuperController.singleton.fileBrowserUI.shortCuts = shortCutsForDirectory;
		SuperController.singleton.fileBrowserUI.keepOpen = false;
		SuperController.singleton.fileBrowserUI.defaultPath = text2;
		SuperController.singleton.fileBrowserUI.SetTitle("Select Preset File");
		SuperController.singleton.fileBrowserUI.SetTextEntry(b: false);
		SuperController.singleton.fileBrowserUI.Show(LoadAppearancePreset);
	}

	public void LoadAppearancePreset(string saveName = "savefile")
	{
		if (!(saveName != string.Empty))
		{
			return;
		}
		try
		{
			using FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(saveName);
			lastLoadAppearanceDir = FileManager.GetDirectoryName(saveName, returnSlashPath: true);
			FileManager.SetLoadDirFromFilePath(saveName);
			string aJSON = fileEntryStreamReader.ReadToEnd();
			JSONNode jSONNode = JSON.Parse(aJSON);
			JSONArray asArray = jSONNode["atoms"].AsArray;
			loadedAppearanceName = saveName;
			JSONClass asObject = asArray[0].AsObject;
			if (!(asObject != null))
			{
				return;
			}
			string text = asObject["type"];
			if (text == type)
			{
				Restore(asObject, restorePhysical: false, restoreAppearance: true, restoreCore: false);
				LateRestore(asObject, restorePhysical: false, restoreAppearance: true, restoreCore: false);
				if (SuperController.singleton != null && asObject["id"] != null)
				{
					SuperController.singleton.RenameAtom(this, asObject["id"]);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during LoadAppearancePreset " + ex);
		}
	}

	public void RegisterDynamicPhysicsSimulator(PhysicsSimulator ps)
	{
		if (_dynamicPhysicsSimulators == null)
		{
			_dynamicPhysicsSimulators = new List<PhysicsSimulator>();
		}
		ps.useInterpolation = _useRigidbodyInterpolation;
		ps.collisionEnabled = _collisionEnabled;
		if (waitResumeSimulationFlags != null)
		{
			foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
			{
				ps.PauseSimulation(waitResumeSimulationFlag);
			}
		}
		_dynamicPhysicsSimulators.Add(ps);
	}

	public void DeregisterDynamicPhysicsSimulator(PhysicsSimulator ps)
	{
		_dynamicPhysicsSimulators.Remove(ps);
	}

	public void RegisterDynamicScaleChangeReceiver(ScaleChangeReceiver scr)
	{
		if (_dynamicScaleChangeReceivers == null)
		{
			_dynamicScaleChangeReceivers = new List<ScaleChangeReceiver>();
		}
		scr.ScaleChanged(_currentScale);
		_dynamicScaleChangeReceivers.Add(scr);
	}

	public void DeregisterDynamicScaleChangeReceiver(ScaleChangeReceiver scr)
	{
		_dynamicScaleChangeReceivers.Remove(scr);
	}

	public void RegisterDynamicScaleChangeReceiverJSONStorable(ScaleChangeReceiverJSONStorable scr)
	{
		if (_dynamicScaleChangeReceiverJSONStorables == null)
		{
			_dynamicScaleChangeReceiverJSONStorables = new List<ScaleChangeReceiverJSONStorable>();
		}
		scr.ScaleChanged(_currentScale);
		_dynamicScaleChangeReceiverJSONStorables.Add(scr);
	}

	public void DeregisterDynamicScaleChangeReceiverJSONStorable(ScaleChangeReceiverJSONStorable scr)
	{
		_dynamicScaleChangeReceiverJSONStorables.Remove(scr);
	}

	public void SetParentAtomSelectPopupValues()
	{
		if (!(parentAtomSelectionPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		List<string> atomUIDs = SuperController.singleton.GetAtomUIDs();
		if (atomUIDs == null)
		{
			parentAtomSelectionPopup.numPopupValues = 1;
			parentAtomSelectionPopup.setPopupValue(0, "None");
			return;
		}
		parentAtomSelectionPopup.numPopupValues = atomUIDs.Count + 1;
		parentAtomSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < atomUIDs.Count; i++)
		{
			parentAtomSelectionPopup.setPopupValue(i + 1, atomUIDs[i]);
		}
	}

	public bool RegisterAdditionalStorable(JSONStorable js)
	{
		if (js != null && !js.exclude)
		{
			if (!_storableById.ContainsKey(js.storeId))
			{
				js.containingAtom = this;
				_storables.Add(js);
				_storableById.Add(js.storeId, js);
				return true;
			}
			Debug.LogError("Found duplicate storable uid " + js.storeId + " in atom " + uid);
		}
		return false;
	}

	public void AddCanvas(Canvas c)
	{
		_canvases.Add(c);
		if (SuperController.singleton != null)
		{
			SuperController.singleton.AddCanvas(c);
		}
	}

	public void RemoveCanvas(Canvas c)
	{
		_canvases.Remove(c);
		if (SuperController.singleton != null)
		{
			SuperController.singleton.RemoveCanvas(c);
		}
	}

	public void UnregisterAdditionalStorable(JSONStorable js)
	{
		if (js != null)
		{
			if (_storableById.ContainsKey(js.storeId))
			{
				_storableById.Remove(js.storeId);
			}
			js.containingAtom = null;
			_storables.Remove(js);
		}
	}

	public virtual void SetParentAtom(string atomUID)
	{
		if (SuperController.singleton != null)
		{
			Atom atomByUid = SuperController.singleton.GetAtomByUid(atomUID);
			parentAtom = atomByUid;
		}
	}

	protected void OnAtomRename(string oldid, string newid)
	{
		if (parentAtom != null && parentAtomSelectionPopup != null)
		{
			parentAtomSelectionPopup.currentValueNoCallback = parentAtom.uid;
		}
	}

	public void SelectAtomParent(Atom a)
	{
		if (parentAtomSelectionPopup != null)
		{
			if (a == null)
			{
				parentAtomSelectionPopup.currentValue = "None";
			}
			else
			{
				parentAtomSelectionPopup.currentValue = a.uid;
			}
		}
		parentAtom = a;
	}

	public void SelectAtomParentFromScene()
	{
		SetParentAtomSelectPopupValues();
		SuperController.singleton.SelectModeAtom(SelectAtomParent);
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		AtomUI componentInChildren = UITransform.GetComponentInChildren<AtomUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		onJSON.toggle = componentInChildren.onToggle;
		hiddenJSON.toggle = componentInChildren.hiddenToggle;
		collisionEnabledJSON.toggle = componentInChildren.collisionEnabledToggle;
		parentAtomSelectionPopup = componentInChildren.parentAtomSelectionPopup;
		if (parentAtomSelectionPopup != null)
		{
			parentAtomSelectionPopup.numPopupValues = 1;
			parentAtomSelectionPopup.setPopupValue(0, "None");
			if (parentAtom != null)
			{
				parentAtomSelectionPopup.currentValue = parentAtom.uid;
			}
			else
			{
				parentAtomSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup = parentAtomSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetParentAtomSelectPopupValues));
			UIPopup uIPopup2 = parentAtomSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetParentAtom));
		}
		if (componentInChildren.selectAtomParentFromSceneButton != null)
		{
			componentInChildren.selectAtomParentFromSceneButton.onClick.AddListener(SelectAtomParentFromScene);
		}
		if (componentInChildren.resetButton != null)
		{
			componentInChildren.resetButton.onClick.AddListener(delegate
			{
				Reset();
			});
		}
		if (componentInChildren.resetPhysicalButton != null)
		{
			componentInChildren.resetPhysicalButton.onClick.AddListener(delegate
			{
				ResetPhysical();
			});
		}
		if (componentInChildren.resetAppearanceButton != null)
		{
			componentInChildren.resetAppearanceButton.onClick.AddListener(delegate
			{
				ResetAppearance();
			});
		}
		if (componentInChildren.removeButton != null)
		{
			componentInChildren.removeButton.onClick.AddListener(delegate
			{
				Remove();
			});
		}
		if (componentInChildren.savePresetButton != null)
		{
			componentInChildren.savePresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(includePhysical: true, includeAppearance: true);
			});
		}
		if (componentInChildren.saveAppearancePresetButton != null)
		{
			componentInChildren.saveAppearancePresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(includePhysical: false, includeAppearance: true);
			});
		}
		if (componentInChildren.savePhysicalPresetButton != null)
		{
			componentInChildren.savePhysicalPresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(includePhysical: true);
			});
		}
		if (componentInChildren.loadPresetButton != null)
		{
			componentInChildren.loadPresetButton.onClick.AddListener(delegate
			{
				LoadPresetDialog();
			});
		}
		if (componentInChildren.loadAppearancePresetButton != null)
		{
			componentInChildren.loadAppearancePresetButton.onClick.AddListener(delegate
			{
				LoadAppearancePresetDialog();
			});
		}
		if (componentInChildren.loadPhysicalPresetButton != null)
		{
			componentInChildren.loadPhysicalPresetButton.onClick.AddListener(delegate
			{
				LoadPhysicalPresetDialog();
			});
		}
		idText = componentInChildren.idText;
		if (idText != null)
		{
			idText.onEndEdit.AddListener(SetUID);
		}
		idTextAction = componentInChildren.idTextAction;
		if (idTextAction != null)
		{
			InputFieldAction inputFieldAction = idTextAction;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetUIDToInputField));
		}
		descriptionText = componentInChildren.descriptionText;
		SyncIdText();
		SyncDescriptionText();
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		AtomUI componentInChildren = UITransformAlt.GetComponentInChildren<AtomUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		onJSON.toggleAlt = componentInChildren.onToggle;
		hiddenJSON.toggleAlt = componentInChildren.hiddenToggle;
		collisionEnabledJSON.toggleAlt = componentInChildren.collisionEnabledToggle;
		if (componentInChildren.selectAtomParentFromSceneButton != null)
		{
			componentInChildren.selectAtomParentFromSceneButton.onClick.AddListener(SelectAtomParentFromScene);
		}
		if (componentInChildren.resetButton != null)
		{
			componentInChildren.resetButton.onClick.AddListener(delegate
			{
				Reset();
			});
		}
		if (componentInChildren.resetPhysicalButton != null)
		{
			componentInChildren.resetPhysicalButton.onClick.AddListener(delegate
			{
				ResetPhysical();
			});
		}
		if (componentInChildren.resetAppearanceButton != null)
		{
			componentInChildren.resetAppearanceButton.onClick.AddListener(delegate
			{
				ResetAppearance();
			});
		}
		if (componentInChildren.removeButton != null)
		{
			componentInChildren.removeButton.onClick.AddListener(delegate
			{
				Remove();
			});
		}
		if (componentInChildren.savePresetButton != null)
		{
			componentInChildren.savePresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(includePhysical: true, includeAppearance: true);
			});
		}
		if (componentInChildren.saveAppearancePresetButton != null)
		{
			componentInChildren.saveAppearancePresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(includePhysical: false, includeAppearance: true);
			});
		}
		if (componentInChildren.savePhysicalPresetButton != null)
		{
			componentInChildren.savePhysicalPresetButton.onClick.AddListener(delegate
			{
				SavePresetDialog(includePhysical: true);
			});
		}
		if (componentInChildren.loadPresetButton != null)
		{
			componentInChildren.loadPresetButton.onClick.AddListener(delegate
			{
				LoadPresetDialog();
			});
		}
		if (componentInChildren.loadAppearancePresetButton != null)
		{
			componentInChildren.loadAppearancePresetButton.onClick.AddListener(delegate
			{
				LoadAppearancePresetDialog();
			});
		}
		if (componentInChildren.loadPhysicalPresetButton != null)
		{
			componentInChildren.loadPhysicalPresetButton.onClick.AddListener(delegate
			{
				LoadPhysicalPresetDialog();
			});
		}
		idTextAlt = componentInChildren.idText;
		if (idTextAlt != null)
		{
			idTextAlt.onEndEdit.AddListener(SetUID);
		}
		idTextActionAlt = componentInChildren.idTextAction;
		if (idTextActionAlt != null)
		{
			InputFieldAction inputFieldAction = idTextActionAlt;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetUIDToInputField));
		}
		descriptionTextAlt = componentInChildren.descriptionText;
		SyncIdText();
		SyncDescriptionText();
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}

	private void Start()
	{
		if (reParentObject != null)
		{
			reParentObjectStartingPosition = reParentObject.position;
			reParentObjectStartingRotation = reParentObject.rotation;
		}
		if (childAtomContainer != null)
		{
			childAtomContainerStartingPosition = childAtomContainer.position;
			childAtomContainerStartingRotation = childAtomContainer.rotation;
		}
		SyncMasterControllerCorners();
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			CheckResumeSimulation();
		}
		if (!(_masterController != null) || masterControllerCorners == null || masterControllerCorners.Length < 8)
		{
			return;
		}
		if (_freeControllers.Length > 1 || (alwaysShowExtents && _freeControllers.Length > 0))
		{
			Vector3 position = _freeControllers[0].transform.position;
			extentLowX = position.x;
			extentHighX = position.x;
			extentLowY = position.y;
			extentHighY = position.y;
			extentLowZ = position.z;
			extentHighZ = position.z;
			FreeControllerV3[] array = _freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				if (freeControllerV != _masterController)
				{
					position = freeControllerV.transform.position;
					if (position.x > extentHighX)
					{
						extentHighX = position.x;
					}
					else if (position.x < extentLowX)
					{
						extentLowX = position.x;
					}
					if (position.y > extentHighY)
					{
						extentHighY = position.y;
					}
					else if (position.y < extentLowY)
					{
						extentLowY = position.y;
					}
					if (position.z > extentHighZ)
					{
						extentHighZ = position.z;
					}
					else if (position.z < extentLowZ)
					{
						extentLowZ = position.z;
					}
				}
			}
			extentLowX -= extentPadding;
			extentLowY -= extentPadding;
			extentLowZ -= extentPadding;
			extentHighX += extentPadding;
			extentHighY += extentPadding;
			extentHighZ += extentPadding;
			extentlll.x = extentLowX;
			extentlll.y = extentLowY;
			extentlll.z = extentLowZ;
			extentllh.x = extentLowX;
			extentllh.y = extentLowY;
			extentllh.z = extentHighZ;
			extentlhl.x = extentLowX;
			extentlhl.y = extentHighY;
			extentlhl.z = extentLowZ;
			extentlhh.x = extentLowX;
			extentlhh.y = extentHighY;
			extentlhh.z = extentHighZ;
			extenthll.x = extentHighX;
			extenthll.y = extentLowY;
			extenthll.z = extentLowZ;
			extenthlh.x = extentHighX;
			extenthlh.y = extentLowY;
			extenthlh.z = extentHighZ;
			extenthhl.x = extentHighX;
			extenthhl.y = extentHighY;
			extenthhl.z = extentLowZ;
			extenthhh.x = extentHighX;
			extenthhh.y = extentHighY;
			extenthhh.z = extentHighZ;
			masterControllerCorners[0].position = extentlll;
			masterControllerCorners[1].position = extentllh;
			masterControllerCorners[2].position = extentlhl;
			masterControllerCorners[3].position = extentlhh;
			masterControllerCorners[4].position = extenthll;
			masterControllerCorners[5].position = extenthlh;
			masterControllerCorners[6].position = extenthhl;
			masterControllerCorners[7].position = extenthhh;
			Transform[] array2 = masterControllerCorners;
			foreach (Transform transform in array2)
			{
				transform.gameObject.SetActive(value: true);
			}
		}
		else
		{
			Transform[] array3 = masterControllerCorners;
			foreach (Transform transform2 in array3)
			{
				transform2.gameObject.SetActive(value: false);
			}
		}
	}
}
