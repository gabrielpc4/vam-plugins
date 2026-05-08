using UnityEngine;
using UnityEngine.PostProcessing;
using System;
using UnityEngine.UI;

namespace MacGruber
{
	namespace PostMagic 
	{		
		public class UserLUT : MVRScript
		{
			private Manager manager;
			private UserLutModel model;
			private UserLutModel.Settings settings;
			
			private JSONStorableBool active;
			private JSONStorableFloat contribution;
			private JSONStorableBool bilinearFiltering;			
			private JSONStorableUrl lookUpTable;
			
			private Texture2D texture;
			
			public override void Init()
			{
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.userLut;
				
				Utils.SetupInfoText(this, 
					"<b>Contribution:</b> Blending factor.\n\n" + 					
					"<b>Bilinear Filtering:</b> Switch between Point and Bilinear filter modes.\n\n" +
					"<b>UserLUT:</b> Custom color lookup texture in strip format, e.g. 256x16 or 1024x32.\n\n" +
					"UserLUT is a simple method of color grading where pixels on screen are replaced by new values from an LUT (or look-up texture) supplied by the user. To create an LUT import one of the neutral LUTs into an image editing tool such as Photoshop with a screenshot of your scene. Apply color corrections in a non destructive manner on top of these two images until you are happy with the result. Note that only pixel-local effects are supported by LUTs, meaning no blur and other effects that depends on the value of neighboring pixels. Now export the LUT with these color changes applied back into VaM to be used in the UserLUT effect.",
					800.0f, true
				);
				
				active = Utils.SetupToggle(this, "UserLUT Enabled", false, false);
				active.setCallbackFunction  += (bool v) => { model.enabled = enabledJSON.val = v; };
				
				contribution = Utils.SetupSliderFloat(this, "Contribution", 1.0f, 0.0f, 1.0f, false);
				contribution.setCallbackFunction  += (float v) => { settings.contribution = v; model.settings = settings; };
				
				bilinearFiltering = Utils.SetupToggle(this, "Bilinear Filtering", true, false);
				bilinearFiltering.setCallbackFunction  += (bool v) => { lookUpTable.setCallbackFunction(lookUpTable.val); };
										
				string defaultDir = "Custom/Assets/MacGruber/PostMagic/";
				string defaultTex = defaultDir + "LUT32/NeutralLUT32.png";
				TextureSettings textureSettings = new TextureSettings() { createMipMaps=false, linearColor=true, wrapMode=TextureWrapMode.Clamp, anisoLevel=0 };
				lookUpTable = Utils.SetupTexture2DChooser(this, "UserLUT", defaultTex, defaultDir, false, textureSettings, TextureLoaded);
				
				settings = UserLutModel.Settings.defaultSettings;
				settings.contribution = contribution.val;
				model.settings = settings;
				
				lookUpTable.setCallbackFunction(lookUpTable.val); // load texture
								
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
				settings.lut = texture = tex;
				if (tex != null)
					tex.filterMode = bilinearFiltering.val ? FilterMode.Bilinear : FilterMode.Point;
				model.settings = settings;
			}
		}
	}
}