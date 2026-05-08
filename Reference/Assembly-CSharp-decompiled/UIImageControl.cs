using UnityEngine;
using UnityEngine.UI;

public class UIImageControl : JSONStorable
{
	protected Image _image;

	protected JSONStorableFloat alphaJSON;

	protected JSONStorableColor colorJSON;

	public void SyncAlpha(float a)
	{
		if (_image != null)
		{
			Color color = _image.color;
			color.a = a;
			_image.color = color;
		}
	}

	public void SyncColor(float h, float s, float v)
	{
		if (_image != null)
		{
			Color color = HSVColorPicker.HSVToRGB(h, s, v);
			color.a = _image.color.a;
			_image.color = color;
		}
	}

	protected void Init()
	{
		_image = GetComponent<Image>();
		if (_image != null)
		{
			Color color = _image.color;
			HSVColor startingColor = HSVColorPicker.RGBToHSV(color.r, color.g, color.b);
			colorJSON = new JSONStorableColor("color", startingColor, SyncColor);
			RegisterColor(colorJSON);
			alphaJSON = new JSONStorableFloat("alpha", color.a, SyncAlpha, 0f, 1f);
			RegisterFloat(alphaJSON);
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		UIImageControlUI componentInChildren = UITransform.GetComponentInChildren<UIImageControlUI>();
		if (componentInChildren != null)
		{
			if (colorJSON != null)
			{
				colorJSON.colorPicker = componentInChildren.colorPicker;
			}
			if (alphaJSON != null)
			{
				alphaJSON.slider = componentInChildren.alphaSlider;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		UIImageControlUI componentInChildren = UITransformAlt.GetComponentInChildren<UIImageControlUI>();
		if (componentInChildren != null)
		{
			if (colorJSON != null)
			{
				colorJSON.colorPickerAlt = componentInChildren.colorPicker;
			}
			if (alphaJSON != null)
			{
				alphaJSON.sliderAlt = componentInChildren.alphaSlider;
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}
}
