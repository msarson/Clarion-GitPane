using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Refactoring;

namespace SoftVelocity.Common.ClassBrowser;

public class ClaDerivedTypesNode : ExtFolderNode, IDerivedTypesNode
{
	private IProject project;

	private IClass c;

	public override bool Visible
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			ClassBrowserFilter filter = ClassBrowserPad.Instance.Filter;
			return (filter & 0x20) != 0;
		}
	}

	public IProject Project => project;

	public ClaDerivedTypesNode(IProject project, IClass c)
	{
		((ExtTreeNode)this).sortOrder = 1;
		this.project = project;
		this.c = c;
		((TreeNode)this).Text = "Derived types";
		((ExtFolderNode)this).OpenedIcon = "ProjectBrowser.Folder.Open";
		((ExtFolderNode)this).ClosedIcon = "ProjectBrowser.Folder.Closed";
		((TreeNode)this).Nodes.Add(new TreeNode(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Pads.ClassScout.LoadingNode}")));
	}

	protected override void Initialize()
	{
		((ExtTreeNode)this).Initialize();
		((TreeNode)this).Nodes.Clear();
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
				foreach (IClass item in RefactoringService.FindDerivedClasses(c, (IEnumerable<IProjectContent>)list, true))
				{
					((ExtTreeNode)new ClaClassNode(project, item)).AddTo((TreeNode)(object)this);
				}
			}
		}
		if (((TreeNode)this).Nodes.Count == 0)
		{
			((ExtTreeNode)this).SetIcon(((ExtFolderNode)this).ClosedIcon);
			string openedIcon = (((ExtFolderNode)this).ClosedIcon = null);
			((ExtFolderNode)this).OpenedIcon = openedIcon;
		}
	}
}
