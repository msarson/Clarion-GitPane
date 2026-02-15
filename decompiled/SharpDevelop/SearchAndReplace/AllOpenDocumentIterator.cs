using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SearchAndReplace;

public class AllOpenDocumentIterator : IDocumentIterator
{
	private int startIndex = -1;

	private int curIndex = -1;

	private bool resetted = true;

	public string CurrentFileName
	{
		get
		{
			IViewContent currentTextEditorViewContent = GetCurrentTextEditorViewContent();
			if (currentTextEditorViewContent != null)
			{
				if (currentTextEditorViewContent.FileName == null)
				{
					return currentTextEditorViewContent.UntitledName;
				}
				return currentTextEditorViewContent.FileName;
			}
			return null;
		}
	}

	public ProvidedDocumentInformation Current
	{
		get
		{
			IViewContent currentTextEditorViewContent = GetCurrentTextEditorViewContent();
			if (currentTextEditorViewContent != null)
			{
				TextEditorControl textEditorControl = ((ITextEditorControlProvider)currentTextEditorViewContent).TextEditorControl;
				IDocument document = textEditorControl.Document;
				return new ProvidedDocumentInformation(document, CurrentFileName, textEditorControl.ActiveTextAreaControl);
			}
			return null;
		}
	}

	public AllOpenDocumentIterator()
	{
		Reset();
	}

	private IViewContent GetCurrentTextEditorViewContent()
	{
		GetCurIndex();
		if (curIndex >= 0)
		{
			IViewContent viewContent = WorkbenchSingleton.Workbench.ViewContentCollection[curIndex];
			if (viewContent is ITextEditorControlProvider)
			{
				return viewContent;
			}
		}
		return null;
	}

	private void GetCurIndex()
	{
		int count = WorkbenchSingleton.Workbench.ViewContentCollection.Count;
		if (curIndex != -1 && curIndex < count)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ViewContent == WorkbenchSingleton.Workbench.ViewContentCollection[i])
			{
				curIndex = i;
				return;
			}
		}
		curIndex = -1;
	}

	public bool MoveForward()
	{
		GetCurIndex();
		if (curIndex < 0)
		{
			return false;
		}
		if (resetted)
		{
			resetted = false;
			return true;
		}
		curIndex = (curIndex + 1) % WorkbenchSingleton.Workbench.ViewContentCollection.Count;
		if (curIndex == startIndex)
		{
			return false;
		}
		return true;
	}

	public bool MoveBackward()
	{
		GetCurIndex();
		if (curIndex < 0)
		{
			return false;
		}
		if (resetted)
		{
			resetted = false;
			return true;
		}
		if (curIndex == 0)
		{
			curIndex = WorkbenchSingleton.Workbench.ViewContentCollection.Count - 1;
		}
		if (curIndex > 0)
		{
			curIndex--;
			return true;
		}
		return false;
	}

	public void Reset()
	{
		curIndex = -1;
		GetCurIndex();
		startIndex = curIndex;
		resetted = true;
	}
}
