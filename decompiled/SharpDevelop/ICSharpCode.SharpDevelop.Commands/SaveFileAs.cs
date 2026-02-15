using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class SaveFileAs : AbstractMenuCommand
{
	private string defaultName;

	public void Run(string defaultFileName)
	{
		defaultName = defaultFileName;
		Run();
	}

	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null || activeWorkbenchWindow.ViewContent.IsViewOnly || (activeWorkbenchWindow.ViewContent is ICustomizedCommands && ((ICustomizedCommands)activeWorkbenchWindow.ViewContent).SaveAsCommand()))
		{
			return;
		}
		using SoftVelocity.Ide.Core.SaveFileDialog saveFileDialog = FileDialogService.SaveFileDialog();
		saveFileDialog.OverwritePrompt = true;
		saveFileDialog.AddExtension = true;
		saveFileDialog.InitialDirectory = FileService.CurrentDirectory;
		if (!string.IsNullOrEmpty(defaultName))
		{
			saveFileDialog.FileName = defaultName;
		}
		string[] array = (string[])AddInTree.GetTreeNode("/SharpDevelop/Workbench/FileFilter").BuildChildItems(this).ToArray(typeof(string));
		saveFileDialog.Filter = string.Join("|", array);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IndexOf(Path.GetExtension((activeWorkbenchWindow.ViewContent.FileName == null) ? activeWorkbenchWindow.ViewContent.UntitledName : activeWorkbenchWindow.ViewContent.FileName), StringComparison.OrdinalIgnoreCase) >= 0)
			{
				saveFileDialog.FilterIndex = i + 1;
				break;
			}
		}
		if (saveFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			string fileName = saveFileDialog.FileName;
			if (FileService.CheckFileName(fileName) && FileUtility.ObservedSave((NamedFileOperationDelegate)activeWorkbenchWindow.ViewContent.Save, fileName) == FileOperationResult.OK)
			{
				FileService.RecentOpen.AddLastItem(FileService.GetFileCategory(fileName), fileName, null);
				MessageService.ShowMessage(fileName, "${res:ICSharpCode.SharpDevelop.Commands.SaveFile.FileSaved}");
			}
		}
	}
}
