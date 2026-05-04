using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.XR;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    // World-space HUD: Ctrl+Shift+S = toggle Spankings off / merge onto Persons
    // missing it; Possess+Align+Select (F/M/P) merges Spankings onto other
    // Persons
    // missing it when at least one possessed hand on the target; F = freeze
    // animation (VaM HUD); Y = pose log — Shift+Y when isOVR/isOpenVR,
    // else plain Y; K = write camera/rig patch request + run Python on current
    // scene JSON
    // (currentLoadDir); E-Motion HUD: Lite / Original / M-F gender / Final /
    // remove-all; swaps via TryReplaceEmotionFamilyWithExactPath; I / VR gestures
    // (see EasyMateVrGestureRuntime): over-HMD hand zone → unpossess all;
    // ~3 s dual-hand HMD-relative euler windows → Possess+Align+Select closest
    // female; P =
    // Possess+Align+Select closest Person by head;
    // O = unpossess all; C = cycle Female then Male Persons (uid), Edit +
    // Selected
    // Options + root control.
    public class MainUIButtons
    {
        public const string PluginEMotion = "Custom/Scripts/AutoMate/PERSON_PLUGINS/E-Motion - VaM Auto Blink/E-Motion_AddThisONLY.cslist";
        /// <summary>Lite face/emotion pack (no scripted head/neck / eye target per fork). Loaded when <see cref="EasyMateEmotionPathKeywords"/> matches load/save folder paths (<see cref="EasyMateEmotionPathKeywords.KeywordsFileRelative"/>). Uses the same cslist file name as <see cref="PluginEMotion"/>; Easy Mate swaps packs via <see cref="TryReplaceEmotionFamilyWithExactPath"/>.</summary>
        public const string PluginEMotionLite = "Custom/Scripts/E-MotionLite/E-Motion_AddThisONLY.cslist";
        /// <summary>VRAdultFun “Final” pack; own folder and unique <c>.cslist</c> basename so it is independent of AutoMate original and Lite sources.</summary>
        public const string PluginEMotionFinal = "Custom/Scripts/E-MotionFinal/E-Motion_Final_AddThisONLY.cslist";
        public const string PluginSpankings = "Custom/Scripts/Spankings/Spankings.cslist";

        /// <summary>Label passed to <see cref="StartAutoPossessRoutine"/> for the dual-hand euler VR gesture.</summary>
        public const string VrEulerPossessLabel = "VR euler";
        public const string PluginEasyMateClothingTouchFallOff = "Custom/Scripts/Easy Mate/EasyMateClothingTouchFallOff.cslist";

        /// <summary>Scene atom UIDs created by <c>octopussy.Spankings</c>; removed when Spankings is toggled off (<see cref="RemoveSpankingsFromAllPersons"/>).</summary>
        private static readonly string[] SpankingsOwnedSceneAtomUids =
        {
            "HitAudioSource",
            "CheekLeft",
            "CheekRight"
        };

        private static MVRScript _pluginHost;
        private static Coroutine _autoPossessCoroutine;
        private static Coroutine _autoPossessConfirmCo;
        private static Coroutine _vrPalmHudMenuConfirmCo;
        /// <summary>Next index for <see cref="HotkeySnapNearestHeadHideHandsThenSnap"/> among <see cref="AllPersonsSortedByUidForISnapCycle"/>.</summary>
        private static int _hotkeyISnapPersonCycleNextIndex;
        /// <summary>Set in <see cref="Init"/> so static possess coroutine can refresh HUD after merging plugins.</summary>
        private static System.Action _refreshPluginToggleLabelsStatic;

        /// <summary>Rebuilt from <see cref="SuperController.GetAtoms"/> when invalid; see <see cref="InvalidatePersonGenderCaches"/>.</summary>
        private static bool _personGenderListsCacheValid;
        private static List<Atom> _cachedFemalePersonsByUid;
        private static List<Atom> _cachedMalePersonsByUid;

        private static void InvalidatePersonGenderCaches()
        {
            _personGenderListsCacheValid = false;
            _cachedFemalePersonsByUid = null;
            _cachedMalePersonsByUid = null;
        }

        private static void EnsurePersonGenderCaches()
        {
            if (_personGenderListsCacheValid)
                return;
            SuperController sc = SuperController.singleton;
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
            _hotkeyISnapPersonCycleNextIndex = 0;
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

        /// <summary>Offset snap target for Snap F (and any caller that wants a slightly raised/backed eye point). Snap M aligns to <see cref="GetPossessionMatchHeadSnapWorldPosition"/> instead.</summary>
        private const float SnapHeadTargetAboveControlMeters = 0.15f;
        private const float SnapHeadTargetBackAlongPossessMeters = 0.05f;

        /// <summary>Same anchor VaM uses for head possession (<c>headControl</c> transform / control), no Easy Mate offset.</summary>
        private static Vector3 GetPossessionMatchHeadSnapWorldPosition(FreeControllerV3 head)
        {
            if (head == null)
                return Vector3.zero;
            if (head.control != null)
                return head.control.position;
            if (head.possessPoint != null)
                return head.possessPoint.position;
            return head.transform.position;
        }

        private static Vector3 GetHeadSnapTargetWorld(FreeControllerV3 head)
        {
            Vector3 basePos = head.control != null
                ? head.control.position
                : (head.possessPoint != null ? head.possessPoint.position : head.transform.position);

            Vector3 upAxis = head.GetUpPossessAxis();
            Vector3 faceForward = head.GetForwardPossessAxis();
            if (upAxis.sqrMagnitude < 1e-12f)
            {
                if (head.control != null)
                    upAxis = head.control.up;
                else
                    upAxis = Vector3.up;
            }
            if (faceForward.sqrMagnitude < 1e-12f)
            {
                if (head.followWhenOff != null)
                    faceForward = head.followWhenOff.forward;
                else if (head.control != null)
                    faceForward = head.control.forward;
                else
                    faceForward = Vector3.forward;
            }
            upAxis.Normalize();
            faceForward.Normalize();

            return basePos + upAxis * SnapHeadTargetAboveControlMeters - faceForward * SnapHeadTargetBackAlongPossessMeters;
        }

        MVRScript plugin;

        private Camera _mainCamera;
        public static Canvas canvas = null;

        private bool isDesktopMode = false;

        UIDynamicButton emotionLiteHudButton = null;
        UIDynamicButton emotionOriginalHudButton = null;
        UIDynamicButton emotionMaleHudButton = null;
        UIDynamicButton emotionFemaleHudButton = null;
        UIDynamicButton emotionFinalHudButton = null;
        UIDynamicButton emotionRemoveAllHudButton = null;
        UIDynamicButton spankingsButton = null;
        UIDynamicButton stripAllClothesButton = null;
        UIDynamicButton removeUnderwearButton = null;
        UIDynamicButton snapFemaleHeadButton = null;
        UIDynamicButton snapMaleHeadButton = null;
        UIDynamicButton possessAlignSelectFemaleButton = null;
        UIDynamicButton possessAlignSelectMaleButton = null;

        private EasyMateVrGestureBindings _vrGestureBindings;

        private static float _lastYDebugLogUnscaledTime = -1000f;
        private const float YDebugLogMinIntervalSeconds = 0.35f;

        public void Init(MVRScript _plugin)
        {
            plugin = _plugin;
            _pluginHost = _plugin;
            _mainCamera = CameraTarget.centerTarget?.targetCamera;
            isDesktopMode = !(SuperController.singleton.isOVR || SuperController.singleton.isOpenVR);
            RegisterPersonGenderCacheInvalidation();
            _refreshPluginToggleLabelsStatic = RefreshPluginToggleLabels;

            _vrGestureBindings = new EasyMateVrGestureBindings();
            _vrGestureBindings.TriggerVrOverHeadHandUnpossessAll =
                delegate()
                {
                    RequestClearAllPossession(
                        "Easy Mate: VR over-HMD hand — cleared possession.");
                };
            _vrGestureBindings.TriggerPossessAlignSelectClosestFemaleByHead =
                delegate() { PossessAlignSelectClosestFemaleByHeadToCamera(); };
        }

        /// <summary>
        /// Call from session plugin <c>Update</c>. <b>Ctrl+Shift+S</b>
        /// toggles Spankings (same as HUD <b>+/- Spankings Male</b>): removes when
        /// everyone has it; otherwise merges onto Persons that do not.
        /// <b>Y</b> logs look camera / HMD-related poses (debounced ~0.35s).
        /// With Oculus or OpenVR active, hold <b>Shift+Y</b> so the controller
        /// Y binding does not spam logs; <c>XRSettings.enabled</c> alone is not
        /// used for that gate (it often stays true with drivers while using the
        /// desktop keyboard).
        /// E‑Motion merges via HUD: <b>Lite</b>, <b>Original</b>
        /// (everyone), <b>M</b> / <b>F</b> (<see cref="PluginEMotion"/> males
        /// or females only), <b>Final</b>, <b>Remove all</b> — replacing other
        /// family packs first.
        /// <b>O</b> stops auto-possess and
        /// <see cref="SuperController.ClearPossess"/>.
        /// <b>I</b> hides VR hand models then cycles rig snap across
        /// <b>Person</b> heads by uid (same rules as <b>Passenger Female</b> /
        /// <b>Passenger Male</b> per figure). VR: <see cref="EasyMateVrGestureRuntime"/> — e.g.
        /// right hand over the HMD (height + lateral cap), 4s cooldown, unpossess
        /// all once per visit until the hand exits; ~3s dual-hand euler vs HMD
        /// triggers Possess+Align+Select for the closest female by head (skipped
        /// while any Person head/hand is already possessed); more gestures can use
        /// the same pipeline.
        /// <b>P</b> runs the same <b>Possess+Align+Select</b> flow as the HUD
        /// buttons on the <b>closest Person by head</b> to the look/center
        /// camera (not alphabetically first F/M).
        /// <b>C</b> (without Shift, Ctrl, or Alt) cycles visible Person atoms
        /// in order: all <b>female</b> then all <b>male</b> (by atom uid),
        /// switches to <b>Edit</b>, shows the main HUD, opens
        /// <b>Selected Options</b>,
        /// and selects each atom's root <c>control</c> (or the first free
        /// controller if there is no <c>control</c>).
        /// <b>F</b> toggles VaM <b>Freeze animation</b> (same as the main HUD
        /// toggle).
        /// <b>K</b> logs navigation rig / monitor peel / <c>WindowCamera</c> /
        /// <c>playerHeightAdjust</c> and runs
        /// <see cref="EasyMateKSceneCameraPatch"/> (Python patch of the main
        /// scene JSON under <see cref="SuperController.currentLoadDir"/>).
        /// Blocked when Ctrl/Alt is held (same gate as O/I/P).
        /// Skips while VaM is loading or a Unity UI text field has focus.
        /// </summary>
        public void ProcessHotkeysUpdate()
        {
            if (plugin == null || SuperController.singleton == null)
                return;
            if (SuperController.singleton.isLoading)
                return;
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                return;

            if (Input.GetKeyDown(KeyCode.S) &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                try
                {
                    ToggleSpankingsPluginOnAllPersons();
                }
                catch (Exception e)
                {
                    SuperController.LogError("Ctrl+Shift+S hotkey (Spankings toggle): " + e);
                }

                return;
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                return;

            if (Input.GetKeyDown(KeyCode.K))
            {
                try
                {
                    EasyMateKSceneCameraPatch.TryRunFromHotkey();
                }
                catch (Exception e)
                {
                    SuperController.LogError("K hotkey (patch scene JSON camera / rig): " + e);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                try
                {
                    ClearAllPossession("Easy Mate: O — cleared possession.");
                }
                catch (Exception e)
                {
                    SuperController.LogError("O hotkey (clear possession): " + e);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                try
                {
                    ToggleFreezeAnimationHotkey();
                }
                catch (Exception e)
                {
                    SuperController.LogError("F hotkey (freeze animation toggle): " + e);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                HotkeySnapNearestHeadHideHandsThenSnap();
                return;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                try
                {
                    PossessAlignSelectClosestPersonByHeadToCamera();
                }
                catch (Exception e)
                {
                    SuperController.LogError("P hotkey (Possess+Align+Select closest): " + e);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.C) && !Input.GetKey(KeyCode.LeftShift) &&
                !Input.GetKey(KeyCode.RightShift))
            {
                try
                {
                    CycleFemaleThenMalePersonRootEditMenuOnHotkey();
                }
                catch (Exception e)
                {
                    SuperController.LogError("C hotkey (cycle Person edit root): " + e);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                SuperController scY = SuperController.singleton;
                // Shift+Y only when VaM is in Oculus/OpenVR mode — not XRSettings.enabled (often true with SteamVR etc. while using keyboard).
                bool yNeedsShiftForControllerConflict = scY != null && (scY.isOVR || scY.isOpenVR);
                if (yNeedsShiftForControllerConflict && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                    return;

                float now = Time.unscaledTime;
                if (now - _lastYDebugLogUnscaledTime < YDebugLogMinIntervalSeconds)
                    return;
                _lastYDebugLogUnscaledTime = now;

                try
                {
                    LogYKeyHmdPoseDebug();
                }
                catch (Exception e)
                {
                    SuperController.LogError("Y debug (HMD pose): " + e);
                }

                return;
            }

            try
            {
                EasyMateVrGestureRuntime.ProcessUpdate(_vrGestureBindings);
            }
            catch (Exception e)
            {
                SuperController.LogError("Easy Mate VR gestures: " + e);
            }
        }

        /// <summary>
        /// Desktop <c>C</c> (without Shift): cycles visible Person atoms — all females (by uid) then all males — selects root <c>control</c>, Edit mode, shows main HUD and Selected Options panel (matches VaM edit/target UI).
        /// </summary>
        private static void CycleFemaleThenMalePersonRootEditMenuOnHotkey()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            EnsurePersonGenderCaches();
            List<Atom> cycle = new List<Atom>();
            foreach (Atom a in _cachedFemalePersonsByUid)
            {
                if (a != null && !a.hidden && PersonHasFreeControllers(sc, a))
                    cycle.Add(a);
            }

            foreach (Atom a in _cachedMalePersonsByUid)
            {
                if (a != null && !a.hidden && PersonHasFreeControllers(sc, a))
                    cycle.Add(a);
            }

            if (cycle.Count == 0)
                return;

            int idx = -1;
            Atom sel = sc.GetSelectedAtom();
            if (sel != null && sel.type == "Person")
                idx = cycle.FindIndex(x => x.uid == sel.uid);

            int next = (idx + 1) % cycle.Count;
            Atom target = cycle[next];
            string rootName = ResolvePersonRootControllerName(sc, target);
            if (rootName == null)
                return;

            sc.ShowMainHUDAuto();
            sc.gameMode = SuperController.GameMode.Edit;
            sc.SelectController(target.uid, rootName, alignView: false);
            SyncHudPopupsToSelectedController(sc);
            sc.activeUI = SuperController.ActiveUI.SelectedOptions;
        }

        private static bool PersonHasFreeControllers(SuperController sc, Atom person)
        {
            if (sc == null || person == null || person.type != "Person")
                return false;
            List<string> names = sc.GetFreeControllerNamesInAtom(person.uid);
            return names != null && names.Count > 0;
        }

        private static string ResolvePersonRootControllerName(SuperController sc, Atom person)
        {
            if (person == null || sc == null)
                return null;
            List<string> names = sc.GetFreeControllerNamesInAtom(person.uid);
            if (names == null || names.Count == 0)
                return null;
            if (names.Contains("control"))
                return "control";
            return names[0];
        }

        private static void SyncHudPopupsToSelectedController(SuperController sc)
        {
            if (sc == null)
                return;
            FreeControllerV3 fc = sc.GetSelectedController();
            if (fc == null || fc.containingAtom == null)
                return;
            if (sc.selectAtomPopup != null)
                sc.selectAtomPopup.currentValue = fc.containingAtom.uid;
            if (sc.selectControllerPopup != null)
                sc.selectControllerPopup.currentValueNoCallback = fc.name;
        }

        private static void LogYKeyHmdPoseDebug()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            if (sc.lookCamera != null)
            {
                Transform t = sc.lookCamera.transform;
                SuperController.LogMessage(string.Format(
                    "EasyMate Y debug: lookCamera world pos=({0:F4},{1:F4},{2:F4}) euler=({3:F2},{4:F2},{5:F2})",
                    t.position.x, t.position.y, t.position.z,
                    t.rotation.eulerAngles.x, t.rotation.eulerAngles.y, t.rotation.eulerAngles.z));
            }
            else
                SuperController.LogMessage("EasyMate Y debug: lookCamera is null");

            if (CameraTarget.centerTarget != null && CameraTarget.centerTarget.targetCamera != null)
            {
                Transform ct = CameraTarget.centerTarget.targetCamera.transform;
                SuperController.LogMessage(string.Format(
                    "EasyMate Y debug: centerTarget camera world pos=({0:F4},{1:F4},{2:F4}) euler=({3:F2},{4:F2},{5:F2})",
                    ct.position.x, ct.position.y, ct.position.z,
                    ct.rotation.eulerAngles.x, ct.rotation.eulerAngles.y, ct.rotation.eulerAngles.z));
            }
            else
                SuperController.LogMessage("EasyMate Y debug: CameraTarget.centerTarget / targetCamera is null");

            if (sc.navigationRig != null)
            {
                Transform nr = sc.navigationRig;
                SuperController.LogMessage(string.Format(
                    "EasyMate Y debug: navigationRig world pos=({0:F4},{1:F4},{2:F4}) euler=({3:F2},{4:F2},{5:F2})",
                    nr.position.x, nr.position.y, nr.position.z,
                    nr.rotation.eulerAngles.x, nr.rotation.eulerAngles.y, nr.rotation.eulerAngles.z));
            }

            try
            {
                if (XRSettings.enabled)
                {
                    Vector3 lp = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
                    Quaternion lr = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
                    SuperController.LogMessage(string.Format(
                        "EasyMate Y debug: XR CenterEye local pos=({0:F4},{1:F4},{2:F4}) local euler=({3:F2},{4:F2},{5:F2})",
                        lp.x, lp.y, lp.z, lr.eulerAngles.x, lr.eulerAngles.y, lr.eulerAngles.z));
                }
            }
            catch (Exception e)
            {
                SuperController.LogMessage("EasyMate Y debug: XR InputTracking: " + e.Message);
            }
        }

        private static void ToggleFreezeAnimationHotkey()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;

            bool currentlyOn = false;
            if (sc.freezeAnimationToggle != null)
                currentlyOn = sc.freezeAnimationToggle.isOn;
            else if (sc.freezeAnimationToggleAlt != null)
                currentlyOn = sc.freezeAnimationToggleAlt.isOn;
            else
                currentlyOn = sc.freezeAnimation;

            sc.SetFreezeAnimation(!currentlyOn);
        }

        /// <summary>All <c>Person</c> atoms, stable uid order (I hotkey snap cycle).</summary>
        private static List<Atom> AllPersonsSortedByUidForISnapCycle()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return new List<Atom>();
            return sc.GetAtoms()
                .Where(a => a != null && a.type == "Person")
                .OrderBy(a => a.uid, StringComparer.Ordinal)
                .ToList();
        }

        private void HotkeySnapNearestHeadHideHandsThenSnap()
        {
            try
            {
                EasyMateGripHandVisibility.DisableVrHandModelsForSceneStart();
                SuperController sc = SuperController.singleton;
                if (sc == null || sc.isLoading)
                    return;

                List<Atom> persons = AllPersonsSortedByUidForISnapCycle();
                if (persons.Count == 0)
                {
                    SuperController.LogMessage("Easy Mate: I — no Person in scene.");
                    return;
                }

                int n = persons.Count;
                int idx = _hotkeyISnapPersonCycleNextIndex % n;
                _hotkeyISnapPersonCycleNextIndex = (idx + 1) % n;
                Atom target = persons[idx];
                bool isFemale = IsPersonFemale(target);

                if (isFemale)
                    OneShotSnapRigToPersonHead(target, null, false);
                else
                    SnapRigToMalePersonHeadWithPostSteps(target);
            }
            catch (Exception e)
            {
                SuperController.LogError("I hotkey (hide hands + cycle head snap): " + e);
            }
        }

        private static void ClearAllPossession(string logMessage)
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;
            StopAutoPossessRoutine();
            sc.ClearPossess();
            UnlinkStrayHmdLinkedFreeControllersAndNaturalizeHeads(sc);
            if (!string.IsNullOrEmpty(logMessage))
                SuperController.LogMessage(logMessage);
        }

        /// <summary>
        /// After <see cref="SuperController.ClearPossess"/>, free any
        /// <see cref="FreeControllerV3"/> still parent-linked to the HMD/center
        /// camera rigidbody (Easy Mate can possess the head without VaM's
        /// tracked <c>headPossessedController</c>). For <c>headControl</c>,
        /// nudge pose toward neck/chest so the head sits naturally on the body.
        /// </summary>
        private static void UnlinkStrayHmdLinkedFreeControllersAndNaturalizeHeads(
            SuperController sc)
        {
            if (sc == null || sc.centerCameraTarget == null)
                return;
            Rigidbody hmdRb = sc.centerCameraTarget.GetComponent<Rigidbody>();
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
                        if (fc == null || fc.linkToRB != hmdRb)
                            continue;

                        bool isHead = a.type == "Person" &&
                            a.GetStorableByID("headControl") == fc;

                        fc.RestorePreLinkState();
                        if (fc.linkToRB == hmdRb)
                            fc.SelectLinkToRigidbody(null);
                        fc.possessed = false;
                        fc.startedPossess = false;

                        MotionAnimationControl mac =
                            fc.GetComponent<MotionAnimationControl>();
                        if (mac != null)
                        {
                            mac.suspendPositionPlayback = false;
                            mac.suspendRotationPlayback = false;
                        }

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

        /// <summary>Stops Easy Mate auto-possess coroutine and <see cref="SuperController.ClearPossess"/> (for hotkeys and auto-release).</summary>
        public static void RequestClearAllPossession(string logMessage)
        {
            ClearAllPossession(string.IsNullOrEmpty(logMessage) ? null : logMessage);
        }

        /// <summary>
        /// VR palm HUD: same as dual-hand euler possess (closest female by head).
        /// </summary>
        public static void RequestPossessClosestFemaleByVrHandHud()
        {
            PossessAlignSelectClosestFemaleByHeadToCamera();
        }

        /// <summary>
        /// VR palm HUD: <see cref="SuperController.GetMenuShow"/> (Quest <b>B</b> /
        /// SteamVR menu). Waits 100ms, clears <see cref="SuperController.activeUI"/>
        /// so the menu closes, then same Possess+Align+Select closest female as
        /// dual-hand euler (<see cref="PossessAlignSelectClosestFemaleByHeadToCamera"/>).
        /// </summary>
        public static void RequestVrPalmHudMenuButtonPossessAfterDismissMenu()
        {
            if (_pluginHost == null)
                return;
            if (_vrPalmHudMenuConfirmCo != null)
                return;
            _vrPalmHudMenuConfirmCo = _pluginHost.StartCoroutine(
                VrPalmHudMenuButtonPossessAfterDismissMenuCo());
        }

        private static IEnumerator VrPalmHudMenuButtonPossessAfterDismissMenuCo()
        {
            try
            {
                yield return new WaitForSecondsRealtime(0.1f);
                SuperController sc = SuperController.singleton;
                if (sc != null)
                    sc.activeUI = SuperController.ActiveUI.None;
                PossessAlignSelectClosestFemaleByHeadToCamera();
            }
            finally
            {
                _vrPalmHudMenuConfirmCo = null;
            }
        }

        private static void StopVrPalmHudMenuConfirmRoutine()
        {
            if (_pluginHost != null && _vrPalmHudMenuConfirmCo != null)
            {
                _pluginHost.StopCoroutine(_vrPalmHudMenuConfirmCo);
                _vrPalmHudMenuConfirmCo = null;
            }
        }

        private const string NextSceneUIButtonAtomUid = "nxtUIButton";
        private const string UiButtonTriggerStorableId = "Trigger";

        /// <summary>
        /// Pulses the scene &quot;next&quot; <c>UIButton</c> trigger: atom
        /// <see cref="NextSceneUIButtonAtomUid"/> if present, else any active
        /// <c>UIButton</c> whose UI <c>Text</c> contains &quot;Next&quot;
        /// (same as Quest thumbstick shortcut).
        /// </summary>
        public static void RequestFireNextSceneUiButton()
        {
            if (_pluginHost == null)
                return;
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            UIButtonTrigger ubt = TryResolveNextSceneUIButtonTrigger(sc);
            if (ubt == null || ubt.trigger == null)
                return;

            _pluginHost.StartCoroutine(FireUIButtonTriggerActivePulseCo(ubt));
        }

        private static UIButtonTrigger TryResolveNextSceneUIButtonTrigger(
            SuperController sc)
        {
            if (sc == null)
                return null;

            Atom byUid = sc.GetAtomByUid(NextSceneUIButtonAtomUid);
            if (byUid != null && byUid.gameObject.activeInHierarchy &&
                string.Equals(byUid.type, "UIButton", StringComparison.Ordinal))
            {
                UIButtonTrigger ubt =
                    byUid.GetStorableByID(UiButtonTriggerStorableId) as UIButtonTrigger;
                if (ubt != null && ubt.trigger != null)
                    return ubt;
            }

            foreach (Atom at in sc.GetAtoms())
            {
                if (at == null ||
                    !string.Equals(at.type, "UIButton", StringComparison.Ordinal) ||
                    !at.gameObject.activeInHierarchy)
                    continue;

                Text[] texts = at.gameObject.GetComponentsInChildren<Text>(true);
                if (texts == null)
                    continue;

                bool labelLooksLikeNext = false;
                for (int i = 0; i < texts.Length; i++)
                {
                    Text t = texts[i];
                    if (t == null || t.text == null)
                        continue;
                    if (t.text.Trim().IndexOf("next", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        labelLooksLikeNext = true;
                        break;
                    }
                }

                if (!labelLooksLikeNext)
                    continue;

                UIButtonTrigger ubt =
                    at.GetStorableByID(UiButtonTriggerStorableId) as UIButtonTrigger;
                if (ubt != null && ubt.trigger != null)
                    return ubt;
            }

            return null;
        }

        private static IEnumerator FireUIButtonTriggerActivePulseCo(UIButtonTrigger ubt)
        {
            if (ubt == null || ubt.trigger == null)
                yield break;
            ubt.trigger.active = true;
            yield return null;
            if (ubt != null && ubt.trigger != null)
                ubt.trigger.active = false;
        }

        /// <summary>Merges <see cref="PluginEMotionLite"/> onto every Person (HUD). Removes other E-Motion family entries first.</summary>
        public void MergeEmotionLiteOnAllPersonsOnly()
        {
            try
            {
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null)
                        continue;
                    TryReplaceEmotionFamilyWithExactPath(at, PluginEMotionLite);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-MotionLite merge on all Persons: " + e);
            }
        }

        /// <summary>Merges AutoMate E-Motion onto every Person. Removes any other E-Motion family entry (Lite, Final) on that atom, then installs <see cref="PluginEMotion"/>.</summary>
        public void MergeEmotionOnAllPersonsOnly()
        {
            try
            {
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null)
                        continue;
                    TryReplaceEmotionFamilyWithExactPath(at, PluginEMotion);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion merge on all Persons: " + e);
            }
        }

        /// <summary>
        /// Merges AutoMate E-Motion (<see cref="PluginEMotion"/>) onto every male
        /// <c>Person</c> only (same swap as HUD <b>E-Motion Original</b>,
        /// gender-filtered).
        /// </summary>
        public void MergeEmotionOriginalOnMalePersonsOnly()
        {
            try
            {
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null || !IsMalePerson(at))
                        continue;
                    TryReplaceEmotionFamilyWithExactPath(at, PluginEMotion);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion Original merge on males: " + e);
            }
        }

        /// <summary>
        /// Merges <see cref="PluginEMotion"/> onto every female <c>Person</c> only
        /// (<b>E‑Motion F</b> HUD).
        /// </summary>
        public void MergeEmotionOriginalOnFemalePersonsOnly()
        {
            try
            {
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null || !IsPersonFemale(at))
                        continue;
                    TryReplaceEmotionFamilyWithExactPath(at, PluginEMotion);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion Original merge on females: " + e);
            }
        }

        /// <summary>Installs <see cref="PluginEMotionLite"/> on every Person when the load/save path rule matches. Same merge as <see cref="MergeEmotionLiteOnAllPersonsOnly"/>.</summary>
        public void MergeEmotionLiteForPathRuleOnAllPersonsOnly()
        {
            MergeEmotionLiteOnAllPersonsOnly();
        }

        /// <summary>Merges <see cref="PluginEMotionFinal"/> onto every Person (HUD). Removes other E-Motion family entries first.</summary>
        public void MergeEmotionFinalOnAllPersonsOnly()
        {
            try
            {
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null)
                        continue;
                    TryReplaceEmotionFamilyWithExactPath(at, PluginEMotionFinal);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion Final merge on all Persons: " + e);
            }
        }

        /// <summary>Merges <see cref="PluginEMotionFinal"/> onto every <b>female</b> <c>Person</c> (used after long non-loop mocap ends — see <see cref="EasyMateMotionAnimationEmotionEnd"/>).</summary>
        public void MergeEmotionFinalOnFemalePersonsOnly()
        {
            try
            {
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null || !IsPersonFemale(at))
                        continue;
                    TryReplaceEmotionFamilyWithExactPath(at, PluginEMotionFinal);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion Final merge on female Persons: " + e);
            }
        }

        /// <summary>Merges Easy Mate clothing touch fall-off onto every Person (after scene settles; idempotent merge).</summary>
        public void MergeClothingTouchFallOffOnAllPersonsOnly()
        {
            try
            {
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null)
                        continue;
                    TryMergePluginOntoPerson(at, PluginEasyMateClothingTouchFallOff);
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Easy Mate clothing touch fall-off merge on all Persons: " + e);
            }
        }

        /// <summary>Removes AutoMate E-Motion, E-MotionLite, and E-Motion Final from every Person.</summary>
        public void RemoveEmotionFromAllPersons()
        {
            try
            {
                string fnPack = GetFileName(PluginEMotion);
                string fnFinal = GetFileName(PluginEMotionFinal);
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null)
                        continue;
                    TryRemovePluginFromPerson(at, fnPack);
                    TryRemovePluginFromPerson(at, fnFinal);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("Remove all E-Motion variants from all Persons: " + e);
            }
        }

        /// <summary>Removes Spankings from every Person and deletes Spankings-owned scene atoms (hit audio, cheek triggers) left behind when the plugin is stripped.</summary>
        public void RemoveSpankingsFromAllPersons()
        {
            try
            {
                string fn = GetFileName(PluginSpankings);
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null)
                        continue;
                    TryRemovePluginFromPerson(at, fn);
                }

                TryRemoveSpankingsOwnedSceneAtoms();

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("Spankings remove from all Persons: " + e);
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
                    Atom a = sc.GetAtomByUid(uid);
                    if (a == null)
                        continue;
                    sc.RemoveAtom(a);
                }
                catch (Exception e)
                {
                    SuperController.LogError("Easy Mate: remove Spankings scene atom \"" + uid + "\": " + e.Message);
                }
            }
        }

        /// <summary>Strip Spankings from every Person (same as HUD remove); static for VR euler possess.</summary>
        private static void RemoveSpankingsFromAllPersonsStatic()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return;
                string fn = GetFileName(PluginSpankings);
                foreach (Atom at in sc.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null)
                        continue;
                    TryRemovePluginFromPerson(at, fn);
                }

                TryRemoveSpankingsOwnedSceneAtoms();
                if (_refreshPluginToggleLabelsStatic != null)
                    _refreshPluginToggleLabelsStatic();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "Easy Mate: remove all Spankings (VR euler possess): " + e);
            }
        }

        /// <summary>Merges Spankings onto every <c>Person</c> that does not already have it (no strip pass).</summary>
        public void MergeSpankingsOnAllPersonsOnly()
        {
            try
            {
                string fn = GetFileName(PluginSpankings);
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null)
                        continue;
                    if (!PersonHasPluginByFileName(at, fn))
                        TryMergePluginOntoPerson(at, PluginSpankings);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("Spankings merge on all Persons: " + e);
            }
        }

        /// <summary>Merges Spankings (original full plugin) onto every <b>female</b> <c>Person</c> that does not already have it.</summary>
        public void MergeSpankingsOnFemalePersonsOnly()
        {
            try
            {
                string fn = GetFileName(PluginSpankings);
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null || !IsPersonFemale(at))
                        continue;
                    if (!PersonHasPluginByFileName(at, fn))
                        TryMergePluginOntoPerson(at, PluginSpankings);
                }

                RefreshPluginToggleLabels();
            }
            catch (Exception e)
            {
                SuperController.LogError("Spankings merge on female Persons: " + e);
            }
        }

        /// <summary>
        /// True when at least one female <c>Person</c> lacks Spankings (same file
        /// name check as <see cref="MergeSpankingsOnFemalePersonsOnly"/>).
        /// </summary>
        public bool AnyFemalePersonMissingSpankings()
        {
            try
            {
                string fn = GetFileName(PluginSpankings);
                foreach (Atom at in SuperController.singleton.GetAtoms()
                    .Where(a => a.type == "Person"))
                {
                    if (at == null || !IsPersonFemale(at))
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

        private void OnEmotionLiteHudClicked()
        {
            try
            {
                MergeEmotionLiteOnAllPersonsOnly();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion Lite HUD: " + e);
            }
        }

        private void OnEmotionOriginalHudClicked()
        {
            try
            {
                MergeEmotionOnAllPersonsOnly();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion Original HUD: " + e);
            }
        }

        private void OnEmotionMaleHudClicked()
        {
            try
            {
                MergeEmotionOriginalOnMalePersonsOnly();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion M HUD: " + e);
            }
        }

        private void OnEmotionFemaleHudClicked()
        {
            try
            {
                MergeEmotionOriginalOnFemalePersonsOnly();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion F HUD: " + e);
            }
        }

        private void OnEmotionFinalHudClicked()
        {
            try
            {
                MergeEmotionFinalOnAllPersonsOnly();
            }
            catch (Exception e)
            {
                SuperController.LogError("E-Motion Final HUD: " + e);
            }
        }

        private void OnEmotionRemoveAllHudClicked()
        {
            try
            {
                RemoveEmotionFromAllPersons();
            }
            catch (Exception e)
            {
                SuperController.LogError("Remove E-Motion HUD: " + e);
            }
        }

        public void Start()
        {
            Cleanup();
            float worldScale = SuperController.singleton.worldScale;
            SuperController.singleton.worldScale = 1.0f;
            CreateButtons();
            SuperController.singleton.worldScale = worldScale;
        }

        public void Cleanup()
        {
            if (canvas != null)
            {
                if (SuperController.singleton != null)
                {
                    SuperController.singleton.RemoveCanvas(canvas);
                }

                canvas.transform.SetParent(null, false);

                if (canvas.gameObject != null)
                {
                    GameObject.Destroy(canvas.gameObject);
                }
            }
        }

        public void CreateButtons()
        {
            float scale = 0.001f;

            Cleanup();

            GameObject canvasObject = new GameObject();
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.pixelPerfect = false;
            SuperController.singleton.AddCanvas(canvas);

            canvas.transform.SetParent(SuperController.singleton.mainHUD, false);

            CanvasScaler cs = canvasObject.AddComponent<CanvasScaler>();
            cs.scaleFactor = 80.0f;
            cs.dynamicPixelsPerUnit = 1f;

            canvasObject.AddComponent<GraphicRaycaster>();

            canvas.transform.localScale = new Vector3(scale, scale, scale);
            canvas.transform.localPosition = new Vector3(-.45f, -0.72f, 0.35f);

            LookAtCamera();

            // Columns 1–3 only; column 0 is VaMLogClipboardHud (separate plugin).
            const float emotionColButtonWidth = 132f;
            const float midColButtonWidth = 118f;
            const float rightColButtonWidth = 132f;

            emotionLiteHudButton = AddButton("E-Motion Lite", OnEmotionLiteHudClicked, 1, 0, emotionColButtonWidth);
            possessAlignSelectMaleButton = AddButton("Possess Male", PossessAlignSelectMaleIfAny, 2, 0, midColButtonWidth);
            removeUnderwearButton = AddButton("Remove underwear", RemoveUnderwearOnAllPersons, 3, 0, rightColButtonWidth);

            emotionOriginalHudButton = AddButton("E-Motion Original", OnEmotionOriginalHudClicked, 1, 1, emotionColButtonWidth);
            possessAlignSelectFemaleButton = AddButton("Possess Female", PossessAlignSelectFirstFemale, 2, 1, midColButtonWidth);
            stripAllClothesButton = AddButton("Remove All Clothes", StripAllClothesOnAllPersons, 3, 1, rightColButtonWidth);

            emotionFinalHudButton = AddButton("E-Motion Final", OnEmotionFinalHudClicked, 1, 2, emotionColButtonWidth);
            snapMaleHeadButton = AddButton("Passenger Male", SnapRigToClosestMaleHead, 2, 2, midColButtonWidth);
            spankingsButton = AddButton("+ Spankings Male", OnSpankingsPluginToggleClicked, 3, 2, rightColButtonWidth);

            emotionRemoveAllHudButton = AddButton("Remove E-Motion", OnEmotionRemoveAllHudClicked, 1, 3, emotionColButtonWidth);
            snapFemaleHeadButton = AddButton("Passenger Female", SnapRigToClosestFemaleHead, 2, 3, midColButtonWidth);

            emotionMaleHudButton = AddButton(
                "E-Motion M",
                OnEmotionMaleHudClicked,
                1,
                4,
                emotionColButtonWidth);
            emotionFemaleHudButton = AddButton(
                "E-Motion F",
                OnEmotionFemaleHudClicked,
                2,
                4,
                midColButtonWidth);

            RefreshPluginToggleLabels();

            canvas.transform.Translate(0, 0.2f, 0);
            // Same as EasyMate Hide UI — start collapsed until Show UI /
            // user toggles visibility.
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
            if (stripAllClothesButton != null)
                stripAllClothesButton.gameObject.SetActive(setToActive);
            if (removeUnderwearButton != null)
                removeUnderwearButton.gameObject.SetActive(setToActive);
            if (snapFemaleHeadButton != null)
                snapFemaleHeadButton.gameObject.SetActive(setToActive);
            if (snapMaleHeadButton != null)
                snapMaleHeadButton.gameObject.SetActive(setToActive);
            if (possessAlignSelectFemaleButton != null)
                possessAlignSelectFemaleButton.gameObject.SetActive(setToActive);
            if (possessAlignSelectMaleButton != null)
                possessAlignSelectMaleButton.gameObject.SetActive(setToActive);
            if (setToActive)
            {
                RefreshPluginToggleLabels();
            }
        }

        /// <summary>EasyMate still calls this on load-dir change; clothing UI was removed.</summary>
        public void ClothingResetCycle()
        {
            InvalidatePersonGenderCaches();
            RefreshPluginToggleLabels();
        }

        /// <summary>Call when the scene set may have changed without a load-dir change (e.g. same save reloaded).</summary>
        public void InvalidateCachedPersonLists()
        {
            InvalidatePersonGenderCaches();
        }

        public UIDynamicButton AddButton(string name, UnityAction callback, int column, int row, float width = 100f)
        {
            Color accessButtonColor = new Color(0.8392f, 0.8392f, 0.8392f);
            Color accessTextColor = new Color(0, 0, 0);
            float xSpacing = 0.22f;
            float ySpacing = 0.05f;

            UIDynamicButton button = CreateButton(name, width, 40);
            button.button.onClick.AddListener(callback);
            button.transform.Translate(column * xSpacing, 0.50f - row * ySpacing, 0, Space.Self);
            ColorButton(button, accessTextColor, accessButtonColor);

            return button;
        }

        public UIDynamicButton CreateButton(string name, float width = 100, float height = 80)
        {
            Transform button = GameObject.Instantiate<Transform>(plugin.manager.configurableButtonPrefab);
            ConfigureTransform(button, width, height);
            ParentToCanvas(button);

            UIDynamicButton uiButton = button.GetComponent<UIDynamicButton>();
            uiButton.label = name;
            uiButton.buttonText.fontSize = 18;
            return uiButton;
        }

        public static void ColorButton(UIDynamicButton button, Color textColor, Color buttonColor)
        {
            button.textColor = textColor;
            button.buttonColor = buttonColor;
        }

        private void ConfigureTransform(Transform t, float width, float height)
        {
            t.transform.position = Vector3.zero;
            RectTransform rt = t.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(width / 2, height / 2);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void ParentToCanvas(Transform t)
        {
            t.SetParent(canvas.transform, false);
        }

        public void LookAtCamera()
        {
            if (isDesktopMode)
            {
                canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
            }
            else
            {
                if (XRSettings.enabled == false)
                {
                    Transform cameraT = SuperController.singleton.lookCamera.transform;
                    Vector3 endPos = cameraT.position + cameraT.forward * 10000000.0f;
                    canvas.transform.LookAt(endPos, cameraT.up);
                }
                else
                {
                    canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
                }
            }
        }

        public void OnDestroy()
        {
            try
            {
                StopVrPalmHudMenuConfirmRoutine();
                StopAutoPossessRoutine();
                UnregisterPersonGenderCacheInvalidation();
                InvalidatePersonGenderCaches();
                _refreshPluginToggleLabelsStatic = null;

                if (SuperController.singleton != null)
                {
                    SuperController.singleton.RemoveCanvas(canvas);
                }

                if (canvas != null)
                {
                    canvas.transform.SetParent(null, false);

                    if (canvas.gameObject != null)
                    {
                        GameObject.Destroy(canvas.gameObject);
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void OnSpankingsPluginToggleClicked()
        {
            ToggleSpankingsPluginOnAllPersons();
        }

        /// <summary>Merge or remove Spankings on every Person (HUD and <b>Ctrl+Shift+S</b>). When disabling (everyone had it): full remove + cleanup scene atoms. When enabling: merge only onto Persons missing the plugin (does not strip scene-loaded Spankings first).</summary>
        private void ToggleSpankingsPluginOnAllPersons()
        {
            try
            {
                string desiredFileName = GetFileName(PluginSpankings);
                bool turningOff = AllPersonAtomsHavePluginByFileName(desiredFileName);
                if (turningOff)
                {
                    RemoveSpankingsFromAllPersons();
                }
                else
                {
                    foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                    {
                        if (at != null && !PersonHasPluginByFileName(at, desiredFileName))
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

        public void RefreshPluginToggleLabels()
        {
            SetPluginToggleLabel(spankingsButton, PluginSpankings, "Spankings Male");
        }

        private static void SetPluginToggleLabel(UIDynamicButton btn, string pluginPath, string labelBase)
        {
            if (btn == null || SuperController.singleton == null)
                return;
            string fn = GetFileName(pluginPath);
            bool allHave = AllPersonAtomsHavePluginByFileName(fn);
            string prefix = allHave ? "- " : "+ ";
            btn.label = prefix + labelBase;
        }

        private static bool AllPersonAtomsHavePluginByFileName(string desiredFileName)
        {
            IEnumerable<Atom> persons = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");
            bool any = false;
            foreach (Atom at in persons)
            {
                any = true;
                if (!PersonHasPluginByFileName(at, desiredFileName))
                    return false;
            }

            return any;
        }

        private static bool PersonHasPluginByFileName(Atom at, string desiredFileName)
        {
            MVRPluginManager manager = at.GetStorableByID("PluginManager") as MVRPluginManager;
            if (manager == null)
                return false;

            foreach (string path in CollectNormalizedPluginPaths(manager))
            {
                if (GetFileName(path) == desiredFileName)
                    return true;
            }

            return false;
        }

        private static List<string> CollectNormalizedPluginPaths(MVRPluginManager manager)
        {
            var newPlugins = new List<string>();
            JSONClass current = manager.GetJSON(true, true, true);

            if (current["plugins"] == null || current["plugins"]["plugin#0"] == null ||
                current["plugins"]["plugin#0"].Value == "")
                return newPlugins;

            foreach (JSONNode pluginNode in current["plugins"].Childs)
            {
                string path = pluginNode.Value;

                if (path.StartsWith("./"))
                    path = SuperController.singleton.currentSaveDir + "/" + path.Substring(2);

                int folderSeparatorIndex = path.LastIndexOf("/");
                if (folderSeparatorIndex < 0)
                {
                    path = SuperController.singleton.currentSaveDir + "/" + path;
                    folderSeparatorIndex = path.LastIndexOf("/");
                }

                if (folderSeparatorIndex > 0 && folderSeparatorIndex < path.Length - 1)
                {
                    if (!FileExists(path))
                    {
                        string scriptInStandardFolder = "Custom/Scripts/" + GetFileName(path);
                        if (FileExists(scriptInStandardFolder))
                            path = scriptInStandardFolder;
                        else
                            continue;
                    }
                }

                newPlugins.Add(path);
            }

            return newPlugins;
        }

        private static void ApplyPluginPathsToManager(MVRPluginManager manager, List<string> paths)
        {
            if (paths == null || paths.Count == 0)
            {
                const string emptyPluginManager = "{ \"id\" : \"PluginManager\", \"plugins\" : { } }";
                JSONClass jc = JSONNode.Parse(emptyPluginManager).AsObject;
                manager.LateRestoreFromJSON(jc);
            }
            else
            {
                manager.LateRestoreFromJSON(CreatePluginJSON(paths.ToArray()));
            }
        }

        private static void TryMergePluginOntoPerson(Atom at, string desiredPluginPath)
        {
            MVRPluginManager manager = at.GetStorableByID("PluginManager") as MVRPluginManager;
            if (manager == null)
                return;

            string desiredFileName = GetFileName(desiredPluginPath);
            List<string> newPlugins = CollectNormalizedPluginPaths(manager);

            bool has = false;
            foreach (string p in newPlugins)
            {
                if (GetFileName(p) == desiredFileName)
                {
                    has = true;
                    break;
                }
            }

            if (!has)
                newPlugins.Add(desiredPluginPath);

            ApplyPluginPathsToManager(manager, newPlugins);
        }

        private static void TryRemovePluginFromPerson(Atom at, string desiredFileName)
        {
            MVRPluginManager manager = at.GetStorableByID("PluginManager") as MVRPluginManager;
            if (manager == null)
                return;

            List<string> newPlugins = CollectNormalizedPluginPaths(manager);
            newPlugins.RemoveAll(p => GetFileName(p) == desiredFileName);
            ApplyPluginPathsToManager(manager, newPlugins);
        }

        /// <summary>
        /// Removes every known E-Motion plugin entry on <paramref name="at"/> (AutoMate original and Lite share <c>E-Motion_AddThisONLY.cslist</c>; Final uses <c>E-Motion_Final_AddThisONLY.cslist</c>),
        /// then adds exactly <paramref name="desiredPluginPath"/>. Keeps independent source trees on disk while enforcing one pack per merge action.
        /// </summary>
        private static void TryReplaceEmotionFamilyWithExactPath(Atom at, string desiredPluginPath)
        {
            MVRPluginManager manager = at.GetStorableByID("PluginManager") as MVRPluginManager;
            if (manager == null)
                return;

            string fnOrig = GetFileName(PluginEMotion);
            string fnLite = GetFileName(PluginEMotionLite);
            string fnFinal = GetFileName(PluginEMotionFinal);
            List<string> newPlugins = CollectNormalizedPluginPaths(manager);
            newPlugins.RemoveAll(p =>
            {
                string f = GetFileName(p);
                return f == fnOrig || f == fnLite || f == fnFinal;
            });
            newPlugins.Add(desiredPluginPath);
            ApplyPluginPathsToManager(manager, newPlugins);
        }

        private static DAZCharacterSelector TryGetCharacterSelector(Atom at)
        {
            JSONStorable geo = at.GetStorableByID("geometry");
            return geo as DAZCharacterSelector;
        }

        private static string ClothingSearchBlob(DAZClothingItem item)
        {
            string s = " " + (item.displayName ?? "") + " " + (item.tags ?? "") + " ";
            if (item.tagsArray != null)
            {
                foreach (string t in item.tagsArray)
                {
                    if (!string.IsNullOrEmpty(t))
                        s += t + " ";
                }
            }

            return s.ToLowerInvariant();
        }

        private static bool LooksLikeSkirtDressOuterGarment(DAZClothingItem item)
        {
            string blob = ClothingSearchBlob(item);
            string[] avoid =
            {
                "skirt", "dress", "gown", "catsuit", "jumpsuit", "hobble", "kilt", "robe", "sari", "cheongsam", "ballgown"
            };

            foreach (string a in avoid)
            {
                if (blob.Contains(a))
                    return true;
            }

            return false;
        }

        private static bool IsUnderwearLikeItem(DAZClothingItem item)
        {
            if (!item.active)
                return false;
            if (LooksLikeSkirtDressOuterGarment(item))
                return false;

            if (item.exclusiveRegion == DAZClothingItem.ExclusiveRegion.UnderChest ||
                item.exclusiveRegion == DAZClothingItem.ExclusiveRegion.UnderHip)
                return true;

            string blob = ClothingSearchBlob(item);
            string[] keys =
            {
                "bra", "panty", "panties", "underwear", "thong", "brief", "bikini", "lingerie", "boxer", "boyshort",
                "pantie", "undershirt", "camisole", "pantyhose", "stocking", "garter", "corset ", " bustier"
            };

            foreach (string k in keys)
            {
                if (blob.Contains(k))
                    return true;
            }

            return false;
        }

        private static void StripAllClothesOnAllPersons()
        {
            try
            {
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    DAZCharacterSelector ch = TryGetCharacterSelector(at);
                    if (ch == null)
                        continue;
                    try
                    {
                        ch.EnableUndressAllClothingItems();
                    }
                    catch
                    {
                    }

                    foreach (DAZClothingItem item in ch.clothingItems.ToList())
                        ch.SetActiveClothingItem(item, false);
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
                foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
                {
                    DAZCharacterSelector ch = TryGetCharacterSelector(at);
                    if (ch == null)
                        continue;
                    try
                    {
                        ch.EnableUndressAllClothingItems();
                    }
                    catch
                    {
                    }

                    foreach (DAZClothingItem item in ch.clothingItems.ToList())
                    {
                        if (IsUnderwearLikeItem(item))
                            ch.SetActiveClothingItem(item, false);
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Remove underwear: " + e);
            }
        }

        private static bool IsPersonFemale(Atom a)
        {
            if (a == null || a.type != "Person")
                return false;
            DAZCharacter d = a.GetComponentInChildren<DAZCharacter>();
            return d != null && !d.isMale;
        }

        /// <summary>Male <c>Person</c> (VaM geometry); complements <see cref="IsPersonFemale"/>.</summary>
        public static bool IsMalePerson(Atom a)
        {
            return a != null && a.type == "Person" && !IsPersonFemale(a);
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

        private static Transform GetMotionControllerTransform(SuperController sc, bool left)
        {
            if (sc == null)
                return null;

            return left ? sc.leftHand : sc.rightHand;
        }

        private static Vector3 GetPassengerHeadTargetWorldPosition(
            FreeControllerV3 head)
        {
            if (head != null && head.containingAtom != null)
            {
                Rigidbody[] bodies = head.containingAtom.linkableRigidbodies;
                if (bodies != null)
                {
                    foreach (Rigidbody rb in bodies)
                    {
                        if (rb != null && rb.name == "head")
                            return rb.position;
                    }
                }
            }

            if (head != null && head.followWhenOff != null)
                return head.followWhenOff.position;
            if (head != null && head.possessPoint != null)
                return head.possessPoint.position;
            return head != null ? head.control.position : Vector3.zero;
        }

        private static bool TryPrepareHeadForPossessAndAlign(SuperController sc, FreeControllerV3 head, out string error)
        {
            error = null;
            if (sc == null || head == null)
            {
                error = "missing SuperController or headControl";
                return false;
            }

            Transform motionControllerHead = sc.centerCameraTarget != null ? sc.centerCameraTarget.transform : null;
            Possessor possessor = motionControllerHead != null ? motionControllerHead.GetComponent<Possessor>() : null;
            Transform navigationRig = sc.navigationRig;
            if (motionControllerHead == null || possessor == null || possessor.autoSnapPoint == null || navigationRig == null)
            {
                error = "missing centerCameraTarget, Possessor, autoSnapPoint, or navigationRig";
                return false;
            }

            try
            {
                Vector3 forwardPossessAxis = head.GetForwardPossessAxis();
                Vector3 upPossessAxis = head.GetUpPossessAxis();
                Vector3 up = navigationRig.up;
                Vector3 fromDirection = Vector3.ProjectOnPlane(motionControllerHead.forward, up);
                Vector3 desiredForward = Vector3.ProjectOnPlane(forwardPossessAxis, navigationRig.up);
                if (Vector3.Dot(upPossessAxis, up) < 0f && Vector3.Dot(motionControllerHead.up, up) > 0f)
                    desiredForward = -desiredForward;

                if (fromDirection.sqrMagnitude > 1e-8f && desiredForward.sqrMagnitude > 1e-8f)
                {
                    Quaternion q = Quaternion.FromToRotation(fromDirection, desiredForward);
                    navigationRig.rotation = q * navigationRig.rotation;
                }

                // Match Passenger's default "head" target when available so
                // the HMD lands where Passenger would place it.
                Vector3 headTarget = GetPassengerHeadTargetWorldPosition(head);
                Vector3 delta = headTarget - possessor.autoSnapPoint.position;
                Vector3 targetRigPos = navigationRig.position + delta;
                float verticalDelta = Vector3.Dot(targetRigPos - navigationRig.position, up);
                targetRigPos += up * (0f - verticalDelta);
                navigationRig.position = targetRigPos;
                sc.playerHeightAdjust += verticalDelta;

                if (sc.MonitorCenterCamera != null)
                {
                    sc.MonitorCenterCamera.transform.LookAt(head.transform.position + forwardPossessAxis);
                    Vector3 euler = sc.MonitorCenterCamera.transform.localEulerAngles;
                    euler.y = 0f;
                    euler.z = 0f;
                    sc.MonitorCenterCamera.transform.localEulerAngles = euler;
                }

                return TryLinkHeadToMotionControllerHead(motionControllerHead, head, out error);
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private static bool TryLinkHeadToMotionControllerHead(
            Transform motionControllerHead,
            FreeControllerV3 head,
            out string error)
        {
            error = null;
            if (motionControllerHead == null || head == null)
            {
                error = "missing motionControllerHead or headControl";
                return false;
            }

            Rigidbody headRb = motionControllerHead.GetComponent<Rigidbody>();
            if (headRb == null)
            {
                error = "missing motionControllerHead rigidbody";
                return false;
            }

            try
            {
                MotionAnimationControl mac = head.GetComponent<MotionAnimationControl>();
                if (head.canGrabPosition && mac != null)
                    mac.suspendPositionPlayback = true;
                if (head.canGrabRotation && mac != null)
                    mac.suspendRotationPlayback = true;

                head.possessed = true;

                FreeControllerV3.SelectLinkState linkState =
                    FreeControllerV3.SelectLinkState.Position;
                if (head.canGrabPosition)
                {
                    if (head.canGrabRotation)
                        linkState =
                            FreeControllerV3.SelectLinkState.PositionAndRotation;
                }
                else if (head.canGrabRotation)
                {
                    linkState = FreeControllerV3.SelectLinkState.Rotation;
                }

                head.SelectLinkToRigidbody(headRb, linkState);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private static bool TryPrepareHandForPossess(SuperController sc, FreeControllerV3 controller, bool left, out string error)
        {
            error = null;
            if (controller == null)
                return true;

            Transform motionController = GetMotionControllerTransform(sc, left);
            if (sc == null || motionController == null)
            {
                error = "missing SuperController or player hand transform";
                return false;
            }

            try
            {
                controller.PossessMoveAndAlignTo(motionController);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private static bool TryDriveControllerIntoPossessOverlap(SuperController sc, FreeControllerV3 controller, bool left, out string error)
        {
            error = null;
            if (controller == null)
                return true;

            if (controller.possessed)
                return true;

            return TryPrepareHandForPossess(sc, controller, left, out error);
        }

        private static void StopAutoPossessRoutine()
        {
            if (_pluginHost != null && _autoPossessConfirmCo != null)
            {
                _pluginHost.StopCoroutine(_autoPossessConfirmCo);
                _autoPossessConfirmCo = null;
            }

            if (_pluginHost != null && _autoPossessCoroutine != null)
            {
                _pluginHost.StopCoroutine(_autoPossessCoroutine);
                _autoPossessCoroutine = null;
            }
        }

        private static void StartAutoPossessConfirmRoutine(
            FreeControllerV3 head,
            FreeControllerV3 leftHand,
            FreeControllerV3 rightHand)
        {
            if (_pluginHost == null)
                return;

            if (_autoPossessConfirmCo != null)
            {
                _pluginHost.StopCoroutine(_autoPossessConfirmCo);
                _autoPossessConfirmCo = null;
            }

            _autoPossessConfirmCo = _pluginHost.StartCoroutine(
                AutoPossessConfirmAfterDelayCo(head, leftHand, rightHand));
        }

        private static IEnumerator AutoPossessConfirmAfterDelayCo(
            FreeControllerV3 head,
            FreeControllerV3 leftHand,
            FreeControllerV3 rightHand)
        {
            try
            {
                yield return new WaitForSecondsRealtime(1f);

                SuperController sc = SuperController.singleton;
                if (sc == null)
                    yield break;

                bool anyPossessed =
                    (head != null && head.possessed) ||
                    (leftHand != null && leftHand.possessed) ||
                    (rightHand != null && rightHand.possessed);
                if (anyPossessed)
                    sc.SelectModeOff();
            }
            finally
            {
                _autoPossessConfirmCo = null;
            }
        }

        /// <summary>Same possess/align/select slot sequence as the HUD, for whichever Person is closest by head to the look/center camera.</summary>
        private static void PossessAlignSelectClosestPersonByHeadToCamera()
        {
            Atom target;
            bool isFemale;
            if (!TryFindClosestPersonByHeadToCamera(out target, out isFemale))
            {
                SuperController.LogMessage("Easy Mate: P — no Person in scene.");
                return;
            }

            StartAutoPossessRoutine(target, "P");
        }

        /// <summary>
        /// Closest female <c>Person</c> by head to look/center camera
        /// (VR dual-hand euler gesture); same routine as <b>P</b> but female-only.
        /// </summary>
        private static void PossessAlignSelectClosestFemaleByHeadToCamera()
        {
            EnsurePersonGenderCaches();
            Atom target = FindClosestPersonInListByHeadToCamera(
                _cachedFemalePersonsByUid);
            if (target == null)
            {
                SuperController.LogMessage(
                    "Easy Mate: VR hand euler — no female Person in scene.");
                return;
            }

            StartAutoPossessRoutine(target, VrEulerPossessLabel);
        }

        private static void PossessAlignSelectFirstFemale()
        {
            List<Atom> list = FemalePersonsByUid();
            if (list.Count == 0)
            {
                SuperController.LogMessage("Easy Mate HUD: Possess Female — no female Person in scene.");
                return;
            }

            StartAutoPossessRoutine(list[0], "F");
        }

        private static void PossessAlignSelectMaleIfAny()
        {
            List<Atom> list = MalePersonsByUid();
            if (list.Count == 0)
            {
                SuperController.LogMessage("Easy Mate HUD: Possess Male — no male Person in scene.");
                return;
            }

            StartAutoPossessRoutine(list[0], "M");
        }

        private static Vector3 GetPersonHeadWorldPosition(Atom person)
        {
            if (person == null)
                return Vector3.zero;
            FreeControllerV3 head = person.GetStorableByID("headControl") as FreeControllerV3;
            if (head != null && head.followWhenOff != null)
                return head.followWhenOff.position;
            return person.transform.position;
        }

        /// <summary>Same position source as head snap alignment (<see cref="SuperController.lookCamera"/> then center camera target).</summary>
        private static Vector3 GetLookOrCenterCameraWorldPosition()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return Vector3.zero;
            if (sc.lookCamera != null)
                return sc.lookCamera.transform.position;
            if (sc.centerCameraTarget != null)
                return sc.centerCameraTarget.transform.position;
            return Vector3.zero;
        }

        /// <summary>Closest <c>Person</c> by <see cref="GetPersonHeadWorldPosition"/> to the look/center camera (any gender).</summary>
        private static bool TryFindClosestPersonByHeadToCamera(out Atom closest, out bool isFemale)
        {
            closest = null;
            isFemale = false;
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return false;

            Vector3 cam = GetLookOrCenterCameraWorldPosition();
            float bestSq = float.MaxValue;
            foreach (Atom at in sc.GetAtoms())
            {
                if (at == null || at.type != "Person")
                    continue;
                float dSq = (GetPersonHeadWorldPosition(at) - cam).sqrMagnitude;
                if (dSq < bestSq)
                {
                    bestSq = dSq;
                    closest = at;
                    isFemale = IsPersonFemale(at);
                }
            }

            return closest != null;
        }

        private static Atom FindClosestPersonInListByHeadToCamera(IEnumerable<Atom> persons)
        {
            if (persons == null)
                return null;
            Vector3 cam = GetLookOrCenterCameraWorldPosition();
            Atom best = null;
            float bestSq = float.MaxValue;
            foreach (Atom at in persons)
            {
                if (at == null)
                    continue;
                float dSq = (GetPersonHeadWorldPosition(at) - cam).sqrMagnitude;
                if (dSq < bestSq)
                {
                    bestSq = dSq;
                    best = at;
                }
            }

            return best;
        }

        /// <summary>After Possess+Align+Select, merge Spankings onto every other Person that does not already have it (runs once per trigger).</summary>
        private static void MergeSpankingsOntoOtherPersonsMissingPluginAfterPossess(Atom possessedPerson)
        {
            if (possessedPerson == null || SuperController.singleton == null)
                return;

            string fn = GetFileName(PluginSpankings);
            foreach (Atom at in SuperController.singleton.GetAtoms().Where(a => a.type == "Person"))
            {
                if (at == null || at.uid == possessedPerson.uid)
                    continue;
                if (PersonHasPluginByFileName(at, fn))
                    continue;
                TryMergePluginOntoPerson(at, PluginSpankings);
            }
        }

        private static void StartAutoPossessRoutine(Atom person, string label)
        {
            if (_pluginHost == null)
            {
                SuperController.LogError("Easy Mate HUD: Possess+Align+Select " + label + " — plugin host missing.");
                return;
            }

            if (string.Equals(label, VrEulerPossessLabel, StringComparison.Ordinal))
                EasyMateGripHandVisibility.NotifyVrEulerPossessTenSecondSuppress();

            StopAutoPossessRoutine();
            _autoPossessCoroutine = _pluginHost.StartCoroutine(PossessAlignSelectRoutine(person, label));
        }

        private static IEnumerator PossessAlignSelectRoutine(Atom person, string label)
        {
            try
            {
                bool isVrEulerPossess = string.Equals(
                    label,
                    VrEulerPossessLabel,
                    StringComparison.Ordinal);
                EasyMateHeadSnapPovRuntime.EndSnapSession();

                SuperController sc = SuperController.singleton;
                if (sc == null || person == null || person.type != "Person")
                    yield break;

                FreeControllerV3 head = person.GetStorableByID("headControl") as FreeControllerV3;
                FreeControllerV3 leftHand = person.GetStorableByID("lHandControl") as FreeControllerV3;
                FreeControllerV3 rightHand = person.GetStorableByID("rHandControl") as FreeControllerV3;
                if (head == null)
                {
                    SuperController.LogError("Easy Mate HUD: Possess+Align+Select " + label + " — no headControl on " + person.name);
                    yield break;
                }

                if (isVrEulerPossess)
                    RemoveSpankingsFromAllPersonsStatic();

                sc.ClearPossess();
                UnlinkStrayHmdLinkedFreeControllersAndNaturalizeHeads(sc);
                yield return null;

                string headError;
                if (!TryPrepareHeadForPossessAndAlign(sc, head, out headError))
                {
                    SuperController.LogError("Easy Mate HUD: Possess+Align+Select " + label + " head failed: " + headError);
                    yield break;
                }

                sc.SelectController(head, false);
                sc.SelectModePossess(true);
                StartAutoPossessConfirmRoutine(head, leftHand, rightHand);

                yield return null;
                yield return null;

                bool headDone = head.possessed;
                bool leftDone = leftHand == null || leftHand.possessed;
                bool rightDone = rightHand == null || rightHand.possessed;
                string headPrepError = null;
                string leftError = null;
                string rightError = null;

                for (int i = 0; i < 120 && (!headDone || !leftDone || !rightDone); i++)
                {
                    if (!headDone)
                    {
                        TryPrepareHeadForPossessAndAlign(sc, head, out headPrepError);
                        headDone = head.possessed;
                    }
                    if (!leftDone)
                    {
                        TryDriveControllerIntoPossessOverlap(sc, leftHand, true, out leftError);
                        leftDone = leftHand != null && leftHand.possessed;
                    }
                    if (!rightDone)
                    {
                        TryDriveControllerIntoPossessOverlap(sc, rightHand, false, out rightError);
                        rightDone = rightHand != null && rightHand.possessed;
                    }

                    if (!headDone || !leftDone || !rightDone)
                        yield return null;
                }

                if (!headDone || !leftDone || !rightDone)
                    sc.SelectModeOff();

                sc.SelectController(head, false);

                string headState = headDone ? "ok" : "failed";
                string leftState = leftDone ? "ok" : "failed";
                string rightState = rightDone ? "ok" : "failed";
                SuperController.LogMessage("Easy Mate HUD: Possess+Align+Select " + label + " — " + person.name + " (head " + headState + ", left " + leftState + ", right " + rightState + ").");
                if (!headDone && headPrepError != null)
                    SuperController.LogMessage("Easy Mate HUD: head possess prep detail: " + headPrepError);
                if (!leftDone && leftError != null)
                    SuperController.LogMessage("Easy Mate HUD: left hand auto-possess detail: " + leftError);
                if (!rightDone && rightError != null)
                    SuperController.LogMessage("Easy Mate HUD: right hand auto-possess detail: " + rightError);

                bool anyHandPossessed =
                    (leftHand != null && leftHand.possessed) ||
                    (rightHand != null && rightHand.possessed);
                if (anyHandPossessed && !isVrEulerPossess)
                {
                    try
                    {
                        MergeSpankingsOntoOtherPersonsMissingPluginAfterPossess(person);
                        if (_refreshPluginToggleLabelsStatic != null)
                            _refreshPluginToggleLabelsStatic();
                    }
                    catch (Exception ex)
                    {
                        SuperController.LogError("Easy Mate: Possess+Align+Select — Spankings on other Persons: " + ex.Message);
                    }
                }
                else if (anyHandPossessed && isVrEulerPossess &&
                         _refreshPluginToggleLabelsStatic != null)
                    _refreshPluginToggleLabelsStatic();
            }
            finally
            {
                _autoPossessCoroutine = null;
            }
        }

        private static void SnapRigToClosestFemaleHead()
        {
            EnsurePersonGenderCaches();
            List<Atom> list = FemalePersonsByUid();
            if (list.Count == 0)
            {
                SuperController.LogMessage("Easy Mate HUD: Passenger Female — no female Person in scene.");
                return;
            }

            Atom female = FindClosestPersonInListByHeadToCamera(list);
            if (female == null)
                return;
            OneShotSnapRigToPersonHead(female, null, false);
        }

        private static void SnapRigToClosestMaleHead()
        {
            EnsurePersonGenderCaches();
            List<Atom> list = MalePersonsByUid();
            if (list.Count == 0)
            {
                SuperController.LogMessage("Easy Mate HUD: Passenger Male — no male Person in scene.");
                return;
            }

            Atom male = FindClosestPersonInListByHeadToCamera(list);
            if (male == null)
                return;
            SnapRigToMalePersonHeadWithPostSteps(male);
        }

        private static void SnapRigToMalePersonHeadWithPostSteps(Atom male)
        {
            if (male == null)
                return;
            Vector3 maleHead = GetPersonHeadWorldPosition(male);
            Atom closestFemale = FindClosestFemalePersonFromPoint(maleHead, male);
            OneShotSnapRigToPersonHead(male, closestFemale, snapTargetAtHeadControl: true);
            EnsureSnapMEndsWithoutPossessionOrTargetHud();
            EasyMateHeadSnapPovRuntime.HidePossessorAlignmentPreviewMeshes();
            EasyMateGripHandVisibility.DisableVrHandModelsForSceneStart();
        }

        /// <summary>
        /// Snap M only moves the navigation rig; VaM may still be in possess/target UI (green alignment spheres).
        /// Clear those so this is not mistaken for normal possession.
        /// </summary>
        private static void EnsureSnapMEndsWithoutPossessionOrTargetHud()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;
            try
            {
                sc.ClearPossess();
                UnlinkStrayHmdLinkedFreeControllersAndNaturalizeHeads(sc);
                sc.SelectModeOff();
            }
            catch (Exception e)
            {
                SuperController.LogError("Easy Mate Snap M: ClearPossess/SelectModeOff: " + e.Message);
            }
        }

        /// <summary>Closest female Person by head world distance from <paramref name="fromWorld"/>, excluding <paramref name="exclude"/> (e.g. the snap subject).</summary>
        private static Atom FindClosestFemalePersonFromPoint(Vector3 fromWorld, Atom exclude)
        {
            List<Atom> females = FemalePersonsByUid();
            Atom best = null;
            float bestSq = float.MaxValue;
            string excludeUid = exclude != null ? exclude.uid : null;
            foreach (Atom f in females)
            {
                if (f == null || (excludeUid != null && f.uid == excludeUid))
                    continue;
                float dSq = (GetPersonHeadWorldPosition(f) - fromWorld).sqrMagnitude;
                if (dSq < bestSq)
                {
                    bestSq = dSq;
                    best = f;
                }
            }

            return best;
        }

        /// <summary>One-time navigationRig snap to a Person head (no possession). Snap F uses offset target (<see cref="GetHeadSnapTargetWorld"/>). Snap M passes <paramref name="yawTowardPerson"/> and <paramref name="snapTargetAtHeadControl"/> so the rig aligns to head control like possession, with yaw toward that Person’s head when set.</summary>
        private static void OneShotSnapRigToPersonHead(Atom person, Atom yawTowardPerson = null, bool snapTargetAtHeadControl = false)
        {
            try
            {
                EasyMateHeadSnapPovRuntime.EndSnapSession();

                SuperController sc = SuperController.singleton;
                FreeControllerV3 head = person.GetStorableByID("headControl") as FreeControllerV3;
                if (head == null)
                {
                    SuperController.LogError("Easy Mate HUD: Snap — no headControl on " + person.name);
                    return;
                }

                if (head.possessed)
                {
                    SuperController.LogMessage("Easy Mate HUD: Snap skipped — head is possessed (" + person.name + ").");
                    return;
                }

                Possessor possessor = sc.centerCameraTarget != null
                    ? sc.centerCameraTarget.transform.GetComponent<Possessor>()
                    : null;
                if (possessor == null || possessor.autoSnapPoint == null)
                {
                    SuperController.LogError("Easy Mate HUD: Snap — Possessor or autoSnapPoint missing.");
                    return;
                }

                Transform navigationRig = sc.navigationRig;
                Vector3 up = navigationRig.up;

                Transform camT = sc.lookCamera != null
                    ? sc.lookCamera.transform
                    : (sc.centerCameraTarget != null ? sc.centerCameraTarget.transform : null);
                if (camT == null)
                {
                    SuperController.LogError("Easy Mate HUD: Snap — no camera transform for alignment.");
                    return;
                }

                Vector3 snapTargetWorld = snapTargetAtHeadControl
                    ? GetPossessionMatchHeadSnapWorldPosition(head)
                    : GetHeadSnapTargetWorld(head);

                Vector3 forwardPossessAxis = head.GetForwardPossessAxis();
                Vector3 upPossessAxis = head.GetUpPossessAxis();
                Vector3 fromDirection = Vector3.ProjectOnPlane(camT.forward, up);
                Vector3 vector;
                bool useTowardFemaleYaw = yawTowardPerson != null && yawTowardPerson != person;
                if (useTowardFemaleYaw)
                {
                    Vector3 toTarget = GetPersonHeadWorldPosition(yawTowardPerson) - snapTargetWorld;
                    vector = Vector3.ProjectOnPlane(toTarget, navigationRig.up);
                    if (vector.sqrMagnitude < 1e-8f)
                        vector = Vector3.ProjectOnPlane(forwardPossessAxis, navigationRig.up);
                }
                else
                {
                    vector = Vector3.ProjectOnPlane(forwardPossessAxis, navigationRig.up);
                    if (Vector3.Dot(upPossessAxis, up) < 0f && Vector3.Dot(camT.up, up) > 0f)
                        vector = -vector;
                }

                if (fromDirection.sqrMagnitude > 1e-8f && vector.sqrMagnitude > 1e-8f)
                {
                    fromDirection.Normalize();
                    vector.Normalize();
                    float signedAngle = Vector3.SignedAngle(fromDirection, vector, up);
                    if (Mathf.Abs(signedAngle) > 0.01f)
                    {
                        Quaternion q = Quaternion.AngleAxis(signedAngle, up);
                        navigationRig.rotation = q * navigationRig.rotation;
                    }
                }

                Vector3 vector2 = snapTargetWorld;
                Vector3 vector3 = vector2 - possessor.autoSnapPoint.position;
                Vector3 vector4 = navigationRig.position + vector3;
                float num = Vector3.Dot(vector4 - navigationRig.position, up);
                vector4 += up * (0f - num);
                navigationRig.position = vector4;
                sc.playerHeightAdjust += num;

                if (sc.MonitorCenterCamera != null)
                {
                    Vector3 lookAtWorld = useTowardFemaleYaw
                        ? GetPersonHeadWorldPosition(yawTowardPerson)
                        : head.transform.position + forwardPossessAxis;
                    sc.MonitorCenterCamera.transform.LookAt(lookAtWorld);
                    Vector3 localEulerAngles = sc.MonitorCenterCamera.transform.localEulerAngles;
                    localEulerAngles.y = 0f;
                    localEulerAngles.z = 0f;
                    sc.MonitorCenterCamera.transform.localEulerAngles = localEulerAngles;
                }

                EasyMateHeadSnapPovRuntime.Begin(person, _pluginHost);
            }
            catch (Exception e)
            {
                EasyMateHeadSnapPovRuntime.EndSnapSession();
                SuperController.LogError("Easy Mate HUD: Snap to head failed: " + e);
            }
        }

        private static JSONClass CreatePluginJSON(string[] pluginList)
        {
            var sb = new System.Text.StringBuilder();
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

        private static string GetFileName(string relativePath)
        {
            return relativePath.Substring(relativePath.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
        }

        private static bool FileExists(string relativePath)
        {
            int folderSeparatorIndex = relativePath.LastIndexOfAny(new char[] { '/', '\\' });
            if (folderSeparatorIndex < 0)
                return false;

            string pathFolder = relativePath.Substring(0, folderSeparatorIndex);
            string pathFile = relativePath.Substring(folderSeparatorIndex + 1);
            string[] pathFileList = SuperController.singleton.GetFilesAtPath(pathFolder);
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
