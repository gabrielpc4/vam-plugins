using System;
using System.Collections.Generic;
using MeshVR;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// World-space HUD and hotkey hub for Gabriel session features. Late runtime
    /// orchestration lives in <see cref="GabrielSessionOrchestrator"/>.
    /// </summary>
    public class GabrielHud : MVRScript
    {
        private const string CoreControlAtomUid = "CoreControl";
        private const string GabrielSessionOrchestratorClassSuffix =
            ".GabrielSessionOrchestrator";

        public const string PluginEMotion =
            "Custom/Scripts/AutoMate/PERSON_PLUGINS/E-Motion - VaM Auto Blink/E-Motion_AddThisONLY.cslist";

        /// <summary>
        /// Lite face/emotion pack (no scripted head/neck / eye target per fork).
        /// Loaded when <see cref="EmotionPathKeywords"/> matches load/save folder
        /// paths (<see cref="EmotionPathKeywords.KeywordsFileRelative"/>). Uses the
        /// same cslist file name as <see cref="PluginEMotion"/>.
        /// </summary>
        public const string PluginEMotionLite =
            "Custom/Scripts/E-MotionLite/E-Motion_AddThisONLY.cslist";

        /// <summary>
        /// VRAdultFun Final pack; own folder and unique <c>.cslist</c> basename so
        /// it is independent of AutoMate original and Lite sources.
        /// </summary>
        public const string PluginEMotionFinal =
            "Custom/Scripts/E-MotionFinal/E-Motion_Final_AddThisONLY.cslist";

        public const string PluginSpankings =
            "Custom/Scripts/Spankings/Spankings.cslist";

        private GabrielHotkeys _hotkeys;

        private GabrielSessionOrchestrator _orchestrator;

        private Canvas _canvas;

        private bool _isDesktopMode;

        private UIDynamicButton emotionLoadHudButton;

        private UIDynamicButton emotionPackCycleHudButton;

        private UIDynamicButton emotionGenderCycleHudButton;

        /// <summary>
        /// 0 = Lite, 1 = Original, 2 = Final, 3 = None (see cycle button label).
        /// </summary>
        private int _emotionPackIndex;

        private bool _emotionMaleOnlyGender = true;

        /// <summary>
        /// Shown on pack/gender HUD buttons that cycle on click.
        /// </summary>
        private const string EmotionCycleButtonSuffix = " >";

        private UIDynamicButton spankingsButton;

        private UIDynamicButton removeSpankingsButton;

        public JSONStorableAction hideUI;

        public JSONStorableAction showUI;

        public override void Init()
        {
            NextSceneUiButton.BindHost(this);
            _isDesktopMode = !(SuperController.singleton.isOVR ||
                SuperController.singleton.isOpenVR);
            PersonAtomCache.RegisterPersonGenderCacheInvalidation();
            _hotkeys = new GabrielHotkeys(this);

            hideUI = new JSONStorableAction("Hide UI", HideUI);
            RegisterAction(hideUI);
            showUI = new JSONStorableAction("Show UI", ShowUI);
            RegisterAction(showUI);

            TryBindSessionOrchestrator();
        }

        private void Start()
        {
            SuperController sc = SuperController.singleton;
            float worldScale;

            TryBindSessionOrchestrator();
            if (sc == null)
                return;

            worldScale = sc.worldScale;
            sc.worldScale = 1f;
            try
            {
                RebuildHudCanvas();
            }
            finally
            {
                if (SuperController.singleton != null)
                    SuperController.singleton.worldScale = worldScale;
            }
        }

        private void Update()
        {
            TryBindSessionOrchestrator();
            if (_hotkeys != null)
                _hotkeys.ProcessUpdate();
        }

        private void OnDestroy()
        {
            try
            {
                if (_orchestrator != null)
                {
                    _orchestrator.BindGabrielHud(null);
                    _orchestrator = null;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "GabrielHud.OnDestroy orchestrator bind: " + e.Message);
            }

            try
            {
                NextSceneUiButton.ReleaseHost();
                PassengerRuntime.StopVrPassengerHandsRoutine();
                PersonAtomCache.UnregisterPersonGenderCacheInvalidation();
                PersonAtomCache.InvalidatePersonGenderCaches();
                DestroyHudCanvas();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "GabrielHud.OnDestroy teardown: " + e.Message);
            }
        }

        private void TryBindSessionOrchestrator()
        {
            Atom coreControl;
            List<string> storableIds;
            int i;
            string storableId;
            JSONStorable storable;
            GabrielSessionOrchestrator orchestrator;

            if (_orchestrator != null)
                return;

            coreControl = containingAtom;
            if (coreControl == null && SuperController.singleton != null)
            {
                coreControl =
                    SuperController.singleton.GetAtomByUid(CoreControlAtomUid);
            }

            if (coreControl == null)
                return;

            storableIds = coreControl.GetStorableIDs();
            if (storableIds == null)
                return;

            for (i = 0; i < storableIds.Count; i++)
            {
                storableId = storableIds[i];
                if (string.IsNullOrEmpty(storableId) ||
                    !storableId.EndsWith(
                        GabrielSessionOrchestratorClassSuffix,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                storable = coreControl.GetStorableByID(storableId);
                orchestrator = storable as GabrielSessionOrchestrator;
                if (orchestrator == null)
                    continue;

                _orchestrator = orchestrator;
                _orchestrator.BindGabrielHud(this);
                return;
            }
        }

        internal static void ToggleFreezeAnimationHotkey()
        {
            SuperController sc = SuperController.singleton;
            bool currentlyOn;

            if (sc == null)
                return;

            currentlyOn = false;
            if (sc.freezeAnimationToggle != null)
                currentlyOn = sc.freezeAnimationToggle.isOn;
            else if (sc.freezeAnimationToggleAlt != null)
                currentlyOn = sc.freezeAnimationToggleAlt.isOn;
            else
                currentlyOn = sc.freezeAnimation;

            sc.SetFreezeAnimation(!currentlyOn);
        }

        /// <summary>
        /// Stops Easy Mate auto-possess coroutine and
        /// <see cref="SuperController.ClearPossess"/> (for hotkeys and auto-release).
        /// </summary>
        public static void RequestClearAllPossession(string logMessage)
        {
            SuperController sc = SuperController.singleton;

            if (sc == null)
                return;

            PassengerRuntime.StopVrPassengerHandsRoutine();
            HeadProximityHide.RestoreTransientHeadHideState();
            PassengerRuntime.StopPassengerMode();
            sc.ClearPossess();
            PassengerHandPrePossessSnapshot
                .RestoreAfterPossessClearThenDiscardSnapshot();
            PassengerHmdFreeControllerCleanup
                .UnlinkStrayHmdLinkedFreeControllersAndNaturalizeHeads(sc);
            try
            {
                sc.SelectModeOff();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "Easy Mate ClearPossess: SelectModeOff: " + e.Message);
            }

            HeadProximityHide.HidePossessorAlignmentPreviewMeshes();
            if (!string.IsNullOrEmpty(logMessage))
                SuperController.LogMessage(logMessage);
        }

        public void MergeEmotionLiteOnAllPersonsOnly()
        {
            MergeEmotionFamilyOnPersons(
                PluginEMotionLite,
                null,
                "E-MotionLite merge on all Persons");
        }

        /// <summary>
        /// Merges AutoMate E-Motion onto every Person. Removes any other E-Motion
        /// family entry (Lite, Final) on that atom, then installs
        /// <see cref="PluginEMotion"/>.
        /// </summary>
        public void MergeEmotionOnAllPersonsOnly()
        {
            MergeEmotionFamilyOnPersons(
                PluginEMotion,
                null,
                "E-Motion merge on all Persons");
        }

        /// <summary>
        /// Merges AutoMate E-Motion (<see cref="PluginEMotion"/>) onto every male
        /// <c>Person</c> only.
        /// </summary>
        public void MergeEmotionOriginalOnMalePersonsOnly()
        {
            MergeEmotionFamilyOnPersons(
                PluginEMotion,
                PersonAtomCache.IsMalePerson,
                "E-Motion Original merge on males");
        }

        /// <summary>
        /// Merges <see cref="PluginEMotion"/> onto every female <c>Person</c>
        /// only.
        /// </summary>
        public void MergeEmotionOriginalOnFemalePersonsOnly()
        {
            MergeEmotionFamilyOnPersons(
                PluginEMotion,
                PersonAtomCache.IsPersonFemale,
                "E-Motion Original merge on females");
        }

        /// <summary>
        /// Merges <see cref="PluginEMotionFinal"/> onto every Person.
        /// </summary>
        public void MergeEmotionFinalOnAllPersonsOnly()
        {
            MergeEmotionFamilyOnPersons(
                PluginEMotionFinal,
                null,
                "E-Motion Final merge on all Persons");
        }

        /// <summary>
        /// Merges E-Motion Final onto female Persons.
        /// </summary>
        public void MergeEmotionFinalOnFemalePersonsOnly()
        {
            MergeEmotionFamilyOnPersons(
                PluginEMotionFinal,
                PersonAtomCache.IsPersonFemale,
                "E-Motion Final merge on female Persons");
        }

        /// <summary>
        /// Merges the E-Motion pack and gender scope chosen on the HUD (cycle
        /// buttons). Index 3 (None) removes all E-Motion variants instead.
        /// </summary>
        public void LoadEmotionFromHudConfiguration()
        {
            if (_emotionPackIndex == 3)
            {
                RemoveEmotionFromAllPersons();
                return;
            }

            string path;
            string packName;
            Func<Atom, bool> filter;
            string scopeWord;

            switch (_emotionPackIndex)
            {
            case 0:
                path = PluginEMotionLite;
                packName = "Lite";
                break;
            case 1:
                path = PluginEMotion;
                packName = "Original";
                break;
            default:
                path = PluginEMotionFinal;
                packName = "Final";
                break;
            }

            if (_emotionMaleOnlyGender)
            {
                filter = PersonAtomCache.IsMalePerson;
                scopeWord = "males";
            }
            else
            {
                filter = PersonAtomCache.IsPersonFemale;
                scopeWord = "females";
            }

            MergeEmotionFamilyOnPersons(
                path,
                filter,
                "E-Motion " + packName + " HUD load on " + scopeWord);
        }

        /// <summary>
        /// Removes AutoMate E-Motion, E-MotionLite, and E-Motion Final from every
        /// Person.
        /// </summary>
        public void RemoveEmotionFromAllPersons()
        {
            try
            {
                string fnPack = FileManager.GetFileName(PluginEMotion);
                string fnLite = FileManager.GetFileName(PluginEMotionLite);
                string fnFinal = FileManager.GetFileName(PluginEMotionFinal);
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    PluginManager.TryRemovePluginFromPerson(
                        at,
                        fnPack);
                    PluginManager.TryRemovePluginFromPerson(
                        at,
                        fnLite);
                    PluginManager.TryRemovePluginFromPerson(
                        at,
                        fnFinal);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "Remove all E-Motion variants from all Persons: " + e);
            }
        }

        /// <summary>
        /// Removes Spankings from every Person and deletes Spankings-owned scene
        /// atoms left behind when the plugin is stripped.
        /// </summary>
        public void RemoveSpankingsFromAllPersons()
        {
            try
            {
                string fn = FileManager.GetFileName(PluginSpankings);
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    PluginManager.TryRemovePluginFromPerson(at, fn);
                }

                SpankingsAtomsRemoval.TryRemoveOwnedSceneAtoms();
                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "Spankings remove from all Persons: " + e);
            }
        }

        /// <summary>
        /// Merges Spankings onto every <c>Person</c> that does not already have it.
        /// </summary>
        public void MergeSpankingsOnAllPersonsOnly()
        {
            MergeSpankingsOnPersons(
                null,
                "Spankings merge on all Persons");
        }

        /// <summary>
        /// Merges Spankings onto every female <c>Person</c> that does not already
        /// have it.
        /// </summary>
        public void MergeSpankingsOnFemalePersonsOnly()
        {
            MergeSpankingsOnPersons(
                PersonAtomCache.IsPersonFemale,
                "Spankings merge on female Persons");
        }

        /// <summary>
        /// True when at least one female <c>Person</c> lacks Spankings.
        /// </summary>
        public bool AnyFemalePersonMissingSpankings()
        {
            try
            {
                string fn = FileManager.GetFileName(PluginSpankings);
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    if (!PersonAtomCache.IsPersonFemale(at))
                        continue;
                    if (!PluginManager.PersonHasPluginByFileName(
                        at,
                        fn))
                        return true;
                }

                return false;
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "AnyFemalePersonMissingSpankings: " + e.Message);
                return false;
            }
        }

        private void MergeEmotionFamilyOnPersons(
            string desiredPluginPath,
            Func<Atom, bool> includePerson,
            string logContext)
        {
            try
            {
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    if (includePerson != null && !includePerson(at))
                        continue;

                    TryReplaceEmotionFamilyWithExactPath(
                        at,
                        desiredPluginPath);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError(logContext + ": " + e);
            }
        }

        private void MergeSpankingsOnPersons(
            Func<Atom, bool> includePerson,
            string logContext)
        {
            try
            {
                string fn = FileManager.GetFileName(PluginSpankings);
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    if (includePerson != null && !includePerson(at))
                        continue;
                    if (!PluginManager.PersonHasPluginByFileName(
                        at,
                        fn))
                        PluginManager.TryMergePluginOntoPerson(
                            at,
                            PluginSpankings);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError(logContext + ": " + e);
            }
        }

        private void RebuildHudCanvas()
        {
            GameObject canvasObject;
            CanvasScaler scaler;
            const float scale = 0.001f;
            const float hudButtonWidth = 132f;

            DestroyHudCanvas();

            canvasObject = new GameObject("GabrielHudCanvas");
            _canvas = canvasObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.pixelPerfect = false;
            SuperController.singleton.AddCanvas(_canvas);

            _canvas.transform.SetParent(SuperController.singleton.mainHUD, false);

            scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.scaleFactor = 80f;
            scaler.dynamicPixelsPerUnit = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();

            _canvas.transform.localScale = new Vector3(scale, scale, scale);
            _canvas.transform.localPosition = new Vector3(-0.45f, -0.72f, 0.35f);
            LookAtCamera();

            // Columns 1-2 only; column 0 is VaMLogClipboardHud (separate plugin).
            // Column 1: E-Motion stack. Column 2: Spankings.
            emotionLoadHudButton = AddButton(
                EmotionPrimaryButtonLabelText(),
                OnEmotionPrimaryHudButtonClicked,
                1,
                0,
                hudButtonWidth);

            emotionPackCycleHudButton = AddButton(
                EmotionPackLabelForIndex(_emotionPackIndex),
                CycleEmotionPackButton,
                1,
                1,
                hudButtonWidth);

            emotionGenderCycleHudButton = AddButton(
                EmotionGenderCycleLabel(_emotionMaleOnlyGender),
                CycleEmotionGenderButton,
                1,
                2,
                hudButtonWidth);

            spankingsButton = AddButton(
                "+ Spankings Male",
                ToggleSpankingsPluginOnAllPersons,
                2,
                0,
                hudButtonWidth);

            removeSpankingsButton = AddButton(
                "Remove Spankings",
                RemoveSpankingsFromAllPersons,
                2,
                1,
                hudButtonWidth);

            RefreshPluginToggleLabels();
            _canvas.transform.Translate(0f, 0.2f, 0f);

        }

        private void DestroyHudCanvas()
        {
            if (_canvas == null)
                return;

            if (SuperController.singleton != null)
                SuperController.singleton.RemoveCanvas(_canvas);

            _canvas.transform.SetParent(null, false);
            if (_canvas.gameObject != null)
                GameObject.Destroy(_canvas.gameObject);

            _canvas = null;
        }

        public void ShowUI()
        {
            ShowUI(true);
        }

        public void HideUI()
        {
            ShowUI(false);
        }

        public void ShowUI(bool setToActive)
        {
            if (emotionLoadHudButton != null)
                emotionLoadHudButton.gameObject.SetActive(setToActive);
            if (emotionPackCycleHudButton != null)
                emotionPackCycleHudButton.gameObject.SetActive(setToActive);
            if (emotionGenderCycleHudButton != null)
                emotionGenderCycleHudButton.gameObject.SetActive(setToActive);
            if (spankingsButton != null)
                spankingsButton.gameObject.SetActive(setToActive);
            if (removeSpankingsButton != null)
                removeSpankingsButton.gameObject.SetActive(setToActive);

            if (setToActive)
                RefreshPluginToggleLabels();
        }

        private static string EmotionPackLabelForIndex(int index)
        {
            string s;
            switch (index)
            {
            case 0:
                s = "Lite E-Motion";
                break;
            case 1:
                s = "Original E-Motion";
                break;
            case 2:
                s = "Final E-Motion";
                break;
            default:
                s = "None";
                break;
            }

            return s + EmotionCycleButtonSuffix;
        }

        private static string EmotionGenderCycleLabel(bool maleOnly)
        {
            return (maleOnly ? "Male Only" : "Female Only") +
                EmotionCycleButtonSuffix;
        }

        private void CycleEmotionPackButton()
        {
            _emotionPackIndex = (_emotionPackIndex + 1) % 4;
            if (emotionPackCycleHudButton != null)
            {
                emotionPackCycleHudButton.label =
                    EmotionPackLabelForIndex(_emotionPackIndex);
            }
        }

        /// <summary>
        /// True if any Person has Original, Lite, or Final E-Motion installed.
        /// </summary>
        private static bool AnyPersonHasEmotionFamilyPlugin()
        {
            string fnOrig;
            string fnLite;
            string fnFinal;

            fnOrig = FileManager.GetFileName(PluginEMotion);
            fnLite = FileManager.GetFileName(PluginEMotionLite);
            fnFinal = FileManager.GetFileName(PluginEMotionFinal);

            foreach (Atom at in PersonAtomCache.GetPersonAtoms())
            {
                if (PluginManager.PersonHasPluginByFileName(at, fnOrig))
                    return true;
                if (PluginManager.PersonHasPluginByFileName(at, fnLite))
                    return true;
                if (PluginManager.PersonHasPluginByFileName(at, fnFinal))
                    return true;
            }

            return false;
        }

        private static string EmotionPrimaryButtonLabelText()
        {
            if (AnyPersonHasEmotionFamilyPlugin())
                return "Remove E-Motion";

            return "Load E-Motion";
        }

        private void RefreshEmotionPrimaryButtonLabel()
        {
            if (emotionLoadHudButton == null)
                return;

            emotionLoadHudButton.label = EmotionPrimaryButtonLabelText();
        }

        private void OnEmotionPrimaryHudButtonClicked()
        {
            if (AnyPersonHasEmotionFamilyPlugin())
            {
                RemoveEmotionFromAllPersons();
                return;
            }

            LoadEmotionFromHudConfiguration();
        }

        private void CycleEmotionGenderButton()
        {
            _emotionMaleOnlyGender = !_emotionMaleOnlyGender;
            if (emotionGenderCycleHudButton != null)
            {
                emotionGenderCycleHudButton.label =
                    EmotionGenderCycleLabel(_emotionMaleOnlyGender);
            }
        }

        internal void ClothingResetCycle()
        {
            PersonAtomCache.InvalidatePersonGenderCaches();
            RefreshPluginToggleLabels();
        }

        private UIDynamicButton AddButton(
            string name,
            UnityAction callback,
            int column,
            int row,
            float width)
        {
            Color accessButtonColor = new Color(0.8392f, 0.8392f, 0.8392f);
            Color accessTextColor = new Color(0f, 0f, 0f);
            float xSpacing = 0.22f;
            float ySpacing = 0.05f;
            UIDynamicButton button = CreateButton(name, width, 40f);

            button.button.onClick.AddListener(callback);
            button.transform.Translate(
                column * xSpacing,
                0.50f - row * ySpacing,
                0f,
                Space.Self);
            ColorButton(button, accessTextColor, accessButtonColor);

            return button;
        }

        private UIDynamicButton CreateButton(
            string name,
            float width,
            float height)
        {
            Transform button =
                GameObject.Instantiate<Transform>(manager.configurableButtonPrefab);

            ConfigureTransform(button, width, height);
            ParentToCanvas(button);

            UIDynamicButton uiButton = button.GetComponent<UIDynamicButton>();
            uiButton.label = name;
            uiButton.buttonText.fontSize = 18;
            return uiButton;
        }

        private static void ColorButton(
            UIDynamicButton button,
            Color textColor,
            Color buttonColor)
        {
            button.textColor = textColor;
            button.buttonColor = buttonColor;
        }

        private void ConfigureTransform(Transform transform, float width, float height)
        {
            RectTransform rt;

            transform.position = Vector3.zero;
            rt = transform.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(width / 2f, height / 2f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void ParentToCanvas(Transform transform)
        {
            transform.SetParent(_canvas.transform, false);
        }

        private void LookAtCamera()
        {
            if (_canvas == null)
                return;

            if (_isDesktopMode)
            {
                _canvas.transform.localEulerAngles = new Vector3(28f, 180f, 0f);
                return;
            }

            if (!XRSettings.enabled)
            {
                Transform cameraTransform = SuperController.singleton.lookCamera.transform;
                Vector3 endPos = cameraTransform.position +
                    cameraTransform.forward * 10000000f;
                _canvas.transform.LookAt(endPos, cameraTransform.up);
                return;
            }

            _canvas.transform.localEulerAngles = new Vector3(28f, 180f, 0f);
        }

        internal void ToggleSpankingsPluginOnAllPersons()
        {
            try
            {
                string desiredFileName =
                    FileManager.GetFileName(PluginSpankings);
                bool turningOff =
                    AllPersonAtomsHavePluginByFileName(desiredFileName);

                if (turningOff)
                {
                    RemoveSpankingsFromAllPersons();
                }
                else
                {
                    foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                    {
                        if (!PluginManager.PersonHasPluginByFileName(
                            at,
                            desiredFileName))
                            PluginManager.TryMergePluginOntoPerson(
                                at,
                                PluginSpankings);
                    }
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("Spankings HUD toggle: " + e);
            }
        }

        internal void RefreshPluginToggleLabels()
        {
            SetPluginToggleLabel(
                spankingsButton,
                PluginSpankings,
                "Spankings Male");
            RefreshEmotionPrimaryButtonLabel();
        }

        private static void SetPluginToggleLabel(
            UIDynamicButton button,
            string pluginPath,
            string labelBase)
        {
            if (button == null || SuperController.singleton == null)
                return;

            string fileName = FileManager.GetFileName(pluginPath);
            bool allHave = AllPersonAtomsHavePluginByFileName(fileName);
            button.label = (allHave ? "- " : "+ ") + labelBase;
        }

        private static bool AllPersonAtomsHavePluginByFileName(string desiredFileName)
        {
            bool any = false;
            foreach (Atom at in PersonAtomCache.GetPersonAtoms())
            {
                any = true;
                if (!PluginManager.PersonHasPluginByFileName(
                    at,
                    desiredFileName))
                    return false;
            }

            return any;
        }

        /// <summary>
        /// Removes every known E-Motion plugin entry on <paramref name="atom"/>,
        /// then adds exactly <paramref name="desiredPluginPath"/>.
        /// </summary>
        private static void TryReplaceEmotionFamilyWithExactPath(
            Atom atom,
            string desiredPluginPath)
        {
            MVRPluginManager pluginManager =
                atom.GetStorableByID("PluginManager") as MVRPluginManager;
            string fnOrig;
            string fnLite;
            string fnFinal;
            List<string> paths;

            if (pluginManager == null)
                return;

            fnOrig = FileManager.GetFileName(PluginEMotion);
            fnLite = FileManager.GetFileName(PluginEMotionLite);
            fnFinal = FileManager.GetFileName(PluginEMotionFinal);
            paths = PluginManager.CollectNormalizedPluginPaths(
                pluginManager);
            paths.RemoveAll(path =>
            {
                string fileName = FileManager.GetFileName(path);
                return fileName == fnOrig ||
                    fileName == fnLite ||
                    fileName == fnFinal;
            });
            paths.Add(desiredPluginPath);
            PluginManager.ApplyPluginPathsToManager(
                pluginManager,
                paths);
        }
    }
}
