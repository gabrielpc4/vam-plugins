using UnityEngine;
using System;
using System.Collections.Generic;

namespace MacGruber
{
	public partial class Breathing : MVRScript
	{
		//////////////////////////////////////////////////////////////////////////////
		// Constants
		
		public static readonly int MRK_Invalid    = "invalid".GetHashCode();
		public static readonly int MRK_BreathOut  = "bo".GetHashCode();
		public static readonly int MRK_HoldOut    = "ho".GetHashCode();
		public static readonly int MRK_BreathIn   = "bi".GetHashCode();
		public static readonly int MRK_HoldIn     = "hi".GetHashCode();
		public static readonly int MRK_BreathEnd  = "be".GetHashCode();
		public static readonly int MRK_FileNext   = "nx".GetHashCode();
		public static readonly int MRK_Start      = "start".GetHashCode();
		public static readonly int MRK_Interrupt  = "it".GetHashCode();
		public static readonly int MRK_End        = "end".GetHashCode();
		
		//////////////////////////////////////////////////////////////////////////////
		// Helpers
		
		public static SimpleMarker SM(int aMarker) {
			return new SimpleMarker(aMarker, 0.0f);
		}
		public static SimpleMarker SM(int aMarker, float aMarkerOffset) {
			return new SimpleMarker(aMarker, aMarkerOffset);
		}
		public static SimpleMarker SM(int aMarker, int aMarkerSkip, float aMarkerOffset) {
			return new SimpleMarker(aMarker, aMarkerSkip, aMarkerOffset);
		}
		public static ClampMarker CM(int aMarker, int aReference) {
			return new ClampMarker(aMarker, 0.0f, aReference, 0.0f);
		}	
		public static ClampMarker CM(int aMarker, float aMarkerOffset, int aReference, float aReferenceOffset) {
			return new ClampMarker(aMarker, aMarkerOffset, aReference, aReferenceOffset);
		}
		public static ClampMarker CM(int aMarker, int aMarkerSkip, float aMarkerOffset, int aReference, int aReferenceSkip, float aReferenceOffset) {
			return new ClampMarker(aMarker, aMarkerSkip, aMarkerOffset, aReference, aReferenceSkip, aReferenceOffset);
		}
		public static RatioMarker RM(int aMarker, int aReference, float aRatio, float aMinOffset, float aMaxOffset) {
			return new RatioMarker(aMarker, aReference, aRatio, aMinOffset, aMaxOffset);
		}
		public static RatioMarker RM(int aMarker, int aMarkerSkip, int aReference, int aReferenceSkip, float aRatio, float aMinOffset, float aMaxOffset) {
			return new RatioMarker(aMarker, aMarkerSkip, aReference, aReferenceSkip, aRatio, aMinOffset, aMaxOffset);
		}
		
		//////////////////////////////////////////////////////////////////////////////
		// Registration
		
		public CallbackEvent RegisterCallbackEvent(BaseMarker aMarker, CallbackVoid aCallback)
		{
			CallbackEvent e = new CallbackEvent(aMarker, aCallback);
			myEvents.Add(e);
			return e;
		}
				
		public BlendEvent RegisterBlendEvent(BaseMarker aMarkerA, BaseMarker aMarkerB, BaseMarker aMarkerAdvance)
		{
			BlendEvent e = new BlendEvent(aMarkerA, aMarkerB, aMarkerAdvance);
			myEvents.Add(e);
			return e;
		}
		
		public void RegisterEvent(Event aEvent)
		{
			myEvents.Add(aEvent);
		}
			
		public void UnregisterEvent(Event aEvent)
		{
			myEvents.Remove(aEvent);
		}
		
		//////////////////////////////////////////////////////////////////////////////
		// Events
		
		public abstract class Event
		{
			public abstract float Time { get; }
			internal abstract void Reset();
			internal abstract void Enqueue(Breathing aBreathing);
			internal abstract void Invalidate(Breathing aBreathing);
			internal abstract void Update(Breathing aBreathing);
		}
		
		public delegate void CallbackVoid();
		
		public class CallbackEvent : Event
		{
			public override float Time { get { return Marker.Time; } }
			public BaseMarker Marker { get; private set; }			
			public CallbackVoid Callback;
			
			public CallbackEvent(BaseMarker aMarker, CallbackVoid aCallback)
			{				
				Marker = aMarker;
				Callback = aCallback;
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
				if (aBreathing.CurrentTime < Marker.Time)
					return;
										
				Marker.Advance(aBreathing);
				Callback();
			}
		}
		
		public interface BlendValue
		{
			float Value { get; }
		}			
						
		// Linear blend between two markers.
		public class BlendEvent : Event, BlendValue
		{
			public override float Time { get { return (TimeA+TimeB) * 0.5f; } }
			public float Value { get; private set; }
			
			public BaseMarker MarkerA { get; private set; }		
			public BaseMarker MarkerB { get; private set; }	
			public BaseMarker MarkerAdvance { get; private set; }		
			private float TimeA;
			private float TimeB;
			private const float MinTransitionTime = 0.002f;
			private float Velocity;

			public BlendEvent(BaseMarker aMarkerA, BaseMarker aMarkerB, BaseMarker aMarkerAdvance)
			{
				MarkerA = aMarkerA;
				MarkerB = aMarkerB;
				MarkerAdvance = aMarkerAdvance;
				TimeA = TimeB = 0.0f;
				Value = 0.0f;
			}
			
			internal override void Reset()
			{
				MarkerA.Reset();
				MarkerB.Reset();
				MarkerAdvance.Reset();
				TimeA = TimeB = 0.0f;
				Value = 0.0f;
			}
			
			internal override void Enqueue(Breathing aBreathing)
			{
				MarkerA.Enqueue(aBreathing);
				MarkerB.Enqueue(aBreathing);
				MarkerAdvance.Enqueue(aBreathing);
				UpdateInternal();
			}
			
			internal override void Invalidate(Breathing aBreathing)
			{			
				Breath b = aBreathing.Get(CURR);
				float t = MarkerAdvance.Time - b.Start;
				if (t >= b.InvalidTime && t <= b.Duration)
				{
					MarkerA.Advance(aBreathing);
					MarkerB.Advance(aBreathing);
					MarkerAdvance.Advance(aBreathing);
					Value = 0.0f;
					Velocity = 0.0f;
				}
				
				MarkerA.Invalidate(aBreathing);
				MarkerB.Invalidate(aBreathing);
				MarkerAdvance.Invalidate(aBreathing);
				UpdateInternal();
			}
			
			internal override void Update(Breathing aBreathing)
			{	
				if (aBreathing.CurrentTime >= MarkerAdvance.Time)
				{					
					MarkerA.Advance(aBreathing);
					MarkerB.Advance(aBreathing);
					MarkerAdvance.Advance(aBreathing);
					Value = 0.0f;
					Velocity = 0.0f;
					UpdateInternal();
				}
				
				float v = Mathf.Clamp01(Mathf.InverseLerp(TimeA, TimeB, aBreathing.CurrentTime));
				Value = Mathf.SmoothDamp(Value, v, ref Velocity, 1.0f/30.0f, 10.0f);
			}
			
			private void UpdateInternal()
			{
				TimeA = MarkerA.Time;
				TimeB = MarkerB.Time;
				if (TimeA+0.002f > TimeB)
				{
					float midpoint = (TimeA+TimeB)*0.5f;
					TimeA = midpoint-0.001f;
					TimeB = midpoint+0.001f;
				}
			}
		}
		
		//////////////////////////////////////////////////////////////////////////////
		// Markers
		
		public abstract class BaseMarker
		{
			public float Time { get; protected set; }
			
			public abstract bool IsValid();
			internal abstract void Reset();
			internal abstract void Advance(Breathing aBreathing);			
			internal abstract void Enqueue(Breathing aBreathing);
			internal abstract void Invalidate(Breathing aBreathing);
		}
		
		// Define a Time from an offset marker position.
		public class SimpleMarker : BaseMarker
		{			
			public float MarkerOffset;
			private MarkerImpl Marker;			
			
			public SimpleMarker(int aMarker, float aMarkerOffset)
				: this(aMarker, 0, aMarkerOffset)
			{ }
			
			public SimpleMarker(int aMarker, int aMarkerSkip, float aMarkerOffset)
			{
				Marker = new MarkerImpl(aMarker, aMarkerSkip);
				MarkerOffset = aMarkerOffset;
				Time = 0.0f;
			}
			
			public override bool IsValid()
			{
				return Marker.IsValid();
			}
			
			internal override void Reset()
			{
				Marker.Reset();
			}
			
			internal override void Advance(Breathing aBreathing)
			{	
				Marker.Advance(aBreathing);
				Update();
			}
			
			internal override void Enqueue(Breathing aBreathing)
			{
				Marker.Enqueue(aBreathing);
				Update();
			}
			
			internal override void Invalidate(Breathing aBreathing)
			{
				Marker.Invalidate(aBreathing);
				Update();
			}
			
			private void Update()
			{
				Time = Marker.Time + MarkerOffset;
			}
		}
		
		// Define a Time from an offset marker position. The time is clamped by the offseted reference marker.
		public class ClampMarker : BaseMarker
		{			
			public float MarkerOffset;
			public float ReferenceOffset;
			private MarkerImpl Marker;
			private MarkerImpl Reference;					
			
			public ClampMarker(int aMarker, float aMarkerOffset, int aReference, float aReferenceOffset)
				: this(aMarker, 0, aMarkerOffset, aReference, 0, aReferenceOffset)
			{ }
			
			public ClampMarker(int aMarker, int aMarkerSkip, float aMarkerOffset, int aReference, int aReferenceSkip, float aReferenceOffset)
			{
				Marker = new MarkerImpl(aMarker, aMarkerSkip);
				Reference = new MarkerImpl(aReference, aReferenceSkip);
				MarkerOffset = aMarkerOffset;
				ReferenceOffset = aReferenceOffset;
				Time = 0.0f;
			}
			
			public override bool IsValid()
			{
				return Marker.IsValid() && Reference.IsValid();
			}
			
			internal override void Reset()
			{
				Marker.Reset();
				Reference.Reset();
			}
			
			internal override void Advance(Breathing aBreathing)
			{	
				Marker.Advance(aBreathing);
				Reference.Advance(aBreathing);
				Update();
			}
			
			internal override void Enqueue(Breathing aBreathing)
			{
				Marker.Enqueue(aBreathing);
				Reference.Enqueue(aBreathing);
				Update();
			}
			
			internal override void Invalidate(Breathing aBreathing)
			{
				Marker.Invalidate(aBreathing);
				Reference.Invalidate(aBreathing);
				Update();
			}
			
			private void Update()
			{
				Time = Marker.Time + MarkerOffset;
				float c = Reference.Time+ReferenceOffset;				
				if (MarkerOffset < 0.0f)
				{
					c = Mathf.Min(Marker.Time, c);
					Time = Mathf.Clamp(Time, c, Marker.Time);
				}
				else
				{
					c = Mathf.Max(Marker.Time, c);
					Time = Mathf.Clamp(Time, Marker.Time, c);
				}
			}
		}
		
		// Define a Time as ratio between two Markers. The ratio is clamped by a min/max offset from the main marker.
		public class RatioMarker : BaseMarker
		{		
			public float Ratio;
			public float MinOffset;
			public float MaxOffset;
			
			private MarkerImpl Marker;
			private MarkerImpl Reference;		
			
			public RatioMarker(int aMarker, int aReference, float aRatio, float aMinOffset, float aMaxOffset)
				: this(aMarker, 0, aReference, 0, aRatio, aMinOffset, aMaxOffset)
			{ }
			
			public RatioMarker(int aMarker, int aMarkerSkip, int aReference, int aReferenceSkip, float aRatio, float aMinOffset, float aMaxOffset)
			{
				Marker = new MarkerImpl(aMarker, aMarkerSkip);
				Reference = new MarkerImpl(aReference, aReferenceSkip);
				Ratio = aRatio;
				MinOffset = aMinOffset;
				MaxOffset = aMaxOffset;
				Time = 0.0f;
			}
			
			public override bool IsValid()
			{
				return Marker.IsValid() && Reference.IsValid();
			}
			
			internal override void Reset()
			{
				Marker.Reset();
				Reference.Reset();
			}
			
			internal override void Advance(Breathing aBreathing)
			{	
				Marker.Advance(aBreathing);
				Reference.Advance(aBreathing);
				Update();
			}
			
			internal override void Enqueue(Breathing aBreathing)
			{
				Marker.Enqueue(aBreathing);
				Reference.Enqueue(aBreathing);
				Update();
			}
			
			internal override void Invalidate(Breathing aBreathing)
			{
				Marker.Invalidate(aBreathing);
				Reference.Invalidate(aBreathing);
				Update();
			}
			
			private void Update()
			{
				Time = Mathf.Lerp(Marker.Time, Reference.Time, Ratio);
				Time = Mathf.Clamp(Time-Marker.Time, MinOffset, MaxOffset) + Marker.Time;
			}
		}
	
		
		private struct MarkerImpl
		{
			public float Time { get; private set; }			
			public int MarkerHash { get; private set; }
			private sbyte MarkerIndex;
			private sbyte QueueIndex;
			private sbyte MarkerSkip;
			private sbyte MarkerSkipRemaining;
			
			public MarkerImpl()
			{
				QueueIndex = CURR;
				MarkerSkipRemaining = MarkerSkip = 0;
				MarkerHash = MRK_Invalid;
				MarkerIndex = 0;
				Time = 0.0f;
			}
			
			public MarkerImpl(int aMarkerHash)
			{
				QueueIndex = CURR;
				MarkerSkipRemaining = MarkerSkip = 0;
				MarkerHash = aMarkerHash;
				MarkerIndex = 0;
				Time = 0.0f;
			}
			
			public MarkerImpl(int aMarkerHash, int aMarkerSkip)
			{
				QueueIndex = CURR;
				MarkerSkipRemaining = MarkerSkip = (sbyte)aMarkerSkip;
				MarkerHash = aMarkerHash;
				MarkerIndex = 0;
				Time = 0.0f;
			}
			
			internal void Reset()
			{
				QueueIndex = CURR;
				MarkerIndex = 0;
				MarkerSkipRemaining = MarkerSkip;
			}
			
			public bool IsValid()
			{
				return MarkerHash >= 0;
			}
			
			internal void Advance(Breathing aBreathing)
			{	
				++MarkerIndex;
				Update(aBreathing);
			}
			
			internal void Enqueue(Breathing aBreathing)
			{
				QueueIndex -= 1;
				Update(aBreathing);
			}
			
			internal void Invalidate(Breathing aBreathing)
			{
				if (QueueIndex == CURR)
				{
					Breath b = aBreathing.Get(QueueIndex);
					if (Time >= b.Start + b.InvalidTime)
						QueueIndex = NEXT;
				}
				if (QueueIndex == NEXT)
					MarkerIndex = 0;
				Update(aBreathing);
			}
			
			private void Update(Breathing aBreathing)
			{
				QueueIndex = (sbyte)Mathf.Max(QueueIndex, 0);
				for (; QueueIndex<aBreathing.GetCapacity(); ++QueueIndex)
				{
					Breath b = aBreathing.Get(QueueIndex);
					if (b.IsValid())
					{
						List<BreathMarker> Markers = b.Entry.Markers;
						for (; MarkerIndex<Markers.Count; ++MarkerIndex)
						{
							if (Markers[MarkerIndex].NameHash != MarkerHash)
								continue;
							if (Markers[MarkerIndex].Time >= b.InvalidTime)
								continue;
							if (MarkerSkipRemaining > 0 && (MarkerSkipRemaining--) > 0)
								continue;
							float t = Markers[MarkerIndex].Time;							
							Time = b.Start + (t == float.MaxValue ? b.Duration : t);
							return;
						}
					}
					Time = b.Start + b.Duration;
					MarkerIndex = 0;
				}
			}
		}
		
		//////////////////////////////////////////////////////////////////////////////
		// BlendValue Combiners
			
		public class BlendCombineLinearInOut : BlendValue
		{
			private BlendEvent myIn;
			private BlendEvent myOut;
			
			public BlendCombineLinearInOut(BlendEvent aIn, BlendEvent aOut)
			{
				myIn = aIn;
				myOut = aOut;
			}
			
			public virtual float Value { get {
				float a = myIn.Value;
				float b = 1.0f - myOut.Value;
				return (myOut.Time < myIn.Time) ? Mathf.Max(a, b) : Mathf.Min(a, b);
			}}
		}
		
		public class BlendCombineSmoothInOut : BlendCombineLinearInOut
		{
			public BlendCombineSmoothInOut(BlendEvent aIn, BlendEvent aOut)
				: base(aIn, aOut)
			{ }
			
			public override float Value { get {
				return Mathf.SmoothStep(0.0f, 1.0f, base.Value);
			}}
		}
		
		public class BlendCombineLinearRemap : BlendValue
		{
			private BlendValue myBlend;
			private float myMidPoint;
			
			public BlendCombineLinearRemap(BlendValue aBlend, float aMidPoint)
			{
				myBlend = aBlend;
				myMidPoint = aMidPoint;
			}
			
			public virtual float Value { get {
				float v = myBlend.Value;
				v =  Mathf.InverseLerp(v <= myMidPoint ? 0.0f : 1.0f, myMidPoint, v);
				return v;
			}}
		}
		
		public class BlendCombineSmoothRemap : BlendCombineLinearRemap
		{
			public BlendCombineSmoothRemap(BlendValue aBlend, float aMidPoint)
				: base(aBlend, aMidPoint)
			{ }
			
			public override float Value { get {
				return Mathf.SmoothStep(0.0f, 1.0f, base.Value);
			}}
		}
	}	
}