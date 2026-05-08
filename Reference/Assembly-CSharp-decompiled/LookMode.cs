using System;
using System.Collections.Generic;
using UnityEngine;

public class EyesControl : JSONStorable
{
	public enum LookMode
	{
		None,
		Player,
		Target
	}

	protected JSONStorableStringChooser currentLookModeJSON;

	[SerializeField]
	protected LookMode _currentLookMode = LookMode.Player;

	public LookAtWithLimits leftLookAtWithLimits;

	public LookAtWithLimits rightLookAtWithLimits;

	[SerializeField]
	protected Transform _lookAt;

	public LookMode currentLookMode
	{
		get
		{
			return _currentLookMode;
		}
		set
		{
			if (currentLookModeJSON != null)
			{
				currentLookModeJSON.val = value.ToString();
			}
			else if (_currentLookMode != value)
			{
				SetLookMode(value.ToString());
			}
		}
	}

	public Transform lookAt
	{
		get
		{
			return _lookAt;
		}
		set
		{
			if (_lookAt != value)
			{
				_lookAt = value;
				SyncLookMode();
			}
		}
	}

	protected void SyncLookMode()
	{
		switch (_currentLookMode)
		{
			case LookMode.None:
				if (leftLookAtWithLimits != null)
				{
					leftLookAtWithLimits.enabled = false;
				}
				if (rightLookAtWithLimits != null)
				{
					rightLookAtWithLimits.enabled = false;
				}
				break;
			case LookMode.Player:
				if (leftLookAtWithLimits != null)
				{
					leftLookAtWithLimits.enabled = true;
					leftLookAtWithLimits.lookAtCameraLocation = CameraTarget.CameraLocation.Center;
				}
				if (rightLookAtWithLimits != null)
				{
					rightLookAtWithLimits.enabled = true;
					rightLookAtWithLimits.lookAtCameraLocation = CameraTarget.CameraLocation.Center;
				}
				break;
			case LookMode.Target:
				if (leftLookAtWithLimits != null)
				{
					leftLookAtWithLimits.enabled = true;
					leftLookAtWithLimits.lookAtCameraLocation = CameraTarget.CameraLocation.None;
					if (_lookAt != null)
					{
						leftLookAtWithLimits.target = _lookAt;
					}
					else
					{
						leftLookAtWithLimits.target = null;
					}
				}
				if (rightLookAtWithLimits != null)
				{
					rightLookAtWithLimits.enabled = true;
					rightLookAtWithLimits.lookAtCameraLocation = CameraTarget.CameraLocation.None;
					if (_lookAt != null)
					{
						rightLookAtWithLimits.target = _lookAt;
					}
					else
					{
						rightLookAtWithLimits.target = null;
					}
				}
				break;
		}
	}

	public void SetLookMode(string lookModeString)
	{
		try
		{
			LookMode lookMode = (LookMode)Enum.Parse(typeof(LookMode), lookModeString);
			_currentLookMode = lookMode;
			SyncLookMode();
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set look mode type to " + lookModeString + " which is not a valid type");
		}
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			EyesControlUI componentInChildren = t.GetComponentInChildren<EyesControlUI>();
			if (componentInChildren != null)
			{
				currentLookModeJSON.RegisterPopup(componentInChildren.lookModePopup, isAlt);
			}
		}
	}

	protected void Init()
	{
		List<string> choicesList = new List<string>(Enum.GetNames(typeof(LookMode)));
		currentLookModeJSON = new JSONStorableStringChooser("lookMode", choicesList, _currentLookMode.ToString(), "Eyes Look At", SetLookMode);
		currentLookModeJSON.storeType = JSONStorableParam.StoreType.Physical;
		RegisterStringChooser(currentLookModeJSON);
		SyncLookMode();
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
}
