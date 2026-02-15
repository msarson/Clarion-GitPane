using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlProcedureRefreshCommand : AbstractControlMenuCommand
{
	public override string Description => "Refresh";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandProcedureRefresh;
}
