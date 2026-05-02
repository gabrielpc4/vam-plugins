using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EasyMotionLite
    {
        private class MClosed : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableMouthClosed)
				{
				currentMouth = "Closed";
                morphMouthAction = true;
                mTongueInOutTarget = 1.0f;
                mTongueSideSideTarget = 0.0f;
				mHappyTarget = 0.0f;
                mMouthOpenTarget = -0.20f;
                mVisFTarget = 0.0f;
                mLipsPuckerWideTarget = 0.0f;
                mLipsPuckerTarget = 0.0f;
				mLipBiteTarget = 0.0f;
                mSmileSimpleLeftTarget = 0.0f;
                mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
				mSmileFullFaceTarget = 0.0f;Random.Range(0.0f, 0.3f);
                Duration = Random.Range(1.5f, 3.0f) * lookVariation;
				if (Random.Range(0.0f,100.0f) < 60.0f && mainInterest == "Face")
				{
					mTakingItTarget = Random.Range(0.1f, 0.2f);
				}
				mDeserveItTarget = 0.0f;
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphMouthAction = false;
				if (interestArousal > 5.0f)
				{
					mMouthOpenTarget = Random.Range(0.0f,0.2f);
				}
				mTakingItTarget = 0.0f;
            }
        }
    }

}