using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSourceControl : VariableTrigger
{
	public AudioSource audioSource;

	protected Text playingClipNameText;

	protected Text playingClipNameTextAlt;

	protected JSONStorableBool loopJSON;

	protected bool _loop;

	protected JSONStorableFloat volumeJSON;

	protected float _volume = 1f;

	protected JSONStorableFloat pitchJSON;

	protected float _pitch = 1f;

	protected JSONStorableFloat stereoPanJSON;

	protected float _stereoPan;

	protected JSONStorableFloat stereoSpreadJSON;

	protected float _stereoSpread;

	protected JSONStorableFloat minDistanceJSON;

	protected float _minDistance = 0.5f;

	protected JSONStorableFloat maxDistanceJSON;

	protected float _maxDistance = 500f;

	protected JSONStorableFloat spatialBlendJSON;

	protected float _spatialBlend = 1f;

	protected JSONStorableBool spatializeJSON;

	protected bool _spatialize = true;

	protected JSONStorableFloat delayBetweenQueuedClipsJSON;

	protected float _delayBetweenQueuedClips;

	protected JSONStorableFloat volumeTriggerQuicknessJSON;

	protected float _volumeTriggerQuickness = 2.5f;

	protected JSONStorableFloat volumeTriggerMultiplierJSON;

	protected float _volumeTriggerMultiplier = 5f;

	protected float delayTimer;

	protected LinkedList<NamedAudioClip> queue;

	protected NamedAudioClip _playingClip;

	protected JSONStorableActionAudioClip playNextJSONAction;

	protected JSONStorableActionAudioClip playNextClearQueueJSONAction;

	protected JSONStorableActionAudioClip queueClipJSONAction;

	protected JSONStorableActionAudioClip playNowJSONAction;

	protected JSONStorableActionAudioClip playIfClearJSONAction;

	protected JSONStorableActionAudioClip playNowLoopJSONAction;

	protected JSONStorableActionAudioClip playNowClearQueueJSONAction;

	protected JSONStorableAction pauseJSONAction;

	protected JSONStorableAction stopLoopJSONAction;

	protected JSONStorableAction stopJSONAction;

	protected JSONStorableAction unpauseJSONAction;

	protected JSONStorableAction stopAndClearQueueJSONAction;

	protected JSONStorableAction clearQueueJSONAction;

	protected float timeSinceClipFinished;

	public int sampleDataLength = 128;

	private bool wasFrozen;

	public float clipLoudness;

	public float smoothedClipLoudness;

	private float[] clipSampleData;

	public bool loop
	{
		get
		{
			return _loop;
		}
		set
		{
			if (loopJSON != null)
			{
				loopJSON.val = value;
			}
			else if (_loop != value)
			{
				SyncLoop(value);
			}
		}
	}

	public float volume
	{
		get
		{
			return _volume;
		}
		set
		{
			if (volumeJSON != null)
			{
				volumeJSON.val = value;
			}
			else if (_volume != value)
			{
				SyncVolume(value);
			}
		}
	}

	public float pitch
	{
		get
		{
			return _pitch;
		}
		set
		{
			if (pitchJSON != null)
			{
				pitchJSON.val = value;
			}
			else if (_pitch != value)
			{
				SyncPitch(value);
			}
		}
	}

	public float stereoPan
	{
		get
		{
			return _stereoPan;
		}
		set
		{
			if (stereoPanJSON != null)
			{
				stereoPanJSON.val = value;
			}
			else if (_stereoPan != value)
			{
				SyncStereoPan(value);
			}
		}
	}

	public float stereoSpread
	{
		get
		{
			return _stereoSpread;
		}
		set
		{
			if (stereoSpreadJSON != null)
			{
				stereoSpreadJSON.val = value;
			}
			else if (_stereoSpread != value)
			{
				SyncStereoSpread(value);
			}
		}
	}

	public float minDistance
	{
		get
		{
			return _minDistance;
		}
		set
		{
			if (minDistanceJSON != null)
			{
				minDistanceJSON.val = value;
			}
			else if (_minDistance != value)
			{
				SyncMinDistance(value);
			}
		}
	}

	public float maxDistance
	{
		get
		{
			return _maxDistance;
		}
		set
		{
			if (maxDistanceJSON != null)
			{
				maxDistanceJSON.val = value;
			}
			else if (_maxDistance != value)
			{
				SyncMaxDistance(value);
			}
		}
	}

	public float spatialBlend
	{
		get
		{
			return _spatialBlend;
		}
		set
		{
			if (spatialBlendJSON != null)
			{
				spatialBlendJSON.val = value;
			}
			else if (_spatialBlend != value)
			{
				SyncSpatialBlend(value);
			}
		}
	}

	public bool spatialize
	{
		get
		{
			return _spatialize;
		}
		set
		{
			if (spatializeJSON != null)
			{
				spatializeJSON.val = value;
			}
			else if (_spatialize != value)
			{
				SyncSpatialize(value);
			}
		}
	}

	public float delayBetweenQueuedClips
	{
		get
		{
			return _delayBetweenQueuedClips;
		}
		set
		{
			if (delayBetweenQueuedClipsJSON != null)
			{
				delayBetweenQueuedClipsJSON.val = value;
			}
			else if (_delayBetweenQueuedClips != value)
			{
				SyncDelayBetweenQueuedClips(value);
			}
		}
	}

	public float volumeTriggerQuickness
	{
		get
		{
			return _volumeTriggerQuickness;
		}
		set
		{
			if (volumeTriggerQuicknessJSON != null)
			{
				volumeTriggerQuicknessJSON.val = value;
			}
			else if (_volumeTriggerQuickness != value)
			{
				SyncVolumeTriggerQuickness(value);
			}
		}
	}

	public float volumeTriggerMultiplier
	{
		get
		{
			return _volumeTriggerMultiplier;
		}
		set
		{
			if (volumeTriggerMultiplierJSON != null)
			{
				volumeTriggerMultiplierJSON.val = value;
			}
			else if (_volumeTriggerMultiplier != value)
			{
				SyncVolumeTriggerMultiplier(value);
			}
		}
	}

	public NamedAudioClip playingClip => _playingClip;

	protected void SyncLoop(bool b)
	{
		_loop = b;
		if (audioSource != null)
		{
			audioSource.loop = _loop;
		}
	}

	protected void SyncVolume(float f)
	{
		_volume = f;
		if (audioSource != null)
		{
			audioSource.volume = _volume;
		}
	}

	protected void SyncPitch(float f)
	{
		_pitch = f;
		if (audioSource != null)
		{
			audioSource.pitch = _pitch;
		}
	}

	protected void SyncStereoPan(float f)
	{
		_stereoPan = f;
		if (audioSource != null)
		{
			audioSource.panStereo = _stereoPan;
		}
	}

	protected void SyncStereoSpread(float f)
	{
		_stereoSpread = f;
		if (audioSource != null)
		{
			audioSource.spread = _stereoSpread;
		}
	}

	protected void SyncMinDistance(float f)
	{
		_minDistance = f;
		if (_maxDistance < _minDistance + 0.1f)
		{
			maxDistance = _minDistance + 0.1f;
		}
		if (audioSource != null)
		{
			audioSource.minDistance = _minDistance;
		}
	}

	protected void SyncMaxDistance(float f)
	{
		_maxDistance = f;
		if (_maxDistance < _minDistance + 0.1f)
		{
			minDistance = _maxDistance - 0.1f;
		}
		if (audioSource != null)
		{
			audioSource.maxDistance = _maxDistance;
		}
	}

	protected void SyncSpatialBlend(float f)
	{
		_spatialBlend = f;
		if (audioSource != null)
		{
			audioSource.spatialBlend = _spatialBlend;
		}
	}

	protected void SyncSpatialize(bool b)
	{
		_spatialize = b;
		if (audioSource != null)
		{
			audioSource.spatialize = _spatialize;
		}
	}

	protected void SyncDelayBetweenQueuedClips(float f)
	{
		_delayBetweenQueuedClips = f;
	}

	protected void SyncVolumeTriggerQuickness(float f)
	{
		_volumeTriggerQuickness = f;
	}

	protected void SyncVolumeTriggerMultiplier(float f)
	{
		_volumeTriggerMultiplier = f;
	}

	public void PlayNext(NamedAudioClip nac)
	{
		if (audioSource != null && nac.clipToPlay != null)
		{
			if (audioSource.isPlaying)
			{
				queue.AddFirst(nac);
			}
			else
			{
				PlayClip(nac);
			}
		}
	}

	public void PlayNextClearQueue(NamedAudioClip nac)
	{
		queue.Clear();
		QueueClip(nac);
	}

	public void QueueClip(NamedAudioClip nac)
	{
		if (audioSource != null && nac.clipToPlay != null)
		{
			if (audioSource.isPlaying)
			{
				queue.AddLast(nac);
			}
			else
			{
				PlayClip(nac);
			}
		}
	}

	public void PlayNow(NamedAudioClip nac)
	{
		PlayClip(nac);
	}

	public void PlayIfClear(NamedAudioClip nac)
	{
		if (audioSource != null && !audioSource.isPlaying)
		{
			PlayClip(nac);
		}
	}

	public void PlayNowLoop(NamedAudioClip nac)
	{
		queue.Clear();
		PlayClip(nac, loopClip: true);
	}

	public void PlayNowClearQueue(NamedAudioClip nac)
	{
		queue.Clear();
		PlayClip(nac);
	}

	protected virtual void PlayClip(NamedAudioClip nac, bool loopClip = false)
	{
		if (audioSource != null && nac.clipToPlay != null)
		{
			loop = loopClip;
			_playingClip = nac;
			timeSinceClipFinished = 0f;
			audioSource.clip = nac.clipToPlay;
			audioSource.Play();
			if (playingClipNameText != null)
			{
				playingClipNameText.text = _playingClip.displayName;
			}
			if (playingClipNameTextAlt != null)
			{
				playingClipNameTextAlt.text = _playingClip.displayName;
			}
		}
	}

	public void Pause()
	{
		if (audioSource != null)
		{
			audioSource.Pause();
		}
	}

	public void StopLoop()
	{
		loop = false;
	}

	public void Stop()
	{
		if (audioSource != null)
		{
			audioSource.Stop();
		}
	}

	public void UnPause()
	{
		if (audioSource != null)
		{
			audioSource.UnPause();
		}
	}

	public void StopAndClearQueue()
	{
		queue.Clear();
		Stop();
	}

	public void ClearQueue()
	{
		queue.Clear();
	}

	protected override void CreateFloatJSON()
	{
		floatJSON = new JSONStorableFloat("audioVolume", 0f, base.SyncFloat, 0f, 1f, constrain: true, interactable: false);
	}

	protected override void Init()
	{
		base.Init();
		queue = new LinkedList<NamedAudioClip>();
		clipSampleData = new float[sampleDataLength];
		if (audioSource != null)
		{
			_loop = audioSource.loop;
			_volume = audioSource.volume;
			_pitch = audioSource.pitch;
			_stereoPan = audioSource.panStereo;
			_minDistance = audioSource.minDistance;
			_maxDistance = audioSource.maxDistance;
			_spatialBlend = audioSource.spatialBlend;
			_stereoSpread = audioSource.spread;
			_spatialize = audioSource.spatialize;
			loopJSON = new JSONStorableBool("loop", _loop, SyncLoop);
			RegisterBool(loopJSON);
			volumeJSON = new JSONStorableFloat("volume", _volume, SyncVolume, 0f, 1f);
			RegisterFloat(volumeJSON);
			pitchJSON = new JSONStorableFloat("pitch", _pitch, SyncPitch, -3f, 3f);
			RegisterFloat(pitchJSON);
			stereoPanJSON = new JSONStorableFloat("stereoPan", _stereoPan, SyncStereoPan, -1f, 1f);
			RegisterFloat(stereoPanJSON);
			minDistanceJSON = new JSONStorableFloat("minDistance", _minDistance, SyncMinDistance, 0f, 10f);
			RegisterFloat(minDistanceJSON);
			maxDistanceJSON = new JSONStorableFloat("maxDistance", _maxDistance, SyncMaxDistance, 1f, 500f);
			RegisterFloat(maxDistanceJSON);
			spatialBlendJSON = new JSONStorableFloat("spatialBlend", _spatialBlend, SyncSpatialBlend, 0f, 1f);
			RegisterFloat(spatialBlendJSON);
			stereoSpreadJSON = new JSONStorableFloat("stereoSpread", _stereoSpread, SyncStereoSpread, 0f, 360f);
			RegisterFloat(stereoSpreadJSON);
			spatializeJSON = new JSONStorableBool("spatialize", _spatialize, SyncSpatialize);
			RegisterBool(spatializeJSON);
			delayBetweenQueuedClipsJSON = new JSONStorableFloat("delayBetweenQueuedClips", _delayBetweenQueuedClips, SyncDelayBetweenQueuedClips, 0f, 10f);
			RegisterFloat(delayBetweenQueuedClipsJSON);
			volumeTriggerQuicknessJSON = new JSONStorableFloat("volumeTriggerQuickness", _volumeTriggerQuickness, SyncVolumeTriggerQuickness, 1f, 50f);
			RegisterFloat(volumeTriggerQuicknessJSON);
			volumeTriggerMultiplierJSON = new JSONStorableFloat("volumeTriggerMultiplier", _volumeTriggerMultiplier, SyncVolumeTriggerMultiplier, 0f, 100f);
			RegisterFloat(volumeTriggerMultiplierJSON);
			playIfClearJSONAction = new JSONStorableActionAudioClip("PlayIfClear", PlayIfClear);
			RegisterAudioClipAction(playIfClearJSONAction);
			playNowJSONAction = new JSONStorableActionAudioClip("PlayNow", PlayNow);
			RegisterAudioClipAction(playNowJSONAction);
			playNowLoopJSONAction = new JSONStorableActionAudioClip("PlayNowLoop", PlayNowLoop);
			RegisterAudioClipAction(playNowLoopJSONAction);
			playNowClearQueueJSONAction = new JSONStorableActionAudioClip("PlayNowClearQueue", PlayNowClearQueue);
			RegisterAudioClipAction(playNowClearQueueJSONAction);
			playNextJSONAction = new JSONStorableActionAudioClip("PlayNext", PlayNext);
			RegisterAudioClipAction(playNextJSONAction);
			playNextClearQueueJSONAction = new JSONStorableActionAudioClip("PlayNextClearQueue", PlayNextClearQueue);
			RegisterAudioClipAction(playNextClearQueueJSONAction);
			queueClipJSONAction = new JSONStorableActionAudioClip("Queue", QueueClip);
			RegisterAudioClipAction(queueClipJSONAction);
			pauseJSONAction = new JSONStorableAction("Pause", Pause);
			RegisterAction(pauseJSONAction);
			unpauseJSONAction = new JSONStorableAction("UnPause", UnPause);
			RegisterAction(unpauseJSONAction);
			stopLoopJSONAction = new JSONStorableAction("StopLoop", StopLoop);
			RegisterAction(stopLoopJSONAction);
			stopJSONAction = new JSONStorableAction("Stop", Stop);
			RegisterAction(stopJSONAction);
			stopAndClearQueueJSONAction = new JSONStorableAction("StopAndClearQueue", StopAndClearQueue);
			RegisterAction(stopAndClearQueueJSONAction);
			clearQueueJSONAction = new JSONStorableAction("ClearQueue", ClearQueue);
			RegisterAction(clearQueueJSONAction);
		}
	}

	public override void InitUI()
	{
		base.InitUI();
		if (!(UITransform != null))
		{
			return;
		}
		AudioSourceControlUI componentInChildren = UITransform.GetComponentInChildren<AudioSourceControlUI>();
		if (componentInChildren != null)
		{
			playingClipNameText = componentInChildren.playingClipNameText;
			if (_playingClip != null && playingClipNameText != null)
			{
				playingClipNameText.text = _playingClip.displayName;
			}
			loopJSON.toggle = componentInChildren.loopToggle;
			volumeJSON.slider = componentInChildren.volumeSlider;
			pitchJSON.slider = componentInChildren.pitchSlider;
			stereoPanJSON.slider = componentInChildren.stereoPanSlider;
			minDistanceJSON.slider = componentInChildren.minDistanceSlider;
			maxDistanceJSON.slider = componentInChildren.maxDistanceSlider;
			spatialBlendJSON.slider = componentInChildren.spatialBlendSlider;
			stereoSpreadJSON.slider = componentInChildren.stereoSpreadSlider;
			spatializeJSON.toggle = componentInChildren.spatializeToggle;
			delayBetweenQueuedClipsJSON.slider = componentInChildren.delayBetweenQueuedClipsSlider;
			volumeTriggerQuicknessJSON.slider = componentInChildren.volumeTriggerQuicknessSlider;
			volumeTriggerMultiplierJSON.slider = componentInChildren.volumeTriggerMultiplierSlider;
		}
	}

	public override void InitUIAlt()
	{
		base.InitUIAlt();
		if (!(UITransformAlt != null))
		{
			return;
		}
		AudioSourceControlUI componentInChildren = UITransformAlt.GetComponentInChildren<AudioSourceControlUI>();
		if (componentInChildren != null)
		{
			playingClipNameTextAlt = componentInChildren.playingClipNameText;
			if (_playingClip != null && playingClipNameTextAlt != null)
			{
				playingClipNameTextAlt.text = _playingClip.displayName;
			}
			loopJSON.toggleAlt = componentInChildren.loopToggle;
			volumeJSON.sliderAlt = componentInChildren.volumeSlider;
			pitchJSON.sliderAlt = componentInChildren.pitchSlider;
			stereoPanJSON.sliderAlt = componentInChildren.stereoPanSlider;
			minDistanceJSON.sliderAlt = componentInChildren.minDistanceSlider;
			maxDistanceJSON.sliderAlt = componentInChildren.maxDistanceSlider;
			spatialBlendJSON.sliderAlt = componentInChildren.spatialBlendSlider;
			stereoSpreadJSON.sliderAlt = componentInChildren.stereoSpreadSlider;
			spatializeJSON.toggleAlt = componentInChildren.spatializeToggle;
			delayBetweenQueuedClipsJSON.sliderAlt = componentInChildren.delayBetweenQueuedClipsSlider;
			volumeTriggerQuicknessJSON.sliderAlt = componentInChildren.volumeTriggerQuicknessSlider;
			volumeTriggerMultiplierJSON.sliderAlt = componentInChildren.volumeTriggerMultiplierSlider;
		}
	}

	protected virtual void Update()
	{
		if (!(audioSource != null))
		{
			return;
		}
		if (SuperController.singleton != null && SuperController.singleton.freezeAnimation)
		{
			if (audioSource.isPlaying)
			{
				wasFrozen = true;
				audioSource.Pause();
			}
		}
		else if (wasFrozen)
		{
			wasFrozen = false;
			audioSource.UnPause();
		}
		else
		{
			if (audioSource.isPlaying)
			{
				timeSinceClipFinished = 0f;
				audioSource.GetOutputData(clipSampleData, 0);
				clipLoudness = 0f;
				float[] array = clipSampleData;
				foreach (float f in array)
				{
					clipLoudness += Mathf.Abs(f);
				}
				clipLoudness /= sampleDataLength;
			}
			else
			{
				clipLoudness = 0f;
				_playingClip = null;
				timeSinceClipFinished += Time.unscaledDeltaTime;
			}
			smoothedClipLoudness = Mathf.Lerp(floatJSON.val, Mathf.Clamp01(clipLoudness * _volumeTriggerMultiplier), Time.deltaTime * _volumeTriggerQuickness);
			floatJSON.val = smoothedClipLoudness;
		}
		if (queue != null && queue.Count > 0 && timeSinceClipFinished > delayBetweenQueuedClips)
		{
			NamedAudioClip value = queue.First.Value;
			queue.RemoveFirst();
			PlayClip(value);
		}
	}
}
