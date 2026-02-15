using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Commands;

public class OpenRecentProject : AbstractRecentOpenCommand
{
	protected override bool DoOpen()
	{
		return FileUtility.ObservedLoad(ProjectService.LoadSolution, base.FileDescription.FileName) == FileOperationResult.OK;
	}
}
