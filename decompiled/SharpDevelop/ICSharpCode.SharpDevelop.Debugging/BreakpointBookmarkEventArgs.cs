using System;

namespace ICSharpCode.SharpDevelop.Debugging;

public class BreakpointBookmarkEventArgs : EventArgs
{
	private BreakpointBookmark breakpointBookmark;

	public BreakpointBookmark BreakpointBookmark => breakpointBookmark;

	public BreakpointBookmarkEventArgs(BreakpointBookmark breakpointBookmark)
	{
		this.breakpointBookmark = breakpointBookmark;
	}
}
