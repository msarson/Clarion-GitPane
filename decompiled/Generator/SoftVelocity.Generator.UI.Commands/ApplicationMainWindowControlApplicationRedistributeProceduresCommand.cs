using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlApplicationRedistributeProceduresCommand : AbstractControlMenuCommand
{
	public override string Description => "Redistribute Procedures";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandApplicationRedistributeProcedures;
}
