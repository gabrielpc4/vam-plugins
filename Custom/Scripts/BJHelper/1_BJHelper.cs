// ToDo:
// Offset just by headToLipDistance once and hope for the best
// Random head movement a little bit
// Slowly change physics values during start and reset so there's no sudden jerkiness
// Check if head or penis position is outside of expected range - ie if being moved by player - and if so reset movement
// Use forces instead?? 
// Work out lip location compared to headControl
// Bugs:
// Initial head rotation is instant
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
using GPUTools.Hair.Scripts.Settings;

namespace BJHelper
{
    public class BJHelper : MVRScript
    {
        private Atom her;
        private FreeControllerV3 headControl;
        private Rigidbody lipTrigger;
        Dictionary<DAZMorph, float> LipMorphs = new Dictionary<DAZMorph, float>();

        private Atom him;
        private FreeControllerV3 penisTip;
        private FreeControllerV3 penisBase;
        private FreeControllerV3 penisMid;
        private Rigidbody suckTarget;

        private JSONStorableFloat headOffsetXJSON;
        private JSONStorableFloat headOffsetYJSON;

        private JSONStorableFloat positionOne;
        private JSONStorableFloat positionTwo;

        private JSONStorableFloat headSpeedJSON;
        private JSONStorableFloat headMoveToPenisSpeedJSON;

        private JSONStorableFloat headMoveRandomizerJSON;
        private JSONStorableFloat headSpeedRandomizerJSON;

        private JSONStorableBool enableLipMorphs;

        private string _scriptConnectionAtomName = "VAMLaunchScriptConnection";
        private Atom _scriptConnectionAtom = null; //connect to VAMLaunch
        private Transform _scriptConnectionTransform = null;
        private float vamLaunchDirection = 0;

        public override void Init()
        {
            try
            {

                // UI Setup
                var btn = CreateButton("Start");
                btn.button.onClick.AddListener(() => { StartBJMove(); });
                btn = CreateButton("Stop");
                btn.button.onClick.AddListener(() => { StopBJ(); });

                headSpeedJSON = new JSONStorableFloat("Speed", 0.25f, 0f, 1f, false);
                RegisterFloat(headSpeedJSON);
                CreateSlider(headSpeedJSON);


                enableLipMorphs = new JSONStorableBool("Enable Lip Morphs", true);
                enableLipMorphs.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(enableLipMorphs);
                var toggle = CreateToggle(enableLipMorphs);
                toggle.toggle.onValueChanged.AddListener(delegate { ToggleLipMorphs(); });

                positionOne = new JSONStorableFloat("Position One", 0.02f, -1f, 1f, false);
                RegisterFloat(positionOne);
                CreateSlider(positionOne, true);

                positionTwo = new JSONStorableFloat("Position Two", 0.10f, -1f, 1f, false);
                RegisterFloat(positionTwo);
                CreateSlider(positionTwo, true);

                headOffsetYJSON = new JSONStorableFloat("Head Angle Down/Up", 0.02f, -1f, 1f, false);
                RegisterFloat(headOffsetYJSON);
                CreateSlider(headOffsetYJSON, true);

                headOffsetXJSON = new JSONStorableFloat("Head Angle Left/Right", 0f, -5f, 5f, false);
                RegisterFloat(headOffsetXJSON);
                CreateSlider(headOffsetXJSON, true);

                headMoveRandomizerJSON = new JSONStorableFloat("Head Position Randomizer", 20f, 0f, 50f);
                RegisterFloat(headMoveRandomizerJSON);
                CreateSlider(headMoveRandomizerJSON, true);

                headSpeedRandomizerJSON = new JSONStorableFloat("Head Speed Randomizer", 0.5f, 0f, 1f);
                RegisterFloat(headSpeedRandomizerJSON);
                CreateSlider(headSpeedRandomizerJSON, true);
                
                headMoveToPenisSpeedJSON = new JSONStorableFloat("Initial Move Speed", 0.35f, 0f, 1f, false);
                RegisterFloat(headMoveToPenisSpeedJSON);
                CreateSlider(headMoveToPenisSpeedJSON, true);

                btn = CreateButton("Preset: Default");
                btn.button.onClick.AddListener(() => { PresetOne(); });
                btn = CreateButton("Preset: Light");
                btn.button.onClick.AddListener(() => { PresetTwo(); });
                btn = CreateButton("Preset: Deep");
                btn.button.onClick.AddListener(() => { PresetThree(); });

                btn = CreateButton("Debug: Skip Start Move");
                btn.button.onClick.AddListener(() => { StartSucking(); });

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }


        private Vector3 headInitialPosition;
        private Quaternion headInitialRotation;
        private Quaternion penisInitialRotation;
        private Vector3 headMoveTargetPosition;
        private Vector3 headTargetPosition;
        private Rigidbody headTarget;
        private Quaternion headTargetRotation;
        private Quaternion headResetRotation;
        private Vector3 headOffset;
        private bool isMovingToPenis = false;
        private bool isSucking = false;
        private bool isResetting = false;
        private bool flip = false;
        private float journeyDistance;
        private float journeyTime;
        private float morphValue = 0f;
        private float morphChangeDistance;
        private float morphChangeSpeed;
        private float distCovered;
        private float fracJourney;
        private float position;
        private float randomRange;
        private Quaternion headMoveRandomizer;
        private float headSpeedRandomizer = 0;

        private bool ready = false;

        // Start is called once before Update or FixedUpdate is called and after Init()
        void Start()
        {
            try
            {
                
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // Update is called with each rendered frame by Unity
        void Update()
        {
            try
            {
                if (!ready && !SuperController.singleton.isLoading)
                {                    
                    // Find first penis owner 
                    IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

                    Atom him = null;
                    foreach (Atom at in personAtoms)
                    {
                        if (at.GetComponentInChildren<DAZCharacter>().isMale) {
                            him = at;
                            break;
                        }
                    }

                    //still waiting for loading I guess
                    if (him == null) return;

                    her = containingAtom;
                    headControl = her.freeControllers.First(fc => fc.name == "headControl");
                    lipTrigger = her.rigidbodies.First(rb => rb.name == "LipTrigger");
                    flip = true;
                    headInitialPosition = headControl.transform.position;
                    headInitialRotation = headControl.transform.rotation;
                    
                    // Set head physics
                    // Todo: Set slowly during head move to penis phase to avoid jerkiness
                    headControl.RBHoldPositionSpring = 6000;
                    headControl.RBHoldRotationMaxForce = 1000;
                    headControl.RBHoldRotationSpring = 1000;
                    
                    // Set target mouth morphs
                    JSONStorable geometry = her.GetStorableByID("geometry");
                    DAZCharacterSelector character = geometry as DAZCharacterSelector;
                    GenerateDAZMorphsControlUI morphControl = character.morphsControlUI;
                    LipMorphs.Add(morphControl.GetMorphByDisplayName("Lips Close"), 0.8f);
                    LipMorphs.Add(morphControl.GetMorphByDisplayName("Lips Pucker"), 1f);
                    LipMorphs.Add(morphControl.GetMorphByDisplayName("Lips Pucker Wide"), 0.6f);
                    LipMorphs.Add(morphControl.GetMorphByDisplayName("Mouth Open"), 2f);
                    
                    penisTip = him.freeControllers.First(fc => fc.name == "penisTipControl");
                    penisBase = him.freeControllers.First(fc => fc.name == "penisBaseControl");
                    penisMid = him.freeControllers.First(fc => fc.name == "penisMidControl");
                    suckTarget = him.rigidbodies.First(rb => rb.name == "Gen3");

                    ready = true;
                    
                    _scriptConnectionAtom = SuperController.singleton.GetAtomByUid(_scriptConnectionAtomName);
                    if (_scriptConnectionAtom == null)
                    {
                        StartCoroutine(CreateScriptConnectionAtom());
                    }
                }

                if (_scriptConnectionAtom != null)
                {
                    if (_scriptConnectionTransform == null)
                    {
                        _scriptConnectionTransform = _scriptConnectionAtom.transform;
                    }
                    else
                    {
                        if (headSpeedJSON != null)
                        {
                            _scriptConnectionTransform.position = new Vector3(vamLaunchDirection, headSpeedJSON.val, _scriptConnectionTransform.position.z);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private IEnumerator CreateScriptConnectionAtom()
        {
            yield return SuperController.singleton.AddAtomByType("Empty", _scriptConnectionAtomName);
            _scriptConnectionAtom = SuperController.singleton.GetAtomByUid(_scriptConnectionAtomName);
        }

        private float startTime;

        // FixedUpdate is called with each physics simulation frame by Unity
        void FixedUpdate()
        {
            try
            {
                if (!ready) return;

                if (isMovingToPenis)
                {
                    PenisLookAtHead();
                    // Move head to start position
                    float moveStep = headMoveToPenisSpeedJSON.val * Time.deltaTime;
                    headControl.transform.position = Vector3.MoveTowards(headControl.transform.position, headMoveTargetPosition, moveStep);

                    // Slowly add lips morphs
                    if (enableLipMorphs.val)
                    {
                        foreach (KeyValuePair<DAZMorph, float> morphPair in LipMorphs)
                        {
                            morphChangeDistance = morphPair.Value;
                            morphChangeSpeed = morphChangeDistance / journeyTime * 1f;
                            distCovered = (Time.time - startTime) * morphChangeSpeed;
                            fracJourney = distCovered / morphChangeDistance;
                            morphPair.Key.morphValue = Mathf.Lerp(0f, morphPair.Value, fracJourney);
                        }
                    }
                    //  Slowly rotate head;
                    headTargetRotation = Quaternion.LookRotation((penisBase.transform.position + headOffset) - penisMid.transform.position);
                    float rotateSpeed = (journeyDistance / journeyTime) * 1.5f;
                    distCovered = (Time.time - startTime) * rotateSpeed;
                    fracJourney = distCovered / journeyDistance;
                    headControl.transform.rotation = Quaternion.Lerp(headInitialRotation, headTargetRotation, fracJourney);

                    if (Vector3.Distance(headControl.transform.position, headMoveTargetPosition) < 0.1f)
                    {
                        vamLaunchDirection = 1.0f;
                    }

                    // If finished, start sucking
                    if (Vector3.Distance(headControl.transform.position, headMoveTargetPosition) < 0.001f)
                    {
                        StartSucking();
                    }
                }
                if (isSucking)
                {

                    // move head
                    headTargetPosition = (suckTarget.transform.position + (headMoveRandomizer * penisTip.transform.forward * position));
                    float suckStep = (headSpeedJSON.val + headSpeedRandomizer) * Time.deltaTime;
                    headControl.transform.position = Vector3.MoveTowards(headControl.transform.position, headTargetPosition, suckStep);

                    // Set new tagets if reached bottom / top or if penis has moved
                    if (Vector3.Distance(headControl.transform.position, headTargetPosition) < 0.01f)
                    {
                        if (position == positionOne.val)
                        {
                            vamLaunchDirection = -1.0f;
                            position = positionTwo.val;
                        }
                        else
                        {
                            vamLaunchDirection = 1.0f;
                            position = positionOne.val;
                            // Randomize head movement a bit (only in one direction to keep things reasonable)
                            randomRange = headMoveRandomizerJSON.val;
                            headMoveRandomizer = Quaternion.Euler(UnityEngine.Random.Range(-randomRange, randomRange), UnityEngine.Random.Range(-randomRange, randomRange), 0);
                            // Randomize head speed
                            randomRange = headSpeedRandomizerJSON.val / 10;
                            headSpeedRandomizer = UnityEngine.Random.Range(-randomRange, randomRange);
                        }

                    }
                    // Update rotations
                    HeadLookAtPenis();
                    PenisLookAtHead();
                }

                if (isResetting)
                {
                    vamLaunchDirection = -1.0f;
                    // Slowly reset head position
                    float moveStep = headMoveToPenisSpeedJSON.val * Time.deltaTime;
                    headControl.transform.position = Vector3.MoveTowards(headControl.transform.position, headInitialPosition, moveStep);
                    // Slowly reset lips morphs
                    if (enableLipMorphs.val)
                    {
                        foreach (KeyValuePair<DAZMorph, float> morphPair in LipMorphs)
                        {
                            morphChangeDistance = morphPair.Value;
                            morphChangeSpeed = morphChangeDistance / journeyTime;
                            distCovered = (Time.time - startTime) * morphChangeSpeed;
                            fracJourney = distCovered / morphChangeDistance;
                            morphPair.Key.morphValue = Mathf.Lerp(morphPair.Value, 0f, fracJourney);
                        }
                    }
                    //  Slowly reset head rotation;
                    float rotateSpeed = (journeyDistance / journeyTime);
                    distCovered = (Time.time - startTime) * rotateSpeed;
                    fracJourney = distCovered / journeyDistance;
                    headControl.transform.rotation = Quaternion.Slerp(headResetRotation, headInitialRotation, fracJourney);

                    // Check if head is back to start point
                    if (Vector3.Distance(headControl.transform.position, headInitialPosition) < 0.001f)
                    {
                        isResetting = false;
                    }

                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void StartBJMove()
        {
            penisInitialRotation = penisBase.transform.rotation;
            headInitialPosition = headControl.transform.position;
            headInitialRotation = headControl.transform.rotation;
            PenisLookAtHead();

            // Prepare for head move to penis
           // Head target is just penisBase location plus a bit forward and a bit up. Very simplistic but nothing else seems to work.
            // We set it once here rather than constantly updating because if the penis moves when the head hits it then shit gets whacky
            headMoveTargetPosition = penisBase.transform.position + penisBase.transform.up * 0.05f + penisBase.transform.forward * 0.1f;
            journeyDistance = Vector3.Distance(headMoveTargetPosition, lipTrigger.transform.position);
            journeyTime = journeyDistance / headMoveToPenisSpeedJSON.val;
            startTime = Time.time;
            headTargetRotation = Quaternion.LookRotation((penisBase.transform.position) - penisMid.transform.position);
            isMovingToPenis = true;

            // Set All Penis Physics to hard (might not all be necessary but seems to help)
            penisBase.currentPositionState = FreeControllerV3.PositionState.On;
            penisBase.currentRotationState = FreeControllerV3.RotationState.On;
            penisBase.RBHoldPositionSpring = 10000;
            penisBase.RBHoldRotationSpring = 1000;
            penisBase.RBHoldPositionDamper = 100;
            penisBase.RBHoldRotationDamper = 100;
            penisBase.RBHoldPositionMaxForce = 10000;
            penisBase.RBHoldRotationMaxForce = 1000;
            penisBase.jointRotationDriveSpring = 200;
            penisBase.jointRotationDriveDamper = 10;
            penisBase.jointRotationDriveMaxForce = 100;
            penisMid.RBHoldPositionSpring = 10000;
            penisMid.RBHoldRotationSpring = 1000;
            penisMid.RBHoldPositionDamper = 100;
            penisMid.RBHoldRotationDamper = 100;
            penisMid.RBHoldPositionMaxForce = 10000;
            penisMid.RBHoldRotationMaxForce = 1000;
            penisMid.jointRotationDriveSpring = 200;
            penisMid.jointRotationDriveDamper = 10;
            penisMid.jointRotationDriveMaxForce = 100;
            penisTip.RBHoldPositionSpring = 10000;
            penisTip.RBHoldRotationSpring = 1000;
            penisTip.RBHoldPositionDamper = 100;
            penisTip.RBHoldRotationDamper = 100;
            penisTip.RBHoldPositionMaxForce = 10000;
            penisTip.RBHoldRotationMaxForce = 1000;
            penisTip.jointRotationDriveSpring = 200;
            penisTip.jointRotationDriveDamper = 10;
            penisTip.jointRotationDriveMaxForce = 100;
        }

        private void StartSucking()
        {
            position = positionTwo.val;
            isMovingToPenis = false;
            isSucking = true;
            headSpeedRandomizer = 0f;
        }

        private void StopBJ()
        {
            startTime = Time.time;
            headResetRotation = headControl.transform.rotation;
            isSucking = false;
            isMovingToPenis = false;
            isResetting = true;
            //  Reset penis rotation (Todo: do slowly)
            penisBase.transform.rotation = penisInitialRotation;

        }

        private void HeadLookAtPenis()
        {
            headOffset.x = headOffsetXJSON.val;
            headOffset.y = headOffsetYJSON.val;
            headOffset.z = 0;
            headControl.transform.rotation = Quaternion.LookRotation((penisBase.transform.position - penisMid.transform.position) + headOffset); 
        }

        private void PenisLookAtHead()
        {
            penisBase.transform.LookAt(lipTrigger.transform.position);
        }

        private void ToggleLipMorphs()
        {
            if (enableLipMorphs.val)
            {
                LipMorphsEnable();
            }
            else
            {
                LipMorphsReset();
            }
        }

        private void LipMorphsEnable()
        {
            if (enableLipMorphs.val && isSucking)
            {
                foreach (KeyValuePair<DAZMorph, float> morphPair in LipMorphs)
                {
                    morphPair.Key.morphValue = morphPair.Value;
                }
            }
        }

        private void LipMorphsReset()
        {
            foreach (KeyValuePair<DAZMorph, float> morphPair in LipMorphs)
            {
                morphPair.Key.morphValue = 0f;
            }
        }

        private void PresetOne()
        {
            headSpeedJSON.val = 0.25f;
            positionOne.val = 0.02f;
            positionTwo.val = 0.1f;
            headOffsetYJSON.val = 0.02f;
            headMoveRandomizerJSON.val = 20f;
            headSpeedRandomizerJSON.val = 0.5f;
            headMoveToPenisSpeedJSON.val = 0.35f;
        }

        private void PresetTwo()
        {
            headSpeedJSON.val = 0.05f;
            positionOne.val = 0.08f;
            positionTwo.val = 0.13f;
            headOffsetYJSON.val = 0.02f;
            headMoveRandomizerJSON.val = 0.1f;
            headSpeedRandomizerJSON.val = 0f;
            headMoveToPenisSpeedJSON.val = 0.25f;
        } 

        private void PresetThree()
        {
            headSpeedJSON.val = 0.55f;
            positionOne.val = -0.07f;
            positionTwo.val = 0.04f;
            headOffsetYJSON.val = 0.02f;
            headMoveRandomizerJSON.val = 20f;
            headSpeedRandomizerJSON.val = 0.5f;
            headMoveToPenisSpeedJSON.val = 0.55f;
        } 

        // OnDestroy is where you should put any cleanup
        // if you registered objects to supercontroller or atom, you should unregister them here
        void OnDestroy()
        {
        }
    }

}