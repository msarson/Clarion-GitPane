using System;
using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.Gui;

public interface IWorkbenchWindow
{
	string Title { get; set; }

	bool IsDisposed { get; }

	IViewContent ViewContent { get; }

	IBaseViewContent ActiveViewContent { get; }

	event EventHandler WindowSelected;

	event EventHandler WindowDeselected;

	event EventHandler TitleChanged;

	event EventHandler CloseEvent;

	event CancelEventHandler ClosingEvent;

	event EventHandler SecondaryViewsUpdated;

	bool CloseWindow(bool force);

	void SelectWindow();

	void RedrawContent();

	void SwitchView(int viewNumber);

	void OnWindowSelected(EventArgs e);

	void OnWindowDeselected(EventArgs e);
}
