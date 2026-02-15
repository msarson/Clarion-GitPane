using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class PreviousErrorButton : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
			if (ErrorListPad.Instance != null)
			{
				return ErrorListPad.Instance.PreviousValid;
			}
			return false;
		}
	}

	public override void Run()
	{
		ErrorListPad.Instance.SelectPrevious();
	}
}
