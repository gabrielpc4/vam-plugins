using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EasyMotionLite
    {
        private class LPlayful : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enablePlayful)
				{
				if (interestClock <= 0.0f)
				{
					peronalityAdjustH = Random.Range(-35.0f, 35.0f) * Mathf.Deg2Rad;
					peronalityAdjustV = Random.Range(0.0f, Mathf.Lerp(Mathf.Lerp(-0.0f, -3.0f, Mathf.Min(playerHeadToHead, 1.0f)),Mathf.Lerp(-2.0f, -10.0f, Mathf.Min(playerHeadToHead, 1.0f)),interestArousal / 10.0f)) * Mathf.Deg2Rad;
					shoulderUp = Random.Range(0.1f,0.2f);
				}
				currentLook = "Playful";
				sexActionNeckX = 0.0f;
                //LogError("Playful");
                //gHeadSpeed = 1.0f;
                if (Random.Range(0.0f, 100.0f) < 25.0f * rollChance)
                {
					//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
                    gHeadRollTarget = Random.Range(-35.0f, 35.0f);
					if (headLeftRight > lookDirectAngle)
					{
						gHeadRollTarget = Random.Range(-25.0f, -2.0f);
					}
					if (headLeftRight < -lookDirectAngle)
					{
						gHeadRollTarget = Random.Range(25.0f, 2.0f);
					}
					if (headUpDown > lookPeripheralAngle)
					{
						gHeadRollTarget = Random.Range(-5.0f, 5.0f);
					}
                }
                else
                {
                    //gHeadRollTarget = 0.0f;
                }
                saccadeAmount = Random.Range(1.0f, 10.0f);
                saccadeClock = 0.0f;
                lookAction = true;
                mBrowUpTarget = Random.Range(0.0f, 0.2f);
                lookVariation = Random.Range(0.95f, 1.15f);
                browVariation = Random.Range(0.75f, 1.15f);
                eyeVariation = Random.Range(0.65f, 1.15f);
                mouthVariation = Random.Range(0.85f, 1.15f);
				mTongueInOutTarget = 1.0f;
                browSM.SwitchRandom(new State[] {
                            bRaised,
                            bRaised,
                            bApprehensive,
                            bApprehensive,
                            bApprehensive,
                            bApprehensive,
                            bApprehensive,
                            bConcentrate
                        });
				if (playerHeadToHead > personalSpaceDistance / 2.0f)
				{
					mouthSM.SwitchRandom(new State[] {
								mOpen,
								mOpen,
								mOpen,
								mOpen,
								mOpen,
								mSmirk,
								mSmirk,
								mSmile,
								mSmirk,
								mOpen,
								mSmile,
								mBiteLip,
								mBiteLip,
								mBiteLip,
								mSmile//,
								//mSideways
								//mKiss
							});
				}
				else
				{
					mouthSM.SwitchRandom(new State[] {
								mOpen,
								mOpen,
								mOpen,
								mOpen,
								mOpen,
								mOpen,
								mOpen,
								mClosed,
								mClosed,
								mSmirk
							});
				}
                eyesSM.SwitchRandom(new State[] {
                            eOpen,
                            eOpen,
                            eOpen,
                            eOpen,
                            eOpen,
                            eFocus,
                            eFocus,
                            eFocus,
                            eSquint
                        });
                Duration = Random.Range(1.5f, 4.0f);
				}
            }
            public override void OnTimeout()
            {
                lookAction = false;
            }
        }
    }

}