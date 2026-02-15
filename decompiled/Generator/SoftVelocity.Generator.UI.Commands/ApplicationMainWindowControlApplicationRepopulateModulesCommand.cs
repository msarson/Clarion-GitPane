using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlApplicationRepopulateModulesCommand : AbstractControlMenuCommand
{
	public override string Description => "Repopulate Modules";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandApplicationRepopulateModules;
}
