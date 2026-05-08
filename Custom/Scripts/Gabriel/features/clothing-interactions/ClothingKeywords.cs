using System.Collections.Generic;
using MeshVR;

namespace geesp0t
{
    /// <summary>
    /// Keyword lists for assigning DAZ garments to torso bands (upper, lower,
    /// full-body) when stripping by VR hand proximity.
    /// Shared so other Gabriel features can reuse the same heuristic.
    /// </summary>
    public static class ClothingKeywords
    {
        public static readonly string[] FullBody = new string[]
        {
            "dress",
            "gown",
            "jumpsuit",
            "catsuit",
            "bodysuit",
            "romper",
            "overall",
        };

        public static readonly string[] Upper = new string[]
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

        public static readonly string[] Lower = new string[]
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
    }

    /// <summary>
    /// Torso strip band for <see cref="ClothingTorsoBandPicker"/> classification.
    /// </summary>
    public enum ClothingTorsoBand
    {
        Unknown = 0,
        Upper = 1,
        Lower = 2,
        FullBody = 3
    }

    /// <summary>
    /// Band classification and pick-one-active-garment rules for proximity strip.
    /// </summary>
    public static class ClothingTorsoBandPicker
    {
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

        public static ClothingTorsoBand ClassifyClothingBand(DAZClothingItem item)
        {
            DAZClothingItem.ExclusiveRegion region = item.exclusiveRegion;
            string blob = ClothingTextHeuristics.ClothingSearchBlob(item);

            if (BlobContainsAny(blob, ClothingKeywords.FullBody))
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

            bool upperFromText = BlobContainsAny(blob, ClothingKeywords.Upper);
            bool lowerFromText = BlobContainsAny(blob, ClothingKeywords.Lower);

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

    /// <summary>
    /// Tag/name heuristics for strip and garment text search (shared with proximity
    /// strip and tooling that score items without a full band classify).
    /// </summary>
    public static class ClothingTextHeuristics
    {
        public static string ClothingSearchBlob(DAZClothingItem item)
        {
            string blob = " " + (item.displayName ?? "") + " " +
                (item.tags ?? "") + " ";

            if (item.tagsArray != null)
            {
                foreach (string tag in item.tagsArray)
                {
                    if (!string.IsNullOrEmpty(tag))
                        blob += tag + " ";
                }
            }

            return blob.ToLowerInvariant();
        }

        public static bool LooksLikeSkirtDressOuterGarment(DAZClothingItem item)
        {
            string blob = ClothingSearchBlob(item);
            string[] avoid =
            {
                "skirt", "dress", "gown", "catsuit", "jumpsuit", "hobble",
                "kilt", "robe", "sari", "cheongsam", "ballgown"
            };

            foreach (string token in avoid)
            {
                if (blob.Contains(token))
                    return true;
            }

            return false;
        }

        public static bool IsUnderwearLikeItem(DAZClothingItem item)
        {
            string blob;
            string[] keywords =
            {
                "bra", "panty", "panties", "underwear", "thong", "brief",
                "bikini", "lingerie", "boxer", "boyshort", "pantie",
                "undershirt", "camisole", "pantyhose", "stocking", "garter",
                "corset ", " bustier"
            };

            if (!item.active)
                return false;
            if (LooksLikeSkirtDressOuterGarment(item))
                return false;

            if (item.exclusiveRegion ==
                    DAZClothingItem.ExclusiveRegion.UnderChest ||
                item.exclusiveRegion ==
                    DAZClothingItem.ExclusiveRegion.UnderHip)
            {
                return true;
            }

            blob = ClothingSearchBlob(item);
            foreach (string keyword in keywords)
            {
                if (blob.Contains(keyword))
                    return true;
            }

            return false;
        }
    }
}
