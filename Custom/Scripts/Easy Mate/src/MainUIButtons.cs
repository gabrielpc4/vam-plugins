using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace geesp0t
{
    //Adapted from VAMDeluxe Dollmaster UI
    public class MainUIButtons
    {
        private bool logMessages = false;

        MVRScript plugin;

        private Camera _mainCamera;
        public static Canvas canvas = null;
        private float UIScale = 1.0f;

        private bool isDesktopMode = false;

        UIDynamicButton clothingCycleButton = null;
        UIDynamicButton loadLookButton = null;
        UIDynamicButton loadPoseButton = null;
        UIDynamicButton lookAtPersonButton = null;

        Dictionary<Atom, List<bool>> personClothing = new Dictionary<Atom, List<bool>>();
        int clothingCycle = 0;
        bool savedClothes = false;
        bool canExposeClothing = false;

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

            lookAtPersonButton = AddButton("Look at Person", () =>
            {
                LookAtPerson();
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
                Transform headTransfrom = firstFound.freeControllers.First(freec => freec.name == "headControl").transform;
                Vector3 pos = headTransfrom.position;
                Vector3 newPos = pos + headTransfrom.forward * 0.5f;
                var superController = SuperController.singleton;
                var navigationRig = superController.navigationRig;
                Possessor possessor = superController.centerCameraTarget.transform.GetComponent<Possessor>();

                var up = navigationRig.up;
                var targetPosition = headTransfrom.position + headTransfrom.transform.forward;
                var positionOffset = navigationRig.position + targetPosition - possessor.autoSnapPoint.position;
                // Adjust the player height so the user can adjust as needed
                var playerHeightAdjustOffset = Vector3.Dot(positionOffset - navigationRig.position, up);
                navigationRig.position = positionOffset + up * -playerHeightAdjustOffset;
                superController.playerHeightAdjust += playerHeightAdjustOffset;

                LookAtConstrained(pos, navigationRig);
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

        //using ideas from Dollmaster handling of clothing
        //data of all the clothing, if you start this cycle, then load a new scene, when you get to restore the clothing... we need to detect scene changes

        public void ClothingResetCycle()
        {
            clothingCycle = 0;
            personClothing = new Dictionary<Atom, List<bool>>();

            canExposeClothing = false;

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
                    clothingCycleButton.buttonText.text = "Try Expose Clothes >";
                    break;
                case 1:
                    clothingCycleButton.buttonText.text = "Try Allow Undress >";
                    break;
                case 2:
                    clothingCycleButton.buttonText.text = "Remove Clothes >";
                    break;
                case 3:
                    clothingCycleButton.buttonText.text = "Try Restore Clothes >";
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
                    ClothingExpose();
                    break;
                case 1:
                    ClothingAllowUndress();
                    break;
                case 2:
                    ClothingRemoveAll();
                    break;
                case 3:
                    ClothingRedress();
                    break;
                default:
                    clothingCycle = 0;
                    break;
            }

            clothingCycle++;
            if (clothingCycle > 3) clothingCycle = 0;

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

    }
}
