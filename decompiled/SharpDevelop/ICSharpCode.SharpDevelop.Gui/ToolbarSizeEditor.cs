using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace ICSharpCode.SharpDevelop.Gui;

public class ToolbarSizeEditor : PositionedForm
{
	private int _FrameIconSize_Small;

	private int _FrameHeight_Small;

	private int _DocumentIconSize_Small;

	private int _DocumentHeight_Small;

	private int _PadIconSize_Small;

	private int _PadHeight_Small;

	private int _FrameIconSize_Big;

	private int _FrameHeight_Big;

	private int _DocumentIconSize_Big;

	private int _DocumentHeight_Big;

	private int _PadIconSize_Big;

	private int _PadHeight_Big;

	private IContainer components;

	private Button buttonCancel;

	private Button buttonAccept;

	private TableLayoutPanel tableLayoutPanel1;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	private Label label5;

	private Label label6;

	private Label label7;

	private Label label8;

	private NumericUpDown textBox_FrameIconSize_Small;

	private NumericUpDown textBox_FrameIconSize_Big;

	private NumericUpDown textBox_FrameHeight_Small;

	private NumericUpDown textBox_FrameHeight_Big;

	private NumericUpDown textBox_DocumentIconSize_Small;

	private NumericUpDown textBox_DocumentIconSize_Big;

	private NumericUpDown textBox_DocumentHeight_Small;

	private NumericUpDown textBox_DocumentHeight_Big;

	private NumericUpDown textBox_PadIconSize_Small;

	private NumericUpDown textBox_PadIconSize_Big;

	private NumericUpDown textBox_PadHeight_Small;

	private NumericUpDown textBox_PadHeight_Big;

	private Button buttonRestore;

	public ToolbarSizeEditor()
	{
		InitializeComponent();
		LoadData(useDefault: false);
	}

	public static DialogResult Edit()
	{
		using ToolbarSizeEditor toolbarSizeEditor = new ToolbarSizeEditor();
		return toolbarSizeEditor.ShowDialog(WorkbenchSingleton.MainForm);
	}

	private void buttonAccept_Click(object sender, EventArgs e)
	{
		ToolbarService.SetSize(areSmallSize: true, (int)textBox_DocumentIconSize_Small.Value, (int)textBox_DocumentHeight_Small.Value, (int)textBox_FrameIconSize_Small.Value, (int)textBox_FrameHeight_Small.Value, (int)textBox_PadIconSize_Small.Value, (int)textBox_PadHeight_Small.Value);
		ToolbarService.SetSize(areSmallSize: false, (int)textBox_DocumentIconSize_Big.Value, (int)textBox_DocumentHeight_Big.Value, (int)textBox_FrameIconSize_Big.Value, (int)textBox_FrameHeight_Big.Value, (int)textBox_PadIconSize_Big.Value, (int)textBox_PadHeight_Big.Value);
		Close();
	}

	private void LoadData(bool useDefault)
	{
		ToolbarService.GetSize(areSmallSize: true, useDefault, out _DocumentIconSize_Small, out _DocumentHeight_Small, out _FrameIconSize_Small, out _FrameHeight_Small, out _PadIconSize_Small, out _PadHeight_Small);
		ToolbarService.GetSize(areSmallSize: false, useDefault, out _DocumentIconSize_Big, out _DocumentHeight_Big, out _FrameIconSize_Big, out _FrameHeight_Big, out _PadIconSize_Big, out _PadHeight_Big);
		textBox_FrameIconSize_Small.Value = _FrameIconSize_Small;
		textBox_FrameHeight_Small.Value = _FrameHeight_Small;
		textBox_DocumentIconSize_Small.Value = _DocumentIconSize_Small;
		textBox_DocumentHeight_Small.Value = _DocumentHeight_Small;
		textBox_PadIconSize_Small.Value = _PadIconSize_Small;
		textBox_PadHeight_Small.Value = _PadHeight_Small;
		textBox_FrameIconSize_Big.Value = _FrameIconSize_Big;
		textBox_FrameHeight_Big.Value = _FrameHeight_Big;
		textBox_DocumentIconSize_Big.Value = _DocumentIconSize_Big;
		textBox_DocumentHeight_Big.Value = _DocumentHeight_Big;
		textBox_PadIconSize_Big.Value = _PadIconSize_Big;
		textBox_PadHeight_Big.Value = _PadHeight_Big;
	}

	private void buttonCancel_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void buttonRestore_Click(object sender, EventArgs e)
	{
		LoadData(useDefault: true);
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
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonAccept = new System.Windows.Forms.Button();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.label8 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.textBox_FrameIconSize_Small = new System.Windows.Forms.NumericUpDown();
		this.textBox_FrameIconSize_Big = new System.Windows.Forms.NumericUpDown();
		this.textBox_FrameHeight_Small = new System.Windows.Forms.NumericUpDown();
		this.textBox_FrameHeight_Big = new System.Windows.Forms.NumericUpDown();
		this.textBox_DocumentIconSize_Small = new System.Windows.Forms.NumericUpDown();
		this.textBox_DocumentIconSize_Big = new System.Windows.Forms.NumericUpDown();
		this.textBox_DocumentHeight_Small = new System.Windows.Forms.NumericUpDown();
		this.textBox_DocumentHeight_Big = new System.Windows.Forms.NumericUpDown();
		this.textBox_PadIconSize_Small = new System.Windows.Forms.NumericUpDown();
		this.textBox_PadIconSize_Big = new System.Windows.Forms.NumericUpDown();
		this.textBox_PadHeight_Small = new System.Windows.Forms.NumericUpDown();
		this.textBox_PadHeight_Big = new System.Windows.Forms.NumericUpDown();
		this.buttonRestore = new System.Windows.Forms.Button();
		this.tableLayoutPanel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.textBox_FrameIconSize_Small).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_FrameIconSize_Big).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_FrameHeight_Small).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_FrameHeight_Big).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_DocumentIconSize_Small).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_DocumentIconSize_Big).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_DocumentHeight_Small).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_DocumentHeight_Big).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_PadIconSize_Small).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_PadIconSize_Big).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_PadHeight_Small).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_PadHeight_Big).BeginInit();
		base.SuspendLayout();
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(319, 404);
		this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(104, 46);
		this.buttonCancel.TabIndex = 0;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(buttonCancel_Click);
		this.buttonAccept.Location = new System.Drawing.Point(186, 404);
		this.buttonAccept.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.buttonAccept.Name = "buttonAccept";
		this.buttonAccept.Size = new System.Drawing.Size(113, 46);
		this.buttonAccept.TabIndex = 1;
		this.buttonAccept.Text = "Save";
		this.buttonAccept.UseVisualStyleBackColor = true;
		this.buttonAccept.Click += new System.EventHandler(buttonAccept_Click);
		this.tableLayoutPanel1.ColumnCount = 3;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.Controls.Add(this.label8, 0, 6);
		this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
		this.tableLayoutPanel1.Controls.Add(this.label2, 2, 0);
		this.tableLayoutPanel1.Controls.Add(this.label7, 0, 5);
		this.tableLayoutPanel1.Controls.Add(this.label4, 0, 4);
		this.tableLayoutPanel1.Controls.Add(this.label6, 0, 2);
		this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
		this.tableLayoutPanel1.Controls.Add(this.label5, 0, 1);
		this.tableLayoutPanel1.Controls.Add(this.textBox_FrameIconSize_Small, 1, 1);
		this.tableLayoutPanel1.Controls.Add(this.textBox_FrameIconSize_Big, 2, 1);
		this.tableLayoutPanel1.Controls.Add(this.textBox_FrameHeight_Small, 1, 2);
		this.tableLayoutPanel1.Controls.Add(this.textBox_FrameHeight_Big, 2, 2);
		this.tableLayoutPanel1.Controls.Add(this.textBox_DocumentIconSize_Small, 1, 3);
		this.tableLayoutPanel1.Controls.Add(this.textBox_DocumentIconSize_Big, 2, 3);
		this.tableLayoutPanel1.Controls.Add(this.textBox_DocumentHeight_Small, 1, 4);
		this.tableLayoutPanel1.Controls.Add(this.textBox_DocumentHeight_Big, 2, 4);
		this.tableLayoutPanel1.Controls.Add(this.textBox_PadIconSize_Small, 1, 5);
		this.tableLayoutPanel1.Controls.Add(this.textBox_PadIconSize_Big, 2, 5);
		this.tableLayoutPanel1.Controls.Add(this.textBox_PadHeight_Small, 1, 6);
		this.tableLayoutPanel1.Controls.Add(this.textBox_PadHeight_Big, 2, 6);
		this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 15);
		this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 7;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(408, 356);
		this.tableLayoutPanel1.TabIndex = 2;
		this.label8.AutoSize = true;
		this.label8.Dock = System.Windows.Forms.DockStyle.Top;
		this.label8.Location = new System.Drawing.Point(3, 304);
		this.label8.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(186, 20);
		this.label8.TabIndex = 7;
		this.label8.Text = "Pad Toolbar Height:";
		this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label1.AutoSize = true;
		this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.label1.Location = new System.Drawing.Point(195, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(102, 50);
		this.label1.TabIndex = 0;
		this.label1.Text = "Small Size";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label2.AutoSize = true;
		this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.label2.Location = new System.Drawing.Point(303, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(102, 50);
		this.label2.TabIndex = 1;
		this.label2.Text = "Big Size";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label7.AutoSize = true;
		this.label7.Dock = System.Windows.Forms.DockStyle.Top;
		this.label7.Location = new System.Drawing.Point(3, 254);
		this.label7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(186, 20);
		this.label7.TabIndex = 6;
		this.label7.Text = "Pad Icon Size:";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label4.AutoSize = true;
		this.label4.Dock = System.Windows.Forms.DockStyle.Top;
		this.label4.Location = new System.Drawing.Point(3, 204);
		this.label4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(186, 20);
		this.label4.TabIndex = 3;
		this.label4.Text = "Document Toolbar Height:";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label6.AutoSize = true;
		this.label6.Dock = System.Windows.Forms.DockStyle.Top;
		this.label6.Location = new System.Drawing.Point(3, 104);
		this.label6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(186, 20);
		this.label6.TabIndex = 5;
		this.label6.Text = "Frame Toolbar Height:";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label3.AutoSize = true;
		this.label3.Dock = System.Windows.Forms.DockStyle.Top;
		this.label3.Location = new System.Drawing.Point(3, 154);
		this.label3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(186, 20);
		this.label3.TabIndex = 2;
		this.label3.Text = "Document Icon Size:";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label5.AutoSize = true;
		this.label5.Dock = System.Windows.Forms.DockStyle.Top;
		this.label5.Location = new System.Drawing.Point(3, 54);
		this.label5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 0);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(186, 20);
		this.label5.TabIndex = 4;
		this.label5.Text = "Frame Icon Size:";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.textBox_FrameIconSize_Small.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_FrameIconSize_Small.Location = new System.Drawing.Point(195, 54);
		this.textBox_FrameIconSize_Small.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_FrameIconSize_Small.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_FrameIconSize_Small.Name = "textBox_FrameIconSize_Small";
		this.textBox_FrameIconSize_Small.Size = new System.Drawing.Size(102, 27);
		this.textBox_FrameIconSize_Small.TabIndex = 8;
		this.textBox_FrameIconSize_Small.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_FrameIconSize_Big.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_FrameIconSize_Big.Location = new System.Drawing.Point(303, 54);
		this.textBox_FrameIconSize_Big.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_FrameIconSize_Big.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_FrameIconSize_Big.Name = "textBox_FrameIconSize_Big";
		this.textBox_FrameIconSize_Big.Size = new System.Drawing.Size(102, 27);
		this.textBox_FrameIconSize_Big.TabIndex = 9;
		this.textBox_FrameIconSize_Big.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_FrameHeight_Small.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_FrameHeight_Small.Location = new System.Drawing.Point(195, 104);
		this.textBox_FrameHeight_Small.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_FrameHeight_Small.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_FrameHeight_Small.Name = "textBox_FrameHeight_Small";
		this.textBox_FrameHeight_Small.Size = new System.Drawing.Size(102, 27);
		this.textBox_FrameHeight_Small.TabIndex = 10;
		this.textBox_FrameHeight_Small.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_FrameHeight_Big.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_FrameHeight_Big.Location = new System.Drawing.Point(303, 104);
		this.textBox_FrameHeight_Big.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_FrameHeight_Big.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_FrameHeight_Big.Name = "textBox_FrameHeight_Big";
		this.textBox_FrameHeight_Big.Size = new System.Drawing.Size(102, 27);
		this.textBox_FrameHeight_Big.TabIndex = 11;
		this.textBox_FrameHeight_Big.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_DocumentIconSize_Small.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_DocumentIconSize_Small.Location = new System.Drawing.Point(195, 154);
		this.textBox_DocumentIconSize_Small.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_DocumentIconSize_Small.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_DocumentIconSize_Small.Name = "textBox_DocumentIconSize_Small";
		this.textBox_DocumentIconSize_Small.Size = new System.Drawing.Size(102, 27);
		this.textBox_DocumentIconSize_Small.TabIndex = 12;
		this.textBox_DocumentIconSize_Small.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_DocumentIconSize_Big.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_DocumentIconSize_Big.Location = new System.Drawing.Point(303, 154);
		this.textBox_DocumentIconSize_Big.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_DocumentIconSize_Big.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_DocumentIconSize_Big.Name = "textBox_DocumentIconSize_Big";
		this.textBox_DocumentIconSize_Big.Size = new System.Drawing.Size(102, 27);
		this.textBox_DocumentIconSize_Big.TabIndex = 13;
		this.textBox_DocumentIconSize_Big.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_DocumentHeight_Small.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_DocumentHeight_Small.Location = new System.Drawing.Point(195, 204);
		this.textBox_DocumentHeight_Small.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_DocumentHeight_Small.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_DocumentHeight_Small.Name = "textBox_DocumentHeight_Small";
		this.textBox_DocumentHeight_Small.Size = new System.Drawing.Size(102, 27);
		this.textBox_DocumentHeight_Small.TabIndex = 14;
		this.textBox_DocumentHeight_Small.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_DocumentHeight_Big.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_DocumentHeight_Big.Location = new System.Drawing.Point(303, 204);
		this.textBox_DocumentHeight_Big.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_DocumentHeight_Big.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_DocumentHeight_Big.Name = "textBox_DocumentHeight_Big";
		this.textBox_DocumentHeight_Big.Size = new System.Drawing.Size(102, 27);
		this.textBox_DocumentHeight_Big.TabIndex = 15;
		this.textBox_DocumentHeight_Big.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_PadIconSize_Small.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_PadIconSize_Small.Location = new System.Drawing.Point(195, 254);
		this.textBox_PadIconSize_Small.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_PadIconSize_Small.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_PadIconSize_Small.Name = "textBox_PadIconSize_Small";
		this.textBox_PadIconSize_Small.Size = new System.Drawing.Size(102, 27);
		this.textBox_PadIconSize_Small.TabIndex = 16;
		this.textBox_PadIconSize_Small.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_PadIconSize_Big.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_PadIconSize_Big.Location = new System.Drawing.Point(303, 254);
		this.textBox_PadIconSize_Big.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_PadIconSize_Big.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_PadIconSize_Big.Name = "textBox_PadIconSize_Big";
		this.textBox_PadIconSize_Big.Size = new System.Drawing.Size(102, 27);
		this.textBox_PadIconSize_Big.TabIndex = 17;
		this.textBox_PadIconSize_Big.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_PadHeight_Small.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_PadHeight_Small.Location = new System.Drawing.Point(195, 304);
		this.textBox_PadHeight_Small.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_PadHeight_Small.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_PadHeight_Small.Name = "textBox_PadHeight_Small";
		this.textBox_PadHeight_Small.Size = new System.Drawing.Size(102, 27);
		this.textBox_PadHeight_Small.TabIndex = 18;
		this.textBox_PadHeight_Small.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_PadHeight_Big.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBox_PadHeight_Big.Location = new System.Drawing.Point(303, 304);
		this.textBox_PadHeight_Big.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.textBox_PadHeight_Big.Minimum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.textBox_PadHeight_Big.Name = "textBox_PadHeight_Big";
		this.textBox_PadHeight_Big.Size = new System.Drawing.Size(102, 27);
		this.textBox_PadHeight_Big.TabIndex = 19;
		this.textBox_PadHeight_Big.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.buttonRestore.Location = new System.Drawing.Point(22, 404);
		this.buttonRestore.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.buttonRestore.Name = "buttonRestore";
		this.buttonRestore.Size = new System.Drawing.Size(113, 46);
		this.buttonRestore.TabIndex = 3;
		this.buttonRestore.Text = "Default";
		this.buttonRestore.UseVisualStyleBackColor = true;
		this.buttonRestore.Click += new System.EventHandler(buttonRestore_Click);
		base.AcceptButton = this.buttonAccept;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 20f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.buttonCancel;
		base.ClientSize = new System.Drawing.Size(435, 470);
		base.Controls.Add(this.buttonRestore);
		base.Controls.Add(this.tableLayoutPanel1);
		base.Controls.Add(this.buttonAccept);
		base.Controls.Add(this.buttonCancel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ToolbarSizeEditor";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Toolbar Sizes";
		this.tableLayoutPanel1.ResumeLayout(false);
		this.tableLayoutPanel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.textBox_FrameIconSize_Small).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_FrameIconSize_Big).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_FrameHeight_Small).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_FrameHeight_Big).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_DocumentIconSize_Small).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_DocumentIconSize_Big).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_DocumentHeight_Small).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_DocumentHeight_Big).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_PadIconSize_Small).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_PadIconSize_Big).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_PadHeight_Small).EndInit();
		((System.ComponentModel.ISupportInitialize)this.textBox_PadHeight_Big).EndInit();
		base.ResumeLayout(false);
	}
}
