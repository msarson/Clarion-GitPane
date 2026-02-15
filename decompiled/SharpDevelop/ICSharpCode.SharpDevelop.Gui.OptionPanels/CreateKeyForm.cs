using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class CreateKeyForm : BaseSharpDevelopForm
{
	private string baseDirectory;

	public string KeyFile
	{
		get
		{
			return base.ControlDictionary["keyFileTextBox"].Text;
		}
		set
		{
			base.ControlDictionary["keyFileTextBox"].Text = value;
		}
	}

	public static string StrongNameTool => FileUtility.NetSdkInstallRoot + "bin\\sn.exe";

	public CreateKeyForm(string baseDirectory)
	{
		this.baseDirectory = baseDirectory;
		SetupFromXmlResource("ProjectOptions.CreateKey.xfrm");
		CheckBox checkBox = Get<CheckBox>("usePassword");
		EventHandler value = delegate
		{
			base.ControlDictionary["passwordPanel"].Enabled = Get<CheckBox>("usePassword").Checked;
		};
		checkBox.CheckedChanged += value;
		base.ControlDictionary["okButton"].Click += OkButtonClick;
	}

	private void OkButtonClick(object sender, EventArgs e)
	{
		KeyFile = KeyFile.Trim();
		if (KeyFile.Length == 0)
		{
			MessageService.ShowMessage("${res:Dialog.ProjectOptions.Signing.EnterKeyName}");
			return;
		}
		if (Get<CheckBox>("usePassword").Checked)
		{
			if (CheckPassword(base.ControlDictionary["passwordTextBox"], base.ControlDictionary["confirmPasswordTextBox"]))
			{
				MessageService.ShowMessage("Creating a key file with a password is currently not supported.");
			}
			return;
		}
		if (!KeyFile.EndsWith(".snk") && !KeyFile.EndsWith(".pfx"))
		{
			KeyFile += ".snk";
		}
		if (CreateKey(Path.Combine(baseDirectory, KeyFile)))
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	public static bool CreateKey(string keyPath)
	{
		if (File.Exists(keyPath))
		{
			string input = "${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.OverwriteQuestion}";
			input = StringParser.Parse(input, new string[1, 2] { { "fileName", keyPath } });
			if (!MessageService.AskQuestion(input, "${res:ICSharpCode.SharpDevelop.Internal.Templates.ProjectDescriptor.OverwriteQuestion.InfoName}"))
			{
				return false;
			}
		}
		Process process = Process.Start(StrongNameTool, "-k \"" + keyPath + "\"");
		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			MessageService.ShowMessage("${res:Dialog.ProjectOptions.Signing.ErrorCreatingKey}");
			return false;
		}
		return true;
	}

	public static bool CheckPassword(Control password, Control confirm)
	{
		password.Text = password.Text.Trim();
		confirm.Text = confirm.Text.Trim();
		if (password.Text.Length < 6)
		{
			MessageService.ShowMessage("${res:Dialog.ProjectOptions.Signing.PasswordTooShort}");
			password.Focus();
			return false;
		}
		if (password.Text != confirm.Text)
		{
			MessageService.ShowMessage("${res:Dialog.ProjectOptions.Signing.PasswordsDontMatch}");
			return false;
		}
		return true;
	}
}
