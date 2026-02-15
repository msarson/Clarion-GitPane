using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator.Commands;

public abstract class ApplicationsInSolutionMenuCommand : AbstractMenuCommand
{
	protected Application app;

	protected AbstractProjectBrowserTreeNode appNode;

	public override bool IsEnabled
	{
		get
		{
			appNode = ProjectBrowserPad.Instance.SelectedNode;
			if (appNode != null)
			{
				string directoryName = Path.GetDirectoryName(ProjectService.OpenSolution.FileName);
				string fileName = Path.Combine(directoryName, ((TreeNode)(object)appNode).Text);
				app = ApplicationService.FindApplication(fileName);
				if (app != null)
				{
					return true;
				}
			}
			app = null;
			return false;
		}
		set
		{
		}
	}

	protected abstract void ExecuteApplicationService();

	public override void Run()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		if (((AbstractMenuCommand)this).IsEnabled && app != null && !app.IsBusy)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(new Action(ExecuteApplicationService));
		}
	}
}
