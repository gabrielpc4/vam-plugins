using UnityEngine;
using UnityEngine.UI;

public class TriggerActionDiscreteUI : TriggerActionUI
{
	public Button testButton;

	public RectTransform audioClipPopupsContainer;

	public UIPopup audioClipTypePopup;

	public UIPopup audioClipCategoryPopup;

	public UIPopup audioClipPopup;

	public Button chooseSceneFilePathButton;

	public Button choosePresetFilePathButton;

	public Text sceneFilePathText;

	public Text presetFilePathText;

	public Toggle boolValueToggle;

	public UIDynamicSlider floatValueDynamicSlider;

	public Slider floatValueSlider;

	public InputField stringValueField;

	public InputFieldAction stringValueFieldAction;

	public RectTransform colorPickerContainer;

	public HSVColorPicker colorPicker;

	public UIPopup stringChooserValuePopup;
}
