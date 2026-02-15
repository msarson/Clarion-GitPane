using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class Signing : AbstractProjectOptionPanel
{
	private const string KeyFileExtensions = "*.snk;*.pfx;*.key";

	private ComboBox keyFile;

	private ConfigurationGuiBinding signAssemblyBinding;

	public override void LoadPanelContents()
	{
		SetupFromXmlResource("ProjectOptions.Signing.xfrm");
		InitializeHelper();
		signAssemblyBinding = helper.BindBoolean("signAssemblyCheckBox", "SignAssembly", defaultValue: false);
		ChooseStorageLocationButton btn = signAssemblyBinding.CreateLocationButtonInPanel("signingGroupBox");
		Get<CheckBox>("signAssembly").CheckedChanged += UpdateEnabledStates;
		keyFile = Get<ComboBox>("keyFile");
		ConfigurationGuiBinding configurationGuiBinding = helper.BindString(keyFile, "AssemblyOriginatorKeyFile", TextBoxEditMode.EditEvaluatedProperty);
		configurationGuiBinding.RegisterLocationButton(btn);
		FindKeys(baseDirectory);
		if (keyFile.Text.Length > 0 && !keyFile.Items.Contains(keyFile.Text))
		{
			keyFile.Items.Add(keyFile.Text);
		}
		keyFile.Items.Add(StringParser.Parse("<${res:Global.CreateButtonText}...>"));
		keyFile.Items.Add(StringParser.Parse("<${res:Global.BrowseText}...>"));
		keyFile.SelectedIndexChanged += delegate
		{
			if (keyFile.SelectedIndex == keyFile.Items.Count - 1)
			{
				BeginInvoke(new MethodInvoker(BrowseKeyFile));
			}
			if (keyFile.SelectedIndex == keyFile.Items.Count - 2)
			{
				BeginInvoke(new MethodInvoker(CreateKeyFile));
			}
		};
		configurationGuiBinding = helper.BindBoolean("delaySignOnlyCheckBox", "DelaySign", defaultValue: false);
		configurationGuiBinding.RegisterLocationButton(btn);
		UpdateEnabledStates(this, EventArgs.Empty);
		helper.AddConfigurationSelector(this);
		helper.Saved += delegate
		{
			if (Get<CheckBox>("signAssembly").Checked)
			{
				helper.SetProperty("AssemblyOriginatorKeyMode", "File", treatPropertyValueAsLiteral: true, signAssemblyBinding.Location);
			}
		};
	}

	private void FindKeys(string directory)
	{
		directory = Path.GetFullPath(directory);
		while (true)
		{
			try
			{
				string[] files = Directory.GetFiles(directory, "*.snk");
				foreach (string absPath in files)
				{
					keyFile.Items.Add(FileUtility.GetRelativePath(baseDirectory, absPath));
				}
				string[] files2 = Directory.GetFiles(directory, "*.pfx");
				foreach (string absPath2 in files2)
				{
					keyFile.Items.Add(FileUtility.GetRelativePath(baseDirectory, absPath2));
				}
				string[] files3 = Directory.GetFiles(directory, "*.key");
				foreach (string absPath3 in files3)
				{
					keyFile.Items.Add(FileUtility.GetRelativePath(baseDirectory, absPath3));
				}
			}
			catch
			{
				break;
			}
			int num = directory.LastIndexOf(Path.DirectorySeparatorChar);
			if (num < 0)
			{
				break;
			}
			directory = directory.Substring(0, num);
		}
	}

	private void BrowseKeyFile()
	{
		keyFile.SelectedIndex = -1;
		new BrowseButtonEvent(this, "keyFileComboBox", "${res:SharpDevelop.FileFilter.KeyFiles} (*.snk;*.pfx;*.key)|*.snk;*.pfx;*.key|${res:SharpDevelop.FileFilter.AllFiles}|*.*", TextBoxEditMode.EditEvaluatedProperty).Event(this, EventArgs.Empty);
	}

	private void CreateKeyFile()
	{
		if (File.Exists(CreateKeyForm.StrongNameTool))
		{
			using CreateKeyForm createKeyForm = new CreateKeyForm(baseDirectory);
			createKeyForm.KeyFile = project.Name;
			if (createKeyForm.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK)
			{
				keyFile.Text = createKeyForm.KeyFile;
				return;
			}
		}
		else
		{
			MessageService.ShowMessage("${res:Dialog.ProjectOptions.Signing.SNnotFound}");
		}
		keyFile.Text = "";
	}

	private void UpdateEnabledStates(object sender, EventArgs e)
	{
		ControlDictionary["strongNameSignPanel"].Enabled = Get<CheckBox>("signAssembly").Checked;
		Get<Button>("changePassword").Enabled = false;
	}
}
