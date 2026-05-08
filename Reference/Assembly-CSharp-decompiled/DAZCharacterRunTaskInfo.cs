using System;
using System.Threading;
using MeshVR;
using MVR.FileManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class DAZCharacterRun : MonoBehaviour
{
	public enum DAZCharacterRunTaskType
	{
		Run,
		SecondaryMergeVerts,
		MergeVertsRun2,
		MergeVertsRun3,
		MergeVertsRun4
	}

	public class DAZCharacterRunTaskInfo
	{
		public DAZCharacterRunTaskType taskType;

		public string name;

		public AutoResetEvent resetEvent;

		public Thread thread;

		public volatile bool working;

		public volatile bool kill;

		public int index1;

		public int index2;
	}

	public bool waitForThread = true;

	private CustomSampler runTask1Sampler = CustomSampler.Create("RunThreaded");

	protected DAZCharacterRunTaskInfo characterRunTask;

	protected DAZCharacterRunTaskInfo characterRunTask2;

	protected DAZCharacterRunTaskInfo characterRunTask3;

	protected DAZCharacterRunTaskInfo characterRunTask4;

	protected DAZCharacterRunTaskInfo characterRunTask5;

	protected bool _threadsRunning;

	public DAZCharacterSelector characterSelector;

	public DAZMorphBank morphBank1;

	public bool useOtherGenderMorphs;

	public DAZMorphBank morphBank1OtherGender;

	public DAZMorphBank morphBank2;

	public DAZMorphBank morphBank3;

	public DAZBones bones;

	public DAZMergedSkinV2 skin;

	protected DAZMergedSkinV2 skinForThread;

	protected DAZMergedMesh mergedMesh;

	protected DAZMesh mesh1;

	protected DAZMesh mesh2;

	protected DAZMesh mesh3;

	public AutoColliderBatchUpdater[] autoColliderUpdaters;

	public DAZPhysicsMesh[] physicsMeshes;

	public Transform setDazMorphContainer;

	protected SetDAZMorphFromBoneAngle[] sdmFromBoneAngleComps;

	protected SetDAZMorphFromAverageBoneAngle[] sdmFromAverageBoneAngleComps;

	protected SetDAZMorphFromDistance[] sdmFromDistanceComps;

	protected SetDAZMorphFromLocalDistance[] sdmFromLocalDistanceComps;

	protected Vector3[] mesh1MorphedUVVertices;

	protected Vector3[] mesh1VisibleMorphedUVVertices;

	protected Vector3[] mesh2MorphedUVVertices;

	protected Vector3[] mesh2VisibleMorphedUVVertices;

	protected Vector3[] mesh3MorphedUVVertices;

	protected Vector3[] mesh3VisibleMorphedUVVertices;

	protected Vector3[] mergedMeshMorphedUVVertices;

	protected Vector3[] mergedMeshMorphedUVVerticesCopy;

	protected Vector3[] mergedMeshMorphedUVVerticesMergedOnly;

	protected Vector3[] mergedMeshVisibleMorphedUVVertices;

	protected bool _doSnap;

	protected Vector3[] _snappedMorphedUVVertices;

	protected Vector3[] _snappedMorphedUVNormals;

	protected Vector3[] _snappedSkinnedVertices;

	protected Vector3[] _snappedSkinnedNormals;

	protected float fixedStartTime;

	public float FIXED_time;

	protected float updateTimeStart;

	public float UPDATE_time;

	protected float lateTimeStart;

	public float LATE_time;

	protected float threadWaitTimeStart;

	public Text MAIN_threadWaitTimeText;

	public float MAIN_threadWaitTime;

	protected float autoColliderFinishTimeStart;

	public float MAIN_autoColliderFinishTime;

	protected float skinPrepStartTime;

	public float MAIN_skinPrepTime;

	protected float skinFinishStartTime;

	public float MAIN_skinFinishTime;

	protected float skinDrawStartTime;

	public float MAIN_skinDrawTime;

	protected float physicsMeshPrepStartTime;

	public float MAIN_physicsMeshPrepTime;

	protected float physicsMeshFinishStartTime;

	public float MAIN_physicsMeshFinishTime;

	protected float morphFinishStartTime;

	public float MAIN_morphFinishTime;

	protected float bonePrepStartTime;

	public float MAIN_bonePrepTime;

	protected float threadStartTime;

	public Text THREAD_timeText;

	public float THREAD_time;

	protected float morphTime1Start;

	public float THREAD_morphTime1;

	protected float morphTime2Start;

	public float THREAD_morphTime2;

	protected float morphTime3Start;

	public float THREAD_morphTime3;

	protected float mergeTimeStart;

	public Text THREAD_mergeTimeText;

	public float THREAD_mergeTime;

	protected float skinPostTimeStart;

	public float THREAD_skinPostTime;

	protected float skinTimeStart;

	public Text THREAD_skinTimeText;

	public float THREAD_skinTime;

	protected float postSkinMorphTimeStart;

	public float THREAD_postSkinMorphTime;

	protected float autoColliderUpdateSizeTimeStart;

	public float THREAD_autoColliderUpdateSizeTime;

	protected float autoColliderUpdateAnchorsTimeStart;

	public float THREAD_autoColliderUpdateAnchorsTime;

	protected float physicsMeshUpdateTargetsStartTime;

	public float THREAD_physicsMeshUpdateTargetsTime;

	protected float physicsMeshMorphStartTime;

	public float THREAD_physicsMeshMorphTime;

	protected float boneUpdateStartTime;

	public float THREAD_boneUpdateTime;

	protected int mergeRun2mini;

	protected int mergeRun2maxi;

	protected int mergeRun3mini;

	protected int mergeRun3maxi;

	protected int mergeRun4mini;

	protected int mergeRun4maxi;

	public bool visibleChangedThreaded;

	protected float lastMorphResetTime;

	protected bool triggerReset;

	protected bool threadWasRun;

	protected bool pmJointTargetsUpdatingOnThread;

	public bool waitForNewTargets = true;

	public bool fixedUpdateWaitForThread = true;

	protected bool newJointTargets = true;

	public bool doUpdate = true;

	public bool doSkin = true;

	public bool doDraw = true;

	public bool doSetMergedVerts;

	public bool doSetMergedMeshVerts;

	public bool doSetMorphedMeshVerts;

	public bool useThreading = true;

	public bool doSnap
	{
		get
		{
			return _doSnap;
		}
		set
		{
			_doSnap = value;
		}
	}

	public Vector3[] snappedMorphedUVVertices => _snappedMorphedUVVertices;

	public Vector3[] snappedMorphedUVNormals => _snappedMorphedUVNormals;

	public Vector3[] snappedSkinnedVertices => _snappedSkinnedVertices;

	public Vector3[] snappedSkinnedNormals => _snappedSkinnedNormals;

	protected void StopThreads()
	{
		_threadsRunning = false;
		if (characterRunTask != null)
		{
			characterRunTask.kill = true;
			characterRunTask.resetEvent.Set();
			while (characterRunTask.thread.IsAlive)
			{
			}
			characterRunTask = null;
		}
		if (characterRunTask2 != null)
		{
			characterRunTask2.kill = true;
			characterRunTask2.resetEvent.Set();
			while (characterRunTask2.thread.IsAlive)
			{
			}
			characterRunTask2 = null;
		}
		if (characterRunTask3 != null)
		{
			characterRunTask3.kill = true;
			characterRunTask3.resetEvent.Set();
			while (characterRunTask3.thread.IsAlive)
			{
			}
			characterRunTask3 = null;
		}
		if (characterRunTask4 != null)
		{
			characterRunTask4.kill = true;
			characterRunTask4.resetEvent.Set();
			while (characterRunTask4.thread.IsAlive)
			{
			}
			characterRunTask4 = null;
		}
		if (characterRunTask5 != null)
		{
			characterRunTask5.kill = true;
			characterRunTask5.resetEvent.Set();
			while (characterRunTask5.thread.IsAlive)
			{
			}
			characterRunTask5 = null;
		}
	}

	protected void StartThreads()
	{
		if (!_threadsRunning)
		{
			_threadsRunning = true;
			characterRunTask = new DAZCharacterRunTaskInfo();
			characterRunTask.name = "characterRunTask";
			characterRunTask.resetEvent = new AutoResetEvent(initialState: false);
			characterRunTask.thread = new Thread(MTTask);
			characterRunTask.thread.Priority = System.Threading.ThreadPriority.AboveNormal;
			characterRunTask.taskType = DAZCharacterRunTaskType.Run;
			characterRunTask.thread.Start(characterRunTask);
			characterRunTask2 = new DAZCharacterRunTaskInfo();
			characterRunTask2.name = "characterRunTask2";
			characterRunTask2.resetEvent = new AutoResetEvent(initialState: false);
			characterRunTask2.thread = new Thread(MTTask);
			characterRunTask2.thread.Priority = System.Threading.ThreadPriority.Normal;
			characterRunTask2.taskType = DAZCharacterRunTaskType.SecondaryMergeVerts;
			characterRunTask2.thread.Start(characterRunTask2);
			characterRunTask3 = new DAZCharacterRunTaskInfo();
			characterRunTask3.name = "characterRunTask3";
			characterRunTask3.resetEvent = new AutoResetEvent(initialState: false);
			characterRunTask3.thread = new Thread(MTTask);
			characterRunTask3.thread.Priority = System.Threading.ThreadPriority.AboveNormal;
			characterRunTask3.taskType = DAZCharacterRunTaskType.MergeVertsRun2;
			characterRunTask3.thread.Start(characterRunTask3);
			characterRunTask4 = new DAZCharacterRunTaskInfo();
			characterRunTask4.name = "characterRunTask4";
			characterRunTask4.resetEvent = new AutoResetEvent(initialState: false);
			characterRunTask4.thread = new Thread(MTTask);
			characterRunTask4.thread.Priority = System.Threading.ThreadPriority.AboveNormal;
			characterRunTask4.taskType = DAZCharacterRunTaskType.MergeVertsRun3;
			characterRunTask4.thread.Start(characterRunTask4);
			characterRunTask5 = new DAZCharacterRunTaskInfo();
			characterRunTask5.name = "characterRunTask5";
			characterRunTask5.resetEvent = new AutoResetEvent(initialState: false);
			characterRunTask5.thread = new Thread(MTTask);
			characterRunTask5.thread.Priority = System.Threading.ThreadPriority.AboveNormal;
			characterRunTask5.taskType = DAZCharacterRunTaskType.MergeVertsRun4;
			characterRunTask5.thread.Start(characterRunTask5);
		}
	}

	protected void MTTask(object info)
	{
		DAZCharacterRunTaskInfo dAZCharacterRunTaskInfo = (DAZCharacterRunTaskInfo)info;
		while (_threadsRunning)
		{
			dAZCharacterRunTaskInfo.resetEvent.WaitOne(-1, exitContext: true);
			if (dAZCharacterRunTaskInfo.kill)
			{
				break;
			}
			if (dAZCharacterRunTaskInfo.taskType == DAZCharacterRunTaskType.Run)
			{
				RunThreaded();
			}
			else if (dAZCharacterRunTaskInfo.taskType == DAZCharacterRunTaskType.SecondaryMergeVerts)
			{
				Thread.Sleep(0);
				SecondaryMergeVerts();
			}
			else if (dAZCharacterRunTaskInfo.taskType == DAZCharacterRunTaskType.MergeVertsRun2)
			{
				MergeRun2();
			}
			else if (dAZCharacterRunTaskInfo.taskType == DAZCharacterRunTaskType.MergeVertsRun3)
			{
				MergeRun3();
			}
			else if (dAZCharacterRunTaskInfo.taskType == DAZCharacterRunTaskType.MergeVertsRun4)
			{
				MergeRun4();
			}
			dAZCharacterRunTaskInfo.working = false;
		}
	}

	public void Disconnect()
	{
		skin = null;
		skinForThread = null;
	}

	public void Connect(bool genderChange)
	{
		if (!Application.isPlaying || !base.enabled || !(skin != null) || !(morphBank1 != null) || !(morphBank2 != null) || !(bones != null))
		{
			return;
		}
		WaitForRunTask();
		skinForThread = skin;
		if (setDazMorphContainer != null)
		{
			sdmFromAverageBoneAngleComps = setDazMorphContainer.GetComponentsInChildren<SetDAZMorphFromAverageBoneAngle>(includeInactive: true);
			SetDAZMorphFromAverageBoneAngle[] array = sdmFromAverageBoneAngleComps;
			foreach (SetDAZMorphFromAverageBoneAngle setDAZMorphFromAverageBoneAngle in array)
			{
				setDAZMorphFromAverageBoneAngle.updateEnabled = false;
			}
			sdmFromBoneAngleComps = setDazMorphContainer.GetComponentsInChildren<SetDAZMorphFromBoneAngle>(includeInactive: true);
			SetDAZMorphFromBoneAngle[] array2 = sdmFromBoneAngleComps;
			foreach (SetDAZMorphFromBoneAngle setDAZMorphFromBoneAngle in array2)
			{
				setDAZMorphFromBoneAngle.updateEnabled = false;
			}
			sdmFromDistanceComps = setDazMorphContainer.GetComponentsInChildren<SetDAZMorphFromDistance>(includeInactive: true);
			SetDAZMorphFromDistance[] array3 = sdmFromDistanceComps;
			foreach (SetDAZMorphFromDistance setDAZMorphFromDistance in array3)
			{
				setDAZMorphFromDistance.updateEnabled = false;
			}
			sdmFromLocalDistanceComps = setDazMorphContainer.GetComponentsInChildren<SetDAZMorphFromLocalDistance>(includeInactive: true);
			SetDAZMorphFromLocalDistance[] array4 = sdmFromLocalDistanceComps;
			foreach (SetDAZMorphFromLocalDistance setDAZMorphFromLocalDistance in array4)
			{
				setDAZMorphFromLocalDistance.updateEnabled = false;
			}
		}
		mergedMesh = skin.GetComponent<DAZMergedMesh>();
		mergedMesh.Init();
		mergedMeshMorphedUVVertices = (Vector3[])mergedMesh.morphedUVVertices.Clone();
		mergedMeshMorphedUVVerticesCopy = (Vector3[])mergedMesh.morphedUVVertices.Clone();
		mergedMeshMorphedUVVerticesMergedOnly = (Vector3[])mergedMesh.morphedUVVertices.Clone();
		mergedMeshVisibleMorphedUVVertices = (Vector3[])mergedMesh.morphedUVVertices.Clone();
		mesh1 = mergedMesh.targetMesh;
		mesh2 = mergedMesh.graftMesh;
		mesh3 = mergedMesh.graft2Mesh;
		morphBank1.Init();
		morphBank1.updateEnabled = false;
		if (morphBank1OtherGender != null)
		{
			morphBank1OtherGender.Init();
			morphBank1OtherGender.updateEnabled = false;
		}
		morphBank2.Init();
		morphBank2.updateEnabled = false;
		if (morphBank3 != null)
		{
			morphBank3.Init();
			morphBank3.updateEnabled = false;
		}
		mergedMesh.staticMesh = true;
		skin.Init();
		skin.skin = false;
		skin.draw = false;
		if (autoColliderUpdaters != null)
		{
			AutoColliderBatchUpdater[] array5 = autoColliderUpdaters;
			foreach (AutoColliderBatchUpdater autoColliderBatchUpdater in array5)
			{
				autoColliderBatchUpdater.UpdateAutoColliders();
				autoColliderBatchUpdater.clumpUpdate = false;
			}
		}
		if (physicsMeshes != null)
		{
			DAZPhysicsMesh[] array6 = physicsMeshes;
			foreach (DAZPhysicsMesh dAZPhysicsMesh in array6)
			{
				dAZPhysicsMesh.updateEnabled = false;
			}
		}
		DAZBone[] dazBones = bones.dazBones;
		foreach (DAZBone dAZBone in dazBones)
		{
			dAZBone.enabled = false;
		}
		ResetMorphsInternal();
		PrepRun();
		RunThreaded(!_threadsRunning);
		FinishRun();
		PrepRun();
		RunThreaded(!_threadsRunning);
		FinishRun();
		if (physicsMeshes != null)
		{
			DAZPhysicsMesh[] array7 = physicsMeshes;
			foreach (DAZPhysicsMesh dAZPhysicsMesh2 in array7)
			{
				dAZPhysicsMesh2.ResetSoftJoints();
			}
		}
		if (genderChange)
		{
			ResetMorphs();
		}
	}

	protected void RefreshPackageMorphs()
	{
		WaitForRunTask();
		if (characterSelector != null && characterSelector.RefreshPackageMorphs())
		{
			SmoothResetMorphs();
		}
	}

	protected void ResetMorphsInternal(bool resetBones = true)
	{
		mesh1MorphedUVVertices = (Vector3[])mesh1.UVVertices.Clone();
		mesh2MorphedUVVertices = (Vector3[])mesh2.UVVertices.Clone();
		mesh1VisibleMorphedUVVertices = (Vector3[])mesh1.UVVertices.Clone();
		mesh2VisibleMorphedUVVertices = (Vector3[])mesh2.UVVertices.Clone();
		if (mesh3 != null)
		{
			mesh3MorphedUVVertices = (Vector3[])mesh3.UVVertices.Clone();
			mesh3VisibleMorphedUVVertices = (Vector3[])mesh3.UVVertices.Clone();
		}
		morphBank1.ResetMorphsFast(resetBones);
		if (morphBank1OtherGender != null)
		{
			morphBank1OtherGender.ResetMorphsFast(resetBones);
		}
		morphBank2.ResetMorphsFast(resetBones);
		if (morphBank3 != null)
		{
			morphBank3.ResetMorphsFast(resetBones);
		}
	}

	public void ResetMorphs()
	{
		triggerReset = true;
	}

	public void WaitForRunTask()
	{
		if (_threadsRunning)
		{
			while (characterRunTask.working)
			{
				Thread.Sleep(0);
			}
		}
	}

	protected void SmoothResetMorphs()
	{
		WaitForRunTask();
		ResetMorphsInternal();
		SmoothApplyMorphs();
	}

	public void SmoothApplyMorphsLite()
	{
		WaitForRunTask();
		PrepRun();
		RunThreaded(!_threadsRunning);
		FinishRun();
	}

	public void SmoothApplyMorphs()
	{
		WaitForRunTask();
		PrepRun();
		RunThreaded(!_threadsRunning);
		FinishRun();
		PrepRun();
		RunThreaded(!_threadsRunning);
		FinishRun();
		if (physicsMeshes != null)
		{
			DAZPhysicsMesh[] array = physicsMeshes;
			foreach (DAZPhysicsMesh dAZPhysicsMesh in array)
			{
				dAZPhysicsMesh.ResetSoftJoints();
			}
		}
	}

	protected void PrepRun()
	{
		if (skin == null)
		{
			return;
		}
		float elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		skinPrepStartTime = elapsedMilliseconds;
		skin.SkinMeshCPUandGPUStartFrameFast();
		elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		MAIN_skinPrepTime = elapsedMilliseconds - skinPrepStartTime;
		physicsMeshPrepStartTime = elapsedMilliseconds;
		DAZPhysicsMesh[] array = physicsMeshes;
		foreach (DAZPhysicsMesh dAZPhysicsMesh in array)
		{
			if (dAZPhysicsMesh.isEnabled && dAZPhysicsMesh.wasInit)
			{
				dAZPhysicsMesh.PrepareSoftUpdateJointsThreaded();
				dAZPhysicsMesh.PrepareSoftMorphVerticesThreadedFast();
			}
		}
		elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		MAIN_physicsMeshPrepTime = elapsedMilliseconds - physicsMeshPrepStartTime;
		morphBank1.PrepMorphsThreadedFast();
		if (useOtherGenderMorphs && morphBank1OtherGender != null)
		{
			morphBank1OtherGender.PrepMorphsThreadedFast();
		}
		morphBank2.PrepMorphsThreadedFast();
		if (morphBank3 != null)
		{
			morphBank3.PrepMorphsThreadedFast();
		}
		elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		bonePrepStartTime = elapsedMilliseconds;
		if (bones != null)
		{
			DAZBone[] dazBones = bones.dazBones;
			foreach (DAZBone dAZBone in dazBones)
			{
				dAZBone.PrepThreadUpdate();
			}
		}
		elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		MAIN_bonePrepTime = elapsedMilliseconds - bonePrepStartTime;
		if (sdmFromDistanceComps != null)
		{
			SetDAZMorphFromDistance[] array2 = sdmFromDistanceComps;
			foreach (SetDAZMorphFromDistance setDAZMorphFromDistance in array2)
			{
				setDAZMorphFromDistance.DoUpdate();
			}
		}
		if (sdmFromLocalDistanceComps != null)
		{
			SetDAZMorphFromLocalDistance[] array3 = sdmFromLocalDistanceComps;
			foreach (SetDAZMorphFromLocalDistance setDAZMorphFromLocalDistance in array3)
			{
				setDAZMorphFromLocalDistance.DoUpdate();
			}
		}
	}

	protected void SecondaryMergeVerts()
	{
		try
		{
			mergedMesh.UpdateVerticesPrepThreadedFast(mesh1VisibleMorphedUVVertices, mergedMeshVisibleMorphedUVVertices, useAltMovementArray: true);
			mergedMesh.UpdateVerticesThreadedFast(mesh1VisibleMorphedUVVertices, mesh2VisibleMorphedUVVertices, mesh3VisibleMorphedUVVertices, mergedMeshVisibleMorphedUVVertices, 0, mergedMesh.numGraftBaseVertices, useAltMovementArray: true);
			mergedMesh.UpdateVerticesFinishThreadedFast(mesh1VisibleMorphedUVVertices, mesh2VisibleMorphedUVVertices, mesh3VisibleMorphedUVVertices, mergedMeshVisibleMorphedUVVertices, useAltMovementArray: true);
			if (autoColliderUpdaters != null && visibleChangedThreaded)
			{
				AutoColliderBatchUpdater[] array = autoColliderUpdaters;
				foreach (AutoColliderBatchUpdater autoColliderBatchUpdater in array)
				{
					if (autoColliderBatchUpdater.isEnabled)
					{
						autoColliderBatchUpdater.UpdateSizeThreadedFast(mergedMeshVisibleMorphedUVVertices, mergedMesh.morphedUVNormals);
					}
				}
			}
			DAZPhysicsMesh[] array2 = physicsMeshes;
			foreach (DAZPhysicsMesh dAZPhysicsMesh in array2)
			{
				if (dAZPhysicsMesh.isEnabled && dAZPhysicsMesh.wasInit && visibleChangedThreaded)
				{
					dAZPhysicsMesh.RecalculateLinkJointsFast(mergedMeshVisibleMorphedUVVertices);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception in thread caught " + ex.ToString());
		}
	}

	protected void MergeRun2()
	{
		try
		{
			mergedMesh.UpdateVerticesThreadedFast(mesh1MorphedUVVertices, mesh2MorphedUVVertices, mesh3MorphedUVVertices, mergedMeshMorphedUVVertices, mergeRun2mini, mergeRun2maxi);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception in thread caught " + ex.ToString());
		}
	}

	protected void MergeRun3()
	{
		try
		{
			mergedMesh.UpdateVerticesThreadedFast(mesh1MorphedUVVertices, mesh2MorphedUVVertices, mesh3MorphedUVVertices, mergedMeshMorphedUVVertices, mergeRun3mini, mergeRun3maxi);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception in thread caught " + ex.ToString());
		}
	}

	protected void MergeRun4()
	{
		try
		{
			mergedMesh.UpdateVerticesThreadedFast(mesh1MorphedUVVertices, mesh2MorphedUVVertices, mesh3MorphedUVVertices, mergedMeshMorphedUVVertices, mergeRun4mini, mergeRun4maxi);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception in thread caught " + ex.ToString());
		}
	}

	protected void RunThreaded(bool noThreading = false)
	{
		if (skin == null)
		{
			return;
		}
		try
		{
			threadStartTime = (boneUpdateStartTime = GlobalStopwatch.GetElapsedMilliseconds());
			DAZBone[] dazBones = bones.dazBones;
			foreach (DAZBone dAZBone in dazBones)
			{
				dAZBone.ThreadsafeUpdate();
			}
			if (skinForThread != null)
			{
				skinForThread.RecalcBones();
			}
			float elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
			THREAD_boneUpdateTime = elapsedMilliseconds - boneUpdateStartTime;
			if (sdmFromAverageBoneAngleComps != null)
			{
				SetDAZMorphFromAverageBoneAngle[] array = sdmFromAverageBoneAngleComps;
				foreach (SetDAZMorphFromAverageBoneAngle setDAZMorphFromAverageBoneAngle in array)
				{
					setDAZMorphFromAverageBoneAngle.DoUpdate();
				}
			}
			if (sdmFromBoneAngleComps != null)
			{
				SetDAZMorphFromBoneAngle[] array2 = sdmFromBoneAngleComps;
				foreach (SetDAZMorphFromBoneAngle setDAZMorphFromBoneAngle in array2)
				{
					setDAZMorphFromBoneAngle.DoUpdate();
				}
			}
			float num = 600000f;
			if (elapsedMilliseconds - lastMorphResetTime > num)
			{
				lastMorphResetTime = elapsedMilliseconds;
				ResetMorphsInternal(resetBones: false);
			}
			morphTime1Start = elapsedMilliseconds;
			bool flag = morphBank1.ApplyMorphsThreadedFast(mesh1MorphedUVVertices, mesh1VisibleMorphedUVVertices, bones);
			bool flag2 = false;
			if (useOtherGenderMorphs && morphBank1OtherGender != null)
			{
				flag2 = morphBank1OtherGender.ApplyMorphsThreadedFast(mesh1MorphedUVVertices, mesh1VisibleMorphedUVVertices, bones);
			}
			elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
			THREAD_morphTime1 = elapsedMilliseconds - morphTime1Start;
			morphTime2Start = elapsedMilliseconds;
			bool flag3 = morphBank2.ApplyMorphsThreadedFast(mesh2MorphedUVVertices, mesh2VisibleMorphedUVVertices, bones);
			elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
			THREAD_morphTime2 = elapsedMilliseconds - morphTime2Start;
			bool flag4 = false;
			if (morphBank3 != null && mesh3 != null)
			{
				morphTime3Start = elapsedMilliseconds;
				flag4 = morphBank3.ApplyMorphsThreadedFast(mesh3MorphedUVVertices, mesh3VisibleMorphedUVVertices, bones);
				elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				THREAD_morphTime3 = elapsedMilliseconds - morphTime3Start;
			}
			visibleChangedThreaded = flag || flag2 || flag3 || flag4;
			mergeTimeStart = elapsedMilliseconds;
			if (noThreading)
			{
				SecondaryMergeVerts();
			}
			else
			{
				characterRunTask2.working = true;
				characterRunTask2.resetEvent.Set();
			}
			int numGraftBaseVertices = mergedMesh.numGraftBaseVertices;
			int mini = 0;
			int num2 = (mergeRun2mini = numGraftBaseVertices / 4);
			mergeRun2maxi = mergeRun2mini + num2;
			mergeRun3mini = mergeRun2maxi;
			mergeRun3maxi = mergeRun3mini + num2;
			mergeRun4mini = mergeRun3maxi;
			mergeRun4maxi = numGraftBaseVertices;
			mergedMesh.UpdateVerticesPrepThreadedFast(mesh1MorphedUVVertices, mergedMeshMorphedUVVertices);
			if (noThreading)
			{
				MergeRun2();
				MergeRun3();
				MergeRun4();
			}
			else
			{
				characterRunTask3.working = true;
				characterRunTask3.resetEvent.Set();
				characterRunTask4.working = true;
				characterRunTask4.resetEvent.Set();
				characterRunTask5.working = true;
				characterRunTask5.resetEvent.Set();
			}
			mergedMesh.UpdateVerticesThreadedFast(mesh1MorphedUVVertices, mesh2MorphedUVVertices, mesh3MorphedUVVertices, mergedMeshMorphedUVVertices, mini, num2);
			if (!noThreading)
			{
				while (characterRunTask3.working)
				{
					Thread.Sleep(0);
				}
				while (characterRunTask4.working)
				{
					Thread.Sleep(0);
				}
				while (characterRunTask5.working)
				{
					Thread.Sleep(0);
				}
			}
			mergedMesh.UpdateVerticesFinishThreadedFast(mesh1MorphedUVVertices, mesh2MorphedUVVertices, mesh3MorphedUVVertices, mergedMeshMorphedUVVertices);
			Array.Copy(mergedMeshMorphedUVVertices, mergedMeshMorphedUVVerticesCopy, mergedMeshMorphedUVVertices.Length);
			elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
			THREAD_mergeTime = elapsedMilliseconds - mergeTimeStart;
			if (_doSnap)
			{
				_snappedMorphedUVVertices = (Vector3[])mergedMeshMorphedUVVertices.Clone();
			}
			if (doSetMergedVerts)
			{
				Array.Copy(mergedMeshMorphedUVVertices, mergedMeshMorphedUVVerticesMergedOnly, mergedMeshMorphedUVVertices.Length);
			}
			if (doSkin && skinForThread != null)
			{
				skinPostTimeStart = elapsedMilliseconds;
				skinForThread.SkinMeshPostVertsThreadedFast(mergedMeshMorphedUVVerticesCopy);
				elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				THREAD_skinPostTime = elapsedMilliseconds - skinPostTimeStart;
				physicsMeshUpdateTargetsStartTime = elapsedMilliseconds;
				DAZPhysicsMesh[] array3 = physicsMeshes;
				foreach (DAZPhysicsMesh dAZPhysicsMesh in array3)
				{
					if (dAZPhysicsMesh.isEnabled && dAZPhysicsMesh.wasInit)
					{
						dAZPhysicsMesh.UpdateSoftJointTargetsThreadedFast(skinForThread.rawSkinnedWorkingVerts, skinForThread.postSkinNormalsThreaded);
					}
				}
				newJointTargets = true;
				pmJointTargetsUpdatingOnThread = false;
				elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				THREAD_physicsMeshUpdateTargetsTime = elapsedMilliseconds - physicsMeshUpdateTargetsStartTime;
				skinTimeStart = elapsedMilliseconds;
				skinForThread.SkinMeshThreadedFast(mergedMeshMorphedUVVertices);
				elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				THREAD_skinTime = elapsedMilliseconds - skinTimeStart;
				physicsMeshMorphStartTime = elapsedMilliseconds;
				DAZPhysicsMesh[] array4 = physicsMeshes;
				foreach (DAZPhysicsMesh dAZPhysicsMesh2 in array4)
				{
					if (dAZPhysicsMesh2.isEnabled && dAZPhysicsMesh2.wasInit)
					{
						dAZPhysicsMesh2.MorphSoftVerticesThreadedFast(skinForThread.rawSkinnedWorkingVerts);
					}
				}
				elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				THREAD_physicsMeshMorphTime = elapsedMilliseconds - physicsMeshMorphStartTime;
				autoColliderUpdateAnchorsTimeStart = elapsedMilliseconds;
				if (autoColliderUpdaters != null)
				{
					AutoColliderBatchUpdater[] array5 = autoColliderUpdaters;
					foreach (AutoColliderBatchUpdater autoColliderBatchUpdater in array5)
					{
						if (autoColliderBatchUpdater.isEnabled)
						{
							autoColliderBatchUpdater.UpdateAnchorsThreadedFast(mergedMeshMorphedUVVertices, skinForThread.postSkinNormalsThreaded);
						}
					}
				}
				elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				THREAD_autoColliderUpdateAnchorsTime = elapsedMilliseconds - autoColliderUpdateAnchorsTimeStart;
				postSkinMorphTimeStart = elapsedMilliseconds;
				skinForThread.SkinMeshCPUandGPUApplyPostSkinMorphsFast();
				elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
				THREAD_postSkinMorphTime = elapsedMilliseconds - postSkinMorphTimeStart;
			}
			if (!noThreading)
			{
				while (characterRunTask2.working)
				{
					Thread.Sleep(0);
				}
			}
			THREAD_time = elapsedMilliseconds - threadStartTime;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception in thread caught " + ex.ToString());
		}
		pmJointTargetsUpdatingOnThread = false;
	}

	protected void FinishRun()
	{
		if (skin == null)
		{
			return;
		}
		if (triggerReset)
		{
			ResetMorphsInternal();
			triggerReset = false;
			return;
		}
		float elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		morphFinishStartTime = elapsedMilliseconds;
		morphBank1.ApplyMorphsThreadedFastFinish();
		if (useOtherGenderMorphs && morphBank1OtherGender != null)
		{
			morphBank1OtherGender.ApplyMorphsThreadedFastFinish();
		}
		morphBank2.ApplyMorphsThreadedFastFinish();
		if (morphBank3 != null)
		{
			morphBank3.ApplyMorphsThreadedFastFinish();
		}
		bool flag = false;
		if (useOtherGenderMorphs && morphBank1OtherGender != null)
		{
			flag = morphBank1OtherGender.bonesDirty;
		}
		if (flag || morphBank1.bonesDirty || morphBank2.bonesDirty)
		{
			bones.SetMorphedTransform();
			morphBank1.bonesDirty = false;
			if (useOtherGenderMorphs && morphBank1OtherGender != null)
			{
				morphBank1OtherGender.bonesDirty = false;
			}
			morphBank2.bonesDirty = false;
		}
		if (morphBank1.boneRotationsDirty != null && morphBank1.boneRotationsDirty.Count > 0)
		{
			foreach (DAZBone key in morphBank1.boneRotationsDirty.Keys)
			{
				key.SyncMorphBoneRotations();
			}
			morphBank1.boneRotationsDirty.Clear();
		}
		if (useOtherGenderMorphs && morphBank1OtherGender != null && morphBank1OtherGender.boneRotationsDirty != null)
		{
			foreach (DAZBone key2 in morphBank1OtherGender.boneRotationsDirty.Keys)
			{
				key2.SyncMorphBoneRotations();
			}
			morphBank1OtherGender.boneRotationsDirty.Clear();
		}
		if (morphBank2.boneRotationsDirty != null && morphBank2.boneRotationsDirty.Count > 0)
		{
			foreach (DAZBone key3 in morphBank2.boneRotationsDirty.Keys)
			{
				key3.SyncMorphBoneRotations();
			}
			morphBank2.boneRotationsDirty.Clear();
		}
		if (doSetMorphedMeshVerts)
		{
			mesh1.SetMorphedUVMeshVertices(mesh1MorphedUVVertices);
			mesh2.SetMorphedUVMeshVertices(mesh2MorphedUVVertices);
			if (mesh3 != null)
			{
				mesh3.SetMorphedUVMeshVertices(mesh3MorphedUVVertices);
			}
		}
		if (doSetMergedVerts)
		{
			Vector3[] morphedUVVertices = mergedMesh.morphedUVVertices;
			Array.Copy(mergedMeshMorphedUVVerticesMergedOnly, morphedUVVertices, mergedMeshMorphedUVVerticesMergedOnly.Length);
			if (doSetMergedMeshVerts)
			{
				mergedMesh.SetMorphedUVMeshVertices(morphedUVVertices);
			}
		}
		elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		MAIN_morphFinishTime = elapsedMilliseconds - morphFinishStartTime;
		if (!(skin != null))
		{
			return;
		}
		if (_doSnap)
		{
			if (_snappedMorphedUVVertices == null)
			{
				_snappedMorphedUVVertices = (Vector3[])mergedMeshMorphedUVVertices.Clone();
			}
			_snappedMorphedUVNormals = skin.RecalcAndGetAllNormals(_snappedMorphedUVVertices);
			Vector3[] array = (Vector3[])mergedMeshMorphedUVVertices.Clone();
			_snappedSkinnedVertices = (Vector3[])array.Clone();
			skin.SmoothAllVertices(array, _snappedSkinnedVertices);
			_snappedSkinnedNormals = skin.RecalcAndGetAllNormals(_snappedSkinnedVertices);
		}
		if (!doSkin)
		{
			return;
		}
		autoColliderFinishTimeStart = elapsedMilliseconds;
		if (autoColliderUpdaters != null)
		{
			AutoColliderBatchUpdater[] array2 = autoColliderUpdaters;
			foreach (AutoColliderBatchUpdater autoColliderBatchUpdater in array2)
			{
				autoColliderBatchUpdater.UpdateThreadedFinish(mergedMeshMorphedUVVertices, skin.postSkinNormalsThreaded);
			}
		}
		elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		MAIN_autoColliderFinishTime = elapsedMilliseconds - autoColliderFinishTimeStart;
		physicsMeshFinishStartTime = elapsedMilliseconds;
		DAZPhysicsMesh[] array3 = physicsMeshes;
		foreach (DAZPhysicsMesh dAZPhysicsMesh in array3)
		{
			if (dAZPhysicsMesh.isEnabled && dAZPhysicsMesh.wasInit && (visibleChangedThreaded || dAZPhysicsMesh.softVerticesAutoColliderRadiusDirty))
			{
				dAZPhysicsMesh.SoftVerticesSetAutoRadiusFast(mergedMeshVisibleMorphedUVVertices);
				dAZPhysicsMesh.RecalculateLinkJointsFinishFast();
			}
		}
		elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		MAIN_physicsMeshFinishTime = elapsedMilliseconds - physicsMeshFinishStartTime;
		skinFinishStartTime = elapsedMilliseconds;
		skin.SkinMeshCPUandGPUFinishFrameFast();
		elapsedMilliseconds = GlobalStopwatch.GetElapsedMilliseconds();
		MAIN_skinFinishTime = elapsedMilliseconds - skinFinishStartTime;
	}

	private void FixedUpdate()
	{
		if (!(skin != null) || mergedMeshMorphedUVVertices == null || !threadWasRun || !doSkin || !doUpdate)
		{
			return;
		}
		float num = (fixedStartTime = GlobalStopwatch.GetElapsedMilliseconds());
		if (useThreading && fixedUpdateWaitForThread && DAZPhysicsMesh.globalEnable)
		{
			threadWaitTimeStart = num;
			while (pmJointTargetsUpdatingOnThread)
			{
				Thread.Sleep(0);
			}
			if (newJointTargets)
			{
				num = GlobalStopwatch.GetElapsedMilliseconds();
				MAIN_threadWaitTime = num - threadWaitTimeStart;
				PerfMon.ReportWaitTime(MAIN_threadWaitTime);
				if (MAIN_threadWaitTimeText != null && MAIN_threadWaitTimeText.IsActive())
				{
					MAIN_threadWaitTimeText.text = MAIN_threadWaitTime.ToString("F2");
				}
			}
		}
		DAZPhysicsMesh[] array = physicsMeshes;
		foreach (DAZPhysicsMesh dAZPhysicsMesh in array)
		{
			if (dAZPhysicsMesh.isEnabled && dAZPhysicsMesh.wasInit)
			{
				dAZPhysicsMesh.UpdateSoftJointsFast(waitForNewTargets && !newJointTargets);
				dAZPhysicsMesh.ApplySoftJointBackForces();
			}
		}
		num = GlobalStopwatch.GetElapsedMilliseconds();
		FIXED_time = num - fixedStartTime;
		newJointTargets = false;
	}

	private void Update()
	{
		if (!(skin != null) || mergedMeshMorphedUVVertices == null || !doUpdate)
		{
			return;
		}
		float num = (updateTimeStart = GlobalStopwatch.GetElapsedMilliseconds());
		if (useThreading)
		{
			StartThreads();
			threadWaitTimeStart = num;
			if (waitForThread)
			{
				while (characterRunTask.working)
				{
					Thread.Sleep(0);
				}
			}
			num = GlobalStopwatch.GetElapsedMilliseconds();
			float num2 = num - threadWaitTimeStart;
			PerfMon.ReportWaitTime(num2);
			if (!fixedUpdateWaitForThread)
			{
				MAIN_threadWaitTime = num2;
				if (MAIN_threadWaitTimeText != null && MAIN_threadWaitTimeText.IsActive())
				{
					MAIN_threadWaitTimeText.text = MAIN_threadWaitTime.ToString("F2");
				}
			}
			if (!characterRunTask.working)
			{
				threadWasRun = true;
				if (THREAD_timeText != null && THREAD_timeText.IsActive())
				{
					THREAD_timeText.text = THREAD_time.ToString("F2");
				}
				if (THREAD_mergeTimeText != null && THREAD_timeText.IsActive())
				{
					THREAD_mergeTimeText.text = THREAD_mergeTime.ToString("F2");
				}
				if (THREAD_skinTimeText != null && THREAD_skinTimeText.IsActive())
				{
					THREAD_skinTimeText.text = THREAD_skinTime.ToString("F2");
				}
				FinishRun();
				PrepRun();
				pmJointTargetsUpdatingOnThread = true;
				characterRunTask.working = true;
				characterRunTask.resetEvent.Set();
			}
		}
		else
		{
			threadWasRun = true;
			RunThreaded(noThreading: true);
			FinishRun();
			PrepRun();
		}
		num = GlobalStopwatch.GetElapsedMilliseconds();
		UPDATE_time = num - updateTimeStart;
	}

	private void LateUpdate()
	{
		if (skin != null && mergedMeshMorphedUVVertices != null)
		{
			float num = (lateTimeStart = GlobalStopwatch.GetElapsedMilliseconds());
			if (doDraw)
			{
				num = GlobalStopwatch.GetElapsedMilliseconds();
				skinDrawStartTime = num;
				skin.DrawMeshGPU();
				num = GlobalStopwatch.GetElapsedMilliseconds();
				MAIN_skinDrawTime = num - skinDrawStartTime;
			}
			LATE_time = num - lateTimeStart;
		}
	}

	protected void Start()
	{
		FileManager.RegisterRefreshHandler(RefreshPackageMorphs);
	}

	protected void OnDestroy()
	{
		FileManager.UnregisterRefreshHandler(RefreshPackageMorphs);
		StopThreads();
	}

	protected void OnApplicationQuit()
	{
		StopThreads();
	}
}
