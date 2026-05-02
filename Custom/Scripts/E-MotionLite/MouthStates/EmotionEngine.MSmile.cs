using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MSmile : State
        {
            public override void OnEnter()
            {
				currentMouth = "Smile";
                //interestArousal += 0.1f;
                //interestValence += 0.2f;
                morphMouthAction = true;
                mSmileFullFaceTarget = 0.45f * (interestValence / 10.0f);
				if (mSmileOpenFullFaceTarget < 0.2f || interestValence < 5.0f)
				{
					mSmileOpenFullFaceTarget = 0.2f;
				}
				else
				{
					mSmileOpenFullFaceTarget = 0.0f;
				}
				mHappyTarget = Random.Range(0.0f,0.22f);
                mGlareTarget = 0.0f;
                mFlirtingTarget = 0.1f * (interestValence / 10.0f);
                mMouthOpenTarget = -0.2f;
                mVisFTarget = 0.0f;
				mLipBiteTarget = 0.0f;
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = 0.0f;
                mSmileSimpleLeftTarget = 0.0f;
                mSmileSimpleRightTarget = 0.6f * (interestValence / 10.0f);
                mTongueInOutTarget = 1.0f;
                mTongueSideSideTarget = 0.0f;
                mTongueBendTipTarget = 0.0f;
                mLipsPuckerTarget = 0.0f;
                mLipsPuckerWideTarget = 0.0f;
                Duration = Random.Range(4.5f, 9.5f) * lookVariation;
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphMouthAction = false;
                //mSmileFullFaceTarget = 0.0f;
                mMouthOpenTarget = Random.Range(0.0f,0.2f);
            }
        }
    }

}