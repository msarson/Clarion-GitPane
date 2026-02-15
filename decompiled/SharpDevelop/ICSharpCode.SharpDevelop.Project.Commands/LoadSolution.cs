using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class LoadSolution : AbstractMenuCommand
{
	private string defaultFileFilterExtension;

	public string DefaultFileFilterExtension
	{
		get
		{
			return defaultFileFilterExtension;
		}
		set
		{
			defaultFileFilterExtension = value;
		}
	}

	public override void Run()
	{
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.AddExtension = true;
		openFileDialog.Filter = ProjectService.GetAllProjectsFilter(this);
		if (!string.IsNullOrEmpty(DefaultFileFilterExtension))
		{
			string[] array = openFileDialog.Filter.Split('|');
			if (array.Length > 0)
			{
				int num = 1;
				int num2 = 0;
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					_ = array2[i];
					if (array[num].Equals(DefaultFileFilterExtension, StringComparison.OrdinalIgnoreCase))
					{
						openFileDialog.FilterIndex = num2 + 1;
						break;
					}
					num += 2;
					num2++;
				}
			}
		}
		openFileDialog.Multiselect = false;
		openFileDialog.CheckFileExists = true;
		openFileDialog.InitialDirectory = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.DefaultPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Clarion Projects")).ToString();
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			ProjectService.LoadSolutionOrProject(openFileDialog.FileName);
		}
	}
}
