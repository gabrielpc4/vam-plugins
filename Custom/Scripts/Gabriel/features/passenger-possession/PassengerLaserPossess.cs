using System;
using MeshVR;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Start passenger mode when the <b>right</b> UI-aim beam (see
    /// <see cref="MonitorModeLaserRestore"/>) hits a Person and the user presses
    /// right face <b>A</b> / OpenVR Select (<see cref="VrInput.PollRightFaceADown"/>).
    /// Palm-hand back-of-hand pose HUD does not trigger passenger start — only this
    /// path and <see cref="GabrielHud.RequestPassengerForSpecificPerson"/> do.
    /// </summary>
    internal static class PassengerLaserPossess
    {
        private static float _nextPossessTriggerTime = -1f;

        /// <summary>Min seconds between successful laser+A triggers.</summary>
        private const float RetriggerCooldownSeconds = 0.2f;

        /// <summary>
        /// First <c>Person</c> collider along the beam, sorted by hit distance.
        /// </summary>
        internal static Atom FindFirstPersonAlongBeam(
            Vector3 origin,
            Vector3 direction,
            float length)
        {
            if (direction.sqrMagnitude < 1e-12f)
            {
                return null;
            }

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, length);
            if (hits == null || hits.Length == 0)
            {
                return null;
            }

            Array.Sort(hits, delegate(RaycastHit a, RaycastHit b)
            {
                return a.distance.CompareTo(b.distance);
            });

            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                Atom hitPerson = TryResolvePersonFromHit(hits[hitIndex]);
                if (hitPerson != null)
                {
                    return hitPerson;
                }
            }

            return null;
        }

        /// <summary>
        /// Only the <b>right</b> UI-aim beam (see <see cref="MonitorModeLaserRestore"/>)
        /// can arm a passenger start. Skips Edit mode, palm HUD visible, cooldown, and
        /// requires face A / OpenVR Select this frame.
        /// </summary>
        internal static void TryTriggerFromRightBeamPersonHit(
            SuperController sc,
            Atom rightPersonHit,
            bool palmHandHudVisible)
        {
            if (sc == null)
            {
                return;
            }

            if (sc.gameMode == SuperController.GameMode.Edit)
            {
                return;
            }

            if (palmHandHudVisible)
            {
                return;
            }

            if (Time.unscaledTime < _nextPossessTriggerTime)
            {
                return;
            }

            if (!VrInput.PollRightFaceADown(sc))
            {
                return;
            }

            Atom targetPerson = rightPersonHit;
            if (targetPerson == null)
            {
                return;
            }

            if (!GabrielHud.RequestPassengerForSpecificPerson(targetPerson))
            {
                return;
            }

            _nextPossessTriggerTime = Time.unscaledTime + RetriggerCooldownSeconds;
        }

        private static Atom TryResolvePersonFromHit(RaycastHit hit)
        {
            if (hit.collider == null)
            {
                return null;
            }

            Transform hitTransform = hit.collider.transform;
            if (hitTransform == null)
            {
                return null;
            }

            FreeControllerV3 freeController =
                hitTransform.GetComponentInParent<FreeControllerV3>();
            if (freeController != null &&
                freeController.containingAtom != null &&
                freeController.containingAtom.type == "Person")
            {
                return freeController.containingAtom;
            }

            ForceReceiver forceReceiver =
                hitTransform.GetComponentInParent<ForceReceiver>();
            if (forceReceiver != null &&
                forceReceiver.containingAtom != null &&
                forceReceiver.containingAtom.type == "Person")
            {
                return forceReceiver.containingAtom;
            }

            JSONStorable jsonStorable =
                hitTransform.GetComponentInParent<JSONStorable>();
            if (jsonStorable != null &&
                jsonStorable.containingAtom != null &&
                jsonStorable.containingAtom.type == "Person")
            {
                return jsonStorable.containingAtom;
            }

            return null;
        }
    }
}
