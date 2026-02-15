using System;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class NavigateForward : AbstractMenuCommand
{
	public override bool IsEnabled => NavigationService.CanNavigateForwards;

	public override void Run()
	{
		NavigationService.Go(1);
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
	}
}
