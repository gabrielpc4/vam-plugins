using Random = UnityEngine.Random;
using UnityEngine;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MBigSmile : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableBigSmile && suppressSmile == false && interestValence > 4.0f)
				{
				smiledlast = true;
				currentMouth = "Big Smile";
				tongueExpressionOffset = 0.1f;
                //interestValence += 0.3f;
                morphMouthAction = true;
                mTakingItTarget = 0.0f;
				mTongueInOutTarget = 1.0f;
				mLipBiteTarget = 0.0f;
				if (interestValence < 4.0f || Random.Range(0.0f, 100.0f) > 90.0f)
				{
					mSmileSimpleLeftTarget = 0.3f + Random.Range(0.0f,0.25f);
					mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
					mSmileOpenFullFaceTarget = 0.0f;
					mSmileFullFaceTarget = 0.0f;
					mLipsCloseTarget = 0.1f;
				}
				else
				{
					if (interestValence > 5.5f || Random.Range(0.0f, 100.0f) > 30.0f)
					{
						mSmileOpenFullFaceTarget = 0.3f + (0.3f * (interestValence / 10.0f)) + Random.Range(0.0f,0.15f);
						mSmileSimpleLeftTarget = 0.1f + Mathf.Lerp(0.0f, 0.2f, interestArousal/10.0f);
						mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
						mSmileFullFaceTarget = 0.0f;
						//SuperController.LogMessage("Full Face set to 0 Big Smile using open", false);
						mLipsCenterPartTarget = 0.4f;
						if (Random.Range(0.0f,100.0f) < 10.0f)
						{
							//mTongueInOutTarget = 0.5f;
						}
					}
					else
					{
						mSmileFullFaceTarget = 0.5f + (0.4f * (interestValence / 10.0f));
						//SuperController.LogMessage("Full Face set to " + Round(mSmileFullFaceTarget), false);
						mSmileOpenFullFaceTarget = 0.0f;
						mSmileSimpleLeftTarget = 0.0f;
						mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
					}
				}
                mGlareTarget = 0.0f;
                mHappyTarget = 0.0f;
				mBrowDownTarget = 0.0f;
				mBrowUpTarget = Mathf.Lerp(0.0f, 0.2f, interestArousal/10.0f);
				mFlirtingTarget = 0.0f;
                //mMouthOpenTarget = -0.2f * (interestValence / 10.0f);
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = 0.0f;
                mVisFTarget = 0.0f;
				mLipBiteTarget = 0.0f;
                mTongueSideSideTarget = 0.0f;
                mTongueBendTipTarget = 0.0f;
                mLipsPuckerTarget = 0.0f;
                mLipsPuckerWideTarget = 0.0f;
                Duration = Random.Range(3.5f, 5.5f) * lookVariation;
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
                mSmileOpenFullFaceTarget = 0.0f;
                mMouthOpenTarget = 0.0f;
				//mSmileOpenFullFaceTarget = 0.0f;//mSmileOpenFullFaceTarget / 2.0f;
				//mSmileSimpleLeftTarget = 0.0f;
				//mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
				mLipBiteTarget = 0.0f;
				//mSmileFullFaceTarget = 0.0f;
				//SuperController.LogMessage("Full Face set to 0 Big Smile end", false);
            }
        }
    }

}