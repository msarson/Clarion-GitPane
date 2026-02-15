using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ClearErrorsButton : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (ErrorListPad.Instance != null)
			{
				return ErrorListPad.Instance.EnableDelete;
			}
			return false;
		}
	}

	public override void Run()
	{
		ErrorListPad.Instance.Delete();
	}
}
