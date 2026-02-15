using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ReferenceFolderNode : ExtFolderNode
{
	private IProject project;

	public IProject Project => project;

	public override bool Visible
	{
		get
		{
			ClassBrowserFilter filter = ClassBrowserPad.Instance.Filter;
			return (filter & ClassBrowserFilter.ShowProjectReferences) != 0;
		}
	}

	public ReferenceFolderNode(IProject project)
	{
		sortOrder = -1;
		this.project = project;
		base.Text = ResourceService.GetString("ICSharpCode.SharpDevelop.Commands.ProjectBrowser.ReferencesNodeText");
		base.OpenedIcon = "ProjectBrowser.ReferenceFolder.Open";
		base.ClosedIcon = "ProjectBrowser.ReferenceFolder.Closed";
		base.Nodes.Add(new TreeNode(ResourceService.GetString("ICSharpCode.SharpDevelop.Gui.Pads.ClassScout.LoadingNode")));
	}

	protected override void Initialize()
	{
		base.Initialize();
		UpdateReferenceNodes();
	}

	public virtual void UpdateReferenceNodes()
	{
		base.Nodes.Clear();
		foreach (ProjectItem item in project.Items)
		{
			if (item.ItemType == ItemType.Reference)
			{
				new ReferenceNode((ReferenceProjectItem)item).AddTo(this);
			}
		}
	}
}
