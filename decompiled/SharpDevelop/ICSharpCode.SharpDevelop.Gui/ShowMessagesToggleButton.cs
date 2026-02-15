using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ShowMessagesToggleButton : AbstractCheckableMenuCommand
{
	public override bool IsChecked
	{
		get
		{
			return ErrorListPad.Instance.ShowMessages;
		}
		set
		{
			ErrorListPad.Instance.ShowMessages = value;
		}
	}
}
