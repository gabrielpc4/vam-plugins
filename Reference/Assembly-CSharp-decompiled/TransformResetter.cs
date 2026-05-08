using System.Collections;
using UnityEngine;

public class TransformResetter : PhysicsSimulator
{
	public Vector3 startingLocalPosition;

	public Quaternion startingLocalRotation;

	public int numResetFrames = 10;

	protected int resetCount;

	protected bool wasInit;

	protected override void SyncPauseSimulation()
	{
		base.SyncPauseSimulation();
		Init();
		ResetTransform();
	}

	private IEnumerator Reset()
	{
		resetCount = 0;
		while (resetCount < numResetFrames)
		{
			ResetTransform();
			resetCount++;
			yield return null;
		}
	}

	protected void Init()
	{
		if (!wasInit)
		{
			wasInit = true;
			startingLocalPosition = base.transform.localPosition;
			startingLocalRotation = base.transform.localRotation;
		}
	}

	private void Awake()
	{
		Init();
	}

	protected void ResetTransform()
	{
		base.transform.localPosition = startingLocalPosition;
		base.transform.localRotation = startingLocalRotation;
	}
}
