using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project.Dialogs;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ViewProjectOptions : AbstractMenuCommand
{
	public override void Run()
	{
		ShowProjectOptions(ProjectService.CurrentProject);
	}

	public static void ShowProjectOptions(IProject project)
	{
		if (project == null)
		{
			return;
		}
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			if (item is ProjectOptionsView projectOptionsView && projectOptionsView.Project == project)
			{
				projectOptionsView.WorkbenchWindow.SelectWindow();
				return;
			}
		}
		try
		{
			AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/BackendBindings/ProjectOptions/" + project.Language);
			ProjectOptionsView content = new ProjectOptionsView(treeNode, project);
			WorkbenchSingleton.Workbench.ShowView(content);
		}
		catch (TreePathNotFoundException)
		{
			MessageService.ShowError("${res:Dialog.ProjectOptions.NoPanelsInstalledForProject}");
		}
	}
}
