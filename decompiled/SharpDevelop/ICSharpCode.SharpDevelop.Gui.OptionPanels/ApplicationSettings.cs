using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class ApplicationSettings : AbstractProjectOptionPanel
{
	public override void LoadPanelContents()
	{
		SetupFromXmlResource("ProjectOptions.ApplicationSettings.xfrm");
		InitializeHelper();
		ConnectBrowseButton("applicationIconBrowseButton", "applicationIconComboBox", "${res:SharpDevelop.FileFilter.Icons}|*.ico|${res:SharpDevelop.FileFilter.AllFiles}|*.*", TextBoxEditMode.EditEvaluatedProperty);
		ConnectBrowseButton("win32ResourceFileBrowseButton", "win32ResourceFileComboBox", "${res:SharpDevelop.FileFilter.AllFiles}|*.*", TextBoxEditMode.EditEvaluatedProperty);
		ConfigurationGuiBinding configurationGuiBinding = helper.BindString("assemblyNameTextBox", "AssemblyName", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.CreateLocationButton("assemblyNameTextBox");
		Get<TextBox>("assemblyName").TextChanged += RefreshOutputNameTextBox;
		configurationGuiBinding = helper.BindString("rootNamespaceTextBox", "RootNamespace", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.CreateLocationButton("rootNamespaceTextBox");
		configurationGuiBinding = helper.BindEnum("outputTypeComboBox", "OutputType", new OutputType[0]);
		ChooseStorageLocationButton btn = configurationGuiBinding.CreateLocationButton("outputTypeComboBox");
		Get<ComboBox>("outputType").SelectedIndexChanged += RefreshOutputNameTextBox;
		configurationGuiBinding = helper.BindString("startupObjectComboBox", "StartupObject", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.RegisterLocationButton(btn);
		configurationGuiBinding = helper.BindString("applicationIconComboBox", "ApplicationIcon", TextBoxEditMode.EditEvaluatedProperty);
		Get<ComboBox>("applicationIcon").TextChanged += ApplicationIconComboBoxTextChanged;
		configurationGuiBinding.CreateLocationButton("applicationIconComboBox");
		configurationGuiBinding = helper.BindString("win32ResourceFileComboBox", "Win32Resource", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.CreateLocationButton("win32ResourceFileComboBox");
		Get<TextBox>("projectFolder").Text = project.Directory;
		Get<TextBox>("projectFile").Text = Path.GetFileName(project.FileName);
		Get<TextBox>("projectFile").ReadOnly = true;
		RefreshOutputNameTextBox(null, EventArgs.Empty);
		helper.AddConfigurationSelector(this);
	}

	private void RefreshOutputNameTextBox(object sender, EventArgs e)
	{
		Get<TextBox>("outputName").Text = Get<TextBox>("assemblyName").Text + CompilableProject.GetExtension((OutputType)Get<ComboBox>("outputType").SelectedIndex);
	}

	private void ApplicationIconComboBoxTextChanged(object sender, EventArgs e)
	{
		if (!FileUtility.IsValidFileName(Get<ComboBox>("applicationIcon").Text))
		{
			return;
		}
		string text = Path.Combine(baseDirectory, Get<ComboBox>("applicationIcon").Text);
		if (File.Exists(text))
		{
			try
			{
				Get<PictureBox>("applicationIcon").Image = Image.FromFile(text);
				return;
			}
			catch (OutOfMemoryException)
			{
				Get<PictureBox>("applicationIcon").Image = null;
				MessageService.ShowErrorFormatted("${res:Dialog.ProjectOptions.ApplicationSettings.InvalidIconFile}", Path.GetFullPath(text));
				return;
			}
		}
		Get<PictureBox>("applicationIcon").Image = null;
	}
}
