using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.Editor.Commands;

public abstract class AbstractGenEditorCommand : AbstractMenuCommand
{
	public CommonGenEditor GenEditor
	{
		get
		{
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
			{
				return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as CommonGenEditor;
			}
			return null;
		}
	}
}
