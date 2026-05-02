using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class LBored : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableBored)
				{
				currentLook = "Bored";
                //LogError("Bored");
				shoulderUp = Random.Range(0.0f,0.1f);
				if (adjustTimeout <= 0.0f)
				{
					adjustTimeout = adjustWaitTime;
					tempFloat = Random.Range(0.0f, 65.0f) * gazeVariation;
					peronalityAdjustV = 0.0f;
					if (targetH * Mathf.Rad2Deg < -lookDirectAngle)
					{
						peronalityAdjustH = -tempFloat;
					}
					else
					{
						if (targetH * Mathf.Rad2Deg > lookDirectAngle)
						{
							peronalityAdjustH = tempFloat;
						}
						else
						{
							peronalityAdjustH = Random.Range(-tempFloat, tempFloat);
							peronalityAdjustV = (Random.Range(-2.0f, 10.0f)) * gazeVariation;
						}
					}
				}
				sexActionNeckX = 0.0f;
				gHeadSpeed = 20.0f;
                if (Random.Range(0.0f, 100.0f) < 10.0f * rollChance)
                {
					float headLeftRight = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
					float headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
                    gHeadRollTarget = Random.Range(-25.0f, 25.0f);
					if (headLeftRight > lookDirectAngle)
					{
						gHeadRollTarget = Random.Range(-15.0f, -5.0f);
					}
					if (headLeftRight < -lookDirectAngle)
					{
						gHeadRollTarget = Random.Range(15.0f, 5.0f);
					}
					if (headUpDown > lookPeripheralAngle)
					{
						gHeadRollTarget = Random.Range(-5.0f, 5.0f);
					}
                }
                saccadeAmount = Random.Range(10.0f, 20.0f);
                mBrowDownTarget = Random.Range(0.0f, 0.2f);
                saccadeClock = 0.0f;
                lookAction = true;
                lookVariation = Random.Range(0.5f, 1.25f);
                browVariation = Random.Range(0.5f, 1.25f);
                eyeVariation = Random.Range(0.5f, 1.25f);
                mouthVariation = Random.Range(0.35f, 0.45f);
                if (Random.Range(0.0f, 100.0f) > 33.0f && morphBrowAction == false)
                {
                  browSM.Switch(bLowered);
                  if (Random.Range(0.0f, 100.0f) <= 25.0f)
                  {
                    browSM.Switch(bRaised);
                  }
                  if (Random.Range(0.0f, 100.0f) <= 50.0f)
                  {
                    browSM.Switch(bConcentrate);
                  }
                  if (Random.Range(0.0f, 100.0f) <= 50.0f)
                  {
                    browSM.Switch(bApprehensive);
                  }
                }
                mouthSM.Switch(mClosed);
                if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestValence/10.0f))
                {
                  mouthSM.Switch(mSmile);
                }
                if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 15.0f, interestValence/10.0f))
                {
                  mouthSM.Switch(mBigSmile);
                }
                if (Random.Range(0.0f, 100.0f) <= 5.0f)
                {
                  mouthSM.Switch(mOpen);
                }
                if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 3.0f, interestArousal/10.0f))
                {
                  mouthSM.Switch(mSmirk);
                }
                if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 2.0f, interestValence/10.0f))
                {
                  mouthSM.Switch(mSideways);
                }
                if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 1.0f, interestValence/10.0f))
                {
                  mouthSM.Switch(mBiteLip);
                }
                eyesSM.Switch(eOpen);
                if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 40.0f, interestValence/10.0f))
                {
                  eyesSM.Switch(eFocus);
                }
                if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 10.0f, interestArousal/10.0f))
                {
                  eyesSM.Switch(eSquint);
                }
                float rand = Random.Range(0.0f, 100.0f);
                if (rand > 33.0f)
                {
                    mLHandStraightenTarget = Random.Range(0.0f, 0.4f);
                    mLHandFistTarget = Random.Range(0.0f, 0.4f);
                }
                if (rand < 66.0f)
                {
                    mRHandStraightenTarget = Random.Range(0.0f, 0.4f);
                    mRHandFistTarget = Random.Range(0.0f, 0.4f);
                }
                Duration = Random.Range(1.0f, 3.0f) * uiExpressionLengthVal;
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                lookAction = false;
            }
        }
    }

}