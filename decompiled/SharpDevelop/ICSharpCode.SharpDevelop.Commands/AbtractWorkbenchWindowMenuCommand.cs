using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public abstract class AbtractWorkbenchWindowMenuCommand : AbstractMenuCommand
{
	private IWorkbenchWindow _Window;

	private bool windowSet;

	protected IWorkbenchWindow Window
	{
		get
		{
			if (!windowSet && Owner != null)
			{
				_Window = Owner as IWorkbenchWindow;
				windowSet = true;
			}
			return _Window;
		}
	}

	public override bool IsEnabled
	{
		get
		{
			if (Window != null)
			{
				return true;
			}
			return false;
		}
	}
}
