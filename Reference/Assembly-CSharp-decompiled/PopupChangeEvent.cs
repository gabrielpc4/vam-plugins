using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	public delegate void OnOpenPopup();

	public delegate void OnValueChange(string value);

	[Serializable]
	public class PopupChangeEvent : UnityEvent<string>
	{
	}

	public bool alwaysOpen;

	public bool dynamicTextSize = true;

	public int minTextSize = 14;

	public Color normalColor = Color.white;

	public Color normalBackgroundColor = Color.white;

	public Color selectColor = Color.blue;

	public Button topButton;

	public RectTransform popupPanel;

	[SerializeField]
	protected float _popupPanelHeight = 350f;

	public RectTransform buttonParent;

	public Image backgroundImage;

	public Text labelText;

	public Slider sliderControl;

	[SerializeField]
	protected bool _showSlider = true;

	public RectTransform popupButtonPrefab;

	[SerializeField]
	private RectTransform[] _popupTransforms;

	[SerializeField]
	private Button[] _popupButtons;

	[SerializeField]
	private int _numPopupValues;

	[SerializeField]
	private string[] _popupValues;

	public bool useDifferentDisplayValues;

	[SerializeField]
	private string[] _displayPopupValues;

	public float topBottomBuffer = 5f;

	private bool _visible;

	public OnOpenPopup onOpenPopupHandlers;

	public OnValueChange onValueChangeHandlers;

	public PopupChangeEvent onValueChangeUnityEvent;

	[SerializeField]
	private string _currentValue = string.Empty;

	protected int _currentValueIndex = -1;

	protected bool _currentValueChanged;

	private Vector3 popupPanelRelativePosition;

	public float popupPanelHeight
	{
		get
		{
			return _popupPanelHeight;
		}
		set
		{
			if (_popupPanelHeight == value)
			{
				return;
			}
			_popupPanelHeight = value;
			if (popupPanel != null)
			{
				bool flag = _visible;
				if (flag)
				{
					MakeInvisible();
				}
				Vector2 sizeDelta = popupPanel.sizeDelta;
				sizeDelta.y = _popupPanelHeight;
				popupPanel.sizeDelta = sizeDelta;
				if (flag)
				{
					MakeVisible();
				}
			}
		}
	}

	public bool showSlider
	{
		get
		{
			return _showSlider;
		}
		set
		{
			if (_showSlider == value)
			{
				return;
			}
			_showSlider = value;
			if (topButton != null)
			{
				RectTransform component = topButton.GetComponent<RectTransform>();
				if (component != null)
				{
					Vector2 offsetMax = component.offsetMax;
					if (_showSlider && sliderControl != null)
					{
						RectTransform component2 = sliderControl.GetComponent<RectTransform>();
						if (component2 != null)
						{
							offsetMax.y = -5f - component2.sizeDelta.y;
						}
						else
						{
							offsetMax.y = -5f;
						}
					}
					else
					{
						offsetMax.y = -5f;
					}
					component.offsetMax = offsetMax;
				}
			}
			if (sliderControl != null)
			{
				if (_showSlider)
				{
					sliderControl.gameObject.SetActive(value: true);
				}
				else
				{
					sliderControl.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public int numPopupValues
	{
		get
		{
			return _numPopupValues;
		}
		set
		{
			if (_numPopupValues == value || !(base.gameObject != null))
			{
				return;
			}
			ClearPanel();
			int num = _numPopupValues;
			_numPopupValues = value;
			string[] array = new string[_numPopupValues];
			string[] array2 = new string[_numPopupValues];
			for (int i = 0; i < _numPopupValues; i++)
			{
				if (i < num)
				{
					if (_displayPopupValues != null && i < _displayPopupValues.Length)
					{
						if (_displayPopupValues[i] == null)
						{
							array2[i] = string.Empty;
						}
						else
						{
							array2[i] = _displayPopupValues[i];
						}
					}
					if (_popupValues[i] == null)
					{
						array[i] = string.Empty;
					}
					else
					{
						array[i] = _popupValues[i];
					}
				}
				else
				{
					array[i] = string.Empty;
				}
			}
			_popupValues = array;
			_displayPopupValues = array2;
			CreatePanelButtons();
			SyncSlider();
		}
	}

	public string[] popupValues => _popupValues;

	public string[] displayPopupValues => _displayPopupValues;

	public bool visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (_visible == value)
			{
				return;
			}
			_visible = value;
			if (alwaysOpen)
			{
				_visible = true;
			}
			if (_visible)
			{
				if (onOpenPopupHandlers != null)
				{
					onOpenPopupHandlers();
				}
				MakeVisible();
			}
			else
			{
				MakeInvisible();
			}
		}
	}

	public string currentValue
	{
		get
		{
			return _currentValue;
		}
		set
		{
			if (_currentValue != value || _currentValueIndex == -1)
			{
				SetCurrentValue(value);
			}
		}
	}

	public string currentValueNoCallback
	{
		get
		{
			return _currentValue;
		}
		set
		{
			if (_currentValue != value || _currentValueIndex == -1)
			{
				SetCurrentValue(value, callBack: false);
			}
		}
	}

	public string label
	{
		get
		{
			if (labelText != null)
			{
				return labelText.text;
			}
			return null;
		}
		set
		{
			if (labelText != null)
			{
				labelText.text = value;
			}
		}
	}

	public Color labelTextColor
	{
		get
		{
			if (labelText != null)
			{
				return labelText.color;
			}
			return Color.black;
		}
		set
		{
			if (labelText != null)
			{
				labelText.color = value;
			}
		}
	}

	public void setPopupValue(int index, string text)
	{
		if (index >= 0 && index < _numPopupValues)
		{
			_popupValues[index] = text;
			SetPanelButtonText(index);
			_currentValueChanged = true;
		}
	}

	public void setDisplayPopupValue(int index, string text)
	{
		if (index >= 0 && index < _numPopupValues)
		{
			_displayPopupValues[index] = text;
			SetPanelButtonText(index);
			_currentValueChanged = true;
		}
	}

	protected void HighlightCurrentValue()
	{
		if (_popupButtons == null || _popupButtons.Length != _numPopupValues)
		{
			return;
		}
		for (int i = 0; i < _numPopupValues; i++)
		{
			if (_popupButtons[i] != null)
			{
				ColorBlock colors = _popupButtons[i].colors;
				if (_popupValues[i] == currentValue)
				{
					colors.normalColor = selectColor;
				}
				else
				{
					colors.normalColor = normalColor;
				}
				_popupButtons[i].colors = colors;
			}
		}
	}

	private void MakeInvisible()
	{
		if (popupPanel != null)
		{
			popupPanel.gameObject.SetActive(value: false);
			if (base.transform.parent != null)
			{
				popupPanel.transform.SetParent(base.transform);
			}
		}
	}

	private void MakeVisible()
	{
		if (popupPanel != null)
		{
			popupPanel.gameObject.SetActive(value: true);
			if (base.transform.parent != null)
			{
				popupPanel.transform.SetParent(base.transform.parent);
			}
		}
	}

	protected void SetCurrentValue(string value, bool callBack = true, bool forceSet = false)
	{
		if (_currentValue != value || forceSet)
		{
			_currentValue = value;
			if (popupPanel != null && !alwaysOpen)
			{
				_visible = false;
				popupPanel.gameObject.SetActive(value: false);
			}
			if (callBack && onValueChangeHandlers != null)
			{
				onValueChangeHandlers(_currentValue);
			}
			if (callBack && onValueChangeUnityEvent != null)
			{
				onValueChangeUnityEvent.Invoke(_currentValue);
			}
		}
		_currentValueChanged = true;
		_currentValueIndex = -1;
		for (int i = 0; i < _popupValues.Length; i++)
		{
			if (_currentValue == _popupValues[i])
			{
				_currentValueIndex = i;
				if (sliderControl != null)
				{
					sliderControl.value = _currentValueIndex;
				}
				break;
			}
		}
		if (!(topButton != null))
		{
			return;
		}
		Text[] componentsInChildren = topButton.GetComponentsInChildren<Text>(includeInactive: true);
		if (componentsInChildren == null)
		{
			return;
		}
		if (dynamicTextSize)
		{
			if (!componentsInChildren[0].resizeTextForBestFit)
			{
				componentsInChildren[0].resizeTextMaxSize = componentsInChildren[0].fontSize;
				componentsInChildren[0].resizeTextMinSize = minTextSize;
				componentsInChildren[0].resizeTextForBestFit = dynamicTextSize;
			}
		}
		else
		{
			componentsInChildren[0].resizeTextForBestFit = dynamicTextSize;
		}
		if (useDifferentDisplayValues && _currentValueIndex != -1)
		{
			componentsInChildren[0].text = _displayPopupValues[_currentValueIndex];
		}
		else
		{
			componentsInChildren[0].text = _currentValue;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (backgroundImage != null)
		{
			backgroundImage.color = selectColor;
		}
		else if (topButton != null)
		{
			ColorBlock colors = topButton.colors;
			colors.normalColor = selectColor;
			topButton.colors = colors;
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (backgroundImage != null)
		{
			backgroundImage.color = normalBackgroundColor;
		}
		else if (topButton != null)
		{
			ColorBlock colors = topButton.colors;
			colors.normalColor = normalColor;
			topButton.colors = colors;
		}
	}

	public void SetPreviousValue()
	{
		if (_popupValues != null && _currentValueIndex > 0)
		{
			currentValue = _popupValues[_currentValueIndex - 1];
		}
	}

	public void SetNextValue()
	{
		if (_popupValues != null && _currentValueIndex < _popupValues.Length - 1)
		{
			currentValue = _popupValues[_currentValueIndex + 1];
		}
	}

	public void Toggle()
	{
		visible = !visible;
	}

	private void ClearPanel()
	{
		RectTransform rectTransform = ((!(buttonParent != null)) ? popupPanel : buttonParent);
		if (rectTransform != null)
		{
			List<GameObject> list = new List<GameObject>();
			foreach (RectTransform item in rectTransform)
			{
				if (!popupButtonPrefab || !(popupButtonPrefab.gameObject == item.gameObject))
				{
					list.Add(item.gameObject);
				}
			}
			foreach (GameObject item2 in list)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(item2);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(item2);
				}
			}
		}
		_popupTransforms = null;
		_popupButtons = null;
	}

	private void SetPanelButtonText(int index)
	{
		string text = _popupValues[index];
		RectTransform rectTransform = _popupTransforms[index];
		if (!(rectTransform != null))
		{
			return;
		}
		Button component = rectTransform.GetComponent<Button>();
		if (component != null)
		{
			_popupButtons[index] = component;
			string popupValueCopy = string.Copy(text);
			component.onClick.RemoveAllListeners();
			component.onClick.AddListener(delegate
			{
				SetCurrentValue(popupValueCopy, callBack: true, forceSet: true);
			});
		}
		Text[] componentsInChildren = component.GetComponentsInChildren<Text>(includeInactive: true);
		if (componentsInChildren == null)
		{
			return;
		}
		if (dynamicTextSize)
		{
			if (!componentsInChildren[0].resizeTextForBestFit)
			{
				componentsInChildren[0].resizeTextMaxSize = componentsInChildren[0].fontSize;
				componentsInChildren[0].resizeTextMinSize = minTextSize;
				componentsInChildren[0].resizeTextForBestFit = dynamicTextSize;
			}
		}
		else
		{
			componentsInChildren[0].resizeTextForBestFit = dynamicTextSize;
		}
		if (useDifferentDisplayValues)
		{
			componentsInChildren[0].text = _displayPopupValues[index];
		}
		else
		{
			componentsInChildren[0].text = text;
		}
	}

	private void CreatePanelButtons()
	{
		if (_popupValues == null || !(popupButtonPrefab != null) || !(base.gameObject != null))
		{
			return;
		}
		_popupTransforms = new RectTransform[_numPopupValues];
		_popupButtons = new Button[_numPopupValues];
		int num = 1;
		float num2 = 2f * topBottomBuffer;
		float num3 = num2;
		for (int i = 0; i < _numPopupValues; i++)
		{
			RectTransform rectTransform = UnityEngine.Object.Instantiate(popupButtonPrefab);
			_popupTransforms[i] = rectTransform;
			rectTransform.gameObject.SetActive(value: true);
			SetPanelButtonText(i);
			float height = rectTransform.rect.height;
			num2 += height;
			num3 += height;
			if (buttonParent != null)
			{
				rectTransform.SetParent(buttonParent, worldPositionStays: false);
			}
			else
			{
				rectTransform.SetParent(popupPanel, worldPositionStays: false);
			}
			Vector2 zero = Vector2.zero;
			zero.y = (float)num * (0f - height) + height * 0.5f - topBottomBuffer;
			rectTransform.anchoredPosition = zero;
			num++;
		}
		if (topButton != null)
		{
			RectTransform component = topButton.GetComponent<RectTransform>();
			if (component != null)
			{
				num2 += component.rect.height;
			}
		}
		if (buttonParent == null)
		{
			RectTransform component2 = GetComponent<RectTransform>();
			if (component2 != null)
			{
				Vector2 sizeDelta = component2.sizeDelta;
				sizeDelta.y = num2;
				component2.sizeDelta = sizeDelta;
			}
		}
		else
		{
			Vector2 sizeDelta2 = buttonParent.sizeDelta;
			sizeDelta2.y = num3;
			buttonParent.sizeDelta = sizeDelta2;
		}
		HighlightCurrentValue();
	}

	private void SyncSlider()
	{
		if (sliderControl != null)
		{
			sliderControl.minValue = 0f;
			sliderControl.maxValue = Mathf.Max(_numPopupValues - 1, 0f);
		}
	}

	private void InitSlider()
	{
		if (!(sliderControl != null))
		{
			return;
		}
		sliderControl.minValue = 0f;
		sliderControl.maxValue = Mathf.Max(_numPopupValues - 1, 0f);
		sliderControl.onValueChanged.AddListener(delegate
		{
			int num = Mathf.FloorToInt(sliderControl.value);
			if (num >= 0 && num < _numPopupValues)
			{
				currentValue = _popupValues[num];
			}
		});
	}

	private void TestDelegate(string test)
	{
		Debug.Log("TestDelegate on " + base.name + " called with " + test);
	}

	private void Start()
	{
		if (popupPanel != null)
		{
			popupPanelRelativePosition = popupPanel.localPosition;
		}
		if (alwaysOpen)
		{
			_visible = true;
			MakeVisible();
		}
		else
		{
			_visible = false;
			if (popupPanel != null)
			{
				popupPanel.gameObject.SetActive(value: false);
			}
		}
		ClearPanel();
		CreatePanelButtons();
		InitSlider();
	}

	private void Update()
	{
		if (_currentValueChanged)
		{
			HighlightCurrentValue();
		}
		if (popupPanel != null)
		{
			popupPanel.position = base.transform.localToWorldMatrix.MultiplyPoint3x4(popupPanelRelativePosition);
		}
		_currentValueChanged = false;
	}
}
