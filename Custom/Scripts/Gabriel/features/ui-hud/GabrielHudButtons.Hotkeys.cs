using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace geesp0t
{
    public partial class GabrielHudButtons
    {
        private const string CoreControlAtomUid = "CoreControl";
        private const string GabrielSessionStackSuffix = ".GabrielSessionStack";
        private const string ForceReleaseSceneSettleHoldActionName =
            "ForceReleaseSceneSettleHold";

        /// <summary>
        /// Owns the first-party keyboard polling surface.
        /// <b>Space</b> proxies to the session-stack settle release action even
        /// while loading; the remaining HUD hotkeys keep their existing
        /// non-loading guards and behavior.
        /// </summary>
        public void ProcessHotkeysUpdate()
        {
            bool noTextFocus;
            bool noCtrlAlt;

            if (plugin == null || SuperController.singleton == null)
            {
                return;
            }

            noTextFocus =
                EventSystem.current == null ||
                EventSystem.current.currentSelectedGameObject == null;
            noCtrlAlt =
                !Input.GetKey(KeyCode.LeftControl) &&
                !Input.GetKey(KeyCode.RightControl) &&
                !Input.GetKey(KeyCode.LeftAlt) &&
                !Input.GetKey(KeyCode.RightAlt);

            if (noTextFocus && noCtrlAlt && Input.GetKeyDown(KeyCode.Space))
            {
                TryForceReleaseSceneSettleHoldFromHotkey();
                return;
            }

            if (SuperController.singleton.isLoading || !noTextFocus)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.S) &&
                (Input.GetKey(KeyCode.LeftShift) ||
                    Input.GetKey(KeyCode.RightShift)) &&
                (Input.GetKey(KeyCode.LeftControl) ||
                    Input.GetKey(KeyCode.RightControl)))
            {
                try
                {
                    ToggleSpankingsPluginOnAllPersons();
                }
                catch (Exception e)
                {
                    SuperController.LogError(
                        "Ctrl+Shift+S hotkey (Spankings toggle): " + e);
                }

                return;
            }

            if (!noCtrlAlt)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                try
                {
                    SceneCameraPatch.TryRunFromHotkey();
                }
                catch (Exception e)
                {
                    SuperController.LogError(
                        "K hotkey (patch scene JSON camera / rig): " + e);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                try
                {
                    PassengerRuntime.RequestStopForPalmHud();
                    SuperController.LogMessage(
                        "Easy Mate: O — stopped Passenger mode.");
                }
                catch (Exception e)
                {
                    SuperController.LogError(
                        "O hotkey (stop Passenger): " + e);
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
                    SuperController.LogError(
                        "F hotkey (freeze animation toggle): " + e);
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
                    RequestPossessVrPalmHudByGender(true);
                }
                catch (Exception e)
                {
                    SuperController.LogError(
                        "P hotkey (start female Passenger): " + e);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.C) &&
                !Input.GetKey(KeyCode.LeftShift) &&
                !Input.GetKey(KeyCode.RightShift))
            {
                try
                {
                    CycleFemaleThenMalePersonRootEditMenuOnHotkey();
                }
                catch (Exception e)
                {
                    SuperController.LogError(
                        "C hotkey (cycle Person edit root): " + e);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                SuperController scY = SuperController.singleton;
                bool yNeedsShiftForControllerConflict =
                    scY != null && (scY.isOVR || scY.isOpenVR);
                if (yNeedsShiftForControllerConflict &&
                    !Input.GetKey(KeyCode.LeftShift) &&
                    !Input.GetKey(KeyCode.RightShift))
                {
                    return;
                }

                float now = Time.unscaledTime;
                if (now - _lastYDebugLogUnscaledTime <
                    YDebugLogMinIntervalSeconds)
                {
                    return;
                }

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
                VrGestureRuntime.ProcessUpdate(_vrGestureBindings);
            }
            catch (Exception e)
            {
                SuperController.LogError("Easy Mate VR gestures: " + e);
            }
        }

        private static void TryForceReleaseSceneSettleHoldFromHotkey()
        {
            Atom coreControl;
            JSONStorable sessionStackStorable;
            JSONStorableAction forceReleaseAction;

            coreControl =
                SuperController.singleton.GetAtomByUid(CoreControlAtomUid);
            if (coreControl == null)
            {
                return;
            }

            sessionStackStorable =
                FindSessionPluginStorableBySuffix(
                    coreControl,
                    GabrielSessionStackSuffix);
            if (sessionStackStorable == null)
            {
                SuperController.LogError(
                    "Space hotkey: GabrielSessionStack plugin not found on " +
                    "CoreControl.");
                return;
            }

            forceReleaseAction =
                sessionStackStorable.GetAction(
                    ForceReleaseSceneSettleHoldActionName);
            if (forceReleaseAction == null ||
                forceReleaseAction.actionCallback == null)
            {
                SuperController.LogError(
                    "Space hotkey: GabrielSessionStack action <" +
                    ForceReleaseSceneSettleHoldActionName +
                    "> is unavailable.");
                return;
            }

            forceReleaseAction.actionCallback();
        }

        private static JSONStorable FindSessionPluginStorableBySuffix(
            Atom atom,
            string storableIdSuffix)
        {
            List<string> storableIds;
            int i;
            string storableId;

            if (atom == null || string.IsNullOrEmpty(storableIdSuffix))
            {
                return null;
            }

            storableIds = atom.GetStorableIDs();
            if (storableIds == null)
            {
                return null;
            }

            for (i = 0; i < storableIds.Count; i++)
            {
                storableId = storableIds[i];
                if (string.IsNullOrEmpty(storableId) ||
                    !storableId.EndsWith(
                        storableIdSuffix,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                return atom.GetStorableByID(storableId);
            }

            return null;
        }
    }
}
