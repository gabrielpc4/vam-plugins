using System;
using UnityEngine;
using System.Collections.Generic;
/***********************************************************************************************************************************
 * prestigitis_vamClock.cs
 * 
 * run this script as a session plugin to change the version text in the UI to one of the following:
 *      - Time
 *      - Date
 *      - Date + Time
 *      - Time Wasted (time since VaM startup)
 * 
 * This work is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License
 * https://creativecommons.org/licenses/by-nc-sa/4.0/
 *
 ***********************************************************************************************************************************/
namespace prestigitis {
	public class vamClock_20191121 : MVRScript {
/***********************************************************************************************************************************/
/* configuration settings
/***********************************************************************************************************************************/
protected TextAnchor UI_alignment = TextAnchor.MiddleCenter; //options are: Lower/Middle/Upper + Center/Left/Right
protected float UI_lineSpacing = 0.80f;                      //space between lines in the display replacing the version text
/***********************************************************************************************************************************/

        // IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
        // some reason

        // IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
        // is called right after creation

        protected JSONStorableStringChooser displayChoiceJSON;
        protected List<string> displayChoices;
        protected string originalVersionText;
        protected UnityEngine.UI.Text UITextField;

        protected void SyncChoices(string input)
        {
            //placeholder function
        }

        public override void Init() {
			try {
                // put init code in here

                // create custom JSON storable params here if you want them to be stored with scene JSON
                // types are JSONStorableFloat, JSONStorableBool, JSONStorableString, JSONStorableStringChooser
                displayChoices = new List<string>();
                displayChoices.Add("Time");
                displayChoices.Add("Date");
                displayChoices.Add("Date + Time");
                displayChoices.Add("Time Wasted");
                displayChoiceJSON = new JSONStorableStringChooser("display", displayChoices, null, "Display", SyncChoices);
                displayChoiceJSON.storeType = JSONStorableParam.StoreType.Full;
                RegisterStringChooser(displayChoiceJSON);
                UIDynamicPopup dp = CreateScrollablePopup(displayChoiceJSON);
                displayChoiceJSON.val = "Time";
            }
            catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Start is called once before Update or FixedUpdate is called and after Init()
		void Start() {
            try
            {
                UITextField = SuperController.singleton.GetAtomByUid("CoreControl").gameObject.transform.Find("WorldScaleAdjust/HUD/LowerHUDPivot/LowerHUDFlip/Scene Control Canvas/Panel/Left/VersionText").GetComponent<UnityEngine.UI.Text>();
                originalVersionText = UITextField.text;
                UITextField.resizeTextForBestFit = true;
                UITextField.lineSpacing = UI_lineSpacing;
                UITextField.alignment = UI_alignment;
            }
            catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

        // Update is called with each rendered frame by Unity
        void Update()
        {
            try
            {
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
        void LateUpdate()
        {
            try
            {
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // FixedUpdate is called with each physics simulation frame by Unity
        void FixedUpdate() {
			try {
                switch (displayChoiceJSON.val)
                {
                    case "Time":
                        ChangeVersionText(System.DateTime.Now.ToLongTimeString());
                        break;
                    case "Date":
                        ChangeVersionText(System.DateTime.Now.ToLongDateString());
                        break;
                    case "Date + Time":
                        ChangeVersionText(string.Concat(System.DateTime.Now.ToLongDateString(), System.Environment.NewLine, System.DateTime.Now.ToLongTimeString())); //long date + time
                        break;
                    case "Time Wasted":
                        float counter = Time.realtimeSinceStartup;
                        ChangeVersionText(string.Concat("Time Wasted: ", System.Environment.NewLine, Mathf.Floor((counter / 3600)).ToString(), "h ", Mathf.Floor(((counter % 3600) / 60)).ToString(), "m ", Mathf.Floor(((counter % 3600) % 60)).ToString(), "s"));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// OnDestroy is where you should put any cleanup
		// if you registered objects to supercontroller or atom, you should unregister them here
		void OnDestroy() {
		}
        public void OnDisable()
        {
            ChangeVersionText(originalVersionText);
        }
        public void OnEnable()
        {
        }
        void ChangeVersionText(string newText)
        {
            UITextField.text = newText;
        }

    }
}