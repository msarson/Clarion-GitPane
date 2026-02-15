using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.OptionPanels;

public class BehaviorTextEditorPanel : AbstractOptionPanel
{
	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.BehaviorTextEditorPanel.xfrm"));
		SharpDevelopTextEditorProperties instance = SharpDevelopTextEditorProperties.Instance;
		((CheckBox)ControlDictionary["autoinsertCurlyBraceCheckBox"]).Checked = instance.AutoInsertCurlyBracket;
		((CheckBox)ControlDictionary["hideMouseCursorCheckBox"]).Checked = instance.HideMouseCursor;
		((CheckBox)ControlDictionary["caretBehindEOLCheckBox"]).Checked = instance.AllowCaretBeyondEOL;
		((CheckBox)ControlDictionary["auotInsertTemplatesCheckBox"]).Checked = instance.AutoInsertTemplates;
		((CheckBox)ControlDictionary["cutCopyWholeLine"]).Checked = instance.CutCopyWholeLine;
		((CheckBox)ControlDictionary["convertTabsToSpacesCheckBox"]).Checked = instance.ConvertTabsToSpaces;
		ControlDictionary["tabSizeTextBox"].Text = instance.TabIndent.ToString();
		ControlDictionary["indentSizeTextBox"].Text = instance.IndentationSize.ToString();
		((ComboBox)ControlDictionary["indentStyleComboBox"]).Items.Add(StringParser.Parse("${res:Dialog.Options.IDEOptions.TextEditor.Behaviour.IndentStyle.None}"));
		((ComboBox)ControlDictionary["indentStyleComboBox"]).Items.Add(StringParser.Parse("${res:Dialog.Options.IDEOptions.TextEditor.Behaviour.IndentStyle.Automatic}"));
		((ComboBox)ControlDictionary["indentStyleComboBox"]).Items.Add(StringParser.Parse("${res:Dialog.Options.IDEOptions.TextEditor.Behaviour.IndentStyle.Smart}"));
		((ComboBox)ControlDictionary["indentStyleComboBox"]).SelectedIndex = (int)instance.IndentStyle;
		((ComboBox)ControlDictionary["mouseWhellDirectionComboBox"]).Items.Add(StringParser.Parse("${res:Dialog.Options.IDEOptions.TextEditor.Behaviour.NormalMouseDirectionRadioButton}"));
		((ComboBox)ControlDictionary["mouseWhellDirectionComboBox"]).Items.Add(StringParser.Parse("${res:Dialog.Options.IDEOptions.TextEditor.Behaviour.ReverseMouseDirectionRadioButton}"));
		((ComboBox)ControlDictionary["mouseWhellDirectionComboBox"]).SelectedIndex = ((!instance.MouseWheelScrollDown) ? 1 : 0);
	}

	public override bool StorePanelContents()
	{
		SharpDevelopTextEditorProperties instance = SharpDevelopTextEditorProperties.Instance;
		instance.ConvertTabsToSpaces = ((CheckBox)ControlDictionary["convertTabsToSpacesCheckBox"]).Checked;
		instance.MouseWheelScrollDown = ((ComboBox)ControlDictionary["mouseWhellDirectionComboBox"]).SelectedIndex == 0;
		instance.AutoInsertCurlyBracket = ((CheckBox)ControlDictionary["autoinsertCurlyBraceCheckBox"]).Checked;
		instance.HideMouseCursor = ((CheckBox)ControlDictionary["hideMouseCursorCheckBox"]).Checked;
		instance.AllowCaretBeyondEOL = ((CheckBox)ControlDictionary["caretBehindEOLCheckBox"]).Checked;
		instance.AutoInsertTemplates = ((CheckBox)ControlDictionary["auotInsertTemplatesCheckBox"]).Checked;
		instance.CutCopyWholeLine = ((CheckBox)ControlDictionary["cutCopyWholeLine"]).Checked;
		instance.IndentStyle = (IndentStyle)((ComboBox)ControlDictionary["indentStyleComboBox"]).SelectedIndex;
		try
		{
			int num = int.Parse(ControlDictionary["tabSizeTextBox"].Text);
			if (num > 0)
			{
				instance.TabIndent = num;
			}
		}
		catch (Exception)
		{
		}
		try
		{
			instance.IndentationSize = int.Parse(ControlDictionary["indentSizeTextBox"].Text);
		}
		catch (Exception)
		{
		}
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ViewContent is ITextEditorControlProvider)
		{
			TextEditorControl textEditorControl = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
			textEditorControl.OptionsChanged();
		}
		return true;
	}
}
