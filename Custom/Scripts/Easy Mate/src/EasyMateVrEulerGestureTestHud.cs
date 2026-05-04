using System.Collections;
using System.Text;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// World-fixed <b>Cube</b> + <b>UIText</b> atoms near the origin: live
    /// HMD-relative euler readout and cube diffuse (MaterialOptions color1)
    /// green/red when the euler possess angle window matches (no dwell/cooldown).
    /// </summary>
    internal static class EasyMateVrEulerGestureTestHud
    {
        private const string AtomUidCube = "EasyMateVrEulerTestCube";

        private const string AtomUidUiText = "EasyMateVrEulerTestUIText";

        private const string StorableMaterials = "materials";

        private const string StorableText = "Text";

        private const string StorableCanvas = "Canvas";

        private static Atom _cubeAtom;

        private static Atom _textAtom;

        private static bool _atomsReady;

        private static bool _spawnInProgress;

        private static bool _lastTickWantedVisible;

        private static readonly StringBuilder Sb = new StringBuilder(1024);

        private static readonly Color ColorOk = new Color(0.15f, 0.95f, 0.2f, 1f);

        private static readonly Color ColorBad = new Color(0.95f, 0.22f, 0.2f, 1f);

        /// <summary>Cube control offset from text (m), same cluster as origin.</summary>
        private static readonly Vector3 CubeOffsetFromText =
            new Vector3(0.22f, 0.04f, 0f);

        /// <summary>Cube scale (local), small indicator beside the UIText.</summary>
        private static readonly Vector3 CubeLocalScale =
            new Vector3(0.07f, 0.07f, 0.07f);

        /// <summary>
        /// Call from <see cref="EasyMate.LateUpdate"/> with plugin toggle.
        /// </summary>
        internal static void Tick(bool enabled)
        {
            SuperController sc = SuperController.singleton;
            _lastTickWantedVisible = enabled;
            if (!enabled || sc == null || sc.isLoading)
            {
                SetAtomsHidden(true);
                return;
            }

            if (!_atomsReady)
            {
                TryStartSpawnCoroutine(sc);
                return;
            }

            SetAtomsHidden(false);

            float ws = sc.worldScale;
            if (ws < 0.01f)
                ws = 0.01f;

            Vector3 textPos = Vector3.zero;
            Quaternion textRot = Quaternion.identity;
            PlaceTextAtom(_textAtom, textPos, textRot);
            PlaceCubeAtomBesideText(_cubeAtom, textPos, textRot, ws);

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

            ApplyCubeDiffuse(_cubeAtom, bothOk ? ColorOk : ColorBad);

            BuildLines(Sb, leftEuler, rightEuler, leftOk, rightOk, bothOk);
            ApplyUiTextBody(_textAtom, Sb.ToString());
        }

        internal static void OnPluginDestroy()
        {
            SuperController sc = SuperController.singleton;
            if (sc != null)
            {
                RemoveAtomIfPresent(sc, AtomUidCube);
                RemoveAtomIfPresent(sc, AtomUidUiText);
            }

            _cubeAtom = null;
            _textAtom = null;
            _atomsReady = false;
            _spawnInProgress = false;
        }

        private static void TryStartSpawnCoroutine(SuperController sc)
        {
            if (_spawnInProgress)
                return;
            _spawnInProgress = true;
            sc.StartCoroutine(CoSpawnAtoms());
        }

        private static IEnumerator CoSpawnAtoms()
        {
            SuperController sc = null;
            try
            {
                sc = SuperController.singleton;
                while (sc != null && sc.isLoading)
                    yield return null;

                if (sc == null)
                    yield break;

                _cubeAtom = sc.GetAtomByUid(AtomUidCube);
                if (_cubeAtom == null)
                {
                    yield return sc.AddAtomByType("Cube", AtomUidCube);
                    yield return null;
                    yield return null;
                    _cubeAtom = sc.GetAtomByUid(AtomUidCube);
                }

                _textAtom = sc.GetAtomByUid(AtomUidUiText);
                if (_textAtom == null)
                {
                    yield return sc.AddAtomByType("UIText", AtomUidUiText);
                    yield return null;
                    yield return null;
                    _textAtom = sc.GetAtomByUid(AtomUidUiText);
                }

                if (_cubeAtom == null || _textAtom == null)
                {
                    SuperController.LogError(
                        "Easy Mate: VR euler test HUD failed to spawn Cube/UIText atoms.");
                    yield break;
                }

                ConfigureNewUiTextAtom(_textAtom);
                if (_cubeAtom.transform != null)
                    _cubeAtom.transform.localScale = CubeLocalScale;

                ApplyCubeDiffuse(_cubeAtom, ColorBad);
                _atomsReady = true;
            }
            finally
            {
                _spawnInProgress = false;
                if (sc != null && !_lastTickWantedVisible)
                    SetAtomsHidden(true);
            }
        }

        private static void ConfigureNewUiTextAtom(Atom uitext)
        {
            if (uitext == null)
                return;
            JSONStorable canvas = uitext.GetStorableByID(StorableCanvas) as JSONStorable;
            if (canvas != null)
            {
                if (canvas.IsFloatJSONParam("xSize"))
                    canvas.SetFloatParamValue("xSize", 920f);
                if (canvas.IsFloatJSONParam("ySize"))
                    canvas.SetFloatParamValue("ySize", 420f);
            }

            JSONStorable txt = uitext.GetStorableByID(StorableText) as JSONStorable;
            if (txt != null)
            {
                if (txt.IsFloatJSONParam("fontSize"))
                    txt.SetFloatParamValue("fontSize", 44f);
            }
        }

        private static void PlaceTextAtom(Atom uitext, Vector3 worldPos, Quaternion worldRot)
        {
            if (uitext == null || uitext.mainController == null)
                return;
            if (uitext.mainController.control == null)
                return;
            uitext.mainController.control.position = worldPos;
            uitext.mainController.control.rotation = worldRot;
        }

        private static void PlaceCubeAtomBesideText(
            Atom cube,
            Vector3 textWorldPos,
            Quaternion textWorldRot,
            float worldScale)
        {
            if (cube == null || cube.mainController == null)
                return;
            if (cube.mainController.control == null)
                return;
            Vector3 off = CubeOffsetFromText * worldScale;
            cube.mainController.control.position =
                textWorldPos + textWorldRot * off;
            cube.mainController.control.rotation = textWorldRot;
        }

        private static void ApplyCubeDiffuse(Atom cube, Color rgb)
        {
            if (cube == null)
                return;

            Color opaque = new Color(rgb.r, rgb.g, rgb.b, 1f);
            HSVColor hsv = HSVColorPicker.RGBToHSV(opaque.r, opaque.g, opaque.b);

            JSONStorable moStore =
                cube.GetStorableByID(StorableMaterials) as JSONStorable;
            if (moStore != null && moStore.IsColorJSONParam("Diffuse Color"))
                moStore.SetColorParamValue("Diffuse Color", hsv);

            MaterialOptions moOpt = moStore as MaterialOptions;
            if (moOpt != null)
            {
                moOpt.color1Alpha = 1f;
                moOpt.SetColor1(opaque);
            }

            ApplyCubeDiffuseToRenderers(cube, opaque);
        }

        /// <summary>
        /// Cube prefabs sometimes never register MaterialOptions color1; still
        /// push tint into every instance material so red/green updates visibly.
        /// </summary>
        private static void ApplyCubeDiffuseToRenderers(Atom cube, Color rgb)
        {
            if (cube == null || cube.gameObject == null)
                return;

            Renderer[] rends = cube.gameObject.GetComponentsInChildren<Renderer>(
                true);
            int ri;
            int mi;
            for (ri = 0; ri < rends.Length; ri++)
            {
                Renderer r = rends[ri];
                if (r == null)
                    continue;

                Material[] mats = r.materials;
                for (mi = 0; mi < mats.Length; mi++)
                {
                    Material m = mats[mi];
                    if (m == null)
                        continue;

                    m.color = rgb;
                    if (m.HasProperty("_Color"))
                        m.SetColor("_Color", rgb);
                    if (m.HasProperty("_BaseColor"))
                        m.SetColor("_BaseColor", rgb);
                    if (m.HasProperty("_TintColor"))
                        m.SetColor("_TintColor", rgb);
                    if (m.HasProperty("_DiffuseColor"))
                        m.SetColor("_DiffuseColor", rgb);
                }
            }
        }

        private static void ApplyUiTextBody(Atom uitext, string body)
        {
            if (uitext == null)
                return;
            JSONStorable txt = uitext.GetStorableByID(StorableText) as JSONStorable;
            if (txt == null)
                return;
            txt.SetStringParamValue("text", body);
        }

        private static void SetAtomsHidden(bool hidden)
        {
            if (_cubeAtom != null)
                _cubeAtom.hidden = hidden;
            if (_textAtom != null)
                _textAtom.hidden = hidden;
        }

        private static void RemoveAtomIfPresent(SuperController sc, string uid)
        {
            if (sc == null || string.IsNullOrEmpty(uid))
                return;
            Atom a = sc.GetAtomByUid(uid);
            if (a == null)
                return;
            try
            {
                sc.RemoveAtom(a);
            }
            catch
            {
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
