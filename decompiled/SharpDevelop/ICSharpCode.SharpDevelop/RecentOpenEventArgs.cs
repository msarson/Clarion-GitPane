using System;

namespace ICSharpCode.SharpDevelop;

public class RecentOpenEventArgs : EventArgs
{
	private string type;

	public string RecentType => type;

	public RecentOpenEventArgs(string type)
	{
		this.type = type;
	}
}
