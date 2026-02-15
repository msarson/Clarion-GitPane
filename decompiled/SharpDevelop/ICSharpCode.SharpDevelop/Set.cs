using System.Collections;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop;

public sealed class Set<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	private SortedDictionary<T, object> dict;

	public int Count => dict.Count;

	bool ICollection<T>.IsReadOnly => true;

	public Set()
	{
		dict = new SortedDictionary<T, object>();
	}

	public Set(IEnumerable<T> list)
		: this()
	{
		AddRange(list);
	}

	public Set(params T[] list)
		: this()
	{
		AddRange(list);
	}

	public Set(IComparer<T> comparer)
	{
		dict = new SortedDictionary<T, object>(comparer);
	}

	public Set(IEnumerable<T> list, IComparer<T> comparer)
		: this(comparer)
	{
		AddRange(list);
	}

	public void Add(T element)
	{
		dict[element] = null;
	}

	public void AddRange(IEnumerable<T> elements)
	{
		foreach (T element in elements)
		{
			Add(element);
		}
	}

	public bool Contains(T element)
	{
		return dict.ContainsKey(element);
	}

	public bool Remove(T element)
	{
		return dict.Remove(element);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return dict.Keys.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		dict.Clear();
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		dict.Keys.CopyTo(array, arrayIndex);
	}

	public T[] ToArray()
	{
		T[] array = new T[dict.Count];
		dict.Keys.CopyTo(array, 0);
		return array;
	}

	public ReadOnlyCollectionWrapper<T> AsReadOnly()
	{
		return new ReadOnlyCollectionWrapper<T>(dict.Keys);
	}
}
