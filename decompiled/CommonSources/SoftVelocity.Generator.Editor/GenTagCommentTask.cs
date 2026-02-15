using System.IO;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Generator.Editor;

public class GenTagCommentTask : Task
{
	private string appFileName;

	public GenTagCommentTask(string appFileName, string fileName, string description, int column, int line, TaskType type)
		: base(fileName, description, column, line, type)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		this.appFileName = appFileName;
	}

	public override void JumpToPosition()
	{
		if (File.Exists(appFileName))
		{
			FileService.JumpToFilePosition(appFileName, ((Task)this).Line, ((Task)this).Column);
		}
	}
}
