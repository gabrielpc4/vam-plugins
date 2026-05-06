using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    public class Auto_Load_Person_Plugins : MVRScript
    {
        //by geesp0t
        //based on SessionPluginBooter by Blazedust
        //using SessionPluginBooster mod code to add person plugins by ch3apsk8r
        //using concepts from PluginAssist by JayJayWon

        // Add the Load_Session_Plugins.cs (or ADD_ME_TO_ATOM_IN_DEFAULT_JSON_SCENE.cslist) to one atom in your default.json scene

        // Add all sessions plugins you want to load in Load_Session_Plugins.cs
        // Add all person plugins you want to load here

        const string SETTINGS_FILE_PATH = "Custom/Scripts/AutoMate";
        const string SETTINGS_FILE_NAME = "SETTINGS.json";

        List<string> femalePlugins = new List<string>();
        List<string> femaleSoloPlugins = new List<string>();
        List<string> malePlugins = new List<string>();

        private bool appliedPersonPlugins = false;

        private bool personPluginReloadPending = false;

        public static bool logMessages = false;

        protected JSONStorableString explanationString;

        public JSONStorableAction hideUI;
        public JSONStorableAction showUI;

        protected KeyboardShortcuts keyboardShortcuts = null;

        private bool sceneChanged = false;
        private string lastSceneName = "";
        private bool isLoading = true;
        private float loadingTimeCounter = 0;
        private bool prevSuperControllerIsLoading = false;
        private bool suppressSpankingsForPendingSceneLoad = false;
        private SameFolderSceneLoadCheck sameFolderSceneLoadCheck =
            new SameFolderSceneLoadCheck();

        private OnSceneStartup onSceneStartup = new OnSceneStartup();

        //to place a male atom in a scene (if starting with a female look scene)
        protected Atom createdMaleAtom = null;
        protected String createdMaleName = "Auto_Load_Male_Person";

        private bool wantToSetAppearance = false;
        private bool useImprovedPOVLicking = false;
        private Atom maleAtomLicking = null;

        private const string PLUGIN_LIFE = "AddonPackages/MacGruber.Life.6.var:/Custom/Scripts/MacGruber/Life/MacGruber_Life.cslist";
        private const string PLUGIN_SPANKINGS = "Custom/Scripts/Spankings/Spankings.cslist";
        private const string PLUGIN_EXPLOSION_LIMITER = "Custom/Scripts/ExplosionLimiter-ns.cs";
        private const string PLUGIN_KISS = "Custom/Scripts/Kiss5.cs";
        private const string PLUGIN_EASY_MOAN = "Custom/Scripts/Easy Moan/EasyMoan.cslist";
        private const string PLUGIN_IMPROVED_POV = "Custom/Scripts/ImprovedPoV.cs";
        private const string PLUGIN_IMPROVED_POV_TONGUE = "Custom/Scripts/ImprovedPoV_TongueLicking.cs";
        private const string PLUGIN_E_MOTION = "Custom/Scripts/AutoMate/PERSON_PLUGINS/E-Motion - VaM Auto Blink/E-Motion_AddThisONLY.cslist";
        private const string PLUGIN_DOLLMASTER = "Custom/Scripts/VAMDeluxe/Dollmaster/ADD_ME.cslist";
        private const string PLUGIN_EASY_BACKGROUND_SOUNDS = "Custom/Scripts/Easy Background Sounds/EasyBackgroundSounds.cs";
        private const string PLUGIN_VAM_LAUNCH = "Custom/Scripts/VAMLaunch/ADD_ME.cslist";
        private const string PLUGIN_EASY_MATE_CLOTHING_TOUCH_FALLOFF = "Custom/Scripts/Easy Mate/EasyMateClothingTouchFallOff.cslist";
        private const string PLUGIN_POSSESS_SEX = "Custom/Scripts/Possess Sex/PossessSex.cs";

        private List<string> pluginsThatShouldBeAddedOnlyOnce = new List<string>();
        private List<string> managedFemalePluginPaths = new List<string>()
        {
            PLUGIN_EXPLOSION_LIMITER,
        };
        private List<string> managedMalePluginPaths = new List<string>()
        {
            PLUGIN_EXPLOSION_LIMITER,
            PLUGIN_IMPROVED_POV,
            PLUGIN_IMPROVED_POV_TONGUE
        };

        private const string PLUGIN_VAM_LAUNCH_CLASS_NAME = "VAMLaunchPlugin.VAMLaunch";
        private const string PLUGIN_VAM_LAUNCH_ATOM_NAME = "VAM Launch";
        private Atom vamLaunchAtom = null;

        void SetDefaultSettings()
        {
            femalePlugins.Clear();
            femaleSoloPlugins.Clear();
            malePlugins.Clear();

            // For now, I don't want to load any plugins by default.
            // femalePlugins.Add(PLUGIN_EXPLOSION_LIMITER);
            // femalePlugins.Add(PLUGIN_SPANKINGS);

            // femaleSoloPlugins.Add(PLUGIN_EASY_MOAN);
            // femaleSoloPlugins.Add(PLUGIN_EASY_BACKGROUND_SOUNDS);

            // malePlugins.Add(PLUGIN_EXPLOSION_LIMITER);
            // malePlugins.Add(PLUGIN_IMPROVED_POV);

            appliedPersonPlugins = false;
        }

        public override void Init()
        {
            hideUI = new JSONStorableAction("Hide UI", () => HideUI());
            RegisterAction(hideUI);
            showUI = new JSONStorableAction("Show UI", () => ShowUI());
            RegisterAction(showUI);
            explanationString = new JSONStorableString("", "Each time a scene is loaded, or a Person atom is added, plugins from SETTINGS.json are added to Person atoms.\n\n" +
                "AutoMate now only manages ExplosionLimiter and ImprovedPoV entries from Custom/Scripts/AutoMate/SETTINGS.json.\n\n" +
                "Female plugins apply to all female atoms, male plugins apply to all male atoms, and female solo plugins apply only when there is one Person in the scene.\n\n" +
                "Press Remove All Plugins on Person Atoms if you want to clear every Person atom plugin manager and reset expression morphs."
                );
            UIDynamicTextField dtext = CreateTextField(explanationString);
            dtext.height = 650;


            CreateButton("Remove All Plugins on Person Atoms", true).button.onClick.AddListener(() =>
            {
                RemoveAllPersonPlugins();
                ResetMorphs();
            });

            SuperController.singleton.onAtomUIDsChangedHandlers += new SuperController.OnAtomUIDsChanged(this.AtomUIDChange);


            keyboardShortcuts = new KeyboardShortcuts();

            keyboardShortcuts.Init(this);

            createdMaleAtom = SuperController.singleton.GetAtomByUid(createdMaleName);
        }

        public void ShowUI()
        {
        }
        public void HideUI()
        {
        }


        protected void AtomUIDChange(List<string> atomUIDs)
        {
            try
            {
                createdMaleAtom = SuperController.singleton.GetAtomByUid(createdMaleName);
                if (createdMaleAtom != null)
                {
                    Log("Create Male Atom: AtomUIDChange wantToSetAppearance = true");
                    wantToSetAppearance = true;
                    isLoading = true;
                    loadingTimeCounter = Time.timeSinceLevelLoad;
                }

                bool sawPerson = false;
                foreach (string atomName in atomUIDs)
                {
                    Atom atom = SuperController.singleton.GetAtomByUid(atomName);

                    if (atom != null && atom.type == "Person")
                    {
                        sawPerson = true;
                        appliedPersonPlugins = false;
                    }
                }

                if (sawPerson)
                    personPluginReloadPending = true;
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        //Button for opening Plugin Manager UI, along with Auto Load Plugins for All Scenes with fields showing the current list of
        //plugins plus buttons, save current Session Plugins, save current Female Person Plugins, save current Male Person Plugins, save current Female Person Plugins as Female Solo Plugins
        /*List<string> GetExistingPlugins()
        {
        }*/

        private bool IsAllowedConfiguredPluginPath(string pluginPath, bool isMale)
        {
            if (string.IsNullOrEmpty(pluginPath))
                return false;

            if (pluginPath == PLUGIN_EXPLOSION_LIMITER)
                return true;

            if (isMale &&
                (pluginPath == PLUGIN_IMPROVED_POV ||
                 pluginPath == PLUGIN_IMPROVED_POV_TONGUE))
            {
                return true;
            }

            return false;
        }

        private void LoadConfiguredPluginArray(
            JSONClass savedSettings,
            string key,
            List<string> target,
            bool isMale)
        {
            JSONArray pluginsArray;
            int i;
            string pluginPath;

            target.Clear();
            if (savedSettings == null || savedSettings[key] == null)
                return;

            pluginsArray = savedSettings[key].AsArray;
            if (pluginsArray == null)
                return;

            for (i = 0; i < pluginsArray.Count; i++)
            {
                pluginPath = pluginsArray[i];
                if (!IsAllowedConfiguredPluginPath(pluginPath, isMale))
                    continue;
                if (!target.Contains(pluginPath))
                    target.Add(pluginPath);
            }
        }

        void LoadSettingsFrom(string path)
        {
            JSONClass savedSettings;

            savedSettings = SuperController.singleton.LoadJSON(path).AsObject;
            if (savedSettings == null)
            {
                Log("Saved Settings not found");
                SetDefaultSettings();
                return;
            }

            LoadConfiguredPluginArray(
                savedSettings,
                "femalePlugins",
                femalePlugins,
                false);
            LoadConfiguredPluginArray(
                savedSettings,
                "femaleSoloPlugins",
                femaleSoloPlugins,
                false);
            LoadConfiguredPluginArray(
                savedSettings,
                "malePlugins",
                malePlugins,
                true);

            appliedPersonPlugins = false;
        }

        void LoadSettingsFromDefaultFile()
        {
            LoadSettingsFrom(SETTINGS_FILE_PATH + "/" + SETTINGS_FILE_NAME);
        }

        //DOESN'T YET WORK AND DON'T KNOW IF WE NEED IT
        //void StoreOriginalPluginSet()
        //{
        //    //storing names only, not paths
        //    Log("STORING SCENE DEFAULT PLUGINS");

        //    originalFemalePluginNames.Clear();
        //    originalFemalePluginPaths.Clear();
        //    originalMalePluginNames.Clear();
        //    originalMalePluginPaths.Clear();

        //    IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

        //    foreach (Atom at in personAtoms)
        //    {
        //        MVRPluginManager manager = at.GetStorableByID("PluginManager") as MVRPluginManager;
        //        JSONClass current = manager.GetJSON(true, true, true);
        //        if (current["plugins"] != null && current["plugins"]["plugin#0"] != null &&
        //            current["plugins"]["plugin#0"].Value != "")
        //        {
        //            foreach (JSONNode pluginNode in current["plugins"].Childs)
        //            {
        //                string pluginPath = pluginNode.Value;
        //                if (pluginPath.StartsWith("./"))
        //                {
        //                    //make relative to saves dir
        //                    pluginPath = SuperController.singleton.currentSaveDir + "/" + pluginPath.Substring(2);
        //                    //Log("Fixing Relative Plugin Path: " + pluginPath);
        //                }
        //                else if (pluginPath.LastIndexOfAny(new char[] { '/', '\\' }) < 0)
        //                {
        //                    //no path, just a file name, add the current directory
        //                    pluginPath = SuperController.singleton.currentSaveDir + "/" + pluginPath;
        //                    //Log("Including Full Plugin Path: " + pluginPath);
        //                }

        //                if (at.GetComponentInChildren<DAZCharacter>().isMale)
        //                {
        //                    originalMalePluginPaths.Add(pluginPath);
        //                    originalMalePluginNames.Add(GetFileName(pluginPath));
        //                    Log("Saving Original Male Plugin: " + pluginPath);
        //                }
        //                else
        //                {
        //                    originalFemalePluginPaths.Add(pluginPath);
        //                    originalFemalePluginNames.Add(GetFileName(pluginPath));
        //                    Log("Saving Original Female Plugin: " + pluginPath);
        //                }
        //            }
        //        }
        //    }
        //}


        public void Update()
        {
            if (keyboardShortcuts != null)
                keyboardShortcuts.ProcessHotkeysUpdate();

            bool superLoadingNow = SuperController.singleton.isLoading;
            if (!prevSuperControllerIsLoading && superLoadingNow)
            {
                suppressSpankingsForPendingSceneLoad =
                    sameFolderSceneLoadCheck.IsSameFolderLoad(
                        SuperController.singleton);
            }
            else if (!superLoadingNow)
            {
                sameFolderSceneLoadCheck.CaptureIdleLoadDir(
                    SuperController.singleton);
            }
            prevSuperControllerIsLoading = superLoadingNow;

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

            // sceneChanged must stay true until we decide to run LoadPersonPlugins; the old code cleared it
            // in the first block so the second "if (sceneChanged && ...)" never ran (silent skip after scene load).
            if (sceneChanged && !SuperController.singleton.isLoading)
            {
                Log("Scene finished loading (session plugin)");
                sceneChanged = false;
                appliedPersonPlugins = false;

                CheckMaleForImprovedPOV();

                if (!wantToSetAppearance)
                {
                    appliedPersonPlugins = true;
                    Log("load person plugins after scene load (Update)");
                    try
                    {
                        LoadPersonPlugins(
                            false,
                            suppressSpankingsForPendingSceneLoad);
                        if (!personPluginReloadPending)
                            suppressSpankingsForPendingSceneLoad = false;
                    }
                    catch (Exception e)
                    {
                        SuperController.LogError("[Auto_Load_Person_Plugins] LoadPersonPlugins after scene load failed: " + e);
                    }
                }
            }

            if (wantToSetAppearance && !SuperController.singleton.isLoading)
            {
                Log("Create Male Atom: Update calls SetupMaleAtom");
                wantToSetAppearance = false;
                SetupMaleAtom();
            }

            if (personPluginReloadPending && !wantToSetAppearance && !SuperController.singleton.isLoading)
            {
                personPluginReloadPending = false;
                try
                {
                    LoadPersonPlugins(
                        false,
                        suppressSpankingsForPendingSceneLoad);
                    suppressSpankingsForPendingSceneLoad = false;
                }
                catch (Exception e)
                {
                    SuperController.LogError("[Auto_Load_Person_Plugins] LoadPersonPlugins (deferred atom change) failed: " + e);
                }
            }
        }

        void LateUpdate()
        {
            bool sceneSettleJustEnded =
                onSceneStartup.TickDuringSuperControllerLoad(
                    suppressSpankingsForPendingSceneLoad);
            if (sceneSettleJustEnded)
            {
                EasyMateVrHeadCylinderHide.AfterSuperControllerFinishedSceneSettle(this);
            }
        }

        void FindMaleAtomLicking(bool adjustUIButtons)
        {
            Log("FindMaleAtomLicking.");
            maleAtomLicking = null;
            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            Atom impPOV = null;
            Atom impPOVTongue = null;

            foreach (Atom at in personAtoms)
            {
                Log("Person: " + at.name);
                bool isMale = at.GetComponentInChildren<DAZCharacter>().isMale;
                if (at.uid == createdMaleName) isMale = true; //sometimes the created atom wasn't being seen as male

                if (isMale)
                {
                    maleAtomLicking = at;

                    if (FindPluginInAtom("ImprovedPoV", maleAtomLicking) != null)
                    {
                        impPOV = at;
                    }
                    else if (FindPluginInAtom("ImprovedPoV_TongueLicking", maleAtomLicking) != null)
                    {
                        impPOVTongue = at;
                    }
                }
            }

            //prefer a male that has the tongue licking version, or at least improved pov
            if (impPOVTongue != null)
            {
                Log("Male is licking.");
                maleAtomLicking = impPOVTongue;
                useImprovedPOVLicking = true;
            }
            else if (impPOV != null)
            {
                Log("Male isn't licking but has ImprovedPoV.");
                maleAtomLicking = impPOV;
                useImprovedPOVLicking = false;
            }
            else if (maleAtomLicking != null)
            {
                Log("Male isn't licking and doesn't have ImprovedPoV.");
                useImprovedPOVLicking = false;
            }
            else
            {
                Log("No male found.");
            }
        }

        void CheckMaleForImprovedPOV()
        {   
            //if we have a male, check if he has improved_pov   
            //if he does, then show button to go to tongue licking, if he does not show button to add improved pov, if no male hide button

            FindMaleAtomLicking(true);
        }

        void OnDestroy()
        {
            onSceneStartup.OnOwningPluginDestroy();

            SuperController.singleton.onAtomUIDsChangedHandlers -= new SuperController.OnAtomUIDsChanged(this.AtomUIDChange);
            if (keyboardShortcuts != null)
                keyboardShortcuts.OnDestroy();
            // Log("SessionPluginBooter Destroyed");
        }

        void Log(string msg)
        {
            if (logMessages)
                SuperController.LogMessage(msg);
        }

        void Start()
        {
            isLoading = true;
            loadingTimeCounter = Time.timeSinceLevelLoad;
            SetDefaultSettings();
            LoadSettingsFromDefaultFile();
        }

        void RemoveAllPersonPlugins()
        {
            Log("Removing All Person Plugins.");
            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            foreach (Atom at in personAtoms)
            {
                MVRPluginManager manager = at.GetStorableByID("PluginManager") as MVRPluginManager;
                string emptyPluginManager = "{ \"id\" : \"PluginManager\", \"plugins\" : { } }";
                JSONClass jc = JSONNode.Parse(emptyPluginManager).AsObject;
                manager.LateRestoreFromJSON(jc);
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

        public bool FileExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || SuperController.singleton == null)
                return false;

            int folderSeparatorIndex = relativePath.LastIndexOfAny(new char[] { '/', '\\' });
            if (folderSeparatorIndex <= 0)
                return false;

            string pathFolder = relativePath.Substring(0, folderSeparatorIndex);
            string pathFile = relativePath.Substring(folderSeparatorIndex + 1);
            if (string.IsNullOrEmpty(pathFolder) || string.IsNullOrEmpty(pathFile))
                return false;

            string[] pathFileList = null;
            try
            {
                pathFileList = SuperController.singleton.GetFilesAtPath(pathFolder);
            }
            catch (Exception)
            {
                return false;
            }

            Log("Checking folder: " + pathFolder + " for file: " + pathFile);
            if (pathFileList != null && pathFileList.Length > 0)
                Log("Folder contains files: " + string.Join(", ", pathFileList));

            if (pathFileList != null && pathFileList.Length > 0)
            {
                foreach (string foundPathFile in pathFileList)
                {
                    string correctedPathFile = foundPathFile.Replace("\\", "/");
                    if (correctedPathFile.EndsWith("/" + pathFile))
                    {
                        Log("FOUND FILE: " + pathFolder + "/" + pathFile);
                        return true;
                    }
                }
            }

            return false;
        }

        public string GetFileName(string relativePath)
        {
            string fileName = relativePath.Substring(relativePath.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            return fileName;
        }

        private bool IsManagedPluginPath(string pluginPath, bool isMale)
        {
            if (isMale)
            {
                return managedMalePluginPaths.Contains(pluginPath);
            }

            return managedFemalePluginPaths.Contains(pluginPath) ||
                femalePlugins.Contains(pluginPath) ||
                femaleSoloPlugins.Contains(pluginPath);
        }

        /// <summary>
        /// Our patched plugin path only — hub/var copies of <c>ImprovedPoV.cs</c> are replaced.
        /// </summary>
        private bool IsOurModifiedImprovedPoVPath(string pluginPath)
        {
            if (string.IsNullOrEmpty(pluginPath))
                return false;

            string normalized;

            normalized = pluginPath.Replace('\\', '/');
            return normalized == PLUGIN_IMPROVED_POV;
        }

        /// <summary>
        /// Any script file named <c>ImprovedPoV.cs</c> (not Tongue/Licking variants).
        /// </summary>
        private bool IsImprovedPoVScriptFile(string pluginPath)
        {
            if (string.IsNullOrEmpty(pluginPath))
                return false;

            string fileName;

            fileName = GetFileName(pluginPath);
            return string.Equals(
                fileName,
                "ImprovedPoV.cs",
                StringComparison.OrdinalIgnoreCase);
        }

        public void StartLicking()
        {

            //find the first male
            FindMaleAtomLicking(false);

            //start licking
            useImprovedPOVLicking = true;

            if (maleAtomLicking != null)
            {
                malePlugins.Remove(PLUGIN_IMPROVED_POV);
                malePlugins.Add(PLUGIN_IMPROVED_POV_TONGUE);

                malePlugins = malePlugins.Distinct().ToList();

                Log("load person plugins from startlicking");
                LoadPersonPlugins(true);
            }
        }

        public void StopLicking()
        {
            //find the first male
            FindMaleAtomLicking(false);

            //stop licking
            useImprovedPOVLicking = false;
            
            if (maleAtomLicking != null)
            {

                malePlugins.Add(PLUGIN_IMPROVED_POV);
                malePlugins.Remove(PLUGIN_IMPROVED_POV_TONGUE);

                malePlugins = malePlugins.Distinct().ToList();

                Log("load person plugins from stop licking");
                LoadPersonPlugins(true);

                JSONStorable geometry = maleAtomLicking.GetStorableByID("geometry");
                DAZCharacterSelector character = geometry as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;

                DAZMorph tongueLength = morphControl.GetMorphByDisplayName("Tongue Length");
                DAZMorph tongueRaiseLower = morphControl.GetMorphByDisplayName("Tongue Raise-Lower");
                DAZMorph tongueRoll1 = morphControl.GetMorphByDisplayName("Tongue Roll 1");
                DAZMorph tongueTwist = morphControl.GetMorphByDisplayName("Tongue Twist");
                DAZMorph tongueNarrowWide = morphControl.GetMorphByDisplayName("Tongue Narrow-Wide");
                DAZMorph tongueUpDown = morphControl.GetMorphByDisplayName("Tongue Up-Down");
                DAZMorph tongueBendTip = morphControl.GetMorphByDisplayName("Tongue Bend Tip");

                if (tongueLength != null)
                {
                    tongueLength.SetValue(0);
                    tongueLength.Reset();
                }
                if (tongueRaiseLower != null)
                {
                    tongueRaiseLower.SetValue(0);
                    tongueRaiseLower.Reset();
                }
                if (tongueRoll1 != null)
                {
                    tongueRoll1.SetValue(0);
                    tongueRoll1.Reset();
                }
                if (tongueTwist != null)
                {
                    tongueTwist.SetValue(0);
                    tongueTwist.Reset();
                }
                if (tongueNarrowWide != null)
                {
                    tongueNarrowWide.SetValue(0);
                    tongueNarrowWide.Reset();
                }
                if (tongueUpDown != null)
                {
                    tongueUpDown.SetValue(0);
                    tongueUpDown.Reset();
                }
                if (tongueBendTip != null)
                {
                    tongueBendTip.SetValue(0);
                    tongueBendTip.Reset();
                }

                Log("finished resetting male tongue morphs");
            }
        }

        void LoadPersonPlugins(
            bool maleOnly = false,
            bool suppressSpankingsForThisCall = false)
        {
            List<Atom> personList = SuperController.singleton.GetAtoms().Where(a => a.type == "Person").ToList();
            if (personList.Count == 0)
            {
                Log("[Auto_Load_Person_Plugins] LoadPersonPlugins: no Person atoms yet (normal for empty scenes; will apply when Person atoms appear).");
                return;
            }

            List<string> pluginHasBeenAddedToSomeAtom = new List<string>();

            foreach (Atom at in personList)
            {
                bool isMale = at.GetComponentInChildren<DAZCharacter>().isMale;
                if (at.uid == createdMaleName) isMale = true; //sometimes the created atom wasn't being seen as male

                Log("Is male: " + isMale);

                //which plugins should we add?
                List<string> desiredPlugins = new List<string>();

                if (isMale)
                {
                    foreach (string pluginString in malePlugins)
                    {
                        desiredPlugins.Add(pluginString);
                    }
                }
                else
                {
                    if (maleOnly) continue;

                    foreach (string pluginString in femalePlugins)
                    {
                        desiredPlugins.Add(pluginString);
                    }

                    Log("Num Person Atoms: " + personList.Count);

                    if (personList.Count == 1)
                    {
                        foreach (string pluginString in femaleSoloPlugins)
                        {
                            Log("Desired plugin: " + pluginString);
                            desiredPlugins.Add(pluginString);
                        }
                    }
                }

                //don't double add the same plugin in case female and female solo both contain some of the same
                desiredPlugins = desiredPlugins.Distinct().ToList();
                if (suppressSpankingsForThisCall)
                    desiredPlugins.Remove(PLUGIN_SPANKINGS);
                Log("Desired plugins: " + desiredPlugins.Count);

                if (desiredPlugins.Contains(PLUGIN_VAM_LAUNCH))
                {
                    desiredPlugins.Remove(PLUGIN_VAM_LAUNCH);

                    //VAM LAUNCH CAN'T BE HANDLED NORMALLY, IF IT'S REMOVED AND ADDED TO THE SCENE, IT STOPS WORKING
                    //ALSO WE MAY ALREADY HAVE ONE, AND IT WOULD BE IN THE VAM LAUNCH ATOM, NOT THE PERSON ATOM
                    HandleVAMLaunch();

                    return;
                }

                MVRPluginManager manager = at.GetStorableByID("PluginManager") as MVRPluginManager;
                JSONClass current = manager.GetJSON(true, true, true);
                if (current["plugins"] != null && current["plugins"]["plugin#0"] != null &&
                    current["plugins"]["plugin#0"].Value != "")
                {
                    //make new list, going to have to re-add all existing plugins first, then new plugins after to maintain plugin numbers
                    List<string> newPlugins = new List<string>();
                    List<string> newPluginFilenames = new List<string>();
                    foreach (JSONNode pluginNode in current["plugins"].Childs)
                    {
                        string path = pluginNode.Value;

                        if (isMale &&
                            (path == PLUGIN_IMPROVED_POV || path == PLUGIN_IMPROVED_POV_TONGUE))
                        {
                            continue;
                        }
                        if (IsManagedPluginPath(path, isMale))
                        {
                            continue;
                        }

                        //if we start with a plugin that is relative like ./ this can be a wrong path for some reason, fix the path
                        if (path.StartsWith("./"))
                        {
                            //make relative to saves dir
                            path = SuperController.singleton.currentSaveDir + "/" + path.Substring(2);
                            Log("Fixed Path: " + pluginNode.Value + " to: " + path);
                        }

                        //does this path exist?
                        int folderSeparatorIndex = path.LastIndexOf("/");
                        if (folderSeparatorIndex < 0)
                        {
                            //local path can be incorrect if we change the look, so let's be clear about what path VaM will really look at
                            path = SuperController.singleton.currentSaveDir + "/" + path;
                            folderSeparatorIndex = path.LastIndexOf("/");
                        }

                        Log("currentSavesDir: " + SuperController.singleton.currentSaveDir + ", folderSeparatorIndex: " + folderSeparatorIndex);
                        if (folderSeparatorIndex > 0 && folderSeparatorIndex < path.Length - 1)
                        {
                            if (!FileExists(path))
                            {
                                //how about at the standard location?
                                string scriptInStandardFolder = "Custom/Scripts/" + GetFileName(path);
                                if (!FileExists(scriptInStandardFolder))
                                {
                                    //file not found, don't add it
                                    Log("Plugin " + path + " not found, we may have changed the active folder by loading a new look that doesn't have that plugin in it's folder.");
                                    continue;
                                }
                                else
                                {
                                    //found the file in the scripts folder
                                    Log("Plugin " + path + " not found, but we found a plugin with the same name at: " + scriptInStandardFolder);
                                    path = scriptInStandardFolder;
                                }
                            }
                        }

                        if (IsImprovedPoVScriptFile(path) &&
                            !IsOurModifiedImprovedPoVPath(path))
                        {
                            Log(
                                at.name +
                                " Removing ImprovedPoV.cs (use Custom/Scripts only); was: " +
                                path);
                            continue;
                        }

                        newPlugins.Add(path); //first all existing plugins
                        newPluginFilenames.Add(GetFileName(path));
                        Log(at.name + " Already Has Plugin: " + GetFileName(path));
                    }

                    //then add missing plugins, MISSING IS BASED NOT ON PATH BUT ONLY ON THE SCRIPT NAME!!!
                    //THIS CAN CAUSE AN ISSUE IF MULTIPLE SCRIPTS HAVE THE SAME NAME, LIKE ADD_ME.CSLIST
                    foreach (string desiredPlugin in desiredPlugins)
                    {
                        string realPlugin = desiredPlugin;
                        if (realPlugin == PLUGIN_IMPROVED_POV)
                        {
                            //which one are we really using
                            if (useImprovedPOVLicking)
                            {
                                realPlugin = PLUGIN_IMPROVED_POV_TONGUE;
                            }
                        }

                        string desiredPluginFileName = GetFileName(realPlugin);
                        if (!newPluginFilenames.Contains(desiredPluginFileName))
                        {
                            Log(at.name + " Doesn't Yet Have: " + desiredPluginFileName);
                            newPlugins.Add(realPlugin);
                        } else if (desiredPluginFileName.ToLower().EndsWith((string)("ADD_ME.cslist").ToLower()))
                        {
                            //special case check for ADD_ME.CSLIST in full path
                            if (!newPlugins.Contains(realPlugin))
                            {
                                Log(at.name + " Doesn't Yet Have (matched ADD_ME.cslist by full path): " + desiredPluginFileName);
                                newPlugins.Add(realPlugin);
                            }
                        }
                    }

                    //if we have already added a plugin that should only be added once, remove it
                    foreach (string pluginToTest in pluginsThatShouldBeAddedOnlyOnce)
                    {
                        if (pluginHasBeenAddedToSomeAtom.Contains(pluginToTest))
                        {
                            newPlugins.Remove(pluginToTest);
                        }
                    }

                    try
                    {
                        Log("Setting Plugins");
                        pluginHasBeenAddedToSomeAtom.AddRange(newPlugins);
                        JSONClass jc = CreatePluginJSON(newPlugins.ToArray());
                        manager.LateRestoreFromJSON(jc);
                    }
                    catch (Exception e)
                    {
                        SuperController.LogError("Failed to load plugin");
                        SuperController.LogError(e.ToString());
                    }
                }
                else
                {
                    //if we have already added a plugin that should only be added once, remove it
                    foreach (string pluginToTest in pluginsThatShouldBeAddedOnlyOnce)
                    {
                        if (pluginHasBeenAddedToSomeAtom.Contains(pluginToTest))
                        {
                            desiredPlugins.Remove(pluginToTest);
                        }
                    }

                    // load person plugins
                    Log(at.name + " Doesn't Have Any Plugins, Adding All Plugins");
                    try
                    {
                        if (desiredPlugins.Count() > 0) { 
                            pluginHasBeenAddedToSomeAtom.AddRange(desiredPlugins);
                            JSONClass jc = CreatePluginJSON(desiredPlugins.ToArray());
                            manager.LateRestoreFromJSON(jc);
                        }
                    }
                    catch (Exception e)
                    {
                        SuperController.LogError("Failed to load plugin");
                        SuperController.LogError(e.ToString());
                    }
                }
            }
        }

        private JSONStorable FindPluginInAtom(string fullClassName, Atom atomToSearch)
        {
            JSONStorable plugin = null;
            List<string> names = atomToSearch.GetStorableIDs();
            if (names != null && names.Count > 0)
            {
                string pluginName = names.Find(s => s.StartsWith("plugin#") && s.EndsWith(fullClassName));

                if (pluginName != null && pluginName != "")
                {
                    plugin = atomToSearch.GetStorableByID(pluginName);
                    if (plugin != null) return plugin;
                }
            }
            return plugin;
        }

        private JSONStorable FindPluginInScene(string fullClassName)
        {
            JSONStorable plugin = null;
            foreach (Atom testAtom in SuperController.singleton.GetAtoms()) { 
                List<string> names = testAtom.GetStorableIDs();
                if (names != null && names.Count > 0) { 
                    string pluginName = names.Find(s => s.StartsWith("plugin#") && s.EndsWith(fullClassName));
                    if (pluginName != null && pluginName != "") { 
                        plugin = testAtom.GetStorableByID(pluginName);
                        if (plugin != null) return plugin;
                    }
                }
            }
            return plugin;
        }

        public void HandleVAMLaunch()
        {
            Log("handling vam launch");
            //we want to add PLUGIN_VAM_LAUNCH

            //do we already have it?
            JSONStorable vamLaunchJSON = FindPluginInScene(PLUGIN_VAM_LAUNCH_CLASS_NAME);

            if (vamLaunchJSON == null)
            {
                Log("adding vam launch atom");
                //create it in a separate atom so it doesn't keep getting added and removed
                StartCoroutine(CreateVAMLaunchAtom());
            } else
            {
                Log(vamLaunchJSON.ToString());
            }
        }

        public void CheckVAMLaunchPlugin()
        {
            if (vamLaunchAtom != null)
            {
                MVRPluginManager manager = vamLaunchAtom.GetStorableByID("PluginManager") as MVRPluginManager;
                JSONClass current = manager.GetJSON(true, true, true);

                //do we have the VAM Launch plugin?
                List<string> allPlugins = new List<string>();
                allPlugins.Add(PLUGIN_VAM_LAUNCH);

                bool hasVAMLaunch = false;
                if (current["plugins"] != null && current["plugins"]["plugin#0"] != null &&
                    current["plugins"]["plugin#0"].Value != "")
                {
                    //probably impossible as we already checked the whole scene for vam launch plugins, if that worked...
                    foreach (JSONNode pluginNode in current["plugins"].Childs)
                    {
                        if (pluginNode.Value.EndsWith("VAMLaunch/ADD_ME.cslist"))
                        {
                            //can have the setup instructions version, must replace with correct version
                            JSONStorable vamLaunchJSONCheck = FindPluginInScene(PLUGIN_VAM_LAUNCH_CLASS_NAME);
                            if (vamLaunchJSONCheck != null) { 
                                Log("already has VAM Launch plugin, don't add it");
                                hasVAMLaunch = true;
                                break;
                            }
                        }
                    }

                    if (!hasVAMLaunch)
                    {
                        //add it
                        Log("adding vam launch to atom that already has plugins");

                        //if we start with a plugin that is relative like ./ this can be a wrong path for some reason, fix the path
                        foreach (JSONNode pluginNode in current["plugins"].Childs)
                        {
                            Log("Check for vamlaunch: " + pluginNode.Value);
                            if (!pluginNode.Value.EndsWith("VAMLaunch/ADD_ME.cslist")) { 
                                string path = pluginNode.Value;
                                if (path.StartsWith("./"))
                                {
                                    //make relative to saves dir
                                    path = SuperController.singleton.currentSaveDir + "/" + path.Substring(2);
                                    Log("Fixed Path: " + pluginNode.Value + " to: " + path);
                                }
                                allPlugins.Add(path);
                            }
                        }
                    }
                } else
                {
                    Log("adding vam launch to atom that doesn't have plugins");
                }

                if (!hasVAMLaunch)
                {
                    Log("Setting VAM LAUNCH Plugins");
                    JSONClass jc = CreatePluginJSON(allPlugins.ToArray());
                    manager.LateRestoreFromJSON(jc);
                }
            }

            JSONStorable vamLaunchJSON = FindPluginInScene(PLUGIN_VAM_LAUNCH_CLASS_NAME);
            if (vamLaunchJSON == null)
            {
                SuperController.LogError("To enable VAMLaunch for the Fleshlight Launch, follow the Setup Instructions in:\nVAM / Custom / Scripts / VAMLaunch.");
            }
        }

        private IEnumerator CreateVAMLaunchAtom()
        {
            vamLaunchAtom = SuperController.singleton.GetAtomByUid(PLUGIN_VAM_LAUNCH_ATOM_NAME);
            if (vamLaunchAtom == null)
            {
                yield return SuperController.singleton.AddAtomByType("Empty", PLUGIN_VAM_LAUNCH_ATOM_NAME);
                if (logMessages) SuperController.LogMessage("created " + PLUGIN_VAM_LAUNCH_ATOM_NAME);
                vamLaunchAtom = SuperController.singleton.GetAtomByUid(PLUGIN_VAM_LAUNCH_ATOM_NAME);
            }

            CheckVAMLaunchPlugin();
        }


        public void ResetMorphs()
        {
            //I don't want to catalog all real morphs, maybe I should, not sure, for now let's just reset some things
            //anyway, we don't want to take too long doing this when the button is pressed, and we maybe it's ok some expressions would stay???

            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            foreach (Atom at in personAtoms)
            {
                //reset look
                at.GetStorableByID("Eyes").SetStringChooserParamValue("lookMode", "Player");

                //RESET DOLLMASTER
                JSONStorable geometry = at.GetStorableByID("geometry");
                if (geometry == null)
                {
                    continue;
                }
                DAZCharacterSelector character = geometry as DAZCharacterSelector;
                var morphUI = character.morphsControlUI;
                morphUI.GetMorphDisplayNames().ToList().ForEach(name =>
                {
                    DAZMorph morph = morphUI.GetMorphByDisplayName(name);
                    if (morph.isPoseControl || morph.region.Contains("Expression"))
                    {
                        morph.SetValue(morph.startValue);
                    }
                });

            }
        }



        private void RemoveMaleAtom()
        {
            if (createdMaleAtom != null)
            {
                SuperController.singleton.RemoveAtom(createdMaleAtom);
                createdMaleAtom = null;
            }
        }

        public void CreateMaleIfNeeded()
        {
            createdMaleAtom = SuperController.singleton.GetAtomByUid(createdMaleName);
            if (createdMaleAtom == null)
            {
                Log("Create Male Atom: Start coroutine");
                StartCoroutine(CreateMaleAtom());
            }
            else
            {
                //we already have the male, set it up
                SetupMaleAtom();
            }
        }

        private void SetupMaleAtom()
        {

            Log("Create Male Atom: SetupMaleAtom");
            createdMaleAtom = SuperController.singleton.GetAtomByUid(createdMaleName);
            Log("Create Male Atom: is active and enabled: " + createdMaleAtom.isActiveAndEnabled);
            createdMaleAtom.LoadAppearancePreset("Saves/Person/appearance/EasyMate_AddMalePerson_Template.json");
            createdMaleAtom.uid = createdMaleName;

            //now that it's male, we can turn it on and it will load the correct plugins
            JSONStorable atomControlReceiver = atomControlReceiver = createdMaleAtom.GetStorableByID("AtomControl");
            atomControlReceiver.SetBoolParamValue("on", true);


            //put him in front of the girl?
            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            Atom femaleAtom = null;
            foreach (Atom at in personAtoms)
            {
                if (!at.GetComponentInChildren<DAZCharacter>().isMale && !(at.uid == createdMaleName))
                {
                    femaleAtom = at;
                    break;
                }
            }

            if (femaleAtom != null)
            {
                Transform headTransfrom = femaleAtom.freeControllers.First(freec => freec.name == "headControl").transform;
                Vector3 pos = headTransfrom.position;
                Vector3 newPos = pos + headTransfrom.forward;

                createdMaleAtom.transform.position = newPos;

                LookAtConstrained(pos, createdMaleAtom.transform);

                createdMaleAtom.transform.position = new Vector3(newPos.x, 0, newPos.z);
            }
            else
            {

                createdMaleAtom.transform.SetPositionAndRotation(new Vector3(0, 0, 1.0f), Quaternion.Euler(new Vector3(0, 180.0f, 0)));
            }

            //now that it's in place we can turn on physics
            atomControlReceiver.SetBoolParamValue("collisionEnabled", true);

            LoadPersonPlugins(true);
            Log("Create Male Atom: SetupMaleAtom END");
        }
        public void LookAtConstrained(Vector3 targetPos, Transform sourceTransform)
        {
            sourceTransform.LookAt(new Vector3(targetPos.x, sourceTransform.position.y, targetPos.z));
        }

        private IEnumerator CreateMaleAtom()
        {
            yield return SuperController.singleton.AddAtomByType("Person", createdMaleName);
            if (logMessages) SuperController.LogMessage("created " + createdMaleName);
            createdMaleAtom = SuperController.singleton.GetAtomByUid(createdMaleName);

            JSONStorable atomControlReceiver = atomControlReceiver = createdMaleAtom.GetStorableByID("AtomControl");
            atomControlReceiver.SetBoolParamValue("collisionEnabled", false);
            //atomControlReceiver.SetBoolParamValue("on", false);
        }
    }
}
 