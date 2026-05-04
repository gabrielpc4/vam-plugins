using System.Text;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// HMD-facing <see cref="TextMesh"/> plus a cube: green when both hands
    /// satisfy the VR euler possess angle window (ignores dwell/cooldown).
    /// </summary>
    internal static class EasyMateVrEulerGestureTestHud
    {
        private static GameObject _root;

        private static TextMesh _textMesh;

        private static MeshRenderer _cubeRenderer;

        private static Material _cubeMaterial;

        private static readonly StringBuilder Sb = new StringBuilder(1024);

        private static readonly Color ColorOk = new Color(0.15f, 0.95f, 0.2f, 1f);

        private static readonly Color ColorBad = new Color(0.95f, 0.22f, 0.2f, 1f);

        private const float ForwardOffsetM = 0.52f;

        private const float BelowCenterOffsetM = 0.07f;

        /// <summary>
        /// Call from <see cref="EasyMate.LateUpdate"/> with plugin toggle.
        /// </summary>
        internal static void Tick(bool enabled)
        {
            SuperController sc = SuperController.singleton;
            if (!enabled || sc == null || sc.isLoading)
            {
                SetVisible(false);
                return;
            }

            if (sc.lookCamera == null)
            {
                SetVisible(false);
                return;
            }

            EnsureHud();
            SetVisible(true);

            Transform ct = sc.lookCamera.transform;
            float ws = sc.worldScale;
            if (ws < 0.01f)
                ws = 0.01f;

            Vector3 forward = ct.forward;
            if (forward.sqrMagnitude < 1e-10f)
                forward = Vector3.forward;
            else
                forward.Normalize();

            Vector3 up = ct.up;
            if (up.sqrMagnitude < 1e-10f)
                up = Vector3.up;
            else
                up.Normalize();

            Vector3 hudPos =
                ct.position + forward * (ForwardOffsetM * ws) -
                up * (BelowCenterOffsetM * ws);
            _root.transform.position = hudPos;
            _root.transform.rotation = Quaternion.LookRotation(
                ct.position - hudPos,
                up);
            _root.transform.Rotate(0f, 180f, 0f);

            float charScale = 0.0014f * ws;
            _root.transform.localScale = new Vector3(
                charScale,
                charScale,
                charScale);

            Vector3 leftEuler;
            Vector3 rightEuler;
            bool leftOk;
            bool rightOk;
            bool bothOk = EasyMateVrEulerPossessPoseCheck.BothHandsMatchTriggerWindow(
                sc,
                out leftEuler,
                out rightEuler,
                out leftOk,
                out rightOk);

            if (_cubeMaterial != null)
                _cubeMaterial.color = bothOk ? ColorOk : ColorBad;

            BuildLines(Sb, leftEuler, rightEuler, leftOk, rightOk, bothOk);
            _textMesh.text = Sb.ToString();
        }

        internal static void OnPluginDestroy()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }

            _textMesh = null;
            _cubeRenderer = null;
            _cubeMaterial = null;
        }

        private static void SetVisible(bool v)
        {
            if (_root == null)
                return;
            _root.SetActive(v);
        }

        private static void EnsureHud()
        {
            if (_root != null)
                return;

            _root = new GameObject("EasyMateVrEulerGestureTestHud");
            _textMesh = _root.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            if (font != null)
                _textMesh.font = font;

            _textMesh.anchor = TextAnchor.UpperLeft;
            _textMesh.alignment = TextAlignment.Left;
            _textMesh.fontSize = 34;
            _textMesh.characterSize = 1f;
            _textMesh.lineSpacing = 0.88f;
            _textMesh.color = new Color(0.92f, 1f, 0.95f, 1f);
            _textMesh.richText = false;

            GameObject cubeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeGo.name = "TriggerWindowCube";
            cubeGo.transform.SetParent(_root.transform, false);
            cubeGo.transform.localPosition = new Vector3(500f, -40f, 0f);
            cubeGo.transform.localRotation = Quaternion.identity;
            cubeGo.transform.localScale = new Vector3(72f, 72f, 72f);

            Collider col = cubeGo.GetComponent<Collider>();
            if (col != null)
                UnityEngine.Object.Destroy(col);

            _cubeRenderer = cubeGo.GetComponent<MeshRenderer>();
            if (_cubeRenderer != null)
            {
                Shader sh = Shader.Find("Unlit/Color");
                if (sh != null)
                {
                    _cubeMaterial = new Material(sh);
                    _cubeMaterial.color = ColorBad;
                    _cubeRenderer.material = _cubeMaterial;
                }

                _cubeRenderer.shadowCastingMode =
                    UnityEngine.Rendering.ShadowCastingMode.Off;
                _cubeRenderer.receiveShadows = false;
            }
        }

        private static void BuildLines(
            StringBuilder sb,
            Vector3 leftEuler,
            Vector3 rightEuler,
            bool leftOk,
            bool rightOk,
            bool bothOk)
        {
            sb.Length = 0;
            sb.Append("EASY MATE VR EULER POSSESS (angle test)\n");
            sb.Append("Cube = trigger window only (not dwell/cooldown)\n");
            sb.Append("L eulerRelHMD ");
            sb.Append(EasyMateVrEulerPossessPoseCheck.FormatEuler(leftEuler));
            sb.Append(leftOk ? " OK\n" : " --\n");
            sb.Append("  need X>");
            sb.Append(EasyMateVrEulerPossessPoseCheck.LeftMinEulerX.ToString("F0"));
            sb.Append(" ");
            sb.Append(EasyMateVrEulerPossessPoseCheck.LeftMinEulerZ.ToString("F0"));
            sb.Append("<Z<");
            sb.Append(EasyMateVrEulerPossessPoseCheck.LeftMaxEulerZ.ToString("F0"));
            sb.Append("\n");
            sb.Append("R eulerRelHMD ");
            sb.Append(EasyMateVrEulerPossessPoseCheck.FormatEuler(rightEuler));
            sb.Append(rightOk ? " OK\n" : " --\n");
            sb.Append("  need X>");
            sb.Append(EasyMateVrEulerPossessPoseCheck.RightMinEulerX.ToString("F0"));
            sb.Append(" ");
            sb.Append(EasyMateVrEulerPossessPoseCheck.RightMinEulerZ.ToString("F0"));
            sb.Append("<Z<");
            sb.Append(EasyMateVrEulerPossessPoseCheck.RightMaxEulerZ.ToString("F0"));
            sb.Append("\n");
            sb.Append("BOTH: ");
            sb.Append(bothOk ? "OK (cube green)" : "no (cube red)");
        }
    }
}
