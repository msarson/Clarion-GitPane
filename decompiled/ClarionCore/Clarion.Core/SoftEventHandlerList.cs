using System;
using System.Reflection;

namespace Clarion.Core;

public class SoftEventHandlerList<T> : GuidLinkedList<T> where T : SoftEventHandler
{
	public new static SoftEventHandlerList<T> Instance
	{
		get
		{
			if (GuidLinkedList<T>.instance == null)
			{
				GuidLinkedList<T>.instance = new SoftEventHandlerList<T>();
			}
			return (SoftEventHandlerList<T>)GuidLinkedList<T>.instance;
		}
	}

	private T NewT()
	{
		ConstructorInfo constructor = typeof(T).GetConstructor(Type.EmptyTypes);
		return (T)constructor.Invoke(null);
	}

	public override T Object(Guid id)
	{
		T val = base.Object(id);
		if (val == null)
		{
			val = NewT();
			AddObject(id, val);
		}
		return val;
	}

	public void AddObject(Guid id)
	{
		Object(id);
	}

	protected override void Removing(T item)
	{
		item.Detach();
	}
}
