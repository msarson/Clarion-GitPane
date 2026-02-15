using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ToggleMessageViewWordWrap : AbstractCheckableMenuCommand
{
	private ToolBarCheckBox checkBox;

	public override bool IsChecked
	{
		get
		{
			return CompilerMessageView.Instance.WordWrap;
		}
		set
		{
			CompilerMessageView.Instance.WordWrap = value;
		}
	}

	public override object Owner
	{
		set
		{
			base.Owner = value;
			checkBox = (ToolBarCheckBox)Owner;
		}
	}

	public override void Run()
	{
		IsChecked = !IsChecked;
	}
}
