using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class Trigger : TriggerActionHandler
{
	protected List<TriggerActionDiscrete> discreteActionsStart;

	protected List<TriggerActionTransition> transitionActions;

	protected List<TriggerActionDiscrete> discreteActionsEnd;

	protected Button closeTriggerActionsPanelButton;

	protected Button clearActionsButton;

	protected Button addDiscreteActionStartButton;

	protected Button addTransitionActionButton;

	protected Button addDiscreteActionEndButton;

	protected ScrollRectContentManager discreteActionStartContentManager;

	protected ScrollRectContentManager transitionActionsContentManager;

	protected ScrollRectContentManager discreteActionEndContentManager;

	public TriggerHandler handler;

	public Transform triggerPanel;

	public Transform triggerActionsPanel;

	public Transform triggerActionsParent;

	protected Button removeButton;

	protected Button duplicateButton;

	protected Button triggerActionsButton;

	protected JSONStorableString displayNameJSON;

	protected Slider transitionInterpValueSlider;

	protected float _transitionInterpValue;

	protected Toggle activeToggle;

	public bool reverse;

	protected bool _active;

	public string displayName
	{
		get
		{
			return displayNameJSON.val;
		}
		set
		{
			displayNameJSON.val = value;
		}
	}

	public float transitionInterpValue
	{
		get
		{
			return _transitionInterpValue;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (_transitionInterpValue == num)
			{
				return;
			}
			_transitionInterpValue = num;
			if (transitionInterpValueSlider != null)
			{
				transitionInterpValueSlider.value = _transitionInterpValue;
			}
			foreach (TriggerActionTransition transitionAction in transitionActions)
			{
				transitionAction.TriggerInterp(_transitionInterpValue);
			}
		}
	}

	public bool active
	{
		get
		{
			return _active;
		}
		set
		{
			if (_active == value)
			{
				return;
			}
			_active = value;
			if ((bool)activeToggle)
			{
				activeToggle.isOn = _active;
			}
			foreach (TriggerActionTransition transitionAction in transitionActions)
			{
				transitionAction.active = _active;
				if (_active && !reverse)
				{
					transitionAction.InitTriggerStart();
				}
			}
			if ((_active && !reverse) || (!_active && reverse))
			{
				foreach (TriggerActionDiscrete item in discreteActionsStart)
				{
					item.Trigger(reverse);
				}
				return;
			}
			foreach (TriggerActionDiscrete item2 in discreteActionsEnd)
			{
				item2.Trigger(reverse);
			}
		}
	}

	public Trigger()
	{
		displayNameJSON = new JSONStorableString("displayName", string.Empty, SyncDisplayName);
		discreteActionsStart = new List<TriggerActionDiscrete>();
		transitionActions = new List<TriggerActionTransition>();
		discreteActionsEnd = new List<TriggerActionDiscrete>();
	}

	public virtual JSONClass GetJSON()
	{
		JSONClass jSONClass = new JSONClass();
		JSONArray jSONArray = new JSONArray();
		JSONArray jSONArray2 = new JSONArray();
		JSONArray jSONArray3 = new JSONArray();
		displayNameJSON.StoreJSON(jSONClass);
		jSONClass["startActions"] = jSONArray;
		jSONClass["transitionActions"] = jSONArray2;
		jSONClass["endActions"] = jSONArray3;
		foreach (TriggerActionDiscrete item in discreteActionsStart)
		{
			JSONClass jSON = item.GetJSON();
			jSONArray.Add(jSON);
		}
		foreach (TriggerActionTransition transitionAction in transitionActions)
		{
			JSONClass jSON2 = transitionAction.GetJSON();
			jSONArray2.Add(jSON2);
		}
		foreach (TriggerActionDiscrete item2 in discreteActionsEnd)
		{
			JSONClass jSON3 = item2.GetJSON();
			jSONArray3.Add(jSON3);
		}
		return jSONClass;
	}

	protected void ClearActions()
	{
		List<TriggerAction> list = new List<TriggerAction>();
		foreach (TriggerActionDiscrete item in discreteActionsStart)
		{
			list.Add(item);
		}
		foreach (TriggerActionTransition transitionAction in transitionActions)
		{
			list.Add(transitionAction);
		}
		foreach (TriggerActionDiscrete item2 in discreteActionsEnd)
		{
			list.Add(item2);
		}
		foreach (TriggerAction item3 in list)
		{
			item3.Remove();
		}
	}

	public virtual void RestoreFromJSON(JSONClass jc)
	{
		ClearActions();
		displayNameJSON.RestoreFromJSON(jc);
		if (jc["startActions"] != null)
		{
			JSONArray asArray = jc["startActions"].AsArray;
			if (asArray != null)
			{
				foreach (JSONNode item in asArray)
				{
					JSONClass asObject = item.AsObject;
					if (asObject != null)
					{
						TriggerActionDiscrete triggerActionDiscrete = CreateDiscreteActionStartInternal();
						triggerActionDiscrete.RestoreFromJSON(asObject);
					}
				}
			}
		}
		if (jc["transitionActions"] != null)
		{
			JSONArray asArray2 = jc["transitionActions"].AsArray;
			if (asArray2 != null)
			{
				foreach (JSONNode item2 in asArray2)
				{
					JSONClass asObject2 = item2.AsObject;
					if (asObject2 != null)
					{
						TriggerActionTransition triggerActionTransition = CreateTransitionActionInternal();
						triggerActionTransition.RestoreFromJSON(asObject2);
						triggerActionTransition.active = _active;
						triggerActionTransition.TriggerInterp(_transitionInterpValue);
					}
				}
			}
		}
		if (!(jc["endActions"] != null))
		{
			return;
		}
		JSONArray asArray3 = jc["endActions"].AsArray;
		if (!(asArray3 != null))
		{
			return;
		}
		foreach (JSONNode item3 in asArray3)
		{
			JSONClass asObject3 = item3.AsObject;
			if (asObject3 != null)
			{
				TriggerActionDiscrete triggerActionDiscrete2 = CreateDiscreteActionEndInternal();
				triggerActionDiscrete2.RestoreFromJSON(asObject3);
			}
		}
	}

	public virtual void Remove()
	{
		ClearActions();
		if (handler != null)
		{
			DeregisterUI();
			handler.RemoveTrigger(this);
		}
	}

	public virtual void Duplicate()
	{
		if (handler != null)
		{
			handler.DuplicateTrigger(this);
		}
	}

	public virtual void SyncAtomNames()
	{
		foreach (TriggerActionDiscrete item in discreteActionsStart)
		{
			item.SyncAtomName();
		}
		foreach (TriggerActionTransition transitionAction in transitionActions)
		{
			transitionAction.SyncAtomName();
		}
		foreach (TriggerActionDiscrete item2 in discreteActionsEnd)
		{
			item2.SyncAtomName();
		}
	}

	public virtual void Validate()
	{
		List<TriggerActionDiscrete> list = new List<TriggerActionDiscrete>(discreteActionsStart);
		foreach (TriggerActionDiscrete item in list)
		{
			item.Validate();
		}
		List<TriggerActionTransition> list2 = new List<TriggerActionTransition>(transitionActions);
		foreach (TriggerActionTransition item2 in list2)
		{
			item2.Validate();
		}
		list = new List<TriggerActionDiscrete>(discreteActionsEnd);
		foreach (TriggerActionDiscrete item3 in list)
		{
			item3.Validate();
		}
	}

	protected void CreateTriggerActionsPanel()
	{
		if (triggerActionsParent != null)
		{
			triggerActionsPanel = handler.CreateTriggerActionsUI();
			if (triggerActionsPanel != null)
			{
				triggerActionsPanel.SetParent(triggerActionsParent, worldPositionStays: false);
				InitTriggerActionsUI();
			}
		}
		else
		{
			Debug.LogError("Attempted to CreateTriggerActionsPanel when triggerActionsParent was null");
		}
	}

	public virtual void OpenTriggerActionsPanel()
	{
		if (triggerActionsPanel == null)
		{
			CreateTriggerActionsPanel();
		}
		if (triggerActionsPanel != null)
		{
			triggerActionsPanel.gameObject.SetActive(value: true);
		}
	}

	public virtual void CloseTriggerActionsPanel()
	{
		if (triggerActionsPanel != null)
		{
			triggerActionsPanel.gameObject.SetActive(value: false);
		}
	}

	protected void SyncDisplayName(string n)
	{
	}

	public void ForceTransitionsActive()
	{
		foreach (TriggerActionTransition transitionAction in transitionActions)
		{
			if (!transitionAction.active)
			{
				if (!reverse)
				{
					transitionAction.InitTriggerStart();
				}
				transitionAction.active = true;
			}
		}
	}

	public void ForceTransitionsInactive()
	{
		foreach (TriggerActionTransition transitionAction in transitionActions)
		{
			transitionAction.active = false;
		}
	}

	public void Reset()
	{
		_active = true;
		foreach (TriggerActionTransition transitionAction in transitionActions)
		{
			transitionAction.active = _active;
		}
		transitionInterpValue = 0f;
		foreach (TriggerActionTransition transitionAction2 in transitionActions)
		{
			transitionAction2.TriggerInterp(_transitionInterpValue);
		}
		_active = false;
		foreach (TriggerActionTransition transitionAction3 in transitionActions)
		{
			transitionAction3.active = _active;
		}
		if ((bool)activeToggle)
		{
			activeToggle.isOn = _active;
		}
	}

	protected virtual void CreateTriggerActionMiniUI(ScrollRectContentManager contentManager, TriggerAction ta, int index, int totalCount)
	{
		if (!(contentManager != null) || handler == null)
		{
			return;
		}
		RectTransform rectTransform = handler.CreateTriggerActionMiniUI();
		if (rectTransform != null)
		{
			contentManager.AddItem(rectTransform, index);
			ta.triggerActionMiniPanel = rectTransform;
			if (index == -1)
			{
				rectTransform.SetSiblingIndex(0);
			}
			else
			{
				rectTransform.SetSiblingIndex(totalCount - index);
			}
			ta.InitTriggerActionMiniPanelUI();
		}
		else
		{
			Debug.LogError("Failed to create trigger action mini UI");
		}
	}

	public virtual RectTransform CreateTriggerActionDiscreteUI()
	{
		if (triggerActionsParent != null)
		{
			RectTransform rectTransform = handler.CreateTriggerActionDiscreteUI();
			rectTransform.SetParent(triggerActionsParent, worldPositionStays: false);
			rectTransform.gameObject.SetActive(value: false);
			return rectTransform;
		}
		Debug.LogError("Attempted to CreateTriggerActionDiscreteUI when triggerActionsParent was null");
		return null;
	}

	public virtual RectTransform CreateTriggerActionTransitionUI()
	{
		if (triggerActionsParent != null)
		{
			RectTransform rectTransform = handler.CreateTriggerActionTransitionUI();
			rectTransform.SetParent(triggerActionsParent, worldPositionStays: false);
			rectTransform.gameObject.SetActive(value: false);
			return rectTransform;
		}
		Debug.LogError("Attempted to CreateTriggerActionTransitionUI when triggerActionsParent was null");
		return null;
	}

	public virtual TriggerActionDiscrete CreateDiscreteActionStartInternal(int index = -1)
	{
		TriggerActionDiscrete triggerActionDiscrete = new TriggerActionDiscrete();
		triggerActionDiscrete.handler = this;
		if (index == -1)
		{
			discreteActionsStart.Add(triggerActionDiscrete);
		}
		else
		{
			discreteActionsStart.Insert(index, triggerActionDiscrete);
		}
		CreateTriggerActionMiniUI(discreteActionStartContentManager, triggerActionDiscrete, index, discreteActionsStart.Count);
		return triggerActionDiscrete;
	}

	public virtual void CreateDiscreteActionStart()
	{
		CreateDiscreteActionStartInternal();
	}

	public virtual TriggerActionTransition CreateTransitionActionInternal(int index = -1)
	{
		TriggerActionTransition triggerActionTransition = new TriggerActionTransition();
		triggerActionTransition.handler = this;
		if (index == -1)
		{
			transitionActions.Add(triggerActionTransition);
		}
		else
		{
			transitionActions.Insert(index, triggerActionTransition);
		}
		CreateTriggerActionMiniUI(transitionActionsContentManager, triggerActionTransition, index, transitionActions.Count);
		return triggerActionTransition;
	}

	public virtual void CreateTransitionAction()
	{
		TriggerActionTransition triggerActionTransition = CreateTransitionActionInternal();
		triggerActionTransition.active = _active;
		triggerActionTransition.TriggerInterp(_transitionInterpValue);
	}

	public virtual TriggerActionDiscrete CreateDiscreteActionEndInternal(int index = -1)
	{
		TriggerActionDiscrete triggerActionDiscrete = new TriggerActionDiscrete();
		triggerActionDiscrete.handler = this;
		if (index == -1)
		{
			discreteActionsEnd.Add(triggerActionDiscrete);
		}
		else
		{
			discreteActionsEnd.Insert(index, triggerActionDiscrete);
		}
		CreateTriggerActionMiniUI(discreteActionEndContentManager, triggerActionDiscrete, index, discreteActionsEnd.Count);
		return triggerActionDiscrete;
	}

	public virtual void CreateDiscreteActionEnd()
	{
		CreateDiscreteActionEndInternal();
	}

	public void RemoveTriggerAction(TriggerAction ta)
	{
		if (ta is TriggerActionDiscrete)
		{
			if (discreteActionsStart.Remove(ta as TriggerActionDiscrete))
			{
				if (ta.triggerActionMiniPanel != null && discreteActionStartContentManager != null)
				{
					discreteActionStartContentManager.RemoveItem(ta.triggerActionMiniPanel);
				}
			}
			else
			{
				if (!discreteActionsEnd.Remove(ta as TriggerActionDiscrete))
				{
					Debug.LogError("TriggerAction not found. Cannot remove");
					return;
				}
				if (ta.triggerActionMiniPanel != null && discreteActionEndContentManager != null)
				{
					discreteActionEndContentManager.RemoveItem(ta.triggerActionMiniPanel);
				}
			}
		}
		else if (ta is TriggerActionTransition)
		{
			if (!transitionActions.Remove(ta as TriggerActionTransition))
			{
				Debug.LogError("TriggerAction not found. Cannot remove");
				return;
			}
			if (ta.triggerActionMiniPanel != null && transitionActionsContentManager != null)
			{
				transitionActionsContentManager.RemoveItem(ta.triggerActionMiniPanel);
			}
		}
		if (handler != null)
		{
			if (ta.triggerActionPanel != null)
			{
				handler.RemoveTriggerActionUI(ta.triggerActionPanel);
				ta.triggerActionPanel = null;
			}
			if (ta.triggerActionMiniPanel != null)
			{
				handler.RemoveTriggerActionUI(ta.triggerActionMiniPanel);
				ta.triggerActionMiniPanel = null;
			}
		}
	}

	public void DuplicateTriggerAction(TriggerAction ta)
	{
		if (ta is TriggerActionDiscrete)
		{
			TriggerActionDiscrete triggerActionDiscrete = ta as TriggerActionDiscrete;
			int num = discreteActionsStart.IndexOf(triggerActionDiscrete);
			if (num == -1)
			{
				num = discreteActionsEnd.IndexOf(triggerActionDiscrete);
				if (num != -1)
				{
					JSONClass jSON = triggerActionDiscrete.GetJSON();
					TriggerActionDiscrete triggerActionDiscrete2 = CreateDiscreteActionEndInternal(num + 1);
					triggerActionDiscrete2.RestoreFromJSON(jSON);
				}
			}
			else
			{
				JSONClass jSON2 = triggerActionDiscrete.GetJSON();
				TriggerActionDiscrete triggerActionDiscrete3 = CreateDiscreteActionStartInternal(num + 1);
				triggerActionDiscrete3.RestoreFromJSON(jSON2);
			}
		}
		else if (ta is TriggerActionTransition)
		{
			TriggerActionTransition triggerActionTransition = ta as TriggerActionTransition;
			int num2 = transitionActions.IndexOf(triggerActionTransition);
			if (num2 != -1)
			{
				JSONClass jSON3 = triggerActionTransition.GetJSON();
				TriggerActionTransition triggerActionTransition2 = CreateTransitionActionInternal(num2);
				triggerActionTransition2.RestoreFromJSON(jSON3);
				triggerActionTransition2.active = _active;
				triggerActionTransition2.TriggerInterp(_transitionInterpValue);
			}
		}
	}

	public virtual void InitTriggerUI()
	{
		if (triggerPanel != null)
		{
			TriggerUI componentInChildren = triggerPanel.GetComponentInChildren<TriggerUI>();
			if (componentInChildren != null)
			{
				removeButton = componentInChildren.removeButton;
				duplicateButton = componentInChildren.duplicateButton;
				triggerActionsButton = componentInChildren.triggerActionsButton;
				displayNameJSON.inputField = componentInChildren.displayNameField;
				activeToggle = componentInChildren.activeToggle;
				transitionInterpValueSlider = componentInChildren.transitionInterpValueSlider;
			}
			if (removeButton != null)
			{
				removeButton.onClick.AddListener(Remove);
			}
			if (duplicateButton != null)
			{
				duplicateButton.onClick.AddListener(Duplicate);
			}
			if (triggerActionsButton != null)
			{
				triggerActionsButton.onClick.AddListener(OpenTriggerActionsPanel);
			}
			if (activeToggle != null)
			{
				activeToggle.isOn = _active;
			}
			if (transitionInterpValueSlider != null)
			{
				transitionInterpValueSlider.value = _transitionInterpValue;
			}
		}
	}

	public virtual void InitTriggerActionsUI()
	{
		if (!(triggerActionsPanel != null))
		{
			return;
		}
		TriggerActionsPanelUI componentInChildren = triggerActionsPanel.GetComponentInChildren<TriggerActionsPanelUI>();
		if (componentInChildren != null)
		{
			displayNameJSON.text = componentInChildren.triggerDisplayNameText;
			closeTriggerActionsPanelButton = componentInChildren.closeTriggerActionsPanelButton;
			clearActionsButton = componentInChildren.clearActionsButtons;
			addDiscreteActionStartButton = componentInChildren.addDiscreteActionStartButton;
			addTransitionActionButton = componentInChildren.addTransitionActionButton;
			addDiscreteActionEndButton = componentInChildren.addDiscreteActionEndButton;
			discreteActionStartContentManager = componentInChildren.discreteActionsStartContentManager;
			if (discreteActionStartContentManager != null)
			{
				discreteActionStartContentManager.RemoveAllItems();
				for (int i = 0; i < discreteActionsStart.Count; i++)
				{
					CreateTriggerActionMiniUI(discreteActionStartContentManager, discreteActionsStart[i], i, discreteActionsStart.Count);
				}
			}
			transitionActionsContentManager = componentInChildren.transitionActionsContentManager;
			if (transitionActionsContentManager != null)
			{
				transitionActionsContentManager.RemoveAllItems();
				for (int j = 0; j < transitionActions.Count; j++)
				{
					CreateTriggerActionMiniUI(transitionActionsContentManager, transitionActions[j], j, transitionActions.Count);
				}
			}
			discreteActionEndContentManager = componentInChildren.discreteActionsEndContentManager;
			if (discreteActionEndContentManager != null)
			{
				discreteActionEndContentManager.RemoveAllItems();
				for (int k = 0; k < discreteActionsEnd.Count; k++)
				{
					CreateTriggerActionMiniUI(discreteActionEndContentManager, discreteActionsEnd[k], k, discreteActionsEnd.Count);
				}
			}
		}
		if (closeTriggerActionsPanelButton != null)
		{
			closeTriggerActionsPanelButton.onClick.AddListener(CloseTriggerActionsPanel);
		}
		if (clearActionsButton != null)
		{
			clearActionsButton.onClick.AddListener(ClearActions);
		}
		if (addDiscreteActionStartButton != null)
		{
			addDiscreteActionStartButton.onClick.AddListener(CreateDiscreteActionStart);
		}
		if (addTransitionActionButton != null)
		{
			addTransitionActionButton.onClick.AddListener(CreateTransitionAction);
		}
		if (addDiscreteActionEndButton != null)
		{
			addDiscreteActionEndButton.onClick.AddListener(CreateDiscreteActionEnd);
		}
	}

	public virtual void DeregisterUI()
	{
		displayNameJSON.inputField = null;
		if (removeButton != null)
		{
			removeButton.onClick.RemoveListener(Remove);
		}
		if (duplicateButton != null)
		{
			duplicateButton.onClick.RemoveListener(Duplicate);
		}
		if (triggerActionsButton != null)
		{
			triggerActionsButton.onClick.RemoveListener(OpenTriggerActionsPanel);
		}
		if (closeTriggerActionsPanelButton != null)
		{
			closeTriggerActionsPanelButton.onClick.RemoveListener(CloseTriggerActionsPanel);
		}
		if (clearActionsButton != null)
		{
			clearActionsButton.onClick.RemoveListener(ClearActions);
		}
		if (addDiscreteActionStartButton != null)
		{
			addDiscreteActionStartButton.onClick.RemoveListener(CreateDiscreteActionStart);
		}
		if (addTransitionActionButton != null)
		{
			addTransitionActionButton.onClick.RemoveListener(CreateTransitionAction);
		}
		if (addDiscreteActionEndButton != null)
		{
			addDiscreteActionEndButton.onClick.RemoveListener(CreateDiscreteActionEnd);
		}
	}

	public virtual void Update()
	{
	}
}
