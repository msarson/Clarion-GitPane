using Clarion.Core.Redirection;
using ICSharpCode.SharpDevelop;

namespace SoftVelocity.Common.Redirection;

public class OpenRedirectionFileMenuBuilder : VersionBasedMenuBuilder
{
	public override void MenuItemSelected(string version, bool forWindows)
	{
		FileService.OpenFile(RedirectionFile.GetRedirectionFile(forWindows, version).FullName);
	}

	public OpenRedirectionFileMenuBuilder()
	{
		baseMenuText = "${res:MainMenu.ToolsMenu.Redirection}";
	}
}
