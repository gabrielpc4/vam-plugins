using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class EBlink : State
        {
            public override void OnEnter()
            {
              Duration = 0.001f;
              if (enableBlink)
              {
                //SuperController.LogError("ACTUAL BLINK");
                Duration = 0.10f;
                morphEyeAction = true;
                morphBlinking = true;
                currentEye = "Blink";
                mEyesClosedLeftTarget = eyeCloseMaxMorph;
                mEyesClosedRightTarget = eyeCloseMaxMorph;
                blinkTimer = 0.0f;
                blinkRepeat += 1.0f;
                blinkRepTimer = 0.0f;
                eyeClock = 0.0f;
              }
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphEyeAction = false;
                mEyesClosedLeftTarget = eyeOpenMaxMorph;
                mEyesClosedRightTarget = eyeOpenMaxMorph;
				//eyeClock = 0.0f;
            }
        }
    }

}