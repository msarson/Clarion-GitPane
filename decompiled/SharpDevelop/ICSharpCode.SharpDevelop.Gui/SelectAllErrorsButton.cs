using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class SelectAllErrorsButton : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (ErrorListPad.Instance != null)
			{
				return ErrorListPad.Instance.EnableSelectAll;
			}
			return false;
		}
	}

	public override void Run()
	{
		ErrorListPad.Instance.SelectAll();
	}
}
