using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class RunIncrementalSearch : AbstractMenuCommand
{
	private static IncrementalSearch incrementalSearch;

	protected virtual bool Forwards => true;

	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ActiveViewContent is ITextEditorControlProvider textEditorControlProvider)
		{
			if (incrementalSearch != null)
			{
				incrementalSearch.Dispose();
			}
			incrementalSearch = new IncrementalSearch(textEditorControlProvider.TextEditorControl, Forwards);
		}
	}
}
