using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace geesp0t
{
    public class Auto_Load_Spankings_On_All_Female_Persons_Every_Scene
        : MVRScript
    {
        private const string PluginSpankings =
            "Custom/Scripts/Spankings/Spankings.cslist";
        private const string PluginSpankingsFileName = "Spankings.cslist";

        private bool _sceneLoadWaitActive = true;
        private float _sceneLoadDetectedTime;
        private bool _applyPending = true;
        private float _applyNotBeforeUnscaledTime;

        public override void Init()
        {
            SuperController sc = SuperController.singleton;
            if (sc != null)
                sc.onAtomUIDsChangedHandlers += OnAtomUIDsChanged;

            RequestApplyAfterSeconds(0f);
        }

        private void Start()
        {
            _sceneLoadDetectedTime = Time.timeSinceLevelLoad;
            RequestApplyAfterSeconds(0f);
        }

        private void Update()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            if (sc.isLoading)
            {
                _sceneLoadWaitActive = true;
                _sceneLoadDetectedTime = Time.timeSinceLevelLoad;
                RequestApplyAfterSeconds(0f);
                return;
            }

            if (_sceneLoadWaitActive)
            {
                if (Time.timeSinceLevelLoad <= _sceneLoadDetectedTime + 1f)
                    return;

                _sceneLoadWaitActive = false;
                RequestApplyAfterSeconds(0f);
            }

            if (!_applyPending)
                return;

            if (Time.unscaledTime < _applyNotBeforeUnscaledTime)
                return;

            if (TryEnsureSpankingsOnAllFemalePersons())
            {
                _applyPending = false;
                return;
            }

            RequestApplyAfterSeconds(0.5f);
        }

        private void OnDestroy()
        {
            SuperController sc = SuperController.singleton;
            if (sc != null)
                sc.onAtomUIDsChangedHandlers -= OnAtomUIDsChanged;
        }

        private void OnAtomUIDsChanged(List<string> atomUids)
        {
            if (atomUids == null || atomUids.Count == 0)
                return;

            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            for (int i = 0; i < atomUids.Count; i++)
            {
                Atom atom = sc.GetAtomByUid(atomUids[i]);
                if (atom != null && atom.type == "Person")
                {
                    RequestApplyAfterSeconds(0.25f);
                    return;
                }
            }
        }

        private void RequestApplyAfterSeconds(float seconds)
        {
            _applyPending = true;
            _applyNotBeforeUnscaledTime = Time.unscaledTime + seconds;
        }

        private bool TryEnsureSpankingsOnAllFemalePersons()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return false;

            bool allReady = true;
            List<Atom> personAtoms = sc.GetAtoms()
                .Where(a => a != null && a.type == "Person")
                .ToList();

            for (int i = 0; i < personAtoms.Count; i++)
            {
                Atom atom = personAtoms[i];
                if (!IsFemalePerson(atom))
                    continue;

                MVRPluginManager manager =
                    atom.GetStorableByID("PluginManager") as MVRPluginManager;
                if (manager == null)
                {
                    allReady = false;
                    continue;
                }

                if (PersonHasPluginByFileName(manager, PluginSpankingsFileName))
                    continue;

                try
                {
                    MergePluginOntoPerson(manager, PluginSpankings);
                }
                catch (Exception e)
                {
                    allReady = false;
                    SuperController.LogError(
                        "Auto Load Spankings Females: " + atom.name +
                        " merge failed: " + e.Message);
                }
            }

            return allReady;
        }

        private static bool IsFemalePerson(Atom atom)
        {
            if (atom == null || atom.type != "Person")
                return false;

            DAZCharacter character = atom.GetComponentInChildren<DAZCharacter>();
            return character != null && !character.isMale;
        }

        private static bool PersonHasPluginByFileName(
            MVRPluginManager manager,
            string fileName)
        {
            if (manager == null || string.IsNullOrEmpty(fileName))
                return false;

            JSONClass current = manager.GetJSON(true, true, true);
            if (current == null || current["plugins"] == null)
                return false;

            foreach (JSONNode pluginNode in current["plugins"].Childs)
            {
                string pluginPath = pluginNode.Value;
                if (string.IsNullOrEmpty(pluginPath))
                    continue;

                if (string.Equals(
                    GetFileName(pluginPath),
                    fileName,
                    StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void MergePluginOntoPerson(
            MVRPluginManager manager,
            string pluginPath)
        {
            List<string> plugins = new List<string>();
            JSONClass current = manager.GetJSON(true, true, true);

            if (current != null &&
                current["plugins"] != null &&
                current["plugins"]["plugin#0"] != null &&
                current["plugins"]["plugin#0"].Value != "")
            {
                foreach (JSONNode pluginNode in current["plugins"].Childs)
                {
                    if (!string.IsNullOrEmpty(pluginNode.Value))
                        plugins.Add(pluginNode.Value);
                }
            }

            plugins.Add(pluginPath);
            manager.LateRestoreFromJSON(CreatePluginJson(plugins.ToArray()));
        }

        private static JSONClass CreatePluginJson(string[] pluginList)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{");
            sb.Append(" \"id\": \"PluginManager\",");
            sb.Append(" \"plugins\": {");
            for (int i = 0; i < pluginList.Length; i++)
            {
                sb.Append(string.Format(
                    "    \"plugin#{0}\": \"{1}\"{2}",
                    i.ToString(),
                    pluginList[i],
                    (i + 1) < pluginList.Length ? "," : ""));
            }
            sb.Append("  }");
            sb.Append("}");
            return JSONNode.Parse(sb.ToString()).AsObject;
        }

        private static string GetFileName(string relativePath)
        {
            int index = relativePath.LastIndexOfAny(new[] { '/', '\\' });
            if (index < 0 || index >= relativePath.Length - 1)
                return relativePath;

            return relativePath.Substring(index + 1);
        }
    }
}
