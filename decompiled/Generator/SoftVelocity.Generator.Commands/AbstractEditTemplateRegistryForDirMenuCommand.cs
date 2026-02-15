using System.IO;
using System.Windows.Forms;
using Clarion.Core.Options;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Ide.Core;

namespace SoftVelocity.Generator.Commands;

public abstract class AbstractEditTemplateRegistryForDirMenuCommand : AbstractEditTemplateRegistryMenuCommand
{
	public override void Run()
	{
		if (!((AbstractMenuCommand)this).IsEnabled)
		{
			return;
		}
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.Multiselect = false;
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		openFileDialog.CheckFileExists = true;
		openFileDialog.Title = ResourceService.GetString("Clarion.Generator.EditTemplateRegistryForDir.Title");
		string name = Versions.GetVersion(Versions.GetActiveVersion(ForWindows), ForWindows).RedirectionFile.Name;
		string text = string.Format(ResourceService.GetString("Clarion.Generator.EditTemplateRegistryForDir.Filter") + "|{0}", name);
		openFileDialog.Filter = StringParser.Parse(text + "|${res:SharpDevelop.FileFilter.AllFiles}|*.*");
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			ApplicationService.EditTemplateRegistry(ForWindows, Path.GetDirectoryName(openFileDialog.FileName));
		}
	}
}
