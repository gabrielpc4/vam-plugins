using System;
using System.Collections;
using MeshVR;
using UnityEngine;
using UnityEngine.UI;

namespace geesp0t
{
    /// <summary>
    /// Resolves the scene &quot;next&quot; <c>UIButton</c> (preferred atom UID or
    /// Portuguese/English label match), then pulses its trigger. Used by the palm HUD
    /// <b>Próxima cena</b> row and VR B/menu shortcuts.
    /// </summary>
    internal static class GabrielHudNextSceneButton
    {
        private static MVRScript _pluginRunner;

        private const string NextSceneUIButtonAtomUid = "nxtUIButton";

        private const string UiButtonTriggerStorableId = "Trigger";

        private const string UiButtonTextStorableId = "Text";

        public static void BindHost(MVRScript plugin)
        {
            _pluginRunner = plugin;
        }

        public static void ReleaseHost()
        {
            _pluginRunner = null;
        }

        /// <summary>
        /// True when a <c>UIButton</c> label should be treated as the scene-advance
        /// control (<c>next</c>, <c>proxima</c>/<c>próxima</c>, etc.).
        /// </summary>
        private static bool LabelMatchesSceneNext(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return false;
            string trimmed = raw.Trim();
            if (trimmed.Length == 0)
                return false;
            if (trimmed.IndexOf("next", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (trimmed.IndexOf("proxima", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (trimmed.IndexOf("pr\u00F3xima", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            return false;
        }

        /// <summary>
        /// Atom UID <c>nxtUIButton</c> if valid, else any active <c>UIButton</c>
        /// matching <see cref="LabelMatchesSceneNext"/>.
        /// </summary>
        public static void RequestFireNextSceneUiButton()
        {
            if (_pluginRunner == null)
                return;
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            UIButtonTrigger ubt = TryResolveTrigger(sc);
            if (ubt == null || ubt.trigger == null)
                return;

            _pluginRunner.StartCoroutine(FireTriggerActivePulseCo(ubt));
        }

        /// <summary>
        /// Close VaM menu first; pulse next-frame so menu-open routing does not block
        /// the scene advance.
        /// </summary>
        public static void RequestFireNextSceneAfterClosingMenu()
        {
            if (_pluginRunner == null)
                return;
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            _pluginRunner.StartCoroutine(FireAfterClosingMenuCo(sc));
        }

        /// <summary>
        /// Used to show palm <b>Próxima cena</b> only when firing would succeed.
        /// </summary>
        public static bool HasNextSceneUiButtonInScene()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return false;
            UIButtonTrigger ubt = TryResolveTrigger(sc);
            return ubt != null && ubt.trigger != null;
        }

        private static UIButtonTrigger TryResolveTrigger(SuperController sc)
        {
            if (sc == null)
                return null;

            Atom byUid = sc.GetAtomByUid(NextSceneUIButtonAtomUid);
            if (byUid != null && byUid.gameObject.activeInHierarchy &&
                string.Equals(byUid.type, "UIButton", StringComparison.Ordinal))
            {
                UIButtonTrigger ubt =
                    byUid.GetStorableByID(UiButtonTriggerStorableId) as UIButtonTrigger;
                if (ubt != null && ubt.trigger != null)
                    return ubt;
            }

            foreach (Atom at in sc.GetAtoms())
            {
                if (at == null ||
                    !string.Equals(at.type, "UIButton", StringComparison.Ordinal) ||
                    !at.gameObject.activeInHierarchy)
                    continue;

                Text[] texts = at.gameObject.GetComponentsInChildren<Text>(true);
                if (texts == null)
                    continue;

                bool labelLooksLikeNext = false;
                for (int i = 0; i < texts.Length; i++)
                {
                    Text t = texts[i];
                    if (t == null || t.text == null)
                        continue;
                    if (LabelMatchesSceneNext(t.text))
                    {
                        labelLooksLikeNext = true;
                        break;
                    }
                }

                if (!labelLooksLikeNext)
                {
                    JSONStorable textStorable = at.GetStorableByID(
                        UiButtonTextStorableId);
                    if (textStorable != null)
                    {
                        JSONStorableString textParam = textStorable.GetStringJSONParam(
                            "text");
                        if (textParam != null &&
                            LabelMatchesSceneNext(textParam.val))
                        {
                            labelLooksLikeNext = true;
                        }
                    }
                }

                if (!labelLooksLikeNext)
                    continue;

                UIButtonTrigger ubt =
                    at.GetStorableByID(UiButtonTriggerStorableId) as UIButtonTrigger;
                if (ubt != null && ubt.trigger != null)
                    return ubt;
            }

            return null;
        }

        private static IEnumerator FireTriggerActivePulseCo(UIButtonTrigger ubt)
        {
            if (ubt == null || ubt.trigger == null)
                yield break;
            ubt.trigger.active = true;
            yield return null;
            if (ubt != null && ubt.trigger != null)
                ubt.trigger.active = false;
        }

        private static IEnumerator FireAfterClosingMenuCo(SuperController sc)
        {
            if (sc == null)
                yield break;

            sc.activeUI = SuperController.ActiveUI.None;
            sc.HideMainHUD();

            yield return null;

            if (sc == null || sc.isLoading)
                yield break;

            UIButtonTrigger ubt = TryResolveTrigger(sc);
            if (ubt == null || ubt.trigger == null)
                yield break;

            yield return FireTriggerActivePulseCo(ubt);
        }
    }
}
