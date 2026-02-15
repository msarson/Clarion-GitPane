using System;

namespace ICSharpCode.SharpDevelop.Util;

public class LineReceivedEventArgs : EventArgs
{
	private string line = string.Empty;

	public string Line => line;

	public LineReceivedEventArgs(string line)
	{
		this.line = line;
	}
}
