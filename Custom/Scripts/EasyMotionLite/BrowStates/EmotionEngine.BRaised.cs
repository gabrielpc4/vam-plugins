using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EasyMotionLite
    {
        private class BRaised : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableRaised)
				{
				if (mainInterest != mainOld)
				{
					//interestArousal += 0.1f;
					//interestValence -= 0.15f;
					morphBrowAction = true;
					mExcitementTarget = 0.0f;
					if (mBrowDownTarget > 0.0f)
					{
						currentBrow = "Relaxed";
						mBrowDownTarget = 0.0f;
						mBrowUpTarget = 0.0f;
					}
					else
					{
						currentBrow = "Raised";
						mBrowDownTarget = 0.0f;
						mBrowUpTarget = Mathf.Min(0.5f + (interestValence / 10.0f), 1.0f);
					}
					mBrowCenterUpTarget = 0.0f;
					mBrowOuterUpLeftTarget = 0.0f;
					mBrowOuterUpRightTarget = 0.0f;
					Duration = Random.Range(3.55f, 7.35f) * lookVariation;
				}
				else
				{
					Duration = 0.05f;
				}
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphBrowAction = false;
                mBrowUpTarget = 0.0f;
            }
        }
    }

}