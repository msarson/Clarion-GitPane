using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class ReloadFile : AbstractMenuCommand
{
	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null || activeWorkbenchWindow.ViewContent.FileName == null || activeWorkbenchWindow.ViewContent.IsViewOnly)
		{
			return;
		}
		bool flag = true;
		if (activeWorkbenchWindow.ViewContent.IsDirty && !MessageService.AskQuestion("${res:ICSharpCode.SharpDevelop.Commands.ReloadFile.ReloadFileQuestion}"))
		{
			flag = false;
		}
		if (flag)
		{
			Properties properties = null;
			if (activeWorkbenchWindow.ViewContent is IMementoCapable)
			{
				properties = ((IMementoCapable)activeWorkbenchWindow.ViewContent).CreateMemento();
			}
			try
			{
				activeWorkbenchWindow.ViewContent.Load(activeWorkbenchWindow.ViewContent.FileName);
			}
			catch (FileNotFoundException)
			{
				MessageService.ShowWarning("${res:ICSharpCode.SharpDevelop.Commands.ReloadFile.FileDeletedMessage}");
				return;
			}
			if (properties != null)
			{
				((IMementoCapable)activeWorkbenchWindow.ViewContent).SetMemento(properties);
			}
		}
	}
}
