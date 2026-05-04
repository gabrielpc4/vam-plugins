using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MeshVR;
using SimpleJSON;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VR: <b>right thumbstick click</b> (OVR / same binding as free navigation in
    /// OpenVR via <c>SuperController</c>) spawns at the right hand. Clone mode restores
    /// catalog storables (colors, scale, presets). Fallback: AddAtomByType. Mandatory
    /// first catalog Dildo still gets stiff segment springs in JSON. ToyBP Alpha Adjust, etc.
    /// </summary>
    public class DildoOnHands : MVRScript
    {
        public const string PluginName = "HandSpawnToy";

        /// Default VaM-relative path: slim <c>{ "atoms": [...] }</c> from
        /// extract_toy_catalog.py (checked in beside this plugin). Full scenes
        /// work too if you paste their path here.
        public const string DefaultCatalogSceneRelativePath =
            "Custom/Scripts/DildoOnHands/handspawn_toy_atoms.json";

        private static readonly string[] VarietyToyAtomTypes =
        {
            "Dildo",
            "ToyAH",
            "ToyBP",
            "Paddle",
        };

        private SuperController _sc;

        private JSONStorableBool _vrToySpawnListenEnabled;

        private JSONStorableBool _cloneFromCatalogScene;

        private JSONStorableString _catalogSceneRelativePath;

        private JSONStorableString _catalogExtraTypesWhitelist;

        private JSONStorableFloat _localOffsetForward;

        private JSONStorableFloat _localOffsetRight;

        private JSONStorableFloat _localOffsetUp;

        private JSONStorableFloat _localEulerPitchDeg;

        private JSONStorableFloat _localEulerYawDeg;

        private JSONStorableFloat _localEulerRollDeg;

        private JSONStorableBool _builtInTipCorrections;

        private JSONStorableFloat _tipExtraEulerPitchDeg;

        private JSONStorableFloat _tipExtraEulerYawDeg;

        private JSONStorableFloat _tipExtraEulerRollDeg;

        private JSONStorableBool _spawnAtHandPivotOnly;

        private JSONStorableString _extraToyAtomTypes;

        private bool _spawnCoroutineRunning;

        private bool _waitingMandatoryFirstDildo = true;

        private string _lastToyAtomTypeSpawned;

        /// <summary>Scene atom id of last spawned catalog clone (#suffix).</summary>
        private string _lastSceneToySourceId;

        private sealed class SceneToyTemplate
        {
            public readonly string SceneAtomId;
            public readonly string AtomTypeName;
            public readonly string SerializedAtomJson;

            public SceneToyTemplate(
                string sceneAtomId,
                string atomTypeName,
                string serializedAtomJson)
            {
                SceneAtomId = sceneAtomId;
                AtomTypeName = atomTypeName;
                SerializedAtomJson = serializedAtomJson;
            }
        }

        private List<SceneToyTemplate> _catalogToyTemplates;

        private string _catalogPathLastLoaded;

        private SceneToyTemplate _mandatoryDildoCatalogEntry;

        /// <summary>Plugin UI: toggles VR toy spawn input listener; label shows ON/OFF.</summary>
        private UIDynamicButton _vrToySpawnToggleButton;

        public override void Init()
        {
            try
            {
                pluginLabelJSON.val = PluginName;
                _sc = SuperController.singleton;

                _vrToySpawnListenEnabled = new JSONStorableBool(
                    "Listen for VR toy spawn (right thumbstick click)",
                    true,
                    OnVrToySpawnListenEnabledChanged);

                RegisterBool(_vrToySpawnListenEnabled);

                _vrToySpawnToggleButton = CreateButton(
                    VrToySpawnToggleButtonLabel(),
                    false);

                if (_vrToySpawnToggleButton != null &&
                    _vrToySpawnToggleButton.button != null)
                {
                    _vrToySpawnToggleButton.button.onClick.AddListener(
                        OnVrToySpawnToggleButtonClicked);
                }

                _cloneFromCatalogScene = new JSONStorableBool(
                    "Clone toys from catalog scene JSON",
                    true);

                RegisterBool(_cloneFromCatalogScene);

                _catalogSceneRelativePath = new JSONStorableString(
                    "Toy catalog scene path (VaM-relative)",
                    DefaultCatalogSceneRelativePath);
                RegisterString(_catalogSceneRelativePath);

                _catalogExtraTypesWhitelist = new JSONStorableString(
                    "Extra atom types allowed in catalog (one per line)",
                    "");
                RegisterString(_catalogExtraTypesWhitelist);

                _localOffsetForward =
                    new JSONStorableFloat(
                        "Hand local offset forward (m)",
                        0.05f,
                        -0.2f,
                        0.2f,
                        false);
                _localOffsetRight =
                    new JSONStorableFloat(
                        "Hand local offset right (m)",
                        0f,
                        -0.2f,
                        0.2f,
                        false);
                _localOffsetUp =
                    new JSONStorableFloat(
                        "Hand local offset up (m)",
                        -0.02f,
                        -0.2f,
                        0.2f,
                        false);
                RegisterFloat(_localOffsetForward);
                RegisterFloat(_localOffsetRight);
                RegisterFloat(_localOffsetUp);

                _localEulerPitchDeg =
                    new JSONStorableFloat(
                        "Hand local euler pitch (deg)",
                        -90f,
                        -180f,
                        180f,
                        false);
                _localEulerYawDeg =
                    new JSONStorableFloat(
                        "Hand local euler yaw (deg)",
                        0f,
                        -180f,
                        180f,
                        false);
                _localEulerRollDeg =
                    new JSONStorableFloat(
                        "Hand local euler roll (deg)",
                        0f,
                        -180f,
                        180f,
                        false);
                RegisterFloat(_localEulerPitchDeg);
                RegisterFloat(_localEulerYawDeg);
                RegisterFloat(_localEulerRollDeg);

                _builtInTipCorrections = new JSONStorableBool(
                    "Built-in tip direction per atom type (heuristic)",
                    true);
                RegisterBool(_builtInTipCorrections);

                _tipExtraEulerPitchDeg =
                    new JSONStorableFloat(
                        "Extra tip euler pitch (deg)",
                        0f,
                        -180f,
                        180f,
                        false);
                _tipExtraEulerYawDeg =
                    new JSONStorableFloat(
                        "Extra tip euler yaw (deg)",
                        0f,
                        -180f,
                        180f,
                        false);
                _tipExtraEulerRollDeg =
                    new JSONStorableFloat(
                        "Extra tip euler roll (deg)",
                        0f,
                        -180f,
                        180f,
                        false);
                RegisterFloat(_tipExtraEulerPitchDeg);
                RegisterFloat(_tipExtraEulerYawDeg);
                RegisterFloat(_tipExtraEulerRollDeg);

                _spawnAtHandPivotOnly = new JSONStorableBool(
                    "Spawn exactly at hand (ignore offset sliders)",
                    true);
                RegisterBool(_spawnAtHandPivotOnly);

                _extraToyAtomTypes = new JSONStorableString(
                    "Legacy fallback: extra atom types (one per line)",
                    "");
                _extraToyAtomTypes.storeType = JSONStorableParam.StoreType.Full;
                RegisterString(_extraToyAtomTypes);
            }
            catch (Exception e)
            {
                SuperController.LogError(PluginName + " Init: " + e);
            }
        }

        public override void InitUI()
        {
            base.InitUI();
            RefreshVrToySpawnToggleButtonLabel();
        }

        private static string VrToySpawnToggleButtonLabelForState(bool enabled)
        {
            if (enabled)
                return "VR toy spawn: ON (click to disable)";
            return "VR toy spawn: OFF (click to enable)";
        }

        private string VrToySpawnToggleButtonLabel()
        {
            if (_vrToySpawnListenEnabled == null)
                return "VR toy spawn: ?";
            return VrToySpawnToggleButtonLabelForState(_vrToySpawnListenEnabled.val);
        }

        private void RefreshVrToySpawnToggleButtonLabel()
        {
            if (_vrToySpawnToggleButton == null)
                return;
            _vrToySpawnToggleButton.label = VrToySpawnToggleButtonLabel();
        }

        private void OnVrToySpawnListenEnabledChanged(bool newVal)
        {
            RefreshVrToySpawnToggleButtonLabel();
        }

        private void OnVrToySpawnToggleButtonClicked()
        {
            if (_vrToySpawnListenEnabled == null)
                return;
            _vrToySpawnListenEnabled.val = !_vrToySpawnListenEnabled.val;
        }

        private static void AppendTypeIfDistinct(List<string> dest, string t)
        {
            if (dest == null || t == null)
                return;
            string trimmed;
            trimmed = t.Trim();
            if (trimmed.Length == 0)
                return;
            foreach (string existing in dest)
            {
                if (existing == trimmed)
                    return;
            }

            dest.Add(trimmed);
        }

        private HashSet<string> BuildCatalogTypeWhitelist()
        {
            HashSet<string> set = new HashSet<string>();
            foreach (string builtin in VarietyToyAtomTypes)
            {
                set.Add(builtin);
            }

            try
            {
                if (_catalogExtraTypesWhitelist == null)
                    return set;
                string raw;
                raw = _catalogExtraTypesWhitelist.val;
                if (string.IsNullOrEmpty(raw))
                    return set;
                string[] lines = raw.Split(
                    new char[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string t = line.Trim();
                    if (t.Length > 0)
                        set.Add(t);
                }
            }
            catch (Exception ex)
            {
                SuperController.LogError(
                    PluginName + ": catalog whitelist parse: " + ex.Message);
            }

            return set;
        }

        private List<string> BuildVarietyToyTypePool()
        {
            List<string> pool;
            pool = new List<string>();
            foreach (string s in VarietyToyAtomTypes)
            {
                AppendTypeIfDistinct(pool, s);
            }

            try
            {
                if (_extraToyAtomTypes == null ||
                    string.IsNullOrEmpty(_extraToyAtomTypes.val))
                    return pool;
                string[] lines;
                lines = _extraToyAtomTypes.val.Split(
                    new char[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    AppendTypeIfDistinct(pool, line);
                }
            }
            catch (Exception ex)
            {
                SuperController.LogError(
                    PluginName + ": extra toy atom type parse: " + ex.Message);
            }

            return pool;
        }


        private static JSONClass AxisZeroEulerJson()
        {
            JSONClass e = new JSONClass();
            e["x"] = "0";
            e["y"] = "0";
            e["z"] = "0";
            return e;
        }

        /// <remarks>
        /// Catalog scenes often freeze toys: physicsDisabled makes the follow-RB
        /// kinematic, and grab booleans false. JSON is patched before Restore;
        /// runtime reinforce here via <see cref="UnlockMainPhysicsForGrab"/>.
        /// </remarks>
        private static void NeutralizeStoredWorldPose(JSONClass atomJc)
        {
            atomJc.Remove("parentAtom");
            atomJc.Remove("containerPosition");
            atomJc.Remove("containerRotation");
            atomJc.Remove("position");
            atomJc.Remove("rotation");

            JSONNode storN = atomJc["storables"];
            JSONArray arr = storN != null ? storN.AsArray : null;
            if (arr == null)
                return;

            for (int i = 0; i < arr.Count; i++)
            {
                JSONClass st = arr[i] as JSONClass;
                if (st == null)
                    continue;
                JSONNode sid = st["id"];
                string idText = sid != null ? sid.Value : "";
                if (idText != "control")
                    continue;

                JSONClass pos = new JSONClass();
                pos["x"] = "0";
                pos["y"] = "0";
                pos["z"] = "0";
                JSONClass euler = AxisZeroEulerJson();
                st["position"] = pos;
                st["rotation"] = euler;

                // Main FreeController JSON: drive physics RB, not kinematic glue.
                st["physicsEnabled"] = "true";

                // Allow laser / hand grab arrows on XYZ even if preset disabled.
                st["canGrabPosition"] = "true";
                st["canGrabRotation"] = "true";

                st["positionState"] = "On";
                st["rotationState"] = "On";

                st["xPositionLock"] = "false";
                st["yPositionLock"] = "false";
                st["zPositionLock"] = "false";
                st["xRotationLock"] = "false";
                st["yRotationLock"] = "false";
                st["zRotationLock"] = "false";

                st.Remove("linkTo");
                st.Remove("linkPositionSpring");
                st.Remove("linkPositionDamper");
                st.Remove("linkRotationSpring");
                st.Remove("linkRotationDamper");
            }
        }

        /// <remarks>
        /// Dildos use storables/id <c>springControl</c>; <c>springStrength</c> scales
        /// internal segment joints (~0=droopy, ~1=stiff).
        /// </remarks>
        private static void ApplyDildoSpringStrength01InToyJson(
            JSONClass atomJc,
            float strength01)
        {
            JSONNode tn = atomJc["type"];
            string tTxt;

            tTxt = tn != null ? tn.Value : "";

            if (tTxt != "Dildo")
                return;

            JSONArray arr;

            arr = atomJc["storables"] != null
                ? atomJc["storables"].AsArray
                : null;
            if (arr == null)
                return;

            float clamped;

            clamped = Mathf.Clamp01(strength01);
            string sVal;

            sVal =
                clamped.ToString("G", CultureInfo.InvariantCulture);

            int iIdx;
            for (iIdx = 0; iIdx < arr.Count; iIdx++)
            {
                JSONClass st = arr[iIdx] as JSONClass;
                if (st == null)
                    continue;

                JSONNode sidn = st["id"];
                string sidTxt;

                sidTxt = sidn != null ? sidn.Value : "";
                if (sidTxt != "springControl")
                    continue;

                st["springStrength"] = sVal;
            }
        }

        /// <summary>
        /// Catalog JSON is loaded and parsed once per catalog path for a VaM
        /// session (see <see cref="EnsureToyCatalogFresh"/> cache). Spawn does not
        /// re-open the file every trigger. Slim files with only
        /// <c>atoms</c> work; offline build: extract_toy_catalog.py.
        /// </summary>
        /// <returns>Whether catalog rebuilt successfully with one+ toys.</returns>
        private bool TryRebuildToyCatalog(bool logErrors)
        {
            string desiredPath;

            desiredPath =
                (_catalogSceneRelativePath != null)
                    ? _catalogSceneRelativePath.val.Trim()
                    : DefaultCatalogSceneRelativePath;

            if (desiredPath.Length == 0)
                desiredPath = DefaultCatalogSceneRelativePath;

            HashSet<string> whitelist = BuildCatalogTypeWhitelist();

            _catalogToyTemplates = new List<SceneToyTemplate>();
            _mandatoryDildoCatalogEntry = null;
            _catalogPathLastLoaded = desiredPath;

            if (_sc == null)
                return false;

            string txt;
            try
            {
                txt = SuperController.singleton.ReadFileIntoString(desiredPath);
            }
            catch (Exception ex)
            {
                if (logErrors)
                    SuperController.LogError(
                        PluginName + ": catalog read failed: " + ex.Message);

                return false;
            }

            if (txt == null || txt.Length == 0)
            {
                if (logErrors)
                    SuperController.LogError(
                        PluginName + ": catalog file empty / missing (" +
                            desiredPath + ").");

                return false;
            }

            JSONNode rootNode;
            try
            {
                rootNode = JSONNode.Parse(txt);
            }
            catch (Exception ex)
            {
                if (logErrors)
                    SuperController.LogError(
                        PluginName + ": catalog JSON parse failed: " + ex.Message);

                return false;
            }

            JSONClass sceneRoot = rootNode.AsObject;
            JSONArray atoms = sceneRoot != null && sceneRoot["atoms"] != null
                ? sceneRoot["atoms"].AsArray
                : null;

            if (atoms == null)
                return false;

            foreach (JSONNode child in atoms)
            {
                JSONClass entry = child as JSONClass;
                if (entry == null)
                    continue;
                JSONNode typeN = entry["type"];
                if (typeN == null)
                    continue;
                string typeName = typeN.Value;
                if (!whitelist.Contains(typeName))
                    continue;
                JSONNode idN = entry["id"];
                string sceneUid = idN != null ? idN.Value : "";
                if (sceneUid.Length == 0)
                    continue;

                SceneToyTemplate row = new SceneToyTemplate(
                    sceneUid,
                    typeName,
                    entry.ToString());

                _catalogToyTemplates.Add(row);

                if (typeName == "Dildo")
                {
                    if (_mandatoryDildoCatalogEntry == null &&
                        sceneUid == "Dildo")
                        _mandatoryDildoCatalogEntry = row;
                }
            }

            if (_mandatoryDildoCatalogEntry == null)
            {
                foreach (SceneToyTemplate t in _catalogToyTemplates)
                {
                    if (t.AtomTypeName != "Dildo")
                        continue;
                    _mandatoryDildoCatalogEntry = t;
                    break;
                }
            }

            return _catalogToyTemplates.Count > 0;
        }

        private bool EnsureToyCatalogFresh()
        {
            string p = (_catalogSceneRelativePath != null)
                ? _catalogSceneRelativePath.val.Trim()
                : DefaultCatalogSceneRelativePath;
            if (p.Length == 0)
                p = DefaultCatalogSceneRelativePath;

            if (_catalogToyTemplates != null &&
                _catalogPathLastLoaded == p)
                return _catalogToyTemplates.Count > 0;

            return TryRebuildToyCatalog(true);
        }

        private SceneToyTemplate PickMandatoryDildoOrNull()
        {
            if (_mandatoryDildoCatalogEntry != null)
                return _mandatoryDildoCatalogEntry;

            foreach (SceneToyTemplate t in _catalogToyTemplates)
            {
                if (t.AtomTypeName == "Dildo")
                    return t;
            }

            return null;
        }

        private static string EscapeSceneAtomIdForUid(string sceneAtomId)
        {
            string s;

            if (sceneAtomId == null)
                return "_";

            s = sceneAtomId.Trim();

            if (s.Length == 0)
                return "_";

            s = s.Replace("#", "h");
            s = s.Replace(" ", "_");
            s = s.Replace("/", "_");
            s = s.Replace("\\", "_");
            s = s.Replace(":", "_");

            return s;
        }

        /// <remarks>
        /// Detect which catalog preset is still spawned (HandSpawnToy + double
        /// underscore). Legacy atoms use a single underscore only.
        /// </remarks>
        private static string SpawnUidPrefixForCatalogRow(SceneToyTemplate t)
        {
            return PluginName +
                "__" +
                EscapeSceneAtomIdForUid(t.SceneAtomId) +
                "__";
        }

        private static bool IsCatalogToyInstanceInScene(
            SceneToyTemplate t,
            SuperController svc)
        {
            if (t == null || svc == null)
                return false;

            string pfx;
            List<Atom> lst;

            pfx = SpawnUidPrefixForCatalogRow(t);

            lst = svc.GetAtoms();
            foreach (Atom a in lst)
            {
                string u;

                if (a == null)
                    continue;
                u = a.uid;

                if (u != null && u.StartsWith(pfx))
                    return true;
            }

            return false;
        }

        /// <remarks>
        /// Prefer a catalog toy not yet spawned (by uid prefix scan). When every
        /// row has an instance present, reuse the whole pool (duplicate ok).
        /// </remarks>
        private SceneToyTemplate PickVarietyToyTemplate()
        {
            SuperController svc;
            svc = SuperController.singleton;

            int count;
            count = (_catalogToyTemplates != null)
                ? _catalogToyTemplates.Count
                : 0;

            if (count == 0)
                return null;

            if (count == 1)
                return _catalogToyTemplates[0];

            List<SceneToyTemplate> absent;
            absent = new List<SceneToyTemplate>();

            foreach (SceneToyTemplate t in _catalogToyTemplates)
            {
                if (!IsCatalogToyInstanceInScene(t, svc))
                    absent.Add(t);
            }

            List<SceneToyTemplate> bag;
            if (absent.Count > 0)
                bag = absent;
            else
                bag = _catalogToyTemplates;

            int bagCount;
            bagCount = bag.Count;

            int guard;

            guard = 0;

            while (guard < 96)
            {
                guard++;

                SceneToyTemplate pick;
                pick = bag[UnityEngine.Random.Range(0, bagCount)];

                if (_lastSceneToySourceId != null &&
                    pick.SceneAtomId == _lastSceneToySourceId &&
                    bagCount > 1)
                    continue;

                return pick;
            }

            return bag[0];
        }

        private string PickLegacyTypePreferringAbsentFromScene()
        {
            List<string> pool;

            pool = BuildVarietyToyTypePool();

            if (pool.Count == 0)
                return "Dildo";

            SuperController svc;
            svc = SuperController.singleton;

            List<string> absent;
            absent = new List<string>();

            if (svc != null)
            {
                foreach (string tp in pool)
                {
                    bool any;
                    any = false;

                    foreach (Atom a in svc.GetAtoms())
                    {
                        if (a == null)
                            continue;

                        if (a.type != tp)
                            continue;

                        any = true;
                        break;
                    }

                    if (!any)
                        absent.Add(tp);
                }
            }

            List<string> bag;
            if (absent.Count > 0)
                bag = absent;
            else
                bag = new List<string>(pool);

            if (bag.Count == 1)
                return bag[0];

            string last;
            last = _lastToyAtomTypeSpawned;

            int tries;
            tries = 0;

            while (tries < 72)
            {
                tries++;

                string cand;
                cand = bag[UnityEngine.Random.Range(0, bag.Count)];

                if (bag.Count <= 1)
                    return cand;

                if (last == null || cand != last)
                    return cand;
            }

            return bag[0];
        }

        private Transform ResolveHandWorldTransform(SuperController sc,
            bool left)
        {
            if (sc == null)
                return null;
            Transform primary = left ? sc.leftHand : sc.rightHand;
            if (primary != null)
                return primary;
            if (left)
                return sc.isOpenVR ? sc.viveObjectLeft : sc.touchObjectLeft;
            return sc.isOpenVR ? sc.viveObjectRight : sc.touchObjectRight;
        }

        private Quaternion LocalGripOffsetQuaternion()
        {
            return Quaternion.Euler(
                _localEulerPitchDeg.val,
                _localEulerYawDeg.val,
                _localEulerRollDeg.val);
        }

        /// <summary>
        /// Cheap per-type guesses so length tends away from palm. Meshes differ by
        /// prefab ; turn Built-in hint off and use Extra euler sliders to tune.
        /// </summary>
        private static Quaternion TipHeuristicQuaternionForAtomType(
            string atomType)
        {
            if (atomType == null)
                return Quaternion.identity;

            switch (atomType)
            {
                case "Dildo":
                    // Often reads as world-up with default -90 grip; try +X spin.
                    return Quaternion.Euler(90f, 0f, 0f);

                case "ToyAH":
                case "ToyBP":
                case "Paddle":
                default:
                    // Grip +Z tended to aim props at the headset; flip about local X so
                    // length runs away along palm forward (+Z) instead of toward you.
                    return Quaternion.Euler(180f, 0f, 0f);
            }
        }

        private Quaternion ComposeGripLocalTipRotation(Atom spawned)
        {
            Quaternion qOut;
            qOut = Quaternion.identity;

            bool useHint =
                _builtInTipCorrections != null &&
                _builtInTipCorrections.val;

            if (useHint && spawned != null)
                qOut =
                    TipHeuristicQuaternionForAtomType(spawned.type);

            Quaternion extras;
            extras = Quaternion.Euler(
                (_tipExtraEulerPitchDeg != null)
                    ? _tipExtraEulerPitchDeg.val
                    : 0f,
                (_tipExtraEulerYawDeg != null)
                    ? _tipExtraEulerYawDeg.val
                    : 0f,
                (_tipExtraEulerRollDeg != null)
                    ? _tipExtraEulerRollDeg.val
                    : 0f);

            return qOut * extras;
        }

        private static void SnapSpawnRigidbodyToHand(FreeControllerV3 fc)
        {
            Rigidbody rb;

            rb = fc != null ? fc.followWhenOffRB : null;

            if (rb == null)
                return;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        /// <summary>
        /// Match scene presets that disable physics (kinematic), grab toggles, or axis
        /// locks — user cannot laser-grab otherwise.
        /// </summary>
        private static void UnlockMainPhysicsForGrab(FreeControllerV3 fc)
        {
            if (fc == null)
                return;

            fc.physicsEnabled = true;
            fc.canGrabPosition = true;
            fc.canGrabRotation = true;
            fc.currentPositionState = FreeControllerV3.PositionState.On;
            fc.currentRotationState = FreeControllerV3.RotationState.On;
        }

        private const string MATERIALS_STORE_ID = "materials";

        /// <remarks>
        /// VaM exposes this float on toy <see cref="MaterialOptions"/> as "Alpha Adjust"
        /// (shader ~_AlphaAdjust); used for butt-plug transparency.
        /// </remarks>
        private const string MATERIAL_FLOAT_ALPHA_ADJUST = "Alpha Adjust";

        /// <remarks>Alternate label some meshes use vs engine default.</remarks>
        private const string MATERIAL_FLOAT_ALPHA_ADJUST_ALT = "Alpha Adjustment";
        private const float ToyBpAlphaAdjustPreset = -0.5f;

        private static Color RandomToyDiffuseRgb()
        {
            float hVal;
            float sVal;
            float vVal;
            Color cRgb;

            hVal = UnityEngine.Random.Range(0f, 1f);
            sVal = UnityEngine.Random.Range(0.49f, 1f);
            vVal = UnityEngine.Random.Range(0.52f, 1f);

            cRgb = HSVColorPicker.HSVToRGB(hVal, sVal, vVal);

            return new Color(cRgb.r, cRgb.g, cRgb.b, 1f);
        }

        /// <summary>
        /// After Restore/catalog JSON: replaces Diffuse Color (HSV-derived) once per spawn;
        /// ToyBP also drives Alpha Adjust for transparency meshes.
        /// </summary>
        private static void ApplySpawnToyMaterialLook(Atom spawned)
        {
            MaterialOptions matsOpt;
            JSONStorable jst;
            Color cRand;

            if (spawned == null)
                return;

            matsOpt =
                spawned.GetStorableByID(MATERIALS_STORE_ID) as MaterialOptions;

            if (matsOpt == null)
                return;

            cRand = RandomToyDiffuseRgb();
            matsOpt.color1Alpha = 1f;
            matsOpt.SetColor1(cRand);

            if (spawned.type != "ToyBP")
                return;

            jst = matsOpt as JSONStorable;

            if (jst == null)
                return;

            if (jst.IsFloatJSONParam(MATERIAL_FLOAT_ALPHA_ADJUST))
            {
                jst.SetFloatParamValue(
                    MATERIAL_FLOAT_ALPHA_ADJUST,
                    ToyBpAlphaAdjustPreset);
            }
            else if (jst.IsFloatJSONParam(MATERIAL_FLOAT_ALPHA_ADJUST_ALT))
            {
                jst.SetFloatParamValue(
                    MATERIAL_FLOAT_ALPHA_ADJUST_ALT,
                    ToyBpAlphaAdjustPreset);
            }
        }

        private void PlaceSpawnAtHand(Atom spawned, bool leftHand)
        {
            if (spawned == null || _sc == null)
                return;

            Transform hand = ResolveHandWorldTransform(_sc, leftHand);
            if (hand == null)
                return;

            FreeControllerV3 fc = spawned.mainController;

            if (fc == null)
                return;

            Vector3 localOff;

            if (_spawnAtHandPivotOnly != null && _spawnAtHandPivotOnly.val)
                localOff = Vector3.zero;
            else
                localOff =
                    new Vector3(
                        _localOffsetRight.val,
                        _localOffsetUp.val,
                        _localOffsetForward.val);

            Vector3 worldPos =
                hand.TransformPoint(localOff);

            Quaternion worldRot =
                hand.rotation *
                    LocalGripOffsetQuaternion() *
                    ComposeGripLocalTipRotation(spawned);

            UnlockMainPhysicsForGrab(fc);
            fc.transform.rotation = worldRot;
            fc.transform.position = worldPos;
            SnapSpawnRigidbodyToHand(fc);
        }

        /// <summary>
        /// Match Easy Mate / VaM POV: skip toy spawn while a Person head or
        /// hand is possessed (input is used for UI / grab).
        /// </summary>
        private static bool AnyPersonHeadOrHandPossessedForToySpawnGuard()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;
                foreach (Atom a in sc.GetAtoms())
                {
                    if (a == null || a.type != "Person" ||
                        !a.gameObject.activeInHierarchy)
                        continue;
                    FreeControllerV3 h =
                        a.GetStorableByID("headControl") as FreeControllerV3;
                    if (h != null && h.possessed)
                        return true;
                    FreeControllerV3 l =
                        a.GetStorableByID("lHandControl") as FreeControllerV3;
                    if (l != null && l.possessed)
                        return true;
                    FreeControllerV3 r =
                        a.GetStorableByID("rHandControl") as FreeControllerV3;
                    if (r != null && r.possessed)
                        return true;
                }
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// Right-thumbstick click when XR is active; VaM maps it through
        /// <c>GetGrabNavigateStartRight</c> on OVR/OpenVR.
        /// </summary>
        private bool TryGetVrToySpawnThumbstickClickDown()
        {
            if (_sc == null)
                return false;

            if (_sc.isOVR || _sc.isOpenVR)
                return _sc.GetGrabNavigateStartRight();

            try
            {
                return OVRInput.GetDown(
                    OVRInput.Button.SecondaryThumbstick,
                    OVRInput.Controller.RTouch);
            }
            catch
            {
                return false;
            }
        }

        private void Update()
        {
            if (_vrToySpawnListenEnabled == null ||
                !_vrToySpawnListenEnabled.val ||
                _sc == null ||
                _sc.isLoading ||
                _spawnCoroutineRunning)
                return;

            bool xrOn = UnityEngine.XR.XRSettings.enabled ||
                _sc.isOVR ||
                _sc.isOpenVR;
            if (!xrOn)
                return;

            if (AnyPersonHeadOrHandPossessedForToySpawnGuard())
                return;

            if (!TryGetVrToySpawnThumbstickClickDown())
                return;

            StartCoroutine(CoSpawnToyAtHand());
        }

        private IEnumerator CoSpawnToyAtHand()
        {
            _spawnCoroutineRunning = true;
            SuperController svc = SuperController.singleton;
            bool cloneMode =
                (_cloneFromCatalogScene != null &&
                    _cloneFromCatalogScene.val);

            try
            {
                if (cloneMode && EnsureToyCatalogFresh())
                {
                    SceneToyTemplate tmpl = null;

                    if (_waitingMandatoryFirstDildo)
                        tmpl = PickMandatoryDildoOrNull();
                    else
                        tmpl = PickVarietyToyTemplate();

                    if (tmpl != null)
                    {
                        JSONClass atomJc = null;
                        try
                        {
                            atomJc = JSONNode.Parse(tmpl.SerializedAtomJson).
                                AsObject;
                        }
                        catch (Exception ex)
                        {
                            SuperController.LogError(
                                PluginName + ": toy template JSON: " +
                                    ex.Message);
                        }

                        if (atomJc != null)
                        {
                            NeutralizeStoredWorldPose(atomJc);
                            bool firstDildo;

                            firstDildo =
                                _waitingMandatoryFirstDildo &&
                                tmpl.AtomTypeName == "Dildo";

                            if (firstDildo)
                            {
                                // Only this clone: max segment stiffness via JSON.
                                // All later catalog restores keep catalog/springControl
                                // as authored (nothing else here touches springs).
                                ApplyDildoSpringStrength01InToyJson(
                                    atomJc,
                                    1f);
                            }

                            string atomType = tmpl.AtomTypeName;
                            string uidCandidate = null;

                            int tIdx;
                            for (tIdx = 0; tIdx < 32; tIdx++)
                            {
                                uidCandidate =
                                    SpawnUidPrefixForCatalogRow(tmpl) +
                                    Mathf.FloorToInt(
                                        Time.realtimeSinceStartup *
                                        1000f) + "_" +
                                    UnityEngine.Random.Range(
                                        100000,
                                        999999999).ToString();

                                if (svc.GetAtomByUid(uidCandidate) == null)
                                    break;

                                uidCandidate = null;
                            }

                            if (uidCandidate != null)
                            {
                                atomJc["id"] = uidCandidate;

                                yield return svc.AddAtomByType(atomType,
                                    uidCandidate);

                                Atom spawned = svc.GetAtomByUid(uidCandidate);
                                if (spawned != null)
                                {
                                    try
                                    {
                                        spawned.PreRestore();
                                        spawned.Restore(atomJc);
                                        spawned.LateRestore(atomJc);
                                        spawned.PostRestore();
                                        _waitingMandatoryFirstDildo =
                                            false;

                                        _lastToyAtomTypeSpawned = atomType;
                                        _lastSceneToySourceId =
                                            tmpl.SceneAtomId;

                                        ApplySpawnToyMaterialLook(spawned);

                                        PlaceSpawnAtHand(spawned, false);

                                        yield break;
                                    }
                                    catch (Exception exR)
                                    {
                                        SuperController.LogError(
                                            PluginName +
                                            ": Restore catalog toy: " +
                                            exR.Message);

                                        try
                                        {
                                            svc.RemoveAtom(spawned);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                            }
                        }

                        SuperController.LogError(
                            PluginName +
                            ": catalog clone failed — trying legacy spawn.");
                    }
                }

                bool consumedMandatory = false;
                string atomLegacy;

                if (_waitingMandatoryFirstDildo)
                {
                    atomLegacy = "Dildo";
                    consumedMandatory = true;
                }
                else
                {
                    atomLegacy =
                        PickLegacyTypePreferringAbsentFromScene();
                }

                string uid = null;

                Atom clashLegacy;
                int tries;
                tries = 0;

                while (tries < 32)
                {
                    uid =
                        PluginName + "_" +
                        Mathf.FloorToInt(Time.realtimeSinceStartup * 1000f) +
                        "_" +
                        UnityEngine.Random.Range(
                            100000,
                            999999999).ToString();

                    clashLegacy = svc.GetAtomByUid(uid);

                    if (clashLegacy == null)
                        break;

                    uid = null;
                    tries++;
                }

                if (uid == null)
                {
                    if (consumedMandatory)
                        _waitingMandatoryFirstDildo = true;

                    yield break;
                }

                yield return svc.AddAtomByType(atomLegacy, uid);

                Atom spawnedLegacy = svc.GetAtomByUid(uid);
                if (spawnedLegacy == null)
                {
                    SuperController.LogError(
                        PluginName +
                            ": legacy failed '" + atomLegacy + "'.");

                    if (consumedMandatory)
                        _waitingMandatoryFirstDildo = true;

                    yield break;
                }

                _waitingMandatoryFirstDildo = false;
                _lastToyAtomTypeSpawned = atomLegacy;
                _lastSceneToySourceId = null;

                ApplySpawnToyMaterialLook(spawnedLegacy);

                PlaceSpawnAtHand(spawnedLegacy, false);
            }
            finally
            {
                _spawnCoroutineRunning = false;
            }
        }
    }
}



