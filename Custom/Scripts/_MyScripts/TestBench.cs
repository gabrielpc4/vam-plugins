using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
	public class TestBench
	{
		private FreeControllerV3[] jointControls;
		Atom currentAtom;
		static string head = "headControl";
		static string chest = "chestControl";
		static string hip = "hipControl";
		static string lFoot = "lFootControl";
		static string rFoot = "rFootControl";
		static string lKnee = "lKneeControl";
		static string rKnee = "rKneeControl";
		static string lShoulder = "lArmControl";
		static string rShoulder = "rArmControl";
		static string lElbow = "lElbowControl";
		static string rElbow = "rElbowControl";
		static string lHand = "lHandControl";
		static string rHand = "rHandControl";
		static string lThigh = "lThighControl";
		static string rThigh = "rThighControl";
		static string neck = "neckControl";
		static string abdomen = "abdomenControl";
		static string abdomen2 = "abdomen2Control";
		static string pelvis = "pelvisControl";
		static string penisTip = "penisTipControl";
		static string penisMid = "penisMidControl";
		static string penisBase = "penisBaseControl";
		static string testicles = "testesControl";

		FreeControllerV3 headControl;
		FreeControllerV3 chestControl;
		FreeControllerV3 hipControl;
		FreeControllerV3 lFootControl;
		FreeControllerV3 rFootControl;
		FreeControllerV3 lKneeControl;
		FreeControllerV3 rKneeControl;
		FreeControllerV3 lArmControl;
		FreeControllerV3 rArmControl;
		FreeControllerV3 lElbowControl;
		FreeControllerV3 rElbowControl;
		FreeControllerV3 lHandControl;
		FreeControllerV3 rHandControl;
		FreeControllerV3 lThighControl;
		FreeControllerV3 rThighControl;
		FreeControllerV3 neckControl;
		FreeControllerV3 abdomenControl;
		FreeControllerV3 abdomen2Control;
		FreeControllerV3 pelvisControl;
		FreeControllerV3 penisTipControl;
		FreeControllerV3 penisMidControl;
		FreeControllerV3 penisBaseControl;
		FreeControllerV3 testesControl;

		string[] notInteractableJoints = { abdomen, abdomen2, pelvis, hip };
		string[] possessableJoints = { };
		string[] onJoints = { };
		string[] complyJoints = { head, chest, hip, lThigh, rThigh, lKnee, rKnee, lFoot, rFoot, lElbow, rElbow, lHand, rHand };

			
		IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

		foreach (Atom at in personAtoms)
		{
			currentAtom = at;
			bool isMale = currentAtom.GetComponentInChildren<DAZCharacter>().isMale;
			if (at.uid == "Auto_Load_Male_Person") isMale = true;

			if (isMale)
			{
				
				possessableJoints = new string[] { head, lHand, rHand }; 
				onJoints = new string[] {  };
				complyJoints = new string[] { };

				jointControls = currentAtom.GetComponentsInChildren<FreeControllerV3>(true);
				configureJoints();
				
				/*
				penisMidControl.canGrabPosition = false;
				penisMidControl.canGrabRotation = false;

				linkJoint(penisTip, testicles);

				Rigidbody rightHandCameraRig = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "RightHand");
				Rigidbody leftHandCameraRig = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "LeftHand");

				// (When leap motion is enabled, this will be the regular controllers)
				Rigidbody rightHandCameraRigAlt = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "RightHandAlternate");
				Rigidbody leftHandCameraRigAlt = SuperController.singleton.GetAtomByUid("[CameraRig]").rigidbodies.First(rb => rb.name == "LeftHandAlternate");

				// Possess hands
				SuperController.singleton.SelectModePossess();

				// Link Right Hand to Chest                      
				chestControl.SelectLinkToRigidbody(rightHandMotionController, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
				chestControl.currentPositionState = FreeControllerV3.PositionState.ParentLink;
				chestControl.currentRotationState = FreeControllerV3.RotationState.ParentLink;
				*/
			}
			else // Is Female
			{
				possessableJoints = new string[] { head, lHand, rHand }; 
				onJoints = new string[] {  };
				complyJoints = new string[] { };

				jointControls = currentAtom.GetComponentsInChildren<FreeControllerV3>(true);
				configureJoints();
			}
		}
	

		public void UnPossess()
		{		
			IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");
			
			foreach (Atom at in personAtoms)
			{
				currentAtom = at;
				bool isMale = currentAtom.GetComponentInChildren<DAZCharacter>().isMale;
				if (currentAtom.uid == "Auto_Load_Male_Person") isMale = true;
								
				jointControls = currentAtom.GetComponentsInChildren<FreeControllerV3>(true);
				configureJoints();
				
				
				chestControl.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
				
				rHandControl.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);				
				lHandControl.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);

				//SuperController.singleton.SelectModeOff();								
			}			
		}

		private void configureJoints()
		{
			foreach (var joint in jointControls)
			{
				if (joint == null || joint.followWhenOff == null || joint.control == null || joint.name == "control")
					continue;

				if (notInteractableJoints.Contains(joint.name))
				{
					joint.interactableInPlayMode = false;
					joint.canGrabPosition = false;
					joint.canGrabRotation = false;
				}
				else
					joint.interactableInPlayMode = true;

				if (possessableJoints.Contains(joint.name))
					joint.possessable = true;
				else
					joint.possessable = false;

				if (onJoints.Contains(joint.name))
				{
					joint.currentPositionState = FreeControllerV3.PositionState.On;
					joint.currentRotationState = FreeControllerV3.RotationState.On;
				}
				else if (complyJoints.Contains(joint.name))
				{
					joint.currentPositionState = FreeControllerV3.PositionState.Comply;
					joint.currentRotationState = FreeControllerV3.RotationState.Comply;
				}
				else
				{
					joint.currentPositionState = FreeControllerV3.PositionState.Off;
					joint.currentRotationState = FreeControllerV3.RotationState.Off;
				}

				joint.SelectLinkToRigidbody(null, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);

				switch (joint.name)
				{
					case "headControl":
						headControl = joint;
						break;
					case "chestControl":
						chestControl = joint;
						break;
					case "hipControl":
						hipControl = joint;
						break;
					case "lFootControl":
						lFootControl = joint;
						break;
					case "rFootControl":
						rFootControl = joint;
						break;
					case "lHandControl":
						lHandControl = joint;
						break;
					case "rHandControl":
						rHandControl = joint;
						break;
					case "lThighControl":
						lThighControl = joint;
						break;
					case "rThighControl":
						rThighControl = joint;
						break;
					case "lArmControl":
						lArmControl = joint;
						break;
					case "rArmControl":
						rArmControl = joint;
						break;
					case "lElbowControl":
						lElbowControl = joint;
						break;
					case "rElbowControl":
						rElbowControl = joint;
						break;
					case "lKneeControl":
						lKneeControl = joint;
						break;
					case "neckControl":
						neckControl = joint;
						break;
					case "abdomenControl":
						abdomenControl = joint;
						break;
					case "abdomen2Control":
						abdomen2Control = joint;
						break;
					case "pelvisControl":
						pelvisControl = joint;
						break;
					case "penisTipControl":
						penisTipControl = joint;
						break;
					case "penisMidControl":
						penisMidControl = joint;
						break;
					case "penisBaseControl":
						penisBaseControl = joint;
						break;
					case "testesControl":
						testesControl = joint;
						break;
				}
			}
		}

		private void linkJoint(string joint1, string joint2)
		{
			Rigidbody chestControl = currentAtom.rigidbodies.First(rb => rb.name == joint2);
			var jointControl = jointControls.First(joint => joint.name == joint1);

			jointControl.SelectLinkToRigidbody(chestControl, FreeControllerV3.SelectLinkState.PositionAndRotation, true, true);
			jointControl.currentPositionState = FreeControllerV3.PositionState.ParentLink;
			jointControl.currentRotationState = FreeControllerV3.RotationState.ParentLink;
		}
	}
}
