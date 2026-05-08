using UnityEngine;
using UnityEngine.PostProcessing;
using System;
using System.Collections.Generic;


namespace MacGruber
{
	namespace PostMagic 
	{		
		public class DepthOfField : MVRScript
		{
			private Manager manager;
			private DepthOfFieldModel model;
			private DepthOfFieldModel.Settings settings;
			
			private JSONStorableBool active;
			private JSONStorableFloat focusDistance;
			private JSONStorableBool autoFocus;
			private JSONStorableBool autoFocusWindowCamera;
			private JSONStorableBool autoFocusFocalLength;
			private JSONStorableFloat autoFocusAdjustDuration;
			private JSONStorableFloat autoFocusNearDistance;
			private JSONStorableFloat autoFocusFarDistance;
			private JSONStorableFloat autoFocusNearFocalLength;
			private JSONStorableFloat autoFocusFarFocalLength;
			
			private JSONStorableFloat aperture;
			private JSONStorableFloat focalLength;
			private JSONStorableBool useCameraFov;
			private JSONStorableStringChooser kernelSize;
			
			private bool setAutoFocusNear = false;
			private bool setAutoFocusFar = false;
			
			
			private Transform focusPoint;
			
			public override void Init()
			{
				manager = Utils.FindWithinSamePlugin<Manager>(this);
				model = manager.profile.depthOfField;
				
				Utils.SetupInfoText(this, 
					"<b>FocusDistance:</b> Distance to the point of focus.\n\n" + 					
					"<b>Aperture:</b> Ratio of aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is.\n\n" + 
					"<b>FocalLength:</b> Distance between the lens and the film. The larger the value is, the shallower the depth of field is.\n\n" +
					"<b>UseCameraFov:</b> Calculate the focal length automatically from the field-of-view value set on the camera. Using this setting isn't recommended.\n\n" +
					"<b>KernelSize:</b> Convolution kernel size of the bokeh filter, which determines the maximum radius of bokeh. It also affects the performance, smaller = faster.\n\n" +
					"<b>AutoFocus:</b> Continuously auto-adjust FocusDistance. Place an 'Empty' atom in your scene and name it 'AutoFocusPoint' to indicate the focus point. Attach it to a Person head or animate it, whatever.\n\n" + 
					"<b>AutoFocus WindowCamera:</b> Use WindowCamera atom instead of Player position to determine AutoFocus distance.\n\n" + 
					"<b>AutoFocus FocalLength:</b> AutoFocus also blends FocalLength based on a Near and Far setting. Usage:\n1. Enable 'AutoFocus'.\n2. Disable this toggle.\n3. Move your camera into a nice Near position and setup 'FocalLength' to your liking.\n4. Press 'Set AutoFocus Near' to save the setting.\n5. Do the same for a Far position.\n6. Reenable 'AutoFocus FocalLength'.\n\n" + 
					"<b>AutoFocus AdjustTime:</b> Time until AutoFocus adjusts to the current distance. Set to zero for instant adjustment.\n\n",
					1200.0f, true
				);
				
				active = Utils.SetupToggle(this, "DepthOfField Enabled", false, false);
				active.setCallbackFunction  += (bool v) => { model.enabled = enabledJSON.val = v; };
				
				focusDistance = Utils.SetupSliderFloat(this, "FocusDistance", 1.5f, 0.1f, 20.0f, false);
				focusDistance.setCallbackFunction  += (float v) => { 
					if (!autoFocus.val || focusPoint == null)
					{
						settings.focusDistance = v;
						model.settings = settings;
					}
				};
								
				aperture = Utils.SetupSliderFloat(this, "Aperture", 5.6f, 0.05f, 32.0f, false);
				aperture.setCallbackFunction  += (float v) => { settings.aperture = v; model.settings = settings; };
				
				focalLength = Utils.SetupSliderFloat(this, "FocalLength", 50.0f, 1.0f, 300.0f, false);
				focalLength.setCallbackFunction  += (float v) => { settings.focalLength = v; model.settings = settings; };
				
				useCameraFov = Utils.SetupToggle(this, "UseCameraFov", false, false);
				useCameraFov.setCallbackFunction  += (bool v) => { settings.useCameraFov = v; model.settings = settings; };
							
				kernelSize = Utils.SetupEnumChooser(this, "KernelSize", DepthOfFieldModel.KernelSize.Medium, false, 
					(DepthOfFieldModel.KernelSize v) => { settings.kernelSize = v; model.settings = settings; });


				autoFocus = Utils.SetupToggle(this, "AutoFocus", true, false);
				autoFocus.setCallbackFunction  += (bool v) => { 
					if (!v)
					{
						settings.focusDistance = focusDistance.val;
						model.settings = settings;
					}
				};
				
				autoFocusWindowCamera = Utils.SetupToggle(this, "AutoFocus WindowCamera", false, false);
				autoFocusFocalLength = Utils.SetupToggle(this, "AutoFocus FocalLength", true, false);
				
				
				Utils.SetupButton(this, "Set AutoFocus Near", () => { setAutoFocusNear = true; }, false);
				Utils.SetupButton(this, "Set AutoFocus Far", () => { setAutoFocusFar = true; }, false);
				setAutoFocusNear = false;
				setAutoFocusFar = false;				
				
				autoFocusNearDistance = new JSONStorableFloat("AutoFocusNearDistance", 1.0f, 0.1f, 20.0f, true, true);
				autoFocusFarDistance = new JSONStorableFloat("AutoFocusFarDistance", 5.0f, 0.1f, 20.0f, true, true);
				autoFocusNearFocalLength = new JSONStorableFloat("AutoFocusNearFocalLength", 50.0f, 1.0f, 300.0f, true, true);
				autoFocusFarFocalLength = new JSONStorableFloat("AutoFocusFarFocalLength", 140.0f, 1.0f, 300.0f, true, true);				
				autoFocusNearDistance.storeType = JSONStorableParam.StoreType.Full;
				autoFocusFarDistance.storeType = JSONStorableParam.StoreType.Full;
				autoFocusNearFocalLength.storeType = JSONStorableParam.StoreType.Full;
				autoFocusFarFocalLength.storeType = JSONStorableParam.StoreType.Full;
				RegisterFloat(autoFocusNearDistance);
				RegisterFloat(autoFocusFarDistance);
				RegisterFloat(autoFocusNearFocalLength);
				RegisterFloat(autoFocusFarFocalLength);				

				autoFocusAdjustDuration = Utils.SetupSliderFloat(this, "AutoFocus AdjustTime", 0.04f, 0.0f, 0.5f, false);
				
				settings = DepthOfFieldModel.Settings.defaultSettings;
				settings.focusDistance = focusDistance.val;
				settings.aperture = aperture.val;
				settings.focalLength = focalLength.val;
				settings.useCameraFov = useCameraFov.val;
				model.settings = settings;
				
				kernelSize.setCallbackFunction(kernelSize.val);
				
				active.setCallbackFunction(active.val);
				enabledJSON.setCallbackFunction += (bool v) => { active.val = v; };
				
				FindFocusPoint();
				SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChanged;
			}
			
			private void OnDestroy()
			{
				SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChanged;
			}
			
			private void Update()
			{
				if (manager != null && focusPoint != null && autoFocus.val)
				{
					SuperController sc = SuperController.singleton;
					Camera camera = CameraTarget.centerTarget.targetCamera;
					if (autoFocusWindowCamera.val)
						camera = manager.windowCamera;
					else if (sc.hiResScreenshotPreview.gameObject.activeSelf)
						camera = manager.screenshotCamera;
					else if (sc.screenshotPreview.gameObject.activeSelf)
						camera = manager.thumbnailCamera;
					
					Vector3 position = camera.WorldToScreenPoint(focusPoint.position);
					float distance = Mathf.Max(position.z, 0.1f);
					
					if (setAutoFocusNear)
					{
						autoFocusNearDistance.val = distance;
						autoFocusNearFocalLength.val = focalLength.val;
					}
					if (setAutoFocusFar)
					{
						autoFocusFarDistance.val = distance;
						autoFocusFarFocalLength.val = focalLength.val;
					}
					setAutoFocusNear = setAutoFocusFar = false;
					
					float focusDistanceLerp = 1.0f;
					if (autoFocusAdjustDuration.val > 0.001f)
						focusDistanceLerp = Mathf.Min(Time.deltaTime / autoFocusAdjustDuration.val, 1.0f);
					settings.focusDistance = Mathf.Lerp(settings.focusDistance, distance, focusDistanceLerp);
					
					if (autoFocusFocalLength.val)
					{
						float focalLengthLerp = Mathf.InverseLerp(autoFocusNearDistance.val, autoFocusFarDistance.val, settings.focusDistance);
						settings.focalLength = Mathf.Lerp(autoFocusNearFocalLength.val, autoFocusFarFocalLength.val, focalLengthLerp);
					}
					else
					{
						settings.focalLength = focalLength.val;
					}
					
					model.settings = settings;
				}
			}
			
			private void OnAtomUIDsChanged(List<string> atomUIDs)
			{
				FindFocusPoint();
			}
			
			private void FindFocusPoint()
			{
				bool needReset = focusPoint != null;
				Atom atom = GetAtomById("AutoFocusPoint");
				if (atom != null)
				{
					JSONStorable storable = atom.GetStorableByID("control");
					if (storable != null)
					{
						focusPoint = storable.transform;
						return;
					}
				}
				
				if (needReset && autoFocus.val)
				{
					focusPoint = null;
					settings.focusDistance = focusDistance.val;
					model.settings = settings;
				}
			}
		}
	}
}