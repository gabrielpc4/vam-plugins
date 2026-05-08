using UnityEngine;
using UnityEngine.PostProcessing;
using System;

namespace MacGruber
{
	namespace PostMagic 
	{		
		public class Bloom : MVRScript
		{
			private Manager manager;
			private BloomModel model;
			private BloomModel.Settings settings;
			
			private JSONStorableBool active;
			private JSONStorableFloat intensity;
			private JSONStorableFloat threshold;
			private JSONStorableFloat softKnee;
			private JSONStorableFloat radius;
			private JSONStorableBool antiFlicker;
			private JSONStorableFloat lensDirt;
			private JSONStorableUrl lensDirtTexture;
			
			private Texture2D texture;
			
			public override void Init()
			{
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.bloom;
				
				Utils.SetupInfoText(this, 
					"<b>Intensity:</b> Strength of the bloom filter.\n\n" + 
					"<b>Threshold:</b> Filters out pixels under this level of brightness. (lower = brighter)\n\n" + 
					"<b>SoftKnee:</b> Makes transition between under/over-threshold gradual (0 = hard threshold, 1 = soft threshold).\n\n" +
					"<b>Radius:</b> Changes extent of veiling effects in a screen resolution-independent fashion. Affects performance, smaller = faster.\n\n" +
					"<b>Anti Flicker:</b> Reduces flashing noise with an additional filter.\n\n" +
					"<b>LensDirt Intensity:</b> Amount of lens dirtiness. Set to zero to turn off.\n\n" +
					"<b>LensDirt Texture:</b> Dirtiness texture to add smudges or dust to the lens.\n\n",
					800.0f, true
				);
				
				active = Utils.SetupToggle(this, "Bloom Enabled", false, false);
				active.setCallbackFunction  += (bool v) => { model.enabled = enabledJSON.val = v; };
				
				intensity = Utils.SetupSliderFloat(this, "Intensity", 0.5f, 0.0f, 5.0f, false);
				intensity.setCallbackFunction  += (float v) => { settings.bloom.intensity = v; model.settings = settings; };
				
				threshold = Utils.SetupSliderFloat(this, "Threshold", 1.1f, 0.0f, 2.0f, false);
				threshold.setCallbackFunction  += (float v) => { settings.bloom.threshold = v; model.settings = settings; };
				
				softKnee = Utils.SetupSliderFloat(this, "SoftKnee", 0.5f, 0.0f, 1.0f, false);
				softKnee.setCallbackFunction  += (float v) => { settings.bloom.softKnee = v; model.settings = settings; };
				
				radius = Utils.SetupSliderFloat(this, "Radius", 4.0f, 1.0f, 7.0f, false);
				radius.setCallbackFunction  += (float v) => { settings.bloom.radius = v; model.settings = settings; };
				
				antiFlicker = Utils.SetupToggle(this, "Anti Flicker", false, false);
				antiFlicker.setCallbackFunction  += (bool v) => { settings.bloom.antiFlicker = v; model.settings = settings; };
				
				lensDirt = Utils.SetupSliderFloat(this, "LensDirt Intensity", 1.0f, 0.0f, 10.0f, false);
				lensDirt.setCallbackFunction  += (float v) => {
					settings.lensDirt.intensity = v;
					settings.lensDirt.texture = v > 0.0f ? texture : null;
					model.settings = settings;
				};
				
				string defaultDir = "Custom/Assets/MacGruber/PostMagic/";
				string defaultTex = defaultDir + "Lens Dirt/LensDirt00.png";
				TextureSettings textureSettings = new TextureSettings() { createMipMaps=true, linearColor=true, anisoLevel=0 };
				lensDirtTexture = Utils.SetupTexture2DChooser(this, "LensDirt Texture", defaultTex, defaultDir, false, textureSettings, TextureLoaded);
				
				settings = BloomModel.Settings.defaultSettings;
				settings.bloom.intensity = intensity.val;
				settings.bloom.threshold = threshold.val;
				settings.bloom.softKnee = softKnee.val;
				settings.bloom.radius = radius.val;
				settings.bloom.antiFlicker = antiFlicker.val;
				settings.lensDirt.intensity = lensDirt.val;
				model.settings = settings;
				
				lensDirtTexture.setCallbackFunction(lensDirtTexture.val); // load texture
				
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
				texture = tex;
				settings.lensDirt.texture = lensDirt.val > 0.0f ? texture : null;
				model.settings = settings;
			}
		}
	}
}