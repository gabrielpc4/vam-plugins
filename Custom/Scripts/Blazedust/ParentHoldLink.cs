using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace Blazedust
{

    /// <summary>
    /// When the containing atom link is set to "Hold" this plugin will keep updating the relative position/rotation to the linked parent atom, making this into a "ParentHold" link.
    /// It will keep the position and rotation updated relative to the linked parent in the LateUpdate() function, making it look like a solid connection.
    /// </summary>
    public class ParentHoldLink : MVRScript
    {

        Vector3 linkPos;
        Vector3 linkRot;

        bool posInitDone = false;
        bool rotInitDone = false;

        JSONStorableBool active;
        JSONStorableString text;

        UIDynamicToggle toggle;
        UIDynamicTextField textField;

        public override void Init()
        {
            active = new JSONStorableBool("Active", true);
            RegisterBool(active);
            toggle = CreateToggle(active, false);
            toggle.labelText.text = "Active";

            text = new JSONStorableString("text", "");
            textField = CreateTextField(text, false);
            textField.height = 400;
            text.val = "This plugin updates the atom's relative position/rotation to follow the linked atom using the LateUpdate() function when the link type is set to 'Hold'.\n\nThis can make certain atom types follow exactly after it's linked parent per frame.\n\nIntended for CustomUnityAssets with physics turned off but might work for other atom types.";

            JSONStorableAction resetPosRotAction = new JSONStorableAction("resetPosRot", () =>
            {
                // SuperController.LogMessage("Pos/Rot reset");
                posInitDone = false;
                rotInitDone = false;   
            });
            RegisterAction(resetPosRotAction);
        }

        void OnDestroy()
        {
            RemoveToggle(toggle);
            RemoveTextField(textField);
        }

        //void FixedUpdate()
        //{

        //}

        //void Update()
        //{

        //}

        void LateUpdate()
        {
            Atom atom = GetContainingAtom();
            if (active.val && atom.mainController.linkToRB != null)
            {
                Rigidbody rbLink = atom.mainController.linkToRB;
                if (atom.mainController.currentPositionState == FreeControllerV3.PositionState.Hold)
                {
                    if (!posInitDone)
                    {
                        linkPos = rbLink.transform.InverseTransformPoint(atom.childAtomContainer.position);
                        posInitDone = true;
                        // SuperController.LogMessage("Position set");
                    }
                } 
                else
                {
                    posInitDone = false;
                }

                if (atom.mainController.currentRotationState == FreeControllerV3.RotationState.Hold)
                {
                    if (!rotInitDone)
                    {
                        linkRot = (Quaternion.Inverse(atom.childAtomContainer.transform.rotation) * rbLink.transform.rotation).eulerAngles;
                        rotInitDone = true;
                        // SuperController.LogMessage("Rotation set");
                    }
                } 
                else
                {
                    rotInitDone = false;
                }

                if (posInitDone | rotInitDone)
                {
                    Vector3 pos = (posInitDone ? rbLink.transform.TransformPoint(linkPos) : atom.transform.position);
                    Quaternion rot = (rotInitDone ? (rbLink.transform.rotation * Quaternion.Inverse(Quaternion.Euler(linkRot))) : atom.transform.rotation);
                    
                    atom.mainController.transform.SetPositionAndRotation(pos, rot);
                }
            } 
            else
            {
                posInitDone = false;
                rotInitDone = false;
            }
        }



    }
}