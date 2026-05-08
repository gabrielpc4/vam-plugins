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
    /// </summary>
    public class TriggerClothingRemover : MVRScript
    {
        private const float MaxHandToTorsoMeters = 0.62f;

        public override void Init()
        {
        }

        public void Update()
        {
            TickStrip();
        }

        private static void TickStrip()
        {
            SuperController sc = SuperController.singleton;
            bool leftTriggerDown;
            bool rightTriggerDown;

            if (sc == null || sc.isLoading)
            {
                return;
            }

            if (!sc.isOVR && !sc.isOpenVR)
            {
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

            if (!TryFindNearestPersonForStrip(handPos, out bestPerson, out preferUpper))
            {
                return;
            }

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

                FreeControllerV3 chest =
                    atom.GetStorableByID("chestControl") as FreeControllerV3;
                FreeControllerV3 pelvis =
                    atom.GetStorableByID("pelvisControl") as FreeControllerV3;

                Vector3 chestPos;
                Vector3 pelvisPos;
                if (!TryFreeControllerWorldPosition(chest, out chestPos))
                {
                    continue;
                }

                if (!TryFreeControllerWorldPosition(pelvis, out pelvisPos))
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

        private static bool TryFreeControllerWorldPosition(
            FreeControllerV3 fc,
            out Vector3 world)
        {
            world = Vector3.zero;
            if (fc == null)
            {
                return false;
            }

            if (fc.followWhenOff != null)
            {
                world = fc.followWhenOff.position;
                return true;
            }

            // VaM session compile crashed when this fallback used fc.control
            // (see 814214a); follow + transform still valid on FreeControllerV3.
            if (fc.follow != null)
            {
                world = fc.follow.position;
                return true;
            }

            if (fc.transform != null)
            {
                world = fc.transform.position;
                return true;
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
