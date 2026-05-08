using MeshVR;

namespace geesp0t
{
    /// <summary>
    /// Torso-band keywords, text heuristics, classification, and pick-one rules
    /// for proximity strip. No enum or List in this module so VaM dynamic Mono
    /// emit stays minimal (emitter crash at ClassifyTorsoBand public enum slice).
    /// </summary>
    public static class ClothingClassifier
    {
        private const int BandUnknown = 0;

        private const int BandUpper = 1;

        private const int BandLower = 2;

        private const int BandFullBody = 3;

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
            DAZClothingItem[] bucket;
            DAZClothingItem[] items;
            int activeCount;
            int i;

            picked = null;
            items = selector.clothingItems;
            if (items == null)
            {
                return false;
            }

            activeCount = 0;
            for (i = 0; i < items.Length; i++)
            {
                if (items[i] != null && items[i].active)
                {
                    activeCount++;
                }
            }

            if (activeCount == 0)
            {
                return false;
            }

            bucket = new DAZClothingItem[activeCount];
            activeCount = 0;
            for (i = 0; i < items.Length; i++)
            {
                DAZClothingItem item = items[i];
                if (item != null && item.active)
                {
                    bucket[activeCount++] = item;
                }
            }

            if (preferUpper)
            {
                if (TryFirstMatchingBand(bucket, activeCount, BandUpper, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(bucket, activeCount, BandFullBody, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(bucket, activeCount, BandLower, out picked))
                {
                    return true;
                }
            }
            else
            {
                if (TryFirstMatchingBand(bucket, activeCount, BandLower, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(bucket, activeCount, BandFullBody, out picked))
                {
                    return true;
                }

                if (TryFirstMatchingBand(bucket, activeCount, BandUpper, out picked))
                {
                    return true;
                }
            }

            if (TryFirstMatchingBand(bucket, activeCount, BandUnknown, out picked))
            {
                return true;
            }

            picked = bucket[0];
            return true;
        }

        private static int ClassifyTorsoBandInt(DAZClothingItem item)
        {
            DAZClothingItem.ExclusiveRegion region = item.exclusiveRegion;
            string blob = SearchBlob(item);

            if (BlobContainsAny(blob, _fullBodyKeywords))
            {
                return BandFullBody;
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
                return BandFullBody;
            }

            if (upperFromRegion)
            {
                return BandUpper;
            }

            if (lowerFromRegion)
            {
                return BandLower;
            }

            bool upperFromText = BlobContainsAny(blob, _upperKeywords);
            bool lowerFromText = BlobContainsAny(blob, _lowerKeywords);

            if (upperFromText && lowerFromText)
            {
                return BandFullBody;
            }

            if (upperFromText)
            {
                return BandUpper;
            }

            if (lowerFromText)
            {
                return BandLower;
            }

            if (blob.Contains("bikini"))
            {
                return BandFullBody;
            }

            if (blob.Contains("lingerie"))
            {
                return BandFullBody;
            }

            return BandUnknown;
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
            DAZClothingItem[] bucket,
            int bucketCount,
            int band,
            out DAZClothingItem found)
        {
            int i;

            found = null;
            for (i = 0; i < bucketCount; i++)
            {
                DAZClothingItem item = bucket[i];
                if (ClassifyTorsoBandInt(item) == band)
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
