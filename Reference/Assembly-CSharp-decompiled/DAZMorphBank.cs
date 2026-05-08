using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MeshVR;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;

[ExecuteInEditMode]
public class DAZMorphBank : MonoBehaviour
{
	[SerializeField]
	protected DAZMesh _connectedMesh;

	public string autoImportFolder = string.Empty;

	public string autoImportFolderLegacy = string.Empty;

	public string conflictingFolder = string.Empty;

	public string geometryId;

	public DAZBones morphBones;

	public DAZBones morphBones2;

	public bool enableMCMMorphs = true;

	public bool bonesDirty;

	public Dictionary<DAZBone, bool> boneRotationsDirty;

	protected Dictionary<string, DAZMorphSubBank> _morphSubBanksByRegion;

	protected List<DAZMorph> _morphs;

	protected List<DAZMorph> _unactivatedMorphs;

	protected Dictionary<string, DAZMorph> _morphsByName;

	protected Dictionary<string, DAZMorph> _morphsByUid;

	protected Dictionary<string, DAZMorph> _morphsByDisplayName;

	protected bool demandActivatedMorphsDirty;

	protected Dictionary<string, string> _morphToRegionName;

	protected HashSet<string> badMorphNames;

	public bool updateEnabled = true;

	public bool useThreadedMorphing;

	protected DAZMorphTaskInfo applyMorphsTask;

	protected bool _threadsRunning;

	protected bool triggerThreadResetMorphs;

	protected bool threadedVerticesChanged;

	protected int[] threadedChangedVertices;

	protected int numThreadedChangedVertices;

	protected int numMaxThreadedChangedVertices;

	protected bool checkAllThreadedVertices;

	protected bool visibleThreadedVerticesChanged;

	public bool visibleVerticesChanged;

	protected Vector3[] _threadedMorphedUVVertices;

	protected Vector3[] _threadedVisibleMorphedUVVertices;

	public bool _threadedNormalsDirtyThisFrame;

	public bool _threadedTangentsDirtyThisFrame;

	protected List<DAZMorph> dirtyMorphs;

	protected int totalFrameCount;

	protected int missedFrameCount;

	protected bool wasInit;

	protected HashSet<string> currentLoadedMorphPackageUids;

	public DAZMesh connectedMesh
	{
		get
		{
			return _connectedMesh;
		}
		set
		{
			if (_connectedMesh != value)
			{
				_connectedMesh = value;
				ResetMorphs();
			}
		}
	}

	public List<DAZMorph> morphs => _morphs;

	public int numMorphs
	{
		get
		{
			if (_morphs == null)
			{
				return 0;
			}
			return _morphs.Count;
		}
	}

	protected void BuildMorphSubBanksByRegion()
	{
		if (_morphSubBanksByRegion == null)
		{
			_morphSubBanksByRegion = new Dictionary<string, DAZMorphSubBank>();
		}
		else
		{
			_morphSubBanksByRegion.Clear();
		}
		DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
		DAZMorphSubBank[] array = componentsInChildren;
		foreach (DAZMorphSubBank dAZMorphSubBank in array)
		{
			_morphSubBanksByRegion.Add(dAZMorphSubBank.name, dAZMorphSubBank);
		}
	}

	protected void BuildMorphsList()
	{
		if (_morphs == null)
		{
			_morphs = new List<DAZMorph>();
		}
		else
		{
			_morphs.Clear();
		}
		if (_unactivatedMorphs == null)
		{
			_unactivatedMorphs = new List<DAZMorph>();
		}
		else
		{
			_unactivatedMorphs.Clear();
		}
		foreach (DAZMorphSubBank value in _morphSubBanksByRegion.Values)
		{
			foreach (DAZMorph combinedMorph in value.combinedMorphs)
			{
				if (combinedMorph.isDemandLoaded && !combinedMorph.isDemandActivated)
				{
					_unactivatedMorphs.Add(combinedMorph);
				}
				else
				{
					_morphs.Add(combinedMorph);
				}
			}
		}
	}

	protected void BuildMorphsByNameAndUid()
	{
		if (_morphsByName == null)
		{
			_morphsByName = new Dictionary<string, DAZMorph>();
		}
		else
		{
			_morphsByName.Clear();
		}
		if (_morphsByUid == null)
		{
			_morphsByUid = new Dictionary<string, DAZMorph>();
		}
		else
		{
			_morphsByUid.Clear();
		}
		if (_morphsByDisplayName == null)
		{
			_morphsByDisplayName = new Dictionary<string, DAZMorph>();
		}
		else
		{
			_morphsByDisplayName.Clear();
		}
		foreach (DAZMorph morph in _morphs)
		{
			if (morph.disable)
			{
				continue;
			}
			DAZMorph value;
			if (!_morphsByUid.ContainsKey(morph.uid))
			{
				_morphsByUid.Add(morph.uid, morph);
			}
			else if (_morphsByUid.TryGetValue(morph.uid, out value))
			{
				Debug.LogError("Duplicate morph by uid " + morph.uid + " Region 1: " + value.region + " Region 2: " + morph.region);
			}
			if (!_morphsByName.ContainsKey(morph.morphName))
			{
				_morphsByName.Add(morph.morphName, morph);
			}
			if (morph.resolvedDisplayName != null)
			{
				if (!_morphsByDisplayName.ContainsKey(morph.resolvedDisplayName))
				{
					_morphsByDisplayName.Add(morph.resolvedDisplayName, morph);
				}
			}
			else
			{
				Debug.LogError("Morph " + morph.morphName + " has null display name");
			}
		}
		foreach (DAZMorph unactivatedMorph in _unactivatedMorphs)
		{
			if (unactivatedMorph.disable)
			{
				continue;
			}
			DAZMorph value2;
			if (!_morphsByUid.ContainsKey(unactivatedMorph.uid))
			{
				_morphsByUid.Add(unactivatedMorph.uid, unactivatedMorph);
			}
			else if (_morphsByUid.TryGetValue(unactivatedMorph.uid, out value2))
			{
				Debug.LogError("Duplicate morph by uid " + unactivatedMorph.uid + " Region 1: " + value2.region + " Region 2: " + unactivatedMorph.region);
			}
			if (!_morphsByName.ContainsKey(unactivatedMorph.morphName))
			{
				_morphsByName.Add(unactivatedMorph.morphName, unactivatedMorph);
			}
			if (unactivatedMorph.resolvedDisplayName != null)
			{
				if (!_morphsByDisplayName.ContainsKey(unactivatedMorph.resolvedDisplayName))
				{
					_morphsByDisplayName.Add(unactivatedMorph.resolvedDisplayName, unactivatedMorph);
				}
			}
			else
			{
				Debug.LogError("Morph " + unactivatedMorph.morphName + " has null display name");
			}
		}
	}

	protected void DemandActivateMorph(DAZMorph dm)
	{
		if (dm.isDemandLoaded && !dm.isDemandActivated)
		{
			dm.morphBank = this;
			dm.Init();
			dm.isDemandActivated = true;
			demandActivatedMorphsDirty = true;
		}
	}

	public bool CleanDemandActivatedMorphs()
	{
		if (demandActivatedMorphsDirty)
		{
			BuildMorphsList();
			demandActivatedMorphsDirty = false;
			return true;
		}
		return false;
	}

	public DAZMorph GetMorph(string morphName)
	{
		Init();
		if (_morphsByName.TryGetValue(morphName, out var value))
		{
			DemandActivateMorph(value);
			return value;
		}
		return null;
	}

	public DAZMorph GetMorphByUid(string uid)
	{
		Init();
		if (_morphsByUid.TryGetValue(uid, out var value))
		{
			DemandActivateMorph(value);
			return value;
		}
		return null;
	}

	public DAZMorph GetMorphByDisplayName(string morphDisplayName)
	{
		Init();
		if (_morphsByDisplayName.TryGetValue(morphDisplayName, out var value))
		{
			DemandActivateMorph(value);
			return value;
		}
		return null;
	}

	protected void BuildMorphToRegionName()
	{
		if (_morphToRegionName == null)
		{
			_morphToRegionName = new Dictionary<string, string>();
		}
		else
		{
			_morphToRegionName.Clear();
		}
		foreach (DAZMorphSubBank value2 in _morphSubBanksByRegion.Values)
		{
			foreach (DAZMorph combinedMorph in value2.combinedMorphs)
			{
				string value = ((combinedMorph.overrideRegion != null && combinedMorph.overrideRegion != string.Empty) ? combinedMorph.overrideRegion : ((!value2.useOverrideRegionName) ? combinedMorph.region : value2.overrideRegionName));
				if (!_morphToRegionName.ContainsKey(combinedMorph.morphName))
				{
					_morphToRegionName.Add(combinedMorph.morphName, value);
				}
			}
		}
	}

	public string GetMorphRegionName(string morphName)
	{
		Init();
		if (_morphToRegionName.TryGetValue(morphName, out var value))
		{
			return value;
		}
		return null;
	}

	public bool IsBadMorph(string name)
	{
		if (name == null)
		{
			return true;
		}
		if (badMorphNames != null && badMorphNames.Contains(name))
		{
			return true;
		}
		return false;
	}

	public bool AddMorphUsingSubBanks(DAZMorph dm)
	{
		if (badMorphNames == null)
		{
			badMorphNames = new HashSet<string>();
		}
		if (dm.numDeltas == 0 && dm.formulas.Length == 0)
		{
			if (dm.resolvedDisplayName != null && dm.resolvedDisplayName != string.Empty)
			{
				badMorphNames.Add(dm.resolvedDisplayName);
			}
			return false;
		}
		if (dm.min == dm.max)
		{
			if (dm.resolvedDisplayName != null && dm.resolvedDisplayName != string.Empty)
			{
				badMorphNames.Add(dm.resolvedDisplayName);
			}
			return false;
		}
		if (dm.resolvedDisplayName == null || dm.resolvedDisplayName == string.Empty)
		{
			return false;
		}
		if (_morphSubBanksByRegion == null)
		{
			BuildMorphSubBanksByRegion();
		}
		if (dm.region == null || dm.region == string.Empty)
		{
			dm.region = "NoRegion";
		}
		if (!_morphSubBanksByRegion.TryGetValue(dm.region, out var value))
		{
			GameObject gameObject = new GameObject(dm.region);
			gameObject.transform.parent = base.transform;
			value = gameObject.AddComponent<DAZMorphSubBank>();
			_morphSubBanksByRegion.Add(dm.region, value);
		}
		if (value != null)
		{
			value.AddMorph(dm);
			if (wasInit && Application.isPlaying)
			{
				dm.Init();
			}
		}
		return true;
	}

	protected void MoveOrDeleteFile(string oldFile, string newFile)
	{
		FileManager.SetFileAttributes(oldFile, FileAttributes.Normal);
		if (FileManager.FileExists(newFile, onlySystemFiles: true, restrictPath: true))
		{
			FileManager.DeleteFile(oldFile);
		}
		else
		{
			FileManager.MoveFile(oldFile, newFile);
		}
	}

	protected bool RuntimeImportMorph(FileEntry morphInfFile, bool isTransient = false, bool isDemandLoaded = false)
	{
		bool result = false;
		string path = morphInfFile.Path;
		string text = path.Replace(".vmi", ".vmb");
		string text2 = path.Replace(".vmi", ".dsf");
		FileEntry fileEntry = FileManager.GetFileEntry(text);
		if (fileEntry != null)
		{
			try
			{
				using FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(morphInfFile);
				string aJSON = fileEntryStreamReader.ReadToEnd();
				JSONNode jSONNode = JSON.Parse(aJSON);
				if (jSONNode != null)
				{
					DAZMorph dAZMorph = new DAZMorph();
					dAZMorph.morphBank = this;
					dAZMorph.LoadMetaFromJSON(jSONNode);
					dAZMorph.visible = true;
					dAZMorph.isRuntime = true;
					dAZMorph.isDemandLoaded = isDemandLoaded;
					if (morphInfFile is VarFileEntry)
					{
						VarFileEntry varFileEntry = morphInfFile as VarFileEntry;
						dAZMorph.isInPackage = true;
						dAZMorph.packageUid = varFileEntry.Package.Uid;
						dAZMorph.packageLicense = varFileEntry.Package.LicenseType;
						dAZMorph.isLatestVersion = varFileEntry.Package.isNewestEnabledVersion;
						dAZMorph.version = "v" + varFileEntry.Package.Version;
					}
					else
					{
						dAZMorph.isInPackage = false;
						dAZMorph.packageUid = null;
						dAZMorph.packageLicense = null;
						dAZMorph.isLatestVersion = true;
						dAZMorph.version = null;
					}
					dAZMorph.metaLoadPath = morphInfFile.Path;
					dAZMorph.isTransient = isTransient;
					dAZMorph.deltasLoadPath = fileEntry.Path;
					dAZMorph.uid = morphInfFile.Uid;
					if (isTransient)
					{
						DAZMorph morphByDisplayName = GetMorphByDisplayName(dAZMorph.resolvedDisplayName);
						if (morphByDisplayName == null)
						{
							result = true;
							AddMorphUsingSubBanks(dAZMorph);
						}
					}
					else
					{
						DAZMorph morphByUid = GetMorphByUid(dAZMorph.uid);
						if (morphByUid == null)
						{
							result = true;
							AddMorphUsingSubBanks(dAZMorph);
						}
					}
				}
				else if (SuperController.singleton != null)
				{
					SuperController.singleton.Error("Parse error while loading morph " + path);
				}
				else
				{
					Debug.LogError("Parse error while loading morph " + path);
				}
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during read of morph binary file " + text + " " + ex);
			}
		}
		else if (SuperController.singleton != null)
		{
			SuperController.singleton.Error("Missing morph deltas file " + text + ". Cannot import");
		}
		else
		{
			Debug.LogError("Missing morph deltas file " + text + ". Cannot import");
		}
		return result;
	}

	protected bool RuntimeImportFromDir(string path, bool isTransient = false, bool isDemandLoaded = false)
	{
		DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(path);
		return RuntimeImportFromDir(directoryEntry, isTransient, isDemandLoaded);
	}

	protected bool RuntimeImportFromDir(DirectoryEntry de, bool isTransient = false, bool isDemandLoaded = false)
	{
		bool result = false;
		if (de != null)
		{
			List<DirectoryEntry> subDirectories = de.SubDirectories;
			foreach (DirectoryEntry item in subDirectories)
			{
				if (RuntimeImportFromDir(item, isTransient, isDemandLoaded))
				{
					result = true;
				}
			}
			bool flag = true;
			if (de is SystemDirectoryEntry)
			{
				List<FileEntry> files = de.GetFiles("*.dsf");
				foreach (FileEntry item2 in files)
				{
					string path = item2.Path;
					string text = path.Replace(".dsf", string.Empty);
					string text2 = text + ".vmi";
					string path2 = text + ".vmb";
					if (FileManager.FileExists(text2, onlySystemFiles: true, restrictPath: true) && FileManager.FileExists(path2, onlySystemFiles: true, restrictPath: true))
					{
						continue;
					}
					Debug.Log("Compiling custom morphs from " + item2);
					JSONNode jSONNode = DAZImport.ReadJSON(path);
					if (!(jSONNode != null))
					{
						continue;
					}
					JSONNode jSONNode2 = jSONNode["modifier_library"];
					if (!(jSONNode2 != null))
					{
						continue;
					}
					int num = 0;
					foreach (JSONNode item3 in jSONNode2.AsArray)
					{
						num++;
						DAZMorph dAZMorph = new DAZMorph();
						dAZMorph.Import(item3);
						string text3 = ((num != 1) ? (text + "_" + num + ".vmi") : text2);
						string path3 = text3.Replace(".vmi", ".vmb");
						bool flag2 = false;
						try
						{
							using (StreamWriter streamWriter = FileManager.OpenStreamWriter(text3))
							{
								JSONClass metaJSON = dAZMorph.GetMetaJSON();
								if (metaJSON != null)
								{
									flag2 = true;
									StringBuilder stringBuilder = new StringBuilder(10000);
									metaJSON.ToString(string.Empty, stringBuilder);
									streamWriter.Write(stringBuilder.ToString());
									dAZMorph.SaveDeltasToBinaryFile(path3);
								}
							}
							if (!flag2)
							{
								FileManager.DeleteFile(text3);
							}
						}
						catch (Exception ex)
						{
							Debug.LogError(string.Concat("Error during compile of morph ", item2, " ", ex));
						}
					}
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				List<FileEntry> files2 = de.GetFiles("*.vmi");
				foreach (FileEntry item4 in files2)
				{
					if (RuntimeImportMorph(item4, isTransient, isDemandLoaded))
					{
						result = true;
					}
				}
			}
		}
		return result;
	}

	public bool LoadTransientMorphs(string dir)
	{
		DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
		bool flag = false;
		DAZMorphSubBank[] array = componentsInChildren;
		foreach (DAZMorphSubBank dAZMorphSubBank in array)
		{
			if (dAZMorphSubBank.ClearTransientMorphs())
			{
				flag = true;
			}
		}
		if (flag)
		{
			RebuildAllLookups();
		}
		bool flag2 = false;
		if (autoImportFolder != null && autoImportFolder != string.Empty)
		{
			string path = dir + "/" + autoImportFolder;
			if (RuntimeImportFromDir(path, isTransient: true))
			{
				componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
				DAZMorphSubBank[] array2 = componentsInChildren;
				foreach (DAZMorphSubBank dAZMorphSubBank2 in array2)
				{
					dAZMorphSubBank2.CompleteTransientMorphAdd();
				}
				flag2 = true;
			}
		}
		if (autoImportFolderLegacy != null && autoImportFolderLegacy != string.Empty)
		{
			string path2 = dir + "/" + autoImportFolderLegacy;
			if (RuntimeImportFromDir(path2, isTransient: true))
			{
				componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
				DAZMorphSubBank[] array3 = componentsInChildren;
				foreach (DAZMorphSubBank dAZMorphSubBank3 in array3)
				{
					dAZMorphSubBank3.CompleteTransientMorphAdd();
				}
				flag2 = true;
			}
		}
		if (flag2)
		{
			RebuildAllLookups();
		}
		if (flag || flag2)
		{
			return true;
		}
		return false;
	}

	public void ClearPackageMorphs()
	{
		DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
		bool flag = false;
		DAZMorphSubBank[] array = componentsInChildren;
		foreach (DAZMorphSubBank dAZMorphSubBank in array)
		{
			if (dAZMorphSubBank.ClearPackageMorphs())
			{
				flag = true;
			}
		}
		if (flag)
		{
			RebuildAllLookups();
		}
	}

	public void ClearTransientMorphs()
	{
		DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
		bool flag = false;
		DAZMorphSubBank[] array = componentsInChildren;
		foreach (DAZMorphSubBank dAZMorphSubBank in array)
		{
			if (dAZMorphSubBank.ClearTransientMorphs())
			{
				flag = true;
			}
		}
		if (flag)
		{
			RebuildAllLookups();
		}
	}

	public void RebuildAllLookups()
	{
		BuildMorphSubBanksByRegion();
		BuildMorphsList();
		BuildMorphsByNameAndUid();
		BuildMorphToRegionName();
	}

	protected void InitMorphs()
	{
		if (_morphs != null)
		{
			for (int i = 0; i < _morphs.Count; i++)
			{
				_morphs[i].morphBank = this;
				_morphs[i].Init();
			}
		}
	}

	public void ResetMorphs()
	{
		Init();
		if (useThreadedMorphing && Application.isPlaying)
		{
			triggerThreadResetMorphs = true;
			return;
		}
		ApplyMorphsInternal();
		if (connectedMesh != null)
		{
			connectedMesh.ResetMorphedVertices();
		}
		if (_morphs != null)
		{
			for (int i = 0; i < _morphs.Count; i++)
			{
				_morphs[i].appliedValue = 0f;
				_morphs[i].active = false;
			}
		}
		ZeroAllBoneMorphs();
	}

	public void ResetMorphsFast(bool resetBones = true)
	{
		if (_morphs != null)
		{
			for (int i = 0; i < _morphs.Count; i++)
			{
				DAZMorph dAZMorph = _morphs[i];
				if (dAZMorph.appliedValue == 0f)
				{
					continue;
				}
				dAZMorph.appliedValue = 0f;
				dAZMorph.active = false;
				if (dAZMorph.disable || !dAZMorph.visible || !dAZMorph.hasMorphValueFormulas)
				{
					continue;
				}
				DAZMorphFormula[] formulas = _morphs[i].formulas;
				foreach (DAZMorphFormula dAZMorphFormula in formulas)
				{
					if (dAZMorphFormula.targetType != 0)
					{
						continue;
					}
					DAZMorph morph = GetMorph(dAZMorphFormula.target);
					if (morph != null)
					{
						morph.SetValue(0f);
						morph.SetDriven(b: false, string.Empty);
						if (dirtyMorphs != null)
						{
							dirtyMorphs.Add(morph);
						}
					}
				}
			}
		}
		if (resetBones)
		{
			ZeroAllBoneMorphs();
		}
	}

	protected void MTTask(object info)
	{
		DAZMorphTaskInfo dAZMorphTaskInfo = (DAZMorphTaskInfo)info;
		while (_threadsRunning)
		{
			dAZMorphTaskInfo.resetEvent.WaitOne(-1, exitContext: true);
			if (dAZMorphTaskInfo.kill)
			{
				break;
			}
			if (dAZMorphTaskInfo.taskType == DAZMorphTaskType.ApplyMorphs)
			{
				Thread.Sleep(0);
				ApplyMorphsThreaded();
			}
			dAZMorphTaskInfo.working = false;
		}
	}

	protected void StopThreads()
	{
		_threadsRunning = false;
		if (applyMorphsTask != null)
		{
			applyMorphsTask.kill = true;
			applyMorphsTask.resetEvent.Set();
			while (applyMorphsTask.thread.IsAlive)
			{
			}
			applyMorphsTask = null;
		}
	}

	protected void StartThreads()
	{
		if (!_threadsRunning)
		{
			_threadsRunning = true;
			applyMorphsTask = new DAZMorphTaskInfo();
			applyMorphsTask.threadIndex = 0;
			applyMorphsTask.name = "ApplyMorphsTask";
			applyMorphsTask.resetEvent = new AutoResetEvent(initialState: false);
			applyMorphsTask.thread = new Thread(MTTask);
			applyMorphsTask.thread.Priority = System.Threading.ThreadPriority.Lowest;
			applyMorphsTask.taskType = DAZMorphTaskType.ApplyMorphs;
			applyMorphsTask.thread.Start(applyMorphsTask);
		}
	}

	protected void ClearDirtyMorphs()
	{
		if (dirtyMorphs == null)
		{
			dirtyMorphs = new List<DAZMorph>();
		}
		else
		{
			dirtyMorphs.Clear();
		}
	}

	protected void CleanDirtyMorphs()
	{
		foreach (DAZMorph dirtyMorph in dirtyMorphs)
		{
			dirtyMorph.SyncJSON();
			dirtyMorph.SyncDrivenUI();
		}
	}

	public void ApplyMorphsThreadedStart()
	{
		if (_morphs == null)
		{
			_morphs = new List<DAZMorph>();
		}
		ClearDirtyMorphs();
		if (threadedChangedVertices == null)
		{
			numMaxThreadedChangedVertices = _threadedMorphedUVVertices.Length;
			threadedChangedVertices = new int[numMaxThreadedChangedVertices];
		}
		checkAllThreadedVertices = false;
		numThreadedChangedVertices = 0;
		threadedVerticesChanged = false;
		visibleThreadedVerticesChanged = false;
		bonesDirty = false;
		if (boneRotationsDirty == null)
		{
			boneRotationsDirty = new Dictionary<DAZBone, bool>();
		}
	}

	public void ApplyMorphsThreaded()
	{
		bool flag = true;
		int num = 5;
		while (flag)
		{
			num--;
			if (num == 0)
			{
				break;
			}
			flag = false;
			for (int i = 0; i < _morphs.Count; i++)
			{
				DAZMorph dAZMorph = _morphs[i];
				if (dAZMorph.disable)
				{
					continue;
				}
				float num2 = dAZMorph.morphValue;
				float appliedValue = dAZMorph.appliedValue;
				bool flag2 = appliedValue != num2;
				bool flag3 = num2 != 0f;
				if (dAZMorph.hasMorphValueFormulas && (flag2 || flag3))
				{
					DAZMorphFormula[] formulas = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula in formulas)
					{
						if (dAZMorphFormula.targetType != 0)
						{
							continue;
						}
						DAZMorph morph = GetMorph(dAZMorphFormula.target);
						if (morph == null)
						{
							continue;
						}
						if (dAZMorph.wasZeroedKeepChildValues)
						{
							if (morph.SetDriven(b: false, string.Empty))
							{
								dirtyMorphs.Add(morph);
							}
						}
						else if (morph.SetValue(dAZMorphFormula.multiplier * num2))
						{
							morph.SetDriven(flag3, dAZMorph.displayName);
							dirtyMorphs.Add(morph);
						}
					}
				}
				dAZMorph.wasZeroedKeepChildValues = false;
				if (enableMCMMorphs && dAZMorph.hasMCMFormulas)
				{
					DAZMorphFormula[] formulas2 = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula2 in formulas2)
					{
						if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCM)
						{
							DAZMorph morph2 = GetMorph(dAZMorphFormula2.target);
							if (morph2 != null)
							{
								num2 = morph2.morphValue * dAZMorphFormula2.multiplier;
								if (dAZMorph.SetValue(num2))
								{
									dAZMorph.SetDriven(b: true, morph2.displayName);
									dirtyMorphs.Add(dAZMorph);
								}
								continue;
							}
							num2 = 0f;
							if (dAZMorph.SetValue(num2))
							{
								dAZMorph.SetDriven(b: true, "Missing source morph");
								dirtyMorphs.Add(dAZMorph);
							}
							num2 = dAZMorph.morphValue;
						}
						else if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCMMult)
						{
							DAZMorph morph3 = GetMorph(dAZMorphFormula2.target);
							if (morph3 != null)
							{
								num2 = dAZMorph.morphValue * morph3.morphValue;
							}
						}
					}
					flag2 = appliedValue != num2;
					flag3 = num2 != 0f;
				}
				if (!flag2)
				{
					continue;
				}
				flag = true;
				if (dAZMorph.isRuntime && !dAZMorph.deltasLoaded)
				{
					dAZMorph.LoadDeltas();
				}
				ApplyBoneMorphs(morphBones, dAZMorph);
				ApplyBoneMorphs(morphBones2, dAZMorph);
				if (dAZMorph.deltas.Length > 0)
				{
					threadedVerticesChanged = true;
					if (dAZMorph.visible)
					{
						visibleThreadedVerticesChanged = true;
						float num3 = num2 - appliedValue;
						DAZMorphVertex[] deltas = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex in deltas)
						{
							if (numThreadedChangedVertices < numMaxThreadedChangedVertices)
							{
								threadedChangedVertices[numThreadedChangedVertices] = dAZMorphVertex.vertex;
								numThreadedChangedVertices++;
							}
							else
							{
								checkAllThreadedVertices = true;
							}
							_threadedMorphedUVVertices[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num3;
							_threadedMorphedUVVertices[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num3;
							_threadedMorphedUVVertices[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num3;
							_threadedVisibleMorphedUVVertices[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num3;
							_threadedVisibleMorphedUVVertices[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num3;
							_threadedVisibleMorphedUVVertices[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num3;
						}
					}
					else
					{
						float num4 = num2 - appliedValue;
						DAZMorphVertex[] deltas2 = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex2 in deltas2)
						{
							if (numThreadedChangedVertices < numMaxThreadedChangedVertices)
							{
								threadedChangedVertices[numThreadedChangedVertices] = dAZMorphVertex2.vertex;
								numThreadedChangedVertices++;
							}
							else
							{
								checkAllThreadedVertices = true;
							}
							_threadedMorphedUVVertices[dAZMorphVertex2.vertex].x += dAZMorphVertex2.delta.x * num4;
							_threadedMorphedUVVertices[dAZMorphVertex2.vertex].y += dAZMorphVertex2.delta.y * num4;
							_threadedMorphedUVVertices[dAZMorphVertex2.vertex].z += dAZMorphVertex2.delta.z * num4;
						}
					}
				}
				dAZMorph.appliedValue = num2;
				dAZMorph.active = flag3;
			}
		}
	}

	public void PrepMorphsThreadedFast()
	{
		ClearDirtyMorphs();
	}

	public bool ApplyMorphsThreadedFast(Vector3[] verts, Vector3[] visibleVerts, DAZBones bones)
	{
		bool flag = true;
		int num = 5;
		bool result = false;
		while (flag)
		{
			num--;
			if (num == 0)
			{
				break;
			}
			flag = false;
			for (int i = 0; i < _morphs.Count; i++)
			{
				DAZMorph dAZMorph = _morphs[i];
				if (dAZMorph.disable)
				{
					continue;
				}
				float num2 = dAZMorph.morphValue;
				float appliedValue = dAZMorph.appliedValue;
				bool flag2 = appliedValue != num2;
				bool flag3 = num2 != 0f;
				if (dAZMorph.hasMorphValueFormulas && (flag2 || flag3))
				{
					DAZMorphFormula[] formulas = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula in formulas)
					{
						if (dAZMorphFormula.targetType != 0)
						{
							continue;
						}
						DAZMorph morph = GetMorph(dAZMorphFormula.target);
						if (morph == null)
						{
							continue;
						}
						if (dAZMorph.wasZeroedKeepChildValues)
						{
							if (morph.SetDriven(b: false, string.Empty))
							{
								dirtyMorphs.Add(morph);
							}
						}
						else if (morph.SetValue(dAZMorphFormula.multiplier * num2))
						{
							morph.SetDriven(flag3, dAZMorph.displayName);
							dirtyMorphs.Add(morph);
						}
					}
				}
				dAZMorph.wasZeroedKeepChildValues = false;
				if (enableMCMMorphs && dAZMorph.hasMCMFormulas)
				{
					DAZMorphFormula[] formulas2 = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula2 in formulas2)
					{
						if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCM)
						{
							DAZMorph morph2 = GetMorph(dAZMorphFormula2.target);
							if (morph2 != null)
							{
								num2 = morph2.morphValue * dAZMorphFormula2.multiplier;
								if (dAZMorph.SetValue(num2))
								{
									dAZMorph.SetDriven(b: true, morph2.displayName);
									dirtyMorphs.Add(dAZMorph);
								}
							}
							else
							{
								num2 = 0f;
								if (dAZMorph.SetValue(num2))
								{
									dAZMorph.SetDriven(b: true, "Missing source morph");
									dirtyMorphs.Add(dAZMorph);
								}
							}
						}
						else if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCMMult)
						{
							DAZMorph morph3 = GetMorph(dAZMorphFormula2.target);
							if (morph3 != null)
							{
								num2 = dAZMorph.morphValue * morph3.morphValue;
							}
						}
					}
					flag2 = appliedValue != num2;
					flag3 = num2 != 0f;
				}
				if (!flag2)
				{
					continue;
				}
				flag = true;
				if (dAZMorph.isRuntime && !dAZMorph.deltasLoaded)
				{
					dAZMorph.LoadDeltas();
				}
				ApplyBoneMorphs(bones, dAZMorph);
				if (dAZMorph.deltas.Length > 0)
				{
					threadedVerticesChanged = true;
					float num3 = num2 - appliedValue;
					if (dAZMorph.visible)
					{
						result = true;
						visibleThreadedVerticesChanged = true;
						DAZMorphVertex[] deltas = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex in deltas)
						{
							verts[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num3;
							verts[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num3;
							verts[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num3;
							visibleVerts[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num3;
							visibleVerts[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num3;
							visibleVerts[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num3;
						}
					}
					else
					{
						DAZMorphVertex[] deltas2 = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex2 in deltas2)
						{
							verts[dAZMorphVertex2.vertex].x += dAZMorphVertex2.delta.x * num3;
							verts[dAZMorphVertex2.vertex].y += dAZMorphVertex2.delta.y * num3;
							verts[dAZMorphVertex2.vertex].z += dAZMorphVertex2.delta.z * num3;
						}
					}
				}
				dAZMorph.appliedValue = num2;
				dAZMorph.active = flag3;
			}
		}
		return result;
	}

	public void ApplyMorphsThreadedFastFinish()
	{
		CleanDirtyMorphs();
	}

	public void ApplyMorphsThreadedFinish()
	{
		if (triggerThreadResetMorphs)
		{
			ApplyMorphsInternal();
			if (connectedMesh != null)
			{
				connectedMesh.ResetMorphedVertices();
				_threadedMorphedUVVertices = (Vector3[])connectedMesh.UVVertices.Clone();
				_threadedVisibleMorphedUVVertices = (Vector3[])connectedMesh.UVVertices.Clone();
			}
			else
			{
				Debug.LogWarning("ResetMorphs called when connected mesh was not set. Vertices were not reset.");
			}
			if (_morphs != null)
			{
				for (int i = 0; i < _morphs.Count; i++)
				{
					_morphs[i].appliedValue = 0f;
					_morphs[i].active = false;
				}
			}
			ZeroAllBoneMorphs();
			triggerThreadResetMorphs = false;
			visibleVerticesChanged = true;
			return;
		}
		CleanDirtyMorphs();
		if (bonesDirty)
		{
			if (morphBones != null)
			{
				morphBones.SetMorphedTransform();
			}
			if (morphBones2 != null)
			{
				morphBones2.SetMorphedTransform();
			}
			bonesDirty = false;
		}
		foreach (DAZBone key in boneRotationsDirty.Keys)
		{
			key.SyncMorphBoneRotations();
		}
		boneRotationsDirty.Clear();
		visibleVerticesChanged = visibleThreadedVerticesChanged;
		if (!threadedVerticesChanged || !(connectedMesh != null))
		{
			return;
		}
		Vector3[] rawMorphedUVVertices = connectedMesh.rawMorphedUVVertices;
		Vector3[] visibleMorphedUVVertices = connectedMesh.visibleMorphedUVVertices;
		if (checkAllThreadedVertices)
		{
			int numBaseVertices = connectedMesh.numBaseVertices;
			for (int j = 0; j < numBaseVertices; j++)
			{
				ref Vector3 reference = ref rawMorphedUVVertices[j];
				reference = _threadedMorphedUVVertices[j];
				ref Vector3 reference2 = ref visibleMorphedUVVertices[j];
				reference2 = _threadedVisibleMorphedUVVertices[j];
			}
		}
		else
		{
			for (int k = 0; k < numThreadedChangedVertices; k++)
			{
				int num = threadedChangedVertices[k];
				ref Vector3 reference3 = ref rawMorphedUVVertices[num];
				reference3 = _threadedMorphedUVVertices[num];
				ref Vector3 reference4 = ref visibleMorphedUVVertices[num];
				reference4 = _threadedVisibleMorphedUVVertices[num];
			}
		}
		connectedMesh.ApplyMorphVertices(visibleVerticesChanged);
	}

	protected void ApplyBoneMorphs(DAZBones bones, DAZMorph morph, bool zero = false)
	{
		if (!(bones != null))
		{
			return;
		}
		DAZMorphFormula[] formulas = morph.formulas;
		foreach (DAZMorphFormula dAZMorphFormula in formulas)
		{
			switch (dAZMorphFormula.targetType)
			{
				case DAZMorphFormulaTargetType.GeneralScale:
					if (zero)
					{
						bones.SetGeneralScale(geometryId + ":" + morph.morphName, 0f);
					}
					else
					{
						bones.SetGeneralScale(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
					}
					break;
				case DAZMorphFormulaTargetType.BoneCenterX:
				{
					bonesDirty = true;
					DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
					if (dAZBone != null)
					{
						if (zero)
						{
							dAZBone.SetBoneXOffset(geometryId + ":" + morph.morphName, 0f);
						}
						else
						{
							dAZBone.SetBoneXOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
						}
					}
					break;
				}
				case DAZMorphFormulaTargetType.BoneCenterY:
				{
					bonesDirty = true;
					DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
					if (dAZBone != null)
					{
						if (zero)
						{
							dAZBone.SetBoneYOffset(geometryId + ":" + morph.morphName, 0f);
						}
						else
						{
							dAZBone.SetBoneYOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
						}
					}
					break;
				}
				case DAZMorphFormulaTargetType.BoneCenterZ:
				{
					bonesDirty = true;
					DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
					if (dAZBone != null)
					{
						if (zero)
						{
							dAZBone.SetBoneZOffset(geometryId + ":" + morph.morphName, 0f);
						}
						else
						{
							dAZBone.SetBoneZOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
						}
					}
					break;
				}
				case DAZMorphFormulaTargetType.OrientationX:
				{
					bonesDirty = true;
					DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
					if (dAZBone != null)
					{
						if (zero)
						{
							dAZBone.SetBoneOrientationXOffset(geometryId + ":" + morph.morphName, 0f);
						}
						else
						{
							dAZBone.SetBoneOrientationXOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
						}
					}
					break;
				}
				case DAZMorphFormulaTargetType.OrientationY:
				{
					bonesDirty = true;
					DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
					if (dAZBone != null)
					{
						if (zero)
						{
							dAZBone.SetBoneOrientationYOffset(geometryId + ":" + morph.morphName, 0f);
						}
						else
						{
							dAZBone.SetBoneOrientationYOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
						}
					}
					break;
				}
				case DAZMorphFormulaTargetType.OrientationZ:
				{
					bonesDirty = true;
					DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
					if (dAZBone != null)
					{
						if (zero)
						{
							dAZBone.SetBoneOrientationZOffset(geometryId + ":" + morph.morphName, 0f);
						}
						else
						{
							dAZBone.SetBoneOrientationZOffset(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
						}
					}
					break;
				}
				case DAZMorphFormulaTargetType.RotationX:
				{
					DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
					if (dAZBone != null)
					{
						if (!boneRotationsDirty.ContainsKey(dAZBone))
						{
							boneRotationsDirty.Add(dAZBone, value: true);
						}
						if (zero)
						{
							dAZBone.SetBoneXRotation(geometryId + ":" + morph.morphName, 0f);
						}
						else
						{
							dAZBone.SetBoneXRotation(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
						}
					}
					break;
				}
				case DAZMorphFormulaTargetType.RotationY:
				{
					DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
					if (dAZBone != null)
					{
						if (!boneRotationsDirty.ContainsKey(dAZBone))
						{
							boneRotationsDirty.Add(dAZBone, value: true);
						}
						if (zero)
						{
							dAZBone.SetBoneYRotation(geometryId + ":" + morph.morphName, 0f);
						}
						else
						{
							dAZBone.SetBoneYRotation(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
						}
					}
					break;
				}
				case DAZMorphFormulaTargetType.RotationZ:
				{
					DAZBone dAZBone = bones.GetDAZBone(dAZMorphFormula.target);
					if (dAZBone != null)
					{
						if (!boneRotationsDirty.ContainsKey(dAZBone))
						{
							boneRotationsDirty.Add(dAZBone, value: true);
						}
						if (zero)
						{
							dAZBone.SetBoneZRotation(geometryId + ":" + morph.morphName, 0f);
						}
						else
						{
							dAZBone.SetBoneZRotation(geometryId + ":" + morph.morphName, dAZMorphFormula.multiplier * morph.morphValue);
						}
					}
					break;
				}
			}
		}
	}

	protected void ApplyAllBoneMorphs()
	{
		for (int i = 0; i < _morphs.Count; i++)
		{
			ApplyBoneMorphs(morphBones, _morphs[i]);
			ApplyBoneMorphs(morphBones2, _morphs[i]);
		}
		if (morphBones != null)
		{
			morphBones.SetMorphedTransform(forceAllDirty: true);
		}
		if (morphBones2 != null)
		{
			morphBones2.SetMorphedTransform(forceAllDirty: true);
		}
		bonesDirty = false;
	}

	protected void ZeroAllBoneMorphs()
	{
		if (_morphs != null)
		{
			for (int i = 0; i < _morphs.Count; i++)
			{
				ApplyBoneMorphs(morphBones, _morphs[i], zero: true);
				ApplyBoneMorphs(morphBones2, _morphs[i], zero: true);
			}
		}
		if (morphBones != null)
		{
			morphBones.SetMorphedTransform(forceAllDirty: true);
		}
		if (morphBones2 != null)
		{
			morphBones2.SetMorphedTransform(forceAllDirty: true);
		}
		bonesDirty = false;
	}

	protected void ApplyMorphsInternal(bool force = false)
	{
		if (_morphs == null)
		{
			_morphs = new List<DAZMorph>();
		}
		visibleVerticesChanged = false;
		bool flag = false;
		bool flag2 = true;
		int num = 5;
		bonesDirty = false;
		Vector3[] array = null;
		Vector3[] array2 = null;
		Vector3[] array3 = null;
		if (connectedMesh != null)
		{
			array = connectedMesh.morphedBaseVertices;
			array2 = connectedMesh.rawMorphedUVVertices;
			array3 = connectedMesh.visibleMorphedUVVertices;
		}
		while (flag2)
		{
			num--;
			if (num == 0)
			{
				break;
			}
			flag2 = false;
			for (int i = 0; i < _morphs.Count; i++)
			{
				DAZMorph dAZMorph = _morphs[i];
				if (dAZMorph.disable)
				{
					continue;
				}
				float num2 = dAZMorph.morphValue;
				float appliedValue = dAZMorph.appliedValue;
				bool flag3 = appliedValue != num2;
				bool flag4 = num2 != 0f;
				if (dAZMorph.hasMorphValueFormulas && (flag3 || flag4))
				{
					DAZMorphFormula[] formulas = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula in formulas)
					{
						if (dAZMorphFormula.targetType != 0)
						{
							continue;
						}
						DAZMorph morph = GetMorph(dAZMorphFormula.target);
						if (morph != null)
						{
							if (dAZMorph.wasZeroedKeepChildValues)
							{
								morph.SetDriven(b: false, string.Empty, syncUI: true);
								continue;
							}
							morph.morphValue = dAZMorphFormula.multiplier * num2;
							morph.SetDriven(flag4, dAZMorph.displayName, syncUI: true);
						}
					}
				}
				dAZMorph.wasZeroedKeepChildValues = false;
				if (enableMCMMorphs && dAZMorph.hasMCMFormulas)
				{
					DAZMorphFormula[] formulas2 = dAZMorph.formulas;
					foreach (DAZMorphFormula dAZMorphFormula2 in formulas2)
					{
						if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCM)
						{
							DAZMorph morph2 = GetMorph(dAZMorphFormula2.target);
							if (morph2 != null)
							{
								num2 = (dAZMorph.morphValue = morph2.morphValue * dAZMorphFormula2.multiplier);
								dAZMorph.SetDriven(b: true, morph2.displayName);
							}
							else
							{
								num2 = 0f;
								dAZMorph.morphValue = 0f;
							}
						}
						else if (dAZMorphFormula2.targetType == DAZMorphFormulaTargetType.MCMMult)
						{
							DAZMorph morph3 = GetMorph(dAZMorphFormula2.target);
							if (morph3 != null)
							{
								num2 = dAZMorph.morphValue * morph3.morphValue;
							}
						}
					}
					flag3 = appliedValue != num2;
					flag4 = num2 != 0f;
				}
				if (!flag3)
				{
					continue;
				}
				flag2 = true;
				if (dAZMorph.isRuntime && !dAZMorph.deltasLoaded)
				{
					dAZMorph.LoadDeltas();
				}
				ApplyBoneMorphs(morphBones, dAZMorph);
				ApplyBoneMorphs(morphBones2, dAZMorph);
				if (dAZMorph.deltas.Length > 0 && connectedMesh != null && array2 != null)
				{
					flag = true;
					if (dAZMorph.visible)
					{
						visibleVerticesChanged = true;
						float num4 = num2 - appliedValue;
						DAZMorphVertex[] deltas = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex in deltas)
						{
							array2[dAZMorphVertex.vertex].x += dAZMorphVertex.delta.x * num4;
							array[dAZMorphVertex.vertex].x = array2[dAZMorphVertex.vertex].x;
							array3[dAZMorphVertex.vertex].x = array2[dAZMorphVertex.vertex].x;
							array2[dAZMorphVertex.vertex].y += dAZMorphVertex.delta.y * num4;
							array[dAZMorphVertex.vertex].y = array2[dAZMorphVertex.vertex].y;
							array3[dAZMorphVertex.vertex].y = array2[dAZMorphVertex.vertex].y;
							array2[dAZMorphVertex.vertex].z += dAZMorphVertex.delta.z * num4;
							array[dAZMorphVertex.vertex].z = array2[dAZMorphVertex.vertex].z;
							array3[dAZMorphVertex.vertex].z = array2[dAZMorphVertex.vertex].z;
							if (dAZMorph.triggerNormalRecalc)
							{
								connectedMesh.morphedBaseDirtyVertices[dAZMorphVertex.vertex] = true;
								connectedMesh.morphedNormalsDirty = true;
							}
							if (dAZMorph.triggerTangentRecalc)
							{
								connectedMesh.morphedUVDirtyVertices[dAZMorphVertex.vertex] = true;
								connectedMesh.morphedTangentsDirty = true;
							}
						}
					}
					else
					{
						float num5 = num2 - appliedValue;
						DAZMorphVertex[] deltas2 = dAZMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex2 in deltas2)
						{
							array2[dAZMorphVertex2.vertex].x += dAZMorphVertex2.delta.x * num5;
							array[dAZMorphVertex2.vertex].x = array2[dAZMorphVertex2.vertex].x;
							array2[dAZMorphVertex2.vertex].y += dAZMorphVertex2.delta.y * num5;
							array[dAZMorphVertex2.vertex].y = array2[dAZMorphVertex2.vertex].y;
							array2[dAZMorphVertex2.vertex].z += dAZMorphVertex2.delta.z * num5;
							array[dAZMorphVertex2.vertex].z = array2[dAZMorphVertex2.vertex].z;
							if (dAZMorph.triggerNormalRecalc)
							{
								connectedMesh.morphedBaseDirtyVertices[dAZMorphVertex2.vertex] = true;
								connectedMesh.morphedNormalsDirty = true;
							}
							if (dAZMorph.triggerTangentRecalc)
							{
								connectedMesh.morphedUVDirtyVertices[dAZMorphVertex2.vertex] = true;
								connectedMesh.morphedTangentsDirty = true;
							}
						}
					}
				}
				dAZMorph.appliedValue = num2;
				dAZMorph.active = flag4;
			}
		}
		if (bonesDirty)
		{
			if (morphBones != null)
			{
				morphBones.SetMorphedTransform(forceAllDirty: true);
			}
			if (morphBones2 != null)
			{
				morphBones2.SetMorphedTransform(forceAllDirty: true);
			}
			bonesDirty = false;
		}
		if ((flag || force) && connectedMesh != null)
		{
			connectedMesh.ApplyMorphVertices(visibleVerticesChanged);
		}
	}

	public void ApplyMorphs(bool force = false)
	{
		if ((bool)connectedMesh && connectedMesh.wasInit)
		{
			connectedMesh.StartMorph();
			if (!Application.isPlaying || !useThreadedMorphing)
			{
				ApplyMorphsInternal(force);
			}
		}
	}

	public void ApplyMorphsImmediate()
	{
		if (Application.isPlaying && useThreadedMorphing && _threadsRunning)
		{
			while (applyMorphsTask.working)
			{
				Thread.Sleep(0);
			}
			ApplyMorphsThreadedFinish();
			ApplyMorphsThreadedStart();
			applyMorphsTask.working = true;
			applyMorphsTask.resetEvent.Set();
			while (applyMorphsTask.working)
			{
				Thread.Sleep(0);
			}
			ApplyMorphsThreadedFinish();
			ApplyMorphsThreadedStart();
			applyMorphsTask.working = true;
			applyMorphsTask.resetEvent.Set();
			visibleVerticesChanged = true;
			if (connectedMesh != null)
			{
				connectedMesh.ApplyMorphVertices(visibleVerticesChanged);
			}
		}
		else
		{
			ApplyMorphsInternal();
		}
	}

	private void Update()
	{
		if (!connectedMesh || !connectedMesh.wasInit || !updateEnabled)
		{
			return;
		}
		connectedMesh.StartMorph();
		if (Application.isPlaying && useThreadedMorphing)
		{
			StartThreads();
			totalFrameCount++;
			if (!applyMorphsTask.working)
			{
				ApplyMorphsThreadedFinish();
				ApplyMorphsThreadedStart();
				applyMorphsTask.working = true;
				applyMorphsTask.resetEvent.Set();
			}
			else if (OVRManager.isHmdPresent)
			{
				missedFrameCount++;
				Debug.LogWarning("ApplyMorphsTask did not complete in 1 frame. Missed " + missedFrameCount + " out of total " + totalFrameCount);
				DebugHUD.Msg("ApplyMorphsTask miss " + missedFrameCount + " out of total " + totalFrameCount);
				DebugHUD.Alert2();
			}
		}
		else
		{
			ApplyMorphsInternal();
		}
	}

	public void ReInit()
	{
		RebuildAllLookups();
		ResetMorphs();
		ApplyMorphs(force: true);
	}

	public void Init()
	{
		if (!wasInit)
		{
			wasInit = true;
			if (boneRotationsDirty == null)
			{
				boneRotationsDirty = new Dictionary<DAZBone, bool>();
			}
			if (morphBones != null)
			{
				morphBones.Init();
			}
			if (morphBones2 != null)
			{
				morphBones2.Init();
			}
			RebuildAllLookups();
			if (Application.isPlaying)
			{
				RefreshRuntimeMorphs();
				RefreshPackageMorphs();
				InitMorphs();
			}
		}
	}

	protected void RefreshRuntimeMorphs()
	{
		if (wasInit && autoImportFolder != null && autoImportFolder != string.Empty && RuntimeImportFromDir(autoImportFolder))
		{
			DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
			DAZMorphSubBank[] array = componentsInChildren;
			foreach (DAZMorphSubBank dAZMorphSubBank in array)
			{
				dAZMorphSubBank.CompleteRuntimeMorphAdd();
			}
			RebuildAllLookups();
		}
	}

	public bool RefreshPackageMorphs()
	{
		if (wasInit && autoImportFolder != null && autoImportFolder != string.Empty)
		{
			List<VarDirectoryEntry> list = FileManager.FindVarDirectories(autoImportFolder);
			HashSet<string> hashSet = new HashSet<string>();
			foreach (VarDirectoryEntry item in list)
			{
				hashSet.Add(item.Package.Uid);
			}
			if (currentLoadedMorphPackageUids != null && currentLoadedMorphPackageUids.SetEquals(hashSet))
			{
				return false;
			}
			currentLoadedMorphPackageUids = hashSet;
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			Dictionary<string, float> dictionary3 = new Dictionary<string, float>();
			foreach (DAZMorph morph in _morphs)
			{
				if (morph.isInPackage && morph.morphValue != morph.startValue)
				{
					dictionary.Add(morph.uid, morph.morphValue);
					dictionary2.Add(morph.uid, morph.resolvedDisplayName);
					if (!dictionary3.ContainsKey(morph.resolvedDisplayName))
					{
						dictionary3.Add(morph.resolvedDisplayName, morph.morphValue);
					}
				}
			}
			DAZMorphSubBank[] componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
			bool flag = false;
			DAZMorphSubBank[] array = componentsInChildren;
			foreach (DAZMorphSubBank dAZMorphSubBank in array)
			{
				if (dAZMorphSubBank.ClearPackageMorphs())
				{
					flag = true;
				}
			}
			if (flag)
			{
				RebuildAllLookups();
			}
			bool flag2 = false;
			foreach (VarDirectoryEntry item2 in list)
			{
				if (item2.Package.Group.GetCustomOption("preloadMorphs"))
				{
					if (RuntimeImportFromDir(item2))
					{
						flag2 = true;
					}
				}
				else if (RuntimeImportFromDir(item2, isTransient: false, isDemandLoaded: true))
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				componentsInChildren = GetComponentsInChildren<DAZMorphSubBank>();
				DAZMorphSubBank[] array2 = componentsInChildren;
				foreach (DAZMorphSubBank dAZMorphSubBank2 in array2)
				{
					dAZMorphSubBank2.CompletePackageMorphAdd();
				}
				RebuildAllLookups();
				foreach (string key in dictionary.Keys)
				{
					DAZMorph dAZMorph = GetMorphByUid(key);
					if (dAZMorph == null && dictionary2.TryGetValue(key, out var value))
					{
						dAZMorph = GetMorphByDisplayName(value);
					}
					if (dAZMorph != null && dictionary.TryGetValue(key, out var value2))
					{
						dAZMorph.morphValue = value2;
					}
				}
				CleanDemandActivatedMorphs();
			}
			if (flag || flag2)
			{
				return true;
			}
		}
		return false;
	}

	protected void EnableInit()
	{
		if (connectedMesh != null)
		{
			connectedMesh.Init();
			if (Application.isPlaying)
			{
				if (_threadedMorphedUVVertices == null)
				{
					_threadedMorphedUVVertices = (Vector3[])connectedMesh.UVVertices.Clone();
				}
				if (_threadedVisibleMorphedUVVertices == null)
				{
					_threadedVisibleMorphedUVVertices = (Vector3[])connectedMesh.UVVertices.Clone();
				}
			}
		}
		ResetMorphs();
		ApplyMorphs(force: true);
	}

	private void OnDisable()
	{
		if (!Application.isPlaying)
		{
			wasInit = false;
		}
		ZeroAllBoneMorphs();
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			Init();
		}
		EnableInit();
	}

	private void Awake()
	{
		if (Application.isPlaying)
		{
			Init();
		}
	}

	protected void OnDestroy()
	{
		if (Application.isPlaying)
		{
			StopThreads();
		}
	}

	protected void OnApplicationQuit()
	{
		if (Application.isPlaying)
		{
			StopThreads();
		}
	}
}
