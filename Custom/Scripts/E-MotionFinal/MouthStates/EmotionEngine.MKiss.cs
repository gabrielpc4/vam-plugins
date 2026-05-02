using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MKiss : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableKiss)
				{
				smiledlast = false;
				currentMouth = "Kissing";
				tongueExpressionOffset = 0.0f;
                //interestArousal += 5.0f;
                //interestValence += 1.0f;
				interestKissing = true;
                morphMouthAction = true;
                mSmileFullFaceTarget = 0.0f;
                mSmileOpenFullFaceTarget = 0.0f;
				mLipBiteTarget = 0.0f;
                mGlareTarget = 0.0f;
				//sexActionNeckX = -5.0f;
				float tempDist = Mathf.Clamp(kissingDistance - playerHeadToHead,0.0f,1.0f);
				if (kissingDistance > 0.22f)
				{
					tempDist = (tempDist + 0.23f) / kissingDistance;
				}
				else
				{
					tempDist = 1.0f;
				}
				if (lipsOnly == false)
				{
					if (mTongueInOutTarget < 0.0f)
					{
						mTongueInOutTarget = Mathf.Lerp(1.0f, Random.Range(0.0f, 0.5f), tempDist) * kissingAmount;
						mTongueSideSideTarget = Random.Range(-0.15f, 0.15f) * kissingAmount;
						mTongueTongueTwistTarget = Random.Range(-0.3f, 0.3f) * kissingAmount;
						if (mTongueInOutTarget < -0.1f)
						{
							mTongueBendTipTarget = Random.Range(-0.2f, 0.2f) * kissingAmount;
						}
						else
						{
							mTongueBendTipTarget = 0.0f;
						}
					}
					else
					{
						mTongueInOutTarget = Mathf.Lerp(1.0f, Random.Range(-0.9f, -0.0f), tempDist) * kissingAmount;
						mTongueSideSideTarget = Random.Range(-0.15f, 0.15f) * kissingAmount;
						mTongueTongueTwistTarget = Random.Range(-0.3f, 0.3f) * kissingAmount;
						mTongueBendTipTarget = 0.0f;
					}
                Duration = Random.Range(0.2f, 0.4f) * lookVariation;
				}
				else
				{
					if (mTongueInOutTarget < 0.0f)
					{
						mTongueInOutTarget = Mathf.Lerp(1.0f, Random.Range(0.5f, 0.75f), tempDist) * kissingAmount;
						mTongueSideSideTarget = Random.Range(-0.15f, 0.15f) * kissingAmount;
						mTongueTongueTwistTarget = Random.Range(-0.3f, 0.3f) * kissingAmount;
						if (mTongueInOutTarget < -0.1f)
						{
							mTongueBendTipTarget = Random.Range(-0.2f, 0.2f) * kissingAmount;
						}
						else
						{
							mTongueBendTipTarget = 0.0f;
						}
					}
					else
					{
						mTongueInOutTarget = Mathf.Lerp(1.0f, Random.Range(-0.4f, -0.0f), tempDist) * kissingAmount;
						mTongueSideSideTarget = Random.Range(-0.15f, 0.15f) * kissingAmount;
						mTongueTongueTwistTarget = Random.Range(-0.3f, 0.3f) * kissingAmount;
						mTongueBendTipTarget = 0.0f;
					}
                
				}
				mLipsCloseTarget = Random.Range(-0.3f, 0.0f) * kissingAmount;
                mHappyTarget = 0.0f;
                mFlirtingTarget = 0.0f;
				if (mMouthOpenValue >= 0.4f && Random.Range(0.0f,100.0f) > 20.0f && lipsOnly == false)
				{
					//mMouthOpenTarget = Random.Range(0.0f, 0.3f);
					//mMouthOpenWideTarget = Random.Range(0.0f, 0.0f);
					mLipsPuckerTarget = Random.Range(0.0f, 0.5f) * kissingAmount;
					//SuperController.LogError("A Pucker Set to " + mLipsPuckerTarget);
					mLipsPuckerWideTarget = Random.Range(0.0f, 0.1f) * kissingAmount;
				}
				else
				{
					//mMouthOpenTarget = Random.Range(0.7f, 1.1f) * kissingAmount;
					//mMouthOpenWideTarget = Random.Range(0.0f, 0.3f) * kissingAmount;
					mLipsPuckerTarget = Random.Range(0.1f, 1.1f) * kissingAmount;
					//SuperController.LogError("B Pucker Set to " + mLipsPuckerTarget);
					mLipsPuckerWideTarget = Random.Range(0.1f, 0.4f) * kissingAmount;
					if (mLipsPuckerValue > 0.8f)
					{
						mLipsPuckerTarget = Random.Range(0.0f, 0.4f) * kissingAmount;
						//SuperController.LogError("C Pucker Set to " + mLipsPuckerTarget);
						mLipsPuckerWideTarget = Random.Range(0.1f, 0.2f) * kissingAmount;
					}
					if (mLipsPuckerValue < 0.2f)
					{
						mLipsPuckerTarget = Random.Range(0.2f, 0.8f) * kissingAmount;
						//SuperController.LogError("D Pucker Set to " + mLipsPuckerTarget);
					}
				}
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = 0.0f;
                mSmileSimpleLeftTarget = 0.0f;
                mSmileSimpleRightTarget = 0.0f;
				Duration = Random.Range(0.2f, 0.4f);// * lookVariation;
				}
				else
				{
                Duration = 0.01f;
				}
            }
			public override void OnUpdate()
			{
				//SuperController.LogError("Kiss Duration " + Duration);
			}
            public override void OnTimeout()
            {
				//SuperController.LogError("Setting Kiss 6");
				currentMouth = "KissingEnd";
                morphMouthAction = false;
				interestKissing = false;
                mTongueInOutTarget = 1.0f;
                mTongueSideSideTarget = 0.0f;
                mMouthOpenTarget = 0.0f;
				mMouthOpenWideTarget = 0.0f;
                mLipsPuckerTarget = 0.0f;
				mLipsPuckerWideTarget = 0.0f;
				sexActionNeckX = 0.0f;
            }
            public override void OnInterrupt(string parameter)
            {
				//SuperController.LogError("Setting Kiss 7");
                OnTimeout();
            }
        }
    }

}