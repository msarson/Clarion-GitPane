using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator;

internal class AppItemNode : SolutionItemNode
{
	private string prjName;

	private IProject prj;

	public override IProject Project
	{
		get
		{
			if (prj == null)
			{
				prj = ProjectService.GetProject(prjName);
			}
			return prj;
		}
	}

	private AppItemNode(Solution solution, SolutionItem item, string prjName)
		: base(solution, item)
	{
		this.prjName = prjName;
		((AbstractProjectBrowserTreeNode)this).ToolbarAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ToolBar/AppFile";
		((ExtTreeNode)this).ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/AppFile";
		((ExtTreeNode)this).SetIcon("Generator.Application.File");
	}

	public static void AppItemNodeCreator(object caller, SolutionItemCreatingEventArgs args)
	{
		string absolutePath = FileUtility.GetAbsolutePath(args.Solution.Directory, args.Item.Name);
		IProject projectWithOutTypeHint = ProjectService.GetProjectWithOutTypeHint(absolutePath);
		string value = ((projectWithOutTypeHint == null) ? ApplicationService.ProjectFileName(absolutePath) : ((ISolutionFolder)projectWithOutTypeHint).Name);
		if (!string.IsNullOrEmpty(value))
		{
			args.Node = (SolutionItemNode)(object)new AppItemNode(args.Solution, args.Item, value);
		}
	}
}
