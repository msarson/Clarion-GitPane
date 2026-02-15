using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ShowWarningsToggleButton : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return ErrorListPad.Instance.ShowWarnings;
		}
		set
		{
			ErrorListPad.Instance.ShowWarnings = value;
		}
	}
}
