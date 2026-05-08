using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace geesp0t
{
    /// <summary>
    /// Separate session plugin: Copy Errors / Copy Console / Clear logs, plus when
    /// VaM adds new error-lines (SuperController.errorCount rises), brings main HUD
    /// to headset in VR and expands the Error Log panel so faults are readable in-HMD.
    /// Own .cslist so controls still load if GabrielHud.cslist fails; layout column 0
    /// matches GabrielHud columns 1–3 on mainHUD.
    /// </summary>
    public class VaMLogClipboardHud : MVRScript
    {
        private Canvas _canvas;

        private bool _isDesktopMode;

        /// <summary>Previous SuperController.errorCount; syncs ClearErrors dips.</summary>
        private int _lastSeenErrorCount;

        /// <summary>False until baseline count captured (avoids spurious HUD pop).</summary>
        private bool _baselineErrorSeen;

        public override void Init()
        {
        }

        private void Start()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc != null)
                {
                    _lastSeenErrorCount = sc.errorCount;
                    _baselineErrorSeen = true;
                    _isDesktopMode = !(sc.isOVR || sc.isOpenVR);
                }
                CreateHud();
            }
            catch (Exception e)
            {
                SuperController.LogError("VaMLogClipboardHud Start: " + e);
            }
        }

        private void LateUpdate()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || !_baselineErrorSeen)
                return;

            int c = sc.errorCount;
            if (c > _lastSeenErrorCount)
                BringVaMErrorHudToView(sc);

            _lastSeenErrorCount = c;
        }

        /// <summary>
        /// Main HUD follows headset anchors (skip monitor mirror rig in VR); open Error
        /// Log drawer and Panel child match ErrorLogToggle / LFE command behavior.
        /// </summary>
        private static void BringVaMErrorHudToView(SuperController sc)
        {
            if (sc == null)
                return;
            if (!sc.IsMonitorOnly)
                sc.ShowMainHUD(setAnchors: true, forceMonitor: false);
            sc.OpenErrorLogPanel();
            if (sc.errorLogPanel != null)
            {
                Transform subPanel = sc.errorLogPanel.Find("Panel");
                if (subPanel != null)
                    subPanel.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            try
            {
                DestroyHud();
            }
            catch (Exception e)
            {
                SuperController.LogError("VaMLogClipboardHud OnDestroy: " + e);
            }
        }

        private void CreateHud()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || manager == null)
                return;

            DestroyHud();

            float worldScale = sc.worldScale;
            sc.worldScale = 1.0f;

            GameObject canvasObject = new GameObject();
            _canvas = canvasObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.pixelPerfect = false;
            sc.AddCanvas(_canvas);

            _canvas.transform.SetParent(sc.mainHUD, false);

            CanvasScaler cs = canvasObject.AddComponent<CanvasScaler>();
            cs.scaleFactor = 80.0f;
            cs.dynamicPixelsPerUnit = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();

            float scale = 0.001f;
            _canvas.transform.localScale = new Vector3(scale, scale, scale);
            _canvas.transform.localPosition = new Vector3(-0.45f, -0.72f, 0.35f);
            LookAtCamera();

            const float logColButtonWidth = 118f;
            AddHudButton("Copy Errors", OnCopyErrorLogClicked, 0, 0, logColButtonWidth);
            AddHudButton("Copy Console", OnCopyMessageLogClicked, 0, 1, logColButtonWidth);
            AddHudButton("Clear logs", OnClearLogsClicked, 0, 2, logColButtonWidth);

            _canvas.transform.Translate(0, 0.2f, 0);
            sc.worldScale = worldScale;
        }

        private void LookAtCamera()
        {
            SuperController sc = SuperController.singleton;
            if (_canvas == null || sc == null)
                return;
            if (_isDesktopMode)
            {
                _canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
                return;
            }

            if (sc.isOVR || sc.isOpenVR)
            {
                _canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
                return;
            }

            if (sc.lookCamera != null)
            {
                Transform cameraT = sc.lookCamera.transform;
                Vector3 endPos = cameraT.position + cameraT.forward * 10000000.0f;
                _canvas.transform.LookAt(endPos, cameraT.up);
            }
            else
            {
                _canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
            }
        }

        private void AddHudButton(string name, UnityAction callback, int column, int row, float width)
        {
            Color accessButtonColor = new Color(0.8392f, 0.8392f, 0.8392f);
            Color accessTextColor = new Color(0, 0, 0);
            float xSpacing = 0.22f;
            float ySpacing = 0.05f;

            UIDynamicButton button = CreateHudButton(name, width, 40);
            button.button.onClick.AddListener(callback);
            button.transform.Translate(column * xSpacing, 0.50f - row * ySpacing, 0, Space.Self);
            button.textColor = accessTextColor;
            button.buttonColor = accessButtonColor;
        }

        private UIDynamicButton CreateHudButton(string name, float width, float height)
        {
            Transform button = GameObject.Instantiate<Transform>(manager.configurableButtonPrefab);
            button.transform.position = Vector3.zero;
            RectTransform rt = button.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(width / 2, height / 2);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            button.SetParent(_canvas.transform, false);
            UIDynamicButton uiButton = button.GetComponent<UIDynamicButton>();
            uiButton.label = name;
            uiButton.buttonText.fontSize = 18;
            return uiButton;
        }

        private void DestroyHud()
        {
            if (_canvas == null)
                return;
            SuperController sc = SuperController.singleton;
            if (sc != null)
                sc.RemoveCanvas(_canvas);
            _canvas.transform.SetParent(null, false);
            if (_canvas.gameObject != null)
                UnityEngine.Object.Destroy(_canvas.gameObject);
            _canvas = null;
        }

        private static string GetVaMErrorLogText(SuperController sc)
        {
            if (sc == null)
                return string.Empty;
            if (sc.allErrorsText != null && sc.allErrorsText.text != null)
                return sc.allErrorsText.text;
            if (sc.allErrorsText2 != null && sc.allErrorsText2.text != null)
                return sc.allErrorsText2.text;
            return string.Empty;
        }

        private static string GetVaMMessageLogText(SuperController sc)
        {
            if (sc == null)
                return string.Empty;
            if (sc.allMessagesText != null && sc.allMessagesText.text != null)
                return sc.allMessagesText.text;
            if (sc.allMessagesText2 != null && sc.allMessagesText2.text != null)
                return sc.allMessagesText2.text;
            return string.Empty;
        }

        private void OnCopyErrorLogClicked()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                string t = GetVaMErrorLogText(sc);
                GUIUtility.systemCopyBuffer = t != null ? t : string.Empty;
                if (t == null || t.Length == 0)
                    SuperController.LogMessage(
                        "VaMLogClipboardHud: copied error log (empty).");
                else
                    SuperController.LogMessage(
                        "VaMLogClipboardHud: copied error log (" + t.Length
                        + " chars).");
            }
            catch (Exception e)
            {
                SuperController.LogError("VaMLogClipboardHud: copy error log: " + e);
            }
        }

        private void OnCopyMessageLogClicked()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                string t = GetVaMMessageLogText(sc);
                GUIUtility.systemCopyBuffer = t != null ? t : string.Empty;
                if (t == null || t.Length == 0)
                    SuperController.LogMessage(
                        "VaMLogClipboardHud: copied console log (empty).");
                else
                    SuperController.LogMessage(
                        "VaMLogClipboardHud: copied console log (" + t.Length
                        + " chars).");
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "VaMLogClipboardHud: copy console log: " + e);
            }
        }

        private void OnClearLogsClicked()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return;
                sc.ClearErrors();
                sc.ClearMessages();
                SuperController.LogMessage(
                    "VaMLogClipboardHud: cleared error and message logs.");
            }
            catch (Exception e)
            {
                SuperController.LogError("VaMLogClipboardHud: clear logs: " + e);
            }
        }
    }
}
