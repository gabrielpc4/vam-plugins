using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

namespace MacGruber
{
	// Collection of various utility functions
    public static class Utils
    {
		// VaM Plugins can contain multiple Scripts, if you load them via a *.cslist file. This function allows you to get
		// an instance of another script within the same plugin, allowing you directly interact with it by reading/writing
		// data, calling functions, etc.
		public static T FindWithinSamePlugin<T>(MVRScript self) where T : MVRScript
		{
			int i = self.name.IndexOf('_');
			if (i < 0)
				return null;
			string prefix = self.name.Substring(0, i+1);
			string scriptName = prefix + typeof(T).FullName;	
			return self.containingAtom.GetStorableByID(scriptName) as T;
		}
		
		// Get spawned prefab from CustomUnityAsset atom. Note that these are loaded asynchronously,
		// this function returns null while the prefab is not yet there.
		public static GameObject GetCustomUnityAsset(Atom atom, string prefabName)
		{
			Transform t = atom.transform.Find("reParentObject/object/rescaleObject/"+prefabName+"(Clone)");
			if (t == null)
				return null;
			else
				return t.gameObject;
		}
		
		// ===========================================================================================
		
		// Create VaM-UI Toggle button
		public static JSONStorableBool SetupToggle(MVRScript script, string label, bool defaultValue, bool rightSide)
		{
			JSONStorableBool storable = new JSONStorableBool(label, defaultValue);
			storable.storeType = JSONStorableParam.StoreType.Full;
			script.CreateToggle(storable, rightSide);
			script.RegisterBool(storable);
			return storable;
		}
		
		// Create VaM-UI Float slider
		public static JSONStorableFloat SetupSliderFloat(MVRScript script, string label, float defaultValue, float minValue, float maxValue, bool rightSide)
		{
			JSONStorableFloat storable = new JSONStorableFloat(label, defaultValue, minValue, maxValue, true, true);
			storable.storeType = JSONStorableParam.StoreType.Full;
			script.CreateSlider(storable, rightSide);
			script.RegisterFloat(storable);
			return storable;
		}
		
		// Create VaM-UI ColorPicker
		public static JSONStorableColor SetupColor(MVRScript script, string label, Color color, bool rightSide)
		{
			HSVColor hsvColor = HSVColorPicker.RGBToHSV(color.r, color.g, color.b);
			JSONStorableColor storable = new JSONStorableColor(label, hsvColor);
			storable.storeType = JSONStorableParam.StoreType.Full;
			script.CreateColorPicker(storable, rightSide);
			script.RegisterColor(storable);
			return storable;
		}
		
		// Create VaM-UI StringChooser for Enum
		public static JSONStorableStringChooser SetupEnumChooser<TEnum>(MVRScript self, string label, TEnum defaultValue, bool rightSide, EnumSetCallback<TEnum> callback)
			where TEnum : struct, IComparable, IConvertible, IFormattable
		{
			List<string> names = Enum.GetNames(typeof(TEnum)).ToList();			
			JSONStorableStringChooser storable = new JSONStorableStringChooser(label, names, defaultValue.ToString(), label);
			storable.setCallbackFunction += (string name) => {
				TEnum v = (TEnum)Enum.Parse(typeof(TEnum), name);
				callback(v);
			};
			self.CreateScrollablePopup(storable, rightSide);
			self.RegisterStringChooser(storable);
			return storable;
		}		
		
		// Create VaM-UI TextureChooser. Note that you are responsible for destroying the texture when you don't need it anymore.
		public static JSONStorableUrl SetupTexture2DChooser(MVRScript self, string label, string defaultValue, string defaultDir, bool rightSide, TextureSettings settings, TextureSetCallback callback)
		{
			JSONStorableUrl storable = new JSONStorableUrl(label, defaultValue, (string url) => { QueueLoadTexture(url, settings, callback); }, "jpg|png|tif|tiff");
			self.RegisterUrl(storable);
			UIDynamicButton button = self.CreateButton("Browse " + label, false);			
			UIDynamicTextField textfield = self.CreateTextField(storable, false);
			textfield.UItext.alignment = TextAnchor.MiddleRight;
			textfield.UItext.horizontalOverflow = HorizontalWrapMode.Overflow;
			textfield.UItext.verticalOverflow = VerticalWrapMode.Truncate;
			LayoutElement layout = textfield.GetComponent<LayoutElement>();
			layout.preferredHeight = layout.minHeight = 35;
			textfield.height = 35;
			if (!string.IsNullOrEmpty(defaultDir))
				storable.suggestedPath = defaultDir;
			storable.RegisterFileBrowseButton(button.button);
			return storable;
		}	
			
		// Create VaM-UI InfoText field
		public static JSONStorableString SetupInfoText(MVRScript script, string text, float height, bool rightSide)
		{
			JSONStorableString storable = new JSONStorableString("Info", text);
			UIDynamic textfield = script.CreateTextField(storable, rightSide);
			textfield.height = height;
			return storable;
		}
		
		// Create VaM-UI InfoText field
		public static JSONStorableString SetupInfoOneLine(MVRScript script, string text, bool rightSide)
		{
			JSONStorableString storable = new JSONStorableString("Info", text);
			UIDynamicTextField textfield = script.CreateTextField(storable, rightSide);
			textfield.UItext.alignment = TextAnchor.LowerLeft;
			LayoutElement layout = textfield.GetComponent<LayoutElement>();
			layout.preferredHeight = layout.minHeight = 35;
			textfield.height = 35;
			return storable;
		}
		
		// Create VaM-UI button
		public static void SetupButton(MVRScript script, string label, UnityAction callback, bool rightSide)
		{
			UIDynamicButton button = script.CreateButton(label, rightSide);
			button.button.onClick.AddListener(callback);
		}
				
		// Create input action trigger
		public static JSONStorableAction SetupAction(MVRScript script, string name, JSONStorableAction.ActionCallback callback)
		{
			JSONStorableAction action = new JSONStorableAction(name, callback);
			script.RegisterAction(action);
			return action;
		}

		// ===========================================================================================

		// Helper to add a component if missing.
		public static T GetOrAddComponent<T>(Component c) where T : Component
		{
			T t = c.GetComponent<T>();
			if (t == null)
				t = c.gameObject.AddComponent<T>();
			return t;
		}
		
		// ===========================================================================================
				
		private static void QueueLoadTexture(string url, TextureSettings settings, TextureSetCallback callback)
		{ 
			if (ImageLoaderThreaded.singleton == null)
				return;
			if (string.IsNullOrEmpty(url))
				return;
			
			ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
			queuedImage.imgPath = url;
			queuedImage.forceReload = true;
			queuedImage.skipCache = true;
			queuedImage.compress = settings.compress;
			queuedImage.createMipMaps = settings.createMipMaps;
			queuedImage.isNormalMap = settings.isNormalMap;			
			queuedImage.linear = settings.linearColor;
			queuedImage.createAlphaFromGrayscale = settings.createAlphaFromGrayscale;
			queuedImage.createNormalFromBump = settings.createNormalFromBump;
			queuedImage.bumpStrength = settings.bumpStrength;
			queuedImage.isThumbnail = false;
			queuedImage.fillBackground = false;
			queuedImage.invert = false;
			queuedImage.callback = (ImageLoaderThreaded.QueuedImage qi) =>
			{
				Texture2D tex = qi.tex;
				if (tex != null)
				{
					tex.wrapMode = settings.wrapMode;
					tex.filterMode = settings.filterMode;
					tex.anisoLevel = settings.anisoLevel;
				}				
				callback(tex);
			};
			ImageLoaderThreaded.singleton.QueueImage(queuedImage);			
		}
	}
	
	public delegate void EnumSetCallback<TEnum>(TEnum v);
	public delegate void TextureSetCallback(Texture2D tex);
	
	public class TextureSettings
	{
		public bool compress = false;
		public bool createMipMaps = true;
		public bool isNormalMap = false;
		public bool linearColor = true; // Using linear or sRGB color space.
		public bool createAlphaFromGrayscale = false;
		public bool createNormalFromBump = false;
		public float bumpStrength = 1.0f;
		public TextureWrapMode wrapMode = TextureWrapMode.Repeat;
		public FilterMode filterMode = FilterMode.Trilinear;
		public int anisoLevel = 5; // 0: Forced off, 1: Off, quality setting can override, 2-9: Anisotropic filtering levels.
	}
}
