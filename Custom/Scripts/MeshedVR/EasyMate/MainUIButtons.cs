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
    public class MainUIButtons2
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
        static string lShoulder = "lArmControl";
        static string rShoulder = "rArmControl";
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


        private bool logMessages = false;

        MVRScript plugin;

        private Camera _mainCamera;
        public static Canvas canvas = null;
        private float UIScale = 1.0f;
                
        private bool isDesktopMode = false;
		
		public bool shouldPossessMale = true;

        UIDynamicButton clothingCycleButton = null;
        UIDynamicButton loadLookButton = null;
        UIDynamicButton loadPoseButton = null;
        UIDynamicButton lookAtPersonButton = null;

        Dictionary<Atom, List<bool>> personClothing = new Dictionary<Atom, List<bool>>();
        int clothingCycle = 0;
        bool savedClothes = false;
        bool canExposeClothing = true;

        private void Log(string message)
        {
            if (logMessages) SuperController.LogMessage(message);
        }

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
            clothingCycle = 0;
			
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

            lookAtPersonButton = AddButton("Possuir", () =>
            {
				PossessMale();
            }, 0, -1);

            clothingCycleButton = AddButton("", () =>
            {
                CycleClothing();
            }, 0, 0);

            if (isDesktopMode) { 
                loadLookButton = AddButton("Load Look", () =>
                {
                    LoadLook();
                }, 0, 1);

                loadPoseButton = AddButton("Load Pose", () =>
                {
                    LoadPose();
                }, 0, 2);
            } else
            {
                loadLookButton = AddButton("Load Look", () =>
                {
                    LoadLook();
                }, 0, 2);

                loadPoseButton = AddButton("Load Pose", () =>
                {
                    LoadPose();
                }, 0, 3);
            }

            SetButtonNames();
                                   
            canvas.transform.Translate(0, 0.2f, 0);
        }
        public void ShowUI(bool setToActive)
        {
            clothingCycleButton.gameObject.SetActive(setToActive);
            loadLookButton.gameObject.SetActive(setToActive);
            loadPoseButton.gameObject.SetActive(setToActive);
            lookAtPersonButton.gameObject.SetActive(setToActive);
        }
		
		public void PossessMale() {							
			// TODO
			IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

			foreach (Atom at in personAtoms)
			{				
				
				bool isMale = at.GetComponentInChildren<DAZCharacter>().isMale;
				if (at.uid == "Auto_Load_Male_Person") isMale = true;
				
				if (isMale) 
				{	 
					currentAtom = at;
					
					if  (shouldPossessMale) {
						possessableJoints = new string[] { head, lHand, rHand };;
						onJoints = new string[] { chest };
						complyJoints = new string[]{ };		

						jointControls = currentAtom.GetComponentsInChildren<FreeControllerV3>(true);
						configureJoints();
						linkJoint(head, chest);
						linkJoint(hip, chest);
						linkJoint(lThigh, chest);
						linkJoint(rThigh, chest);
						linkJoint(testicles, hip);
						linkJoint(penisBase, penisMid);
						linkJoint(penisMid, penisTip);
						linkJoint(penisTip, testicles);
						
						Rigidbody leapMotionRightHand = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "RightHandAlternate");
						Rigidbody leapMotionLeftHand = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "LeftHandAlternate");
						
						Rigidbody controllerRightHand = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "RightHand");
						Rigidbody controllerLeftHand = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "LeftHand");
						
														
						
						
						// Possess left hand
						var leftHandPossessTarget = leapMotionLeftHand;						
						if (leapMotionLeftHand == null) {
							leftHandPossessTarget = controllerLeftHand;						
						}
						lHandControl.transform.position = leftHandPossessTarget.transform.position;				

												
						lHandControl.SelectLinkToRigidbody(leftHandPossessTarget, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
						lHandControl.currentPositionState = FreeControllerV3.PositionState.ParentLink;
						lHandControl.currentRotationState = FreeControllerV3.RotationState.ParentLink;			
						
						// Possess right hand																		
						rHandControl.transform.position = leapMotionRightHand.transform.position;
						rHandControl.transform.rotation = leapMotionRightHand.transform.rotation;
						rHandControl.SelectLinkToRigidbody(leapMotionRightHand, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
						rHandControl.currentPositionState = FreeControllerV3.PositionState.ParentLink;
						rHandControl.currentRotationState = FreeControllerV3.RotationState.ParentLink;			
						
						
						/*
						// Link Right Hand to Chest
						var chestLinkTarget = leapMotionRightHand;
						if (leapMotionRightHand == null) {
							chestLinkTarget = controllerRightHand;							
						} 
						chestControl.SelectLinkToRigidbody(chestLinkTarget, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
						chestControl.currentPositionState = FreeControllerV3.PositionState.ParentLink;
						chestControl.currentRotationState = FreeControllerV3.RotationState.ParentLink;			
						*/
																		
						penisTipControl.canGrabPosition = true;
						penisTipControl.canGrabRotation = true;
						
						penisMidControl.canGrabPosition = false;
						penisMidControl.canGrabRotation = false;		
						
						lookAtPersonButton.buttonText.text = "DesPossuir";
						shouldPossessMale = false;
					} else {
						var jointControls = at.GetComponentsInChildren<FreeControllerV3>(true);
						var chestControl = jointControls.First(joint => joint.name == "chestControl");
						chestControl.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
						rHandControl.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
						lHandControl.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
						lookAtPersonButton.buttonText.text = "Possuir";
						shouldPossessMale = true;
					}
				}
			}               
		}

        public void LookAtConstrained(Vector3 targetPos, Transform sourceTransform)
        {
            sourceTransform.LookAt(new Vector3(targetPos.x, sourceTransform.position.y, targetPos.z));
        }

        public void LoadLook()
        {
            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");
            //first female, or if no female, first male
            Atom firstFound = null;
            foreach (Atom at in personAtoms)
            {
                if (!at.GetComponentInChildren<DAZCharacter>().isMale)
                {
                    firstFound = at;
                    break;
                }
            }

            if (firstFound == null)
            {
                //strange code but ienumerable wants to enumerate
                foreach (Atom at in personAtoms)
                {
                    firstFound = at;
                    break;
                }
            }

            if (firstFound != null)
            {
                //load the look
                firstFound.PreRestore();
                firstFound.LoadAppearancePresetDialog();
                ClothingResetCycle();
                firstFound.PostRestore();//runs before load finishes, but may still help prevent errors?
            }
        }
        public void LoadPose()
        {
            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            //first female, or if no female, first male
            Atom firstFound = null;
            foreach (Atom at in personAtoms)
            {
                if (!at.GetComponentInChildren<DAZCharacter>().isMale)
                {
                    firstFound = at;
                    break;
                }
            }

            if (firstFound == null)
            {
                //strange code but ienumerable wants to enumerate
                foreach (Atom at in personAtoms)
                {
                    firstFound = at;
                    break;
                }
            }

            if (firstFound != null)
            {
                //load the pose
                //SuperController.singleton.editModeToggle.isOn = true;
                firstFound.PreRestore();
                firstFound.LoadPhysicalPresetDialog();
                firstFound.PostRestore(); //runs before load finishes, but may still help prevent errors?
            }
        }			
		
        public void LookAtPerson()
        {			
			IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            foreach (Atom at in personAtoms)
            {
				bool isMale = at.GetComponentInChildren<DAZCharacter>().isMale;
				
				if (isMale) 
				{	
					if  (shouldPossessMale) {

					} else {
						var jointControls = at.GetComponentsInChildren<FreeControllerV3>(true);
						var chestControl = jointControls.First(joint => joint.name == "chestControl");
						chestControl.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
						lookAtPersonButton.buttonText.text = "Possuir";
						shouldPossessMale = true;
					}
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
            Log("BUILT PLUGIN STRING: " + sb.ToString());
            return JSONNode.Parse(sb.ToString()).AsObject;
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

        //using ideas from Dollmaster handling of clothing
        //data of all the clothing, if you start this cycle, then load a new scene, when you get to restore the clothing... we need to detect scene changes

        public void ClothingResetCycle()
        {
            clothingCycle = 0;
            personClothing = new Dictionary<Atom, List<bool>>();

            canExposeClothing = true;

            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            foreach (Atom at in personAtoms)
            { 
                if (CycleDressProgression(GetWrapProgression(at), false, true))
                {
                    canExposeClothing = true;
                    break;
                }
            }

            savedClothes = false;

            if (!canExposeClothing)
            {
                clothingCycle = 1;
            }

            SetButtonNames();
        }

        public void SaveCurrentClothing()
        {
            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            foreach (Atom at in personAtoms)
            {
                if (!personClothing.ContainsKey(at))
                {
                    personClothing.Add(at, new List<bool>());
                }
                var atGeometry = at.GetStorableByID("geometry");
                DAZCharacterSelector character = atGeometry as DAZCharacterSelector;
                personClothing[at] = character.clothingItems.ToList().Select((clothing) =>
                {
                    return clothing.active;
                }).ToList();
            }
        }

        public void SetButtonNames()
        {
            switch (clothingCycle)
            {
                case 0:
                    clothingCycleButton.buttonText.text = "Remover Roupas >";
                    break;
                case 1:
                    clothingCycleButton.buttonText.text = "Restaurar Roupas >";
                    break;               
                default:
                    clothingCycle = 0;
                    break;
            }
        }

        public void CycleClothing()
        {
            switch (clothingCycle)
            {               
                case 0:
                    ClothingRemoveAll();
                    break;
                case 1:
                    ClothingRedress();
                    break;
                default:
                    clothingCycle = 0;
                    break;
            }

            clothingCycle++;
            if (clothingCycle > 1) clothingCycle = 0;

            SetButtonNames();

            if (clothingCycle == 0) ClothingResetCycle();
        }

        public void ClothingExpose()
        {
            if (!savedClothes) SaveCurrentClothing();

            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            foreach (Atom at in personAtoms)
            {
                CycleDressProgression(GetWrapProgression(at));
            }
        }

        public void ClothingAllowUndress()
        {
            if (!savedClothes) SaveCurrentClothing();

            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            foreach (Atom at in personAtoms)
            {
                var atGeometry = at.GetStorableByID("geometry");
                DAZCharacterSelector character = atGeometry as DAZCharacterSelector;
                character.EnableUndressAllClothingItems();
            }
        }

        public void ClothingRemoveAll()
        {
            if (!savedClothes) SaveCurrentClothing();

            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            foreach (Atom at in personAtoms)
            {
                var atGeometry = at.GetStorableByID("geometry");
                DAZCharacterSelector character = atGeometry as DAZCharacterSelector;

                //remove all clothes
                int index = 0;
                character.clothingItems.ToList().ForEach((clothing) =>
                {
                    character.SetActiveClothingItem(clothing, active: false);
                    index++;
                });
            }
        }
        public void ClothingRedress()
        {
            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            foreach (Atom at in personAtoms)
            {
                if (personClothing.ContainsKey(at)) { 
                    var atGeometry = at.GetStorableByID("geometry");
                    DAZCharacterSelector character = atGeometry as DAZCharacterSelector;
                    List<DAZClothingItem> clothes = character.clothingItems.ToList();
                    for (int i = 0; i < personClothing[at].Count; i++)
                    {
                        if (personClothing[at][i]) clothes[i].ResetPhysics();
                        character.SetActiveClothingItem(clothes[i], personClothing[at][i]);
                        if (personClothing[at][i])
                        {
                            ClothSimControl simControl = clothes[i].GetComponentInChildren<ClothSimControl>();
                            if (simControl != null)
                            {
                                simControl.SetBoolParamValue("allowDetach", false);
                            }
                            clothes[i].ResetPhysics();
                        }
                    }
                    CycleDressProgression(GetWrapProgression(at), true); //reset
                }
            }
			ClothingAllowUndress();
        }

        //DOLLMASTER DRESS CONTROLLER
        private bool CycleDressProgression(Dictionary<DAZSkinWrapSwitcher, List<string>> wrapProgression, bool reset = false, bool testOnly = false)
        {
            bool didModify = false;
            wrapProgression.Keys.ToList().ForEach((switcher) =>
            {
                if (switcher.enabled == false)
                {
                    return;
                }

                //Debug.Log(wrap.wrapName + " " + wrap.wrapProgress);
                List<string> progressNames = wrapProgression[switcher];
                int current = progressNames.FindIndex((s) =>
                {
                    return s == switcher.currentWrapName;
                });

                int next = current + 1;
                if (reset || next >= progressNames.Count)
                {
                    next = 0;
                }

                string nextName = progressNames[next];
                if (nextName != null)
                {
                    //SuperController.LogMessage(switcher.currentWrapName + " switching to " + nextName);
                    didModify = true;

                    if (!testOnly)
                        switcher.SetCurrentWrapName(nextName);
                }

            });
            return didModify;
        }

        Dictionary<DAZSkinWrapSwitcher, List<string>> GetWrapProgression(Atom at)
        {
            JSONStorable geometryStorable = at.GetStorableByID("geometry");
            DAZCharacterSelector geometry = geometryStorable as DAZCharacterSelector;
            Dictionary<DAZSkinWrapSwitcher, List<string>> wrapToProgressList = new Dictionary<DAZSkinWrapSwitcher, List<string>>();

            at.GetStorableIDs().ForEach((s) =>
            {
                JSONStorable store = at.GetStorableByID(s);
                store.GetComponents<DAZSkinWrap>().ToList().ForEach((wrap) =>
                {
                    DAZSkinWrapSwitcher switcher = store.GetComponent<DAZSkinWrapSwitcher>();
                    if (switcher == null)
                    {
                        return;
                    }

                    if (wrap.enabled == false)
                    {
                        return;
                    }

                    if (s.Contains("Style") == false)
                    {
                        return;
                    }

                    if (wrapToProgressList.ContainsKey(switcher) == false)
                    {
                        wrapToProgressList[switcher] = new List<string>();
                    }

                    wrapToProgressList[switcher].Add(wrap.wrapName);
                });
            });

            return wrapToProgressList;
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
