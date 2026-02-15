using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using Microsoft.Build.Framework;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class ProjectAndSolutionOptionsPanel : AbstractOptionPanel
{
	public override void LoadPanelContents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.ProjectAndSolutionOptionsPanel.xfrm"));
		ControlDictionary["projectLocationTextBox"].Text = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.DefaultPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Clarion Projects")).ToString();
		if (VersionService.Version == IDEVersion.Enterprise || VersionService.Version == IDEVersion.Standard)
		{
			((CheckBox)ControlDictionary["killProcessCheckBox"]).Visible = true;
			((CheckBox)ControlDictionary["killProcessCheckBox"]).Checked = PropertyService.Get("SharpDevelop.KillProcessBeforeBuild", defaultValue: true);
			((CheckBox)ControlDictionary["queryBeforeKillRunningTargetCheckBox"]).Visible = true;
			((CheckBox)ControlDictionary["queryBeforeKillRunningTargetCheckBox"]).Checked = PropertyService.Get("SharpDevelop.QueryBeforeKillRunningTarget", defaultValue: true);
		}
		else
		{
			((CheckBox)ControlDictionary["killProcessCheckBox"]).Visible = false;
			((CheckBox)ControlDictionary["queryBeforeKillRunningTargetCheckBox"]).Visible = false;
		}
		((CheckBox)ControlDictionary["killCCCEXECheckBox"]).Checked = PropertyService.Get("SharpDevelop.KillCCCEXEBeforeBuild", defaultValue: true);
		((CheckBox)ControlDictionary["useLastDirCheckBox"]).Checked = PropertyService.Get("SharpDevelop.UseLastSolutionFolderForDefault", defaultValue: true);
		((CheckBox)ControlDictionary["useSolutionFolderCheckBox"]).Checked = PropertyService.Get("SharpDevelop.UseSolutionFolderInsteadDefault", defaultValue: true);
		((CheckBox)ControlDictionary["loadPrevProjectCheckBox"]).Checked = PropertyService.Get("SharpDevelop.LoadPrevProjectOnStartup", defaultValue: false);
		((CheckBox)ControlDictionary["treatUnrelatedFilesAsSolutionsCheckBox"]).Checked = PropertyService.Get("SharpDevelop.TreatUnrelatedFilesAsSolutions", defaultValue: false);
		((CheckBox)ControlDictionary["closeStartPageCheckBox"]).Checked = PropertyService.Get("SharpDevelop.CloseStartPageOnSolutionOpening", defaultValue: true);
		((CheckBox)ControlDictionary["showErrorListCheckBox"]).Checked = ErrorListPad.ShouldShowAfterBuild;
		((CheckBox)ControlDictionary["pinIfErrorsErrorListPadCheckBox"]).Checked = ErrorListPad.ShowAndPinIfErrors;
		((CheckBox)ControlDictionary["buildViaProcessCheckBox"]).Checked = PropertyService.Get("SharpDevelop.BuildAsProcess", defaultValue: true);
		((CheckBox)ControlDictionary["useReleaseCheckBox"]).Checked = PropertyService.Get("SharpDevelop.UseReleaseAsDefault", defaultValue: false);
		((CheckBox)ControlDictionary["readOnlyProjectSaveCheckBox"]).Checked = PropertyService.Get("SharpDevelop.ReadOnlyPrjWarning", defaultValue: true);
		((CheckBox)ControlDictionary["silentReadOnlyWarningsCheckBox"]).Checked = PropertyService.Get("SharpDevelop.SilentReadOnlyWarnings", defaultValue: true);
		((Button)ControlDictionary["selectProjectLocationButton"]).Click += SelectProjectLocationButtonClicked;
		ComboBox comboBox = (ComboBox)ControlDictionary["outputDetailsComboBox"];
		string text = PropertyService.Get("SharpDevelop.LoggerVerbosity", LoggerVerbosity.Minimal.ToString());
		foreach (LoggerVerbosity value in Enum.GetValues(typeof(LoggerVerbosity)))
		{
			comboBox.Items.Add(value);
			if (value.ToString() == text)
			{
				comboBox.SelectedItem = value;
			}
		}
	}

	public override bool StorePanelContents()
	{
		string text = ControlDictionary["projectLocationTextBox"].Text;
		if (text.Length > 0 && !FileUtility.IsValidFileName(text))
		{
			MessageService.ShowError(StringParser.Parse("${res:Dialog.Options.IDEOptions.ProjectAndSolutionOptions.InvalidProjectPathSpecified}"));
			return false;
		}
		PropertyService.Set("ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.DefaultPath", text);
		PropertyService.Set("SharpDevelop.UseSolutionFolderInsteadDefault", ((CheckBox)ControlDictionary["useSolutionFolderCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.LoadPrevProjectOnStartup", ((CheckBox)ControlDictionary["loadPrevProjectCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.TreatUnrelatedFilesAsSolutions", ((CheckBox)ControlDictionary["treatUnrelatedFilesAsSolutionsCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.CloseStartPageOnSolutionOpening", ((CheckBox)ControlDictionary["closeStartPageCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.LoggerVerbosity", ((ComboBox)ControlDictionary["outputDetailsComboBox"]).Text);
		PropertyService.Set("SharpDevelop.BuildAsProcess", ((CheckBox)ControlDictionary["buildViaProcessCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.UseLastSolutionFolderForDefault", ((CheckBox)ControlDictionary["useLastDirCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.UseReleaseAsDefault", ((CheckBox)ControlDictionary["useReleaseCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.ReadOnlyPrjWarning", ((CheckBox)ControlDictionary["readOnlyProjectSaveCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.SilentReadOnlyWarnings", ((CheckBox)ControlDictionary["silentReadOnlyWarningsCheckBox"]).Checked);
		PropertyService.Set("SharpDevelop.KillCCCEXEBeforeBuild", ((CheckBox)ControlDictionary["killCCCEXECheckBox"]).Checked);
		ErrorListPad.ShouldShowAfterBuild = ((CheckBox)ControlDictionary["showErrorListCheckBox"]).Checked;
		ErrorListPad.ShowAndPinIfErrors = ((CheckBox)ControlDictionary["pinIfErrorsErrorListPadCheckBox"]).Checked;
		if (VersionService.Version == IDEVersion.Enterprise || VersionService.Version == IDEVersion.Standard)
		{
			PropertyService.Set("SharpDevelop.KillProcessBeforeBuild", ((CheckBox)ControlDictionary["killProcessCheckBox"]).Checked);
			PropertyService.Set("SharpDevelop.QueryBeforeKillRunningTarget", ((CheckBox)ControlDictionary["queryBeforeKillRunningTargetCheckBox"]).Checked);
		}
		return true;
	}

	private void SelectProjectLocationButtonClicked(object sender, EventArgs e)
	{
		TextBox textBox = (TextBox)ControlDictionary["projectLocationTextBox"];
		using FolderBrowserDialog folderBrowserDialog = FileService.CreateFolderBrowserDialog("${res:Dialog.Options.IDEOptions.ProjectAndSolutionOptions.SelectDefaultProjectLocationDialog.Title}", textBox.Text);
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			textBox.Text = folderBrowserDialog.SelectedPath;
		}
	}
}
