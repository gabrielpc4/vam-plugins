using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class EWide : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableWide)
				{
                if (morphBlinking == false)
                {
					currentEye = "Wide";
                    //interestArousal += 0.5f;
                    //interestValence -= 0.1f;
                    morphEyeAction = true;
                    mEyesClosedLeftTarget = eyeOpenMaxMorph -0.10f;
                    mEyesClosedRightTarget = eyeOpenMaxMorph -0.10f;
                    mEyesSquintTarget = -0.52f;
                    Duration = Random.Range(0.5f, 1.0f) * lookVariation;
                }
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
                mEyesSquintTarget = 0.0f;
            }
        }
    }

}