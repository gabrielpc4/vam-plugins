using System;
using System.Collections;
using MeshVR;
using UnityEngine;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// Quest squeeze / OpenVR HoldGrab: toggles Male2 vs sphere unless blocked
    /// (10s after VR euler possess, or while any Person head/hand is possessed,
    /// or passenger mode is active/pending — then <b>None</b> hand models,
    /// no Spankings merge on grip).
    /// </summary>
    internal static class EasyMateGripHandVisibility
    {
        /// <summary>First entry in VaM’s VR hand list: no visible proxy model.</summary>
        private const string NoneHandChoice = "None";

        /// <summary>Second entry in VaM’s VR hand choice list (after None): sphere / kinematic proxy.</summary>
        private const string SphereKinematicChoice = "SphereKinematic";

        private static readonly string[] PreferredHandIds = { "Male2", "Male 2" };

        private static bool _leftArticulated;

        private static bool _rightArticulated;

        /// <summary>Until the user presses a VR grip this scene, sphere proxies stay non-colliding after scene reset.</summary>
        private static bool _vrGripUsedThisScene;

        /// <summary>After <see cref="DisableVrHandModelsForSceneStart"/>, first grip press this scene merges Spankings once onto females missing it.</summary>
        private static bool _mergedSpankingsAfterFirstGripThisScene;

        private static Action _mergeSpankingsOntoPersonsMissingOnly;

        /// <summary>
        /// After VR euler possess start: no grip toggle / first-grip Spankings for
        /// 10s (see <see cref="NotifyVrEulerPossessTenSecondSuppress"/>).
        /// </summary>
        private static float _suppressGripToggleUntilUnscaled;

        /// <summary>Called when VR dual-hand euler possess starts.</summary>
        public static void NotifyVrEulerPossessTenSecondSuppress()
        {
            _suppressGripToggleUntilUnscaled = Time.unscaledTime + 10f;
            _leftArticulated = false;
            _rightArticulated = false;
            SuperController sc = SuperController.singleton;
            ApplyNoneBothControls(sc);
            QueueApplyHandsEndOfFrame(sc);
        }

        /// <summary>Called from <see cref="EasyMate.Init"/>; pass <c>null</c> on teardown.</summary>
        public static void SetMergeSpankingsOnFirstGrip(Action mergeSpankingsOntoPersonsMissingOnly)
        {
            _mergeSpankingsOntoPersonsMissingOnly = mergeSpankingsOntoPersonsMissingOnly;
        }

        /// <summary>Re-apply after SuperController.Update / internal toggles (e.g. grab+trigger hand hide via <c>ToggleRightHandEnabled</c>).</summary>
        private static void QueueApplyHandsEndOfFrame(SuperController sc)
        {
            if (sc == null)
                return;
            sc.StartCoroutine(CoApplyHandsEndOfFrame());
        }

        private static IEnumerator CoApplyHandsEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            if (SuperController.singleton != null)
                ApplyBothControls(SuperController.singleton);
        }

        /// <summary>Scene load: both sides sphere (or legacy-off if slot missing); collisions off until first VR grip.</summary>
        public static void DisableVrHandModelsForSceneStart()
        {
            _leftArticulated = false;
            _rightArticulated = false;
            _mergedSpankingsAfterFirstGripThisScene = false;
            _vrGripUsedThisScene = false;
            SuperController sc = SuperController.singleton;
            ApplyBothControls(sc);
            QueueApplyHandsEndOfFrame(sc);
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

            if (ShouldForceNoneVrHandProxies())
            {
                ApplyNoneBothHandsWhilePossessed(sc);
                return;
            }

            if (Time.unscaledTime < _suppressGripToggleUntilUnscaled)
                return;

            bool leftDown = EasyMateVrInput.PollLeftGripClickDown(sc);
            bool rightDown = EasyMateVrInput.PollRightGripClickDown(sc);

            if (!leftDown && !rightDown)
                return;

            TryMergeSpankingsOnFirstGripPressThisScene();

            _vrGripUsedThisScene = true;

            // Toggle both hands in lockstep (show both Male2 when going articulated) — per-side still respects possession.
            bool nextBothArticulated = !(_leftArticulated && _rightArticulated);
            if (nextBothArticulated)
            {
                _leftArticulated = !AnyPersonLeftHandPossessed();
                _rightArticulated = !AnyPersonRightHandPossessed();
            }
            else
            {
                _leftArticulated = false;
                _rightArticulated = false;
            }

            ApplyBothControls(sc);
            QueueApplyHandsEndOfFrame(sc);
        }

        /// <summary>
        /// True if any <c>Person</c> head or hand control is possessed (player
        /// POV). Used by grip handling and VR euler possess gesture gating.
        /// </summary>
        public static bool IsAnyPersonHeadOrHandPossessed()
        {
            return AnyPersonHeadOrHandPossessed();
        }

        /// <summary>
        /// Force VaM hand proxies to <b>None</b> while possessed <em>or</em> while
        /// passenger mode is active/pending (before hands report possessed).
        /// </summary>
        private static bool ShouldForceNoneVrHandProxies()
        {
            return AnyPersonHeadOrHandPossessed() ||
                EasyMatePassengerRuntime.IsPassengerModeActiveOrPending();
        }

        /// <summary>
        /// Any <c>Person</c> with head or hand control possessed (player POV).
        /// </summary>
        private static bool AnyPersonHeadOrHandPossessed()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || a.type != "Person" ||
                        !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3 h =
                        a.GetStorableByID("headControl") as FreeControllerV3;
                    if (h != null && h.possessed)
                        return true;
                    FreeControllerV3 l =
                        a.GetStorableByID("lHandControl") as FreeControllerV3;
                    if (l != null && l.possessed)
                        return true;
                    FreeControllerV3 r =
                        a.GetStorableByID("rHandControl") as FreeControllerV3;
                    if (r != null && r.possessed)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Name is historical: also used when passenger mode hides proxies
        /// before possession flags flip.
        /// </summary>
        private static void ApplyNoneBothHandsWhilePossessed(SuperController sc)
        {
            if (sc == null)
                return;
            _leftArticulated = false;
            _rightArticulated = false;
            ApplyNoneBothControls(sc);
            QueueApplyHandsEndOfFrame(sc);
        }

        private static void ApplyNoneBothControls(SuperController sc)
        {
            if (sc == null)
                return;
            ApplyNoneOnSingleControl(sc.commonHandModelControl);
            ApplyNoneOnSingleControl(sc.alternateControllerHandModelControl);
        }

        /// <summary>
        /// Both sides use VaM’s <c>None</c> hand entry when present; else that
        /// side disabled.
        /// </summary>
        private static void ApplyNoneOnSingleControl(HandModelControl h)
        {
            if (h == null)
                return;
            h.leftHandEnabled = true;
            h.rightHandEnabled = true;

            if (HandsArrayHasNamedModel(h.leftHands, NoneHandChoice))
                h.leftHandChoice = NoneHandChoice;
            else
                h.leftHandEnabled = false;

            if (HandsArrayHasNamedModel(h.rightHands, NoneHandChoice))
                h.rightHandChoice = NoneHandChoice;
            else
                h.rightHandEnabled = false;

            bool noGripYetNoArticulated =
                !_vrGripUsedThisScene && !_leftArticulated && !_rightArticulated;
            h.useCollision = !noGripYetNoArticulated;
        }

        private static void TryMergeSpankingsOnFirstGripPressThisScene()
        {
            if (_mergedSpankingsAfterFirstGripThisScene)
                return;
            if (_mergeSpankingsOntoPersonsMissingOnly == null)
                return;

            _mergedSpankingsAfterFirstGripThisScene = true;
            try
            {
                _mergeSpankingsOntoPersonsMissingOnly();
            }
            catch (Exception e)
            {
                _mergedSpankingsAfterFirstGripThisScene = false;
                SuperController.LogError("EasyMateGripHandVisibility: Spankings merge on first grip: " + e.Message);
            }
        }

        private static void ApplyBothControls(SuperController sc)
        {
            if (sc == null)
                return;
            bool forceNoneHandProxies = ShouldForceNoneVrHandProxies();
            ApplySingleControl(sc.commonHandModelControl, forceNoneHandProxies);
            ApplySingleControl(
                sc.alternateControllerHandModelControl,
                forceNoneHandProxies);
        }

        private static void ApplySingleControl(
            HandModelControl h,
            bool forceNoneHandProxies)
        {
            if (h == null)
                return;

            if (forceNoneHandProxies)
            {
                ApplyNoneOnSingleControl(h);
                return;
            }

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

            bool sphereOnlyNoGripYet = !_vrGripUsedThisScene && !_leftArticulated && !_rightArticulated;
            h.useCollision = !sphereOnlyNoGripYet;
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
