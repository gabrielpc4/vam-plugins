using System.Collections;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Scenes that expose a <c>UIButton</c> atom uid <c>nxtUIButton</c> (see storables → Trigger): pressing the
    /// <b>right</b> Oculus / Quest thumbstick runs the same trigger pulse as clicking that UI button.
    /// </summary>
    internal static class EasyMateNxtUiQuestThumbstick
    {
        private const string NxtUiAtomUid = "nxtUIButton";
        private const string TriggerStorableId = "Trigger";

        public static void Tick(MVRScript host)
        {
            if (host == null || SuperController.singleton == null || SuperController.singleton.isLoading)
                return;

            SuperController sc = SuperController.singleton;
            if (!sc.isOVR && !sc.isOpenVR && !UnityEngine.XR.XRSettings.enabled)
                return;

            if (!EasyMateVrInput.PollRightThumbstickClickDown(sc))
                return;

            Atom a = SuperController.singleton.GetAtomByUid(NxtUiAtomUid);
            if (a == null || !a.gameObject.activeInHierarchy || a.type != "UIButton")
                return;

            UIButtonTrigger ubt = a.GetStorableByID(TriggerStorableId) as UIButtonTrigger;
            if (ubt == null || ubt.trigger == null)
                return;

            host.StartCoroutine(FireTriggerLikeButtonClick(ubt));
        }

        private static IEnumerator FireTriggerLikeButtonClick(UIButtonTrigger ubt)
        {
            if (ubt == null || ubt.trigger == null)
                yield break;
            ubt.trigger.active = true;
            yield return null;
            if (ubt != null && ubt.trigger != null)
                ubt.trigger.active = false;
        }
    }
}
