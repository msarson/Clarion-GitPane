using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Commands;

public class SaveFile : AbstractMenuCommand
{
	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null || activeWorkbenchWindow.ViewContent.IsViewOnly)
		{
			return;
		}
		if (activeWorkbenchWindow.ViewContent.FileName == null)
		{
			SaveFileAs saveFileAs = new SaveFileAs();
			saveFileAs.Run(activeWorkbenchWindow.Title.Substring(0, activeWorkbenchWindow.Title.Length - 1));
			return;
		}
		FileAttributes fileAttributes = FileAttributes.ReadOnly | FileAttributes.System | FileAttributes.Directory | FileAttributes.Offline;
		if (File.Exists(activeWorkbenchWindow.ViewContent.FileName) && (File.GetAttributes(activeWorkbenchWindow.ViewContent.FileName) & fileAttributes) != 0)
		{
			SaveFileAs saveFileAs2 = new SaveFileAs();
			saveFileAs2.Run();
		}
		else
		{
			ProjectService.MarkFileDirty(activeWorkbenchWindow.ViewContent.FileName);
			FileUtility.ObservedSave((FileOperationDelegate)activeWorkbenchWindow.ViewContent.Save, activeWorkbenchWindow.ViewContent.FileName, FileErrorPolicy.ProvideAlternative);
		}
	}
}
