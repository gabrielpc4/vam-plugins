using UnityEngine;
using UnityEngine.PostProcessing;
using System;


namespace MacGruber
{
	namespace PostMagic 
	{		
		public class Grain : MVRScript
		{
			private Manager manager;
			private GrainModel model;
			private GrainModel.Settings settings;
			
			private JSONStorableBool active;
			private JSONStorableFloat intensity;
			private JSONStorableFloat size;
			private JSONStorableFloat luminanceContribution;
			private JSONStorableBool colored;
			
			public override void Init()
			{
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.grain;
				
				Utils.SetupInfoText(this, 
					"<b>Intensity:</b> Grain strength. Higher means more visible grain.\n\n" + 
					"<b>Size:</b> Grain particle size.\n\n" + 
					"<b>Luminance Contribution:</b> Controls the noisiness response curve based on scene luminance. Lower values mean less noise in dark areas.\n\n" +
					"<b>Colored:</b> Enable the use of colored grain.\n\n",
					800.0f, true
				);
				
				active = Utils.SetupToggle(this, "Grain Enabled", false, false);
				active.setCallbackFunction  += (bool v) => { model.enabled = enabledJSON.val = v; };
				
				intensity = Utils.SetupSliderFloat(this, "Intensity", 0.5f, 0.0f, 1.0f, false);
				intensity.setCallbackFunction  += (float v) => { settings.intensity = v; model.settings = settings; };
				
				size = Utils.SetupSliderFloat(this, "Size", 1.0f, 0.3f, 3.0f, false);
				size.setCallbackFunction  += (float v) => { settings.size = v; model.settings = settings; };
				
				luminanceContribution = Utils.SetupSliderFloat(this, "Luminance Contribution", 0.8f, 0.0f, 1.0f, false);
				luminanceContribution.setCallbackFunction  += (float v) => { settings.luminanceContribution = v; model.settings = settings; };
				
				colored = Utils.SetupToggle(this, "Colored", false, false);
				colored.setCallbackFunction  += (bool v) => { settings.colored = v; model.settings = settings; };
										
				
				settings = GrainModel.Settings.defaultSettings;
				settings.intensity = intensity.val;
				settings.size = size.val;
				settings.luminanceContribution = luminanceContribution.val;
				settings.colored = colored.val;
				model.settings = settings;
				
				
				active.setCallbackFunction(active.val);
				enabledJSON.setCallbackFunction += (bool v) => { active.val = v; };
			}
		}
	}
}