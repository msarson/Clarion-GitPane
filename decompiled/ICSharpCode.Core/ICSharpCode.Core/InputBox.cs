using System;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

internal class InputBox : Form
{
	private Label label;

	private Button cancelButton;

	private TextBox textBox;

	private Button acceptButton;

	private string result;

	public string Result => result;

	public InputBox(string text, string caption, string defaultValue)
	{
		InitializeComponent();
		text = StringParser.Parse(text);
		Text = StringParser.Parse(caption);
		acceptButton.Text = StringParser.Parse("${res:Global.OKButtonText}");
		cancelButton.Text = StringParser.Parse("${res:Global.CancelButtonText}");
		Size size;
		using (Graphics graphics = CreateGraphics())
		{
			Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
			size = graphics.MeasureString(text, label.Font, workingArea.Width - 20).ToSize();
			size.Width += 4;
		}
		if (size.Width < 200)
		{
			size.Width = 200;
		}
		Size clientSize = base.ClientSize;
		clientSize.Width += size.Width - label.Width;
		clientSize.Height += size.Height - label.Height;
		base.ClientSize = clientSize;
		label.Text = text;
		textBox.Text = defaultValue;
		base.DialogResult = DialogResult.Cancel;
		RightToLeftConverter.ConvertRecursive(this);
	}

	private void InitializeComponent()
	{
		this.acceptButton = new System.Windows.Forms.Button();
		this.textBox = new System.Windows.Forms.TextBox();
		this.cancelButton = new System.Windows.Forms.Button();
		this.label = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.acceptButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.acceptButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.acceptButton.Location = new System.Drawing.Point(176, 114);
		this.acceptButton.Name = "acceptButton";
		this.acceptButton.TabIndex = 2;
		this.acceptButton.Text = "OK";
		this.acceptButton.Click += new System.EventHandler(AcceptButtonClick);
		this.textBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.textBox.Location = new System.Drawing.Point(8, 86);
		this.textBox.Name = "textBox";
		this.textBox.Size = new System.Drawing.Size(318, 20);
		this.textBox.TabIndex = 1;
		this.textBox.Text = "";
		this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.cancelButton.Location = new System.Drawing.Point(256, 114);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.TabIndex = 3;
		this.cancelButton.Text = "Cancel";
		this.cancelButton.Click += new System.EventHandler(CancelButtonClick);
		this.label.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.label.Location = new System.Drawing.Point(8, 8);
		this.label.Name = "label";
		this.label.Size = new System.Drawing.Size(328, 74);
		this.label.TabIndex = 0;
		this.label.UseMnemonic = false;
		base.AcceptButton = this.acceptButton;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(338, 144);
		base.Controls.Add(this.textBox);
		base.Controls.Add(this.label);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.acceptButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "InputBox";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "InputBox";
		base.ResumeLayout(false);
	}

	private void CancelButtonClick(object sender, EventArgs e)
	{
		result = null;
		Close();
	}

	private void AcceptButtonClick(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		result = textBox.Text;
		Close();
	}
}
