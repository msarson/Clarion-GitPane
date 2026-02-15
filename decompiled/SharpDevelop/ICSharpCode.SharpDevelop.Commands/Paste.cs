using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class Paste : AbstractClipboardCommand
{
	protected override bool GetEnabled(IClipboardHandler editable)
	{
		return editable.EnablePaste;
	}

	protected override void Run(IClipboardHandler editable)
	{
		editable.Paste();
	}
}
