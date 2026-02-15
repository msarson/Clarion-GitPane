using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class SelectAll : AbstractClipboardCommand
{
	protected override bool GetEnabled(IClipboardHandler editable)
	{
		return editable.EnableSelectAll;
	}

	protected override void Run(IClipboardHandler editable)
	{
		editable.SelectAll();
	}
}
