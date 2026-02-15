using System;

namespace ICSharpCode.SharpDevelop.Project;

public struct BuildTarget : IEquatable<BuildTarget>, IComparable<BuildTarget>
{
	public static readonly BuildTarget Build = new BuildTarget("Build");

	public static readonly BuildTarget Rebuild = new BuildTarget("Rebuild");

	public static readonly BuildTarget Clean = new BuildTarget("Clean");

	public static readonly BuildTarget ResolveComReferences = new BuildTarget("ResolveComReferences");

	private readonly string targetName;

	public string TargetName => targetName;

	public BuildTarget(string targetName)
	{
		if (targetName == null)
		{
			throw new ArgumentNullException("targetName");
		}
		this.targetName = targetName;
	}

	public override string ToString()
	{
		return targetName;
	}

	public override bool Equals(object obj)
	{
		if (obj is BuildTarget)
		{
			return Equals((BuildTarget)obj);
		}
		return false;
	}

	public bool Equals(BuildTarget other)
	{
		return targetName == other.targetName;
	}

	public override int GetHashCode()
	{
		return targetName.GetHashCode();
	}

	public static bool operator ==(BuildTarget lhs, BuildTarget rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(BuildTarget lhs, BuildTarget rhs)
	{
		return !lhs.Equals(rhs);
	}

	public int CompareTo(BuildTarget other)
	{
		return targetName.CompareTo(other.targetName);
	}
}
