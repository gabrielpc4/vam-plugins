using UnityEngine;

public interface TriggerActionHandler
{
	RectTransform CreateTriggerActionDiscreteUI();

	RectTransform CreateTriggerActionTransitionUI();

	void RemoveTriggerAction(TriggerAction ta);

	void DuplicateTriggerAction(TriggerAction ta);
}
