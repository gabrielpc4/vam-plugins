namespace geesp0t
{
    /// <summary>
    /// Sets <see cref="UserPreferences.softPhysics"/> from Gabriel scene-motion
    /// policy driven by long non-loop detection: off for dominant non-loop
    /// dance-length clips (performance), on for all other scenes. Exception paths
    /// (same folder tokens as animation no-loop exclusion, e.g.
    /// <c>booty shake</c>) keep soft physics on even when motion qualifies as long
    /// non-looping.
    /// </summary>
    internal static class SoftPhysicsScenePreference
    {
        /// <summary>
        /// Applies prefs once callers have recomputed animation flags (typically
        /// from <see cref="AnimationNoLoopDetection"/> after a scene reset).
        /// </summary>
        internal static void ApplyFromLongNonLoopSceneFlags(
            bool sceneHasLongNonLoopAnimation,
            bool matchesExceptionRule)
        {
            UserPreferences prefs;
            bool wantSoftPhysics;

            prefs = UserPreferences.singleton;
            if (prefs == null)
            {
                return;
            }

            wantSoftPhysics =
                !sceneHasLongNonLoopAnimation || matchesExceptionRule;
            prefs.softPhysics = wantSoftPhysics;
        }
    }
}
