using System;
using System.Threading;

namespace UnityThreading;

public static class WaitOneExtension
{
	public static bool InterWaitOne(this ManualResetEvent that, int ms)
	{
		return that.WaitOne(ms, exitContext: false);
	}

	public static bool InterWaitOne(this ManualResetEvent that, TimeSpan duration)
	{
		return that.WaitOne(duration, exitContext: false);
	}
}
