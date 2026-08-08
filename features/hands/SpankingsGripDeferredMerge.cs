using System.Collections;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// First-grip Spankings merge on female Persons (deferred), with long
    /// non-loop scene skip rules matching <see cref="AnimationNoLoopDetection"/>.
    /// </summary>
    internal static class SpankingsGripDeferredMerge
    {
        internal static bool ShouldSkipQueue(
            SessionOrchestrator orchestrator)
        {
            if (SpankingsGripBlockPathKeywords
                .CurrentSceneBlocksGripSpankingsMerge())
                return true;
            if (orchestrator != null)
            {
                if (AnimationNoLoopDetection
                    .CurrentSceneBlocksGripSpankingsMerge(
                        SessionOrchestrator
                            .MinNonLoopAnimationSecondsForDefaultSceneLoad))
                    return true;
            }

            return false;
        }

        internal static IEnumerator CoMergeAfterGripDeferred(
            SessionOrchestrator orchestrator,
            Hud hud)
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
                if (orchestrator != null)
                {
                    if (AnimationNoLoopDetection
                        .CurrentSceneBlocksGripSpankingsMerge(
                            SessionOrchestrator
                                .MinNonLoopAnimationSecondsForDefaultSceneLoad))
                        yield break;
                }
                hud.MergeSpankingsOnFemalePersonsOnly();
                yield return new WaitForSeconds(4f);
                if (hud == null)
                    yield break;
                if (SpankingsGripBlockPathKeywords
                    .CurrentSceneBlocksGripSpankingsMerge())
                    yield break;
                if (orchestrator != null)
                {
                    if (AnimationNoLoopDetection
                        .CurrentSceneBlocksGripSpankingsMerge(
                            SessionOrchestrator
                                .MinNonLoopAnimationSecondsForDefaultSceneLoad))
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
