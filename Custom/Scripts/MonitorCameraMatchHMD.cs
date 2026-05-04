using UnityEngine;

/// <summary>
/// <para>
/// 1) On each scene load (<see cref="SuperController.isLoading"/> becomes true), sets
/// <see cref="SuperController.activeUI"/> to <see cref="SuperController.ActiveUI.None"/> (closes
/// main menu and other fullscreen UI). Monitor mode is no longer forced off here;
/// Easy Mate now keeps the monitor camera aligned live while monitor mode stays on.
/// </para>
/// <para>
/// 2) Optionally forces <see cref="SuperController.GameMode.Play"/> (plugin toggle; turn off for Load for Edit).
/// </para>
/// <para>
/// 3) <b>Once per load</b>: aligns <see cref="SuperController.MonitorCenterCamera"/> to
/// <see cref="SuperController.lookCamera"/> (HMD) the first frame both exist during loading,
/// or if that never happens while loading, tries again once when loading finishes.
/// Does <b>not</b> run every frame afterward (avoids fighting VR locomotion).
/// </para>
/// <para>
/// 4) Disables proxy meshes on <see cref="SuperController.MonitorRig"/> (except <see cref="SuperController.MonitorUI"/>)
/// when the snap runs.
/// </para>
/// <para>
/// 5) When a load starts or when this plugin is destroyed, calls <see cref="SuperController.SelectModeOff"/>
/// (exits Tab / free-move-mouse, unlocks cursor). Monitor mode is left unchanged.
/// </para>
/// Session plugin on <c>CoreControl</c> recommended.
/// </summary>
public class MonitorCameraMatchHMD : MVRScript
{
	private bool _proxyRenderersDisabled;
	private bool _wasLoading;
	private bool _didPoseSnapThisLoad;

	public JSONStorableBool forcePlayModeOnSceneLoad;

	public override void Init()
	{
		base.Init();
		pluginLabelJSON.val = "MonitorCameraMatchHMD";

		forcePlayModeOnSceneLoad = new JSONStorableBool(
			"forcePlayModeOnSceneLoad",
			true
		);
		RegisterBool(forcePlayModeOnSceneLoad);
		CreateToggle(forcePlayModeOnSceneLoad, rightSide: false);
	}

	protected void Update()
	{
		SuperController sc = SuperController.singleton;
		if (sc == null)
		{
			return;
		}
		bool loading = sc.isLoading;

		if (loading && !_wasLoading)
		{
			OnSceneLoadOrTransitionStarted(sc);
		}

		if (loading)
		{
			if (!_didPoseSnapThisLoad)
			{
				TrySnapMonitorToHmdOnce(sc);
			}
		}
		else if (_wasLoading)
		{
			if (!_didPoseSnapThisLoad)
			{
				TrySnapMonitorToHmdOnce(sc);
			}
		}

		_wasLoading = loading;
	}

	private void OnSceneLoadOrTransitionStarted(SuperController sc)
	{
		_proxyRenderersDisabled = false;
		_didPoseSnapThisLoad = false;

		CloseGameMenu(sc);
		CleanupSelectMode(sc);

		if (forcePlayModeOnSceneLoad != null && forcePlayModeOnSceneLoad.val)
		{
			sc.gameMode = SuperController.GameMode.Play;
		}
	}

	/// <summary>
	/// Dismiss main menu, file browsers, and other panels that use <see cref="SuperController.activeUI"/>.
	/// </summary>
	private static void CloseGameMenu(SuperController sc)
	{
		if (sc == null)
		{
			return;
		}
		sc.activeUI = SuperController.ActiveUI.None;
	}

	/// <summary>
	/// Exit mouse free-move / any select mode. Monitor mode is left as-is.
	/// </summary>
	private static void CleanupSelectMode(SuperController sc)
	{
		if (sc == null)
		{
			return;
		}
		sc.SelectModeOff();
	}

	protected void OnDestroy()
	{
		CleanupSelectMode(SuperController.singleton);
	}

	/// <summary>
	/// Copies monitor camera pose to match HMD; runs at most once per load cycle.
	/// </summary>
	private void TrySnapMonitorToHmdOnce(SuperController sc)
	{
		if (_didPoseSnapThisLoad || sc.IsMonitorOnly)
		{
			return;
		}

		Camera hmdCam = sc.lookCamera;
		Camera monitorCam = sc.MonitorCenterCamera;
		if (hmdCam == null || monitorCam == null)
		{
			return;
		}

		Transform hmdT = hmdCam.transform;
		Transform monT = monitorCam.transform;
		monT.position = hmdT.position;
		monT.rotation = hmdT.rotation;

		if (!_proxyRenderersDisabled)
		{
			if (sc.MonitorRig != null)
			{
				DisableProxyRenderersOnMonitorRig(sc);
			}
			else
			{
				DisableNonCameraRenderersUnder(monitorCam.transform, null);
			}
			_proxyRenderersDisabled = true;
		}

		_didPoseSnapThisLoad = true;
	}

	private static void DisableProxyRenderersOnMonitorRig(SuperController sc)
	{
		Transform rig = sc.MonitorRig;
		Transform uiRoot = sc.MonitorUI;
		DisableNonCameraRenderersUnder(rig, uiRoot);
	}

	private static void DisableNonCameraRenderersUnder(Transform root, Transform uiRoot)
	{
		if (root == null)
		{
			return;
		}
		Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
		int i;
		for (i = 0; i < renderers.Length; i++)
		{
			Renderer r = renderers[i];
			if (r == null)
			{
				continue;
			}
			if (r.gameObject.GetComponent<Camera>() != null)
			{
				continue;
			}
			if (uiRoot != null && r.transform.IsChildOf(uiRoot))
			{
				continue;
			}
			r.enabled = false;
		}
	}
}
