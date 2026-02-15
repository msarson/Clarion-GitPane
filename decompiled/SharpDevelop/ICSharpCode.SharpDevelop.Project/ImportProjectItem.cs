using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class ImportProjectItem : ProjectItem
{
	public ImportProjectItem(IProject project, string include)
		: base(project, ItemType.Import, include)
	{
	}

	internal ImportProjectItem(IProject project, BuildItem buildItem)
		: base(project, buildItem)
	{
	}
}
