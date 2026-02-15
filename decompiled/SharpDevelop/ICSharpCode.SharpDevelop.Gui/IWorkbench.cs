using System;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public interface IWorkbench : IMementoCapable
{
	string Title { get; set; }

	List<IViewContent> ViewContentCollection { get; }

	List<PadDescriptor> PadContentCollection { get; }

	IWorkbenchWindow ActiveWorkbenchWindow { get; }

	object ActiveContent { get; }

	IWorkbenchLayout WorkbenchLayout { get; set; }

	bool IsActiveWindow { get; }

	event ViewContentEventHandler ViewOpened;

	event ViewContentEventHandler ViewClosed;

	event EventHandler ActiveWorkbenchWindowChanged;

	void SetProjectTitle(IProject p);

	void SetClarionVersion(string version, bool forWindows);

	void SetDefaultClarionVersion(string version, bool forWindows);

	void ShowView(IViewContent content);

	void CreateView(IViewContent content);

	void ShowPad(PadDescriptor content);

	void UnloadPad(PadDescriptor content);

	PadDescriptor GetPad(Type type);

	void CloseContent(IViewContent content);

	void CloseAllViews();

	bool CloseAllSolutionViews();

	void RedrawAllComponents();
}
