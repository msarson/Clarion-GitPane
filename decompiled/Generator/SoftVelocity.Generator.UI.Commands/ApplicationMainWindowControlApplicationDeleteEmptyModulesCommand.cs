using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlApplicationDeleteEmptyModulesCommand : AbstractControlMenuCommand
{
	public override string Description => "Delete Empty Modules";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandApplicationDeleteEmptyModules;
}
