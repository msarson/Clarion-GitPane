using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class LoadSavePanel : AbstractOptionPanel
{
	private const string loadUserDataCheckBox = "loadUserDataCheckBox";

	private const string createBackupCopyCheckBox = "createBackupCopyCheckBox";

	private const string lineTerminatorStyleComboBox = "lineTerminatorStyleComboBox";

	private CheckBox autoLoadExternalChangesCheckBox;

	private CheckBox detectExternalChangesCheckBox;

	private CheckBox deleteWillSendToRecycleBinCheckBox;

	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.LoadSaveOptionPanel.xfrm"));
		((CheckBox)ControlDictionary["loadUserDataCheckBox"]).Checked = PropertyService.Get("SharpDevelop.LoadDocumentProperties", defaultValue: true);
		((CheckBox)ControlDictionary["createBackupCopyCheckBox"]).Checked = PropertyService.Get("SharpDevelop.CreateBackupCopy", defaultValue: false);
		((ComboBox)ControlDictionary["lineTerminatorStyleComboBox"]).Items.Add(StringParser.Parse("${res:Dialog.Options.IDEOptions.LoadSaveOptions.WindowsRadioButton}"));
		((ComboBox)ControlDictionary["lineTerminatorStyleComboBox"]).Items.Add(StringParser.Parse("${res:Dialog.Options.IDEOptions.LoadSaveOptions.MacintoshRadioButton}"));
		((ComboBox)ControlDictionary["lineTerminatorStyleComboBox"]).Items.Add(StringParser.Parse("${res:Dialog.Options.IDEOptions.LoadSaveOptions.UnixRadioButton}"));
		((ComboBox)ControlDictionary["lineTerminatorStyleComboBox"]).SelectedIndex = (int)PropertyService.Get("SharpDevelop.LineTerminatorStyle", LineTerminatorStyle.Windows);
		((NumericUpDown)ControlDictionary["MaxRecentNumericUpDown"]).Value = PropertyService.Get("MaximumRecentEntries", 16);
		Get<CheckBox>("openDialogUsesRedFile").Checked = FileDialogService.UseRedirectionFile;
		((TextBox)ControlDictionary["readOnlyTextTextBox"]).Text = PropertyService.Get("FileReadOnlyText", "+");
		autoLoadExternalChangesCheckBox = Get<CheckBox>("autoLoadExternalChanges");
		detectExternalChangesCheckBox = Get<CheckBox>("detectExternalChanges");
		deleteWillSendToRecycleBinCheckBox = Get<CheckBox>("deleteWillSendToRecycleBin");
		detectExternalChangesCheckBox.CheckedChanged += delegate
		{
			autoLoadExternalChangesCheckBox.Enabled = detectExternalChangesCheckBox.Checked;
		};
		autoLoadExternalChangesCheckBox.Enabled = detectExternalChangesCheckBox.Checked;
		detectExternalChangesCheckBox.Checked = TextEditorDisplayBindingWrapper.FileChangeWatcher.DetectExternalChangesOption;
		autoLoadExternalChangesCheckBox.Checked = TextEditorDisplayBindingWrapper.FileChangeWatcher.AutoLoadExternalChangesOption;
		deleteWillSendToRecycleBinCheckBox.Checked = PropertyService.Get("DeleteWillSendToRecycleBin", defaultValue: true);
		ToolTip toolTip = new ToolTip();
		toolTip.SetToolTip(ControlDictionary["openDialogUsesRedFileCheckBox"], StringParser.Parse("${res:Dialog.Options.IDEOptions.SelectStyle.OpenViaRedirectionCheckBox.ToolTip}"));
		toolTip.Active = true;
	}

	public override bool StorePanelContents()
	{
		PropertyService.Set("SharpDevelop.LoadDocumentProperties", ((CheckBox)ControlDictionary["loadUserDataCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.CreateBackupCopy", ((CheckBox)ControlDictionary["createBackupCopyCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.LineTerminatorStyle", (LineTerminatorStyle)((ComboBox)ControlDictionary["lineTerminatorStyleComboBox"]).SelectedIndex);
		PropertyService.Set("MaximumRecentEntries", (int)((NumericUpDown)ControlDictionary["MaxRecentNumericUpDown"]).Value);
		PropertyService.Set("DeleteWillSendToRecycleBin", deleteWillSendToRecycleBinCheckBox.Checked);
		TextEditorDisplayBindingWrapper.FileChangeWatcher.DetectExternalChangesOption = detectExternalChangesCheckBox.Checked;
		TextEditorDisplayBindingWrapper.FileChangeWatcher.AutoLoadExternalChangesOption = autoLoadExternalChangesCheckBox.Checked;
		FileDialogService.UseRedirectionFile = Get<CheckBox>("openDialogUsesRedFile").Checked;
		PropertyService.Set("SoftVelocity.Gui.FileDialog.RememberInitialDirectory", FileDialogService.UseRedirectionFile);
		if (((TextBox)ControlDictionary["readOnlyTextTextBox"]).Text.Trim() == "")
		{
			((TextBox)ControlDictionary["readOnlyTextTextBox"]).Text = "+";
		}
		PropertyService.Set("FileReadOnlyText", ((TextBox)ControlDictionary["readOnlyTextTextBox"]).Text);
		return true;
	}
}
