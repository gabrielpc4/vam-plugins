namespace Leap.Unity;

public class HandEnableDisable : HandTransitionBehavior
{
	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(value: false);
	}

	protected override void HandReset()
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void HandFinish()
	{
		base.gameObject.SetActive(value: false);
	}
}
