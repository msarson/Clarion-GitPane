using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Generator.UI;

public class NewAppWizardControl : UserControl
{
	private IContainer components;

	private Label _MessageLabel;

	public NewAppWizardControl()
	{
		InitializeComponent();
	}

	public void SetMessage(string message)
	{
		_MessageLabel.Text = message;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this._MessageLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this._MessageLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this._MessageLabel.AutoSize = true;
		this._MessageLabel.Location = new System.Drawing.Point(52, 65);
		this._MessageLabel.Name = "_MessageLabel";
		this._MessageLabel.Size = new System.Drawing.Size(45, 13);
		this._MessageLabel.TabIndex = 0;
		this._MessageLabel.Text = "HELLO!";
		this._MessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Desktop;
		base.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		base.Controls.Add(this._MessageLabel);
		base.Name = "NewAppWizardControl";
		base.Size = new System.Drawing.Size(499, 263);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
