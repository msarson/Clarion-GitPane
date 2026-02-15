using ICSharpCode.Core;

namespace SoftVelocity.Common.DependencyEditor.Commands;

public class OpenProjectDependencyEditorFromSelectedCommand : AbstractMenuCommand
{
	public override void Run()
	{
		ProjectDependencyEditor.OpenFromSelected();
	}

	public static void OpenDependencyEditor()
	{
		OpenProjectDependencyEditorFromSelectedCommand openProjectDependencyEditorFromSelectedCommand = new OpenProjectDependencyEditorFromSelectedCommand();
		((AbstractCommand)openProjectDependencyEditorFromSelectedCommand).Run();
	}
}
