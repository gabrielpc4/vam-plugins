using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Scenes that expose a <c>UIButton</c> atom uid <c>nxtUIButton</c> (see storables → Trigger): pressing the
    /// <b>right</b> Oculus / Quest thumbstick runs the same trigger pulse as clicking that UI button, except when
    /// the VR palm HUD shows the two main rows — then thumbstick runs the <b>Possuir</b>/<b>Despossuir</b> row
    /// (see <see cref="EasyMateVrEulerPossessHandHud"/> after <c>LateUpdate</c> ordering).
    /// </summary>
    internal static class EasyMateNxtUiQuestThumbstick
    {
        public static void Tick()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;
            if (!sc.isOVR && !sc.isOpenVR && !UnityEngine.XR.XRSettings.enabled)
                return;

            if (!EasyMateVrInput.PollRightThumbstickClickDown(sc))
            {
                return;
            }

            if (EasyMateVrEulerPossessHandHud.TryConsumeThumbstickClickForPalmPossessRow())
            {
                return;
            }

            MainUIButtons.RequestFireNextSceneUiButton();
        }
    }
}
