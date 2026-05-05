using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace geesp0t
{
    /// <summary>
    /// When the <b>right hand alone</b> matches the HMD-relative euler
    /// window for the VR possess rule, shows a small world UI on
    /// <c>rightHand</c> (back of hand / watch pose): <b>Próxima cena</b> on the
    /// <b>upper</b> row and
    /// <b>Possuir</b> / <b>Despossuir</b> on the <b>lower</b> row.
    /// <b>Upper</b> row (<b>Próxima cena</b>): only when the scene has a resolvable
    /// &quot;next&quot; <c>UIButton</c> (same rules as <see cref="MainUIButtons.HasNextSceneUiButtonInScene"/>);
    /// face <b>B</b> / OpenVR menu, or tap the button.
    /// <b>Lower</b> row (<b>Possuir</b> / <b>Despossuir</b>): face <b>A</b> / OpenVR select.
    /// While possessed, <b>A</b> triggers Despossuir; <b>B</b> still runs
    /// <see cref="MainUIButtons.RequestFireNextSceneUiButton"/> (same as when not possessed). Unity often
    /// sends no laser hits to this hand canvas, so face buttons are polled.
    /// The whole HUD, including this row, only shows while the right-hand
    /// euler window matches.
    /// <b>Gender step:</b> <b>Homem (B)</b> upper row, <b>Mulher (A)</b> lower row.
    /// VaM menu shortcut still dismisses the main UI into this step when a Person exists.
    /// Leaving the pose closes the HUD (no third “back” button). Billboard faces the HMD.
    /// </summary>
    internal static class EasyMateVrEulerPossessHandHud
    {
        private static GameObject _root;

        private static Button _btnPossessRow;

        private static Image _possessRowImage;

        private static Text _possessRowText;

        private static Button _btnProximaCena;

        private static Button _btnMulher;

        private static Button _btnHomem;

        private static bool _genderChooseStepActive;

        private static bool _listenersAttached;

        private static Sprite _whiteSprite;

        private static bool _cachedSceneHasNextSceneUIButton;

        private static float _nextSceneButtonPresenceRecheckTime;

        private const float NextSceneButtonPresenceRecheckSeconds = 0.35f;

        private static readonly Color PossessRowPossuirColor =
            new Color(0.12f, 0.45f, 0.22f, 0.92f);

        private static readonly Color PossessRowDespossuirColor =
            new Color(0.5f, 0.14f, 0.14f, 0.92f);

        /// <summary>
        /// Local offset on <c>rightHand</c> (meters in hand space). Positive
        /// X is the dorsum / watch side on OpenVR / OVR right controllers
        /// (palm-facing HUD used negative X with the same Y,Z magnitudes).
        /// </summary>
        private static readonly Vector3 LocalHandHudOffset =
            new Vector3(0.065f, 0.012f, 0.025f);

        private const float CanvasWidthPx = 260f;

        private const float CanvasHeightPx = 140f;

        /// <summary>
        /// Opens <b>Mulher</b>/<b>Homem</b> on the next palm HUD tick (for
        /// menu shortcuts and <see cref="MainUIButtons"/> helpers).
        /// </summary>
        internal static void RequestGenderChooseStep()
        {
            _genderChooseStepActive = true;
        }

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

            if (EasyMateFemalePassengerRuntime.IsPalmHandHudBlockedAfterPassengerHandsTrigger())
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

            _root.transform.localPosition = LocalHandHudOffset;

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
                    MainUIButtons.HasNextSceneUiButtonInScene();
            }

            bool possessed =
                EasyMateFemalePassengerRuntime.IsFemalePassengerModeActiveOrPending() ||
                EasyMateGripHandVisibility.IsAnyPersonHeadOrHandPossessed();
            if (possessed)
                _genderChooseStepActive = false;

            RefreshGenderVersusMainRows(possessed);

            bool onGenderChoosePanel = _genderChooseStepActive && !possessed;
            bool proximaCenaAvailable =
                _cachedSceneHasNextSceneUIButton && !onGenderChoosePanel;

            if (_btnPossessRow != null)
                _btnPossessRow.interactable = true;
            if (_btnProximaCena != null)
                _btnProximaCena.interactable = true;
            if (_btnMulher != null)
                _btnMulher.interactable = true;
            if (_btnHomem != null)
                _btnHomem.interactable = true;

            if (onGenderChoosePanel)
            {
                bool mulherA = EasyMateVrInput.PollPalmHudMulherChoiceDown(sc);
                bool homemB = EasyMateVrInput.PollPalmHudHomemChoiceDown(sc);
                if (mulherA)
                {
                    InvokeGenderMulherChoice();
                }
                else if (homemB)
                {
                    InvokeGenderHomemChoice();
                }
            }
            else if (possessed)
            {
                bool possessA = EasyMateVrInput.PollPalmHudPossessRowFaceADown(sc);
                bool proximaB = EasyMateVrInput.PollPalmHudProximaCenaFaceBDown(sc);

                if (proximaCenaAvailable && proximaB)
                {
                    MainUIButtons.RequestFireNextSceneUiButton();
                    DismissVaMOverlayUiIfAny();
                }
                else if (possessA)
                {
                    InvokePossessRowPrimaryAction();
                }
            }
            else
            {
                if (EasyMateVrInput.PollPalmHudPossessRowFaceADown(sc))
                {
                    InvokePossessRowPrimaryAction();
                }
                else if (proximaCenaAvailable &&
                    EasyMateVrInput.PollPalmHudProximaCenaFaceBDown(sc))
                {
                    MainUIButtons.RequestFireNextSceneUiButton();
                    DismissVaMOverlayUiIfAny();
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
            _btnMulher = null;
            _btnHomem = null;
            _genderChooseStepActive = false;
            _listenersAttached = false;
            _whiteSprite = null;
            _cachedSceneHasNextSceneUIButton = false;
            _nextSceneButtonPresenceRecheckTime = 0f;
        }

        private static void SetVisible(bool v)
        {
            if (_root != null)
                _root.SetActive(v);
            if (!v)
                _genderChooseStepActive = false;
        }

        private static void RefreshGenderVersusMainRows(bool possessed)
        {
            bool gender = _genderChooseStepActive && !possessed;
            bool showProximaCenaRow = !gender && _cachedSceneHasNextSceneUIButton;
            if (_btnMulher != null)
                _btnMulher.gameObject.SetActive(gender);
            if (_btnHomem != null)
                _btnHomem.gameObject.SetActive(gender);
            if (_btnPossessRow != null)
                _btnPossessRow.gameObject.SetActive(!gender);
            if (_btnProximaCena != null)
                _btnProximaCena.gameObject.SetActive(showProximaCenaRow);

            if (!gender)
                ApplyPossessRowAnchorsForProximaVisible(showProximaCenaRow);

            if (gender || _possessRowText == null)
                return;
            _possessRowText.text = possessed ?
                "Despossuir (A)" :
                "Possuir (A)";
            if (_possessRowImage != null)
            {
                _possessRowImage.color = possessed ?
                    PossessRowDespossuirColor :
                    PossessRowPossuirColor;
            }
        }

        /// <summary>
        /// When the upper <b>Próxima cena</b> row is hidden, stretch the possess row over the full HUD height.
        /// </summary>
        private static void ApplyPossessRowAnchorsForProximaVisible(bool proximaRowVisible)
        {
            if (_btnPossessRow == null)
                return;
            RectTransform rowRt = _btnPossessRow.GetComponent<RectTransform>();
            if (rowRt == null)
                return;
            if (proximaRowVisible)
            {
                rowRt.anchorMin = new Vector2(0.05f, 0.02f);
                rowRt.anchorMax = new Vector2(0.95f, 0.48f);
            }
            else
            {
                rowRt.anchorMin = new Vector2(0.05f, 0.02f);
                rowRt.anchorMax = new Vector2(0.95f, 0.98f);
            }
            rowRt.offsetMin = new Vector2(4f, 3f);
            rowRt.offsetMax = new Vector2(-4f, -3f);
        }

        private static void InvokeGenderMulherChoice()
        {
            DismissVaMOverlayUiIfAny();
            MainUIButtons.RequestPossessVrPalmHudByGender(true);
            _genderChooseStepActive = false;
            RefreshGenderVersusMainRows(
                EasyMateFemalePassengerRuntime.IsFemalePassengerModeActiveOrPending() ||
                EasyMateGripHandVisibility.IsAnyPersonHeadOrHandPossessed());
            DismissVaMOverlayUiIfAny();
        }

        private static void InvokeGenderHomemChoice()
        {
            DismissVaMOverlayUiIfAny();
            SuperController.LogMessage(
                "Easy Mate: fluxo masculino desativado.");
            _genderChooseStepActive = false;
            RefreshGenderVersusMainRows(
                EasyMateFemalePassengerRuntime.IsFemalePassengerModeActiveOrPending() ||
                EasyMateGripHandVisibility.IsAnyPersonHeadOrHandPossessed());
            DismissVaMOverlayUiIfAny();
        }

        private static void DismissVaMOverlayUiIfAny()
        {
            SuperController sc = SuperController.singleton;
            if (sc != null)
                sc.activeUI = SuperController.ActiveUI.None;
        }

        private static void InvokePossessRowPrimaryAction()
        {
            if (EasyMateFemalePassengerRuntime.IsFemalePassengerModeActiveOrPending() ||
                EasyMateGripHandVisibility.IsAnyPersonHeadOrHandPossessed())
            {
                EasyMateFemalePassengerRuntime.RequestStopForPalmHud();
            }
            else if (MainUIButtons.VrPalmHudNeedsGenderChoiceStep())
            {
                DismissVaMOverlayUiIfAny();
                RequestGenderChooseStep();
                RefreshGenderVersusMainRows(false);
            }
            else
            {
                MainUIButtons.RequestPossessVrPalmHudAutoWithoutGenderMenu();
            }
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
                    InvokePossessRowPrimaryAction();
                });
            }

            if (_btnMulher != null)
            {
                _btnMulher.onClick.AddListener(delegate
                {
                    InvokeGenderMulherChoice();
                });
            }

            if (_btnHomem != null)
            {
                _btnHomem.onClick.AddListener(delegate
                {
                    InvokeGenderHomemChoice();
                });
            }

            if (_btnProximaCena != null)
            {
                _btnProximaCena.onClick.AddListener(delegate
                {
                    MainUIButtons.RequestFireNextSceneUiButton();
                    DismissVaMOverlayUiIfAny();
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
            panelImg.raycastTarget = false;
            StretchFull(panelGo);

            GameObject possessGo = new GameObject("PossessRowBtn");
            possessGo.transform.SetParent(_root.transform, false);
            _possessRowImage = possessGo.AddComponent<Image>();
            _possessRowImage.sprite = WhiteSprite();
            _possessRowImage.color = PossessRowPossuirColor;
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
            _possessRowText.text = "Possuir (A)";
            _possessRowText.fontSize = 22;
            _possessRowText.fontStyle = FontStyle.Bold;
            _possessRowText.alignment = TextAnchor.MiddleCenter;
            _possessRowText.color = Color.white;
            _possessRowText.raycastTarget = false;
            StretchFull(possessTextGo);

            _btnProximaCena = CreateHandButton(
                _root.transform,
                "ProximaCenaBtn",
                "Próxima cena (B)",
                new Vector2(0.05f, 0.52f),
                new Vector2(0.95f, 0.98f),
                new Color(0.14f, 0.32f, 0.52f, 0.92f),
                20);

            _btnHomem = CreateHandButton(
                _root.transform,
                "HomemBtn",
                "Homem (B)",
                new Vector2(0.05f, 0.52f),
                new Vector2(0.95f, 0.98f),
                new Color(0.14f, 0.32f, 0.52f, 0.92f),
                20);
            _btnHomem.gameObject.SetActive(false);

            _btnMulher = CreateHandButton(
                _root.transform,
                "MulherBtn",
                "Mulher (A)",
                new Vector2(0.05f, 0.02f),
                new Vector2(0.95f, 0.48f),
                PossessRowPossuirColor,
                20);
            _btnMulher.gameObject.SetActive(false);
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
