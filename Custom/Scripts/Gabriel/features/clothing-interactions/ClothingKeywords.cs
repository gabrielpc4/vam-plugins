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
