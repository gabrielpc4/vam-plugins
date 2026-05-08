using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using UnityEngine;
using UnityEngine.UI;

namespace uFileBrowser;

public class FileBrowser : MonoBehaviour
{
	public class FileAndDirInfo
	{
		protected bool _isWriteable;

		protected bool _isDirectory;

		public bool isWriteable => _isWriteable;

		public bool isDirectory => _isDirectory;

		public string Name { get; protected set; }

		public string FullName { get; protected set; }

		public DateTime LastWriteTime { get; protected set; }

		public FileAndDirInfo(DirectoryInfo directoryInfo, string overrideFullName)
		{
			_isDirectory = true;
			_isWriteable = FileManager.IsSecureWritePath(directoryInfo.FullName);
			Name = directoryInfo.Name;
			FullName = overrideFullName;
			LastWriteTime = directoryInfo.LastWriteTime;
		}

		public FileAndDirInfo(FileSystemInfo fileInfo)
		{
			_isDirectory = false;
			_isWriteable = FileManager.IsSecureWritePath(fileInfo.FullName);
			Name = fileInfo.Name;
			FullName = fileInfo.FullName;
			LastWriteTime = fileInfo.LastWriteTime;
		}

		public FileAndDirInfo(DirectoryEntry dirEntry, string currentPath)
		{
			_isDirectory = true;
			_isWriteable = !(dirEntry is VarDirectoryEntry) && FileManager.IsSecureWritePath(dirEntry.FullPath);
			Name = dirEntry.Name;
			if (currentPath != string.Empty)
			{
				FullName = currentPath + "\\" + Name;
			}
			else
			{
				FullName = Name;
			}
			LastWriteTime = dirEntry.LastWriteTime;
		}

		public FileAndDirInfo(FileEntry fileEntry)
		{
			_isDirectory = false;
			_isWriteable = !(fileEntry is VarFileEntry) && FileManager.IsSecureWritePath(fileEntry.FullPath);
			Name = fileEntry.Name;
			FullName = fileEntry.Uid;
			LastWriteTime = fileEntry.LastWriteTime;
		}
	}

	public string defaultPath = string.Empty;

	public bool selectDirectory;

	public bool showFiles;

	public bool showDirs = true;

	public bool canCancel = true;

	public bool selectOnClick = true;

	public bool browseVarFilesAsDirectories = true;

	public bool showInstallFolderInDirectoryList;

	public string fileFormat = string.Empty;

	public bool hideExtension;

	public string fileRemovePrefix;

	[SerializeField]
	[HideInInspector]
	private string currentPath;

	[SerializeField]
	[HideInInspector]
	private string search;

	[SerializeField]
	[HideInInspector]
	private string slash;

	[SerializeField]
	[HideInInspector]
	private List<string> drives;

	private List<FileButton> fileButtons;

	private List<DirectoryButton> dirButtons;

	private List<ShortCutButton> shortCutButtons;

	private List<GameObject> dirSpacers;

	private int selected = -1;

	private FileBrowserCallback callback;

	public List<ShortCut> shortCuts;

	public UIPopup directoryOptionPopup;

	protected UserPreferences.DirectoryOption _directoryOption;

	public UIPopup sortByPopup;

	protected UserPreferences.SortBy _sortBy = UserPreferences.SortBy.NewToOld;

	public GameObject overlay;

	public GameObject window;

	public GameObject fileButtonPrefab;

	public GameObject directoryButtonPrefab;

	public GameObject directorySpacerPrefab;

	public Text titleText;

	public RectTransform fileContent;

	public ScrollRect filesScrollRect;

	public RectTransform dirContent;

	public RectTransform dirOption;

	public GameObject shortCutButtonPrefab;

	public RectTransform shortCutContent;

	public Button openPackageButton;

	public Button promotionalButton;

	public Text promotionalButtonText;

	public Toggle keepOpenToggle;

	[SerializeField]
	protected bool _keepOpen;

	public Toggle onlyShowLatestToggle;

	protected bool _onlyShowLatest = true;

	public InputField currentPathField;

	public InputField searchField;

	public Button searchCancelButton;

	public Button cancelButton;

	public Button selectButton;

	public Text selectButtonText;

	public Transform renameContainer;

	public InputField renameField;

	public InputFieldAction renameFieldAction;

	protected int renameIndex;

	public Transform deleteContainer;

	public InputField deleteField;

	protected int deleteIndex;

	public InputField statusField;

	public InputField fileEntryField;

	public Sprite folderIcon;

	public Sprite defaultIcon;

	public List<FileIcon> fileIcons = new List<FileIcon>();

	protected Dictionary<string, float> directoryScrollPositions;

	protected string currentPackageUid;

	protected string currentPackageFilter;

	protected bool useFlatten;

	protected bool includeRegularDirsInFlatten;

	public string SelectedPath
	{
		get
		{
			if (selected > -1)
			{
				return fileButtons[selected].fullPath;
			}
			return null;
		}
	}

	public UserPreferences.DirectoryOption directoryOption
	{
		get
		{
			return _directoryOption;
		}
		set
		{
			if (_directoryOption != value)
			{
				_directoryOption = value;
				if (directoryOptionPopup != null)
				{
					directoryOptionPopup.currentValueNoCallback = _directoryOption.ToString();
				}
				UpdateFileList();
			}
		}
	}

	public UserPreferences.SortBy sortBy
	{
		get
		{
			return _sortBy;
		}
		set
		{
			if (_sortBy != value)
			{
				_sortBy = value;
				if (sortByPopup != null)
				{
					sortByPopup.currentValueNoCallback = _sortBy.ToString();
				}
				UpdateFileList();
			}
		}
	}

	public bool keepOpen
	{
		get
		{
			return _keepOpen;
		}
		set
		{
			if (_keepOpen != value)
			{
				_keepOpen = value;
				if (keepOpenToggle != null)
				{
					keepOpenToggle.isOn = _keepOpen;
				}
			}
		}
	}

	public bool onlyShowLatest
	{
		get
		{
			return _onlyShowLatest;
		}
		set
		{
			if (_onlyShowLatest != value)
			{
				_onlyShowLatest = value;
				if (onlyShowLatestToggle != null)
				{
					onlyShowLatestToggle.isOn = _onlyShowLatest;
				}
				UpdateDirectoryList();
			}
		}
	}

	public void ClearCacheImage(string imgPath)
	{
		if (ImageLoaderThreaded.singleton != null)
		{
			ImageLoaderThreaded.singleton.ClearCacheThumbnail(imgPath);
		}
	}

	public void ClearImageQueue()
	{
		if (ImageLoaderThreaded.singleton != null)
		{
			ImageLoaderThreaded.singleton.ClearQueuedThumbnails();
		}
	}

	public void MakeNewUniqueFolder()
	{
		string text = currentPath + slash + "NewFolder";
		int num = 0;
		while (num < 20)
		{
			if (FileManager.DirectoryExists(text))
			{
				num++;
				text = currentPath + slash + "NewFolder" + num;
				continue;
			}
			try
			{
				FileManager.CreateDirectory(text);
			}
			catch (Exception ex)
			{
				Debug.LogError("Could not make directory " + text + " Exception: " + ex.Message);
				if (statusField != null)
				{
					statusField.text = ex.Message;
				}
			}
			break;
		}
		UpdateDirectoryList();
		UpdateFileList();
	}

	public void SetDirectoryOption(string dirOptionString)
	{
		try
		{
			UserPreferences.DirectoryOption fileBrowserDirectoryOption = (this.directoryOption = (UserPreferences.DirectoryOption)Enum.Parse(typeof(UserPreferences.DirectoryOption), dirOptionString));
			if (UserPreferences.singleton != null)
			{
				UserPreferences.singleton.fileBrowserDirectoryOption = fileBrowserDirectoryOption;
			}
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set directory option to " + dirOptionString + " which is not a valid type");
		}
	}

	public void SetSortBy(string sortByString)
	{
		try
		{
			UserPreferences.SortBy fileBrowserSortBy = (this.sortBy = (UserPreferences.SortBy)Enum.Parse(typeof(UserPreferences.SortBy), sortByString));
			if (UserPreferences.singleton != null)
			{
				UserPreferences.singleton.fileBrowserSortBy = fileBrowserSortBy;
			}
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set sort by to " + sortByString + " which is not a valid type");
		}
	}

	private void SortFilesAndDirs(List<FileAndDirInfo> fdlist)
	{
		switch (sortBy)
		{
			case UserPreferences.SortBy.AtoZ:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => a.Name.CompareTo(b.Name));
				break;
			case UserPreferences.SortBy.ZtoA:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => b.Name.CompareTo(a.Name));
				break;
			case UserPreferences.SortBy.NewToOld:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
				break;
			case UserPreferences.SortBy.OldToNew:
				fdlist.Sort((FileAndDirInfo a, FileAndDirInfo b) => a.LastWriteTime.CompareTo(b.LastWriteTime));
				break;
		}
	}

	private void CreateFileButton(string text, string path, bool dir, int i, bool writeable)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(fileButtonPrefab, Vector3.zero, Quaternion.identity);
		gameObject.GetComponent<RectTransform>().SetParent(fileContent, worldPositionStays: false);
		FileButton component = gameObject.GetComponent<FileButton>();
		string text2 = text;
		if (hideExtension)
		{
			text2 = Regex.Replace(text2, "\\.[^\\.]*$", string.Empty);
		}
		if (fileRemovePrefix != null)
		{
			string text3 = Regex.Replace(text2, "^" + fileRemovePrefix, string.Empty);
			if (text2 != text3)
			{
				component.removedPrefix = fileRemovePrefix;
				text2 = text3;
			}
		}
		component.Set(this, text2, path, dir, i, writeable);
		if (ImageLoaderThreaded.singleton != null)
		{
			Transform transform = null;
			if (component.fileIcon != null)
			{
				transform = component.fileIcon.transform;
			}
			Transform transform2 = null;
			if (component.altIcon != null)
			{
				transform2 = component.altIcon.transform;
			}
			if (transform != null)
			{
				transform.gameObject.SetActive(value: true);
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(value: false);
					RawImage altIcon = component.altIcon;
					if (altIcon != null)
					{
						FileEntry fileEntry = FileManager.GetFileEntry(path);
						if (fileEntry != null)
						{
							string text4 = fileEntry.Path;
							if (Regex.IsMatch(text4, "\\.duf$"))
							{
								text4 += ".png";
							}
							else if (Regex.IsMatch(text4, "\\.(json|vac|vap|vam|scene|assetbundle)$"))
							{
								text4 = Regex.Replace(text4, "\\.(json|vac|vap|vam|scene|assetbundle)$", ".jpg");
							}
							if (FileManager.FileExists(text4) && (text4.EndsWith(".jpg") || text4.EndsWith(".jpeg") || text4.EndsWith(".png") || text4.EndsWith(".tif")))
							{
								transform.gameObject.SetActive(value: false);
								transform2.gameObject.SetActive(value: true);
								Texture2D cachedThumbnail = ImageLoaderThreaded.singleton.GetCachedThumbnail(text4);
								if (cachedThumbnail != null)
								{
									altIcon.texture = cachedThumbnail;
								}
								else
								{
									ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
									queuedImage.imgPath = text4;
									queuedImage.width = 512;
									queuedImage.height = 512;
									queuedImage.setSize = true;
									queuedImage.fillBackground = true;
									queuedImage.rawImageToLoad = altIcon;
									ImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
								}
							}
						}
					}
				}
			}
		}
		fileButtons.Add(component);
	}

	private void CreateDirectoryButton(string package, string text, string path, int i)
	{
		if (dirContent != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(directoryButtonPrefab, Vector3.zero, Quaternion.identity);
			gameObject.GetComponent<RectTransform>().SetParent(dirContent, worldPositionStays: false);
			DirectoryButton component = gameObject.GetComponent<DirectoryButton>();
			component.Set(this, package, currentPackageFilter, text, path, i);
			dirButtons.Add(component);
		}
	}

	private void CreateShortCutButton(ShortCut shortCut, int i)
	{
		if (shortCutContent != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(shortCutButtonPrefab, Vector3.zero, Quaternion.identity);
			gameObject.GetComponent<RectTransform>().SetParent(shortCutContent, worldPositionStays: false);
			ShortCutButton component = gameObject.GetComponent<ShortCutButton>();
			component.Set(this, shortCut.package, shortCut.packageFilter, shortCut.flatten, shortCut.includeRegularDirsInFlatten, shortCut.displayName, shortCut.path, i);
			shortCutButtons.Add(component);
		}
	}

	private void CreateDirectorySpacer()
	{
		if (dirContent != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(directorySpacerPrefab, Vector3.zero, Quaternion.identity);
			gameObject.GetComponent<RectTransform>().SetParent(dirContent, worldPositionStays: false);
			dirSpacers.Add(gameObject);
		}
	}

	public void SetTitle(string title)
	{
		if (titleText != null)
		{
			titleText.text = title;
		}
	}

	public void Show(FileBrowserCallback callback)
	{
		if (statusField != null)
		{
			statusField.text = string.Empty;
		}
		if (fileEntryField != null)
		{
			fileEntryField.text = string.Empty;
		}
		if (UserPreferences.singleton != null)
		{
			sortBy = UserPreferences.singleton.fileBrowserSortBy;
			directoryOption = UserPreferences.singleton.fileBrowserDirectoryOption;
		}
		GotoDirectory(defaultPath);
		UpdateUI();
		this.callback = callback;
		if ((bool)overlay)
		{
			overlay.SetActive(value: true);
		}
		window.SetActive(value: true);
	}

	public void Hide()
	{
		if (window.activeSelf)
		{
			if (selected > -1)
			{
				fileButtons[selected].Unselect();
			}
			ClearImageQueue();
			SaveDirectoryScrollPos(currentPath);
			currentPath = string.Empty;
			selected = -1;
			search = string.Empty;
			if ((bool)overlay)
			{
				overlay.SetActive(value: false);
			}
			window.SetActive(value: false);
		}
	}

	public bool IsHidden()
	{
		return !window.activeSelf;
	}

	public void UpdateUI()
	{
		if ((bool)cancelButton)
		{
			cancelButton.gameObject.SetActive(canCancel);
		}
		if (currentPathField != null)
		{
			currentPathField.text = currentPath;
		}
		if (searchField != null)
		{
			searchField.text = search;
		}
	}

	public Sprite GetFileIcon(string path)
	{
		string empty = string.Empty;
		if (path.Contains("."))
		{
			empty = path.Substring(path.LastIndexOf('.') + 1);
			for (int i = 0; i < fileIcons.Count; i++)
			{
				if (fileIcons[i].extension == empty)
				{
					return fileIcons[i].icon;
				}
			}
			return defaultIcon;
		}
		return defaultIcon;
	}

	private IEnumerator RenameProcess()
	{
		yield return null;
		LookInputModule.SelectGameObject(renameField.gameObject);
		renameField.ActivateInputField();
	}

	public void OnRenameClick(int i)
	{
		if (statusField != null)
		{
			statusField.text = string.Empty;
		}
		renameIndex = -1;
		if (i >= fileButtons.Count)
		{
			Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
			return;
		}
		renameIndex = i;
		OpenRenameDialog();
		if (renameField != null)
		{
			string text = fileButtons[renameIndex].text;
			if (text.EndsWith(".json"))
			{
				text = text.Replace(".json", string.Empty);
			}
			else if (text.EndsWith(".vac"))
			{
				text = text.Replace(".vac", string.Empty);
			}
			else if (text.EndsWith(".vap"))
			{
				text = text.Replace(".vap", string.Empty);
			}
			renameField.text = text;
			StartCoroutine(RenameProcess());
		}
	}

	protected void OpenRenameDialog()
	{
		if (renameContainer != null)
		{
			renameContainer.gameObject.SetActive(value: true);
		}
	}

	public void OnRenameConfirm()
	{
		if (renameField != null && renameField.text != string.Empty && renameIndex != -1)
		{
			string text = ((fileButtons[renameIndex].removedPrefix == null) ? (currentPath + slash + renameField.text) : (currentPath + slash + fileButtons[renameIndex].removedPrefix + renameField.text));
			if (fileButtons[renameIndex].isDir)
			{
				string fullPath = fileButtons[renameIndex].fullPath;
				try
				{
					FileManager.MoveDirectory(fullPath, text);
				}
				catch (Exception ex)
				{
					Debug.LogError("Could not move directory " + fullPath + " to " + text + " Exception: " + ex.Message);
					if (statusField != null)
					{
						statusField.text = ex.Message;
					}
				}
			}
			else
			{
				string fullPath2 = fileButtons[renameIndex].fullPath;
				bool flag = false;
				string oldValue = string.Empty;
				if (fullPath2.EndsWith(".json"))
				{
					flag = true;
					oldValue = ".json";
					if (!text.EndsWith(".json"))
					{
						text += ".json";
					}
				}
				else if (fullPath2.EndsWith(".vac"))
				{
					flag = true;
					oldValue = ".vac";
					if (!text.EndsWith(".vac"))
					{
						text += ".vac";
					}
				}
				else if (fullPath2.EndsWith(".vap"))
				{
					flag = true;
					oldValue = ".vap";
					if (!text.EndsWith(".vap"))
					{
						text += ".vap";
					}
				}
				Debug.Log("Rename file " + fullPath2 + " to " + text);
				try
				{
					FileManager.MoveFile(fullPath2, text);
				}
				catch (Exception ex2)
				{
					Debug.LogError("Could not move file " + fullPath2 + " to " + text + " Exception: " + ex2);
					if (statusField != null)
					{
						statusField.text = ex2.Message;
					}
				}
				if (flag)
				{
					string text2 = fullPath2.Replace(oldValue, ".jpg");
					string text3 = text.Replace(oldValue, ".jpg");
					if (FileManager.FileExists(text2))
					{
						try
						{
							FileManager.MoveFile(text2, text3);
						}
						catch (Exception ex3)
						{
							Debug.LogError("Could not move file " + text2 + " to " + text3 + " Exception: " + ex3.Message);
							if (statusField != null)
							{
								statusField.text = ex3.Message;
							}
						}
					}
					string text4 = fullPath2 + ".fav";
					string text5 = text + ".fav";
					if (FileManager.FileExists(text4))
					{
						try
						{
							FileManager.MoveFile(text4, text5);
						}
						catch (Exception ex4)
						{
							Debug.LogError("Could not move file " + text4 + " to " + text5 + " Exception: " + ex4.Message);
							if (statusField != null)
							{
								statusField.text = ex4.Message;
							}
						}
					}
				}
			}
			UpdateFileList();
			UpdateDirectoryList();
		}
		OnRenameCancel();
	}

	public void OnRenameCancel()
	{
		if (renameContainer != null)
		{
			renameContainer.gameObject.SetActive(value: false);
		}
		renameIndex = -1;
	}

	public void OnDeleteClick(int i)
	{
		deleteIndex = -1;
		if (statusField != null)
		{
			statusField.text = string.Empty;
		}
		if (i >= fileButtons.Count)
		{
			Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
			return;
		}
		deleteIndex = i;
		OpenDeleteDialog();
		if (deleteField != null)
		{
			string text = fileButtons[deleteIndex].text;
			if (text.EndsWith(".json"))
			{
				text = text.Replace(".json", string.Empty);
			}
			else if (text.EndsWith(".vac"))
			{
				text = text.Replace(".vac", string.Empty);
			}
			else if (text.EndsWith(".vap"))
			{
				text = text.Replace(".vap", string.Empty);
			}
			deleteField.text = text;
		}
	}

	protected void OpenDeleteDialog()
	{
		if (deleteContainer != null)
		{
			deleteContainer.gameObject.SetActive(value: true);
		}
	}

	public void OnDeleteConfirm()
	{
		if (deleteIndex != -1)
		{
			string fullPath = fileButtons[deleteIndex].fullPath;
			if (fileButtons[deleteIndex].isDir)
			{
				if (FileManager.DirectoryExists(fullPath))
				{
					try
					{
						FileManager.DeleteDirectory(fullPath, recursive: true);
					}
					catch (Exception ex)
					{
						Debug.LogError("Could not delete directory " + fullPath + " Exception: " + ex.Message);
						if (statusField != null)
						{
							statusField.text = ex.Message;
						}
					}
				}
			}
			else
			{
				if (FileManager.FileExists(fullPath))
				{
					try
					{
						FileManager.DeleteFile(fullPath);
					}
					catch (Exception ex2)
					{
						Debug.LogError("Could not delete file " + fullPath + " Exception: " + ex2.Message);
						if (statusField != null)
						{
							statusField.text = ex2.Message;
						}
					}
				}
				string text = string.Empty;
				if (fullPath.EndsWith(".json"))
				{
					text = ".json";
				}
				else if (fullPath.EndsWith(".vac"))
				{
					text = ".vac";
				}
				else if (fullPath.EndsWith(".vap"))
				{
					text = ".vap";
				}
				if (text != string.Empty)
				{
					string text2 = fullPath.Replace(text, ".jpg");
					if (FileManager.FileExists(text2))
					{
						try
						{
							FileManager.DeleteFile(text2);
						}
						catch (Exception ex3)
						{
							Debug.LogError("Could not delete file " + text2 + " Exception: " + ex3.Message);
							if (statusField != null)
							{
								statusField.text = ex3.Message;
							}
						}
					}
				}
				string text3 = fullPath + ".fav";
				if (FileManager.FileExists(text3))
				{
					try
					{
						FileManager.DeleteFile(text3);
					}
					catch (Exception ex4)
					{
						Debug.LogError("Could not delete file " + text3 + " Exception: " + ex4.Message);
						if (statusField != null)
						{
							statusField.text = ex4.Message;
						}
					}
				}
			}
			UpdateFileList();
			UpdateDirectoryList();
		}
		OnDeleteCancel();
	}

	public void OnDeleteCancel()
	{
		if (deleteContainer != null)
		{
			deleteContainer.gameObject.SetActive(value: false);
		}
		deleteIndex = -1;
	}

	protected string DeterminePathToGoTo(string pathToGoTo)
	{
		DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(pathToGoTo);
		if (!selectDirectory && directoryEntry != null && directoryEntry is VarDirectoryEntry)
		{
			DirectoryEntry directoryEntry2 = directoryEntry.FindFirstDirectoryWithFiles();
			string uid = directoryEntry.Uid;
			string uid2 = directoryEntry2.Uid;
			if (uid2 != uid)
			{
				string text = uid2.Replace(uid, string.Empty);
				text = text.Replace('/', '\\');
				pathToGoTo += text;
			}
		}
		return pathToGoTo;
	}

	public void OnFileClick(int i)
	{
		if (i >= fileButtons.Count)
		{
			Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
		}
		else if (fileButtons[i].isDir)
		{
			if (!selectDirectory)
			{
				string text = fileButtons[i].fullPath;
				VarDirectoryEntry varDirectoryEntry = FileManager.GetVarDirectoryEntry(text);
				if (currentPackageFilter != null && varDirectoryEntry != null && varDirectoryEntry.Package.RootDirectory == varDirectoryEntry)
				{
					text = text + "\\" + currentPackageFilter;
				}
				GotoDirectory(DeterminePathToGoTo(text), currentPackageFilter);
			}
			else
			{
				SelectFile(i);
			}
		}
		else
		{
			SelectFile(i);
		}
	}

	public void OnDirectoryClick(int i)
	{
		if (i >= dirButtons.Count)
		{
			Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
		}
		else
		{
			GotoDirectory(dirButtons[i].fullPath, dirButtons[i].packageFilter);
		}
	}

	public void OnShortCutClick(int i)
	{
		if (i >= shortCutButtons.Count)
		{
			Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
		}
		else
		{
			GotoDirectory(DeterminePathToGoTo(shortCutButtons[i].fullPath), shortCutButtons[i].packageFilter, shortCutButtons[i].flatten, shortCutButtons[i].includeRegularDirsInFlatten);
		}
	}

	private IEnumerator DelaySetScroll(float scrollPos)
	{
		yield return null;
		filesScrollRect.verticalNormalizedPosition = scrollPos;
	}

	private void SaveDirectoryScrollPos(string path)
	{
		if (filesScrollRect != null)
		{
			if (directoryScrollPositions == null)
			{
				directoryScrollPositions = new Dictionary<string, float>();
			}
			string text = currentPath;
			if (!text.EndsWith("\\"))
			{
				text += "\\";
			}
			string key = fileFormat + ":" + text;
			if (directoryScrollPositions.TryGetValue(key, out var _))
			{
				directoryScrollPositions.Remove(key);
			}
			float verticalNormalizedPosition = filesScrollRect.verticalNormalizedPosition;
			directoryScrollPositions.Add(key, verticalNormalizedPosition);
		}
	}

	private void GoToPromotionalLink()
	{
		if (promotionalButtonText != null)
		{
			SuperController.singleton.OpenLinkInBrowser(promotionalButtonText.text);
		}
	}

	private void OpenPackageInManager()
	{
		if (promotionalButtonText != null && currentPackageUid != null && currentPackageUid != string.Empty)
		{
			SuperController.singleton.OpenPackageInManager(currentPackageUid);
		}
	}

	private void GotoDirectory(string path, string pkgFilter = null, bool flatten = false, bool includeRegularDirs = false)
	{
		if (path == currentPath && path != string.Empty && pkgFilter == currentPackageFilter && useFlatten == flatten && includeRegularDirsInFlatten == includeRegularDirs)
		{
			return;
		}
		currentPackageFilter = pkgFilter;
		useFlatten = flatten;
		includeRegularDirsInFlatten = includeRegularDirs;
		ClearSearch();
		SaveDirectoryScrollPos(currentPath);
		if (string.IsNullOrEmpty(path))
		{
			currentPath = string.Empty;
		}
		else if (!FileManager.DirectoryExists(path))
		{
			Debug.LogError("uFileBrowser: Directory doesn't exist:\n" + path);
			currentPath = string.Empty;
		}
		else
		{
			currentPath = path;
		}
		if ((bool)currentPathField)
		{
			currentPathField.text = currentPath;
		}
		if (selectDirectory && fileEntryField != null)
		{
			fileEntryField.text = string.Empty;
		}
		selected = -1;
		if (openPackageButton != null && promotionalButton != null && promotionalButtonText != null)
		{
			DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(path);
			if (directoryEntry is VarDirectoryEntry)
			{
				VarDirectoryEntry varDirectoryEntry = directoryEntry as VarDirectoryEntry;
				VarPackage package = varDirectoryEntry.Package;
				currentPackageUid = package.Uid;
				openPackageButton.gameObject.SetActive(value: true);
				if (package.PromotionalLink != null && package.PromotionalLink != string.Empty)
				{
					promotionalButton.gameObject.SetActive(value: true);
					promotionalButtonText.text = package.PromotionalLink;
				}
				else
				{
					promotionalButton.gameObject.SetActive(value: false);
				}
			}
			else
			{
				currentPackageUid = null;
				openPackageButton.gameObject.SetActive(value: false);
				promotionalButton.gameObject.SetActive(value: false);
			}
		}
		UpdateFileList();
		UpdateDirectoryList();
		if (!(filesScrollRect != null))
		{
			return;
		}
		float value = 1f;
		if (directoryScrollPositions != null)
		{
			string text = currentPath;
			if (!text.EndsWith("\\"))
			{
				text += "\\";
			}
			string key = fileFormat + ":" + text;
			if (!directoryScrollPositions.TryGetValue(key, out value))
			{
				value = 1f;
			}
		}
		StartCoroutine(DelaySetScroll(value));
	}

	private void SelectFile(int i)
	{
		if (i >= fileButtons.Count)
		{
			Debug.LogError("uFileBrowser: Selection index bigger than array.");
		}
		else if (i == selected && selectDirectory && fileButtons[i].isDir)
		{
			GotoDirectory(fileButtons[i].fullPath, currentPackageFilter);
		}
		else
		{
			if (!fileButtons[i].isDir && selectDirectory)
			{
				return;
			}
			if (selected != -1)
			{
				fileButtons[selected].Unselect();
			}
			selected = i;
			fileButtons[i].Select();
			if (fileEntryField != null)
			{
				fileEntryField.text = fileButtons[i].text;
				if (fileEntryField.text.EndsWith(".json"))
				{
					fileEntryField.text = fileEntryField.text.Replace(".json", string.Empty);
				}
				else if (fileEntryField.text.EndsWith(".vac"))
				{
					fileEntryField.text = fileEntryField.text.Replace(".vac", string.Empty);
				}
			}
			if (selectOnClick)
			{
				SelectButtonClicked();
			}
		}
	}

	public void PathFieldEndEdit()
	{
		if (currentPathField != null)
		{
			if (FileManager.DirectoryExists(currentPathField.text))
			{
				GotoDirectory(currentPathField.text);
			}
			else
			{
				currentPathField.text = currentPath;
			}
		}
	}

	private void FilterList()
	{
		if (string.IsNullOrEmpty(search))
		{
			for (int i = 0; i < fileButtons.Count; i++)
			{
				fileButtons[i].gameObject.SetActive(value: true);
			}
			return;
		}
		string value = search.ToLowerInvariant();
		for (int j = 0; j < fileButtons.Count; j++)
		{
			if (!fileButtons[j].text.ToLowerInvariant().Contains(value))
			{
				fileButtons[j].gameObject.SetActive(value: false);
			}
			else
			{
				fileButtons[j].gameObject.SetActive(value: true);
			}
		}
	}

	public void SearchChanged()
	{
		if ((bool)searchField)
		{
			search = searchField.text.Trim();
			FilterList();
		}
	}

	public void SearchCancelClick()
	{
		search = string.Empty;
		searchField.text = string.Empty;
		FilterList();
	}

	protected void ClearSearch()
	{
		search = string.Empty;
		searchField.text = string.Empty;
	}

	public void SetTextEntry(bool b)
	{
		if (b)
		{
			selectOnClick = false;
			if (selectButton != null)
			{
				selectButton.gameObject.SetActive(value: true);
			}
			if (fileEntryField != null)
			{
				fileEntryField.gameObject.SetActive(value: true);
			}
			if (keepOpenToggle != null)
			{
				keepOpenToggle.gameObject.SetActive(value: false);
			}
		}
		else
		{
			selectOnClick = true;
			if (selectButton != null)
			{
				selectButton.gameObject.SetActive(value: false);
			}
			if (fileEntryField != null)
			{
				fileEntryField.gameObject.SetActive(value: false);
			}
			if (keepOpenToggle != null)
			{
				keepOpenToggle.gameObject.SetActive(value: true);
			}
		}
	}

	private IEnumerator ActivateFileNameFieldProcess()
	{
		yield return null;
		LookInputModule.SelectGameObject(fileEntryField.gameObject);
		fileEntryField.ActivateInputField();
	}

	public void ActivateFileNameField()
	{
		if (fileEntryField != null)
		{
			StartCoroutine(ActivateFileNameFieldProcess());
		}
	}

	public void SelectButtonClicked()
	{
		if (!selectOnClick && fileEntryField != null)
		{
			if (fileEntryField.text != string.Empty)
			{
				string path = currentPath + slash + fileEntryField.text;
				if (!_keepOpen)
				{
					Hide();
				}
				callback(path);
			}
			else if (selectDirectory)
			{
				string path2 = currentPath;
				Hide();
				callback(path2);
			}
		}
		else if (selected > -1 && ((fileButtons[selected].isDir && selectDirectory) || (!fileButtons[selected].isDir && !selectDirectory)))
		{
			string fullPath = fileButtons[selected].fullPath;
			if (!_keepOpen)
			{
				Hide();
			}
			callback(fullPath);
		}
	}

	public void CancelButtonClicked()
	{
		if (canCancel)
		{
			Hide();
			if (callback != null)
			{
				callback(string.Empty);
			}
		}
	}

	private void FilterFormat(List<FileAndDirInfo> files, bool skipFileFormatCheck = false)
	{
		if (fileRemovePrefix != null && fileRemovePrefix != string.Empty)
		{
			List<FileAndDirInfo> list = new List<FileAndDirInfo>();
			for (int i = 0; i < files.Count; i++)
			{
				if (!Regex.IsMatch(files[i].Name, "^" + fileRemovePrefix))
				{
					list.Add(files[i]);
				}
			}
			foreach (FileAndDirInfo item in list)
			{
				files.Remove(item);
			}
		}
		if (string.IsNullOrEmpty(fileFormat) || skipFileFormatCheck)
		{
			return;
		}
		string[] array = fileFormat.Split('|');
		for (int j = 0; j < files.Count; j++)
		{
			bool flag = true;
			string text = string.Empty;
			if (files[j].Name.Contains("."))
			{
				text = files[j].Name.Substring(files[j].Name.LastIndexOf('.') + 1).ToLowerInvariant();
			}
			for (int k = 0; k < array.Length; k++)
			{
				if (text == array[k].Trim().ToLowerInvariant())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				files.RemoveAt(j);
				j--;
			}
		}
	}

	private void UpdateFileList()
	{
		ClearImageQueue();
		if (fileButtons == null)
		{
			fileButtons = new List<FileButton>();
		}
		else
		{
			for (int i = 0; i < fileButtons.Count; i++)
			{
				UnityEngine.Object.Destroy(fileButtons[i].gameObject);
			}
			fileButtons.Clear();
		}
		if (string.IsNullOrEmpty(currentPath))
		{
			for (int j = 0; j < drives.Count; j++)
			{
				CreateFileButton(drives[j], drives[j], dir: true, j, writeable: false);
			}
			return;
		}
		List<FileAndDirInfo> list = new List<FileAndDirInfo>();
		List<FileAndDirInfo> list2 = new List<FileAndDirInfo>();
		List<FileAndDirInfo> list3 = new List<FileAndDirInfo>();
		if (useFlatten)
		{
			List<FileEntry> list4 = new List<FileEntry>();
			if (fileFormat != null)
			{
				string regex = "\\.(" + fileFormat + ")$";
				if (includeRegularDirsInFlatten)
				{
					FileManager.FindAllFilesRegex(currentPath, regex, list4);
				}
				else
				{
					FileManager.FindVarFilesRegex(currentPath, regex, list4);
				}
			}
			else if (includeRegularDirsInFlatten)
			{
				FileManager.FindAllFiles(currentPath, "*", list4);
			}
			else
			{
				FileManager.FindVarFiles(currentPath, "*", list4);
			}
			foreach (FileEntry item6 in list4)
			{
				if (item6.Exists || FileManager.IsPackage(item6.Path))
				{
					FileAndDirInfo item = new FileAndDirInfo(item6);
					list.Add(item);
					continue;
				}
				Debug.LogError("Unable to read file " + item6.FullPath);
				if (statusField != null)
				{
					statusField.text = "Unable to read file " + item6.FullPath;
				}
			}
			list3 = list;
			FilterFormat(list, skipFileFormatCheck: true);
			SortFilesAndDirs(list3);
		}
		else
		{
			DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(currentPath);
			if ((selectDirectory && showFiles) || !selectDirectory)
			{
				try
				{
					List<FileEntry> files = directoryEntry.Files;
					foreach (FileEntry item7 in files)
					{
						if (item7.Exists || FileManager.IsPackage(item7.Path))
						{
							FileAndDirInfo item2 = new FileAndDirInfo(item7);
							list.Add(item2);
							continue;
						}
						Debug.LogError("Unable to read file " + item7.FullPath);
						if (statusField != null)
						{
							statusField.text = "Unable to read file " + item7.FullPath;
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("uFileBrowser: " + ex);
					if (statusField != null)
					{
						statusField.text = ex.Message;
					}
				}
				FilterFormat(list);
			}
			if (showDirs)
			{
				if (dirOption != null)
				{
					dirOption.gameObject.SetActive(value: true);
				}
				try
				{
					string empty = currentPath;
					if (empty == ".")
					{
						empty = string.Empty;
					}
					List<DirectoryEntry> subDirectories = directoryEntry.SubDirectories;
					foreach (DirectoryEntry item8 in subDirectories)
					{
						if (item8 is VarDirectoryEntry)
						{
							if (!browseVarFilesAsDirectories)
							{
								continue;
							}
							VarDirectoryEntry varDirectoryEntry = item8 as VarDirectoryEntry;
							if (currentPackageFilter != null && varDirectoryEntry.Package.RootDirectory == varDirectoryEntry)
							{
								if (varDirectoryEntry.Package.HasMatchingDirectories(currentPackageFilter))
								{
									FileAndDirInfo item3 = new FileAndDirInfo(item8, empty);
									list2.Add(item3);
								}
							}
							else
							{
								FileAndDirInfo item4 = new FileAndDirInfo(item8, empty);
								list2.Add(item4);
							}
						}
						else
						{
							FileAndDirInfo item5 = new FileAndDirInfo(item8, empty);
							list2.Add(item5);
						}
					}
				}
				catch (Exception ex2)
				{
					Debug.LogError("uFileBrowser: " + ex2.Message);
					if (statusField != null)
					{
						statusField.text = ex2.Message;
					}
				}
			}
			else if (dirOption != null)
			{
				dirOption.gameObject.SetActive(value: false);
			}
			switch (directoryOption)
			{
				case UserPreferences.DirectoryOption.Hide:
					list3 = list;
					SortFilesAndDirs(list3);
					break;
				case UserPreferences.DirectoryOption.Intermix:
					list3.AddRange(list);
					list3.AddRange(list2);
					SortFilesAndDirs(list3);
					break;
				case UserPreferences.DirectoryOption.ShowFirst:
					SortFilesAndDirs(list2);
					list3.AddRange(list2);
					SortFilesAndDirs(list);
					list3.AddRange(list);
					break;
				case UserPreferences.DirectoryOption.ShowLast:
					SortFilesAndDirs(list);
					list3.AddRange(list);
					SortFilesAndDirs(list2);
					list3.AddRange(list2);
					break;
			}
		}
		for (int k = 0; k < list3.Count; k++)
		{
			CreateFileButton(list3[k].Name, list3[k].FullName, list3[k].isDirectory, fileButtons.Count, list3[k].isWriteable);
		}
	}

	private void UpdateDirectoryList()
	{
		if (!directoryButtonPrefab)
		{
			return;
		}
		if (dirButtons == null)
		{
			dirButtons = new List<DirectoryButton>();
		}
		else
		{
			for (int i = 0; i < dirButtons.Count; i++)
			{
				UnityEngine.Object.Destroy(dirButtons[i].gameObject);
			}
			dirButtons.Clear();
		}
		if (shortCutButtons == null)
		{
			shortCutButtons = new List<ShortCutButton>();
		}
		else
		{
			for (int j = 0; j < shortCutButtons.Count; j++)
			{
				UnityEngine.Object.Destroy(shortCutButtons[j].gameObject);
			}
			shortCutButtons.Clear();
		}
		if (dirSpacers == null)
		{
			dirSpacers = new List<GameObject>();
		}
		else
		{
			for (int k = 0; k < dirSpacers.Count; k++)
			{
				UnityEngine.Object.Destroy(dirSpacers[k]);
			}
			dirSpacers.Clear();
		}
		if (shortCuts != null)
		{
			foreach (ShortCut shortCut in shortCuts)
			{
				if (!_onlyShowLatest || shortCut.isLatest)
				{
					CreateShortCutButton(shortCut, shortCutButtons.Count);
				}
			}
		}
		if (useFlatten || string.IsNullOrEmpty(currentPath))
		{
			return;
		}
		if (Regex.IsMatch(currentPath, "^[A-Za-z]:\\\\"))
		{
			CreateDirectoryButton("My Computer", string.Empty, string.Empty, dirButtons.Count);
			CreateDirectorySpacer();
		}
		else if (showInstallFolderInDirectoryList && !FileManager.IsDirectoryInPackage(currentPath))
		{
			CreateDirectoryButton("Root", string.Empty, ".", dirButtons.Count);
		}
		string[] array = currentPath.Split(slash[0]);
		string text = string.Empty;
		for (int l = 0; l < array.Length; l++)
		{
			if (!string.IsNullOrEmpty(array[l]) && array[l] != ".")
			{
				text = ((!(text == string.Empty)) ? (text + slash + array[l]) : array[l]);
				string package = string.Empty;
				string empty = string.Empty;
				string packageFolder = FileManager.PackageFolder;
				if (array[l].Contains(packageFolder))
				{
					empty = Regex.Replace(array[l], ".*:/", string.Empty);
					package = Regex.Replace(array[l], ":/.*", string.Empty);
					package = package.Replace(packageFolder, string.Empty);
					package = package.TrimStart('/', '\\');
				}
				else
				{
					empty = array[l] + slash;
				}
				CreateDirectoryButton(package, empty, text, dirButtons.Count);
			}
		}
	}

	private void Awake()
	{
		slash = Path.DirectorySeparatorChar.ToString();
		drives = new List<string>(Directory.GetLogicalDrives());
		if (renameFieldAction != null)
		{
			InputFieldAction inputFieldAction = renameFieldAction;
			inputFieldAction.onSubmitHandlers = (InputFieldAction.OnSubmit)Delegate.Combine(inputFieldAction.onSubmitHandlers, new InputFieldAction.OnSubmit(OnRenameConfirm));
		}
		if (onlyShowLatestToggle != null)
		{
			onlyShowLatestToggle.isOn = _onlyShowLatest;
		}
		if (openPackageButton != null)
		{
			openPackageButton.onClick.AddListener(OpenPackageInManager);
		}
		if (promotionalButton != null)
		{
			promotionalButton.onClick.AddListener(GoToPromotionalLink);
		}
		if (sortByPopup != null)
		{
			sortByPopup.currentValueNoCallback = _sortBy.ToString();
			UIPopup uIPopup = sortByPopup;
			uIPopup.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup.onValueChangeHandlers, new UIPopup.OnValueChange(SetSortBy));
		}
		if (directoryOptionPopup != null)
		{
			directoryOptionPopup.currentValueNoCallback = _directoryOption.ToString();
			UIPopup uIPopup2 = directoryOptionPopup;
			uIPopup2.onValueChangeHandlers = (UIPopup.OnValueChange)Delegate.Combine(uIPopup2.onValueChangeHandlers, new UIPopup.OnValueChange(SetDirectoryOption));
		}
	}
}
