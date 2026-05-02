using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
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
					if ((Random.Range(0.0f, 100.0f) > Mathf.Lerp(90.0f, 10.0f, interestArousal/10.0f) * (2.0f-variationChance) || Mathf.Abs(peronalityAdjustH) < 10.0f) && adjustTimeout <= 0.0f)
					{
						adjustTimeout = adjustWaitTime;
						shoulderUp = Random.Range(0.0f,0.2f);
						//peronalityAdjustH = (Random.Range(-25.0f, 25.0f)) * gazeVariation;
						tempFloat = Random.Range(0.0f, 25.0f) * gazeVariation;
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
								peronalityAdjustV = (Random.Range(0.0f, Mathf.Lerp(-2.0f, -15.0f, Mathf.Min(playerHeadToHead, 1.0f)) * (pExtraversion / 100.0f))) * gazeVariation;                //gHeadSpeed = 4.0f;
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
					if (Random.Range(0.0f, 100.0f) < 45.0f * rollChance)
					{
						//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
						float headLeftRight = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
						float headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
						gHeadRollTarget = Random.Range(-maxHeadRoll / 2.0f, maxHeadRoll / 2.0f);
						if (headLeftRight > lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(-maxHeadRoll / 2.0f, -maxHeadRoll / 5.0f);
						}
						if (headLeftRight < -lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(maxHeadRoll / 2.0f, maxHeadRoll / 5.0f);
						}
						if (headUpDown > lookPeripheralAngle)
						{
							gHeadRollTarget = Random.Range(-maxHeadRoll / 2.0f, maxHeadRoll / 2.0f);
						}
					}
					else
					{
						gHeadRollTarget = 0.0f;
					}
					saccadeAmount = Random.Range(9.0f, 10.0f);
					saccadeClock = 0.0f;
					lookAction = true;
					lookVariation = Random.Range(0.85f, 1.5f);
					browVariation = Random.Range(0.55f, 1.3f);
					eyeVariation = Random.Range(0.85f, 1.0f);
					mouthVariation = Random.Range(0.75f, 1.1f);

					if (Random.Range(0.0f, 100.0f) > Mathf.Lerp(10.0f, 30.0f, interestValence/10.0f) && morphBrowAction == false)
					{
						if (amGlancing || gAvoid == 1.0f)
						{
							browSM.Switch(bLowered);
							if (Random.Range(0.0f, 100.0f) <= 30.0f)
							{
							  browSM.Switch(bConcentrate);
							}
							if (Random.Range(0.0f, 100.0f) <= 20.0f)
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
							if (lastBrowState == bLowered || lastBrowState == bApprehensive)
							{
								browSM.Switch(bRaised);
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 25.0f, interestValence/10.0f))
								{
								  browSM.Switch(bConcentrate);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(65.0f, 35.0f, interestValence/10.0f))
								{
								  browSM.Switch(bLowered);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(15.0f, 45.0f, interestArousal/10.0f))
								{
								  browSM.Switch(bApprehensive);
								}
							}
							else
							{
								if (lastBrowState == bRaised)
								{
								  browSM.Switch(bLowered);
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 25.0f, interestValence/10.0f))
								  {
									browSM.Switch(bRaised);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(50.0f, 5.0f, interestArousal/10.0f))
								  {
									browSM.Switch(bConcentrate);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 25.0f, interestArousal/10.0f))
								  {
									browSM.Switch(bApprehensive);
								  }
								}
								else
								{
								  browSM.Switch(bLowered);
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 45.0f, interestValence/10.0f))
								  {
									browSM.Switch(bRaised);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 65.0f, interestArousal/10.0f))
								  {
									browSM.Switch(bOneRaise);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 25.0f, interestValence/10.0f))
								  {
									browSM.Switch(bConcentrate);
								  }
								}
							}
						}
					}
							
							
					if (playerHeadToHead > personalSpaceDistance / Mathf.Lerp(2.0f, 5.0f, pExtraversion/100.0f))
					{
						if (Random.Range(0.0f, 100.0f) > Mathf.Lerp(05.0f, 65.0f, pExtraversion/100.0f) && morphMouthAction == false)
						{
							if (lastMouthState == mClosed)
							{
								if (pExtraversion > Mathf.Lerp(75.0f, 55.0f, pAgreeableness/100.0f))
								{
									if (smiledlast)
									{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mOpen);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mOpen);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 15.0f, interestArousal/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSmirk);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 30.0f, interestValence/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSideways);
										}

									}
									else
									{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mSmile);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mOpen);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 20.0f, interestArousal/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSmirk);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 10.0f, interestValence/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSideways);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 65.0f, interestValence/10.0f))
										{
										  mouthSM.Switch(mSmile);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 50.0f, interestValence/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mBigSmile);
										}
									}
								}
								else
								{
									if (smiledlast)
									{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mOpen);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mOpen);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 15.0f, interestArousal/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSmirk);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 20.0f, interestValence/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSideways);
										}
									}
									else
									{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mSmile);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mOpen);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 6.0f, interestArousal/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSmirk);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSideways);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 57.0f, interestValence/10.0f))
										{
										  mouthSM.Switch(mSmile);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 35.0f, interestValence/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mBigSmile);
										}
									}
								}
							}
							else
							{
								if (lastMouthState == mOpen)
								{
									if (smiledlast)
									{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mSmile);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 30.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mOpen);
										}
										if (Random.Range(0.0f, 100.0f) <= 35.0f)
										{
										  mouthSM.Switch(mClosed);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 15.0f, interestArousal/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSmirk);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 20.0f, interestValence/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSideways);
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
										if (Random.Range(0.0f, 100.0f) <= 15.0f)
										{
										  mouthSM.Switch(mClosed);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mOpen);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 6.0f, interestArousal/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSmirk);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mSideways);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 23.0f, interestValence/10.0f))
										{
										  mouthSM.Switch(mSmile);
										}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 15.0f, interestValence/10.0f))
										{
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										  mouthSM.Switch(mBigSmile);
										}
									}
								}
								else
								{
									if (lastMouthState == mBiteLip || lastMouthState == mSideways)
									{
										mouthSM.Switch(mClosed);
									}
									else
									{
										if (smiledlast)
										{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mOpen);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mOpen);
										}
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 5.0f, interestArousal/10.0f))
											  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
												mouthSM.Switch(mSmirk);
											  }
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 10.0f, interestValence/10.0f))
											  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
												mouthSM.Switch(mSideways);
											  }
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 10.0f, interestValence/10.0f))
											  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
												mouthSM.Switch(mBiteLip);
											  }
										}
										else
										{
								  if (interestValence > Mathf.Lerp(8.0f, 4.0f, interestArousal/10.0f))
									{
										mouthSM.Switch(mSmile);
									}
								  else
									{
										mouthSM.Switch(mClosed);
									}
										if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestArousal/10.0f))
										{
										  mouthSM.Switch(mOpen);
										}
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 6.0f, interestArousal/10.0f))
											  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
												mouthSM.Switch(mSmirk);
											  }
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
											  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
												mouthSM.Switch(mSideways);
											  }
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 10.0f, interestValence/10.0f))
											  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
												mouthSM.Switch(mBiteLip);
											  }
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 13.0f, interestValence/10.0f))
											  {
												mouthSM.Switch(mSmile);
											  }
											  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 10.0f, interestValence/10.0f))
											  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
												mouthSM.Switch(mBigSmile);
											  }
										}
									}
								}
							}
						}
						eyesSM.Switch(eOpen);
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 30.0f, interestValence/10.0f))
						{
						  eyesSM.Switch(eFocus);
						}
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 20.0f, interestArousal/10.0f))
						{
						  eyesSM.Switch(eSquint);
						}
					}
					else
					{
						mouthSM.Switch(mClosed);
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 13.0f, interestValence/10.0f))
						{
						  mouthSM.Switch(mSmile);
						}
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 10.0f, interestValence/10.0f))
						{
						  mouthSM.Switch(mBigSmile);
						}
						if (Random.Range(0.0f, 100.0f) <= 25.0f)
						{
						  mouthSM.Switch(mOpen);
						}
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 6.0f, interestArousal/10.0f))
						{
						  mouthSM.Switch(mSmirk);
						}
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
						{
						  mouthSM.Switch(mSideways);
						}
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 10.0f, interestValence/10.0f))
						{
						  mouthSM.Switch(mBiteLip);
						}
						eyesSM.Switch(eOpen);
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 10.0f, interestValence/10.0f))
						{
						  eyesSM.Switch(eFocus);
						}
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 20.0f, interestArousal/10.0f))
						{
						  eyesSM.Switch(eSquint);
						}
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
					Duration = Random.Range(3.0f, 5.0f) * uiExpressionLengthVal;
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