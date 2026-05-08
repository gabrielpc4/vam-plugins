using UnityEngine;
using UnityEngine.PostProcessing;
using System;
using UnityEngine.UI;

namespace MacGruber
{
	namespace PostMagic 
	{		
		public class ChromaticAberration : MVRScript
		{
			private Manager manager;
			private ChromaticAberrationModel model;
			private ChromaticAberrationModel.Settings settings;
			
			private JSONStorableBool active;
			private JSONStorableFloat intensity;	
			private JSONStorableUrl spectralTexture;
			
			private Texture2D texture;
			
			public override void Init()
			{
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.chromaticAberration;
				
				Utils.SetupInfoText(this, 
					"<b>Intensity:</b> Amount of tangential distortion.\n\n" + 					
					"<b>Spectral LUT:</b> Shift the hue of chromatic aberrations.\n\n",
					800.0f, true
				);
				
				active = Utils.SetupToggle(this, "Chromatic Aberration Enabled", false, false);
				active.setCallbackFunction  += (bool v) => { model.enabled = enabledJSON.val = v; };
				
				intensity = Utils.SetupSliderFloat(this, "Intensity", 0.1f, 0.0f, 1.0f, false);
				intensity.setCallbackFunction  += (float v) => { settings.intensity = v; model.settings = settings; };			
										
				string defaultDir = "Custom/Assets/MacGruber/PostMagic/";
				string defaultTex = defaultDir + "Spectral LUTs/SpectralLut_BlueRed.png";
				TextureSettings textureSettings = new TextureSettings() { createMipMaps=false, linearColor=true, wrapMode=TextureWrapMode.Clamp, anisoLevel=0 };
				spectralTexture = Utils.SetupTexture2DChooser(this, "Spectral LUT", defaultTex, defaultDir, false, textureSettings, TextureLoaded);
				
				settings = ChromaticAberrationModel.Settings.defaultSettings;
				settings.intensity = intensity.val;
				model.settings = settings;
				
				spectralTexture.setCallbackFunction(spectralTexture.val); // load texture
								
				active.setCallbackFunction(active.val);
				enabledJSON.setCallbackFunction += (bool v) => { active.val = v; };
			}
			
			private void OnDestroy()
			{
				if (texture != null)
					Destroy(texture);
				texture = null;
			}
						
			private void TextureLoaded(Texture2D tex)
			{			
				if (texture != null)
					Destroy(texture);
				settings.spectralTexture = texture = tex;
				model.settings = settings;
			}
		}
	}
}