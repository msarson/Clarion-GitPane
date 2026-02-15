using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class OverrideCompletionData : DefaultCompletionData
{
	private IMember member;

	private static string GetName(IMethod method, ConversionFlags flags)
	{
		AmbienceService.CurrentAmbience.ConversionFlags = flags | ConversionFlags.ShowParameterNames;
		return AmbienceService.CurrentAmbience.Convert(method);
	}

	public OverrideCompletionData(IMethod method)
		: base(GetName(method, ConversionFlags.None), "override " + GetName(method, ConversionFlags.ShowAccessibility | ConversionFlags.ShowReturnType) + "\n\n" + method.Documentation, ClassBrowserIconService.GetIcon(method))
	{
		member = method;
	}

	public OverrideCompletionData(IProperty property)
		: base(property.Name, "override " + property.Name + "\n\n" + property.Documentation, ClassBrowserIconService.GetIcon(property))
	{
		member = property;
	}

	public override bool InsertAction(TextArea textArea, char ch)
	{
		ClassFinder targetContext = new ClassFinder(textArea.MotherTextEditorControl.FileName, textArea.Caret.Line + 1, textArea.Caret.Column + 1);
		int offset = textArea.Caret.Offset;
		LineSegment lineSegment = textArea.Document.GetLineSegment(textArea.Caret.Line);
		string text = textArea.Document.GetText(lineSegment.Offset, offset - lineSegment.Offset);
		string text2 = text;
		foreach (char c in text2)
		{
			if (!char.IsWhiteSpace(c) && !char.IsLetterOrDigit(c))
			{
				return base.InsertAction(textArea, ch);
			}
		}
		string indentation = text.Substring(0, text.Length - text.TrimStart().Length);
		CodeGenerator codeGenerator = ParserService.CurrentProjectContent.Language.CodeGenerator;
		string text3 = codeGenerator.GenerateCode(codeGenerator.GetOverridingMethod(member, targetContext), indentation);
		text3 = text3.TrimEnd();
		textArea.Document.Replace(lineSegment.Offset, offset - lineSegment.Offset, text3);
		int num = lineSegment.Offset + text3.Length;
		int lineNumberForOffset = textArea.Document.GetLineNumberForOffset(num);
		lineSegment = textArea.Document.GetLineSegment(lineNumberForOffset);
		textArea.MotherTextAreaControl.JumpTo(lineNumberForOffset, num - lineSegment.Offset);
		textArea.Refresh();
		return true;
	}
}
