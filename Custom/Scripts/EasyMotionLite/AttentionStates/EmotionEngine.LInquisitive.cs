using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EasyMotionLite
    {
        private class LInquisitive : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableInquisitive)
				{
				currentLook = "Inquisitive";
				sexActionNeckX = 0.0f;
                //LogError("Inquisitive");
				if (interestClock <= 0.0f)
				{
					shoulderUp = Random.Range(0.0f,0.2f);
					peronalityAdjustH = Random.Range(-25.0f, 25.0f) * Mathf.Deg2Rad;
					peronalityAdjustV = Random.Range(0.0f, Mathf.Lerp(-2.0f, -15.0f, Mathf.Min(playerHeadToHead, 1.0f)) * (pExtraversion / 100.0f)) * Mathf.Deg2Rad;                //gHeadSpeed = 4.0f;
				}
                if (Random.Range(0.0f, 100.0f) < 45.0f * rollChance)
                {
					//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
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
				else
				{
                    gHeadRollTarget = 0.0f;
				}
                saccadeAmount = Random.Range(5.0f, 10.0f);
                saccadeClock = 0.0f;
                lookAction = true;
                lookVariation = Random.Range(0.85f, 1.2f);
                browVariation = Random.Range(0.85f, 1.3f);
                eyeVariation = Random.Range(0.65f, 1.1f);
                mouthVariation = Random.Range(0.45f, 1.0f);

                if (Random.Range(0.0f, 100.0f) > 10.0f && morphBrowAction == false)
                {
                    if (amGlancing || gAvoid == 1.0f)
                    {
                        browSM.SwitchRandom(new State[] {
                                    bLowered,
                                    bRaised,
                                    bApprehensive,
                                    bApprehensive,
                                    bConcentrate
                                });
                    }
                    else
                    {
                        if (lastBrowState == bLowered || lastBrowState == bApprehensive)
                        {
                            browSM.SwitchRandom(new State[] {
                                        bRaised,
                                        bRaised,
                                        bRaised,
                                        bConcentrate,
                                        bOneRaise
                                    });
                        }
                        else
                        {
                            if (lastBrowState == bRaised)
                            {
                                browSM.SwitchRandom(new State[] {
                                            bRaised,
                                            bRaised,
                                            bLowered,
                                            bApprehensive
                                        });
                            }
                            else
                            {
                                browSM.SwitchRandom(new State[] {
                                            bRaised,
                                            bRaised,
                                            bApprehensive,
                                            bConcentrate,
                                            bLowered,
                                            bLowered,
                                            bOneRaise,
                                            bApprehensive
                                        });
                            }
                        }
                    }
                }
						
						
				if (playerHeadToHead > personalSpaceDistance / 2.0f)
				{
					if (Random.Range(0.0f, 100.0f) > 25.0f && morphMouthAction == false)
					{
						if (lastMouthState == mClosed)
						{
							mouthSM.SwitchRandom(new State[] {
										mOpen,
										mOpen,
										mOpen,
										mOpen,
										mClosed,
										mClosed,
										mClosed,
										mBigSmile,
										mBigSmile,
										mOpen,
										mSideways
									});
						}
						else
						{
							if (lastMouthState == mOpen)
							{
								mouthSM.SwitchRandom(new State[] {
											mOpen,
										mOpen,
										mOpen,
											mClosed,
											mClosed,
											mBiteLip,
											mBigSmile,
											mBigSmile,
											mBigSmile,
											mSmile,
											mSmile,
											mSmile,
											mSmile,
											mSmile,
											mSmile,
											mSideways,
											mSideways,
											mSmirk
										});
							}
							else
							{
								if (lastMouthState == mBiteLip || lastMouthState == mSideways)
								{
									mouthSM.Switch(mClosed);
								}
								else
								{
									mouthSM.SwitchRandom(new State[] {
												mOpen,
												mClosed,
												mClosed,
												mClosed,
												mClosed,
												mBiteLip,
												mSmile,
												mSmile,
												mBigSmile,
												mBigSmile,
												mOpen,
												mOpen,
												mOpen,
												mOpen,
												mSmile,
												mSmile,
												mBigSmile,
												mBigSmile,
												mSmile,
												mSmile,
												mBigSmile,
												mBigSmile,
												mSideways,
												mSideways,
												mOpen,
												mSideways,
											});
								}
							}
						}
					}

					eyesSM.SwitchRandom(new State[] {
								eOpen,
								eOpen,
								eOpen,
								eFocus,
								eFocus,
								eFocus,
								eFocus,
								eFocus,
								eSquint
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
                    mLHandStraightenTarget = Random.Range(0.0f, 0.3f);
                    mLHandFistTarget = Random.Range(0.0f, 0.5f);
                }
                if (rand < 66.0f)
                {
                    mRHandStraightenTarget = Random.Range(0.0f, 0.3f);
                    mRHandFistTarget = Random.Range(0.0f, 0.5f);
                }
                Duration = Random.Range(3.0f, 5.0f);
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