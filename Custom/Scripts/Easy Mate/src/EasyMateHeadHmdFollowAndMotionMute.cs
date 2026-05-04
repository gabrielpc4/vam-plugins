using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// When <c>headControl</c> is possessed and linked to the center camera
    /// rigidbody (HMD): Motion Animation cannot drive head <b>rotation</b>, and we
    /// re-apply the headset world rotation in <see cref="LateTick"/> so Timeline
    /// / Animation cannot leave the neck fighting the rig. When the head is not
    /// possessed, rotation suspend flags are cleared for that head&apos;s MACs.
    /// </summary>
    internal static class EasyMateHeadHmdFollowAndMotionMute
    {
        internal static void LateTick(SuperController sc)
        {
            if (sc == null || sc.isLoading)
                return;

            Rigidbody hmdRigidbody;

            hmdRigidbody = TryGetCenterCameraTargetRigidbody(sc);
            if (hmdRigidbody == null)
                return;

            foreach (Atom atom in sc.GetAtoms())
            {
                if (atom == null || atom.type != "Person" ||
                    !atom.gameObject.activeInHierarchy)
                    continue;

                FreeControllerV3 headControl;

                headControl = atom.GetStorableByID("headControl") as FreeControllerV3;
                if (headControl == null)
                    continue;

                if (headControl.possessed &&
                    headControl.linkToRB == hmdRigidbody)
                {
                    SetHeadMotionAnimationRotationSuspended(atom, headControl, true);
                    ApplyCenterEyeWorldRotationToHead(headControl, sc);
                }
                else if (!headControl.possessed)
                {
                    SetHeadMotionAnimationRotationSuspended(atom, headControl, false);
                }
            }
        }

        /// <summary>
        /// Same world rotation as the center camera target (VR eye / monitor rig root).
        /// Used by passenger pre-possession follow and by <see cref="LateTick"/>.
        /// </summary>
        internal static void ApplyCenterEyeWorldRotationToHead(
            FreeControllerV3 headControl,
            SuperController sc)
        {
            if (headControl == null || headControl.control == null || sc == null)
                return;

            Transform motionControllerHead;

            motionControllerHead = sc.centerCameraTarget != null
                ? sc.centerCameraTarget.transform
                : null;
            if (motionControllerHead == null)
                return;

            headControl.currentRotationState = FreeControllerV3.RotationState.On;
            headControl.control.rotation = motionControllerHead.rotation;

            if (headControl.followWhenOff != null)
            {
                headControl.followWhenOff.rotation =
                    motionControllerHead.rotation;
            }
        }

        /// <summary>
        /// Mirrors VaM&apos;s possess-head policy but applies to every
        /// <see cref="MotionAnimationControl"/> on the Person that targets
        /// <paramref name="head"/> (not only <c>GetComponent</c> on the same GO).
        /// </summary>
        internal static void ApplyHeadHmdLinkMotionSuspension(FreeControllerV3 head)
        {
            Atom person;

            person = head != null ? head.containingAtom : null;
            if (person == null || head == null)
                return;

            MotionAnimationControl[] macs;

            macs = person.motionAnimationControls;
            if (macs != null)
            {
                int macIndex;

                for (macIndex = 0; macIndex < macs.Length; macIndex++)
                {
                    MotionAnimationControl mac;

                    mac = macs[macIndex];
                    if (mac == null || mac.controller != head)
                        continue;
                    if (head.canGrabPosition)
                        mac.suspendPositionPlayback = true;
                    if (head.canGrabRotation)
                        mac.suspendRotationPlayback = true;
                }
            }

            MotionAnimationControl macOnHead;

            macOnHead = head.GetComponent<MotionAnimationControl>();
            if (macOnHead != null)
            {
                if (head.canGrabPosition)
                    macOnHead.suspendPositionPlayback = true;
                if (head.canGrabRotation)
                    macOnHead.suspendRotationPlayback = true;
            }
        }

        internal static void ClearHeadHmdLinkMotionSuspension(
            Atom person,
            FreeControllerV3 head)
        {
            if (person == null || head == null)
                return;

            MotionAnimationControl[] macs;

            macs = person.motionAnimationControls;
            if (macs != null)
            {
                int macIndex;

                for (macIndex = 0; macIndex < macs.Length; macIndex++)
                {
                    MotionAnimationControl mac;

                    mac = macs[macIndex];
                    if (mac == null || mac.controller != head)
                        continue;
                    mac.suspendPositionPlayback = false;
                    mac.suspendRotationPlayback = false;
                }
            }

            MotionAnimationControl macOnHead;

            macOnHead = head.GetComponent<MotionAnimationControl>();
            if (macOnHead != null)
            {
                macOnHead.suspendPositionPlayback = false;
                macOnHead.suspendRotationPlayback = false;
            }
        }

        private static Rigidbody TryGetCenterCameraTargetRigidbody(SuperController sc)
        {
            if (sc == null || sc.centerCameraTarget == null)
                return null;
            return sc.centerCameraTarget.GetComponent<Rigidbody>();
        }

        private static void SetHeadMotionAnimationRotationSuspended(
            Atom person,
            FreeControllerV3 head,
            bool suspendRotationPlayback)
        {
            if (person == null || head == null)
                return;

            MotionAnimationControl[] macs;

            macs = person.motionAnimationControls;
            if (macs != null)
            {
                int macIndex;

                for (macIndex = 0; macIndex < macs.Length; macIndex++)
                {
                    MotionAnimationControl mac;

                    mac = macs[macIndex];
                    if (mac == null || mac.controller != head)
                        continue;
                    mac.suspendRotationPlayback = suspendRotationPlayback;
                }
            }

            MotionAnimationControl macOnHead;

            macOnHead = head.GetComponent<MotionAnimationControl>();
            if (macOnHead != null)
                macOnHead.suspendRotationPlayback = suspendRotationPlayback;
        }
    }
}
