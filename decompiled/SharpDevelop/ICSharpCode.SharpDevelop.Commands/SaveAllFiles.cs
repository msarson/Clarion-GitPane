using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class SaveAllFiles : AbstractMenuCommand
{
	public static void SaveAll()
	{
		IViewContent[] array = WorkbenchSingleton.Workbench.ViewContentCollection.ToArray();
		foreach (IViewContent viewContent in array)
		{
			if (viewContent.IsViewOnly)
			{
				continue;
			}
			if (viewContent.FileName == null)
			{
				if (viewContent is ICustomizedCommands)
				{
					if (!((ICustomizedCommands)viewContent).SaveAsCommand())
					{
					}
					continue;
				}
				using SoftVelocity.Ide.Core.SaveFileDialog saveFileDialog = FileDialogService.SaveFileDialog();
				saveFileDialog.OverwritePrompt = true;
				saveFileDialog.AddExtension = true;
				saveFileDialog.Filter = string.Join("|", (string[])AddInTree.GetTreeNode("/SharpDevelop/Workbench/FileFilter").BuildChildItems(null).ToArray(typeof(string)));
				saveFileDialog.InitialDirectory = FileService.CurrentDirectory;
				if (saveFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
				{
					string text = saveFileDialog.FileName;
					if (Path.GetExtension(text).StartsWith("?") || Path.GetExtension(text) == "*")
					{
						text = Path.ChangeExtension(text, "");
					}
					if (FileUtility.ObservedSave((NamedFileOperationDelegate)viewContent.Save, text) == FileOperationResult.OK)
					{
						MessageService.ShowMessage(text, "${res:ICSharpCode.SharpDevelop.Commands.SaveFile.FileSaved}");
					}
				}
			}
			else if (viewContent.IsDirty)
			{
				FileUtility.ObservedSave((FileOperationDelegate)viewContent.Save, viewContent.FileName);
			}
		}
	}

	public override void Run()
	{
		SaveAll();
	}
}
