using UnityEngine;
using System;
using static MacGruber.Breathing;
using static MacGruber.Utils;

namespace MacGruber
{
	public class DriverAP : MVRScript
	{
		private Breathing myBreathing;		
		private JSONStorableFloat myPower;
        private JSONStorableFloat myPosition;		
		private bool myAnimationValid = false;
		
		private BlendValue myPositionValue;	
		
		public override void Init()
		{
			myBreathing = FindWithinSamePlugin<Breathing>(this);
			InitUI();
			InitAnimation();
			InitEvents();
		}
		
		private void InitUI()
		{
			myPower = SetupSliderFloat(this, "Power", 1.0f, 0.0f, 1.0f, false);
        }
		
		private void InitAnimation()
		{			
            Atom atom = GetAtomById(containingAtom.name + "#AP"); 
            myPosition = atom?.GetStorableByID("AnimationPattern")?.GetFloatJSONParam("currentTime"); 			
			
			myAnimationValid = myPosition != null;
		}
		
		private void InitEvents()
		{
			/*BlendEvent value1 = myBreathing.RegisterBlendEvent(
				SM(MRK_BreathOut),
				RM(MRK_BreathOut, 0, MRK_BreathOut, 1, 0.5f, 0.0f, 10.0f),
				SM(MRK_BreathOut, 1, 0.0f)
			);
			BlendEvent value2 = myBreathing.RegisterBlendEvent(				
				RM(MRK_BreathOut, 1, MRK_BreathOut, 0, 0.5f, -10.0f, 0.0f),
				SM(MRK_BreathOut, 1, 0.0f),
				SM(MRK_BreathOut, 1, 0.0f)
			);*/
			BlendEvent value1 = myBreathing.RegisterBlendEvent(
				SM(MRK_BreathOut),
				SM(MRK_BreathIn),
				SM(MRK_BreathOut, 1, 0.0f)
			);
			BlendEvent value2 = myBreathing.RegisterBlendEvent(				
				SM(MRK_BreathIn),
				SM(MRK_BreathOut, 1, 0.0f),
				SM(MRK_BreathOut, 1, 0.0f)
			);
							
			myPositionValue = new BlendCombineSmoothInOut(value1, value2);
		}
		
		private void Update()
		{						
			Breath breath = myBreathing.Get(CURR);
			if (!breath.IsValid() || !myAnimationValid)
				return;
			
			float v = 1.0f - myPositionValue.Value;
			myPosition.val = v * myPower.val;
		}	
	}	
}