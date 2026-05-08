public class PossessControl : JSONStorable
{
	protected JSONStorableAction startHandPossessJSON;

	protected JSONStorableAction stopHandPossessJSON;

	protected JSONStorableAction stopAllPossessJSON;

	protected void StartHandPossess()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectModePossess(excludeHeadClear: true);
		}
	}

	protected void StopHandPossess()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ClearPossess(excludeHeadClear: true);
		}
	}

	protected void StopAllPossess()
	{
		if (SuperController.singleton != null)
		{
			SuperController.singleton.ClearPossess();
		}
	}

	protected void Init()
	{
		startHandPossessJSON = new JSONStorableAction("StartHandPossess", StartHandPossess);
		RegisterAction(startHandPossessJSON);
		stopHandPossessJSON = new JSONStorableAction("StopHandPossess", StopHandPossess);
		RegisterAction(stopHandPossessJSON);
		stopAllPossessJSON = new JSONStorableAction("StopAllPossess", StopAllPossess);
		RegisterAction(stopAllPossessJSON);
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
		}
	}
}
