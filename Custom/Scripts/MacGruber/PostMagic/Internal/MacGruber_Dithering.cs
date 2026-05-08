using UnityEngine;
using UnityEngine.PostProcessing;
using System;

namespace MacGruber
{
	namespace PostMagic 
	{		
		public class Dithering : MVRScript
		{
			private Manager manager;
			private DitheringModel model;
			
			private JSONStorableBool active;
						
			public override void Init()
			{
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.dithering;
				
				Utils.SetupInfoText(this,
					"<color=red><b>EXPERIMENTAL:</b> Not sure this is actually doing anything!</color>\n\n" +
					"Dithering is the process of intentionally applying noise as to randomize quantization error. This prevents large-scale patterns such as color banding in images.\n\n",
					800.0f, true
				);
				
				active = Utils.SetupToggle(this, "Dithering Enabled", false, false);
				active.setCallbackFunction  += (bool v) => { model.enabled = enabledJSON.val = v; };
								
				active.setCallbackFunction(active.val);
				enabledJSON.setCallbackFunction += (bool v) => { active.val = v; };
			}
		}
	}
}