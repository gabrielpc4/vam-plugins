using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Starts when grip first enables Male2 hands. Polls at a low rate until any
    /// controller hand gets very close to any female body, then merges Spankings
    /// onto females missing it and retries once after 4 seconds if needed.
    /// </summary>
    internal static class EasyMateSpankingsHandNearAutoLoad
    {
        private const float PollSeconds = 0.2f;

        private const float TriggerDistanceMeters = 0.16f;

        private static readonly string SpankingsFileName =
            GetFileName(MainUIButtons.PluginSpankings);

        private static MVRScript _owner;

        private static MainUIButtons _mainUIButtons;

        private static Coroutine _proximityCoroutine;

        private static bool _completedThisScene;

        public static void Initialize(
            MVRScript owner,
            MainUIButtons mainUIButtons)
        {
            _owner = owner;
            _mainUIButtons = mainUIButtons;
        }

        public static void ResetForScene()
        {
            _completedThisScene = false;
            StopWatcher();
        }

        public static void OnDestroy()
        {
            StopWatcher();
            _owner = null;
            _mainUIButtons = null;
            _completedThisScene = false;
        }

        public static void NotifyMale2HandsEnabled()
        {
            if (_owner == null || _mainUIButtons == null ||
                _completedThisScene)
            {
                return;
            }

            if (EasyMateSpankingsGripBlockPathKeywords
                .CurrentSceneBlocksGripSpankingsMerge())
            {
                _completedThisScene = true;
                return;
            }

            if (!EasyMateGripHandVisibility.IsAnyPreferredHandArticulated())
                return;

            if (!AnyFemalePersonMissingSpankings())
            {
                _completedThisScene = true;
                return;
            }

            if (_proximityCoroutine != null)
                return;

            _proximityCoroutine = _owner.StartCoroutine(
                CoWaitForHandNearFemaleThenMergeSpankings());
        }

        private static void StopWatcher()
        {
            if (_owner == null || _proximityCoroutine == null)
                return;

            _owner.StopCoroutine(_proximityCoroutine);
            _proximityCoroutine = null;
        }

        private static IEnumerator CoWaitForHandNearFemaleThenMergeSpankings()
        {
            try
            {
                while (true)
                {
                    if (_owner == null || _mainUIButtons == null ||
                        _completedThisScene)
                    {
                        yield break;
                    }

                    if (EasyMateSpankingsGripBlockPathKeywords
                        .CurrentSceneBlocksGripSpankingsMerge())
                    {
                        _completedThisScene = true;
                        yield break;
                    }

                    if (!EasyMateGripHandVisibility
                        .IsAnyPreferredHandArticulated())
                    {
                        yield break;
                    }

                    if (!AnyFemalePersonMissingSpankings())
                    {
                        _completedThisScene = true;
                        yield break;
                    }

                    if (!AnyFemalePersonWithinControllerHandDistance(
                        TriggerDistanceMeters))
                    {
                        yield return new WaitForSeconds(PollSeconds);
                        continue;
                    }

                    _mainUIButtons.MergeSpankingsOnFemalePersonsOnly();
                    yield return new WaitForSeconds(4f);

                    if (_mainUIButtons == null)
                        yield break;

                    if (EasyMateSpankingsGripBlockPathKeywords
                        .CurrentSceneBlocksGripSpankingsMerge())
                    {
                        _completedThisScene = true;
                        yield break;
                    }

                    if (!EasyMateGripHandVisibility
                        .IsAnyPreferredHandArticulated())
                    {
                        yield break;
                    }

                    if (AnyFemalePersonMissingSpankings())
                    {
                        _mainUIButtons.MergeSpankingsOnFemalePersonsOnly();
                        yield return new WaitForSeconds(PollSeconds);
                        continue;
                    }

                    _completedThisScene = true;
                    yield break;
                }
            }
            finally
            {
                _proximityCoroutine = null;
            }
        }

        private static bool AnyFemalePersonMissingSpankings()
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;

                foreach (Atom at in sc.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null || !IsPersonFemale(at))
                        continue;
                    if (!PersonHasPluginByFileName(at, SpankingsFileName))
                        return true;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "EasyMate Spankings missing-check: " + e.Message);
            }

            return false;
        }

        private static bool AnyFemalePersonWithinControllerHandDistance(
            float maxDistanceMeters)
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null)
                    return false;

                foreach (Atom at in sc.GetAtoms().Where(a => a.type == "Person"))
                {
                    if (at == null || !IsPersonFemale(at))
                        continue;
                    if (GetMinControllerHandDistanceToPersonBody(at) <=
                        maxDistanceMeters)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError(
                    "EasyMate Spankings hand-near check: " + e.Message);
            }

            return false;
        }

        private static float GetMinControllerHandDistanceToPersonBody(
            Atom person)
        {
            if (person == null)
                return float.MaxValue;

            SuperController sc = SuperController.singleton;
            if (sc == null)
                return float.MaxValue;

            Rigidbody[] rigidbodies = person.linkableRigidbodies;
            if (rigidbodies == null || rigidbodies.Length == 0)
                rigidbodies = person.rigidbodies;
            if (rigidbodies == null || rigidbodies.Length == 0)
                return float.MaxValue;

            float best = float.MaxValue;
            UpdateMinDistanceToRigidbodies(sc.leftHand, rigidbodies, ref best);
            UpdateMinDistanceToRigidbodies(sc.rightHand, rigidbodies, ref best);
            UpdateMinDistanceToRigidbodies(
                sc.leftHandAlternate,
                rigidbodies,
                ref best);
            UpdateMinDistanceToRigidbodies(
                sc.rightHandAlternate,
                rigidbodies,
                ref best);
            return best;
        }

        private static void UpdateMinDistanceToRigidbodies(
            Transform hand,
            Rigidbody[] rigidbodies,
            ref float best)
        {
            if (!TransformUsable(hand) || rigidbodies == null)
                return;

            Vector3 hp = hand.position;
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                Rigidbody rb = rigidbodies[i];
                if (rb == null)
                    continue;
                float d = Vector3.Distance(hp, rb.position);
                if (d < best)
                    best = d;
            }
        }

        private static bool TransformUsable(Transform t)
        {
            return t != null && t.gameObject != null &&
                t.gameObject.activeInHierarchy;
        }

        private static bool IsPersonFemale(Atom a)
        {
            if (a == null || a.type != "Person")
                return false;
            DAZCharacter d = a.GetComponentInChildren<DAZCharacter>();
            return d != null && !d.isMale;
        }

        private static bool PersonHasPluginByFileName(
            Atom at,
            string desiredFileName)
        {
            MVRPluginManager manager =
                at.GetStorableByID("PluginManager") as MVRPluginManager;
            if (manager == null)
                return false;

            List<string> paths = CollectNormalizedPluginPaths(manager);
            for (int i = 0; i < paths.Count; i++)
            {
                if (GetFileName(paths[i]) == desiredFileName)
                    return true;
            }

            return false;
        }

        private static List<string> CollectNormalizedPluginPaths(
            MVRPluginManager manager)
        {
            var paths = new List<string>();
            JSONClass current = manager.GetJSON(true, true, true);
            JSONNode plugins = current["plugins"];
            if (plugins == null)
                return paths;

            for (int i = 0; ; i++)
            {
                JSONNode node = plugins["plugin#" + i];
                if (node == null)
                    break;

                string path = node.Value;
                if (string.IsNullOrEmpty(path))
                    continue;
                paths.Add(path.Replace('\\', '/'));
            }

            return paths;
        }

        private static string GetFileName(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return "";
            int slash = relativePath.LastIndexOf('/');
            if (slash >= 0 && slash + 1 < relativePath.Length)
                return relativePath.Substring(slash + 1);
            return relativePath;
        }
    }
}
