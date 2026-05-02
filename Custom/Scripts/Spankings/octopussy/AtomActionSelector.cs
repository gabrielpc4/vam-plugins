using System.Collections.Generic;
using System.Linq;

namespace octopussy
{
    class AtomActionSelector
    {
        protected JSONStorableStringChooser atomJSON;
        protected Atom receivingAtom;
        protected JSONStorableStringChooser receiverJSON;
        protected JSONStorable receiver;
        protected JSONStorableStringChooser receiverTargetActionJSON;
        public string action;

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


        string[] skipAction = new string[2] { "Save", "Restore" };

        protected void SyncReceiver(string receiverID)
        {

            List<string> receiverTargetFloatChoices = new List<string>();
            List<string> receiverTargetActionChoices = new List<string>();
            List<string> receiverTargetAllChoices = new List<string>();

            receiverTargetFloatChoices.Add("None");
            receiverTargetActionChoices.Add("None");
            receiverTargetAllChoices.Add("None");
            if (receivingAtom != null && receiverID != null)
            {
                receiver = receivingAtom.GetStorableByID(receiverID);
                if (receiver != null)
                {
                    foreach (string action in receiver.GetActionNames())
                    {
                        if (!skipAction.Any(x => action.StartsWith(x)))
                        {
                            receiverTargetActionChoices.Add(action);
                        }
                    }
                }
            }
            else
            {
                receiver = null;
            }
            receiverTargetActionJSON.choices = receiverTargetActionChoices;
            receiverTargetActionJSON.val = "None";
        }

        protected void SyncReceiverTarget(string receiverTargetName)
        {
        }

        protected void SyncReceiverAction(string receiverTargetName)
        {
            action = receiverTargetName;
        }


        public void SetupActionCallback(MVRScript script)
        {
            atomJSON = new JSONStorableStringChooser("atom", SuperController.singleton.GetAtomUIDs(), null, "Atom", SyncAtom);
            receiverJSON = new JSONStorableStringChooser("receiver", null, null, "Receiver", SyncReceiver);
            receiverTargetActionJSON = new JSONStorableStringChooser("receiver actions", null, null, "Actions", SyncReceiverAction);

            script.RegisterStringChooser(atomJSON);
            script.RegisterStringChooser(receiverJSON);
            script.RegisterStringChooser(receiverTargetActionJSON);

            SyncAtomChoices();

            UIDynamicPopup dp = script.CreateScrollablePopup(atomJSON, true);
            dp.popupPanelHeight = 1100f;
            dp.popup.onOpenPopupHandlers += SyncAtomChoices;
            dp = script.CreateScrollablePopup(receiverJSON, true);
            dp.popupPanelHeight = 960f;
            dp = script.CreateScrollablePopup(receiverTargetActionJSON, true);
            dp.popupPanelHeight = 620f;

            script.CreateButton("Call action", true).button.onClick.AddListener(() =>
            {
                receiver.CallAction(action);
            });
        }

        public void CallAction()
        {
            if (action != null) receiver.CallAction(action);
        }
    }
}
