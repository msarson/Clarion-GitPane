using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlApplicationDeleteEmptylibrariesCommand : AbstractControlMenuCommand
{
	public override string Description => "Delete Empty Libraries";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandApplicationDeleteEmptylibraries;
}
