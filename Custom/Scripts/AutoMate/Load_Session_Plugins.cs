using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    public class AddAutoLoad : MVRScript
    {
        //by geesp0t
        //based on SessionPluginBooter by Blazedust

        // Add the Load_Session_Plugins.cs (or ADD_ME_TO_ATOM_IN_DEFAULT_JSON_SCENE.cslist) to one atom in your default.json scene

        // Add all sessions plugins you want to load here
        // Add all person plugins you want to load in SESSION_PLUGINS/Auto_Load_Person_Plugins.cslist

        string[] sessionPlugins = new string[] {
            "Custom/Scripts/AutoMate/SESSION_PLUGINS/Auto_Load_Person_Plugins.cslist",
            "Custom/Scripts/AutoMate/SESSION_PLUGINS/VrTriggerProximityStripClothing.cslist",
            "Custom/Scripts/LocalMp4Viewer/IntroSceneLocalMp4Bootstrap.cslist",
        };

        string[] desktopSessionPlugins = new string[] {
            "Custom/Scripts/prestigitis_DesktopClothGrab.cs"
        };

        public static bool logMessages = false;

        public override void Init()
        {
            bool isDesktopMode = !(SuperController.singleton.isOVR || SuperController.singleton.isOpenVR);
            if (isDesktopMode) { 
                //add the desktop plugins to the set
                sessionPlugins = sessionPlugins.Concat(desktopSessionPlugins).ToArray();
            }
        }

        bool firstRun = false;
        void Update()
        {            
            if (!firstRun)
            {
                firstRun = true;

                if (sessionPlugins == null || sessionPlugins.Length == 0)
                {
                    // No sessionPlugins to run - just ignore the rest then!
                    return;
                }

                Component mainPluginManagerComponent = null;
                foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "PluginManager"))
                {
                    // Log(go.name); // PluginManager

                    Component comp = go.GetComponent("MVRPluginManager");

                    // Component c = go.GetComponent("Atom"); null
                    if (comp != null)
                    {
                        // Log(comp.name); // PluginManager... !?
                        if (comp.transform.parent != null)
                        {
                            if (comp.transform.parent.name == "CoreControl")
                            {
                                // Hmm core control here!
                                // Log("parent: " + comp.transform.parent.name);
                                Component[] comps = comp.transform.parent.GetComponentsInChildren<MVRPluginManager>();
                                mainPluginManagerComponent = comps[0];
                                //foreach (Component pc in comps)
                                //{
                                //    Log("pc: " + pc.name);
                                //}
                                break;
                            }
                            //if (comp.transform.parent.parent != null)
                            //{
                            //    Log("parent.parent: " + comp.transform.parent.parent.name);
                            //}
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
                            Log("Found Session Plugin: " + pluginNode.Value);
                        }

                        List<string> newPlugins = new List<string>();
                        foreach (string desiredPlugin in sessionPlugins)
                        {
                            if (!existingPlugins.Contains(desiredPlugin))
                            {
                                Log("New Session Plugin: " + desiredPlugin.ToString());
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
                                Log("Already has all Session plugins.");
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
                            Log("SessionPluginBooter: Session plugins loaded");
                        }
                        catch (Exception e)
                        {
                            SuperController.LogError("SessionPluginBooter: Failed to load session plugins.");
                            SuperController.LogError(e.ToString());
                        }
                    }
                }
                else
                {
                    Log("SessionPluginBooter: Failed to find PluginManager, no session plugins loaded.");
                }
            }
        }

        void OnDestroy()
        {
            // Log("SessionPluginBooter Destroyed");
        }

        void Log(string msg)
        {
            if (logMessages)
                SuperController.LogMessage(msg);
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
            Log("Built string: " + sb.ToString() + " from: " + pluginList[0]);
            return JSONNode.Parse(sb.ToString()).AsObject;
        }

    }
}