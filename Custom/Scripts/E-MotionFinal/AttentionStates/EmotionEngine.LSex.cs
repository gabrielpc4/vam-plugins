using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class LSex : State
        {
            public override void OnEnter()
            {
				Duration = 0.001f;
				if (enableSex)
				{
				currentLook = "Sex";
                //LogError("Sex");
                //gHeadSpeed = 2.0f;
				shoulderUp = Random.Range(-0.2f,0.5f);
                peronalityAdjustH = Random.Range(-15.0f, 15.0f);
                peronalityAdjustV = Random.Range(-45.0f, 45.0f);
                if (playerTipToPelvis < interactionDistance * 1.5f || interestArousal > 8.0f)
                {
                    peronalityAdjustH = Random.Range(-35.0f, 35.0f) * Mathf.Deg2Rad;
                    peronalityAdjustV = Random.Range(45.0f, interestArousal * 10.0f) * Mathf.Deg2Rad;
                }
                if (Random.Range(0.0f, 100.0f) < 15.0f * rollChance)
                {
                    gHeadRollTarget = Random.Range(-5.0f, 5.0f);
                }
                else
                {
                    gHeadRollTarget = 0.0f;
                }
                saccadeAmount = Random.Range(0.0f, 5.0f);
                saccadeClock = 0.0f;
                lookAction = true;
                lookVariation = Random.Range(0.8f, 1.35f);
                browVariation = Random.Range(0.8f, 1.35f);
                eyeVariation = Random.Range(0.8f, 1.35f);
                mouthVariation = Random.Range(0.8f, 1.35f);
				sexActionNeckX = Random.Range(-15.0f, 10.0f);
				//mDeserveItTarget = Random.Range(0.0f, 0.5f);
				//mTakingItTarget = Random.Range(0.0f, 0.5f);
                browSM.SwitchRandom(new State[] {
                            bApprehensive,
                            bApprehensive,
                            bApprehensive,
                            bConcentrate,
                            bConcentrate,
                            bConcentrate
                        });

					if (Random.Range(0.0f, 100.0f) > Mathf.Lerp(35.0f, 90.0f, pExtraversion/100.0f) && morphMouthAction == false)
					{
						if (interestValence > Mathf.Lerp(6.0f, 3.5f, pExtraversion/100.0f))
						{
							if (smiledlast)
							{
							  mouthSM.Switch(mClosed);
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(15.0f, 60.0f, interestArousal/10.0f), 77.0f, interestValence/10.0f))
							  {
										saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
										lastSaccade = "";
								mouthSM.Switch(mJoy);
							  }
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(25.0f, 55.0f, interestArousal/10.0f), 95.0f, interestValence/10.0f))
							  {
										saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
										lastSaccade = "";
								mouthSM.Switch(mOh);
							  }
							  if (Random.Range(0.0f, 100.0f) <= 45.0f)
							  {
								mouthSM.Switch(mOpen);
							  }
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 16.0f, interestArousal/10.0f))
							  {
								mouthSM.Switch(mSmile);
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
							  mouthSM.Switch(mClosed);
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(15.0f, 60.0f, interestArousal/10.0f), 77.0f, interestValence/10.0f))
							  {
										saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
										lastSaccade = "";
								mouthSM.Switch(mJoy);
							  }
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(Mathf.Lerp(25.0f, 75.0f, interestArousal/10.0f), 95.0f, interestValence/10.0f))
							  {
										saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
										lastSaccade = "";
								mouthSM.Switch(mOh);
							  }
							  if (Random.Range(0.0f, 100.0f) <= 45.0f)
							  {
								mouthSM.Switch(mOpen);
							  }
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(2.0f, 26.0f, interestArousal/10.0f))
							  {
								mouthSM.Switch(mSmile);
							  }
							  if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 20.0f, interestValence/10.0f))
							  {
										saccadeOffset = new Vector3(0.0f, 0.0f, 0.0f);
										lastSaccade = "";
								mouthSM.Switch(mBiteLip);
							  }
							}
						}
					}

				eyesSM.Switch(eOpen);
				if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 70.0f, interestArousal/10.0f))
				{
					eyesSM.Switch(eClosed);
				}
				if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 30.0f, interestValence/10.0f))
				{
					eyesSM.Switch(eSquint);
				}
				if (Random.Range(0.0f, 100.0f) <= Mathf.Lerp(5.0f, 40.0f, interestArousal/10.0f))
				{
					eyesSM.Switch(eFocus);
				}
                float rand = Random.Range(0.0f, 100.0f);
                if (rand > pExtraversion)
                {
                    if (rand > 33.0f)
                    {
                        mLHandFistTarget = Random.Range(0.0f, 1.2f);
                    }
                    if (rand < 66.0f)
                    {
                        mRHandFistTarget = Random.Range(0.0f, 1.2f);
                    }
                }
                else
                {
                    if (rand > 33.0f)
                    {
                        mLHandStraightenTarget = Random.Range(0.0f, 0.5f);
                        mLHandFistTarget = Random.Range(0.0f, 0.5f);
                    }
                    if (rand < 66.0f)
                    {
                        mRHandStraightenTarget = Random.Range(0.0f, 0.5f);
                        mRHandFistTarget = Random.Range(0.0f, 0.5f);
                    }
                }
                Duration = Random.Range(1.0f, 1.0f) * uiExpressionLengthVal;
				}
            }
            public override void OnInterrupt(string parameter)
            {
                OnTimeout();
            }
            public override void OnTimeout()
            {
                lookAction = false;
				sexActionNeckX = 0.0f;
				mDeserveItTarget = 0.0f;
				mTakingItTarget = 0.0f;
            }
        }
    }

}