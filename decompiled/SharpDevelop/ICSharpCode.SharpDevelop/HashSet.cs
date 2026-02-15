using System;
using System.Collections;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop;

public sealed class HashSet<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable, ICloneable where T : class
{
	private Dictionary<T, object> _dict;

	private bool _copyOnWrite;

	public int Count => _dict.Count;

	bool ICollection<T>.IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => null;

	public HashSet()
	{
		_dict = new Dictionary<T, object>();
	}

	public HashSet(HashSet<T> existingSet)
	{
		existingSet._copyOnWrite = true;
		_copyOnWrite = true;
		_dict = existingSet._dict;
	}

	public bool Add(T item)
	{
		if (item == null)
		{
			return false;
		}
		if (_dict.ContainsKey(item))
		{
			return false;
		}
		CopyIfRequired();
		_dict.Add(item, null);
		return true;
	}

	public void AddRange(IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			Add(item);
		}
	}

	private void CopyIfRequired()
	{
		if (_copyOnWrite)
		{
			_copyOnWrite = false;
			_dict = new Dictionary<T, object>(_dict);
		}
	}

	public void Clear()
	{
		_dict.Clear();
	}

	public bool Contains(T item)
	{
		if (item == null)
		{
			return false;
		}
		return _dict.ContainsKey(item);
	}

	public bool Remove(T item)
	{
		if (item == null)
		{
			return false;
		}
		CopyIfRequired();
		if (_dict.Remove(item))
		{
			return true;
		}
		return false;
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		_dict.Keys.CopyTo(array, arrayIndex);
	}

	void ICollection<T>.Add(T item)
	{
		Add(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _dict.Keys.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _dict.Keys.GetEnumerator();
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_dict).CopyTo(array, index);
	}

	public HashSet<T> Clone()
	{
		return new HashSet<T>(this);
	}

	object ICloneable.Clone()
	{
		return Clone();
	}
}
