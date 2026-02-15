using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class Undo : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (WorkbenchSingleton.Workbench.ActiveContent is IUndoHandler undoHandler)
			{
				return undoHandler.EnableUndo;
			}
			if (WorkbenchSingleton.ActiveControl is TextBoxBase textBoxBase)
			{
				return textBoxBase.CanUndo;
			}
			return false;
		}
	}

	public override void Run()
	{
		if (WorkbenchSingleton.Workbench.ActiveContent is IUndoHandler undoHandler)
		{
			undoHandler.Undo();
		}
		else if (WorkbenchSingleton.ActiveControl is TextBoxBase textBoxBase)
		{
			textBoxBase.Undo();
		}
	}
}
