using System;
using MeshVR;
using UnityEngine;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// Per-controller VR grip (Quest squeeze / OpenVR HoldGrab): toggles each side between articulated hands
    /// (<b>Male2</b> / <b>Male 2</b> when present) with collision, and VaM’s compact <see cref="SphereKinematicChoice"/>
    /// hand (same string as User Preferences → VR Hands → Left/Right Hand Choice). Sphere mode keeps the hand
    /// enabled so the proxy stays visible; <see cref="HandModelControl.useCollision"/> is off when neither side
    /// is articulated (matches “hidden” = no overlap grabs). See decompiled <c>MeshVR.HandModelControl</c>.
    /// Articulated-on for a side is skipped while that Person hand control is possessed.
    /// </summary>
    internal static class EasyMateGripHandVisibility
    {
        /// <summary>Second entry in VaM’s VR hand choice list (after None): sphere / kinematic proxy.</summary>
        private const string SphereKinematicChoice = "SphereKinematic";

        private static readonly string[] PreferredHandIds = { "Male2", "Male 2" };

        private static bool _leftArticulated;

        private static bool _rightArticulated;

        /// <summary>Scene load: both sides sphere (or legacy-off if slot missing), no collisions.</summary>
        public static void DisableVrHandModelsForSceneStart()
        {
            _leftArticulated = false;
            _rightArticulated = false;
            ApplyBothControls(SuperController.singleton);
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

        private static void ToggleLeft(SuperController sc)
        {
            bool nextArticulated = !_leftArticulated;
            if (nextArticulated && AnyPersonLeftHandPossessed())
                return;
            _leftArticulated = nextArticulated;
            ApplyBothControls(sc);
        }

        private static void ToggleRight(SuperController sc)
        {
            bool nextArticulated = !_rightArticulated;
            if (nextArticulated && AnyPersonRightHandPossessed())
                return;
            _rightArticulated = nextArticulated;
            ApplyBothControls(sc);
        }

        private static void ApplyBothControls(SuperController sc)
        {
            if (sc == null)
                return;
            ApplySingleControl(sc.commonHandModelControl);
            ApplySingleControl(sc.alternateControllerHandModelControl);
        }

        private static void ApplySingleControl(HandModelControl h)
        {
            if (h == null)
                return;

            h.leftHandEnabled = true;
            h.rightHandEnabled = true;

            if (_leftArticulated)
            {
                EnsurePreferredHandModels(h, left: true, right: false);
            }
            else if (HandsArrayHasNamedModel(h.leftHands, SphereKinematicChoice))
            {
                h.leftHandChoice = SphereKinematicChoice;
            }
            else
            {
                h.leftHandEnabled = false;
            }

            if (_rightArticulated)
            {
                EnsurePreferredHandModels(h, left: false, right: true);
            }
            else if (HandsArrayHasNamedModel(h.rightHands, SphereKinematicChoice))
            {
                h.rightHandChoice = SphereKinematicChoice;
            }
            else
            {
                h.rightHandEnabled = false;
            }

            h.useCollision = _leftArticulated || _rightArticulated;
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

        private static bool HandsArrayHasNamedModel(HandModelControl.Hand[] hands, string modelName)
        {
            if (hands == null || string.IsNullOrEmpty(modelName))
                return false;
            for (int i = 0; i < hands.Length; i++)
            {
                HandModelControl.Hand hand = hands[i];
                if (hand != null && hand.name == modelName)
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
