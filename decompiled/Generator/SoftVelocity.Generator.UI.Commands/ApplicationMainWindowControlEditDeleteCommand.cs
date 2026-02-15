using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlEditDeleteCommand : AbstractControlMenuCommand
{
	public override string Description => "Delete";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandEditDelete;
}
