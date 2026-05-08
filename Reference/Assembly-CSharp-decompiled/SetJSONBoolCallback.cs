using SimpleJSON;
using UnityEngine.UI;

public class JSONStorableBool : JSONStorableParam
{
	public delegate void SetBoolCallback(bool val);

	public delegate void SetJSONBoolCallback(JSONStorableBool jb);

	protected bool _defaultVal;

	protected bool _val;

	public SetBoolCallback setCallbackFunction;

	public SetJSONBoolCallback setJSONCallbackFunction;

	protected Toggle _toggle;

	protected Toggle _toggleAlt;

	public bool defaultVal
	{
		get
		{
			return _defaultVal;
		}
		set
		{
			if (_defaultVal != value)
			{
				_defaultVal = value;
			}
		}
	}

	public bool val
	{
		get
		{
			return _val;
		}
		set
		{
			InternalSetVal(value);
		}
	}

	public bool valNoCallback
	{
		get
		{
			return _val;
		}
		set
		{
			InternalSetVal(value, doCallback: false);
		}
	}

	public Toggle toggle
	{
		get
		{
			return _toggle;
		}
		set
		{
			if (_toggle != value)
			{
				if (_toggle != null)
				{
					_toggle.onValueChanged.RemoveListener(SetVal);
				}
				_toggle = value;
				if (_toggle != null)
				{
					_toggle.isOn = _val;
					_toggle.onValueChanged.AddListener(SetVal);
				}
			}
		}
	}

	public Toggle toggleAlt
	{
		get
		{
			return _toggleAlt;
		}
		set
		{
			if (_toggleAlt != value)
			{
				if (_toggleAlt != null)
				{
					_toggleAlt.onValueChanged.RemoveListener(SetVal);
				}
				_toggleAlt = value;
				if (_toggleAlt != null)
				{
					_toggleAlt.isOn = _val;
					_toggleAlt.onValueChanged.AddListener(SetVal);
				}
			}
		}
	}

	public JSONStorableBool(string paramName, bool startingValue)
	{
		name = paramName;
		defaultVal = startingValue;
		val = startingValue;
		setCallbackFunction = null;
		setJSONCallbackFunction = null;
	}

	public JSONStorableBool(string paramName, bool startingValue, SetBoolCallback callback)
	{
		name = paramName;
		defaultVal = startingValue;
		val = startingValue;
		setCallbackFunction = callback;
		setJSONCallbackFunction = null;
	}

	public JSONStorableBool(string paramName, bool startingValue, SetJSONBoolCallback callback)
	{
		name = paramName;
		defaultVal = startingValue;
		val = startingValue;
		setCallbackFunction = null;
		setJSONCallbackFunction = callback;
	}

	public override bool StoreJSON(JSONClass jc, bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		bool flag = NeedsStore(jc, includePhysical, includeAppearance) && (forceStore || _val != _defaultVal);
		if (flag)
		{
			jc[name].AsBool = _val;
		}
		return flag;
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		if (!NeedsRestore(jc, restorePhysical, restoreAppearance))
		{
			return;
		}
		if (jc[name] != null)
		{
			val = jc[name].AsBool;
		}
		else if (altName != null)
		{
			if (jc[altName] != null)
			{
				val = jc[altName].AsBool;
			}
			else if (setMissingToDefault)
			{
				val = _defaultVal;
			}
		}
		else if (setMissingToDefault)
		{
			val = _defaultVal;
		}
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		if (!NeedsLateRestore(jc, restorePhysical, restoreAppearance))
		{
			return;
		}
		if (jc[name] != null)
		{
			val = jc[name].AsBool;
		}
		else if (altName != null)
		{
			if (jc[altName] != null)
			{
				val = jc[altName].AsBool;
			}
			else if (setMissingToDefault)
			{
				val = _defaultVal;
			}
		}
		else if (setMissingToDefault)
		{
			val = _defaultVal;
		}
	}

	public override void SetDefaultFromCurrent()
	{
		defaultVal = val;
	}

	public override void SetValToDefault()
	{
		val = defaultVal;
	}

	protected void InternalSetVal(bool b, bool doCallback = true)
	{
		if (_val == b)
		{
			return;
		}
		_val = b;
		if (_toggle != null)
		{
			_toggle.isOn = _val;
		}
		if (_toggleAlt != null)
		{
			_toggleAlt.isOn = _val;
		}
		if (doCallback)
		{
			if (setCallbackFunction != null)
			{
				setCallbackFunction(_val);
			}
			if (setJSONCallbackFunction != null)
			{
				setJSONCallbackFunction(this);
			}
		}
	}

	public void SetVal(bool value)
	{
		val = value;
	}

	public void RegisterToggle(Toggle t, bool isAlt = false)
	{
		if (isAlt)
		{
			toggleAlt = t;
		}
		else
		{
			toggle = t;
		}
	}
}
