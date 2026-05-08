using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace MVRPlugin {

	public class PhyRandomizer : MVRScript {

		protected Rigidbody RB;
		protected void SyncReceiver(string receiver) {
			if (receiver != null) {
				ForceReceiver fr;
				if (receiverNameToForceReceiver.TryGetValue(receiver, out fr)) {
					RB = fr.GetComponent<Rigidbody>();
				} else {
					RB = null;
				}
			} else {
				RB = null;
			}
		}
		protected JSONStorableStringChooser receiverChoiceJSON;


	    protected JSONStorableFloat fSpeedJSON;
	    protected JSONStorableFloat tSpeedJSON;

	    protected UIDynamicButton btnResetForce;
	    protected UIDynamicButton btnResetTorque;

	    protected UIDynamicButton btnForce50Percent;
	    protected UIDynamicButton btnTorque50Percent;

	    protected UIDynamicButton btnForce100Percent;
	    protected UIDynamicButton btnTorque100Percent;

	    protected JSONStorableFloat fXMinJSON;
	    protected JSONStorableFloat fXMaxJSON;
	    protected JSONStorableFloat fXConJSON;
        
	    protected JSONStorableFloat fYMinJSON;
	    protected JSONStorableFloat fYMaxJSON;
	    protected JSONStorableFloat fYConJSON;

	    protected JSONStorableFloat fZMinJSON;
	    protected JSONStorableFloat fZMaxJSON;
	    protected JSONStorableFloat fZConJSON;

	    protected JSONStorableFloat tXMinJSON;
	    protected JSONStorableFloat tXMaxJSON;
	    protected JSONStorableFloat tXConJSON;

	    protected JSONStorableFloat tYMinJSON;
	    protected JSONStorableFloat tYMaxJSON;
	    protected JSONStorableFloat tYConJSON;

	    protected JSONStorableFloat tZMinJSON;
	    protected JSONStorableFloat tZMaxJSON;
	    protected JSONStorableFloat tZConJSON;

		protected List<string> receiverChoices;
		protected Dictionary<string, ForceReceiver> receiverNameToForceReceiver;

	    protected List<JSONStorableFloat> _sliders; // all right-hand side sliders 
	    protected List<UIDynamicButton> _buttons; // all right-hand side buttons

        // Slider ranges and initial values
        protected float forceMin = -500;
	    protected float forceMinDefault = -250;
	    protected float forceMax = 500;
	    protected float forceMaxDefault = 500;

	    protected float torqueMin = -50;
	    protected float torqueMinDefault = -25;
	    protected float torqueMax = 50;
	    protected float torqueMaxDefault = 25;

	    protected float torqueSpeedDefault = 50f;
	    protected float forceSpeedDefault = 500f;


		public override void Init() {
			try
			{
			    _sliders = new List<JSONStorableFloat>();
			    _buttons = new List<UIDynamicButton>();

                // Left side

				receiverChoices = new List<string>();
				receiverNameToForceReceiver = new Dictionary<string, ForceReceiver>();
				foreach (ForceReceiver fr in containingAtom.forceReceivers) {
					receiverChoices.Add(fr.name);
					receiverNameToForceReceiver.Add(fr.name, fr);
				}
				receiverChoiceJSON = new JSONStorableStringChooser("receiver", receiverChoices, null, "Receiver", SyncReceiver);
				receiverChoiceJSON.storeType = JSONStorableParam.StoreType.Full;
				RegisterStringChooser(receiverChoiceJSON);
				UIDynamicPopup dp = CreateScrollablePopup(receiverChoiceJSON);
				dp.popupPanelHeight = 1100f;
				dp.popup.alwaysOpen = false;

			    var btn = CreateButton("Main Settings");
                btn.button.onClick.AddListener(() =>
                {
                    ResetRightSideUI();
                    CreateSlider(fSpeedJSON, true);
                    CreateSlider(tSpeedJSON, true);
                });

                btn = CreateButton("X Torque");
                btn.button.onClick.AddListener(() =>
                {
                    ResetRightSideUI();
                    CreateSlider(tXMinJSON, true);
                    CreateSlider(tXMaxJSON, true);
                    CreateSlider(tXConJSON, true);
                });
                btn = CreateButton("Y Torque");
                btn.button.onClick.AddListener(() =>
                {
                    ResetRightSideUI();
                    CreateSlider(tYMinJSON, true);
                    CreateSlider(tYMaxJSON, true);
                    CreateSlider(tYConJSON, true);
                });
                btn = CreateButton("Z Torque");
                btn.button.onClick.AddListener(() =>
                {
                    ResetRightSideUI();
                    CreateSlider(tZMinJSON, true);
                    CreateSlider(tZMaxJSON, true);
                    CreateSlider(tZConJSON, true);
                });

			    btnResetTorque = CreateButton("Set all torques to 0%");
                _buttons.Add(btnResetTorque);
                btnResetTorque.button.onClick.AddListener(() =>
                {
                    ResetTorques();
                });

			    btnTorque50Percent = CreateButton("Set all torques to 50%");
                _buttons.Add(btnTorque50Percent);
                btnTorque50Percent.button.onClick.AddListener(() =>
                {
                    Torque50Percent();
                });

			    btnTorque100Percent = CreateButton("Set all torques to 100%");
                _buttons.Add(btnTorque100Percent);
                btnTorque100Percent.button.onClick.AddListener(() =>
                {
                    Torque100Percent();
                });

                btn = CreateButton("X Force");
                btn.button.onClick.AddListener(() =>
                {
                    ResetRightSideUI();
                    CreateSlider(fXMinJSON, true);
                    CreateSlider(fXMaxJSON, true);
                    CreateSlider(fXConJSON, true);
                });
                btn = CreateButton("Y Force");
                btn.button.onClick.AddListener(() =>
                {
                    ResetRightSideUI();
                    CreateSlider(fYMinJSON, true);
                    CreateSlider(fYMaxJSON, true);
                    CreateSlider(fYConJSON, true);
                });
                btn = CreateButton("Z Force");
                btn.button.onClick.AddListener(() =>
                {
                    ResetRightSideUI();
                    CreateSlider(fZMinJSON, true);
                    CreateSlider(fZMaxJSON, true);
                    CreateSlider(fZConJSON, true);
                });

			    btnResetForce = CreateButton("Set all forces to 0%");
                _buttons.Add(btnResetForce);
                btnResetForce.button.onClick.AddListener(() =>
                {
                    ResetForces();
                });

			    btnForce50Percent = CreateButton("Set all forces to 50%");
			    _buttons.Add(btnForce50Percent);
                btnForce50Percent.button.onClick.AddListener(() =>
                {
                    Force50Percent();
                });

			    btnForce100Percent = CreateButton("Set all forces to 100%");
			    _buttons.Add(btnForce100Percent);
                btnForce100Percent.button.onClick.AddListener(() =>
                {
                    Force100Percent();
                });



                // Right side
				fSpeedJSON = new JSONStorableFloat("Force speed", forceSpeedDefault, 0f, 5000f, false);
				fSpeedJSON.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(fSpeedJSON);
                _sliders.Add(fSpeedJSON);
			    CreateSlider(fSpeedJSON, true);

                tSpeedJSON = new JSONStorableFloat("Torque Speed", torqueSpeedDefault, 0f, 500f, false);
			    tSpeedJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tSpeedJSON);
			    _sliders.Add(tSpeedJSON);
			    CreateSlider(tSpeedJSON, true);


                fXMinJSON = new JSONStorableFloat("Force direction X Min", forceMinDefault, forceMin, forceMax, false, true);
                fXMinJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(fXMinJSON);
			    _sliders.Add(fXMinJSON);

                fXMaxJSON = new JSONStorableFloat("Force direction X Max", forceMaxDefault, forceMin, forceMax, false, true);
                fXMaxJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(fXMaxJSON);
			    _sliders.Add(fXMaxJSON);

                fXConJSON = new JSONStorableFloat("Force direction X Constraint", 100f, 0f, 100f, false, true);
                fXConJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(fXConJSON);
			    _sliders.Add(fXConJSON);

                fYMinJSON = new JSONStorableFloat("Force direction Y Min", forceMinDefault, forceMin, forceMax, false, true);
                fYMinJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(fYMinJSON);
			    _sliders.Add(fYMinJSON);

                fYMaxJSON = new JSONStorableFloat("Force direction Y Max", forceMaxDefault, forceMin, forceMax, false, true);
                fYMaxJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(fYMaxJSON);
			    _sliders.Add(fYMaxJSON);

                fYConJSON = new JSONStorableFloat("Force direction Y Constraint", 100f, 0f, 100f, false, true);
                fYConJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(fYConJSON);
			    _sliders.Add(fYConJSON);

                fZMinJSON = new JSONStorableFloat("Force direction Z Min", forceMinDefault, forceMin, forceMax, false, true);
                fZMinJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(fZMinJSON);
			    _sliders.Add(fZMinJSON);

                fZMaxJSON = new JSONStorableFloat("Force direction Z Max", forceMinDefault, forceMin, forceMax, false, true);
                fZMaxJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(fZMaxJSON);
			    _sliders.Add(fZMaxJSON);

                fZConJSON = new JSONStorableFloat("Force direction Z Constraint", 100f, 0f, 100f, false, true);
                fZConJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(fZConJSON);
			    _sliders.Add(fZConJSON);

                tXMinJSON = new JSONStorableFloat("Torque direction X Min", torqueMinDefault, torqueMin, torqueMax,false,true);
			    tXMinJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tXMinJSON);
                _sliders.Add(tXMinJSON);

                tXMaxJSON = new JSONStorableFloat("Torque direction X Max", torqueMaxDefault, torqueMin, torqueMax,false,true);
			    tXMaxJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tXMaxJSON);
                _sliders.Add(tXMaxJSON);

                tXConJSON = new JSONStorableFloat("Torque Direction X Change Constraint", 100f, 0f, 100f, false, true);
			    tXConJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tXConJSON);
                _sliders.Add(tXConJSON);

                tYMinJSON = new JSONStorableFloat("Torque direction Y Min", torqueMinDefault, torqueMin, torqueMax, false,true);
			    tYMinJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tYMinJSON);
                _sliders.Add(tYMinJSON);

                tYMaxJSON = new JSONStorableFloat("Torque direction Y Max", torqueMaxDefault, torqueMin, torqueMax,false,true);
			    tYMaxJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tYMaxJSON);
                _sliders.Add(tYMaxJSON);

                tYConJSON = new JSONStorableFloat("Torque Direction Y Change Constraint", 100f, 0f, 100f, false, true);
			    tYConJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tYConJSON);
                _sliders.Add(tYConJSON);

                tZMinJSON = new JSONStorableFloat("Torque direction Z Min", torqueMinDefault, torqueMin, torqueMax,false,true);
			    tZMinJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tZMinJSON);
                _sliders.Add(tZMinJSON);

                tZMaxJSON = new JSONStorableFloat("Torque direction Z Max", torqueMaxDefault, torqueMin, torqueMax,false,true);
			    tZMaxJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tZMaxJSON);
                _sliders.Add(tZMaxJSON);

                tZConJSON = new JSONStorableFloat("Torque Direction Z Change Constraint", 100f, 0f, 100f, false, true);
			    tZConJSON.storeType = JSONStorableParam.StoreType.Full;
			    RegisterFloat(tZConJSON);
                _sliders.Add(tZConJSON);

			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

	    protected float randomMin;
	    protected float randomMax;
	    protected float range;

	    private float forceStartTime;
	    private float forceJourneyLength;
	    protected Vector3 forceStart;
		protected Vector3 forceTarget;
		protected Vector3 forceCurrent;

	    private float torqueStartTime;
	    private float torqueJourneyLength;
        protected Vector3 torqueStart;
		protected Vector3 torqueTarget;
		protected Vector3 torqueCurrent;


		protected void Start()
		{
		}

	    protected void ResetRightSideUI()
	    {
            RemoveAllSliders();
            //RemoveAllButtons();
	    }

	    protected void RemoveAllSliders()
	    {
	        foreach (var slider in _sliders)
	        {
	            RemoveSlider(slider);
	        }
	    }

	    protected void RemoveAllButtons()
	    {
	        foreach (var button in _buttons)
	        {
	            RemoveButton(button);
	        }
	    }

	    protected void ResetForces()
	    {

                fXMinJSON.val = 0;
                fXMaxJSON.val = 0;
                fYMinJSON.val = 0;
                fYMaxJSON.val = 0;
                fZMinJSON.val = 0;
                fZMaxJSON.val = 0;
	    }

	    protected void ResetTorques()
	    {
                tXMinJSON.val = 0;
                tXMaxJSON.val = 0;
                tYMinJSON.val = 0;
                tYMaxJSON.val = 0;
                tZMinJSON.val = 0;
                tZMaxJSON.val = 0;
	    }

	    protected void Force50Percent()
	    {
	        float halfMinForce = forceMin / 2;
	        fXMinJSON.val = halfMinForce;  
	        fYMinJSON.val = halfMinForce;  
	        fZMinJSON.val = halfMinForce; 

	        float halfMaxForce = forceMax / 2;
	        fXMaxJSON.val = halfMaxForce;
	        fYMaxJSON.val = halfMaxForce; 
	        fZMaxJSON.val = halfMaxForce;
	    }

        protected void Force100Percent()
        {
            fXMinJSON.val = forceMin;
            fYMinJSON.val = forceMin;
            fZMinJSON.val = forceMin;

            fXMaxJSON.val = forceMax;
            fYMaxJSON.val = forceMax;
            fZMaxJSON.val = forceMax;
        }

	    protected void Torque50Percent()
	    {
	        float halfMinTorque = torqueMin / 2;
	        tXMinJSON.val = halfMinTorque;
	        tYMinJSON.val = halfMinTorque;  
	        tZMinJSON.val = halfMinTorque; 

	        float halfMaxTorque = torqueMax / 2;
	        tXMaxJSON.val = halfMaxTorque;
	        tYMaxJSON.val = halfMaxTorque;
	        tZMaxJSON.val = halfMaxTorque;  
	    }

        protected void Torque100Percent()
        {
            tXMinJSON.val = torqueMin;
            tYMinJSON.val = torqueMin;
            tZMinJSON.val = torqueMin;

            tXMaxJSON.val = torqueMax;
            tYMaxJSON.val = torqueMax;
            tZMaxJSON.val = torqueMax;
        }


	    protected float Random(float current, float lowerLimit, float upperLimit, float constraint)
	    {
	        range = ((upperLimit - lowerLimit) / 100) * constraint;
	        randomMin = current - range;
	        randomMax = current + range;
	        if (randomMin < lowerLimit)
	        {
	            randomMin = lowerLimit;
	        }

	        if (randomMax > upperLimit)
	        {
	            randomMax = upperLimit;
	        }

	        return UnityEngine.Random.Range(randomMin, randomMax);
	    }

	    protected void SetForceTargets()
	    {
		    forceStart = forceTarget;
		    forceStartTime = Time.time;

		    Vector3 forceDirection;
		    forceDirection.x = Random(forceCurrent.x, fXMinJSON.val, fXMaxJSON.val, fXConJSON.val);
            forceDirection.y = Random(forceCurrent.y, fYMinJSON.val, fYMaxJSON.val, fYConJSON.val);
            forceDirection.z = Random(forceCurrent.z, fZMinJSON.val, fZMaxJSON.val, fZConJSON.val);

		    forceTarget = forceDirection;
		    forceJourneyLength = Vector3.Distance(forceStart, forceTarget);
	    }

		protected void SetTorqueTargets()
		{
		    torqueStart = torqueTarget;
		    torqueStartTime = Time.time;

		    Vector3 torqueDirection;

		    torqueDirection.x = Random(torqueCurrent.x, tXMinJSON.val, tXMaxJSON.val, tXConJSON.val);
            torqueDirection.y = Random(torqueCurrent.y, tYMinJSON.val, tYMaxJSON.val, tYConJSON.val);
            torqueDirection.z = Random(torqueCurrent.z, tZMinJSON.val, tZMaxJSON.val, tZConJSON.val);

            torqueTarget = torqueDirection;
		    torqueJourneyLength = Vector3.Distance(torqueStart, torqueTarget);
		}

		protected void Update()
		{
        }



		// FixedUpdate is called with each physics simulation frame by Unity
		void FixedUpdate() {
			try
			{

                // Force
			    float forceDistCovered = (Time.time - forceStartTime) * fSpeedJSON.val;
			    float forceFractJourney = forceDistCovered / forceJourneyLength;
			    forceCurrent = Vector3.Lerp(forceStart, forceTarget, forceFractJourney);

                // Torque
			    float torqueDistCovered = (Time.time - torqueStartTime) * tSpeedJSON.val;
			    float torqueFractJourney = torqueDistCovered / torqueJourneyLength;
				torqueCurrent = Vector3.Lerp(torqueStart, torqueTarget, torqueFractJourney);
            
				if (RB && (!SuperController.singleton || !SuperController.singleton.freezeAnimation)) {
					RB.AddForce(forceCurrent, ForceMode.Force);
					RB.AddTorque(torqueCurrent, ForceMode.Force);
				}
                if (torqueFractJourney >= 1f)
                {
                    SetTorqueTargets();
                }
                if (forceFractJourney >= 1f)
                {
                    SetForceTargets();
                }
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

	}
}