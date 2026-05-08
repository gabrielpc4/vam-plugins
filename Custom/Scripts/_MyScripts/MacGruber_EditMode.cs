/* /////////////////////////////////////////////////////////////////////////////////////////////////
EditMode v0.2 by MacGruber.
Switch VaM to Edit mode on scene start. On desktop mode it also opens the MainMenu and enables targets.

Version 0.2 2020-03-21
	Adding to MacGruber-Toolkit Package.
	Adding info text to explain what this plugin is doing.
	
Version 0.1 2019-10-03
	Initial release.
	
///////////////////////////////////////////////////////////////////////////////////////////////// */


using UnityEngine;

namespace MacGruber
{
    public class EditMode : MVRScript
    {	
        public override void Init()
		{
			SetupInfoText(this, 
				"<color=#606060><size=40><b>EditMode v0.2</b></size>\nPut this plugin into your scene to automatically switch VaM to 'Edit Mode' on loading your scene. When launching on Desktop-mode, this does also enable 'Targets', the green icons for atoms.</color>",
				280.0f, true
			);			
			
			SuperController sc = SuperController.singleton;
			sc.gameMode = SuperController.GameMode.Edit;
			if (!sc.isOVR && !sc.isOpenVR)
			{
				if (!sc.GetTargetShow())
					sc.ToggleTargetsOnWithButton();
				sc.ShowMainHUDMonitor();
			}
		}
		
		private static JSONStorableString SetupInfoText(MVRScript script, string text, float height, bool rightSide)
		{
			JSONStorableString storable = new JSONStorableString("Info", text);
			UIDynamic textfield = script.CreateTextField(storable, rightSide);
			textfield.height = height;
			return storable;
		}
    }
}
