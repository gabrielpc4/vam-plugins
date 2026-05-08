using System.Collections;

namespace geesp0t
{
    /// <summary>
    /// VaM path to the person <c>ClothingTouchFallOff</c> script for merge/remove
    /// calls. Lives with session glue in this folder (not on <see cref="GabrielHud"/>
    /// or <see cref="GabrielSessionOrchestrator"/>).
    /// </summary>
    public static class ClothingTouchFallOffPluginPath
    {
        public const string PersonPlugin =
            "Custom/Scripts/Gabriel/features/clothing-interactions/ClothingTouchFallOff.cs";
    }

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
