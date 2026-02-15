namespace ICSharpCode.SharpDevelop.Gui;

public class ExtFolderNode : ExtTreeNode
{
	private string closedIcon;

	private string openedIcon;

	public string ClosedIcon
	{
		get
		{
			return closedIcon;
		}
		set
		{
			closedIcon = value;
			if (closedIcon != null && !base.IsExpanded)
			{
				SetIcon(closedIcon);
			}
		}
	}

	public string OpenedIcon
	{
		get
		{
			return openedIcon;
		}
		set
		{
			openedIcon = value;
			if (openedIcon != null && base.IsExpanded)
			{
				SetIcon(openedIcon);
			}
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (base.Nodes.Count == 0)
		{
			SetIcon(ClosedIcon);
		}
		else if (base.IsExpanded)
		{
			SetIcon(OpenedIcon);
		}
	}

	public override void Expanding()
	{
		base.Expanding();
		if (openedIcon != null)
		{
			SetIcon(openedIcon);
		}
	}

	public override void Collapsing()
	{
		base.Collapsing();
		if (closedIcon != null)
		{
			SetIcon(closedIcon);
		}
	}
}
