using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.OptionPanels;

public class MarkersTextEditorPanel : AbstractOptionPanel
{
	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.MarkersTextEditorPanel.xfrm"));
		SharpDevelopTextEditorProperties instance = SharpDevelopTextEditorProperties.Instance;
		((CheckBox)ControlDictionary["showLineNumberCheckBox"]).Checked = instance.ShowLineNumbers;
		((CheckBox)ControlDictionary["showInvalidLinesCheckBox"]).Checked = instance.ShowInvalidLines;
		((CheckBox)ControlDictionary["showBracketHighlighterCheckBox"]).Checked = instance.ShowMatchingBracket;
		((CheckBox)ControlDictionary["showErrorsCheckBox"]).Checked = instance.UnderlineErrors;
		((CheckBox)ControlDictionary["showHRulerCheckBox"]).Checked = instance.ShowHorizontalRuler;
		((CheckBox)ControlDictionary["showEOLMarkersCheckBox"]).Checked = instance.ShowEOLMarker;
		((CheckBox)ControlDictionary["showVRulerCheckBox"]).Checked = instance.ShowVerticalRuler;
		((CheckBox)ControlDictionary["showTabCharsCheckBox"]).Checked = instance.ShowTabs;
		((CheckBox)ControlDictionary["showSpaceCharsCheckBox"]).Checked = instance.ShowSpaces;
		ControlDictionary["vRulerRowTextBox"].Text = instance.VerticalRulerRow.ToString();
		((ComboBox)ControlDictionary["lineMarkerStyleComboBox"]).Items.Add(ResourceService.GetString("Dialog.Options.IDEOptions.TextEditor.Markers.LineViewerStyle.None"));
		((ComboBox)ControlDictionary["lineMarkerStyleComboBox"]).Items.Add(ResourceService.GetString("Dialog.Options.IDEOptions.TextEditor.Markers.LineViewerStyle.FullRow"));
		((ComboBox)ControlDictionary["lineMarkerStyleComboBox"]).SelectedIndex = (int)instance.LineViewerStyle;
		((ComboBox)ControlDictionary["bracketMatchingStyleComboBox"]).Items.Add(ResourceService.GetString("Dialog.Options.IDEOptions.TextEditor.Markers.BracketMatchingStyle.BeforeCaret"));
		((ComboBox)ControlDictionary["bracketMatchingStyleComboBox"]).Items.Add(ResourceService.GetString("Dialog.Options.IDEOptions.TextEditor.Markers.BracketMatchingStyle.AfterCaret"));
		((ComboBox)ControlDictionary["bracketMatchingStyleComboBox"]).SelectedIndex = (int)instance.BracketMatchingStyle;
	}

	public override bool StorePanelContents()
	{
		SharpDevelopTextEditorProperties instance = SharpDevelopTextEditorProperties.Instance;
		instance.ShowInvalidLines = ((CheckBox)ControlDictionary["showInvalidLinesCheckBox"]).Checked;
		instance.ShowLineNumbers = ((CheckBox)ControlDictionary["showLineNumberCheckBox"]).Checked;
		instance.ShowMatchingBracket = ((CheckBox)ControlDictionary["showBracketHighlighterCheckBox"]).Checked;
		instance.UnderlineErrors = ((CheckBox)ControlDictionary["showErrorsCheckBox"]).Checked;
		instance.ShowHorizontalRuler = ((CheckBox)ControlDictionary["showHRulerCheckBox"]).Checked;
		instance.ShowEOLMarker = ((CheckBox)ControlDictionary["showEOLMarkersCheckBox"]).Checked;
		instance.ShowVerticalRuler = ((CheckBox)ControlDictionary["showVRulerCheckBox"]).Checked;
		instance.ShowTabs = ((CheckBox)ControlDictionary["showTabCharsCheckBox"]).Checked;
		instance.ShowSpaces = ((CheckBox)ControlDictionary["showSpaceCharsCheckBox"]).Checked;
		try
		{
			instance.VerticalRulerRow = int.Parse(ControlDictionary["vRulerRowTextBox"].Text);
		}
		catch (Exception)
		{
		}
		instance.LineViewerStyle = (LineViewerStyle)((ComboBox)ControlDictionary["lineMarkerStyleComboBox"]).SelectedIndex;
		instance.BracketMatchingStyle = (BracketMatchingStyle)((ComboBox)ControlDictionary["bracketMatchingStyleComboBox"]).SelectedIndex;
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ViewContent is ITextEditorControlProvider)
		{
			TextEditorControl textEditorControl = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
			textEditorControl.OptionsChanged();
		}
		return true;
	}
}
