using System;
using System.IO;
using System.Windows.Forms;
using Clarion.GEN;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Internal.Templates;
using SoftVelocity.Ide.Core;

namespace SoftVelocity.Generator.UI;

public class NewAppFromTxaBinaryProjectGenerator : IBinaryProjectGenerator
{
	public static string txaOutputName;

	public bool ProjectCreated => false;

	public string ProjectCreatedName
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool GenerateFiles(ProjectTemplate projectTemplate, ProjectCreateInformation projectCreateInformation)
	{
		using (SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog())
		{
			openFileDialog.AddExtension = true;
			openFileDialog.InitialDirectory = projectCreateInformation.ProjectBasePath;
			string[] value = (string[])AddInTree.GetTreeNode("/Clarion/Generator/FileFilter/Txa").BuildChildItems((object)this).ToArray(typeof(string));
			openFileDialog.FilterIndex = 0;
			openFileDialog.Filter = string.Join("|", value);
			openFileDialog.Multiselect = false;
			openFileDialog.CheckFileExists = true;
			openFileDialog.Title = StringParser.Parse("${res:Generator.SelectTxaFile}");
			if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
			{
				string text = Path.Combine(projectCreateInformation.ProjectBasePath, projectCreateInformation.ProjectName + ".app");
				string fileName = openFileDialog.FileName;
				Win32App win32App = ApplicationService.NewAppFromTxa(text, fileName);
				if (win32App != null)
				{
					txaOutputName = win32App.Target;
					ApplicationService.PushApplication(text);
					return true;
				}
				return false;
			}
		}
		return false;
	}
}
