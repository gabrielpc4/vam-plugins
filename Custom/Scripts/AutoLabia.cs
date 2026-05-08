using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace VeeRifter
{
    public class AutoLabia : MVRScript
    {
		private static FreeControllerV3 chest;		
		private static FreeControllerV3 lKnee;
		private static FreeControllerV3 rKnee;
        private DAZMorph labiaMorph;
		protected JSONStorableFloat labiaMorphChangeMin;
		protected JSONStorableFloat labiaMorphChangeMax;
		
        public override void Init()
        {
            try
            {
				if (containingAtom.type != "Person")
				{
					SuperController.LogError($"Plugin for use with 'Person' atom, not '{containingAtom.type}'");
					return;
				}				
				pluginLabelJSON.val = "AutoLabia v3.0 - VeeRifter";			
                JSONStorable js = containingAtom.GetStorableByID("geometry");
                if (js != null)
                {
                    DAZCharacterSelector dcs = js as DAZCharacterSelector;
                    GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;
                    if (morphUI != null)
                    {
                        labiaMorph = morphUI.GetMorphByDisplayName("Genitals-extreme expansion");
						if (labiaMorph == null)
						{
							SuperController.LogError("Couldn't find morph 'Genitals-extreme expansion'");
							return;
						}
                    }
                }
				chest = containingAtom.GetStorableByID("chestControl") as FreeControllerV3;				
				lKnee = containingAtom.GetStorableByID("lKneeControl") as FreeControllerV3;				
				rKnee = containingAtom.GetStorableByID("rKneeControl") as FreeControllerV3;

				labiaMorphChangeMin = new JSONStorableFloat("Labia Morph Min", 0f, lmin => labiaMorphChangeMax.SetVal(Mathf.Max(labiaMorphChangeMax.val, lmin)), -1f, 1f, false, true);
				labiaMorphChangeMin.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(labiaMorphChangeMin);
				CreateSlider(labiaMorphChangeMin, false);

				labiaMorphChangeMax = new JSONStorableFloat("Labia Morph Max", 0.5f, lmax => labiaMorphChangeMin.SetVal(Mathf.Min(labiaMorphChangeMin.val, lmax)), 0f, 2f, false, true);
				labiaMorphChangeMax.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(labiaMorphChangeMax);
				CreateSlider(labiaMorphChangeMax, true);				
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        public void Update()
        {
			Vector3 chestPos = chest.followWhenOff.position;			
			Vector3 lKneePos = lKnee.followWhenOff.position;
			Vector3 rKneePos = rKnee.followWhenOff.position;
			Vector3 direction = lKneePos - rKneePos;
			float rKneeTolKnee = direction.magnitude;
			direction = lKneePos - chestPos;
			float lKneeToChest = direction.magnitude;
			direction = rKneePos - chestPos;
			float rKneeToChest = direction.magnitude;
			float spread = rKneeTolKnee - (Mathf.Abs(lKneeToChest - rKneeToChest) * 2);
			labiaMorph.morphValue = Remap(spread, 0, 1, labiaMorphChangeMin.val, labiaMorphChangeMax.val);		
        }
		
        private float Remap(float inVal, float min1, float max1, float min2, float max2)
        {
            var ratio = (max2 - min2) / (max1 - min1);
            var c = min2 - ratio * min1;			
            return ratio * inVal + c;
        }
    }
}
