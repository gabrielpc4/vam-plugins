namespace geesp0t
{
    /// <summary>
    /// Keyword lists for assigning DAZ garments to torso bands (upper, lower,
    /// full-body) when stripping by VR hand proximity.
    /// Shared so other Gabriel features can reuse the same heuristic.
    /// </summary>
    public static class VrProximityStripClothingBandKeywords
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
}
