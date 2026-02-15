using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlFileImportFromApplicationCommand : AbstractControlMenuCommand
{
	public override string Description => "Import From Application...";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandFileImportFromApplication;
}
