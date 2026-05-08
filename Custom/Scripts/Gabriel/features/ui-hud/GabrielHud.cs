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

        public const string PluginClothingTouchFallOff =
            "Custom/Scripts/Gabriel/features/clothing-interactions/ClothingTouchFallOff.cslist";

        /// <summary>
        /// Scene atom UIDs created by <c>octopussy.Spankings</c>; removed when
        /// Spankings is toggled off.
        /// </summary>
        private static readonly string[] SpankingsOwnedSceneAtomUids =
        {
            "HitAudioSource",
            "CheekLeft",
            "CheekRight"
        };

        private GabrielHotkeys _hotkeys;

        private GabrielSessionOrchestrator _orchestrator;

        private Canvas _canvas;

        private bool _isDesktopMode;

        private UIDynamicButton emotionLiteHudButton;

        private UIDynamicButton emotionOriginalHudButton;

        private UIDynamicButton emotionMaleHudButton;

        private UIDynamicButton emotionFemaleHudButton;

        private UIDynamicButton emotionFinalHudButton;

        private UIDynamicButton emotionRemoveAllHudButton;

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
            if (_orchestrator != null)
            {
                _orchestrator.BindGabrielHud(null);
                _orchestrator = null;
            }

            NextSceneUiButton.ReleaseHost();
            PassengerRuntime.StopVrPassengerHandsRoutine();
            PersonAtomCache.UnregisterPersonGenderCacheInvalidation();
            PersonAtomCache.InvalidatePersonGenderCaches();
            DestroyHudCanvas();
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
        /// Merges Gabriel clothing touch fall-off onto every Person.
        /// </summary>
        public void MergeClothingTouchFallOffOnAllPersonsOnly()
        {
            try
            {
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    GabrielPluginManagerMerge.TryMergePluginOntoPerson(
                        at,
                        PluginClothingTouchFallOff);
                }
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "Easy Mate clothing touch fall-off merge on all Persons: " +
                    e);
            }
        }

        /// <summary>
        /// Removes AutoMate E-Motion, E-MotionLite, and E-Motion Final from every
        /// Person.
        /// </summary>
        public void RemoveEmotionFromAllPersons()
        {
            try
            {
                string fnPack = VaMFilePathUtil.GetFileName(PluginEMotion);
                string fnFinal = VaMFilePathUtil.GetFileName(PluginEMotionFinal);
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    GabrielPluginManagerMerge.TryRemovePluginFromPerson(
                        at,
                        fnPack);
                    GabrielPluginManagerMerge.TryRemovePluginFromPerson(
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
                string fn = VaMFilePathUtil.GetFileName(PluginSpankings);
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    GabrielPluginManagerMerge.TryRemovePluginFromPerson(at, fn);
                }

                TryRemoveSpankingsOwnedSceneAtoms();
                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "Spankings remove from all Persons: " + e);
            }
        }

        private static void TryRemoveSpankingsOwnedSceneAtoms()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            foreach (string uid in SpankingsOwnedSceneAtomUids)
            {
                try
                {
                    Atom atom = sc.GetAtomByUid(uid);
                    if (atom != null)
                        sc.RemoveAtom(atom);
                }
                catch (Exception e)
                {
                    SuperController.LogError(
                        "Easy Mate: remove Spankings scene atom \"" + uid +
                        "\": " + e.Message);
                }
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
                string fn = VaMFilePathUtil.GetFileName(PluginSpankings);
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    if (!PersonAtomCache.IsPersonFemale(at))
                        continue;
                    if (!GabrielPluginManagerMerge.PersonHasPluginByFileName(
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
                string fn = VaMFilePathUtil.GetFileName(PluginSpankings);
                foreach (Atom at in PersonAtomCache.GetPersonAtoms())
                {
                    if (includePerson != null && !includePerson(at))
                        continue;
                    if (!GabrielPluginManagerMerge.PersonHasPluginByFileName(
                        at,
                        fn))
                        GabrielPluginManagerMerge.TryMergePluginOntoPerson(
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
            const float emotionColButtonWidth = 132f;
            const float midColButtonWidth = 118f;
            const float rightColButtonWidth = 132f;

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

            // Columns 1-3 only; column 0 is VaMLogClipboardHud (separate plugin).
            emotionLiteHudButton = AddButton(
                "E-Motion Lite",
                MergeEmotionLiteOnAllPersonsOnly,
                1,
                0,
                emotionColButtonWidth);

            emotionOriginalHudButton = AddButton(
                "E-Motion Original",
                MergeEmotionOnAllPersonsOnly,
                1,
                1,
                emotionColButtonWidth);

            emotionFinalHudButton = AddButton(
                "E-Motion Final",
                MergeEmotionFinalOnAllPersonsOnly,
                1,
                2,
                emotionColButtonWidth);

            spankingsButton = AddButton(
                "+ Spankings Male",
                ToggleSpankingsPluginOnAllPersons,
                3,
                0,
                rightColButtonWidth);

            emotionRemoveAllHudButton = AddButton(
                "Remove E-Motion",
                RemoveEmotionFromAllPersons,
                1,
                3,
                emotionColButtonWidth);
            removeSpankingsButton = AddButton(
                "Remove Spankings",
                RemoveSpankingsFromAllPersons,
                3,
                1,
                rightColButtonWidth);

            emotionMaleHudButton = AddButton(
                "E-Motion M",
                MergeEmotionOriginalOnMalePersonsOnly,
                1,
                4,
                emotionColButtonWidth);
            emotionFemaleHudButton = AddButton(
                "E-Motion F",
                MergeEmotionOriginalOnFemalePersonsOnly,
                2,
                4,
                midColButtonWidth);

            RefreshPluginToggleLabels();
            _canvas.transform.Translate(0f, 0.2f, 0f);
            // Start collapsed until Show UI / Hide UI toggles visibility.
            ShowUI(false);
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
            if (emotionLiteHudButton != null)
                emotionLiteHudButton.gameObject.SetActive(setToActive);
            if (emotionOriginalHudButton != null)
                emotionOriginalHudButton.gameObject.SetActive(setToActive);
            if (emotionFinalHudButton != null)
                emotionFinalHudButton.gameObject.SetActive(setToActive);
            if (emotionRemoveAllHudButton != null)
                emotionRemoveAllHudButton.gameObject.SetActive(setToActive);
            if (emotionMaleHudButton != null)
                emotionMaleHudButton.gameObject.SetActive(setToActive);
            if (emotionFemaleHudButton != null)
                emotionFemaleHudButton.gameObject.SetActive(setToActive);
            if (spankingsButton != null)
                spankingsButton.gameObject.SetActive(setToActive);
            if (removeSpankingsButton != null)
                removeSpankingsButton.gameObject.SetActive(setToActive);

            if (setToActive)
                RefreshPluginToggleLabels();
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
                    VaMFilePathUtil.GetFileName(PluginSpankings);
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
                        if (!GabrielPluginManagerMerge.PersonHasPluginByFileName(
                            at,
                            desiredFileName))
                            GabrielPluginManagerMerge.TryMergePluginOntoPerson(
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
        }

        private static void SetPluginToggleLabel(
            UIDynamicButton button,
            string pluginPath,
            string labelBase)
        {
            if (button == null || SuperController.singleton == null)
                return;

            string fileName = VaMFilePathUtil.GetFileName(pluginPath);
            bool allHave = AllPersonAtomsHavePluginByFileName(fileName);
            button.label = (allHave ? "- " : "+ ") + labelBase;
        }

        private static bool AllPersonAtomsHavePluginByFileName(string desiredFileName)
        {
            bool any = false;
            foreach (Atom at in PersonAtomCache.GetPersonAtoms())
            {
                any = true;
                if (!GabrielPluginManagerMerge.PersonHasPluginByFileName(
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

            fnOrig = VaMFilePathUtil.GetFileName(PluginEMotion);
            fnLite = VaMFilePathUtil.GetFileName(PluginEMotionLite);
            fnFinal = VaMFilePathUtil.GetFileName(PluginEMotionFinal);
            paths = GabrielPluginManagerMerge.CollectNormalizedPluginPaths(
                pluginManager);
            paths.RemoveAll(path =>
            {
                string fileName = VaMFilePathUtil.GetFileName(path);
                return fileName == fnOrig ||
                    fileName == fnLite ||
                    fileName == fnFinal;
            });
            paths.Add(desiredPluginPath);
            GabrielPluginManagerMerge.ApplyPluginPathsToManager(
                pluginManager,
                paths);
        }
    }
}
