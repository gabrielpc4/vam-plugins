using System;
using UnityEngine;
using System.Collections.Generic;


namespace octopussy
{
    //incomplete
    public class Ramp : MVRScript
    {
        protected Atom receivingAtom;
        protected JSONStorableStringChooser atomJSON;
        protected JSONStorable receiver;

        public JSONStorableAction actionJSON;
        protected JSONStorableStringChooser receiverJSON;
        protected JSONStorableStringChooser receiverTargetJSON;
        protected JSONStorableFloat receiverTarget;

        protected JSONStorableFloat targetValueJSON;
        protected JSONStorableFloat decayJSON;
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
                            = targetValueJSON.min
                            = receiverTarget.min;

                        currentValueJSON.max
                            = targetValueJSON.max
                            = receiverTarget.max;

                        if (!insideRestore)
                        {
                            // only sync up val if not in restore
                            //lowerValueJSON.val = receiverTarget.val;
                            //upperValueJSON.val = receiverTarget.val;
                            currentValueJSON.val
                                = targetValueJSON.val
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

                targetValueJSON = new JSONStorableFloat("target value", 0f, 0f, 1f, false);
                decayJSON = new JSONStorableFloat("decay", 0f, 0f, 1f, false);

                RegisterFloat(targetValueJSON);

                RegisterFloat(decayJSON);
                JSONStorableAction actionJSON = new JSONStorableAction("Play", Restart);
                RegisterAction(actionJSON);

                CreateSlider(targetValueJSON, true);
                CreateSlider(decayJSON, true);

                CreateButton("Restart").button.onClick.AddListener(() => { Restart(); });

                currentValueJSON = new JSONStorableFloat("currentValue", 0f, 0f, 1f, true, false);
                // don't register - this is for viewing only and is generated
                UIDynamicSlider ds = CreateSlider(currentValueJSON, true);
                ds.defaultButtonEnabled = false;
                ds.quickButtonsEnabled = false;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        protected float timer = 0f;

        protected void Start()
        {

        }

        public void Restart()
        {
            try
            {
                timer = decayJSON.val;
                currentValueJSON.val = receiverTarget.val;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        protected void Update()
        {
            try
            {
                if (timer > 0.0f)
                {
                    currentValueJSON.val = Mathf.Lerp(currentValueJSON.val, targetValueJSON.val, Time.deltaTime);
                    if (receiverTarget != null)
                    {
                        receiverTarget.val = currentValueJSON.val;
                    }
                    timer -= Time.deltaTime;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }


    }
}
