using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class AddReferenceToProject : AbstractMenuCommand
{
	public override void Run()
	{
		IProject project = ((Owner is AbstractProjectBrowserTreeNode abstractProjectBrowserTreeNode) ? abstractProjectBrowserTreeNode.Project : ProjectService.CurrentProject);
		if (project == null)
		{
			return;
		}
		LoggingService.Info("Show add reference dialog for " + project.FileName);
		using SelectReferenceDialog selectReferenceDialog = new SelectReferenceDialog(project);
		if (selectReferenceDialog.ShowDialog(WorkbenchSingleton.MainForm) != DialogResult.OK)
		{
			return;
		}
		foreach (ReferenceProjectItem referenceInformation in selectReferenceDialog.ReferenceInformations)
		{
			ProjectService.AddProjectItem(project, referenceInformation);
		}
		project.Save();
	}
}
