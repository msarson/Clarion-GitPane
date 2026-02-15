using System;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class NavigateBack : AbstractMenuCommand
{
	private ToolBarSplitButton splitButton;

	public override bool IsEnabled
	{
		get
		{
			UpdateEnabledState();
			return NavigationService.CanNavigateBack;
		}
	}

	public override void Run()
	{
		NavigationService.Go(-1);
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		splitButton = (ToolBarSplitButton)Owner;
		NavigationService.HistoryChanged += NavHistoryChanged;
		NavHistoryChanged(this, EventArgs.Empty);
	}

	public void NavHistoryChanged(object sender, EventArgs e)
	{
		_ = NavigationService.CurrentPosition;
		UpdateEnabledState();
	}

	public void UpdateEnabledState()
	{
		splitButton.ButtonEnabled = NavigationService.CanNavigateBack;
		splitButton.DropDownEnabled = NavigationService.Count > 1;
	}
}
