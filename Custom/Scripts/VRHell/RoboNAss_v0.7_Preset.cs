//VRHell *RoboNAss v0.6* Cycle drived by penis linear position
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
using System.IO.Ports; 

namespace RoboNAss
{
    public class RoboNAss : MVRScript
    {
		// Name of plugin
        public static string pluginName = "RoboNAss";
		
		
		//**EDIT THIS **
		private string USBPort = "COM10"; //-> Your OSR3 USB Port
		private float PenisToyOfset = 20f; //-> Your Penis tip Offset (relative to OSR max)
		private bool DisableToy = true; //-> Disable Toy connexion ( if you havent OSR toy)
		//***********
		
		//USB
		
		private string Baudrate = "9600";
		SerialPort USBCom;
		private float USBTimer = 1f;
		private float Period = 0.02f;
		protected UIDynamicButton ConnectToy;
		protected UIDynamicButton DisconnectToy;
		private JSONStorableBool AutoConnect;

        private Rigidbody pTip;
        private Rigidbody pBase;

		private float Result;
		private float Output;
		private Vector3 Angle;
		private float SpeedResult;
		
		private JSONStorableFloat UsableL;
		private JSONStorableFloat UsableLS;
		
		private float StockL = 90;
		private float StockLS = 60;
		
		public string Pos = "START";
		public string MessageBox = "Don't forget to edit the plugin file with your usb port. \nEnjoy\nVRHell";

		protected JSONStorableString MessageBoxLog;
		protected UIDynamicTextField positionWindow;
		

		private JSONStorableFloat Gain;
		private JSONStorableFloat Speed;
		private JSONStorableFloat RotGain;
		
		private JSONStorableFloat ToyGain;

		private float SinSt = 0;
		private float TimeSt = 0;

		
		protected Vector3 targetForce;
		protected Vector3 targetTorque;
		protected Vector3 currentForce;
		protected Vector3 currentTorque;
		
		private float ForceUp = 400;
		private float ForceDown = 400;
		private bool FirstCycle = true;
		private Rigidbody ForceReceiver;
		private Rigidbody BestCandidate;
		
		Dictionary<Rigidbody, Rigidbody> Candidates = new Dictionary<Rigidbody, Rigidbody>();
		
		//Rotations
		//forward - back rotation X
		private float ClampX = 0.60f;
		//Left-Right rotation Y
		private float ClampY = 0.60f;
		
		private float PresetTimer = -1f;

		
		private JSONStorableBool EditMode;
		private JSONStorableBool RandomPreset;
		protected JSONStorableStringChooser Preset;
		protected string CurrentPreset = "Preset_0";
		
		//Target Values
		private float T_UsableL = 90;
		private float T_UsableLS = 60;
		private float T_Gain =1;
		private float T_Speed =1;
		private float T_RotGain =1;
		private float T_ToyGain =1;
		
		HSVColor hsvc = HSVColorPicker.RGBToHSV(166f, 123f, 159f);
		//Preset0
		private JSONStorableFloat P0_UsableL;
		private JSONStorableFloat P0_UsableLS;
		private JSONStorableFloat P0_Gain;
		private JSONStorableFloat P0_Speed;
		private JSONStorableFloat P0_RotGain;
		private JSONStorableFloat P0_ToyGain;

		
		//Preset1
		private JSONStorableFloat P1_UsableL;
		private JSONStorableFloat P1_UsableLS;
		private JSONStorableFloat P1_Gain;
		private JSONStorableFloat P1_Speed;
		private JSONStorableFloat P1_RotGain;
		private JSONStorableFloat P1_ToyGain;

		
		//Preset2
		private JSONStorableFloat P2_UsableL;
		private JSONStorableFloat P2_UsableLS;
		private JSONStorableFloat P2_Gain;
		private JSONStorableFloat P2_Speed;
		private JSONStorableFloat P2_RotGain;
		private JSONStorableFloat P2_ToyGain;
		
		//Preset2
		private JSONStorableFloat P3_UsableL;
		private JSONStorableFloat P3_UsableLS;
		private JSONStorableFloat P3_Gain;
		private JSONStorableFloat P3_Speed;
		private JSONStorableFloat P3_RotGain;
		private JSONStorableFloat P3_ToyGain;

		
		protected UIDynamicButton ButtonSavePreset;
		protected UIDynamicButton ButtonLoadPreset;
		
		protected int CurrentIndex = 4;
		
		private void PresetChooserCallback(string P)
		{
			CurrentPreset = P;
			if (P == "Preset_0" && !EditMode.val) ApplyPreset(0);
			if (P == "Preset_1" && !EditMode.val) ApplyPreset(1);
			if (P == "Preset_2" && !EditMode.val) ApplyPreset(2);
			if (P == "Preset_3" && !EditMode.val) ApplyPreset(3);
		}
		
		public void SavePreset()
		{
			if( CurrentPreset == "Preset_0")
			{
				P0_UsableL.val = UsableL.val;
				P0_UsableLS.val = UsableLS.val;
				P0_Gain.val = Gain.val; 
				P0_Speed.val = Speed.val;
				P0_RotGain.val = RotGain.val;
				P0_ToyGain.val = ToyGain.val;

			}
			if( CurrentPreset == "Preset_1")
			{
				P1_UsableL.val = UsableL.val;
				P1_UsableLS.val = UsableLS.val;
				P1_Gain.val = Gain.val; 
				P1_Speed.val = Speed.val;
				P1_RotGain.val = RotGain.val;
				P1_ToyGain.val = ToyGain.val;
			}
			if( CurrentPreset == "Preset_2")
			{
				P2_UsableL.val = UsableL.val;
				P2_UsableLS.val = UsableLS.val;
				P2_Gain.val = Gain.val; 
				P2_Speed.val = Speed.val;
				P2_RotGain.val = RotGain.val;
				P2_ToyGain.val = ToyGain.val;
			}
			if( CurrentPreset == "Preset_3")
			{
				P3_UsableL.val = UsableL.val;
				P3_UsableLS.val = UsableLS.val;
				P3_Gain.val = Gain.val; 
				P3_Speed.val = Speed.val;
				P3_RotGain.val = RotGain.val;
				P3_ToyGain.val = ToyGain.val;
			}
		}
		
		public void LoadPreset()
		{
			if( CurrentPreset == "Preset_0")
			{
				UsableL.val = P0_UsableL.val;
				UsableLS.val = P0_UsableLS.val;
				Gain.val = P0_Gain.val; 
				Speed.val = P0_Speed.val;
				RotGain.val = P0_RotGain.val;
				ToyGain.val = P0_ToyGain.val;
			}
			if( CurrentPreset == "Preset_1")
			{
				UsableL.val = P1_UsableL.val;
				UsableLS.val = P1_UsableLS.val;
				Gain.val = P1_Gain.val; 
				Speed.val = P1_Speed.val;
				RotGain.val = P1_RotGain.val;
				ToyGain.val = P1_ToyGain.val;
			}
			if( CurrentPreset == "Preset_2")
			{
				UsableL.val = P2_UsableL.val;
				UsableLS.val = P2_UsableLS.val;
				Gain.val = P2_Gain.val; 
				Speed.val = P2_Speed.val;
				RotGain.val = P2_RotGain.val;
				ToyGain.val = P2_ToyGain.val;
			}
			if( CurrentPreset == "Preset_3")
			{
				UsableL.val = P3_UsableL.val;
				UsableLS.val = P3_UsableLS.val;
				Gain.val = P3_Gain.val; 
				Speed.val = P3_Speed.val;
				RotGain.val = P3_RotGain.val;
				ToyGain.val = P3_ToyGain.val;
			}
		}
		
		private void ApplyPreset(int Index)
		{
			if( Index == 0)
			{
				T_UsableL = P0_UsableL.val;
				T_UsableLS = P0_UsableLS.val;
				T_Gain = P0_Gain.val; 
				T_Speed = P0_Speed.val;
				T_RotGain = P0_RotGain.val;
				T_ToyGain = P0_ToyGain.val;
			}
			if( Index == 1)
			{
				T_UsableL = P1_UsableL.val;
				T_UsableLS = P1_UsableLS.val;
				T_Gain = P1_Gain.val; 
				T_Speed = P1_Speed.val;
				T_RotGain = P1_RotGain.val;
				T_ToyGain = P1_ToyGain.val;
			}
			if( Index == 2)
			{
				T_UsableL = P2_UsableL.val;
				T_UsableLS = P2_UsableLS.val;
				T_Gain = P2_Gain.val; 
				T_Speed = P2_Speed.val;
				T_RotGain = P2_RotGain.val;
				T_ToyGain = P2_ToyGain.val;
			}
			if( Index == 3)
			{
				T_UsableL = P3_UsableL.val;
				T_UsableLS = P3_UsableLS.val;
				T_Gain = P3_Gain.val; 
				T_Speed = P3_Speed.val;
				T_RotGain = P3_RotGain.val;
				T_ToyGain = P3_ToyGain.val;
			}
		}
		
		private void SmoothPresetTransition()
		{
			if (!EditMode.val)
			{
				float dT = Time.fixedDeltaTime;
				float SpeedT = 0.666f;
				
				UsableL.val = Mathf.Lerp(UsableL.val, T_UsableL, dT * SpeedT);
				UsableLS.val = Mathf.Lerp(UsableLS.val, T_UsableLS, dT * SpeedT);
				Gain.val = Mathf.Lerp(Gain.val, T_Gain, dT * SpeedT);
				Speed.val = Mathf.Lerp(Speed.val, T_Speed, dT * SpeedT);
				RotGain.val = Mathf.Lerp(RotGain.val, T_RotGain, dT * SpeedT);
				ToyGain.val = Mathf.Lerp(ToyGain.val, T_ToyGain, dT * SpeedT);
			}
		}
		
		private void PresetCycle()
		{

			PresetTimer -= Time.deltaTime;
			if (PresetTimer <= 0f && !EditMode.val) 
			{
				if (RandomPreset.val) CurrentIndex = UnityEngine.Random.Range(0,3);
				else CurrentIndex = (CurrentIndex + 1)%4;
				PresetTimer = UnityEngine.Random.Range(6,26);
				ApplyPreset(CurrentIndex);
			}
			
		}
		
		private void UpdateMinMax()
		{
			if (UsableLS.val > UsableL.val) UsableL.val = UsableLS.val+10f;
			StockL = UsableL.val * (1f + 0.1f * Mathf.Sin(TimeSt*0.09f + 108f) + 0.05f * Mathf.Sin(TimeSt*0.2f + 8f));
			StockLS = UsableLS.val * (1f + 0.15f * Mathf.Sin(TimeSt*0.2f + 52f) + 0.1f * Mathf.Sin(TimeSt*0.4f + 16f));
			StockL = Mathf.Clamp(StockL, 10f, 100f);
			StockLS = Mathf.Clamp(StockLS, 0f, 90f);
		}
		
		public void FindBest(Vector3 Target)
		{

			MakeList();
			float Distance = 99999f;
			
			foreach ( KeyValuePair<Rigidbody, Rigidbody> C in Candidates)
			{
				
				float TargetDistance = (C.Key.transform.position - Target).magnitude;
                if ( TargetDistance < Distance) 
				{
                    Distance = TargetDistance;
					BestCandidate = C.Key;
					ForceReceiver = C.Value;
                }
			}
		}
		
		public float RemapToyOutput(float In, float Ref)
		{


			float result = Ref - (Ref - In) * (1f - ToyGain.val);
			//float result = In;
			result = remap(result, 0f, 100f, PenisToyOfset, 100f);
			
			//MessageBoxLog.val = "Result  " + Result.ToString("00") + "\n";
			
			
			return result;
			
		}
				
		
		public void MakeList()
		{
			Candidates = new Dictionary<Rigidbody, Rigidbody>();

			foreach (Atom A in SuperController.singleton.GetAtoms()) 
			{
					if (A.type == "Person" && A != containingAtom)
					{
						Rigidbody Rb1, Rb2, Rb3, Rb4;
						if (A.rigidbodies.First(rb => rb.name == "LabiaTrigger")) //female test
						{
							Rb1 = A.rigidbodies.First(rb => rb.name == "LabiaTrigger");
							//Rb2 = A.rigidbodies.First(rb => rb.name == "hip");
							Rb2 = A.rigidbodies.First(rb => rb.name == "pelvis");
							Rb3 = A.rigidbodies.First(rb => rb.name == "LipTrigger");
							Rb4 = A.rigidbodies.First(rb => rb.name == "head");
							Candidates.Add(Rb1, Rb2);
							Candidates.Add(Rb3, Rb4);
						}
						
                    }
            }
			//SuperController.LogMessage(Candidates.Count.ToString());
		}
		
        public override void Init()
        {
            try
            {

			//Get male reference
			pBase = containingAtom.rigidbodies.First(rb => rb.name == "Gen1");
			pTip = containingAtom.rigidbodies.First(rb => rb.name == "Gen3");
			

			//UI SLIDER
			
			UsableLS = new JSONStorableFloat("Ride Start (penis percent)", 60f, 20f, 85f, true, true);
            RegisterFloat(UsableLS);
            CreateSlider(UsableLS, false);  
			
			UsableL = new JSONStorableFloat("Ride Stop (penis percent)", 95f, 20f, 100f, true, true);
            RegisterFloat(UsableL);
            CreateSlider(UsableL, true); 
			
			
			Gain = new JSONStorableFloat("Gain (Movement intencity)", 1f, 0f, 5f, true, true);
            RegisterFloat(Gain);
            CreateSlider(Gain, true); 

			
			Speed = new JSONStorableFloat("Speed", 1f, 0.1f, 3f, true, true);
            RegisterFloat(Speed);
            CreateSlider(Speed, false); 
			
			RotGain = new JSONStorableFloat("Rotation Gain (Rotation intencity)", 1f, 0f, 5f, true, true);
            RegisterFloat(RotGain);
            CreateSlider(RotGain, true); 
			
			ToyGain = new JSONStorableFloat("Maximise Toy amplitude", 0f, 0f, 1f, true, true);
            RegisterFloat(ToyGain);
            CreateSlider(ToyGain, false); 
			

			
						    // Serial buttons
            ConnectToy = CreateButton("Connect Toy", false);
            if (ConnectToy != null) ConnectToy.button.onClick.AddListener(Connect);
 
			
			DisconnectToy = CreateButton("Disconnect Toy", false);
            if (DisconnectToy != null) DisconnectToy.button.onClick.AddListener(Disconnect);

			AutoConnect = new JSONStorableBool("Auto connect Toy", false);
			RegisterBool(AutoConnect);
			CreateToggle(AutoConnect, false);
			
			List<string> PresetList = new List<string>();
            PresetList.Add("Preset_0");
            PresetList.Add("Preset_1");
			PresetList.Add("Preset_2");
			PresetList.Add("Preset_3");
			Preset = new JSONStorableStringChooser("Output Mode Chooser", PresetList, "Preset_0", "Select & Load Preset", PresetChooserCallback);
            UIDynamicPopup PresetPopup = CreatePopup(Preset, false);
			PresetPopup.labelWidth = 300f;
			
			EditMode = new JSONStorableBool("Edit mode", true);
			RegisterBool(EditMode);
			CreateToggle(EditMode, false);
			
			ButtonLoadPreset = CreateButton("Load selected preset", false);
            if (ButtonLoadPreset != null) ButtonLoadPreset.button.onClick.AddListener(LoadPreset);				
			
			ButtonSavePreset = CreateButton("Save selected preset", false);
            if (ButtonSavePreset != null) ButtonSavePreset.button.onClick.AddListener(SavePreset);			
			
			RandomPreset = new JSONStorableBool("Play random preset", true);
			RegisterBool(RandomPreset);
			CreateToggle(RandomPreset, false);
			
			UIDynamic spacer = CreateSpacer(true);
			spacer.height = 400f;
			
			P0_UsableLS = new JSONStorableFloat("P0_UsableLS", 60f, 20f, 85f, true, false);
            RegisterFloat(P0_UsableLS);
			CreateSlider(P0_UsableLS);
			
			P0_UsableL = new JSONStorableFloat("P0_UsableL", 95f, 20f, 100f, true, false);
            RegisterFloat(P0_UsableL);
			CreateSlider(P0_UsableL);
			
			P0_Speed= new JSONStorableFloat("P0_Speed", 1f, 0.1f, 3f, true, false);
            RegisterFloat(P0_Speed);
			CreateSlider(P0_Speed);
			
			P0_RotGain= new JSONStorableFloat("P0_RotGain", 1f, 0.1f, 3f, true, false);
            RegisterFloat(P0_RotGain);
			CreateSlider(P0_RotGain);

			P0_Gain = new JSONStorableFloat("P0_Gain", 1f, 0f, 5f, true, false);
            RegisterFloat(P0_Gain);
			CreateSlider(P0_Gain);
			
			P0_ToyGain= new JSONStorableFloat("P0_ToyGain", 0f, 0f, 1f, true, false);
            RegisterFloat(P0_ToyGain);
			CreateSlider(P0_ToyGain);

			P1_UsableLS = new JSONStorableFloat("P1_UsableLS", 60f, 20f, 85f, true, false);
            RegisterFloat(P1_UsableLS);
			CreateSlider(P1_UsableLS);
			
			P1_UsableL = new JSONStorableFloat("P1_UsableL", 95f, 20f, 100f, true, false);
            RegisterFloat(P1_UsableL);
			CreateSlider(P1_UsableL);
			
			P1_Speed = new JSONStorableFloat("P1_Speed", 1f, 0.1f, 3f, true, false);
            RegisterFloat(P1_Speed);
			CreateSlider(P1_Speed);
			
			P1_RotGain= new JSONStorableFloat("P1_RotGain", 1f, 0.1f, 3f, true, false);
            RegisterFloat(P1_RotGain);
			CreateSlider(P1_RotGain);
			
			P1_Gain = new JSONStorableFloat("P1_Gain", 1f, 0f, 5f, true, false);
            RegisterFloat(P1_Gain);
			CreateSlider(P1_Gain);
			
			P1_ToyGain= new JSONStorableFloat("P1_ToyGain", 0f, 0f, 1f, true, false);
            RegisterFloat(P1_ToyGain);
			CreateSlider(P1_ToyGain);
			
			P2_UsableLS = new JSONStorableFloat("P2_UsableLS", 60f, 20f, 85f, true, false);
            RegisterFloat(P2_UsableLS);
			CreateSlider(P2_UsableLS);
			
			P2_UsableL = new JSONStorableFloat("P2_UsableL", 95f, 20f, 100f, true, false);
            RegisterFloat(P2_UsableL);
			CreateSlider(P2_UsableL);
		
			P2_Gain = new JSONStorableFloat("P2_Gain", 1f, 0f, 5f, true, false);
            RegisterFloat(P2_Gain);
			CreateSlider(P2_Gain);
			
			P2_Speed = new JSONStorableFloat("P2_Speed", 1f, 0.1f, 3f, true, false);
            RegisterFloat(P2_Speed);
			CreateSlider(P2_Speed);
			
			P2_RotGain= new JSONStorableFloat("P2_RotGain", 1f, 0.1f, 3f, true, false);
            RegisterFloat(P2_RotGain);
			CreateSlider(P2_RotGain);
			
			P2_ToyGain = new JSONStorableFloat("P2_ToyGain", 0f, 0f, 1f, true, false);
            RegisterFloat(P2_ToyGain);
			CreateSlider(P2_ToyGain);
			
			P2_UsableLS = new JSONStorableFloat("P2_UsableLS", 60f, 20f, 85f, true, false);
            RegisterFloat(P2_UsableLS);
			CreateSlider(P2_UsableLS);
			
			P3_UsableL = new JSONStorableFloat("P3_UsableL", 95f, 20f, 100f, true, false);
            RegisterFloat(P3_UsableL);
			CreateSlider(P3_UsableL);
		
			P3_Gain = new JSONStorableFloat("P3_Gain", 1f, 0f, 5f, true, false);
            RegisterFloat(P3_Gain);
			CreateSlider(P3_Gain);
			
			P3_Speed = new JSONStorableFloat("P3_Speed", 1f, 0.1f, 3f, true, false);
            RegisterFloat(P3_Speed);
			CreateSlider(P3_Speed);
			
			P3_RotGain= new JSONStorableFloat("P3_RotGain", 1f, 0.1f, 3f, true, false);
            RegisterFloat(P3_RotGain);
			CreateSlider(P3_RotGain);
			
			P3_ToyGain = new JSONStorableFloat("P3_ToyGain", 0f, 0f, 1f, true, false);
            RegisterFloat(P3_ToyGain);
			CreateSlider(P3_ToyGain);
			
			//UI LOG
			
			MessageBoxLog = new JSONStorableString("Target Position", "");
			positionWindow = CreateTextField(MessageBoxLog, true);
			MessageBoxLog.val = MessageBox;
			

				
            }
			
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
		
		
		// game tick
		void Update()
		{
			
			UpdateMinMax();
			PresetCycle();
			SmoothPresetTransition();
			
			float NaturalLS = StockLS;
			float NaturalL = StockL; 
			
			NaturalLS = Mathf.Clamp(NaturalLS, 10f, 90f);
			NaturalL = Mathf.Clamp(NaturalL, NaturalLS + 10f, 100);
			FindBest(pBase.transform.position);
			if (BestCandidate == null || ForceReceiver == null) return;
			
			//Rotations
			
			if (BestCandidate.name == "LabiaTrigger") //Pelvis
			{
				Angle.y = Vector3.Dot(pBase.transform.right, BestCandidate.transform.up);//-->R L
				Angle.x = Vector3.Dot(pBase.transform.up, BestCandidate.transform.up);//--> F B
			}
			
			if (BestCandidate.name == "LipTrigger") //Head
			{
				Angle.y = -Vector3.Dot(pBase.transform.right, BestCandidate.transform.forward);//-->R L
				Angle.x = -Vector3.Dot(pBase.transform.up, BestCandidate.transform.forward);//--> F B
			}
			
			
			Angle.x = Mathf.Clamp(Angle.x, -ClampX, ClampX);
			Angle.x = remap(Angle.x, -ClampX, ClampX, 0f, 99f);
			
			Angle.y = Mathf.Clamp(Angle.y, -ClampY, ClampY);
			Angle.y = remap(Angle.y,-ClampY, ClampY, 0f, 99f);	
			
			//Linear
			float ajustL = 1.33f;
			float zobL = ajustL*(pTip.transform.position - pBase.transform.position).magnitude;
			float zobLajust = zobL * (NaturalL - NaturalLS)/100;
			Vector3 pBaseAjustPos = pBase.transform.position + pBase.transform.forward * (NaturalLS*zobL/100);
			Vector3 pTipAjustPos = pBase.transform.position + pBase.transform.forward * (NaturalL*zobL/100);


			float cosTeta = Vector3.Dot((pBase.transform.position - pBaseAjustPos).normalized, (BestCandidate.transform.position - pBaseAjustPos).normalized);
			float cosBeta = Vector3.Dot((pBaseAjustPos - pTipAjustPos).normalized, (BestCandidate.transform.position - pTipAjustPos).normalized);
			float cosGama = Vector3.Dot((pTip.transform.position - pBaseAjustPos).normalized, (BestCandidate.transform.position - pBaseAjustPos).normalized);
			float cosAlpha = Vector3.Dot((pTip.transform.position - pBase.transform.position).normalized, (BestCandidate.transform.position - pBase.transform.position).normalized);

			float VDistance = (BestCandidate.transform.position - pBaseAjustPos ).magnitude * cosGama;
			float VDistanceClamped = Mathf.Clamp(VDistance, 0f, zobLajust);

			float VToyDistance = (BestCandidate.transform.position - pBase.transform.position ).magnitude * cosAlpha;
			VToyDistance = Mathf.Clamp(VToyDistance, 0f, zobL);
			float ToyDistanceRatio = (1-VToyDistance/zobL)*100;
			
			float ForceDistanceRatio = (VDistance)/(zobL*ajustL);
			ForceDistanceRatio = 1f-Mathf.Clamp(ForceDistanceRatio, 0, 1);
			
			Result = 1f - (VDistanceClamped)/(zobLajust);
			Result = Mathf.Clamp(Result, 0, 1);
			Output = remap(Result, 0f, 1f, 0f, 100f);
			
			Angle.z = RemapToyOutput(ToyDistanceRatio, Output);
			
			// MessageBoxLog.val = "Toy  " + ToyDistanceRatio.ToString("00") + "\n";
			// MessageBoxLog.val += "Output  " + Output.ToString("00") + "\n";
			// MessageBoxLog.val += "Angle.z  " + Angle.z.ToString("00") + "\n";
			
			float txt = 1;
			TimeSt += Time.deltaTime;
			SinSt = Mathf.Sin(TimeSt*1f);

			float sinGain = Gain.val * (1 + 0.1f * Mathf.Sin(TimeSt*0.2f) + 0.1f * Mathf.Sin(TimeSt*0.12f));
			SpeedResult = Speed.val *(1 + 0.1f*Mathf.Sin(TimeSt*0.26f +12f) + 0.1f*Mathf.Sin(TimeSt*0.16f+ 8f));
			
			string State = "Start";
			//STATE
	
			if (BestCandidate.name == "LabiaTrigger")//Hips
			{


				
				if ( cosBeta < 0f )//Change direction
				{
					targetForce = -ForceDown * (1 + ForceDistanceRatio ) * pBase.transform.forward * sinGain;
					Pos = "DOWN";
				}
				
				else if (Pos == "START" || cosTeta > 0f)//Change direction
				{
					targetForce = ForceUp * (1 + ForceDistanceRatio ) *pBase.transform.forward * sinGain;
					Pos = "UP";
				}
				
				else if (VDistance > 4f*zobL)//Stop
				{
					targetForce = Vector3.zero;
					currentTorque = Vector3.zero;
					Pos = "WAIT";
				}
				
				currentTorque += ForceReceiver.transform.forward * (Mathf.Sin(TimeSt*0.42f)/2 + Mathf.Sin(TimeSt*0.64f+600)/3 + Mathf.Sin(TimeSt*0.72f+600)/2)*4.2f * RotGain.val;
				currentTorque += ForceReceiver.transform.right * (1+Mathf.Sin(TimeSt*0.12f)/2 + Mathf.Sin(TimeSt*0.24f+600)/4 )*3.8f * RotGain.val;
				//MessageBoxLog.val += "\n" + Output.ToString("00.00") + "\n" +  "\n"+ Pos;
			}
			
			if (BestCandidate.name == "LipTrigger")//Head
			{

				if (cosBeta < 0f )//Change direction
				{
					targetForce = -ForceDown * (1 + ForceDistanceRatio ) * pBase.transform.forward * sinGain;
					Pos = "DOWN";
				}
				
				else if (Pos == "START" ||  cosTeta > 0f)//Change direction
				{
					targetForce = ForceUp * pBase.transform.forward * sinGain;
					Pos = "UP";
				}
				
				else if (VDistance > 4f*zobL)
				{
					targetForce = Vector3.zero;
					currentTorque = Vector3.zero;
					Pos = "WAIT";
				}
				currentTorque += ForceReceiver.transform.forward * (Mathf.Sin(TimeSt*0.42f)/2 + Mathf.Sin(TimeSt*0.64f+600)/3 + Mathf.Sin(TimeSt*0.72f+600)/2)*6 * RotGain.val;
				currentTorque += ForceReceiver.transform.right * (-1+Mathf.Sin(TimeSt*0.12f)/2 + Mathf.Sin(TimeSt*0.24f+600)/4 )*0.8f * RotGain.val;

				//MessageBoxLog.val = "\n" + Output.ToString("00") + "\n" +  "\n"+ RemapToyOutput(Output).ToString("00") + "\n" ;
				
			}
			
            // TOY TICK
            if (USBCom != null && USBCom.IsOpen && DisableToy == false) 
			  {
				USBTimer -= Time.deltaTime;
					if (USBTimer <= 0f) 
					{
						USBTimer = Period;
						TickUSB(Angle);
					}
            }	
			if (AutoConnect.val == true)	Connect();

		}
		
		
		
		//remap function
		private float remap(float s, float a1, float a2, float b1, float b2)
		{
			float Result = b1 + (s-a1)*(b2-b1)/(a2-a1);
			return Result;
		}
		

		
		// physics tick
		void FixedUpdate() 
		{
			try {
				// apply forces here
				float timeFactor = Time.fixedDeltaTime;
				currentForce = Vector3.Lerp(currentForce, targetForce, timeFactor * SpeedResult);
				currentTorque = Vector3.Lerp(currentTorque, targetTorque, timeFactor*SpeedResult*12);
				 if (ForceReceiver && (!SuperController.singleton || !SuperController.singleton.freezeAnimation)) {

					 ForceReceiver.AddForce(currentForce, ForceMode.Force);
					 ForceReceiver.AddTorque(currentTorque, ForceMode.Force);

					//SuperController.LogMessage(targetForce.ToString());
				 }
			}
			catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}
				// Function to start serial
        void Connect() 
		{
			if (USBCom == null && DisableToy == false) 
			{
                // Open the serial connection
                USBCom = new SerialPort("\\\\.\\" + USBPort, Convert.ToInt32(Baudrate));
				SuperController.LogMessage("Open " + "\\\\.\\" + USBPort);
                USBCom.Open();
                USBCom.ReadTimeout = 10;
            }
        }
        // Function to stop serial
        void Disconnect() 
		{
            if(USBCom != null && USBCom.IsOpen) USBCom.Close();
        }
		
		// Function to send serial (from TempestVR pluggin)
        void TickUSB(Vector3 In) 
		{
            // Create T-code line to send 3 linear axis positions "L0", "L1" & "L2"
            string Message = "L" + In.z.ToString("00") + " L1" + In.x.ToString("00") + " L2" + In.y.ToString("00");
            USBCom.WriteLine(Message);
			//MessageBoxLog.val = Message;
        }
		
		void OnDestroy() 
		{
			Disconnect();
        }

    }

}