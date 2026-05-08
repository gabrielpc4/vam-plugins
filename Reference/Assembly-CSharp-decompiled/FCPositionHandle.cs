using Battlehub.RTHandles;
using UnityEngine;

public class FCPositionHandle : PositionHandle
{
	public FreeControllerV3 controller;

	public FCRotationHandle priorityHandle;

	private Rigidbody rb;

	protected bool isDragging;

	public bool HasSelectedAxis => SelectedAxis != RuntimeHandleAxis.None;

	protected override bool OnBeginDrag()
	{
		if (controller != null && base.OnBeginDrag() && controller.canGrabPosition)
		{
			isDragging = true;
			controller.SelectLinkToRigidbody(rb, FreeControllerV3.SelectLinkState.PositionAndRotation);
			return true;
		}
		return false;
	}

	protected override void OnDrop()
	{
		base.OnDrop();
		if (isDragging)
		{
			isDragging = false;
			controller.RestorePreLinkState();
		}
	}

	public void ForceDeselectAxis()
	{
		SelectedAxis = RuntimeHandleAxis.None;
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	protected override void UpdateOverride()
	{
		base.UpdateOverride();
		if (priorityHandle != null)
		{
			if (isDragging)
			{
				priorityHandle.ForceDeselectAxis();
			}
			else if (priorityHandle.HasSelectedAxis)
			{
				SelectedAxis = RuntimeHandleAxis.None;
			}
		}
		if (!isDragging && controller != null)
		{
			base.transform.position = controller.transform.position;
		}
	}

	protected override void DrawOverride()
	{
		if (controller != null && controller.canGrabPosition)
		{
			base.DrawOverride();
		}
	}
}
