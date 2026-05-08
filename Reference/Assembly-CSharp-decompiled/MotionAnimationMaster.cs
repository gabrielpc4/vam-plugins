using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class MotionAnimationMaster : JSONStorableTriggerHandler, AnimationTimelineTriggerHandler
{
	protected List<MotionAnimationControl> controllers;

	public Slider playbackCounterSlider;

	protected float _playbackCounter;

	protected float _lastPlaybackCounter;

	public Slider startTimestepSlider;

	public Slider stopTimestepSlider;

	public bool freeze;

	protected float _loopbackCounter;

	public Slider loopbackTimeSlider;

	[SerializeField]
	protected float _loopbackTime = 1f;

	public Toggle loopToggle;

	[SerializeField]
	protected bool _loop;

	public Toggle autoRecordStopToggle;

	protected bool _ignoreAutoRecordStop;

	[SerializeField]
	protected bool _autoRecordStop = true;

	public Slider playbackSpeedSlider;

	[SerializeField]
	protected float _playbackSpeed = 1f;

	public Toggle showRecordPathsToggle;

	[SerializeField]
	protected bool _showRecordPaths;

	public Toggle showStartMarkersToggle;

	[SerializeField]
	protected bool _showStartMarkers;

	protected List<AnimationTimelineTrigger> triggers;

	protected List<AnimationTimelineTrigger> reverseTriggers;

	public RectTransform triggerActionsParent;

	public RectTransform triggerPrefab;

	public ScrollRectContentManager triggerContentManager;

	public Button clearAllTriggersButton;

	public Button addTriggerButton;

	protected float _lastRecordTime;

	public float recordInterval = 0.02f;

	protected bool _isRecording;

	protected bool _isPlaying;

	protected bool _isLoopingBack;

	protected float _recordedLength;

	public float playbackCounter
	{
		get
		{
			return _playbackCounter;
		}
		set
		{
			if (!_isRecording)
			{
				InternalSetPlaybackCounter(value, manualSet: true);
			}
		}
	}

	public float loopbackTime
	{
		get
		{
			return _loopbackTime;
		}
		set
		{
			if (_loopbackTime != value)
			{
				_loopbackTime = value;
				if (loopbackTimeSlider != null)
				{
					loopbackTimeSlider.value = value;
				}
			}
		}
	}

	public bool loop
	{
		get
		{
			return _loop;
		}
		set
		{
			if (_loop != value)
			{
				_loop = value;
				if (loopToggle.isOn != value)
				{
					loopToggle.isOn = value;
				}
			}
		}
	}

	public bool autoRecordStop
	{
		get
		{
			return _autoRecordStop;
		}
		set
		{
			if (_autoRecordStop != value)
			{
				_autoRecordStop = value;
				if (autoRecordStopToggle.isOn != value)
				{
					autoRecordStopToggle.isOn = value;
				}
			}
		}
	}

	public float playbackSpeed
	{
		get
		{
			return _playbackSpeed;
		}
		set
		{
			if (_playbackSpeed != value)
			{
				_playbackSpeed = value;
				if (playbackSpeedSlider != null)
				{
					playbackSpeedSlider.value = value;
				}
			}
		}
	}

	public bool showRecordPaths
	{
		get
		{
			return _showRecordPaths;
		}
		set
		{
			if (_showRecordPaths != value)
			{
				_showRecordPaths = value;
				if (showRecordPathsToggle.isOn != value)
				{
					showRecordPathsToggle.isOn = value;
				}
				SyncShowRecordPaths();
			}
		}
	}

	public bool showStartMarkers
	{
		get
		{
			return _showStartMarkers;
		}
		set
		{
			if (_showStartMarkers != value)
			{
				_showStartMarkers = value;
				if (showStartMarkersToggle.isOn != value)
				{
					showStartMarkersToggle.isOn = value;
				}
			}
		}
	}

	protected float recordedLength
	{
		get
		{
			return _recordedLength;
		}
		set
		{
			_recordedLength = value;
			if (_recordedLength < 0f)
			{
				_recordedLength = 0f;
			}
			if (playbackCounterSlider != null)
			{
				playbackCounterSlider.maxValue = _recordedLength;
			}
			if (startTimestepSlider != null)
			{
				startTimestepSlider.maxValue = _recordedLength;
			}
			if (stopTimestepSlider != null)
			{
				stopTimestepSlider.maxValue = _recordedLength;
				stopTimestepSlider.value = _recordedLength;
			}
			foreach (AnimationTimelineTrigger trigger in triggers)
			{
				trigger.ResyncMaxStartAndEndTimes();
			}
		}
	}

	public float totalTime => _recordedLength;

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical || forceStore)
		{
			if (_recordedLength > 0f)
			{
				needsStore = true;
				jSON["recordedLength"].AsFloat = _recordedLength;
				if (startTimestepSlider != null)
				{
					jSON["startTimestep"].AsFloat = startTimestepSlider.value;
				}
				if (stopTimestepSlider != null)
				{
					jSON["stopTimestep"].AsFloat = stopTimestepSlider.value;
				}
			}
			if (_loopbackTime != 1f || forceStore)
			{
				needsStore = true;
				jSON["loopbackTime"].AsFloat = _loopbackTime;
			}
			if (_loop || forceStore)
			{
				needsStore = true;
				jSON["loop"].AsBool = _loop;
			}
			if (_playbackSpeed != 1f || forceStore)
			{
				needsStore = true;
				jSON["playbackSpeed"].AsFloat = _playbackSpeed;
			}
			if (triggers != null)
			{
				needsStore = true;
				JSONArray jSONArray = (JSONArray)(jSON["triggers"] = new JSONArray());
				foreach (AnimationTimelineTrigger trigger in triggers)
				{
					jSONArray.Add(trigger.GetJSON());
				}
			}
		}
		return jSON;
	}

	public override void PreRestore()
	{
		ClearTriggers();
		ClearAllAnimation();
	}

	public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
	{
		base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		if (!restorePhysical)
		{
			return;
		}
		if (jc["recordedLength"] != null)
		{
			recordedLength = jc["recordedLength"].AsFloat;
			if (_recordedLength > 0f)
			{
				float val = 0f;
				if (startTimestepSlider != null && jc["startTimestep"] != null)
				{
					startTimestepSlider.value = jc["startTimestep"].AsFloat;
					val = startTimestepSlider.value;
				}
				if (stopTimestepSlider != null && jc["stopTimestep"] != null)
				{
					stopTimestepSlider.value = jc["stopTimestep"].AsFloat;
				}
				InternalSetPlaybackCounter(val);
				if (SuperController.singleton == null || SuperController.singleton.gameMode == SuperController.GameMode.Play)
				{
					StartPlayback();
				}
			}
		}
		else if (setMissingToDefault)
		{
			recordedLength = 0f;
		}
		if (jc["loopbackTime"] != null)
		{
			loopbackTime = jc["loopbackTime"].AsFloat;
		}
		else if (setMissingToDefault)
		{
			loopbackTime = 1f;
		}
		if (jc["playbackSpeed"] != null)
		{
			playbackSpeed = jc["playbackSpeed"].AsFloat;
		}
		else if (setMissingToDefault)
		{
			playbackSpeed = 1f;
		}
		if (jc["loop"] != null)
		{
			loop = jc["loop"].AsBool;
		}
		else if (setMissingToDefault)
		{
			loop = false;
		}
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (!restorePhysical || !(jc["triggers"] != null))
		{
			return;
		}
		JSONArray asArray = jc["triggers"].AsArray;
		if (!(asArray != null))
		{
			return;
		}
		foreach (JSONNode item in asArray)
		{
			JSONClass asObject = item.AsObject;
			if (asObject != null)
			{
				AnimationTimelineTrigger animationTimelineTrigger = AddTriggerInternal();
				animationTimelineTrigger.RestoreFromJSON(asObject);
			}
		}
	}

	public override void Validate()
	{
		base.Validate();
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.Validate();
		}
	}

	public void RegisterAnimationControl(MotionAnimationControl mac)
	{
		controllers.Add(mac);
	}

	public void DeregisterAnimationControl(MotionAnimationControl mac)
	{
		controllers.Remove(mac);
	}

	public float GetTotalTime()
	{
		return totalTime;
	}

	public float GetCurrentTimeCounter()
	{
		return _playbackCounter;
	}

	protected void SetLoopback()
	{
		PlaybackStep();
		_isLoopingBack = true;
		_loopbackCounter = 0f;
	}

	public void ResetAnimation()
	{
		_isLoopingBack = false;
		float num = 0f;
		if (startTimestepSlider != null)
		{
			num = startTimestepSlider.value;
		}
		_lastPlaybackCounter = num;
		_playbackCounter = num;
		_isPlaying = false;
		playbackCounterSlider.value = num;
		List<AnimationTimelineTrigger> list = new List<AnimationTimelineTrigger>(triggers);
		list.Sort((AnimationTimelineTrigger t1, AnimationTimelineTrigger t2) => t2.triggerStartTime.CompareTo(t1.triggerStartTime));
		foreach (AnimationTimelineTrigger item in list)
		{
			item.Reset();
		}
	}

	protected void InternalSetPlaybackCounter(float val, bool manualSet = false)
	{
		_isLoopingBack = false;
		if (_playbackCounter == val)
		{
			return;
		}
		bool flag = false;
		if (manualSet && _playbackCounter > val)
		{
			flag = true;
		}
		_lastPlaybackCounter = _playbackCounter;
		_playbackCounter = val;
		if (_isRecording)
		{
			if (_playbackCounter > _recordedLength)
			{
				if (_autoRecordStop && !_ignoreAutoRecordStop)
				{
					_playbackCounter = _recordedLength;
					if (SuperController.singleton != null)
					{
						SuperController.singleton.StopPlayback();
					}
					else
					{
						StopRecord();
					}
				}
				else
				{
					recordedLength = _playbackCounter;
				}
			}
			playbackCounterSlider.value = val;
		}
		else if (stopTimestepSlider != null && _playbackCounter > stopTimestepSlider.value)
		{
			if (manualSet)
			{
				_playbackCounter = stopTimestepSlider.value;
				playbackCounterSlider.value = _playbackCounter;
			}
			else if (_loop)
			{
				SetLoopback();
			}
			else
			{
				StopPlayback();
			}
		}
		else if (playbackCounterSlider != null && _playbackCounter > playbackCounterSlider.maxValue)
		{
			if (_loop)
			{
				SetLoopback();
			}
			else
			{
				StopPlayback();
			}
		}
		else
		{
			playbackCounterSlider.value = val;
		}
		if (!_isLoopingBack)
		{
			PlaybackStep();
		}
		if (flag)
		{
			foreach (AnimationTimelineTrigger reverseTrigger in reverseTriggers)
			{
				reverseTrigger.Update(flag, _lastPlaybackCounter);
			}
			return;
		}
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.Update(flag, _lastPlaybackCounter);
		}
	}

	protected void SyncShowRecordPaths()
	{
		if (controllers == null)
		{
			return;
		}
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.drawPathOpt = _showRecordPaths;
		}
	}

	public void ClearTriggers()
	{
		List<Trigger> list = new List<Trigger>();
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			list.Add(trigger);
		}
		foreach (Trigger item in list)
		{
			item.Remove();
		}
	}

	protected void CreateTriggerUI(AnimationTimelineTrigger att, int index)
	{
		if (triggerContentManager != null)
		{
			if (triggerPrefab != null)
			{
				RectTransform rectTransform = UnityEngine.Object.Instantiate(triggerPrefab);
				triggerContentManager.AddItem(rectTransform, index);
				att.triggerPanel = rectTransform;
			}
			else
			{
				Debug.LogError("Attempted to make TriggerUI when prefab was not set");
			}
		}
	}

	protected AnimationTimelineTrigger AddTriggerInternal(int index = -1)
	{
		AnimationTimelineTrigger animationTimelineTrigger = new AnimationTimelineTrigger();
		animationTimelineTrigger.timeLineHandler = this;
		animationTimelineTrigger.handler = this;
		if (index == -1)
		{
			triggers.Add(animationTimelineTrigger);
		}
		else
		{
			triggers.Insert(index, animationTimelineTrigger);
		}
		reverseTriggers = new List<AnimationTimelineTrigger>(triggers);
		reverseTriggers.Reverse();
		CreateTriggerUI(animationTimelineTrigger, index);
		animationTimelineTrigger.InitTriggerUI();
		animationTimelineTrigger.triggerActionsParent = triggerActionsParent;
		return animationTimelineTrigger;
	}

	public void AddTrigger()
	{
		AddTriggerInternal();
	}

	public override void RemoveTrigger(Trigger trigger)
	{
		if (triggers.Remove(trigger as AnimationTimelineTrigger))
		{
			reverseTriggers.Remove(trigger as AnimationTimelineTrigger);
			if (trigger.triggerActionsPanel != null)
			{
				UnityEngine.Object.Destroy(trigger.triggerActionsPanel.gameObject);
			}
			if (!(trigger.triggerPanel != null))
			{
				return;
			}
			if (triggerContentManager != null)
			{
				RectTransform component = trigger.triggerPanel.GetComponent<RectTransform>();
				if (component != null)
				{
					triggerContentManager.RemoveItem(component);
				}
			}
			UnityEngine.Object.Destroy(trigger.triggerPanel.gameObject);
		}
		else
		{
			Debug.Log("Could not remove trigger " + trigger.displayName);
		}
	}

	public override void DuplicateTrigger(Trigger trigger)
	{
		if (trigger is AnimationTimelineTrigger animationTimelineTrigger)
		{
			int num = triggers.IndexOf(animationTimelineTrigger);
			if (num != -1)
			{
				JSONClass jSON = animationTimelineTrigger.GetJSON();
				AnimationTimelineTrigger animationTimelineTrigger2 = AddTriggerInternal(num + 1);
				animationTimelineTrigger2.RestoreFromJSON(jSON);
			}
		}
	}

	public override void InitUI()
	{
		if (addTriggerButton != null)
		{
			addTriggerButton.onClick.AddListener(AddTrigger);
		}
		if (clearAllTriggersButton != null)
		{
			clearAllTriggersButton.onClick.AddListener(ClearTriggers);
		}
		if (playbackCounterSlider != null)
		{
			playbackCounterSlider.value = _playbackCounter;
			playbackCounterSlider.onValueChanged.AddListener(delegate
			{
				playbackCounter = playbackCounterSlider.value;
			});
			playbackCounterSlider.maxValue = 0f;
		}
		if (loopbackTimeSlider != null)
		{
			loopbackTimeSlider.value = _loopbackTime;
			loopbackTimeSlider.onValueChanged.AddListener(delegate
			{
				loopbackTime = loopbackTimeSlider.value;
			});
		}
		if (playbackSpeedSlider != null)
		{
			playbackSpeedSlider.value = _playbackSpeed;
			playbackSpeedSlider.onValueChanged.AddListener(delegate
			{
				playbackSpeed = playbackSpeedSlider.value;
			});
		}
		if (loopToggle != null)
		{
			loopToggle.isOn = _loop;
			loopToggle.onValueChanged.AddListener(delegate
			{
				loop = loopToggle.isOn;
			});
		}
		if (autoRecordStopToggle != null)
		{
			autoRecordStopToggle.isOn = _autoRecordStop;
			autoRecordStopToggle.onValueChanged.AddListener(delegate
			{
				autoRecordStop = autoRecordStopToggle.isOn;
			});
		}
		if (showRecordPathsToggle != null)
		{
			showRecordPathsToggle.isOn = _showRecordPaths;
			showRecordPathsToggle.onValueChanged.AddListener(delegate
			{
				showRecordPaths = showRecordPathsToggle.isOn;
			});
		}
		SyncShowRecordPaths();
		if (showStartMarkersToggle != null)
		{
			showStartMarkersToggle.isOn = _showStartMarkers;
			showStartMarkersToggle.onValueChanged.AddListener(delegate
			{
				showStartMarkers = showStartMarkersToggle.isOn;
			});
		}
	}

	public void ClearAllAnimation()
	{
		StopPlayback();
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.ClearAnimation();
		}
		recordedLength = 0f;
		_playbackCounter = 0f;
	}

	public void StartRecord()
	{
		SyncShowRecordPaths();
		_isRecording = true;
		_isPlaying = true;
		_lastRecordTime = -1f;
		if (_recordedLength == 0f)
		{
			_ignoreAutoRecordStop = true;
		}
		else
		{
			_ignoreAutoRecordStop = false;
		}
		TimeControl.singleton.currentScale = 1f;
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SetFreezeAnimation(freeze: false);
		}
		int recordCounter = Mathf.FloorToInt(_playbackCounter);
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.PrepareRecord(recordCounter);
		}
		if (!_showStartMarkers)
		{
			return;
		}
		foreach (MotionAnimationControl controller2 in controllers)
		{
			if (controller2.armedForRecord && controller2.controller != null)
			{
				controller2.controller.TakeSnapshot();
				controller2.controller.drawSnapshot = true;
			}
		}
	}

	public void StopRecord()
	{
		if (!_isRecording)
		{
			return;
		}
		RecordStep(forceRecord: true);
		_isRecording = false;
		foreach (MotionAnimationControl controller in controllers)
		{
			if (controller.controller != null)
			{
				controller.controller.drawSnapshot = false;
			}
			controller.FinalizeRecord();
			controller.armedForRecord = false;
		}
		StopLoopback();
		StartPlayback();
	}

	public void StartPlayback()
	{
		_isPlaying = true;
	}

	public void StopPlayback()
	{
		if (_isLoopingBack)
		{
			StopLoopback();
		}
		if (_isRecording)
		{
			StopRecord();
		}
		_isPlaying = false;
	}

	public void TrimAnimation()
	{
		float num = 0f;
		if (startTimestepSlider != null)
		{
			num = startTimestepSlider.value;
		}
		float value = _recordedLength;
		if (stopTimestepSlider != null)
		{
			value = stopTimestepSlider.value;
		}
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.TrimClip(num, value);
		}
		recordedLength = value - num;
		if (startTimestepSlider != null)
		{
			startTimestepSlider.value = 0f;
		}
	}

	protected void StopLoopback()
	{
		_isLoopingBack = false;
		float val = 0f;
		if (startTimestepSlider != null)
		{
			val = startTimestepSlider.value;
		}
		InternalSetPlaybackCounter(val);
	}

	protected void RecordStep(bool forceRecord = false)
	{
		if (!(_playbackCounter - _lastRecordTime > recordInterval))
		{
			return;
		}
		_lastRecordTime = _playbackCounter;
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.RecordStep(_playbackCounter, forceRecord);
		}
	}

	public void SeekToBeginning()
	{
		StopLoopback();
	}

	protected void PlaybackStep()
	{
		foreach (MotionAnimationControl controller in controllers)
		{
			if (!_isRecording || !controller.armedForRecord)
			{
				controller.PlaybackStep(_playbackCounter);
			}
		}
	}

	protected void LoopbackStep()
	{
		float toTimeStep = 0f;
		if (startTimestepSlider != null)
		{
			toTimeStep = startTimestepSlider.value;
		}
		foreach (MotionAnimationControl controller in controllers)
		{
			controller.LoopbackStep(_loopbackCounter / _loopbackTime, toTimeStep);
		}
	}

	protected void OnAtomRename(string fromuid, string touid)
	{
		foreach (AnimationTimelineTrigger trigger in triggers)
		{
			trigger.SyncAtomNames();
		}
	}

	protected void Init()
	{
		controllers = new List<MotionAnimationControl>();
		triggers = new List<AnimationTimelineTrigger>();
		reverseTriggers = new List<AnimationTimelineTrigger>();
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	protected void OnDestroy()
	{
		if (SuperController.singleton != null)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
		}
	}

	private void Update()
	{
		if (freeze || (!(SuperController.singleton == null) && SuperController.singleton.freezeAnimation))
		{
			return;
		}
		if (_isRecording)
		{
			RecordStep();
		}
		if (_isLoopingBack)
		{
			_loopbackCounter += Time.deltaTime * _playbackSpeed;
			if (_loopbackCounter > _loopbackTime)
			{
				StopLoopback();
			}
			else
			{
				LoopbackStep();
			}
		}
		else if (_isPlaying)
		{
			InternalSetPlaybackCounter(playbackCounter + Time.deltaTime * _playbackSpeed);
		}
	}
}
