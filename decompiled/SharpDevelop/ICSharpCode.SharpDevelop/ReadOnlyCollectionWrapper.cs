using System;
using System.Collections;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop;

public sealed class ReadOnlyCollectionWrapper<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	private readonly ICollection<T> c;

	public int Count => c.Count;

	public bool IsReadOnly => true;

	public ReadOnlyCollectionWrapper(ICollection<T> c)
	{
		if (c == null)
		{
			throw new ArgumentNullException("c");
		}
		this.c = c;
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	public bool Contains(T item)
	{
		return c.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		c.CopyTo(array, arrayIndex);
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return c.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)c).GetEnumerator();
	}
}
