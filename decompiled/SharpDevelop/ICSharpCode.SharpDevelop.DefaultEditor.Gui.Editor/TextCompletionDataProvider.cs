using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class TextCompletionDataProvider : AbstractCompletionDataProvider
{
	private string[] texts;

	public TextCompletionDataProvider(params string[] texts)
	{
		this.texts = texts;
	}

	public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
	{
		ICompletionData[] array = new ICompletionData[texts.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new DefaultCompletionData(texts[i], null, 13);
		}
		return array;
	}
}
