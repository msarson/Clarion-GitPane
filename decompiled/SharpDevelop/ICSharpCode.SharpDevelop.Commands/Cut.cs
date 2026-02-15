using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class Cut : AbstractClipboardCommand
{
	protected override bool GetEnabled(IClipboardHandler editable)
	{
		return editable.EnableCut;
	}

	protected override void Run(IClipboardHandler editable)
	{
		editable.Cut();
	}
}
