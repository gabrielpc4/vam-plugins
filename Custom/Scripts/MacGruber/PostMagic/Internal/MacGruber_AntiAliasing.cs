using UnityEngine;
using UnityEngine.PostProcessing;
using System.Collections.Generic;


namespace MacGruber
{
	namespace PostMagic 
	{		
		public class AntiAliasing : MVRScript
		{
			private Manager manager;
			private AntialiasingModel model;
			private AntialiasingModel.Settings settings;
			
			private JSONStorableBool active;
			private JSONStorableBool useTAA;
			private JSONStorableStringChooser fxaaPreset;
			private JSONStorableStringChooser msaaOverride;
			
			private readonly int[] msaaLevel = new int[] { 0, 2, 4, 8 };
			private readonly string[] msaaName = new string[] { "Off", "2x", "4x", "8x" };			
			private int originalMsaaLevel = 0;
			
			public override void Init()
			{
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.antialiasing;
				
				Utils.SetupInfoText(this,
					"<b>Enable TAA:</b> Use Temporal Anti-aliasing (TAA) <color=red>when in Desktop mode</color>. When in VR mode or when disabled we use FXAA.\n\n" +
					"<b>FXAA Preset:</b> Preset when using Fast Approximate Anti-aliasing (FXAA).\n\n" +
					"<b>MSAA Override:</b> Override VaMs setting for Multisample Anti-aliasing (MSAA). Note that MSAA will be disabled when using TAA. However, FXAA and MSAA can be combined.\n\n",
					800.0f, true
				);
				
				active = Utils.SetupToggle(this, "AntiAliasing Enabled", false, false);
				active.setCallbackFunction  += (bool v) => { model.enabled = enabledJSON.val = v; };
				
				useTAA = Utils.SetupToggle(this, "Enable TAA (Desktop only)", false, false);
				useTAA.setCallbackFunction  += (bool v) => {
					bool enableTAA = v && !Manager.gameIsVR;
					settings.method = enableTAA ? AntialiasingModel.Method.Taa : AntialiasingModel.Method.Fxaa;
					model.settings = settings;					
					if (enableTAA)
						UserPreferences.singleton.msaaLevel = 0;
				};
									
				fxaaPreset = Utils.SetupEnumChooser(this, "FXAA Preset", AntialiasingModel.FxaaPreset.Default, false, 
					(AntialiasingModel.FxaaPreset v) => { settings.fxaaSettings.preset = v; model.settings = settings; });
					
					
				List<string> modes = new List<string>(msaaName);
				msaaOverride = new JSONStorableStringChooser("MSAA Override", modes, modes[0], "MSAA Override");
				msaaOverride.setCallbackFunction += (string modeName) => {
					int idx = modes.FindIndex((string entry) => { return entry == modeName; });
					if (idx < 0 || idx >= msaaLevel.Length)
						idx = 0;
					if (useTAA.val && !Manager.gameIsVR)
						idx = 0;
					if (active.val)
						UserPreferences.singleton.msaaLevel = msaaLevel[idx];
				};
				CreateScrollablePopup(msaaOverride, false);
				RegisterStringChooser(msaaOverride);
					
				model.settings = settings = AntialiasingModel.Settings.defaultSettings;

				useTAA.setCallbackFunction(useTAA.val);
				fxaaPreset.setCallbackFunction(fxaaPreset.val);
				msaaOverride.setCallbackFunction(msaaOverride.val);
				
				active.setCallbackFunction(active.val);
				enabledJSON.setCallbackFunction += (bool v) => { active.val = v; };
			}
			
			private void OnEnable()
			{
				originalMsaaLevel = UserPreferences.singleton.msaaLevel;
				if (msaaOverride != null)
					msaaOverride.setCallbackFunction(msaaOverride.val);
			}
			
			private void OnDisable()
			{
				UserPreferences.singleton.msaaLevel = originalMsaaLevel;
			}
		}
	}
}