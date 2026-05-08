using UnityEngine;

namespace MeshVR;

public class PerfMonCamera : MonoBehaviour
{
	public static float renderStartTime;

	private void OnPreCull()
	{
		renderStartTime = GlobalStopwatch.GetElapsedMilliseconds();
	}
}
