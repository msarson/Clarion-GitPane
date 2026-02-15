namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class DefaultCodeCompletionBinding : ICodeCompletionBinding
{
	private bool enableMethodInsight = true;

	private bool enableIndexerInsight = true;

	private bool enableXmlCommentCompletion = true;

	private bool enableDotCompletion = true;

	public bool EnableMethodInsight
	{
		get
		{
			return enableMethodInsight;
		}
		set
		{
			enableMethodInsight = value;
		}
	}

	public bool EnableIndexerInsight
	{
		get
		{
			return enableIndexerInsight;
		}
		set
		{
			enableIndexerInsight = value;
		}
	}

	public bool EnableXmlCommentCompletion
	{
		get
		{
			return enableXmlCommentCompletion;
		}
		set
		{
			enableXmlCommentCompletion = value;
		}
	}

	public bool EnableDotCompletion
	{
		get
		{
			return enableDotCompletion;
		}
		set
		{
			enableDotCompletion = value;
		}
	}

	public virtual bool HandleKeyPress(SharpDevelopTextAreaControl editor, char ch)
	{
		switch (ch)
		{
		case '(':
			if (enableMethodInsight && CodeCompletionOptions.InsightEnabled)
			{
				editor.ShowInsightWindow(new MethodInsightDataProvider());
				return true;
			}
			return false;
		case '[':
			if (enableIndexerInsight && CodeCompletionOptions.InsightEnabled)
			{
				editor.ShowInsightWindow(new IndexerInsightDataProvider());
				return true;
			}
			return false;
		case '<':
			if (enableXmlCommentCompletion)
			{
				editor.ShowCompletionWindow(new CommentCompletionDataProvider(), ch);
				return true;
			}
			return false;
		case '.':
			if (enableDotCompletion)
			{
				editor.ShowCompletionWindow(new CodeCompletionDataProvider(), ch);
				return true;
			}
			return false;
		case ' ':
		{
			if (!CodeCompletionOptions.KeywordCompletionEnabled)
			{
				return false;
			}
			string wordBeforeCaret = editor.GetWordBeforeCaret();
			if (wordBeforeCaret != null)
			{
				return HandleKeyword(editor, wordBeforeCaret);
			}
			return false;
		}
		default:
			return false;
		}
	}

	public virtual bool HandleKeyword(SharpDevelopTextAreaControl editor, string word)
	{
		return false;
	}
}
