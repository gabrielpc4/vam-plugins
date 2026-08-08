namespace geesp0t
{
    /// <summary>
    /// After loading a new scene preset, clears possession via VaMScripts HUD if any
    /// FreeController possession was active (avoids stuck rig/person wiring).
    /// </summary>
    internal static class SceneLoadPossessionCleanup
    {
        private static bool SceneHasAnyActivePossession()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3[] fcs =
                        a.transform.GetComponentsInChildren<FreeControllerV3>(true);
                    for (int i = 0; i < fcs.Length; i++)
                    {
                        if (fcs[i] != null && fcs[i].possessed)
                            return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        internal static void ClearPossessionAfterSceneApplyIfHadAny()
        {
            try
            {
                if (!SceneHasAnyActivePossession())
                    return;
                Hud.RequestClearAllPossession(
                    "Hud: ClearPossess after scene load (possession was active).");
            }
            catch (System.Exception e)
            {
                SuperController.LogError(
                    "Hud ClearPossess on scene load failed: " + e.Message);
            }
        }
    }
}
