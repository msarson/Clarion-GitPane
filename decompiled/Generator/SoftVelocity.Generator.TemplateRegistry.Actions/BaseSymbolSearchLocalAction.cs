using System.IO;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using SearchAndReplace;

namespace SoftVelocity.Generator.TemplateRegistry.Actions;

public abstract class BaseSymbolSearchLocalAction : BaseSymbolSearchRedFileAction
{
	protected override bool WritebackBaseSearchOptions(string symbolToSearch, TextArea textArea)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		string lookIn = ".";
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ActiveViewContent is IViewContent)
		{
			IViewContent val = (IViewContent)activeWorkbenchWindow.ActiveViewContent;
			if (!string.IsNullOrEmpty(val.FileName))
			{
				lookIn = Path.GetDirectoryName(val.FileName);
			}
		}
		SearchOptions.LookIn = lookIn;
		SearchOptions.LookInFiletypes = "*.tpl;*.tpw";
		SearchOptions.ReplacePattern = "";
		SearchOptions.MatchCase = false;
		SearchOptions.MatchWholeWord = false;
		SearchOptions.IncludeSubdirectories = false;
		SearchOptions.SearchAndReplaceBinding = SearchOptions.DirectoryBinding;
		return true;
	}
}
