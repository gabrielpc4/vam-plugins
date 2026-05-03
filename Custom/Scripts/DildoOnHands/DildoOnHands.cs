using System;
using System.Collections;
using System.Collections.Generic;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VR Grab spawns atoms at the hand pose (VaM Grab = index trigger). First
    /// successful spawn each session is always Dildo. Later picks a different
    /// type than last from built-ins plus lines in Extra toy atom types (paste
    /// names from VaM Add Atom / Toys popup on your PC). Custom/Assets in this
    /// repo bundle list has no extra toy defs. OVR LT/RT fallback when Oculus
    /// paths are inactive.
    /// </summary>
    public class DildoOnHands : MVRScript
    {
        public const string PluginName = "HandSpawnToy";

        private static readonly string[] VarietyToyAtomTypes =
        {
            "Dildo",
            "ToyAH",
            "ToyBP",
            "Paddle",
        };

        private SuperController _sc;

        private JSONStorableBool _listenEnabled;

        private JSONStorableFloat _localOffsetForward;

        private JSONStorableFloat _localOffsetRight;

        private JSONStorableFloat _localOffsetUp;

        private JSONStorableFloat _localEulerPitchDeg;

        private JSONStorableFloat _localEulerYawDeg;

        private JSONStorableFloat _localEulerRollDeg;

        /// <summary>Optional AddAtom type names one per line (VAR / menu).</summary>
        private JSONStorableString _extraToyAtomTypes;

        private bool _spawnCoroutineRunning;

        /// <summary>First trigger after Init must spawn Dildo only.</summary>
        private bool _waitingMandatoryFirstDildo = true;

        private string _lastToyAtomTypeSpawned;

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

                _extraToyAtomTypes = new JSONStorableString(
                    "Extra toy atom types (one name per line)",
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

        /// <summary>Built-in suspects plus trimmed lines from storables UI.</summary>
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
                lines = _extraToyAtomTypes.val.Split(new char[] {
                    '\r',
                    '\n'
                }, StringSplitOptions.RemoveEmptyEntries);

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

        /// <summary>Random pool entry unlike prior successful spawn.</summary>
        private string PickRandomToyDifferentFromLast()
        {
            List<string> pool;
            pool = BuildVarietyToyTypePool();
            if (pool.Count == 0)
                return "Dildo";

            string last;
            last = _lastToyAtomTypeSpawned;
            int guard;
            guard = 0;
            while (guard < 64)
            {
                string candidate;
                candidate =
                    pool[UnityEngine.Random.Range(0, pool.Count)];
                guard++;
                if (pool.Count <= 1)
                    return candidate;
                if (last == null || candidate != last)
                    return candidate;
            }

            string fallback;
            fallback = pool[0];
            if (pool.Count <= 1)
                return fallback;
            if (fallback != last)
                return fallback;
            return pool[1];
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

            Vector3 worldPos =
                hand.TransformPoint(new Vector3(
                    _localOffsetRight.val,
                    _localOffsetUp.val,
                    _localOffsetForward.val));

            Quaternion worldRot = hand.rotation * LocalGripOffsetQuaternion();

            fc.currentPositionState = FreeControllerV3.PositionState.On;
            fc.currentRotationState = FreeControllerV3.RotationState.On;

            fc.transform.rotation = worldRot;
            fc.transform.position = worldPos;
        }

        private void Update()
        {
            if (_listenEnabled == null || !_listenEnabled.val || _sc == null ||
                _sc.isLoading || _spawnCoroutineRunning)
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

            bool leftOnly = leftPressed && !rightPressed;
            bool rightOnly = rightPressed && !leftPressed;
            bool dual = leftPressed && rightPressed;

            if (!leftOnly && !rightOnly && !dual)
                return;

            bool pickLeft;
            if (rightOnly)
                pickLeft = false;
            else
                pickLeft = true;

            StartCoroutine(CoSpawnToyAtHand(pickLeft));
        }

        private IEnumerator CoSpawnToyAtHand(bool leftHandPreferred)
        {
            _spawnCoroutineRunning = true;
            try
            {
                bool consumedMandatoryFirst;
                consumedMandatoryFirst = false;

                string atomType;
                if (_waitingMandatoryFirstDildo)
                {
                    atomType = "Dildo";
                    consumedMandatoryFirst = true;
                }
                else
                {
                    atomType = PickRandomToyDifferentFromLast();
                }

                string uid;
                Atom clash;
                int tries = 0;
                SuperController svc = SuperController.singleton;
                do
                {
                    uid = PluginName + "_" +
                        Mathf.FloorToInt(Time.realtimeSinceStartup * 1000f) +
                        "_" +
                        UnityEngine.Random.Range(100000, 999999999).ToString();
                    clash = svc.GetAtomByUid(uid);
                    tries++;
                    if (tries > 32)
                    {
                        if (consumedMandatoryFirst)
                            _waitingMandatoryFirstDildo = true;
                        yield break;
                    }
                }
                while (clash != null);

                yield return svc.AddAtomByType(atomType, uid);

                Atom spawned = svc.GetAtomByUid(uid);
                if (spawned == null)
                {
                    SuperController.LogError(
                        PluginName +
                        ": failed to spawn toy atom '" +
                        atomType +
                        "'.");

                    if (consumedMandatoryFirst)
                        _waitingMandatoryFirstDildo = true;

                    yield break;
                }

                _waitingMandatoryFirstDildo = false;
                _lastToyAtomTypeSpawned = atomType;
                PlaceSpawnAtHand(spawned, leftHandPreferred);
            }
            finally
            {
                _spawnCoroutineRunning = false;
            }
        }
    }
}
