using System;
using System.Collections;
using AssetBundles;
using UnityEngine;
using UnityEngine.UI;

namespace MeshVR;

public class GlobalSceneOptions : MonoBehaviour
{
	[Serializable]
	public class AssetBundleAssetNames
	{
		public string assetBundleName;

		public string assetName;

		public bool setPosition;

		public Vector3 position;

		public bool setRotation;

		public Vector3 rotationEuler;
	}

	private static GlobalSceneOptions _singleton;

	protected bool isLoading;

	public Transform atomContainer;

	public Text versionText;

	public bool disableAdvancedSceneEdit;

	public bool disableSaveSceneButton;

	public bool disableLoadSceneButton;

	public bool disableCustomUI;

	public bool disableBrowse;

	public bool disableUI;

	public bool alwaysEnablePointers;

	public bool disableNavigation;

	public bool disableVR;

	public float startingMonitorCameraFOV = 40f;

	public bool enableStartScene;

	public JSONEmbed startJSONEmbedScene;

	public bool assetManagerSimulateInEditor = true;

	public bool disableLeap;

	public bool loadPrefsFileOnStart = true;

	public bool overridePhysicsRate;

	public UserPreferences.PhysicsRate physicsRate = UserPreferences.PhysicsRate._90;

	public bool overridePhysicsUpdateCap;

	public int physicsUpdateCap = 2;

	public bool overrideSoftPhysics;

	public bool softPhysics = true;

	public bool overrideMsaaLevel;

	public int msaaLevel = 4;

	public bool overridePixelLightCount;

	public int pixelLightCount = 2;

	public bool enablePerfMonOnStart;

	public AssetBundleAssetNames[] assetsToLoadOnStart;

	public Transform[] transformsToActivateAfterAssetLoad;

	public static GlobalSceneOptions singleton => _singleton;

	public static bool IsLoading
	{
		get
		{
			if (_singleton != null)
			{
				return _singleton.isLoading;
			}
			return false;
		}
	}

	private IEnumerator LoadAssets()
	{
		AssetBundleManager.SetSourceAssetBundleDirectory(Application.streamingAssetsPath + "/");
		AssetBundleLoadManifestOperation request = AssetBundleManager.Initialize();
		if (request != null)
		{
			yield return StartCoroutine(request);
		}
		Debug.Log("Asset Manager Ready");
		AssetBundleAssetNames[] array = assetsToLoadOnStart;
		foreach (AssetBundleAssetNames assetToLoad in array)
		{
			AssetBundleLoadAssetOperation arequest = AssetBundleManager.LoadAssetAsync(assetToLoad.assetBundleName, assetToLoad.assetName, typeof(GameObject));
			if (arequest == null)
			{
				Debug.LogError("Failed to load asset " + assetToLoad.assetBundleName + " " + assetToLoad.assetName);
				continue;
			}
			yield return StartCoroutine(arequest);
			GameObject go = arequest.GetAsset<GameObject>();
			if (!(go != null))
			{
				continue;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(go);
			if (atomContainer != null)
			{
				gameObject.transform.SetParent(atomContainer, worldPositionStays: true);
				if (assetToLoad.setPosition)
				{
					gameObject.transform.position = assetToLoad.position;
				}
				if (assetToLoad.setRotation)
				{
					gameObject.transform.eulerAngles = assetToLoad.rotationEuler;
				}
				gameObject.name = assetToLoad.assetName;
			}
		}
		if (transformsToActivateAfterAssetLoad != null)
		{
			Transform[] array2 = transformsToActivateAfterAssetLoad;
			foreach (Transform transform in array2)
			{
				transform.gameObject.SetActive(value: true);
			}
		}
		isLoading = false;
	}

	private void Awake()
	{
		_singleton = this;
	}

	private void Start()
	{
		if (assetsToLoadOnStart != null && assetsToLoadOnStart.Length > 0)
		{
			isLoading = true;
			StartCoroutine(LoadAssets());
		}
	}
}
