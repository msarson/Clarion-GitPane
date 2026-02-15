using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ShowErrorsToggleButton : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return ErrorListPad.Instance.ShowErrors;
		}
		set
		{
			ErrorListPad.Instance.ShowErrors = value;
		}
	}
}
