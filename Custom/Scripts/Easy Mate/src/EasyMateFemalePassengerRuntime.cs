using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Passenger mode: navigation rig follows the model head
    /// <b>position</b> (Passenger-style); <b>rotation</b> aligns to the head only on
    /// the first activation frame, then stays independent so the model can turn without
    /// dragging the rig&apos;s yaw/pitch/roll. ImprovedPoV setup; VR hand possession starts
    /// once per session when you press any grip or trigger (see
    /// <see cref="EasyMateVrInput.PollVrAnyTriggerOrGripPressDown"/>).
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
        private const int InitialHeadNeutralizeFrames = 2;
        private const float HeadNeutralizeAngleToleranceDegrees = 1.5f;

        private const float PalmHudHideSecondsAfterHandsPossessTrigger = 5f;

        private static MVRScript _sessionPluginHost;

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

        private static bool _passengerVrHandsPossessionStartedThisSession;
        private static bool _waitingForInitialTeleportAfterHeadNeutralize;
        private static int _initialHeadNeutralizeFramesRemaining;
        private static float _preservedInitialHeadDownwardPitchDegrees;
        private static int _headNeutralizeDebugLogCount;

        public static bool IsPassengerModeActiveOrPending()
        {
            return _isFemalePassengerModeActive || !string.IsNullOrEmpty(_pendingPassengerModeTargetUid);
        }

        public static bool IsFemalePassengerModeActiveOrPending()
        {
            return IsPassengerModeActiveOrPending();
        }

        /// <summary>
        /// When true for <paramref name="person"/>, skip Easy Mate VR head proximity hide for that atom:
        /// female Passenger VR hand flow is active (<c>_passengerVrHandsPossessionStartedThisSession</c>
        /// after grip/trigger — same lifecycle as palm <b>Despossuir</b>), and ImprovedPoV already owns
        /// face/hair hiding on the target from <see cref="PrepareImprovedPoVForPassenger"/>.
        /// </summary>
        internal static bool ShouldSuppressVrHeadProximityHideForPerson(Atom person)
        {
            if (person == null || !_isFemalePassengerModeActive ||
                !_passengerVrHandsPossessionStartedThisSession ||
                _femalePassengerTargetPerson == null)
                return false;

            return person.uid == _femalePassengerTargetPerson.uid;
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

            RequestStartForPerson(femalePerson);
        }

        public static void RequestStartForMale(Atom malePerson)
        {
            if (!IsMalePerson(malePerson))
            {
                SuperController.LogError(
                    "Easy Mate passenger: missing male Person target.");
                return;
            }

            RequestStartForPerson(malePerson);
        }

        public static void RequestStopForPalmHud()
        {
            ClearPendingPassengerModeActivation();
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
            ClearPalmHandHudPassengerTriggerCooldown();
            _passengerVrHandsPossessionStartedThisSession = false;

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
                _waitingForInitialTeleportAfterHeadNeutralize = false;
                _initialHeadNeutralizeFramesRemaining = 0;
                _preservedInitialHeadDownwardPitchDegrees = 0f;
                _headNeutralizeDebugLogCount = 0;
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

        private static void RequestStartForPerson(Atom passengerPerson)
        {
            RequestStopForPalmHud();

            JSONStorable improvedPoVStorable = FindPluginStorableByClassSuffix(
                passengerPerson,
                ImprovedPoVClassSuffix);

            if (improvedPoVStorable == null)
            {
                MainUIButtons.TryMergePluginOntoPerson(
                    passengerPerson,
                    ImprovedPoVPluginPath);
                QueuePassengerModeUntilImprovedPoVReady(passengerPerson.uid);
                return;
            }

            PrepareImprovedPoVForPassenger(improvedPoVStorable);
            ActivatePassengerForPerson(passengerPerson);
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
                    "Easy Mate passenger: ImprovedPoV did not finish loading on '" +
                    expiredUid +
                    "'.");
                return;
            }

            Atom pendingPerson = superController.GetAtomByUid(
                _pendingPassengerModeTargetUid);
            if (!IsPassengerPerson(pendingPerson))
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
        }

        private static void ActivatePassengerForPerson(Atom femalePerson)
        {
            if (!IsPassengerPerson(femalePerson))
            {
                return;
            }

            SuperController superController = SuperController.singleton;
            if (superController == null || superController.navigationRig == null)
            {
                SuperController.LogError(
                    "Easy Mate passenger: missing navigationRig.");
                return;
            }

            Rigidbody headRigidbody = FindHeadRigidbody(femalePerson);
            if (headRigidbody == null)
            {
                SuperController.LogError(
                    "Easy Mate passenger: '" +
                    femalePerson.uid +
                    "' has no head rigidbody.");
                return;
            }

            if (superController.centerCameraTarget == null)
            {
                SuperController.LogError(
                    "Easy Mate passenger: missing centerCameraTarget.");
                return;
            }

            _possessor =
                superController.centerCameraTarget.transform.GetComponent<Possessor>();
            if (_possessor == null || _possessor.autoSnapPoint == null)
            {
                SuperController.LogError(
                    "Easy Mate passenger: missing Possessor or autoSnapPoint.");
                return;
            }

            _previousNavigationRigRotation =
                superController.navigationRig.rotation;
            _previousNavigationRigPosition =
                superController.navigationRig.position;
            _previousPlayerHeightAdjust =
                superController.playerHeightAdjust;

            EasyMatePassengerHandPrePossessSnapshot
                .CaptureHeadFromPersonBeforePassengerFollow(femalePerson);

            _femalePassengerTargetPerson = femalePerson;
            _femalePassengerHeadRigidbody = headRigidbody;
            _currentRotationVelocity = Quaternion.identity;
            _currentPositionVelocity = Vector3.zero;
            _isFemalePassengerModeActive = true;
            _passengerVrHandsPossessionStartedThisSession = false;
            _waitingForInitialTeleportAfterHeadNeutralize = true;
            _initialHeadNeutralizeFramesRemaining =
                InitialHeadNeutralizeFrames;
            _headNeutralizeDebugLogCount = 0;

            FreeControllerV3 headControl =
                femalePerson.GetStorableByID("headControl") as FreeControllerV3;
            _preservedInitialHeadDownwardPitchDegrees =
                GetPassengerDownwardPitchToPreserve(
                    headControl != null && headControl.control != null
                        ? headControl.control.rotation
                        : headRigidbody.transform.rotation);
            ForcePassengerHeadControlNeutralRotation(headControl);
            LogPassengerHeadNeutralizeDebug("start", headControl);

            SuperController.LogMessage(
                "Easy Mate passenger: press any VR grip or trigger to possess the model's hands.");
        }

        private static void UpdatePassengerRuntime(
            SuperController superController)
        {
            if (!_isFemalePassengerModeActive)
            {
                return;
            }

            if (!IsPassengerPerson(_femalePassengerTargetPerson) || _femalePassengerHeadRigidbody == null)
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

            bool initialTeleportCompletedThisTurn = false;
            if (_waitingForInitialTeleportAfterHeadNeutralize)
            {
                ForcePassengerHeadControlNeutralRotation(headControl);

                if (_initialHeadNeutralizeFramesRemaining > 0)
                {
                    LogPassengerHeadNeutralizeDebug("warmup", headControl);
                    _initialHeadNeutralizeFramesRemaining--;
                    return;
                }

                if (!IsPassengerHeadControlNeutralized(headControl))
                {
                    LogPassengerHeadNeutralizeDebug("waiting", headControl);
                    return;
                }

                LogPassengerHeadNeutralizeDebug("settled", headControl);
                ApplyPassengerPose(superController, true);
                _waitingForInitialTeleportAfterHeadNeutralize = false;
                initialTeleportCompletedThisTurn = true;
            }

            TryStartPassengerVrHandsFromUserPress(superController);

            try
            {
                if (!initialTeleportCompletedThisTurn)
                    ApplyPassengerPose(superController, false);
                ApplyPassengerHeadRotationFollow(superController, headControl);
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

            Transform motionControllerHead =
                superController.centerCameraTarget != null ?
                superController.centerCameraTarget.transform :
                null;
            Quaternion navigationRigRotationBefore =
                navigationRig.rotation;
            Vector3 navigationRigPositionBefore =
                navigationRig.position;
            string desiredHeadRotationSourceName;
            Quaternion desiredHeadRotation =
                BuildPassengerDesiredHeadRotation(
                    navigationRig.up,
                    out desiredHeadRotationSourceName);
            Quaternion navigationRigRotation = desiredHeadRotation;
            Quaternion headRotationDelta = Quaternion.identity;
            if (activeThisTurn)
            {
                if (motionControllerHead != null)
                {
                    desiredHeadRotation = KeepOnlyDownwardPitch(
                        desiredHeadRotation,
                        motionControllerHead.rotation);
                    headRotationDelta =
                        desiredHeadRotation *
                        Quaternion.Inverse(motionControllerHead.rotation);
                    navigationRigRotation =
                        headRotationDelta * navigationRig.rotation;
                }

                Vector3 rotationEulerAngles = navigationRigRotation.eulerAngles;
                navigationRigRotation.eulerAngles = new Vector3(
                    rotationEulerAngles.x,
                    rotationEulerAngles.y,
                    0f);

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
                float playerHeightAdjustBefore =
                    superController.playerHeightAdjust;
                float playerHeightAdjustOffset = Vector3.Dot(
                    positionOffset - navigationRig.position,
                    up);

                navigationRig.position = positionOffset;

                ApplyPassengerFirstSnapLateralCenter(
                    superController,
                    motionControllerHead);

                SuperController.LogMessage(
                    "EasyMate DEBUG passenger first teleport: " +
                    "person=" +
                    (_femalePassengerTargetPerson != null ?
                        _femalePassengerTargetPerson.uid :
                        "null") +
                    " hmdRot=" +
                    FormatEulerForDebug(
                        motionControllerHead != null ?
                        motionControllerHead.rotation :
                        Quaternion.identity) +
                    " rigBefore=" +
                    FormatEulerForDebug(navigationRigRotationBefore) +
                    " desiredHeadRot=" +
                    FormatEulerForDebug(desiredHeadRotation) +
                    " desiredHeadRotSource=" +
                    desiredHeadRotationSourceName +
                    " headDelta=" +
                    FormatEulerForDebug(headRotationDelta) +
                    " rigAfter=" +
                    FormatEulerForDebug(navigationRig.rotation) +
                    " charHeadRot=" +
                    FormatEulerForDebug(
                        _femalePassengerHeadRigidbody.transform.rotation) +
                    " hmdPos=" +
                    FormatVectorForDebug(
                        motionControllerHead != null ?
                        motionControllerHead.position :
                        Vector3.zero) +
                    " rigPosBefore=" +
                    FormatVectorForDebug(navigationRigPositionBefore) +
                    " targetPos=" +
                    FormatVectorForDebug(targetPosition) +
                    " autoSnapPos=" +
                    FormatVectorForDebug(_possessor.autoSnapPoint.position) +
                    " posOffset=" +
                    FormatVectorForDebug(positionOffset) +
                    " rigPosAfter=" +
                    FormatVectorForDebug(navigationRig.position) +
                    " charHeadPos=" +
                    FormatVectorForDebug(
                        _femalePassengerHeadRigidbody.position) +
                    " playerHeightAdjustBefore=" +
                    playerHeightAdjustBefore.ToString("F4") +
                    " playerHeightAdjustAfter=" +
                    superController.playerHeightAdjust.ToString("F4") +
                    " playerHeightAdjustOffset=" +
                    playerHeightAdjustOffset.ToString("F4"));
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

        /// <summary>
        /// First snap: shift the rig along the person&apos;s left-right axis so the
        /// HMD sits on the torso midline (sagittal plane through chest/pelvis).
        /// </summary>
        private static void ApplyPassengerFirstSnapLateralCenter(
            SuperController superController,
            Transform motionControllerHead)
        {
            if (superController == null || motionControllerHead == null ||
                _femalePassengerTargetPerson == null)
            {
                return;
            }

            Transform navigationRig = superController.navigationRig;
            if (navigationRig == null)
            {
                return;
            }

            FreeControllerV3 torso =
                _femalePassengerTargetPerson.GetStorableByID("chestControl") as FreeControllerV3;
            if (torso == null || torso.control == null)
            {
                torso =
                    _femalePassengerTargetPerson.GetStorableByID("pelvisControl") as FreeControllerV3;
            }
            if (torso == null || torso.control == null)
            {
                torso =
                    _femalePassengerTargetPerson.GetStorableByID("abdomenControl") as FreeControllerV3;
            }
            if (torso == null || torso.control == null)
            {
                return;
            }

            Vector3 up = navigationRig.up;
            if (up.sqrMagnitude < 1e-12f)
            {
                up = Vector3.up;
            }
            up.Normalize();

            Vector3 lateralAxis = Vector3.ProjectOnPlane(torso.control.right, up);
            if (lateralAxis.sqrMagnitude < 1e-10f)
            {
                return;
            }
            lateralAxis.Normalize();

            Vector3 midReference = torso.control.position;
            Vector3 hmdWorld = motionControllerHead.position;
            float lateralSigned = Vector3.Dot(hmdWorld - midReference, lateralAxis);
            Vector3 correction = -lateralSigned * lateralAxis;
            navigationRig.position += correction;
        }

        private static void ApplyPassengerHeadRotationFollow(
            SuperController superController,
            FreeControllerV3 headControl)
        {
            if (superController == null || headControl == null ||
                headControl.control == null)
            {
                return;
            }

            Transform motionControllerHead;

            motionControllerHead = superController.centerCameraTarget != null ?
                superController.centerCameraTarget.transform :
                null;
            if (motionControllerHead == null)
            {
                return;
            }

            headControl.currentRotationState = FreeControllerV3.RotationState.On;
            headControl.control.rotation = motionControllerHead.rotation;

            if (headControl.followWhenOff != null)
            {
                headControl.followWhenOff.rotation =
                    motionControllerHead.rotation;
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

        private static bool IsMalePerson(Atom atom)
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

            return dazCharacter.isMale;
        }

        private static bool IsPassengerPerson(Atom atom)
        {
            return IsFemalePerson(atom) || IsMalePerson(atom);
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

        private static void TryStartPassengerVrHandsFromUserPress(
            SuperController superController)
        {
            if (_passengerVrHandsPossessionStartedThisSession)
            {
                return;
            }

            if (!EasyMateVrInput.PollVrAnyTriggerOrGripPressDown(superController))
            {
                return;
            }

            if (_femalePassengerTargetPerson == null ||
                string.IsNullOrEmpty(_femalePassengerTargetPerson.uid))
            {
                return;
            }

            Atom resolvedFemalePerson =
                superController.GetAtomByUid(_femalePassengerTargetPerson.uid);
            if (!IsFemalePerson(resolvedFemalePerson))
            {
                return;
            }

            _passengerVrHandsPossessionStartedThisSession = true;

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

        private static string FormatEulerForDebug(Quaternion rotation)
        {
            Vector3 eulerAngles = rotation.eulerAngles;
            return string.Format(
                "({0:F2}, {1:F2}, {2:F2})",
                eulerAngles.x,
                eulerAngles.y,
                eulerAngles.z);
        }

        private static string FormatVectorForDebug(Vector3 vector)
        {
            return string.Format(
                "({0:F4}, {1:F4}, {2:F4})",
                vector.x,
                vector.y,
                vector.z);
        }

        private static void ForcePassengerHeadControlNeutralRotation(
            FreeControllerV3 headControl)
        {
            if (headControl == null || headControl.control == null)
                return;

            Quaternion neutralRotation =
                GetPassengerNeutralHeadControlRotation(headControl);
            headControl.currentRotationState = FreeControllerV3.RotationState.On;
            headControl.control.rotation = neutralRotation;

            if (headControl.followWhenOff != null)
                headControl.followWhenOff.rotation = neutralRotation;
        }

        private static void LogPassengerHeadNeutralizeDebug(
            string phase,
            FreeControllerV3 headControl)
        {
            if (_headNeutralizeDebugLogCount >= 12 &&
                _headNeutralizeDebugLogCount % 30 != 0)
            {
                _headNeutralizeDebugLogCount++;
                return;
            }

            if (headControl == null || headControl.control == null)
            {
                SuperController.LogMessage(
                    "EasyMate DEBUG passenger head neutralize: phase=" +
                    phase +
                    " headControl=null");
                _headNeutralizeDebugLogCount++;
                return;
            }

            Quaternion neutralRotation =
                GetPassengerNeutralHeadControlRotation(headControl);
            Vector3 currentEulerAngles =
                headControl.control.rotation.eulerAngles;
            Vector3 targetEulerAngles =
                neutralRotation.eulerAngles;
            Vector3 currentLocalEulerAngles =
                headControl.control.localEulerAngles;

            float xDeltaDegrees = Mathf.Abs(
                NormalizeSignedEulerAngle(
                    currentEulerAngles.x - targetEulerAngles.x));
            float yDeltaDegrees = Mathf.Abs(
                NormalizeSignedEulerAngle(
                    currentEulerAngles.y - targetEulerAngles.y));
            float zDeltaDegrees = Mathf.Abs(
                NormalizeSignedEulerAngle(
                    currentEulerAngles.z - targetEulerAngles.z));

            string sourceName;
            Vector3 upAxis;
            Vector3 neutralForward = GetPassengerNeutralForward(
                headControl,
                out upAxis,
                out sourceName);

            SuperController.LogMessage(
                "EasyMate DEBUG passenger head neutralize: phase=" +
                phase +
                " person=" +
                (_femalePassengerTargetPerson != null ?
                    _femalePassengerTargetPerson.uid :
                    "null") +
                " currentWorld=" +
                FormatEulerForDebug(headControl.control.rotation) +
                " currentLocal=" +
                FormatVectorForDebug(currentLocalEulerAngles) +
                " targetWorld=" +
                FormatEulerForDebug(neutralRotation) +
                " delta=(" +
                xDeltaDegrees.ToString("F2") + ", " +
                yDeltaDegrees.ToString("F2") + ", " +
                zDeltaDegrees.ToString("F2") + ")" +
                " preservedDownPitch=" +
                _preservedInitialHeadDownwardPitchDegrees.ToString("F2") +
                " source=" +
                sourceName +
                " neutralForward=" +
                FormatVectorForDebug(neutralForward) +
                " warmupFramesRemaining=" +
                _initialHeadNeutralizeFramesRemaining.ToString() +
                " rotationState=" +
                headControl.currentRotationState.ToString());
            _headNeutralizeDebugLogCount++;
        }

        private static bool IsPassengerHeadControlNeutralized(
            FreeControllerV3 headControl)
        {
            if (headControl == null || headControl.control == null)
                return false;

            Quaternion neutralRotation =
                GetPassengerNeutralHeadControlRotation(headControl);
            Vector3 currentEulerAngles =
                headControl.control.rotation.eulerAngles;
            Vector3 targetEulerAngles =
                neutralRotation.eulerAngles;

            float xDeltaDegrees = Mathf.Abs(
                NormalizeSignedEulerAngle(
                    currentEulerAngles.x - targetEulerAngles.x));
            float yDeltaDegrees = Mathf.Abs(
                NormalizeSignedEulerAngle(
                    currentEulerAngles.y - targetEulerAngles.y));
            float zDeltaDegrees = Mathf.Abs(
                NormalizeSignedEulerAngle(
                    currentEulerAngles.z - targetEulerAngles.z));

            return xDeltaDegrees <= HeadNeutralizeAngleToleranceDegrees &&
                yDeltaDegrees <= HeadNeutralizeAngleToleranceDegrees &&
                zDeltaDegrees <= HeadNeutralizeAngleToleranceDegrees;
        }

        private static Quaternion GetPassengerNeutralHeadControlRotation(
            FreeControllerV3 headControl)
        {
            string sourceName;
            Vector3 upAxis;
            Vector3 neutralForward = GetPassengerNeutralForward(
                headControl,
                out upAxis,
                out sourceName);
            if (neutralForward.sqrMagnitude < 1e-10f)
                neutralForward = Vector3.ProjectOnPlane(
                    headControl.control.forward,
                    upAxis);
            if (neutralForward.sqrMagnitude < 1e-10f)
                neutralForward = Vector3.ProjectOnPlane(
                    Vector3.forward,
                    upAxis);
            if (neutralForward.sqrMagnitude < 1e-10f)
                neutralForward = Vector3.forward;

            neutralForward.Normalize();
            return Quaternion.LookRotation(
                neutralForward,
                upAxis) * Quaternion.Euler(
                    _preservedInitialHeadDownwardPitchDegrees,
                    0f,
                    0f);
        }

        private static float GetPassengerDownwardPitchToPreserve(
            Quaternion rotation)
        {
            float pitchDegrees = NormalizeSignedEulerAngle(
                rotation.eulerAngles.x);
            if (pitchDegrees > 0f && pitchDegrees <= 90f)
                return pitchDegrees;

            return 0f;
        }

        private static Vector3 GetPassengerNeutralForward(
            FreeControllerV3 headControl,
            out Vector3 upAxis,
            out string sourceName)
        {
            sourceName = "none";
            upAxis = Vector3.up;
            Vector3 neutralForward = Vector3.zero;

            if (_femalePassengerTargetPerson != null)
            {
                FreeControllerV3 chest =
                    _femalePassengerTargetPerson.GetStorableByID(
                        "chestControl") as FreeControllerV3;
                if (chest != null && chest.control != null)
                {
                    upAxis = chest.control.up;
                    if (upAxis.sqrMagnitude < 1e-10f)
                        upAxis = Vector3.up;
                    upAxis.Normalize();
                    neutralForward = Vector3.ProjectOnPlane(
                        chest.control.forward,
                        upAxis);
                    if (neutralForward.sqrMagnitude >= 1e-10f)
                        sourceName = "chestControl.forward";
                }

                if (neutralForward.sqrMagnitude < 1e-10f)
                {
                    FreeControllerV3 pelvis =
                        _femalePassengerTargetPerson.GetStorableByID(
                            "pelvisControl") as FreeControllerV3;
                    if (pelvis != null && pelvis.control != null)
                    {
                        upAxis = pelvis.control.up;
                        if (upAxis.sqrMagnitude < 1e-10f)
                            upAxis = Vector3.up;
                        upAxis.Normalize();
                        neutralForward = Vector3.ProjectOnPlane(
                            pelvis.control.forward,
                            upAxis);
                        if (neutralForward.sqrMagnitude >= 1e-10f)
                            sourceName = "pelvisControl.forward";
                    }
                }

                if (neutralForward.sqrMagnitude < 1e-10f)
                {
                    FreeControllerV3 abdomen =
                        _femalePassengerTargetPerson.GetStorableByID(
                            "abdomenControl") as FreeControllerV3;
                    if (abdomen != null && abdomen.control != null)
                    {
                        upAxis = abdomen.control.up;
                        if (upAxis.sqrMagnitude < 1e-10f)
                            upAxis = Vector3.up;
                        upAxis.Normalize();
                        neutralForward = Vector3.ProjectOnPlane(
                            abdomen.control.forward,
                            upAxis);
                        if (neutralForward.sqrMagnitude >= 1e-10f)
                            sourceName = "abdomenControl.forward";
                    }
                }

                if (neutralForward.sqrMagnitude < 1e-10f)
                {
                    upAxis = _femalePassengerTargetPerson.transform.up;
                    if (upAxis.sqrMagnitude < 1e-10f)
                        upAxis = Vector3.up;
                    upAxis.Normalize();
                    neutralForward = Vector3.ProjectOnPlane(
                        _femalePassengerTargetPerson.transform.forward,
                        upAxis);
                    if (neutralForward.sqrMagnitude >= 1e-10f)
                        sourceName = "person.transform.forward";
                }
            }

            if (neutralForward.sqrMagnitude < 1e-10f &&
                _femalePassengerHeadRigidbody != null)
            {
                upAxis = _femalePassengerHeadRigidbody.transform.up;
                if (upAxis.sqrMagnitude < 1e-10f)
                    upAxis = Vector3.up;
                upAxis.Normalize();
                neutralForward = Vector3.ProjectOnPlane(
                    _femalePassengerHeadRigidbody.transform.forward,
                    upAxis);
                if (neutralForward.sqrMagnitude >= 1e-10f)
                    sourceName = "headRigidbody.forward";
            }

            if (neutralForward.sqrMagnitude < 1e-10f &&
                headControl != null && headControl.control != null)
            {
                upAxis = headControl.control.up;
                if (upAxis.sqrMagnitude < 1e-10f)
                    upAxis = Vector3.up;
                upAxis.Normalize();
            }

            return neutralForward;
        }

        private static Quaternion BuildPassengerDesiredHeadRotation(
            Vector3 upAxis,
            out string sourceName)
        {
            sourceName = "none";
            if (_femalePassengerHeadRigidbody == null)
                return Quaternion.identity;

            Quaternion headRotationWithOffset =
                _femalePassengerHeadRigidbody.transform.rotation *
                Quaternion.Euler(
                    RotationOffsetXDegrees,
                    0f,
                    0f);
            float pitchDegrees = NormalizeSignedEulerAngle(
                headRotationWithOffset.eulerAngles.x);
            if (_preservedInitialHeadDownwardPitchDegrees > pitchDegrees)
                pitchDegrees = _preservedInitialHeadDownwardPitchDegrees;

            Vector3 stableUpAxis;
            Vector3 neutralForward = GetPassengerNeutralForward(
                null,
                out stableUpAxis,
                out sourceName);
            if (stableUpAxis.sqrMagnitude >= 1e-10f)
                upAxis = stableUpAxis;
            if (neutralForward.sqrMagnitude < 1e-10f)
                return headRotationWithOffset;

            neutralForward.Normalize();

            Quaternion neutralRotation = Quaternion.LookRotation(
                neutralForward,
                upAxis);
            return neutralRotation * Quaternion.Euler(
                pitchDegrees,
                0f,
                0f);
        }

        private static Quaternion KeepOnlyDownwardPitch(
            Quaternion desiredRotation,
            Quaternion currentHmdRotation)
        {
            Vector3 desiredEulerAngles = desiredRotation.eulerAngles;
            float desiredPitch =
                NormalizeSignedEulerAngle(desiredEulerAngles.x);
            if (desiredPitch > 0f)
            {
                Vector3 currentHmdEulerAngles =
                    currentHmdRotation.eulerAngles;
                desiredEulerAngles.x = currentHmdEulerAngles.x;
                desiredRotation = Quaternion.Euler(desiredEulerAngles);
            }

            return desiredRotation;
        }

        private static float NormalizeSignedEulerAngle(float eulerAngle)
        {
            if (eulerAngle > 180f)
                return eulerAngle - 360f;

            return eulerAngle;
        }
    }
}
