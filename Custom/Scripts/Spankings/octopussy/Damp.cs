using System;
using UnityEngine;
using System.Collections.Generic;

namespace octopussy
{
    //incomplete
    public class Damp : MVRScript
    {
        protected Atom receivingAtom;
        protected JSONStorableStringChooser atomJSON;
        protected JSONStorable receiver;

        public JSONStorableAction actionJSON;
        protected JSONStorableStringChooser receiverJSON;
        protected JSONStorableStringChooser receiverTargetJSON;
        protected JSONStorableFloat receiverTarget;

        protected JSONStorableFloat maxValueJSON;
        protected JSONStorableFloat incrementJSON;
        protected JSONStorableFloat dampTargetJSON;
        protected JSONStorableFloat dampSpeedJSON;
        protected JSONStorableFloat currentValueJSON;

        protected void SyncAtomChoices()
        {
            List<string> atomChoices = new List<string>();
            atomChoices.Add("None");
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
                atomChoices.Add(atomUID);
            }
            atomJSON.choices = atomChoices;
        }

        protected void SyncAtom(string atomUID)
        {
            List<string> receiverChoices = new List<string>();
            receiverChoices.Add("None");
            if (atomUID != null)
            {
                receivingAtom = SuperController.singleton.GetAtomByUid(atomUID);
                if (receivingAtom != null)
                {
                    foreach (string receiverChoice in receivingAtom.GetStorableIDs())
                    {
                        receiverChoices.Add(receiverChoice);
                        //SuperController.LogMessage("Found receiver " + receiverChoice);
                    }
                }
            }
            else
            {
                receivingAtom = null;
            }
            receiverJSON.choices = receiverChoices;
            receiverJSON.val = "None";
        }



        protected void SyncReceiver(string receiverID)
        {
            List<string> receiverTargetChoices = new List<string>();
            receiverTargetChoices.Add("None");
            if (receivingAtom != null && receiverID != null)
            {
                receiver = receivingAtom.GetStorableByID(receiverID);
                if (receiver != null)
                {
                    foreach (string floatParam in receiver.GetFloatParamNames())
                    {
                        receiverTargetChoices.Add(floatParam);
                    }
                }
            }
            else
            {
                receiver = null;
            }
            receiverTargetJSON.choices = receiverTargetChoices;
            receiverTargetJSON.val = "None";
        }


        protected void SyncReceiverTarget(string receiverTargetName)
        {
            receiverTarget = null;
            if (receiver != null && receiverTargetName != null)
            {
                receiverTarget = receiver.GetFloatJSONParam(receiverTargetName);
                if (receiverTarget != null)
                {
                    /*lowerValueJSON.min = receiverTarget.min;
                    lowerValueJSON.max = receiverTarget.max;
                    upperValueJSON.min = receiverTarget.min;
                    upperValueJSON.max = receiverTarget.max;*/
                    try
                    {
                        currentValueJSON.min
                            = maxValueJSON.min
                            = receiverTarget.min;

                        currentValueJSON.max
                            = maxValueJSON.max
                            = receiverTarget.max;

                        if (!insideRestore)
                        {
                            // only sync up val if not in restore
                            //lowerValueJSON.val = receiverTarget.val;
                            //upperValueJSON.val = receiverTarget.val;
                            currentValueJSON.val
                                = maxValueJSON.val
                                = receiverTarget.val;
                        }
                    }
                    catch (Exception e)
                    {
                        SuperController.LogError("Exception caught: " + e);
                    }
                }
            }
        }

        public override void Init()
        {
            try
            {

                atomJSON = new JSONStorableStringChooser("atom", SuperController.singleton.GetAtomUIDs(), null, "Atom", SyncAtom);
                receiverJSON = new JSONStorableStringChooser("receiver", null, null, "Receiver", SyncReceiver);
                receiverTargetJSON = new JSONStorableStringChooser("receiverTarget", null, null, "Target", SyncReceiverTarget);

                RegisterStringChooser(atomJSON);
                RegisterStringChooser(receiverJSON);
                RegisterStringChooser(receiverTargetJSON);

                SyncAtomChoices();

                UIDynamicPopup dp = CreateScrollablePopup(atomJSON);
                dp.popupPanelHeight = 1100f;
                dp.popup.onOpenPopupHandlers += SyncAtomChoices;
                dp = CreateScrollablePopup(receiverJSON);
                dp.popupPanelHeight = 960f;
                dp = CreateScrollablePopup(receiverTargetJSON);
                dp.popupPanelHeight = 820f;

                atomJSON.val = containingAtom.uid;

                incrementJSON = new JSONStorableFloat("increment", 0f, 0f, 1f, false);
                maxValueJSON = new JSONStorableFloat("max value", 0f, 0f, 1f, false);
                dampTargetJSON = new JSONStorableFloat("damp target", 0f, 0f, 1f, false);
                dampSpeedJSON = new JSONStorableFloat("damp speed", 0f, 0f, 1f, false);

                RegisterFloat(maxValueJSON);
                RegisterFloat(incrementJSON);
                RegisterFloat(dampTargetJSON);
                RegisterFloat(dampSpeedJSON);

                CreateButton("Increase").button.onClick.AddListener(() => Increment() );
                JSONStorableAction actionJSON = new JSONStorableAction("Increment", Increment);
                RegisterAction(actionJSON);

                CreateSlider(maxValueJSON, true);
                CreateSlider(incrementJSON, true);
                CreateSlider(dampSpeedJSON, true);
                CreateSlider(dampTargetJSON, true);

                currentValueJSON = new JSONStorableFloat("currentValue", 0f, -1f, 1f, true, false);
                // don't register - read only
                UIDynamicSlider ds = CreateSlider(currentValueJSON, true);
                ds.defaultButtonEnabled = false;
                ds.quickButtonsEnabled = false;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        public void Increment()
        {
            if (receiverTarget != null && receiverTarget.val + incrementJSON.val < maxValueJSON.val)
            {
                receiverTarget.val += incrementJSON.val;
            }
        }


        public void Update() {
            if(receiverTarget != null && receiverTarget.val > dampTargetJSON.val) {
                currentValueJSON.val = receiverTarget.val -= dampSpeedJSON.val * Time.deltaTime;
            }

        }

    }
}
