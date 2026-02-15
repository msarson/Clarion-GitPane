using System.IO;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Commands.TabStrip;

public class CopyPathName : AbtractNamedWorkbenchWindowMenuCommand
{
	public override void Run()
	{
		if (IsEnabled)
		{
			ClipboardWrapper.SetText(Path.GetFullPath(base.FileName));
		}
	}
}
