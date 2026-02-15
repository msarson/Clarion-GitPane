using System.Windows.Forms;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Actions;
using SoftVelocity.Common.ClarionEditor.Actions;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.Dialogs;

namespace SoftVelocity.Common.ClarionEditor;

public abstract class ClarionCommonTextAreaControl : SharpDevelopTextAreaControl
{
	private CommonClarionEditor editor;

	private Control quickClassBrowserPanel;

	private int lastIndentSize = int.MaxValue;

	public abstract CommonCompletionBinding.CompletionRule KeywordsCompletionRule { get; }

	public abstract CommonCompletionBinding.CompletionRule NamesCompletionRule { get; }

	public CommonClarionEditor ClaEditor => editor;

	protected abstract Control CreateQuickClassBrowser(SharpDevelopTextAreaControl tac);

	public ClarionCommonTextAreaControl(CommonClarionEditor editor)
	{
		this.editor = editor;
		GenerateEditActions();
	}

	protected virtual void GenerateEditActions()
	{
		((TextEditorControlBase)this).editactions[Keys.Return | Keys.Control] = (IEditAction)(object)new GoToDeclaration();
		((TextEditorControlBase)this).editactions[Keys.Return | Keys.Shift | Keys.Control] = (IEditAction)(object)new GoToDefinition();
	}

	protected override bool HandleKeyPress(char ch)
	{
		bool flag = ((SharpDevelopTextAreaControl)this).HandleKeyPress(ch);
		if (ch == '.' && !flag)
		{
			ClaEditor.DotKeyPressed();
		}
		return flag;
	}

	protected override void RemoveQuickClassBrowserPanel()
	{
		if (quickClassBrowserPanel != null)
		{
			((Control)this).Controls.Remove(quickClassBrowserPanel);
			quickClassBrowserPanel.Dispose();
			quickClassBrowserPanel = null;
			((TextEditorControl)this).textAreaPanel.BorderStyle = BorderStyle.None;
		}
	}

	protected override void ShowQuickClassBrowserPanel()
	{
		if (quickClassBrowserPanel == null)
		{
			quickClassBrowserPanel = CreateQuickClassBrowser((SharpDevelopTextAreaControl)(object)this);
			((Control)this).Controls.Add(quickClassBrowserPanel);
			((TextEditorControl)this).textAreaPanel.BorderStyle = BorderStyle.Fixed3D;
		}
	}

	protected override void InitializeTextAreaControl(TextAreaControl newControl)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		((SharpDevelopTextAreaControl)this).InitializeTextAreaControl(newControl);
		newControl.TextArea.DoProcessDialogKey += new DialogKeyProcessor(TextArea_DoProcessDialogKey);
	}

	private bool TextArea_DoProcessDialogKey(Keys keyData)
	{
		if ((keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift)) && editor.ShowBlockIndentDialog)
		{
			if (!((TextEditorControlBase)((TextEditorDisplayBindingWrapper)editor).TextEditorControl).ActiveTextAreaControl.SelectionManager.HasSomethingSelected || ((TextEditorControlBase)((TextEditorDisplayBindingWrapper)editor).TextEditorControl).ActiveTextAreaControl.SelectionManager.SelectionIsReadonly)
			{
				return false;
			}
			if (lastIndentSize == int.MaxValue)
			{
				lastIndentSize = ((keyData == Keys.Tab) ? ((TextEditorControlBase)this).TextEditorProperties.TabIndent : (-((TextEditorControlBase)this).TextEditorProperties.TabIndent));
			}
			using (BlockIndentDialog blockIndentDialog = new BlockIndentDialog(lastIndentSize))
			{
				if (blockIndentDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
				{
					RelativeBlockIndent relativeBlockIndent = new RelativeBlockIndent();
					lastIndentSize = blockIndentDialog.IndentValue;
					relativeBlockIndent.Indent = lastIndentSize;
					((AbstractEditAction)relativeBlockIndent).Execute(((TextEditorControlBase)((TextEditorDisplayBindingWrapper)editor).TextEditorControl).ActiveTextAreaControl.TextArea);
				}
			}
			return true;
		}
		return false;
	}

	protected override bool IsSelectableChar(char ch)
	{
		if (!char.IsLetterOrDigit(ch) && ch != '_')
		{
			return ch == ':';
		}
		return true;
	}
}
