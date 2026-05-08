using System.Collections;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Defers ClothingTouchFallOff merge on first Male2 VR hand grip unless the scene has
    /// a qualifying long non-loop main motion timeline (matches Default.json-after-mocap
    /// heuristic).
    /// </summary>
    internal static class ClothingTouchFallOffGripMerge
    {
        internal static IEnumerator CoMergeAfterGripDeferred(
            float minNonLoopClipSeconds,
            GabrielHudButtons buttons)
        {
            try
            {
                yield return null;
                yield return null;
                if (buttons == null)
                    yield break;
                if (NonLoopMocapMainEnd
                    .CurrentSceneUsesLongNonLoopMocap(minNonLoopClipSeconds))
                    yield break;
                buttons.MergeClothingTouchFallOffOnAllPersonsOnly();
                buttons.RefreshPluginToggleLabels();
            }
            finally
            {
            }
        }
    }
}
