using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class ToggleShowAll : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return AbstractProjectBrowserTreeNode.ShowAll;
		}
		set
		{
			ProjectBrowserPad.Instance.ProjectBrowserControl.ShowAll = value;
		}
	}
}
