using System;
using System.Collections.Generic;

namespace octopussy
{
    public class AtomFloatSelector : MVRScript
    {
        protected Atom receivingAtom;
        protected JSONStorableStringChooser atomJSON;
        protected JSONStorable receiver;

        protected JSONStorableStringChooser receiverJSON;
        protected JSONStorableStringChooser receiverTargetJSON;
        protected JSONStorableFloat receiverTarget;

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

        protected virtual void SyncTarget() {}

        protected void SyncReceiverTarget(string receiverTargetName)
        {
            receiverTarget = null;
            if (receiver != null && receiverTargetName != null)
            {
                receiverTarget = receiver.GetFloatJSONParam(receiverTargetName);
                if (receiverTarget != null)
                {
                    try
                    {
                         currentValueJSON.min = receiverTarget.min;
                        currentValueJSON.max = receiverTarget.max;

                        if (!insideRestore)
                        {
                            currentValueJSON.val = receiverTarget.val;
                        }

                        SyncTarget();
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

    }
}
