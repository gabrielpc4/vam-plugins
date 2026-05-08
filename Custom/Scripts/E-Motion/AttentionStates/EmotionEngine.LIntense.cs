using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class LIntense : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableIntense)
				{
				currentLook = "Intense";
				float randomRoll = Random.Range(0.0f, 100.0f);
                //LogError("Intense");
                //gHeadSpeed = 1.0f;
				sexActionNeckX = 0.0f;
				if (interestClock <= 0.0f)
				{
					shoulderUp = Random.Range(0.0f,0.3f);
					peronalityAdjustH = Random.Range(-45.0f, 45.0f) * (1.0f-((interestValence-2.0f)/10.0f)) * Mathf.Deg2Rad;
					peronalityAdjustV = Mathf.Lerp(Random.Range(-4.0f, -0.0f), Random.Range(-20.0f, -5.0f), Mathf.Min(playerHeadToHead, 1.0f)) * (interestArousal/10.0f) * Mathf.Deg2Rad;
				}
                saccadeAmount = Random.Range(1.0f, 3.0f);
                if (randomRoll < 10.0f * rollChance)
                {
					//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
					float headLeftRight = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
					float headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
                    gHeadRollTarget = Random.Range(-10.0f, 10.0f);
					if (headLeftRight > lookDirectAngle)
					{
						gHeadRollTarget = Random.Range(-10.0f, 0.0f);
					}
					if (headLeftRight < -lookDirectAngle)
					{
						gHeadRollTarget = Random.Range(10.0f, 0.0f);
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

                saccadeClock = 0.0f;
                lookAction = true;
                lookVariation = Random.Range(0.25f, 0.5f);
                browVariation = Random.Range(0.50f, 0.95f);
                eyeVariation = Random.Range(0.65f, 1.0f);
                mouthVariation = Random.Range(0.7f, 1.15f);
                if (Random.Range(0.0f, 100.0f) > 10.0f && morphBrowAction == false)
                {
					if (Random.Range(0.0f,100.0f) < 25.0f)
					{
						if (amGlancing)
						{
							browSM.SwitchRandom(new State[] {
										bLowered,
										bApprehensive,
										bConcentrate,
										bRaised,
										bRaised,
										bRaised
									});
						}
						else
						{
							if (lastBrowState == bLowered)
							{
									browSM.SwitchRandom(new State[] {
												bLowered,
												bApprehensive,
												bApprehensive,
												bConcentrate,
												bLowered,
												bRaised
											});
							}
							else
							{
								if (lastBrowState == bRaised)
								{
									browSM.SwitchRandom(new State[] {
												bLowered,
												bApprehensive,
												bConcentrate
											});
								}
								else
								{
									browSM.SwitchRandom(new State[] {
												bLowered,
												bApprehensive,
												bApprehensive,
												bRaised,
												bRaised,
												bRaised
											});
								}
							}
						}
					}
                }
				
				if (playerHeadToHead > personalSpaceDistance / 2.0f)
				{
					if (randomRoll > 5.0f && morphMouthAction == false)
					{
						if (lastMouthState == mClosed)
						{
							mouthSM.SwitchRandom(new State[] {
										mClosed,
										mOpen,
										mOpen,
										mOpen,
										mOpen,
										mSmirk,
										mBiteLip,
										mBiteLip,
										mBiteLip
									});
						}
						else
						{
							if (lastMouthState == mOpen && Random.Range(0.0f,100.0f) < 50.0f)
							{
								if (lastMouthState == mOpen)
								{
									mouthSM.SwitchRandom(new State[] {
												mClosed,
												mClosed,
												mClosed,
												mOpen,
												mOpen,
												mSmirk,
												mSmirk,
												mSmile,
												mSmile,
												mSmile,
												mSmile,
												mSmile,
												mBiteLip
											});
								}
								else
								{
									if (lastMouthState == mBiteLip || lastMouthState == mSmirk || lastMouthState == mSideways)
									{
										mouthSM.SwitchRandom(new State[] {
													mClosed,
													mOpen
												});
									}
									else
									{
										mouthSM.SwitchRandom(new State[] {
													mClosed,
													mClosed,
													mClosed,
													mOpen,
													mSmirk,
													mSmile,
													mSmile,
													mBiteLip
												});
									}
								}
							}
						}
					}
					eyesSM.SwitchRandom(new State[] {
								eOpen,
								eSquint,
								eSquint,
								eSquint,
								eFocus,
								eFocus,
								eFocus,
								eFocus,
								eFocus
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
					eyesSM.SwitchRandom(new State[] {
								eOpen,
								eOpen,
								eOpen,
								eOpen,
								eOpen,
								eFocus
							});
				}
                float rand = Random.Range(0.0f, 100.0f);
                if (rand > 33.0f)
                {
                    mLHandFistTarget = Random.Range(0.0f, 0.4f);
                }
                if (rand < 66.0f)
                {
                    mRHandFistTarget = Random.Range(0.0f, 0.4f);
                }
                Duration = Random.Range(2.0f, 4.0f);
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