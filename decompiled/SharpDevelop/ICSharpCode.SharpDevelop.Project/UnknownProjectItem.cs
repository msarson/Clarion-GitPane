using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class UnknownProjectItem : ProjectItem
{
	internal UnknownProjectItem(IProject project, BuildItem buildItem)
		: base(project, buildItem)
	{
	}

	internal UnknownProjectItem(IProject project, string itemType, string include)
		: base(project, new ItemType(itemType), include)
	{
	}
}
