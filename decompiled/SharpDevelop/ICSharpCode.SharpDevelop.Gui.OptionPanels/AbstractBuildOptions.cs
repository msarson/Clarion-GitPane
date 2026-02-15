using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class AbstractBuildOptions : AbstractProjectOptionPanel
{
	protected class WarningsAsErrorsBinding : ConfigurationGuiBinding
	{
		private RadioButton none;

		private RadioButton specific;

		private RadioButton all;

		private Control specificWarningsTextBox;

		public WarningsAsErrorsBinding(AbstractProjectOptionPanel panel)
		{
			none = panel.Get<RadioButton>("none");
			specific = panel.Get<RadioButton>("specificWarnings");
			all = panel.Get<RadioButton>("all");
			specificWarningsTextBox = panel.ControlDictionary["specificWarningsTextBox"];
		}

		public override void Load()
		{
			if (bool.Parse(Get("false")))
			{
				all.Checked = true;
			}
			else if (base.Helper.GetProperty("WarningsAsErrors", "", treatPropertyValueAsLiteral: true).Length > 0)
			{
				specific.Checked = true;
			}
			else
			{
				none.Checked = true;
			}
		}

		public override bool Save()
		{
			if (none.Checked)
			{
				specificWarningsTextBox.Text = "";
			}
			if (all.Checked)
			{
				Set("true");
			}
			else
			{
				Set("false");
			}
			return true;
		}
	}

	private ConfigurationGuiBinding debugInfoBinding;

	protected ChooseStorageLocationButton advancedLocationButton;

	protected void InitBaseIntermediateOutputPath()
	{
		helper.BindString(Get<TextBox>("baseIntermediateOutputPath"), "BaseIntermediateOutputPath", TextBoxEditMode.EditRawProperty, () => "obj\\").CreateLocationButton("baseIntermediateOutputPathTextBox");
		ConnectBrowseFolder("baseIntermediateOutputPathBrowseButton", "baseIntermediateOutputPathTextBox", "${res:Dialog.Options.PrjOptions.Configuration.FolderBrowserDescription}", TextBoxEditMode.EditRawProperty);
	}

	protected void InitIntermediateOutputPath()
	{
		ConfigurationGuiBinding configurationGuiBinding = helper.BindString(Get<TextBox>("intermediateOutputPath"), "IntermediateOutputPath", TextBoxEditMode.EditRawProperty, () => Path.Combine(helper.GetProperty("BaseIntermediateOutputPath", "obj\\", treatPropertyValueAsLiteral: true), helper.Configuration));
		configurationGuiBinding.DefaultLocation = PropertyStorageLocations.ConfigurationSpecific;
		configurationGuiBinding.CreateLocationButton("intermediateOutputPathTextBox");
		ConnectBrowseFolder("intermediateOutputPathBrowseButton", "intermediateOutputPathTextBox", "${res:Dialog.Options.PrjOptions.Configuration.FolderBrowserDescription}", TextBoxEditMode.EditRawProperty);
	}

	protected void InitOutputPath()
	{
		helper.BindString("outputPathTextBox", "OutputPath", TextBoxEditMode.EditRawProperty).CreateLocationButton("outputPathTextBox");
		ConnectBrowseFolder("outputPathBrowseButton", "outputPathTextBox", "${res:Dialog.Options.PrjOptions.Configuration.FolderBrowserDescription}", TextBoxEditMode.EditRawProperty);
	}

	protected void InitXmlDoc()
	{
		ConfigurationGuiBinding configurationGuiBinding = helper.BindString("xmlDocumentationTextBox", "DocumentationFile", TextBoxEditMode.EditRawProperty);
		configurationGuiBinding.CreateLocationButton("xmlDocumentationCheckBox");
		helper.Loaded += XmlDocHelperLoaded;
		XmlDocHelperLoaded(null, null);
	}

	private void XmlDocHelperLoaded(object sender, EventArgs e)
	{
		Get<CheckBox>("xmlDocumentation").CheckedChanged -= UpdateXmlEnabled;
		Get<CheckBox>("xmlDocumentation").Checked = Get<TextBox>("xmlDocumentation").Text.Length > 0;
		Get<CheckBox>("xmlDocumentation").CheckedChanged += UpdateXmlEnabled;
		Get<TextBox>("xmlDocumentation").Enabled = Get<CheckBox>("xmlDocumentation").Checked;
	}

	private void UpdateXmlEnabled(object sender, EventArgs e)
	{
		Get<TextBox>("xmlDocumentation").Enabled = Get<CheckBox>("xmlDocumentation").Checked;
		if (Get<CheckBox>("xmlDocumentation").Checked)
		{
			if (Get<TextBox>("xmlDocumentation").Text.Length == 0)
			{
				Get<TextBox>("xmlDocumentation").Text = Path.ChangeExtension(FileUtility.GetRelativePath(baseDirectory, project.OutputAssemblyFullPath), ".xml");
			}
		}
		else
		{
			Get<TextBox>("xmlDocumentation").Text = "";
		}
	}

	protected void InitWarnings()
	{
		ConfigurationGuiBinding configurationGuiBinding = helper.BindStringEnum("warningLevelComboBox", "WarningLevel", "4", new KeyValuePair<string, string>("0", "0"), new KeyValuePair<string, string>("1", "1"), new KeyValuePair<string, string>("2", "2"), new KeyValuePair<string, string>("3", "3"), new KeyValuePair<string, string>("4", "4"));
		ChooseStorageLocationButton btn = configurationGuiBinding.CreateLocationButtonInPanel("warningsGroupBox");
		configurationGuiBinding = helper.BindString("suppressWarningsTextBox", "NoWarn", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.RegisterLocationButton(btn);
		configurationGuiBinding = new WarningsAsErrorsBinding(this);
		helper.AddBinding("TreatWarningsAsErrors", configurationGuiBinding);
		btn = configurationGuiBinding.CreateLocationButtonInPanel("treatWarningsAsErrorsGroupBox");
		configurationGuiBinding = helper.BindString("specificWarningsTextBox", "WarningsAsErrors", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.RegisterLocationButton(btn);
		EventHandler value = delegate
		{
			helper.IsDirty = true;
		};
		Get<RadioButton>("none").CheckedChanged += value;
		Get<RadioButton>("specificWarnings").CheckedChanged += value;
		Get<RadioButton>("all").CheckedChanged += value;
		Get<RadioButton>("specificWarnings").CheckedChanged += UpdateWarningChecked;
		UpdateWarningChecked(this, EventArgs.Empty);
	}

	private void UpdateWarningChecked(object sender, EventArgs e)
	{
		Get<TextBox>("specificWarnings").Enabled = Get<RadioButton>("specificWarnings").Checked;
	}

	protected void InitDebugInfo()
	{
		debugInfoBinding = helper.BindEnum("debugInfoComboBox", "DebugType", new DebugSymbolType[0]);
		debugInfoBinding.CreateLocationButton("debugInfoLabel");
		DebugSymbolsLoaded(null, null);
		helper.Loaded += DebugSymbolsLoaded;
		helper.Saved += DebugSymbolsSave;
	}

	protected void InitAdvanced()
	{
		ConfigurationGuiBinding configurationGuiBinding = helper.BindBoolean("registerCOMInteropCheckBox", "RegisterForComInterop", defaultValue: false);
		configurationGuiBinding.DefaultLocation = PropertyStorageLocations.PlatformSpecific;
		advancedLocationButton = configurationGuiBinding.CreateLocationButtonInPanel("platformSpecificOptionsPanel");
		configurationGuiBinding = helper.BindStringEnum("generateSerializationAssemblyComboBox", "GenerateSerializationAssemblies", "Auto", new KeyValuePair<string, string>("Off", "${res:Dialog.ProjectOptions.Build.Off}"), new KeyValuePair<string, string>("On", "${res:Dialog.ProjectOptions.Build.On}"), new KeyValuePair<string, string>("Auto", "${res:Dialog.ProjectOptions.Build.Auto}"));
		configurationGuiBinding.DefaultLocation = PropertyStorageLocations.PlatformSpecific;
		configurationGuiBinding.RegisterLocationButton(advancedLocationButton);
		configurationGuiBinding = helper.BindHexadecimal(Get<TextBox>("dllBaseAddress"), "BaseAddress", 4194304);
		configurationGuiBinding.DefaultLocation = PropertyStorageLocations.PlatformSpecific;
		configurationGuiBinding.RegisterLocationButton(advancedLocationButton);
		configurationGuiBinding = helper.BindStringEnum("targetCpuComboBox", "PlatformTarget", "AnyCPU", new KeyValuePair<string, string>("AnyCPU", "${res:Dialog.ProjectOptions.Build.TargetCPU.Any}"), new KeyValuePair<string, string>("x86", "${res:Dialog.ProjectOptions.Build.TargetCPU.x86}"), new KeyValuePair<string, string>("x64", "${res:Dialog.ProjectOptions.Build.TargetCPU.x64}"), new KeyValuePair<string, string>("Itanium", "${res:Dialog.ProjectOptions.Build.TargetCPU.Itanium}"));
		configurationGuiBinding.DefaultLocation = PropertyStorageLocations.PlatformSpecific;
		configurationGuiBinding.RegisterLocationButton(advancedLocationButton);
	}

	private void DebugSymbolsLoaded(object sender, EventArgs e)
	{
		helper.GetProperty("DebugType", "", treatPropertyValueAsLiteral: true, out var location);
		if (location == PropertyStorageLocations.Unchanged)
		{
			bool property = helper.GetProperty("DebugSymbols", defaultValue: false, treatPropertyValueAsLiteral: true, out location);
			if (location != PropertyStorageLocations.Unchanged)
			{
				debugInfoBinding.Location = location;
				helper.SetProperty("DebugType", property ? DebugSymbolType.Full : DebugSymbolType.None, treatPropertyValueAsLiteral: true, location);
				debugInfoBinding.Load();
			}
		}
	}

	private void DebugSymbolsSave(object sender, EventArgs e)
	{
		if (Get<ComboBox>("debugInfo").SelectedIndex == 1)
		{
			helper.SetProperty("DebugSymbols", "true", treatPropertyValueAsLiteral: true, debugInfoBinding.Location);
		}
		else
		{
			helper.SetProperty("DebugSymbols", "false", treatPropertyValueAsLiteral: true, debugInfoBinding.Location);
		}
	}

	protected void InitTargetFramework(string defaultTargets, string extendedTargets)
	{
		ConfigurationGuiBinding configurationGuiBinding = helper.BindStringEnum("targetFrameworkComboBox", "TargetFrameworkVersion", "", new KeyValuePair<string, string>("v4.0", "Default (.NET 4.0)"), new KeyValuePair<string, string>("v2.0", ".NET Framework 2.0"), new KeyValuePair<string, string>("v3.0", ".NET Framework 3.0"), new KeyValuePair<string, string>("v3.5", ".NET Framework 3.5"));
		configurationGuiBinding.CreateLocationButton("targetFrameworkLabel");
		helper.Saved += delegate
		{
			MSBuildBasedProject mSBuildBasedProject = helper.Project;
			bool flag = false;
			foreach (BuildProperty allProperty in mSBuildBasedProject.GetAllProperties("TargetFrameworkVersion"))
			{
				if (!allProperty.IsImported && allProperty.Value.Length > 0)
				{
					flag = true;
					break;
				}
			}
			foreach (Import import in mSBuildBasedProject.MSBuildProject.Imports)
			{
				if (flag)
				{
					if (defaultTargets.Equals(import.ProjectPath, StringComparison.InvariantCultureIgnoreCase))
					{
						MSBuildInternals.SetImportProjectPath(mSBuildBasedProject, import, extendedTargets);
						break;
					}
				}
				else if (extendedTargets.Equals(import.ProjectPath, StringComparison.InvariantCultureIgnoreCase))
				{
					MSBuildInternals.SetImportProjectPath(mSBuildBasedProject, import, defaultTargets);
					break;
				}
			}
		};
	}
}
