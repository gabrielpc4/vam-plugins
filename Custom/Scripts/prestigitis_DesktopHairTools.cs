using System;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Animations;

/***********************************************************************************************************************************
 * prestigitis_DesktopHairTools.cs
 * 20200202
 * 
 * add this script as a session plugin to attach a hair editing tool to the mouse pointer in desktop mode.
 * known issues: window camera will not turn on automatically if it hasn't been selected before, as UI is not in the hierarchy yet.
 * 
 * This work is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License
 * https://creativecommons.org/licenses/by-nc-sa/4.0/
 *
 ***********************************************************************************************************************************/
namespace prestigitis {
	public class DesktopHairTools_20200202 : MVRScript {
/***********************************************************************************************************************************/
/* configuration settings 
/***********************************************************************************************************************************/
// hair tool
private float ToolTransparency = 0.5f; // hair tool initial transparency
protected float InitToolSize = 0.07f;  // the initial size of the tool sphere, default: 0.07f
private float ActiveToolTargetStrength = 1f; // hair tool initial strength
protected float HairToolScreenOffset = 1.0f; // tool starts this many units in front of camera when loaded, when contour follow off
protected float AdjustHairToolSizeSensitivity = 0.0004f; // mouse sensitivity when adjusting tool size
protected float AdjustHairToolStrengthSensitivity = 0.001f; // mouse sensitivity when adjusting tool strength

// contour following
protected int ContourLayerMask = (1 << 29) | (1 << 26) | (1 << 12); //collision layers for contour following mode. character layer = 29, character extremities = 26, tongue = 12
protected bool ContourFollowMode = true; // start in contour follow mode or not

// z plane
private Color ZPlaneColor = new Color(1f, 1f, 1f, 0.3f); // z plane initial color
private float AdjustZSensitivity = 0.0005f; // mouse sensitivity for adjusting z plane depth

// hold spheres
private Color HoldSphereColor = new Color(1f, 1f, 1f, 0.2f); // hold sphere initial color
private float ClearHoldSpheresDuration = 2f; // how many seconds to hold down key before hold spheres are cleared

//window cam following
private Vector3 WindowCamViewPlaneCustomLocalPosition = new Vector3(-0.0450f, 0.522f, 0.903f); // view plane position in camera following mode
private float WindowCamViewPlaneCustomLocalScale = 0.18f; // view plane scale in camera following mode
private float AdjustWindowCamXSensitivity = 0.0025f; // mouse sensitivity when adjusting window cam x position
/***********************************************************************************************************************************/

        // IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
        // some reason

        // IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
        // is called right after creation

        protected int ActiveLayerMask = 0;
        protected GameObject HairTool;
        private GPUTools.Physics.Scripts.Behaviours.GpuBrushCapsule HBrush;
        private GPUTools.Physics.Scripts.Behaviours.GpuCutCapsule HCut;
        private GPUTools.Physics.Scripts.Behaviours.GpuGrabCapsule HGrab;
        private GPUTools.Physics.Scripts.Behaviours.GpuGrowCapsule HGrow;
        private GPUTools.Physics.Scripts.Behaviours.GpuHoldCapsule HHold;
        private GPUTools.Physics.Scripts.Behaviours.GpuPullCapsule HPull;
        private GPUTools.Physics.Scripts.Behaviours.GpuPushCapsule HPush;
        private GPUTools.Physics.Scripts.Behaviours.GpuRigidityDecreaseCapsule HRDec;
        private GPUTools.Physics.Scripts.Behaviours.GpuRigidityIncreaseCapsule HRInc;
        private GPUTools.Physics.Scripts.Behaviours.GpuRigiditySetCapsule HRSet;
        private GPUTools.Physics.Scripts.Behaviours.GpuEditCapsule ActiveTool;

        // tool adjustment
        private Vector3 AdjustToolStartMousePosition = Vector3.zero;
        private Vector3 AdjustScaleStart = Vector3.zero;
        private float AdjustStrengthStart = 0f;
        private int AdjustToolState = 0; //no enums, so 0 = off, 1 = starting, 2 = adjusting

        // z depth adjustment
        protected GameObject ZPlane;
        private Vector3 ZPlaneScale = new Vector3(1000f, 1000f, 1000f);
        private int AdjustZPlaneState = 0; //no enums, so 0 = off, 1 = starting, 2 = adjusting
        private Vector3 AdjustZPlaneStartPosition = Vector3.zero;
        private Vector3 AdjustZStartMousePosition = Vector3.zero;

        // hold spheres
        private List<GameObject> HoldSpheres = new List<GameObject>();
        private float ClearHoldSpheresStartTime = 0f;

        // window camera
        private Transform WindowCamControl; //set to window camera's control node
        private int WindowCamFollowMode = 0; // 0 = off, 1 = adjusting, 2 = on
        private GameObject WindowCamLocation;
        private Vector3 WindowCamSavedPosition;
        private Quaternion WindowCamSavedRotation;
        private bool WindowCamSavedOnState = false;
        private bool WindowCamViewPlaneSavedOnState = false;
        private AimConstraint WindowCamAimConstraint;
        private float WindowCamXOffset = 1.25f;
        private Vector3 AdjustWindowCamXStartMousePosition = Vector3.zero;
        private float AdjustWindowCamXStartX = 0;

        // window cam view plane
        private Transform WindowCamViewPlane;
        private Transform WindowCamViewBG;
        private Vector3 WindowCamViewPlaneSavedScale;
        private Vector3 WindowCamViewBGSavedScale;
        private Vector3 WindowCamViewPlaneSavedLocalPosition;

        // interface
        protected JSONStorableFloat ToolTransparencyJSON;
        protected JSONStorableFloat ToolScaleJSON;
        protected JSONStorableColor ZPlaneColorJSON;
        protected JSONStorableFloat ZPlaneTransparencyJSON;
        protected JSONStorableColor HoldSphereColorJSON;
        protected JSONStorableFloat HoldSphereTransparencyJSON;
        private UIDynamicTextField UIInstructionsTextField;
        private string StatusText = "";

        protected void ToolScaleCallback(float f)
        {
            //scale tool without scaling z plane
            float cf = Mathf.Clamp(f, ToolScaleJSON.min, ToolScaleJSON.max);
            HairTool.transform.localScale = new Vector3(cf, cf, cf);
            ZPlane.transform.localScale = new Vector3(1/cf, 1/cf, 1/cf);
        }
        protected void ZPlaneColorCallback(JSONStorableColor jsc)
        {
            //change zplane color
            Color c = jsc.colorPicker.currentColor;
            ZPlaneColor = new Color(c.r, c.g, c.b, ZPlaneColor.a);
            ZPlane.GetComponent<Renderer>().material.color = ZPlaneColor;
        }
        protected void ZPlaneTransparencyCallback(float f)
        {
            ZPlaneColor.a = f;
            ZPlane.GetComponent<Renderer>().material.color = ZPlaneColor;
        }
        protected void HoldSphereColorCallback(JSONStorableColor jsc)
        {
            //change zplane color
            Color c = jsc.colorPicker.currentColor;
            HoldSphereColor = new Color(c.r, c.g, c.b, HoldSphereColor.a);
            foreach (GameObject hs in HoldSpheres)
            {
                hs.GetComponent<Renderer>().material.color = HoldSphereColor;
            }
        }
        protected void HoldSphereTransparencyCallback(float f)
        {
            HoldSphereColor.a = f;
            foreach (GameObject hs in HoldSpheres)
            {
                hs.GetComponent<Renderer>().material.color = HoldSphereColor;
            }
        }
        protected void ToolTransparencyCallback(float f)
        {
            var c = HairTool.GetComponent<Renderer>().material.color;
            ToolTransparency = f;
            HairTool.GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, ToolTransparency);
        }
        public override void Init() {
			try {
                ToolScaleJSON = new JSONStorableFloat("Tool Size", InitToolSize, ToolScaleCallback, 0.01f, 1f, false, true);
                RegisterFloat(ToolScaleJSON);
                CreateSlider(ToolScaleJSON);

                ToolTransparencyJSON = new JSONStorableFloat("Tool Transparency", ToolTransparency, ToolTransparencyCallback, 0f, 1f, true);
                RegisterFloat(ToolTransparencyJSON);
                CreateSlider(ToolTransparencyJSON);

                HSVColor zplane_hsvc = HSVColorPicker.RGBToHSV(ZPlaneColor.r, ZPlaneColor.g, ZPlaneColor.b);
                ZPlaneColorJSON = new JSONStorableColor("Z Plane Color", zplane_hsvc, ZPlaneColorCallback);
                RegisterColor(ZPlaneColorJSON);
                CreateColorPicker(ZPlaneColorJSON);

                ZPlaneTransparencyJSON = new JSONStorableFloat("Z Plane Transparency", ZPlaneColor.a, ZPlaneTransparencyCallback, 0f, 1f, true);
                RegisterFloat(ZPlaneTransparencyJSON);
                CreateSlider(ZPlaneTransparencyJSON);

                HSVColor hsphere_hsvc = HSVColorPicker.RGBToHSV(HoldSphereColor.r, HoldSphereColor.g, HoldSphereColor.b);
                HoldSphereColorJSON = new JSONStorableColor("Hold Sphere Color", hsphere_hsvc, HoldSphereColorCallback);
                RegisterColor(HoldSphereColorJSON);
                CreateColorPicker(HoldSphereColorJSON);

                HoldSphereTransparencyJSON = new JSONStorableFloat("Hold Sphere Transparency", HoldSphereColor.a, HoldSphereTransparencyCallback, 0f, 1f, true);
                RegisterFloat(HoldSphereTransparencyJSON);
                CreateSlider(HoldSphereTransparencyJSON);

                var UIInstructions = new StringBuilder();

                UIInstructions.AppendLine("[prestigitis] Desktop Hair Tools 20200202");
                UIInstructions.AppendLine();
                UIInstructions.AppendLine("Numpad Tool Bindings:");
                UIInstructions.AppendLine("  1 Brush");
                UIInstructions.AppendLine("  2 Pull");
                UIInstructions.AppendLine("  3 Grab");
                UIInstructions.AppendLine("  4 Push");
                UIInstructions.AppendLine("  5 Cut");
                UIInstructions.AppendLine("  6 Grow");
                UIInstructions.AppendLine("  7 Decrease Rigidity");
                UIInstructions.AppendLine("  8 Set Rigidity");
                UIInstructions.AppendLine("  9 Increase Rigidity");
                UIInstructions.AppendLine();
                UIInstructions.AppendLine("Hold tool key 0-9 and mouse");
                UIInstructions.AppendLine("UP/DOWN to change size.");
                UIInstructions.AppendLine("LEFT/RIGHT to change strength.");
                UIInstructions.AppendLine();
                UIInstructions.AppendLine("Hold Numpad 0 to adjust Z plane depth.");
                UIInstructions.AppendLine("Numpad . to toggle contour following.");
                UIInstructions.AppendLine();
                UIInstructions.AppendLine("Numpad + to spawn a hold sphere.");
                UIInstructions.AppendLine("Hold + to clear hold spheres.");
                UIInstructions.AppendLine();
                UIInstructions.AppendLine("Numpad - to toggle WindowCam following.");
                UIInstructions.AppendLine("When following, hold Enter and mouse LEFT/RIGHT to adjust WindowCam offset.");
                UIInstructionsTextField = CreateTextField(new JSONStorableString("info", UIInstructions.ToString()), true);
                UIInstructionsTextField.height = 1200;
                UIInstructionsTextField.UItext.fontSize = 32;
            }
            catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Start is called once before Update or FixedUpdate is called and after Init()
		void Start() {
            try
            {
                HairTool = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                HairTool.transform.localScale = new Vector3(ToolScaleJSON.val, ToolScaleJSON.val, ToolScaleJSON.val);
                HairTool.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
                HairTool.GetComponent<Renderer>().material.color = new Color(1f, 0f, 0f, ToolTransparency);
                HairTool.GetComponent<Renderer>().enabled = true;
                HairTool.GetComponent<Collider>().enabled = false;

                var cc = HairTool.gameObject.AddComponent<CapsuleCollider>();
                cc.enabled = false;
                cc.radius = 0.5f;
                cc.height = 0f;

                HBrush = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuBrushCapsule>();
                HCut = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuCutCapsule>();
                HGrab = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuGrabCapsule>();
                HGrow = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuGrowCapsule>();
                HHold = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuHoldCapsule>();
                HPull = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuPullCapsule>();
                HPush = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuPushCapsule>();
                HRDec = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuRigidityDecreaseCapsule>();
                HRInc = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuRigidityIncreaseCapsule>();
                HRSet = HairTool.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuRigiditySetCapsule>();

                HBrush.enabled = false;
                HCut.enabled = false;
                HGrab.enabled = false;
                HGrow.enabled = false;
                HHold.enabled = false;
                HPull.enabled = false;
                HPush.enabled = false;
                HRDec.enabled = false;
                HRInc.enabled = false;
                HRSet.enabled = false;

                HBrush.capsuleCollider = cc;
                HCut.capsuleCollider = cc;
                HGrab.capsuleCollider = cc;
                HGrow.capsuleCollider = cc;
                HHold.capsuleCollider = cc;
                HPull.capsuleCollider = cc;
                HPush.capsuleCollider = cc;
                HRDec.capsuleCollider = cc;
                HRInc.capsuleCollider = cc;
                HRSet.capsuleCollider = cc;

                HBrush.strength = 1f;
                HCut.strength = 1f;
                HGrab.strength = 1f;
                HGrow.strength = 1f;
                HHold.strength = 1f;
                HPull.strength = 1f;
                HPush.strength = 1f;
                HRDec.strength = 1f;
                HRInc.strength = 1f;
                HRSet.strength = 1f;

                ActiveTool = HBrush;
                ActiveTool.name = "Brush";

                ZPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ZPlane.transform.parent = HairTool.transform;
                ZPlane.transform.localScale = ZPlaneScale;
                ZPlane.transform.localRotation = Quaternion.AngleAxis(-90f, Vector3.left);
                ZPlane.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
                ZPlane.GetComponent<Renderer>().material.color = ZPlaneColor;
                ZPlane.GetComponent<Renderer>().enabled = true;
                ZPlane.GetComponent<Collider>().enabled = false;
                ActiveLayerMask = ContourLayerMask;
                HairTool.transform.position = SuperController.singleton.MonitorCenterCamera.ScreenToWorldPoint(new Vector3(0f, 0f, HairToolScreenOffset));

                WindowCamControl = SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("reParentObject/control");
                WindowCamLocation = new GameObject("WindowCamLocation");
                WindowCamLocation.transform.parent = HairTool.transform;
                WindowCamLocation.transform.localPosition = new Vector3(WindowCamXOffset / HairTool.transform.localScale.x, 0f, -0.005f / HairTool.transform.localScale.z);
                WindowCamLocation.transform.localRotation = Quaternion.AngleAxis(-90f, Vector3.up);

                WindowCamViewPlane = SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("reParentObject/object/rescaleObject/CameraGroup/CameraView/PlaneForCamera").transform;
                WindowCamViewBG = SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("reParentObject/object/rescaleObject/CameraGroup/CameraView/Background").transform;

                WindowCamControl.gameObject.AddComponent<AimConstraint>();
                WindowCamAimConstraint = WindowCamControl.gameObject.GetComponent<AimConstraint>();
                ConstraintSource cs = new ConstraintSource
                {
                    weight = 1,
                    sourceTransform = HairTool.transform
                };
                WindowCamAimConstraint.AddSource(cs);
                WindowCamAimConstraint.aimVector.Set(0, 0, 1);
                WindowCamAimConstraint.upVector.Set(0, 1, 0);
                WindowCamAimConstraint.worldUpType = AimConstraint.WorldUpType.None;
                WindowCamAimConstraint.weight = 1;
                WindowCamAimConstraint.rotationAxis = Axis.X | Axis.Y; //change this to constrain axis
                WindowCamAimConstraint.constraintActive = false;
            }
            catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

        // Update is called with each rendered frame by Unity
        void Update()
        {
            try
            {
                HairTool.transform.LookAt(SuperController.singleton.MonitorCenterCamera.transform);
                if (Input.GetMouseButtonDown(0))
                {
                    HairTool.GetComponent<Renderer>().enabled = true;
                    ActiveTool.enabled = true;
                    if (ActiveTool != HRDec && ActiveTool != HRSet && ActiveTool != HRInc) //do not ramp up for rigidity painting tools
                    {
                        InvokeRepeating("RampUp", 0f, 0.00002f);
                    }
                    else
                    {
                        ActiveTool.strength = ActiveToolTargetStrength;
                        HairTool.GetComponent<Renderer>().material.color = new Color(0f, 0f, ActiveTool.strength, ToolTransparency);
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    ActiveTool.enabled = false;
                    ActiveTool.strength = 0f;
                    CancelInvoke("RampUp");
                    HairTool.GetComponent<Collider>().enabled = false;
                    HairTool.GetComponent<Renderer>().material.color = new Color(ActiveToolTargetStrength, 0f, 0f, ToolTransparency);
                }
                switch (AdjustToolState)
                {
                    case 0: //normal operation, no adjustment
                        if (Input.GetKeyDown(KeyCode.Keypad1))
                        {
                            ActiveTool = HBrush;
                            ActiveTool.name = "Brush";
                            AdjustToolState = 1;
                        }
                        if (Input.GetKeyDown(KeyCode.Keypad2))
                        {
                            ActiveTool = HPull;
                            ActiveTool.name = "Pull";
                            AdjustToolState = 1;
                        }
                        if (Input.GetKeyDown(KeyCode.Keypad3))
                        {
                            ActiveTool = HGrab;
                            ActiveTool.name = "Grab";
                            AdjustToolState = 1;
                        }
                        if (Input.GetKeyDown(KeyCode.Keypad4))
                        {
                            ActiveTool = HPush;
                            ActiveTool.name = "Push";
                            AdjustToolState = 1;
                        }
                        if (Input.GetKeyDown(KeyCode.Keypad5))
                        {
                            ActiveTool = HCut;
                            ActiveTool.name = "Cut";
                            AdjustToolState = 1;
                        }
                        if (Input.GetKeyDown(KeyCode.Keypad6))
                        {
                            ActiveTool = HGrow;
                            ActiveTool.name = "Grow";
                            AdjustToolState = 1;
                        }
                        if (Input.GetKeyDown(KeyCode.Keypad7))
                        {
                            ActiveTool = HRDec;
                            ActiveTool.name = "Dec. Rigidity";
                            AdjustToolState = 1;
                        }
                        if (Input.GetKeyDown(KeyCode.Keypad8))
                        {
                            ActiveTool = HRSet;
                            ActiveTool.name = "Set Rigidity";
                            AdjustToolState = 1;
                        }
                        if (Input.GetKeyDown(KeyCode.Keypad9))
                        {
                            ActiveTool = HRInc;
                            ActiveTool.name = "Inc. Rigidity";
                            AdjustToolState = 1;
                        }
                        if (Input.GetKey(KeyCode.KeypadPlus))
                        {
                            if (Input.GetKeyDown(KeyCode.KeypadPlus))
                            {
                                SpawnHoldSphere(ActiveTool);
                                ClearHoldSpheresStartTime = Time.time;
                            }
                            if ((ClearHoldSpheresStartTime + ClearHoldSpheresDuration) <= Time.time) ClearHoldSpheres();
                        }
                        break;
                    case 1: //started adjusting
                        AdjustToolStartMousePosition = Input.mousePosition;
                        AdjustScaleStart = ActiveTool.transform.localScale;
                        AdjustStrengthStart = ActiveToolTargetStrength;
                        ActiveTool.transform.GetComponent<Renderer>().enabled = true;
                        AdjustToolState = 2;
                        break;
                    case 2: //during adjustment, update tool continuously
                        float mouseDiffy = (Input.mousePosition.y - AdjustToolStartMousePosition.y) * AdjustHairToolSizeSensitivity;
                        float mouseDiffx = (Input.mousePosition.x - AdjustToolStartMousePosition.x) * AdjustHairToolStrengthSensitivity;

                        ToolScaleJSON.val = Mathf.Clamp(AdjustScaleStart.x + mouseDiffy, ToolScaleJSON.min, ToolScaleJSON.max);

                        ActiveToolTargetStrength = Mathf.Clamp(AdjustStrengthStart + mouseDiffx, 0f, 1f);
                        HairTool.GetComponent<Renderer>().material.color = new Color(0f, 0f, ActiveToolTargetStrength, ToolTransparency);

                        HairTool.transform.position = SuperController.singleton.MonitorCenterCamera.ScreenToWorldPoint(new Vector3(AdjustToolStartMousePosition.x, AdjustToolStartMousePosition.y, HairToolScreenOffset));
                        //if key up, set adjustmode to 0
                        if (Input.GetKeyUp(KeyCode.Keypad1) ||
                            Input.GetKeyUp(KeyCode.Keypad2) ||
                            Input.GetKeyUp(KeyCode.Keypad3) ||
                            Input.GetKeyUp(KeyCode.Keypad4) ||
                            Input.GetKeyUp(KeyCode.Keypad5) ||
                            Input.GetKeyUp(KeyCode.Keypad6) ||
                            Input.GetKeyUp(KeyCode.Keypad7) ||
                            Input.GetKeyUp(KeyCode.Keypad8) ||
                            Input.GetKeyUp(KeyCode.Keypad9))
                        {
                            HairTool.GetComponent<Renderer>().material.color = new Color( ActiveToolTargetStrength, 0f, 0f, ToolTransparency);
                            AdjustToolState = 0;
                        }
                        break;
                }
                                
                //toggle contour follow mode
                if (Input.GetKeyDown(KeyCode.KeypadPeriod)) ContourFollowMode = !ContourFollowMode;

                if (ContourFollowMode)
                {
                    if (Input.GetKeyDown(KeyCode.Keypad0))
                    {
                        ContourFollowMode = false;
                        AdjustZPlaneState = 1;
                    }
                    else
                    {
                        RaycastHit hit = new RaycastHit();
                        Ray r = SuperController.singleton.MonitorCenterCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(r, out hit, Mathf.Infinity, ActiveLayerMask))
                        {
                            if (AdjustToolState == 0) HairTool.transform.position = hit.point;
                            HairToolScreenOffset = SuperController.singleton.MonitorCenterCamera.WorldToScreenPoint(HairTool.transform.position).z;
                        }
                        else
                        {
                            if (AdjustToolState == 0) HairTool.transform.position = SuperController.singleton.MonitorCenterCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, HairToolScreenOffset));
                        }
                    }
                }
                else
                {
                    switch (AdjustZPlaneState)
                    {
                        case 0:
                            if (Input.GetKeyDown(KeyCode.Keypad0))
                            {
                                AdjustZPlaneState = 1; //start zplane adjust mode
                            }
                            if (AdjustToolState == 0) HairTool.transform.position = SuperController.singleton.MonitorCenterCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, HairToolScreenOffset));
                            break;
                        case 1:
                            //save hairtool depth
                            AdjustZPlaneStartPosition = SuperController.singleton.MonitorCenterCamera.WorldToScreenPoint(HairTool.transform.position);
                            //save mouse screen position
                            AdjustZStartMousePosition = Input.mousePosition;
                            AdjustZPlaneState = 2;
                            break;
                        case 2:
                            //adjust hairtool and zplane depth (screen z) based on mouse up/down
                            float mouseDiffy = (Input.mousePosition.y - AdjustZStartMousePosition.y) * AdjustZSensitivity;
                            HairToolScreenOffset = AdjustZPlaneStartPosition.z + mouseDiffy;
                            if (Input.GetKeyUp(KeyCode.Keypad0))
                            {
                                //end zplane adjust mode
                                AdjustZPlaneState = 0;
                            }
                            if (AdjustToolState == 0) HairTool.transform.position = SuperController.singleton.MonitorCenterCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, AdjustZStartMousePosition.y, HairToolScreenOffset));
                            break;
                    }
                }
                switch (WindowCamFollowMode)
                {
                    case 0: //follow mode off
                        if (Input.GetKeyDown(KeyCode.KeypadMinus))
                        {
                            WindowCamFollowMode = 2;
                            //save windowcam position and rotation
                            WindowCamSavedPosition = WindowCamControl.transform.position;
                            WindowCamSavedRotation = WindowCamControl.transform.rotation;
                            if (SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("UIPlaceHolder/UIAtomCamera(Clone)/Canvas/Panel/Content/Camera/DynamicToggle") != null)
                            {
                                WindowCamSavedOnState = GetWindowCamToggle(0).GetComponent<UIDynamicToggle>().toggle.isOn;
                                WindowCamViewPlaneSavedOnState = GetWindowCamToggle(1).GetComponent<UIDynamicToggle>().toggle.isOn;
                            }
                            WindowCamAimConstraint.constraintActive = true;

							WindowCamViewPlaneSavedScale = WindowCamViewPlane.localScale;
                            WindowCamViewBGSavedScale = WindowCamViewBG.localScale;
                            WindowCamViewPlaneSavedLocalPosition = WindowCamViewPlane.localPosition;
                            WindowCamViewPlane.localPosition = WindowCamViewPlaneCustomLocalPosition;
                            WindowCamViewBG.localPosition = WindowCamViewPlaneCustomLocalPosition + new Vector3(0f, 0f, -0.0001f);
                            WindowCamViewPlane.localScale = WindowCamViewPlaneSavedScale * WindowCamViewPlaneCustomLocalScale;
                            WindowCamViewBG.localScale = WindowCamViewBGSavedScale * WindowCamViewPlaneCustomLocalScale;
						}
                        break;
                    case 1: //adjusting
                        //calcaulate new offset based on mouse diff from saved mouse position
                        var mouseDiffX = (AdjustWindowCamXStartMousePosition.x - Input.mousePosition.x) * AdjustWindowCamXSensitivity;
                        HairTool.transform.position = SuperController.singleton.MonitorCenterCamera.ScreenToWorldPoint(new Vector3(AdjustWindowCamXStartMousePosition.x, AdjustWindowCamXStartMousePosition.y, HairToolScreenOffset));
                        WindowCamXOffset = (AdjustWindowCamXStartX + mouseDiffX);
                        WindowCamLocation.transform.localPosition = new Vector3(WindowCamXOffset / HairTool.transform.localScale.x, 0f, -0.05f / HairTool.transform.localScale.z); //adjust windowcamlocation based on new offset
                        WindowCamControl.transform.position = WindowCamLocation.transform.position; //move windowcam to new adjusted location
                        if (Input.GetKeyUp(KeyCode.KeypadEnter))
                        {
                            WindowCamFollowMode = 2;
                        }
                        break;
                    case 2: //on
                        //turn on windowcam
                        if (SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("UIPlaceHolder/UIAtomCamera(Clone)/Canvas/Panel/Content/Camera/DynamicToggle") != null)
                        {
                            GetWindowCamToggle(0).GetComponent<UIDynamicToggle>().toggle.isOn = true;
                            GetWindowCamToggle(1).GetComponent<UIDynamicToggle>().toggle.isOn = true;

                        }
                        WindowCamLocation.transform.localPosition = new Vector3(WindowCamXOffset / HairTool.transform.localScale.x, 0f, -0.05f / HairTool.transform.localScale.z); //adjust windowcam location based on new offset
                        WindowCamControl.transform.position = WindowCamLocation.transform.position;
                        if (Input.GetKeyDown(KeyCode.KeypadEnter))
                        {
                            WindowCamFollowMode = 1;
                            AdjustWindowCamXStartMousePosition = Input.mousePosition; //save mouse position
                            AdjustWindowCamXStartX = WindowCamXOffset;
                        }
                        if (Input.GetKeyDown(KeyCode.KeypadMinus))
                        {
                            WindowCamFollowMode = 0;
                            WindowCamAimConstraint.constraintActive = false;
                            //restore windowcam saved position and rotation
                            WindowCamControl.transform.position = WindowCamSavedPosition;
                            WindowCamControl.transform.rotation = WindowCamSavedRotation;
                            if (SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("UIPlaceHolder/UIAtomCamera(Clone)/Canvas/Panel/Content/Camera/DynamicToggle") != null)
                            {
                                GetWindowCamToggle(0).GetComponent<UIDynamicToggle>().toggle.isOn = WindowCamSavedOnState;
                                GetWindowCamToggle(1).GetComponent<UIDynamicToggle>().toggle.isOn = WindowCamViewPlaneSavedOnState;
                            }
                            WindowCamViewPlane.localScale = WindowCamViewPlaneSavedScale;
                            WindowCamViewBG.localScale = WindowCamViewBGSavedScale;
                            WindowCamViewPlane.localPosition = WindowCamViewPlaneSavedLocalPosition;
                            WindowCamViewBG.localPosition = WindowCamViewPlaneSavedLocalPosition + new Vector3(0f, 0f, -0.001f);
                        }
                        break;
                }
                StatusText = "Scale: " + ActiveTool.transform.localScale.x.ToString("F3") + " | Strength: " + ActiveToolTargetStrength.ToString("F3") + "\nZ Depth: " + HairToolScreenOffset.ToString("F2") + " | Follow Contours: " + ContourFollowMode.ToString();
                OutputStatusText(ActiveTool.name, StatusText);
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // FixedUpdate is called with each physics simulation frame by Unity
        void FixedUpdate() {
			try {
                // put code in here
            }
            catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
            }
        }

		// OnDestroy is where you should put any cleanup
		// if you registered objects to supercontroller or atom, you should unregister them here
		void OnDestroy() {
            Destroy(ZPlane);
            Destroy(HairTool);
            ClearHoldSpheres();
            ResetWindowCamViewPlane();
            WindowCamAimConstraint.constraintActive = false;
            if (SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("UIPlaceHolder/UIAtomCamera(Clone)/Canvas/Panel/Content/Camera/DynamicToggle") != null)
            {
                GetWindowCamToggle(0).GetComponent<UIDynamicToggle>().toggle.isOn = WindowCamSavedOnState;
                GetWindowCamToggle(1).GetComponent<UIDynamicToggle>().toggle.isOn = WindowCamViewPlaneSavedOnState;
            }
            ResetStatusText();
        }
        public void OnDisable()
        {
            DisableHoldSpheres();
            HairTool.GetComponent<Renderer>().material.color = new Color(ActiveToolTargetStrength, 0f, 0f, ToolTransparency);
            HairTool.gameObject.SetActive(false);
            WindowCamFollowMode = 0;
            WindowCamAimConstraint.constraintActive = false;
            ResetWindowCamViewPlane();
            if (SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("UIPlaceHolder/UIAtomCamera(Clone)/Canvas/Panel/Content/Camera/DynamicToggle") != null)
            {
                GetWindowCamToggle(0).GetComponent<UIDynamicToggle>().toggle.isOn = WindowCamSavedOnState;
                GetWindowCamToggle(1).GetComponent<UIDynamicToggle>().toggle.isOn = WindowCamViewPlaneSavedOnState;
            }
            ResetStatusText();
        }
        public void OnEnable()
        {
            EnableHoldSpheres();
            HairTool.transform.position = SuperController.singleton.MonitorCenterCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, HairToolScreenOffset));
            HairTool.gameObject.SetActive(true);
            ActiveTool.enabled = false;
        }
        void RampUp()
        {
            if (ActiveTool.strength < ActiveToolTargetStrength)
            {
                ActiveTool.strength = Mathf.Clamp(ActiveTool.strength * 1.2f + 0.0001f, 0f, 1f);
                HairTool.GetComponent<Renderer>().material.color = new Color(0f, 0f, ActiveTool.strength, ToolTransparency);
            }
            else
                CancelInvoke("RampUp");
        }
        void OutputStatusText(string s1, string s2)
        {
            var t1 = SuperController.singleton.MonitorModeAuxUI.transform.Find("HighlightText1").GetComponent<UnityEngine.UI.Text>();
            var t2 = SuperController.singleton.MonitorModeAuxUI.transform.Find("HighlightText2").GetComponent<UnityEngine.UI.Text>();

            t1.color = new Color(0.0f, 0.0f, 0.5f);
            t1.fontSize = 40;
            t1.resizeTextForBestFit = true;
            t1.alignment = TextAnchor.MiddleCenter;
            t1.text = s1;

            t2.color = new Color(0.0f, 0.0f, 0.5f);
            t2.fontSize = 25;
            t2.resizeTextForBestFit = true;
            t2.text = s2;
            t2.gameObject.SetActive(true);
        }
        void ResetStatusText()
        {
            //reset to VAM defaults
            var t1 = SuperController.singleton.MonitorModeAuxUI.transform.Find("HighlightText1").GetComponent<UnityEngine.UI.Text>();
            var t2 = SuperController.singleton.MonitorModeAuxUI.transform.Find("HighlightText2").GetComponent<UnityEngine.UI.Text>();

            t1.color = new Color(0.199f, 0.199f, 0.199f, 1.0f);
            t1.fontSize = 28;
            t1.resizeTextForBestFit = false;
            t1.alignment = TextAnchor.MiddleRight;
            t1.text = "Hightlighted:\n(C)To Cycle Stack";

            t2.color = new Color(0.199f, 0.199f, 0.199f, 1.0f);
            t2.fontSize = 28;
            t2.resizeTextForBestFit = false;
            t2.text = "";
        }
        void SpawnHoldSphere(GPUTools.Physics.Scripts.Behaviours.GpuEditCapsule tool) 
        {
            //spawn a new hold sphere at hairtool location 
            GameObject hs = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hs.transform.localScale = tool.transform.localScale;
            hs.transform.position = tool.transform.position;
            hs.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
            hs.GetComponent<Renderer>().material.color = HoldSphereColor;
            hs.GetComponent<Renderer>().enabled = true;
            hs.GetComponent<Collider>().enabled = false;

            var cc = HairTool.gameObject.AddComponent<CapsuleCollider>();
            cc.enabled = false;
            cc.radius = 0.5f;
            cc.height = 0f;

            var hc = hs.gameObject.AddComponent<GPUTools.Physics.Scripts.Behaviours.GpuHoldCapsule>();
            hc.capsuleCollider = cc;
            hc.strength = ActiveToolTargetStrength;
            HoldSpheres.Add(hs);

            hc.enabled = true;
        }
        void ClearHoldSpheres()
        {
            foreach(GameObject hs in HoldSpheres)
            {
                hs.GetComponent<GPUTools.Physics.Scripts.Behaviours.GpuHoldCapsule>().enabled = false;
                Destroy(hs);
            }
            HoldSpheres = new List<GameObject>();
        }
        void DisableHoldSpheres()
        {
            foreach (GameObject hs in HoldSpheres)
            {
                hs.SetActive(false);
            }
        }
        void EnableHoldSpheres()
        {
            foreach (GameObject hs in HoldSpheres)
            {
                hs.SetActive(true);
            }
        }
        void ResetWindowCamViewPlane()
        {
            //reset to VAM defaults
            WindowCamViewPlane.localPosition = new Vector3(-0.2f, 0.23f, 0.00f);
            WindowCamViewBG.localPosition = WindowCamViewPlane.localPosition + new Vector3(0f, 0f, -0.002f);
            WindowCamViewPlane.localScale = new Vector3(0.02222f, 0.0125f, 0.0125f);
            WindowCamViewBG.localScale = new Vector3(0.02322f, 0.0135f, 0.0135f);
        }
        private UIDynamicToggle GetWindowCamToggle(int i)
        {
            if (SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("UIPlaceHolder/UIAtomCamera(Clone)/Canvas/Panel/Content/Camera"))
            {
                Transform[] WindowCamUIToggles = SuperController.singleton.GetAtomByUid("WindowCamera").gameObject.transform.Find("UIPlaceHolder/UIAtomCamera(Clone)/Canvas/Panel/Content/Camera").transform.GetComponentsInChildren<Transform>().Where(t => t.name == "DynamicToggle").ToArray<Transform>();
                return WindowCamUIToggles[i].GetComponent<UIDynamicToggle>();
            }
            else return null;
        }
    }
}