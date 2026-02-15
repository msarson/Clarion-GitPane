using System.IO;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Project;

public class ReferenceNode : AbstractProjectBrowserTreeNode
{
	private ReferenceProjectItem referenceProjectItem;

	public ReferenceProjectItem ReferenceProjectItem => referenceProjectItem;

	public override bool EnableDelete => true;

	public ReferenceNode(ReferenceProjectItem referenceProjectItem)
	{
		this.referenceProjectItem = referenceProjectItem;
		base.Tag = referenceProjectItem;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/ReferenceNode";
		SetIcon("Icons.16x16.Reference");
		if (referenceProjectItem.ItemType == ItemType.ProjectReference)
		{
			base.Text = Path.GetFileNameWithoutExtension(referenceProjectItem.Include);
		}
		else
		{
			base.Text = referenceProjectItem.Name;
		}
	}

	public override void Delete()
	{
		IProject project = Project;
		TreeNode prevNode = base.PrevNode;
		ProjectService.RemoveProjectItem(referenceProjectItem.Project, referenceProjectItem);
		((ReferenceFolder)base.Parent).ShowReferences();
		project.Save();
		SelectPreviousNode(prevNode);
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
	}
}
