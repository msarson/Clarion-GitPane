using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class SaveErrorChooseDialog : Form
{
	private Button retryButton;

	private Button ignoreButton;

	private Label descriptionLabel;

	private TextBox descriptionTextBox;

	private Button exceptionButton;

	private Button chooseLocationButton;

	private string displayMessage;

	private Exception exceptionGot;

	public SaveErrorChooseDialog(string fileName, string message, string dialogName, Exception exceptionGot, bool chooseLocationEnabled)
	{
		Text = StringParser.Parse(dialogName);
		InitializeComponents(chooseLocationEnabled);
		RightToLeftConverter.ConvertRecursive(this);
		displayMessage = StringParser.Parse(message, new string[4, 2]
		{
			{ "FileName", fileName },
			{
				"Path",
				Path.GetDirectoryName(fileName)
			},
			{
				"FileNameWithoutPath",
				Path.GetFileName(fileName)
			},
			{
				"Exception",
				exceptionGot.GetType().FullName
			}
		});
		descriptionTextBox.Lines = StringParser.Parse(displayMessage).Split('\n');
		this.exceptionGot = exceptionGot;
	}

	private void ShowException(object sender, EventArgs e)
	{
		MessageService.ShowMessage(exceptionGot.ToString(), StringParser.Parse("${res:ICSharpCode.Core.Services.ErrorDialogs.ExceptionGotDescription}"));
	}

	private void InitializeComponents(bool chooseLocationEnabled)
	{
		base.ClientSize = new Size(508, 320);
		SuspendLayout();
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SaveErrorChooseDialog";
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		descriptionLabel = new Label();
		descriptionLabel.Name = "descriptionLabel";
		descriptionLabel.Location = new Point(8, 8);
		descriptionLabel.Size = new Size(584, 24);
		descriptionLabel.TabIndex = 3;
		descriptionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		descriptionLabel.TextAlign = ContentAlignment.BottomLeft;
		descriptionLabel.Text = StringParser.Parse("${res:ICSharpCode.Core.Services.ErrorDialogs.DescriptionLabel}");
		base.Controls.Add(descriptionLabel);
		descriptionTextBox = new TextBox();
		descriptionTextBox.Multiline = true;
		descriptionTextBox.Size = new Size(584, 237);
		descriptionTextBox.Location = new Point(8, 40);
		descriptionTextBox.TabIndex = 2;
		descriptionTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		descriptionTextBox.ReadOnly = true;
		descriptionTextBox.Name = "descriptionTextBox";
		base.Controls.Add(descriptionTextBox);
		retryButton = new Button();
		retryButton.DialogResult = DialogResult.Retry;
		retryButton.Name = "retryButton";
		retryButton.TabIndex = 5;
		retryButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		retryButton.Text = StringParser.Parse("${res:Global.RetryButtonText}");
		retryButton.Size = new Size(110, 27);
		retryButton.Location = new Point(28, 285);
		base.Controls.Add(retryButton);
		ignoreButton = new Button();
		ignoreButton.Name = "ignoreButton";
		ignoreButton.DialogResult = DialogResult.Ignore;
		ignoreButton.TabIndex = 4;
		ignoreButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		ignoreButton.Text = StringParser.Parse("${res:Global.IgnoreButtonText}");
		ignoreButton.Size = new Size(110, 27);
		ignoreButton.Location = new Point(146, 285);
		base.Controls.Add(ignoreButton);
		exceptionButton = new Button();
		exceptionButton.TabIndex = 1;
		exceptionButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		exceptionButton.Name = "exceptionButton";
		exceptionButton.Text = ResourceService.GetString("ICSharpCode.Core.Services.ErrorDialogs.ShowExceptionButton");
		exceptionButton.Size = new Size(110, 27);
		exceptionButton.Location = new Point(382, 285);
		exceptionButton.Click += ShowException;
		base.Controls.Add(exceptionButton);
		if (chooseLocationEnabled)
		{
			chooseLocationButton = new Button();
			chooseLocationButton.Name = "chooseLocationButton";
			chooseLocationButton.DialogResult = DialogResult.OK;
			chooseLocationButton.TabIndex = 0;
			chooseLocationButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			chooseLocationButton.Text = ResourceService.GetString("Global.ChooseLocationButtonText");
			chooseLocationButton.Size = new Size(110, 27);
			chooseLocationButton.Location = new Point(264, 285);
		}
		base.Controls.Add(chooseLocationButton);
		ResumeLayout(performLayout: false);
		base.Size = new Size(526, 262);
	}
}
