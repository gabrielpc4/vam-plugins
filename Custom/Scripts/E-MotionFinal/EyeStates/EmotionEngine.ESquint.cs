using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class ESquint : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableSquint && (interestArousal < 4.0f || interestValence > 5.0f))
				{
                if (morphBlinking == false)
                {
					currentEye = "Squint";
                    //interestArousal += 0.2f;
                    //interestValence += 0.05f;
                    morphEyeAction = true;
                    //mEyesClosedLeftTarget = 0.0f;
                    //mEyesClosedRightTarget = 0.0f;
                    mEyesSquintTarget = Random.Range(0.1f, 0.3f);
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
				//mEyesClosedLeftTarget = 0.0f;
				//mEyesClosedRightTarget = 0.0f;
                mEyesSquintTarget = 0.0f;
            }
        }
    }

}