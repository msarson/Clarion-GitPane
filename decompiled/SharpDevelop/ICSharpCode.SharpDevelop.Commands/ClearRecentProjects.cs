using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class ClearRecentProjects : AbstractMenuCommand
{
	public override void Run()
	{
		try
		{
			FileService.RecentOpen.ClearRecentItems(RecentOpen.defaultTypeProjects);
		}
		catch
		{
		}
	}
}
