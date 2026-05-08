using System;using UnityEngine;using System.Collections;using System.Collections.Generic;using SimpleJSON;using System.Linq;
namespace JayJayWon{
    public class PluginAssist : MVRScript{
        const string pluginName = "PluginAssist";const string pluginAuthor = "JayJayWon"; const string pluginVersion = "v1.0";
		const string tempFileName = ".PluginAssistTempSave.json";
		const string defaultPIPFileName = "default.vpip";
		public override void Init() {
			try {
				UIDynamicButton loadPIPButton = CreateButton("Load Plugin Profile (PIP)");
                loadPIPButton.button.onClick.AddListener(() =>{SuperController.singleton.GetMediaPathDialog(LoadPIP, "vpip",GetPluginPath(),false,true,false);});

				UIDynamicButton savePIPButton = CreateButton("Save Plugin Profile (PIP)");
                savePIPButton.button.onClick.AddListener(() =>{
					SuperController.singleton.NormalizeMediaPath(GetPluginPath());
					SuperController.singleton.GetMediaPathDialog(SavePIP, "vpip",GetPluginPath(),false,true,false);

                    // Update the browser to be a Save browser
                    uFileBrowser.FileBrowser browser = SuperController.singleton.mediaFileBrowserUI;
                    browser.SetTextEntry(true);
                    browser.fileEntryField.text = String.Format("{0}.{1}", ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString(), "vpip");
                    browser.ActivateFileNameField();
					});
				
				UIDynamicButton setAsDefeaultPIPButton = CreateButton("Set current PIP as Default");
                setAsDefeaultPIPButton.button.onClick.AddListener(() =>{SetAsDefaultPIP();});
				
				var spacer = CreateSpacer();spacer.height = 15;
				
				_atomSelector = new JSONStorableStringChooser("atomChooser", null, "All Atoms","Atom Selector");
				UIDynamicPopup atomSelecterPopup = CreateScrollablePopup(_atomSelector,false);
                atomSelecterPopup.popup.onOpenPopupHandlers += SyncAtomSelectorPopup;
                atomSelecterPopup.popupPanelHeight = 400f;
 
				UIDynamicButton applyProfileToAtoms = CreateButton("Apply PIP to Selection");
                applyProfileToAtoms.button.onClick.AddListener(() =>{ApplyPIP();});
				applyProfileToAtoms.buttonColor = Color.green;

				_replacePlugins = new JSONStorableBool("Replace existing Plugins",false);
				_replacePlugins.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_replacePlugins);
                UIDynamicToggle toggle = CreateToggle(_replacePlugins);								
				
				 spacer = CreateSpacer();spacer.height = 15;
				 
				_autoApplyPeople = new JSONStorableBool("Auto Apply: New People Atoms",false);
				_autoApplyPeople.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_autoApplyPeople);
                toggle = CreateToggle(_autoApplyPeople);
				 
				_autoApplyAP = new JSONStorableBool("Auto Apply: New Animation Patterns",false);
				_autoApplyAP.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_autoApplyAP);
                toggle = CreateToggle(_autoApplyAP);
				 
				_autoApplyAS = new JSONStorableBool("Auto Apply: New Animation Steps",false);
				_autoApplyAS.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_autoApplyAS);
                toggle = CreateToggle(_autoApplyAS);
				 
				_autoApplyForce = new JSONStorableBool("Auto Apply: New Force Atoms",false);
				_autoApplyForce.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_autoApplyForce);
                toggle = CreateToggle(_autoApplyForce);
				 
				_autoApplyLight = new JSONStorableBool("Auto Apply: New Light Atoms",false);
				_autoApplyLight.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_autoApplyLight);
                toggle = CreateToggle(_autoApplyLight);

				_autoApplyAudio = new JSONStorableBool("Auto Apply: New Audio Atoms",false);
				_autoApplyAudio.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_autoApplyAudio);
                toggle = CreateToggle(_autoApplyAudio);
				 
				_autoApplyCustomUA = new JSONStorableBool("Auto Apply: New Custom U Assets",false);
				_autoApplyCustomUA.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_autoApplyCustomUA);
                toggle = CreateToggle(_autoApplyCustomUA);
				 
				_autoApplyEmpty = new JSONStorableBool("Auto Apply: New Empty Atoms",false);
				_autoApplyEmpty.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_autoApplyEmpty);
                toggle = CreateToggle(_autoApplyEmpty);
				 
				_autoApplyTrigger = new JSONStorableBool("Auto Apply: New Trigger Atoms",false);
				_autoApplyTrigger.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_autoApplyTrigger);
                toggle = CreateToggle(_autoApplyTrigger);
					
				_pluginEntrySelector = new JSONStorableStringChooser("pluginSelector", null, "PIP Entry 1","Plugin Profile (PIP) Entry",SwitchPIPEntry);
				UIDynamicPopup pluginSelecterPopup = CreateScrollablePopup(_pluginEntrySelector,true);
                pluginSelecterPopup.popup.onOpenPopupHandlers += SyncPluginEntrySelectorPopup;
                pluginSelecterPopup.popupPanelHeight = 400f;				

				UIDynamicButton selectPluginFileButton = CreateButton("Select Plugin File",true);
                selectPluginFileButton.button.onClick.AddListener(() =>{ SuperController.singleton.GetMediaPathDialog(SelectPluginFile, "cs|cslist","Custom\\Scripts",false,true,false);});

				_uiPIPEntry = new PIPEntry (1);
				
				UIDynamicTextField pulginFileText = CreateTextField(_uiPIPEntry.pluginFileName,true);
				pulginFileText.height = 1;

				spacer = CreateSpacer(true);spacer.height = 20;
				
				UIDynamicButton selectPluginDataSourceButton = CreateButton("Select Plugin Data source Scene",true);
                selectPluginDataSourceButton.button.onClick.AddListener(() =>{SuperController.singleton.GetMediaPathDialog(SelectDataScene,"json","Saves\\scene",false,true,false);});

				_pluginDataSelector = new JSONStorableStringChooser("pluginDataSelector", null, "<SELECT SCENE>","Plugin Data Source");
				UIDynamicPopup pluginDataSelecterPopup = CreateScrollablePopup(_pluginDataSelector,true);
                pluginDataSelecterPopup.popup.onOpenPopupHandlers += SyncPluginDataSelectorPopup;
                pluginDataSelecterPopup.popupPanelHeight = 400f;
			
				UIDynamicButton applyDataButton = CreateButton("Import Plugin Data for Plugin Entry",true);
                applyDataButton.button.onClick.AddListener(() =>{ImportDataScene();});			

				UIDynamicTextField pulginDataText = CreateTextField(_uiPIPEntry.pluginData,true);
				pulginDataText.height = 1;
				
				spacer = CreateSpacer(true);spacer.height = 20;				

                toggle = CreateToggle(_uiPIPEntry.personLimiter,true);
				toggle.label = "Limit to People Atoms";
				toggle.toggle.onValueChanged.AddListener((bool val)=>{CommitUIPIP();});
				
                toggle = CreateToggle(_uiPIPEntry.apLimiter,true);
				toggle.label = "Limit to Animation Patterns";
				toggle.toggle.onValueChanged.AddListener((bool val)=>{CommitUIPIP();});
				 
                toggle = CreateToggle(_uiPIPEntry.asLimiter,true);
				toggle.label = "Limit to Animation Steps";
				toggle.toggle.onValueChanged.AddListener((bool val)=>{CommitUIPIP();});
				 
                toggle = CreateToggle(_uiPIPEntry.forceLimiter,true);
				toggle.label = "Limit to Force Atoms";
				toggle.toggle.onValueChanged.AddListener((bool val)=>{CommitUIPIP();});
				 
                toggle = CreateToggle(_uiPIPEntry.lightLimiter,true);
				toggle.label = "Limit to Light Atoms";
				toggle.toggle.onValueChanged.AddListener((bool val)=>{CommitUIPIP();});
				 
                toggle = CreateToggle(_uiPIPEntry.audioLimiter,true);
				toggle.label = "Limit to Audio Atoms";
				toggle.toggle.onValueChanged.AddListener((bool val)=>{CommitUIPIP();});
				 
                toggle = CreateToggle(_uiPIPEntry.customUALimiter,true);
				toggle.label = "Limit to Custom U Assets";
				toggle.toggle.onValueChanged.AddListener((bool val)=>{CommitUIPIP();});
				 
                toggle = CreateToggle(_uiPIPEntry.emptyAtomLimiter,true);
				toggle.label = "Limit to Empty Atoms";
				toggle.toggle.onValueChanged.AddListener((bool val)=>{CommitUIPIP();});
				 
                toggle = CreateToggle(_uiPIPEntry.triggerLimiter,true);
				toggle.label = "Limit to Trigger Atoms";
				toggle.toggle.onValueChanged.AddListener((bool val)=>{CommitUIPIP();});
				
				UIDynamicButton removePluginButton = CreateButton("Remove PIP Entry from Profile",true);
                removePluginButton.button.onClick.AddListener(() =>{RemovePluginEntry();});

				removePluginButton.buttonColor = Color.red;
				removePluginButton.textColor = Color.white;

				spacer = CreateSpacer(false);spacer.height = 10;
				JSONStorableString pluginVersionJSON = new JSONStorableString("PluginVersion","");UIDynamicTextField dtext = CreateTextField(pluginVersionJSON,false);pluginVersionJSON.val = pluginName +" "+pluginVersion + "\nby "+pluginAuthor;
				dtext.height = 1;
				
				_defaultLoaded = new JSONStorableBool ("defaultLoaded",false);
				_defaultLoaded.storeType = JSONStorableParam.StoreType.Physical;
				RegisterBool(_defaultLoaded);
				_pipEntryCount = new JSONStorableFloat("pipEntryCount", 0f, 0f, 10000f);
				_pipEntryCount.storeType = JSONStorableParam.StoreType.Physical;
				RegisterFloat(_pipEntryCount);
				SuperController.singleton.onAtomUIDRenameHandlers += new SuperController.OnAtomUIDRename(this.AtomNameUpdate);				
				SuperController.singleton.onAtomUIDsChangedHandlers += new SuperController.OnAtomUIDsChanged(this.AtomUIDChange);				
				
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
		
		protected PIPEntry _uiPIPEntry;
		protected JSONStorableFloat _pipEntryCount ;
		
		protected JSONStorableStringChooser _atomSelector ;
		protected JSONStorableStringChooser _pluginDataSelector ;
		protected JSONStorableStringChooser _pluginEntrySelector ;
		protected JSONStorableString _pluginDataScene ;
			
		protected JSONStorableBool _replacePlugins ;
		protected JSONStorableBool _defaultLoaded;
		
		protected JSONStorableBool _autoApplyPeople ;
		protected JSONStorableBool _autoApplyAP ;
		protected JSONStorableBool _autoApplyAS ;
		protected JSONStorableBool _autoApplyForce ;
		protected JSONStorableBool _autoApplyLight ;
		protected JSONStorableBool _autoApplyAudio ;
		protected JSONStorableBool _autoApplyCustomUA ;
		protected JSONStorableBool _autoApplyEmpty ;
		protected JSONStorableBool _autoApplyTrigger ;

        List<PIPEntry> _pipEntryList = new List<PIPEntry>();
		List<string> _pipEntryChoices = new List<string>();
		List<string> _atomNames = new List<string>();
        protected Dictionary<string, PIPEntry> _pipEntryDict = new Dictionary<string, PIPEntry>();
		
		List<PluginAtomLink> _pluginAtomList = new List<PluginAtomLink>();
		
		bool _switchingPIPEntries =false;
		bool _atomNameChangeWait = false;
		float _atomWaitTimeStart ;
		List<string> _pipApplyAtoms = new List<string>();
		
		protected JSONNode _sceneJSON ;

		protected void AtomUIDChange(List<string> atomUIDs){
			try{
				if (!SuperController.singleton.isLoading){
					if (_atomNameChangeWait==false){_pipApplyAtoms.Clear();}
					
					foreach (string atomName in atomUIDs){
						if(!_atomNames.Contains(atomName) && !_pipApplyAtoms.Contains(atomName)){						
							Atom atom = SuperController.singleton.GetAtomByUid(atomName);
							if (atom.type == "Person" && _autoApplyPeople.val==true){_pipApplyAtoms.Add (atomName);}
							if (atom.type == "AnimationPattern" && _autoApplyAP.val==true){_pipApplyAtoms.Add (atomName);}
							if (atom.type == "AnimationStep" && _autoApplyAS.val==true){_pipApplyAtoms.Add (atomName);}
							if (atom.category == "Force" && _autoApplyForce.val==true){_pipApplyAtoms.Add (atomName);}
							if (atom.category == "Light" && _autoApplyLight.val==true){_pipApplyAtoms.Add (atomName);}
							if (atom.category == "Audio" && _autoApplyAudio.val==true){_pipApplyAtoms.Add (atomName);;}
							if (atom.type == "CustomUnityAsset" && _autoApplyCustomUA.val==true){_pipApplyAtoms.Add (atomName);}
							if (atom.type == "Empty" && _autoApplyEmpty.val==true){_pipApplyAtoms.Add (atomName);}
							if (atom.category == "Trigger" && _autoApplyTrigger.val==true){_pipApplyAtoms.Add (atomName);}
						}
					}
					
					_atomNameChangeWait = true;
					_atomWaitTimeStart = Time.unscaledTime ;
				}

			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}

		protected void AtomNameUpdate(string oldName, string newName){
			try{
				if (_atomSelector.val == oldName){_atomSelector.val=newName;}
				
				_atomNames.Clear();
				foreach (string atomName in SuperController.singleton.GetAtomUIDs()){_atomNames.Add(atomName);}
				
				for (int listIndex =0;listIndex<_pipApplyAtoms.Count; listIndex++){
					if (_pipApplyAtoms.ElementAt(listIndex) == newName){_pipApplyAtoms.RemoveAt(listIndex);}
				}
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}	

		public void RemovePluginEntry(){
			try {
				PIPEntry currentPIPEntry ;
				PIPEntry previousPIPEntry ;
				List<PIPEntry> newPIPEntryList = new List<PIPEntry>();
				int deletedPIPEntryNumber = 0;
				if (_pipEntryCount.val==1f){CreateNewPIP(2);}
				_pipEntryDict.TryGetValue("PIP Entry 1", out previousPIPEntry) ;
				_pipEntryDict.TryGetValue(_pluginEntrySelector.val, out currentPIPEntry) ;
				deletedPIPEntryNumber = currentPIPEntry.pluginEntryID ;
				_pipEntryDict.Clear();
				_pipEntryChoices.Clear();
				_pipEntryDict = new Dictionary<string, PIPEntry>();
				foreach (PIPEntry pipEntry in _pipEntryList){
					if (pipEntry.pluginEntryID <= deletedPIPEntryNumber && pipEntry.pluginEntryID != _pipEntryCount.val){newPIPEntryList.Add(pipEntry);}
					else {
						if (pipEntry.pluginEntryID > deletedPIPEntryNumber){previousPIPEntry.CopyPIPEntry(pipEntry,false);}
						if (pipEntry.pluginEntryID == _pipEntryCount.val ){
							pipEntry.DeregisterJSON(this);
						}else {newPIPEntryList.Add(pipEntry);}
					}	
					previousPIPEntry = pipEntry;
				}
				_pipEntryList.Clear();
				foreach (PIPEntry pipEntry in newPIPEntryList){_pipEntryList.Add (pipEntry);_pipEntryChoices.Add ("PIP Entry "+pipEntry.pluginEntryID.ToString());_pipEntryDict.Add("PIP Entry "+pipEntry.pluginEntryID.ToString(),pipEntry);}				
				_pipEntryCount.val = _pipEntryCount.val - 1f;
				SyncPluginEntrySelectorPopup();
				if (deletedPIPEntryNumber>(int)_pipEntryCount.val){deletedPIPEntryNumber = (int) _pipEntryCount.val;_pluginEntrySelector.val="PIP Entry "+_pipEntryCount.val.ToString();}
				else{SwitchPIPEntry ("PIP Entry "+deletedPIPEntryNumber.ToString());}
				
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
		public void LoadPIP(string fileName){
			try {
				if (String.IsNullOrEmpty(fileName)){return;}
				JSONClass pipJSON = (JSONClass) SuperController.singleton.LoadJSON(fileName); 
						
				if (pipJSON ["Auto Apply: New People Atoms"].Value =="true") {_autoApplyPeople.val =true;}			
				if (pipJSON ["Auto Apply: New Animation Patterns"].Value =="true") {_autoApplyAP.val =true;}
				if (pipJSON ["Auto Apply: New Animation Steps"].Value =="true") {_autoApplyAS.val =true;}
				if (pipJSON ["Auto Apply: New Force Atoms"].Value =="true") {_autoApplyForce.val =true;}
				if (pipJSON ["Auto Apply: New Light Atoms"].Value =="true") {_autoApplyLight.val =true;}
				if (pipJSON ["Auto Apply: New Audio Atoms"].Value =="true") {_autoApplyAudio.val =true;}
				if (pipJSON ["Auto Apply: New Custom U Atoms"].Value =="true") {_autoApplyCustomUA.val =true;}
				if (pipJSON ["Auto Apply: New Empty Atoms"].Value =="true") {_autoApplyEmpty.val =true;}
				if (pipJSON ["Auto Apply: New Trigger Atoms"].Value =="true") {_autoApplyTrigger.val =true;}

				foreach (PIPEntry pipEntry in _pipEntryList){pipEntry.DeregisterJSON(this);}
				_pipEntryList.Clear();					
				_pipEntryDict.Clear();
				_pipEntryChoices.Clear();							
				_pipEntryCount.val = pipJSON ["pipEntryCount"].AsFloat;
				LoadPIPEntries(pipJSON);
				PIPEntry nextPIPEntry;
				_pipEntryDict.TryGetValue("PIP Entry 1", out nextPIPEntry) ;			
				_uiPIPEntry.CopyPIPEntry(nextPIPEntry);
				_pluginEntrySelector.val = "PIP Entry 1";			
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
		public void SavePIP(string fileName){
			try {
				if (String.IsNullOrEmpty(fileName)){return;}

				if ( !fileName.ToLower().EndsWith("vpip")){fileName += ".vpip";}
				JSONClass saveJson = this.GetJSON();
				this.SaveJSON(saveJson, fileName);
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
		public void SetAsDefaultPIP(){
			try {
				SavePIP(GetPluginPath()+defaultPIPFileName);
				
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}

		public void SyncAtomSelectorPopup(){
			try {
				List <string> atomChoices = new List <string>() ;
				atomChoices.Add("All Atoms");
				atomChoices.Add("All People Atoms");
				atomChoices.Add("All Animation Patterns");
				atomChoices.Add("All Animation Steps");
				atomChoices.Add("All Force Atoms");
				atomChoices.Add("All Light Atoms");
				atomChoices.Add("All Audio Atoms");
				atomChoices.Add("All Custom U Assets");
				atomChoices.Add("All Empty Atoms");
				atomChoices.Add("All Trigger Atoms");
				
				foreach (string atomName in SuperController.singleton.GetAtomUIDs()){if (atomName!="[CameraRig]" && atomName!="CoreControl"){atomChoices.Add(atomName);}}
				_atomSelector.choices = atomChoices;								
			
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
		public List<string> GetAtomsByType (string atomType){
			List<string> atomList = new List<string>();
			foreach (string atomName in SuperController.singleton.GetAtomUIDs()){
				if (atomName !="[CameraRig]" && atomName !="CoreControl"){
					Atom atom = SuperController.singleton.GetAtomByUid(atomName);
					if (atomType =="All Atoms" ||
						(atomType == "All People Atoms" && atom.category == "People") ||
						(atomType == "All Animation Patterns" && atom.type == "AnimationPattern") ||
						(atomType == "All Animation Steps" && atom.type == "AnimationStep") ||
						(atomType == "All Force Atoms" && atom.category == "Force") ||
						(atomType == "All Light Atoms" && atom.category == "Light") ||
						(atomType == "All Audio Atoms" && atom.category == "Sound") ||
						(atomType == "All Custom U Assets" && atom.type == "CustomUnityAsset") ||
						(atomType == "All Empty Atoms" && atom.type == "Empty") ||
						(atomType == "All Trigger Atoms" && atom.category == "Triggers")){atomList.Add(atomName);}
				}
			}
			return(atomList);		
		}
		public void CommitUIPIP(){
			try {
				if (!_switchingPIPEntries){
					PIPEntry currentPIPEntry;
					_pipEntryDict.TryGetValue("PIP Entry "+_uiPIPEntry.pluginEntryID.ToString(), out currentPIPEntry) ;				
					currentPIPEntry.CopyPIPEntry(_uiPIPEntry);
				}				
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
		public void ApplyPIP(List<string> atomNames = null){
			try {
				List<string> allAtomNames = SuperController.singleton.GetAtomUIDs();
				List<string> pipApplyAtoms = new List<string>();
				bool editModeOn;
				
				editModeOn = SuperController.singleton.editModeToggle.isOn;				
				CommitUIPIP();
				
				JSONClass sceneJSON = SuperController.singleton.GetSaveJSON(null,true, true);
				JSONClass mergeSceneJSON = new JSONClass ();
				bool mergeLoad = false;
				
				
				if (atomNames!=null){
					foreach (string atomName in atomNames){pipApplyAtoms.Add (atomName);}
				}
				else if (allAtomNames.Contains(_atomSelector.val)){
					pipApplyAtoms.Add(_atomSelector.val);
				}
				else {
					pipApplyAtoms = GetAtomsByType(_atomSelector.val);
				}
				foreach (JSONClass atomJSON in sceneJSON["atoms"].AsArray){
					bool mergeAtom = false;
					if (pipApplyAtoms.Contains(atomJSON["id"])){						
						JSONClass atomJSONCopy = new JSONClass();
						atomJSONCopy = atomJSON;
						int pluginMgrIndex = -1;
						int storablesIndex = 0;
						foreach (JSONClass storableJSON in atomJSONCopy["storables"].AsArray){
							if (storableJSON["id"].Value =="PluginManager"){pluginMgrIndex=storablesIndex; }
							storablesIndex++;
						}
						
						JSONClass emptyJSON = new JSONClass();
						if(_replacePlugins.val){atomJSONCopy["storables"][pluginMgrIndex]["plugins"]=emptyJSON;}
						foreach (PIPEntry pipEntry in _pipEntryList){
							if (!IsAtomFilteredByPIP(atomJSON["id"].Value,pipEntry)&&pipEntry.pluginFileName.val!="<NO PLUGIN>"){
								int nextFreePluginSlot = GetNextPluginSlot((JSONClass) atomJSONCopy["storables"][pluginMgrIndex]);
								atomJSONCopy["storables"][pluginMgrIndex]["plugins"]["plugin#"+nextFreePluginSlot.ToString()] = pipEntry.pluginFileName.val;
								mergeAtom = true;
								
								if (pipEntry.pluginData.val!="<NO PLUGIN DATA>"){
									JSONArray newPluginStorables = (JSONArray) JSON.Parse("["+pipEntry.pluginData.val+"]");
									
									foreach (JSONClass newPluginStorable in newPluginStorables){
										string pluginStorableID = newPluginStorable["id"].Value;
										int pluginIDEndIndex = pluginStorableID.IndexOf("_");
										newPluginStorable["id"].Value = "plugin#"+nextFreePluginSlot.ToString()+pluginStorableID.Substring(pluginIDEndIndex,pluginStorableID.Length-pluginIDEndIndex);									
										atomJSONCopy["storables"][-1] = newPluginStorable ;
									}
								}
							}
						}
						mergeSceneJSON["atoms"][-1] = atomJSONCopy;
						if (mergeAtom) {mergeLoad = true;}
					}				
				}

				if (mergeLoad){
					SuperController.singleton.SaveJSON(mergeSceneJSON,GetPluginPath()+tempFileName);
					SuperController.singleton.LoadMerge(GetPluginPath()+tempFileName);
					SuperController.singleton.editModeToggle.isOn = editModeOn;
				}
				
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
		public int GetNextPluginSlot (JSONClass pluginManagerJSON){
			int slotID =1000;
			List<int> usedPluginSlots = new List<int>();		
			if (pluginManagerJSON == null ) {slotID =0;}
			else{
				for (int slotIndex = 0; slotIndex<100; slotIndex++){
					if (pluginManagerJSON["plugins"]["plugin#"+slotIndex.ToString()]==null){
						if (slotIndex<slotID){slotID=slotIndex;}
					}
				}
			}
			return(slotID);
		}
		public bool IsAtomFilteredByPIP (string atomName, PIPEntry pipEntry){			
			Atom atom = SuperController.singleton.GetAtomByUid(atomName);
			
			if (atom.type == "Person" && pipEntry.personLimiter.val==true){return false;}
			if (atom.type == "AnimationPattern" && pipEntry.apLimiter.val==true){return false;}
			if (atom.type == "AnimationStep" && pipEntry.asLimiter.val==true){return false;}
			if (atom.category == "Force" && pipEntry.forceLimiter.val==true){return false;}
			if (atom.category == "Light" && pipEntry.lightLimiter.val==true){return false;}
			if (atom.category == "Audio" && pipEntry.audioLimiter.val==true){return false;}
			if (atom.type == "CustomUnityAsset" && pipEntry.customUALimiter.val==true){return false;}
			if (atom.type == "Empty" && pipEntry.emptyAtomLimiter.val==true){return false;}
			if (atom.category == "Trigger" && pipEntry.triggerLimiter.val==true){return false;}
				
			if (pipEntry.personLimiter.val==false && pipEntry.apLimiter.val==false && pipEntry.asLimiter.val==false && pipEntry.forceLimiter.val==false && pipEntry.lightLimiter.val==false && pipEntry.audioLimiter.val==false && pipEntry.customUALimiter.val==false && pipEntry.emptyAtomLimiter.val==false && pipEntry.triggerLimiter.val==false) {return (false);}
			
			return(true);
		}
		public void SelectDataScene(string sceneFileName){
			try {
				if (sceneFileName!=""){
					_sceneJSON = SuperController.singleton.LoadJSON(sceneFileName); 
					_pluginAtomList.Clear();
					
					foreach (JSONClass atomJSON in _sceneJSON["atoms"].AsArray){
						foreach (JSONClass storableJSON in atomJSON["storables"].AsArray){
							if (storableJSON["id"].Value =="PluginManager"){
								if (storableJSON["plugins"]!=null){
									for (int slotIndex = 0; slotIndex<100; slotIndex++){
										if (storableJSON["plugins"]["plugin#"+slotIndex.ToString()]!=null){
											PluginAtomLink link = new PluginAtomLink ();
											link.atomName = atomJSON["id"].Value;
											link.pluginID = "plugin#"+slotIndex.ToString();
											string pluginFilePath = storableJSON["plugins"]["plugin#"+slotIndex.ToString()].Value ;
											string targetFileName = pluginFilePath.Substring(pluginFilePath.LastIndexOfAny(new char[] { '/', '\\' })+1);
											link.pluginFileName = targetFileName ;
											_pluginAtomList.Add(link);
										}
									}
								}									
							}
						}
					}						
				}
				SyncPluginDataSelectorPopup();

			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}

		public void ImportDataScene(){
			try {					
				if (_pluginDataSelector.val != "<SELECT SCENE>" && _pluginDataSelector.val!="<PLUGIN NOT IN SCENE>" && _pluginDataSelector.val!="<SELECT PLUGIN>" && _sceneJSON !=null){
					int atomNameEndIndex = _pluginDataSelector.val.IndexOf(">");
					string atomName =_pluginDataSelector.val.Substring(0,atomNameEndIndex);
					string pluginName =_pluginDataSelector.val.Substring(atomNameEndIndex+1);
					int pluginNameLength = pluginName.Length;
					string pluginData = "";

					foreach (JSONClass atomJSON in _sceneJSON["atoms"].AsArray){
						if (atomJSON["id"].Value== atomName) {
							foreach (JSONClass storableJSON in atomJSON["storables"].AsArray){
								if (storableJSON["id"].Value.Length > pluginNameLength && storableJSON["id"].Value.Substring(0,pluginNameLength) ==pluginName){
									if (pluginData !=""){pluginData = pluginData + ",\n";}
									pluginData = pluginData +storableJSON.ToString();
								}
							}
						}
					}
					_uiPIPEntry.pluginData.val = pluginData;
					CommitUIPIP();
				}
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}

		public void SyncPluginDataSelectorPopup(){
			try {
				List<string> pluginChoices = new List<string>();
				string pluginFilename = _uiPIPEntry.pluginFileName.val.Substring(_uiPIPEntry.pluginFileName.val.LastIndexOfAny(new char[] { '/', '\\' })+1);
				if (_sceneJSON == null){pluginChoices.Add("<SELECT SCENE>");}
				else if (_uiPIPEntry.pluginFileName.val == "<NO PLUGIN>"){pluginChoices.Add("<SELECT PLUGIN>");}
				else{
					foreach (PluginAtomLink link in _pluginAtomList){
						if (link.pluginFileName == pluginFilename) {pluginChoices.Add(link.atomName+">"+link.pluginID);}
					}
				}
				if (pluginChoices.Count ==0){pluginChoices.Add("<PLUGIN NOT IN SCENE>");}
				
				_pluginDataSelector.choices = pluginChoices;
				if (!_pluginDataSelector.choices.Contains(_pluginDataSelector.val)){_pluginDataSelector.val = _pluginDataSelector.choices[0];}
				
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
		public void SyncPluginEntrySelectorPopup(){
			try {
				List <string> PIPchoices = new List <string>() ;
				foreach (string pip in _pipEntryChoices){PIPchoices.Add(pip);}
				PIPchoices.Add("ADD NEW PIP ENTRY");
				_pluginEntrySelector.choices = 	PIPchoices;								
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}		

		public void SelectPluginFile(string fileName){
			try {
				if(fileName!=""){
					string newPluginFilename = SuperController.singleton.NormalizeSavePath(fileName) ;
					if (newPluginFilename!=_uiPIPEntry.pluginFileName.val) {
						_uiPIPEntry.pluginFileName.val = SuperController.singleton.NormalizeSavePath(fileName) ;
						_uiPIPEntry.pluginData.val ="<NO PLUGIN DATA>";
						SyncPluginDataSelectorPopup();
						CommitUIPIP();						
					}
				}				
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}		


		public bool DefaultPIPFileExists(){
			string [] fileNamesArray = SuperController.singleton.GetFilesAtPath(GetPluginPath());
			List<string> fileNameList = new List<string>();
			foreach (string filePath in fileNamesArray){
				string fileName = filePath.Substring(filePath.LastIndexOfAny(new char[] { '/', '\\' })+1);
				fileNameList.Add(fileName);
			}
			
			if (fileNameList.Contains(defaultPIPFileName)){return (true);}
			return (false);
		}

		public void CreateNewPIP(int pipID,JSONClass savedJson =null ){
			PIPEntry newPIP = new PIPEntry (pipID);
			_pipEntryChoices.Add("PIP Entry "+pipID.ToString());
			_pipEntryDict.Add ("PIP Entry "+pipID.ToString(),newPIP);
			_pipEntryList.Add(newPIP);
			newPIP.RegisterJSON(this);
			if (savedJson !=null){newPIP.RestoreFromJSON(pipID,savedJson);}
			else{_pipEntryCount.val = _pipEntryCount.val +1f;}
		}
		public void SwitchPIPEntry (string pipEntryName){
			try {
				PIPEntry nextPIPEntry;
				
				if (pipEntryName=="ADD NEW PIP ENTRY"){
					CreateNewPIP((int)(_pipEntryCount.val+1f));
					_pluginEntrySelector.val = "PIP Entry "+_pipEntryCount.val.ToString();
				}
				else{
					_pipEntryDict.TryGetValue(pipEntryName, out nextPIPEntry) ;	
					_switchingPIPEntries = true;
					_uiPIPEntry.CopyPIPEntry(nextPIPEntry);
					_switchingPIPEntries = false;
				}
				SyncPluginDataSelectorPopup();
					
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
			
		}

        JSONClass GetPluginJsonFromSave(){
			foreach (JSONNode atom in SuperController.singleton.loadJson["atoms"].AsArray){
                if (atom["id"].Value == containingAtom.name){
                    foreach (JSONClass storable in atom["storables"].AsArray){
						if (storable["id"].Value == this.storeId){return storable;}}
                }
            }			

            return null;
        }		
		protected void LoadPIPEntries (JSONClass savedJson){
			for (int pipEntryIndex = 1; pipEntryIndex <= savedJson["pipEntryCount"].AsInt; pipEntryIndex++){
				CreateNewPIP(pipEntryIndex,savedJson);
			}
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
				if (!_defaultLoaded.val) {
					if (DefaultPIPFileExists()){LoadPIP(GetPluginPath()+defaultPIPFileName);}
					else{CreateNewPIP(1);}
					_defaultLoaded.val = true;
				}
				else {LoadPIPEntries(GetPluginJsonFromSave());}
				PIPEntry nextPIPEntry;
				_pipEntryDict.TryGetValue(_pluginEntrySelector.val, out nextPIPEntry) ;			
				_uiPIPEntry.CopyPIPEntry(nextPIPEntry);	
				
				foreach (string atomName in SuperController.singleton.GetAtomUIDs()){_atomNames.Add(atomName);}

			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
		
		void OnDestroy() {
				SuperController.singleton.onAtomUIDRenameHandlers -= new SuperController.OnAtomUIDRename(this.AtomNameUpdate);				
				SuperController.singleton.onAtomUIDsChangedHandlers -= new SuperController.OnAtomUIDsChanged(this.AtomUIDChange);				
			
		}
		
		void Update(){
			try {
				if (_atomNameChangeWait){
					if (Time.unscaledTime > _atomWaitTimeStart +0.1f){
						if (_pipApplyAtoms.Count>0){ApplyPIP(_pipApplyAtoms);}			
						_atomNames.Clear();
						foreach (string atomName in SuperController.singleton.GetAtomUIDs()){_atomNames.Add (atomName);}
						_atomNameChangeWait= false;
					}
				}
				
			}catch (Exception e) {SuperController.LogError("Exception caught: " + e);}
		}
	}

	public class PluginAtomLink{
		public string atomName;
		public string pluginID;
		public string pluginFileName;
	}
	
	public class PIPEntry{
        public PIPEntry(int pluginID){
			pluginEntryID=pluginID;
			pluginFileName = new JSONStorableString ("plugin"+pluginID.ToString()+":fileName","*?*");
			pluginFileName.val ="<NO PLUGIN>";
			pluginFileName.storeType = JSONStorableParam.StoreType.Physical;
			
			pluginData = new JSONStorableString ("plugin"+pluginID.ToString()+":data","*?*");
			pluginData.val ="<NO PLUGIN DATA>";
			pluginData.storeType = JSONStorableParam.StoreType.Physical;
			
			personLimiter = new JSONStorableBool ("plugin"+pluginID.ToString()+":personLimiter",false);
			personLimiter.storeType = JSONStorableParam.StoreType.Physical;
			
			apLimiter = new JSONStorableBool ("plugin"+pluginID.ToString()+":APLimiter",false);
			apLimiter.storeType = JSONStorableParam.StoreType.Physical;

			asLimiter = new JSONStorableBool ("plugin"+pluginID.ToString()+":ASLimiter",false);
			asLimiter.storeType = JSONStorableParam.StoreType.Physical;

			forceLimiter = new JSONStorableBool ("plugin"+pluginID.ToString()+":forceLimiter",false);
			forceLimiter.storeType = JSONStorableParam.StoreType.Physical;

			lightLimiter = new JSONStorableBool ("plugin"+pluginID.ToString()+":lightLimiter",false);
			lightLimiter.storeType = JSONStorableParam.StoreType.Physical;

			audioLimiter = new JSONStorableBool ("plugin"+pluginID.ToString()+":audioLimiter",false);
			audioLimiter.storeType = JSONStorableParam.StoreType.Physical;

			customUALimiter = new JSONStorableBool ("plugin"+pluginID.ToString()+":customUALimiter",false);
			customUALimiter.storeType = JSONStorableParam.StoreType.Physical;

			emptyAtomLimiter = new JSONStorableBool ("plugin"+pluginID.ToString()+":emptyAtomLimiter",false);
			emptyAtomLimiter.storeType = JSONStorableParam.StoreType.Physical;

			triggerLimiter = new JSONStorableBool ("plugin"+pluginID.ToString()+":triggerLimiter",false);
			triggerLimiter.storeType = JSONStorableParam.StoreType.Physical;
		}
		
		public void DeregisterJSON(PluginAssist parent){
			parent.DeregisterString(pluginFileName);
			parent.DeregisterString(pluginData);			
			parent.DeregisterBool(personLimiter);
			parent.DeregisterBool(apLimiter);
			parent.DeregisterBool(asLimiter);
			parent.DeregisterBool(forceLimiter);
			parent.DeregisterBool(lightLimiter);
			parent.DeregisterBool(audioLimiter);
			parent.DeregisterBool(customUALimiter);
			parent.DeregisterBool(emptyAtomLimiter);
			parent.DeregisterBool(triggerLimiter);
		}
		public void RegisterJSON(PluginAssist parent){
			parent.RegisterString(pluginFileName);
			parent.RegisterString(pluginData);			
			parent.RegisterBool(personLimiter);
			parent.RegisterBool(apLimiter);
			parent.RegisterBool(asLimiter);
			parent.RegisterBool(forceLimiter);
			parent.RegisterBool(lightLimiter);
			parent.RegisterBool(audioLimiter);
			parent.RegisterBool(customUALimiter);
			parent.RegisterBool(emptyAtomLimiter);
			parent.RegisterBool(triggerLimiter);
		}
		
		public void CopyPIPEntry(PIPEntry sourcePIP, bool updateID =true){
			if (updateID) {pluginEntryID = sourcePIP.pluginEntryID ;}
			pluginFileName.val = sourcePIP.pluginFileName.val ;			
			pluginData.val = sourcePIP.pluginData.val ;
			personLimiter.val = sourcePIP.personLimiter.val ;
			apLimiter.val = sourcePIP.apLimiter.val ;
			asLimiter.val = sourcePIP.asLimiter.val ;
			forceLimiter.val = sourcePIP.forceLimiter.val ;
			lightLimiter.val = sourcePIP.lightLimiter.val ;
			audioLimiter.val = sourcePIP.audioLimiter.val ;
			customUALimiter.val = sourcePIP.customUALimiter.val ;
			emptyAtomLimiter.val = sourcePIP.emptyAtomLimiter.val ;
			triggerLimiter.val = sourcePIP.triggerLimiter.val ;			
		}
		
		public void RestoreFromJSON(int index, JSONClass pluginSaveJSON){
			pluginFileName.val = pluginSaveJSON["plugin"+index.ToString()+":fileName"].Value ;			
			pluginData.val = pluginSaveJSON["plugin"+index.ToString()+":data"].Value ;		
			if (pluginSaveJSON["plugin"+index.ToString()+":personLimiter"].Value =="true"){personLimiter.val=true;}else{personLimiter.val=false;}
			if (pluginSaveJSON["plugin"+index.ToString()+":APLimiter"].Value =="true"){apLimiter.val=true;}else{apLimiter.val=false;}
			if (pluginSaveJSON["plugin"+index.ToString()+":ASLimiter"].Value =="true"){asLimiter.val=true;}else{asLimiter.val=false;}
			if (pluginSaveJSON["plugin"+index.ToString()+":forceLimiter"].Value =="true"){forceLimiter.val=true;}else{forceLimiter.val=false;}
			if (pluginSaveJSON["plugin"+index.ToString()+":lighLimiter"].Value =="true"){lightLimiter.val=true;}else{lightLimiter.val=false;}
			if (pluginSaveJSON["plugin"+index.ToString()+":audioLimiter"].Value =="true"){audioLimiter.val=true;}else{audioLimiter.val=false;}
			if (pluginSaveJSON["plugin"+index.ToString()+":customUALimiter"].Value =="true"){customUALimiter.val=true;}else{customUALimiter.val=false;}
			if (pluginSaveJSON["plugin"+index.ToString()+":emptyAtomLimiter"].Value =="true"){emptyAtomLimiter.val=true;}else{emptyAtomLimiter.val=false;}
			if (pluginSaveJSON["plugin"+index.ToString()+":triggerLimiter"].Value =="true"){triggerLimiter.val=true;}else{triggerLimiter.val=false;}					
		}
		public int pluginEntryID ;
		public JSONStorableString pluginFileName ;
		public JSONStorableString pluginData ;
		public JSONStorableBool personLimiter ;
		public JSONStorableBool apLimiter;
		public JSONStorableBool asLimiter;
		public JSONStorableBool forceLimiter;
		public JSONStorableBool lightLimiter;
		public JSONStorableBool audioLimiter;
		public JSONStorableBool customUALimiter;
		public JSONStorableBool emptyAtomLimiter;
		public JSONStorableBool triggerLimiter;
	}	
	

}