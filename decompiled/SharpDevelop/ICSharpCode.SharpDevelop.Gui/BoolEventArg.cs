using System;

namespace ICSharpCode.SharpDevelop.Gui;

public class BoolEventArg : EventArgs
{
	private bool barg;

	public bool Arg
	{
		get
		{
			return barg;
		}
		set
		{
			barg = value;
		}
	}

	public BoolEventArg(bool v)
	{
		barg = v;
	}
}
