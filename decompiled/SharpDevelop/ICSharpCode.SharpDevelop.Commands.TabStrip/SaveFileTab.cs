using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Commands.TabStrip;

public class SaveFileTab : AbtractWorkbenchWindowMenuCommand
{
	public override void Run()
	{
		if (IsEnabled && base.Window.ViewContent != null && !base.Window.ViewContent.IsViewOnly)
		{
			if (base.Window.ViewContent.IsUntitled)
			{
				SaveFileAsTab.SaveFileAs(base.Window);
				return;
			}
			ProjectService.MarkFileDirty(base.Window.ViewContent.FileName);
			FileUtility.ObservedSave((FileOperationDelegate)base.Window.ViewContent.Save, base.Window.ViewContent.FileName);
		}
	}
}
