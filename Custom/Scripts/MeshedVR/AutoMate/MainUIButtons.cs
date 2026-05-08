using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    //Adapted from VAMDeluxe Dollmaster UI
    public class MainUIButtons
    {
		private FreeControllerV3[] jointControls;
		Atom currentAtom;
		static string head = "headControl";
        static string chest = "chestControl";
        static string hip = "hipControl";
        static string lFoot = "lFootControl";
        static string rFoot = "rFootControl";
        static string lKnee = "lKneeControl";
        static string rKnee = "rKneeControl";
        static string lArm = "lArmControl";
        static string rArm = "rArmControl";
		static string lShoulder = "lShoulderControl";
        static string rShoulder= "rShoulderControl";
        static string lElbow = "lElbowControl";
        static string rElbow = "rElbowControl";
        static string lHand = "lHandControl";
        static string rHand = "rHandControl";
        static string lThigh = "lThighControl";
        static string rThigh = "rThighControl";
		static string neck = "neckControl";
		static string abdomen = "abdomenControl";
		static string abdomen2 = "abdomen2Control";
		static string pelvis = "pelvisControl";
		static string penisTip = "penisTipControl";
		static string penisMid = "penisMidControl";
		static string penisBase = "penisBaseControl";
		static string testicles = "testesControl";
		
		FreeControllerV3 headControl;
        FreeControllerV3 chestControl;
        FreeControllerV3 hipControl;
        FreeControllerV3 lFootControl;
        FreeControllerV3 rFootControl;
        FreeControllerV3 lKneeControl;
        FreeControllerV3 rKneeControl;
        FreeControllerV3 lArmControl;
        FreeControllerV3 rArmControl;
		FreeControllerV3 lShoulderControl;
        FreeControllerV3 rShoulderControl;
        FreeControllerV3 lElbowControl;
        FreeControllerV3 rElbowControl;
        FreeControllerV3 lHandControl;
        FreeControllerV3 rHandControl;
        FreeControllerV3 lThighControl;
        FreeControllerV3 rThighControl;
		FreeControllerV3 neckControl;
		FreeControllerV3 abdomenControl;
		FreeControllerV3 abdomen2Control;
		FreeControllerV3 pelvisControl;
		FreeControllerV3 penisTipControl;
		FreeControllerV3 penisMidControl;
		FreeControllerV3 penisBaseControl;
		FreeControllerV3 testesControl;
		
		string[] notInteractableJoints = { abdomen, abdomen2, pelvis, hip };
		string[] possessableJoints = { };
		string[] onJoints = {};
		string[] complyJoints =  { head, chest, hip, lThigh, rThigh, lKnee, rKnee, lFoot, rFoot, lElbow, rElbow, lHand, rHand };		

        MVRScript plugin;

        private Camera _mainCamera;
        public static Canvas canvas = null;
        private float UIScale = 1.0f;

        UIDynamicButton loadNowButton = null;
        public UIDynamicButton autoLoadButton = null;
        UIDynamicButton cycleSetButton = null;
        UIDynamicButton cycleSet2Button = null;
        UIDynamicButton createMaleButton = null;
		UIDynamicButton releaseJointsButton = null;
		UIDynamicButton complyJointsButton = null;
		UIDynamicButton dollifyButton = null;
		UIDynamicButton resetFemalesButton = null;
		UIDynamicButton resetMalesButton = null;
        UIDynamicButton maleImprovedPOVLickingButton = null;

        public bool wantToChangeAutoLoadType = false;
        public bool wantToLoadNow = false;
        public bool wantsToCycleSet = false;
        public bool wantsToCycleSet2 = false;
        public bool wantsToCreateMaleAtom = false;
        public bool wantsToRemoveMaleAtom = false;
        public bool wantsToStartLicking = false;
        public bool wantsToStopLicking = false;
        public bool createdMale = false;
        public bool isLicking = false;

        private bool isDesktopMode = false;

        public string cycleSetText = "";
        public string cycleSet2Text = "";

        public int autoLoadType = 0;

        public void Init(MVRScript _plugin)
        {
            plugin = _plugin;
            _mainCamera = CameraTarget.centerTarget?.targetCamera;
            isDesktopMode = !(SuperController.singleton.isOVR || SuperController.singleton.isOpenVR);
        }

        public void Start()
        {
            Cleanup();
            float worldScale = SuperController.singleton.worldScale;
            SuperController.singleton.worldScale = 1.0f;
            CreateButtons();
            SuperController.singleton.worldScale = worldScale;
        }

        public void ShowUI(bool setToActive)
        {
            if (loadNowButton != null) { 
                loadNowButton.gameObject.SetActive(setToActive);
                autoLoadButton.gameObject.SetActive(setToActive);
                cycleSetButton.gameObject.SetActive(setToActive);
                cycleSet2Button.gameObject.SetActive(setToActive);
                createMaleButton.gameObject.SetActive(setToActive);
				releaseJointsButton.gameObject.SetActive(setToActive);
				complyJointsButton.gameObject.SetActive(setToActive);
				dollifyButton.gameObject.SetActive(setToActive);
				resetFemalesButton.gameObject.SetActive(setToActive);
				resetMalesButton.gameObject.SetActive(setToActive);
                maleImprovedPOVLickingButton.gameObject.SetActive(createdMale);
            }
        }


        public void Cleanup()
        {
            if (canvas != null)
            {
                if (SuperController.singleton != null)
                {
                    SuperController.singleton.RemoveCanvas(canvas);
                }

                canvas.transform.SetParent(null, false);

                if (canvas.gameObject != null)
                {
                    GameObject.Destroy(canvas.gameObject);
                }
            }
        }
		
		JSONClass CreatePluginJSON(string[] pluginList)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{");
            sb.Append(" \"id\": \"" + "PluginManager" + "\",");
            sb.Append(" \"plugins\": " + "{");
            for (int i = 0; i < pluginList.Length; i++)
            {
                sb.Append(string.Format("    \"plugin#{0}\": \"{1}\"{2}", i.ToString(), pluginList[i], ((i + 1) < pluginList.Length ? "," : "")));
            }
            sb.Append("  }");
            sb.Append("}");
            return JSONNode.Parse(sb.ToString()).AsObject;
        }

        public void CreateButtons()
        {
            float scale = 0.001f;

            Cleanup();

            GameObject canvasObject = new GameObject();
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.pixelPerfect = false;
            SuperController.singleton.AddCanvas(canvas);

            canvas.transform.SetParent(SuperController.singleton.mainHUD, false);

            CanvasScaler cs = canvasObject.AddComponent<CanvasScaler>();
            cs.scaleFactor = 80.0f;
            cs.dynamicPixelsPerUnit = 1f;

            GraphicRaycaster gr = canvasObject.AddComponent<GraphicRaycaster>();

            canvas.transform.localScale = new Vector3(scale, scale, scale);
            //canvas.transform.localPosition = new Vector3(-0.7f, 0, 0);

            canvas.transform.localPosition = new Vector3(-.45f, -0.72f, 0.35f);

            LookAtCamera();

            loadNowButton = AddButton("Load Plugins Now", () =>
            {
                wantToLoadNow = true;
            }, 1, -1);

            autoLoadButton = AddButton("Auto Load Disabled", () =>
            {
                wantToChangeAutoLoadType = true;
             }, 1, 2);

            cycleSetButton = AddButton("Cycle Set 1", () =>
            {
                wantsToCycleSet = true;
            }, 1, 0);

            cycleSet2Button = AddButton("Cycle Set 2", () =>
            {
                wantsToCycleSet2 = true;
            }, 1, 1);
            
            createMaleButton = AddButton("Create Male", () =>
            {
                if (createdMale)
                {
                    wantsToRemoveMaleAtom = true;
                    createdMale = false;
                    isLicking = false;
                    SetButtonText();
                }
                else
                {
                    wantsToCreateMaleAtom = true;
                    createdMale = true;
                    isLicking = false;
                    SetButtonText();
                }
            }, 2, -1);
			
			releaseJointsButton = AddButton("Release Joints", () =>
            {
                // TODO
				IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

				foreach (Atom at in personAtoms)
				{
					currentAtom = at;
					bool isFemale = !at.GetComponentInChildren<DAZCharacter>().isMale;
					
					if (isFemale) 
					{				
						 possessableJoints = new string[] {};
						 onJoints = new string[] {};
						 complyJoints = new string[] { head, abdomen, lKnee, rKnee };	
						 
						 jointControls = currentAtom.GetComponentsInChildren<FreeControllerV3>(true);
						 configureJoints();
			             linkJoint(abdomen, abdomen2);								
					}
				}               
            }, 2, 0);
			
			dollifyButton = AddButton("Dollify", () =>
            {
                // TODO
				IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

				foreach (Atom at in personAtoms)
				{
					bool isFemale = !at.GetComponentInChildren<DAZCharacter>().isMale;
					
					if (isFemale) 
					{	 
						currentAtom = at;
						possessableJoints = new string[] {};
						onJoints = new string[] {};
						complyJoints = new string[] {chest}	;

						jointControls = currentAtom.GetComponentsInChildren<FreeControllerV3>(true);
						configureJoints();
						linkJoint(hip, chest);
						linkJoint(lShoulder, chest);
						linkJoint(rShoulder, chest);
						linkJoint(rThigh, hip);
						linkJoint(lThigh, hip);						
					}
				}               
            }, 2, 1);
			
			complyJointsButton = AddButton("Comply Joints", () =>
            {
                // TODO
				IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");
				IEnumerable<Atom> animationPatterns = SuperController.singleton.GetAtoms().Where(a => a.type == "AnimationPattern");

				foreach (Atom at in personAtoms)
				{
					bool isFemale = !at.GetComponentInChildren<DAZCharacter>().isMale;
					
					if (isFemale) 
					{	 					
						 currentAtom = at;
						 possessableJoints = new string[] {};
						 onJoints = new string[] {};
						 complyJoints = new string[] { head, hip, lKnee, rKnee, lFoot, rFoot,/* lElbow, rElbow, lHand, rHand,*/ lThigh, rThigh };	
						 
						 jointControls = currentAtom.GetComponentsInChildren<FreeControllerV3>(true);
						 configureJoints();
			             linkJoint(hip, rThigh);
						 linkJoint(pelvis, lThigh);	
						 //linkJoint(abdomen2, abdomen);

						 pelvisControl.currentRotationState = FreeControllerV3.RotationState.Off;						 
						 hipControl.currentRotationState = FreeControllerV3.RotationState.Off;
					}
				}       

				foreach (Atom at in animationPatterns)
				{
					//at.Remove();
				}
            }, 2, 2);
			
			resetFemalesButton = AddButton("Reset Females Position", () =>
            {
                // TODO
				IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

				foreach (Atom at in personAtoms)
				{					
					bool isFemale = !at.GetComponentInChildren<DAZCharacter>().isMale;
					
					if (isFemale) 
					{				
						currentAtom = at;
						possessableJoints = new string[] {};
						onJoints = new string[] {  head, chest, hip, lFoot, rFoot, lHand, rHand };
						complyJoints = new string[] {};	

						jointControls = currentAtom.GetComponentsInChildren<FreeControllerV3>(true);
						configureJoints();

						headControl.transform.position = new Vector3(0.0f, 1.622f, -0.020f);
						rHandControl.transform.position = new Vector3(0.641f, 1.456f, 0.079f);
						lHandControl.transform.position = new Vector3(-0.641f, 1.456f, 0.079f);
						chestControl.transform.position = new Vector3(0.0f, 1.273f, -0.034f);
						hipControl.transform.position = new Vector3(0.0f, 1.059f, -0.003f);
						rFootControl.transform.position = new Vector3(0.109f, 0.064f, -0.027f);							
						lFootControl.transform.position = new Vector3(-0.109f, 0.064f, -0.027f);
					
						headControl.transform.rotation = Quaternion.Euler(1.0f, 0.00f, 0.0f);
						rHandControl.transform.rotation = Quaternion.Euler(352.96f, 332.30f, 1.42f);
						lHandControl.transform.rotation = Quaternion.Euler(352.96f, 27.70f, 358.58f);		
						chestControl.transform.rotation = Quaternion.Euler(0.0f, 0.00f, 0.0f);
						hipControl.transform.rotation =  Quaternion.Euler(0.0f, 0.00f, 0.0f);																						
						rFootControl.transform.rotation = Quaternion.Euler(18.42f, 14.81f, 2.42f);											
						lFootControl.transform.rotation = Quaternion.Euler(18.42f, -14.81f, -2.42f);							
					}
				}               
            }, 3, 1);
			
			resetMalesButton = AddButton("Reset Males Position", () =>
            {
                // TODO
				IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");
				
				foreach (Atom at in personAtoms)
				{
					bool isMale = at.GetComponentInChildren<DAZCharacter>().isMale;
					
					if (at.uid == "Auto_Load_Male_Person") isMale = true;
					
					if (isMale) 
					{	
						SuperController.LogMessage("isMale");
						currentAtom = at;
						possessableJoints = new string[] {};
						onJoints = new string[] {  head, chest, hip, lFoot, rFoot, lHand, rHand };
						complyJoints = new string[] {};	

						jointControls = currentAtom.GetComponentsInChildren<FreeControllerV3>(true);
						configureJoints();

						headControl.transform.position = new Vector3(0.0f, 1.622f, 1.020f);						 
						rHandControl.transform.position = new Vector3(-0.641f, 1.456f, 0.921f);
						lHandControl.transform.position = new Vector3(0.641f, 1.456f, 0.921f);
						chestControl.transform.position = new Vector3(0.0f, 1.273f, 1.034f);
						hipControl.transform.position = new Vector3(0.0f, 1.059f, 1.003f);
						rFootControl.transform.position = new Vector3(-0.109f, 0.064f, 1.027f); 
						lFootControl.transform.position = new Vector3(0.109f, 0.064f, 1.027f);							
					
						headControl.transform.rotation = Quaternion.Euler(1.0f, 180.00f, 0.0f);
						rHandControl.transform.rotation = Quaternion.Euler(352.96f, 152.30f, 1.42f);
						lHandControl.transform.rotation = Quaternion.Euler(352.96f, 207.70f, 358.58f);
						chestControl.transform.rotation = Quaternion.Euler(0.0f, 180.00f, 0.0f);
						hipControl.transform.rotation =  Quaternion.Euler(-45.0f, 180.00f, 0.0f);																						
						rFootControl.transform.rotation = Quaternion.Euler(18.42f, 194.81f, 2.42f);
						lFootControl.transform.rotation = Quaternion.Euler(18.42f, 165.19f, 2.42f);											
					} else {
						SuperController.LogMessage("is not Male");
					}
				}               
            }, 3, 2);

            maleImprovedPOVLickingButton = AddButton("Male Starts Licking", () =>
            {
                if (isLicking)
                {
                    wantsToStopLicking = true;
                    isLicking = false;
                    SetButtonText();
                }
                else
                {
                    wantsToStartLicking = true;
                    isLicking = true;
                    SetButtonText();
                }
            }, 3, -1);

            SetButtonText();

            canvas.transform.Translate(0, 0.2f, 0);
        }

        public void SetButtonText()
        {
            cycleSetButton.buttonText.text = cycleSetText;
            cycleSet2Button.buttonText.text = cycleSet2Text;

            if (createdMale)
            {
                createMaleButton.buttonText.text = "Remove Male Person";
                maleImprovedPOVLickingButton.gameObject.SetActive(true);
            }
            else
            {
                createMaleButton.buttonText.text = "Add Male Person";
                maleImprovedPOVLickingButton.gameObject.SetActive(false);
            }

            if (isLicking)
            {
                maleImprovedPOVLickingButton.buttonText.text = "Male Stops Licking";
            }
            else
            {
                maleImprovedPOVLickingButton.buttonText.text = "Male Starts Licking";
            }

            if (autoLoadType == 0)
            {
                autoLoadButton.buttonText.text = "Auto Load Disabled >";
                loadNowButton.gameObject.SetActive(true);
            } else if (autoLoadType == 1)
            {
                autoLoadButton.buttonText.text = "Auto Load for Solo Scenes >";
                loadNowButton.gameObject.SetActive(false);
            } else if (autoLoadType == 2)
            {
                autoLoadButton.buttonText.text = "Auto Load for All Scenes >";
                loadNowButton.gameObject.SetActive(false);
            }
            else if (autoLoadType == 3)
            {
                autoLoadButton.buttonText.text = "Custom SETTINGS >";
                loadNowButton.gameObject.SetActive(false);
            }

            if (cycleSet2Button.buttonText.text == "Add VAM Launch >")
            {
                loadNowButton.buttonText.text = "Add Launch Only";
            }
            else
            {
                loadNowButton.buttonText.text = "Load Plugins Now";
            }
        }


        public UIDynamicButton AddButton(string name, UnityAction callback, int column, int row)
        {
            Color accessButtonColor = new Color(0.8392f, 0.8392f, 0.8392f);
            Color accessTextColor = new Color(0, 0, 0);
            float xSpacing = 0.22f;
            float ySpacing = 0.05f;

            UIDynamicButton button = CreateButton(name, 100, 40);
            button.button.onClick.AddListener(callback);
            button.transform.Translate(column * xSpacing, 0.45f - row * ySpacing, 0, Space.Self);
            ColorButton(button, accessTextColor, accessButtonColor);

            return button;
        }

        public UIDynamicButton CreateButton(string name, float width = 100, float height = 80)
        {
            Transform button = GameObject.Instantiate<Transform>(plugin.manager.configurableButtonPrefab);
            ConfigureTransform(button, width, height);
            ParentToCanvas(button);

            UIDynamicButton uiButton = button.GetComponent<UIDynamicButton>();
            uiButton.label = name;
            uiButton.buttonText.fontSize = 18;
            return uiButton;
        }

        public static void ColorButton(UIDynamicButton button, Color textColor, Color buttonColor)
        {
            button.textColor = textColor;
            button.buttonColor = buttonColor;
        }

        private void ConfigureTransform(Transform t, float width, float height)
        {
            t.transform.position = Vector3.zero;
            RectTransform rt = t.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(width / 2, height / 2);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void ParentToCanvas(Transform t)
        {
            t.SetParent(canvas.transform, false);
        }

        public void LookAtCamera()
        {
            if (isDesktopMode)
            {
                canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
            }
            else
            {
                if (XRSettings.enabled == false)
                {
                    Transform cameraT = SuperController.singleton.lookCamera.transform;
                    Vector3 endPos = cameraT.position + cameraT.forward * 10000000.0f;
                    canvas.transform.LookAt(endPos, cameraT.up);
                }
                else
                {
                    canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
                }
            }
        }

        public void OnDestroy()
        {
            try
            {
                if (SuperController.singleton != null)
                {
                    SuperController.singleton.RemoveCanvas(canvas);
                }

                if (canvas != null)
                {
                    canvas.transform.SetParent(null, false);

                    if (canvas.gameObject != null)
                    {
                        GameObject.Destroy(canvas.gameObject);
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }

        }
		
		private void configureJoints()
        {
			foreach (var joint in jointControls)
			{
				if (joint == null || joint.followWhenOff == null || joint.control == null || joint.name == "control")
					continue;
				
				if (notInteractableJoints.Contains(joint.name)) {
					joint.interactableInPlayMode = false;
					joint.canGrabPosition = false;
					joint.canGrabRotation = false;
				}
				else
					joint.interactableInPlayMode = true;
				
				if (possessableJoints.Contains(joint.name))
					joint.possessable = true;
				else
					joint.possessable = false;
				
				if (onJoints.Contains(joint.name)) 
				{
					joint.currentPositionState = FreeControllerV3.PositionState.On;
					joint.currentRotationState = FreeControllerV3.RotationState.On;
				}
				else if (complyJoints.Contains(joint.name)) 
				{
					joint.currentPositionState = FreeControllerV3.PositionState.Comply;
					joint.currentRotationState = FreeControllerV3.RotationState.Comply;
				}
				else  
				{
					joint.currentPositionState = FreeControllerV3.PositionState.Off;
					joint.currentRotationState = FreeControllerV3.RotationState.Off;
				}
				
				joint.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
				
				// SuperController.LogMessage(joint.name);
                    
				switch (joint.name)
				{
					case "headControl":
						headControl = joint;
						break;
					case "chestControl":
						chestControl = joint;
						break;
					case "hipControl":
						hipControl = joint;
						break;
					case "lFootControl":
						lFootControl = joint;
						break;
					case "rFootControl":
						rFootControl = joint;
						break;
					case "lHandControl":
						lHandControl = joint;
						break;
					case "rHandControl":
						rHandControl = joint;
						break;
					case "lThighControl":
						lThighControl = joint;
						break;
					case "rThighControl":
						rThighControl = joint;
						break;
					case "lArmControl":
						lArmControl = joint;
						break;
					case "rShoulderControl":
						rShoulderControl = joint;
						break;
					case "lShoulderControl":
						lShoulderControl = joint;
						break;
					case "rArmControl":
						rArmControl = joint;
						break;
					case "lElbowControl":
						lElbowControl = joint;
						break;
					case "rElbowControl":
						rElbowControl = joint;
						break;
					case "lKneeControl":
						lKneeControl = joint;
						break;
					case "neckControl":
						neckControl = joint;
						break;
					case "abdomenControl":
						abdomenControl = joint;
						break;
					case "abdomen2Control":
						abdomen2Control = joint;
						break;
					case "pelvisControl":
						pelvisControl = joint;
						break;
					case "penisTipControl":
						penisTipControl = joint;
						break;
					case "penisMidControl":
						penisMidControl = joint;
						break;
					case "penisBaseControl":
						penisBaseControl = joint;
						break;
					case "testesControl":
						testesControl = joint;
						break;  						
				}
			}
		}	
		
		private void linkJoint(string joint1, string joint2)
		{
			Rigidbody chestControl = currentAtom.rigidbodies.First(rb => rb.name == joint2);
			var jointControl = jointControls.First(joint => joint.name == joint1);
			
			jointControl.SelectLinkToRigidbody(chestControl, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
			jointControl.currentPositionState = FreeControllerV3.PositionState.ParentLink;
			jointControl.currentRotationState = FreeControllerV3.RotationState.ParentLink;
		}
	}
}
