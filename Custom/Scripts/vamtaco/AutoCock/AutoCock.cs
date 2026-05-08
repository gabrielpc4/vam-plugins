using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace vamtaco
{
	/// <summary>
	/// Plugin Name:	Auto Cock
	///	Author:			vamtaco
	///	Description:	Set a flaccid and erect state for the penis.  Set arousal to make the penis transition between states.
	///					Please see README.md for more detail.
	/// </summary>
	public class AutoCock : MVRScript
    {
        //as the arousal level rises, the penis morphs back toward an erect state
        //val 0 - 1f
        private JSONStorableFloat _arousal;

        private JSONStorableFloat _arousalUpDownRate;

        //default flaccid values
        private JSONStorableFloat _penisBaseControlJointRotationDriveXTargetFlaccid;
        private JSONStorableFloat _penisMidControlJointRotationDriveXTargetFlaccid;
        private JSONStorableFloat _penisTipControlJointRotationDriveXTargetFlaccid;

        //flaccid lean
        private JSONStorableFloat _penisBaseControlJointRotationDriveYTargetFlaccid;
        private JSONStorableFloat _penisBaseControlJointRotationDriveZTargetFlaccid;

        //flaccid physics
        private JSONStorableBool _flaccidPhysicsEnabled;

        //default erect values
        private JSONStorableFloat _penisBaseControlJointRotationDriveXTargetErect;
        private JSONStorableFloat _penisMidControlJointRotationDriveXTargetErect;
        private JSONStorableFloat _penisTipControlJointRotationDriveXTargetErect;

        //erect lean
        private JSONStorableFloat _penisBaseControlJointRotationDriveYTargetErect;
        private JSONStorableFloat _penisBaseControlJointRotationDriveZTargetErect;

        //cached morph control
        private GenerateDAZMorphsControlUI _morphControl;

        //FreeControllerV3 controls used
        private FreeControllerV3 _penisBaseControl;
        private FreeControllerV3 _penisMidControl;
        private FreeControllerV3 _penisTipControl;

        private float _sliderHeight;

        private AutoCockJson _autoCockJson;

        public Dictionary<string, DAZMorph> Morphs;

        private Dictionary<string, JSONStorableFloat> FlaccidMorphsValues;
        private Dictionary<string, JSONStorableFloat> ErectMorphsValues;

        private string _instructions;

        public override void Init()
        {
            if (containingAtom.type != "Person")
            {
                SuperController.LogMessage("AutoCock Plugin (Disabled): This plugin is designed to be used on a male person atom or a female person atom that has use male morphs enabled.");
                enabled = false;
                return;
            }

            //get morphs list from file...
            var json = SuperController.singleton.ReadFileIntoString("Custom/Scripts/vamtaco/AutoCock/AutoCock.Morphs.json");
            _autoCockJson = new AutoCockJson(json);

            Morphs = new Dictionary<string, DAZMorph>();
            FlaccidMorphsValues = new Dictionary<string, JSONStorableFloat>();
            ErectMorphsValues = new Dictionary<string, JSONStorableFloat>();

            //get the morph control to access morphs
            JSONStorable geo = containingAtom.GetStorableByID("geometry");
            DAZCharacterSelector character = geo as DAZCharacterSelector;
            _morphControl = character.morphsControlUI;

            //gather supported morphs
            foreach (var sm in _autoCockJson.Morphs)
            {
                var morph = _morphControl.GetMorphByDisplayName(sm.Key);

                if (morph != null)
                    Morphs.Add(sm.Key, morph);
            }

            //store the FreeControllers that will be used
            _penisBaseControl = containingAtom.freeControllers.Single(c => c.storeId == "penisBaseControl");
            _penisMidControl = containingAtom.freeControllers.Single(c => c.storeId == "penisMidControl");
            _penisTipControl = containingAtom.freeControllers.Single(c => c.storeId == "penisTipControl");

            //get the instructins from the file
            _instructions = SuperController.singleton.ReadFileIntoString("Custom/Scripts/vamtaco/AutoCock/AutoCock.Instructions.txt");

            //setup instuctions ui button
            var instructionsButton = CreateButton("View Instructions");
            instructionsButton.buttonColor = new Color(0.75f, 0.44f, 0.1f);
            instructionsButton.button.onClick.AddListener(() =>
            {
                SuperController.singleton.ClearMessages();
                SuperController.LogMessage(_instructions);
            });
            
            var resetPluginDefaults = CreateButton("Reset Plugin Defaults", true);
            resetPluginDefaults.button.onClick.AddListener(() =>
            {
                SetDefaultFlaccidState();
                SetDefaultErectState();
                Flaccid();
            });

            //setup arousal var and ui
            _arousal = new JSONStorableFloat("Arousal", 0.0f, SetArousalCallback, 0.0f, 1f);
            RegisterFloat(_arousal);
            var slider = CreateSlider(_arousal);

            //this will be used to get the correct height for spacers
            _sliderHeight = slider.height;

            _arousalUpDownRate = new JSONStorableFloat("Arousal Up / Down Rate", 0.1f, 0.0f, 1f);
            RegisterFloat(_arousalUpDownRate);
            CreateSlider(_arousalUpDownRate, true);

            CreateSpacer().height = instructionsButton.height;
            CreateSpacer(true).height = instructionsButton.height;

            //set current penis state as flaccid
            var flaccidButton = CreateButton("Flaccid");
            flaccidButton.button.onClick.AddListener(() =>
            {
                Flaccid();
            });

            _flaccidPhysicsEnabled = new JSONStorableBool("Flaccid Physics Enabled", false, FlaccidPhysicsEnabledCallback);
            RegisterBool(_flaccidPhysicsEnabled);
            CreateToggle(_flaccidPhysicsEnabled, true);

            bool right = false;

            foreach (var morph in Morphs)
            {
                
                var morphFlaccidValue = new JSONStorableFloat($"Flaccid {morph.Key}"
                    , _autoCockJson.Morphs[morph.Key].InitialFlaccidValue
                    , UpdateFlaccidDataCallback
                    , morph.Value.min
                    , morph.Value.max);

                var displayName = _autoCockJson.Morphs[morph.Key].DisplayName;
                if (displayName == string.Empty)
                    displayName = _autoCockJson.Morphs[morph.Key].Name;

                RegisterFloat(morphFlaccidValue);
                CreateSlider(morphFlaccidValue, right).labelText.text = displayName;
                right = !right;

                FlaccidMorphsValues[morph.Key] = morphFlaccidValue;
            }

            if(right)
                CreateSpacer(true).height = _sliderHeight;

            //flaccid data ui
            _penisBaseControlJointRotationDriveXTargetFlaccid = new JSONStorableFloat("Flaccid Base Joint Rotation Drive X Target", -55f, UpdateFlaccidDataCallback, -80f, 80f);
            RegisterFloat(_penisBaseControlJointRotationDriveXTargetFlaccid);
            CreateSlider(_penisBaseControlJointRotationDriveXTargetFlaccid).labelText.text = "Penis Base Flaccid";

            _penisMidControlJointRotationDriveXTargetFlaccid = new JSONStorableFloat("Flaccid Mid Joint Rotation Drive X Target", -18f, UpdateFlaccidDataCallback, -80f, 80f);
            RegisterFloat(_penisMidControlJointRotationDriveXTargetFlaccid);
            CreateSlider(_penisMidControlJointRotationDriveXTargetFlaccid, true).labelText.text = "Penis Mid Flaccid";

            _penisTipControlJointRotationDriveXTargetFlaccid = new JSONStorableFloat("Flaccid Tip Joint Rotation Drive X Target", 0.0f, UpdateFlaccidDataCallback, -80f, 80f);
            RegisterFloat(_penisTipControlJointRotationDriveXTargetFlaccid);
            CreateSlider(_penisTipControlJointRotationDriveXTargetFlaccid).labelText.text = "Penis Tip Flaccid";

            _penisBaseControlJointRotationDriveYTargetFlaccid = new JSONStorableFloat("Flaccid Lean", 0f, UpdateFlaccidDataCallback, -80f, 80f);
            RegisterFloat(_penisBaseControlJointRotationDriveYTargetFlaccid);
            CreateSlider(_penisBaseControlJointRotationDriveYTargetFlaccid, true).labelText.text = "Lean Right (-) / Left (+)";

            _penisBaseControlJointRotationDriveZTargetFlaccid = new JSONStorableFloat("Flaccid Twist", 0f, UpdateFlaccidDataCallback, -45f, 45f);
            RegisterFloat(_penisBaseControlJointRotationDriveZTargetFlaccid);
            CreateSlider(_penisBaseControlJointRotationDriveZTargetFlaccid).labelText.text = "Twist Right (-) / Left (+)";

            CreateSpacer(true).height = _sliderHeight;
            CreateSpacer().height = instructionsButton.height;
            CreateSpacer(true).height = instructionsButton.height;

            //set current penis state as erect
            var erectButton = CreateButton("Erect");
            erectButton.button.onClick.AddListener(() =>
            {
                Erect();
            });

            CreateSpacer(true).height = flaccidButton.height;

            right = false;

            foreach (var morph in Morphs)
            {
                var morphErectValue = new JSONStorableFloat($"Erect {morph.Key}"
                    , _autoCockJson.Morphs[morph.Key].InitialErectValue
                    , UpdateErectDataCallback
                    , morph.Value.min
                    , morph.Value.max);

                var displayName = _autoCockJson.Morphs[morph.Key].DisplayName;
                if (displayName == string.Empty)
                    displayName = _autoCockJson.Morphs[morph.Key].Name;

                RegisterFloat(morphErectValue);
                CreateSlider(morphErectValue, right).labelText.text = displayName;
                right = !right;

                ErectMorphsValues[morph.Key] = morphErectValue;
            }

            if(right)
                CreateSpacer(true).height = _sliderHeight;

            //erect data ui
            _penisBaseControlJointRotationDriveXTargetErect = new JSONStorableFloat("Erect Base Joint Rotation Drive X Target", 0f, UpdateErectDataCallback, -90f, 90f);
            RegisterFloat(_penisBaseControlJointRotationDriveXTargetErect);
            CreateSlider(_penisBaseControlJointRotationDriveXTargetErect).labelText.text = "Penis Base Erect";

            _penisMidControlJointRotationDriveXTargetErect = new JSONStorableFloat("Erect Mid Joint Rotation Drive X Target", 0f, UpdateErectDataCallback, -90f, 90f);
            RegisterFloat(_penisMidControlJointRotationDriveXTargetErect);
            CreateSlider(_penisMidControlJointRotationDriveXTargetErect, true).labelText.text = "Penis Mid Erect";

            _penisTipControlJointRotationDriveXTargetErect = new JSONStorableFloat("Erect Tip Joint Rotation Drive X Target", 0f, UpdateErectDataCallback, -90f, 90f);
            RegisterFloat(_penisTipControlJointRotationDriveXTargetErect);
            CreateSlider(_penisTipControlJointRotationDriveXTargetErect).labelText.text = "Penis Tip Erect";

            _penisBaseControlJointRotationDriveYTargetErect = new JSONStorableFloat("Erect Lean", 0f, UpdateErectDataCallback, -80f, 80f);
            RegisterFloat(_penisBaseControlJointRotationDriveYTargetErect);
            CreateSlider(_penisBaseControlJointRotationDriveYTargetErect, true).labelText.text = "Lean Right (-) / Left (+)";

            _penisBaseControlJointRotationDriveZTargetErect = new JSONStorableFloat("Erect Twist", 0f, UpdateErectDataCallback, -45f, 45f);
            RegisterFloat(_penisBaseControlJointRotationDriveZTargetErect);
            CreateSlider(_penisBaseControlJointRotationDriveZTargetErect).labelText.text = "Twist Right (-) / Left (+)";

            CreateSpacer(true).height = _sliderHeight;
            CreateSpacer().height = instructionsButton.height;
            CreateSpacer(true).height = instructionsButton.height;

            //expose actions that can be used by triggers, etc
            JSONStorableAction flaccidAction = new JSONStorableAction("Flaccid", () => Flaccid());
            RegisterAction(flaccidAction);

            JSONStorableAction erectAction = new JSONStorableAction("Erect", () => Erect());
            RegisterAction(erectAction);

            JSONStorableAction arousalUpAction = new JSONStorableAction("Arousal Up", () => ArousalUp());
            RegisterAction(arousalUpAction);

            JSONStorableAction arousalDownAction = new JSONStorableAction("Arousal Down", () => ArousalDown());
            RegisterAction(arousalDownAction);

            Flaccid();
        }

        //normalizes a range to 0 - returned value
        private float GetMaxRange(float min, float max)
        {
            //min is negative (should work if max is neg or pos)
            if (min < 0)
                return max + (min * -1);

            //both positive
            return max - min;
        }

        private void SetArousalCallback(float arousal)
        {
            float maxRange = 0.0f;

            foreach (var morph in Morphs)
            {
                maxRange = GetMaxRange(FlaccidMorphsValues[morph.Key].val, ErectMorphsValues[morph.Key].val);
                morph.Value.morphValue = (maxRange * arousal) + FlaccidMorphsValues[morph.Key].val;
            }

            maxRange = GetMaxRange(_penisBaseControlJointRotationDriveXTargetFlaccid.val, _penisBaseControlJointRotationDriveXTargetErect.val);
            _penisBaseControl.jointRotationDriveXTarget = (maxRange * arousal) + _penisBaseControlJointRotationDriveXTargetFlaccid.val;

            maxRange = GetMaxRange(_penisMidControlJointRotationDriveXTargetFlaccid.val, _penisMidControlJointRotationDriveXTargetErect.val);
            _penisMidControl.jointRotationDriveXTarget = (maxRange * arousal) + _penisMidControlJointRotationDriveXTargetFlaccid.val;

            maxRange = GetMaxRange(_penisTipControlJointRotationDriveXTargetFlaccid.val, _penisTipControlJointRotationDriveXTargetErect.val);
            _penisTipControl.jointRotationDriveXTarget = (maxRange * arousal) + _penisTipControlJointRotationDriveXTargetFlaccid.val;

            //lean
            maxRange = GetMaxRange(_penisBaseControlJointRotationDriveYTargetFlaccid.val, _penisBaseControlJointRotationDriveYTargetErect.val);
            _penisBaseControl.jointRotationDriveYTarget = (maxRange * arousal) + _penisBaseControlJointRotationDriveYTargetFlaccid.val;

            maxRange = GetMaxRange(_penisBaseControlJointRotationDriveZTargetFlaccid.val, _penisBaseControlJointRotationDriveZTargetErect.val);
            _penisBaseControl.jointRotationDriveZTarget = (maxRange * arousal) + _penisBaseControlJointRotationDriveZTargetFlaccid.val;

            //physics support
            maxRange = GetMaxRange(1f, 25f);
            _penisBaseControl.jointRotationDriveSpring = (maxRange * arousal) + 0f;

            maxRange = GetMaxRange(0.1f, 0.1f);
            _penisBaseControl.jointRotationDriveDamper = (maxRange * arousal) + 0.1f;

            maxRange = GetMaxRange(0.05f, 10f);
            _penisBaseControl.jointRotationDriveMaxForce = (maxRange * arousal) + 0.05f;
            
            maxRange = GetMaxRange(1f, 24f);
            _penisMidControl.jointRotationDriveSpring = (maxRange * arousal) + 0f;
            
            maxRange = GetMaxRange(0.1f, 0.1f);
            _penisMidControl.jointRotationDriveDamper = (maxRange * arousal) + 0.1f;

            maxRange = GetMaxRange(0.05f, 10f);
            _penisMidControl.jointRotationDriveMaxForce = (maxRange * arousal) + 0.05f;

            maxRange = GetMaxRange(1f, 12f);
            _penisTipControl.jointRotationDriveSpring = (maxRange * arousal) + 0f;

            maxRange = GetMaxRange(0.1f, 0.1f);
            _penisTipControl.jointRotationDriveDamper = (maxRange * arousal) + 0.1f;

            maxRange = GetMaxRange(1f, 10f);
            _penisTipControl.jointRotationDriveMaxForce = (maxRange * arousal) + 1f;
        }

        private void UpdateFlaccidDataCallback(float val)
        {
            //we actually don't care about the value here since it has already been set on the state, so simply call Flaccid to update in real time.
            Flaccid();
        }

        private void UpdateErectDataCallback(float val)
        {
            //we actually don't care about the value here since it has already been set on the state, so simply call Erect to update in real time.
            Erect();
        }

		private void FlaccidPhysicsEnabledCallback(bool val)
		{
			if(val)
			{
				SetFlaccidPhysics();
				return;
			}

			SetErectPhysics();
		}


		public void Flaccid()
        {
            _arousal.SetVal(0);

            foreach (var morph in Morphs)
            {
                morph.Value.morphValue = FlaccidMorphsValues[morph.Key].val;
            }

            _penisBaseControl.jointRotationDriveXTarget = _penisBaseControlJointRotationDriveXTargetFlaccid.val;
            _penisMidControl.jointRotationDriveXTarget = _penisMidControlJointRotationDriveXTargetFlaccid.val;
            _penisTipControl.jointRotationDriveXTarget = _penisTipControlJointRotationDriveXTargetFlaccid.val;

            //lean
            _penisBaseControl.jointRotationDriveYTarget = _penisBaseControlJointRotationDriveYTargetFlaccid.val;
            _penisBaseControl.jointRotationDriveZTarget = _penisBaseControlJointRotationDriveZTargetFlaccid.val;

            //physics support
            if (_flaccidPhysicsEnabled.val)
                SetFlaccidPhysics();
        }

        private void SetFlaccidPhysics()
        {
            _penisBaseControl.jointRotationDriveSpring = 1f;
            _penisBaseControl.jointRotationDriveDamper = 0.1f;
            _penisBaseControl.jointRotationDriveMaxForce = 0.05f;
            _penisMidControl.jointRotationDriveSpring = 1f;
            _penisMidControl.jointRotationDriveDamper = 0.1f;
            _penisMidControl.jointRotationDriveMaxForce = 0.05f;
            _penisTipControl.jointRotationDriveSpring = 1f;
            _penisTipControl.jointRotationDriveDamper = 0.1f;
            _penisTipControl.jointRotationDriveMaxForce = 1f;
        }

        public void Erect()
        {
            _arousal.SetVal(1);

            foreach (var morph in Morphs)
            {
                morph.Value.morphValue = ErectMorphsValues[morph.Key].val;
            }

            _penisBaseControl.jointRotationDriveXTarget = _penisBaseControlJointRotationDriveXTargetErect.val;
            _penisMidControl.jointRotationDriveXTarget = _penisMidControlJointRotationDriveXTargetErect.val;
            _penisTipControl.jointRotationDriveXTarget = _penisTipControlJointRotationDriveXTargetErect.val;

            //lean
            _penisBaseControl.jointRotationDriveYTarget = _penisBaseControlJointRotationDriveYTargetErect.val;
            _penisBaseControl.jointRotationDriveZTarget = _penisBaseControlJointRotationDriveZTargetErect.val;

            //physics support
            SetErectPhysics();
        }

        private void SetErectPhysics()
        {
            _penisBaseControl.jointRotationDriveSpring = 25f;
            _penisBaseControl.jointRotationDriveDamper = 0.1f;
            _penisBaseControl.jointRotationDriveMaxForce = 10f;
            _penisMidControl.jointRotationDriveSpring = 24f;
            _penisMidControl.jointRotationDriveDamper = 0.1f;
            _penisMidControl.jointRotationDriveMaxForce = 10f;
            _penisTipControl.jointRotationDriveSpring = 12f;
            _penisTipControl.jointRotationDriveDamper = 0.1f;
            _penisTipControl.jointRotationDriveMaxForce = 10f;
        }

        private void SetDefaultFlaccidState()
        {
            foreach (var fv in FlaccidMorphsValues)
            {
                fv.Value.SetValToDefault();
            }

            _penisBaseControlJointRotationDriveXTargetFlaccid.SetValToDefault();
            _penisMidControlJointRotationDriveXTargetFlaccid.SetValToDefault();
            _penisTipControlJointRotationDriveXTargetFlaccid.SetValToDefault();

            //lean
            _penisBaseControlJointRotationDriveYTargetFlaccid.SetValToDefault();
            _penisBaseControlJointRotationDriveZTargetFlaccid.SetValToDefault();
        }

        private void SetDefaultErectState()
        {
            foreach (var ev in ErectMorphsValues)
            {
                ev.Value.SetValToDefault();
            }

            _penisBaseControlJointRotationDriveXTargetErect.SetValToDefault();
            _penisMidControlJointRotationDriveXTargetErect.SetValToDefault();
            _penisTipControlJointRotationDriveXTargetErect.SetValToDefault();

            //lean
            _penisBaseControlJointRotationDriveYTargetErect.SetValToDefault();
            _penisBaseControlJointRotationDriveZTargetErect.SetValToDefault();
        }

        private void ArousalUp()
        {
            _arousal.SetVal(_arousal.val + _arousalUpDownRate.val);
        }

        private void ArousalDown()
        {
            _arousal.SetVal(_arousal.val - _arousalUpDownRate.val);
        }
    }

    public class AutoCockJson
    {
        public Dictionary<string, AutoCockMorph> Morphs;

        public AutoCockJson(string json)
        {
            Morphs = new Dictionary<string, AutoCockMorph>();

            var node = SimpleJSON.JSON.Parse(json);
            var array = node["morphs"].AsArray;
            for (int i = 0; i < array.Count; i++)
            {
                var name = array[i]["name"].Value;

                Morphs.Add(name, new AutoCockMorph
                {
                    Name = name,
                    InitialFlaccidValue = array[i]["initialFlaccidValue"].AsFloat,
                    InitialErectValue = array[i]["initialErectValue"].AsFloat,
                    DisplayName = array[i]["displaNname"].Value
                });
            }
        }
    }

    public class AutoCockMorph
    {
        public string Name { get; set; }
        public float InitialFlaccidValue { get; set; }
        public float InitialErectValue { get; set; }
        public string DisplayName { get; set; }
    }
}
