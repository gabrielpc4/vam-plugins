using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// When the <b>right hand alone</b> matches the HMD-relative euler
    /// window for the VR possess rule, shows two small world UI buttons
    /// over the palm on <c>rightHand</c>: <b>Possuir</b> / <b>Despossuir</b>
    /// (Brazilian Portuguese); or press the VaM menu button (<b>B</b> on
    /// Quest, SteamVR menu) to dismiss the main UI after 100ms and run the
    /// same possess flow as dual-hand euler. Billboard faces the HMD so labels
    /// read correctly from the player's view.
    /// </summary>
    internal static class EasyMateVrEulerPossessHandHud
    {
        private static GameObject _root;

        private static Button _btnPossuir;

        private static Button _btnDespossuir;

        private static bool _listenersAttached;

        private static Sprite _whiteSprite;

        /// <summary>
        /// Local offset on <c>rightHand</c> (meters in hand space). Negative
        /// X tends toward the palm on OpenVR / OVR right controllers.
        /// </summary>
        private static readonly Vector3 LocalPalmOffset =
            new Vector3(-0.065f, 0.012f, 0.025f);

        private const float CanvasWidthPx = 260f;

        private const float CanvasHeightPx = 118f;

        internal static void Tick()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
            {
                SetVisible(false);
                return;
            }

            if (!(sc.isOVR || sc.isOpenVR || XRSettings.enabled))
            {
                SetVisible(false);
                return;
            }

            Transform rh = sc.rightHand;
            if (rh == null)
            {
                SetVisible(false);
                return;
            }

            if (!EasyMateVrEulerPossessPoseCheck.RightHandOnlyMatchTriggerWindow(sc))
            {
                SetVisible(false);
                return;
            }

            float ws = sc.worldScale;
            if (ws < 0.01f)
                ws = 0.01f;

            EnsureHud();
            AttachListenersOnce();

            if (_root.transform.parent != rh)
                _root.transform.SetParent(rh, false);

            // Hand pose is already in world meters; do not scale offset by
            // worldScale (only scale the canvas mesh).
            _root.transform.localPosition = LocalPalmOffset;

            Transform hmdTf = EasyMateVrEulerPossessPoseCheck.ResolveHmdTransform(sc);
            if (hmdTf != null)
            {
                Vector3 hudWorld = _root.transform.position;
                Vector3 toHmd = hmdTf.position - hudWorld;
                if (toHmd.sqrMagnitude > 1e-10f)
                {
                    Vector3 f = toHmd.normalized;
                    Vector3 u = hmdTf.up;
                    float dotfu = Mathf.Abs(Vector3.Dot(f, u));
                    if (dotfu > 0.92f)
                    {
                        Vector3 side = Vector3.Cross(f, Vector3.up);
                        if (side.sqrMagnitude > 1e-8f)
                            u = side.normalized;
                        else
                            u = Vector3.Cross(Vector3.forward, f).normalized;
                    }
                    if (u.sqrMagnitude < 1e-8f)
                        u = Vector3.up;
                    _root.transform.rotation = Quaternion.LookRotation(f, u);
                    // World-space UI faces +Z; flip 180° so text reads from
                    // the HMD, not mirrored.
                    _root.transform.Rotate(0f, 180f, 0f, Space.Self);
                }
            }

            float uniform = 0.00036f * ws;
            _root.transform.localScale = new Vector3(uniform, uniform, uniform);

            SetVisible(true);

            bool possessed = EasyMateGripHandVisibility.IsAnyPersonHeadOrHandPossessed();
            if (_btnPossuir != null)
                _btnPossuir.interactable = !possessed;
            if (_btnDespossuir != null)
                _btnDespossuir.interactable = possessed;

            if (sc.GetMenuShow())
                MainUIButtons.RequestVrPalmHudMenuButtonPossessAfterDismissMenu();
        }

        internal static void OnPluginDestroy()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }

            _btnPossuir = null;
            _btnDespossuir = null;
            _listenersAttached = false;
            _whiteSprite = null;
        }

        private static void SetVisible(bool v)
        {
            if (_root != null)
                _root.SetActive(v);
        }

        private static Sprite WhiteSprite()
        {
            if (_whiteSprite != null)
                return _whiteSprite;
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _whiteSprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                100f);
            return _whiteSprite;
        }

        private static void AttachListenersOnce()
        {
            if (_listenersAttached)
                return;
            _listenersAttached = true;

            if (_btnPossuir != null)
            {
                _btnPossuir.onClick.AddListener(delegate
                {
                    MainUIButtons.RequestPossessClosestFemaleByVrHandHud();
                });
            }

            if (_btnDespossuir != null)
            {
                _btnDespossuir.onClick.AddListener(delegate
                {
                    MainUIButtons.RequestClearAllPossession(
                        "Easy Mate: Despossuir (VR mão).");
                });
            }
        }

        private static void EnsureHud()
        {
            if (_root != null)
                return;

            _root = new GameObject("EasyMateVrEulerPossessHandHud");

            Canvas canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 32000;
            _root.AddComponent<GraphicRaycaster>();

            RectTransform crt = _root.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(CanvasWidthPx, CanvasHeightPx);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.anchoredPosition = Vector2.zero;

            GameObject panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(_root.transform, false);
            Image panelImg = panelGo.AddComponent<Image>();
            panelImg.sprite = WhiteSprite();
            panelImg.color = new Color(0.08f, 0.08f, 0.1f, 0.82f);
            panelImg.raycastTarget = true;
            StretchFull(panelGo);

            _btnPossuir = CreateHandButton(
                _root.transform,
                "PossuirBtn",
                "Possuir",
                new Vector2(0.05f, 0.52f),
                new Vector2(0.95f, 0.98f),
                new Color(0.12f, 0.45f, 0.22f, 0.92f));

            _btnDespossuir = CreateHandButton(
                _root.transform,
                "DespossuirBtn",
                "Despossuir",
                new Vector2(0.05f, 0.02f),
                new Vector2(0.95f, 0.48f),
                new Color(0.5f, 0.14f, 0.14f, 0.92f));
        }

        private static void StretchFull(GameObject go)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null)
                rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Button CreateHandButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color bg)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            Image img = go.AddComponent<Image>();
            img.sprite = WhiteSprite();
            img.color = bg;
            img.raycastTarget = true;

            Button btn = go.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            cb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.55f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            RectTransform brt = go.GetComponent<RectTransform>();
            brt.anchorMin = anchorMin;
            brt.anchorMax = anchorMax;
            brt.offsetMin = new Vector2(4f, 3f);
            brt.offsetMax = new Vector2(-4f, -3f);

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            Text txt = textGo.AddComponent<Text>();
            Font font =
                Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            if (font != null)
                txt.font = font;
            txt.text = label;
            txt.fontSize = 22;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = false;
            StretchFull(textGo);

            return btn;
        }
    }
}
