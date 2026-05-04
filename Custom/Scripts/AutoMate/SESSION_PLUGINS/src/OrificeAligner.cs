using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
using System.Collections.Specialized;

namespace JayJayWon
{
    public class OrificeAligner : MVRScript
    {
		public override void Init() 
		{
			try {
		        const string pluginName = "OrificeAligner";
				const string pluginVersion = "v1.0.0";

				_orificeFemaleChoices = new List<string>();
				_orificeFemaleChoices.Add( "Mouth");
				_orificeFemaleChoices.Add("Vagina");
				_orificeFemaleChoices.Add("Anus");
				
				_orificeMaleChoices = new List<string>();
				_orificeMaleChoices.Add( "Mouth");
				_orificeMaleChoices.Add("Anus");
				
				List<string> rotationModeChoices  = new List<string>();
				rotationModeChoices.Add( "Point at Target");
				rotationModeChoices.Add( "Sync with Target");

				_thisAtom = containingAtom ;
				_thisControl = _thisAtom.freeControllers.First(fc => fc.name == "control");

                // make atom selector
                _atom1JSON = new JSONStorableStringChooser("atom1", SuperController.singleton.GetAtomUIDs(), null, "Atom", SyncAtom1);
                SyncAtom1Choices();
				_atom1JSON.storeType = JSONStorableParam.StoreType.Physical;
                RegisterStringChooser(_atom1JSON);
                
                _displayPopup = CreateScrollablePopup(_atom1JSON);
                _displayPopup.popupPanelHeight = 700f;
                // want to always resync the atom choices on opening popup since atoms can be added/removed
                _displayPopup.popup.onOpenPopupHandlers = SyncAtom1Choices;
                _displayPopup.label = "Target Character";

				if (_atom1JSON == null)
				{
					SyncAtom1("");
				}
				else
				{
					SyncAtom1(_atom1JSON.val);
				}
								
				_orificeJSON = new JSONStorableStringChooser("OrificeChoice", null, "Vagina", "", OrificeSelect);
                _orificeJSON.storeType = JSONStorableParam.StoreType.Physical;
				RegisterStringChooser(_orificeJSON);
				
                _displayPopup = CreateScrollablePopup(_orificeJSON,true);
                _displayPopup.popupPanelHeight = 300f;
				_displayPopup.popup.onOpenPopupHandlers += SyncOrificeChoices;
                _displayPopup.label = "Target Orifice:";

				_isMale = false ;
				_orificeJSON.choices = _orificeFemaleChoices ;

                var spacer = CreateSpacer(true);
                spacer.height = 100;
                spacer = CreateSpacer(false);
                spacer.height = 100;

				_controlPosJSON = new JSONStorableBool("Control Position",true);
                _controlPosJSON.storeType = JSONStorableParam.StoreType.Physical;
                RegisterBool(_controlPosJSON);
                var posToggle = CreateToggle(_controlPosJSON);
				
				_controlRotJSON = new JSONStorableBool("Control Rotation",true);
                _controlRotJSON.storeType = JSONStorableParam.StoreType.Physical;
                RegisterBool(_controlRotJSON);
                var rotToggle = CreateToggle(_controlRotJSON, true);

                _controlPosOffsetXJSON = new JSONStorableFloat("X Position Offset", 0f, -1f, 1f);
                _controlPosOffsetXJSON.storeType = JSONStorableParam.StoreType.Physical;
                RegisterFloat(_controlPosOffsetXJSON);
                CreateSlider(_controlPosOffsetXJSON);				

                _controlPosOffsetYJSON = new JSONStorableFloat("Y Position Offset", 0f, -1f, 1f);
                _controlPosOffsetYJSON.storeType = JSONStorableParam.StoreType.Physical;
                RegisterFloat(_controlPosOffsetYJSON);
                CreateSlider(_controlPosOffsetYJSON);				

                _controlPosOffsetZJSON = new JSONStorableFloat("Z Position Offset", 0f, -1f, 1f);
                _controlPosOffsetZJSON.storeType = JSONStorableParam.StoreType.Physical;
                RegisterFloat(_controlPosOffsetZJSON);
                CreateSlider(_controlPosOffsetZJSON);				

                _controlRotOffsetXJSON = new JSONStorableFloat("X Rotation Offset", 0f, -180f, 180f);
                _controlRotOffsetXJSON.storeType = JSONStorableParam.StoreType.Physical;
                RegisterFloat(_controlRotOffsetXJSON);
                CreateSlider(_controlRotOffsetXJSON, true);				

                _controlRotOffsetYJSON = new JSONStorableFloat("Y Rotation Offset", 0f, -180f, 180f);
                _controlRotOffsetYJSON.storeType = JSONStorableParam.StoreType.Physical;
                RegisterFloat(_controlRotOffsetYJSON);
                CreateSlider(_controlRotOffsetYJSON, true);				

                _controlRotOffsetZJSON = new JSONStorableFloat("Z Rotation Offset", 0f, -180f, 180f);
                _controlRotOffsetZJSON.storeType = JSONStorableParam.StoreType.Physical;
                RegisterFloat(_controlRotOffsetZJSON);
                CreateSlider(_controlRotOffsetZJSON, true);				

				_rotationModeJSON = new  JSONStorableStringChooser("RotationMode", null, "Point at Target", "", RotationModeSelect);
                _rotationModeJSON.storeType = JSONStorableParam.StoreType.Physical;
				RegisterStringChooser(_rotationModeJSON);
				_rotationModeJSON.choices = rotationModeChoices;
                _displayPopup = CreateScrollablePopup(_rotationModeJSON,true);
                _displayPopup.popupPanelHeight = 750f;
                _displayPopup.label = "Rotation Mode:";
				
				spacer = CreateSpacer(false);
                spacer.height = 380;
                var versionLabel = CreateTextField(new JSONStorableString("", ""));
				versionLabel.text = pluginName + " " + pluginVersion ;
				versionLabel.height =10;

			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		protected bool CheckMaleGender (Atom atom)
		{
			if (atom == null)
			{
				return false ;
			}
			else
			{
				bool charName = atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("male");
				if (charName )
				{
					return true ;
				}
				else
				{
					return false ;
				}
			}
		}

		protected void UpdateOrificeChoices ()
		{
			if (_atom1 != null && _orificeJSON !=null)
			{
				if (!_isMale && CheckMaleGender (_atom1) )
				{
					_isMale = true;
					_orificeJSON.choices = _orificeMaleChoices ;					
					if (_orificeJSON.val == "Vagina") 
					{
						_orificeJSON.val = "Anus";
					}				
				}
				else if (_isMale && !CheckMaleGender (_atom1))
				{
					_isMale = false;
					_orificeJSON.choices = _orificeFemaleChoices ;
				}			
			}
		}

		protected void SyncOrificeChoices ()
		{
			UpdateOrificeChoices ();
		}

        protected void SyncAtom1Choices()
        {
            _atom1JSON.choices = GetAtomListChoices ();
        }

        protected List<string> GetAtomListChoices()
        {
            List<string> atomChoices = new List<string>();
			Atom atom ;
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
				atom = SuperController.singleton.GetAtomByUid(atomUID);
				JSONStorable geometry = atom.GetStorableByID("geometry");
				if (geometry != null)
				{
					atomChoices.Add(atomUID);
				}
            }
			return atomChoices;
		}

       // receiver Atom
        protected void SyncAtom1(string atomUID)
        {
			if (atomUID == null)
			{
				atomUID="";
			}
			if (atomUID == "" || !_atom1JSON.choices.Contains(atomUID))
            {
				SyncAtom1Choices();
				if (_atom1JSON.choices.Count >0)
				{
					_atom1JSON.val = _atom1JSON.choices[0];
				}
				atomUID = _atom1JSON.val;
            }
			if(atomUID !="" && atomUID !=null )
			{
				_atom1 = SuperController.singleton.GetAtomByUid(atomUID);
				UpdateOrificeChoices ();
				
				_lipTrigger = _atom1.rigidbodies.First(rb => rb.name == "LipTrigger");
				_mouthTrigger = _atom1.rigidbodies.First(rb => rb.name == "MouthTrigger");
				_labiaTrigger = _atom1.rigidbodies.First(rb => rb.name == "LabiaTrigger");
				_vaginaTrigger = _atom1.rigidbodies.First(rb => rb.name == "VaginaTrigger");
			}
        }

        protected void OrificeSelect(string orificeType)
        {
        }

        protected void RotationModeSelect(string rotationMode)
        {
        }
 
		// FixedUpdate is called with each physics simulation frame by Unity
		void FixedUpdate() 
		{
			try 
			{
				Vector3 targetPosition ;
				Vector3 targetUpDirection ;
				Vector3 tempPosition;
				Quaternion targetRotation ;
				Quaternion targetRotationInverse ;
				Quaternion tempRotation ;
				Transform tempTransform ;
				Transform anusTransform ;
				
				UpdateOrificeChoices ();

				float xRot = _thisControl.transform.eulerAngles.x;
				float zRot = _thisControl.transform.eulerAngles.z;

				if (_labiaTrigger != null)
				{
					targetPosition = _lipTrigger.transform.position ;
					targetUpDirection = _lipTrigger.transform.up;
					targetRotation = _lipTrigger.transform.rotation ;
					targetRotationInverse = _lipTrigger.transform.rotation ;
					Vector3 offsetPosition = new Vector3(_controlPosOffsetXJSON.val,_controlPosOffsetYJSON.val,_controlPosOffsetZJSON.val);
					Vector3 anusOffsetPosition ;
					
					if (_orificeJSON.val == "Mouth")
					{
						targetPosition = _lipTrigger.transform.position ;
						targetUpDirection = _lipTrigger.transform.up ;
						targetRotation = Quaternion.LookRotation (_mouthTrigger.transform.position - _lipTrigger.transform.position, targetUpDirection) ;
						targetRotationInverse = Quaternion.LookRotation (_lipTrigger.transform.position - _mouthTrigger.transform.position, targetUpDirection) ;
					}
					if (_orificeJSON.val == "Vagina")
					{
						targetPosition = _labiaTrigger.transform.position ;
						targetUpDirection = _labiaTrigger.transform.up ;
						targetRotation = Quaternion.LookRotation (_vaginaTrigger.transform.position - _labiaTrigger.transform.position, targetUpDirection) ;
						targetRotationInverse = Quaternion.LookRotation (_labiaTrigger.transform.position - _vaginaTrigger.transform.position, targetUpDirection) ;
					}
					if (_orificeJSON.val == "Anus")
					{
						anusTransform = _thisControl.transform ;
						tempPosition = anusTransform.position ;
						tempRotation = anusTransform.rotation ;
						
						anusTransform.position = _labiaTrigger.transform.position ;
						anusTransform.rotation = Quaternion.LookRotation (_labiaTrigger.transform.position - _vaginaTrigger.transform.position,_labiaTrigger.transform.up) ;
						
						if (!_isMale)
						{
							anusOffsetPosition = new Vector3 (0.0f,-0.025f,-0.0015f);
						}
						else
						{
							anusOffsetPosition = new Vector3 (0.0f,-0.025f,0.008f);
						}
						anusTransform.Translate (anusOffsetPosition, anusTransform) ;
						
						targetPosition = anusTransform.position;
						targetUpDirection = anusTransform.up ;
						targetRotation = Quaternion.LookRotation (_vaginaTrigger.transform.position - anusTransform.position, targetUpDirection) ;
						targetRotationInverse = Quaternion.LookRotation (anusTransform.position - _vaginaTrigger.transform.position, targetUpDirection) ;
						
						anusTransform.position = tempPosition ;
						anusTransform.rotation = tempRotation ;
					}
				
					if (_controlPosJSON.val)
					{
						_thisControl.transform.position = new Vector3(targetPosition.x, targetPosition.y, _thisControl.transform.position.z);
						tempTransform = _thisControl.transform ;
						tempTransform.rotation = targetRotationInverse ;					
						_thisControl.transform.Translate (offsetPosition, tempTransform);
					}
					if (_controlRotJSON.val)
					{
						if (_rotationModeJSON.val == "Sync with Target")
						{
							_thisControl.transform.rotation = targetRotation ;
						}
						if (_rotationModeJSON.val == "Point at Target")
						{
							var trabsform = targetRotation;
							//float xRot = _thisControl.transform.eulerAngles.x;
							float yRot = targetRotation.eulerAngles.y;
							//float zRot = targetRotation.eulerAngles.z;

							_thisControl.transform.rotation = Quaternion.Euler(new Vector3(xRot, yRot, zRot));

						}
						_thisControl.transform.Rotate (_controlRotOffsetXJSON.val, _controlRotOffsetYJSON.val, _controlRotOffsetZJSON.val);
					}
				}
				else
				{
					SyncAtom1 (_atom1JSON.val);
				}

			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		protected Atom _atom1;      
        protected JSONStorableStringChooser _atom1JSON;
        protected JSONStorableStringChooser _orificeJSON;
        protected JSONStorableStringChooser _rotationModeJSON;
        protected JSONStorableStringChooser _positionModeJSON;
		protected List<String> _orificeMaleChoices;
		protected List<String> _orificeFemaleChoices;
        protected JSONStorableBool _controlPosJSON;
        protected JSONStorableBool _controlRotJSON;
		protected bool _isMale ;

        protected JSONStorableFloat _controlPosOffsetXJSON;
        protected JSONStorableFloat _controlPosOffsetYJSON;
        protected JSONStorableFloat _controlPosOffsetZJSON;

        protected JSONStorableFloat _controlRotOffsetXJSON;
        protected JSONStorableFloat _controlRotOffsetYJSON;
        protected JSONStorableFloat _controlRotOffsetZJSON;

		private Atom _thisAtom ;
		private FreeControllerV3 _thisControl;

		private Rigidbody _lipTrigger;
		private Rigidbody _mouthTrigger;
		private Rigidbody _labiaTrigger;
		private Rigidbody _vaginaTrigger;
		
        protected UIDynamicPopup _displayPopup; // any UI element, just to check if visible
	}
}