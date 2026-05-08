using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Person plugin: periodically, if a SuperController hand transform is near this Person, enables cloth "fall off"
    /// (<see cref="DAZCharacterSelector.EnableUndressOnClothingItem"/>) for garments that do not yet have break on all
    /// <see cref="ClothSimControl"/>. Easy Mate merges this onto every Person after a scene load.
    /// </summary>
    public class ClothingTouchFallOff : MVRScript
    {
        private const float RetouchCooldownSeconds = 0.35f;
        private const float ProximityCheckIntervalSeconds = 5f;
        private const float HandNearPersonRadiusMeters = 0.5f;

        private DAZCharacterSelector _selector;
        private Atom _person;
        private float _nextProximityCheckTime;

        /// <summary>When clothing slot count and active count are unchanged, skip heavy scans until clothing changes.</summary>
        private int _lastClothingSlots = -1;
        private int _lastActiveClothingCount = -1;
        /// <summary>True when there is no active clothing, or every sim-capable garment already has break/fall-off enabled.</summary>
        private bool _noFallOffWorkKnown;
        /// <summary>So we re-run garment scan when hand collision turns on after being off (idle cache would otherwise skip).</summary>
        private bool _lastTickHandCollision;

        private readonly List<Vector3> _proximityAnchorsScratch = new List<Vector3>(6);

        /// <summary>One GetComponents pass: active garment with at least one cloth sim that still needs break enabled.</summary>
        private static bool GarmentHasClothSimNeedingBreakEnabled(DAZClothingItem item)
        {
            if (item == null || !item.active)
                return false;
            ClothSimControl[] sims = item.GetComponentsInChildren<ClothSimControl>(true);
            if (sims == null || sims.Length == 0)
                return false;
            for (int i = 0; i < sims.Length; i++)
            {
                ClothSimControl c = sims[i];
                if (c == null || c.clothSettings == null)
                    continue;
                if (!c.clothSettings.BreakEnabled)
                    return true;
            }

            return false;
        }

        public static bool IsFallOffAlreadyActiveOnGarment(DAZClothingItem item)
        {
            if (item == null || !item.active)
                return true;
            return !GarmentHasClothSimNeedingBreakEnabled(item);
        }

        /// <summary>Cooldown + idempotency so sustained contact does not spam.</summary>
        public static bool TryEnableFallOffOnGarment(
            DAZCharacterSelector selector,
            DAZClothingItem item,
            Atom personAtom,
            Dictionary<string, float> cooldownByKey,
            float now)
        {
            if (selector == null || item == null || personAtom == null)
                return false;
            if (!item.active)
                return false;
            if (IsFallOffAlreadyActiveOnGarment(item))
                return false;

            string key = personAtom.uid + "::" + (string.IsNullOrEmpty(item.uid) ? item.name : item.uid);
            float last;
            if (cooldownByKey.TryGetValue(key, out last) && now - last < RetouchCooldownSeconds)
                return false;

            try
            {
                selector.EnableUndressOnClothingItem(item);
                cooldownByKey[key] = now;
                return true;
            }
            catch (Exception e)
            {
                SuperController.LogError("ClothingTouchFallOff: EnableUndressOnClothingItem failed: " + e.Message);
                return false;
            }
        }

        public override void Init()
        {
            try
            {
                if (containingAtom == null || containingAtom.type != "Person")
                {
                    SuperController.LogError("ClothingTouchFallOff: apply only to a Person atom.");
                    return;
                }

                _person = containingAtom;
                _selector = containingAtom.GetStorableByID("geometry") as DAZCharacterSelector;

                if (_selector == null)
                {
                    SuperController.LogError("ClothingTouchFallOff: no DAZCharacterSelector (geometry) on " + containingAtom.uid);
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("ClothingTouchFallOff Init: " + e);
            }
        }

        private void Start()
        {
            DestroyLegacyRelays();
            _nextProximityCheckTime = 0f;
        }

        private void Update()
        {
            if (_person == null || _selector == null)
                return;
            if (Time.time < _nextProximityCheckTime)
                return;
            _nextProximityCheckTime = Time.time + ProximityCheckIntervalSeconds;

            DAZClothingItem[] items = _selector.clothingItems;
            int slots = items != null ? items.Length : 0;
            int activeCount = QuickCountActiveClothing(items);
            bool slotsChanged = slots != _lastClothingSlots;
            bool activeChanged = activeCount != _lastActiveClothingCount;
            _lastClothingSlots = slots;
            _lastActiveClothingCount = activeCount;

            if (activeCount == 0)
            {
                _noFallOffWorkKnown = true;
                _lastTickHandCollision = false;
                return;
            }

            SuperController sc = SuperController.singleton;
            bool collisionNow = IsHandCollisionEnabled(sc);
            if (!collisionNow)
            {
                _lastTickHandCollision = false;
                return;
            }

            bool collisionBecameTrue = collisionNow && !_lastTickHandCollision;
            _lastTickHandCollision = true;

            if (_noFallOffWorkKnown && !slotsChanged && !activeChanged && !collisionBecameTrue)
                return;

            if (!AnyGarmentNeedsFallOffEnableScan(items))
            {
                _noFallOffWorkKnown = true;
                return;
            }

            _noFallOffWorkKnown = false;

            float minHandM = GetMinControllerHandToPersonBodyDistanceMeters();
            bool handsNear = minHandM <= HandNearPersonRadiusMeters;

            if (!handsNear)
                return;

            if (items == null)
                return;

            float now = Time.time;
            for (int i = 0; i < items.Length; i++)
            {
                DAZClothingItem item = items[i];
                if (item == null || !item.active)
                    continue;
                TryEnableFallOffOnGarment(_selector, item, _person, _cooldownByGarmentKey, now);
            }
        }

        private void OnDestroy()
        {
            DestroyLegacyRelays();
        }

        private Vector3 GetPersonApproximateWorldPosition()
        {
            if (_person == null)
                return Vector3.zero;
            if (_person.mainController != null && _person.mainController.transform != null)
                return _person.mainController.transform.position;
            return _person.transform.position;
        }

        private static bool TransformUsable(Transform t)
        {
            return t != null && t.gameObject.activeInHierarchy;
        }

        private static int QuickCountActiveClothing(DAZClothingItem[] items)
        {
            if (items == null)
                return 0;
            int n = 0;
            for (int i = 0; i < items.Length; i++)
            {
                DAZClothingItem x = items[i];
                if (x != null && x.active)
                    n++;
            }

            return n;
        }

        private static bool AnyGarmentNeedsFallOffEnableScan(DAZClothingItem[] items)
        {
            if (items == null)
                return false;
            for (int i = 0; i < items.Length; i++)
            {
                if (GarmentHasClothSimNeedingBreakEnabled(items[i]))
                    return true;
            }

            return false;
        }

        private static bool IsHandCollisionEnabled(SuperController sc)
        {
            if (sc == null)
                return false;
            MeshVR.HandModelControl common = sc.commonHandModelControl;
            if (common != null && common.useCollision)
                return true;
            MeshVR.HandModelControl alt = sc.alternateControllerHandModelControl;
            return alt != null && alt.useCollision;
        }

        private void CollectPersonProximityAnchors(List<Vector3> anchors)
        {
            anchors.Clear();
            if (_person == null)
                return;

            anchors.Add(GetPersonApproximateWorldPosition());

            FreeControllerV3 head = _person.GetStorableByID("headControl") as FreeControllerV3;
            if (head != null && head.followWhenOff != null)
                anchors.Add(head.followWhenOff.position);

            FreeControllerV3 chest = _person.GetStorableByID("chest") as FreeControllerV3;
            if (chest != null && chest.followWhenOff != null)
                anchors.Add(chest.followWhenOff.position);
            else if (chest != null && chest.control != null)
                anchors.Add(chest.control.position);
        }

        private static void UpdateMinDistanceHandToNearestAnchor(ref float best, List<Vector3> anchors, Transform hand)
        {
            if (!TransformUsable(hand) || anchors == null || anchors.Count == 0)
                return;
            float closest = float.MaxValue;
            for (int i = 0; i < anchors.Count; i++)
            {
                float d = Vector3.Distance(hand.position, anchors[i]);
                if (d < closest)
                    closest = d;
            }
            if (closest < best)
                best = closest;
        }

        /// <summary>Minimum world distance from any usable controller hand to the nearest of several Person body anchors (hip, head, chest).</summary>
        private float GetMinControllerHandToPersonBodyDistanceMeters()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || _person == null)
                return float.MaxValue;

            CollectPersonProximityAnchors(_proximityAnchorsScratch);
            if (_proximityAnchorsScratch.Count == 0)
                return float.MaxValue;

            float best = float.MaxValue;
            UpdateMinDistanceHandToNearestAnchor(ref best, _proximityAnchorsScratch, sc.leftHand);
            UpdateMinDistanceHandToNearestAnchor(ref best, _proximityAnchorsScratch, sc.rightHand);
            UpdateMinDistanceHandToNearestAnchor(ref best, _proximityAnchorsScratch, sc.leftHandAlternate);
            UpdateMinDistanceHandToNearestAnchor(ref best, _proximityAnchorsScratch, sc.rightHandAlternate);

            return best;
        }

        private void DestroyLegacyRelays()
        {
            if (_person == null)
                return;
            ClothingFallOffRelay[] all = _person.GetComponentsInChildren<ClothingFallOffRelay>(true);
            for (int k = 0; k < all.Length; k++)
            {
                if (all[k] != null)
                    UnityEngine.Object.Destroy(all[k]);
            }
        }

        private readonly Dictionary<string, float> _cooldownByGarmentKey = new Dictionary<string, float>();
    }

    /// <summary>Versão antiga: relay em rigidbody de roupa. Mantida vazia para Unity remover componentes antigos em cena.</summary>
    public sealed class ClothingFallOffRelay : MonoBehaviour
    {
    }

    /// <summary>
    /// Defers ClothingTouchFallOff merge on first Male2 VR hand grip unless the scene has
    /// a qualifying long non-loop main motion timeline (matches Default.json-after-
    /// animation heuristic).
    /// </summary>
    internal static class ClothingTouchFallOffGripMerge
    {
        internal static IEnumerator CoMergeAfterGripDeferred(
            float minNonLoopAnimationClipSeconds,
            GabrielHudButtons buttons)
        {
            try
            {
                yield return null;
                yield return null;
                if (buttons == null)
                    yield break;
                if (AnimationNoLoopDetection
                    .CurrentSceneUsesLongNonLoopAnimation(
                        minNonLoopAnimationClipSeconds))
                    yield break;
                buttons.MergeClothingTouchFallOffOnAllPersonsOnly();
                buttons.RefreshPluginToggleLabels();
            }
            finally
            {
            }
        }
    }
}
