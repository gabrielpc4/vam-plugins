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
        internal static bool ShouldSkipQueue(
            GabrielSessionOrchestrator orchestrator)
        {
            if (SpankingsGripBlockPathKeywords
                .CurrentSceneBlocksGripSpankingsMerge())
                return true;
            if (orchestrator != null &&
                orchestrator.IsLoadDefaultOnLongNonLoopAnimationEndEnabled())
            {
                float animationMinSec =
                    orchestrator.GetMinNonLoopAnimationSecondsForDefaultScene();
                if (AnimationNoLoopDetection
                    .CurrentSceneBlocksGripSpankingsMerge(animationMinSec))
                    return true;
            }

            return false;
        }

        internal static IEnumerator CoMergeAfterGripDeferred(
            GabrielSessionOrchestrator orchestrator,
            GabrielHud hud)
        {
            try
            {
                yield return null;
                yield return null;
                if (hud == null)
                    yield break;
                if (SpankingsGripBlockPathKeywords
                    .CurrentSceneBlocksGripSpankingsMerge())
                    yield break;
                if (orchestrator != null &&
                    orchestrator.IsLoadDefaultOnLongNonLoopAnimationEndEnabled())
                {
                    float animationMinSec =
                        orchestrator
                            .GetMinNonLoopAnimationSecondsForDefaultScene();
                    if (AnimationNoLoopDetection
                        .CurrentSceneBlocksGripSpankingsMerge(animationMinSec))
                        yield break;
                }
                hud.MergeSpankingsOnFemalePersonsOnly();
                yield return new WaitForSeconds(4f);
                if (hud == null)
                    yield break;
                if (SpankingsGripBlockPathKeywords
                    .CurrentSceneBlocksGripSpankingsMerge())
                    yield break;
                if (orchestrator != null &&
                    orchestrator.IsLoadDefaultOnLongNonLoopAnimationEndEnabled())
                {
                    float animationMinSecRetry =
                        orchestrator
                            .GetMinNonLoopAnimationSecondsForDefaultScene();
                    if (AnimationNoLoopDetection
                        .CurrentSceneBlocksGripSpankingsMerge(
                            animationMinSecRetry))
                        yield break;
                }
                if (hud.AnyFemalePersonMissingSpankings())
                    hud.MergeSpankingsOnFemalePersonsOnly();
            }
            finally
            {
            }
        }
    }
}
