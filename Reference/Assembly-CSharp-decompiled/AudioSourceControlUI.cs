using UnityEngine.UI;

public class AudioSourceControlUI : UIProvider
{
	public Toggle loopToggle;

	public Slider volumeSlider;

	public Slider pitchSlider;

	public Slider stereoPanSlider;

	public Slider minDistanceSlider;

	public Slider maxDistanceSlider;

	public Slider spatialBlendSlider;

	public Slider stereoSpreadSlider;

	public Toggle spatializeToggle;

	public Slider delayBetweenQueuedClipsSlider;

	public Slider volumeTriggerQuicknessSlider;

	public Slider volumeTriggerMultiplierSlider;

	public Text playingClipNameText;
}
