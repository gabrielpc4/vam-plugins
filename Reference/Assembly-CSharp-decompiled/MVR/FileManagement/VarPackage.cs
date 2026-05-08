using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using SimpleJSON;
using UnityEngine;

namespace MVR.FileManagement;

public class VarPackage
{
	public enum ReferenceVersionOption
	{
		Latest,
		Minimum,
		Exact
	}

	protected bool _enabled;

	protected Thread unpackThread;

	protected Thread repackThread;

	protected int packFileProgressCount;

	protected int packFileTotalCount;

	public bool packThreadAbort;

	public string packThreadError;

	protected Dictionary<string, bool> customOptions;

	protected Dictionary<string, VarDirectoryEntry> _DirectoryEntryLookup;

	public bool isNewestVersion;

	public bool isNewestEnabledVersion;

	protected VarFileEntry metaEntry;

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			if (_enabled == value)
			{
				return;
			}
			_enabled = value;
			string path = Path + ".disabled";
			if (FileManager.FileExists(path))
			{
				if (_enabled)
				{
					FileManager.DeleteFile(path);
					FileManager.Refresh();
				}
			}
			else if (!_enabled)
			{
				FileManager.WriteAllText(path, string.Empty);
				FileManager.Refresh();
			}
		}
	}

	public float packProgress { get; protected set; }

	public bool IsUnpacking
	{
		get
		{
			if (unpackThread != null && unpackThread.IsAlive)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsRepacking
	{
		get
		{
			if (repackThread != null && repackThread.IsAlive)
			{
				return true;
			}
			return false;
		}
	}

	public bool HasOriginalCopy
	{
		get
		{
			string path = Path + ".orig";
			return FileManager.FileExists(path);
		}
	}

	public bool IsSimulated { get; protected set; }

	public string Uid { get; protected set; }

	public string Path { get; protected set; }

	public string SlashPath { get; protected set; }

	public string FullPath { get; protected set; }

	public string FullSlashPath { get; protected set; }

	public VarPackageGroup Group { get; protected set; }

	public string GroupName { get; protected set; }

	public string Creator { get; protected set; }

	public string Name { get; protected set; }

	public int Version { get; protected set; }

	public ReferenceVersionOption StandardReferenceVersionOption { get; protected set; }

	public ReferenceVersionOption ScriptReferenceVersionOption { get; protected set; }

	public DateTime LastWriteTime { get; protected set; }

	public List<VarFileEntry> FileEntries { get; protected set; }

	public List<VarDirectoryEntry> DirectoryEntries { get; protected set; }

	public string LicenseType { get; protected set; }

	public string SecondaryLicenseType { get; protected set; }

	public string EAEndYear { get; protected set; }

	public string EAEndMonth { get; protected set; }

	public string EAEndDay { get; protected set; }

	public string Description { get; protected set; }

	public string Credits { get; protected set; }

	public string Instructions { get; protected set; }

	public string PromotionalLink { get; protected set; }

	public string ProgramVersion { get; protected set; }

	public List<string> Contents { get; protected set; }

	public List<string> PackageDependencies { get; protected set; }

	public List<VarPackage> PackageDependenciesResolved { get; protected set; }

	public VarDirectoryEntry RootDirectory { get; protected set; }

	public VarPackage(string uid, string path, VarPackageGroup group, string creator, string name, int version)
	{
		Uid = uid;
		Path = path.Replace('/', '\\');
		SlashPath = path.Replace('\\', '/');
		FullPath = System.IO.Path.GetFullPath(Path);
		FullSlashPath = FullPath.Replace('\\', '/');
		Name = name;
		Group = group;
		GroupName = group.Name;
		Creator = creator;
		Version = version;
		if (FileManager.debug)
		{
			Debug.Log("New package\n Uid: " + Uid + "\n Path: " + Path + "\n FullPath: " + FullPath + "\n SlashPath: " + SlashPath + "\n Name: " + Name + "\n GroupName: " + GroupName + "\n Creator: " + Creator + "\n Version: " + Version);
		}
		Scan();
	}

	protected void SyncEnabled()
	{
		_enabled = !FileManager.FileExists(Path + ".disabled");
	}

	public void Delete()
	{
		if (File.Exists(Path))
		{
			FileManager.DeleteFile(Path);
		}
		else if (Directory.Exists(Path))
		{
			FileManager.DeleteDirectory(Path, recursive: true);
		}
		string path = Path + ".disabled";
		if (File.Exists(path))
		{
			FileManager.DeleteFile(path);
		}
		FileManager.Refresh();
	}

	protected void ProcessFileMethod(object sender, ScanEventArgs args)
	{
		packFileProgressCount++;
		if (packFileTotalCount != 0)
		{
			packProgress = (float)packFileProgressCount / (float)packFileTotalCount;
		}
		if (packThreadAbort)
		{
			args.ContinueRunning = false;
		}
	}

	protected void UnpackThreaded()
	{
		try
		{
			string text = Path + ".orig";
			if (!FileManager.FileExists(text))
			{
				FileManager.CopyFile(Path, text);
			}
			FastZipEvents fastZipEvents = new FastZipEvents();
			fastZipEvents.ProcessFile = ProcessFileMethod;
			FastZip fastZip = new FastZip(fastZipEvents);
			string text2 = Path + ".extracted";
			if (FileManager.DirectoryExists(text2))
			{
				FileManager.DeleteDirectory(text2, recursive: true);
			}
			fastZip.ExtractZip(Path, text2, string.Empty);
			FileManager.DeleteFile(Path);
			FileManager.MoveDirectory(text2, Path);
		}
		catch (Exception ex)
		{
			packThreadError = ex.Message;
		}
	}

	public void Unpack()
	{
		if (!IsSimulated && (unpackThread == null || !unpackThread.IsAlive))
		{
			packThreadError = null;
			packThreadAbort = false;
			packProgress = 0f;
			packFileProgressCount = 0;
			packFileTotalCount = FileEntries.Count;
			unpackThread = new Thread(UnpackThreaded);
			unpackThread.Start();
		}
	}

	protected void RepackThreaded()
	{
		try
		{
			FastZipEvents fastZipEvents = new FastZipEvents();
			fastZipEvents.ProcessFile = ProcessFileMethod;
			FastZip fastZip = new FastZip(fastZipEvents);
			fastZip.CreateEmptyDirectories = true;
			string text = Path + ".zip";
			if (FileManager.FileExists(text))
			{
				FileManager.DeleteFile(text);
			}
			fastZip.CreateZip(text, Path, recurse: true, string.Empty);
			string text2 = Path + ".todelete";
			try
			{
				FileManager.MoveDirectory(Path, text2);
			}
			catch (Exception)
			{
				packThreadError = "Error during attempt of move and delete of " + Path + ". Do you have this folder open in explorer or files in this folder open in another tool?";
				return;
			}
			FileManager.MoveFile(text, Path);
			FileManager.DeleteDirectory(text2, recursive: true);
		}
		catch (Exception ex2)
		{
			packThreadError = ex2.Message;
		}
	}

	public void Repack()
	{
		if (IsSimulated && (repackThread == null || !repackThread.IsAlive))
		{
			packThreadError = null;
			packThreadAbort = false;
			packProgress = 0f;
			packFileProgressCount = 0;
			packFileTotalCount = FileManager.FolderContentsCount(Path);
			repackThread = new Thread(RepackThreaded);
			repackThread.Start();
		}
	}

	public void RestoreFromOriginal()
	{
		string text = Path + ".orig";
		if (FileManager.FileExists(text))
		{
			if (FileManager.DirectoryExists(Path))
			{
				FileManager.DeleteDirectory(Path, recursive: true);
			}
			else if (FileManager.FileExists(Path))
			{
				FileManager.DeleteFile(Path);
			}
			FileManager.MoveFile(text, Path);
		}
	}

	public List<string> GetCustomOptionNames()
	{
		if (customOptions != null)
		{
			return customOptions.Keys.ToList();
		}
		return new List<string>();
	}

	public bool GetCustomOption(string optionName)
	{
		bool value = false;
		if (customOptions != null)
		{
			customOptions.TryGetValue(optionName, out value);
		}
		return value;
	}

	public VarDirectoryEntry GetDirectoryEntry(string path)
	{
		VarDirectoryEntry value = null;
		if (_DirectoryEntryLookup != null)
		{
			_DirectoryEntryLookup.TryGetValue(path, out value);
		}
		return value;
	}

	public bool HasMatchingDirectories(string dir)
	{
		string input = dir.Replace('\\', '/');
		input = Regex.Replace(input, "/$", string.Empty);
		foreach (VarDirectoryEntry directoryEntry in DirectoryEntries)
		{
			if (directoryEntry.InternalSlashPath == input)
			{
				return true;
			}
		}
		return false;
	}

	public List<VarDirectoryEntry> FindVarDirectories(string dir, bool exactMatch = true)
	{
		string input = dir.Replace('\\', '/');
		input = Regex.Replace(input, "/$", string.Empty);
		List<VarDirectoryEntry> list = new List<VarDirectoryEntry>();
		foreach (VarDirectoryEntry directoryEntry in DirectoryEntries)
		{
			if (exactMatch)
			{
				if (directoryEntry.InternalSlashPath == input)
				{
					list.Add(directoryEntry);
				}
			}
			else if (directoryEntry.InternalSlashPath.StartsWith(dir))
			{
				list.Add(directoryEntry);
			}
		}
		return list;
	}

	public bool HasMatchingFiles(string dir, string pattern)
	{
		if (HasMatchingDirectories(dir))
		{
			string pattern2 = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
			foreach (VarFileEntry fileEntry in FileEntries)
			{
				if (!fileEntry.InternalSlashPath.StartsWith(dir) || !Regex.IsMatch(fileEntry.Name, pattern2))
				{
					continue;
				}
				return true;
			}
		}
		return false;
	}

	public void FindFiles(string dir, string pattern, List<FileEntry> foundFiles)
	{
		string pattern2 = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
		foreach (VarFileEntry fileEntry in FileEntries)
		{
			if (fileEntry.InternalSlashPath.StartsWith(dir) && Regex.IsMatch(fileEntry.Name, pattern2))
			{
				foundFiles.Add(fileEntry);
			}
		}
	}

	protected void CreateDirectoryEntries(VarFileEntry varFileEntry)
	{
		string internalSlashPath = varFileEntry.InternalSlashPath;
		string[] array = internalSlashPath.Split('/');
		VarDirectoryEntry varDirectoryEntry = RootDirectory;
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			if (i == array.Length - 1)
			{
				varDirectoryEntry.AddFileEntry(varFileEntry);
				continue;
			}
			text = ((!(text == string.Empty)) ? (text + "/" + array[i]) : (text + array[i]));
			if (!_DirectoryEntryLookup.TryGetValue(text, out var value))
			{
				value = new VarDirectoryEntry(this, text, varDirectoryEntry);
				varDirectoryEntry.AddSubDirectory(value);
				DirectoryEntries.Add(value);
				_DirectoryEntryLookup.Add(text, value);
			}
			varDirectoryEntry = value;
		}
	}

	protected void Scan()
	{
		FileEntries = new List<VarFileEntry>();
		DirectoryEntries = new List<VarDirectoryEntry>();
		_DirectoryEntryLookup = new Dictionary<string, VarDirectoryEntry>();
		_DirectoryEntryLookup.Add(string.Empty, RootDirectory);
		SyncEnabled();
		if (File.Exists(Path))
		{
			IsSimulated = false;
			float elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
			ZipFile zipFile = null;
			try
			{
				LastWriteTime = File.GetLastWriteTime(Path);
				RootDirectory = new VarDirectoryEntry(this, string.Empty);
				DirectoryEntries.Add(RootDirectory);
				FileStream file = File.OpenRead(Path);
				zipFile = new ZipFile(file);
				metaEntry = null;
				foreach (ZipEntry item in zipFile)
				{
					if (item.IsFile)
					{
						VarFileEntry varFileEntry = new VarFileEntry(this, item.Name, item.DateTime);
						FileEntries.Add(varFileEntry);
						CreateDirectoryEntries(varFileEntry);
						if (item.Name == "meta.json")
						{
							metaEntry = varFileEntry;
						}
					}
				}
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during zip file scan of " + Path + ": " + ex);
			}
			finally
			{
				if (zipFile != null)
				{
					zipFile.IsStreamOwner = true;
					zipFile.Close();
				}
			}
			float elapsedMilliseconds2 = GlobalStopwatch.GetElapsedMilliseconds();
			float num = elapsedMilliseconds2 - elapsedMilliseconds;
			if (FileManager.debug)
			{
				Debug.Log("Scanned var package " + Path + " in " + num.ToString("F1") + " ms");
			}
		}
		else
		{
			if (!Directory.Exists(Path))
			{
				return;
			}
			IsSimulated = true;
			float elapsedMilliseconds3 = GlobalStopwatch.GetElapsedMilliseconds();
			try
			{
				LastWriteTime = Directory.GetLastWriteTime(Path);
				RootDirectory = new VarDirectoryEntry(this, string.Empty);
				DirectoryEntries.Add(RootDirectory);
				metaEntry = null;
				string[] files = Directory.GetFiles(Path, "*", SearchOption.AllDirectories);
				string[] array = files;
				foreach (string text in array)
				{
					string text2 = text.Replace(Path + "\\", string.Empty);
					text2 = text2.Replace('\\', '/');
					VarFileEntry varFileEntry2 = new VarFileEntry(this, text2, File.GetLastWriteTime(text), simulated: true);
					FileEntries.Add(varFileEntry2);
					CreateDirectoryEntries(varFileEntry2);
					if (text2 == "meta.json")
					{
						metaEntry = varFileEntry2;
					}
				}
			}
			catch (Exception ex2)
			{
				Debug.LogError("Exception during var directory scan of " + Path + ": " + ex2);
			}
			float elapsedMilliseconds4 = GlobalStopwatch.GetElapsedMilliseconds();
			float num2 = elapsedMilliseconds4 - elapsedMilliseconds3;
			if (FileManager.debug)
			{
				Debug.Log("Scanned var package " + Path + " in " + num2.ToString("F1") + " ms");
			}
		}
	}

	public void LoadMetaData()
	{
		if (metaEntry == null)
		{
			return;
		}
		using VarFileEntryStreamReader varFileEntryStreamReader = new VarFileEntryStreamReader(metaEntry);
		string aJSON = varFileEntryStreamReader.ReadToEnd();
		JSONClass asObject = JSON.Parse(aJSON).AsObject;
		if (!(asObject != null))
		{
			return;
		}
		if (asObject["licenseType"] != null)
		{
			LicenseType = asObject["licenseType"];
		}
		else
		{
			LicenseType = "MISSING";
		}
		SecondaryLicenseType = asObject["secondaryLicenseType"];
		EAEndYear = asObject["EAEndYear"];
		EAEndMonth = asObject["EAEndMonth"];
		EAEndDay = asObject["EAEndDay"];
		if (asObject["standardReferenceVersionOption"] != null)
		{
			try
			{
				string value = asObject["standardReferenceVersionOption"];
				ReferenceVersionOption standardReferenceVersionOption = (ReferenceVersionOption)Enum.Parse(typeof(ReferenceVersionOption), value);
				StandardReferenceVersionOption = standardReferenceVersionOption;
			}
			catch (ArgumentException)
			{
				StandardReferenceVersionOption = ReferenceVersionOption.Latest;
			}
		}
		else
		{
			StandardReferenceVersionOption = ReferenceVersionOption.Latest;
		}
		if (asObject["scriptReferenceVersionOption"] != null)
		{
			try
			{
				string value2 = asObject["scriptReferenceVersionOption"];
				ReferenceVersionOption scriptReferenceVersionOption = (ReferenceVersionOption)Enum.Parse(typeof(ReferenceVersionOption), value2);
				ScriptReferenceVersionOption = scriptReferenceVersionOption;
			}
			catch (ArgumentException)
			{
				ScriptReferenceVersionOption = ReferenceVersionOption.Exact;
			}
		}
		else
		{
			ScriptReferenceVersionOption = ReferenceVersionOption.Exact;
		}
		Description = asObject["description"];
		Credits = asObject["credits"];
		Instructions = asObject["instructions"];
		if (asObject["promotionalLink"] != null)
		{
			PromotionalLink = asObject["promotionalLink"];
		}
		else
		{
			PromotionalLink = asObject["patreonLink"];
		}
		List<string> list = new List<string>();
		JSONArray asArray = asObject["contentList"].AsArray;
		if (asArray != null)
		{
			foreach (JSONNode item in asArray)
			{
				string text = item;
				if (text != null)
				{
					list.Add(text);
				}
			}
		}
		Contents = list;
		PackageDependencies = new List<string>();
		PackageDependenciesResolved = new List<VarPackage>();
		JSONClass asObject2 = asObject["dependencies"].AsObject;
		if (asObject2 != null)
		{
			foreach (string key in asObject2.Keys)
			{
				VarPackage package = FileManager.GetPackage(key);
				if (package == null)
				{
					string packageGroupUid = Regex.Replace(key, "\\.[0-9]+$", string.Empty);
					VarPackageGroup packageGroup = FileManager.GetPackageGroup(packageGroupUid);
					if (packageGroup != null)
					{
						VarPackage newestPackage = packageGroup.NewestPackage;
						PackageDependenciesResolved.Add(newestPackage);
					}
					else
					{
						SuperController.LogError("Missing addon package " + key + " that package" + Uid + " depends on");
					}
				}
				else
				{
					PackageDependenciesResolved.Add(package);
				}
				PackageDependencies.Add(key);
			}
		}
		JSONClass asObject3 = asObject["customOptions"].AsObject;
		customOptions = new Dictionary<string, bool>();
		if (!(asObject3 != null))
		{
			return;
		}
		foreach (string key2 in asObject3.Keys)
		{
			if (!customOptions.ContainsKey(key2))
			{
				customOptions.Add(key2, asObject3[key2].AsBool);
			}
		}
	}
}
