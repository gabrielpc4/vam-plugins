using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using SimpleJSON;

namespace geesp0t
{
    public class Person
    {
        public Atom atom;
        public Dictionary<string, FreeControllerV3> joints;
        public bool isMale;
        public FreeControllerV3 control;

        private static int Off = 0;
        private static int On = 1;
        private static int Comply = 2;
        private static int ParentLink = 3;
		private static int Hold = 4;

        public string allJointsConectedTo;
        public string chestConnectedTo;

        public Vector3 previousControlPos;
        public Vector3 previousControlRot;

        string[] controlNames = new string[] {
            "headControl",
            "chestControl",
            "hipControl",
            "lFootControl",
            "rFootControl",
            "lKneeControl",
            "rKneeControl",
            "lArmControl",
            "rArmControl",
            "lShoulderControl",
            "rShoulderControl",
            "lElbowControl",
            "rElbowControl",
            "lHandControl",
            "rHandControl",
            "lThighControl",
            "rThighControl",
            "neckControl",
            "abdomenControl",
            "abdomen2Control",
            "pelvisControl",
            "penisTipControl",
            "penisMidControl",
            "penisBaseControl",
            "testesControl",
            "rToeControl",
            "lToeControl",
            "rNippleControl",
            "lNippleControl"
        };

        public string[] keyjoints = {
            "rHandControl", "lHandControl", "rKneeControl", "lKneeControl", "rElbowControl", "lElbowControl", "headControl"
        };

        public Person(Atom personAtom)
        {            
            atom = personAtom;           
            isMale = atom.GetComponentInChildren<DAZCharacter>().isMale;  
            if (personAtom == null)
            {
                SuperController.LogMessage("null");
            }
            FreeControllerV3[] controllers = atom.GetComponentsInChildren<FreeControllerV3>(true);
            if (controllers.Length == 0)
            {
                SuperController.LogError("No controllers found for atom: " + atom.uid);
            }            
            joints = new Dictionary<string, FreeControllerV3>();
           
            foreach (var c in controllers)
            {                
                if (c.name == "control")
                {
                    control = c;
                   
                }
                
                if (controlNames.Contains(c.name))
                {
                    if (c.name == "rHandControl" ||
                        c.name == "lHandControl" ||
                        c.name == "headControl"  ||
                        c.name == "rKneeControl" ||
                        c.name == "lKneeControl" ||
                        c.name == "rFootControl" ||
                        c.name == "lFootControl" ||
                        c.name.Contains("penis"))
                    {
                        c.interactableInPlayMode = true;
                    }
                    else                    
                        c.interactableInPlayMode = false;
                   
                    joints.Add(c.name, c);
                }                
            }

            previousControlPos = joints["chestControl"].transform.position;
            previousControlRot = joints["chestControl"].transform.eulerAngles;
        }

        public void setup()
        {
            foreach (var joint in joints)
            {
                setPosRotState(joint.Key, Off);              
                joint.Value.possessable = false;
            }
        }

        public void SetJointsState(string[] jointNames, int state)
        {
            foreach (var jointName in jointNames)
            {
                setPosRotState(jointName, state);
            }
        }

        public bool isLinked(string jointName)
        {
            return joints[jointName].currentPositionState == FreeControllerV3.PositionState.ParentLink;
        }

        public void setPositionState(string jointName, int state)
        {
            FreeControllerV3.PositionState positionState = FreeControllerV3.PositionState.Comply;
            switch (state)
            {
                case 0: positionState = FreeControllerV3.PositionState.Off; break;
                case 1: positionState = FreeControllerV3.PositionState.On; break;
                case 2: positionState = FreeControllerV3.PositionState.Comply; break;
                case 3: positionState = FreeControllerV3.PositionState.ParentLink; break;
                default: break;
            }
            joints[jointName].currentPositionState = positionState;
        }

        public void setPositionState(int pair, int state)
        {
            string[] jointNames = getJointPairNames(pair);
            setPositionState(jointNames[0], state);
            setPositionState(jointNames[1], state);
        }

        public void SetPossessableJoints(string[] jointNames)
        {
            foreach (var joint in joints)
            {
               joint.Value.possessable = false;
            }

            foreach (var jointName in jointNames)
            {              
                joints[jointName].possessable = true;
				joints[jointName].canGrabPosition = true;
				joints[jointName].canGrabRotation = true;
            }
        }

        public void setRotationState(string jointName, int state)
        {
            FreeControllerV3.RotationState rotationState = FreeControllerV3.RotationState.Comply;
            switch (state)
            {
                case 0: rotationState = FreeControllerV3.RotationState.Off; break;
                case 1: rotationState = FreeControllerV3.RotationState.On; break;
                case 2: rotationState = FreeControllerV3.RotationState.Comply; break;
                case 3: rotationState = FreeControllerV3.RotationState.ParentLink; break;
                default: break;
            }

            joints[jointName].currentRotationState = rotationState;
        }

        public int getJointState(string jointName)
        {
            switch (joints[jointName].currentPositionState)
            {
                case FreeControllerV3.PositionState.Off: return Off;
                case FreeControllerV3.PositionState.On: return On;
                case FreeControllerV3.PositionState.Comply: return Comply;
				case FreeControllerV3.PositionState.Hold: return Hold;
                case FreeControllerV3.PositionState.ParentLink: return ParentLink;
                default: return On; 
            }
        }

        public int getJointRotationState(string jointName)
        {
            switch (joints[jointName].currentRotationState)
            {
                case FreeControllerV3.RotationState.Off: return Off;
                case FreeControllerV3.RotationState.On: return On;
                case FreeControllerV3.RotationState.Comply: return Comply;
                case FreeControllerV3.RotationState.ParentLink: return ParentLink;
                default: return On;
            }
        }

        public void setRotationState(int pair, int state)
        {
            string[] jointNames = getJointPairNames(pair);
            setRotationState(jointNames[0], state);
            setRotationState(jointNames[1], state);
        }

        public void setPosRotState(string jointName, int state)
        {
            setPositionState(jointName, state);
            setRotationState(jointName, state);
        }

        public void setPosRotState(int pair, int state)
        {
            string[] jointNames = getJointPairNames(pair);
            setPosRotState(jointNames[0], state);
            setPosRotState(jointNames[1], state);
        }

        public void linkJoint(string joint1Name, string joint2Name)
        {            
            if(joint1Name == "chestControl")
            {
                chestConnectedTo = joint2Name;
            }
            var jointControl = joints[joint1Name];            
            Rigidbody joint2Rb = atom.rigidbodies.First(rb => rb.name == joint2Name);
         
            jointControl.SelectLinkToRigidbody(joint2Rb, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);            
            setPosRotState(joint1Name, ParentLink);
        }

        public void linkJoints(int pair, string target)
        {            
            string[] jointNames = getJointPairNames(pair);
            linkJoint(jointNames[0], target);
            linkJoint(jointNames[1], target);
        }

        public void unlinkJoint(string jointName)
        {
            joints[jointName].SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
            if (isLinked(jointName))
            {
                setPosRotState(jointName, Comply);
            }
        }

        public void unlinkJoints(int pair)
        {
            string[] jointNames = getJointPairNames(pair);
            unlinkJoint(jointNames[0]);
            unlinkJoint(jointNames[1]);
        }

        public void unlinkAllJoints()
        {
            allJointsConectedTo = "";
            foreach (var joint in joints)
            {
                unlinkJoint(joint.Key);
            }
        }

        public void setAllJointsToState(int state)
        {
            foreach(var joint in joints)
            {
                setPosRotState(joint.Key, state);
            }
        }

        public void setJointState(string jointName, int state)
        {
            setPosRotState(jointName, state);
        }

        public void linkAllJointsTo(string jointName)
        {
            foreach (var joint in joints)
            {
                if (joint.Key != jointName)
                {
                    linkJoint(joint.Key, jointName);
                }
            }
            allJointsConectedTo = jointName;
        }

        private string[] getJointPairNames(int pair)
        {
            string joint1 = "";
            string joint2 = "";

            switch (pair)
            {
                case 0:
                    joint1 = "rHandControl";
                    joint2 = "lHandControl";
                    break;
                case 1:
                    joint1 = "rFootControl";
                    joint2 = "lFootControl";
                    break;
                case 2:
                    joint1 = "rElbowControl";
                    joint2 = "lElbowControl";
                    break;
                case 3:
                    joint1 = "rKneeControl";
                    joint2 = "lKneeControl";
                    break;
                case 4:
                    joint1 = "rThighControl";
                    joint2 = "lThighControl";
                    break;
                case 5:
                    joint1 = "rArmControl";
                    joint2 = "lArmControl";
                    break;
                case 6:
                    joint1 = "rShoulderControl";
                    joint2 = "lShoulderControl";
                    break;
                default:
                    break;
            }
            return new string[] { joint1, joint2 };
        }
    }
}
