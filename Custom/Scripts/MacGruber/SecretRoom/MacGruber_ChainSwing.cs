/* /////////////////////////////////////////////////////////////////////////////////////////////////
ChainSwing v1.1 by MacGruber.
Apply simplified pendulum math to make a chain asset swing.
http://hyperphysics.phy-astr.gsu.edu/hbase/pend.html

Version 1.1 2019-10-11
	Added OffsetY parameter to allow rotated chains.

Version 1.0 2019-09-18
	Initial release.
	
///////////////////////////////////////////////////////////////////////////////////////////////// */

using UnityEngine;

namespace MacGruber
{
	public class ChainSwing : MVRScript
	{
		private JSONStorableFloat angleX;
		private JSONStorableFloat angleY;
		private JSONStorableFloat offsetY;
		private JSONStorableFloat angleZ;
		private JSONStorableFloat length;
		private Transform chain = null;
		
		private float t = 0;
		private float sqrtLG = 1.0f;
		private	float period = 1.0f;
		private const float gravity = 9.81f;
		
		public override void Init()
		{
			chain = null;
			JSONStorable control = containingAtom.GetStorableByID("control");
			if (control == null)
			{
				SuperController.LogMessage("No control storable found. This script only works on normal atoms, not with Person atoms.");
				return;
			}
			chain = control.transform;
			
			angleX = SetupSliderFloat("AngleX", 0.0f, -5.0f, 5.0f, false);					
			angleZ = SetupSliderFloat("AngleZ", 0.0f, -5.0f, 5.0f, false);
			length = SetupSliderFloat("ChainLength", 2.0f, 0.5f, 3.0f, false);			
			angleY = SetupSliderFloat("AngleY", 0.0f, -15.0f, 15.0f, true);
			offsetY = SetupSliderFloat("OffsetY", 0.0f, -180.0f, 180.0f, true);
			
			length.setCallbackFunction += PrecalcPeriod;
			
			PrecalcPeriod(length.val);
			t = Random.Range(0.0f, period);
		}
		
		private void FixedUpdate()
		{
			if (chain == null)
				return;
			
			t += Time.fixedDeltaTime;
			if (t >= period)
				t -= period;			
			float s = Mathf.Sin(t / sqrtLG);
			chain.localEulerAngles = new Vector3(angleX.val*s, offsetY.val+angleY.val*s, angleZ.val*s);
		}
		
		private void PrecalcPeriod(float length)
		{			
			sqrtLG = Mathf.Sqrt(length / gravity);
			period = 2.0f * Mathf.PI * sqrtLG;
		}
		
		private JSONStorableFloat SetupSliderFloat(string name, float defaultValue, float minValue, float maxValue, bool rightSide)
		{
			JSONStorableFloat storable = new JSONStorableFloat(name, defaultValue, minValue, maxValue, true, true);
			storable.storeType = JSONStorableParam.StoreType.Full;
			CreateSlider(storable, rightSide);
			RegisterFloat(storable);
			return storable;
		}
	}
}