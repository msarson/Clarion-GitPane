using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class SaveErrorInformDialog : Form
{
	private Label descriptionLabel;

	private TextBox descriptionTextBox;

	private Button exceptionButton;

	private Button okButton;

	private string displayMessage;

	private Exception exceptionGot;

	public SaveErrorInformDialog(string fileName, string message, string dialogName, Exception exceptionGot)
	{
		Text = StringParser.Parse(dialogName);
		InitializeComponent2();
		RightToLeftConverter.ConvertRecursive(this);
		string text = "";
		string text2 = fileName;
		try
		{
			text = Path.GetDirectoryName(fileName);
		}
		catch
		{
		}
		try
		{
			text2 = Path.GetFileName(fileName);
		}
		catch
		{
		}
		displayMessage = StringParser.Parse(message, new string[4, 2]
		{
			{ "FileName", fileName },
			{ "Path", text },
			{ "FileNameWithoutPath", text2 },
			{
				"Exception",
				exceptionGot.GetType().FullName
			}
		});
		descriptionTextBox.Lines = displayMessage.Split('\n');
		this.exceptionGot = exceptionGot;
	}

	private void ShowException(object sender, EventArgs e)
	{
		MessageService.ShowMessage(exceptionGot.ToString(), "Exception got");
	}

	private void InitializeComponent2()
	{
		base.ClientSize = new Size(508, 320);
		SuspendLayout();
		descriptionLabel = new Label();
		descriptionLabel.Location = new Point(8, 8);
		descriptionLabel.Size = new Size(584, 24);
		descriptionLabel.TabIndex = 3;
		descriptionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		descriptionLabel.TextAlign = ContentAlignment.BottomLeft;
		descriptionLabel.Text = StringParser.Parse("${res:ICSharpCode.Core.Services.ErrorDialogs.DescriptionLabel}");
		descriptionLabel.Name = "descriptionLabel";
		base.Controls.Add(descriptionLabel);
		descriptionTextBox = new TextBox();
		descriptionTextBox.Name = "descriptionTextBox";
		descriptionTextBox.Multiline = true;
		descriptionTextBox.Size = new Size(584, 237);
		descriptionTextBox.Location = new Point(8, 40);
		descriptionTextBox.TabIndex = 2;
		descriptionTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		descriptionTextBox.ReadOnly = true;
		base.Controls.Add(descriptionTextBox);
		exceptionButton = new Button();
		exceptionButton.TabIndex = 1;
		exceptionButton.Name = "exceptionButton";
		exceptionButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		exceptionButton.Text = StringParser.Parse("${res:ICSharpCode.Core.Services.ErrorDialogs.ShowExceptionButton}");
		exceptionButton.Size = new Size(120, 27);
		exceptionButton.Location = new Point(372, 285);
		exceptionButton.Click += ShowException;
		base.Controls.Add(exceptionButton);
		okButton = new Button();
		okButton.Name = "okButton";
		okButton.TabIndex = 0;
		okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		okButton.Text = StringParser.Parse("${res:Global.OKButtonText}");
		okButton.Size = new Size(120, 27);
		okButton.Location = new Point(244, 285);
		okButton.DialogResult = DialogResult.OK;
		base.Controls.Add(okButton);
		base.MaximizeBox = false;
		base.Name = "SaveErrorInformDialog";
		base.MinimizeBox = false;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		ResumeLayout(performLayout: false);
		base.Size = new Size(526, 262);
	}
}
