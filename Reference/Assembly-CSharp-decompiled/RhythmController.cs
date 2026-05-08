using UnityEngine;

public class RhythmController : MonoBehaviour
{
	public Atom containingAtom;

	public RhythmTool rhythmTool;

	public Frame[] low;

	public Frame[] mid;

	public Frame[] high;

	private int index;

	protected bool _wasInit;

	protected void Init()
	{
		if (!_wasInit)
		{
			_wasInit = true;
			rhythmTool.Init(this, preCalculate: false);
		}
	}

	private void Awake()
	{
		Init();
	}

	public void StartSong(AudioClip ac)
	{
		Init();
		rhythmTool.NewSong(ac);
		rhythmTool.Play();
		low = rhythmTool.GetResults("Low");
		mid = rhythmTool.GetResults("Mid");
		high = rhythmTool.GetResults("High");
	}

	private void Update()
	{
		rhythmTool.Update();
	}
}
