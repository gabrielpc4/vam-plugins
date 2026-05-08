// Scene_Life03 v1.0 by MacGruber
// Scene controller plugin for scene "Life03", which is part of the Life plugin.

using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace MacGruber
{
    public class Scene_Life03 : MVRScript
    {	
		private JSONStorableFloat myBreathingIntensity;
		private JSONStorableFloat myThrustPower;
		private JSONStorableFloat myThrustMultiplier;
		private JSONStorableFloat myThrustOffset;
		private JSONStorableFloat myThrustSpeedDamping;
		
		private float intensityChangeClock = 0.0f;
		private float intensityChangeDuration = 0.0f;
		
        public override void Init()
		{
			if (containingAtom.type != "Person")
			{
				SuperController.LogError($"Requires atom of type person, instead selected: {containingAtom.type}");
				return;
			}

			JSONStorable breathing = FindPlugin("MacGruber.Breathing");
			JSONStorable thrust = FindPlugin("MacGruber.DriverThrust");
			if (breathing == null)
			{
				SuperController.LogError($"Scene_Life03 requires MacGruber.Breathing plugin!");
				return;
			}
			if (thrust == null)
			{
				SuperController.LogError($"Scene_Life03 requires MacGruber.DriverThrust plugin!");
				return;
			}
			
			myBreathingIntensity = breathing.GetFloatJSONParam("Intensity");
			myThrustPower        = thrust.GetFloatJSONParam("Power");
			myThrustMultiplier   = thrust.GetFloatJSONParam("Multiplier");
			myThrustOffset       = thrust.GetFloatJSONParam("Offset");
			myThrustSpeedDamping = thrust.GetFloatJSONParam("SpeedDamping");
		}
		
		private JSONStorable FindPlugin(string fullClassName)
		{
			List<string> names = containingAtom.GetStorableIDs();
			string pluginName = names.Find(s => s.StartsWith("plugin#") && s.EndsWith(fullClassName));
			JSONStorable plugin = containingAtom.GetStorableByID(pluginName);
			return plugin;
		}
		
        private void Update()
		{
			intensityChangeClock += Time.deltaTime;
			if (intensityChangeClock >= intensityChangeDuration)
			{
				if (intensityIndex < 0 || intensityIndex >= intensityData.Length)
					intensityIndex = 0;
				else if (intensityData[intensityIndex].choices != null)
				{
					int choice = UnityEngine.Random.Range(0, intensityData[intensityIndex].choices.Length);
					intensityIndex = intensityData[intensityIndex].choices[choice];
				}
				else
				{
					int choice = UnityEngine.Random.Range(0, intensityData.Length);
					intensityIndex = choice;
				}

				IntensityData data = intensityData[intensityIndex];
				myBreathingIntensity.val = data.breathingIntensity;
				myThrustMultiplier.val = data.thrustMultiplier;
				myThrustOffset.val = data.thrustOffset;
				myThrustSpeedDamping.val = data.thrustSpeedDamping;
				
				intensityChangeClock = 0.0f;
				intensityChangeDuration = UnityEngine.Random.Range(data.minDuration, data.maxDuration);
			}
			
			if (intensityIndex >= 0 && intensityIndex < intensityData.Length)
			{				
				IntensityData data = intensityData[intensityIndex];
				myThrustPower.val = Mathf.Lerp(myThrustPower.val, data.thrustPower, Mathf.Min(1.0f, Time.deltaTime));
			}
        }
		
		
		private class IntensityData
		{
			public float breathingIntensity = 0.5f;
			public float thrustPower = 0.5f;
			public float thrustMultiplier = 1.0f;
			public float thrustOffset = -0.2f;		
			public float thrustSpeedDamping = 1.0f;
			public float minDuration = 15.0f;
			public float maxDuration = 25.0f;
			public int[] choices = null;
		}
		
		private int intensityIndex = -1;
		private IntensityData[] intensityData = new IntensityData[] {
			new IntensityData() { // 0
				breathingIntensity = 0.2f,
				thrustPower = 0.0f,
				minDuration = 3.0f,
				maxDuration = 5.0f,
				choices = new int[]{1,1,2}
			},
			new IntensityData() { // 1
				breathingIntensity = 0.5f,
				thrustPower = 0.7f,
				minDuration = 10.0f,
				maxDuration = 15.0f,
				choices = new int[]{2,2,3}
			},
			new IntensityData() { // 2
				breathingIntensity = 0.75f,
				thrustPower = 0.9f,
				thrustOffset = -0.2f,
				choices = new int[]{0,1,3,3,4}
			},
			new IntensityData() { // 3
				breathingIntensity = 0.8f,
				thrustPower = 1.0f,
				thrustMultiplier = 2.0f,
				thrustOffset = 0.0f,
				thrustSpeedDamping = 0.2f,
				choices = new int[]{0,4,4}				
			},
			new IntensityData() { // 4
				breathingIntensity = 1.0f,
				thrustPower = 1.0f,
				thrustMultiplier = 2.0f,
				thrustOffset = 0.0f,
				thrustSpeedDamping = 0.2f,
				minDuration = 10.0f,
				maxDuration = 15.0f,
				choices = new int[]{0,2}
			}
		};
    }
}
