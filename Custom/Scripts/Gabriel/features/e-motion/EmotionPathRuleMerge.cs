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
        internal static IEnumerator CoApplyAfterSceneSettles(GabrielHud hud)
        {
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.35f);

            try
            {
                if (SuperController.singleton == null || hud == null)
                    yield break;

                bool pathRuleMerge = EmotionPathKeywords.MatchesCurrentScenePath();

                if (pathRuleMerge)
                    hud.MergeEmotionLiteOnAllPersonsOnly();
                hud.RefreshPluginToggleLabels();
            }
            finally
            {
            }
        }

        internal static IEnumerator CoPathRuleMergeDeferred(GabrielHud hud)
        {
            try
            {
                SuperController sc = SuperController.singleton;
                while (sc != null && sc.isLoading)
                    yield return null;

                yield return null;
                yield return null;
                yield return new WaitForSecondsRealtime(0.35f);

                if (sc == null || hud == null)
                    yield break;
                if (!EmotionPathKeywords.MatchesCurrentScenePath())
                    yield break;

                hud.MergeEmotionLiteOnAllPersonsOnly();
                hud.RefreshPluginToggleLabels();
            }
            finally
            {
            }
        }
    }
}
