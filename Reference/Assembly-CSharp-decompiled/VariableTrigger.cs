using System;
using SimpleJSON;

public class VariableTrigger : JSONStorableTriggerHandler
{
	public Trigger trigger;

	protected JSONStorableFloat floatJSON;

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (trigger != null)
		{
			needsStore = true;
			jSON["trigger"] = trigger.GetJSON();
		}
		return jSON;
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (jc["trigger"] != null)
		{
			JSONClass asObject = jc["trigger"].AsObject;
			if (asObject != null)
			{
				trigger.RestoreFromJSON(asObject);
			}
		}
		else if (setMissingToDefault)
		{
			trigger.RestoreFromJSON(new JSONClass());
		}
	}

	public override void Validate()
	{
		base.Validate();
		if (trigger != null)
		{
			trigger.Validate();
		}
	}

	protected void SyncFloat(float f)
	{
		if (trigger != null)
		{
			trigger.transitionInterpValue = floatJSON.val;
		}
	}

	protected void OnAtomRename(string oldid, string newid)
	{
		if (trigger != null)
		{
			trigger.SyncAtomNames();
		}
	}

	protected virtual void CreateFloatJSON()
	{
		floatJSON = new JSONStorableFloat("value", 0f, SyncFloat, 0f, 1f);
		RegisterFloat(floatJSON);
	}

	protected virtual void Init()
	{
		trigger = new Trigger();
		trigger.handler = this;
		trigger.active = true;
		CreateFloatJSON();
		if ((bool)SuperController.singleton)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	public override void InitUI()
	{
		if (UITransform != null && trigger != null)
		{
			VariableTriggerUI componentInChildren = UITransform.GetComponentInChildren<VariableTriggerUI>();
			if (componentInChildren != null)
			{
				floatJSON.slider = componentInChildren.variableSlider;
				trigger.triggerActionsParent = componentInChildren.transform;
				trigger.triggerPanel = componentInChildren.transform;
				trigger.triggerActionsPanel = componentInChildren.transform;
				trigger.InitTriggerUI();
				trigger.InitTriggerActionsUI();
			}
		}
	}

	public override void InitUIAlt()
	{
		if (UITransformAlt != null)
		{
			VariableTriggerUI componentInChildren = UITransformAlt.GetComponentInChildren<VariableTriggerUI>();
			if (componentInChildren != null)
			{
				floatJSON.sliderAlt = componentInChildren.variableSlider;
			}
		}
	}

	protected virtual void OnEnable()
	{
		if (trigger != null)
		{
			trigger.active = true;
		}
	}

	protected virtual void OnDisable()
	{
		floatJSON.val = 0f;
		trigger.active = false;
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

	protected void OnDestroy()
	{
		if (trigger != null)
		{
			trigger.Remove();
		}
		if ((bool)SuperController.singleton)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}
}
