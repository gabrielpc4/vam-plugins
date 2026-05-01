using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.XR;
namespace geesp0t
{
    //Adapted from VAMDeluxe Dollmaster UI
    public class MainUIButtons
    {

        MVRScript plugin;

        private Camera _mainCamera;
        public static Canvas canvas = null;
        private float UIScale = 1.0f;

        UIDynamicButton loadNowButton = null;
        public UIDynamicButton autoLoadButton = null;
        UIDynamicButton cycleSetButton = null;
        UIDynamicButton cycleSet2Button = null;
        UIDynamicButton createMaleButton = null;
        UIDynamicButton maleImprovedPOVLickingButton = null;

        public bool wantToChangeAutoLoadType = false;
        public bool wantToLoadNow = false;
        public bool wantsToCycleSet = false;
        public bool wantsToCycleSet2 = false;
        public bool wantsToCreateMaleAtom = false;
        public bool wantsToRemoveMaleAtom = false;
        public bool wantsToStartLicking = false;
        public bool wantsToStopLicking = false;
        public bool createdMale = false;
        public bool isLicking = false;

        private bool isDesktopMode = false;

        public string cycleSetText = "";
        public string cycleSet2Text = "";

        public int autoLoadType = 0;

        private static MVRScript _pluginHost;
        private static Coroutine _autoPossessCoroutine;

        public void Init(MVRScript _plugin)
        {
            plugin = _plugin;
            _pluginHost = _plugin;
            _mainCamera = CameraTarget.centerTarget?.targetCamera;
            isDesktopMode = !(SuperController.singleton.isOVR || SuperController.singleton.isOpenVR);
        }

        /// <summary>Desktop <c>P</c>: possess + align + select the Person under the look ray (session plugin build).</summary>
        public void ProcessHotkeysUpdate()
        {
            if (plugin == null || SuperController.singleton == null)
                return;
            if (SuperController.singleton.isLoading)
                return;
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                return;
            if (!Input.GetKeyDown(KeyCode.P))
                return;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                return;

            PossessAlignSelectPersonUnderLookOrClosest();
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
                EasyMateHeadSnapPovRuntime.EndSnapSession();

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

        public void Start()
        {
            Cleanup();
            float worldScale = SuperController.singleton.worldScale;
            SuperController.singleton.worldScale = 1.0f;
            CreateButtons();
            SuperController.singleton.worldScale = worldScale;
        }

        public void ShowUI(bool setToActive)
        {
            if (loadNowButton != null) { 
                loadNowButton.gameObject.SetActive(setToActive);
                autoLoadButton.gameObject.SetActive(setToActive);
                cycleSetButton.gameObject.SetActive(setToActive);
                cycleSet2Button.gameObject.SetActive(setToActive);
                createMaleButton.gameObject.SetActive(setToActive);
                maleImprovedPOVLickingButton.gameObject.SetActive(createdMale);
            }
        }


        public void Cleanup()
        {
            if (canvas != null)
            {
                if (SuperController.singleton != null)
                {
                    SuperController.singleton.RemoveCanvas(canvas);
                }

                canvas.transform.SetParent(null, false);

                if (canvas.gameObject != null)
                {
                    GameObject.Destroy(canvas.gameObject);
                }
            }
        }

        public void CreateButtons()
        {
            float scale = 0.001f;

            Cleanup();

            GameObject canvasObject = new GameObject();
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.pixelPerfect = false;
            SuperController.singleton.AddCanvas(canvas);

            canvas.transform.SetParent(SuperController.singleton.mainHUD, false);

            CanvasScaler cs = canvasObject.AddComponent<CanvasScaler>();
            cs.scaleFactor = 80.0f;
            cs.dynamicPixelsPerUnit = 1f;

            GraphicRaycaster gr = canvasObject.AddComponent<GraphicRaycaster>();

            canvas.transform.localScale = new Vector3(scale, scale, scale);
            //canvas.transform.localPosition = new Vector3(-0.7f, 0, 0);

            canvas.transform.localPosition = new Vector3(-.45f, -0.72f, 0.35f);

            LookAtCamera();

            loadNowButton = AddButton("Load Plugins Now", () =>
            {
                wantToLoadNow = true;
            }, 1, -1);

            autoLoadButton = AddButton("Auto Load Disabled", () =>
            {
                wantToChangeAutoLoadType = true;
             }, 1, 2);

            cycleSetButton = AddButton("Cycle Set 1", () =>
            {
                wantsToCycleSet = true;
            }, 1, 0);

            cycleSet2Button = AddButton("Cycle Set 2", () =>
            {
                wantsToCycleSet2 = true;
            }, 1, 1);
            
            createMaleButton = AddButton("Create Male", () =>
            {
                if (createdMale)
                {
                    wantsToRemoveMaleAtom = true;
                    createdMale = false;
                    isLicking = false;
                    SetButtonText();
                }
                else
                {
                    wantsToCreateMaleAtom = true;
                    createdMale = true;
                    isLicking = false;
                    SetButtonText();
                }
            }, 2, -1);

            maleImprovedPOVLickingButton = AddButton("Male Starts Licking", () =>
            {
                if (isLicking)
                {
                    wantsToStopLicking = true;
                    isLicking = false;
                    SetButtonText();
                }
                else
                {
                    wantsToStartLicking = true;
                    isLicking = true;
                    SetButtonText();
                }
            }, 3, -1);

            SetButtonText();

            canvas.transform.Translate(0, 0.2f, 0);
        }

        public void SetButtonText()
        {
            cycleSetButton.buttonText.text = cycleSetText;
            cycleSet2Button.buttonText.text = cycleSet2Text;

            if (createdMale)
            {
                createMaleButton.buttonText.text = "Remove Male Person";
                maleImprovedPOVLickingButton.gameObject.SetActive(true);
            }
            else
            {
                createMaleButton.buttonText.text = "Add Male Person";
                maleImprovedPOVLickingButton.gameObject.SetActive(false);
            }

            if (isLicking)
            {
                maleImprovedPOVLickingButton.buttonText.text = "Male Stops Licking";
            }
            else
            {
                maleImprovedPOVLickingButton.buttonText.text = "Male Starts Licking";
            }

            if (autoLoadType == 0)
            {
                autoLoadButton.buttonText.text = "Auto Load Disabled >";
                loadNowButton.gameObject.SetActive(true);
            } else if (autoLoadType == 1)
            {
                autoLoadButton.buttonText.text = "Auto Load for Solo Scenes >";
                loadNowButton.gameObject.SetActive(false);
            } else if (autoLoadType == 2)
            {
                autoLoadButton.buttonText.text = "Auto Load for All Scenes >";
                loadNowButton.gameObject.SetActive(false);
            }
            else if (autoLoadType == 3)
            {
                autoLoadButton.buttonText.text = "Custom SETTINGS >";
                loadNowButton.gameObject.SetActive(false);
            }

            if (cycleSet2Button.buttonText.text == "Add VAM Launch >")
            {
                loadNowButton.buttonText.text = "Add Launch Only";
            }
            else
            {
                loadNowButton.buttonText.text = "Load Plugins Now";
            }
        }


        public UIDynamicButton AddButton(string name, UnityAction callback, int column, int row)
        {
            Color accessButtonColor = new Color(0.8392f, 0.8392f, 0.8392f);
            Color accessTextColor = new Color(0, 0, 0);
            float xSpacing = 0.22f;
            float ySpacing = 0.05f;

            UIDynamicButton button = CreateButton(name, 100, 40);
            button.button.onClick.AddListener(callback);
            button.transform.Translate(column * xSpacing, 0.45f - row * ySpacing, 0, Space.Self);
            ColorButton(button, accessTextColor, accessButtonColor);

            return button;
        }

        public UIDynamicButton CreateButton(string name, float width = 100, float height = 80)
        {
            Transform button = GameObject.Instantiate<Transform>(plugin.manager.configurableButtonPrefab);
            ConfigureTransform(button, width, height);
            ParentToCanvas(button);

            UIDynamicButton uiButton = button.GetComponent<UIDynamicButton>();
            uiButton.label = name;
            uiButton.buttonText.fontSize = 18;
            return uiButton;
        }

        public static void ColorButton(UIDynamicButton button, Color textColor, Color buttonColor)
        {
            button.textColor = textColor;
            button.buttonColor = buttonColor;
        }

        private void ConfigureTransform(Transform t, float width, float height)
        {
            t.transform.position = Vector3.zero;
            RectTransform rt = t.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(width / 2, height / 2);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void ParentToCanvas(Transform t)
        {
            t.SetParent(canvas.transform, false);
        }

        public void LookAtCamera()
        {
            if (isDesktopMode)
            {
                canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
            }
            else
            {
                if (XRSettings.enabled == false)
                {
                    Transform cameraT = SuperController.singleton.lookCamera.transform;
                    Vector3 endPos = cameraT.position + cameraT.forward * 10000000.0f;
                    canvas.transform.LookAt(endPos, cameraT.up);
                }
                else
                {
                    canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
                }
            }
        }

        public void OnDestroy()
        {
            try
            {
                StopAutoPossessRoutine();

                if (SuperController.singleton != null)
                {
                    SuperController.singleton.RemoveCanvas(canvas);
                }

                if (canvas != null)
                {
                    canvas.transform.SetParent(null, false);

                    if (canvas.gameObject != null)
                    {
                        GameObject.Destroy(canvas.gameObject);
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }

        }
    }
}
