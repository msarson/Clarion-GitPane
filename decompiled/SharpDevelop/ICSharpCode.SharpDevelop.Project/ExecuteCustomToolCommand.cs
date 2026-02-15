using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class ExecuteCustomToolCommand : AbstractMenuCommand
{
	public override void Run()
	{
		if (Owner is FileNode { ProjectItem: FileProjectItem projectItem })
		{
			CustomToolsService.RunCustomTool(projectItem, showMessageBoxOnErrors: true);
		}
	}
}
