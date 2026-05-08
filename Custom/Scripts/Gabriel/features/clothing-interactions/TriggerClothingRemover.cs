using System;
using System.Collections.Generic;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VR: trigger press (<see cref="SuperController.GetLeftGrab"/> /
    /// <see cref="SuperController.GetRightGrab"/> are one-frame edges in VaM)
    /// removes one active clothing item on the nearest Person when the hand is
    /// within reach of torso anchors. Only when Male2 VR hand model is active.
    /// Upper vs lower follows chest vs pelvis distance; falls back across bands.
    ///
    /// Runs from <see cref="GabrielSessionOrchestrator.LateUpdate"/> (not a
    /// separate CoreControl <c>MVRScript</c>) so the session bundle avoids an
    /// extra plugin instance / <c>Update</c> shim that destabilizes some loads.
    /// When every active Person atom has zero active garments, skips all work until
    /// the next orchestrator scene change or atom UID list change wakes checks.
    /// Gated by <see cref="VrInput.IsLikelyVrRuntimeSafe"/> so desktop modes skip
    /// all garment and trigger polls; VR uses OVR/OpenVR flags and XR fallback when
    /// needed.
    /// </summary>
    internal static class TriggerClothingRemover
    {
        private const float MaxHandToTorsoMeters = 0.62f;

        /// <summary>
        /// When true, no Person in the scene had active clothing last time we
        /// looked; skip work until <see cref="NotifyClothingStripEligibilityDirty"/>.
        /// </summary>
        private static bool _idleNoStripBecauseNoGarments;

        internal static void NotifyClothingStripEligibilityDirty()
        {
            _idleNoStripBecauseNoGarments = false;
        }

        internal static void LateTickStrip(SuperController sc)
        {
            if (!VrInput.IsLikelyVrRuntimeSafe(sc))
            {
                return;
            }

            TickStrip(sc);
        }

        private static void TickStrip(SuperController sc)
        {
            bool leftTriggerDown;
            bool rightTriggerDown;
            bool anyClothes;

            if (sc == null || sc.isLoading)
            {
                return;
            }

            if (_idleNoStripBecauseNoGarments)
            {
                return;
            }

            try
            {
                anyClothes = SceneHasPersonWithAnyActiveClothing();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "TriggerClothingRemover clothing presence scan: " + e.Message);
                return;
            }

            if (!anyClothes)
            {
                _idleNoStripBecauseNoGarments = true;
                return;
            }

            try
            {
                leftTriggerDown = sc.GetLeftGrab();
                rightTriggerDown = sc.GetRightGrab();
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "TriggerClothingRemover input failed: " + e.Message);
                return;
            }

            if (!leftTriggerDown && !rightTriggerDown)
            {
                return;
            }

            if (leftTriggerDown && IsMale2VrHandActive(sc, true))
            {
                TryStripWithHand(sc.leftHand, "left");
            }

            if (rightTriggerDown && IsMale2VrHandActive(sc, false))
            {
                TryStripWithHand(sc.rightHand, "right");
            }
        }

        private static bool IsMale2VrHandActive(
            SuperController superController,
            bool leftHand)
        {
            if (superController == null)
            {
                return false;
            }

            return IsMale2VrHandActiveOnControl(
                superController.commonHandModelControl,
                leftHand) ||
                IsMale2VrHandActiveOnControl(
                    superController.alternateControllerHandModelControl,
                    leftHand);
        }

        private static bool IsMale2VrHandActiveOnControl(
            HandModelControl handModelControl,
            bool leftHand)
        {
            string selectedHandModel;
            bool handEnabled;

            if (handModelControl == null)
            {
                return false;
            }

            if (leftHand)
            {
                selectedHandModel = handModelControl.leftHandChoice;
                handEnabled = handModelControl.leftHandEnabled;
            }
            else
            {
                selectedHandModel = handModelControl.rightHandChoice;
                handEnabled = handModelControl.rightHandEnabled;
            }

            if (!handEnabled || string.IsNullOrEmpty(selectedHandModel))
            {
                return false;
            }

            return string.Equals(selectedHandModel, "Male2", StringComparison.Ordinal) ||
                string.Equals(selectedHandModel, "Male 2", StringComparison.Ordinal);
        }

        private static void TryStripWithHand(Transform handTransform, string whichHandLabel)
        {
            if (handTransform == null || !handTransform.gameObject.activeInHierarchy)
            {
                return;
            }

            Atom bestPerson;
            bool preferUpper;
            Vector3 handPos = handTransform.position;

            try
            {
                if (!TryFindNearestPersonForStrip(handPos, out bestPerson, out preferUpper))
                {
                    return;
                }

                StripOneGarment(bestPerson, preferUpper, whichHandLabel);
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "TriggerClothingRemover strip proximity: " + e.Message);
            }
        }

        private static void StripOneGarment(
            Atom bestPerson,
            bool preferUpper,
            string whichHandLabel)
        {
            DAZCharacterSelector selector =
                bestPerson.GetStorableByID("geometry") as DAZCharacterSelector;
            if (selector == null)
            {
                SuperController.LogError(
                    "TriggerClothingRemover: no geometry on Person " + bestPerson.uid);
                return;
            }

            DAZClothingItem toRemove;
            if (!ClothingClassifier.TryPickClothingItemToRemove(
                    selector,
                    preferUpper,
                    out toRemove))
            {
                return;
            }

            try
            {
                selector.SetActiveClothingItem(toRemove, false);
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "TriggerClothingRemover: SetActiveClothingItem failed (" +
                    whichHandLabel +
                    " hand, " +
                    bestPerson.uid +
                    "): " +
                    e.Message);
            }
        }

        private static bool TryFindNearestPersonForStrip(
            Vector3 handWorld,
            out Atom bestPerson,
            out bool preferUpper)
        {
            bestPerson = null;
            preferUpper = true;

            float bestScore = float.MaxValue;
            bool bestPreferUpper = true;
            List<Atom> persons = PersonAtomCache.GetActivePersonsThisFrame();

            if (persons == null || persons.Count == 0)
            {
                return false;
            }

            for (int personIndex = 0; personIndex < persons.Count; personIndex++)
            {
                Atom atom = persons[personIndex];
                if (atom == null || atom.type != "Person")
                {
                    continue;
                }

                if (atom.gameObject == null || !atom.gameObject.activeInHierarchy)
                {
                    continue;
                }

                FreeControllerV3 chest;
                FreeControllerV3 pelvis;

                Vector3 chestPos;
                Vector3 pelvisPos;
                PersonAtomCache.TryGetCachedFreeController(
                    atom,
                    "chestControl",
                    out chest);
                PersonAtomCache.TryGetCachedFreeController(
                    atom,
                    "pelvisControl",
                    out pelvis);
                if (!PersonAtomCache.TryGetFreeControllerWorldPosition(
                        chest,
                        out chestPos))
                {
                    continue;
                }

                if (!PersonAtomCache.TryGetFreeControllerWorldPosition(
                        pelvis,
                        out pelvisPos))
                {
                    pelvisPos = chestPos;
                }

                float distanceChest = Vector3.Distance(handWorld, chestPos);
                float distancePelvis = Vector3.Distance(handWorld, pelvisPos);
                float nearestTorso = Mathf.Min(distanceChest, distancePelvis);

                if (nearestTorso > MaxHandToTorsoMeters)
                {
                    continue;
                }

                DAZCharacterSelector selector =
                    atom.GetStorableByID("geometry") as DAZCharacterSelector;
                if (selector == null || !HasAnyActiveClothing(selector))
                {
                    continue;
                }

                float score = nearestTorso;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestPerson = atom;
                    bestPreferUpper = distanceChest <= distancePelvis;
                }
            }

            if (bestPerson == null)
            {
                return false;
            }

            preferUpper = bestPreferUpper;
            return true;
        }

        /// <summary>
        /// Cheap presence check: any active Person geometry has active clothing.
        /// </summary>
        private static bool SceneHasPersonWithAnyActiveClothing()
        {
            int i;
            List<Atom> persons = PersonAtomCache.GetActivePersonsThisFrame();

            if (persons == null || persons.Count == 0)
            {
                return false;
            }

            for (i = 0; i < persons.Count; i++)
            {
                Atom atom = persons[i];
                if (atom == null || atom.type != "Person")
                {
                    continue;
                }

                if (atom.gameObject == null || !atom.gameObject.activeInHierarchy)
                {
                    continue;
                }

                try
                {
                    if (PersonAtomCache.PersonHasAnyActiveClothingOnGeometry(atom))
                        return true;
                }
                catch
                {
                    // Geometry / garment state during teardown; skip atom.
                }
            }

            return false;
        }

        private static bool HasAnyActiveClothing(DAZCharacterSelector selector)
        {
            DAZClothingItem[] items = selector.clothingItems;
            if (items == null)
            {
                return false;
            }

            for (int i = 0; i < items.Length; i++)
            {
                DAZClothingItem item = items[i];
                if (item != null && item.active)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
