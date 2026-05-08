using System;
using System.Collections;
using System.Collections.Generic;

namespace Leap.Unity;

public abstract class MultiTypedList<BaseType> : MultiTypedList, IList<BaseType>, ICollection<BaseType>, IEnumerable<BaseType>, IEnumerable
{
	public struct Enumerator : IEnumerator<BaseType>, IEnumerator, IDisposable
	{
		private MultiTypedList<BaseType> _list;

		private int _index;

		private BaseType _current;

		object IEnumerator.Current
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public BaseType Current => _current;

		public Enumerator(MultiTypedList<BaseType> list)
		{
			_list = list;
			_index = 0;
			_current = default(BaseType);
		}

		public void Dispose()
		{
			_list = null;
			_current = default(BaseType);
		}

		public bool MoveNext()
		{
			if (_index >= _list.Count)
			{
				return false;
			}
			_current = _list[_index++];
			return true;
		}

		public void Reset()
		{
			_index = 0;
			_current = default(BaseType);
		}
	}

	public abstract int Count { get; }

	public bool IsReadOnly => false;

	public abstract BaseType this[int index] { get; set; }

	public abstract void Add(BaseType obj);

	public abstract void Clear();

	public bool Contains(BaseType item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(BaseType[] array, int arrayIndex)
	{
		for (int i = 0; i < Count; i++)
		{
			array[i + arrayIndex] = this[i];
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public int IndexOf(BaseType item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].Equals(item))
			{
				return i;
			}
		}
		return -1;
	}

	public abstract void Insert(int index, BaseType item);

	public bool Remove(BaseType item)
	{
		int num = IndexOf(item);
		if (num >= 0)
		{
			RemoveAt(num);
			return true;
		}
		return false;
	}

	public abstract void RemoveAt(int index);

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<BaseType> IEnumerable<BaseType>.GetEnumerator()
	{
		return new Enumerator(this);
	}
}
