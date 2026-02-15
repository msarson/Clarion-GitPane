using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlApplicationInsertModuleCommand : AbstractControlMenuCommand
{
	public override string Description => "Insert Module";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandApplicationInsertModule;
}
