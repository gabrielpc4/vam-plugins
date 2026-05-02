using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MSmile : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				smiledlast = true;
				if (enableSmile && suppressSmile == false)
				{
				currentMouth = "Smile";
				tongueExpressionOffset = 0.1f;
                //interestArousal += 0.1f;
                //interestValence += 0.2f;
                morphMouthAction = true;
				mExcitementTarget = 0.0f;
                //mSmileFullFaceTarget = 0.2f + (0.25f * (interestValence / 10.0f));
				//SuperController.LogMessage("Full Face set to " + Round(mSmileFullFaceTarget) + " for smile", false);
				if (interestValence > Mathf.Lerp(8.0f, 5.0f, interestArousal/10.0f))
				{
					if (interestArousal > 5.0f && Random.Range(0.0f, 100.0f) > 30.0f)
					{
						mSmileOpenFullFaceTarget = Mathf.Lerp(0.2f, 0.5f, interestValence/10.0f);
					}
					else
					{
						mSmileFullFaceTarget = Mathf.Lerp(0.3f, 0.7f, interestValence/10.0f);
					}
				}
				else
				{
					if (Random.Range(0.0f,100.0f) < Mathf.Lerp(80.0f, 20.0f, interestArousal/10.0f))
					{
						mSmileSimpleLeftTarget = Mathf.Lerp(0.2f, 0.8f, interestValence/10.0f);
						mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
					}
					else
					{
						if (Random.Range(0.0f,100.0f) < 50.0f)
						{
							mSmileSimpleLeftTarget = Random.Range(0.2f,0.5f)  + (0.4f * (interestValence / 10.0f));
							mSmileSimpleRightTarget = 0.0f;
						}
						else
						{
							mSmileSimpleLeftTarget = 0.0f;
							mSmileSimpleRightTarget = Random.Range(0.2f,0.5f) + (0.4f * (interestValence / 10.0f));
						}
					}
				}
				mTakingItTarget = 0.0f;
				mHappyTarget = Random.Range(0.0f,0.22f);
                mGlareTarget = 0.0f;
                mFlirtingTarget = 0.0f;
                mMouthOpenTarget = -0.2f;
				mMouthOpenWideTarget = 0.0f;
                mVisFTarget = 0.0f;
				mLipBiteTarget = 0.0f;
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = 0.0f;
                mTongueInOutTarget = 1.0f;
                mTongueSideSideTarget = 0.0f;
                mTongueBendTipTarget = 0.0f;
                mLipsPuckerTarget = 0.0f;
                mLipsPuckerWideTarget = 0.0f;
                Duration = Random.Range(1.5f, 3.5f) * lookVariation;
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphMouthAction = false;
                mSmileFullFaceTarget = 0.0f;
				//SuperController.LogMessage("Full Face set to 0 for smile end", false);
				mSmileSimpleRightTarget = 0.0f;
				mSmileSimpleLeftTarget = 0.0f;
				mSmileOpenFullFaceTarget = 0.0f;
                mMouthOpenTarget = Random.Range(0.0f,0.1f);
            }
        }
    }

}