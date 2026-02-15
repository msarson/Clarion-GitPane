using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlApplicationRefreshCommand : AbstractControlMenuCommand
{
	public override string Description => "Refresh";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandApplicationRefresh;
}
