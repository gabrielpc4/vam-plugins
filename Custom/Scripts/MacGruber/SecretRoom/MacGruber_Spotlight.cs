/* /////////////////////////////////////////////////////////////////////////////////////////////////
Spotlight v1.0.1 by MacGruber.
Spotlight customizer script for SecretRoom scenes

Version 1.0.1 2019-11-10
	Fixed version history (= this comment)	

Version 1.0 2019-09-20
	Initial release.
	
///////////////////////////////////////////////////////////////////////////////////////////////// */

using UnityEngine;
using UnityEngine.Events;

namespace MacGruber
{
    public class Spotlight : MVRScript
    {
		private Transform bracket;
		private Transform lamp;
		private Transform light;
		private bool init = false;
		
		private readonly Quaternion rotationOffset = Quaternion.Euler(90, 0, 0);
		
		private JSONStorableFloat yawAngle;
		private JSONStorableFloat pitchAngle;
		private JSONStorableFloat positionOffset;
		private JSONStorableColor emissionColor;
		
        public override void Init()
		{
			init = false;
			
			yawAngle = SetupSliderFloat("Yaw", 0.0f, -180.0f, 180.0f, false);
			pitchAngle = SetupSliderFloat("Pitch", -30.0f, -90.0f, 0.0f, false);
			positionOffset = SetupSliderFloat("Position Offset", 0.06f, -0.1f, 0.2f, false);
			yawAngle.setCallbackFunction += (float v) => { init = false; };
			pitchAngle.setCallbackFunction += (float v) => { init = false; };
			positionOffset.setCallbackFunction += (float v) => { init = false; };
			
					
			SetupButton("Force Refresh", () => { init = false; }, false);			
			
			SetupInfoText(
				"Place this script on a CustomUnityAsset atom that has the spotlight asset loaded. " +
				"Name this atom for example \"Spot1\". Then the script will search for an InvisibleLight " +
				"atom with the name \"Spot1_Light\" to update its position and rotation.\n\nIf you " +
				"manually moved this atom, use the ForceRefresh button.",
				350.0f, false
			);
			
			emissionColor = SetupColor("Emission Color", new Color(1.000f, 0.949f, 0.902f), true);
			emissionColor.setCallbackFunction += (float h, float s, float v) => { init = false; };
		}
		
		private void InitLamp()
		{
			if (init)
				return;
		
			try 
			{
				bracket = lamp = light = null;
				Transform baseplate = containingAtom.transform.Find("reParentObject/object/rescaleObject/Spotlight(Clone)");
				if (baseplate == null)
					return;
				
				bracket = baseplate.Find("Bracket");
				if (bracket == null)
					return;
				
				lamp = bracket.Find("Lamp");
				if (lamp == null)
					return;
				
				bracket.localEulerAngles = new Vector3(0.0f, yawAngle.val, 0.0f);
				lamp.localEulerAngles = new Vector3(90.0f+pitchAngle.val, 0.0f, 0.0f);
				
				Atom lightAtom = GetAtomById(containingAtom.uid + "_Light");
				if (lightAtom != null && lightAtom.type == "InvisibleLight")
				{
					JSONStorable lightControl = lightAtom.GetStorableByID("control");
					light = lightControl.transform;
					light.position = lamp.position - lamp.up * positionOffset.val;
					light.rotation = lamp.rotation * rotationOffset;				
				}
				
				Renderer lampRenderer = lamp.GetComponent<Renderer>();
				if (lampRenderer != null)
				{			
					HSVColor hsvColor = emissionColor.val;
					Color color = HSVColorPicker.HSVToRGB(hsvColor.H, hsvColor.S, hsvColor.V);
					lampRenderer.material.SetColor("_EmissionColor", color);
				}
				
				init = true;
			}
			catch (System.Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}
		
		private JSONStorableFloat SetupSliderFloat(string label, float defaultValue, float minValue, float maxValue, bool rightSide)
		{
			JSONStorableFloat storable = new JSONStorableFloat(label, defaultValue, minValue, maxValue, true, true);
			storable.storeType = JSONStorableParam.StoreType.Full;
			CreateSlider(storable, rightSide);
			RegisterFloat(storable);
			return storable;
		}
		
		private void SetupButton(string label, UnityAction callback, bool rightSide)
		{
			UIDynamicButton button = CreateButton(label, rightSide);
			button.button.onClick.AddListener(callback);
		}
		
		private JSONStorableColor SetupColor(string label, Color color, bool rightSide)
		{
			HSVColor hsvColor = HSVColorPicker.RGBToHSV(color.r, color.g, color.b);
			JSONStorableColor storable = new JSONStorableColor(label, hsvColor);
			storable.storeType = JSONStorableParam.StoreType.Full;
			CreateColorPicker(storable, rightSide);
			RegisterColor(storable);
			return storable;
		}
		
		private void SetupInfoText(string text, float height, bool rightSide)
		{
			JSONStorableString storable = new JSONStorableString("Info", text);
			UIDynamic textfield = CreateTextField(storable, rightSide);
			textfield.height = height;
		}
		
		private void Update()
		{
			if (!init)
				InitLamp();
		}
    }
	
	
}
