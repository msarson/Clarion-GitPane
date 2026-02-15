using System.Diagnostics;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Commands;

namespace CommonSources.Commands.TabStrip;

public class OpenContainingFolder : AbtractNamedWorkbenchWindowMenuCommand
{
	public static void Run(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName))
		{
			Process.Start("explorer.exe", "/select," + fileName);
		}
	}

	public override void Run()
	{
		if (((AbstractMenuCommand)this).IsEnabled)
		{
			Run(((AbtractNamedWorkbenchWindowMenuCommand)this).FileName);
		}
	}
}
