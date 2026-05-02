using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MBiteLip : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableBiteLip && interestArousal > 5.0f)
				{
				//SuperController.LogError("START " + currentInterest, false);
				smiledlast = false;
				if ((interestArousal > 5.0f && (currentInterest == "Face" || currentInterest == "Pelvis")) || currentInterest == "Tip" && Random.Range(0.0f,100.0f) <= 50.0f)
				{
					currentMouth = "Demure";
					tongueExpressionOffset = 0.0f;
					//interestArousal += 0.25f;
					//interestValence += 0.1f;
					morphMouthAction = true;
					mSmileFullFaceTarget = 0.0f;
					//SuperController.LogMessage("Full Face set to 0 for bite lip", false);
					mSmileOpenFullFaceTarget = -1.0f;
					mGlareTarget = 0.0f;
					mHappyTarget = 0.0f;//Random.Range(0.1f, 0.3f);
					//mTongueInOutTarget = Random.Range(-0.15f,0.5f);
					//mTongueSideSideTarget = 0.5f;
					mTongueBendTipTarget = 0.0f;
					mExcitementTarget = 0.0f;
					///mVisFTarget = Random.Range(0.5f,1.0f);
					mMouthOpenTarget = -1.0f;
					mTakingItTarget = 0.0f;
					mFlirtingTarget = 0.0f;
					mMouthNarrowTarget = -1.0f;
					mMouthOpenWideTarget = -1.0f;
					mMouthSideLeftTarget = 0.0f;
					mMouthSideRightTarget = 0.0f;
					//mSmileSimpleLeftTarget = -1.0f;
					//mSmileSimpleRightTarget = -1.0f;
					mLipsPuckerTarget = 0.0f;//Random.Range(0.1f, 0.3f);
					mLipsPuckerWideTarget = 0.0f;
					if (person2IsMale)
					{
						mSmileFullFaceTarget = 0.5f;
					}
					else
					{
						if (mSmileOpenFullFaceValue < 0.1f && mSmileFullFaceTarget < 0.1f && mMouthOpenTarget < 0.1f && mMouthOpenValue < 0.05f)
						{
							//SuperController.LogError("doing demure", false);
							mLipBiteTarget = 0.85f;
							mLipsPartTarget = 0.1f;
							mLipsCenterPartTarget = 0.42f;
							mLipsCloseTarget = 0.07f;
							mSmileFullFaceTarget = 0.0f;
							//SuperController.LogMessage("Full Face set to 0 for biting of lip", false);
							mSmileOpenFullFaceTarget = -1.0f;
							mSmileSimpleLeftTarget = -1.0f;
							mSmileSimpleRightTarget = -1.0f;
							mHappyTarget = 0.0f;
						}
					}
					mExcitementTarget = 0.0f;
				}
				else
				{
					mExcitementTarget = Mathf.Lerp(0.35f, 0.65f, interestArousal/10.0f);
				}
                Duration = Random.Range(3.5f, 4.5f) * lookVariation;
				}
				//SuperController.LogError("END", false);
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphMouthAction = false;
                mHappyTarget = 0.0f;
                mMouthOpenTarget = 0.0f;
                mLipsPuckerTarget = 0.0f;
				mLipBiteTarget = 0.0f;
				mMouthOpenTarget = 0.0f;
				mLipsPartTarget = 0.0f;
				mLipsCenterPartTarget = 0.0f;
				mLipsCloseTarget = 0.0f;
				mSmileFullFaceTarget = 0.0f;
				//SuperController.LogMessage("Full Face set to 0 for lip bite end", false);
				mExcitementTarget = 0.0f;
            }
        }
    }

}