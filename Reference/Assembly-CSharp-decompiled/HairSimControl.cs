using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPUTools.Hair.Scripts;
using GPUTools.Hair.Scripts.Geometry.Create;
using GPUTools.Hair.Scripts.Types;
using MeshVR;
using UnityEngine;
using UnityEngine.UI;

public class HairSimControl : ScaleChangeReceiverJSONStorable
{
	public enum ShaderType
	{
		Fast,
		Quality,
		QualityThicken,
		QualityThickenMore,
		NonStandard
	}

	public HairSettings hairSettings;

	public RuntimeHairGeometryCreator creator;

	protected bool _styleMode;

	protected Text simNearbyJointCountText;

	protected Transform styleModelPanel;

	protected JSONStorableAction startStyleModeAction;

	protected JSONStorableAction resetAndStartStyleModeAction;

	protected JSONStorableAction cancelStyleModeAction;

	protected JSONStorableAction keepStyleAction;

	protected JSONStorableBool styleModeAllowControlOtherNodesJSON;

	protected JSONStorableFloat styleJointsSearchDistanceJSON;

	protected JSONStorableAction clearStyleJointsAction;

	protected Text styleStatusText;

	protected Thread rebuildStyleJointsThread;

	protected string threadError;

	protected bool isRebuildingStyleJoints;

	protected JSONStorableAction rebuildStyleJointsAction;

	protected JSONStorableFloat styleModeGravityMultiplierJSON;

	protected JSONStorableBool styleModeShowCurlsJSON;

	protected JSONStorableFloat styleModeUpHairPullStrengthJSON;

	protected JSONStorableFloat styleModeCollisionRadiusJSON;

	protected JSONStorableFloat styleModeCollisionRadiusRootJSON;

	protected HairSimControlTools _hairSimControlTools;

	protected JSONStorableBool styleModeShowTool1JSON;

	protected JSONStorableBool styleModeShowTool2JSON;

	protected JSONStorableBool styleModeShowTool3JSON;

	protected JSONStorableBool styleModeShowTool4JSON;

	protected JSONStorableBool simulationEnabledJSON;

	protected JSONStorableBool collisionEnabledJSON;

	protected JSONStorableFloat collisionRadiusJSON;

	protected JSONStorableFloat collisionRadiusRootJSON;

	protected JSONStorableFloat dragJSON;

	protected JSONStorableFloat frictionJSON;

	protected JSONStorableFloat gravityMultiplierJSON;

	protected JSONStorableFloat weightJSON;

	protected JSONStorableFloat iterationsJSON;

	protected RectTransform paintedRigidityIndicatorPanel;

	protected JSONStorableBool usePaintedRigidityJSON;

	protected JSONStorableFloat rootRigidityJSON;

	protected JSONStorableFloat mainRigidityJSON;

	protected JSONStorableFloat tipRigidityJSON;

	protected JSONStorableFloat rigidityRolloffPowerJSON;

	protected JSONStorableFloat jointRigidityJSON;

	protected JSONStorableFloat clingJSON;

	protected JSONStorableFloat clingRolloffJSON;

	protected JSONStorableFloat snapJSON;

	protected JSONStorableFloat bendResistanceJSON;

	protected JSONStorableVector3 windJSON;

	public Shader qualityShader;

	public Shader qualityThickenShader;

	public Shader qualityThickenMoreShader;

	public Shader fastShader;

	protected Shader nonStandardShader;

	protected ShaderType _currentShaderType = ShaderType.Quality;

	protected JSONStorableStringChooser shaderTypeJSON;

	protected Color _rootColor;

	protected HSVColor _rootHSVColor;

	protected JSONStorableColor rootColorJSON;

	protected Color _tipColor;

	protected HSVColor _tipHSVColor;

	protected JSONStorableColor tipColorJSON;

	protected JSONStorableFloat colorRolloffJSON;

	protected Color _specularColor;

	protected HSVColor _specularHSVColor;

	protected JSONStorableColor specularColorJSON;

	protected JSONStorableFloat diffuseSoftnessJSON;

	protected JSONStorableFloat primarySpecularSharpnessJSON;

	protected JSONStorableFloat secondarySpecularSharpnessJSON;

	protected JSONStorableFloat specularShiftJSON;

	protected JSONStorableFloat fresnelPowerJSON;

	protected JSONStorableFloat fresnelAttenuationJSON;

	protected JSONStorableFloat randomColorPowerJSON;

	protected JSONStorableFloat randomColorOffsetJSON;

	protected JSONStorableFloat IBLFactorJSON;

	protected JSONStorableFloat normalRandomizeJSON;

	protected JSONStorableFloat curlXJSON;

	protected JSONStorableFloat curlYJSON;

	protected JSONStorableFloat curlZJSON;

	protected JSONStorableFloat curlScaleJSON;

	protected JSONStorableFloat curlScaleRandomnessJSON;

	protected JSONStorableFloat curlFrequencyJSON;

	protected JSONStorableFloat curlFrequencyRandomnessJSON;

	protected JSONStorableBool curlAllowReverseJSON;

	protected JSONStorableBool curlAllowFlipAxisJSON;

	protected JSONStorableFloat curlNormalAdjustJSON;

	protected JSONStorableFloat curlRootJSON;

	protected JSONStorableFloat curlMidJSON;

	protected JSONStorableFloat curlTipJSON;

	protected JSONStorableFloat curlMidpointJSON;

	protected JSONStorableFloat curlCurvePowerJSON;

	protected JSONStorableFloat length1JSON;

	protected JSONStorableFloat length2JSON;

	protected JSONStorableFloat length3JSON;

	protected JSONStorableFloat widthJSON;

	protected JSONStorableFloat densityJSON;

	protected JSONStorableFloat detailJSON;

	protected JSONStorableFloat maxSpreadJSON;

	protected JSONStorableFloat spreadRootJSON;

	protected JSONStorableFloat spreadMidJSON;

	protected JSONStorableFloat spreadTipJSON;

	protected JSONStorableFloat spreadMidpointJSON;

	protected JSONStorableFloat spreadCurvePowerJSON;

	protected HairSimControlTools hairSimControlTools
	{
		get
		{
			if (_hairSimControlTools == null)
			{
				_hairSimControlTools = GetComponentInParent<HairSimControlTools>();
			}
			return _hairSimControlTools;
		}
	}

	protected void UpdateAllHairSettings(bool keepParticlePositions = false)
	{
		if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particles != null)
		{
			if (keepParticlePositions)
			{
				hairSettings.HairBuidCommand.particles.UpdateParticleRadius();
				hairSettings.HairBuidCommand.particles.SaveGPUState();
			}
			hairSettings.HairBuidCommand.particles.UpdateSettings();
		}
		if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.distanceJoints != null)
		{
			hairSettings.HairBuidCommand.distanceJoints.UpdateSettings();
		}
		if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.compressionJoints != null)
		{
			hairSettings.HairBuidCommand.compressionJoints.UpdateSettings();
		}
		if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particles != null && keepParticlePositions)
		{
			hairSettings.HairBuidCommand.particles.RestoreGPUState();
		}
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		if (widthJSON != null)
		{
			SyncWidth(widthJSON.val);
		}
		if (hairSettings != null)
		{
			if (hairSettings.PhysicsSettings != null)
			{
				hairSettings.PhysicsSettings.WorldScale = scale;
			}
			UpdateAllHairSettings();
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.physics != null)
			{
				hairSettings.HairBuidCommand.physics.PartialResetPhysics();
			}
		}
	}

	public void Rebuild()
	{
		if (hairSettings != null && hairSettings.HairBuidCommand != null)
		{
			hairSettings.HairBuidCommand.RebuildHair();
		}
	}

	public void SyncStyleText()
	{
		if (!(creator != null) || !(simNearbyJointCountText != null))
		{
			return;
		}
		List<Vector4ListContainer> nearbyVertexGroups = creator.GetNearbyVertexGroups();
		int num = 0;
		if (nearbyVertexGroups != null)
		{
			num = nearbyVertexGroups.Sum((Vector4ListContainer container) => container.List.Count);
		}
		simNearbyJointCountText.text = num.ToString();
		if (rebuildStyleJointsAction.button != null)
		{
			if (num == 0)
			{
				rebuildStyleJointsAction.button.image.color = Color.yellow;
			}
			else
			{
				rebuildStyleJointsAction.button.image.color = Color.gray;
			}
		}
	}

	protected void SyncStyleModeButtons()
	{
		if (styleModelPanel != null)
		{
			styleModelPanel.gameObject.SetActive(_styleMode);
		}
		if (resetAndStartStyleModeAction.button != null)
		{
			resetAndStartStyleModeAction.button.interactable = !_styleMode;
		}
		if (startStyleModeAction.button != null)
		{
			startStyleModeAction.button.interactable = !_styleMode;
		}
		if (cancelStyleModeAction.button != null)
		{
			cancelStyleModeAction.button.interactable = _styleMode;
		}
		if (keepStyleAction.button != null)
		{
			keepStyleAction.button.interactable = _styleMode;
		}
		if (rebuildStyleJointsAction.button != null)
		{
			rebuildStyleJointsAction.button.interactable = !_styleMode;
		}
	}

	public void StartStyleMode(bool reset = false)
	{
		if (!(hairSettings != null))
		{
			return;
		}
		_styleMode = true;
		SyncStyleModeButtons();
		SyncStyleToolVisibility();
		hairSettings.PhysicsSettings.StyleMode = true;
		if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.pointJoints != null)
		{
			hairSettings.HairBuidCommand.pointJoints.UpdateSettings();
		}
		if (reset)
		{
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particles != null)
			{
				hairSettings.HairBuidCommand.particles.UpdateSettings();
				hairSettings.HairBuidCommand.physics.ResetPhysics();
			}
		}
		else if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particles != null)
		{
			hairSettings.HairBuidCommand.particles.UpdateParticleRadius();
		}
		if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
		{
			hairSettings.HairBuidCommand.particlesData.UpdateSettings();
		}
		if (SuperController.singleton != null)
		{
			SuperController.singleton.SelectModeCustomWithTargetControl("Use tools to style hair. Save and cancel when done");
			SuperController.singleton.DisableRemoteHoldGrab();
		}
	}

	protected void StartStyleModeWithReset()
	{
		StartStyleMode(reset: true);
	}

	protected void StartStyleModeWithoutReset()
	{
		StartStyleMode();
	}

	public void CancelStyleMode()
	{
		if (hairSettings != null && _styleMode)
		{
			_styleMode = false;
			SyncStyleModeButtons();
			SyncStyleToolVisibility();
			hairSettings.PhysicsSettings.StyleMode = false;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.pointJoints != null)
			{
				hairSettings.HairBuidCommand.pointJoints.UpdateSettings();
			}
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particles != null)
			{
				hairSettings.HairBuidCommand.particles.UpdateParticleRadius();
			}
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.distanceJoints != null)
			{
				hairSettings.HairBuidCommand.distanceJoints.UpdateSettings();
			}
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.compressionJoints != null)
			{
				hairSettings.HairBuidCommand.compressionJoints.UpdateSettings();
			}
			if (SuperController.singleton != null)
			{
				SuperController.singleton.SelectModeOff();
				SuperController.singleton.EnableRemoteHoldGrab();
			}
		}
	}

	public void KeepStyle()
	{
		if (hairSettings != null)
		{
			_styleMode = false;
			SyncStyleModeButtons();
			SyncStyleToolVisibility();
			hairSettings.PhysicsSettings.StyleMode = false;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.pointJoints != null)
			{
				hairSettings.HairBuidCommand.pointJoints.RebuildFromGPUData();
			}
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
			UpdateAllHairSettings(keepParticlePositions: true);
			if (rebuildStyleJointsAction.button != null)
			{
				rebuildStyleJointsAction.button.image.color = Color.yellow;
			}
			if (SuperController.singleton != null)
			{
				SuperController.singleton.SelectModeOff();
				SuperController.singleton.EnableRemoteHoldGrab();
			}
		}
	}

	protected void SyncStyleModeAllowControlOtherNodes(bool b)
	{
		SyncStyleToolVisibility();
	}

	public void SyncStyleJointsSearchDistance(float f)
	{
		if (creator != null)
		{
			creator.NearbyVertexSearchDistance = f;
		}
	}

	public void ClearStyleJoints()
	{
		if (creator != null)
		{
			creator.ClearNearbyVertexGroups();
			if (hairSettings != null)
			{
				Rebuild();
			}
			SyncStyleText();
		}
	}

	protected void AbortRebuildStyleJointsThread(bool wait = true)
	{
		if (rebuildStyleJointsThread == null || !rebuildStyleJointsThread.IsAlive)
		{
			return;
		}
		creator.CancelCalculateNearbyVertexGroups();
		if (wait)
		{
			while (rebuildStyleJointsThread.IsAlive)
			{
				Thread.Sleep(0);
			}
		}
	}

	protected void RebuildStyleJointsThreaded()
	{
		creator.CalculateNearbyVertexGroups();
	}

	protected IEnumerator RebuildStyleJointsCo()
	{
		yield return null;
		threadError = null;
		creator.PrepareCalculateNearbyVertexGroups();
		rebuildStyleJointsThread = new Thread(RebuildStyleJointsThreaded);
		rebuildStyleJointsThread.Start();
		while (rebuildStyleJointsThread.IsAlive)
		{
			if (styleStatusText != null)
			{
				styleStatusText.text = creator.status;
			}
			yield return null;
		}
		if (styleStatusText != null)
		{
			styleStatusText.text = creator.status;
		}
		if (hairSettings != null)
		{
			Rebuild();
		}
		SyncStyleText();
		isRebuildingStyleJoints = false;
	}

	public void RebuildStyleJoints()
	{
		if (!isRebuildingStyleJoints)
		{
			isRebuildingStyleJoints = true;
			StartCoroutine(RebuildStyleJointsCo());
		}
	}

	public void SyncStyleModeGravityMultiplier(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.StyleModeGravityMultiplier = f;
		}
	}

	public void SyncStyleModeShowCurls(bool b)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.StyleModeShowCurls = b;
		}
	}

	public void SyncStyleModeUpHairPullStrength(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.ReverseSplineJointPower = f;
		}
	}

	public void SyncStyleModeCollisionRadius(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.StyleModeStrandRadius = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particles != null)
			{
				hairSettings.HairBuidCommand.particles.UpdateParticleRadius();
			}
		}
	}

	public void SyncStyleModeCollisionRadiusRoot(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.StyleModeStrandRootRadius = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particles != null)
			{
				hairSettings.HairBuidCommand.particles.UpdateParticleRadius();
			}
		}
	}

	protected void SyncStyleToolVisibility()
	{
		HairSimControlTools hairSimControlTools = this.hairSimControlTools;
		if (hairSimControlTools != null)
		{
			hairSimControlTools.SetHairStyleToolVisibility(_styleMode && styleModeShowTool1JSON.val, _styleMode && styleModeShowTool2JSON.val, _styleMode && styleModeShowTool3JSON.val, _styleMode && styleModeShowTool4JSON.val);
			hairSimControlTools.SetOnlyToolsControllable(_styleMode && !styleModeAllowControlOtherNodesJSON.val);
		}
	}

	protected void SyncShowStyleTool1(bool b)
	{
		SyncStyleToolVisibility();
	}

	protected void SyncShowStyleTool2(bool b)
	{
		SyncStyleToolVisibility();
	}

	protected void SyncShowStyleTool3(bool b)
	{
		SyncStyleToolVisibility();
	}

	protected void SyncShowStyleTool4(bool b)
	{
		SyncStyleToolVisibility();
	}

	public void SyncSimulationEnabled(bool b)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.IsEnabled = b;
		}
	}

	public void SyncCollisionEnabled(bool b)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.IsCollisionEnabled = b;
		}
	}

	public void SyncCollisionRadius(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.StandRadius = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particles != null)
			{
				hairSettings.HairBuidCommand.particles.UpdateParticleRadius();
			}
		}
	}

	public void SyncCollisionRadiusRoot(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.StandRootRadius = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particles != null)
			{
				hairSettings.HairBuidCommand.particles.UpdateParticleRadius();
			}
		}
	}

	public void SyncDrag(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.Drag = f;
		}
	}

	public void SyncFriction(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.Friction = f;
		}
	}

	public void SyncGravityMultiplier(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.GravityMultiplier = f;
		}
	}

	public void SyncWeight(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.Weight = f;
		}
	}

	public void SyncIterations(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.Iterations = Mathf.FloorToInt(f);
		}
	}

	public void SyncUsePaintedRigidity(bool b)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.UsePaintedRigidity = b;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.pointJoints != null)
			{
				hairSettings.HairBuidCommand.pointJoints.UpdateSettings();
			}
		}
		if (rootRigidityJSON.slider != null)
		{
			rootRigidityJSON.slider.gameObject.SetActive(!b);
		}
		if (rootRigidityJSON.sliderAlt != null)
		{
			rootRigidityJSON.sliderAlt.gameObject.SetActive(!b);
		}
		if (mainRigidityJSON.slider != null)
		{
			mainRigidityJSON.slider.gameObject.SetActive(!b);
		}
		if (mainRigidityJSON.sliderAlt != null)
		{
			mainRigidityJSON.sliderAlt.gameObject.SetActive(!b);
		}
		if (tipRigidityJSON.slider != null)
		{
			tipRigidityJSON.slider.gameObject.SetActive(!b);
		}
		if (tipRigidityJSON.sliderAlt != null)
		{
			tipRigidityJSON.sliderAlt.gameObject.SetActive(!b);
		}
		if (rigidityRolloffPowerJSON.slider != null)
		{
			rigidityRolloffPowerJSON.slider.gameObject.SetActive(!b);
		}
		if (rigidityRolloffPowerJSON.sliderAlt != null)
		{
			rigidityRolloffPowerJSON.sliderAlt.gameObject.SetActive(!b);
		}
		if (paintedRigidityIndicatorPanel != null)
		{
			paintedRigidityIndicatorPanel.gameObject.SetActive(b);
		}
		HairSimControlTools hairSimControlTools = this.hairSimControlTools;
		if (hairSimControlTools != null)
		{
			hairSimControlTools.SetAllowRigidityPaint(b);
		}
	}

	public void SyncRootRigidity(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.RootRigidity = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.pointJoints != null)
			{
				hairSettings.HairBuidCommand.pointJoints.UpdateSettings();
			}
		}
	}

	public void SyncMainRigidity(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.MainRigidity = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.pointJoints != null)
			{
				hairSettings.HairBuidCommand.pointJoints.UpdateSettings();
			}
		}
	}

	public void SyncTipRigidity(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.TipRigidity = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.pointJoints != null)
			{
				hairSettings.HairBuidCommand.pointJoints.UpdateSettings();
			}
		}
	}

	public void SyncRigidityRolloffPower(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.RigidityRolloffPower = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.pointJoints != null)
			{
				hairSettings.HairBuidCommand.pointJoints.UpdateSettings();
			}
		}
	}

	public void SyncJointRigidity(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.JointRigidity = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.pointJoints != null)
			{
				hairSettings.HairBuidCommand.pointJoints.UpdateSettings();
			}
		}
	}

	public void SyncCling(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.NearbyJointPower = f;
		}
	}

	public void SyncClingRolloff(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.NearbyJointPowerRolloff = f;
		}
	}

	public void SyncSnap(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.SplineJointPower = f;
		}
	}

	public void SyncBendResistance(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.PhysicsSettings.CompressionJointPower = f;
		}
	}

	protected void SyncWind()
	{
		if (windJSON != null)
		{
			SyncWind(windJSON.val);
		}
	}

	protected void SyncWind(Vector3 v)
	{
		if (hairSettings != null && hairSettings.RuntimeData != null)
		{
			hairSettings.RuntimeData.Wind = v + WindControl.globalWind;
		}
	}

	protected void SyncShader()
	{
		if (_currentShaderType == ShaderType.Quality)
		{
			if (qualityShader != null && hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.render != null)
			{
				hairSettings.HairBuidCommand.render.SetShader(qualityShader);
			}
		}
		else if (_currentShaderType == ShaderType.QualityThicken)
		{
			if (qualityThickenShader != null && hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.render != null)
			{
				hairSettings.HairBuidCommand.render.SetShader(qualityThickenShader);
			}
		}
		else if (_currentShaderType == ShaderType.QualityThickenMore)
		{
			if (qualityThickenMoreShader != null && hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.render != null)
			{
				hairSettings.HairBuidCommand.render.SetShader(qualityThickenMoreShader);
			}
		}
		else if (_currentShaderType == ShaderType.Fast)
		{
			if (fastShader != null && hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.render != null)
			{
				hairSettings.HairBuidCommand.render.SetShader(fastShader);
			}
		}
		else if (_currentShaderType == ShaderType.NonStandard && nonStandardShader != null && hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.render != null)
		{
			hairSettings.HairBuidCommand.render.SetShader(nonStandardShader);
		}
	}

	protected void SyncShaderType(string shaderTypeString)
	{
		try
		{
			ShaderType currentShaderType = (ShaderType)Enum.Parse(typeof(ShaderType), shaderTypeString);
			_currentShaderType = currentShaderType;
			SyncShader();
		}
		catch (ArgumentException)
		{
			Debug.LogError("Attempted to set shader type to " + shaderTypeString);
		}
	}

	protected void SyncRootColor(float h, float s, float v)
	{
		_rootHSVColor.H = h;
		_rootHSVColor.S = s;
		_rootHSVColor.V = v;
		_rootColor = HSVColorPicker.HSVToRGB(h, s, v);
		if (hairSettings != null && hairSettings.RenderSettings.RootTipColorProvider != null)
		{
			hairSettings.RenderSettings.RootTipColorProvider.RootColor = _rootColor;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	protected void SyncTipColor(float h, float s, float v)
	{
		_tipHSVColor.H = h;
		_tipHSVColor.S = s;
		_tipHSVColor.V = v;
		_tipColor = HSVColorPicker.HSVToRGB(h, s, v);
		if (hairSettings != null && hairSettings.RenderSettings.RootTipColorProvider != null)
		{
			hairSettings.RenderSettings.RootTipColorProvider.TipColor = _tipColor;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncColorRolloff(float f)
	{
		if (hairSettings != null && hairSettings.RenderSettings.RootTipColorProvider != null)
		{
			hairSettings.RenderSettings.RootTipColorProvider.ColorRolloff = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	protected void SyncSpecularColor(float h, float s, float v)
	{
		_specularHSVColor.H = h;
		_specularHSVColor.S = s;
		_specularHSVColor.V = v;
		_specularColor = HSVColorPicker.HSVToRGB(h, s, v);
		hairSettings.RenderSettings.SpecularColor = _specularColor;
	}

	public void SyncDiffuseSoftness(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.Diffuse = f;
		}
	}

	public void SyncPrimarySpecularSharpness(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.PrimarySpecular = f;
		}
	}

	public void SyncSecondarySpecularSharpness(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.SecondarySpecular = f;
		}
	}

	public void SyncSpecularShift(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.SpecularShift = f;
		}
	}

	public void SyncFresnelPower(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.FresnelPower = f;
		}
	}

	public void SyncFresnelAttenuation(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.FresnelAttenuation = f;
		}
	}

	public void SyncRandomColorPower(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.RandomTexColorPower = f;
		}
	}

	public void SyncRandomColorOffset(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.RandomTexColorOffset = f;
		}
	}

	public void SyncIBLFactor(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.IBLFactor = f;
		}
	}

	public void SyncNormalRandomize(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.NormalRandomize = f;
		}
	}

	public void SyncCurlX(float f)
	{
		if (hairSettings != null)
		{
			Vector3 wavinessAxis = hairSettings.RenderSettings.WavinessAxis;
			wavinessAxis.x = f;
			hairSettings.RenderSettings.WavinessAxis = wavinessAxis;
		}
	}

	public void SyncCurlY(float f)
	{
		if (hairSettings != null)
		{
			Vector3 wavinessAxis = hairSettings.RenderSettings.WavinessAxis;
			wavinessAxis.y = f;
			hairSettings.RenderSettings.WavinessAxis = wavinessAxis;
		}
	}

	public void SyncCurlZ(float f)
	{
		if (hairSettings != null)
		{
			Vector3 wavinessAxis = hairSettings.RenderSettings.WavinessAxis;
			wavinessAxis.z = f;
			hairSettings.RenderSettings.WavinessAxis = wavinessAxis;
		}
	}

	public void SyncCurlScale(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessScale = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncCurlScaleRandomness(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessScaleRandomness = f;
		}
	}

	public void SyncCurlFrequency(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessFrequency = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncCurlFrequencyRandomness(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessFrequencyRandomness = f;
		}
	}

	public void SyncCurlAllowReverse(bool b)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessAllowReverse = b;
		}
	}

	public void SyncCurlAllowFlipAxis(bool b)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessAllowFlipAxis = b;
		}
	}

	public void SyncCurlNormalAdjust(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessNormalAdjust = f;
		}
	}

	public void SyncCurlRoot(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessRoot = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncCurlMid(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessMid = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncCurlTip(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessTip = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncCurlMidpoint(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessMidpoint = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncCurlCurvePower(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.WavinessCurvePower = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncLength1(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.Length1 = f;
		}
	}

	public void SyncLength2(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.Length2 = f;
		}
	}

	public void SyncLength3(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.Length3 = f;
		}
	}

	public void SyncWidth(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.LODSettings.FixedWidth = f * _scale;
		}
	}

	public void SyncDensity(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.LODSettings.FixedDensity = Mathf.FloorToInt(f);
		}
	}

	public void SyncDetail(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.LODSettings.FixedDetail = Mathf.FloorToInt(f);
		}
	}

	public void SyncMaxSpread(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.MaxSpread = f;
		}
	}

	public void SyncSpreadRoot(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.InterpolationRoot = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncSpreadMid(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.InterpolationMid = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncSpreadTip(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.InterpolationTip = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncSpreadMidpoint(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.InterpolationMidpoint = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	public void SyncSpreadCurvePower(float f)
	{
		if (hairSettings != null)
		{
			hairSettings.RenderSettings.InterpolationCurvePower = f;
			if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.particlesData != null)
			{
				hairSettings.HairBuidCommand.particlesData.UpdateSettings();
			}
		}
	}

	protected void Init()
	{
		if (hairSettings == null)
		{
			hairSettings = GetComponent<HairSettings>();
		}
		if (!(hairSettings != null))
		{
			return;
		}
		if (creator != null)
		{
			resetAndStartStyleModeAction = new JSONStorableAction("ResetAndStartStyleMode", StartStyleModeWithReset);
			RegisterAction(resetAndStartStyleModeAction);
			startStyleModeAction = new JSONStorableAction("StartStyleMode", StartStyleModeWithoutReset);
			RegisterAction(startStyleModeAction);
			cancelStyleModeAction = new JSONStorableAction("CancelStyleMode", CancelStyleMode);
			RegisterAction(cancelStyleModeAction);
			keepStyleAction = new JSONStorableAction("KeepStyle", KeepStyle);
			RegisterAction(keepStyleAction);
			rebuildStyleJointsAction = new JSONStorableAction("RebuildStyleJoints", RebuildStyleJoints);
			RegisterAction(rebuildStyleJointsAction);
			styleModeAllowControlOtherNodesJSON = new JSONStorableBool("styleModeAllowControlOtherNodes", startingValue: false, SyncStyleModeAllowControlOtherNodes);
			RegisterBool(styleModeAllowControlOtherNodesJSON);
			styleJointsSearchDistanceJSON = new JSONStorableFloat("styleJointsSearchDistance", creator.NearbyVertexSearchDistance, SyncStyleJointsSearchDistance, 0.0001f, 0.01f);
			RegisterFloat(styleJointsSearchDistanceJSON);
			clearStyleJointsAction = new JSONStorableAction("ClearStyleJoints", ClearStyleJoints);
			RegisterAction(clearStyleJointsAction);
			styleModeCollisionRadiusJSON = new JSONStorableFloat("styleModeCollisionRadius", hairSettings.PhysicsSettings.StyleModeStrandRadius, SyncStyleModeCollisionRadius, 0.001f, 0.1f);
			RegisterFloat(styleModeCollisionRadiusJSON);
			if (hairSettings.PhysicsSettings.UseSeparateRootRadius)
			{
				styleModeCollisionRadiusRootJSON = new JSONStorableFloat("styleModeCollisionRadiusRoot", hairSettings.PhysicsSettings.StyleModeStrandRootRadius, SyncStyleModeCollisionRadiusRoot, 0.001f, 0.1f);
				RegisterFloat(styleModeCollisionRadiusRootJSON);
			}
			styleModeGravityMultiplierJSON = new JSONStorableFloat("styleModeGravityMultiplier", hairSettings.PhysicsSettings.StyleModeGravityMultiplier, SyncStyleModeGravityMultiplier, -2f, 2f);
			RegisterFloat(styleModeGravityMultiplierJSON);
			styleModeShowCurlsJSON = new JSONStorableBool("styleModeShowCurls", hairSettings.RenderSettings.StyleModeShowCurls, SyncStyleModeShowCurls);
			RegisterBool(styleModeShowCurlsJSON);
			styleModeUpHairPullStrengthJSON = new JSONStorableFloat("styleModeUpHairPullStrength", hairSettings.PhysicsSettings.ReverseSplineJointPower, SyncStyleModeUpHairPullStrength, 0.1f, 1f);
			RegisterFloat(styleModeUpHairPullStrengthJSON);
			styleModeShowTool1JSON = new JSONStorableBool("styleModeShowTool1", startingValue: true, SyncShowStyleTool1);
			RegisterBool(styleModeShowTool1JSON);
			styleModeShowTool2JSON = new JSONStorableBool("styleModeShowTool2", startingValue: true, SyncShowStyleTool2);
			RegisterBool(styleModeShowTool2JSON);
			styleModeShowTool3JSON = new JSONStorableBool("styleModeShowTool3", startingValue: false, SyncShowStyleTool3);
			RegisterBool(styleModeShowTool3JSON);
			styleModeShowTool4JSON = new JSONStorableBool("styleModeShowTool4", startingValue: false, SyncShowStyleTool4);
			RegisterBool(styleModeShowTool4JSON);
		}
		simulationEnabledJSON = new JSONStorableBool("simulationEnabled", hairSettings.PhysicsSettings.IsEnabled, SyncSimulationEnabled);
		RegisterBool(simulationEnabledJSON);
		collisionEnabledJSON = new JSONStorableBool("collisionEnabled", hairSettings.PhysicsSettings.IsCollisionEnabled, SyncCollisionEnabled);
		RegisterBool(collisionEnabledJSON);
		collisionRadiusJSON = new JSONStorableFloat("collisionRadius", hairSettings.PhysicsSettings.StandRadius, SyncCollisionRadius, 0.001f, 0.1f);
		RegisterFloat(collisionRadiusJSON);
		if (hairSettings.PhysicsSettings.UseSeparateRootRadius)
		{
			collisionRadiusRootJSON = new JSONStorableFloat("collisionRadiusRoot", hairSettings.PhysicsSettings.StandRootRadius, SyncCollisionRadiusRoot, 0.001f, 0.1f);
			RegisterFloat(collisionRadiusRootJSON);
		}
		dragJSON = new JSONStorableFloat("drag", hairSettings.PhysicsSettings.Drag, SyncDrag, 0f, 1f);
		RegisterFloat(dragJSON);
		usePaintedRigidityJSON = new JSONStorableBool("usePaintedRigidity", hairSettings.PhysicsSettings.UsePaintedRigidity, SyncUsePaintedRigidity);
		RegisterBool(usePaintedRigidityJSON);
		rootRigidityJSON = new JSONStorableFloat("rootRigidity", hairSettings.PhysicsSettings.RootRigidity, SyncRootRigidity, 0f, 1f);
		RegisterFloat(rootRigidityJSON);
		mainRigidityJSON = new JSONStorableFloat("mainRigidity", hairSettings.PhysicsSettings.MainRigidity, SyncMainRigidity, 0f, 1f);
		RegisterFloat(mainRigidityJSON);
		tipRigidityJSON = new JSONStorableFloat("tipRigidity", hairSettings.PhysicsSettings.TipRigidity, SyncTipRigidity, 0f, 1f);
		RegisterFloat(tipRigidityJSON);
		rigidityRolloffPowerJSON = new JSONStorableFloat("rigidityRolloffPower", hairSettings.PhysicsSettings.RigidityRolloffPower, SyncRigidityRolloffPower, 0f, 16f);
		RegisterFloat(rigidityRolloffPowerJSON);
		if (hairSettings.PhysicsSettings.JointAreas != null && hairSettings.PhysicsSettings.JointAreas.Count > 0)
		{
			jointRigidityJSON = new JSONStorableFloat("jointRigidity", hairSettings.PhysicsSettings.JointRigidity, SyncJointRigidity, 0f, 1f);
			RegisterFloat(jointRigidityJSON);
		}
		frictionJSON = new JSONStorableFloat("friction", hairSettings.PhysicsSettings.Friction, SyncFriction, 0f, 1f);
		RegisterFloat(frictionJSON);
		gravityMultiplierJSON = new JSONStorableFloat("gravityMultiplier", hairSettings.PhysicsSettings.GravityMultiplier, SyncGravityMultiplier, -2f, 2f);
		RegisterFloat(gravityMultiplierJSON);
		weightJSON = new JSONStorableFloat("weight", hairSettings.PhysicsSettings.Weight, SyncWeight, 0f, 2f);
		RegisterFloat(weightJSON);
		iterationsJSON = new JSONStorableFloat("iterations", hairSettings.PhysicsSettings.Iterations, SyncIterations, 1f, 5f);
		RegisterFloat(iterationsJSON);
		clingJSON = new JSONStorableFloat("cling", hairSettings.PhysicsSettings.NearbyJointPower, SyncCling, 0f, 1f);
		RegisterFloat(clingJSON);
		clingRolloffJSON = new JSONStorableFloat("clingRolloff", hairSettings.PhysicsSettings.NearbyJointPowerRolloff, SyncClingRolloff, 0f, 1f);
		RegisterFloat(clingRolloffJSON);
		snapJSON = new JSONStorableFloat("snap", hairSettings.PhysicsSettings.SplineJointPower, SyncSnap, 0f, 1f);
		RegisterFloat(snapJSON);
		bendResistanceJSON = new JSONStorableFloat("bendResistance", hairSettings.PhysicsSettings.CompressionJointPower, SyncBendResistance, 0f, 1f);
		RegisterFloat(bendResistanceJSON);
		windJSON = new JSONStorableVector3("wind", Vector3.zero, new Vector3(-50f, -50f, -50f), new Vector3(50f, 50f, 50f), constrain: false);
		RegisterVector3(windJSON);
		Shader shader = null;
		if (hairSettings.HairBuidCommand != null && hairSettings.HairBuidCommand.render != null)
		{
			shader = hairSettings.HairBuidCommand.render.GetShader();
		}
		else if (hairSettings.RenderSettings.material != null)
		{
			shader = hairSettings.RenderSettings.material.shader;
		}
		if (shader != null)
		{
			if (qualityShader != null && shader == qualityShader)
			{
				_currentShaderType = ShaderType.Quality;
			}
			else if (qualityThickenShader != null && shader == qualityThickenShader)
			{
				_currentShaderType = ShaderType.QualityThicken;
			}
			else if (qualityThickenMoreShader != null && shader == qualityThickenMoreShader)
			{
				_currentShaderType = ShaderType.QualityThickenMore;
			}
			else if (fastShader != null && shader == fastShader)
			{
				_currentShaderType = ShaderType.Fast;
			}
			else
			{
				_currentShaderType = ShaderType.NonStandard;
				nonStandardShader = shader;
			}
		}
		List<string> list = new List<string>();
		if (fastShader != null)
		{
			list.Add("Fast");
		}
		if (qualityShader != null)
		{
			list.Add("Quality");
		}
		if (qualityThickenShader != null)
		{
			list.Add("QualityThicken");
		}
		if (qualityThickenMoreShader != null)
		{
			list.Add("QualityThickenMore");
		}
		if (nonStandardShader != null)
		{
			list.Add("NonStandard");
		}
		shaderTypeJSON = new JSONStorableStringChooser("shaderType", list, _currentShaderType.ToString(), "Shader Type", SyncShaderType);
		RegisterStringChooser(shaderTypeJSON);
		if (hairSettings.RenderSettings.RootTipColorProvider != null)
		{
			_rootColor = hairSettings.RenderSettings.RootTipColorProvider.RootColor;
			_tipColor = hairSettings.RenderSettings.RootTipColorProvider.TipColor;
		}
		_specularColor = hairSettings.RenderSettings.SpecularColor;
		_rootHSVColor = HSVColorPicker.RGBToHSV(_rootColor.r, _rootColor.g, _rootColor.b);
		_tipHSVColor = HSVColorPicker.RGBToHSV(_tipColor.r, _tipColor.g, _tipColor.b);
		_specularHSVColor = HSVColorPicker.RGBToHSV(_specularColor.r, _specularColor.g, _specularColor.b);
		rootColorJSON = new JSONStorableColor("rootColor", _rootHSVColor, SyncRootColor);
		RegisterColor(rootColorJSON);
		tipColorJSON = new JSONStorableColor("tipColor", _tipHSVColor, SyncTipColor);
		RegisterColor(tipColorJSON);
		colorRolloffJSON = new JSONStorableFloat("colorRolloff", hairSettings.RenderSettings.RootTipColorProvider.ColorRolloff, SyncColorRolloff, 0f, 5f);
		RegisterFloat(colorRolloffJSON);
		specularColorJSON = new JSONStorableColor("specularColor", _specularHSVColor, SyncSpecularColor);
		RegisterColor(specularColorJSON);
		diffuseSoftnessJSON = new JSONStorableFloat("diffuseSoftness", hairSettings.RenderSettings.Diffuse, SyncDiffuseSoftness, 0f, 1f);
		RegisterFloat(diffuseSoftnessJSON);
		primarySpecularSharpnessJSON = new JSONStorableFloat("primarySpecularSharpness", hairSettings.RenderSettings.PrimarySpecular, SyncPrimarySpecularSharpness, 2f, 256f);
		RegisterFloat(primarySpecularSharpnessJSON);
		secondarySpecularSharpnessJSON = new JSONStorableFloat("secondarySpecularSharpness", hairSettings.RenderSettings.SecondarySpecular, SyncSecondarySpecularSharpness, 2f, 256f);
		RegisterFloat(secondarySpecularSharpnessJSON);
		specularShiftJSON = new JSONStorableFloat("specularShift", hairSettings.RenderSettings.SpecularShift, SyncSpecularShift, 0f, 1f);
		RegisterFloat(specularShiftJSON);
		fresnelPowerJSON = new JSONStorableFloat("fresnelPower", hairSettings.RenderSettings.FresnelPower, SyncFresnelPower, 0f, 10f);
		RegisterFloat(fresnelPowerJSON);
		fresnelAttenuationJSON = new JSONStorableFloat("fresnelAttenuation", hairSettings.RenderSettings.FresnelAttenuation, SyncFresnelAttenuation, 0f, 1f);
		RegisterFloat(fresnelAttenuationJSON);
		randomColorPowerJSON = new JSONStorableFloat("randomColorPower", hairSettings.RenderSettings.RandomTexColorPower, SyncRandomColorPower, 0f, 10f);
		RegisterFloat(randomColorPowerJSON);
		randomColorOffsetJSON = new JSONStorableFloat("randomColorOffset", hairSettings.RenderSettings.RandomTexColorOffset, SyncRandomColorOffset, 0f, 1f);
		RegisterFloat(randomColorOffsetJSON);
		IBLFactorJSON = new JSONStorableFloat("IBLFactor", hairSettings.RenderSettings.IBLFactor, SyncIBLFactor, 0f, 1f);
		RegisterFloat(IBLFactorJSON);
		normalRandomizeJSON = new JSONStorableFloat("normalRandomize", hairSettings.RenderSettings.NormalRandomize, SyncNormalRandomize, 0f, 1f);
		RegisterFloat(normalRandomizeJSON);
		curlXJSON = new JSONStorableFloat("curlX", hairSettings.RenderSettings.WavinessAxis.x, SyncCurlX, 0f, 1f);
		RegisterFloat(curlXJSON);
		curlYJSON = new JSONStorableFloat("curlY", hairSettings.RenderSettings.WavinessAxis.y, SyncCurlY, 0f, 1f);
		RegisterFloat(curlYJSON);
		curlZJSON = new JSONStorableFloat("curlZ", hairSettings.RenderSettings.WavinessAxis.z, SyncCurlZ, 0f, 1f);
		RegisterFloat(curlZJSON);
		curlScaleJSON = new JSONStorableFloat("curlScale", hairSettings.RenderSettings.WavinessScale, SyncCurlScale, 0f, 1f);
		RegisterFloat(curlScaleJSON);
		curlScaleRandomnessJSON = new JSONStorableFloat("curlScaleRandomness", hairSettings.RenderSettings.WavinessScaleRandomness, SyncCurlScaleRandomness, 0f, 2f);
		RegisterFloat(curlScaleRandomnessJSON);
		curlFrequencyJSON = new JSONStorableFloat("curlFrequency", hairSettings.RenderSettings.WavinessFrequency, SyncCurlFrequency, 0f, 20f);
		RegisterFloat(curlFrequencyJSON);
		curlFrequencyRandomnessJSON = new JSONStorableFloat("curlFrequencyRandomness", hairSettings.RenderSettings.WavinessFrequencyRandomness, SyncCurlFrequencyRandomness, 0f, 2f);
		RegisterFloat(curlFrequencyRandomnessJSON);
		curlAllowReverseJSON = new JSONStorableBool("curlAllowReverse", hairSettings.RenderSettings.WavinessAllowReverse, SyncCurlAllowReverse);
		RegisterBool(curlAllowReverseJSON);
		curlAllowFlipAxisJSON = new JSONStorableBool("curlAllowFlipAxis", hairSettings.RenderSettings.WavinessAllowFlipAxis, SyncCurlAllowFlipAxis);
		RegisterBool(curlAllowFlipAxisJSON);
		curlNormalAdjustJSON = new JSONStorableFloat("curlNormalAdjust", hairSettings.RenderSettings.WavinessNormalAdjust, SyncCurlNormalAdjust, -0.5f, 0.5f);
		RegisterFloat(curlNormalAdjustJSON);
		if (!hairSettings.RenderSettings.UseWavinessCurves)
		{
			curlRootJSON = new JSONStorableFloat("curlRoot", hairSettings.RenderSettings.WavinessRoot, SyncCurlRoot, 0f, 1f);
			RegisterFloat(curlRootJSON);
			curlMidJSON = new JSONStorableFloat("curlMid", hairSettings.RenderSettings.WavinessMid, SyncCurlMid, 0f, 1f);
			RegisterFloat(curlMidJSON);
			curlTipJSON = new JSONStorableFloat("curlTip", hairSettings.RenderSettings.WavinessTip, SyncCurlTip, 0f, 1f);
			RegisterFloat(curlTipJSON);
			curlMidpointJSON = new JSONStorableFloat("curlMidpoint", hairSettings.RenderSettings.WavinessMidpoint, SyncCurlMidpoint, 0f, 1f);
			RegisterFloat(curlMidpointJSON);
			curlCurvePowerJSON = new JSONStorableFloat("curlCurvePower", hairSettings.RenderSettings.WavinessCurvePower, SyncCurlCurvePower, 0f, 16f);
			RegisterFloat(curlCurvePowerJSON);
		}
		length1JSON = new JSONStorableFloat("length1", hairSettings.RenderSettings.Length1, SyncLength1, 0f, 1f);
		RegisterFloat(length1JSON);
		length2JSON = new JSONStorableFloat("length2", hairSettings.RenderSettings.Length2, SyncLength2, 0f, 1f);
		RegisterFloat(length2JSON);
		length3JSON = new JSONStorableFloat("length3", hairSettings.RenderSettings.Length3, SyncLength3, 0f, 1f);
		RegisterFloat(length3JSON);
		widthJSON = new JSONStorableFloat("width", hairSettings.LODSettings.FixedWidth, SyncWidth, 0f, 0.001f);
		RegisterFloat(widthJSON);
		SyncWidth(widthJSON.val);
		densityJSON = new JSONStorableFloat("curveDensity", hairSettings.LODSettings.FixedDensity, SyncDensity, 2f, 64f);
		RegisterFloat(densityJSON);
		detailJSON = new JSONStorableFloat("hairMultiplier", hairSettings.LODSettings.FixedDetail, SyncDetail, 1f, 64f);
		RegisterFloat(detailJSON);
		maxSpreadJSON = new JSONStorableFloat("maxSpread", hairSettings.RenderSettings.MaxSpread, SyncMaxSpread, 0f, 0.5f);
		RegisterFloat(maxSpreadJSON);
		if (!hairSettings.RenderSettings.UseInterpolationCurves)
		{
			spreadRootJSON = new JSONStorableFloat("spreadRoot", hairSettings.RenderSettings.InterpolationRoot, SyncSpreadRoot, 0f, 1f);
			RegisterFloat(spreadRootJSON);
			spreadMidJSON = new JSONStorableFloat("spreadMid", hairSettings.RenderSettings.InterpolationMid, SyncSpreadMid, 0f, 1f);
			RegisterFloat(spreadMidJSON);
			spreadTipJSON = new JSONStorableFloat("spreadTip", hairSettings.RenderSettings.InterpolationTip, SyncSpreadTip, 0f, 1f);
			RegisterFloat(spreadTipJSON);
			spreadMidpointJSON = new JSONStorableFloat("spreadMidpoint", hairSettings.RenderSettings.InterpolationMidpoint, SyncSpreadMidpoint, 0f, 1f);
			RegisterFloat(spreadMidpointJSON);
			spreadCurvePowerJSON = new JSONStorableFloat("spreadCurvePower", hairSettings.RenderSettings.InterpolationCurvePower, SyncSpreadCurvePower, 0f, 16f);
			RegisterFloat(spreadCurvePowerJSON);
		}
	}

	public override void InitUI()
	{
		if (!(UITransform != null))
		{
			return;
		}
		HairSimControlUI componentInChildren = UITransform.GetComponentInChildren<HairSimControlUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		restoreAllFromDefaultsAction.button = componentInChildren.restoreAllFromDefaultsButton;
		saveToStore1Action.button = componentInChildren.saveToStore1Button;
		restoreAllFromStore1Action.button = componentInChildren.restoreFromStore1Button;
		if (creator != null)
		{
			styleStatusText = componentInChildren.styleStatusText;
			styleModelPanel = componentInChildren.styleModePanel;
			simNearbyJointCountText = componentInChildren.simNearbyJointCountText;
			resetAndStartStyleModeAction.button = componentInChildren.resetAndStartStyleModeButton;
			startStyleModeAction.button = componentInChildren.startStyleModeButton;
			cancelStyleModeAction.button = componentInChildren.cancelStyleModeButton;
			keepStyleAction.button = componentInChildren.keepStyleButton;
			styleModeAllowControlOtherNodesJSON.toggle = componentInChildren.styleModelAllowControlOtherNodesToggle;
			styleJointsSearchDistanceJSON.slider = componentInChildren.styleJointsSearchDistanceSlider;
			rebuildStyleJointsAction.button = componentInChildren.rebuildStyleJointsButton;
			clearStyleJointsAction.button = componentInChildren.clearStyleJointsButton;
			styleModeCollisionRadiusJSON.slider = componentInChildren.styleModeCollisionRadiusSlider;
			if (styleModeCollisionRadiusRootJSON != null)
			{
				styleModeCollisionRadiusRootJSON.slider = componentInChildren.styleModeCollisionRadiusRootSlider;
			}
			styleModeGravityMultiplierJSON.slider = componentInChildren.styleModeGravityMultiplierSlider;
			styleModeShowCurlsJSON.toggle = componentInChildren.styleModeShowCurlsToggle;
			styleModeUpHairPullStrengthJSON.slider = componentInChildren.styleModeUpHairPullStrengthSlider;
			styleModeShowTool1JSON.toggle = componentInChildren.styleModeShowTool1Toggle;
			styleModeShowTool2JSON.toggle = componentInChildren.styleModeShowTool2Toggle;
			styleModeShowTool3JSON.toggle = componentInChildren.styleModeShowTool3Toggle;
			styleModeShowTool4JSON.toggle = componentInChildren.styleModeShowTool4Toggle;
			SyncStyleModeButtons();
		}
		simulationEnabledJSON.toggle = componentInChildren.simulationEnabledToggle;
		collisionEnabledJSON.toggle = componentInChildren.collisionEnabledToggle;
		collisionRadiusJSON.slider = componentInChildren.collisionRadiusSlider;
		if (collisionRadiusRootJSON != null)
		{
			collisionRadiusRootJSON.slider = componentInChildren.collisionRadiusRootSlider;
			if (componentInChildren.collisionRadiusRootSlider != null)
			{
				componentInChildren.collisionRadiusRootSlider.transform.parent.gameObject.SetActive(value: true);
			}
		}
		else if (componentInChildren.collisionRadiusRootSlider != null)
		{
			componentInChildren.collisionRadiusRootSlider.transform.parent.gameObject.SetActive(value: false);
		}
		dragJSON.slider = componentInChildren.dragSlider;
		usePaintedRigidityJSON.toggle = componentInChildren.usePaintedRigidityToggle;
		rootRigidityJSON.slider = componentInChildren.rootRigiditySlider;
		mainRigidityJSON.slider = componentInChildren.mainRigiditySlider;
		tipRigidityJSON.slider = componentInChildren.tipRigiditySlider;
		rigidityRolloffPowerJSON.slider = componentInChildren.rigidityRolloffPowerSlider;
		paintedRigidityIndicatorPanel = componentInChildren.paintedRigidityIndicatorPanel;
		SyncUsePaintedRigidity(usePaintedRigidityJSON.val);
		if (jointRigidityJSON != null)
		{
			if (componentInChildren.jointRigiditySlider != null)
			{
				jointRigidityJSON.slider = componentInChildren.jointRigiditySlider;
				componentInChildren.jointRigiditySlider.transform.parent.gameObject.SetActive(value: true);
			}
		}
		else if (componentInChildren.jointRigiditySlider != null)
		{
			componentInChildren.jointRigiditySlider.transform.parent.gameObject.SetActive(value: false);
		}
		frictionJSON.slider = componentInChildren.frictionSlider;
		gravityMultiplierJSON.slider = componentInChildren.gravityMultiplierSlider;
		weightJSON.slider = componentInChildren.weightSlider;
		iterationsJSON.slider = componentInChildren.iterationsSlider;
		clingJSON.slider = componentInChildren.clingSlider;
		clingRolloffJSON.slider = componentInChildren.clingRolloffSlider;
		snapJSON.slider = componentInChildren.snapSlider;
		bendResistanceJSON.slider = componentInChildren.bendResistanceSlider;
		windJSON.sliderX = componentInChildren.windXSlider;
		windJSON.sliderY = componentInChildren.windYSlider;
		windJSON.sliderZ = componentInChildren.windZSlider;
		shaderTypeJSON.popup = componentInChildren.shaderTypePopup;
		rootColorJSON.colorPicker = componentInChildren.rootColorPicker;
		tipColorJSON.colorPicker = componentInChildren.tipColorPicker;
		colorRolloffJSON.slider = componentInChildren.colorRolloffSlider;
		specularColorJSON.colorPicker = componentInChildren.specularColorPicker;
		diffuseSoftnessJSON.slider = componentInChildren.diffuseSoftnessSlider;
		primarySpecularSharpnessJSON.slider = componentInChildren.primarySpecularSharpnessSlider;
		secondarySpecularSharpnessJSON.slider = componentInChildren.secondarySpecularSharpnessSlider;
		specularShiftJSON.slider = componentInChildren.specularShiftSlider;
		fresnelPowerJSON.slider = componentInChildren.fresnelPowerSlider;
		fresnelAttenuationJSON.slider = componentInChildren.fresnelAttenuationSlider;
		randomColorPowerJSON.slider = componentInChildren.randomColorPowerSlider;
		randomColorOffsetJSON.slider = componentInChildren.randomColorOffsetSlider;
		IBLFactorJSON.slider = componentInChildren.IBLFactorSlider;
		normalRandomizeJSON.slider = componentInChildren.normalRandomizeSlider;
		curlXJSON.slider = componentInChildren.curlXSlider;
		curlYJSON.slider = componentInChildren.curlYSlider;
		curlZJSON.slider = componentInChildren.curlZSlider;
		curlScaleJSON.slider = componentInChildren.curlScaleSlider;
		curlFrequencyJSON.slider = componentInChildren.curlFrequencySlider;
		curlScaleRandomnessJSON.slider = componentInChildren.curlScaleRandomnessSlider;
		curlFrequencyRandomnessJSON.slider = componentInChildren.curlFrequencyRandomnessSlider;
		curlAllowReverseJSON.toggle = componentInChildren.curlAllowReverseToggle;
		curlAllowFlipAxisJSON.toggle = componentInChildren.curlAllowFlipAxisToggle;
		curlNormalAdjustJSON.slider = componentInChildren.curlNormalAdjustSlider;
		if (curlRootJSON != null)
		{
			curlRootJSON.slider = componentInChildren.curlRootSlider;
		}
		if (curlMidJSON != null)
		{
			curlMidJSON.slider = componentInChildren.curlMidSlider;
		}
		if (curlTipJSON != null)
		{
			curlTipJSON.slider = componentInChildren.curlTipSlider;
		}
		if (curlMidpointJSON != null)
		{
			curlMidpointJSON.slider = componentInChildren.curlMidpointSlider;
		}
		if (curlCurvePowerJSON != null)
		{
			curlCurvePowerJSON.slider = componentInChildren.curlCurvePowerSlider;
		}
		length1JSON.slider = componentInChildren.length1Slider;
		length2JSON.slider = componentInChildren.length2Slider;
		length3JSON.slider = componentInChildren.length3Slider;
		widthJSON.slider = componentInChildren.widthSlider;
		densityJSON.slider = componentInChildren.densitySlider;
		detailJSON.slider = componentInChildren.detailSlider;
		maxSpreadJSON.slider = componentInChildren.maxSpreadSlider;
		if (spreadRootJSON != null)
		{
			spreadRootJSON.slider = componentInChildren.spreadRootSlider;
		}
		if (spreadMidJSON != null)
		{
			spreadMidJSON.slider = componentInChildren.spreadMidSlider;
		}
		if (spreadTipJSON != null)
		{
			spreadTipJSON.slider = componentInChildren.spreadTipSlider;
		}
		if (spreadMidpointJSON != null)
		{
			spreadMidpointJSON.slider = componentInChildren.spreadMidpointSlider;
		}
		if (spreadCurvePowerJSON != null)
		{
			spreadCurvePowerJSON.slider = componentInChildren.spreadCurvePowerSlider;
		}
	}

	public override void InitUIAlt()
	{
		if (!(UITransformAlt != null))
		{
			return;
		}
		HairSimControlUI componentInChildren = UITransformAlt.GetComponentInChildren<HairSimControlUI>();
		if (!(componentInChildren != null))
		{
			return;
		}
		simulationEnabledJSON.toggleAlt = componentInChildren.simulationEnabledToggle;
		collisionEnabledJSON.toggleAlt = componentInChildren.collisionEnabledToggle;
		collisionRadiusJSON.sliderAlt = componentInChildren.collisionRadiusSlider;
		dragJSON.sliderAlt = componentInChildren.dragSlider;
		usePaintedRigidityJSON.toggleAlt = componentInChildren.usePaintedRigidityToggle;
		rootRigidityJSON.sliderAlt = componentInChildren.rootRigiditySlider;
		mainRigidityJSON.sliderAlt = componentInChildren.mainRigiditySlider;
		tipRigidityJSON.sliderAlt = componentInChildren.tipRigiditySlider;
		rigidityRolloffPowerJSON.sliderAlt = componentInChildren.rigidityRolloffPowerSlider;
		SyncUsePaintedRigidity(usePaintedRigidityJSON.val);
		if (jointRigidityJSON != null)
		{
			if (componentInChildren.jointRigiditySlider != null)
			{
				jointRigidityJSON.sliderAlt = componentInChildren.jointRigiditySlider;
				componentInChildren.jointRigiditySlider.transform.parent.gameObject.SetActive(value: true);
			}
		}
		else if (componentInChildren.jointRigiditySlider != null)
		{
			componentInChildren.jointRigiditySlider.transform.parent.gameObject.SetActive(value: false);
		}
		frictionJSON.sliderAlt = componentInChildren.frictionSlider;
		gravityMultiplierJSON.sliderAlt = componentInChildren.gravityMultiplierSlider;
		weightJSON.sliderAlt = componentInChildren.weightSlider;
		iterationsJSON.sliderAlt = componentInChildren.iterationsSlider;
		clingJSON.sliderAlt = componentInChildren.clingSlider;
		clingRolloffJSON.sliderAlt = componentInChildren.clingRolloffSlider;
		snapJSON.sliderAlt = componentInChildren.snapSlider;
		bendResistanceJSON.sliderAlt = componentInChildren.bendResistanceSlider;
		windJSON.sliderXAlt = componentInChildren.windXSlider;
		windJSON.sliderYAlt = componentInChildren.windYSlider;
		windJSON.sliderZAlt = componentInChildren.windZSlider;
		shaderTypeJSON.popupAlt = componentInChildren.shaderTypePopup;
		rootColorJSON.colorPickerAlt = componentInChildren.rootColorPicker;
		tipColorJSON.colorPickerAlt = componentInChildren.tipColorPicker;
		colorRolloffJSON.sliderAlt = componentInChildren.colorRolloffSlider;
		specularColorJSON.colorPickerAlt = componentInChildren.specularColorPicker;
		diffuseSoftnessJSON.sliderAlt = componentInChildren.diffuseSoftnessSlider;
		primarySpecularSharpnessJSON.sliderAlt = componentInChildren.primarySpecularSharpnessSlider;
		secondarySpecularSharpnessJSON.sliderAlt = componentInChildren.secondarySpecularSharpnessSlider;
		specularShiftJSON.sliderAlt = componentInChildren.specularShiftSlider;
		fresnelPowerJSON.sliderAlt = componentInChildren.fresnelPowerSlider;
		fresnelAttenuationJSON.sliderAlt = componentInChildren.fresnelAttenuationSlider;
		randomColorPowerJSON.sliderAlt = componentInChildren.randomColorPowerSlider;
		randomColorOffsetJSON.sliderAlt = componentInChildren.randomColorOffsetSlider;
		IBLFactorJSON.sliderAlt = componentInChildren.IBLFactorSlider;
		normalRandomizeJSON.sliderAlt = componentInChildren.normalRandomizeSlider;
		curlXJSON.sliderAlt = componentInChildren.curlXSlider;
		curlYJSON.sliderAlt = componentInChildren.curlYSlider;
		curlZJSON.sliderAlt = componentInChildren.curlZSlider;
		curlScaleJSON.sliderAlt = componentInChildren.curlScaleSlider;
		curlFrequencyJSON.sliderAlt = componentInChildren.curlFrequencySlider;
		curlScaleRandomnessJSON.sliderAlt = componentInChildren.curlScaleRandomnessSlider;
		curlFrequencyRandomnessJSON.sliderAlt = componentInChildren.curlFrequencyRandomnessSlider;
		curlAllowReverseJSON.toggleAlt = componentInChildren.curlAllowReverseToggle;
		curlAllowFlipAxisJSON.toggleAlt = componentInChildren.curlAllowFlipAxisToggle;
		curlNormalAdjustJSON.sliderAlt = componentInChildren.curlNormalAdjustSlider;
		if (curlRootJSON != null)
		{
			curlRootJSON.sliderAlt = componentInChildren.curlRootSlider;
		}
		if (curlMidJSON != null)
		{
			curlMidJSON.sliderAlt = componentInChildren.curlMidSlider;
		}
		if (curlTipJSON != null)
		{
			curlTipJSON.sliderAlt = componentInChildren.curlTipSlider;
		}
		if (curlMidpointJSON != null)
		{
			curlMidpointJSON.sliderAlt = componentInChildren.curlMidpointSlider;
		}
		if (curlCurvePowerJSON != null)
		{
			curlCurvePowerJSON.sliderAlt = componentInChildren.curlCurvePowerSlider;
		}
		length1JSON.sliderAlt = componentInChildren.length1Slider;
		length2JSON.sliderAlt = componentInChildren.length2Slider;
		length3JSON.sliderAlt = componentInChildren.length3Slider;
		widthJSON.sliderAlt = componentInChildren.widthSlider;
		densityJSON.sliderAlt = componentInChildren.densitySlider;
		detailJSON.sliderAlt = componentInChildren.detailSlider;
		maxSpreadJSON.sliderAlt = componentInChildren.maxSpreadSlider;
		if (spreadRootJSON != null)
		{
			spreadRootJSON.sliderAlt = componentInChildren.spreadRootSlider;
		}
		if (spreadMidJSON != null)
		{
			spreadMidJSON.sliderAlt = componentInChildren.spreadMidSlider;
		}
		if (spreadTipJSON != null)
		{
			spreadTipJSON.sliderAlt = componentInChildren.spreadTipSlider;
		}
		if (spreadMidpointJSON != null)
		{
			spreadMidpointJSON.sliderAlt = componentInChildren.spreadMidpointSlider;
		}
		if (spreadCurvePowerJSON != null)
		{
			spreadCurvePowerJSON.sliderAlt = componentInChildren.spreadCurvePowerSlider;
		}
	}

	protected void DeregisterUI()
	{
		if (creator != null)
		{
			simNearbyJointCountText = null;
			resetAndStartStyleModeAction.button = null;
			startStyleModeAction.button = null;
			cancelStyleModeAction.button = null;
			keepStyleAction.button = null;
			styleModeAllowControlOtherNodesJSON.toggle = null;
			styleJointsSearchDistanceJSON.slider = null;
			rebuildStyleJointsAction.button = null;
			clearStyleJointsAction.button = null;
			styleModeGravityMultiplierJSON.slider = null;
			styleModeShowCurlsJSON.toggle = null;
			styleModeUpHairPullStrengthJSON.slider = null;
			styleModeCollisionRadiusJSON.slider = null;
			if (styleModeCollisionRadiusRootJSON != null)
			{
				styleModeCollisionRadiusRootJSON.slider = null;
			}
			styleModeShowTool1JSON.toggle = null;
			styleModeShowTool2JSON.toggle = null;
			styleModeShowTool3JSON.toggle = null;
			styleModeShowTool4JSON.toggle = null;
		}
		simulationEnabledJSON.toggle = null;
		collisionEnabledJSON.toggle = null;
		collisionRadiusJSON.slider = null;
		if (collisionRadiusRootJSON != null)
		{
			collisionRadiusRootJSON.slider = null;
		}
		dragJSON.slider = null;
		usePaintedRigidityJSON.toggle = null;
		rootRigidityJSON.slider = null;
		mainRigidityJSON.slider = null;
		tipRigidityJSON.slider = null;
		rigidityRolloffPowerJSON.slider = null;
		if (jointRigidityJSON != null)
		{
			jointRigidityJSON.slider = null;
		}
		frictionJSON.slider = null;
		gravityMultiplierJSON.slider = null;
		weightJSON.slider = null;
		iterationsJSON.slider = null;
		clingJSON.slider = null;
		clingRolloffJSON.slider = null;
		snapJSON.slider = null;
		bendResistanceJSON.slider = null;
		windJSON.sliderX = null;
		windJSON.sliderY = null;
		windJSON.sliderZ = null;
		shaderTypeJSON.popup = null;
		rootColorJSON.colorPicker = null;
		tipColorJSON.colorPicker = null;
		colorRolloffJSON.slider = null;
		specularColorJSON.colorPicker = null;
		diffuseSoftnessJSON.slider = null;
		primarySpecularSharpnessJSON.slider = null;
		secondarySpecularSharpnessJSON.slider = null;
		specularShiftJSON.slider = null;
		fresnelPowerJSON.slider = null;
		fresnelAttenuationJSON.slider = null;
		randomColorPowerJSON.slider = null;
		randomColorOffsetJSON.slider = null;
		IBLFactorJSON.slider = null;
		normalRandomizeJSON.slider = null;
		curlXJSON.slider = null;
		curlYJSON.slider = null;
		curlZJSON.slider = null;
		curlScaleJSON.slider = null;
		curlFrequencyJSON.slider = null;
		curlScaleRandomnessJSON.slider = null;
		curlFrequencyRandomnessJSON.slider = null;
		curlAllowReverseJSON.toggle = null;
		curlAllowFlipAxisJSON.toggle = null;
		curlNormalAdjustJSON.slider = null;
		if (curlRootJSON != null)
		{
			curlRootJSON.slider = null;
		}
		if (curlMidJSON != null)
		{
			curlMidJSON.slider = null;
		}
		if (curlTipJSON != null)
		{
			curlTipJSON.slider = null;
		}
		if (curlMidpointJSON != null)
		{
			curlMidpointJSON.slider = null;
		}
		if (curlCurvePowerJSON != null)
		{
			curlCurvePowerJSON.slider = null;
		}
		length1JSON.slider = null;
		length2JSON.slider = null;
		length3JSON.slider = null;
		widthJSON.slider = null;
		densityJSON.slider = null;
		detailJSON.slider = null;
		maxSpreadJSON.slider = null;
		if (spreadRootJSON != null)
		{
			spreadRootJSON.slider = null;
		}
		if (spreadMidJSON != null)
		{
			spreadMidJSON.slider = null;
		}
		if (spreadTipJSON != null)
		{
			spreadTipJSON.slider = null;
		}
		if (spreadMidpointJSON != null)
		{
			spreadMidpointJSON.slider = null;
		}
		if (spreadCurvePowerJSON != null)
		{
			spreadCurvePowerJSON.slider = null;
		}
		simulationEnabledJSON.toggleAlt = null;
		collisionEnabledJSON.toggleAlt = null;
		collisionRadiusJSON.sliderAlt = null;
		dragJSON.sliderAlt = null;
		usePaintedRigidityJSON.toggleAlt = null;
		rootRigidityJSON.sliderAlt = null;
		mainRigidityJSON.sliderAlt = null;
		tipRigidityJSON.sliderAlt = null;
		rigidityRolloffPowerJSON.sliderAlt = null;
		if (jointRigidityJSON != null)
		{
			jointRigidityJSON.sliderAlt = null;
		}
		frictionJSON.sliderAlt = null;
		gravityMultiplierJSON.sliderAlt = null;
		weightJSON.sliderAlt = null;
		iterationsJSON.sliderAlt = null;
		clingJSON.sliderAlt = null;
		clingRolloffJSON.sliderAlt = null;
		snapJSON.sliderAlt = null;
		bendResistanceJSON.sliderAlt = null;
		shaderTypeJSON.popupAlt = null;
		rootColorJSON.colorPickerAlt = null;
		tipColorJSON.colorPickerAlt = null;
		colorRolloffJSON.sliderAlt = null;
		specularColorJSON.colorPickerAlt = null;
		diffuseSoftnessJSON.sliderAlt = null;
		primarySpecularSharpnessJSON.sliderAlt = null;
		secondarySpecularSharpnessJSON.sliderAlt = null;
		specularShiftJSON.sliderAlt = null;
		fresnelPowerJSON.sliderAlt = null;
		fresnelAttenuationJSON.sliderAlt = null;
		randomColorPowerJSON.sliderAlt = null;
		randomColorOffsetJSON.sliderAlt = null;
		IBLFactorJSON.sliderAlt = null;
		normalRandomizeJSON.sliderAlt = null;
		curlXJSON.sliderAlt = null;
		curlYJSON.sliderAlt = null;
		curlZJSON.sliderAlt = null;
		curlScaleJSON.sliderAlt = null;
		curlFrequencyJSON.sliderAlt = null;
		curlScaleRandomnessJSON.sliderAlt = null;
		curlFrequencyRandomnessJSON.sliderAlt = null;
		curlAllowReverseJSON.toggleAlt = null;
		curlAllowFlipAxisJSON.toggleAlt = null;
		curlNormalAdjustJSON.slider = null;
		if (curlRootJSON != null)
		{
			curlRootJSON.sliderAlt = null;
		}
		if (curlMidJSON != null)
		{
			curlMidJSON.sliderAlt = null;
		}
		if (curlTipJSON != null)
		{
			curlTipJSON.sliderAlt = null;
		}
		if (curlMidpointJSON != null)
		{
			curlMidpointJSON.sliderAlt = null;
		}
		if (curlCurvePowerJSON != null)
		{
			curlCurvePowerJSON.sliderAlt = null;
		}
		length1JSON.sliderAlt = null;
		length2JSON.sliderAlt = null;
		length3JSON.sliderAlt = null;
		widthJSON.sliderAlt = null;
		densityJSON.sliderAlt = null;
		detailJSON.sliderAlt = null;
		maxSpreadJSON.sliderAlt = null;
		if (spreadRootJSON != null)
		{
			spreadRootJSON.sliderAlt = null;
		}
		if (spreadMidJSON != null)
		{
			spreadMidJSON.sliderAlt = null;
		}
		if (spreadTipJSON != null)
		{
			spreadTipJSON.sliderAlt = null;
		}
		if (spreadMidpointJSON != null)
		{
			spreadMidpointJSON.sliderAlt = null;
		}
		if (spreadCurvePowerJSON != null)
		{
			spreadCurvePowerJSON.sliderAlt = null;
		}
	}

	public override void SetUI(Transform t)
	{
		if (UITransform != t)
		{
			UITransform = t;
			if (base.isActiveAndEnabled)
			{
				InitUI();
			}
		}
	}

	public override void SetUIAlt(Transform t)
	{
		if (UITransformAlt != t)
		{
			UITransformAlt = t;
			if (base.isActiveAndEnabled)
			{
				InitUIAlt();
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
		}
	}

	private void OnEnable()
	{
		InitUI();
		InitUIAlt();
	}

	private void OnDisable()
	{
		DeregisterUI();
		if (isRebuildingStyleJoints)
		{
			AbortRebuildStyleJointsThread(wait: false);
			isRebuildingStyleJoints = false;
		}
		CancelStyleMode();
	}

	private void Update()
	{
		SyncWind();
		if (!_styleMode || !(SuperController.singleton != null))
		{
			return;
		}
		HairSimControlTools hairSimControlTools = this.hairSimControlTools;
		if (!(hairSimControlTools != null))
		{
			return;
		}
		if (hairSimControlTools.StyleTool1 != null)
		{
			hairSimControlTools.StyleTool1.ResetToolStrengthMultiplier();
		}
		if (hairSimControlTools.StyleTool2 != null)
		{
			hairSimControlTools.StyleTool2.ResetToolStrengthMultiplier();
		}
		if (hairSimControlTools.StyleTool3 != null)
		{
			hairSimControlTools.StyleTool3.ResetToolStrengthMultiplier();
		}
		if (hairSimControlTools.StyleTool4 != null)
		{
			hairSimControlTools.StyleTool4.ResetToolStrengthMultiplier();
		}
		FreeControllerV3 rightFullGrabbedController = SuperController.singleton.RightFullGrabbedController;
		if (rightFullGrabbedController != null)
		{
			HairSimStyleToolControl hairSimStyleToolControl = null;
			if (rightFullGrabbedController == hairSimControlTools.hairStyleTool1Controller)
			{
				hairSimStyleToolControl = hairSimControlTools.StyleTool1;
			}
			else if (rightFullGrabbedController == hairSimControlTools.hairStyleTool2Controller)
			{
				hairSimStyleToolControl = hairSimControlTools.StyleTool2;
			}
			else if (rightFullGrabbedController == hairSimControlTools.hairStyleTool3Controller)
			{
				hairSimStyleToolControl = hairSimControlTools.StyleTool3;
			}
			else if (rightFullGrabbedController == hairSimControlTools.hairStyleTool4Controller)
			{
				hairSimStyleToolControl = hairSimControlTools.StyleTool4;
			}
			if (hairSimStyleToolControl != null)
			{
				hairSimStyleToolControl.SetToolStrengthMultiplier(SuperController.singleton.GetRightGrabVal());
			}
		}
		FreeControllerV3 leftFullGrabbedController = SuperController.singleton.LeftFullGrabbedController;
		if (leftFullGrabbedController != null)
		{
			HairSimStyleToolControl hairSimStyleToolControl2 = null;
			if (leftFullGrabbedController == hairSimControlTools.hairStyleTool1Controller)
			{
				hairSimStyleToolControl2 = hairSimControlTools.StyleTool1;
			}
			else if (leftFullGrabbedController == hairSimControlTools.hairStyleTool2Controller)
			{
				hairSimStyleToolControl2 = hairSimControlTools.StyleTool2;
			}
			else if (leftFullGrabbedController == hairSimControlTools.hairStyleTool3Controller)
			{
				hairSimStyleToolControl2 = hairSimControlTools.StyleTool3;
			}
			else if (leftFullGrabbedController == hairSimControlTools.hairStyleTool4Controller)
			{
				hairSimStyleToolControl2 = hairSimControlTools.StyleTool4;
			}
			if (hairSimStyleToolControl2 != null)
			{
				hairSimStyleToolControl2.SetToolStrengthMultiplier(SuperController.singleton.GetLeftGrabVal());
			}
		}
	}
}
