using System.Collections;

namespace geesp0t
{
    /// <summary>
    /// Session-side deferred merge for <see cref="ClothingTouchFallOff"/> (person
    /// plugin). Loaded with <c>GabrielSessionPlugins.cslist</c>, not with the
    /// standalone Person <c>ClothingTouchFallOff.cs</c> compile.
    /// </summary>
    internal static class ClothingTouchFallOffGripMerge
    {
        internal static IEnumerator CoMergeAfterGripDeferred(
            float minNonLoopAnimationClipSeconds,
            GabrielSessionOrchestrator orchestrator)
        {
            try
            {
                yield return null;
                yield return null;
                if (orchestrator == null)
                {
                    yield break;
                }

                if (AnimationNoLoopDetection
                        .CurrentSceneUsesLongNonLoopAnimation(
                            minNonLoopAnimationClipSeconds))
                {
                    yield break;
                }

                orchestrator.MergeClothingTouchFallOffOnAllPersonsOnly();
                orchestrator.RefreshHudPluginToggleLabels();
            }
            finally
            {
            }
        }
    }
}
