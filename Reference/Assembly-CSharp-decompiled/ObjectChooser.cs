using System.Collections.Generic;
using UnityEngine;

public class ObjectChooser : JSONStorable
{
	public Transform choiceContainer;

	public string startingChoiceName;

	protected ObjectChoice[] choices;

	protected List<string> choiceNames;

	protected Dictionary<string, ObjectChoice> choiceNameToObjectChoice;

	protected JSONStorableStringChooser chooserJSON;

	public ObjectChoice CurrentChoice { get; private set; }

	protected void SyncChoice(string s)
	{
		if (choiceNameToObjectChoice.TryGetValue(s, out var value))
		{
			if (CurrentChoice != null)
			{
				CurrentChoice.gameObject.SetActive(value: false);
			}
			CurrentChoice = value;
			value.gameObject.SetActive(value: true);
		}
	}

	protected virtual void Init()
	{
		choiceNames = new List<string>();
		if (choiceContainer != null)
		{
			choices = choiceContainer.GetComponentsInChildren<ObjectChoice>(includeInactive: true);
		}
		else
		{
			choices = GetComponentsInChildren<ObjectChoice>(includeInactive: true);
		}
		choiceNameToObjectChoice = new Dictionary<string, ObjectChoice>();
		ObjectChoice[] array = choices;
		foreach (ObjectChoice objectChoice in array)
		{
			choiceNames.Add(objectChoice.displayName);
			choiceNameToObjectChoice.Add(objectChoice.displayName, objectChoice);
			if (startingChoiceName == null || startingChoiceName == string.Empty)
			{
				if (objectChoice.gameObject.activeSelf)
				{
					CurrentChoice = objectChoice;
					startingChoiceName = CurrentChoice.displayName;
				}
			}
			else if (objectChoice.displayName == startingChoiceName)
			{
				CurrentChoice = objectChoice;
				objectChoice.gameObject.SetActive(value: true);
			}
			else
			{
				objectChoice.gameObject.SetActive(value: false);
			}
		}
		chooserJSON = new JSONStorableStringChooser("choiceName", choiceNames, startingChoiceName, null, SyncChoice);
		RegisterStringChooser(chooserJSON);
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		ObjectChoice[] array = choices;
		foreach (ObjectChoice objectChoice in array)
		{
			JSONStorable[] componentsInChildren = objectChoice.GetComponentsInChildren<JSONStorable>(includeInactive: true);
			JSONStorable[] array2 = componentsInChildren;
			foreach (JSONStorable jSONStorable in array2)
			{
				jSONStorable.SetUI(UITransform);
			}
		}
		ObjectChooserUI componentInChildren = UITransform.GetComponentInChildren<ObjectChooserUI>();
		if (componentInChildren != null)
		{
			chooserJSON.popup = componentInChildren.chooserPopup;
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		ObjectChoice[] array = choices;
		foreach (ObjectChoice objectChoice in array)
		{
			JSONStorable[] componentsInChildren = objectChoice.GetComponentsInChildren<JSONStorable>(includeInactive: true);
			JSONStorable[] array2 = componentsInChildren;
			foreach (JSONStorable jSONStorable in array2)
			{
				jSONStorable.SetUIAlt(UITransformAlt);
			}
		}
		ObjectChooserUI componentInChildren = UITransformAlt.GetComponentInChildren<ObjectChooserUI>();
		if (componentInChildren != null)
		{
			chooserJSON.popupAlt = componentInChildren.chooserPopup;
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
