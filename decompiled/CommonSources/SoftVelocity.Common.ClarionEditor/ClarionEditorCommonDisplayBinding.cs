using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common.ClarionEditor;

public abstract class ClarionEditorCommonDisplayBinding : TextEditorDisplayBinding
{
	protected abstract CommonClarionEditor CreateClarionEditor();

	protected abstract IFoldingStrategy CreateFoldingStrategy();

	public override IViewContent CreateContentForFile(string fileName)
	{
		//IL_0059: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		CommonClarionEditor commonClarionEditor = CreateClarionEditor();
		((Control)(object)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Dock = DockStyle.Fill;
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.FoldingManager.FoldingStrategy = CreateFoldingStrategy();
		((AbstractViewContent)commonClarionEditor).Load(fileName);
		try
		{
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategyForFile(fileName);
			((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl.InitializeAdvancedHighlighter();
		}
		catch (HighlightingDefinitionInvalidException ex)
		{
			HighlightingDefinitionInvalidException ex2 = ex;
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy();
			MessageBox.Show(((object)ex2).ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl.InitializeFormatter();
		if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.FormattingStrategy is ClaCommonFormattingStrategy)
		{
			ClaCommonFormattingStrategy claCommonFormattingStrategy = (ClaCommonFormattingStrategy)(object)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.FormattingStrategy;
			claCommonFormattingStrategy.InitializeParser(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document);
			claCommonFormattingStrategy.ParseDocument(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document);
		}
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).TextEditorProperties.UseCustomLine = true;
		commonClarionEditor.ForceFoldingUpdate();
		((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl.ActivateQuickClassBrowserOnDemand();
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).TextEditorControl).ActiveTextAreaControl.Caret.Position = new TextLocation(0, 0);
		return (IViewContent)(object)commonClarionEditor;
	}

	public override IViewContent CreateContentForLanguage(string language, string content)
	{
		//IL_005c: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		CommonClarionEditor commonClarionEditor = CreateClarionEditor();
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.FoldingManager.FoldingStrategy = CreateFoldingStrategy();
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.TextContent = StringParser.Parse(content);
		try
		{
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(language);
			((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl.InitializeAdvancedHighlighter();
		}
		catch (HighlightingDefinitionInvalidException ex)
		{
			HighlightingDefinitionInvalidException ex2 = ex;
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy();
			MessageBox.Show(((object)ex2).ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl.InitializeFormatter();
		if (((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.FormattingStrategy is ClaCommonFormattingStrategy)
		{
			ClaCommonFormattingStrategy claCommonFormattingStrategy = (ClaCommonFormattingStrategy)(object)((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document.FormattingStrategy;
			claCommonFormattingStrategy.InitializeParser(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document);
			claCommonFormattingStrategy.ParseDocument(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).Document);
		}
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl).TextEditorProperties.UseCustomLine = true;
		((TextEditorDisplayBindingWrapper)commonClarionEditor).textAreaControl.ActivateQuickClassBrowserOnDemand();
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)commonClarionEditor).TextEditorControl).ActiveTextAreaControl.Caret.Position = new TextLocation(0, 0);
		return (IViewContent)(object)commonClarionEditor;
	}
}
