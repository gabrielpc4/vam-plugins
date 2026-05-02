using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MSuck : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableSuck)
				{
				smiledlast = false;
				currentMouth = "Sucking";
                //interestArousal += 2.0f;
                //interestValence -= 0.1f;
				tongueExpressionOffset = 0.0f;
                morphMouthAction = true;
                mSmileFullFaceTarget = 0.0f;
                mSmileOpenFullFaceTarget = 0.0f;
				if (playerTipToHead < interactionDistance / 2.0f)
				{
					sexActionNeckX = Random.Range(-10.0f, 30.0f);
				}
                mGlareTarget = 0.0f;
                mHappyTarget = 0.0f;
                mFlirtingTarget = 0.0f;
				gHeadRollTarget = Random.Range(30.0f, 60.0f);
				if (Random.Range(0.0f, 100.0f) < 50.0f)
				{
					gHeadRollTarget = Random.Range(-60.0f, -60.0f);
				}
                mTongueBendTipTarget = Random.Range(-0.2f, 0.3f);
                mTongueInOutTarget = 1.0f;//Random.Range(-1.8f, 0.4f);
				if (lipsTouchCount > 0.0f && Random.Range(0.0f,100.0f) > 60.0f)
				{
					mTongueSideSideTarget = Random.Range(-0.7f, 0.7f);
				}
				if (Random.Range(0.0f, 100.0f) < 50.0f)
				{
					mTongueInOutTarget = Random.Range(-1.4f, 0.4f);
				}
                mMouthOpenTarget = Random.Range(0.5f, 0.8f);
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = 0.0f;
				mLipBiteTarget = 0.0f;
                mSmileSimpleLeftTarget = 0.0f;
                mSmileSimpleRightTarget = 0.0f;
                mLipsPuckerTarget = Random.Range(0.0f, 0.4f);
                mLipsPuckerWideTarget = Random.Range(0.0f, 0.3f);
                Duration = Random.Range(0.4f, 1.2f);// * lookVariation;
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphMouthAction = false;
				mTongueInOutTarget = 1.0f;
                mLipsPuckerTarget = 0.0f;
				mLipsPuckerWideTarget = 0.0f;
				mTongueSideSideTarget = 0.0f;
				sexActionNeckX = 0.0f;
            }
        }
    }

}