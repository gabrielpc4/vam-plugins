using UnityEngine;
using MeshVR;
using System;
using System.Collections.Generic;
using static MacGruber.DriverThrust;
using static MacGruber.Utils;
using Request = MeshVR.AssetLoader.AssetBundleFromFileRequest;
using SimpleJSON;

namespace MacGruber
{
	public class DriverThrustSFX : MVRScript
	{
		private static readonly string BASE_PATH = "Assets/SoundFX/";
		
		private DriverThrust myDriver;
		private DriverThrust.Trigger myTriggerLubeUp;
		private DriverThrust.Trigger myTriggerLubeDown;
		private DriverThrust.Trigger myTriggerSlapDown;
		private DriverThrust.Trigger myTriggerBedDown;
		
		private string myBundleURL = null;
		private Request myBundleRequest = null;
		private JSONNode myDatabase;
		private bool myWasLoading;
		
		private Dictionary<string, List<EffectEntry>> myEntries = new Dictionary<string, List<EffectEntry>>();
		private bool myInitialized = false;
	
		private AudioSourceControl myLubeAudioSource;
		private JSONStorableStringChooser myLubeAudioSourceUID;
		private JSONStorableFloat myLubeRatioUp;
		private JSONStorableFloat myLubeRatioDown;
		private JSONStorableFloat myLubeVolumeMin;
		private JSONStorableFloat myLubeVolumeMax;
		private JSONStorableFloat myLubeVelocityMin;
		private JSONStorableFloat myLubeVelocityMax;
		private List<EffectEntry> myLubeEntries;
		private int myLubeNextIndex = -1;
		
		private AudioSourceControl mySlapAudioSource;
		private JSONStorableStringChooser mySlapAudioSourceUID;
		private JSONStorableFloat mySlapRatio;
		private JSONStorableFloat mySlapVolumeMin;
		private JSONStorableFloat mySlapVolumeMax;
		private JSONStorableFloat mySlapVelocityMin;
		private JSONStorableFloat mySlapVelocityMax;
		private List<EffectEntry> mySlapEntries;
		private int mySlapNextIndex = -1;
		
		private AudioSourceControl myBedAudioSource;
		private JSONStorableStringChooser myBedAudioSourceUID;
		private JSONStorableFloat myBedRatio;
		private JSONStorableFloat myBedVolumeMin;
		private JSONStorableFloat myBedVolumeMax;
		private JSONStorableFloat myBedPitchMin;
		private JSONStorableFloat myBedPitchMax;
		private JSONStorableFloat myBedVelocityMin;
		private JSONStorableFloat myBedVelocityMax;
		private List<EffectEntry> myBedEntries;
		private int myBedNextIndex = -1;
		
		public override void Init()
		{
			Cleanup();			
			if (containingAtom.type != "Person")
			{
				SuperController.LogError("MacGruber DriverThrustSFX script needs to be on a Person atom.");
				return;
			}
			
			myDriver = FindWithinSamePlugin<DriverThrust>(this);
			if (myDriver == null)
			{
				SuperController.LogError("Plugin 'MacGruber.DriverThrust' not found.");
				return;
			}
			
			myBundleURL = Utils.GetPluginPath(this) + "/MacGruber_Effects.audiobundle";
			//SuperController.LogMessage(myBundleURL);

			Request request = new AssetLoader.AssetBundleFromFileRequest {path = myBundleURL, callback = OnAssetBundleLoaded};
			AssetLoader.QueueLoadAssetBundleFromFile(request);
			
			
			Utils.SetupInfoText(this, 
				"<color=#606060><size=40><b>DriverThrustSFX</b></size>\nSound effects for DriverThrust plugin.</color>",
				60.0f, true
			);
			
			List<string> audioSourceUIDs = GetAudioSourceUIDs();
			myLubeAudioSourceUID = Utils.SetupStringChooser(this, "Lube Audio", audioSourceUIDs, false);
			myLubeAudioSourceUID.setCallbackFunction += (string name) => { 
				myLubeAudioSource = GetAudioSourceById(name);
				if (myLubeAudioSource != null)
				{
					if (myTriggerLubeUp == null)
						myTriggerLubeUp = myDriver.RegisterTrigger(TRG_Up, 0.0f, OnTriggerLube);
					if (myTriggerLubeDown == null)
						myTriggerLubeDown = myDriver.RegisterTrigger(TRG_Down, 0.0f, OnTriggerLube);
				}
				else
				{
					myDriver.UnregisterTrigger(myTriggerLubeUp);
					myDriver.UnregisterTrigger(myTriggerLubeDown);
					myTriggerLubeUp = null;
					myTriggerLubeDown = null;
				}
			};
			myLubeAudioSourceUID.setCallbackFunction(myLubeAudioSourceUID.val);
			myLubeRatioUp = SetupSliderFloat(this, "Lube Ratio Up", -0.7f, -1.0f, 1.0f, false);
			myLubeRatioDown = SetupSliderFloat(this, "Lube Ratio Down", -0.7f, -1.0f, 1.0f, true);
			myLubeVolumeMin = SetupSliderFloat(this, "Lube Volume Min", 0.3f, 0.0f, 1.0f, false);
			myLubeVolumeMax = SetupSliderFloat(this, "Lube Volume Max", 0.5f, 0.0f, 1.0f, true);
			myLubeVelocityMin = SetupSliderFloat(this, "Lube Velocity Min", 0.1f, 0.1f, 5.0f, false);
			myLubeVelocityMax = SetupSliderFloat(this, "Lube Velocity Max", 3.0f, 0.1f, 5.0f, true);
			
			Utils.SetupInfoText(this, "", 238.0f, true);
		
			mySlapAudioSourceUID = Utils.SetupStringChooser(this, "Slap Audio", audioSourceUIDs, false);
			mySlapAudioSourceUID.setCallbackFunction += (string name) => { 
				mySlapAudioSource = GetAudioSourceById(name);
				if (mySlapAudioSource != null)
				{
					if (myTriggerSlapDown == null)
						myTriggerSlapDown = myDriver.RegisterTrigger(TRG_Down, 0.0f, OnTriggerSlap);
				}
				else
				{
					myDriver.UnregisterTrigger(myTriggerSlapDown);
					myTriggerSlapDown = null;
				}
			};
			mySlapAudioSourceUID.setCallbackFunction(mySlapAudioSourceUID.val);
			mySlapRatio = SetupSliderFloat(this, "Slap Ratio", 0.0f, -1.0f, 1.0f, false);
			mySlapVolumeMin = SetupSliderFloat(this, "Slap Volume Min", 0.0f, 0.0f, 1.0f, false);
			mySlapVolumeMax = SetupSliderFloat(this, "Slap Volume Max", 0.5f, 0.0f, 1.0f, true);
			mySlapVelocityMin = SetupSliderFloat(this, "Slap Velocity Min", 1.5f, 0.1f, 5.0f, false);
			mySlapVelocityMax = SetupSliderFloat(this, "Slap Velocity Max", 3.0f, 0.1f, 5.0f, true);
			
			
			Utils.SetupInfoText(this, "", 238.0f, true);
			
			myBedAudioSourceUID = Utils.SetupStringChooser(this, "Bed Audio", audioSourceUIDs, false);
			myBedAudioSourceUID.setCallbackFunction += (string name) => { 
				myBedAudioSource = GetAudioSourceById(name);
				if (myBedAudioSource != null)
				{
					if (myTriggerBedDown == null)
						myTriggerBedDown = myDriver.RegisterTrigger(TRG_Down, 0.0f, OnTriggerBed);
				}
				else
				{
					myDriver.UnregisterTrigger(myTriggerBedDown);
					myTriggerBedDown = null;
				}
			};
			myBedAudioSourceUID.setCallbackFunction(myBedAudioSourceUID.val);
			myBedRatio = SetupSliderFloat(this, "Bed Ratio", 0.0f, -1.0f, 1.0f, false);
			myBedVolumeMin = SetupSliderFloat(this, "Bed Volume Min", 0.0f, 0.0f, 1.0f, false);
			myBedVolumeMax = SetupSliderFloat(this, "Bed Volume Max", 0.5f, 0.0f, 1.0f, true);
			myBedPitchMin = SetupSliderFloat(this, "Bed Pitch Min", 2.0f, 0.1f, 3.0f, false);
			myBedPitchMax = SetupSliderFloat(this, "Bed Pitch Max", 1.0f, 0.1f, 3.0f, true);
			myBedVelocityMin = SetupSliderFloat(this, "Bed Velocity Min", 1.5f, 0.1f, 5.0f, false);
			myBedVelocityMax = SetupSliderFloat(this, "Bed Velocity Max", 3.0f, 0.1f, 5.0f, true);
			
			SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChanged;
		}
		
		private AudioSourceControl GetAudioSourceById(string name)
		{
			if (name == INVALID_AUDIOSOURCE)
				return null;			
			return GetAtomById(name)?.GetStorableByID("AudioSource") as AudioSourceControl;
		}
		
		private void OnAssetBundleLoaded(Request aRequest)
		{	
			Cleanup();
			myBundleRequest = aRequest;

			TextAsset ta = myBundleRequest.assetBundle?.LoadAsset<TextAsset>(BASE_PATH + "Database.json");
			if (ta == null)
			{
				SuperController.LogError("Failed loading JSON database from AssetBundle.");
				return;
			}
			
			myDatabase = JSON.Parse(ta.text);
			if (myDatabase == null || myDatabase["datasets"] == null)
			{
				SuperController.LogError("Failed parsing JSON database.");
				return;
			}
			
			JSONClass datasets = myDatabase["datasets"].AsObject;
			IEnumerator<string> dataset = datasets.Keys.GetEnumerator();
			while (dataset.MoveNext())
				LoadDataset(dataset.Current);
			
			myLubeEntries = null;
			if (myEntries.TryGetValue("Bluzz44-Lube", out myLubeEntries))
				myLubeNextIndex = UnityEngine.Random.Range(0, myLubeEntries.Count);
			
			mySlapEntries = null;
			if (myEntries.TryGetValue("Bluzz44-FleshySlap", out mySlapEntries))
				mySlapNextIndex = UnityEngine.Random.Range(0, mySlapEntries.Count);
			
			myBedEntries = null;
			if (myEntries.TryGetValue("FreeSFX-Bed", out myBedEntries))
				myBedNextIndex = UnityEngine.Random.Range(0, myBedEntries.Count);
		}
		
		private void LoadDataset(string aDatasetName)
		{				
			JSONNode dataset = myDatabase["datasets"]?[aDatasetName];
			if (dataset == null)
			{
				SuperController.LogError("Failed loading '"+aDatasetName+"' dataset.");
				return;
			}
			
			string path = dataset["path"];			
			JSONNode jsonVolume = dataset["volume"];
			float volume = jsonVolume != null ? dataset["volume"].AsFloat : 1.0f;
			
			JSONArray dataentries = dataset["entries"].AsArray;
			List<EffectEntry> entries = new List<EffectEntry>(dataentries.Count);
			myEntries[aDatasetName] = entries;
			bool skippedEntries = false;
			for (int i=0; i<dataentries.Count; ++i)
			{
				EffectEntry entry = new EffectEntry(myBundleRequest, path, volume, dataentries[i]);
				if (entry.IsValid())
					entries.Add(entry);
				else
					skippedEntries = true;
			}
			if (skippedEntries)
				SuperController.LogError("Skipped invalid entries when loading dataset '"+aDatasetName+"'.");
			
			//SuperController.LogMessage("Loaded dataset '"+aDatasetName+"'.");
			myInitialized |= entries.Count > 0;
		}
		
		private void Cleanup()
		{
			myInitialized = false;
			myEntries.Clear();
			myLubeEntries = null;
			mySlapEntries = null;
			myBedEntries = null;
		}
		
		private void OnDestroy()
		{
			Cleanup();			
			if (myDriver != null)
			{
				myDriver.UnregisterTrigger(myTriggerLubeUp);
				myDriver.UnregisterTrigger(myTriggerLubeDown);
				myDriver.UnregisterTrigger(myTriggerSlapDown);
				myDriver.UnregisterTrigger(myTriggerBedDown);
			}			
			if (myBundleURL != null)
			{
                AssetLoader.DoneWithAssetBundleFromFile(myBundleURL);
				myBundleURL = null;
			}
			
			SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChanged;
		}
						
		private void OnAtomUIDsChanged(List<string> atomUIDs)
		{
			myWasLoading = true;
		}
		
		private void Update()
		{
			bool isLoading = SuperController.singleton.isLoading;
			if (!isLoading && myWasLoading)
				UpdateAtomLinks();
			myWasLoading = isLoading;
			
			if (myDriver == null || myLubeEntries == null || mySlapEntries == null)
				return;
			
			{
				float ratio = myLubeRatioUp.val;
				float duration = ratio > 0.0f ? myDriver.DurationUp : myDriver.DurationDown;
				duration -= myLubeEntries[myLubeNextIndex].Length;
				myTriggerLubeUp.myOffset = duration * (duration > 0 ? ratio : 1.0f);
			}			
			{
				float ratio = myLubeRatioDown.val;
				float duration = ratio > 0.0f ? myDriver.DurationDown : myDriver.DurationUp;
				duration -= myLubeEntries[myLubeNextIndex].Length;
				myTriggerLubeDown.myOffset = duration * (duration > 0 ? ratio : 1.0f);
			}			
			{
				float ratio = mySlapRatio.val;
				float duration = ratio > 0.0f ? myDriver.DurationDown : myDriver.DurationUp;
				duration -= mySlapEntries[mySlapNextIndex].Length;
				myTriggerSlapDown.myOffset = duration * (duration > 0 ? ratio : 1.0f);
				//SuperController.LogMessage("myTriggerSlapDown.myOffset="+myTriggerSlapDown.myOffset);
			}
			{
				float ratio = myBedRatio.val;
				float duration = ratio > 0.0f ? myDriver.DurationDown : myDriver.DurationUp;
				float velocity = myDriver.Power / duration;
				velocity = Mathf.InverseLerp(myBedVelocityMin.val, myBedVelocityMax.val, velocity);
				float pitch = Mathf.Lerp(myBedPitchMin.val, myBedPitchMax.val, velocity);
				duration *= pitch;
				duration -= myBedEntries[myBedNextIndex].Length;
				myTriggerBedDown.myOffset = duration * (duration > 0 ? ratio : 1.0f);
			}
		}
		
		private void UpdateAtomLinks()
		{
			if (myLubeAudioSourceUID == null || mySlapAudioSourceUID == null)
				return;
			
			List<string> audioSourceUIDs = GetAudioSourceUIDs();
			
			myLubeAudioSourceUID.choices = audioSourceUIDs;
			myLubeAudioSourceUID.setCallbackFunction(myLubeAudioSourceUID.val);
			
			mySlapAudioSourceUID.choices = audioSourceUIDs;
			mySlapAudioSourceUID.setCallbackFunction(mySlapAudioSourceUID.val);
			
			myBedAudioSourceUID.choices = audioSourceUIDs;
			myBedAudioSourceUID.setCallbackFunction(myBedAudioSourceUID.val);
		}
		
		////////////////////////////////////////////////////////////////////////////////////
		
		private void OnTriggerLube()
		{
			if (myLubeEntries == null)
				return;			
			
			float velocity = myDriver.Power / myDriver.DurationDown;
			if (velocity > 0.001f)
			{
				int choice = myLubeNextIndex;
				EffectEntry entry = myLubeEntries[choice];
				velocity = Mathf.InverseLerp(myLubeVelocityMin.val, myLubeVelocityMax.val, velocity);
				velocity = Mathf.Clamp01(velocity);
				myLubeAudioSource.volume = Mathf.Lerp(myLubeVolumeMin.val, myLubeVolumeMax.val, velocity) * entry.Volume;
				myLubeAudioSource.pitch = UnityEngine.Random.Range(0.8f, 1.1f);
				myLubeAudioSource.PlayIfClear(entry.Clip);
				
				myLubeNextIndex = UnityEngine.Random.Range(0, myLubeEntries.Count-1);
				if (myLubeNextIndex >= choice)
					++myLubeNextIndex;
			}
		}
		
		private void OnTriggerSlap()
		{
			if (mySlapEntries == null)
				return;			
			
			float velocity = myDriver.Power / myDriver.DurationDown;
			if (velocity > 0.001f)
			{
				int choice = mySlapNextIndex;
				EffectEntry entry = mySlapEntries[choice];
				velocity = Mathf.InverseLerp(mySlapVelocityMin.val, mySlapVelocityMax.val, velocity);
				velocity = Mathf.Clamp01(velocity);
				mySlapAudioSource.volume = Mathf.Lerp(mySlapVolumeMin.val, mySlapVolumeMax.val, velocity) * entry.Volume;
				mySlapAudioSource.PlayIfClear(entry.Clip);
				
				mySlapNextIndex = UnityEngine.Random.Range(0, mySlapEntries.Count-1);
				if (mySlapNextIndex >= choice)
					++mySlapNextIndex;
			}
		}
		
		private void OnTriggerBed()
		{
			if (myBedEntries == null)
				return;
			
			float velocity = myDriver.Power / myDriver.DurationDown;
			if (velocity > 0.001f)
			{
				int choice = myBedNextIndex;
				EffectEntry entry = myBedEntries[choice];
				velocity = Mathf.InverseLerp(myBedVelocityMin.val, myBedVelocityMax.val, velocity);
				velocity = Mathf.Clamp01(velocity);
				myBedAudioSource.volume = Mathf.Lerp(myBedVolumeMin.val, myBedVolumeMax.val, velocity) * entry.Volume;
				myBedAudioSource.pitch = Mathf.Lerp(myBedPitchMin.val, myBedPitchMax.val, velocity);
				myBedAudioSource.PlayIfClear(entry.Clip);
				
				myBedNextIndex = UnityEngine.Random.Range(0, myBedEntries.Count-1);
				if (myBedNextIndex >= choice)
					++myBedNextIndex;
			}
		}
		
		////////////////////////////////////////////////////////////////////////////////////
		
		private readonly string INVALID_AUDIOSOURCE = "[Disabled]";
		
		private List<string> GetAudioSourceUIDs()
		{
			List<string> atomUIDs = GetAtomUIDs();
			List<string> audioSourceUIDs = new List<string>();
			audioSourceUIDs.Add(INVALID_AUDIOSOURCE);
			for (int i=0; i<atomUIDs.Count; ++i)
			{
				Atom atom = GetAtomById(atomUIDs[i]);
				if (atom.type == "AudioSource")
					audioSourceUIDs.Add(atomUIDs[i]);
			}
			return audioSourceUIDs;
		}
		
		////////////////////////////////////////////////////////////////////////////////////
			
		public class EffectEntry
		{
			public AssetBundleAudioClip Clip { get; private set; }
			public string Name { get; private set; }
			public float Volume { get; private set; }
			public float Length { get; private set; }
			
			public EffectEntry(Request aRequest, string aPath, float aVolume, JSONNode aEntryData)
			{
				Name = aPath + aEntryData["file"].Value;
				Volume = aVolume;
				
				JSONArray markers = aEntryData["markers"].AsArray;
				for (int i=0; i<markers.Count; ++i)
				{
					JSONArray a = markers[i].AsArray;					
					if (a[0].Value == "len");
						Length = a[1].AsFloat;
				}
				
				Clip = new AssetBundleAudioClip(aRequest, BASE_PATH, Name);
			}
			
			public bool IsValid()
			{
				return (Clip?.sourceClip) != null;
			}
		}
	}
}