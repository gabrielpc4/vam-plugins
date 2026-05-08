using System.Collections;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// First-grip Spankings merge on female Persons (deferred), with long non-loop
    /// scene skip rules tied to HUD toggles.
    /// </summary>
    internal static class SpankingsGripDeferredMerge
    {
        internal static bool ShouldSkipQueue(GabrielHud hud, GabrielHudButtons buttons)
        {
            if (buttons == null)
                return true;
            if (SpankingsGripBlockPathKeywords
                .CurrentSceneBlocksGripSpankingsMerge())
                return true;
            if (hud != null && hud.IsLoadDefaultOnLongNonLoopAnimationEndEnabled())
            {
                float animationMinSec =
                    hud.GetMinNonLoopAnimationSecondsForDefaultScene();
                if (AnimationNoLoopMainEnd
                    .CurrentSceneBlocksGripSpankingsMerge(animationMinSec))
                    return true;
            }

            return false;
        }

        internal static IEnumerator CoMergeAfterGripDeferred(
            GabrielHud hud,
            GabrielHudButtons buttons)
        {
            try
            {
                yield return null;
                yield return null;
                if (buttons == null)
                    yield break;
                if (SpankingsGripBlockPathKeywords
                    .CurrentSceneBlocksGripSpankingsMerge())
                    yield break;
                if (hud != null &&
                    hud.IsLoadDefaultOnLongNonLoopAnimationEndEnabled())
                {
                    float animationMinSec =
                        hud.GetMinNonLoopAnimationSecondsForDefaultScene();
                    if (AnimationNoLoopMainEnd
                        .CurrentSceneBlocksGripSpankingsMerge(animationMinSec))
                        yield break;
                }
                buttons.MergeSpankingsOnFemalePersonsOnly();
                yield return new WaitForSeconds(4f);
                if (buttons == null)
                    yield break;
                if (SpankingsGripBlockPathKeywords
                    .CurrentSceneBlocksGripSpankingsMerge())
                    yield break;
                if (hud != null &&
                    hud.IsLoadDefaultOnLongNonLoopAnimationEndEnabled())
                {
                    float animationMinSecRetry =
                        hud.GetMinNonLoopAnimationSecondsForDefaultScene();
                    if (AnimationNoLoopMainEnd
                        .CurrentSceneBlocksGripSpankingsMerge(
                            animationMinSecRetry))
                        yield break;
                }
                if (buttons.AnyFemalePersonMissingSpankings())
                    buttons.MergeSpankingsOnFemalePersonsOnly();
            }
            finally
            {
            }
        }
    }
}
