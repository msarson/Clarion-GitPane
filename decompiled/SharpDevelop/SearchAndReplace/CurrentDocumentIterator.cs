using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;

namespace SearchAndReplace;

public class CurrentDocumentIterator : IDocumentIterator
{
	private bool didRead;

	public string CurrentFileName
	{
		get
		{
			if (!SearchReplaceUtilities.IsTextAreaSelected)
			{
				return null;
			}
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName == null)
			{
				return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.UntitledName;
			}
			return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent.FileName;
		}
	}

	public ProvidedDocumentInformation Current
	{
		get
		{
			if (!SearchReplaceUtilities.IsTextAreaSelected)
			{
				return null;
			}
			TextEditorControl textEditorControl = ((ITextEditorControlProvider)WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent).TextEditorControl;
			return new ProvidedDocumentInformation(textEditorControl.Document, CurrentFileName, textEditorControl.ActiveTextAreaControl);
		}
	}

	public CurrentDocumentIterator()
	{
		Reset();
	}

	public bool MoveForward()
	{
		if (!SearchReplaceUtilities.IsTextAreaSelected)
		{
			return false;
		}
		if (didRead)
		{
			return false;
		}
		didRead = true;
		return true;
	}

	public bool MoveBackward()
	{
		return MoveForward();
	}

	public void Reset()
	{
		didRead = false;
	}
}
