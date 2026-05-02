using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class BOneRaise : State
        {
            public override void OnEnter()
            {
				currentBrow = "One Raised";
                //interestArousal += 0.3f;
                //interestValence += 0.1f;
                morphBrowAction = true;
                mExcitementTarget = 0.0f;
                mBrowDownTarget = 0.0f;
                mBrowUpTarget = 0.0f;
                mBrowCenterUpTarget = 0.0f;
				Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
                if (cross.y < 0.0f)//(pStableness > 50.0f)
                {
                    mBrowOuterUpLeftTarget = Mathf.Min(0.5f + (interestValence / 10.0f), 1.0f);
                    mBrowOuterUpRightTarget = 0.0f;
                }
                else
                {
                    mBrowOuterUpLeftTarget = 0.0f;
                    mBrowOuterUpRightTarget = Mathf.Min(0.5f + (interestValence / 10.0f), 1.0f);
                }
                Duration = Random.Range(0.85f, 4.75f) * lookVariation;
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                morphBrowAction = false;
                //mBrowOuterUpLeftTarget = 0.0f;
                //mBrowOuterUpRightTarget = 0.0f;
				currentBrow = "Neutral";
            }
        }
    }

}