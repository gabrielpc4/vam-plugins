using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace MeshVR;

public class PresetManagerControl : JSONStorable
{
	protected PresetManager pm;

	public bool setMaterialOptionsTexturePaths = true;

	protected bool forceLoadOnPresetBrowsePathSync;

	protected JSONStorableUrl presetBrowsePathJSON;

	protected JSONStorableString presetNameJSON;

	protected JSONStorableString loadPresetWithNameJSON;

	protected JSONStorableActionPresetFilePath loadPresetWithPathJSON;

	protected JSONStorableUrl loadPresetWithPathUrlJSON;

	protected JSONStorableString storePresetWithNameJSON;

	protected JSONStorableBool favoriteJSON;

	protected JSONStorableAction storePresetAction;

	protected JSONStorableAction storePresetWithScreenshotAction;

	protected JSONStorableBool storeOptionalJSON;

	protected JSONStorableBool storePresetBinaryJSON;

	protected bool isLoadingPreset;

	protected JSONStorableAction loadPresetAction;

	protected JSONStorableAction loadDefaultsAction;

	protected Text statusText;

	protected Text statusTextAlt;

	protected JSONStorableBool loadPresetOnSelectJSON;

	protected JSONStorableStringChooser favoriteSelectionJSON;

	protected JSONStorableBool includeOptionalJSON;

	protected JSONStorableBool includeAppearanceJSON;

	protected JSONStorableBool includePhysicalJSON;

	protected void SyncPresetBrowsePath(string url)
	{
		if (!(pm != null) || url == null || !(url != string.Empty))
		{
			return;
		}
		string presetNameFromFilePath = pm.GetPresetNameFromFilePath(url);
		if (presetNameFromFilePath != null)
		{
			presetNameJSON.val = presetNameFromFilePath;
			if ((forceLoadOnPresetBrowsePathSync || (loadPresetOnSelectJSON != null && loadPresetOnSelectJSON.val)) && pm.CheckPresetExistance())
			{
				LoadPreset();
				presetNameJSON.val = presetNameFromFilePath;
			}
		}
	}

	protected void SyncPresetLoadButton()
	{
		if (pm.CheckPresetExistance())
		{
			if (loadPresetAction != null && loadPresetAction.dynamicButton != null && loadPresetAction.dynamicButton.button != null)
			{
				loadPresetAction.dynamicButton.button.interactable = true;
			}
		}
		else if (loadPresetAction != null && loadPresetAction.dynamicButton != null && loadPresetAction.dynamicButton.button != null)
		{
			loadPresetAction.dynamicButton.button.interactable = false;
		}
	}

	protected void SyncPresetStoreButton()
	{
		if (pm.CheckPresetReadyForStore())
		{
			if (pm.CheckPresetExistance())
			{
				if (storePresetAction != null && storePresetAction.dynamicButton != null)
				{
					storePresetAction.dynamicButton.buttonColor = Color.red;
					if (storePresetAction.dynamicButton.button != null)
					{
						storePresetAction.dynamicButton.button.interactable = true;
					}
					if (storePresetAction.dynamicButton.buttonText != null)
					{
						storePresetAction.dynamicButton.buttonText.text = "Overwrite Preset";
					}
				}
				if (storePresetWithScreenshotAction != null && storePresetWithScreenshotAction.dynamicButton != null)
				{
					storePresetWithScreenshotAction.dynamicButton.buttonColor = Color.red;
					if (storePresetWithScreenshotAction.dynamicButton.button != null)
					{
						storePresetWithScreenshotAction.dynamicButton.button.interactable = true;
					}
					if (storePresetWithScreenshotAction.dynamicButton.buttonText != null)
					{
						storePresetWithScreenshotAction.dynamicButton.buttonText.text = "Overwrite Preset";
					}
				}
				return;
			}
			if (storePresetAction != null && storePresetAction.dynamicButton != null)
			{
				storePresetAction.dynamicButton.buttonColor = Color.green;
				if (storePresetAction.dynamicButton.button != null)
				{
					storePresetAction.dynamicButton.button.interactable = true;
				}
				if (storePresetAction.dynamicButton.buttonText != null)
				{
					storePresetAction.dynamicButton.buttonText.text = "Create New Preset";
				}
			}
			if (storePresetWithScreenshotAction != null && storePresetWithScreenshotAction.dynamicButton != null)
			{
				storePresetWithScreenshotAction.dynamicButton.buttonColor = Color.green;
				if (storePresetWithScreenshotAction.dynamicButton.button != null)
				{
					storePresetWithScreenshotAction.dynamicButton.button.interactable = true;
				}
				if (storePresetWithScreenshotAction.dynamicButton.buttonText != null)
				{
					storePresetWithScreenshotAction.dynamicButton.buttonText.text = "Create New Preset";
				}
			}
			return;
		}
		bool flag = pm.IsPresetInPackage();
		if (storePresetAction != null && storePresetAction.dynamicButton != null)
		{
			storePresetAction.dynamicButton.buttonColor = Color.gray;
			if (storePresetAction.dynamicButton.button != null)
			{
				storePresetAction.dynamicButton.button.interactable = false;
			}
			if (storePresetAction.dynamicButton.buttonText != null)
			{
				if (flag)
				{
					storePresetAction.dynamicButton.buttonText.text = "Unavailable...Package Preset";
				}
				else
				{
					storePresetAction.dynamicButton.buttonText.text = "Create New Preset";
				}
			}
		}
		if (storePresetWithScreenshotAction == null || !(storePresetWithScreenshotAction.dynamicButton != null))
		{
			return;
		}
		storePresetWithScreenshotAction.dynamicButton.buttonColor = Color.gray;
		if (storePresetWithScreenshotAction.dynamicButton.button != null)
		{
			storePresetWithScreenshotAction.dynamicButton.button.interactable = false;
		}
		if (storePresetWithScreenshotAction.dynamicButton.buttonText != null)
		{
			if (flag)
			{
				storePresetWithScreenshotAction.dynamicButton.buttonText.text = "Unavailable...Package Preset";
			}
			else
			{
				storePresetWithScreenshotAction.dynamicButton.buttonText.text = "Create New Preset";
			}
		}
	}

	protected void SyncPresetName(string s)
	{
		if (pm != null)
		{
			if (s.Contains("\\"))
			{
				presetNameJSON.val = s.Replace("\\", "/");
				return;
			}
			pm.presetName = s;
			favoriteJSON.valNoCallback = pm.IsFavorite();
			SyncPresetLoadButton();
			SyncPresetStoreButton();
		}
	}

	protected void SyncLoadPresetWithName(string s)
	{
		if (pm != null)
		{
			loadPresetWithNameJSON.valNoCallback = string.Empty;
			pm.presetName = s;
			favoriteJSON.valNoCallback = pm.IsFavorite();
			SyncPresetLoadButton();
			SyncPresetStoreButton();
			LoadPreset();
		}
	}

	protected void LoadPresetWithPath(string p)
	{
		forceLoadOnPresetBrowsePathSync = true;
		presetBrowsePathJSON.SetFilePath(p);
		forceLoadOnPresetBrowsePathSync = false;
	}

	protected void SyncStorePresetWithName(string s)
	{
		if (pm != null)
		{
			storePresetWithNameJSON.valNoCallback = string.Empty;
			pm.presetName = s;
			favoriteJSON.valNoCallback = pm.IsFavorite();
			SyncPresetLoadButton();
			SyncPresetStoreButton();
			StorePreset();
		}
	}

	protected void SyncFavorite(bool b)
	{
		pm.SetFavorite(b);
		RefreshFavoriteNames();
	}

	protected virtual void StorePreset()
	{
		if (pm != null)
		{
			if (pm.StorePreset())
			{
				SyncFavorite(favoriteJSON.val);
				SyncPresetUI();
				SetStatus("Stored preset " + pm.presetName);
			}
			else
			{
				SetStatus("Failed to store preset " + pm.presetName);
			}
		}
	}

	protected virtual void StorePresetWithScreenshot()
	{
		if (pm != null)
		{
			if (pm.StorePreset(doScreenshot: true))
			{
				SyncFavorite(favoriteJSON.val);
				SyncPresetUI();
				SetStatus("Stored preset " + pm.presetName);
			}
			else
			{
				SetStatus("Failed to store preset " + pm.presetName);
			}
		}
	}

	protected void SyncStoreOptional(bool b)
	{
		if (pm != null)
		{
			pm.storeOptionalStorables = b;
		}
	}

	protected void SyncStorePresetBinary(bool b)
	{
		if (pm != null)
		{
			pm.storePresetBinary = b;
		}
	}

	protected virtual void LoadPreset()
	{
		if (!(pm != null))
		{
			return;
		}
		isLoadingPreset = true;
		if (pm.LoadPresetPre())
		{
			if (pm.itemType == PresetManager.ItemType.Atom && containingAtom != null)
			{
				containingAtom.SetLastRestoredData(pm.lastLoadedJSON, pm.includeAppearance, pm.includePhysical);
			}
			if (pm.LoadPresetPost())
			{
				SetStatus("Loaded preset " + pm.presetName);
			}
			else
			{
				SetStatus("Failed to load preset " + pm.presetName);
			}
		}
		else
		{
			SetStatus("Failed to load preset " + pm.presetName);
		}
		isLoadingPreset = false;
	}

	protected virtual void LoadDefaults()
	{
		if (!(pm != null))
		{
			return;
		}
		isLoadingPreset = true;
		if (pm.LoadDefaultsPre())
		{
			if (pm.itemType == PresetManager.ItemType.Atom && containingAtom != null)
			{
				containingAtom.SetLastRestoredData(pm.lastLoadedJSON, pm.includeAppearance, pm.includePhysical);
			}
			if (pm.LoadPresetPost())
			{
				pm.RestorePresetBinary();
				SetStatus("Loaded defaults");
			}
			else
			{
				SetStatus("Failed to load defaults");
			}
		}
		else
		{
			SetStatus("Failed to load defaults");
		}
		isLoadingPreset = false;
	}

	protected virtual void SetStatus(string status)
	{
		if (statusText != null)
		{
			statusText.text = status;
		}
		if (statusTextAlt != null)
		{
			statusTextAlt.text = status;
		}
	}

	protected void SyncFavoriteSelection(string s)
	{
		favoriteSelectionJSON.valNoCallback = string.Empty;
		presetNameJSON.val = s;
		if (pm != null && loadPresetOnSelectJSON != null && loadPresetOnSelectJSON.val && pm.CheckPresetExistance())
		{
			LoadPreset();
			presetNameJSON.val = s;
		}
	}

	protected void RefreshFavoriteNames()
	{
		if (pm != null && favoriteSelectionJSON != null)
		{
			favoriteSelectionJSON.choices = pm.FindFavoriteNames();
		}
	}

	protected void SyncIncludeOptional(bool b)
	{
		if (pm != null)
		{
			pm.includeOptional = b;
		}
	}

	protected void SyncIncludeAppearance(bool b)
	{
		if (pm != null)
		{
			pm.includeAppearance = b;
		}
	}

	protected void SyncIncludePhysical(bool b)
	{
		if (pm != null)
		{
			pm.includePhysical = b;
		}
	}

	protected void BeginBrowse(JSONStorableUrl jsurl)
	{
		if (pm != null)
		{
			jsurl.fileRemovePrefix = pm.storeName + "_";
			pm.CreateStoreFolderPath();
			string storeFolderPath = pm.GetStoreFolderPath(includePackage: false);
			string input = storeFolderPath;
			input = Regex.Replace(input, "/$", string.Empty);
			jsurl.suggestedPath = input;
			List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(storeFolderPath, allowNavigationAboveRegularDirectories: false, useFullPaths: false, generateAllFlattenedShortcut: true, includeRegularDirsInFlattenedShortcut: true);
			jsurl.shortCuts = shortCutsForDirectory;
		}
	}

	public void SyncPresetUI()
	{
		RefreshFavoriteNames();
		SyncPresetLoadButton();
		SyncPresetStoreButton();
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		base.InitUI(t, isAlt);
		if (!(t != null))
		{
			return;
		}
		PresetManagerControlUI componentInChildren = t.GetComponentInChildren<PresetManagerControlUI>();
		if (pm != null && componentInChildren != null)
		{
			presetBrowsePathJSON.RegisterFileBrowseButton(componentInChildren.browsePresetsButton, isAlt);
			presetNameJSON.RegisterInputField(componentInChildren.presetNameField, isAlt);
			favoriteJSON.RegisterToggle(componentInChildren.favoriteToggle, isAlt);
			storePresetAction.RegisterButton(componentInChildren.storePresetButton, isAlt);
			storePresetWithScreenshotAction.RegisterButton(componentInChildren.storePresetWithScreenshotButton, isAlt);
			storeOptionalJSON.RegisterToggle(componentInChildren.storeOptionalToggle, isAlt);
			storePresetBinaryJSON.RegisterToggle(componentInChildren.storePresetBinaryToggle, isAlt);
			loadPresetAction.RegisterButton(componentInChildren.loadPresetButton, isAlt);
			loadDefaultsAction.RegisterButton(componentInChildren.loadDefaultsButton, isAlt);
			if (isAlt)
			{
				statusTextAlt = componentInChildren.statusText;
			}
			else
			{
				statusText = componentInChildren.statusText;
			}
			SyncPresetLoadButton();
			SyncPresetStoreButton();
			loadPresetOnSelectJSON.RegisterToggle(componentInChildren.loadPresetOnSelectToggle, isAlt);
			favoriteSelectionJSON.RegisterPopup(componentInChildren.favoriteSelectionPopup, isAlt);
			includeOptionalJSON.RegisterToggle(componentInChildren.includeOptionalToggle, isAlt);
			includeAppearanceJSON.RegisterToggle(componentInChildren.includeAppearanceToggle, isAlt);
			includePhysicalJSON.RegisterToggle(componentInChildren.includePhysicalToggle, isAlt);
		}
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		presetNameJSON.val = FileManager.NormalizeID(presetNameJSON.val);
		if (pm != null && !isLoadingPreset)
		{
			pm.RestorePresetBinary();
		}
	}

	protected virtual void Init()
	{
		pm = GetComponent<PresetManager>();
		if (!(pm != null))
		{
			return;
		}
		string text = string.Empty;
		if (pm.itemType == PresetManager.ItemType.Atom && containingAtom != null)
		{
			pm.customPath = containingAtom.type + "/";
			text = "Textures";
		}
		string storeFolderPath = pm.GetStoreFolderPath();
		if (storeFolderPath != null)
		{
			Directory.CreateDirectory(storeFolderPath);
			if (setMaterialOptionsTexturePaths)
			{
				MaterialOptions[] componentsInChildren = GetComponentsInChildren<MaterialOptions>(includeInactive: true);
				if (componentsInChildren != null && componentsInChildren.Length > 0)
				{
					string text2 = storeFolderPath + text;
					Directory.CreateDirectory(text2);
					MaterialOptions[] array = componentsInChildren;
					foreach (MaterialOptions materialOptions in array)
					{
						materialOptions.SetCustomTextureFolder(text2);
					}
				}
			}
		}
		presetBrowsePathJSON = new JSONStorableUrl("presetBrowsePath", string.Empty, SyncPresetBrowsePath, "vap", pm.GetStoreFolderPath(), forceCallbackOnSet: true);
		presetBrowsePathJSON.beginBrowseWithObjectCallback = BeginBrowse;
		presetBrowsePathJSON.allowFullComputerBrowse = false;
		presetBrowsePathJSON.allowBrowseAboveSuggestedPath = false;
		presetBrowsePathJSON.hideExtension = true;
		presetBrowsePathJSON.fileRemovePrefix = pm.storeName + "_";
		presetBrowsePathJSON.showDirs = true;
		presetBrowsePathJSON.isStorable = false;
		presetBrowsePathJSON.isRestorable = false;
		RegisterUrl(presetBrowsePathJSON);
		presetNameJSON = new JSONStorableString("presetName", string.Empty, SyncPresetName);
		presetNameJSON.isStorable = true;
		presetNameJSON.isRestorable = true;
		presetNameJSON.enableOnChange = true;
		RegisterString(presetNameJSON);
		favoriteJSON = new JSONStorableBool("favorite", startingValue: false, SyncFavorite);
		favoriteJSON.isStorable = false;
		favoriteJSON.isRestorable = false;
		RegisterBool(favoriteJSON);
		storePresetAction = new JSONStorableAction("StorePreset", StorePreset);
		RegisterAction(storePresetAction);
		storePresetWithScreenshotAction = new JSONStorableAction("StorePresetWithScreenshot", StorePresetWithScreenshot);
		RegisterAction(storePresetWithScreenshotAction);
		storePresetWithNameJSON = new JSONStorableString("StorePresetWithName", string.Empty, SyncStorePresetWithName);
		storePresetWithNameJSON.isStorable = false;
		storePresetWithNameJSON.isRestorable = false;
		RegisterString(storePresetWithNameJSON);
		storeOptionalJSON = new JSONStorableBool("storeOptional", pm.storeOptionalStorables, SyncStoreOptional);
		storeOptionalJSON.isStorable = false;
		storeOptionalJSON.isRestorable = false;
		RegisterBool(storeOptionalJSON);
		storePresetBinaryJSON = new JSONStorableBool("storeBinary", pm.storePresetBinary, SyncStorePresetBinary);
		storePresetBinaryJSON.isStorable = false;
		storePresetBinaryJSON.isRestorable = false;
		RegisterBool(storePresetBinaryJSON);
		loadPresetAction = new JSONStorableAction("LoadPreset", LoadPreset);
		RegisterAction(loadPresetAction);
		loadPresetWithPathUrlJSON = new JSONStorableUrl("loadPresetWithPathUrl", string.Empty, "vap", pm.GetStoreFolderPath());
		loadPresetWithPathUrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
		loadPresetWithPathUrlJSON.allowFullComputerBrowse = false;
		loadPresetWithPathUrlJSON.allowBrowseAboveSuggestedPath = false;
		loadPresetWithPathUrlJSON.hideExtension = true;
		loadPresetWithPathUrlJSON.fileRemovePrefix = pm.storeName + "_";
		loadPresetWithPathUrlJSON.showDirs = true;
		loadPresetWithPathJSON = new JSONStorableActionPresetFilePath("LoadPresetWithPath", LoadPresetWithPath, loadPresetWithPathUrlJSON);
		RegisterPresetFilePathAction(loadPresetWithPathJSON);
		loadPresetWithNameJSON = new JSONStorableString("LoadPresetWithName", string.Empty, SyncLoadPresetWithName);
		loadPresetWithNameJSON.isStorable = false;
		loadPresetWithNameJSON.isRestorable = false;
		RegisterString(loadPresetWithNameJSON);
		loadDefaultsAction = new JSONStorableAction("LoadDefaults", LoadDefaults);
		RegisterAction(loadDefaultsAction);
		loadPresetOnSelectJSON = new JSONStorableBool("loadPresetOnSelect", startingValue: true);
		loadPresetOnSelectJSON.isStorable = false;
		loadPresetOnSelectJSON.isRestorable = false;
		RegisterBool(loadPresetOnSelectJSON);
		favoriteSelectionJSON = new JSONStorableStringChooser("favoriteSelection", null, string.Empty, "Favorite Selection", SyncFavoriteSelection);
		favoriteSelectionJSON.isStorable = false;
		favoriteSelectionJSON.isRestorable = false;
		includeOptionalJSON = new JSONStorableBool("includeOptional", pm.includeOptional, SyncIncludeOptional);
		includeOptionalJSON.isStorable = false;
		includeOptionalJSON.isRestorable = false;
		includeAppearanceJSON = new JSONStorableBool("includeAppearance", pm.includeAppearance, SyncIncludeAppearance);
		includeAppearanceJSON.isStorable = false;
		includeAppearanceJSON.isRestorable = false;
		includePhysicalJSON = new JSONStorableBool("includePhysical", pm.includePhysical, SyncIncludePhysical);
		includePhysicalJSON.isStorable = false;
		includePhysicalJSON.isRestorable = false;
		RefreshFavoriteNames();
		RegisterStringChooser(favoriteSelectionJSON);
	}

	public override void PreRestore()
	{
		if (!isLoadingPreset)
		{
			if (presetBrowsePathJSON != null)
			{
				presetBrowsePathJSON.val = string.Empty;
			}
			if (presetNameJSON != null)
			{
				presetNameJSON.val = string.Empty;
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
