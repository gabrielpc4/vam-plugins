using System;

namespace Leap.Unity;

public abstract class MultiTypedList
{
	[Serializable]
	public struct Key
	{
		public int id;

		public int index;
	}
}
