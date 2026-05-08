using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetAnchorFromVertex : PhysicsSimulator
{
	[HideInInspector]
	public Transform skinTransform;

	[HideInInspector]
	public DAZSkinV2 skin;

	[HideInInspector]
	public int subMeshSelection;

	public int targetVertex = -1;

	public ConfigurableJoint joint;

	public bool setX = true;

	public bool setY = true;

	public bool setZ = true;

	public bool showHandles = true;

	public bool showBackfaceHandles;

	public float handleSize = 0.0002f;

	protected Dictionary<int, int> _uvVertToBaseVertDict;

	protected Dictionary<int, int> uvVertToBaseVertDict
	{
		get
		{
			if (_uvVertToBaseVertDict == null)
			{
				if (skin != null && skin.dazMesh != null)
				{
					_uvVertToBaseVertDict = skin.dazMesh.uvVertToBaseVert;
				}
				else
				{
					_uvVertToBaseVertDict = new Dictionary<int, int>();
				}
			}
			return _uvVertToBaseVertDict;
		}
	}

	public void ClickVertex(int vid)
	{
		if (targetVertex == vid)
		{
			targetVertex = -1;
		}
		else
		{
			targetVertex = vid;
		}
	}

	public int GetBaseVertex(int vid)
	{
		if (skin != null && skin.dazMesh != null && uvVertToBaseVertDict.TryGetValue(vid, out var value))
		{
			vid = value;
		}
		return vid;
	}

	public bool IsBaseVertex(int vid)
	{
		if (skin != null && skin.dazMesh != null)
		{
			return !uvVertToBaseVertDict.ContainsKey(vid);
		}
		return true;
	}

	protected override void SyncPauseSimulation()
	{
		base.SyncPauseSimulation();
		SyncCollisionEnabled();
	}

	protected override void SyncCollisionEnabled()
	{
		base.SyncCollisionEnabled();
		if (joint != null)
		{
			Rigidbody component = joint.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.detectCollisions = _collisionEnabled && !_pauseSimulation;
			}
		}
	}

	protected override void SyncUseInterpolation()
	{
		base.SyncUseInterpolation();
		if (!(joint != null))
		{
			return;
		}
		Rigidbody component = joint.GetComponent<Rigidbody>();
		if (component != null)
		{
			if (_useInterpolation)
			{
				component.interpolation = RigidbodyInterpolation.Interpolate;
			}
			else
			{
				component.interpolation = RigidbodyInterpolation.None;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!(joint != null) || !(skin != null) || targetVertex == -1)
		{
			return;
		}
		bool flag = true;
		Vector3[] array;
		if (Application.isPlaying)
		{
			array = skin.rawSkinnedVerts;
			if (!skin.postSkinVerts[targetVertex])
			{
				skin.postSkinVerts[targetVertex] = true;
				skin.postSkinVertsChanged = true;
			}
			flag = skin.postSkinVertsReady[targetVertex];
		}
		else
		{
			array = skin.dazMesh.morphedUVVertices;
		}
		if (array != null && targetVertex < array.Length && flag)
		{
			Vector3 position = array[targetVertex];
			Vector3 connectedAnchor = joint.connectedAnchor;
			Vector3 vector = joint.connectedBody.transform.InverseTransformPoint(position);
			if (!setX)
			{
				vector.x = connectedAnchor.x;
			}
			if (!setY)
			{
				vector.y = connectedAnchor.y;
			}
			if (!setZ)
			{
				vector.z = connectedAnchor.z;
			}
			joint.connectedAnchor = vector;
			if (!Application.isPlaying || _pauseSimulation)
			{
				base.transform.localPosition = vector;
			}
		}
	}
}
