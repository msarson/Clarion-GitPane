using System;

namespace ICSharpCode.SharpDevelop.Gui;

public class ViewContentEventArgs : EventArgs
{
	private IViewContent content;

	public IViewContent Content
	{
		get
		{
			return content;
		}
		set
		{
			content = value;
		}
	}

	public ViewContentEventArgs(IViewContent content)
	{
		this.content = content;
	}
}
