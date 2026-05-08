using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class JSONStorable : MonoBehaviour
{
	public enum Type
	{
		None,
		Bool,
		Float,
		Vector3,
		String,
		Url,
		StringChooser,
		Color,
		Action,
		AudioClipAction,
		SceneFilePathAction,
		PresetFilePathAction
	}

	public Atom containingAtom;

	public bool exclude;

	public bool onlyStoreIfActive;

	public bool needsStore;

	public string overrideId;

	public Transform UITransform;

	public Transform UITransformAlt;

	protected static JSONClass copyStore1;

	protected static JSONClass copyStore2;

	protected static JSONClass copyStore3;

	public JSONStorableAction saveToStore1Action;

	public JSONStorableAction saveToStore2Action;

	public JSONStorableAction saveToStore3Action;

	public JSONStorableAction restoreAllFromStore1Action;

	public JSONStorableAction restorePhysicalFromStore1Action;

	public JSONStorableAction restoreAppearanceFromStore1Action;

	public JSONStorableAction restoreAllFromStore2Action;

	public JSONStorableAction restorePhysicalFromStore2Action;

	public JSONStorableAction restoreAppearanceFromStore2Action;

	public JSONStorableAction restoreAllFromStore3Action;

	public JSONStorableAction restorePhysicalFromStore3Action;

	public JSONStorableAction restoreAppearanceFromStore3Action;

	public JSONStorableAction restoreAllFromDefaultsAction;

	public JSONStorableAction restorePhysicalFromDefaultsAction;

	public JSONStorableAction restoreAppearanceFromDefaultsAction;

	protected List<string> allParamAndActionNames;

	protected List<string> allFloatAndColorParamNames;

	protected Dictionary<string, JSONStorableBool> boolParams;

	protected List<string> boolParamNames;

	protected Dictionary<string, JSONStorableFloat> floatParams;

	protected List<string> floatParamNames;

	protected Dictionary<string, JSONStorableVector3> vector3Params;

	protected List<string> vector3ParamNames;

	protected Dictionary<string, JSONStorableString> stringParams;

	protected List<string> stringParamNames;

	protected Dictionary<string, JSONStorableStringChooser> stringChooserParams;

	protected List<string> stringChooserParamNames;

	protected Dictionary<string, JSONStorableUrl> urlParams;

	protected List<string> urlParamNames;

	protected Dictionary<string, JSONStorableColor> colorParams;

	protected List<string> colorParamNames;

	protected HSVColor hsvc;

	protected Dictionary<string, JSONStorableAction> actions;

	protected List<string> actionNames;

	protected Dictionary<string, JSONStorableActionAudioClip> audioClipActions;

	protected List<string> audioClipActionNames;

	protected Dictionary<string, JSONStorableActionSceneFilePath> sceneFilePathActions;

	protected List<string> sceneFilePathActionNames;

	protected Dictionary<string, JSONStorableActionPresetFilePath> presetFilePathActions;

	protected List<string> presetFilePathActionNames;

	protected bool _isPresetRestore;

	protected bool insideRestore;

	protected bool _JSONWasInit;

	protected bool awakecalled;

	public string storeId
	{
		get
		{
			if (overrideId == null || overrideId == string.Empty)
			{
				return base.name;
			}
			if (overrideId[0] == '+')
			{
				if (overrideId.StartsWith("+parent+", StringComparison.CurrentCultureIgnoreCase))
				{
					if (base.transform.parent != null)
					{
						return base.transform.parent.name + base.name + overrideId.Substring(8);
					}
					return base.name + overrideId.Substring(8);
				}
				return base.name + overrideId.Substring(1);
			}
			return overrideId;
		}
	}

	public bool isPresetRestore
	{
		get
		{
			return _isPresetRestore;
		}
		set
		{
			_isPresetRestore = value;
		}
	}

	protected virtual void InitUI(Transform t, bool isAlt)
	{
	}

	public virtual void SetUI(Transform t)
	{
		if (UITransform != t)
		{
			UITransform = t;
			InitUI();
		}
	}

	public virtual void InitUI()
	{
		InitUI(UITransform, isAlt: false);
	}

	public virtual void SetUIAlt(Transform t)
	{
		if (UITransformAlt != t)
		{
			UITransformAlt = t;
			InitUIAlt();
		}
	}

	public virtual void InitUIAlt()
	{
		InitUI(UITransformAlt, isAlt: false);
	}

	public void RestoreAllFromDefaults()
	{
		RestoreFromJSON(new JSONClass());
	}

	public void RestorePhysicalFromDefaults()
	{
		RestoreFromJSON(new JSONClass(), restorePhysical: true, restoreAppearance: false);
	}

	public void RestoreAppearanceFromDefaults()
	{
		RestoreFromJSON(new JSONClass(), restorePhysical: false);
	}

	public void RestoreFromStore1(bool restorePhysical = true, bool restoreAppearance = true)
	{
		if (copyStore1 != null)
		{
			RestoreFromJSON(copyStore1, restorePhysical, restoreAppearance);
		}
	}

	public void RestoreAllFromStore1()
	{
		RestoreFromStore1();
	}

	public void RestorePhysicalFromStore1()
	{
		RestoreFromStore1(restorePhysical: true, restoreAppearance: false);
	}

	public void RestoreAppearanceFromStore1()
	{
		RestoreFromStore1(restorePhysical: false);
	}

	public void SaveToStore1()
	{
		copyStore1 = GetJSON(includePhysical: true, includeAppearance: true, forceStore: true);
	}

	public void RestoreFromStore2(bool restorePhysical = true, bool restoreAppearance = true)
	{
		if (copyStore2 != null)
		{
			RestoreFromJSON(copyStore2, restorePhysical, restoreAppearance);
		}
	}

	public void RestoreAllFromStore2()
	{
		RestoreFromStore2();
	}

	public void RestorePhysicalFromStore2()
	{
		RestoreFromStore2(restorePhysical: true, restoreAppearance: false);
	}

	public void RestoreAppearanceFromStore2()
	{
		RestoreFromStore2(restorePhysical: false);
	}

	public void SaveToStore2()
	{
		copyStore2 = GetJSON(includePhysical: true, includeAppearance: true, forceStore: true);
	}

	public void RestoreFromStore3(bool restorePhysical = true, bool restoreAppearance = true)
	{
		if (copyStore3 != null)
		{
			RestoreFromJSON(copyStore3, restorePhysical, restoreAppearance);
		}
	}

	public void RestoreAllFromStore3()
	{
		RestoreFromStore3();
	}

	public void RestorePhysicalFromStore3()
	{
		RestoreFromStore3(restorePhysical: true, restoreAppearance: false);
	}

	public void RestoreAppearanceFromStore3()
	{
		RestoreFromStore3(restorePhysical: false);
	}

	public void SaveToStore3()
	{
		copyStore3 = GetJSON(includePhysical: true, includeAppearance: true, forceStore: true);
	}

	public bool HasParamsOrActions()
	{
		if (!awakecalled)
		{
			Awake();
		}
		if (boolParamNames.Count > 0 || floatParamNames.Count > 0 || vector3ParamNames.Count > 0 || stringParamNames.Count > 0 || stringChooserParams.Count > 0 || urlParamNames.Count > 0 || colorParamNames.Count > 0 || actionNames.Count > 0 || audioClipActionNames.Count > 0 || sceneFilePathActionNames.Count > 0)
		{
			return true;
		}
		return false;
	}

	public List<string> GetAllParamAndActionNames()
	{
		if (!awakecalled)
		{
			Awake();
		}
		return allParamAndActionNames;
	}

	public List<string> GetAllFloatAndColorParamNames()
	{
		if (!awakecalled)
		{
			Awake();
		}
		return allFloatAndColorParamNames;
	}

	public Type GetParamOrActionType(string name)
	{
		if (!awakecalled)
		{
			Awake();
		}
		if (IsBoolJSONParam(name))
		{
			return Type.Bool;
		}
		if (IsFloatJSONParam(name))
		{
			return Type.Float;
		}
		if (IsVector3JSONParam(name))
		{
			return Type.Vector3;
		}
		if (IsColorJSONParam(name))
		{
			return Type.Color;
		}
		if (IsStringJSONParam(name))
		{
			return Type.String;
		}
		if (IsUrlJSONParam(name))
		{
			return Type.Url;
		}
		if (IsStringChooserJSONParam(name))
		{
			return Type.StringChooser;
		}
		if (IsAction(name))
		{
			return Type.Action;
		}
		if (IsAudioClipAction(name))
		{
			return Type.AudioClipAction;
		}
		if (IsSceneFilePathAction(name))
		{
			return Type.SceneFilePathAction;
		}
		if (IsPresetFilePathAction(name))
		{
			return Type.PresetFilePathAction;
		}
		return Type.None;
	}

	protected void InitBoolParams()
	{
		boolParams = new Dictionary<string, JSONStorableBool>();
		boolParamNames = new List<string>();
	}

	public void RegisterBool(JSONStorableBool param)
	{
		if (boolParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
			return;
		}
		boolParams.Add(param.name, param);
		boolParamNames.Add(param.name);
		allParamAndActionNames.Add(param.name);
	}

	public void DeregisterBool(JSONStorableBool param)
	{
		if (!boolParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
			return;
		}
		boolParams.Remove(param.name);
		boolParamNames.Remove(param.name);
		allParamAndActionNames.Remove(param.name);
	}

	public List<string> GetBoolParamNames()
	{
		return boolParamNames;
	}

	public virtual bool IsBoolJSONParam(string name)
	{
		return boolParams.ContainsKey(name);
	}

	public JSONStorableBool GetBoolJSONParam(string name)
	{
		if (boolParams.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual bool GetBoolParamValue(string param)
	{
		if (boolParams.TryGetValue(param, out var value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return false;
	}

	public virtual void SetBoolParamValue(string param, bool value)
	{
		if (boolParams.TryGetValue(param, out var value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	protected void InitFloatParams()
	{
		floatParams = new Dictionary<string, JSONStorableFloat>();
		floatParamNames = new List<string>();
	}

	public void RegisterFloat(JSONStorableFloat param)
	{
		if (floatParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
			return;
		}
		floatParams.Add(param.name, param);
		floatParamNames.Add(param.name);
		allParamAndActionNames.Add(param.name);
		allFloatAndColorParamNames.Add(param.name);
	}

	public void DeregisterFloat(JSONStorableFloat param)
	{
		if (!floatParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
			return;
		}
		floatParams.Remove(param.name);
		floatParamNames.Remove(param.name);
		allParamAndActionNames.Remove(param.name);
		allFloatAndColorParamNames.Remove(param.name);
	}

	public List<string> GetFloatParamNames()
	{
		return floatParamNames;
	}

	public virtual bool IsFloatJSONParam(string name)
	{
		return floatParams.ContainsKey(name);
	}

	public JSONStorableFloat GetFloatJSONParam(string name)
	{
		if (floatParams.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual float GetFloatParamValue(string param)
	{
		if (floatParams.TryGetValue(param, out var value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return 0f;
	}

	public virtual void SetFloatParamValue(string param, float value)
	{
		if (floatParams.TryGetValue(param, out var value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	public virtual float GetFloatJSONParamMinValue(string param)
	{
		if (floatParams.TryGetValue(param, out var value))
		{
			return value.min;
		}
		Debug.LogError("Tried to get min param value for param " + param + " that does not exist");
		return 0f;
	}

	public virtual float GetFloatJSONParamMaxValue(string param)
	{
		if (floatParams.TryGetValue(param, out var value))
		{
			return value.max;
		}
		Debug.LogError("Tried to get max param value for param " + param + " that does not exist");
		return 0f;
	}

	protected void InitVector3Params()
	{
		vector3Params = new Dictionary<string, JSONStorableVector3>();
		vector3ParamNames = new List<string>();
	}

	public void RegisterVector3(JSONStorableVector3 param)
	{
		if (vector3Params.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
			return;
		}
		vector3Params.Add(param.name, param);
		vector3ParamNames.Add(param.name);
		allParamAndActionNames.Add(param.name);
	}

	public void DeregisterVector3(JSONStorableVector3 param)
	{
		if (!vector3Params.ContainsKey(param.name))
		{
			Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
			return;
		}
		vector3Params.Remove(param.name);
		vector3ParamNames.Remove(param.name);
		allParamAndActionNames.Remove(param.name);
	}

	public List<string> GetVector3ParamNames()
	{
		return vector3ParamNames;
	}

	public virtual bool IsVector3JSONParam(string name)
	{
		return vector3Params.ContainsKey(name);
	}

	public JSONStorableVector3 GetVector3JSONParam(string name)
	{
		if (vector3Params.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual Vector3 GetVector3ParamValue(string param)
	{
		if (vector3Params.TryGetValue(param, out var value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return Vector3.zero;
	}

	public virtual void SetVector3ParamValue(string param, Vector3 value)
	{
		if (vector3Params.TryGetValue(param, out var value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	public virtual Vector3 GetVector3JSONParamMinValue(string param)
	{
		if (vector3Params.TryGetValue(param, out var value))
		{
			return value.min;
		}
		Debug.LogError("Tried to get min param value for param " + param + " that does not exist");
		return Vector3.zero;
	}

	public virtual Vector3 GetVector3JSONParamMaxValue(string param)
	{
		if (vector3Params.TryGetValue(param, out var value))
		{
			return value.max;
		}
		Debug.LogError("Tried to get max param value for param " + param + " that does not exist");
		return Vector3.zero;
	}

	protected void InitStringParams()
	{
		stringParams = new Dictionary<string, JSONStorableString>();
		stringParamNames = new List<string>();
	}

	public void RegisterString(JSONStorableString param)
	{
		if (stringParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
			return;
		}
		stringParams.Add(param.name, param);
		stringParamNames.Add(param.name);
		allParamAndActionNames.Add(param.name);
	}

	public void DeregisterString(JSONStorableString param)
	{
		if (!stringParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
			return;
		}
		stringParams.Remove(param.name);
		stringParamNames.Remove(param.name);
		allParamAndActionNames.Remove(param.name);
	}

	public List<string> GetStringParamNames()
	{
		return stringParamNames;
	}

	public virtual bool IsStringJSONParam(string name)
	{
		return stringParams.ContainsKey(name);
	}

	public JSONStorableString GetStringJSONParam(string name)
	{
		if (stringParams.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual string GetStringParamValue(string param)
	{
		if (stringParams.TryGetValue(param, out var value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return null;
	}

	public virtual void SetStringParamValue(string param, string value)
	{
		if (stringParams.TryGetValue(param, out var value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	protected void InitStringChooserParams()
	{
		stringChooserParams = new Dictionary<string, JSONStorableStringChooser>();
		stringChooserParamNames = new List<string>();
	}

	public void RegisterStringChooser(JSONStorableStringChooser param)
	{
		if (stringChooserParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
			return;
		}
		stringChooserParams.Add(param.name, param);
		stringChooserParamNames.Add(param.name);
		allParamAndActionNames.Add(param.name);
	}

	public void DeregisterStringChooser(JSONStorableStringChooser param)
	{
		if (!stringChooserParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
			return;
		}
		stringChooserParams.Remove(param.name);
		stringChooserParamNames.Remove(param.name);
		allParamAndActionNames.Remove(param.name);
	}

	public List<string> GetStringChooserParamNames()
	{
		return stringChooserParamNames;
	}

	public virtual bool IsStringChooserJSONParam(string name)
	{
		return stringChooserParams.ContainsKey(name);
	}

	public JSONStorableStringChooser GetStringChooserJSONParam(string name)
	{
		if (stringChooserParams.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual string GetStringChooserParamValue(string param)
	{
		if (stringChooserParams.TryGetValue(param, out var value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return null;
	}

	public virtual void SetStringChooserParamValue(string param, string value)
	{
		if (stringChooserParams.TryGetValue(param, out var value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	public virtual List<string> GetStringChooserJSONParamChoices(string param)
	{
		if (stringChooserParams.TryGetValue(param, out var value))
		{
			return value.choices;
		}
		Debug.LogError("Tried to get param choices for param " + param + " that does not exist");
		return null;
	}

	protected void InitUrlParams()
	{
		urlParams = new Dictionary<string, JSONStorableUrl>();
		urlParamNames = new List<string>();
	}

	public void RegisterUrl(JSONStorableUrl param)
	{
		if (urlParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
			return;
		}
		urlParams.Add(param.name, param);
		urlParamNames.Add(param.name);
		allParamAndActionNames.Add(param.name);
	}

	public void DeregisterUrl(JSONStorableUrl param)
	{
		if (!urlParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
			return;
		}
		urlParams.Remove(param.name);
		urlParamNames.Remove(param.name);
		allParamAndActionNames.Remove(param.name);
	}

	public List<string> GetUrlParamNames()
	{
		return urlParamNames;
	}

	public virtual bool IsUrlJSONParam(string name)
	{
		return urlParams.ContainsKey(name);
	}

	public JSONStorableUrl GetUrlJSONParam(string name)
	{
		if (urlParams.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual string GetUrlParamValue(string param)
	{
		if (urlParams.TryGetValue(param, out var value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		return null;
	}

	public virtual void SetUrlParamValue(string param, string value)
	{
		if (urlParams.TryGetValue(param, out var value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	protected void InitColorParams()
	{
		colorParams = new Dictionary<string, JSONStorableColor>();
		colorParamNames = new List<string>();
	}

	public void RegisterColor(JSONStorableColor param)
	{
		if (colorParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried registering param " + param.name + " that already exists");
			return;
		}
		colorParams.Add(param.name, param);
		colorParamNames.Add(param.name);
		allParamAndActionNames.Add(param.name);
		allFloatAndColorParamNames.Add(param.name);
	}

	public void DeregisterColor(JSONStorableColor param)
	{
		if (!colorParams.ContainsKey(param.name))
		{
			Debug.LogError("Tried deregistering param " + param.name + " that does not exist");
			return;
		}
		colorParams.Remove(param.name);
		colorParamNames.Remove(param.name);
		allParamAndActionNames.Remove(param.name);
		allFloatAndColorParamNames.Remove(param.name);
	}

	public List<string> GetColorParamNames()
	{
		return colorParamNames;
	}

	public virtual bool IsColorJSONParam(string name)
	{
		return colorParams.ContainsKey(name);
	}

	public JSONStorableColor GetColorJSONParam(string name)
	{
		if (colorParams.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual HSVColor GetColorParamValue(string param)
	{
		if (colorParams.TryGetValue(param, out var value))
		{
			return value.val;
		}
		Debug.LogError("Tried to get param value for param " + param + " that does not exist");
		hsvc.H = 0f;
		hsvc.S = 0f;
		hsvc.V = 0f;
		return hsvc;
	}

	public virtual void SetColorParamValue(string param, HSVColor value)
	{
		if (colorParams.TryGetValue(param, out var value2))
		{
			value2.val = value;
		}
		else
		{
			Debug.LogError("Tried to set param value for param " + param + " that does not exist");
		}
	}

	protected void InitActions()
	{
		actions = new Dictionary<string, JSONStorableAction>();
		actionNames = new List<string>();
	}

	public void RegisterAction(JSONStorableAction action)
	{
		if (actions.ContainsKey(action.name))
		{
			Debug.LogError("Tried registering action " + action.name + " that already exists");
			return;
		}
		actions.Add(action.name, action);
		actionNames.Add(action.name);
		allParamAndActionNames.Add(action.name);
	}

	public void DeregisterAction(JSONStorableAction action)
	{
		if (!actions.ContainsKey(action.name))
		{
			Debug.LogError("Tried deregistering action " + action.name + " that does not exist");
			return;
		}
		actions.Remove(action.name);
		actionNames.Remove(action.name);
		allParamAndActionNames.Remove(action.name);
	}

	public List<string> GetActionNames()
	{
		return actionNames;
	}

	public virtual bool IsAction(string name)
	{
		return actions.ContainsKey(name);
	}

	public JSONStorableAction GetAction(string name)
	{
		if (actions.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual void CallAction(string actionName)
	{
		if (actions.TryGetValue(actionName, out var value))
		{
			if (value.actionCallback != null)
			{
				value.actionCallback();
			}
		}
		else
		{
			Debug.LogError("Tried to call action " + actionName + " that does not exist");
		}
	}

	protected void InitAudioClipActions()
	{
		audioClipActions = new Dictionary<string, JSONStorableActionAudioClip>();
		audioClipActionNames = new List<string>();
	}

	public void RegisterAudioClipAction(JSONStorableActionAudioClip action)
	{
		if (audioClipActions.ContainsKey(action.name))
		{
			Debug.LogError("Tried registering action " + action.name + " that already exists");
			return;
		}
		audioClipActions.Add(action.name, action);
		audioClipActionNames.Add(action.name);
		allParamAndActionNames.Add(action.name);
	}

	public void DeregisterAudioClipAction(JSONStorableActionAudioClip action)
	{
		if (!actions.ContainsKey(action.name))
		{
			Debug.LogError("Tried deregistering action " + action.name + " that does not exist");
			return;
		}
		audioClipActions.Remove(action.name);
		audioClipActionNames.Remove(action.name);
		allParamAndActionNames.Remove(action.name);
	}

	public List<string> GetAudioClipActionNames()
	{
		return audioClipActionNames;
	}

	public virtual bool IsAudioClipAction(string name)
	{
		return audioClipActions.ContainsKey(name);
	}

	public JSONStorableActionAudioClip GetAudioClipAction(string name)
	{
		if (audioClipActions.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual void CallAction(string actionName, NamedAudioClip nac)
	{
		if (audioClipActions.TryGetValue(actionName, out var value))
		{
			if (value.actionCallback != null)
			{
				value.actionCallback(nac);
			}
		}
		else
		{
			Debug.LogError("Tried to call action " + actionName + " that does not exist");
		}
	}

	protected void InitSceneFilePathActions()
	{
		sceneFilePathActions = new Dictionary<string, JSONStorableActionSceneFilePath>();
		sceneFilePathActionNames = new List<string>();
	}

	public void RegisterSceneFilePathAction(JSONStorableActionSceneFilePath action)
	{
		if (sceneFilePathActions.ContainsKey(action.name))
		{
			Debug.LogError("Tried registering action " + action.name + " that already exists");
			return;
		}
		sceneFilePathActions.Add(action.name, action);
		sceneFilePathActionNames.Add(action.name);
		allParamAndActionNames.Add(action.name);
	}

	public void DeregisterSceneFilePathAction(JSONStorableActionSceneFilePath action)
	{
		if (!actions.ContainsKey(action.name))
		{
			Debug.LogError("Tried deregistering action " + action.name + " that does not exist");
			return;
		}
		sceneFilePathActions.Remove(action.name);
		sceneFilePathActionNames.Remove(action.name);
		allParamAndActionNames.Remove(action.name);
	}

	public List<string> GetSceneFilePathActionNames()
	{
		return sceneFilePathActionNames;
	}

	public virtual bool IsSceneFilePathAction(string name)
	{
		return sceneFilePathActions.ContainsKey(name);
	}

	public JSONStorableActionSceneFilePath GetSceneFilePathAction(string name)
	{
		if (sceneFilePathActions.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual void CallAction(string actionName, string path)
	{
		if (sceneFilePathActions.TryGetValue(actionName, out var value))
		{
			if (value.actionCallback != null)
			{
				value.actionCallback(path);
			}
		}
		else
		{
			Debug.LogError("Tried to call action " + actionName + " that does not exist");
		}
	}

	protected void InitPresetFilePathActions()
	{
		presetFilePathActions = new Dictionary<string, JSONStorableActionPresetFilePath>();
		presetFilePathActionNames = new List<string>();
	}

	public void RegisterPresetFilePathAction(JSONStorableActionPresetFilePath action)
	{
		if (presetFilePathActions.ContainsKey(action.name))
		{
			Debug.LogError("Tried registering action " + action.name + " that already exists");
			return;
		}
		presetFilePathActions.Add(action.name, action);
		presetFilePathActionNames.Add(action.name);
		allParamAndActionNames.Add(action.name);
	}

	public void DeregisterPresetFilePathAction(JSONStorableActionPresetFilePath action)
	{
		if (!actions.ContainsKey(action.name))
		{
			Debug.LogError("Tried deregistering action " + action.name + " that does not exist");
			return;
		}
		presetFilePathActions.Remove(action.name);
		presetFilePathActionNames.Remove(action.name);
		allParamAndActionNames.Remove(action.name);
	}

	public List<string> GetPresetFilePathActionNames()
	{
		return presetFilePathActionNames;
	}

	public virtual bool IsPresetFilePathAction(string name)
	{
		return presetFilePathActions.ContainsKey(name);
	}

	public JSONStorableActionPresetFilePath GetPresetFilePathAction(string name)
	{
		if (presetFilePathActions.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual void CallPresetFileAction(string actionName, string path)
	{
		if (presetFilePathActions.TryGetValue(actionName, out var value))
		{
			if (value.actionCallback != null)
			{
				value.actionCallback(path);
			}
		}
		else
		{
			Debug.LogError("Tried to call action " + actionName + " that does not exist");
		}
	}

	public virtual JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		if (!awakecalled)
		{
			Awake();
		}
		JSONClass jSONClass = new JSONClass();
		jSONClass["id"] = storeId;
		needsStore = false;
		foreach (string boolParamName in boolParamNames)
		{
			if (boolParams.TryGetValue(boolParamName, out var value) && value.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string floatParamName in floatParamNames)
		{
			if (floatParams.TryGetValue(floatParamName, out var value2) && value2.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string vector3ParamName in vector3ParamNames)
		{
			if (vector3Params.TryGetValue(vector3ParamName, out var value3) && value3.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string stringParamName in stringParamNames)
		{
			if (stringParams.TryGetValue(stringParamName, out var value4) && value4.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string stringChooserParamName in stringChooserParamNames)
		{
			if (stringChooserParams.TryGetValue(stringChooserParamName, out var value5) && value5.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string urlParamName in urlParamNames)
		{
			if (urlParams.TryGetValue(urlParamName, out var value6) && value6.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		foreach (string colorParamName in colorParamNames)
		{
			if (colorParams.TryGetValue(colorParamName, out var value7) && value7.StoreJSON(jSONClass, includePhysical, includeAppearance, forceStore))
			{
				needsStore = true;
			}
		}
		return jSONClass;
	}

	public virtual void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		insideRestore = true;
		if (!awakecalled)
		{
			Awake();
		}
		foreach (string boolParamName in boolParamNames)
		{
			if (boolParams.TryGetValue(boolParamName, out var value))
			{
				value.RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string floatParamName in floatParamNames)
		{
			if (floatParams.TryGetValue(floatParamName, out var value2))
			{
				value2.RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string vector3ParamName in vector3ParamNames)
		{
			if (vector3Params.TryGetValue(vector3ParamName, out var value3))
			{
				value3.RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string stringParamName in stringParamNames)
		{
			if (stringParams.TryGetValue(stringParamName, out var value4))
			{
				value4.RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string stringChooserParamName in stringChooserParamNames)
		{
			if (stringChooserParams.TryGetValue(stringChooserParamName, out var value5))
			{
				value5.RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string urlParamName in urlParamNames)
		{
			if (urlParams.TryGetValue(urlParamName, out var value6))
			{
				value6.RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string colorParamName in colorParamNames)
		{
			if (colorParams.TryGetValue(colorParamName, out var value7))
			{
				value7.RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		insideRestore = false;
	}

	public virtual void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		insideRestore = true;
		if (!awakecalled)
		{
			Awake();
		}
		foreach (string boolParamName in boolParamNames)
		{
			if (boolParams.TryGetValue(boolParamName, out var value))
			{
				value.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string floatParamName in floatParamNames)
		{
			if (floatParams.TryGetValue(floatParamName, out var value2))
			{
				value2.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string vector3ParamName in vector3ParamNames)
		{
			if (vector3Params.TryGetValue(vector3ParamName, out var value3))
			{
				value3.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string stringParamName in stringParamNames)
		{
			if (stringParams.TryGetValue(stringParamName, out var value4))
			{
				value4.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string stringChooserParamName in stringChooserParamNames)
		{
			if (stringChooserParams.TryGetValue(stringChooserParamName, out var value5))
			{
				value5.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string urlParamName in urlParamNames)
		{
			if (urlParams.TryGetValue(urlParamName, out var value6))
			{
				value6.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		foreach (string colorParamName in colorParamNames)
		{
			if (colorParams.TryGetValue(colorParamName, out var value7))
			{
				value7.RestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
			}
		}
		insideRestore = false;
	}

	public virtual void Validate()
	{
	}

	public virtual void PreRestore()
	{
	}

	public virtual void PostRestore()
	{
	}

	public virtual void SetDefaultsFromCurrent()
	{
		if (!awakecalled)
		{
			Awake();
		}
		foreach (string boolParamName in boolParamNames)
		{
			if (boolParams.TryGetValue(boolParamName, out var value))
			{
				value.SetDefaultFromCurrent();
			}
		}
		foreach (string floatParamName in floatParamNames)
		{
			if (floatParams.TryGetValue(floatParamName, out var value2))
			{
				value2.SetDefaultFromCurrent();
			}
		}
		foreach (string vector3ParamName in vector3ParamNames)
		{
			if (vector3Params.TryGetValue(vector3ParamName, out var value3))
			{
				value3.SetDefaultFromCurrent();
			}
		}
		foreach (string stringParamName in stringParamNames)
		{
			if (stringParams.TryGetValue(stringParamName, out var value4))
			{
				value4.SetDefaultFromCurrent();
			}
		}
		foreach (string stringChooserParamName in stringChooserParamNames)
		{
			if (stringChooserParams.TryGetValue(stringChooserParamName, out var value5))
			{
				value5.SetDefaultFromCurrent();
			}
		}
		foreach (string urlParamName in urlParamNames)
		{
			if (urlParams.TryGetValue(urlParamName, out var value6))
			{
				value6.SetDefaultFromCurrent();
			}
		}
		foreach (string colorParamName in colorParamNames)
		{
			if (colorParams.TryGetValue(colorParamName, out var value7))
			{
				value7.SetDefaultFromCurrent();
			}
		}
	}

	public virtual void Remove()
	{
	}

	protected virtual void InitJSONStorable()
	{
		if (!_JSONWasInit)
		{
			_JSONWasInit = true;
			allParamAndActionNames = new List<string>();
			allFloatAndColorParamNames = new List<string>();
			InitBoolParams();
			InitFloatParams();
			InitVector3Params();
			InitStringParams();
			InitStringChooserParams();
			InitUrlParams();
			InitColorParams();
			InitActions();
			InitAudioClipActions();
			InitSceneFilePathActions();
			InitPresetFilePathActions();
			saveToStore1Action = new JSONStorableAction("SaveToStore1", SaveToStore1);
			RegisterAction(saveToStore1Action);
			saveToStore2Action = new JSONStorableAction("SaveToStore2", SaveToStore2);
			RegisterAction(saveToStore2Action);
			saveToStore3Action = new JSONStorableAction("SaveToStore3", SaveToStore3);
			RegisterAction(saveToStore3Action);
			restoreAllFromStore1Action = new JSONStorableAction("RestoreAllFromStore1", RestoreAllFromStore1);
			RegisterAction(restoreAllFromStore1Action);
			restorePhysicalFromStore1Action = new JSONStorableAction("RestorePhysicsFromStore1", RestorePhysicalFromStore1);
			RegisterAction(restorePhysicalFromStore1Action);
			restoreAppearanceFromStore1Action = new JSONStorableAction("RestoreAppearanceFromStore1", RestoreAppearanceFromStore1);
			RegisterAction(restoreAppearanceFromStore1Action);
			restoreAllFromStore2Action = new JSONStorableAction("RestoreAllFromStore2", RestoreAllFromStore2);
			RegisterAction(restoreAllFromStore2Action);
			restorePhysicalFromStore2Action = new JSONStorableAction("RestorePhysicsFromStore2", RestorePhysicalFromStore2);
			RegisterAction(restorePhysicalFromStore2Action);
			restoreAppearanceFromStore2Action = new JSONStorableAction("RestoreAppearanceFromStore2", RestoreAppearanceFromStore2);
			RegisterAction(restoreAppearanceFromStore2Action);
			restoreAllFromStore3Action = new JSONStorableAction("RestoreAllFromStore3", RestoreAllFromStore3);
			RegisterAction(restoreAllFromStore3Action);
			restorePhysicalFromStore3Action = new JSONStorableAction("RestorePhysicsFromStore3", RestorePhysicalFromStore3);
			RegisterAction(restorePhysicalFromStore3Action);
			restoreAppearanceFromStore3Action = new JSONStorableAction("RestoreAppearanceFromStore3", RestoreAppearanceFromStore3);
			RegisterAction(restoreAppearanceFromStore3Action);
			restoreAllFromDefaultsAction = new JSONStorableAction("RestoreAllFromDefaults", RestoreAllFromDefaults);
			RegisterAction(restoreAllFromDefaultsAction);
			restorePhysicalFromDefaultsAction = new JSONStorableAction("RestorePhysicalFromDefaults", RestorePhysicalFromDefaults);
			RegisterAction(restorePhysicalFromDefaultsAction);
			restoreAppearanceFromDefaultsAction = new JSONStorableAction("RestoreAppearanceFromDefaults", RestoreAppearanceFromDefaults);
			RegisterAction(restoreAppearanceFromDefaultsAction);
		}
	}

	protected virtual void Awake()
	{
		if (!awakecalled)
		{
			awakecalled = true;
			InitJSONStorable();
		}
	}
}
