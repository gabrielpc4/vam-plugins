using SimpleJSON;
using UnityEngine.UI;

public class AnimationTimelineTrigger : Trigger
{
	public AnimationTimelineTriggerHandler timeLineHandler;

	public Button startTimeToCurrentTimeButton;

	public Slider triggerStartTimeSlider;

	protected float _triggerStartTime;

	public Button endTimeToCurrentTimeButton;

	public Slider triggerEndTimeSlider;

	protected float _triggerEndTime;

	public float triggerStartTime
	{
		get
		{
			return _triggerStartTime;
		}
		set
		{
			if (_triggerStartTime != value)
			{
				_triggerStartTime = value;
				if (triggerStartTimeSlider != null)
				{
					triggerStartTimeSlider.value = _triggerStartTime;
				}
				if (_triggerEndTime < _triggerStartTime)
				{
					triggerEndTime = _triggerStartTime;
				}
			}
		}
	}

	public float triggerEndTime
	{
		get
		{
			return _triggerEndTime;
		}
		set
		{
			if (_triggerEndTime != value)
			{
				_triggerEndTime = value;
				if (_triggerEndTime < _triggerStartTime)
				{
					triggerEndTime = _triggerStartTime;
				}
				if (triggerEndTimeSlider != null)
				{
					triggerEndTimeSlider.value = _triggerEndTime;
				}
			}
		}
	}

	public override JSONClass GetJSON()
	{
		JSONClass jSON = base.GetJSON();
		jSON["startTime"].AsFloat = _triggerStartTime;
		jSON["endTime"].AsFloat = _triggerEndTime;
		return jSON;
	}

	public override void RestoreFromJSON(JSONClass jc)
	{
		base.RestoreFromJSON(jc);
		if (jc["startTime"] != null)
		{
			triggerStartTime = jc["startTime"].AsFloat;
		}
		if (jc["endTime"] != null)
		{
			triggerEndTime = jc["endTime"].AsFloat;
		}
	}

	protected void SetTriggerStartTimeToCurrentTime()
	{
		if (timeLineHandler != null)
		{
			triggerStartTime = timeLineHandler.GetCurrentTimeCounter();
		}
	}

	protected void SetTriggerEndTimeToCurrentTime()
	{
		if (timeLineHandler != null)
		{
			triggerEndTime = timeLineHandler.GetCurrentTimeCounter();
		}
	}

	public override void InitTriggerUI()
	{
		base.InitTriggerUI();
		if (!(triggerPanel != null))
		{
			return;
		}
		AnimationTimelineTriggerUI component = triggerPanel.GetComponent<AnimationTimelineTriggerUI>();
		if (component != null)
		{
			triggerStartTimeSlider = component.triggerStartTimeSlider;
			triggerEndTimeSlider = component.triggerEndTimeSlider;
			startTimeToCurrentTimeButton = component.startTimeToCurrentTimeButton;
			endTimeToCurrentTimeButton = component.endTimeToCurrentTimeButton;
		}
		if (triggerStartTimeSlider != null)
		{
			if (timeLineHandler != null)
			{
				triggerStartTimeSlider.maxValue = timeLineHandler.GetTotalTime();
			}
			triggerStartTimeSlider.value = _triggerStartTime;
			triggerStartTimeSlider.onValueChanged.AddListener(delegate
			{
				triggerStartTime = triggerStartTimeSlider.value;
			});
		}
		if (triggerEndTimeSlider != null)
		{
			if (timeLineHandler != null)
			{
				triggerEndTimeSlider.maxValue = timeLineHandler.GetTotalTime();
			}
			triggerEndTimeSlider.value = _triggerEndTime;
			triggerEndTimeSlider.onValueChanged.AddListener(delegate
			{
				triggerEndTime = triggerEndTimeSlider.value;
			});
		}
		if (startTimeToCurrentTimeButton != null)
		{
			startTimeToCurrentTimeButton.onClick.AddListener(SetTriggerStartTimeToCurrentTime);
		}
		if (endTimeToCurrentTimeButton != null)
		{
			endTimeToCurrentTimeButton.onClick.AddListener(SetTriggerEndTimeToCurrentTime);
		}
	}

	public override void DeregisterUI()
	{
		base.DeregisterUI();
		if (triggerStartTimeSlider != null)
		{
			triggerStartTimeSlider.onValueChanged.RemoveAllListeners();
		}
		if (triggerEndTimeSlider != null)
		{
			triggerEndTimeSlider.onValueChanged.RemoveAllListeners();
		}
		if (startTimeToCurrentTimeButton != null)
		{
			startTimeToCurrentTimeButton.onClick.RemoveListener(SetTriggerStartTimeToCurrentTime);
		}
		if (endTimeToCurrentTimeButton != null)
		{
			endTimeToCurrentTimeButton.onClick.RemoveListener(SetTriggerEndTimeToCurrentTime);
		}
	}

	public void ResyncMaxStartAndEndTimes()
	{
		if (timeLineHandler != null)
		{
			if (triggerEndTimeSlider != null)
			{
				triggerEndTimeSlider.maxValue = timeLineHandler.GetTotalTime();
			}
			if (triggerStartTimeSlider != null)
			{
				triggerStartTimeSlider.maxValue = timeLineHandler.GetTotalTime();
			}
		}
	}

	public void Update(bool reverse_playback, float lastPlaybackTime)
	{
		if (timeLineHandler == null)
		{
			return;
		}
		float currentTimeCounter = timeLineHandler.GetCurrentTimeCounter();
		reverse = reverse_playback;
		if (reverse)
		{
			if (currentTimeCounter > lastPlaybackTime)
			{
				if (_triggerEndTime <= lastPlaybackTime)
				{
					base.active = true;
				}
				if (_triggerStartTime <= lastPlaybackTime)
				{
					base.transitionInterpValue = 0f;
					base.active = false;
				}
				if (_triggerEndTime >= currentTimeCounter)
				{
					base.active = true;
				}
				if (_triggerStartTime >= currentTimeCounter)
				{
					base.transitionInterpValue = 0f;
					base.active = false;
				}
			}
			else
			{
				if (_triggerEndTime <= lastPlaybackTime && _triggerEndTime >= currentTimeCounter)
				{
					base.active = true;
				}
				if (_triggerStartTime <= lastPlaybackTime && _triggerStartTime >= currentTimeCounter)
				{
					base.transitionInterpValue = 0f;
					base.active = false;
				}
			}
		}
		else if (currentTimeCounter < lastPlaybackTime)
		{
			if (_triggerStartTime >= lastPlaybackTime)
			{
				base.active = true;
			}
			if (_triggerEndTime >= lastPlaybackTime)
			{
				base.transitionInterpValue = 1f;
				base.active = false;
			}
			if (_triggerStartTime <= currentTimeCounter)
			{
				base.active = true;
			}
			if (_triggerEndTime <= currentTimeCounter)
			{
				base.transitionInterpValue = 1f;
				base.active = false;
			}
		}
		else
		{
			if (_triggerStartTime >= lastPlaybackTime && _triggerStartTime <= currentTimeCounter)
			{
				base.active = true;
			}
			if (_triggerEndTime >= lastPlaybackTime && _triggerEndTime <= currentTimeCounter)
			{
				base.transitionInterpValue = 1f;
				base.active = false;
			}
		}
		if (triggerEndTime != triggerStartTime)
		{
			base.transitionInterpValue = (currentTimeCounter - triggerStartTime) / (triggerEndTime - triggerStartTime);
		}
		else if (base.active)
		{
			base.transitionInterpValue = 0f;
		}
		else
		{
			base.transitionInterpValue = 1f;
		}
		base.Update();
	}
}
