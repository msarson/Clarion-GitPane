using System;

namespace ICSharpCode.SharpDevelop;

public struct Pair<A, B> : IEquatable<Pair<A, B>> where A : IEquatable<A> where B : IEquatable<B>
{
	public readonly A First;

	public readonly B Second;

	public Pair(A first, B second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		First = first;
		Second = second;
	}

	public override bool Equals(object obj)
	{
		if (obj is Pair<A, B>)
		{
			return Equals((Pair<A, B>)obj);
		}
		return false;
	}

	public bool Equals(Pair<A, B> other)
	{
		if (First.Equals(other.First))
		{
			return Second.Equals(other.Second);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 27 * First.GetHashCode() + Second.GetHashCode();
	}

	public static bool operator ==(Pair<A, B> lhs, Pair<A, B> rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(Pair<A, B> lhs, Pair<A, B> rhs)
	{
		return !lhs.Equals(rhs);
	}
}
