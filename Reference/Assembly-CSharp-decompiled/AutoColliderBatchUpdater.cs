using System.Collections.Generic;
using UnityEngine;

public class AutoColliderBatchUpdater : MonoBehaviour
{
	protected AutoCollider[] _autoColliders;

	protected AutoCollider[] _autoCollidersWithJoints;

	protected AutoCollider[] _autoCollidersWithoutJoints;

	public bool clumpUpdate;

	public DAZSkinV2 skin;

	public int numControlledColliders;

	protected bool _on = true;

	private bool _pauseSimulation;

	protected List<AsyncFlag> waitResumeSimulationFlags;

	protected bool morphsChanged;

	public bool isEnabled;

	protected AsyncFlag enablePauseFlag;

	protected int pauseCountdown;

	public AutoCollider[] autoColliders => _autoColliders;

	public bool on
	{
		get
		{
			return _on;
		}
		set
		{
			if (_on != value)
			{
				_on = value;
			}
		}
	}

	public bool pauseSimulation
	{
		get
		{
			return _pauseSimulation;
		}
		set
		{
			if (_pauseSimulation != value)
			{
				_pauseSimulation = value;
				if (_autoColliders == null)
				{
					UpdateAutoColliders();
				}
				AutoCollider[] array = _autoColliders;
				foreach (AutoCollider autoCollider in array)
				{
					autoCollider.pauseSimulation = value;
				}
			}
		}
	}

	public void UpdateAutoColliders()
	{
		AutoCollider[] componentsInChildren = GetComponentsInChildren<AutoCollider>();
		List<AutoCollider> list = new List<AutoCollider>();
		List<AutoCollider> list2 = new List<AutoCollider>();
		List<AutoCollider> list3 = new List<AutoCollider>();
		AutoCollider[] array = componentsInChildren;
		foreach (AutoCollider autoCollider in array)
		{
			if (autoCollider.allowBatchUpdate)
			{
				autoCollider.enabled = false;
				list.Add(autoCollider);
				if (autoCollider.joint != null)
				{
					list2.Add(autoCollider);
				}
				else
				{
					list3.Add(autoCollider);
				}
			}
		}
		_autoColliders = list.ToArray();
		_autoCollidersWithJoints = list2.ToArray();
		_autoCollidersWithoutJoints = list3.ToArray();
		numControlledColliders = _autoColliders.Length;
	}

	protected void CheckResumeSimulation()
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		bool flag = false;
		if (waitResumeSimulationFlags.Count > 0)
		{
			List<AsyncFlag> list = new List<AsyncFlag>();
			foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
			{
				if (waitResumeSimulationFlag.Raised)
				{
					list.Add(waitResumeSimulationFlag);
					flag = true;
				}
			}
			foreach (AsyncFlag item in list)
			{
				waitResumeSimulationFlags.Remove(item);
			}
		}
		if (waitResumeSimulationFlags.Count > 0)
		{
			pauseSimulation = true;
		}
		else if (flag)
		{
			pauseSimulation = false;
		}
	}

	public void PauseSimulation(AsyncFlag waitFor)
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		waitResumeSimulationFlags.Add(waitFor);
		pauseSimulation = true;
	}

	public void UpdateSizeThreadedFast(Vector3[] verts, Vector3[] norms)
	{
		if (_autoColliders != null && _autoColliders.Length > 0)
		{
			AutoCollider[] array = _autoColliders;
			foreach (AutoCollider autoCollider in array)
			{
				autoCollider.AutoColliderSizeSetFast(verts);
				autoCollider.UpdateHardTransformPositionFast(verts, norms);
			}
		}
	}

	public void UpdateAnchorsThreadedFast(Vector3[] verts, Vector3[] norms)
	{
		if (_autoColliders == null || _autoColliders.Length <= 0)
		{
			return;
		}
		AutoCollider[] array = _autoColliders;
		foreach (AutoCollider autoCollider in array)
		{
			if (autoCollider.centerJoint)
			{
				if (autoCollider.lookAtOption == AutoCollider.LookAtOption.Opposite && autoCollider.oppositeVertex != -1)
				{
					autoCollider.anchorTarget = (verts[autoCollider.targetVertex] + verts[autoCollider.oppositeVertex]) * 0.5f;
				}
				else if (autoCollider.lookAtOption == AutoCollider.LookAtOption.AnchorCenters && autoCollider.anchorVertex1 != -1 && autoCollider.anchorVertex2 != -1)
				{
					autoCollider.anchorTarget = (verts[autoCollider.anchorVertex1] + verts[autoCollider.anchorVertex2]) * 0.5f;
				}
				else
				{
					if (autoCollider.lookAtOption != 0)
					{
						continue;
					}
					if (autoCollider.colliderOrient == AutoCollider.ColliderOrient.Look)
					{
						float num = autoCollider.colliderLength * 0.5f * autoCollider.scale;
						if (num < autoCollider.colliderRadius * autoCollider.scale)
						{
							num = autoCollider.colliderRadius * autoCollider.scale;
						}
						autoCollider.anchorTarget = verts[autoCollider.targetVertex] + norms[autoCollider.targetVertex] * (0f - num);
					}
					else
					{
						autoCollider.anchorTarget = verts[autoCollider.targetVertex] + norms[autoCollider.targetVertex] * (0f - autoCollider.colliderRadius) * autoCollider.scale;
					}
				}
			}
			else
			{
				autoCollider.anchorTarget = verts[autoCollider.targetVertex];
			}
		}
	}

	public void UpdateThreadedFinish(Vector3[] verts, Vector3[] norms)
	{
		if (_pauseSimulation)
		{
			AutoCollider[] array = _autoColliders;
			foreach (AutoCollider autoCollider in array)
			{
				if (autoCollider.joint != null && !autoCollider.skipResetOnPause)
				{
					autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
					autoCollider.joint.transform.position = autoCollider.anchorTarget;
				}
				if (autoCollider.colliderDirty)
				{
					autoCollider.AutoColliderSizeSetFinishFast();
				}
			}
			return;
		}
		AutoCollider[] autoCollidersWithJoints = _autoCollidersWithJoints;
		foreach (AutoCollider autoCollider2 in autoCollidersWithJoints)
		{
			autoCollider2.joint.connectedAnchor = autoCollider2.backForceRigidbody.transform.InverseTransformPoint(autoCollider2.anchorTarget);
			if (autoCollider2.colliderDirty)
			{
				autoCollider2.AutoColliderSizeSetFinishFast();
			}
		}
		AutoCollider[] autoCollidersWithoutJoints = _autoCollidersWithoutJoints;
		foreach (AutoCollider autoCollider3 in autoCollidersWithoutJoints)
		{
			if (autoCollider3.colliderDirty)
			{
				autoCollider3.AutoColliderSizeSetFinishFast();
			}
		}
	}

	protected void UpdateAnchors()
	{
		if (_autoColliders == null || _autoColliders.Length <= 0 || !(skin != null))
		{
			return;
		}
		Vector3[] rawSkinnedVerts = skin.rawSkinnedVerts;
		Vector3[] postSkinNormals = skin.postSkinNormals;
		AutoCollider[] array = _autoColliders;
		foreach (AutoCollider autoCollider in array)
		{
			if (autoCollider.joint != null && (!_pauseSimulation || !autoCollider.skipResetOnPause))
			{
				if (autoCollider.centerJoint)
				{
					if (autoCollider.lookAtOption == AutoCollider.LookAtOption.Opposite && autoCollider.oppositeVertex != -1)
					{
						autoCollider.anchorTarget = (rawSkinnedVerts[autoCollider.targetVertex] + rawSkinnedVerts[autoCollider.oppositeVertex]) * 0.5f;
						autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
					}
					else if (autoCollider.lookAtOption == AutoCollider.LookAtOption.AnchorCenters && autoCollider.anchorVertex1 != -1 && autoCollider.anchorVertex2 != -1)
					{
						autoCollider.anchorTarget = (rawSkinnedVerts[autoCollider.anchorVertex1] + rawSkinnedVerts[autoCollider.anchorVertex2]) * 0.5f;
						autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
					}
					else if (autoCollider.lookAtOption == AutoCollider.LookAtOption.VertexNormal)
					{
						if (autoCollider.colliderOrient == AutoCollider.ColliderOrient.Look)
						{
							float num = autoCollider.colliderLength * 0.5f;
							if (num < autoCollider.colliderRadius)
							{
								num = autoCollider.colliderRadius;
							}
							autoCollider.anchorTarget = rawSkinnedVerts[autoCollider.targetVertex] + postSkinNormals[autoCollider.targetVertex] * (0f - num);
							autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
						}
						else
						{
							autoCollider.anchorTarget = rawSkinnedVerts[autoCollider.targetVertex] + postSkinNormals[autoCollider.targetVertex] * (0f - autoCollider.colliderRadius);
							autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
						}
					}
				}
				else
				{
					autoCollider.anchorTarget = rawSkinnedVerts[autoCollider.targetVertex];
					autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
				}
			}
			if (autoCollider.debug)
			{
				MyDebug.DrawWireCube(autoCollider.anchorTarget, 0.005f, Color.blue);
			}
		}
	}

	protected void ResetJoints()
	{
		UpdateAnchors();
		if (_autoColliders == null || _autoColliders.Length <= 0)
		{
			return;
		}
		AutoCollider[] array = _autoColliders;
		foreach (AutoCollider autoCollider in array)
		{
			if (autoCollider.joint != null && !autoCollider.skipResetOnPause)
			{
				autoCollider.joint.transform.position = autoCollider.anchorTarget;
			}
		}
	}

	private void OnEnable()
	{
		UpdateAutoColliders();
		AutoCollider[] array = _autoColliders;
		foreach (AutoCollider autoCollider in array)
		{
			autoCollider.enabled = false;
			autoCollider.Init();
		}
		isEnabled = true;
		enablePauseFlag = new AsyncFlag("EnablePauseFlag");
		PauseSimulation(enablePauseFlag);
		pauseCountdown = 10;
	}

	private void OnDisable()
	{
		isEnabled = false;
		AutoCollider[] array = _autoColliders;
		foreach (AutoCollider autoCollider in array)
		{
			autoCollider.enabled = true;
		}
		if (enablePauseFlag != null)
		{
			enablePauseFlag.Raise();
			pauseCountdown = 0;
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			if (enablePauseFlag != null && !enablePauseFlag.Raised)
			{
				pauseCountdown--;
				if (pauseCountdown <= 0)
				{
					enablePauseFlag.Raise();
				}
			}
			CheckResumeSimulation();
		}
		if (skin != null && (skin.dazMesh.visibleVerticesChangedLastFrame || skin.dazMesh.visibleVerticesChangedThisFrame))
		{
			morphsChanged = true;
		}
	}

	private void FixedUpdate()
	{
		if (!_on || !clumpUpdate)
		{
			return;
		}
		if (pauseSimulation)
		{
			ResetJoints();
			return;
		}
		UpdateAnchors();
		if (morphsChanged)
		{
			morphsChanged = false;
			AutoCollider[] array = _autoColliders;
			foreach (AutoCollider autoCollider in array)
			{
				autoCollider.AutoColliderSizeSet();
			}
		}
	}
}
