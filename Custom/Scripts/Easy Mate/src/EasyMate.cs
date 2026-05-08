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

        public JSONStorableAction hideUI;
        public JSONStorableAction showUI;

        public override void Init()
        {
            Log("EasyMate Init");
            mainUIButtons = new MainUIButtons();
            mainUIButtons.Init(this);

            hideUI = new JSONStorableAction("Hide UI", () => HideUI());
            RegisterAction(hideUI);
            showUI = new JSONStorableAction("Show UI", () => ShowUI());
            RegisterAction(showUI);
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
            }
        }

        void OnDestroy()
        {
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