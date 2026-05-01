using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    public class EasyMate : MVRScript
    {
        //Manage the EasyMate menu system, add Main Menu and other buttons to scenes which are loaded from a menu but don't have any return buttons
        //This is also required for scenes packaged in var files, etc., which we won't modify

        private static bool logMessages = false;

        private MainUIButtons mainUIButtons = null; //LOAD PERSON, POSE, ETC.

        public const string resetVROrientationID = "_ResetVROrientation";
        Atom resetVROrientation;

        private string menuDataJSONNodeName = "_EasyMate";
        private string menuDataJSONName = "plugin#0_geesp0t.AutoLoadEasyMate";
        private string pluginDataJSONName = "plugin#0_geesp0t.ResetVROrientation";
        private string uiNeedsUpdateJSONName = "UI Needs Update";
        private string buttonTextJSONName = "Additional Button Text";
        private string buttonSceneJSONName = "Additional Button Scene";
        private string menuButtonScene = "Saves/scene/MainMenu_Page_3.json"; //change this as we browse between menus
        private string menuButtonText = "Menu Page 3";

        private bool isLoading = true;
        private bool sceneChanged = true;
        private float loadingTimeCounter = 0;

        private string lastLoadDir = ""; //wish this was last full path of loaded file included directory and filename!

        private Coroutine _applyEmotionAfterSceneCo;

        public JSONStorableAction hideUI;
        public JSONStorableAction showUI;

        /// <summary>
        /// When true (default), calls <see cref="SuperController.DisableRemoteHoldGrab"/> so the VR <b>grip</b>
        /// (Quest side squeeze / OpenVR HoldGrab) cannot start the remote “hand sticks to FreeController” link
        /// while pointing with the controller laser. VaM has no built-in toggle for this; overlap (hand inside
        /// the control collider) + grip can still full-grab.
        /// </summary>
        public JSONStorableBool disableRemoteGripHandLink;

        /// <summary>
        /// When true (default), a short press on each controller’s <b>physical grip</b> (Oculus) or HoldGrab (OpenVR)
        /// toggles that hand’s VR model on/off; turning a hand on re-enables <c>HandModelControl.useCollision</c> if it was off.
        /// </summary>
        public JSONStorableBool gripTogglesHandVisibility;

        /// <summary>
        /// When true (default), calls <see cref="FreeControllerV3.RestorePreLinkState"/> on active full-grab targets
        /// each <see cref="LateUpdate"/> when not using remote hold-grab (public <see cref="SuperController"/> API only).
        /// </summary>
        public JSONStorableBool blockOverlapFullGrab;

        /// <summary>
        /// When true (default), HMD inside any Person’s head cylinder hides face/hair/glasses without using Snap F/M.
        /// </summary>
        public JSONStorableBool headProximityHideWithoutSnap;

        /// <summary>
        /// When true (default), clears all possession if the look camera moves farther than
        /// <see cref="possessAutoUnpossessFeetMaxHorizontalM"/> (meters) from the possessed character’s feet in the horizontal plane (rig up).
        /// </summary>
        public JSONStorableBool possessAutoUnpossessWhenFarFromFeet;

        /// <summary>When true, merges E-Motion onto every Person shortly after each scene load (deferred so every Person’s plugin list has finished restoring). Default is off; enable in plugin settings, the HUD <b>E-Motion all</b> button, or keyboard <b>E</b>. Keyboard <b>Shift+E</b> turns this off and removes E-Motion from all Persons.</summary>
        public JSONStorableBool loadEmotionOnSceneLoad;

        /// <summary>Horizontal distance threshold from look camera to feet midpoint for <see cref="possessAutoUnpossessWhenFarFromFeet"/>.</summary>
        public JSONStorableFloat possessAutoUnpossessFeetMaxHorizontalM;

        public override void Init()
        {
            Log("EasyMate Init");
            mainUIButtons = new MainUIButtons();
            mainUIButtons.Init(this);

            hideUI = new JSONStorableAction("Hide UI", () => HideUI());
            RegisterAction(hideUI);
            showUI = new JSONStorableAction("Show UI", () => ShowUI());
            RegisterAction(showUI);

            disableRemoteGripHandLink = new JSONStorableBool(
                "Disable remote grip hand-link (Quest / OpenVR)",
                true,
                OnDisableRemoteGripHandLinkChanged);
            RegisterBool(disableRemoteGripHandLink);

            gripTogglesHandVisibility = new JSONStorableBool("Grip toggles VR hand visibility", true);
            RegisterBool(gripTogglesHandVisibility);

            blockOverlapFullGrab = new JSONStorableBool("Block overlap full-grab (auto-release each frame)", true);
            RegisterBool(blockOverlapFullGrab);

            headProximityHideWithoutSnap = new JSONStorableBool("VR head proximity hide (no Snap required)", true, OnHeadProximityHideWithoutSnapChanged);
            RegisterBool(headProximityHideWithoutSnap);

            possessAutoUnpossessWhenFarFromFeet = new JSONStorableBool(
                "Auto-unpossess when look camera drifts from feet (horizontal)",
                true);
            RegisterBool(possessAutoUnpossessWhenFarFromFeet);

            possessAutoUnpossessFeetMaxHorizontalM = new JSONStorableFloat(
                "Max horizontal camera–feet distance (m)",
                1.35f,
                0.35f,
                5f);
            RegisterFloat(possessAutoUnpossessFeetMaxHorizontalM);

            loadEmotionOnSceneLoad = new JSONStorableBool("Load E-Motion on every scene", false, OnLoadEmotionOnSceneLoadChanged);
            RegisterBool(loadEmotionOnSceneLoad);
            mainUIButtons.BindSceneEmotionAutoLoad(loadEmotionOnSceneLoad);

            EasyMateGripHandVisibility.SetSpankingsFirstHandShowMergeCallback(() =>
            {
                if (mainUIButtons != null)
                    mainUIButtons.MergeSpankingsOnAllPersonsOnly();
            });
        }

        private void OnLoadEmotionOnSceneLoadChanged(bool v)
        {
            if (mainUIButtons == null)
                return;
            if (!v)
                mainUIButtons.RemoveEmotionFromAllPersons();
            else
                mainUIButtons.MergeEmotionOnAllPersonsOnly();
            mainUIButtons.RefreshEmotionSceneLoadButtonLabel();
        }

        private void OnHeadProximityHideWithoutSnapChanged(bool v)
        {
            EasyMateHeadSnapPovRuntime.SetHeadProximityHideWithoutSnapEnabled(v, this);
        }

        private void OnDisableRemoteGripHandLinkChanged(bool v)
        {
            ApplyRemoteHoldGrabPreference();
        }

        private void ApplyRemoteHoldGrabPreference()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || disableRemoteGripHandLink == null)
                return;
            if (!sc.isOVR && !sc.isOpenVR)
                return;
            if (disableRemoteGripHandLink.val)
                sc.DisableRemoteHoldGrab();
            else
                sc.EnableRemoteHoldGrab();
        }
        public void ShowUI()
        {
            if (mainUIButtons != null)
            {
                mainUIButtons.ShowUI(true);
            }
        }
        public void HideUI()
        {
            if (mainUIButtons != null)
            {
                mainUIButtons.ShowUI(false);
            }
        }

        void Start()
        {
            Log("EasyMate Start");
            if (mainUIButtons != null) mainUIButtons.Start();
            ApplyRemoteHoldGrabPreference();
            if (headProximityHideWithoutSnap != null)
                EasyMateHeadSnapPovRuntime.SetHeadProximityHideWithoutSnapEnabled(headProximityHideWithoutSnap.val, this);
            EasyMateGripHandVisibility.DisableVrHandModelsForSceneStart();

            if (mainUIButtons != null)
                mainUIButtons.RefreshEmotionSceneLoadButtonLabel();
        }

        /// <summary>
        /// Person plugin lists can restore over several frames; merge E-Motion (when enabled), clothing touch fall-off on everyone, then refresh HUD labels.
        /// </summary>
        private IEnumerator CoApplyEmotionAfterSceneSettles()
        {
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.35f);

            try
            {
                if (SuperController.singleton == null || mainUIButtons == null)
                    yield break;

                if (loadEmotionOnSceneLoad != null && loadEmotionOnSceneLoad.val)
                    mainUIButtons.MergeEmotionOnAllPersonsOnly();
                mainUIButtons.MergeClothingTouchFallOffOnAllPersonsOnly();
                mainUIButtons.RefreshEmotionSceneLoadButtonLabel();
                mainUIButtons.RefreshPluginToggleLabels();
            }
            finally
            {
                _applyEmotionAfterSceneCo = null;
            }
        }

        private IEnumerator CreateResetVROrientationAtom()
        {
            yield return SuperController.singleton.AddAtomByType("Empty", resetVROrientationID);
            resetVROrientation = SuperController.singleton.GetAtomByUid(resetVROrientationID);
            ConfigureResetVROrientation();
        }

        private void GetMenuData()
        {
            //THIS DATA ISN'T WHAT TO SET FOR THIS PAGE, IT'S WHAT TO SET ON ANY NEXT PAGE THAT DOESN'T HAVE ANYTHING SET
            Atom menuData = SuperController.singleton.GetAtomByUid(menuDataJSONNodeName);
            if (menuData != null)
            {
                JSONStorable easyMateMenuData = menuData.GetStorableByID(menuDataJSONName);
                if (easyMateMenuData != null)
                {
                    string buttonText = easyMateMenuData.GetStringParamValue(buttonTextJSONName);
                    if (buttonText != null && buttonText != "")
                    {
                        Log("Last Menu Button Text: " + buttonText);
                        menuButtonText = buttonText;
                    }

                    string buttonScene = easyMateMenuData.GetStringParamValue(buttonSceneJSONName);
                    if (buttonScene != null && buttonScene != "")
                    {
                        Log("Last Menu Button Scene: " + buttonScene);
                        menuButtonScene = buttonScene;
                    }
                }
            }
        }

        private void LogError(string error)
        {
            SuperController.LogError(error);
        }

        private void Log(string message)
        {
            if (logMessages) SuperController.LogMessage(message);
        }

        private static bool SceneHasAnyActivePossession()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3[] fcs = a.transform.GetComponentsInChildren<FreeControllerV3>(true);
                    for (int i = 0; i < fcs.Length; i++)
                    {
                        if (fcs[i] != null && fcs[i].possessed)
                            return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private void ClearAllPossessionIfLoadedSceneHadAny()
        {
            try
            {
                if (!SceneHasAnyActivePossession())
                    return;
                // SuperController.ClearPossess() — see Reference/Assembly-CSharp-decompiled/SuperController.cs
                SuperController.singleton.ClearPossess();
                SuperController.LogMessage("EasyMate: ClearPossess after scene load (possession was active).");
            }
            catch (Exception e)
            {
                LogError("EasyMate ClearPossess on scene load failed: " + e.Message);
            }
        }

        private void ConfigureResetVROrientation()
        {
            if (resetVROrientation == null)
            {
                LogError("Missing required " + resetVROrientationID + " Atom.");
            }

            JSONClass jc;
            MVRPluginManager pluginManager = resetVROrientation.GetStorableByID("PluginManager") as MVRPluginManager;
            JSONClass pluginManagerJSON = pluginManager.GetJSON(true, true, true);
            if (pluginManagerJSON["plugins"] != null && pluginManagerJSON["plugins"]["plugin#0"] != null &&
                    pluginManagerJSON["plugins"]["plugin#0"].Value != "")
            {
                //has a plugin
            } else
            {
                //make the plugin
                Log("Adding ResetVROrientation Plugin");
                jc = BuildResetVROrientationPlugin();
                pluginManager.LateRestoreFromJSON(jc);
            }

            JSONStorable pluginData = resetVROrientation.GetStorableByID(pluginDataJSONName);
            if (pluginData != null)
            {
                string buttonText = pluginData.GetStringParamValue(buttonTextJSONName);
                string buttonScene = pluginData.GetStringParamValue(buttonSceneJSONName);

                if (buttonText == null || buttonText == "" || buttonText == "Looks Menu"
                    || buttonScene == null || buttonScene == "" || buttonScene.EndsWith("PersonLooksMenu.json"))
                { 
                    pluginData.SetStringParamValue(buttonTextJSONName, menuButtonText);
                    pluginData.SetStringParamValue(buttonSceneJSONName, menuButtonScene);
                    pluginData.SetBoolParamValue(uiNeedsUpdateJSONName, true);
                    Log("Setting ResetVROrientation Plugin Data: " + menuButtonText + ", " + menuButtonScene);
                } else
                {
                    Log("Already had ResetVROrientation Plugin Data");
                }
            } else
            {
                LogError("Missing ResetVROrientation Data Storable: " + pluginDataJSONName);
            }
        }

        JSONClass BuildResetVROrientationPlugin()
        {
            //the plugin
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(" {");
            sb.Append(" \"id\" : \"" + "PluginManager" + "\",");
            sb.Append(" \"plugins\" : " + "{");
            sb.Append(" \"plugin#0\" : \"Custom/Scripts/Reset VR Orientation/ResetVROrientation.cs\" ");
            sb.Append(" }");
            sb.Append(" }");
            Log("Built ResetVROrientation Plugin String: " + sb.ToString());
            return JSONNode.Parse(sb.ToString()).AsObject;
        }

        void Update()
        {
            //once finished loading, apply
            if (SuperController.singleton.isLoading)
            {
                isLoading = true;
                loadingTimeCounter = Time.timeSinceLevelLoad;
            }

            if (isLoading && !SuperController.singleton.isLoading)
            {
                //wait a little, otherwise it reloads plugins multiple times during loading
                if (Time.timeSinceLevelLoad > loadingTimeCounter + 1.0f)
                {
                    isLoading = false;
                    sceneChanged = true;
                }
            }

            if (sceneChanged)
            {
                sceneChanged = false;
                Log("EasyMate Scene Changed, Load Dir: " + SuperController.singleton.currentLoadDir + ", Time Since Level Load: " + Time.timeSinceLevelLoad);
                //get menu data, if this is a menu
                GetMenuData();

                if (mainUIButtons != null)
                    mainUIButtons.InvalidateCachedPersonLists();

                ClearAllPossessionIfLoadedSceneHadAny();

                if (mainUIButtons != null)
                {
                    if (_applyEmotionAfterSceneCo != null)
                    {
                        StopCoroutine(_applyEmotionAfterSceneCo);
                        _applyEmotionAfterSceneCo = null;
                    }

                    _applyEmotionAfterSceneCo = StartCoroutine(CoApplyEmotionAfterSceneSettles());
                }

                if (lastLoadDir != SuperController.singleton.currentLoadDir)
                {
                    Log("Load Dir Changed from " + lastLoadDir + " to " + SuperController.singleton.currentLoadDir);
                    lastLoadDir = SuperController.singleton.currentLoadDir;
                    //reset clothes cycling, etc.
                    if (mainUIButtons != null) mainUIButtons.ClothingResetCycle();
                }

                resetVROrientation = SuperController.singleton.GetAtomByUid(resetVROrientationID);
                if (resetVROrientation == null)
                {
                    Log("EasyMate adding ResetVROrientation Atom.");
                    StartCoroutine(CreateResetVROrientationAtom());
                }
                else
                {
                    ConfigureResetVROrientation();
                }

                ApplyRemoteHoldGrabPreference();
                EasyMateVrInput.ResetEdgeState();
                EasyMateGripHandVisibility.DisableVrHandModelsForSceneStart();
            }

            if (!SuperController.singleton.isLoading)
            {
                EasyMateNxtUiQuestThumbstick.Tick(this);
                if (mainUIButtons != null)
                    mainUIButtons.ProcessHotkeysUpdate();
            }
        }

        void LateUpdate()
        {
            if (SuperController.singleton == null || SuperController.singleton.isLoading)
                return;
            bool blockOverlap = blockOverlapFullGrab != null && blockOverlapFullGrab.val;
            EasyMateOverlapFullGrabRelease.LateTick(blockOverlap);

            bool gripHands = gripTogglesHandVisibility != null && gripTogglesHandVisibility.val;
            EasyMateGripHandVisibility.LateTick(gripHands);

            bool footDist = possessAutoUnpossessWhenFarFromFeet != null && possessAutoUnpossessWhenFarFromFeet.val;
            float footMax = possessAutoUnpossessFeetMaxHorizontalM != null ? possessAutoUnpossessFeetMaxHorizontalM.val : 1.35f;
            EasyMatePossessFootDistanceAutoRelease.LateTick(footDist, footMax);
        }

        void OnDestroy()
        {
            EasyMateHeadSnapPovRuntime.End();
            EasyMateGripHandVisibility.ClearSpankingsFirstHandShowMergeCallback();
            if (mainUIButtons != null) mainUIButtons.OnDestroy();
        }

        //NOT USED, ADDING THE OBJECT, THEN ONLY BUILDING THE PLUGIN STRING, NOT THE WHOLE OBJECT STRING
        string BuildFullResetVROrientationJSON()
        {
            string json = " { \"id\" : \"_ResetVROrientation\", \"on\" : \"true\", \"type\" : \"Empty\", " +
                "\"position\" : { \"x\" : \"0.5\", \"y\" : \"1\", \"z\" : \"0\" }, " +
                "\"rotation\" : { \"x\" : \"0\", \"y\" : \"0\", \"z\" : \"0\" }, " +
                "\"containerPosition\" : { \"x\" : \"0.5\", \"y\" : \"1\", \"z\" : \"0\" }, " +
                "\"containerRotation\" : { \"x\" : \"0\", \"y\" : \"0\", \"z\" : \"0\" }," +
                "\"storables\" : [" +
                "{ \"id\" : \"CollisionTrigger\", \"trigger\" : { \"startActions\" : [ ], \"transitionActions\" : [ ], \"endActions\" : [ ] } }," +
                "{ \"id\" : \"PluginManager\", \"plugins\" : { \"plugin#0\" : \"Custom/Scripts/Reset VR Orientation/ResetVROrientation.cs\" } }," +
                "{ \"id\" : \"control\", \"position\" : { \"x\" : \"0.5\", \"y\" : \"1\", \"z\" : \"0\" }, \"rotation\" : { \"x\" : \"0\", \"y\" : \"0\", \"z\" : \"0\" } }," +
                "{ \"id\" : \"plugin#0_geesp0t.ResetVROrientation\", " +
                "\"Show Additional Button (reload scene to see the change)\" : \"true\"," +
                "\"Additional Button Text\" : \"Looks Page 2\"," +
                "\"Additional Button Scene\" : \"Saves/scene/PersonLooksMenu_2.json\"" +
                " } ] }, } ";

            return json;
        }
    }
}