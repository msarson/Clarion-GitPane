using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class AssemblyReferencePanel : Panel, IReferencePanel
{
	private ISelectReferenceDialog selectDialog;

	public AssemblyReferencePanel(ISelectReferenceDialog selectDialog)
	{
		this.selectDialog = selectDialog;
		Button button = new Button();
		button.Location = new Point(10, 10);
		button.Text = StringParser.Parse("${res:Global.BrowseButtonText}");
		button.Click += SelectReferenceDialog;
		button.FlatStyle = FlatStyle.System;
		base.Controls.Add(button);
	}

	private void SelectReferenceDialog(object sender, EventArgs e)
	{
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.AddExtension = true;
		openFileDialog.InitialDirectory = FileService.CurrentDirectory;
		openFileDialog.Filter = StringParser.Parse("${res:SharpDevelop.FileFilter.AssemblyFiles}|*.dll;*.exe|${res:SharpDevelop.FileFilter.AllFiles}|*.*");
		openFileDialog.Multiselect = true;
		openFileDialog.CheckFileExists = true;
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			string[] fileNames = openFileDialog.FileNames;
			foreach (string text in fileNames)
			{
				selectDialog.AddReference(ReferenceType.Assembly, Path.GetFileName(text), text, null);
			}
		}
	}

	public void AddReference()
	{
		SelectReferenceDialog(null, null);
	}
}
