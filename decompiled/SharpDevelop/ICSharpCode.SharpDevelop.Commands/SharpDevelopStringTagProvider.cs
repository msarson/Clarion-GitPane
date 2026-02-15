using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Commands;

public class SharpDevelopStringTagProvider : IStringTagProvider
{
	private static readonly string[] tags = new string[21]
	{
		"ItemPath", "ItemDir", "ItemFilename", "ItemExt", "CurLine", "CurCol", "CurText", "TargetPath", "TargetDir", "TargetName",
		"TargetExt", "CurrentProjectName", "ProjectDir", "ProjectFilename", "CombineDir", "CombineFilename", "Startuppath", "TaskService.Warnings", "TaskService.Errors", "TaskService.Messages",
		"NetSdkDir"
	};

	public string[] Tags => tags;

	private string GetCurrentItemPath()
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null && !WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsViewOnly && !WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.IsUntitled)
		{
			return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName;
		}
		return string.Empty;
	}

	private string GetCurrentTargetPath()
	{
		if (ProjectService.CurrentProject != null)
		{
			return ProjectService.CurrentProject.OutputAssemblyFullPath;
		}
		return string.Empty;
	}

	public string Convert(string tag)
	{
		switch (tag)
		{
		case "TaskService.Warnings":
			return TaskService.GetCount(TaskType.Warning).ToString();
		case "TaskService.Errors":
			return TaskService.GetCount(TaskType.Error).ToString();
		case "TaskService.Messages":
			return TaskService.GetCount(TaskType.Message).ToString();
		case "CurrentProjectName":
			if (ProjectService.CurrentProject == null)
			{
				return "<no current project>";
			}
			return ProjectService.CurrentProject.Name;
		default:
			switch (tag.ToUpperInvariant())
			{
			case "NETSDKDIR":
				return FileUtility.NetSdkInstallRoot;
			case "ITEMPATH":
				try
				{
					return GetCurrentItemPath();
				}
				catch (Exception)
				{
				}
				break;
			case "ITEMDIR":
				try
				{
					return Path.GetDirectoryName(GetCurrentItemPath());
				}
				catch (Exception)
				{
				}
				break;
			case "ITEMFILENAME":
				try
				{
					return Path.GetFileName(GetCurrentItemPath());
				}
				catch (Exception)
				{
				}
				break;
			case "ITEMEXT":
				try
				{
					return Path.GetExtension(GetCurrentItemPath());
				}
				catch (Exception)
				{
				}
				break;
			case "CURLINE":
				return string.Empty;
			case "CURCOL":
				return string.Empty;
			case "CURTEXT":
				return string.Empty;
			case "TARGETPATH":
				try
				{
					return GetCurrentTargetPath();
				}
				catch (Exception)
				{
				}
				break;
			case "TARGETDIR":
				try
				{
					return Path.GetDirectoryName(GetCurrentTargetPath());
				}
				catch (Exception)
				{
				}
				break;
			case "TARGETNAME":
				try
				{
					return Path.GetFileName(GetCurrentTargetPath());
				}
				catch (Exception)
				{
				}
				break;
			case "TARGETEXT":
				try
				{
					return Path.GetExtension(GetCurrentTargetPath());
				}
				catch (Exception)
				{
				}
				break;
			case "PROJECTDIR":
				if (ProjectService.CurrentProject != null)
				{
					return ProjectService.CurrentProject.FileName;
				}
				break;
			case "PROJECTFILENAME":
				if (ProjectService.CurrentProject != null)
				{
					try
					{
						return Path.GetFileName(ProjectService.CurrentProject.FileName);
					}
					catch (Exception)
					{
					}
				}
				break;
			case "COMBINEDIR":
				return Path.GetDirectoryName(ProjectService.OpenSolution.FileName);
			case "COMBINEFILENAME":
				try
				{
					return Path.GetFileName(ProjectService.OpenSolution.FileName);
				}
				catch (Exception)
				{
				}
				break;
			case "STARTUPPATH":
				return Application.StartupPath;
			}
			return string.Empty;
		}
	}
}
