/* /////////////////////////////////////////////////////////////////////////////////////////////////
PostMagic v0.1 by MacGruber.
Enables and exposes hidden PostProcessing settings in VaM.

Version 0.2 2019-10-12
	Added FXAA and TAA.
	Added Vignette.
	Added ChromaticAberration.
	Auto-disable MotionBlur in VR.
	DepthOfField AutoFocus honors screenshot and thumbnail cameras.
	DepthOfField AutoFocus can blend FocalLength based on Near and Far setting.
	DepthOfField AutoFocus damping for slow adjustment.

Version 0.1 2019-10-06
	Initial release.

///////////////////////////////////////////////////////////////////////////////////////////////// */

using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.XR;
using System;
using System.Collections.Generic;


namespace MacGruber
{
	namespace PostMagic 
	{		
		public class Manager : MVRScript
		{			
			public PostProcessingProfile profile { get; private set; }
			public Camera screenshotCamera { get; private set; }
			public Camera thumbnailCamera { get; private set; }
			public Camera windowCamera { get; private set; }
			public static bool gameIsVR { get; private set; }
					
			private PostProcessingBehaviour mainBehaviour;
			private PostProcessingBehaviour screenshotBehaviour;
			private PostProcessingBehaviour thumbnailBehaviour;
			private PostProcessingBehaviour windowBehaviour;
			private CameraHook cameraHook;
			
			private JSONStorableBool postMagicEnabled;
			
			public override void Init()
			{
				// Workaround: PostMagic sub-plugins crash on scene load, when Manager is disabled.
				postMagicEnabled = new JSONStorableBool("PostMagicEnabled", true);
				postMagicEnabled.storeType = JSONStorableParam.StoreType.Full;
				RegisterBool(postMagicEnabled);
				postMagicEnabled.setCallbackFunction += (bool v) => { enabled = enabledJSON.val = v; };
				enabledJSON.isStorable = false; 
				enabledJSON.setCallbackFunction += (bool v) => { postMagicEnabled.valNoCallback = v; };
			}
					
			private void OnEnable()
			{
				SuperController.LogMessage("MacGruber PostMagic Enabled");
				
				gameIsVR = XRSettings.isDeviceActive && !string.IsNullOrEmpty(XRSettings.loadedDeviceName);
				
				// Main Camera				
				Camera mainCamera = CameraTarget.centerTarget.targetCamera;			
				mainBehaviour = Utils.GetOrAddComponent<PostProcessingBehaviour>(mainCamera);
				mainBehaviour.enabled = true;				
				profile = mainBehaviour.profile;
				if (profile == null)
					mainBehaviour.profile = profile = ScriptableObject.CreateInstance<PostProcessingProfile>();	
				
				// Main Camera - UI Separation
				cameraHook = Utils.GetOrAddComponent<CameraHook>(mainCamera);
				cameraHook.enabled = true;
				
				// Screenshot Camera
				screenshotCamera = SuperController.singleton.hiResScreenshotCamera;
				screenshotBehaviour = Utils.GetOrAddComponent<PostProcessingBehaviour>(screenshotCamera);
				screenshotBehaviour.enabled = true;
				screenshotBehaviour.profile = profile;
				
				// Thumbnail Camera
				thumbnailCamera = SuperController.singleton.screenshotCamera;
				thumbnailBehaviour = Utils.GetOrAddComponent<PostProcessingBehaviour>(thumbnailCamera);
				thumbnailBehaviour.enabled = true;
				thumbnailBehaviour.profile = profile;
				
				// Window Camera
				List<Atom> atoms = SuperController.singleton.GetAtoms();
				Atom windowAtom = atoms.Find((Atom a) => { return a.type == "WindowCamera"; });
				if (windowAtom != null)
				{
					CameraControl cameraControl = windowAtom.GetStorableByID("CameraControl") as CameraControl;
					if (cameraControl != null)
					{
						windowCamera = cameraControl.cameraToControl;
						windowBehaviour = Utils.GetOrAddComponent<PostProcessingBehaviour>(windowCamera);
						windowBehaviour.enabled = true;
						windowBehaviour.profile = profile;
					}
				}
			}					
							
			private void OnDisable()
			{
				SuperController.LogMessage("MacGruber PostMagic Disabled (Restart VaM to ensure its fully gone!)");
				
				// Workaround: Calling Destroy would interfere with hot-reload of this plugin. So we just disable.
				if (mainBehaviour != null)
					mainBehaviour.enabled = false;
				if (screenshotBehaviour != null)
					screenshotBehaviour.enabled = false; 
				if (thumbnailBehaviour != null)
					thumbnailBehaviour.enabled = false;
				if (windowBehaviour != null)
					windowBehaviour.enabled = false;
				if (cameraHook != null)
					cameraHook.enabled = false;
			}
		}
		
		public class CameraHook : MonoBehaviour
		{
			private Camera uiCamera;
			private Camera mainCamera;
			private int uiMask = 0;
			
			private int mainCullingMask = -1;			
			
			private void OnEnable()
			{				
				uiMask = 1 << LayerMask.NameToLayer("UI")
				       | 1 << LayerMask.NameToLayer("LoadUI")
				       | 1 << LayerMask.NameToLayer("ScreenUI")
				       | 1 << LayerMask.NameToLayer("GUI");
				
				mainCamera = GetComponent<Camera>();
				mainCullingMask = mainCamera.cullingMask;
				mainCamera.cullingMask &= ~uiMask;			
				
				Transform t = transform.Find("CameraHook");
				if (t == null)
				{
					t = new GameObject("CameraHook").transform;
					t.parent = transform;
				}
				uiCamera = Utils.GetOrAddComponent<Camera>(t);
				
				uiCamera.CopyFrom(mainCamera);
				uiCamera.transform.localPosition = Vector3.zero;
				uiCamera.transform.localRotation = Quaternion.identity;
				uiCamera.cullingMask = uiMask;
				uiCamera.clearFlags = CameraClearFlags.Depth;
				uiCamera.depth = mainCamera.depth + 1;
				uiCamera.enabled = true;
			}
			
			private void OnDisable()
			{			
				if (mainCamera != null)
					mainCamera.cullingMask = mainCullingMask;
				uiCamera.enabled = false;
			}
			
			private void OnPreCull()
			{
				uiCamera.transform.localPosition = Vector3.zero;
				uiCamera.transform.localRotation = Quaternion.identity;
				if (!Manager.gameIsVR)
					uiCamera.fieldOfView = mainCamera.fieldOfView;
			}
		}
	}
}