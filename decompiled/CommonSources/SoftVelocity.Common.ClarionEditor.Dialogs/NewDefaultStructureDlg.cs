using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CommonSources.Properties;

namespace SoftVelocity.Common.ClarionEditor.Dialogs;

public class NewDefaultStructureDlg : Form
{
	private string m_ReturnStructure = string.Empty;

	private PictureBox pictureBox1;

	private Label label1;

	private Label label2;

	private Button m_btnReport;

	private Button m_btnWindow;

	private Button m_btnCancel;

	private Container components;

	public string ReturnStructure => m_ReturnStructure;

	public NewDefaultStructureDlg()
	{
		InitializeComponent();
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
		this.m_btnReport = new System.Windows.Forms.Button();
		this.m_btnWindow = new System.Windows.Forms.Button();
		this.m_btnCancel = new System.Windows.Forms.Button();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.m_btnReport.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_btnReport.Location = new System.Drawing.Point(8, 65);
		this.m_btnReport.Name = "m_btnReport";
		this.m_btnReport.Size = new System.Drawing.Size(88, 23);
		this.m_btnReport.TabIndex = 0;
		this.m_btnReport.Text = "Default &Report";
		this.m_btnReport.Click += new System.EventHandler(m_btnReport_Click);
		this.m_btnWindow.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_btnWindow.Location = new System.Drawing.Point(104, 65);
		this.m_btnWindow.Name = "m_btnWindow";
		this.m_btnWindow.Size = new System.Drawing.Size(96, 23);
		this.m_btnWindow.TabIndex = 1;
		this.m_btnWindow.Text = "Default &Window";
		this.m_btnWindow.Click += new System.EventHandler(m_btnWindow_Click);
		this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_btnCancel.Location = new System.Drawing.Point(208, 65);
		this.m_btnCancel.Name = "m_btnCancel";
		this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
		this.m_btnCancel.TabIndex = 2;
		this.m_btnCancel.Text = "Cancel";
		this.pictureBox1.Image = CommonSources.Properties.Resources.Question;
		this.pictureBox1.Location = new System.Drawing.Point(24, 16);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(37, 40);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
		this.pictureBox1.TabIndex = 3;
		this.pictureBox1.TabStop = false;
		this.label1.Location = new System.Drawing.Point(91, 16);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(192, 16);
		this.label1.TabIndex = 4;
		this.label1.Text = "The templates file hasn't been found.";
		this.label2.Location = new System.Drawing.Point(92, 40);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(192, 16);
		this.label2.TabIndex = 5;
		this.label2.Text = "You can create a default structure.";
		base.AcceptButton = this.m_btnCancel;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		base.CancelButton = this.m_btnCancel;
		base.ClientSize = new System.Drawing.Size(296, 95);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.m_btnCancel);
		base.Controls.Add(this.m_btnWindow);
		base.Controls.Add(this.m_btnReport);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "NewDefaultStructureDlg";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "New Default Structure";
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
	}

	private void m_btnReport_Click(object sender, EventArgs e)
	{
		m_ReturnStructure = "Report REPORT,AT(1000,2000,6250,7688),PAPER(PAPER:A4),PRE(RPT),FONT('Arial',10,,FONT:regular,CHARSET:ANSI),THOUS\r\n\tHEADER,AT(1000,1000,6250,1000),USE(?Header)\r\n\tEND\r\nDetail DETAIL,USE(?Detail)\r\n\tEND\r\n\tFOOTER,AT(1000,9688,6250,1000),USE(?Footer)\r\n\tEND\r\n\tFORM,AT(1000,1000,6250,9688),USE(?Form)\r\n\tEND\r\n\tEND";
	}

	private void m_btnWindow_Click(object sender, EventArgs e)
	{
		m_ReturnStructure = "Window WINDOW('Caption'),AT(,,395,224),FONT('MS Sans Serif',8,,FONT:regular),GRAY\r\n\tBUTTON('&OK'),AT(309,201,35,14),USE(?OkButton),LEFT,DEFAULT\r\n\tBUTTON('&Cancel'),AT(351,201,36,14),USE(?CancelButton),LEFT\r\n\tEND";
	}
}
