using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlProcedureNewCommand : AbstractControlMenuCommand
{
	public override string Description => "New Procedure";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandProcedureNew;
}
