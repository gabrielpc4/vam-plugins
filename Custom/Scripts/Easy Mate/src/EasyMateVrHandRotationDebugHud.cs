using System.Text;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// World-space <see cref="TextMesh"/> in front of
    /// <see cref="SuperController.lookCamera"/>, listing positions and
    /// rotations for VR hand transforms (for palm-gesture calibration).
    /// </summary>
    internal static class EasyMateVrHandRotationDebugHud
    {
        private static GameObject _root;

        private static TextMesh _textMesh;

        private static readonly StringBuilder Sb = new StringBuilder(3072);

        private const float ForwardOffsetM = 0.55f;

        private const float BelowCenterOffsetM = 0.07f;

        /// <summary>
        /// Call from <see cref="EasyMate.LateUpdate"/> with plugin toggle.
        /// </summary>
        public static void Tick(bool enabled)
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

            Sb.Length = 0;
            Sb.Append("EASY MATE VR HAND DEBUG\n");
            Sb.Append("OVR=");
            Sb.Append(sc.isOVR ? "1" : "0");
            Sb.Append(" OpenVR=");
            Sb.Append(sc.isOpenVR ? "1" : "0");
            Sb.Append(" ws=");
            Sb.Append(ws.ToString("F2"));
            Sb.Append("\nHMD pos ");
            AppendV3(Sb, ct.position);
            Sb.Append(" eulerW ");
            AppendV3(Sb, ct.eulerAngles);
            Sb.Append("\n");
            Sb.Append("HMD fwd ");
            AppendV3(Sb, ct.forward);
            Sb.Append(" up ");
            AppendV3(Sb, ct.up);
            Sb.Append("\n\n");

            AppendHandSection(Sb, "L leftHand", sc.leftHand, ct);
            Sb.Append("\n");
            AppendHandSection(Sb, "L touchObj", sc.touchObjectLeft, ct);
            Sb.Append("\n");
            AppendHandSection(Sb, "L viveObj", sc.viveObjectLeft, ct);
            Sb.Append("\n---\n");
            AppendHandSection(Sb, "R rightHand", sc.rightHand, ct);
            Sb.Append("\n");
            AppendHandSection(Sb, "R touchObj", sc.touchObjectRight, ct);
            Sb.Append("\n");
            AppendHandSection(Sb, "R viveObj", sc.viveObjectRight, ct);

            _textMesh.text = Sb.ToString();
        }

        public static void OnPluginDestroy()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }

            _textMesh = null;
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

            _root = new GameObject("EasyMateVrHandRotationDebugHud");
            _textMesh = _root.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            if (font != null)
                _textMesh.font = font;

            _textMesh.anchor = TextAnchor.UpperLeft;
            _textMesh.alignment = TextAlignment.Left;
            _textMesh.fontSize = 38;
            _textMesh.characterSize = 1f;
            _textMesh.lineSpacing = 0.9f;
            _textMesh.color = new Color(1f, 1f, 0.88f, 1f);
            _textMesh.richText = false;
        }

        private static void AppendV3(StringBuilder sb, Vector3 v)
        {
            sb.Append("(");
            sb.Append(v.x.ToString("F1"));
            sb.Append(",");
            sb.Append(v.y.ToString("F1"));
            sb.Append(",");
            sb.Append(v.z.ToString("F1"));
            sb.Append(")");
        }

        private static void AppendHandSection(
            StringBuilder sb,
            string title,
            Transform hand,
            Transform hmdTf)
        {
            sb.Append(title);
            sb.Append("\n");
            if (hand == null)
            {
                sb.Append("  (null)\n");
                return;
            }

            Vector3 hp = hand.position;
            Vector3 hmdPos = hmdTf.position;
            Vector3 toHand = hp - hmdPos;
            float dist = toHand.magnitude;

            Vector3 hmdFwd = hmdTf.forward;
            if (hmdFwd.sqrMagnitude < 1e-10f)
                hmdFwd = Vector3.forward;
            else
                hmdFwd.Normalize();

            float dotFwd = 0f;
            if (dist > 1e-5f)
            {
                Vector3 dir = toHand * (1f / dist);
                dotFwd = Vector3.Dot(hmdFwd, dir);
            }

            Vector3 towardCam = hmdPos - hp;
            float tcMag = towardCam.magnitude;
            float palmDot = -1f;
            if (tcMag > 1e-5f)
            {
                towardCam = towardCam * (1f / tcMag);
                palmDot = BestLocalAxisDotToward(hand, towardCam);
            }

            Quaternion rel =
                Quaternion.Inverse(hmdTf.rotation) * hand.rotation;

            sb.Append("  pos ");
            AppendV3(sb, hp);
            sb.Append(" dist ");
            sb.Append(dist.ToString("F2"));
            sb.Append(" dotFwd=");
            sb.Append(dotFwd.ToString("F2"));
            sb.Append(" palmDot=");
            sb.Append(palmDot.ToString("F2"));
            sb.Append("\n  eulerW ");
            AppendV3(sb, hand.eulerAngles);
            sb.Append(" eulerRelHMD ");
            AppendV3(sb, rel.eulerAngles);
            sb.Append("\n  eulerL ");
            AppendV3(sb, hand.localEulerAngles);
            sb.Append("\n  fwd ");
            AppendV3(sb, hand.forward);
            sb.Append(" up ");
            AppendV3(sb, hand.up);
            sb.Append(" right ");
            AppendV3(sb, hand.right);
            Quaternion q = hand.rotation;
            sb.Append("\n  quat xyzw ");
            sb.Append(q.x.ToString("F3"));
            sb.Append(" ");
            sb.Append(q.y.ToString("F3"));
            sb.Append(" ");
            sb.Append(q.z.ToString("F3"));
            sb.Append(" ");
            sb.Append(q.w.ToString("F3"));
            sb.Append("\n");
        }

        /// <summary>
        /// Same idea as palm gesture: best dot among ±local axes toward cam.
        /// </summary>
        private static float BestLocalAxisDotToward(
            Transform hand,
            Vector3 unitTowardCam)
        {
            if (hand == null || unitTowardCam.sqrMagnitude < 1e-6f)
                return -1f;

            float best = -1f;
            float d;

            d = Vector3.Dot(hand.forward, unitTowardCam);
            if (d > best)
                best = d;
            d = Vector3.Dot(-hand.forward, unitTowardCam);
            if (d > best)
                best = d;
            d = Vector3.Dot(hand.up, unitTowardCam);
            if (d > best)
                best = d;
            d = Vector3.Dot(-hand.up, unitTowardCam);
            if (d > best)
                best = d;
            d = Vector3.Dot(hand.right, unitTowardCam);
            if (d > best)
                best = d;
            d = Vector3.Dot(-hand.right, unitTowardCam);
            if (d > best)
                best = d;

            return best;
        }
    }
}
