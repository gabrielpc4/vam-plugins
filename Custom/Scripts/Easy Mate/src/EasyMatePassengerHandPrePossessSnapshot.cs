using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Saves each Person hand <see cref="FreeControllerV3"/> pose, link target, and
    /// position/rotation state (including Comply) before Easy Mate possesses hands for
    /// Passenger mode; restores them after <see cref="SuperController.ClearPossess"/>.
    /// </summary>
    internal static class EasyMatePassengerHandPrePossessSnapshot
    {
        private sealed class HandSideSnapshot
        {
            internal bool HasData;

            internal Vector3 WorldPosition;

            internal Quaternion WorldRotation;

            internal FreeControllerV3.PositionState PositionState;

            internal FreeControllerV3.RotationState RotationState;

            internal string LinkRigidbodyKey;
        }

        private static HandSideSnapshot _leftHandSnapshot;

        private static HandSideSnapshot _rightHandSnapshot;

        private static string _capturedTargetPersonUid;

        public static void CaptureFromPersonBeforeHandPossess(Atom person)
        {
            DiscardSnapshot();

            if (person == null || person.type != "Person" ||
                string.IsNullOrEmpty(person.uid))
            {
                return;
            }

            SuperController sc = SuperController.singleton;
            if (sc == null)
            {
                return;
            }

            _capturedTargetPersonUid = person.uid;

            FreeControllerV3 leftHand =
                person.GetStorableByID("lHandControl") as FreeControllerV3;
            FreeControllerV3 rightHand =
                person.GetStorableByID("rHandControl") as FreeControllerV3;

            _leftHandSnapshot = CaptureOneHand(leftHand);
            _rightHandSnapshot = CaptureOneHand(rightHand);
        }

        public static void RestoreAfterPossessClearThenDiscardSnapshot()
        {
            SuperController sc = SuperController.singleton;
            if (sc == null || string.IsNullOrEmpty(_capturedTargetPersonUid))
            {
                DiscardSnapshot();
                return;
            }

            Atom person = sc.GetAtomByUid(_capturedTargetPersonUid);
            if (person == null || person.type != "Person")
            {
                DiscardSnapshot();
                return;
            }

            FreeControllerV3 leftHand =
                person.GetStorableByID("lHandControl") as FreeControllerV3;
            FreeControllerV3 rightHand =
                person.GetStorableByID("rHandControl") as FreeControllerV3;

            try
            {
                RestoreOneHand(sc, leftHand, _leftHandSnapshot);
                RestoreOneHand(sc, rightHand, _rightHandSnapshot);
            }
            catch (Exception exception)
            {
                SuperController.LogError(
                    "Easy Mate: restoring pre-passenger hand state failed: " +
                    exception.Message);
            }
            finally
            {
                DiscardSnapshot();
            }
        }

        public static void DiscardSnapshot()
        {
            _leftHandSnapshot = null;
            _rightHandSnapshot = null;
            _capturedTargetPersonUid = null;
        }

        private static HandSideSnapshot CaptureOneHand(FreeControllerV3 handControl)
        {
            if (handControl == null)
            {
                return null;
            }

            HandSideSnapshot snapshot = new HandSideSnapshot();

            snapshot.HasData = true;
            if (handControl.control != null)
            {
                snapshot.WorldPosition = handControl.control.position;
                snapshot.WorldRotation = handControl.control.rotation;
            }

            snapshot.PositionState = handControl.currentPositionState;
            snapshot.RotationState = handControl.currentRotationState;

            snapshot.LinkRigidbodyKey = null;
            Rigidbody linkBody = handControl.linkToRB;
            if (linkBody != null)
            {
                string linkKey;
                if (TryBuildRigidbodyLinkKey(linkBody, out linkKey))
                {
                    snapshot.LinkRigidbodyKey = linkKey;
                }
            }

            return snapshot;
        }

        private static bool TryBuildRigidbodyLinkKey(
            Rigidbody rigidbody,
            out string linkKey)
        {
            linkKey = null;
            if (rigidbody == null)
            {
                return false;
            }

            Atom atom = null;
            FreeControllerV3 freeController =
                rigidbody.GetComponent<FreeControllerV3>();
            if (freeController != null)
            {
                atom = freeController.containingAtom;
            }

            if (atom == null)
            {
                ForceReceiver receiver =
                    rigidbody.GetComponent<ForceReceiver>();
                if (receiver != null)
                {
                    atom = receiver.containingAtom;
                }
            }

            if (atom == null)
            {
                return false;
            }

            linkKey = atom.uid + ":" + rigidbody.name;
            return true;
        }

        private static bool IsPositionLinkState(FreeControllerV3.PositionState state)
        {
            return state == FreeControllerV3.PositionState.ParentLink ||
                state == FreeControllerV3.PositionState.PhysicsLink;
        }

        private static bool IsRotationLinkState(FreeControllerV3.RotationState state)
        {
            return state == FreeControllerV3.RotationState.ParentLink ||
                state == FreeControllerV3.RotationState.PhysicsLink;
        }

        private static FreeControllerV3.SelectLinkState InferSelectLinkState(
            HandSideSnapshot snapshot)
        {
            bool positionLinked = IsPositionLinkState(snapshot.PositionState);
            bool rotationLinked = IsRotationLinkState(snapshot.RotationState);

            if (positionLinked && rotationLinked)
            {
                return FreeControllerV3.SelectLinkState.PositionAndRotation;
            }

            if (positionLinked)
            {
                return FreeControllerV3.SelectLinkState.Position;
            }

            if (rotationLinked)
            {
                return FreeControllerV3.SelectLinkState.Rotation;
            }

            return FreeControllerV3.SelectLinkState.PositionAndRotation;
        }

        private static void RestoreOneHand(
            SuperController sc,
            FreeControllerV3 handControl,
            HandSideSnapshot snapshot)
        {
            if (sc == null || handControl == null || snapshot == null ||
                !snapshot.HasData)
            {
                return;
            }

            handControl.possessed = false;

            handControl.SelectLinkToRigidbody(
                null,
                FreeControllerV3.SelectLinkState.PositionAndRotation,
                false,
                false);

            Rigidbody linkBody = null;
            if (!string.IsNullOrEmpty(snapshot.LinkRigidbodyKey))
            {
                linkBody =
                    sc.RigidbodyNameToRigidbody(snapshot.LinkRigidbodyKey);
            }

            bool usePhysicalLink =
                snapshot.PositionState ==
                FreeControllerV3.PositionState.PhysicsLink ||
                snapshot.RotationState ==
                FreeControllerV3.RotationState.PhysicsLink;

            bool positionLinked = IsPositionLinkState(snapshot.PositionState);
            bool rotationLinked = IsRotationLinkState(snapshot.RotationState);

            if (linkBody != null && (positionLinked || rotationLinked))
            {
                FreeControllerV3.SelectLinkState linkSelection =
                    InferSelectLinkState(snapshot);
                handControl.SelectLinkToRigidbody(
                    linkBody,
                    linkSelection,
                    usePhysicalLink,
                    true);
            }
            else if (linkBody == null &&
                (positionLinked || rotationLinked) &&
                !string.IsNullOrEmpty(snapshot.LinkRigidbodyKey))
            {
                SuperController.LogMessage(
                    "Easy Mate: pre-passenger hand link \"" +
                    snapshot.LinkRigidbodyKey +
                    "\" is missing; restoring world pose and On/Comply-style states only.");
            }

            FreeControllerV3.PositionState positionToApply = snapshot.PositionState;
            FreeControllerV3.RotationState rotationToApply = snapshot.RotationState;

            if (linkBody == null && positionLinked)
            {
                positionToApply = FreeControllerV3.PositionState.On;
            }

            if (linkBody == null && rotationLinked)
            {
                rotationToApply = FreeControllerV3.RotationState.On;
            }

            handControl.currentPositionState = positionToApply;
            handControl.currentRotationState = rotationToApply;

            if (handControl.control != null)
            {
                handControl.control.position = snapshot.WorldPosition;
                handControl.control.rotation = snapshot.WorldRotation;
            }

            if (handControl.followWhenOff != null)
            {
                handControl.followWhenOff.position = snapshot.WorldPosition;
            }
        }
    }
}
