using System;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class CachedCompletionDataProvider : AbstractCompletionDataProvider
{
	private ICompletionDataProvider baseProvider;

	private ICompletionData[] completionData;

	public ICompletionData[] CompletionData
	{
		get
		{
			return completionData;
		}
		set
		{
			completionData = value;
		}
	}

	public override ImageList ImageList => baseProvider.ImageList;

	[Obsolete("Cannot use InsertSpace on CachedCompletionDataProvider, please set it on the underlying provider!")]
	public new bool InsertSpace
	{
		get
		{
			return false;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public CachedCompletionDataProvider(ICompletionDataProvider baseProvider)
	{
		this.baseProvider = baseProvider;
	}

	public override CompletionDataProviderKeyResult ProcessKey(char key)
	{
		return baseProvider.ProcessKey(key);
	}

	public override bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
	{
		return baseProvider.InsertAction(data, textArea, insertionOffset, key);
	}

	public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
	{
		if (completionData == null)
		{
			completionData = baseProvider.GenerateCompletionData(fileName, textArea, charTyped) ?? new ICompletionData[0];
			preSelection = baseProvider.PreSelection;
			base.DefaultIndex = baseProvider.DefaultIndex;
		}
		return completionData;
	}
}
