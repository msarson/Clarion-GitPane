using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlFileExportTextCommand : AbstractControlMenuCommand
{
	public override string Description => "Export Text";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandFileExportText;
}
