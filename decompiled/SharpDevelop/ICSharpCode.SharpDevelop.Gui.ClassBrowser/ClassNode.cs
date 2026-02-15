using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ClassNode : ExtTreeNode, IClassNode
{
	private IClass c;

	private IProject project;

	public IClass Class
	{
		get
		{
			return c;
		}
		set
		{
			c = value;
			Initialize();
		}
	}

	public ClassNode(IProject project, IClass c)
	{
		sortOrder = 3;
		ContextmenuAddinTreePath = "/SharpDevelop/Pads/ClassBrowser/ClassContextMenu";
		this.project = project;
		this.c = c;
		base.Text = c.Name;
		base.SelectedImageIndex = (base.ImageIndex = ClassBrowserIconService.GetIcon(c));
		if (c.ClassType != ClassType.Delegate)
		{
			base.Nodes.Add(new TreeNode());
		}
	}

	public override void ActivateItem()
	{
		if (c.CompilationUnit != null)
		{
			FileService.JumpToFilePosition(c.CompilationUnit.FileName, c.Region.BeginLine - 1, c.Region.BeginColumn - 1);
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Nodes.Clear();
		if (c.ClassType == ClassType.Delegate)
		{
			return;
		}
		if (c.BaseTypes.Count > 0)
		{
			new BaseTypesNode(project, c).AddTo(this);
		}
		if ((c.Modifiers & ModifierEnum.Sealed) != ModifierEnum.Sealed)
		{
			new DerivedTypesNode(project, c).AddTo(this);
		}
		foreach (IClass innerClass in c.InnerClasses)
		{
			new ClassNode(project, innerClass).AddTo(this);
		}
		foreach (IMethod method in c.Methods)
		{
			new MemberNode(method).AddTo(this);
		}
		foreach (IProperty property in c.Properties)
		{
			new MemberNode(property).AddTo(this);
		}
		foreach (IField field in c.Fields)
		{
			new MemberNode(field).AddTo(this);
		}
		foreach (IEvent @event in c.Events)
		{
			new MemberNode(@event).AddTo(this);
		}
		UpdateVisibility();
	}
}
