using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace MVRPlugin {
	public class Template : MVRScript {

		// IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
		// some reason
		private static Atom person;
        private static FreeControllerV3 lShoulderController;
        private static FreeControllerV3 rShoulderController;
        private static FreeControllerV3 lArmController;
        private static FreeControllerV3 rArmController;
        private static FreeControllerV3 lHandController;
        private static FreeControllerV3 rHandController;
        private static FreeControllerV3 abdomenController;
        private static FreeControllerV3 chestController;
        private static FreeControllerV3 lNippleController;
        private static FreeControllerV3 rNippleController;
        private static FreeControllerV3 neckController;
        private static FreeControllerV3 headController;
        private static FreeControllerV3 lHipController;
        private static FreeControllerV3 rHipController;
        private static FreeControllerV3 lFootController;
        private static FreeControllerV3 rFootController;
        private static FreeControllerV3 lToeController;
        private static FreeControllerV3 rToeController;
		
		protected JSONStorableBool uianimatehands;
		protected JSONStorableBool uigenericcol;
		protected JSONStorableFloat uimotionspeed;
		protected JSONStorableFloat uianimationspeed;
		protected JSONStorableFloat uidistancescale;
		protected JSONStorableFloat uiposeadjust;
		protected JSONStorableFloat uithumbadjust;
		protected JSONStorableFloat uigraspadjust;
		protected JSONStorableFloat uistraightenadjust;
		protected JSONStorableFloat uichopadjust;
		protected JSONStorableFloat uithumboffset;
		protected JSONStorableFloat uifingercurl;
		protected JSONStorableBool uidowalk;
		protected JSONStorableFloat uitoebaseangleoffset;
		protected JSONStorableFloat uitoemaxangle;
		protected JSONStorableFloat uitoeground;
		protected JSONStorableFloat uitoegroundclear;
		protected JSONStorableBool uiShowStats;
		
		
        private static DAZMorph morphLHandFist;
        private static float mLHandFistValue = 0.0f;
        private static float mLHandFistTarget = 0.0f;
        private static DAZMorph morphRHandFist;
        private static float mRHandFistValue = 0.0f;
        private static float mRHandFistTarget = 0.0f;

        private static DAZMorph morphLThumbGrasp;
        private static DAZMorph morphRThumbGrasp;
        private static DAZMorph morphLThumbFist;
        private static DAZMorph morphRThumbFist;
        private static DAZMorph morphLThumbInOut;
        private static DAZMorph morphRThumbInOut;
        private static DAZMorph morphLHandChop;

        private static DAZMorph morphLPinkyBend;
        private static DAZMorph morphRPinkyBend;
        private static DAZMorph morphLRingBend;
        private static DAZMorph morphRRingBend;
        private static DAZMorph morphLMiddleBend;
        private static DAZMorph morphRMiddleBend;
        private static DAZMorph morphLIndexBend;
        private static DAZMorph morphRIndexBend;


        private static float mLHandChopValue = 0.0f;
        private static float mLHandChopTarget = 0.0f;
        private static DAZMorph morphRHandChop;
        private static float mRHandChopValue = 0.0f;
        private static float mRHandChopTarget = 0.0f;
        private static DAZMorph morphLHandStraighten;
        private static float mLHandStraightenValue = 0.0f;
        private static float mLHandStraightenTarget = 0.0f;
        private static DAZMorph morphRHandStraighten;
        private static float mRHandStraightenValue = 0.0f;
        private static float mRHandStraightenTarget = 0.0f;
		private static float lHandCollisionTimer = 0.0f;
		private static float rHandCollisionTimer = 0.0f;
		private static float toeAmount = 0.0f;
		
		private static string lhanddebug = "";
		private static string rhanddebug = "";
		private static string lToeDebug = "";
		private static string rToeDebug = "";
		
		protected Vector3 lhandpos;
		protected Vector3 rhandpos;
		protected Vector3 lcurhandpos;
		protected Vector3 rcurhandpos;
		
		// IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
		// is called right after creation
		public override void Init() {
			try {
				// put init code in here
				SuperController.LogMessage("Template Loaded");
				person = containingAtom;
                headController = person.GetStorableByID("headControl") as FreeControllerV3;
                neckController = person.GetStorableByID("neckControl") as FreeControllerV3;
                chestController = person.GetStorableByID("chestControl") as FreeControllerV3;
                lNippleController = person.GetStorableByID("lNippleControl") as FreeControllerV3;
                rNippleController = person.GetStorableByID("rNippleControl") as FreeControllerV3;
                abdomenController = person.GetStorableByID("abdomen2Control") as FreeControllerV3;
                lShoulderController = person.GetStorableByID("lShoulderControl") as FreeControllerV3;
                rShoulderController = person.GetStorableByID("rShoulderControl") as FreeControllerV3;
                lArmController = person.GetStorableByID("lArmControl") as FreeControllerV3;
                rArmController = person.GetStorableByID("rArmControl") as FreeControllerV3;
                lHandController = person.GetStorableByID("lHandControl") as FreeControllerV3;
                rHandController = person.GetStorableByID("rHandControl") as FreeControllerV3;
                lHipController = person.GetStorableByID("lThighControl") as FreeControllerV3;
                rHipController = person.GetStorableByID("rThighControl") as FreeControllerV3;
                lFootController = person.GetStorableByID("lFootControl") as FreeControllerV3;
                rFootController = person.GetStorableByID("rFootControl") as FreeControllerV3;
                lToeController = person.GetStorableByID("lToeControl") as FreeControllerV3;
                rToeController = person.GetStorableByID("rToeControl") as FreeControllerV3;
				
				lhandpos = lcurhandpos;
				rhandpos = rHandController.followWhenOff.position;
				
                JSONStorable js = person.GetStorableByID("geometry");
                DAZCharacterSelector dcs = js as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;
				morphLHandFist = morphUI.GetMorphByDisplayName("Left Hand Grasp");
				morphRHandFist = morphUI.GetMorphByDisplayName("Right Hand Grasp");
				morphLThumbGrasp = morphUI.GetMorphByDisplayName("Left Thumb Bend");
				morphRThumbGrasp = morphUI.GetMorphByDisplayName("Right Thumb Bend");
				morphLThumbFist = morphUI.GetMorphByDisplayName("Left Thumb Fist");
				morphRThumbFist = morphUI.GetMorphByDisplayName("Right Thumb Fist");
				morphLThumbInOut = morphUI.GetMorphByDisplayName("Left Thumb In-Out");
				morphRThumbInOut = morphUI.GetMorphByDisplayName("Right Thumb In-Out");
				morphLHandChop = morphUI.GetMorphByDisplayName("Left Hand Chop");
				morphRHandChop = morphUI.GetMorphByDisplayName("Right Hand Chop");
				morphLHandStraighten = morphUI.GetMorphByDisplayName("Left Hand Straighten");
				morphRHandStraighten = morphUI.GetMorphByDisplayName("Right Hand Straighten");

				morphLPinkyBend = morphUI.GetMorphByDisplayName("Left Pinky Finger Bend");
				morphRPinkyBend = morphUI.GetMorphByDisplayName("Right Pinky Finger Bend");
				morphLRingBend = morphUI.GetMorphByDisplayName("Left Ring Finger Bend");
				morphRRingBend = morphUI.GetMorphByDisplayName("Right Ring Finger Bend");
				morphLMiddleBend = morphUI.GetMorphByDisplayName("Left Mid Finger Bend");
				morphRMiddleBend = morphUI.GetMorphByDisplayName("Right Mid Finger Bend");
				morphLIndexBend = morphUI.GetMorphByDisplayName("Left Index Finger Bend");
				morphRIndexBend = morphUI.GetMorphByDisplayName("Right Index Finger Bend");

				


			uianimatehands = new JSONStorableBool("Dynamically Adjust Hand Morphs", true);
			RegisterBool(uianimatehands);
			CreateToggle(uianimatehands, true);
			uigenericcol = new JSONStorableBool("flatten on any surface (buggy)", false);
			RegisterBool(uigenericcol);
			CreateToggle(uigenericcol, true);
			uiShowStats = new JSONStorableBool("Show stats on message log", false);
			RegisterBool(uiShowStats);
			CreateToggle(uiShowStats, true);
			uiposeadjust = new JSONStorableFloat("Static Pose Modifier", 0.4f, 0.0f, 2.0f, true, true);
			RegisterFloat(uiposeadjust);
			CreateSlider(uiposeadjust, true);
			uigraspadjust = new JSONStorableFloat("Grasp Morph Modifier", 1.0f, 0.0f, 2.0f, true, true);
			RegisterFloat(uigraspadjust);
			CreateSlider(uigraspadjust, true);
			uistraightenadjust = new JSONStorableFloat("Straighten Morph Modifier", 1.5f, 0.0f, 2.0f, true, true);
			RegisterFloat(uistraightenadjust);
			CreateSlider(uistraightenadjust, true);
			uichopadjust = new JSONStorableFloat("Chop Morph Modifier", 0.5f, 0.0f, 2.0f, true, true);
			RegisterFloat(uichopadjust);
			CreateSlider(uichopadjust, true);
			uithumbadjust = new JSONStorableFloat("Thumbs Morphs Modifier", 0.3f, 0.0f, 2.0f, true, true);
			RegisterFloat(uithumbadjust);
			CreateSlider(uithumbadjust, true);
			uithumboffset = new JSONStorableFloat("Thumbs Position Offset", 0.17f, -1.0f, 1.0f, true, true);
			RegisterFloat(uithumboffset);
			CreateSlider(uithumboffset, true);
			uimotionspeed = new JSONStorableFloat("Motion Speed Scale", 1.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uimotionspeed);
			CreateSlider(uimotionspeed, false);
			uidistancescale = new JSONStorableFloat("Distance Scale", 1.06f, 0.0f, 5.0f, true, true);
			RegisterFloat(uidistancescale);
			CreateSlider(uidistancescale, false);
			uianimationspeed = new JSONStorableFloat("Animation Speed", 2.0f, 0.0f, 5.0f, true, true);
			RegisterFloat(uianimationspeed);
			CreateSlider(uianimationspeed, false);
			uifingercurl = new JSONStorableFloat("Relaxed finger curl", 1.0f, -1.0f, 2.0f, true, true);
			RegisterFloat(uifingercurl);
			CreateSlider(uifingercurl, false);

			uitoeground = new JSONStorableFloat("Ground Level", 0.0f, -5.0f, 5.0f, true, true);
			RegisterFloat(uitoeground);
			CreateSlider(uitoeground, false);
			uitoegroundclear = new JSONStorableFloat("Ground Clearance", 0.1f, -5.0f, 5.0f, true, true);
			RegisterFloat(uitoegroundclear);
			CreateSlider(uitoegroundclear, false);
			uidowalk = new JSONStorableBool("Walk:Adjust Toes", true);
			RegisterBool(uidowalk);
			CreateToggle(uidowalk, false);
			uitoebaseangleoffset = new JSONStorableFloat("Walk:Foot Angle Offset", 10.0f, -90.0f, 90.0f, true, true);
			RegisterFloat(uitoebaseangleoffset);
			CreateSlider(uitoebaseangleoffset, false);
			uitoemaxangle = new JSONStorableFloat("Walk:Toe Max Up Angle", 30.0f, 0.0f, 90.0f, true, true);
			RegisterFloat(uitoemaxangle);
			CreateSlider(uitoemaxangle, false);

				// create custom JSON storable params here if you want them to be stored with scene JSON
				// types are JSONStorableFloat, JSONStorableBool, JSONStorableString, JSONStorableStringChooser
				// JSONStorableColor

			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Start is called once before Update or FixedUpdate is called and after Init()
		void Start() {
			try {
				// put code in here
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}


		// FixedUpdate is called with each physics simulation frame by Unity
		void Update() {
			try {
				// put code in here
				float minHeadDist = 0.08f * uidistancescale.val;
				float minChestDist = 0.2f * uidistancescale.val;
				float minNippleDist = 0.1f  * uidistancescale.val;
				float minAbsDist = 0.22f * uidistancescale.val;
				float minHipDist = 0.36f * uidistancescale.val;
				float minHandDist = 0.14f * uidistancescale.val;
				float minMotion = 0.2f;
				float maxStraighten = 1.0f;
				float maxFist = 1.0f;
				float maxChop = 1.0f;
				
				if (uianimatehands.val)
				{
					lhanddebug = "Left Hand : ";
					rhanddebug = "Right Hand : ";
					RaycastHit hit;
					float hitDist = 0.0f;
					bool hitdetected = false;

					lcurhandpos = lHandController.followWhenOff.TransformPoint(new Vector3(-0.1f, 0.0f, 0.0f));
					rcurhandpos = rHandController.followWhenOff.TransformPoint(new Vector3(0.1f, 0.0f, 0.0f));
					float lhipLDist = Vector3.Distance(lcurhandpos, lHipController.followWhenOff.position);
					float lhipRDist = Vector3.Distance(lcurhandpos, rHipController.followWhenOff.position);
					float lchestDist = Vector3.Distance(lcurhandpos, chestController.followWhenOff.position);
					float lnippleLDist = Vector3.Distance(lcurhandpos, lNippleController.followWhenOff.position);
					float lnippleRDist = Vector3.Distance(lcurhandpos, rNippleController.followWhenOff.position);
					float labsDist = Vector3.Distance(lcurhandpos, abdomenController.followWhenOff.position);
					float lheadDist = Vector3.Distance(lcurhandpos, headController.followWhenOff.position);
					float lrhandDist = Vector3.Distance(lcurhandpos, rcurhandpos);
					float lmoveDist = Vector3.Distance(lcurhandpos, lhandpos) * uimotionspeed.val;
					float lfloorDist = Mathf.Max(lcurhandpos.y, uitoeground.val);
					float lhandCurl = 1.0f;
					float lthumbOut = 0.0f;
					
					Vector3 lhanddir = chestController.followWhenOff.position - lcurhandpos;
					float ldotProduct = Vector3.Dot(lhanddir, chestController.followWhenOff.forward);
					
					if ((lchestDist > minChestDist && labsDist > minAbsDist && lheadDist > minHeadDist && lhipLDist > minHipDist && lhipRDist > minHipDist && lnippleLDist > minNippleDist && lnippleRDist > minNippleDist) || lmoveDist > minMotion)
					{
						if (lcurhandpos.y > neckController.followWhenOff.position.y)
						{
							if (lcurhandpos.y < headController.followWhenOff.position.y + 0.15f && lheadDist < 0.55f * uidistancescale.val)
							{
								mLHandStraightenTarget = 0.3f;
								mLHandFistTarget = maxFist * (Mathf.Clamp(lmoveDist*10.0f, 0.0f, 1.0f));
								lhanddebug += " ParHead";
							}
							else
							{
								mLHandStraightenTarget = Mathf.Lerp(0.3f, maxStraighten, Mathf.Clamp(lmoveDist*10.0f, 0.0f, 1.0f));
								mLHandFistTarget = 0.0f;
								lhanddebug += " AboveHead";
							}
						}
						else
						{
							if (lmoveDist > 0.02f)
							{
								mLHandStraightenTarget = maxStraighten;
								mLHandFistTarget = 0.0f;
								lhandCurl = 0.0f;
								lhanddebug += " FastMove";
							}
							else
							{
								mLHandStraightenTarget = 0.0f;
								mLHandFistTarget = maxFist * 0.2f * uiposeadjust.val;
								lhanddebug += " Slow";
							}
						}
						mLHandChopTarget = Mathf.Clamp(lmoveDist*33.0f, -0.2f, maxChop * 0.5f);
						
						if (lcurhandpos.y < lHipController.followWhenOff.position.y && lmoveDist > 0.02f)
						{
							mLHandStraightenTarget = maxStraighten;
							lhandCurl = 0.0f;
							mLHandChopTarget = -1.0f;
							lhanddebug += " Low";
						}
						
						if (lcurhandpos.y > chestController.followWhenOff.position.y && lmoveDist > 0.03f)
						{
							mLHandChopTarget = -0.5f;
							mLHandStraightenTarget = maxStraighten * 0.5f;
							mLHandFistTarget = 0.0f;
							lhanddebug += " ParChest";
						}
						if (lcurhandpos.y > chestController.followWhenOff.position.y && lcurhandpos.y < neckController.followWhenOff.position.y && lmoveDist < 0.0025f && lchestDist < 0.3f)
						{
							mLHandChopTarget = 0.0f;
							mLHandStraightenTarget = 0.0f;
							mLHandFistTarget = 0.7f * uiposeadjust.val;
							lhanddebug += " OnChest";
						}
						lhanddir = abdomenController.followWhenOff.position - lcurhandpos;
						ldotProduct = Vector3.Dot(lhanddir, abdomenController.followWhenOff.forward);
						if (ldotProduct > 0.3f)
						{
							mLHandStraightenTarget = 0.0f;
							mLHandFistTarget = 0.8f;
							mLHandChopTarget = 0.0f;
							lhanddebug += " AtBack";
						}
						
						if (lrhandDist < minHandDist)
						{
							mLHandStraightenTarget = maxStraighten;
							mLHandFistTarget = 0.0f;
							mLHandChopTarget = -1.0f;
							lhanddebug += " Hands";
						}
						if (uigenericcol.val)
						{
							if (lHandController.followWhenOffRB.SweepTest(-lHandController.followWhenOff.up, out hit, 0.1f))
							{
								hitdetected = true;
								if (lmoveDist > 0.001f)
								{
									hitDist = Mathf.Min(hit.distance, 0.08f);
								}
								//SuperController.singleton.ClearErrors();
								//SuperController.LogError("Collision " + hit.distance);
							}
							if (hitdetected || lHandCollisionTimer > 0.0f)
							{
								if (lHandCollisionTimer <= 0.0f)
								{
									lHandCollisionTimer = 0.5f;
								}
								else
								{
									lHandCollisionTimer -= Time.fixedDeltaTime;
								}
								
								mLHandStraightenTarget = maxStraighten * 0.8f * (1.0f - (hitDist * 100.0f));
								mLHandFistTarget = maxFist * 0.1f;
								mLHandChopTarget = -0.2f;
								lhanddebug += " Col";
							}
						}
						if (lfloorDist < uitoeground.val + uitoegroundclear.val)
						{
							mLHandStraightenTarget = Mathf.Lerp(maxStraighten, 0.0f, lfloorDist/(uitoeground.val + uitoegroundclear.val));
							mLHandFistTarget = 0.0f;
							mLHandChopTarget = -0.2f;
							lhanddebug += " Floor";
						}
					}
					else
					{
						lhanddir = abdomenController.followWhenOff.position - lcurhandpos;
						ldotProduct = Vector3.Dot(lhanddir, abdomenController.followWhenOff.forward);
						if (lhipLDist < minHipDist || lhipRDist < minHipDist)
						{
							if (ldotProduct > 0.1f)
							{
								mLHandStraightenTarget = 1.0f;
								mLHandFistTarget = 0.0f;
								mLHandChopTarget = 0.2f;
								lhanddebug += " AtButt";
							}
							else
							{
								mLHandStraightenTarget = maxStraighten * 0.6f;
								mLHandFistTarget = maxFist * 0.3f;
								mLHandChopTarget = -0.2f;
								lhanddebug += " AtHip";
							}
						}
						if (labsDist < minAbsDist)
						{
							lhanddir = abdomenController.followWhenOff.position - lcurhandpos;
							ldotProduct = Vector3.Dot(lhanddir, abdomenController.followWhenOff.forward);
							if (ldotProduct > 0.1f)
							{
								mLHandStraightenTarget = 0.0f;
								mLHandFistTarget = maxFist;
								mLHandChopTarget = 0.2f;
								lhanddebug += " BehindAbs";
							}
							else
							{
								mLHandStraightenTarget = maxStraighten * 0.8f;
								mLHandFistTarget = maxFist * 0.1f;
								mLHandChopTarget = 0.2f;
								lhanddebug += " AtAbs";
							}
						}
						if (lchestDist < minChestDist)
						{
							mLHandStraightenTarget = maxStraighten * 0.6f;
							mLHandFistTarget = maxFist * 0.3f;
							mLHandChopTarget = -0.5f;
							lhanddebug += " AtChest";
						}
						if (lnippleLDist < minNippleDist || lnippleRDist < minNippleDist)
						{
							mLHandStraightenTarget = 0.0f;
							mLHandFistTarget = maxFist * 0.2f;
							mLHandChopTarget = -0.3f;
							lhandCurl = 0.0f;
							lthumbOut = 0.2f;
							lhanddebug += " AtNipple";
						}
						lhanddir = headController.followWhenOff.position - lcurhandpos;
						ldotProduct = Vector3.Dot(lhanddir, headController.followWhenOff.forward);
						if (lheadDist < minHeadDist)
						{
							if (ldotProduct < -0.1f)
							{
								mLHandStraightenTarget = maxStraighten * 0.8f;
								mLHandFistTarget = maxFist * 0.1f;
								mLHandChopTarget = -0.7f;
								lhanddebug += " AtFace";
							}
							else
							{
								if (ldotProduct > 0.07f)
								{
									mLHandStraightenTarget = maxStraighten * 0.4f;
									mLHandFistTarget = maxFist * 0.3f;
									mLHandChopTarget = -0.1f;
									lhanddebug += " BehindHead";
								}
								else
								{
									mLHandStraightenTarget = maxStraighten * 0.3f;
									mLHandFistTarget = maxFist * 0.1f;
									mLHandChopTarget = 0.3f;
									lhanddebug += " SideHead";
								}
							}
						}
					}
					
					if (uidowalk.val)
					{
						Vector3 tempVector = new Vector3(0.0f,0.0f,0.0f);
						tempVector = lFootController.followWhenOff.position;
						float footangle = Vector3.Angle(lFootController.followWhenOff.forward, abdomenController.followWhenOff.forward);
						lToeController.jointRotationDriveXTarget = Mathf.Clamp(Mathf.Lerp(footangle - uitoebaseangleoffset.val, 0.0f, Mathf.Min(tempVector.y - uitoeground.val, uitoeground.val + uitoegroundclear.val) / (uitoeground.val + uitoegroundclear.val)),0.0f, uitoemaxangle.val);
						//footangle = Vector3.Angle(lFootController.followWhenOff.position - lHipController.followWhenOff.position, lHipController.followWhenOff.position);
						lToeDebug = Mathf.Lerp(footangle - uitoebaseangleoffset.val, 0.0f, Mathf.Min(tempVector.y - uitoeground.val, uitoeground.val + uitoegroundclear.val) / (uitoeground.val + uitoegroundclear.val)).ToString();

						tempVector = rFootController.followWhenOff.position;
						footangle = Vector3.Angle(rFootController.followWhenOff.forward, abdomenController.followWhenOff.forward);
						rToeController.jointRotationDriveXTarget = Mathf.Clamp(Mathf.Lerp(footangle - uitoebaseangleoffset.val, 0.0f, Mathf.Min(tempVector.y - uitoeground.val, uitoeground.val + uitoegroundclear.val) / (uitoeground.val + uitoegroundclear.val)),0.0f, uitoemaxangle.val);
						rToeDebug = Mathf.Lerp(footangle - uitoebaseangleoffset.val, 0.0f, Mathf.Min(tempVector.y - uitoeground.val, uitoeground.val + uitoegroundclear.val) / (uitoeground.val + uitoegroundclear.val)).ToString();
					}

					float rhipLDist = Vector3.Distance(rcurhandpos, lHipController.followWhenOff.position);
					float rhipRDist = Vector3.Distance(rcurhandpos, rHipController.followWhenOff.position);
					float rchestDist = Vector3.Distance(rcurhandpos, chestController.followWhenOff.position);
					float rnippleLDist = Vector3.Distance(rcurhandpos, lNippleController.followWhenOff.position);
					float rnippleRDist = Vector3.Distance(rcurhandpos, rNippleController.followWhenOff.position);
					float rabsDist = Vector3.Distance(rcurhandpos, abdomenController.followWhenOff.position);
					float rheadDist = Vector3.Distance(rcurhandpos, headController.followWhenOff.position);
					float rlhandDist = Vector3.Distance(lcurhandpos, rcurhandpos);
					float rmoveDist = Vector3.Distance(rcurhandpos, rhandpos) * uimotionspeed.val;
					float rfloorDist = Mathf.Max(lcurhandpos.y, uitoeground.val);
					float rhandCurl = 1.0f;
					float rthumbOut = 0.0f;
					hitdetected = false;
					
					Vector3 rhanddir = chestController.followWhenOff.position - rcurhandpos;
					float rdotProduct = Vector3.Dot(rhanddir, chestController.followWhenOff.forward);

					if ((rchestDist > minChestDist && rabsDist > minAbsDist && rheadDist > minHeadDist && rhipLDist > minHipDist && rhipRDist > minHipDist && rnippleLDist > minNippleDist && rnippleRDist > minNippleDist) || rmoveDist > minMotion)
					{
						if (rcurhandpos.y > neckController.followWhenOff.position.y)
						{
							if (rcurhandpos.y < headController.followWhenOff.position.y + 0.15f && rheadDist < 0.55f * uidistancescale.val)
							{
								mRHandStraightenTarget = 0.3f;
								mRHandFistTarget = maxFist * (Mathf.Clamp(rmoveDist*10.0f, 0.0f, 1.0f));
								rhanddebug += " ParHead";
							}
							else
							{
								mRHandStraightenTarget = Mathf.Lerp(0.3f, maxStraighten, Mathf.Clamp(rmoveDist*10.0f, 0.0f, 1.0f));
								mRHandFistTarget = 0.0f;
								rhanddebug += " AboveHead";
							}
						}
						else
						{
							if (rmoveDist > 0.02f)
							{
								mRHandStraightenTarget = maxStraighten;
								mRHandFistTarget = 0.0f;
								rhandCurl = 0.0f;
								rhanddebug += " FastMove";
							}
							else
							{
								mRHandStraightenTarget = 0.0f;
								mRHandFistTarget = maxFist * 0.2f * uiposeadjust.val;
								rhanddebug += " Slow";
							}
						}
						mRHandChopTarget = Mathf.Clamp(rmoveDist*33.0f, -0.2f, maxChop * 0.5f);
						
						if (rcurhandpos.y < rHipController.followWhenOff.position.y && rmoveDist > 0.02f)
						{
							mRHandStraightenTarget = maxStraighten;
							mRHandChopTarget = -1.0f;
							rhandCurl = 0.0f;
							rhanddebug += " Low";
						}
						if (rcurhandpos.y > chestController.followWhenOff.position.y && rmoveDist > 0.03f)
						{
							mRHandChopTarget = -0.5f;
							mRHandStraightenTarget = maxStraighten * 0.5f;
							mRHandFistTarget = 0.0f;
							rhanddebug += " ParChest";
						}
						if (rcurhandpos.y > chestController.followWhenOff.position.y && rcurhandpos.y < neckController.followWhenOff.position.y && rmoveDist < 0.0025f && rchestDist < 0.3f)
						{
							mRHandChopTarget = 0.0f;
							mRHandStraightenTarget = 0.0f;
							mRHandFistTarget = 0.7f * uiposeadjust.val;
							rhanddebug += " OnChest";
						}
						rhanddir = abdomenController.followWhenOff.position - rcurhandpos;
						rdotProduct = Vector3.Dot(rhanddir, abdomenController.followWhenOff.forward);
						if (rdotProduct > 0.3f)
						{
							mRHandStraightenTarget = 0.0f;
							mRHandFistTarget = 0.8f;
							mRHandChopTarget = 0.0f;
							rhanddebug += " AtBack";
						}
						
						if (rlhandDist < minHandDist)
						{
							mRHandStraightenTarget = maxStraighten;
							mRHandFistTarget = 0.0f;
							mRHandChopTarget = -1.0f;
							rhanddebug += " Hands";
						}
						if (uigenericcol.val)
						{
							if (rHandController.followWhenOffRB.SweepTest(-rHandController.followWhenOff.up, out hit, 0.1f))
							{
								hitdetected = true;
								if (rmoveDist > 0.001f)
								{
									hitDist = Mathf.Min(hit.distance, 0.08f);
								}
								//SuperController.singleton.ClearErrors();
								//SuperController.LogError("Collision " + hit.distance);
							}
							if (hitdetected || rHandCollisionTimer > 0.0f)
							{
								if (rHandCollisionTimer <= 0.0f)
								{
									rHandCollisionTimer = 0.5f;
								}
								else
								{
									rHandCollisionTimer -= Time.fixedDeltaTime;
								}
								
								mRHandStraightenTarget = maxStraighten * 0.8f * (1.0f - (hitDist * 100.0f));
								mRHandFistTarget = maxFist * 0.1f;
								mRHandChopTarget = -0.2f;
								rhanddebug += " Col";
							}
						}
						if (rfloorDist < uitoeground.val + uitoegroundclear.val)
						{
							mRHandStraightenTarget = Mathf.Lerp(maxStraighten, 0.0f, rfloorDist/(uitoeground.val + uitoegroundclear.val));
							mRHandFistTarget = 0.0f;
							mRHandChopTarget = -0.2f;
							rhanddebug += " Floor";
						}
					}
					else
					{
						rhanddir = abdomenController.followWhenOff.position - rcurhandpos;
						rdotProduct = Vector3.Dot(rhanddir, abdomenController.followWhenOff.forward);
						if (rhipLDist < minHipDist || rhipRDist < minHipDist)
						{
							if (rdotProduct > 0.1f)
							{
								mRHandStraightenTarget = 1.0f;
								mRHandFistTarget = 0.0f;
								mRHandChopTarget = 0.2f;
								rhanddebug += " AtButt";
							}
							else
							{
								mRHandStraightenTarget = maxStraighten * 0.6f;
								mRHandFistTarget = maxFist * 0.3f;
								mRHandChopTarget = -0.2f;
								rhanddebug += " AtHip";
							}
						}
						if (rabsDist < minAbsDist)
						{
							rhanddir = abdomenController.followWhenOff.position - rcurhandpos;
							rdotProduct = Vector3.Dot(rhanddir, abdomenController.followWhenOff.forward);
							if (rdotProduct > 0.1f)
							{
								mRHandStraightenTarget = 0.0f;
								mRHandFistTarget = maxFist;
								mRHandChopTarget = 0.2f;
								rhanddebug += " BehindAbs";
							}
							else
							{
								mRHandStraightenTarget = maxStraighten * 0.8f;
								mRHandFistTarget = maxFist * 0.1f;
								mRHandChopTarget = 0.2f;
								rhanddebug += " AtAbs";
							}
						}
						if (rchestDist < minChestDist)
						{
							mRHandStraightenTarget = maxStraighten * 0.6f;
							mRHandFistTarget = maxFist * 0.3f;
							mRHandChopTarget = -0.5f;
							rhanddebug += " AtChest";
						}
						if (rnippleLDist < minNippleDist || rnippleRDist < minNippleDist)
						{
							mRHandStraightenTarget = 0.0f;
							mRHandFistTarget = maxFist * 0.2f;
							mRHandChopTarget = -0.3f;
							rhandCurl = 0.0f;
							rthumbOut = 0.2f;
							rhanddebug += " AtNipple";
						}
						rhanddir = headController.followWhenOff.position - rcurhandpos;
						rdotProduct = Vector3.Dot(rhanddir, headController.followWhenOff.forward);
						if (rheadDist < minHeadDist)
						{
							if (rdotProduct < -0.1f)
							{
								mRHandStraightenTarget = maxStraighten * 0.8f;
								mRHandFistTarget = maxFist * 0.1f;
								mRHandChopTarget = -0.7f;
								rhanddebug += " AtFace";
							}
							else
							{
								if (rdotProduct > 0.07f)
								{
									mRHandStraightenTarget = maxStraighten * 0.4f;
									mRHandFistTarget = maxFist * 0.3f;
									mRHandChopTarget = -0.1f;
									rhanddebug += " BehindHead";
								}
								else
								{
									mRHandStraightenTarget = maxStraighten * 0.3f;
									mRHandFistTarget = maxFist * 0.1f;
									mRHandChopTarget = 0.3f;
									rhanddebug += " SideHead";
								}
							}
						}
					}


					if (mLHandStraightenTarget > mLHandStraightenValue) { mLHandStraightenValue = Mathf.Min(mLHandStraightenValue + (mLHandStraightenTarget - mLHandStraightenValue) / (15.0f / uianimationspeed.val), mLHandStraightenTarget); }
					if (mLHandStraightenTarget < mLHandStraightenValue) { mLHandStraightenValue = Mathf.Max(mLHandStraightenValue - (mLHandStraightenValue - mLHandStraightenTarget) / (15.0f / uianimationspeed.val), mLHandStraightenTarget); }
					morphLHandStraighten.SetValue(Round(mLHandStraightenValue) * uistraightenadjust.val);
					if (mRHandStraightenTarget > mRHandStraightenValue) { mRHandStraightenValue = Mathf.Min(mRHandStraightenValue + (mRHandStraightenTarget - mRHandStraightenValue) / (15.0f / uianimationspeed.val), mRHandStraightenTarget); }
					if (mRHandStraightenTarget < mRHandStraightenValue) { mRHandStraightenValue = Mathf.Max(mRHandStraightenValue - (mRHandStraightenValue - mRHandStraightenTarget) / (15.0f / uianimationspeed.val), mRHandStraightenTarget); }
					morphRHandStraighten.SetValue(Round(mRHandStraightenValue) * uistraightenadjust.val);

					if (mLHandFistTarget > mLHandFistValue) { mLHandFistValue = Mathf.Min(mLHandFistValue + (mLHandFistTarget - mLHandFistValue) / (15.0f / uianimationspeed.val), mLHandFistTarget); }
					if (mLHandFistTarget < mLHandFistValue) { mLHandFistValue = Mathf.Max(mLHandFistValue - (mLHandFistValue - mLHandFistTarget) / (15.0f / uianimationspeed.val), mLHandFistTarget); }
					morphLHandFist.SetValue(Round(mLHandFistValue) * uigraspadjust.val);
					if (mRHandFistTarget > mRHandFistValue) { mRHandFistValue = Mathf.Min(mRHandFistValue + (mRHandFistTarget - mRHandFistValue) / (15.0f / uianimationspeed.val), mRHandFistTarget); }
					if (mRHandFistTarget < mRHandFistValue) { mRHandFistValue = Mathf.Max(mRHandFistValue - (mRHandFistValue - mRHandFistTarget) / (15.0f / uianimationspeed.val), mRHandFistTarget); }
					morphRHandFist.SetValue(Round(mRHandFistValue) * uigraspadjust.val);

					if (mLHandChopTarget > mLHandChopValue) { mLHandChopValue = Mathf.Min(mLHandChopValue + (mLHandChopTarget - mLHandChopValue) / (15.0f / uianimationspeed.val), mLHandChopTarget); }
					if (mLHandChopTarget < mLHandChopValue) { mLHandChopValue = Mathf.Max(mLHandChopValue - (mLHandChopValue - mLHandChopTarget) / (15.0f / uianimationspeed.val), mLHandChopTarget); }
					morphLHandChop.SetValue(Round(mLHandChopValue) * uichopadjust.val);
					if (mRHandChopTarget > mRHandChopValue) { mRHandChopValue = Mathf.Min(mRHandChopValue + (mRHandChopTarget - mRHandChopValue) / (15.0f / uianimationspeed.val), mRHandChopTarget); }
					if (mRHandChopTarget < mRHandChopValue) { mRHandChopValue = Mathf.Max(mRHandChopValue - (mRHandChopValue - mRHandChopTarget) / (15.0f / uianimationspeed.val), mRHandChopTarget); }
					morphRHandChop.SetValue(Round(mRHandChopValue) * uichopadjust.val);
					
					morphLThumbGrasp.SetValue((Round(mLHandFistValue - mLHandStraightenValue) * uithumbadjust.val) - lthumbOut + uithumboffset.val);
					morphRThumbGrasp.SetValue((Round(mRHandFistValue - mRHandStraightenValue) * uithumbadjust.val) - rthumbOut + uithumboffset.val);
					morphLThumbFist.SetValue((Round(mLHandStraightenValue + mLHandFistValue) * uithumbadjust.val) - lthumbOut + uithumboffset.val);//Round(mLHandFistValue - (mLHandStraightenValue / 2.0f) + (Mathf.Max(-mLHandChopValue, 0.0f)), Mathf.Lerp(-0.5f, 0.0f, Round(lmoveDist*100.0f, 0.0f, 1.0f)), 1.0f));
					morphRThumbFist.SetValue((Round(mRHandStraightenValue + mRHandFistValue) * uithumbadjust.val) - rthumbOut + uithumboffset.val);//Round(mRHandFistValue - (mRHandStraightenValue / 2.0f) + (Mathf.Max(-mRHandChopValue, 0.0f)), Mathf.Lerp(-0.5f, 0.0f, Round(rmoveDist*100.0f, 0.0f, 1.0f)), 1.0f));
					morphLThumbInOut.SetValue(-uithumboffset.val * 3.0f);
					morphRThumbInOut.SetValue(-uithumboffset.val * 3.0f);
					float tempfloat;
					tempfloat = Round(Mathf.Clamp((1.0f - mLHandStraightenValue) * (1.0f - (lmoveDist * 33.0f)),0.0f,1.0f) * uifingercurl.val * lhandCurl);
					morphLIndexBend.SetValue(tempfloat * 0.1f);
					morphLMiddleBend.SetValue(tempfloat * 0.3f);
					morphLRingBend.SetValue(tempfloat * 0.45f);
					morphLPinkyBend.SetValue(tempfloat * 0.7f);
					
					tempfloat = Round(Mathf.Clamp((1.0f - mRHandStraightenValue) * (1.0f - (rmoveDist * 33.0f)),0.0f,1.0f) * uifingercurl.val * rhandCurl);
					morphRIndexBend.SetValue(tempfloat * 0.1f);
					morphRMiddleBend.SetValue(tempfloat * 0.3f);
					morphRRingBend.SetValue(tempfloat * 0.45f);
					morphRPinkyBend.SetValue(tempfloat * 0.7f);
					
					if (uiShowStats.val)
					{
						SuperController.singleton.ClearMessages();
						SuperController.LogMessage(lhanddebug, false);
						SuperController.LogMessage(rhanddebug, false);
						SuperController.LogMessage(lToeDebug, false);
						SuperController.LogMessage(rToeDebug, false);
					}
					
					
				}
				
				
				lhandpos = lcurhandpos;
				rhandpos = rcurhandpos;
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// OnDestroy is where you should put any cleanup
		// if you registered objects to supercontroller or atom, you should unregister them here
		void OnDestroy() {
		}
		
		public static float Round(float value)
		{
			return Mathf.Round(value * 100.0f) / 100.0f;
		}
	}
}