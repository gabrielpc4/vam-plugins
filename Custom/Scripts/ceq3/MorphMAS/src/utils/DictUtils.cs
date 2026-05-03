using System.Collections.Generic;

namespace Utils.DictUtils
{
    public class DictUtils
    {
        public static Dictionary<TKey, TValue> CloneDict<TKey, TValue>(Dictionary<TKey, TValue> original)
        {
            Dictionary<TKey, TValue> clone = new Dictionary<TKey, TValue>();
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                clone.Add(entry.Key, entry.Value);
            }
            return clone;
        }

        public static Dictionary<TKey1, Dictionary<TKey2, TValue>> CloneDictDeep<TKey1, TKey2, TValue>(Dictionary<TKey1, Dictionary<TKey2, TValue>> original)
        {
            Dictionary<TKey1, Dictionary<TKey2, TValue>> clone = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
            foreach (KeyValuePair<TKey1, Dictionary<TKey2, TValue>> entry in original)
            {
                clone.Add(entry.Key, CloneDict(entry.Value));
            }
            return clone;
        }
    }
}
