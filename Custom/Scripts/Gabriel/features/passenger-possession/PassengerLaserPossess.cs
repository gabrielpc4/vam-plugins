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
    /// path and <see cref="PassengerRuntime.RequestPassengerForSpecificPerson"/> do.
    /// </summary>
    internal static class PassengerLaserPossess
    {
        private static float _nextPossessTriggerTime = -1f;

        /// <summary>Min seconds between successful laser+A triggers.</summary>
        private const float RetriggerCooldownSeconds = 0.2f;

        private const float BeamClosestPersonRefreshSeconds = 1f;

        private const int InitialBeamHitBufferSize = 32;

        private const int MaxBeamHitBufferSize = 256;

        private static RaycastHit[] _beamHits =
            new RaycastHit[InitialBeamHitBufferSize];

        private static float _nextBeamClosestPersonRefreshTime = -1f;

        private static Atom _cachedBeamClosestPerson;

        /// <summary>
        /// First <c>Person</c> collider along the beam, sorted by hit distance.
        /// </summary>
        internal static Atom FindFirstPersonAlongBeam(
            Vector3 origin,
            Vector3 direction,
            float length)
        {
            Vector3 rayDirection;
            bool saturated;
            int hitCount;

            if (direction.sqrMagnitude < 1e-12f)
            {
                return null;
            }

            if (Time.unscaledTime < _nextBeamClosestPersonRefreshTime)
                return _cachedBeamClosestPerson;

            rayDirection = direction.normalized;
            hitCount = RaycastBeamHits(
                origin,
                rayDirection,
                length,
                out saturated);
            if (hitCount == 0)
            {
                _cachedBeamClosestPerson = null;
                _nextBeamClosestPersonRefreshTime =
                    Time.unscaledTime + BeamClosestPersonRefreshSeconds;
                return null;
            }

            if (saturated)
            {
                RaycastHit[] overflowHits =
                    Physics.RaycastAll(origin, rayDirection, length);
                _cachedBeamClosestPerson = FindClosestPersonInHits(
                    overflowHits,
                    overflowHits != null ? overflowHits.Length : 0);
                _nextBeamClosestPersonRefreshTime =
                    Time.unscaledTime + BeamClosestPersonRefreshSeconds;
                return _cachedBeamClosestPerson;
            }

            _cachedBeamClosestPerson =
                FindClosestPersonInHits(_beamHits, hitCount);
            _nextBeamClosestPersonRefreshTime =
                Time.unscaledTime + BeamClosestPersonRefreshSeconds;
            return _cachedBeamClosestPerson;
        }

        internal static void ClearBeamClosestPersonCache()
        {
            _cachedBeamClosestPerson = null;
            _nextBeamClosestPersonRefreshTime = -1f;
        }

        private static Atom FindClosestPersonInHits(
            RaycastHit[] hits,
            int hitCount)
        {
            Atom bestPerson;
            float bestDistance;
            int hitIndex;

            if (hits == null || hitCount <= 0)
            {
                return null;
            }

            bestPerson = null;
            bestDistance = float.MaxValue;
            for (hitIndex = 0; hitIndex < hitCount; hitIndex++)
            {
                RaycastHit hit = hits[hitIndex];
                Atom hitPerson = TryResolvePersonFromHit(hit);
                if (hitPerson != null)
                {
                    if (hit.distance < bestDistance)
                    {
                        bestDistance = hit.distance;
                        bestPerson = hitPerson;
                    }
                }
            }

            return bestPerson;
        }

        private static int RaycastBeamHits(
            Vector3 origin,
            Vector3 direction,
            float length,
            out bool saturated)
        {
            int hitCount;
            int newSize;

            saturated = false;
            while (true)
            {
                hitCount = Physics.RaycastNonAlloc(
                    origin,
                    direction,
                    _beamHits,
                    length);
                if (hitCount < _beamHits.Length)
                {
                    return hitCount;
                }

                if (_beamHits.Length >= MaxBeamHitBufferSize)
                {
                    saturated = true;
                    return hitCount;
                }

                newSize = _beamHits.Length * 2;
                if (newSize > MaxBeamHitBufferSize)
                {
                    newSize = MaxBeamHitBufferSize;
                }

                _beamHits = new RaycastHit[newSize];
            }
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

            if (!PassengerRuntime.RequestPassengerForSpecificPerson(targetPerson))
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
