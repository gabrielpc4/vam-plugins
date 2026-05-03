using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    public class AutoLoadEasyMate : MVRScript
    {
        //by geesp0t
        //based on SessionPluginBooter by Blazedust

        private static bool logMessages = false;

        private static bool addedSessionPlugins = false;
        string[] sessionPlugins = new string[] {
            "Custom/Scripts/Easy Mate/VaMLogClipboardHud.cslist",
            "Custom/Scripts/Easy Mate/EasyMate.cslist",
            "Custom/Scripts/DildoOnHands/DildoOnHands.cslist",
            "Custom/Scripts/AutoMate/SESSION_PLUGINS/Auto_Load_Person_Plugins.cslist",
        };

        string[] desktopSessionPlugins = new string[] {
            "Custom/Scripts/prestigitis_DesktopClothGrab.cs"
        };

        protected JSONStorableString _additionalButtonText;
        protected JSONStorableString _additionalButtonScene;

        public override void Init()
        {
            _additionalButtonText = new JSONStorableString("Additional Button Text", "Looks Menu");
            RegisterString(_additionalButtonText);

            _additionalButtonScene = new JSONStorableString("Additional Button Scene", "Saves/scene/PersonLooksMenu.json");
            RegisterString(_additionalButtonScene);
            bool isDesktopMode = !(SuperController.singleton.isOVR || SuperController.singleton.isOpenVR);
            if (isDesktopMode)
            {
                sessionPlugins = sessionPlugins.Concat(desktopSessionPlugins).ToArray();
                Vector3 targetPosition = new Vector3(-0.25f, -1.2f, 3.2f);
                SuperController.singleton.navigationRig.position = targetPosition;
            } else
            {
                Vector3 targetPosition = new Vector3(-0.25f, -1.2f, 2.2f);
                SuperController.singleton.navigationRig.position = targetPosition;
            }
        }

        public void Update()
        {
            //if we arrive at a menu, make sure we have all required session plugins
            if (!addedSessionPlugins)
            {
                addedSessionPlugins = true;
                AddSessionPlugins();
            }
        }

        void AddSessionPlugins()
        {
            Component mainPluginManagerComponent = null;
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "PluginManager"))
            {
                Component comp = go.GetComponent("MVRPluginManager");
                if (comp != null)
                {
                    if (comp.transform.parent != null)
                    {
                        if (comp.transform.parent.name == "CoreControl")
                        {
                            // Core control here!
                            Component[] comps = comp.transform.parent.GetComponentsInChildren<MVRPluginManager>();
                            mainPluginManagerComponent = comps[0];
                            break;
                        }
                    }
                }
            }

            if (mainPluginManagerComponent != null)
            {
                MVRPluginManager man = ((MVRPluginManager)mainPluginManagerComponent);

                JSONClass currentJc = man.GetJSON(true, true, true);
                if (currentJc["plugins"] != null && currentJc["plugins"]["plugin#0"] != null && currentJc["plugins"]["plugin#0"].Value != "")
                {

                    //make new list
                    List<string> existingPlugins = new List<string>();
                    foreach (JSONNode pluginNode in currentJc["plugins"].Childs)
                    {
                        existingPlugins.Add(pluginNode.Value);
                    }

                    List<string> newPlugins = new List<string>();
                    foreach (string desiredPlugin in sessionPlugins)
                    {
                        if (!existingPlugins.Contains(desiredPlugin))
                        {
                            Log("Adding Easy Mate Session Plugin: " + desiredPlugin.ToString());
                            newPlugins.Add(desiredPlugin);
                        }
                    }

                    try
                    {
                        if (newPlugins.Count() > 0)
                        {
                            Log("Adding Missing Session Plugins");
                            newPlugins.AddRange(existingPlugins);
                            newPlugins = newPlugins.Distinct().ToList();
                            JSONClass jc = CreatePluginJSON(newPlugins.ToArray());
                            Log(jc.ToString());
                            man.LateRestoreFromJSON(jc);
                        }
                        else
                        {
                            Log("Already has all Easy Mate Session plugins.");
                        }
                    }
                    catch (Exception e)
                    {
                        SuperController.LogError("Failed to load plugin");
                        SuperController.LogError(e.ToString());
                    }
                }
                else
                {
                    // Initialize boot of session plugins!
                    try
                    {
                        JSONClass jc = CreatePluginJSON(sessionPlugins);
                        Log(jc.ToString());
                        man.LateRestoreFromJSON(jc);
                        Log("Adding all Easy Mate Session Plugins");
                    }
                    catch (Exception e)
                    {
                        LogError("Failed to Easy Mate Session Plugins.");
                        LogError(e.ToString());
                    }
                }
            }
            else
            {
                LogError("Failed to find PluginManager, no Easy Mate Session Plugins Loaded.");
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
    }
}