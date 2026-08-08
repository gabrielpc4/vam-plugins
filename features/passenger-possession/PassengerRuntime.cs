using System;
using System.Collections;
using System.Collections.Generic;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Passenger mode: navigation rig follows the model head
    /// <b>position</b> (Passenger-style); startup snap aligns rig once from the head.
    /// Per-frame HMD rotation uses <see cref="FreeControllerV3.AlignTo"/> with
    /// alsoRotateRb false — avoids pairing <c>control</c> and
    /// <c>followWhenOff</c> every frame (that caused one-frame mesh pops).
    /// ImprovedPoV setup; VR hand possession starts once per session when you
    /// press any grip or trigger (see
    /// <see cref="VrInput.PollVrAnyTriggerOrGripPressDown"/>). Start is requested from
    /// <see cref="PassengerLaserPossess"/> (right UI-aim laser + face A) via
    /// <see cref="PassengerRuntime.RequestPassengerForSpecificPerson"/>.
    /// </summary>
    internal static class PassengerRuntime
    {
        private const string ImprovedPoVPluginPath =
            "Custom/Scripts/features/improved-pov/ImprovedPoV.cs";
        private const string ImprovedPoVClassSuffix = "ImprovedPoV";

        private const float RotationSmoothingSeconds = 0.1985169f;
        private const float RotationOffsetXDegrees = 15.17952f;
        private const float PositionSmoothingSeconds = 0.05345887f;
        /// <summary>
        /// Forward offset along the head rigidbody forward when snapping the
        /// navigation rig to the passenger head (VaM <c>LookAtWithLimits</c>
        /// converges on <c>CameraTarget</c>; a larger value keeps the rig
        /// center farther in front of the skull).
        /// </summary>
        private const float PositionOffsetZMeters = 0.1149023f;
        /// <summary>
        /// World-space distance from each eye to its synthetic look target along
        /// torso (chest) forward — only direction matters for
        /// <see cref="LookAtWithLimits"/>.
        /// </summary>
        private const float PassengerChestForwardEyeLookDistanceMeters = 3f;
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
        /// <summary>
        /// Signed pitch (deg) of head vs torso horizontal at passenger start;
        /// see <see cref="ComputePassengerSignedHeadPitchVsTorso"/>.
        /// </summary>
        private static float _preservedInitialHeadPitchDegrees;

        private static bool _passengerEyeLookLeftPatched;
        private static bool _passengerEyeLookRightPatched;
        private static CameraTarget.CameraLocation _passengerSavedLeftLookLoc;
        private static CameraTarget.CameraLocation _passengerSavedRightLookLoc;
        private static Transform _passengerSavedLeftLookTarget;
        private static Transform _passengerSavedRightLookTarget;
        private static Transform _passengerEyeLookProxyLeft;
        private static Transform _passengerEyeLookProxyRight;

        /// <summary>
        /// VaM <see cref="EyesControl"/> was switched to <c>LookMode.None</c> so
        /// it stops driving <see cref="LookAtWithLimits"/> while passenger owns
        /// eye targets; restored on stop.
        /// </summary>
        private static bool _passengerEyesDriverDetached;

        private static string _passengerSavedEyesLookMode;

        private static Transform _passengerSavedEyesLookAt;

        /// <summary>
        /// <see cref="AnimationPattern"/> uses a child <see cref="MoveProducer"/>
        /// whose <c>receiver</c> drives a free controller; during passenger we
        /// clear that link for the target Person&apos;s
        /// <c>eyeTargetControl</c> and <c>headControl</c>, then restore it on
        /// exit.
        /// </summary>
        private static List<PassengerAnimationPatternMoveProducerState>
            _passengerAnimationPatternMoveProducerSnaps =
                new List<PassengerAnimationPatternMoveProducerState>();

        private sealed class PassengerAnimationPatternMoveProducerState
        {
            public MoveProducer moveProducer;
            public FreeControllerV3 savedReceiver;
        }

        private sealed class PassengerSunglassesClothingSnap
        {
            public DAZClothingItem Item;

            public bool SavedActive;
        }

        private static readonly List<PassengerSunglassesClothingSnap>
            PassengerSunglassesSnaps =
                new List<PassengerSunglassesClothingSnap>(4);

        private static bool PassengerSunglassesSnapsContainItem(
            DAZClothingItem item)
        {
            int idx;

            if (item == null)
            {
                return false;
            }

            for (idx = 0; idx < PassengerSunglassesSnaps.Count; idx++)
            {
                if (PassengerSunglassesSnaps[idx].Item == item)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tracks <paramref name="item"/> for passenger-stop restore exactly once (
        /// <see cref="RestorePassengerSunglassesClothing"/>).
        /// </summary>
        private static void EnsurePassengerSunglassesSnapTracked(
            DAZClothingItem item,
            bool savedActiveWas)
        {
            PassengerSunglassesClothingSnap snap;

            if (item == null || !savedActiveWas)
            {
                return;
            }

            if (PassengerSunglassesSnapsContainItem(item))
            {
                return;
            }

            snap = new PassengerSunglassesClothingSnap();
            snap.Item = item;
            snap.SavedActive = savedActiveWas;
            PassengerSunglassesSnaps.Add(snap);
        }

        /// <summary>
        /// VaM disables worn clothing correctly only through
        /// <see cref="DAZCharacterSelector.SetActiveClothingItem"/> (also toggles the
        /// garment Unity <c>GameObject</c>, clothing selector UI JSON, and runs
        /// <c>SyncAnatomy</c>). Do not assign <c>DAZClothingItem.active</c>
        /// alone—meshes usually stay rendered. VaMScripts-wide note:
        /// <c>features/clothing-interactions/FEATURE.md</c>.
        /// </summary>
        private static void HidePassengerSunglassesClothingViaSelector(
            DAZCharacterSelector selector,
            DAZClothingItem item)
        {
            if (item == null)
            {
                return;
            }

            if (selector != null)
            {
                selector.SetActiveClothingItem(item, false);
            }
            else
            {
                item.active = false;
                if (item.gameObject != null)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }

        private static void RestorePassengerSunglassesSingleViaSelectorOrFallback(
            DAZCharacterSelector selector,
            PassengerSunglassesClothingSnap snap)
        {
            if (snap.Item == null || !snap.SavedActive)
            {
                return;
            }

            if (selector != null)
            {
                selector.SetActiveClothingItem(snap.Item, true);
            }
            else
            {
                snap.Item.active = true;
                if (snap.Item.gameObject != null)
                {
                    snap.Item.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Signed pitch (deg): angle from torso-horizontal head forward to actual
        /// <c>headControl.forward</c> around the horizontal right axis (torso
        /// <c>up</c>).
        /// </summary>
        private static float ComputePassengerSignedHeadPitchVsTorso(
            FreeControllerV3 headControl)
        {
            Vector3 headFwd;
            if (headControl != null && headControl.control != null)
            {
                headFwd = headControl.control.forward;
            }
            else if (_passengerHeadRigidbody != null)
            {
                headFwd = _passengerHeadRigidbody.transform.forward;
            }
            else
            {
                return 0f;
            }

            string src;
            Vector3 torsoUp;
            Vector3 torsoFlatFwd = GetPassengerNeutralForward(
                headControl,
                out torsoUp,
                out src);
            if (torsoFlatFwd.sqrMagnitude < 1e-10f ||
                torsoUp.sqrMagnitude < 1e-10f)
            {
                return 0f;
            }

            torsoFlatFwd.Normalize();
            torsoUp.Normalize();

            Vector3 flatHead = Vector3.ProjectOnPlane(headFwd, torsoUp);
            if (flatHead.sqrMagnitude < 1e-10f)
            {
                return 0f;
            }

            flatHead.Normalize();
            Vector3 right = Vector3.Cross(torsoUp, flatHead);
            if (right.sqrMagnitude < 1e-10f)
            {
                return 0f;
            }

            right.Normalize();
            return Vector3.SignedAngle(flatHead, headFwd, right);
        }

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

        /// <summary>
        /// Called on same-folder idle→loading before <see cref="NotifySceneChanged"/>.
        /// If passenger owns the rig, returns the pose stored when passenger
        /// started; otherwise reads current navigation rig plus
        /// <see cref="SuperController.playerHeightAdjust"/>.
        /// </summary>
        public static bool TryGetRigPoseForSameFolderRestore(
            SuperController sc,
            out Vector3 position,
            out Quaternion rotation,
            out float playerHeightAdjust)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            playerHeightAdjust = 0f;

            if (sc == null || sc.navigationRig == null)
            {
                return false;
            }

            if (_isPassengerModeActive)
            {
                position = _previousNavigationRigPosition;
                rotation = _previousNavigationRigRotation;
                playerHeightAdjust = _previousPlayerHeightAdjust;
                return true;
            }

            position = sc.navigationRig.position;
            rotation = sc.navigationRig.rotation;
            playerHeightAdjust = sc.playerHeightAdjust;
            return true;
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

        /// <summary>
        /// Runs after animation <c>Update</c> (same frame) so eye-socket poses
        /// see motion / timeline before we place chest-forward proxies.
        /// </summary>
        public static void LateTick(MVRScript host)
        {
            SetSessionPluginHost(host);

            if (!_isPassengerModeActive)
            {
                return;
            }

            UpdatePassengerChestForwardEyeLookProxyPositions();
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

        /// <summary>
        /// Passenger follow drives <c>headControl</c> directly, so any saved VaM
        /// head possession from the scene must be cleared before the hands-only
        /// possess step starts.
        /// </summary>
        private static void ClearHeadPossessBeforePassengerHands(
            SuperController sc)
        {
            if (sc == null)
            {
                return;
            }

            try
            {
                sc.ClearHeadPossess();
            }
            catch (Exception exception)
            {
                SuperController.LogError(
                    "Easy Mate passenger hands clear head possess: " +
                    exception.Message);
            }
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

                ClearHeadPossessBeforePassengerHands(sc);
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
            SessionOrchestrator orchestrator =
                _sessionPluginHost as SessionOrchestrator;
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
                try
                {
                    RestorePassengerSunglassesClothing();
                }
                catch (Exception passengerEyewearRestoreException)
                {
                    SuperController.LogError(
                        "Easy Mate passenger eyewear clothing restore failed: " +
                            passengerEyewearRestoreException.Message);
                }

                try
                {
                    RestorePassengerChestForwardEyeLook(_passengerTargetPerson);
                }
                catch (Exception lookAtException)
                {
                    SuperController.LogError(
                        "Easy Mate passenger look-at restore failed: " +
                        lookAtException.Message);
                }

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
                _preservedInitialHeadPitchDegrees = 0f;
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
                PluginManager.TryMergePluginOntoPerson(
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

            FreeControllerV3 headControl =
                passengerPerson.GetStorableByID("headControl") as FreeControllerV3;
            _preservedInitialHeadPitchDegrees =
                ComputePassengerSignedHeadPitchVsTorso(headControl);
            ForcePassengerHeadControlNeutralRotation(headControl);
            ApplyPassengerChestForwardEyeLook(passengerPerson);
            ApplyPassengerSunglassesHideForTarget(passengerPerson);
        }

        /// <summary>
        /// Disables active classified eyewear slots when passenger starts using
        /// <see cref="ClothingClassifier.IsPassengerSunglassesClothing"/> and
        /// <see cref="DAZCharacterSelector.SetActiveClothingItem"/>.
        /// </summary>
        private static void ApplyPassengerSunglassesHideForTarget(Atom passengerPerson)
        {
            DAZCharacterSelector selector;
            DAZClothingItem[] items;
            int index;
            DAZClothingItem item;

            RestorePassengerSunglassesClothing();

            if (passengerPerson == null ||
                passengerPerson.type != "Person")
            {
                return;
            }

            selector = PersonAtomCache.TryGetCharacterSelector(passengerPerson);
            if (selector == null || selector.clothingItems == null)
            {
                return;
            }

            items = selector.clothingItems;
            for (index = 0; index < items.Length; index++)
            {
                item = items[index];
                if (item == null || !item.active)
                {
                    continue;
                }

                if (!ClothingClassifier.IsPassengerSunglassesClothing(item))
                {
                    continue;
                }

                EnsurePassengerSunglassesSnapTracked(item, true);
                HidePassengerSunglassesClothingViaSelector(selector, item);
            }
        }

        /// <summary>
        /// Turns suppressed eyewear back on after passenger exits.
        /// </summary>
        private static void RestorePassengerSunglassesClothing()
        {
            PassengerSunglassesClothingSnap snap;
            DAZCharacterSelector selectorRestore;
            int i;

            selectorRestore =
                PersonAtomCache.TryGetCharacterSelector(_passengerTargetPerson);

            for (i = 0; i < PassengerSunglassesSnaps.Count; i++)
            {
                snap = PassengerSunglassesSnaps[i];
                if (snap.Item == null)
                {
                    continue;
                }

                RestorePassengerSunglassesSingleViaSelectorOrFallback(
                    selectorRestore,
                    snap);
            }

            PassengerSunglassesSnaps.Clear();
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
                    _initialHeadNeutralizeFramesRemaining--;
                    return;
                }

                if (!IsPassengerHeadControlNeutralized(headControl))
                {
                    return;
                }

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

        /// <summary>
        /// Pick a world <c>up</c> for <see cref="Quaternion.LookRotation"/> on
        /// the navigation rig after the initial head delta. Torso up is wrong
        /// when it is nearly parallel to <paramref name="rigForward"/> (common
        /// when lying on the back looking at the ceiling).
        /// </summary>
        private static Vector3 ComputePassengerRigLookSnapUp(
            Vector3 rigForward,
            Vector3 torsoUpWorld,
            Vector3 rigUpBeforeSnap)
        {
            Vector3 fwd = rigForward;
            if (fwd.sqrMagnitude < 1e-12f)
            {
                return Vector3.up;
            }

            fwd.Normalize();

            Vector3 torso = torsoUpWorld;
            if (torso.sqrMagnitude < 1e-12f)
            {
                torso = Vector3.up;
            }
            else
            {
                torso.Normalize();
            }

            float parallel = Mathf.Abs(Vector3.Dot(fwd, torso));
            Vector3 upReference = torso;
            if (parallel > 0.88f)
            {
                upReference = rigUpBeforeSnap;
                if (upReference.sqrMagnitude < 1e-12f)
                {
                    upReference = Vector3.up;
                }
                else
                {
                    upReference.Normalize();
                }
            }

            Vector3 projected = Vector3.ProjectOnPlane(upReference, fwd);
            if (projected.sqrMagnitude < 1e-10f)
            {
                projected = Vector3.ProjectOnPlane(Vector3.up, fwd);
            }

            if (projected.sqrMagnitude < 1e-10f)
            {
                projected = Vector3.ProjectOnPlane(Vector3.right, fwd);
            }

            if (projected.sqrMagnitude < 1e-10f)
            {
                return Vector3.up;
            }

            return projected.normalized;
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
            Quaternion desiredHeadRotation = Quaternion.identity;
            Vector3 snapTorsoUp = Vector3.up;
            Quaternion navigationRigRotation;
            Quaternion headRotationDelta = Quaternion.identity;

            if (activeThisTurn)
            {
                desiredHeadRotation = BuildPassengerDesiredHeadRotation(
                    navigationRig.up,
                    out snapTorsoUp);
                navigationRigRotation = desiredHeadRotation;

                if (motionControllerHead != null)
                {
                    // Play-space up before the snap — stable roll reference when
                    // lying down (view forward ~ parallel to torso up).
                    Vector3 rigUpBeforeSnap = navigationRig.up;
                    headRotationDelta =
                        desiredHeadRotation *
                        Quaternion.Inverse(motionControllerHead.rotation);
                    navigationRigRotation =
                        headRotationDelta * navigationRig.rotation;

                    // Drop roll only: keep rig forward, orthonormalize up.
                    // Torso-up alone fails supine: HMD forward and chest.up can
                    // both align with world vertical, so LookRotation picks
                    // arbitrary roll (~90° camera tilt). Then fall back to
                    // pre-snap rig up projected perpendicular to forward.
                    Vector3 rigFwd = navigationRigRotation * Vector3.forward;
                    if (rigFwd.sqrMagnitude > 1e-12f &&
                        snapTorsoUp.sqrMagnitude > 1e-12f)
                    {
                        rigFwd.Normalize();
                        // First snap: world-level roll only (no twist from chest
                        // up in LookRotation). Near zenith/nadir, world up is
                        // degenerate — keep existing torso/rig-up fallback.
                        Vector3 rollFreeUp;
                        float fwdAbsDotWorldUp =
                            Mathf.Abs(Vector3.Dot(rigFwd, Vector3.up));
                        if (fwdAbsDotWorldUp > 0.985f)
                        {
                            rollFreeUp = ComputePassengerRigLookSnapUp(
                                rigFwd,
                                snapTorsoUp,
                                rigUpBeforeSnap);
                        }
                        else
                        {
                            rollFreeUp = Vector3.up;
                        }

                        navigationRigRotation = Quaternion.LookRotation(
                            rigFwd,
                            rollFreeUp);
                    }

                    navigationRig.rotation = navigationRigRotation;
                }
            }

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
                navigationRig.position = positionOffset;

                ApplyPassengerFirstSnapLateralCenter(
                    superController,
                    motionControllerHead);
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

        /// <summary>
        /// Same alignment as VaM head possession: <see cref="FreeControllerV3.AlignTo"/>
        /// maps <see cref="FreeControllerV3.PossessForwardAxis"/> /
        /// <see cref="FreeControllerV3.PossessUpAxis"/> from the HMD transform.
        /// A plain <c>LookRotation(hmd.forward, …)</c> assumes +Z is the nose
        /// axis; custom persons may use +X (or other), which reads as a large yaw
        /// error on the mesh while the camera stays correct.
        /// Uses <c>alsoRotateRb: false</c> so AlignTo does not pair control and
        /// <c>followWhenOff</c> each frame (<c>true</c> caused mesh pops).
        /// </summary>
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
            headControl.AlignTo(motionControllerHead, false);
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

        private static void RestorePassengerAnimationPatternMoveProducers()
        {
            if (_passengerAnimationPatternMoveProducerSnaps == null ||
                _passengerAnimationPatternMoveProducerSnaps.Count == 0)
            {
                return;
            }

            int i;
            for (i = 0; i < _passengerAnimationPatternMoveProducerSnaps.Count; i++)
            {
                PassengerAnimationPatternMoveProducerState s =
                    _passengerAnimationPatternMoveProducerSnaps[i];
                if (s.moveProducer != null)
                {
                    s.moveProducer.receiver = s.savedReceiver;
                }
            }

            _passengerAnimationPatternMoveProducerSnaps.Clear();
        }

        /// <summary>
        /// Breaks <see cref="MoveProducer.receiver"/> on every in-scene
        /// <see cref="AnimationPattern"/> that targets this Person&apos;s
        /// <c>eyeTargetControl</c> or <c>headControl</c> (same as setting
        /// Receiver to None in the pattern UI).
        /// </summary>
        private static void QuarantinePassengerAnimationPatternMoveProducers(
            Atom passengerPerson)
        {
            RestorePassengerAnimationPatternMoveProducers();

            if (passengerPerson == null)
            {
                return;
            }

            FreeControllerV3 eyeTargetFc =
                passengerPerson.GetStorableByID("eyeTargetControl") as
                FreeControllerV3;
            FreeControllerV3 headFc =
                passengerPerson.GetStorableByID("headControl") as FreeControllerV3;
            if (eyeTargetFc == null && headFc == null)
            {
                return;
            }

            SuperController sc = SuperController.singleton;
            if (sc == null)
            {
                return;
            }

            List<Atom> atoms = sc.GetAtoms();
            int atomIndex;
            for (atomIndex = 0; atomIndex < atoms.Count; atomIndex++)
            {
                Atom a = atoms[atomIndex];
                if (a == null)
                {
                    continue;
                }

                AnimationPattern[] patterns =
                    a.GetComponentsInChildren<AnimationPattern>(true);
                if (patterns == null)
                {
                    continue;
                }

                int p;
                for (p = 0; p < patterns.Length; p++)
                {
                    AnimationPattern pattern = patterns[p];
                    if (pattern == null || pattern.animatedTransform == null)
                    {
                        continue;
                    }

                    MoveProducer mp =
                        pattern.animatedTransform.GetComponent<MoveProducer>();
                    if (mp == null || mp.receiver == null)
                    {
                        continue;
                    }

                    FreeControllerV3 rcv = mp.receiver;
                    bool eyeMatch = eyeTargetFc != null && rcv == eyeTargetFc;
                    bool headMatch = headFc != null && rcv == headFc;
                    if (!eyeMatch && !headMatch)
                    {
                        continue;
                    }

                    PassengerAnimationPatternMoveProducerState state =
                        new PassengerAnimationPatternMoveProducerState();
                    state.moveProducer = mp;
                    state.savedReceiver = rcv;
                    _passengerAnimationPatternMoveProducerSnaps.Add(state);
                    mp.receiver = null;
                }
            }
        }

        /// <summary>
        /// Finds <c>lEye</c> / <c>rEye</c> <see cref="LookAtWithLimits"/> on a
        /// Person (same name convention as stock VaM).
        /// </summary>
        private static void FindEyeLookAtLimits(
            Atom passengerPerson,
            out LookAtWithLimits left,
            out LookAtWithLimits right)
        {
            left = null;
            right = null;
            if (passengerPerson == null)
            {
                return;
            }

            LookAtWithLimits[] eyes =
                passengerPerson.GetComponentsInChildren<LookAtWithLimits>(
                    true);
            for (int i = 0; i < eyes.Length; i++)
            {
                LookAtWithLimits e = eyes[i];
                if (e == null)
                {
                    continue;
                }

                if (e.name == "lEye")
                {
                    left = e;
                }
                else if (e.name == "rEye")
                {
                    right = e;
                }
            }
        }

        private static void EnsurePassengerEyeLookProxyTransforms(Atom person)
        {
            if (person == null)
            {
                return;
            }

            if (_passengerEyeLookProxyLeft == null)
            {
                GameObject go = new GameObject("VaMScriptsPassengerChestFwdEyeL");
                go.hideFlags = HideFlags.HideAndDontSave;
                _passengerEyeLookProxyLeft = go.transform;
                _passengerEyeLookProxyLeft.SetParent(person.transform, false);
            }

            if (_passengerEyeLookProxyRight == null)
            {
                GameObject go = new GameObject("VaMScriptsPassengerChestFwdEyeR");
                go.hideFlags = HideFlags.HideAndDontSave;
                _passengerEyeLookProxyRight = go.transform;
                _passengerEyeLookProxyRight.SetParent(person.transform, false);
            }
        }

        private static void DestroyPassengerEyeLookProxies()
        {
            if (_passengerEyeLookProxyLeft != null)
            {
                UnityEngine.Object.Destroy(_passengerEyeLookProxyLeft.gameObject);
                _passengerEyeLookProxyLeft = null;
            }

            if (_passengerEyeLookProxyRight != null)
            {
                UnityEngine.Object.Destroy(_passengerEyeLookProxyRight.gameObject);
                _passengerEyeLookProxyRight = null;
            }
        }

        /// <summary>
        /// Horizontal torso forward (chest preferred): eyes aim along this axis
        /// from each socket so head/HMD yaw does not steer the gaze.
        /// </summary>
        private static bool TryGetPassengerTorsoForward(
            Atom person,
            out Vector3 forward)
        {
            forward = Vector3.zero;
            if (person == null)
            {
                return false;
            }

            FreeControllerV3 chest =
                person.GetStorableByID("chestControl") as FreeControllerV3;
            FreeControllerV3 torso = chest;
            if (torso == null || torso.control == null)
            {
                torso =
                    person.GetStorableByID("pelvisControl") as FreeControllerV3;
            }

            if (torso == null || torso.control == null)
            {
                torso =
                    person.GetStorableByID("abdomenControl") as FreeControllerV3;
            }

            if (torso != null && torso.control != null)
            {
                Vector3 up = torso.control.up;
                if (up.sqrMagnitude < 1e-12f)
                {
                    up = Vector3.up;
                }

                up.Normalize();
                forward = Vector3.ProjectOnPlane(torso.control.forward, up);
                if (forward.sqrMagnitude < 1e-10f)
                {
                    forward = torso.control.forward;
                }

                forward.Normalize();
                return true;
            }

            if (_passengerHeadRigidbody != null)
            {
                forward = _passengerHeadRigidbody.transform.forward;
                if (forward.sqrMagnitude < 1e-12f)
                {
                    return false;
                }

                forward.Normalize();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Torso <b>yaw</b> in the world-horizontal plane (VaM up): proxy lies
        /// at the same world height as the eye so gaze does not pitch toward the
        /// chest when the torso leans.
        /// </summary>
        private static bool TryGetPassengerEyeLevelYawForward(
            Atom person,
            out Vector3 forward)
        {
            forward = Vector3.zero;
            if (person == null)
            {
                return false;
            }

            Vector3 torsoFwd;
            if (!TryGetPassengerTorsoForward(person, out torsoFwd))
            {
                return false;
            }

            forward = Vector3.ProjectOnPlane(torsoFwd, Vector3.up);
            if (forward.sqrMagnitude < 1e-10f)
            {
                forward = Vector3.Scale(
                    torsoFwd,
                    new Vector3(1f, 0f, 1f));
            }

            if (forward.sqrMagnitude < 1e-10f)
            {
                return false;
            }

            forward.Normalize();
            return true;
        }

        private static void UpdatePassengerChestForwardEyeLookProxyPositions()
        {
            if (!_passengerEyeLookLeftPatched && !_passengerEyeLookRightPatched)
            {
                return;
            }

            if (_passengerTargetPerson == null)
            {
                return;
            }

            Vector3 eyeLevelFwd;
            if (!TryGetPassengerEyeLevelYawForward(
                    _passengerTargetPerson,
                    out eyeLevelFwd))
            {
                return;
            }

            LookAtWithLimits left;
            LookAtWithLimits right;
            FindEyeLookAtLimits(_passengerTargetPerson, out left, out right);

            if (left != null &&
                _passengerEyeLookLeftPatched &&
                _passengerEyeLookProxyLeft != null)
            {
                Transform le = left.transform;
                _passengerEyeLookProxyLeft.position =
                    le.position +
                    eyeLevelFwd * PassengerChestForwardEyeLookDistanceMeters;
            }

            if (right != null &&
                _passengerEyeLookRightPatched &&
                _passengerEyeLookProxyRight != null)
            {
                Transform re = right.transform;
                _passengerEyeLookProxyRight.position =
                    re.position +
                    eyeLevelFwd * PassengerChestForwardEyeLookDistanceMeters;
            }
        }

        /// <summary>
        /// Repoints each eye <see cref="LookAtWithLimits"/> at a synthetic
        /// target along <b>world-horizontal</b> torso yaw from that eye socket
        /// (eye height, character-forward - not head-local, not the HMD).
        /// </summary>
        private static void ApplyPassengerChestForwardEyeLook(Atom passengerPerson)
        {
            _passengerEyeLookLeftPatched = false;
            _passengerEyeLookRightPatched = false;
            DestroyPassengerEyeLookProxies();

            if (passengerPerson == null ||
                passengerPerson.type != "Person")
            {
                return;
            }

            LookAtWithLimits left;
            LookAtWithLimits right;
            FindEyeLookAtLimits(passengerPerson, out left, out right);
            if (left == null && right == null)
            {
                return;
            }

            EyesControl eyesControl =
                passengerPerson.GetStorableByID("Eyes") as EyesControl;
            if (eyesControl != null)
            {
                _passengerSavedEyesLookMode =
                    eyesControl.currentLookMode.ToString();
                _passengerSavedEyesLookAt = eyesControl.lookAt;
                eyesControl.currentLookMode = EyesControl.LookMode.None;
                _passengerEyesDriverDetached = true;
            }

            EnsurePassengerEyeLookProxyTransforms(passengerPerson);

            if (left != null)
            {
                _passengerSavedLeftLookLoc = left.lookAtCameraLocation;
                _passengerSavedLeftLookTarget = left.target;
                left.lookAtCameraLocation = CameraTarget.CameraLocation.None;
                left.target = _passengerEyeLookProxyLeft;
                left.enabled = true;
                left.on = true;
                _passengerEyeLookLeftPatched = true;
            }

            if (right != null)
            {
                _passengerSavedRightLookLoc = right.lookAtCameraLocation;
                _passengerSavedRightLookTarget = right.target;
                right.lookAtCameraLocation = CameraTarget.CameraLocation.None;
                right.target = _passengerEyeLookProxyRight;
                right.enabled = true;
                right.on = true;
                _passengerEyeLookRightPatched = true;
            }

            QuarantinePassengerAnimationPatternMoveProducers(passengerPerson);
            UpdatePassengerChestForwardEyeLookProxyPositions();
        }

        private static void RestorePassengerChestForwardEyeLook(Atom passengerPerson)
        {
            RestorePassengerAnimationPatternMoveProducers();

            if (passengerPerson != null && passengerPerson.type == "Person")
            {
                LookAtWithLimits left;
                LookAtWithLimits right;
                FindEyeLookAtLimits(passengerPerson, out left, out right);
                if (left != null && _passengerEyeLookLeftPatched)
                {
                    left.lookAtCameraLocation = _passengerSavedLeftLookLoc;
                    left.target = _passengerSavedLeftLookTarget;
                }

                if (right != null && _passengerEyeLookRightPatched)
                {
                    right.lookAtCameraLocation = _passengerSavedRightLookLoc;
                    right.target = _passengerSavedRightLookTarget;
                }
            }

            _passengerEyeLookLeftPatched = false;
            _passengerEyeLookRightPatched = false;
            DestroyPassengerEyeLookProxies();

            if (_passengerEyesDriverDetached &&
                passengerPerson != null &&
                passengerPerson.type == "Person")
            {
                EyesControl eyesControl =
                    passengerPerson.GetStorableByID("Eyes") as EyesControl;
                if (eyesControl != null &&
                    !string.IsNullOrEmpty(_passengerSavedEyesLookMode))
                {
                    eyesControl.lookAt = _passengerSavedEyesLookAt;
                    eyesControl.SetLookMode(_passengerSavedEyesLookMode);
                }

                _passengerEyesDriverDetached = false;
                _passengerSavedEyesLookMode = null;
                _passengerSavedEyesLookAt = null;
            }
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
            if (upAxis.sqrMagnitude < 1e-10f)
            {
                upAxis = Vector3.up;
            }

            upAxis.Normalize();
            Vector3 rightNeutral = Vector3.Cross(upAxis, neutralForward);
            if (rightNeutral.sqrMagnitude < 1e-10f)
            {
                return Quaternion.LookRotation(neutralForward, upAxis);
            }

            rightNeutral.Normalize();
            return
                Quaternion.AngleAxis(_preservedInitialHeadPitchDegrees, rightNeutral) *
                Quaternion.LookRotation(neutralForward, upAxis);
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
            out Vector3 resolvedTorsoUp)
        {
            resolvedTorsoUp = Vector3.up;
            if (_passengerHeadRigidbody == null)
            {
                return Quaternion.identity;
            }

            string sourceName;
            Vector3 stableUpAxis;
            Vector3 neutralForward = GetPassengerNeutralForward(
                null,
                out stableUpAxis,
                out sourceName);
            if (stableUpAxis.sqrMagnitude >= 1e-10f)
            {
                upAxis = stableUpAxis;
            }

            resolvedTorsoUp = upAxis;

            if (neutralForward.sqrMagnitude < 1e-10f)
            {
                return Quaternion.identity;
            }

            neutralForward.Normalize();
            if (upAxis.sqrMagnitude < 1e-10f)
            {
                upAxis = Vector3.up;
            }

            upAxis.Normalize();

            float pitchDegrees =
                _preservedInitialHeadPitchDegrees + RotationOffsetXDegrees;

            // Chest horizontal yaw only (not head twist on that axis); pitch is
            // head nod in torso frame (same axis as preserved vs. horizontal).
            Vector3 rightChest = Vector3.Cross(upAxis, neutralForward);
            if (rightChest.sqrMagnitude < 1e-10f)
            {
                return Quaternion.LookRotation(neutralForward, upAxis);
            }

            rightChest.Normalize();
            Quaternion neutralRotation = Quaternion.LookRotation(
                neutralForward,
                upAxis);
            Quaternion result =
                Quaternion.AngleAxis(pitchDegrees, rightChest) *
                neutralRotation;

            return result;
        }

        private static float NormalizeSignedEulerAngle(float eulerAngle)
        {
            if (eulerAngle > 180f)
                return eulerAngle - 360f;

            return eulerAngle;
        }
    }
}
