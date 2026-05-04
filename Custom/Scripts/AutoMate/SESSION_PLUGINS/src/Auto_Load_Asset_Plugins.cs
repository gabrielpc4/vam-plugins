using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    public class Auto_Load_Asset_Plugins : MVRScript
    {
        //by geesp0t
        //based on SessionPluginBooter by Blazedust
        //using SessionPluginBooster mod code to add person plugins by ch3apsk8r
        //using concepts from PluginAssist by JayJayWon



        //Any plugins you want to be loaded on all Unity assets should be placed here
        string[] assetPlugins = new string[] {
            "AddonPackages/NoStage3.UnityAssetVamifier.19.var:/Custom/Scripts/NoStage3/UnityAssetVamifier.cs" //VAR Package script example, to find the path, copy the .var to a .zip and open to see the path
            //"Custom/Scripts/UnityAssetVamifier.cs" //Regular script path example
        };

        public static bool logMessages = false;



        private string atomType = "CustomUnityAsset";

        List<string> atomNames = new List<string>();

        private bool appliedPlugins = false;

        private bool loadedSettings = false;

        private float lastLoading = 0;

        
        public override void Init()
        {
            SuperController.singleton.onAtomUIDsChangedHandlers += new SuperController.OnAtomUIDsChanged(this.AtomUIDChange);
        }
       
        protected void AtomUIDChange(List<string> atomUIDs)
        {
            try
            {
                foreach (string atomName in atomUIDs)
                {
                    Atom atom = SuperController.singleton.GetAtomByUid(atomName);
                    if (atom != null && atom.type == atomType)
                    {
                        Log("CustomUnityAsset: " + atom.name);
                        if (atomNames != null && !atomNames.Contains(atomName))
                        {
                            Log("Adding CustomUnityAsset: " + atom.name);
                            appliedPlugins = false;
                            atomNames.Add(atomName);
                        }
                    }
                }
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }
        
        void Update()
        {
            //once finished loading, apply person plugins
            if (SuperController.singleton.isLoading || loadedSettings) { 
                appliedPlugins = false;
                lastLoading = Time.timeSinceLevelLoad;
            }

            if (!appliedPlugins && !SuperController.singleton.isLoading)
            {
                //wait a little, otherwise it reloads plugins multiple times during loading
                if (Time.timeSinceLevelLoad > lastLoading + 1.0f) { 
                    Log("Applying CustomUnityAsset Plugins");
                    appliedPlugins = true;
                    LoadPersonPlugins();
                }
            }
        }

        void OnDestroy()
        {
            SuperController.singleton.onAtomUIDsChangedHandlers -= new SuperController.OnAtomUIDsChanged(this.AtomUIDChange);
        }

        void Log(string msg)
        {
            if (logMessages)
                SuperController.LogMessage(msg);
        }

        void Start()
        {
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

        public string GetFileName(string relativePath)
        {
            string fileName = relativePath.Substring(relativePath.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            return fileName;
        }
        
        void LoadPersonPlugins()
        {
            IEnumerable<Atom> atoms = SuperController.singleton.GetAtoms().Where(a => a.type == atomType);

            foreach (Atom at in atoms)
            {
                //which plugins should we add?
                List<string> desiredPlugins = new List<string>();
                foreach (string pluginString in assetPlugins)
                {
                    desiredPlugins.Add(pluginString);
                }
                
                desiredPlugins = desiredPlugins.Distinct().ToList();
                Log("Num desired plugins: " + desiredPlugins.Count);

                MVRPluginManager manager = at.GetStorableByID("PluginManager") as MVRPluginManager;
                JSONClass current = manager.GetJSON(true, true, true);
                if (current["plugins"] != null && current["plugins"]["plugin#0"] != null && current["plugins"]["plugin#0"].Value != "")
                {

                    //make new list
                    List<string> existingPlugins = new List<string>();
                    foreach (JSONNode pluginNode in current["plugins"].Childs)
                    {
                        existingPlugins.Add(pluginNode.Value);
                        Log("Found Plugin: " + pluginNode.Value);
                    }

                    List<string> newPlugins = new List<string>();
                    foreach (string desiredPlugin in assetPlugins)
                    {
                        if (!existingPlugins.Contains(desiredPlugin))
                        {
                            Log("New Plugin: " + desiredPlugin.ToString());
                            newPlugins.Add(desiredPlugin);
                        }
                    }

                    try
                    {
                        if (newPlugins.Count() > 0)
                        {
                            Log("Adding Missing Plugins");
                            JSONClass jc = CreatePluginJSON(newPlugins.ToArray());
                            Log(jc.ToString());
                            manager.LateRestoreFromJSON(jc);
                        }
                        else
                        {
                            Log("Already has all plugins.");
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
                        JSONClass jc = CreatePluginJSON(assetPlugins);
                        Log(jc.ToString());
                        manager.LateRestoreFromJSON(jc);
                        Log("SessionPluginBooter: Session plugins loaded");
                    }
                    catch (Exception e)
                    {
                        SuperController.LogError("SessionPluginBooter: Failed to load session plugins.");
                        SuperController.LogError(e.ToString());
                    }
                }
            }
        }

    }
}