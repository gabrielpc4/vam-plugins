using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
namespace geesp0t
{
    public class PossessSex : MVRScript
    {
        const string pluginName = "PossessSex";
        const string pluginAuthor = "geesp0t";
        const string pluginVersion = "v1.0";
        const string apAtomName = "Possess Sex";

        const string LEFT_HAND = "LeftHand";
        const string RIGHT_HAND = "RightHand";

        const string LEFT_HAND_ANCHOR = "LeftHandAnchor";
        const string RIGHT_HAND_ANCHOR = "RightHandAnchor";

        //LEAP NOT YET WORKING
        const string LEFT_HAND_LEAP = "LeftHandAlternate";
        const string RIGHT_HAND_LEAP = "RightHandAlternate";

        const bool logMessages = false;

        protected JSONStorableStringChooser _targetPersonJSON;
        protected FreeControllerV3 _thisAtomFC;

        protected Rigidbody _femalePelvis;
        protected List<FreeControllerV3> _femaleStiffenList = new List<FreeControllerV3>();
        protected List<FreeControllerV3> _femaleDisableGrabList = new List<FreeControllerV3>();
        protected List<FreeControllerV3> _femaleEnablePossessList = new List<FreeControllerV3>();
        protected List<FreeControllerV3> _femaleLinkToPelvisList = new List<FreeControllerV3>();

        protected Rigidbody _penisBase;
        protected List<FreeControllerV3> _maleStiffenList = new List<FreeControllerV3>();
        protected List<FreeControllerV3> _maleDisableGrabList = new List<FreeControllerV3>();
        protected List<FreeControllerV3> _maleLinkOffList = new List<FreeControllerV3>();
        protected List<FreeControllerV3> _maleEnablePossessList = new List<FreeControllerV3>();
        protected List<FreeControllerV3> _maleLinkToPenisBaseList = new List<FreeControllerV3>();

        protected FreeControllerV3 _femaleLinkFrom;
        protected FreeControllerV3 _femaleHeadLinkFrom;
        protected FreeControllerV3 _maleLinkFrom;
        protected FreeControllerV3 _dildoLinkFrom;

        protected Vector3 _lastOrificeControlPosition;
        protected Quaternion _lastOrificeControlRotation;
        protected UIDynamicPopup _targetPersonPopup;
        protected bool _loaded = false;
        protected bool _atomNameTargetUpdate = false;

        public static Canvas canvas = null;
        MVRScript plugin;
        private float UIScale = 1.0f;
        protected UIDynamicButton linkPelvisButton;
        protected UIDynamicButton linkHeadButton;
        protected UIDynamicButton linkPenisButton;
        protected UIDynamicButton linkDildoButton;
        protected bool _pelvisLinked = false;
        protected bool _headLinked = false;
        protected bool _penisLinked = false;
        protected bool _dildoLinked = false;

        protected bool _hasMale = true;
        protected Rigidbody _pelvisLinkedTo = null;
        protected Rigidbody _headLinkedTo = null;
        protected Rigidbody _penisLinkedTo = null;
        protected Rigidbody _dildoLinkedTo = null;

        private bool isDesktopMode = false;

        public JSONStorableAction hideUI;
        public JSONStorableAction showUI;


        public override void Init()
        {
            try
            {
                hideUI = new JSONStorableAction("Hide UI", () => HideUI());
                RegisterAction(hideUI);
                showUI = new JSONStorableAction("Show UI", () => ShowUI());
                RegisterAction(showUI);

                if (containingAtom.category == "People")
                {
                    _loaded = false;
                    _thisAtomFC = containingAtom.freeControllers.First(freec => freec.name == "control");

                    _targetPersonJSON = new JSONStorableStringChooser("targetPerson", GetPersonListChoices(), GetPersonListChoices()[0], "TargetPersonChoice", SyncTargetPerson);
                    _targetPersonJSON.storeType = JSONStorableParam.StoreType.Physical;
                    RegisterStringChooser(_targetPersonJSON);
                    // make target person selector
                    _targetPersonPopup = CreateScrollablePopup(_targetPersonJSON);
                    _targetPersonPopup.popupPanelHeight = 400f;
                    _targetPersonPopup.popup.onOpenPopupHandlers = SyncTargetPersonChoices;
                    _targetPersonPopup.label = "Target Penis";

                    var btn = CreateButton("Control Female Pelvis", true);
                    btn.button.onClick.AddListener(() => { LinkToFemalePelvis(); });
                    btn.buttonColor = Color.green;

                    btn = CreateButton("Release Female Pelvis", true);
                    btn.button.onClick.AddListener(() => { ReleaseFemalePelvis(); });
                    btn.buttonColor = Color.white;

                    btn = CreateButton("Control Female Head", true);
                    btn.button.onClick.AddListener(() => { LinkToFemaleHead(); });
                    btn.buttonColor = Color.green;

                    btn = CreateButton("Release Female Head", true);
                    btn.button.onClick.AddListener(() => { ReleaseFemaleHead(); });
                    btn.buttonColor = Color.white;

                    btn = CreateButton("Control Penis", true);
                    btn.button.onClick.AddListener(() => { LinkToPenis(); });
                    btn.buttonColor = Color.green;

                    btn = CreateButton("Release Penis", true);
                    btn.button.onClick.AddListener(() => { ReleasePenis(); });
                    btn.buttonColor = Color.white;

                    btn = CreateButton("Control Dildo", true);
                    btn.button.onClick.AddListener(() => { LinkToDildo(); });
                    btn.buttonColor = Color.green;

                    btn = CreateButton("Release Dildo", true);
                    btn.button.onClick.AddListener(() => { ReleaseDildo(); });
                    btn.buttonColor = Color.white;

                    JSONStorableString pluginVersionJSON = new JSONStorableString("If you want to possess (link to) the female pelvis, align your controller or tracker to the pelvis, then press Control Female Pelvis.\nIf you want to possess (link to) the penis, align your controller or tracker to the penis, then press Control Penis.\n", "");
                    UIDynamicTextField dtext = CreateTextField(pluginVersionJSON, false);
                    pluginVersionJSON.val = pluginName + " " + pluginVersion + "\nby " + pluginAuthor;
                    dtext.height = 1;

                    SuperController.singleton.onAtomUIDRenameHandlers += new SuperController.OnAtomUIDRename(this.AtomNameUpdate);

                    isDesktopMode = !(SuperController.singleton.isOVR || SuperController.singleton.isOpenVR);

                }
                else { _loaded = false; SuperController.LogError("Possess Sex must be loaded on a female Person atom."); }
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        public void ShowUIButtons(bool doShow)
        {
            //I THINK FOR NOW IT'S BETTER TO ALWAYS HAVE THESE VISIBLE, IF WE ARE USING POSSESS SEX, WE PROBABLY WANT THESE AS OPTIONS
            //AND WE MAY WANT TO HIDE EVERYTHING ELSE BUT SHOW THESE STILL
            return; 

            //if (linkPelvisButton != null) linkPelvisButton.gameObject.SetActive(doShow);
            //if (linkHeadButton != null) linkHeadButton.gameObject.SetActive(doShow);
            //if (linkPenisButton != null) linkPenisButton.gameObject.SetActive(doShow);
            //if (linkDildoButton != null) linkDildoButton.gameObject.SetActive(doShow);
        }

        public void ShowUI()
        {
            ShowUIButtons(true);
        }
        public void HideUI()
        {
            ShowUIButtons(false);
        }

        public void Start()
        {
            try
            {
                Cleanup();
                float worldScale = SuperController.singleton.worldScale;
                SuperController.singleton.worldScale = 1.0f;
                CreateButtons();
                SuperController.singleton.worldScale = worldScale;

                InitFemaleControlNodes();
                SetupFemaleControlNodes();

                SyncTargetPersonChoices();
                if (!_targetPersonJSON.choices.Contains(_targetPersonJSON.val) && (_targetPersonJSON.val == "None" || _targetPersonJSON.val == "")) { _targetPersonJSON.val = _targetPersonJSON.choices[0]; }
                SyncTargetPerson(_targetPersonJSON.val);
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        protected string FindMale(string atomUID)
        {
            try
            {
                _hasMale = false;
                Atom testAtom = SuperController.singleton.GetAtomByUid(atomUID);
                if (testAtom != null && testAtom.type == "Person" && atomUID != containingAtom.name && testAtom.GetComponentInChildren<DAZCharacter>().name.StartsWith("male"))
                {
                    //we were passed in a good UID
                    _hasMale = true;
                    return atomUID;
                }
                else
                {
                    atomUID = SuperController.singleton.GetAtoms()
                        .Where(a => a.category == "People" &&
                                    a.GetComponentInChildren<DAZCharacter>().name.StartsWith("male"))
                        .Select(atom => atom.uid).First();

                    _hasMale = true;
                    return atomUID;
                }
            }
            catch (Exception e)
            {
                //SuperController.LogMessage("Male person not found, Possess Sex only set for female.");
                _hasMale = false;
                return "None";
            }
        }

        protected void SyncTargetPerson(string atomUID)
        {
            try
            {
                atomUID = FindMale(atomUID);
                if (atomUID != "None" && !_atomNameTargetUpdate)
                {
                    InitMaleControlNodes(atomUID);
                    SetupMaleControlNodes(atomUID);
                    _loaded = true;
                }
                else { _atomNameTargetUpdate = false; }
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        protected IEnumerator CreateAtom(string atomType, string atomId, Action<Atom> onAtomCreated)
        {
            yield return new WaitForSeconds(1.0f);
            Atom atom = SuperController.singleton.GetAtomByUid(atomId);
            if (atom == null)
            {
                yield return SuperController.singleton.AddAtomByType(atomType, atomId);
                atom = SuperController.singleton.GetAtomByUid(atomId);
            }
            onAtomCreated(atom);
        }

        protected void InitMaleControlNodes(string atomUID)
        {
            Atom targetPersonAtom = SuperController.singleton.GetAtomByUid(atomUID);

            _maleStiffenList.Clear();
            _maleStiffenList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "penisTipControl"));
            _maleStiffenList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "penisMidControl"));
            _maleStiffenList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "penisBaseControl"));

            _maleDisableGrabList.Clear();
            _maleDisableGrabList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "hipControl"));
            _maleDisableGrabList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "pelvisControl"));
            _maleDisableGrabList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "abdomenControl"));
            _maleDisableGrabList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "abdomen2Control"));
            _maleDisableGrabList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "lThighControl"));
            _maleDisableGrabList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "rThighControl"));
            _maleDisableGrabList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "penisTipControl"));
            _maleDisableGrabList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "penisMidControl"));
            _maleDisableGrabList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "testesControl"));

            _maleLinkOffList.Clear();
            _maleLinkOffList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "penisTipControl"));
            _maleLinkOffList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "penisMidControl"));
            _maleLinkOffList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "testesControl"));

            _maleEnablePossessList.Clear();
            _maleEnablePossessList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "penisBaseControl"));

            _penisBase = targetPersonAtom.rigidbodies.First(rb => rb.name == "penisBaseControl"); //why not penisBase, I don't know, doesn't work

            _maleLinkToPenisBaseList.Clear();
            _maleLinkToPenisBaseList.Add(targetPersonAtom.freeControllers.First(freec => freec.name == "hipControl"));

            _maleLinkFrom = targetPersonAtom.freeControllers.First(freec => freec.name == "penisBaseControl");
        }

        protected void SetupMaleControlNodes(string atomUID)
        {
            for (int i = 0; i < _maleDisableGrabList.Count; i++)
            {
                _maleDisableGrabList[i].interactableInPlayMode = false;
                _maleDisableGrabList[i].canGrabPosition = false;
                _maleDisableGrabList[i].canGrabRotation = false;
            }

            for (int i = 0; i < _maleEnablePossessList.Count; i++)
            {
                _maleEnablePossessList[i].canGrabPosition = true;
                _maleEnablePossessList[i].canGrabRotation = true;
                _maleEnablePossessList[i].possessable = true;
                _maleEnablePossessList[i].interactableInPlayMode = true;
            }
        }

        protected void SetupMaleControlNodesOnLink()
        {
            for (int i = 0; i < _maleEnablePossessList.Count; i++)
            {
                _maleEnablePossessList[i].currentPositionState = FreeControllerV3.PositionState.PhysicsLink;
                _maleEnablePossessList[i].currentRotationState = FreeControllerV3.RotationState.PhysicsLink;
            }

            for (int i = 0; i < _maleLinkOffList.Count; i++)
            {
                _maleLinkOffList[i].currentPositionState = FreeControllerV3.PositionState.Off;
                _maleLinkOffList[i].currentRotationState = FreeControllerV3.RotationState.Off;
            }

            for (int i = 0; i < _maleLinkToPenisBaseList.Count; i++)
            {
                _maleLinkToPenisBaseList[i].SelectLinkToRigidbody(_penisBase, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
                _maleLinkToPenisBaseList[i].currentPositionState = FreeControllerV3.PositionState.PhysicsLink;
                _maleLinkToPenisBaseList[i].currentRotationState = FreeControllerV3.RotationState.PhysicsLink;
            }

            for (int i = 0; i < _maleStiffenList.Count; i++)
            {
                _maleStiffenList[i].RBHoldPositionSpring = 10000;
                _maleStiffenList[i].RBHoldRotationSpring = 1000;
                _maleStiffenList[i].RBHoldPositionDamper = 100;
                _maleStiffenList[i].RBHoldRotationDamper = 100;
                _maleStiffenList[i].RBHoldPositionMaxForce = 10000;
                _maleStiffenList[i].RBHoldRotationMaxForce = 1000;
                _maleStiffenList[i].jointRotationDriveSpring = 200;
                _maleStiffenList[i].jointRotationDriveDamper = 10;
                _maleStiffenList[i].jointRotationDriveMaxForce = 100;
            }
        }

        protected void InitFemaleControlNodes()
        {
            _femaleStiffenList.Clear();
            _femaleStiffenList.Add(containingAtom.freeControllers.First(freec => freec.name == "pelvisControl"));

            _femaleDisableGrabList.Clear();
            _femaleDisableGrabList.Add(containingAtom.freeControllers.First(freec => freec.name == "hipControl"));
            _femaleDisableGrabList.Add(containingAtom.freeControllers.First(freec => freec.name == "abdomenControl"));
            _femaleDisableGrabList.Add(containingAtom.freeControllers.First(freec => freec.name == "abdomen2Control"));
            _femaleDisableGrabList.Add(containingAtom.freeControllers.First(freec => freec.name == "lThighControl"));
            _femaleDisableGrabList.Add(containingAtom.freeControllers.First(freec => freec.name == "rThighControl"));

            _femaleEnablePossessList.Clear();
            _femaleEnablePossessList.Add(containingAtom.freeControllers.First(freec => freec.name == "pelvisControl"));
            _femaleEnablePossessList.Add(containingAtom.freeControllers.First(freec => freec.name == "headControl"));

            _femalePelvis = containingAtom.rigidbodies.First(rb => rb.name == "pelvisControl");

            _femaleLinkToPelvisList.Clear();
            _femaleLinkToPelvisList.Add(containingAtom.freeControllers.First(freec => freec.name == "hipControl"));

            _femaleLinkFrom = containingAtom.freeControllers.First(freec => freec.name == "pelvisControl");
            _femaleHeadLinkFrom = containingAtom.freeControllers.First(freec => freec.name == "headControl");
        }

        protected void SetupFemaleControlNodes()
        {
            for (int i = 0; i < _femaleDisableGrabList.Count; i++)
            {
                _femaleDisableGrabList[i].interactableInPlayMode = false;
                _femaleDisableGrabList[i].canGrabPosition = false;
                _femaleDisableGrabList[i].canGrabRotation = false;
            }

            for (int i = 0; i < _femaleEnablePossessList.Count; i++)
            {
                _femaleEnablePossessList[i].canGrabPosition = true;
                _femaleEnablePossessList[i].canGrabRotation = true;
                _femaleEnablePossessList[i].possessable = true;
                _femaleEnablePossessList[i].interactableInPlayMode = true;
            }
        }

        protected void SetupFemaleControlNodesOnLink()
        {
            for (int i = 0; i < _femaleEnablePossessList.Count; i++)
            {
                _femaleEnablePossessList[i].currentPositionState = FreeControllerV3.PositionState.PhysicsLink;
                _femaleEnablePossessList[i].currentRotationState = FreeControllerV3.RotationState.PhysicsLink;
            }

            for (int i = 0; i < _femaleLinkToPelvisList.Count; i++)
            {
                _femaleLinkToPelvisList[i].SelectLinkToRigidbody(_femalePelvis, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
                _femaleLinkToPelvisList[i].currentPositionState = FreeControllerV3.PositionState.PhysicsLink;
                _femaleLinkToPelvisList[i].currentRotationState = FreeControllerV3.RotationState.PhysicsLink;
            }

            for (int i = 0; i < _femaleStiffenList.Count; i++)
            {
                _femaleStiffenList[i].RBHoldPositionSpring = 10000;
                _femaleStiffenList[i].RBHoldRotationSpring = 1000;
                _femaleStiffenList[i].RBHoldPositionDamper = 100;
                _femaleStiffenList[i].RBHoldRotationDamper = 100;
                _femaleStiffenList[i].RBHoldPositionMaxForce = 10000;
                _femaleStiffenList[i].RBHoldRotationMaxForce = 1000;
                _femaleStiffenList[i].jointRotationDriveSpring = 200;
                _femaleStiffenList[i].jointRotationDriveDamper = 10;
                _femaleStiffenList[i].jointRotationDriveMaxForce = 100;
            }
        }

        protected void ToggleFemalePelvisLink()
        {
            if (_pelvisLinked)
            {
                ReleaseFemalePelvis();
            }
            else
            {
                LinkToFemalePelvis();
            }
        }

        protected void ToggleFemaleHeadLink()
        {
            if (_headLinked)
            {
                ReleaseFemaleHead();
            }
            else
            {
                LinkToFemaleHead();
            }
        }

        protected void TogglePenisLink()
        {
            if (_penisLinked)
            {
                ReleasePenis();
            }
            else
            {
                LinkToPenis();
            }
        }

        protected void ToggleDildoLink()
        {
            if (_dildoLinked)
            {
                ReleaseDildo();
            }
            else
            {
                LinkToDildo();
            }
        }

        protected FreeControllerV3 FindFreeControllerNamed(string name)
        {
             foreach (string testAtomUID in SuperController.singleton.GetAtomUIDs())
             {
                 Atom testAtom = SuperController.singleton.GetAtomByUid(testAtomUID);
                 if (testAtom.name == name)
                 {
                    FreeControllerV3 fc = testAtom.gameObject.GetComponentInChildren<FreeControllerV3>();
                    if (fc != null) return fc;
                 }
             }
            return null;
        }

        protected Rigidbody FindTrackedRigidbodyCloseTo(Vector3 pos)
        {
            Rigidbody closestRigidbody = null;
            float rigidbodyDistance = 0;

            GameObject cameraRig = GameObject.Find("[CameraRig]");

            if (SuperController.singleton.isOVR)
            {
                foreach (Rigidbody rb in cameraRig.GetComponentsInChildren<Rigidbody>())
                {
                    if (rb.name == LEFT_HAND && OVRInput.IsControllerConnected(OVRInput.Controller.LTouch))
                    {
                        float dist = Vector3.Distance(rb.position, pos);
                        if (closestRigidbody == null || dist < rigidbodyDistance)
                        {
                            if (rb != _pelvisLinkedTo && rb != _headLinkedTo && rb != _penisLinkedTo && rb!= _dildoLinkedTo)
                            {
                                closestRigidbody = rb;
                                rigidbodyDistance = dist;
                            }
                        }
                    }
                    else if (rb.name == RIGHT_HAND && OVRInput.IsControllerConnected(OVRInput.Controller.RTouch))
                    {
                        float dist = Vector3.Distance(rb.position, pos);
                        if (closestRigidbody == null || dist < rigidbodyDistance)
                        {
                            if (rb != _pelvisLinkedTo && rb != _headLinkedTo && rb != _penisLinkedTo && rb!= _dildoLinkedTo)
                            {
                                closestRigidbody = rb;
                                rigidbodyDistance = dist;
                            }
                        }
                    }
                    else if (rb.name == LEFT_HAND_LEAP || rb.name == RIGHT_HAND_LEAP || rb.name == RIGHT_HAND_ANCHOR || rb.name == LEFT_HAND_ANCHOR)
                    {
                        float dist = Vector3.Distance(rb.position, pos);
                        if (closestRigidbody == null || dist < rigidbodyDistance)
                        {
                            if (rb != _pelvisLinkedTo && rb != _headLinkedTo && rb != _penisLinkedTo && rb != _dildoLinkedTo)
                            {
                                Log("." + rb.name + ".");
                                closestRigidbody = rb;
                                rigidbodyDistance = dist;
                            }
                        }
                    }
                }
            }
            else
            {
                List<XRNodeState> nodes = new List<XRNodeState>();
                InputTracking.GetNodeStates(nodes);
                Vector3 nodePosition = Vector3.zero;

                bool leftHandActive = false;
                bool rightHandActive = false;

                foreach (XRNodeState ns in nodes)
                {
                    if (ns.TryGetPosition(out nodePosition))
                    {
                       /* float dist = Vector3.Distance(nodePosition, _femaleLinkFrom.gameObject.transform.position);
                         SuperController.LogError(ns.ToString() + " type:" + ns.nodeType + " tracked:" + ns.tracked + " id:" + ns.uniqueID + " pos:" + nodePosition + " dist:" + dist);*/
                        if (ns.nodeType == XRNode.LeftHand && ns.tracked) leftHandActive = true;
                        if (ns.nodeType == XRNode.RightHand && ns.tracked) rightHandActive = true;
                    }
                }

                foreach (Rigidbody rb in cameraRig.GetComponentsInChildren<Rigidbody>())
                {
                    if (rb.name == LEFT_HAND && leftHandActive)
                    {
                        float dist = Vector3.Distance(rb.position, pos);
                        if (closestRigidbody == null || dist < rigidbodyDistance)
                        {
                            if (rb != _pelvisLinkedTo && rb != _headLinkedTo && rb != _penisLinkedTo && rb!= _dildoLinkedTo)
                            {
                                closestRigidbody = rb;
                                rigidbodyDistance = dist;
                            }
                        }
                    }
                    else if (rb.name == RIGHT_HAND && rightHandActive)
                    {
                        float dist = Vector3.Distance(rb.position, pos);
                        if (closestRigidbody == null || dist < rigidbodyDistance)
                        {
                            if (rb != _pelvisLinkedTo && rb != _headLinkedTo && rb != _penisLinkedTo && rb!= _dildoLinkedTo)
                            {
                                closestRigidbody = rb;
                                rigidbodyDistance = dist;
                            }
                        }
                    } else if (rb.name.ToString().StartsWith("Tracker"))
                    {
                        float dist = Vector3.Distance(rb.position, pos);
                        if (closestRigidbody == null || dist < rigidbodyDistance)
                        {
                            if (rb != _pelvisLinkedTo && rb != _headLinkedTo && rb != _penisLinkedTo && rb!= _dildoLinkedTo)
                            {
                                closestRigidbody = rb;
                                rigidbodyDistance = dist;
                            }
                        }
                    }
                    else if (rb.name == LEFT_HAND_LEAP || rb.name == RIGHT_HAND_LEAP || rb.name == RIGHT_HAND_ANCHOR || rb.name == LEFT_HAND_ANCHOR)
                    {
                        float dist = Vector3.Distance(rb.position, pos);
                        if (closestRigidbody == null || dist < rigidbodyDistance)
                        {
                            if (rb != _pelvisLinkedTo && rb != _headLinkedTo && rb != _penisLinkedTo && rb != _dildoLinkedTo)
                            {
                                closestRigidbody = rb;
                                rigidbodyDistance = dist;
                            }
                        }
                    }
                }
            }

            return closestRigidbody;
        }

        protected void LinkToFemalePelvis()
        {
            SetupFemaleControlNodesOnLink();

            _pelvisLinkedTo = null;

            Rigidbody closestRigidbody = FindTrackedRigidbodyCloseTo(_femaleLinkFrom.transform.position);

            if (closestRigidbody != null)
            {
                _pelvisLinked = true;
                _pelvisLinkedTo = closestRigidbody;
                _femaleLinkFrom.SelectLinkToRigidbody(closestRigidbody, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
                _femaleLinkFrom.currentPositionState = FreeControllerV3.PositionState.PhysicsLink;
                _femaleLinkFrom.currentRotationState = FreeControllerV3.RotationState.PhysicsLink;
            }
            else
            {
                _pelvisLinked = false;
            }


            UpdateButtonNames();
        }
        protected void ReleaseFemalePelvis()
        {
            _pelvisLinkedTo = null;

            _femaleLinkFrom.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
            _pelvisLinked = false;

            UpdateButtonNames();
        }
        protected void LinkToFemaleHead()
        {
            _headLinkedTo = null;

            Rigidbody closestRigidbody = FindTrackedRigidbodyCloseTo(_femaleHeadLinkFrom.transform.position);

            if (closestRigidbody != null)
            {
                _headLinked = true;
                _headLinkedTo = closestRigidbody;
                _femaleHeadLinkFrom.SelectLinkToRigidbody(closestRigidbody, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
                _femaleHeadLinkFrom.currentPositionState = FreeControllerV3.PositionState.PhysicsLink;
                _femaleHeadLinkFrom.currentRotationState = FreeControllerV3.RotationState.PhysicsLink;
            }
            else
            {
                _headLinked = false;
            }


            UpdateButtonNames();
        }
        protected void ReleaseFemaleHead()
        {
            _headLinkedTo = null;

            _femaleHeadLinkFrom.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
            _headLinked = false;

            UpdateButtonNames();
        }
        protected void LinkToPenis()
        {
            SetupMaleControlNodesOnLink();

            _penisLinkedTo = null;

            Rigidbody closestRigidbody = FindTrackedRigidbodyCloseTo(_maleLinkFrom.transform.position);

            if (closestRigidbody != null)
            {
                _penisLinked = true;
                _penisLinkedTo = closestRigidbody;
                _maleLinkFrom.SelectLinkToRigidbody(closestRigidbody, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
                _maleLinkFrom.currentPositionState = FreeControllerV3.PositionState.PhysicsLink;
                _maleLinkFrom.currentRotationState = FreeControllerV3.RotationState.PhysicsLink;
            }
            else
            {
                _penisLinked = false;
            }

            UpdateButtonNames();
        }
        protected void ReleasePenis()
        {
            _penisLinkedTo = null;

            _maleLinkFrom.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
            _penisLinked = false;

            UpdateButtonNames();
        }
        protected void LinkToDildo()
        {

            _dildoLinkFrom = FindFreeControllerNamed("Dildo");

            _dildoLinkedTo = null;

            Rigidbody closestRigidbody = FindTrackedRigidbodyCloseTo(_dildoLinkFrom.transform.position);

            if (closestRigidbody != null)
            {
                _dildoLinked = true;
                _dildoLinkedTo = closestRigidbody;
                _dildoLinkFrom.SelectLinkToRigidbody(closestRigidbody, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
                _dildoLinkFrom.currentPositionState = FreeControllerV3.PositionState.PhysicsLink;
                _dildoLinkFrom.currentRotationState = FreeControllerV3.RotationState.PhysicsLink;
            }
            else
            {
                _dildoLinked = false;
            }

            UpdateButtonNames();
        }
        protected void ReleaseDildo()
        {
            _dildoLinkedTo = null;

            _dildoLinkFrom.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
            _dildoLinked = false;

            UpdateButtonNames();
        }

        protected void SyncTargetPersonChoices() { _targetPersonJSON.choices = GetPersonListChoices(); }
        protected List<string> GetPersonListChoices()
        {
            List<string> atomChoices = new List<string>();
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
                Atom atom = SuperController.singleton.GetAtomByUid(atomUID);
                if (atom.type == "Person" && atomUID != containingAtom.name && atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("male")) { atomChoices.Add(atomUID); }
            }
            if (atomChoices.Count == 0) { atomChoices.Add("None"); }
            return atomChoices;
        }

        protected void EnableCollision(Atom person, bool enable)
        {
            JSONStorable atomControlReceiver = null;
            atomControlReceiver = person.GetStorableByID("AtomControl");
            JSONStorableBool actionBoolJSON = atomControlReceiver.GetBoolJSONParam("collisionEnabled");
            if (enable) { actionBoolJSON.val = true; }
            else { actionBoolJSON.val = false; }
        }

        protected bool GetCollision(Atom person)
        {
            JSONStorable atomControlReceiver = null;
            atomControlReceiver = person.GetStorableByID("AtomControl");
            JSONStorableBool actionBoolJSON = atomControlReceiver.GetBoolJSONParam("collisionEnabled");
            return actionBoolJSON.val;
        }


        protected void AtomNameUpdate(string oldName, string newName)
        {
            try
            {
                _atomNameTargetUpdate = true;
                if (oldName == _targetPersonJSON.val) { _targetPersonJSON.val = newName; }
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        //Based on Dollmaster UI code
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
            /*ui = new UI(this, 0.001f);
            ui.canvas.transform.Translate(0, 0.2f, 0);*/
            plugin = this;
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
            canvas.transform.localPosition = new Vector3(0.5f, -0.66f, 0.35f);

            LookAtCamera();
            
            if (!isDesktopMode)
            {
                linkPelvisButton = AddButton("Link Female Pelvis", 0, 3, () =>
                {
                    ToggleFemalePelvisLink();
                });
                linkPenisButton = AddButton("Link Penis", 0, 4, () =>
                {
                    TogglePenisLink();
                });
                linkHeadButton = AddButton("Link Female Head", -1, 3, () =>
                {
                    ToggleFemaleHeadLink();
                });
                linkDildoButton = AddButton("Link Dildo", -1, 4, () =>
                {
                    ToggleDildoLink();
                });
            }

            canvas.transform.Translate(0, 0.2f, 0);
        }

        public void UpdateButtonNames()
        {
            if (_pelvisLinked)
            {
                linkPelvisButton.buttonText.text = "Unlink Female Pelvis";
            }
            else
            {
                linkPelvisButton.buttonText.text = "Link Female Pelvis";
            }

            if (_headLinked)
            {
                linkHeadButton.buttonText.text = "Unlink Female Head";
            }
            else
            {
                linkHeadButton.buttonText.text = "Link Female Head";
            }

            if (_penisLinked)
            {
                linkPenisButton.buttonText.text = "Unlink Penis";
            }
            else
            {
                linkPenisButton.buttonText.text = "Link Penis";
            }

            if (_dildoLinked)
            {
                linkDildoButton.buttonText.text = "Unlink Dildo";
            }
            else
            {
                linkDildoButton.buttonText.text = "Link Dildo";
            }
        }

        public UIDynamicButton AddButton(string name, int button_column, int button_row, UnityAction callback)
        {
            Color accessButtonColor = new Color(0.8392f, 0.8392f, 0.8392f);
            Color accessTextColor = new Color(0, 0, 0);
            float xSpacing = 0.22f;
            float ySpacing = 0.05f;

            UIDynamicButton button = CreateButton(name, 100, 40);
            button.button.onClick.AddListener(callback);
            button.transform.Translate(button_column * xSpacing, 0.45f - button_row * ySpacing, 0, Space.Self);
            ColorButton(button, accessTextColor, accessButtonColor);
            
            return button;
        }

        public UIDynamicButton CreateButton(string name, float width = 100, float height = 80)
        {
            Transform button = GameObject.Instantiate<Transform>(this.manager.configurableButtonPrefab);
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

        void OnDestroy()
        {
            try
            {
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

        void Log(string message)
        {
            if (logMessages) SuperController.LogMessage(message);
        }
    }
}