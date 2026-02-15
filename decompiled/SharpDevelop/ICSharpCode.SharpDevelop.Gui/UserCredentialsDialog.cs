using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class UserCredentialsDialog : Form
{
	private string authenticationType = string.Empty;

	private Label infoLabel;

	private TextBox passwordTextBox;

	private Label userNameLabel;

	private Button cancelButton;

	private Button okButton;

	private Label url;

	private TextBox domainTextBox;

	private TextBox userTextBox;

	private Label domainLabel;

	private Label passwordLabel;

	private Label urlLabel;

	public DiscoveryNetworkCredential Credential => new DiscoveryNetworkCredential(userTextBox.Text, passwordTextBox.Text, domainTextBox.Text, authenticationType);

	public UserCredentialsDialog(string url, string authenticationType)
	{
		InitializeComponent();
		this.url.Text = url;
		this.authenticationType = authenticationType;
		AddStringResources();
	}

	private void InitializeComponent()
	{
		this.urlLabel = new System.Windows.Forms.Label();
		this.userNameLabel = new System.Windows.Forms.Label();
		this.passwordLabel = new System.Windows.Forms.Label();
		this.domainLabel = new System.Windows.Forms.Label();
		this.userTextBox = new System.Windows.Forms.TextBox();
		this.passwordTextBox = new System.Windows.Forms.TextBox();
		this.domainTextBox = new System.Windows.Forms.TextBox();
		this.url = new System.Windows.Forms.Label();
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.infoLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.urlLabel.Location = new System.Drawing.Point(10, 59);
		this.urlLabel.Name = "urlLabel";
		this.urlLabel.Size = new System.Drawing.Size(91, 23);
		this.urlLabel.TabIndex = 0;
		this.urlLabel.Text = "Url:";
		this.urlLabel.UseCompatibleTextRendering = true;
		this.userNameLabel.Location = new System.Drawing.Point(10, 88);
		this.userNameLabel.Name = "userNameLabel";
		this.userNameLabel.Size = new System.Drawing.Size(91, 23);
		this.userNameLabel.TabIndex = 1;
		this.userNameLabel.Text = "&User name:";
		this.userNameLabel.UseCompatibleTextRendering = true;
		this.passwordLabel.Location = new System.Drawing.Point(10, 115);
		this.passwordLabel.Name = "passwordLabel";
		this.passwordLabel.Size = new System.Drawing.Size(91, 23);
		this.passwordLabel.TabIndex = 3;
		this.passwordLabel.Text = "&Password:";
		this.passwordLabel.UseCompatibleTextRendering = true;
		this.domainLabel.Location = new System.Drawing.Point(10, 142);
		this.domainLabel.Name = "domainLabel";
		this.domainLabel.Size = new System.Drawing.Size(91, 23);
		this.domainLabel.TabIndex = 5;
		this.domainLabel.Text = "&Domain:";
		this.domainLabel.UseCompatibleTextRendering = true;
		this.userTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.userTextBox.Location = new System.Drawing.Point(93, 85);
		this.userTextBox.Name = "userTextBox";
		this.userTextBox.Size = new System.Drawing.Size(187, 21);
		this.userTextBox.TabIndex = 2;
		this.passwordTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.passwordTextBox.Location = new System.Drawing.Point(93, 112);
		this.passwordTextBox.Name = "passwordTextBox";
		this.passwordTextBox.PasswordChar = '*';
		this.passwordTextBox.Size = new System.Drawing.Size(187, 21);
		this.passwordTextBox.TabIndex = 4;
		this.domainTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.domainTextBox.Location = new System.Drawing.Point(93, 139);
		this.domainTextBox.Name = "domainTextBox";
		this.domainTextBox.Size = new System.Drawing.Size(187, 21);
		this.domainTextBox.TabIndex = 6;
		this.url.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.url.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.url.Location = new System.Drawing.Point(93, 57);
		this.url.Name = "url";
		this.url.Size = new System.Drawing.Size(187, 21);
		this.url.TabIndex = 9;
		this.url.UseCompatibleTextRendering = true;
		this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Location = new System.Drawing.Point(146, 166);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(64, 26);
		this.okButton.TabIndex = 7;
		this.okButton.Text = "OK";
		this.okButton.UseCompatibleTextRendering = true;
		this.okButton.UseVisualStyleBackColor = true;
		this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(216, 166);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(64, 26);
		this.cancelButton.TabIndex = 8;
		this.cancelButton.Text = "Cancel";
		this.cancelButton.UseCompatibleTextRendering = true;
		this.cancelButton.UseVisualStyleBackColor = true;
		this.infoLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.infoLabel.Location = new System.Drawing.Point(12, 9);
		this.infoLabel.Name = "infoLabel";
		this.infoLabel.Size = new System.Drawing.Size(267, 48);
		this.infoLabel.TabIndex = 10;
		this.infoLabel.Text = "Please supply the credentials to access the specified url.";
		this.infoLabel.UseCompatibleTextRendering = true;
		base.AcceptButton = this.okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(292, 202);
		base.Controls.Add(this.infoLabel);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.url);
		base.Controls.Add(this.domainTextBox);
		base.Controls.Add(this.passwordTextBox);
		base.Controls.Add(this.userTextBox);
		base.Controls.Add(this.domainLabel);
		base.Controls.Add(this.passwordLabel);
		base.Controls.Add(this.userNameLabel);
		base.Controls.Add(this.urlLabel);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(300, 236);
		base.Name = "UserCredentialsDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Discovery Credential";
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private void AddStringResources()
	{
		Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.UserCredentialsDialog.DialogTitle}");
		infoLabel.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.UserCredentialsDialog.InformationLabel}");
		urlLabel.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.UserCredentialsDialog.UrlLabel}");
		userNameLabel.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.UserCredentialsDialog.UserNameLabel}");
		passwordLabel.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.UserCredentialsDialog.PasswordLabel}");
		domainLabel.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.UserCredentialsDialog.DomainLabel}");
		cancelButton.Text = StringParser.Parse("${res:Global.CancelButtonText}");
		okButton.Text = StringParser.Parse("${res:Global.OKButtonText}");
	}
}
