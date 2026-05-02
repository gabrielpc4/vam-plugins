using UnityEngine;
using Random = UnityEngine.Random;

namespace VRAdultFun
{
    partial class EmotionEngine
    {
        private class LKissing : State
        {
            public override void OnEnter()
            {
                interestKissing = true;
				if (enableKissing || 1.0f == 1.0f)
				{
					currentLook = "Kissing";
					//SuperController.LogError("Kissing start");
					//LogError("Kissing");
					shoulderUp = Random.Range(0.2f,0.2f);
					sexActionNeckX = 0.0f;
					//gHeadSpeed = 1.0f;
					saccadeAmount = Random.Range(0.0f, 0.0f);
					//saccadeClock = 0.0f;
					if (Random.Range(0.0f,100.0f) > 50.0f)
					{
						tempFloat2 = playerHeadTransform.eulerAngles.z - headController.transform.eulerAngles.z;
						if (tempFloat2 > 180.0f){tempFloat2 -= 360.0f;}
						if (tempFloat2 < -180.0f){tempFloat2 += 360.0f;}
						if (Mathf.Abs(tempFloat2) < 15.0f)
						{
							gHeadRollTarget = Mathf.Clamp(tempFloat2 + Random.Range(-15.0f,15.0f),-60.0f,60.0f);
						}
						else
						{
							gHeadRollTarget = Mathf.Clamp(tempFloat2 + Random.Range(-5.0f,5.0f),-60.0f,60.0f);
						}
						if (Mathf.Abs(gHeadRollTarget) < 25.0f)
						{
							if (person2IsMale)
							{
								gHeadRollTarget = 40.0f;
							}
							else
							{
								gHeadRollTarget = -40.0f;
							}
						}
					}
					/*tempFloat = playerHeadTransform.eulerAngles.z;
					if (tempFloat > 180.0f)
					{
						tempFloat = (360.0f - tempFloat) * -1.0f;
					}
					if (tempFloat > 0.0f && tempFloat < 15.0f)
					{
						gHeadRollTarget = tempFloat + 20.0f;
					}
					if (tempFloat < 0.0f && tempFloat > -15.0f)
					{
						gHeadRollTarget = tempFloat - 20.0f;
					}
					if (Mathf.Abs(tempFloat2) > 20.0f && Mathf.Abs(tempFloat2) < 50.0f)
					{
						//gHeadRollTarget -= tempFloat2 / 2.0f;
					}
					if (gHeadRollTarget < 0.0f)
					{
						gHeadRollTarget = Mathf.Clamp(gHeadRollTarget,-60.0f,-15.0f);
					}
					else
					{
						gHeadRollTarget = Mathf.Clamp(gHeadRollTarget,15.0f,60.0f);
					}*/
					
					
					peronalityAdjustH = 0.0f * Mathf.Deg2Rad;//Random.Range(-2.0f,2.0f) * Mathf.Deg2Rad;
					peronalityAdjustV = 0.0f * Mathf.Deg2Rad;//Random.Range(-1.0f,1.0f) * Mathf.Deg2Rad;

					lookAction = true;
					lookVariation = Random.Range(1.0f, 1.0f);
					browVariation = Random.Range(0.55f, 0.75f);
					eyeVariation = Random.Range(0.55f, 0.75f);
					mouthVariation = Random.Range(0.55f, 0.95f);
					browSM.SwitchRandom(new State[] {
								bApprehensive,
								bRaised,
								bConcentrate
							});
					if (morphMouthAction == false)
					{
						mouthSM.Switch(mKiss);
					}
					if (lipsOnly == false)
					{
						eyesSM.SwitchRandom(new State[] {
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eClosed,
									eOpen
								});
					}
					float rand = Random.Range(0.0f, 100.0f);
					if (rand > 33.0f)
					{
						mLHandFistTarget = Random.Range(0.3f, 0.6f);
					}
					if (rand < 66.0f)
					{
						mRHandFistTarget = Random.Range(0.3f, 0.6f);
					}
					Duration = Random.Range(0.273f,0.295f) * uiExpressionLengthVal;
				}
				else
				{
				Duration = 0.001f;
				}
            }
			public override void OnUpdate()
			{
				//SuperController.LogError("Kissing Look Duration " );
			}
            public override void OnInterrupt(string parameter)
            {
				//SuperController.LogError("Kissing Interrupted");
                OnTimeout();
            }
            public override void OnTimeout()
            {
                lookAction = false;
                interestKissing = false;
				morphMouthAction = false;
				//currentLook = "Idle";
				//SuperController.LogError("Kissing timed out");
            }
        }



    }

}