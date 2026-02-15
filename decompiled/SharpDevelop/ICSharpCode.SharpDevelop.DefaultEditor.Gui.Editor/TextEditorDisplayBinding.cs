using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Codons;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class TextEditorDisplayBinding : IDisplayBinding
{
	static TextEditorDisplayBinding()
	{
		string text = Path.Combine(PropertyService.ConfigDirectory, "modes");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		HighlightingManager.Manager.AddSyntaxModeFileProvider(new AddInTreeSyntaxModeProvider());
		if (Directory.Exists(Path.Combine(PropertyService.DataDirectory, "modes")))
		{
			HighlightingManager.Manager.AddSyntaxModeFileProvider(new FileSyntaxModeProvider(Path.Combine(PropertyService.DataDirectory, "modes")));
		}
		if (Directory.Exists(text))
		{
			HighlightingManager.Manager.AddSyntaxModeFileProvider(new FileSyntaxModeProvider(text));
		}
	}

	public virtual bool CanCreateContentForFile(string fileName)
	{
		return true;
	}

	public virtual bool CanCreateContentForLanguage(string language)
	{
		return true;
	}

	public virtual IViewContent CreateContentForFile(string fileName)
	{
		TextEditorDisplayBindingWrapper textEditorDisplayBindingWrapper = new TextEditorDisplayBindingWrapper();
		textEditorDisplayBindingWrapper.textAreaControl.Dock = DockStyle.Fill;
		textEditorDisplayBindingWrapper.Load(fileName);
		try
		{
			textEditorDisplayBindingWrapper.textAreaControl.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategyForFile(fileName);
			textEditorDisplayBindingWrapper.textAreaControl.InitializeAdvancedHighlighter();
		}
		catch (HighlightingDefinitionInvalidException ex)
		{
			textEditorDisplayBindingWrapper.textAreaControl.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy();
			MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		textEditorDisplayBindingWrapper.textAreaControl.InitializeFormatter();
		textEditorDisplayBindingWrapper.textAreaControl.ActivateQuickClassBrowserOnDemand();
		return textEditorDisplayBindingWrapper;
	}

	public virtual IViewContent CreateContentForLanguage(string language, string content)
	{
		TextEditorDisplayBindingWrapper textEditorDisplayBindingWrapper = new TextEditorDisplayBindingWrapper();
		textEditorDisplayBindingWrapper.textAreaControl.Document.TextContent = content;
		try
		{
			textEditorDisplayBindingWrapper.textAreaControl.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(language);
			textEditorDisplayBindingWrapper.textAreaControl.InitializeAdvancedHighlighter();
		}
		catch (HighlightingDefinitionInvalidException ex)
		{
			textEditorDisplayBindingWrapper.textAreaControl.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy();
			MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		textEditorDisplayBindingWrapper.textAreaControl.InitializeFormatter();
		textEditorDisplayBindingWrapper.textAreaControl.ActivateQuickClassBrowserOnDemand();
		return textEditorDisplayBindingWrapper;
	}
}
