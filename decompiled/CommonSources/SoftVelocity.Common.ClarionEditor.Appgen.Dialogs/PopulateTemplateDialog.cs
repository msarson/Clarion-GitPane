using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoftVelocity.Common.ClarionEditor.Appgen.Dialogs;

public class PopulateTemplateDialog : Form
{
	private static string m_initialData = "  BUTTON('&Insert'),AT(,,42,12),USE(?Insert)\r\n  BUTTON('&Change'),AT(42,0,42,12),USE(?Change)\r\n  BUTTON('&Delete'),AT(42,0,42,12),USE(?Delete)";

	private IContainer components;

	private Label label1;

	private TextBox m_txtTemplate;

	private Button m_btnAccept;

	private Button m_btnCancel;

	public static string InitialStructure
	{
		get
		{
			return m_initialData;
		}
		set
		{
			m_initialData = value;
		}
	}

	public PopulateTemplateDialog()
	{
		InitializeComponent();
		m_txtTemplate.Text = InitialStructure;
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
		this.label1 = new System.Windows.Forms.Label();
		this.m_txtTemplate = new System.Windows.Forms.TextBox();
		this.m_btnAccept = new System.Windows.Forms.Button();
		this.m_btnCancel = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(13, 13);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(78, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "&Template Text:";
		this.m_txtTemplate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_txtTemplate.Location = new System.Drawing.Point(8, 30);
		this.m_txtTemplate.Multiline = true;
		this.m_txtTemplate.Name = "m_txtTemplate";
		this.m_txtTemplate.Size = new System.Drawing.Size(286, 97);
		this.m_txtTemplate.TabIndex = 1;
		this.m_btnAccept.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_btnAccept.Location = new System.Drawing.Point(74, 134);
		this.m_btnAccept.Name = "m_btnAccept";
		this.m_btnAccept.Size = new System.Drawing.Size(75, 23);
		this.m_btnAccept.TabIndex = 2;
		this.m_btnAccept.Text = "&Accept";
		this.m_btnAccept.UseVisualStyleBackColor = true;
		this.m_btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnCancel.Location = new System.Drawing.Point(155, 134);
		this.m_btnCancel.Name = "m_btnCancel";
		this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
		this.m_btnCancel.TabIndex = 3;
		this.m_btnCancel.Text = "Cancel";
		this.m_btnCancel.UseVisualStyleBackColor = true;
		base.AcceptButton = this.m_btnAccept;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_btnCancel;
		base.ClientSize = new System.Drawing.Size(302, 167);
		base.Controls.Add(this.m_btnCancel);
		base.Controls.Add(this.m_btnAccept);
		base.Controls.Add(this.m_txtTemplate);
		base.Controls.Add(this.label1);
		base.Name = "PopulateTemplateDialog";
		this.Text = "Populate Control";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
