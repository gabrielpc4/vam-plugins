using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EasyMotionLite
    {
        private class MBiteLip : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableBiteLip)
				{
				if ((interestArousal > 7.0f && (currentInterest == "Face" || currentInterest == "Pelvis")) || currentInterest == "Tip")
				{
					currentMouth = "Demure";
					//interestArousal += 0.25f;
					//interestValence += 0.1f;
					morphMouthAction = true;
					mSmileFullFaceTarget = -1.0f;
					mSmileOpenFullFaceTarget = -1.0f;
					mGlareTarget = 0.0f;
					mHappyTarget = 0.0f;//Random.Range(0.1f, 0.3f);
					//mTongueInOutTarget = Random.Range(-0.15f,0.5f);
					//mTongueSideSideTarget = 0.5f;
					mTongueBendTipTarget = 0.0f;
					///mVisFTarget = Random.Range(0.5f,1.0f);
					mMouthOpenTarget = -1.0f;
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
						if (mSmileOpenFullFaceValue < 0.05f && mSmileFullFaceTarget < 0.05f && mMouthOpenTarget < 0.05f && mMouthOpenValue < 0.05f)
						{
							mLipBiteTarget = 0.5f;
							mLipsPartTarget = 0.7f;
							mLipsCenterPartTarget = 0.42f;
							mLipsCloseTarget = 0.07f;
							mSmileFullFaceTarget = 0.0f;
							mSmileSimpleLeftTarget = -1.0f;
							mSmileSimpleRightTarget = -1.0f;
						}
					}
					mExcitementTarget = 0.0f;
				}
				else
				{
					mExcitementTarget = 0.5f;
				}
                Duration = Random.Range(4.0f, 12.5f) * lookVariation;
				}
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
				mMouthOpenTarget = Random.Range(0.0f,0.2f);
				mLipsPartTarget = 0.0f;
				mLipsCenterPartTarget = 0.0f;
				mLipsCloseTarget = 0.0f;
				mSmileFullFaceTarget = 0.0f;
				mExcitementTarget = 0.0f;
            }
        }
    }

}