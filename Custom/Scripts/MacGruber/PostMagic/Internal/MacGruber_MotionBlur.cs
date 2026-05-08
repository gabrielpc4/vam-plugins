using UnityEngine;
using UnityEngine.PostProcessing;
using System;

namespace MacGruber
{
	namespace PostMagic 
	{		
		public class MotionBlur : MVRScript
		{
			private Manager manager;
			private MotionBlurModel model;
			private MotionBlurModel.Settings settings;
			
			private JSONStorableBool active;
			private JSONStorableFloat shutterAngle;
			private JSONStorableFloat sampleCount;
			private JSONStorableFloat frameBlending;
			
			public override void Init()
			{
				pluginLabelJSON.val = "(Desktop Only)";
				
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.motionBlur;
				
				Utils.SetupInfoText(this,
					"<color=red>MotionBlur is only supported in Desktop mode!</color>\n\n" +
					"<b>ShutterAngle:</b> The angle of rotary shutter. Larger values give longer exposure.\n\n" + 
					"<b>SampleCount:</b> The amount of sample points, which affects quality and performances.\n\n" + 
					"<b>FrameBlending:</b> The strength of multiple frame blending. The opacity of preceding frames are determined from this coefficient and time differences.\n\n",
					800.0f, true
				);
				
				active = Utils.SetupToggle(this, "MotionBlur Enabled", false, false);
				active.setCallbackFunction  += (bool v) => {
					enabledJSON.val = v;
					model.enabled = v && !Manager.gameIsVR;
				};
				
				shutterAngle = Utils.SetupSliderFloat(this, "ShutterAngle", 270.0f, 0.0f, 360.0f, false);
				shutterAngle.setCallbackFunction  += (float v) => { settings.shutterAngle = v; model.settings = settings; };
				
				sampleCount = Utils.SetupSliderFloat(this, "SampleCount", 10.0f, 4.0f, 32.0f, false);
				sampleCount.setCallbackFunction  += (float v) => {
					settings.sampleCount = Mathf.RoundToInt(v);
					model.settings = settings;
					sampleCount.val = settings.sampleCount;
				};
				
				frameBlending = Utils.SetupSliderFloat(this, "FrameBlending", 0.0f, 0.0f, 1.0f, false);
				frameBlending.setCallbackFunction  += (float v) => { settings.frameBlending = v; model.settings = settings; };
				
				
				settings = MotionBlurModel.Settings.defaultSettings;
				settings.shutterAngle = shutterAngle.val;
				settings.sampleCount = Mathf.RoundToInt(sampleCount.val);
				settings.frameBlending = frameBlending.val;
				model.settings = settings;
				
				active.setCallbackFunction(active.val);
				enabledJSON.setCallbackFunction += (bool v) => { active.val = v; };
			}
		}
	}
}