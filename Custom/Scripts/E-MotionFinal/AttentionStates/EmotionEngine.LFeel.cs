using Random = UnityEngine.Random;
using UnityEngine;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class LFeel : State
        {
            public override void OnEnter()
            {
				//SuperController.LogError("Look Start");
				Duration = 0.001f;
				if (enableFeel)
				{
					//SuperController.LogError("Enabled");
					tempFloat = Random.Range(05.0f, Mathf.Lerp(35.0f, 55.0f, interestValence/10.0f)) * gazeVariation;
					if ((Random.Range(0.0f, 100.0f) > 30.0f * (2.0f-variationChance) && adjustTimeout <= 0.0f))
					{
						adjustTimeout = adjustWaitTime;
						tempFloat = Random.Range(Mathf.Lerp(0.0f, 15.0f, interestArousal/10.0f), Mathf.Lerp(45.0f, 60.0f, interestValence/10.0f)) * gazeVariation;
						peronalityAdjustV = 0.0f;
						if (targetH * Mathf.Rad2Deg < -lookDirectAngle*2.0f)
						{
							peronalityAdjustH = -tempFloat;
						}
						else
						{
							if (targetH * Mathf.Rad2Deg > lookDirectAngle*2.0f)
							{
								peronalityAdjustH = tempFloat;
							}
							else
							{
								peronalityAdjustH = Random.Range(-tempFloat, tempFloat);
								peronalityAdjustV = Random.Range(-15.0f, 15.0f);
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

					sexActionNeckX = Random.Range(0.0f,10.0f);
					shoulderUp = Random.Range(0.2f,0.5f);
					currentLook = "Feel";
					//LogError("Feel");
					//gHeadSpeed = 2.0f;
					if (Random.Range(0.0f, 100.0f) < 15.0f * rollChance)
					{
						float headLeftRight = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, chestController.followWhenOff.up);
						float headUpDown = Vector3.SignedAngle (chestController.followWhenOff.forward, headController.followWhenOff.forward, -chestController.followWhenOff.right);
						gHeadRollTarget = Random.Range(-maxHeadRoll / 2.0f, maxHeadRoll / 2.0f);
						if (headLeftRight > lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(-maxHeadRoll / 3.0f, -maxHeadRoll / 5.0f);
						}
						if (headLeftRight < -lookDirectAngle)
						{
							gHeadRollTarget = Random.Range(maxHeadRoll / 3.0f, maxHeadRoll / 5.0f);
						}
						if (headUpDown > lookPeripheralAngle)
						{
							gHeadRollTarget = Random.Range(-maxHeadRoll / 5.0f, maxHeadRoll / 5.0f);
						}
					}
					saccadeAmount = Random.Range(2.0f, 3.0f);
					//mBrowUpTarget = Random.Range(0.2f, 0.5f);
					saccadeClock = 0.0f;
					lookAction = true;
					lookVariation = Random.Range(0.35f, 0.75f);
					browVariation = Random.Range(0.20f, 0.55f);
					eyeVariation = Random.Range(0.05f, 0.35f);
					mouthVariation = Random.Range(0.45f, 0.75f);
					browSM.Switch(bRaised);
					if (Random.Range(0.0f, 100.0f) <= 5.0f)
					{
						browSM.Switch(bLowered);
					}
					if (Random.Range(0.0f, 100.0f) <= 40.0f)
					{
						browSM.Switch(bApprehensive);
					}
					if (Random.Range(0.0f, 100.0f) <= 30.0f)
					{
						browSM.Switch(bConcentrate);
					}
					if (Random.Range(0.0f, 100.0f) <= 5.0f)
					{
						browSM.Switch(bOneRaise);
					}


					if (1==1)
					{
						if ((playerLHandInteract || playerRHandInteract || playerHeadInteract || playerPenisInteract) && interestArousal > 7.0f)
						{
							//SuperController.LogError("Feeling");
							mouthSM.Switch(mBigSmile);
							if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(0.0f, 30.0f, interestValence/10.0f))
							{
								mouthSM.Switch(mBiteLip);
							}
							if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(15.0f, 30.0f, interestValence/10.0f), 57.0f, interestArousal/10.0f))
							{
								saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
								lastSaccade = "";
								mouthSM.Switch(mJoy);
							}
							if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(05.0f, 10.0f, interestValence/10.0f), 25.0f, interestArousal/10.0f))
							{
								saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
								lastSaccade = "";
								mouthSM.Switch(mOh);
							}
						}
						else
						{
							//SuperController.LogError("Not Feeling");
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
									if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 35.0f, interestArousal/10.0f))
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
									if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 25.0f, interestValence/10.0f))
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
									if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 25.0f, interestValence/10.0f))
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
									if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 30.0f, interestValence/10.0f))
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
									if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 25.0f, interestValence/10.0f))
									{
										saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
										lastSaccade = "";
										mouthSM.Switch(mBiteLip);
									}
								}
							}
						}
					}
					/*else
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
						if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 30.0f, interestValence/10.0f))
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
					}*/



					  eyesSM.Switch(eOpen);
					  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(10.0f, 50.0f, interestArousal/10.0f))
					  {
						eyesSM.Switch(eClosed);
					  }
					  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 10.0f, interestValence/10.0f))
					  {
						eyesSM.Switch(eFocus);
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
					Duration = Random.Range(1.5f, 4.0f) * uiExpressionLengthVal;
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