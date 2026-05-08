using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MOpen : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableMouthOpen)
				{
				currentMouth = "Open";
                //interestArousal += 0.1f;
                //interestValence += 0.05f;
                morphMouthAction = true;
                mSmileFullFaceTarget = -1.0f;
                mSmileOpenFullFaceTarget = 0.0f;
				mHappyTarget = 0.0f;
                mFlirtingTarget = 0.0f;
                mVisFTarget = 0.0f;
                mTongueInOutTarget = 1.0f;
                mTongueSideSideTarget = 0.0f;
                mTongueBendTipTarget = 0.0f;
				
				tempFloat = (interestArousal / 10.0f);
				
				mMouthOpenWideTarget = 0.0f;
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = 0.0f;
				mDeserveItTarget = 0.0f;
				mTakingItTarget = 0.0f;
				mLipBiteTarget = 0.0f;
                mSmileSimpleLeftTarget = Mathf.Lerp(0.2f, 0.0f, tempFloat);;
                mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
				mLipsCloseTarget = Mathf.Lerp(0.0f, 0.16f, tempFloat);
				mLipsPartTarget = Mathf.Clamp(Mathf.Lerp(-0.5f, 0.72f, tempFloat),0.0f,1.0f);
				mLipsCenterPartTarget = Mathf.Lerp(0.2f, 0.54f, tempFloat);
                mMouthOpenTarget = Mathf.Lerp(0.2f, 0.12f, tempFloat);
                mLipsPuckerTarget = Mathf.Lerp(0.0f, 0.27f, tempFloat);
                mLipsPuckerWideTarget = Mathf.Lerp(0.0f, 0.01f, tempFloat);
                Duration = Random.Range(4.0f, 9.0f) * lookVariation;
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphMouthAction = false;
				if (Random.Range(0.0f, 100.0f) > 50.0f)
				{
					mMouthOpenTarget = -1.0f;
					mSmileSimpleLeftTarget = 0.0f;
					mSmileSimpleRightTarget = 0.0f;
					mLipsPuckerTarget = 0.0f;
				}
            }
        }
    }

}