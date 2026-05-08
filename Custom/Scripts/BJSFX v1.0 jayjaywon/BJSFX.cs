using System;using UnityEngine;using System.Collections;using System.Collections.Generic;using SimpleJSON;using System.Linq;
namespace JayJayWon{

    public class BJSFX : MVRScript{
		const string pluginName = "BJSFX";const string pluginAuthor = "JayJayWon"; const string pluginVersion = "v1.0";
        public static string _vamFilePath = Application.dataPath.Substring(0,Application.dataPath.Length-8).Replace('/','\\');
		
		
        public override void Init() {
			try {
				_enableBreathing = new JSONStorableBool ("enableBreathing",true);
				_enableBreathing.storeType = JSONStorableParam.StoreType.Physical;	
				RegisterBool(_enableBreathing);
				UIDynamicToggle breathingToggle = CreateToggle(_enableBreathing, false);
				breathingToggle.label = "Enable Breathing SFX";
				
				JSONStorableString pluginVersionJSON = new JSONStorableString("PluginVersion","");
				UIDynamicTextField dtext = CreateTextField(pluginVersionJSON,false);
				
				pluginVersionJSON.val = pluginName +" "+pluginVersion + "\nby "+pluginAuthor;dtext.height = 1;
				
				if (containingAtom.type == "Person"){
					
				}
			}catch (Exception e) {SuperController.LogError("Exception caught in "+containingAtom.name+" "+this.storeId+": " + e);}
		}
		
		void LipContact(object sender, TriggerEventArgs eventArgs)
        {
			try {
				
				AudioSourceControl headAudioSource = containingAtom.GetStorableByID("HeadAudioSource") as AudioSourceControl;
				
				if (eventArgs.evtType=="Entered" )
				{
					if (!_lipTriggered && !_mouthTriggered && !_throatTriggered && enabledJSON.val && _clipsSuckStart.Count>0 ){
						NamedAudioClip clip = _clipsSuckStart[UnityEngine.Random.Range(0, _clipsSuckStart.Count)];
						headAudioSource.PlayNowClearQueue(clip);
					}
					_lipTriggered = true;
				}
				if (eventArgs.evtType=="Exited")
				{
					_lipTriggered = false;
				}				
			}catch (Exception e) {SuperController.LogError("Exception caught in "+containingAtom.name+" "+this.storeId+": " + e);}
		}
		
		void MouthContact(object sender, TriggerEventArgs eventArgs)
        {
			try {
				
				AudioSourceControl headAudioSource = containingAtom.GetStorableByID("HeadAudioSource") as AudioSourceControl;
				if (eventArgs.evtType=="Entered")
				{
					_mouthTriggered = true;
				}
				if (eventArgs.evtType=="Exited" )
				{
					if (!_throatTriggered && _mouthTriggered && enabledJSON.val && _clipsSuckEnd.Count>0){
						NamedAudioClip clip = _clipsSuckEnd[UnityEngine.Random.Range(0, _clipsSuckEnd.Count)];
						headAudioSource.PlayNowClearQueue(clip);
					}
					_mouthTriggered = false;
				}					
			}catch (Exception e) {SuperController.LogError("Exception caught in "+containingAtom.name+" "+this.storeId+": " + e);}
		}
		
		void ThroatContact(object sender, TriggerEventArgs eventArgs)
        {
			try {
				AudioSourceControl headAudioSource = containingAtom.GetStorableByID("HeadAudioSource") as AudioSourceControl;
				
				if (eventArgs.evtType=="Entered")
				{
					if (_timeSinceLastGag>1f){
						
						if (enabledJSON.val && _clipsGag.Count >0){
							NamedAudioClip clip = _clipsGag[UnityEngine.Random.Range(0, _clipsGag.Count)];
							headAudioSource.PlayNowClearQueue(clip);
						}
						_timeSinceLastGag = 0f;
					}
					_throatTriggered = true;
				}
				if (eventArgs.evtType=="Exited")
				{
					_throatTriggered = false;
				}					
			}catch (Exception e) {SuperController.LogError("Exception caught in "+containingAtom.name+" "+this.storeId+": " + e);}
		}

		void LoadClips (string path, List<NamedAudioClip> clipList)
		{
			string[] files = SuperController.singleton.GetFilesAtPath(SuperController.singleton.NormalizePath(path));
			foreach (string fileName in files)
			{
				if (fileName.Contains(".json"))
				{
					return;
				}

				if (!fileName.Contains(".mp3") && !fileName.Contains(".wav") && !fileName.Contains(".ogg"))
				{
					return;
				}
				clipList.Add(LoadAudio(fileName));				
			}			
		}
		
        NamedAudioClip LoadAudio(string path)
        {
            string localPath = SuperController.singleton.NormalizeLoadPath(path);
            NamedAudioClip existing = URLAudioClipManager.singleton.GetClip(localPath);
            if (existing != null)
            {
                return existing;
            }

            URLAudioClip clip = URLAudioClipManager.singleton.QueueClip(SuperController.singleton.NormalizeMediaPath(path));
            if (clip == null)
            {
                return null;
            }

            NamedAudioClip nac = URLAudioClipManager.singleton.GetClip(clip.uid);
            if (nac == null)
            {
                return null;
            }
            return nac;
        }
		
        string GetPluginPath()
        {
            string pluginId = this.storeId.Split('_')[0];
            string pathToScriptFile = this.manager.GetJSON(true, true)["plugins"][pluginId].Value;
            string pathToScriptFolder = pathToScriptFile.Substring(0, pathToScriptFile.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            pathToScriptFolder = pathToScriptFolder.Replace('/', '\\');
            return pathToScriptFolder;
        }		

		void Start() {
			try	{
				if (containingAtom.type == "Person"){
					
					Rigidbody lipRB = containingAtom.rigidbodies.First(rb => rb.name == "LipTrigger");
					_lipCollider = lipRB.gameObject.AddComponent<TriggerCollide>();
					_lipCollider.OnCollide += LipContact;
					Rigidbody mouthRB = containingAtom.rigidbodies.First(rb => rb.name == "MouthTrigger");
					_mouthCollider = mouthRB.gameObject.AddComponent<TriggerCollide>();
					_mouthCollider.OnCollide += MouthContact;
					Rigidbody throatRB = containingAtom.rigidbodies.First(rb => rb.name == "ThroatTrigger");
					_throatCollider = throatRB.gameObject.AddComponent<TriggerCollide>();
					_throatCollider.OnCollide += ThroatContact;
										
					LoadClips(GetPluginPath()+"\\BJSFXSound\\Suck Start",_clipsSuckStart);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\Suck End",_clipsSuckEnd);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\Hold",_clipsHold);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\Gag",_clipsGag);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\Breathing1",_clipsBreath1);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\Breathing2",_clipsBreath2);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\Breathing3",_clipsBreath3);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\FirstBreath1",_clipsInitialBreath1);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\FirstBreath2",_clipsInitialBreath2);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\FirstBreath3",_clipsInitialBreath3);
					LoadClips(GetPluginPath()+"\\BJSFXSound\\Swallow",_clipsSwallow);
				

					_loaded =true;
				}
				else{SuperController.LogMessage("BJSFX plugin must be loaded on a Person Atom");}

			}catch (Exception e) {SuperController.LogError("Exception caught in "+containingAtom.name+" "+this.storeId+": " + e);}
		}
		
		void OnDestroy() {
			try {
				if(_loaded){
					int bjsfxPluginCount =0 ;
					
					foreach (string atomName in SuperController.singleton.GetAtomUIDs()){						
						foreach (string receiverName in SuperController.singleton.GetAtomByUid(atomName).GetStorableIDs()){							
							if (receiverName.Length >4 && receiverName.Substring(receiverName.Length - 5, 5) == "BJSFX"){bjsfxPluginCount++;}						
						}
					}

					if (bjsfxPluginCount <1){
						foreach (NamedAudioClip clip in _clipsSuckStart){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}
						foreach (NamedAudioClip clip in _clipsSuckEnd){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}
						foreach (NamedAudioClip clip in _clipsHold){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}
						foreach (NamedAudioClip clip in _clipsGag){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}
						foreach (NamedAudioClip clip in _clipsBreath1){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}						
						foreach (NamedAudioClip clip in _clipsBreath2){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}
						foreach (NamedAudioClip clip in _clipsBreath3){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}
						foreach (NamedAudioClip clip in _clipsInitialBreath1){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}
						foreach (NamedAudioClip clip in _clipsInitialBreath2){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}
						foreach (NamedAudioClip clip in _clipsInitialBreath3){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}					
						foreach (NamedAudioClip clip in _clipsSwallow){
							URLAudioClipManager.singleton.RemoveClip(clip);
						}					

					}

					Destroy(containingAtom.rigidbodies.First(rb => rb.name == "LipTrigger").GetComponent<TriggerCollide>());
					Destroy(containingAtom.rigidbodies.First(rb => rb.name == "MouthTrigger").GetComponent<TriggerCollide>());
					Destroy(containingAtom.rigidbodies.First(rb => rb.name == "ThroatTrigger").GetComponent<TriggerCollide>());					
				}

			}catch (Exception e) {SuperController.LogError("Exception caught in "+containingAtom.name+" "+this.storeId+": " + e);}
		}


		void Update() {
			try {
				if(_loaded){
					AudioSourceControl headAudioSource = containingAtom.GetStorableByID("HeadAudioSource") as AudioSourceControl;
					NamedAudioClip clip = null;

					if (_mouthTriggered || _throatTriggered)
					{
						if (_clipsHold.Count>0){
							clip =_clipsHold[UnityEngine.Random.Range(0, _clipsHold.Count)];
							if (clip!=null){headAudioSource.PlayNextClearQueue(clip);}
						}
						if(_breathIntensity<1f){_breathIntensity = _breathIntensity +  (Time.unscaledDeltaTime/20f);}
						_initialBreath = false;
					}
					
					if (_throatTriggered)
					{
						_timeSinceLastGag = _timeSinceLastGag+Time.unscaledDeltaTime;
						
						if (_timeSinceLastGag >_timeToNextGag && _clipsGag.Count>0){
							clip = _clipsGag[UnityEngine.Random.Range(0, _clipsGag.Count)];
							if (clip!=null){headAudioSource.PlayNowClearQueue(clip);}							
							_timeSinceLastGag =0f;
							_timeToNextGag = UnityEngine.Random.Range(1f, 5f - UnityEngine.Random.Range(0,_breathIntensity*2f)-UnityEngine.Random.Range(0,_breathIntensity*2f));
						}
						if(_breathIntensity<1f){_breathIntensity = _breathIntensity +  (Time.unscaledDeltaTime/10f);}
					}

					if (_mouthTriggered & !_throatTriggered)
					{
						_timeSinceLastSwallow = _timeSinceLastSwallow+Time.unscaledDeltaTime;
						
						if (_timeSinceLastSwallow >_timeToNextSwallow && _clipsSwallow.Count>0){
							clip = _clipsSwallow[UnityEngine.Random.Range(0, _clipsSwallow.Count)];
							if (clip!=null){headAudioSource.PlayNowClearQueue(clip);}							
							_timeSinceLastSwallow =0f;
							_timeToNextSwallow = UnityEngine.Random.Range(5f, 8f);
						}
						if(_breathIntensity<1f){_breathIntensity = _breathIntensity +  (Time.unscaledDeltaTime/10f);}
					}
					
					if (!_lipTriggered && !_mouthTriggered && !_throatTriggered){
						if (_enableBreathing.val){
							if (!_initialBreath){
								if (_breathIntensity < 0.3f && _clipsInitialBreath1.Count>0){clip = _clipsInitialBreath1[UnityEngine.Random.Range(0, _clipsInitialBreath1.Count)];}
								else if (_breathIntensity < 0.6f && _clipsInitialBreath2.Count>0){clip = _clipsInitialBreath2[UnityEngine.Random.Range(0, _clipsInitialBreath2.Count)];}
								else if (_clipsInitialBreath3.Count>0){clip = _clipsInitialBreath3[UnityEngine.Random.Range(0, _clipsInitialBreath3.Count)];}
								if (headAudioSource.playingClip==null || !_clipsSuckEnd.Contains(headAudioSource.playingClip)) {
									headAudioSource.PlayNow(clip);
									_initialBreath = true;
								}
							}
							else {
								if (_breathIntensity < 0.3f && _clipsBreath1.Count>0){clip = _clipsBreath1[UnityEngine.Random.Range(0, _clipsBreath1.Count)];}
								else if (_breathIntensity < 0.6f && _clipsBreath2.Count>0){clip = _clipsBreath2[UnityEngine.Random.Range(0, _clipsBreath2.Count)];}
								else if (_clipsBreath3.Count>0){clip = _clipsBreath3[UnityEngine.Random.Range(0, _clipsBreath3.Count)];}
								if (clip!=null){headAudioSource.PlayNextClearQueue(clip);}
							}
						}
						if(_breathIntensity>0f){_breathIntensity = _breathIntensity -  (Time.unscaledDeltaTime/20f);}
					}					
				}

			}catch (Exception e) {SuperController.LogError("Exception caught in "+containingAtom.name+" "+this.storeId+": " + e);}
		}

		
		protected TriggerCollide _lipCollider;
		protected TriggerCollide _mouthCollider;
		protected TriggerCollide _throatCollider;
		
		JSONStorableBool _enableBreathing ;
		
		bool _lipTriggered =false;
		bool _mouthTriggered =false;
		bool _throatTriggered =false;
		bool _initialBreath = true ;
		
		float _timeSinceLastGag = 0f;
		float _timeSinceLastSwallow = 0f;

		float _timeToNextGag = 3f;
		float _timeToNextSwallow = 3f;
		float _breathIntensity = 0f;
		
		List<NamedAudioClip> _clipsSuckStart = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsSuckEnd = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsHold = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsGag = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsBreath1 = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsBreath2 = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsBreath3 = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsInitialBreath1 = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsInitialBreath2 = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsInitialBreath3 = new List<NamedAudioClip>();
		List<NamedAudioClip> _clipsSwallow = new List<NamedAudioClip>();
		
		bool _loaded = false;
	}
	
	public class TriggerEventArgs:EventArgs
    {
        public Collider collider { get; set; }
        public string evtType { get; set; }
    }

    public class TriggerCollide :MonoBehaviour
    {
        TriggerEventArgs lastEvent;

        public event EventHandler<TriggerEventArgs> OnCollide;

        void Awake()
        {
            lastEvent = new TriggerEventArgs
            {
                evtType = "none",
                collider = null
            };
        }

        private void OnTriggerEnter(Collider other)
        {
            DoCollideEvent("Entered", other);
        }

        private void OnTriggerExit(Collider other)
        {
            DoCollideEvent("Exited", other);
        }
        
        private void OnTriggerStay(Collider other)
        {
            DoCollideEvent("Stay", other);
        }

        private void DoCollideEvent(string evtType,Collider col)
        {
            if (string.Equals(evtType, lastEvent.evtType) && col.gameObject == lastEvent.collider.gameObject)
            {
                return;
            }
            else
            {
                TriggerEventArgs tempEvent = new TriggerEventArgs
                {
                    collider = col,
                    evtType = evtType
                };
                OnCollideEvent(tempEvent);
                lastEvent = tempEvent;
            }
        }

        protected virtual void OnCollideEvent(TriggerEventArgs e)
        {
            EventHandler<TriggerEventArgs> handler = OnCollide;
            handler?.Invoke(this, e);            
        }
	}

}
	
