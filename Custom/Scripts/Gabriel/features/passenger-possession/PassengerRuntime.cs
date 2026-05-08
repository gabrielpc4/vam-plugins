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
    /// <see cref="VrInput.PollVrAnyTriggerOrGripPressDown"/>). Start is requested from
    /// <see cref="PassengerLaserPossess"/> (right UI-aim laser + face A) via
    /// <see cref="PassengerRuntime.RequestPassengerForSpecificPerson"/>.
    /// </summary>
    internal static class PassengerRuntime
    {
        private const string ImprovedPoVPluginPath =
            "Custom/Scripts/Gabriel/features/improved-pov/ImprovedPoV.cs";
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

        private static Coroutine _autoPossessCoroutine;

        private static bool _isPassengerModeActive;
        private static Atom _passengerTargetPerson;
        private static Rigidbody _passengerHeadRigidbody;
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
            return _isPassengerModeActive || !string.IsNullOrEmpty(_pendingPassengerModeTargetUid);
        }

        /// <summary>
        /// When true for <paramref name="person"/>, skip Easy Mate VR head proximity hide for that atom:
        /// passenger VR hand flow is active (<c>_passengerVrHandsPossessionStartedThisSession</c>
        /// after grip/trigger — same lifecycle as palm <b>Despossuir</b>), and ImprovedPoV already owns
        /// face/hair hiding on the target from <see cref="PrepareImprovedPoVForPassenger"/>.
        /// </summary>
        internal static bool ShouldSuppressVrHeadProximityHideForPerson(Atom person)
        {
            if (person == null || !_isPassengerModeActive ||
                !_passengerVrHandsPossessionStartedThisSession ||
                _passengerTargetPerson == null)
                return false;

            return person.uid == _passengerTargetPerson.uid;
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

        internal static void SetSessionPluginHost(MVRScript host)
        {
            if (host != null)
            {
                _sessionPluginHost = host;
            }
        }

        public static void NotifySceneChanged(MVRScript host)
        {
            SetSessionPluginHost(host);
            StopVrPassengerHandsRoutine();

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

            PassengerHandPrePossessSnapshot.DiscardSnapshot();
            PassengerPossessableNarrow.Restore();

            StopPassengerMode();
            ClearPendingPassengerModeActivation();
        }

        public static void NotifyAtomUidsChanged(
            List<string> atomUids,
            MVRScript host)
        {
            SetSessionPluginHost(host);
        }

        public static void Tick(MVRScript host)
        {
            SetSessionPluginHost(host);

            SuperController superController = SuperController.singleton;
            if (superController == null || superController.isLoading)
            {
                return;
            }

            TryProcessPendingPassengerModeStartup(superController);

            if (_isPassengerModeActive)
            {
                UpdatePassengerRuntime(superController);
            }
        }

        private static void StopAutoPossessRoutine()
        {
            if (_sessionPluginHost != null && _autoPossessCoroutine != null)
            {
                _sessionPluginHost.StopCoroutine(_autoPossessCoroutine);
                _autoPossessCoroutine = null;
            }

            PassengerPossessableNarrow.Restore();
        }

        internal static void StopVrPassengerHandsRoutine()
        {
            StopAutoPossessRoutine();
        }

        internal static void StartVrPassengerHandsRoutine(Atom person)
        {
            if (_sessionPluginHost == null)
            {
                SuperController.LogError(
                    "Easy Mate passenger hands: session host missing.");
                return;
            }

            StopAutoPossessRoutine();
            _autoPossessCoroutine = _sessionPluginHost.StartCoroutine(
                PossessHandsOnlyRoutine(person));
        }

        private static IEnumerator PossessHandsOnlyRoutine(Atom person)
        {
            try
            {
                SuperController sc = SuperController.singleton;
                if (sc == null || person == null || person.type != "Person")
                    yield break;

                FreeControllerV3 leftHand =
                    person.GetStorableByID("lHandControl") as FreeControllerV3;
                FreeControllerV3 rightHand =
                    person.GetStorableByID("rHandControl") as FreeControllerV3;

                if (leftHand == null && rightHand == null)
                {
                    SuperController.LogError(
                        "Easy Mate passenger hands: no hand controls on " +
                        person.name);
                    yield break;
                }

                PassengerPossessableNarrow.ApplyForTargetPerson(person);
                sc.SelectModePossess(true);

                yield return null;
                yield return null;

                bool leftDone = leftHand == null || leftHand.possessed;
                bool rightDone = rightHand == null || rightHand.possessed;
                string leftError = null;
                string rightError = null;
                int i;

                for (i = 0; i < 120 && (!leftDone || !rightDone); i++)
                {
                    if (!leftDone)
                    {
                        TryDriveControllerIntoPossessOverlap(
                            sc,
                            leftHand,
                            true,
                            out leftError);
                        leftDone = leftHand != null && leftHand.possessed;
                    }

                    if (!rightDone)
                    {
                        TryDriveControllerIntoPossessOverlap(
                            sc,
                            rightHand,
                            false,
                            out rightError);
                        rightDone = rightHand != null && rightHand.possessed;
                    }

                    if (!leftDone || !rightDone)
                        yield return null;
                }

                try
                {
                    sc.SelectModeOff();
                }
                catch (Exception selectModeException)
                {
                    SuperController.LogError(
                        "Easy Mate passenger hands SelectModeOff: " +
                        selectModeException.Message);
                }

                string leftState = leftDone ? "ok" : "failed";
                string rightState = rightDone ? "ok" : "failed";
                SuperController.LogMessage(
                    "Easy Mate passenger hands: " +
                    person.name +
                    " (left " +
                    leftState +
                    ", right " +
                    rightState +
                    ").");

                if (!leftDone && leftError != null)
                {
                    SuperController.LogMessage(
                        "Easy Mate passenger hands left detail: " +
                        leftError);
                }

                if (!rightDone && rightError != null)
                {
                    SuperController.LogMessage(
                        "Easy Mate passenger hands right detail: " +
                        rightError);
                }

                if ((leftHand != null && leftHand.possessed) ||
                    (rightHand != null && rightHand.possessed))
                {
                    RefreshHudPluginToggleLabels();
                }
            }
            finally
            {
                PassengerPossessableNarrow.Restore();
                _autoPossessCoroutine = null;
            }
        }

        private static void RefreshHudPluginToggleLabels()
        {
            GabrielSessionOrchestrator orchestrator =
                _sessionPluginHost as GabrielSessionOrchestrator;
            if (orchestrator != null)
            {
                orchestrator.RefreshHudPluginToggleLabels();
            }
        }

        private static bool TryPrepareHandForPossess(
            SuperController sc,
            FreeControllerV3 controller,
            bool left,
            out string error)
        {
            error = null;
            if (controller == null)
                return true;

            Transform motionController = GetMotionControllerTransform(sc, left);
            if (sc == null || motionController == null)
            {
                error = "missing SuperController or player hand transform";
                return false;
            }

            try
            {
                controller.PossessMoveAndAlignTo(motionController);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private static bool TryDriveControllerIntoPossessOverlap(
            SuperController sc,
            FreeControllerV3 controller,
            bool left,
            out string error)
        {
            error = null;
            if (controller == null)
                return true;

            if (controller.possessed)
                return true;

            return TryPrepareHandForPossess(sc, controller, left, out error);
        }

        private static Transform GetMotionControllerTransform(
            SuperController sc,
            bool left)
        {
            if (sc == null)
                return null;

            return left ? sc.leftHand : sc.rightHand;
        }

        public static void RequestStartForFemale(Atom femalePerson)
        {
            if (!IsFemalePerson(femalePerson))
            {
                SuperController.LogError(
                    "Easy Mate passenger: missing female Person target.");
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

        /// <summary>
        /// Laser + face A entry: start passenger for a <c>Person</c> of either
        /// gender when recognized as male or female.
        /// </summary>
        public static bool RequestPassengerForSpecificPerson(Atom targetPerson)
        {
            if (targetPerson == null || targetPerson.type != "Person")
                return false;

            if (IsFemalePerson(targetPerson))
            {
                RequestStartForFemale(targetPerson);
                return true;
            }

            if (IsMalePerson(targetPerson))
            {
                RequestStartForMale(targetPerson);
                return true;
            }

            return false;
        }

        public static void RequestStopForPalmHud()
        {
            ClearPendingPassengerModeActivation();
            StopVrPassengerHandsRoutine();

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

            PassengerHandPrePossessSnapshot.RestoreAfterPossessClearThenDiscardSnapshot();

            GripHandVisibility.DisableVrHandModelsForSceneStart();
        }

        public static void StopPassengerMode()
        {
            ClearPendingPassengerModeActivation();
            ClearPalmHandHudPassengerTriggerCooldown();
            _passengerVrHandsPossessionStartedThisSession = false;

            if (!_isPassengerModeActive)
            {
                return;
            }

            Atom passengerPersonForDeferredImprovedPoVRestore = _passengerTargetPerson;

            try
            {
                RestoreImprovedPoVForPassengerTarget(_passengerTargetPerson);

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
                    passengerPersonForDeferredImprovedPoVRestore);

                _isPassengerModeActive = false;
                _passengerTargetPerson = null;
                _passengerHeadRigidbody = null;
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
            StopVrPassengerHandsRoutine();
            PassengerHandPrePossessSnapshot.DiscardSnapshot();
            PassengerPossessableNarrow.Restore();
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
                GabrielPluginManagerMerge.TryMergePluginOntoPerson(
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

        private static void ActivatePassengerForPerson(Atom passengerPerson)
        {
            if (!IsPassengerPerson(passengerPerson))
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

            Rigidbody headRigidbody = FindHeadRigidbody(passengerPerson);
            if (headRigidbody == null)
            {
                SuperController.LogError(
                    "Easy Mate passenger: '" +
                    passengerPerson.uid +
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

            PassengerHandPrePossessSnapshot
                .CaptureHeadFromPersonBeforePassengerFollow(passengerPerson);

            _passengerTargetPerson = passengerPerson;
            _passengerHeadRigidbody = headRigidbody;
            _currentRotationVelocity = Quaternion.identity;
            _currentPositionVelocity = Vector3.zero;
            _isPassengerModeActive = true;
            _passengerVrHandsPossessionStartedThisSession = false;
            _waitingForInitialTeleportAfterHeadNeutralize = true;
            _initialHeadNeutralizeFramesRemaining =
                InitialHeadNeutralizeFrames;
            _headNeutralizeDebugLogCount = 0;

            FreeControllerV3 headControl =
                passengerPerson.GetStorableByID("headControl") as FreeControllerV3;
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
            if (!_isPassengerModeActive)
            {
                return;
            }

            if (!IsPassengerPerson(_passengerTargetPerson) || _passengerHeadRigidbody == null)
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
                _passengerTargetPerson.GetStorableByID("headControl") as FreeControllerV3;
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
            if (navigationRig == null || _passengerHeadRigidbody == null)
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
                _passengerHeadRigidbody.position +
                _passengerHeadRigidbody.transform.forward *
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
                    "GabrielHud DEBUG passenger first teleport: " +
                    "person=" +
                    (_passengerTargetPerson != null ?
                        _passengerTargetPerson.uid :
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
                        _passengerHeadRigidbody.transform.rotation) +
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
                        _passengerHeadRigidbody.position) +
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
                _passengerTargetPerson == null)
            {
                return;
            }

            Transform navigationRig = superController.navigationRig;
            if (navigationRig == null)
            {
                return;
            }

            FreeControllerV3 torso =
                _passengerTargetPerson.GetStorableByID("chestControl") as FreeControllerV3;
            if (torso == null || torso.control == null)
            {
                torso =
                    _passengerTargetPerson.GetStorableByID("pelvisControl") as FreeControllerV3;
            }
            if (torso == null || torso.control == null)
            {
                torso =
                    _passengerTargetPerson.GetStorableByID("abdomenControl") as FreeControllerV3;
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

        private static Rigidbody FindHeadRigidbody(Atom passengerPerson)
        {
            if (passengerPerson == null ||
                passengerPerson.linkableRigidbodies == null)
            {
                return null;
            }

            for (int bodyIndex = 0;
                bodyIndex < passengerPerson.linkableRigidbodies.Length;
                bodyIndex++)
            {
                Rigidbody rigidbody =
                    passengerPerson.linkableRigidbodies[bodyIndex];
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
            Atom passengerPerson)
        {
            JSONStorable improvedPoVStorable =
                FindPluginStorableByClassSuffix(
                    passengerPerson,
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

        private static void ScheduleDeferredImprovedPoVRestore(Atom passengerPerson)
        {
            if (_sessionPluginHost == null || passengerPerson == null)
            {
                return;
            }

            try
            {
                _sessionPluginHost.StartCoroutine(
                    CoDeferImprovedPoVSkinRestoreKick(passengerPerson));
            }
            catch (Exception exception)
            {
                SuperController.LogError(
                    "Easy Mate passenger ImprovedPoV deferred restore: " +
                    exception.Message);
            }
        }

        private static IEnumerator CoDeferImprovedPoVSkinRestoreKick(
            Atom passengerPerson)
        {
            yield return null;

            if (!IsPassengerPerson(passengerPerson))
            {
                yield break;
            }

            RestoreImprovedPoVForPassengerTarget(passengerPerson);
        }

        private static void TryStartPassengerVrHandsFromUserPress(
            SuperController superController)
        {
            if (_passengerVrHandsPossessionStartedThisSession)
            {
                return;
            }

            if (!VrInput.PollVrAnyTriggerOrGripPressDown(superController))
            {
                return;
            }

            if (_passengerTargetPerson == null ||
                string.IsNullOrEmpty(_passengerTargetPerson.uid))
            {
                return;
            }

            Atom resolvedPassengerPerson =
                superController.GetAtomByUid(_passengerTargetPerson.uid);
            if (!IsPassengerPerson(resolvedPassengerPerson))
            {
                return;
            }

            _passengerVrHandsPossessionStartedThisSession = true;

            PassengerHandPrePossessSnapshot.CaptureFromPersonBeforeHandPossess(
                resolvedPassengerPerson);
            NotifyPassengerHandsPossessionTriggeredForPalmHud();
            StartVrPassengerHandsRoutine(resolvedPassengerPerson);
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
                    "GabrielHud DEBUG passenger head neutralize: phase=" +
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
                "GabrielHud DEBUG passenger head neutralize: phase=" +
                phase +
                " person=" +
                (_passengerTargetPerson != null ?
                    _passengerTargetPerson.uid :
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

            if (_passengerTargetPerson != null)
            {
                FreeControllerV3 chest =
                    _passengerTargetPerson.GetStorableByID(
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
                        _passengerTargetPerson.GetStorableByID(
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
                        _passengerTargetPerson.GetStorableByID(
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
                    upAxis = _passengerTargetPerson.transform.up;
                    if (upAxis.sqrMagnitude < 1e-10f)
                        upAxis = Vector3.up;
                    upAxis.Normalize();
                    neutralForward = Vector3.ProjectOnPlane(
                        _passengerTargetPerson.transform.forward,
                        upAxis);
                    if (neutralForward.sqrMagnitude >= 1e-10f)
                        sourceName = "person.transform.forward";
                }
            }

            if (neutralForward.sqrMagnitude < 1e-10f &&
                _passengerHeadRigidbody != null)
            {
                upAxis = _passengerHeadRigidbody.transform.up;
                if (upAxis.sqrMagnitude < 1e-10f)
                    upAxis = Vector3.up;
                upAxis.Normalize();
                neutralForward = Vector3.ProjectOnPlane(
                    _passengerHeadRigidbody.transform.forward,
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
            if (_passengerHeadRigidbody == null)
                return Quaternion.identity;

            Quaternion headRotationWithOffset =
                _passengerHeadRigidbody.transform.rotation *
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
