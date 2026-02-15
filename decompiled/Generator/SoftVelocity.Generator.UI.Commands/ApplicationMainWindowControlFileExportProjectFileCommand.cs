using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlFileExportProjectFileCommand : AbstractControlMenuCommand
{
	public override string Description => "Export Project File";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandFileExportProjectFile;
}
