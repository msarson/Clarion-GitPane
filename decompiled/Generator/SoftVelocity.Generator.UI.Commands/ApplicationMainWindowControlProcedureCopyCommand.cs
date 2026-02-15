using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlProcedureCopyCommand : AbstractControlMenuCommand
{
	public override string Description => "Copy";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandProcedureCopy;
}
