using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Generator.Editor;

namespace SoftVelocity.Generator;

public static class AppGenEditorsService
{
	private static Dictionary<string, IProject> pweeFile2Project;

	private static Dictionary<string, string> pweeFile2GeneratedFile;

	static AppGenEditorsService()
	{
		pweeFile2Project = new Dictionary<string, IProject>(StringComparer.InvariantCultureIgnoreCase);
		pweeFile2GeneratedFile = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		ProjectService.SolutionClosed += SolutionClosed;
	}

	private static void SolutionClosed(object sender, EventArgs e)
	{
		Clear();
	}

	public static void RegisterPweeFile(string pweeFileName, string generatedFileName, IProject prj)
	{
		pweeFile2Project[pweeFileName] = prj;
		pweeFile2GeneratedFile[pweeFileName] = generatedFileName;
	}

	public static void RemovePweeFile(string pweeFileName)
	{
		pweeFile2Project.Remove(pweeFileName);
		pweeFile2GeneratedFile.Remove(pweeFileName);
	}

	public static void Clear()
	{
		pweeFile2Project.Clear();
		pweeFile2GeneratedFile.Clear();
	}

	public static IProject GetProjectForFile(string pweeFileName)
	{
		if (pweeFileName != null)
		{
			pweeFile2Project.TryGetValue(pweeFileName, out var value);
			return value;
		}
		return null;
	}

	public static string GetGeneratedFileNameForFile(string pweeFileName)
	{
		pweeFile2GeneratedFile.TryGetValue(pweeFileName, out var value);
		return value;
	}

	public static bool IsRegistered(string pweeFileName)
	{
		return pweeFile2Project.ContainsKey(pweeFileName);
	}

	public static string GetPweeFileContent(string pweeFileName)
	{
		CommonGenEditor pweeEditor = GetPweeEditor(pweeFileName);
		if (pweeEditor != null)
		{
			return ((IParseableContent)pweeEditor).ParseableText;
		}
		return null;
	}

	public static CommonGenEditor GetPweeEditor(string pweeFileName)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(pweeFileName))
		{
			return null;
		}
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			if (((IBaseViewContent)item).WorkbenchWindow != null)
			{
				IBaseViewContent activeViewContent = ((IBaseViewContent)item).WorkbenchWindow.ActiveViewContent;
				if (activeViewContent is CommonGenEditor && pweeFileName.Equals(((IParseableContent)activeViewContent).ParseableContentName, StringComparison.InvariantCultureIgnoreCase))
				{
					return (CommonGenEditor)(object)activeViewContent;
				}
			}
		}
		return null;
	}
}
