using System;

namespace ICSharpCode.SharpDevelop.Gui;

public interface IWorkbenchLayout
{
	IWorkbenchWindow ActiveWorkbenchwindow { get; }

	object ActiveContent { get; }

	event EventHandler ActiveWorkbenchWindowChanged;

	void Attach(IWorkbench workbench);

	void Detach();

	void ShowPad(PadDescriptor content);

	void ShowAndDockPad(PadDescriptor content);

	void ActivatePad(PadDescriptor content);

	void ActivateAndDockPad(PadDescriptor content);

	void ActivateAndDockPad(string fullyQualifiedTypeName);

	void ActivatePad(string fullyQualifiedTypeName);

	void ActivatePadContent(PadDescriptor padContent);

	void HidePad(PadDescriptor content);

	void UnloadPad(PadDescriptor content);

	bool IsVisible(PadDescriptor padContent);

	void RedrawAllComponents();

	IWorkbenchWindow ShowView(IViewContent content);

	IWorkbenchWindow CreateWorkbenchWindow(IViewContent content);

	void LoadConfiguration();

	void StoreConfiguration();

	void OnActiveWorkbenchWindowChanged(EventArgs e);
}
