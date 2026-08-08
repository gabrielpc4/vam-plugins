using System.Collections;
using System.Collections.Generic;
using System.Text;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Optional editor utility (not loaded by SceneControlSuite session bundle). Pivot
    /// handle: spawn a small Cube, parent chosen atoms under it, move the cube
    /// to move the group (VaM atom parent / childAtomContainer). Detach restores
    /// previous parentAtom links saved at attach time.
    ///
    /// Load path: merge <c>MultiAtomPivotHandle.cslist</c> from this folder onto
    /// CoreControl — or another atom — only when needed.
    /// </summary>
    public class MultiAtomPivotHandle : MVRScript
    {
        private const string NoneVal = "None";
        private const char PairSep = '|';
        private const char KeySep = ':';

        private JSONStorableStringChooser pivotAtomUid;
        private JSONStorableStringChooser target1Uid;
        private JSONStorableStringChooser target2Uid;
        private JSONStorableStringChooser target3Uid;
        private JSONStorableStringChooser target4Uid;

        private JSONStorableBool isAttachedJSON;
        /// <summary>
        /// Persisted map: childUid:parentUid pairs, parentUid None = no parent.
        /// </summary>
        private JSONStorableString savedParentMapJSON;

        private JSONStorableFloat handleScaleJSON;

        private JSONStorableAction spawnHandleAction;
        private JSONStorableAction attachAction;
        private JSONStorableAction detachAction;

        private Dictionary<string, string> sessionParentMap;
        private bool subscribedAtomList;
        private Coroutine spawnCo;

        private void EnsureMap()
        {
            if (sessionParentMap == null)
            {
                sessionParentMap =
                    new Dictionary<string, string>();
            }
        }

        public override void Init()
        {
            try
            {
                sessionParentMap =
                    new Dictionary<string, string>();

                pivotAtomUid = new JSONStorableStringChooser(
                    "pivotAtomUid",
                    new List<string>(),
                    NoneVal,
                    "Pivot handle (Cube)")
                {
                    isStorable = true,
                    isRestorable = true
                };
                RegisterStringChooser(pivotAtomUid);

                target1Uid = NewTargetChooser("target1Uid", "Person / object 1");
                target2Uid = NewTargetChooser("target2Uid", "Person / object 2");
                target3Uid = NewTargetChooser("target3Uid", "Person / object 3");
                target4Uid = NewTargetChooser("target4Uid", "Person / object 4");

                isAttachedJSON = new JSONStorableBool(
                    "groupPivotAttached",
                    false)
                {
                    isStorable = true,
                    isRestorable = true
                };
                RegisterBool(isAttachedJSON);

                savedParentMapJSON = new JSONStorableString(
                    "groupPivotSavedParents",
                    "")
                {
                    isStorable = true,
                    isRestorable = true
                };
                RegisterString(savedParentMapJSON);

                handleScaleJSON = new JSONStorableFloat(
                    "handleUniformScale",
                    0.06f,
                    0.02f,
                    0.25f,
                    true,
                    true)
                {
                    isStorable = true,
                    isRestorable = true
                };
                RegisterFloat(handleScaleJSON);

                spawnHandleAction =
                    new JSONStorableAction(
                        "SpawnPivotCube",
                        SpawnPivotCubeFromAction);
                RegisterAction(spawnHandleAction);

                attachAction =
                    new JSONStorableAction(
                        "AttachTargetsToPivot",
                        AttachFromAction);
                RegisterAction(attachAction);

                detachAction =
                    new JSONStorableAction(
                        "DetachTargetsFromPivot",
                        DetachFromAction);
                RegisterAction(detachAction);

                SubscribeAtomList();
                RefreshAllAtomChoosers();
                ParseSavedMapFromJson();

                UIDynamicTextField help =
                    CreateTextField(
                        new JSONStorableString(
                            "Help",
                            "1) Spawn pivot cube (placed in front of the " +
                            "camera). 2) Pick up to four atoms. 3) Attach " +
                            "— they become children of the pivot; move " +
                            "the pivot's main control. 4) Detach restores " +
                            "previous parents. Save the scene to keep the " +
                            "hierarchy; detach still works after reload " +
                            "if this plugin state was saved."),
                        true);
                help.height = 200f;

                CreateScrollablePopup(pivotAtomUid, true);
                CreateScrollablePopup(target1Uid, true);
                CreateScrollablePopup(target2Uid, true);
                CreateScrollablePopup(target3Uid, true);
                CreateScrollablePopup(target4Uid, true);

                CreateSlider(handleScaleJSON, true);
                CreateButton("Spawn pivot cube (Small)")
                    .button.onClick.AddListener(SpawnPivotCubeFromAction);
                CreateButton("Attach group to pivot").button.onClick
                    .AddListener(AttachFromAction);
                CreateButton("Detach group (restore parents)").button.onClick
                    .AddListener(DetachFromAction);
            }
            catch (System.Exception e)
            {
                SuperController.LogError(
                    "MultiAtomPivotHandle Init: " + e);
            }
        }

        void Start()
        {
            RefreshAllAtomChoosers();
        }

        private JSONStorableStringChooser NewTargetChooser(
            string sid,
            string label)
        {
            JSONStorableStringChooser c =
                new JSONStorableStringChooser(
                    sid,
                    new List<string>(),
                    NoneVal,
                    label)
                {
                    isStorable = true,
                    isRestorable = true
                };
            RegisterStringChooser(c);
            return c;
        }

        private void SubscribeAtomList()
        {
            if (subscribedAtomList)
                return;
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;
            sc.onAtomUIDsChangedHandlers -= OnAtomUidsChanged;
            sc.onAtomUIDsChangedHandlers += OnAtomUidsChanged;
            subscribedAtomList = true;
        }

        private void OnAtomUidsChanged(List<string> uids)
        {
            RefreshAllAtomChoosers();
        }

        private void RefreshAllAtomChoosers()
        {
            List<string> choices = BuildAtomChoiceList();
            ApplyChoices(pivotAtomUid, choices);
            ApplyChoices(target1Uid, choices);
            ApplyChoices(target2Uid, choices);
            ApplyChoices(target3Uid, choices);
            ApplyChoices(target4Uid, choices);
        }

        private static List<string> BuildAtomChoiceList()
        {
            List<string> atomChoices = new List<string>();
            atomChoices.Add(NoneVal);
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return atomChoices;

            List<string> uids = sc.GetAtomUIDs();
            int i;
            for (i = 0; i < uids.Count; i++)
                atomChoices.Add(uids[i]);

            return atomChoices;
        }

        private static void ApplyChoices(
            JSONStorableStringChooser chooser,
            List<string> choices)
        {
            if (chooser == null)
                return;
            string preserve = chooser.val;
            chooser.choices = choices;
            if (preserve != null &&
                choices.Contains(preserve))
            {
                chooser.val = preserve;
            }
            else
            {
                chooser.val = NoneVal;
            }
        }

        private void ParseSavedMapFromJson()
        {
            EnsureMap();
            sessionParentMap.Clear();
            string raw = savedParentMapJSON.val;
            if (raw == null || raw.Length == 0)
                return;

            string[] pairs = raw.Split(PairSep);
            int p;
            for (p = 0; p < pairs.Length; p++)
            {
                string pair = pairs[p];
                if (pair == null || pair.Length == 0)
                    continue;
                int sep = pair.IndexOf(KeySep);
                if (sep <= 0 || sep >= pair.Length - 1)
                    continue;
                string child = pair.Substring(0, sep);
                string parent =
                    pair.Substring(sep + 1);
                sessionParentMap[child] = parent;
            }
        }

        private void WriteMapToJson()
        {
            EnsureMap();
            StringBuilder sb = new StringBuilder();
            bool first = true;
            Dictionary<string, string>.Enumerator e =
                sessionParentMap.GetEnumerator();
            while (e.MoveNext())
            {
                if (!first)
                    sb.Append(PairSep);
                first = false;
                sb.Append(e.Current.Key);
                sb.Append(KeySep);
                sb.Append(e.Current.Value);
            }
            savedParentMapJSON.val = sb.ToString();
        }

        private void SpawnPivotCubeFromAction()
        {
            if (spawnCo != null)
            {
                StopCoroutine(spawnCo);
            }
            spawnCo = StartCoroutine(CoSpawnPivotCube());
        }

        private IEnumerator CoSpawnPivotCube()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null)
                yield break;

            // Do not combine GetTempUID() with passing that string into AddAtom —
            // VaM's CreateUID() would treat it as already taken and rename (e.g.
            // append #2), so GetAtomByUid(firstUid) would fail.

            List<string> uidsBefore;
            List<string> uidsAfter;
            Atom cube;
            List<string> snap;

            snap = sc.GetAtomUIDs();
            uidsBefore = CopyUidList(snap);

            yield return sc.AddAtomByType("Cube", null, userInvoked: true);

            snap = sc.GetAtomUIDs();
            uidsAfter = snap != null ? snap : new List<string>();
            cube = PickNewCubeAtom(sc, uidsBefore, uidsAfter);
            if (cube == null)
            {
                SuperController.LogError(
                    "MultiAtomPivotHandle: spawned Cube not found " +
                    "(no new Cube atom appeared).");
                yield break;
            }

            float scale = handleScaleJSON.val;
            cube.transform.localScale =
                new Vector3(scale, scale, scale);

            Transform camT = sc.lookCamera.transform;
            Vector3 pos =
                camT.position + camT.forward * 1.4f;
            cube.transform.position = pos;
            cube.transform.rotation = Quaternion.identity;

            RefreshAllAtomChoosers();
            pivotAtomUid.val = cube.uid;

            spawnCo = null;
        }

        private static List<string> CopyUidList(List<string> source)
        {
            List<string> copy;
            int i;

            copy = new List<string>();
            if (source == null)
                return copy;

            for (i = 0; i < source.Count; i++)
                copy.Add(source[i]);

            return copy;
        }

        /// <summary>
        /// Locate the Cube added by diffing UID lists — AddAtom resolves the
        /// final uid internally (avoid GetTempUID + CreateUID rename mismatch).
        /// </summary>
        private static Atom PickNewCubeAtom(
            SuperController sc,
            List<string> beforeSnap,
            List<string> afterSnap)
        {
            Atom cand;
            Atom lastCube;
            Atom loneNew;
            int ai;
            int bi;
            int newAtoms;
            bool wasBefore;
            string uid;

            lastCube = null;
            loneNew = null;
            newAtoms = 0;

            if (afterSnap == null || sc == null)
                return null;

            for (ai = 0; ai < afterSnap.Count; ai++)
            {
                uid = afterSnap[ai];
                if (uid == null)
                    continue;
                wasBefore = false;
                if (beforeSnap != null)
                {
                    for (bi = 0; bi < beforeSnap.Count; bi++)
                    {
                        if (beforeSnap[bi] == uid)
                        {
                            wasBefore = true;
                            break;
                        }
                    }
                }
                if (wasBefore)
                    continue;

                cand = sc.GetAtomByUid(uid);
                if (cand == null)
                    continue;

                newAtoms++;
                loneNew = cand;

                if (cand.type != null &&
                    cand.type.Equals(
                        "Cube",
                        System.StringComparison.OrdinalIgnoreCase))
                {
                    lastCube = cand;
                }
            }

            if (lastCube != null)
                return lastCube;

            if (newAtoms == 1)
                return loneNew;

            return null;
        }

        private void AttachFromAction()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            if (isAttachedJSON.val)
            {
                SuperController.LogMessage(
                    "MultiAtomPivotHandle: already attached. Detach first.");
                return;
            }

            Atom pivot = ResolveAtom(pivotAtomUid.val);
            if (pivot == null)
            {
                SuperController.LogMessage(
                    "MultiAtomPivotHandle: choose or spawn a pivot atom.");
                return;
            }

            List<Atom> targets = CollectTargetsDistinct();
            if (targets.Count == 0)
            {
                SuperController.LogMessage(
                    "MultiAtomPivotHandle: pick at least one target.");
                return;
            }

            int t;
            for (t = 0; t < targets.Count; t++)
            {
                if (targets[t] == pivot)
                {
                    SuperController.LogMessage(
                        "MultiAtomPivotHandle: pivot cannot be a target.");
                    return;
                }
            }

            EnsureMap();
            sessionParentMap.Clear();

            for (t = 0; t < targets.Count; t++)
            {
                Atom child = targets[t];
                Atom prevParent = child.parentAtom;
                string parentKey;
                if (prevParent != null)
                    parentKey = prevParent.uid;
                else
                    parentKey = NoneVal;
                sessionParentMap[child.uid] = parentKey;
                child.parentAtom = pivot;
            }

            WriteMapToJson();
            isAttachedJSON.val = true;

            SuperController.LogMessage(
                "MultiAtomPivotHandle: attached " +
                targets.Count +
                " atoms to " +
                pivot.uid +
                ". Move the pivot atom's control.");
        }

        private void DetachFromAction()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || sc.isLoading)
                return;

            if (!isAttachedJSON.val)
                return;

            ParseSavedMapFromJson();
            EnsureMap();

            Dictionary<string, string>.Enumerator e =
                sessionParentMap.GetEnumerator();
            while (e.MoveNext())
            {
                string childUid = e.Current.Key;
                string parentUid = e.Current.Value;
                Atom child = sc.GetAtomByUid(childUid);
                if (child == null)
                    continue;

                if (parentUid == null || parentUid == NoneVal)
                {
                    child.parentAtom = null;
                }
                else
                {
                    Atom pa = sc.GetAtomByUid(parentUid);
                    child.parentAtom = pa;
                }
            }

            sessionParentMap.Clear();
            savedParentMapJSON.val = "";
            isAttachedJSON.val = false;

            SuperController.LogMessage(
                "MultiAtomPivotHandle: detached; parents restored.");
        }

        private List<Atom> CollectTargetsDistinct()
        {
            List<Atom> result = new List<Atom>();
            AddIfValidTarget(result, target1Uid.val);
            AddIfValidTarget(result, target2Uid.val);
            AddIfValidTarget(result, target3Uid.val);
            AddIfValidTarget(result, target4Uid.val);
            return result;
        }

        private static void AddIfValidTarget(
            List<Atom> bucket,
            string uid)
        {
            if (uid == null || uid == NoneVal)
                return;
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return;
            Atom at = sc.GetAtomByUid(uid);
            if (at == null)
                return;
            int i;
            for (i = 0; i < bucket.Count; i++)
            {
                if (bucket[i].uid == uid)
                    return;
            }
            bucket.Add(at);
        }

        private static Atom ResolveAtom(string uid)
        {
            if (uid == null || uid == NoneVal)
                return null;
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return null;
            return sc.GetAtomByUid(uid);
        }

        void OnDestroy()
        {
            SuperController sc = SuperController.singleton;
            if (sc != null && subscribedAtomList)
            {
                sc.onAtomUIDsChangedHandlers -= OnAtomUidsChanged;
            }
            subscribedAtomList = false;
        }
    }
}
