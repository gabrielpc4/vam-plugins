using System;
using UnityEngine;

namespace geesp0t
{
    /// <summary>
    /// Thin session plugin so VR strip logic compiles in a small assembly; compiling it inside
    /// <c>Auto_Load_Person_Plugins.cslist</c> triggered a Mono emit crash (TypeBuilder) on some loads.
    /// </summary>
    public class VrTriggerProximityStripClothingPlugin : MVRScript
    {
        public override void Init()
        {
        }

        public void Update()
        {
            try
            {
                VrTriggerProximityStripClothing.Tick();
            }
            catch (Exception e)
            {
                SuperController.LogError("VrTriggerProximityStripClothingPlugin: " + e);
            }
        }
    }
}
