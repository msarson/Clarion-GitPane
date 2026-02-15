using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public abstract class AbstractCompletionDataProvider : ICompletionDataProvider
{
	private int defaultIndex = -1;

	protected string preSelection;

	private bool insertSpace;

	public virtual ImageList ImageList => ClassBrowserIconService.ImageList;

	public int DefaultIndex
	{
		get
		{
			return defaultIndex;
		}
		set
		{
			defaultIndex = value;
		}
	}

	public string PreSelection => preSelection;

	public bool InsertSpace
	{
		get
		{
			return insertSpace;
		}
		set
		{
			insertSpace = value;
		}
	}

	public virtual CompletionDataProviderKeyResult ProcessKey(char key)
	{
		if (key == ' ' && insertSpace)
		{
			insertSpace = false;
			return CompletionDataProviderKeyResult.BeforeStartKey;
		}
		if (char.IsLetterOrDigit(key) || key == '_')
		{
			insertSpace = false;
			return CompletionDataProviderKeyResult.NormalKey;
		}
		return CompletionDataProviderKeyResult.InsertionKey;
	}

	public virtual bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
	{
		if (InsertSpace)
		{
			textArea.Document.Insert(insertionOffset++, " ");
		}
		textArea.Caret.Position = textArea.Document.OffsetToPosition(insertionOffset);
		return data.InsertAction(textArea, key);
	}

	public abstract ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped);
}
