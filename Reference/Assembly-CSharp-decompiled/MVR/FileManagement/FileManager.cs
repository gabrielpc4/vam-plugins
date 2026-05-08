using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using MVR.FileManagementSecure;
using UnityEngine;

namespace MVR.FileManagement;

public class FileManager : MonoBehaviour
{
	public delegate void OnRefresh();

	public static bool debug;

	protected static Dictionary<string, VarPackage> packagesByUid;

	protected static Dictionary<string, VarPackage> packagesByPath;

	protected static Dictionary<string, VarPackageGroup> packageGroups;

	protected static List<VarFileEntry> allVarFileEntries;

	protected static List<VarDirectoryEntry> allVarDirectoryEntries;

	protected static Dictionary<string, VarFileEntry> uidToVarFileEntry;

	protected static Dictionary<string, VarFileEntry> pathToVarFileEntry;

	protected static Dictionary<string, VarDirectoryEntry> uidToVarDirectoryEntry;

	protected static Dictionary<string, VarDirectoryEntry> pathToVarDirectoryEntry;

	protected static Dictionary<string, VarDirectoryEntry> varPackagePathToRootVarDirectory;

	protected static string packageFolder = "AddonPackages";

	protected static string userPrefsFolder = "AddonPackagesUserPrefs";

	protected static OnRefresh onRefreshHandlers;

	protected static HashSet<string> secureReadPaths;

	protected static HashSet<string> secureWritePaths;

	protected static Stack<string> loadDirStack;

	public static string PackageFolder => packageFolder;

	public static string UserPrefsFolder => userPrefsFolder;

	public static string CurrentLoadDir
	{
		get
		{
			if (loadDirStack != null && loadDirStack.Count > 0)
			{
				return loadDirStack.Peek();
			}
			return null;
		}
	}

	public static string CurrentPackageUid
	{
		get
		{
			string currentLoadDir = CurrentLoadDir;
			if (currentLoadDir != null)
			{
				VarDirectoryEntry varDirectoryEntry = GetVarDirectoryEntry(currentLoadDir);
				if (varDirectoryEntry != null)
				{
					return varDirectoryEntry.Package.Uid;
				}
			}
			return null;
		}
	}

	public static string CurrentSaveDir { get; protected set; }

	protected static void RegisterPackage(string vpath)
	{
		if (debug)
		{
			Debug.Log("RegisterPackage " + vpath);
		}
		string input = vpath.Replace('\\', '/');
		input = Regex.Replace(input, "\\.(var|zip)$", string.Empty);
		input = Regex.Replace(input, ".*/", string.Empty);
		string[] array = input.Split('.');
		if (array.Length == 3)
		{
			string text = array[0];
			string text2 = array[1];
			string key = text + "." + text2;
			string s = array[2];
			try
			{
				int version = int.Parse(s);
				if (packagesByUid.ContainsKey(input))
				{
					SuperController.LogError("Duplicate package uid " + input + ". Cannot register");
					return;
				}
				if (!packageGroups.TryGetValue(key, out var value))
				{
					value = new VarPackageGroup(key);
					packageGroups.Add(key, value);
				}
				VarPackage varPackage = new VarPackage(input, vpath, value, text, text2, version);
				packagesByUid.Add(input, varPackage);
				packagesByPath.Add(varPackage.Path, varPackage);
				packagesByPath.Add(varPackage.SlashPath, varPackage);
				packagesByPath.Add(varPackage.FullPath, varPackage);
				packagesByPath.Add(varPackage.FullSlashPath, varPackage);
				value.AddPackage(varPackage);
				if (!varPackage.Enabled)
				{
					return;
				}
				foreach (VarFileEntry fileEntry in varPackage.FileEntries)
				{
					allVarFileEntries.Add(fileEntry);
					uidToVarFileEntry.Add(fileEntry.Uid, fileEntry);
					if (debug)
					{
						Debug.Log("Add var file with UID " + fileEntry.Uid);
					}
					pathToVarFileEntry.Add(fileEntry.Path, fileEntry);
					pathToVarFileEntry.Add(fileEntry.SlashPath, fileEntry);
					pathToVarFileEntry.Add(fileEntry.FullPath, fileEntry);
					pathToVarFileEntry.Add(fileEntry.FullSlashPath, fileEntry);
				}
				foreach (VarDirectoryEntry directoryEntry in varPackage.DirectoryEntries)
				{
					allVarDirectoryEntries.Add(directoryEntry);
					if (debug)
					{
						Debug.Log("Add var directory with UID " + directoryEntry.Uid);
					}
					uidToVarDirectoryEntry.Add(directoryEntry.Uid, directoryEntry);
					pathToVarDirectoryEntry.Add(directoryEntry.Path, directoryEntry);
					pathToVarDirectoryEntry.Add(directoryEntry.SlashPath, directoryEntry);
					pathToVarDirectoryEntry.Add(directoryEntry.FullPath, directoryEntry);
					pathToVarDirectoryEntry.Add(directoryEntry.FullSlashPath, directoryEntry);
				}
				varPackagePathToRootVarDirectory.Add(varPackage.Path, varPackage.RootDirectory);
				varPackagePathToRootVarDirectory.Add(varPackage.FullPath, varPackage.RootDirectory);
				return;
			}
			catch (FormatException)
			{
				SuperController.LogError("VAR file " + vpath + " does not use integer version field in name <creator>.<name>.<version>");
				return;
			}
		}
		SuperController.LogError("VAR file " + vpath + " is not named with convention <creator>.<name>.<version>");
	}

	public static void RegisterRefreshHandler(OnRefresh refreshHandler)
	{
		onRefreshHandlers = (OnRefresh)Delegate.Combine(onRefreshHandlers, refreshHandler);
	}

	public static void UnregisterRefreshHandler(OnRefresh refreshHandler)
	{
		onRefreshHandlers = (OnRefresh)Delegate.Remove(onRefreshHandlers, refreshHandler);
	}

	public static void Refresh()
	{
		if (debug)
		{
			Debug.Log("FileManager Refresh()");
		}
		float num = GlobalStopwatch.GetElapsedMilliseconds();
		packagesByUid = new Dictionary<string, VarPackage>();
		packagesByPath = new Dictionary<string, VarPackage>();
		packageGroups = new Dictionary<string, VarPackageGroup>();
		allVarFileEntries = new List<VarFileEntry>();
		allVarDirectoryEntries = new List<VarDirectoryEntry>();
		uidToVarFileEntry = new Dictionary<string, VarFileEntry>();
		pathToVarFileEntry = new Dictionary<string, VarFileEntry>();
		uidToVarDirectoryEntry = new Dictionary<string, VarDirectoryEntry>();
		pathToVarDirectoryEntry = new Dictionary<string, VarDirectoryEntry>();
		varPackagePathToRootVarDirectory = new Dictionary<string, VarDirectoryEntry>();
		float elapsedMilliseconds;
		try
		{
			if (!Directory.Exists(packageFolder))
			{
				CreateDirectory(packageFolder);
			}
			if (!Directory.Exists(userPrefsFolder))
			{
				CreateDirectory(userPrefsFolder);
			}
			if (Directory.Exists(packageFolder))
			{
				string[] files = Directory.GetFiles(packageFolder, "*.var.zip", SearchOption.AllDirectories);
				foreach (string text in files)
				{
					string text2 = Regex.Replace(text, "\\.zip$", string.Empty);
					if (!Directory.Exists(text2))
					{
						Directory.CreateDirectory(text2);
						FastZip fastZip = new FastZip();
						fastZip.ExtractZip(text, text2, string.Empty);
					}
				}
				string[] directories = Directory.GetDirectories(packageFolder, "*.var", SearchOption.AllDirectories);
				foreach (string vpath in directories)
				{
					RegisterPackage(vpath);
				}
				string[] files2 = Directory.GetFiles(packageFolder, "*.var", SearchOption.AllDirectories);
				foreach (string vpath2 in files2)
				{
					RegisterPackage(vpath2);
				}
			}
			foreach (VarPackage value in packagesByUid.Values)
			{
				value.LoadMetaData();
			}
			foreach (VarPackageGroup value2 in packageGroups.Values)
			{
				value2.Init();
			}
			elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
			float num2 = elapsedMilliseconds - num;
			Debug.Log("Scanned " + packagesByUid.Count + " packages in " + num2.ToString("F1") + " ms");
			num = elapsedMilliseconds;
			if (onRefreshHandlers != null)
			{
				onRefreshHandlers();
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during package refresh " + ex);
		}
		elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		Debug.Log("Refresh package handlers took " + (elapsedMilliseconds - num).ToString("F1") + " ms");
	}

	public static void RegisterSecureReadPath(string path)
	{
		if (secureReadPaths == null)
		{
			secureReadPaths = new HashSet<string>();
		}
		secureReadPaths.Add(Path.GetFullPath(path));
	}

	public static void ClearSecureReadPaths()
	{
		if (secureReadPaths == null)
		{
			secureReadPaths = new HashSet<string>();
		}
		else
		{
			secureReadPaths.Clear();
		}
	}

	public static bool IsSecureReadPath(string path)
	{
		if (secureReadPaths == null)
		{
			secureReadPaths = new HashSet<string>();
		}
		string fullPath = GetFullPath(path);
		bool result = false;
		foreach (string secureReadPath in secureReadPaths)
		{
			if (fullPath.StartsWith(secureReadPath))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static void ClearSecureWritePaths()
	{
		if (secureWritePaths == null)
		{
			secureWritePaths = new HashSet<string>();
		}
		else
		{
			secureWritePaths.Clear();
		}
	}

	public static void RegisterSecureWritePath(string path)
	{
		if (secureWritePaths == null)
		{
			secureWritePaths = new HashSet<string>();
		}
		secureWritePaths.Add(Path.GetFullPath(path));
	}

	public static bool IsSecureWritePath(string path)
	{
		if (secureWritePaths == null)
		{
			secureWritePaths = new HashSet<string>();
		}
		string fullPath = GetFullPath(path);
		bool result = false;
		foreach (string secureWritePath in secureWritePaths)
		{
			if (fullPath.StartsWith(secureWritePath))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static string GetFullPath(string path)
	{
		string path2 = Regex.Replace(path, "^file:///", string.Empty);
		return Path.GetFullPath(path2);
	}

	public static bool IsSimulatedPackagePath(string path)
	{
		string input = path.Replace('\\', '/');
		string packageUidOrPath = Regex.Replace(input, ":/.*", string.Empty);
		return GetPackage(packageUidOrPath)?.IsSimulated ?? false;
	}

	public static string ConvertSimulatedPackagePathToNormalPath(string path)
	{
		string text = path.Replace('\\', '/');
		if (text.Contains(":/"))
		{
			string packageUidOrPath = Regex.Replace(text, ":/.*", string.Empty);
			VarPackage package = GetPackage(packageUidOrPath);
			if (package != null && package.IsSimulated)
			{
				string text2 = Regex.Replace(text, ".*:/", string.Empty);
				path = package.SlashPath + "/" + text2;
			}
		}
		return path;
	}

	public static string RemovePackageFromPath(string path)
	{
		string input = Regex.Replace(path, ".*:/", string.Empty);
		return Regex.Replace(input, ".*:\\\\", string.Empty);
	}

	public static string NormalizePath(string path)
	{
		string text = path;
		VarFileEntry varFileEntry = GetVarFileEntry(path);
		if (varFileEntry == null)
		{
			string fullPath = GetFullPath(path);
			string oldValue = Path.GetFullPath(".") + "\\";
			string text2 = fullPath.Replace(oldValue, string.Empty);
			if (text2 != fullPath)
			{
				text = text2;
			}
			return text.Replace('\\', '/');
		}
		return varFileEntry.Uid;
	}

	public static string GetDirectoryName(string path, bool returnSlashPath = false)
	{
		VarFileEntry value;
		string path2 = ((uidToVarFileEntry != null && uidToVarFileEntry.TryGetValue(path, out value)) ? ((!returnSlashPath) ? value.Path : value.SlashPath) : ((!returnSlashPath) ? path.Replace('/', '\\') : path.Replace('\\', '/')));
		return Path.GetDirectoryName(path2);
	}

	public static string GetSuggestedBrowserDirectoryFromDirectoryPath(string suggestedDir, string currentDir, bool allowPackagePath = true)
	{
		if (currentDir == null || currentDir == string.Empty)
		{
			return suggestedDir;
		}
		string input = suggestedDir.Replace('\\', '/');
		input = Regex.Replace(input, "/$", string.Empty);
		string text = currentDir.Replace('\\', '/');
		VarDirectoryEntry varDirectoryEntry = GetVarDirectoryEntry(text);
		if (varDirectoryEntry != null)
		{
			if (!allowPackagePath)
			{
				return null;
			}
			string text2 = varDirectoryEntry.InternalSlashPath.Replace(input, string.Empty);
			if (varDirectoryEntry.InternalSlashPath != text2)
			{
				text2 = text2.Replace('/', '\\');
				return varDirectoryEntry.Package.SlashPath + ":/" + input + text2;
			}
		}
		else
		{
			string text3 = text.Replace(input, string.Empty);
			if (text != text3)
			{
				text3 = text3.Replace('/', '\\');
				return suggestedDir + text3;
			}
		}
		return null;
	}

	public static void SetLoadDir(string dir, bool restrictPath = false)
	{
		if (loadDirStack != null)
		{
			loadDirStack.Clear();
		}
		PushLoadDir(dir, restrictPath);
	}

	public static void PushLoadDir(string dir, bool restrictPath = false)
	{
		string text = dir.Replace('\\', '/');
		if (text != "/")
		{
			text = Regex.Replace(text, "/$", string.Empty);
		}
		if (restrictPath && !IsSecureReadPath(text))
		{
			throw new Exception("Attempted to push load dir for non-secure dir " + text);
		}
		if (loadDirStack == null)
		{
			loadDirStack = new Stack<string>();
		}
		loadDirStack.Push(text);
	}

	public static string PopLoadDir()
	{
		string result = null;
		if (loadDirStack != null)
		{
			result = loadDirStack.Pop();
		}
		return result;
	}

	public static void SetLoadDirFromFilePath(string path, bool restrictPath = false)
	{
		if (loadDirStack != null)
		{
			loadDirStack.Clear();
		}
		PushLoadDirFromFilePath(path, restrictPath);
	}

	public static void PushLoadDirFromFilePath(string path, bool restrictPath = false)
	{
		if (restrictPath && !IsSecureReadPath(path))
		{
			throw new Exception("Attempted to set load dir from non-secure path " + path);
		}
		FileEntry fileEntry = GetFileEntry(path);
		string dir;
		if (fileEntry != null)
		{
			if (fileEntry is VarFileEntry)
			{
				dir = Path.GetDirectoryName(fileEntry.Uid);
			}
			else
			{
				dir = Path.GetDirectoryName(fileEntry.FullPath);
				string oldValue = Path.GetFullPath(".") + "\\";
				dir = dir.Replace(oldValue, string.Empty);
			}
		}
		else
		{
			dir = Path.GetDirectoryName(GetFullPath(path));
			string oldValue2 = Path.GetFullPath(".") + "\\";
			dir = dir.Replace(oldValue2, string.Empty);
		}
		PushLoadDir(dir, restrictPath);
	}

	public static string PackageIDToPackageGroupID(string packageId)
	{
		string input = Regex.Replace(packageId, "\\.[0-9]+$", string.Empty);
		input = Regex.Replace(input, "\\.latest$", string.Empty);
		return Regex.Replace(input, "\\.min[0-9]+$", string.Empty);
	}

	public static string NormalizeID(string id)
	{
		string text = id;
		if (text.StartsWith("SELF:"))
		{
			string currentPackageUid = CurrentPackageUid;
			if (currentPackageUid != null)
			{
				return text.Replace("SELF:", currentPackageUid + ":");
			}
			return text.Replace("SELF:", string.Empty);
		}
		return NormalizeCommon(text);
	}

	protected static string NormalizeCommon(string path)
	{
		string text = path;
		Match match;
		if ((match = Regex.Match(text, "^(([^\\.]+\\.[^\\.]+)\\.latest):")).Success)
		{
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			VarPackageGroup packageGroup = GetPackageGroup(value2);
			if (packageGroup != null)
			{
				VarPackage newestEnabledPackage = packageGroup.NewestEnabledPackage;
				if (newestEnabledPackage != null)
				{
					text = text.Replace(value, newestEnabledPackage.Uid);
				}
			}
		}
		else if ((match = Regex.Match(text, "^(([^\\.]+\\.[^\\.]+)\\.min([0-9]+)):")).Success)
		{
			string value3 = match.Groups[1].Value;
			string value4 = match.Groups[2].Value;
			int requestVersion = int.Parse(match.Groups[3].Value);
			VarPackageGroup packageGroup2 = GetPackageGroup(value4);
			if (packageGroup2 != null)
			{
				VarPackage closestMatchingPackageVersion = packageGroup2.GetClosestMatchingPackageVersion(requestVersion);
				if (closestMatchingPackageVersion != null)
				{
					text = text.Replace(value3, closestMatchingPackageVersion.Uid);
				}
			}
		}
		else if ((match = Regex.Match(text, "^([^\\.]+\\.[^\\.]+\\.[0-9]+):")).Success)
		{
			string value5 = match.Groups[1].Value;
			VarPackage package = GetPackage(value5);
			if (package == null || !package.Enabled)
			{
				string packageGroupUid = PackageIDToPackageGroupID(value5);
				VarPackageGroup packageGroup3 = GetPackageGroup(packageGroupUid);
				if (packageGroup3 != null)
				{
					package = packageGroup3.NewestEnabledPackage;
					if (package != null)
					{
						text = text.Replace(value5, package.Uid);
					}
				}
			}
		}
		return text;
	}

	public static string NormalizeLoadPath(string path)
	{
		string result = path;
		if (path != null && path != string.Empty && path != "/" && path != "NULL")
		{
			result = path.Replace('\\', '/');
			string currentLoadDir = CurrentLoadDir;
			if (currentLoadDir != null && currentLoadDir != string.Empty)
			{
				if (!result.Contains("/"))
				{
					result = currentLoadDir + "/" + result;
				}
				else if (Regex.IsMatch(result, "^\\./"))
				{
					result = Regex.Replace(result, "^\\./", currentLoadDir + "/");
				}
			}
			if (result.StartsWith("SELF:/"))
			{
				string currentPackageUid = CurrentPackageUid;
				result = ((currentPackageUid == null) ? result.Replace("SELF:/", string.Empty) : result.Replace("SELF:/", currentPackageUid + ":/"));
			}
			else
			{
				result = NormalizeCommon(result);
			}
		}
		return result;
	}

	public static void SetSaveDir(string path, bool restrictPath = true)
	{
		if (path == null || path == string.Empty)
		{
			CurrentSaveDir = string.Empty;
			return;
		}
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (restrictPath && !IsSecureWritePath(path))
		{
			throw new Exception("Attempted to set save dir from non-secure path " + path);
		}
		string fullPath = GetFullPath(path);
		string oldValue = Path.GetFullPath(".") + "\\";
		fullPath = fullPath.Replace(oldValue, string.Empty);
		CurrentSaveDir = fullPath.Replace('\\', '/');
	}

	public static void SetSaveDirFromFilePath(string path, bool restrictPath = true)
	{
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (restrictPath && !IsSecureWritePath(path))
		{
			throw new Exception("Attempted to set save dir from non-secure path " + path);
		}
		string directoryName = Path.GetDirectoryName(GetFullPath(path));
		string oldValue = Path.GetFullPath(".") + "\\";
		directoryName = directoryName.Replace(oldValue, string.Empty);
		CurrentSaveDir = directoryName.Replace('\\', '/');
	}

	public static void SetNullSaveDir()
	{
		CurrentSaveDir = null;
	}

	public static string NormalizeSavePath(string path)
	{
		string text = path;
		if (path != null && path != string.Empty && path != "/" && path != "NULL")
		{
			string path2 = Regex.Replace(path, "^file:///", string.Empty);
			string fullPath = Path.GetFullPath(path2);
			string oldValue = Path.GetFullPath(".") + "\\";
			string text2 = fullPath.Replace(oldValue, string.Empty);
			if (text2 != fullPath)
			{
				text = text2;
			}
			text = text.Replace('\\', '/');
			string fileName = Path.GetFileName(text2);
			string text3 = Path.GetDirectoryName(text2);
			if (text3 != null)
			{
				text3 = text3.Replace('\\', '/');
			}
			if (CurrentSaveDir == text3)
			{
				text = fileName;
			}
			else if (CurrentSaveDir != null && CurrentSaveDir != string.Empty && Regex.IsMatch(text3, "^" + CurrentSaveDir + "/"))
			{
				text = text3.Replace(CurrentSaveDir, ".") + "/" + fileName;
			}
		}
		return text;
	}

	public static List<VarPackage> GetPackages()
	{
		List<VarPackage> list = null;
		if (packagesByUid != null)
		{
			return packagesByUid.Values.ToList();
		}
		return new List<VarPackage>();
	}

	public static List<string> GetPackageUids()
	{
		List<string> list = null;
		if (packagesByUid != null)
		{
			list = packagesByUid.Keys.ToList();
			list.Sort();
		}
		else
		{
			list = new List<string>();
		}
		return list;
	}

	public static bool IsPackage(string packageUidOrPath)
	{
		if (packagesByUid != null && packagesByUid.ContainsKey(packageUidOrPath))
		{
			return true;
		}
		if (packagesByPath != null && packagesByPath.ContainsKey(packageUidOrPath))
		{
			return true;
		}
		return false;
	}

	public static VarPackage GetPackage(string packageUidOrPath)
	{
		VarPackage value = null;
		Match match;
		if ((match = Regex.Match(packageUidOrPath, "^([^\\.]+\\.[^\\.]+)\\.latest$")).Success)
		{
			string value2 = match.Groups[1].Value;
			VarPackageGroup packageGroup = GetPackageGroup(value2);
			if (packageGroup != null)
			{
				value = packageGroup.NewestPackage;
			}
		}
		else if ((match = Regex.Match(packageUidOrPath, "^([^\\.]+\\.[^\\.]+)\\.min([0-9]+)$")).Success)
		{
			string value3 = match.Groups[1].Value;
			int requestVersion = int.Parse(match.Groups[2].Value);
			VarPackageGroup packageGroup2 = GetPackageGroup(value3);
			if (packageGroup2 != null)
			{
				value = packageGroup2.GetClosestMatchingPackageVersion(requestVersion, onlyUseEnabledPackages: false);
			}
		}
		else if (packagesByUid != null && packagesByUid.ContainsKey(packageUidOrPath))
		{
			packagesByUid.TryGetValue(packageUidOrPath, out value);
		}
		else if (packagesByPath != null && packagesByPath.ContainsKey(packageUidOrPath))
		{
			packagesByPath.TryGetValue(packageUidOrPath, out value);
		}
		return value;
	}

	public static List<VarPackageGroup> GetPackageGroups()
	{
		List<VarPackageGroup> list = null;
		if (packageGroups != null)
		{
			return packageGroups.Values.ToList();
		}
		return new List<VarPackageGroup>();
	}

	public static VarPackageGroup GetPackageGroup(string packageGroupUid)
	{
		VarPackageGroup value = null;
		if (packageGroups != null)
		{
			packageGroups.TryGetValue(packageGroupUid, out value);
		}
		return value;
	}

	public static string CleanFilePath(string path)
	{
		return path?.Replace('\\', '/');
	}

	public static void FindAllFiles(string dir, string pattern, List<FileEntry> foundFiles, bool restrictPath = false)
	{
		FindRegularFiles(dir, pattern, foundFiles, restrictPath);
		FindVarFiles(dir, pattern, foundFiles);
	}

	public static void FindAllFilesRegex(string dir, string regex, List<FileEntry> foundFiles, bool restrictPath = false)
	{
		FindRegularFilesRegex(dir, regex, foundFiles, restrictPath);
		FindVarFilesRegex(dir, regex, foundFiles);
	}

	public static void FindRegularFiles(string dir, string pattern, List<FileEntry> foundFiles, bool restrictPath = false)
	{
		if (Directory.Exists(dir))
		{
			if (restrictPath && !IsSecureReadPath(dir))
			{
				throw new Exception("Attempted to find files for non-secure path " + dir);
			}
			string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
			FindRegularFilesRegex(dir, regex, foundFiles, restrictPath);
		}
	}

	public static void FindRegularFilesRegex(string dir, string regex, List<FileEntry> foundFiles, bool restrictPath = false)
	{
		dir = CleanDirectoryPath(dir);
		if (!Directory.Exists(dir))
		{
			return;
		}
		if (restrictPath && !IsSecureReadPath(dir))
		{
			throw new Exception("Attempted to find files for non-secure path " + dir);
		}
		string[] files = Directory.GetFiles(dir);
		foreach (string text in files)
		{
			if (Regex.IsMatch(text, regex))
			{
				SystemFileEntry systemFileEntry = new SystemFileEntry(text);
				if (systemFileEntry.Exists)
				{
					foundFiles.Add(systemFileEntry);
				}
				else
				{
					Debug.LogError("Error in lookup SystemFileEntry for " + text);
				}
			}
		}
		string[] directories = Directory.GetDirectories(dir);
		foreach (string dir2 in directories)
		{
			FindRegularFilesRegex(dir2, regex, foundFiles);
		}
	}

	public static void FindVarFiles(string dir, string pattern, List<FileEntry> foundFiles)
	{
		if (allVarFileEntries != null)
		{
			string regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
			FindVarFilesRegex(dir, regex, foundFiles);
		}
	}

	public static void FindVarFilesRegex(string dir, string regex, List<FileEntry> foundFiles)
	{
		dir = CleanDirectoryPath(dir);
		if (allVarFileEntries == null)
		{
			return;
		}
		foreach (VarFileEntry allVarFileEntry in allVarFileEntries)
		{
			if (allVarFileEntry.InternalSlashPath.StartsWith(dir) && Regex.IsMatch(allVarFileEntry.Name, regex))
			{
				foundFiles.Add(allVarFileEntry);
			}
		}
	}

	public static bool FileExists(string path, bool onlySystemFiles = false, bool restrictPath = false)
	{
		if (path != null && path != string.Empty)
		{
			if (!onlySystemFiles)
			{
				string key = CleanFilePath(path);
				if (uidToVarFileEntry != null && uidToVarFileEntry.ContainsKey(path))
				{
					return true;
				}
				if (pathToVarFileEntry != null && pathToVarFileEntry.ContainsKey(key))
				{
					return true;
				}
			}
			if (File.Exists(path))
			{
				if (restrictPath && !IsSecureReadPath(path))
				{
					throw new Exception("Attempted to check file existence for non-secure path " + path);
				}
				return true;
			}
		}
		return false;
	}

	public static bool IsFileInPackage(string path)
	{
		string key = CleanFilePath(path);
		if (uidToVarFileEntry != null && uidToVarFileEntry.ContainsKey(key))
		{
			return true;
		}
		if (pathToVarFileEntry != null && pathToVarFileEntry.ContainsKey(key))
		{
			return true;
		}
		return false;
	}

	public static FileEntry GetFileEntry(string path, bool restrictPath = false)
	{
		FileEntry fileEntry = null;
		fileEntry = GetVarFileEntry(path);
		if (fileEntry == null)
		{
			fileEntry = GetSystemFileEntry(path, restrictPath);
		}
		return fileEntry;
	}

	public static SystemFileEntry GetSystemFileEntry(string path, bool restrictPath = false)
	{
		SystemFileEntry result = null;
		if (File.Exists(path))
		{
			if (restrictPath && !IsSecureReadPath(path))
			{
				throw new Exception("Attempted to get file entry for non-secure path " + path);
			}
			result = new SystemFileEntry(path);
		}
		return result;
	}

	public static VarFileEntry GetVarFileEntry(string path)
	{
		VarFileEntry value = null;
		string key = CleanFilePath(path);
		if ((uidToVarFileEntry != null && uidToVarFileEntry.TryGetValue(key, out value)) || pathToVarFileEntry == null || pathToVarFileEntry.TryGetValue(key, out value))
		{
		}
		return value;
	}

	public static void SortFileEntriesByLastWriteTime(List<FileEntry> fileEntries)
	{
		fileEntries.Sort((FileEntry e1, FileEntry e2) => e1.LastWriteTime.CompareTo(e2.LastWriteTime));
	}

	public static string CleanDirectoryPath(string path)
	{
		if (path != null)
		{
			string input = path.Replace('\\', '/');
			return Regex.Replace(input, "/$", string.Empty);
		}
		return null;
	}

	public static int FolderContentsCount(string path)
	{
		int num = Directory.GetFiles(path).Length;
		string[] directories = Directory.GetDirectories(path);
		string[] array = directories;
		foreach (string path2 in array)
		{
			num += FolderContentsCount(path2);
		}
		return num;
	}

	public static List<VarDirectoryEntry> FindVarDirectories(string dir, bool exactMatch = true)
	{
		dir = CleanDirectoryPath(dir);
		List<VarDirectoryEntry> list = new List<VarDirectoryEntry>();
		if (allVarDirectoryEntries != null)
		{
			foreach (VarDirectoryEntry allVarDirectoryEntry in allVarDirectoryEntries)
			{
				if (exactMatch)
				{
					if (allVarDirectoryEntry.InternalSlashPath == dir)
					{
						list.Add(allVarDirectoryEntry);
					}
				}
				else if (allVarDirectoryEntry.InternalSlashPath.StartsWith(dir))
				{
					list.Add(allVarDirectoryEntry);
				}
			}
		}
		return list;
	}

	public static List<ShortCut> GetShortCutsForDirectory(string dir, bool allowNavigationAboveRegularDirectories = false, bool useFullPaths = false, bool generateAllFlattenedShortcut = false, bool includeRegularDirsInFlattenedShortcut = false)
	{
		dir = Regex.Replace(dir, ".*:\\\\", string.Empty);
		string text = dir.TrimEnd('/', '\\');
		text = text.Replace('\\', '/');
		List<VarDirectoryEntry> list = FindVarDirectories(text);
		List<ShortCut> list2 = new List<ShortCut>();
		if (DirectoryExists(text))
		{
			ShortCut shortCut = new ShortCut();
			shortCut.package = string.Empty;
			if (allowNavigationAboveRegularDirectories)
			{
				text = text.Replace('/', '\\');
				if (useFullPaths)
				{
					shortCut.path = Path.GetFullPath(text);
				}
				else
				{
					shortCut.path = text;
				}
			}
			else
			{
				shortCut.path = text;
			}
			shortCut.displayName = text;
			list2.Add(shortCut);
		}
		if (list.Count > 0)
		{
			if (generateAllFlattenedShortcut)
			{
				ShortCut shortCut2 = new ShortCut();
				shortCut2.path = text;
				shortCut2.displayName = "From: " + text;
				shortCut2.flatten = true;
				if (includeRegularDirsInFlattenedShortcut)
				{
					shortCut2.package = "All Flattened";
					shortCut2.includeRegularDirsInFlatten = true;
				}
				else
				{
					shortCut2.package = "AddonPackages Flattened";
				}
				list2.Add(shortCut2);
			}
			ShortCut shortCut3 = new ShortCut();
			shortCut3.package = "AddonPackages Filtered";
			shortCut3.path = "AddonPackages";
			shortCut3.displayName = "Filter: " + text;
			shortCut3.packageFilter = text;
			list2.Add(shortCut3);
		}
		foreach (VarDirectoryEntry item in list)
		{
			ShortCut shortCut4 = new ShortCut();
			shortCut4.isLatest = item.Package.isNewestEnabledVersion;
			shortCut4.package = item.Package.Uid;
			shortCut4.displayName = item.InternalSlashPath;
			shortCut4.path = item.SlashPath;
			list2.Add(shortCut4);
		}
		return list2;
	}

	public static bool DirectoryExists(string path, bool onlySystemDirectories = false, bool restrictPath = false)
	{
		if (path != null && path != string.Empty)
		{
			if (!onlySystemDirectories)
			{
				string key = CleanDirectoryPath(path);
				if (uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.ContainsKey(key))
				{
					return true;
				}
				if (pathToVarDirectoryEntry != null && pathToVarDirectoryEntry.ContainsKey(key))
				{
					return true;
				}
			}
			if (Directory.Exists(path))
			{
				if (restrictPath && !IsSecureReadPath(path))
				{
					throw new Exception("Attempted to check file existence for non-secure path " + path);
				}
				return true;
			}
		}
		return false;
	}

	public static bool IsDirectoryInPackage(string path)
	{
		string key = CleanDirectoryPath(path);
		if (uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.ContainsKey(key))
		{
			return true;
		}
		if (pathToVarDirectoryEntry != null && pathToVarDirectoryEntry.ContainsKey(key))
		{
			return true;
		}
		return false;
	}

	public static DirectoryEntry GetDirectoryEntry(string path, bool restrictPath = false)
	{
		string path2 = Regex.Replace(path, "(/|\\\\)$", string.Empty);
		DirectoryEntry directoryEntry = null;
		directoryEntry = GetVarDirectoryEntry(path2);
		if (directoryEntry == null)
		{
			directoryEntry = GetSystemDirectoryEntry(path2, restrictPath);
		}
		return directoryEntry;
	}

	public static SystemDirectoryEntry GetSystemDirectoryEntry(string path, bool restrictPath = false)
	{
		SystemDirectoryEntry result = null;
		if (Directory.Exists(path))
		{
			if (restrictPath && !IsSecureReadPath(path))
			{
				throw new Exception("Attempted to get directory entry for non-secure path " + path);
			}
			result = new SystemDirectoryEntry(path);
		}
		return result;
	}

	public static VarDirectoryEntry GetVarDirectoryEntry(string path)
	{
		VarDirectoryEntry value = null;
		string key = CleanDirectoryPath(path);
		if ((uidToVarDirectoryEntry != null && uidToVarDirectoryEntry.TryGetValue(key, out value)) || pathToVarDirectoryEntry == null || pathToVarDirectoryEntry.TryGetValue(key, out value))
		{
		}
		return value;
	}

	public static VarDirectoryEntry GetVarRootDirectoryEntryFromPath(string path)
	{
		VarDirectoryEntry value = null;
		if (varPackagePathToRootVarDirectory != null)
		{
			varPackagePathToRootVarDirectory.TryGetValue(path, out value);
		}
		return value;
	}

	public static string[] GetDirectories(string dir, string pattern = null, bool restrictPath = false)
	{
		if (restrictPath && !IsSecureReadPath(dir))
		{
			throw new Exception("Attempted to get directories at non-secure path " + dir);
		}
		List<string> list = new List<string>();
		DirectoryEntry directoryEntry = GetDirectoryEntry(dir, restrictPath);
		if (directoryEntry == null)
		{
			throw new Exception("Attempted to get directories at non-existent path " + dir);
		}
		string text = null;
		if (pattern != null)
		{
			text = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
		}
		foreach (DirectoryEntry subDirectory in directoryEntry.SubDirectories)
		{
			if (text == null || Regex.IsMatch(subDirectory.Name, text))
			{
				list.Add(dir + "\\" + subDirectory.Name);
			}
		}
		return list.ToArray();
	}

	public static string[] GetFiles(string dir, string pattern = null, bool restrictPath = false)
	{
		if (restrictPath && !IsSecureReadPath(dir))
		{
			throw new Exception("Attempted to get files at non-secure path " + dir);
		}
		List<string> list = new List<string>();
		DirectoryEntry directoryEntry = GetDirectoryEntry(dir, restrictPath);
		if (directoryEntry == null)
		{
			throw new Exception("Attempted to get files at non-existent path " + dir);
		}
		foreach (FileEntry file in directoryEntry.GetFiles(pattern))
		{
			list.Add(dir + "\\" + file.Name);
		}
		return list.ToArray();
	}

	public static void CreateDirectory(string path)
	{
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (!IsSecureWritePath(path))
		{
			throw new Exception("Attempted to create directory at non-secure path " + path);
		}
		Directory.CreateDirectory(path);
	}

	public static void DeleteDirectory(string path, bool recursive = false)
	{
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (!IsSecureWritePath(path))
		{
			throw new Exception("Attempted to delete file at non-secure path " + path);
		}
		if (Directory.Exists(path))
		{
			Directory.Delete(path, recursive);
		}
	}

	public static void MoveDirectory(string oldPath, string newPath)
	{
		oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
		if (!IsSecureWritePath(oldPath))
		{
			throw new Exception("Attempted to move directory from non-secure path " + oldPath);
		}
		newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
		if (!IsSecureWritePath(newPath))
		{
			throw new Exception("Attempted to move directory to non-secure path " + newPath);
		}
		Directory.Move(oldPath, newPath);
	}

	public static FileEntryStream OpenStream(FileEntry fe)
	{
		if (fe == null)
		{
			throw new Exception("Null FileEntry passed to OpenStreamReader");
		}
		if (fe is VarFileEntry)
		{
			return new VarFileEntryStream(fe as VarFileEntry);
		}
		if (fe is SystemFileEntry)
		{
			return new SystemFileEntryStream(fe as SystemFileEntry);
		}
		throw new Exception("Unknown FileEntry class passed to OpenStreamReader");
	}

	public static FileEntryStream OpenStream(string path, bool restrictPath = false)
	{
		FileEntry fileEntry = GetFileEntry(path, restrictPath);
		if (fileEntry == null)
		{
			throw new Exception("Path " + path + " not found");
		}
		return OpenStream(fileEntry);
	}

	public static FileEntryStreamReader OpenStreamReader(FileEntry fe)
	{
		if (fe == null)
		{
			throw new Exception("Null FileEntry passed to OpenStreamReader");
		}
		if (fe is VarFileEntry)
		{
			return new VarFileEntryStreamReader(fe as VarFileEntry);
		}
		if (fe is SystemFileEntry)
		{
			return new SystemFileEntryStreamReader(fe as SystemFileEntry);
		}
		throw new Exception("Unknown FileEntry class passed to OpenStreamReader");
	}

	public static FileEntryStreamReader OpenStreamReader(string path, bool restrictPath = false)
	{
		FileEntry fileEntry = GetFileEntry(path, restrictPath);
		if (fileEntry == null)
		{
			throw new Exception("Path " + path + " not found");
		}
		return OpenStreamReader(fileEntry);
	}

	public static byte[] ReadAllBytes(string path, bool restrictPath = false)
	{
		FileEntry fileEntry = GetFileEntry(path, restrictPath);
		if (fileEntry == null)
		{
			throw new Exception("Path " + path + " not found");
		}
		return ReadAllBytes(fileEntry);
	}

	public static byte[] ReadAllBytes(FileEntry fe)
	{
		byte[] buffer = new byte[4096];
		using FileEntryStream fileEntryStream = OpenStream(fe);
		using MemoryStream memoryStream = new MemoryStream();
		StreamUtils.Copy(fileEntryStream.Stream, memoryStream, buffer);
		return memoryStream.ToArray();
	}

	public static string ReadAllText(string path, bool restrictPath = false)
	{
		FileEntry fileEntry = GetFileEntry(path, restrictPath);
		if (fileEntry == null)
		{
			throw new Exception("Path " + path + " not found");
		}
		return ReadAllText(fileEntry);
	}

	public static string ReadAllText(FileEntry fe)
	{
		using FileEntryStreamReader fileEntryStreamReader = OpenStreamReader(fe);
		return fileEntryStreamReader.ReadToEnd();
	}

	public static FileStream OpenStreamForCreate(string path)
	{
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (!IsSecureWritePath(path))
		{
			throw new Exception("Attempted to open stream for create at non-secure path " + path);
		}
		return File.Open(path, FileMode.Create);
	}

	public static StreamWriter OpenStreamWriter(string path)
	{
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (!IsSecureWritePath(path))
		{
			throw new Exception("Attempted to open stream writer at non-secure path " + path);
		}
		return new StreamWriter(path);
	}

	public static void WriteAllText(string path, string text)
	{
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (!IsSecureWritePath(path))
		{
			throw new Exception("Attempted to write all text at non-secure path " + path);
		}
		File.WriteAllText(path, text);
	}

	public static void WriteAllBytes(string path, byte[] bytes)
	{
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (!IsSecureWritePath(path))
		{
			throw new Exception("Attempted to write all bytes at non-secure path " + path);
		}
		File.WriteAllBytes(path, bytes);
	}

	public static void SetFileAttributes(string path, FileAttributes attrs)
	{
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (!IsSecureWritePath(path))
		{
			throw new Exception("Attempted to set file attributes at non-secure path " + path);
		}
		File.SetAttributes(path, attrs);
	}

	public static void DeleteFile(string path)
	{
		path = ConvertSimulatedPackagePathToNormalPath(path);
		if (!IsSecureWritePath(path))
		{
			throw new Exception("Attempted to delete file at non-secure path " + path);
		}
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public static void CopyFile(string oldPath, string newPath, bool restrictPath = false)
	{
		oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
		if (restrictPath && !IsSecureReadPath(oldPath))
		{
			throw new Exception("Attempted to copy file from non-secure path " + oldPath);
		}
		newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
		if (!IsSecureWritePath(newPath))
		{
			throw new Exception("Attempted to copy file to non-secure path " + newPath);
		}
		FileEntry fileEntry = GetFileEntry(oldPath);
		if (fileEntry != null && fileEntry is VarFileEntry)
		{
			byte[] buffer = new byte[4096];
			using FileEntryStream fileEntryStream = OpenStream(fileEntry);
			using FileStream destination = OpenStreamForCreate(newPath);
			StreamUtils.Copy(fileEntryStream.Stream, destination, buffer);
			return;
		}
		File.Copy(oldPath, newPath);
	}

	public static void MoveFile(string oldPath, string newPath)
	{
		oldPath = ConvertSimulatedPackagePathToNormalPath(oldPath);
		if (!IsSecureWritePath(oldPath))
		{
			throw new Exception("Attempted to move file from non-secure path " + oldPath);
		}
		newPath = ConvertSimulatedPackagePathToNormalPath(newPath);
		if (!IsSecureWritePath(newPath))
		{
			throw new Exception("Attempted to move file to non-secure path " + newPath);
		}
		if (File.Exists(newPath))
		{
			File.Delete(newPath);
		}
		File.Move(oldPath, newPath);
	}

	private void Awake()
	{
		Refresh();
	}
}
