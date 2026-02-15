using System;
using System.Collections.Generic;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public interface IAdvancedHighlighter : IDisposable
{
	void Initialize(TextEditorControl textEditor);

	void BeginUpdate(IDocument document, IList<LineSegment> inputLines);

	void EndUpdate();

	void MarkLine(int lineNumber, LineSegment currentLine, List<TextWord> words);
}
