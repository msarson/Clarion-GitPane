using System;
using System.Collections.Generic;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

internal class AdvancedHighlightingStrategy : DefaultHighlightingStrategy
{
	private readonly IAdvancedHighlighter highlighter;

	public AdvancedHighlightingStrategy(DefaultHighlightingStrategy baseStrategy, IAdvancedHighlighter highlighter)
	{
		if (highlighter == null)
		{
			throw new ArgumentNullException("highlighter");
		}
		ImportSettingsFrom(baseStrategy);
		this.highlighter = highlighter;
	}

	public override void MarkTokens(IDocument document)
	{
		highlighter.BeginUpdate(document, null);
		base.MarkTokens(document);
		highlighter.EndUpdate();
	}

	public override void MarkTokens(IDocument document, List<LineSegment> inputLines)
	{
		highlighter.BeginUpdate(document, inputLines);
		base.MarkTokens(document, inputLines);
		highlighter.EndUpdate();
	}

	protected override void OnParsedLine(IDocument document, LineSegment currentLine, List<TextWord> words)
	{
		highlighter.MarkLine(currentLineNumber, currentLine, words);
	}
}
