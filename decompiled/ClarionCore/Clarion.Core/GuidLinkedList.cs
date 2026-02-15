using System;
using System.Collections.Generic;

namespace Clarion.Core;

public class GuidLinkedList<T>
{
	protected static GuidLinkedList<T> instance;

	private Dictionary<Guid, T> list;

	public static GuidLinkedList<T> Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GuidLinkedList<T>();
			}
			return instance;
		}
	}

	protected GuidLinkedList()
	{
		list = new Dictionary<Guid, T>();
	}

	public virtual T Object(Guid id)
	{
		list.TryGetValue(id, out var value);
		return value;
	}

	public void AddObject(Guid id, T obj)
	{
		list.Add(id, obj);
	}

	protected virtual void Removing(T item)
	{
	}

	public void RemoveObject(Guid id)
	{
		try
		{
			T item = list[id];
			Removing(item);
			list.Remove(id);
		}
		catch (Exception)
		{
		}
	}
}
