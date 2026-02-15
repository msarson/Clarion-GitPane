using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class Redo : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (WorkbenchSingleton.Workbench.ActiveContent is IUndoHandler undoHandler)
			{
				return undoHandler.EnableRedo;
			}
			return false;
		}
	}

	public override void Run()
	{
		if (WorkbenchSingleton.Workbench.ActiveContent is IUndoHandler undoHandler)
		{
			undoHandler.Redo();
		}
	}
}
