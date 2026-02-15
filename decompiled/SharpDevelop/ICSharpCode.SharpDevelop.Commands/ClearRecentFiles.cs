using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class ClearRecentFiles : AbstractMenuCommand
{
	public override void Run()
	{
		try
		{
			FileService.RecentOpen.ClearRecentItems(RecentOpen.defaultTypeFiles);
		}
		catch
		{
		}
	}
}
