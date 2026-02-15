using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Commands.TabStrip;

public class SaveFileAsTab : AbstractMenuCommand
{
	public static void SaveFileAs(IWorkbenchWindow window)
	{
		using SoftVelocity.Ide.Core.SaveFileDialog saveFileDialog = FileDialogService.SaveFileDialog();
		saveFileDialog.OverwritePrompt = true;
		saveFileDialog.AddExtension = true;
		saveFileDialog.InitialDirectory = FileService.CurrentDirectory;
		saveFileDialog.Filter = string.Join("|", (string[])AddInTree.GetTreeNode("/SharpDevelop/Workbench/FileFilter").BuildChildItems(null).ToArray(typeof(string)));
		string[] array = (string[])AddInTree.GetTreeNode("/SharpDevelop/Workbench/FileFilter").BuildChildItems(null).ToArray(typeof(string));
		saveFileDialog.Filter = string.Join("|", array);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IndexOf(Path.GetExtension((window.ViewContent.FileName == null) ? window.ViewContent.UntitledName : window.ViewContent.FileName)) >= 0)
			{
				saveFileDialog.FilterIndex = i + 1;
				break;
			}
		}
		if (saveFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			string text = saveFileDialog.FileName;
			if (Path.GetExtension(text).StartsWith("?") || Path.GetExtension(text) == "*")
			{
				text = Path.ChangeExtension(text, "");
			}
			window.ViewContent.Save(text);
			MessageService.ShowMessage(text, "${res:ICSharpCode.SharpDevelop.Commands.SaveFile.FileSaved}");
		}
	}

	public override void Run()
	{
		if (Owner is IWorkbenchWindow workbenchWindow && !workbenchWindow.ViewContent.IsViewOnly)
		{
			SaveFileAs(workbenchWindow);
		}
	}
}
