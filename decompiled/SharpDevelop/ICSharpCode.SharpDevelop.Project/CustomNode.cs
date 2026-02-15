namespace ICSharpCode.SharpDevelop.Project;

public class CustomNode : AbstractProjectBrowserTreeNode
{
	private NodeInitializer nodeInitializer;

	public NodeInitializer NodeInitializer
	{
		get
		{
			return nodeInitializer;
		}
		set
		{
			nodeInitializer = value;
		}
	}

	protected override void Initialize()
	{
		if (nodeInitializer != null)
		{
			nodeInitializer(this);
		}
		base.Initialize();
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
	}
}
