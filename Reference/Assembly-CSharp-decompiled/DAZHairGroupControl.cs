using System.Collections.Generic;
using UnityEngine;

public class DAZHairGroupControl : JSONStorable
{
	protected DAZHairGroup _hairItem;

	public JSONStorableBool disableAnatomyJSON;

	public DAZHairGroup hairItem
	{
		get
		{
			return _hairItem;
		}
		set
		{
			if (_hairItem != value)
			{
				_hairItem = value;
				SyncHairGroup();
			}
		}
	}

	protected void SyncHairGroup()
	{
		if (_hairItem != null)
		{
			disableAnatomyJSON = new JSONStorableBool("disableAnatomy", _hairItem.disableAnatomy, SyncDisableAnatomy);
			RegisterBool(disableAnatomyJSON);
		}
	}

	public void Delete()
	{
		if (hairItem != null)
		{
			hairItem.transform.SetParent(null);
			Object.Destroy(hairItem.gameObject);
		}
	}

	public void RefreshHairItemThumbnail(string dynamicItemPath)
	{
		if (_hairItem != null)
		{
			_hairItem.RefreshHairItemThumbnail(dynamicItemPath);
		}
	}

	public void RefreshHairItems()
	{
		if (_hairItem != null)
		{
			_hairItem.RefreshHairItems();
		}
	}

	public bool IsHairItemUIDAvailable(string uid)
	{
		if (_hairItem != null)
		{
			return _hairItem.IsHairUIDAvailable(uid);
		}
		return false;
	}

	public HashSet<string> GetAllHairOtherTags()
	{
		if (_hairItem != null)
		{
			return _hairItem.GetAllHairOtherTags();
		}
		return null;
	}

	protected void SyncDisableAnatomy(bool b)
	{
		if (_hairItem != null)
		{
			_hairItem.disableAnatomy = b;
			_hairItem.SyncHairAdjustments();
		}
	}

	protected void Init()
	{
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (!(t != null))
		{
			return;
		}
		DAZDynamicItemLabelUI componentInChildren = t.GetComponentInChildren<DAZDynamicItemLabelUI>(includeInactive: true);
		if (_hairItem != null && componentInChildren != null && componentInChildren.label != null)
		{
			componentInChildren.label.text = _hairItem.displayName;
		}
		ObjectChoice[] componentsInChildren = GetComponentsInChildren<ObjectChoice>(includeInactive: true);
		ObjectChoice[] array = componentsInChildren;
		foreach (ObjectChoice objectChoice in array)
		{
			JSONStorable[] componentsInChildren2 = objectChoice.GetComponentsInChildren<JSONStorable>(includeInactive: true);
			JSONStorable[] array2 = componentsInChildren2;
			foreach (JSONStorable jSONStorable in array2)
			{
				if (isAlt)
				{
					jSONStorable.SetUIAlt(t);
				}
				else
				{
					jSONStorable.SetUI(t);
				}
			}
		}
		DAZHairGroupControlUI componentInChildren2 = t.GetComponentInChildren<DAZHairGroupControlUI>(includeInactive: true);
		if (!(componentInChildren2 != null))
		{
			return;
		}
		if (disableAnatomyJSON != null)
		{
			if (componentInChildren2.disableAnatomyToggle != null)
			{
				componentInChildren2.disableAnatomyToggle.gameObject.SetActive(value: true);
			}
			disableAnatomyJSON.RegisterToggle(componentInChildren2.disableAnatomyToggle, isAlt);
		}
		else if (componentInChildren2.disableAnatomyToggle != null)
		{
			componentInChildren2.disableAnatomyToggle.gameObject.SetActive(value: false);
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
}
