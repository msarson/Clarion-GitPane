using System.Diagnostics;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class OpenConfigDirectory : AbstractMenuCommand
{
	public override void Run()
	{
		if (!string.IsNullOrEmpty(PropertyService.ConfigDirectory))
		{
			Process.Start(PropertyService.ConfigDirectory);
		}
	}
}
