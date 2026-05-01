using MeshVR;
using UnityEngine;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// Per-hand VR grip press toggles <see cref="HandModelControl.leftHandEnabled"/> /
    /// <see cref="HandModelControl.rightHandEnabled"/> on both <see cref="SuperController.commonHandModelControl"/>
    /// and <see cref="SuperController.alternateControllerHandModelControl"/>.
    /// Turning a hand <b>on</b> is skipped while that side’s Person hand control is <see cref="FreeControllerV3.possessed"/>
    /// so VR hand models do not appear over an active hand possession.
    /// When turning a hand <b>on</b>, sets <see cref="HandModelControl.useCollision"/> to true if it was false,
    /// and selects the <b>Male2</b> hand model (or <b>Male 2</b>) when that option exists and is not already active.
    /// <see cref="DisableVrHandModelsForSceneStart"/> turns both hands off on each scene load (EasyMate).
    /// </summary>
    internal static class EasyMateGripHandVisibility
    {
        private static readonly string[] PreferredHandIds = { "Male2", "Male 2" };

        /// <summary>Turn off VR hand models on common + alternate <see cref="HandModelControl"/> (scene / plugin start).</summary>
        public static void DisableVrHandModelsForSceneStart()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;
            SetBothHandsEnabled(sc.commonHandModelControl, false);
            SetBothHandsEnabled(sc.alternateControllerHandModelControl, false);
        }

        private static void SetBothHandsEnabled(HandModelControl h, bool enabled)
        {
            if (h == null)
                return;
            h.leftHandEnabled = enabled;
            h.rightHandEnabled = enabled;
        }

        public static void LateTick(bool featureEnabled)
        {
            if (!featureEnabled)
                return;

            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;
            if (!sc.isOVR && !sc.isOpenVR && !XRSettings.enabled)
                return;

            bool leftDown = EasyMateVrInput.PollLeftGripClickDown(sc);
            bool rightDown = EasyMateVrInput.PollRightGripClickDown(sc);

            if (leftDown)
                ToggleLeft(sc);
            if (rightDown)
                ToggleRight(sc);
        }

        private static bool CurrentLeftEnabled(SuperController sc)
        {
            if (sc.commonHandModelControl != null)
                return sc.commonHandModelControl.leftHandEnabled;
            if (sc.alternateControllerHandModelControl != null)
                return sc.alternateControllerHandModelControl.leftHandEnabled;
            return false;
        }

        private static bool CurrentRightEnabled(SuperController sc)
        {
            if (sc.commonHandModelControl != null)
                return sc.commonHandModelControl.rightHandEnabled;
            if (sc.alternateControllerHandModelControl != null)
                return sc.alternateControllerHandModelControl.rightHandEnabled;
            return false;
        }

        private static void ToggleLeft(SuperController sc)
        {
            bool next = !CurrentLeftEnabled(sc);
            if (next && AnyPersonLeftHandPossessed())
                return;
            ApplyLeft(sc, next);
        }

        private static void ToggleRight(SuperController sc)
        {
            bool next = !CurrentRightEnabled(sc);
            if (next && AnyPersonRightHandPossessed())
                return;
            ApplyRight(sc, next);
        }

        private static bool AnyPersonLeftHandPossessed()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || a.type != "Person" || !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3 l = a.GetStorableByID("lHandControl") as FreeControllerV3;
                    if (l != null && l.possessed)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static bool AnyPersonRightHandPossessed()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || a.type != "Person" || !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3 r = a.GetStorableByID("rHandControl") as FreeControllerV3;
                    if (r != null && r.possessed)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static void ApplyLeft(SuperController sc, bool enabled)
        {
            HandModelControl common = sc.commonHandModelControl;
            HandModelControl alt = sc.alternateControllerHandModelControl;
            if (common != null)
                common.leftHandEnabled = enabled;
            if (alt != null)
                alt.leftHandEnabled = enabled;

            if (enabled)
            {
                EnableCollisionIfOff(common, alt);
                EnsurePreferredHandModels(common, left: true, right: false);
                EnsurePreferredHandModels(alt, left: true, right: false);
            }
        }

        private static void ApplyRight(SuperController sc, bool enabled)
        {
            HandModelControl common = sc.commonHandModelControl;
            HandModelControl alt = sc.alternateControllerHandModelControl;
            if (common != null)
                common.rightHandEnabled = enabled;
            if (alt != null)
                alt.rightHandEnabled = enabled;

            if (enabled)
            {
                EnableCollisionIfOff(common, alt);
                EnsurePreferredHandModels(common, left: false, right: true);
                EnsurePreferredHandModels(alt, left: false, right: true);
            }
        }

        private static void EnableCollisionIfOff(HandModelControl common, HandModelControl alt)
        {
            if (common != null && !common.useCollision)
                common.useCollision = true;
            if (alt != null && !alt.useCollision)
                alt.useCollision = true;
        }

        private static bool HandsArrayHasNamedModel(HandModelControl.Hand[] hands, string modelName)
        {
            if (hands == null || string.IsNullOrEmpty(modelName))
                return false;
            for (int i = 0; i < hands.Length; i++)
            {
                HandModelControl.Hand h = hands[i];
                if (h != null && h.name == modelName)
                    return true;
            }

            return false;
        }

        private static string ResolvePreferredModelIdForSide(HandModelControl.Hand[] hands)
        {
            if (hands == null)
                return null;
            for (int p = 0; p < PreferredHandIds.Length; p++)
            {
                string id = PreferredHandIds[p];
                if (HandsArrayHasNamedModel(hands, id))
                    return id;
            }

            return null;
        }

        private static void EnsurePreferredHandModels(HandModelControl h, bool left, bool right)
        {
            if (h == null)
                return;

            if (left)
            {
                string id = ResolvePreferredModelIdForSide(h.leftHands);
                if (!string.IsNullOrEmpty(id) && !string.Equals(h.leftHandChoice, id, StringComparison.Ordinal))
                    h.leftHandChoice = id;
            }

            if (right)
            {
                string id = ResolvePreferredModelIdForSide(h.rightHands);
                if (!string.IsNullOrEmpty(id) && !string.Equals(h.rightHandChoice, id, StringComparison.Ordinal))
                    h.rightHandChoice = id;
            }
        }
    }
}
