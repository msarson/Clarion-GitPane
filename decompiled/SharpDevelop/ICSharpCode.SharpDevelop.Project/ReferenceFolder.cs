using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

public class ReferenceFolder : CustomFolderNode
{
	private IProject project;

	public ReferenceFolder(IProject project)
	{
		sortOrder = 0;
		this.project = project;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/ReferenceFolderNode";
		base.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.ProjectBrowser.ReferencesNodeText}");
		base.OpenedImage = "ProjectBrowser.ReferenceFolder.Open";
		base.ClosedImage = "ProjectBrowser.ReferenceFolder.Closed";
		foreach (ProjectItem item in project.Items)
		{
			if (item is ReferenceProjectItem)
			{
				new CustomNode().AddTo(this);
				break;
			}
		}
	}

	public virtual void ShowReferences()
	{
		base.Nodes.Clear();
		foreach (ProjectItem item in project.Items)
		{
			if (item is ReferenceProjectItem)
			{
				ReferenceNode referenceNode = new ReferenceNode((ReferenceProjectItem)item);
				referenceNode.AddTo(this);
			}
		}
		UpdateIcon();
	}

	protected override void Initialize()
	{
		ShowReferences();
		base.Initialize();
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
	}
}
