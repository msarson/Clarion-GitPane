using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class CopyErrorsButton : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (ErrorListPad.Instance != null)
			{
				return ErrorListPad.Instance.EnableCopy;
			}
			return false;
		}
	}

	public override void Run()
	{
		ErrorListPad.Instance.Copy();
	}
}
