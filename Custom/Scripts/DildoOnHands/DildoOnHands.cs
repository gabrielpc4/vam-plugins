using System;
using System.Collections;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// VR: VaM Grab (= index trigger on Quest / Oculus Touch SteamVR) adds one
    /// vanilla Dildo atom if none exist, placed once at the triggering-hand
    /// pose. If XR runs without isOVR/isOpenVR, uses LT/RT index-trigger OVR
    /// presses as fallback.
    /// </summary>
    public class DildoOnHands : MVRScript
    {
        public const string PluginName = "DildoOnHands";

        private SuperController _sc;

        private JSONStorableBool _listenEnabled;

        private JSONStorableFloat _localOffsetForward;

        private JSONStorableFloat _localOffsetRight;

        private JSONStorableFloat _localOffsetUp;

        private JSONStorableFloat _localEulerPitchDeg;

        private JSONStorableFloat _localEulerYawDeg;

        private JSONStorableFloat _localEulerRollDeg;

        private bool _spawnCoroutineRunning;

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
            }
            catch (Exception e)
            {
                SuperController.LogError(PluginName + " Init: " + e);
            }
        }

        private bool SceneHasAnyDildoAlready()
        {
            if (_sc == null)
                return true;
            try
            {
                foreach (Atom a in _sc.GetAtoms())
                {
                    if (a == null || a.type != "Dildo")
                        continue;
                    return true;
                }
            }
            catch
            {
            }

            return false;
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

            if (SceneHasAnyDildoAlready())
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

            StartCoroutine(CoSpawnDildoAtHand(pickLeft));
        }

        private IEnumerator CoSpawnDildoAtHand(bool leftHandPreferred)
        {
            _spawnCoroutineRunning = true;
            try
            {
                if (SceneHasAnyDildoAlready())
                    yield break;

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
                        yield break;
                }
                while (clash != null);

                yield return svc.AddAtomByType("Dildo", uid);

                Atom spawned = svc.GetAtomByUid(uid);
                if (spawned == null)
                {
                    SuperController.LogError(
                        PluginName + ": failed to spawn Dildo.");

                    yield break;
                }

                PlaceSpawnAtHand(spawned, leftHandPreferred);
            }
            finally
            {
                _spawnCoroutineRunning = false;
            }
        }
    }
}
