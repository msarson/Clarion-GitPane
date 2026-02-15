using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class BaseTypesNode : ExtFolderNode, IBaseTypesNode
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

	public BaseTypesNode(IProject project, IClass c)
	{
		sortOrder = 0;
		this.project = project;
		this.c = c;
		base.Text = ResourceService.GetString("MainWindow.Windows.ClassBrowser.BaseTypes");
		base.OpenedIcon = "ProjectBrowser.Folder.Open";
		base.ClosedIcon = "ProjectBrowser.Folder.Closed";
		base.Nodes.Add(new TreeNode(ResourceService.GetString("ICSharpCode.SharpDevelop.Gui.Pads.ClassScout.LoadingNode")));
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Nodes.Clear();
		IProjectContent projectContent = c.ProjectContent;
		if (projectContent != null)
		{
			int count = c.BaseTypes.Count;
			for (int i = 0; i < count; i++)
			{
				IClass obj = c.GetBaseType(i)?.GetUnderlyingClass();
				if (obj != null)
				{
					new ClassNode(project, obj).AddTo(this);
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
