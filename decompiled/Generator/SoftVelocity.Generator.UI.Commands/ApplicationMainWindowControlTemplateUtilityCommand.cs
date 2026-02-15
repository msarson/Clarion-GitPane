using System.ComponentModel.Design;

namespace SoftVelocity.Generator.UI.Commands;

public class ApplicationMainWindowControlTemplateUtilityCommand : AbstractControlMenuCommand
{
	public override string Description => "Template Utility";

	public override CommandID CommandID => ApplicationMainWindowControl.CommandTemplateUtility;
}
