using System.Collections.Generic;
using MeshVR;

namespace geesp0t
{
    /// <summary>
    /// Torso-band keywords, text heuristics, classification, and pick-one rules
    /// for proximity strip (shared across Gabriel clothing features).
    /// </summary>
    public static class ClothingClassifier
    {
        /// <summary>
        /// Name/tag tokens for upper, lower, and full-body torso bands.
        /// </summary>
        public static class Keywords
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
        /// Strip priority band for torso garment classification.
        /// </summary>
        public enum TorsoBand
        {
            Unknown = 0,
            Upper = 1,
            Lower = 2,
            FullBody = 3
        }

        /// <summary>
        /// Lowercased search blob and underwear / outerwear heuristics.
        /// </summary>
        public static class Text
        {
            public static string SearchBlob(DAZClothingItem item)
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
                string blob = SearchBlob(item);
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

                blob = SearchBlob(item);
                foreach (string keyword in keywords)
                {
                    if (blob.Contains(keyword))
                        return true;
                }

                return false;
            }
        }

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
                if (TryFirstMatchingBand(active, TorsoBand.Upper, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, TorsoBand.FullBody, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, TorsoBand.Lower, out picked))
                {
                    return true;
                }
            }
            else
            {
                if (TryFirstMatchingBand(active, TorsoBand.Lower, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, TorsoBand.FullBody, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(active, TorsoBand.Upper, out picked))
                {
                    return true;
                }
            }

            if (TryFirstMatchingBand(active, TorsoBand.Unknown, out picked))
            {
                return true;
            }

            picked = active[0];
            return true;
        }

        public static TorsoBand ClassifyTorsoBand(DAZClothingItem item)
        {
            DAZClothingItem.ExclusiveRegion region = item.exclusiveRegion;
            string blob = Text.SearchBlob(item);

            if (BlobContainsAny(blob, Keywords.FullBody))
            {
                return TorsoBand.FullBody;
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
                return TorsoBand.FullBody;
            }

            if (upperFromRegion)
            {
                return TorsoBand.Upper;
            }

            if (lowerFromRegion)
            {
                return TorsoBand.Lower;
            }

            bool upperFromText = BlobContainsAny(blob, Keywords.Upper);
            bool lowerFromText = BlobContainsAny(blob, Keywords.Lower);

            if (upperFromText && lowerFromText)
            {
                return TorsoBand.FullBody;
            }

            if (upperFromText)
            {
                return TorsoBand.Upper;
            }

            if (lowerFromText)
            {
                return TorsoBand.Lower;
            }

            if (blob.Contains("bikini"))
            {
                return TorsoBand.FullBody;
            }

            if (blob.Contains("lingerie"))
            {
                return TorsoBand.FullBody;
            }

            return TorsoBand.Unknown;
        }

        private static bool TryFirstMatchingBand(
            List<DAZClothingItem> items,
            TorsoBand band,
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
