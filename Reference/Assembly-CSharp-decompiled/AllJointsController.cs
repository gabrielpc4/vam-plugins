using UnityEngine;
using UnityEngine.UI;

public class AllJointsController : MonoBehaviour
{
	public FreeControllerV3[] freeControllers;

	public FreeControllerV3[] keyControllers;

	public FreeControllerV3[] keyControllersAlt;

	public Slider springPercentSlider;

	public Slider damperPercentSlider;

	public void SetOnlyKeyJointsOn()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
		if (keyControllers != null)
		{
			FreeControllerV3[] array2 = keyControllers;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				freeControllerV2.currentPositionState = FreeControllerV3.PositionState.On;
				freeControllerV2.currentRotationState = FreeControllerV3.RotationState.On;
			}
		}
	}

	public void SetOnlyKeyJointsComply()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
		if (keyControllers != null)
		{
			FreeControllerV3[] array2 = keyControllers;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				freeControllerV2.currentPositionState = FreeControllerV3.PositionState.Comply;
				freeControllerV2.currentRotationState = FreeControllerV3.RotationState.Comply;
			}
		}
	}

	public void SetOnlyKeyAltJointsOn()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
		if (keyControllers != null)
		{
			FreeControllerV3[] array2 = keyControllersAlt;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				freeControllerV2.currentPositionState = FreeControllerV3.PositionState.On;
				freeControllerV2.currentRotationState = FreeControllerV3.RotationState.On;
			}
		}
	}

	public void SetOnlyKeyAltJointsComply()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
		if (keyControllers != null)
		{
			FreeControllerV3[] array2 = keyControllersAlt;
			foreach (FreeControllerV3 freeControllerV2 in array2)
			{
				freeControllerV2.currentPositionState = FreeControllerV3.PositionState.Comply;
				freeControllerV2.currentRotationState = FreeControllerV3.RotationState.Comply;
			}
		}
	}

	public void SetAllJointsControlPositionAndRotation()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.On;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.On;
			}
		}
	}

	public void SetAllJointsComply()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Comply;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Comply;
			}
		}
	}

	public void SetAllJointsControlPosition()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.On;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
	}

	public void SetAllJointsControlOff()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.currentPositionState = FreeControllerV3.PositionState.Off;
				freeControllerV.currentRotationState = FreeControllerV3.RotationState.Off;
			}
		}
	}

	public void SetAllJointsMaxHoldSpring()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionSpringMax();
				freeControllerV.SetHoldRotationSpringMax();
			}
		}
	}

	public void SetAllJointsPercentHoldSpring()
	{
		if (freeControllers != null && springPercentSlider != null)
		{
			float value = springPercentSlider.value;
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionSpringPercent(value);
				freeControllerV.SetHoldRotationSpringPercent(value);
			}
		}
	}

	public void SetAllJointsMinHoldSpring()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionSpringMin();
				freeControllerV.SetHoldRotationSpringMin();
			}
		}
	}

	public void SetAllJointsMaxHoldDamper()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionDamperMax();
				freeControllerV.SetHoldRotationDamperMax();
			}
		}
	}

	public void SetAllJointsPercentHoldDamper()
	{
		if (freeControllers != null && damperPercentSlider != null)
		{
			float value = damperPercentSlider.value;
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionDamperPercent(value);
				freeControllerV.SetHoldRotationDamperPercent(value);
			}
		}
	}

	public void SetAllJointsMinHoldDamper()
	{
		if (freeControllers != null)
		{
			FreeControllerV3[] array = freeControllers;
			foreach (FreeControllerV3 freeControllerV in array)
			{
				freeControllerV.SetHoldPositionDamperMin();
				freeControllerV.SetHoldRotationDamperMin();
			}
		}
	}
}
