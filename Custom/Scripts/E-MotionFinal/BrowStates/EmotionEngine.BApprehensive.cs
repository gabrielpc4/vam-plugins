using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class BApprehensive : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableApprehensive && (interestArousal > 5.0f && interestValence < 6.0f))
				{
					currentBrow = "Apprehensive";
					//interestArousal += 0.5f;
					//interestValence -= 0.2f;
					morphBrowAction = true;
					mExcitementTarget = 0.0f;
					mBrowDownTarget = 0.0f;
					mBrowUpTarget = 0.0f;
					mBrowCenterUpTarget = Mathf.Clamp(Random.Range(0.25f,1.0f) + (interestArousal / 10.0f), 0.6f, 1.3f);
					//mLipsPuckerTarget = 0.0f;//Random.Range(0.1f, 0.3f);
					//mLipsPuckerWideTarget = 0.0f;
					mBrowOuterUpLeftTarget = 0.0f;
					mBrowOuterUpRightTarget = 0.0f;
					Duration = Random.Range(2.65f, 5.35f);
					if (currentMouth == "Smile" || currentMouth == "Big Smile" || currentMouth == "Sideways" && currentMouth == "Pout")
					{
						mBrowCenterUpTarget = 0.0f;
						Duration = 0.1f;
					}
				}
				else
				{
					Duration = 0.05f;
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphBrowAction = false;
                //mBrowDownTarget = 0.0f;
                mBrowCenterUpTarget = 0.0f;
            }
        }
    }

}