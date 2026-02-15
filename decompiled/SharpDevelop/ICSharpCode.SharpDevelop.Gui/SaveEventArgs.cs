using System;

namespace ICSharpCode.SharpDevelop.Gui;

public class SaveEventArgs : EventArgs
{
	private bool successful;

	public bool Successful => successful;

	public SaveEventArgs(bool successful)
	{
		this.successful = successful;
	}
}
