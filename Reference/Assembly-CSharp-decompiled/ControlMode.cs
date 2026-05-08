using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class FreeControllerV3 : ScaleChangeReceiverJSONStorable
{
	public enum SelectLinkState
	{
		PositionAndRotation,
		Position,
		Rotation
	}

	public enum PositionState
	{
		On,
		Off,
		Following,
		Hold,
		Lock,
		ParentLink,
		PhysicsLink,
		Comply
	}

	public enum RotationState
	{
		On,
		Off,
		Following,
		Hold,
		Lock,
		LookAt,
		ParentLink,
		PhysicsLink,
		Comply
	}

	public enum GridMode
	{
		None,
		Local,
		Global
	}

	public enum MoveAxisnames
	{
		X,
		Y,
		Z,
		CameraRight,
		CameraUp,
		CameraForward,
		CameraRightNoY,
		CameraForwardNoY,
		None
	}

	public enum RotateAxisnames
	{
		X,
		Y,
		Z,
		NegX,
		NegY,
		NegZ,
		WorldY,
		None
	}

	public enum DrawAxisnames
	{
		X,
		Y,
		Z,
		NegX,
		NegY,
		NegZ
	}

	public enum ControlMode
	{
		Off,
		Position,
		Rotation
	}

	public static float targetAlpha = 1f;

	protected List<UnityEngine.Object> allocatedObjects;

	public bool storePositionRotationAsLocal;

	public bool enableSelectRoot;

	public UIPopup linkToSelectionPopup;

	public UIPopup linkToAtomSelectionPopup;

	private Rigidbody _linkToRB;

	private Transform _linkToConnector;

	private ConfigurableJoint _linkToJoint;

	protected string linkToAtomUID;

	private Rigidbody preLinkRB;

	private PositionState preLinkPositionState;

	private RotationState preLinkRotationState;

	public bool stateCanBeModified = true;

	public PositionState startingPositionState;

	private JSONStorableStringChooser currentPositionStateJSON;

	private PositionState _currentPositionState;

	public RotationState startingRotationState;

	private JSONStorableStringChooser currentRotationStateJSON;

	private RotationState _currentRotationState;

	protected float scalePow = 1f;

	public Quaternion2Angles.RotationOrder naturalJointDriveRotationOrder;

	private JSONStorableBool xLockJSON;

	[SerializeField]
	private bool _xLock;

	private JSONStorableBool yLockJSON;

	[SerializeField]
	private bool _yLock;

	private JSONStorableBool zLockJSON;

	[SerializeField]
	private bool _zLock;

	private JSONStorableBool xLocalLockJSON;

	[SerializeField]
	private bool _xLocalLock;

	private JSONStorableBool yLocalLockJSON;

	[SerializeField]
	private bool _yLocalLock;

	private JSONStorableBool zLocalLockJSON;

	[SerializeField]
	private bool _zLocalLock;

	private JSONStorableBool xRotLockJSON;

	[SerializeField]
	private bool _xRotLock;

	private JSONStorableBool yRotLockJSON;

	[SerializeField]
	private bool _yRotLock;

	private JSONStorableBool zRotLockJSON;

	[SerializeField]
	private bool _zRotLock;

	public SetTextFromFloat xPositionText;

	public SetTextFromFloat xPositionTextAlt;

	public SetTextFromFloat yPositionText;

	public SetTextFromFloat yPositionTextAlt;

	public SetTextFromFloat zPositionText;

	public SetTextFromFloat zPositionTextAlt;

	public SetTextFromFloat xRotationText;

	public SetTextFromFloat xRotationTextAlt;

	public SetTextFromFloat yRotationText;

	public SetTextFromFloat yRotationTextAlt;

	public SetTextFromFloat zRotationText;

	public SetTextFromFloat zRotationTextAlt;

	public bool controlsOn;

	protected JSONStorableBool onJSON;

	[SerializeField]
	private bool _on = true;

	protected JSONStorableBool interactableInPlayModeJSON;

	[SerializeField]
	private bool _interactableInPlayMode = true;

	public FreeControllerV3[] onPossessDeactiveList;

	[SerializeField]
	private bool _possessed;

	public bool startedPossess;

	public Transform possessPoint;

	protected JSONStorableBool possessableJSON;

	[SerializeField]
	private bool _possessable;

	protected JSONStorableBool canGrabPositionJSON;

	[SerializeField]
	private bool _canGrabPosition = true;

	protected JSONStorableBool canGrabRotationJSON;

	[SerializeField]
	private bool _canGrabRotation = true;

	private JSONStorableStringChooser positionGridModeJSON;

	[SerializeField]
	private GridMode _positionGridMode;

	private JSONStorableFloat positionGridJSON;

	[SerializeField]
	private float _positionGrid = 0.1f;

	private JSONStorableStringChooser rotationGridModeJSON;

	[SerializeField]
	private GridMode _rotationGridMode;

	private JSONStorableFloat rotationGridJSON;

	[SerializeField]
	private float _rotationGrid = 15f;

	private JSONStorableBool useGravityJSON;

	[SerializeField]
	private bool _useGravityOnRBWhenOff = true;

	private JSONStorableBool physicsEnabledJSON;

	public bool controlsCollisionEnabled;

	private bool _globalCollisionEnabled = true;

	private JSONStorableBool collisionEnabledJSON;

	private bool _collisionEnabled;

	private Rigidbody _followWhenOffRB;

	private Rigidbody kinematicRB;

	private ConfigurableJoint connectedJoint;

	private ConfigurableJoint naturalJoint;

	public bool useForceWhenOff = true;

	public float distanceHolder;

	public float forceFactor = 10000f;

	public float torqueFactor = 2000f;

	public Rigidbody[] rigidbodySlavesForMass;

	private JSONStorableFloat RBMassJSON;

	private JSONStorableFloat RBDragJSON;

	private JSONStorableFloat RBAngularDragJSON;

	[SerializeField]
	private float _RBLockPositionSpring = 250000f;

	[SerializeField]
	private float _RBLockPositionDamper = 250f;

	[SerializeField]
	public float _RBLockPositionMaxForce = 100000000f;

	private JSONStorableFloat RBHoldPositionSpringJSON;

	[SerializeField]
	private float _RBHoldPositionSpring = 1000f;

	private JSONStorableFloat RBHoldPositionDamperJSON;

	[SerializeField]
	private float _RBHoldPositionDamper = 50f;

	private JSONStorableFloat RBHoldPositionMaxForceJSON;

	[SerializeField]
	private float _RBHoldPositionMaxForce = 10000f;

	private JSONStorableFloat RBComplyPositionSpringJSON;

	[SerializeField]
	private float _RBComplyPositionSpring = 1500f;

	private JSONStorableFloat RBComplyPositionDamperJSON;

	[SerializeField]
	private float _RBComplyPositionDamper = 100f;

	private float _RBComplyPositionMaxForce = 1E+13f;

	private JSONStorableFloat RBLinkPositionSpringJSON;

	[SerializeField]
	private float _RBLinkPositionSpring = 250000f;

	private JSONStorableFloat RBLinkPositionDamperJSON;

	[SerializeField]
	private float _RBLinkPositionDamper = 250f;

	private JSONStorableFloat RBLinkPositionMaxForceJSON;

	[SerializeField]
	private float _RBLinkPositionMaxForce = 100000000f;

	[SerializeField]
	private float _RBLockRotationSpring = 250000f;

	[SerializeField]
	private float _RBLockRotationDamper = 250f;

	[SerializeField]
	public float _RBLockRotationMaxForce = 100000000f;

	private JSONStorableFloat RBHoldRotationSpringJSON;

	[SerializeField]
	private float _RBHoldRotationSpring = 1000f;

	private JSONStorableFloat RBHoldRotationDamperJSON;

	[SerializeField]
	private float _RBHoldRotationDamper = 50f;

	private JSONStorableFloat RBHoldRotationMaxForceJSON;

	[SerializeField]
	private float _RBHoldRotationMaxForce = 10000f;

	private JSONStorableFloat RBComplyRotationSpringJSON;

	[SerializeField]
	private float _RBComplyRotationSpring = 150f;

	private JSONStorableFloat RBComplyRotationDamperJSON;

	[SerializeField]
	private float _RBComplyRotationDamper = 10f;

	private float _RBComplyRotationMaxForce = 1E+13f;

	private JSONStorableFloat RBLinkRotationSpringJSON;

	[SerializeField]
	private float _RBLinkRotationSpring = 250000f;

	private JSONStorableFloat RBLinkRotationDamperJSON;

	[SerializeField]
	private float _RBLinkRotationDamper = 250f;

	private JSONStorableFloat RBLinkRotationMaxForceJSON;

	[SerializeField]
	private float _RBLinkRotationMaxForce = 100000000f;

	private JSONStorableFloat RBComplyJointRotationDriveSpringJSON;

	[SerializeField]
	private float _RBComplyJointRotationDriveSpring = 20f;

	private JSONStorableFloat jointRotationDriveSpringJSON;

	[SerializeField]
	private float _jointRotationDriveSpring;

	private JSONStorableFloat jointRotationDriveDamperJSON;

	[SerializeField]
	private float _jointRotationDriveDamper;

	private JSONStorableFloat jointRotationDriveMaxForceJSON;

	[SerializeField]
	private float _jointRotationDriveMaxForce;

	private JSONStorableFloat jointRotationDriveXTargetJSON;

	private float _jointRotationDriveXTargetMin;

	private float _jointRotationDriveXTargetMax;

	[SerializeField]
	private float _jointRotationDriveXTarget;

	[SerializeField]
	private float _jointRotationDriveXTargetAdditional;

	private JSONStorableFloat jointRotationDriveYTargetJSON;

	private float _jointRotationDriveYTargetMin;

	private float _jointRotationDriveYTargetMax;

	[SerializeField]
	private float _jointRotationDriveYTarget;

	[SerializeField]
	private float _jointRotationDriveYTargetAdditional;

	private JSONStorableFloat jointRotationDriveZTargetJSON;

	private float _jointRotationDriveZTargetMin;

	private float _jointRotationDriveZTargetMax;

	[SerializeField]
	private float _jointRotationDriveZTarget;

	[SerializeField]
	private float _jointRotationDriveZTargetAdditional;

	public Text UIDText;

	public Text UIDTextAlt;

	public Transform[] UITransforms;

	public Transform[] UITransformsPlayMode;

	public bool GUIalwaysVisibleWhenSelected;

	public bool useContainedMeshRenderers = true;

	private bool _hidden = true;

	private bool _guihidden = true;

	public float unhighlightedScale = 0.5f;

	public float highlightedScale = 0.5f;

	public float selectedScale = 1f;

	private bool _highlighted;

	private Vector3 _selectedPosition;

	private bool _selected;

	public Color onColor = new Color(0f, 1f, 0f, 0.5f);

	public Color offColor = new Color(1f, 0f, 0f, 0.5f);

	public Color followingColor = new Color(1f, 0f, 1f, 0.5f);

	public Color holdColor = new Color(1f, 0.5f, 0f, 0.5f);

	public Color lockColor = new Color(0.5f, 0.25f, 0f, 0.5f);

	public Color lookAtColor = new Color(0f, 1f, 1f, 0.5f);

	public Color highlightColor = new Color(1f, 1f, 0f, 0.5f);

	public Color selectedColor = new Color(0f, 0f, 1f, 0.5f);

	public Color overlayColor = new Color(1f, 1f, 1f, 0.5f);

	private Color _currentPositionColor;

	private Color _currentRotationColor;

	public Material material;

	public Material linkLineMaterial;

	private LineDrawer linkLineDrawer;

	private Material positionMaterialLocal;

	private Material rotationMaterialLocal;

	private Material snapshotMaterialLocal;

	private Material materialOverlay;

	public float meshScale = 0.5f;

	private Mesh _currentPositionMesh;

	private Mesh _currentRotationMesh;

	public bool drawSnapshot;

	private Matrix4x4 snapshotMatrix;

	public bool drawMesh = true;

	public bool drawMeshWhenDeselected = true;

	public Mesh onPositionMesh;

	public Mesh offPositionMesh;

	public Mesh followingPositionMesh;

	public Mesh holdPositionMesh;

	public Mesh lockPositionMesh;

	public Mesh onRotationMesh;

	public Mesh offRotationMesh;

	public Mesh followingRotationMesh;

	public Mesh holdRotationMesh;

	public Mesh lockRotationMesh;

	public Mesh lookAtRotationMesh;

	public Mesh moveModeOverlayMesh;

	public Mesh rotateModeOverlayMesh;

	public Mesh deselectedMesh;

	public float deselectedMeshScale = 0.5f;

	public bool debug;

	public Transform control;

	public Transform follow;

	public Transform followWhenOff;

	public Transform lookAt;

	public Transform alsoMoveWhenInactive;

	public Transform alsoMoveWhenInactiveParentWhenActive;

	public Transform alsoMoveWhenInactiveParentWhenInactive;

	public Transform focusPoint;

	public MoveAxisnames MoveAxis1 = MoveAxisnames.CameraRightNoY;

	public MoveAxisnames MoveAxis2 = MoveAxisnames.CameraForwardNoY;

	public MoveAxisnames MoveAxis3 = MoveAxisnames.Y;

	public RotateAxisnames RotateAxis1 = RotateAxisnames.Z;

	public RotateAxisnames RotateAxis2;

	public RotateAxisnames RotateAxis3 = RotateAxisnames.Y;

	public DrawAxisnames MeshForwardAxis = DrawAxisnames.Y;

	public DrawAxisnames MeshUpAxis = DrawAxisnames.Z;

	public DrawAxisnames DrawForwardAxis = DrawAxisnames.Z;

	public DrawAxisnames DrawUpAxis = DrawAxisnames.Y;

	public DrawAxisnames PossessForwardAxis = DrawAxisnames.Z;

	public DrawAxisnames PossessUpAxis = DrawAxisnames.Y;

	public float moveFactor = 1f;

	public float rotateFactor = 60f;

	private bool _moveEnabled = true;

	private bool _moveForceEnabled;

	private bool _rotationEnabled = true;

	private bool _rotationForceEnabled;

	private Vector3 appliedForce;

	private Vector3 appliedTorque;

	private ControlMode _controlMode = ControlMode.Position;

	public Vector3 startingPosition;

	public Quaternion startingRotation;

	public Vector3 startingLocalPosition;

	public Quaternion startingLocalRotation;

	private Vector3 initialLocalPosition;

	private Quaternion initialLocalRotation;

	private MeshRenderer[] mrs;

	protected FreeControllerV3UI currentFCUI;

	protected FreeControllerV3UI currentFCUIAlt;

	protected int complyPauseFrames;

	[SerializeField]
	protected float complyPositionThreshold = 0.001f;

	protected JSONStorableFloat complyPositionThresholdJSON;

	[SerializeField]
	protected float complyRotationThreshold = 5f;

	protected JSONStorableFloat complyRotationThresholdJSON;

	[SerializeField]
	protected float complySpeed = 10f;

	protected JSONStorableFloat complySpeedJSON;

	private bool wasInit;

	public Rigidbody linkToRB
	{
		get
		{
			return _linkToRB;
		}
		set
		{
			if (!(_linkToRB != value))
			{
				return;
			}
			if (_linkToConnector != null)
			{
				UnityEngine.Object.DestroyImmediate(_linkToConnector.gameObject);
				_linkToConnector = null;
			}
			if (_linkToJoint != null)
			{
				UnityEngine.Object.DestroyImmediate(_linkToJoint);
				_linkToJoint = null;
			}
			_linkToRB = value;
			if (_linkToRB != null)
			{
				if (_followWhenOffRB != null && _linkToRB != null)
				{
					GameObject gameObject = _followWhenOffRB.gameObject;
					_linkToJoint = gameObject.AddComponent<ConfigurableJoint>();
					_linkToJoint.connectedBody = _linkToRB;
					_linkToJoint.xMotion = ConfigurableJointMotion.Free;
					_linkToJoint.yMotion = ConfigurableJointMotion.Free;
					_linkToJoint.zMotion = ConfigurableJointMotion.Free;
					_linkToJoint.angularXMotion = ConfigurableJointMotion.Free;
					_linkToJoint.angularYMotion = ConfigurableJointMotion.Free;
					_linkToJoint.angularZMotion = ConfigurableJointMotion.Free;
					_linkToJoint.rotationDriveMode = RotationDriveMode.Slerp;
					SetLinkedJointSprings();
				}
				GameObject gameObject2 = new GameObject();
				_linkToConnector = gameObject2.transform;
				_linkToConnector.position = base.transform.position;
				_linkToConnector.rotation = base.transform.rotation;
				_linkToConnector.SetParent(_linkToRB.transform);
			}
		}
	}

	public PositionState currentPositionState
	{
		get
		{
			return _currentPositionState;
		}
		set
		{
			if (stateCanBeModified)
			{
				if (currentPositionStateJSON != null)
				{
					currentPositionStateJSON.val = value.ToString();
				}
				else if (_currentPositionState != value)
				{
					_currentPositionState = value;
					SyncPositionState();
				}
			}
		}
	}

	public bool isPositionOn => _currentPositionState == PositionState.On || _currentPositionState == PositionState.Comply || _currentPositionState == PositionState.Following || _currentPositionState == PositionState.Hold || _currentPositionState == PositionState.ParentLink || _currentPositionState == PositionState.PhysicsLink;

	public RotationState currentRotationState
	{
		get
		{
			return _currentRotationState;
		}
		set
		{
			if (stateCanBeModified)
			{
				if (currentRotationStateJSON != null)
				{
					currentRotationStateJSON.val = value.ToString();
				}
				else if (_currentRotationState != value)
				{
					_currentRotationState = value;
					SyncRotationState();
				}
			}
		}
	}

	public bool isRotationOn => _currentRotationState == RotationState.On || _currentRotationState == RotationState.Comply || _currentRotationState == RotationState.Following || _currentRotationState == RotationState.Hold || _currentRotationState == RotationState.LookAt || _currentRotationState == RotationState.ParentLink || _currentPositionState == PositionState.PhysicsLink;

	public bool xLock
	{
		get
		{
			return _xLock;
		}
		set
		{
			if (xLockJSON != null)
			{
				xLockJSON.val = value;
			}
			else if (_xLock != value)
			{
				SyncXLock(value);
			}
		}
	}

	public bool yLock
	{
		get
		{
			return _yLock;
		}
		set
		{
			if (yLockJSON != null)
			{
				yLockJSON.val = value;
			}
			else if (_yLock != value)
			{
				SyncYLock(value);
			}
		}
	}

	public bool zLock
	{
		get
		{
			return _zLock;
		}
		set
		{
			if (zLockJSON != null)
			{
				zLockJSON.val = value;
			}
			else if (_zLock != value)
			{
				SyncZLock(value);
			}
		}
	}

	public bool xLocalLock
	{
		get
		{
			return _xLocalLock;
		}
		set
		{
			if (xLocalLockJSON != null)
			{
				xLocalLockJSON.val = value;
			}
			else if (_xLocalLock != value)
			{
				SyncXLocalLock(value);
			}
		}
	}

	public bool yLocalLock
	{
		get
		{
			return _yLocalLock;
		}
		set
		{
			if (yLocalLockJSON != null)
			{
				yLocalLockJSON.val = value;
			}
			else if (_yLocalLock != value)
			{
				SyncYLocalLock(value);
			}
		}
	}

	public bool zLocalLock
	{
		get
		{
			return _zLocalLock;
		}
		set
		{
			if (zLocalLockJSON != null)
			{
				zLocalLockJSON.val = value;
			}
			else if (_zLocalLock != value)
			{
				SyncZLocalLock(value);
			}
		}
	}

	public bool xRotLock
	{
		get
		{
			return _xRotLock;
		}
		set
		{
			if (xRotLockJSON != null)
			{
				xRotLockJSON.val = value;
			}
			else if (_xRotLock != value)
			{
				SyncXRotLock(value);
			}
		}
	}

	public bool yRotLock
	{
		get
		{
			return _yRotLock;
		}
		set
		{
			if (yRotLockJSON != null)
			{
				yRotLockJSON.val = value;
			}
			else if (_yRotLock != value)
			{
				SyncYRotLock(value);
			}
		}
	}

	public bool zRotLock
	{
		get
		{
			return _zRotLock;
		}
		set
		{
			if (zRotLockJSON != null)
			{
				zRotLockJSON.val = value;
			}
			else if (_zRotLock != value)
			{
				SyncZRotLock(value);
			}
		}
	}

	public bool on
	{
		get
		{
			return _on;
		}
		set
		{
			if (onJSON != null)
			{
				onJSON.val = value;
			}
			else if (_on != value)
			{
				SyncOn(value);
			}
		}
	}

	public bool interactableInPlayMode
	{
		get
		{
			return _interactableInPlayMode;
		}
		set
		{
			if (interactableInPlayModeJSON != null)
			{
				interactableInPlayModeJSON.val = value;
			}
			else if (_interactableInPlayMode != value)
			{
				SyncInteractableInPlayMode(value);
			}
		}
	}

	public bool possessed
	{
		get
		{
			return _possessed;
		}
		set
		{
			if (_possessed == value)
			{
				return;
			}
			_possessed = value;
			if (_possessed && onPossessDeactiveList != null)
			{
				FreeControllerV3[] array = onPossessDeactiveList;
				foreach (FreeControllerV3 freeControllerV in array)
				{
					freeControllerV.currentPositionState = PositionState.Off;
					freeControllerV.currentRotationState = RotationState.Off;
				}
			}
		}
	}

	public bool possessable
	{
		get
		{
			return _possessable;
		}
		set
		{
			if (possessableJSON != null)
			{
				possessableJSON.val = value;
			}
			else if (_possessable != value)
			{
				SyncPossessable(value);
			}
		}
	}

	public bool canGrabPosition
	{
		get
		{
			return _canGrabPosition;
		}
		set
		{
			if (canGrabPositionJSON != null)
			{
				canGrabPositionJSON.val = value;
			}
			else if (_canGrabPosition != value)
			{
				SyncCanGrabPosition(value);
			}
		}
	}

	public bool canGrabRotation
	{
		get
		{
			return _canGrabRotation;
		}
		set
		{
			if (canGrabRotationJSON != null)
			{
				canGrabRotationJSON.val = value;
			}
			else if (_canGrabRotation != value)
			{
				SyncCanGrabRotation(value);
			}
		}
	}

	public GridMode positionGridMode
	{
		get
		{
			return _positionGridMode;
		}
		set
		{
			if (positionGridModeJSON != null)
			{
				positionGridModeJSON.val = value.ToString();
			}
			else if (_positionGridMode != value)
			{
				_positionGridMode = value;
			}
		}
	}

	public float positionGrid
	{
		get
		{
			return _positionGrid;
		}
		set
		{
			if (positionGridJSON != null)
			{
				positionGridJSON.val = value;
			}
			else
			{
				SyncPositionGrid(value);
			}
		}
	}

	public GridMode rotationGridMode
	{
		get
		{
			return _rotationGridMode;
		}
		set
		{
			if (rotationGridModeJSON != null)
			{
				rotationGridModeJSON.val = value.ToString();
			}
			else if (_rotationGridMode != value)
			{
				_rotationGridMode = value;
			}
		}
	}

	public float rotationGrid
	{
		get
		{
			return _rotationGrid;
		}
		set
		{
			if (rotationGridJSON != null)
			{
				rotationGridJSON.val = value;
			}
			else
			{
				SyncRotationGrid(value);
			}
		}
	}

	public bool useGravityOnRBWhenOff
	{
		get
		{
			return _useGravityOnRBWhenOff;
		}
		set
		{
			if (useGravityJSON != null)
			{
				useGravityJSON.val = value;
			}
			else if (_useGravityOnRBWhenOff != value)
			{
				SyncUseGravityOnRBWhenOff(value);
			}
		}
	}

	public bool physicsEnabled
	{
		get
		{
			if (_followWhenOffRB != null)
			{
				return !_followWhenOffRB.isKinematic;
			}
			return false;
		}
		set
		{
			if (physicsEnabledJSON != null)
			{
				physicsEnabledJSON.val = value;
			}
			else if (_followWhenOffRB != null && _followWhenOffRB.isKinematic == value)
			{
				SyncPhysicsEnabled(value);
			}
		}
	}

	public bool globalCollisionEnabled
	{
		get
		{
			return _globalCollisionEnabled;
		}
		set
		{
			if (_globalCollisionEnabled != value)
			{
				_globalCollisionEnabled = value;
				SyncCollisionEnabled(_collisionEnabled);
			}
		}
	}

	public bool collisionEnabled
	{
		get
		{
			return _collisionEnabled;
		}
		set
		{
			if (collisionEnabledJSON != null)
			{
				collisionEnabledJSON.val = value;
			}
			else if (_collisionEnabled != value)
			{
				SyncCollisionEnabled(value);
			}
		}
	}

	public Rigidbody followWhenOffRB
	{
		get
		{
			return _followWhenOffRB;
		}
		set
		{
			if (!(_followWhenOffRB != value))
			{
				return;
			}
			_followWhenOffRB = value;
			followWhenOff = _followWhenOffRB.transform;
			ConfigurableJoint[] components = followWhenOff.GetComponents<ConfigurableJoint>();
			ConfigurableJoint[] array = components;
			foreach (ConfigurableJoint configurableJoint in array)
			{
				if (configurableJoint.connectedBody == kinematicRB)
				{
					connectedJoint = configurableJoint;
					SetJointSprings();
				}
			}
		}
	}

	public float RBMass
	{
		get
		{
			if (_followWhenOffRB != null)
			{
				return _followWhenOffRB.mass;
			}
			return 0f;
		}
		set
		{
			if (RBMassJSON != null)
			{
				RBMassJSON.val = value;
			}
			else if (_followWhenOffRB != null && _followWhenOffRB.mass != value)
			{
				SyncRBMass(value);
			}
		}
	}

	public float RBDrag
	{
		get
		{
			if (_followWhenOffRB != null)
			{
				return _followWhenOffRB.drag;
			}
			return 0f;
		}
		set
		{
			if (RBDragJSON != null)
			{
				RBDragJSON.val = value;
			}
			else if (_followWhenOffRB != null && _followWhenOffRB.drag != value)
			{
				SyncRBDrag(value);
			}
		}
	}

	public float RBAngularDrag
	{
		get
		{
			if (_followWhenOffRB != null)
			{
				return _followWhenOffRB.angularDrag;
			}
			return 0f;
		}
		set
		{
			if (RBAngularDragJSON != null)
			{
				RBAngularDragJSON.val = value;
			}
			else if (_followWhenOffRB != null && _followWhenOffRB.angularDrag != value)
			{
				SyncRBAngularDrag(value);
			}
		}
	}

	public float RBLockPositionSpring
	{
		get
		{
			return _RBLockPositionSpring;
		}
		set
		{
			if (_RBLockPositionSpring != value)
			{
				_RBLockPositionSpring = value;
				SetJointSprings();
			}
		}
	}

	public float RBLockPositionDamper
	{
		get
		{
			return _RBLockPositionDamper;
		}
		set
		{
			if (_RBLockPositionDamper != value)
			{
				_RBLockPositionDamper = value;
				SetJointSprings();
			}
		}
	}

	public float RBLockPositionMaxForce
	{
		get
		{
			return _RBLockPositionMaxForce;
		}
		set
		{
			if (_RBLockPositionMaxForce != value)
			{
				_RBLockPositionMaxForce = value;
				SetJointSprings();
			}
		}
	}

	public float RBHoldPositionSpring
	{
		get
		{
			return _RBHoldPositionSpring;
		}
		set
		{
			if (RBHoldPositionSpringJSON != null)
			{
				RBHoldPositionSpringJSON.val = value;
			}
			else if (_RBHoldPositionSpring != value)
			{
				SyncRBHoldPositionSpring(value);
			}
		}
	}

	public float RBHoldPositionDamper
	{
		get
		{
			return _RBHoldPositionDamper;
		}
		set
		{
			if (RBHoldPositionDamperJSON != null)
			{
				RBHoldPositionDamperJSON.val = value;
			}
			else if (_RBHoldPositionDamper != value)
			{
				SyncRBHoldPositionDamper(value);
			}
		}
	}

	public float RBHoldPositionMaxForce
	{
		get
		{
			return _RBHoldPositionMaxForce;
		}
		set
		{
			if (RBHoldPositionMaxForceJSON != null)
			{
				RBHoldPositionMaxForceJSON.val = value;
			}
			else if (_RBHoldPositionMaxForce != value)
			{
				SyncRBHoldPositionMaxForce(value);
				SetJointSprings();
			}
		}
	}

	public float RBComplyPositionSpring
	{
		get
		{
			return _RBComplyPositionSpring;
		}
		set
		{
			if (RBComplyPositionSpringJSON != null)
			{
				RBComplyPositionSpringJSON.val = value;
			}
			else if (_RBComplyPositionSpring != value)
			{
				SyncRBComplyPositionSpring(value);
			}
		}
	}

	public float RBComplyPositionDamper
	{
		get
		{
			return _RBComplyPositionDamper;
		}
		set
		{
			if (RBComplyPositionDamperJSON != null)
			{
				RBComplyPositionDamperJSON.val = value;
			}
			else if (_RBComplyPositionDamper != value)
			{
				SyncRBComplyPositionDamper(value);
			}
		}
	}

	public float RBLinkPositionSpring
	{
		get
		{
			return _RBLinkPositionSpring;
		}
		set
		{
			if (RBLinkPositionSpringJSON != null)
			{
				RBLinkPositionSpringJSON.val = value;
			}
			else if (_RBLinkPositionSpring != value)
			{
				SyncRBLinkPositionSpring(value);
			}
		}
	}

	public float RBLinkPositionDamper
	{
		get
		{
			return _RBLinkPositionDamper;
		}
		set
		{
			if (RBLinkPositionDamperJSON != null)
			{
				RBLinkPositionDamperJSON.val = value;
			}
			else if (_RBLinkPositionDamper != value)
			{
				SyncRBLinkPositionDamper(value);
			}
		}
	}

	public float RBLinkPositionMaxForce
	{
		get
		{
			return _RBLinkPositionMaxForce;
		}
		set
		{
			if (RBLinkPositionMaxForceJSON != null)
			{
				RBLinkPositionMaxForceJSON.val = value;
			}
			else if (_RBLinkPositionMaxForce != value)
			{
				SyncRBLinkPositionMaxForce(value);
			}
		}
	}

	public float RBLockRotationSpring
	{
		get
		{
			return _RBLockRotationSpring;
		}
		set
		{
			if (_RBLockRotationSpring != value)
			{
				_RBLockRotationSpring = value;
				SetJointSprings();
			}
		}
	}

	public float RBLockRotationDamper
	{
		get
		{
			return _RBLockRotationDamper;
		}
		set
		{
			if (_RBLockRotationDamper != value)
			{
				_RBLockRotationDamper = value;
				SetJointSprings();
			}
		}
	}

	public float RBLockRotationMaxForce
	{
		get
		{
			return _RBLockRotationMaxForce;
		}
		set
		{
			if (_RBLockRotationMaxForce != value)
			{
				_RBLockRotationMaxForce = value;
				SetJointSprings();
			}
		}
	}

	public float RBHoldRotationSpring
	{
		get
		{
			return _RBHoldRotationSpring;
		}
		set
		{
			if (RBHoldRotationSpringJSON != null)
			{
				RBHoldRotationSpringJSON.val = value;
			}
			else if (_RBHoldRotationSpring != value)
			{
				SyncRBHoldRotationSpring(value);
			}
		}
	}

	public float RBHoldRotationDamper
	{
		get
		{
			return _RBHoldRotationDamper;
		}
		set
		{
			if (RBHoldRotationDamperJSON != null)
			{
				RBHoldRotationDamperJSON.val = value;
			}
			else if (_RBHoldRotationDamper != value)
			{
				SyncRBHoldRotationSpring(value);
			}
		}
	}

	public float RBHoldRotationMaxForce
	{
		get
		{
			return _RBHoldRotationMaxForce;
		}
		set
		{
			if (RBHoldRotationMaxForceJSON != null)
			{
				RBHoldRotationMaxForceJSON.val = value;
			}
			else if (_RBHoldRotationMaxForce != value)
			{
				SyncRBHoldRotationMaxForce(value);
			}
		}
	}

	public float RBComplyRotationSpring
	{
		get
		{
			return _RBComplyRotationSpring;
		}
		set
		{
			if (RBComplyRotationSpringJSON != null)
			{
				RBComplyRotationSpringJSON.val = value;
			}
			else
			{
				SyncRBComplyRotationSpring(value);
			}
		}
	}

	public float RBComplyRotationDamper
	{
		get
		{
			return _RBComplyRotationDamper;
		}
		set
		{
			if (RBComplyRotationDamperJSON != null)
			{
				RBComplyRotationDamperJSON.val = value;
			}
			else if (_RBComplyRotationDamper != value)
			{
				SyncRBComplyRotationDamper(value);
			}
		}
	}

	public float RBLinkRotationSpring
	{
		get
		{
			return _RBLinkRotationSpring;
		}
		set
		{
			if (RBLinkRotationSpringJSON != null)
			{
				RBLinkRotationSpringJSON.val = value;
			}
			else
			{
				SyncRBLinkRotationSpring(value);
			}
		}
	}

	public float RBLinkRotationDamper
	{
		get
		{
			return _RBLinkRotationDamper;
		}
		set
		{
			if (RBLinkRotationDamperJSON != null)
			{
				RBLinkRotationDamperJSON.val = value;
			}
			else if (_RBLinkRotationDamper != value)
			{
				SyncRBLinkRotationDamper(value);
			}
		}
	}

	public float RBLinkRotationMaxForce
	{
		get
		{
			return _RBLinkRotationMaxForce;
		}
		set
		{
			if (RBLinkRotationMaxForceJSON != null)
			{
				RBLinkRotationMaxForceJSON.val = value;
			}
			else if (_RBLinkRotationMaxForce != value)
			{
				SyncRBLinkRotationMaxForce(value);
			}
		}
	}

	public float RBComplyJointRotationDriveSpring
	{
		get
		{
			return _RBComplyJointRotationDriveSpring;
		}
		set
		{
			if (RBComplyJointRotationDriveSpringJSON != null)
			{
				RBComplyJointRotationDriveSpringJSON.val = value;
			}
			else
			{
				SyncRBComplyJointRotationDriveSpring(value);
			}
		}
	}

	public float jointRotationDriveSpring
	{
		get
		{
			return _jointRotationDriveSpring;
		}
		set
		{
			if (jointRotationDriveSpringJSON != null)
			{
				jointRotationDriveSpringJSON.val = value;
			}
			else if (_jointRotationDriveSpring != value)
			{
				SyncJointRotationDriveSpring(value);
				SetNaturalJointDrive();
			}
		}
	}

	public float jointRotationDriveDamper
	{
		get
		{
			return _jointRotationDriveDamper;
		}
		set
		{
			if (jointRotationDriveDamperJSON != null)
			{
				jointRotationDriveDamperJSON.val = value;
			}
			else if (_jointRotationDriveDamper != value)
			{
				SyncJointRotationDriveDamper(value);
			}
		}
	}

	public float jointRotationDriveMaxForce
	{
		get
		{
			return _jointRotationDriveMaxForce;
		}
		set
		{
			if (jointRotationDriveMaxForceJSON != null)
			{
				jointRotationDriveMaxForceJSON.val = value;
			}
			else if (_jointRotationDriveMaxForce != value)
			{
				SyncJointRotationDriveMaxForce(value);
			}
		}
	}

	public float jointRotationDriveXTarget
	{
		get
		{
			return _jointRotationDriveXTarget;
		}
		set
		{
			if (jointRotationDriveXTargetJSON != null)
			{
				jointRotationDriveXTargetJSON.val = value;
			}
			else if (_jointRotationDriveXTarget != value)
			{
				SyncJointRotationDriveXTarget(value);
			}
		}
	}

	public float jointRotationDriveXTargetAdditional
	{
		get
		{
			return _jointRotationDriveXTargetAdditional;
		}
		set
		{
			if (_jointRotationDriveXTargetAdditional != value)
			{
				_jointRotationDriveXTargetAdditional = value;
				SetNaturalJointDriveTarget();
			}
		}
	}

	public float jointRotationDriveYTarget
	{
		get
		{
			return _jointRotationDriveYTarget;
		}
		set
		{
			if (jointRotationDriveYTargetJSON != null)
			{
				jointRotationDriveYTargetJSON.val = value;
			}
			else if (_jointRotationDriveYTarget != value)
			{
				SyncJointRotationDriveYTarget(value);
			}
		}
	}

	public float jointRotationDriveYTargetAdditional
	{
		get
		{
			return _jointRotationDriveYTargetAdditional;
		}
		set
		{
			if (_jointRotationDriveYTargetAdditional != value)
			{
				_jointRotationDriveYTargetAdditional = value;
				SetNaturalJointDriveTarget();
			}
		}
	}

	public float jointRotationDriveZTarget
	{
		get
		{
			return _jointRotationDriveZTarget;
		}
		set
		{
			if (jointRotationDriveZTargetJSON != null)
			{
				jointRotationDriveZTargetJSON.val = value;
			}
			else if (_jointRotationDriveZTarget != value)
			{
				SyncJointRotationDriveZTarget(value);
				SetNaturalJointDriveTarget();
			}
		}
	}

	public float jointRotationDriveZTargetAdditional
	{
		get
		{
			return _jointRotationDriveZTargetAdditional;
		}
		set
		{
			if (_jointRotationDriveZTargetAdditional != value)
			{
				_jointRotationDriveZTargetAdditional = value;
				SetNaturalJointDriveTarget();
			}
		}
	}

	public bool hidden
	{
		get
		{
			return _hidden;
		}
		set
		{
			_hidden = value;
			if (_hidden)
			{
				if (mrs != null)
				{
					MeshRenderer[] array = mrs;
					foreach (MeshRenderer meshRenderer in array)
					{
						meshRenderer.enabled = false;
					}
				}
			}
			else if (mrs != null)
			{
				MeshRenderer[] array2 = mrs;
				foreach (MeshRenderer meshRenderer2 in array2)
				{
					meshRenderer2.enabled = true;
				}
			}
		}
	}

	public bool guihidden
	{
		get
		{
			return _guihidden;
		}
		set
		{
			_guihidden = value;
			if (_guihidden)
			{
				if (!GUIalwaysVisibleWhenSelected || !_selected)
				{
					HideGUI();
				}
			}
			else if (_selected)
			{
				ShowGUI();
			}
		}
	}

	public bool highlighted
	{
		get
		{
			return _highlighted;
		}
		set
		{
			_highlighted = value;
			SetColor();
		}
	}

	public Vector3 selectedPosition => _selectedPosition;

	public bool selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected = value;
			if (_selected)
			{
				if (!_guihidden || GUIalwaysVisibleWhenSelected)
				{
					ShowGUI();
				}
				_selectedPosition = control.position;
			}
			else
			{
				HideGUI();
			}
			SetColor();
		}
	}

	public Color currentPositionColor => _currentPositionColor;

	public Color currentRotationColor => _currentRotationColor;

	public ControlMode controlMode
	{
		get
		{
			return _controlMode;
		}
		set
		{
			switch (value)
			{
				case ControlMode.Position:
					if (_moveEnabled || _moveForceEnabled)
					{
						_controlMode = value;
					}
					break;
				case ControlMode.Rotation:
					if (_rotationEnabled || _rotationForceEnabled)
					{
						_controlMode = value;
					}
					break;
				default:
					_controlMode = ControlMode.Off;
					break;
			}
		}
	}

	protected void RegisterAllocatedObject(UnityEngine.Object o)
	{
		if (Application.isPlaying)
		{
			if (allocatedObjects == null)
			{
				allocatedObjects = new List<UnityEngine.Object>();
			}
			allocatedObjects.Add(o);
		}
	}

	protected void DestroyAllocatedObjects()
	{
		if (!Application.isPlaying || allocatedObjects == null)
		{
			return;
		}
		foreach (UnityEngine.Object allocatedObject in allocatedObjects)
		{
			UnityEngine.Object.Destroy(allocatedObject);
		}
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = true)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical || forceStore)
		{
			needsStore = true;
			if (storePositionRotationAsLocal)
			{
				Vector3 localPosition = base.transform.localPosition;
				jSON["localPosition"]["x"].AsFloat = localPosition.x;
				jSON["localPosition"]["y"].AsFloat = localPosition.y;
				jSON["localPosition"]["z"].AsFloat = localPosition.z;
				Vector3 localEulerAngles = base.transform.localEulerAngles;
				jSON["localRotation"]["x"].AsFloat = localEulerAngles.x;
				jSON["localRotation"]["y"].AsFloat = localEulerAngles.y;
				jSON["localRotation"]["z"].AsFloat = localEulerAngles.z;
			}
			else
			{
				Vector3 position = base.transform.position;
				jSON["position"]["x"].AsFloat = position.x;
				jSON["position"]["y"].AsFloat = position.y;
				jSON["position"]["z"].AsFloat = position.z;
				Vector3 eulerAngles = base.transform.eulerAngles;
				jSON["rotation"]["x"].AsFloat = eulerAngles.x;
				jSON["rotation"]["y"].AsFloat = eulerAngles.y;
				jSON["rotation"]["z"].AsFloat = eulerAngles.z;
			}
			if (_linkToRB != null && linkToAtomUID != null && linkToAtomUID != "[CameraRig]")
			{
				jSON["linkTo"] = linkToAtomUID + ":" + _linkToRB.name;
			}
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		if (restorePhysical)
		{
			SelectLinkToRigidbody(null);
		}
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!restorePhysical)
		{
			return;
		}
		PauseComply();
		if (jc["position"] != null)
		{
			Vector3 position = base.transform.position;
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
			base.transform.position = position;
			if (control != null)
			{
				control.position = position;
			}
			if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>())
			{
				followWhenOff.position = position;
			}
		}
		else if (jc["localPosition"] != null)
		{
			Vector3 localPosition = base.transform.localPosition;
			if (jc["localPosition"]["x"] != null)
			{
				localPosition.x = jc["localPosition"]["x"].AsFloat;
			}
			if (jc["localPosition"]["y"] != null)
			{
				localPosition.y = jc["localPosition"]["y"].AsFloat;
			}
			if (jc["localPosition"]["z"] != null)
			{
				localPosition.z = jc["localPosition"]["z"].AsFloat;
			}
			base.transform.localPosition = localPosition;
			if (control != null)
			{
				control.position = base.transform.position;
			}
			if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>())
			{
				followWhenOff.position = base.transform.position;
			}
		}
		else if (setMissingToDefault)
		{
			if (storePositionRotationAsLocal)
			{
				base.transform.localPosition = startingLocalPosition;
			}
			else
			{
				base.transform.position = startingPosition;
			}
			if (control != null)
			{
				control.position = base.transform.position;
			}
			if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>())
			{
				followWhenOff.position = base.transform.position;
			}
		}
		if (jc["rotation"] != null)
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
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
			base.transform.eulerAngles = eulerAngles;
			if (control != null)
			{
				control.rotation = base.transform.rotation;
			}
			if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>())
			{
				followWhenOff.rotation = base.transform.rotation;
			}
		}
		else if (jc["localRotation"] != null)
		{
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			if (jc["localRotation"]["x"] != null)
			{
				localEulerAngles.x = jc["localRotation"]["x"].AsFloat;
			}
			if (jc["localRotation"]["y"] != null)
			{
				localEulerAngles.y = jc["localRotation"]["y"].AsFloat;
			}
			if (jc["localRotation"]["z"] != null)
			{
				localEulerAngles.z = jc["localRotation"]["z"].AsFloat;
			}
			base.transform.localEulerAngles = localEulerAngles;
			if (control != null)
			{
				control.rotation = base.transform.rotation;
			}
			if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>())
			{
				followWhenOff.rotation = base.transform.rotation;
			}
		}
		else if (setMissingToDefault)
		{
			if (storePositionRotationAsLocal)
			{
				base.transform.localRotation = startingLocalRotation;
			}
			else
			{
				base.transform.rotation = startingRotation;
			}
			if (control != null)
			{
				control.rotation = base.transform.rotation;
			}
			if (followWhenOff != null && !followWhenOff.GetComponent<JSONStorable>())
			{
				followWhenOff.rotation = base.transform.rotation;
			}
		}
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (!restorePhysical)
		{
			return;
		}
		if (jc["linkTo"] != null)
		{
			if (SuperController.singleton != null)
			{
				Rigidbody rb = SuperController.singleton.RigidbodyNameToRigidbody(jc["linkTo"]);
				SelectLinkToRigidbody(rb, SelectLinkState.PositionAndRotation, usePhysicalLink: false, modifyState: false);
			}
		}
		else if (setMissingToDefault)
		{
			SelectLinkToRigidbody(null);
		}
		jointRotationDriveXTargetJSON.RestoreFromJSON(jc);
	}

	public void SelectRoot()
	{
		if (enableSelectRoot && containingAtom != null && containingAtom.mainController != null)
		{
			SuperController.singleton.SelectController(containingAtom.mainController);
		}
	}

	public void MovePlayerTo()
	{
	}

	public void MovePlayerToAndControl()
	{
	}

	protected virtual void SetLinkToAtomNames()
	{
		if (!(linkToAtomSelectionPopup != null) || !(SuperController.singleton != null))
		{
			return;
		}
		List<string> atomUIDsWithRigidbodies = SuperController.singleton.GetAtomUIDsWithRigidbodies();
		if (atomUIDsWithRigidbodies == null)
		{
			linkToAtomSelectionPopup.numPopupValues = 1;
			linkToAtomSelectionPopup.setPopupValue(0, "None");
			return;
		}
		linkToAtomSelectionPopup.numPopupValues = atomUIDsWithRigidbodies.Count + 1;
		linkToAtomSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < atomUIDsWithRigidbodies.Count; i++)
		{
			linkToAtomSelectionPopup.setPopupValue(i + 1, atomUIDsWithRigidbodies[i]);
		}
	}

	protected void OnAtomUIDRename(string fromid, string toid)
	{
		SyncAtomUID();
		if (linkToAtomUID == fromid)
		{
			linkToAtomUID = toid;
			if (linkToAtomSelectionPopup != null)
			{
				linkToAtomSelectionPopup.currentValueNoCallback = toid;
			}
		}
	}

	protected void onLinkToRigidbodyNamesChanged(List<string> rbNames)
	{
		if (!(linkToSelectionPopup != null))
		{
			return;
		}
		if (rbNames == null)
		{
			linkToSelectionPopup.numPopupValues = 1;
			linkToSelectionPopup.setPopupValue(0, "None");
			return;
		}
		linkToSelectionPopup.numPopupValues = rbNames.Count + 1;
		linkToSelectionPopup.setPopupValue(0, "None");
		for (int i = 0; i < rbNames.Count; i++)
		{
			linkToSelectionPopup.setPopupValue(i + 1, rbNames[i]);
		}
	}

	public virtual void SetLinkToAtom(string atomUID)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		Atom atomByUid = SuperController.singleton.GetAtomByUid(atomUID);
		if (atomByUid != null)
		{
			linkToAtomUID = atomUID;
			List<string> rigidbodyNamesInAtom = SuperController.singleton.GetRigidbodyNamesInAtom(linkToAtomUID);
			onLinkToRigidbodyNamesChanged(rigidbodyNamesInAtom);
			if (linkToSelectionPopup != null)
			{
				linkToSelectionPopup.currentValue = "None";
			}
		}
	}

	public void SetLinkToRigidbody(string rigidbodyName)
	{
		if (SuperController.singleton != null)
		{
			Rigidbody rigidbody = SuperController.singleton.RigidbodyNameToRigidbody(rigidbodyName);
			linkToRB = rigidbody;
		}
	}

	public virtual void SetLinkToRigidbodyObject(string objectName)
	{
		if (linkToAtomUID != null)
		{
			SetLinkToRigidbody(linkToAtomUID + ":" + objectName);
		}
	}

	private void GetLinkToAtomUIDFromLinkToRB(Rigidbody rb)
	{
		linkToAtomUID = "None";
		Atom atom = null;
		FreeControllerV3 component = rb.GetComponent<FreeControllerV3>();
		if (component != null)
		{
			atom = component.containingAtom;
		}
		else
		{
			ForceReceiver component2 = rb.GetComponent<ForceReceiver>();
			if (component2 != null)
			{
				atom = component2.containingAtom;
			}
		}
		if (atom != null)
		{
			linkToAtomUID = atom.uid;
		}
	}

	public void SelectLinkToRigidbody(Rigidbody rb)
	{
		SelectLinkToRigidbody(rb, SelectLinkState.PositionAndRotation);
	}

	public void SelectLinkToRigidbody(Rigidbody rb, SelectLinkState linkState, bool usePhysicalLink = false, bool modifyState = true)
	{
		if (rb != null)
		{
			preLinkRB = linkToRB;
			preLinkPositionState = currentPositionState;
			preLinkRotationState = currentRotationState;
			GetLinkToAtomUIDFromLinkToRB(rb);
			if (linkToAtomSelectionPopup != null)
			{
				linkToAtomSelectionPopup.currentValue = linkToAtomUID;
			}
		}
		else if (linkToAtomSelectionPopup != null)
		{
			linkToAtomSelectionPopup.currentValue = "None";
		}
		if (linkToSelectionPopup != null)
		{
			if (rb != null)
			{
				linkToSelectionPopup.currentValueNoCallback = rb.name;
			}
			else
			{
				linkToSelectionPopup.currentValueNoCallback = "None";
			}
		}
		if (rb != null)
		{
			linkToRB = rb;
			if (!modifyState)
			{
				return;
			}
			if (linkState == SelectLinkState.Position || linkState == SelectLinkState.PositionAndRotation)
			{
				if (usePhysicalLink && _currentPositionState != PositionState.PhysicsLink)
				{
					currentPositionState = PositionState.PhysicsLink;
				}
				else if (_currentPositionState != PositionState.ParentLink && _currentPositionState != PositionState.PhysicsLink)
				{
					currentPositionState = PositionState.ParentLink;
				}
			}
			if (linkState == SelectLinkState.Rotation || linkState == SelectLinkState.PositionAndRotation)
			{
				if (usePhysicalLink && _currentRotationState != RotationState.PhysicsLink)
				{
					currentRotationState = RotationState.PhysicsLink;
				}
				else if (_currentRotationState != RotationState.ParentLink && _currentRotationState != RotationState.PhysicsLink)
				{
					currentRotationState = RotationState.ParentLink;
				}
			}
			return;
		}
		linkToRB = null;
		if (modifyState)
		{
			if (_currentPositionState == PositionState.ParentLink || _currentPositionState == PositionState.PhysicsLink)
			{
				currentPositionState = preLinkPositionState;
			}
			if (_currentRotationState == RotationState.ParentLink || _currentRotationState == RotationState.PhysicsLink)
			{
				currentRotationState = preLinkRotationState;
			}
		}
	}

	public void RestorePreLinkState()
	{
		if (preLinkRB != null)
		{
			GetLinkToAtomUIDFromLinkToRB(preLinkRB);
			if (linkToAtomUID == "None")
			{
				if (linkToAtomSelectionPopup != null)
				{
					linkToAtomSelectionPopup.currentValue = "None";
				}
				if (linkToSelectionPopup != null)
				{
					linkToSelectionPopup.currentValueNoCallback = "None";
				}
			}
			else
			{
				if (linkToAtomSelectionPopup != null)
				{
					linkToAtomSelectionPopup.currentValue = linkToAtomUID;
				}
				if (linkToSelectionPopup != null)
				{
					linkToSelectionPopup.currentValueNoCallback = preLinkRB.name;
				}
			}
		}
		else
		{
			if (linkToAtomSelectionPopup != null)
			{
				linkToAtomSelectionPopup.currentValue = "None";
			}
			if (linkToSelectionPopup != null)
			{
				linkToSelectionPopup.currentValueNoCallback = "None";
			}
		}
		if (preLinkRB != null && linkToAtomUID != "None")
		{
			linkToRB = preLinkRB;
			if (currentPositionState != preLinkPositionState)
			{
				currentPositionState = preLinkPositionState;
			}
			if (currentRotationState != preLinkRotationState)
			{
				currentRotationState = preLinkRotationState;
			}
		}
		else
		{
			linkToRB = null;
			if (_currentPositionState == PositionState.ParentLink || _currentPositionState == PositionState.PhysicsLink)
			{
				currentPositionState = preLinkPositionState;
			}
			if (_currentRotationState == RotationState.ParentLink || _currentRotationState == RotationState.PhysicsLink)
			{
				currentRotationState = preLinkRotationState;
			}
		}
	}

	public void SelectLinkToRigidbodyFromScene()
	{
		SetLinkToAtomNames();
		SuperController.singleton.SelectModeRigidbody(SelectLinkToRigidbody);
	}

	public void SelectAlignToRigidbody(Rigidbody rb)
	{
		control.position = rb.transform.position;
		control.rotation = rb.transform.rotation;
	}

	public void SelectAlignToRigidbodyFromScene()
	{
		SuperController.singleton.SelectModeRigidbody(SelectAlignToRigidbody);
	}

	public void SetPositionStateFromString(string state)
	{
		try
		{
			PositionState positionState = (PositionState)Enum.Parse(typeof(PositionState), state);
			_currentPositionState = positionState;
			SyncPositionState();
		}
		catch (ArgumentException)
		{
			Debug.LogError("State " + state + " is not a valid position state");
		}
	}

	private void SyncPositionState()
	{
		switch (_currentPositionState)
		{
			case PositionState.On:
			case PositionState.Following:
			case PositionState.Hold:
			case PositionState.Lock:
			case PositionState.ParentLink:
			case PositionState.PhysicsLink:
			case PositionState.Comply:
				if (_followWhenOffRB != null)
				{
					_followWhenOffRB.useGravity = _useGravityOnRBWhenOff;
				}
				break;
			case PositionState.Off:
				if (_followWhenOffRB != null)
				{
					_followWhenOffRB.useGravity = _useGravityOnRBWhenOff;
				}
				break;
		}
		switch (_currentPositionState)
		{
			case PositionState.On:
			case PositionState.Comply:
				_moveEnabled = true;
				_moveForceEnabled = false;
				break;
			case PositionState.Off:
			case PositionState.Following:
			case PositionState.Hold:
				_moveEnabled = useForceWhenOff;
				_moveForceEnabled = useForceWhenOff;
				break;
			case PositionState.ParentLink:
			case PositionState.PhysicsLink:
				_moveEnabled = useForceWhenOff;
				_moveForceEnabled = useForceWhenOff;
				if (_linkToConnector != null)
				{
					_linkToConnector.position = base.transform.position;
				}
				if (_linkToJoint != null)
				{
					_linkToJoint.connectedBody = null;
					_linkToJoint.connectedBody = _linkToRB;
				}
				break;
			case PositionState.Lock:
				_moveEnabled = false;
				_moveForceEnabled = false;
				break;
		}
		SetLinkedJointSprings();
		SetJointSprings();
		StateChanged();
	}

	public void SetRotationStateFromString(string state)
	{
		try
		{
			RotationState rotationState = (RotationState)Enum.Parse(typeof(RotationState), state);
			_currentRotationState = rotationState;
			SyncRotationState();
		}
		catch (ArgumentException)
		{
			Debug.LogError("State " + state + " is not a valid rotation state");
		}
	}

	private void SyncRotationState()
	{
		switch (_currentRotationState)
		{
			case RotationState.On:
			case RotationState.Comply:
				_rotationEnabled = true;
				_rotationForceEnabled = false;
				break;
			case RotationState.Off:
			case RotationState.Following:
			case RotationState.Hold:
			case RotationState.LookAt:
				_rotationEnabled = useForceWhenOff;
				_rotationForceEnabled = useForceWhenOff;
				break;
			case RotationState.ParentLink:
			case RotationState.PhysicsLink:
				_rotationEnabled = useForceWhenOff;
				_rotationForceEnabled = useForceWhenOff;
				if (_linkToConnector != null)
				{
					_linkToConnector.rotation = base.transform.rotation;
				}
				if (_linkToJoint != null)
				{
					_linkToJoint.connectedBody = null;
					_linkToJoint.connectedBody = _linkToRB;
				}
				break;
			case RotationState.Lock:
				_rotationEnabled = false;
				_rotationForceEnabled = false;
				break;
		}
		SetLinkedJointSprings();
		SetJointSprings();
		SetNaturalJointDrive();
		StateChanged();
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		PauseComply(10);
		scalePow = Mathf.Pow(1.7f, scale - 1f);
		SetJointSprings();
		SetNaturalJointDrive();
	}

	private void SetLinkedJointSprings()
	{
		if (_linkToJoint != null)
		{
			JointDrive xDrive = _linkToJoint.xDrive;
			if (_currentPositionState == PositionState.PhysicsLink)
			{
				xDrive.positionSpring = _RBLinkPositionSpring;
				xDrive.positionDamper = _RBLinkPositionDamper;
				xDrive.maximumForce = _RBLinkPositionMaxForce;
			}
			else
			{
				xDrive.positionSpring = 0f;
				xDrive.positionDamper = 0f;
				xDrive.maximumForce = 0f;
			}
			_linkToJoint.xDrive = xDrive;
			_linkToJoint.yDrive = xDrive;
			_linkToJoint.zDrive = xDrive;
			xDrive = _linkToJoint.slerpDrive;
			if (_currentRotationState == RotationState.PhysicsLink)
			{
				xDrive.positionSpring = _RBLinkRotationSpring;
				xDrive.positionDamper = _RBLinkRotationDamper;
				xDrive.maximumForce = _RBLinkRotationMaxForce;
			}
			else
			{
				xDrive.positionSpring = 0f;
				xDrive.positionDamper = 0f;
				xDrive.maximumForce = 0f;
			}
			_linkToJoint.slerpDrive = xDrive;
			_linkToJoint.angularXDrive = xDrive;
			_linkToJoint.angularYZDrive = xDrive;
		}
	}

	private void SetJointSprings()
	{
		if (connectedJoint != null)
		{
			float num = scalePow;
			float num2 = scalePow;
			float num3 = scalePow;
			JointDrive xDrive = connectedJoint.xDrive;
			switch (_currentPositionState)
			{
				case PositionState.On:
				case PositionState.Following:
				case PositionState.Hold:
				case PositionState.ParentLink:
				case PositionState.PhysicsLink:
					xDrive.positionSpring = _RBHoldPositionSpring;
					xDrive.positionDamper = _RBHoldPositionDamper;
					xDrive.maximumForce = _RBHoldPositionMaxForce;
					break;
				case PositionState.Comply:
					xDrive.positionSpring = _RBComplyPositionSpring * RBMass;
					xDrive.positionDamper = _RBComplyPositionDamper * RBMass;
					xDrive.maximumForce = _RBComplyPositionMaxForce;
					break;
				case PositionState.Lock:
					xDrive.positionSpring = _RBLockPositionSpring;
					xDrive.positionDamper = _RBLockPositionDamper;
					xDrive.maximumForce = _RBLockPositionMaxForce;
					break;
				case PositionState.Off:
					xDrive.positionSpring = 0f;
					xDrive.positionDamper = 0f;
					xDrive.maximumForce = 0f;
					break;
			}
			connectedJoint.xDrive = xDrive;
			connectedJoint.yDrive = xDrive;
			connectedJoint.zDrive = xDrive;
			xDrive = connectedJoint.slerpDrive;
			switch (_currentRotationState)
			{
				case RotationState.On:
				case RotationState.Following:
				case RotationState.Hold:
				case RotationState.LookAt:
				case RotationState.ParentLink:
				case RotationState.PhysicsLink:
					xDrive.positionSpring = _RBHoldRotationSpring * num;
					xDrive.positionDamper = _RBHoldRotationDamper * num2;
					xDrive.maximumForce = _RBHoldRotationMaxForce * num3;
					break;
				case RotationState.Comply:
					xDrive.positionSpring = _RBComplyRotationSpring * num * RBMass;
					xDrive.positionDamper = _RBComplyRotationDamper * num2 * RBMass;
					xDrive.maximumForce = _RBComplyRotationMaxForce * num3;
					break;
				case RotationState.Lock:
					xDrive.positionSpring = _RBLockRotationSpring * num;
					xDrive.positionDamper = _RBLockRotationDamper * num2;
					xDrive.maximumForce = _RBLockRotationMaxForce * num3;
					break;
				case RotationState.Off:
					xDrive.positionSpring = 0f;
					xDrive.positionDamper = 0f;
					xDrive.maximumForce = 0f;
					break;
			}
			connectedJoint.slerpDrive = xDrive;
			connectedJoint.angularXDrive = xDrive;
			connectedJoint.angularYZDrive = xDrive;
			_followWhenOffRB.WakeUp();
		}
	}

	private void SetNaturalJointDrive()
	{
		if (naturalJoint != null)
		{
			float num = scalePow;
			float num2 = scalePow;
			float num3 = scalePow;
			JointDrive slerpDrive = naturalJoint.slerpDrive;
			if (_currentRotationState == RotationState.Comply)
			{
				slerpDrive.positionSpring = _RBComplyJointRotationDriveSpring * num;
			}
			else
			{
				slerpDrive.positionSpring = _jointRotationDriveSpring * num;
			}
			slerpDrive.positionDamper = _jointRotationDriveDamper * num2;
			slerpDrive.maximumForce = _jointRotationDriveMaxForce * num3;
			naturalJoint.slerpDrive = slerpDrive;
		}
	}

	private void SetNaturalJointDriveTarget()
	{
		if (naturalJoint != null)
		{
			Quaternion quaternion = Quaternion.Euler(_jointRotationDriveXTarget + _jointRotationDriveXTargetAdditional, 0f, 0f);
			Quaternion quaternion2 = Quaternion.Euler(0f, _jointRotationDriveYTarget + _jointRotationDriveYTargetAdditional, 0f);
			Quaternion quaternion3 = Quaternion.Euler(0f, 0f, _jointRotationDriveZTarget + _jointRotationDriveZTargetAdditional);
			Quaternion targetRotation = quaternion;
			switch (naturalJointDriveRotationOrder)
			{
				case Quaternion2Angles.RotationOrder.XYZ:
					targetRotation = quaternion * quaternion2 * quaternion3;
					break;
				case Quaternion2Angles.RotationOrder.XZY:
					targetRotation = quaternion * quaternion3 * quaternion2;
					break;
				case Quaternion2Angles.RotationOrder.YXZ:
					targetRotation = quaternion2 * quaternion * quaternion3;
					break;
				case Quaternion2Angles.RotationOrder.YZX:
					targetRotation = quaternion2 * quaternion3 * quaternion3;
					break;
				case Quaternion2Angles.RotationOrder.ZXY:
					targetRotation = quaternion3 * quaternion * quaternion2;
					break;
				case Quaternion2Angles.RotationOrder.ZYX:
					targetRotation = quaternion3 * quaternion2 * quaternion;
					break;
			}
			naturalJoint.targetRotation = targetRotation;
		}
	}

	private void SyncXLock(bool b)
	{
		_xLock = b;
	}

	private void SyncYLock(bool b)
	{
		_yLock = b;
	}

	private void SyncZLock(bool b)
	{
		_zLock = b;
	}

	private void SyncXLocalLock(bool b)
	{
		_xLocalLock = b;
	}

	private void SyncYLocalLock(bool b)
	{
		_yLocalLock = b;
	}

	private void SyncZLocalLock(bool b)
	{
		_zLocalLock = b;
	}

	private void SyncXRotLock(bool b)
	{
		_xRotLock = b;
	}

	private void SyncYRotLock(bool b)
	{
		_yRotLock = b;
	}

	private void SyncZRotLock(bool b)
	{
		_zRotLock = b;
	}

	public void XPositionSnapPoint1()
	{
		Vector3 position = control.position;
		position.x *= 10f;
		position.x = Mathf.Round(position.x);
		position.x /= 10f;
		control.position = position;
	}

	public void YPositionSnapPoint1()
	{
		Vector3 position = control.position;
		position.y *= 10f;
		position.y = Mathf.Round(position.y);
		position.y /= 10f;
		control.position = position;
	}

	public void ZPositionSnapPoint1()
	{
		Vector3 position = control.position;
		position.z *= 10f;
		position.z = Mathf.Round(position.z);
		position.z /= 10f;
		control.position = position;
	}

	public void XRotationSnap1()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.x = Mathf.Round(eulerAngles.x);
		control.eulerAngles = eulerAngles;
	}

	public void YRotationSnap1()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.y = Mathf.Round(eulerAngles.y);
		control.eulerAngles = eulerAngles;
	}

	public void ZRotationSnap1()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.z = Mathf.Round(eulerAngles.z);
		control.eulerAngles = eulerAngles;
	}

	public void XRotation0()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.x = 0f;
		control.eulerAngles = eulerAngles;
	}

	public void YRotation0()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.y = 0f;
		control.eulerAngles = eulerAngles;
	}

	public void ZRotation0()
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.z = 0f;
		control.eulerAngles = eulerAngles;
	}

	public void XRotationAdd(float a)
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.x += a;
		control.eulerAngles = eulerAngles;
	}

	public void YRotationAdd(float a)
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.y += a;
		control.eulerAngles = eulerAngles;
	}

	public void ZRotationAdd(float a)
	{
		Vector3 eulerAngles = control.eulerAngles;
		eulerAngles.z += a;
		control.eulerAngles = eulerAngles;
	}

	protected void SyncOn(bool b)
	{
		_on = b;
		if (controlsOn && followWhenOff != null)
		{
			followWhenOff.gameObject.SetActive(_on);
		}
	}

	protected void SyncInteractableInPlayMode(bool b)
	{
		_interactableInPlayMode = b;
	}

	protected void SyncPossessable(bool b)
	{
		_possessable = b;
	}

	protected void SyncCanGrabPosition(bool b)
	{
		_canGrabPosition = b;
	}

	protected void SyncCanGrabRotation(bool b)
	{
		_canGrabRotation = b;
	}

	public void SetPositionGridModeFromString(string gridModeString)
	{
		try
		{
			GridMode gridMode = (GridMode)Enum.Parse(typeof(GridMode), gridModeString);
			_positionGridMode = gridMode;
		}
		catch (ArgumentException)
		{
			Debug.LogError("Grid Mode " + gridModeString + " is not a valid grid mode");
		}
	}

	private void SyncPositionGrid(float f)
	{
		float num = f;
		num *= 1000f;
		num = Mathf.Round(num);
		num /= 1000f;
		if (positionGridJSON != null && num != f)
		{
			positionGridJSON.valNoCallback = num;
		}
		_positionGrid = num;
	}

	public void SetRotationGridModeFromString(string gridModeString)
	{
		try
		{
			GridMode gridMode = (GridMode)Enum.Parse(typeof(GridMode), gridModeString);
			_rotationGridMode = gridMode;
		}
		catch (ArgumentException)
		{
			Debug.LogError("Grid Mode " + gridModeString + " is not a valid grid mode");
		}
	}

	private void SyncRotationGrid(float f)
	{
		float num = f;
		num *= 100f;
		num = Mathf.Round(num);
		num /= 100f;
		if (rotationGridJSON != null && num != f)
		{
			rotationGridJSON.valNoCallback = num;
		}
		_rotationGrid = num;
	}

	private void SyncUseGravityOnRBWhenOff(bool b)
	{
		_useGravityOnRBWhenOff = b;
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.useGravity = _useGravityOnRBWhenOff;
		}
	}

	private void SyncPhysicsEnabled(bool b)
	{
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.isKinematic = !b;
			MeshCollider component = _followWhenOffRB.GetComponent<MeshCollider>();
			if (component != null)
			{
				component.convex = b;
			}
		}
	}

	private void SyncCollisionEnabled(bool b)
	{
		_collisionEnabled = b;
		if (controlsCollisionEnabled && _followWhenOffRB != null)
		{
			_followWhenOffRB.detectCollisions = _collisionEnabled && _globalCollisionEnabled;
		}
	}

	private void SyncRBMass(float f)
	{
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.mass = f;
			_followWhenOffRB.WakeUp();
			SetJointSprings();
		}
		if (rigidbodySlavesForMass != null)
		{
			Rigidbody[] array = rigidbodySlavesForMass;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.mass = f;
			}
		}
	}

	private void SyncRBDrag(float f)
	{
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.drag = f;
			_followWhenOffRB.WakeUp();
		}
	}

	private void SyncRBAngularDrag(float f)
	{
		if (_followWhenOffRB != null)
		{
			_followWhenOffRB.angularDrag = f;
			_followWhenOffRB.WakeUp();
		}
	}

	private void SyncRBHoldPositionSpring(float f)
	{
		_RBHoldPositionSpring = f;
		SetJointSprings();
	}

	public void SetHoldPositionSpringMin()
	{
		if (RBHoldPositionSpringJSON != null)
		{
			RBHoldPositionSpringJSON.val = RBHoldPositionSpringJSON.min;
		}
	}

	public void SetHoldPositionSpringMax()
	{
		if (RBHoldPositionSpringJSON != null)
		{
			RBHoldPositionSpringJSON.val = RBHoldPositionSpringJSON.max;
		}
	}

	public void SetHoldPositionSpringPercent(float percent)
	{
		if (RBHoldPositionSpringJSON != null)
		{
			RBHoldPositionSpringJSON.val = (RBHoldPositionSpringJSON.max - RBHoldPositionSpringJSON.min) * percent + RBHoldPositionSpringJSON.min;
		}
	}

	private void SyncRBHoldPositionDamper(float f)
	{
		_RBHoldPositionDamper = f;
		SetJointSprings();
	}

	public void SetHoldPositionDamperMin()
	{
		if (RBHoldPositionDamperJSON != null)
		{
			RBHoldPositionDamperJSON.val = RBHoldPositionDamperJSON.min;
		}
	}

	public void SetHoldPositionDamperMax()
	{
		if (RBHoldPositionDamperJSON != null)
		{
			RBHoldPositionDamperJSON.val = RBHoldPositionDamperJSON.max;
		}
	}

	public void SetHoldPositionDamperPercent(float percent)
	{
		if (RBHoldPositionDamperJSON != null)
		{
			RBHoldPositionDamperJSON.val = (RBHoldPositionDamperJSON.max - RBHoldPositionDamperJSON.min) * percent + RBHoldPositionDamperJSON.min;
		}
	}

	private void SyncRBHoldPositionMaxForce(float f)
	{
		_RBHoldPositionMaxForce = f;
		SetJointSprings();
	}

	private void SyncRBComplyPositionSpring(float f)
	{
		_RBComplyPositionSpring = f;
		SetJointSprings();
	}

	private void SyncRBComplyPositionDamper(float f)
	{
		_RBComplyPositionDamper = f;
		SetJointSprings();
	}

	private void SyncRBLinkPositionSpring(float f)
	{
		_RBLinkPositionSpring = f;
		SetLinkedJointSprings();
	}

	private void SyncRBLinkPositionDamper(float f)
	{
		_RBLinkPositionDamper = f;
		SetLinkedJointSprings();
	}

	private void SyncRBLinkPositionMaxForce(float f)
	{
		_RBLinkPositionMaxForce = f;
		SetLinkedJointSprings();
	}

	private void SyncRBHoldRotationSpring(float f)
	{
		_RBHoldRotationSpring = f;
		SetJointSprings();
	}

	public void SetHoldRotationSpringMin()
	{
		if (RBHoldRotationSpringJSON != null)
		{
			RBHoldRotationSpringJSON.val = RBHoldRotationSpringJSON.min;
		}
	}

	public void SetHoldRotationSpringMax()
	{
		if (RBHoldRotationSpringJSON != null)
		{
			RBHoldRotationSpringJSON.val = RBHoldRotationSpringJSON.max;
		}
	}

	public void SetHoldRotationSpringPercent(float percent)
	{
		if (RBHoldRotationSpringJSON != null)
		{
			RBHoldRotationSpringJSON.val = (RBHoldRotationSpringJSON.max - RBHoldRotationSpringJSON.min) * percent + RBHoldRotationSpringJSON.min;
		}
	}

	private void SyncRBHoldRotationDamper(float f)
	{
		_RBHoldRotationDamper = f;
		SetJointSprings();
	}

	public void SetHoldRotationDamperMin()
	{
		if (RBHoldRotationDamperJSON != null)
		{
			RBHoldRotationDamperJSON.val = RBHoldRotationDamperJSON.min;
		}
	}

	public void SetHoldRotationDamperMax()
	{
		if (RBHoldRotationDamperJSON != null)
		{
			RBHoldRotationDamperJSON.val = RBHoldRotationDamperJSON.max;
		}
	}

	public void SetHoldRotationDamperPercent(float percent)
	{
		if (RBHoldRotationDamperJSON != null)
		{
			RBHoldRotationDamperJSON.val = (RBHoldRotationDamperJSON.max - RBHoldRotationDamperJSON.min) * percent + RBHoldRotationDamperJSON.min;
		}
	}

	private void SyncRBHoldRotationMaxForce(float f)
	{
		_RBHoldRotationMaxForce = f;
		SetJointSprings();
	}

	private void SyncRBComplyRotationSpring(float f)
	{
		_RBComplyRotationSpring = f;
		SetJointSprings();
	}

	private void SyncRBComplyRotationDamper(float f)
	{
		_RBComplyRotationDamper = f;
		SetJointSprings();
	}

	private void SyncRBLinkRotationSpring(float f)
	{
		_RBLinkRotationSpring = f;
		SetLinkedJointSprings();
	}

	private void SyncRBLinkRotationDamper(float f)
	{
		_RBLinkRotationDamper = f;
		SetLinkedJointSprings();
	}

	private void SyncRBLinkRotationMaxForce(float f)
	{
		_RBLinkRotationMaxForce = f;
		SetLinkedJointSprings();
	}

	private void SyncRBComplyJointRotationDriveSpring(float f)
	{
		_RBComplyJointRotationDriveSpring = f;
		SetNaturalJointDrive();
	}

	private void SyncJointRotationDriveSpring(float f)
	{
		_jointRotationDriveSpring = f;
		SetNaturalJointDrive();
	}

	private void SyncJointRotationDriveDamper(float f)
	{
		_jointRotationDriveDamper = f;
		SetNaturalJointDrive();
	}

	private void SyncJointRotationDriveMaxForce(float f)
	{
		_jointRotationDriveMaxForce = f;
		SetNaturalJointDrive();
	}

	private void SyncJointRotationDriveXTarget(float f)
	{
		_jointRotationDriveXTarget = f;
		SetNaturalJointDriveTarget();
	}

	private void SyncJointRotationDriveYTarget(float f)
	{
		_jointRotationDriveYTarget = f;
		SetNaturalJointDriveTarget();
	}

	private void SyncJointRotationDriveZTarget(float f)
	{
		_jointRotationDriveZTarget = f;
		SetNaturalJointDriveTarget();
	}

	public void NextControlMode()
	{
		if (_controlMode == ControlMode.Off)
		{
			controlMode = ControlMode.Position;
		}
		else if (_controlMode == ControlMode.Position)
		{
			controlMode = ControlMode.Rotation;
		}
		else
		{
			controlMode = ControlMode.Position;
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			HideGUI();
		}
	}

	protected void SyncAtomUID()
	{
		if (containingAtom != null)
		{
			if (UIDText != null)
			{
				UIDText.text = containingAtom.uid + ":" + base.name;
			}
			if (UIDTextAlt != null)
			{
				UIDTextAlt.text = containingAtom.uid + ":" + base.name;
			}
		}
	}

	protected void RegisterFCUI(FreeControllerV3UI fcui, bool isAlt)
	{
		if (currentPositionStateJSON != null)
		{
			currentPositionStateJSON.RegisterToggleGroupValue(fcui.positionToggleGroup, isAlt);
		}
		if (currentRotationStateJSON != null)
		{
			currentRotationStateJSON.RegisterToggleGroupValue(fcui.rotationToggleGroup, isAlt);
		}
		RBHoldPositionSpringJSON.RegisterSlider(fcui.holdPositionSpringSlider, isAlt);
		RBHoldPositionDamperJSON.RegisterSlider(fcui.holdPositionDamperSlider, isAlt);
		RBHoldPositionMaxForceJSON.RegisterSlider(fcui.holdPositionMaxForceSlider, isAlt);
		RBHoldRotationSpringJSON.RegisterSlider(fcui.holdRotationSpringSlider, isAlt);
		RBHoldRotationDamperJSON.RegisterSlider(fcui.holdRotationDamperSlider, isAlt);
		RBHoldRotationMaxForceJSON.RegisterSlider(fcui.holdRotationMaxForceSlider, isAlt);
		RBComplyPositionSpringJSON.RegisterSlider(fcui.complyPositionSpringSlider, isAlt);
		RBComplyPositionDamperJSON.RegisterSlider(fcui.complyPositionDamperSlider, isAlt);
		RBComplyRotationSpringJSON.RegisterSlider(fcui.complyRotationSpringSlider, isAlt);
		RBComplyRotationDamperJSON.RegisterSlider(fcui.complyRotationDamperSlider, isAlt);
		RBComplyJointRotationDriveSpringJSON.RegisterSlider(fcui.complyJointRotationDriveSpringSlider, isAlt);
		if (complyPositionThresholdJSON != null)
		{
			complyPositionThresholdJSON.RegisterSlider(fcui.complyPositionThresholdSlider, isAlt);
		}
		if (complyRotationThresholdJSON != null)
		{
			complyRotationThresholdJSON.RegisterSlider(fcui.complyRotationThresholdSlider, isAlt);
		}
		if (complySpeedJSON != null)
		{
			complySpeedJSON.RegisterSlider(fcui.complySpeedSlider, isAlt);
		}
		RBLinkPositionSpringJSON.RegisterSlider(fcui.linkPositionSpringSlider, isAlt);
		RBLinkPositionDamperJSON.RegisterSlider(fcui.linkPositionDamperSlider, isAlt);
		RBLinkPositionMaxForceJSON.RegisterSlider(fcui.linkPositionMaxForceSlider, isAlt);
		RBLinkRotationSpringJSON.RegisterSlider(fcui.linkRotationSpringSlider, isAlt);
		RBLinkRotationDamperJSON.RegisterSlider(fcui.linkRotationDamperSlider, isAlt);
		RBLinkRotationMaxForceJSON.RegisterSlider(fcui.linkRotationMaxForceSlider, isAlt);
		jointRotationDriveSpringJSON.RegisterSlider(fcui.jointRotationDriveSpringSlider, isAlt);
		jointRotationDriveDamperJSON.RegisterSlider(fcui.jointRotationDriveDamperSlider, isAlt);
		jointRotationDriveMaxForceJSON.RegisterSlider(fcui.jointRotationDriveMaxForceSlider, isAlt);
		jointRotationDriveXTargetJSON.RegisterSlider(fcui.jointRotationDriveXTargetSlider, isAlt);
		jointRotationDriveYTargetJSON.RegisterSlider(fcui.jointRotationDriveYTargetSlider, isAlt);
		jointRotationDriveZTargetJSON.RegisterSlider(fcui.jointRotationDriveZTargetSlider, isAlt);
		if (onJSON != null)
		{
			onJSON.RegisterToggle(fcui.onToggle, isAlt);
		}
		RBMassJSON.RegisterSlider(fcui.massSlider, isAlt);
		RBDragJSON.RegisterSlider(fcui.dragSlider, isAlt);
		RBAngularDragJSON.RegisterSlider(fcui.angularDragSlider, isAlt);
		physicsEnabledJSON.RegisterToggle(fcui.physicsEnabledToggle, isAlt);
		if (collisionEnabledJSON != null)
		{
			collisionEnabledJSON.RegisterToggle(fcui.collisionEnabledToggle, isAlt);
		}
		useGravityJSON.RegisterToggle(fcui.useGravityWhenOffToggle, isAlt);
		interactableInPlayModeJSON.RegisterToggle(fcui.interactableInPlayModeToggle, isAlt);
		possessableJSON.RegisterToggle(fcui.possessableToggle, isAlt);
		canGrabPositionJSON.RegisterToggle(fcui.canGrabPositionToggle, isAlt);
		canGrabRotationJSON.RegisterToggle(fcui.canGrabRotationToggle, isAlt);
		positionGridModeJSON.RegisterPopup(fcui.positionGridModePopup, isAlt);
		rotationGridModeJSON.RegisterPopup(fcui.rotationGridModePopup, isAlt);
		positionGridJSON.RegisterSlider(fcui.positionGridSlider, isAlt);
		rotationGridJSON.RegisterSlider(fcui.rotationGridSlider, isAlt);
		xLockJSON.RegisterToggle(fcui.xPositionLockToggle, isAlt);
		yLockJSON.RegisterToggle(fcui.yPositionLockToggle, isAlt);
		zLockJSON.RegisterToggle(fcui.zPositionLockToggle, isAlt);
		xLocalLockJSON.RegisterToggle(fcui.xPositionLocalLockToggle, isAlt);
		yLocalLockJSON.RegisterToggle(fcui.yPositionLocalLockToggle, isAlt);
		zLocalLockJSON.RegisterToggle(fcui.zPositionLocalLockToggle, isAlt);
		xRotLockJSON.RegisterToggle(fcui.xRotationLockToggle, isAlt);
		yRotLockJSON.RegisterToggle(fcui.yRotationLockToggle, isAlt);
		zRotLockJSON.RegisterToggle(fcui.zRotationLockToggle, isAlt);
		if (fcui.linkToAtomSelectionPopup != null)
		{
			fcui.linkToAtomSelectionPopup.numPopupValues = 1;
			fcui.linkToAtomSelectionPopup.setPopupValue(0, "None");
			if (linkToRB != null)
			{
				GetLinkToAtomUIDFromLinkToRB(linkToRB);
				SetLinkToAtom(linkToAtomUID);
				fcui.linkToAtomSelectionPopup.currentValue = linkToAtomUID;
			}
			else
			{
				fcui.linkToAtomSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup = fcui.linkToAtomSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetLinkToAtomNames));
			UIPopup uIPopup2 = fcui.linkToAtomSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetLinkToAtom));
		}
		if (fcui.linkToSelectionPopup != null)
		{
			fcui.linkToSelectionPopup.numPopupValues = 1;
			fcui.linkToSelectionPopup.setPopupValue(0, "None");
			if (linkToRB != null)
			{
				fcui.linkToSelectionPopup.currentValue = linkToRB.name;
			}
			else
			{
				onLinkToRigidbodyNamesChanged(null);
				fcui.linkToSelectionPopup.currentValue = "None";
			}
			UIPopup uIPopup3 = fcui.linkToSelectionPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetLinkToRigidbodyObject));
		}
		if (fcui.selectLinkToFromSceneButton != null)
		{
			fcui.selectLinkToFromSceneButton.onClick.AddListener(SelectLinkToRigidbodyFromScene);
		}
		if (fcui.selectAlignToFromSceneButton != null)
		{
			fcui.selectAlignToFromSceneButton.onClick.AddListener(SelectAlignToRigidbodyFromScene);
		}
		if (fcui.xPositionMinus1Button != null)
		{
			fcui.xPositionMinus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(-1f, 0f, 0f);
			});
		}
		if (fcui.xPositionMinusPoint1Button != null)
		{
			fcui.xPositionMinusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(-0.1f, 0f, 0f);
			});
		}
		if (fcui.xPositionMinusPoint01Button != null)
		{
			fcui.xPositionMinusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(-0.01f, 0f, 0f);
			});
		}
		if (fcui.xPositionPlusPoint01Button != null)
		{
			fcui.xPositionPlusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0.01f, 0f, 0f);
			});
		}
		if (fcui.xPositionPlusPoint1Button != null)
		{
			fcui.xPositionPlusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0.1f, 0f, 0f);
			});
		}
		if (fcui.xPositionPlus1Button != null)
		{
			fcui.xPositionPlus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(1f, 0f, 0f);
			});
		}
		if (fcui.yPositionMinus1Button != null)
		{
			fcui.yPositionMinus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, -1f, 0f);
			});
		}
		if (fcui.yPositionMinusPoint1Button != null)
		{
			fcui.yPositionMinusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, -0.1f, 0f);
			});
		}
		if (fcui.yPositionMinusPoint01Button != null)
		{
			fcui.yPositionMinusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, -0.01f, 0f);
			});
		}
		if (fcui.yPositionPlusPoint01Button != null)
		{
			fcui.yPositionPlusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0.01f, 0f);
			});
		}
		if (fcui.yPositionPlusPoint1Button != null)
		{
			fcui.yPositionPlusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0.1f, 0f);
			});
		}
		if (fcui.yPositionPlus1Button != null)
		{
			fcui.yPositionPlus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 1f, 0f);
			});
		}
		if (fcui.zPositionMinus1Button != null)
		{
			fcui.zPositionMinus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, -1f);
			});
		}
		if (fcui.zPositionMinusPoint1Button != null)
		{
			fcui.zPositionMinusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, -0.1f);
			});
		}
		if (fcui.zPositionMinusPoint01Button != null)
		{
			fcui.zPositionMinusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, -0.01f);
			});
		}
		if (fcui.zPositionPlusPoint01Button != null)
		{
			fcui.zPositionPlusPoint01Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, 0.01f);
			});
		}
		if (fcui.zPositionPlusPoint1Button != null)
		{
			fcui.zPositionPlusPoint1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, 0.1f);
			});
		}
		if (fcui.zPositionPlus1Button != null)
		{
			fcui.zPositionPlus1Button.onClick.AddListener(delegate
			{
				MoveAbsoluteNoForce(0f, 0f, 1f);
			});
		}
		if (fcui.xRotationMinus45Button != null)
		{
			fcui.xRotationMinus45Button.onClick.AddListener(delegate
			{
				XRotationAdd(-45f);
			});
		}
		if (fcui.xRotationMinus5Button != null)
		{
			fcui.xRotationMinus5Button.onClick.AddListener(delegate
			{
				XRotationAdd(-5f);
			});
		}
		if (fcui.xRotationMinusPoint5Button != null)
		{
			fcui.xRotationMinusPoint5Button.onClick.AddListener(delegate
			{
				XRotationAdd(-0.5f);
			});
		}
		if (fcui.xRotationPlusPoint5Button != null)
		{
			fcui.xRotationPlusPoint5Button.onClick.AddListener(delegate
			{
				XRotationAdd(0.5f);
			});
		}
		if (fcui.xRotationPlus5Button != null)
		{
			fcui.xRotationPlus5Button.onClick.AddListener(delegate
			{
				XRotationAdd(5f);
			});
		}
		if (fcui.xRotationPlus45Button != null)
		{
			fcui.xRotationPlus45Button.onClick.AddListener(delegate
			{
				XRotationAdd(45f);
			});
		}
		if (fcui.yRotationMinus45Button != null)
		{
			fcui.yRotationMinus45Button.onClick.AddListener(delegate
			{
				YRotationAdd(-45f);
			});
		}
		if (fcui.yRotationMinus5Button != null)
		{
			fcui.yRotationMinus5Button.onClick.AddListener(delegate
			{
				YRotationAdd(-5f);
			});
		}
		if (fcui.yRotationMinusPoint5Button != null)
		{
			fcui.yRotationMinusPoint5Button.onClick.AddListener(delegate
			{
				YRotationAdd(-0.5f);
			});
		}
		if (fcui.yRotationPlusPoint5Button != null)
		{
			fcui.yRotationPlusPoint5Button.onClick.AddListener(delegate
			{
				YRotationAdd(0.5f);
			});
		}
		if (fcui.yRotationPlus5Button != null)
		{
			fcui.yRotationPlus5Button.onClick.AddListener(delegate
			{
				YRotationAdd(5f);
			});
		}
		if (fcui.yRotationPlus45Button != null)
		{
			fcui.yRotationPlus45Button.onClick.AddListener(delegate
			{
				YRotationAdd(45f);
			});
		}
		if (fcui.zRotationMinus45Button != null)
		{
			fcui.zRotationMinus45Button.onClick.AddListener(delegate
			{
				ZRotationAdd(-45f);
			});
		}
		if (fcui.zRotationMinus5Button != null)
		{
			fcui.zRotationMinus5Button.onClick.AddListener(delegate
			{
				ZRotationAdd(-5f);
			});
		}
		if (fcui.zRotationMinusPoint5Button != null)
		{
			fcui.zRotationMinusPoint5Button.onClick.AddListener(delegate
			{
				ZRotationAdd(-0.5f);
			});
		}
		if (fcui.zRotationPlusPoint5Button != null)
		{
			fcui.zRotationPlusPoint5Button.onClick.AddListener(delegate
			{
				ZRotationAdd(0.5f);
			});
		}
		if (fcui.zRotationPlus5Button != null)
		{
			fcui.zRotationPlus5Button.onClick.AddListener(delegate
			{
				ZRotationAdd(5f);
			});
		}
		if (fcui.zRotationPlus45Button != null)
		{
			fcui.zRotationPlus45Button.onClick.AddListener(delegate
			{
				ZRotationAdd(45f);
			});
		}
		if (fcui.xPosition0Button != null)
		{
			fcui.xPosition0Button.onClick.AddListener(delegate
			{
				MoveTo(0f, control.position.y, control.position.z);
			});
		}
		if (fcui.yPosition0Button != null)
		{
			fcui.yPosition0Button.onClick.AddListener(delegate
			{
				MoveTo(control.position.x, 0f, control.position.z);
			});
		}
		if (fcui.zPosition0Button != null)
		{
			fcui.zPosition0Button.onClick.AddListener(delegate
			{
				MoveTo(control.position.x, control.position.y, 0f);
			});
		}
		if (fcui.xRotation0Button != null)
		{
			fcui.xRotation0Button.onClick.AddListener(XRotation0);
		}
		if (fcui.yRotation0Button != null)
		{
			fcui.yRotation0Button.onClick.AddListener(YRotation0);
		}
		if (fcui.zRotation0Button != null)
		{
			fcui.zRotation0Button.onClick.AddListener(ZRotation0);
		}
		if (fcui.xPositionSnapPoint1Button != null)
		{
			fcui.xPositionSnapPoint1Button.onClick.AddListener(XPositionSnapPoint1);
		}
		if (fcui.yPositionSnapPoint1Button != null)
		{
			fcui.yPositionSnapPoint1Button.onClick.AddListener(YPositionSnapPoint1);
		}
		if (fcui.zPositionSnapPoint1Button != null)
		{
			fcui.zPositionSnapPoint1Button.onClick.AddListener(ZPositionSnapPoint1);
		}
		if (fcui.xRotationSnap1Button != null)
		{
			fcui.xRotationSnap1Button.onClick.AddListener(XRotationSnap1);
		}
		if (fcui.yRotationSnap1Button != null)
		{
			fcui.yRotationSnap1Button.onClick.AddListener(YRotationSnap1);
		}
		if (fcui.zRotationSnap1Button != null)
		{
			fcui.zRotationSnap1Button.onClick.AddListener(ZRotationSnap1);
		}
		if (fcui.selectRootButton != null)
		{
			if (enableSelectRoot)
			{
				fcui.selectRootButton.gameObject.SetActive(value: true);
				fcui.selectRootButton.onClick.AddListener(SelectRoot);
			}
			else
			{
				fcui.selectRootButton.gameObject.SetActive(value: false);
			}
		}
		if (isAlt)
		{
			UIDTextAlt = fcui.UIDText;
			if (UIDTextAlt != null)
			{
				if (containingAtom != null)
				{
					UIDTextAlt.text = containingAtom.uid + ":" + base.name;
				}
				else
				{
					UIDTextAlt.text = base.name;
				}
			}
			xPositionTextAlt = fcui.xPositionText;
			yPositionTextAlt = fcui.yPositionText;
			zPositionTextAlt = fcui.zPositionText;
			xRotationTextAlt = fcui.xRotationText;
			yRotationTextAlt = fcui.yRotationText;
			zRotationTextAlt = fcui.zRotationText;
			return;
		}
		UIDText = fcui.UIDText;
		if (UIDText != null)
		{
			if (containingAtom != null)
			{
				UIDText.text = containingAtom.uid + ":" + base.name;
			}
			else
			{
				UIDText.text = base.name;
			}
		}
		xPositionText = fcui.xPositionText;
		yPositionText = fcui.yPositionText;
		zPositionText = fcui.zPositionText;
		xRotationText = fcui.xRotationText;
		yRotationText = fcui.yRotationText;
		zRotationText = fcui.zRotationText;
		linkToAtomSelectionPopup = fcui.linkToAtomSelectionPopup;
		linkToSelectionPopup = fcui.linkToSelectionPopup;
	}

	protected void DeregisterFCUI(FreeControllerV3UI fcui)
	{
		if (fcui.selectLinkToFromSceneButton != null)
		{
			fcui.selectLinkToFromSceneButton.onClick.RemoveListener(SelectLinkToRigidbodyFromScene);
		}
		if (fcui.selectAlignToFromSceneButton != null)
		{
			fcui.selectAlignToFromSceneButton.onClick.RemoveListener(SelectAlignToRigidbodyFromScene);
		}
		if (fcui.xPositionMinus1Button != null)
		{
			fcui.xPositionMinus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionMinusPoint1Button != null)
		{
			fcui.xPositionMinusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionMinusPoint01Button != null)
		{
			fcui.xPositionMinusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionPlusPoint01Button != null)
		{
			fcui.xPositionPlusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionPlusPoint1Button != null)
		{
			fcui.xPositionPlusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPositionPlus1Button != null)
		{
			fcui.xPositionPlus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionMinus1Button != null)
		{
			fcui.yPositionMinus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionMinusPoint1Button != null)
		{
			fcui.yPositionMinusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionMinusPoint01Button != null)
		{
			fcui.yPositionMinusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionPlusPoint01Button != null)
		{
			fcui.yPositionPlusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionPlusPoint1Button != null)
		{
			fcui.yPositionPlusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPositionPlus1Button != null)
		{
			fcui.yPositionPlus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionMinus1Button != null)
		{
			fcui.zPositionMinus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionMinusPoint1Button != null)
		{
			fcui.zPositionMinusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionMinusPoint01Button != null)
		{
			fcui.zPositionMinusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionPlusPoint01Button != null)
		{
			fcui.zPositionPlusPoint01Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionPlusPoint1Button != null)
		{
			fcui.zPositionPlusPoint1Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPositionPlus1Button != null)
		{
			fcui.zPositionPlus1Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationMinus45Button != null)
		{
			fcui.xRotationMinus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationMinus5Button != null)
		{
			fcui.xRotationMinus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationMinusPoint5Button != null)
		{
			fcui.xRotationMinusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationPlusPoint5Button != null)
		{
			fcui.xRotationPlusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationPlus5Button != null)
		{
			fcui.xRotationPlus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotationPlus45Button != null)
		{
			fcui.xRotationPlus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationMinus45Button != null)
		{
			fcui.yRotationMinus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationMinus5Button != null)
		{
			fcui.yRotationMinus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationMinusPoint5Button != null)
		{
			fcui.yRotationMinusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationPlusPoint5Button != null)
		{
			fcui.yRotationPlusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationPlus5Button != null)
		{
			fcui.yRotationPlus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.yRotationPlus45Button != null)
		{
			fcui.yRotationPlus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationMinus45Button != null)
		{
			fcui.zRotationMinus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationMinus5Button != null)
		{
			fcui.zRotationMinus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationMinusPoint5Button != null)
		{
			fcui.zRotationMinusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationPlusPoint5Button != null)
		{
			fcui.zRotationPlusPoint5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationPlus5Button != null)
		{
			fcui.zRotationPlus5Button.onClick.RemoveAllListeners();
		}
		if (fcui.zRotationPlus45Button != null)
		{
			fcui.zRotationPlus45Button.onClick.RemoveAllListeners();
		}
		if (fcui.xPosition0Button != null)
		{
			fcui.xPosition0Button.onClick.RemoveAllListeners();
		}
		if (fcui.yPosition0Button != null)
		{
			fcui.yPosition0Button.onClick.RemoveAllListeners();
		}
		if (fcui.zPosition0Button != null)
		{
			fcui.zPosition0Button.onClick.RemoveAllListeners();
		}
		if (fcui.xRotation0Button != null)
		{
			fcui.xRotation0Button.onClick.RemoveListener(XRotation0);
		}
		if (fcui.yRotation0Button != null)
		{
			fcui.yRotation0Button.onClick.RemoveListener(YRotation0);
		}
		if (fcui.zRotation0Button != null)
		{
			fcui.zRotation0Button.onClick.RemoveListener(ZRotation0);
		}
		if (fcui.xPositionSnapPoint1Button != null)
		{
			fcui.xPositionSnapPoint1Button.onClick.RemoveListener(XPositionSnapPoint1);
		}
		if (fcui.yPositionSnapPoint1Button != null)
		{
			fcui.yPositionSnapPoint1Button.onClick.RemoveListener(YPositionSnapPoint1);
		}
		if (fcui.zPositionSnapPoint1Button != null)
		{
			fcui.zPositionSnapPoint1Button.onClick.RemoveListener(ZPositionSnapPoint1);
		}
		if (fcui.xRotationSnap1Button != null)
		{
			fcui.xRotationSnap1Button.onClick.RemoveListener(XRotationSnap1);
		}
		if (fcui.yRotationSnap1Button != null)
		{
			fcui.yRotationSnap1Button.onClick.RemoveListener(YRotationSnap1);
		}
		if (fcui.zRotationSnap1Button != null)
		{
			fcui.zRotationSnap1Button.onClick.RemoveListener(ZRotationSnap1);
		}
		if (fcui.linkToAtomSelectionPopup != null)
		{
			UIPopup uIPopup = linkToAtomSelectionPopup;
			uIPopup.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetLinkToAtomNames));
			UIPopup uIPopup2 = linkToAtomSelectionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetLinkToAtom));
		}
		if (fcui.linkToSelectionPopup != null)
		{
			UIPopup uIPopup3 = linkToSelectionPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetLinkToRigidbodyObject));
		}
		if (enableSelectRoot && fcui.selectRootButton != null)
		{
			fcui.selectRootButton.onClick.RemoveListener(SelectRoot);
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		FreeControllerV3UI componentInChildren = UITransform.GetComponentInChildren<FreeControllerV3UI>();
		if (componentInChildren != null)
		{
			if (currentFCUI != null)
			{
				DeregisterFCUI(currentFCUI);
			}
			currentFCUI = componentInChildren;
			RegisterFCUI(componentInChildren, isAlt: false);
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		FreeControllerV3UI componentInChildren = UITransformAlt.GetComponentInChildren<FreeControllerV3UI>();
		if (componentInChildren != null)
		{
			if (currentFCUIAlt != null)
			{
				DeregisterFCUI(currentFCUIAlt);
			}
			currentFCUIAlt = componentInChildren;
			RegisterFCUI(componentInChildren, isAlt: true);
		}
	}

	public void TakeSnapshot()
	{
		snapshotMatrix = control.localToWorldMatrix;
	}

	private Vector3 GetVectorFromAxis(DrawAxisnames axis)
	{
		Vector3 zero = Vector3.zero;
		switch (axis)
		{
			case DrawAxisnames.X:
				zero.x = 1f;
				break;
			case DrawAxisnames.Y:
				zero.y = 1f;
				break;
			case DrawAxisnames.Z:
				zero.z = 1f;
				break;
			case DrawAxisnames.NegX:
				zero.x = -1f;
				break;
			case DrawAxisnames.NegY:
				zero.y = -1f;
				break;
			case DrawAxisnames.NegZ:
				zero.z = -1f;
				break;
		}
		return zero;
	}

	private void ApplyForce()
	{
		if ((bool)_followWhenOffRB)
		{
			if (_moveForceEnabled)
			{
				_followWhenOffRB.AddForce(appliedForce * Time.fixedDeltaTime, ForceMode.Force);
			}
			if (_rotationForceEnabled)
			{
				_followWhenOffRB.AddRelativeTorque(appliedTorque * Time.fixedDeltaTime, ForceMode.Force);
			}
		}
	}

	protected void SyncComplyPositionThreshold(float f)
	{
		complyPositionThreshold = f;
	}

	protected void SyncComplyRotationThreshold(float f)
	{
		complyRotationThreshold = f;
	}

	protected void SyncComplySpeed(float f)
	{
		complySpeed = f;
	}

	public void PauseComply(int numFrames = 100)
	{
		if (complyPauseFrames < numFrames)
		{
			complyPauseFrames = numFrames;
		}
	}

	private void ApplyComply()
	{
		if (!(SuperController.singleton == null) && SuperController.singleton.IsSimulationPaused())
		{
			return;
		}
		if (complyPauseFrames > 0)
		{
			complyPauseFrames--;
		}
		if (complyPauseFrames != 0 || !(control != null) || !(_followWhenOffRB != null) || _followWhenOffRB.isKinematic)
		{
			return;
		}
		if (_currentPositionState == PositionState.Comply)
		{
			Vector3 vector = _followWhenOffRB.position - Physics.gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
			Vector3 vector2 = vector - control.position;
			if (vector2.sqrMagnitude > complyPositionThreshold)
			{
				control.position += vector2 * Time.fixedDeltaTime * complySpeed;
			}
		}
		if (_currentRotationState == RotationState.Comply)
		{
			float num = Quaternion.Angle(control.rotation, _followWhenOffRB.rotation);
			if (num > complyRotationThreshold)
			{
				control.rotation = Quaternion.Lerp(control.rotation, _followWhenOffRB.rotation, Mathf.Clamp01(Time.fixedDeltaTime * complySpeed));
			}
		}
	}

	public void MoveControlRelatve(Vector3 move)
	{
		MoveControl(control.position + move);
	}

	public void MoveControl(Vector3 newPosition)
	{
		if (_positionGridMode == GridMode.Global)
		{
			if (GridControl.singleton != null)
			{
				float num = GridControl.singleton.positionGrid;
				float num2 = Mathf.Round(newPosition.x / num);
				newPosition.x = num2 * num;
				float num3 = Mathf.Round(newPosition.y / num);
				newPosition.y = num3 * num;
				float num4 = Mathf.Round(newPosition.z / num);
				newPosition.z = num4 * num;
			}
		}
		else if (_positionGridMode == GridMode.Local)
		{
			float num5 = Mathf.Round(newPosition.x / _positionGrid);
			newPosition.x = num5 * _positionGrid;
			float num6 = Mathf.Round(newPosition.y / _positionGrid);
			newPosition.y = num6 * _positionGrid;
			float num7 = Mathf.Round(newPosition.z / _positionGrid);
			newPosition.z = num7 * _positionGrid;
		}
		Vector3 position = control.position;
		Vector3 localPosition = control.localPosition;
		if (!_xLock)
		{
			position.x = newPosition.x;
		}
		if (!_yLock)
		{
			position.y = newPosition.y;
		}
		if (!_zLock)
		{
			position.z = newPosition.z;
		}
		control.position = position;
		if (_xLocalLock || _yLocalLock || _zLocalLock)
		{
			Vector3 localPosition2 = control.localPosition;
			if (_xLocalLock)
			{
				localPosition2.x = localPosition.x;
			}
			if (_yLocalLock)
			{
				localPosition2.y = localPosition.y;
			}
			if (_zLocalLock)
			{
				localPosition2.z = localPosition.z;
			}
			control.localPosition = localPosition2;
		}
	}

	public void MoveLinkConnectorTowards(Transform t, float moveDistance)
	{
		if (_linkToConnector != null)
		{
			_linkToConnector.transform.Translate(0f, 0f, moveDistance, t);
		}
	}

	public void RotateControl(Vector3 axis, float angle)
	{
		Vector3 eulerAngles = control.eulerAngles;
		control.transform.Rotate(axis, angle, Space.World);
		Vector3 eulerAngles2 = control.eulerAngles;
		RotateControlContrained(eulerAngles, eulerAngles2);
	}

	public void RotateControl(Vector3 newWorldRotation)
	{
		Vector3 eulerAngles = control.eulerAngles;
		RotateControlContrained(eulerAngles, newWorldRotation);
	}

	private void RotateControlContrained(Vector3 oldRotation, Vector3 newRotation)
	{
		if (_rotationGridMode == GridMode.Global)
		{
			if (GridControl.singleton != null)
			{
				float num = GridControl.singleton.rotationGrid;
				float num2 = Mathf.Round(newRotation.x / num);
				newRotation.x = num2 * num;
				float num3 = Mathf.Round(newRotation.y / num);
				newRotation.y = num3 * num;
				float num4 = Mathf.Round(newRotation.z / num);
				newRotation.z = num4 * num;
			}
		}
		else if (_rotationGridMode == GridMode.Local)
		{
			float num5 = Mathf.Round(newRotation.x / _rotationGrid);
			newRotation.x = num5 * _rotationGrid;
			float num6 = Mathf.Round(newRotation.y / _rotationGrid);
			newRotation.y = num6 * _rotationGrid;
			float num7 = Mathf.Round(newRotation.z / _rotationGrid);
			newRotation.z = num7 * _rotationGrid;
		}
		if (!xRotLock)
		{
			oldRotation.x = newRotation.x;
		}
		if (!yRotLock)
		{
			oldRotation.y = newRotation.y;
		}
		if (!zRotLock)
		{
			oldRotation.z = newRotation.z;
		}
		control.eulerAngles = oldRotation;
	}

	private void UpdateTransform(bool updateGUI)
	{
		if (_currentPositionState == PositionState.Off)
		{
			if ((bool)followWhenOff)
			{
				control.position = followWhenOff.position;
			}
		}
		else if (_currentPositionState == PositionState.Following)
		{
			if ((bool)follow)
			{
				MoveControl(follow.position);
			}
		}
		else if ((_currentPositionState == PositionState.ParentLink || _currentPositionState == PositionState.PhysicsLink) && (bool)_linkToConnector)
		{
			MoveControl(_linkToConnector.position);
		}
		if (_currentRotationState == RotationState.Off)
		{
			if ((bool)followWhenOff)
			{
				control.rotation = followWhenOff.rotation;
			}
		}
		else if (_currentRotationState == RotationState.Following)
		{
			if ((bool)follow)
			{
				RotateControl(follow.eulerAngles);
			}
		}
		else if (_currentRotationState == RotationState.LookAt)
		{
			control.LookAt(lookAt.position);
		}
		else if ((_currentRotationState == RotationState.ParentLink || _currentPositionState == PositionState.PhysicsLink) && (bool)_linkToConnector)
		{
			RotateControl(_linkToConnector.eulerAngles);
		}
		if (control != base.transform)
		{
			base.transform.position = control.position;
			base.transform.rotation = control.rotation;
		}
		if (alsoMoveWhenInactive != null && alsoMoveWhenInactiveParentWhenActive != null && alsoMoveWhenInactiveParentWhenInactive != null)
		{
			if (alsoMoveWhenInactive.gameObject.activeInHierarchy)
			{
				if (alsoMoveWhenInactive.parent != alsoMoveWhenInactiveParentWhenActive)
				{
					alsoMoveWhenInactive.SetParent(alsoMoveWhenInactiveParentWhenActive);
				}
			}
			else
			{
				alsoMoveWhenInactiveParentWhenInactive.position = control.position;
				alsoMoveWhenInactiveParentWhenInactive.rotation = control.rotation;
				if (alsoMoveWhenInactive.parent != alsoMoveWhenInactiveParentWhenInactive)
				{
					alsoMoveWhenInactive.SetParent(alsoMoveWhenInactiveParentWhenInactive);
				}
			}
		}
		if (_followWhenOffRB != null)
		{
			if (!_followWhenOffRB.gameObject.activeInHierarchy)
			{
				_followWhenOffRB.position = control.position;
				_followWhenOffRB.transform.position = control.position;
				_followWhenOffRB.rotation = control.rotation;
				_followWhenOffRB.transform.rotation = control.rotation;
			}
			else if (_followWhenOffRB.isKinematic)
			{
				followWhenOff.position = control.position;
				followWhenOff.rotation = control.rotation;
			}
		}
		if (updateGUI && !_guihidden)
		{
			if (xPositionText != null)
			{
				xPositionText.floatVal = control.position.x;
			}
			if (yPositionText != null)
			{
				yPositionText.floatVal = control.position.y;
			}
			if (zPositionText != null)
			{
				zPositionText.floatVal = control.position.z;
			}
			Vector3 eulerAngles = control.eulerAngles;
			if (xRotationText != null)
			{
				xRotationText.floatVal = eulerAngles.x;
			}
			if (yRotationText != null)
			{
				yRotationText.floatVal = eulerAngles.y;
			}
			if (zRotationText != null)
			{
				zRotationText.floatVal = eulerAngles.z;
			}
		}
	}

	public void ShowGUI()
	{
		bool flag = false;
		if (SuperController.singleton != null && SuperController.singleton.gameMode == SuperController.GameMode.Play)
		{
			flag = true;
		}
		if (flag)
		{
			Transform[] uITransforms = UITransforms;
			foreach (Transform transform in uITransforms)
			{
				if (transform != null)
				{
					transform.gameObject.SetActive(value: false);
				}
			}
			Transform[] uITransformsPlayMode = UITransformsPlayMode;
			foreach (Transform transform2 in uITransformsPlayMode)
			{
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(value: true);
				}
			}
			return;
		}
		Transform[] uITransformsPlayMode2 = UITransformsPlayMode;
		foreach (Transform transform3 in uITransformsPlayMode2)
		{
			if (transform3 != null)
			{
				transform3.gameObject.SetActive(value: false);
			}
		}
		Transform[] uITransforms2 = UITransforms;
		foreach (Transform transform4 in uITransforms2)
		{
			if (transform4 != null)
			{
				transform4.gameObject.SetActive(value: true);
			}
		}
	}

	public void HideGUI()
	{
		bool flag = false;
		if (SuperController.singleton != null && SuperController.singleton.gameMode == SuperController.GameMode.Play)
		{
			flag = true;
		}
		Transform[] uITransforms = UITransforms;
		foreach (Transform transform in uITransforms)
		{
			if (!(transform != null))
			{
				continue;
			}
			if (flag)
			{
				transform.gameObject.SetActive(value: false);
				continue;
			}
			UIVisibility component = transform.GetComponent<UIVisibility>();
			if (component != null)
			{
				if (!component.keepVisible)
				{
					transform.gameObject.SetActive(value: false);
				}
			}
			else
			{
				transform.gameObject.SetActive(value: false);
			}
		}
		Transform[] uITransformsPlayMode = UITransformsPlayMode;
		foreach (Transform transform2 in uITransformsPlayMode)
		{
			if (!(transform2 != null))
			{
				continue;
			}
			if (!flag)
			{
				transform2.gameObject.SetActive(value: false);
				continue;
			}
			UIVisibility component2 = transform2.GetComponent<UIVisibility>();
			if (component2 != null)
			{
				if (!component2.keepVisible)
				{
					transform2.gameObject.SetActive(value: false);
				}
			}
			else
			{
				transform2.gameObject.SetActive(value: false);
			}
		}
	}

	private void SetColor()
	{
		if (_selected)
		{
			_currentPositionColor = selectedColor;
			_currentRotationColor = selectedColor;
		}
		else if (_highlighted)
		{
			_currentPositionColor = highlightColor;
			_currentRotationColor = highlightColor;
		}
		else
		{
			switch (_currentPositionState)
			{
				case PositionState.On:
				case PositionState.Comply:
					_currentPositionColor = onColor;
					break;
				case PositionState.Off:
					_currentPositionColor = offColor;
					break;
				case PositionState.Following:
				case PositionState.ParentLink:
				case PositionState.PhysicsLink:
					_currentPositionColor = followingColor;
					break;
				case PositionState.Hold:
					_currentPositionColor = holdColor;
					break;
				case PositionState.Lock:
					_currentPositionColor = lockColor;
					break;
			}
			switch (_currentRotationState)
			{
				case RotationState.On:
				case RotationState.Comply:
					_currentRotationColor = onColor;
					break;
				case RotationState.Off:
					_currentRotationColor = offColor;
					break;
				case RotationState.Following:
				case RotationState.ParentLink:
				case RotationState.PhysicsLink:
					_currentRotationColor = followingColor;
					break;
				case RotationState.Hold:
					_currentRotationColor = holdColor;
					break;
				case RotationState.Lock:
					_currentRotationColor = lockColor;
					break;
				case RotationState.LookAt:
					_currentRotationColor = lookAtColor;
					break;
			}
		}
		if (mrs != null)
		{
			MeshRenderer[] array = mrs;
			foreach (MeshRenderer meshRenderer in array)
			{
				meshRenderer.material.color = _currentPositionColor;
			}
		}
	}

	private void SetMesh()
	{
		switch (_currentPositionState)
		{
			case PositionState.Off:
				_currentPositionMesh = offPositionMesh;
				break;
			case PositionState.On:
			case PositionState.Comply:
				_currentPositionMesh = onPositionMesh;
				break;
			case PositionState.Following:
			case PositionState.ParentLink:
			case PositionState.PhysicsLink:
				_currentPositionMesh = followingPositionMesh;
				break;
			case PositionState.Hold:
				_currentPositionMesh = holdPositionMesh;
				break;
			case PositionState.Lock:
				_currentPositionMesh = lockPositionMesh;
				break;
		}
		switch (_currentRotationState)
		{
			case RotationState.Off:
				_currentRotationMesh = offRotationMesh;
				break;
			case RotationState.On:
			case RotationState.Comply:
				_currentRotationMesh = onRotationMesh;
				break;
			case RotationState.Following:
			case RotationState.ParentLink:
			case RotationState.PhysicsLink:
				_currentRotationMesh = followingRotationMesh;
				break;
			case RotationState.Hold:
				_currentRotationMesh = holdRotationMesh;
				break;
			case RotationState.Lock:
				_currentRotationMesh = lockRotationMesh;
				break;
			case RotationState.LookAt:
				_currentRotationMesh = lookAtRotationMesh;
				break;
		}
	}

	private void StateChanged()
	{
		SetMesh();
		SetColor();
	}

	public void ResetControl()
	{
		control.localPosition = initialLocalPosition;
		control.localRotation = initialLocalRotation;
	}

	private void Move(Vector3 direction)
	{
		if (_moveForceEnabled && (bool)_followWhenOffRB && useForceWhenOff)
		{
			appliedForce += direction * forceFactor;
		}
		else if (_moveEnabled)
		{
			Vector3 translation = direction * moveFactor * Time.unscaledDeltaTime;
			control.Translate(translation, Space.World);
			if (connectedJoint != null)
			{
				_followWhenOffRB.WakeUp();
			}
		}
	}

	private void MoveAbsoluteNoForce(Vector3 direction)
	{
		if (!_moveForceEnabled && _moveEnabled)
		{
			control.Translate(direction, Space.World);
		}
	}

	private void MoveAbsoluteNoForce(float x, float y, float z)
	{
		Vector3 direction = default(Vector3);
		direction.x = x;
		direction.y = y;
		direction.z = z;
		MoveAbsoluteNoForce(direction);
	}

	private void MoveTo(Vector3 pos, bool alsoMoveRB = false)
	{
		if (!_moveForceEnabled && _moveEnabled)
		{
			control.position = pos;
			if (alsoMoveRB && followWhenOff != null)
			{
				followWhenOff.position = pos;
			}
		}
	}

	private void MoveTo(float x, float y, float z)
	{
		Vector3 pos = default(Vector3);
		pos.x = x;
		pos.y = y;
		pos.z = z;
		MoveTo(pos);
	}

	public void PossessMoveAndAlignTo(Transform t)
	{
		if (_canGrabRotation)
		{
			AlignTo(t, alsoRotateRB: true);
		}
		if (!_canGrabPosition)
		{
			return;
		}
		if (possessPoint != null && followWhenOff != null)
		{
			Vector3 position = t.position + (followWhenOff.position - possessPoint.position);
			control.position = position;
			followWhenOff.position = position;
			return;
		}
		control.position = t.position;
		if (followWhenOff != null)
		{
			followWhenOff.position = t.position;
		}
	}

	public void RotateTo(Quaternion q)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			control.rotation = q;
		}
	}

	public Vector3 GetForwardPossessAxis()
	{
		Vector3 result = Vector3.forward;
		switch (PossessForwardAxis)
		{
			case DrawAxisnames.X:
				result = base.transform.right;
				break;
			case DrawAxisnames.NegX:
				result = -base.transform.right;
				break;
			case DrawAxisnames.Y:
				result = base.transform.up;
				break;
			case DrawAxisnames.NegY:
				result = -base.transform.up;
				break;
			case DrawAxisnames.Z:
				result = base.transform.forward;
				break;
			case DrawAxisnames.NegZ:
				result = -base.transform.forward;
				break;
		}
		return result;
	}

	public Vector3 GetUpPossessAxis()
	{
		Vector3 result = Vector3.up;
		switch (PossessUpAxis)
		{
			case DrawAxisnames.X:
				result = base.transform.right;
				break;
			case DrawAxisnames.NegX:
				result = -base.transform.right;
				break;
			case DrawAxisnames.Y:
				result = base.transform.up;
				break;
			case DrawAxisnames.NegY:
				result = -base.transform.up;
				break;
			case DrawAxisnames.Z:
				result = base.transform.forward;
				break;
			case DrawAxisnames.NegZ:
				result = -base.transform.forward;
				break;
		}
		return result;
	}

	public void AlignTo(Transform t, bool alsoRotateRB = false)
	{
		Quaternion rotation = control.rotation;
		Vector3 view = Vector3.forward;
		Vector3 up = Vector3.up;
		switch (PossessForwardAxis)
		{
			case DrawAxisnames.X:
				view = t.right;
				break;
			case DrawAxisnames.NegX:
				view = -t.right;
				break;
			case DrawAxisnames.Y:
				view = t.up;
				break;
			case DrawAxisnames.NegY:
				view = -t.up;
				break;
			case DrawAxisnames.Z:
				view = t.forward;
				break;
			case DrawAxisnames.NegZ:
				view = -t.forward;
				break;
		}
		switch (PossessUpAxis)
		{
			case DrawAxisnames.X:
				up = t.right;
				break;
			case DrawAxisnames.NegX:
				up = -t.right;
				break;
			case DrawAxisnames.Y:
				up = t.up;
				break;
			case DrawAxisnames.NegY:
				up = -t.up;
				break;
			case DrawAxisnames.Z:
				up = t.forward;
				break;
			case DrawAxisnames.NegZ:
				up = -t.forward;
				break;
		}
		rotation.SetLookRotation(view, up);
		control.rotation = rotation;
		if (alsoRotateRB && followWhenOff != null)
		{
			followWhenOff.rotation = rotation;
		}
	}

	public void RotateX(float val)
	{
		if (_rotationForceEnabled && (bool)_followWhenOffRB && useForceWhenOff)
		{
			appliedTorque.x = val * torqueFactor;
		}
		else if (_rotationEnabled)
		{
			control.Rotate(new Vector3(val * rotateFactor * Time.unscaledDeltaTime, 0f, 0f));
			if (connectedJoint != null)
			{
				_followWhenOffRB.WakeUp();
			}
		}
	}

	public void RotateY(float val)
	{
		if (_rotationForceEnabled && (bool)_followWhenOffRB && useForceWhenOff)
		{
			appliedTorque.y = val * torqueFactor;
		}
		else if (_rotationEnabled)
		{
			control.Rotate(new Vector3(0f, val * rotateFactor * Time.unscaledDeltaTime, 0f));
			if (connectedJoint != null)
			{
				_followWhenOffRB.WakeUp();
			}
		}
	}

	public void RotateZ(float val)
	{
		if (_rotationForceEnabled && (bool)_followWhenOffRB && useForceWhenOff)
		{
			appliedTorque.z = val * torqueFactor;
		}
		else if (_rotationEnabled)
		{
			control.Rotate(new Vector3(0f, 0f, val * rotateFactor * Time.unscaledDeltaTime));
			if (connectedJoint != null)
			{
				_followWhenOffRB.WakeUp();
			}
		}
	}

	public void RotateWorldX(float val, bool absolute = false)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			float num = val;
			if (!absolute)
			{
				num *= rotateFactor * Time.unscaledDeltaTime;
			}
			control.Rotate(num, 0f, 0f, Space.World);
		}
	}

	public void RotateWorldY(float val, bool absolute = false)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			float num = val;
			if (!absolute)
			{
				num *= rotateFactor * Time.unscaledDeltaTime;
			}
			control.Rotate(0f, num, 0f, Space.World);
		}
	}

	public void RotateWorldZ(float val, bool absolute = false)
	{
		if (!_rotationForceEnabled && _rotationEnabled)
		{
			float num = val;
			if (!absolute)
			{
				num *= rotateFactor * Time.unscaledDeltaTime;
			}
			control.Rotate(0f, 0f, num, Space.World);
		}
	}

	public void ResetAppliedForces()
	{
		appliedForce.x = 0f;
		appliedForce.y = 0f;
		appliedForce.z = 0f;
		appliedTorque.x = 0f;
		appliedTorque.y = 0f;
		appliedTorque.z = 0f;
	}

	public void MoveAxis(MoveAxisnames man, float val)
	{
		switch (man)
		{
			case MoveAxisnames.X:
				Move(new Vector3(val, 0f, 0f));
				break;
			case MoveAxisnames.Y:
				Move(new Vector3(0f, val, 0f));
				break;
			case MoveAxisnames.Z:
				Move(new Vector3(0f, 0f, val));
				break;
			case MoveAxisnames.CameraRight:
			{
				Vector3 direction5 = Camera.main.transform.right * val;
				Move(direction5);
				break;
			}
			case MoveAxisnames.CameraRightNoY:
			{
				Vector3 direction4 = Camera.main.transform.right * val;
				direction4.y = 0f;
				Move(direction4);
				break;
			}
			case MoveAxisnames.CameraForward:
			{
				Vector3 direction3 = Camera.main.transform.forward * val;
				Move(direction3);
				break;
			}
			case MoveAxisnames.CameraForwardNoY:
			{
				Vector3 direction2 = Camera.main.transform.forward * val;
				direction2.y = 0f;
				Move(direction2);
				break;
			}
			case MoveAxisnames.CameraUp:
			{
				Vector3 direction = Camera.main.transform.up * val;
				Move(direction);
				break;
			}
		}
	}

	public void RotateAxis(RotateAxisnames ran, float val)
	{
		switch (ran)
		{
			case RotateAxisnames.X:
				RotateX(val);
				break;
			case RotateAxisnames.NegX:
				RotateX(0f - val);
				break;
			case RotateAxisnames.Y:
				RotateY(val);
				break;
			case RotateAxisnames.NegY:
				RotateY(0f - val);
				break;
			case RotateAxisnames.Z:
				RotateZ(val);
				break;
			case RotateAxisnames.NegZ:
				RotateZ(0f - val);
				break;
			case RotateAxisnames.WorldY:
				RotateWorldY(val);
				break;
		}
	}

	public void ControlAxis1(float val)
	{
		if (controlMode == ControlMode.Rotation)
		{
			RotateAxis(RotateAxis1, val);
		}
		else if (controlMode == ControlMode.Position)
		{
			MoveAxis(MoveAxis1, val);
		}
	}

	public void ControlAxis2(float val)
	{
		if (controlMode == ControlMode.Rotation)
		{
			RotateAxis(RotateAxis2, val);
		}
		else if (controlMode == ControlMode.Position)
		{
			MoveAxis(MoveAxis2, val);
		}
	}

	public void ControlAxis3(float val)
	{
		if (controlMode == ControlMode.Rotation)
		{
			RotateAxis(RotateAxis3, val);
		}
		else if (controlMode == ControlMode.Position)
		{
			MoveAxis(MoveAxis3, val);
		}
	}

	private void Init()
	{
		if (wasInit)
		{
			return;
		}
		wasInit = true;
		if (control == null)
		{
			control = base.transform;
		}
		if ((bool)linkLineMaterial)
		{
			linkLineDrawer = new LineDrawer(linkLineMaterial);
		}
		if (useContainedMeshRenderers)
		{
			mrs = GetComponentsInChildren<MeshRenderer>();
		}
		if ((bool)material)
		{
			positionMaterialLocal = UnityEngine.Object.Instantiate(material);
			RegisterAllocatedObject(positionMaterialLocal);
			rotationMaterialLocal = UnityEngine.Object.Instantiate(material);
			RegisterAllocatedObject(rotationMaterialLocal);
			snapshotMaterialLocal = UnityEngine.Object.Instantiate(material);
			RegisterAllocatedObject(snapshotMaterialLocal);
			materialOverlay = UnityEngine.Object.Instantiate(material);
			RegisterAllocatedObject(materialOverlay);
		}
		if ((bool)followWhenOff)
		{
			_followWhenOffRB = followWhenOff.GetComponent<Rigidbody>();
		}
		kinematicRB = GetComponent<Rigidbody>();
		if (kinematicRB != null && (bool)followWhenOff)
		{
			control.position = followWhenOff.position;
			control.rotation = followWhenOff.rotation;
			ConfigurableJoint[] components = followWhenOff.GetComponents<ConfigurableJoint>();
			ConfigurableJoint[] array = components;
			foreach (ConfigurableJoint configurableJoint in array)
			{
				if (configurableJoint.connectedBody == kinematicRB)
				{
					connectedJoint = configurableJoint;
					configurableJoint.connectedAnchor = Vector3.zero;
					SetJointSprings();
					continue;
				}
				naturalJoint = configurableJoint;
				JointDrive slerpDrive = naturalJoint.slerpDrive;
				_jointRotationDriveSpring = slerpDrive.positionSpring;
				_jointRotationDriveDamper = slerpDrive.positionDamper;
				_jointRotationDriveMaxForce = slerpDrive.maximumForce;
				Vector3 eulerAngles = naturalJoint.targetRotation.eulerAngles;
				if (eulerAngles.x > 180f)
				{
					eulerAngles.x -= 360f;
				}
				else if (eulerAngles.x < -180f)
				{
					eulerAngles.x += 360f;
				}
				if (eulerAngles.y > 180f)
				{
					eulerAngles.y -= 360f;
				}
				else if (eulerAngles.y < -180f)
				{
					eulerAngles.y += 360f;
				}
				if (eulerAngles.z > 180f)
				{
					eulerAngles.z -= 360f;
				}
				else if (eulerAngles.z < -180f)
				{
					eulerAngles.z += 360f;
				}
				_jointRotationDriveXTarget = eulerAngles.x;
				_jointRotationDriveYTarget = eulerAngles.y;
				_jointRotationDriveZTarget = eulerAngles.z;
				if (naturalJoint.lowAngularXLimit.limit < naturalJoint.highAngularXLimit.limit)
				{
					_jointRotationDriveXTargetMin = naturalJoint.lowAngularXLimit.limit;
					_jointRotationDriveXTargetMax = naturalJoint.highAngularXLimit.limit;
				}
				else
				{
					_jointRotationDriveXTargetMin = naturalJoint.highAngularXLimit.limit;
					_jointRotationDriveXTargetMax = naturalJoint.lowAngularXLimit.limit;
				}
				_jointRotationDriveYTargetMin = 0f - naturalJoint.angularYLimit.limit;
				_jointRotationDriveYTargetMax = naturalJoint.angularYLimit.limit;
				_jointRotationDriveZTargetMin = 0f - naturalJoint.angularZLimit.limit;
				_jointRotationDriveZTargetMax = naturalJoint.angularZLimit.limit;
			}
		}
		startingPosition = base.transform.position;
		startingRotation = base.transform.rotation;
		startingLocalPosition = base.transform.localPosition;
		startingLocalRotation = base.transform.localRotation;
		if (stateCanBeModified)
		{
			string[] names = Enum.GetNames(typeof(PositionState));
			List<string> choicesList = new List<string>(names);
			currentPositionStateJSON = new JSONStorableStringChooser("positionState", choicesList, startingPositionState.ToString(), "Position State", SetPositionStateFromString);
			currentPositionStateJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterStringChooser(currentPositionStateJSON);
			string[] names2 = Enum.GetNames(typeof(RotationState));
			List<string> choicesList2 = new List<string>(names2);
			currentRotationStateJSON = new JSONStorableStringChooser("rotationState", choicesList2, startingRotationState.ToString(), "Rotation State", SetRotationStateFromString);
			currentRotationStateJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterStringChooser(currentRotationStateJSON);
			complyPositionThresholdJSON = new JSONStorableFloat("complyPositionThreshold", complyPositionThreshold, SyncComplyPositionThreshold, 0.0001f, 0.1f);
			complyPositionThresholdJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(complyPositionThresholdJSON);
			complyRotationThresholdJSON = new JSONStorableFloat("complyRotationThreshold", complyRotationThreshold, SyncComplyRotationThreshold, 0.1f, 30f);
			complyRotationThresholdJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(complyRotationThresholdJSON);
			complySpeedJSON = new JSONStorableFloat("complySpeed", complySpeed, SyncComplySpeed, 0f, 100f);
			complySpeedJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterFloat(complySpeedJSON);
		}
		if (controlsOn)
		{
			onJSON = new JSONStorableBool("on", _on, SyncOn);
			onJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterBool(onJSON);
			SyncOn(_on);
		}
		interactableInPlayModeJSON = new JSONStorableBool("interactableInPlayMode", _interactableInPlayMode, SyncInteractableInPlayMode);
		interactableInPlayModeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(interactableInPlayModeJSON);
		possessableJSON = new JSONStorableBool("possessable", _possessable, SyncPossessable);
		possessableJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(possessableJSON);
		canGrabPositionJSON = new JSONStorableBool("canGrabPosition", _canGrabPosition, SyncCanGrabPosition);
		canGrabPositionJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(canGrabPositionJSON);
		canGrabRotationJSON = new JSONStorableBool("canGrabRotation", _canGrabRotation, SyncCanGrabRotation);
		canGrabRotationJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(canGrabRotationJSON);
		string[] names3 = Enum.GetNames(typeof(GridMode));
		List<string> choicesList3 = new List<string>(names3);
		positionGridModeJSON = new JSONStorableStringChooser("positionGridMode", choicesList3, positionGridMode.ToString(), "Position Grid Mode", SetPositionGridModeFromString);
		positionGridModeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(positionGridModeJSON);
		rotationGridModeJSON = new JSONStorableStringChooser("rotationGridMode", choicesList3, rotationGridMode.ToString(), "Rotation Grid Mode", SetRotationGridModeFromString);
		rotationGridModeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(rotationGridModeJSON);
		positionGridJSON = new JSONStorableFloat("positionGrid", _positionGrid, SyncPositionGrid, 0.001f, 1f);
		positionGridJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(positionGridJSON);
		rotationGridJSON = new JSONStorableFloat("rotationGrid", _rotationGrid, SyncRotationGrid, 0.01f, 90f);
		rotationGridJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(rotationGridJSON);
		xLockJSON = new JSONStorableBool("xPositionLock", _xLock, SyncXLock);
		xLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(xLockJSON);
		yLockJSON = new JSONStorableBool("yPositionLock", _yLock, SyncYLock);
		yLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(yLockJSON);
		zLockJSON = new JSONStorableBool("zPositionLock", _zLock, SyncZLock);
		zLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(zLockJSON);
		xLocalLockJSON = new JSONStorableBool("xPositionLocalLock", _xLocalLock, SyncXLocalLock);
		xLocalLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(xLocalLockJSON);
		yLocalLockJSON = new JSONStorableBool("yPositionLocalLock", _yLocalLock, SyncYLocalLock);
		yLocalLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(yLocalLockJSON);
		zLocalLockJSON = new JSONStorableBool("zPositionLocalLock", _zLocalLock, SyncZLocalLock);
		zLocalLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(zLocalLockJSON);
		xRotLockJSON = new JSONStorableBool("xRotationLock", _xRotLock, SyncXRotLock);
		xRotLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(xRotLockJSON);
		yRotLockJSON = new JSONStorableBool("yRotationLock", _yRotLock, SyncYRotLock);
		yRotLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(yRotLockJSON);
		zRotLockJSON = new JSONStorableBool("zRotationLock", _zRotLock, SyncZRotLock);
		zRotLockJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(zRotLockJSON);
		if (controlsCollisionEnabled)
		{
			if (_followWhenOffRB != null)
			{
				_collisionEnabled = _followWhenOffRB.detectCollisions;
			}
			collisionEnabledJSON = new JSONStorableBool("collisionEnabled", _collisionEnabled, SyncCollisionEnabled);
			collisionEnabledJSON.storeType = JSONStorableParam.StoreType.Physical;
			RegisterBool(collisionEnabledJSON);
		}
		physicsEnabledJSON = new JSONStorableBool("physicsEnabled", physicsEnabled, SyncPhysicsEnabled);
		physicsEnabledJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(physicsEnabledJSON);
		useGravityJSON = new JSONStorableBool("useGravity", useGravityOnRBWhenOff, SyncUseGravityOnRBWhenOff);
		useGravityJSON.altName = "useGravityWhenOff";
		useGravityJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterBool(useGravityJSON);
		RBMassJSON = new JSONStorableFloat("mass", RBMass, SyncRBMass, 0.01f, 10f);
		RBMassJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBMassJSON);
		RBDragJSON = new JSONStorableFloat("drag", RBDrag, SyncRBDrag, 0f, 10f, constrain: false);
		RBDragJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBDragJSON);
		RBAngularDragJSON = new JSONStorableFloat("angularDrag", RBAngularDrag, SyncRBAngularDrag, 0f, 10f, constrain: false);
		RBAngularDragJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBAngularDragJSON);
		RBHoldPositionSpringJSON = new JSONStorableFloat("holdPositionSpring", _RBHoldPositionSpring, SyncRBHoldPositionSpring, 0f, 10000f, constrain: false);
		RBHoldPositionSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldPositionSpringJSON);
		RBHoldPositionDamperJSON = new JSONStorableFloat("holdPositionDamper", _RBHoldPositionDamper, SyncRBHoldPositionDamper, 0f, 100f, constrain: false);
		RBHoldPositionDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldPositionDamperJSON);
		RBHoldPositionMaxForceJSON = new JSONStorableFloat("holdPositionMaxForce", _RBHoldPositionMaxForce, SyncRBHoldPositionMaxForce, 0f, 10000f, constrain: false);
		RBHoldPositionMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldPositionMaxForceJSON);
		RBHoldRotationSpringJSON = new JSONStorableFloat("holdRotationSpring", _RBHoldRotationSpring, SyncRBHoldRotationSpring, 0f, 1000f, constrain: false);
		RBHoldRotationSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldRotationSpringJSON);
		RBHoldRotationDamperJSON = new JSONStorableFloat("holdRotationDamper", _RBHoldRotationDamper, SyncRBHoldRotationDamper, 0f, 10f, constrain: false);
		RBHoldRotationDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldRotationDamperJSON);
		RBHoldRotationMaxForceJSON = new JSONStorableFloat("holdRotationMaxForce", _RBHoldRotationMaxForce, SyncRBHoldRotationMaxForce, 0f, 1000f, constrain: false);
		RBHoldRotationMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBHoldRotationMaxForceJSON);
		RBComplyPositionSpringJSON = new JSONStorableFloat("complyPositionSpring", _RBComplyPositionSpring, SyncRBComplyPositionSpring, 0f, 10000f, constrain: false);
		RBComplyPositionSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyPositionSpringJSON);
		RBComplyPositionDamperJSON = new JSONStorableFloat("complyPositionDamper", _RBComplyPositionDamper, SyncRBComplyPositionDamper, 0f, 1000f, constrain: false);
		RBComplyPositionDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyPositionDamperJSON);
		RBComplyRotationSpringJSON = new JSONStorableFloat("complyRotationSpring", _RBComplyRotationSpring, SyncRBComplyRotationSpring, 0f, 1000f, constrain: false);
		RBComplyRotationSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyRotationSpringJSON);
		RBComplyRotationDamperJSON = new JSONStorableFloat("complyRotationDamper", _RBComplyRotationDamper, SyncRBComplyRotationDamper, 0f, 100f, constrain: false);
		RBComplyRotationDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyRotationDamperJSON);
		RBLinkPositionSpringJSON = new JSONStorableFloat("linkPositionSpring", _RBLinkPositionSpring, SyncRBLinkPositionSpring, 0f, 100000f, constrain: false);
		RBLinkPositionSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkPositionSpringJSON);
		RBLinkPositionDamperJSON = new JSONStorableFloat("linkPositionDamper", _RBLinkPositionDamper, SyncRBLinkPositionDamper, 0f, 1000f, constrain: false);
		RBLinkPositionDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkPositionDamperJSON);
		RBLinkPositionMaxForceJSON = new JSONStorableFloat("linkPositionMaxForce", _RBLinkPositionMaxForce, SyncRBLinkPositionMaxForce, 0f, 100000f, constrain: false);
		RBLinkPositionMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkPositionMaxForceJSON);
		RBLinkRotationSpringJSON = new JSONStorableFloat("linkRotationSpring", _RBLinkRotationSpring, SyncRBLinkRotationSpring, 0f, 100000f, constrain: false);
		RBLinkRotationSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkRotationSpringJSON);
		RBLinkRotationDamperJSON = new JSONStorableFloat("linkRotationDamper", _RBLinkRotationDamper, SyncRBLinkRotationDamper, 0f, 1000f, constrain: false);
		RBLinkRotationDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkRotationDamperJSON);
		RBLinkRotationMaxForceJSON = new JSONStorableFloat("linkRotationMaxForce", _RBLinkRotationMaxForce, SyncRBLinkRotationMaxForce, 0f, 100000f, constrain: false);
		RBLinkRotationMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBLinkRotationMaxForceJSON);
		RBComplyJointRotationDriveSpringJSON = new JSONStorableFloat("complyJointDriveSpring", _RBComplyJointRotationDriveSpring, SyncRBComplyJointRotationDriveSpring, 0f, 100f, constrain: false);
		RBComplyJointRotationDriveSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(RBComplyJointRotationDriveSpringJSON);
		jointRotationDriveSpringJSON = new JSONStorableFloat("jointDriveSpring", _jointRotationDriveSpring, SyncJointRotationDriveSpring, 0f, 200f, constrain: false);
		jointRotationDriveSpringJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveSpringJSON);
		jointRotationDriveDamperJSON = new JSONStorableFloat("jointDriveDamper", _jointRotationDriveDamper, SyncJointRotationDriveDamper, 0f, 10f, constrain: false);
		jointRotationDriveDamperJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveDamperJSON);
		jointRotationDriveMaxForceJSON = new JSONStorableFloat("jointDriveMaxForce", _jointRotationDriveMaxForce, SyncJointRotationDriveMaxForce, 0f, 100f, constrain: false);
		jointRotationDriveMaxForceJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveMaxForceJSON);
		jointRotationDriveXTargetJSON = new JSONStorableFloat("jointDriveXTarget", _jointRotationDriveXTarget, SyncJointRotationDriveXTarget, _jointRotationDriveXTargetMin, _jointRotationDriveXTargetMax);
		jointRotationDriveXTargetJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveXTargetJSON);
		jointRotationDriveYTargetJSON = new JSONStorableFloat("jointDriveYTarget", _jointRotationDriveYTarget, SyncJointRotationDriveYTarget, _jointRotationDriveYTargetMin, _jointRotationDriveYTargetMax);
		jointRotationDriveYTargetJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveYTargetJSON);
		jointRotationDriveZTargetJSON = new JSONStorableFloat("jointDriveZTarget", _jointRotationDriveZTarget, SyncJointRotationDriveZTarget, _jointRotationDriveZTargetMin, _jointRotationDriveZTargetMax);
		jointRotationDriveZTargetJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterFloat(jointRotationDriveZTargetJSON);
		initialLocalPosition = control.localPosition;
		initialLocalRotation = control.localRotation;
		_currentPositionState = startingPositionState;
		SyncPositionState();
		_currentRotationState = startingRotationState;
		SyncRotationState();
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
		}
	}

	private void OnDestroy()
	{
		if (SuperController.singleton != null && wasInit)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
		}
		DestroyAllocatedObjects();
		if (linkLineDrawer != null)
		{
			linkLineDrawer.Destroy();
		}
	}

	private void FixedUpdate()
	{
		if (_followWhenOffRB != null && _followWhenOffRB.isKinematic)
		{
			UpdateTransform(updateGUI: false);
		}
		ApplyComply();
		ApplyForce();
	}

	private void Update()
	{
		UpdateTransform(updateGUI: true);
		if (((bool)_currentPositionMesh || (bool)_currentRotationMesh) && !hidden)
		{
			if (deselectedMesh != null && !_selected && SuperController.singleton != null && SuperController.singleton.centerCameraTarget != null)
			{
				if (drawMeshWhenDeselected)
				{
					Transform transform = SuperController.singleton.centerCameraTarget.transform;
					Vector3 forward = ((!(transform.position == base.transform.position)) ? (transform.position - base.transform.position) : transform.forward);
					Vector3 up = transform.up;
					Quaternion q = Quaternion.LookRotation(forward, up);
					Vector3 s = new Vector3(deselectedMeshScale, deselectedMeshScale, deselectedMeshScale);
					Matrix4x4 identity = Matrix4x4.identity;
					identity.SetTRS(base.transform.position, q, s);
					positionMaterialLocal.SetFloat("_Alpha", targetAlpha);
					positionMaterialLocal.color = _currentPositionColor;
					Graphics.DrawMesh(deselectedMesh, identity, positionMaterialLocal, base.gameObject.layer, null, 0, null, castShadows: false, receiveShadows: false);
				}
			}
			else if (drawMesh)
			{
				Matrix4x4 localToWorldMatrix = control.localToWorldMatrix;
				Vector3 vectorFromAxis = GetVectorFromAxis(MeshForwardAxis);
				Vector3 vectorFromAxis2 = GetVectorFromAxis(MeshUpAxis);
				Quaternion quaternion = Quaternion.LookRotation(vectorFromAxis, vectorFromAxis2);
				Vector3 vectorFromAxis3 = GetVectorFromAxis(DrawForwardAxis);
				Vector3 vectorFromAxis4 = GetVectorFromAxis(DrawUpAxis);
				Quaternion quaternion2 = Quaternion.LookRotation(vectorFromAxis3, vectorFromAxis4);
				Quaternion q2 = quaternion2 * quaternion;
				float num = meshScale;
				num = (_selected ? (num * selectedScale) : ((!_highlighted) ? (num * unhighlightedScale) : (num * highlightedScale)));
				Vector3 s2 = new Vector3(num, num, num);
				Matrix4x4 identity2 = Matrix4x4.identity;
				identity2.SetTRS(Vector3.zero, q2, s2);
				Matrix4x4 matrix = localToWorldMatrix * identity2;
				if ((bool)_currentPositionMesh)
				{
					positionMaterialLocal.SetFloat("_Alpha", targetAlpha);
					positionMaterialLocal.color = _currentPositionColor;
					Graphics.DrawMesh(_currentPositionMesh, matrix, positionMaterialLocal, base.gameObject.layer, null, 0, null, castShadows: false, receiveShadows: false);
				}
				if ((bool)_currentRotationMesh)
				{
					rotationMaterialLocal.SetFloat("_Alpha", targetAlpha);
					rotationMaterialLocal.color = _currentRotationColor;
					Graphics.DrawMesh(_currentRotationMesh, matrix, rotationMaterialLocal, base.gameObject.layer, null, 0, null, castShadows: false, receiveShadows: false);
				}
				if (_selected)
				{
					materialOverlay.SetFloat("_Alpha", targetAlpha);
					materialOverlay.color = overlayColor;
					if (_controlMode == ControlMode.Position)
					{
						if ((bool)moveModeOverlayMesh)
						{
							Graphics.DrawMesh(moveModeOverlayMesh, matrix, materialOverlay, base.gameObject.layer, null, 0, null, castShadows: false, receiveShadows: false);
						}
					}
					else if (_controlMode == ControlMode.Rotation && (bool)rotateModeOverlayMesh)
					{
						Graphics.DrawMesh(rotateModeOverlayMesh, matrix, materialOverlay, base.gameObject.layer, null, 0, null, castShadows: false, receiveShadows: false);
					}
				}
			}
		}
		if (drawSnapshot)
		{
			Matrix4x4 matrix4x = snapshotMatrix;
			Vector3 vectorFromAxis5 = GetVectorFromAxis(MeshForwardAxis);
			Vector3 vectorFromAxis6 = GetVectorFromAxis(MeshUpAxis);
			Quaternion quaternion3 = Quaternion.LookRotation(vectorFromAxis5, vectorFromAxis6);
			Vector3 vectorFromAxis7 = GetVectorFromAxis(DrawForwardAxis);
			Vector3 vectorFromAxis8 = GetVectorFromAxis(DrawUpAxis);
			Quaternion quaternion4 = Quaternion.LookRotation(vectorFromAxis7, vectorFromAxis8);
			Quaternion q3 = quaternion4 * quaternion3;
			float num2 = meshScale * unhighlightedScale;
			Vector3 s3 = new Vector3(num2, num2, num2);
			Matrix4x4 identity3 = Matrix4x4.identity;
			identity3.SetTRS(Vector3.zero, q3, s3);
			Matrix4x4 matrix2 = matrix4x * identity3;
			snapshotMaterialLocal.SetFloat("_Alpha", targetAlpha);
			snapshotMaterialLocal.color = lockColor;
			if ((bool)_currentPositionMesh)
			{
				Graphics.DrawMesh(_currentPositionMesh, matrix2, snapshotMaterialLocal, base.gameObject.layer, null, 0, null, castShadows: false, receiveShadows: false);
			}
			if ((bool)_currentRotationMesh)
			{
				Graphics.DrawMesh(_currentRotationMesh, matrix2, snapshotMaterialLocal, base.gameObject.layer, null, 0, null, castShadows: false, receiveShadows: false);
			}
		}
		if (linkLineDrawer != null && _linkToRB != null && !_hidden)
		{
			ForceReceiver component = _linkToRB.GetComponent<ForceReceiver>();
			if (component == null || !component.skipUIDrawing)
			{
				linkLineMaterial.SetFloat("_Alpha", targetAlpha);
				linkLineDrawer.SetLinePoints(base.transform.position, _linkToRB.transform.position);
				linkLineDrawer.Draw(base.gameObject.layer);
			}
		}
	}
}
