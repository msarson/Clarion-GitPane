using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Common.ClassBrowser;

public class ClaBaseTypesNode : ExtFolderNode, IBaseTypesNode
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

	public ClaBaseTypesNode(IProject project, IClass c)
	{
		((ExtTreeNode)this).sortOrder = 0;
		this.project = project;
		this.c = c;
		((TreeNode)this).Text = "Base types";
		((ExtFolderNode)this).OpenedIcon = "ProjectBrowser.Folder.Open";
		((ExtFolderNode)this).ClosedIcon = "ProjectBrowser.Folder.Closed";
		((TreeNode)this).Nodes.Add(new TreeNode(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Pads.ClassScout.LoadingNode}")));
	}

	protected override void Initialize()
	{
		((ExtTreeNode)this).Initialize();
		((TreeNode)this).Nodes.Clear();
		IProjectContent projectContent = c.ProjectContent;
		if (projectContent != null)
		{
			int count = c.BaseTypes.Count;
			for (int i = 0; i < count; i++)
			{
				IReturnType baseType = c.GetBaseType(i);
				IClass val = ((baseType != null) ? baseType.GetUnderlyingClass() : null);
				if (val != null)
				{
					((ExtTreeNode)new ClaClassNode(project, val)).AddTo((TreeNode)(object)this);
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
