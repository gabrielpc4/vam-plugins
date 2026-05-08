using System.Collections.Generic;

namespace Battlehub.RTSaveLoad;

public static class DictionaryExt
{
	public static U Get<T, U>(this Dictionary<T, U> dict, T key)
	{
		if (dict.TryGetValue(key, out var value))
		{
			return value;
		}
		return default(U);
	}
}
