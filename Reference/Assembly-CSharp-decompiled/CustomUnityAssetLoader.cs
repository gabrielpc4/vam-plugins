using System.Collections.Generic;
using System.IO;
using DynamicCSharp;
using MeshVR;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using UnityEngine;

public class CustomUnityAssetLoader : JSONStorable
{
	public RectTransform loadingIndicator;

	public RectTransform loadingIndicatorAlt;

	protected AsyncFlag isLoadingFlag;

	protected bool _isLoading;

	protected AsyncOperation async;

	protected LightmapData[] lightmapData;

	protected GlobalLightingManager.LightProbesHolder lightProbesHolder;

	protected AssetBundle assetBundle;

	protected string assetBundleUrl;

	protected AssetLoader.AssetBundleFromFileRequest bundleRequest;

	protected AssetLoader.SceneLoadIntoTransformRequest sceneRequest;

	protected Dictionary<string, bool> assetBundleScenePaths;

	protected Dictionary<string, bool> assetBundlePrefabNames;

	protected List<string> combinedAssetNames;

	protected Canvas[] canvases;

	protected ScriptDomain domain;

	protected JSONStorableUrl assetUrlJSON;

	protected JSONStorableUrl assetDllUrlJSON;

	protected JSONStorableStringChooser assetNameJSON;

	protected JSONStorableBool importLightmapsJSON;

	protected JSONStorableBool importLightProbesJSON;

	protected JSONStorableBool registerCanvasesJSON;

	protected JSONStorableBool showCanvasesJSON;

	protected JSONStorableBool loadDllJSON;

	protected bool isLoading
	{
		get
		{
			return _isLoading;
		}
		set
		{
			if (_isLoading == value)
			{
				return;
			}
			_isLoading = value;
			if (_isLoading)
			{
				if (isLoadingFlag != null)
				{
					isLoadingFlag.Raise();
				}
				isLoadingFlag = new AsyncFlag("CustomUnityAssetLoader");
				if (SuperController.singleton != null)
				{
					SuperController.singleton.HoldLoadComplete(isLoadingFlag);
				}
			}
			else if (isLoadingFlag != null)
			{
				isLoadingFlag.Raise();
				isLoadingFlag = null;
			}
			if (loadingIndicator != null)
			{
				loadingIndicator.gameObject.SetActive(_isLoading);
			}
			if (loadingIndicatorAlt != null)
			{
				loadingIndicatorAlt.gameObject.SetActive(_isLoading);
			}
		}
	}

	protected void SceneRequestCallback(AssetLoader.SceneLoadIntoTransformRequest slr)
	{
		lightmapData = slr.lightmapData;
		lightProbesHolder = slr.lightProbesHolder;
		canvases = slr.transform.GetComponentsInChildren<Canvas>();
		if (registerCanvasesJSON.val)
		{
			Canvas[] array = canvases;
			foreach (Canvas canvas in array)
			{
				SuperController.singleton.AddCanvas(canvas);
				canvas.gameObject.SetActive(showCanvasesJSON.val);
			}
		}
		isLoading = false;
	}

	protected void SyncAssetSelectionChoices()
	{
		if (assetNameJSON != null && combinedAssetNames != null)
		{
			assetNameJSON.choices = combinedAssetNames;
		}
	}

	protected void BundleRequestCallback(AssetLoader.AssetBundleFromFileRequest abffr)
	{
		assetBundle = abffr.assetBundle;
		assetBundleUrl = abffr.path;
		isLoading = false;
		if (!(assetBundle != null))
		{
			return;
		}
		combinedAssetNames = new List<string>();
		combinedAssetNames.Add("None");
		string[] allScenePaths = assetBundle.GetAllScenePaths();
		string[] array = allScenePaths;
		foreach (string text in array)
		{
			if (!assetBundleScenePaths.ContainsKey(text))
			{
				combinedAssetNames.Add(text);
				assetBundleScenePaths.Add(text, value: true);
			}
		}
		string[] allAssetNames = assetBundle.GetAllAssetNames();
		string[] array2 = allAssetNames;
		foreach (string text2 in array2)
		{
			if (!assetBundlePrefabNames.ContainsKey(text2) && text2.EndsWith(".prefab"))
			{
				combinedAssetNames.Add(text2);
				assetBundlePrefabNames.Add(text2, value: true);
			}
		}
		SyncAssetSelectionChoices();
		if (assetNameJSON != null)
		{
			SyncAssetName(assetNameJSON.val);
		}
	}

	protected void SyncAssetUrl(string url)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		if (isLoading)
		{
			SuperController.LogError("Attempt to load new assetbundle " + url + " while another is already being loaded");
			assetUrlJSON.valNoCallback = bundleRequest.path;
			return;
		}
		ClearAssetBundle();
		if (!insideRestore)
		{
			assetNameJSON.val = "None";
		}
		if (url == null || !(url != string.Empty))
		{
			return;
		}
		isLoading = true;
		isLoadingFlag.Name = "CustomUnityAssetLoader: " + url;
		if (loadDllJSON.val)
		{
			string text = url.Replace(".scene", string.Empty);
			text = text.Replace(".assetbundle", string.Empty);
			string text2 = text + ".dll";
			if (FileManager.FileExists(text2))
			{
				if (UserPreferences.singleton == null || UserPreferences.singleton.enablePlugins)
				{
					assetDllUrlJSON.valNoCallback = text2;
					if (domain == null)
					{
						domain = ScriptDomain.CreateDomain("MVRAssets", initCompiler: true);
					}
					if (FileManager.IsFileInPackage(text2))
					{
						domain.LoadAssembly(FileManager.ReadAllBytes(text2));
					}
					else
					{
						domain.LoadAssembly(text2);
					}
				}
				else
				{
					SuperController.LogError("Attempted to load dll when plugins option is disabled. To enable, see User Preferences -> Security tab");
					SuperController.singleton.SetActiveUI("MainMenu");
					SuperController.singleton.SetMainMenuTab("TabUserPrefs");
					SuperController.singleton.SetUserPrefsTab("TabSecurity");
				}
			}
			else
			{
				assetDllUrlJSON.valNoCallback = string.Empty;
			}
		}
		bundleRequest = new AssetLoader.AssetBundleFromFileRequest
		{
			path = url,
			callback = BundleRequestCallback
		};
		AssetLoader.QueueLoadAssetBundleFromFile(bundleRequest);
	}

	protected void SyncAssetName(string assetName)
	{
		RemoveData();
		if (!(assetBundle != null))
		{
			return;
		}
		if (assetBundleScenePaths.TryGetValue(assetName, out var value))
		{
			isLoading = true;
			isLoadingFlag.Name = "CustomUnityAssetLoader: " + assetName;
			sceneRequest = new AssetLoader.SceneLoadIntoTransformRequest
			{
				scenePath = assetName,
				transform = base.transform,
				importLightmaps = importLightmapsJSON.val,
				importLightProbes = importLightProbesJSON.val,
				callback = SceneRequestCallback
			};
			AssetLoader.QueueLoadSceneIntoTransform(sceneRequest);
		}
		else
		{
			if (!assetBundlePrefabNames.TryGetValue(assetName, out value))
			{
				return;
			}
			isLoading = true;
			isLoadingFlag.Name = "CustomUnityAssetLoader: " + assetName;
			GameObject gameObject = assetBundle.LoadAsset<GameObject>(assetName);
			if (gameObject != null)
			{
				Transform transform = Object.Instantiate(gameObject.transform);
				Vector3 localPosition = transform.localPosition;
				Quaternion localRotation = transform.localRotation;
				Vector3 localScale = transform.localScale;
				transform.SetParent(base.transform);
				transform.localPosition = localPosition;
				transform.localRotation = localRotation;
				transform.localScale = localScale;
				canvases = base.transform.GetComponentsInChildren<Canvas>();
				if (registerCanvasesJSON.val)
				{
					Canvas[] array = canvases;
					foreach (Canvas canvas in array)
					{
						SuperController.singleton.AddCanvas(canvas);
						canvas.gameObject.SetActive(showCanvasesJSON.val);
					}
				}
			}
			isLoading = false;
		}
	}

	protected void RemoveData()
	{
		if (lightmapData != null)
		{
			if (GlobalLightingManager.singleton != null)
			{
				GlobalLightingManager.singleton.RemoveLightmapData(lightmapData);
			}
			lightmapData = null;
		}
		if (lightProbesHolder != null)
		{
			if (GlobalLightingManager.singleton != null)
			{
				GlobalLightingManager.singleton.RemoveLightProbesHolder(lightProbesHolder);
			}
			lightProbesHolder = null;
		}
		if (canvases != null && registerCanvasesJSON.val)
		{
			Canvas[] array = canvases;
			foreach (Canvas c in array)
			{
				SuperController.singleton.RemoveCanvas(c);
			}
		}
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in base.transform)
		{
			list.Add(item.gameObject);
		}
		foreach (GameObject item2 in list)
		{
			Object.Destroy(item2);
		}
	}

	protected void ClearAssetBundle()
	{
		if (assetBundleScenePaths != null)
		{
			assetBundleScenePaths.Clear();
		}
		if (assetBundlePrefabNames != null)
		{
			assetBundlePrefabNames.Clear();
		}
		if (assetBundle != null)
		{
			AssetLoader.DoneWithAssetBundleFromFile(assetBundleUrl);
			assetBundle = null;
			assetBundleUrl = null;
		}
		combinedAssetNames = new List<string>();
		combinedAssetNames.Add("None");
		SyncAssetSelectionChoices();
	}

	protected void SyncImportLightmaps(bool b)
	{
		if (lightmapData != null && GlobalLightingManager.singleton != null)
		{
			if (b)
			{
				GlobalLightingManager.singleton.PushLightmapData(lightmapData);
			}
			else
			{
				GlobalLightingManager.singleton.RemoveLightmapData(lightmapData);
			}
		}
	}

	protected void SyncImportLightProbes(bool b)
	{
		if (lightProbesHolder != null && GlobalLightingManager.singleton != null)
		{
			if (b)
			{
				GlobalLightingManager.singleton.PushLightProbesHolder(lightProbesHolder);
			}
			else
			{
				GlobalLightingManager.singleton.RemoveLightProbesHolder(lightProbesHolder);
			}
		}
	}

	protected void BeginBrowse(JSONStorableUrl jsurl)
	{
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory("Custom/Assets", allowNavigationAboveRegularDirectories: true, useFullPaths: true, generateAllFlattenedShortcut: true, includeRegularDirsInFlattenedShortcut: true);
		ShortCut shortCut = new ShortCut();
		shortCut.displayName = "Root";
		shortCut.path = Path.GetFullPath(".");
		shortCutsForDirectory.Insert(0, shortCut);
		jsurl.shortCuts = shortCutsForDirectory;
	}

	protected void Init()
	{
		assetUrlJSON = new JSONStorableUrl("assetUrl", string.Empty, SyncAssetUrl, "assetbundle|scene", "Custom\\Assets");
		assetUrlJSON.allowFullComputerBrowse = true;
		assetUrlJSON.allowBrowseAboveSuggestedPath = true;
		assetUrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
		assetUrlJSON.suggestedPathGroup = "UnityAsset";
		RegisterUrl(assetUrlJSON);
		assetDllUrlJSON = new JSONStorableUrl("assetDllUrl", string.Empty, "dll", "Custom\\Assets");
		assetDllUrlJSON.allowFullComputerBrowse = true;
		assetDllUrlJSON.allowBrowseAboveSuggestedPath = true;
		assetDllUrlJSON.beginBrowseWithObjectCallback = BeginBrowse;
		assetDllUrlJSON.suggestedPathGroup = "UnityAsset";
		RegisterUrl(assetDllUrlJSON);
		List<string> list = new List<string>();
		list.Add("None");
		assetNameJSON = new JSONStorableStringChooser("assetName", list, "None", "Asset", SyncAssetName);
		RegisterStringChooser(assetNameJSON);
		importLightmapsJSON = new JSONStorableBool("importLightmaps", startingValue: true, SyncImportLightmaps);
		RegisterBool(importLightmapsJSON);
		importLightProbesJSON = new JSONStorableBool("importLightProbes", startingValue: true, SyncImportLightProbes);
		RegisterBool(importLightProbesJSON);
		registerCanvasesJSON = new JSONStorableBool("registerCanvases", startingValue: true, SyncRegisterCanvases);
		RegisterBool(registerCanvasesJSON);
		showCanvasesJSON = new JSONStorableBool("showCanvases", startingValue: true, SyncShowCanvases);
		RegisterBool(showCanvasesJSON);
		loadDllJSON = new JSONStorableBool("loadDll", startingValue: true, SyncLoadDll);
		RegisterBool(loadDllJSON);
		assetBundleScenePaths = new Dictionary<string, bool>();
		assetBundlePrefabNames = new Dictionary<string, bool>();
		combinedAssetNames = new List<string>();
		combinedAssetNames.Add("None");
	}

	protected void SyncRegisterCanvases(bool b)
	{
		if (canvases == null)
		{
			return;
		}
		Canvas[] array = canvases;
		foreach (Canvas c in array)
		{
			if (b)
			{
				SuperController.singleton.AddCanvas(c);
			}
			else
			{
				SuperController.singleton.RemoveCanvas(c);
			}
		}
	}

	protected void SyncShowCanvases(bool b)
	{
		if (canvases != null)
		{
			Canvas[] array = canvases;
			foreach (Canvas canvas in array)
			{
				canvas.gameObject.SetActive(b);
			}
		}
	}

	protected void SyncLoadDll(bool b)
	{
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			CustomUnityAssetLoaderUI componentInChildren = UITransform.GetComponentInChildren<CustomUnityAssetLoaderUI>();
			if (componentInChildren != null)
			{
				assetUrlJSON.fileBrowseButton = componentInChildren.fileBrowseButton;
				assetUrlJSON.clearButton = componentInChildren.clearButton;
				assetUrlJSON.text = componentInChildren.urlText;
				importLightmapsJSON.toggle = componentInChildren.importLightmapsToggle;
				importLightProbesJSON.toggle = componentInChildren.importLightProbesToggle;
				registerCanvasesJSON.toggle = componentInChildren.registerCanvasesToggle;
				showCanvasesJSON.toggle = componentInChildren.showCanvasesToggle;
				loadDllJSON.toggle = componentInChildren.loadDllToggle;
				assetNameJSON.popup = componentInChildren.assetSelectionPopup;
				loadingIndicator = componentInChildren.loadingIndicator;
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			CustomUnityAssetLoaderUI componentInChildren = UITransformAlt.GetComponentInChildren<CustomUnityAssetLoaderUI>();
			if (componentInChildren != null)
			{
				assetUrlJSON.fileBrowseButtonAlt = componentInChildren.fileBrowseButton;
				assetUrlJSON.clearButtonAlt = componentInChildren.clearButton;
				assetUrlJSON.textAlt = componentInChildren.urlText;
				importLightmapsJSON.toggleAlt = componentInChildren.importLightmapsToggle;
				importLightProbesJSON.toggleAlt = componentInChildren.importLightProbesToggle;
				registerCanvasesJSON.toggleAlt = componentInChildren.registerCanvasesToggle;
				showCanvasesJSON.toggleAlt = componentInChildren.showCanvasesToggle;
				loadDllJSON.toggleAlt = componentInChildren.loadDllToggle;
				assetNameJSON.popupAlt = componentInChildren.assetSelectionPopup;
				loadingIndicatorAlt = componentInChildren.loadingIndicator;
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

	public override void Remove()
	{
		RemoveData();
		ClearAssetBundle();
	}
}
