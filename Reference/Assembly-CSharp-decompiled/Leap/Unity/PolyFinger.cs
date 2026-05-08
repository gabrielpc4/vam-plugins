using System;
using UnityEngine;

namespace Leap.Unity;

public class PolyFinger : FingerModel
{
	private const int MAX_SIDES = 30;

	private const int TRIANGLE_INDICES_PER_QUAD = 6;

	private const int VERTICES_PER_QUAD = 4;

	public int sides = 4;

	public bool smoothNormals;

	public float startingAngle;

	public float[] widths = new float[3];

	protected Vector3[] vertices_;

	protected Vector3[] normals_;

	protected Vector3[] joint_vertices_;

	protected Mesh mesh_;

	protected Mesh cap_mesh_;

	protected Vector3[] cap_vertices_;

	public override void InitFinger()
	{
		InitJointVertices();
		InitCapsMesh();
		InitMesh();
		GetComponent<MeshFilter>().mesh = new Mesh();
		UpdateFinger();
	}

	public override void UpdateFinger()
	{
		UpdateMesh();
		UpdateCapMesh();
		if (vertices_ != null)
		{
			mesh_.vertices = vertices_;
			if (smoothNormals)
			{
				mesh_.normals = normals_;
			}
			else
			{
				mesh_.RecalculateNormals();
			}
			cap_mesh_.vertices = cap_vertices_;
			cap_mesh_.RecalculateNormals();
			CombineInstance[] array = new CombineInstance[2];
			array[0].mesh = mesh_;
			array[1].mesh = cap_mesh_;
			GetComponent<MeshFilter>().sharedMesh.CombineMeshes(array, mergeSubMeshes: true, useMatrices: false);
			GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(mesh_);
		UnityEngine.Object.Destroy(cap_mesh_);
		UnityEngine.Object.Destroy(GetComponent<MeshFilter>().mesh);
	}

	private void Update()
	{
	}

	protected Quaternion GetJointRotation(int joint)
	{
		if (joint <= 0)
		{
			return GetBoneRotation(joint);
		}
		if (joint >= 4)
		{
			return GetBoneRotation(joint - 1);
		}
		return Quaternion.Slerp(GetBoneRotation(joint - 1), GetBoneRotation(joint), 0.5f);
	}

	protected void InitJointVertices()
	{
		joint_vertices_ = new Vector3[sides];
		for (int i = 0; i < sides; i++)
		{
			float angle = startingAngle + (float)i * 360f / (float)sides;
			ref Vector3 reference = ref joint_vertices_[i];
			reference = Quaternion.AngleAxis(angle, -Vector3.forward) * Vector3.up;
		}
	}

	protected void UpdateMesh()
	{
		if (joint_vertices_ == null || joint_vertices_.Length != sides)
		{
			InitJointVertices();
		}
		if (normals_ == null || normals_.Length != 4 * sides * 4 || vertices_ == null || vertices_.Length != 4 * sides * 4)
		{
			InitMesh();
		}
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector = base.transform.InverseTransformPoint(GetJointPosition(i));
			Vector3 vector2 = base.transform.InverseTransformPoint(GetJointPosition(i + 1));
			Quaternion quaternion = Quaternion.Inverse(base.transform.rotation) * GetJointRotation(i);
			Quaternion quaternion2 = Quaternion.Inverse(base.transform.rotation) * GetJointRotation(i + 1);
			for (int j = 0; j < sides; j++)
			{
				int num2 = (j + 1) % sides;
				if (smoothNormals)
				{
					Vector3 vector3 = quaternion * joint_vertices_[j];
					Vector3 vector4 = quaternion * joint_vertices_[num2];
					ref Vector3 reference = ref normals_[num];
					reference = (normals_[num + 2] = vector3);
					ref Vector3 reference2 = ref normals_[num + 1];
					reference2 = (normals_[num + 3] = vector4);
				}
				Vector3 vector5 = quaternion * (widths[i] * joint_vertices_[j]);
				ref Vector3 reference3 = ref vertices_[num++];
				reference3 = vector + vector5;
				vector5 = quaternion * (widths[i] * joint_vertices_[num2]);
				ref Vector3 reference4 = ref vertices_[num++];
				reference4 = vector + vector5;
				vector5 = quaternion2 * (widths[i + 1] * joint_vertices_[j]);
				ref Vector3 reference5 = ref vertices_[num++];
				reference5 = vector2 + vector5;
				vector5 = quaternion2 * (widths[i + 1] * joint_vertices_[num2]);
				ref Vector3 reference6 = ref vertices_[num++];
				reference6 = vector2 + vector5;
			}
		}
	}

	protected void UpdateCapMesh()
	{
		Vector3 vector = base.transform.InverseTransformPoint(GetJointPosition(0));
		Vector3 vector2 = base.transform.InverseTransformPoint(GetJointPosition(2));
		Quaternion quaternion = Quaternion.Inverse(base.transform.rotation) * GetJointRotation(0);
		Quaternion quaternion2 = Quaternion.Inverse(base.transform.rotation) * GetJointRotation(2);
		if (cap_vertices_ == null || cap_vertices_.Length != 2 * sides)
		{
			InitCapsMesh();
		}
		for (int i = 0; i < sides; i++)
		{
			ref Vector3 reference = ref cap_vertices_[i];
			reference = vector + quaternion * (widths[0] * joint_vertices_[i]);
			ref Vector3 reference2 = ref cap_vertices_[sides + i];
			reference2 = vector2 + quaternion2 * (widths[2] * joint_vertices_[i]);
		}
	}

	protected void InitMesh()
	{
		mesh_ = new Mesh();
		mesh_.MarkDynamic();
		int num = 0;
		int num2 = 4 * sides * 4;
		vertices_ = new Vector3[num2];
		normals_ = new Vector3[num2];
		Vector2[] array = new Vector2[num2];
		int num3 = 0;
		int num4 = 6 * sides * 4;
		int[] array2 = new int[num4];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < sides; j++)
			{
				array2[num3++] = num;
				array2[num3++] = num + 2;
				array2[num3++] = num + 1;
				array2[num3++] = num + 2;
				array2[num3++] = num + 3;
				array2[num3++] = num + 1;
				ref Vector2 reference = ref array[num];
				reference = new Vector3(1f * (float)j / (float)sides, 1f * (float)i / 4f);
				ref Vector2 reference2 = ref array[num + 1];
				reference2 = new Vector3((1f + (float)j) / (float)sides, 1f * (float)i / 4f);
				ref Vector2 reference3 = ref array[num + 2];
				reference3 = new Vector3(1f * (float)j / (float)sides, (1f + (float)i) / 4f);
				ref Vector2 reference4 = ref array[num + 3];
				reference4 = new Vector3((1f + (float)j) / (float)sides, (1f + (float)i) / 4f);
				ref Vector3 reference5 = ref vertices_[num++];
				reference5 = new Vector3(0f, 0f, 0f);
				ref Vector3 reference6 = ref vertices_[num++];
				reference6 = new Vector3(0f, 0f, 0f);
				ref Vector3 reference7 = ref vertices_[num++];
				reference7 = new Vector3(0f, 0f, 0f);
				ref Vector3 reference8 = ref vertices_[num++];
				reference8 = new Vector3(0f, 0f, 0f);
			}
		}
		mesh_.vertices = vertices_;
		mesh_.normals = normals_;
		mesh_.uv = array;
		mesh_.triangles = array2;
	}

	protected void InitCapsMesh()
	{
		cap_mesh_ = new Mesh();
		cap_mesh_.MarkDynamic();
		cap_vertices_ = cap_mesh_.vertices;
		int num = 2 * sides;
		if (num != cap_vertices_.Length)
		{
			Array.Resize(ref cap_vertices_, num);
		}
		Vector2[] array = cap_mesh_.uv;
		if (array.Length != num)
		{
			Array.Resize(ref array, num);
		}
		int num2 = 0;
		int[] array2 = cap_mesh_.triangles;
		int num3 = 6 * (sides - 2);
		if (num3 != array2.Length)
		{
			Array.Resize(ref array2, num3);
		}
		for (int i = 0; i < sides; i++)
		{
			ref Vector3 reference = ref cap_vertices_[i];
			reference = new Vector3(0f, 0f, 0f);
			ref Vector3 reference2 = ref cap_vertices_[i + sides];
			reference2 = new Vector3(0f, 0f, 0f);
			ref Vector2 reference3 = ref array[i];
			reference3 = 0.5f * joint_vertices_[i];
			array[i] += new Vector2(0.5f, 0.5f);
			ref Vector2 reference4 = ref array[i + sides];
			reference4 = 0.5f * joint_vertices_[i];
			array[i + sides] += new Vector2(0.5f, 0.5f);
		}
		for (int j = 0; j < sides - 2; j++)
		{
			array2[num2++] = 0;
			array2[num2++] = j + 1;
			array2[num2++] = j + 2;
			array2[num2++] = sides;
			array2[num2++] = sides + j + 2;
			array2[num2++] = sides + j + 1;
		}
		cap_mesh_.vertices = cap_vertices_;
		cap_mesh_.uv = array;
		cap_mesh_.triangles = array2;
	}
}
