namespace ICSharpCode.SharpDevelop.Project;

public class CustomFolderNode : AbstractProjectBrowserTreeNode
{
	private string closedImage;

	private string openedImage;

	public string ClosedImage
	{
		get
		{
			return closedImage;
		}
		set
		{
			closedImage = value;
			if (!base.IsExpanded)
			{
				SetIcon(closedImage);
			}
		}
	}

	public string OpenedImage
	{
		get
		{
			return openedImage;
		}
		set
		{
			openedImage = value;
			if (base.IsExpanded)
			{
				SetIcon(openedImage);
			}
		}
	}

	protected void UpdateIcon()
	{
		if (base.Nodes.Count == 0)
		{
			SetIcon(ClosedImage);
		}
		else if (base.IsExpanded)
		{
			SetIcon(openedImage);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		UpdateIcon();
	}

	public override void Expanding()
	{
		if (openedImage != null)
		{
			SetIcon(openedImage);
		}
		base.Expanding();
		if (base.Nodes.Count == 0)
		{
			SetIcon(ClosedImage);
		}
	}

	public override void Collapsing()
	{
		if (closedImage != null)
		{
			SetIcon(closedImage);
		}
		base.Collapsing();
	}

	public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
	{
		return visitor.Visit(this, data);
	}
}
