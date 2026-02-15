using System;

namespace ICSharpCode.SharpDevelop.Project;

[Flags]
public enum FileNodeStatus
{
	None = 1,
	InProject = 2,
	Missing = 4,
	BehindFile = 8,
	Link = 0x10
}
