using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZenFulcrum.EmbeddedBrowser;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(Browser))]
public class VRWebBrowser : JSONStorable, IBrowserUI, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, LookInputModule.IPointerMoveHandler, INewWindowHandler, KeyEventHandler, IScrollHandler, IEventSystemHandler
{
	public struct QuickSite
	{
		public string name;

		public string url;
	}

	protected RawImage myImage;

	protected Browser browser;

	public bool enableInput = true;

	public RawImage cursor;

	public string navigatedURL;

	public Text navigatedURLText;

	public Text navigatedURLTextAlt;

	protected List<QuickSite> quickSitesList;

	public string homeUrl;

	public string quickSitesFile;

	protected JSONStorableUrl urlJSON;

	protected string _url;

	protected JSONStorableBool fullMouseClickOnDownJSON;

	[SerializeField]
	protected bool _fullMouseClickOnDown = true;

	protected JSONStorableBool disableInteractionJSON;

	[SerializeField]
	protected bool _disableInteraction;

	public Browser.NewWindowAction newWindowAction = Browser.NewWindowAction.Ignore;

	protected List<Event> keyEvents = new List<Event>();

	protected List<Event> keyEventsLast = new List<Event>();

	protected BaseRaycaster raycaster;

	protected RectTransform rTransform;

	protected float urlUpdateTimer = 1f;

	protected bool _mouseHasFocus;

	protected bool _eventPointerDown;

	protected Vector2 _eventPosition;

	protected bool _keyboardHasFocus;

	protected Vector2 _mouseScroll;

	protected int pointerCount;

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
			else if (_url != value)
			{
				SyncUrl(_url);
			}
		}
	}

	public bool fullMouseClickOnDown
	{
		get
		{
			return _fullMouseClickOnDown;
		}
		set
		{
			if (fullMouseClickOnDownJSON != null)
			{
				fullMouseClickOnDownJSON.val = value;
			}
			else if (_fullMouseClickOnDown != value)
			{
				SyncFullMouseClickOnDown(value);
			}
		}
	}

	public bool disableInteraction
	{
		get
		{
			return _disableInteraction;
		}
		set
		{
			if (disableInteractionJSON != null)
			{
				disableInteractionJSON.val = value;
			}
			else if (_disableInteraction != value)
			{
				SyncDisableInteraction(value);
			}
		}
	}

	public bool MouseHasFocus => _mouseHasFocus && enableInput;

	public Vector2 MousePosition { get; private set; }

	public MouseButton MouseButtons { get; private set; }

	public Vector2 MouseScroll { get; private set; }

	public bool KeyboardHasFocus => _keyboardHasFocus && enableInput;

	public List<Event> KeyEvents => keyEventsLast;

	public BrowserCursor BrowserCursor { get; private set; }

	public BrowserInputSettings InputSettings { get; private set; }

	protected void SyncUrl(string u)
	{
		if (UserPreferences.singleton == null || UserPreferences.singleton.enableWebBrowser)
		{
			if (browser != null)
			{
				browser.Url = u;
			}
		}
		else if (UserPreferences.singleton == null || !UserPreferences.singleton.hideDisabledWebMessages)
		{
			SuperController.LogError("Attempted to load browser URL when web browser option is disabled. To enable, see User Preferences -> Security tab");
			SuperController.singleton.SetActiveUI("MainMenu");
			SuperController.singleton.SetMainMenuTab("TabUserPrefs");
			SuperController.singleton.SetUserPrefsTab("TabSecurity");
		}
	}

	protected void SyncJSONToBrowserURL()
	{
		if (urlJSON != null && browser != null)
		{
			urlJSON.valNoCallback = browser.Url;
		}
	}

	protected void SyncFullMouseClickOnDown(bool b)
	{
		_fullMouseClickOnDown = b;
	}

	public void SyncDisableInteraction()
	{
		IgnoreRaycast component = GetComponent<IgnoreRaycast>();
		if (_disableInteraction)
		{
			if (component == null)
			{
				base.gameObject.AddComponent<IgnoreRaycast>();
			}
		}
		else if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	protected void SyncDisableInteraction(bool b)
	{
		_disableInteraction = b;
		SyncDisableInteraction();
	}

	public Browser CreateBrowser(Browser parent)
	{
		GameObject gameObject = new GameObject("PopupBrowser");
		gameObject.transform.SetParent(base.transform);
		return gameObject.AddComponent<Browser>();
	}

	public void GoBack()
	{
		if (browser != null)
		{
			browser.GoBack();
			SyncJSONToBrowserURL();
		}
	}

	public void GoForward()
	{
		if (browser != null)
		{
			browser.GoForward();
			SyncJSONToBrowserURL();
		}
	}

	public void GoHome()
	{
		if (browser != null && homeUrl != null && homeUrl != string.Empty)
		{
			url = homeUrl;
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		VRWebBrowserUI componentInChildren = UITransform.GetComponentInChildren<VRWebBrowserUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		fullMouseClickOnDownJSON.toggle = componentInChildren.fullMouseClickOnDownToggle;
		disableInteractionJSON.toggle = componentInChildren.disableInteractionToggle;
		urlJSON.inputField = componentInChildren.urlInput;
		urlJSON.inputFieldAction = componentInChildren.urlInputAction;
		urlJSON.copyToClipboardButton = componentInChildren.copyToClipboardButton;
		urlJSON.copyFromClipboardButton = componentInChildren.copyFromClipboardButton;
		urlJSON.setValToInputFieldButton = componentInChildren.goButton;
		navigatedURLText = componentInChildren.navigatedURLText;
		if (navigatedURLText != null)
		{
			navigatedURLText.text = navigatedURL;
		}
		if (componentInChildren.backButton != null)
		{
			componentInChildren.backButton.onClick.AddListener(GoBack);
		}
		if (componentInChildren.forwardButton != null)
		{
			componentInChildren.forwardButton.onClick.AddListener(GoForward);
		}
		if (componentInChildren.quickSitesPopup != null)
		{
			componentInChildren.quickSitesPopup.useDifferentDisplayValues = true;
			int num = 0;
			if (homeUrl != null && homeUrl != string.Empty)
			{
				componentInChildren.quickSitesPopup.numPopupValues = quickSitesList.Count + 1;
				componentInChildren.quickSitesPopup.setDisplayPopupValue(0, "Home");
				componentInChildren.quickSitesPopup.setPopupValue(0, homeUrl);
				num = 1;
			}
			else
			{
				componentInChildren.quickSitesPopup.numPopupValues = quickSitesList.Count;
			}
			for (int i = 0; i < quickSitesList.Count; i++)
			{
				componentInChildren.quickSitesPopup.setDisplayPopupValue(i + num, quickSitesList[i].name);
				componentInChildren.quickSitesPopup.setPopupValue(i + num, quickSitesList[i].url);
			}
			UIPopup quickSitesPopup = componentInChildren.quickSitesPopup;
			quickSitesPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(quickSitesPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetUrl));
		}
		if (componentInChildren.homeButton != null)
		{
			componentInChildren.homeButton.onClick.AddListener(GoHome);
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		VRWebBrowserUI componentInChildren = UITransformAlt.GetComponentInChildren<VRWebBrowserUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		fullMouseClickOnDownJSON.toggleAlt = componentInChildren.fullMouseClickOnDownToggle;
		disableInteractionJSON.toggleAlt = componentInChildren.disableInteractionToggle;
		urlJSON.inputFieldAlt = componentInChildren.urlInput;
		urlJSON.inputFieldActionAlt = componentInChildren.urlInputAction;
		urlJSON.copyToClipboardButtonAlt = componentInChildren.copyToClipboardButton;
		urlJSON.copyFromClipboardButtonAlt = componentInChildren.copyFromClipboardButton;
		urlJSON.setValToInputFieldButtonAlt = componentInChildren.goButton;
		navigatedURLTextAlt = componentInChildren.navigatedURLText;
		if (navigatedURLTextAlt != null)
		{
			navigatedURLTextAlt.text = navigatedURL;
		}
		if (componentInChildren.backButton != null)
		{
			componentInChildren.backButton.onClick.AddListener(GoBack);
		}
		if (componentInChildren.forwardButton != null)
		{
			componentInChildren.forwardButton.onClick.AddListener(GoForward);
		}
		if (componentInChildren.quickSitesPopup != null)
		{
			componentInChildren.quickSitesPopup.useDifferentDisplayValues = true;
			int num = 0;
			if (homeUrl != null && homeUrl != string.Empty)
			{
				componentInChildren.quickSitesPopup.numPopupValues = quickSitesList.Count + 1;
				componentInChildren.quickSitesPopup.setDisplayPopupValue(0, "Home");
				componentInChildren.quickSitesPopup.setPopupValue(0, homeUrl);
				num = 1;
			}
			else
			{
				componentInChildren.quickSitesPopup.numPopupValues = quickSitesList.Count;
			}
			for (int i = 0; i < quickSitesList.Count; i++)
			{
				componentInChildren.quickSitesPopup.setDisplayPopupValue(i + num, quickSitesList[i].name);
				componentInChildren.quickSitesPopup.setPopupValue(i + num, quickSitesList[i].url);
			}
			UIPopup quickSitesPopup = componentInChildren.quickSitesPopup;
			quickSitesPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(quickSitesPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetUrl));
		}
		if (componentInChildren.homeButton != null)
		{
			componentInChildren.homeButton.onClick.AddListener(GoHome);
		}
	}

	protected void Init()
	{
		BrowserCursor = new BrowserCursor();
		if (cursor != null)
		{
			cursor.texture = BrowserCursor.Texture;
		}
		InputSettings = new BrowserInputSettings();
		browser = GetComponent<Browser>();
		myImage = GetComponent<RawImage>();
		fullMouseClickOnDownJSON = new JSONStorableBool("fullMouseClickOnDown", _fullMouseClickOnDown, SyncFullMouseClickOnDown);
		RegisterBool(fullMouseClickOnDownJSON);
		disableInteractionJSON = new JSONStorableBool("disableInteraction", _disableInteraction, SyncDisableInteraction);
		RegisterBool(disableInteractionJSON);
		urlJSON = new JSONStorableUrl("url", browser.Url, SyncUrl);
		urlJSON.disableOnEndEdit = true;
		RegisterUrl(urlJSON);
		SyncDisableInteraction();
		MVRDownloadManager component = GetComponent<MVRDownloadManager>();
		if (component != null)
		{
			component.ManageDownloads(browser);
		}
		if (UserPreferences.singleton == null)
		{
			browser.enabled = false;
			myImage.enabled = false;
		}
		else
		{
			browser.enabled = UserPreferences.singleton.enableWebBrowser;
			myImage.enabled = UserPreferences.singleton.enableWebBrowser;
		}
		browser.afterResize += UpdateTexture;
		browser.UIHandler = this;
		browser.SetNewWindowHandler(newWindowAction, this);
		BrowserCursor.cursorChange += delegate
		{
			SetCursor(BrowserCursor);
		};
		browser.onLoad += delegate
		{
			browser.RegisterFunction("reportImageClick", delegate(ZenFulcrum.EmbeddedBrowser.JSONNode args)
			{
				string text = args[0];
				if (Regex.IsMatch(text, "^http"))
				{
					Debug.Log("Image clicked " + text);
					GUIUtility.systemCopyBuffer = text;
				}
			});
			browser.EvalJS("\r\n\t\t\t\twindow.addEventListener('click', ev => {\r\n\t\t\t\t\tif (ev.target.tagName == 'IMG') reportImageClick(ev.target.src);\r\n\t\t\t\t});\r\n\t\t\t");
		};
		rTransform = GetComponent<RectTransform>();
		quickSitesList = new List<QuickSite>();
		if (quickSitesFile == null || !(quickSitesFile != string.Empty) || !File.Exists(quickSitesFile))
		{
			return;
		}
		try
		{
			using StreamReader streamReader = new StreamReader(quickSitesFile);
			string aJSON = streamReader.ReadToEnd();
			SimpleJSON.JSONNode jSONNode = JSON.Parse(aJSON);
			JSONArray asArray = jSONNode["sites"].AsArray;
			if (!(asArray != null))
			{
				return;
			}
			for (int i = 0; i < asArray.Count; i++)
			{
				JSONArray asArray2 = asArray[i].AsArray;
				if (asArray2 != null)
				{
					QuickSite quickSite = default(QuickSite);
					quickSite.name = asArray2[0];
					quickSite.url = asArray2[1];
					QuickSite item = quickSite;
					quickSitesList.Add(item);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during read of quick sites file " + ex);
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
		StartCoroutine(WatchResizeAndEnable());
	}

	private IEnumerator WatchResizeAndEnable()
	{
		Rect currentSize = default(Rect);
		while (base.enabled)
		{
			if (UserPreferences.singleton != null)
			{
				if (browser != null)
				{
					browser.enabled = UserPreferences.singleton.enableWebBrowser;
				}
				if (myImage != null)
				{
					myImage.enabled = UserPreferences.singleton.enableWebBrowser;
				}
			}
			Rect rect = rTransform.rect;
			if (rect.size.x <= 0f || rect.size.y <= 0f)
			{
				rect.size = new Vector2(512f, 512f);
			}
			if (rect.size != currentSize.size)
			{
				browser.Resize((int)rect.size.x, (int)rect.size.y);
				currentSize = rect;
			}
			yield return null;
		}
	}

	protected void UpdateTexture(Texture2D texture)
	{
		myImage.texture = texture;
		myImage.uvRect = new Rect(0f, 0f, 1f, 1f);
	}

	public void CopyURLToClipboard()
	{
		GUIUtility.systemCopyBuffer = url;
	}

	public void CopyURLFromClipboard()
	{
		url = GUIUtility.systemCopyBuffer;
	}

	public void SetUrl(string surl)
	{
		url = surl;
	}

	public virtual void InputUpdate()
	{
		List<Event> list = keyEvents;
		keyEvents = keyEventsLast;
		keyEventsLast = list;
		keyEvents.Clear();
		if (navigatedURLText != null)
		{
			navigatedURLText.text = browser.Url;
			SyncJSONToBrowserURL();
		}
		if (navigatedURLTextAlt != null)
		{
			navigatedURLTextAlt.text = browser.Url;
			SyncJSONToBrowserURL();
		}
		if (MouseHasFocus)
		{
			MousePosition = _eventPosition;
			MouseButton mouseButton = (MouseButton)0;
			if (_eventPointerDown)
			{
				mouseButton |= MouseButton.Left;
				if (fullMouseClickOnDown && !Input.GetMouseButton(0))
				{
					_eventPointerDown = false;
				}
			}
			MouseButtons = mouseButton;
			MouseScroll = _mouseScroll;
			_mouseScroll.x = 0f;
			_mouseScroll.y = 0f;
		}
		else
		{
			MouseButtons = (MouseButton)0;
		}
	}

	public void AddKeyEvent(Event ev)
	{
		keyEvents.Add(new Event(ev));
	}

	public void OnGUI()
	{
		Event current = Event.current;
		if (current.type == EventType.KeyDown || current.type == EventType.KeyUp)
		{
			keyEvents.Add(new Event(current));
		}
	}

	protected virtual void SetCursor(BrowserCursor newCursor)
	{
		if (cursor != null)
		{
			if (newCursor == null)
			{
				cursor.gameObject.SetActive(value: false);
				cursor.texture = null;
			}
			else
			{
				cursor.gameObject.SetActive(value: true);
				cursor.texture = newCursor.Texture;
			}
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		_keyboardHasFocus = true;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		_keyboardHasFocus = false;
	}

	public void OnScroll(PointerEventData eventData)
	{
		_mouseScroll = eventData.scrollDelta * 0.01f;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		pointerCount++;
		_mouseHasFocus = true;
		SetCursor(BrowserCursor);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		pointerCount--;
		if (pointerCount == 0)
		{
			_mouseHasFocus = false;
			_eventPointerDown = false;
			SetCursor(null);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_eventPointerDown = false;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)base.transform, eventData.position, eventData.enterEventCamera, out var localPoint))
		{
			localPoint.x = localPoint.x / rTransform.rect.width + 0.5f;
			localPoint.y = localPoint.y / rTransform.rect.height + 0.5f;
			_eventPosition = localPoint;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_eventPointerDown = true;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)base.transform, eventData.position, eventData.enterEventCamera, out var localPoint))
		{
			localPoint.x = localPoint.x / rTransform.rect.width + 0.5f;
			localPoint.y = localPoint.y / rTransform.rect.height + 0.5f;
			_eventPosition = localPoint;
		}
		if (LookInputModule.singleton != null)
		{
			LookInputModule.singleton.Select(base.gameObject);
		}
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)base.transform, eventData.position, eventData.enterEventCamera, out var localPoint))
		{
			if (cursor != null)
			{
				cursor.rectTransform.anchoredPosition = localPoint;
			}
			localPoint.x = localPoint.x / rTransform.rect.width + 0.5f;
			localPoint.y = localPoint.y / rTransform.rect.height + 0.5f;
			_eventPosition = localPoint;
		}
	}
}
