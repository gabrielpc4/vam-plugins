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
					if (randomRoll > (50.0f * (2.0f-variationChance)) && adjustTimeout <= 0.0f)
					{
						adjustTimeout = adjustWaitTime;
						shoulderUp = Random.Range(0.0f,0.3f);
						//peronalityAdjustH = (Random.Range(-15.0f, 15.0f) * (1.0f-((interestValence-2.0f)/10.0f))) * gazeVariation;
						tempFloat = Random.Range(0.0f, 15.0f) * (1.0f-((interestValence-2.0f)/10.0f)) * gazeVariation;
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
								peronalityAdjustV = (Mathf.Lerp(Random.Range(-4.0f, -0.0f), Random.Range(-20.0f, -5.0f), Mathf.Min(playerHeadToHead, 1.0f)) * (interestArousal/10.0f)) * gazeVariation;
							}
						}						
					}
					else
					{
						if (adjustTimeout <= 0.0f)
						{
						peronalityAdjustH = 0.0f;
						peronalityAdjustV = 0.0f;
						}
					}
					saccadeAmount = Random.Range(8.0f, 8.0f);
					if (randomRoll < 10.0f * rollChance)
					{
						//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
						float headLeftRight = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
						float headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
						gHeadRollTarget = Random.Range(-maxHeadRoll / 3.0f, maxHeadRoll / 3.0f);
						if (headLeftRight > lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(-maxHeadRoll / 3.0f, 0.0f);
						}
						if (headLeftRight < -lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(maxHeadRoll / 3.0f, 0.0f);
						}
						if (headUpDown > lookPeripheralAngle)
						{
							gHeadRollTarget = Random.Range(-maxHeadRoll / 5.0f, maxHeadRoll / 5.0f);
						}
					}
					else
					{
						gHeadRollTarget = 0.0f;
					}

					saccadeClock = 0.0f;
					lookAction = true;
					lookVariation = Random.Range(0.25f, 0.75f);
					browVariation = Random.Range(0.50f, 1.25f);
					eyeVariation = Random.Range(0.85f, 1.0f);
					mouthVariation = Random.Range(0.7f, 1.15f);
					if (Random.Range(0.0f, 100.0f) > Mathf.Lerp(10.0f, 30.0f, interestValence/10.0f) && morphBrowAction == false)
					{
						if (Random.Range(0.0f,100.0f) < Mathf.Lerp(25.0f, 85.0f, interestArousal/10.0f))
						{
							if (amGlancing)
							{
								browSM.Switch(bLowered);
								if (Random.Range(0.0f, 100.0f) <= 30.0f)
								{
								  browSM.Switch(bConcentrate);
								}
								if (Random.Range(0.0f, 100.0f) <= 10.0f)
								{
								  browSM.Switch(bRaised);
								}
								if (Random.Range(0.0f, 100.0f) <= 10.0f)
								{
								  browSM.Switch(bApprehensive);
								}
							}
							else
							{
								if (lastBrowState == bLowered)
								{
								  browSM.Switch(bLowered);
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(15.0f, 45.0f, interestArousal/10.0f))
								  {
									browSM.Switch(bRaised);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 25.0f, interestArousal/10.0f))
								  {
									browSM.Switch(bConcentrate);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(15.0f, 35.0f, interestArousal/10.0f))
								  {
									browSM.Switch(bApprehensive);
								  }
								}
								else
								{
									if (lastBrowState == bRaised)
									{
										browSM.Switch(bLowered);
										if (Random.Range(0.0f, 100.0f) <= 20.0f)
										{
										  browSM.Switch(bRaised);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(3.0f, 25.0f, interestArousal/10.0f))
										{
										  browSM.Switch(bApprehensive);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(35.0f, 5.0f, interestValence/10.0f))
										{
										  browSM.Switch(bConcentrate);
										}
									}
									else
									{
										browSM.Switch(bRaised);
										if (Random.Range(0.0f, 100.0f) <= 40.0f)
										{
										  browSM.Switch(bLowered);
										}
										if (Random.Range(0.0f, 100.0f) <= 10.0f)
										{
										  browSM.Switch(bApprehensive);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 3.0f, interestValence/10.0f))
										{
										  browSM.Switch(bConcentrate);
										}
									}
								}
							}
						}
					}
					
					if (playerHeadToHead > personalSpaceDistance / Mathf.Lerp(2.0f, 5.0f, interestValence/10.0f))
					{
						if (randomRoll > 5.0f && morphMouthAction == false)
						{
							if (lastMouthState == mClosed)
							{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mSmile);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
								if (Random.Range(0.0f, 100.0f) <= 60.0f)
								{
								  mouthSM.Switch(mOpen);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 15.0f, interestArousal/10.0f))
								{
								  mouthSM.Switch(mSmirk);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 20.0f, interestArousal/10.0f))
								{
								  mouthSM.Switch(mBiteLip);
								}
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 15.0f, interestValence/10.0f))
								  {
									mouthSM.Switch(mSmile);
								  }
							}
							else
							{
								if (lastMouthState == mOpen && Random.Range(0.0f,100.0f) < 70.0f)
								{
									if (lastMouthState == mOpen)
									{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mSmile);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 10.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mSmirk);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mBiteLip);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 45.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mOpen);
										}
									}
									else
									{
										if (lastMouthState == mBiteLip || lastMouthState == mSmirk || lastMouthState == mSideways)
										{
											  mouthSM.Switch(mClosed);
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(15.0f, 65.0f, interestArousal/10.0f))
											  {
												mouthSM.Switch(mOpen);
											  }
										}
										else
										{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mOpen);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 25.0f, interestValence/10.0f))
											  {
												mouthSM.Switch(mSmile);
											  }
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 10.0f, interestArousal/10.0f))
											  {
												mouthSM.Switch(mSmirk);
											  }
											  if (Random.Range(0.0f, 100.0f) <= 37.0f)
											  {
												mouthSM.Switch(mOpen);
											  }
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
											  {
												mouthSM.Switch(mBiteLip);
											  }
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 15.0f, interestValence/10.0f))
											  {
												mouthSM.Switch(mBigSmile);
											  }
										}
									}
								}
							}
						}
						eyesSM.Switch(eOpen);
						if (Random.Range(0.0f, 100.0f) <= 40.0f)
						{
						  eyesSM.Switch(eFocus);
						}
						if (Random.Range(0.0f, 100.0f) <= 25.0f)
						{
						  eyesSM.Switch(eSquint);
						}
						if (Random.Range(0.0f, 100.0f) <= 5.0f)
						{
						  eyesSM.Switch(eWide);
						}
					}
					else
					{
						mouthSM.Switch(mOpen);
						if (Random.Range(0.0f, 100.0f) <= 30.0f)
						{
						  mouthSM.Switch(mClosed);
						}
						if (Random.Range(0.0f, 100.0f) <= 15.0f)
						{
						  mouthSM.Switch(mSmile);
						}
						if (Random.Range(0.0f, 100.0f) <= 5.0f)
						{
						  mouthSM.Switch(mSmirk);
						}
						if (Random.Range(0.0f, 100.0f) <= 2.0f)
						{
						  mouthSM.Switch(mBiteLip);
						}
						if (Random.Range(0.0f, 100.0f) <= 2.0f)
						{
						  mouthSM.Switch(mBigSmile);
						}
						eyesSM.Switch(eOpen);
						if (Random.Range(0.0f, 100.0f) <= 40.0f)
						{
						  eyesSM.Switch(eFocus);
						}
						if (Random.Range(0.0f, 100.0f) <= 5.0f)
						{
						  eyesSM.Switch(eSquint);
						}
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
					Duration = Random.Range(2.0f, 4.0f) * uiExpressionLengthVal;
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