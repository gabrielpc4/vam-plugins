/* /////////////////////////////////////////////////////////////////////////////////////////////////
Life#Breathing v0.4 by MacGruber.
Audio-Synchronized Breathing for VaM.

Version 0.4 2020-03-01
	Improved breathing-sync of Thrust plugin.
	Implemented SoundFX for Thrust plugin with Lube, FleshySlap and BedSqueak sounds.
	Implemented UpDown-Balance option for Thrust plugin in preparation for VaMConnect interface.
	Fixed audio compression for breathing audio.
	Forcing Pitch to 1.0 for breathing audio to prevent potential desyncs.
	Removed Volume seperation for Desktop/VR, use AudioDistanceAttenuation plugin instead.
	Added Life04 demo scene to show SFX.

Version 0.3.1 2020-02-16
	Fixed nose breathing-in offset.
	Fixed lips morph animating parts of nose.

Version 0.3 2020-02-15
	Implemented custom morphs for chest, stomach, nose (by DDQuidam)
	Implemented custom morph for lips (by TimelordToby)
	Fixed MacGruber_Life.cslist

Version 0.2 2020-01-20
	Implemented RhythmDamping to allow you control how regular you want your breathing.
	Implemented "DriverThrust", drives AnimationPatterns and/or VariableTriggers so you can have synchronized 'action'.
	Implemented separate volume for VR and Desktop.	Also rebalanced audio volume between recordings.
	Implemented audio-interrupt feature which allows faster transition between IO/OI modes.
	Implemented PostMagic-style info text to explain all the sliders.
	Fixed potential silent crash during scene load.
	Fixed issue with Breathing settings not surviving changing Person look.
	Tweaked some default values.

Version 0.1 2019-12-18
	Initial release.

///////////////////////////////////////////////////////////////////////////////////////////////// */

using UnityEngine;
using UnityEngine.XR;
using MeshVR;
using System;
using System.Collections.Generic;
using Request = MeshVR.AssetLoader.AssetBundleFromFileRequest;
using SimpleJSON;

namespace MacGruber
{
	public partial class Breathing : MVRScript
	{
		////////////////////////////////////////////////////////////////////////////////////
		// Constants
		
		private static readonly string BASE_PATH = "Assets/Breathing/";
		
		public const int TAG_MOAN    = 1 << 0;
		public const int TAG_NOSEIN  = 1 << 1;
		public const int TAG_NOSEOUT = 1 << 2;
		public const int TAG_MULTI   = 1 << 3;
		public const sbyte PREV = 0;
		public const sbyte CURR = 1;
		public const sbyte NEXT = 2;
		public const int FORMAT_INOUT = 0;
		public const int FORMAT_OUTIN = 1;
		
		////////////////////////////////////////////////////////////////////////////////////
		// Members
			
		private string myBundleURL = null;
		private Request myBundleRequest = null;
		private JSONNode myDatabase;
		private Dictionary<string, List<BreathEntry>> myEntries = new Dictionary<string, List<BreathEntry>>();
		private List<BreathEntry> mySelection = new List<BreathEntry>();
		private MiniQueue<Breath> myQueue = new MiniQueue<Breath>();
		private List<Event> myEvents = new List<Event>();
		private float myDurationDamped = -1.0f;
		private bool myInitialized = false;
		private bool myQueueInitialized = false;
		
		private AudioSourceControl myHeadAudioSource;

		private JSONStorableStringChooser myDataset;
		private JSONStorableFloat myIntensity;
		private JSONStorableFloat myVariance;
		internal JSONStorableFloat myRhythmRandomness;
		internal JSONStorableFloat myRhythmDamping;
		private JSONStorableFloat myVolume;
				
		public float CurrentTime { get; private set; }
		
		////////////////////////////////////////////////////////////////////////////////////
		// Core
		
		public override void Init()
        {
			Cleanup();
			
			if (containingAtom.type != "Person")
			{
				SuperController.LogError("MacGruber Breathing script needs to be on a Person atom.");
				return;
			}
						
			InitCore();
			InitUI();
        }
		
		private void InitCore()
		{
			myBundleURL = Utils.GetPluginPath(this) + "/MacGruber_Breathing.audiobundle";
			//SuperController.LogMessage(myBundleURL);

			Request request = new AssetLoader.AssetBundleFromFileRequest {path = myBundleURL, callback = OnAssetBundleLoaded};
			AssetLoader.QueueLoadAssetBundleFromFile(request);
		
			myHeadAudioSource = containingAtom.GetStorableByID("HeadAudioSource") as AudioSourceControl;
			
			RegisterCallbackEvent(SM(MRK_Interrupt), Interrupt);
		}
		
		private void InitUI()
		{		
			Utils.SetupInfoText(this,
				"<color=#606060><size=40><b>Breathing</b></size>\nThis selects and plays breathing audio as well as providing the event system as a basis for animation. Animation driver plugins can connect to this to control their body animation.</color>\n\n" + 
				"<b>Dataset:</b> Choose between available audio recording sets.\n\n" + 					
				"<b>Intensity:</b> Intensity of breathing. Higher values generally mean faster breathing.\n\n" + 
				"<b>Audio Variance:</b> Variance when selecting audio files. Higher variance means less repetition of the same audio, but higher variance in intensity.\n\n" +
				"<b>Rhythm Randomness:</b> Artificial randomness on wait time between breaths. Use as alternative to 'Audio Variance'.\n\n" +
				"<b>Rhythm Damping:</b> Damping on duration of each breath. This is achieved by adjusting wait time between breaths. Higher values mean a more regular breathing, but slower adjustment to different intensities.\n\n" +
				"<b>Volume:</b> Volume of the AudioSource. Note that this plugin continuously overrides the HeadAudio volume of your Person atom.\n\n",
				1200.0f, true
			);
			
			List<string> datasets = new List<string> {
				"Candy Nose",
				"Candy Sensual",
				"Original",
				"Original Moan"
			};
			myDataset = new JSONStorableStringChooser("Dataset", datasets, datasets[0], "Dataset");
			myDataset.storeType = JSONStorableParam.StoreType.Full;
			CreateScrollablePopup(myDataset, false);
			RegisterStringChooser(myDataset);
			
			myIntensity = Utils.SetupSliderFloat(this, "Intensity", 0.0f, 0.0f, 1.0f, false);
			myVariance = Utils.SetupSliderFloat(this, "Audio Variance", 8.0f, 5.0f, 12.0f, false);
			myVariance.setCallbackFunction += (float v) => {
				float r = Mathf.Round(v);
				if (myVariance.val != r)
					myVariance.val = r;
			};
			myRhythmRandomness = Utils.SetupSliderFloat(this, "Rhythm Randomness", 0.07f, 0.0f, 0.2f, false);
			myRhythmDamping = Utils.SetupSliderFloat(this, "Rhythm Damping", 0.3f, 0.0f, 0.9f, false);
			
			myVolume = Utils.SetupSliderFloat(this, "Volume", 0.5f, 0.0f, 1.0f, false);
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
		}
		
		private void OnDestroy()
        {
			Cleanup();
            if (myBundleURL != null)
                AssetLoader.DoneWithAssetBundleFromFile(myBundleURL);
			myBundleURL = null;
			myEvents.Clear();
        }
		
		private void Cleanup()
		{
			myInitialized = false;
			myQueueInitialized = false;
			myQueue.Clear();
			myEntries.Clear();
			mySelection.Clear();
			myDurationDamped = -1;
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
			string formatStr = dataset["format"];			
			int format = FORMAT_INOUT;
			if (formatStr == "outin")
				format = FORMAT_OUTIN;
			JSONNode jsonVolume = dataset["volume"];
			float volume = jsonVolume != null ? dataset["volume"].AsFloat : 1.0f;
			
			JSONArray dataentries = dataset["entries"].AsArray;
			List<BreathEntry> entries = new List<BreathEntry>(dataentries.Count);
			myEntries[aDatasetName] = entries;
			bool skippedEntries = false;
			for (int i=0; i<dataentries.Count; ++i)
			{
				BreathEntry entry = new BreathEntry(myBundleRequest, path, format, volume, dataentries[i]);
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
		
		private void Update()
		{
			if (!myInitialized || SuperController.singleton.isLoading)
				return;
			
			if (myQueueInitialized)
			{
				CurrentTime += Time.deltaTime;
			}
			else
			{
				myQueue.Enqueue(new Breath());
				myQueue.Enqueue(new Breath() { Duration = UnityEngine.Random.Range(0.5f, 1.5f) });
				myQueue.Enqueue(new Breath());
				UpdateBreathTime();
				Choose();
				CurrentTime = Get(CURR).Start;
				myQueueInitialized = true;
				for (int i=0; i<myEvents.Count; ++i)
					myEvents[i].Reset();
			}
			
			if (CurrentTime >= Get(NEXT).Start)
				Enqueue();
			
			for (int i=0; i<myEvents.Count; ++i)
				myEvents[i].Update(this);
		}
		
		private void Interrupt()
		{		
			Choose();
			Breath curr = Get(CURR);
			Breath next = Get(NEXT);
			
			bool forceInterrupt = curr.IsValid() && next.IsValid() && curr.Entry.Format != next.Entry.Format;
			if (forceInterrupt)
				curr.InvalidTime = CurrentTime - curr.Start;
			for (int i=0; i<myEvents.Count; ++i)
				myEvents[i].Invalidate(this);
									
			if (forceInterrupt)
			{
				CurrentTime = next.Start;
				Enqueue();
			}
		}
		
		private void Enqueue()
		{
			CurrentTime -= Get(PREV).Duration;
			myQueue.Enqueue(new Breath());
			UpdateBreathTime();
			Choose();			
			
			for (int i=0; i<myEvents.Count; ++i)
				myEvents[i].Enqueue(this);
						
			BreathEntry currentEntry = Get(CURR).Entry;
			myHeadAudioSource.volume = Mathf.Clamp01(myVolume.val * currentEntry.Volume);
			myHeadAudioSource.pitch = 1.0f; // Force pitch to avoid desyncs!
			myHeadAudioSource.PlayNow(currentEntry.Clip);
			//SuperController.LogMessage("Playing " + currentEntry.Name);
		}
		
		private void Choose()
		{
			Breath currentBreath = Get(CURR);
			Breath nextBreath = Get(NEXT);
			nextBreath.Start = currentBreath.Start + currentBreath.Duration;
			
			List<BreathEntry> entries;
			if (myEntries.TryGetValue(myDataset.val, out entries))
			{	
				float variance = myVariance.val * 0.5f;				
				float intensity = Mathf.Lerp(variance-0.5f, entries.Count-variance-0.5f, myIntensity.val);
				float fmin = intensity - variance;
				float fmax = intensity + variance;
				int min = Mathf.RoundToInt(fmin);
				int max = Mathf.RoundToInt(fmax);
				if (UnityEngine.Random.value > min + 0.5f - fmin)
					min++;
				if (UnityEngine.Random.value > fmax - max + 0.5f)
					max--;
				min = Mathf.Clamp(min, 0, entries.Count-1);
				max = Mathf.Clamp(max, 0, entries.Count-1);
				
				mySelection.Clear();				
				BreathEntry curr = Get(CURR).Entry;
				BreathEntry prev = Get(PREV).Entry;
				bool hasCurr = false;
				bool hasPrev = false;
				for (int i=min; i<=max; ++i)					
				{
					BreathEntry entry = entries[i];
					if      (curr == entry) { hasCurr = true; continue; }
					else if (prev == entry) { hasPrev = true; continue; }
					mySelection.Add(entry);
				}
				
				// Fallbacks in case there are not enough nextBreaths to select from. Allow some more repetition
				if (mySelection.Count < 2 && hasPrev)
					mySelection.Add(prev); 
				if (mySelection.Count < 1 && hasCurr)
					mySelection.Add(curr);
				
				if (mySelection.Count > 0)
				{
					int selectionIdx = UnityEngine.Random.Range(0, mySelection.Count);
					BreathEntry entry = mySelection[selectionIdx];									
					float nextBreathEnd = entry.GetFileEndTime();
					if (nextBreathEnd < 0)
						nextBreathEnd = 1.0f;
					float nextFile = entry.GetFileNextTime();
					if (nextFile < 0)
						nextFile = nextBreathEnd + 0.1f;
					else if (nextFile < nextBreathEnd)
						nextFile = nextBreathEnd;
					
					float wait = nextFile - nextBreathEnd;
					float r = myRhythmRandomness.val;
					float duration = nextBreathEnd + Mathf.Max(wait * UnityEngine.Random.Range(1-r, 1+r), 0.05f);
					if (myDurationDamped > 0)
						myDurationDamped = Mathf.Lerp(duration, myDurationDamped, myRhythmDamping.val);
					else
						myDurationDamped = duration;										
					
					nextBreath.Duration = Mathf.Max(myDurationDamped, nextBreathEnd + 0.05f);
					nextBreath.Entry = entry;
					nextBreath.Intensity = myIntensity.val;
					nextBreath.Depth = (entry.Depth >= 0.0f) ? entry.Depth : (nextBreath.Intensity*0.5f+0.5f);
					nextBreath.Power = UnityEngine.Random.Range(0.8f, 1.2f);
				}
			}
		}		
		
		private void UpdateBreathTime()
		{
			float time = 0.0f;
			for (int i=0; i<GetCapacity(); ++i)
			{
				Breath b = myQueue.Get(i);
				b.Start = time;
				time += b.Duration;
			}
		}
		
		
		////////////////////////////////////////////////////////////////////////////////////
		// Helpers
			
		public Breath Get(int index)
		{
			return myQueue.Get(index);
		}
		
		public int GetCapacity() { return MiniQueue<Breath>.Capacity; }
		
		/*private void FullLog()
		{			
			string msg = "FullLog: " + CurrentTime + "\n";			
			for (int i=0; i<GetCapacity(); ++i)
			{
				Breath b = myQueue.Get(i);
				msg += "[" + i + "] ## Start=" + b.Start + ", Duration=" + b.Duration + ", End=" + (b.Start+b.Duration) + ", Invalid=" + b.InvalidTime + "\n";
				if (b.IsValid())
				{
					msg += "    ## ";
					int len = msg.Length;
					for (int j=0; j<b.Entry.Markers.Count; ++j)
					{
						BreathMarker m = b.Entry.Markers[j];
						if (m.NameHash == MRK_BreathIn)
							msg += "MRK_BreathIn=" + (b.Start+m.Time);
						else if (m.NameHash == MRK_HoldIn)
							msg += "MRK_HoldIn=" + (b.Start+m.Time);
						else if (m.NameHash == MRK_Interrupt)
							msg += "MRK_Interrupt=" + (b.Start+m.Time);
						msg += ", ";
					}
					msg += "\n";
				}
			}
			SuperController.LogMessage(msg);
		}*/
		
		////////////////////////////////////////////////////////////////////////////////////
		// Datastructures
		
		private class MiniQueue<T> where T : new()
		{
			public const int Capacity = 3;
			
			private int position = 0;
			private T[] data = new T[Capacity];
			
			public T Get(int index) {
				return data[(position + index) % Capacity];
			}
			
			public void Enqueue(T t)
			{
				data[position] = t;
				position = (position+1) % Capacity;
			}
			
			public void Clear()
			{
				for (int i=0; i<Capacity; ++i)
					data[i] = new T();
				position = 0;
			}
		}
		
		public class Breath
		{
			public BreathEntry Entry;
			public float Start;
			public float Duration = 1.0f;
			public float Intensity = 0.0f;
			public float Depth = 1.0f;
			public float Power = 1.0f;
			public float InvalidTime = float.MaxValue;
			
			public bool IsValid()
			{
				return Entry != null;
			}
		}
		
		public class BreathEntry
		{
			public AssetBundleAudioClip Clip { get; private set; }
			public string Name { get; private set; }
			public int Tags { get; private set; }
			public int Format { get; private set; }
			public float Depth { get; private set; }
			public float Volume { get; private set; }
			public List<BreathMarker> Markers { get; private set; }
			
			public BreathEntry(Request aRequest, string aPath, int aFormat, float aVolume, JSONNode aEntryData)
			{
				Name = aPath + aEntryData["file"].Value;
				Format = aFormat;
				Volume = aVolume;
				
				Tags = 0;
				string tags = aEntryData["tags"].Value;
				if (tags != null)
				{
					string[] tagsList = tags.Split(',');
					if (Array.Exists(tagsList, t => t == "moan"))						
						Tags |= TAG_MOAN;
					if (Array.Exists(tagsList, t => t == "noseIn"))
						Tags |= TAG_NOSEIN;
					if (Array.Exists(tagsList, t => t == "noseOut"))
						Tags |= TAG_NOSEOUT;
					if (Array.Exists(tagsList, t => t == "multi"))
						Tags |= TAG_MULTI;
				}				
				
				JSONArray markers = aEntryData["markers"].AsArray;
				Markers = new List<BreathMarker>(markers.Count+2);
				BreathMarker m;
				for (int i=0; i<markers.Count; ++i)
				{
					JSONArray a = markers[i].AsArray;					
					m.NameHash = a[0].Value.GetHashCode();
					m.Time = a[1].AsFloat;
					Markers.Add(m);
				}
				
				m.NameHash = MRK_Start;
				m.Time = 0.0f;
				Markers.Add(m);
				m.NameHash = MRK_End;
				m.Time = float.MaxValue;
				Markers.Add(m);
				
				if (!HasTag(TAG_MULTI))
				{
					int m1 = -1, m2 = -1;
					if (aFormat == FORMAT_INOUT)
					{
						m1 = Markers.FindIndex(bm => bm.NameHash == MRK_HoldIn);
						m2 = Markers.FindIndex(bm => bm.NameHash == MRK_BreathOut);					
					}
					else if (aFormat == FORMAT_OUTIN)
					{
						m1 = Markers.FindIndex(bm => bm.NameHash == MRK_HoldOut);
						m2 = Markers.FindIndex(bm => bm.NameHash == MRK_BreathIn);
					}
					if (m1 >= 0 && m2 >= 0)		
					{
						m.NameHash = MRK_Interrupt;
						m.Time = (Markers[m1].Time + Markers[m2].Time) * 0.5f;
						Markers.Add(m);
					}
				}
				
				Markers.Sort();
				
				JSONNode depthNode = aEntryData["depth"];
				if (depthNode != null)
					Depth = depthNode.AsFloat;
				else
					Depth = -1;
				
				Clip = new AssetBundleAudioClip(aRequest, BASE_PATH, Name);
			}
			
			public bool IsValid()
			{
				return (Clip?.sourceClip) != null;
			}
			
			public bool HasTag(int aTag)
			{
				return (Tags & aTag) > 0;
			}
			
			public float GetFileEndTime()
			{
				int idx = Markers.FindLastIndex(m => m.NameHash == MRK_BreathEnd);
				if (idx >= 0)
					return Markers[idx].Time;
				else
					return -1.0f;
			}
			
			public float GetFileNextTime()
			{
				int idx = Markers.FindLastIndex(m => m.NameHash == MRK_FileNext);
				if (idx >= 0)
					return Markers[idx].Time;
				else
					return -1.0f;
			}
		}
		
		public struct BreathMarker : IComparable<BreathMarker>
		{
			public int NameHash;
			public float Time;
			
			public int CompareTo(BreathMarker aOther)
			{
				return Time.CompareTo(aOther.Time);
			}
		}
	}
}