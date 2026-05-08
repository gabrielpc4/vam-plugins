using System.Collections.Generic;
using MeshVR;

namespace geesp0t
{
    /// <summary>
    /// Torso garment band for proximity-strip classification.
    /// </summary>
    public enum ClothingTorsoBand
    {
        Unknown = 0,

        Upper = 1,

        Lower = 2,

        FullBody = 3
    }

    /// <summary>
    /// Torso-band keywords, text heuristics, classification, and pick-one rules
    /// for proximity strip (shared across Gabriel clothing features).
    /// Nested helper types flattened for VaM dynamic Mono emitter stability.
    /// </summary>
    public static class ClothingClassifier
    {
        private static readonly string[] _fullBodyKeywords = new string[]
        {
            "dress",
            "gown",
            "jumpsuit",
            "catsuit",
            "bodysuit",
            "romper",
            "overall",
        };

        private static readonly string[] _upperKeywords = new string[]
        {
            "top",
            "shirt",
            "bra",
            "blouse",
            "jacket",
            "coat",
            "sweater",
            "hoodie",
            "vest",
            "cardigan",
            "tank",
            "corset",
            "bustier",
            "halter",
            "tube top",
            "tubetop",
            "crop ",
            "tie",
            "scarf",
            "glass",
        };

        private static readonly string[] _lowerKeywords = new string[]
        {
            "panties",
            "underwear",
            "pant",
            "jeans",
            "shorts",
            "skirt",
            "thong",
            "brief",
            "boxer",
            "legging",
            "stocking",
            "hose",
            "garter",
            "sock",
            "shoe",
            "boot",
            "heel",
            "belt",
            "bikini bottom",
            "mini skirt",
            "miniskirt",
            "cargo",
            "trouser",
            "kilt",
        };

        public static bool TryPickClothingItemToRemove(
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
            int i;
            for (i = 0; i < items.Length; i++)
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
                if (TryFirstMatchingBand(active, ClothingTorsoBand.Upper, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, ClothingTorsoBand.FullBody, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, ClothingTorsoBand.Lower, out picked))
                {
                    return true;
                }
            }
            else
            {
                if (TryFirstMatchingBand(active, ClothingTorsoBand.Lower, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, ClothingTorsoBand.FullBody, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, ClothingTorsoBand.Upper, out picked))
                {
                    return true;
                }
            }

            if (TryFirstMatchingBand(active, ClothingTorsoBand.Unknown, out picked))
            {
                return true;
            }

            picked = active[0];
            return true;
        }

        public static ClothingTorsoBand ClassifyTorsoBand(DAZClothingItem item)
        {
            DAZClothingItem.ExclusiveRegion region = item.exclusiveRegion;
            string blob = SearchBlob(item);

            if (BlobContainsAny(blob, _fullBodyKeywords))
            {
                return ClothingTorsoBand.FullBody;
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
                return ClothingTorsoBand.FullBody;
            }

            if (upperFromRegion)
            {
                return ClothingTorsoBand.Upper;
            }

            if (lowerFromRegion)
            {
                return ClothingTorsoBand.Lower;
            }

            bool upperFromText = BlobContainsAny(blob, _upperKeywords);
            bool lowerFromText = BlobContainsAny(blob, _lowerKeywords);

            if (upperFromText && lowerFromText)
            {
                return ClothingTorsoBand.FullBody;
            }

            if (upperFromText)
            {
                return ClothingTorsoBand.Upper;
            }

            if (lowerFromText)
            {
                return ClothingTorsoBand.Lower;
            }

            if (blob.Contains("bikini"))
            {
                return ClothingTorsoBand.FullBody;
            }

            if (blob.Contains("lingerie"))
            {
                return ClothingTorsoBand.FullBody;
            }

            return ClothingTorsoBand.Unknown;
        }

        private static string SearchBlob(DAZClothingItem item)
        {
            string blob = " " + (item.displayName ?? "") + " " +
                (item.tags ?? "") + " ";

            if (item.tagsArray != null)
            {
                int ti;
                for (ti = 0; ti < item.tagsArray.Length; ti++)
                {
                    string tag = item.tagsArray[ti];
                    if (!string.IsNullOrEmpty(tag))
                    {
                        blob += tag + " ";
                    }
                }
            }

            return blob.ToLowerInvariant();
        }

        private static bool TryFirstMatchingBand(
            List<DAZClothingItem> items,
            ClothingTorsoBand band,
            out DAZClothingItem found)
        {
            found = null;
            int i;
            for (i = 0; i < items.Count; i++)
            {
                DAZClothingItem item = items[i];
                if (ClassifyTorsoBand(item) == band)
                {
                    found = item;
                    return true;
                }
            }

            return false;
        }

        private static bool BlobContainsAny(string blob, string[] keys)
        {
            int i;
            for (i = 0; i < keys.Length; i++)
            {
                if (blob.Contains(keys[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
