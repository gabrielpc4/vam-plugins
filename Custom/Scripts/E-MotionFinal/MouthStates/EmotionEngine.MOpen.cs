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
				smiledlast = false;
				tempFloat = (interestArousal / 10.0f);
				if (currentMouth == "Open")
				{
					mMouthOpenTarget = Mathf.Lerp(0.0f, Mathf.Lerp(0.025f,0.05f,Mathf.Clamp(pantCount/10.0f, 0.0f, 10.0f)/10.0f), tempFloat);
				}
				else
				{
					mMouthOpenTarget = Mathf.Lerp(0.005f, Mathf.Lerp(0.025f,0.05f,Mathf.Clamp(pantCount/10.0f, 0.0f, 10.0f)/10.0f), tempFloat);
				}
				//SuperController.LogMessage("Setting Mouth Open to " + Round(mMouthOpenTarget), false);
				currentMouth = "Open";
				tongueExpressionOffset = -0.1f;
                //interestArousal += 0.1f;
                //interestValence += 0.05f;
                morphMouthAction = true;
                mSmileFullFaceTarget = 0.0f;
				//SuperController.LogMessage("Full Face set to 0 for mouth open", false);
                mSmileOpenFullFaceTarget = 0.0f;
				mLipBiteTarget = 0.0f;
				mTakingItTarget = 0.0f;
				
				
				mHappyTarget = 0.0f;
                mFlirtingTarget = 0.0f;
                mVisFTarget = 0.0f;
                mTongueInOutTarget = 1.0f;
                mTongueSideSideTarget = 0.0f;
                mTongueBendTipTarget = 0.0f;
				mExcitementTarget = 0.0f;
				
				
				mMouthOpenWideTarget = 0.0f;
                mMouthSideLeftTarget = 0.0f;
                mMouthSideRightTarget = 0.0f;
				mDeserveItTarget = 0.0f;
				mTakingItTarget = 0.0f;
				mLipBiteTarget = 0.0f;
				
                mSmileSimpleLeftTarget = 0.0f;//Mathf.Lerp(0.2f, 0.0f, tempFloat);;
                mSmileSimpleRightTarget = mSmileSimpleLeftTarget;
				
				mLipsCloseTarget = Mathf.Lerp(0.0f, 0.16f, tempFloat);
				mLipsPartTarget = Mathf.Clamp(Mathf.Lerp(-0.5f, 0.72f, tempFloat),0.0f,1.0f);
				mLipsCenterPartTarget = Mathf.Lerp(0.2f, 0.54f, tempFloat);
				
                mLipsPuckerTarget = 0.0f;//Mathf.Lerp(0.0f, 0.5f, tempFloat);
				//SuperController.LogError("E Pucker Set to " + mLipsPuckerTarget);
                //mLipsPuckerWideTarget = Mathf.Lerp(0.0f, 0.01f, tempFloat);
                Duration = Random.Range(5.0f, 15.0f) * lookVariation;
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
				//SuperController.LogMessage("Full Face set to 0 for mouth open end", false);
					mLipsPuckerTarget = 0.0f;
                mSmileOpenFullFaceTarget = 0.0f;
				if (Random.Range(0.0f, 100.0f) > 50.0f)
				{
					//mMouthOpenTarget = 0.0f;
					mSmileSimpleLeftTarget = 0.0f;
					mSmileSimpleRightTarget = 0.0f;
					//SuperController.LogError("F Pucker Set to " + mLipsPuckerTarget);
				}
            }
        }
    }

}