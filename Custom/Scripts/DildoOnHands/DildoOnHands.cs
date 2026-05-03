using System;
using System.Collections;
using System.Collections.Generic;
using MeshVR;
using SimpleJSON;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VR: left-hand Grab / index-trigger only spawns at the right hand so the
    /// right trigger stays normal grab. Clone mode restores toy storables from
    /// catalog JSON (colors, scale, springs). Fallback uses AddAtomByType plus
    /// optional extras. First session spawn is catalog Dildo when present else
    /// legacy Dildo. Oculus uses OVR LTouch triggers when Oculus paths drive
    /// input.
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

        /// <remarks>
        /// Multi-atom props (wand + CollisionTrigger + AudioSource, vibrators +
        /// triggers, …) share an id prefix; spawn clones the whole subgraph with new
        /// uids so cross-refs stay valid. Sorted longest-prefix first so
        /// Vibrator2 wins over Vibrator1.
        /// </remarks>
        private sealed class CatalogCompositeToyDef
        {
            public readonly string Key;
            public readonly string IdPrefix;
            public readonly string GripCatalogUid;

            public CatalogCompositeToyDef(
                string userKey,
                string idPrefix,
                string gripCatalogUid)
            {
                Key = userKey;
                IdPrefix = idPrefix;
                GripCatalogUid = gripCatalogUid;
            }
        }

        /// <remarks>Unified variety pick across singles + composite bundles.</remarks>
        private sealed class CatalogVarietyToyPick
        {
            public readonly bool FromCompositeBundle;
            public readonly SceneToyTemplate SingleToy;
            public readonly CatalogCompositeToyDef Composite;

            private CatalogVarietyToyPick(
                bool fromComposite,
                SceneToyTemplate single,
                CatalogCompositeToyDef composite)
            {
                FromCompositeBundle = fromComposite;
                SingleToy = single;
                Composite = composite;
            }

            public static CatalogVarietyToyPick ForSingleTemplate(
                SceneToyTemplate t)
            {
                return new CatalogVarietyToyPick(false, t, null);
            }

            public static CatalogVarietyToyPick ForComposite(
                CatalogCompositeToyDef c)
            {
                return new CatalogVarietyToyPick(true, null, c);
            }
        }

        private SuperController _sc;

        private JSONStorableBool _listenEnabled;

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

        private Dictionary<string, string> _catalogAtomJsonBySceneUid;

        private List<CatalogCompositeToyDef> _compositeToyBundlesInCatalog;

        private static CatalogCompositeToyDef[] _cachedSortedCompositeToyDefs;

        private string _catalogPathLastLoaded;

        private SceneToyTemplate _mandatoryDildoCatalogEntry;

        public override void Init()
        {
            try
            {
                pluginLabelJSON.val = PluginName;
                _sc = SuperController.singleton;

                _listenEnabled = new JSONStorableBool(
                    "Listen for VR trigger",
                    true);

                RegisterBool(_listenEnabled);

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

        private static void ZeroSpringControlsInToyJson(JSONClass atomJc)
        {
            JSONArray arr;

            arr = atomJc["storables"] != null
                ? atomJc["storables"].AsArray
                : null;
            if (arr == null)
                return;

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

                st["springStrength"] = "0";
            }
        }

        private static int CompareCompositePrefixLength(
            CatalogCompositeToyDef a,
            CatalogCompositeToyDef b)
        {
            return b.IdPrefix.Length - a.IdPrefix.Length;
        }

        private static CatalogCompositeToyDef[] GetSortedCompositeToyDefs()
        {
            if (_cachedSortedCompositeToyDefs != null)
                return _cachedSortedCompositeToyDefs;

            CatalogCompositeToyDef[] arr;
            arr =
                new CatalogCompositeToyDef[]
                {
                    new CatalogCompositeToyDef(
                        "MagicWand",
                        "MagicWand",
                        "MagicWandBody"),

                    new CatalogCompositeToyDef(
                        "Vibrator1",
                        "Vibrator1",
                        "Vibrator1Body"),

                    new CatalogCompositeToyDef(
                        "Vibrator2",
                        "Vibrator2",
                        "Vibrator2Body"),

                    new CatalogCompositeToyDef(
                        "Lush",
                        "Lush",
                        "LushStem"),

                    new CatalogCompositeToyDef(
                        "AnalBalls",
                        "AnalBalls",
                        "AnalBallsRing"),

                    new CatalogCompositeToyDef(
                        "ButtPlug1",
                        "ButtPlug1",
                        "ButtPlug1Stop"),
                };

            Array.Sort(arr, CompareCompositePrefixLength);

            _cachedSortedCompositeToyDefs = arr;

            return arr;
        }

        private static CatalogCompositeToyDef LookupCompositeToyDefForAtomUid(
            string sceneAtomUid)
        {
            if (sceneAtomUid == null || sceneAtomUid.Length == 0)
                return null;

            CatalogCompositeToyDef[] defs;
            defs = GetSortedCompositeToyDefs();

            for (int i = 0; i < defs.Length; i++)
            {
                CatalogCompositeToyDef d;
                d = defs[i];
                if (sceneAtomUid.StartsWith(d.IdPrefix))
                    return d;
            }

            return null;
        }

        private static bool IsCatalogCompositeMemberAtomUid(string sceneUid)
        {
            return LookupCompositeToyDefForAtomUid(sceneUid) != null;
        }

        private static string SpawnCompositeGroupPresenceUidPrefix(
            CatalogCompositeToyDef group)
        {
            return PluginName +
                "__grp__" +
                EscapeSceneAtomIdForUid(group.Key) +
                "__";
        }

        private static bool IsCompositeBundlePresentInScene(
            CatalogCompositeToyDef group,
            SuperController svc)
        {
            if (svc == null || group == null)
                return false;

            string pfx;
            pfx = SpawnCompositeGroupPresenceUidPrefix(group);

            List<Atom> lst;
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

        private static string RemapColonFirstAtomUid(
            string compound,
            Dictionary<string, string> oldToNew)
        {
            if (compound == null || oldToNew == null || compound.Length == 0)
                return compound;

            int iCol;
            iCol = compound.IndexOf(':');

            if (iCol <= 0)
            {
                if (oldToNew.ContainsKey(compound))
                    return oldToNew[compound];

                return compound;
            }

            string pre;
            string suf;

            pre = compound.Substring(0, iCol);
            suf = compound.Substring(iCol);

            if (!oldToNew.ContainsKey(pre))
                return compound;

            return oldToNew[pre] + suf;
        }

        private static string RemapPlainAtomUidMaybe(
            string maybeUid,
            Dictionary<string, string> oldToNew)
        {
            if (maybeUid == null || maybeUid.Length == 0 ||
                oldToNew == null)
                return maybeUid;

            if (oldToNew.ContainsKey(maybeUid))
                return oldToNew[maybeUid];

            return maybeUid;
        }

        private static void RemapUidReferencesRecursive(
            JSONNode n,
            Dictionary<string, string> oldToNew)
        {
            JSONArray arrCandidate;

            arrCandidate = n != null ? n.AsArray : null;

            if (arrCandidate != null)
            {
                int j;
                for (j = 0; j < arrCandidate.Count; j++)
                    RemapUidReferencesRecursive(arrCandidate[j], oldToNew);

                return;
            }

            JSONClass jc;
            jc = n != null ? n.AsObject : null;

            if (jc == null)
                return;

            foreach (KeyValuePair<string, JSONNode> kv in jc)
            {
                string ky;
                JSONNode chNode;

                ky = kv.Key;
                chNode = kv.Value;

                if (chNode == null)
                    continue;

                JSONArray chArr;
                JSONClass nestedObj;

                chArr = chNode.AsArray;
                nestedObj = chNode.AsObject;

                if (chArr != null)
                    RemapUidReferencesRecursive(chArr, oldToNew);
                else if (nestedObj != null)
                    RemapUidReferencesRecursive(nestedObj, oldToNew);
                else
                {
                    string cur;
                    cur = chNode.Value;

                    if (cur == null || cur.Length == 0 || ky.Length == 0)
                        continue;

                    string next;

                    next = cur;

                    if (ky == "parentAtom" || ky == "receiverAtom")
                        next = RemapPlainAtomUidMaybe(cur, oldToNew);
                    else if (ky == "linkTo")
                        next = RemapColonFirstAtomUid(cur, oldToNew);
                    else if (ky == "receiver")
                    {
                        int colPos;
                        colPos = cur.IndexOf(':');

                        if (colPos >= 1)
                            next = RemapColonFirstAtomUid(cur, oldToNew);
                        else
                            next = RemapPlainAtomUidMaybe(cur, oldToNew);
                    }

                    if (next != cur)
                        jc[ky] = next;
                }
            }
        }

        private static List<string> TopoSortAtomsByParentHints(
            List<string> ids,
            Dictionary<string, string> parentOfInsideGroup)
        {
            List<string> outOrder;
            outOrder = new List<string>();

            if (ids == null || ids.Count == 0)
                return outOrder;

            Dictionary<string, List<string>> childrenByParent;
            childrenByParent = new Dictionary<string, List<string>>();

            HashSet<string> idSet;
            idSet = new HashSet<string>();
            foreach (string x in ids)
                idSet.Add(x);

            for (int i = 0; i < ids.Count; i++)
            {
                string id;
                id = ids[i];
                string p;
                p = "";

                if (parentOfInsideGroup.ContainsKey(id))
                    p = parentOfInsideGroup[id];

                bool parentOutside;
                parentOutside =
                    string.IsNullOrEmpty(p) || !idSet.Contains(p);

                if (parentOutside)
                {
                    if (!childrenByParent.ContainsKey(""))
                        childrenByParent[""] = new List<string>();

                    childrenByParent[""].Add(id);
                    continue;
                }

                if (!childrenByParent.ContainsKey(p))
                    childrenByParent[p] = new List<string>();

                childrenByParent[p].Add(id);
            }

            List<string> q;
            q = new List<string>();

            if (!childrenByParent.ContainsKey(""))
                return outOrder;

            q.AddRange(childrenByParent[""]);

            HashSet<string> seen;
            seen = new HashSet<string>();

            int guardSteps;
            guardSteps = 0;

            while (q.Count > 0 && guardSteps < 4096)
            {
                guardSteps++;

                string tip;
                tip = q[0];
                q.RemoveAt(0);

                if (!idSet.Contains(tip))
                    continue;

                if (seen.Contains(tip))
                    continue;

                seen.Add(tip);

                outOrder.Add(tip);

                if (!childrenByParent.ContainsKey(tip))
                    continue;

                List<string> ch;
                ch = childrenByParent[tip];

                for (int c = 0; c < ch.Count; c++)
                    q.Add(ch[c]);
            }

            if (outOrder.Count < ids.Count)
            {
                HashSet<string> seen2;
                seen2 = new HashSet<string>();

                for (int s = 0; s < outOrder.Count; s++)
                    seen2.Add(outOrder[s]);

                for (int r = 0; r < ids.Count; r++)
                {
                    string rest;
                    rest = ids[r];

                    if (seen2.Contains(rest))
                        continue;

                    outOrder.Add(rest);
                }
            }

            return outOrder;
        }

        private IEnumerator CoSpawnCompositeToyBundle(CatalogCompositeToyDef group)
        {
            SuperController svc;
            svc = SuperController.singleton;

            if (svc == null || group == null || _catalogAtomJsonBySceneUid == null)
                yield break;

            List<string> members;
            members = new List<string>();

            Dictionary<string, string> parentHints;
            parentHints = new Dictionary<string, string>();

            Dictionary<string, string> oldToNew;
            oldToNew = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kv in _catalogAtomJsonBySceneUid)
            {
                string cid;
                cid = kv.Key;

                CatalogCompositeToyDef chkMembership;
                chkMembership = LookupCompositeToyDefForAtomUid(cid);

                if (chkMembership != group)
                    continue;

                members.Add(cid);
            }

            if (members.Count == 0)
                yield break;

            foreach (string mem in members)
            {
                JSONNode rootPn;
                rootPn =
                    JSONNode.Parse(_catalogAtomJsonBySceneUid[mem]);

                JSONClass one;
                one = rootPn.AsObject;

                JSONNode pn;
                pn = one != null ? one["parentAtom"] : null;
                string ptxt;

                ptxt = pn != null ? pn.Value : "";

                parentHints[mem] = ptxt;
            }

            List<string> order;
            order = TopoSortAtomsByParentHints(members, parentHints);

            string batchNonce;
            batchNonce =
                Mathf.FloorToInt(Time.realtimeSinceStartup * 1000f) +
                "_" +
                UnityEngine.Random.Range(100000, 999999999).ToString();

            oldToNew.Clear();

            int mapIdx;

            mapIdx = 0;

            for (mapIdx = 0; mapIdx < members.Count; mapIdx++)
            {
                string origId;
                origId = members[mapIdx];

                oldToNew[origId] =
                    SpawnCompositeGroupPresenceUidPrefix(group) +
                    EscapeSceneAtomIdForUid(origId) +
                    "__" +
                    batchNonce;
            }

            int spIdx;

            spIdx = 0;

            for (spIdx = 0; spIdx < order.Count; spIdx++)
            {
                string oldUid;
                oldUid = order[spIdx];

                JSONClass clone;
                JSONNode clonedRoot;

                clonedRoot =
                    JSONNode.Parse(_catalogAtomJsonBySceneUid[oldUid]);

                clone = clonedRoot.AsObject;

                NeutralizeStoredWorldPose(clone);
                ZeroSpringControlsInToyJson(clone);

                RemapUidReferencesRecursive(clone, oldToNew);

                clone["id"] = oldToNew[oldUid];

                JSONNode nidType;
                string typ;

                nidType = clone["type"];
                typ = nidType != null ? nidType.Value : "";

                string newUidResolved;

                newUidResolved = clone["id"] != null
                    ? clone["id"].Value
                    : "";

                if (newUidResolved == null || newUidResolved.Length == 0 ||
                    typ == null || typ.Length == 0 ||
                    svc.GetAtomByUid(newUidResolved) != null)
                    continue;

                yield return svc.AddAtomByType(typ, newUidResolved);

                Atom spawnedPart;
                spawnedPart = svc.GetAtomByUid(newUidResolved);

                if (spawnedPart == null)
                {
                    SuperController.LogError(
                        PluginName +
                            ": composite part missing '" +
                            typ +
                            "'.");

                    yield break;
                }

                try
                {
                    spawnedPart.PreRestore();
                    spawnedPart.Restore(clone);
                    spawnedPart.LateRestore(clone);
                    spawnedPart.PostRestore();
                }
                catch (Exception exRb)
                {
                    SuperController.LogError(
                        PluginName +
                            ": composite Restore: " +
                            exRb.Message);

                    try
                    {
                        svc.RemoveAtom(spawnedPart);
                    }
                    catch
                    {
                    }

                    yield break;
                }
            }

            string gripNew;

            gripNew =
                RemapPlainAtomUidMaybe(group.GripCatalogUid, oldToNew);

            Atom gripAtom;

            gripAtom = svc.GetAtomByUid(gripNew);

            if (gripAtom == null)
            {
                SuperController.LogError(
                    PluginName +
                        ": composite grip atom missing.");
            }
            else
            {
                PlaceSpawnAtHand(gripAtom, false);

                string grpSidDone;

                grpSidDone = GrpVarietyScratchId(group.Key);

                _waitingMandatoryFirstDildo = false;
                _lastToyAtomTypeSpawned = grpSidDone;
                _lastSceneToySourceId = grpSidDone;
            }
        }

        private static string GrpVarietyScratchId(string grpKeyRaw)
        {
            return "Grp:" + grpKeyRaw;
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
            _compositeToyBundlesInCatalog = new List<CatalogCompositeToyDef>();
            _catalogAtomJsonBySceneUid =
                new Dictionary<string, string>();
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

                JSONNode idN = entry["id"];
                string sceneUid = idN != null ? idN.Value : "";
                if (sceneUid.Length == 0)
                    continue;

                _catalogAtomJsonBySceneUid[sceneUid] = entry.ToString();

                if (IsCatalogCompositeMemberAtomUid(sceneUid))
                    continue;

                JSONNode typeN = entry["type"];
                if (typeN == null)
                    continue;

                string typeName = typeN.Value;

                if (!whitelist.Contains(typeName))
                    continue;

                SceneToyTemplate row =
                    new SceneToyTemplate(
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

            CatalogCompositeToyDef[] grpDefsSorted;
            grpDefsSorted = GetSortedCompositeToyDefs();

            int gdi;

            for (gdi = 0; gdi < grpDefsSorted.Length; gdi++)
            {
                CatalogCompositeToyDef grpRow;
                grpRow = grpDefsSorted[gdi];

                bool grpExists;
                grpExists = false;

                List<string> allKeysScratch;
                allKeysScratch =
                    new List<string>(_catalogAtomJsonBySceneUid.Keys);

                for (int kix = 0; kix < allKeysScratch.Count; kix++)
                {
                    string kk;
                    kk = allKeysScratch[kix];

                    if (LookupCompositeToyDefForAtomUid(kk) != grpRow)
                        continue;

                    grpExists = true;
                    break;
                }

                if (grpExists)
                    _compositeToyBundlesInCatalog.Add(grpRow);
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

            return (_catalogToyTemplates.Count > 0) ||
                (_compositeToyBundlesInCatalog.Count > 0);
        }

        private bool EnsureToyCatalogFresh()
        {
            string p = (_catalogSceneRelativePath != null)
                ? _catalogSceneRelativePath.val.Trim()
                : DefaultCatalogSceneRelativePath;
            if (p.Length == 0)
                p = DefaultCatalogSceneRelativePath;

            if (_catalogToyTemplates != null &&
                _compositeToyBundlesInCatalog != null &&
                _catalogPathLastLoaded == p)
                return (_catalogToyTemplates.Count > 0) ||
                    (_compositeToyBundlesInCatalog.Count > 0);

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
        /// Variety across singleton templates and multi-atom prefab bundles
        /// (wand + CollisionTrigger + AudioSource, vibrators …).
        /// </remarks>
        private CatalogVarietyToyPick PickVarietyCatalogToyPick()
        {
            SuperController svc;
            svc = SuperController.singleton;

            List<CatalogVarietyToyPick> choices;
            choices = new List<CatalogVarietyToyPick>();

            if (_catalogToyTemplates != null)
            {
                foreach (SceneToyTemplate t in _catalogToyTemplates)
                {
                    if (IsCatalogToyInstanceInScene(t, svc))
                        continue;

                    choices.Add(
                        CatalogVarietyToyPick.ForSingleTemplate(t));
                }
            }

            if (_compositeToyBundlesInCatalog != null)
            {
                for (int c = 0; c < _compositeToyBundlesInCatalog.Count; c++)
                {
                    CatalogCompositeToyDef defC;
                    defC = _compositeToyBundlesInCatalog[c];

                    if (IsCompositeBundlePresentInScene(defC, svc))
                        continue;

                    choices.Add(CatalogVarietyToyPick.ForComposite(defC));
                }
            }

            List<CatalogVarietyToyPick> bag;
            if (choices.Count > 0)
                bag = choices;
            else
            {
                bag = new List<CatalogVarietyToyPick>();

                if (_catalogToyTemplates != null)
                {
                    foreach (SceneToyTemplate t in _catalogToyTemplates)
                        bag.Add(CatalogVarietyToyPick.ForSingleTemplate(t));
                }

                if (_compositeToyBundlesInCatalog != null)
                {
                    for (int cx = 0; cx < _compositeToyBundlesInCatalog.Count;
                        cx++)
                    {
                        bag.Add(
                            CatalogVarietyToyPick.ForComposite(
                                _compositeToyBundlesInCatalog[cx]));
                    }
                }
            }

            if (bag.Count == 0)
                return null;

            if (bag.Count == 1)
                return bag[0];

            string lastScr;
            lastScr = _lastSceneToySourceId;

            int guard;
            guard = 0;

            while (guard < 128)
            {
                guard++;

                CatalogVarietyToyPick pick;
                pick = bag[UnityEngine.Random.Range(0, bag.Count)];

                if (lastScr == null ||
                    bag.Count <= 1)
                    return pick;

                if (pick.FromCompositeBundle)
                {
                    if (GrpVarietyScratchId(pick.Composite.Key) !=
                        lastScr)
                        return pick;
                }
                else
                {
                    if (pick.SingleToy.SceneAtomId != lastScr)
                        return pick;
                }
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
                    // User: Dildo needs X spin; others read correct without yaw flip.
                    return Quaternion.identity;
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

        private void Update()
        {
            if (_listenEnabled == null || !_listenEnabled.val ||
                _sc == null ||
                _sc.isLoading ||
                _spawnCoroutineRunning)
                return;

            bool xrOn = UnityEngine.XR.XRSettings.enabled ||
                _sc.isOVR ||
                _sc.isOpenVR;
            if (!xrOn)
                return;

            bool leftPressed = false;
            bool rightPressed = false;

            if (_sc.isOVR || _sc.isOpenVR)
            {
                leftPressed = _sc.GetLeftGrab();
                rightPressed = _sc.GetRightGrab();
            }
            else
            {
                try
                {
                    bool swapped = UserPreferences.singleton != null &&
                        UserPreferences.singleton.oculusSwapGrabAndTrigger;

                    if (swapped)
                    {
                        leftPressed =
                            OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger,
                                OVRInput.Controller.LTouch);
                        rightPressed =
                            OVRInput.GetDown(
                                OVRInput.Button.SecondaryHandTrigger,
                                OVRInput.Controller.RTouch);
                    }
                    else
                    {
                        leftPressed =
                            OVRInput.GetDown(
                                OVRInput.Button.PrimaryIndexTrigger,
                                OVRInput.Controller.LTouch);
                        rightPressed =
                            OVRInput.GetDown(
                                OVRInput.Button.SecondaryIndexTrigger,
                                OVRInput.Controller.RTouch);
                    }
                }
                catch
                {
                }
            }

            if (!leftPressed || rightPressed)
                return;

            StartCoroutine(CoSpawnToyAtHand());
        }

        /// <remarks>Handles one singleton whitelist catalog row.</remarks>
        private IEnumerator SpawnCatalogSingleToyCoroutine(
            SceneToyTemplate tmpl)
        {
            SuperController svc;
            svc = SuperController.singleton;

            if (tmpl == null || svc == null)
                yield break;

            JSONClass atomJc;

            atomJc = null;

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

            if (atomJc == null)
                yield break;

            NeutralizeStoredWorldPose(atomJc);
            ZeroSpringControlsInToyJson(atomJc);

            string atomType;
            atomType = tmpl.AtomTypeName;

            string uidCandidate;

            uidCandidate = null;

            int tIdx;
            for (tIdx = 0; tIdx < 32; tIdx++)
            {
                uidCandidate =
                    SpawnUidPrefixForCatalogRow(tmpl) +
                    Mathf.FloorToInt(Time.realtimeSinceStartup * 1000f) +
                    "_" +
                    UnityEngine.Random.Range(
                        100000,
                        999999999).ToString();

                if (svc.GetAtomByUid(uidCandidate) == null)
                    break;

                uidCandidate = null;
            }

            if (uidCandidate == null)
                yield break;

            atomJc["id"] = uidCandidate;

            yield return svc.AddAtomByType(atomType, uidCandidate);

            Atom spawned;
            spawned = svc.GetAtomByUid(uidCandidate);

            if (spawned == null)
                yield break;

            try
            {
                spawned.PreRestore();
                spawned.Restore(atomJc);
                spawned.LateRestore(atomJc);
                spawned.PostRestore();
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

                yield break;
            }

            _waitingMandatoryFirstDildo = false;
            _lastToyAtomTypeSpawned = atomType;
            _lastSceneToySourceId = tmpl.SceneAtomId;

            PlaceSpawnAtHand(spawned, false);
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
                    SceneToyTemplate mandatoryTmpl;

                    mandatoryTmpl = null;

                    if (_waitingMandatoryFirstDildo)
                        mandatoryTmpl = PickMandatoryDildoOrNull();

                    if (_waitingMandatoryFirstDildo &&
                        mandatoryTmpl != null)
                    {
                        IEnumerator singleM;

                        singleM =
                            SpawnCatalogSingleToyCoroutine(mandatoryTmpl);

                        while (singleM.MoveNext())
                            yield return singleM.Current;

                        if (!_waitingMandatoryFirstDildo)
                            yield break;
                    }

                    CatalogVarietyToyPick vpick;

                    vpick = PickVarietyCatalogToyPick();

                    if (vpick != null)
                    {
                        if (vpick.FromCompositeBundle)
                        {
                            IEnumerator compE;

                            compE =
                                CoSpawnCompositeToyBundle(vpick.Composite);

                            while (compE.MoveNext())
                                yield return compE.Current;
                        }
                        else
                        {
                            IEnumerator sgE;

                            sgE =
                                SpawnCatalogSingleToyCoroutine(
                                    vpick.SingleToy);

                            while (sgE.MoveNext())
                                yield return sgE.Current;
                        }

                        yield break;
                    }

                    SuperController.LogError(
                        PluginName +
                            ": catalog clone failed — trying legacy spawn.");
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
                PlaceSpawnAtHand(spawnedLegacy, false);
            }
            finally
            {
                _spawnCoroutineRunning = false;
            }
        }
    }
}



