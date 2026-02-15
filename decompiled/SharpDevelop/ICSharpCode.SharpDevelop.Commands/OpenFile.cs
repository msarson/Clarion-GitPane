using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class OpenFile : AbstractMenuCommand
{
	private string defaultFileFilterExtension;

	private string defaultInitialDirectory;

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

	public string DefaultInitialDirectory
	{
		get
		{
			return defaultInitialDirectory;
		}
		set
		{
			defaultInitialDirectory = value;
		}
	}

	public override void Run()
	{
		using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
		openFileDialog.AddExtension = true;
		if (string.IsNullOrEmpty(DefaultInitialDirectory))
		{
			DefaultInitialDirectory = FileService.CurrentDirectory;
		}
		openFileDialog.InitialDirectory = DefaultInitialDirectory;
		string[] array = (string[])AddInTree.GetTreeNode("/SharpDevelop/Workbench/FileFilter").BuildChildItems(this).ToArray(typeof(string));
		Array.Sort(array);
		openFileDialog.Filter = string.Join("|", array);
		bool flag = false;
		if (!string.IsNullOrEmpty(DefaultFileFilterExtension))
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].IndexOf(DefaultFileFilterExtension, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					openFileDialog.FilterIndex = i + 1;
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
			if (activeWorkbenchWindow != null)
			{
				string extension = Path.GetExtension((activeWorkbenchWindow.ViewContent.FileName == null) ? activeWorkbenchWindow.ViewContent.UntitledName : activeWorkbenchWindow.ViewContent.FileName);
				if (!string.IsNullOrEmpty(extension))
				{
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j].IndexOf(extension, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							openFileDialog.FilterIndex = j + 1;
							flag = true;
							break;
						}
					}
				}
			}
		}
		if (!flag)
		{
			openFileDialog.FilterIndex = array.Length;
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k].IndexOf("*.*") >= 0)
				{
					openFileDialog.FilterIndex = k + 1;
					flag = true;
					break;
				}
			}
			if (!string.IsNullOrEmpty(DefaultFileFilterExtension) && DefaultFileFilterExtension.Contains("*"))
			{
				openFileDialog.FileName = DefaultFileFilterExtension;
			}
		}
		if (!flag)
		{
			openFileDialog.FilterIndex = array.Length;
		}
		openFileDialog.Multiselect = true;
		openFileDialog.CheckFileExists = true;
		if (openFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
		{
			string[] fileNames = openFileDialog.FileNames;
			foreach (string fileName in fileNames)
			{
				FileService.OpenFile(fileName);
			}
		}
		DefaultFileFilterExtension = null;
		DefaultInitialDirectory = null;
	}
}
