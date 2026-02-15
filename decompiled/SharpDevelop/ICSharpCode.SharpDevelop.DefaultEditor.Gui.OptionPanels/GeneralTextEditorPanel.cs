using System;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.TextEditor;
using SearchAndReplace;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.OptionPanels;

public class GeneralTextEditorPanel : AbstractOptionPanel
{
	private ComboBox fontListComboBox;

	private ComboBox fontSizeComboBox;

	private FontSelectionPanelHelper helper;

	private Font CurrentFont => helper.GetSelectedFont();

	private bool IsClearTypeEnabled
	{
		get
		{
			try
			{
				return SystemInformation.IsFontSmoothingEnabled && SystemInformation.FontSmoothingType >= 2;
			}
			catch (NotSupportedException)
			{
				return false;
			}
		}
	}

	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.GeneralTextEditorPanel.xfrm"));
		fontListComboBox = (ComboBox)ControlDictionary["fontListComboBox"];
		fontSizeComboBox = (ComboBox)ControlDictionary["fontSizeComboBox"];
		SharpDevelopTextEditorProperties instance = SharpDevelopTextEditorProperties.Instance;
		((CheckBox)ControlDictionary["enableFoldingCheckBox"]).Checked = instance.EnableFolding;
		((CheckBox)ControlDictionary["showQuickClassBrowserCheckBox"]).Checked = instance.ShowQuickClassBrowserPanel;
		((CheckBox)ControlDictionary["circularSearchCheckBox"]).Checked = instance.CircularSearch;
		if (IsClearTypeEnabled)
		{
			((CheckBox)ControlDictionary["enableAAFontRenderingCheckBox"]).Checked = true;
			((CheckBox)ControlDictionary["enableAAFontRenderingCheckBox"]).Enabled = false;
		}
		else
		{
			((CheckBox)ControlDictionary["enableAAFontRenderingCheckBox"]).Checked = instance.TextRenderingHint == TextRenderingHint.AntiAliasGridFit || instance.TextRenderingHint == TextRenderingHint.ClearTypeGridFit;
		}
		((CheckBox)ControlDictionary["mouseWheelZoomCheckBox"]).Checked = instance.MouseWheelTextZoom;
		foreach (string name in CharacterEncodings.Names)
		{
			((ComboBox)ControlDictionary["textEncodingComboBox"]).Items.Add(name);
		}
		int num = 0;
		try
		{
			num = CharacterEncodings.GetEncodingIndex(instance.EncodingCodePage);
		}
		catch
		{
			num = CharacterEncodings.GetEncodingIndex(Encoding.UTF8.CodePage);
		}
		((ComboBox)ControlDictionary["textEncodingComboBox"]).SelectedIndex = num;
		for (int i = 6; i <= 24; i++)
		{
			fontSizeComboBox.Items.Add(i);
		}
		((NumericUpDown)ControlDictionary["searchMaxNumberOfFindPatternsNumericUpDown"]).Value = SearchOptions.MaxNumberOfFindPatterns;
		fontSizeComboBox.TextChanged += UpdateFontPreviewLabel;
		fontSizeComboBox.Enabled = false;
		fontListComboBox.Enabled = false;
		fontListComboBox.TextChanged += UpdateFontPreviewLabel;
		fontListComboBox.SelectedIndexChanged += UpdateFontPreviewLabel;
		Font font = FontService.GetFont(FontService.FontType.TextEditor);
		helper = new FontSelectionPanelHelper(fontSizeComboBox, fontListComboBox, font);
		fontListComboBox.MeasureItem += helper.MeasureComboBoxItem;
		fontListComboBox.DrawItem += helper.ComboBoxDrawItem;
		UpdateFontPreviewLabel(null, null);
		helper.StartThread();
	}

	private void UpdateFontPreviewLabel(object sender, EventArgs e)
	{
		helper.UpdateFontPreviewLabel(ControlDictionary["fontPreviewLabel"]);
	}

	public override bool StorePanelContents()
	{
		SharpDevelopTextEditorProperties instance = SharpDevelopTextEditorProperties.Instance;
		if (((CheckBox)ControlDictionary["enableAAFontRenderingCheckBox"]).Enabled)
		{
			instance.TextRenderingHint = (((CheckBox)ControlDictionary["enableAAFontRenderingCheckBox"]).Checked ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.SystemDefault);
		}
		else
		{
			instance.TextRenderingHint = TextRenderingHint.SystemDefault;
		}
		instance.MouseWheelTextZoom = ((CheckBox)ControlDictionary["mouseWheelZoomCheckBox"]).Checked;
		instance.EnableFolding = ((CheckBox)ControlDictionary["enableFoldingCheckBox"]).Checked;
		instance.CircularSearch = ((CheckBox)ControlDictionary["circularSearchCheckBox"]).Checked;
		Font currentFont = CurrentFont;
		if (currentFont != null)
		{
			FontService.SetFont(FontService.FontType.TextEditor, currentFont);
		}
		instance.EncodingCodePage = CharacterEncodings.GetCodePageByIndex(((ComboBox)ControlDictionary["textEncodingComboBox"]).SelectedIndex);
		instance.ShowQuickClassBrowserPanel = ((CheckBox)ControlDictionary["showQuickClassBrowserCheckBox"]).Checked;
		SearchOptions.MaxNumberOfFindPatterns = (int)((NumericUpDown)ControlDictionary["searchMaxNumberOfFindPatternsNumericUpDown"]).Value;
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ViewContent is ITextEditorControlProvider)
		{
			TextEditorControl textEditorControl = ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
			textEditorControl.OptionsChanged();
		}
		return true;
	}
}
