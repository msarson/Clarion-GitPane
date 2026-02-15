using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Gui;

public class DefinitionViewPad : AbstractPadContent
{
	private TextEditorControl ctl;

	private FilePosition oldPosition;

	public override Control Control => ctl;

	public DefinitionViewPad()
	{
		ctl = new TextEditorControl();
		ctl.Document.ReadOnly = true;
		ctl.TextEditorProperties = SharpDevelopTextEditorProperties.Instance;
		ctl.ActiveTextAreaControl.TextArea.DoubleClick += OnDoubleClick;
		ParserService.ParserUpdateStepFinished += UpdateTick;
		TextEditorControl textEditorControl = ctl;
		EventHandler value = delegate
		{
			UpdateTick(null, null);
		};
		textEditorControl.VisibleChanged += value;
	}

	private void OnDoubleClick(object sender, EventArgs e)
	{
		string fileName = ctl.FileName;
		if (fileName != null)
		{
			Caret caret = ctl.ActiveTextAreaControl.Caret;
			FileService.JumpToFilePosition(fileName, caret.Line, caret.Column);
			UpdateTick(null, null);
		}
	}

	private void UpdateTick(object sender, ParserUpdateStepEventArgs e)
	{
		if (!base.IsVisible)
		{
			return;
		}
		LoggingService.Debug("DefinitionViewPad.Update");
		ResolveResult resolveResult = ResolveAtCaret(e);
		if (resolveResult != null)
		{
			FilePosition definitionPosition = resolveResult.GetDefinitionPosition();
			if (!definitionPosition.IsEmpty && File.Exists(definitionPosition.FileName))
			{
				WorkbenchSingleton.SafeThreadAsyncCall(OpenFile, definitionPosition);
			}
		}
	}

	private ResolveResult ResolveAtCaret(ParserUpdateStepEventArgs e)
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null)
		{
			return null;
		}
		if (!(activeWorkbenchWindow.ActiveViewContent is ITextEditorControlProvider { TextEditorControl: var textEditorControl }))
		{
			return null;
		}
		string text = ((e == null) ? textEditorControl.FileName : e.FileName);
		if (textEditorControl.FileName != text)
		{
			return null;
		}
		IExpressionFinder expressionFinder = ParserService.GetExpressionFinder(text);
		if (expressionFinder == null)
		{
			return null;
		}
		Caret caret = textEditorControl.ActiveTextAreaControl.Caret;
		string text2 = ((e == null) ? textEditorControl.Text : e.Content);
		ExpressionResult expressionResult = expressionFinder.FindFullExpression(text2, caret.Offset);
		if (expressionResult.Expression == null)
		{
			return null;
		}
		return ParserService.Resolve(expressionResult, caret.Line + 1, caret.Column + 1, text, text2);
	}

	private void OpenFile(FilePosition pos)
	{
		if (!pos.Equals(oldPosition))
		{
			oldPosition = pos;
			if (pos.FileName != ctl.FileName)
			{
				LoadFile(pos.FileName);
			}
			ctl.ActiveTextAreaControl.ScrollTo(int.MaxValue);
			ctl.ActiveTextAreaControl.Caret.Line = pos.Line - 1;
			ctl.ActiveTextAreaControl.ScrollToCaret();
		}
	}

	private void LoadFile(string fileName)
	{
		TextEditorControl textEditorControl = null;
		IWorkbenchWindow openFile = FileService.GetOpenFile(fileName);
		if (openFile != null && openFile.ActiveViewContent is ITextEditorControlProvider textEditorControlProvider)
		{
			textEditorControl = textEditorControlProvider.TextEditorControl;
		}
		if (textEditorControl != null)
		{
			ctl.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategyForFile(fileName);
			ctl.Text = textEditorControl.Text;
			ctl.FileName = fileName;
		}
		else
		{
			ctl.LoadFile(fileName, autoLoadHighlighting: true, autodetectEncoding: true);
		}
	}

	public override void RedrawContent()
	{
	}

	public override void Dispose()
	{
		ParserService.ParserUpdateStepFinished -= UpdateTick;
		ctl.Dispose();
		base.Dispose();
	}
}
