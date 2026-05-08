namespace geesp0t
{
    /// <summary>
    /// Remembers whether first-grip merge slots remain after a VR hand-model
    /// reset. Call <see cref="OnSceneHandModelReset"/> whenever the session
    /// applies <c>DisableVrHandModelsForSceneStart</c> semantics: sequential loads
    /// from the <b>same</b> VaM folder can pass <c>true</c> for a lane so it does
    /// not count as a fresh &quot;first grip&quot; for that lane.
    /// </summary>
    internal static class FirstGripDetection
    {
        private static bool _spankingsFirstGripMergeConsumed;

        private static bool _clothingTouchFallOffFirstMale2MergeConsumed;

        /// <summary>
        /// Call when sphere/idle hand state is applied for a new scene context.
        /// </summary>
        /// <param name="spankingsFirstGripMergeAlreadyConsumed">
        /// When true (e.g. same-folder scene change), skip Spankings
        /// first-grip merge.
        /// </param>
        /// <param name="clothingTouchFallOffFirstMale2MergeAlreadyConsumed">
        /// When true, skip clothing touch fall-off merge on next Male2-on grip.
        /// </param>
        public static void OnSceneHandModelReset(
            bool spankingsFirstGripMergeAlreadyConsumed,
            bool clothingTouchFallOffFirstMale2MergeAlreadyConsumed)
        {
            _spankingsFirstGripMergeConsumed =
                spankingsFirstGripMergeAlreadyConsumed;
            _clothingTouchFallOffFirstMale2MergeConsumed =
                clothingTouchFallOffFirstMale2MergeAlreadyConsumed;
        }

        public static bool SpankingsFirstGripMergeStillAvailable()
        {
            return !_spankingsFirstGripMergeConsumed;
        }

        public static void MarkSpankingsFirstGripMergeConsumed()
        {
            _spankingsFirstGripMergeConsumed = true;
        }

        public static void RevertSpankingsFirstGripMergeConsumed()
        {
            _spankingsFirstGripMergeConsumed = false;
        }

        public static bool ClothingTouchFallOffFirstMale2MergeStillAvailable()
        {
            return !_clothingTouchFallOffFirstMale2MergeConsumed;
        }

        public static void MarkClothingTouchFallOffFirstMale2MergeConsumed()
        {
            _clothingTouchFallOffFirstMale2MergeConsumed = true;
        }
    }
}
