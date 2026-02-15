using System;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

public interface IBaseViewContent : IDisposable
{
	Control Control { get; }

	IWorkbenchWindow WorkbenchWindow { get; set; }

	string TabPageText { get; }

	void SwitchedTo();

	void Selected();

	void Deselecting();

	void Deselected();

	void RedrawContent();
}
