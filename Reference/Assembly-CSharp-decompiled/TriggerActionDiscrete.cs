using System;
using System.Collections.Generic;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class TriggerActionDiscrete : TriggerAction
{
	public enum AudioClipType
	{
		Embedded,
		URL
	}

	protected Button testButton;

	protected bool triggerFlip = true;

	protected JSONStorable.Type _actionType;

	protected RectTransform audioClipPopupsContainer;

	protected UIPopup audioClipTypePopup;

	protected AudioClipType _audioClipType;

	protected UIPopup audioClipCategoryPopup;

	protected string _audioClipCategory;

	protected UIPopup audioClipPopup;

	protected NamedAudioClip _audioClip;

	protected Button chooseSceneFilePathButton;

	protected Text sceneFilePathText;

	protected string _sceneFilePath;

	protected Button choosePresetFilePathButton;

	protected Text presetFilePathText;

	protected string _presetFilePath;

	protected Toggle boolValueToggle;

	protected bool _boolValue;

	protected UIDynamicSlider floatValueDynamicSlider;

	protected Slider floatValueSlider;

	protected float _floatValue;

	protected InputField stringValueField;

	protected InputFieldAction stringValueFieldAction;

	protected string _stringValue;

	protected RectTransform colorPickerContainer;

	protected HSVColorPicker colorPicker;

	protected float _HSVColorH;

	protected float _HSVColorS;

	protected float _HSVColorV;

	protected UIPopup stringChooserValuePopup;

	protected string _stringChooserValue;

	protected HSVColor triggerColor;

	public JSONStorable.Type actionType
	{
		get
		{
			return _actionType;
		}
		set
		{
			if (_actionType != value)
			{
				_actionType = value;
				SyncType();
			}
		}
	}

	public AudioClipType audioClipType
	{
		get
		{
			return _audioClipType;
		}
		set
		{
			if (_audioClipType != value)
			{
				_audioClipType = value;
				if (audioClipTypePopup != null)
				{
					audioClipTypePopup.currentValue = _audioClipType.ToString();
				}
				audioClipCategory = null;
				SetClipCategoryPopupValues();
			}
		}
	}

	public string audioClipCategory
	{
		get
		{
			return _audioClipCategory;
		}
		set
		{
			if (_audioClipCategory != value)
			{
				_audioClipCategory = value;
				if (audioClipCategoryPopup != null)
				{
					audioClipCategoryPopup.currentValue = value;
				}
				audioClip = null;
				SetClipPopupValues();
			}
		}
	}

	public NamedAudioClip audioClip
	{
		get
		{
			return _audioClip;
		}
		set
		{
			if (_audioClip == value)
			{
				return;
			}
			_audioClip = value;
			if (audioClipPopup != null)
			{
				if (_audioClip == null)
				{
					audioClipPopup.currentValue = "None";
				}
				else
				{
					audioClipPopup.currentValue = _audioClip.uid;
				}
			}
		}
	}

	public string sceneFilePath
	{
		get
		{
			return _sceneFilePath;
		}
		set
		{
			if (_sceneFilePath != value)
			{
				_sceneFilePath = value;
				if (sceneFilePathText != null)
				{
					sceneFilePathText.text = _sceneFilePath;
				}
			}
		}
	}

	public string presetFilePath
	{
		get
		{
			return _presetFilePath;
		}
		set
		{
			if (_presetFilePath != value)
			{
				_presetFilePath = value;
				if (presetFilePathText != null)
				{
					presetFilePathText.text = _presetFilePath;
				}
			}
		}
	}

	public bool boolValue
	{
		get
		{
			return _boolValue;
		}
		set
		{
			if (_boolValue != value)
			{
				_boolValue = value;
				if (boolValueToggle != null)
				{
					boolValueToggle.isOn = _boolValue;
				}
			}
		}
	}

	public float floatValue
	{
		get
		{
			return _floatValue;
		}
		set
		{
			if (_floatValue != value)
			{
				_floatValue = value;
				if (floatValueSlider != null)
				{
					floatValueSlider.value = _floatValue;
				}
			}
		}
	}

	public string stringValue
	{
		get
		{
			return _stringValue;
		}
		set
		{
			if (_stringValue != value)
			{
				_stringValue = value;
				if (stringValueField != null)
				{
					stringValueField.text = _stringValue;
				}
			}
		}
	}

	public string stringChooserValue
	{
		get
		{
			return _stringChooserValue;
		}
		set
		{
			if (_stringChooserValue != value)
			{
				_stringChooserValue = value;
				if (stringChooserValuePopup != null)
				{
					stringChooserValuePopup.currentValueNoCallback = _stringChooserValue;
				}
			}
		}
	}

	public override JSONClass GetJSON()
	{
		CheckMissingReceiver();
		JSONClass jSON = base.GetJSON();
		switch (actionType)
		{
			case JSONStorable.Type.Bool:
				jSON["boolValue"].AsBool = _boolValue;
				break;
			case JSONStorable.Type.Float:
				jSON["floatValue"].AsFloat = _floatValue;
				break;
			case JSONStorable.Type.String:
				if (_stringValue != null)
				{
					jSON["stringValue"] = _stringValue;
				}
				break;
			case JSONStorable.Type.Url:
				if (_stringValue != null)
				{
					jSON["urlValue"] = _stringValue;
				}
				break;
			case JSONStorable.Type.StringChooser:
				if (_stringChooserValue != null)
				{
					jSON["stringChooserValue"] = _stringChooserValue;
				}
				break;
			case JSONStorable.Type.Color:
				jSON["color"]["h"].AsFloat = _HSVColorH;
				jSON["color"]["s"].AsFloat = _HSVColorS;
				jSON["color"]["v"].AsFloat = _HSVColorV;
				break;
			case JSONStorable.Type.AudioClipAction:
				jSON["audioClipType"] = _audioClipType.ToString();
				if (_audioClipCategory != null)
				{
					jSON["audioClipCategory"] = _audioClipCategory;
				}
				if (_audioClip != null)
				{
					jSON["audioClip"] = _audioClip.uid;
				}
				break;
			case JSONStorable.Type.SceneFilePathAction:
				if (_sceneFilePath != null && _sceneFilePath != string.Empty)
				{
					string text2 = _sceneFilePath;
					if (SuperController.singleton != null)
					{
						text2 = SuperController.singleton.NormalizeSavePath(text2);
					}
					jSON["sceneFilePath"] = text2;
				}
				break;
			case JSONStorable.Type.PresetFilePathAction:
				if (_presetFilePath != null && _presetFilePath != string.Empty)
				{
					string text = _presetFilePath;
					if (SuperController.singleton != null)
					{
						text = SuperController.singleton.NormalizeSavePath(text);
					}
					jSON["presetFilePath"] = text;
				}
				break;
		}
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc)
	{
		base.RestoreFromJSON(jc);
		if (jc["boolValue"] != null)
		{
			boolValue = jc["boolValue"].AsBool;
		}
		if (jc["floatValue"] != null)
		{
			floatValue = jc["floatValue"].AsFloat;
		}
		if (jc["stringValue"] != null)
		{
			stringValue = jc["stringValue"];
		}
		if (jc["urlValue"] != null)
		{
			stringValue = FileManager.NormalizeLoadPath(jc["urlValue"]);
		}
		if (jc["stringChooserValue"] != null)
		{
			stringChooserValue = jc["stringChooserValue"];
		}
		if (jc["color"] != null)
		{
			float h = _HSVColorH;
			float s = _HSVColorS;
			float v = _HSVColorV;
			if (jc["color"]["h"] != null)
			{
				h = jc["color"]["h"].AsFloat;
			}
			if (jc["color"]["s"] != null)
			{
				s = jc["color"]["s"].AsFloat;
			}
			if (jc["color"]["v"] != null)
			{
				v = jc["color"]["v"].AsFloat;
			}
			SetColorFromHSV(h, s, v);
		}
		if (jc["audioClipType"] != null)
		{
			SetAudioClipType(jc["audioClipType"]);
		}
		if (jc["audioClipCategory"] != null)
		{
			SetAudioClipCategory(jc["audioClipCategory"]);
		}
		if (jc["audioClip"] != null)
		{
			SetAudioClip(jc["audioClip"]);
		}
		if (jc["sceneFilePath"] != null)
		{
			string path = jc["sceneFilePath"];
			if (SuperController.singleton != null)
			{
				path = SuperController.singleton.NormalizeLoadPath(path);
			}
			SetSceneFilePath(path);
		}
		if (jc["presetFilePath"] != null)
		{
			string path2 = jc["presetFilePath"];
			if (SuperController.singleton != null)
			{
				path2 = SuperController.singleton.NormalizeLoadPath(path2);
			}
			SetPresetFilePath(path2);
		}
	}

	protected override void CreateTriggerActionPanel()
	{
		if (handler != null)
		{
			triggerActionPanel = handler.CreateTriggerActionDiscreteUI();
			InitTriggerActionPanelUI();
		}
		else
		{
			Debug.LogError("Attempt to CreateTriggerActionPanel when handler is null");
		}
	}

	public void Test()
	{
		triggerFlip = !triggerFlip;
		Trigger(triggerFlip, force: true);
	}

	protected override void SetReceiverTargetPopupNames()
	{
		receiverTargetNames = null;
		if (_receiver != null)
		{
			receiverTargetNames = _receiver.GetAllParamAndActionNames();
		}
		SyncTargetPopupNames();
	}

	protected override void SetInitialParamsFromReceiverTarget()
	{
		base.SetInitialParamsFromReceiverTarget();
		if (_receiver != null && base.receiverTargetName != null)
		{
			switch (actionType)
			{
				case JSONStorable.Type.Bool:
					boolValue = _receiver.GetBoolParamValue(_receiverTargetName);
					break;
				case JSONStorable.Type.Float:
					floatValue = _receiver.GetFloatParamValue(_receiverTargetName);
					break;
				case JSONStorable.Type.String:
					stringValue = _receiver.GetStringParamValue(_receiverTargetName);
					break;
				case JSONStorable.Type.Url:
					stringValue = _receiver.GetUrlParamValue(_receiverTargetName);
					break;
				case JSONStorable.Type.StringChooser:
					stringChooserValue = _receiver.GetStringChooserParamValue(_receiverTargetName);
					break;
				case JSONStorable.Type.Color:
				{
					HSVColor colorParamValue = _receiver.GetColorParamValue(_receiverTargetName);
					SetColorFromHSV(colorParamValue.H, colorParamValue.S, colorParamValue.V);
					break;
				}
				case JSONStorable.Type.Vector3:
					break;
			}
		}
	}

	protected void SyncStringChooserPopup()
	{
		if (!(stringChooserValuePopup != null) || !(_receiver != null) || _receiverTargetName == null)
		{
			return;
		}
		stringChooserValuePopup.label = _receiverTargetName;
		if (_receiver.IsStringChooserJSONParam(_receiverTargetName))
		{
			List<string> stringChooserJSONParamChoices = _receiver.GetStringChooserJSONParamChoices(_receiverTargetName);
			stringChooserValuePopup.numPopupValues = stringChooserJSONParamChoices.Count;
			for (int i = 0; i < stringChooserJSONParamChoices.Count; i++)
			{
				stringChooserValuePopup.setPopupValue(i, stringChooserJSONParamChoices[i]);
			}
		}
	}

	protected override void SyncFromReceiverTarget()
	{
		base.SyncFromReceiverTarget();
		if (!(_receiver != null) || _receiverTargetName == null)
		{
			return;
		}
		actionType = _receiver.GetParamOrActionType(_receiverTargetName);
		switch (actionType)
		{
			case JSONStorable.Type.Float:
				if (floatValueSlider != null)
				{
					floatValueSlider.minValue = paramMin;
					floatValueSlider.maxValue = paramMax;
				}
				if (floatValueDynamicSlider != null)
				{
					floatValueDynamicSlider.rangeAdjustEnabled = !paramContrained;
					if (receiverTargetFloat != null)
					{
						floatValueDynamicSlider.label = receiverTargetFloat.name;
					}
				}
				break;
			case JSONStorable.Type.StringChooser:
				SyncStringChooserPopup();
				break;
		}
	}

	protected void SyncType()
	{
		if (boolValueToggle != null)
		{
			if (_actionType == JSONStorable.Type.Bool)
			{
				boolValueToggle.gameObject.SetActive(value: true);
			}
			else
			{
				boolValueToggle.gameObject.SetActive(value: false);
			}
		}
		if (floatValueSlider != null)
		{
			if (_actionType == JSONStorable.Type.Float)
			{
				floatValueSlider.transform.parent.gameObject.SetActive(value: true);
			}
			else
			{
				floatValueSlider.transform.parent.gameObject.SetActive(value: false);
			}
		}
		if (stringValueField != null)
		{
			if (_actionType == JSONStorable.Type.String || _actionType == JSONStorable.Type.Url)
			{
				stringValueField.gameObject.SetActive(value: true);
			}
			else
			{
				stringValueField.gameObject.SetActive(value: false);
			}
		}
		if (stringChooserValuePopup != null)
		{
			if (_actionType == JSONStorable.Type.StringChooser)
			{
				stringChooserValuePopup.gameObject.SetActive(value: true);
			}
			else
			{
				stringChooserValuePopup.gameObject.SetActive(value: false);
			}
		}
		if (colorPickerContainer != null)
		{
			if (_actionType == JSONStorable.Type.Color)
			{
				colorPickerContainer.gameObject.SetActive(value: true);
			}
			else
			{
				colorPickerContainer.gameObject.SetActive(value: false);
			}
		}
		if (audioClipPopupsContainer != null)
		{
			if (_actionType == JSONStorable.Type.AudioClipAction)
			{
				audioClipPopupsContainer.gameObject.SetActive(value: true);
			}
			else
			{
				audioClipPopupsContainer.gameObject.SetActive(value: false);
			}
		}
		if (sceneFilePathText != null)
		{
			if (_actionType == JSONStorable.Type.SceneFilePathAction)
			{
				sceneFilePathText.gameObject.SetActive(value: true);
			}
			else
			{
				sceneFilePathText.gameObject.SetActive(value: false);
			}
		}
		if (presetFilePathText != null)
		{
			if (_actionType == JSONStorable.Type.PresetFilePathAction)
			{
				presetFilePathText.gameObject.SetActive(value: true);
			}
			else
			{
				presetFilePathText.gameObject.SetActive(value: false);
			}
		}
		if (chooseSceneFilePathButton != null)
		{
			if (_actionType == JSONStorable.Type.SceneFilePathAction)
			{
				chooseSceneFilePathButton.gameObject.SetActive(value: true);
			}
			else
			{
				chooseSceneFilePathButton.gameObject.SetActive(value: false);
			}
		}
		if (choosePresetFilePathButton != null)
		{
			if (_actionType == JSONStorable.Type.PresetFilePathAction)
			{
				choosePresetFilePathButton.gameObject.SetActive(value: true);
			}
			else
			{
				choosePresetFilePathButton.gameObject.SetActive(value: false);
			}
		}
	}

	public void SetAudioClipType(string type)
	{
		try
		{
			audioClipType = (AudioClipType)Enum.Parse(typeof(AudioClipType), type);
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to TriggerActionDiscrete audioClipType to " + type + " which is not a valid type");
		}
	}

	protected void SetClipCategoryPopupValues()
	{
		if (!(audioClipCategoryPopup != null))
		{
			return;
		}
		List<string> list = null;
		if (_audioClipType == AudioClipType.Embedded)
		{
			if (EmbeddedAudioClipManager.singleton != null)
			{
				list = EmbeddedAudioClipManager.singleton.GetCategories();
			}
		}
		else if (_audioClipType == AudioClipType.URL && URLAudioClipManager.singleton != null)
		{
			list = URLAudioClipManager.singleton.GetCategories();
		}
		if (list != null)
		{
			audioClipCategoryPopup.numPopupValues = list.Count + 1;
			int num = 0;
			audioClipCategoryPopup.setPopupValue(num, "None");
			num++;
			{
				foreach (string item in list)
				{
					audioClipCategoryPopup.setPopupValue(num, item);
					num++;
				}
				return;
			}
		}
		audioClipCategoryPopup.numPopupValues = 1;
		int index = 0;
		audioClipCategoryPopup.setPopupValue(index, "None");
	}

	public void SetAudioClipCategory(string cat)
	{
		audioClipCategory = cat;
	}

	protected void SetClipPopupValues()
	{
		if (!(audioClipPopup != null))
		{
			return;
		}
		audioClipPopup.useDifferentDisplayValues = true;
		List<NamedAudioClip> list = null;
		if (_audioClipCategory != null)
		{
			if (_audioClipType == AudioClipType.Embedded)
			{
				if (EmbeddedAudioClipManager.singleton != null)
				{
					list = EmbeddedAudioClipManager.singleton.GetCategoryClips(_audioClipCategory);
				}
			}
			else if (_audioClipType == AudioClipType.URL && URLAudioClipManager.singleton != null)
			{
				list = URLAudioClipManager.singleton.GetCategoryClips(_audioClipCategory);
			}
		}
		if (list != null)
		{
			audioClipPopup.numPopupValues = list.Count + 1;
			int num = 0;
			audioClipPopup.setPopupValue(num, "None");
			audioClipPopup.setDisplayPopupValue(num, "None");
			num++;
			{
				foreach (NamedAudioClip item in list)
				{
					audioClipPopup.setPopupValue(num, item.uid);
					audioClipPopup.setDisplayPopupValue(num, item.displayName);
					num++;
				}
				return;
			}
		}
		audioClipPopup.numPopupValues = 1;
		audioClipPopup.setPopupValue(0, "None");
	}

	public void SetAudioClip(string clipUID)
	{
		if (clipUID != "None")
		{
			if (_audioClipType == AudioClipType.Embedded)
			{
				if (EmbeddedAudioClipManager.singleton != null)
				{
					audioClip = EmbeddedAudioClipManager.singleton.GetClip(clipUID);
				}
			}
			else if (_audioClipType == AudioClipType.URL && URLAudioClipManager.singleton != null)
			{
				audioClip = URLAudioClipManager.singleton.GetClip(clipUID);
			}
		}
		else
		{
			audioClip = null;
		}
	}

	protected void GetSceneFilePath()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.GetScenePathDialog(SetSceneFilePath);
		}
	}

	public void SetSceneFilePath(string path)
	{
		path = SuperController.singleton.NormalizeScenePath(path);
		sceneFilePath = path;
	}

	protected void GetPresetFilePath()
	{
		if (base.receiver != null && base.receiverTargetName != null)
		{
			base.receiver.GetPresetFilePathAction(base.receiverTargetName)?.Browse(SetPresetFilePath);
		}
	}

	public void SetPresetFilePath(string path)
	{
		path = SuperController.singleton.NormalizePath(path);
		presetFilePath = path;
	}

	public void SetFloatValue(float f)
	{
		_floatValue = f;
	}

	protected void SetStringValue(string v)
	{
		stringValue = v;
	}

	protected void SetStringValueToField()
	{
		if (stringValueField != null)
		{
			stringValue = stringValueField.text;
		}
	}

	protected virtual void SetColorFromHSV(float h, float s, float v)
	{
		if (_HSVColorH != h || _HSVColorS != s || _HSVColorV != v)
		{
			_HSVColorH = h;
			_HSVColorS = s;
			_HSVColorV = v;
			if (colorPicker != null)
			{
				colorPicker.SetHSV(h, s, v, noCallback: true);
			}
		}
	}

	protected void SetStringChooserValue(string v)
	{
		stringChooserValue = v;
	}

	public override void Validate()
	{
		if (base.receiver != null && base.receiverTargetName != null)
		{
			switch (_actionType)
			{
				case JSONStorable.Type.Bool:
					if (!base.receiver.IsBoolJSONParam(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					break;
				case JSONStorable.Type.Float:
					if (!base.receiver.IsFloatJSONParam(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					break;
				case JSONStorable.Type.Color:
					if (!base.receiver.IsColorJSONParam(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					break;
				case JSONStorable.Type.Action:
					if (!base.receiver.IsAction(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					break;
				case JSONStorable.Type.AudioClipAction:
					if (!base.receiver.IsAudioClipAction(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					if (_audioClip != null && _audioClip.destroyed)
					{
						audioClip = null;
					}
					break;
				case JSONStorable.Type.SceneFilePathAction:
					if (!base.receiver.IsSceneFilePathAction(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					break;
				case JSONStorable.Type.PresetFilePathAction:
					if (!base.receiver.IsPresetFilePathAction(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					break;
				case JSONStorable.Type.String:
					if (!base.receiver.IsStringJSONParam(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					break;
				case JSONStorable.Type.Url:
					if (!base.receiver.IsUrlJSONParam(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					break;
				case JSONStorable.Type.StringChooser:
					if (!base.receiver.IsStringChooserJSONParam(base.receiverTargetName))
					{
						base.receiverTargetName = null;
					}
					break;
			}
		}
		base.Validate();
	}

	public void Trigger(bool reverse = false, bool force = false)
	{
		CheckMissingReceiver();
		if ((!base.enabled && !force) || !(base.receiver != null) || base.receiverTargetName == null)
		{
			return;
		}
		switch (_actionType)
		{
			case JSONStorable.Type.Bool:
				if (reverse)
				{
					base.receiver.SetBoolParamValue(base.receiverTargetName, !boolValue);
				}
				else
				{
					base.receiver.SetBoolParamValue(base.receiverTargetName, boolValue);
				}
				break;
			case JSONStorable.Type.Float:
				base.receiver.SetFloatParamValue(base.receiverTargetName, floatValue);
				break;
			case JSONStorable.Type.Color:
				triggerColor.H = _HSVColorH;
				triggerColor.S = _HSVColorS;
				triggerColor.V = _HSVColorV;
				base.receiver.SetColorParamValue(base.receiverTargetName, triggerColor);
				break;
			case JSONStorable.Type.Action:
				base.receiver.CallAction(base.receiverTargetName);
				break;
			case JSONStorable.Type.AudioClipAction:
				if (audioClip != null)
				{
					if (reverse)
					{
						base.receiver.CallAction("Stop");
					}
					else
					{
						base.receiver.CallAction(base.receiverTargetName, audioClip);
					}
				}
				break;
			case JSONStorable.Type.SceneFilePathAction:
				if (sceneFilePath != string.Empty && !reverse)
				{
					base.receiver.CallAction(base.receiverTargetName, sceneFilePath);
				}
				break;
			case JSONStorable.Type.PresetFilePathAction:
				if (presetFilePath != string.Empty && !reverse)
				{
					base.receiver.CallPresetFileAction(base.receiverTargetName, presetFilePath);
				}
				break;
			case JSONStorable.Type.String:
				base.receiver.SetStringParamValue(base.receiverTargetName, stringValue);
				break;
			case JSONStorable.Type.Url:
				base.receiver.SetUrlParamValue(base.receiverTargetName, stringValue);
				break;
			case JSONStorable.Type.StringChooser:
				base.receiver.SetStringChooserParamValue(base.receiverTargetName, stringChooserValue);
				break;
			case JSONStorable.Type.Vector3:
				break;
		}
	}

	public override void InitTriggerActionPanelUI()
	{
		CheckMissingReceiver();
		base.InitTriggerActionPanelUI();
		if (triggerActionPanel != null)
		{
			TriggerActionDiscreteUI component = triggerActionPanel.GetComponent<TriggerActionDiscreteUI>();
			if (component != null)
			{
				testButton = component.testButton;
				audioClipPopupsContainer = component.audioClipPopupsContainer;
				audioClipTypePopup = component.audioClipTypePopup;
				audioClipCategoryPopup = component.audioClipCategoryPopup;
				audioClipPopup = component.audioClipPopup;
				sceneFilePathText = component.sceneFilePathText;
				presetFilePathText = component.presetFilePathText;
				chooseSceneFilePathButton = component.chooseSceneFilePathButton;
				choosePresetFilePathButton = component.choosePresetFilePathButton;
				boolValueToggle = component.boolValueToggle;
				floatValueSlider = component.floatValueSlider;
				floatValueDynamicSlider = component.floatValueDynamicSlider;
				colorPickerContainer = component.colorPickerContainer;
				colorPicker = component.colorPicker;
				stringValueField = component.stringValueField;
				stringValueFieldAction = component.stringValueFieldAction;
				stringChooserValuePopup = component.stringChooserValuePopup;
			}
		}
		if (testButton != null)
		{
			testButton.onClick.AddListener(Test);
		}
		if (audioClipTypePopup != null)
		{
			audioClipTypePopup.numPopupValues = 2;
			audioClipTypePopup.setPopupValue(0, "Embedded");
			audioClipTypePopup.setPopupValue(1, "URL");
			audioClipTypePopup.currentValue = _audioClipType.ToString();
			UIPopup uIPopup = audioClipTypePopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClipType));
		}
		if (audioClipCategoryPopup != null)
		{
			SetClipCategoryPopupValues();
			if (_audioClipCategory == null)
			{
				audioClipCategoryPopup.currentValue = "None";
			}
			else
			{
				audioClipCategoryPopup.currentValue = _audioClipCategory;
			}
			UIPopup uIPopup2 = audioClipCategoryPopup;
			uIPopup2.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup2.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetClipCategoryPopupValues));
			UIPopup uIPopup3 = audioClipCategoryPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClipCategory));
		}
		if (audioClipPopup != null)
		{
			SetClipPopupValues();
			if (_audioClip == null)
			{
				audioClipPopup.currentValue = "None";
			}
			else
			{
				audioClipPopup.currentValue = _audioClip.uid;
			}
			UIPopup uIPopup4 = audioClipPopup;
			uIPopup4.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Combine(uIPopup4.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetClipPopupValues));
			UIPopup uIPopup5 = audioClipPopup;
			uIPopup5.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup5.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClip));
		}
		if (chooseSceneFilePathButton != null)
		{
			chooseSceneFilePathButton.onClick.AddListener(GetSceneFilePath);
		}
		if (choosePresetFilePathButton != null)
		{
			choosePresetFilePathButton.onClick.AddListener(GetPresetFilePath);
		}
		if (sceneFilePathText != null)
		{
			sceneFilePathText.text = _sceneFilePath;
		}
		if (presetFilePathText != null)
		{
			presetFilePathText.text = _presetFilePath;
		}
		if (boolValueToggle != null)
		{
			boolValueToggle.isOn = _boolValue;
			boolValueToggle.onValueChanged.AddListener(delegate
			{
				boolValue = boolValueToggle.isOn;
			});
		}
		if (floatValueSlider != null)
		{
			floatValueSlider.minValue = paramMin;
			if (floatValueSlider.minValue > _floatValue)
			{
				floatValueSlider.minValue = _floatValue;
			}
			floatValueSlider.maxValue = paramMax;
			if (floatValueSlider.maxValue < _floatValue)
			{
				floatValueSlider.maxValue = _floatValue;
			}
			floatValueSlider.value = _floatValue;
			floatValueSlider.onValueChanged.AddListener(SetFloatValue);
		}
		if (floatValueDynamicSlider != null)
		{
			floatValueDynamicSlider.rangeAdjustEnabled = !paramContrained;
			if (receiverTargetFloat != null)
			{
				floatValueDynamicSlider.label = receiverTargetFloat.name;
			}
		}
		if (colorPicker != null)
		{
			colorPicker.SetHSV(_HSVColorH, _HSVColorS, _HSVColorV);
			HSVColorPicker hSVColorPicker = colorPicker;
			hSVColorPicker.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Combine(hSVColorPicker.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetColorFromHSV));
		}
		if (stringValueField != null)
		{
			stringValueField.text = _stringValue;
			stringValueField.onEndEdit.AddListener(SetStringValue);
		}
		if (stringValueFieldAction != null)
		{
			InputFieldAction inputFieldAction = stringValueFieldAction;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetStringValueToField));
		}
		if (stringChooserValuePopup != null)
		{
			SyncStringChooserPopup();
			stringChooserValuePopup.currentValue = _stringChooserValue;
			UIPopup uIPopup6 = stringChooserValuePopup;
			uIPopup6.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup6.onValueChangeHandlers, new UIPopup.OnValueChange(SetStringChooserValue));
		}
		SyncType();
	}

	public override void DeregisterUI()
	{
		base.DeregisterUI();
		if (testButton != null)
		{
			testButton.onClick.RemoveListener(Test);
		}
		if (audioClipTypePopup != null)
		{
			UIPopup uIPopup = audioClipTypePopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClipType));
		}
		if (audioClipCategoryPopup != null)
		{
			UIPopup uIPopup2 = audioClipCategoryPopup;
			uIPopup2.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup2.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetClipCategoryPopupValues));
			UIPopup uIPopup3 = audioClipCategoryPopup;
			uIPopup3.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup3.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClipCategory));
		}
		if (audioClipPopup != null)
		{
			UIPopup uIPopup4 = audioClipPopup;
			uIPopup4.onOpenPopupHandlers = (UIPopup.OnOpenPopup)Delegate.Remove(uIPopup4.onOpenPopupHandlers, new UIPopup.OnOpenPopup(SetClipPopupValues));
			UIPopup uIPopup5 = audioClipPopup;
			uIPopup5.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup5.onValueChangeHandlers, new UIPopup.OnValueChange(SetAudioClip));
		}
		if (chooseSceneFilePathButton != null)
		{
			chooseSceneFilePathButton.onClick.RemoveListener(GetSceneFilePath);
		}
		if (choosePresetFilePathButton != null)
		{
			choosePresetFilePathButton.onClick.RemoveListener(GetPresetFilePath);
		}
		if (boolValueToggle != null)
		{
			boolValueToggle.onValueChanged.RemoveAllListeners();
		}
		if (floatValueSlider != null)
		{
			floatValueSlider.onValueChanged.RemoveListener(SetFloatValue);
		}
		if (colorPicker != null)
		{
			HSVColorPicker hSVColorPicker = colorPicker;
			hSVColorPicker.onHSVColorChangedHandlers = (HSVColorPicker.OnHSVColorChanged)Delegate.Remove(hSVColorPicker.onHSVColorChangedHandlers, new HSVColorPicker.OnHSVColorChanged(SetColorFromHSV));
		}
		if (stringValueField != null)
		{
			stringValueField.onEndEdit.RemoveListener(SetStringValue);
		}
		if (stringValueFieldAction != null)
		{
			InputFieldAction inputFieldAction = stringValueFieldAction;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Remove(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(SetStringValueToField));
		}
		if (stringChooserValuePopup != null)
		{
			UIPopup uIPopup6 = stringChooserValuePopup;
			uIPopup6.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Remove(uIPopup6.onValueChangeHandlers, new UIPopup.OnValueChange(SetStringChooserValue));
		}
	}
}
