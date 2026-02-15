using System.IO;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator;

internal class ApplicationBuildErrorTask : Task
{
	private Application app;

	private BuildError error;

	internal ApplicationBuildErrorTask(Application app, BuildError error)
		: base(error)
	{
		this.app = app;
		this.error = error;
	}

	public override void JumpToPosition()
	{
		try
		{
			if (!app.EditError(error) && File.Exists(((Task)this).FileName))
			{
				FileService.JumpToFilePosition(((Task)this).FileName, ((Task)this).Line, ((Task)this).Column);
			}
		}
		catch
		{
		}
	}
}
