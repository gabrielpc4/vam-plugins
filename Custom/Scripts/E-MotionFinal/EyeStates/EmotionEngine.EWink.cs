using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class EWink : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableWink && (interestArousal > 7.0f || interestValence > 9.0f))
				{
				currentEye = "Wink";
                //interestArousal += 0.5f;
                //interestValence += 0.1f;
                morphEyeAction = true;
                if (Random.Range(0.0f, 100.0f) > 50.0f)
                {
                    mEyesClosedLeftTarget = eyeCloseMaxMorph;
                    mEyesClosedRightTarget = eyeOpenMaxMorph;
                    mSmileSimpleLeftTarget = 0.4f;
                    mSmileSimpleRightTarget = 0.0f;
                }
                else
                {
                    mEyesClosedLeftTarget = eyeOpenMaxMorph;
                    mEyesClosedRightTarget = eyeCloseMaxMorph;
                    mSmileSimpleLeftTarget = 0.0f;
                    mSmileSimpleRightTarget = 0.4f;
                }
                mEyesSquintTarget = 0.0f;
                Duration = Random.Range(0.25f, 0.3f);
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphEyeAction = false;
                //eyeClock = 0.0f;
                mEyesClosedLeftTarget = eyeOpenMaxMorph;
                mEyesClosedRightTarget = eyeOpenMaxMorph;
            }
        }
    }

}