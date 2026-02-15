using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class DebugOptions : AbstractProjectOptionPanel
{
	public override void LoadPanelContents()
	{
		SetupFromXmlResource("ProjectOptions.DebugOptions.xfrm");
		ConnectBrowseButton("startExternalProgramBrowseButton", "startExternalProgramTextBox", "${res:SharpDevelop.FileFilter.ExecutableFiles}|*.exe;*.com;*.pif;*.bat;*.cmd", TextBoxEditMode.EditEvaluatedProperty);
		ConnectBrowseFolder("workingDirectoryBrowseButton", "workingDirectoryTextBox", TextBoxEditMode.EditEvaluatedProperty);
		InitializeHelper();
		ConfigurationGuiBinding configurationGuiBinding = helper.BindRadioEnum<StartAction>("StartAction", new KeyValuePair<StartAction, RadioButton>(StartAction.Project, Get<RadioButton>("startProject")), new KeyValuePair<StartAction, RadioButton>(StartAction.Program, Get<RadioButton>("startExternalProgram")), new KeyValuePair<StartAction, RadioButton>(StartAction.StartURL, Get<RadioButton>("startBrowserInURL")));
		configurationGuiBinding.DefaultLocation = PropertyStorageLocations.ConfigurationSpecific;
		ChooseStorageLocationButton btn = configurationGuiBinding.CreateLocationButtonInPanel("startActionGroupBox");
		configurationGuiBinding = helper.BindString("startExternalProgramTextBox", "StartProgram", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.DefaultLocation = PropertyStorageLocations.ConfigurationSpecific;
		configurationGuiBinding.RegisterLocationButton(btn);
		configurationGuiBinding = helper.BindString("startBrowserInURLTextBox", "StartURL", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.DefaultLocation = PropertyStorageLocations.ConfigurationSpecific;
		configurationGuiBinding.RegisterLocationButton(btn);
		Get<RadioButton>("startExternalProgram").CheckedChanged += UpdateEnabledStates;
		Get<RadioButton>("startBrowserInURL").CheckedChanged += UpdateEnabledStates;
		configurationGuiBinding = helper.BindString("commandLineArgumentsTextBox", "StartArguments", TextBoxEditMode.EditEvaluatedProperty);
		btn = configurationGuiBinding.CreateLocationButtonInPanel("startOptionsGroupBox");
		configurationGuiBinding = helper.BindString("workingDirectoryTextBox", "StartWorkingDirectory", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.RegisterLocationButton(btn);
		UpdateEnabledStates(this, EventArgs.Empty);
		helper.AddConfigurationSelector(this);
	}

	private void UpdateEnabledStates(object sender, EventArgs e)
	{
		TextBox textBox = Get<TextBox>("startExternalProgram");
		bool enabled = (Get<Button>("startExternalProgramBrowse").Enabled = Get<RadioButton>("startExternalProgram").Checked);
		textBox.Enabled = enabled;
		Get<TextBox>("startBrowserInURL").Enabled = Get<RadioButton>("startBrowserInURL").Checked;
	}
}
