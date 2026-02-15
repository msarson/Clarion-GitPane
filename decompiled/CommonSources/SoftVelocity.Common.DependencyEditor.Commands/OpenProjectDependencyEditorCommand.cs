using ICSharpCode.Core;

namespace SoftVelocity.Common.DependencyEditor.Commands;

public class OpenProjectDependencyEditorCommand : AbstractMenuCommand
{
	public override void Run()
	{
		ProjectDependencyEditor.Open();
	}

	public static void OpenDependencyEditor()
	{
		OpenProjectDependencyEditorCommand openProjectDependencyEditorCommand = new OpenProjectDependencyEditorCommand();
		((AbstractCommand)openProjectDependencyEditorCommand).Run();
	}
}
