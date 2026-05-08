using UnityEngine;
using UnityEngine.PostProcessing;
using System;
using System.Collections.Generic;

namespace MacGruber
{
	namespace PostMagic
	{		
		public class AmbientOcclusion : MVRScript
		{
			private Manager manager;
			private AmbientOcclusionModel model;
			private AmbientOcclusionModel.Settings settings;
			
			private JSONStorableBool active;
			private JSONStorableFloat intensity;
			private JSONStorableFloat radius;
			private JSONStorableStringChooser sampleCount;
			
			public override void Init()
			{
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.ambientOcclusion;
				
				Utils.SetupInfoText(this,
					"<color=red><b>EXPERIMENTAL:</b> AmbientOcclusion from behind characters is applied on top of them!</color>\n\n" +
					"<b>Intensity:</b> Degree of darkness produced by the effect.\n\n" + 
					"<b>Radius:</b> Radius of sample points, which affects extent of darkened areas.\n\n" + 
					"<b>SampleCount:</b> Number of sample points, which affects quality and performance.\n\n",
					800.0f, true
				);				
				
				active = Utils.SetupToggle(this, "AmbientOcclusion Enabled", false, false);
				active.setCallbackFunction  += (bool v) => { model.enabled = enabledJSON.val = v; };
				
				intensity = Utils.SetupSliderFloat(this, "Intensity", 1.0f, 0.0f, 4.0f, false);
				intensity.setCallbackFunction  += (float v) => { settings.intensity = v; model.settings = settings; };
				
				radius = Utils.SetupSliderFloat(this, "Radius", 0.3f, 0.03f, 1.0f, false);
				radius.setCallbackFunction  += (float v) => { settings.radius = v; model.settings = settings; };
							
				sampleCount = Utils.SetupEnumChooser(this, "SampleCount", AmbientOcclusionModel.SampleCount.Medium, false, 
					(AmbientOcclusionModel.SampleCount v) => { settings.sampleCount = v; model.settings = settings; });				
				
				settings = AmbientOcclusionModel.Settings.defaultSettings;
				settings.intensity = intensity.val;
				settings.radius = radius.val;
				model.settings = settings;
				
				sampleCount.setCallbackFunction(sampleCount.val);
				
				active.setCallbackFunction(active.val);
				enabledJSON.setCallbackFunction += (bool v) => { active.val = v; };
			}
		}
	}
}