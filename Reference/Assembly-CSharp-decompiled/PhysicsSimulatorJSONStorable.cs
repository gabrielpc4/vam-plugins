using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PhysicsSimulatorJSONStorable : ScaleChangeReceiverJSONStorable
{
	protected bool _pauseSimulation;

	protected bool _collisionEnabled;

	protected bool _useInterpolation;

	protected int _solverIterations = 15;

	protected List<AsyncFlag> waitResumeSimulationFlags;

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
				SyncPauseSimulation();
			}
		}
	}

	public bool collisionEnabled
	{
		get
		{
			return _collisionEnabled;
		}
		set
		{
			if (_collisionEnabled != value)
			{
				_collisionEnabled = value;
				SyncCollisionEnabled();
			}
		}
	}

	public bool useInterpolation
	{
		get
		{
			return _useInterpolation;
		}
		set
		{
			if (_useInterpolation != value)
			{
				_useInterpolation = value;
				SyncUseInterpolation();
			}
		}
	}

	public int solverIterations
	{
		get
		{
			return _solverIterations;
		}
		set
		{
			if (_solverIterations != value)
			{
				_solverIterations = value;
				SyncSolverIterations();
			}
		}
	}

	protected virtual void SyncPauseSimulation()
	{
		SyncCollisionEnabled();
	}

	protected virtual void SyncCollisionEnabled()
	{
	}

	protected virtual void SyncUseInterpolation()
	{
	}

	protected virtual void SyncSolverIterations()
	{
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

	public bool IsSimulationPaused()
	{
		return _pauseSimulation;
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

	protected virtual void Update()
	{
		if (Application.isPlaying)
		{
			CheckResumeSimulation();
		}
	}
}
