using System.Collections.Generic;
using UnityEngine;

internal class OVRCompositionUtil
{
	public static void SafeDestroy(GameObject obj)
	{
		if (Application.isPlaying)
		{
			Object.Destroy(obj);
		}
		else
		{
			Object.DestroyImmediate(obj);
		}
	}

	public static void SafeDestroy(ref GameObject obj)
	{
		SafeDestroy(obj);
		obj = null;
	}

	public static OVRPlugin.CameraDevice ConvertCameraDevice(OVRManager.CameraDevice cameraDevice)
	{
		return cameraDevice switch
		{
			OVRManager.CameraDevice.WebCamera0 => OVRPlugin.CameraDevice.WebCamera0, 
			OVRManager.CameraDevice.WebCamera1 => OVRPlugin.CameraDevice.WebCamera1, 
			OVRManager.CameraDevice.ZEDCamera => OVRPlugin.CameraDevice.ZEDCamera, 
			_ => OVRPlugin.CameraDevice.None, 
		};
	}

	public static OVRBoundary.BoundaryType ToBoundaryType(OVRManager.VirtualGreenScreenType type)
	{
		switch (type)
		{
			case OVRManager.VirtualGreenScreenType.OuterBoundary:
				return OVRBoundary.BoundaryType.OuterBoundary;
			case OVRManager.VirtualGreenScreenType.PlayArea:
				return OVRBoundary.BoundaryType.PlayArea;
			default:
				Debug.LogWarning("Unmatched VirtualGreenScreenType");
				return OVRBoundary.BoundaryType.OuterBoundary;
		}
	}

	public static Vector3 GetWorldPosition(Vector3 trackingSpacePosition)
	{
		OVRPose trackingSpacePose = default(OVRPose);
		trackingSpacePose.position = trackingSpacePosition;
		trackingSpacePose.orientation = Quaternion.identity;
		return OVRExtensions.ToWorldSpacePose(trackingSpacePose).position;
	}

	public static float GetMaximumBoundaryDistance(Camera camera, OVRBoundary.BoundaryType boundaryType)
	{
		if (!OVRManager.boundary.GetConfigured())
		{
			return float.MaxValue;
		}
		Vector3[] geometry = OVRManager.boundary.GetGeometry(boundaryType);
		if (geometry.Length == 0)
		{
			return float.MaxValue;
		}
		float num = float.MinValue;
		Vector3[] array = geometry;
		foreach (Vector3 trackingSpacePosition in array)
		{
			Vector3 worldPosition = GetWorldPosition(trackingSpacePosition);
			float num2 = Vector3.Dot(camera.transform.forward, worldPosition);
			if (num < num2)
			{
				num = num2;
			}
		}
		return num;
	}

	public static Mesh BuildBoundaryMesh(OVRBoundary.BoundaryType boundaryType, float topY, float bottomY)
	{
		if (!OVRManager.boundary.GetConfigured())
		{
			return null;
		}
		List<Vector3> list = new List<Vector3>(OVRManager.boundary.GetGeometry(boundaryType));
		if (list.Count == 0)
		{
			return null;
		}
		list.Add(list[0]);
		int count = list.Count;
		Vector3[] array = new Vector3[count * 2];
		Vector2[] array2 = new Vector2[count * 2];
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = list[i];
			ref Vector3 reference = ref array[i];
			reference = new Vector3(vector.x, bottomY, vector.z);
			ref Vector3 reference2 = ref array[i + count];
			reference2 = new Vector3(vector.x, topY, vector.z);
			ref Vector2 reference3 = ref array2[i];
			reference3 = new Vector2((float)i / (float)(count - 1), 0f);
			ref Vector2 reference4 = ref array2[i + count];
			reference4 = new Vector2(array2[i].x, 1f);
		}
		int[] array3 = new int[(count - 1) * 2 * 3];
		for (int j = 0; j < count - 1; j++)
		{
			array3[j * 6] = j;
			array3[j * 6 + 1] = j + count;
			array3[j * 6 + 2] = j + 1 + count;
			array3[j * 6 + 3] = j;
			array3[j * 6 + 4] = j + 1 + count;
			array3[j * 6 + 5] = j + 1;
		}
		Mesh mesh = new Mesh();
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		return mesh;
	}
}
