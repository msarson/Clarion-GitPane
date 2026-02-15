using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

public class ReferenceNode : ProjectNode
{
	private ReferenceProjectItem item;

	public ReferenceNode(ReferenceProjectItem item)
	{
		this.item = item;
		base.Text = item.Name;
		SetIcon("Icons.16x16.Reference");
		base.Nodes.Add(new TreeNode(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Pads.ClassScout.LoadingNode}")));
	}

	protected override void Initialize()
	{
		isInitialized = true;
		IProjectContent projectContentForReference = ParserService.GetProjectContentForReference(item);
		if (projectContentForReference == null)
		{
			return;
		}
		base.Nodes.Clear();
		foreach (IClass @class in projectContentForReference.Classes)
		{
			TreeNode nodeByPath = GetNodeByPath(@class.Namespace, create: true);
			new ClassNode(item.Project, @class).AddTo(nodeByPath);
		}
	}

	protected override string StripRootNamespace(string directory)
	{
		string text = item.Include;
		int num = text.IndexOf(',');
		if (num > 0)
		{
			text = text.Substring(0, num);
		}
		if (directory.ToLowerInvariant().StartsWith(text.ToLowerInvariant()))
		{
			directory = directory.Substring(text.Length);
		}
		return directory;
	}
}
