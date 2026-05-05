using System;
using UnityEngine;

namespace geesp0t
{
    public class VrTriggerProximityStripClothingPlugin : MVRScript
    {
        private const float MaxHandToTorsoMeters = 0.62f;

        private const int BandUnknown = 0;
        private const int BandUpper = 1;
        private const int BandLower = 2;
        private const int BandFull = 3;

        private static readonly string[] FullBodyKeywords = new string[]
        {
            "dress", "gown", "jumpsuit", "catsuit", "bodysuit", "romper", "overall"
        };

        private static readonly string[] UpperKeywords = new string[]
        {
            "top", "shirt", "bra", "blouse", "jacket", "coat", "sweater", "hoodie", "vest", "cardigan",
            "tank", "corset", "bustier", "halter", "tube top", "tubetop", "crop ", "tie", "scarf", "glass"
        };

        private static readonly string[] LowerKeywords = new string[]
        {
            "panties", "underwear", "pant", "jeans", "shorts", "skirt", "thong", "brief", "boxer",
            "legging", "stocking", "hose", "garter", "sock", "shoe", "boot", "heel", "belt", "bikini bottom",
            "mini skirt", "miniskirt", "cargo", "trouser", "kilt"
        };

        public override void Init()
        {
        }

        public void Update()
        {
            SuperController superController = SuperController.singleton;
            bool leftTriggerDown;
            bool rightTriggerDown;

            if (superController == null || superController.isLoading)
            {
                return;
            }

            if (!superController.isOVR && !superController.isOpenVR)
            {
                return;
            }

            try
            {
                leftTriggerDown = superController.GetLeftGrab();
                rightTriggerDown = superController.GetRightGrab();
            }
            catch (Exception e)
            {
                SuperController.LogError("VrTriggerProximityStripClothingPlugin input failed: " + e.Message);
                return;
            }

            if (!leftTriggerDown && !rightTriggerDown)
            {
                return;
            }

            if (leftTriggerDown)
            {
                TryStripClothingWithHand(superController.leftHand, "left");
            }

            if (rightTriggerDown)
            {
                TryStripClothingWithHand(superController.rightHand, "right");
            }
        }

        private static void TryStripClothingWithHand(Transform handTransform, string handLabel)
        {
            Atom bestPerson;
            bool preferUpper;
            Vector3 handPosition;
            DAZCharacterSelector selector;
            DAZClothingItem clothingItem;

            if (handTransform == null || !handTransform.gameObject.activeInHierarchy)
            {
                return;
            }

            handPosition = handTransform.position;

            if (!TryFindNearestPerson(handPosition, out bestPerson, out preferUpper))
            {
                return;
            }

            selector = bestPerson.GetStorableByID("geometry") as DAZCharacterSelector;
            if (selector == null)
            {
                return;
            }

            if (!TryPickClothingItem(selector, preferUpper, out clothingItem))
            {
                return;
            }

            try
            {
                selector.SetActiveClothingItem(clothingItem, false);
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "VrTriggerProximityStripClothingPlugin strip failed (" +
                    handLabel +
                    " hand, " +
                    bestPerson.uid +
                    "): " +
                    e.Message);
            }
        }

        private static bool TryFindNearestPerson(Vector3 handPosition, out Atom bestPerson, out bool preferUpper)
        {
            float bestDistance;
            System.Collections.Generic.List<Atom> atoms;
            int atomIndex;

            bestPerson = null;
            preferUpper = true;
            bestDistance = float.MaxValue;

            atoms = SuperController.singleton.GetAtoms();
            if (atoms == null)
            {
                return false;
            }

            for (atomIndex = 0; atomIndex < atoms.Count; atomIndex++)
            {
                Atom atom;
                FreeControllerV3 chestControl;
                FreeControllerV3 pelvisControl;
                Vector3 chestPosition;
                Vector3 pelvisPosition;
                float chestDistance;
                float pelvisDistance;
                float nearestDistance;
                DAZCharacterSelector selector;

                atom = atoms[atomIndex];
                if (atom == null || atom.type != "Person")
                {
                    continue;
                }

                chestControl = atom.GetStorableByID("chestControl") as FreeControllerV3;
                pelvisControl = atom.GetStorableByID("pelvisControl") as FreeControllerV3;

                if (!TryGetControllerPosition(chestControl, out chestPosition))
                {
                    continue;
                }

                if (!TryGetControllerPosition(pelvisControl, out pelvisPosition))
                {
                    pelvisPosition = chestPosition;
                }

                chestDistance = Vector3.Distance(handPosition, chestPosition);
                pelvisDistance = Vector3.Distance(handPosition, pelvisPosition);
                nearestDistance = Mathf.Min(chestDistance, pelvisDistance);

                if (nearestDistance > MaxHandToTorsoMeters)
                {
                    continue;
                }

                selector = atom.GetStorableByID("geometry") as DAZCharacterSelector;
                if (selector == null || !HasActiveClothing(selector))
                {
                    continue;
                }

                if (nearestDistance < bestDistance)
                {
                    bestDistance = nearestDistance;
                    bestPerson = atom;
                    preferUpper = chestDistance <= pelvisDistance;
                }
            }

            return bestPerson != null;
        }

        private static bool TryGetControllerPosition(FreeControllerV3 freeController, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;

            if (freeController == null)
            {
                return false;
            }

            if (freeController.followWhenOff != null)
            {
                worldPosition = freeController.followWhenOff.position;
                return true;
            }

            if (freeController.follow != null)
            {
                worldPosition = freeController.follow.position;
                return true;
            }

            if (freeController.transform != null)
            {
                worldPosition = freeController.transform.position;
                return true;
            }

            return false;
        }

        private static bool HasActiveClothing(DAZCharacterSelector selector)
        {
            DAZClothingItem[] clothingItems;
            int itemIndex;

            clothingItems = selector.clothingItems;
            if (clothingItems == null)
            {
                return false;
            }

            for (itemIndex = 0; itemIndex < clothingItems.Length; itemIndex++)
            {
                DAZClothingItem clothingItem = clothingItems[itemIndex];
                if (clothingItem != null && clothingItem.active)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryPickClothingItem(DAZCharacterSelector selector, bool preferUpper, out DAZClothingItem pickedItem)
        {
            DAZClothingItem[] clothingItems;

            clothingItems = selector.clothingItems;
            pickedItem = null;

            if (clothingItems == null || clothingItems.Length == 0)
            {
                return false;
            }

            if (preferUpper)
            {
                if (TryPickFirstMatchingBand(clothingItems, BandUpper, out pickedItem))
                {
                    return true;
                }

                if (TryPickFirstMatchingBand(clothingItems, BandFull, out pickedItem))
                {
                    return true;
                }

                if (TryPickFirstMatchingBand(clothingItems, BandLower, out pickedItem))
                {
                    return true;
                }
            }
            else
            {
                if (TryPickFirstMatchingBand(clothingItems, BandLower, out pickedItem))
                {
                    return true;
                }

                if (TryPickFirstMatchingBand(clothingItems, BandFull, out pickedItem))
                {
                    return true;
                }

                if (TryPickFirstMatchingBand(clothingItems, BandUpper, out pickedItem))
                {
                    return true;
                }
            }

            if (TryPickFirstMatchingBand(clothingItems, BandUnknown, out pickedItem))
            {
                return true;
            }

            return TryPickFirstActive(clothingItems, out pickedItem);
        }

        private static bool TryPickFirstMatchingBand(DAZClothingItem[] clothingItems, int desiredBand, out DAZClothingItem pickedItem)
        {
            int itemIndex;

            pickedItem = null;

            for (itemIndex = 0; itemIndex < clothingItems.Length; itemIndex++)
            {
                DAZClothingItem clothingItem = clothingItems[itemIndex];
                if (clothingItem == null || !clothingItem.active)
                {
                    continue;
                }

                if (ClassifyBand(clothingItem) == desiredBand)
                {
                    pickedItem = clothingItem;
                    return true;
                }
            }

            return false;
        }

        private static bool TryPickFirstActive(DAZClothingItem[] clothingItems, out DAZClothingItem pickedItem)
        {
            int itemIndex;

            pickedItem = null;

            for (itemIndex = 0; itemIndex < clothingItems.Length; itemIndex++)
            {
                DAZClothingItem clothingItem = clothingItems[itemIndex];
                if (clothingItem != null && clothingItem.active)
                {
                    pickedItem = clothingItem;
                    return true;
                }
            }

            return false;
        }

        private static int ClassifyBand(DAZClothingItem clothingItem)
        {
            DAZClothingItem.ExclusiveRegion region;
            string searchText;
            bool upperByRegion;
            bool lowerByRegion;
            bool upperByText;
            bool lowerByText;

            region = clothingItem.exclusiveRegion;
            searchText = BuildSearchText(clothingItem);

            if (ContainsAny(searchText, FullBodyKeywords))
            {
                return BandFull;
            }

            upperByRegion =
                region == DAZClothingItem.ExclusiveRegion.Chest ||
                region == DAZClothingItem.ExclusiveRegion.UnderChest ||
                region == DAZClothingItem.ExclusiveRegion.Hat ||
                region == DAZClothingItem.ExclusiveRegion.Glasses ||
                region == DAZClothingItem.ExclusiveRegion.Gloves;

            lowerByRegion =
                region == DAZClothingItem.ExclusiveRegion.Hip ||
                region == DAZClothingItem.ExclusiveRegion.UnderHip ||
                region == DAZClothingItem.ExclusiveRegion.Legs ||
                region == DAZClothingItem.ExclusiveRegion.Shoes;

            if (upperByRegion)
            {
                return BandUpper;
            }

            if (lowerByRegion)
            {
                return BandLower;
            }

            upperByText = ContainsAny(searchText, UpperKeywords);
            lowerByText = ContainsAny(searchText, LowerKeywords);

            if (upperByText && lowerByText)
            {
                return BandFull;
            }

            if (upperByText)
            {
                return BandUpper;
            }

            if (lowerByText)
            {
                return BandLower;
            }

            if (searchText.Contains("bikini") || searchText.Contains("lingerie"))
            {
                return BandFull;
            }

            return BandUnknown;
        }

        private static string BuildSearchText(DAZClothingItem clothingItem)
        {
            string searchText;
            string[] tagsArray;
            int tagIndex;

            searchText = " " + (clothingItem.displayName ?? "") + " " + (clothingItem.tags ?? "") + " ";
            tagsArray = clothingItem.tagsArray;

            if (tagsArray == null)
            {
                return searchText.ToLowerInvariant();
            }

            for (tagIndex = 0; tagIndex < tagsArray.Length; tagIndex++)
            {
                string tag = tagsArray[tagIndex];
                if (!string.IsNullOrEmpty(tag))
                {
                    searchText += tag + " ";
                }
            }

            return searchText.ToLowerInvariant();
        }

        private static bool ContainsAny(string sourceText, string[] values)
        {
            int valueIndex;

            for (valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                if (sourceText.Contains(values[valueIndex]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
