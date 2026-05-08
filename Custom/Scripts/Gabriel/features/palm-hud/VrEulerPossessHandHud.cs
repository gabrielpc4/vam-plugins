using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// When the <b>right hand alone</b> matches the HMD-relative euler window
    /// (back of hand toward HMD), shows a small world UI on
    /// <c>rightHand</c>: <b>Próxima cena</b> on the upper row when available,
    /// and <b>Despossuir</b> on the lower row only while already in possession
    /// (VaM passenger or head/hand possessed). Passenger <b>start</b> does not use
    /// this HUD — aim lasers + face <b>A</b> (<see cref="PassengerLaserPossess"/>).
    /// Face <b>B</b> / OpenVR menu runs next scene; face <b>A</b> / Select runs
    /// <see cref="PassengerRuntime.RequestStopForPalmHud"/> while the Despossuir row is
    /// visible. Buttons are polled; VaM lasers often miss the canvas.
    /// </summary>
    internal static class VrEulerPossessHandHud
    {
        private static GameObject _root;

        private static Button _btnPossessRow;

        private static Image _possessRowImage;

        private static Text _possessRowText;

        private static Button _btnProximaCena;

        private static bool _listenersAttached;

        private static Sprite _whiteSprite;

        private static bool _cachedSceneHasNextSceneUIButton;

        private static float _nextSceneButtonPresenceRecheckTime;

        private const float NextSceneButtonPresenceRecheckSeconds = 1f;

        private static readonly Color PossessRowDespossuirColor =
            new Color(0.5f, 0.14f, 0.14f, 0.92f);

        private static readonly Vector3 LocalHandHudOffset =
            new Vector3(0.065f, 0.012f, 0.025f);

        private const float CanvasWidthPx = 260f;

        private const float CanvasHeightPx = 140f;

        internal static void Tick()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
            {
                _nextSceneButtonPresenceRecheckTime = 0f;
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

            if (PassengerRuntime.IsPalmHandHudBlockedAfterPassengerHandsTrigger())
            {
                SetVisible(false);
                return;
            }

            if (!VrEulerPossessPoseCheck.RightHandOnlyMatchTriggerWindow(sc))
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

            _root.transform.localPosition = LocalHandHudOffset;

            Transform hmdTf = VrEulerPossessPoseCheck.ResolveHmdTransform(sc);
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
                    _root.transform.Rotate(0f, 180f, 0f, Space.Self);
                }
            }

            float uniform = 0.00036f * ws;
            _root.transform.localScale = new Vector3(uniform, uniform, uniform);

            SetVisible(true);

            float nowUnscaled = Time.unscaledTime;
            if (_nextSceneButtonPresenceRecheckTime <= 0f ||
                nowUnscaled >= _nextSceneButtonPresenceRecheckTime)
            {
                _nextSceneButtonPresenceRecheckTime =
                    nowUnscaled + NextSceneButtonPresenceRecheckSeconds;
                _cachedSceneHasNextSceneUIButton =
                    GabrielHudButtons.HasNextSceneUiButtonInScene();
            }

            bool possessed =
                PassengerRuntime.IsPassengerModeActiveOrPending() ||
                GripHandVisibility.IsAnyPersonHeadOrHandPossessed();

            RefreshPanels(possessed);

            bool proximaCenaAvailable = _cachedSceneHasNextSceneUIButton;

            if (_btnPossessRow != null)
                _btnPossessRow.interactable = possessed;
            if (_btnProximaCena != null)
                _btnProximaCena.interactable = true;

            if (!possessed)
            {
                if (proximaCenaAvailable &&
                    VrInput.PollPalmHudProximaCenaFaceBDown(sc))
                {
                    GabrielHudButtons.RequestFireNextSceneAfterClosingMenu();
                }
            }
            else
            {
                bool possessA = VrInput.PollPalmHudPossessRowFaceADown(sc);
                bool proximaB = VrInput.PollPalmHudProximaCenaFaceBDown(sc);

                if (proximaCenaAvailable && proximaB)
                {
                    GabrielHudButtons.RequestFireNextSceneAfterClosingMenu();
                }
                else if (possessA)
                {
                    PassengerRuntime.RequestStopForPalmHud();
                }
            }
        }

        internal static bool IsVisible()
        {
            return _root != null && _root.activeSelf;
        }

        internal static void OnPluginDestroy()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }

            _btnPossessRow = null;
            _possessRowImage = null;
            _possessRowText = null;
            _btnProximaCena = null;
            _listenersAttached = false;
            _whiteSprite = null;
            _cachedSceneHasNextSceneUIButton = false;
            _nextSceneButtonPresenceRecheckTime = 0f;
        }

        private static void SetVisible(bool v)
        {
            if (_root != null)
                _root.SetActive(v);
        }

        private static void RefreshPanels(bool possessed)
        {
            bool showProximaRow = _cachedSceneHasNextSceneUIButton;
            if (_btnPossessRow != null)
                _btnPossessRow.gameObject.SetActive(possessed);
            if (_btnProximaCena != null)
                _btnProximaCena.gameObject.SetActive(showProximaRow);

            if (!possessed || _possessRowText == null)
                return;
            _possessRowText.text = "Despossuir (A)";
            if (_possessRowImage != null)
                _possessRowImage.color = PossessRowDespossuirColor;
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

            if (_btnPossessRow != null)
            {
                _btnPossessRow.onClick.AddListener(delegate
                {
                    PassengerRuntime.RequestStopForPalmHud();
                });
            }

            if (_btnProximaCena != null)
            {
                _btnProximaCena.onClick.AddListener(delegate
                {
                    GabrielHudButtons.RequestFireNextSceneAfterClosingMenu();
                });
            }
        }

        private static void EnsureHud()
        {
            if (_root != null)
                return;

            _root = new GameObject("VrEulerPossessHandHud");

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
            panelImg.raycastTarget = false;
            StretchFull(panelGo);

            GameObject possessGo = new GameObject("DespossuirRowBtn");
            possessGo.transform.SetParent(_root.transform, false);
            _possessRowImage = possessGo.AddComponent<Image>();
            _possessRowImage.sprite = WhiteSprite();
            _possessRowImage.color = PossessRowDespossuirColor;
            _possessRowImage.raycastTarget = true;
            _btnPossessRow = possessGo.AddComponent<Button>();
            ColorBlock cb = _btnPossessRow.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            cb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.55f);
            cb.colorMultiplier = 1f;
            _btnPossessRow.colors = cb;
            RectTransform pbrt = possessGo.GetComponent<RectTransform>();
            pbrt.anchorMin = new Vector2(0.05f, 0.02f);
            pbrt.anchorMax = new Vector2(0.95f, 0.48f);
            pbrt.offsetMin = new Vector2(4f, 3f);
            pbrt.offsetMax = new Vector2(-4f, -3f);
            GameObject possessTextGo = new GameObject("Text");
            possessTextGo.transform.SetParent(possessGo.transform, false);
            _possessRowText = possessTextGo.AddComponent<Text>();
            Font font =
                Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            if (font != null)
                _possessRowText.font = font;
            _possessRowText.text = "Despossuir (A)";
            _possessRowText.fontSize = 22;
            _possessRowText.fontStyle = FontStyle.Bold;
            _possessRowText.alignment = TextAnchor.MiddleCenter;
            _possessRowText.color = Color.white;
            _possessRowText.raycastTarget = false;
            StretchFull(possessTextGo);

            _btnPossessRow.gameObject.SetActive(false);

            _btnProximaCena = CreateHandButton(
                _root.transform,
                "ProximaCenaBtn",
                "Próxima cena (B)",
                new Vector2(0.05f, 0.52f),
                new Vector2(0.95f, 0.98f),
                new Color(0.14f, 0.32f, 0.52f, 0.92f),
                20);
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
            Color bg,
            int fontSize)
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
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = false;
            StretchFull(textGo);

            return btn;
        }
    }
}
