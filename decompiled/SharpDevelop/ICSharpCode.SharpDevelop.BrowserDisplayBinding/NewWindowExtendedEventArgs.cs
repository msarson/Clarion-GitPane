using System;
using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class NewWindowExtendedEventArgs : CancelEventArgs
{
	private Uri url;

	public Uri Url => url;

	public NewWindowExtendedEventArgs(Uri url)
	{
		this.url = url;
	}
}
