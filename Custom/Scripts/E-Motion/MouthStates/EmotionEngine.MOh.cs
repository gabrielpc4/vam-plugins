using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MOh : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableOh)
				{
				currentMouth = "Oh!";
                morphMouthAction = true;
                mVisFTarget = 0.0f;
                mTongueSideSideTarget = 0.0f;
                mTongueBendTipTarget = 0.0f;
				mMouthOpenWideTarget = -1.0f;
				if (Random.Range(0.0f,100.0f) < 50.0f)
				{
					mMouthOpenWideTarget = Random.Range(0.2f, 0.4f) - mSmileOpenFullFaceTarget - mSmileFullFaceTarget;
				}
				mMouthOpenTarget = 0.0f;
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = 0.0f;
                mSmileSimpleLeftTarget = 0.0f;
                mSmileSimpleRightTarget = 0.0f;
				mLipBiteTarget = 0.0f;
                mLipsPuckerTarget = Random.Range(0.2f, 0.4f);
                mLipsPuckerWideTarget = 0.0f;
				if (Random.Range(0.0f,100.0f) < 50.0f)
				{
					mDeserveItTarget = Random.Range(0.5f, 0.6f);
				}
				else
				{
					mTakingItTarget = Random.Range(0.5f, 0.7f);
				}
                interestArousal += 5.0f;
                interestValence += 0.25f;
                Duration = Random.Range(1.0f, 2.0f) * mouthVariation;
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphMouthAction = false;
                mLipsPuckerTarget = 0.0f;
				mTakingItTarget = 0.0f;
				mDeserveItTarget = 0.0f;
				mMouthOpenWideTarget = 0.0f;
				mMouthOpenTarget = 0.0f;
            }
        }
    }

}