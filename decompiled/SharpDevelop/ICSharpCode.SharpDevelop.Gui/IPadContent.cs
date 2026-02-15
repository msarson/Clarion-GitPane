using System;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

public interface IPadContent : IDisposable
{
	Control Control { get; }

	bool WantsEscape { get; }

	void RedrawContent();
}
