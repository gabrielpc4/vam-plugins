using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace geesp0t
{
    public class GabrielBootstrap : MVRScript
    {
        private static bool logMessages = false;

        private bool addedSessionPlugins = false;

        private string[] sessionPlugins = new string[]
        {
            "Custom/Scripts/Gabriel/features/ui-hud/VaMLogClipboardHud.cslist",
            "Custom/Scripts/Gabriel/features/ui-hud/GabrielHud.cslist",
            "Custom/Scripts/Gabriel/features/dildo-on-hands/DildoOnHands.cslist",
            "Custom/Scripts/Gabriel/session-stack/GabrielSessionStack.cslist",
            "Custom/Scripts/Gabriel/features/clothing-interactions/VrProximityStripClothing.cslist"
        };

        private static readonly string[] DesktopSessionPlugins = new string[]
        {
            "Custom/Scripts/prestigitis_DesktopClothGrab.cs"
        };

        protected JSONStorableString additionalButtonText;
        protected JSONStorableString additionalButtonScene;

        public override void Init()
        {
            bool isDesktopMode;
            Vector3 targetPosition;

            additionalButtonText = new JSONStorableString(
                "Additional Button Text",
                "Looks Menu");
            RegisterString(additionalButtonText);

            additionalButtonScene = new JSONStorableString(
                "Additional Button Scene",
                "Saves/scene/PersonLooksMenu.json");
            RegisterString(additionalButtonScene);

            isDesktopMode =
                !(SuperController.singleton.isOVR ||
                  SuperController.singleton.isOpenVR);

            if (isDesktopMode)
            {
                sessionPlugins =
                    sessionPlugins.Concat(DesktopSessionPlugins).ToArray();
                targetPosition = new Vector3(-0.25f, -1.2f, 3.2f);
            }
            else
            {
                targetPosition = new Vector3(-0.25f, -1.2f, 2.2f);
            }

            if (SuperController.singleton.navigationRig != null)
            {
                SuperController.singleton.navigationRig.position =
                    targetPosition;
            }
        }

        public void Update()
        {
            if (addedSessionPlugins)
            {
                return;
            }

            addedSessionPlugins = true;
            AddSessionPlugins();
        }

        private void AddSessionPlugins()
        {
            Component mainPluginManagerComponent;
            MVRPluginManager manager;
            JSONClass currentJson;
            List<string> existingPlugins;
            List<string> missingPlugins;
            JSONClass pluginJson;

            mainPluginManagerComponent = FindCorePluginManager();
            if (mainPluginManagerComponent == null)
            {
                LogError(
                    "Failed to find PluginManager, no Gabriel session plugins loaded.");
                return;
            }

            manager = (MVRPluginManager)mainPluginManagerComponent;
            currentJson = manager.GetJSON(true, true, true);
            if (currentJson["plugins"] == null ||
                currentJson["plugins"]["plugin#0"] == null ||
                currentJson["plugins"]["plugin#0"].Value == "")
            {
                try
                {
                    pluginJson = CreatePluginJSON(sessionPlugins);
                    manager.LateRestoreFromJSON(pluginJson);
                    Log("Loaded all Gabriel session plugins.");
                }
                catch (Exception e)
                {
                    LogError(
                        "Failed to load Gabriel session plugins: " + e);
                }
                return;
            }

            existingPlugins = new List<string>();
            foreach (JSONNode pluginNode in currentJson["plugins"].Childs)
            {
                existingPlugins.Add(pluginNode.Value);
            }

            missingPlugins = new List<string>();
            foreach (string desiredPlugin in sessionPlugins)
            {
                if (!existingPlugins.Contains(desiredPlugin))
                {
                    missingPlugins.Add(desiredPlugin);
                }
            }

            if (missingPlugins.Count == 0)
            {
                Log("All Gabriel session plugins already loaded.");
                return;
            }

            try
            {
                missingPlugins.AddRange(existingPlugins);
                missingPlugins = missingPlugins.Distinct().ToList();
                pluginJson = CreatePluginJSON(missingPlugins.ToArray());
                manager.LateRestoreFromJSON(pluginJson);
                Log("Added missing Gabriel session plugins.");
            }
            catch (Exception e)
            {
                LogError("Failed to add Gabriel session plugins: " + e);
            }
        }

        private static Component FindCorePluginManager()
        {
            foreach (GameObject gameObject
                in Resources.FindObjectsOfTypeAll<GameObject>()
                    .Where(obj => obj.name == "PluginManager"))
            {
                Component component;
                Component[] components;

                component = gameObject.GetComponent("MVRPluginManager");
                if (component == null || component.transform.parent == null)
                {
                    continue;
                }

                if (component.transform.parent.name != "CoreControl")
                {
                    continue;
                }

                components =
                    component.transform.parent
                        .GetComponentsInChildren<MVRPluginManager>();
                if (components != null && components.Length > 0)
                {
                    return components[0];
                }
            }

            return null;
        }

        private static JSONClass CreatePluginJSON(string[] pluginList)
        {
            System.Text.StringBuilder builder;
            int i;

            builder = new System.Text.StringBuilder();
            builder.Append("{");
            builder.Append(" \"id\": \"PluginManager\",");
            builder.Append(" \"plugins\": {");
            for (i = 0; i < pluginList.Length; i++)
            {
                builder.Append(
                    string.Format(
                        "    \"plugin#{0}\": \"{1}\"{2}",
                        i,
                        pluginList[i],
                        (i + 1) < pluginList.Length ? "," : ""));
            }
            builder.Append("  }");
            builder.Append("}");
            return JSONNode.Parse(builder.ToString()).AsObject;
        }

        private static void Log(string message)
        {
            if (logMessages)
            {
                SuperController.LogMessage(message);
            }
        }

        private static void LogError(string error)
        {
            SuperController.LogError(error);
        }
    }
}
