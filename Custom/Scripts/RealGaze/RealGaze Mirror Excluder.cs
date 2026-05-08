using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
using System.Timers;

namespace PA4148415 {
    public class RealGazeMirror : MVRScript {
        //"Script" by pornaway4148415
        //Adding this "script" to a reflective surface forces lookers to ignore it as a mirror.

        public override void Init() {
            SuperController.LogMessage("Excluded mirror \"" + containingAtom.uid + "\".");
            pluginLabelJSON.val = "Excluded mirror";
        }
    }
}