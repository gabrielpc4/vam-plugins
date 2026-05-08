using System.Collections;
using AssetBundles;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JSONStorableDynamic : MonoBehaviour
{
	public delegate void OnLoaded();

	public Atom containingAtom;

	public Transform prefab;

	public string sceneName;

	public string assetBundleName;

	public string assetName;

	[SerializeField]
	protected Transform instance;

	public bool nameInstance;

	public string instanceName;

	public OnLoaded onPreloadedHandlers;

	public OnLoaded onLoadedHandlers;

	public bool unloadOnDisable;

	public bool needsPostLoadJSONRestore;

	protected AsyncFlag loadFlag;

	protected bool didStartLoadFromBundleAsync;

	protected bool didRegisterBundle;

	protected AssetBundleLoadAssetOperation loadRequest;

	protected AsyncOperation async;

	public bool ready { get; protected set; }

	protected virtual void InitInstance()
	{
		instance.parent = base.transform;
		instance.localPosition = Vector3.zero;
		instance.localRotation = Quaternion.identity;
		instance.localScale = Vector3.one;
		UIConnectorMaster[] componentsInChildren = instance.GetComponentsInChildren<UIConnectorMaster>(includeInactive: true);
		UIConnectorMaster[] array = componentsInChildren;
		foreach (UIConnectorMaster uIConnectorMaster in array)
		{
			uIConnectorMaster.containingAtom = containingAtom;
		}
	}

	protected void RegisterDynamicObjs()
	{
		if (!(containingAtom != null) || !(instance != null))
		{
			return;
		}
		JSONStorable[] componentsInChildren = instance.GetComponentsInChildren<JSONStorable>(includeInactive: true);
		JSONStorable[] array = componentsInChildren;
		foreach (JSONStorable jSONStorable in array)
		{
			if (!jSONStorable.exclude)
			{
				containingAtom.RegisterAdditionalStorable(jSONStorable);
			}
		}
		Canvas[] componentsInChildren2 = instance.GetComponentsInChildren<Canvas>();
		Canvas[] array2 = componentsInChildren2;
		foreach (Canvas c in array2)
		{
			containingAtom.AddCanvas(c);
		}
		PhysicsSimulator[] componentsInChildren3 = instance.GetComponentsInChildren<PhysicsSimulator>(includeInactive: true);
		PhysicsSimulator[] array3 = componentsInChildren3;
		foreach (PhysicsSimulator ps in array3)
		{
			containingAtom.RegisterDynamicPhysicsSimulator(ps);
		}
		ScaleChangeReceiver[] componentsInChildren4 = instance.GetComponentsInChildren<ScaleChangeReceiver>(includeInactive: true);
		ScaleChangeReceiver[] array4 = componentsInChildren4;
		foreach (ScaleChangeReceiver scr in array4)
		{
			containingAtom.RegisterDynamicScaleChangeReceiver(scr);
		}
		ScaleChangeReceiverJSONStorable[] componentsInChildren5 = instance.GetComponentsInChildren<ScaleChangeReceiverJSONStorable>(includeInactive: true);
		ScaleChangeReceiverJSONStorable[] array5 = componentsInChildren5;
		foreach (ScaleChangeReceiverJSONStorable scr2 in array5)
		{
			containingAtom.RegisterDynamicScaleChangeReceiverJSONStorable(scr2);
		}
	}

	protected void UnregisterDynamicObjs()
	{
		if (!(containingAtom != null) || !(instance != null))
		{
			return;
		}
		JSONStorable[] componentsInChildren = instance.GetComponentsInChildren<JSONStorable>(includeInactive: true);
		JSONStorable[] array = componentsInChildren;
		foreach (JSONStorable jSONStorable in array)
		{
			if (!jSONStorable.exclude)
			{
				containingAtom.UnregisterAdditionalStorable(jSONStorable);
			}
		}
		Canvas[] componentsInChildren2 = instance.GetComponentsInChildren<Canvas>();
		Canvas[] array2 = componentsInChildren2;
		foreach (Canvas c in array2)
		{
			containingAtom.RemoveCanvas(c);
		}
		PhysicsSimulator[] componentsInChildren3 = instance.GetComponentsInChildren<PhysicsSimulator>(includeInactive: true);
		PhysicsSimulator[] array3 = componentsInChildren3;
		foreach (PhysicsSimulator ps in array3)
		{
			containingAtom.DeregisterDynamicPhysicsSimulator(ps);
		}
		ScaleChangeReceiver[] componentsInChildren4 = instance.GetComponentsInChildren<ScaleChangeReceiver>(includeInactive: true);
		ScaleChangeReceiver[] array4 = componentsInChildren4;
		foreach (ScaleChangeReceiver scr in array4)
		{
			containingAtom.DeregisterDynamicScaleChangeReceiver(scr);
		}
		ScaleChangeReceiverJSONStorable[] componentsInChildren5 = instance.GetComponentsInChildren<ScaleChangeReceiverJSONStorable>(includeInactive: true);
		ScaleChangeReceiverJSONStorable[] array5 = componentsInChildren5;
		foreach (ScaleChangeReceiverJSONStorable scr2 in array5)
		{
			containingAtom.DeregisterDynamicScaleChangeReceiverJSONStorable(scr2);
		}
	}

	public void PostLoadJSONRestore()
	{
		if (containingAtom != null && instance != null && needsPostLoadJSONRestore)
		{
			JSONStorable[] componentsInChildren = instance.GetComponentsInChildren<JSONStorable>(includeInactive: true);
			JSONStorable[] array = componentsInChildren;
			foreach (JSONStorable js in array)
			{
				containingAtom.RestoreFromLast(js);
			}
			needsPostLoadJSONRestore = false;
		}
	}

	protected void ClearLoadFlag()
	{
		if (loadFlag != null)
		{
			loadFlag.Raise();
			loadFlag = null;
		}
	}

	protected IEnumerator LoadFromBundleAsync()
	{
		yield return SuperController.AssetManagerReady();
		loadFlag = new AsyncFlag("Load " + assetName);
		SuperController.singleton.SetLoadingIconFlag(loadFlag);
		yield return null;
		float startTime = Time.realtimeSinceStartup;
		GameObject go = null;
		if (SuperController.singleton != null)
		{
			go = SuperController.singleton.GetCachedPrefab(assetBundleName, assetName);
			if (go != null)
			{
				didRegisterBundle = true;
			}
		}
		if (go == null)
		{
			if (loadRequest == null)
			{
				loadRequest = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
				if (loadRequest == null)
				{
					Debug.LogError("Failed to load asset " + assetName);
					yield break;
				}
			}
			didStartLoadFromBundleAsync = true;
			yield return StartCoroutine(loadRequest);
			go = loadRequest.GetAsset<GameObject>();
			loadRequest = null;
			didStartLoadFromBundleAsync = false;
			if (go != null)
			{
				if (SuperController.singleton != null)
				{
					SuperController.singleton.RegisterPrefab(assetBundleName, assetName, go);
					didRegisterBundle = true;
				}
			}
			else
			{
				Debug.LogError("Asset " + assetName + " is missing game object");
			}
		}
		if (go != null)
		{
			startTime = Time.realtimeSinceStartup;
			instance = Object.Instantiate(go.transform);
			if (nameInstance)
			{
				instance.name = instanceName;
			}
			else
			{
				instance.name = instance.name.Replace("(Clone)", string.Empty);
			}
			instance.gameObject.SetActive(value: true);
			InitInstance();
			OnPreloadComplete();
			RegisterDynamicObjs();
		}
		OnLoadComplete();
		ClearLoadFlag();
	}

	protected void LoadFromPrefab()
	{
		instance = Object.Instantiate(prefab);
		if (nameInstance)
		{
			instance.name = instanceName;
		}
		else
		{
			instance.name = instance.name.Replace("(Clone)", string.Empty);
		}
		instance.gameObject.SetActive(value: true);
		InitInstance();
		OnPreloadComplete();
		RegisterDynamicObjs();
		OnLoadComplete();
	}

	protected IEnumerator LoadFromPrefabAsync()
	{
		loadFlag = new AsyncFlag("Load " + prefab.name);
		SuperController.singleton.SetLoadingIconFlag(loadFlag);
		yield return null;
		yield return null;
		LoadFromPrefab();
		ClearLoadFlag();
	}

	private IEnumerator LoadAsync()
	{
		yield return null;
		Scene sc = SceneManager.GetSceneByName(sceneName);
		if (!sc.IsValid() || !sc.isLoaded)
		{
			async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			yield return async;
			sc = SceneManager.GetSceneByName(sceneName);
		}
		if (sc.IsValid())
		{
			GameObject[] rootGameObjects = sc.GetRootGameObjects();
			if (rootGameObjects.Length > 0)
			{
				GameObject gameObject = rootGameObjects[0];
				instance = Object.Instantiate(gameObject.transform);
				if (nameInstance)
				{
					instance.name = instanceName;
				}
				else
				{
					instance.name = instance.name.Replace("(Clone)", string.Empty);
				}
				instance.gameObject.SetActive(value: true);
				gameObject.SetActive(value: false);
				InitInstance();
				OnPreloadComplete();
				RegisterDynamicObjs();
				OnLoadComplete();
			}
		}
		else
		{
			Debug.LogError("Could not open scene " + sceneName);
		}
	}

	protected virtual void Connect()
	{
	}

	protected virtual void OnPreloadComplete()
	{
		if (onPreloadedHandlers != null)
		{
			onPreloadedHandlers();
			onPreloadedHandlers = null;
		}
	}

	protected virtual void OnLoadComplete(bool skipJSONRestore = false)
	{
		Connect();
		ready = true;
		if (onLoadedHandlers != null)
		{
			onLoadedHandlers();
			onLoadedHandlers = null;
		}
		PostLoadJSONRestore();
	}

	protected virtual void OnEnable()
	{
		if (instance == null)
		{
			if (assetBundleName != null && assetBundleName != string.Empty)
			{
				if (assetName == null || assetName == string.Empty)
				{
					assetName = base.transform.name;
				}
				if (Application.isPlaying)
				{
					StartCoroutine(LoadFromBundleAsync());
				}
			}
			else if (sceneName != null && sceneName != string.Empty)
			{
				if (Application.isPlaying)
				{
					StartCoroutine(LoadAsync());
				}
			}
			else if (prefab != null)
			{
				if (Application.isPlaying)
				{
					StartCoroutine(LoadFromPrefabAsync());
				}
				else
				{
					LoadFromPrefab();
				}
			}
			else
			{
				OnPreloadComplete();
				OnLoadComplete(skipJSONRestore: true);
			}
		}
		else
		{
			OnPreloadComplete();
			RegisterDynamicObjs();
			OnLoadComplete();
		}
	}

	protected void UnloadInstance()
	{
		if (instance != null)
		{
			Object.Destroy(instance.gameObject);
			instance = null;
			ready = false;
			if (didRegisterBundle)
			{
				didRegisterBundle = false;
				SuperController.singleton.UnregisterPrefab(assetBundleName, assetName);
			}
			else if (didStartLoadFromBundleAsync)
			{
				didStartLoadFromBundleAsync = false;
				loadRequest = null;
				AssetBundleManager.UnloadAssetBundle(assetBundleName);
			}
		}
	}

	public void UnloadIfInactive()
	{
		if (!base.gameObject.activeInHierarchy || !base.enabled)
		{
			UnloadInstance();
		}
	}

	protected virtual void OnDisable()
	{
		ClearLoadFlag();
		if (instance != null)
		{
			UnregisterDynamicObjs();
			if (!Application.isPlaying)
			{
				Object.DestroyImmediate(instance.gameObject);
				instance = null;
				ready = false;
			}
			else if (unloadOnDisable)
			{
				UnloadInstance();
			}
			if (onPreloadedHandlers != null)
			{
				onPreloadedHandlers = null;
			}
			if (onLoadedHandlers != null)
			{
				onLoadedHandlers = null;
			}
		}
	}

	protected virtual void OnDestroy()
	{
		ClearLoadFlag();
		StopAllCoroutines();
		if (didRegisterBundle)
		{
			SuperController.singleton.UnregisterPrefab(assetBundleName, assetName);
		}
		else if (didStartLoadFromBundleAsync)
		{
			AssetBundleManager.UnloadAssetBundle(assetBundleName);
		}
	}
}
