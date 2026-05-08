using System.Collections.Generic;
using GPUTools.Hair.Scripts.Runtime.Physics;
using GPUTools.Skinner.Scripts.Providers;
using MeshVR;
using MVR.FileManagement;
using UnityEngine;

[ExecuteInEditMode]
public class DAZHairGroup : DAZDynamicItem
{
	public CapsuleCollider[] hairColliders;

	public DAZBones rootBonesForSkinning;

	public void SyncHairAdjustments()
	{
		if (characterSelector != null)
		{
			characterSelector.SyncHairAdjustments();
		}
	}

	public void RefreshHairItems()
	{
		if (characterSelector != null)
		{
			characterSelector.RefreshDynamicHair();
		}
	}

	public void RefreshHairItemThumbnail(string thumbPath)
	{
		if (characterSelector != null)
		{
			characterSelector.InvalidateDynamicHairItemThumbnail(thumbPath);
			characterSelector.RefreshDynamicHairThumbnails();
		}
	}

	public bool IsHairUIDAvailable(string uid)
	{
		if (characterSelector != null)
		{
			return characterSelector.IsHairUIDAvailable(uid);
		}
		return false;
	}

	protected override void SyncOtherTags()
	{
		otherTags = GetAllHairOtherTags();
		base.SyncOtherTags();
	}

	public HashSet<string> GetAllHairOtherTags()
	{
		if (characterSelector != null)
		{
			return characterSelector.GetHairOtherTags();
		}
		return null;
	}

	protected override void SetUserPrefsPath()
	{
		if (dynamicRuntimeLoadPath != null && dynamicRuntimeLoadPath != string.Empty)
		{
			string text = FileManager.RemovePackageFromPath(dynamicRuntimeLoadPath);
			userPrefsPath = text.Replace(".vam", ".prefs");
		}
		else if (assetName != null && assetName != string.Empty)
		{
			if (gender == Gender.Female)
			{
				userPrefsPath = "Custom/Hair/Female/Builtin/" + assetName + "/" + assetName + ".prefs";
			}
			else if (gender == Gender.Male)
			{
				userPrefsPath = "Custom/Hair/Male/Builtin/" + assetName + "/" + assetName + ".prefs";
			}
			else
			{
				userPrefsPath = "Custom/Hair/Neutral/Builtin/" + assetName + "/" + assetName + ".prefs";
			}
		}
	}

	protected override void InitInstance()
	{
		base.InitInstance();
		DAZHairGroupControl[] componentsInChildren = GetComponentsInChildren<DAZHairGroupControl>(includeInactive: true);
		if (componentsInChildren != null)
		{
			DAZHairGroupControl[] array = componentsInChildren;
			foreach (DAZHairGroupControl dAZHairGroupControl in array)
			{
				dAZHairGroupControl.hairItem = this;
			}
		}
		if (base.skin != null)
		{
			PreCalcMeshProviderHolder[] componentsInChildren2 = GetComponentsInChildren<PreCalcMeshProviderHolder>(includeInactive: true);
			PreCalcMeshProviderHolder[] array2 = componentsInChildren2;
			foreach (PreCalcMeshProviderHolder preCalcMeshProviderHolder in array2)
			{
				preCalcMeshProviderHolder.provider = base.skin;
			}
		}
		DAZDynamic componentInChildren = GetComponentInChildren<DAZDynamic>(includeInactive: true);
		if (componentInChildren != null)
		{
			if (gender == Gender.Female)
			{
				componentInChildren.itemType = PresetManager.ItemType.HairFemale;
			}
			else if (gender == Gender.Male)
			{
				componentInChildren.itemType = PresetManager.ItemType.HairMale;
			}
			else
			{
				componentInChildren.itemType = PresetManager.ItemType.HairNeutral;
			}
			if (dynamicRuntimeLoadPath != null && dynamicRuntimeLoadPath != string.Empty)
			{
				componentInChildren.SetNamesFromPath(dynamicRuntimeLoadPath);
				componentInChildren.Load();
			}
		}
		PresetManagerControl componentInChildren2 = GetComponentInChildren<PresetManagerControl>(includeInactive: true);
		if (componentInChildren2 != null)
		{
			componentInChildren2.SyncPresetUI();
		}
	}

	protected override void Connect()
	{
		base.Connect();
		DAZHairMesh[] componentsInChildren = GetComponentsInChildren<DAZHairMesh>(includeInactive: true);
		DAZHairMesh[] array = componentsInChildren;
		foreach (DAZHairMesh dAZHairMesh in array)
		{
			dAZHairMesh.capsuleColliders = hairColliders;
			DAZSkinV2MeshSelection component = dAZHairMesh.gameObject.GetComponent<DAZSkinV2MeshSelection>();
			if (component != null && base.skin != null)
			{
				component.meshTransform = base.skin.transform;
				component.skin = base.skin;
			}
			dAZHairMesh.Reset();
		}
		PreCalcMeshProviderHolder[] componentsInChildren2 = GetComponentsInChildren<PreCalcMeshProviderHolder>(includeInactive: true);
		PreCalcMeshProviderHolder[] array2 = componentsInChildren2;
		foreach (PreCalcMeshProviderHolder preCalcMeshProviderHolder in array2)
		{
			preCalcMeshProviderHolder.provider = base.skin;
		}
		ResetPhysics();
		if (rootBonesForSkinning != null)
		{
			DAZSkinV2[] componentsInChildren3 = GetComponentsInChildren<DAZSkinV2>(includeInactive: true);
			DAZSkinV2[] array3 = componentsInChildren3;
			foreach (DAZSkinV2 dAZSkinV in array3)
			{
				dAZSkinV.root = rootBonesForSkinning;
			}
		}
	}

	public override void PartialResetPhysics()
	{
		if (base.enabled)
		{
			GPHairPhysics[] componentsInChildren = GetComponentsInChildren<GPHairPhysics>(includeInactive: true);
			GPHairPhysics[] array = componentsInChildren;
			foreach (GPHairPhysics gPHairPhysics in array)
			{
				gPHairPhysics.PartialResetPhysics();
			}
		}
	}

	public override void ResetPhysics()
	{
		if (base.enabled)
		{
			GPHairPhysics[] componentsInChildren = GetComponentsInChildren<GPHairPhysics>(includeInactive: true);
			GPHairPhysics[] array = componentsInChildren;
			foreach (GPHairPhysics gPHairPhysics in array)
			{
				gPHairPhysics.ResetPhysics();
			}
		}
	}
}
