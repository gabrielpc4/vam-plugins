using System;
using System.Collections;
using System.Threading;

namespace UnityThreading;

public sealed class TickThread : ThreadBase
{
	private Action action;

	private int tickLengthInMilliseconds;

	private ManualResetEvent tickEvent = new ManualResetEvent(initialState: false);

	public TickThread(Action action, int tickLengthInMilliseconds)
		: this(action, tickLengthInMilliseconds, autoStartThread: true)
	{
	}

	public TickThread(Action action, int tickLengthInMilliseconds, bool autoStartThread)
		: base("TickThread", Dispatcher.CurrentNoThrow, autoStartThread: false)
	{
		this.tickLengthInMilliseconds = tickLengthInMilliseconds;
		this.action = action;
		if (autoStartThread)
		{
			Start();
		}
	}

	protected override IEnumerator Do()
	{
		while (!exitEvent.InterWaitOne(0))
		{
			action();
			if (WaitHandle.WaitAny(new WaitHandle[2] { exitEvent, tickEvent }, tickLengthInMilliseconds) == 0)
			{
				return null;
			}
		}
		return null;
	}
}
