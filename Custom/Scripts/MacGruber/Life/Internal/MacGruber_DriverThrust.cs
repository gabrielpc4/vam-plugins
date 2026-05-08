using UnityEngine;
using System;
using System.Collections.Generic;
using static MacGruber.Breathing;
using static MacGruber.Utils;

namespace MacGruber
{
	public class DriverThrust : MVRScript
	{
		private Breathing myBreathing;
		private JSONStorableBool mySmooth;
		private JSONStorableBool myAPCompensation;
		private JSONStorableFloat myPower;
		private JSONStorableFloat myMultiplier;
		private JSONStorableFloat mySpeedDamping;
		private JSONStorableFloat myOffset;
		private JSONStorableFloat myUpDownBalance;
        private List<JSONStorableFloat> myValues = new List<JSONStorableFloat>();
		private ThrustEvent myThrustEvent;
		private bool myEventInit = false;
		private bool myWasLoading = true;

		private float myIncrement = 0.5f;
		private float myDirection = 1.0f;
		private float myPowerScale = 1.0f;
		private float myValue = -1;
		private float myEventUp = 0;
		private float myEventDown = 0;
		private int myMultiplierValue;

		public float Value { get { return myValue; } }
		public float DurationUp { get; private set; }
		public float DurationDown { get; private set; }
		public float Power { get { return myPower.val; } }

		public override void Init()
		{
			myBreathing = FindWithinSamePlugin<Breathing>(this);
			InitUI();
			InitAnimation();
			InitEvents();

			SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChanged;
		}

		private void InitUI()
		{
			Utils.SetupInfoText(this,
				"<color=#606060><size=40><b>DriverThrust</b></size>\nThis animation driver controls AnimationPatterns or VariableTriggers from breathing. " +
				"The plugin is searching for atoms whose name starts with 'Person#Thrust', where 'Person' is the name of your Person atom. You can control as many AnimationPatterns or VariableTriggers you want.\n" +
				"The animation should range from 0 to 1, where 0 is the 'inside' position. The plugin tries continuously adjust speed to " +
				"keep 'going in when breathing out'.</color>\n\n" +
				"<b>Power:</b> Range of animation being used. E.g. a value of 0.8 means only the range from 0.0 to 0.8 is used. Note that even with a power value of the 1.0 a part of the available range is used to smooth out animation velocity when breaths are not precisely regular.\n\n" +
				"<b>Smooth:</b> Apply classic 'SmoothStep' to output value instead of using linear motion.\n\n" +
				"<b>AP Compensation:</b> Compensate for VaM AnimationPatterns using cubic bezier curves for interpolation, making the motion more linear. Recommended when driving AnimationPatterns.\n\n" +
				"<b>Multiplier:</b> Number of thrusts per breath. Higher values are recommended to be used with high Rhythm Damping in the main plugin.\n\n" +
				"<b>Offset:</b> Sync offset in seconds between animation and audio. Generally you want to start the animation slightly earlier. Less so when using Multiplier.\n\n" +
				"<b>UpDownBalance:</b> When using suction devices it can be useful that it should suck slightly longer than blow, to prevent it falling off. Value in seconds, positive values mean going down takes longer.\n\n" +
				"<b>Rhythm Randomness:</b> Artificial randomness on wait time between breaths. <color=#606060>Slider linked from Breathing plugin for convenience.</color>\n\n" +
				"<b>Rhythm Damping:</b> Damping on duration of each breath. Increase if you have trouble with syncing thrust animation to audio. <color=#606060>Slider linked from Breathing plugin for convenience.</color>\n\n",
				1200.0f, true
			);

			myPower = SetupSliderFloat(this, "Power", 1.0f, 0.0f, 1.0f, false);
			mySmooth = SetupToggle(this, "Smooth", false, false);
			myAPCompensation = SetupToggle(this, "AP Compensation", true, false);
			myMultiplier = SetupSliderFloat(this, "Multiplier", 1.0f, 1.0f, 5.0f, false);
			myMultiplier.setCallbackFunction  += (float v) => {
				myMultiplier.valNoCallback = Mathf.Round(v);
			};

			myOffset = SetupSliderFloat(this, "Offset", -0.1f, -0.3f, 0.1f, false);
			myUpDownBalance = SetupSliderFloat(this, "UpDownBalance", 0.0f, -0.5f, 0.5f, false);
			
			
			JSONStorableFloat rrm = myBreathing.myRhythmRandomness;
			JSONStorableFloat rrl = Utils.SetupSliderFloat(this, rrm.name, rrm.defaultVal, rrm.min, rrm.max, false);
			rrm.setCallbackFunction += (float v) => { rrl.valNoCallback = v; };
			rrl.setCallbackFunction += (float v) => { rrm.val = v; };
			rrl.valNoCallback = rrm.val;
			
			JSONStorableFloat rdm = myBreathing.myRhythmDamping;
			JSONStorableFloat rdl = Utils.SetupSliderFloat(this, rdm.name, rdm.defaultVal, rdm.min, rdm.max, false);
			rdm.setCallbackFunction += (float v) => { rdl.valNoCallback = v; };
			rdl.setCallbackFunction += (float v) => { rdm.val = v; };
			rdl.valNoCallback = rdm.val;
        }

		private void InitAnimation()
		{
			myValues.Clear();
			string prefix = containingAtom.name + "#Thrust";
			List<string> atoms = GetAtomUIDs();
			for (int i=0; i<atoms.Count; ++i)
			{
				if (!atoms[i].StartsWith(prefix))
					continue;

				Atom atom = GetAtomById(atoms[i]);
				if (atom == null)
					continue;
				JSONStorableFloat v = null;
				if (atom.type == "AnimationPattern")
					v = atom?.GetStorableByID("AnimationPattern")?.GetFloatJSONParam("currentTime");
				else if (atom.type == "VariableTrigger")
					v = atom?.GetStorableByID("Trigger")?.GetFloatJSONParam("value");

				if (v != null)
					myValues.Add(v);
			}

			if (myValue < 0.0f)
			{
				if (myValues.Count > 0 && myPower.val > 0.001f)
					myValue = Mathf.Clamp01(myValues[0].val / myPower.val);
				else
					myValue = 0.0f;

				myMultiplierValue = Mathf.RoundToInt(myMultiplier.val);
				myIncrement = 2.0f * myMultiplier.val / 3.0f;
				myDirection = 1.0f;
				myPowerScale = 1.0f;
			}
		}

		private void InitEvents()
		{
			myThrustEvent = new ThrustEvent(this);
			myBreathing.RegisterEvent(myThrustEvent);

			myOffset.setCallbackFunction  += (float v) => {
				myThrustEvent.Offset = v;
			};
			myOffset.setCallbackFunction(myOffset.val);
			myEventInit = true;
		}

		private void OnEnable()
		{
			if (myEventInit)
			{
				myBreathing.UnregisterEvent(myThrustEvent);
				myBreathing.RegisterEvent(myThrustEvent);
			}
		}

		private void Update()
		{
			bool isLoading = SuperController.singleton.isLoading;
			if (!isLoading && myWasLoading)
				InitAnimation();
			myWasLoading = isLoading;
		}

		private void OnDisable()
		{
			if (myEventInit)
				myBreathing.UnregisterEvent(myThrustEvent);
			myWasLoading = true;
		}

		private void OnDestroy()
		{
			SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChanged;
		}

		private void OnAtomUIDsChanged(List<string> atomUIDs)
		{
			myWasLoading = true;
		}

		private void ThrustUpdate()
		{
			float duration = 1.0f / myIncrement;
			float balanceUp = Mathf.Min(myUpDownBalance.val * 0.5f, Mathf.Max(duration-0.05f, 0.0f));
			float balanceDown = myUpDownBalance.val - balanceUp;
			DurationUp = duration - balanceUp;
			DurationDown = duration + balanceDown;
			float actualIncrement = myDirection / (myDirection > 0 ? DurationUp : DurationDown);
			myValue = Mathf.Clamp01(myValue + actualIncrement * Time.deltaTime);

			TriggerUpdate();

			float power = myPower.val * myPowerScale / POWER_SCALE_MAX;
			float v = myValue;
			if (myAPCompensation.val)
				v = 1.0f-Mathf.Pow(1.0f-v, 1.6f-1.2f*Mathf.Abs(power-0.5f));
			if (mySmooth.val)
				v = Mathf.SmoothStep(0.0f, power, v);
			else
				v = power*v;

			for (int i=0; i<myValues.Count; ++i)
				myValues[i].val = v;

			if (myDirection > 0.0f && myValue < 1.0f)
				return;
			if (myDirection < 0.0f && myValue > 0.0f)
				return;
			
			
			myDirection = -myDirection;
			bool isDown = myValue <= 0.0f;
			
			TriggerReset();

			// gradually increase/decrease multiplier for smoother transition
			int multiplierValue = Mathf.RoundToInt(myMultiplier.val);
			if (multiplierValue < myMultiplierValue)
				--myMultiplierValue;
			else if (multiplierValue > myMultiplierValue)
				++myMultiplierValue;
			
			// compute required velocity for sync
			float available = myThrustEvent.Time - myBreathing.CurrentTime;
			float count = available * 2.0f * myMultiplierValue / myThrustEvent.Duration;
			if (isDown)
				count = Mathf.Round(count*0.5f+0.5f)*2.0f-1.0f; // round to odd
			else
				count = Mathf.Round(count*0.5f)*2.0f; // round to even			
			float powerScale = myPowerScale;
			float velocity = (count>=0.5f) ? (count/available) : (2.0f*myMultiplierValue/myThrustEvent.Duration);
			float factor = velocity / (myIncrement*myPowerScale);
			
			// partially compensate velocity difference by varying power (=range of motion)
			if (isDown && factor < VELOCITY_FACTOR_THRESHOLD_MIN)
				powerScale = Mathf.Max(factor/VELOCITY_FACTOR_THRESHOLD_MIN, POWER_SCALE_MIN);
			else if (isDown && factor > VELOCITY_FACTOR_THRESHOLD_MAX)
				powerScale = Mathf.Min(factor/VELOCITY_FACTOR_THRESHOLD_MAX, POWER_SCALE_MAX);
			
			// hard clamp velocity difference, compute new increment		
			factor = Mathf.Clamp(factor, VELOCITY_FACTOR_MIN, VELOCITY_FACTOR_MAX);
			myIncrement = factor * myPowerScale/powerScale * myIncrement;
			myPowerScale = powerScale;
		}

		private const float POWER_SCALE_MIN = 0.7f;
		private const float POWER_SCALE_MAX = 1.2f;
		private const float VELOCITY_FACTOR_THRESHOLD_MIN = 0.85f;
		private const float VELOCITY_FACTOR_THRESHOLD_MAX = 1.15f;
		private const float VELOCITY_FACTOR_MIN = 0.66f;
		private const float VELOCITY_FACTOR_MAX = 1.50f;

		private void TriggerUpdate()
		{
			float signedIncrement = myIncrement*myDirection;
			myEventUp = Mathf.Max(1.0f - myValue, 0.0f) / -signedIncrement;
			myEventDown = Mathf.Max(myValue, 0.0f) / signedIncrement;
			for (int i=0; i<myTriggers.Count; ++i)
			{
				Trigger t = myTriggers[i];
				if (t.myWasTriggered)
					continue;

				t.myWasTriggered = t.myTriggerType == TRG_Down && myEventDown >= t.myOffset
				                || t.myTriggerType == TRG_Up   && myEventUp   >= t.myOffset;
				if (t.myWasTriggered)
					t.myCallback();
			}
		}

		private void TriggerReset()
		{
			int triggerTypeToReset = (myValue < 0.5f) ? TRG_Up : TRG_Down;
			for (int i=0; i<myTriggers.Count; ++i)
			{
				Trigger t = myTriggers[i];
				if (t.myTriggerType == triggerTypeToReset)
					t.myWasTriggered = false;
			}
		}

		private class ThrustEvent : MacGruber.Breathing.Event
		{
			public override float Time { get { return Marker.Time; } }
			public float Offset { get { return Marker.MarkerOffset; } set { Marker.MarkerOffset = value; } }
			public float Duration;
			private SimpleMarker Marker;
			private DriverThrust Driver;

			public ThrustEvent(DriverThrust aDriver)
			{
				Marker = SM(MRK_BreathOut);
				Driver = aDriver;
				Duration = 3.0f;
			}

			internal override void Reset()
			{
				Marker.Reset();
			}

			internal override void Enqueue(Breathing aBreathing)
			{
				Marker.Enqueue(aBreathing);
			}

			internal override void Invalidate(Breathing aBreathing)
			{
				Marker.Invalidate(aBreathing);
			}

			internal override void Update(Breathing aBreathing)
			{
				if (aBreathing.CurrentTime >= Marker.Time)
				{
					float previousTime = Marker.Time;
					Marker.Advance(aBreathing);
					Duration = Marker.Time - previousTime;
				}
				Driver.ThrustUpdate();
			}
		}

		public static readonly int TRG_Invalid = -1;
		public static readonly int TRG_Down = 0;
		public static readonly int TRG_Up = 1;

		public class Trigger
		{
			public int myTriggerType = TRG_Invalid;
			public float myOffset = 0.0f;
			public CallbackVoid myCallback;
			public bool myWasTriggered = false;
		}

		public Trigger RegisterTrigger(int triggerType, float offset, CallbackVoid callback)
		{
			Trigger t = new Trigger() {
				myTriggerType = triggerType,
				myOffset = offset,
				myCallback = callback
			};
			myTriggers.Add(t);
			return t;
		}

		public void UnregisterTrigger(Trigger trigger)
		{
			myTriggers.Remove(trigger);
		}

		private List<Trigger> myTriggers = new List<Trigger>();
	}
}