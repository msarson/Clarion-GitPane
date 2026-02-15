using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Refactoring;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class DerivedTypesNode : ExtFolderNode, IDerivedTypesNode
{
	private IProject project;

	private IClass c;

	public override bool Visible
	{
		get
		{
			ClassBrowserFilter filter = ClassBrowserPad.Instance.Filter;
			return (filter & ClassBrowserFilter.ShowBaseAndDerivedTypes) != 0;
		}
	}

	public IProject Project => project;

	public DerivedTypesNode(IProject project, IClass c)
	{
		sortOrder = 1;
		this.project = project;
		this.c = c;
		base.Text = ResourceService.GetString("MainWindow.Windows.ClassBrowser.DerivedTypes");
		base.OpenedIcon = "ProjectBrowser.Folder.Open";
		base.ClosedIcon = "ProjectBrowser.Folder.Closed";
		base.Nodes.Add(new TreeNode(ResourceService.GetString("ICSharpCode.SharpDevelop.Gui.Pads.ClassScout.LoadingNode")));
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Nodes.Clear();
		List<IProjectContent> list = new List<IProjectContent>(1);
		list.Add(null);
		if (ProjectService.OpenSolution != null)
		{
			foreach (IProject project in ProjectService.OpenSolution.Projects)
			{
				IProjectContent projectContent = ParserService.GetProjectContent(project);
				if (projectContent == null)
				{
					continue;
				}
				list[0] = projectContent;
				foreach (IClass item in RefactoringService.FindDerivedClasses(c, list, directDerivationOnly: true))
				{
					new ClassNode(project, item).AddTo(this);
				}
			}
		}
		if (base.Nodes.Count == 0)
		{
			SetIcon(base.ClosedIcon);
			string text = (base.ClosedIcon = null);
			base.OpenedIcon = text;
		}
	}
}
