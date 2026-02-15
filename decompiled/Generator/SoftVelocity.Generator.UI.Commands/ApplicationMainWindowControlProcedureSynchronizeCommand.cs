using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlProcedureSynchronizeCommand : AbstractControlMenuCommand
{
	public override string Description => "Synchronize";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandProcedureSynchronize;
}
