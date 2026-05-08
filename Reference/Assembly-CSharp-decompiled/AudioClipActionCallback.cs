public class JSONStorableActionAudioClip
{
	public delegate void AudioClipActionCallback(NamedAudioClip nac);

	public string name;

	public AudioClipActionCallback actionCallback;

	public JSONStorableActionAudioClip(string n, AudioClipActionCallback callback)
	{
		name = n;
		actionCallback = callback;
	}
}
