using System;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Generator;

internal class ClarionTemplateEditorDisplayBinding : TextEditorDisplayBinding
{
	private const string languageNameClarionTemplate = "Clarion Template";

	private IViewContent GetTplEditorFromFile(string fileName)
	{
		return GetTplEditor(fileName, isContent: false);
	}

	private IViewContent GetTplEditorFromMemory(string content)
	{
		return GetTplEditor(content, isContent: true);
	}

	private IViewContent GetTplEditor(string fileName, bool isContent)
	{
		//IL_0082: Expected O, but got Unknown
		ClarionTemplateEditorDisplayBindingWrapper clarionTemplateEditorDisplayBindingWrapper = new ClarionTemplateEditorDisplayBindingWrapper();
		((Control)(object)((TextEditorDisplayBindingWrapper)clarionTemplateEditorDisplayBindingWrapper).textAreaControl).Dock = DockStyle.Fill;
		((TextEditorControlBase)((TextEditorDisplayBindingWrapper)clarionTemplateEditorDisplayBindingWrapper).textAreaControl).Document.FoldingManager.FoldingStrategy = (IFoldingStrategy)(object)new ClarionTemplateFolding();
		if (isContent)
		{
			((TextEditorDisplayBindingWrapper)clarionTemplateEditorDisplayBindingWrapper).Text = fileName;
		}
		else
		{
			((AbstractViewContent)clarionTemplateEditorDisplayBindingWrapper).Load(fileName);
		}
		try
		{
			if (isContent)
			{
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)clarionTemplateEditorDisplayBindingWrapper).textAreaControl).Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy("Clarion Template");
			}
			else
			{
				((TextEditorControlBase)((TextEditorDisplayBindingWrapper)clarionTemplateEditorDisplayBindingWrapper).textAreaControl).Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategyForFile(fileName);
			}
			((TextEditorDisplayBindingWrapper)clarionTemplateEditorDisplayBindingWrapper).textAreaControl.InitializeAdvancedHighlighter();
		}
		catch (HighlightingDefinitionInvalidException ex)
		{
			HighlightingDefinitionInvalidException ex2 = ex;
			((TextEditorControlBase)((TextEditorDisplayBindingWrapper)clarionTemplateEditorDisplayBindingWrapper).textAreaControl).Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy();
			MessageBox.Show(((object)ex2).ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		((TextEditorDisplayBindingWrapper)clarionTemplateEditorDisplayBindingWrapper).textAreaControl.InitializeFormatter();
		clarionTemplateEditorDisplayBindingWrapper.ForceFoldingRefresh();
		return (IViewContent)(object)clarionTemplateEditorDisplayBindingWrapper;
	}

	public override IViewContent CreateContentForFile(string fileName)
	{
		return GetTplEditorFromFile(fileName);
	}

	public override bool CanCreateContentForFile(string fileName)
	{
		return true;
	}

	public override bool CanCreateContentForLanguage(string languageName)
	{
		return languageName == "Clarion Template";
	}

	public override IViewContent CreateContentForLanguage(string languageName, string content)
	{
		if (languageName != "Clarion Template")
		{
			throw new NotImplementedException();
		}
		return GetTplEditorFromMemory(content);
	}
}
