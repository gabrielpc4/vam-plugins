using System;
using System.Collections.Generic;
using UnityEngine;

namespace VAMLaunchPlugin
{
    public class VAMLaunchSetupInstructions : MVRScript
    {
        public override void Init()
        {
            JSONStorableString _instructionText = new JSONStorableString("Additional Button Text", "To enable VAMLaunch for the Fleshlight Launch, follow the Setup Instructions in:\nVAM / Custom / Scripts / VAMLaunch.");
            // register tells engine you want value saved in json file during save and also make it available to
            // animation/trigger system
            RegisterString(_instructionText);
            CreateTextField(_instructionText);
        }
    }
}