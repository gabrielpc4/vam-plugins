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
				Duration = 0.001f;
				if (enablePlayful)
				{
					if (Random.Range(0.0f, 100.0f) > 30.0f * (2.0f-rollChance) && adjustTimeout <= 0.0f)
					{
						adjustTimeout = adjustWaitTime;
						shoulderUp = Random.Range(0.1f,0.2f);
						tempFloat = Random.Range(0.0f, 35.0f) * gazeVariation;
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
								peronalityAdjustV = (Random.Range(0.0f, Mathf.Lerp(Mathf.Lerp(-0.0f, -3.0f, Mathf.Min(playerHeadToHead, 1.0f)),Mathf.Lerp(-2.0f, -10.0f, Mathf.Min(playerHeadToHead, 1.0f)),interestArousal / 10.0f))) * gazeVariation;
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
					currentLook = "Playful";
					sexActionNeckX = 0.0f;
					//LogError("Playful");
					//gHeadSpeed = 1.0f;
					if (Random.Range(0.0f, 100.0f) < 25.0f * variationChance)
					{
						//Vector3 cross = Vector3.Cross(chestController.followWhenOff.forward, headController.followWhenOff.forward);
						gHeadRollTarget = Random.Range(-maxHeadRoll / 1.5f, -maxHeadRoll / 1.5f);
						if (headLeftRight > lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(-maxHeadRoll / 1.75f, -maxHeadRoll / 15.0f);
						}
						if (headLeftRight < -lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(maxHeadRoll / 1.5f, maxHeadRoll / 15.0f);
						}
						if (headUpDown > lookPeripheralAngle)
						{
							gHeadRollTarget = Random.Range(-maxHeadRoll / 7.0f, maxHeadRoll / 7.0f);
						}
					}
					else
					{
						//gHeadRollTarget = 0.0f;
					}
					saccadeAmount = Random.Range(6.0f, 7.0f);
					saccadeClock = 0.0f;
					lookAction = true;
					//mBrowUpTarget = Random.Range(0.0f, 0.2f);
					lookVariation = Random.Range(0.95f, 1.35f);
					browVariation = Random.Range(0.75f, 1.15f);
					eyeVariation = Random.Range(0.85f, 1.15f);
					mouthVariation = Random.Range(0.85f, 1.05f);
					//mTongueInOutTarget = 1.0f;
					
					if (Random.Range(0.0f, 100.0f) > Mathf.Lerp(10.0f, 30.0f, interestArousal/10.0f) && morphBrowAction == false)
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
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 25.0f, interestArousal/10.0f))
								{
								  browSM.Switch(bConcentrate);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(45.0f, 15.0f, interestValence/10.0f))
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
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 25.0f, interestValence/10.0f))
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
								  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 25.0f, interestArousal/10.0f))
								  {
									browSM.Switch(bConcentrate);
								  }
								}
							}
						}
					}
					  
					  
					if (playerHeadToHead > personalSpaceDistance / Mathf.Lerp(2.0f, 5.0f, pExtraversion/100.0f))
					{
						if (pExtraversion > Mathf.Lerp(35.0f, 75.0f, pAgreeableness/100.0f))
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
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(30.0f, 50.0f, interestArousal/10.0f))
								{
									mouthSM.Switch(mOpen);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 15.0f, interestArousal/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
									mouthSM.Switch(mSmirk);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 15.0f, interestValence/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
								    mouthSM.Switch(mSideways);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 15.0f, interestValence/10.0f))
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
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(30.0f, 50.0f, interestArousal/10.0f))
								{
								    mouthSM.Switch(mOpen);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 10.0f, interestArousal/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
								    mouthSM.Switch(mSmirk);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 15.0f, interestValence/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
								    mouthSM.Switch(mSideways);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 15.0f, interestValence/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
								    mouthSM.Switch(mBiteLip);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 60.0f, interestValence/10.0f))
								{
								    mouthSM.Switch(mSmile);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 20.0f, interestValence/10.0f))
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
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestValence/10.0f))
								{
								    mouthSM.Switch(mSmile);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 15.0f, interestValence/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
								    mouthSM.Switch(mBigSmile);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(30.0f, 50.0f, interestArousal/10.0f))
								{
								    mouthSM.Switch(mOpen);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 7.0f, interestArousal/10.0f))
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
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(30.0f, 50.0f, interestArousal/10.0f))
								{
								    mouthSM.Switch(mOpen);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 10.0f, interestArousal/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
								    mouthSM.Switch(mSmirk);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 15.0f, interestValence/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
								    mouthSM.Switch(mSideways);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(30.0f, 80.0f, interestValence/10.0f))
								{
								    mouthSM.Switch(mSmile);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(15.0f, 35.0f, interestValence/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
								    mouthSM.Switch(mBigSmile);
								}
								if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 15.0f, interestValence/10.0f))
								{
									saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
									lastSaccade = "";
								    mouthSM.Switch(mBiteLip);
								}
							}
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
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(40.0f, 50.0f, interestArousal/10.0f))
						{
							mouthSM.Switch(mOpen);
						}
						if (Random.Range(0.0f, 100.0f) <= 15.0f)
						{
						  mouthSM.Switch(mClosed);
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
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestValence/10.0f))
						{
						    mouthSM.Switch(mSmile);
						}
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 15.0f, interestValence/10.0f))
						{
							saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
							lastSaccade = "";
						    mouthSM.Switch(mBigSmile);
						}
					}
					
					eyesSM.Switch(eOpen);
					if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 20.0f, interestValence/10.0f))
					{
						eyesSM.Switch(eFocus);
					}
					if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 10.0f, interestArousal/10.0f))
					{
						eyesSM.Switch(eSquint);
					}
					Duration = Random.Range(1.5f, 4.0f) * uiExpressionLengthVal;
				}
            }
            public override void OnTimeout()
            {
                lookAction = false;
            }
        }
    }

}