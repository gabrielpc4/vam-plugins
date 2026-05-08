using UnityEngine;
using UnityEngine.UI;

public class TriggerActionTransitionUI : TriggerActionUI
{
	public Slider testTransitionSlider;

	public Toggle startWithCurrentValToggle;

	public Slider startValSlider;

	public UIDynamicSlider startValDynamicSlider;

	public Slider endValSlider;

	public UIDynamicSlider endValDynamicSlider;

	public RectTransform startColorPickerContainer;

	public HSVColorPicker startColorPicker;

	public RectTransform endColorPickerContainer;

	public HSVColorPicker endColorPicker;
}
