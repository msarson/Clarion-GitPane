using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public static class CustomToolsService
{
	private class CustomToolRun
	{
		internal CustomToolContext context;

		internal string file;

		internal FileProjectItem baseItem;

		internal ICustomTool customTool;

		internal bool showMessageBoxOnErrors;

		public CustomToolRun(CustomToolContext context, string file, FileProjectItem baseItem, ICustomTool customTool, bool showMessageBoxOnErrors)
		{
			this.context = context;
			this.file = file;
			this.baseItem = baseItem;
			this.customTool = customTool;
			this.showMessageBoxOnErrors = showMessageBoxOnErrors;
		}
	}

	private static bool initialized;

	private static List<CustomToolRun> toolRuns = new List<CustomToolRun>();

	private static Dictionary<string, CustomToolDescriptor> toolDict;

	private static List<CustomToolDescriptor> customToolList;

	private static CustomToolRun activeToolRun;

	internal static void Initialize()
	{
		customToolList = AddInTree.BuildItems<CustomToolDescriptor>("/SharpDevelop/CustomTools", null, throwOnNotFound: false);
		toolDict = new Dictionary<string, CustomToolDescriptor>(StringComparer.OrdinalIgnoreCase);
		foreach (CustomToolDescriptor customTool in customToolList)
		{
			toolDict[customTool.Name] = customTool;
		}
		if (!initialized)
		{
			initialized = true;
			FileUtility.FileSaved += OnFileSaved;
		}
	}

	private static void OnFileSaved(object sender, FileNameEventArgs e)
	{
		Solution openSolution = ProjectService.OpenSolution;
		if (openSolution == null)
		{
			return;
		}
		IProject project = openSolution.FindProjectContainingFile(e.FileName);
		if (project != null)
		{
			FileProjectItem fileProjectItem = project.FindFile(e.FileName);
			if (fileProjectItem != null && !string.IsNullOrEmpty(fileProjectItem.CustomTool))
			{
				RunCustomTool(fileProjectItem, showMessageBoxOnErrors: false);
			}
		}
	}

	public static IEnumerable<string> GetCustomToolNames()
	{
		return customToolList.ConvertAll((CustomToolDescriptor desc) => desc.Name);
	}

	public static IEnumerable<string> GetCompatibleCustomToolNames(FileProjectItem item)
	{
		string fileName = item.FileName;
		foreach (CustomToolDescriptor desc in customToolList)
		{
			if (desc.CanRunOnFile(fileName))
			{
				yield return desc.Name;
			}
		}
	}

	public static ICustomTool GetCustomTool(string name)
	{
		lock (toolDict)
		{
			if (toolDict.TryGetValue(name, out var value))
			{
				return value.Tool;
			}
			return null;
		}
	}

	public static void RunCustomTool(FileProjectItem baseItem, bool showMessageBoxOnErrors)
	{
		if (baseItem == null)
		{
			throw new ArgumentNullException("baseItem");
		}
		if (string.IsNullOrEmpty(baseItem.CustomTool))
		{
			return;
		}
		ICustomTool customTool = GetCustomTool(baseItem.CustomTool);
		if (customTool == null)
		{
			string text = "Cannot find custom tool '" + baseItem.CustomTool + "'.";
			CustomToolContext.StaticMessageView.AppendLine(text);
			if (showMessageBoxOnErrors)
			{
				MessageService.ShowError(text);
			}
		}
		else
		{
			RunCustomTool(baseItem, customTool, showMessageBoxOnErrors);
		}
	}

	public static void RunCustomTool(FileProjectItem baseItem, ICustomTool customTool, bool showMessageBoxOnErrors)
	{
		if (baseItem == null)
		{
			throw new ArgumentNullException("baseItem");
		}
		if (customTool == null)
		{
			throw new ArgumentNullException("customTool");
		}
		WorkbenchSingleton.AssertMainThread();
		string fileName = baseItem.FileName;
		if (!toolRuns.Exists((CustomToolRun run) => FileUtility.IsEqualFileName(run.file, fileName)))
		{
			CustomToolContext customToolContext = new CustomToolContext(baseItem.Project);
			if (string.IsNullOrEmpty(baseItem.CustomToolNamespace))
			{
				customToolContext.OutputNamespace = GetDefaultNamespace(baseItem.Project, baseItem.FileName);
			}
			else
			{
				customToolContext.OutputNamespace = baseItem.CustomToolNamespace;
			}
			RunCustomTool(new CustomToolRun(customToolContext, fileName, baseItem, customTool, showMessageBoxOnErrors));
		}
	}

	public static string GetDefaultNamespace(IProject project, string fileName)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		string relativePath = FileUtility.GetRelativePath(project.Directory, Path.GetDirectoryName(fileName));
		string[] array = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		StringBuilder stringBuilder = new StringBuilder(project.RootNamespace);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!(text == ".") && !(text == "..") && text.Length != 0 && !text.Equals("src", StringComparison.OrdinalIgnoreCase) && !text.Equals("source", StringComparison.OrdinalIgnoreCase))
			{
				stringBuilder.Append('.');
				stringBuilder.Append(NewFileDialog.GenerateValidClassName(text));
			}
		}
		return stringBuilder.ToString();
	}

	private static void RunCustomTool(CustomToolRun run)
	{
		if (activeToolRun != null)
		{
			toolRuns.Add(run);
			return;
		}
		try
		{
			run.customTool.GenerateCode(run.baseItem, run.context);
		}
		catch (Exception ex)
		{
			LoggingService.Error(ex);
			run.context.MessageView.AppendLine("Custom tool '" + run.baseItem.CustomTool + "' failed.");
			if (run.showMessageBoxOnErrors)
			{
				MessageService.ShowError("Custom tool '" + run.baseItem.CustomTool + "'failed:" + Environment.NewLine + ex.ToString());
			}
		}
		if (run.context.RunningSeparateThread)
		{
			activeToolRun = run;
		}
	}

	internal static void NotifyAsyncFinish(CustomToolContext context)
	{
		WorkbenchSingleton.SafeThreadAsyncCall(delegate
		{
			activeToolRun = null;
			CustomToolRun run = toolRuns[0];
			toolRuns.RemoveAt(0);
			RunCustomTool(run);
		});
	}
}
