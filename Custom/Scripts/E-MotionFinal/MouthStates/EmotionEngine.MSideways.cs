using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class MSideways : State
        {
            public override void OnEnter()
            {
            Duration = 0.001f;
            if (enableSideways && (interestArousal > 5.0f || interestValence > 7.0f))
            {
              smiledlast = false;
              currentMouth = "Pout";
              tongueExpressionOffset = 0.0f;
              //interestArousal += 0.1f;
              interestValence += 0.5f;
              morphMouthAction = true;
              mSmileFullFaceTarget = 0.0f;
			  //SuperController.LogMessage("Full Face set to 0 for sideways", false);
              mSmileOpenFullFaceTarget = 0.0f;
              mExcitementTarget = 0.0f;
              mGlareTarget = 0.0f;
              mHappyTarget = 0.0f;
              mTongueInOutTarget = 1.0f;
              mTongueSideSideTarget = 0.0f;
              mTongueBendTipTarget = 0.0f;
              mVisFTarget = 0.0f;
              mHappyTarget = 0.0f;
              mFlirtingTarget = 0.0f;
              mMouthOpenTarget = -0.12f;
              mLipBiteTarget = 0.0f;
              mMouthSideLeftTarget = 0.0f;
              mMouthSideRightTarget = 0.0f;
              mSmileSimpleLeftTarget = 0.0f;
              mSmileSimpleRightTarget = 0.0f;
              if (mainInterest != "Face")
              {
                if (Random.Range(0.0f,100.0f) > 50.0f)
                {
                  mMouthSideLeftTarget = Random.Range(0.4f, 0.8f);
                  mMouthSideRightTarget = 0.0f;
                }
                else
                {
                  mMouthSideLeftTarget = 0.0f;
                  mMouthSideRightTarget = Random.Range(0.4f, 0.8f);
                }
				Duration = Random.Range(1.25f, 3.25f) * (1.0f + lookVariation);
              }
              else
              {
                mFlirtingTarget = 0.35f + Mathf.Lerp(0.0f, 0.2f, interestArousal/10.0f);
				Duration = Random.Range(2.25f, 4.25f) * (1.0f + lookVariation);
              }
              if (mainInterest == "RandomR" || mainInterest == "RHand")
              {
                mMouthSideLeftTarget = Random.Range(0.6f, 0.8f);
                mMouthSideRightTarget = 0.0f;
				mFlirtingTarget = 0.0f;
				Duration = Random.Range(2.25f, 3.25f) * (1.0f + lookVariation);
              }
              if (mainInterest == "RandomL" || mainInterest == "LHand")
              {
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = Random.Range(0.6f, 0.8f);
				mFlirtingTarget = 0.0f;
				Duration = Random.Range(2.25f, 3.25f) * (1.0f + lookVariation);
              }
              mLipsPuckerTarget = 0.0f;
              mLipsPuckerWideTarget = 0.0f;
              }
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
              morphMouthAction = false;
              mMouthSideLeftTarget = 0.0f;
              mMouthSideRightTarget = 0.0f;
              mFlirtingTarget = 0.0f;
              mTakingItTarget = 0.0f;
              //mMouthOpenWideTarget = Random.Range(0.0f,0.2f);
            }
        }
    }

}