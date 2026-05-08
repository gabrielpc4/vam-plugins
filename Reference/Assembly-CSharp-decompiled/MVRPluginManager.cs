using System;
using System.Collections.Generic;
using System.IO;
using DynamicCSharp;
using Mono.CSharp;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class MVRPluginManager : JSONStorable
{
	public Transform pluginPanelPrefab;

	public Transform scriptControllerPanelPrefab;

	public Transform scriptUIPrefab;

	public Transform configurableSliderPrefab;

	public Transform configurableTogglePrefab;

	public Transform configurableColorPickerPrefab;

	public Transform configurableButtonPrefab;

	public Transform configurablePopupPrefab;

	public Transform configurableScrollablePopupPrefab;

	public Transform configurableTextFieldPrefab;

	public Transform configurableSpacerPrefab;

	public Transform pluginListPanel;

	public RectTransform scriptUIParent;

	public Transform pluginContainer;

	protected List<MVRPlugin> plugins;

	protected Dictionary<string, bool> pluginUIDs;

	protected ScriptDomain domain;

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if ((includeAppearance && includePhysical) || forceStore)
		{
			needsStore = true;
			JSONClass jc = (JSONClass)(jSON["plugins"] = new JSONClass());
			foreach (MVRPlugin plugin in plugins)
			{
				plugin.pluginURLJSON.StoreJSON(jc, forceStore);
			}
		}
		return jSON;
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		insideRestore = true;
		if (restoreAppearance && restorePhysical)
		{
			if (jc["plugins"] != null)
			{
				RemoveAllPlugins();
				JSONClass asObject = jc["plugins"].AsObject;
				if (asObject != null)
				{
					foreach (string key in asObject.Keys)
					{
						MVRPlugin mVRPlugin = CreatePluginWithId(key);
						pluginUIDs.Add(key, value: true);
						mVRPlugin.pluginURLJSON.RestoreFromJSON(asObject);
					}
				}
			}
			else if (setMissingToDefault)
			{
				RemoveAllPlugins();
			}
		}
		insideRestore = false;
	}

	protected string CreatePluginUID()
	{
		string text = "plugin#";
		for (int i = 0; i < 1000; i++)
		{
			string text2 = text + i;
			if (!pluginUIDs.ContainsKey(text2))
			{
				text = text2;
				pluginUIDs.Add(text, value: true);
				break;
			}
		}
		return text;
	}

	protected void BeginBrowse(JSONStorableUrl jsurl)
	{
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory("Custom/Scripts", allowNavigationAboveRegularDirectories: true, useFullPaths: true, generateAllFlattenedShortcut: true, includeRegularDirsInFlattenedShortcut: true);
		ShortCut shortCut = new ShortCut();
		shortCut.displayName = "Root";
		shortCut.path = Path.GetFullPath(".");
		shortCutsForDirectory.Insert(0, shortCut);
		jsurl.shortCuts = shortCutsForDirectory;
	}

	protected MVRPlugin CreatePluginWithId(string pluginUID)
	{
		MVRPlugin mvrp = new MVRPlugin();
		mvrp.uid = pluginUID;
		plugins.Add(mvrp);
		JSONStorableUrl jSONStorableUrl = new JSONStorableUrl(pluginUID, string.Empty, (JSONStorableString.SetStringCallback)delegate
		{
			SyncPluginUrl(mvrp);
		}, "cs|cslist|dll", "Custom/Scripts");
		mvrp.pluginURLJSON = jSONStorableUrl;
		jSONStorableUrl.beginBrowseWithObjectCallback = BeginBrowse;
		if (pluginPanelPrefab != null)
		{
			Transform transform = UnityEngine.Object.Instantiate(pluginPanelPrefab);
			if (pluginListPanel != null)
			{
				transform.SetParent(pluginListPanel, worldPositionStays: false);
			}
			else
			{
				transform.gameObject.SetActive(value: false);
			}
			mvrp.configUI = transform;
			MVRPluginUI component = transform.GetComponent<MVRPluginUI>();
			if (component != null)
			{
				mvrp.scriptControllerContent = component.scriptControllerContent;
				jSONStorableUrl.fileBrowseButton = component.fileBrowseButton;
				jSONStorableUrl.clearButton = component.clearButton;
				jSONStorableUrl.reloadButton = component.reloadButton;
				jSONStorableUrl.text = component.urlText;
				if (component.uidText != null)
				{
					component.uidText.text = pluginUID;
				}
				if (component.removeButton != null)
				{
					component.removeButton.onClick.AddListener(delegate
					{
						RemovePlugin(mvrp);
					});
				}
			}
		}
		return mvrp;
	}

	protected void CreatePlugin()
	{
		string pluginUID = CreatePluginUID();
		CreatePluginWithId(pluginUID);
	}

	protected void DestroyScriptController(MVRScriptController mvrsc)
	{
		if (mvrsc.script != null)
		{
			if (mvrsc.script.enabledJSON != null)
			{
				mvrsc.script.enabledJSON.toggle = null;
			}
			if (mvrsc.script.pluginLabelJSON != null)
			{
				mvrsc.script.pluginLabelJSON.inputField = null;
				mvrsc.script.pluginLabelJSON.inputFieldAction = null;
			}
			UnityEngine.Object.Destroy(mvrsc.script);
			mvrsc.script = null;
		}
		if (mvrsc.configUI != null)
		{
			UnityEngine.Object.Destroy(mvrsc.configUI.gameObject);
			mvrsc.configUI = null;
		}
		if (mvrsc.customUI != null)
		{
			Canvas componentInChildren = mvrsc.customUI.GetComponentInChildren<Canvas>();
			if (componentInChildren != null)
			{
				SuperController.singleton.RemoveCanvas(componentInChildren);
			}
			UnityEngine.Object.Destroy(mvrsc.customUI.gameObject);
			mvrsc.customUI = null;
		}
		if (mvrsc.gameObject != null)
		{
			UnityEngine.Object.Destroy(mvrsc.gameObject);
			mvrsc.gameObject = null;
		}
	}

	protected void RemovePluginScriptControllers(MVRPlugin mvrp)
	{
		if (mvrp.scriptControllers != null)
		{
			foreach (MVRScriptController scriptController in mvrp.scriptControllers)
			{
				if (scriptController.script != null)
				{
					containingAtom.UnregisterAdditionalStorable(scriptController.script);
				}
				DestroyScriptController(scriptController);
			}
		}
		mvrp.scriptControllers = new List<MVRScriptController>();
	}

	protected void RemovePlugin(MVRPlugin mvrp)
	{
		RemovePluginScriptControllers(mvrp);
		if (mvrp.configUI != null)
		{
			UnityEngine.Object.Destroy(mvrp.configUI.gameObject);
			mvrp.configUI = null;
		}
		pluginUIDs.Remove(mvrp.uid);
		plugins.Remove(mvrp);
	}

	protected void RemoveAllPlugins()
	{
		List<MVRPlugin> list = new List<MVRPlugin>(plugins);
		foreach (MVRPlugin item in list)
		{
			RemovePlugin(item);
		}
	}

	protected MVRScriptController CreateScriptController(MVRPlugin mvrp, ScriptType type)
	{
		MVRScriptController mVRScriptController = new MVRScriptController();
		GameObject gameObject = new GameObject(mvrp.uid + "temp");
		gameObject.transform.SetParent(pluginContainer);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		mVRScriptController.gameObject = gameObject;
		ScriptProxy scriptProxy = type.CreateInstance(gameObject);
		if (scriptProxy == null)
		{
			SuperController.LogError("Failed to create instance of " + mvrp.pluginURLJSON.val);
			return null;
		}
		string text2 = (gameObject.name = mvrp.uid + "_" + scriptProxy.GetInstanceType().ToString());
		if (scriptUIPrefab != null)
		{
			Transform transform = UnityEngine.Object.Instantiate(scriptUIPrefab);
			if (scriptUIParent != null)
			{
				transform.SetParent(scriptUIParent, worldPositionStays: false);
			}
			transform.gameObject.SetActive(value: false);
			mVRScriptController.customUI = transform;
		}
		Toggle toggle = null;
		InputField inputField = null;
		InputFieldAction inputFieldAction = null;
		if (scriptControllerPanelPrefab != null)
		{
			Transform transform2 = UnityEngine.Object.Instantiate(scriptControllerPanelPrefab);
			if (mvrp.scriptControllerContent != null)
			{
				transform2.SetParent(mvrp.scriptControllerContent, worldPositionStays: false);
			}
			MVRScriptControllerUI component = transform2.GetComponent<MVRScriptControllerUI>();
			if (component != null)
			{
				if (component.label != null)
				{
					component.label.text = text2;
				}
				if (component.openUIButton != null)
				{
					component.openUIButton.onClick.AddListener(mVRScriptController.OpenUI);
				}
				toggle = component.enabledToggle;
				inputField = component.userLabelInputField;
				inputFieldAction = component.userLabelInputFieldAction;
			}
			mVRScriptController.configUI = transform2;
		}
		MVRScript mVRScript = (mVRScriptController.script = gameObject.GetComponent<MVRScript>());
		if (mVRScript.ShouldIgnore())
		{
			DestroyScriptController(mVRScriptController);
			mVRScriptController = null;
		}
		else
		{
			mVRScript.ForceAwake();
			mVRScript.containingAtom = containingAtom;
			mVRScript.manager = this;
			mVRScript.Init();
			if (mVRScript.enabledJSON != null)
			{
				mVRScript.enabledJSON.toggle = toggle;
			}
			if (mVRScript.pluginLabelJSON != null)
			{
				mVRScript.pluginLabelJSON.inputField = inputField;
				mVRScript.pluginLabelJSON.inputFieldAction = inputFieldAction;
			}
			if (mVRScriptController.customUI != null)
			{
				mVRScript.UITransform = mVRScriptController.customUI;
				mVRScript.InitUI();
				MVRScriptUI componentInChildren = mVRScriptController.customUI.GetComponentInChildren<MVRScriptUI>();
				if (componentInChildren != null && componentInChildren.closeButton != null)
				{
					componentInChildren.closeButton.onClick.AddListener(mVRScriptController.CloseUI);
				}
				Canvas componentInChildren2 = mVRScriptController.customUI.GetComponentInChildren<Canvas>();
				if (componentInChildren2 != null)
				{
					SuperController.singleton.AddCanvas(componentInChildren2);
				}
			}
			containingAtom.RegisterAdditionalStorable(mVRScript);
			if (insideRestore)
			{
				containingAtom.RestoreFromLast(mVRScript);
			}
		}
		return mVRScriptController;
	}

	protected void SyncPluginUrl(MVRPlugin mvrp)
	{
		if (UserPreferences.singleton == null || UserPreferences.singleton.enablePlugins)
		{
			if (domain == null)
			{
				domain = ScriptDomain.CreateDomain("MVRPlugins", initCompiler: true);
			}
			RemovePluginScriptControllers(mvrp);
			string val = mvrp.pluginURLJSON.val;
			if (domain == null || !(pluginContainer != null) || val == null || !(val != string.Empty))
			{
				return;
			}
			if (FileManager.FileExists(val))
			{
				try
				{
					Location.Reset();
					if (val.EndsWith(".cslist") || val.EndsWith(".dll"))
					{
						ScriptAssembly scriptAssembly = null;
						if (!val.EndsWith(".cslist"))
						{
							scriptAssembly = ((!FileManager.IsFileInPackage(val)) ? domain.LoadAssembly(val) : domain.LoadAssembly(FileManager.ReadAllBytes(val)));
						}
						else
						{
							string directoryName = FileManager.GetDirectoryName(val);
							List<string> list = new List<string>();
							bool flag = false;
							using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(val, restrictPath: true))
							{
								StreamReader streamReader = fileEntryStreamReader.StreamReader;
								string text;
								while ((text = streamReader.ReadLine()) != null)
								{
									string text2 = ((!(directoryName != string.Empty)) ? text.Trim() : (directoryName + "/" + text.Trim()));
									if (text2 != string.Empty)
									{
										text2 = text2.Replace('/', '\\');
										list.Add(text2);
										if (FileManager.IsFileInPackage(text2))
										{
											flag = true;
										}
									}
								}
							}
							if (list.Count > 0)
							{
								try
								{
									if (flag)
									{
										List<string> list2 = new List<string>();
										foreach (string item in list)
										{
											list2.Add(FileManager.ReadAllText(item));
										}
										scriptAssembly = domain.CompileAndLoadScriptSources(list2.ToArray());
									}
									else
									{
										scriptAssembly = domain.CompileAndLoadScriptFiles(list.ToArray());
									}
									if (scriptAssembly == null)
									{
										SuperController.LogError("Compile of " + val + " failed. Errors:");
										string[] errors = domain.CompilerService.Errors;
										foreach (string text3 in errors)
										{
											if (!text3.StartsWith("[CS]"))
											{
												SuperController.LogError(text3 + "\n");
											}
										}
										return;
									}
								}
								catch (Exception ex)
								{
									SuperController.LogError("Compile of " + val + " failed. Exception: " + ex);
									SuperController.LogError("Compile of " + val + " failed. Errors:");
									string[] errors2 = domain.CompilerService.Errors;
									foreach (string err in errors2)
									{
										SuperController.LogError(err);
									}
									return;
								}
							}
						}
						if (scriptAssembly != null)
						{
							ScriptType[] array = scriptAssembly.FindAllSubtypesOf<MVRScript>();
							if (array.Length > 0)
							{
								ScriptType[] array2 = array;
								foreach (ScriptType type in array2)
								{
									MVRScriptController mVRScriptController = CreateScriptController(mvrp, type);
									if (mVRScriptController != null)
									{
										mvrp.scriptControllers.Add(mVRScriptController);
									}
								}
							}
							else
							{
								Debug.LogError("No MVRScript types found");
							}
						}
						else
						{
							SuperController.LogError("Unable to load assembly from " + val);
						}
					}
					else
					{
						ScriptType scriptType = null;
						try
						{
							scriptType = ((!FileManager.IsFileInPackage(val)) ? domain.CompileAndLoadScriptFile(val) : domain.CompileAndLoadScriptSource(FileManager.ReadAllText(val)));
							if (scriptType == null)
							{
								SuperController.LogError("Compile of " + val + " failed. Errors:");
								string[] errors3 = domain.CompilerService.Errors;
								foreach (string err2 in errors3)
								{
									SuperController.LogError(err2);
								}
								return;
							}
						}
						catch (Exception ex2)
						{
							SuperController.LogError("Compile of " + val + " failed. Exception: " + ex2);
							SuperController.LogError("Compile of " + val + " failed. Errors:");
							string[] errors4 = domain.CompilerService.Errors;
							foreach (string err3 in errors4)
							{
								SuperController.LogError(err3);
							}
							return;
						}
						if (scriptType.IsSubtypeOf<MVRScript>())
						{
							MVRScriptController mVRScriptController2 = CreateScriptController(mvrp, scriptType);
							if (mVRScriptController2 != null)
							{
								mvrp.scriptControllers.Add(mVRScriptController2);
							}
						}
						else
						{
							SuperController.LogError("Script loaded at " + val + " must inherit from MVRScript");
						}
					}
					return;
				}
				catch (Exception ex3)
				{
					SuperController.LogError("Exception during compile of " + val + ": " + ex3);
					return;
				}
			}
			SuperController.LogError("Plugin file " + val + " does not exist");
		}
		else
		{
			SuperController.LogError("Attempted to load plugin when plugins option is disabled. To enable, see User Preferences -> Security tab");
			SuperController.singleton.SetActiveUI("MainMenu");
			SuperController.singleton.SetMainMenuTab("TabUserPrefs");
			SuperController.singleton.SetUserPrefsTab("TabSecurity");
		}
	}

	protected void Init()
	{
		if (pluginContainer == null)
		{
			pluginContainer = base.transform;
		}
		plugins = new List<MVRPlugin>();
		pluginUIDs = new Dictionary<string, bool>();
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		MVRPluginManagerUI componentInChildren = UITransform.GetComponentInChildren<MVRPluginManagerUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		scriptUIParent = componentInChildren.scriptUIParent;
		pluginListPanel = componentInChildren.pluginListPanel;
		if (pluginListPanel != null)
		{
			foreach (MVRPlugin plugin in plugins)
			{
				if (plugin.configUI != null)
				{
					plugin.configUI.SetParent(pluginListPanel, worldPositionStays: false);
					plugin.configUI.gameObject.SetActive(value: true);
				}
				foreach (MVRScriptController scriptController in plugin.scriptControllers)
				{
					if (scriptController.customUI != null && scriptUIParent != null)
					{
						scriptController.customUI.SetParent(scriptUIParent, worldPositionStays: false);
					}
				}
			}
		}
		if (componentInChildren.addPluginButton != null)
		{
			componentInChildren.addPluginButton.onClick.AddListener(CreatePlugin);
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}

	protected void OnDestroy()
	{
		foreach (MVRPlugin plugin in plugins)
		{
			foreach (MVRScriptController scriptController in plugin.scriptControllers)
			{
				if (scriptController.customUI != null)
				{
					UnityEngine.Object.Destroy(scriptController.customUI.gameObject);
				}
			}
			if (plugin.configUI != null)
			{
				UnityEngine.Object.Destroy(plugin.configUI.gameObject);
			}
		}
	}
}
