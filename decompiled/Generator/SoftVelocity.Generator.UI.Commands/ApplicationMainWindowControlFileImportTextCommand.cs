using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlFileImportTextCommand : AbstractControlMenuCommand
{
	public override string Description => "Import Text";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandFileImportText;
}
