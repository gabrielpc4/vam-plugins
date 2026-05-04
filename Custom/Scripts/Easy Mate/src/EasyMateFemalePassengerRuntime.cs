using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Female &quot;Be the girl&quot; mode: navigation rig follows the model head
    /// <b>position</b> (Passenger-style); <b>rotation</b> aligns to the head only on
    /// the first activation frame, then stays independent so the model can turn without
    /// dragging the rig&apos;s yaw/pitch/roll. ImprovedPoV setup, deferred VR hand possession.
    /// </summary>
    internal static class EasyMateFemalePassengerRuntime
    {
        private const string ImprovedPoVPluginPath =
            "Custom/Scripts/ImprovedPoV.cs";
        private const string ImprovedPoVClassSuffix = "ImprovedPoV";

        private const float RotationSmoothingSeconds = 0.1985169f;
        private const float RotationOffsetXDegrees = 15.17952f;
        private const float PositionSmoothingSeconds = 0.05345887f;
        private const float PositionOffsetZMeters = 0.1149023f;
        private const float PendingActivationTimeoutSeconds = 3f;

        private const float PassengerHandsStartDelaySeconds = 3f;

        private const float PalmHudHideSecondsAfterHandsPossessTrigger = 5f;

        private static MVRScript _sessionPluginHost;

        private static Coroutine _femalePassengerHandsDelayCoroutine;

        private static bool _isFemalePassengerModeActive;
        private static Atom _femalePassengerTargetPerson;
        private static Rigidbody _femalePassengerHeadRigidbody;
        private static Possessor _possessor;
        private static Vector3 _previousNavigationRigPosition;
        private static Quaternion _previousNavigationRigRotation;
        private static float _previousPlayerHeightAdjust;
        private static Quaternion _currentRotationVelocity =
            Quaternion.identity;
        private static Vector3 _currentPositionVelocity = Vector3.zero;

        private static string _pendingPassengerModeTargetUid;
        private static float _pendingPassengerModeDeadlineTime;

        private static float _palmHandHudAllowedAfterTime =
            -1f;

        public static bool IsFemalePassengerModeActiveOrPending()
        {
            return _isFemalePassengerModeActive || !string.IsNullOrEmpty(_pendingPassengerModeTargetUid);
        }

        /// <summary>
        /// Palm HUD stays hidden until <see cref="PalmHudHideSecondsAfterHandsPossessTrigger"/>
        /// after <see cref="NotifyPassengerHandsPossessionTriggeredForPalmHud"/> runs
        /// (when VR hand possession routine starts).
        /// </summary>
        internal static bool IsPalmHandHudBlockedAfterPassengerHandsTrigger()
        {
            if (_palmHandHudAllowedAfterTime < 0f)
            {
                return false;
            }

            return Time.time < _palmHandHudAllowedAfterTime;
        }

        internal static void NotifyPassengerHandsPossessionTriggeredForPalmHud()
        {
            _palmHandHudAllowedAfterTime =
                Time.time + PalmHudHideSecondsAfterHandsPossessTrigger;
        }

        private static void ClearPalmHandHudPassengerTriggerCooldown()
        {
            _palmHandHudAllowedAfterTime = -1f;
        }

        public static void NotifySceneChanged(MVRScript host)
        {
            if (host != null)
            {
                _sessionPluginHost = host;
            }

            MainUIButtons.StopVrPassengerHandsRoutine();

            SuperController superController = SuperController.singleton;
            if (superController != null && !superController.isLoading)
            {
                try
                {
                    superController.ClearPossess();
                }
                catch (Exception exception)
                {
                    SuperController.LogError(
                        "Easy Mate passenger scene change clear failed: " +
                        exception.Message);
                }
            }

            EasyMatePassengerHandPrePossessSnapshot.DiscardSnapshot();
            EasyMatePassengerPossessableNarrow.Restore();

            StopPassengerMode();
            ClearPendingPassengerModeActivation();
        }

        public static void NotifyAtomUidsChanged(
            List<string> atomUids,
            MVRScript host)
        {
            if (host != null)
            {
                _sessionPluginHost = host;
            }
        }

        public static void Tick(MVRScript host)
        {
            if (host != null)
            {
                _sessionPluginHost = host;
            }

            SuperController superController = SuperController.singleton;
            if (superController == null || superController.isLoading)
            {
                return;
            }

            TryProcessPendingPassengerModeStartup(superController);

            if (_isFemalePassengerModeActive)
            {
                UpdatePassengerRuntime(superController);
            }
        }

        public static void RequestStartForFemale(Atom femalePerson)
        {
            if (!IsFemalePerson(femalePerson))
            {
                SuperController.LogError(
                    "Easy Mate Be the girl: missing female Person target.");
                return;
            }

            RequestStopForPalmHud();

            JSONStorable improvedPoVStorable = FindPluginStorableByClassSuffix(
                femalePerson,
                ImprovedPoVClassSuffix);

            if (improvedPoVStorable == null)
            {
                MainUIButtons.TryMergePluginOntoPerson(
                    femalePerson,
                    ImprovedPoVPluginPath);
                QueuePassengerModeUntilImprovedPoVReady(femalePerson.uid);
                return;
            }

            PrepareImprovedPoVForPassenger(improvedPoVStorable);
            ActivatePassengerForPerson(femalePerson);
            SchedulePassengerHandsAfterHeadFollowingDelay(femalePerson);
        }

        public static void RequestStopForPalmHud()
        {
            ClearPendingPassengerModeActivation();
            CancelPassengerHandsDelayCoroutine();
            MainUIButtons.StopVrPassengerHandsRoutine();

            StopPassengerMode();

            SuperController superController = SuperController.singleton;
            if (superController != null)
            {
                try
                {
                    superController.ClearPossess();
                }
                catch (Exception exception)
                {
                    SuperController.LogError(
                        "Easy Mate passenger stop clear failed: " +
                        exception.Message);
                }
            }

            EasyMatePassengerHandPrePossessSnapshot.RestoreAfterPossessClearThenDiscardSnapshot();

            EasyMateGripHandVisibility.DisableVrHandModelsForSceneStart();
        }

        public static void StopPassengerMode()
        {
            ClearPendingPassengerModeActivation();
            CancelPassengerHandsDelayCoroutine();
            ClearPalmHandHudPassengerTriggerCooldown();

            if (!_isFemalePassengerModeActive)
            {
                return;
            }

            Atom personForDeferredImprovedPoVRestore = _femalePassengerTargetPerson;

            try
            {
                RestoreImprovedPoVForPassengerTarget(_femalePassengerTargetPerson);

                SuperController superController = SuperController.singleton;
                if (superController != null &&
                    superController.navigationRig != null)
                {
                    superController.navigationRig.rotation =
                        _previousNavigationRigRotation;
                    superController.navigationRig.position =
                        _previousNavigationRigPosition;
                    superController.playerHeightAdjust =
                        _previousPlayerHeightAdjust;
                }
            }
            catch (Exception exception)
            {
                SuperController.LogError(
                    "Easy Mate passenger stop failed: " +
                    exception.Message);
            }
            finally
            {
                ScheduleDeferredImprovedPoVRestore(
                    personForDeferredImprovedPoVRestore);

                _isFemalePassengerModeActive = false;
                _femalePassengerTargetPerson = null;
                _femalePassengerHeadRigidbody = null;
                _possessor = null;
                _currentRotationVelocity = Quaternion.identity;
                _currentPositionVelocity = Vector3.zero;
            }
        }

        public static void OnPluginDestroy()
        {
            MainUIButtons.StopVrPassengerHandsRoutine();
            EasyMatePassengerHandPrePossessSnapshot.DiscardSnapshot();
            EasyMatePassengerPossessableNarrow.Restore();
            StopPassengerMode();
            ClearPendingPassengerModeActivation();
            _sessionPluginHost = null;
        }

        private static void QueuePassengerModeUntilImprovedPoVReady(string femaleUid)
        {
            _pendingPassengerModeTargetUid = femaleUid;
            _pendingPassengerModeDeadlineTime =
                Time.time + PendingActivationTimeoutSeconds;
        }

        private static void ClearPendingPassengerModeActivation()
        {
            _pendingPassengerModeTargetUid = null;
            _pendingPassengerModeDeadlineTime = 0f;
        }

        private static void TryProcessPendingPassengerModeStartup(
            SuperController superController)
        {
            if (string.IsNullOrEmpty(_pendingPassengerModeTargetUid))
            {
                return;
            }

            if (Time.time > _pendingPassengerModeDeadlineTime)
            {
                string expiredUid = _pendingPassengerModeTargetUid;
                ClearPendingPassengerModeActivation();
                SuperController.LogError(
                    "Easy Mate Be the girl: ImprovedPoV did not finish loading on '" +
                    expiredUid +
                    "'.");
                return;
            }

            Atom pendingPerson = superController.GetAtomByUid(
                _pendingPassengerModeTargetUid);
            if (!IsFemalePerson(pendingPerson))
            {
                ClearPendingPassengerModeActivation();
                return;
            }

            JSONStorable improvedPoVStorable = FindPluginStorableByClassSuffix(
                pendingPerson,
                ImprovedPoVClassSuffix);
            if (improvedPoVStorable == null)
            {
                return;
            }

            PrepareImprovedPoVForPassenger(improvedPoVStorable);
            ClearPendingPassengerModeActivation();
            ActivatePassengerForPerson(pendingPerson);
            SchedulePassengerHandsAfterHeadFollowingDelay(pendingPerson);
        }

        private static void ActivatePassengerForPerson(Atom femalePerson)
        {
            if (!IsFemalePerson(femalePerson))
            {
                return;
            }

            SuperController superController = SuperController.singleton;
            if (superController == null || superController.navigationRig == null)
            {
                SuperController.LogError(
                    "Easy Mate Be the girl: missing navigationRig.");
                return;
            }

            Rigidbody headRigidbody = FindHeadRigidbody(femalePerson);
            if (headRigidbody == null)
            {
                SuperController.LogError(
                    "Easy Mate Be the girl: '" +
                    femalePerson.uid +
                    "' has no head rigidbody.");
                return;
            }

            if (superController.centerCameraTarget == null)
            {
                SuperController.LogError(
                    "Easy Mate Be the girl: missing centerCameraTarget.");
                return;
            }

            _possessor =
                superController.centerCameraTarget.transform.GetComponent<Possessor>();
            if (_possessor == null || _possessor.autoSnapPoint == null)
            {
                SuperController.LogError(
                    "Easy Mate Be the girl: missing Possessor or autoSnapPoint.");
                return;
            }

            _previousNavigationRigRotation =
                superController.navigationRig.rotation;
            _previousNavigationRigPosition =
                superController.navigationRig.position;
            _previousPlayerHeightAdjust =
                superController.playerHeightAdjust;

            _femalePassengerTargetPerson = femalePerson;
            _femalePassengerHeadRigidbody = headRigidbody;
            _currentRotationVelocity = Quaternion.identity;
            _currentPositionVelocity = Vector3.zero;
            _isFemalePassengerModeActive = true;

            ApplyPassengerPose(superController, true);
        }

        private static void UpdatePassengerRuntime(
            SuperController superController)
        {
            if (!_isFemalePassengerModeActive)
            {
                return;
            }

            if (!IsFemalePerson(_femalePassengerTargetPerson) || _femalePassengerHeadRigidbody == null)
            {
                RequestStopForPalmHud();
                return;
            }

            if (superController.navigationRig == null ||
                superController.centerCameraTarget == null)
            {
                RequestStopForPalmHud();
                return;
            }

            _possessor =
                superController.centerCameraTarget.transform.GetComponent<Possessor>();
            if (_possessor == null || _possessor.autoSnapPoint == null)
            {
                RequestStopForPalmHud();
                return;
            }

            FreeControllerV3 headControl =
                _femalePassengerTargetPerson.GetStorableByID("headControl") as FreeControllerV3;
            if (headControl != null && headControl.possessed)
            {
                RequestStopForPalmHud();
                return;
            }

            try
            {
                ApplyPassengerPose(superController, false);
            }
            catch (Exception exception)
            {
                RequestStopForPalmHud();
                SuperController.LogError(
                    "Easy Mate passenger update failed: " +
                    exception.Message);
            }
        }

        private static void ApplyPassengerPose(
            SuperController superController,
            bool activeThisTurn)
        {
            Transform navigationRig = superController.navigationRig;
            if (navigationRig == null || _femalePassengerHeadRigidbody == null)
            {
                return;
            }

            if (activeThisTurn)
            {
                Quaternion navigationRigRotation =
                    _femalePassengerHeadRigidbody.transform.rotation;
                navigationRigRotation *= Quaternion.Euler(
                    RotationOffsetXDegrees,
                    0f,
                    0f);

                Vector3 rotationEulerAngles = navigationRigRotation.eulerAngles;
                navigationRigRotation.eulerAngles = new Vector3(
                    rotationEulerAngles.x,
                    rotationEulerAngles.y,
                    0f);

                if (RotationSmoothingSeconds > 0f)
                {
                    navigationRigRotation = SmoothDamp(
                        navigationRig.rotation,
                        navigationRigRotation,
                        ref _currentRotationVelocity,
                        RotationSmoothingSeconds);
                }

                navigationRig.rotation = navigationRigRotation;
            }

            Vector3 up = navigationRig.up;
            Vector3 targetPosition =
                _femalePassengerHeadRigidbody.position +
                _femalePassengerHeadRigidbody.transform.forward *
                PositionOffsetZMeters;

            Vector3 positionOffset =
                navigationRig.position +
                targetPosition -
                _possessor.autoSnapPoint.position;

            if (activeThisTurn)
            {
                float playerHeightAdjustOffset = Vector3.Dot(
                    positionOffset - navigationRig.position,
                    up);

                navigationRig.position =
                    positionOffset + up * -playerHeightAdjustOffset;
                superController.playerHeightAdjust +=
                    playerHeightAdjustOffset;
            }
            else
            {
                if (PositionSmoothingSeconds > 0f)
                {
                    positionOffset = Vector3.SmoothDamp(
                        navigationRig.position,
                        positionOffset,
                        ref _currentPositionVelocity,
                        PositionSmoothingSeconds);
                }

                navigationRig.position = positionOffset;
            }
        }

        private static bool IsFemalePerson(Atom atom)
        {
            if (atom == null || atom.type != "Person")
            {
                return false;
            }

            if (!atom.gameObject.activeInHierarchy || atom.hidden)
            {
                return false;
            }

            DAZCharacter dazCharacter =
                atom.GetComponentInChildren<DAZCharacter>();
            if (dazCharacter == null)
            {
                return false;
            }

            return !dazCharacter.isMale;
        }

        private static Rigidbody FindHeadRigidbody(Atom femalePerson)
        {
            if (femalePerson == null ||
                femalePerson.linkableRigidbodies == null)
            {
                return null;
            }

            for (int bodyIndex = 0;
                bodyIndex < femalePerson.linkableRigidbodies.Length;
                bodyIndex++)
            {
                Rigidbody rigidbody =
                    femalePerson.linkableRigidbodies[bodyIndex];
                if (rigidbody != null && rigidbody.name == "head")
                {
                    return rigidbody;
                }
            }

            return null;
        }

        private static void PrepareImprovedPoVForPassenger(
            JSONStorable improvedPoVStorable)
        {
            if (improvedPoVStorable == null)
            {
                return;
            }

            JSONStorableFloat cameraDepthFloat =
                improvedPoVStorable.GetFloatJSONParam("Camera depth");
            if (cameraDepthFloat != null)
            {
                cameraDepthFloat.val = 0f;
            }

            JSONStorableBool hideFaceBool =
                improvedPoVStorable.GetBoolJSONParam("Hide face");
            if (hideFaceBool != null)
            {
                hideFaceBool.val = true;
            }

            JSONStorableBool hideHairBool =
                improvedPoVStorable.GetBoolJSONParam("Hide hair");
            if (hideHairBool != null)
            {
                hideHairBool.val = true;
            }

            JSONStorableBool possessedOnlyBool =
                improvedPoVStorable.GetBoolJSONParam(
                    "Activate only when possessed");
            if (possessedOnlyBool != null)
            {
                possessedOnlyBool.val = false;
            }
        }

        private static void RestoreImprovedPoVForPassengerTarget(
            Atom femalePerson)
        {
            JSONStorable improvedPoVStorable =
                FindPluginStorableByClassSuffix(
                    femalePerson,
                    ImprovedPoVClassSuffix);
            if (improvedPoVStorable == null)
            {
                return;
            }

            JSONStorableFloat cameraDepthFloat =
                improvedPoVStorable.GetFloatJSONParam("Camera depth");
            if (cameraDepthFloat != null)
            {
                cameraDepthFloat.val = cameraDepthFloat.defaultVal;
            }

            JSONStorableBool possessedOnlyBool =
                improvedPoVStorable.GetBoolJSONParam(
                    "Activate only when possessed");
            if (possessedOnlyBool != null)
            {
                possessedOnlyBool.val = possessedOnlyBool.defaultVal;
            }
        }

        private static void ScheduleDeferredImprovedPoVRestore(Atom femalePerson)
        {
            if (_sessionPluginHost == null || femalePerson == null)
            {
                return;
            }

            try
            {
                _sessionPluginHost.StartCoroutine(
                    CoDeferImprovedPoVSkinRestoreKick(femalePerson));
            }
            catch (Exception exception)
            {
                SuperController.LogError(
                    "Easy Mate passenger ImprovedPoV deferred restore: " +
                    exception.Message);
            }
        }

        private static IEnumerator CoDeferImprovedPoVSkinRestoreKick(
            Atom femalePerson)
        {
            yield return null;

            if (!IsFemalePerson(femalePerson))
            {
                yield break;
            }

            RestoreImprovedPoVForPassengerTarget(femalePerson);
        }

        private static void CancelPassengerHandsDelayCoroutine()
        {
            if (_femalePassengerHandsDelayCoroutine == null)
            {
                return;
            }

            if (_sessionPluginHost != null)
            {
                try
                {
                    _sessionPluginHost.StopCoroutine(_femalePassengerHandsDelayCoroutine);
                }
                catch (Exception exception)
                {
                    SuperController.LogError(
                        "Easy Mate passenger hands delay cancel: " +
                        exception.Message);
                }
            }

            _femalePassengerHandsDelayCoroutine = null;
        }

        private static void SchedulePassengerHandsAfterHeadFollowingDelay(Atom femalePerson)
        {
            if (femalePerson == null || string.IsNullOrEmpty(femalePerson.uid))
            {
                return;
            }

            if (_sessionPluginHost == null)
            {
                SuperController.LogError(
                    "Easy Mate Be the girl: hand possession delay skipped (session host not ready).");
                return;
            }

            CancelPassengerHandsDelayCoroutine();

            try
            {
                string femalePersonUid = femalePerson.uid;
                _femalePassengerHandsDelayCoroutine = _sessionPluginHost.StartCoroutine(
                    CoStartPassengerHandsAfterDelay(femalePersonUid));
            }
            catch (Exception exception)
            {
                SuperController.LogError(
                    "Easy Mate passenger hands delay start: " +
                    exception.Message);
            }
        }

        private static IEnumerator CoStartPassengerHandsAfterDelay(
            string femalePersonUid)
        {
            yield return new WaitForSeconds(PassengerHandsStartDelaySeconds);

            _femalePassengerHandsDelayCoroutine = null;

            if (string.IsNullOrEmpty(femalePersonUid))
            {
                yield break;
            }

            SuperController superController = SuperController.singleton;
            if (superController == null)
            {
                yield break;
            }

            if (!_isFemalePassengerModeActive || _femalePassengerTargetPerson == null)
            {
                yield break;
            }

            if (_femalePassengerTargetPerson.uid != femalePersonUid)
            {
                yield break;
            }

            Atom resolvedFemalePerson = superController.GetAtomByUid(femalePersonUid);
            if (!IsFemalePerson(resolvedFemalePerson))
            {
                yield break;
            }

            EasyMatePassengerHandPrePossessSnapshot.CaptureFromPersonBeforeHandPossess(
                resolvedFemalePerson);
            NotifyPassengerHandsPossessionTriggeredForPalmHud();
            MainUIButtons.StartVrPassengerHandsRoutine(resolvedFemalePerson);
        }

        private static JSONStorable FindPluginStorableByClassSuffix(
            Atom atom,
            string classSuffix)
        {
            if (atom == null || string.IsNullOrEmpty(classSuffix))
            {
                return null;
            }

            List<string> storableIds = atom.GetStorableIDs();
            if (storableIds == null)
            {
                return null;
            }

            for (int storableIndex = 0;
                storableIndex < storableIds.Count;
                storableIndex++)
            {
                string storableId = storableIds[storableIndex];
                if (string.IsNullOrEmpty(storableId))
                {
                    continue;
                }

                if (!storableId.StartsWith("plugin#", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!storableId.EndsWith(classSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                return atom.GetStorableByID(storableId);
            }

            return null;
        }

        private static Quaternion SmoothDamp(
            Quaternion current,
            Quaternion target,
            ref Quaternion currentVelocity,
            float smoothTime)
        {
            float dot = Quaternion.Dot(current, target);
            float multiplier = dot > 0f ? 1f : -1f;
            target.x *= multiplier;
            target.y *= multiplier;
            target.z *= multiplier;
            target.w *= multiplier;

            Vector4 result = new Vector4(
                Mathf.SmoothDamp(
                    current.x,
                    target.x,
                    ref currentVelocity.x,
                    smoothTime),
                Mathf.SmoothDamp(
                    current.y,
                    target.y,
                    ref currentVelocity.y,
                    smoothTime),
                Mathf.SmoothDamp(
                    current.z,
                    target.z,
                    ref currentVelocity.z,
                    smoothTime),
                Mathf.SmoothDamp(
                    current.w,
                    target.w,
                    ref currentVelocity.w,
                    smoothTime)).normalized;

            float deltaTimeInverse = 1f / Time.deltaTime;
            currentVelocity.x = (result.x - current.x) * deltaTimeInverse;
            currentVelocity.y = (result.y - current.y) * deltaTimeInverse;
            currentVelocity.z = (result.z - current.z) * deltaTimeInverse;
            currentVelocity.w = (result.w - current.w) * deltaTimeInverse;

            return new Quaternion(result.x, result.y, result.z, result.w);
        }
    }
}
