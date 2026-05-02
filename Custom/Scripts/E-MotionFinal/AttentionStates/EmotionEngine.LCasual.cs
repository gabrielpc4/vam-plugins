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
					if (adjustTimeout <= 0.0f)
					{
						adjustTimeout = adjustWaitTime;
						tempFloat = Random.Range(5.0f, Mathf.Lerp(15.0f, 35.0f, interestValence/10.0f)) * gazeVariation;
						//SuperController.singleton.ClearMessages();
						//SuperController.LogMessage("Set H to " + Round(tempFloat), false);
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
								peronalityAdjustH = tempFloat;
								if (Random.Range(0.0f, 100.0f) > 50.0f)
								{
									peronalityAdjustH = -tempFloat;
								}
								peronalityAdjustV = (Random.Range(-5.0f, 2.0f) * Mathf.Deg2Rad) * gazeVariation;
							}
						}
					}

					//peronalityAdjustH = (Random.Range(-35.0f, 35.0f) * Mathf.Deg2Rad) * gazeVariation;
					if (Random.Range(0.0f, 100.0f) < 35.0f * rollChance)
					{
						//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
						float headLeftRight = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
						float headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
						gHeadRollTarget = Random.Range(-maxHeadRoll / 7.0f, maxHeadRoll / 7.0f);
						if (headLeftRight > lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(-maxHeadRoll / 7.0f, -0.0f);
						}
						if (headLeftRight < -lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(maxHeadRoll / 7.0f, 0.0f);
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
					saccadeAmount = Random.Range(5.0f, 10.0f);
					saccadeClock = 0.0f;
					lookAction = true;
					lookVariation = Random.Range(0.25f, 1.35f);
					browVariation = Random.Range(0.6f, 1.0f);
					eyeVariation = Random.Range(0.8f, 1.0f);
					mouthVariation = Random.Range(0.4f, 0.85f);
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
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 15.0f, interestValence/10.0f))
								{
								  browSM.Switch(bConcentrate);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(45.0f, 15.0f, interestValence/10.0f))
								{
								  browSM.Switch(bLowered);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(15.0f, 25.0f, interestArousal/10.0f))
								{
								  browSM.Switch(bApprehensive);
								}
							}
							else
							{
								if (lastBrowState == bRaised)
								{
								  browSM.Switch(bRaised);
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 35.0f, interestValence/10.0f))
								  {
									browSM.Switch(bLowered);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(20.0f, 5.0f, interestArousal/10.0f))
								  {
									browSM.Switch(bConcentrate);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 15.0f, interestArousal/10.0f))
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
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 25.0f, interestArousal/10.0f))
								  {
									browSM.Switch(bOneRaise);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 15.0f, interestValence/10.0f))
								  {
									browSM.Switch(bConcentrate);
								  }
								}
							}
						}
					}
					if (Random.Range(0.0f, 100.0f) > Mathf.Lerp(85.0f, 10.0f, interestValence/10.0f) && morphMouthAction == false)
					{
						if (interestValence > Mathf.Lerp(7.0f, 4.0f, pExtraversion/100.0f))
						{
							if (pExtraversion > Mathf.Lerp(75.0f, 55.0f, interestValence/10.0f))
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
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(35.0f, 10.0f, interestArousal/10.0f), 67.0f, interestValence/10.0f))
								  {
									mouthSM.Switch(mSmile);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(25.0f, 05.0f, interestArousal/10.0f), 35.0f, interestValence/10.0f))
								  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
									mouthSM.Switch(mBigSmile);
								  }
								  if (Random.Range(0.0f, 100.0f) <= 15.0f)
								  {
									mouthSM.Switch(mOpen);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 16.0f, interestArousal/10.0f))
								  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
									mouthSM.Switch(mSmirk);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 25.0f, interestValence/10.0f))
								  {
									mouthSM.Switch(mSideways);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 20.0f, interestValence/10.0f))
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
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(55.0f, 10.0f, interestArousal/10.0f), 67.0f, interestValence/10.0f))
									  {
										mouthSM.Switch(mSmile);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(25.0f, 05.0f, interestArousal/10.0f), 35.0f, interestValence/10.0f))
									  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										mouthSM.Switch(mBigSmile);
									  }
									  if (Random.Range(0.0f, 100.0f) <= 5.0f)
									  {
										mouthSM.Switch(mOpen);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 16.0f, interestArousal/10.0f))
									  {
										mouthSM.Switch(mSmirk);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
									  {
										mouthSM.Switch(mSideways);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 15.0f, interestValence/10.0f))
									  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										mouthSM.Switch(mBiteLip);
									  }
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
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(55.0f, 10.0f, interestArousal/10.0f), 42.0f, interestValence/10.0f))
								  {
									mouthSM.Switch(mSmile);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(15.0f, 04.0f, interestArousal/10.0f), 27.0f, interestValence/10.0f))
								  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
									mouthSM.Switch(mBigSmile);
								  }
								  if (Random.Range(0.0f, 100.0f) <= 15.0f)
								  {
									mouthSM.Switch(mOpen);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 6.0f, interestArousal/10.0f))
								  {
									mouthSM.Switch(mSmirk);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
								  {
									mouthSM.Switch(mSideways);
								  }
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
								  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
									mouthSM.Switch(mBiteLip);
								  }
							}
						}
						else
						{
							if (pExtraversion > Mathf.Lerp(75.0f, 55.0f, interestValence/10.0f))
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
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(65.0f, 0.0f, interestArousal/10.0f), 47.0f, interestValence/10.0f))
									  {
										mouthSM.Switch(mSmile);
									  }
									  if (Random.Range(0.0f, 100.0f) <= 15.0f)
									  {
										mouthSM.Switch(mOpen);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
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
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(55.0f, 30.0f, interestArousal/10.0f), 67.0f, interestValence/10.0f))
									  {
										mouthSM.Switch(mSmile);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(25.0f, 0.0f, interestArousal/10.0f), 25.0f, interestValence/10.0f))
									  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										mouthSM.Switch(mBigSmile);
									  }
									  if (Random.Range(0.0f, 100.0f) <= 10.0f)
									  {
										mouthSM.Switch(mOpen);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
									  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										mouthSM.Switch(mSideways);
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
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(15.0f, 10.0f, interestArousal/10.0f), 30.0f, interestValence/10.0f))
									  {
										mouthSM.Switch(mSmile);
									  }
									  if (Random.Range(0.0f, 100.0f) <= 15.0f)
									  {
										mouthSM.Switch(mOpen);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 6.0f, interestArousal/10.0f))
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
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(25.0f, 10.0f, interestArousal/10.0f), 37.0f, interestValence/10.0f))
									  {
										mouthSM.Switch(mSmile);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(15.0f, 0.0f, interestArousal/10.0f), 15.0f, interestValence/10.0f))
									  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										mouthSM.Switch(mBigSmile);
									  }
									  if (Random.Range(0.0f, 100.0f) <= 15.0f)
									  {
										mouthSM.Switch(mOpen);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
									  {
										mouthSM.Switch(mSideways);
									  }
									  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 5.0f, interestValence/10.0f))
									  {
											saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
											lastSaccade = "";
										mouthSM.Switch(mBiteLip);
									  }
								}
							}
						}
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
					Duration = Random.Range(2.0f, 5.0f) * uiExpressionLengthVal;
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