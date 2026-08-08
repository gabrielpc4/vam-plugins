using System.Collections;

namespace geesp0t
{
    /// <summary>
    /// VaM path to the person <c>ClothingTouchFallOff</c> bundle for merge/remove
    /// (<c>.cslist</c> lists <c>ClothingTouchFallOff.cs</c>). Lives with session glue
    /// in this folder (not on <see cref="GabrielHud"/> or
    /// <see cref="GabrielSessionOrchestrator"/>).
    /// </summary>
    public static class ClothingTouchFallOffPluginPath
    {
        public const string PersonPlugin =
            "Custom/Scripts/Gabriel/features/clothing-interactions/" +
            "ClothingTouchFallOff.cslist";
    }

    /// <summary>
    /// Session-side deferred merge for <see cref="ClothingTouchFallOff"/> (person
    /// plugin). This compile comes from <c>GabrielSessionPlugins.cslist</c>; merge
    /// still delivers <c>ClothingTouchFallOff.cslist</c> onto Person atoms.
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
