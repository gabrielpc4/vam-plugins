using UnityEngine;
using System;
using System.Collections.Generic;

namespace MacGruber
{
	public partial class IdleMotion : MVRScript
	{
		private Motion[] myMotions = new Motion[] {
			/*new Motion("headControl",   new Vector3(-10,-10,-10), new Vector3(10,10,10), 8.3f, 1.0f, false),
			new Motion("neckControl",   new Vector3(-10,-10,-10), new Vector3(10,10,10), 5.3f, 1.0f, true),
			new Motion("lArmControl",   new Vector3(  0,-20,  0), new Vector3( 0,20, 0), 5.3f, 1.0f, true),
			new Motion("lHandControl",  new Vector3(-40,  0,  0), new Vector3(40, 0, 0), 5.3f, 1.0f, true),
			new Motion("lThighControl", new Vector3(-30,  0,  0), new Vector3(30, 0, 0), 5.3f, 1.0f, true),
			new Motion("lKneeControl",  new Vector3(-30,  0,  0), new Vector3(-10,0, 0), 5.3f, 1.0f, true),
			new Motion("lFootControl",  new Vector3(-10,-10,-10), new Vector3(10,10,10), 8.3f, 1.0f, false)*/
			
			new Motion("*ArmControl",   -20, 20, 5.3f, true, true, 0.8f, 1.2f, 1.2f, new RotationZ(), new RotationZ()),
			new Motion("*HandControl",  -40, 40, 4.1f, true, true, 0.5f, 1.5f, 1.0f, new RotationX(), new RotationX()),
			new Motion("*HandControl",  -10, 10, 7.9f, true, true, 0.5f, 1.5f, 1.5f, new RotationX(), new RotationX()),
			new Motion("*ThighControl", -30, 30, 6.2f, true, true, 0.8f, 1.2f, 1.0f, new RotationX(), new RotationX()),
			new Motion("*ThighControl",  -2,  2, 5.8f, true, true, 0.8f, 1.2f, 1.2f, new RotationZ(), new RotationZ()),
		};
				
		public override void Init()
		{
			for (int i=0; i<myMotions.Length; ++i)
				myMotions[i].FindControl(this);
		}
		
		private void Update()
		{
			for (int i=0; i<myMotions.Length; ++i)
				myMotions[i].Update();
		}
		
		private abstract class Setter
		{
			protected string myName;
			protected FreeControllerV3 myControl;
			protected float myVarianceMin;
			protected float myVarianceMax;
			protected float myTarget;
			protected float myVelocity;
			protected float mySmoothTime;
			
			public void Init(string aName, float aVarianceMin, float aVarianceMax, float aSmoothTime)
			{
				myName = aName;
				myControl = null;
				myVarianceMin = aVarianceMin;
				myVarianceMax = aVarianceMax;
				mySmoothTime = aSmoothTime;
			}
			
			public void FindControl(IdleMotion self)
			{
				myControl = self.containingAtom.GetStorableByID(myName) as FreeControllerV3;
			}
			
			public void SetTarget(float aTarget)
			{
				float variance = UnityEngine.Random.Range(myVarianceMin, myVarianceMax);
				myTarget = variance * aTarget;
			}
			
			public void Update()
			{
				Value = Mathf.SmoothDamp(Value, myTarget, ref myVelocity, mySmoothTime);
			}
			
			protected virtual float Value { get; set; }
		}
		
		private class RotationX : Setter
		{	
			protected override float Value {
				get { return myControl.jointRotationDriveXTarget; }
				set { myControl.jointRotationDriveXTarget = value; }
			}
		}
		
		private class RotationY : Setter
		{	
			protected override float Value {
				get { return myControl.jointRotationDriveYTarget; }
				set { myControl.jointRotationDriveYTarget = value; }
			}
		}
		
		private class RotationZ : Setter
		{	
			protected override float Value {
				get { return myControl.jointRotationDriveZTarget; }
				set { myControl.jointRotationDriveZTarget = value; }
			}
		}
		
		
		private class Motion
		{
			private float myMin;
			private float myMax;
			private float myPeriod;
			private bool myEnabled;
			
			private float myClock;
			private Setter mySetterA;
			private Setter mySetterB;
			
			public Motion(string myName, float aMin, float aMax, float aPeriod, bool aEnabled, bool aMirror,
			              float aVarianceMin, float aVarianceMax, float aSmoothTime,
						  Setter aSetterA, Setter aSetterB)
			{
				myMin = aMin;
				myMax = aMax;
				myPeriod = aPeriod;
				myEnabled = aEnabled;
				
				mySetterA = aSetterA;
				mySetterB = aSetterB;
				if (mySetterA != null)
					mySetterA.Init(myName.Replace('*','l'), aVarianceMin, aVarianceMax, aSmoothTime);
				if (mySetterB != null)
					mySetterB.Init(myName.Replace('*','r'), 
					               aMirror ? -aVarianceMax : aVarianceMin,
								   aMirror ? -aVarianceMin : aVarianceMax,
								   aSmoothTime);
			}
			
			public void FindControl(IdleMotion self)
			{
				if (mySetterA != null)
					mySetterA.FindControl(self);
				if (mySetterB != null)
					mySetterB.FindControl(self);
			}
			
			public void Update()
			{
				if (myEnabled)					
				{
					myClock -= Time.deltaTime;
					if (myClock <= 0.0f)
					{
						myClock += UnityEngine.Random.Range(0.5f, 1.5f) * myPeriod;
						float target = UnityEngine.Random.Range(myMin, myMax);
						if (mySetterA != null)
							mySetterA.SetTarget(target);
						if (mySetterB != null)
							mySetterB.SetTarget(target);
					}

					if (mySetterA != null)
						mySetterA.Update();
					if (mySetterB != null)
						mySetterB.Update();
				}
			}
		}
	}	
}