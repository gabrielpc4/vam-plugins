using UnityEngine.UI;

namespace MeshVR;

public class PresetManagerControlUI : UIProvider
{
	public Button browsePresetsButton;

	public InputField presetNameField;

	public Toggle loadPresetOnSelectToggle;

	public UIPopup favoriteSelectionPopup;

	public UIDynamicButton storePresetButton;

	public UIDynamicButton storePresetWithScreenshotButton;

	public UIDynamicButton loadPresetButton;

	public UIDynamicButton loadDefaultsButton;

	public Toggle favoriteToggle;

	public Toggle storeOptionalToggle;

	public Toggle storePresetBinaryToggle;

	public Toggle includeOptionalToggle;

	public Toggle includePhysicalToggle;

	public Toggle includeAppearanceToggle;

	public Text statusText;
}
