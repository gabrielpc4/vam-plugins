using System.Collections;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Path-keyword merge of E-Motion Lite when load/save dirs match
    /// <see cref="EmotionPathKeywords"/>.
    /// </summary>
    internal static class EmotionPathRuleMerge
    {
        internal static IEnumerator CoApplyAfterSceneSettles(GabrielHudButtons buttons)
        {
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.35f);

            try
            {
                if (SuperController.singleton == null || buttons == null)
                    yield break;

                bool pathRuleMerge = EmotionPathKeywords.MatchesCurrentScenePath();

                if (pathRuleMerge)
                    buttons.MergeEmotionLiteForPathRuleOnAllPersonsOnly();
                buttons.RefreshPluginToggleLabels();
            }
            finally
            {
            }
        }

        internal static IEnumerator CoPathRuleMergeDeferred(GabrielHudButtons buttons)
        {
            try
            {
                SuperController sc = SuperController.singleton;
                while (sc != null && sc.isLoading)
                    yield return null;

                yield return null;
                yield return null;
                yield return new WaitForSecondsRealtime(0.35f);

                if (sc == null || buttons == null)
                    yield break;
                if (!EmotionPathKeywords.MatchesCurrentScenePath())
                    yield break;

                buttons.MergeEmotionLiteForPathRuleOnAllPersonsOnly();
                buttons.RefreshPluginToggleLabels();
            }
            finally
            {
            }
        }
    }
}
