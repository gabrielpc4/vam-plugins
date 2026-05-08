using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class LPlayful : State
        {
            public override void OnEnter()
            {
				if (interestClock <= 0.0f)
				{
					peronalityAdjustH = Random.Range(-35.0f, 35.0f) * Mathf.Deg2Rad;
					peronalityAdjustV = Random.Range(0.0f, Mathf.Lerp(-2.0f,-10.0f,interestArousal / 10.0f)) * Mathf.Deg2Rad;
					shoulderUp = Random.Range(0.1f,0.2f);
				}
				currentLook = "Playful";
				sexActionNeckX = 0.0f;
                //LogError("Playful");
                //gHeadSpeed = 1.0f;
                if (Random.Range(0.0f, 100.0f) < 25.0f * rollChance)
                {
                    gHeadRollTarget = Random.Range(-35.0f, 35.0f);
                }
                else
                {
                    //gHeadRollTarget = 0.0f;
                }
                saccadeAmount = Random.Range(1.0f, 10.0f);
                saccadeClock = 0.0f;
                lookAction = true;
                mBrowUpTarget = Random.Range(0.0f, 0.2f);
                lookVariation = Random.Range(0.25f, 0.95f);
                browVariation = Random.Range(0.45f, 0.95f);
                eyeVariation = Random.Range(0.65f, 0.95f);
                mouthVariation = Random.Range(0.45f, 0.95f);
				mTongueInOutTarget = 1.0f;
                browSM.SwitchRandom(new State[] {
                            bRaised,
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
								mBigSmile,
								mSmile,
								mSmirk,
								mBigSmile,
								mSmile,
								mBiteLip,
								mSmirk,
								mBigSmile,
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
                            eFocus,
                            eFocus,
                            eFocus,
                            eSquint
                        });
                Duration = Random.Range(1.5f, 4.0f);
            }
            public override void OnTimeout()
            {
                lookAction = false;
            }
        }
    }

}