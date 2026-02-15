using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.Commands;

public class GenerateApplicationFromProjectInSolutionMenuCommand : AbstractMenuCommand
{
	protected Application app;

	protected AbstractProjectBrowserTreeNode prjNode;

	public override bool IsEnabled
	{
		get
		{
			prjNode = ProjectBrowserPad.Instance.SelectedNode;
			if (prjNode != null && prjNode.Project != null)
			{
				app = ApplicationService.GetAppFromIProject(prjNode.Project);
				if (app != null)
				{
					return true;
				}
			}
			app = null;
			prjNode = null;
			return false;
		}
		set
		{
		}
	}

	public override void Run()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (((AbstractMenuCommand)this).IsEnabled && app != null && !app.IsBusy)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(new Action(ExecuteApplicationService));
		}
	}

	protected void ExecuteApplicationService()
	{
		ApplicationService.GenerationEnded += ApplicationService_GenerationEnded;
		ApplicationService.GenerateApplication(app, GenerationMode.Off, GenerationMode.Off);
	}

	private void ApplicationService_GenerationEnded(object sender, GenerationEndEventArgs e)
	{
		if (prjNode != null && prjNode.Project != null)
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.SelectFile(prjNode.Project.FileName);
		}
	}
}
