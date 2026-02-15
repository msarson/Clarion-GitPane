using System;

namespace ICSharpCode.SharpDevelop.Gui;

public class TextEventArgs : EventArgs
{
	private string text;

	public string Text => text;

	public TextEventArgs(string text)
	{
		this.text = text;
	}
}
