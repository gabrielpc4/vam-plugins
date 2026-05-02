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
				if (currentMouth != "Sideways" && currentMouth != "Pout" && currentMouth != "Big Smile")
				{
					currentBrow = "Apprehensive";
					//interestArousal += 0.5f;
					//interestValence -= 0.2f;
					morphBrowAction = true;
					mExcitementTarget = 0.0f;
					mBrowDownTarget = 0.0f;
					mBrowUpTarget = 0.0f;
					mBrowCenterUpTarget = Mathf.Clamp(Random.Range(0.25f,1.0f) + (interestArousal / 10.0f), 0.6f, 1.3f);
					mSmileFullFaceTarget = 0.0f;
					mSmileOpenFullFaceTarget = 0.0f;
					mBrowOuterUpLeftTarget = 0.0f;
					mBrowOuterUpRightTarget = 0.0f;
					Duration = Random.Range(2.65f, 5.35f);
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