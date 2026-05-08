using System;
using System.Collections.Generic;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VR: trigger press (<see cref="SuperController.GetLeftGrab"/> / <see cref="SuperController.GetRightGrab"/> are
    /// one-frame edges in VaM, not hold-repeat) strips one active clothing item on the nearest Person when the hand
    /// is within reach of torso anchors. Upper vs lower follows chest vs pelvis distance; falls back across bands.
    /// Invoked from <see cref="VrProximityStripClothingPlugin"/> (separate session plugin assembly — avoids Mono compile crash when bundled with Auto_Load).
    /// </summary>
    public static class VrProximityStripClothing
    {
        private const float MaxHandToTorsoMeters = 0.62f;

        /// <summary>0 = unknown, 1 = upper, 2 = lower, 3 = full body / both.</summary>
        private enum ClothingBand
        {
            Unknown = 0,
            Upper = 1,
            Lower = 2,
            FullBody = 3
        }

        public static void Tick()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
            {
                return;
            }

            if (!sc.isOVR && !sc.isOpenVR)
            {
                return;
            }

            bool leftTriggerDown = sc.GetLeftGrab();
            bool rightTriggerDown = sc.GetRightGrab();

            if (!leftTriggerDown && !rightTriggerDown)
            {
                return;
            }

            if (leftTriggerDown)
            {
                TryStripWithHand(sc.leftHand, "left");
            }

            if (rightTriggerDown)
            {
                TryStripWithHand(sc.rightHand, "right");
            }
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

            DAZCharacterSelector selector = bestPerson.GetStorableByID("geometry") as DAZCharacterSelector;
            if (selector == null)
            {
                SuperController.LogError(
                    "VrProximityStripClothing: no geometry on Person " + bestPerson.uid);
                return;
            }

            DAZClothingItem toRemove;
            if (!TryPickClothingItemToRemove(selector, preferUpper, out toRemove))
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
                    "VrProximityStripClothing: SetActiveClothingItem failed (" +
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

            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if (atom == null || atom.type != "Person")
                {
                    continue;
                }

                FreeControllerV3 chest = atom.GetStorableByID("chestControl") as FreeControllerV3;
                FreeControllerV3 pelvis = atom.GetStorableByID("pelvisControl") as FreeControllerV3;

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

                DAZCharacterSelector selector = atom.GetStorableByID("geometry") as DAZCharacterSelector;
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

        private static bool TryFreeControllerWorldPosition(FreeControllerV3 fc, out Vector3 world)
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

            // VaM session compile crashed when this fallback used fc.control (see 814214a); follow + transform still valid on FreeControllerV3.
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

        private static bool TryPickClothingItemToRemove(
            DAZCharacterSelector selector,
            bool preferUpper,
            out DAZClothingItem picked)
        {
            picked = null;
            DAZClothingItem[] items = selector.clothingItems;
            if (items == null)
            {
                return false;
            }

            List<DAZClothingItem> active = new List<DAZClothingItem>();
            for (int i = 0; i < items.Length; i++)
            {
                DAZClothingItem item = items[i];
                if (item != null && item.active)
                {
                    active.Add(item);
                }
            }

            if (active.Count == 0)
            {
                return false;
            }

            if (preferUpper)
            {
                if (TryFirstMatchingBand(active, ClothingBand.Upper, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, ClothingBand.FullBody, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, ClothingBand.Lower, out picked))
                {
                    return true;
                }
            }
            else
            {
                if (TryFirstMatchingBand(active, ClothingBand.Lower, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, ClothingBand.FullBody, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, ClothingBand.Upper, out picked))
                {
                    return true;
                }
            }

            if (TryFirstMatchingBand(active, ClothingBand.Unknown, out picked))
            {
                return true;
            }

            picked = active[0];
            return true;
        }

        private static bool TryFirstMatchingBand(List<DAZClothingItem> items, ClothingBand band, out DAZClothingItem found)
        {
            found = null;
            for (int i = 0; i < items.Count; i++)
            {
                DAZClothingItem item = items[i];
                if (ClassifyClothingBand(item) == band)
                {
                    found = item;
                    return true;
                }
            }

            return false;
        }

        private static bool BlobContainsAny(string blob, string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (blob.Contains(keys[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static ClothingBand ClassifyClothingBand(DAZClothingItem item)
        {
            DAZClothingItem.ExclusiveRegion region = item.exclusiveRegion;
            string blob = VrProximityClothingTextHeuristics.ClothingSearchBlob(item);

            string[] fullBodyKeys =
            {
                "dress", "gown", "jumpsuit", "catsuit", "bodysuit", "romper", "overall"
            };

            if (BlobContainsAny(blob, fullBodyKeys))
            {
                return ClothingBand.FullBody;
            }

            bool upperFromRegion =
                region == DAZClothingItem.ExclusiveRegion.Chest ||
                region == DAZClothingItem.ExclusiveRegion.UnderChest ||
                region == DAZClothingItem.ExclusiveRegion.Hat ||
                region == DAZClothingItem.ExclusiveRegion.Glasses ||
                region == DAZClothingItem.ExclusiveRegion.Gloves;

            bool lowerFromRegion =
                region == DAZClothingItem.ExclusiveRegion.Hip ||
                region == DAZClothingItem.ExclusiveRegion.UnderHip ||
                region == DAZClothingItem.ExclusiveRegion.Legs ||
                region == DAZClothingItem.ExclusiveRegion.Shoes;

            if (upperFromRegion && lowerFromRegion)
            {
                return ClothingBand.FullBody;
            }

            if (upperFromRegion)
            {
                return ClothingBand.Upper;
            }

            if (lowerFromRegion)
            {
                return ClothingBand.Lower;
            }

            string[] upperKeys =
            {
                "top", "shirt", "bra", "blouse", "jacket", "coat", "sweater", "hoodie", "vest", "cardigan",
                "tank", "corset", "bustier", "halter", "tube top", "tubetop", "crop ", "tie", "scarf", "glass"
            };

            string[] lowerKeys =
            {
                "panties", "underwear", "pant", "jeans", "shorts", "skirt", "thong", "brief", "boxer",
                "legging", "stocking", "hose", "garter", "sock", "shoe", "boot", "heel", "belt", "bikini bottom",
                "mini skirt", "miniskirt", "cargo", "trouser", "kilt"
            };

            bool upperFromText = BlobContainsAny(blob, upperKeys);
            bool lowerFromText = BlobContainsAny(blob, lowerKeys);

            if (upperFromText && lowerFromText)
            {
                return ClothingBand.FullBody;
            }

            if (upperFromText)
            {
                return ClothingBand.Upper;
            }

            if (lowerFromText)
            {
                return ClothingBand.Lower;
            }

            if (blob.Contains("bikini"))
            {
                return ClothingBand.FullBody;
            }

            if (blob.Contains("lingerie"))
            {
                return ClothingBand.FullBody;
            }

            return ClothingBand.Unknown;
        }
    }
}
