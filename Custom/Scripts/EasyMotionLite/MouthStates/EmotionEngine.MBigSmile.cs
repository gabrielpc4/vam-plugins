using Random = UnityEngine.Random;
using UnityEngine;

namespace VRAdultFun
{
    partial class EasyMotionLite
    {
        private class MBigSmile : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableBigSmile)
				{
				currentMouth = "Big Smile";
                //interestValence += 0.3f;
                morphMouthAction = true;
				mTongueInOutTarget = 1.0f;
				if (interestValence < 5.0f)
				{
					mSmileSimpleLeftTarget = 0.4f + Random.Range(0.0f,0.25f);
					mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
					mSmileOpenFullFaceTarget = 0.0f;
					mSmileFullFaceTarget = 0.0f;
					mLipsCloseTarget = 0.1f;
				}
				else
				{
					if (interestValence > 7.5f)
					{
						mSmileOpenFullFaceTarget = 0.3f + (0.3f * (interestValence / 10.0f)) + Random.Range(0.0f,0.15f);
						mSmileSimpleLeftTarget = 0.1f + Random.Range(0.0f,0.20f);
						mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
						mSmileFullFaceTarget = 0.0f;
						mLipsCenterPartTarget = 0.4f;
						if (Random.Range(0.0f,100.0f) < 10.0f)
						{
							//mTongueInOutTarget = 0.5f;
						}
					}
					else
					{
						mSmileFullFaceTarget = 0.2f + (0.4f * (interestValence / 10.0f)) + Random.Range(0.0f,0.15f);
						mSmileOpenFullFaceTarget = 0.0f;
						mSmileSimpleLeftTarget = 0.0f;
						mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
					}
				}
                mGlareTarget = 0.0f;
                mHappyTarget = 0.2f;
				mBrowDownTarget = 0.3f;
				mBrowUpTarget = 0.0f;
				//mFlirtingTarget = 0.4f * (interestValence / 10.0f);
                mMouthOpenTarget = -0.2f * (interestValence / 10.0f);
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = 0.0f;
                mVisFTarget = 0.0f;
				mLipBiteTarget = 0.0f;
                mTongueSideSideTarget = 0.0f;
                mTongueBendTipTarget = 0.0f;
                mLipsPuckerTarget = 0.0f;
                mLipsPuckerWideTarget = -0.05f;
                Duration = Random.Range(4.5f, 6.5f) * lookVariation;
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
                //mSmileOpenFullFaceTarget = 0.0f;
                mMouthOpenTarget = Random.Range(0.0f,0.2f);
				//mSmileOpenFullFaceTarget = mSmileOpenFullFaceTarget / 2.0f;
				//mSmileSimpleLeftTarget = 0.0f;
				//mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
            }
        }
    }

}