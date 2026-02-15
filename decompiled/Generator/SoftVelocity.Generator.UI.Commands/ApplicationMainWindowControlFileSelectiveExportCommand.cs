using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlFileSelectiveExportCommand : AbstractControlMenuCommand
{
	public override string Description => "Selective Export";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandFileSelectiveExport;
}
