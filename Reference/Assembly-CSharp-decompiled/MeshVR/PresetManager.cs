using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace MeshVR;

public class PresetManager : MonoBehaviour
{
	[Serializable]
	public class SpecificStorable
	{
		public Transform specificStorableBucket;

		public string storeId;

		public string specificKey;

		public bool isSpecificKeyAnObject;

		public bool includeChildren;
	}

	public class Storable
	{
		public JSONStorable storable;

		public string specificKey;

		public bool isSpecificKeyAnObject;
	}

	public enum ItemType
	{
		None,
		Custom,
		ClothingFemale,
		ClothingMale,
		ClothingNeutral,
		Atom,
		HairFemale,
		HairMale,
		HairNeutral
	}

	public UnityEvent postLoadEvent;

	public ItemType itemType;

	public string storedCreatorName;

	public string creatorName;

	public string storeFolderName;

	public string storeName;

	protected string _presetName;

	protected string presetPackageName = string.Empty;

	protected string presetPackagePath = string.Empty;

	protected string presetSubPath;

	protected string presetSubName;

	public string package = string.Empty;

	public string customPath = string.Empty;

	public bool storeOptionalStorables;

	public bool storePresetBinary;

	public bool setOptionalToDefaultOnRestore;

	public bool useTransformAndChildren = true;

	public bool includeOptional = true;

	public bool includePhysical = true;

	public bool includeAppearance = true;

	public SpecificStorable[] optionalSpecificStorables;

	public SpecificStorable[] specificStorables;

	public Transform[] dynamicStorablesBuckets;

	protected bool ignoreExclude;

	protected string storeRoot = "Custom/";

	protected List<Storable> storables;

	protected List<Storable> optionalStorables;

	protected List<Storable> dynamicStorables;

	protected Dictionary<JSONStorable, bool> regularStorables;

	protected Dictionary<string, bool> specificKeyStorables;

	public JSONClass lastLoadedJSON;

	public JSONClass filteredJSON;

	protected bool setUnlistedParamsToDefault = true;

	protected bool setUnlistedDynamicStorableParamsToDefault = true;

	public string presetName
	{
		get
		{
			return _presetName;
		}
		set
		{
			if (!(_presetName != value))
			{
				return;
			}
			_presetName = value;
			if (_presetName == null)
			{
				presetPackageName = string.Empty;
				presetPackagePath = string.Empty;
				presetSubName = string.Empty;
				presetSubPath = string.Empty;
				return;
			}
			string text = _presetName;
			if (_presetName.Contains(":"))
			{
				string[] array = _presetName.Split(':');
				presetPackageName = array[0];
				presetPackagePath = array[0] + ":/";
				text = array[1];
			}
			else
			{
				presetPackageName = string.Empty;
				presetPackagePath = string.Empty;
			}
			if (text.Contains("/"))
			{
				presetSubPath = Path.GetDirectoryName(text) + "/";
				presetSubName = Path.GetFileName(text);
			}
			else
			{
				presetSubPath = string.Empty;
				presetSubName = text;
			}
		}
	}

	public bool IsInPackage()
	{
		return package != string.Empty;
	}

	public void SetIgnoreExclude(bool b)
	{
		ignoreExclude = b;
	}

	public string GetStoreRootPath(bool includePackage = true)
	{
		string result = null;
		string text = string.Empty;
		if (package != string.Empty && includePackage)
		{
			text = package + ":/";
		}
		switch (itemType)
		{
			case ItemType.None:
				result = text + storeRoot;
				break;
			case ItemType.Custom:
				result = text + storeRoot + customPath;
				break;
			case ItemType.ClothingFemale:
				result = text + storeRoot + "Clothing/Female/";
				break;
			case ItemType.ClothingMale:
				result = text + storeRoot + "Clothing/Male/";
				break;
			case ItemType.ClothingNeutral:
				result = text + storeRoot + "Clothing/Neutral/";
				break;
			case ItemType.Atom:
				result = text + storeRoot + "Atom/" + customPath;
				break;
			case ItemType.HairFemale:
				result = text + storeRoot + "Hair/Female/";
				break;
			case ItemType.HairMale:
				result = text + storeRoot + "Hair/Male/";
				break;
			case ItemType.HairNeutral:
				result = text + storeRoot + "Hair/Neutral/";
				break;
		}
		return result;
	}

	public List<string> FindFavoriteNames()
	{
		List<string> list = new List<string>();
		if (itemType != 0)
		{
			string storeFolderPath = GetStoreFolderPath(includePackage: false);
			if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && Directory.Exists(storeFolderPath))
			{
				string[] files = Directory.GetFiles(storeFolderPath, storeName + "_*.vap.fav", SearchOption.AllDirectories);
				string[] array = files;
				foreach (string input in array)
				{
					string text = Regex.Replace(input, "\\.fav$", string.Empty);
					text = text.Replace("\\", "/");
					string presetNameFromFilePath = GetPresetNameFromFilePath(text);
					if (presetNameFromFilePath != null)
					{
						list.Add(presetNameFromFilePath);
					}
				}
			}
		}
		return list;
	}

	public string[] PathToNames(string inpath)
	{
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string text4 = string.Empty;
		string text5 = ":/";
		string text6;
		if (inpath.Contains(text5))
		{
			string[] array = inpath.Split(new string[1] { text5 }, StringSplitOptions.None);
			text4 = array[0];
			text6 = array[1];
		}
		else
		{
			text6 = inpath;
		}
		if (text6 != null && text6 != string.Empty)
		{
			text = Path.GetFileName(text6);
			if (text != null && text != string.Empty)
			{
				text = Regex.Replace(text, "\\.(vap|vam|vaj|vab)$", string.Empty);
				string directoryName = Path.GetDirectoryName(text6);
				if (directoryName != null && directoryName != string.Empty)
				{
					string text7 = Regex.Replace(directoryName, "^" + GetStoreRootPath(includePackage: false), string.Empty);
					if (text7.Contains("/"))
					{
						text3 = Regex.Replace(text7, "/.*", string.Empty);
						text2 = Regex.Replace(text7, text3 + "/", string.Empty);
					}
					else
					{
						text2 = text7;
					}
				}
			}
		}
		return new string[4] { text2, text3, text, text4 };
	}

	public void SetNamesFromPath(string path)
	{
		string[] array = PathToNames(path);
		storeFolderName = array[0];
		creatorName = array[1];
		storeName = array[2];
		package = array[3];
	}

	public string GetStoreFolderPath(bool includePackage = true)
	{
		string text = null;
		string storeRootPath = GetStoreRootPath(includePackage);
		if (storeRootPath != null)
		{
			text = storeRootPath;
			if (creatorName != null && creatorName != string.Empty)
			{
				text = text + creatorName + "/";
			}
			if (storeFolderName != null && storeFolderName != string.Empty)
			{
				string text2 = Regex.Replace(storeFolderName, "/$", string.Empty);
				text = text + text2 + "/";
			}
		}
		return text;
	}

	public string GetStorePathBase()
	{
		string storeFolderPath = GetStoreFolderPath();
		return storeFolderPath + storeName;
	}

	public string GetPresetNameFromFilePath(string fpath)
	{
		VarFileEntry varFileEntry = FileManager.GetVarFileEntry(fpath);
		string text = string.Empty;
		string text2 = fpath;
		if (varFileEntry != null)
		{
			text = varFileEntry.Package.Uid + ":";
			text2 = varFileEntry.InternalSlashPath;
		}
		string storeFolderPath = GetStoreFolderPath(includePackage: false);
		string text3 = text2.Replace(storeFolderPath, string.Empty);
		string result = null;
		if (text3 == text2)
		{
			SuperController.LogError("Preset path " + fpath + " is not compatible with store folder path " + storeFolderPath);
		}
		else
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			if (text3.Contains("/"))
			{
				empty = Path.GetDirectoryName(text3) + "/";
				empty2 = Path.GetFileName(text3);
			}
			else
			{
				empty = string.Empty;
				empty2 = text3;
			}
			string text4 = empty2.Replace(storeName + "_", string.Empty);
			if (text4 == empty2)
			{
				SuperController.LogError("Preset " + empty2 + " is not a preset for current store " + storeName);
			}
			else
			{
				if (text4.Contains("__"))
				{
					text = Regex.Replace(text4, "__.*", string.Empty);
					if (FileManager.IsPackage(text))
					{
						text += ":";
						text4 = Regex.Replace(text4, ".*__", string.Empty);
					}
				}
				result = text4.Replace(".vap", string.Empty);
				result = text + empty + result;
			}
		}
		return result;
	}

	protected void StoreStorablesInList(JSONClass jc, List<Storable> sts)
	{
		if (sts == null)
		{
			return;
		}
		foreach (Storable st in sts)
		{
			JSONStorable storable = st.storable;
			if ((!ignoreExclude && storable.exclude) || !storable.gameObject.activeInHierarchy)
			{
				continue;
			}
			JSONClass jSON = storable.GetJSON(includePhysical, includeAppearance, forceStore: true);
			if (st.specificKey != null && st.specificKey != string.Empty)
			{
				JSONClass jSONClass = new JSONClass();
				foreach (string key in jSON.Keys)
				{
					if (key == st.specificKey || key == "id")
					{
						jSONClass[key] = jSON[key];
					}
				}
				jc["storables"].Add(jSONClass);
			}
			else
			{
				jc["storables"].Add(jSON);
			}
		}
	}

	protected virtual void StorePresetBinary()
	{
	}

	protected void StoreStorables(JSONClass jc)
	{
		if (jc != null)
		{
			RefreshDynamicStorables();
			jc["storables"] = new JSONArray();
			if (includeOptional && storeOptionalStorables)
			{
				StoreStorablesInList(jc, optionalStorables);
			}
			StoreStorablesInList(jc, storables);
			StoreStorablesInList(jc, dynamicStorables);
		}
	}

	protected void PreRestore()
	{
		if (includeOptional && optionalStorables != null)
		{
			foreach (Storable optionalStorable in optionalStorables)
			{
				JSONStorable storable = optionalStorable.storable;
				storable.PreRestore();
			}
		}
		if (storables != null)
		{
			foreach (Storable storable4 in storables)
			{
				JSONStorable storable2 = storable4.storable;
				storable2.PreRestore();
			}
		}
		if (dynamicStorables == null)
		{
			return;
		}
		foreach (Storable dynamicStorable in dynamicStorables)
		{
			JSONStorable storable3 = dynamicStorable.storable;
			storable3.PreRestore();
		}
	}

	protected void Restore(JSONClass jc)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		Dictionary<string, JSONStorable> dictionary2 = new Dictionary<string, JSONStorable>();
		if (optionalStorables != null)
		{
			foreach (Storable optionalStorable in optionalStorables)
			{
				JSONStorable storable = optionalStorable.storable;
				if ((ignoreExclude || !storable.exclude) && !dictionary2.ContainsKey(storable.storeId))
				{
					if (includeOptional)
					{
						dictionary2.Add(storable.storeId, storable);
					}
					if (!setOptionalToDefaultOnRestore && !dictionary.ContainsKey(storable.storeId))
					{
						dictionary.Add(storable.storeId, value: true);
					}
				}
			}
		}
		if (storables != null)
		{
			foreach (Storable storable6 in storables)
			{
				JSONStorable storable2 = storable6.storable;
				if ((ignoreExclude || !storable2.exclude) && !dictionary2.ContainsKey(storable2.storeId))
				{
					dictionary2.Add(storable2.storeId, storable2);
				}
			}
		}
		if (dynamicStorables != null)
		{
			foreach (Storable dynamicStorable in dynamicStorables)
			{
				JSONStorable storable3 = dynamicStorable.storable;
				if ((ignoreExclude || !storable3.exclude) && (!storable3.onlyStoreIfActive || storable3.gameObject.activeInHierarchy) && !dictionary2.ContainsKey(storable3.storeId))
				{
					dictionary2.Add(storable3.storeId, storable3);
				}
			}
		}
		foreach (JSONClass item in jc["storables"].AsArray)
		{
			string key = item["id"];
			if (dictionary2.TryGetValue(key, out var value))
			{
				if (!specificKeyStorables.TryGetValue(key, out var value2))
				{
					value2 = false;
				}
				value.isPresetRestore = true;
				value.RestoreFromJSON(item, includePhysical, includeAppearance, null, !value2 && setUnlistedParamsToDefault);
				value.isPresetRestore = false;
				if (!dictionary.ContainsKey(item["id"]))
				{
					dictionary.Add(item["id"], value: true);
				}
			}
		}
		JSONClass jc2 = new JSONClass();
		if (!setUnlistedParamsToDefault)
		{
			return;
		}
		foreach (Storable storable7 in storables)
		{
			JSONStorable storable4 = storable7.storable;
			if (!storable4.exclude && !dictionary.ContainsKey(storable4.storeId) && !specificKeyStorables.ContainsKey(storable4.storeId))
			{
				storable4.isPresetRestore = true;
				storable4.RestoreFromJSON(jc2, includePhysical, includeAppearance);
				storable4.isPresetRestore = false;
			}
		}
		foreach (Storable dynamicStorable2 in dynamicStorables)
		{
			JSONStorable storable5 = dynamicStorable2.storable;
			if (!storable5.exclude && (!storable5.onlyStoreIfActive || storable5.gameObject.activeInHierarchy) && !dictionary.ContainsKey(storable5.storeId))
			{
				storable5.isPresetRestore = true;
				storable5.RestoreFromJSON(jc2, includePhysical, includeAppearance);
				storable5.isPresetRestore = false;
			}
		}
	}

	protected void LateRestore(JSONClass jc)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		Dictionary<string, JSONStorable> dictionary2 = new Dictionary<string, JSONStorable>();
		if (optionalStorables != null)
		{
			foreach (Storable optionalStorable in optionalStorables)
			{
				JSONStorable storable = optionalStorable.storable;
				if ((ignoreExclude || !storable.exclude) && !dictionary2.ContainsKey(storable.storeId))
				{
					if (includeOptional)
					{
						dictionary2.Add(storable.storeId, storable);
					}
					if (!setOptionalToDefaultOnRestore && !dictionary.ContainsKey(storable.storeId))
					{
						dictionary.Add(storable.storeId, value: true);
					}
				}
			}
		}
		if (storables != null)
		{
			foreach (Storable storable6 in storables)
			{
				JSONStorable storable2 = storable6.storable;
				if ((ignoreExclude || !storable2.exclude) && !dictionary2.ContainsKey(storable2.storeId))
				{
					dictionary2.Add(storable2.storeId, storable2);
				}
			}
		}
		if (dynamicStorables != null)
		{
			foreach (Storable dynamicStorable in dynamicStorables)
			{
				JSONStorable storable3 = dynamicStorable.storable;
				if ((ignoreExclude || !storable3.exclude) && (!storable3.onlyStoreIfActive || storable3.gameObject.activeInHierarchy) && !dictionary2.ContainsKey(storable3.storeId))
				{
					dictionary2.Add(storable3.storeId, storable3);
				}
			}
		}
		foreach (JSONClass item in jc["storables"].AsArray)
		{
			string key = item["id"];
			if (dictionary2.TryGetValue(key, out var value))
			{
				value.isPresetRestore = true;
				value.LateRestoreFromJSON(item, includePhysical, includeAppearance);
				value.isPresetRestore = false;
				if (!dictionary.ContainsKey(item["id"]))
				{
					dictionary.Add(item["id"], value: true);
				}
			}
		}
		JSONClass jc2 = new JSONClass();
		if (!setUnlistedParamsToDefault)
		{
			return;
		}
		foreach (Storable storable7 in storables)
		{
			JSONStorable storable4 = storable7.storable;
			if (!storable4.exclude && !dictionary.ContainsKey(storable4.storeId) && !specificKeyStorables.ContainsKey(storable4.storeId))
			{
				storable4.isPresetRestore = true;
				storable4.LateRestoreFromJSON(jc2, includePhysical, includeAppearance);
				storable4.isPresetRestore = false;
			}
		}
		foreach (Storable dynamicStorable2 in dynamicStorables)
		{
			JSONStorable storable5 = dynamicStorable2.storable;
			if (!storable5.exclude && (!storable5.onlyStoreIfActive || storable5.gameObject.activeInHierarchy) && !dictionary.ContainsKey(storable5.storeId))
			{
				storable5.isPresetRestore = true;
				storable5.LateRestoreFromJSON(jc2, includePhysical, includeAppearance);
				storable5.isPresetRestore = false;
			}
		}
	}

	protected void PostRestore()
	{
		if (includeOptional && optionalStorables != null)
		{
			foreach (Storable optionalStorable in optionalStorables)
			{
				JSONStorable storable = optionalStorable.storable;
				storable.PostRestore();
			}
		}
		if (storables != null)
		{
			foreach (Storable storable4 in storables)
			{
				JSONStorable storable2 = storable4.storable;
				storable2.PostRestore();
			}
		}
		if (dynamicStorables == null)
		{
			return;
		}
		foreach (Storable dynamicStorable in dynamicStorables)
		{
			JSONStorable storable3 = dynamicStorable.storable;
			storable3.PostRestore();
		}
	}

	protected void FilterStorables(JSONClass inputjc, JSONClass outputjc, List<Storable> sts)
	{
		Dictionary<string, Storable> dictionary = new Dictionary<string, Storable>();
		JSONArray asArray = inputjc["storables"].AsArray;
		JSONArray asArray2 = outputjc["storables"].AsArray;
		foreach (Storable st in sts)
		{
			JSONStorable storable = st.storable;
			if (!ignoreExclude && storable.exclude)
			{
				continue;
			}
			if (st.specificKey != null && st.specificKey != string.Empty)
			{
				string key = st.storable.storeId + ":" + st.specificKey;
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, st);
				}
			}
			else if (!dictionary.ContainsKey(st.storable.storeId))
			{
				dictionary.Add(st.storable.storeId, st);
			}
		}
		Dictionary<string, JSONClass> dictionary2 = new Dictionary<string, JSONClass>();
		foreach (JSONClass item in asArray2)
		{
			if (item["id"] != null)
			{
				dictionary2.Add(item["id"], item);
			}
		}
		foreach (JSONClass item2 in asArray)
		{
			if (!(item2["id"] != null))
			{
				continue;
			}
			if (dictionary.TryGetValue(item2["id"], out var value))
			{
				if (!dictionary2.TryGetValue(value.storable.storeId, out var _))
				{
					asArray2.Add(item2);
					dictionary2.Add(value.storable.storeId, item2);
				}
				continue;
			}
			foreach (string key2 in item2.Keys)
			{
				if (dictionary.TryGetValue(string.Concat(item2["id"], ":", key2), out value))
				{
					if (!dictionary2.TryGetValue(value.storable.storeId, out var value3))
					{
						value3 = new JSONClass();
						value3["id"] = value.storable.storeId;
						asArray2.Add(value3);
						dictionary2.Add(value.storable.storeId, value3);
					}
					value3[value.specificKey] = item2[value.specificKey];
				}
			}
		}
	}

	protected void RestoreStorables(JSONClass jc)
	{
		if (jc != null)
		{
			RefreshDynamicStorables();
			PreRestore();
			Restore(jc);
			LateRestore(jc);
			PostRestore();
			if (postLoadEvent != null)
			{
				postLoadEvent.Invoke();
			}
		}
	}

	public virtual void RestorePresetBinary()
	{
	}

	public void CreateStoreFolderPath()
	{
		string storeFolderPath = GetStoreFolderPath(includePackage: false);
		if (storeFolderPath != null && Application.isPlaying)
		{
			FileManager.CreateDirectory(storeFolderPath);
		}
	}

	public bool CheckPresetReadyForStore()
	{
		bool result = false;
		if (itemType != 0)
		{
			string storeFolderPath = GetStoreFolderPath(includePackage: false);
			if (IsPresetInPackage())
			{
				VarPackage varPackage = FileManager.GetPackage(presetPackageName);
				if (varPackage == null || !varPackage.IsSimulated)
				{
					return false;
				}
			}
			if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
			{
				result = true;
			}
		}
		return result;
	}

	public bool CheckPresetExistance()
	{
		bool result = false;
		if (itemType != 0)
		{
			string storeFolderPath = GetStoreFolderPath(includePackage: false);
			if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
			{
				string path = presetPackagePath + storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vap";
				result = FileManager.FileExists(path);
			}
		}
		return result;
	}

	public bool IsPresetInPackage()
	{
		bool result = false;
		if (presetPackageName != null && presetPackageName != string.Empty)
		{
			result = true;
		}
		return result;
	}

	public string GetFavoriteStorePath()
	{
		string result = null;
		string storeFolderPath = GetStoreFolderPath(includePackage: false);
		if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
		{
			result = ((!IsPresetInPackage()) ? (storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vap.fav") : (storeFolderPath + presetSubPath + storeName + "_" + presetPackageName + "__" + presetSubName + ".vap.fav"));
		}
		return result;
	}

	public bool IsFavorite()
	{
		string favoriteStorePath = GetFavoriteStorePath();
		if (favoriteStorePath != null && FileManager.FileExists(favoriteStorePath))
		{
			return true;
		}
		return false;
	}

	public void SetFavorite(bool b)
	{
		string favoriteStorePath = GetFavoriteStorePath();
		if (favoriteStorePath == null)
		{
			return;
		}
		if (FileManager.FileExists(favoriteStorePath))
		{
			if (!b)
			{
				FileManager.DeleteFile(favoriteStorePath);
			}
		}
		else if (b)
		{
			string directoryName = FileManager.GetDirectoryName(favoriteStorePath);
			if (!FileManager.DirectoryExists(directoryName))
			{
				FileManager.CreateDirectory(directoryName);
			}
			FileManager.WriteAllText(favoriteStorePath, string.Empty);
		}
	}

	public bool StorePreset(bool doScreenshot = false)
	{
		bool result = false;
		if (itemType != 0)
		{
			string text = GetStoreFolderPath(includePackage: false);
			if (text != null && text != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
			{
				CreateStoreFolderPath();
				if (presetPackageName != null)
				{
					VarPackage varPackage = FileManager.GetPackage(presetPackageName);
					if (varPackage != null && varPackage.IsSimulated)
					{
						text = varPackage.SlashPath + ":/" + text;
						FileManager.CreateDirectory(text);
					}
				}
				if (presetSubPath != null && presetSubPath != string.Empty)
				{
					FileManager.CreateDirectory(text + presetSubPath);
				}
				string text2 = text + presetSubPath + storeName + "_" + presetSubName + ".vap";
				JSONClass jSONClass = new JSONClass();
				jSONClass["setUnlistedParamsToDefault"].AsBool = true;
				FileManager.SetSaveDirFromFilePath(text2);
				StoreStorables(jSONClass);
				StringBuilder stringBuilder = new StringBuilder(100000);
				jSONClass.ToString(string.Empty, stringBuilder);
				string value = stringBuilder.ToString();
				try
				{
					StreamWriter streamWriter = FileManager.OpenStreamWriter(text2);
					streamWriter.Write(value);
					streamWriter.Close();
					if (doScreenshot)
					{
						string text3 = text + presetSubPath + storeName + "_" + presetSubName + ".jpg";
						text3 = text3.Replace('/', '\\');
						SuperController.singleton.DoSaveScreenshot(text3);
					}
					result = true;
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception while storing to " + text2 + " " + ex);
				}
				if (storePresetBinary)
				{
					StorePresetBinary();
				}
			}
			else
			{
				SuperController.LogError("Not all preset parameters set. Cannot store");
			}
		}
		else
		{
			SuperController.LogError("Item type set to None. Cannot store");
		}
		return result;
	}

	public bool LoadPreset()
	{
		if (LoadPresetPre())
		{
			bool result = LoadPresetPost();
			FileManager.PopLoadDir();
			return result;
		}
		return false;
	}

	public bool LoadPresetPre()
	{
		bool result = false;
		if (itemType != 0)
		{
			string storeFolderPath = GetStoreFolderPath(includePackage: false);
			if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty && _presetName != null && _presetName != string.Empty)
			{
				string text = presetPackagePath + storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vap";
				if (FileManager.FileExists(text))
				{
					string empty = string.Empty;
					try
					{
						FileManager.PushLoadDirFromFilePath(text);
						using FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(text);
						empty = fileEntryStreamReader.ReadToEnd();
						JSONNode jSONNode = JSON.Parse(empty);
						JSONClass jSONClass = (lastLoadedJSON = jSONNode.AsObject);
						setUnlistedParamsToDefault = true;
						if (jSONClass["setUnlistedParamsToDefault"] != null)
						{
							setUnlistedParamsToDefault = jSONClass["setUnlistedParamsToDefault"].AsBool;
						}
						filteredJSON = new JSONClass();
						if (setUnlistedParamsToDefault)
						{
							LoadDefaultsPreInternal();
						}
						else
						{
							filteredJSON["storables"] = new JSONArray();
						}
						if (includeOptional)
						{
							FilterStorables(jSONClass, filteredJSON, optionalStorables);
						}
						FilterStorables(jSONClass, filteredJSON, storables);
						RefreshDynamicStorables();
						FilterStorables(jSONClass, filteredJSON, dynamicStorables);
						result = true;
					}
					catch (Exception ex)
					{
						SuperController.LogError("Exception while loading " + text + " " + ex);
					}
				}
				else
				{
					SuperController.LogError("Could not load json " + text);
				}
				RestorePresetBinary();
			}
			else
			{
				SuperController.LogError("Not all preset parameters set. Cannot load");
			}
		}
		else
		{
			SuperController.LogError("Item type set to None. Cannot load");
		}
		return result;
	}

	protected void LoadDefaultsPreProcessStorables(List<Storable> sts, JSONArray outputStorables)
	{
		if (sts == null)
		{
			return;
		}
		Dictionary<string, JSONClass> dictionary = new Dictionary<string, JSONClass>();
		foreach (JSONClass outputStorable in outputStorables)
		{
			if (outputStorable["id"] != null)
			{
				dictionary.Add(outputStorable["id"], outputStorable);
			}
		}
		foreach (Storable st in sts)
		{
			JSONStorable storable = st.storable;
			if ((ignoreExclude || !storable.exclude) && st.specificKey != null && st.specificKey != string.Empty)
			{
				if (!dictionary.TryGetValue(storable.storeId, out var value))
				{
					value = new JSONClass();
					value["id"] = storable.storeId;
					outputStorables.Add(value);
					dictionary.Add(storable.storeId, value);
				}
				if (st.isSpecificKeyAnObject)
				{
					JSONClass value2 = new JSONClass();
					value[st.specificKey] = value2;
				}
				else
				{
					value[st.specificKey] = string.Empty;
				}
			}
		}
	}

	protected void LoadDefaultsPreInternal()
	{
		JSONArray jSONArray = new JSONArray();
		filteredJSON["storables"] = jSONArray;
		if (includeOptional && storeOptionalStorables)
		{
			LoadDefaultsPreProcessStorables(optionalStorables, jSONArray);
		}
		LoadDefaultsPreProcessStorables(storables, jSONArray);
	}

	public bool LoadDefaultsPre()
	{
		bool result = false;
		if (itemType != 0)
		{
			string storeFolderPath = GetStoreFolderPath(includePackage: false);
			if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty)
			{
				lastLoadedJSON = new JSONClass();
				filteredJSON = lastLoadedJSON;
				setUnlistedParamsToDefault = true;
				LoadDefaultsPreInternal();
				result = true;
			}
		}
		return result;
	}

	public bool LoadPresetPost()
	{
		if (filteredJSON != null)
		{
			RestoreStorables(filteredJSON);
			return true;
		}
		return false;
	}

	public void RefreshStorables()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		storables = new List<Storable>();
		optionalStorables = new List<Storable>();
		regularStorables = new Dictionary<JSONStorable, bool>();
		specificKeyStorables = new Dictionary<string, bool>();
		if (specificStorables != null && specificStorables.Length > 0)
		{
			SpecificStorable[] array = specificStorables;
			foreach (SpecificStorable specificStorable in array)
			{
				if (!(specificStorable.specificStorableBucket != null))
				{
					continue;
				}
				JSONStorable[] array2 = ((!specificStorable.includeChildren) ? specificStorable.specificStorableBucket.GetComponents<JSONStorable>() : specificStorable.specificStorableBucket.GetComponentsInChildren<JSONStorable>(includeInactive: true));
				JSONStorable[] array3 = array2;
				foreach (JSONStorable jSONStorable in array3)
				{
					if (specificStorable.storeId == string.Empty || specificStorable.storeId == jSONStorable.storeId)
					{
						Storable storable = new Storable();
						storable.storable = jSONStorable;
						if (specificStorable.specificKey != null && specificStorable.specificKey != string.Empty && !specificKeyStorables.ContainsKey(specificStorable.storeId))
						{
							specificKeyStorables.Add(specificStorable.storeId, value: true);
						}
						storable.specificKey = specificStorable.specificKey;
						storable.isSpecificKeyAnObject = specificStorable.isSpecificKeyAnObject;
						storables.Add(storable);
						if (!regularStorables.ContainsKey(jSONStorable))
						{
							regularStorables.Add(jSONStorable, value: true);
						}
					}
				}
			}
		}
		if (optionalSpecificStorables != null && optionalSpecificStorables.Length > 0)
		{
			SpecificStorable[] array4 = optionalSpecificStorables;
			foreach (SpecificStorable specificStorable2 in array4)
			{
				if (!(specificStorable2.specificStorableBucket != null))
				{
					continue;
				}
				JSONStorable[] components = specificStorable2.specificStorableBucket.GetComponents<JSONStorable>();
				JSONStorable[] array5 = components;
				foreach (JSONStorable jSONStorable2 in array5)
				{
					if (specificStorable2.storeId == string.Empty || specificStorable2.storeId == jSONStorable2.storeId)
					{
						Storable storable2 = new Storable();
						storable2.storable = jSONStorable2;
						storable2.specificKey = specificStorable2.specificKey;
						storable2.isSpecificKeyAnObject = specificStorable2.isSpecificKeyAnObject;
						optionalStorables.Add(storable2);
						if (!regularStorables.ContainsKey(jSONStorable2))
						{
							regularStorables.Add(jSONStorable2, value: true);
						}
					}
				}
			}
		}
		if (!useTransformAndChildren)
		{
			return;
		}
		JSONStorable[] componentsInChildren = GetComponentsInChildren<JSONStorable>(includeInactive: true);
		JSONStorable[] array6 = componentsInChildren;
		foreach (JSONStorable jSONStorable3 in array6)
		{
			if (!regularStorables.ContainsKey(jSONStorable3))
			{
				Storable storable3 = new Storable();
				storable3.storable = jSONStorable3;
				storables.Add(storable3);
				regularStorables.Add(jSONStorable3, value: true);
			}
		}
	}

	protected void RefreshDynamicStorables()
	{
		dynamicStorables = new List<Storable>();
		Transform[] array = dynamicStorablesBuckets;
		foreach (Transform transform in array)
		{
			JSONStorable[] componentsInChildren = transform.GetComponentsInChildren<JSONStorable>(includeInactive: true);
			JSONStorable[] array2 = componentsInChildren;
			foreach (JSONStorable jSONStorable in array2)
			{
				if (!regularStorables.ContainsKey(jSONStorable))
				{
					Storable storable = new Storable();
					storable.storable = jSONStorable;
					dynamicStorables.Add(storable);
				}
			}
		}
	}

	public void SyncMaterialOptions()
	{
		MaterialOptions[] components = GetComponents<MaterialOptions>();
		string storeFolderPath = GetStoreFolderPath();
		MaterialOptions[] array = components;
		foreach (MaterialOptions materialOptions in array)
		{
			materialOptions.SetCustomTextureFolder(storeFolderPath);
		}
	}

	protected virtual void Awake()
	{
		RefreshStorables();
		RefreshDynamicStorables();
		SyncMaterialOptions();
	}
}
