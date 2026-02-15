using System;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

public abstract class AbstractPadContent : IPadContent, IDisposable
{
	public abstract Control Control { get; }

	public virtual bool WantsEscape => false;

	public bool IsVisible
	{
		get
		{
			if (Control.Visible)
			{
				return Control.Width > 0;
			}
			return false;
		}
	}

	public virtual void RedrawContent()
	{
	}

	public virtual void Dispose()
	{
	}
}
