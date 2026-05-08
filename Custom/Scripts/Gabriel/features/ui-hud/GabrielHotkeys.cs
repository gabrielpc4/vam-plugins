using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace geesp0t
{
    /// <summary>
    /// Desktop keyboard shortcuts for the Gabriel HUD plugin; delegates menu actions
    /// to <see cref="GabrielHudButtons"/> and cross-feature helpers.
    /// </summary>
    internal sealed class GabrielHotkeys
    {
        private const string CoreControlAtomUid = "CoreControl";
        private const string GabrielSessionPluginsSuffix = ".GabrielSessionPlugins";
        private const string ForceReleaseSceneSettleHoldActionName =
            "ForceReleaseSceneSettleHold";

        private readonly GabrielHudButtons owner;

        internal GabrielHotkeys(GabrielHudButtons owner)
        {
            this.owner = owner;
        }

        internal void ProcessUpdate()
        {
            bool noTextFocus;
            bool noCtrlAlt;

            if (owner == null || SuperController.singleton == null)
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
                    owner.ToggleSpankingsPluginOnAllPersons();
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
                    GabrielHudButtons.ToggleFreezeAnimationHotkey();
                }
                catch (Exception e)
                {
                    SuperController.LogError(
                        "F hotkey (freeze animation toggle): " + e);
                }
            }
        }

        private static void TryForceReleaseSceneSettleHoldFromHotkey()
        {
            Atom coreControl;
            JSONStorable sessionPluginsStorable;
            JSONStorableAction forceReleaseAction;

            coreControl =
                SuperController.singleton.GetAtomByUid(CoreControlAtomUid);
            if (coreControl == null)
            {
                return;
            }

            sessionPluginsStorable =
                FindSessionPluginStorableBySuffix(
                    coreControl,
                    GabrielSessionPluginsSuffix);
            if (sessionPluginsStorable == null)
            {
                SuperController.LogError(
                    "Space hotkey: GabrielSessionPlugins plugin not found on " +
                    "CoreControl.");
                return;
            }

            forceReleaseAction =
                sessionPluginsStorable.GetAction(
                    ForceReleaseSceneSettleHoldActionName);
            if (forceReleaseAction == null ||
                forceReleaseAction.actionCallback == null)
            {
                SuperController.LogError(
                    "Space hotkey: GabrielSessionPlugins action <" +
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
