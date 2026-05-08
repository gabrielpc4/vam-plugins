using SimpleJSON;
using UnityEngine.UI;

public abstract class JSONStorableParam
{
	public enum RestoreTime
	{
		Normal,
		Late
	}

	public enum StoreType
	{
		Appearance,
		Physical,
		Any,
		Full
	}

	public bool isStorable = true;

	public bool isRestorable = true;

	public RestoreTime restoreTime;

	public string name;

	public string altName;

	public StoreType storeType;

	protected Button _defaultButton;

	protected Button _defaultButtonAlt;

	public Button defaultButton
	{
		get
		{
			return _defaultButton;
		}
		set
		{
			if (_defaultButton != value)
			{
				if (_defaultButton != null)
				{
					_defaultButton.onClick.RemoveListener(SetValToDefault);
				}
				_defaultButton = value;
				if (_defaultButton != null)
				{
					_defaultButton.onClick.AddListener(SetValToDefault);
				}
			}
		}
	}

	public Button defaultButtonAlt
	{
		get
		{
			return _defaultButtonAlt;
		}
		set
		{
			if (_defaultButtonAlt != value)
			{
				if (_defaultButtonAlt != null)
				{
					_defaultButtonAlt.onClick.RemoveListener(SetValToDefault);
				}
				_defaultButton = value;
				if (_defaultButton != null)
				{
					_defaultButtonAlt.onClick.AddListener(SetValToDefault);
				}
			}
		}
	}

	protected virtual bool NeedsStore(JSONClass jc, bool includePhysical = true, bool includeAppearance = true)
	{
		bool result = false;
		if (isStorable)
		{
			switch (storeType)
			{
				case StoreType.Appearance:
					result = includeAppearance;
					break;
				case StoreType.Physical:
					result = includePhysical;
					break;
				case StoreType.Any:
					result = includeAppearance || includePhysical;
					break;
				case StoreType.Full:
					result = includeAppearance && includePhysical;
					break;
			}
		}
		return result;
	}

	public abstract bool StoreJSON(JSONClass jc, bool includePhysical = true, bool includeAppearance = true, bool forceStore = false);

	public virtual bool NeedsRestore(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true)
	{
		bool result = false;
		if (restoreTime == RestoreTime.Normal && isRestorable)
		{
			switch (storeType)
			{
				case StoreType.Appearance:
					result = restoreAppearance;
					break;
				case StoreType.Physical:
					result = restorePhysical;
					break;
				case StoreType.Any:
					result = restoreAppearance || restorePhysical;
					break;
				case StoreType.Full:
					result = restoreAppearance && restorePhysical;
					break;
			}
		}
		return result;
	}

	public abstract void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true);

	public virtual bool NeedsLateRestore(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true)
	{
		bool result = false;
		if (restoreTime == RestoreTime.Late && isRestorable)
		{
			switch (storeType)
			{
				case StoreType.Appearance:
					result = restoreAppearance;
					break;
				case StoreType.Physical:
					result = restorePhysical;
					break;
				case StoreType.Any:
					result = restoreAppearance || restorePhysical;
					break;
				case StoreType.Full:
					result = restoreAppearance && restorePhysical;
					break;
			}
		}
		return result;
	}

	public abstract void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true);

	public abstract void SetDefaultFromCurrent();

	public abstract void SetValToDefault();

	public void RegisterDefaultButtton(Button b, bool isAlt = false)
	{
		if (isAlt)
		{
			defaultButtonAlt = b;
		}
		else
		{
			defaultButton = b;
		}
	}
}
