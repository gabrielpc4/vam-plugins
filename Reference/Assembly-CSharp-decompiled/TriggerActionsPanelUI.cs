using UnityEngine.UI;

public class TriggerActionsPanelUI : UIProvider
{
	public Text triggerDisplayNameText;

	public Button closeTriggerActionsPanelButton;

	public Button clearActionsButtons;

	public Button addDiscreteActionStartButton;

	public Button addTransitionActionButton;

	public Button addDiscreteActionEndButton;

	public ScrollRectContentManager discreteActionsStartContentManager;

	public ScrollRectContentManager transitionActionsContentManager;

	public ScrollRectContentManager discreteActionsEndContentManager;
}
