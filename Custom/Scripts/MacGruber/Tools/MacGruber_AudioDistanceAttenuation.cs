/* /////////////////////////////////////////////////////////////////////////////////////////////////
AudioDistanceAttenuation v0.1 by MacGruber.
Attempt at an improved audio distance attenuation curve over the Unity default logarithm curve.
There is also an attempt to compensate for the apparent distances differences when dealing with different camera FoV angles.

Version 0.1 2020-02-24
	Initial release.

///////////////////////////////////////////////////////////////////////////////////////////////// */

using UnityEngine;
using UnityEngine.XR;
using System.Collections;

namespace MacGruber
{
	public class AudioDistanceAttenuation : MVRScript
	{	
		private JSONStorableFloat myVolumeDesktop;
		private JSONStorableFloat myVolumeVR;
		private JSONStorableFloat myVolume;
		private JSONStorableFloat myDistanceScale;
		
		private float myFieldOfView = -1.0f;
		private float myMinDistance = 0.05f;
		private float myMaxDistance = 50.0f;
		
		private AudioSourceControl myAudioSource;
		private AnimationCurve myCustomCurve;
		private AnimationCurve myPreviousCurve;
		
		private bool myIsDesktopMode = false;
		private bool myInit = false;

		public override void Init()
		{
			string storableName = string.Empty;
			if (containingAtom.type == "Person")
				storableName = "HeadAudioSource";
			else if (containingAtom.type == "AudioSource")
				storableName = "AudioSource";
			else if (containingAtom.type == "AptSpeaker")
				storableName = "AptSpeaker_Import";
			else if (containingAtom.type == "RhythmAudioSource")
				storableName = "RhythmSource";
			myAudioSource = containingAtom.GetStorableByID(storableName) as AudioSourceControl;
			
			if (myAudioSource == null)
			{
				SuperController.LogError("The MacGruber.AudioDistanceAttenuation plugin needs to be placed on a Person, AudioSource, RhythmAudioSource or AptSpeaker atom.");
				return;
			}
			
			myPreviousCurve = myAudioSource.audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
			
			myCustomCurve = new AnimationCurve();
			myCustomCurve.preWrapMode = WrapMode.ClampForever;
			myCustomCurve.postWrapMode = WrapMode.ClampForever;
			
			SetupInfoText(this, 
				"<color=#606060><size=40><b>AudioDistanceAttenuation</b></size>\nAttempt at an improved audio distance attenuation curve over the Unity default logarithm curve.\n" +
				"There is also an attempt to compensate for the apparent distance differences when dealing with different camera FoV angles.</color>\n\n" +
				"<b>Volume Desktop/VR:</b> Since sound is generally louder with this plugin, you can do additional volume scaling here. Also you can compensate for the volume difference between Desktop and VR.\n\n" +
				"<b>Distance Scale:</b> Scales distance until the volume reaches zero. You likely want smaller distances for silent sounds and larger distances for loud sounds. In VR this is actual meters, in desktop modes this is FoV dependent.\n\n",
				1200.0f, true
			);
			
			myVolumeDesktop = SetupSliderFloat(this, "Volume Desktop", 0.22f, 0.0f, 1.0f, false);
			myVolumeDesktop.setCallbackFunction += (float v) => { UpdateData(true); };
			myVolumeVR = SetupSliderFloat(this, "Volume VR", 0.10f, 0.0f, 1.0f, false);
			myVolumeVR.setCallbackFunction += (float v) => { UpdateData(true); };
			myDistanceScale = SetupSliderFloat(this, "Distance Scale", 50.0f, 1.0f, 500.0f, false);
			myDistanceScale.setCallbackFunction += (float v) => { UpdateData(true); };
			
			myInit = true;
			myIsDesktopMode = !XRSettings.isDeviceActive || string.IsNullOrEmpty(XRSettings.loadedDeviceName);
			myVolume = myIsDesktopMode ? myVolumeDesktop : myVolumeVR;
			
			OnEnable();
		}
		
		private void OnEnable()
		{
			if (!myInit || myAudioSource == null)
				return;
			
			UpdateData(true);
			StartCoroutine(Task());
		}
		
		private void OnDisable()
		{
			StopAllCoroutines();
			
			if (myAudioSource == null)
				return;
			
			// restore defaults
			myAudioSource.minDistance = 0.4f;
			myAudioSource.maxDistance = 100.0f;
			myAudioSource.audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
			myAudioSource.audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, myPreviousCurve);
		}		
	
		private IEnumerator Task()
		{
			while (true)
			{
				yield return new WaitForSeconds(0.51f);
				UpdateData(false);
			}
		}
		
		private readonly float myTanReference = Mathf.Tan(60.0f * Mathf.Deg2Rad * 0.5f);
		
		private void UpdateData(bool force)
		{
			if (myIsDesktopMode)
			{		
				float fov = Camera.main.fieldOfView;
				if (force || fov != myFieldOfView)
				{
					myFieldOfView = fov;
					float tanActual = Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f);
					myMinDistance = 0.3f / tanActual * myTanReference;
					myMaxDistance = myDistanceScale.val / tanActual * myTanReference;
					force = true;
				}
			}
			else
			{
				myMinDistance = 0.05f;
				myMaxDistance = myDistanceScale.val;
			}

			if (force)
			{
				float v = myVolume.val;
				float minDist = myMinDistance / myMaxDistance;
				float midDist = minDist + 0.08f;
				myCustomCurve.keys = new Keyframe[] {
					new Keyframe(minDist, 1.0f*v,  -3.0f*v,  -3.0f*v, 1.0f, 0.50f),
					new Keyframe(midDist, 0.5f*v, -16.0f*v, -16.0f*v, 0.1f, 0.03f),
					new Keyframe(   1.0f, 0.0f*v,  -0.3f*v,  -0.3f*v, 0.0f, 0.00f)
				};
				myAudioSource.audioSource.rolloffMode = AudioRolloffMode.Custom;
				myAudioSource.audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, myCustomCurve);
			}
				
			myAudioSource.minDistance = myMinDistance;
			myAudioSource.maxDistance = myMaxDistance;
		}
		
		public static JSONStorableFloat SetupSliderFloat(MVRScript script, string label, float defaultValue, float minValue, float maxValue, bool rightSide)
		{
			JSONStorableFloat storable = new JSONStorableFloat(label, defaultValue, minValue, maxValue, true, true);
			storable.storeType = JSONStorableParam.StoreType.Full;
			script.CreateSlider(storable, rightSide);
			script.RegisterFloat(storable);
			return storable;
		}
		
		public static JSONStorableString SetupInfoText(MVRScript script, string text, float height, bool rightSide)
		{
			JSONStorableString storable = new JSONStorableString("Info", text);
			UIDynamic textfield = script.CreateTextField(storable, rightSide);
			textfield.height = height;
			return storable;
		}
	}
}