using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MSmirk : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableSmirk && (interestArousal > 7.0f || interestValence > 9.0f))
				{
				smiledlast = false;
					currentMouth = "LipBite";
					tongueExpressionOffset = 0.0f;
					//interestArousal += 0.5f;
					//interestValence += 0.05f;
					morphMouthAction = true;
					mSmileFullFaceTarget = 0.0f;//Mathf.Lerp(Random.Range(0.2f,0.6f),0.0f,interestArousal/10.0f);
					//SuperController.LogMessage("Full Face set to 0 for smirk", false);
					mSmileOpenFullFaceTarget = 0.0f;
					mExcitementTarget = 0.0f;
					mHappyTarget = 0.0f;
					mGlareTarget = 0.0f;
					mTongueInOutTarget = 1.0f;
					mTongueSideSideTarget = 0.0f;
					mTongueBendTipTarget = 0.0f;
					mLipBiteTarget = 0.0f;
					mVisFTarget = 0.0f;
					mTakingItTarget = 0.0f;
					mFlirtingTarget = 0.0f;
					//mMouthOpenTarget = -0.1f + (0.4f * ((10.0f-interestArousal)/10.0f));
					//mFlirtingTarget = Mathf.Lerp(0.7f,0.0f,interestArousal/10.0f);
					mMouthSideLeftTarget = 0.0f;
					mMouthSideRightTarget = 0.0f;
					mMouthOpenWideTarget = 0.0f;
					mSmileSimpleLeftTarget = 0.0f;
					mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
					/*if (pStableness > 50.0f)
					{
						mSmileSimpleLeftTarget = Random.Range(0.5f, 1.0f);
						mSmileSimpleRightTarget = 0.2f;
					}
					else
					{
						mSmileSimpleLeftTarget = 0.2f;
						mSmileSimpleRightTarget = Random.Range(0.5f, 1.0f);
					}*/
					if (person2IsMale)
					{
						mHappyTarget = Random.Range(0.1f, 0.3f);;
					}
					else
					{
						if (Random.Range(0.0f,100.0f) < 20.0f && (interestArousal > 6.0f || interestValence > 5.0f))
						{
							//mLipsPuckerTarget = 0.6f * (1.0f+(interestArousal/10.0f));
							//mLipsPuckerWideTarget = 0.2f * (1.0f+(interestArousal/10.0f));
							Duration = Random.Range(1.25f, 2.75f) * (1.0f + lookVariation);
							//mMouthOpenTarget = 0.2f;
							if (mSmileOpenFullFaceValue < 0.3f && mSmileFullFaceTarget < 0.2f && mMouthOpenTarget < 0.15f && mMouthOpenValue < 0.15f && interestValence > 5.0f && interestArousal > 3.0f)
							{
								mLipBiteTarget = 0.65f;
								mLipsPartTarget = 0.1f;
								mLipsCenterPartTarget = 0.42f;
								mLipsCloseTarget = 0.07f;
								mSmileFullFaceTarget = 0.0f;
								//SuperController.LogMessage("Full Face set to 0 for smirk lip bite", false);
								mSmileOpenFullFaceTarget = -1.0f;
								mSmileSimpleLeftTarget = -1.0f;
								mSmileSimpleRightTarget = -1.0f;
								mHappyTarget = 0.0f;
							}
							else
							{
								mExcitementTarget = Mathf.Lerp(0.35f, 0.65f, interestArousal/10.0f);
							}
						}
						else
						{
							mTakingItTarget = Random.Range(0.25f, 0.6f);
							Duration = Random.Range(1.5f, Mathf.Lerp(2.0f, 3.75f, interestArousal/10.0f)) * (1.0f + lookVariation);
							mLipsPuckerWideTarget = 0.0f;
							mMouthOpenTarget = 0.0f;
						}
					}
					//Duration = Random.Range(0.5f, 1.0f) * lookVariation;
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphMouthAction = false;
                mSmileSimpleLeftTarget = 0.0f;
                mSmileSimpleRightTarget = 0.0f;
                mFlirtingTarget = 0.0f;
				mLipsPuckerTarget = 0.0f;
				mLipsPuckerWideTarget = 0.0f;
				mMouthOpenTarget = 0.0f;
				mTakingItTarget = 0.0f;
				mMouthOpenWideTarget = 0.0f;
				mLipBiteTarget = 0.0f;
				mSmileFullFaceTarget = 0.0f;
				mExcitementTarget = 0.0f;
				//SuperController.LogMessage("Full Face set to 0 for smirk end", false);
            }
        }
    }

}