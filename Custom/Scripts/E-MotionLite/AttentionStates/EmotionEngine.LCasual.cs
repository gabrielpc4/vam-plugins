using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class LCasual : State
        {

            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableCasual)
				{
				currentLook = "Casual";
				sexActionNeckX = 0.0f;
                //LogError("Casual");
				if (interestClock <= 0.0f)
				{
					shoulderUp = Random.Range(0.0f,0.2f);
				}
					peronalityAdjustH = Random.Range(-55.0f, 55.0f) * Mathf.Deg2Rad;
					peronalityAdjustV = Random.Range(-5.0f, 2.0f) * Mathf.Deg2Rad;              //gHeadSpeed = 0.5f;
                if (Random.Range(0.0f, 100.0f) < 35.0f * rollChance)
                {
					//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
					float headLeftRight = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
					float headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
                    gHeadRollTarget = Random.Range(-7.0f, 7.0f);
					if (headLeftRight > lookDirectAngle)
					{
						gHeadRollTarget = Random.Range(-7.0f, -0.0f);
					}
					if (headLeftRight < -lookDirectAngle)
					{
						gHeadRollTarget = Random.Range(7.0f, 0.0f);
					}
					if (headUpDown > lookPeripheralAngle)
					{
						gHeadRollTarget = Random.Range(-5.0f, 5.0f);
					}
                }
				else
				{
                    gHeadRollTarget = 0.0f;
				}
                saccadeAmount = Random.Range(5.0f, 10.0f);
                saccadeClock = 0.0f;
                lookAction = true;
                lookVariation = Random.Range(0.7f, 1.35f);
                browVariation = Random.Range(0.6f, 1.15f);
                eyeVariation = Random.Range(0.8f, 1.25f);
                mouthVariation = Random.Range(0.7f, 1.15f);
                if (Random.Range(0.0f, 100.0f) > 50.0f && morphBrowAction == false)
                {
                    browSM.SwitchRandom(new State[] {
                                bRaised,
                                bLowered,
                                bRaised,
                                bLowered,
                                bRaised,
                                bLowered,
                                bApprehensive,
                                bConcentrate,
                                bApprehensive,
                                bApprehensive
                            });
                }
                if (Random.Range(0.0f, 100.0f) > 50.0f && morphMouthAction == false)
                {
					if (interestValence > 9.0f)
					{
						mouthSM.SwitchRandom(new State[] {
									mOpen,
									mClosed,
									mOpen,
									mClosed,
									mOpen,
									mClosed,
									mSmile,
									mSmile,
									mSmile,
									mSmirk,
									mBigSmile,
									mBigSmile
								});
					}
					else
					{
						mouthSM.SwitchRandom(new State[] {
									mOpen,
									mOpen,
									mClosed,
									mClosed,
									mClosed,
									mSmile,
									mSmile,
									mSmile,
									mSmirk,
									mBigSmile,
									mBigSmile
									});
					}
                }
                eyesSM.SwitchRandom(new State[] {
                            eOpen,
                            eOpen,
                            eWide,
                            eFocus,
                            eFocus
                        });
                float rand = Random.Range(0.0f, 100.0f);
                if (rand > 33.0f)
                {
                    mLHandStraightenTarget = Random.Range(0.0f, 0.2f);
                    mLHandFistTarget = Random.Range(0.0f, 0.5f);
                }
                if (rand < 66.0f)
                {
                    mRHandStraightenTarget = Random.Range(0.0f, 0.2f);
                    mRHandFistTarget = Random.Range(0.0f, 0.5f);
                }
                Duration = Random.Range(2.0f, 5.0f);
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