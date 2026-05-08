using UnityEngine;

public class RhythmAudioSourceControl : AudioSourceControl
{
	public RhythmController rhythmController;

	public AudioClip startClip;

	protected override void PlayClip(NamedAudioClip nac, bool loopClip = false)
	{
		if (rhythmController != null && audioSource != null && nac.clipToPlay != null)
		{
			base.loop = loopClip;
			_playingClip = nac;
			timeSinceClipFinished = 0f;
			rhythmController.StartSong(nac.clipToPlay);
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			if (audioSource != null && startClip != null)
			{
				rhythmController.StartSong(startClip);
			}
		}
	}

	protected override void OnEnable()
	{
	}

	protected override void Update()
	{
		base.Update();
	}
}
