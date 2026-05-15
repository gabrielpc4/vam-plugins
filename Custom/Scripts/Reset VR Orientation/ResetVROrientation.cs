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
    public class ResetVROrientation : MVRScript
    {
        protected JSONStorableBool _uiNeedsUpdate;
        protected JSONStorableBool _showMainMenuButton;
        protected JSONStorableBool _hardResetToggle;
        protected JSONStorableBool _180DegreeOfset;
        protected JSONStorableBool _showAdditionalButton;
        protected JSONStorableString _additionalButtonText;
        protected JSONStorableString _additionalButtonScene;
        protected JSONStorableBool _removeMenuSystemNow;
        protected JSONStorableString _sceneToLoadAfterMenuRemoved;

        protected UIDynamicTextField _additionalButtonTextField;
        protected UIDynamicTextField _additionalButtonSceneField;
        private Camera _mainCamera;
        private static bool rotationAdjust = false;

        public static Canvas canvas = null;
        MVRScript plugin;
        private float UIScale = 1.0f;

        UIDynamicButton _mainMenuButton = null;
        UIDynamicButton _additionalButton = null;
        UIDynamicButton _resetVROrientationButton = null;
        UIDynamicButton toggleUIButton = null;

        /// <summary>
        /// World Gabriel HUD buttons (E-Motion / Spankings column); Easy Mate
        /// MainUIButtons stay visible — this toggle does not hide them.
        /// </summary>
        bool _gabrielHudEasyButtonsVisible = false;

        private bool isDesktopMode = false;

        public override void Init()
        {
            try
            {
                _mainCamera = CameraTarget.centerTarget?.targetCamera;
                
                var btn = CreateButton("Reset VR Orientation", true);
                btn.button.onClick.AddListener(() => { ResetNow(); });
                btn.buttonColor = Color.green;

                _180DegreeOfset = new JSONStorableBool("Turn Around (180 Degree Offset)", true);
                CreateToggle(_180DegreeOfset);
                RegisterBool(_180DegreeOfset);
                _180DegreeOfset.storeType = JSONStorableParam.StoreType.Full;

                _hardResetToggle = new JSONStorableBool("Hard Reset instead of Main Menu Load (reload scene to see the change)", false);
                CreateToggle(_hardResetToggle);
                RegisterBool(_hardResetToggle);
                _hardResetToggle.storeType = JSONStorableParam.StoreType.Full;

                _showMainMenuButton = new JSONStorableBool("Show Main Menu Button (reload scene to see the change)", true);
                CreateToggle(_showMainMenuButton);
                RegisterBool(_showMainMenuButton);
                _showMainMenuButton.storeType = JSONStorableParam.StoreType.Full;

                _showAdditionalButton = new JSONStorableBool("Show Additional Button (reload scene to see the change)", true);
                CreateToggle(_showAdditionalButton);
                RegisterBool(_showAdditionalButton);
                _showAdditionalButton.storeType = JSONStorableParam.StoreType.Full;

                _additionalButtonText = new JSONStorableString("Additional Button Text", "Looks Menu");
                // register tells engine you want value saved in json file during save and also make it available to
                // animation/trigger system
                RegisterString(_additionalButtonText);
                _additionalButtonTextField = CreateTextField(_additionalButtonText);

                _additionalButtonScene = new JSONStorableString("Additional Button Scene", "Saves/scene/PersonLooksMenu.json");
                // register tells engine you want value saved in json file during save and also make it available to
                // animation/trigger system
                RegisterString(_additionalButtonScene);
                _additionalButtonSceneField = CreateTextField(_additionalButtonScene);

                _removeMenuSystemNow = new JSONStorableBool("Remove Menu System Now (for merge load)", false);
                CreateToggle(_removeMenuSystemNow);
                RegisterBool(_removeMenuSystemNow);
                _removeMenuSystemNow.toggle.onValueChanged.AddListener((value) => { RemoveMenuSystem(value); });
                _removeMenuSystemNow.storeType = JSONStorableParam.StoreType.Full;

                _sceneToLoadAfterMenuRemoved = new JSONStorableString("Scene to Load After Menu Removed", "");
                // register tells engine you want value saved in json file during save and also make it available to
                // animation/trigger system
                RegisterString(_sceneToLoadAfterMenuRemoved);
                CreateTextField(_sceneToLoadAfterMenuRemoved);

                isDesktopMode = !(SuperController.singleton.isOVR || SuperController.singleton.isOpenVR);

                _uiNeedsUpdate = new JSONStorableBool("UI Needs Update", false);
                RegisterBool(_uiNeedsUpdate);

                //NOT YET IMPLEMENTED
                //ShowMainMenuButtonChanged(_showMainMenuButton.val);
            }
            catch (Exception e)
            {
                SuperController.LogError("Failed to initialize OffsetDesktopViewToVR: " + e);
                DestroyImmediate(this);
            }
        }

        public void Start()
        {
            try
            {
                Cleanup();
                float worldScale = SuperController.singleton.worldScale;
                SuperController.singleton.worldScale = 1.0f;
                CreateButtons();
                SuperController.singleton.worldScale = worldScale;
            }
            catch (Exception e)
            {
                SuperController.LogError("ResetVROrientation Start(): " + e);
                DestroyImmediate(this);
            }
        }

        public void RemoveMenuSystem(bool doRemove)
        {
            try
            {
                if (doRemove)
                {
                    _removeMenuSystemNow.SetVal(false);
                    foreach (Atom atom in SuperController.singleton.GetAtoms())
                    {
                        if (atom != this.containingAtom && (atom.name.StartsWith("_Menu") || atom.name.StartsWith("Left") || atom.name.StartsWith("Right") || atom.name.StartsWith("Front")))
                        {
                            atom.Remove();
                        }
                    }
                    if (_sceneToLoadAfterMenuRemoved.val != "")
                    {
                        //remove these so the new scene loads in the right place
                        SuperController.singleton.GetAtomByUid("WindowCamera").Remove();
                        SuperController.singleton.GetAtomByUid("PlayerNavigationPanel").Remove();
                        LoadSceneMergeTracked(_sceneToLoadAfterMenuRemoved.val);
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("ResetVROrientation RemoveMenuSystem(): " + e);
                DestroyImmediate(this);
            }
        }


        public void ResetNow()
        {
            try
            {
                float offsetAngle = SuperController.singleton.lookCamera.transform.rotation.eulerAngles.y;
                if (_180DegreeOfset.val)
                {
                    offsetAngle += 180.0f;
                }
                //SuperController.singleton.lookCamera.transform.parent.rotation = Quaternion.identity;
                SuperController.singleton.lookCamera.transform.parent.Rotate(new Vector3(0, -offsetAngle, 0));
                _mainCamera.transform.Rotate(new Vector3(0, offsetAngle, 0));

                SuperController.singleton.lookCamera.transform.parent.position += SuperController.singleton.MonitorCenterCamera.transform.position - SuperController.singleton.lookCamera.transform.position;

                if (_mainCamera.name == "MonitorRig")
                {
                    SuperController.LogMessage("Can't reset VR view when in desktop mode or once desktop UI has been used.");
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("ResetVROrientation ResetNow(): " + e);
                DestroyImmediate(this);
            }
        }

        public void LoadEasyMateMainMenu()
        {
            LoadSceneTracked("Saves/scene/MainMenu.json");
        }
        public void LoadEasyMateLooksMenu()
        {
            LoadSceneTracked("Saves/scene/PersonLooksMenu.json");
        }

        public void HardReset()
        {
            SuperController.singleton.HardReset();
        }

        //Based on Dollmaster UI code
        public void Cleanup()
        {
            try
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
            catch (Exception e)
            {
                SuperController.LogError("ResetVROrientation Cleanup(): " + e);
                DestroyImmediate(this);
            }
        }

        public void CreateButtons()
        {
            try
            {
                plugin = this;
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

                canvas.transform.localPosition = new Vector3(0.5f, -0.66f, 0.35f);

                LookAtCamera();


                if (isDesktopMode)
                {
                    if (_hardResetToggle.val)
                    {
                        _mainMenuButton = AddButton("Hard Reset to Menu", () =>
                        {
                            HardReset();
                        }, 0, 0);

                    }
                    else
                    {
                        _mainMenuButton = AddButton("Main Menu", () =>
                        {
                            //if (SuperController.singleton.playModeToggle.isOn) { }
                            LoadEasyMateMainMenu();
                        }, 0, 0);

                    }
                    if (_showAdditionalButton.val && !_hardResetToggle.val)
                    {
                        _additionalButton = AddButton(_additionalButtonText.val, () =>
                        {
                            LoadSceneTracked(_additionalButtonScene.val);
                        }, 0, 1);
                    }

                    toggleUIButton = AddButton("", () =>
                    {
                        ToggleUIShown();
                    }, 0, 4);
                }
                else
                {
                    _resetVROrientationButton = AddButton("Reset VR Orientation", () =>
                    {
                        ResetNow();
                    }, -1, 1);

                    if (_hardResetToggle.val)
                    {
                        _mainMenuButton = AddButton("Hard Reset to Menu", () =>
                        {
                            HardReset();
                        }, 0, 0);

                    }
                    else
                    {
                        _mainMenuButton = AddButton("Main Menu", () =>
                        {
                            //if (SuperController.singleton.playModeToggle.isOn) { }
                            LoadEasyMateMainMenu();
                        }, 0, 0);

                    }

                    if (_showAdditionalButton.val && !_hardResetToggle.val)
                    {
                        _additionalButton = AddButton(_additionalButtonText.val, () =>
                        {
                            LoadSceneTracked(_additionalButtonScene.val);
                        }, -1, 0);
                    }

                    toggleUIButton = AddButton("", () =>
                    {
                        ToggleUIShown();
                    }, 0, 1);
                }

                SetButtonNames();
                canvas.transform.Translate(0, 0.2f, 0);
            }
            catch (Exception e)
            {
                SuperController.LogError("ResetVROrientation CreateButtons(): " + e);
                DestroyImmediate(this);
            }
        }
        private JSONStorable FindPluginInScene(string fullClassName)
        {
            JSONStorable pluginJSON = null;
            foreach (Atom testAtom in SuperController.singleton.GetAtoms())
            {
                List<string> names = testAtom.GetStorableIDs();
                if (names != null && names.Count > 0)
                {
                    string pluginName = names.Find(s => s.StartsWith("plugin#") && s.EndsWith(fullClassName));
                    if (pluginName != null && pluginName != "")
                    {
                        pluginJSON = testAtom.GetStorableByID(pluginName);
                        if (pluginJSON != null) return pluginJSON;
                    }
                }
            }
            return pluginJSON;
        }

        public void ToggleUIShown()
        {
            JSONStorable gabrielHud;

            _gabrielHudEasyButtonsVisible = !_gabrielHudEasyButtonsVisible;

            SetButtonNames();

            gabrielHud = FindPluginInScene("geesp0t.GabrielHud");
            if (gabrielHud != null)
            {
                if (_gabrielHudEasyButtonsVisible)
                    gabrielHud.CallAction("Show UI");
                else
                    gabrielHud.CallAction("Hide UI");
            }
        }
        
        public void SetButtonNames()
        { 
            if (_gabrielHudEasyButtonsVisible)
            {
                toggleUIButton.buttonText.text = "Hide Easy Buttons";
            }
            else
            {
                toggleUIButton.buttonText.text = "Show Easy Buttons";
            }
        }

        public void UpdateAdditionalButtonName()
        {
            _additionalButton.buttonText.text = _additionalButtonText.val;
        }

        public UIDynamicButton AddButton(string name, UnityAction callback, int column, int row)
        {
            Color accessButtonColor = new Color(0.8392f, 0.8392f, 0.8392f);
            Color accessTextColor = new Color(0, 0, 0);
            float xSpacing = 0.22f;
            float ySpacing = 0.05f;

            UIDynamicButton button = CreateButton(name, 100, 40);
            button.button.onClick.AddListener(callback);

            if (isDesktopMode)
            {
                button.transform.Translate(column * xSpacing, 0.375f - row * ySpacing, 0.33f, Space.Self);
            } else
            {
                button.transform.Translate(column * xSpacing, 0.45f - row * ySpacing, 0, Space.Self);
            }
            ColorButton(button, accessTextColor, accessButtonColor);

            return button;
        }

        public UIDynamicButton CreateButton(string name, float width = 100, float height = 80)
        {
            Transform button = GameObject.Instantiate<Transform>(this.manager.configurableButtonPrefab);
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

        public void Update()
        {
            if (_uiNeedsUpdate.val)
            {
                _uiNeedsUpdate.SetVal(false);
                UpdateAdditionalButtonName();
            }
        }

        private void LoadSceneTracked(string sceneJsonPath)
        {
            SuperController.LogMessage(string.Format(
                "EasyMate [scene load]: starting Load(scene) - {0}",
                NormalizeFwd(sceneJsonPath)));
            SuperController.singleton.Load(sceneJsonPath);
        }

        private void LoadSceneMergeTracked(string sceneJsonPath)
        {
            SuperController.LogMessage(string.Format(
                "EasyMate [scene load]: starting LoadMerge(scene) - {0}",
                NormalizeFwd(sceneJsonPath)));
            SuperController.singleton.LoadMerge(sceneJsonPath);
        }

        private string NormalizeFwd(string path)
        {
            if (path == null)
            {
                return "";
            }

            return path.Replace('\\', '/');
        }

        public void LookAtCamera()
        {
            if (isDesktopMode)
            {
                canvas.transform.localEulerAngles = new Vector3(1, 180, 0);
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

        void OnDestroy()
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
    }
}