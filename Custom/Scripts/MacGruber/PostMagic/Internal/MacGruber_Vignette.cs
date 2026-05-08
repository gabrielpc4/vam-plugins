using UnityEngine;
using UnityEngine.PostProcessing;
using System;


namespace MacGruber
{
	namespace PostMagic 
	{		
		public class Vignette : MVRScript
		{
			private Manager manager;
			private VignetteModel model;
			private VignetteModel.Settings settings;
			
			private JSONStorableBool active;
			private JSONStorableColor color;
			private JSONStorableFloat intensity;
			private JSONStorableFloat smoothness;
			private JSONStorableFloat roundness;
			private JSONStorableBool rounded;
			private JSONStorableFloat centerX;
			private JSONStorableFloat centerY;			
						
			public override void Init()
			{			
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.vignette;
				
				Utils.SetupInfoText(this,
					"<b>Color:</b> Vignette color.\n\n" + 
					"<b>Intensity:</b> Amount of vignetting on screen.\n\n" + 
					"<b>Smoothness:</b> Smoothness of the vignette borders.\n\n" +
					"<b>Roundness:</b> Lower values will make a square-ish vignette.\n\n" +
					"<b>Rounded:</b> Should the vignette be perfectly round or be dependent on the current aspect ratio?\n\n" +
					"<b>Center X/Y:</b> Sets the vignette center point (screen center is [0.5,0.5]).\n\n",
					800.0f, true
				);
				
				active = Utils.SetupToggle(this, "Vignette Enabled", false, false);
				active.setCallbackFunction  += (bool v) => { model.enabled = enabledJSON.val = v; };
						 
				color = Utils.SetupColor(this, "Color", Color.black, false);
				color.setCallbackFunction  += (float h, float s, float v) => { settings.color = HSVColorPicker.HSVToRGB(h,s,v);	model.settings = settings; };
							
				intensity = Utils.SetupSliderFloat(this, "Intensity", 0.45f, 0.0f, 1.0f, false);
				intensity.setCallbackFunction  += (float v) => { settings.intensity = v; model.settings = settings; };
				
				smoothness = Utils.SetupSliderFloat(this, "Smoothness", 0.2f, 0.01f, 1.0f, false);
				smoothness.setCallbackFunction  += (float v) => { settings.smoothness = v; model.settings = settings; };
				
				roundness = Utils.SetupSliderFloat(this, "Roundness", 1.0f, 0.0f, 1.0f, false);
				roundness.setCallbackFunction  += (float v) => { settings.roundness = v; model.settings = settings; };
				
				rounded = Utils.SetupToggle(this, "Rounded", false, false);
				rounded.setCallbackFunction  += (bool v) => { settings.rounded = v; model.settings = settings; };
				
				centerX = Utils.SetupSliderFloat(this, "Center X", 0.5f, 0.0f, 1.0f, false);
				centerX.setCallbackFunction  += (float v) => { settings.center = new Vector2(centerX.val, centerY.val); model.settings = settings; };
				
				centerY = Utils.SetupSliderFloat(this, "Center Y", 0.5f, 0.0f, 1.0f, false);
				centerY.setCallbackFunction  += (float v) => { settings.center = new Vector2(centerX.val, centerY.val); model.settings = settings; };
										
				
				settings = VignetteModel.Settings.defaultSettings;
				settings.mode = VignetteModel.Mode.Classic;
				settings.color = HSVColorPicker.HSVToRGB(color.val);
				settings.intensity = intensity.val;
				settings.roundness = roundness.val;
				settings.roundness = roundness.val;
				settings.rounded = rounded.val;
				settings.center = new Vector2(centerX.val, centerY.val);
				model.settings = settings;
				
				active.setCallbackFunction(active.val);
				enabledJSON.setCallbackFunction += (bool v) => { active.val = v; };
			}
		}
	}
}