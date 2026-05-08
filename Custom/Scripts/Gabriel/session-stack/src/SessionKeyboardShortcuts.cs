using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace geesp0t
{
    /// <summary>
    /// Keyboard shortcuts for the session Auto Load Person Plugins plugin (e.g.
    /// <b>Space</b> = force release scene settle pause / exposure hold even while
    /// loading; <b>P</b> = possess+align+select Person under look or closest).
    /// World-space HUD was removed; use the plugin panel for load/sets.
    /// </summary>
    public class SessionKeyboardShortcuts
    {
        private static MVRScript _pluginHost;
        private static Coroutine _autoPossessCoroutine;
        private static OnSceneStartup _onSceneStartup;

        public void Init(MVRScript host, OnSceneStartup onSceneStartup)
        {
            _pluginHost = host;
            _onSceneStartup = onSceneStartup;
        }

        public void ProcessHotkeysUpdate()
        {
            if (_pluginHost == null || SuperController.singleton == null)
                return;

            bool noTextFocus =
                EventSystem.current == null ||
                EventSystem.current.currentSelectedGameObject == null;
            bool noCtrlAlt =
                !Input.GetKey(KeyCode.LeftControl) &&
                !Input.GetKey(KeyCode.RightControl) &&
                !Input.GetKey(KeyCode.LeftAlt) &&
                !Input.GetKey(KeyCode.RightAlt);

            if (noTextFocus && noCtrlAlt && Input.GetKeyDown(KeyCode.Space) &&
                _onSceneStartup != null)
            {
                if (_onSceneStartup.ForceReleaseSceneSettleHoldUserKey())
                {
                    HeadProximityHide.AfterSuperControllerFinishedSceneSettle(
                        _pluginHost);
                    SuperController.LogMessage(
                        "Auto_Load: Space — scene settle hold released.");
                }
                return;
            }

            if (SuperController.singleton.isLoading)
                return;
            if (!noTextFocus)
                return;
            if (!Input.GetKeyDown(KeyCode.P))
                return;
            if (!noCtrlAlt)
                return;

            PossessAlignSelectPersonUnderLookOrClosest();
        }

        public void OnDestroy()
        {
            StopAutoPossessRoutine();
            _pluginHost = null;
            _onSceneStartup = null;
        }

        private static bool TryGetLookCamera(out Camera cam)
        {
            cam = null;
            SuperController sc = SuperController.singleton;
            if (sc == null)
                return false;
            if (sc.lookCamera != null)
            {
                cam = sc.lookCamera;
                return true;
            }
            if (sc.MonitorCenterCamera != null)
            {
                cam = sc.MonitorCenterCamera;
                return true;
            }
            if (CameraTarget.centerTarget != null && CameraTarget.centerTarget.targetCamera != null)
            {
                cam = CameraTarget.centerTarget.targetCamera;
                return true;
            }
            return false;
        }

        private static Atom ResolvePersonAtomFromHitTransform(Transform t)
        {
            while (t != null)
            {
                FreeControllerV3 fc = t.GetComponent<FreeControllerV3>();
                if (fc != null && fc.containingAtom != null && fc.containingAtom.type == "Person" &&
                    fc.containingAtom.on && !fc.containingAtom.hidden && fc.containingAtom.gameObject.activeInHierarchy)
                    return fc.containingAtom;
                t = t.parent;
            }
            return null;
        }

        private static float PointToRayDistanceSq(Vector3 rayOrigin, Vector3 rayDirUnit, Vector3 point)
        {
            float t = Vector3.Dot(point - rayOrigin, rayDirUnit);
            Vector3 closest = rayOrigin + rayDirUnit * Mathf.Max(0f, t);
            return (point - closest).sqrMagnitude;
        }

        private static Vector3 GetPersonHeadWorldPosition(Atom person)
        {
            if (person == null)
                return Vector3.zero;
            FreeControllerV3 head = person.GetStorableByID("headControl") as FreeControllerV3;
            if (head != null && head.followWhenOff != null)
                return head.followWhenOff.position;
            return person.transform.position;
        }

        private static Atom FindPersonForPossessFromLookCamera()
        {
            SuperController sc = SuperController.singleton;
            Camera cam;
            if (sc == null || !TryGetLookCamera(out cam))
                return null;

            Vector3 origin = cam.transform.position;
            Vector3 dir = cam.transform.forward;
            if (dir.sqrMagnitude < 1e-10f)
                return null;
            dir.Normalize();

            const float maxRayLength = 40f;
            RaycastHit[] hits = Physics.RaycastAll(origin, dir, maxRayLength);
            if (hits != null && hits.Length > 1)
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            if (hits != null)
            {
                foreach (RaycastHit hit in hits)
                {
                    Atom fromHit = ResolvePersonAtomFromHitTransform(hit.transform);
                    if (fromHit != null)
                        return fromHit;
                }
            }

            Atom bestRay = null;
            float bestRayScore = float.MaxValue;
            Atom bestDist = null;
            float bestDistSq = float.MaxValue;

            foreach (Atom a in sc.GetAtoms())
            {
                if (a == null || a.type != "Person" || !a.on || a.hidden || !a.gameObject.activeInHierarchy)
                    continue;

                Vector3 headPos = GetPersonHeadWorldPosition(a);
                float lateralSq = PointToRayDistanceSq(origin, dir, headPos);
                float along = Vector3.Dot(headPos - origin, dir);
                if (along < -3f)
                    continue;

                float rayScore = lateralSq + along * along * 0.0004f;
                if (rayScore < bestRayScore)
                {
                    bestRayScore = rayScore;
                    bestRay = a;
                }

                float dSq = (headPos - origin).sqrMagnitude;
                if (dSq < bestDistSq)
                {
                    bestDistSq = dSq;
                    bestDist = a;
                }
            }

            if (bestRay != null)
                return bestRay;
            return bestDist;
        }

        private static void PossessAlignSelectPersonUnderLookOrClosest()
        {
            Atom target = FindPersonForPossessFromLookCamera();
            if (target == null)
            {
                SuperController.LogMessage("Auto_Load: P — no Person in scene to possess.");
                return;
            }

            StartAutoPossessRoutine(target, "P-look");
        }

        private static Transform GetMotionControllerTransform(SuperController sc, bool left)
        {
            if (sc == null)
                return null;
            return left ? sc.leftHand : sc.rightHand;
        }

        private static bool TryPrepareHeadForPossessAndAlign(SuperController sc, FreeControllerV3 head, out string error)
        {
            error = null;
            if (sc == null || head == null)
            {
                error = "missing SuperController or headControl";
                return false;
            }

            Transform motionControllerHead = sc.centerCameraTarget != null ? sc.centerCameraTarget.transform : null;
            Possessor possessor = motionControllerHead != null ? motionControllerHead.GetComponent<Possessor>() : null;
            Transform navigationRig = sc.navigationRig;
            if (motionControllerHead == null || possessor == null || possessor.autoSnapPoint == null || navigationRig == null)
            {
                error = "missing centerCameraTarget, Possessor, autoSnapPoint, or navigationRig";
                return false;
            }

            try
            {
                Vector3 forwardPossessAxis = head.GetForwardPossessAxis();
                Vector3 upPossessAxis = head.GetUpPossessAxis();
                Vector3 up = navigationRig.up;
                Vector3 fromDirection = Vector3.ProjectOnPlane(motionControllerHead.forward, up);
                Vector3 desiredForward = Vector3.ProjectOnPlane(forwardPossessAxis, navigationRig.up);
                if (Vector3.Dot(upPossessAxis, up) < 0f && Vector3.Dot(motionControllerHead.up, up) > 0f)
                    desiredForward = -desiredForward;

                if (fromDirection.sqrMagnitude > 1e-8f && desiredForward.sqrMagnitude > 1e-8f)
                {
                    Quaternion q = Quaternion.FromToRotation(fromDirection, desiredForward);
                    navigationRig.rotation = q * navigationRig.rotation;
                }

                if (head.canGrabRotation)
                    head.AlignTo(possessor.autoSnapPoint, true);

                Vector3 possessAnchor = head.possessPoint != null ? head.possessPoint.position : head.control.position;
                Vector3 delta = possessAnchor - possessor.autoSnapPoint.position;
                Vector3 targetRigPos = navigationRig.position + delta;
                float verticalDelta = Vector3.Dot(targetRigPos - navigationRig.position, up);
                targetRigPos += up * (0f - verticalDelta);
                navigationRig.position = targetRigPos;
                sc.playerHeightAdjust += verticalDelta;

                if (sc.MonitorCenterCamera != null)
                {
                    sc.MonitorCenterCamera.transform.LookAt(head.transform.position + forwardPossessAxis);
                    Vector3 euler = sc.MonitorCenterCamera.transform.localEulerAngles;
                    euler.y = 0f;
                    euler.z = 0f;
                    sc.MonitorCenterCamera.transform.localEulerAngles = euler;
                }

                head.PossessMoveAndAlignTo(possessor.autoSnapPoint);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private static bool TryPrepareHandForPossess(SuperController sc, FreeControllerV3 controller, bool left, out string error)
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

        private static bool TryDriveControllerIntoPossessOverlap(SuperController sc, FreeControllerV3 controller, bool left, out string error)
        {
            error = null;
            if (controller == null)
                return true;
            if (controller.possessed)
                return true;
            return TryPrepareHandForPossess(sc, controller, left, out error);
        }

        private static void StopAutoPossessRoutine()
        {
            if (_pluginHost != null && _autoPossessCoroutine != null)
            {
                _pluginHost.StopCoroutine(_autoPossessCoroutine);
                _autoPossessCoroutine = null;
            }
        }

        private static void StartAutoPossessRoutine(Atom person, string label)
        {
            if (_pluginHost == null)
            {
                SuperController.LogError("Auto_Load: Possess+Align+Select " + label + " — plugin host missing.");
                return;
            }

            StopAutoPossessRoutine();
            _autoPossessCoroutine = _pluginHost.StartCoroutine(PossessAlignSelectRoutine(person, label));
        }

        private static IEnumerator PossessAlignSelectRoutine(Atom person, string label)
        {
            try
            {
                HeadProximityHide.RestoreTransientHeadHideState();

                SuperController sc = SuperController.singleton;
                if (sc == null || person == null || person.type != "Person")
                    yield break;

                FreeControllerV3 head = person.GetStorableByID("headControl") as FreeControllerV3;
                FreeControllerV3 leftHand = person.GetStorableByID("lHandControl") as FreeControllerV3;
                FreeControllerV3 rightHand = person.GetStorableByID("rHandControl") as FreeControllerV3;
                if (head == null)
                {
                    SuperController.LogError("Auto_Load: Possess+Align+Select " + label + " — no headControl on " + person.name);
                    yield break;
                }

                sc.ClearPossess();
                yield return null;

                string headError;
                if (!TryPrepareHeadForPossessAndAlign(sc, head, out headError))
                {
                    SuperController.LogError("Auto_Load: Possess+Align+Select " + label + " head failed: " + headError);
                    yield break;
                }

                sc.SelectController(head, false);
                sc.SelectModePossess(true);

                yield return null;
                yield return null;

                bool headDone = head.possessed;
                bool leftDone = leftHand == null || leftHand.possessed;
                bool rightDone = rightHand == null || rightHand.possessed;
                string headPrepError = null;
                string leftError = null;
                string rightError = null;

                for (int i = 0; i < 120 && (!headDone || !leftDone || !rightDone); i++)
                {
                    if (!headDone)
                    {
                        TryPrepareHeadForPossessAndAlign(sc, head, out headPrepError);
                        headDone = head.possessed;
                    }
                    if (!leftDone)
                    {
                        TryDriveControllerIntoPossessOverlap(sc, leftHand, true, out leftError);
                        leftDone = leftHand != null && leftHand.possessed;
                    }
                    if (!rightDone)
                    {
                        TryDriveControllerIntoPossessOverlap(sc, rightHand, false, out rightError);
                        rightDone = rightHand != null && rightHand.possessed;
                    }

                    if (!headDone || !leftDone || !rightDone)
                        yield return null;
                }

                if (!headDone || !leftDone || !rightDone)
                    sc.SelectModeOff();

                sc.SelectController(head, false);

                SuperController.LogMessage("Auto_Load: Possess+Align+Select " + label + " — " + person.name +
                    " (head " + (headDone ? "ok" : "failed") + ", left " + (leftDone ? "ok" : "failed") + ", right " + (rightDone ? "ok" : "failed") + ").");
            }
            finally
            {
                _autoPossessCoroutine = null;
            }
        }
    }
}
