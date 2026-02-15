using System;
using System.Collections;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop;

public static class Linq
{
	public static IEnumerable<S> Select<T, S>(IEnumerable<T> input, Converter<T, S> converter)
	{
		foreach (T element in input)
		{
			yield return converter(element);
		}
	}

	public static IEnumerable<T> Where<T>(IEnumerable<T> input, Predicate<T> filter)
	{
		foreach (T element in input)
		{
			if (filter(element))
			{
				yield return element;
			}
		}
	}

	public static IEnumerable<T> OfType<T>(IEnumerable input)
	{
		foreach (object element in input)
		{
			if (element is T)
			{
				yield return (T)element;
			}
		}
	}

	public static IEnumerable<T> CastTo<T>(IEnumerable input)
	{
		foreach (object element in input)
		{
			yield return (T)element;
		}
	}

	public static T Find<T>(IEnumerable<T> input, Predicate<T> filter)
	{
		foreach (T item in input)
		{
			if (filter(item))
			{
				return item;
			}
		}
		return default(T);
	}

	public static bool Exists<T>(IEnumerable<T> input, Predicate<T> filter)
	{
		foreach (T item in input)
		{
			if (filter(item))
			{
				return true;
			}
		}
		return false;
	}

	public static List<T> ToList<T>(IEnumerable<T> input)
	{
		return new List<T>(input);
	}

	public static T[] ToArray<T>(IEnumerable<T> input)
	{
		if (input is ICollection<T>)
		{
			ICollection<T> collection = (ICollection<T>)input;
			T[] array = new T[collection.Count];
			collection.CopyTo(array, 0);
			return array;
		}
		return new List<T>(input).ToArray();
	}

	public static int Count<T>(IEnumerable<T> input)
	{
		if (input is ICollection<T>)
		{
			return ((ICollection<T>)input).Count;
		}
		int num = 0;
		using IEnumerator<T> enumerator = input.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num++;
		}
		return num;
	}

	public static IEnumerable<T> Concat<T>(IEnumerable<T> input1, IEnumerable<T> input2)
	{
		foreach (T item in input1)
		{
			yield return item;
		}
		foreach (T item2 in input2)
		{
			yield return item2;
		}
	}

	public static IEnumerable<T> Concat<T>(IEnumerable<IEnumerable<T>> inputs)
	{
		foreach (IEnumerable<T> input in inputs)
		{
			foreach (T item in input)
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<T> Distinct<T>(IEnumerable<T> input)
	{
		Dictionary<T, object> elements = new Dictionary<T, object>();
		foreach (T element in input)
		{
			if (!elements.ContainsKey(element))
			{
				elements.Add(element, null);
				yield return element;
			}
		}
	}
}
