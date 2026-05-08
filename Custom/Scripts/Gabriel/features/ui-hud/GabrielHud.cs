using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Rebuilt from <see cref="SuperController.GetAtoms"/> when invalid; see
        /// <see cref="InvalidatePersonGenderCaches"/>.
        /// </summary>
        private static bool _personGenderListsCacheValid;

        private static List<Atom> _cachedFemalePersonsByUid;

        private static List<Atom> _cachedMalePersonsByUid;

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

        private UIDynamicButton stripAllClothesButton;

        private UIDynamicButton removeUnderwearButton;

        public JSONStorableAction hideUI;

        public JSONStorableAction showUI;

        public override void Init()
        {
            NextSceneUiButton.BindHost(this);
            _isDesktopMode = !(SuperController.singleton.isOVR ||
                SuperController.singleton.isOpenVR);
            RegisterPersonGenderCacheInvalidation();
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
            UnregisterPersonGenderCacheInvalidation();
            InvalidatePersonGenderCaches();
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

        private static void InvalidatePersonGenderCaches()
        {
            _personGenderListsCacheValid = false;
            _cachedFemalePersonsByUid = null;
            _cachedMalePersonsByUid = null;
        }

        private static void EnsurePersonGenderCaches()
        {
            SuperController sc;

            if (_personGenderListsCacheValid)
                return;

            sc = SuperController.singleton;
            if (sc == null)
            {
                _cachedFemalePersonsByUid = new List<Atom>();
                _cachedMalePersonsByUid = new List<Atom>();
                _personGenderListsCacheValid = true;
                return;
            }

            _cachedFemalePersonsByUid = sc.GetAtoms()
                .Where(x => x != null && x.type == "Person" && IsPersonFemale(x))
                .OrderBy(x => x.uid, StringComparer.Ordinal)
                .ToList();
            _cachedMalePersonsByUid = sc.GetAtoms()
                .Where(x => x != null && x.type == "Person" && !IsPersonFemale(x))
                .OrderBy(x => x.uid, StringComparer.Ordinal)
                .ToList();
            _personGenderListsCacheValid = true;
        }

        private static void OnPersonSceneAtomUIDsChanged(List<string> atomUids)
        {
            InvalidatePersonGenderCaches();
        }

        private static void RegisterPersonGenderCacheInvalidation()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            sc.onAtomUIDsChangedHandlers -= OnPersonSceneAtomUIDsChanged;
            sc.onAtomUIDsChangedHandlers += OnPersonSceneAtomUIDsChanged;
        }

        private static void UnregisterPersonGenderCacheInvalidation()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            sc.onAtomUIDsChangedHandlers -= OnPersonSceneAtomUIDsChanged;
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
            UnlinkStrayHmdLinkedFreeControllersAndNaturalizeHeads(sc);
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

        /// <summary>
        /// After <see cref="SuperController.ClearPossess"/>, free any
        /// <see cref="FreeControllerV3"/> still parent-linked to the HMD/center
        /// camera rigidbody. For <c>headControl</c>, nudge pose toward neck/chest
        /// so the head sits naturally on the body.
        /// </summary>
        private static void UnlinkStrayHmdLinkedFreeControllersAndNaturalizeHeads(
            SuperController sc)
        {
            Rigidbody hmdRb;

            if (sc == null || sc.centerCameraTarget == null)
                return;

            hmdRb = sc.centerCameraTarget.GetComponent<Rigidbody>();
            if (hmdRb == null)
                return;

            foreach (Atom a in sc.GetAtoms())
            {
                if (a == null)
                    continue;

                try
                {
                    FreeControllerV3[] fcs =
                        a.GetComponentsInChildren<FreeControllerV3>(true);
                    if (fcs == null)
                        continue;

                    for (int i = 0; i < fcs.Length; i++)
                    {
                        FreeControllerV3 fc = fcs[i];
                        bool isHead;

                        if (fc == null || fc.linkToRB != hmdRb)
                            continue;

                        isHead = a.type == "Person" &&
                            a.GetStorableByID("headControl") == fc;

                        fc.RestorePreLinkState();
                        if (fc.linkToRB == hmdRb)
                            fc.SelectLinkToRigidbody(null);
                        fc.possessed = false;
                        fc.startedPossess = false;

                        if (isHead)
                            TryRestoreNaturalHeadPose(fc, a);
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Approximate a neutral head-on-neck pose after HMD unlink.
        /// </summary>
        private static void TryRestoreNaturalHeadPose(
            FreeControllerV3 head,
            Atom person)
        {
            if (head == null || head.control == null || person == null)
                return;

            FreeControllerV3 neck =
                person.GetStorableByID("neckControl") as FreeControllerV3;
            if (neck != null && neck.control != null)
            {
                head.control.rotation = neck.control.rotation *
                    Quaternion.Euler(8f, 0f, 0f);
                Vector3 targetPos =
                    neck.control.position + neck.control.up * 0.1f;
                head.control.position = Vector3.Lerp(
                    head.control.position,
                    targetPos,
                    0.75f);
                return;
            }

            FreeControllerV3 chest =
                person.GetStorableByID("chestControl") as FreeControllerV3;
            if (chest != null && chest.control != null)
            {
                Vector3 up = chest.control.up;
                Vector3 fwd = Vector3.ProjectOnPlane(
                    head.control.position - chest.control.position,
                    up);
                if (fwd.sqrMagnitude > 1e-8f)
                    fwd.Normalize();
                else
                    fwd = chest.control.forward;

                head.control.rotation = Quaternion.LookRotation(fwd, up);
                Vector3 targetPos =
                    chest.control.position + fwd * 0.18f + up * 0.38f;
                head.control.position = Vector3.Lerp(
                    head.control.position,
                    targetPos,
                    0.6f);
            }
        }

        public static bool RequestPassengerForSpecificPerson(Atom targetPerson)
        {
            if (targetPerson == null || targetPerson.type != "Person")
                return false;

            if (IsPersonFemale(targetPerson))
            {
                PassengerRuntime.RequestStartForFemale(targetPerson);
                return true;
            }

            if (IsMalePerson(targetPerson))
            {
                PassengerRuntime.RequestStartForMale(targetPerson);
                return true;
            }

            return false;
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
                IsMalePerson,
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
                IsPersonFemale,
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
                IsPersonFemale,
                "E-Motion Final merge on female Persons");
        }

        /// <summary>
        /// Merges Gabriel clothing touch fall-off onto every Person.
        /// </summary>
        public void MergeClothingTouchFallOffOnAllPersonsOnly()
        {
            try
            {
                foreach (Atom at in GetPersonAtoms())
                {
                    TryMergePluginOntoPerson(at, PluginClothingTouchFallOff);
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
                string fnPack = GetFileName(PluginEMotion);
                string fnFinal = GetFileName(PluginEMotionFinal);
                foreach (Atom at in GetPersonAtoms())
                {
                    TryRemovePluginFromPerson(at, fnPack);
                    TryRemovePluginFromPerson(at, fnFinal);
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
                string fn = GetFileName(PluginSpankings);
                foreach (Atom at in GetPersonAtoms())
                {
                    TryRemovePluginFromPerson(at, fn);
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
                IsPersonFemale,
                "Spankings merge on female Persons");
        }

        /// <summary>
        /// True when at least one female <c>Person</c> lacks Spankings.
        /// </summary>
        public bool AnyFemalePersonMissingSpankings()
        {
            try
            {
                string fn = GetFileName(PluginSpankings);
                foreach (Atom at in GetPersonAtoms())
                {
                    if (!IsPersonFemale(at))
                        continue;
                    if (!PersonHasPluginByFileName(at, fn))
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
                foreach (Atom at in GetPersonAtoms())
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
                string fn = GetFileName(PluginSpankings);
                foreach (Atom at in GetPersonAtoms())
                {
                    if (includePerson != null && !includePerson(at))
                        continue;
                    if (!PersonHasPluginByFileName(at, fn))
                        TryMergePluginOntoPerson(at, PluginSpankings);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError(logContext + ": " + e);
            }
        }

        private static List<Atom> GetPersonAtoms()
        {
            List<Atom> persons = new List<Atom>();
            SuperController sc = SuperController.singleton;

            if (sc == null)
                return persons;

            foreach (Atom at in sc.GetAtoms())
            {
                if (at != null && at.type == "Person")
                    persons.Add(at);
            }

            return persons;
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
            removeUnderwearButton = AddButton(
                "Remove underwear",
                RemoveUnderwearOnAllPersons,
                3,
                0,
                rightColButtonWidth);

            emotionOriginalHudButton = AddButton(
                "E-Motion Original",
                MergeEmotionOnAllPersonsOnly,
                1,
                1,
                emotionColButtonWidth);
            stripAllClothesButton = AddButton(
                "Remove All Clothes",
                StripAllClothesOnAllPersons,
                3,
                1,
                rightColButtonWidth);

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
                2,
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
                3,
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
            if (stripAllClothesButton != null)
                stripAllClothesButton.gameObject.SetActive(setToActive);
            if (removeUnderwearButton != null)
                removeUnderwearButton.gameObject.SetActive(setToActive);

            if (setToActive)
                RefreshPluginToggleLabels();
        }

        /// <summary>
        /// Called when the scene set may have changed without a load-dir change
        /// (e.g. same save reloaded).
        /// </summary>
        internal void InvalidateCachedPersonLists()
        {
            InvalidatePersonGenderCaches();
        }

        internal void ClothingResetCycle()
        {
            InvalidatePersonGenderCaches();
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
                string desiredFileName = GetFileName(PluginSpankings);
                bool turningOff =
                    AllPersonAtomsHavePluginByFileName(desiredFileName);

                if (turningOff)
                {
                    RemoveSpankingsFromAllPersons();
                }
                else
                {
                    foreach (Atom at in GetPersonAtoms())
                    {
                        if (!PersonHasPluginByFileName(at, desiredFileName))
                            TryMergePluginOntoPerson(at, PluginSpankings);
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

            string fileName = GetFileName(pluginPath);
            bool allHave = AllPersonAtomsHavePluginByFileName(fileName);
            button.label = (allHave ? "- " : "+ ") + labelBase;
        }

        private static bool AllPersonAtomsHavePluginByFileName(string desiredFileName)
        {
            bool any = false;
            foreach (Atom at in GetPersonAtoms())
            {
                any = true;
                if (!PersonHasPluginByFileName(at, desiredFileName))
                    return false;
            }

            return any;
        }

        private static bool PersonHasPluginByFileName(
            Atom atom,
            string desiredFileName)
        {
            MVRPluginManager pluginManager =
                atom.GetStorableByID("PluginManager") as MVRPluginManager;
            if (pluginManager == null)
                return false;

            foreach (string path in CollectNormalizedPluginPaths(pluginManager))
            {
                if (GetFileName(path) == desiredFileName)
                    return true;
            }

            return false;
        }

        private static List<string> CollectNormalizedPluginPaths(
            MVRPluginManager pluginManager)
        {
            List<string> paths = new List<string>();
            JSONClass current = pluginManager.GetJSON(true, true, true);

            if (current["plugins"] == null ||
                current["plugins"]["plugin#0"] == null ||
                current["plugins"]["plugin#0"].Value == "")
            {
                return paths;
            }

            foreach (JSONNode pluginNode in current["plugins"].Childs)
            {
                string path = pluginNode.Value;
                int folderSeparatorIndex;

                if (path.StartsWith("./"))
                    path = SuperController.singleton.currentSaveDir + "/" +
                        path.Substring(2);

                folderSeparatorIndex = path.LastIndexOf("/");
                if (folderSeparatorIndex < 0)
                {
                    path = SuperController.singleton.currentSaveDir + "/" + path;
                    folderSeparatorIndex = path.LastIndexOf("/");
                }

                if (folderSeparatorIndex > 0 &&
                    folderSeparatorIndex < path.Length - 1 &&
                    !FileExists(path))
                {
                    string scriptInStandardFolder =
                        "Custom/Scripts/" + GetFileName(path);
                    if (FileExists(scriptInStandardFolder))
                        path = scriptInStandardFolder;
                    else
                        continue;
                }

                paths.Add(path);
            }

            return paths;
        }

        private static void ApplyPluginPathsToManager(
            MVRPluginManager pluginManager,
            List<string> paths)
        {
            if (paths == null || paths.Count == 0)
            {
                const string emptyPluginManager =
                    "{ \"id\" : \"PluginManager\", \"plugins\" : { } }";
                JSONClass emptyState =
                    JSONNode.Parse(emptyPluginManager).AsObject;
                pluginManager.LateRestoreFromJSON(emptyState);
                return;
            }

            pluginManager.LateRestoreFromJSON(CreatePluginJSON(paths.ToArray()));
        }

        internal static void TryMergePluginOntoPerson(
            Atom atom,
            string desiredPluginPath)
        {
            MVRPluginManager pluginManager =
                atom.GetStorableByID("PluginManager") as MVRPluginManager;
            if (pluginManager == null)
                return;

            string desiredFileName = GetFileName(desiredPluginPath);
            List<string> paths = CollectNormalizedPluginPaths(pluginManager);
            bool has = false;

            foreach (string path in paths)
            {
                if (GetFileName(path) == desiredFileName)
                {
                    has = true;
                    break;
                }
            }

            if (!has)
                paths.Add(desiredPluginPath);

            ApplyPluginPathsToManager(pluginManager, paths);
        }

        private static void TryRemovePluginFromPerson(
            Atom atom,
            string desiredFileName)
        {
            MVRPluginManager pluginManager =
                atom.GetStorableByID("PluginManager") as MVRPluginManager;
            if (pluginManager == null)
                return;

            List<string> paths = CollectNormalizedPluginPaths(pluginManager);
            paths.RemoveAll(path => GetFileName(path) == desiredFileName);
            ApplyPluginPathsToManager(pluginManager, paths);
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

            fnOrig = GetFileName(PluginEMotion);
            fnLite = GetFileName(PluginEMotionLite);
            fnFinal = GetFileName(PluginEMotionFinal);
            paths = CollectNormalizedPluginPaths(pluginManager);
            paths.RemoveAll(path =>
            {
                string fileName = GetFileName(path);
                return fileName == fnOrig ||
                    fileName == fnLite ||
                    fileName == fnFinal;
            });
            paths.Add(desiredPluginPath);
            ApplyPluginPathsToManager(pluginManager, paths);
        }

        private static DAZCharacterSelector TryGetCharacterSelector(Atom atom)
        {
            JSONStorable geometry = atom.GetStorableByID("geometry");
            return geometry as DAZCharacterSelector;
        }

        private static string ClothingSearchBlob(DAZClothingItem item)
        {
            string blob = " " + (item.displayName ?? "") + " " +
                (item.tags ?? "") + " ";

            if (item.tagsArray != null)
            {
                foreach (string tag in item.tagsArray)
                {
                    if (!string.IsNullOrEmpty(tag))
                        blob += tag + " ";
                }
            }

            return blob.ToLowerInvariant();
        }

        private static bool LooksLikeSkirtDressOuterGarment(DAZClothingItem item)
        {
            string blob = ClothingSearchBlob(item);
            string[] avoid =
            {
                "skirt", "dress", "gown", "catsuit", "jumpsuit", "hobble",
                "kilt", "robe", "sari", "cheongsam", "ballgown"
            };

            foreach (string token in avoid)
            {
                if (blob.Contains(token))
                    return true;
            }

            return false;
        }

        private static bool IsUnderwearLikeItem(DAZClothingItem item)
        {
            string blob;
            string[] keywords =
            {
                "bra", "panty", "panties", "underwear", "thong", "brief",
                "bikini", "lingerie", "boxer", "boyshort", "pantie",
                "undershirt", "camisole", "pantyhose", "stocking", "garter",
                "corset ", " bustier"
            };

            if (!item.active)
                return false;
            if (LooksLikeSkirtDressOuterGarment(item))
                return false;

            if (item.exclusiveRegion ==
                    DAZClothingItem.ExclusiveRegion.UnderChest ||
                item.exclusiveRegion ==
                    DAZClothingItem.ExclusiveRegion.UnderHip)
            {
                return true;
            }

            blob = ClothingSearchBlob(item);
            foreach (string keyword in keywords)
            {
                if (blob.Contains(keyword))
                    return true;
            }

            return false;
        }

        private static void StripAllClothesOnAllPersons()
        {
            try
            {
                foreach (Atom at in GetPersonAtoms())
                {
                    DAZCharacterSelector character = TryGetCharacterSelector(at);
                    if (character == null)
                        continue;

                    try
                    {
                        character.EnableUndressAllClothingItems();
                    }
                    catch
                    {
                    }

                    foreach (DAZClothingItem item in character.clothingItems.ToList())
                        character.SetActiveClothingItem(item, false);
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Strip all clothes: " + e);
            }
        }

        private static void RemoveUnderwearOnAllPersons()
        {
            try
            {
                foreach (Atom at in GetPersonAtoms())
                {
                    DAZCharacterSelector character = TryGetCharacterSelector(at);
                    if (character == null)
                        continue;

                    try
                    {
                        character.EnableUndressAllClothingItems();
                    }
                    catch
                    {
                    }

                    foreach (DAZClothingItem item in character.clothingItems.ToList())
                    {
                        if (IsUnderwearLikeItem(item))
                            character.SetActiveClothingItem(item, false);
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Remove underwear: " + e);
            }
        }

        private static bool IsPersonFemale(Atom atom)
        {
            if (atom == null || atom.type != "Person")
                return false;

            DAZCharacter dazCharacter = atom.GetComponentInChildren<DAZCharacter>();
            return dazCharacter != null && !dazCharacter.isMale;
        }

        private static bool IsMalePerson(Atom atom)
        {
            return atom != null && atom.type == "Person" && !IsPersonFemale(atom);
        }

        private static List<Atom> FemalePersonsByUid()
        {
            EnsurePersonGenderCaches();
            return new List<Atom>(_cachedFemalePersonsByUid);
        }

        private static List<Atom> MalePersonsByUid()
        {
            EnsurePersonGenderCaches();
            return new List<Atom>(_cachedMalePersonsByUid);
        }

        private static JSONClass CreatePluginJSON(string[] pluginList)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.Append("{");
            builder.Append(" \"id\": \"PluginManager\",");
            builder.Append(" \"plugins\": {");
            for (int i = 0; i < pluginList.Length; i++)
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

        private static string GetFileName(string relativePath)
        {
            return relativePath.Substring(
                relativePath.LastIndexOfAny(new[] { '/', '\\' }) + 1);
        }

        private static bool FileExists(string relativePath)
        {
            int folderSeparatorIndex;
            string pathFolder;
            string pathFile;
            string[] pathFileList;

            folderSeparatorIndex = relativePath.LastIndexOfAny(
                new[] { '/', '\\' });
            if (folderSeparatorIndex < 0)
                return false;

            pathFolder = relativePath.Substring(0, folderSeparatorIndex);
            pathFile = relativePath.Substring(folderSeparatorIndex + 1);
            pathFileList = SuperController.singleton.GetFilesAtPath(pathFolder);
            if (pathFileList == null || pathFileList.Length == 0)
                return false;

            foreach (string foundPathFile in pathFileList)
            {
                string correctedPathFile = foundPathFile.Replace("\\", "/");
                if (correctedPathFile.EndsWith("/" + pathFile))
                    return true;
            }

            return false;
        }
    }
}
