using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using OldMoatGames;
using UnityEngine;

public class ImageControl : JSONStorable
{
	protected Texture2D currentTexture;

	protected Texture2D createdTexture;

	protected Texture2D registeredTexture;

	protected bool notActiveOnSync;

	public Transform targetTransformForScale;

	protected JSONStorableUrl urlJSON;

	[SerializeField]
	protected string _url;

	protected bool _allowTiling;

	protected JSONStorableBool allowTilingJSON;

	public string url
	{
		get
		{
			return _url;
		}
		set
		{
			if (urlJSON != null)
			{
				urlJSON.val = value;
			}
			if (_url != value)
			{
				SyncUrl(value);
			}
		}
	}

	public void SyncImageRatio(Texture2D tex)
	{
		float num = 1f;
		float num2 = 1f;
		float num3 = 1f;
		if (targetTransformForScale != null)
		{
			num2 = targetTransformForScale.localScale.x;
			num3 = targetTransformForScale.localScale.y;
			num = num2 / num3;
		}
		float num4 = (float)tex.width / (float)tex.height;
		Vector3 localScale = default(Vector3);
		localScale.z = 1f;
		if (num4 > num)
		{
			localScale.x = num2;
			localScale.y = num2 / num4;
		}
		else
		{
			localScale.x = num4 * num3;
			localScale.y = num3;
		}
		base.transform.localScale = localScale;
	}

	public void SyncTexture(Texture2D tex)
	{
		SyncImageRatio(tex);
		currentTexture = tex;
		SyncAllowTiling();
		MeshRenderer[] components = GetComponents<MeshRenderer>();
		MeshRenderer[] array = components;
		foreach (MeshRenderer meshRenderer in array)
		{
			Material[] array2 = ((!Application.isPlaying) ? meshRenderer.sharedMaterials : meshRenderer.materials);
			Material material = array2[0];
			if (material.HasProperty("_MainTex"))
			{
				material.SetTexture("_MainTex", tex);
			}
			if (material.HasProperty("_SpecTex"))
			{
				material.SetTexture("_SpecTex", tex);
			}
		}
		GetComponent<Renderer>().material.mainTexture = tex;
	}

	private IEnumerator SyncImage()
	{
		Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT5, mipmap: true);
		string urltoload = _url;
		if (!Regex.IsMatch(urltoload, "^http") && !Regex.IsMatch(urltoload, "^file"))
		{
			urltoload = ((!urltoload.Contains(":/")) ? ("file:///.\\" + urltoload) : ("file:///" + urltoload));
		}
		WWW www = new WWW(urltoload);
		yield return www;
		if (www.error == null || www.error == string.Empty)
		{
			www.LoadImageIntoTexture(tex);
			if (createdTexture != null)
			{
				Object.Destroy(createdTexture);
			}
			createdTexture = tex;
			SyncTexture(tex);
		}
		else
		{
			SuperController.LogError("Could not load image at " + urltoload + " Error: " + www.error);
		}
	}

	protected void OnImageLoaded(ImageLoaderThreaded.QueuedImage qi)
	{
		if (qi.tex != null && this != null)
		{
			SyncTexture(qi.tex);
			ImageLoaderThreaded.singleton.RegisterTextureUse(qi.tex);
			if (registeredTexture != null)
			{
				ImageLoaderThreaded.singleton.DeregisterTextureUse(registeredTexture);
			}
			registeredTexture = qi.tex;
		}
	}

	public void StartSyncImage()
	{
		if (_url == null || !(_url != string.Empty))
		{
			return;
		}
		if (UserPreferences.singleton == null || UserPreferences.singleton.enableWebMisc || !Regex.IsMatch(url, "^http"))
		{
			if (base.gameObject.activeInHierarchy)
			{
				AnimatedGifPlayer component = GetComponent<AnimatedGifPlayer>();
				if (_url.EndsWith(".gif"))
				{
					if (component != null)
					{
						component.enabled = true;
						component.FileName = url;
						component.Init();
					}
				}
				else if (_url.EndsWith(".jpg") || _url.EndsWith(".jpeg") || _url.EndsWith(".png"))
				{
					if (component != null)
					{
						component.enabled = false;
					}
					if (_url.StartsWith("http") || _url.StartsWith("file") || ImageLoaderThreaded.singleton == null)
					{
						StartCoroutine(SyncImage());
						return;
					}
					ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
					queuedImage.imgPath = _url;
					queuedImage.callback = OnImageLoaded;
					ImageLoaderThreaded.singleton.QueueImage(queuedImage);
				}
				else
				{
					if (component != null)
					{
						component.enabled = false;
					}
					StartCoroutine(SyncImage());
				}
			}
			else
			{
				notActiveOnSync = true;
			}
		}
		else if (UserPreferences.singleton == null || !UserPreferences.singleton.hideDisabledWebMessages)
		{
			SuperController.LogError("Attempted to load http URL image when web load option is disabled. To enable, see User Preferences -> Web Security tab");
			SuperController.singleton.SetActiveUI("MainMenu");
			SuperController.singleton.SetMainMenuTab("TabUserPrefs");
			SuperController.singleton.SetUserPrefsTab("TabSecurity");
		}
	}

	protected void SyncUrl(string s)
	{
		_url = s;
		StartSyncImage();
	}

	protected void SyncAllowTiling()
	{
		if (currentTexture != null)
		{
			if (_allowTiling)
			{
				currentTexture.wrapMode = TextureWrapMode.Repeat;
			}
			else
			{
				currentTexture.wrapMode = TextureWrapMode.Clamp;
			}
		}
	}

	protected void SyncAllowTiling(bool b)
	{
		_allowTiling = b;
		SyncAllowTiling();
	}

	protected void BeginBrowse(JSONStorableUrl jsurl)
	{
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory("Custom/Images", allowNavigationAboveRegularDirectories: true, useFullPaths: true, generateAllFlattenedShortcut: true, includeRegularDirsInFlattenedShortcut: true);
		ShortCut shortCut = new ShortCut();
		shortCut.displayName = "Root";
		shortCut.path = Path.GetFullPath(".");
		shortCutsForDirectory.Insert(0, shortCut);
		jsurl.shortCuts = shortCutsForDirectory;
	}

	protected void Init()
	{
		if (!FileManager.DirectoryExists("Custom/Images"))
		{
			FileManager.CreateDirectory("Custom/Images");
		}
		urlJSON = new JSONStorableUrl("url", _url, SyncUrl, "jpg|jpeg|png|gif", "Custom/Images");
		urlJSON.beginBrowseWithObjectCallback = BeginBrowse;
		urlJSON.disableOnEndEdit = true;
		RegisterUrl(urlJSON);
		allowTilingJSON = new JSONStorableBool("allowTiling", _allowTiling, SyncAllowTiling);
		RegisterBool(allowTilingJSON);
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			UrlUI componentInChildren = t.GetComponentInChildren<UrlUI>();
			if (componentInChildren != null)
			{
				urlJSON.RegisterInputField(componentInChildren.urlInputField, isAlt);
				urlJSON.RegisterInputFieldAction(componentInChildren.urlInputFieldAction, isAlt);
				urlJSON.RegisterCopyToClipboardButton(componentInChildren.copyToClipboardButton, isAlt);
				urlJSON.RegisterCopyFromClipboardButton(componentInChildren.copyFromClipboardButton, isAlt);
				urlJSON.RegisterFileBrowseButton(componentInChildren.fileBrowseButton, isAlt);
				urlJSON.RegisterSetValToInputFieldButton(componentInChildren.loadButton, isAlt);
				allowTilingJSON.RegisterToggle(componentInChildren.allowImageTilingToggle, isAlt);
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

	protected void OnEnable()
	{
		if (notActiveOnSync)
		{
			notActiveOnSync = false;
			StartSyncImage();
		}
	}

	private void OnDestroy()
	{
		if (createdTexture != null)
		{
			Object.Destroy(createdTexture);
		}
		if (registeredTexture != null)
		{
			ImageLoaderThreaded.singleton.DeregisterTextureUse(registeredTexture);
		}
	}

	public void Start()
	{
		StartSyncImage();
	}
}
