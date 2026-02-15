using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlApplicationRenumberModulesCommand : AbstractControlMenuCommand
{
	public override string Description => "Renumber Modules";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandApplicationRenumberModules;
}
